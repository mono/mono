//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ServiceModel;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Threading;

    abstract class TransportChannelFactory<TChannel> : ChannelFactoryBase<TChannel>, ITransportFactorySettings
    {
        BufferManager bufferManager;
        long maxBufferPoolSize;
        long maxReceivedMessageSize;
        MessageEncoderFactory messageEncoderFactory;
        bool manualAddressing;
        MessageVersion messageVersion;

        protected TransportChannelFactory(TransportBindingElement bindingElement, BindingContext context)
            : this(bindingElement, context, TransportDefaults.GetDefaultMessageEncoderFactory())
        {
        }

        protected TransportChannelFactory(TransportBindingElement bindingElement, BindingContext context,
                                          MessageEncoderFactory defaultMessageEncoderFactory)
            : base(context.Binding)
        {
            this.manualAddressing = bindingElement.ManualAddressing;
            this.maxBufferPoolSize = bindingElement.MaxBufferPoolSize;
            this.maxReceivedMessageSize = bindingElement.MaxReceivedMessageSize;

            Collection<MessageEncodingBindingElement> messageEncoderBindingElements
                = context.BindingParameters.FindAll<MessageEncodingBindingElement>();

            if (messageEncoderBindingElements.Count > 1)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.MultipleMebesInParameters)));
            }
            else if (messageEncoderBindingElements.Count == 1)
            {
                this.messageEncoderFactory = messageEncoderBindingElements[0].CreateMessageEncoderFactory();
                context.BindingParameters.Remove<MessageEncodingBindingElement>();
            }
            else
            {
                this.messageEncoderFactory = defaultMessageEncoderFactory;
            }

            if (null != this.messageEncoderFactory)
                this.messageVersion = this.messageEncoderFactory.MessageVersion;
            else
                this.messageVersion = MessageVersion.None;
        }

        public BufferManager BufferManager
        {
            get
            {
                return this.bufferManager;
            }
        }

        public long MaxBufferPoolSize
        {
            get
            {
                return this.maxBufferPoolSize;
            }
        }

        public long MaxReceivedMessageSize
        {
            get
            {
                return maxReceivedMessageSize;
            }
        }

        public MessageEncoderFactory MessageEncoderFactory
        {
            get
            {
                return this.messageEncoderFactory;
            }
        }

        public MessageVersion MessageVersion
        {
            get
            {
                return this.messageVersion;
            }
        }

        public bool ManualAddressing
        {
            get
            {
                return this.manualAddressing;
            }
        }

        public abstract string Scheme { get; }

        public override T GetProperty<T>()
        {
            if (typeof(T) == typeof(MessageVersion))
            {
                return (T)(object)this.MessageVersion;
            }

            if (typeof(T) == typeof(FaultConverter))
            {
                if (null == this.MessageEncoderFactory)
                    return null;
                else
                    return this.MessageEncoderFactory.Encoder.GetProperty<T>();
            }

            if (typeof(T) == typeof(ITransportFactorySettings))
            {
                return (T)(object)this;
            }

            return base.GetProperty<T>();
        }


        protected override void OnAbort()
        {
            OnCloseOrAbort();
            base.OnAbort();
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            OnCloseOrAbort();
            return base.OnBeginClose(timeout, callback, state);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            OnCloseOrAbort();
            base.OnClose(timeout);
        }

        void OnCloseOrAbort()
        {
            if (this.bufferManager != null)
            {
                this.bufferManager.Clear();
            }
        }

        internal virtual int GetMaxBufferSize()
        {
            if (MaxReceivedMessageSize > int.MaxValue)
                return int.MaxValue;
            else
                return (int)MaxReceivedMessageSize;
        }

        protected override void OnOpening()
        {
            base.OnOpening();
            this.bufferManager = BufferManager.CreateBufferManager(MaxBufferPoolSize, GetMaxBufferSize());
        }

        internal void ValidateScheme(Uri via)
        {
            if (via.Scheme != this.Scheme)
            {
                // URI schemes are case-insensitive, so try a case insensitive compare now
                if (string.Compare(via.Scheme, this.Scheme, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("via", SR.GetString(SR.InvalidUriScheme,
                        via.Scheme, this.Scheme));
                }
            }
        }

        long ITransportFactorySettings.MaxReceivedMessageSize
        {
            get { return MaxReceivedMessageSize; }
        }

        BufferManager ITransportFactorySettings.BufferManager
        {
            get { return BufferManager; }
        }

        bool ITransportFactorySettings.ManualAddressing
        {
            get { return ManualAddressing; }
        }

        MessageEncoderFactory ITransportFactorySettings.MessageEncoderFactory
        {
            get { return MessageEncoderFactory; }
        }
    }
}
