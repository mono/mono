//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Dispatcher
{
    using System.Collections.Generic;
    using System.Runtime.Serialization.Json;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Web;
    using System.Text;
    using System.Xml;

    class JsonFormatMapping : MultiplexingFormatMapping
    {
        public static readonly WebContentFormat WebContentFormat = WebContentFormat.Json;
        
        static readonly string defaultMediaType = JsonGlobals.applicationJsonMediaType;
        static Dictionary<Encoding, MessageEncoder> encoders = new Dictionary<Encoding, MessageEncoder>();
        static object thisLock = new object();

        public JsonFormatMapping(Encoding writeEncoding, WebContentTypeMapper contentTypeMapper) 
            : base(writeEncoding, contentTypeMapper)
        { }

        public override WebContentFormat ContentFormat
        {
            get { return JsonFormatMapping.WebContentFormat; }
        }

        public override WebMessageFormat MessageFormat
        {
            get { return WebMessageFormat.Json; }
        }

        public override string DefaultMediaType
        {
            get { return JsonFormatMapping.defaultMediaType; }
        }

        protected override MessageEncoder Encoder
        {
            get 
            {
                lock (thisLock)
                {
                    if (!JsonFormatMapping.encoders.ContainsKey(this.writeEncoding))
                    {
                        JsonFormatMapping.encoders[this.writeEncoding] = new JsonMessageEncoderFactory(this.writeEncoding, 0, 0, new XmlDictionaryReaderQuotas(), false).Encoder;
                    }
                }
                return JsonFormatMapping.encoders[this.writeEncoding];
            }
        }
    }
}
