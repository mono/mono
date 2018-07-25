using SimpleJit.Metadata;
using System;
using System.Reflection.Emit;

namespace Mono.Compiler
{
	public class MethodInfo
	{
		public ClassInfo ClassInfo { get; }
		public string Name { get; }
		public MethodBody Body { get; }

		/* this is a MonoMethod in C */
		internal RuntimeMethodHandle RuntimeMethodHandle { get; }

		public MethodInfo (ClassInfo ci, string name, MethodBody body, RuntimeMethodHandle runtimeMethodHandle)
			: this (runtimeMethodHandle)
		{
			ClassInfo = ci;
			Name = name;
			Body = body;
		}

		/* Used by MiniCompiler */
		public MethodInfo (RuntimeMethodHandle runtimeMethodHandle)
		{
			RuntimeMethodHandle = runtimeMethodHandle;
		}
	}
}
