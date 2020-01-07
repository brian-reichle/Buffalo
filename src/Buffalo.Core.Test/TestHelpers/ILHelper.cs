// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Buffalo.Core.Test
{
	static class ILHelper
	{
		public static ConstructorInfo GetConstructor(Type objType, params Type[] argTypes)
		{
			var constructor = objType.GetConstructor(argTypes);

			if (constructor == null)
			{
				constructor = objType.GetConstructor(
					BindingFlags.Instance | BindingFlags.NonPublic,
					null,
					argTypes,
					null);
			}

			return constructor;
		}

		public static MethodInfo GetMethod(Type objType, string methodName, params Type[] argTypes)
		{
			return objType.GetMethod(
				methodName,
				BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance,
				null,
				argTypes,
				null);
		}

		public static void LoadType(ILGenerator gen, Type type)
		{
			gen.Emit(OpCodes.Ldtoken, type);
			gen.Emit(OpCodes.Call, GetMethod(typeof(Type), "GetTypeFromHandle", typeof(RuntimeTypeHandle)));
		}

		public static void ThrowNotSupportedException(ILGenerator gen, string message)
		{
			gen.Emit(OpCodes.Ldstr, message);
			gen.Emit(OpCodes.Newobj, typeof(NotSupportedException).GetConstructor(new Type[] { typeof(string) }));
			gen.Emit(OpCodes.Throw);
		}

		public static void ThrowArgumentException(ILGenerator gen, string message, string paramName)
		{
			gen.Emit(OpCodes.Ldstr, message);
			gen.Emit(OpCodes.Ldstr, paramName);
			gen.Emit(OpCodes.Newobj, typeof(ArgumentException).GetConstructor(new Type[] { typeof(string), typeof(string) }));
			gen.Emit(OpCodes.Throw);
		}

		public static MethodBuilder OverrideMethod(TypeBuilder builder, MethodInfo method)
		{
			return builder.DefineMethod(
				method.Name,
				(method.Attributes & MethodAttributes.MemberAccessMask) | MethodAttributes.Virtual,
				method.CallingConvention,
				method.ReturnType,
				Array.ConvertAll(method.GetParameters(), p => p.ParameterType));
		}
	}
}
