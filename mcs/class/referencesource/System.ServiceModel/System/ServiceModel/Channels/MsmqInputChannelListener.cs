//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    sealed class MsmqInputChannelListener
        : MsmqInputChannelListenerBase
    {
        internal MsmqInputChannelListener(MsmqBindingElementBase bindingElement, BindingContext context, MsmqReceiveParameters receiveParameters)
            : base(bindingElement, context, receiveParameters)
        {
            SetSecurityTokenAuthenticator(MsmqUri.NetMsmqAddressTranslator.Scheme, context);
        }

        protected override IInputChannel CreateInputChannel(MsmqInputChannelListenerBase listener)
        {
            return new MsmqInputChannel(listener as MsmqInputChannelListener);
        }
    }
}
