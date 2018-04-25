//------------------------------------------------------------------------------
// <copyright file="XmlHierarchicalDataSourceView.cs" company="Microsoft">
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
    using System.Web.Util;
    using System.Xml;



    /// <devdoc>
    /// Represents a hierarchical view of an XmlDataSource.
    /// </devdoc>
    public class XmlHierarchicalDataSourceView : HierarchicalDataSourceView {

        private XmlDataSource _owner;
        private string _viewPath;

        /// <devdoc>
        /// Creates a new instance of XmlHierarchicalDataSourceView.
        /// </devdoc>
        internal XmlHierarchicalDataSourceView(XmlDataSource owner, string viewPath) {
            Debug.Assert(owner != null);
            _owner = owner;
            _viewPath = viewPath;
        }



        public override IHierarchicalEnumerable Select() {
            XmlNode root = _owner.GetXmlDocument();

            XmlNodeList nodes = null;
            if (!String.IsNullOrEmpty(_viewPath)) {
                XmlNode node = root.SelectSingleNode(_viewPath);
                if (node != null) {
                    nodes = node.ChildNodes;
                }
            }
            else {
                if (_owner.XPath.Length > 0) {
                    nodes = root.SelectNodes(_owner.XPath);
                }
                else {
                    nodes = root.ChildNodes;
                }
            }

            return new XmlHierarchicalEnumerable(nodes);
        }
    }
}

