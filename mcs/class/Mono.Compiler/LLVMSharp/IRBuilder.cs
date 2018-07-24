namespace LLVMSharp
{
    using System;

    public sealed class IRBuilder : IDisposable
    {
        private readonly LLVMBuilderRef instance;

        private bool disposed;

        public IRBuilder(LLVMContextRef context)
        {
            this.instance = LLVM.CreateBuilderInContext(context);
        }

        public IRBuilder() : this(LLVM.GetGlobalContext())
        {
        }

        ~IRBuilder()
        {
            this.Dispose(false);
        }

        public void PositionBuilder(LLVMBasicBlockRef @Block, LLVMValueRef @Instr)
        {
            LLVM.PositionBuilder(this.instance, @Block, @Instr);
        }

        public void PositionBuilderBefore(LLVMValueRef @Instr)
        {
            LLVM.PositionBuilderBefore(this.instance, @Instr);
        }

        public void PositionBuilderAtEnd(LLVMBasicBlockRef @Block)
        {
            LLVM.PositionBuilderAtEnd(this.instance, @Block);
        }

        public LLVMBasicBlockRef GetInsertBlock()
        {
            return LLVM.GetInsertBlock(this.instance);
        }

        public void ClearInsertionPosition()
        {
            LLVM.ClearInsertionPosition(this.instance);
        }

        public void InsertIntoBuilder(LLVMValueRef @Instr)
        {
            LLVM.InsertIntoBuilder(this.instance, @Instr);
        }

        public void InsertIntoBuilderWithName(LLVMValueRef @Instr, string @Name)
        {
            LLVM.InsertIntoBuilderWithName(this.instance, @Instr, @Name);
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

            LLVM.DisposeBuilder(this.instance);

            this.disposed = true;
        }

        public void SetCurrentDebugLocation(LLVMValueRef @L)
        {
            LLVM.SetCurrentDebugLocation(this.instance, @L);
        }

        public LLVMValueRef GetCurrentDebugLocation()
        {
            return LLVM.GetCurrentDebugLocation(this.instance);
        }

        public void SetInstDebugLocation(LLVMValueRef @Inst)
        {
            LLVM.SetInstDebugLocation(this.instance, @Inst);
        }

        public LLVMValueRef CreateRetVoid()
        {
            return LLVM.BuildRetVoid(this.instance);
        }

        public LLVMValueRef CreateRet(LLVMValueRef @V)
        {
            return LLVM.BuildRet(this.instance, @V);
        }

        public LLVMValueRef CreateAggregateRet(LLVMValueRef[] @RetVals)
        {
            return LLVM.BuildAggregateRet(this.instance, @RetVals);
        }

        public LLVMValueRef CreateBr(LLVMBasicBlockRef @Dest)
        {
            return LLVM.BuildBr(this.instance, @Dest);
        }

        public LLVMValueRef CreateCondBr(LLVMValueRef @If, LLVMBasicBlockRef @Then, LLVMBasicBlockRef @Else)
        {
            return LLVM.BuildCondBr(this.instance, @If, @Then, @Else);
        }

        public LLVMValueRef CreateSwitch(LLVMValueRef @V, LLVMBasicBlockRef @Else, uint @NumCases)
        {
            return LLVM.BuildSwitch(this.instance, @V, @Else, @NumCases);
        }

        public LLVMValueRef CreateIndirectBr(LLVMValueRef @Addr, uint @NumDests)
        {
            return LLVM.BuildIndirectBr(this.instance, @Addr, @NumDests);
        }

        public LLVMValueRef CreateInvoke(LLVMValueRef @Fn, LLVMValueRef[] @Args, LLVMBasicBlockRef @Then, LLVMBasicBlockRef @Catch, string @Name)
        {
            return LLVM.BuildInvoke(this.instance, @Fn, Args, @Then, @Catch, @Name);
        }

        public LLVMValueRef CreateLandingPad(LLVMTypeRef @Ty, LLVMValueRef @PersFn, uint @NumClauses, string @Name)
        {
            return LLVM.BuildLandingPad(this.instance, @Ty, @PersFn, @NumClauses, @Name);
        }

        public LLVMValueRef CreateResume(LLVMValueRef @Exn)
        {
            return LLVM.BuildResume(this.instance, @Exn);
        }

        public LLVMValueRef CreateUnreachable()
        {
            return LLVM.BuildUnreachable(this.instance);
        }

        public LLVMValueRef CreateAdd(LLVMValueRef @LHS, LLVMValueRef @RHS, string @Name)
        {
            return LLVM.BuildAdd(this.instance, @LHS, @RHS, @Name);
        }

        public LLVMValueRef CreateNSWAdd(LLVMValueRef @LHS, LLVMValueRef @RHS, string @Name)
        {
            return LLVM.BuildNSWAdd(this.instance, @LHS, @RHS, @Name);
        }

        public LLVMValueRef CreateNUWAdd(LLVMValueRef @LHS, LLVMValueRef @RHS, string @Name)
        {
            return LLVM.BuildNUWAdd(this.instance, @LHS, @RHS, @Name);
        }

        public LLVMValueRef CreateFAdd(LLVMValueRef @LHS, LLVMValueRef @RHS, string @Name)
        {
            return LLVM.BuildFAdd(this.instance, @LHS, @RHS, @Name);
        }

        public LLVMValueRef CreateSub(LLVMValueRef @LHS, LLVMValueRef @RHS, string @Name)
        {
            return LLVM.BuildSub(this.instance, @LHS, @RHS, @Name);
        }

        public LLVMValueRef CreateNSWSub(LLVMValueRef @LHS, LLVMValueRef @RHS, string @Name)
        {
            return LLVM.BuildNSWSub(this.instance, @LHS, @RHS, @Name);
        }

        public LLVMValueRef CreateNUWSub(LLVMValueRef @LHS, LLVMValueRef @RHS, string @Name)
        {
            return LLVM.BuildNUWSub(this.instance, @LHS, @RHS, @Name);
        }

        public LLVMValueRef CreateFSub(LLVMValueRef @LHS, LLVMValueRef @RHS, string @Name)
        {
            return LLVM.BuildFSub(this.instance, @LHS, @RHS, @Name);
        }

        public LLVMValueRef CreateMul(LLVMValueRef @LHS, LLVMValueRef @RHS, string @Name)
        {
            return LLVM.BuildMul(this.instance, @LHS, @RHS, @Name);
        }

        public LLVMValueRef CreateNSWMul(LLVMValueRef @LHS, LLVMValueRef @RHS, string @Name)
        {
            return LLVM.BuildNSWMul(this.instance, @LHS, @RHS, @Name);
        }

        public LLVMValueRef CreateNUWMul(LLVMValueRef @LHS, LLVMValueRef @RHS, string @Name)
        {
            return LLVM.BuildNUWMul(this.instance, @LHS, @RHS, @Name);
        }

        public LLVMValueRef CreateFMul(LLVMValueRef @LHS, LLVMValueRef @RHS, string @Name)
        {
            return LLVM.BuildFMul(this.instance, @LHS, @RHS, @Name);
        }

        public LLVMValueRef CreateUDiv(LLVMValueRef @LHS, LLVMValueRef @RHS, string @Name)
        {
            return LLVM.BuildUDiv(this.instance, @LHS, @RHS, @Name);
        }

        public LLVMValueRef CreateSDiv(LLVMValueRef @LHS, LLVMValueRef @RHS, string @Name)
        {
            return LLVM.BuildSDiv(this.instance, @LHS, @RHS, @Name);
        }

        public LLVMValueRef CreateExactSDiv(LLVMValueRef @LHS, LLVMValueRef @RHS, string @Name)
        {
            return LLVM.BuildExactSDiv(this.instance, @LHS, @RHS, @Name);
        }

        public LLVMValueRef CreateFDiv(LLVMValueRef @LHS, LLVMValueRef @RHS, string @Name)
        {
            return LLVM.BuildFDiv(this.instance, @LHS, @RHS, @Name);
        }

        public LLVMValueRef CreateURem(LLVMValueRef @LHS, LLVMValueRef @RHS, string @Name)
        {
            return LLVM.BuildURem(this.instance, @LHS, @RHS, @Name);
        }

        public LLVMValueRef CreateSRem(LLVMValueRef @LHS, LLVMValueRef @RHS, string @Name)
        {
            return LLVM.BuildSRem(this.instance, @LHS, @RHS, @Name);
        }

        public LLVMValueRef CreateFRem(LLVMValueRef @LHS, LLVMValueRef @RHS, string @Name)
        {
            return LLVM.BuildFRem(this.instance, @LHS, @RHS, @Name);
        }

        public LLVMValueRef CreateShl(LLVMValueRef @LHS, LLVMValueRef @RHS, string @Name)
        {
            return LLVM.BuildShl(this.instance, @LHS, @RHS, @Name);
        }

        public LLVMValueRef CreateLShr(LLVMValueRef @LHS, LLVMValueRef @RHS, string @Name)
        {
            return LLVM.BuildLShr(this.instance, @LHS, @RHS, @Name);
        }

        public LLVMValueRef CreateAShr(LLVMValueRef @LHS, LLVMValueRef @RHS, string @Name)
        {
            return LLVM.BuildAShr(this.instance, @LHS, @RHS, @Name);
        }

        public LLVMValueRef CreateAnd(LLVMValueRef @LHS, LLVMValueRef @RHS, string @Name)
        {
            return LLVM.BuildAnd(this.instance, @LHS, @RHS, @Name);
        }

        public LLVMValueRef CreateOr(LLVMValueRef @LHS, LLVMValueRef @RHS, string @Name)
        {
            return LLVM.BuildOr(this.instance, @LHS, @RHS, @Name);
        }

        public LLVMValueRef CreateXor(LLVMValueRef @LHS, LLVMValueRef @RHS, string @Name)
        {
            return LLVM.BuildXor(this.instance, @LHS, @RHS, @Name);
        }

        public LLVMValueRef CreateBinOp(LLVMOpcode @Op, LLVMValueRef @LHS, LLVMValueRef @RHS, string @Name)
        {
            return LLVM.BuildBinOp(this.instance, @Op, @LHS, @RHS, @Name);
        }

        public LLVMValueRef CreateNeg(LLVMValueRef @V, string @Name)
        {
            return LLVM.BuildNeg(this.instance, @V, @Name);
        }

        public LLVMValueRef CreateNSWNeg(LLVMValueRef @V, string @Name)
        {
            return LLVM.BuildNSWNeg(this.instance, @V, @Name);
        }

        public LLVMValueRef CreateNUWNeg(LLVMValueRef @V, string @Name)
        {
            return LLVM.BuildNUWNeg(this.instance, @V, @Name);
        }

        public LLVMValueRef CreateFNeg(LLVMValueRef @V, string @Name)
        {
            return LLVM.BuildFNeg(this.instance, @V, @Name);
        }

        public LLVMValueRef CreateNot(LLVMValueRef @V, string @Name)
        {
            return LLVM.BuildNot(this.instance, @V, @Name);
        }

        public LLVMValueRef CreateMalloc(LLVMTypeRef @Ty, string @Name)
        {
            return LLVM.BuildMalloc(this.instance, @Ty, @Name);
        }

        public LLVMValueRef CreateArrayMalloc(LLVMTypeRef @Ty, LLVMValueRef @Val, string @Name)
        {
            return LLVM.BuildArrayMalloc(this.instance, @Ty, @Val, @Name);
        }

        public LLVMValueRef CreateAlloca(LLVMTypeRef @Ty, string @Name)
        {
            return LLVM.BuildAlloca(this.instance, @Ty, @Name);
        }

        public LLVMValueRef CreateArrayAlloca(LLVMTypeRef @Ty, LLVMValueRef @Val, string @Name)
        {
            return LLVM.BuildArrayAlloca(this.instance, @Ty, @Val, @Name);
        }

        public LLVMValueRef CreateFree(LLVMValueRef @PointerVal)
        {
            return LLVM.BuildFree(this.instance, @PointerVal);
        }

        public LLVMValueRef CreateLoad(LLVMValueRef @PointerVal, string @Name)
        {
            return LLVM.BuildLoad(this.instance, @PointerVal, @Name);
        }

        public LLVMValueRef CreateStore(LLVMValueRef @Val, LLVMValueRef @Ptr)
        {
            return LLVM.BuildStore(this.instance, @Val, @Ptr);
        }

        public LLVMValueRef CreateGEP(LLVMValueRef @Pointer, LLVMValueRef[] @Indices, string @Name)
        {
            return LLVM.BuildGEP(this.instance, @Pointer, @Indices, @Name);
        }

        public LLVMValueRef CreateInBoundsGEP(LLVMValueRef @Pointer, LLVMValueRef[] @Indices, string @Name)
        {
            return LLVM.BuildInBoundsGEP(this.instance, @Pointer, @Indices, @Name);
        }

        public LLVMValueRef CreateStructGEP(LLVMValueRef @Pointer, uint @Idx, string @Name)
        {
            return LLVM.BuildStructGEP(this.instance, @Pointer, @Idx, @Name);
        }

        public LLVMValueRef CreateGlobalString(string @Str, string @Name)
        {
            return LLVM.BuildGlobalString(this.instance, @Str, @Name);
        }

        public LLVMValueRef CreateGlobalStringPtr(string @Str, string @Name)
        {
            return LLVM.BuildGlobalStringPtr(this.instance, @Str, @Name);
        }

        public LLVMValueRef CreateTrunc(LLVMValueRef @Val, LLVMTypeRef @DestTy, string @Name)
        {
            return LLVM.BuildTrunc(this.instance, @Val, @DestTy, @Name);
        }

        public LLVMValueRef CreateZExt(LLVMValueRef @Val, LLVMTypeRef @DestTy, string @Name)
        {
            return LLVM.BuildZExt(this.instance, @Val, @DestTy, @Name);
        }

        public LLVMValueRef CreateSExt(LLVMValueRef @Val, LLVMTypeRef @DestTy, string @Name)
        {
            return LLVM.BuildSExt(this.instance, @Val, @DestTy, @Name);
        }

        public LLVMValueRef CreateFPToUI(LLVMValueRef @Val, LLVMTypeRef @DestTy, string @Name)
        {
            return LLVM.BuildFPToUI(this.instance, @Val, @DestTy, @Name);
        }

        public LLVMValueRef CreateFPToSI(LLVMValueRef @Val, LLVMTypeRef @DestTy, string @Name)
        {
            return LLVM.BuildFPToSI(this.instance, @Val, @DestTy, @Name);
        }

        public LLVMValueRef CreateUIToFP(LLVMValueRef @Val, LLVMTypeRef @DestTy, string @Name)
        {
            return LLVM.BuildUIToFP(this.instance, @Val, @DestTy, @Name);
        }

        public LLVMValueRef CreateSIToFP(LLVMValueRef @Val, LLVMTypeRef @DestTy, string @Name)
        {
            return LLVM.BuildSIToFP(this.instance, @Val, @DestTy, @Name);
        }

        public LLVMValueRef CreateFPTrunc(LLVMValueRef @Val, LLVMTypeRef @DestTy, string @Name)
        {
            return LLVM.BuildFPTrunc(this.instance, @Val, @DestTy, @Name);
        }

        public LLVMValueRef CreateFPExt(LLVMValueRef @Val, LLVMTypeRef @DestTy, string @Name)
        {
            return LLVM.BuildFPExt(this.instance, @Val, @DestTy, @Name);
        }

        public LLVMValueRef CreatePtrToInt(LLVMValueRef @Val, LLVMTypeRef @DestTy, string @Name)
        {
            return LLVM.BuildPtrToInt(this.instance, @Val, @DestTy, @Name);
        }

        public LLVMValueRef CreateIntToPtr(LLVMValueRef @Val, LLVMTypeRef @DestTy, string @Name)
        {
            return LLVM.BuildIntToPtr(this.instance, @Val, @DestTy, @Name);
        }

        public LLVMValueRef CreateBitCast(LLVMValueRef @Val, LLVMTypeRef @DestTy, string @Name)
        {
            return LLVM.BuildBitCast(this.instance, @Val, @DestTy, @Name);
        }

        public LLVMValueRef CreateAddrSpaceCast(LLVMValueRef @Val, LLVMTypeRef @DestTy, string @Name)
        {
            return LLVM.BuildAddrSpaceCast(this.instance, @Val, @DestTy, @Name);
        }

        public LLVMValueRef CreateZExtOrBitCast(LLVMValueRef @Val, LLVMTypeRef @DestTy, string @Name)
        {
            return LLVM.BuildZExtOrBitCast(this.instance, @Val, @DestTy, @Name);
        }

        public LLVMValueRef CreateSExtOrBitCast(LLVMValueRef @Val, LLVMTypeRef @DestTy, string @Name)
        {
            return LLVM.BuildSExtOrBitCast(this.instance, @Val, @DestTy, @Name);
        }

        public LLVMValueRef CreateTruncOrBitCast(LLVMValueRef @Val, LLVMTypeRef @DestTy, string @Name)
        {
            return LLVM.BuildTruncOrBitCast(this.instance, @Val, @DestTy, @Name);
        }

        public LLVMValueRef CreateCast(LLVMOpcode @Op, LLVMValueRef @Val, LLVMTypeRef @DestTy, string @Name)
        {
            return LLVM.BuildCast(this.instance, @Op, @Val, @DestTy, @Name);
        }

        public LLVMValueRef CreatePointerCast(LLVMValueRef @Val, LLVMTypeRef @DestTy, string @Name)
        {
            return LLVM.BuildPointerCast(this.instance, @Val, @DestTy, @Name);
        }

        public LLVMValueRef CreateIntCast(LLVMValueRef @Val, LLVMTypeRef @DestTy, string @Name)
        {
            return LLVM.BuildIntCast(this.instance, @Val, @DestTy, @Name);
        }

        public LLVMValueRef CreateFPCast(LLVMValueRef @Val, LLVMTypeRef @DestTy, string @Name)
        {
            return LLVM.BuildFPCast(this.instance, @Val, @DestTy, @Name);
        }

        public LLVMValueRef CreateICmp(LLVMIntPredicate @Op, LLVMValueRef @LHS, LLVMValueRef @RHS, string @Name)
        {
            return LLVM.BuildICmp(this.instance, @Op, @LHS, @RHS, @Name);
        }

        public LLVMValueRef CreateFCmp(LLVMRealPredicate @Op, LLVMValueRef @LHS, LLVMValueRef @RHS, string @Name)
        {
            return LLVM.BuildFCmp(this.instance, @Op, @LHS, @RHS, @Name);
        }

        public LLVMValueRef CreatePhi(LLVMTypeRef @Ty, string @Name)
        {
            return LLVM.BuildPhi(this.instance, @Ty, @Name);
        }

        public LLVMValueRef CreateCall(LLVMValueRef @Fn, LLVMValueRef[] @Args, string @Name)
        {
            return LLVM.BuildCall(this.instance, @Fn, @Args, @Name);
        }

        public LLVMValueRef CreateSelect(LLVMValueRef @If, LLVMValueRef @Then, LLVMValueRef @Else, string @Name)
        {
            return LLVM.BuildSelect(this.instance, @If, @Then, @Else, @Name);
        }

        public LLVMValueRef CreateVAArg(LLVMValueRef @List, LLVMTypeRef @Ty, string @Name)
        {
            return LLVM.BuildVAArg(this.instance, @List, @Ty, @Name);
        }

        public LLVMValueRef CreateExtractElement(LLVMValueRef @VecVal, LLVMValueRef @Index, string @Name)
        {
            return LLVM.BuildExtractElement(this.instance, @VecVal, @Index, @Name);
        }

        public LLVMValueRef CreateInsertElement(LLVMValueRef @VecVal, LLVMValueRef @EltVal, LLVMValueRef @Index, string @Name)
        {
            return LLVM.BuildInsertElement(this.instance, @VecVal, @EltVal, @Index, @Name);
        }

        public LLVMValueRef CreateShuffleVector(LLVMValueRef @V1, LLVMValueRef @V2, LLVMValueRef @Mask, string @Name)
        {
            return LLVM.BuildShuffleVector(this.instance, @V1, @V2, @Mask, @Name);
        }

        public LLVMValueRef CreateExtractValue(LLVMValueRef @AggVal, uint @Index, string @Name)
        {
            return LLVM.BuildExtractValue(this.instance, @AggVal, @Index, @Name);
        }

        public LLVMValueRef CreateInsertValue(LLVMValueRef @AggVal, LLVMValueRef @EltVal, uint @Index, string @Name)
        {
            return LLVM.BuildInsertValue(this.instance, @AggVal, @EltVal, @Index, @Name);
        }

        public LLVMValueRef CreateIsNull(LLVMValueRef @Val, string @Name)
        {
            return LLVM.BuildIsNull(this.instance, @Val, @Name);
        }

        public LLVMValueRef CreateIsNotNull(LLVMValueRef @Val, string @Name)
        {
            return LLVM.BuildIsNotNull(this.instance, @Val, @Name);
        }

        public LLVMValueRef CreatePtrDiff(LLVMValueRef @LHS, LLVMValueRef @RHS, string @Name)
        {
            return LLVM.BuildPtrDiff(this.instance, @LHS, @RHS, @Name);
        }

        public LLVMValueRef CreateFence(LLVMAtomicOrdering @ordering, bool @singleThread, string @Name)
        {
            return LLVM.BuildFence(this.instance, @ordering, @singleThread, @Name);
        }

        public LLVMValueRef CreateAtomicRMW(LLVMAtomicRMWBinOp @op, LLVMValueRef @PTR, LLVMValueRef @Val, LLVMAtomicOrdering @ordering, bool @singleThread)
        {
            return LLVM.BuildAtomicRMW(this.instance, @op, @PTR, @Val, @ordering, @singleThread);
        }
    }
}