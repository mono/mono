//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Runtime;
    using System.Security.Authentication.ExtendedProtection;
    using System.ServiceModel;
    using System.ServiceModel.Security;
    using System.Threading;

    class StreamedFramingRequestChannel : RequestChannel
    {
        IConnectionInitiator connectionInitiator;
        ConnectionPool connectionPool;
        MessageEncoder messageEncoder;
        IConnectionOrientedTransportFactorySettings settings;
        byte[] startBytes;
        StreamUpgradeProvider upgrade;
        ChannelBinding channelBindingToken;

        public StreamedFramingRequestChannel(ChannelManagerBase factory, IConnectionOrientedTransportChannelFactorySettings settings,
            EndpointAddress remoteAddresss, Uri via, IConnectionInitiator connectionInitiator, ConnectionPool connectionPool)
            : base(factory, remoteAddresss, via, settings.ManualAddressing)
        {
            this.settings = settings;
            this.connectionInitiator = connectionInitiator;
            this.connectionPool = connectionPool;

            this.messageEncoder = settings.MessageEncoderFactory.Encoder;
            this.upgrade = settings.Upgrade;
        }

        byte[] Preamble
        {
            get { return this.startBytes; }
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CompletedAsyncResult(callback, state);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }

        protected override void OnOpen(TimeSpan timeout)
        {
        }

        protected override void OnOpened()
        {
            // setup our preamble which we'll use for all connections we establish
            EncodedVia encodedVia = new EncodedVia(this.Via.AbsoluteUri);
            EncodedContentType encodedContentType = EncodedContentType.Create(settings.MessageEncoderFactory.Encoder.ContentType);
            int startSize = ClientSingletonEncoder.ModeBytes.Length + ClientSingletonEncoder.CalcStartSize(encodedVia, encodedContentType);
            int preambleEndOffset = 0;
            if (this.upgrade == null)
            {
                preambleEndOffset = startSize;
                startSize += ClientDuplexEncoder.PreambleEndBytes.Length;
            }
            this.startBytes = DiagnosticUtility.Utility.AllocateByteArray(startSize);
            Buffer.BlockCopy(ClientSingletonEncoder.ModeBytes, 0, startBytes, 0, ClientSingletonEncoder.ModeBytes.Length);
            ClientSingletonEncoder.EncodeStart(this.startBytes, ClientSingletonEncoder.ModeBytes.Length, encodedVia, encodedContentType);
            if (preambleEndOffset > 0)
            {
                Buffer.BlockCopy(ClientSingletonEncoder.PreambleEndBytes, 0, startBytes, preambleEndOffset, ClientSingletonEncoder.PreambleEndBytes.Length);
            }

            // and then transition to the Opened state
            base.OnOpened();
        }

        protected override IAsyncRequest CreateAsyncRequest(Message message, AsyncCallback callback, object state)
        {
            return new StreamedFramingAsyncRequest(this, callback, state);
        }

        protected override IRequest CreateRequest(Message message)
        {
            return new StreamedFramingRequest(this);
        }

        IConnection SendPreamble(IConnection connection, ref TimeoutHelper timeoutHelper,
            ClientFramingDecoder decoder, out SecurityMessageProperty remoteSecurity)
        {
            connection.Write(Preamble, 0, Preamble.Length, true, timeoutHelper.RemainingTime());

            if (upgrade != null)
            {
                IStreamUpgradeChannelBindingProvider channelBindingProvider = upgrade.GetProperty<IStreamUpgradeChannelBindingProvider>();

                StreamUpgradeInitiator upgradeInitiator = upgrade.CreateUpgradeInitiator(this.RemoteAddress, this.Via);

                if (!ConnectionUpgradeHelper.InitiateUpgrade(upgradeInitiator, ref connection, decoder,
                    this, ref timeoutHelper))
                {
                    ConnectionUpgradeHelper.DecodeFramingFault(decoder, connection, Via, messageEncoder.ContentType, ref timeoutHelper);
                }

                if (channelBindingProvider != null && channelBindingProvider.IsChannelBindingSupportEnabled)
                {
                    this.channelBindingToken = channelBindingProvider.GetChannelBinding(upgradeInitiator, ChannelBindingKind.Endpoint);
                }

                remoteSecurity = StreamSecurityUpgradeInitiator.GetRemoteSecurity(upgradeInitiator);

                connection.Write(ClientSingletonEncoder.PreambleEndBytes, 0,
                    ClientSingletonEncoder.PreambleEndBytes.Length, true, timeoutHelper.RemainingTime());
            }
            else
            {
                remoteSecurity = null;
            }

            // read ACK
            byte[] ackBuffer = new byte[1];
            int ackBytesRead = connection.Read(ackBuffer, 0, ackBuffer.Length, timeoutHelper.RemainingTime());
            if (!ConnectionUpgradeHelper.ValidatePreambleResponse(ackBuffer, ackBytesRead, decoder, this.Via))
            {
                ConnectionUpgradeHelper.DecodeFramingFault(decoder, connection, Via, messageEncoder.ContentType, ref timeoutHelper);
            }

            return connection;
        }

        protected override void OnClose(TimeSpan timeout)
        {
            base.WaitForPendingRequests(timeout);
        }

        protected override void OnClosed()
        {
            base.OnClosed();

            // clean up the CBT after transitioning to the closed state
            ChannelBindingUtility.Dispose(ref this.channelBindingToken);
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return base.BeginWaitForPendingRequests(timeout, callback, state);
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            base.EndWaitForPendingRequests(result);
        }

        internal class StreamedConnectionPoolHelper : ConnectionPoolHelper
        {
            StreamedFramingRequestChannel channel;
            ClientSingletonDecoder decoder;
            SecurityMessageProperty remoteSecurity;

            public StreamedConnectionPoolHelper(StreamedFramingRequestChannel channel)
                : base(channel.connectionPool, channel.connectionInitiator, channel.Via)
            {
                this.channel = channel;
            }

            public ClientSingletonDecoder Decoder
            {
                get { return this.decoder; }
            }

            public SecurityMessageProperty RemoteSecurity
            {
                get { return this.remoteSecurity; }
            }

            protected override TimeoutException CreateNewConnectionTimeoutException(TimeSpan timeout, TimeoutException innerException)
            {
                return new TimeoutException(SR.GetString(SR.RequestTimedOutEstablishingTransportSession,
                        timeout, channel.Via.AbsoluteUri), innerException);
            }

            protected override IConnection AcceptPooledConnection(IConnection connection, ref TimeoutHelper timeoutHelper)
            {
                this.decoder = new ClientSingletonDecoder(0);
                return channel.SendPreamble(connection, ref timeoutHelper, this.decoder, out this.remoteSecurity);
            }

            protected override IAsyncResult BeginAcceptPooledConnection(IConnection connection, ref TimeoutHelper timeoutHelper, AsyncCallback callback, object state)
            {
                this.decoder = new ClientSingletonDecoder(0);
                return new SendPreambleAsyncResult(channel, connection, ref timeoutHelper, decoder, callback, state);
            }

            protected override IConnection EndAcceptPooledConnection(IAsyncResult result)
            {
                return SendPreambleAsyncResult.End(result, out this.remoteSecurity);
            }

            class SendPreambleAsyncResult : AsyncResult
            {
                StreamedFramingRequestChannel channel;
                IConnection connection;
                ClientFramingDecoder decoder;
                StreamUpgradeInitiator upgradeInitiator;
                SecurityMessageProperty remoteSecurity;
                TimeoutHelper timeoutHelper;
                static WaitCallback onWritePreamble = Fx.ThunkCallback(new WaitCallback(OnWritePreamble));
                static WaitCallback onWritePreambleEnd;
                static WaitCallback onReadPreambleAck = new WaitCallback(OnReadPreambleAck);
                static AsyncCallback onUpgrade;
                static AsyncCallback onFailedUpgrade;
                IStreamUpgradeChannelBindingProvider channelBindingProvider;

                public SendPreambleAsyncResult(StreamedFramingRequestChannel channel, IConnection connection,
                    ref TimeoutHelper timeoutHelper, ClientFramingDecoder decoder, AsyncCallback callback, object state)
                    : base(callback, state)
                {
                    this.channel = channel;
                    this.connection = connection;
                    this.timeoutHelper = timeoutHelper;
                    this.decoder = decoder;

                    AsyncCompletionResult writePreambleResult = connection.BeginWrite(channel.Preamble, 0, channel.Preamble.Length,
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

                public static IConnection End(IAsyncResult result, out SecurityMessageProperty remoteSecurity)
                {
                    SendPreambleAsyncResult thisPtr = AsyncResult.End<SendPreambleAsyncResult>(result);
                    remoteSecurity = thisPtr.remoteSecurity;
                    return thisPtr.connection;
                }

                bool HandleWritePreamble()
                {
                    connection.EndWrite();

                    if (channel.upgrade == null)
                    {
                        return ReadPreambleAck();
                    }
                    else
                    {
                        this.channelBindingProvider = channel.upgrade.GetProperty<IStreamUpgradeChannelBindingProvider>();
                        this.upgradeInitiator = channel.upgrade.CreateUpgradeInitiator(channel.RemoteAddress, channel.Via);
                        if (onUpgrade == null)
                        {
                            onUpgrade = Fx.ThunkCallback(new AsyncCallback(OnUpgrade));
                        }

                        IAsyncResult initiateUpgradeResult = ConnectionUpgradeHelper.BeginInitiateUpgrade(channel.settings, channel.RemoteAddress,
                            connection, decoder, this.upgradeInitiator, channel.messageEncoder.ContentType, null,
                            this.timeoutHelper, onUpgrade, this);

                        if (!initiateUpgradeResult.CompletedSynchronously)
                        {
                            return false;
                        }
                        return HandleUpgrade(initiateUpgradeResult);
                    }
                }

                bool HandleUpgrade(IAsyncResult result)
                {
                    connection = ConnectionUpgradeHelper.EndInitiateUpgrade(result);

                    if (this.channelBindingProvider != null && this.channelBindingProvider.IsChannelBindingSupportEnabled)
                    {
                        this.channel.channelBindingToken = this.channelBindingProvider.GetChannelBinding(this.upgradeInitiator, ChannelBindingKind.Endpoint);
                    }

                    this.remoteSecurity = StreamSecurityUpgradeInitiator.GetRemoteSecurity(this.upgradeInitiator);
                    this.upgradeInitiator = null; // we're done with the initiator
                    if (onWritePreambleEnd == null)
                    {
                        onWritePreambleEnd = Fx.ThunkCallback(new WaitCallback(OnWritePreambleEnd));
                    }

                    AsyncCompletionResult writePreambleResult = connection.BeginWrite(
                        ClientSingletonEncoder.PreambleEndBytes, 0, ClientSingletonEncoder.PreambleEndBytes.Length, true,
                        timeoutHelper.RemainingTime(), onWritePreambleEnd, this);

                    if (writePreambleResult == AsyncCompletionResult.Queued)
                    {
                        return false;
                    }

                    connection.EndWrite();
                    return ReadPreambleAck();
                }

                bool ReadPreambleAck()
                {
                    AsyncCompletionResult readAckResult = connection.BeginRead(0, 1,
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
                    if (!ConnectionUpgradeHelper.ValidatePreambleResponse(
                        connection.AsyncReadBuffer, ackBytesRead, decoder, channel.Via))
                    {
                        if (onFailedUpgrade == null)
                        {
                            onFailedUpgrade = Fx.ThunkCallback(new AsyncCallback(OnFailedUpgrade));
                        }
                        IAsyncResult decodeFaultResult = ConnectionUpgradeHelper.BeginDecodeFramingFault(decoder,
                            connection, channel.Via, channel.messageEncoder.ContentType, ref timeoutHelper,
                            onFailedUpgrade, this);

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
                    bool completeSelf;
                    try
                    {
                        completeSelf = thisPtr.HandleWritePreamble();
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

                    if (completeSelf)
                    {
                        thisPtr.Complete(false, completionException);
                    }
                }

                static void OnWritePreambleEnd(object asyncState)
                {
                    SendPreambleAsyncResult thisPtr = (SendPreambleAsyncResult)asyncState;

                    Exception completionException = null;
                    bool completeSelf;
                    try
                    {
                        thisPtr.connection.EndWrite();
                        completeSelf = thisPtr.ReadPreambleAck();
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

                    Exception completionException = null;
                    bool completeSelf;
                    try
                    {
                        completeSelf = thisPtr.HandleUpgrade(result);
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

                    SendPreambleAsyncResult thisPtr = (SendPreambleAsyncResult)result.AsyncState;

                    Exception completionException = null;
                    try
                    {
                        ConnectionUpgradeHelper.EndDecodeFramingFault(result);
                    }
#pragma warning suppress 56500 // Microsoft, transferring exception to another thread
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
            }
        }

        class ClientSingletonConnectionReader : SingletonConnectionReader
        {
            StreamedConnectionPoolHelper connectionPoolHelper;

            public ClientSingletonConnectionReader(IConnection connection, StreamedConnectionPoolHelper connectionPoolHelper,
                IConnectionOrientedTransportFactorySettings settings)
                : base(connection, 0, 0, connectionPoolHelper.RemoteSecurity, settings, null)
            {
                this.connectionPoolHelper = connectionPoolHelper;
            }

            protected override long StreamPosition
            {
                get { return connectionPoolHelper.Decoder.StreamPosition; }
            }

            protected override bool DecodeBytes(byte[] buffer, ref int offset, ref int size, ref bool isAtEof)
            {
                while (size > 0)
                {
                    int bytesRead = connectionPoolHelper.Decoder.Decode(buffer, offset, size);
                    if (bytesRead > 0)
                    {
                        offset += bytesRead;
                        size -= bytesRead;
                    }

                    switch (connectionPoolHelper.Decoder.CurrentState)
                    {
                        case ClientFramingDecoderState.EnvelopeStart:
                            // we're at the envelope
                            return true;

                        case ClientFramingDecoderState.End:
                            isAtEof = true;
                            return false;
                    }
                }

                return false;
            }

            protected override void OnClose(TimeSpan timeout)
            {
                connectionPoolHelper.Close(timeout);
            }
        }

        class StreamedFramingRequest : IRequest
        {
            StreamedFramingRequestChannel channel;
            StreamedConnectionPoolHelper connectionPoolHelper;
            IConnection connection;

            public StreamedFramingRequest(StreamedFramingRequestChannel channel)
            {
                this.channel = channel;
                this.connectionPoolHelper = new StreamedConnectionPoolHelper(channel);
            }

            public void SendRequest(Message message, TimeSpan timeout)
            {
                TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);

                try
                {
                    this.connection = connectionPoolHelper.EstablishConnection(timeoutHelper.RemainingTime());

                    ChannelBindingUtility.TryAddToMessage(this.channel.channelBindingToken, message, false);

                    bool success = false;
                    try
                    {
                        StreamingConnectionHelper.WriteMessage(message, this.connection, true, channel.settings, ref timeoutHelper);
                        success = true;
                    }
                    finally
                    {
                        if (!success)
                        {
                            connectionPoolHelper.Abort();
                        }
                    }
                }
                catch (TimeoutException exception)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new TimeoutException(SR.GetString(SR.TimeoutOnRequest, timeout), exception));
                }
            }

            public Message WaitForReply(TimeSpan timeout)
            {
                ClientSingletonConnectionReader connectionReader = new ClientSingletonConnectionReader(
                    connection, connectionPoolHelper, channel.settings);

                connectionReader.DoneSending(TimeSpan.Zero); // we still need to receive
                Message message = connectionReader.Receive(timeout);

                if (message != null)
                {
                    ChannelBindingUtility.TryAddToMessage(this.channel.channelBindingToken, message, false);
                }

                return message;
            }

            void Cleanup()
            {
                this.connectionPoolHelper.Abort();
            }

            public void Abort(RequestChannel requestChannel)
            {
                Cleanup();
            }

            public void Fault(RequestChannel requestChannel)
            {
                Cleanup();
            }

            public void OnReleaseRequest()
            {                
            }
        }

        class StreamedFramingAsyncRequest : AsyncResult, IAsyncRequest
        {
            StreamedFramingRequestChannel channel;
            IConnection connection;
            StreamedConnectionPoolHelper connectionPoolHelper;
            Message message;
            Message replyMessage;
            TimeoutHelper timeoutHelper;
            static AsyncCallback onEstablishConnection = Fx.ThunkCallback(new AsyncCallback(OnEstablishConnection));
            static AsyncCallback onWriteMessage = Fx.ThunkCallback(new AsyncCallback(OnWriteMessage));
            static AsyncCallback onReceiveReply = Fx.ThunkCallback(new AsyncCallback(OnReceiveReply));
            ClientSingletonConnectionReader connectionReader;

            public StreamedFramingAsyncRequest(StreamedFramingRequestChannel channel, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.channel = channel;
                this.connectionPoolHelper = new StreamedConnectionPoolHelper(channel);
            }

            public void BeginSendRequest(Message message, TimeSpan timeout)
            {
                this.timeoutHelper = new TimeoutHelper(timeout);
                this.message = message;

                bool completeSelf = false;
                bool success = false;
                try
                {
                    try
                    {
                        IAsyncResult result = connectionPoolHelper.BeginEstablishConnection(timeoutHelper.RemainingTime(), onEstablishConnection, this);
                        if (result.CompletedSynchronously)
                        {
                            completeSelf = HandleEstablishConnection(result);
                        }
                    }
                    catch (TimeoutException exception)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                            new TimeoutException(SR.GetString(SR.TimeoutOnRequest, timeout), exception));
                    }

                    success = true;
                }
                finally
                {
                    if (!success)
                    {
                        Cleanup();
                    }
                }

                if (completeSelf)
                {
                    base.Complete(true);
                }
            }

            bool HandleEstablishConnection(IAsyncResult result)
            {
                this.connection = connectionPoolHelper.EndEstablishConnection(result);

                ChannelBindingUtility.TryAddToMessage(this.channel.channelBindingToken, this.message, false);

                IAsyncResult writeResult = StreamingConnectionHelper.BeginWriteMessage(this.message, this.connection, true, this.channel.settings, ref timeoutHelper, onWriteMessage, this);
                if (!writeResult.CompletedSynchronously)
                {
                    return false;
                }

                return HandleWriteMessage(writeResult);
            }

            public Message End()
            {
                try
                {
                    AsyncResult.End<StreamedFramingAsyncRequest>(this);
                }
                catch (TimeoutException exception)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new TimeoutException(SR.GetString(SR.TimeoutOnRequest, this.timeoutHelper.OriginalTimeout), exception));
                }
                return replyMessage;
            }

            public void Abort(RequestChannel requestChannel)
            {
                Cleanup();
            }

            public void Fault(RequestChannel requestChannel)
            {
                Cleanup();
            }

            void Cleanup()
            {
                connectionPoolHelper.Abort();
            }

            bool HandleWriteMessage(IAsyncResult result)
            {
                // write out the streamed message
                StreamingConnectionHelper.EndWriteMessage(result);

                connectionReader = new ClientSingletonConnectionReader(connection, connectionPoolHelper, channel.settings);
                connectionReader.DoneSending(TimeSpan.Zero); // we still need to receive

                IAsyncResult receiveResult = connectionReader.BeginReceive(timeoutHelper.RemainingTime(), onReceiveReply, this);

                if (!receiveResult.CompletedSynchronously)
                {
                    return false;
                }

                return CompleteReceiveReply(receiveResult);
            }

            bool CompleteReceiveReply(IAsyncResult result)
            {
                this.replyMessage = connectionReader.EndReceive(result);

                if (this.replyMessage != null)
                {
                    ChannelBindingUtility.TryAddToMessage(this.channel.channelBindingToken, this.replyMessage, false);
                }

                return true;
            }

            static void OnEstablishConnection(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }

                StreamedFramingAsyncRequest thisPtr = (StreamedFramingAsyncRequest)result.AsyncState;

                Exception completionException = null;
                bool completeSelf;
                bool throwing = true;
                try
                {
                    completeSelf = thisPtr.HandleEstablishConnection(result);
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

            static void OnWriteMessage(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }

                StreamedFramingAsyncRequest thisPtr = (StreamedFramingAsyncRequest)result.AsyncState;

                Exception completionException = null;
                bool completeSelf;
                bool throwing = true;
                try
                {
                    completeSelf = thisPtr.HandleWriteMessage(result);
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

            static void OnReceiveReply(IAsyncResult result)
            {
                StreamedFramingAsyncRequest thisPtr = (StreamedFramingAsyncRequest)result.AsyncState;

                Exception completionException = null;
                bool completeSelf;
                bool throwing = true;
                try
                {
                    completeSelf = thisPtr.CompleteReceiveReply(result);
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

            public void OnReleaseRequest()
            {                
            }
        }
    }
}
