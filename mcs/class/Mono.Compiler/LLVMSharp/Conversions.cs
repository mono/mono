namespace LLVMSharp
{
    using System;

    partial struct LLVMBool
    {
        public static implicit operator bool(LLVMBool b)
        {
            return b.Value != 0;
        }

        public static implicit operator LLVMBool(bool b)
        {
            return new LLVMBool(b ? 1 : 0);
        }
    }

    partial struct LLVMValueRef
    {
        public static implicit operator LLVMValueRef(LLVMBasicBlockRef b)
        {
            return LLVM.BasicBlockAsValue(b);
        }

        public static implicit operator LLVMBasicBlockRef(LLVMValueRef v)
        {
            return LLVM.ValueAsBasicBlock(v);
        }
    }

    partial struct size_t
    {
        public static implicit operator size_t(int b)
        {
            return new size_t(new IntPtr(b));
        }

        public static implicit operator size_t(long b)
        {
            return new size_t(new IntPtr(b));
        }

        public static implicit operator int(size_t v)
        {
            return v.Pointer.ToInt32();
        }

        public static implicit operator long(size_t b)
        {
            return b.Pointer.ToInt64();
        }
    }
}