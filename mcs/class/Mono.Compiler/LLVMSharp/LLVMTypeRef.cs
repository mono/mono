namespace LLVMSharp
{
    using System;
    using System.Runtime.InteropServices;

    partial struct LLVMTypeRef
    {
        public LLVMTypeKind TypeKind
        {
            get { return LLVM.GetTypeKind(this); }
        }

        public bool TypeIsSized
        {
            get { return LLVM.TypeIsSized(this); }
        }

        public LLVMContextRef GetTypeContext()
        {
            return LLVM.GetTypeContext(this);
        }

        public void Dump()
        {
            LLVM.DumpType(this);
        }

        public string PrintTypeToString()
        {
            IntPtr ptr = LLVM.PrintTypeToString(this);
            string retval = Marshal.PtrToStringAnsi(ptr) ?? "";
            LLVM.DisposeMessage(ptr);
            return retval;
        }

        public uint GetIntTypeWidth()
        {
            return LLVM.GetIntTypeWidth(this);
        }

        public bool IsFunctionVarArg
        {
            get { return LLVM.IsFunctionVarArg(this); }
        }

        public LLVMTypeRef GetReturnType()
        {
            return LLVM.GetReturnType(this);
        }

        public uint CountParamTypes()
        {
            return LLVM.CountParamTypes(this);
        }

        public LLVMTypeRef[] GetParamTypes()
        {
            return LLVM.GetParamTypes(this);
        }

        public LLVMTypeRef[] GetSubtypes()
        {
            return LLVM.GetSubtypes(this);
        }

        public string GetStructName()
        {
            return LLVM.GetStructName(this);
        }

        public void StructSetBody(LLVMTypeRef[] @ElementTypes, bool @Packed)
        {
            LLVM.StructSetBody(this, @ElementTypes, @Packed);
        }

        public uint CountStructElementTypes()
        {
            return LLVM.CountStructElementTypes(this);
        }

        public LLVMTypeRef[] GetStructElementTypes()
        {
            return LLVM.GetStructElementTypes(this);
        }

        public LLVMTypeRef StructGetTypeAtIndex(uint @index)
        {
            return LLVM.StructGetTypeAtIndex(this, @index);
        }

        public bool IsPackedStruct
        {
            get { return LLVM.IsPackedStruct(this); }
        }

        public bool IsOpaqueStruct
        {
            get { return LLVM.IsOpaqueStruct(this); }
        }

        public LLVMTypeRef GetElementType()
        {
            return LLVM.GetElementType(this);
        }

        public uint GetArrayLength()
        {
            return LLVM.GetArrayLength(this);
        }

        public uint GetPointerAddressSpace()
        {
            return LLVM.GetPointerAddressSpace(this);
        }

        public uint GetVectorSize()
        {
            return LLVM.GetVectorSize(this);
        }

        public LLVMValueRef GetUndef()
        {
            return LLVM.GetUndef(this);
        }

        public LLVMValueRef AlignOf()
        {
            return LLVM.AlignOf(this);
        }

        public LLVMValueRef SizeOf()
        {
            return LLVM.SizeOf(this);
        }

        public double GenericValueToFloat(LLVMGenericValueRef @GenVal)
        {
            return LLVM.GenericValueToFloat(this, @GenVal);
        }

        public override string ToString()
        {
            return this.PrintTypeToString();
        }
    }
}