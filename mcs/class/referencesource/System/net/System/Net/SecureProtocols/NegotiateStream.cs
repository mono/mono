/*++
Copyright (c) 2003 Microsoft Corporation

Module Name:

    NegotiateStream.cs

Abstract:

    A public implementation an authenticated stream based on NEGO SSP

        The class that can be used by client and server side applications
        - to transfer Identities across the stream
        - to ecnrypt data based on NEGO SSP package

        In most cases the innerStream will be of type NetworkStream.
        On Win9x data encryption is not available and both sides have
        to explicitly drop SecurityLevel and MuatualAuth requirements.

        This is a simple wrapper class.
        All real work is done by internal NegoState class and partial implementaion in _NegoStream.cs

Author:
    Alexei Vopilov    Sept 28-2003

Revision History:

--*/
namespace System.Net.Security {
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Authentication.ExtendedProtection;
using System.Security.Permissions;
using System.Security.Principal;

    //
    // Negotiate
    //
    public partial class NegotiateStream: AuthenticatedStream
    {
        private  NegoState _NegoState;
        private  string     _Package;
        private  IIdentity  _RemoteIdentity;

        public NegotiateStream(Stream innerStream): this(innerStream, false)
        {
        }

        public NegotiateStream(Stream innerStream,  bool leaveInnerStreamOpen): base(innerStream,  leaveInnerStreamOpen)
        {
#if DEBUG
            using (GlobalLog.SetThreadKind(ThreadKinds.User)) {
#endif
            _NegoState = new NegoState(innerStream, leaveInnerStreamOpen);
            _Package = NegoState.DefaultPackage;
            InitializeStreamPart();
#if DEBUG
            }
#endif
        }

        //
        // Client side auth
        //
        public virtual void AuthenticateAsClient()
        {
            AuthenticateAsClient((NetworkCredential)CredentialCache.DefaultCredentials, null, string.Empty, ProtectionLevel.EncryptAndSign, TokenImpersonationLevel.Identification);
        }
        public virtual void AuthenticateAsClient(NetworkCredential credential, string targetName)
        {
            AuthenticateAsClient(credential, null, targetName, ProtectionLevel.EncryptAndSign, TokenImpersonationLevel.Identification);
        }
        public virtual void AuthenticateAsClient(NetworkCredential credential, ChannelBinding binding, string targetName)
        {
            AuthenticateAsClient(credential, binding, targetName, ProtectionLevel.EncryptAndSign, TokenImpersonationLevel.Identification);
        }

