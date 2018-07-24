namespace LLVMSharp
{
    using System;
    using System.Runtime.InteropServices;

    public sealed class Module
    {
        internal readonly LLVMModuleRef instance;

        public Module(string moduleId)
        {
            this.instance = LLVM.ModuleCreateWithName(moduleId);
        }

        public Module(string moduleId, LLVMContextRef context)
        {
            this.instance = LLVM.ModuleCreateWithNameInContext(moduleId, context);
        }

        internal Module(LLVMModuleRef module)
        {
            this.instance = LLVM.CloneModule(module);
        }
        
        public Module Clone()
        {
            return new Module(this.instance);
        }

        public void DisposeModule()
        {
            LLVM.DisposeModule(this.instance);
        }
        
        public string GetDataLayout()
        {
            return LLVM.GetDataLayout(this.instance);
        }

        public void SetDataLayout(string @Triple)
        {
            LLVM.SetDataLayout(this.instance, @Triple);
        }

        public string GetTarget()
        {
            return LLVM.GetTarget(this.instance);
        }

        public void SetTarget(string @Triple)
        {
            LLVM.SetTarget(this.instance, @Triple);
        }

        public void Dump()
        {
            LLVM.DumpModule(this.instance);
        }

        public bool PrintModuleToFile(string @Filename, out IntPtr @ErrorMessage)
        {
            return LLVM.PrintModuleToFile(this.instance, @Filename, out @ErrorMessage);
        }

        public string PrintModuleToString()
        {
            IntPtr ptr = LLVM.PrintModuleToString(this.instance);
            string retval = Marshal.PtrToStringAnsi(ptr) ?? string.Empty;
            LLVM.DisposeMessage(ptr);
            return retval;
        }

        public void SetModuleInlineAsm(string @Asm)
        {
            LLVM.SetModuleInlineAsm(this.instance, @Asm);
        }

        public LLVMContextRef GetModuleContext()
        {
            return LLVM.GetModuleContext(this.instance);
        }

        public LLVMTypeRef GetTypeByName(string @Name)
        {
            return LLVM.GetTypeByName(this.instance, @Name);
        }

        public uint GetNamedMetadataNumOperands(string @name)
        {
            return LLVM.GetNamedMetadataNumOperands(this.instance, @name);
        }

        public LLVMValueRef[] GetNamedMetadataOperands(string @name)
        {
            return LLVM.GetNamedMetadataOperands(this.instance, @name);
        }

        public void AddNamedMetadataOperand(string @name, LLVMValueRef @Val)
        {
            LLVM.AddNamedMetadataOperand(this.instance, @name, @Val);
        }

        public LLVMValueRef AddFunction(string @Name, LLVMTypeRef @FunctionTy)
        {
            return LLVM.AddFunction(this.instance, @Name, @FunctionTy);
        }

        public LLVMValueRef GetNamedFunction(string @Name)
        {
            return LLVM.GetNamedFunction(this.instance, @Name);
        }

        public LLVMValueRef GetFirstFunction()
        {
            return LLVM.GetFirstFunction(this.instance);
        }

        public LLVMValueRef GetLastFunction()
        {
            return LLVM.GetLastFunction(this.instance);
        }

        public LLVMValueRef AddGlobal(LLVMTypeRef @Ty, string @Name)
        {
            return LLVM.AddGlobal(this.instance, @Ty, @Name);
        }

        public LLVMValueRef AddGlobalInAddressSpace(LLVMTypeRef @Ty, string @Name, uint @AddressSpace)
        {
            return LLVM.AddGlobalInAddressSpace(this.instance, @Ty, @Name, @AddressSpace);
        }

        public LLVMValueRef GetNamedGlobal(string @Name)
        {
            return LLVM.GetNamedGlobal(this.instance, @Name);
        }

        public LLVMValueRef GetFirstGlobal()
        {
            return LLVM.GetFirstGlobal(this.instance);
        }

        public LLVMValueRef GetLastGlobal()
        {
            return LLVM.GetLastGlobal(this.instance);
        }

        public LLVMValueRef AddAlias(LLVMTypeRef @Ty, LLVMValueRef @Aliasee, string @Name)
        {
            return LLVM.AddAlias(this.instance, @Ty, @Aliasee, @Name);
        }

        public LLVMModuleProviderRef CreateModuleProviderForExistingModule()
        {
            return LLVM.CreateModuleProviderForExistingModule(this.instance);
        }

        public LLVMPassManagerRef CreateFunctionPassManagerForModule()
        {
            return LLVM.CreateFunctionPassManagerForModule(this.instance);
        }

        public bool VerifyModule(LLVMVerifierFailureAction @Action, out IntPtr @OutMessage)
        {
            return LLVM.VerifyModule(this.instance, @Action, out @OutMessage);
        }

        public int WriteBitcodeToFile(string @Path)
        {
            return LLVM.WriteBitcodeToFile(this.instance, @Path);
        }

        public int WriteBitcodeToFD(int @FD, int @ShouldClose, int @Unbuffered)
        {
            return LLVM.WriteBitcodeToFD(this.instance, @FD, @ShouldClose, @Unbuffered);
        }

        public int WriteBitcodeToFileHandle(int @Handle)
        {
            return LLVM.WriteBitcodeToFileHandle(this.instance, @Handle);
        }

        public LLVMMemoryBufferRef WriteBitcodeToMemoryBuffer()
        {
            return LLVM.WriteBitcodeToMemoryBuffer(this.instance);
        }

        public override string ToString()
        {
            return this.PrintModuleToString();
        }
    }
}