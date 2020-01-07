// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using Buffalo.Core.Test;
using Buffalo.TestResources;

namespace Buffalo.Core.Lexer.Test
{
	static class LexerFactory
	{
		public static IEnumerable<Token> NewLexer(Type baseType, string expressionString, TokenFilterDelegate filter)
		{
			var scannerType = GetLexerEnumeratorType(baseType);
			return (IEnumerable<Token>)Activator.CreateInstance(scannerType, expressionString, filter);
		}

		static Type GetLexerEnumeratorType(Type baseType)
		{
			Type result;

			lock (_cache)
			{
				if (!_cache.TryGetValue(baseType, out result))
				{
					EnsureSetup();

					var baseTypeImp = baseType.MakeGenericType(typeof(Token));
					result = CreateEnumeratorType(baseTypeImp);

					_cache.Add(baseType, result);
				}
			}

			return result;
		}

		static Type CreateEnumeratorType(Type baseType)
		{
			const TypeAttributes attributes =
				TypeAttributes.Public |
				TypeAttributes.Class |
				TypeAttributes.AutoClass |
				TypeAttributes.AnsiClass |
				TypeAttributes.BeforeFieldInit |
				TypeAttributes.AutoLayout;

			var typeBuilder = _modBuilder.DefineType(
				baseType.FullName + "<Concrete>",
				attributes,
				baseType,
				Type.EmptyTypes);

			FieldInfo filter = typeBuilder.DefineField(
				"_tokenise",
				typeof(TokenFilterDelegate),
				FieldAttributes.Private | FieldAttributes.InitOnly);

			CreateEnumeratorConstructor(typeBuilder, filter, baseType);
			ImplementNewToken(typeBuilder, filter, baseType);
			return typeBuilder.CreateType();
		}

		static void CreateEnumeratorConstructor(TypeBuilder typeBuilder, FieldInfo filter, Type baseType)
		{
			const MethodAttributes attributes =
				MethodAttributes.Public |
				MethodAttributes.HideBySig |
				MethodAttributes.SpecialName |
				MethodAttributes.RTSpecialName;

			var ctor = typeBuilder.DefineConstructor(
				attributes,
				CallingConventions.Standard,
				new Type[] { typeof(string), typeof(TokenFilterDelegate) });

			var gen = ctor.GetILGenerator();
			gen.Emit(OpCodes.Ldarg_0);
			gen.Emit(OpCodes.Ldarg_1);
			gen.Emit(OpCodes.Call, ILHelper.GetConstructor(baseType, typeof(string)));
			gen.Emit(OpCodes.Ldarg_0);
			gen.Emit(OpCodes.Ldarg_2);
			gen.Emit(OpCodes.Stfld, filter);
			gen.Emit(OpCodes.Ret);
		}

		static void ImplementNewToken(TypeBuilder typeBuilder, FieldInfo filter, Type baseType)
		{
			var baseMethod = baseType.GetMethod(
				"NewToken",
				BindingFlags.NonPublic | BindingFlags.Instance);

			var tokenTypeType = baseMethod.GetParameters()[0].ParameterType;

			var getCurrentState = ILHelper.GetMethod(baseType, "get_CurrentState");

			var newTokenCoreMethod = CreateNewTokenCore(typeBuilder, tokenTypeType, baseType);

			var builder = ILHelper.OverrideMethod(typeBuilder, baseMethod);

			var gen = builder.GetILGenerator();
			var state = gen.DeclareLocal(typeof(string));
			LocalBuilder original;

			gen.Emit(OpCodes.Ldarg_0);
			gen.Emit(OpCodes.Ldfld, filter);

			if (getCurrentState != null)
			{
				gen.Emit(OpCodes.Ldarg_0);
				gen.Emit(OpCodes.Call, getCurrentState);
				gen.Emit(OpCodes.Box, getCurrentState.ReturnType);
				gen.Emit(OpCodes.Callvirt, ILHelper.GetMethod(typeof(object), "ToString"));

				original = gen.DeclareLocal(typeof(string));
				gen.Emit(OpCodes.Dup);
				gen.Emit(OpCodes.Stloc, original);
			}
			else
			{
				gen.Emit(OpCodes.Ldnull);
				original = null;
			}

			gen.Emit(OpCodes.Stloc, state);
			gen.Emit(OpCodes.Ldloca, state);

			gen.Emit(OpCodes.Ldarg_0);
			gen.Emit(OpCodes.Ldarg_1);
			gen.Emit(OpCodes.Ldarg_2);
			gen.Emit(OpCodes.Ldarg_3);
			gen.Emit(OpCodes.Ldarg, 4);
			gen.Emit(OpCodes.Call, newTokenCoreMethod);

			gen.Emit(OpCodes.Call, ILHelper.GetMethod(typeof(TokenFilterDelegate), "Invoke", typeof(string).MakeByRefType(), typeof(Token)));

			if (original != null)
			{
				var allDone = gen.DefineLabel();
				var push = gen.DefineLabel();

				gen.Emit(OpCodes.Ldloc, state);
				gen.Emit(OpCodes.Ldloc, original);
				gen.Emit(OpCodes.Beq, allDone);

				gen.Emit(OpCodes.Ldloc, state);
				gen.Emit(OpCodes.Brtrue, push);

				gen.Emit(OpCodes.Ldarg_0);
				gen.Emit(OpCodes.Call, ILHelper.GetMethod(baseType, "PopState"));
				gen.Emit(OpCodes.Br, allDone);

				gen.MarkLabel(push);
				gen.Emit(OpCodes.Ldarg_0);
				ILHelper.LoadType(gen, getCurrentState.ReturnType);
				gen.Emit(OpCodes.Ldloc, state);
				gen.Emit(OpCodes.Call, ILHelper.GetMethod(typeof(Enum), "Parse", typeof(Type), typeof(string)));
				gen.Emit(OpCodes.Unbox_Any, getCurrentState.ReturnType);
				gen.Emit(OpCodes.Call, ILHelper.GetMethod(baseType, "PushState", getCurrentState.ReturnType));

				gen.MarkLabel(allDone);
			}

			gen.Emit(OpCodes.Ret);
		}

