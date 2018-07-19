//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Runtime;
    using System.Xml;

    class ByteStreamMessageEncoderFactory : MessageEncoderFactory
    {
        ByteStreamMessageEncoder encoder;

        public ByteStreamMessageEncoderFactory(XmlDictionaryReaderQuotas quotas)
        {
            this.encoder = new ByteStreamMessageEncoder(quotas);
        }

        public override MessageEncoder Encoder
        {
            get { return this.encoder; }
        }

        public override MessageVersion MessageVersion
        {
            get { return encoder.MessageVersion; }
        }
    }
}
