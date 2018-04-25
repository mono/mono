//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Security.Authentication.ExtendedProtection;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Security;
    using System.Threading;
    using System.Xml;
    using System.ServiceModel.Diagnostics.Application;

    delegate void ServerSessionPreambleCallback(ServerSessionPreambleConnectionReader serverSessionPreambleReader);
    delegate void ServerSessionPreambleDemuxCallback(ServerSessionPreambleConnectionReader serverSessionPreambleReader, ConnectionDemuxer connectionDemuxer);
    interface ISessionPreambleHandler
    {
        void HandleServerSessionPreamble(ServerSessionPreambleConnectionReader serverSessionPreambleReader,
            ConnectionDemuxer connectionDemuxer);
    }

    // reads everything we need in order to match a channel (i.e. up to the via) 
    class ServerSessionPreambleConnectionReader : InitialServerConnectionReader
    {
        ServerSessionDecoder decoder;
        byte[] connectionBuffer;
        int offset;
        int size;
        TransportSettingsCallback transportSettingsCallback;
        ServerSessionPreambleCallback callback;
        static WaitCallback readCallback;
        IConnectionOrientedTransportFactorySettings settings;
        Uri via;
        Action<Uri> viaDelegate;
        TimeoutHelper receiveTimeoutHelper;
        IConnection rawConnection;
        static AsyncCallback onValidate;

        public ServerSessionPreambleConnectionReader(IConnection connection, Action connectionDequeuedCallback,
            long streamPosition, int offset, int size, TransportSettingsCallback transportSettingsCallback,
            ConnectionClosedCallback closedCallback, ServerSessionPreambleCallback callback)
            : base(connection, closedCallback)
        {
            this.rawConnection = connection;
            this.decoder = new ServerSessionDecoder(streamPosition, MaxViaSize, MaxContentTypeSize);
            this.offset = offset;
            this.size = size;
            this.transportSettingsCallback = transportSettingsCallback;
            this.callback = callback;
            this.ConnectionDequeuedCallback = connectionDequeuedCallback;
        }

        public int BufferOffset
        {
            get { return offset; }
        }

        public int BufferSize
        {
            get { return size; }
        }

        public ServerSessionDecoder Decoder
        {
            get { return decoder; }
        }

        public IConnection RawConnection
        {
            get { return rawConnection; }
        }

        public Uri Via
        {
            get { return this.via; }
        }

        TimeSpan GetRemainingTimeout()
        {
            return this.receiveTimeoutHelper.RemainingTime();
        }

        static void ReadCallback(object state)
        {
            ServerSessionPreambleConnectionReader reader = (ServerSessionPreambleConnectionReader)state;
            bool success = false;
            try
            {
                reader.GetReadResult();
                reader.ContinueReading();
                success = true;
            }
            catch (CommunicationException exception)
            {
                DiagnosticUtility.TraceHandledException(exception, TraceEventType.Information);
            }
            catch (TimeoutException exception)
            {
                if (TD.ReceiveTimeoutIsEnabled())
                {
                    TD.ReceiveTimeout(exception.Message);
                }
                DiagnosticUtility.TraceHandledException(exception, TraceEventType.Information);
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }
                if (!ExceptionHandler.HandleTransportExceptionHelper(e))
                {
                    throw;
                }
                // containment -- all errors abort the reader, no additional containment action needed
            }
            finally
            {
                if (!success)
                {
                    reader.Abort();
                }
            }
        }

        void GetReadResult()
        {
            offset = 0;
            size = Connection.EndRead();
            if (size == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(decoder.CreatePrematureEOFException());
            }
        }

        void ContinueReading()
        {
            bool success = false;
            try
            {
                for (;;)
                {
                    if (size == 0)
                    {
                        if (readCallback == null)
                        {
                            readCallback = new WaitCallback(ReadCallback);
                        }

                        if (Connection.BeginRead(0, connectionBuffer.Length, GetRemainingTimeout(), readCallback, this)
                            == AsyncCompletionResult.Queued)
                        {
                            break;
                        }

                        GetReadResult();
                    }


                    int bytesDecoded = decoder.Decode(connectionBuffer, offset, size);
                    if (bytesDecoded > 0)
                    {
                        offset += bytesDecoded;
                        size -= bytesDecoded;
                    }

                    if (decoder.CurrentState == ServerSessionDecoder.State.PreUpgradeStart)
                    {
                        if (onValidate == null)
                        {
                            onValidate = Fx.ThunkCallback(new AsyncCallback(OnValidate));
                        }
                        this.via = decoder.Via;
                        IAsyncResult result = this.Connection.BeginValidate(this.via, onValidate, this);

                        if (result.CompletedSynchronously)
                        {
                            if (!VerifyValidationResult(result))
                            {
                                // This goes through the failure path (Abort) even though it doesn't throw.
                                return;
                            }
                        }
                        break; //exit loop, set success=true;
                    }
                }
                success = true;
            }
            catch (CommunicationException exception)
            {
                DiagnosticUtility.TraceHandledException(exception, TraceEventType.Information);
            }
            catch (TimeoutException exception)
            {
                if (TD.ReceiveTimeoutIsEnabled())
                {
                    TD.ReceiveTimeout(exception.Message);
                }
                DiagnosticUtility.TraceHandledException(exception, TraceEventType.Information);
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }
                if (!ExceptionHandler.HandleTransportExceptionHelper(e))
                {
                    throw;
                }
                // containment -- all exceptions abort the reader, no additional containment action necessary
            }
            finally
            {
                if (!success)
                {
                    Abort();
                }
            }
        }

        //returns true if validation was successful
        bool VerifyValidationResult(IAsyncResult result)
        {
            return this.Connection.EndValidate(result) && this.ContinuePostValidationProcessing();
        }

        static void OnValidate(IAsyncResult result)
        {
            bool success = false;
            ServerSessionPreambleConnectionReader thisPtr = (ServerSessionPreambleConnectionReader)result.AsyncState;
            try
            {
                if (!result.CompletedSynchronously)
                {
                    if (!thisPtr.VerifyValidationResult(result))
                    {
                        // This goes through the failure path (Abort) even though it doesn't throw.
                        return;
                    }
                }
                success = true;
            }
            catch (CommunicationException exception)
            {
                DiagnosticUtility.TraceHandledException(exception, TraceEventType.Information);
            }
            catch (TimeoutException exception)
            {
                if (TD.ReceiveTimeoutIsEnabled())
                {
                    TD.ReceiveTimeout(exception.Message);
                }
                DiagnosticUtility.TraceHandledException(exception, TraceEventType.Information);
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }

                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
            }
            finally
            {
                if (!success)
                {
                    thisPtr.Abort();
                }
            }
        }

        //returns false if the connection should be aborted
        bool ContinuePostValidationProcessing()
        {
            if (viaDelegate != null)
            {
                try
                {
                    viaDelegate(via);
                }
                catch (ServiceActivationException e)
                {
                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                    // return fault and close connection
                    SendFault(FramingEncodingString.ServiceActivationFailedFault);
                    return true;
                }
            }


            this.settings = transportSettingsCallback(via);

            if (settings == null)
            {
                EndpointNotFoundException e = new EndpointNotFoundException(SR.GetString(SR.EndpointNotFound, decoder.Via));
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);

                SendFault(FramingEncodingString.EndpointNotFoundFault);
                return false;
            }

            // we have enough information to hand off to a channel. Our job is done
            callback(this);
            return true;
        }

        public void SendFault(string faultString)
        {
            InitialServerConnectionReader.SendFault(
                Connection, faultString, this.connectionBuffer, GetRemainingTimeout(),
                TransportDefaults.MaxDrainSize);
            base.Close(GetRemainingTimeout());
        }

        public void StartReading(Action<Uri> viaDelegate, TimeSpan receiveTimeout)
        {
            this.viaDelegate = viaDelegate;
            this.receiveTimeoutHelper = new TimeoutHelper(receiveTimeout);
            this.connectionBuffer = Connection.AsyncReadBuffer;
            ContinueReading();
        }

        public IDuplexSessionChannel CreateDuplexSessionChannel(ConnectionOrientedTransportChannelListener channelListener, EndpointAddress localAddress, bool exposeConnectionProperty, ConnectionDemuxer connectionDemuxer)
        {
            return new ServerFramingDuplexSessionChannel(channelListener, this, localAddress, exposeConnectionProperty, connectionDemuxer);
        }

        class ServerFramingDuplexSessionChannel : FramingDuplexSessionChannel
        {
            ConnectionOrientedTransportChannelListener channelListener;
            ConnectionDemuxer connectionDemuxer;
            ServerSessionConnectionReader sessionReader;
            ServerSessionDecoder decoder;
            IConnection rawConnection;
            byte[] connectionBuffer;
            int offset;
            int size;
            StreamUpgradeAcceptor upgradeAcceptor;
            IStreamUpgradeChannelBindingProvider channelBindingProvider;

            public ServerFramingDuplexSessionChannel(ConnectionOrientedTransportChannelListener channelListener, ServerSessionPreambleConnectionReader preambleReader,
                EndpointAddress localAddress, bool exposeConnectionProperty, ConnectionDemuxer connectionDemuxer)
                : base(channelListener, localAddress, preambleReader.Via, exposeConnectionProperty)
            {
                this.channelListener = channelListener;
                this.connectionDemuxer = connectionDemuxer;
                this.Connection = preambleReader.Connection;
                this.decoder = preambleReader.Decoder;
                this.connectionBuffer = preambleReader.connectionBuffer;
                this.offset = preambleReader.BufferOffset;
                this.size = preambleReader.BufferSize;
                this.rawConnection = preambleReader.RawConnection;
                StreamUpgradeProvider upgrade = channelListener.Upgrade;
                if (upgrade != null)
                {
                    this.channelBindingProvider = upgrade.GetProperty<IStreamUpgradeChannelBindingProvider>();
                    this.upgradeAcceptor = upgrade.CreateUpgradeAcceptor();
                }
            }

            protected override void ReturnConnectionIfNecessary(bool abort, TimeSpan timeout)
            {
                IConnection localConnection = null;
                if (this.sessionReader != null)
                {
                    lock (ThisLock)
                    {
                        localConnection = this.sessionReader.GetRawConnection();
                    }
                }

                if (localConnection != null)
                {
                    if (abort)
                    {
                        localConnection.Abort();
                    }
                    else
                    {
                        this.connectionDemuxer.ReuseConnection(localConnection, timeout);
                    }
                    this.connectionDemuxer = null;
                }
            }

            public override T GetProperty<T>()
            {
                if (typeof(T) == typeof(IChannelBindingProvider))
                {
                    return (T)(object)this.channelBindingProvider;
                }

                return base.GetProperty<T>();
            }

            protected override void PrepareMessage(Message message)
            {
                channelListener.RaiseMessageReceived();
                base.PrepareMessage(message);
            }

            // perform security handshake and ACK connection
            protected override void OnOpen(TimeSpan timeout)
            {
                bool success = false;
                try
                {
                    TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);

                    // first validate our content type
                    ValidateContentType(ref timeoutHelper);

                    // next read any potential upgrades and finish consuming the preamble
                    for (;;)
                    {
                        if (size == 0)
                        {
                            offset = 0;
                            size = Connection.Read(connectionBuffer, 0, connectionBuffer.Length, timeoutHelper.RemainingTime());
                            if (size == 0)
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(decoder.CreatePrematureEOFException());
                            }
                        }

                        for (;;)
                        {
                            DecodeBytes();
                            switch (decoder.CurrentState)
                            {
                                case ServerSessionDecoder.State.UpgradeRequest:
                                    ProcessUpgradeRequest(ref timeoutHelper);

                                    // accept upgrade
                                    Connection.Write(ServerSessionEncoder.UpgradeResponseBytes, 0, ServerSessionEncoder.UpgradeResponseBytes.Length, true, timeoutHelper.RemainingTime());

                                    IConnection connectionToUpgrade = this.Connection;
                                    if (this.size > 0)
                                    {
                                        connectionToUpgrade = new PreReadConnection(connectionToUpgrade, this.connectionBuffer, this.offset, this.size);
                                    }

                                    try
                                    {
                                        this.Connection = InitialServerConnectionReader.UpgradeConnection(connectionToUpgrade, upgradeAcceptor, this);

                                        if (this.channelBindingProvider != null && this.channelBindingProvider.IsChannelBindingSupportEnabled)
                                        {
                                            this.SetChannelBinding(this.channelBindingProvider.GetChannelBinding(this.upgradeAcceptor, ChannelBindingKind.Endpoint));
                                        }

                                        this.connectionBuffer = Connection.AsyncReadBuffer;
                                    }
#pragma warning suppress 56500
                                    catch (Exception exception)
                                    {
                                        if (Fx.IsFatal(exception))
                                            throw;

                                        // Audit Authentication Failure
                                        WriteAuditFailure(upgradeAcceptor as StreamSecurityUpgradeAcceptor, exception);
                                        throw;
                                    }
                                    break;

                                case ServerSessionDecoder.State.Start:
                                    SetupSecurityIfNecessary();

                                    // we've finished the preamble. Ack and return.
                                    Connection.Write(ServerSessionEncoder.AckResponseBytes, 0,
                                        ServerSessionEncoder.AckResponseBytes.Length, true, timeoutHelper.RemainingTime());
                                    SetupSessionReader();
                                    success = true;
                                    return;
                            }

                            if (size == 0)
                                break;
                        }
                    }
                }
                finally
                {
                    if (!success)
                    {
                        Connection.Abort();
                    }
                }
            }

            void AcceptUpgradedConnection(IConnection upgradedConnection)
            {
                this.Connection = upgradedConnection;

                if (this.channelBindingProvider != null && this.channelBindingProvider.IsChannelBindingSupportEnabled)
                {
                    this.SetChannelBinding(this.channelBindingProvider.GetChannelBinding(this.upgradeAcceptor, ChannelBindingKind.Endpoint));
                }

                this.connectionBuffer = Connection.AsyncReadBuffer;
            }

            void ValidateContentType(ref TimeoutHelper timeoutHelper)
            {
                this.MessageEncoder = channelListener.MessageEncoderFactory.CreateSessionEncoder();

                if (!this.MessageEncoder.IsContentTypeSupported(decoder.ContentType))
                {
                    SendFault(FramingEncodingString.ContentTypeInvalidFault, ref timeoutHelper);
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ProtocolException(SR.GetString(
                        SR.ContentTypeMismatch, decoder.ContentType, this.MessageEncoder.ContentType)));
                }

                ICompressedMessageEncoder compressedMessageEncoder = this.MessageEncoder as ICompressedMessageEncoder;
                if (compressedMessageEncoder != null && compressedMessageEncoder.CompressionEnabled)
                {
                    compressedMessageEncoder.SetSessionContentType(this.decoder.ContentType);
                }
            }

            void DecodeBytes()
            {
                int bytesDecoded = decoder.Decode(connectionBuffer, offset, size);
                if (bytesDecoded > 0)
                {
                    offset += bytesDecoded;
                    size -= bytesDecoded;
                }
            }

            void ProcessUpgradeRequest(ref TimeoutHelper timeoutHelper)
            {
                if (this.upgradeAcceptor == null)
                {
                    SendFault(FramingEncodingString.UpgradeInvalidFault, ref timeoutHelper);
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new ProtocolException(SR.GetString(SR.UpgradeRequestToNonupgradableService, decoder.Upgrade)));
                }

                if (!this.upgradeAcceptor.CanUpgrade(decoder.Upgrade))
                {
                    SendFault(FramingEncodingString.UpgradeInvalidFault, ref timeoutHelper);
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new ProtocolException(SR.GetString(SR.UpgradeProtocolNotSupported, decoder.Upgrade)));
                }
            }

            void SendFault(string faultString, ref TimeoutHelper timeoutHelper)
            {
                InitialServerConnectionReader.SendFault(Connection, faultString,
                    connectionBuffer, timeoutHelper.RemainingTime(), TransportDefaults.MaxDrainSize);
            }

            void SetupSecurityIfNecessary()
            {
                StreamSecurityUpgradeAcceptor securityUpgradeAcceptor = this.upgradeAcceptor as StreamSecurityUpgradeAcceptor;
                if (securityUpgradeAcceptor != null)
                {
                    this.RemoteSecurity = securityUpgradeAcceptor.GetRemoteSecurity();

                    if (this.RemoteSecurity == null)
                    {
                        Exception securityFailedException = new ProtocolException(
                            SR.GetString(SR.RemoteSecurityNotNegotiatedOnStreamUpgrade, this.Via));
                        WriteAuditFailure(securityUpgradeAcceptor, securityFailedException);
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(securityFailedException);
                    }
                    else
                    {
                        // Audit Authentication Success
                        WriteAuditEvent(securityUpgradeAcceptor, AuditLevel.Success, null);
                    }
                }
            }

            void SetupSessionReader()
            {
                this.sessionReader = new ServerSessionConnectionReader(this);
                base.SetMessageSource(this.sessionReader);
            }

            protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return new OpenAsyncResult(this, timeout, callback, state);
            }

            protected override void OnEndOpen(IAsyncResult result)
            {
                OpenAsyncResult.End(result);
            }

            #region Transport Security Auditing
            void WriteAuditFailure(StreamSecurityUpgradeAcceptor securityUpgradeAcceptor, Exception exception)
            {
                try
                {
                    WriteAuditEvent(securityUpgradeAcceptor, AuditLevel.Failure, exception);
                }
#pragma warning suppress 56500 // covered by FxCop
                catch (Exception auditException)
                {
                    if (Fx.IsFatal(auditException))
                    {
                        throw;
                    }

                    DiagnosticUtility.TraceHandledException(auditException, TraceEventType.Error);
                }
            }

            void WriteAuditEvent(StreamSecurityUpgradeAcceptor securityUpgradeAcceptor, AuditLevel auditLevel, Exception exception)
            {
                if ((this.channelListener.AuditBehavior.MessageAuthenticationAuditLevel & auditLevel) != auditLevel)
                {
                    return;
                }

                if (securityUpgradeAcceptor == null)
                {
                    return;
                }

                String primaryIdentity = String.Empty;
                SecurityMessageProperty clientSecurity = securityUpgradeAcceptor.GetRemoteSecurity();
                if (clientSecurity != null)
                {
                    primaryIdentity = GetIdentityNameFromContext(clientSecurity);
                }

                ServiceSecurityAuditBehavior auditBehavior = this.channelListener.AuditBehavior;

                if (auditLevel == AuditLevel.Success)
                {
                    SecurityAuditHelper.WriteTransportAuthenticationSuccessEvent(auditBehavior.AuditLogLocation,
                        auditBehavior.SuppressAuditFailure, null, this.LocalVia, primaryIdentity);
                }
                else
                {
                    SecurityAuditHelper.WriteTransportAuthenticationFailureEvent(auditBehavior.AuditLogLocation,
                        auditBehavior.SuppressAuditFailure, null, this.LocalVia, primaryIdentity, exception);
                }
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static string GetIdentityNameFromContext(SecurityMessageProperty clientSecurity)
            {
                return SecurityUtils.GetIdentityNamesFromContext(
                    clientSecurity.ServiceSecurityContext.AuthorizationContext);
            }
            #endregion

            class OpenAsyncResult : AsyncResult
            {
                ServerFramingDuplexSessionChannel channel;
                TimeoutHelper timeoutHelper;
                static WaitCallback readCallback;
                static WaitCallback onWriteAckResponse;
                static WaitCallback onWriteUpgradeResponse;
                static AsyncCallback onUpgradeConnection;

                public OpenAsyncResult(ServerFramingDuplexSessionChannel channel, TimeSpan timeout,
                    AsyncCallback callback, object state)
                    : base(callback, state)
                {
                    this.channel = channel;
                    this.timeoutHelper = new TimeoutHelper(timeout);

                    bool completeSelf = false;
                    bool success = false;
                    try
                    {
                        channel.ValidateContentType(ref this.timeoutHelper);
                        completeSelf = ContinueReading();
                        success = true;
                    }
                    finally
                    {
                        if (!success)
                        {
                            CleanupOnError();
                        }
                    }

                    if (completeSelf)
                    {
                        base.Complete(true);
                    }
                }

                public static void End(IAsyncResult result)
                {
                    AsyncResult.End<OpenAsyncResult>(result);
                }

                void CleanupOnError()
                {
                    this.channel.Connection.Abort();
                }

                bool ContinueReading()
                {
                    for (;;)
                    {
                        if (channel.size == 0)
                        {
                            if (readCallback == null)
                            {
                                readCallback = new WaitCallback(ReadCallback);
                            }

                            if (channel.Connection.BeginRead(0, channel.connectionBuffer.Length, timeoutHelper.RemainingTime(),
                                readCallback, this) == AsyncCompletionResult.Queued)
                            {
                                return false;
                            }

                            GetReadResult();
                        }

                        for (;;)
                        {
                            channel.DecodeBytes();
                            switch (channel.decoder.CurrentState)
                            {
                                case ServerSessionDecoder.State.UpgradeRequest:
                                    channel.ProcessUpgradeRequest(ref this.timeoutHelper);

                                    // accept upgrade
                                    if (onWriteUpgradeResponse == null)
                                    {
                                        onWriteUpgradeResponse = Fx.ThunkCallback(new WaitCallback(OnWriteUpgradeResponse));
                                    }

                                    AsyncCompletionResult writeResult = channel.Connection.BeginWrite(
                                        ServerSessionEncoder.UpgradeResponseBytes, 0, ServerSessionEncoder.UpgradeResponseBytes.Length,
                                        true, timeoutHelper.RemainingTime(), onWriteUpgradeResponse, this);

                                    if (writeResult == AsyncCompletionResult.Queued)
                                    {
                                        return false;
                                    }

                                    if (!HandleWriteUpgradeResponseComplete())
                                    {
                                        return false;
                                    }
                                    break;

                                case ServerSessionDecoder.State.Start:
                                    channel.SetupSecurityIfNecessary();

                                    // we've finished the preamble. Ack and return.
                                    if (onWriteAckResponse == null)
                                    {
                                        onWriteAckResponse = Fx.ThunkCallback(new WaitCallback(OnWriteAckResponse));
                                    }

                                    AsyncCompletionResult writeAckResult =
                                        channel.Connection.BeginWrite(ServerSessionEncoder.AckResponseBytes, 0,
                                        ServerSessionEncoder.AckResponseBytes.Length, true, timeoutHelper.RemainingTime(),
                                        onWriteAckResponse, this);

                                    if (writeAckResult == AsyncCompletionResult.Queued)
                                    {
                                        return false;
                                    }

                                    return HandleWriteAckComplete();
                            }

                            if (channel.size == 0)
                                break;
                        }
                    }
                }

                void GetReadResult()
                {
                    channel.offset = 0;
                    channel.size = channel.Connection.EndRead();
                    if (channel.size == 0)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(channel.decoder.CreatePrematureEOFException());
                    }
                }

                bool HandleWriteUpgradeResponseComplete()
                {
                    channel.Connection.EndWrite();

                    IConnection connectionToUpgrade = channel.Connection;
                    if (channel.size > 0)
                    {
                        connectionToUpgrade = new PreReadConnection(connectionToUpgrade, channel.connectionBuffer, channel.offset, channel.size);
                    }

                    if (onUpgradeConnection == null)
                    {
                        onUpgradeConnection = Fx.ThunkCallback(new AsyncCallback(OnUpgradeConnection));
                    }

                    try
                    {
                        IAsyncResult upgradeConnectionResult = InitialServerConnectionReader.BeginUpgradeConnection(
                            connectionToUpgrade, channel.upgradeAcceptor, channel, onUpgradeConnection, this);
                        if (!upgradeConnectionResult.CompletedSynchronously)
                        {
                            return false;
                        }

                        return HandleUpgradeConnectionComplete(upgradeConnectionResult);
                    }
#pragma warning suppress 56500
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                        {
                            throw;
                        }

                        // Audit Authentication Failure
                        this.channel.WriteAuditFailure(channel.upgradeAcceptor as StreamSecurityUpgradeAcceptor, exception);
                        throw;
                    }
                }

                bool HandleUpgradeConnectionComplete(IAsyncResult result)
                {
                    channel.AcceptUpgradedConnection(InitialServerConnectionReader.EndUpgradeConnection(result));
                    return true;
                }

                bool HandleWriteAckComplete()
                {
                    channel.Connection.EndWrite();
                    channel.SetupSessionReader();
                    return true;
                }

                static void ReadCallback(object state)
                {
                    OpenAsyncResult thisPtr = (OpenAsyncResult)state;
                    bool completeSelf = false;
                    Exception completionException = null;
                    try
                    {
                        thisPtr.GetReadResult();
                        completeSelf = thisPtr.ContinueReading();
                    }
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }

                        completeSelf = true;
                        completionException = e;
                        thisPtr.CleanupOnError();
                    }

                    if (completeSelf)
                    {
                        thisPtr.Complete(false, completionException);
                    }
                }

                static void OnWriteUpgradeResponse(object asyncState)
                {
                    OpenAsyncResult thisPtr = (OpenAsyncResult)asyncState;
                    bool completeSelf = false;
                    Exception completionException = null;
                    try
                    {
                        completeSelf = thisPtr.HandleWriteUpgradeResponseComplete();

                        if (completeSelf)
                        {
                            completeSelf = thisPtr.ContinueReading();
                        }
                    }
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }

                        completionException = e;
                        completeSelf = true;
                        thisPtr.CleanupOnError();

                        // Audit Authentication Failure
                        thisPtr.channel.WriteAuditFailure(thisPtr.channel.upgradeAcceptor as StreamSecurityUpgradeAcceptor, e);
                    }

                    if (completeSelf)
                    {
                        thisPtr.Complete(false, completionException);
                    }
                }

                static void OnUpgradeConnection(IAsyncResult result)
                {
                    if (result.CompletedSynchronously)
                    {
                        return;
                    }

                    OpenAsyncResult thisPtr = (OpenAsyncResult)result.AsyncState;
                    bool completeSelf = false;
                    Exception completionException = null;
                    try
                    {
                        completeSelf = thisPtr.HandleUpgradeConnectionComplete(result);

                        if (completeSelf)
                        {
                            completeSelf = thisPtr.ContinueReading();
                        }
                    }
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }

                        completionException = e;
                        completeSelf = true;
                        thisPtr.CleanupOnError();

                        // Audit Authentication Failure
                        thisPtr.channel.WriteAuditFailure(thisPtr.channel.upgradeAcceptor as StreamSecurityUpgradeAcceptor, e);
                    }

                    if (completeSelf)
                    {
                        thisPtr.Complete(false, completionException);
                    }
                }

                static void OnWriteAckResponse(object asyncState)
                {
                    OpenAsyncResult thisPtr = (OpenAsyncResult)asyncState;
                    bool completeSelf = false;
                    Exception completionException = null;
                    try
                    {
                        completeSelf = thisPtr.HandleWriteAckComplete();
                    }
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }

                        completionException = e;
                        completeSelf = true;
                        thisPtr.CleanupOnError();
                    }

                    if (completeSelf)
                    {
                        thisPtr.Complete(false, completionException);
                    }
                }
            }

            class ServerSessionConnectionReader : SessionConnectionReader
            {
                ServerSessionDecoder decoder;
                int maxBufferSize;
                BufferManager bufferManager;
                MessageEncoder messageEncoder;
                string contentType;
                IConnection rawConnection;

                public ServerSessionConnectionReader(ServerFramingDuplexSessionChannel channel)
                    : base(channel.Connection, channel.rawConnection, channel.offset, channel.size, channel.RemoteSecurity)
                {
                    this.decoder = channel.decoder;
                    this.contentType = this.decoder.ContentType;
                    this.maxBufferSize = channel.channelListener.MaxBufferSize;
                    this.bufferManager = channel.channelListener.BufferManager;
                    this.messageEncoder = channel.MessageEncoder;
                    this.rawConnection = channel.rawConnection;
                }

                protected override void EnsureDecoderAtEof()
                {
                    if (!(decoder.CurrentState == ServerSessionDecoder.State.End || decoder.CurrentState == ServerSessionDecoder.State.EnvelopeEnd))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(decoder.CreatePrematureEOFException());
                    }
                }

                protected override Message DecodeMessage(byte[] buffer, ref int offset, ref int size, ref bool isAtEof, TimeSpan timeout)
                {
                    while (!isAtEof && size > 0)
                    {
                        int bytesRead = decoder.Decode(buffer, offset, size);
                        if (bytesRead > 0)
                        {
                            if (EnvelopeBuffer != null)
                            {
                                if (!object.ReferenceEquals(buffer, EnvelopeBuffer))
                                {
                                    System.Buffer.BlockCopy(buffer, offset, EnvelopeBuffer, EnvelopeOffset, bytesRead);
                                }
                                EnvelopeOffset += bytesRead;
                            }

                            offset += bytesRead;
                            size -= bytesRead;
                        }

                        switch (decoder.CurrentState)
                        {
                            case ServerSessionDecoder.State.EnvelopeStart:
                                int envelopeSize = decoder.EnvelopeSize;
                                if (envelopeSize > maxBufferSize)
                                {
                                    base.SendFault(FramingEncodingString.MaxMessageSizeExceededFault, timeout);

                                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                                        MaxMessageSizeStream.CreateMaxReceivedMessageSizeExceededException(maxBufferSize));
                                }
                                EnvelopeBuffer = bufferManager.TakeBuffer(envelopeSize);
                                EnvelopeOffset = 0;
                                EnvelopeSize = envelopeSize;
                                break;

                            case ServerSessionDecoder.State.EnvelopeEnd:
                                if (EnvelopeBuffer != null)
                                {
                                    using (ServiceModelActivity activity = DiagnosticUtility.ShouldUseActivity ? ServiceModelActivity.CreateBoundedActivity(true) : null)
                                    {
                                        if (DiagnosticUtility.ShouldUseActivity)
                                        {
                                            ServiceModelActivity.Start(activity, SR.GetString(SR.ActivityProcessingMessage, TraceUtility.RetrieveMessageNumber()), ActivityType.ProcessMessage);
                                        }
                                        Message message = null;

                                        try
                                        {
                                            message = messageEncoder.ReadMessage(new ArraySegment<byte>(EnvelopeBuffer, 0, EnvelopeSize), bufferManager, this.contentType);
                                        }
                                        catch (XmlException xmlException)
                                        {
                                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                                                new ProtocolException(SR.GetString(SR.MessageXmlProtocolError), xmlException));
                                        }

                                        if (DiagnosticUtility.ShouldUseActivity)
                                        {
                                            TraceUtility.TransferFromTransport(message);
                                        }
                                        EnvelopeBuffer = null;
                                        return message;
                                    }
                                }
                                break;

                            case ServerSessionDecoder.State.End:
                                isAtEof = true;
                                break;
                        }
                    }

                    return null;
                }

                protected override void PrepareMessage(Message message)
                {
                    base.PrepareMessage(message);

                    IPEndPoint remoteEndPoint = this.rawConnection.RemoteIPEndPoint;
                    // pipes will return null
                    if (remoteEndPoint != null)
                    {
                        RemoteEndpointMessageProperty remoteEndpointProperty = new RemoteEndpointMessageProperty(remoteEndPoint);
                        message.Properties.Add(RemoteEndpointMessageProperty.Name, remoteEndpointProperty);
                    }
                }
            }
        }
    }

    abstract class SessionConnectionReader : IMessageSource
    {
        bool isAtEOF;
        bool usingAsyncReadBuffer;
        IConnection connection;
        byte[] buffer;
        int offset;
        int size;
        byte[] envelopeBuffer;
        int envelopeOffset;
        int envelopeSize;
        bool readIntoEnvelopeBuffer;
        WaitCallback onAsyncReadComplete;
        Message pendingMessage;
        Exception pendingException;
        WaitCallback pendingCallback;
        object pendingCallbackState;
        SecurityMessageProperty security;
        TimeoutHelper readTimeoutHelper;
        // Raw connection that we will revert to after end handshake
        IConnection rawConnection;

        protected SessionConnectionReader(IConnection connection, IConnection rawConnection,
            int offset, int size, SecurityMessageProperty security)
        {
            this.offset = offset;
            this.size = size;
            if (size > 0)
            {
                this.buffer = connection.AsyncReadBuffer;
            }
            this.connection = connection;
            this.rawConnection = rawConnection;
            this.onAsyncReadComplete = new WaitCallback(OnAsyncReadComplete);
            this.security = security;
        }

        Message DecodeMessage(TimeSpan timeout)
        {
            if (DiagnosticUtility.ShouldUseActivity &&
                ServiceModelActivity.Current != null &&
                ServiceModelActivity.Current.ActivityType == ActivityType.ProcessAction)
            {
                ServiceModelActivity.Current.Resume();
            }
            if (!readIntoEnvelopeBuffer)
            {
                return DecodeMessage(buffer, ref offset, ref size, ref isAtEOF, timeout);
            }
            else
            {
                // decode from the envelope buffer
                int dummyOffset = this.envelopeOffset;
                return DecodeMessage(envelopeBuffer, ref dummyOffset, ref size, ref isAtEOF, timeout);
            }
        }

        protected abstract Message DecodeMessage(byte[] buffer, ref int offset, ref int size, ref bool isAtEof, TimeSpan timeout);

        protected byte[] EnvelopeBuffer
        {
            get { return envelopeBuffer; }
            set { envelopeBuffer = value; }
        }

        protected int EnvelopeOffset
        {
            get { return envelopeOffset; }
            set { envelopeOffset = value; }
        }

        protected int EnvelopeSize
        {
            get { return envelopeSize; }
            set { envelopeSize = value; }
        }

        public IConnection GetRawConnection()
        {
            IConnection result = null;
            if (this.rawConnection != null)
            {
                result = this.rawConnection;
                this.rawConnection = null;
                if (size > 0)
                {
                    PreReadConnection preReadConnection = result as PreReadConnection;
                    if (preReadConnection != null) // make sure we don't keep wrapping
                    {
                        preReadConnection.AddPreReadData(this.buffer, this.offset, this.size);
                    }
                    else
                    {
                        result = new PreReadConnection(result, this.buffer, this.offset, this.size);
                    }
                }
            }

            return result;
        }

        public AsyncReceiveResult BeginReceive(TimeSpan timeout, WaitCallback callback, object state)
        {
            if (pendingMessage != null || pendingException != null)
            {
                return AsyncReceiveResult.Completed;
            }

            this.readTimeoutHelper = new TimeoutHelper(timeout);
            for (;;)
            {
                if (isAtEOF)
                {
                    return AsyncReceiveResult.Completed;
                }

                if (size > 0)
                {
                    pendingMessage = DecodeMessage(readTimeoutHelper.RemainingTime());

                    if (pendingMessage != null)
                    {
                        PrepareMessage(pendingMessage);
                        return AsyncReceiveResult.Completed;
                    }
                    else if (isAtEOF) // could have read the END record under DecodeMessage
                    {
                        return AsyncReceiveResult.Completed;
                    }
                }

                if (size != 0)
                {
                    throw Fx.AssertAndThrow("BeginReceive: DecodeMessage() should consume the outstanding buffer or return a message.");
                }

                if (!usingAsyncReadBuffer)
                {
                    buffer = connection.AsyncReadBuffer;
                    usingAsyncReadBuffer = true;
                }

                pendingCallback = callback;
                pendingCallbackState = state;

                bool throwing = true;
                AsyncCompletionResult asyncReadResult;
                try
                {
                    asyncReadResult =
                        connection.BeginRead(0, buffer.Length, readTimeoutHelper.RemainingTime(), onAsyncReadComplete, null);

                    throwing = false;
                }
                finally
                {
                    if (throwing)
                    {
                        pendingCallback = null;
                        pendingCallbackState = null;
                    }
                }

                if (asyncReadResult == AsyncCompletionResult.Queued)
                {
                    return AsyncReceiveResult.Pending;
                }

                pendingCallback = null;
                pendingCallbackState = null;

                int bytesRead = connection.EndRead();

                HandleReadComplete(bytesRead, false);
            }
        }

        public Message Receive(TimeSpan timeout)
        {
            Message message = GetPendingMessage();

            if (message != null)
            {
                return message;
            }

            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            for (;;)
            {
                if (isAtEOF)
                {
                    return null;
                }

                if (size > 0)
                {
                    message = DecodeMessage(timeoutHelper.RemainingTime());

                    if (message != null)
                    {
                        PrepareMessage(message);
                        return message;
                    }
                    else if (isAtEOF) // could have read the END record under DecodeMessage
                    {
                        return null;
                    }
                }

                if (size != 0)
                {
                    throw Fx.AssertAndThrow("Receive: DecodeMessage() should consume the outstanding buffer or return a message.");
                }

                if (buffer == null)
                {
                    buffer = DiagnosticUtility.Utility.AllocateByteArray(connection.AsyncReadBufferSize);
                }

                int bytesRead;

                if (EnvelopeBuffer != null &&
                    (EnvelopeSize - EnvelopeOffset) >= buffer.Length)
                {
                    bytesRead = connection.Read(EnvelopeBuffer, EnvelopeOffset, buffer.Length, timeoutHelper.RemainingTime());
                    HandleReadComplete(bytesRead, true);
                }
                else
                {
                    bytesRead = connection.Read(buffer, 0, buffer.Length, timeoutHelper.RemainingTime());
                    HandleReadComplete(bytesRead, false);
                }
            }
        }

        public Message EndReceive()
        {
            return GetPendingMessage();
        }

        Message GetPendingMessage()
        {
            if (pendingException != null)
            {
                Exception exception = pendingException;
                pendingException = null;
                throw TraceUtility.ThrowHelperError(exception, pendingMessage);
            }

            if (pendingMessage != null)
            {
                Message message = pendingMessage;
                pendingMessage = null;
                return message;
            }

            return null;
        }

        public AsyncReceiveResult BeginWaitForMessage(TimeSpan timeout, WaitCallback callback, object state)
        {
            try
            {
                return BeginReceive(timeout, callback, state);
            }
            catch (TimeoutException e)
            {
                pendingException = e;
                return AsyncReceiveResult.Completed;
            }
        }

        public bool EndWaitForMessage()
        {
            try
            {
                Message message = EndReceive();
                this.pendingMessage = message;
                return true;
            }
            catch (TimeoutException e)
            {
                if (TD.ReceiveTimeoutIsEnabled())
                {
                    TD.ReceiveTimeout(e.Message);
                }
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                return false;
            }
        }

        public bool WaitForMessage(TimeSpan timeout)
        {
            try
            {
                Message message = Receive(timeout);
                this.pendingMessage = message;
                return true;
            }
            catch (TimeoutException e)
            {
                if (TD.ReceiveTimeoutIsEnabled())
                {
                    TD.ReceiveTimeout(e.Message);
                }
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                return false;
            }
        }

        protected abstract void EnsureDecoderAtEof();

        void HandleReadComplete(int bytesRead, bool readIntoEnvelopeBuffer)
        {
            this.readIntoEnvelopeBuffer = readIntoEnvelopeBuffer;

            if (bytesRead == 0)
            {
                EnsureDecoderAtEof();
                isAtEOF = true;
            }
            else
            {
                this.offset = 0;
                this.size = bytesRead;
            }
        }

        void OnAsyncReadComplete(object state)
        {
            try
            {
                for (;;)
                {
                    int bytesRead = connection.EndRead();

                    HandleReadComplete(bytesRead, false);

                    if (isAtEOF)
                    {
                        break;
                    }

                    Message message = DecodeMessage(this.readTimeoutHelper.RemainingTime());

                    if (message != null)
                    {
                        PrepareMessage(message);
                        this.pendingMessage = message;
                        break;
                    }
                    else if (isAtEOF) // could have read the END record under DecodeMessage
                    {
                        break;
                    }
                    if (size != 0)
                    {
                        throw Fx.AssertAndThrow("OnAsyncReadComplete: DecodeMessage() should consume the outstanding buffer or return a message.");
                    }

                    if (connection.BeginRead(0, buffer.Length, this.readTimeoutHelper.RemainingTime(),
                        onAsyncReadComplete, null) == AsyncCompletionResult.Queued)
                    {
                        return;
                    }
                }
            }
#pragma warning suppress 56500 // Microsoft, transferring exception to caller
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }

                pendingException = e;
            }

            WaitCallback callback = pendingCallback;
            object callbackState = pendingCallbackState;
            pendingCallback = null;
            pendingCallbackState = null;
            callback(callbackState);
        }

        protected virtual void PrepareMessage(Message message)
        {
            if (security != null)
            {
                message.Properties.Security = (SecurityMessageProperty)security.CreateCopy();
            }
        }

        protected void SendFault(string faultString, TimeSpan timeout)
        {
            byte[] drainBuffer = new byte[128];
            InitialServerConnectionReader.SendFault(
                connection, faultString, drainBuffer, timeout,
                TransportDefaults.MaxDrainSize);
        }
    }


    class ClientDuplexConnectionReader : SessionConnectionReader
    {
        ClientDuplexDecoder decoder;
        int maxBufferSize;
        BufferManager bufferManager;
        MessageEncoder messageEncoder;
        ClientFramingDuplexSessionChannel channel;

        public ClientDuplexConnectionReader(ClientFramingDuplexSessionChannel channel, IConnection connection, ClientDuplexDecoder decoder,
            IConnectionOrientedTransportFactorySettings settings, MessageEncoder messageEncoder)
            : base(connection, null, 0, 0, null)
        {
            this.decoder = decoder;
            this.maxBufferSize = settings.MaxBufferSize;
            this.bufferManager = settings.BufferManager;
            this.messageEncoder = messageEncoder;
            this.channel = channel;
        }

        protected override void EnsureDecoderAtEof()
        {
            if (!(decoder.CurrentState == ClientFramingDecoderState.End
                || decoder.CurrentState == ClientFramingDecoderState.EnvelopeEnd
                || decoder.CurrentState == ClientFramingDecoderState.ReadingUpgradeRecord
                || decoder.CurrentState == ClientFramingDecoderState.UpgradeResponse))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(decoder.CreatePrematureEOFException());
            }
        }

        static IDisposable CreateProcessActionActivity()
        {
            IDisposable retval = null;
            if (DiagnosticUtility.ShouldUseActivity)
            {
                if ((ServiceModelActivity.Current != null) &&
                    (ServiceModelActivity.Current.ActivityType == ActivityType.ProcessAction))
                {
                    // Do nothing-- we are already OK
                }
                else if ((ServiceModelActivity.Current != null) &&
                    (ServiceModelActivity.Current.PreviousActivity != null) &&
                    (ServiceModelActivity.Current.PreviousActivity.ActivityType == ActivityType.ProcessAction))
                {
                    retval = ServiceModelActivity.BoundOperation(ServiceModelActivity.Current.PreviousActivity);
                }
                else
                {
                    ServiceModelActivity activity = ServiceModelActivity.CreateBoundedActivity(true);
                    ServiceModelActivity.Start(activity, SR.GetString(SR.ActivityProcessingMessage, TraceUtility.RetrieveMessageNumber()), ActivityType.ProcessMessage);
                    retval = activity;
                }
            }
            return retval;
        }

        protected override Message DecodeMessage(byte[] buffer, ref int offset, ref int size, ref bool isAtEOF, TimeSpan timeout)
        {
            while (size > 0)
            {
                int bytesRead = decoder.Decode(buffer, offset, size);
                if (bytesRead > 0)
                {
                    if (EnvelopeBuffer != null)
                    {
                        if (!object.ReferenceEquals(buffer, EnvelopeBuffer))
                            System.Buffer.BlockCopy(buffer, offset, EnvelopeBuffer, EnvelopeOffset, bytesRead);
                        EnvelopeOffset += bytesRead;
                    }

                    offset += bytesRead;
                    size -= bytesRead;
                }

                switch (decoder.CurrentState)
                {
                    case ClientFramingDecoderState.Fault:
                        channel.Session.CloseOutputSession(channel.InternalCloseTimeout);
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(FaultStringDecoder.GetFaultException(decoder.Fault, channel.RemoteAddress.Uri.ToString(), messageEncoder.ContentType));

                    case ClientFramingDecoderState.End:
                        isAtEOF = true;
                        return null; // we're done

                    case ClientFramingDecoderState.EnvelopeStart:
                        int envelopeSize = decoder.EnvelopeSize;
                        if (envelopeSize > maxBufferSize)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                                MaxMessageSizeStream.CreateMaxReceivedMessageSizeExceededException(maxBufferSize));
                        }
                        EnvelopeBuffer = bufferManager.TakeBuffer(envelopeSize);
                        EnvelopeOffset = 0;
                        EnvelopeSize = envelopeSize;
                        break;

                    case ClientFramingDecoderState.EnvelopeEnd:
                        if (EnvelopeBuffer != null)
                        {
                            Message message = null;
                            try
                            {
                                IDisposable activity = ClientDuplexConnectionReader.CreateProcessActionActivity();
                                using (activity)
                                {
                                    message = messageEncoder.ReadMessage(new ArraySegment<byte>(EnvelopeBuffer, 0, EnvelopeSize), bufferManager);
                                    if (DiagnosticUtility.ShouldUseActivity)
                                    {
                                        TraceUtility.TransferFromTransport(message);
                                    }
                                }
                            }
                            catch (XmlException xmlException)
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                                    new ProtocolException(SR.GetString(SR.MessageXmlProtocolError), xmlException));
                            }
                            EnvelopeBuffer = null;
                            return message;
                        }
                        break;
                }
            }
            return null;
        }
    }
}
