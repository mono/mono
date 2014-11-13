//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.MsmqIntegration
{
    using System.Net.Security;
    using System.Runtime.Serialization;
    using System.ServiceModel.Channels;
    using System.Collections.Generic;


    public sealed class MsmqIntegrationBindingElement : MsmqBindingElementBase
    {
        MsmqMessageSerializationFormat serializationFormat;
        Type[] targetSerializationTypes;

        public MsmqIntegrationBindingElement()
        {
            this.serializationFormat = MsmqIntegrationDefaults.SerializationFormat;
        }

        MsmqIntegrationBindingElement(MsmqIntegrationBindingElement other)
            : base(other)
        {
            this.serializationFormat = other.serializationFormat;
            if (other.targetSerializationTypes != null)
            {
                this.targetSerializationTypes = other.targetSerializationTypes.Clone() as Type[];
            }
        }

        public override string Scheme { get { return "msmq.formatname"; } }

        internal override MsmqUri.IAddressTranslator AddressTranslator
        {
            get
            {
                return MsmqUri.FormatNameAddressTranslator;
            }
        }

        // applicable on: client, server 
        public MsmqMessageSerializationFormat SerializationFormat
        {
            get { return this.serializationFormat; }
            set
            {
                if (!MsmqMessageSerializationFormatHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }

                this.serializationFormat = value;
            }
        }

        // applicable on: receiver
        public Type[] TargetSerializationTypes
        {
            get
            {
                if (null == this.targetSerializationTypes)
                    return null;
                else
                    return this.targetSerializationTypes.Clone() as Type[];
            }

            set
            {
                if (null == value)
                    this.targetSerializationTypes = null;
                else
                    this.targetSerializationTypes = value.Clone() as Type[];
            }
        }

        public override BindingElement Clone()
        {
            return new MsmqIntegrationBindingElement(this);
        }

        public override bool CanBuildChannelFactory<TChannel>(BindingContext context)
        {
            return typeof(TChannel) == typeof(IOutputChannel);
        }

        public override bool CanBuildChannelListener<TChannel>(BindingContext context)
        {
            return typeof(TChannel) == typeof(IInputChannel);
        }

        public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

            if (typeof(TChannel) != typeof(IOutputChannel))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("TChannel", SR.GetString(SR.ChannelTypeNotSupported, typeof(TChannel)));
            }

            MsmqChannelFactoryBase<IOutputChannel> factory = new MsmqIntegrationChannelFactory(this, context);
            MsmqVerifier.VerifySender<IOutputChannel>(factory);
            return (IChannelFactory<TChannel>)(object)factory;
        }

        public override IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

            if (typeof(TChannel) != typeof(IInputChannel))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("TChannel", SR.GetString(SR.ChannelTypeNotSupported, typeof(TChannel)));
            }

            MsmqIntegrationReceiveParameters receiveParameters = new MsmqIntegrationReceiveParameters(this);
            MsmqIntegrationChannelListener listener = new MsmqIntegrationChannelListener(this, context, receiveParameters);
            MsmqVerifier.VerifyReceiver(receiveParameters, listener.Uri);

            return (IChannelListener<TChannel>)(object)listener;
        }

        public override T GetProperty<T>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            if (typeof(T) == typeof(MessageVersion))
            {
                return (T)(object)(MessageVersion.None);
            }
            else
            {
                return base.GetProperty<T>(context);
            }
        }
    }
}

