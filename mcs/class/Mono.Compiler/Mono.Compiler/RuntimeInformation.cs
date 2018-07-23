using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Mono.Compiler {
	public class RuntimeInformation : IRuntimeInformation {
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern static InstalledRuntimeCode mono_install_compilation_result (int compilationResult, NativeCodeHandle codeHandle);

		public InstalledRuntimeCode InstallCompilationResult (CompilationResult compilationResult, NativeCodeHandle codeHandle) {
			return mono_install_compilation_result ((int) compilationResult, codeHandle);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern static int mono_execute_installed_method_2 (InstalledRuntimeCode irc, int arg0, int arg1);

		public object ExecuteInstalledMethod (InstalledRuntimeCode irc, params object[] args) {
			if (args.Length == 2)
				return mono_execute_installed_method_2 (irc, (int) args [0], (int) args [1]);
			throw new Exception ("execute installed method: signature not supported yet");
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
