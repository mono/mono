using Mono.Compiler;
using SimpleJit.Metadata;

/// <summary>
///   Compile from CIL to LLVM IR (and then to native code) in one big step
///   (without using our own intermediate representation).
///
///   Basically mimic the Kaleidoscope tutorial.
/// </summary>
namespace Mono.Compiler.BigStep
{
	public class BigStep
	{
		const CompilationResult Ok = CompilationResult.Ok;
		CompilationFlags Flags { get; }
		IRuntimeInformation RuntimeInfo { get; }

		public BigStep (IRuntimeInformation runtimeInfo, CompilationFlags flags)
		{
			this.Flags = flags;
			this.RuntimeInfo = runtimeInfo;
		}

		public CompilationResult CompileMethod (MethodInfo methodInfo, out NativeCodeHandle result)
		{
			var env = new Env ();
			var builder = new Builder ();

			result = NativeCodeHandle.Invalid;
			var r = Translate (env, builder, methodInfo.Body);
			if (r != Ok)
				return r;
			r = builder.Finish (out result);
			return r;
		}

		class Env {
			public Env () { }
		}

		class Builder {
			public Builder () { }

			internal CompilationResult Finish (out NativeCodeHandle result) {
				throw NIE ("Builder.Finish");
			}

			// Wrap an LLVM irbuilder here
		}

		CompilationResult Translate (Env env, Builder builder, MethodBody body)
		{
			throw new System.NotImplementedException ("BigStep.Translate");
		}

	}
		
}
