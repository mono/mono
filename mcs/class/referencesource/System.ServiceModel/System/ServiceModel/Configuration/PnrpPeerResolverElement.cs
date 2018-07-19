//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Configuration;
    using System.ServiceModel.Channels;

    public class PnrpPeerResolverElement : BindingElementExtensionElement
    {
        public PnrpPeerResolverElement()
        {
        }

        public override Type BindingElementType
        {
            get { return typeof(PnrpPeerResolverBindingElement); }
        }

        protected internal override BindingElement CreateBindingElement()
        {
            return new PnrpPeerResolverBindingElement();
        }
    }
}



