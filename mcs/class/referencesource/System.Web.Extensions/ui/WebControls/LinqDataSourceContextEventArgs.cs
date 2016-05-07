//------------------------------------------------------------------------------
// <copyright file="LinqDataSourceContextEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {
    using System;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;

    public class LinqDataSourceContextEventArgs : EventArgs {

        private object _objectInstance;
        private DataSourceOperation _operation;

        public LinqDataSourceContextEventArgs() {
            _operation = DataSourceOperation.Select;
        }

        public LinqDataSourceContextEventArgs(DataSourceOperation operation) {
            _operation = operation;
        }

        public object ObjectInstance {
            get {
                return _objectInstance;
            }
            set {
                _objectInstance = value;
            }
        }

        public DataSourceOperation Operation {
            get {
                return _operation;
            }
        }

    }
}

