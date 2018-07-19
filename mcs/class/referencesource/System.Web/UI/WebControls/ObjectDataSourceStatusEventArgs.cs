//------------------------------------------------------------------------------
// <copyright file="ObjectDataSourceStatusEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.Collections;


    /// <devdoc>
    /// Represents data that is passed into an ObjectDataSourceMethodExecutedEventHandler delegate.
    /// </devdoc>
    public class ObjectDataSourceStatusEventArgs : EventArgs {

        private object _returnValue;
        private IDictionary _outputParameters;
        private Exception _exception;
        private bool _exceptionHandled;
        private int _affectedRows = -1;


        /// <devdoc>
        /// Creates a new instance of ObjectDataSourceStatusEventArgs.
        /// </devdoc>
        public ObjectDataSourceStatusEventArgs(object returnValue, IDictionary outputParameters) : this(returnValue, outputParameters, null) {
        }

        /// <devdoc>
        /// Creates a new instance of ObjectDataSourceStatusEventArgs.
        /// </devdoc>
        public ObjectDataSourceStatusEventArgs(object returnValue, IDictionary outputParameters, Exception exception) : base() {
            _returnValue = returnValue;
            _outputParameters = outputParameters;
            _exception = exception;
        }


        /// <devdoc>
        /// The output parameters of the method invocation.
        /// </devdoc>
        public IDictionary OutputParameters {
            get {
                return _outputParameters;
            }
        }

        /// <devdoc>
        /// If an exception was thrown by the invoked method, this property will contain the exception.
        /// If there was no exception, the value will be null.
        /// </devdoc>
        public Exception Exception {
            get {
                return _exception;
            }
        }

        /// <devdoc>
        /// If you wish to handle the exception using your own logic, set this value to true for it to be ignored by the control.
        /// If an exception was thrown and this value remains false, the exception will be re-thrown by the control.
        /// </devdoc>
        public bool ExceptionHandled {
            get {
                return _exceptionHandled;
            }
            set {
                _exceptionHandled = value;
            }
        }

        /// <devdoc>
        /// The return value of the method invocation.
        /// </devdoc>
        public object ReturnValue {
            get {
                return _returnValue;
            }
        }

        /// <devdoc>
        /// The number of rows affected by the operation.
        /// The default value is -1, which means that an unknown number
        /// of rows were affected. The user must set this value in the
        /// Deleted/Inserted/Updated/Selected event in order for the value
        /// to be available elsewhere. Typically the value would come either
        /// from the return value of the method or one of the output parameters.
        /// </devdoc>
        public int AffectedRows {
            get {
                return _affectedRows;
            }
            set {
                _affectedRows = value;
            }
        }
    }
}

