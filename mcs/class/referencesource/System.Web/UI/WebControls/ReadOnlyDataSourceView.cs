//------------------------------------------------------------------------------
// <copyright file="ReadOnlyDataSourceView.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.Collections;
    using System.Web.UI;
    using System.Web.Util;


    /// <devdoc>
    /// This class is used by ReadOnlyDataSource to represent an individual
    /// view of a generic data source.
    /// </devdoc>
    internal sealed class ReadOnlyDataSourceView : DataSourceView {

        private IEnumerable _dataSource;

        public ReadOnlyDataSourceView(ReadOnlyDataSource owner, string name, IEnumerable dataSource) : base(owner, name) {
            _dataSource = dataSource;
        }

        protected internal override IEnumerable ExecuteSelect(DataSourceSelectArguments arguments) {
            arguments.RaiseUnsupportedCapabilitiesError(this);
            return _dataSource;
        }
    }
}

