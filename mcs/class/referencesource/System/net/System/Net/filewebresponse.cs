//------------------------------------------------------------------------------
// <copyright file="filewebresponse.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net {
    using System.Runtime.Serialization;
    using System.IO;
    using System.Globalization;
    using System.Security.Permissions;
    using System.Diagnostics.CodeAnalysis;

    [Serializable]
    public class FileWebResponse : WebResponse, ISerializable, ICloseEx
    {
        const int DefaultFileStreamBufferSize = 8192;
        const string DefaultFileContentType = "application/octet-stream";

    // fields

        bool m_closed;
        long m_contentLength;
        FileAccess m_fileAccess;
        WebHeaderCollection m_headers;
        Stream m_stream;
        Uri m_uri;

    // constructors

        internal FileWebResponse(FileWebRequest request, Uri uri, FileAccess access, bool asyncHint) {
            GlobalLog.Enter("FileWebResponse::FileWebResponse", "uri="+uri+", access="+access+", asyncHint="+asyncHint);
            try {
                m_fileAccess = access;
                if (access == FileAccess.Write) {
                    m_stream = Stream.Null;
                } else {

                    //
                    // apparently, specifying async when the stream will be read
                    // synchronously, or vice versa, can lead to a 10x perf hit.
                    // While we don't know how the app will read the stream, we
                    // use the hint from whether the app called BeginGetResponse
                    // or GetResponse to supply the async flag to the stream ctor
                    //

                    m_stream = new FileWebStream(request,
                                                 uri.LocalPath,
                                                 FileMode.Open,
                                                 FileAccess.Read,
                                                 FileShare.Read,
                                                 DefaultFileStreamBufferSize,
                                                 asyncHint
                                                 );
                    m_contentLength = m_stream.Length;
                }
                m_headers = new WebHeaderCollection(WebHeaderCollectionType.FileWebResponse);
                m_headers.AddInternal(HttpKnownHeaderNames.ContentLength, m_contentLength.ToString(NumberFormatInfo.InvariantInfo));
                m_headers.AddInternal(HttpKnownHeaderNames.ContentType, DefaultFileContentType);
                m_uri = uri;
            } catch (Exception e) {
                Exception ex = new WebException(e.Message, e, WebExceptionStatus.ConnectFailure, null);
                GlobalLog.LeaveException("FileWebResponse::FileWebResponse", ex);
                throw ex;
            }            
            GlobalLog.Leave("FileWebResponse::FileWebResponse");
        }

        //
        // ISerializable constructor
        //
        [Obsolete("Serialization is obsoleted for this type. http://go.microsoft.com/fwlink/?linkid=14202")]
        protected FileWebResponse(SerializationInfo serializationInfo, StreamingContext streamingContext):base(serializationInfo, streamingContext) {
            m_headers       = (WebHeaderCollection)serializationInfo.GetValue("headers", typeof(WebHeaderCollection));
            m_uri           = (Uri)serializationInfo.GetValue("uri", typeof(Uri));
            m_contentLength = serializationInfo.GetInt64("contentLength");
            m_fileAccess    = (FileAccess )serializationInfo.GetInt32("fileAccess");
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
        // FxCop: provide some way for derived classes to access GetObjectData even if the derived class
        // explicitly re-inherits ISerializable.
        //
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        protected override void GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
            serializationInfo.AddValue("headers", m_headers, typeof(WebHeaderCollection));
            serializationInfo.AddValue("uri", m_uri, typeof(Uri));
            serializationInfo.AddValue("contentLength", m_contentLength);
            serializationInfo.AddValue("fileAccess", m_fileAccess);
            base.GetObjectData(serializationInfo, streamingContext);
        }

    // properties

        public override long ContentLength {
            get {
                CheckDisposed();
                return m_contentLength;
            }
        }

        public override string ContentType {
            get {
                CheckDisposed();
                return DefaultFileContentType;
            }
        }

        public override WebHeaderCollection Headers {
            get {
                CheckDisposed();
                return m_headers;
            }
        }
        
        // For portability only
        public override bool SupportsHeaders {
            get {
                return true;
            }
        }

        public override Uri ResponseUri {
            get {
                CheckDisposed();
                return m_uri;
            }
        }

    // methods

        private void CheckDisposed() {
            if (m_closed) {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
        }

        public override void Close() {
            ((ICloseEx)this).CloseEx(CloseExState.Normal);
        }

        void ICloseEx.CloseEx(CloseExState closeState) {
            GlobalLog.Enter("FileWebResponse::Close()");
            try {
                if (!m_closed) {
                    m_closed = true;

                    Stream chkStream = m_stream;
                    if (chkStream!=null) {
                        if (chkStream is ICloseEx)
                            ((ICloseEx)chkStream).CloseEx(closeState);
                        else
                            chkStream.Close();
                        m_stream = null;
                    }
                }
            }
            finally {
                GlobalLog.Leave("FileWebResponse::Close()");
            }
        }

        public override Stream GetResponseStream() {
            GlobalLog.Enter("FileWebResponse::GetResponseStream()");
            try {
                CheckDisposed();
            }
            finally {
                GlobalLog.Leave("FileWebResponse::GetResponseStream()");
            }
            return m_stream;
        }
    }
}