        //
        public virtual void AuthenticateAsClient( NetworkCredential       credential,
                                                string                  targetName,
                                                ProtectionLevel         requiredProtectionLevel,   //this will be the ultimate result or exception
                                                TokenImpersonationLevel allowedImpersonationLevel) //this OR LOWER will be ultimate result in auth context
        {
            AuthenticateAsClient(credential, null, targetName, requiredProtectionLevel, allowedImpersonationLevel);
        }
        //
        public virtual void AuthenticateAsClient(NetworkCredential          credential,
                                                 ChannelBinding binding,
                                                 string                     targetName,
                                                 ProtectionLevel            requiredProtectionLevel,
                                                 TokenImpersonationLevel    allowedImpersonationLevel)
        {
#if DEBUG
            using (GlobalLog.SetThreadKind(ThreadKinds.User | ThreadKinds.Sync)) {
#endif
            _NegoState.ValidateCreateContext(_Package, false, credential, targetName, binding, requiredProtectionLevel, allowedImpersonationLevel);
            _NegoState.ProcessAuthentication(null);
#if DEBUG
            }
#endif
        }
        //
        [HostProtection(ExternalThreading=true)]
        public virtual IAsyncResult BeginAuthenticateAsClient(AsyncCallback asyncCallback, object asyncState)
        {
            return BeginAuthenticateAsClient((NetworkCredential)CredentialCache.DefaultCredentials, null, string.Empty,
                                           ProtectionLevel.EncryptAndSign, TokenImpersonationLevel.Identification,
                                           asyncCallback, asyncState);
        }
        //
        [HostProtection(ExternalThreading=true)]
        public virtual IAsyncResult BeginAuthenticateAsClient(NetworkCredential credential, string targetName, AsyncCallback asyncCallback, object asyncState)
        {
            return BeginAuthenticateAsClient(credential, null, targetName,
                                           ProtectionLevel.EncryptAndSign, TokenImpersonationLevel.Identification,
                                           asyncCallback, asyncState);
        }
        //
        [HostProtection(ExternalThreading = true)]
        public virtual IAsyncResult BeginAuthenticateAsClient(NetworkCredential credential, ChannelBinding binding, string targetName, AsyncCallback asyncCallback, object asyncState)
        {
            return BeginAuthenticateAsClient(credential, binding, targetName,
                                             ProtectionLevel.EncryptAndSign, TokenImpersonationLevel.Identification,
                                             asyncCallback, asyncState);
        }
        //
        [HostProtection(ExternalThreading=true)]
        public virtual IAsyncResult BeginAuthenticateAsClient(NetworkCredential       credential,
                                                            string                  targetName,
                                                            ProtectionLevel         requiredProtectionLevel,           //this will be ultimatelly the result or exception
                                                            TokenImpersonationLevel allowedImpersonationLevel, //this OR LOWER will be ultimate result in auth context
                                                            AsyncCallback           asyncCallback,
                                                            object                  asyncState)
        {
            return BeginAuthenticateAsClient(credential, null, targetName,
                                             requiredProtectionLevel, allowedImpersonationLevel,
                                             asyncCallback, asyncState);
        }
        //
        [HostProtection(ExternalThreading = true)]
        public virtual IAsyncResult BeginAuthenticateAsClient(NetworkCredential         credential,
                                                              ChannelBinding            binding,
                                                              string                    targetName,
                                                              ProtectionLevel           requiredProtectionLevel,
                                                              TokenImpersonationLevel   allowedImpersonationLevel,
                                                              AsyncCallback             asyncCallback,
                                                              object                    asyncState)
        {
#if DEBUG
            using (GlobalLog.SetThreadKind(ThreadKinds.User | ThreadKinds.Async)) {
#endif
            _NegoState.ValidateCreateContext(_Package, false, credential, targetName, binding, requiredProtectionLevel, allowedImpersonationLevel);

            LazyAsyncResult result = new LazyAsyncResult(_NegoState, asyncState, asyncCallback);
            _NegoState.ProcessAuthentication(result);

            return result;
#if DEBUG
            }
#endif
        }
        //
        public virtual void EndAuthenticateAsClient(IAsyncResult asyncResult)
        {
#if DEBUG
            using (GlobalLog.SetThreadKind(ThreadKinds.User)) {
#endif
            _NegoState.EndProcessAuthentication(asyncResult);
#if DEBUG
            }
#endif
        }
        //
        //
        //Server side Authenticate
        //
        public virtual void AuthenticateAsServer()
        {
            AuthenticateAsServer((NetworkCredential)CredentialCache.DefaultCredentials, null, ProtectionLevel.EncryptAndSign, TokenImpersonationLevel.Identification);
        }
        //
        public virtual void AuthenticateAsServer(ExtendedProtectionPolicy policy)
        {
            AuthenticateAsServer((NetworkCredential)CredentialCache.DefaultCredentials, policy, ProtectionLevel.EncryptAndSign, TokenImpersonationLevel.Identification);
        }
        //
        public virtual void AuthenticateAsServer( NetworkCredential        credential,
                                                ProtectionLevel          requiredProtectionLevel,                 //throw if the result is below than this
                                                TokenImpersonationLevel  requiredImpersonationLevel)             //throw if the result is below than this
        {
            AuthenticateAsServer(credential, null, requiredProtectionLevel, requiredImpersonationLevel);
        }
        //
        public virtual void AuthenticateAsServer(NetworkCredential          credential,
                                                 ExtendedProtectionPolicy   policy,
                                                 ProtectionLevel            requiredProtectionLevel,
                                                 TokenImpersonationLevel    requiredImpersonationLevel)
        {
#if DEBUG
            using (GlobalLog.SetThreadKind(ThreadKinds.User | ThreadKinds.Sync)) {
#endif
            _NegoState.ValidateCreateContext(_Package, credential, string.Empty, policy, requiredProtectionLevel, requiredImpersonationLevel);

            _NegoState.ProcessAuthentication(null);
#if DEBUG
            }
#endif
        }
        //
        [HostProtection(ExternalThreading=true)]
        public virtual IAsyncResult BeginAuthenticateAsServer(AsyncCallback asyncCallback, object asyncState)
        {
            return BeginAuthenticateAsServer((NetworkCredential)CredentialCache.DefaultCredentials, null, ProtectionLevel.EncryptAndSign, TokenImpersonationLevel.Identification, asyncCallback, asyncState);
        }
        //
        [HostProtection(ExternalThreading=true)]
        public virtual IAsyncResult BeginAuthenticateAsServer(ExtendedProtectionPolicy policy, AsyncCallback asyncCallback, object asyncState)
        {
            return BeginAuthenticateAsServer((NetworkCredential)CredentialCache.DefaultCredentials, policy, ProtectionLevel.EncryptAndSign, TokenImpersonationLevel.Identification, asyncCallback, asyncState);
        }
        //
        [HostProtection(ExternalThreading=true)]
        public virtual IAsyncResult BeginAuthenticateAsServer(NetworkCredential   credential,
                                                            ProtectionLevel     requiredProtectionLevel,                 //throw if the result is below than this
                                                            TokenImpersonationLevel  requiredImpersonationLevel,        //throw if the result is below than this
                                                            AsyncCallback       asyncCallback,
                                                            object              asyncState)
        {
            return BeginAuthenticateAsServer(credential, null, requiredProtectionLevel, requiredImpersonationLevel, asyncCallback, asyncState);
        }
        //
        [HostProtection(ExternalThreading=true)]
        public virtual IAsyncResult BeginAuthenticateAsServer(NetworkCredential   credential,
                                                              ExtendedProtectionPolicy policy,
                                                              ProtectionLevel     requiredProtectionLevel,                 //throw if the result is below than this
                                                              TokenImpersonationLevel  requiredImpersonationLevel,        //throw if the result is below than this
                                                              AsyncCallback       asyncCallback,
                                                              object              asyncState)
        {
#if DEBUG
            using (GlobalLog.SetThreadKind(ThreadKinds.User | ThreadKinds.Async)) {
#endif
            _NegoState.ValidateCreateContext(_Package, credential, string.Empty, policy, requiredProtectionLevel, requiredImpersonationLevel);

            LazyAsyncResult result = new LazyAsyncResult(_NegoState, asyncState, asyncCallback);
            _NegoState.ProcessAuthentication(result);

            return result;
#if DEBUG
            }
#endif
        }
        //
        public virtual void EndAuthenticateAsServer(IAsyncResult asyncResult)
        {
#if DEBUG
            using (GlobalLog.SetThreadKind(ThreadKinds.User)) {
#endif
            _NegoState.EndProcessAuthentication(asyncResult);
#if DEBUG
            }
#endif
        }

