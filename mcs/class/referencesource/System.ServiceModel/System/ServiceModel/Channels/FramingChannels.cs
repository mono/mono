//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Diagnostics;
    using System.IO;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Security;
    using System.Threading;
    using System.ServiceModel.Diagnostics.Application;
    using System.Security;
    using System.Security.Authentication.ExtendedProtection;
    using System.Security.Principal;
    using System.Runtime.Diagnostics;

    abstract class FramingDuplexSessionChannel : TransportDuplexSessionChannel
    {
        IConnection connection;
        bool exposeConnectionProperty; 

        FramingDuplexSessionChannel(ChannelManagerBase manager, IConnectionOrientedTransportFactorySettings settings,
            EndpointAddress localAddress, Uri localVia, EndpointAddress remoteAddresss, Uri via, bool exposeConnectionProperty)
            : base(manager, settings, localAddress, localVia, remoteAddresss, via)
        {
            this.exposeConnectionProperty = exposeConnectionProperty;
        }

        protected FramingDuplexSessionChannel(ChannelManagerBase factory, IConnectionOrientedTransportFactorySettings settings,
            EndpointAddress remoteAddresss, Uri via, bool exposeConnectionProperty)
            : this(factory, settings, EndpointAddress.AnonymousAddress, settings.MessageVersion.Addressing.AnonymousUri,
            remoteAddresss, via, exposeConnectionProperty)
        {
            this.Session = FramingConnectionDuplexSession.CreateSession(this, settings.Upgrade);
        }

        protected FramingDuplexSessionChannel(ConnectionOrientedTransportChannelListener channelListener,
            EndpointAddress localAddress, Uri localVia, bool exposeConnectionProperty)
            : this(channelListener, channelListener, localAddress, localVia,
            EndpointAddress.AnonymousAddress, channelListener.MessageVersion.Addressing.AnonymousUri, exposeConnectionProperty)
        {
            this.Session = FramingConnectionDuplexSession.CreateSession(this, channelListener.Upgrade);
        }

        protected IConnection Connection
        {
            get
            {
                return connection;
            }
            set
            {
                this.connection = value;
            }
        }

        protected override bool IsStreamedOutput
        {
            get { return false; }
        }

        protected override void CloseOutputSessionCore(TimeSpan timeout)
        {
            Connection.Write(SessionEncoder.EndBytes, 0, SessionEncoder.EndBytes.Length, true, timeout);
        }

        protected override void CompleteClose(TimeSpan timeout)
        {
            this.ReturnConnectionIfNecessary(false, timeout);
        }

        protected override void PrepareMessage(Message message)
        {
            if (exposeConnectionProperty)
            {
                message.Properties[ConnectionMessageProperty.Name] = this.connection;
            }
            base.PrepareMessage(message);
        }

        protected override void OnSendCore(Message message, TimeSpan timeout)
        {
            bool allowOutputBatching;
            ArraySegment<byte> messageData;
            allowOutputBatching = message.Properties.AllowOutputBatching;
            messageData = this.EncodeMessage(message);
            this.Connection.Write(messageData.Array, messageData.Offset, messageData.Count, !allowOutputBatching,
                timeout, this.BufferManager);
        }

        protected override AsyncCompletionResult BeginCloseOutput(TimeSpan timeout, WaitCallback callback, object state)
        { 
            return this.Connection.BeginWrite(SessionEncoder.EndBytes, 0, SessionEncoder.EndBytes.Length,
                    true, timeout, callback, state);
        }

        protected override void FinishWritingMessage()
        {
            this.Connection.EndWrite();
        }

        protected override AsyncCompletionResult StartWritingBufferedMessage(Message message, ArraySegment<byte> messageData, bool allowOutputBatching, TimeSpan timeout, WaitCallback callback, object state)
        {
            return this.Connection.BeginWrite(messageData.Array, messageData.Offset, messageData.Count,
                    !allowOutputBatching, timeout, callback, state);
        }

        protected override AsyncCompletionResult StartWritingStreamedMessage(Message message, TimeSpan timeout, WaitCallback callback, object state)
        {
            Fx.Assert(false, "Streamed output should never be called in this channel.");
            throw FxTrace.Exception.AsError(new InvalidOperationException());
        }

        protected override ArraySegment<byte> EncodeMessage(Message message)
        {
            ArraySegment<byte> messageData = MessageEncoder.WriteMessage(message,
                int.MaxValue, this.BufferManager, SessionEncoder.MaxMessageFrameSize);

            messageData = SessionEncoder.EncodeMessageFrame(messageData);

            return messageData;
        }

        class FramingConnectionDuplexSession : ConnectionDuplexSession
        {

            FramingConnectionDuplexSession(FramingDuplexSessionChannel channel)
                : base(channel)
            {
            }

            public static FramingConnectionDuplexSession CreateSession(FramingDuplexSessionChannel channel,
                StreamUpgradeProvider upgrade)
            {
                StreamSecurityUpgradeProvider security = upgrade as StreamSecurityUpgradeProvider;
                if (security == null)
                {
                    return new FramingConnectionDuplexSession(channel);
                }
                else
                {
                    return new SecureConnectionDuplexSession(channel);
                }
            }

            class SecureConnectionDuplexSession : FramingConnectionDuplexSession, ISecuritySession
            {
                EndpointIdentity remoteIdentity;

                public SecureConnectionDuplexSession(FramingDuplexSessionChannel channel)
                    : base(channel)
                {
                    // empty
                }

                EndpointIdentity ISecuritySession.RemoteIdentity
                {
                    get
                    {
                        if (remoteIdentity == null)
                        {
                            SecurityMessageProperty security = this.Channel.RemoteSecurity;
                            if (security != null && security.ServiceSecurityContext != null &&
                                security.ServiceSecurityContext.IdentityClaim != null &&
                                security.ServiceSecurityContext.PrimaryIdentity != null)
                            {
                                this.remoteIdentity = EndpointIdentity.CreateIdentity(
                                    security.ServiceSecurityContext.IdentityClaim);
                            }
                        }

                        return this.remoteIdentity;
                    }
                }
            }
        }
    }

    class ClientFramingDuplexSessionChannel : FramingDuplexSessionChannel
    {
        IConnectionOrientedTransportChannelFactorySettings settings;
        ClientDuplexDecoder decoder;
        StreamUpgradeProvider upgrade;
        ConnectionPoolHelper connectionPoolHelper;
        bool flowIdentity;

        public ClientFramingDuplexSessionChannel(ChannelManagerBase factory, IConnectionOrientedTransportChannelFactorySettings settings,
            EndpointAddress remoteAddresss, Uri via, IConnectionInitiator connectionInitiator, ConnectionPool connectionPool,
            bool exposeConnectionProperty, bool flowIdentity)
            : base(factory, settings, remoteAddresss, via, exposeConnectionProperty)
        {
            this.settings = settings;
            this.MessageEncoder = settings.MessageEncoderFactory.CreateSessionEncoder();
            this.upgrade = settings.Upgrade;
            this.flowIdentity = flowIdentity;
            this.connectionPoolHelper = new DuplexConnectionPoolHelper(this, connectionPool, connectionInitiator);
        }

        ArraySegment<byte> CreatePreamble()
        {
            EncodedVia encodedVia = new EncodedVia(this.Via.AbsoluteUri);
            EncodedContentType encodedContentType = EncodedContentType.Create(this.MessageEncoder.ContentType);

            // calculate preamble length
            int startSize = ClientDuplexEncoder.ModeBytes.Length + SessionEncoder.CalcStartSize(encodedVia, encodedContentType);
            int preambleEndOffset = 0;
            if (this.upgrade == null)
            {
                preambleEndOffset = startSize;
                startSize += ClientDuplexEncoder.PreambleEndBytes.Length;
            }

            byte[] startBytes = DiagnosticUtility.Utility.AllocateByteArray(startSize);
            Buffer.BlockCopy(ClientDuplexEncoder.ModeBytes, 0, startBytes, 0, ClientDuplexEncoder.ModeBytes.Length);
            SessionEncoder.EncodeStart(startBytes, ClientDuplexEncoder.ModeBytes.Length, encodedVia, encodedContentType);
            if (preambleEndOffset > 0)
            {
                Buffer.BlockCopy(ClientDuplexEncoder.PreambleEndBytes, 0, startBytes, preambleEndOffset, ClientDuplexEncoder.PreambleEndBytes.Length);
            }

            return new ArraySegment<byte>(startBytes, 0, startSize);
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new OpenAsyncResult(this, timeout, callback, state);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            OpenAsyncResult.End(result);
        }

        public override T GetProperty<T>()
        {
            T result = base.GetProperty<T>();

            if (result == null && this.upgrade != null)
            {
                result = this.upgrade.GetProperty<T>();
            }

            return result;
        }

        IConnection SendPreamble(IConnection connection, ArraySegment<byte> preamble, ref TimeoutHelper timeoutHelper)
        {
            if (TD.ClientSendPreambleStartIsEnabled())
            {
                TD.ClientSendPreambleStart(this.EventTraceActivity);
            }

            // initialize a new decoder
            this.decoder = new ClientDuplexDecoder(0);
            byte[] ackBuffer = new byte[1];
            connection.Write(preamble.Array, preamble.Offset, preamble.Count, true, timeoutHelper.RemainingTime());

            if (this.upgrade != null)
            {
                IStreamUpgradeChannelBindingProvider channelBindingProvider = upgrade.GetProperty<IStreamUpgradeChannelBindingProvider>();
                StreamUpgradeInitiator upgradeInitiator = upgrade.CreateUpgradeInitiator(this.RemoteAddress, this.Via);

                upgradeInitiator.Open(timeoutHelper.RemainingTime());
                if (!ConnectionUpgradeHelper.InitiateUpgrade(upgradeInitiator, ref connection, this.decoder,
                    this, ref timeoutHelper))
                {
                    ConnectionUpgradeHelper.DecodeFramingFault(this.decoder, connection, Via, MessageEncoder.ContentType, ref timeoutHelper);
                }

                if (channelBindingProvider != null && channelBindingProvider.IsChannelBindingSupportEnabled)
                {
                    this.SetChannelBinding(channelBindingProvider.GetChannelBinding(upgradeInitiator, ChannelBindingKind.Endpoint));
                }

                SetRemoteSecurity(upgradeInitiator);
                upgradeInitiator.Close(timeoutHelper.RemainingTime());
                connection.Write(ClientDuplexEncoder.PreambleEndBytes, 0,
                    ClientDuplexEncoder.PreambleEndBytes.Length, true, timeoutHelper.RemainingTime());
            }

            // read ACK
            int ackBytesRead = connection.Read(ackBuffer, 0, ackBuffer.Length, timeoutHelper.RemainingTime());
            if (!ConnectionUpgradeHelper.ValidatePreambleResponse(ackBuffer, ackBytesRead, this.decoder, Via))
            {
                ConnectionUpgradeHelper.DecodeFramingFault(this.decoder, connection, Via,
                    MessageEncoder.ContentType, ref timeoutHelper);
            }

            if (TD.ClientSendPreambleStopIsEnabled())
            {
                TD.ClientSendPreambleStop(this.EventTraceActivity);
            }

            return connection;
        }

        IAsyncResult BeginSendPreamble(IConnection connection, ArraySegment<byte> preamble, ref TimeoutHelper timeoutHelper,
            AsyncCallback callback, object state)
        {
            return new SendPreambleAsyncResult(this, connection, preamble, this.flowIdentity, ref timeoutHelper, callback, state);
        }

        IConnection EndSendPreamble(IAsyncResult result)
        {
            return SendPreambleAsyncResult.End(result);
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            IConnection connection;
            try
            {
                connection = connectionPoolHelper.EstablishConnection(timeout);
            }
            catch (TimeoutException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new TimeoutException(SR.GetString(SR.TimeoutOnOpen, timeout), exception));
            }

            bool connectionAccepted = false;
            try
            {
                AcceptConnection(connection);
                connectionAccepted = true;
            }
            finally
            {
                if (!connectionAccepted)
                {
                    this.connectionPoolHelper.Abort();
                }
            }
        }

        protected override void ReturnConnectionIfNecessary(bool abort, TimeSpan timeout)
        {
            lock (ThisLock)
            {
                if (abort)
                {
                    this.connectionPoolHelper.Abort();
                }
                else
                {
                    this.connectionPoolHelper.Close(timeout);
                }
            }
        }

        void AcceptConnection(IConnection connection)
        {
            base.SetMessageSource(new ClientDuplexConnectionReader(this, connection, decoder, this.settings, MessageEncoder));

            lock (ThisLock)
            {
                if (this.State != CommunicationState.Opening)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new CommunicationObjectAbortedException(SR.GetString(SR.DuplexChannelAbortedDuringOpen, this.Via)));
                }

                this.Connection = connection;
            }
        }

        void SetRemoteSecurity(StreamUpgradeInitiator upgradeInitiator)
        {
            this.RemoteSecurity = StreamSecurityUpgradeInitiator.GetRemoteSecurity(upgradeInitiator);
        }

        protected override void PrepareMessage(Message message)
        {
            base.PrepareMessage(message);

            if (this.RemoteSecurity != null)
            {
                message.Properties.Security = (SecurityMessageProperty)this.RemoteSecurity.CreateCopy();
            }
        }

        class DuplexConnectionPoolHelper : ConnectionPoolHelper
        {
            ClientFramingDuplexSessionChannel channel;
            ArraySegment<byte> preamble;

            public DuplexConnectionPoolHelper(ClientFramingDuplexSessionChannel channel,
                ConnectionPool connectionPool, IConnectionInitiator connectionInitiator)
                : base(connectionPool, connectionInitiator, channel.Via)
            {
                this.channel = channel;
                this.preamble = channel.CreatePreamble();
            }

            protected override TimeoutException CreateNewConnectionTimeoutException(TimeSpan timeout, TimeoutException innerException)
            {
                return new TimeoutException(SR.GetString(SR.OpenTimedOutEstablishingTransportSession,
                        timeout, channel.Via.AbsoluteUri), innerException);
            }

            protected override IAsyncResult BeginAcceptPooledConnection(IConnection connection, ref TimeoutHelper timeoutHelper, AsyncCallback callback, object state)
            {
                return channel.BeginSendPreamble(connection, preamble, ref timeoutHelper, callback, state);
            }

            protected override IConnection EndAcceptPooledConnection(IAsyncResult result)
            {
                return channel.EndSendPreamble(result);
            }

            protected override IConnection AcceptPooledConnection(IConnection connection, ref TimeoutHelper timeoutHelper)
            {
                return channel.SendPreamble(connection, preamble, ref timeoutHelper);
            }
        }

        class SendPreambleAsyncResult : AsyncResult
        {
            ClientFramingDuplexSessionChannel channel;
            IConnection connection;
            TimeoutHelper timeoutHelper;
            StreamUpgradeInitiator upgradeInitiator;
            IStreamUpgradeChannelBindingProvider channelBindingProvider;
            static WaitCallback onReadPreambleAck = new WaitCallback(OnReadPreambleAck);
            static WaitCallback onWritePreamble = Fx.ThunkCallback(new WaitCallback(OnWritePreamble));
            static WaitCallback onWritePreambleEnd;
            static AsyncCallback onUpgrade;
            static AsyncCallback onUpgradeInitiatorOpen;
            static AsyncCallback onUpgradeInitiatorClose;
            WindowsIdentity identityToImpersonate;
            EventTraceActivity eventTraceActivity;

            public SendPreambleAsyncResult(ClientFramingDuplexSessionChannel channel,
                IConnection connection, ArraySegment<byte> preamble, bool flowIdentity,
                ref TimeoutHelper timeoutHelper, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.channel = channel;
                this.timeoutHelper = timeoutHelper;
                this.connection = connection;

                if (TD.ClientSendPreambleStartIsEnabled())
                {
                    TD.ClientSendPreambleStart(this.EventTraceActivity);
                }

                if (flowIdentity && !SecurityContext.IsWindowsIdentityFlowSuppressed())
                {
                    this.identityToImpersonate = WindowsIdentity.GetCurrent(true);
                }

                // initialize a new decoder
                channel.decoder = new ClientDuplexDecoder(0);
                AsyncCompletionResult writePreambleResult = connection.BeginWrite(
                    preamble.Array, preamble.Offset, preamble.Count,
                        true, timeoutHelper.RemainingTime(), onWritePreamble, this);

                if (writePreambleResult == AsyncCompletionResult.Queued)
                {
                    return;
                }

                if (HandleWritePreamble())
                {
                    base.Complete(true);
                }
            }

            EventTraceActivity EventTraceActivity
            {
                get
                {
                    if (this.eventTraceActivity == null)
                    {
                        this.eventTraceActivity = EventTraceActivity.GetFromThreadOrCreate();
                    }
                    return this.eventTraceActivity;
                }
            }

            public static IConnection End(IAsyncResult result)
            {
                SendPreambleAsyncResult thisPtr = AsyncResult.End<SendPreambleAsyncResult>(result);
                return thisPtr.connection;
            }

            bool HandleWritePreamble()
            {
                this.connection.EndWrite();

                if (TD.ClientSendPreambleStopIsEnabled())
                {
                    TD.ClientSendPreambleStop(this.EventTraceActivity);
                }

                // now upgrade if necessary
                if (channel.upgrade != null)
                {
                    this.channelBindingProvider = this.channel.upgrade.GetProperty<IStreamUpgradeChannelBindingProvider>();
                    this.upgradeInitiator = channel.upgrade.CreateUpgradeInitiator(channel.RemoteAddress, channel.Via);
                    if (onUpgradeInitiatorOpen == null)
                    {
                        onUpgradeInitiatorOpen = Fx.ThunkCallback(new AsyncCallback(OnUpgradeInitiatorOpen));
                    }

                    IAsyncResult initiatorOpenResult =
                        this.upgradeInitiator.BeginOpen(timeoutHelper.RemainingTime(), onUpgradeInitiatorOpen, this);

                    if (!initiatorOpenResult.CompletedSynchronously)
                    {
                        return false;
                    }

                    return HandleInitiatorOpen(initiatorOpenResult);
                }
                else
                {
                    return ReadAck();
                }
            }

            bool HandleInitiatorOpen(IAsyncResult result)
            {
                this.upgradeInitiator.EndOpen(result);
                if (onUpgrade == null)
                {
                    onUpgrade = Fx.ThunkCallback(new AsyncCallback(OnUpgrade));
                }

                IAsyncResult upgradeResult = ConnectionUpgradeHelper.BeginInitiateUpgrade(
                    channel, channel.RemoteAddress, this.connection, channel.decoder, this.upgradeInitiator,
                    channel.MessageEncoder.ContentType, this.identityToImpersonate, timeoutHelper, onUpgrade, this);

                if (!upgradeResult.CompletedSynchronously)
                {
                    return false;
                }

                return HandleUpgrade(upgradeResult);
            }

            // finish our upgrade and read ack
            bool HandleUpgrade(IAsyncResult result)
            {
                this.connection = ConnectionUpgradeHelper.EndInitiateUpgrade(result);

                if (this.channelBindingProvider != null && this.channelBindingProvider.IsChannelBindingSupportEnabled)
                {
                    this.channel.SetChannelBinding(this.channelBindingProvider.GetChannelBinding(this.upgradeInitiator, ChannelBindingKind.Endpoint));
                }

                channel.SetRemoteSecurity(this.upgradeInitiator);

                if (onUpgradeInitiatorClose == null)
                {
                    onUpgradeInitiatorClose = Fx.ThunkCallback(new AsyncCallback(OnUpgradeInitiatorClose));
                }

                IAsyncResult initiatorCloseResult =
                    this.upgradeInitiator.BeginClose(timeoutHelper.RemainingTime(), onUpgradeInitiatorClose, this);

                if (!initiatorCloseResult.CompletedSynchronously)
                {
                    return false;
                }

                return HandleInitiatorClose(initiatorCloseResult);
            }

            bool HandleInitiatorClose(IAsyncResult result)
            {
                this.upgradeInitiator.EndClose(result);
                this.upgradeInitiator = null; // we're done with the upgrade

                // in the upgrade case, preamble end bytes aren't sent with the initial bytes; we need to send them here
                if (onWritePreambleEnd == null)
                {
                    onWritePreambleEnd = Fx.ThunkCallback(new WaitCallback(OnWritePreambleEnd));
                }

                AsyncCompletionResult writePreambleResult = this.connection.BeginWrite(
                    ClientDuplexEncoder.PreambleEndBytes, 0, ClientDuplexEncoder.PreambleEndBytes.Length, true,
                    timeoutHelper.RemainingTime(), onWritePreambleEnd, this);

                if (writePreambleResult == AsyncCompletionResult.Queued)
                {
                    return false;
                }

                this.connection.EndWrite();
                return ReadAck();
            }

            bool ReadAck()
            {
                AsyncCompletionResult readAckResult = this.connection.BeginRead(0, 1,
                    timeoutHelper.RemainingTime(), onReadPreambleAck, this);

                if (readAckResult == AsyncCompletionResult.Queued)
                {
                    return false;
                }

                return HandlePreambleAck();
            }

            bool HandlePreambleAck()
            {
                int ackBytesRead = connection.EndRead();

                // it's possible to get a fault instead of an ack
                if (!ConnectionUpgradeHelper.ValidatePreambleResponse(
                    connection.AsyncReadBuffer, ackBytesRead, channel.decoder, channel.Via))
                {
                    IAsyncResult decodeFaultResult = ConnectionUpgradeHelper.BeginDecodeFramingFault(channel.decoder,
                        connection, channel.Via, channel.MessageEncoder.ContentType, ref timeoutHelper,
                        Fx.ThunkCallback(new AsyncCallback(OnFailedPreamble)), this);

                    if (!decodeFaultResult.CompletedSynchronously)
                    {
                        return false;
                    }

                    ConnectionUpgradeHelper.EndDecodeFramingFault(decodeFaultResult);
                    return true;
                }

                return true;
            }

            static void OnWritePreamble(object asyncState)
            {
                SendPreambleAsyncResult thisPtr = (SendPreambleAsyncResult)asyncState;

                Exception completionException = null;
                bool completeSelf = false;
                try
                {
                    completeSelf = thisPtr.HandleWritePreamble();
                }
#pragma warning suppress 56500 // [....], transferring exception to another thread
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    completeSelf = true;
                    completionException = e;
                }

                if (completeSelf)
                {
                    thisPtr.Complete(false, completionException);
                }
            }

            static void OnReadPreambleAck(object state)
            {
                SendPreambleAsyncResult thisPtr = (SendPreambleAsyncResult)state;

                Exception completionException = null;
                bool completeSelf;
                try
                {
                    completeSelf = thisPtr.HandlePreambleAck();
                }
#pragma warning suppress 56500 // [....], transferring exception to another thread
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    completeSelf = true;
                    completionException = e;
                }

                if (completeSelf)
                {
                    thisPtr.Complete(false, completionException);
                }
            }

            static void OnUpgradeInitiatorOpen(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }

                SendPreambleAsyncResult thisPtr = (SendPreambleAsyncResult)result.AsyncState;

                bool completeSelf = false;
                Exception completionException = null;
                try
                {
                    completeSelf = thisPtr.HandleInitiatorOpen(result);
                }
