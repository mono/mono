//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System;

    /// <summary>
    /// Event Argument that is created when an Trust Request Fault is raised.
    /// </summary>
    public class WSTrustRequestProcessingErrorEventArgs : EventArgs
    {
        Exception _exception;
        string _requestType;

        /// <summary>
        /// Creates an instance of this Event Argument.
        /// </summary>
        /// <param name="requestType">The Trust Request Type that failed.</param>
        /// <param name="exception">The exception happend during this Request.</param>
        public WSTrustRequestProcessingErrorEventArgs( string requestType, Exception exception )
        {
            _exception = exception;
            _requestType = requestType;
        }

        /// <summary>
        /// Gets the Exception thrown.
        /// </summary>
        public Exception Exception
        {
            get { return _exception; }
        }

        /// <summary>
        /// Gets the Request Type that failed.
        /// </summary>
        public string RequestType
        {
            get { return _requestType; }
        }
    }
}
