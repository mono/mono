namespace LLVMSharp
{
    partial struct LLVMValueRef
    {
        public static LLVMValueRef ConstVector(LLVMValueRef[] @ScalarConstantVars)
        {
            return LLVM.ConstVector(@ScalarConstantVars);
        }

        public static LLVMValueRef ConstStruct(LLVMValueRef[] @ConstantVals, bool @Packed)
        {
            return LLVM.ConstStruct(@ConstantVals, @Packed);
        }

        public static LLVMValueRef MDNode(LLVMValueRef[] Vals)
        {
            return LLVM.MDNode(Vals);
        }

        public static LLVMValueRef ConstNeg(LLVMValueRef @ConstantVal)
        {
            return LLVM.ConstNeg(@ConstantVal);
        }

        public static LLVMValueRef ConstNSWNeg(LLVMValueRef @ConstantVal)
        {
            return LLVM.ConstNSWNeg(@ConstantVal);
        }

        public static LLVMValueRef ConstNUWNeg(LLVMValueRef @ConstantVal)
        {
            return LLVM.ConstNUWNeg(@ConstantVal);
        }

        public static LLVMValueRef ConstFNeg(LLVMValueRef @ConstantVal)
        {
            return LLVM.ConstFNeg(@ConstantVal);
        }

        public static LLVMValueRef ConstNot(LLVMValueRef @ConstantVal)
        {
            return LLVM.ConstNot(@ConstantVal);
        }

        public static LLVMValueRef ConstAdd(LLVMValueRef @LHSConstant, LLVMValueRef @RHSConstant)
        {
            return LLVM.ConstAdd(@LHSConstant, @RHSConstant);
        }

        public static LLVMValueRef ConstNSWAdd(LLVMValueRef @LHSConstant, LLVMValueRef @RHSConstant)
        {
            return LLVM.ConstNSWAdd(@LHSConstant, @RHSConstant);
        }

        public static LLVMValueRef ConstNUWAdd(LLVMValueRef @LHSConstant, LLVMValueRef @RHSConstant)
        {
            return LLVM.ConstNUWAdd(@LHSConstant, @RHSConstant);
        }

        public static LLVMValueRef ConstFAdd(LLVMValueRef @LHSConstant, LLVMValueRef @RHSConstant)
        {
            return LLVM.ConstFAdd(@LHSConstant, @RHSConstant);
        }

        public static LLVMValueRef ConstSub(LLVMValueRef @LHSConstant, LLVMValueRef @RHSConstant)
        {
            return LLVM.ConstSub(@LHSConstant, @RHSConstant);
        }

        public static LLVMValueRef ConstNSWSub(LLVMValueRef @LHSConstant, LLVMValueRef @RHSConstant)
        {
            return LLVM.ConstNSWSub(@LHSConstant, @RHSConstant);
        }

        public static LLVMValueRef ConstNUWSub(LLVMValueRef @LHSConstant, LLVMValueRef @RHSConstant)
        {
            return LLVM.ConstNUWSub(@LHSConstant, @RHSConstant);
        }

        public static LLVMValueRef ConstFSub(LLVMValueRef @LHSConstant, LLVMValueRef @RHSConstant)
        {
            return LLVM.ConstFSub(@LHSConstant, @RHSConstant);
        }

        public static LLVMValueRef ConstMul(LLVMValueRef @LHSConstant, LLVMValueRef @RHSConstant)
        {
            return LLVM.ConstMul(@LHSConstant, @RHSConstant);
        }

        public static LLVMValueRef ConstNSWMul(LLVMValueRef @LHSConstant, LLVMValueRef @RHSConstant)
        {
            return LLVM.ConstNSWMul(@LHSConstant, @RHSConstant);
        }

        public static LLVMValueRef ConstNUWMul(LLVMValueRef @LHSConstant, LLVMValueRef @RHSConstant)
        {
            return LLVM.ConstNUWMul(@LHSConstant, @RHSConstant);
        }

        public static LLVMValueRef ConstFMul(LLVMValueRef @LHSConstant, LLVMValueRef @RHSConstant)
        {
            return LLVM.ConstFMul(@LHSConstant, @RHSConstant);
        }

        public static LLVMValueRef ConstUDiv(LLVMValueRef @LHSConstant, LLVMValueRef @RHSConstant)
        {
            return LLVM.ConstUDiv(@LHSConstant, @RHSConstant);
        }

        public static LLVMValueRef ConstSDiv(LLVMValueRef @LHSConstant, LLVMValueRef @RHSConstant)
        {
            return LLVM.ConstSDiv(@LHSConstant, @RHSConstant);
        }

        public static LLVMValueRef ConstExactSDiv(LLVMValueRef @LHSConstant, LLVMValueRef @RHSConstant)
        {
            return LLVM.ConstExactSDiv(@LHSConstant, @RHSConstant);
        }

        public static LLVMValueRef ConstExactUDiv(LLVMValueRef @LHSConstant, LLVMValueRef @RHSConstant)
        {
            return LLVM.ConstExactUDiv(@LHSConstant, @RHSConstant);
        }

        public static LLVMValueRef ConstFDiv(LLVMValueRef @LHSConstant, LLVMValueRef @RHSConstant)
        {
            return LLVM.ConstFDiv(@LHSConstant, @RHSConstant);
        }

        public static LLVMValueRef ConstURem(LLVMValueRef @LHSConstant, LLVMValueRef @RHSConstant)
        {
            return LLVM.ConstURem(@LHSConstant, @RHSConstant);
        }

        public static LLVMValueRef ConstSRem(LLVMValueRef @LHSConstant, LLVMValueRef @RHSConstant)
        {
            return LLVM.ConstSRem(@LHSConstant, @RHSConstant);
        }

        public static LLVMValueRef ConstFRem(LLVMValueRef @LHSConstant, LLVMValueRef @RHSConstant)
        {
            return LLVM.ConstFRem(@LHSConstant, @RHSConstant);
        }

        public static LLVMValueRef ConstAnd(LLVMValueRef @LHSConstant, LLVMValueRef @RHSConstant)
        {
            return LLVM.ConstAnd(@LHSConstant, @RHSConstant);
        }

        public static LLVMValueRef ConstOr(LLVMValueRef @LHSConstant, LLVMValueRef @RHSConstant)
        {
            return LLVM.ConstOr(@LHSConstant, @RHSConstant);
        }

        public static LLVMValueRef ConstXor(LLVMValueRef @LHSConstant, LLVMValueRef @RHSConstant)
        {
            return LLVM.ConstXor(@LHSConstant, @RHSConstant);
        }

        public static LLVMValueRef ConstShl(LLVMValueRef @LHSConstant, LLVMValueRef @RHSConstant)
        {
            return LLVM.ConstShl(@LHSConstant, @RHSConstant);
        }

        public static LLVMValueRef ConstLShr(LLVMValueRef @LHSConstant, LLVMValueRef @RHSConstant)
        {
            return LLVM.ConstLShr(@LHSConstant, @RHSConstant);
        }

        public static LLVMValueRef ConstAShr(LLVMValueRef @LHSConstant, LLVMValueRef @RHSConstant)
        {
            return LLVM.ConstAShr(@LHSConstant, @RHSConstant);
        }

        public static LLVMValueRef ConstGEP(LLVMValueRef @ConstantVal, LLVMValueRef[] @ConstantIndices)
        {
            return LLVM.ConstGEP(@ConstantVal, @ConstantIndices);
        }

        public static LLVMValueRef ConstInBoundsGEP(LLVMValueRef @ConstantVal, LLVMValueRef[] @ConstantIndices)
        {
            return LLVM.ConstInBoundsGEP(@ConstantVal, @ConstantIndices);
        }

        public static LLVMValueRef ConstTrunc(LLVMValueRef @ConstantVal, LLVMTypeRef @ToType)
        {
            return LLVM.ConstTrunc(@ConstantVal, @ToType);
        }

        public static LLVMValueRef ConstSExt(LLVMValueRef @ConstantVal, LLVMTypeRef @ToType)
        {
            return LLVM.ConstSExt(@ConstantVal, @ToType);
        }

        public static LLVMValueRef ConstZExt(LLVMValueRef @ConstantVal, LLVMTypeRef @ToType)
        {
            return LLVM.ConstZExt(@ConstantVal, @ToType);
        }

        public static LLVMValueRef ConstFPTrunc(LLVMValueRef @ConstantVal, LLVMTypeRef @ToType)
        {
            return LLVM.ConstFPTrunc(@ConstantVal, @ToType);
        }

        public static LLVMValueRef ConstFPExt(LLVMValueRef @ConstantVal, LLVMTypeRef @ToType)
        {
            return LLVM.ConstFPExt(@ConstantVal, @ToType);
        }

        public static LLVMValueRef ConstUIToFP(LLVMValueRef @ConstantVal, LLVMTypeRef @ToType)
        {
            return LLVM.ConstUIToFP(@ConstantVal, @ToType);
        }

        public static LLVMValueRef ConstSIToFP(LLVMValueRef @ConstantVal, LLVMTypeRef @ToType)
        {
            return LLVM.ConstSIToFP(@ConstantVal, @ToType);
        }

        public static LLVMValueRef ConstFPToUI(LLVMValueRef @ConstantVal, LLVMTypeRef @ToType)
        {
            return LLVM.ConstFPToUI(@ConstantVal, @ToType);
        }

        public static LLVMValueRef ConstFPToSI(LLVMValueRef @ConstantVal, LLVMTypeRef @ToType)
        {
            return LLVM.ConstFPToSI(@ConstantVal, @ToType);
        }

        public static LLVMValueRef ConstPtrToInt(LLVMValueRef @ConstantVal, LLVMTypeRef @ToType)
        {
            return LLVM.ConstPtrToInt(@ConstantVal, @ToType);
        }

        public static LLVMValueRef ConstIntToPtr(LLVMValueRef @ConstantVal, LLVMTypeRef @ToType)
        {
            return LLVM.ConstIntToPtr(@ConstantVal, @ToType);
        }

        public static LLVMValueRef ConstBitCast(LLVMValueRef @ConstantVal, LLVMTypeRef @ToType)
        {
            return LLVM.ConstBitCast(@ConstantVal, @ToType);
        }

        public static LLVMValueRef ConstAddrSpaceCast(LLVMValueRef @ConstantVal, LLVMTypeRef @ToType)
        {
            return LLVM.ConstAddrSpaceCast(@ConstantVal, @ToType);
        }

        public static LLVMValueRef ConstZExtOrBitCast(LLVMValueRef @ConstantVal, LLVMTypeRef @ToType)
        {
            return LLVM.ConstZExtOrBitCast(@ConstantVal, @ToType);
        }

        public static LLVMValueRef ConstSExtOrBitCast(LLVMValueRef @ConstantVal, LLVMTypeRef @ToType)
        {
            return LLVM.ConstSExtOrBitCast(@ConstantVal, @ToType);
        }

        public static LLVMValueRef ConstTruncOrBitCast(LLVMValueRef @ConstantVal, LLVMTypeRef @ToType)
        {
            return LLVM.ConstTruncOrBitCast(@ConstantVal, @ToType);
        }

        public static LLVMValueRef ConstPointerCast(LLVMValueRef @ConstantVal, LLVMTypeRef @ToType)
        {
            return LLVM.ConstPointerCast(@ConstantVal, @ToType);
        }

        public static LLVMValueRef ConstIntCast(LLVMValueRef @ConstantVal, LLVMTypeRef @ToType, bool @isSigned)
        {
            return LLVM.ConstIntCast(@ConstantVal, @ToType, @isSigned);
        }

        public static LLVMValueRef ConstFPCast(LLVMValueRef @ConstantVal, LLVMTypeRef @ToType)
        {
            return LLVM.ConstFPCast(@ConstantVal, @ToType);
        }

        public static LLVMValueRef ConstSelect(LLVMValueRef @ConstantCondition, LLVMValueRef @ConstantIfTrue, LLVMValueRef @ConstantIfFalse)
        {
            return LLVM.ConstSelect(@ConstantCondition, @ConstantIfTrue, @ConstantIfFalse);
        }

        public static LLVMValueRef ConstExtractElement(LLVMValueRef @VectorConstant, LLVMValueRef @IndexConstant)
        {
            return LLVM.ConstExtractElement(@VectorConstant, @IndexConstant);
        }

        public static LLVMValueRef ConstInsertElement(LLVMValueRef @VectorConstant, LLVMValueRef @ElementValueConstant, LLVMValueRef @IndexConstant)
        {
            return LLVM.ConstInsertElement(@VectorConstant, @ElementValueConstant, @IndexConstant);
        }

        public static LLVMValueRef ConstShuffleVector(LLVMValueRef @VectorAConstant, LLVMValueRef @VectorBConstant, LLVMValueRef @MaskConstant)
        {
            return LLVM.ConstShuffleVector(@VectorAConstant, @VectorBConstant, @MaskConstant);
        }

        public static LLVMValueRef ConstExtractValue(LLVMValueRef @AggConstant, uint[] @IdxList)
        {
            return LLVM.ConstExtractValue(@AggConstant, @IdxList);
        }

        public static LLVMValueRef ConstInsertValue(LLVMValueRef @AggConstant, LLVMValueRef @ElementValueConstant, uint[] @IdxList)
        {
            return LLVM.ConstInsertValue(@AggConstant, @ElementValueConstant, @IdxList);
        }
    }
}