using System;

namespace Mono.Compiler
{
	public class ManagedJIT : ICompiler
	{
		public CompilationResult CompileMethod (ICompilerInformation compilerInfo, MethodInfo methodInfo, CompilationFlags flags, out NativeCodeHandle nativeCode) {
			// do LLVM stuff
			throw new Exception ("not implemented yet");
		}
	}
}