#pragma warning suppress 56500 // [....], transferring exception to another thread
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    completeSelf = true;
                    completionException = e;
                }

                if (completeSelf)
                {
                    thisPtr.Complete(false, completionException);
                }
            }

            static void OnUpgrade(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }

                SendPreambleAsyncResult thisPtr = (SendPreambleAsyncResult)result.AsyncState;

                bool completeSelf = false;
                Exception completionException = null;
                try
                {
                    completeSelf = thisPtr.HandleUpgrade(result);
                }
#pragma warning suppress 56500 // [....], transferring exception to another thread
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    completeSelf = true;
                    completionException = e;
                }

                if (completeSelf)
                {
                    thisPtr.Complete(false, completionException);
                }
            }

            static void OnUpgradeInitiatorClose(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }

                SendPreambleAsyncResult thisPtr = (SendPreambleAsyncResult)result.AsyncState;

                bool completeSelf = false;
                Exception completionException = null;
                try
                {
                    completeSelf = thisPtr.HandleInitiatorClose(result);
                }
#pragma warning suppress 56500 // [....], transferring exception to another thread
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    completeSelf = true;
                    completionException = e;
                }

                if (completeSelf)
                {
                    thisPtr.Complete(false, completionException);
                }
            }

            static void OnWritePreambleEnd(object asyncState)
            {
                SendPreambleAsyncResult thisPtr = (SendPreambleAsyncResult)asyncState;
                Exception completionException = null;
                bool completeSelf = false;
                try
                {
                    thisPtr.connection.EndWrite();
                    completeSelf = thisPtr.ReadAck();
                }
#pragma warning suppress 56500 // [....], transferring exception to another thread
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    completeSelf = true;
                    completionException = e;
                }

                if (completeSelf)
                {
                    thisPtr.Complete(false, completionException);
                }
            }

            void OnFailedPreamble(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }

                Exception completionException = null;
                try
                {
                    ConnectionUpgradeHelper.EndDecodeFramingFault(result);
                }
