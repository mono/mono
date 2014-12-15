//------------------------------------------------------------------------------
// <copyright file="filewebrequest.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net {
    using System.IO;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Threading;
    using System.Runtime.Versioning;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Tracing;

    [Serializable]
    public class FileWebRequest : WebRequest, ISerializable {

        private static WaitCallback s_GetRequestStreamCallback = new WaitCallback(GetRequestStreamCallback);
        private static WaitCallback s_GetResponseCallback = new WaitCallback(GetResponseCallback);
        private static ContextCallback s_WrappedGetRequestStreamCallback = new ContextCallback(GetRequestStreamCallback);
        private static ContextCallback s_WrappedResponseCallback = new ContextCallback(GetResponseCallback);

    // fields

        string m_connectionGroupName;
        long m_contentLength;
        ICredentials m_credentials;
        FileAccess m_fileAccess;
        WebHeaderCollection m_headers;
        string m_method = "GET";
        bool m_preauthenticate;
        IWebProxy m_proxy;
        ManualResetEvent m_readerEvent;
        bool m_readPending;
        WebResponse m_response;
        Stream m_stream;
        bool m_syncHint;
        int m_timeout = WebRequest.DefaultTimeout;
        Uri m_uri;
        bool m_writePending;
        bool m_writing;
        private LazyAsyncResult m_WriteAResult;
        private LazyAsyncResult m_ReadAResult;
        private int                m_Aborted;

    // constructors

         internal FileWebRequest(Uri uri)
         {
             if ((object)uri.Scheme != (object)Uri.UriSchemeFile)
                 throw new ArgumentOutOfRangeException("uri");

            m_uri = uri;
            m_fileAccess = FileAccess.Read;
            m_headers = new WebHeaderCollection(WebHeaderCollectionType.FileWebRequest);
        }


        //
        // ISerializable constructor
        //

        [Obsolete("Serialization is obsoleted for this type. http://go.microsoft.com/fwlink/?linkid=14202")]
        protected FileWebRequest(SerializationInfo serializationInfo, StreamingContext streamingContext):base(serializationInfo, streamingContext) {
            m_headers               = (WebHeaderCollection)serializationInfo.GetValue("headers", typeof(WebHeaderCollection));
            m_proxy                 = (IWebProxy)serializationInfo.GetValue("proxy", typeof(IWebProxy));
            m_uri                   = (Uri)serializationInfo.GetValue("uri", typeof(Uri));
            m_connectionGroupName   = serializationInfo.GetString("connectionGroupName");
            m_method                = serializationInfo.GetString("method");
            m_contentLength         = serializationInfo.GetInt64("contentLength");
            m_timeout               = serializationInfo.GetInt32("timeout");
            m_fileAccess            = (FileAccess )serializationInfo.GetInt32("fileAccess");
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
            serializationInfo.AddValue("proxy", m_proxy, typeof(IWebProxy));
            serializationInfo.AddValue("uri", m_uri, typeof(Uri));
            serializationInfo.AddValue("connectionGroupName", m_connectionGroupName);
            serializationInfo.AddValue("method", m_method);
            serializationInfo.AddValue("contentLength", m_contentLength);
            serializationInfo.AddValue("timeout", m_timeout);
            serializationInfo.AddValue("fileAccess", m_fileAccess);

            //we're leaving this for legacy.  V1.1 and V1.0 had this field in the serialization constructor
            serializationInfo.AddValue("preauthenticate", false);
            base.GetObjectData(serializationInfo, streamingContext);
        }


    // properties

        internal bool Aborted {
            get {
                return m_Aborted != 0;
            }
        }

        public override string ConnectionGroupName {
            get {
                return m_connectionGroupName;
            }
            set {
                m_connectionGroupName = value;
            }
        }

        public override long ContentLength {
            get {
                return m_contentLength;
            }
            set {
                if (value < 0) {
                    throw new ArgumentException(SR.GetString(SR.net_clsmall), "value");
                }
                m_contentLength = value;
            }
        }

        public override string ContentType {
            get {
                return m_headers["Content-Type"];
            }
            set {
                m_headers["Content-Type"] = value;
            }
        }

        public override ICredentials Credentials {
            get {
                return m_credentials;
            }
            set {
                m_credentials = value;
            }
        }

        public override WebHeaderCollection Headers {
            get {
                return m_headers;
            }
        }

        public override string Method {
            get {
                return m_method;
            }
            set {
                if (ValidationHelper.IsBlankString(value)) {
                    throw new ArgumentException(SR.GetString(SR.net_badmethod), "value");
                }
                m_method = value;
            }
        }

        public override bool PreAuthenticate {
            get {
                return m_preauthenticate;
            }
            set {
                m_preauthenticate = true;
            }
        }

        public override IWebProxy Proxy {
            get {
                return m_proxy;
            }
            set {
                m_proxy = value;
            }
        }

        //UEUE changed default from infinite to 100 seconds
        public override int Timeout {
            get {
                return m_timeout;
            }
            set {
                if ((value < 0) && (value != System.Threading.Timeout.Infinite)) {
                    throw new ArgumentOutOfRangeException("value", SR.GetString(SR.net_io_timeout_use_ge_zero));
                }
                m_timeout = value;
            }
        }

        public override Uri RequestUri {
            get {
                return m_uri;
            }
        }

    // methods

        [HostProtection(ExternalThreading=true)]
        public override IAsyncResult BeginGetRequestStream(AsyncCallback callback, object state)
        {
            GlobalLog.Enter("FileWebRequest::BeginGetRequestStream");

            try {
                if (Aborted)
                    throw ExceptionHelper.RequestAbortedException;
                if (!CanGetRequestStream()) {
                    Exception e = new ProtocolViolationException(SR.GetString(SR.net_nouploadonget));
                    GlobalLog.LeaveException("FileWebRequest::BeginGetRequestStream", e);
                    throw e;
                }
                if (m_response != null) {
                    Exception e = new InvalidOperationException(SR.GetString(SR.net_reqsubmitted));
                    GlobalLog.LeaveException("FileWebRequest::BeginGetRequestStream", e);
                    throw e;
                }
                lock(this) {
                    if (m_writePending) {
                        Exception e = new InvalidOperationException(SR.GetString(SR.net_repcall));
                        GlobalLog.LeaveException("FileWebRequest::BeginGetRequestStream", e);
                        throw e;
                    }
                    m_writePending = true;
                }
                                                    
                //we need to force the capture of the identity and context to make sure the
                //posted callback doesn't inavertently gain access to something it shouldn't.
                m_ReadAResult = new LazyAsyncResult(this, state, callback);
                ThreadPool.QueueUserWorkItem(s_GetRequestStreamCallback, m_ReadAResult);
            } catch (Exception exception) {
                if(Logging.On)Logging.Exception(Logging.Web, this, "BeginGetRequestStream", exception);
                throw;
            } finally {
                GlobalLog.Leave("FileWebRequest::BeginGetRequestSteam");
            }

            string suri;
            if (FrameworkEventSource.Log.IsEnabled(EventLevel.Verbose, FrameworkEventSource.Keywords.NetClient))
                suri = this.RequestUri.ToString();
            else
                suri = this.RequestUri.OriginalString;
            if (FrameworkEventSource.Log.IsEnabled()) LogBeginGetRequestStream(suri);

            return m_ReadAResult;
        }

        [HostProtection(ExternalThreading=true)]
        public override IAsyncResult BeginGetResponse(AsyncCallback callback, object state)
        {
            GlobalLog.Enter("FileWebRequest::BeginGetResponse");

            try {
                if (Aborted)
                    throw ExceptionHelper.RequestAbortedException;
                lock(this) {
                    if (m_readPending) {
                        Exception e = new InvalidOperationException(SR.GetString(SR.net_repcall));
                        GlobalLog.LeaveException("FileWebRequest::BeginGetResponse", e);
                        throw e;
                    }
                    m_readPending = true;
                }

                m_WriteAResult = new LazyAsyncResult(this,state,callback);
                ThreadPool.QueueUserWorkItem(s_GetResponseCallback,m_WriteAResult);
            } catch (Exception exception) {
                if(Logging.On)Logging.Exception(Logging.Web, this, "BeginGetResponse", exception);
                throw;
            } finally {
                GlobalLog.Leave("FileWebRequest::BeginGetResponse");
            }

            string suri;
            if (FrameworkEventSource.Log.IsEnabled(EventLevel.Verbose, FrameworkEventSource.Keywords.NetClient))
                suri = this.RequestUri.ToString();
            else
                suri = this.RequestUri.OriginalString;
            if (FrameworkEventSource.Log.IsEnabled()) LogBeginGetResponse(suri);

            return m_WriteAResult;
        }

        private bool CanGetRequestStream() {
            return !KnownHttpVerb.Parse(m_method).ContentBodyNotAllowed;
        }

        public override Stream EndGetRequestStream(IAsyncResult asyncResult)
        {
            GlobalLog.Enter("FileWebRequest::EndGetRequestStream");

            Stream stream;
            try {
                LazyAsyncResult  ar = asyncResult as LazyAsyncResult;
                if (asyncResult == null || ar == null) {
                    Exception e = asyncResult == null? new ArgumentNullException("asyncResult"): new ArgumentException(SR.GetString(SR.InvalidAsyncResult), "asyncResult");
                    GlobalLog.LeaveException("FileWebRequest::EndGetRequestStream", e);
                    throw e;
                }

                object result = ar.InternalWaitForCompletion();
                if(result is Exception){
                    throw (Exception)result;
                }
                stream = (Stream) result;
                m_writePending = false;
            } catch (Exception exception) {
                if(Logging.On)Logging.Exception(Logging.Web, this, "EndGetRequestStream", exception);
                throw;
            } finally {
                GlobalLog.Leave("FileWebRequest::EndGetRequestStream");
            }

            if (FrameworkEventSource.Log.IsEnabled()) LogEndGetRequestStream();

            return stream;
        }

        public override WebResponse EndGetResponse(IAsyncResult asyncResult)
        {
            GlobalLog.Enter("FileWebRequest::EndGetResponse");

            WebResponse response;
            try {
                LazyAsyncResult  ar = asyncResult as LazyAsyncResult;
                if (asyncResult == null || ar == null) {
                    Exception e = asyncResult == null? new ArgumentNullException("asyncResult"): new ArgumentException(SR.GetString(SR.InvalidAsyncResult), "asyncResult");
                    GlobalLog.LeaveException("FileWebRequest::EndGetRequestStream", e);
                    throw e;
                }


                object result = ar.InternalWaitForCompletion();
                if(result is Exception){
                    throw (Exception)result;
                }
                response = (WebResponse) result;
                m_readPending = false;
            } catch (Exception exception) {
                if(Logging.On)Logging.Exception(Logging.Web, this, "EndGetResponse", exception);
                throw;
            } finally {
                GlobalLog.Leave("FileWebRequest::EndGetResponse");
            }

            if (FrameworkEventSource.Log.IsEnabled()) LogEndGetResponse();

            return response;
        }

        public override Stream GetRequestStream()
        {
            GlobalLog.Enter("FileWebRequest::GetRequestStream");

            IAsyncResult result;

            try {
                result = BeginGetRequestStream(null, null);

                if ((Timeout != System.Threading.Timeout.Infinite) && !result.IsCompleted) {
                    if (!result.AsyncWaitHandle.WaitOne(Timeout, false) || !result.IsCompleted) {
                        if (m_stream != null) {
                            m_stream.Close();
                        }
                        Exception e = new WebException(NetRes.GetWebStatusString(WebExceptionStatus.Timeout), WebExceptionStatus.Timeout);
                        GlobalLog.LeaveException("FileWebRequest::GetRequestStream", e);
                        throw e;
                    }
                }
            } catch (Exception exception) {
                if(Logging.On)Logging.Exception(Logging.Web, this, "GetRequestStream", exception);
                throw;
            } finally {
                GlobalLog.Leave("FileWebRequest::GetRequestStream");
            }
            return EndGetRequestStream(result);
        }

        public override WebResponse GetResponse() {
            GlobalLog.Enter("FileWebRequest::GetResponse");

            m_syncHint = true;

            IAsyncResult result;

            try {
                result = BeginGetResponse(null, null);

                if ((Timeout != System.Threading.Timeout.Infinite) && !result.IsCompleted) {
                    if (!result.AsyncWaitHandle.WaitOne(Timeout, false) || !result.IsCompleted) {
                        if (m_response != null) {
                            m_response.Close();
                        }
                        Exception e = new WebException(NetRes.GetWebStatusString(WebExceptionStatus.Timeout), WebExceptionStatus.Timeout);
                        GlobalLog.LeaveException("FileWebRequest::GetResponse", e);
                        throw e;
                    }
                }
            } catch (Exception exception) {
                if(Logging.On)Logging.Exception(Logging.Web, this, "GetResponse", exception);
                throw;
            } finally {
                GlobalLog.Leave("FileWebRequest::GetResponse");
            }
            return EndGetResponse(result);
        }

        private static void GetRequestStreamCallback(object state)
        {
            GlobalLog.Enter("FileWebRequest::GetRequestStreamCallback");
            LazyAsyncResult asyncResult = (LazyAsyncResult) state;
            FileWebRequest request = (FileWebRequest)asyncResult.AsyncObject;

            try
            {
                if (request.m_stream == null)
                {
                    request.m_stream = new FileWebStream(request, request.m_uri.LocalPath, FileMode.Create, FileAccess.Write, FileShare.Read);
                    request.m_fileAccess = FileAccess.Write;
                    request.m_writing = true;
                }
            }
            catch (Exception e)
            {
                // any exceptions previously thrown must be passed to the callback
                Exception ex = new WebException(e.Message, e);
                GlobalLog.LeaveException("FileWebRequest::GetRequestStreamCallback", ex);

                // if the callback throws, correct behavior is to crash the process
                asyncResult.InvokeCallback(ex);
                return;
            }

            // if the callback throws, correct behavior is to crash the process
            asyncResult.InvokeCallback(request.m_stream);
            GlobalLog.Leave("FileWebRequest::GetRequestStreamCallback");
        }

        private static void GetResponseCallback(object state)
        {
            GlobalLog.Enter("FileWebRequest::GetResponseCallback");
            LazyAsyncResult asyncResult = (LazyAsyncResult) state;
            FileWebRequest request = (FileWebRequest)asyncResult.AsyncObject;

            if (request.m_writePending || request.m_writing) {
                lock(request) {
                    if (request.m_writePending || request.m_writing) {
                        request.m_readerEvent = new ManualResetEvent(false);
                    }
                }
            }
            if (request.m_readerEvent != null)
                request.m_readerEvent.WaitOne();

            try
            {
                if (request.m_response == null)
                    request.m_response = new FileWebResponse(request, request.m_uri, request.m_fileAccess, !request.m_syncHint);               
            }
            catch (Exception e)
            {
                // any exceptions previously thrown must be passed to the callback
                Exception ex = new WebException(e.Message, e);
                GlobalLog.LeaveException("FileWebRequest::GetResponseCallback", ex);

                // if the callback throws, correct behavior is to crash the process
                asyncResult.InvokeCallback(ex);
                return;
            }

            // if the callback throws, the correct behavior is to crash the process
            asyncResult.InvokeCallback(request.m_response);
            GlobalLog.Leave("FileWebRequest::GetResponseCallback");
        }

        internal void UnblockReader() {
            GlobalLog.Enter("FileWebRequest::UnblockReader");
            lock(this) {
                if (m_readerEvent != null) {
                    m_readerEvent.Set();
                }
            }
            m_writing = false;
            GlobalLog.Leave("FileWebRequest::UnblockReader");
        }

        // NOT SUPPORTED method
        public override bool UseDefaultCredentials  {
            get {
                throw ExceptionHelper.PropertyNotSupportedException;
            }
            set {
                throw ExceptionHelper.PropertyNotSupportedException;
            }
        }

        public override void Abort()
        {
            GlobalLog.Enter("FileWebRequest::Abort");
            if(Logging.On)Logging.PrintWarning(Logging.Web, NetRes.GetWebStatusString("net_requestaborted", WebExceptionStatus.RequestCanceled));
            try {
                if (Interlocked.Increment(ref m_Aborted) == 1)
                {
                    LazyAsyncResult readAResult = m_ReadAResult;
                    LazyAsyncResult writeAResult = m_WriteAResult;

                    WebException webException = new WebException(NetRes.GetWebStatusString("net_requestaborted", WebExceptionStatus.RequestCanceled), WebExceptionStatus.RequestCanceled);

                    Stream requestStream = m_stream;

                    if (readAResult != null && !readAResult.IsCompleted)
                        readAResult.InvokeCallback(webException);
                    if (writeAResult != null && !writeAResult.IsCompleted)
                        writeAResult.InvokeCallback(webException);

                    if (requestStream != null)
                        if (requestStream is ICloseEx)
                            ((ICloseEx)requestStream).CloseEx(CloseExState.Abort);
                        else
                            requestStream.Close();

                    if (m_response != null)
                        ((ICloseEx)m_response).CloseEx(CloseExState.Abort);
                }
            } catch (Exception exception) {
                if(Logging.On)Logging.Exception(Logging.Web, this, "Abort", exception);
                throw;
            } finally {
                GlobalLog.Leave("FileWebRequest::Abort");
            }
        }
    }

    internal class FileWebRequestCreator : IWebRequestCreate {

        internal FileWebRequestCreator() {
        }

        public WebRequest Create(Uri uri) {
            return new FileWebRequest(uri);
        }
    }

    internal sealed class FileWebStream : FileStream, ICloseEx {

        FileWebRequest m_request;

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public FileWebStream(FileWebRequest request, string path, FileMode mode, FileAccess access, FileShare sharing)
                 : base(path, mode, access, sharing)
        {
            GlobalLog.Enter("FileWebStream::FileWebStream");
            m_request = request;
            GlobalLog.Leave("FileWebStream::FileWebStream");
        }

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public FileWebStream(FileWebRequest request, string path, FileMode mode, FileAccess access, FileShare sharing, int length, bool async)
                : base(path, mode, access, sharing, length, async)
        {
            GlobalLog.Enter("FileWebStream::FileWebStream");
            m_request = request;
            GlobalLog.Leave("FileWebStream::FileWebStream");
        }

        protected override void Dispose(bool disposing) {
            GlobalLog.Enter("FileWebStream::Close");
            try {
                if (disposing && m_request != null) {
                    m_request.UnblockReader();
                }
            }
            finally {
                base.Dispose(disposing);
            }
            GlobalLog.Leave("FileWebStream::Close");
        }

        void ICloseEx.CloseEx(CloseExState closeState) {
            if ((closeState & CloseExState.Abort) != 0)
                SafeFileHandle.Close();
            else
                Close();
        }

        public override int Read(byte[] buffer, int offset, int size) {
            CheckError();
            try {
                return base.Read(buffer, offset, size);
            }
            catch {
                CheckError();
                throw;
            }
        }

        public override void Write(byte[] buffer, int offset, int size) {
            CheckError();
            try {
                base.Write(buffer, offset, size);
            }
            catch {
                CheckError();
                throw;
            }
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int size, AsyncCallback callback, Object state) {
            CheckError();
            try {
                return base.BeginRead(buffer, offset, size, callback, state);
            } 
            catch {
                CheckError();
                throw;
            }
        }

        public override int EndRead(IAsyncResult ar) {
            try {
                return base.EndRead(ar);
            }
            catch {
                CheckError();
                throw;
            }
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int size, AsyncCallback callback, Object state) {
            CheckError();
            try {
                return base.BeginWrite(buffer, offset, size, callback, state);
            } 
            catch {
                CheckError();
                throw;
            }
        }

        public override void EndWrite(IAsyncResult ar) {
            try {
                base.EndWrite(ar);
            }
            catch {
                CheckError();
                throw;
            }
        }

        private void CheckError() {
            if (m_request.Aborted) {
                throw new WebException(
                              NetRes.GetWebStatusString(
                                  "net_requestaborted", 
                                  WebExceptionStatus.RequestCanceled),
                              WebExceptionStatus.RequestCanceled);
            }    
        }
    }
}
