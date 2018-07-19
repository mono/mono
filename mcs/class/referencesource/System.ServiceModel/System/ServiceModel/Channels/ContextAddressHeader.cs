//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.Runtime;
    using System.Xml;

    class ContextAddressHeader : AddressHeader
    {
        ContextDictionary context;

        public ContextAddressHeader(IDictionary<string, string> context)
            : base()
        {
            Fx.Assert(context != null, "caller must validate");
            this.context = new ContextDictionary(context);
        }

        public override string Name
        {
            get { return ContextMessageHeader.ContextHeaderName; }
        }

        public override string Namespace
        {
            get { return ContextMessageHeader.ContextHeaderNamespace; }
        }

        protected override void OnWriteAddressHeaderContents(XmlDictionaryWriter writer)
        {
            ContextMessageHeader.WriteHeaderContents(writer, this.context);
        }
    }
}
