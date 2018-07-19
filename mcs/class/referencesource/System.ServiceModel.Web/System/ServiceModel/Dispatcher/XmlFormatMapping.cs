//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Dispatcher
{
    using System.Collections.Generic;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Web;
    using System.Text;
    using System.Xml;

    class XmlFormatMapping : MultiplexingFormatMapping
    {
        public static readonly WebContentFormat WebContentFormat = WebContentFormat.Xml;
        
        internal static readonly string defaultMediaType = "application/xml";
        static Dictionary<Encoding, MessageEncoder> encoders = new Dictionary<Encoding, MessageEncoder>();
        static object thisLock = new object();

        public XmlFormatMapping(Encoding writeEncoding, WebContentTypeMapper contentTypeMapper)
            : base(writeEncoding, contentTypeMapper)
        { }

        public override WebContentFormat ContentFormat
        {
            get { return XmlFormatMapping.WebContentFormat; }
        }

        public override WebMessageFormat MessageFormat
        {
            get { return WebMessageFormat.Xml; }
        }

        public override string DefaultMediaType
        {
            get { return XmlFormatMapping.defaultMediaType; }
        }

        protected override MessageEncoder Encoder
        {
            get
            {
                lock (thisLock)
                {
                    if (!XmlFormatMapping.encoders.ContainsKey(this.writeEncoding))
                    {
                        XmlFormatMapping.encoders[this.writeEncoding] = new TextMessageEncoderFactory(MessageVersion.None, this.writeEncoding, 0, 0, new XmlDictionaryReaderQuotas()).Encoder;
                    }
                }
                return XmlFormatMapping.encoders[this.writeEncoding];
            }
        }
    }
}
