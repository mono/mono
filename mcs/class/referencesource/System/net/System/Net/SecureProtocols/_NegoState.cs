/*++
Copyright (c) Microsoft Corporation

Module Name:

    _NegoState.cs

Abstract:
        The internal class is used by Negotiate Client&Server and by (internal) NegoStream.
        It encapsulates security context and does the real work in authentication and
        user data encryption with NEGO SSPI package.

Author:
    Alexei Vopilov    12-Aug-2003

Revision History:
    12-Aug-2003 New design that has obsoleted Authenticator class

--*/

namespace System.Net.Security {
    using System;
    using System.Net;
    using System.IO;
    using System.Security;
    using System.Security.Principal;
    using System.Threading;
    using System.ComponentModel;
    using System.Security.Authentication;
    using System.Security.Authentication.ExtendedProtection;

    //
    // The class maintains the state of the authentication process and the security context.
    // On the success it returns the remote side identites created as the result
    // of authentication.
    //
    internal class NegoState {

        private const int ERROR_TRUST_FAILURE = 1790;   //used to serialize protectionLevel or impersonationLevel mismatch error to the remote side

        private static readonly byte[]  _EmptyMessage = new byte[0];
        private static readonly AsyncCallback   _ReadCallback   = new AsyncCallback(ReadCallback);
        private static readonly AsyncCallback   _WriteCallback  = new AsyncCallback(WriteCallback);

        private Stream              _InnerStream;
        private bool                _LeaveStreamOpen;

        private Exception           _Exception;

        private StreamFramer        _Framer;
        private NTAuthentication    _Context;

        private int                 _NestedAuth;

        internal const int          c_MaxReadFrameSize   = 64*1024;
        internal const int          c_MaxWriteDataSize   = 63*1024; //we give 1k for the framing and trailer that is laways less as per SSPI.

        private  bool                       _CanRetryAuthentication;
        private  ProtectionLevel            _ExpectedProtectionLevel;
        private  TokenImpersonationLevel    _ExpectedImpersonationLevel;
        private  uint                       _WriteSequenceNumber;
        private  uint                       _ReadSequenceNumber;

        private ExtendedProtectionPolicy    _ExtendedProtectionPolicy;

        // SSPI does not send a server ack on successfull auth.
        // So, this is a state variable used to gracefully handle auth confirmation
        private bool _RemoteOk = false;

