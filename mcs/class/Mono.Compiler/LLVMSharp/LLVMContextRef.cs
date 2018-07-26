namespace LLVMSharp
{
    using System;

    partial struct LLVMContextRef
    {
        public void ContextDispose()
        {
            LLVM.ContextDispose(this);
        }

        public uint GetMDKindIDInContext(string @Name, uint @SLen)
        {
            return LLVM.GetMDKindIDInContext(this, @Name, @SLen);
        }

        public LLVMTypeRef Int1TypeInContext()
        {
            return LLVM.Int1TypeInContext(this);
        }

        public LLVMTypeRef Int8TypeInContext()
        {
            return LLVM.Int8TypeInContext(this);
        }

        public LLVMTypeRef Int16TypeInContext()
        {
            return LLVM.Int16TypeInContext(this);
        }

        public LLVMTypeRef Int32TypeInContext()
        {
            return LLVM.Int32TypeInContext(this);
        }

        public LLVMTypeRef Int64TypeInContext()
        {
            return LLVM.Int64TypeInContext(this);
        }

        public LLVMTypeRef IntTypeInContext(uint @NumBits)
        {
            return LLVM.IntTypeInContext(this, @NumBits);
        }

        public LLVMTypeRef HalfTypeInContext()
        {
            return LLVM.HalfTypeInContext(this);
        }

        public LLVMTypeRef FloatTypeInContext()
        {
            return LLVM.FloatTypeInContext(this);
        }

        public LLVMTypeRef DoubleTypeInContext()
        {
            return LLVM.DoubleTypeInContext(this);
        }

        public LLVMTypeRef X86FP80TypeInContext()
        {
            return LLVM.X86FP80TypeInContext(this);
        }

        public LLVMTypeRef FP128TypeInContext()
        {
            return LLVM.FP128TypeInContext(this);
        }

        public LLVMTypeRef PPCFP128TypeInContext()
        {
            return LLVM.PPCFP128TypeInContext(this);
        }

        public LLVMTypeRef StructTypeInContext(LLVMTypeRef[] @ElementTypes, bool @Packed)
        {
            return LLVM.StructTypeInContext(this, @ElementTypes, @Packed);
        }

        public LLVMTypeRef StructCreateNamed(string @Name)
        {
            return LLVM.StructCreateNamed(this, @Name);
        }

        public LLVMTypeRef VoidTypeInContext()
        {
            return LLVM.VoidTypeInContext(this);
        }

        public LLVMTypeRef LabelTypeInContext()
        {
            return LLVM.LabelTypeInContext(this);
        }

        public LLVMTypeRef X86MMXTypeInContext()
        {
            return LLVM.X86MMXTypeInContext(this);
        }

        public LLVMValueRef ConstStringInContext(string @Str, uint @Length, bool @DontNullTerminate)
        {
            return LLVM.ConstStringInContext(this, @Str, @Length, @DontNullTerminate);
        }

        public LLVMValueRef ConstStructInContext(LLVMValueRef[] @ConstantVals, bool @Packed)
        {
            return LLVM.ConstStructInContext(this, @ConstantVals, @Packed);
        }

        public LLVMValueRef MDStringInContext(string @Str, uint @SLen)
        {
            return LLVM.MDStringInContext(this, @Str, @SLen);
        }

        public LLVMValueRef MDNodeInContext(LLVMValueRef[] @Vals)
        {
            return LLVM.MDNodeInContext(this, @Vals);
        }

        public LLVMBasicBlockRef AppendBasicBlockInContext(LLVMValueRef @Fn, string @Name)
        {
            return LLVM.AppendBasicBlockInContext(this, @Fn, @Name);
        }

        public LLVMBasicBlockRef InsertBasicBlockInContext(LLVMBasicBlockRef @BB, string @Name)
        {
            return LLVM.InsertBasicBlockInContext(this, @BB, @Name);
        }

        public LLVMBuilderRef CreateBuilderInContext()
        {
            return LLVM.CreateBuilderInContext(this);
        }

        public bool ParseBitcodeInContext(LLVMMemoryBufferRef @MemBuf, out LLVMModuleRef @OutModule, out IntPtr @OutMessage)
        {
            return LLVM.ParseBitcodeInContext(this, @MemBuf, out @OutModule, out @OutMessage);
        }

        public bool GetBitcodeModuleInContext(LLVMMemoryBufferRef @MemBuf, out LLVMModuleRef @OutM, out IntPtr @OutMessage)
        {
            return LLVM.GetBitcodeModuleInContext(this, @MemBuf, out @OutM, out @OutMessage);
        }

        public LLVMTypeRef IntPtrTypeInContext(LLVMTargetDataRef @TD)
        {
            return LLVM.IntPtrTypeInContext(this, @TD);
        }

        public LLVMTypeRef IntPtrTypeForASInContext(LLVMTargetDataRef @TD, uint @AS)
        {
            return LLVM.IntPtrTypeForASInContext(this, @TD, @AS);
        }

        public bool ParseIRInContext(LLVMMemoryBufferRef @MemBuf, out LLVMModuleRef @OutM, out IntPtr @OutMessage)
        {
            return LLVM.ParseIRInContext(this, @MemBuf, out @OutM, out @OutMessage);
        }
    }
}