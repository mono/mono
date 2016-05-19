// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.Activities.XamlIntegration
{
    using System;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    // Exposes a DC-serializable type as IXmlSerializable so it can be serialized to XAML using x:XData
    internal class NetDataContractXmlSerializable<T> : IXmlSerializable where T : class
    {
        public NetDataContractXmlSerializable(T value = null)
        {
            this.Value = value;
        }

        public T Value
        {
            get;
            private set;
        }

        public XmlSchema GetSchema()
        {
            throw FxTrace.Exception.AsError(new NotSupportedException(SR.CannotGenerateSchemaForXmlSerializable(typeof(T).Name)));
        }

        public void ReadXml(XmlReader reader)
        {
            NetDataContractSerializer serializer = this.CreateSerializer();
            this.Value = (T)serializer.ReadObject(reader);
        }

        public void WriteXml(XmlWriter writer)
        {
            if (this.Value != null)
            {
                NetDataContractSerializer serializer = this.CreateSerializer();
                serializer.WriteObject(writer, this.Value);
            }
        }

        private NetDataContractSerializer CreateSerializer()
        {
            NetDataContractSerializer result = new NetDataContractSerializer();
 
            // The version-tolerant fallback of Simple is closer to the semantics of XAML
            result.AssemblyFormat = FormatterAssemblyStyle.Simple;
            return result;
        }
    }
}
