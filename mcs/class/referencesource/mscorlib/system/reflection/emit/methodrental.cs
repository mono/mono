// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Class:  MethodRental
**
** <OWNER>Microsoft</OWNER>
**
**
** MethodRental class is to provide a fast way to swap method body implementation
**  given a method of a class
**
** 
===========================================================*/
namespace System.Reflection.Emit {
    
    using System;
    using System.Reflection;
    using System.Threading;
    using System.Runtime.CompilerServices;
    using System.Security.Permissions;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;
    using System.Globalization;
    using System.Security;
    using System.Diagnostics.Contracts;

    // MethodRental class provides the ability to insert a new method body of an 
    // existing method on a class defined in a DynamicModule.
    // Can throw OutOfMemory exception.
    // 
    //This class contains only static methods and does not require serialization.
    [HostProtection(MayLeakOnAbort = true)]
    [ClassInterface(ClassInterfaceType.None)]
    [ComDefaultInterface(typeof(_MethodRental))]
[System.Runtime.InteropServices.ComVisible(true)]
    sealed public class MethodRental : _MethodRental
    {
        public const int JitOnDemand            = 0x0000;        // jit the method body when it is necessary
        public const int JitImmediate        = 0x0001;        // jit the method body now 
    
        [System.Security.SecuritySafeCritical]  // auto-generated
        [SecurityPermissionAttribute(SecurityAction.Demand, UnmanagedCode=true)]
        [MethodImplAttribute(MethodImplOptions.NoInlining)] // Methods containing StackCrawlMark local var has to be marked non-inlineable
        public static void SwapMethodBody(
            Type    cls,            // [in] class containing the method
            int     methodtoken,    // [in] method token
            IntPtr  rgIL,           // [in] pointer to bytes
            int     methodSize,     // [in] the size of the new method body in bytes
            int     flags)          // [in] flags
        {
            if (methodSize <= 0 || methodSize >= 0x3f0000)
                throw new ArgumentException(Environment.GetResourceString("Argument_BadSizeForData"), "methodSize");

            if (cls==null)
                throw new ArgumentNullException("cls");
            Contract.EndContractBlock();

            Module module = cls.Module;
            InternalModuleBuilder internalMB;
            ModuleBuilder mb = module as ModuleBuilder;
            if (mb != null)
                internalMB = mb.InternalModule;
            else
                internalMB = module as InternalModuleBuilder;

            // can only swap method body on dynamic module
            // dynamic internal module type is always exactly InternalModuleBuilder, non-dynamic is always something different
            if (internalMB == null)
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_NotDynamicModule"));

            RuntimeType rType;

            if (cls is TypeBuilder)
            {
                // If it is a TypeBuilder, make sure that TypeBuilder is already been baked.
                TypeBuilder typeBuilder = (TypeBuilder) cls;
                if (!typeBuilder.IsCreated())
                    throw new NotSupportedException(Environment.GetResourceString("NotSupported_NotAllTypesAreBaked", typeBuilder.Name)); 
                    
                // get the corresponding runtime type for the TypeBuilder.
                rType = typeBuilder.BakedRuntimeType;
                
            }
            else
            {
                rType = cls as RuntimeType;
            }

            if (rType == null)
                throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeType"), "cls");

            StackCrawlMark mark = StackCrawlMark.LookForMyCaller;

            RuntimeAssembly rtAssembly = internalMB.GetRuntimeAssembly();
            lock (rtAssembly.SyncRoot)
            {
                SwapMethodBody(rType.GetTypeHandleInternal(), methodtoken, rgIL, methodSize, flags, JitHelpers.GetStackCrawlMarkHandle(ref mark));
            }
        }
    
        [System.Security.SecurityCritical]  // auto-generated
        [ResourceExposure(ResourceScope.None)]
        [DllImport(JitHelpers.QCall, CharSet = CharSet.Unicode)]
        [SuppressUnmanagedCodeSecurity]
        private extern static void SwapMethodBody(
            RuntimeTypeHandle cls,            // [in] class containing the method
            int            methodtoken,        // [in] method token
            IntPtr        rgIL,                // [in] pointer to bytes
            int            methodSize,            // [in] the size of the new method body in bytes
            int         flags,              // [in] flags
            StackCrawlMarkHandle stackMark); // [in] stack crawl mark used to find caller

        // private constructor to prevent class to be constructed.
        private MethodRental() {}


        void _MethodRental.GetTypeInfoCount(out uint pcTInfo)
        {
            throw new NotImplementedException();
        }

        void _MethodRental.GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo)
        {
            throw new NotImplementedException();
        }

        void _MethodRental.GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
        {
            throw new NotImplementedException();
        }

        void _MethodRental.Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
        {
            throw new NotImplementedException();
        }

    }
}
