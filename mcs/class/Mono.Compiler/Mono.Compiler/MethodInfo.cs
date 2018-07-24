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
		/* TODO: unify with MethodHandle. This is a MonoReflectionMethod in C */
		public RuntimeMethodHandle RuntimeMethodHandle { get; }

		internal MethodInfo (ClassInfo ci, string name, MethodBody body, RuntimeMethodHandle runtimeMethodHandle) {
			ClassInfo = ci;
			Name = name;
			Body = body;
			RuntimeMethodHandle = runtimeMethodHandle;
		}

		/* Used only for MiniCompiler. This should be merged with the above constructor. */
		/* this is a MonoMethod in C */
		internal IntPtr MethodHandle { get; }

		internal MethodInfo (IntPtr runtimeMethodHandle) {
			MethodHandle = runtimeMethodHandle;
		}
	}
}
