//------------------------------------------------------------------------------
// <copyright file="WebException.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------


namespace System.Net {
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    /*++

    Abstract:

        Contains the defintion for the WebException object. This is a subclass of
        Exception that contains a WebExceptionStatus and possible a reference to a
        WebResponse.


    --*/



    /// <devdoc>
    ///    <para>
    ///       Provides network communication exceptions to the application.
    ///
    ///       This is the exception that is thrown by WebRequests when something untoward
    ///       happens. It's a subclass of WebException that contains a WebExceptionStatus and possibly
    ///       a reference to a WebResponse. The WebResponse is only present if we actually
    ///       have a response from the remote server.
    ///    </para>
    /// </devdoc>
    [Serializable]
    public class WebException : InvalidOperationException, ISerializable {

        private WebExceptionStatus m_Status = WebExceptionStatus.UnknownError; //Should be changed to GeneralFailure;
        private WebResponse m_Response;
        [NonSerialized]
        private WebExceptionInternalStatus m_InternalStatus = WebExceptionInternalStatus.RequestFatal;


        /// <devdoc>
        ///    <para>
        ///       Creates a new instance of the <see cref='System.Net.WebException'/>
        ///       class with the default status
        ///    <see langword='Error'/> from the
        ///    <see cref='System.Net.WebExceptionStatus'/> values.
        ///    </para>
        /// </devdoc>
        public WebException() {

        }


        /// <devdoc>
        ///    <para>
        ///       Creates a new instance of the <see cref='System.Net.WebException'/> class with the specified error
        ///       message.
        ///    </para>
        /// </devdoc>
        public WebException(string message) : this(message, null) {
        }


        /// <devdoc>
        ///    <para>
        ///       Creates a new instance of the <see cref='System.Net.WebException'/> class with the specified error
        ///       message and nested exception.
        ///
        ///           Message         - Message string for exception.
        ///           InnerException  - Exception that caused this exception.
        ///
        ///    </para>
        /// </devdoc>
        public WebException(string message, Exception innerException) :
            base(message, innerException) {
        }

        public WebException(string message, WebExceptionStatus status) :
            this(message, null, status, null) {
        }


        /// <devdoc>
        ///    <para>
        ///       Creates a new instance of the <see cref='System.Net.WebException'/> class with the specified error
        ///       message and status.
        ///
        ///           Message         - Message string for exception.
        ///           Status          - Network status of exception
        ///    </para>
        /// </devdoc>
        internal WebException(string message, WebExceptionStatus status, WebExceptionInternalStatus internalStatus, Exception innerException) :
            this(message, innerException, status, null, internalStatus) {
        }

        /// <devdoc>
        ///    <para>
        ///       Creates a new instance of the <see cref='System.Net.WebException'/> class with the specified error
        ///       message, nested exception, status and response.
        ///
        ///           Message         - Message string for exception.
        ///           InnerException  - The exception that caused this one.
        ///           Status          - Network status of exception
        ///           Response        - The WebResponse we have.
        ///    </para>
        /// </devdoc>
        public WebException(string message,
                            Exception innerException,
                            WebExceptionStatus status,
                            WebResponse response) :
            this(message, null, innerException, status, response)
        { }

        internal WebException(string message, string data, Exception innerException, WebExceptionStatus status, WebResponse response) :
            base(message + (data != null ? ": '" + data + "'" : ""), innerException)
        {
            m_Status = status;
            m_Response = response;
        }

        internal WebException(string message,
                            Exception innerException,
                            WebExceptionStatus status,
                            WebResponse response,
                            WebExceptionInternalStatus internalStatus) :
            this(message, null, innerException, status, response, internalStatus)
        { }

        internal WebException(string message, string data, Exception innerException, WebExceptionStatus status, WebResponse response, WebExceptionInternalStatus internalStatus) :
            base(message + (data != null ? ": '" + data + "'" : ""), innerException)
        {
            m_Status = status;
            m_Response = response;
            m_InternalStatus = internalStatus;
        }


        protected WebException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext) {
           // m_Status = (WebExceptionStatus)serializationInfo.GetInt32("Status");
           // m_InternalStatus = (WebExceptionInternalStatus)serializationInfo.GetInt32("InternalStatus");
        }

        /// <internalonly/>

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase", Justification = "System.dll is still using pre-v4 security model and needs this demand")]
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        void ISerializable.GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
            GetObjectData(serializationInfo, streamingContext);
        }


        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase", Justification = "System.dll is still using pre-v4 security model and needs this demand")]
        [SecurityPermissionAttribute(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.SerializationFormatter)] 		
        public override void GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext){
            base.GetObjectData(serializationInfo, streamingContext);
            //serializationInfo.AddValue("Status", (int)m_Status, typeof(int));
            //serializationInfo.AddValue("InternalStatus", (int)m_InternalStatus, typeof(int));
        }

        /// <devdoc>
        ///    <para>
        ///       Gets the status of the response.
        ///    </para>
        /// </devdoc>
        public WebExceptionStatus Status {
            get {
                return m_Status;
            }
        }


        /// <devdoc>
        ///    <para>
        ///       Gets the error message returned from the remote host.
        ///    </para>
        /// </devdoc>
        public WebResponse Response {
            get {
                return m_Response;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets the error message returned from the remote host.
        ///    </para>
        /// </devdoc>
        internal WebExceptionInternalStatus InternalStatus {
            get {
                return m_InternalStatus;
            }
        }

    }; // class WebException

    internal enum WebExceptionInternalStatus {
        RequestFatal      = 0,
        ServicePointFatal = 1,
        Recoverable       = 2,
        Isolated          = 3,
    }


} // namespace System.Net