        //************* Task-based async public methods *************************
        [HostProtection(ExternalThreading = true)]
        public virtual Task AuthenticateAsClientAsync()
        {
            return Task.Factory.FromAsync(BeginAuthenticateAsClient, EndAuthenticateAsClient, null);
        }

        [HostProtection(ExternalThreading=true)]
        public virtual Task AuthenticateAsClientAsync(NetworkCredential credential, string targetName)
        {
            return Task.Factory.FromAsync(BeginAuthenticateAsClient, EndAuthenticateAsClient, credential, targetName, null);
        }

        [HostProtection(ExternalThreading=true)]
        public virtual Task AuthenticateAsClientAsync(
            NetworkCredential credential, string targetName,
            ProtectionLevel requiredProtectionLevel,
            TokenImpersonationLevel allowedImpersonationLevel)
        {
            return Task.Factory.FromAsync((callback, state) => BeginAuthenticateAsClient(credential, targetName, requiredProtectionLevel, allowedImpersonationLevel, callback, state), EndAuthenticateAsClient, null);
        }

        [HostProtection(ExternalThreading = true)]
        public virtual Task AuthenticateAsClientAsync(NetworkCredential credential, ChannelBinding binding, string targetName)
        {
            return Task.Factory.FromAsync(BeginAuthenticateAsClient, EndAuthenticateAsClient, credential, binding, targetName, null);
        }

