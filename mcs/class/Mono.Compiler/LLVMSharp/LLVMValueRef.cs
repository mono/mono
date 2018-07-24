namespace LLVMSharp
{
    using System;
    using System.Runtime.InteropServices;

    partial struct LLVMValueRef
    {   
        public LLVMValueRef GetNextFunction()
        {
            return LLVM.GetNextFunction(this);
        }

        public LLVMValueRef GetPreviousFunction()
        {
            return LLVM.GetPreviousFunction(this);
        }

        public LLVMTypeRef TypeOf()
        {
            return LLVM.TypeOf(this);
        }

        public string GetValueName()
        {
            return this.ToString();
        }

        public void SetValueName(string @Name)
        {
            LLVM.SetValueName(this, @Name);
        }

        public void Dump()
        {
            LLVM.DumpValue(this);
        }

        public string PrintValueToString()
        {
            IntPtr ptr = LLVM.PrintValueToString(this);
            string retVal = Marshal.PtrToStringAnsi(ptr) ?? string.Empty;
            LLVM.DisposeMessage(ptr);
            return retVal;
        }

        public void ReplaceAllUsesWith(LLVMValueRef @NewVal)
        {
            LLVM.ReplaceAllUsesWith(this, @NewVal);
        }

        public bool IsConstant()
        {
            return LLVM.IsConstant(this);
        }

        public bool IsUndef()
        {
            return LLVM.IsUndef(this);
        }

        public LLVMValueRef IsAArgument()
        {
            return LLVM.IsAArgument(this);
        }

        public LLVMValueRef IsABasicBlock()
        {
            return LLVM.IsABasicBlock(this);
        }

        public LLVMValueRef IsAInlineAsm()
        {
            return LLVM.IsAInlineAsm(this);
        }

        public LLVMValueRef IsAUser()
        {
            return LLVM.IsAUser(this);
        }

        public LLVMValueRef IsAConstant()
        {
            return LLVM.IsAConstant(this);
        }

        public LLVMValueRef IsABlockAddress()
        {
            return LLVM.IsABlockAddress(this);
        }

        public LLVMValueRef IsAConstantAggregateZero()
        {
            return LLVM.IsAConstantAggregateZero(this);
        }

        public LLVMValueRef IsAConstantArray()
        {
            return LLVM.IsAConstantArray(this);
        }

        public LLVMValueRef IsAConstantDataSequential()
        {
            return LLVM.IsAConstantDataSequential(this);
        }

        public LLVMValueRef IsAConstantDataArray()
        {
            return LLVM.IsAConstantDataArray(this);
        }

        public LLVMValueRef IsAConstantDataVector()
        {
            return LLVM.IsAConstantDataVector(this);
        }

        public LLVMValueRef IsAConstantExpr()
        {
            return LLVM.IsAConstantExpr(this);
        }

        public LLVMValueRef IsAConstantFP()
        {
            return LLVM.IsAConstantFP(this);
        }

        public LLVMValueRef IsAConstantInt()
        {
            return LLVM.IsAConstantInt(this);
        }

        public LLVMValueRef IsAConstantPointerNull()
        {
            return LLVM.IsAConstantPointerNull(this);
        }

        public LLVMValueRef IsAConstantStruct()
        {
            return LLVM.IsAConstantStruct(this);
        }

        public LLVMValueRef IsAConstantVector()
        {
            return LLVM.IsAConstantVector(this);
        }

        public LLVMValueRef IsAGlobalValue()
        {
            return LLVM.IsAGlobalValue(this);
        }

        public LLVMValueRef IsAGlobalAlias()
        {
            return LLVM.IsAGlobalAlias(this);
        }

        public LLVMValueRef IsAGlobalObject()
        {
            return LLVM.IsAGlobalObject(this);
        }

        public LLVMValueRef IsAFunction()
        {
            return LLVM.IsAFunction(this);
        }

        public LLVMValueRef IsAGlobalVariable()
        {
            return LLVM.IsAGlobalVariable(this);
        }

        public LLVMValueRef IsAUndefValue()
        {
            return LLVM.IsAUndefValue(this);
        }

        public LLVMValueRef IsAInstruction()
        {
            return LLVM.IsAInstruction(this);
        }

        public LLVMValueRef IsABinaryOperator()
        {
            return LLVM.IsABinaryOperator(this);
        }

        public LLVMValueRef IsACallInst()
        {
            return LLVM.IsACallInst(this);
        }

        public LLVMValueRef IsAIntrinsicInst()
        {
            return LLVM.IsAIntrinsicInst(this);
        }

        public LLVMValueRef IsADbgInfoIntrinsic()
        {
            return LLVM.IsADbgInfoIntrinsic(this);
        }

        public LLVMValueRef IsADbgDeclareInst()
        {
            return LLVM.IsADbgDeclareInst(this);
        }

        public LLVMValueRef IsAMemIntrinsic()
        {
            return LLVM.IsAMemIntrinsic(this);
        }

        public LLVMValueRef IsAMemCpyInst()
        {
            return LLVM.IsAMemCpyInst(this);
        }

        public LLVMValueRef IsAMemMoveInst()
        {
            return LLVM.IsAMemMoveInst(this);
        }

        public LLVMValueRef IsAMemSetInst()
        {
            return LLVM.IsAMemSetInst(this);
        }

        public LLVMValueRef IsACmpInst()
        {
            return LLVM.IsACmpInst(this);
        }

        public LLVMValueRef IsAFCmpInst()
        {
            return LLVM.IsAFCmpInst(this);
        }

        public LLVMValueRef IsAICmpInst()
        {
            return LLVM.IsAICmpInst(this);
        }

        public LLVMValueRef IsAExtractElementInst()
        {
            return LLVM.IsAExtractElementInst(this);
        }

        public LLVMValueRef IsAGetElementPtrInst()
        {
            return LLVM.IsAGetElementPtrInst(this);
        }

        public LLVMValueRef IsAInsertElementInst()
        {
            return LLVM.IsAInsertElementInst(this);
        }

        public LLVMValueRef IsAInsertValueInst()
        {
            return LLVM.IsAInsertValueInst(this);
        }

        public LLVMValueRef IsALandingPadInst()
        {
            return LLVM.IsALandingPadInst(this);
        }

        public LLVMValueRef IsAPHINode()
        {
            return LLVM.IsAPHINode(this);
        }

        public LLVMValueRef IsASelectInst()
        {
            return LLVM.IsASelectInst(this);
        }

        public LLVMValueRef IsAShuffleVectorInst()
        {
            return LLVM.IsAShuffleVectorInst(this);
        }

        public LLVMValueRef IsAStoreInst()
        {
            return LLVM.IsAStoreInst(this);
        }

        public LLVMValueRef IsATerminatorInst()
        {
            return LLVM.IsATerminatorInst(this);
        }

        public LLVMValueRef IsABranchInst()
        {
            return LLVM.IsABranchInst(this);
        }

        public LLVMValueRef IsAIndirectBrInst()
        {
            return LLVM.IsAIndirectBrInst(this);
        }

        public LLVMValueRef IsAInvokeInst()
        {
            return LLVM.IsAInvokeInst(this);
        }

        public LLVMValueRef IsAReturnInst()
        {
            return LLVM.IsAReturnInst(this);
        }

        public LLVMValueRef IsASwitchInst()
        {
            return LLVM.IsASwitchInst(this);
        }

        public LLVMValueRef IsAUnreachableInst()
        {
            return LLVM.IsAUnreachableInst(this);
        }

        public LLVMValueRef IsAResumeInst()
        {
            return LLVM.IsAResumeInst(this);
        }

        public LLVMValueRef IsAUnaryInstruction()
        {
            return LLVM.IsAUnaryInstruction(this);
        }

        public LLVMValueRef IsAAllocaInst()
        {
            return LLVM.IsAAllocaInst(this);
        }

        public LLVMValueRef IsACastInst()
        {
            return LLVM.IsACastInst(this);
        }

        public LLVMValueRef IsAAddrSpaceCastInst()
        {
            return LLVM.IsAAddrSpaceCastInst(this);
        }

        public LLVMValueRef IsABitCastInst()
        {
            return LLVM.IsABitCastInst(this);
        }

        public LLVMValueRef IsAFPExtInst()
        {
            return LLVM.IsAFPExtInst(this);
        }

        public LLVMValueRef IsAFPToSIInst()
        {
            return LLVM.IsAFPToSIInst(this);
        }

        public LLVMValueRef IsAFPToUIInst()
        {
            return LLVM.IsAFPToUIInst(this);
        }

        public LLVMValueRef IsAFPTruncInst()
        {
            return LLVM.IsAFPTruncInst(this);
        }

        public LLVMValueRef IsAIntToPtrInst()
        {
            return LLVM.IsAIntToPtrInst(this);
        }

        public LLVMValueRef IsAPtrToIntInst()
        {
            return LLVM.IsAPtrToIntInst(this);
        }

        public LLVMValueRef IsASExtInst()
        {
            return LLVM.IsASExtInst(this);
        }

        public LLVMValueRef IsASIToFPInst()
        {
            return LLVM.IsASIToFPInst(this);
        }

        public LLVMValueRef IsATruncInst()
        {
            return LLVM.IsATruncInst(this);
        }

        public LLVMValueRef IsAUIToFPInst()
        {
            return LLVM.IsAUIToFPInst(this);
        }

        public LLVMValueRef IsAZExtInst()
        {
            return LLVM.IsAZExtInst(this);
        }

        public LLVMValueRef IsAExtractValueInst()
        {
            return LLVM.IsAExtractValueInst(this);
        }

        public LLVMValueRef IsALoadInst()
        {
            return LLVM.IsALoadInst(this);
        }

        public LLVMValueRef IsAVAArgInst()
        {
            return LLVM.IsAVAArgInst(this);
        }

        public LLVMValueRef IsAMDNode()
        {
            return LLVM.IsAMDNode(this);
        }

        public LLVMValueRef IsAMDString()
        {
            return LLVM.IsAMDString(this);
        }

        public LLVMUseRef GetFirstUse()
        {
            return LLVM.GetFirstUse(this);
        }

        public LLVMValueRef GetOperand(uint @Index)
        {
            return LLVM.GetOperand(this, @Index);
        }

        public LLVMUseRef GetOperandUse(uint @Index)
        {
            return LLVM.GetOperandUse(this, @Index);
        }

        public void SetOperand(uint @Index, LLVMValueRef @Val)
        {
            LLVM.SetOperand(this, @Index, @Val);
        }

        public int GetNumOperands()
        {
            return LLVM.GetNumOperands(this);
        }

        public bool IsNull()
        {
            return LLVM.IsNull(this);
        }

        public ulong ConstIntGetZExtValue()
        {
            return LLVM.ConstIntGetZExtValue(this);
        }

        public long ConstIntGetSExtValue()
        {
            return LLVM.ConstIntGetSExtValue(this);
        }

        public double ConstRealGetDouble(out LLVMBool @losesInfo)
        {
            return LLVM.ConstRealGetDouble(this, out @losesInfo);
        }

        public bool IsConstantString()
        {
            return LLVM.IsConstantString(this);
        }

        public string GetAsString(out size_t @out)
        {
            return LLVM.GetAsString(this, out @out);
        }

        public LLVMValueRef GetElementAsConstant(uint @idx)
        {
            return LLVM.GetElementAsConstant(this, @idx);
        }

        public LLVMOpcode GetConstOpcode()
        {
            return LLVM.GetConstOpcode(this);
        }

        public LLVMValueRef BlockAddress(LLVMBasicBlockRef @BB)
        {
            return LLVM.BlockAddress(this, @BB);
        }

        public LLVMModuleRef GetGlobalParent()
        {
            return LLVM.GetGlobalParent(this);
        }

        public bool IsDeclaration()
        {
            return LLVM.IsDeclaration(this);
        }

        public LLVMLinkage GetLinkage()
        {
            return LLVM.GetLinkage(this);
        }

        public void SetLinkage(LLVMLinkage @Linkage)
        {
            LLVM.SetLinkage(this, @Linkage);
        }

        public string GetSection()
        {
            return LLVM.GetSection(this);
        }

        public void SetSection(string @Section)
        {
            LLVM.SetSection(this, @Section);
        }

        public LLVMVisibility GetVisibility()
        {
            return LLVM.GetVisibility(this);
        }

        public void SetVisibility(LLVMVisibility @Viz)
        {
            LLVM.SetVisibility(this, @Viz);
        }

        public LLVMDLLStorageClass GetDLLStorageClass()
        {
            return LLVM.GetDLLStorageClass(this);
        }

        public void SetDLLStorageClass(LLVMDLLStorageClass @Class)
        {
            LLVM.SetDLLStorageClass(this, @Class);
        }

        public bool HasUnnamedAddr()
        {
            return LLVM.HasUnnamedAddr(this);
        }

        public void SetUnnamedAddr(bool @HasUnnamedAddr)
        {
            LLVM.SetUnnamedAddr(this, @HasUnnamedAddr);
        }

        public uint GetAlignment()
        {
            return LLVM.GetAlignment(this);
        }

        public void SetAlignment(uint @Bytes)
        {
            LLVM.SetAlignment(this, @Bytes);
        }

        public LLVMValueRef GetNextGlobal()
        {
            return LLVM.GetNextGlobal(this);
        }

        public LLVMValueRef GetPreviousGlobal()
        {
            return LLVM.GetPreviousGlobal(this);
        }

        public void DeleteGlobal()
        {
            LLVM.DeleteGlobal(this);
        }

        public LLVMValueRef GetInitializer()
        {
            return LLVM.GetInitializer(this);
        }

        public void SetInitializer(LLVMValueRef @ConstantVal)
        {
            LLVM.SetInitializer(this, @ConstantVal);
        }

        public bool IsThreadLocal()
        {
            return LLVM.IsThreadLocal(this);
        }

        public void SetThreadLocal(bool @IsThreadLocal)
        {
            LLVM.SetThreadLocal(this, @IsThreadLocal);
        }

        public bool IsGlobalConstant()
        {
            return LLVM.IsGlobalConstant(this);
        }

        public void SetGlobalConstant(bool @IsConstant)
        {
            LLVM.SetGlobalConstant(this, @IsConstant);
        }

        public LLVMThreadLocalMode GetThreadLocalMode()
        {
            return LLVM.GetThreadLocalMode(this);
        }

        public void SetThreadLocalMode(LLVMThreadLocalMode @Mode)
        {
            LLVM.SetThreadLocalMode(this, @Mode);
        }

        public bool IsExternallyInitialized()
        {
            return LLVM.IsExternallyInitialized(this);
        }

        public void SetExternallyInitialized(bool @IsExtInit)
        {
            LLVM.SetExternallyInitialized(this, @IsExtInit);
        }

        public void DeleteFunction()
        {
            LLVM.DeleteFunction(this);
        }

        public LLVMValueRef GetPersonalityFn()
        {
            return LLVM.GetPersonalityFn(this);
        }

        public void SetPersonalityFn(LLVMValueRef @PersonalityFn)
        {
            LLVM.SetPersonalityFn(this, @PersonalityFn);
        }

        public uint GetIntrinsicID()
        {
            return LLVM.GetIntrinsicID(this);
        }

        public uint GetFunctionCallConv()
        {
            return LLVM.GetFunctionCallConv(this);
        }

        public void SetFunctionCallConv(uint @CC)
        {
            LLVM.SetFunctionCallConv(this, @CC);
        }

        public string GetGC()
        {
            return LLVM.GetGC(this);
        }

        public void SetGC(string @Name)
        {
            LLVM.SetGC(this, @Name);
        }

        public void AddTargetDependentFunctionAttr(string @A, string @V)
        {
            LLVM.AddTargetDependentFunctionAttr(this, @A, @V);
        }

        public LLVMAttributeRef[] GetAttributesAtIndex(LLVMAttributeIndex @Idx)
        {
            return LLVM.GetAttributesAtIndex(this, Idx);
        }

        public LLVMAttributeRef[] GetCallSiteAttributes(LLVMAttributeIndex @Idx)
        {
            return LLVM.GetCallSiteAttributes(this, Idx);
        }

        public uint CountParams()
        {
            return LLVM.CountParams(this);
        }

        public LLVMValueRef[] GetParams()
        {
            return LLVM.GetParams(this);
        }

        public LLVMValueRef GetParam(uint @Index)
        {
            return LLVM.GetParam(this, @Index);
        }

        public LLVMValueRef GetParamParent()
        {
            return LLVM.GetParamParent(this);
        }

        public LLVMValueRef GetFirstParam()
        {
            return LLVM.GetFirstParam(this);
        }

        public LLVMValueRef GetLastParam()
        {
            return LLVM.GetLastParam(this);
        }

        public LLVMValueRef GetNextParam()
        {
            return LLVM.GetNextParam(this);
        }

        public LLVMValueRef GetPreviousParam()
        {
            return LLVM.GetPreviousParam(this);
        }

        public void SetParamAlignment(uint @align)
        {
            LLVM.SetParamAlignment(this, @align);
        }

        public string GetMDString(out uint @Len)
        {
            return LLVM.GetMDString(this, out @Len);
        }

        public uint GetMDNodeNumOperands()
        {
            return LLVM.GetMDNodeNumOperands(this);
        }

        public LLVMValueRef[] GetMDNodeOperands()
        {
            return LLVM.GetMDNodeOperands(this);
        }

        public bool ValueIsBasicBlock()
        {
            return LLVM.ValueIsBasicBlock(this);
        }

        public LLVMBasicBlockRef ValueAsBasicBlock()
        {
            return LLVM.ValueAsBasicBlock(this);
        }

        public uint CountBasicBlocks()
        {
            return LLVM.CountBasicBlocks(this);
        }

        public LLVMBasicBlockRef[] GetBasicBlocks()
        {
            return LLVM.GetBasicBlocks(this);
        }

        public LLVMBasicBlockRef GetFirstBasicBlock()
        {
            return LLVM.GetFirstBasicBlock(this);
        }

        public LLVMBasicBlockRef GetLastBasicBlock()
        {
            return LLVM.GetLastBasicBlock(this);
        }

        public LLVMBasicBlockRef GetEntryBasicBlock()
        {
            return LLVM.GetEntryBasicBlock(this);
        }

        public LLVMBasicBlockRef AppendBasicBlock(string @Name)
        {
            return LLVM.AppendBasicBlock(this, @Name);
        }

        public int HasMetadata()
        {
            return LLVM.HasMetadata(this);
        }

        public LLVMValueRef GetMetadata(uint @KindID)
        {
            return LLVM.GetMetadata(this, @KindID);
        }

        public void SetMetadata(uint @KindID, LLVMValueRef @Node)
        {
            LLVM.SetMetadata(this, @KindID, @Node);
        }

        public LLVMBasicBlockRef GetInstructionParent()
        {
            return LLVM.GetInstructionParent(this);
        }

        public LLVMValueRef GetNextInstruction()
        {
            return LLVM.GetNextInstruction(this);
        }

        public LLVMValueRef GetPreviousInstruction()
        {
            return LLVM.GetPreviousInstruction(this);
        }

        public void InstructionEraseFromParent()
        {
            LLVM.InstructionEraseFromParent(this);
        }

        public LLVMOpcode GetInstructionOpcode()
        {
            return LLVM.GetInstructionOpcode(this);
        }

        public LLVMIntPredicate GetICmpPredicate()
        {
            return LLVM.GetICmpPredicate(this);
        }

        public LLVMRealPredicate GetFCmpPredicate()
        {
            return LLVM.GetFCmpPredicate(this);
        }

        public LLVMValueRef InstructionClone()
        {
            return LLVM.InstructionClone(this);
        }

        public void SetInstructionCallConv(uint @CC)
        {
            LLVM.SetInstructionCallConv(this, @CC);
        }

        public uint GetInstructionCallConv()
        {
            return LLVM.GetInstructionCallConv(this);
        }

        public void SetInstrParamAlignment(uint @index, uint @align)
        {
            LLVM.SetInstrParamAlignment(this, @index, @align);
        }

        public bool IsTailCall()
        {
            return LLVM.IsTailCall(this);
        }

        public void SetTailCall(bool @IsTailCall)
        {
            LLVM.SetTailCall(this, @IsTailCall);
        }

        public uint GetNumSuccessors()
        {
            return LLVM.GetNumSuccessors(this);
        }

        public LLVMBasicBlockRef GetSuccessor(uint @i)
        {
            return LLVM.GetSuccessor(this, @i);
        }

        public void SetSuccessor(uint @i, LLVMBasicBlockRef @block)
        {
            LLVM.SetSuccessor(this, @i, @block);
        }

        public bool IsConditional()
        {
            return LLVM.IsConditional(this);
        }

        public LLVMValueRef GetCondition()
        {
            return LLVM.GetCondition(this);
        }

        public void SetCondition(LLVMValueRef @Cond)
        {
            LLVM.SetCondition(this, @Cond);
        }

        public LLVMBasicBlockRef GetSwitchDefaultDest()
        {
            return LLVM.GetSwitchDefaultDest(this);
        }

        public void AddIncoming(LLVMValueRef[] @IncomingValues, LLVMBasicBlockRef[] @IncomingBlocks, uint @Count)
        {
            LLVM.AddIncoming(this, @IncomingValues, @IncomingBlocks, @Count);
        }

        public uint CountIncoming()
        {
            return LLVM.CountIncoming(this);
        }

        public LLVMValueRef GetIncomingValue(uint @Index)
        {
            return LLVM.GetIncomingValue(this, @Index);
        }

        public LLVMBasicBlockRef GetIncomingBlock(uint @Index)
        {
            return LLVM.GetIncomingBlock(this, @Index);
        }

        public void AddCase(LLVMValueRef @OnVal, LLVMBasicBlockRef @Dest)
        {
            LLVM.AddCase(this, @OnVal, @Dest);
        }

        public void AddDestination(LLVMBasicBlockRef @Dest)
        {
            LLVM.AddDestination(this, @Dest);
        }

        public void AddClause(LLVMValueRef @ClauseVal)
        {
            LLVM.AddClause(this, @ClauseVal);
        }

        public void SetCleanup(bool @Val)
        {
            LLVM.SetCleanup(this, @Val);
        }

        public bool GetVolatile()
        {
            return LLVM.GetVolatile(this);
        }

        public void SetVolatile(bool @IsVolatile)
        {
            LLVM.SetVolatile(this, @IsVolatile);
        }

        public bool VerifyFunction(LLVMVerifierFailureAction @Action)
        {
            return LLVM.VerifyFunction(this, @Action);
        }

        public void ViewFunctionCFG()
        {
            LLVM.ViewFunctionCFG(this);
        }

        public void ViewFunctionCFGOnly()
        {
            LLVM.ViewFunctionCFGOnly(this);
        }

        public override string ToString()
        {
            return this.PrintValueToString();
        }
    }
}