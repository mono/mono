using System;
using System.Reflection;

namespace Mono.Compiler {
	public class RuntimeInformation : IRuntimeInformation {
		public InstalledRuntimeCode InstallCompilationResult (CompilationResult compilationResult, NativeCodeHandle codeHandle) {
			throw new Exception ("not implemented yet");

		}

		public object ExecuteInstalledMethod (InstalledRuntimeCode irc, params object[] args) {
			throw new Exception ("icall into runtime");
		}

		public ClassInfo GetClassInfoFor (string className)
		{
			var t = Type.GetType (className, true); /* FIXME: get assembly first, then type */
			return ClassInfo.FromType (t);
		}

		public MethodInfo GetMethodInfoFor (ClassInfo classInfo, string methodName) {
			/* FIXME: methodName doesn't uniquely determine a method */
			return classInfo.GetMethodInfoFor (methodName);
		}
	}
}
