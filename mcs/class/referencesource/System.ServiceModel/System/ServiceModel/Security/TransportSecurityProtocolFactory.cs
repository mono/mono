//----------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    // Note that this protocol and other protocls represented by its
    // subclasses rely on transport security to provide message
    // integrity, confidentiality and request-reply correlation.  SOAP
    // level security features are add-ons to support custom tokens,
    // and do not have the responsibility to protect specific exchange
    // patterns.  So, thie protocol return true to both requst-reply
    // support as well as duplex support.
    class TransportSecurityProtocolFactory : SecurityProtocolFactory
    {
        public TransportSecurityProtocolFactory()
            : base()
        {
        }

        internal TransportSecurityProtocolFactory(TransportSecurityProtocolFactory factory)
            : base(factory)
        {
        }

        public override bool SupportsDuplex
        {
            get
            {
                return true;
            }
        }

        public override bool SupportsReplayDetection
        {
            get
            {
                return false;
            }
        }

        protected override SecurityProtocol OnCreateSecurityProtocol(EndpointAddress target, Uri via, object listenerSecurityState, TimeSpan timeout)
        {
            return new TransportSecurityProtocol(this, target, via);
        }
    }
}
