using System;

namespace Mono.Compiler {
	public class CompilerToRuntime {
		public bool InstallCompilationResult (CompilationResult compilationResult, NativeCodeHandle codeHandle)
		{
			throw new Exception ("not implemented yet");

		}

		public object ExecuteInstalledMethod (NativeCodeHandle codeHandle, params object[] args) {
			throw new Exception ("icall into runtime");
		}

		public ClassInfo GetClassInfoFor (string className) {
			throw new Exception ("not implemented yet");
		}

		public MethodInfo GetMethodInfoFor (ClassInfo classInfo, string methodName) {
			throw new Exception ("not implemented yet");
		}
	}
}
