using System;
using LLVMSharp;

namespace Mono.Compiler
{
	public class ManagedJIT : ICompiler
	{

		public ManagedJIT () {
			// do some LLVMSharp init
			LLVMBuilderRef builder = LLVM.CreateBuilder();
		}

		public CompilationResult CompileMethod (IRuntimeInformation runtimeInfo, MethodInfo methodInfo, CompilationFlags flags, out NativeCodeHandle nativeCode) {
			var bs = new Mono.Compiler.BigStep.BigStep (runtimeInfo, flags);
			return bs.CompileMethod (methodInfo, out nativeCode);
		}
	}
}