        //
        //
        //
        internal NegoState(Stream innerStream, bool leaveStreamOpen) {
            if (innerStream==null) {
                throw new ArgumentNullException("stream");
            }
            _InnerStream = innerStream;
            _LeaveStreamOpen = leaveStreamOpen;
        }
        //
        internal static string DefaultPackage {
            get {
                return NegotiationInfoClass.Negotiate;
            }
        }
        //
        //
        //
        internal void ValidateCreateContext(string package,
                                            NetworkCredential credential,
                                            string servicePrincipalName,
                                            ExtendedProtectionPolicy policy,
                                            ProtectionLevel protectionLevel,
                                            TokenImpersonationLevel impersonationLevel)
        {
            if (policy != null)
            {
                if (!AuthenticationManager.OSSupportsExtendedProtection)
                {
                    if (policy.PolicyEnforcement == PolicyEnforcement.Always)
                    {
                        throw new PlatformNotSupportedException(SR.GetString(SR.security_ExtendedProtection_NoOSSupport));
                    }
                }
                else
                {
                    // One of these must be set if EP is turned on
                    if (policy.CustomChannelBinding == null && policy.CustomServiceNames == null)
                        throw new ArgumentException(SR.GetString(SR.net_auth_must_specify_extended_protection_scheme), "policy");
                }

                _ExtendedProtectionPolicy = policy;
            }
            else
            {
                _ExtendedProtectionPolicy = new ExtendedProtectionPolicy(PolicyEnforcement.Never);
            }

            ValidateCreateContext(package, true, credential, servicePrincipalName, _ExtendedProtectionPolicy.CustomChannelBinding, protectionLevel, impersonationLevel);
        }
        //
        internal void ValidateCreateContext(
                                            string package,
                                            bool isServer,
                                            NetworkCredential credential,
                                            string servicePrincipalName,
                                            ChannelBinding channelBinding,
                                            ProtectionLevel         protectionLevel,
                                            TokenImpersonationLevel impersonationLevel
                                            )
        {

            if (_Exception != null && !_CanRetryAuthentication) {
                throw _Exception;
            }

            if (_Context != null && _Context.IsValidContext) {
                throw new InvalidOperationException(SR.GetString(SR.net_auth_reauth));
            }

            if (credential == null) {
                throw new ArgumentNullException("credential");
            }

            if (servicePrincipalName == null) {
                throw new ArgumentNullException("servicePrincipalName");
            }

            if (impersonationLevel != TokenImpersonationLevel.Identification &&
                impersonationLevel != TokenImpersonationLevel.Impersonation &&
                impersonationLevel != TokenImpersonationLevel.Delegation)
            {
                throw new ArgumentOutOfRangeException("impersonationLevel", impersonationLevel.ToString(), SR.GetString(SR.net_auth_supported_impl_levels));
            }

            if (_Context != null && IsServer != isServer) {
                throw new InvalidOperationException(SR.GetString(SR.net_auth_client_server));
            }

            _Exception = null;
            _RemoteOk = false;
            _Framer = new StreamFramer(_InnerStream);
            _Framer.WriteHeader.MessageId = FrameHeader.HandshakeId;

            _ExpectedProtectionLevel    = protectionLevel;
            _ExpectedImpersonationLevel = isServer? impersonationLevel: TokenImpersonationLevel.None;
            _WriteSequenceNumber        = 0;
            _ReadSequenceNumber         = 0;

            ContextFlags flags = ContextFlags.Connection;

            // A workaround for the client when talking to Win9x on the server side
            if (protectionLevel == ProtectionLevel.None && !isServer)
            {
                package = NegotiationInfoClass.NTLM;
            }

            else if (protectionLevel == ProtectionLevel.EncryptAndSign)
            {
                flags |= ContextFlags.Confidentiality;
            }
            else if (protectionLevel == ProtectionLevel.Sign)
            {
                // Assuming user expects NT4 SP4 and above
                flags |= ContextFlags.ReplayDetect | ContextFlags.SequenceDetect | ContextFlags.InitIntegrity;
            }

            if (isServer)
            {
                if (_ExtendedProtectionPolicy.PolicyEnforcement == PolicyEnforcement.WhenSupported) { flags |= ContextFlags.AllowMissingBindings; }
                if (_ExtendedProtectionPolicy.PolicyEnforcement != PolicyEnforcement.Never &&
                    _ExtendedProtectionPolicy.ProtectionScenario == ProtectionScenario.TrustedProxy)
                {
                    flags |= ContextFlags.ProxyBindings;
                }
            }
            else
            {
                // According to lzhu server side should not request any of these flags
                if (protectionLevel != ProtectionLevel.None)                        {flags |= ContextFlags.MutualAuth;}
                if (impersonationLevel == TokenImpersonationLevel.Identification)   {flags |= ContextFlags.InitIdentify;}
                if (impersonationLevel == TokenImpersonationLevel.Delegation)       {flags |= ContextFlags.Delegate;}
            }
            

            _CanRetryAuthentication = false;

            //
            // Security: We used to rely on NetworkCredential class to demand permission
            //           Switched over to explicit ControlPrincipalPermission demand (except for DefaultCredential case)
            //           The mitigated attack is brute-force pasword guessing through SSPI.
            if (!(credential is SystemNetworkCredential))
                ExceptionHelper.ControlPrincipalPermission.Demand();

            try {
                //
                _Context = new NTAuthentication(isServer, package, credential, servicePrincipalName, flags, channelBinding);
            }
            catch (Win32Exception e)
            {
                throw new AuthenticationException(SR.GetString(SR.net_auth_SSPI), e);
            }
        }

        //
        //
        private Exception SetException(Exception e)
        {
            if (_Exception == null || !(_Exception is ObjectDisposedException))
            {
                _Exception = e;
            }
            if (_Exception != null && _Context != null) {
                _Context.CloseContext();
            }
            return _Exception;
        }

        //
        // General informational properties
        //

        //
        //
        internal bool IsAuthenticated {
            get {
                return _Context != null && HandshakeComplete && _Exception == null && _RemoteOk;
            }
        }
        //
        //
        //
        internal  bool IsMutuallyAuthenticated {
            get {
                if (!IsAuthenticated)
                    return false;

                // Suppressing for NTLM since SSPI does not return correct value int the context flags.
                if (_Context.IsNTLM)
                    return false;

                return _Context.IsMutualAuthFlag;
            }
        }
        //
        internal  bool IsEncrypted {
            get {
                return IsAuthenticated && _Context.IsConfidentialityFlag;
            }
        }
        //
        internal  bool IsSigned {
            get {
                return IsAuthenticated && (_Context.IsIntegrityFlag || _Context.IsConfidentialityFlag);
            }
        }
        //
        internal bool IsServer {
            get {
                return _Context != null && _Context.IsServer;
            }
        }
        //
        // NEGO specific informational properties
        //

