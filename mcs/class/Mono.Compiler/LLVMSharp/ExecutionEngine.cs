namespace LLVMSharp
{
    using System;
    using System.Runtime.CompilerServices;

    public sealed class ExecutionEngine : IDisposable
    {
        private readonly LLVMExecutionEngineRef instance;
        
        private bool disposed;

        public static ExecutionEngine Create(Module module)
        {
            if (!LLVM.CreateExecutionEngineForModule(out var instance, module.instance, out string error))
            {
                ThrowError(error);
            }

            return new ExecutionEngine(instance);
        }

        public static ExecutionEngine CreateInterpreter(Module module)
        {
            if (LLVM.CreateInterpreterForModule(out var instance, module.instance, out string error))
            {
                ThrowError(error);
            }

            return new ExecutionEngine(instance);
        }

        public static ExecutionEngine CreateMCJITCompiler(Module module, LLVMMCJITCompilerOptions options)
        {
            LLVM.InitializeMCJITCompilerOptions(options);
            if (LLVM.CreateMCJITCompilerForModule(out var instance, module.instance, options, out var error))
            {
                ThrowError(error);
            }

            return new ExecutionEngine(instance);
        }

        internal ExecutionEngine(LLVMExecutionEngineRef ee)
        {
            this.instance = ee;
        }

        ~ExecutionEngine()
        {
            this.Dispose(false);
        }

        public void RunStaticConstructors()
        {
            LLVM.RunStaticConstructors(this.instance);
        }

        public void RunStaticDestructors()
        {
            LLVM.RunStaticDestructors(this.instance);
        }

        public int RunFunctionAsMain(LLVMValueRef @F, uint @ArgC, string[] @ArgV, string[] @EnvP)
        {
            return LLVM.RunFunctionAsMain(this.instance, @F, @ArgC, @ArgV, @EnvP);
        }

        public LLVMGenericValueRef RunFunction(LLVMValueRef @F, LLVMGenericValueRef[] @Args)
        {
            return LLVM.RunFunction(this.instance, @F, @Args);
        }

        public void FreeMachineCodeForFunction(LLVMValueRef @F)
        {
            LLVM.FreeMachineCodeForFunction(this.instance, @F);
        }

        public void AddModule(LLVMModuleRef @M)
        {
            LLVM.AddModule(this.instance, @M);
        }

        public bool RemoveModule(LLVMModuleRef @M, out LLVMModuleRef @OutMod, out IntPtr @OutError)
        {
            return LLVM.RemoveModule(this.instance, @M, out @OutMod, out @OutError);
        }

        public bool FindFunction(string @Name, out LLVMValueRef @OutFn)
        {
            return LLVM.FindFunction(this.instance, @Name, out @OutFn);
        }

        public IntPtr RecompileAndRelinkFunction(LLVMValueRef @Fn)
        {
            return LLVM.RecompileAndRelinkFunction(this.instance, @Fn);
        }

        public LLVMTargetDataRef GetExecutionEngineTargetData()
        {
            return LLVM.GetExecutionEngineTargetData(this.instance);
        }

        public LLVMTargetMachineRef GetExecutionEngineTargetMachine()
        {
            return LLVM.GetExecutionEngineTargetMachine(this.instance);
        }

        public void AddGlobalMapping(LLVMValueRef @Global, IntPtr @Addr)
        {
            LLVM.AddGlobalMapping(this.instance, @Global, @Addr);
        }

        public IntPtr GetPointerToGlobal(LLVMValueRef @Global)
        {
            return LLVM.GetPointerToGlobal(this.instance, @Global);
        }

        public ulong GetGlobalValueAddress(string @Name)
        {
            return LLVM.GetGlobalValueAddress(this.instance, @Name);
        }

        public ulong GetFunctionAddress(string @Name)
        {
            return LLVM.GetFunctionAddress(this.instance, @Name);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            LLVM.DisposeExecutionEngine(this.instance);

            this.disposed = true;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowError(string error)
        {
            throw new Exception(error);
        }
    }
}