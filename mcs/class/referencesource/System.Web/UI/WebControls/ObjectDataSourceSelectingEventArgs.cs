//------------------------------------------------------------------------------
// <copyright file="ObjectDataSourceSelectingEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.Collections.Specialized;

    /// <devdoc>
    /// Represents data that is passed into an ObjectDataSourceSelectingEventHandler delegate.
    /// </devdoc>
    public class ObjectDataSourceSelectingEventArgs : ObjectDataSourceMethodEventArgs {

        private DataSourceSelectArguments _arguments;
        private bool _executingSelectCount;


        /// <devdoc>
        /// Creates a new instance of ObjectDataSourceSelectingEventArgs.
        /// </devdoc>
        public ObjectDataSourceSelectingEventArgs(IOrderedDictionary inputParameters, DataSourceSelectArguments arguments, bool executingSelectCount) : base(inputParameters) {
            _arguments = arguments;
            _executingSelectCount = executingSelectCount;
        }

        public DataSourceSelectArguments Arguments {
            get {
                return _arguments;
            }
        }

        public bool ExecutingSelectCount {
            get {
                return _executingSelectCount;
            }
        }
    }
}

