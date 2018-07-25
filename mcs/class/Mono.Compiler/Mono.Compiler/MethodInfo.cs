using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using SimpleJit.Metadata;

namespace Mono.Compiler
{
	public class MethodInfo
	{
		public ClassInfo ClassInfo { get; }
		public string Name { get; }
		public MethodBody Body { get; }

		/* this is a MonoMethod in C */
		internal RuntimeMethodHandle RuntimeMethodHandle { get; }

		internal MethodInfo (ClassInfo ci, string name, MethodBody body, RuntimeMethodHandle runtimeMethodHandle, ClrType returnType, IReadOnlyCollection<ParameterInfo> parameters)
			: this (runtimeMethodHandle)
		{
			ClassInfo = ci;
			Name = name;
			Body = body;
			ReturnType = returnType;
			Parameters = parameters;
		}

		/* Used by MiniCompiler */
		public MethodInfo (RuntimeMethodHandle runtimeMethodHandle)
		{
			RuntimeMethodHandle = runtimeMethodHandle;
		}

		internal ClrType ReturnType { get ; }

		internal IReadOnlyCollection<ParameterInfo> Parameters { get ; }
	}
}
