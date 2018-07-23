
namespace Mono.Compiler
{
	public interface IRuntimeInformation
	{
		InstalledRuntimeCode InstallCompilationResult (CompilationResult compilationResult, NativeCodeHandle codeHandle);

		object ExecuteInstalledMethod (InstalledRuntimeCode irc, params object[] args);

		ClassInfo GetClassInfoFor (string className);

		MethodInfo GetMethodInfoFor (ClassInfo classInfo, string methodName);
	}
}