#pragma warning suppress 56500 // [....], transferring exception to another thread
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    completionException = e;
                }

                base.Complete(false, completionException);
            }
        }

        class OpenAsyncResult : AsyncResult
        {
            static AsyncCallback onEstablishConnection = Fx.ThunkCallback(new AsyncCallback(OnEstablishConnection));
            ClientFramingDuplexSessionChannel duplexChannel;
            TimeoutHelper timeoutHelper;

            public OpenAsyncResult(ClientFramingDuplexSessionChannel duplexChannel, TimeSpan timeout, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.timeoutHelper = new TimeoutHelper(timeout);
                this.duplexChannel = duplexChannel;

                IAsyncResult result;
                try
                {
                    result = duplexChannel.connectionPoolHelper.BeginEstablishConnection(
                        timeoutHelper.RemainingTime(), onEstablishConnection, this);
                }
                catch (TimeoutException exception)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new TimeoutException(SR.GetString(SR.TimeoutOnOpen, timeout), exception));
                }

                if (!result.CompletedSynchronously)
                {
                    return;
                }

                if (HandleEstablishConnection(result))
                {
                    base.Complete(true);
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<OpenAsyncResult>(result);
            }

            bool HandleEstablishConnection(IAsyncResult result)
            {
                IConnection connection;
                try
                {
                    connection = duplexChannel.connectionPoolHelper.EndEstablishConnection(result);
                }
                catch (TimeoutException exception)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new TimeoutException(SR.GetString(SR.TimeoutOnOpen, this.timeoutHelper.OriginalTimeout), exception));
                }

                duplexChannel.AcceptConnection(connection);
                return true;
            }

            static void OnEstablishConnection(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }

                OpenAsyncResult thisPtr = (OpenAsyncResult)result.AsyncState;

                Exception completionException = null;
                bool completeSelf;
                try
                {
                    completeSelf = thisPtr.HandleEstablishConnection(result);
                }
