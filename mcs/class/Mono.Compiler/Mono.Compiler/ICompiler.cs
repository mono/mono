
namespace Mono.Compiler
{
	public interface ICompiler
	{
		CompilationResult CompileMethod (IRuntimeInformation runtimeInfo, MethodInfo methodInfo, CompilationFlags flags, out NativeCodeHandle nativeCode);
	}
}
