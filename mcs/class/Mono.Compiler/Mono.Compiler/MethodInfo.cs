
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

		/* Used for MiniCompiler */
		internal IntPtr MethodHandle { get; }

		internal MethodInfo (ClassInfo ci, string name, MethodBody body) {
			ClassInfo = ci;
			Name = name;
			Body = body;
		}

		/* Used for MiniCompiler */
		internal MethodInfo (IntPtr runtimeMethodHandle) {
			MethodHandle = runtimeMethodHandle;
		}
	}
}
