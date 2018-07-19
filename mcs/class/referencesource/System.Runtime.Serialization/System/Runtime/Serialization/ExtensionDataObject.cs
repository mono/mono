//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.Runtime.Serialization
{
    using System;
    using System.Xml;
    using System.Collections.Generic;
    using System.Globalization;

    public sealed class ExtensionDataObject
    {
        IList<ExtensionDataMember> members;

#if USE_REFEMIT
        public ExtensionDataObject()
#else
        internal ExtensionDataObject()
#endif
        {
        }

#if USE_REFEMIT
        public IList<ExtensionDataMember> Members
#else
        internal IList<ExtensionDataMember> Members
#endif
        {
            get { return members; }
            set { members = value; }
        }
    }

#if USE_REFEMIT
    public class ExtensionDataMember
#else
    internal class ExtensionDataMember
#endif
    {
        string name;
        string ns;
        IDataNode value;
        int memberIndex;
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public string Namespace
        {
            get { return ns; }
            set { ns = value; }
        }

        public IDataNode Value
        {
            get { return value; }
            set { this.value = value; }
        }

        public int MemberIndex
        {
            get { return memberIndex; }
            set { memberIndex = value; }
        }

    }

#if USE_REFEMIT
    public interface IDataNode
#else
    internal interface IDataNode
#endif
    {
        Type DataType { get; }
        object Value { get; set; }  // boxes for primitives
        string DataContractName { get; set; }
        string DataContractNamespace { get; set; }
        string ClrTypeName { get; set; }
        string ClrAssemblyName { get; set; }
        string Id { get; set; }
        bool PreservesReferences { get; }

        // NOTE: consider moving below APIs to DataNode<T> if IDataNode API is made public
        void GetData(ElementData element);
        bool IsFinalValue { get; set; }
        void Clear();
    }

    internal class DataNode<T> : IDataNode
    {
        protected Type dataType;
        T value;
        string dataContractName;
        string dataContractNamespace;
        string clrTypeName;
        string clrAssemblyName;
        string id = Globals.NewObjectId;
        bool isFinalValue;

        internal DataNode()
        {
            this.dataType = typeof(T);
            this.isFinalValue = true;
        }

        internal DataNode(T value)
            : this()
        {
            this.value = value;
        }

        public Type DataType
        {
            get { return dataType; }
        }

        public object Value
        {
            get { return value; }
            set { this.value = (T)value; }
        }

        bool IDataNode.IsFinalValue
        {
            get { return isFinalValue; }
            set { isFinalValue = value; }
        }

        public T GetValue()
        {
            return value;
        }

#if NotUsed
        public void SetValue(T value)
        {
            this.value = value;
        }
#endif

        public string DataContractName
        {
            get { return dataContractName; }
            set { dataContractName = value; }
        }

        public string DataContractNamespace
        {
            get { return dataContractNamespace; }
            set { dataContractNamespace = value; }
        }

        public string ClrTypeName
        {
            get { return clrTypeName; }
            set { clrTypeName = value; }
        }

        public string ClrAssemblyName
        {
            get { return clrAssemblyName; }
            set { clrAssemblyName = value; }
        }

        public bool PreservesReferences
        {
            get { return (Id != Globals.NewObjectId); }
        }

        public string Id
        {
            get { return id; }
            set { id = value; }
        }

        public virtual void GetData(ElementData element)
        {
            element.dataNode = this;
            element.attributeCount = 0;
            element.childElementIndex = 0;

            if (DataContractName != null)
                AddQualifiedNameAttribute(element, Globals.XsiPrefix, Globals.XsiTypeLocalName, Globals.SchemaInstanceNamespace, DataContractName, DataContractNamespace);
            if (ClrTypeName != null)
                element.AddAttribute(Globals.SerPrefix, Globals.SerializationNamespace, Globals.ClrTypeLocalName, ClrTypeName);
            if (ClrAssemblyName != null)
                element.AddAttribute(Globals.SerPrefix, Globals.SerializationNamespace, Globals.ClrAssemblyLocalName, ClrAssemblyName);
        }

        public virtual void Clear()
        {
            // dataContractName not cleared because it is used when re-serializing from unknown data
            clrTypeName = clrAssemblyName = null;
        }

        internal void AddQualifiedNameAttribute(ElementData element, string elementPrefix, string elementName, string elementNs, string valueName, string valueNs)
        {
            string prefix = ExtensionDataReader.GetPrefix(valueNs);
            element.AddAttribute(elementPrefix, elementNs, elementName, String.Format(CultureInfo.InvariantCulture, "{0}:{1}", prefix, valueName));

            bool prefixDeclaredOnElement = false;
            if (element.attributes != null)
            {
                for (int i = 0; i < element.attributes.Length; i++)
                {
                    AttributeData attribute = element.attributes[i];
                    if (attribute != null && attribute.prefix == Globals.XmlnsPrefix && attribute.localName == prefix)
                    {
                        prefixDeclaredOnElement = true;
                        break;
                    }
                }
            }
            if (!prefixDeclaredOnElement)
                element.AddAttribute(Globals.XmlnsPrefix, Globals.XmlnsNamespace, prefix, valueNs);
        }
    }

    internal class ClassDataNode : DataNode<object>
    {
        IList<ExtensionDataMember> members;

        internal ClassDataNode()
        {
            dataType = Globals.TypeOfClassDataNode;
        }

        internal IList<ExtensionDataMember> Members
        {
            get { return members; }
            set { members = value; }
        }

        public override void Clear()
        {
            base.Clear();
            members = null;
        }
    }

    internal class CollectionDataNode : DataNode<Array>
    {
        IList<IDataNode> items;
        string itemName;
        string itemNamespace;
        int size = -1;

        internal CollectionDataNode()
        {
            dataType = Globals.TypeOfCollectionDataNode;
        }

        internal IList<IDataNode> Items
        {
            get { return items; }
            set { items = value; }
        }

        internal string ItemName
        {
            get { return itemName; }
            set { itemName = value; }
        }

        internal string ItemNamespace
        {
            get { return itemNamespace; }
            set { itemNamespace = value; }
        }

        internal int Size
        {
            get { return size; }
            set { size = value; }
        }

        public override void GetData(ElementData element)
        {
            base.GetData(element);

            element.AddAttribute(Globals.SerPrefix, Globals.SerializationNamespace, Globals.ArraySizeLocalName, Size.ToString(NumberFormatInfo.InvariantInfo));
        }

        public override void Clear()
        {
            base.Clear();
            items = null;
            size = -1;
        }
    }

    internal class XmlDataNode : DataNode<object>
    {
        IList<XmlAttribute> xmlAttributes;
        IList<XmlNode> xmlChildNodes;
        XmlDocument ownerDocument;

        internal XmlDataNode()
        {
            dataType = Globals.TypeOfXmlDataNode;
        }

        internal IList<XmlAttribute> XmlAttributes
        {
            get { return xmlAttributes; }
            set { xmlAttributes = value; }
        }

        internal IList<XmlNode> XmlChildNodes
        {
            get { return xmlChildNodes; }
            set { xmlChildNodes = value; }
        }

        internal XmlDocument OwnerDocument
        {
            get { return ownerDocument; }
            set { ownerDocument = value; }
        }

        public override void Clear()
        {
            base.Clear();
            xmlAttributes = null;
            xmlChildNodes = null;
            ownerDocument = null;
        }
    }

    internal class ISerializableDataNode : DataNode<object>
    {
        string factoryTypeName;
        string factoryTypeNamespace;
        IList<ISerializableDataMember> members;

        internal ISerializableDataNode()
        {
            dataType = Globals.TypeOfISerializableDataNode;
        }

        internal string FactoryTypeName
        {
            get { return factoryTypeName; }
            set { factoryTypeName = value; }
        }

        internal string FactoryTypeNamespace
        {
            get { return factoryTypeNamespace; }
            set { factoryTypeNamespace = value; }
        }

        internal IList<ISerializableDataMember> Members
        {
            get { return members; }
            set { members = value; }
        }

        public override void GetData(ElementData element)
        {
            base.GetData(element);

            if (FactoryTypeName != null)
                AddQualifiedNameAttribute(element, Globals.SerPrefix, Globals.ISerializableFactoryTypeLocalName, Globals.SerializationNamespace, FactoryTypeName, FactoryTypeNamespace);
        }

        public override void Clear()
        {
            base.Clear();
            members = null;
            factoryTypeName = factoryTypeNamespace = null;
        }
    }

    internal class ISerializableDataMember
    {
        string name;
        IDataNode value;

        internal string Name
        {
            get { return name; }
            set { name = value; }
        }

        internal IDataNode Value
        {
            get { return value; }
            set { this.value = value; }
        }
    }

}