#pragma warning suppress 56500 // [....], transferring exception to another thread
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    completeSelf = true;
                    completionException = e;
                }

                if (completeSelf)
                {
                    thisPtr.Complete(false, completionException);
                }
            }
        }
    }

    // used by StreamedFramingRequestChannel and ClientFramingDuplexSessionChannel
    class ConnectionUpgradeHelper
    {
        public static IAsyncResult BeginDecodeFramingFault(ClientFramingDecoder decoder, IConnection connection,
            Uri via, string contentType, ref TimeoutHelper timeoutHelper, AsyncCallback callback, object state)
        {
            return new DecodeFailedUpgradeAsyncResult(decoder, connection, via, contentType, ref timeoutHelper,
                callback, state);
        }

        public static void EndDecodeFramingFault(IAsyncResult result)
        {
            DecodeFailedUpgradeAsyncResult.End(result);
        }

        public static void DecodeFramingFault(ClientFramingDecoder decoder, IConnection connection,
            Uri via, string contentType, ref TimeoutHelper timeoutHelper)
        {
            ValidateReadingFaultString(decoder);

            int offset = 0;
            byte[] faultBuffer = DiagnosticUtility.Utility.AllocateByteArray(FaultStringDecoder.FaultSizeQuota);
            int size = connection.Read(faultBuffer, offset, faultBuffer.Length, timeoutHelper.RemainingTime());

            while (size > 0)
            {
                int bytesDecoded = decoder.Decode(faultBuffer, offset, size);
                offset += bytesDecoded;
                size -= bytesDecoded;

                if (decoder.CurrentState == ClientFramingDecoderState.Fault)
                {
                    ConnectionUtilities.CloseNoThrow(connection, timeoutHelper.RemainingTime());
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        FaultStringDecoder.GetFaultException(decoder.Fault, via.ToString(), contentType));
                }
                else
                {
                    if (decoder.CurrentState != ClientFramingDecoderState.ReadingFaultString)
                    {
                        throw Fx.AssertAndThrow("invalid framing client state machine");
                    }
                    if (size == 0)
                    {
                        offset = 0;
                        size = connection.Read(faultBuffer, offset, faultBuffer.Length, timeoutHelper.RemainingTime());
                    }
                }
            }

            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(decoder.CreatePrematureEOFException());
        }

        public static IAsyncResult BeginInitiateUpgrade(IDefaultCommunicationTimeouts timeouts, EndpointAddress remoteAddress,
            IConnection connection, ClientFramingDecoder decoder,
            StreamUpgradeInitiator upgradeInitiator, string contentType, WindowsIdentity identityToImpersonate, TimeoutHelper timeoutHelper,
            AsyncCallback callback, object state)
        {
            return new InitiateUpgradeAsyncResult(timeouts, remoteAddress, connection, decoder, upgradeInitiator, contentType, identityToImpersonate, timeoutHelper,
                callback, state);
        }

        public static IConnection EndInitiateUpgrade(IAsyncResult result)
        {
            return InitiateUpgradeAsyncResult.End(result);
        }

        public static bool InitiateUpgrade(StreamUpgradeInitiator upgradeInitiator, ref IConnection connection,
            ClientFramingDecoder decoder, IDefaultCommunicationTimeouts defaultTimeouts, ref TimeoutHelper timeoutHelper)
        {
            string upgradeContentType = upgradeInitiator.GetNextUpgrade();

            while (upgradeContentType != null)
            {
                EncodedUpgrade encodedUpgrade = new EncodedUpgrade(upgradeContentType);
                // write upgrade request framing for synchronization
                connection.Write(encodedUpgrade.EncodedBytes, 0, encodedUpgrade.EncodedBytes.Length, true, timeoutHelper.RemainingTime());
                byte[] buffer = new byte[1];

                // read upgrade response framing 
                int size = connection.Read(buffer, 0, buffer.Length, timeoutHelper.RemainingTime());

                if (!ValidateUpgradeResponse(buffer, size, decoder)) // we have a problem
                {
                    return false;
                }

                // initiate wire upgrade
                ConnectionStream connectionStream = new ConnectionStream(connection, defaultTimeouts);
                Stream upgradedStream = upgradeInitiator.InitiateUpgrade(connectionStream);

                // and re-wrap connection
                connection = new StreamConnection(upgradedStream, connectionStream);

                upgradeContentType = upgradeInitiator.GetNextUpgrade();
            }

            return true;
        }

        static void ValidateReadingFaultString(ClientFramingDecoder decoder)
        {
            if (decoder.CurrentState != ClientFramingDecoderState.ReadingFaultString)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new System.ServiceModel.Security.MessageSecurityException(
                    SR.GetString(SR.ServerRejectedUpgradeRequest)));
            }
        }

        public static bool ValidatePreambleResponse(byte[] buffer, int count, ClientFramingDecoder decoder, Uri via)
        {
            if (count == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new ProtocolException(SR.GetString(SR.ServerRejectedSessionPreamble, via),
                    decoder.CreatePrematureEOFException()));
            }

            // decode until the framing byte has been processed (it always will be)
            while (decoder.Decode(buffer, 0, count) == 0)
            {
                // do nothing
            }

            if (decoder.CurrentState != ClientFramingDecoderState.Start) // we have a problem
            {
                return false;
            }

            return true;
        }

        static bool ValidateUpgradeResponse(byte[] buffer, int count, ClientFramingDecoder decoder)
        {
            if (count == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.ServerRejectedUpgradeRequest), decoder.CreatePrematureEOFException()));
            }

            // decode until the framing byte has been processed (it always will be)
            while (decoder.Decode(buffer, 0, count) == 0)
            {
                // do nothing
            }

            if (decoder.CurrentState != ClientFramingDecoderState.UpgradeResponse) // we have a problem
            {
                return false;
            }

            return true;
        }

        class DecodeFailedUpgradeAsyncResult : AsyncResult
        {
            ClientFramingDecoder decoder;
            IConnection connection;
            Uri via;
            string contentType;
            TimeoutHelper timeoutHelper;
            static WaitCallback onReadFaultData = new WaitCallback(OnReadFaultData);

            public DecodeFailedUpgradeAsyncResult(ClientFramingDecoder decoder, IConnection connection,
                Uri via, string contentType, ref TimeoutHelper timeoutHelper,
                AsyncCallback callback, object state)
                : base(callback, state)
            {
                ValidateReadingFaultString(decoder);

                this.decoder = decoder;
                this.connection = connection;
                this.via = via;
                this.contentType = contentType;
                this.timeoutHelper = timeoutHelper;

                if (connection.BeginRead(0, Math.Min(FaultStringDecoder.FaultSizeQuota, connection.AsyncReadBufferSize),
                    timeoutHelper.RemainingTime(), onReadFaultData, this) == AsyncCompletionResult.Queued)
                {
                    return;
                }

                CompleteReadFaultData();
            }

            void CompleteReadFaultData()
            {
                int offset = 0;
                int size = connection.EndRead();

                while (size > 0)
                {
                    int bytesDecoded = decoder.Decode(connection.AsyncReadBuffer, offset, size);
                    offset += bytesDecoded;
                    size -= bytesDecoded;

                    if (decoder.CurrentState == ClientFramingDecoderState.Fault)
                    {
                        ConnectionUtilities.CloseNoThrow(connection, timeoutHelper.RemainingTime());
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                            FaultStringDecoder.GetFaultException(decoder.Fault, via.ToString(), contentType));
                    }
                    else
                    {
                        if (decoder.CurrentState != ClientFramingDecoderState.ReadingFaultString)
                        {
                            throw Fx.AssertAndThrow("invalid framing client state machine");
                        }
                        if (size == 0)
                        {
                            offset = 0;
                            if (connection.BeginRead(0, Math.Min(FaultStringDecoder.FaultSizeQuota, connection.AsyncReadBufferSize),
                                timeoutHelper.RemainingTime(), onReadFaultData, this) == AsyncCompletionResult.Queued)
                            {
                                return;
                            }

                            size = connection.EndRead();
                        }
                    }
                }

                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(decoder.CreatePrematureEOFException());
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<DecodeFailedUpgradeAsyncResult>(result);
            }

            static void OnReadFaultData(object state)
            {
                DecodeFailedUpgradeAsyncResult thisPtr = (DecodeFailedUpgradeAsyncResult)state;

                // This AsyncResult only completes with an exception.
                Exception completionException = null;
                try
                {
                    thisPtr.CompleteReadFaultData();
                }
#pragma warning suppress 56500 // [....], transferring exception to another thread
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    completionException = e;
                }
                if (completionException != null)
                {
                    thisPtr.Complete(false, completionException);
                }
            }
        }

        class InitiateUpgradeAsyncResult : AsyncResult
        {
            IDefaultCommunicationTimeouts defaultTimeouts;
            IConnection connection;
            ConnectionStream connectionStream;
            string contentType;
            ClientFramingDecoder decoder;
            static AsyncCallback onInitiateUpgrade = Fx.ThunkCallback(new AsyncCallback(OnInitiateUpgrade));
            static WaitCallback onReadUpgradeResponse = new WaitCallback(OnReadUpgradeResponse);
            static AsyncCallback onFailedUpgrade;
            static WaitCallback onWriteUpgradeBytes = Fx.ThunkCallback(new WaitCallback(OnWriteUpgradeBytes));
            EndpointAddress remoteAddress;
            StreamUpgradeInitiator upgradeInitiator;
            TimeoutHelper timeoutHelper;
            WindowsIdentity identityToImpersonate;

            public InitiateUpgradeAsyncResult(IDefaultCommunicationTimeouts timeouts, EndpointAddress remoteAddress,
                IConnection connection,
                ClientFramingDecoder decoder, StreamUpgradeInitiator upgradeInitiator,
                string contentType, WindowsIdentity identityToImpersonate, TimeoutHelper timeoutHelper,
                AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.defaultTimeouts = timeouts;
                this.decoder = decoder;
                this.upgradeInitiator = upgradeInitiator;
                this.contentType = contentType;
                this.timeoutHelper = timeoutHelper;
                this.connection = connection;
                this.remoteAddress = remoteAddress;
                this.identityToImpersonate = identityToImpersonate;
                if (Begin())
                {
                    base.Complete(true);
                }
            }

            bool Begin()
            {
                string upgradeContentType = upgradeInitiator.GetNextUpgrade();

                while (upgradeContentType != null)
                {
                    EncodedUpgrade encodedUpgrade = new EncodedUpgrade(upgradeContentType);
                    AsyncCompletionResult writeFrameResult = connection.BeginWrite(
                        encodedUpgrade.EncodedBytes, 0, encodedUpgrade.EncodedBytes.Length, true, timeoutHelper.RemainingTime(),
                        onWriteUpgradeBytes, this);
                    if (writeFrameResult == AsyncCompletionResult.Queued)
                    {
                        return false;
                    }

                    if (!CompleteWriteUpgradeBytes())
                    {
                        return false;
                    }

                    upgradeContentType = upgradeInitiator.GetNextUpgrade();
                }

                return true;
            }

            bool CompleteWriteUpgradeBytes()
            {
                connection.EndWrite();

                if (connection.BeginRead(0, ServerSessionEncoder.UpgradeResponseBytes.Length, timeoutHelper.RemainingTime(),
                    onReadUpgradeResponse, this) == AsyncCompletionResult.Queued)
                {
                    return false;
                }

                return CompleteReadUpgradeResponse();
            }

            bool CompleteReadUpgradeResponse()
            {
                int bytesRead = connection.EndRead();

                if (!ConnectionUpgradeHelper.ValidateUpgradeResponse(connection.AsyncReadBuffer, bytesRead, decoder))
                {
                    if (onFailedUpgrade == null)
                    {
                        onFailedUpgrade = Fx.ThunkCallback(new AsyncCallback(OnFailedUpgrade));
                    }

                    IAsyncResult result = ConnectionUpgradeHelper.BeginDecodeFramingFault(decoder, connection,
                        remoteAddress.Uri, contentType, ref timeoutHelper, onFailedUpgrade, this);

                    if (result.CompletedSynchronously)
                    {
                        ConnectionUpgradeHelper.EndDecodeFramingFault(result);
                    }

                    return result.CompletedSynchronously;
                }

                this.connectionStream = new ConnectionStream(connection, this.defaultTimeouts);
                IAsyncResult initiateUpgradeResult = null;
                // ensure that any impersonated identity is available to the upgrade initiator
                WindowsImpersonationContext impersonationContext = (this.identityToImpersonate == null) ? null : this.identityToImpersonate.Impersonate();
                try
                {
                    using (impersonationContext)
                    {
                        initiateUpgradeResult = upgradeInitiator.BeginInitiateUpgrade(connectionStream, onInitiateUpgrade, this);
                    }
                }
                catch
                {
                    //Added to guarantee that the finally of the using statement above runs before exception filters higher in the stack
                    throw;
                }
                if (!initiateUpgradeResult.CompletedSynchronously)
                    return false;

                CompleteUpgrade(initiateUpgradeResult);
                return true;
            }

            void CompleteUpgrade(IAsyncResult result)
            {
                Stream stream = upgradeInitiator.EndInitiateUpgrade(result);
                this.connection = new StreamConnection(stream, connectionStream);
            }

            public static IConnection End(IAsyncResult result)
            {
                InitiateUpgradeAsyncResult thisPtr = AsyncResult.End<InitiateUpgradeAsyncResult>(result);
                return thisPtr.connection;
            }

            static void OnReadUpgradeResponse(object state)
            {
                InitiateUpgradeAsyncResult thisPtr = (InitiateUpgradeAsyncResult)state;

                Exception completionException = null;
                bool completeSelf = false;
                try
                {
                    if (thisPtr.CompleteReadUpgradeResponse())
                    {
                        completeSelf = thisPtr.Begin();
                    }
                }
#pragma warning suppress 56500 // [....], transferring exception to another thread
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    completeSelf = true;
                    completionException = e;
                }
                if (completeSelf)
                {
                    thisPtr.Complete(false, completionException);
                }
            }

            static void OnFailedUpgrade(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }

                InitiateUpgradeAsyncResult thisPtr = (InitiateUpgradeAsyncResult)result.AsyncState;

                Exception completionException = null;
                try
                {
                    ConnectionUpgradeHelper.EndDecodeFramingFault(result);
                }
