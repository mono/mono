using System;

namespace Mono.Compiler
{
	public class ManagedJIT : ICompiler
	{

		public ManagedJIT () {
			// do some LLVMSharp init
		}

		public CompilationResult CompileMethod (ICompilerInformation compilerInfo, MethodInfo methodInfo, CompilationFlags flags, out NativeCodeHandle nativeCode) {
			// do LLVM stuff
			throw new Exception ("not implemented yet");
		}
	}
}