        [HostProtection(ExternalThreading=true)]
        public virtual Task AuthenticateAsClientAsync(
            NetworkCredential credential, ChannelBinding binding,
            string targetName, ProtectionLevel requiredProtectionLevel,
            TokenImpersonationLevel allowedImpersonationLevel)
        {
            return Task.Factory.FromAsync((callback, state) => BeginAuthenticateAsClient(credential, binding, targetName, requiredProtectionLevel, allowedImpersonationLevel, callback, state), EndAuthenticateAsClient, null);
        }

        [HostProtection(ExternalThreading = true)]
        public virtual Task AuthenticateAsServerAsync()
        {
            return Task.Factory.FromAsync(BeginAuthenticateAsServer, EndAuthenticateAsServer, null);
        }

        [HostProtection(ExternalThreading = true)]
        public virtual Task AuthenticateAsServerAsync(ExtendedProtectionPolicy policy)
        {
            return Task.Factory.FromAsync(BeginAuthenticateAsServer, EndAuthenticateAsServer, policy, null);
        }

        [HostProtection(ExternalThreading = true)]
        public virtual Task AuthenticateAsServerAsync(NetworkCredential credential, ProtectionLevel requiredProtectionLevel, TokenImpersonationLevel requiredImpersonationLevel)
        {
            return Task.Factory.FromAsync(BeginAuthenticateAsServer, EndAuthenticateAsServer, credential, requiredProtectionLevel, requiredImpersonationLevel, null);
        }

        [HostProtection(ExternalThreading = true)]
        public virtual Task AuthenticateAsServerAsync(
            NetworkCredential credential, ExtendedProtectionPolicy policy,
            ProtectionLevel requiredProtectionLevel,
            TokenImpersonationLevel requiredImpersonationLevel)
        {
            return Task.Factory.FromAsync((callback, state) => BeginAuthenticateAsServer(credential, policy, requiredProtectionLevel, requiredImpersonationLevel, callback, state), EndAuthenticateAsClient, null);
        }


        //
        // Base class properties
        //
        public override bool IsAuthenticated {
            get {
#if DEBUG
                using (GlobalLog.SetThreadKind(ThreadKinds.User | ThreadKinds.Async)) {
#endif
                return _NegoState.IsAuthenticated;
#if DEBUG
                }
#endif
            }
        }
        //
        public override bool IsMutuallyAuthenticated {
            get {
#if DEBUG
                using (GlobalLog.SetThreadKind(ThreadKinds.User | ThreadKinds.Async)) {
#endif
                return _NegoState.IsMutuallyAuthenticated;
#if DEBUG
                }
#endif
            }
        }
        //
        public override bool IsEncrypted {
            get {
#if DEBUG
                using (GlobalLog.SetThreadKind(ThreadKinds.User | ThreadKinds.Async)) {
#endif
                return _NegoState.IsEncrypted;
#if DEBUG
                }
#endif
            }
        }
        //
        public override bool IsSigned {
            get {
#if DEBUG
                using (GlobalLog.SetThreadKind(ThreadKinds.User | ThreadKinds.Async)) {
#endif
                return _NegoState.IsSigned;
#if DEBUG
                }
#endif
            }
        }
        //
        public override bool IsServer {
            get {
#if DEBUG
                using (GlobalLog.SetThreadKind(ThreadKinds.User | ThreadKinds.Async)) {
#endif
                return _NegoState.IsServer;
#if DEBUG
                }
#endif
            }
        }
        //
        // Informational NEGO properties
        //
        public virtual TokenImpersonationLevel ImpersonationLevel
        {
            get {
#if DEBUG
                using (GlobalLog.SetThreadKind(ThreadKinds.User | ThreadKinds.Async)) {
#endif
                return _NegoState.AllowedImpersonation;
#if DEBUG
                }
#endif
            }
        }
        //
        public virtual IIdentity RemoteIdentity
        {
            get {
#if DEBUG
                using (GlobalLog.SetThreadKind(ThreadKinds.User | ThreadKinds.Async)) {
#endif

                new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
                
                if (_RemoteIdentity == null)
                {   
                    _RemoteIdentity = _NegoState.GetIdentity();
                }
                return _RemoteIdentity;
#if DEBUG
                }
#endif
            }
        }

