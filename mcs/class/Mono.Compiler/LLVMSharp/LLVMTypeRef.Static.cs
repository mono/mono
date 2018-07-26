namespace LLVMSharp
{
    partial struct LLVMTypeRef
    {
        public static LLVMTypeRef FunctionType(LLVMTypeRef returnType, LLVMTypeRef[] @ParamTypes, bool @IsVarArg)
        {
            return LLVM.FunctionType(returnType, @ParamTypes, @IsVarArg);
        }

        public static LLVMTypeRef Int1Type()
        {
            return LLVM.Int1Type();
        }

        public static LLVMTypeRef Int1TypeInContext(LLVMContextRef @C)
        {
            return LLVM.Int1TypeInContext(@C);
        }

        public static LLVMTypeRef Int8Type()
        {
            return LLVM.Int8Type();
        }

        public static LLVMTypeRef Int8TypeInContext(LLVMContextRef @C)
        {
            return LLVM.Int8TypeInContext(@C);
        }

        public static LLVMTypeRef Int16Type()
        {
            return LLVM.Int16Type();
        }

        public static LLVMTypeRef Int16TypeInContext(LLVMContextRef @C)
        {
            return LLVM.Int16TypeInContext(@C);
        }

        public static LLVMTypeRef Int32Type()
        {
            return LLVM.Int32Type();
        }

        public static LLVMTypeRef Int32TypeInContext(LLVMContextRef @C)
        {
            return LLVM.Int32TypeInContext(@C);
        }

        public static LLVMTypeRef Int64Type()
        {
            return LLVM.Int64Type();
        }

        public static LLVMTypeRef Int64TypeInContext(LLVMContextRef @C)
        {
            return LLVM.Int64TypeInContext(@C);
        }

        public static LLVMTypeRef IntType(uint @NumBits)
        {
            return LLVM.IntType(@NumBits);
        }

        public static LLVMTypeRef IntTypeInContext(LLVMContextRef @C, uint @NumBits)
        {
            return LLVM.IntTypeInContext(@C, @NumBits);
        }

        public static LLVMTypeRef HalfType()
        {
            return LLVM.HalfType();
        }

        public static LLVMTypeRef HalfTypeInContext(LLVMContextRef @C)
        {
            return LLVM.HalfTypeInContext(@C);
        }

        public static LLVMTypeRef FloatType()
        {
            return LLVM.FloatType();
        }

        public static LLVMTypeRef FloatTypeInContext(LLVMContextRef @C)
        {
            return LLVM.FloatTypeInContext(@C);
        }

        public static LLVMTypeRef DoubleType()
        {
            return LLVM.DoubleType();
        }

        public static LLVMTypeRef DoubleTypeInContext(LLVMContextRef @C)
        {
            return LLVM.DoubleTypeInContext(@C);
        }

        public static LLVMTypeRef X86FP80Type()
        {
            return LLVM.X86FP80Type();
        }

        public static LLVMTypeRef X86FP80TypeInContext(LLVMContextRef @C)
        {
            return LLVM.X86FP80TypeInContext(@C);
        }

        public static LLVMTypeRef FP128Type()
        {
            return LLVM.FP128Type();
        }

        public static LLVMTypeRef FP128TypeInContext(LLVMContextRef @C)
        {
            return LLVM.FP128TypeInContext(@C);
        }

        public static LLVMTypeRef PPCFP128Type()
        {
            return LLVM.PPCFP128Type();
        }

        public static LLVMTypeRef PPCFP128TypeInContext(LLVMContextRef @C)
        {
            return LLVM.PPCFP128TypeInContext(@C);
        }

        public static LLVMTypeRef X86MMXTypeInContext(LLVMContextRef @C)
        {
            return LLVM.X86MMXTypeInContext(@C);
        }

        public static LLVMTypeRef X86MMXType()
        {
            return LLVM.X86MMXType();
        }

        public static LLVMTypeRef IntPtrType(LLVMTargetDataRef @TD)
        {
            return LLVM.IntPtrType(@TD);
        }

        public static LLVMTypeRef IntPtrTypeInContext(LLVMContextRef @C, LLVMTargetDataRef @TD)
        {
            return LLVM.IntPtrTypeInContext(@C, @TD);
        }

        public static LLVMTypeRef IntPtrTypeForAS(LLVMTargetDataRef @TD, uint @AS)
        {
            return LLVM.IntPtrTypeForAS(@TD, @AS);
        }

        public static LLVMTypeRef IntPtrTypeForASInContext(LLVMContextRef @C, LLVMTargetDataRef @TD, uint @AS)
        {
            return LLVM.IntPtrTypeForASInContext(@C, @TD, @AS);
        }

        public static LLVMTypeRef VoidType()
        {
            return LLVM.VoidType();
        }

        public static LLVMTypeRef VoidTypeInContext(LLVMContextRef @C)
        {
            return LLVM.VoidTypeInContext(@C);
        }

        public static LLVMTypeRef LabelType()
        {
            return LLVM.LabelType();
        }

        public static LLVMTypeRef LabelTypeInContext(LLVMContextRef @C)
        {
            return LLVM.LabelTypeInContext(@C);
        }

        public static LLVMTypeRef PointerType(LLVMTypeRef @ElementType, uint @AddressSpace)
        {
            return LLVM.PointerType(@ElementType, @AddressSpace);
        }

        public static LLVMTypeRef ArrayType(LLVMTypeRef @ElementType, uint @ElementCount)
        {
            return LLVM.ArrayType(@ElementType, @ElementCount);
        }

        public static LLVMTypeRef VectorType(LLVMTypeRef @ElementType, uint @ElementCount)
        {
            return LLVM.VectorType(@ElementType, @ElementCount);
        }

        public static LLVMTypeRef StructType(LLVMTypeRef[] @ElementTypes, bool @Packed)
        {
            return LLVM.StructType(@ElementTypes, @Packed);
        }

        public static LLVMTypeRef StructCreateNamed(LLVMContextRef @C, string @Name)
        {
            return LLVM.StructCreateNamed(@C, @Name);
        }

        public static LLVMGenericValueRef CreateGenericValueOfInt(LLVMTypeRef @Ty, ulong @N, bool @IsSigned)
        {
            return LLVM.CreateGenericValueOfInt(@Ty, @N, @IsSigned);
        }

        public static LLVMGenericValueRef CreateGenericValueOfFloat(LLVMTypeRef @Ty, double @N)
        {
            return LLVM.CreateGenericValueOfFloat(@Ty, @N);
        }

        public static LLVMValueRef ConstInlineAsm(LLVMTypeRef @Ty, string @AsmString, string @Constraints, bool @HasSideEffects, bool @IsAlignStack)
        {
            return LLVM.ConstInlineAsm(@Ty, @AsmString, @Constraints, @HasSideEffects, @IsAlignStack);
        }

        public static LLVMValueRef ConstPointerNull(LLVMTypeRef @Ty)
        {
            return LLVM.ConstPointerNull(@Ty);
        }

        public static LLVMValueRef ConstInt(LLVMTypeRef @IntTy, ulong @N, bool @SignExtend)
        {
            return LLVM.ConstInt(@IntTy, @N, @SignExtend);
        }

        public static LLVMValueRef ConstIntOfArbitraryPrecision(LLVMTypeRef @IntTy, ulong[] @Words)
        {
            return LLVM.ConstIntOfArbitraryPrecision(@IntTy, (uint)@Words.Length, @Words);
        }

        public static LLVMValueRef ConstIntOfString(LLVMTypeRef @IntTy, string @Text, byte @Radix)
        {
            return LLVM.ConstIntOfString(@IntTy, @Text, @Radix);
        }

        public static LLVMValueRef ConstIntOfStringAndSize(LLVMTypeRef @IntTy, string @Text, uint @SLen, byte @Radix)
        {
            return LLVM.ConstIntOfStringAndSize(@IntTy, @Text, @SLen, @Radix);
        }

        public static LLVMValueRef ConstReal(LLVMTypeRef @RealTy, double @N)
        {
            return LLVM.ConstReal(@RealTy, @N);
        }

        public static LLVMValueRef ConstRealOfString(LLVMTypeRef @RealTy, string @Text)
        {
            return LLVM.ConstRealOfString(@RealTy, @Text);
        }

        public static LLVMValueRef ConstRealOfStringAndSize(LLVMTypeRef @RealTy, string @Text, uint @SLen)
        {
            return LLVM.ConstRealOfStringAndSize(@RealTy, @Text, @SLen);
        }

        public static LLVMValueRef ConstArray(LLVMTypeRef @ElementTy, LLVMValueRef[] @ConstantVals)
        {
            return LLVM.ConstArray(@ElementTy, @ConstantVals);
        }

        public static LLVMValueRef ConstNamedStruct(LLVMTypeRef @StructTy, LLVMValueRef[] @ConstantVals)
        {
            return LLVM.ConstNamedStruct(@StructTy, @ConstantVals);
        }

        public static LLVMValueRef ConstNull(LLVMTypeRef @Ty)
        {
            return LLVM.ConstNull(@Ty);
        }

        public static LLVMValueRef ConstAllOnes(LLVMTypeRef @Ty)
        {
            return LLVM.ConstAllOnes(@Ty);
        }
    }
}