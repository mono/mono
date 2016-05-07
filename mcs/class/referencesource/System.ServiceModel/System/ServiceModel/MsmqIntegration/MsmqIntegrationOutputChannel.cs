//------------------------------------------------------------  
// Copyright (c) Microsoft Corporation.  All rights reserved.   
//------------------------------------------------------------  

namespace System.ServiceModel.MsmqIntegration
{
    using System.IO;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Security.Tokens;

    sealed class MsmqIntegrationOutputChannel : TransportOutputChannel
    {
        MsmqQueue msmqQueue;
        MsmqTransactionMode transactionMode;
        MsmqIntegrationChannelFactory factory;
        SecurityTokenProviderContainer certificateTokenProvider;

        public MsmqIntegrationOutputChannel(MsmqIntegrationChannelFactory factory, EndpointAddress to, Uri via, bool manualAddressing)
            : base(factory, to, via, manualAddressing, factory.MessageVersion)
        {
            this.factory = factory;
            if (factory.IsMsmqX509SecurityConfigured)
            {
                this.certificateTokenProvider = factory.CreateX509TokenProvider(to, via);
            }
        }
        
        void CloseQueue()
        {
            if (null != this.msmqQueue)
                this.msmqQueue.Dispose();
            this.msmqQueue = null;
        }

        void OnCloseCore(bool isAborting, TimeSpan timeout)
        {
            this.CloseQueue();
            if (this.certificateTokenProvider != null)
            {
                if (isAborting)
                    this.certificateTokenProvider.Abort();
                else
                    this.certificateTokenProvider.Close(timeout);
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

        void OpenQueue()
        {
            try
            {
                this.msmqQueue = new MsmqQueue(this.factory.AddressTranslator.UriToFormatName(this.RemoteAddress.Uri), UnsafeNativeMethods.MQ_SEND_ACCESS);
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

        void OnOpenCore(TimeSpan timeout)
        {
            OpenQueue();
            if (this.certificateTokenProvider != null)
            {
                this.certificateTokenProvider.Open(timeout);
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
            MessageProperties properties = message.Properties;
            Stream stream = null;
            
            MsmqIntegrationMessageProperty property = MsmqIntegrationMessageProperty.Get(message);
            if (null == property)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(SR.GetString(SR.MsmqMessageDoesntHaveIntegrationProperty)));                
            if (null != property.Body)
                stream = this.factory.Serialize(property);

            int size;
            if (stream == null)
            {
                size = 0;
            }
            else
            {
                if (stream.Length > int.MaxValue)
                {
                    throw TraceUtility.ThrowHelperError(new ProtocolException(SR.GetString(SR.MessageSizeMustBeInIntegerRange)), message);
                }

                size = (int)stream.Length;
            }

            using (MsmqIntegrationOutputMessage msmqMessage = new MsmqIntegrationOutputMessage(this.factory, size, this.RemoteAddress, property))
            {
                msmqMessage.ApplyCertificateIfNeeded(this.certificateTokenProvider, this.factory.MsmqTransportSecurity.MsmqAuthenticationMode, timeout);

                if (stream != null)
                {
                    stream.Position = 0;
                    for (int bytesRemaining = size; bytesRemaining > 0; )
                    {
                        int bytesRead = stream.Read(msmqMessage.Body.Buffer, 0, bytesRemaining);
                        bytesRemaining -= bytesRead;
                    }
                }

                bool lockHeld = false;
                try
                {
                    Msmq.EnterXPSendLock(out lockHeld, this.factory.MsmqTransportSecurity.MsmqProtectionLevel);
                    this.msmqQueue.Send(msmqMessage, this.transactionMode);
                    MsmqDiagnostics.DatagramSent(msmqMessage.MessageId, message);
                    property.Id = MsmqMessageId.ToString(msmqMessage.MessageId.Buffer);
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
        }
        class MsmqIntegrationOutputMessage : MsmqOutputMessage<IOutputChannel>
        {
            ByteProperty acknowledge;
            StringProperty adminQueue;
            IntProperty appSpecific;
            BufferProperty correlationId;
            BufferProperty extension;
            StringProperty label;
            ByteProperty priority;
            StringProperty responseQueue;


            public MsmqIntegrationOutputMessage(
                MsmqChannelFactoryBase<IOutputChannel> factory, 
                int bodySize, 
                EndpointAddress remoteAddress, 
                MsmqIntegrationMessageProperty property)
                : base(factory, bodySize, remoteAddress, 8)
            {
                if (null == property)
                {
                    Fx.Assert("MsmqIntegrationMessageProperty expected");
                }

                if (property.AcknowledgeType.HasValue)
                    EnsureAcknowledgeProperty((byte)property.AcknowledgeType.Value);

                if (null != property.AdministrationQueue)
                    EnsureAdminQueueProperty(property.AdministrationQueue, false);

                if (property.AppSpecific.HasValue)
                    this.appSpecific = new IntProperty(this, UnsafeNativeMethods.PROPID_M_APPSPECIFIC, property.AppSpecific.Value);

                if (property.BodyType.HasValue)
                    EnsureBodyTypeProperty(property.BodyType.Value);

                if (null != property.CorrelationId)
                    this.correlationId = new BufferProperty(this, UnsafeNativeMethods.PROPID_M_CORRELATIONID, MsmqMessageId.FromString(property.CorrelationId));

                if (null != property.Extension)
                    this.extension = new BufferProperty(this, UnsafeNativeMethods.PROPID_M_EXTENSION, property.Extension);

                if (null != property.Label)
                    this.label = new StringProperty(this, UnsafeNativeMethods.PROPID_M_LABEL, property.Label);

                if (property.Priority.HasValue)
                    this.priority = new ByteProperty(this, UnsafeNativeMethods.PROPID_M_PRIORITY, (byte)property.Priority.Value);

                if (null != property.ResponseQueue)
                    EnsureResponseQueueProperty(property.ResponseQueue);

                if (property.TimeToReachQueue.HasValue)
                    EnsureTimeToReachQueueProperty(MsmqDuration.FromTimeSpan(property.TimeToReachQueue.Value));
            }

            void EnsureAcknowledgeProperty(byte value)
            {
                if (this.acknowledge == null)
                {
                    this.acknowledge = new ByteProperty(this, UnsafeNativeMethods.PROPID_M_ACKNOWLEDGE);
                }
                this.acknowledge.Value = value;
            }

            void EnsureAdminQueueProperty(Uri value, bool useNetMsmqTranslator)
            {
                if (null != value)
                {
                    string queueName = useNetMsmqTranslator ?
                        MsmqUri.NetMsmqAddressTranslator.UriToFormatName(value) : 
                        MsmqUri.FormatNameAddressTranslator.UriToFormatName(value);

                    if (this.adminQueue == null)
                    {
                        this.adminQueue = new StringProperty(this, UnsafeNativeMethods.PROPID_M_ADMIN_QUEUE, queueName);
                    }
                    else
                    {
                        this.adminQueue.SetValue(queueName);
                    }
                }
            }

            void EnsureResponseQueueProperty(Uri value)
            {
                if (null != value)
                {
                    string queueName = MsmqUri.FormatNameAddressTranslator.UriToFormatName(value);
                    if (this.responseQueue == null)
                    {
                        this.responseQueue = new StringProperty(this, UnsafeNativeMethods.PROPID_M_RESP_FORMAT_NAME, queueName);
                    }
                    else
                    {
                        this.responseQueue.SetValue(queueName);
                    }
                }
            }
        }
    }
}

    
