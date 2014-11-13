//------------------------------------------------------------  
// Copyright (c) Microsoft Corporation.  All rights reserved.   
//------------------------------------------------------------  

namespace System.ServiceModel.Channels
{
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Security.Tokens;

    sealed class MsmqOutputChannel : TransportOutputChannel
    {
        MsmqQueue msmqQueue;
        MsmqTransactionMode transactionMode;
        readonly byte[] preamble; // cached .NET framing singleton preamble
        SynchronizedDisposablePool<MsmqOutputMessage<IOutputChannel>> outputMessages;
        MsmqChannelFactory<IOutputChannel> factory;
        SecurityTokenProviderContainer certificateTokenProvider;

        public MsmqOutputChannel(MsmqChannelFactory<IOutputChannel> factory, EndpointAddress to, Uri via, bool manualAddressing)
            : base(factory, to, via, manualAddressing, factory.MessageVersion)
        {
            // construct the .NET framing preamble used for every message
            byte[] modeBytes = ClientSingletonSizedEncoder.ModeBytes;
            EncodedVia encodedVia = new EncodedVia(this.Via.AbsoluteUri);
            EncodedContentType encodedContentType = EncodedContentType.Create(factory.MessageEncoderFactory.Encoder.ContentType);

            this.preamble = DiagnosticUtility.Utility.AllocateByteArray(modeBytes.Length + ClientSingletonSizedEncoder.CalcStartSize(encodedVia, encodedContentType));

            Buffer.BlockCopy(modeBytes, 0, this.preamble, 0, modeBytes.Length);
            ClientSingletonSizedEncoder.EncodeStart(this.preamble, modeBytes.Length, encodedVia, encodedContentType);

            this.outputMessages = new SynchronizedDisposablePool<MsmqOutputMessage<IOutputChannel>>(factory.MaxPoolSize);
            if (factory.IsMsmqX509SecurityConfigured)
            {
                this.certificateTokenProvider = factory.CreateX509TokenProvider(to, via);
            }
            this.factory = factory;
        }

        void CloseQueue()
        {
            this.outputMessages.Dispose();
            if (null != this.msmqQueue)
                this.msmqQueue.Dispose();
            this.msmqQueue = null;
        }

        void OnCloseCore(bool isAborting, TimeSpan timeout)
        {
            this.CloseQueue();
            this.outputMessages.Dispose();
            if (factory.IsMsmqX509SecurityConfigured)
            {
                if (isAborting)
                    this.certificateTokenProvider.Abort();
                else
                    this.certificateTokenProvider.Close(timeout);
            }
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            OnCloseCore(false, timeout);
            return new CompletedAsyncResult(callback, state);
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            OnCloseCore(false, timeout);
        }

        protected override void OnAbort()
        {
            OnCloseCore(true, TimeSpan.Zero);
        }

        void OnOpenCore(TimeSpan timeout)
        {
            OpenQueue();
            if (factory.IsMsmqX509SecurityConfigured)
            {
                this.certificateTokenProvider.Open(timeout);
            }
        }

        void OpenQueue()
        {
            try
            {
                this.msmqQueue = new MsmqQueue(this.factory.AddressTranslator.UriToFormatName(this.RemoteAddress.Uri),
                                           UnsafeNativeMethods.MQ_SEND_ACCESS);
            }
            catch (MsmqException ex)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(ex.Normalized);
            }
            if (this.factory.ExactlyOnce)
            {
                this.transactionMode = MsmqTransactionMode.CurrentOrSingle;
            }
            else
            {
                this.transactionMode = MsmqTransactionMode.None;
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
            // serialize the indigo message to byte array and copy the .NET framing preamble
            ArraySegment<byte> messageData = this.factory.MessageEncoderFactory.Encoder.WriteMessage(
                message, int.MaxValue, this.factory.BufferManager, preamble.Length);
            Buffer.BlockCopy(preamble, 0, messageData.Array, messageData.Offset - preamble.Length, preamble.Length);

            byte[] buffer = messageData.Array;
            int offset = messageData.Offset - preamble.Length;
            int size = messageData.Count + preamble.Length;

            MsmqOutputMessage<IOutputChannel> msmqMessage = this.outputMessages.Take();
            if (msmqMessage == null)
            {
                msmqMessage = new MsmqOutputMessage<IOutputChannel>(this.factory, size, this.RemoteAddress);
                MsmqDiagnostics.PoolFull(this.factory.MaxPoolSize);
            }
            try
            {
                msmqMessage.ApplyCertificateIfNeeded(this.certificateTokenProvider, this.factory.MsmqTransportSecurity.MsmqAuthenticationMode, timeout);
                msmqMessage.Body.EnsureBufferLength(size);
                msmqMessage.Body.BufferLength = size;
                Buffer.BlockCopy(buffer, offset, msmqMessage.Body.Buffer, 0, size);
                this.factory.BufferManager.ReturnBuffer(buffer);

                bool lockHeld = false;
                try
                {
                    Msmq.EnterXPSendLock(out lockHeld, this.factory.MsmqTransportSecurity.MsmqProtectionLevel);
                    this.msmqQueue.Send(msmqMessage, this.transactionMode);
                    MsmqDiagnostics.DatagramSent(msmqMessage.MessageId, message);
                }
                catch (MsmqException ex)
                {
                    if (ex.FaultSender)
                        this.Fault();
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(ex.Normalized);
                }
                finally
                {
                    if (lockHeld)
                    {
                        Msmq.LeaveXPSendLock();
                    }
                }
            }
            finally
            {
                if (!this.outputMessages.Return(msmqMessage))
                {
                    msmqMessage.Dispose();
                }
            }
        }
    }
}
