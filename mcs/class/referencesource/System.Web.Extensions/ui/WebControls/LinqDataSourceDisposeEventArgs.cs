//------------------------------------------------------------------------------
// <copyright file="LinqDataSourceDisposeEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {
    using System;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;

    public class LinqDataSourceDisposeEventArgs : CancelEventArgs {

        private object _objectInstance;

        public LinqDataSourceDisposeEventArgs(object instance) {
            _objectInstance = instance;
        }

        public object ObjectInstance {
            get {
                return _objectInstance;
            }
        }

    }
}