#pragma warning suppress 56500 // [....], transferring exception to another thread
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    completionException = e;
                }
                thisPtr.Complete(false, completionException);
            }

            static void OnWriteUpgradeBytes(object asyncState)
            {
                InitiateUpgradeAsyncResult thisPtr = (InitiateUpgradeAsyncResult)asyncState;

                Exception completionException = null;
                bool completeSelf = false;
                try
                {
                    if (thisPtr.CompleteWriteUpgradeBytes())
                    {
                        completeSelf = thisPtr.Begin();
                    }
                }
#pragma warning suppress 56500 // [....], transferring exception to another thread
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    completeSelf = true;
                    completionException = e;
                }
                if (completeSelf)
                {
                    thisPtr.Complete(false, completionException);
                }
            }

            static void OnInitiateUpgrade(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                    return;

                InitiateUpgradeAsyncResult thisPtr = (InitiateUpgradeAsyncResult)result.AsyncState;

                Exception completionException = null;
                bool completeSelf;
                try
                {
                    thisPtr.CompleteUpgrade(result);
                    completeSelf = thisPtr.Begin();
                }
#pragma warning suppress 56500 // [....], transferring exception to another thread
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    completeSelf = true;
                    completionException = e;
                }
                if (completeSelf)
                {
                    thisPtr.Complete(false, completionException);
                }
            }
        }
    }
}
