//------------------------------------------------------------  
// Copyright (c) Microsoft Corporation.  All rights reserved.   
//------------------------------------------------------------  

namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Security.Tokens;
    using System.Transactions;
    using SR = System.ServiceModel.SR;

    sealed class MsmqOutputSessionChannel : TransportOutputChannel, IOutputSessionChannel
    {
        MsmqQueue msmqQueue;
        List<ArraySegment<byte>> buffers;
        Transaction associatedTx;
        IOutputSession session;
        MsmqChannelFactory<IOutputSessionChannel> factory;
        MessageEncoder encoder;
        SecurityTokenProviderContainer certificateTokenProvider;

        public MsmqOutputSessionChannel(MsmqChannelFactory<IOutputSessionChannel> factory, EndpointAddress to, Uri via, bool manualAddressing)
            : base(factory, to, via, manualAddressing, factory.MessageVersion)
        {
            this.factory = factory;
            this.encoder = this.factory.MessageEncoderFactory.CreateSessionEncoder();
            this.buffers = new List<ArraySegment<byte>>();
            this.buffers.Add(EncodeSessionPreamble());
            if (factory.IsMsmqX509SecurityConfigured)
            {
                this.certificateTokenProvider = factory.CreateX509TokenProvider(to, via);
            }
            this.session = new OutputSession();
        }

        int CalcSessionGramSize()
        {
            long sessionGramSize = 0;
            for (int i = 0; i < this.buffers.Count; i++)
            {
                ArraySegment<byte> buffer = this.buffers[i];
                sessionGramSize += buffer.Count;
            }

            if (sessionGramSize > int.MaxValue)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new InvalidOperationException(SR.GetString(SR.MsmqSessionGramSizeMustBeInIntegerRange)));

            return (int)sessionGramSize;
        }

        void CopySessionGramToBuffer(byte[] sessionGramBuffer)
        {
            int sessionGramOffset = 0;
            for (int i = 0; i < this.buffers.Count; i++)
            {
                ArraySegment<byte> buffer = this.buffers[i];
                Buffer.BlockCopy(buffer.Array, buffer.Offset, sessionGramBuffer, sessionGramOffset, buffer.Count);
                sessionGramOffset += buffer.Count;
            }
        }

        void ReturnSessionGramBuffers()
        {
            // Don't return that fancy/schmancy end buffer
            for (int i = 0; i < this.buffers.Count - 1; i++)
            {
                this.Factory.BufferManager.ReturnBuffer(this.buffers[i].Array);
            }
        }

        public IOutputSession Session
        {
            get { return this.session; }
        }

        void OnCloseCore(bool isAborting, TimeSpan timeout)
        {
            // Dump the messages into the queue as a big bag.
            // no MSMQ send if aborting 
            // no MSMQ send if the channel has only a preamble (no actual messages sent)
            if (!isAborting && this.buffers.Count > 1)
            {
                lock (ThisLock)
                {
                    VerifyTransaction();

                    buffers.Add(EncodeEndMarker());
                }

                int size = CalcSessionGramSize();

                using (MsmqOutputMessage<IOutputSessionChannel> msmqMessage = new MsmqOutputMessage<IOutputSessionChannel>(this.Factory, size, this.RemoteAddress))
                {
                    msmqMessage.ApplyCertificateIfNeeded(this.certificateTokenProvider, this.factory.MsmqTransportSecurity.MsmqAuthenticationMode, timeout);
                    msmqMessage.Body.EnsureBufferLength(size);
                    msmqMessage.Body.BufferLength = size;
                    CopySessionGramToBuffer(msmqMessage.Body.Buffer);

                    bool lockHeld = false;
                    try
                    {
                        Msmq.EnterXPSendLock(out lockHeld, this.factory.MsmqTransportSecurity.MsmqProtectionLevel);
                        this.msmqQueue.Send(msmqMessage, MsmqTransactionMode.CurrentOrSingle);
                        MsmqDiagnostics.SessiongramSent(this.Session.Id, msmqMessage.MessageId, this.buffers.Count);
                    }
                    catch (MsmqException ex)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(ex.Normalized);
                    }
                    finally
                    {
                        if (lockHeld)
                        {
                            Msmq.LeaveXPSendLock();
                        }
                        ReturnSessionGramBuffers();
                    }
                }
            }

            if (null != this.msmqQueue)
                this.msmqQueue.Dispose();
            this.msmqQueue = null;

            if (certificateTokenProvider != null)
            {
                if (isAborting)
                    certificateTokenProvider.Abort();
                else
                    certificateTokenProvider.Close(timeout);
            }
        }

        protected override void OnAbort()
        {
            this.OnCloseCore(true, TimeSpan.Zero);
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.OnCloseCore(false, timeout);
            return new CompletedAsyncResult(callback, state);
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            this.OnCloseCore(false, timeout);
        }

        void OnOpenCore(TimeSpan timeout)
        {
            if (null == Transaction.Current)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new InvalidOperationException(SR.GetString(SR.MsmqTransactionCurrentRequired)));
            this.associatedTx = Transaction.Current;
            this.associatedTx.EnlistVolatile(new TransactionEnlistment(this, this.associatedTx), EnlistmentOptions.None);
            this.msmqQueue = new MsmqQueue(this.Factory.AddressTranslator.UriToFormatName(this.RemoteAddress.Uri),
                                           UnsafeNativeMethods.MQ_SEND_ACCESS);
            if (certificateTokenProvider != null)
            {
                certificateTokenProvider.Open(timeout);
            }
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            OnOpenCore(timeout);
            return new CompletedAsyncResult(callback, state);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            OnOpenCore(timeout);
        }

        protected override IAsyncResult OnBeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            OnSend(message, timeout);
            return new CompletedAsyncResult(callback, state);
        }

        protected override void OnEndSend(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }

        protected override void OnSend(Message message, TimeSpan timeout)
        {
            lock (ThisLock)
            {
                ThrowIfDisposed();
                VerifyTransaction();
                // serialize the indigo message to byte array and save...
                this.buffers.Add(EncodeMessage(message));
            }
        }

        void VerifyTransaction()
        {
            if (this.associatedTx != Transaction.Current)
            {
                this.Fault();
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new InvalidOperationException(SR.GetString(SR.MsmqSameTransactionExpected)));
            }

            if (TransactionStatus.Active != Transaction.Current.TransactionInformation.Status)
            {
                this.Fault();
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new InvalidOperationException(SR.GetString(SR.MsmqTransactionNotActive)));
            }
        }

        ArraySegment<byte> EncodeSessionPreamble()
        {
            EncodedVia encodedVia = new EncodedVia(this.Via.AbsoluteUri);
            EncodedContentType encodedContentType = EncodedContentType.Create(this.encoder.ContentType);

            int startSize = ClientSimplexEncoder.ModeBytes.Length
                + SessionEncoder.CalcStartSize(encodedVia, encodedContentType)
                + ClientSimplexEncoder.PreambleEndBytes.Length;
            byte[] startBytes = this.Factory.BufferManager.TakeBuffer(startSize);
            Buffer.BlockCopy(ClientSimplexEncoder.ModeBytes, 0, startBytes, 0, ClientSimplexEncoder.ModeBytes.Length);
            SessionEncoder.EncodeStart(startBytes, ClientSimplexEncoder.ModeBytes.Length, encodedVia, encodedContentType);
            Buffer.BlockCopy(ClientSimplexEncoder.PreambleEndBytes, 0, startBytes, startSize - ClientSimplexEncoder.PreambleEndBytes.Length, ClientSimplexEncoder.PreambleEndBytes.Length);

            return new ArraySegment<byte>(startBytes, 0, startSize);
        }

        ArraySegment<byte> EncodeEndMarker()
        {
            return new ArraySegment<byte>(SessionEncoder.EndBytes, 0, SessionEncoder.EndBytes.Length);
        }

        // Stick a message into a buffer
        ArraySegment<byte> EncodeMessage(Message message)
        {
            ArraySegment<byte> messageData = this.encoder.WriteMessage(message,
                                                                       int.MaxValue,
                                                                       this.Factory.BufferManager,
                                                                       SessionEncoder.MaxMessageFrameSize);

            return SessionEncoder.EncodeMessageFrame(messageData);
        }

        MsmqChannelFactory<IOutputSessionChannel> Factory
        {
            get { return this.factory; }
        }

        class OutputSession : IOutputSession
        {
            string id = "uuid:/session-gram/" + Guid.NewGuid();

            public string Id
            {
                get { return this.id; }
            }
        }

        class TransactionEnlistment : IEnlistmentNotification
        {
            MsmqOutputSessionChannel channel;
            Transaction transaction;

            public TransactionEnlistment(MsmqOutputSessionChannel channel, Transaction transaction)
            {
                this.channel = channel;
                this.transaction = transaction;
            }

            public void Prepare(PreparingEnlistment preparingEnlistment)
            {
                // Abort if this happens before the channel is closed...
                if (this.channel.State != CommunicationState.Closed)
                {
                    channel.Fault();
                    Exception e = DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.MsmqSessionChannelsMustBeClosed)));
                    preparingEnlistment.ForceRollback(e);
                }
                else
                    preparingEnlistment.Prepared();
            }

            public void Commit(Enlistment enlistment)
            {
                enlistment.Done();
            }

            public void Rollback(Enlistment enlistment)
            {
                this.channel.Fault();
                enlistment.Done();
            }

            public void InDoubt(Enlistment enlistment)
            {
                enlistment.Done();
            }
        }
    }
}
