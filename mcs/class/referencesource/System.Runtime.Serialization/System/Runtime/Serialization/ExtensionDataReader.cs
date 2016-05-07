//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.Runtime.Serialization
{
    using System;
    using System.Text;
    using System.IO;
    using System.Xml;
    using System.Diagnostics;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Security;

    // NOTE: XmlReader methods that are not needed have been left un-implemented

    class ExtensionDataReader : XmlReader
    {
        enum ExtensionDataNodeType
        {
            None,
            Element,
            EndElement,
            Text,
            Xml,
            ReferencedElement,
            NullElement,
        }

        Hashtable cache = new Hashtable();

        ElementData[] elements;
        ElementData element;
        ElementData nextElement;

        ReadState readState = ReadState.Initial;
        ExtensionDataNodeType internalNodeType;
        XmlNodeType nodeType;
        int depth;
        string localName;
        string ns;
        string prefix;
        string value;
        int attributeCount;
        int attributeIndex;
        XmlNodeReader xmlNodeReader;
        Queue<IDataNode> deserializedDataNodes;
        XmlObjectSerializerReadContext context;

        [Fx.Tag.SecurityNote(Critical = "Holds static mappings from namespaces to prefixes."
            + " Static fields are marked SecurityCritical or readonly to prevent data from being modified or leaked to other components in appdomain.")]
        [SecurityCritical]
        static Dictionary<string, string> nsToPrefixTable;

        [Fx.Tag.SecurityNote(Critical = "Holds static mappings from prefixes to namespaces."
            + " Static fields are marked SecurityCritical or readonly to prevent data from being modified or leaked to other components in appdomain.")]
        [SecurityCritical]
        static Dictionary<string, string> prefixToNsTable;

        [Fx.Tag.SecurityNote(Critical = "Initializes information in critical static cache.",
            Safe = "Cache is initialized with well-known namespace, prefix mappings.")]
        [SecuritySafeCritical]
        static ExtensionDataReader()
        {
            nsToPrefixTable = new Dictionary<string, string>();
            prefixToNsTable = new Dictionary<string, string>();
            AddPrefix(Globals.XsiPrefix, Globals.SchemaInstanceNamespace);
            AddPrefix(Globals.SerPrefix, Globals.SerializationNamespace);
            AddPrefix(String.Empty, String.Empty);
        }

        internal ExtensionDataReader(XmlObjectSerializerReadContext context)
        {
            this.attributeIndex = -1;
            this.context = context;
        }

        internal void SetDeserializedValue(object obj)
        {
            IDataNode deserializedDataNode = (deserializedDataNodes == null || deserializedDataNodes.Count == 0) ? null : deserializedDataNodes.Dequeue();
            if (deserializedDataNode != null && !(obj is IDataNode))
            {
                deserializedDataNode.Value = obj;
                deserializedDataNode.IsFinalValue = true;
            }
        }

        internal IDataNode GetCurrentNode()
        {
            IDataNode retVal = element.dataNode;
            Skip();
            return retVal;
        }

        internal void SetDataNode(IDataNode dataNode, string name, string ns)
        {
            SetNextElement(dataNode, name, ns, null);
            this.element = nextElement;
            this.nextElement = null;
            SetElement();
        }

        internal void Reset()
        {
            this.localName = null;
            this.ns = null;
            this.prefix = null;
            this.value = null;
            this.attributeCount = 0;
            this.attributeIndex = -1;
            this.depth = 0;
            this.element = null;
            this.nextElement = null;
            this.elements = null;
            this.deserializedDataNodes = null;
        }

        bool IsXmlDataNode { get { return (internalNodeType == ExtensionDataNodeType.Xml); } }

        public override XmlNodeType NodeType { get { return IsXmlDataNode ? xmlNodeReader.NodeType : nodeType; } }
        public override string LocalName { get { return IsXmlDataNode ? xmlNodeReader.LocalName : localName; } }
        public override string NamespaceURI { get { return IsXmlDataNode ? xmlNodeReader.NamespaceURI : ns; } }
        public override string Prefix { get { return IsXmlDataNode ? xmlNodeReader.Prefix : prefix; } }
        public override string Value { get { return IsXmlDataNode ? xmlNodeReader.Value : value; } }
        public override int Depth { get { return IsXmlDataNode ? xmlNodeReader.Depth : depth; } }
        public override int AttributeCount { get { return IsXmlDataNode ? xmlNodeReader.AttributeCount : attributeCount; } }
        public override bool EOF { get { return IsXmlDataNode ? xmlNodeReader.EOF : (readState == ReadState.EndOfFile); } }
        public override ReadState ReadState { get { return IsXmlDataNode ? xmlNodeReader.ReadState : readState; } }
        public override bool IsEmptyElement { get { return IsXmlDataNode ? xmlNodeReader.IsEmptyElement : false; } }
        public override bool IsDefault { get { return IsXmlDataNode ? xmlNodeReader.IsDefault : base.IsDefault; } }
        public override char QuoteChar { get { return IsXmlDataNode ? xmlNodeReader.QuoteChar : base.QuoteChar; } }
        public override XmlSpace XmlSpace { get { return IsXmlDataNode ? xmlNodeReader.XmlSpace : base.XmlSpace; } }
        public override string XmlLang { get { return IsXmlDataNode ? xmlNodeReader.XmlLang : base.XmlLang; } }
        public override string this[int i] { get { return IsXmlDataNode ? xmlNodeReader[i] : GetAttribute(i); } }
        public override string this[string name] { get { return IsXmlDataNode ? xmlNodeReader[name] : GetAttribute(name); } }
        public override string this[string name, string namespaceURI] { get { return IsXmlDataNode ? xmlNodeReader[name, namespaceURI] : GetAttribute(name, namespaceURI); } }

        public override bool MoveToFirstAttribute()
        {
            if (IsXmlDataNode)
                return xmlNodeReader.MoveToFirstAttribute();

            if (attributeCount == 0)
                return false;
            MoveToAttribute(0);
            return true;
        }

        public override bool MoveToNextAttribute()
        {
            if (IsXmlDataNode)
                return xmlNodeReader.MoveToNextAttribute();

            if (attributeIndex + 1 >= attributeCount)
                return false;
            MoveToAttribute(attributeIndex + 1);
            return true;
        }

        public override void MoveToAttribute(int index)
        {
            if (IsXmlDataNode)
                xmlNodeReader.MoveToAttribute(index);
            else
            {
                if (index < 0 || index >= attributeCount)
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.InvalidXmlDeserializingExtensionData)));

                this.nodeType = XmlNodeType.Attribute;
                AttributeData attribute = element.attributes[index];
                this.localName = attribute.localName;
                this.ns = attribute.ns;
                this.prefix = attribute.prefix;
                this.value = attribute.value;
                this.attributeIndex = index;
            }
        }

        public override string GetAttribute(string name, string namespaceURI)
        {
            if (IsXmlDataNode)
                return xmlNodeReader.GetAttribute(name, namespaceURI);

            for (int i = 0; i < element.attributeCount; i++)
            {
                AttributeData attribute = element.attributes[i];
                if (attribute.localName == name && attribute.ns == namespaceURI)
                    return attribute.value;
            }

            return null;
        }

        public override bool MoveToAttribute(string name, string namespaceURI)
        {
            if (IsXmlDataNode)
                return xmlNodeReader.MoveToAttribute(name, ns);

            for (int i = 0; i < element.attributeCount; i++)
            {
                AttributeData attribute = element.attributes[i];
                if (attribute.localName == name && attribute.ns == namespaceURI)
                {
                    MoveToAttribute(i);
                    return true;
                }
            }

            return false;
        }

        public override bool MoveToElement()
        {
            if (IsXmlDataNode)
                return xmlNodeReader.MoveToElement();

            if (this.nodeType != XmlNodeType.Attribute)
                return false;

            SetElement();
            return true;
        }

        void SetElement()
        {
            this.nodeType = XmlNodeType.Element;
            this.localName = element.localName;
            this.ns = element.ns;
            this.prefix = element.prefix;
            this.value = String.Empty;
            this.attributeCount = element.attributeCount;
            this.attributeIndex = -1;
        }

        [Fx.Tag.SecurityNote(Critical = "Accesses SecurityCritical static cache to look up namespace given a prefix.",
            Safe = "Read only access.")]
        [SecuritySafeCritical]
        public override string LookupNamespace(string prefix)
        {
            if (IsXmlDataNode)
                return xmlNodeReader.LookupNamespace(prefix);

            string ns;
            if (!prefixToNsTable.TryGetValue(prefix, out ns))
                return null;
            return ns;
        }

        public override void Skip()
        {
            if (IsXmlDataNode)
                xmlNodeReader.Skip();
            else
            {
                if (ReadState != ReadState.Interactive)
                    return;
                MoveToElement();
                if (IsElementNode(this.internalNodeType))
                {
                    int depth = 1;
                    while (depth != 0)
                    {
                        if (!Read())
                            throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.InvalidXmlDeserializingExtensionData)));

                        if (IsElementNode(this.internalNodeType))
                            depth++;
                        else if (this.internalNodeType == ExtensionDataNodeType.EndElement)
                        {
                            ReadEndElement();
                            depth--;
                        }
                    }
                }
                else
                    Read();
            }
        }

        bool IsElementNode(ExtensionDataNodeType nodeType)
        {
            return (nodeType == ExtensionDataNodeType.Element ||
                nodeType == ExtensionDataNodeType.ReferencedElement ||
                nodeType == ExtensionDataNodeType.NullElement);
        }

        public override void Close()
        {
            if (IsXmlDataNode)
                xmlNodeReader.Close();
            else
            {
                Reset();
                this.readState = ReadState.Closed;
            }
        }

        public override bool Read()
        {
            if (nodeType == XmlNodeType.Attribute && MoveToNextAttribute())
                return true;

            MoveNext(element.dataNode);

            switch (internalNodeType)
            {
                case ExtensionDataNodeType.Element:
                case ExtensionDataNodeType.ReferencedElement:
                case ExtensionDataNodeType.NullElement:
                    PushElement();
                    SetElement();
                    break;

                case ExtensionDataNodeType.Text:
                    this.nodeType = XmlNodeType.Text;
                    this.prefix = String.Empty;
                    this.ns = String.Empty;
                    this.localName = String.Empty;
                    this.attributeCount = 0;
                    this.attributeIndex = -1;
                    break;

                case ExtensionDataNodeType.EndElement:
                    this.nodeType = XmlNodeType.EndElement;
                    this.prefix = String.Empty;
                    this.ns = String.Empty;
                    this.localName = String.Empty;
                    this.value = String.Empty;
                    this.attributeCount = 0;
                    this.attributeIndex = -1;
                    PopElement();
                    break;

                case ExtensionDataNodeType.None:
                    if (depth != 0)
                        throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.InvalidXmlDeserializingExtensionData)));
                    this.nodeType = XmlNodeType.None;
                    this.prefix = String.Empty;
                    this.ns = String.Empty;
                    this.localName = String.Empty;
                    this.value = String.Empty;
                    this.attributeCount = 0;
                    readState = ReadState.EndOfFile;
                    return false;

                case ExtensionDataNodeType.Xml:
                    // do nothing
                    break;

                default:
                    Fx.Assert("ExtensionDataReader in invalid state");
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(SR.GetString(SR.InvalidStateInExtensionDataReader)));
            }
            readState = ReadState.Interactive;
            return true;
        }

        public override string Name
        {
            get
            {
                if (IsXmlDataNode)
                {
                    return xmlNodeReader.Name;
                }
                Fx.Assert("ExtensionDataReader Name property should only be called for IXmlSerializable");
                return string.Empty;
            }
        }

        public override bool HasValue
        {
            get
            {
                if (IsXmlDataNode)
                {
                    return xmlNodeReader.HasValue;
                }
                Fx.Assert("ExtensionDataReader HasValue property should only be called for IXmlSerializable");
                return false;
            }
        }

        public override string BaseURI
        {
            get
            {
                if (IsXmlDataNode)
                {
                    return xmlNodeReader.BaseURI;
                }
                Fx.Assert("ExtensionDataReader BaseURI property should only be called for IXmlSerializable");
                return string.Empty;
            }
        }

        public override XmlNameTable NameTable
        {
            get
            {
                if (IsXmlDataNode)
                {
                    return xmlNodeReader.NameTable;
                }
                Fx.Assert("ExtensionDataReader NameTable property should only be called for IXmlSerializable");
                return null;
            }
        }

        public override string GetAttribute(string name)
        {
            if (IsXmlDataNode)
            {
                return xmlNodeReader.GetAttribute(name);
            }
            Fx.Assert("ExtensionDataReader GetAttribute method should only be called for IXmlSerializable");
            return null;
        }

        public override string GetAttribute(int i)
        {
            if (IsXmlDataNode)
            {
                return xmlNodeReader.GetAttribute(i);
            }
            Fx.Assert("ExtensionDataReader GetAttribute method should only be called for IXmlSerializable");
            return null;
        }

        public override bool MoveToAttribute(string name)
        {
            if (IsXmlDataNode)
            {
                return xmlNodeReader.MoveToAttribute(name);
            }
            Fx.Assert("ExtensionDataReader MoveToAttribute method should only be called for IXmlSerializable");
            return false;
        }

        public override void ResolveEntity()
        {
            if (IsXmlDataNode)
            {
                xmlNodeReader.ResolveEntity();
            }
            else
            {
                Fx.Assert("ExtensionDataReader ResolveEntity method should only be called for IXmlSerializable");
            }
        }

        public override bool ReadAttributeValue()
        {
            if (IsXmlDataNode)
            {
                return xmlNodeReader.ReadAttributeValue();
            }
            Fx.Assert("ExtensionDataReader ReadAttributeValue method should only be called for IXmlSerializable");
            return false;
        }

        void MoveNext(IDataNode dataNode)
        {
            switch (this.internalNodeType)
            {
                case ExtensionDataNodeType.Text:
                case ExtensionDataNodeType.ReferencedElement:
                case ExtensionDataNodeType.NullElement:
                    this.internalNodeType = ExtensionDataNodeType.EndElement;
                    return;
                default:
                    Type dataNodeType = dataNode.DataType;
                    if (dataNodeType == Globals.TypeOfClassDataNode)
                        MoveNextInClass((ClassDataNode)dataNode);
                    else if (dataNodeType == Globals.TypeOfCollectionDataNode)
                        MoveNextInCollection((CollectionDataNode)dataNode);
                    else if (dataNodeType == Globals.TypeOfISerializableDataNode)
                        MoveNextInISerializable((ISerializableDataNode)dataNode);
                    else if (dataNodeType == Globals.TypeOfXmlDataNode)
                        MoveNextInXml((XmlDataNode)dataNode);
                    else if (dataNode.Value != null)
                        MoveToDeserializedObject(dataNode);
                    else
                    {
                        Fx.Assert("Encountered invalid data node when deserializing unknown data");
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(SR.GetString(SR.InvalidStateInExtensionDataReader)));
                    }
                    break;
            }
        }

        void SetNextElement(IDataNode node, string name, string ns, string prefix)
        {
            this.internalNodeType = ExtensionDataNodeType.Element;
            nextElement = GetNextElement();
            nextElement.localName = name;
            nextElement.ns = ns;
            nextElement.prefix = prefix;
            if (node == null)
            {
                nextElement.attributeCount = 0;
                nextElement.AddAttribute(Globals.XsiPrefix, Globals.SchemaInstanceNamespace, Globals.XsiNilLocalName, Globals.True);
                this.internalNodeType = ExtensionDataNodeType.NullElement;
            }
            else if (!CheckIfNodeHandled(node))
            {
                AddDeserializedDataNode(node);
                node.GetData(nextElement);
                if (node is XmlDataNode)
                    MoveNextInXml((XmlDataNode)node);
            }
        }

        void AddDeserializedDataNode(IDataNode node)
        {
            if (node.Id != Globals.NewObjectId && (node.Value == null || !node.IsFinalValue))
            {
                if (deserializedDataNodes == null)
                    deserializedDataNodes = new Queue<IDataNode>();
                deserializedDataNodes.Enqueue(node);
            }
        }

        bool CheckIfNodeHandled(IDataNode node)
        {
            bool handled = false;
            if (node.Id != Globals.NewObjectId)
            {
                handled = (cache[node] != null);
                if (handled)
                {
                    if (nextElement == null)
                        nextElement = GetNextElement();
                    nextElement.attributeCount = 0;
                    nextElement.AddAttribute(Globals.SerPrefix, Globals.SerializationNamespace, Globals.RefLocalName, node.Id.ToString(NumberFormatInfo.InvariantInfo));
                    nextElement.AddAttribute(Globals.XsiPrefix, Globals.SchemaInstanceNamespace, Globals.XsiNilLocalName, Globals.True);
                    this.internalNodeType = ExtensionDataNodeType.ReferencedElement;
                }
                else
                {
                    cache.Add(node, node);
                }
            }
            return handled;
        }

        void MoveNextInClass(ClassDataNode dataNode)
        {
            if (dataNode.Members != null && element.childElementIndex < dataNode.Members.Count)
            {
                if (element.childElementIndex == 0)
                    this.context.IncrementItemCount(-dataNode.Members.Count);

                ExtensionDataMember member = dataNode.Members[element.childElementIndex++];
                SetNextElement(member.Value, member.Name, member.Namespace, GetPrefix(member.Namespace));
            }
            else
            {
                this.internalNodeType = ExtensionDataNodeType.EndElement;
                element.childElementIndex = 0;
            }
        }

        void MoveNextInCollection(CollectionDataNode dataNode)
        {
            if (dataNode.Items != null && element.childElementIndex < dataNode.Items.Count)
            {
                if (element.childElementIndex == 0)
                    this.context.IncrementItemCount(-dataNode.Items.Count);

                IDataNode item = dataNode.Items[element.childElementIndex++];
                SetNextElement(item, dataNode.ItemName, dataNode.ItemNamespace, GetPrefix(dataNode.ItemNamespace));
            }
            else
            {
                this.internalNodeType = ExtensionDataNodeType.EndElement;
                element.childElementIndex = 0;
            }
        }

        void MoveNextInISerializable(ISerializableDataNode dataNode)
        {
            if (dataNode.Members != null && element.childElementIndex < dataNode.Members.Count)
            {
                if (element.childElementIndex == 0)
                    this.context.IncrementItemCount(-dataNode.Members.Count);

                ISerializableDataMember member = dataNode.Members[element.childElementIndex++];
                SetNextElement(member.Value, member.Name, String.Empty, String.Empty);
            }
            else
            {
                this.internalNodeType = ExtensionDataNodeType.EndElement;
                element.childElementIndex = 0;
            }
        }

        void MoveNextInXml(XmlDataNode dataNode)
        {
            if (IsXmlDataNode)
            {
                xmlNodeReader.Read();
                if (xmlNodeReader.Depth == 0)
                {
                    this.internalNodeType = ExtensionDataNodeType.EndElement;
                    xmlNodeReader = null;
                }
            }
            else
            {
                internalNodeType = ExtensionDataNodeType.Xml;
                if (element == null)
                    element = nextElement;
                else
                    PushElement();

                XmlNode wrapperElement = XmlObjectSerializerReadContext.CreateWrapperXmlElement(dataNode.OwnerDocument,
                    dataNode.XmlAttributes, dataNode.XmlChildNodes, element.prefix, element.localName, element.ns);
                for (int i = 0; i < element.attributeCount; i++)
                {
                    AttributeData a = element.attributes[i];
                    XmlAttribute xmlAttr = dataNode.OwnerDocument.CreateAttribute(a.prefix, a.localName, a.ns);
                    xmlAttr.Value = a.value;
                    wrapperElement.Attributes.Append(xmlAttr);
                }
                xmlNodeReader = new XmlNodeReader(wrapperElement);
                xmlNodeReader.Read();
            }
        }

        void MoveToDeserializedObject(IDataNode dataNode)
        {
            Type type = dataNode.DataType;
            bool isTypedNode = true;
            if (type == Globals.TypeOfObject)
            {
                type = dataNode.Value.GetType();
                if (type == Globals.TypeOfObject)
                {
                    this.internalNodeType = ExtensionDataNodeType.EndElement;
                    return;
                }
                isTypedNode = false;
            }

            if (!MoveToText(type, dataNode, isTypedNode))
            {
                if (dataNode.IsFinalValue)
                {
                    this.internalNodeType = ExtensionDataNodeType.EndElement;
                }
                else
                {
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.InvalidDataNode, DataContract.GetClrTypeFullName(type))));
                }
            }
        }

        bool MoveToText(Type type, IDataNode dataNode, bool isTypedNode)
        {
            bool handled = true;
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                    this.value = XmlConvert.ToString(isTypedNode ? ((DataNode<bool>)dataNode).GetValue() : (bool)dataNode.Value);
                    break;
                case TypeCode.Char:
                    this.value = XmlConvert.ToString((int)(isTypedNode ? ((DataNode<char>)dataNode).GetValue() : (char)dataNode.Value));
                    break;
                case TypeCode.Byte:
                    this.value = XmlConvert.ToString(isTypedNode ? ((DataNode<byte>)dataNode).GetValue() : (byte)dataNode.Value);
                    break;
                case TypeCode.Int16:
                    this.value = XmlConvert.ToString(isTypedNode ? ((DataNode<short>)dataNode).GetValue() : (short)dataNode.Value);
                    break;
                case TypeCode.Int32:
                    this.value = XmlConvert.ToString(isTypedNode ? ((DataNode<int>)dataNode).GetValue() : (int)dataNode.Value);
                    break;
                case TypeCode.Int64:
                    this.value = XmlConvert.ToString(isTypedNode ? ((DataNode<long>)dataNode).GetValue() : (long)dataNode.Value);
                    break;
                case TypeCode.Single:
                    this.value = XmlConvert.ToString(isTypedNode ? ((DataNode<float>)dataNode).GetValue() : (float)dataNode.Value);
                    break;
                case TypeCode.Double:
                    this.value = XmlConvert.ToString(isTypedNode ? ((DataNode<double>)dataNode).GetValue() : (double)dataNode.Value);
                    break;
                case TypeCode.Decimal:
                    this.value = XmlConvert.ToString(isTypedNode ? ((DataNode<decimal>)dataNode).GetValue() : (decimal)dataNode.Value);
                    break;
                case TypeCode.DateTime:
                    DateTime dateTime = isTypedNode ? ((DataNode<DateTime>)dataNode).GetValue() : (DateTime)dataNode.Value;
                    this.value = dateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffffffK", DateTimeFormatInfo.InvariantInfo);
                    break;
                case TypeCode.String:
                    this.value = isTypedNode ? ((DataNode<string>)dataNode).GetValue() : (string)dataNode.Value;
                    break;
                case TypeCode.SByte:
                    this.value = XmlConvert.ToString(isTypedNode ? ((DataNode<sbyte>)dataNode).GetValue() : (sbyte)dataNode.Value);
                    break;
                case TypeCode.UInt16:
                    this.value = XmlConvert.ToString(isTypedNode ? ((DataNode<ushort>)dataNode).GetValue() : (ushort)dataNode.Value);
                    break;
                case TypeCode.UInt32:
                    this.value = XmlConvert.ToString(isTypedNode ? ((DataNode<uint>)dataNode).GetValue() : (uint)dataNode.Value);
                    break;
                case TypeCode.UInt64:
                    this.value = XmlConvert.ToString(isTypedNode ? ((DataNode<ulong>)dataNode).GetValue() : (ulong)dataNode.Value);
                    break;
                case TypeCode.Object:
                default:
                    if (type == Globals.TypeOfByteArray)
                    {
                        byte[] bytes = isTypedNode ? ((DataNode<byte[]>)dataNode).GetValue() : (byte[])dataNode.Value;
                        this.value = (bytes == null) ? String.Empty : Convert.ToBase64String(bytes);
                    }
                    else if (type == Globals.TypeOfTimeSpan)
                        this.value = XmlConvert.ToString(isTypedNode ? ((DataNode<TimeSpan>)dataNode).GetValue() : (TimeSpan)dataNode.Value);
                    else if (type == Globals.TypeOfGuid)
                    {
                        Guid guid = isTypedNode ? ((DataNode<Guid>)dataNode).GetValue() : (Guid)dataNode.Value;
                        this.value = guid.ToString();
                    }
                    else if (type == Globals.TypeOfUri)
                    {
                        Uri uri = isTypedNode ? ((DataNode<Uri>)dataNode).GetValue() : (Uri)dataNode.Value;
                        this.value = uri.GetComponents(UriComponents.SerializationInfoString, UriFormat.UriEscaped);
                    }
                    else
                        handled = false;
                    break;
            }

            if (handled)
                this.internalNodeType = ExtensionDataNodeType.Text;
            return handled;
        }

        void PushElement()
        {
            GrowElementsIfNeeded();
            elements[depth++] = this.element;
            if (nextElement == null)
                element = GetNextElement();
            else
            {
                element = nextElement;
                nextElement = null;
            }
        }

        void PopElement()
        {
            this.prefix = element.prefix;
            this.localName = element.localName;
            this.ns = element.ns;

            if (depth == 0)
                return;

            this.depth--;

            if (elements != null)
            {
                this.element = elements[depth];
            }
        }

        void GrowElementsIfNeeded()
        {
            if (elements == null)
                elements = new ElementData[8];
            else if (elements.Length == depth)
            {
                ElementData[] newElements = new ElementData[elements.Length * 2];
                Array.Copy(elements, 0, newElements, 0, elements.Length);
                elements = newElements;
            }
        }

        ElementData GetNextElement()
        {
            int nextDepth = depth + 1;
            return (elements == null || elements.Length <= nextDepth || elements[nextDepth] == null)
                ? new ElementData() : elements[nextDepth];
        }

        [Fx.Tag.SecurityNote(Critical = "Accesses SecurityCritical static cache to look up prefix given a namespace .",
            Safe = "Read only access.")]
        [SecuritySafeCritical]
        internal static string GetPrefix(string ns)
        {
            string prefix;
            ns = ns ?? String.Empty;
            if (!nsToPrefixTable.TryGetValue(ns, out prefix))
            {
                lock (nsToPrefixTable)
                {
                    if (!nsToPrefixTable.TryGetValue(ns, out prefix))
                    {
                        prefix = (ns == null || ns.Length == 0) ? String.Empty : "p" + nsToPrefixTable.Count;
                        AddPrefix(prefix, ns);
                    }
                }
            }
            return prefix;
        }

        [Fx.Tag.SecurityNote(Critical = "Accesses SecurityCritical static cache to look up prefix given a namespace .",
            Safe = "Read only access.")]
        [SecuritySafeCritical]
        static void AddPrefix(string prefix, string ns)
        {
            nsToPrefixTable.Add(ns, prefix);
            prefixToNsTable.Add(prefix, ns);
        }
    }

#if USE_REFEMIT
    public class AttributeData
#else
    internal class AttributeData
#endif
    {
        public string prefix;
        public string ns;
        public string localName;
        public string value;
    }

#if USE_REFEMIT
    public class ElementData
#else
    internal class ElementData
#endif
    {
        public string localName;
        public string ns;
        public string prefix;
        public int attributeCount;
        public AttributeData[] attributes;
        public IDataNode dataNode;
        public int childElementIndex;

        public void AddAttribute(string prefix, string ns, string name, string value)
        {
            GrowAttributesIfNeeded();
            AttributeData attribute = attributes[attributeCount];
            if (attribute == null)
                attributes[attributeCount] = attribute = new AttributeData();
            attribute.prefix = prefix;
            attribute.ns = ns;
            attribute.localName = name;
            attribute.value = value;
            attributeCount++;
        }

        void GrowAttributesIfNeeded()
        {
            if (attributes == null)
                attributes = new AttributeData[4];
            else if (attributes.Length == attributeCount)
            {
                AttributeData[] newAttributes = new AttributeData[attributes.Length * 2];
                Array.Copy(attributes, 0, newAttributes, 0, attributes.Length);
                attributes = newAttributes;
            }
        }
    }

}