        internal bool CanGetSecureStream {
            get {
                return (_Context.IsConfidentialityFlag || _Context.IsIntegrityFlag);
            }
        }
        //
        //
        //
        internal TokenImpersonationLevel AllowedImpersonation {
            get {
                CheckThrow(true);
                return PrivateImpersonationLevel;
            }
        }
        //
        private TokenImpersonationLevel PrivateImpersonationLevel {
            get {
                // according to lzhu we should suppress dlegate flag in NTLM case
                return  (_Context.IsDelegationFlag && _Context.ProtocolName != NegotiationInfoClass.NTLM) ? TokenImpersonationLevel.Delegation
                        :_Context.IsIdentifyFlag?   TokenImpersonationLevel.Identification
                        :TokenImpersonationLevel.Impersonation;
            }
        }
        //
        //
        //
        private bool HandshakeComplete {
            get {
                return _Context.IsCompleted && _Context.IsValidContext;
            }
        }
        //
        // Note that method will demand PrincipalControlPermission
        // which essentially means demanding full trust
        //
        internal IIdentity GetIdentity() {

            CheckThrow(true);

            IIdentity result = null;
            string name = _Context.IsServer? _Context.AssociatedName: _Context.Spn;
            string protocol = "NTLM";

            protocol = _Context.ProtocolName;

            if (_Context.IsServer) {
                SafeCloseHandle token = null;
                try {
                    token = _Context.GetContextToken();
                    string authtype = _Context.ProtocolName;
                    result = new WindowsIdentity(token.DangerousGetHandle(), authtype, WindowsAccountType.Normal, true);
                    return result;
                }
                catch (SecurityException) {
                    //ignore and construct generic Identity if failed due to security problem
                }
                finally {
                    if (token != null) {
                        token.Close();
                    }
                }
            }
            // on the client we don't have access to the remote side identity.
            result = new GenericIdentity(name, protocol);
            return result;
        }

        //
        // Methods
        //

        //
        //
        internal void CheckThrow(bool authSucessCheck) {
            if (_Exception != null) {
                throw _Exception;
            }
            if (authSucessCheck && !IsAuthenticated) {
                throw new InvalidOperationException(SR.GetString(SR.net_auth_noauth));
            }
        }
        //
        // This is to not depend on GC&SafeHandle class if the context is not needed anymore.
        //
        internal void Close() {
            // Mark this instance as disposed
            _Exception = new ObjectDisposedException("NegotiateStream");
            if (_Context != null) {
                _Context.CloseContext();
            }
        }
        //
        //
        //
        internal void ProcessAuthentication(LazyAsyncResult lazyResult)
        {
            CheckThrow(false);
            if (Interlocked.Exchange(ref _NestedAuth, 1) == 1) {
                throw new InvalidOperationException(SR.GetString(SR.net_io_invalidnestedcall, lazyResult==null?"BeginAuthenticate":"Authenticate", "authenticate"));
            }

            try {
                if (_Context.IsServer)
                {
                    // Listen for a client blob
                    StartReceiveBlob(lazyResult);
                }
                else
                {
                    // we start with the first blob
                    StartSendBlob(null, lazyResult);
                }
            }
            catch (Exception e)
            {
                // Roundtrip it through the SetException()
                e = SetException(e);
                throw;
            }
            finally
            {
                if (lazyResult == null || _Exception != null) {
                    _NestedAuth = 0;
                }
            }
        }
        //
        //
        //
        internal void EndProcessAuthentication(IAsyncResult result)
        {
            if (result == null)
            {
                throw new ArgumentNullException("asyncResult");
            }

            LazyAsyncResult lazyResult = result as LazyAsyncResult;
            if (lazyResult == null)
            {
                throw new ArgumentException(SR.GetString(SR.net_io_async_result, result.GetType().FullName), "asyncResult");
            }

            if (Interlocked.Exchange(ref _NestedAuth, 0) == 0)
            {
                throw new InvalidOperationException(SR.GetString(SR.net_io_invalidendcall, "EndAuthenticate"));
            }

            // No "artificial" timeouts implemented so far, InnerStream controls that.
            lazyResult.InternalWaitForCompletion();

            Exception e = lazyResult.Result as Exception;

            if (e != null)
            {
                // Roundtrip it through the SetException()
                e = SetException(e);
                throw e;
            }

        }

