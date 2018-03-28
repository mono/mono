//------------------------------------------------------------------------------
// <copyright file="AccessDataSourceView.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Data;
    using System.Data.OleDb;
    using System.Drawing.Design;
    using System.IO;
    using System.Text;
    using System.Web.UI;
    using System.Web.Util;

    public class AccessDataSourceView : SqlDataSourceView {
        private AccessDataSource _owner;


        /// <devdoc>
        /// Creates a new instance of AccessDataSourceView.
        /// </devdoc>
        public AccessDataSourceView(AccessDataSource owner, string name, HttpContext context) : base(owner, name, context) {
            Debug.Assert(owner != null);
            _owner = owner;
        }



        /// <devdoc>
        /// Returns all the rows of the datasource.
        /// </devdoc>
        protected internal override IEnumerable ExecuteSelect(DataSourceSelectArguments arguments) {
            if (String.IsNullOrEmpty(_owner.DataFile)) {
                throw new InvalidOperationException(SR.GetString(SR.AccessDataSourceView_SelectRequiresDataFile, _owner.ID));
            }
            return base.ExecuteSelect(arguments);
        }
    }
}

