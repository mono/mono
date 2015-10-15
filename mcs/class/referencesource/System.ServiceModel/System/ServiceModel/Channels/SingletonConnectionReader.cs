//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System;
    using System.Diagnostics;
    using System.IO;
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

    delegate void ServerSingletonPreambleCallback(ServerSingletonPreambleConnectionReader serverSingletonPreambleReader);
    delegate ISingletonChannelListener SingletonPreambleDemuxCallback(ServerSingletonPreambleConnectionReader serverSingletonPreambleReader);
    interface ISingletonChannelListener
    {
        TimeSpan ReceiveTimeout { get; }
        void ReceiveRequest(RequestContext requestContext, Action callback, bool canDispatchOnThisThread);
    }

    class ServerSingletonPreambleConnectionReader : InitialServerConnectionReader
    {
        ServerSingletonDecoder decoder;
        ServerSingletonPreambleCallback callback;
        WaitCallback onAsyncReadComplete;
        IConnectionOrientedTransportFactorySettings transportSettings;
        TransportSettingsCallback transportSettingsCallback;
        SecurityMessageProperty security;
        Uri via;
        IConnection rawConnection;
        byte[] connectionBuffer;
        bool isReadPending;
        int offset;
        int size;
        TimeoutHelper receiveTimeoutHelper;
        Action<Uri> viaDelegate;
        ChannelBinding channelBindingToken;
        static AsyncCallback onValidate;

        public ServerSingletonPreambleConnectionReader(IConnection connection, Action connectionDequeuedCallback,
            long streamPosition, int offset, int size, TransportSettingsCallback transportSettingsCallback,
            ConnectionClosedCallback closedCallback, ServerSingletonPreambleCallback callback)
            : base(connection, closedCallback)
        {
            this.decoder = new ServerSingletonDecoder(streamPosition, MaxViaSize, MaxContentTypeSize);
            this.offset = offset;
            this.size = size;
            this.callback = callback;
            this.transportSettingsCallback = transportSettingsCallback;
            this.rawConnection = connection;
            this.ConnectionDequeuedCallback = connectionDequeuedCallback;

        }

        public ChannelBinding ChannelBinding
        {
            get
            {
                return this.channelBindingToken;
            }
        }

        public int BufferOffset
        {
            get { return this.offset; }
        }

        public int BufferSize
        {
            get { return this.size; }
        }

        public ServerSingletonDecoder Decoder
        {
            get { return this.decoder; }
        }

        public IConnection RawConnection
        {
            get { return this.rawConnection; }
        }

        public Uri Via
        {
            get { return this.via; }
        }

        public IConnectionOrientedTransportFactorySettings TransportSettings
        {
            get { return this.transportSettings; }
        }

        public SecurityMessageProperty Security
        {
            get { return this.security; }
        }

        TimeSpan GetRemainingTimeout()
        {
            return this.receiveTimeoutHelper.RemainingTime();
        }

        void ReadAndDispatch()
        {
            bool success = false;
            try
            {
                while ((size > 0 || !isReadPending) && !IsClosed)
                {
                    if (size == 0)
                    {
                        isReadPending = true;
                        if (onAsyncReadComplete == null)
                        {
                            onAsyncReadComplete = new WaitCallback(OnAsyncReadComplete);
                        }

                        if (Connection.BeginRead(0, connectionBuffer.Length, GetRemainingTimeout(),
                            onAsyncReadComplete, null) == AsyncCompletionResult.Queued)
                        {
                            break;
                        }
                        HandleReadComplete();
                    }

                    int bytesRead = decoder.Decode(connectionBuffer, offset, size);
                    if (bytesRead > 0)
                    {
                        offset += bytesRead;
                        size -= bytesRead;
                    }

                    if (decoder.CurrentState == ServerSingletonDecoder.State.PreUpgradeStart)
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

                // containment -- we abort ourselves for any error, no extra containment needed
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
            ServerSingletonPreambleConnectionReader thisPtr = (ServerSingletonPreambleConnectionReader)result.AsyncState;
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


            this.transportSettings = transportSettingsCallback(via);

            if (transportSettings == null)
            {
                EndpointNotFoundException e = new EndpointNotFoundException(SR.GetString(SR.EndpointNotFound, decoder.Via));
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                // return fault and close connection
                SendFault(FramingEncodingString.EndpointNotFoundFault);
                return false;
            }

            // we have enough information to hand off to a channel. Our job is done
            callback(this);
            return true;
        }

        public void SendFault(string faultString)
        {
            SendFault(faultString, ref this.receiveTimeoutHelper);
        }

        void SendFault(string faultString, ref TimeoutHelper timeoutHelper)
        {
            InitialServerConnectionReader.SendFault(Connection, faultString,
                connectionBuffer, timeoutHelper.RemainingTime(), TransportDefaults.MaxDrainSize);
        }

        public IAsyncResult BeginCompletePreamble(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CompletePreambleAsyncResult(timeout, this, callback, state);
        }

        public IConnection EndCompletePreamble(IAsyncResult result)
        {
            return CompletePreambleAsyncResult.End(result);
        }

        class CompletePreambleAsyncResult : TypedAsyncResult<IConnection>
        {
            static WaitCallback onReadCompleted = new WaitCallback(OnReadCompleted);
            static WaitCallback onWriteCompleted = new WaitCallback(OnWriteCompleted);
            static AsyncCallback onUpgradeComplete = Fx.ThunkCallback(OnUpgradeComplete);
            TimeoutHelper timeoutHelper;
            ServerSingletonPreambleConnectionReader parent;
            StreamUpgradeAcceptor upgradeAcceptor;
            StreamUpgradeProvider upgrade;
            IStreamUpgradeChannelBindingProvider channelBindingProvider;
            IConnection currentConnection;
            UpgradeState upgradeState = UpgradeState.None;
            
            public CompletePreambleAsyncResult(TimeSpan timeout, ServerSingletonPreambleConnectionReader parent, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.timeoutHelper = new TimeoutHelper(timeout);
                this.parent = parent;

                Initialize();

                if (ContinueWork(null))
                {
                    Complete(this.currentConnection, true);
                }
            }

            byte[] ConnectionBuffer
            {
                get
                {
                    return this.parent.connectionBuffer;
                }
                set
                {
                    this.parent.connectionBuffer = value;
                }
            }

            int Offset
            {
                get
                {
                    return this.parent.offset;
                }
                set
                {
                    this.parent.offset = value;
                }
            }

            int Size
            {
                get
                {
                    return this.parent.size;
                }
                set
                {
                    this.parent.size = value;
                }
            }

            bool CanReadAndDecode
            {
                get
                {
                    //ok to read/decode before we start the upgrade
                    //and between UpgradeComplete/WritingPreambleAck
                    return this.upgradeState == UpgradeState.None
                        || this.upgradeState == UpgradeState.UpgradeComplete;
                }
            }

            ServerSingletonDecoder Decoder
            {
                get
                {
                    return this.parent.decoder;
                }
            }

            void Initialize()
            {
                if (!this.parent.transportSettings.MessageEncoderFactory.Encoder.IsContentTypeSupported(Decoder.ContentType))
                {
                    SendFault(FramingEncodingString.ContentTypeInvalidFault, ref timeoutHelper);
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ProtocolException(SR.GetString(
                        SR.ContentTypeMismatch, Decoder.ContentType, parent.transportSettings.MessageEncoderFactory.Encoder.ContentType)));
                }

                upgrade = this.parent.transportSettings.Upgrade;                
                if (upgrade != null)
                {
                    channelBindingProvider = upgrade.GetProperty<IStreamUpgradeChannelBindingProvider>();
                    upgradeAcceptor = upgrade.CreateUpgradeAcceptor();
                }

                this.currentConnection = this.parent.Connection;
            }

            void SendFault(string faultString, ref TimeoutHelper timeoutHelper)
            {
                this.parent.SendFault(faultString, ref timeoutHelper);
            }

            bool BeginRead()
            {
                this.Offset = 0;
                return this.currentConnection.BeginRead(0, this.ConnectionBuffer.Length, timeoutHelper.RemainingTime(), onReadCompleted, this) == AsyncCompletionResult.Completed;
            }

            void EndRead()
            {
                this.Size = currentConnection.EndRead();
                if (this.Size == 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.Decoder.CreatePrematureEOFException());
                }
            }

            bool ContinueWork(IAsyncResult upgradeAsyncResult)
            {
                if (upgradeAsyncResult != null)
                {
                    Fx.AssertAndThrow(this.upgradeState == UpgradeState.EndUpgrade, "upgradeAsyncResult should only be passed in from OnUpgradeComplete callback");
                }

                for (;;)
                {
                    if (Size == 0 && this.CanReadAndDecode)
                    {
                        if (BeginRead())
                        {
                            EndRead();
                        }
                        else
                        {
                            //when read completes, we will re-enter this loop.
                            break;
                        }                        
                    }

                    for (;;)
                    {
                        if (this.CanReadAndDecode)
                        {
                            int bytesRead = Decoder.Decode(ConnectionBuffer, Offset, Size);
                            if (bytesRead > 0)
                            {
                                Offset += bytesRead;
                                Size -= bytesRead;
                            }
                        }

                        switch (Decoder.CurrentState)
                        {
                            case ServerSingletonDecoder.State.UpgradeRequest:
                                switch (this.upgradeState)
                                {
                                    case UpgradeState.None:
                                        //change the state so that we don't read/decode until it is safe
                                        ChangeUpgradeState(UpgradeState.VerifyingUpgradeRequest);
                                        break;
                                    case UpgradeState.VerifyingUpgradeRequest:
                                        if (this.upgradeAcceptor == null)
                                        {
                                            SendFault(FramingEncodingString.UpgradeInvalidFault, ref timeoutHelper);
                                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                                                new ProtocolException(SR.GetString(SR.UpgradeRequestToNonupgradableService, Decoder.Upgrade)));
                                        }

                                        if (!this.upgradeAcceptor.CanUpgrade(Decoder.Upgrade))
                                        {
                                            SendFault(FramingEncodingString.UpgradeInvalidFault, ref timeoutHelper);
                                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ProtocolException(SR.GetString(SR.UpgradeProtocolNotSupported, Decoder.Upgrade)));
                                        }

                                        ChangeUpgradeState(UpgradeState.WritingUpgradeAck);
                                        // accept upgrade
                                        if (this.currentConnection.BeginWrite(ServerSingletonEncoder.UpgradeResponseBytes, 0, ServerSingletonEncoder.UpgradeResponseBytes.Length,
                                            true, timeoutHelper.RemainingTime(), onWriteCompleted, this) == AsyncCompletionResult.Queued)
                                        {
                                            //OnWriteCompleted will:
                                            //  1) set upgradeState to UpgradeAckSent 
                                            //  2) call EndWrite
                                            return false;
                                        }
                                        else
                                        {
                                            this.currentConnection.EndWrite();
                                        }

                                        ChangeUpgradeState(UpgradeState.UpgradeAckSent);
                                        break;
                                    case UpgradeState.UpgradeAckSent:
                                        IConnection connectionToUpgrade = this.currentConnection;
                                        if (Size > 0)
                                        {
                                            connectionToUpgrade = new PreReadConnection(connectionToUpgrade, ConnectionBuffer, Offset, Size);
                                        }
                                        ChangeUpgradeState(UpgradeState.BeginUpgrade);
                                        break;
                                    case UpgradeState.BeginUpgrade:
                                        try
                                        {
                                            if (!BeginUpgrade(out upgradeAsyncResult))
                                            {
                                                //OnUpgradeComplete will set upgradeState to EndUpgrade
                                                return false;
                                            }

                                            ChangeUpgradeState(UpgradeState.EndUpgrade);
                                        }
                                        catch (Exception exception)
                                        {
                                            if (Fx.IsFatal(exception))
                                                throw;
                                            
                                            this.parent.WriteAuditFailure(upgradeAcceptor as StreamSecurityUpgradeAcceptor, exception);
                                            throw;
                                        }
                                        break;
                                    case UpgradeState.EndUpgrade://Must be a different state here than UpgradeComplete so that we don't try to read from the connection
                                        try
                                        {
                                            EndUpgrade(upgradeAsyncResult);
                                            ChangeUpgradeState(UpgradeState.UpgradeComplete);
                                        }
                                        catch (Exception exception)
                                        {
                                            if (Fx.IsFatal(exception))
                                                throw;

                                            this.parent.WriteAuditFailure(upgradeAcceptor as StreamSecurityUpgradeAcceptor, exception);
                                            throw;
                                        }
                                        break;
                                    case UpgradeState.UpgradeComplete:
                                        //Client is doing more than one upgrade, reset the state
                                        ChangeUpgradeState(UpgradeState.VerifyingUpgradeRequest);
                                        break;
                                }
                                break;
                            case ServerSingletonDecoder.State.Start:
                                this.parent.SetupSecurityIfNecessary(upgradeAcceptor);

                                if (this.upgradeState == UpgradeState.UpgradeComplete //We have done at least one upgrade, but we are now done.
                                    || this.upgradeState == UpgradeState.None)//no upgrade, just send the preample end bytes
                                {
                                    ChangeUpgradeState(UpgradeState.WritingPreambleEnd);
                                    // we've finished the preamble. Ack and return.
                                    if (this.currentConnection.BeginWrite(ServerSessionEncoder.AckResponseBytes, 0, ServerSessionEncoder.AckResponseBytes.Length,
                                                true, timeoutHelper.RemainingTime(), onWriteCompleted, this) == AsyncCompletionResult.Queued)
                                    {
                                        //OnWriteCompleted will:
                                        //  1) set upgradeState to PreambleEndSent 
                                        //  2) call EndWrite
                                        return false;
                                    }
                                    else
                                    {
                                        this.currentConnection.EndWrite();
                                    }
                                    
                                    //terminal state
                                    ChangeUpgradeState(UpgradeState.PreambleEndSent);
                                }
                                                                
                                //we are done, this.currentConnection is the upgraded connection                                
                                return true;
                        }

                        if (Size == 0)
                        {
                            break;
                        }
                    }
                }

                return false;
            }

            bool BeginUpgrade(out IAsyncResult upgradeAsyncResult)
            {
                upgradeAsyncResult = InitialServerConnectionReader.BeginUpgradeConnection(this.currentConnection, upgradeAcceptor, this.parent.transportSettings, onUpgradeComplete, this);

                if (!upgradeAsyncResult.CompletedSynchronously)
                {
                    upgradeAsyncResult = null; //caller shouldn't use this out param unless completed sync.
                    return false;
                }

                return true;
            }

            void EndUpgrade(IAsyncResult upgradeAsyncResult)
            {
                this.currentConnection = InitialServerConnectionReader.EndUpgradeConnection(upgradeAsyncResult);

                this.ConnectionBuffer = this.currentConnection.AsyncReadBuffer;

                if (this.channelBindingProvider != null 
                    && this.channelBindingProvider.IsChannelBindingSupportEnabled 
                    && this.parent.channelBindingToken == null)//first one wins in the case of multiple upgrades.
                {
                    this.parent.channelBindingToken = channelBindingProvider.GetChannelBinding(this.upgradeAcceptor, ChannelBindingKind.Endpoint);
                }
            }

            void ChangeUpgradeState(UpgradeState newState)
            {
                switch (newState)
                {
                    case UpgradeState.None:
                        throw Fx.AssertAndThrow("Invalid State Transition: currentState=" + this.upgradeState + ", newState=" + newState);
                    case UpgradeState.VerifyingUpgradeRequest:
                        if (this.upgradeState != UpgradeState.None //starting first upgrade
                            && this.upgradeState != UpgradeState.UpgradeComplete)//completing one upgrade and starting another
                        {
                            throw Fx.AssertAndThrow("Invalid State Transition: currentState=" + this.upgradeState + ", newState=" + newState);
                        }
                        break;
                    case UpgradeState.WritingUpgradeAck:
                        if (this.upgradeState != UpgradeState.VerifyingUpgradeRequest)
                        {
                            throw Fx.AssertAndThrow("Invalid State Transition: currentState=" + this.upgradeState + ", newState=" + newState);
                        }
                        break;
                    case UpgradeState.UpgradeAckSent:
                        if (this.upgradeState != UpgradeState.WritingUpgradeAck)
                        {
                            throw Fx.AssertAndThrow("Invalid State Transition: currentState=" + this.upgradeState + ", newState=" + newState);
                        }
                        break;
                    case UpgradeState.BeginUpgrade:
                        if (this.upgradeState != UpgradeState.UpgradeAckSent)
                        {
                            throw Fx.AssertAndThrow("Invalid State Transition: currentState=" + this.upgradeState + ", newState=" + newState);
                        }
                        break;
                    case UpgradeState.EndUpgrade:
                        if (this.upgradeState != UpgradeState.BeginUpgrade)
                        {
                            throw Fx.AssertAndThrow("Invalid State Transition: currentState=" + this.upgradeState + ", newState=" + newState);
                        }
                        break;
                    case UpgradeState.UpgradeComplete:
                        if (this.upgradeState != UpgradeState.EndUpgrade)
                        {
                            throw Fx.AssertAndThrow("Invalid State Transition: currentState=" + this.upgradeState + ", newState=" + newState);
                        }
                        break;
                    case UpgradeState.WritingPreambleEnd:
                        if (this.upgradeState != UpgradeState.None //no upgrade being used
                            && this.upgradeState != UpgradeState.UpgradeComplete)//upgrades are now complete, end the preamble handshake.
                        {
                            throw Fx.AssertAndThrow("Invalid State Transition: currentState=" + this.upgradeState + ", newState=" + newState);
                        }
                        break;
                    case UpgradeState.PreambleEndSent:
                        if (this.upgradeState != UpgradeState.WritingPreambleEnd)
                        {
                            throw Fx.AssertAndThrow("Invalid State Transition: currentState=" + this.upgradeState + ", newState=" + newState);
                        }
                        break;
                    default:
                        throw Fx.AssertAndThrow("Unexpected Upgrade State: " + newState);
                }
                this.upgradeState = newState;
            }

            static void OnReadCompleted(object state)
            {
                CompletePreambleAsyncResult thisPtr = (CompletePreambleAsyncResult)state;
                Exception completionException = null;
                bool completeSelf = false;

                try
                {
                    thisPtr.EndRead();
                    completeSelf = thisPtr.ContinueWork(null);
                }
                catch (Exception ex)
                {
                    if (Fx.IsFatal(ex))
                    {
                        throw;
                    }
                    completionException = ex;
                    completeSelf = true;
                }

                if (completeSelf)
                {
                    if (completionException != null)
                    {
                        thisPtr.Complete(false, completionException);
                    }
                    else
                    {
                        thisPtr.Complete(thisPtr.currentConnection, false);
                    }
                }
            }

            static void OnWriteCompleted(object state)
            {
                CompletePreambleAsyncResult thisPtr = (CompletePreambleAsyncResult)state;
                Exception completionException = null;
                bool completeSelf = false;

                try
                {
                    thisPtr.currentConnection.EndWrite();

                    switch (thisPtr.upgradeState)
                    {
                        case UpgradeState.WritingUpgradeAck:
                            thisPtr.ChangeUpgradeState(UpgradeState.UpgradeAckSent);
                            break;
                        case UpgradeState.WritingPreambleEnd:
                            thisPtr.ChangeUpgradeState(UpgradeState.PreambleEndSent);
                            break;
                    }
                    completeSelf = thisPtr.ContinueWork(null);
                }
                catch (Exception ex)
                {
                    if (Fx.IsFatal(ex))
                    {
                        throw;
                    }
                    completionException = ex;
                    completeSelf = true;
                }

                if (completeSelf)
                {
                    if (completionException != null)
                    {
                        thisPtr.Complete(false, completionException);
                    }
                    else
                    {
                        thisPtr.Complete(thisPtr.currentConnection, false);
                    }
                }
            }
            
            static void OnUpgradeComplete(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }

                CompletePreambleAsyncResult thisPtr = (CompletePreambleAsyncResult)result.AsyncState;                
                Exception completionException = null;
                bool completeSelf = false;
                
                try
                {
                    thisPtr.ChangeUpgradeState(UpgradeState.EndUpgrade);
                    completeSelf = thisPtr.ContinueWork(result);
                }
                catch (Exception ex)
                {
                    if (Fx.IsFatal(ex))
                    {
                        throw;
                    }
                    completionException = ex;
                    completeSelf = true;
                }

                if (completeSelf)
                {
                    if (completionException != null)
                    {
                        thisPtr.Complete(false, completionException);
                    }
                    else
                    {
                        thisPtr.Complete(thisPtr.currentConnection, false);
                    }
                }
            }

            enum UpgradeState
            {
                None, 
                VerifyingUpgradeRequest, 
                WritingUpgradeAck,
                UpgradeAckSent,
                BeginUpgrade,
                EndUpgrade,
                UpgradeComplete,
                WritingPreambleEnd,
                PreambleEndSent,
            }
        }        

        void SetupSecurityIfNecessary(StreamUpgradeAcceptor upgradeAcceptor)
        {
            StreamSecurityUpgradeAcceptor securityUpgradeAcceptor = upgradeAcceptor as StreamSecurityUpgradeAcceptor;
            if (securityUpgradeAcceptor != null)
            {
                this.security = securityUpgradeAcceptor.GetRemoteSecurity();
                if (this.security == null)
                {
                    Exception securityFailedException = new ProtocolException(
                    SR.GetString(SR.RemoteSecurityNotNegotiatedOnStreamUpgrade, this.Via));
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(securityFailedException);
                }
                // Audit Authentication Success
                WriteAuditEvent(securityUpgradeAcceptor, AuditLevel.Success, null);
            }
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
            if ((this.transportSettings.AuditBehavior.MessageAuthenticationAuditLevel & auditLevel) != auditLevel)
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

            ServiceSecurityAuditBehavior auditBehavior = this.transportSettings.AuditBehavior;

            if (auditLevel == AuditLevel.Success)
            {
                SecurityAuditHelper.WriteTransportAuthenticationSuccessEvent(auditBehavior.AuditLogLocation,
                    auditBehavior.SuppressAuditFailure, null, this.Via, primaryIdentity);
            }
            else
            {
                SecurityAuditHelper.WriteTransportAuthenticationFailureEvent(auditBehavior.AuditLogLocation,
                    auditBehavior.SuppressAuditFailure, null, this.Via, primaryIdentity, exception);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        static string GetIdentityNameFromContext(SecurityMessageProperty clientSecurity)
        {
            return SecurityUtils.GetIdentityNamesFromContext(
                clientSecurity.ServiceSecurityContext.AuthorizationContext);
        }
        #endregion

        void HandleReadComplete()
        {
            offset = 0;
            size = Connection.EndRead();
            isReadPending = false;
            if (size == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(decoder.CreatePrematureEOFException());
            }
        }

        void OnAsyncReadComplete(object state)
        {
            bool success = false;
            try
            {
                HandleReadComplete();
                ReadAndDispatch();
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

                // containment -- we abort ourselves for any error, no extra containment needed
            }
            finally
            {
                if (!success)
                {
                    Abort();
                }
            }
        }

        public void StartReading(Action<Uri> viaDelegate, TimeSpan timeout)
        {
            this.viaDelegate = viaDelegate;
            this.receiveTimeoutHelper = new TimeoutHelper(timeout);
            this.connectionBuffer = Connection.AsyncReadBuffer;
            ReadAndDispatch();
        }
    }

    class ServerSingletonConnectionReader : SingletonConnectionReader
    {
        ConnectionDemuxer connectionDemuxer;
        ServerSingletonDecoder decoder;
        IConnection rawConnection;
        string contentType;
        ChannelBinding channelBindingToken;

        public ServerSingletonConnectionReader(ServerSingletonPreambleConnectionReader preambleReader,
            IConnection upgradedConnection, ConnectionDemuxer connectionDemuxer)
            : base(upgradedConnection, preambleReader.BufferOffset, preambleReader.BufferSize,
            preambleReader.Security, preambleReader.TransportSettings, preambleReader.Via)
        {
            this.decoder = preambleReader.Decoder;
            this.contentType = this.decoder.ContentType;
            this.connectionDemuxer = connectionDemuxer;
            this.rawConnection = preambleReader.RawConnection;
            this.channelBindingToken = preambleReader.ChannelBinding;
        }

        protected override string ContentType
        {
            get { return this.contentType; }
        }

        protected override long StreamPosition
        {
            get { return this.decoder.StreamPosition; }
        }

        protected override bool DecodeBytes(byte[] buffer, ref int offset, ref int size, ref bool isAtEof)
        {
            while (size > 0)
            {
                int bytesRead = decoder.Decode(buffer, offset, size);
                if (bytesRead > 0)
                {
                    offset += bytesRead;
                    size -= bytesRead;
                }

                switch (decoder.CurrentState)
                {
                    case ServerSingletonDecoder.State.EnvelopeStart:
                        // we're at the envelope
                        return true;

                    case ServerSingletonDecoder.State.End:
                        isAtEof = true;
                        return false;
                }
            }

            return false;
        }

        protected override void OnClose(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            // send back EOF and then recycle the connection
            this.Connection.Write(SingletonEncoder.EndBytes, 0, SingletonEncoder.EndBytes.Length, true, timeoutHelper.RemainingTime());
            this.connectionDemuxer.ReuseConnection(this.rawConnection, timeoutHelper.RemainingTime());

            ChannelBindingUtility.Dispose(ref this.channelBindingToken);
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

            if (this.channelBindingToken != null)
            {
                ChannelBindingMessageProperty property = new ChannelBindingMessageProperty(this.channelBindingToken, false);
                property.AddTo(message);
                property.Dispose(); //message.Properties.Add() creates a copy...
            }
        }
    }

    abstract class SingletonConnectionReader
    {
        IConnection connection;
        bool doneReceiving;
        bool doneSending;
        bool isAtEof;
        bool isClosed;
        SecurityMessageProperty security;
        object thisLock = new object();
        int offset;
        int size;
        IConnectionOrientedTransportFactorySettings transportSettings;
        Uri via;
        Stream inputStream;

        protected SingletonConnectionReader(IConnection connection, int offset, int size, SecurityMessageProperty security,
            IConnectionOrientedTransportFactorySettings transportSettings, Uri via)
        {
            this.connection = connection;
            this.offset = offset;
            this.size = size;
            this.security = security;
            this.transportSettings = transportSettings;
            this.via = via;
        }

        protected IConnection Connection
        {
            get
            {
                return this.connection;
            }
        }

        protected object ThisLock
        {
            get
            {
                return this.thisLock;
            }
        }

        protected virtual string ContentType
        {
            get { return null; }
        }

        protected abstract long StreamPosition { get; }

        public void Abort()
        {
            this.connection.Abort();
        }

        public void DoneReceiving(bool atEof)
        {
            DoneReceiving(atEof, this.transportSettings.CloseTimeout);
        }

        void DoneReceiving(bool atEof, TimeSpan timeout)
        {
            if (!this.doneReceiving)
            {
                this.isAtEof = atEof;
                this.doneReceiving = true;

                if (this.doneSending)
                {
                    this.Close(timeout);
                }
            }
        }

        public void Close(TimeSpan timeout)
        {
            lock (ThisLock)
            {
                if (this.isClosed)
                {
                    return;
                }

                this.isClosed = true;
            }

            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            bool success = false;
            try
            {
                // first drain our stream if necessary
                if (this.inputStream != null)
                {
                    byte[] dummy = DiagnosticUtility.Utility.AllocateByteArray(transportSettings.ConnectionBufferSize);
                    while (!this.isAtEof)
                    {
                        this.inputStream.ReadTimeout = TimeoutHelper.ToMilliseconds(timeoutHelper.RemainingTime());
                        int bytesRead = this.inputStream.Read(dummy, 0, dummy.Length);
                        if (bytesRead == 0)
                        {
                            this.isAtEof = true;
                        }
                    }
                }
                OnClose(timeoutHelper.RemainingTime());
                success = true;
            }
            finally
            {
                if (!success)
                {
                    this.Abort();
                }
            }
        }

        protected abstract void OnClose(TimeSpan timeout);

        public void DoneSending(TimeSpan timeout)
        {
            this.doneSending = true;
            if (this.doneReceiving)
            {
                this.Close(timeout);
            }
        }

        protected abstract bool DecodeBytes(byte[] buffer, ref int offset, ref int size, ref bool isAtEof);

        protected virtual void PrepareMessage(Message message)
        {
            message.Properties.Via = this.via;
            message.Properties.Security = (this.security != null) ? (SecurityMessageProperty)this.security.CreateCopy() : null;
        }

        public RequestContext ReceiveRequest(TimeSpan timeout)
        {
            Message requestMessage = Receive(timeout);
            return new StreamedFramingRequestContext(this, requestMessage);
        }

        public IAsyncResult BeginReceive(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new ReceiveAsyncResult(this, timeout, callback, state);
        }

        public virtual Message EndReceive(IAsyncResult result)
        {
            return ReceiveAsyncResult.End(result);
        }

        public Message Receive(TimeSpan timeout)
        {
            byte[] buffer = DiagnosticUtility.Utility.AllocateByteArray(connection.AsyncReadBufferSize);

            if (size > 0)
            {
                Buffer.BlockCopy(connection.AsyncReadBuffer, offset, buffer, offset, size);
            }

            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            for (;;)
            {
                if (DecodeBytes(buffer, ref offset, ref size, ref isAtEof))
                {
                    break;
                }

                if (this.isAtEof)
                {
                    DoneReceiving(true, timeoutHelper.RemainingTime());
                    return null;
                }

                if (size == 0)
                {
                    offset = 0;
                    size = connection.Read(buffer, 0, buffer.Length, timeoutHelper.RemainingTime());
                    if (size == 0)
                    {
                        DoneReceiving(true, timeoutHelper.RemainingTime());
                        return null;
                    }
                }
            }

            // we're ready to read a message
            IConnection singletonConnection = this.connection;
            if (size > 0)
            {
                byte[] initialData = DiagnosticUtility.Utility.AllocateByteArray(size);
                Buffer.BlockCopy(buffer, offset, initialData, 0, size);
                singletonConnection = new PreReadConnection(singletonConnection, initialData);
            }

            Stream connectionStream = new SingletonInputConnectionStream(this, singletonConnection, this.transportSettings);
            this.inputStream = new MaxMessageSizeStream(connectionStream, transportSettings.MaxReceivedMessageSize);
            using (ServiceModelActivity activity = DiagnosticUtility.ShouldUseActivity ? ServiceModelActivity.CreateBoundedActivity(true) : null)
            {
                if (DiagnosticUtility.ShouldUseActivity)
                {
                    ServiceModelActivity.Start(activity, SR.GetString(SR.ActivityProcessingMessage, TraceUtility.RetrieveMessageNumber()), ActivityType.ProcessMessage);
                }

                Message message = null;
                try
                {
                    message = transportSettings.MessageEncoderFactory.Encoder.ReadMessage(
                        this.inputStream, transportSettings.MaxBufferSize, this.ContentType);
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

                PrepareMessage(message);

                return message;
            }
        }

        class ReceiveAsyncResult : AsyncResult
        {
            static Action<object> onReceiveScheduled = new Action<object>(OnReceiveScheduled);

            Message message;
            SingletonConnectionReader parent;
            TimeSpan timeout;

            public ReceiveAsyncResult(SingletonConnectionReader parent, TimeSpan timeout, AsyncCallback callback,
                object state)
                : base(callback, state)
            {
                this.parent = parent;
                this.timeout = timeout;

                // 
                ActionItem.Schedule(onReceiveScheduled, this);
            }

            public static Message End(IAsyncResult result)
            {
                ReceiveAsyncResult receiveAsyncResult = AsyncResult.End<ReceiveAsyncResult>(result);
                return receiveAsyncResult.message;
            }

            static void OnReceiveScheduled(object state)
            {
                ReceiveAsyncResult thisPtr = (ReceiveAsyncResult)state;

                Exception completionException = null;
                try
                {
                    thisPtr.message = thisPtr.parent.Receive(thisPtr.timeout);
                }
#pragma warning suppress 56500 // Microsoft, transferring exception to another thread
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    completionException = exception;
                }
                thisPtr.Complete(false, completionException);
            }
        }

        class StreamedFramingRequestContext : RequestContextBase
        {
            IConnection connection;
            SingletonConnectionReader parent;
            IConnectionOrientedTransportFactorySettings settings;
            TimeoutHelper timeoutHelper;

            public StreamedFramingRequestContext(SingletonConnectionReader parent, Message requestMessage)
                : base(requestMessage, parent.transportSettings.CloseTimeout, parent.transportSettings.SendTimeout)
            {
                this.parent = parent;
                this.connection = parent.connection;
                this.settings = parent.transportSettings;
            }

            protected override void OnAbort()
            {
                this.parent.Abort();
            }

            protected override void OnClose(TimeSpan timeout)
            {
                this.parent.Close(timeout);
            }

            protected override void OnReply(Message message, TimeSpan timeout)
            {
                ICompressedMessageEncoder compressedMessageEncoder = this.settings.MessageEncoderFactory.Encoder as ICompressedMessageEncoder;
                if (compressedMessageEncoder != null && compressedMessageEncoder.CompressionEnabled)
                {
                    compressedMessageEncoder.AddCompressedMessageProperties(message, this.parent.ContentType);
                }

                timeoutHelper = new TimeoutHelper(timeout);
                StreamingConnectionHelper.WriteMessage(message, this.connection, false, this.settings, ref timeoutHelper);
                parent.DoneSending(timeoutHelper.RemainingTime());
            }

            protected override IAsyncResult OnBeginReply(Message message, TimeSpan timeout, AsyncCallback callback, object state)
            {
                ICompressedMessageEncoder compressedMessageEncoder = this.settings.MessageEncoderFactory.Encoder as ICompressedMessageEncoder;
                if (compressedMessageEncoder != null && compressedMessageEncoder.CompressionEnabled)
                {
                    compressedMessageEncoder.AddCompressedMessageProperties(message, this.parent.ContentType);
                }

                timeoutHelper = new TimeoutHelper(timeout);
                return StreamingConnectionHelper.BeginWriteMessage(message, this.connection, false, this.settings,
                    ref timeoutHelper, callback, state);
            }

            protected override void OnEndReply(IAsyncResult result)
            {
                StreamingConnectionHelper.EndWriteMessage(result);
                parent.DoneSending(timeoutHelper.RemainingTime());
            }
        }

        // ensures that the reader is notified at end-of-stream, and takes care of the framing chunk headers
        class SingletonInputConnectionStream : ConnectionStream
        {
            SingletonMessageDecoder decoder;
            SingletonConnectionReader reader;
            bool atEof;
            byte[] chunkBuffer; // used for when we have overflow
            int chunkBufferOffset;
            int chunkBufferSize;
            int chunkBytesRemaining;

            public SingletonInputConnectionStream(SingletonConnectionReader reader, IConnection connection,
                IDefaultCommunicationTimeouts defaultTimeouts)
                : base(connection, defaultTimeouts)
            {
                this.reader = reader;
                this.decoder = new SingletonMessageDecoder(reader.StreamPosition);
                this.chunkBytesRemaining = 0;
                this.chunkBuffer = new byte[IntEncoder.MaxEncodedSize];
            }

            void AbortReader()
            {
                this.reader.Abort();
            }

            public override void Close()
            {
                this.reader.DoneReceiving(this.atEof);
            }

            // run chunk data through the decoder
            void DecodeData(byte[] buffer, int offset, int size)
            {
                while (size > 0)
                {
                    int bytesRead = decoder.Decode(buffer, offset, size);
                    offset += bytesRead;
                    size -= bytesRead;
                    Fx.Assert(decoder.CurrentState == SingletonMessageDecoder.State.ReadingEnvelopeBytes || decoder.CurrentState == SingletonMessageDecoder.State.ChunkEnd, "");
                }
            }

            // run the current data through the decoder to get valid message bytes
            void DecodeSize(byte[] buffer, ref int offset, ref int size)
            {
                while (size > 0)
                {
                    int bytesRead = decoder.Decode(buffer, offset, size);

                    if (bytesRead > 0)
                    {
                        offset += bytesRead;
                        size -= bytesRead;
                    }

                    switch (decoder.CurrentState)
                    {
                        case SingletonMessageDecoder.State.ChunkStart:
                            this.chunkBytesRemaining = decoder.ChunkSize;

                            // if we have overflow and we're not decoding out of our buffer, copy over
                            if (size > 0 && !object.ReferenceEquals(buffer, this.chunkBuffer))
                            {
                                Fx.Assert(size <= this.chunkBuffer.Length, "");
                                Buffer.BlockCopy(buffer, offset, this.chunkBuffer, 0, size);
                                this.chunkBufferOffset = 0;
                                this.chunkBufferSize = size;
                            }
                            return;

                        case SingletonMessageDecoder.State.End:
                            ProcessEof();
                            return;
                    }
                }
            }

            int ReadCore(byte[] buffer, int offset, int count)
            {
                int bytesRead = -1;
                try
                {
                    bytesRead = base.Read(buffer, offset, count);
                    if (bytesRead == 0)
                    {
                        ProcessEof();
                    }
                }
                finally
                {
                    if (bytesRead == -1)  // there was an exception
                    {
                        AbortReader();
                    }
                }

                return bytesRead;
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                int result = 0;
                while (true)
                {
                    if (count == 0)
                    {
                        return result;
                    }

                    if (this.atEof)
                    {
                        return result;
                    }

                    // first deal with any residual carryover
                    if (this.chunkBufferSize > 0)
                    {
                        int bytesToCopy = Math.Min(chunkBytesRemaining,
                            Math.Min(this.chunkBufferSize, count));

                        Buffer.BlockCopy(this.chunkBuffer, this.chunkBufferOffset, buffer, offset, bytesToCopy);
                        // keep decoder up to date
                        DecodeData(this.chunkBuffer, this.chunkBufferOffset, bytesToCopy);

                        this.chunkBufferOffset += bytesToCopy;
                        this.chunkBufferSize -= bytesToCopy;
                        this.chunkBytesRemaining -= bytesToCopy;
                        if (this.chunkBytesRemaining == 0 && this.chunkBufferSize > 0)
                        {
                            DecodeSize(this.chunkBuffer, ref this.chunkBufferOffset, ref this.chunkBufferSize);
                        }

                        result += bytesToCopy;
                        offset += bytesToCopy;
                        count -= bytesToCopy;
                    }
                    else if (chunkBytesRemaining > 0)
                    {
                        // We're in the middle of a chunk. Try and include the next chunk size as well

                        int bytesToRead = count;
                        if (int.MaxValue - chunkBytesRemaining >= IntEncoder.MaxEncodedSize)
                        {
                            bytesToRead = Math.Min(count, chunkBytesRemaining + IntEncoder.MaxEncodedSize);
                        }

                        int bytesRead = ReadCore(buffer, offset, bytesToRead);

                        // keep decoder up to date
                        DecodeData(buffer, offset, Math.Min(bytesRead, this.chunkBytesRemaining));

                        if (bytesRead > chunkBytesRemaining)
                        {
                            result += this.chunkBytesRemaining;
                            int overflowCount = bytesRead - chunkBytesRemaining;
                            int overflowOffset = offset + chunkBytesRemaining;
                            this.chunkBytesRemaining = 0;
                            // read at least part of the next chunk, and put any overflow in this.chunkBuffer
                            DecodeSize(buffer, ref overflowOffset, ref overflowCount);
                        }
                        else
                        {
                            result += bytesRead;
                            this.chunkBytesRemaining -= bytesRead;
                        }

                        return result;
                    }
                    else
                    {
                        // Final case: we have a new chunk. Read the size, and loop around again
                        if (count < IntEncoder.MaxEncodedSize)
                        {
                            // we don't have space for MaxEncodedSize, so it's worth the copy cost to read into a temp buffer
                            this.chunkBufferOffset = 0;
                            this.chunkBufferSize = ReadCore(this.chunkBuffer, 0, this.chunkBuffer.Length);
                            DecodeSize(this.chunkBuffer, ref this.chunkBufferOffset, ref this.chunkBufferSize);
                        }
                        else
                        {
                            int bytesRead = ReadCore(buffer, offset, IntEncoder.MaxEncodedSize);
                            int sizeOffset = offset;
                            DecodeSize(buffer, ref sizeOffset, ref bytesRead);
                        }
                    }
                }
            }

            void ProcessEof()
            {
                if (!this.atEof)
                {
                    this.atEof = true;
                    if (this.chunkBufferSize > 0 || this.chunkBytesRemaining > 0
                        || decoder.CurrentState != SingletonMessageDecoder.State.End)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(decoder.CreatePrematureEOFException());
                    }

                    this.reader.DoneReceiving(true);
                }
            }

            public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
            {
                return new ReadAsyncResult(this, buffer, offset, count, callback, state);
            }

            public override int EndRead(IAsyncResult result)
            {
                return ReadAsyncResult.End(result);
            }

            public class ReadAsyncResult : AsyncResult
            {
                SingletonInputConnectionStream parent;
                int result;

                public ReadAsyncResult(SingletonInputConnectionStream parent,
                    byte[] buffer, int offset, int count, AsyncCallback callback, object state)
                    : base(callback, state)
                {
                    this.parent = parent;

                    // 
                    this.result = this.parent.Read(buffer, offset, count);
                    base.Complete(true);
                }

                public static int End(IAsyncResult result)
                {
                    ReadAsyncResult thisPtr = AsyncResult.End<ReadAsyncResult>(result);
                    return thisPtr.result;
                }
            }
        }
    }

    static class StreamingConnectionHelper
    {
        public static void WriteMessage(Message message, IConnection connection, bool isRequest,
            IConnectionOrientedTransportFactorySettings settings, ref TimeoutHelper timeoutHelper)
        {
            byte[] endBytes = null;
            if (message != null)
            {
                MessageEncoder messageEncoder = settings.MessageEncoderFactory.Encoder;
                byte[] envelopeStartBytes = SingletonEncoder.EnvelopeStartBytes;

                bool writeStreamed;
                if (isRequest)
                {
                    endBytes = SingletonEncoder.EnvelopeEndFramingEndBytes;
                    writeStreamed = TransferModeHelper.IsRequestStreamed(settings.TransferMode);
                }
                else
                {
                    endBytes = SingletonEncoder.EnvelopeEndBytes;
                    writeStreamed = TransferModeHelper.IsResponseStreamed(settings.TransferMode);
                }

                if (writeStreamed)
                {
                    connection.Write(envelopeStartBytes, 0, envelopeStartBytes.Length, false, timeoutHelper.RemainingTime());
                    Stream connectionStream = new StreamingOutputConnectionStream(connection, settings);
                    Stream writeTimeoutStream = new TimeoutStream(connectionStream, ref timeoutHelper);
                    messageEncoder.WriteMessage(message, writeTimeoutStream);
                }
                else
                {
                    ArraySegment<byte> messageData = messageEncoder.WriteMessage(message,
                        int.MaxValue, settings.BufferManager, envelopeStartBytes.Length + IntEncoder.MaxEncodedSize);
                    messageData = SingletonEncoder.EncodeMessageFrame(messageData);
                    Buffer.BlockCopy(envelopeStartBytes, 0, messageData.Array, messageData.Offset - envelopeStartBytes.Length,
                        envelopeStartBytes.Length);
                    connection.Write(messageData.Array, messageData.Offset - envelopeStartBytes.Length,
                        messageData.Count + envelopeStartBytes.Length, true, timeoutHelper.RemainingTime(), settings.BufferManager);
                }
            }
            else if (isRequest) // context handles response end bytes
            {
                endBytes = SingletonEncoder.EndBytes;
            }

            if (endBytes != null)
            {
                connection.Write(endBytes, 0, endBytes.Length,
                    true, timeoutHelper.RemainingTime());
            }
        }

        public static IAsyncResult BeginWriteMessage(Message message, IConnection connection, bool isRequest,
            IConnectionOrientedTransportFactorySettings settings, ref TimeoutHelper timeoutHelper,
            AsyncCallback callback, object state)
        {
            return new WriteMessageAsyncResult(message, connection, isRequest, settings, ref timeoutHelper, callback, state);
        }

        public static void EndWriteMessage(IAsyncResult result)
        {
            WriteMessageAsyncResult.End(result);
        }

        // overrides ConnectionStream to add a Framing int at the beginning of each record
        class StreamingOutputConnectionStream : ConnectionStream
        {
            byte[] encodedSize;

            public StreamingOutputConnectionStream(IConnection connection, IDefaultCommunicationTimeouts timeouts)
                : base(connection, timeouts)
            {
                this.encodedSize = new byte[IntEncoder.MaxEncodedSize];
            }
            void WriteChunkSize(int size)
            {
                if (size > 0)
                {
                    int bytesEncoded = IntEncoder.Encode(size, encodedSize, 0);
                    base.Connection.Write(encodedSize, 0, bytesEncoded, false, TimeSpan.FromMilliseconds(this.WriteTimeout));
                }
            }

            public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
            {
                WriteChunkSize(count);
                return base.BeginWrite(buffer, offset, count, callback, state);
            }

            public override void WriteByte(byte value)
            {
                WriteChunkSize(1);
                base.WriteByte(value);
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                WriteChunkSize(count);
                base.Write(buffer, offset, count);
            }
        }

        class WriteMessageAsyncResult : AsyncResult
        {
            IConnection connection;
            MessageEncoder encoder;
            BufferManager bufferManager;
            Message message;
            static WaitCallback onWriteBufferedMessage;
            static WaitCallback onWriteStartBytes;
            static Action<object> onWriteStartBytesScheduled;
            static WaitCallback onWriteEndBytes =
                Fx.ThunkCallback(new WaitCallback(OnWriteEndBytes));
            byte[] bufferToFree;
            IConnectionOrientedTransportFactorySettings settings;
            TimeoutHelper timeoutHelper;
            byte[] endBytes;

            public WriteMessageAsyncResult(Message message, IConnection connection, bool isRequest,
                IConnectionOrientedTransportFactorySettings settings, ref TimeoutHelper timeoutHelper,
                AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.connection = connection;
                this.encoder = settings.MessageEncoderFactory.Encoder;
                this.bufferManager = settings.BufferManager;
                this.timeoutHelper = timeoutHelper;
                this.message = message;
                this.settings = settings;

                bool throwing = true;
                bool completeSelf = false;
                if (message == null)
                {
                    if (isRequest) // context takes care of the end bytes on Close/reader.EOF
                    {
                        this.endBytes = SingletonEncoder.EndBytes;
                    }
                    completeSelf = WriteEndBytes();
                }
                else
                {
                    try
                    {
                        byte[] envelopeStartBytes = SingletonEncoder.EnvelopeStartBytes;
                        bool writeStreamed;
                        if (isRequest)
                        {
                            this.endBytes = SingletonEncoder.EnvelopeEndFramingEndBytes;
                            writeStreamed = TransferModeHelper.IsRequestStreamed(settings.TransferMode);
                        }
                        else
                        {
                            this.endBytes = SingletonEncoder.EnvelopeEndBytes;
                            writeStreamed = TransferModeHelper.IsResponseStreamed(settings.TransferMode);
                        }

                        if (writeStreamed)
                        {
                            if (onWriteStartBytes == null)
                            {
                                onWriteStartBytes = Fx.ThunkCallback(new WaitCallback(OnWriteStartBytes));
                            }

                            AsyncCompletionResult writeStartBytesResult = connection.BeginWrite(envelopeStartBytes, 0, envelopeStartBytes.Length, true,
                                timeoutHelper.RemainingTime(), onWriteStartBytes, this);

                            if (writeStartBytesResult == AsyncCompletionResult.Completed)
                            {
                                if (onWriteStartBytesScheduled == null)
                                {
                                    onWriteStartBytesScheduled = new Action<object>(OnWriteStartBytes);
                                }
                                ActionItem.Schedule(onWriteStartBytesScheduled, this);
                            }
                        }
                        else
                        {
                            ArraySegment<byte> messageData = settings.MessageEncoderFactory.Encoder.WriteMessage(message,
                                int.MaxValue, this.bufferManager, envelopeStartBytes.Length + IntEncoder.MaxEncodedSize);
                            messageData = SingletonEncoder.EncodeMessageFrame(messageData);
                            this.bufferToFree = messageData.Array;
                            Buffer.BlockCopy(envelopeStartBytes, 0, messageData.Array, messageData.Offset - envelopeStartBytes.Length,
                                envelopeStartBytes.Length);

                            if (onWriteBufferedMessage == null)
                            {
                                onWriteBufferedMessage = Fx.ThunkCallback(new WaitCallback(OnWriteBufferedMessage));
                            }
                            AsyncCompletionResult writeBufferedResult =
                                connection.BeginWrite(messageData.Array, messageData.Offset - envelopeStartBytes.Length,
                                messageData.Count + envelopeStartBytes.Length, true, timeoutHelper.RemainingTime(),
                                onWriteBufferedMessage, this);

                            if (writeBufferedResult == AsyncCompletionResult.Completed)
                            {
                                completeSelf = HandleWriteBufferedMessage();
                            }
                        }
                        throwing = false;
                    }
                    finally
                    {
                        if (throwing)
                        {
                            Cleanup();
                        }
                    }
                }

                if (completeSelf)
                {
                    base.Complete(true);
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<WriteMessageAsyncResult>(result);
            }

            void Cleanup()
            {
                if (bufferToFree != null)
                {
                    this.bufferManager.ReturnBuffer(bufferToFree);
                }
            }

            bool HandleWriteStartBytes()
            {
                connection.EndWrite();
                Stream connectionStream = new StreamingOutputConnectionStream(connection, settings);
                Stream writeTimeoutStream = new TimeoutStream(connectionStream, ref timeoutHelper);
                this.encoder.WriteMessage(message, writeTimeoutStream);
                return WriteEndBytes();
            }

            bool HandleWriteBufferedMessage()
            {
                this.connection.EndWrite();
                return WriteEndBytes();
            }

            bool WriteEndBytes()
            {
                if (this.endBytes == null)
                {
                    Cleanup();
                    return true;
                }

                AsyncCompletionResult result = connection.BeginWrite(endBytes, 0,
                    endBytes.Length, true, timeoutHelper.RemainingTime(), onWriteEndBytes, this);

                if (result == AsyncCompletionResult.Queued)
                {
                    return false;
                }

                return HandleWriteEndBytes();
            }

            bool HandleWriteEndBytes()
            {
                this.connection.EndWrite();
                Cleanup();
                return true;
            }

            static void OnWriteStartBytes(object asyncState)
            {
                OnWriteStartBytesCallbackHelper(asyncState);
            }

            static void OnWriteStartBytesCallbackHelper(object asyncState)
            {
                WriteMessageAsyncResult thisPtr = (WriteMessageAsyncResult)asyncState;
                Exception completionException = null;
                bool completeSelf = false;
                bool throwing = true;
                try
                {
                    completeSelf = thisPtr.HandleWriteStartBytes();
                    throwing = false;
                }
#pragma warning suppress 56500 // Microsoft, transferring exception to another thread
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }
                    completeSelf = true;
                    completionException = e;
                }
                finally
                {
                    if (throwing)
                    {
                        thisPtr.Cleanup();
                    }
                }

                if (completeSelf)
                {
                    thisPtr.Complete(false, completionException);
                }
            }

            static void OnWriteBufferedMessage(object asyncState)
            {
                WriteMessageAsyncResult thisPtr = (WriteMessageAsyncResult)asyncState;

                Exception completionException = null;
                bool completeSelf = false;
                bool throwing = true;
                try
                {
                    completeSelf = thisPtr.HandleWriteBufferedMessage();
                    throwing = false;
                }
#pragma warning suppress 56500 // Microsoft, transferring exception to another thread
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }
                    completeSelf = true;
                    completionException = e;
                }
                finally
                {
                    if (throwing)
                    {
                        thisPtr.Cleanup();
                    }
                }
                if (completeSelf)
                {
                    thisPtr.Complete(false, completionException);
                }
            }

            static void OnWriteEndBytes(object asyncState)
            {
                WriteMessageAsyncResult thisPtr = (WriteMessageAsyncResult)asyncState;

                Exception completionException = null;
                bool completeSelf = false;
                bool success = false;
                try
                {
                    completeSelf = thisPtr.HandleWriteEndBytes();
                    success = true;
                }
#pragma warning suppress 56500 // Microsoft, transferring exception to another thread
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }
                    completeSelf = true;
                    completionException = e;
                }
                finally
                {
                    if (!success)
                    {
                        thisPtr.Cleanup();
                    }
                }

                if (completeSelf)
                {
                    thisPtr.Complete(false, completionException);
                }
            }
        }
    }
}
