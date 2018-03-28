//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.DurableInstancing
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Serialization;

    class DefaultObjectSerializer : IObjectSerializer
    {
        NetDataContractSerializer serializer;

        public DefaultObjectSerializer()
        {
            this.serializer = new NetDataContractSerializer();
        }

        public Dictionary<XName, object> DeserializePropertyBag(byte[] serializedValue)
        {
            using (MemoryStream memoryStream = new MemoryStream(serializedValue))
            {
                return this.DeserializePropertyBag(memoryStream);
            }
        }

        public object DeserializeValue(byte[] serializedValue)
        {
            using (MemoryStream memoryStream = new MemoryStream(serializedValue))
            {
                return this.DeserializeValue(memoryStream);
            }
        }

        public ArraySegment<byte> SerializePropertyBag(Dictionary<XName, object> value)
        {
            using (MemoryStream memoryStream = new MemoryStream(4096))
            {
                this.SerializePropertyBag(memoryStream, value);
                return new ArraySegment<byte>(memoryStream.GetBuffer(), 0, Convert.ToInt32(memoryStream.Length));
            }
        }

        public ArraySegment<byte> SerializeValue(object value)
        {
            using (MemoryStream memoryStream = new MemoryStream(4096))
            {
                this.SerializeValue(memoryStream, value);
                return new ArraySegment<byte>(memoryStream.GetBuffer(), 0, Convert.ToInt32(memoryStream.Length));
            }
        }

        protected virtual Dictionary<XName, object> DeserializePropertyBag(Stream stream)
        {
            using (XmlDictionaryReader dictionaryReader = XmlDictionaryReader.CreateBinaryReader(stream, XmlDictionaryReaderQuotas.Max))
            {
                Dictionary<XName, object> propertyBag = new Dictionary<XName, object>();

                if (dictionaryReader.ReadToDescendant("Property"))
                {
                    do
                    {
                        dictionaryReader.Read();
                        KeyValuePair<XName, object> property = (KeyValuePair<XName, object>) this.serializer.ReadObject(dictionaryReader);
                        propertyBag.Add(property.Key, property.Value);
                    }
                    while (dictionaryReader.ReadToNextSibling("Property"));
                }

                return propertyBag;
            }
        }

        protected virtual object DeserializeValue(Stream stream)
        {
            using (XmlDictionaryReader dictionaryReader = XmlDictionaryReader.CreateBinaryReader(stream, XmlDictionaryReaderQuotas.Max))
            {
                return this.serializer.ReadObject(dictionaryReader);
            }
        }

        protected virtual void SerializePropertyBag(Stream stream, Dictionary<XName, object> propertyBag)
        {
            using (XmlDictionaryWriter dictionaryWriter = XmlDictionaryWriter.CreateBinaryWriter(stream, null, null, false))
            {
                dictionaryWriter.WriteStartElement("Properties");

                foreach (KeyValuePair<XName, object> property in propertyBag)
                {
                    dictionaryWriter.WriteStartElement("Property");
                    this.serializer.WriteObject(dictionaryWriter, property);
                    dictionaryWriter.WriteEndElement();
                }

                dictionaryWriter.WriteEndElement();
            }
        }

        protected virtual void SerializeValue(Stream stream, object value)
        {
            using (XmlDictionaryWriter dictionaryWriter = XmlDictionaryWriter.CreateBinaryWriter(stream, null, null, false))
            {
                this.serializer.WriteObject(dictionaryWriter, value);
            }
        }
    }
}
