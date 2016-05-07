//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.IdentityModel
{
    using System.Runtime.InteropServices;
    using System.Diagnostics;
    using System.ComponentModel;
    using System.Security.Permissions;
    using System.Globalization;
    using System.Security.Authentication.ExtendedProtection;
    using System.Net;
    using System.Runtime.CompilerServices;


    //From Schannel.h
    internal enum SchProtocols
    {
        Zero = 0,
        Ssl2Client = 0x00000008,
        Ssl2Server = 0x00000004,
        Ssl2 = (Ssl2Client | Ssl2Server),
        Ssl3Client = 0x00000020,
        Ssl3Server = 0x00000010,
        Ssl3 = (Ssl3Client | Ssl3Server),
        TlsClient = 0x00000080,
        TlsServer = 0x00000040,
        Tls = (TlsClient | TlsServer),
        Ssl3Tls = (Ssl3 | Tls),
    };

    //From WinCrypt.h
    internal enum Alg
    {
        Any = 0,
        ClassSignture = (1 << 13),
        ClassEncrypt = (3 << 13),
        ClassHash = (4 << 13),
        ClassKeyXch = (5 << 13),
        TypeRSA = (2 << 9),
        TypeBlock = (3 << 9),
        TypeStream = (4 << 9),
        TypeDH = (5 << 9),

        NameDES = 1,
        NameRC2 = 2,
        NameRC4 = 1,
        NameSkipJack = 10,

        // want to ensure MD5 is never used
        // NameMD5         = 3,
        NameSHA = 4,

        NameDH_Ephem = 2,
        Fortezza = 4
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct SecurityPackageInfo
    {
        // see SecPkgInfoW in <sspi.h>
        internal int Capabilities;
        internal short Version;
        internal short RPCID;
        internal int MaxToken;
        internal IntPtr Name;
        internal IntPtr Comment;

        internal static readonly int Size = Marshal.SizeOf(typeof(SecurityPackageInfo));
        internal static readonly int NameOffest = (int)Marshal.OffsetOf(typeof(SecurityPackageInfo), "Name");
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct LifeSpan_Struct
    {
        internal long start;
        internal long end;

        internal static readonly int Size = Marshal.SizeOf(typeof(LifeSpan_Struct));
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct NegotiationInfo
    {
        // see SecPkgContext_NegotiationInfoW in <sspi.h>

        // [MarshalAs(UnmanagedType.LPStruct)] internal SecurityPackageInfo PackageInfo;
        internal IntPtr PackageInfo;
        internal uint NegotiationState;
        internal static readonly int Size = Marshal.SizeOf(typeof(NegotiationInfo));
        internal static readonly int NegotiationStateOffset = (int)Marshal.OffsetOf(typeof(NegotiationInfo), "NegotiationState");
    }

    // Note: pack=0 since the first member (SessionKeyLength) is C's long (platform dependent).
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    internal struct SecPkgContext_SessionKey
    {
        //[MarshalAs(UnmanagedType.SysUInt)] internal uint SessionKeyLength;
        internal uint SessionKeyLength;
        internal IntPtr Sessionkey;
        internal static readonly int Size = Marshal.SizeOf(typeof(SecPkgContext_SessionKey));
        internal static readonly int SessionkeyOffset = (int)Marshal.OffsetOf(typeof(SecPkgContext_SessionKey), "Sessionkey");
    }

    internal class LifeSpan
    {
        DateTime effectiveTimeUtc;
        DateTime expiryTimeUtc;

        internal DateTime EffectiveTimeUtc
        {
            get
            {
                return this.effectiveTimeUtc;
            }
        }

        internal DateTime ExpiryTimeUtc
        {
            get
            {
                return this.expiryTimeUtc;
            }
        }

        internal unsafe LifeSpan(byte[] buffer)
        {
            fixed (byte* pbuffer = &buffer[0])
            {
                IntPtr ptr = new IntPtr(pbuffer);
                LifeSpan_Struct lifeSpan = (LifeSpan_Struct)Marshal.PtrToStructure(ptr, typeof(LifeSpan_Struct));
                // start and end times are expressed as local file times.
                // however dateTime.FromFileTime* expects the file time to be in UTC.
                // so we need to add the difference to the DateTime 
                this.effectiveTimeUtc = DateTime.FromFileTimeUtc(lifeSpan.start) + (DateTime.UtcNow - DateTime.Now);
                this.expiryTimeUtc = DateTime.FromFileTimeUtc(lifeSpan.end) + (DateTime.UtcNow - DateTime.Now);
            }
        }
    }

    internal class SecurityPackageInfoClass
    {
        internal int Capabilities = 0;
        internal short Version = 0;
        internal short RPCID = 0;
        internal int MaxToken = 0;
        internal string Name = null;
        internal string Comment = null;

        internal SecurityPackageInfoClass(SafeHandle safeHandle, int index)
        {
            if (safeHandle.IsInvalid)
            {
                return;
            }
            IntPtr unmanagedAddress = IntPtrHelper.Add(safeHandle.DangerousGetHandle(), SecurityPackageInfo.Size * index);
            Capabilities = Marshal.ReadInt32(unmanagedAddress, (int)Marshal.OffsetOf(typeof(SecurityPackageInfo), "Capabilities"));
            Version = Marshal.ReadInt16(unmanagedAddress, (int)Marshal.OffsetOf(typeof(SecurityPackageInfo), "Version"));
            RPCID = Marshal.ReadInt16(unmanagedAddress, (int)Marshal.OffsetOf(typeof(SecurityPackageInfo), "RPCID"));
            MaxToken = Marshal.ReadInt32(unmanagedAddress, (int)Marshal.OffsetOf(typeof(SecurityPackageInfo), "MaxToken"));

            IntPtr unmanagedString;
            unmanagedString = Marshal.ReadIntPtr(unmanagedAddress, (int)Marshal.OffsetOf(typeof(SecurityPackageInfo), "Name"));
            if (unmanagedString != IntPtr.Zero)
            {
                Name = Marshal.PtrToStringUni(unmanagedString);
            }

            unmanagedString = Marshal.ReadIntPtr(unmanagedAddress, (int)Marshal.OffsetOf(typeof(SecurityPackageInfo), "Comment"));
            if (unmanagedString != IntPtr.Zero)
            {

                Comment = Marshal.PtrToStringUni(unmanagedString);
            }
        }

    }


    // we keep it simple since we use this only to know if NTLM or
    // Kerberos are used in the context of a Negotiate handshake
    internal class NegotiationInfoClass
    {
        internal const string NTLM = "NTLM";
        internal const string Kerberos = "Kerberos";
        internal string AuthenticationPackage;

        internal NegotiationInfoClass(SafeHandle safeHandle, int negotiationState)
        {
            if (safeHandle.IsInvalid)
            {
                return;
            }
            IntPtr packageInfo = safeHandle.DangerousGetHandle();

            const int SECPKG_NEGOTIATION_COMPLETE = 0;
            const int SECPKG_NEGOTIATION_OPTIMISTIC = 1;
            // const int SECPKG_NEGOTIATION_IN_PROGRESS     = 2;
            // const int SECPKG_NEGOTIATION_DIRECT          = 3;
            // const int SECPKG_NEGOTIATION_TRY_MULTICRED   = 4;

            if (negotiationState == SECPKG_NEGOTIATION_COMPLETE || negotiationState == SECPKG_NEGOTIATION_OPTIMISTIC)
            {
                IntPtr unmanagedString = Marshal.ReadIntPtr(packageInfo, SecurityPackageInfo.NameOffest);
                string name = null;
                if (unmanagedString != IntPtr.Zero)
                {
                    name = Marshal.PtrToStringUni(unmanagedString);
                }
                // an optimization for future string comparisons
                if (string.Compare(name, "Kerberos", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    AuthenticationPackage = Kerberos;
                }
                else if (string.Compare(name, "NTLM", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    AuthenticationPackage = NTLM;
                }
                else
                {
                    AuthenticationPackage = name;
                }
            }
        }
    }

    internal class SecuritySessionKeyClass
    {
        byte[] sessionKey;

        internal SecuritySessionKeyClass(SafeHandle safeHandle, int sessionKeyLength)
        {
            byte[] sessionKey = new byte[sessionKeyLength];
            Marshal.Copy(safeHandle.DangerousGetHandle(), sessionKey, 0, sessionKeyLength);
            this.sessionKey = sessionKey;
        }

        internal byte[] SessionKey
        {
            get { return this.sessionKey; }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal class StreamSizes
    {
        public int header;
        public int trailer;
        public int maximumMessage;
        public int buffersCount;
        public int blockSize;

        internal unsafe StreamSizes(byte[] memory)
        {
            fixed (void* voidPtr = memory)
            {
                IntPtr unmanagedAddress = new IntPtr(voidPtr);
                header = Marshal.ReadInt32(unmanagedAddress);
                trailer = Marshal.ReadInt32(unmanagedAddress, 4);
                maximumMessage = Marshal.ReadInt32(unmanagedAddress, 8);
                buffersCount = Marshal.ReadInt32(unmanagedAddress, 12);
                blockSize = Marshal.ReadInt32(unmanagedAddress, 16);
            }
        }
        public static readonly int SizeOf = Marshal.SizeOf(typeof(StreamSizes));
    }

    internal static class SspiWrapper
    {
        const int SECPKG_FLAG_NEGOTIABLE2 = 0x00200000;

        static SecurityPackageInfoClass[] securityPackages;

        public static SecurityPackageInfoClass[] SecurityPackages
        {
            get
            {
                return securityPackages;
            }
            set
            {
                securityPackages = value;
            }
        }

        static SecurityPackageInfoClass[] EnumerateSecurityPackages()
        {
            if (SecurityPackages != null)
            {
                return SecurityPackages;
            }

            int moduleCount = 0;
            SafeFreeContextBuffer arrayBaseHandle = null;
            try
            {
                int errorCode = SafeFreeContextBuffer.EnumeratePackages(out moduleCount, out arrayBaseHandle);
                if (errorCode != 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(errorCode));
                }

                SecurityPackageInfoClass[] securityPackages = new SecurityPackageInfoClass[moduleCount];
                for (int i = 0; i < moduleCount; i++)
                {
                    securityPackages[i] = new SecurityPackageInfoClass(arrayBaseHandle, i);
                }
                SecurityPackages = securityPackages;
            }
            finally
            {
                if (arrayBaseHandle != null)
                {
                    arrayBaseHandle.Close();
                }
            }

            return SecurityPackages;
        }

        public static SecurityPackageInfoClass GetVerifyPackageInfo(string packageName)
        {
            SecurityPackageInfoClass[] supportedSecurityPackages = EnumerateSecurityPackages();
            if (supportedSecurityPackages != null)
            {
                for (int i = 0; i < supportedSecurityPackages.Length; i++)
                {
                    if (String.Compare(supportedSecurityPackages[i].Name, packageName, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return supportedSecurityPackages[i];
                    }
                }
            }

            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.SSPIPackageNotSupported, packageName)));
        }

        public static bool IsNegotiateExPackagePresent()
        {
            SecurityPackageInfoClass[] supportedSecurityPackages = EnumerateSecurityPackages();

            if (supportedSecurityPackages != null)
            {
                int nego2FlagIntValue = (int)SECPKG_FLAG_NEGOTIABLE2;

                for (int i = 0; i < supportedSecurityPackages.Length; i++)
                {
                    // if the package is a nego2 package
                    if ((supportedSecurityPackages[i].Capabilities & nego2FlagIntValue) != 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static SafeFreeCredentials AcquireDefaultCredential(
            string package,
            CredentialUse intent,
            params string[] additionalPackages)
        {
            SafeFreeCredentials outCredential = null;
            AuthIdentityEx authIdentity = new AuthIdentityEx(null, null, null, additionalPackages);
            int errorCode = SafeFreeCredentials.AcquireDefaultCredential(package, intent, ref authIdentity, out outCredential);
            if (errorCode != 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(errorCode));
            }
            return outCredential;
        }

        public static SafeFreeCredentials AcquireCredentialsHandle(
            string package,
            CredentialUse intent,
            ref AuthIdentityEx authdata)
        {
            SafeFreeCredentials credentialsHandle = null;
            int errorCode = SafeFreeCredentials.AcquireCredentialsHandle(package,
                intent,
                ref authdata,
                out credentialsHandle
                );
            if (errorCode != 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(errorCode));
            }
            return credentialsHandle;
        }

        public static SafeFreeCredentials AcquireCredentialsHandle(
            string package,
            CredentialUse intent,
            SecureCredential scc)
        {
            SafeFreeCredentials outCredential = null;
            int errorCode = SafeFreeCredentials.AcquireCredentialsHandle(
                package,
                intent,
                ref scc,
                out outCredential
                );
            if (errorCode != 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(errorCode));
            }
            return outCredential;
        }

        public static SafeFreeCredentials AcquireCredentialsHandle(
        string package,
        CredentialUse intent,
        ref IntPtr ppAuthIdentity)
        {
            SafeFreeCredentials outCredential = null;
            int errorCode = SafeFreeCredentials.AcquireCredentialsHandle(
                package,
                intent,
                ref ppAuthIdentity,
                out outCredential
                );
            if (errorCode != 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(errorCode));
            }
            return outCredential;
        }

        internal static int InitializeSecurityContext(
            SafeFreeCredentials credential,
            ref SafeDeleteContext context,
            string targetName,
            SspiContextFlags inFlags,
            Endianness datarep,
            SecurityBuffer inputBuffer,
            SecurityBuffer outputBuffer,
            ref SspiContextFlags outFlags)
        {
            return SafeDeleteContext.InitializeSecurityContext(credential, ref context, targetName, inFlags, datarep, inputBuffer, null, outputBuffer, ref outFlags);
        }

        internal static int InitializeSecurityContext(
            SafeFreeCredentials credential,
            ref SafeDeleteContext context,
            string targetName,
            SspiContextFlags inFlags,
            Endianness datarep,
            SecurityBuffer[] inputBuffers,
            SecurityBuffer outputBuffer,
            ref SspiContextFlags outFlags)
        {
            return SafeDeleteContext.InitializeSecurityContext(credential, ref context, targetName, inFlags, datarep, null, inputBuffers, outputBuffer, ref outFlags);
        }

        internal static int AcceptSecurityContext(
            SafeFreeCredentials credential,
            ref SafeDeleteContext refContext,
            SspiContextFlags inFlags,
            Endianness datarep,
            SecurityBuffer inputBuffer,
            SecurityBuffer outputBuffer,
            ref SspiContextFlags outFlags)
        {
            return SafeDeleteContext.AcceptSecurityContext(credential, ref refContext, inFlags, datarep, inputBuffer, null, outputBuffer, ref outFlags);
        }

        internal static int AcceptSecurityContext(
            SafeFreeCredentials credential,
            ref SafeDeleteContext refContext,
            SspiContextFlags inFlags,
            Endianness datarep,
            SecurityBuffer[] inputBuffers,
            SecurityBuffer outputBuffer,
            ref SspiContextFlags outFlags)
        {
            return SafeDeleteContext.AcceptSecurityContext(credential, ref refContext, inFlags, datarep, null, inputBuffers, outputBuffer, ref outFlags);
        }

        public static int QuerySecurityContextToken(
            SafeDeleteContext context,
            out SafeCloseHandle token)
        {
            return context.GetSecurityContextToken(out token);
        }

        static unsafe int QueryContextAttributes(SafeDeleteContext phContext, ContextAttribute attribute, byte[] buffer, Type handleType, out SafeHandle refHandle)
        {
            refHandle = null;
            if (handleType != null)
            {
                if (handleType == typeof(SafeFreeContextBuffer))
                {
                    refHandle = SafeFreeContextBuffer.CreateEmptyHandle();
                }
                else if (handleType == typeof(SafeFreeCertContext))
                {
                    refHandle = new SafeFreeCertContext();
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("handleType", SR.GetString(SR.ValueMustBeOf2Types, typeof(SafeFreeContextBuffer).ToString(), typeof(SafeFreeCertContext).ToString())));
                }
            }
            fixed (byte* bufferPtr = buffer)
            {
                return SafeFreeContextBuffer.QueryContextAttributes(phContext, attribute, bufferPtr, refHandle);
            }
        }

        public static unsafe object QueryContextAttributes(
            SafeDeleteContext securityContext,
            ContextAttribute contextAttribute)
        {
            int nativeBlockSize = IntPtr.Size;
            Type handleType = null;

            switch (contextAttribute)
            {
                case ContextAttribute.Flags:
                    break;
                case ContextAttribute.Sizes:
                    nativeBlockSize = SecSizes.SizeOf;
                    break;
                case ContextAttribute.StreamSizes:
                    nativeBlockSize = StreamSizes.SizeOf;
                    break;
                case ContextAttribute.Names:
                    handleType = typeof(SafeFreeContextBuffer);
                    break;
                case ContextAttribute.PackageInfo:
                    handleType = typeof(SafeFreeContextBuffer);
                    break;
                case ContextAttribute.NegotiationInfo:
                    handleType = typeof(SafeFreeContextBuffer);
                    nativeBlockSize = Marshal.SizeOf(typeof(NegotiationInfo));
                    break;
                case ContextAttribute.RemoteCertificate:
                    handleType = typeof(SafeFreeCertContext);
                    break;
                case ContextAttribute.LocalCertificate:
                    handleType = typeof(SafeFreeCertContext);
                    break;
                case ContextAttribute.ConnectionInfo:
                    nativeBlockSize = Marshal.SizeOf(typeof(SslConnectionInfo));
                    break;
                case ContextAttribute.Lifespan:
                    nativeBlockSize = LifeSpan_Struct.Size;
                    break;
                case ContextAttribute.SessionKey:
                    handleType = typeof(SafeFreeContextBuffer);
                    nativeBlockSize = SecPkgContext_SessionKey.Size;
                    break;
                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidEnumArgumentException("contextAttribute", (int)contextAttribute,
                    typeof(ContextAttribute)));
            }

            SafeHandle sspiHandle = null;
            object attribute = null;
            try
            {
                byte[] nativeBuffer = new byte[nativeBlockSize];
                int errorCode = QueryContextAttributes(securityContext, contextAttribute, nativeBuffer, handleType, out sspiHandle);
                if (errorCode != 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(errorCode));
                }

                switch (contextAttribute)
                {
                    case ContextAttribute.Flags:
                        fixed (byte* pnativeBuffer = nativeBuffer)
                        {
                            attribute = (object)Marshal.ReadInt32(new IntPtr(pnativeBuffer));
                        }
                        break;
                    case ContextAttribute.Sizes:
                        attribute = new SecSizes(nativeBuffer);
                        break;
                    case ContextAttribute.StreamSizes:
                        attribute = new StreamSizes(nativeBuffer);
                        break;
                    case ContextAttribute.Names:
                        attribute = Marshal.PtrToStringUni(sspiHandle.DangerousGetHandle());
                        break;
                    case ContextAttribute.PackageInfo:
                        attribute = new SecurityPackageInfoClass(sspiHandle, 0);
                        break;
                    case ContextAttribute.NegotiationInfo:
                        unsafe
                        {
                            fixed (void* ptr = nativeBuffer)
                            {
                                attribute = new NegotiationInfoClass(sspiHandle, Marshal.ReadInt32(new IntPtr(ptr), NegotiationInfo.NegotiationStateOffset));
                            }
                        }
                        break;
                    case ContextAttribute.LocalCertificate:
                        goto case ContextAttribute.RemoteCertificate;
                    case ContextAttribute.RemoteCertificate:
                        attribute = sspiHandle;
                        sspiHandle = null;
                        break;
                    case ContextAttribute.ConnectionInfo:
                        attribute = new SslConnectionInfo(nativeBuffer);
                        break;
                    case ContextAttribute.Lifespan:
                        attribute = new LifeSpan(nativeBuffer);
                        break;
                    case ContextAttribute.SessionKey:
                        unsafe
                        {
                            fixed (void* ptr = nativeBuffer)
                            {
                                attribute = new SecuritySessionKeyClass(sspiHandle, Marshal.ReadInt32(new IntPtr(ptr)));
                            }
                        }
                        break;
                    default:
                        // will return null
                        break;
                }
            }
            finally
            {
                if (sspiHandle != null)
                {
                    sspiHandle.Close();
                }
            }
            return attribute;
        }

        /// <summary>
        /// Queries the security context for the target name (SPN for kerb)
        /// </summary>
        /// <param name="securityContext">security context to query</param>
        /// <param name="specifiedTarget">output parameter for the name</param>
        /// <returns>the status code returned from querying the context</returns>
        public static unsafe int QuerySpecifiedTarget(SafeDeleteContext securityContext, out string specifiedTarget)
        {
            int nativeBlockSize = IntPtr.Size;
            Type handleType = typeof(SafeFreeContextBuffer);
            SafeHandle sspiHandle = null;
            int errorCode;

            specifiedTarget = null;
            try
            {
                byte[] nativeBuffer = new byte[nativeBlockSize];
                errorCode = QueryContextAttributes(securityContext, ContextAttribute.SpecifiedTarget, nativeBuffer, handleType, out sspiHandle);
                if (errorCode != (int)SecurityStatus.OK)
                {
                    return errorCode;
                }

                specifiedTarget = Marshal.PtrToStringUni(sspiHandle.DangerousGetHandle()) as string;
            }
            finally
            {
                if (sspiHandle != null)
                {
                    sspiHandle.Close();
                }
            }
            return errorCode;
        }

        public static void ImpersonateSecurityContext(
            SafeDeleteContext context)
        {
            int errorCode = SafeDeleteContext.ImpersonateSecurityContext(context);
            if (errorCode != (int)SecurityStatus.OK)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(errorCode));
            }
        }

        public static unsafe int EncryptDecryptHelper(SafeDeleteContext context, SecurityBuffer[] input, uint sequenceNumber, bool encrypt, bool isGssBlob)
        {
            SecurityBufferDescriptor sdcInOut = new SecurityBufferDescriptor(input.Length);
            SecurityBufferStruct[] unmanagedBuffer = new SecurityBufferStruct[input.Length];
            byte[][] buffers = new byte[input.Length][];
            fixed (void* unmanagedBufferPtr = unmanagedBuffer)
            {
                sdcInOut.UnmanagedPointer = unmanagedBufferPtr;
                GCHandle[] pinnedBuffers = new GCHandle[input.Length];
                try
                {
                    for (int i = 0; i < input.Length; ++i)
                    {
                        SecurityBuffer iBuffer = input[i];
                        unmanagedBuffer[i].count = iBuffer.size;
                        unmanagedBuffer[i].type = iBuffer.type;
                        if (iBuffer.token == null || iBuffer.token.Length == 0)
                        {
                            unmanagedBuffer[i].token = IntPtr.Zero;
                        }
                        else
                        {
                            pinnedBuffers[i] = GCHandle.Alloc(iBuffer.token, GCHandleType.Pinned);
                            unmanagedBuffer[i].token = Marshal.UnsafeAddrOfPinnedArrayElement(iBuffer.token, iBuffer.offset);
                            buffers[i] = iBuffer.token;
                        }
                    }
                    int errorCode;
                    if (encrypt)
                    {
                        errorCode = SafeDeleteContext.EncryptMessage(context, sdcInOut, sequenceNumber);
                    }
                    else
                    {
                        errorCode = SafeDeleteContext.DecryptMessage(context, sdcInOut, sequenceNumber);
                    }
                    // Marshalling back returned sizes (do not marshal the "token" field)
                    for (int i = 0; i < input.Length; ++i)
                    {
                        SecurityBuffer iBuffer = input[i];
                        iBuffer.size = unmanagedBuffer[i].count;
                        iBuffer.type = unmanagedBuffer[i].type;
                        if (iBuffer.size == 0)
                        {
                            iBuffer.offset = 0;
                            iBuffer.token = null;
                        }
                        else if (isGssBlob && !encrypt && iBuffer.type == BufferType.Data)
                        {
                            iBuffer.token = DiagnosticUtility.Utility.AllocateByteArray(iBuffer.size);
                            Marshal.Copy(unmanagedBuffer[i].token, iBuffer.token, 0, iBuffer.size);
                        }
                        else checked
                            {
                                // Find the buffer this is inside of.  Usually they all point inside buffer 0.
                                int j;
                                for (j = 0; j < input.Length; j++)
                                {
                                    if (buffers[j] == null)
                                    {
                                        continue;
                                    }

                                    byte* bufferAddress = (byte*)Marshal.UnsafeAddrOfPinnedArrayElement(buffers[j], 0);
                                    if ((byte*)unmanagedBuffer[i].token >= bufferAddress &&
                                        (byte*)unmanagedBuffer[i].token + iBuffer.size <= bufferAddress + buffers[j].Length)
                                    {
                                        iBuffer.offset = (int)((byte*)unmanagedBuffer[i].token - bufferAddress);
                                        iBuffer.token = buffers[j];
                                        break;
                                    }
                                }

                                if (j >= input.Length)
                                {
                                    iBuffer.size = 0;
                                    iBuffer.offset = 0;
                                    iBuffer.token = null;
                                }
                                if (!(iBuffer.offset >= 0 && iBuffer.offset <= (iBuffer.token == null ? 0 : iBuffer.token.Length)))
                                {
                                    DiagnosticUtility.DebugAssert(SR.GetString(SR.SspiWrapperEncryptDecryptAssert1, iBuffer.offset));
                                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SspiWrapperEncryptDecryptAssert1, iBuffer.offset)));

                                }
                                if (!(iBuffer.size >= 0 && iBuffer.size <= (iBuffer.token == null ? 0 : iBuffer.token.Length - iBuffer.offset)))
                                {
                                    DiagnosticUtility.DebugAssert(SR.GetString(SR.SspiWrapperEncryptDecryptAssert2, iBuffer.size));
                                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SspiWrapperEncryptDecryptAssert2, iBuffer.size)));
                                }
                            }
                    }
                    return errorCode;
                }
                finally
                {
                    for (int i = 0; i < pinnedBuffers.Length; ++i)
                    {
                        if (pinnedBuffers[i].IsAllocated)
                        {
                            pinnedBuffers[i].Free();
                        }
                    }
                }
            }
        }

        public static unsafe int EncryptMessage(SafeDeleteContext context, SecurityBuffer[] input, uint sequenceNumber)
        {
            return EncryptDecryptHelper(context, input, sequenceNumber, true, false);
        }

        public static unsafe int DecryptMessage(SafeDeleteContext context, SecurityBuffer[] input, uint sequenceNumber, bool isGssBlob)
        {
            return EncryptDecryptHelper(context, input, sequenceNumber, false, isGssBlob);
        }

        public static unsafe uint SspiPromptForCredential(string targetName, string packageName, out IntPtr ppAuthIdentity, ref bool saveCredentials)
        {
            CREDUI_INFO credui_Info = new CREDUI_INFO();
            credui_Info.cbSize = Marshal.SizeOf(typeof(CREDUI_INFO));

            credui_Info.pszCaptionText = SR.GetString(SR.SspiLoginPromptHeaderMessage); // Login
            credui_Info.pszMessageText = "";
            uint retCode = uint.MaxValue;
            retCode = NativeMethods.SspiPromptForCredentials(targetName, ref credui_Info, 0, packageName, IntPtr.Zero, out ppAuthIdentity, ref saveCredentials, 0);
            return retCode;
        }

        public static unsafe bool IsSspiPromptingNeeded(uint ErrorOrNtStatus)
        {
            return NativeMethods.SspiIsPromptingNeeded(ErrorOrNtStatus);
        }

        //public static string ErrorDescription(int errorCode) 
        //{
        //    if (errorCode == -1) 
        //    {
        //        return "An exception when invoking Win32 API";
        //    }
        //    switch ((SecurityStatus) errorCode) 
        //    {
        //        case SecurityStatus.InvalidHandle:
        //            return "Invalid handle";
        //        case SecurityStatus.InvalidToken:
        //            return "Invalid token";
        //        case SecurityStatus.ContinueNeeded:
        //            return "Continue needed";
        //        case SecurityStatus.IncompleteMessage:
        //            return "Message incomplete";
        //        case SecurityStatus.WrongPrincipal:
        //            return "Wrong principal";
        //        case SecurityStatus.TargetUnknown:
        //            return "Target unknown";
        //        case SecurityStatus.PackageNotFound:
        //            return "Package not found";
        //        case SecurityStatus.BufferNotEnough:
        //            return "Buffer not enough";
        //        case SecurityStatus.MessageAltered:
        //            return "Message altered";
        //        case SecurityStatus.UntrustedRoot:
        //            return "Untrusted root";
        //        default:
        //            return "0x" + errorCode.ToString("x", NumberFormatInfo.InvariantInfo);
        //    }
        //}
    }

    //From Schannel.h
    [StructLayout(LayoutKind.Sequential)]
    internal class SslConnectionInfo
    {
        public readonly int Protocol;
        public readonly int DataCipherAlg;
        public readonly int DataKeySize;
        public readonly int DataHashAlg;
        public readonly int DataHashKeySize;
        public readonly int KeyExchangeAlg;
        public readonly int KeyExchKeySize;

        internal unsafe SslConnectionInfo(byte[] nativeBuffer)
        {
            fixed (void* voidPtr = nativeBuffer)
            {
                IntPtr unmanagedAddress = new IntPtr(voidPtr);
                Protocol = Marshal.ReadInt32(unmanagedAddress);
                DataCipherAlg = Marshal.ReadInt32(unmanagedAddress, 4);
                DataKeySize = Marshal.ReadInt32(unmanagedAddress, 8);
                DataHashAlg = Marshal.ReadInt32(unmanagedAddress, 12);
                DataHashKeySize = Marshal.ReadInt32(unmanagedAddress, 16);
                KeyExchangeAlg = Marshal.ReadInt32(unmanagedAddress, 20);
                KeyExchKeySize = Marshal.ReadInt32(unmanagedAddress, 24);
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal class SecSizes
    {
        public int MaxToken;
        public int MaxSignature;
        public int BlockSize;
        public int SecurityTrailer;

        internal unsafe SecSizes(byte[] memory)
        {
            fixed (void* voidPtr = memory)
            {
                IntPtr unmanagedAddress = new IntPtr(voidPtr);
                MaxToken = Marshal.ReadInt32(unmanagedAddress);
                MaxSignature = Marshal.ReadInt32(unmanagedAddress, 4);
                BlockSize = Marshal.ReadInt32(unmanagedAddress, 8);
                SecurityTrailer = Marshal.ReadInt32(unmanagedAddress, 12);
            }
        }
        public static readonly int SizeOf = Marshal.SizeOf(typeof(SecSizes));
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct Bindings
    {
        // see SecPkgContext_Bindings in <sspi.h>
        internal int BindingsLength;
        internal IntPtr pBindings;
    }
}
