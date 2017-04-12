//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.ServiceModel.Dispatcher;
    using System.Text;
    using System.Xml;

    public abstract class AddressHeader
    {
        ParameterHeader header;

        protected AddressHeader()
        {
        }

        internal bool IsReferenceProperty
        {
            get
            {
                BufferedAddressHeader bah = this as BufferedAddressHeader;
                return bah != null && bah.IsReferencePropertyHeader;
            }
        }

        public abstract string Name { get; }
        public abstract string Namespace { get; }

        public static AddressHeader CreateAddressHeader(object value)
        {
            Type type = GetObjectType(value);
            return CreateAddressHeader(value, DataContractSerializerDefaults.CreateSerializer(type, int.MaxValue/*maxItems*/));
        }

        public static AddressHeader CreateAddressHeader(object value, XmlObjectSerializer serializer)
        {
            if (serializer == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("serializer"));
            return new XmlObjectSerializerAddressHeader(value, serializer);
        }

        public static AddressHeader CreateAddressHeader(string name, string ns, object value)
        {
            return CreateAddressHeader(name, ns, value, DataContractSerializerDefaults.CreateSerializer(GetObjectType(value), name, ns, int.MaxValue/*maxItems*/));
        }

        internal static AddressHeader CreateAddressHeader(XmlDictionaryString name, XmlDictionaryString ns, object value)
        {
            return new DictionaryAddressHeader(name, ns, value);
        }

        public static AddressHeader CreateAddressHeader(string name, string ns, object value, XmlObjectSerializer serializer)
        {
            if (serializer == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("serializer"));
            return new XmlObjectSerializerAddressHeader(name, ns, value, serializer);
        }

        static Type GetObjectType(object value)
        {
            return (value == null) ? typeof(object) : value.GetType();
        }

        public override bool Equals(object obj)
        {
            AddressHeader hdr = obj as AddressHeader;
            if (hdr == null)
                return false;

            StringBuilder builder = new StringBuilder();
            string hdr1 = GetComparableForm(builder);

            builder.Remove(0, builder.Length);
            string hdr2 = hdr.GetComparableForm(builder);

            if (hdr1.Length != hdr2.Length)
                return false;

            if (string.CompareOrdinal(hdr1, hdr2) != 0)
                return false;

            return true;
        }

        internal string GetComparableForm()
        {
            return GetComparableForm(new StringBuilder());
        }

        internal string GetComparableForm(StringBuilder builder)
        {
            return EndpointAddressProcessor.GetComparableForm(builder, GetComparableReader());
        }

        public override int GetHashCode()
        {
            return GetComparableForm().GetHashCode();
        }

        public T GetValue<T>()
        {
            return GetValue<T>(DataContractSerializerDefaults.CreateSerializer(typeof(T), this.Name, this.Namespace, int.MaxValue/*maxItems*/));
        }

        public T GetValue<T>(XmlObjectSerializer serializer)
        {
            if (serializer == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("serializer"));
            using (XmlDictionaryReader reader = GetAddressHeaderReader())
            {
                if (serializer.IsStartObject(reader))
                    return (T)serializer.ReadObject(reader);
                else
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.ExpectedElementMissing, Name, Namespace)));
            }
        }

        public virtual XmlDictionaryReader GetAddressHeaderReader()
        {
            XmlBuffer buffer = new XmlBuffer(int.MaxValue);
            XmlDictionaryWriter writer = buffer.OpenSection(XmlDictionaryReaderQuotas.Max);
            WriteAddressHeader(writer);
            buffer.CloseSection();
            buffer.Close();
            return buffer.GetReader(0);
        }

        XmlDictionaryReader GetComparableReader()
        {
            XmlBuffer buffer = new XmlBuffer(int.MaxValue);
            XmlDictionaryWriter writer = buffer.OpenSection(XmlDictionaryReaderQuotas.Max);
            // WSAddressingAugust2004 does not write the IsReferenceParameter attribute, 
            // and that's good for a consistent comparable form
            ParameterHeader.WriteStartHeader(writer, this, AddressingVersion.WSAddressingAugust2004);
            ParameterHeader.WriteHeaderContents(writer, this);
            writer.WriteEndElement();
            buffer.CloseSection();
            buffer.Close();
            return buffer.GetReader(0);
        }

        protected virtual void OnWriteStartAddressHeader(XmlDictionaryWriter writer)
        {
            writer.WriteStartElement(Name, Namespace);
        }

        protected abstract void OnWriteAddressHeaderContents(XmlDictionaryWriter writer);

        public MessageHeader ToMessageHeader()
        {
            if (header == null)
                header = new ParameterHeader(this);
            return header;
        }

        public void WriteAddressHeader(XmlWriter writer)
        {
            WriteAddressHeader(XmlDictionaryWriter.CreateDictionaryWriter(writer));
        }

        public void WriteAddressHeader(XmlDictionaryWriter writer)
        {
            if (writer == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("writer"));
            WriteStartAddressHeader(writer);
            WriteAddressHeaderContents(writer);
            writer.WriteEndElement();
        }

        public void WriteStartAddressHeader(XmlDictionaryWriter writer)
        {
            if (writer == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("writer"));
            OnWriteStartAddressHeader(writer);
        }

        public void WriteAddressHeaderContents(XmlDictionaryWriter writer)
        {
            if (writer == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("writer"));
            OnWriteAddressHeaderContents(writer);
        }

        class ParameterHeader : MessageHeader
        {
            AddressHeader parameter;

            public override bool IsReferenceParameter
            {
                get { return true; }
            }

            public override string Name
            {
                get { return parameter.Name; }
            }

            public override string Namespace
            {
                get { return parameter.Namespace; }
            }

            public ParameterHeader(AddressHeader parameter)
            {
                this.parameter = parameter;
            }

            protected override void OnWriteStartHeader(XmlDictionaryWriter writer, MessageVersion messageVersion)
            {
                if (messageVersion == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("messageVersion"));

                WriteStartHeader(writer, parameter, messageVersion.Addressing);
            }

            protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
            {
                WriteHeaderContents(writer, parameter);
            }

            internal static void WriteStartHeader(XmlDictionaryWriter writer, AddressHeader parameter, AddressingVersion addressingVersion)
            {
                parameter.WriteStartAddressHeader(writer);
                if (addressingVersion == AddressingVersion.WSAddressing10)
                {
                    writer.WriteAttributeString(XD.AddressingDictionary.IsReferenceParameter, XD.Addressing10Dictionary.Namespace, "true");
                }
            }

            internal static void WriteHeaderContents(XmlDictionaryWriter writer, AddressHeader parameter)
            {
                parameter.WriteAddressHeaderContents(writer);
            }
        }

        class XmlObjectSerializerAddressHeader : AddressHeader
        {
            XmlObjectSerializer serializer;
            object objectToSerialize;
            string name;
            string ns;

            public XmlObjectSerializerAddressHeader(object objectToSerialize, XmlObjectSerializer serializer)
            {
                this.serializer = serializer;
                this.objectToSerialize = objectToSerialize;

                Type type = (objectToSerialize == null) ? typeof(object) : objectToSerialize.GetType();
                XmlQualifiedName rootName = new XsdDataContractExporter().GetRootElementName(type);
                this.name = rootName.Name;
                this.ns = rootName.Namespace;
            }

            public XmlObjectSerializerAddressHeader(string name, string ns, object objectToSerialize, XmlObjectSerializer serializer)
            {
                if ((null == name) || (name.Length == 0))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("name"));
                }

                this.serializer = serializer;
                this.objectToSerialize = objectToSerialize;
                this.name = name;
                this.ns = ns;
            }

            public override string Name
            {
                get { return name; }
            }

            public override string Namespace
            {
                get { return ns; }
            }

            object ThisLock
            {
                get { return this; }
            }

            protected override void OnWriteAddressHeaderContents(XmlDictionaryWriter writer)
            {
                lock (ThisLock)
                {
                    serializer.WriteObjectContent(writer, objectToSerialize);
                }
            }
        }

        // Microsoft, This will be kept internal for now.  If the optimization needs to be public, we'll re-evaluate it.
        class DictionaryAddressHeader : XmlObjectSerializerAddressHeader
        {
            XmlDictionaryString name;
            XmlDictionaryString ns;

            public DictionaryAddressHeader(XmlDictionaryString name, XmlDictionaryString ns, object value)
                : base(name.Value, ns.Value, value, DataContractSerializerDefaults.CreateSerializer(GetObjectType(value), name, ns, int.MaxValue/*maxItems*/))
            {
                this.name = name;
                this.ns = ns;
            }

            protected override void OnWriteStartAddressHeader(XmlDictionaryWriter writer)
            {
                writer.WriteStartElement(name, ns);
            }
        }
    }

    class BufferedAddressHeader : AddressHeader
    {
        string name;
        string ns;
        XmlBuffer buffer;
        bool isReferenceProperty;

        public BufferedAddressHeader(XmlDictionaryReader reader)
        {
            buffer = new XmlBuffer(int.MaxValue);
            XmlDictionaryWriter writer = buffer.OpenSection(reader.Quotas);
            Fx.Assert(reader.NodeType == XmlNodeType.Element, "");
            name = reader.LocalName;
            ns = reader.NamespaceURI;
            Fx.Assert(name != null, "");
            Fx.Assert(ns != null, "");
            writer.WriteNode(reader, false);
            buffer.CloseSection();
            buffer.Close();
            this.isReferenceProperty = false;
        }

        public BufferedAddressHeader(XmlDictionaryReader reader, bool isReferenceProperty)
            : this(reader)
        {
            this.isReferenceProperty = isReferenceProperty;
        }

        public bool IsReferencePropertyHeader { get { return this.isReferenceProperty; } }

        public override string Name
        {
            get { return name; }
        }

        public override string Namespace
        {
            get { return ns; }
        }

        public override XmlDictionaryReader GetAddressHeaderReader()
        {
            return buffer.GetReader(0);
        }

        protected override void OnWriteStartAddressHeader(XmlDictionaryWriter writer)
        {
            XmlDictionaryReader reader = GetAddressHeaderReader();
            writer.WriteStartElement(reader.Prefix, reader.LocalName, reader.NamespaceURI);
            writer.WriteAttributes(reader, false);
            reader.Close();
        }

        protected override void OnWriteAddressHeaderContents(XmlDictionaryWriter writer)
        {
            XmlDictionaryReader reader = GetAddressHeaderReader();
            reader.ReadStartElement();
            while (reader.NodeType != XmlNodeType.EndElement)
                writer.WriteNode(reader, false);
            reader.ReadEndElement();
            reader.Close();
        }
    }
}
