namespace LLVMSharp
{
    using System;
    using System.Runtime.InteropServices;

    partial struct LLVMBasicBlockRef
    {
        public LLVMValueRef BasicBlockAsValue()
        {
            return LLVM.BasicBlockAsValue(this);
        }

        public LLVMValueRef GetBasicBlockParent()
        {
            return LLVM.GetBasicBlockParent(this);
        }

        public LLVMValueRef GetBasicBlockTerminator()
        {
            return LLVM.GetBasicBlockTerminator(this);
        }

        public LLVMBasicBlockRef GetNextBasicBlock()
        {
            return LLVM.GetNextBasicBlock(this);
        }

        public LLVMBasicBlockRef GetPreviousBasicBlock()
        {
            return LLVM.GetPreviousBasicBlock(this);
        }

        public LLVMBasicBlockRef InsertBasicBlock(string @Name)
        {
            return LLVM.InsertBasicBlock(this, @Name);
        }

        public void DeleteBasicBlock()
        {
            LLVM.DeleteBasicBlock(this);
        }

        public void RemoveBasicBlockFromParent()
        {
            LLVM.RemoveBasicBlockFromParent(this);
        }

        public void MoveBasicBlockBefore(LLVMBasicBlockRef @MovePos)
        {
            LLVM.MoveBasicBlockBefore(this, @MovePos);
        }

        public void MoveBasicBlockAfter(LLVMBasicBlockRef @MovePos)
        {
            LLVM.MoveBasicBlockAfter(this, @MovePos);
        }

        public LLVMValueRef GetFirstInstruction()
        {
            return LLVM.GetFirstInstruction(this);
        }

        public LLVMValueRef GetLastInstruction()
        {
            return LLVM.GetLastInstruction(this);
        }

        public void Dump()
        {
            LLVM.DumpValue(this);
        }

        public override string ToString()
        {
            IntPtr ptr = LLVM.PrintValueToString(this);
            string retval = Marshal.PtrToStringAnsi(ptr) ?? string.Empty;
            LLVM.DisposeMessage(ptr);
            return retval;
        }
    }
}