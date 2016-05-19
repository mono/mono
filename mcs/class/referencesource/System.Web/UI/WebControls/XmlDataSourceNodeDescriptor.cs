//------------------------------------------------------------------------------
// <copyright file="XmlDataSourceNodeDescriptor.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Data;
    using System.Drawing.Design;
    using System.Security.Permissions;
    using System.Text;
    using System.Web;
    using System.Web.UI;
    using System.Web.Util;
    using System.Xml;
    using System.Xml.XPath;

    using AttributeCollection = System.ComponentModel.AttributeCollection;

    /// <devdoc>
    /// </devdoc>
    internal sealed class XmlDataSourceNodeDescriptor : ICustomTypeDescriptor, IXPathNavigable {

        private XmlNode _node;


        /// <devdoc>
        /// Creates a new instance of XmlDataSourceView.
        /// </devdoc>
        public XmlDataSourceNodeDescriptor(XmlNode node) {
            Debug.Assert(node != null, "Did not expect null node");
            _node = node;
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
            return null;
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
            XmlAttributeCollection attrs = _node.Attributes;
            if (attrs != null) {
                for (int i = 0; i < attrs.Count; i++) {
                    list.Add(new XmlDataSourcePropertyDescriptor(attrs[i].Name));
                }
            }

            return new PropertyDescriptorCollection(list.ToArray());
        }

        object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd) {
            if (pd is XmlDataSourcePropertyDescriptor) {
                return this;
            }

            return null;
        }

        XPathNavigator IXPathNavigable.CreateNavigator() {
            return _node.CreateNavigator();
        }


        private class XmlDataSourcePropertyDescriptor : PropertyDescriptor {
            private string _name;

            public XmlDataSourcePropertyDescriptor(string name) : base(name, null) {
                _name = name;
            }


            public override Type ComponentType {
                get {
                    return typeof(XmlDataSourceNodeDescriptor);
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
                XmlDataSourceNodeDescriptor node = o as XmlDataSourceNodeDescriptor;
                if (node != null) {
                    XmlAttributeCollection attrs = node._node.Attributes;

                    if (attrs != null) {
                        XmlAttribute attr = attrs[_name];
                        if (attr != null) {
                            return attr.Value;
                        }
                    }
                }

                return String.Empty;
            }

            public override void ResetValue(object o) {
            }

            public override void SetValue(object o, object value) {
            }

            public override bool ShouldSerializeValue(object o) {
                return true;
            }
        }
    }
}

