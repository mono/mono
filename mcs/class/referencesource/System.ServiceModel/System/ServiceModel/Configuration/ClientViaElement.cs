//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Configuration;
    using System.ServiceModel.Description;

    public sealed partial class ClientViaElement : BehaviorExtensionElement
    {
        public ClientViaElement()
        {
        }

        [ConfigurationProperty(ConfigurationStrings.ViaUri)]
        public Uri ViaUri
        {
            get { return (Uri)base[ConfigurationStrings.ViaUri]; }
            set { base[ConfigurationStrings.ViaUri] = value; }
        }

        public override void CopyFrom(ServiceModelExtensionElement from)
        {
            base.CopyFrom(from);

            ClientViaElement source = (ClientViaElement)from;
#pragma warning suppress 56506 //Microsoft; base.CopyFrom() check for 'from' being null
            this.ViaUri = source.ViaUri;
        }

        protected internal override object CreateBehavior()
        {
            return new ClientViaBehavior(this.ViaUri);
        }

        public override Type BehaviorType
        {
            get { return typeof(ClientViaBehavior); }
        }
    }
}



