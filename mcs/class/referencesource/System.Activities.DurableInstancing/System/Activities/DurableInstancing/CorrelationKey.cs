//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.DurableInstancing
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Runtime;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Serialization;
    using System.Runtime.DurableInstancing;

    sealed class CorrelationKey
    {
        public CorrelationKey(Guid keyId) 
            : this(keyId, null, InstanceEncodingOption.None)
        {
        }

        public CorrelationKey(Guid keyId, IDictionary<XName, InstanceValue> keyMetadata, InstanceEncodingOption encodingOption)
        {
            this.KeyId = keyId;
            this.BinaryData = SerializationUtilities.SerializeKeyMetadata(keyMetadata, encodingOption);
        }

        public Guid KeyId
        {
            get;
            set;
        }

        public long StartPosition
        {
            get;
            set;
        }

        public ArraySegment<byte> BinaryData
        {
            get;
            set;
        }

        public void SerializeToXmlElement(XmlWriter xmlWriter)
        {
            xmlWriter.WriteStartElement("CorrelationKey");
            xmlWriter.WriteAttributeString("KeyId", this.KeyId.ToString());

            if (this.BinaryData.Array != null)
            {
                xmlWriter.WriteAttributeString("StartPosition", this.StartPosition.ToString(CultureInfo.InvariantCulture));
                xmlWriter.WriteAttributeString("BinaryLength", this.BinaryData.Count.ToString(CultureInfo.InvariantCulture));
            }

            xmlWriter.WriteEndElement();
        }

        public static List<CorrelationKey> BuildKeyList(ICollection<Guid> keys)
        {
            List<CorrelationKey> result = null;

            if (keys != null)
            {
                result = new List<CorrelationKey>(keys.Count);

                foreach (Guid guid in keys)
                {
                    result.Add(new CorrelationKey(guid));
                }
            }
            else
            {
                result = new List<CorrelationKey>();
            }

            return result;
        }

        public static List<CorrelationKey> BuildKeyList(IDictionary<Guid, IDictionary<XName, InstanceValue>> keys, InstanceEncodingOption encodingOption)
        {
            List<CorrelationKey> result = new List<CorrelationKey>();

            if (keys != null)
            {
                foreach (KeyValuePair<Guid, IDictionary<XName, InstanceValue>> keyValuePair in keys)
                {
                    result.Add(new CorrelationKey(keyValuePair.Key, keyValuePair.Value, encodingOption));
                }
            }

            return result;
        }
    }
}