        //
        //
        // Stream contract implementation
        //
        //
        //
        public override bool CanSeek {
            get {
                return false;
            }
        }
        //
        public override bool CanRead {
            get {
                return IsAuthenticated && InnerStream.CanRead;
            }
        }
        //
        public override bool CanTimeout {
            get {
                return InnerStream.CanTimeout;
            }
        }
        //
        public override bool CanWrite {
            get {
                return IsAuthenticated && InnerStream.CanWrite;
            }
        }
        //
        //
        public override int ReadTimeout {
            get {
                return InnerStream.ReadTimeout;
            }
            set {
                InnerStream.ReadTimeout = value;
            }
        }
        //
        //
        public override int WriteTimeout {
            get {
                return InnerStream.WriteTimeout;
            }
            set {
                InnerStream.WriteTimeout = value;
            }
        }
        //
        public override long Length {
            get {
                return InnerStream.Length;
            }
        }
        //
        public override long Position {
            get {
                return InnerStream.Position;
            }
            set {
                throw new NotSupportedException(SR.GetString(SR.net_noseek));
            }
        }
        //
        public override void SetLength(long value) {
            InnerStream.SetLength(value);
        }
        //
        public override long Seek(long offset, SeekOrigin origin) {
            throw new NotSupportedException(SR.GetString(SR.net_noseek));
        }

