// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using Buffalo.Core.Test;
using Buffalo.TestResources;

namespace Buffalo.Core.Parser.Test
{
	static class ParserFactory
	{
		public static IParser NewParser(Type baseType, ReductionDelegate reduction)
		{
			return (IParser)Activator.CreateInstance(GetParserType(baseType), reduction);
		}

		static Type GetParserType(Type baseType)
		{
			Type result;

			lock (_cache)
			{
				if (!_cache.TryGetValue(baseType, out result))
				{
					EnsureSetup();
					result = CreateParserType(baseType);
					_cache.Add(baseType, result);
				}
			}

			return result;
		}

		static Type CreateParserType(Type baseType)
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
				new Type[] { typeof(IParser) });

			var fieldBuilder = typeBuilder.DefineField(
				"_reduction",
				typeof(ReductionDelegate),
				FieldAttributes.Private | FieldAttributes.InitOnly);

			CreateConstructor(typeBuilder, fieldBuilder, baseType);
			ImplementGetTokenType(typeBuilder, baseType);
			ImplementIParser_Parse(typeBuilder, baseType);
			ImplementIParser_SupportsTrace(typeBuilder, baseType);
			ImplementIParser_Trace(typeBuilder, baseType);
			ImplementReductions(typeBuilder, fieldBuilder, baseType);
			return typeBuilder.CreateType();
		}

		static void CreateConstructor(TypeBuilder typeBuilder, FieldInfo reduction, Type baseType)
		{
			const MethodAttributes attributes =
				MethodAttributes.Public |
				MethodAttributes.HideBySig |
				MethodAttributes.SpecialName |
				MethodAttributes.RTSpecialName;

			var ctor = typeBuilder.DefineConstructor(
				attributes,
				CallingConventions.Standard,
				new Type[] { typeof(ReductionDelegate) });

			var gen = ctor.GetILGenerator();
			gen.Emit(OpCodes.Ldarg_0);
			gen.Emit(OpCodes.Call, ILHelper.GetConstructor(baseType));
			gen.Emit(OpCodes.Ldarg_0);
			gen.Emit(OpCodes.Ldarg_1);
			gen.Emit(OpCodes.Stfld, reduction);
			gen.Emit(OpCodes.Ret);
		}

		static void ImplementGetTokenType(TypeBuilder typeBuilder, Type baseType)
		{
			var baseMethod = ILHelper.GetMethod(baseType, "GetTokenType", typeof(Token));

			var builder = ILHelper.OverrideMethod(typeBuilder, baseMethod);

			var gen = builder.GetILGenerator();
			ILHelper.LoadType(gen, baseMethod.ReturnType);
			gen.Emit(OpCodes.Ldarg_1);
			gen.Emit(OpCodes.Call, ILHelper.GetMethod(typeof(Token), "get_TokenTypeName"));
			gen.Emit(OpCodes.Call, ILHelper.GetMethod(typeof(Enum), "Parse", typeof(Type), typeof(string)));
			gen.Emit(OpCodes.Unbox_Any, baseMethod.ReturnType);
			gen.Emit(OpCodes.Ret);
		}

		static void ImplementIParser_Parse(TypeBuilder typeBuilder, Type baseType)
		{
			var builder = typeBuilder.DefineMethod(
				"Parse",
				MethodAttributes.Public | MethodAttributes.Virtual,
				typeof(object),
				new Type[] { typeof(string), typeof(Token[]) });

			var gen = builder.GetILGenerator();

			foreach (var info in GetParseMethods(baseType))
			{
				var next = gen.DefineLabel();
				var code = GetParseCode(info);

				if (code == null)
				{
					gen.Emit(OpCodes.Ldarg_1);
				}
				else
				{
					gen.Emit(OpCodes.Call, ILHelper.GetMethod(typeof(StringComparer), "get_Ordinal"));
					gen.Emit(OpCodes.Ldarg_1);
					gen.Emit(OpCodes.Ldstr, code);
					gen.Emit(OpCodes.Callvirt, ILHelper.GetMethod(typeof(StringComparer), "Compare", typeof(string), typeof(string)));
				}

				gen.Emit(OpCodes.Brtrue, next);

				gen.Emit(OpCodes.Ldarg_0);
				gen.Emit(OpCodes.Ldarg_2);

				if (info.ReturnType.IsValueType)
				{
					gen.Emit(OpCodes.Callvirt, info);
					gen.Emit(OpCodes.Box, info.ReturnType);
				}
				else
				{
					gen.Emit(OpCodes.Tailcall);
					gen.Emit(OpCodes.Callvirt, info);
				}

				gen.Emit(OpCodes.Ret);

				gen.MarkLabel(next);
			}

			ILHelper.ThrowArgumentException(gen, "unrecognised entry", "entry");
		}

		static void ImplementIParser_SupportsTrace(TypeBuilder typeBuilder, Type baseType)
		{
			var baseMethod = ILHelper.GetMethod(baseType, "get_Trace");

			var impMethod = typeBuilder.DefineMethod(
				"IParser.SupportsTrace",
				MethodAttributes.Private | MethodAttributes.NewSlot | MethodAttributes.Virtual,
				typeof(bool),
				Type.EmptyTypes);

			var gen = impMethod.GetILGenerator();
			gen.Emit(baseMethod == null ? OpCodes.Ldc_I4_0 : OpCodes.Ldc_I4_1);
			gen.Emit(OpCodes.Ret);

			typeBuilder.DefineMethodOverride(impMethod, ILHelper.GetMethod(typeof(IParser), "get_SupportsTrace"));
		}

		static void ImplementIParser_Trace(TypeBuilder typeBuilder, Type baseType)
		{
			var baseMethod = ILHelper.GetMethod(baseType, "get_Trace");

			var impMethod = typeBuilder.DefineMethod(
				"IParser.Trace",
				MethodAttributes.Private | MethodAttributes.NewSlot | MethodAttributes.Virtual,
				typeof(string),
				Type.EmptyTypes);

			var gen = impMethod.GetILGenerator();

			if (baseMethod == null)
			{
				ILHelper.ThrowNotSupportedException(gen, "This parser does not support the Trace property.");
			}
			else
			{
				gen.Emit(OpCodes.Jmp, baseMethod);
			}

			typeBuilder.DefineMethodOverride(impMethod, ILHelper.GetMethod(typeof(IParser), "get_Trace"));
		}

		static void ImplementReductions(TypeBuilder typeBuilder, FieldInfo reduction, Type baseType)
		{
			foreach (var info in baseType.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic))
			{
				if (info.IsAbstract && info.IsFamily && info.Name.StartsWith("Reduce_", StringComparison.InvariantCulture))
				{
					ImplementReduction(typeBuilder, reduction, info);
				}
			}
		}

		static void ImplementReduction(TypeBuilder typeBuilder, FieldInfo reduction, MethodInfo reductionMethod)
		{
			var paramInfos = reductionMethod.GetParameters();

			var builder = ILHelper.OverrideMethod(typeBuilder, reductionMethod);
			var gen = builder.GetILGenerator();

			gen.Emit(OpCodes.Ldarg_0);
			gen.Emit(OpCodes.Ldfld, reduction);
			gen.Emit(OpCodes.Ldstr, reductionMethod.Name);
			gen.Emit(OpCodes.Ldc_I4, paramInfos.Length);
			gen.Emit(OpCodes.Newarr, typeof(object));

			for (var i = 0; i < paramInfos.Length; i++)
			{
				var paramType = paramInfos[i].ParameterType;

				gen.Emit(OpCodes.Dup);
				gen.Emit(OpCodes.Ldc_I4, i);
				gen.Emit(OpCodes.Ldarg, i + 1);

				if (paramType.IsValueType)
				{
					gen.Emit(OpCodes.Box, paramType);
				}

				gen.Emit(OpCodes.Stelem, typeof(object));
			}

			gen.Emit(OpCodes.Callvirt, ILHelper.GetMethod(typeof(ReductionDelegate), "Invoke", typeof(string), typeof(object[])));

			if (reductionMethod.ReturnType.IsValueType)
			{
				gen.Emit(OpCodes.Unbox_Any, reductionMethod.ReturnType);
			}
			else if (reductionMethod.ReturnType != typeof(object))
			{
				gen.Emit(OpCodes.Castclass, reductionMethod.ReturnType);
			}

			gen.Emit(OpCodes.Ret);
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

		static IList<MethodInfo> GetParseMethods(Type baseType)
		{
			var results = new List<MethodInfo>();

			foreach (var method in baseType.GetMethods())
			{
				if (method.Name == "Parse" || method.Name.StartsWith("Parse_", StringComparison.InvariantCulture))
				{
					results.Add(method);
				}
			}

			return results;
		}

		static string GetParseCode(MethodInfo info)
		{
			var name = info.Name;

			return name.Length < 7 ? null : name.Substring(6);
		}

		static readonly Dictionary<Type, Type> _cache = new Dictionary<Type, Type>();
		static ModuleBuilder _modBuilder;
	}
}
