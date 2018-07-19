//------------------------------------------------------------------------------
// <copyright file="SiteMapDataSourceView.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------
namespace System.Web.UI.WebControls {

    using System.Collections;
    using System.Web;
    using System.Web.UI;

    public class SiteMapDataSourceView : DataSourceView {

        private SiteMapNodeCollection _collection;
        private SiteMapDataSource _owner;


        public SiteMapDataSourceView(SiteMapDataSource owner, string name, SiteMapNode node) : base(owner, name) {
            _owner = owner;
            _collection = new SiteMapNodeCollection(node);
        }


        public SiteMapDataSourceView(SiteMapDataSource owner, string name, SiteMapNodeCollection collection) : base(owner, name) {
            _owner = owner;
            _collection = collection;
        }


        protected internal override IEnumerable ExecuteSelect(DataSourceSelectArguments arguments) {
            arguments.RaiseUnsupportedCapabilitiesError(this);
            return _collection;
        }

        protected override void OnDataSourceViewChanged(EventArgs e) {
            _collection = _owner.GetPathNodeCollection(Name);
            base.OnDataSourceViewChanged(e);
        }

        public IEnumerable Select(DataSourceSelectArguments arguments) {
            return ExecuteSelect(arguments);
        }
    }
}
