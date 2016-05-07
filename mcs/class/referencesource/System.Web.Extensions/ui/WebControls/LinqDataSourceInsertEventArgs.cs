//------------------------------------------------------------------------------
// <copyright file="LinqDataSourceInsertEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {
    using System;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;

    public class LinqDataSourceInsertEventArgs : CancelEventArgs {

        private LinqDataSourceValidationException _exception;
        private bool _exceptionHandled;
        private object _newObject;

        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "object",
            Justification = "Names are consistent with those used in the ObjectDataSource classes")]
        public LinqDataSourceInsertEventArgs(object newObject) {
            _newObject = newObject;
        }

        public LinqDataSourceInsertEventArgs(LinqDataSourceValidationException exception) {
            _exception = exception;
        }

        public LinqDataSourceValidationException Exception {
            get {
                return _exception;
            }
        }

        public bool ExceptionHandled {
            get {
                return _exceptionHandled;
            }
            set {
                _exceptionHandled = value;
            }
        }

        public object NewObject {
            get {
                return _newObject;
            }
        }

    }
}

