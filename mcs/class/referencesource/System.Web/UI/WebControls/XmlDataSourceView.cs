//------------------------------------------------------------------------------
// <copyright file="XmlDataSourceView.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Data;
    using System.Diagnostics;
    using System.Drawing.Design;
    using System.Text;
    using System.Web;
    using System.Web.UI;
    using System.Xml;


    /// <devdoc>
    /// </devdoc>
    public sealed class XmlDataSourceView : DataSourceView {

        private XmlDataSource _owner;


        /// <devdoc>
        /// Creates a new instance of XmlDataSourceView.
        /// </devdoc>
        public XmlDataSourceView(XmlDataSource owner, string name) : base(owner, name) {
            _owner = owner;
        }


        /// <devdoc>
        /// Returns all the rows of the datasource.
        /// </devdoc>
        protected internal override IEnumerable ExecuteSelect(DataSourceSelectArguments arguments) {
            arguments.RaiseUnsupportedCapabilitiesError(this);


            XmlNode root = _owner.GetXmlDocument();

            XmlNodeList nodes = null;
            if (_owner.XPath.Length != 0) {
                // If an XPath is specified on the control, use it
                nodes = root.SelectNodes(_owner.XPath);
            }
            else {
                // Otherwise, get all the children of the root
                nodes = root.SelectNodes("/node()/node()");
            }

            return new XmlDataSourceNodeDescriptorEnumeration(nodes);
        }

        public IEnumerable Select(DataSourceSelectArguments arguments) {
            return ExecuteSelect(arguments);
        }


        private class XmlDataSourceNodeDescriptorEnumeration : ICollection {
            private XmlNodeList _nodes;
            private int _count = -1; // -1 indicates we have not yet calculated the count

            public XmlDataSourceNodeDescriptorEnumeration(XmlNodeList nodes) {
                Debug.Assert(nodes != null, "Did not expect null node list");
                _nodes = nodes;
            }

            IEnumerator IEnumerable.GetEnumerator() {
                foreach (XmlNode node in _nodes) {
                    if (node.NodeType == XmlNodeType.Element) {
                        yield return new XmlDataSourceNodeDescriptor(node);
                    }
                }
            }

            int ICollection.Count {
                get {
                    if (_count == -1) {
                        // If the count has not yet been set, calculate the element count
                        _count = 0;
                        foreach (XmlNode node in _nodes) {
                            if (node.NodeType == XmlNodeType.Element) {
                                _count++;
                            }
                        }
                    }
                    return _count;
                }
            }

            bool ICollection.IsSynchronized {
                get {
                    return false;
                }
            }

            object ICollection.SyncRoot {
                get {
                    return null;
                }
            }

            void ICollection.CopyTo(Array array, int index) {
                for (IEnumerator e = ((IEnumerable)this).GetEnumerator(); e.MoveNext(); ) {
                    array.SetValue(e.Current, index++);
                }
            }
        }
    }
}
