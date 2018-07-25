
using System;
using System.Runtime.CompilerServices;

namespace Mono.Compiler
{
	internal unsafe class MiniCompiler : ICompiler
	{
		public CompilationResult CompileMethod (IRuntimeInformation runtimeInfo, MethodInfo methodInfo, CompilationFlags flags, out NativeCodeHandle nativeCode)
		{
			byte *code = CompileMethod(methodInfo.MethodHandle, methodInfo.Flags, out long codeLength);
			if ((IntPtr) code == IntPtr.Zero) {
				nativeCode = default(NativeCodeHandle);
				return CompilationResult.InternalError;
			}

			nativeCode = new NativeCodeHandle(code, codeLength);
			return CompilationResult.Ok;
		}

		[MethodImpl (MethodImplOptions.InternalCall)]
		unsafe static extern byte* CompileMethod(IntPtr method, int flags, out long codeLength);
	}
}
