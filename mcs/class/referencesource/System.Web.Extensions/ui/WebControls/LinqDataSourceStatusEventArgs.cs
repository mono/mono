//------------------------------------------------------------------------------
// <copyright file="LinqDataSourceStatusEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {
    using System;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;

    public class LinqDataSourceStatusEventArgs : EventArgs {

        private Exception _exception;
        private bool _exceptionHandled;
        private object _result;
        private int _totalRowCount = -1;

        public LinqDataSourceStatusEventArgs(object result) {
            _result = result;
        }

        public LinqDataSourceStatusEventArgs(object result, int totalRowCount) {
            _result = result;
            _totalRowCount = totalRowCount;
        }

        public LinqDataSourceStatusEventArgs(Exception exception) {
            _exception = exception;
        }

        public Exception Exception {
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

        public object Result {
            get {
                return _result;
            }
        }

        public int TotalRowCount {
            get {
                return _totalRowCount;
            }
        }

    }
}

