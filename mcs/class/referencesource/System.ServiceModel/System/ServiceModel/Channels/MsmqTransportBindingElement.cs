//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.ServiceModel.Activation;
    using System.Collections.Generic;
    using System.Net.Security;
    using System.Runtime.Serialization;
    using System.ServiceModel.Security;

    using System.Xml;

    public sealed class MsmqTransportBindingElement : MsmqBindingElementBase
    {
        int maxPoolSize = MsmqDefaults.MaxPoolSize;
        bool useActiveDirectory = MsmqDefaults.UseActiveDirectory;
        QueueTransferProtocol queueTransferProtocol = MsmqDefaults.QueueTransferProtocol;

        public MsmqTransportBindingElement() { }

        MsmqTransportBindingElement(MsmqTransportBindingElement elementToBeCloned)
            : base(elementToBeCloned)
        {
            this.useActiveDirectory = elementToBeCloned.useActiveDirectory;
            this.maxPoolSize = elementToBeCloned.maxPoolSize;
            this.queueTransferProtocol = elementToBeCloned.queueTransferProtocol;
        }

        internal override MsmqUri.IAddressTranslator AddressTranslator
        {
            get
            {
                switch (this.queueTransferProtocol)
                {
                    case QueueTransferProtocol.Srmp:
                        return MsmqUri.SrmpAddressTranslator;
                    case QueueTransferProtocol.SrmpSecure:
                        return MsmqUri.SrmpsAddressTranslator;
                    default:
                        return this.useActiveDirectory ? MsmqUri.ActiveDirectoryAddressTranslator : MsmqUri.NetMsmqAddressTranslator;
                }
            }
        }

        public int MaxPoolSize
        {
            get
            {
                return this.maxPoolSize;
            }
            set
            {
                if (value < 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new ArgumentOutOfRangeException("value", value, SR.GetString(SR.MsmqNonNegativeArgumentExpected)));
                }
                this.maxPoolSize = value;
            }
        }

        public QueueTransferProtocol QueueTransferProtocol
        {
            get
            {
                return this.queueTransferProtocol;
            }
            set
            {
                if (!QueueTransferProtocolHelper.IsDefined(value))
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                this.queueTransferProtocol = value;
            }
        }

        public override string Scheme
        {
            get
            {
                return "net.msmq";
            }
        }

        public bool UseActiveDirectory
        {
            get
            {
                return this.useActiveDirectory;
            }
            set
            {
                this.useActiveDirectory = value;
            }
        }

        internal override string WsdlTransportUri
        {
            get
            {
                return TransportPolicyConstants.MsmqTransportUri;
            }
        }

        public override BindingElement Clone()
        {
            return new MsmqTransportBindingElement(this);
        }

        public override bool CanBuildChannelFactory<TChannel>(BindingContext context)
        {
            return (typeof(TChannel) == typeof(IOutputChannel)
                || typeof(TChannel) == typeof(IOutputSessionChannel));
        }

        public override bool CanBuildChannelListener<TChannel>(BindingContext context)
        {
            return (typeof(TChannel) == typeof(IInputChannel)
                || typeof(TChannel) == typeof(IInputSessionChannel));
        }

        public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

            if (typeof(TChannel) == typeof(IOutputChannel))
            {
                MsmqChannelFactoryBase<IOutputChannel> factory = new MsmqOutputChannelFactory(this, context);
                MsmqVerifier.VerifySender<IOutputChannel>(factory);
                return (IChannelFactory<TChannel>)(object)factory;
            }
            else if (typeof(TChannel) == typeof(IOutputSessionChannel))
            {
                MsmqChannelFactoryBase<IOutputSessionChannel> factory = new MsmqOutputSessionChannelFactory(this, context);
                MsmqVerifier.VerifySender<IOutputSessionChannel>(factory);
                return (IChannelFactory<TChannel>)(object)factory;
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("TChannel", SR.GetString(SR.ChannelTypeNotSupported, typeof(TChannel)));
            }
        }

        public override IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

            TransportChannelListener msmqListener;

            MsmqTransportReceiveParameters receiveParameters = new MsmqTransportReceiveParameters(this, MsmqUri.NetMsmqAddressTranslator);

            if (typeof(TChannel) == typeof(IInputChannel))
            {
                msmqListener = new MsmqInputChannelListener(this, context, receiveParameters);
            }
            else if (typeof(TChannel) == typeof(IInputSessionChannel))
            {
                msmqListener = new MsmqInputSessionChannelListener(this, context, receiveParameters);
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("TChannel", SR.GetString(SR.ChannelTypeNotSupported, typeof(TChannel)));
            }
            AspNetEnvironment.Current.ApplyHostedContext(msmqListener, context);

            MsmqVerifier.VerifyReceiver(receiveParameters, msmqListener.Uri);

            return (IChannelListener<TChannel>)(object)msmqListener;
        }
    }
}
