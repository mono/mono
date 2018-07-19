//------------------------------------------------------------------------------
// <copyright file="XmlHierarchyData.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Data;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Text;
    using System.Web;
    using System.Web.UI;
    using System.Xml;

    using AttributeCollection = System.ComponentModel.AttributeCollection;


    /// <devdoc>
    /// Represents the data associated with a single hierarchy item - in this case an XmlNode.
    /// </devdoc>
    internal sealed class XmlHierarchyData : IHierarchyData, ICustomTypeDescriptor {

        private XmlNode _item;
        private XmlHierarchicalEnumerable _parent;
        private string _path;

        /// <devdoc>
        /// Create a new instance of XmlHierarchyData.
        /// </devdoc>
        internal XmlHierarchyData(XmlHierarchicalEnumerable parent, XmlNode item) {
            _parent = parent;
            _item = item;
        }


        /// <devdoc>
        /// Creates a path to a given XmlNode from the root using XPath.
        /// </devdoc>
        private string CreateRecursivePath(XmlNode node) {
            if (node.ParentNode == null)
                return String.Empty;

            return CreateRecursivePath(node.ParentNode) + FindNodePosition(node);
        }

        /// <devdoc>
        /// Finds a node's position relative to its parent.
        /// </devdoc>
        private string FindNodePosition(XmlNode node) {
            XmlNodeList nodeList = node.ParentNode.ChildNodes;

            int index = 0;

            for (int i = 0; i < nodeList.Count; i++) {
                // Position only considers elements, not other node types
                if (nodeList[i].NodeType == XmlNodeType.Element)
                    index++;

                if (nodeList[i] == node)
                    return "/*[position()=" + Convert.ToString(index, CultureInfo.InvariantCulture) + "]";
            }

            throw new ArgumentException(SR.GetString(SR.XmlHierarchyData_CouldNotFindNode));
        }

        public override string ToString() {
            return _item.Name;
        }


        bool IHierarchyData.HasChildren {
            get {
                return _item.HasChildNodes;
            }
        }

        object IHierarchyData.Item {
            get {
                return _item;
            }
        }

        string IHierarchyData.Path {
            get {
                if (_path == null) {
                    // If we don't yet have a path, create one and cache it
                    if (_parent != null) {
                        // If we have a parent enumerable, then our path is the parent's
                        // path plus our position within that parent.
                        if (_parent.Path == null) {
                            _parent.Path = CreateRecursivePath(_item.ParentNode);
                        }
                        _path = _parent.Path + FindNodePosition(_item);
                    }
                    else {
                        // If we are not associated with a parent enumerable, we
                        // have to build up our entire path from scratch.
                        _path = CreateRecursivePath(_item);
                    }
                }
                return _path;
            }
        }

        string IHierarchyData.Type {
            get {
                return _item.Name;
            }
        }

        IHierarchicalEnumerable IHierarchyData.GetChildren() {
            return new XmlHierarchicalEnumerable(_item.ChildNodes);
        }

        IHierarchyData IHierarchyData.GetParent() {
            XmlNode parentNode = _item.ParentNode;
            if (parentNode == null)
                return null;

            return new XmlHierarchyData(null, parentNode);
        }

        AttributeCollection ICustomTypeDescriptor.GetAttributes() {
            return AttributeCollection.Empty;
        }

        string ICustomTypeDescriptor.GetClassName() {
            return GetType().Name;
        }

        string ICustomTypeDescriptor.GetComponentName() {
            return null;

        }

        TypeConverter ICustomTypeDescriptor.GetConverter() {
            return null;
        }

        EventDescriptor ICustomTypeDescriptor.GetDefaultEvent() {
            return null;
        }

        PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty() {
            return new XmlHierarchyDataPropertyDescriptor("#Name");
        }

        object ICustomTypeDescriptor.GetEditor(Type editorBaseType) {
            return null;
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents() {
            return null;
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attrs) {
            return null;
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties() {
            return ((ICustomTypeDescriptor)this).GetProperties(null);
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attrFilter) {
            System.Collections.Generic.List<PropertyDescriptor> list = new System.Collections.Generic.List<PropertyDescriptor>();
            list.Add(new XmlHierarchyDataPropertyDescriptor("#Name"));
            list.Add(new XmlHierarchyDataPropertyDescriptor("#Value"));
            list.Add(new XmlHierarchyDataPropertyDescriptor("#InnerText"));

            XmlAttributeCollection attrs = _item.Attributes;
            if (attrs != null) {
                for (int i = 0; i < attrs.Count; i++) {
                    list.Add(new XmlHierarchyDataPropertyDescriptor(attrs[i].Name));
                }
            }

            return new PropertyDescriptorCollection(list.ToArray());
        }

        object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd) {
            if (pd is XmlHierarchyDataPropertyDescriptor) {
                return this;
            }

            return null;
        }

        private class XmlHierarchyDataPropertyDescriptor : PropertyDescriptor {
            private string _name;

            public XmlHierarchyDataPropertyDescriptor(string name) : base(name, null) {
                _name = name;
            }


            public override Type ComponentType {
                get {
                    return typeof(XmlHierarchyData);
                }
            }

            public override bool IsReadOnly {
                get {
                    return true;
                }
            }

            public override Type PropertyType {
                get {
                    return typeof(string);
                }
            }

            public override bool CanResetValue(object o) {
                return false;
            }

            public override object GetValue(object o) {
                XmlHierarchyData data = o as XmlHierarchyData;
                if (data != null) {
                    switch (_name) {
                        case "#Name":
                            return data._item.Name;
                        case "#Value":
                            return data._item.Value;
                        case "#InnerText":
                            return data._item.InnerText;
                        default:
                            XmlAttributeCollection attrs = data._item.Attributes;

                            if (attrs != null) {
                                XmlAttribute attr = attrs[_name];
                                if (attr != null) {
                                    return attr.Value;
                                }
                            }
                            break;
                    }
                }

                return String.Empty;
            }

            public override void ResetValue(object o) {
                return;
            }

            public override void SetValue(object o, object value) {
            }

            public override bool ShouldSerializeValue(object o) {
                return true;
            }
        }
    }
}

