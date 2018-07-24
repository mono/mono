using System;

using Mono.Compiler;
using SimpleJit.Metadata;
using SimpleJit.CIL;

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
			var builder = new Builder ();
			var env = new Env (RuntimeInfo, methodInfo);

			result = NativeCodeHandle.Invalid;
			var r = TranslateBody (env, builder, methodInfo.Body);
			if (r != Ok)
				return r;
			r = builder.Finish (out result);
			return r;
		}

		// translation environment for a single function
		class Env {
			private IRuntimeInformation RuntimeInfo { get; }
			public Env (IRuntimeInformation runtimeInfo, MethodInfo methodInfo)
			{
				this.RuntimeInfo = runtimeInfo;
			}

			// FIXME: get return type from methodInfo signature
			public RuntimeTypeHandle ReturnType { get => RuntimeInfo.VoidType; }
		}

		// encapsulate the LLVM module and builder here.
		class Builder {
			public Builder () { }

			internal CompilationResult Finish (out NativeCodeHandle result) {
				throw NIE ("Builder.Finish");
			}

			public void EmitRetVoid () {
				throw NIE ("Builder.EmitRetVoid");
			}

			// Wrap an LLVM irbuilder here
		}

		CompilationResult TranslateBody (Env env, Builder builder, MethodBody body)
		{
			var iter = body.GetIterator ();
			// TODO: alloca for locals and stack; store in env

			var r = Ok;

			while (iter.MoveNext ()) {
				var opcode = iter.Opcode;
				var opflags = iter.Flags;
				switch (opcode) {
					case Opcode.Ret:
						r = TranslateRet (env, builder);
						break;
					default:
						throw NIE ($"BigStep.Translate {opcode}");
				}
				if (r != Ok)
					break;
			}
			return r;
		}

		CompilationResult TranslateRet (Env env, Builder builder)
		{
			if (env.ReturnType.Equals (RuntimeInfo.VoidType)) {
				builder.EmitRetVoid ();
				return Ok;
			} else
				throw NIE ("TranslateRet non-void");
		}

		private static Exception NIE (string msg)
		{
			return new NotImplementedException (msg);
		}

	}
		
}
