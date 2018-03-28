//------------------------------------------------------------------------------
// <copyright file="XmlHierarchicalEnumerable.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Data;
    using System.Drawing.Design;
    using System.Text;
    using System.Web;
    using System.Web.UI;
    using System.Xml;



    /// <devdoc>
    /// An enumerable representing a single level of an XmlHierarchicalDataSourceView.
    /// </devdoc>
    internal sealed class XmlHierarchicalEnumerable : IHierarchicalEnumerable {

        private string _path;
        private XmlNodeList _nodeList;

        /// <devdoc>
        /// Creates a new instance of XmlHierarchicalEnumerable.
        /// </devdoc>
        internal XmlHierarchicalEnumerable(XmlNodeList nodeList) {
            _nodeList = nodeList;
        }

        internal string Path {
            get {
                return _path;
            }
            set {
                _path = value;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            foreach (XmlNode node in _nodeList) {
                if (node.NodeType == XmlNodeType.Element) {
                    yield return new XmlHierarchyData(this, node);
                }
            }
        }

        IHierarchyData IHierarchicalEnumerable.GetHierarchyData(object enumeratedItem) {
            return (IHierarchyData)enumeratedItem;
        }
    }
}

