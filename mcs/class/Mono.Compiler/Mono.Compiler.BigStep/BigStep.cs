using System;

using Mono.Compiler;
using SimpleJit.Metadata;
using SimpleJit.CIL;

using LLVMSharp;

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

			Preamble (env, builder);

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
			static readonly LLVMBool Success = new LLVMBool (0);

			LLVMModuleRef module;
			LLVMBuilderRef builder;
			LLVMValueRef function;
			LLVMBasicBlockRef entry;

			public LLVMModuleRef Module { get => module; }
			public LLVMValueRef Function { get => function; }

			public Builder () {
				module = LLVM.ModuleCreateWithName ("BigStepCompile");
				builder = LLVM.CreateBuilder ();
			}

			public void BeginFunction (string name) {
				//FIXME: get types as args
				var funTy = LLVM.FunctionType (LLVM.VoidType (), Array.Empty <LLVMTypeRef> (), false);
				function = LLVM.AddFunction (module, name, funTy);
				entry = LLVM.AppendBasicBlock (function, "entry");
				LLVM.PositionBuilderAtEnd (builder, entry);
			}


			internal CompilationResult Finish (out NativeCodeHandle result) {

				// FIXME: get rid of this printf
				LLVM.DumpModule (Module);

				//FIXME: do this once
				LLVM.LinkInMCJIT ();
				LLVM.InitializeX86TargetMC ();
				LLVM.InitializeX86Target ();
				LLVM.InitializeX86TargetInfo ();
				LLVM.InitializeX86AsmParser ();
				LLVM.InitializeX86AsmPrinter ();
				LLVMMCJITCompilerOptions options = new LLVMMCJITCompilerOptions { NoFramePointerElim = 0 };
				LLVM.InitializeMCJITCompilerOptions(options);
				if (LLVM.CreateMCJITCompilerForModule(out var engine, Module, options, out var error) != Success)
				{
					Console.WriteLine($"Error: {error}");
				}
				IntPtr fnptr = LLVM.GetPointerToGlobal (engine, Function);
				unsafe {
					result = new NativeCodeHandle ((byte*)fnptr, -1);
				}

				//FIXME: cleanup in a Dispose method?

				LLVM.DisposeBuilder (builder);

				// FIXME: can I really dispose of the EE while code is installed in Mono :-(

				// LLVM.DisposeExecutionEngine (engine);

				return Ok;
			}

			public void EmitRetVoid () {
				LLVM.BuildRetVoid (builder);
			}

			// Wrap an LLVM irbuilder here
		}

		void Preamble (Env env, Builder builder)
		{
			// TODO: look at the method sig
			builder.BeginFunction ("todo-name");
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
