//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IdentityModel;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Authentication.ExtendedProtection;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Principal;
    using System.Threading;

    using DiagnosticUtility = System.ServiceModel.DiagnosticUtility;
    using SR = System.ServiceModel.SR;

    sealed class TlsSspiNegotiation : ISspiNegotiation
    {
        static SspiContextFlags ClientStandardFlags;
        static SspiContextFlags ServerStandardFlags;
        static SspiContextFlags StandardFlags;

        SspiContextFlags attributes;
        X509Certificate2 clientCertificate;
        bool clientCertRequired;
        SslConnectionInfo connectionInfo;
        SafeFreeCredentials credentialsHandle;
        string destination;
        bool disposed;
        bool isCompleted;
        bool isServer;
        SchProtocols protocolFlags;
        X509Certificate2 remoteCertificate;
        SafeDeleteContext securityContext;

        //also used as a static lock object
        const string SecurityPackage = "Microsoft Unified Security Protocol Provider";

        X509Certificate2 serverCertificate;
        StreamSizes streamSizes;
        Object syncObject = new Object();
        bool wasClientCertificateSent;
        X509Certificate2Collection remoteCertificateChain;
        string incomingValueTypeUri;
        /// <summary>
        /// Client side ctor
        /// </summary>
        public TlsSspiNegotiation(
            string destination,
            SchProtocols protocolFlags,
            X509Certificate2 clientCertificate) :
            this(destination, false, protocolFlags, null, clientCertificate, false)
        { }

        /// <summary>
        /// Server side ctor
        /// </summary>
        public TlsSspiNegotiation(
            SchProtocols protocolFlags,
            X509Certificate2 serverCertificate,
            bool clientCertRequired) :
            this(null, true, protocolFlags, serverCertificate, null, clientCertRequired)
        { }

        static TlsSspiNegotiation()
        {
            StandardFlags = SspiContextFlags.ReplayDetect | SspiContextFlags.Confidentiality | SspiContextFlags.AllocateMemory;
            ServerStandardFlags = StandardFlags | SspiContextFlags.AcceptExtendedError | SspiContextFlags.AcceptStream;
            ClientStandardFlags = StandardFlags | SspiContextFlags.InitManualCredValidation | SspiContextFlags.InitStream;
        }

        private TlsSspiNegotiation(
            string destination,
            bool isServer,
            SchProtocols protocolFlags,
            X509Certificate2 serverCertificate,
            X509Certificate2 clientCertificate,
            bool clientCertRequired)
        {
            SspiWrapper.GetVerifyPackageInfo(SecurityPackage);
            this.destination = destination;
            this.isServer = isServer;
            this.protocolFlags = protocolFlags;
            this.serverCertificate = serverCertificate;
            this.clientCertificate = clientCertificate;
            this.clientCertRequired = clientCertRequired;
            this.securityContext = null;
            if (isServer)
            {
                ValidateServerCertificate();
            }
            else
            {
                ValidateClientCertificate();
            }
            if (this.isServer)
            {
                // This retry is to address intermittent failure when accessing private key (MB56153)
                try
                {
                    AcquireServerCredentials();
                }
                catch (Win32Exception ex)
                {
                    if (ex.NativeErrorCode != (int)SecurityStatus.UnknownCredential)
                    {
                        throw;
                    }

                    DiagnosticUtility.TraceHandledException(ex, TraceEventType.Information);

                    // Yield
                    Thread.Sleep(0);
                    AcquireServerCredentials();
                }
            }
            else
            {
                // delay client credentials presenting till they are asked for
                AcquireDummyCredentials();
            }
        }

        /// <summary>
        /// Local cert of client side
        /// </summary>
        public X509Certificate2 ClientCertificate
        {
            get
            {
                ThrowIfDisposed();
                return this.clientCertificate;
            }
        }

        public bool ClientCertRequired
        {
            get
            {
                ThrowIfDisposed();
                return this.clientCertRequired;
            }
        }

        public string Destination
        {
            get
            {
                ThrowIfDisposed();
                return this.destination;
            }
        }

        public DateTime ExpirationTimeUtc
        {
            get
            {
                ThrowIfDisposed();
                return SecurityUtils.MaxUtcDateTime;
            }
        }

        public bool IsCompleted
        {
            get
            {
                ThrowIfDisposed();
                return this.isCompleted;
            }
        }

        public bool IsMutualAuthFlag
        {
            get
            {
                ThrowIfDisposed();
                return (this.attributes & SspiContextFlags.MutualAuth) != 0;
            }
        }

        public bool IsValidContext
        {
            get
            {
                return (this.securityContext != null && this.securityContext.IsInvalid == false);
            }
        }

        public string KeyEncryptionAlgorithm
        {
            get
            {
                return SecurityAlgorithms.TlsSspiKeyWrap;
            }
        }

        /// <summary>
        /// The cert of the remote party
        /// </summary>
        public X509Certificate2 RemoteCertificate
        {
            get
            {
                ThrowIfDisposed();
                if (!IsValidContext)
                {
                    // PreSharp Bug: Property get methods should not throw exceptions.
#pragma warning suppress 56503
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception((int)SecurityStatus.InvalidHandle));
                }
                if (this.remoteCertificate == null)
                {
                    ExtractRemoteCertificate();
                }
                return this.remoteCertificate;
            }
        }

        public X509Certificate2Collection RemoteCertificateChain
        {
            get
            {
                ThrowIfDisposed();
                if (!IsValidContext)
                {
                    // PreSharp Bug: Property get methods should not throw exceptions.
#pragma warning suppress 56503
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception((int)SecurityStatus.InvalidHandle));
                }
                if (this.remoteCertificateChain == null)
                {
                    ExtractRemoteCertificate();
                }
                return this.remoteCertificateChain;
            }
        }

        /// <summary>
        /// Local cert of server side
        /// </summary>
        public X509Certificate2 ServerCertificate
        {
            get
            {
                ThrowIfDisposed();
                return this.serverCertificate;
            }
        }

        public bool WasClientCertificateSent
        {
            get
            {
                ThrowIfDisposed();
                return this.wasClientCertificateSent;
            }
        }

        internal SslConnectionInfo ConnectionInfo
        {
            get
            {
                ThrowIfDisposed();
                if (!IsValidContext)
                {
                    // PreSharp Bug: Property get methods should not throw exceptions.
#pragma warning suppress 56503
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception((int)SecurityStatus.InvalidHandle));
                }
                if (this.connectionInfo == null)
                {
                    SslConnectionInfo tmpInfo = SspiWrapper.QueryContextAttributes(
                        this.securityContext,
                        ContextAttribute.ConnectionInfo
                        ) as SslConnectionInfo;
                    if (IsCompleted)
                    {
                        this.connectionInfo = tmpInfo;
                    }
                    return tmpInfo;
                }
                return this.connectionInfo;
            }
        }

        internal StreamSizes StreamSizes
        {
            get
            {
                ThrowIfDisposed();
                if (this.streamSizes == null)
                {
                    StreamSizes tmpSizes = (StreamSizes)SspiWrapper.QueryContextAttributes(this.securityContext, ContextAttribute.StreamSizes);
                    if (this.IsCompleted)
                    {
                        this.streamSizes = tmpSizes;
                    }
                    return tmpSizes;
                }
                return this.streamSizes;
            }
        }

        // This is for CDF1229 workaround to be able to echo incoming and outgoing ValueType
        internal string IncomingValueTypeUri
        {
            get { return this.incomingValueTypeUri; }
            set { this.incomingValueTypeUri = value; }
        }

        public string GetRemoteIdentityName()
        {
            if (!this.IsValidContext)
            {
                return String.Empty;
            }
            X509Certificate2 cert = this.RemoteCertificate;
            if (cert == null)
            {
                return String.Empty;
            }
            return SecurityUtils.GetCertificateId(cert);
        }

        public byte[] Decrypt(byte[] encryptedContent)
        {
            ThrowIfDisposed();
            byte[] dataBuffer = DiagnosticUtility.Utility.AllocateByteArray(encryptedContent.Length);

            Buffer.BlockCopy(encryptedContent, 0, dataBuffer, 0, encryptedContent.Length);

            int decryptedLen = 0;
            int dataStartOffset;
            this.DecryptInPlace(dataBuffer, out dataStartOffset, out decryptedLen);
            byte[] outputBuffer = DiagnosticUtility.Utility.AllocateByteArray(decryptedLen);

            Buffer.BlockCopy(dataBuffer, dataStartOffset, outputBuffer, 0, decryptedLen);
            return outputBuffer;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public byte[] Encrypt(byte[] input)
        {
            ThrowIfDisposed();
            byte[] buffer = DiagnosticUtility.Utility.AllocateByteArray(checked(input.Length + StreamSizes.header + StreamSizes.trailer));

            Buffer.BlockCopy(input, 0, buffer, StreamSizes.header, input.Length);

            int encryptedSize = 0;

            this.EncryptInPlace(buffer, 0, input.Length, out encryptedSize);
            if (encryptedSize == buffer.Length)
            {
                return buffer;
            }
            else
            {
                byte[] outputBuffer = DiagnosticUtility.Utility.AllocateByteArray(encryptedSize);
                Buffer.BlockCopy(buffer, 0, outputBuffer, 0, encryptedSize);
                return outputBuffer;
            }
        }

        public byte[] GetOutgoingBlob(byte[] incomingBlob, ChannelBinding channelbinding, ExtendedProtectionPolicy protectionPolicy)
        {
            ThrowIfDisposed();
            SecurityBuffer incomingSecurity = null;
            if (incomingBlob != null)
            {
                incomingSecurity = new SecurityBuffer(incomingBlob, BufferType.Token);
            }

            SecurityBuffer outgoingSecurity = new SecurityBuffer(null, BufferType.Token);
            this.remoteCertificate = null;
            int statusCode = 0;
            if (this.isServer == true)
            {
                statusCode = SspiWrapper.AcceptSecurityContext(
                    this.credentialsHandle,
                    ref this.securityContext,
                    ServerStandardFlags | (this.clientCertRequired ? SspiContextFlags.MutualAuth : SspiContextFlags.Zero),
                    Endianness.Native,
                    incomingSecurity,
                    outgoingSecurity,
                    ref this.attributes
                    );

            }
            else
            {
                statusCode = SspiWrapper.InitializeSecurityContext(
                    this.credentialsHandle,
                    ref this.securityContext,
                    this.destination,
                    ClientStandardFlags,
                    Endianness.Native,
                    incomingSecurity,
                    outgoingSecurity,
                    ref this.attributes
                    );
            }

            if ((statusCode & unchecked((int)0x80000000)) != 0)
            {
                this.Dispose();
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(statusCode));
            }

            if (statusCode == (int)SecurityStatus.OK)
            {
                // we're done
                // ensure that the key negotiated is strong enough
                if (SecurityUtils.ShouldValidateSslCipherStrength())
                {
                    SslConnectionInfo connectionInfo = (SslConnectionInfo)SspiWrapper.QueryContextAttributes(this.securityContext, ContextAttribute.ConnectionInfo);
                    if (connectionInfo == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityNegotiationException(SR.GetString(SR.CannotObtainSslConnectionInfo)));
                    }
                    SecurityUtils.ValidateSslCipherStrength(connectionInfo.DataKeySize);
                }
                this.isCompleted = true;
            }
            else if (statusCode == (int)SecurityStatus.CredentialsNeeded)
            {
                // the server requires the client to supply creds
                // Currently we dont attempt to find the client cert to choose at runtime
                // so just re-call the function
                AcquireClientCredentials();
                if (this.ClientCertificate != null)
                {
                    this.wasClientCertificateSent = true;
                }
                return this.GetOutgoingBlob(incomingBlob, channelbinding, protectionPolicy);
            }
            else if (statusCode != (int)SecurityStatus.ContinueNeeded)
            {
                this.Dispose();
                if (statusCode == (int)SecurityStatus.InternalError)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(statusCode, SR.GetString(SR.LsaAuthorityNotContacted)));
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(statusCode));
                }
            }
            return outgoingSecurity.token;
        }

        /// <summary>
        /// The decrypted data will start header bytes from the start of
        /// encryptedContent array.
        /// </summary>
        internal unsafe void DecryptInPlace(byte[] encryptedContent, out int dataStartOffset, out int dataLen)
        {
            ThrowIfDisposed();
            dataStartOffset = StreamSizes.header;
            dataLen = 0;

            byte[] emptyBuffer1 = new byte[0];
            byte[] emptyBuffer2 = new byte[0];
            byte[] emptyBuffer3 = new byte[0];

            SecurityBuffer[] securityBuffer = new SecurityBuffer[4];
            securityBuffer[0] = new SecurityBuffer(encryptedContent, 0, encryptedContent.Length, BufferType.Data);
            securityBuffer[1] = new SecurityBuffer(emptyBuffer1, BufferType.Empty);
            securityBuffer[2] = new SecurityBuffer(emptyBuffer2, BufferType.Empty);
            securityBuffer[3] = new SecurityBuffer(emptyBuffer3, BufferType.Empty);

            int errorCode = SspiWrapper.DecryptMessage(this.securityContext, securityBuffer, 0, false);
            if (errorCode != 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(errorCode));
            }

            for (int i = 0; i < securityBuffer.Length; ++i)
            {
                if (securityBuffer[i].type == BufferType.Data)
                {
                    dataLen = securityBuffer[i].size;
                    return;
                }
            }

            OnBadData();
        }

        /// <summary>
        /// Assumes that the data to encrypt is "header" bytes ahead of bufferStartOffset
        /// </summary>
        internal unsafe void EncryptInPlace(byte[] buffer, int bufferStartOffset, int dataLen, out int encryptedDataLen)
        {
            ThrowIfDisposed();
            encryptedDataLen = 0;
            if (bufferStartOffset + dataLen + StreamSizes.header + StreamSizes.trailer > buffer.Length)
            {
                OnBadData();
            }

            byte[] emptyBuffer = new byte[0];
            int trailerOffset = bufferStartOffset + StreamSizes.header + dataLen;

            SecurityBuffer[] securityBuffer = new SecurityBuffer[4];
            securityBuffer[0] = new SecurityBuffer(buffer, bufferStartOffset, StreamSizes.header, BufferType.Header);
            securityBuffer[1] = new SecurityBuffer(buffer, bufferStartOffset + StreamSizes.header, dataLen, BufferType.Data);
            securityBuffer[2] = new SecurityBuffer(buffer, trailerOffset, StreamSizes.trailer, BufferType.Trailer);
            securityBuffer[3] = new SecurityBuffer(emptyBuffer, BufferType.Empty);

            int errorCode = SspiWrapper.EncryptMessage(this.securityContext, securityBuffer, 0);
            if (errorCode != 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(errorCode));
            }

            int trailerSize = 0;
            for (int i = 0; i < securityBuffer.Length; ++i)
            {
                if (securityBuffer[i].type == BufferType.Trailer)
                {
                    trailerSize = securityBuffer[i].size;
                    encryptedDataLen = StreamSizes.header + dataLen + trailerSize;
                    return;
                }
            }

            OnBadData();
        }

        static void ValidatePrivateKey(X509Certificate2 certificate)
        {
            bool hasPrivateKey = false;
            try
            {
                if (System.ServiceModel.LocalAppContextSwitches.DisableCngCertificates)
                {
                    hasPrivateKey = certificate != null && certificate.PrivateKey != null;
                }
                else
                {
                    hasPrivateKey = certificate.HasPrivateKey && SecurityUtils.CanReadPrivateKey(certificate);
                }
            }
            catch (SecurityException e)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.SslCertMayNotDoKeyExchange, certificate.SubjectName.Name), e));
            }
            catch (CryptographicException e)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.SslCertMayNotDoKeyExchange, certificate.SubjectName.Name), e));
            }
            if (!hasPrivateKey)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.SslCertMustHavePrivateKey, certificate.SubjectName.Name)));
            }
        }

        void ValidateServerCertificate()
        {
            if (this.serverCertificate == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("serverCertificate");
            }

            ValidatePrivateKey(this.serverCertificate);
        }

        void ValidateClientCertificate()
        {
            if (this.clientCertificate != null)
            {
                ValidatePrivateKey(this.clientCertificate);
            }
        }

        private void AcquireClientCredentials()
        {
            SecureCredential secureCredential = new SecureCredential(SecureCredential.CurrentVersion, this.ClientCertificate, SecureCredential.Flags.ValidateManual | SecureCredential.Flags.NoDefaultCred, this.protocolFlags);
            this.credentialsHandle = SspiWrapper.AcquireCredentialsHandle(
                SecurityPackage,
                CredentialUse.Outbound,
                secureCredential
                );
        }

        private void AcquireDummyCredentials()
        {
            SecureCredential secureCredential = new SecureCredential(SecureCredential.CurrentVersion, null, SecureCredential.Flags.ValidateManual | SecureCredential.Flags.NoDefaultCred, this.protocolFlags);
            this.credentialsHandle = SspiWrapper.AcquireCredentialsHandle(SecurityPackage, CredentialUse.Outbound, secureCredential);
        }

        private void AcquireServerCredentials()
        {
            SecureCredential secureCredential = new SecureCredential(SecureCredential.CurrentVersion, this.serverCertificate, SecureCredential.Flags.Zero, this.protocolFlags);
            this.credentialsHandle = SspiWrapper.AcquireCredentialsHandle(
                SecurityPackage,
                CredentialUse.Inbound,
                secureCredential
                );
        }

        private void Dispose(bool disposing)
        {
            lock (this.syncObject)
            {
                if (this.disposed == false)
                {
                    this.disposed = true;
                    if (disposing)
                    {
                        if (this.securityContext != null)
                        {
                            this.securityContext.Close();
                            this.securityContext = null;
                        }
                        if (this.credentialsHandle != null)
                        {
                            this.credentialsHandle.Close();
                            this.credentialsHandle = null;
                        }
                    }

                    // set to null any references that aren't finalizable
                    this.connectionInfo = null;
                    this.destination = null;
                    this.streamSizes = null;
                }
            }
        }

        private SafeFreeCertContext ExtractCertificateHandle(ContextAttribute contextAttribute)
        {
            SafeFreeCertContext result = SspiWrapper.QueryContextAttributes(this.securityContext, contextAttribute) as SafeFreeCertContext;
            return result;
        }

        //This method extracts a remote certificate and chain upon request.
        private void ExtractRemoteCertificate()
        {
            SafeFreeCertContext remoteContext = null;
            this.remoteCertificate = null;
            this.remoteCertificateChain = null;
            try
            {
                remoteContext = ExtractCertificateHandle(ContextAttribute.RemoteCertificate);
                if (remoteContext != null && !remoteContext.IsInvalid)
                {
                    this.remoteCertificateChain = UnmanagedCertificateContext.GetStore(remoteContext);
                    this.remoteCertificate = new X509Certificate2(remoteContext.DangerousGetHandle());
                }
            }
            finally
            {
                if (remoteContext != null)
                {
                    remoteContext.Close();
                }
            }
        }

        internal bool TryGetContextIdentity(out WindowsIdentity mappedIdentity)
        {
            if (!IsValidContext)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception((int)SecurityStatus.InvalidHandle));
            }

            SafeCloseHandle token = null;
            try
            {
                SecurityStatus status = (SecurityStatus)SspiWrapper.QuerySecurityContextToken(this.securityContext, out token);
                if (status != SecurityStatus.OK)
                {
                    mappedIdentity = null;
                    return false;
                }
                mappedIdentity = new WindowsIdentity(token.DangerousGetHandle(), SecurityUtils.AuthTypeCertMap);
                return true;
            }
            finally
            {
                if (token != null)
                {
                    token.Close();
                }
            }
        }

        void OnBadData()
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.BadData)));
        }

        void ThrowIfDisposed()
        {
            lock (this.syncObject)
            {
                if (this.disposed)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ObjectDisposedException(null));
                }
            }
        }

        unsafe static class UnmanagedCertificateContext
        {

            [StructLayout(LayoutKind.Sequential)]
            private struct _CERT_CONTEXT
            {
                internal Int32 dwCertEncodingType;
                internal IntPtr pbCertEncoded;
                internal Int32 cbCertEncoded;
                internal IntPtr pCertInfo;
                internal IntPtr hCertStore;
            };

            internal static X509Certificate2Collection GetStore(SafeFreeCertContext certContext)
            {
                X509Certificate2Collection result = new X509Certificate2Collection();

                if (certContext.IsInvalid)
                    return result;

                _CERT_CONTEXT context = (_CERT_CONTEXT)Marshal.PtrToStructure(certContext.DangerousGetHandle(), typeof(_CERT_CONTEXT));

                if (context.hCertStore != IntPtr.Zero)
                {
                    X509Store store = null;
                    try
                    {
                        store = new X509Store(context.hCertStore);
                        result = store.Certificates;
                    }
                    finally
                    {
                        if (store != null)
                            store.Close();
                    }
                }
                return result;
            }
        }
    }
}
