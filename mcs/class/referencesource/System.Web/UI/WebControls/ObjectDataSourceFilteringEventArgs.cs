//------------------------------------------------------------------------------
// <copyright file="ObjectDataSourceFilteringEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.Collections.Specialized;
    using System.ComponentModel;

    /// <devdoc>
    /// Event arguments for the ObjectDataSource Filter event.
    /// </devdoc>
    public class ObjectDataSourceFilteringEventArgs : CancelEventArgs {

        private IOrderedDictionary _parameterValues;

        public ObjectDataSourceFilteringEventArgs(IOrderedDictionary parameterValues) {
            _parameterValues = parameterValues;
        }

        public IOrderedDictionary ParameterValues {
            get {
                return _parameterValues;
            }
        }
    }
}

