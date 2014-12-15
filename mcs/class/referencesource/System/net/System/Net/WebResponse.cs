//------------------------------------------------------------------------------
// <copyright file="WebResponse.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------


namespace System.Net {

    using System.Collections;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    /*++

        WebResponse - The abstract base class for all WebResponse objects.


    --*/

    /// <devdoc>
    ///    <para>
    ///       A
    ///       response from a Uniform Resource Indentifier (Uri). This is an abstract class.
    ///    </para>
    /// </devdoc>
    [Serializable]
    public abstract class WebResponse : MarshalByRefObject, ISerializable, IDisposable {
        private bool m_IsCacheFresh;
        private bool m_IsFromCache;

        /// <devdoc>
        ///    <para>Initializes a new
        ///       instance of the <see cref='System.Net.WebResponse'/>
        ///       class.</para>
        /// </devdoc>
        protected WebResponse() {
        }

        //
        // ISerializable constructor
        //
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected WebResponse(SerializationInfo serializationInfo, StreamingContext streamingContext) {
        }

        //
        // ISerializable method
        //
        /// <internalonly/>
        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase", Justification = "System.dll is still using pre-v4 security model and needs this demand")]
        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.SerializationFormatter, SerializationFormatter=true)]
        void ISerializable.GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
            GetObjectData(serializationInfo, streamingContext);
        }

        //
        // FxCop: provide a way for derived classes to access this method even if they reimplement ISerializable.
        //
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        protected virtual void GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
        }


        /*++

            Close - Closes the Response after the use.

            This causes the read stream to be closed.

        --*/

        public virtual void Close() {
            // We can't throw MethodNotImplementedException here because this is not listed in the 
            // Portable Libraries subset, so users there cannot override Close.
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (!disposing) {
                return;
            }
            try {   
                Close();
            }
            catch { }
        }

        public virtual bool IsFromCache {
            get {return m_IsFromCache;}
        }
        internal bool InternalSetFromCache {
            set {
                m_IsFromCache = value;
            }
        }

        internal virtual bool IsCacheFresh {
            get {return m_IsCacheFresh;}
        }
        internal bool InternalSetIsCacheFresh {
            set {
                m_IsCacheFresh = value;
            }
        }

        public virtual bool IsMutuallyAuthenticated {
            get {return false;}
        }


        /*++

            ContentLength - Content length of response.

            This property returns the content length of the response.

        --*/

        /// <devdoc>
        ///    <para>When overridden in a derived class, gets or
        ///       sets
        ///       the content length of data being received.</para>
        /// </devdoc>
        public virtual long ContentLength {
            get {
                throw ExceptionHelper.PropertyNotImplementedException;
            }
            set {
                throw ExceptionHelper.PropertyNotImplementedException;
            }
        }


        /*++

            ContentType - Content type of response.

            This property returns the content type of the response.

        --*/

        /// <devdoc>
        ///    <para>When overridden in a derived class,
        ///       gets
        ///       or sets the content type of the data being received.</para>
        /// </devdoc>
        public virtual string ContentType {
            get {
                throw ExceptionHelper.PropertyNotImplementedException;
            }
            set {
                throw ExceptionHelper.PropertyNotImplementedException;
            }
        }

        /*++

            ResponseStream  - Get the response stream for this response.

            This property returns the response stream for this WebResponse.

            Input: Nothing.

            Returns: Response stream for response.

                    read-only

        --*/

        /// <devdoc>
        /// <para>When overridden in a derived class, returns the <see cref='System.IO.Stream'/> object used
        ///    for reading data from the resource referenced in the <see cref='System.Net.WebRequest'/>
        ///    object.</para>
        /// </devdoc>
        public virtual Stream GetResponseStream() {
            Contract.Ensures(Contract.Result<Stream>() != null);
            throw ExceptionHelper.MethodNotImplementedException;
        }


        /*++

            ResponseUri  - Gets the final Response Uri, that includes any
             changes that may have transpired from the orginal request

            This property returns Uri for this WebResponse.

            Input: Nothing.

            Returns: Response Uri for response.

                    read-only

        --*/

        /// <devdoc>
        ///    <para>When overridden in a derived class, gets the Uri that
        ///       actually responded to the request.</para>
        /// </devdoc>
        public virtual Uri ResponseUri {
            // read-only
            get {
                throw ExceptionHelper.PropertyNotImplementedException;
            }
        }

        /*++

            Headers  - Gets any request specific headers associated
             with this request, this is simply a name/value pair collection

            Input: Nothing.

            Returns: This property returns WebHeaderCollection.

                    read-only

        --*/

        /// <devdoc>
        ///    <para>When overridden in a derived class, gets
        ///       a collection of header name-value pairs associated with this
        ///       request.</para>
        /// </devdoc>
        public virtual WebHeaderCollection Headers {
            // read-only
            get {
                Contract.Ensures(Contract.Result<WebHeaderCollection>() != null);
                throw ExceptionHelper.PropertyNotImplementedException;
            }
        }

        // For portability only
        // Returning false indicates that the Headers property has not been implemented and should not be used.
        // Derived types with headers should override both Headers and SupportsHeaders.
        public virtual bool SupportsHeaders {
            get {
                return false;
            }
        }

    }; // class WebResponse


} // namespace System.Net
