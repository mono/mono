//------------------------------------------------------------------------------
// <copyright file="LinqDataSourceDeleteEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {
    using System;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;

    public class LinqDataSourceDeleteEventArgs : CancelEventArgs {

        private LinqDataSourceValidationException _exception;
        private bool _exceptionHandled;
        private object _originalObject;

        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "object",
            Justification = "Names are consistent with those used in the ObjectDataSource classes")]
        public LinqDataSourceDeleteEventArgs(object originalObject) {
            _originalObject = originalObject;
        }

        public LinqDataSourceDeleteEventArgs(LinqDataSourceValidationException exception) {
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

        public object OriginalObject {
            get {
                return _originalObject;
            }
        }

    }
}