        // Should this not block?
        public override void Flush() {
#if DEBUG
            using (GlobalLog.SetThreadKind(ThreadKinds.User | ThreadKinds.Sync)) {
#endif
            InnerStream.Flush();
#if DEBUG
            }
#endif
        }
        //
        //
        protected override void Dispose(bool disposing) {
#if DEBUG
            using (GlobalLog.SetThreadKind(ThreadKinds.User)) {
#endif
                try {
                    _NegoState.Close();
                }
                finally {
                    base.Dispose(disposing);
                }
#if DEBUG
            }
#endif
        }
        //
        //
        public override int Read(byte[] buffer, int offset, int count)
        {
#if DEBUG
            using (GlobalLog.SetThreadKind(ThreadKinds.User | ThreadKinds.Sync)) {
#endif
            _NegoState.CheckThrow(true);

            if (!_NegoState.CanGetSecureStream)
                return InnerStream.Read(buffer, offset, count);

            return ProcessRead(buffer, offset, count, null);
#if DEBUG
            }
#endif
        }
        //
        //
        public override void Write(byte[] buffer, int offset, int count)
        {
#if DEBUG
            using (GlobalLog.SetThreadKind(ThreadKinds.User | ThreadKinds.Sync)) {
#endif
            _NegoState.CheckThrow(true);

            if (!_NegoState.CanGetSecureStream)
            {
                InnerStream.Write(buffer, offset, count);
                return;
            }

            ProcessWrite(buffer, offset, count, null);
#if DEBUG
            }
#endif
        }
        //
        //
        [HostProtection(ExternalThreading=true)]
        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback asyncCallback, object asyncState)
        {
#if DEBUG
            using (GlobalLog.SetThreadKind(ThreadKinds.User | ThreadKinds.Async)) {
#endif
            _NegoState.CheckThrow(true);

            if (!_NegoState.CanGetSecureStream)
                return InnerStream.BeginRead(buffer, offset, count, asyncCallback, asyncState);

            BufferAsyncResult bufferResult = new BufferAsyncResult(this, buffer, offset, count, asyncState, asyncCallback);
            AsyncProtocolRequest asyncRequest = new AsyncProtocolRequest(bufferResult);
            ProcessRead(buffer, offset, count, asyncRequest );
            return bufferResult;
#if DEBUG
            }
#endif
        }
        //
        //
        public override int EndRead(IAsyncResult asyncResult)
        {
#if DEBUG
            using (GlobalLog.SetThreadKind(ThreadKinds.User)) {
#endif
            _NegoState.CheckThrow(true);

            if (!_NegoState.CanGetSecureStream)
                return InnerStream.EndRead(asyncResult);


            if (asyncResult == null)
            {
                throw new ArgumentNullException("asyncResult");
            }

            BufferAsyncResult bufferResult = asyncResult as BufferAsyncResult;
            if (bufferResult == null)
            {
                throw new ArgumentException(SR.GetString(SR.net_io_async_result, asyncResult.GetType().FullName), "asyncResult");
            }

            if (Interlocked.Exchange(ref _NestedRead, 0) == 0)
            {
                throw new InvalidOperationException(SR.GetString(SR.net_io_invalidendcall, "EndRead"));
            }

            // No "artificial" timeouts implemented so far, InnerStream controls timeout.
            bufferResult.InternalWaitForCompletion();

            if (bufferResult.Result is Exception)
            {
                if (bufferResult.Result is IOException)
                {
                    throw (Exception)bufferResult.Result;
                }
                throw new IOException(SR.GetString(SR.net_io_read), (Exception)bufferResult.Result);
            }
            return (int) bufferResult.Result;
#if DEBUG
            }
#endif
        }
        //
        //
        [HostProtection(ExternalThreading=true)]
        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback asyncCallback, object asyncState)
        {
#if DEBUG
            using (GlobalLog.SetThreadKind(ThreadKinds.User | ThreadKinds.Async)) {
#endif
            _NegoState.CheckThrow(true);

            if (!_NegoState.CanGetSecureStream)
                return InnerStream.BeginWrite(buffer, offset, count, asyncCallback, asyncState);

            BufferAsyncResult bufferResult = new BufferAsyncResult(this, buffer, offset, count, true, asyncState, asyncCallback);
            AsyncProtocolRequest asyncRequest = new AsyncProtocolRequest(bufferResult);

            ProcessWrite(buffer, offset, count, asyncRequest);
            return bufferResult;
#if DEBUG
            }
#endif
        }
        //
        //
        public override void EndWrite(IAsyncResult asyncResult)
        {
#if DEBUG
            using (GlobalLog.SetThreadKind(ThreadKinds.User)) {
#endif
            _NegoState.CheckThrow(true);

            if (!_NegoState.CanGetSecureStream)
            {
                InnerStream.EndWrite(asyncResult);
                return;
            }

            if (asyncResult == null)
            {
                throw new ArgumentNullException("asyncResult");
            }

            BufferAsyncResult bufferResult = asyncResult as BufferAsyncResult;
            if (bufferResult == null)
            {
                throw new ArgumentException(SR.GetString(SR.net_io_async_result, asyncResult.GetType().FullName), "asyncResult");
            }

            if (Interlocked.Exchange(ref _NestedWrite, 0) == 0)
            {
                throw new InvalidOperationException(SR.GetString(SR.net_io_invalidendcall, "EndWrite"));
            }

            // No "artificial" timeouts implemented so far, InnerStream controls timeout.
            bufferResult.InternalWaitForCompletion();

            if (bufferResult.Result is Exception)
            {
                if (bufferResult.Result is IOException)
                {
                    throw (Exception)bufferResult.Result;
                }
                throw new IOException(SR.GetString(SR.net_io_write), (Exception)bufferResult.Result);
            }
#if DEBUG
            }
#endif
        }
    }
}



