
using System;
using System.Runtime.CompilerServices;

namespace Mono.Compiler
{
	public unsafe class MiniCompiler : ICompiler
	{
		public CompilationResult CompileMethod (IRuntimeInformation runtimeInfo, MethodInfo methodInfo, CompilationFlags flags, out NativeCodeHandle nativeCode)
		{
			if (!CompileMethod(methodInfo.RuntimeMethodHandle, (int) flags, out nativeCode))
				return CompilationResult.InternalError;

			return CompilationResult.Ok;
		}

		[MethodImpl (MethodImplOptions.InternalCall)]
		static extern bool CompileMethod(RuntimeMethodHandle runtimeMethodHandle, int flags, out NativeCodeHandle nativeCode);
	}
}
