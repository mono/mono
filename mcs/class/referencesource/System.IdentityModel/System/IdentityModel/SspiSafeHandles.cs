//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

/*++
Abstract:
        The file contains SafeHandles implementations for SSPI.
        These handle wrappers do guarantee that OS resources get cleaned up when the app domain dies.

        All PInvoke declarations that do freeing  the  OS resources  _must_ be in this file
        All PInvoke declarations that do allocation the OS resources _must_ be in this file


Details:

        The protection from leaking OF the OS resources is based on two technologies
        1) SafeHandle class
        2) Non interuptible regions using Constrained Execution Region (CER) technology

        For simple cases SafeHandle class does all the job. The Prerequisites are:
        - A resource is able to be represented by IntPtr type (32 bits on 32 bits platforms).
        - There is a PInvoke availble that does the creation of the resource.
          That PInvoke either returns the handle value or it writes the handle into out/ref parameter.
        - The above PInvoke as part of the call does NOT free any OS resource.

        For those "simple" cases we desinged SafeHandle-derived classes that provide
        static methods to allocate a handle object.
        Each such derived class provides a handle release method that is run as non-interrupted.

        For more complicated cases we employ the support for non-interruptible methods (CERs).
        Each CER is a tree of code rooted at a catch or finally clause for a specially marked exception
        handler (preceded by the RuntimeHelpers.PrepareConstrainedRegions() marker) or the Dispose or
        ReleaseHandle method of a SafeHandle derived class. The graph is automatically computed by the
        runtime (typically at the jit time of the root method), but cannot follow virtual or interface
        calls (these must be explicitly prepared via RuntimeHelpers.PrepareMethod once the definite target
        method is known). Also, methods in the graph that must be included in the CER must be marked with
        a reliability contract stating guarantees about the consistency of the system if an error occurs
        while they are executing. Look for ReliabilityContract for examples (a full explanation of the
        semantics of this contract is beyond the scope of this comment).

        An example of the top-level of a CER:

            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                // Normal code
            }
            finally
            {
                // Guaranteed to get here even in low memory scenarios. Thread abort will not interrupt
                // this clause and we won't fail because of a jit allocation of any method called (modulo
                // restrictions on interface/virtual calls listed above and further restrictions listed
                // below).
            }

        Another common pattern is an empty-try (where you really just want a region of code the runtime
        won't interrupt you in):

            RuntimeHelpers.PrepareConstrainedRegions();
            try {} finally
            {
                // Non-interruptible code here
            }

        This ugly syntax will be supplanted with compiler support at some point.

        While within a CER region certain restrictions apply in order to avoid having the runtime inject
        a potential fault point into your code (and of course you're are responsible for ensuring your
        code doesn't inject any explicit fault points of its own unless you know how to tolerate them).

        A quick and dirty guide to the possible causes of fault points in CER regions:
        - Explicit allocations (though allocating a value type only implies allocation on the stack,
          which may not present an issue).
        - Boxing a value type (C# does this implicitly for you in many cases, so be careful).
        - Use of Monitor.Enter or the lock keyword.
        - Accessing a multi-dimensional array.
        - Calling any method outside your control that doesn't make a guarantee (e.g. via a
          ReliabilityAttribute) that it doesn't introduce failure points.
        - Making P/Invoke calls with non-blittable parameters types. Blittable types are:
            - SafeHandle when used as an [in] parameter
            - NON BOXED base types that fit onto a machine word
            - ref struct with blittable fields
            - class type with blittable fields
            - pinned Unicode strings using "fixed" statement
            - pointers of any kind
            - IntPtr type
        - P/Invokes should not have any CharSet attribute on it's declaration.
          Obvioulsy string types should not appear in the parameters.
        - String type MUST not appear in a field of a marshaled ref struct or class in a P?Invoke


(taken from the NCL classes)
--*/
namespace System.IdentityModel
{
    using System.Security;
    using System.Runtime.InteropServices;
    using System.Runtime.CompilerServices;
    using System.Runtime.Versioning;
    using System.Threading;
    using System.Security.Permissions;
    using System.ComponentModel;
    using System.Text;
    using System.ServiceModel.Diagnostics;
    using Microsoft.Win32.SafeHandles;
    using System.Runtime.ConstrainedExecution;


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct SSPIHandle
    {
        IntPtr HandleHi;
        IntPtr HandleLo;

        public bool IsZero
        {
            get { return HandleHi == IntPtr.Zero && HandleLo == IntPtr.Zero; }
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal void SetToInvalid()
        {
            HandleHi = IntPtr.Zero;
            HandleLo = IntPtr.Zero;
        }

        //public static string ToString(ref SSPIHandle obj) 
        //{
        //    return obj.HandleHi.ToString("x") + ":" + obj.HandleLo.ToString("x");
        //}
    }

    class SafeDeleteContext : SafeHandle
    {
        const string SECURITY = "security.Dll";

        const string dummyStr = " ";
        static readonly byte[] dummyBytes = new byte[] { 0 };

        internal SSPIHandle _handle; //should be always used as by ref in PINvokes parameters
        SafeFreeCredentials _EffectiveCredential;

        protected SafeDeleteContext()
            : base(IntPtr.Zero, true)
        {
            _handle = new SSPIHandle();
        }

        public override bool IsInvalid
        {
            get
            {
                return IsClosed || _handle.IsZero;
            }
        }

        //This method should never be called for this type
        //public new IntPtr DangerousGetHandle()
        //{
        //    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException());
        //}

        //public static string ToString(SafeDeleteContext obj) 
        //{
        //    {   return obj == null ? "null" : SSPIHandle.ToString(ref obj._handle); }
        //}

        //-------------------------------------------------------------------
        internal static unsafe int InitializeSecurityContext(
            SafeFreeCredentials inCredentials,
            ref SafeDeleteContext refContext,
            string targetName,
            SspiContextFlags inFlags,
            Endianness endianness,
            SecurityBuffer inSecBuffer,
            SecurityBuffer[] inSecBuffers,
            SecurityBuffer outSecBuffer,
            ref SspiContextFlags outFlags)
        {
            if (inCredentials == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("inCredentials");
            }

            SecurityBufferDescriptor inSecurityBufferDescriptor = null;
            if (inSecBuffer != null)
            {
                inSecurityBufferDescriptor = new SecurityBufferDescriptor(1);
            }
            else if (inSecBuffers != null)
            {
                inSecurityBufferDescriptor = new SecurityBufferDescriptor(inSecBuffers.Length);
            }
            SecurityBufferDescriptor outSecurityBufferDescriptor = new SecurityBufferDescriptor(1);

            // actually this is returned in outFlags
            bool isSspiAllocated = (inFlags & SspiContextFlags.AllocateMemory) != 0 ? true : false;

            int errorCode = -1;
            SSPIHandle contextHandle = new SSPIHandle();
            if (refContext != null)
            {
                contextHandle = refContext._handle;
            }

            // these are pinned user byte arrays passed along with SecurityBuffers
            GCHandle[] pinnedInBytes = null;
            GCHandle pinnedOutBytes = new GCHandle();

            // optional output buffer that may need to be freed
            SafeFreeContextBuffer outFreeContextBuffer = null;

            try
            {
                pinnedOutBytes = GCHandle.Alloc(outSecBuffer.token, GCHandleType.Pinned);

                SecurityBufferStruct[] inUnmanagedBuffer = new SecurityBufferStruct[inSecurityBufferDescriptor == null ? 1 : inSecurityBufferDescriptor.Count];
                fixed (void* inUnmanagedBufferPtr = inUnmanagedBuffer)
                {
                    if (inSecurityBufferDescriptor != null)
                    {
                        // Fix Descriptor pointer that points to unmanaged SecurityBuffers
                        inSecurityBufferDescriptor.UnmanagedPointer = inUnmanagedBufferPtr;
                        pinnedInBytes = new GCHandle[inSecurityBufferDescriptor.Count];
                        SecurityBuffer securityBuffer;
                        for (int index = 0; index < inSecurityBufferDescriptor.Count; ++index)
                        {
                            securityBuffer = inSecBuffer != null ? inSecBuffer : inSecBuffers[index];
                            if (securityBuffer != null)
                            {
                                // Copy the SecurityBuffer content into unmanaged place holder
                                inUnmanagedBuffer[index].count = securityBuffer.size;
                                inUnmanagedBuffer[index].type = securityBuffer.type;
                                // use the unmanaged token if it's not null; otherwise use the managed buffer
                                if (securityBuffer.unmanagedToken != null)
                                {
                                    inUnmanagedBuffer[index].token = securityBuffer.unmanagedToken.DangerousGetHandle();
                                }
                                else if (securityBuffer.token == null || securityBuffer.token.Length == 0)
                                {
                                    inUnmanagedBuffer[index].token = IntPtr.Zero;
                                }
                                else
                                {
                                    pinnedInBytes[index] = GCHandle.Alloc(securityBuffer.token, GCHandleType.Pinned);
                                    inUnmanagedBuffer[index].token = Marshal.UnsafeAddrOfPinnedArrayElement(securityBuffer.token, securityBuffer.offset);
                                }
                            }
                        }
                    }
                    SecurityBufferStruct[] outUnmanagedBuffer = new SecurityBufferStruct[1];
                    fixed (void* outUnmanagedBufferPtr = outUnmanagedBuffer)
                    {
                        // Fix Descriptor pointer that points to unmanaged SecurityBuffers
                        outSecurityBufferDescriptor.UnmanagedPointer = outUnmanagedBufferPtr;
                        outUnmanagedBuffer[0].count = outSecBuffer.size;
                        outUnmanagedBuffer[0].type = outSecBuffer.type;
                        if (outSecBuffer.token == null || outSecBuffer.token.Length == 0)
                        {
                            outUnmanagedBuffer[0].token = IntPtr.Zero;
                        }
                        else
                        {
                            outUnmanagedBuffer[0].token = Marshal.UnsafeAddrOfPinnedArrayElement(outSecBuffer.token, outSecBuffer.offset);
                        }
                        if (isSspiAllocated)
                        {
                            outFreeContextBuffer = SafeFreeContextBuffer.CreateEmptyHandle();
                        }
                        if (refContext == null || refContext.IsInvalid)
                        {
                            refContext = new SafeDeleteContext();
                        }
                        if (targetName == null || targetName.Length == 0)
                        {
                            targetName = dummyStr;
                        }
                        fixed (char* namePtr = targetName)
                        {
                            errorCode = MustRunInitializeSecurityContext(
                                inCredentials,
                                contextHandle.IsZero ? null : &contextHandle,
                                (byte*)(((object)targetName == (object)dummyStr) ? null : namePtr),
                                inFlags,
                                endianness,
                                inSecurityBufferDescriptor,
                                refContext,
                                outSecurityBufferDescriptor,
                                ref outFlags,
                                outFreeContextBuffer
                                );
                        }

                        // Get unmanaged buffer with index 0 as the only one passed into PInvoke
                        outSecBuffer.size = outUnmanagedBuffer[0].count;
                        outSecBuffer.type = outUnmanagedBuffer[0].type;
                        if (outSecBuffer.size > 0)
                        {
                            outSecBuffer.token = DiagnosticUtility.Utility.AllocateByteArray(outSecBuffer.size);
                            Marshal.Copy(outUnmanagedBuffer[0].token, outSecBuffer.token, 0, outSecBuffer.size);
                        }
                        else
                        {
                            outSecBuffer.token = null;
                        }
                    }
                }
            }
            finally
            {
                if (pinnedInBytes != null)
                {
                    for (int index = 0; index < pinnedInBytes.Length; index++)
                    {
                        if (pinnedInBytes[index].IsAllocated)
                        {
                            pinnedInBytes[index].Free();
                        }
                    }
                }
                if (pinnedOutBytes.IsAllocated)
                {
                    pinnedOutBytes.Free();
                }
                if (outFreeContextBuffer != null)
                {
                    outFreeContextBuffer.Close();
                }
            }
            return errorCode;
        }

        //
        // After PINvoke call the method will fix the handleTemplate.handle with the returned value.
        // The caller is responsible for creating a correct SafeFreeContextBuffer or null can be passed if no handle is returned.
        //
        // Since it has a CER, this method can't have any references to imports from DLLs that may not exist on the system.
        //
        static unsafe int MustRunInitializeSecurityContext(
            SafeFreeCredentials inCredentials,
            void* inContextPtr,
            byte* targetName,
            SspiContextFlags inFlags,
            Endianness endianness,
            SecurityBufferDescriptor inputBuffer,
            SafeDeleteContext outContext,
            SecurityBufferDescriptor outputBuffer,
            ref SspiContextFlags attributes,
            SafeFreeContextBuffer handleTemplate)
        {
            int errorCode = -1;
            bool b1 = false;
            bool b2 = false;

            // Run the body of this method as a non-interruptible block.
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                inCredentials.DangerousAddRef(ref b1);
                outContext.DangerousAddRef(ref b2);
            }
            catch (Exception e)
            {
                if (System.Runtime.Fx.IsFatal(e))
                    throw;
                
                if (b1)
                {
                    inCredentials.DangerousRelease();
                    b1 = false;
                }
                
                if (b2)
                {
                    outContext.DangerousRelease();
                    b2 = false;
                }

                if (!(e is ObjectDisposedException))
                    throw;
            }
            finally
            {
                long timeStamp;
                if (!b1)
                {
                    // caller should retry
                    inCredentials = null;
                }
                else if (b1 && b2)
                {

                    SSPIHandle credentialHandle = inCredentials._handle;
                    // PreSharp Bug: Call 'Marshal.GetLastWin32Error' or 'Marshal.GetHRForLastWin32Error' before any other interop call. 
#pragma warning suppress 56523 // This API does not set Win32 Last Error.
                    errorCode = InitializeSecurityContextW(
                        ref credentialHandle,
                        inContextPtr,
                        targetName,
                        inFlags,
                        0,
                        endianness,
                        inputBuffer,
                        0,
                        ref outContext._handle,
                        outputBuffer,
                        ref attributes,
                        out timeStamp
                        );
                    //
                    // When a credential handle is first associated with the context we keep credential
                    // ref count bumped up to ensure ordered finalization.
                    // If the credential handle has been changed we de-ref the old one and associate the
                    //  context with the new cred handle but only if the call was successful.
                    if (outContext._EffectiveCredential != inCredentials && (errorCode & 0x80000000) == 0)
                    {
                        // Disassociate the previous credential handle
                        if (outContext._EffectiveCredential != null)
                            outContext._EffectiveCredential.DangerousRelease();
                        outContext._EffectiveCredential = inCredentials;
                    }
                    else
                    {
                        inCredentials.DangerousRelease();
                    }

                    outContext.DangerousRelease();

                    // The idea is that SSPI has allocated a block and filled up outUnmanagedBuffer+8 slot with the pointer.
                    if (handleTemplate != null)
                    {
                        handleTemplate.Set(((SecurityBufferStruct*)outputBuffer.UnmanagedPointer)->token); //ATTN: on 64 BIT that is still +8 cause of 2* c++ unsigned long == 8 bytes
                        if (handleTemplate.IsInvalid)
                        {
                            handleTemplate.SetHandleAsInvalid();
                        }
                    }

                }
                if (inContextPtr == null && (errorCode & 0x80000000) != 0)
                {
                    // an error on the first call, need to set the out handle to invalid value
                    outContext._handle.SetToInvalid();
                }
            }
            return errorCode;
        }

        //-------------------------------------------------------------------
        internal static unsafe int AcceptSecurityContext(
            SafeFreeCredentials inCredentials,
            ref SafeDeleteContext refContext,
            SspiContextFlags inFlags,
            Endianness endianness,
            SecurityBuffer inSecBuffer,
            SecurityBuffer[] inSecBuffers,
            SecurityBuffer outSecBuffer,
            ref SspiContextFlags outFlags)
        {

            if (inCredentials == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("inCredentials");
            }

            SecurityBufferDescriptor inSecurityBufferDescriptor = null;
            if (inSecBuffer != null)
            {
                inSecurityBufferDescriptor = new SecurityBufferDescriptor(1);
            }
            else if (inSecBuffers != null)
            {
                inSecurityBufferDescriptor = new SecurityBufferDescriptor(inSecBuffers.Length);
            }
            SecurityBufferDescriptor outSecurityBufferDescriptor = new SecurityBufferDescriptor(1);

            // actually this is returned in outFlags
            bool isSspiAllocated = (inFlags & SspiContextFlags.AllocateMemory) != 0 ? true : false;

            int errorCode = -1;
            SSPIHandle contextHandle = new SSPIHandle();
            if (refContext != null)
            {
                contextHandle = refContext._handle;
            }

            // these are pinned user byte arrays passed along with SecurityBuffers
            GCHandle[] pinnedInBytes = null;
            GCHandle pinnedOutBytes = new GCHandle();
            // optional output buffer that may need to be freed
            SafeFreeContextBuffer outFreeContextBuffer = null;

            try
            {
                pinnedOutBytes = GCHandle.Alloc(outSecBuffer.token, GCHandleType.Pinned);

                SecurityBufferStruct[] inUnmanagedBuffer = new SecurityBufferStruct[inSecurityBufferDescriptor == null ? 1 : inSecurityBufferDescriptor.Count];
                fixed (void* inUnmanagedBufferPtr = inUnmanagedBuffer)
                {
                    if (inSecurityBufferDescriptor != null)
                    {
                        // Fix Descriptor pointer that points to unmanaged SecurityBuffers
                        inSecurityBufferDescriptor.UnmanagedPointer = inUnmanagedBufferPtr;
                        pinnedInBytes = new GCHandle[inSecurityBufferDescriptor.Count];
                        SecurityBuffer securityBuffer;
                        for (int index = 0; index < inSecurityBufferDescriptor.Count; ++index)
                        {
                            securityBuffer = inSecBuffer != null ? inSecBuffer : inSecBuffers[index];
                            if (securityBuffer != null)
                            {
                                // Copy the SecurityBuffer content into unmanaged place holder
                                inUnmanagedBuffer[index].count = securityBuffer.size;
                                inUnmanagedBuffer[index].type = securityBuffer.type;
                                // use the unmanaged token if it's not null; otherwise use the managed buffer
                                if (securityBuffer.unmanagedToken != null)
                                {
                                    inUnmanagedBuffer[index].token = securityBuffer.unmanagedToken.DangerousGetHandle();
                                }
                                else if (securityBuffer.token == null || securityBuffer.token.Length == 0)
                                {
                                    inUnmanagedBuffer[index].token = IntPtr.Zero;
                                }
                                else
                                {
                                    pinnedInBytes[index] = GCHandle.Alloc(securityBuffer.token, GCHandleType.Pinned);
                                    inUnmanagedBuffer[index].token = Marshal.UnsafeAddrOfPinnedArrayElement(securityBuffer.token, securityBuffer.offset);
                                }
                            }
                        }
                    }
                    SecurityBufferStruct[] outUnmanagedBuffer = new SecurityBufferStruct[1];
                    fixed (void* outUnmanagedBufferPtr = outUnmanagedBuffer)
                    {
                        // Fix Descriptor pointer that points to unmanaged SecurityBuffers
                        outSecurityBufferDescriptor.UnmanagedPointer = outUnmanagedBufferPtr;
                        // Copy the SecurityBuffer content into unmanaged place holder
                        outUnmanagedBuffer[0].count = outSecBuffer.size;
                        outUnmanagedBuffer[0].type = outSecBuffer.type;
                        if (outSecBuffer.token == null || outSecBuffer.token.Length == 0)
                        {
                            outUnmanagedBuffer[0].token = IntPtr.Zero;
                        }
                        else
                        {
                            outUnmanagedBuffer[0].token = Marshal.UnsafeAddrOfPinnedArrayElement(outSecBuffer.token, outSecBuffer.offset);
                        }
                        if (isSspiAllocated)
                        {
                            outFreeContextBuffer = SafeFreeContextBuffer.CreateEmptyHandle();
                        }

                        if (refContext == null || refContext.IsInvalid)
                        {
                            refContext = new SafeDeleteContext();
                        }
                        errorCode = MustRunAcceptSecurityContext(
                            inCredentials,
                            contextHandle.IsZero ? null : &contextHandle,
                            inSecurityBufferDescriptor,
                            inFlags,
                            endianness,
                            refContext,
                            outSecurityBufferDescriptor,
                            ref outFlags,
                            outFreeContextBuffer
                            );

                        // Get unmanaged buffer with index 0 as the only one passed into PInvoke
                        outSecBuffer.size = outUnmanagedBuffer[0].count;
                        outSecBuffer.type = outUnmanagedBuffer[0].type;
                        if (outSecBuffer.size > 0)
                        {
                            outSecBuffer.token = DiagnosticUtility.Utility.AllocateByteArray(outSecBuffer.size);
                            Marshal.Copy(outUnmanagedBuffer[0].token, outSecBuffer.token, 0, outSecBuffer.size);
                        }
                        else
                        {
                            outSecBuffer.token = null;
                        }
                    }
                }
            }
            finally
            {
                if (pinnedInBytes != null)
                {
                    for (int index = 0; index < pinnedInBytes.Length; index++)
                    {
                        if (pinnedInBytes[index].IsAllocated)
                        {
                            pinnedInBytes[index].Free();
                        }
                    }
                }
                if (pinnedOutBytes.IsAllocated)
                {
                    pinnedOutBytes.Free();
                }
                if (outFreeContextBuffer != null)
                {
                    outFreeContextBuffer.Close();
                }
            }

            return errorCode;
        }

        // After PINvoke call the method will fix the handleTemplate.handle with the returned value.
        // The caller is responsible for creating a correct SafeFreeContextBuffer_XXX flavour or null can be passed if no handle is returned.
        // This method is run as non-interruptible.
        static unsafe int MustRunAcceptSecurityContext(
            SafeFreeCredentials inCredentials,
            void* inContextPtr,
            SecurityBufferDescriptor inputBuffer,
            SspiContextFlags inFlags,
            Endianness endianness,
            SafeDeleteContext outContext,
            SecurityBufferDescriptor outputBuffer,
            ref SspiContextFlags outFlags,
            SafeFreeContextBuffer handleTemplate)
        {
            int errorCode = -1;
            bool b1 = false;
            bool b2 = false;

            // Run the body of this method as a non-interruptible block.
            RuntimeHelpers.PrepareConstrainedRegions();

            try
            {
                inCredentials.DangerousAddRef(ref b1);
                outContext.DangerousAddRef(ref b2);
            }
            catch (Exception e)
            {
                if (System.Runtime.Fx.IsFatal(e))
                    throw;
                
                if (b1)
                {
                    inCredentials.DangerousRelease();
                    b1 = false;
                }
                if (b2)
                {
                    outContext.DangerousRelease();
                    b2 = false;
                }
                if (!(e is ObjectDisposedException))
                    throw;
            }
            finally
            {
                long timeStamp;
                if (!b1)
                {
                    // caller should retry
                    inCredentials = null;
                }
                else if (b1 && b2)
                {

                    SSPIHandle credentialHandle = inCredentials._handle;
                    // PreSharp Bug: Call 'Marshal.GetLastWin32Error' or 'Marshal.GetHRForLastWin32Error' before any other interop call. 
#pragma warning suppress 56523 // This API does not set Win32 Last Error.
                    errorCode = AcceptSecurityContext(
                        ref credentialHandle,
                        inContextPtr,
                        inputBuffer,
                        inFlags,
                        endianness,
                        ref outContext._handle,
                        outputBuffer,
                        ref outFlags,
                        out timeStamp
                        );

                    //
                    // When a credential handle is first associated with the context we keep credential
                    // ref count bumped up to ensure ordered finalization.
                    // If the credential handle has been changed we de-ref the old one and associate the
                    //  context with the new cred handle but only if the call was successful.
                    if (outContext._EffectiveCredential != inCredentials && (errorCode & 0x80000000) == 0)
                    {
                        // Disassociate the previous credential handle
                        if (outContext._EffectiveCredential != null)
                            outContext._EffectiveCredential.DangerousRelease();
                        outContext._EffectiveCredential = inCredentials;
                    }
                    else
                    {
                        inCredentials.DangerousRelease();
                    }

                    outContext.DangerousRelease();

                    // The idea is that SSPI has allocated a block and filled up outUnmanagedBuffer+8 slot with the pointer.
                    if (handleTemplate != null)
                    {
                        handleTemplate.Set(((SecurityBufferStruct*)outputBuffer.UnmanagedPointer)->token); //ATTN: on 64 BIT that is still +8 cause of 2* c++ unsigned long == 8 bytes
                        if (handleTemplate.IsInvalid)
                        {
                            handleTemplate.SetHandleAsInvalid();
                        }
                    }
                    if (inContextPtr == null && (errorCode & 0x80000000) != 0)
                    {
                        // an error on the first call, need to set the out handle to invalid value
                        outContext._handle.SetToInvalid();
                    }
                }
            }
            return errorCode;
        }

        public static int ImpersonateSecurityContext(SafeDeleteContext context)
        {
            int status = (int)SecurityStatus.InvalidHandle;
            bool b = false;

            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                context.DangerousAddRef(ref b);
            }
            catch (Exception e)
            {
                if (System.Runtime.Fx.IsFatal(e))
                    throw;
                
                if (b)
                {
                    context.DangerousRelease();
                    b = false;
                }
                if (!(e is ObjectDisposedException))
                    throw;
            }
            finally
            {

                if (b)
                {
                    // PreSharp Bug: Call 'Marshal.GetLastWin32Error' or 'Marshal.GetHRForLastWin32Error' before any other interop call. 
#pragma warning suppress 56523 // The API does not set Win32 Last Error. It returns a error code.
                    status = ImpersonateSecurityContext(ref context._handle);
                    context.DangerousRelease();
                }
            }
            return status;
        }

