//------------------------------------------------------------------------------
// <copyright file="SqlDataSourceFilteringEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.Collections.Specialized;
    using System.ComponentModel;

    /// <devdoc>
    /// Event arguments for the SqlDataSource Filter event.
    /// </devdoc>
    public class SqlDataSourceFilteringEventArgs : CancelEventArgs {

        private IOrderedDictionary _parameterValues;

        public SqlDataSourceFilteringEventArgs(IOrderedDictionary parameterValues) {
            _parameterValues = parameterValues;
        }

        public IOrderedDictionary ParameterValues {
            get {
                return _parameterValues;
            }
        }
    }
}

