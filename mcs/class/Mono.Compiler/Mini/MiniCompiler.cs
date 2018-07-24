
using System;
using System.Runtime.CompilerServices;

namespace Mono.Compiler
{
	internal unsafe class MiniCompiler : ICompiler
	{
		public CompilationResult CompileMethod (IRuntimeInformation runtimeInfo, MethodInfo methodInfo, CompilationFlags flags, out NativeCodeHandle nativeCode)
		{
			byte *code = CompileMethod(methodInfo.MethodHandle, out long codeLength);
			if ((IntPtr) code == IntPtr.Zero) {
				nativeCode = default(NativeCodeHandle);
				return CompilationResult.InternalError;
			}

			nativeCode = new NativeCodeHandle(code, codeLength, methodInfo);
			return CompilationResult.Ok;
		}

		[MethodImpl (MethodImplOptions.InternalCall)]
		unsafe static extern byte* CompileMethod(IntPtr method, out long codeLength);
	}
}
