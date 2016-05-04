//------------------------------------------------------------------------------
// <copyright file="LinqDataSourceUpdateEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {
    using System;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;

    public class LinqDataSourceUpdateEventArgs : CancelEventArgs {

        private LinqDataSourceValidationException _exception;
        private bool _exceptionHandled;
        private object _originalObject;
        private object _newObject;

        [SuppressMessage("Microsoft.Naming", "CA1720:AvoidTypeNamesInParameters",
            Justification = "Names are consistent with those used in the ObjectDataSource classes")]
        public LinqDataSourceUpdateEventArgs(object originalObject, object newObject) {
            _originalObject = originalObject;
            _newObject = newObject;
        }

        public LinqDataSourceUpdateEventArgs(LinqDataSourceValidationException exception) {
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

        public object NewObject {
            get {
                return _newObject;
            }
        }

    }
}

