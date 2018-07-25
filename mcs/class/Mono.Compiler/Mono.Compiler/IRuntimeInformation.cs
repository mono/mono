using System;

using SimpleJit.Metadata;

namespace Mono.Compiler
{
	public interface IRuntimeInformation
	{
		InstalledRuntimeCode InstallCompilationResult (CompilationResult compilationResult, MethodInfo methodInfo, NativeCodeHandle codeHandle);

		object ExecuteInstalledMethod (InstalledRuntimeCode irc, params object[] args);

		ClassInfo GetClassInfoFor (string className);

		MethodInfo GetMethodInfoFor (ClassInfo classInfo, string methodName);

		ClrType VoidType { get; }
	}
}