        private bool CheckSpn()
        {
            if (_Context.IsKerberos)
            {
                return true;
            }

            if (_ExtendedProtectionPolicy.PolicyEnforcement == PolicyEnforcement.Never ||
                    _ExtendedProtectionPolicy.CustomServiceNames == null)
            {
                return true;
            }

            if (!AuthenticationManager.OSSupportsExtendedProtection)
            {
                GlobalLog.Assert(_ExtendedProtectionPolicy.PolicyEnforcement != PolicyEnforcement.Always, 
                    "User managed to set PolicyEnforcement.Always when the OS does not support extended protection!");
                return true;
            }

            string clientSpn = _Context.ClientSpecifiedSpn;

            if (String.IsNullOrEmpty(clientSpn))
            {
                if (_ExtendedProtectionPolicy.PolicyEnforcement == PolicyEnforcement.WhenSupported)
                {
                    return true;
                }
            }
            else
            {
                return _ExtendedProtectionPolicy.CustomServiceNames.Contains(clientSpn);
            }

            return false;
        }

        //
        // Client side starts here, but server also loops through this method
        //
        private void StartSendBlob(byte[] message, LazyAsyncResult lazyResult)
        {
            Win32Exception win32exception = null;
            if (message != _EmptyMessage)
            {
                message = GetOutgoingBlob(message, ref win32exception);
            }

            if (win32exception != null)
            {
                // signal remote side on a failed attempt
                StartSendAuthResetSignal(lazyResult, message, win32exception);
                return;
            }

            if (HandshakeComplete)
            {
                if (_Context.IsServer && !CheckSpn())
                {
                    Exception exception = new AuthenticationException(SR.GetString(SR.net_auth_bad_client_creds_or_target_mismatch));
                    int statusCode = ERROR_TRUST_FAILURE;
                    message = new byte[8];  //sizeof(long)

                    for (int i = message.Length - 1; i >= 0; --i)
                    {
                        message[i] = (byte)(statusCode & 0xFF);
                        statusCode = (int)((uint)statusCode >> 8);
                    }

                    StartSendAuthResetSignal(lazyResult, message, exception);
                    return;
                }

                if (PrivateImpersonationLevel < _ExpectedImpersonationLevel)
                {
                    Exception exception = new AuthenticationException(SR.GetString(SR.net_auth_context_expectation, _ExpectedImpersonationLevel.ToString(), PrivateImpersonationLevel.ToString()));
                    int statusCode = ERROR_TRUST_FAILURE;
                    message = new byte[8];  //sizeof(long)

                    for (int i = message.Length-1; i >= 0; --i)
                    {
                        message[i] = (byte)(statusCode & 0xFF);
                        statusCode = (int) ((uint) statusCode >> 8);
                    }

                    StartSendAuthResetSignal(lazyResult, message, exception);
                    return;
                }

                ProtectionLevel result = _Context.IsConfidentialityFlag? ProtectionLevel.EncryptAndSign: _Context.IsIntegrityFlag? ProtectionLevel.Sign: ProtectionLevel.None;

                if (result < _ExpectedProtectionLevel)
                {
                    Exception exception = new AuthenticationException(SR.GetString(SR.net_auth_context_expectation, result.ToString(), _ExpectedProtectionLevel.ToString()));
                    int statusCode = ERROR_TRUST_FAILURE;
                    message = new byte[8];  //sizeof(long)

                    for (int i = message.Length-1; i >= 0; --i)
                    {
                        message[i] = (byte)(statusCode & 0xFF);
                        statusCode = (int) ((uint) statusCode >> 8);
                    }

                    StartSendAuthResetSignal(lazyResult, message, exception);
                    return;
                }

                // Signal remote party that we are done
                _Framer.WriteHeader.MessageId = FrameHeader.HandshakeDoneId;
                if (_Context.IsServer)
                {
                    // Server may complete now because client SSPI would not complain at this point.
                    _RemoteOk = true;

                    // However the client will wait for server to send this ACK
                    //Force signalling server OK to the client
                    if (message == null)
                    {
                        message = _EmptyMessage;
                    }
                }
            }
            else if (message == null || message == _EmptyMessage) {
                throw new InternalException();
            }

            if (message != null)
            {
                //even if we are comleted, there could be a blob for sending.
                if (lazyResult == null)
                {
                    _Framer.WriteMessage(message);
                }
                else
                {
                    IAsyncResult ar = _Framer.BeginWriteMessage(message, _WriteCallback, lazyResult);
                    if (!ar.CompletedSynchronously)
                    {
                        return;
                    }
                    _Framer.EndWriteMessage(ar);
                }
            }
            CheckCompletionBeforeNextReceive(lazyResult);
        }
        //
        // This will check and logically complete the auth handshake
        //
        private void CheckCompletionBeforeNextReceive(LazyAsyncResult lazyResult)
        {
            if (HandshakeComplete && _RemoteOk)
            {
                //we are done with success
                if (lazyResult != null)
                {
                    lazyResult.InvokeCallback();
                }
                return;
            }
            StartReceiveBlob(lazyResult);
        }
        //
        // Server side starts here, but client also loops through this method
        //
        private void StartReceiveBlob(LazyAsyncResult lazyResult)
        {
            byte[] message;
            if (lazyResult == null)
            {
                message = _Framer.ReadMessage();
            }
            else
            {
                IAsyncResult ar = _Framer.BeginReadMessage(_ReadCallback, lazyResult);
                if (!ar.CompletedSynchronously)
                {
                    return;
                }
                message = _Framer.EndReadMessage(ar);
            }
            ProcessReceivedBlob(message, lazyResult);
        }
        //
        //
        //
        private void ProcessReceivedBlob(byte[] message, LazyAsyncResult lazyResult)
        {
            // This is an EOF otherwise we would get at least *empty* message but not a null one.
            if (message == null)
            {
                throw new AuthenticationException(SR.GetString(SR.net_auth_eof), null);
            }

            //Process Header information
            if (_Framer.ReadHeader.MessageId == FrameHeader.HandshakeErrId)
            {
                Win32Exception e = null;
                if (message.Length >= 8)    // sizeof(long)
                {
                    // Try to recover remote win32 Exception
                    long error = 0;
                    for (int i = 0; i < 8; ++i)
                        error = (error<<8) + message[i];
                    e = new Win32Exception((int)error);
                }
                if (e != null)
                {
                     if (e.NativeErrorCode == (int)SecurityStatus.LogonDenied)
                        throw new InvalidCredentialException(SR.GetString(SR.net_auth_bad_client_creds), e);

                     if (e.NativeErrorCode == ERROR_TRUST_FAILURE)
                         throw new AuthenticationException(SR.GetString(SR.net_auth_context_expectation_remote), e);
                 }

                throw new AuthenticationException(SR.GetString(SR.net_auth_alert), e);
            }

            if (_Framer.ReadHeader.MessageId == FrameHeader.HandshakeDoneId)
            {
                _RemoteOk = true;
            }
            else if (_Framer.ReadHeader.MessageId != FrameHeader.HandshakeId)
            {
                throw new AuthenticationException(SR.GetString(SR.net_io_header_id, "MessageId", _Framer.ReadHeader.MessageId, FrameHeader.HandshakeId), null);
            }
            CheckCompletionBeforeNextSend(message, lazyResult);
        }
        //
        // This will check and logically complete the auth handshake
        //
        private void CheckCompletionBeforeNextSend(byte[] message, LazyAsyncResult lazyResult)
        {
            //If we are done don't go into send
            if (HandshakeComplete)
            {
                if (!_RemoteOk)
                {
                    throw new AuthenticationException(SR.GetString(SR.net_io_header_id, "MessageId", _Framer.ReadHeader.MessageId, FrameHeader.HandshakeDoneId), null);
                }
                if (lazyResult != null)
                {
                    lazyResult.InvokeCallback();
                }
                return;
            }

            // Not yet done, get a new blob and send it if any
            StartSendBlob(message, lazyResult);
        }
        //
        //  This is to reset auth state on remote side.
        //  If this write succeeds we will allow auth retrying.
        //
        private void StartSendAuthResetSignal(LazyAsyncResult lazyResult, byte[] message, Exception exception)
        {
            _Framer.WriteHeader.MessageId = FrameHeader.HandshakeErrId;

            Win32Exception win32exception = exception as Win32Exception;

            if (win32exception != null && win32exception.NativeErrorCode == (int)SecurityStatus.LogonDenied)
                if (IsServer)
                    exception = new InvalidCredentialException(SR.GetString(SR.net_auth_bad_client_creds), exception);
                else
                    exception = new InvalidCredentialException(SR.GetString(SR.net_auth_bad_client_creds_or_target_mismatch), exception);

            if (!(exception is AuthenticationException))
                exception = new AuthenticationException(SR.GetString(SR.net_auth_SSPI), exception);

            if (lazyResult == null)
            {
                _Framer.WriteMessage(message);
            }
            else
            {
                lazyResult.Result = exception;
                IAsyncResult ar = _Framer.BeginWriteMessage(message, _WriteCallback, lazyResult);
                if(!ar.CompletedSynchronously)
                {
                    return;
                }
                _Framer.EndWriteMessage(ar);
            }

            _CanRetryAuthentication = true;
            throw exception;
        }
        //
        //
        //
        private static void WriteCallback(IAsyncResult transportResult)
        {
            GlobalLog.Assert(transportResult.AsyncState is LazyAsyncResult, "WriteCallback|State type is wrong, expected LazyAsyncResult.");
            if (transportResult.CompletedSynchronously)
            {
                return;
            }

            LazyAsyncResult lazyResult = (LazyAsyncResult) transportResult.AsyncState;

            // Async completion
            try
            {
                NegoState authState = (NegoState)lazyResult.AsyncObject;
                authState._Framer.EndWriteMessage(transportResult);

                //special case for an error notification
                if (lazyResult.Result is Exception)
                {
                    authState._CanRetryAuthentication = true;
                    throw (Exception)lazyResult.Result;
                }
                authState.CheckCompletionBeforeNextReceive(lazyResult);
            }
            catch (Exception e)
            {
                if (lazyResult.InternalPeekCompleted) {
                    // This will throw on a worker thread.
                    throw;
                }
                lazyResult.InvokeCallback(e);
            }
        }
        //
        //
        //
        private static void ReadCallback(IAsyncResult transportResult)
        {
            GlobalLog.Assert(transportResult.AsyncState is LazyAsyncResult, "ReadCallback|State type is wrong, expected LazyAsyncResult.");
            if (transportResult.CompletedSynchronously)
            {
                return;
            }

            LazyAsyncResult lazyResult = (LazyAsyncResult) transportResult.AsyncState;

            // Async completion
            try
            {
                NegoState authState = (NegoState)lazyResult.AsyncObject;
                byte[] message = authState._Framer.EndReadMessage(transportResult);
                authState.ProcessReceivedBlob(message, lazyResult);
            }
            catch (Exception e)
            {
                if (lazyResult.InternalPeekCompleted) {
                    // This will throw on a worker thread.
                    throw;
                }

                lazyResult.InvokeCallback(e);
            }
        }
        //
        //
        //
        private unsafe byte[] GetOutgoingBlob(byte[] incomingBlob, ref Win32Exception e) {

            SecurityStatus statusCode;
            byte[] message = _Context.GetOutgoingBlob(incomingBlob, false, out statusCode);

            if (((int) statusCode & unchecked((int) 0x80000000)) != 0)
            {
                e = new System.ComponentModel.Win32Exception((int) statusCode);

                message = new byte[8];  //sizeof(long)
                for (int i = message.Length-1; i >= 0; --i)
                {
                    message[i] = (byte) ((uint) statusCode & 0xFF);
                    statusCode = (SecurityStatus) ((uint) statusCode >> 8);
                }
            }

            if (message != null && message.Length == 0) {
                message = _EmptyMessage;
            }
            return message;
        }
        //
        //
        //
        internal int EncryptData(byte[] buffer, int offset, int count, ref byte[] outBuffer)
        {
            CheckThrow(true);
            //
            // Well, this is to play by the rules but in reality SSPI seems to ignore this sequence number.
            // Means we could simply pass 0
            //
            ++_WriteSequenceNumber;
            return _Context.Encrypt(buffer, offset, count, ref outBuffer, _WriteSequenceNumber);
        }
        //
        //
        //
        internal int DecryptData(byte[] buffer, int offset, int count, out int newOffset)
        {
            CheckThrow(true);
            //
            // Well, this is to play by the rules but in reality SSPI seems to ignore this sequence number.
            // Means we could simply pass 0
            //
            ++_ReadSequenceNumber;
            return _Context.Decrypt(buffer, offset, count, out newOffset, _ReadSequenceNumber);
        }
    }

}
