
using System;

namespace Mono.Compiler.Aot
{
	internal class AotRuntime : ICompiler
	{
		public CompilationResult CompileMethod (ICompilerInformation compilerInfo, MethodInfo methodInfo, CompilationFlags flags, out byte nativeCode, out ulong nativeCodeSize)
		{
			if (!AppDomain.CurrentDomain.IsDefaultAppDomain) {
				// Non shared AOT code can't be used in other appdomains
				return CompilationResult.Skipped;
			}

			
			// Fetch and return nativeCode from pre-compiled .{so,dylib,dll} image corresponding to `methodInfo`'s assembly.
			throw new NotImplementedException ();
		}
	}
}