		static MethodInfo CreateNewTokenCore(TypeBuilder typeBuilder, Type tokenTypeType, Type baseType)
		{
			var newPos = CreateNewPos(typeBuilder, baseType);

			var builder = typeBuilder.DefineMethod(
				"NewTokenCore",
				MethodAttributes.Private,
				typeof(Token),
				new Type[] { tokenTypeType, typeof(string), typeof(int), typeof(int) });

			var gen = builder.GetILGenerator();
			var skip = gen.DefineLabel();

			gen.Emit(OpCodes.Ldarg_1);
			gen.Emit(OpCodes.Box, tokenTypeType);
			gen.Emit(OpCodes.Callvirt, ILHelper.GetMethod(typeof(object), "ToString"));

			gen.Emit(OpCodes.Ldarg_2);
			gen.Emit(OpCodes.Ldarg_3);
			gen.Emit(OpCodes.Ldarg_S, 4);
			gen.Emit(OpCodes.Call, ILHelper.GetMethod(typeof(string), "Substring", typeof(int), typeof(int)));

			gen.Emit(OpCodes.Ldarg_0);
			gen.Emit(OpCodes.Ldarg_3);
			gen.Emit(OpCodes.Call, newPos);

			gen.Emit(OpCodes.Ldarg_0);
			gen.Emit(OpCodes.Ldarg_3);
			gen.Emit(OpCodes.Ldarg_S, 4);
			gen.Emit(OpCodes.Ldc_I4_1);
			gen.Emit(OpCodes.Sub);
			gen.Emit(OpCodes.Dup);
			gen.Emit(OpCodes.Ldc_I4_0);
			gen.Emit(OpCodes.Bgt, skip);

			gen.Emit(OpCodes.Pop);
			gen.Emit(OpCodes.Ldc_I4_0);

			gen.MarkLabel(skip);
			gen.Emit(OpCodes.Add);
			gen.Emit(OpCodes.Call, newPos);

			gen.Emit(OpCodes.Newobj, ILHelper.GetConstructor(typeof(Token), typeof(string), typeof(string), typeof(CharPosX), typeof(CharPosX)));
			gen.Emit(OpCodes.Ret);

			return builder;
		}

		static MethodInfo CreateNewPos(TypeBuilder typeBuilder, Type baseType)
		{
			var builder = typeBuilder.DefineMethod(
				"NewPos",
				MethodAttributes.Private | MethodAttributes.NewSlot,
				typeof(CharPosX),
				new Type[] { typeof(int) });

			var gen = builder.GetILGenerator();
			var lineNo = gen.DeclareLocal(typeof(int));
			var charNo = gen.DeclareLocal(typeof(int));

			gen.Emit(OpCodes.Ldarg_0);
			gen.Emit(OpCodes.Ldarg_1);
			gen.Emit(OpCodes.Ldloca, lineNo);
			gen.Emit(OpCodes.Ldloca, charNo);
			gen.Emit(OpCodes.Call, ILHelper.GetMethod(baseType, "StartOfLine", typeof(int), typeof(int).MakeByRefType(), typeof(int).MakeByRefType()));
			gen.Emit(OpCodes.Ldarg_1);
			gen.Emit(OpCodes.Ldloc, lineNo);
			gen.Emit(OpCodes.Ldloc, charNo);
			gen.Emit(OpCodes.Newobj, ILHelper.GetConstructor(typeof(CharPosX), typeof(int), typeof(int), typeof(int)));
			gen.Emit(OpCodes.Ret);

			return builder;
		}

		static void EnsureSetup()
		{
			if (_modBuilder == null)
			{
				var name = new AssemblyName(DynamicAssembly.AssemblyName);
				var thisDomain = Thread.GetDomain();

				var asmBuilder = thisDomain.DefineDynamicAssembly(
					name,
					AssemblyBuilderAccess.Run);

				_modBuilder = asmBuilder.DefineDynamicModule(name.Name, false);
			}
		}

		delegate IEnumerator<Token> ConstructEnumeratorDelegate(string expression, TokenFilterDelegate tokenise);

		static readonly Dictionary<Type, Type> _cache = new Dictionary<Type, Type>();
		static ModuleBuilder _modBuilder;
	}
}
