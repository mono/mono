//------------------------------------------------------------------------------
// <copyright file="ReadOnlyHierarchicalDataSourceView.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.Collections;
    using System.Web;
    using System.Web.UI;
    using System.Web.Util;


    /// <devdoc>
    /// This class is used by ReadOnlyHierarchicalDataSource to represent an
    /// individual view of a generic hierarchical data source.
    /// </devdoc>
    internal sealed class ReadOnlyHierarchicalDataSourceView : HierarchicalDataSourceView {

        private IHierarchicalEnumerable _dataSource;

        public ReadOnlyHierarchicalDataSourceView(IHierarchicalEnumerable dataSource) {
            _dataSource = dataSource;
        }

        public override IHierarchicalEnumerable Select() {
            return _dataSource;
        }
    }
}

