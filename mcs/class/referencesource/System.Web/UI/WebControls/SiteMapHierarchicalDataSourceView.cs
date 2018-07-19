//------------------------------------------------------------------------------
// <copyright file="SiteMapHierarchicalDataSourceView.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------
namespace System.Web.UI.WebControls {

    using System.Collections;
    using System.Web;
    using System.Web.UI;

    public class SiteMapHierarchicalDataSourceView : HierarchicalDataSourceView {

        private SiteMapNodeCollection _collection;


        public SiteMapHierarchicalDataSourceView(SiteMapNode node) {
            _collection = new SiteMapNodeCollection(node);
        }


        public SiteMapHierarchicalDataSourceView(SiteMapNodeCollection collection) {
            _collection = collection;
        }


        public override IHierarchicalEnumerable Select() {
            return _collection;
        }
    }
}
