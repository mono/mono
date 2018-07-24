namespace LLVMSharp
{
    using System;

    public sealed class PassManager : IDisposable
    {
        private readonly LLVMPassManagerRef instance;

        private bool disposed;

        public PassManager()
        {
            this.instance = LLVM.CreatePassManager();
        }

        public PassManager(LLVMModuleRef module)
        {
            this.instance = LLVM.CreateFunctionPassManagerForModule(module);
        }

        public PassManager(LLVMModuleProviderRef moduleProvider)
        {
            this.instance = LLVM.CreateFunctionPassManager(moduleProvider);
        }

        ~PassManager()
        {
            this.Dispose(false);
        }

        public bool RunPassManager(LLVMModuleRef @M)
        {
            return LLVM.RunPassManager(this.instance, @M);
        }

        public bool InitializeFunctionPassManager()
        {
            return LLVM.InitializeFunctionPassManager(this.instance);
        }

        public bool RunFunctionPassManager(LLVMValueRef @F)
        {
            return LLVM.RunFunctionPassManager(this.instance, @F);
        }

        public bool FinalizeFunctionPassManager()
        {
            return LLVM.FinalizeFunctionPassManager(this.instance);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            LLVM.DisposePassManager(this.instance);
            this.disposed = true;
        }

        public void AddArgumentPromotionPass()
        {
            LLVM.AddArgumentPromotionPass(this.instance);
        }

        public void AddConstantMergePass()
        {
            LLVM.AddConstantMergePass(this.instance);
        }

        public void AddDeadArgEliminationPass()
        {
            LLVM.AddDeadArgEliminationPass(this.instance);
        }

        public void AddFunctionAttrsPass()
        {
            LLVM.AddFunctionAttrsPass(this.instance);
        }

        public void AddFunctionInliningPass()
        {
            LLVM.AddFunctionInliningPass(this.instance);
        }

        public void AddAlwaysInlinerPass()
        {
            LLVM.AddAlwaysInlinerPass(this.instance);
        }

        public void AddGlobalDCEPass()
        {
            LLVM.AddGlobalDCEPass(this.instance);
        }

        public void AddGlobalOptimizerPass()
        {
            LLVM.AddGlobalOptimizerPass(this.instance);
        }

        public void AddIPConstantPropagationPass()
        {
            LLVM.AddIPConstantPropagationPass(this.instance);
        }

        public void AddPruneEHPass()
        {
            LLVM.AddPruneEHPass(this.instance);
        }

        public void AddIPSCCPPass()
        {
            LLVM.AddIPSCCPPass(this.instance);
        }

        public void AddInternalizePass(uint @AllButMain)
        {
            LLVM.AddInternalizePass(this.instance, @AllButMain);
        }

        public void AddStripDeadPrototypesPass()
        {
            LLVM.AddStripDeadPrototypesPass(this.instance);
        }

        public void AddStripSymbolsPass()
        {
            LLVM.AddStripSymbolsPass(this.instance);
        }

        public void AddAggressiveDCEPass()
        {
            LLVM.AddAggressiveDCEPass(this.instance);
        }

        public void AddBitTrackingDCEPass()
        {
            LLVM.AddBitTrackingDCEPass(this.instance);
        }

        public void AddAlignmentFromAssumptionsPass()
        {
            LLVM.AddAlignmentFromAssumptionsPass(this.instance);
        }

        public void AddCFGSimplificationPass()
        {
            LLVM.AddCFGSimplificationPass(this.instance);
        }

        public void LLVMAddCalledValuePropagationPass()
        {
            LLVM.AddCalledValuePropagationPass(this.instance);
        }

        public void AddDeadStoreEliminationPass()
        {
            LLVM.AddDeadStoreEliminationPass(this.instance);
        }

        public void AddScalarizerPass()
        {
            LLVM.AddScalarizerPass(this.instance);
        }

        public void AddMergedLoadStoreMotionPass()
        {
            LLVM.AddMergedLoadStoreMotionPass(this.instance);
        }

        public void AddGVNPass()
        {
            LLVM.AddGVNPass(this.instance);
        }

        public void AddNewGVNPass()
        {
            LLVM.AddNewGVNPass(this.instance);
        }

        public void AddIndVarSimplifyPass()
        {
            LLVM.AddIndVarSimplifyPass(this.instance);
        }

        public void AddInstructionCombiningPass()
        {
            LLVM.AddInstructionCombiningPass(this.instance);
        }

        public void AddJumpThreadingPass()
        {
            LLVM.AddJumpThreadingPass(this.instance);
        }

        public void AddLICMPass()
        {
            LLVM.AddLICMPass(this.instance);
        }

        public void AddLoopDeletionPass()
        {
            LLVM.AddLoopDeletionPass(this.instance);
        }

        public void AddLoopIdiomPass()
        {
            LLVM.AddLoopIdiomPass(this.instance);
        }

        public void AddLoopRotatePass()
        {
            LLVM.AddLoopRotatePass(this.instance);
        }

        public void AddLoopRerollPass()
        {
            LLVM.AddLoopRerollPass(this.instance);
        }

        public void AddLoopUnrollPass()
        {
            LLVM.AddLoopUnrollPass(this.instance);
        }

        public void AddLoopUnswitchPass()
        {
            LLVM.AddLoopUnswitchPass(this.instance);
        }

        public void AddMemCpyOptPass()
        {
            LLVM.AddMemCpyOptPass(this.instance);
        }

        public void AddPartiallyInlineLibCallsPass()
        {
            LLVM.AddPartiallyInlineLibCallsPass(this.instance);
        }

        public void AddLowerSwitchPass()
        {
            LLVM.AddLowerSwitchPass(this.instance);
        }

        public void AddPromoteMemoryToRegisterPass()
        {
            LLVM.AddPromoteMemoryToRegisterPass(this.instance);
        }

        public void AddReassociatePass()
        {
            LLVM.AddReassociatePass(this.instance);
        }

        public void AddSCCPPass()
        {
            LLVM.AddSCCPPass(this.instance);
        }

        public void AddScalarReplAggregatesPass()
        {
            LLVM.AddScalarReplAggregatesPass(this.instance);
        }

        public void AddScalarReplAggregatesPassSSA()
        {
            LLVM.AddScalarReplAggregatesPassSSA(this.instance);
        }

        public void AddScalarReplAggregatesPassWithThreshold(int @Threshold)
        {
            LLVM.AddScalarReplAggregatesPassWithThreshold(this.instance, @Threshold);
        }

        public void AddSimplifyLibCallsPass()
        {
            LLVM.AddSimplifyLibCallsPass(this.instance);
        }

        public void AddTailCallEliminationPass()
        {
            LLVM.AddTailCallEliminationPass(this.instance);
        }

        public void AddConstantPropagationPass()
        {
            LLVM.AddConstantPropagationPass(this.instance);
        }

        public void AddDemoteMemoryToRegisterPass()
        {
            LLVM.AddDemoteMemoryToRegisterPass(this.instance);
        }

        public void AddVerifierPass()
        {
            LLVM.AddVerifierPass(this.instance);
        }

        public void AddCorrelatedValuePropagationPass()
        {
            LLVM.AddCorrelatedValuePropagationPass(this.instance);
        }

        public void AddEarlyCSEPass()
        {
            LLVM.AddEarlyCSEPass(this.instance);
        }

        public void AddEarlyCSEMemSSAPass()
        {
            LLVM.AddEarlyCSEMemSSAPass(this.instance);
        }

        public void AddLowerExpectIntrinsicPass()
        {
            LLVM.AddLowerExpectIntrinsicPass(this.instance);
        }

        public void AddTypeBasedAliasAnalysisPass()
        {
            LLVM.AddTypeBasedAliasAnalysisPass(this.instance);
        }

        public void AddScopedNoAliasAAPass()
        {
            LLVM.AddScopedNoAliasAAPass(this.instance);
        }

        public void AddBasicAliasAnalysisPass()
        {
            LLVM.AddBasicAliasAnalysisPass(this.instance);
        }

        public void AddBBVectorizePass()
        {
            LLVM.AddBBVectorizePass(this.instance);
        }

        public void AddLoopVectorizePass()
        {
            LLVM.AddLoopVectorizePass(this.instance);
        }

        public void AddSLPVectorizePass()
        {
            LLVM.AddSLPVectorizePass(this.instance);
        }
    }
}