        public static int EncryptMessage(SafeDeleteContext context, SecurityBufferDescriptor inputOutput, uint sequenceNumber)
        {
            int status = (int)SecurityStatus.InvalidHandle;
            bool b = false;

            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                context.DangerousAddRef(ref b);
            }
            catch (Exception e)
            {
                if (System.Runtime.Fx.IsFatal(e))
                    throw;
                
                if (b)
                {
                    context.DangerousRelease();
                    b = false;
                }
                if (!(e is ObjectDisposedException))
                    throw;
            }
            finally
            {
                if (b)
                {
                    // PreSharp Bug: Call 'Marshal.GetLastWin32Error' or 'Marshal.GetHRForLastWin32Error' before any other interop call. 
#pragma warning suppress 56523 // The API does not set Win32 Last Error. It returns a error code.
                    status = EncryptMessage(ref context._handle, 0, inputOutput, sequenceNumber);
                    context.DangerousRelease();
                }
            }
            return status;
        }

        public unsafe static int DecryptMessage(SafeDeleteContext context, SecurityBufferDescriptor inputOutput, uint sequenceNumber)
        {
            int status = (int)SecurityStatus.InvalidHandle;
            bool b = false;
            uint qop = 0;

            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                context.DangerousAddRef(ref b);
            }
            catch (Exception e)
            {
                if (System.Runtime.Fx.IsFatal(e))
                    throw;
                
                if (b)
                {
                    context.DangerousRelease();
                    b = false;
                }
                if (!(e is ObjectDisposedException))
                    throw;
            }
            finally
            {

                if (b)
                {
#pragma warning suppress 56523 // we don't take any action on the win32 error message.
                    status = DecryptMessage(ref context._handle, inputOutput, sequenceNumber, &qop);
                    context.DangerousRelease();
                }
            }

            const uint SECQOP_WRAP_NO_ENCRYPT = 0x80000001;
            if (status == 0 && qop == SECQOP_WRAP_NO_ENCRYPT)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SspiPayloadNotEncrypted)));
            }

            return status;
        }

        internal int GetSecurityContextToken(out SafeCloseHandle safeHandle)
        {
            int status = (int)SecurityStatus.InvalidHandle;
            bool b = false;

            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                DangerousAddRef(ref b);
            }
            catch (Exception e)
            {
                if (System.Runtime.Fx.IsFatal(e))
                    throw;
                
                if (b)
                {
                    DangerousRelease();
                    b = false;
                }
                if (!(e is ObjectDisposedException))
                    throw;
            }
            finally
            {
                if (b)
                {
                    // PreSharp Bug: Call 'Marshal.GetLastWin32Error' or 'Marshal.GetHRForLastWin32Error' before any other interop call. 
#pragma warning suppress 56523 // The API does not set Win32 Last Error. The API returns a error code.
                    status = QuerySecurityContextToken(ref _handle, out safeHandle);
                    DangerousRelease();
                }
                else
                {
                    safeHandle = new SafeCloseHandle(IntPtr.Zero, false);
                }
            }
            return status;
        }

        protected override bool ReleaseHandle()
        {
            if (this._EffectiveCredential != null)
                this._EffectiveCredential.DangerousRelease();

            // PreSharp Bug: Call 'Marshal.GetLastWin32Error' or 'Marshal.GetHRForLastWin32Error' before any other interop call. 
#pragma warning suppress 56523 // The API does not set Win32 Last Error. It returns a error code.
            return DeleteSecurityContext(ref _handle) == 0;
        }

        [DllImport(SECURITY, ExactSpelling = true, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        extern static int QuerySecurityContextToken(ref SSPIHandle phContext, [Out] out SafeCloseHandle handle);

        [DllImport(SECURITY, ExactSpelling = true, SetLastError = true)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        [ResourceExposure(ResourceScope.None)]
        internal extern static unsafe int InitializeSecurityContextW(
            ref SSPIHandle credentialHandle,
            [In] void* inContextPtr,
            [In] byte* targetName,
            [In] SspiContextFlags inFlags,
            [In] int reservedI,
            [In] Endianness endianness,
            [In] SecurityBufferDescriptor inputBuffer,
            [In] int reservedII,
            ref SSPIHandle outContextPtr,
            [In, Out] SecurityBufferDescriptor outputBuffer,
            [In, Out] ref SspiContextFlags attributes,
            out long timestamp
            );

        [DllImport(SECURITY, ExactSpelling = true, SetLastError = true)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        [ResourceExposure(ResourceScope.None)]
        internal extern static unsafe int AcceptSecurityContext(
            ref SSPIHandle credentialHandle,
            [In] void* inContextPtr,
            [In] SecurityBufferDescriptor inputBuffer,
            [In] SspiContextFlags inFlags,
            [In] Endianness endianness,
            ref SSPIHandle outContextPtr,
            [In, Out] SecurityBufferDescriptor outputBuffer,
            [In, Out] ref SspiContextFlags attributes,
            out long timestamp
            );

        [DllImport(SECURITY, ExactSpelling = true, SetLastError = true)]
        [SuppressUnmanagedCodeSecurity]
        [ResourceExposure(ResourceScope.None)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal extern static int DeleteSecurityContext(
            ref SSPIHandle handlePtr
            );

        [DllImport(SECURITY, ExactSpelling = true, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        internal extern static int ImpersonateSecurityContext(
            ref SSPIHandle handlePtr
            );

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        [DllImport(SECURITY, ExactSpelling = true, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        internal extern static int EncryptMessage(
            ref SSPIHandle contextHandle,
            [In] uint qualityOfProtection,
            [In, Out] SecurityBufferDescriptor inputOutput,
            [In] uint sequenceNumber
            );

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        [DllImport(SECURITY, ExactSpelling = true, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        internal unsafe extern static int DecryptMessage(
            ref SSPIHandle contextHandle,
            [In, Out] SecurityBufferDescriptor inputOutput,
            [In] uint sequenceNumber,
            uint* qualityOfProtection
            );
    }

    class SafeFreeCredentials : SafeHandle
    {
        const string SECURITY = "security.Dll";
        internal SSPIHandle _handle; //should be always used as by ref in PINvokes parameters

        protected SafeFreeCredentials()
            : base(IntPtr.Zero, true)
        {
            _handle = new SSPIHandle();
        }

        //internal static string ToString(SafeFreeCredentials obj)
        //{   return obj == null ? "null" : SSPIHandle.ToString(ref obj._handle); }

        public override bool IsInvalid
        {
            get { return IsClosed || _handle.IsZero; }
        }

        //This method should never be called for this type
        //public new IntPtr DangerousGetHandle()
        //{
        //    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException());
        //}

        public static unsafe int AcquireCredentialsHandle(
            string package,
            CredentialUse intent,
            ref AuthIdentityEx authdata,
            out SafeFreeCredentials outCredential)
        {
            int errorCode = -1;
            long timeStamp;

            outCredential = new SafeFreeCredentials();
            RuntimeHelpers.PrepareConstrainedRegions();
            try { }
            finally
            {
                // PreSharp Bug: Call 'Marshal.GetLastWin32Error' or 'Marshal.GetHRForLastWin32Error' before any other interop call. 
#pragma warning suppress 56523 // The API does not set Win32 Last Error. It returns a error code.
                errorCode = AcquireCredentialsHandleW(
                    null,
                    package,
                    (int)intent,
                    null,
                    ref authdata,
                    null,
                    null,
                    ref outCredential._handle,
                    out timeStamp
                    );
                if (errorCode != 0)
                {
                    outCredential.SetHandleAsInvalid();
                }
            }
            return errorCode;
        }

        public static unsafe int AcquireDefaultCredential(
            string package,
            CredentialUse intent,
            ref AuthIdentityEx authIdentity,
            out SafeFreeCredentials outCredential)
        {
            int errorCode = -1;
            long timeStamp;

            outCredential = new SafeFreeCredentials();
            RuntimeHelpers.PrepareConstrainedRegions();
            try { }
            finally
            {
                // PreSharp Bug: Call 'Marshal.GetLastWin32Error' or 'Marshal.GetHRForLastWin32Error' before any other interop call. 
#pragma warning suppress 56523 // The API does not set Win32 Last Error. It returns a error code.
                errorCode = AcquireCredentialsHandleW(
                                null,
                                package,
                                (int)intent,
                                null,
                                ref authIdentity, // IntPtr.Zero,
                                null,
                                null,
                                ref outCredential._handle,
                                out timeStamp
                                );

                if (errorCode != 0)
                {
                    outCredential.SetHandleAsInvalid();
                }
            }
            return errorCode;
        }

        public static unsafe int AcquireCredentialsHandle(
            string package,
            CredentialUse intent,
            ref SecureCredential authdata,
            out SafeFreeCredentials outCredential)
        {
            int errorCode = -1;
            long timeStamp;

            // If there is a certificate, wrap it into an array
            IntPtr copiedPtr = authdata.certContextArray;
            try
            {
                IntPtr certArrayPtr = new IntPtr(&copiedPtr);
                if (copiedPtr != IntPtr.Zero)
                {
                    authdata.certContextArray = certArrayPtr;
                }

                outCredential = new SafeFreeCredentials();
                RuntimeHelpers.PrepareConstrainedRegions();
                try { }
                finally
                {
                    // PreSharp Bug: Call 'Marshal.GetLastWin32Error' or 'Marshal.GetHRForLastWin32Error' before any other interop call. 
#pragma warning suppress 56523 // The API does not set Win32 Last Error. It returns a error code.
                    errorCode = AcquireCredentialsHandleW(
                                    null,
                                    package,
                                    (int)intent,
                                    null,
                                    ref authdata,
                                    null,
                                    null,
                                    ref outCredential._handle,
                                    out timeStamp
                                    );
                    if (errorCode != 0)
                    {
                        outCredential.SetHandleAsInvalid();
                    }
                }
            }
            finally
            {
                authdata.certContextArray = copiedPtr;
            }

            return errorCode;
        }

        public static unsafe int AcquireCredentialsHandle(
         string package,
         CredentialUse intent,
         ref IntPtr ppAuthIdentity,
         out SafeFreeCredentials outCredential
         )
        {
            int errorCode = -1;
            long timeStamp;
            outCredential = new SafeFreeCredentials();
            RuntimeHelpers.PrepareConstrainedRegions();
            try { }
            finally
            {
                // PreSharp Bug: Call 'Marshal.GetLastWin32Error' or 'Marshal.GetHRForLastWin32Error' before any other interop call. 
#pragma warning suppress 56523 // The API does not set Win32 Last Error. It returns a error code.
                errorCode = AcquireCredentialsHandleW(
                                    null,
                                    package,
                                    (int)intent,
                                    null,
                                    ppAuthIdentity,
                                    null,
                                    null,
                                    ref outCredential._handle,
                                    out timeStamp
                                    );
                if (errorCode != 0)
                {
                    outCredential.SetHandleAsInvalid();
                }
            }

            return errorCode;
        }

        protected override bool ReleaseHandle()
        {
            // PreSharp Bug: Call 'Marshal.GetLastWin32Error' or 'Marshal.GetHRForLastWin32Error' before any other interop call. 
#pragma warning suppress 56523 // The API does not set Win32 Last Error. It returns a error code.
            return FreeCredentialsHandle(ref _handle) == 0;
        }

        [ResourceExposure(ResourceScope.None)]
        [DllImport(SECURITY, ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
        internal extern static unsafe int AcquireCredentialsHandleW(
            [In] string principal,
            [In] string moduleName,
            [In] int usage,
            [In] void* logonID,
            [In] ref AuthIdentityEx authdata,
            [In] void* keyCallback,
            [In] void* keyArgument,
            ref SSPIHandle handlePtr,
            [Out] out long timeStamp
            );

        [ResourceExposure(ResourceScope.None)]
        [DllImport(SECURITY, ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
        internal extern static unsafe int AcquireCredentialsHandleW(
            [In] string principal,
            [In] string moduleName,
            [In] int usage,
            [In] void* logonID,
            [In] IntPtr zero,
            [In] void* keyCallback,
            [In] void* keyArgument,
            ref SSPIHandle handlePtr,
            [Out] out long timeStamp
            );

        [ResourceExposure(ResourceScope.None)]
        [DllImport(SECURITY, ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
        internal extern static unsafe int AcquireCredentialsHandleW(
            [In] string principal,
            [In] string moduleName,
            [In] int usage,
            [In] void* logonID,
            [In] ref SecureCredential authData,
            [In] void* keyCallback,
            [In] void* keyArgument,
            ref SSPIHandle handlePtr,
            [Out] out long timeStamp
            );


        [DllImport(SECURITY, ExactSpelling = true, SetLastError = true)]
        [SuppressUnmanagedCodeSecurity]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [ResourceExposure(ResourceScope.None)]
        internal extern static int FreeCredentialsHandle(
            ref SSPIHandle handlePtr
            );
    }

    sealed class SafeFreeCertContext : SafeHandleZeroOrMinusOneIsInvalid
    {
        const string CRYPT32 = "crypt32.dll";
        const string ADVAPI32 = "advapi32.dll";

        internal SafeFreeCertContext() : base(true) { }

        // This must be ONLY called from this file and form a MustRun method
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal unsafe void Set(IntPtr value)
        {
            this.handle = value;
        }

        const uint CRYPT_ACQUIRE_SILENT_FLAG = 0x00000040;

        [DllImport(CRYPT32, ExactSpelling = true, SetLastError = true)]
        [SuppressUnmanagedCodeSecurity]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [ResourceExposure(ResourceScope.None)]
        extern static bool CertFreeCertificateContext(// Suppressing returned status check, it's always==TRUE,
            [In] IntPtr certContext);

        protected override bool ReleaseHandle()
        {
            // PreSharp Bug: Call 'Marshal.GetLastWin32Error' or 'Marshal.GetHRForLastWin32Error' before any other interop call. 
#pragma warning suppress 56523 // The API does not set Win32 Last Error.
            return CertFreeCertificateContext(handle);
        }
    }

    sealed class SafeFreeContextBuffer : SafeHandleZeroOrMinusOneIsInvalid
    {
        const string SECURITY = "security.dll";

        SafeFreeContextBuffer() : base(true) { }

        // This must be ONLY called from this file and form a MustRun method
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal unsafe void Set(IntPtr value)
        {
            this.handle = value;
        }

        internal static int EnumeratePackages(out int pkgnum, out SafeFreeContextBuffer pkgArray)
        {
            int res = -1;
            // PreSharp Bug: Call 'Marshal.GetLastWin32Error' or 'Marshal.GetHRForLastWin32Error' before any other interop call. 
#pragma warning suppress 56523 // The API does not set Win32 Last Error. The API returns a error code.
            res = SafeFreeContextBuffer.EnumerateSecurityPackagesW(out pkgnum, out pkgArray);

            if (res != 0)
            {
                Utility.CloseInvalidOutSafeHandle(pkgArray);
                pkgArray = null;
            }
            return res;
        }

        internal static SafeFreeContextBuffer CreateEmptyHandle()
        {
            return new SafeFreeContextBuffer();
        }

        //
        // After PInvoke call the method will fix the refHandle.handle with the returned value.
        // The caller is responsible for creating a correct SafeHandle template or null can be passed if no handle is returned.
        //
        // This method is run as non-interruptible.
        //
        public static unsafe int QueryContextAttributes(SafeDeleteContext phContext, ContextAttribute contextAttribute, byte* buffer, SafeHandle refHandle)
        {
            int status = (int)SecurityStatus.InvalidHandle;
            bool b = false;

            // We don't want to be interrupted by thread abort exceptions or unexpected out-of-memory errors failing to jit
            // one of the following methods. So run within a CER non-interruptible block.
            RuntimeHelpers.PrepareConstrainedRegions();

            try
            {
                phContext.DangerousAddRef(ref b);
            }
            catch (Exception e)
            {
                if (System.Runtime.Fx.IsFatal(e))
                    throw;
                
                if (b)
                {
                    phContext.DangerousRelease();
                    b = false;
                }
                if (!(e is ObjectDisposedException))
                    throw;
            }
            finally
            {
                if (b)
                {
                    // PreSharp Bug: Call 'Marshal.GetLastWin32Error' or 'Marshal.GetHRForLastWin32Error' before any other interop call. 
#pragma warning suppress 56523 // The API does not set Win32 Last Error. The API returns a error code.
                    status = SafeFreeContextBuffer.QueryContextAttributesW(ref phContext._handle, contextAttribute, buffer);
                    phContext.DangerousRelease();
                }
                if (status == 0 && refHandle != null)
                {
                    if (refHandle is SafeFreeContextBuffer)
                    {
                        if (contextAttribute == ContextAttribute.SessionKey)
                        {
                            IntPtr keyPtr = Marshal.ReadIntPtr(new IntPtr(buffer), SecPkgContext_SessionKey.SessionkeyOffset);
                            ((SafeFreeContextBuffer)refHandle).Set(keyPtr);
                        }
                        else
                        {
                            ((SafeFreeContextBuffer)refHandle).Set(*(IntPtr*)buffer);
                        }
                    }
                    else
                    {
                        ((SafeFreeCertContext)refHandle).Set(*(IntPtr*)buffer);
                    }
                }

                if (status != 0 && refHandle != null)
                {
                    refHandle.SetHandleAsInvalid();
                }
            }
            return status;
        }

        protected override bool ReleaseHandle()
        {
            // PreSharp Bug: Call 'Marshal.GetLastWin32Error' or 'Marshal.GetHRForLastWin32Error' before any other interop call. 
#pragma warning suppress 56523 // The API does not set Win32 Last Error. The API returns a error code.
            return FreeContextBuffer(handle) == 0;
        }

        [DllImport(SECURITY, ExactSpelling = true, SetLastError = true)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        [ResourceExposure(ResourceScope.None)]
        internal extern static unsafe int QueryContextAttributesW(
            ref SSPIHandle contextHandle,
            [In] ContextAttribute attribute,
            [In] void* buffer);

        [DllImport(SECURITY, ExactSpelling = true, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        internal extern static int EnumerateSecurityPackagesW(
            [Out] out int pkgnum,
            [Out] out SafeFreeContextBuffer handle);

        [DllImport(SECURITY, ExactSpelling = true, SetLastError = true)]
        [SuppressUnmanagedCodeSecurity]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [ResourceExposure(ResourceScope.None)]
        extern static int FreeContextBuffer(
            [In] IntPtr contextBuffer);
    }

    sealed class SafeCloseHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        const string KERNEL32 = "kernel32.dll";

        SafeCloseHandle() : base(true) { }
        internal SafeCloseHandle(IntPtr handle, bool ownsHandle)
            : base(ownsHandle)
        {
            DiagnosticUtility.DebugAssert(handle == IntPtr.Zero || !ownsHandle, "Unsafe to create a SafeHandle that owns a pre-existing handle before the SafeHandle was created.");
            SetHandle(handle);
        }

        protected override bool ReleaseHandle()
        {
            // PreSharp Bug: Call 'Marshal.GetLastWin32Error' or 'Marshal.GetHRForLastWin32Error' before any other interop call. 
#pragma warning suppress 56523 // We are not interested to throw an exception here. We can ignore the Last Error code.
            return CloseHandle(handle);
        }

        [DllImport(KERNEL32, ExactSpelling = true, SetLastError = true)]
        [SuppressUnmanagedCodeSecurity]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [ResourceExposure(ResourceScope.None)]
        extern static bool CloseHandle(IntPtr handle);
    }

#pragma warning disable 618 // have not moved to the v4 security model yet
    [SecurityCritical(SecurityCriticalScope.Everything)]
#pragma warning restore 618
    sealed class SafeHGlobalHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        SafeHGlobalHandle() : base(true) { }

        // 0 is an Invalid Handle
        SafeHGlobalHandle(IntPtr handle)
            : base(true)
        {
            DiagnosticUtility.DebugAssert(handle == IntPtr.Zero, "SafeHGlobalHandle constructor can only be called with IntPtr.Zero.");
            SetHandle(handle);
        }

        protected override bool ReleaseHandle()
        {
            Marshal.FreeHGlobal(handle);
            return true;
        }

        public static SafeHGlobalHandle InvalidHandle
        {
            get { return new SafeHGlobalHandle(IntPtr.Zero); }
        }

        public static SafeHGlobalHandle AllocHGlobal(string s)
        {
            byte[] bytes = DiagnosticUtility.Utility.AllocateByteArray(checked((s.Length + 1) * 2));
            Encoding.Unicode.GetBytes(s, 0, s.Length, bytes, 0);
            return AllocHGlobal(bytes);
        }

        public static SafeHGlobalHandle AllocHGlobal(byte[] bytes)
        {
            SafeHGlobalHandle result = AllocHGlobal(bytes.Length);
            Marshal.Copy(bytes, 0, result.DangerousGetHandle(), bytes.Length);
            return result;
        }

        public static SafeHGlobalHandle AllocHGlobal(uint cb)
        {
            // The cast could overflow to minus.
            // Unfortunately, Marshal.AllocHGlobal only takes int.
            return AllocHGlobal((int)cb);
        }

        public static SafeHGlobalHandle AllocHGlobal(int cb)
        {
            if (cb < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("cb", SR.GetString(SR.ValueMustBeNonNegative)));
            }

            SafeHGlobalHandle result = new SafeHGlobalHandle();

            // CER 
            RuntimeHelpers.PrepareConstrainedRegions();
            try { }
            finally
            {
                IntPtr ptr = Marshal.AllocHGlobal(cb);
                result.SetHandle(ptr);
            }
            return result;
        }
    }

    sealed class SafeLsaLogonProcessHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        SafeLsaLogonProcessHandle() : base(true) { }

        // 0 is an Invalid Handle
        internal SafeLsaLogonProcessHandle(IntPtr handle)
            : base(true)
        {
            SetHandle(handle);
        }

        internal static SafeLsaLogonProcessHandle InvalidHandle
        {
            get { return new SafeLsaLogonProcessHandle(IntPtr.Zero); }
        }

        override protected bool ReleaseHandle()
        {
            // LsaDeregisterLogonProcess returns an NTSTATUS
            return NativeMethods.LsaDeregisterLogonProcess(handle) >= 0;
        }
    }

    sealed class SafeLsaReturnBufferHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        SafeLsaReturnBufferHandle() : base(true) { }

        // 0 is an Invalid Handle
        internal SafeLsaReturnBufferHandle(IntPtr handle)
            : base(true)
        {
            SetHandle(handle);
        }

        internal static SafeLsaReturnBufferHandle InvalidHandle
        {
            get { return new SafeLsaReturnBufferHandle(IntPtr.Zero); }
        }

        override protected bool ReleaseHandle()
        {
            // LsaFreeReturnBuffer returns an NTSTATUS
            return NativeMethods.LsaFreeReturnBuffer(handle) >= 0;
        }
    }
}
