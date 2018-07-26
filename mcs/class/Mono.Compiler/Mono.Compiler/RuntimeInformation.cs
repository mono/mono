﻿using System;
using System.Reflection;
using System.Runtime.CompilerServices;

using SimpleJit.Metadata;

namespace Mono.Compiler {
	public class RuntimeInformation : IRuntimeInformation {
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern static InstalledRuntimeCode mono_install_compilation_result (int compilationResult, RuntimeMethodHandle handle, NativeCodeHandle codeHandle);

		public InstalledRuntimeCode InstallCompilationResult (CompilationResult compilationResult, MethodInfo methodInfo, NativeCodeHandle codeHandle) {
			return mono_install_compilation_result ((int) compilationResult, methodInfo.RuntimeMethodHandle, codeHandle);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern static object mono_execute_installed_method (InstalledRuntimeCode irc, params object[] args);

		public object ExecuteInstalledMethod (InstalledRuntimeCode irc, params object[] args) {
			return mono_execute_installed_method (irc, args);
		}

		public ClassInfo GetClassInfoFor (string className)
		{
			var t = Type.GetType (className, true); /* FIXME: get assembly first, then type */
			return ClassInfo.FromType (t);
		}

		public MethodInfo GetMethodInfoFor (ClassInfo classInfo, string methodName) {
			/* FIXME: methodName doesn't uniquely determine a method */
			return classInfo.GetMethodInfoFor (methodName);
		}

		/* Primitive types */
		public ClrType VoidType { get => ClrTypeFromType (typeof (void)); }
		
		public static ClrType VoidTypeInstance { get => ClrTypeFromType (typeof (void)); }

		public static ClrType BoolType { get => ClrTypeFromType (typeof (bool)); }

		public static ClrType CharType { get => ClrTypeFromType (typeof (char)); }

		public static ClrType ObjectType { get => ClrTypeFromType (typeof (object)); }

		public static ClrType StringType { get => ClrTypeFromType (typeof (string)); }

		public static ClrType Int8Type { get => ClrTypeFromType (typeof (System.SByte)); }
		public static ClrType UInt8Type { get => ClrTypeFromType (typeof (System.Byte)); }

		public static ClrType Int16Type { get => ClrTypeFromType (typeof (System.Int16)); }
		public static ClrType UInt16Type { get => ClrTypeFromType (typeof (System.UInt16)); }

		public static ClrType Int32Type { get => ClrTypeFromType (typeof (System.Int32)); }
		public static ClrType UInt32Type { get => ClrTypeFromType (typeof (System.UInt32)); }

		public static ClrType Int64Type { get => ClrTypeFromType (typeof (System.Int16)); }
		public static ClrType UInt64Type { get => ClrTypeFromType (typeof (System.UInt16)); }

		public static ClrType NativeIntType { get => ClrTypeFromType (typeof (System.IntPtr)); }
		public static ClrType NativeUnsignedIntType { get => ClrTypeFromType (typeof (System.UIntPtr)); }

		public static ClrType Float32Type { get => ClrTypeFromType (typeof (System.Single)); }
		public static ClrType Float64Type { get => ClrTypeFromType (typeof (System.Double)); }

		public static ClrType TypedRefType { get => ClrTypeFromType (typeof (System.TypedReference)); }


		internal static ClrType ClrTypeFromType (Type t)
		{
			return new ClrType (t.TypeHandle);
		}
	       
	}
}
