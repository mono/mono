
namespace Mono.Compiler
{
	public interface ICompiler
	{
		CompilationResult CompileMethod (ICompilerInformation compilerInfo, MethodInfo methodInfo, CompilationFlags flags, out byte nativeCode, out ulong nativeCodeSize);
	}
}
