//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.MsmqIntegration
{
    using System.ServiceModel.Channels;
    using System.Xml;
    using System.Xml.Serialization;

    sealed class MsmqIntegrationChannelListener
        : MsmqInputChannelListenerBase
    {
        XmlSerializer[] xmlSerializerList;

        internal MsmqIntegrationChannelListener(MsmqBindingElementBase bindingElement, BindingContext context, MsmqReceiveParameters receiveParameters)
            : base(bindingElement, context, receiveParameters, null)
        {
            SetSecurityTokenAuthenticator(MsmqUri.FormatNameAddressTranslator.Scheme, context);
            MsmqIntegrationReceiveParameters parameters = receiveParameters as MsmqIntegrationReceiveParameters;
            xmlSerializerList = XmlSerializer.FromTypes(parameters.TargetSerializationTypes);
        }

        public override string Scheme
        {
            get { return "msmq.formatname"; }
        }

        internal XmlSerializer[] XmlSerializerList
        {
            get { return this.xmlSerializerList; }
        }

        protected override IInputChannel CreateInputChannel(MsmqInputChannelListenerBase listener)
        {
            return new MsmqIntegrationInputChannel(this);
        }
    }
}
