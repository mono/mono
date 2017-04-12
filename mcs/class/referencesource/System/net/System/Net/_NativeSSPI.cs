//------------------------------------------------------------------------------
// <copyright file="_NativeSSPI.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net {
    using System.Net.Sockets;
    using System.Runtime.InteropServices;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Globalization;

    // need a global so we can pass the interfaces as variables,
    // is there a better way?
    internal static class GlobalSSPI {
        internal static SSPIInterface SSPIAuth = new SSPIAuthType();
        internal static SSPIInterface SSPISecureChannel = new SSPISecureChannelType();
    }

    // used to define the interface for security to use.
    internal interface SSPIInterface {
        SecurityPackageInfoClass[] SecurityPackages { get; set; }
        int EnumerateSecurityPackages(out int pkgnum, out SafeFreeContextBuffer pkgArray);
        int AcquireCredentialsHandle(string moduleName, CredentialUse usage, ref AuthIdentity authdata, out SafeFreeCredentials outCredential);
        int AcquireCredentialsHandle(string moduleName, CredentialUse usage, ref SafeSspiAuthDataHandle authdata, out SafeFreeCredentials outCredential);
        int AcquireDefaultCredential(string moduleName, CredentialUse usage, out SafeFreeCredentials outCredential);
        int AcquireCredentialsHandle(string moduleName, CredentialUse usage, ref SecureCredential authdata, out SafeFreeCredentials outCredential);
        int AcceptSecurityContext(ref SafeFreeCredentials credential, ref SafeDeleteContext context, SecurityBuffer inputBuffer, ContextFlags inFlags, Endianness endianness, SecurityBuffer outputBuffer, ref ContextFlags outFlags);
        int AcceptSecurityContext(SafeFreeCredentials credential, ref SafeDeleteContext context, SecurityBuffer[] inputBuffers, ContextFlags inFlags, Endianness endianness, SecurityBuffer outputBuffer, ref ContextFlags outFlags);
        int InitializeSecurityContext(ref SafeFreeCredentials credential, ref SafeDeleteContext context, string targetName, ContextFlags inFlags, Endianness endianness, SecurityBuffer inputBuffer, SecurityBuffer outputBuffer, ref ContextFlags outFlags);
        int InitializeSecurityContext(SafeFreeCredentials credential, ref SafeDeleteContext context, string targetName, ContextFlags inFlags, Endianness endianness, SecurityBuffer[] inputBuffers, SecurityBuffer outputBuffer, ref ContextFlags outFlags);
        int EncryptMessage(SafeDeleteContext context, SecurityBufferDescriptor inputOutput, uint sequenceNumber);
        int DecryptMessage(SafeDeleteContext context, SecurityBufferDescriptor inputOutput, uint sequenceNumber);
        int MakeSignature(SafeDeleteContext context, SecurityBufferDescriptor inputOutput, uint sequenceNumber);
        int VerifySignature(SafeDeleteContext context, SecurityBufferDescriptor inputOutput, uint sequenceNumber);

        int QueryContextChannelBinding(SafeDeleteContext phContext, ContextAttribute attribute, out SafeFreeContextBufferChannelBinding refHandle);
        int QueryContextAttributes(SafeDeleteContext phContext, ContextAttribute  attribute, byte[] buffer, Type handleType, out SafeHandle refHandle);
        int SetContextAttributes(SafeDeleteContext phContext, ContextAttribute attribute, byte[] buffer);
        int QuerySecurityContextToken(SafeDeleteContext phContext, out SafeCloseHandle phToken);
        int CompleteAuthToken(ref SafeDeleteContext refContext, SecurityBuffer[] inputBuffers);
        int ApplyControlToken(ref SafeDeleteContext refContext, SecurityBuffer[] inputBuffers);
    }

    // For SSL connections:
    internal class SSPISecureChannelType : SSPIInterface {
        private static readonly SecurDll Library = SecurDll.SECURITY;
        private static volatile SecurityPackageInfoClass[] m_SecurityPackages;

        public SecurityPackageInfoClass[] SecurityPackages {
            get {
                return m_SecurityPackages;
            }
            set {
                m_SecurityPackages = value;
            }
        }

        public int EnumerateSecurityPackages(out int pkgnum, out SafeFreeContextBuffer pkgArray) {
            GlobalLog.Print("SSPISecureChannelType::EnumerateSecurityPackages()");
            return SafeFreeContextBuffer.EnumeratePackages(Library, out pkgnum, out pkgArray);
        }

        public int AcquireCredentialsHandle(string moduleName, CredentialUse usage, ref AuthIdentity authdata, out SafeFreeCredentials outCredential) {
            return SafeFreeCredentials.AcquireCredentialsHandle(Library, moduleName, usage, ref authdata, out outCredential);
        }

        public int AcquireCredentialsHandle(string moduleName, CredentialUse usage, ref SafeSspiAuthDataHandle authdata, out SafeFreeCredentials outCredential) {
            return SafeFreeCredentials.AcquireCredentialsHandle(moduleName, usage, ref authdata, out outCredential);
        }

        public int AcquireDefaultCredential(string moduleName, CredentialUse usage, out SafeFreeCredentials outCredential) {
            return SafeFreeCredentials.AcquireDefaultCredential(Library, moduleName, usage, out outCredential);
        }

        public int AcquireCredentialsHandle(string moduleName, CredentialUse usage, ref SecureCredential authdata, out SafeFreeCredentials outCredential) {
            return SafeFreeCredentials.AcquireCredentialsHandle(Library, moduleName, usage, ref authdata, out outCredential);
        }

        public int AcceptSecurityContext(ref SafeFreeCredentials credential, ref SafeDeleteContext context, SecurityBuffer inputBuffer, ContextFlags inFlags, Endianness endianness, SecurityBuffer outputBuffer, ref ContextFlags outFlags) {
            return SafeDeleteContext.AcceptSecurityContext(Library, ref credential, ref context, inFlags, endianness, inputBuffer, null, outputBuffer, ref outFlags);
        }

        public int AcceptSecurityContext(SafeFreeCredentials credential, ref SafeDeleteContext context, SecurityBuffer[] inputBuffers, ContextFlags inFlags, Endianness endianness, SecurityBuffer outputBuffer, ref ContextFlags outFlags) {
            return SafeDeleteContext.AcceptSecurityContext(Library, ref credential, ref context, inFlags, endianness, null, inputBuffers, outputBuffer, ref outFlags);
        }

        public int InitializeSecurityContext(ref SafeFreeCredentials credential, ref SafeDeleteContext context, string targetName, ContextFlags inFlags, Endianness endianness, SecurityBuffer inputBuffer, SecurityBuffer outputBuffer, ref ContextFlags outFlags) {
            return SafeDeleteContext.InitializeSecurityContext(Library, ref credential, ref context, targetName, inFlags, endianness, inputBuffer, null, outputBuffer, ref outFlags);
        }

        public int InitializeSecurityContext(SafeFreeCredentials credential, ref SafeDeleteContext context, string targetName, ContextFlags inFlags, Endianness endianness, SecurityBuffer[] inputBuffers, SecurityBuffer outputBuffer, ref ContextFlags outFlags) {
            return SafeDeleteContext.InitializeSecurityContext(Library, ref credential, ref context, targetName, inFlags, endianness, null, inputBuffers, outputBuffer, ref outFlags);
        }

        public int EncryptMessage(SafeDeleteContext context, SecurityBufferDescriptor inputOutput, uint sequenceNumber)
        {
            int status = (int)SecurityStatus.InvalidHandle;
            bool b = false;

            RuntimeHelpers.PrepareConstrainedRegions();
            try {
                context.DangerousAddRef(ref b);
            }
            catch(Exception e) {
                if (b)
                {
                    context.DangerousRelease();
                    b = false;
                }
                if (!(e is ObjectDisposedException))
                    throw;
            }
            finally {

                if (b)
                {
                    status = UnsafeNclNativeMethods.NativeNTSSPI.EncryptMessage(ref context._handle, 0, inputOutput, sequenceNumber);
                    context.DangerousRelease();
                }
            }
            return status;
        }

        public unsafe int DecryptMessage(SafeDeleteContext context, SecurityBufferDescriptor inputOutput, uint sequenceNumber) {
            int status = (int)SecurityStatus.InvalidHandle;
            bool b = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try {
                context.DangerousAddRef(ref b);
            }
            catch(Exception e) {
                if (b)
                {
                    context.DangerousRelease();
                    b = false;
                }
                if (!(e is ObjectDisposedException))
                    throw;
            }
            finally {

                if (b)
                {
                    status = UnsafeNclNativeMethods.NativeNTSSPI.DecryptMessage(ref context._handle, inputOutput, sequenceNumber, null);
                    context.DangerousRelease();
                }
            }
            return status;
        }

        public int MakeSignature(SafeDeleteContext context, SecurityBufferDescriptor inputOutput, uint sequenceNumber) {
            throw ExceptionHelper.MethodNotSupportedException;
        }

        public int VerifySignature(SafeDeleteContext context, SecurityBufferDescriptor inputOutput, uint sequenceNumber) {
            throw ExceptionHelper.MethodNotSupportedException;
        }

        public unsafe int QueryContextChannelBinding(SafeDeleteContext phContext, ContextAttribute attribute, out SafeFreeContextBufferChannelBinding refHandle)
        {
            refHandle = SafeFreeContextBufferChannelBinding.CreateEmptyHandle(Library);

            // bindings is on the stack, so there's no need for a fixed block
            Bindings bindings = new Bindings();
            return SafeFreeContextBufferChannelBinding.QueryContextChannelBinding(Library, phContext, attribute, &bindings, refHandle);
        }

        public unsafe int QueryContextAttributes(SafeDeleteContext phContext, ContextAttribute  attribute, byte[] buffer, Type handleType, out SafeHandle refHandle) {

            refHandle = null;
            if (handleType != null) {
                if (handleType == typeof(SafeFreeContextBuffer)) {
                    refHandle = SafeFreeContextBuffer.CreateEmptyHandle(Library);
                }
                else if (handleType == typeof(SafeFreeCertContext)) {
                    refHandle = new SafeFreeCertContext();
                }
                else {
                    throw new ArgumentException(SR.GetString(SR.SSPIInvalidHandleType, handleType.FullName), "handleType");
                }
            }
            fixed (byte* bufferPtr = buffer) {
                return SafeFreeContextBuffer.QueryContextAttributes(Library, phContext, attribute, bufferPtr, refHandle);
            }
        }

        public int SetContextAttributes(SafeDeleteContext phContext, ContextAttribute attribute, byte[] buffer) {
            return SafeFreeContextBuffer.SetContextAttributes(Library, phContext, attribute, buffer);
        }

        public int QuerySecurityContextToken(SafeDeleteContext phContext, out SafeCloseHandle phToken) {
            throw new NotSupportedException();
        }

        public int CompleteAuthToken(ref SafeDeleteContext refContext, SecurityBuffer[] inputBuffers) {
            throw new NotSupportedException();
        }

        public int ApplyControlToken(ref SafeDeleteContext refContext, SecurityBuffer[] inputBuffers)
        {
            return SafeDeleteContext.ApplyControlToken(Library, ref refContext, inputBuffers);
        }
    }


    // For Authentication (Kerberos, NTLM, Negotiate and WDigest):
    internal class SSPIAuthType : SSPIInterface {
        private static readonly SecurDll Library = SecurDll.SECURITY;
        private static volatile SecurityPackageInfoClass[] m_SecurityPackages;

        public SecurityPackageInfoClass[] SecurityPackages {
            get {
                return m_SecurityPackages;
            }
            set {
                m_SecurityPackages = value;
            }
        }

        public int EnumerateSecurityPackages(out int pkgnum, out SafeFreeContextBuffer pkgArray) {
            GlobalLog.Print("SSPIAuthType::EnumerateSecurityPackages()");
            return SafeFreeContextBuffer.EnumeratePackages(Library, out pkgnum, out pkgArray);
        }

        public int AcquireCredentialsHandle(string moduleName, CredentialUse usage, ref AuthIdentity authdata, out SafeFreeCredentials outCredential) {
            return SafeFreeCredentials.AcquireCredentialsHandle(Library, moduleName, usage, ref authdata, out outCredential);
        }

        public int AcquireCredentialsHandle(string moduleName, CredentialUse usage, ref SafeSspiAuthDataHandle authdata, out SafeFreeCredentials outCredential) {
            return SafeFreeCredentials.AcquireCredentialsHandle(moduleName, usage, ref authdata, out outCredential);
        }

        public int AcquireDefaultCredential(string moduleName, CredentialUse usage, out SafeFreeCredentials outCredential) {
            return SafeFreeCredentials.AcquireDefaultCredential(Library, moduleName, usage, out outCredential);
        }

        public int AcquireCredentialsHandle(string moduleName, CredentialUse usage, ref SecureCredential authdata, out SafeFreeCredentials outCredential) {
            return SafeFreeCredentials.AcquireCredentialsHandle(Library, moduleName, usage, ref authdata, out outCredential);
        }

        public int AcceptSecurityContext(ref SafeFreeCredentials credential, ref SafeDeleteContext context, SecurityBuffer inputBuffer, ContextFlags inFlags, Endianness endianness, SecurityBuffer outputBuffer, ref ContextFlags outFlags) {
            return SafeDeleteContext.AcceptSecurityContext(Library, ref credential, ref context, inFlags, endianness, inputBuffer, null, outputBuffer, ref outFlags);
        }

        public int AcceptSecurityContext(SafeFreeCredentials credential, ref SafeDeleteContext context, SecurityBuffer[] inputBuffers, ContextFlags inFlags, Endianness endianness, SecurityBuffer outputBuffer, ref ContextFlags outFlags) {
            return SafeDeleteContext.AcceptSecurityContext(Library, ref credential, ref context, inFlags, endianness, null, inputBuffers, outputBuffer, ref outFlags);
        }

        public int InitializeSecurityContext(ref SafeFreeCredentials credential, ref SafeDeleteContext context, string targetName, ContextFlags inFlags, Endianness endianness, SecurityBuffer inputBuffer, SecurityBuffer outputBuffer, ref ContextFlags outFlags) {
            return SafeDeleteContext.InitializeSecurityContext(Library, ref credential, ref context, targetName, inFlags, endianness, inputBuffer, null, outputBuffer, ref outFlags);
        }

        public int InitializeSecurityContext(SafeFreeCredentials credential, ref SafeDeleteContext context, string targetName, ContextFlags inFlags, Endianness endianness, SecurityBuffer[] inputBuffers, SecurityBuffer outputBuffer, ref ContextFlags outFlags) {
            return SafeDeleteContext.InitializeSecurityContext(Library, ref credential, ref context, targetName, inFlags, endianness, null, inputBuffers, outputBuffer, ref outFlags);
        }


        public int EncryptMessage(SafeDeleteContext context, SecurityBufferDescriptor inputOutput, uint sequenceNumber) {
            int status = (int)SecurityStatus.InvalidHandle;
            bool b = false;

            RuntimeHelpers.PrepareConstrainedRegions();
            try {
                context.DangerousAddRef(ref b);
            }
            catch(Exception e) {
                if (b)
                {
                    context.DangerousRelease();
                    b = false;
                }
                if (!(e is ObjectDisposedException))
                    throw;
            }
            finally {

                if (b)
                {
                    status = UnsafeNclNativeMethods.NativeNTSSPI.EncryptMessage(ref context._handle, 0, inputOutput, sequenceNumber);
                    context.DangerousRelease();
                }
            }
            return status;
        }

        public unsafe int DecryptMessage(SafeDeleteContext context, SecurityBufferDescriptor inputOutput, uint sequenceNumber) {
            int status = (int)SecurityStatus.InvalidHandle;
            bool b = false;
            uint qop = 0;

            RuntimeHelpers.PrepareConstrainedRegions();
            try {
                context.DangerousAddRef(ref b);
            }
            catch(Exception e) {
                if (b)
                {
                    context.DangerousRelease();
                    b = false;
                }
                if (!(e is ObjectDisposedException))
                    throw;
            }
            finally {

                if (b)
                {
                    status = UnsafeNclNativeMethods.NativeNTSSPI.DecryptMessage(ref context._handle, inputOutput, sequenceNumber, &qop);
                    context.DangerousRelease();
                }
            }

            const uint SECQOP_WRAP_NO_ENCRYPT = 0x80000001;
            if (status == 0 && qop == SECQOP_WRAP_NO_ENCRYPT)
            {
                GlobalLog.Assert("NativeNTSSPI.DecryptMessage", "Expected qop = 0, returned value = " + qop.ToString("x", CultureInfo.InvariantCulture));
                throw new InvalidOperationException(SR.GetString(SR.net_auth_message_not_encrypted));
            }


            return status;
        }

        public int MakeSignature(SafeDeleteContext context, SecurityBufferDescriptor inputOutput, uint sequenceNumber) {
            int status = (int)SecurityStatus.InvalidHandle;
            bool b = false;

            RuntimeHelpers.PrepareConstrainedRegions();
            try {
                context.DangerousAddRef(ref b);
            }
            catch(Exception e) {
                if (b)
                {
                    context.DangerousRelease();
                    b = false;
                }
                if (!(e is ObjectDisposedException))
                    throw;
            }
            finally {

                if (b)
                {
                    const uint SECQOP_WRAP_NO_ENCRYPT = 0x80000001;
                    status = UnsafeNclNativeMethods.NativeNTSSPI.EncryptMessage(ref context._handle, SECQOP_WRAP_NO_ENCRYPT, inputOutput, sequenceNumber);
                    context.DangerousRelease();
                }
            }
            return status;
        }

        public unsafe int VerifySignature(SafeDeleteContext context, SecurityBufferDescriptor inputOutput, uint sequenceNumber) {
            int status = (int)SecurityStatus.InvalidHandle;
            bool b = false;

            uint qop = 0;
            RuntimeHelpers.PrepareConstrainedRegions();
            try {
                context.DangerousAddRef(ref b);
            }
            catch(Exception e) {
                if (b)
                {
                    context.DangerousRelease();
                    b = false;
                }
                if (!(e is ObjectDisposedException))
                    throw;
            }
            finally {

                if (b)
                {
                    status = UnsafeNclNativeMethods.NativeNTSSPI.DecryptMessage(ref context._handle, inputOutput, sequenceNumber, &qop);
                    context.DangerousRelease();
                }
            }

            return status;

        }

        public int QueryContextChannelBinding(SafeDeleteContext context, ContextAttribute attribute, out SafeFreeContextBufferChannelBinding binding)
        {
            // Querying an auth SSP for a CBT doesn't make sense
            binding = null;
            throw new NotSupportedException();
        }

        public unsafe int QueryContextAttributes(SafeDeleteContext context, ContextAttribute  attribute, byte[] buffer, Type handleType, out SafeHandle refHandle) {

            refHandle = null;
            if (handleType != null) {
                if (handleType == typeof(SafeFreeContextBuffer)) {
                    refHandle = SafeFreeContextBuffer.CreateEmptyHandle(Library);
                }
                else if (handleType == typeof(SafeFreeCertContext)) {
                    refHandle = new SafeFreeCertContext();
                }
                else {
                    throw new ArgumentException(SR.GetString(SR.SSPIInvalidHandleType, handleType.FullName), "handleType");
                }
            }

            fixed (byte* bufferPtr = buffer) {
                return SafeFreeContextBuffer.QueryContextAttributes(Library, context, attribute, bufferPtr, refHandle);
            }
        }

        public int SetContextAttributes(SafeDeleteContext context, ContextAttribute  attribute, byte[] buffer) {
            throw new NotImplementedException();
        }

        public int QuerySecurityContextToken(SafeDeleteContext phContext, out SafeCloseHandle phToken) {
            return GetSecurityContextToken(phContext, out phToken);
        }

        public int CompleteAuthToken(ref SafeDeleteContext refContext, SecurityBuffer[] inputBuffers) {
            return SafeDeleteContext.CompleteAuthToken(Library, ref refContext, inputBuffers);
        }

        public int ApplyControlToken(ref SafeDeleteContext refContext, SecurityBuffer[] inputBuffers)
        {
            throw new NotSupportedException();
        }

        private static int GetSecurityContextToken(SafeDeleteContext phContext, out SafeCloseHandle safeHandle) {

            int status = (int)SecurityStatus.InvalidHandle;
            bool b = false;
            safeHandle = null;

            RuntimeHelpers.PrepareConstrainedRegions();
            try {
                phContext.DangerousAddRef(ref b);
            }
            catch (Exception e) {
                if (b) {
                    phContext.DangerousRelease();
                    b = false;
                }
                if (!(e is ObjectDisposedException))
                    throw;
            }
            finally {
                if (b) {
                    status = UnsafeNclNativeMethods.SafeNetHandles.QuerySecurityContextToken(ref phContext._handle, out safeHandle);
                    phContext.DangerousRelease();
                }
            }

            return status;
        }
    }

}
