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
			// do LLVM stuff
			throw new Exception ("not implemented yet");
		}
	}
}
