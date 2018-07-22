
namespace Mono.Compiler
{
	public interface ICompiler
	{
		CompilationResult CompileMethod (ICompilerInformation compilerInfo, MethodInfo methodInfo, CompilationFlags flags, out NativeCodeHandle nativeCode);
	}
}
