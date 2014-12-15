//------------------------------------------------------------------------------
// <copyright file="webclient.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net {
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Net.Cache;
    using System.Runtime.Versioning;
    using System.Diagnostics.CodeAnalysis;


    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [ComVisible(true)]
    public class WebClient : Component {

    // fields

        const int DefaultCopyBufferLength = 8192;
        const int DefaultDownloadBufferLength = 65536;
        const string DefaultUploadFileContentType = "application/octet-stream";
        const string UploadFileContentType = "multipart/form-data";
        const string UploadValuesContentType = "application/x-www-form-urlencoded";

        Uri m_baseAddress;
        ICredentials m_credentials;
        WebHeaderCollection m_headers;
        NameValueCollection m_requestParameters;
        WebResponse m_WebResponse;
        WebRequest  m_WebRequest;
        Encoding   m_Encoding = Encoding.Default;
        string m_Method;
        long m_ContentLength = -1;
        bool m_InitWebClientAsync;
        bool m_Cancelled;
        ProgressData m_Progress;
        IWebProxy m_Proxy;
        bool m_ProxySet;
        RequestCachePolicy m_CachePolicy;

    // constructors

        [SuppressMessage("Microsoft.Usage", "CA1816:CallGCSuppressFinalizeCorrectly", Scope = "member", 
            Target = "System.Net.WebClient.#.ctor()", Justification =
            "The base class finalizer is unnecessary and hurts performance.")]
        public WebClient() {
            // We don't know if derived types need finalizing, but WebClient doesn't.
            if (this.GetType() == typeof(WebClient)) {
                GC.SuppressFinalize(this);
            }
        }

        /// <devdoc>
        ///    <para>Sets up async delegates, we need to create these on every instance when async</para>
        /// </devdoc>
        private void InitWebClientAsync() {
            if (!m_InitWebClientAsync) {
                openReadOperationCompleted = new SendOrPostCallback(OpenReadOperationCompleted);
                openWriteOperationCompleted = new SendOrPostCallback(OpenWriteOperationCompleted);
                downloadStringOperationCompleted = new SendOrPostCallback(DownloadStringOperationCompleted);
                downloadDataOperationCompleted = new SendOrPostCallback(DownloadDataOperationCompleted);
                downloadFileOperationCompleted = new SendOrPostCallback(DownloadFileOperationCompleted);
                uploadStringOperationCompleted = new SendOrPostCallback(UploadStringOperationCompleted);
                uploadDataOperationCompleted = new SendOrPostCallback(UploadDataOperationCompleted);
                uploadFileOperationCompleted = new SendOrPostCallback(UploadFileOperationCompleted);
                uploadValuesOperationCompleted = new SendOrPostCallback(UploadValuesOperationCompleted);
                reportDownloadProgressChanged = new SendOrPostCallback(ReportDownloadProgressChanged);
                reportUploadProgressChanged = new SendOrPostCallback(ReportUploadProgressChanged);
                m_Progress = new ProgressData();
                m_InitWebClientAsync = true;
            }
        }

        /// <devdoc>
        ///    <para>Sets up shared properties, to prevent a previous request's state from interfering with this request
        ///     ASSUMED to be called at the start of each WebClient api</para>
        /// </devdoc>
        private void ClearWebClientState() {
            if (AnotherCallInProgress(Interlocked.Increment(ref m_CallNesting))) {
                CompleteWebClientState();
                throw new NotSupportedException(SR.GetString(SR.net_webclient_no_concurrent_io_allowed));
            }
            m_ContentLength = -1;
            m_WebResponse = null;
            m_WebRequest = null;
            m_Method = null;
            m_Cancelled = false;

            if (m_Progress != null)
                m_Progress.Reset();
        }

        /// <devdoc>
        ///    <para>Matching code for ClearWebClientState, MUST be matched with ClearWebClientState() calls</para>
        /// </devdoc>
        private void CompleteWebClientState() {
            Interlocked.Decrement(ref m_CallNesting);
        }


        #region designer support for System.Windows.dll
        //introduced for supporting design-time loading of System.Windows.dll
        [Obsolete("This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.", true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool AllowReadStreamBuffering { get; set; }

        //introduced for supporting design-time loading of System.Windows.dll
        [Obsolete("This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.", true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool AllowWriteStreamBuffering { get; set; }
        
        //introduced for supporting design-time loading of System.Windows.dll
        [Obsolete("This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.", true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public event WriteStreamClosedEventHandler WriteStreamClosed { add { } remove { } }
        
        //introduced for supporting design-time loading of System.Windows.dll
        [Obsolete("This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.", true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected virtual void OnWriteStreamClosed(WriteStreamClosedEventArgs e) { }
        #endregion

        // properties
        /// <devdoc>
        ///    <para>Sets the encoding type for converting string to byte[] on String based methods</para>
        /// </devdoc>
        public Encoding Encoding {
            get {
                return m_Encoding;
            }
            set {
                if (value==null) {
                    throw new ArgumentNullException("Encoding");
                }
                m_Encoding = value;
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string BaseAddress {
            get {
                return (m_baseAddress == null) ? String.Empty : m_baseAddress.ToString();
            }
            set {
                if ((value == null) || (value.Length == 0)) {
                    m_baseAddress = null;
                } else {
                    try {
                        m_baseAddress = new Uri(value);
                    }
                    catch (UriFormatException e) {
                        throw new ArgumentException(SR.GetString(SR.net_webclient_invalid_baseaddress), "value", e);
                    }
                }
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public ICredentials Credentials {
            get {
                return m_credentials;
            }
            set {
                m_credentials = value;
            }
        }

        /// <devdoc>
        ///    <para>Sets Credentials to CredentialCache.DefaultCredentials</para>
        /// </devdoc>
        public bool UseDefaultCredentials  {
            get {
                return (m_credentials is SystemNetworkCredential) ? true : false;
            }
            set {
                m_credentials = value ? CredentialCache.DefaultCredentials : null;
            }

        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public WebHeaderCollection Headers {
            get {
                if (m_headers == null) {
                    m_headers = new WebHeaderCollection(WebHeaderCollectionType.WebRequest);
                }
                return m_headers;
            }
            set {
                m_headers = value;
            }
        }

        public NameValueCollection QueryString {
            get {
                if (m_requestParameters == null) {
                    m_requestParameters = new NameValueCollection();
                }
                return m_requestParameters;
            }
            set {
                m_requestParameters = value;
            }
        }

        public WebHeaderCollection ResponseHeaders {
            get {
                if (m_WebResponse != null) {
                    return m_WebResponse.Headers;
                }
                return null;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the proxy information for a request.
        ///    </para>
        /// </devdoc>
        public IWebProxy Proxy {
            get {
                ExceptionHelper.WebPermissionUnrestricted.Demand();
                if (!m_ProxySet) {
                    return WebRequest.InternalDefaultWebProxy;
                } else {
                    return m_Proxy;
                }
            }
            set {
                ExceptionHelper.WebPermissionUnrestricted.Demand();
                m_Proxy = value;
                m_ProxySet = true;
            }
        }

        public RequestCachePolicy CachePolicy {
            get {
                return m_CachePolicy;
            }
            set {
                m_CachePolicy = value;
            }
        }
        /// <devdoc>
        ///    <para>
        ///       Indicates if the request is still in progress
        ///    </para>
        /// </devdoc>
        public bool IsBusy {
            get {
                return m_AsyncOp != null;
            }

        }

        // methods

        /// <devdoc>
        ///    <para>Creates the WebRequest</para>
        /// </devdoc>
        protected virtual WebRequest GetWebRequest(Uri address) {
            WebRequest request = WebRequest.Create(address);
            CopyHeadersTo(request);
            if (Credentials != null) {
                request.Credentials = Credentials;
            }
            if (m_Method != null) {
                request.Method = m_Method;
            }
            if (m_ContentLength != -1) {
                request.ContentLength = m_ContentLength;
            }
            if (m_ProxySet) {
                request.Proxy = m_Proxy;
            }
            if (m_CachePolicy != null)
            {
                request.CachePolicy = m_CachePolicy;
            }
            return request;
        }

        /// <devdoc>
        ///    <para>Retrieves a WebResponse by calling GetResponse()</para>
        /// </devdoc>
        protected virtual WebResponse GetWebResponse(WebRequest request) {
            WebResponse response = request.GetResponse();
            m_WebResponse = response;
            return response;
        }

        /// <devdoc>
        ///    <para>Retrieves a WebResponse by calling async EndGetResponse()</para>
        /// </devdoc>
        protected virtual WebResponse GetWebResponse(WebRequest request, IAsyncResult result) {
            WebResponse response = request.EndGetResponse(result);
            m_WebResponse = response;
            return response;
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public byte[] DownloadData(string address) {
            if (address == null) 
                throw new ArgumentNullException("address");
            return DownloadData(GetUri(address));
        }

        public byte[] DownloadData(Uri address) {
            if(Logging.On)Logging.Enter(Logging.Web, this, "DownloadData", address);
            if (address == null) 
                throw new ArgumentNullException("address");
            ClearWebClientState();
            byte[] result = null;
            try {
                WebRequest request;
                result = DownloadDataInternal(address, out request);
                if(Logging.On)Logging.Exit(Logging.Web, this, "DownloadData", result);
                return result;
            } finally {
                CompleteWebClientState();
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        private byte[] DownloadDataInternal(Uri address, out WebRequest request) {
            if(Logging.On)Logging.Enter(Logging.Web, this, "DownloadData", address);
            request = null;
            try {
                request = m_WebRequest = GetWebRequest(GetUri(address));
                byte [] returnBytes = DownloadBits(request, null, null, null);
                return returnBytes;
            } catch (Exception e) {
                if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException) {
                    throw;
                }

                if (!(e is WebException || e is SecurityException)) {
                    e = new WebException(SR.GetString(SR.net_webclient), e);
                }

                AbortRequest(request);
                throw e;
            }
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public void DownloadFile(string address, string fileName) {
            if (address == null) 
                throw new ArgumentNullException("address");
            DownloadFile(GetUri(address), fileName);
        }

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public void DownloadFile(Uri address, string fileName) {
            if(Logging.On)Logging.Enter(Logging.Web, this, "DownloadFile", address+", "+fileName);
            if (address == null) 
                throw new ArgumentNullException("address");
            if (fileName == null) 
                throw new ArgumentNullException("fileName");

            WebRequest request = null;
            FileStream fs = null;
            bool succeeded = false;
            ClearWebClientState();
            try {
                fs = new FileStream(fileName, FileMode.Create, FileAccess.Write);
                request = m_WebRequest = GetWebRequest(GetUri(address));
                DownloadBits(request, fs, null, null);
                succeeded = true;
            } catch (Exception e) {
                if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException) {
                    throw;
                }

                if (!(e is WebException || e is SecurityException)) {
                    e = new WebException(SR.GetString(SR.net_webclient), e);
                }

                AbortRequest(request);
                throw e;
            }
            finally {
                if (fs != null) {
                    fs.Close();
                    if (!succeeded) {
                        // Security Review: If we were able to create a file we should be able to delete it
                        File.Delete(fileName);
                    }
                    fs = null;
                }
                CompleteWebClientState();
            }
            if(Logging.On)Logging.Exit(Logging.Web, this, "DownloadFile", "");
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public Stream OpenRead(string address) {
            if (address == null) 
                throw new ArgumentNullException("address");
            return OpenRead(GetUri(address));
        }

        public Stream OpenRead(Uri address) {
            if(Logging.On)Logging.Enter(Logging.Web, this, "OpenRead", address);
            if (address == null) 
                throw new ArgumentNullException("address");
            WebRequest request = null;
            ClearWebClientState();
            try {
                request = m_WebRequest = GetWebRequest(GetUri(address));
                WebResponse response = m_WebResponse = GetWebResponse(request);
                Stream stream = response.GetResponseStream();
                if(Logging.On)Logging.Exit(Logging.Web, this, "OpenRead", stream);
                return stream;
            } catch (Exception e) {
                if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException) {
                    throw;
                }

                if (!(e is WebException || e is SecurityException)) {
                    e = new WebException(SR.GetString(SR.net_webclient), e);
                }

                AbortRequest(request);
                throw e;
            }
            finally {
                CompleteWebClientState();
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public Stream OpenWrite(string address) {
            if (address == null) 
                throw new ArgumentNullException("address");
            return OpenWrite(GetUri(address), null);
        }

        public Stream OpenWrite(Uri address) {
            return OpenWrite(address, null);
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public Stream OpenWrite(string address, string method) {
            if (address == null) 
                throw new ArgumentNullException("address");
            return OpenWrite(GetUri(address), method);
        }

        public Stream OpenWrite(Uri address, string method) {
            if(Logging.On)Logging.Enter(Logging.Web, this, "OpenWrite", address +", "+method);
            if (address == null) 
                throw new ArgumentNullException("address");
            if (method == null) {
                method = MapToDefaultMethod(address);
            }
            WebRequest request = null;
            ClearWebClientState();
            try {
                m_Method = method;
                request = m_WebRequest = GetWebRequest(GetUri(address));
                WebClientWriteStream webClientWriteStream =
                    new WebClientWriteStream(request.GetRequestStream(), request, this);
                if(Logging.On)Logging.Exit(Logging.Web, this, "OpenWrite", webClientWriteStream);
                return webClientWriteStream;
            } catch (Exception e) {
                if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException) {
                    throw;
                }

                if (!(e is WebException || e is SecurityException)) {
                    e = new WebException(SR.GetString(SR.net_webclient), e);
                }

                AbortRequest(request);
                throw e;
            }
            finally {
                CompleteWebClientState();
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public byte[] UploadData(string address, byte[] data) {
            if (address == null) 
                throw new ArgumentNullException("address");
            return UploadData(GetUri(address), null, data);
        }

        public byte[] UploadData(Uri address, byte[] data) {
            return UploadData(address, null, data);
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public byte[] UploadData(string address, string method, byte[] data) {
            if (address == null) 
                throw new ArgumentNullException("address");
            return UploadData(GetUri(address), method, data);
        }

        public byte[] UploadData(Uri address, string method, byte[] data) {
            if(Logging.On)Logging.Enter(Logging.Web, this, "UploadData", address +", "+method);
            if (address == null) 
                throw new ArgumentNullException("address");
            if (data == null) 
                throw new ArgumentNullException("data");
            if (method == null) {
                method = MapToDefaultMethod(address);
            }
            ClearWebClientState();
            try {
                WebRequest request;
                byte [] result = UploadDataInternal(address, method, data, out request);
                if(Logging.On)Logging.Exit(Logging.Web, this, "UploadData", result);
                return result;
            } finally {
                CompleteWebClientState();
            }
        }

        /// <devdoc>
        ///    <para>Internal version of UploadData used for UploadString as well</para>
        /// </devdoc>
        private byte[] UploadDataInternal(Uri address, string method, byte[] data, out WebRequest request) {
            request = null;
            try {
                m_Method = method;
                m_ContentLength = data.Length;
                request = m_WebRequest = GetWebRequest(GetUri(address));
                UploadBits(request, null, data, 0, null, null, null, null, null);
                byte [] responseBytes = DownloadBits(request, null, null, null);
                return responseBytes;
            } catch (Exception e) {
                if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException) {
                    throw;
                }

                if (!(e is WebException || e is SecurityException)) {
                    e = new WebException(SR.GetString(SR.net_webclient), e);
                }

                AbortRequest(request);
                throw e;
            }
        }


        /// <devdoc>
        ///    <para>Open a fileStream and prepares data to send over a WebRequest</para>
        /// </devdoc>
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        private void OpenFileInternal(bool needsHeaderAndBoundary, 
                                      string fileName, 
                                      ref FileStream fs, 
                                      ref byte[] buffer, 
                                      ref byte[] formHeaderBytes, 
                                      ref byte[] boundaryBytes) {
            fileName = Path.GetFullPath(fileName);

            if (m_headers == null) {
                m_headers = new WebHeaderCollection(WebHeaderCollectionType.WebRequest);
            }

            string contentType = m_headers[HttpKnownHeaderNames.ContentType];

            if (contentType != null) {
                if (contentType.ToLower(CultureInfo.InvariantCulture).StartsWith("multipart/")) {
                    throw new WebException(SR.GetString(SR.net_webclient_Multipart));
                }
            } else {
                contentType = DefaultUploadFileContentType;
            }

            fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);

            int buffSize = DefaultCopyBufferLength;
            m_ContentLength = -1;

            if (m_Method.ToUpper(CultureInfo.InvariantCulture) == "POST")
            {
                if (needsHeaderAndBoundary)
                {
                    string boundary = "---------------------" + DateTime.Now.Ticks.ToString("x", NumberFormatInfo.InvariantInfo);

                    m_headers[HttpKnownHeaderNames.ContentType] = UploadFileContentType + "; boundary=" + boundary;

                    string formHeader = "--" + boundary + "\r\n"
                                    + "Content-Disposition: form-data; name=\"file\"; filename=\"" + Path.GetFileName(fileName) + "\"\r\n"
                                    + "Content-Type: " + contentType + "\r\n"
                                    + "\r\n";
                    formHeaderBytes = Encoding.UTF8.GetBytes(formHeader);
                    boundaryBytes = Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
                }
                else
                {
                    formHeaderBytes = new byte[0];
                    boundaryBytes = new byte[0];
                }

                if (fs.CanSeek)
                {
                    m_ContentLength = fs.Length + formHeaderBytes.Length + boundaryBytes.Length;
                    buffSize = (int)Math.Min((long)DefaultCopyBufferLength, fs.Length);
                }
            }
            else
            {
                m_headers[HttpKnownHeaderNames.ContentType] = contentType;

                formHeaderBytes = null;
                boundaryBytes = null;

                if (fs.CanSeek)
                {
                    m_ContentLength = fs.Length;
                    buffSize = (int) Math.Min((long) DefaultCopyBufferLength, fs.Length);
                }
            }

            buffer = new byte[buffSize];
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public byte[] UploadFile(string address, string fileName) {
            if (address == null) 
                throw new ArgumentNullException("address");
            return UploadFile(GetUri(address), fileName);
        }

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public byte[] UploadFile(Uri address, string fileName) {
            return UploadFile(address, null, fileName);
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public byte[] UploadFile(string address, string method, string fileName) {
            return UploadFile(GetUri(address), method, fileName);
        }

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public byte[] UploadFile(Uri address, string method, string fileName) {
            if(Logging.On)Logging.Enter(Logging.Web, this, "UploadFile", address +", "+method);
            if (address == null) 
                throw new ArgumentNullException("address");
            if (fileName == null) 
                throw new ArgumentNullException("fileName");
            if (method == null) {
                method = MapToDefaultMethod(address);
            }
            FileStream fs = null;
            WebRequest request = null;
            ClearWebClientState();
            try {
                m_Method = method;
                byte [] formHeaderBytes = null, boundaryBytes = null, buffer = null;
                Uri uri = GetUri(address);
                bool needsHeaderAndBoundary = (uri.Scheme != Uri.UriSchemeFile);
                OpenFileInternal(needsHeaderAndBoundary, fileName, ref fs, ref buffer, ref formHeaderBytes, ref boundaryBytes);
                request = m_WebRequest = GetWebRequest(uri);
                UploadBits(request, fs, buffer, 0, formHeaderBytes, boundaryBytes, null, null, null);
                byte [] responseBytes = DownloadBits(request, null, null, null);
                if(Logging.On)Logging.Exit(Logging.Web, this, "UploadFile", responseBytes);
                return responseBytes;
            } catch (Exception e) {
                if (fs != null) {
                    fs.Close();
                    fs = null;
                }
                if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException) {
                    throw;
                }

                if (!(e is WebException || e is SecurityException)) {
                    e = new WebException(SR.GetString(SR.net_webclient), e);
                }

                AbortRequest(request);
                throw e;
            }
            finally {
                CompleteWebClientState();
            }
        }

        /// <devdoc>
        ///    <para>Shared code for UploadValues, creates a memory stream of data to send</para>
        /// </devdoc>
        private byte[] UploadValuesInternal(NameValueCollection data) {
            if (m_headers == null) {
                m_headers = new WebHeaderCollection(WebHeaderCollectionType.WebRequest);
            }

            string contentType = m_headers[HttpKnownHeaderNames.ContentType];

            if ((contentType != null) && (String.Compare(contentType, UploadValuesContentType, StringComparison.OrdinalIgnoreCase) != 0)) {
                throw new WebException(SR.GetString(SR.net_webclient_ContentType));
            }
            m_headers[HttpKnownHeaderNames.ContentType] = UploadValuesContentType;

            string delimiter = String.Empty;
            StringBuilder values = new StringBuilder();
            foreach (string name in data.AllKeys) {
                values.Append(delimiter);
                values.Append( UrlEncode(name));
                values.Append("=");
                values.Append(UrlEncode(data[name]));
                delimiter = "&";
            }

            byte[] buffer = Encoding.ASCII.GetBytes(values.ToString());
            m_ContentLength = buffer.Length;
            return buffer;
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public byte[] UploadValues(string address, NameValueCollection data) {
            if (address == null) 
                throw new ArgumentNullException("address");
            return UploadValues(GetUri(address), null, data);
        }

        public byte[] UploadValues(Uri address, NameValueCollection data) {
            return UploadValues(address, null, data);
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public byte[] UploadValues(string address, string method, NameValueCollection data) {
            if (address == null) 
                throw new ArgumentNullException("address");
            return UploadValues(GetUri(address), method, data);
        }

        public byte[] UploadValues(Uri address, string method, NameValueCollection data) {
            if(Logging.On)Logging.Enter(Logging.Web, this, "UploadValues", address +", "+method);
            if (address == null) 
                throw new ArgumentNullException("address");
            if (data == null) 
                throw new ArgumentNullException("data");
            if (method == null) {
                method = MapToDefaultMethod(address);
            }
            WebRequest request = null;
            ClearWebClientState();
            try {
                byte[] buffer = UploadValuesInternal(data);
                m_Method = method;
                request = m_WebRequest = GetWebRequest(GetUri(address));
                UploadBits(request, null, buffer, 0, null, null, null, null, null);
                byte [] returnBytes = DownloadBits(request, null, null, null);
                if(Logging.On)Logging.Exit(Logging.Web, this, "UploadValues", address +", "+method);
                return returnBytes;
            } catch (Exception e) {
                if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException) {
                    throw;
                }

                if (!(e is WebException || e is SecurityException)) {
                    e = new WebException(SR.GetString(SR.net_webclient), e);
                }

                AbortRequest(request);
                throw e;
            }
            finally {
                CompleteWebClientState();
            }
        }

        //
        // String Methods -
        //

        /// <devdoc>
        ///    <para>Uploads a string of data and returns a string of data</para>
        /// </devdoc>
        public string UploadString(string address, string data) {
            if (address == null) 
                throw new ArgumentNullException("address");
            return UploadString(GetUri(address), null, data);
        }

        public string UploadString(Uri address, string data) {
            return UploadString(address, null, data);
        }

        /// <devdoc>
        ///    <para>Uploads a string of data and returns a string of data</para>
        /// </devdoc>
        public string UploadString(string address, string method, string data) {
            if (address == null) 
                throw new ArgumentNullException("address");
            return UploadString(GetUri(address), method, data);
        }

        public string UploadString(Uri address, string method, string data) {
            if(Logging.On)Logging.Enter(Logging.Web, this, "UploadString", address +", "+method);
            if (address == null) 
                throw new ArgumentNullException("address");
            if (data == null) 
                throw new ArgumentNullException("data");
            if (method == null) {
                method = MapToDefaultMethod(address);
            }
            ClearWebClientState();
            try {
                WebRequest request;
                byte [] requestData = Encoding.GetBytes(data);
                byte [] responseData = UploadDataInternal(address, method, requestData, out request);
                string responseStringData = GetStringUsingEncoding(request, responseData);
                if(Logging.On)Logging.Exit(Logging.Web, this, "UploadString", responseStringData);
                return responseStringData;
            } finally {
                CompleteWebClientState();
            }
        }

        /// <devdoc>
        ///    <para>Downloads a string from the server</para>
        /// </devdoc>
        public string DownloadString(string address) {
            if (address == null) 
                throw new ArgumentNullException("address");
            return DownloadString(GetUri(address));
        }

        public string DownloadString(Uri address) {
            if(Logging.On)Logging.Enter(Logging.Web, this, "DownloadString", address);
            if (address == null) 
                throw new ArgumentNullException("address");
            ClearWebClientState();
            try {
                WebRequest request;
                byte [] data = DownloadDataInternal(address, out request);
                string stringData = GetStringUsingEncoding(request, data);
                if(Logging.On)Logging.Exit(Logging.Web, this, "DownloadString", stringData);
                return stringData;
            } finally {
                CompleteWebClientState();
            }
        }

        /// <devdoc>
        ///    <para>Aborts the request without throwing, so that we can prevent double errors</para>
        /// </devdoc>
        private static void AbortRequest(WebRequest request) {
            try {
                if (request != null) {
                    request.Abort();
                }
            }
            catch (Exception exception) {
                if (exception is OutOfMemoryException || exception is StackOverflowException || exception is ThreadAbortException) {
                    throw;
                }
            }
        }

        /// <devdoc>
        ///    <para>Copies HTTP headers to a HttpWebRequest.Headers property</para>
        /// </devdoc>
        private void CopyHeadersTo(WebRequest request) {
            if ((m_headers != null) && (request is HttpWebRequest))  {

                string accept = m_headers[HttpKnownHeaderNames.Accept];
                string connection = m_headers[HttpKnownHeaderNames.Connection];
                string contentType = m_headers[HttpKnownHeaderNames.ContentType];
                string expect = m_headers[HttpKnownHeaderNames.Expect];
                string referrer = m_headers[HttpKnownHeaderNames.Referer];
                string userAgent = m_headers[HttpKnownHeaderNames.UserAgent];
                string host = m_headers[HttpKnownHeaderNames.Host];

                m_headers.RemoveInternal(HttpKnownHeaderNames.Accept);
                m_headers.RemoveInternal(HttpKnownHeaderNames.Connection);
                m_headers.RemoveInternal(HttpKnownHeaderNames.ContentType);
                m_headers.RemoveInternal(HttpKnownHeaderNames.Expect);
                m_headers.RemoveInternal(HttpKnownHeaderNames.Referer);
                m_headers.RemoveInternal(HttpKnownHeaderNames.UserAgent);
                m_headers.RemoveInternal(HttpKnownHeaderNames.Host);
                request.Headers = m_headers;
                if ((accept != null) && (accept.Length > 0)) {
                    ((HttpWebRequest)request).Accept = accept;
                }
                if ((connection != null) && (connection.Length > 0)) {
                    ((HttpWebRequest)request).Connection = connection;
                }
                if ((contentType != null) && (contentType.Length > 0)) {
                    ((HttpWebRequest)request).ContentType = contentType;
                }
                if ((expect != null) && (expect.Length > 0)) {
                    ((HttpWebRequest)request).Expect = expect;
                }
                if ((referrer != null) && (referrer.Length > 0)) {
                    ((HttpWebRequest)request).Referer = referrer;
                }
                if ((userAgent != null) && (userAgent.Length > 0)) {
                    ((HttpWebRequest)request).UserAgent = userAgent;
                }
                if (!string.IsNullOrEmpty(host)) {
                    ((HttpWebRequest)request).Host = host;
                }
            }
        }

        /// <devdoc>
        ///    <para>Parses the string uri into a properly formed uri - uses Uri class</para>
        /// </devdoc>
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        private Uri GetUri(string path) {

            Uri uri;

            if (m_baseAddress != null)
            {
                if (!Uri.TryCreate(m_baseAddress, path, out uri))
                    return new Uri(Path.GetFullPath(path));
            } else
            {
                if (!Uri.TryCreate(path, UriKind.Absolute, out uri))
                    return new Uri(Path.GetFullPath(path));
            }

            return GetUri(uri);
        }

        /// <devdoc>
        ///    <para>Parses the string uri into a properly formed uri - uses Uri class</para>
        /// </devdoc>
        private Uri GetUri(Uri address) {
            if (address == null)
                throw new ArgumentNullException("address");

            Uri uri = address;

            if (!address.IsAbsoluteUri && m_baseAddress != null)
            {
                if (!Uri.TryCreate(m_baseAddress, address, out uri))
                    return address;
            }

            if ((uri.Query == null || uri.Query == string.Empty) && m_requestParameters != null) {

                StringBuilder sb = new StringBuilder();
                string delimiter = String.Empty;

                for (int i = 0; i < m_requestParameters.Count; ++i) {
                    sb.Append(delimiter
                              + m_requestParameters.AllKeys[i]
                              + "="
                              + m_requestParameters[i]
                              );
                    delimiter = "&";
                }

                UriBuilder ub = new UriBuilder(uri);

                ub.Query = sb.ToString();
                uri = ub.Uri;
            }

            return uri;
        }


        //
        // ProgressData
        // Keeps track of overall operation progress
        // Used by async operations for client updates, especially to hold state from the upload phase to download.
        //
        private class ProgressData
        {
            internal long BytesSent = 0;
            internal long TotalBytesToSend = -1;
            internal long BytesReceived = 0;
            internal long TotalBytesToReceive = -1;
            internal bool HasUploadPhase = false;

            internal void Reset()
            {
                BytesSent = 0;
                TotalBytesToSend = -1;
                BytesReceived = 0;
                TotalBytesToReceive = -1;
                HasUploadPhase = false;
            }
        }


        //
        // DownloadBits -
        //  works by abstracting the process of downloading using WebRequest.GetResponse()
        //  3 levels of functions/methods are used for this process
        //
        //  1. DownloadBits - generates a state object of DownloadBitsState, then
        //      starts the async GetResponse(), or drives calls directly to
        //      DownloadBitsState.SetResponse() and DownloadBitsState.RetrieveBytes
        //
        //  2. DownloadBitsResponseCallback and DownloadBitsReadCallback -
        //      Abstracts the async EndGetResponse and Stream.EndRead
        //      calls from the process of downloading data.   Notifies the caller of
        //      DownloadBits through a callback when completed.  Catches exceptions
        //      and errors and passes them through the callback
        //
        //  3. DownloadBitsState.SetResponse() and DownloadBitsState.RetrieveBytes -
        //      Updates the state of the download by seeding variables and pumps
        //      data through the streams and structures
        //
        //


        /// <devdoc>
        ///    <para>Holds the state and handles the basic async logic of downloading</para>
        /// </devdoc>
        private class DownloadBitsState {
            internal WebClient WebClient;
            internal Stream WriteStream;
            internal byte[] InnerBuffer;
            internal AsyncOperation AsyncOp;
            internal WebRequest Request;
            internal CompletionDelegate CompletionDelegate;
            internal Stream ReadStream;
            internal ScatterGatherBuffers SgBuffers;

            internal DownloadBitsState(WebRequest request, Stream writeStream, CompletionDelegate completionDelegate, AsyncOperation asyncOp, ProgressData progress, WebClient webClient) {
                WriteStream = writeStream;
                Request = request;
                AsyncOp = asyncOp;
                CompletionDelegate = completionDelegate;
                WebClient = webClient;
                Progress = progress;
            }

            internal long ContentLength;
            internal long Length;
            internal int  Offset;


            internal ProgressData Progress;

            internal bool Async {
                get {
                    return AsyncOp != null;
                }
            }

            internal int SetResponse(WebResponse response) {
                ContentLength = response.ContentLength;

                if (ContentLength == -1 || ContentLength > DefaultDownloadBufferLength) {
                    Length = DefaultDownloadBufferLength; // Read buffer length
                } else {
                    Length = ContentLength; // Read buffer length
                }
                
                // If we are not writing to a stream, we are accumulating in memory
                if (WriteStream == null) {
                    // We are putting a cap on the size we will accumulate in memory
                    if (ContentLength > Int32.MaxValue)
                    {
                        throw new WebException(SR.GetString(SR.net_webstatus_MessageLengthLimitExceeded), WebExceptionStatus.MessageLengthLimitExceeded);
                    }
                    SgBuffers = new ScatterGatherBuffers(Length); // Write buffer
                }

                InnerBuffer = new byte[(int)Length];

                ReadStream = response.GetResponseStream();
                if (Async && response.ContentLength >= 0)
                    Progress.TotalBytesToReceive = response.ContentLength;
                
                if (Async) {
                    if (ReadStream == null || ReadStream == Stream.Null)
                        DownloadBitsReadCallbackState(this, null);
                    else
                        ReadStream.BeginRead(InnerBuffer, Offset, (int)Length-Offset, new AsyncCallback(DownloadBitsReadCallback), this);
                } else {
                    if (ReadStream == null || ReadStream == Stream.Null)
                        return 0;
                    else
                        return ReadStream.Read(InnerBuffer, Offset, (int)Length-Offset);
                }
                return -1;
            }

            internal bool RetrieveBytes(ref int bytesRetrieved) {
                if (bytesRetrieved > 0) {
                    if (WriteStream != null) {
                        WriteStream.Write(InnerBuffer, 0, bytesRetrieved);
                    } else {
                        SgBuffers.Write(InnerBuffer, 0, bytesRetrieved);
                    }

                    if (Async)
                        Progress.BytesReceived += bytesRetrieved;

                    if (Offset != ContentLength) {
                        if (Async) {
                            WebClient.PostProgressChanged(AsyncOp, Progress);
                            ReadStream.BeginRead(InnerBuffer, Offset, (int)Length-Offset, new AsyncCallback(DownloadBitsReadCallback), this);
                        } else {
                            bytesRetrieved = ReadStream.Read(InnerBuffer, Offset, (int)Length-Offset);
                        }
                        return false;
                    }
                }

                // Final change notification
                if (Async)
                {
                    if (Progress.TotalBytesToReceive < 0)
                        Progress.TotalBytesToReceive = Progress.BytesReceived;
                    WebClient.PostProgressChanged(AsyncOp, Progress);
                }

                // completed here
                if (ReadStream != null)
                    ReadStream.Close();
                if (WriteStream != null) {
                    WriteStream.Close();
                } else {
                    if (WriteStream == null) { // We are using Scatter-Gather buffers
                        byte[] newbuf = new byte[SgBuffers.Length];
                        if (SgBuffers.Length > 0) {
                            BufferOffsetSize[] bufferArray = SgBuffers.GetBuffers();
                            int newBufOffset = 0;
                            for (int i=0; i<bufferArray.Length; i++)
                            {
                                BufferOffsetSize bufferOffsetSize = bufferArray[i];
                                Buffer.BlockCopy(bufferOffsetSize.Buffer, 0, newbuf, newBufOffset, bufferOffsetSize.Size);
                                newBufOffset += bufferOffsetSize.Size;
                            }
                        } 
                        InnerBuffer = newbuf;
                    }
                }
                // do callback now
                return true;
            }

            internal void Close() {
                if (WriteStream != null) {
                    WriteStream.Close();
                }
                if (ReadStream != null) {
                    ReadStream.Close();
                }
            }
        }

        static private void DownloadBitsResponseCallback(IAsyncResult result) {
            DownloadBitsState state = (DownloadBitsState) result.AsyncState;
            WebRequest request = (WebRequest) state.Request;
            Exception exception = null;

            try {
                WebResponse response = state.WebClient.GetWebResponse(request, result);
                state.WebClient.m_WebResponse = response;
                state.SetResponse(response);
            } catch (Exception e) {
                if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException) {
                    throw;
                }
                exception = e;
                if (!(e is WebException || e is SecurityException)) {
                    exception = new WebException(SR.GetString(SR.net_webclient), e);
                }
                AbortRequest(request);
                if(state != null && state.WriteStream != null){
                    state.WriteStream.Close();
                }
            }
            finally {
                if (exception != null) {
                    state.CompletionDelegate(null, exception, state.AsyncOp);
                }
            }

        }

        static private void DownloadBitsReadCallback(IAsyncResult result) {
            DownloadBitsState state = (DownloadBitsState) result.AsyncState;
            DownloadBitsReadCallbackState(state, result);
        }

        static private void DownloadBitsReadCallbackState(DownloadBitsState state, IAsyncResult result) {
            Stream stream = state.ReadStream;

            Exception exception = null;
            bool completed = false;

            try {
                int bytesRead = 0;
                if (stream != null && stream != Stream.Null)
                    bytesRead = stream.EndRead(result);
                completed = state.RetrieveBytes(ref bytesRead);
            } catch (Exception e) {
                completed = true;
                if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException) {
                    throw;
                }
                exception = e;
                state.InnerBuffer = null;
                if (!(e is WebException || e is SecurityException)) {
                    exception = new WebException(SR.GetString(SR.net_webclient), e);
                }
                AbortRequest(state.Request);
                if(state != null && state.WriteStream != null){
                    state.WriteStream.Close();
                }
            }
            finally {
                if (completed) {
                    if(exception == null){
                        state.Close();
                    }
                    state.CompletionDelegate(state.InnerBuffer, exception, state.AsyncOp);
                }
            }

        }


        /// <devdoc>
        ///    <para>Generates a byte array or downloads data to an open file stream</para>
        /// </devdoc>
        private byte[] DownloadBits(WebRequest request, Stream writeStream, CompletionDelegate completionDelegate, AsyncOperation asyncOp) {
            WebResponse response = null;
            DownloadBitsState state = new DownloadBitsState(request, writeStream, completionDelegate, asyncOp, m_Progress, this);

            if (state.Async) {
                request.BeginGetResponse(new AsyncCallback(DownloadBitsResponseCallback), state);
                return null;
            } else {
                response = m_WebResponse = GetWebResponse(request);
            }

            bool completed;
            int bytesRead = state.SetResponse(response);
            do {
                completed = state.RetrieveBytes(ref bytesRead);
            } while (!completed);
            state.Close();
            return state.InnerBuffer;
        }

        //
        // UploadBits -
        //  works by abstracting the process of uploading using WebRequest.GetRequestStream()
        //  3 levels of functions/methods are used for this process
        //
        //  1. UploadBits - generates a state object of UploadBitsState, then
        //      starts the async GetRequestStream, or drives calls directly to
        //      UploadBitsState.SetRequestStream() and UploadBitsState.WriteBytes
        //
        //  2. UploadBitsRequestCallback and UploadBitsWriteCallback -
        //      Abstracts the async EndGetRequestStream and Stream.EndWrite
        //      calls from the process of uploading data.   Notifies the caller of
        //      UploadBits through a callback when completed.
        //
        //  3. UploadBitsState.SetRequestStream() and UploadBitsState.WriteBytes -
        //      Updates the state of the upload by seeding variables and pumps
        //      data through the streams and structures
        //
        //

        /// <devdoc>
        ///    <para>Holds the state and handles the basic async logic of uploading</para>
        /// </devdoc>
        private class UploadBitsState {
            
            int m_ChunkSize;
            int m_BufferWritePosition;

            internal WebClient WebClient;
            internal Stream WriteStream;
            internal byte[] InnerBuffer;
            internal byte[] Header;
            internal byte[] Footer;
            internal AsyncOperation AsyncOp;
            internal WebRequest Request;
            internal CompletionDelegate UploadCompletionDelegate;
            internal CompletionDelegate DownloadCompletionDelegate;

            internal Stream ReadStream;

            internal UploadBitsState(WebRequest request, Stream readStream, byte[] buffer, int chunkSize, byte[] header, byte[] footer, CompletionDelegate uploadCompletionDelegate, CompletionDelegate downloadCompletionDelegate, AsyncOperation asyncOp, ProgressData progress, WebClient webClient) {
                InnerBuffer = buffer;
                m_ChunkSize = chunkSize;
                m_BufferWritePosition = 0;
                Header = header;
                Footer = footer;
                ReadStream = readStream;
                Request = request;
                AsyncOp = asyncOp;
                UploadCompletionDelegate = uploadCompletionDelegate;
                DownloadCompletionDelegate = downloadCompletionDelegate;

                if (AsyncOp != null)
                {
                    Progress = progress;
                    Progress.HasUploadPhase = true;
                    Progress.TotalBytesToSend = request.ContentLength < 0 ? -1 : request.ContentLength;
                }

                WebClient = webClient;
            }

            internal long Length;
            internal int  Offset;

            internal ProgressData Progress;

            internal bool FileUpload {
                get {
                    return ReadStream != null;
                }
            }

            internal bool Async {
                get {
                    return AsyncOp != null;
                }
            }
            internal void SetRequestStream(Stream writeStream) {
                WriteStream = writeStream;
                byte [] bytesToWrite = null;

                if (Header != null) {
                    bytesToWrite = Header;
                    Header = null;
                }
                else {
                    bytesToWrite = new byte[0];
                }

                if (Async) {
                    Progress.BytesSent += bytesToWrite.Length;
                    WriteStream.BeginWrite(bytesToWrite, 0, bytesToWrite.Length, new AsyncCallback(UploadBitsWriteCallback), this);
                }
                else {
                    WriteStream.Write(bytesToWrite, 0, bytesToWrite.Length);
                }
            }

            internal bool WriteBytes() {
                byte [] bytesToWrite = null;
                int bytesToWriteLength = 0;
                int bufferOffset = 0;

                if (Async) {
                    WebClient.PostProgressChanged(AsyncOp, Progress);
                }

                if (FileUpload) {
                    int bytesRead = 0;
                    if (InnerBuffer != null) {
                        bytesRead = ReadStream.Read(InnerBuffer, 0, (int)InnerBuffer.Length);
                        if (bytesRead <= 0) {
                            ReadStream.Close();
                            InnerBuffer = null;
                        }
                    }
                    if (InnerBuffer != null) {
                        bytesToWriteLength = bytesRead;
                        bytesToWrite = InnerBuffer;
                    } else if (Footer != null) {
                        bytesToWriteLength = Footer.Length;
                        bytesToWrite = Footer;
                        Footer = null;
                    } else {
                        return true; // completed
                    }
                } else if (InnerBuffer != null) {
                    bytesToWrite = InnerBuffer;
                    if (m_ChunkSize != 0) {
                        // We should send the buffer in chunks of ChunkSize
                        bufferOffset = m_BufferWritePosition;
                        m_BufferWritePosition += m_ChunkSize;
                        bytesToWriteLength = m_ChunkSize;
                        if (m_BufferWritePosition >= InnerBuffer.Length) { // This is the last chunk
                            bytesToWriteLength = InnerBuffer.Length - bufferOffset;
                            InnerBuffer = null;
                        }
                    }
                    else {
                        bytesToWriteLength = InnerBuffer.Length;
                        InnerBuffer = null;
                    }
                }
                else {
                    return true; // completed
                }

                if (Async) {
                    Progress.BytesSent += bytesToWriteLength;
                    WriteStream.BeginWrite(bytesToWrite, bufferOffset, bytesToWriteLength, new AsyncCallback(UploadBitsWriteCallback), this);
                } else {
                    WriteStream.Write(bytesToWrite, 0, bytesToWriteLength);
                }

                return false; // not complete
            }

            internal void Close() {
                if (WriteStream != null) {
                    WriteStream.Close();
                }
                if (ReadStream != null) {
                    ReadStream.Close();
                }
            }
        }


        static private void UploadBitsRequestCallback(IAsyncResult result) {
            UploadBitsState state = (UploadBitsState) result.AsyncState;
            WebRequest request = (WebRequest) state.Request;

            Exception exception = null;

            try {
                Stream stream = request.EndGetRequestStream(result);
                state.SetRequestStream(stream);
            } catch (Exception e) {
                if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException) {
                    throw;
                }
                exception = e;
                if (!(e is WebException || e is SecurityException)) {
                    exception = new WebException(SR.GetString(SR.net_webclient), e);
                }
                AbortRequest(request);
                if(state != null && state.ReadStream != null){
                    state.ReadStream.Close();
                }
            }
            finally {
                if (exception != null) {
                    state.UploadCompletionDelegate(null, exception, state);
                }
            }
        }

        static private void UploadBitsWriteCallback(IAsyncResult result) {
            UploadBitsState state = (UploadBitsState) result.AsyncState;
            Stream stream = (Stream) state.WriteStream;

            Exception exception = null;
            bool completed = false;

            try {
                stream.EndWrite(result);
                completed = state.WriteBytes();
            } catch (Exception e) {
                completed = true;
                if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException) {
                    throw;
                }
                exception = e;
                if (!(e is WebException || e is SecurityException)) {
                    exception = new WebException(SR.GetString(SR.net_webclient), e);
                }
                AbortRequest(state.Request);
                if(state != null && state.ReadStream != null){
                    state.ReadStream.Close();
                }
            }
            finally {
                if (completed) {
                    if(exception == null){
                        state.Close();
                    }

                    state.UploadCompletionDelegate(null, exception, state);
                }
            }
        }


        /// <devdoc>
        ///    <para>Takes a byte array or an open file stream and writes it to a server</para>
        /// </devdoc>

        private void UploadBits(WebRequest request, Stream readStream, byte[] buffer, int chunkSize, byte[] header, byte[] footer, CompletionDelegate uploadCompletionDelegate, CompletionDelegate downloadCompletionDelegate, AsyncOperation asyncOp) {
            if (request.RequestUri.Scheme == Uri.UriSchemeFile)
                header = footer = null;
            UploadBitsState state = new UploadBitsState(request, readStream, buffer, chunkSize, header, footer, uploadCompletionDelegate, 
                downloadCompletionDelegate, asyncOp, m_Progress, this);
            Stream writeStream;
            if (state.Async) {
                request.BeginGetRequestStream(new AsyncCallback(UploadBitsRequestCallback), state);
                return;
            } else {
                writeStream = request.GetRequestStream();
            }
            state.SetRequestStream(writeStream);
            while(!state.WriteBytes());
            state.Close();
        }

        private bool ByteArrayHasPrefix(byte[] prefix, byte[] byteArray)
        {
            if (prefix == null || byteArray == null || prefix.Length > byteArray.Length)
                return false;
            for (int i = 0; i < prefix.Length; i++)
            {
                if (prefix[i] != byteArray[i])
                    return false;
            }
            return true;
        }

        private string GetStringUsingEncoding(WebRequest request, byte[] data)
        {
            Encoding enc = null;
            int bomLengthInData = -1;

            // Figure out encoding by first checking for encoding string in Content-Type HTTP header
            // This can throw NotImplementedException if the derived class of WebRequest doesn't support it.
            string contentType;
            try
            {
                contentType = request.ContentType;
            }
            catch (NotImplementedException)
            {
                contentType = null;
            }
            catch (NotSupportedException)  // need this since our FtpWebRequest class mistakenly does this
            {
                contentType = null;
            }
            // Unexpected exceptions are thrown back to caller
            
            if (contentType != null)
            {
                contentType = contentType.ToLower(CultureInfo.InvariantCulture);
                string[] parsedList = contentType.Split(new char[] { ';', '=', ' ' });
                bool nextItem = false;
                foreach (string item in parsedList)
                {
                    if (item == "charset")
                    {
                        nextItem = true;
                    }
                    else if (nextItem)
                    {
                        try
                        {
                            enc = Encoding.GetEncoding(item);
                        }
                        catch (ArgumentException)
                        {
                            // Eat ArgumentException here.    
                            // We'll assume that Content-Type encoding might have been garbled and wasn't present at all.
                            break;
                        }
                        // Unexpected exceptions are thrown back to caller
                    }
                }
            }

            // If no content encoding listed in the ContentType HTTP header, or no Content-Type header present, then
            // check for a byte-order-mark (BOM) in the data to figure out encoding.
            if (enc == null)
            {
                byte[] preamble;
                // UTF32 must be tested before Unicode because it's BOM is the same but longer.
                Encoding[] encodings = { Encoding.UTF8, Encoding.UTF32, Encoding.Unicode, Encoding.BigEndianUnicode };
                for (int i = 0; i < encodings.Length; i++)
                {
                    preamble = encodings[i].GetPreamble();
                    if (ByteArrayHasPrefix(preamble, data))
                    {
                        enc = encodings[i];
                        bomLengthInData = preamble.Length;
                        break;
                    }
                }
            }

            // Do we have an encoding guess?  If not, use default.
            if (enc == null)
                enc = this.Encoding;

            // Calculate BOM length based on encoding guess.  Then check for it in the data.
            if (bomLengthInData == -1)
            {
                byte[] preamble = enc.GetPreamble();
                if (ByteArrayHasPrefix(preamble, data))
                    bomLengthInData = preamble.Length;
                else
                    bomLengthInData = 0;
            }

            // Convert byte array to string stripping off any BOM before calling GetString().
            // This is required since GetString() doesn't handle stripping off BOM.
            return enc.GetString(data, bomLengthInData, data.Length - bomLengthInData);
        }

        private string MapToDefaultMethod(Uri address) {
            Uri uri;
            if (!address.IsAbsoluteUri && m_baseAddress != null) {
                uri = new Uri(m_baseAddress, address);
            } else {
                uri = address;
            }
            if (uri.Scheme.ToLower(CultureInfo.InvariantCulture) == "ftp") {
                return WebRequestMethods.Ftp.UploadFile;
            } else {
                return "POST";
            }
        }

        private static string UrlEncode(string str) {
            if (str == null)
                return null;
            return UrlEncode(str, Encoding.UTF8);
        }

        private static string UrlEncode(string str, Encoding e) {
            if (str == null)
                return null;
            return Encoding.ASCII.GetString(UrlEncodeToBytes(str, e));
        }

        private static byte[] UrlEncodeToBytes(string str, Encoding e) {
            if (str == null)
                return null;
            byte[] bytes = e.GetBytes(str);
            return UrlEncodeBytesToBytesInternal(bytes, 0, bytes.Length, false);
        }

        private static byte[] UrlEncodeBytesToBytesInternal(byte[] bytes, int offset, int count, bool alwaysCreateReturnValue) {
            int cSpaces = 0;
            int cUnsafe = 0;

            // count them first
            for (int i = 0; i < count; i++) {
                char ch = (char)bytes[offset+i];

                if (ch == ' ')
                    cSpaces++;
                else if (!IsSafe(ch))
                    cUnsafe++;
            }

            // nothing to expand?
            if (!alwaysCreateReturnValue && cSpaces == 0 && cUnsafe == 0)
                return bytes;

            // expand not 'safe' characters into %XX, spaces to +s
            byte[] expandedBytes = new byte[count + cUnsafe*2];
            int pos = 0;

            for (int i = 0; i < count; i++) {
                byte b = bytes[offset+i];
                char ch = (char)b;

                if (IsSafe(ch)) {
                    expandedBytes[pos++] = b;
                }
                else if (ch == ' ') {
                    expandedBytes[pos++] = (byte)'+';
                }
                else {
                    expandedBytes[pos++] = (byte)'%';
                    expandedBytes[pos++] = (byte)IntToHex((b >> 4) & 0xf);
                    expandedBytes[pos++] = (byte)IntToHex(b & 0x0f);
                }
            }

            return expandedBytes;
        }

        private static char IntToHex(int n) {
            Debug.Assert(n < 0x10);

            if (n <= 9)
                return(char)(n + (int)'0');
            else
                return(char)(n - 10 + (int)'a');
        }

        private static bool IsSafe(char ch) {
            if (ch >= 'a' && ch <= 'z' || ch >= 'A' && ch <= 'Z' || ch >= '0' && ch <= '9')
                return true;

            switch (ch) {
                case '-':
                case '_':
                case '.':
                case '!':
                case '*':
                case '\'':
                case '(':
                case ')':
                    return true;
            }

            return false;
        }

        private     int             m_CallNesting;              // > 0 if we're in a Read/Write call
        private     AsyncOperation  m_AsyncOp;
    
        private void InvokeOperationCompleted(AsyncOperation asyncOp, SendOrPostCallback callback, AsyncCompletedEventArgs eventArgs) {
            if ((object)Interlocked.CompareExchange<AsyncOperation>(ref m_AsyncOp, null, asyncOp) ==  (object) asyncOp)
            {
                CompleteWebClientState();
                // AsyncOperationManager is responsible for invoke the callback
                asyncOp.PostOperationCompleted(callback, eventArgs);
            }
        }

        private bool AnotherCallInProgress(int callNesting) {
            return callNesting>1;
        }


        //
        // Async methods and strucs -
        // See spec models at the following addresses:
        // http://dotnetclient/whidbey/M2%20Specs/AsynchronousOperationManager.doc
        // http://dotnetclient/whidbey/M2%20Specs/Guidelines%20and%20Usage%20Model%20for%20Asynchronous%20Pattern%20for%20Components.doc
        //

        //
        // OpenRead
        //
        public event OpenReadCompletedEventHandler OpenReadCompleted;
        protected virtual void OnOpenReadCompleted(OpenReadCompletedEventArgs e) {
            if (OpenReadCompleted != null) {
                OpenReadCompleted(this, e);
            }
        }
        private SendOrPostCallback openReadOperationCompleted;
        private void OpenReadOperationCompleted(object arg) {
            OnOpenReadCompleted((OpenReadCompletedEventArgs)arg);
        }
        private void OpenReadAsyncCallback(IAsyncResult result) {
            LazyAsyncResult lazyAsyncResult = (LazyAsyncResult) result;
            AsyncOperation asyncOp = (AsyncOperation) lazyAsyncResult.AsyncState;
            WebRequest request = (WebRequest) lazyAsyncResult.AsyncObject;
            Stream stream = null;
            Exception exception = null;
            try {
                WebResponse response = m_WebResponse = GetWebResponse(request, result);
                stream = response.GetResponseStream();
            } catch (Exception e) {
                if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException) {
                    throw;
                }
                exception = e;
                if (!(e is WebException || e is SecurityException)) {
                    exception = new WebException(SR.GetString(SR.net_webclient), e);
                }
            }
            OpenReadCompletedEventArgs eventArgs =
                new OpenReadCompletedEventArgs(stream, exception, m_Cancelled, asyncOp.UserSuppliedState);

            InvokeOperationCompleted(asyncOp, openReadOperationCompleted, eventArgs);
        }

        [HostProtection(ExternalThreading=true)]
        public void OpenReadAsync(Uri address)
        {
            OpenReadAsync(address, null);
        }

        [HostProtection(ExternalThreading=true)]
        public void OpenReadAsync(Uri address, object userToken)
        {
            if(Logging.On)Logging.Enter(Logging.Web, this, "OpenReadAsync", address);
            if (address == null) 
                throw new ArgumentNullException("address");
            InitWebClientAsync();
            ClearWebClientState();
            AsyncOperation asyncOp = AsyncOperationManager.CreateOperation(userToken);
            m_AsyncOp = asyncOp;
            try {
                WebRequest request = m_WebRequest = GetWebRequest(GetUri(address));
                request.BeginGetResponse(new AsyncCallback(OpenReadAsyncCallback), asyncOp);
            } catch (Exception e) {
                if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException) {
                    throw;
                }
                if (!(e is WebException || e is SecurityException)) {
                    e = new WebException(SR.GetString(SR.net_webclient), e);
                }

                OpenReadCompletedEventArgs eventArgs = new OpenReadCompletedEventArgs(null, e, m_Cancelled, asyncOp.UserSuppliedState);
                InvokeOperationCompleted(asyncOp, openReadOperationCompleted, eventArgs);
            }
            if(Logging.On)Logging.Exit(Logging.Web, this, "OpenReadAsync", null);
        }

        //
        //OpenWrite
        //

        public event OpenWriteCompletedEventHandler OpenWriteCompleted;
        protected virtual void OnOpenWriteCompleted(OpenWriteCompletedEventArgs e) {
            if (OpenWriteCompleted != null) {
                OpenWriteCompleted(this, e);
            }
        }
        private SendOrPostCallback openWriteOperationCompleted;
        private void OpenWriteOperationCompleted(object arg) {
            OnOpenWriteCompleted((OpenWriteCompletedEventArgs)arg);
        }
        private void OpenWriteAsyncCallback(IAsyncResult result) {
            LazyAsyncResult lazyAsyncResult = (LazyAsyncResult) result;
            AsyncOperation asyncOp = (AsyncOperation) lazyAsyncResult.AsyncState;
            WebRequest request = (WebRequest) lazyAsyncResult.AsyncObject;
            WebClientWriteStream stream = null;
            Exception exception = null;

            try {
                stream =
                    new WebClientWriteStream(request.EndGetRequestStream(result), request, this);
            } catch (Exception e) {
                if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException) {
                    throw;
                }
                exception = e;
                if (!(e is WebException || e is SecurityException)) {
                    exception = new WebException(SR.GetString(SR.net_webclient), e);
                }
            }

            OpenWriteCompletedEventArgs eventArgs =
                new OpenWriteCompletedEventArgs(stream, exception, m_Cancelled, asyncOp.UserSuppliedState);
            InvokeOperationCompleted(asyncOp, openWriteOperationCompleted, eventArgs);
        }


        [HostProtection(ExternalThreading=true)]
        public void OpenWriteAsync(Uri address) {
            OpenWriteAsync(address, null, null);
        }

        [HostProtection(ExternalThreading=true)]
        public void OpenWriteAsync(Uri address, string method)
        {
            OpenWriteAsync(address, method, null);
        }

        [HostProtection(ExternalThreading=true)]
        public void OpenWriteAsync(Uri address, string method, object userToken)
        {
            if(Logging.On)Logging.Enter(Logging.Web, this, "OpenWriteAsync", address +", "+method);
            if (address == null) 
                throw new ArgumentNullException("address");
            if (method == null) {
                method = MapToDefaultMethod(address);
            }
            InitWebClientAsync();
            ClearWebClientState();
            AsyncOperation asyncOp = AsyncOperationManager.CreateOperation(userToken);
            m_AsyncOp = asyncOp;
            try {
                m_Method = method;
                WebRequest request = m_WebRequest = GetWebRequest(GetUri(address));
                request.BeginGetRequestStream(new AsyncCallback(OpenWriteAsyncCallback), asyncOp);
            } catch (Exception e) {
                if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException) {
                    throw;
                }
                if (!(e is WebException || e is SecurityException)) {
                    e = new WebException(SR.GetString(SR.net_webclient), e);
                }

                OpenWriteCompletedEventArgs eventArgs = new OpenWriteCompletedEventArgs(null, e, m_Cancelled, asyncOp.UserSuppliedState);
                InvokeOperationCompleted(asyncOp, openWriteOperationCompleted, eventArgs);
            }
            if(Logging.On)Logging.Exit(Logging.Web, this, "OpenWriteAsync", null);
        }

        //
        //DownloadString
        //

        public event DownloadStringCompletedEventHandler DownloadStringCompleted;
        protected virtual void OnDownloadStringCompleted(DownloadStringCompletedEventArgs e) {
            if (DownloadStringCompleted != null) {
                DownloadStringCompleted(this, e);
            }
        }
        private SendOrPostCallback downloadStringOperationCompleted;
        private void DownloadStringOperationCompleted(object arg) {
            OnDownloadStringCompleted((DownloadStringCompletedEventArgs)arg);
        }

        private void DownloadStringAsyncCallback(byte [] returnBytes, Exception exception, Object state) {

            AsyncOperation asyncOp = (AsyncOperation)state;
            string stringData = null;
            try {
                if (returnBytes != null) {
                    stringData = GetStringUsingEncoding(m_WebRequest, returnBytes);
                }
            } catch (Exception e) {
                if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException) {
                    throw;
                }
                exception = e;
            }

            DownloadStringCompletedEventArgs eventArgs =
                new DownloadStringCompletedEventArgs(stringData, exception, m_Cancelled, asyncOp.UserSuppliedState);

            InvokeOperationCompleted(asyncOp, downloadStringOperationCompleted, eventArgs);
        }

        [HostProtection(ExternalThreading=true)]
        public void DownloadStringAsync(Uri address)
        {
            DownloadStringAsync(address, null);
        }
        [HostProtection(ExternalThreading=true)]
        public void DownloadStringAsync(Uri address, object userToken)
        {
            if(Logging.On)Logging.Enter(Logging.Web, this, "DownloadStringAsync", address);
            if (address == null) 
                throw new ArgumentNullException("address");
            InitWebClientAsync();
            ClearWebClientState();
            AsyncOperation asyncOp = AsyncOperationManager.CreateOperation(userToken);
            m_AsyncOp = asyncOp;
            try {
                WebRequest request = m_WebRequest = GetWebRequest(GetUri(address));
                DownloadBits(request, null, new CompletionDelegate(DownloadStringAsyncCallback), asyncOp);
            } catch (Exception e) {
                if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException) {
                    throw;
                }
                if (!(e is WebException || e is SecurityException)) {
                    e = new WebException(SR.GetString(SR.net_webclient), e);
                }
                DownloadStringAsyncCallback(null, e, asyncOp);
            }
            if(Logging.On)Logging.Exit(Logging.Web, this, "DownloadStringAsync", "");
        }

        //
        //DownloadData
        //
        public event DownloadDataCompletedEventHandler DownloadDataCompleted;
        protected virtual void OnDownloadDataCompleted(DownloadDataCompletedEventArgs e) {
            if (DownloadDataCompleted != null) {
                DownloadDataCompleted(this, e);
            }
        }
        private SendOrPostCallback downloadDataOperationCompleted;
        private void DownloadDataOperationCompleted(object arg) {
            OnDownloadDataCompleted((DownloadDataCompletedEventArgs)arg);
        }

        private void DownloadDataAsyncCallback(byte [] returnBytes, Exception exception, Object state)
        {
            AsyncOperation asyncOp = (AsyncOperation)state;
            DownloadDataCompletedEventArgs eventArgs =
                new DownloadDataCompletedEventArgs(returnBytes, exception, m_Cancelled, asyncOp.UserSuppliedState);

            InvokeOperationCompleted(asyncOp, downloadDataOperationCompleted, eventArgs);
        }

        [HostProtection(ExternalThreading=true)]
        public void DownloadDataAsync(Uri address)
        {
            DownloadDataAsync(address, null);
        }

        [HostProtection(ExternalThreading=true)]
        public void DownloadDataAsync(Uri address, object userToken)
        {
            if(Logging.On)Logging.Enter(Logging.Web, this, "DownloadDataAsync", address);
            if (address == null) 
                throw new ArgumentNullException("address");
            InitWebClientAsync();
            ClearWebClientState();
            AsyncOperation asyncOp = AsyncOperationManager.CreateOperation(userToken);
            m_AsyncOp = asyncOp;
            try {
                WebRequest request = m_WebRequest = GetWebRequest(GetUri(address));
                DownloadBits(request, null, new CompletionDelegate(DownloadDataAsyncCallback), asyncOp);
            } catch (Exception e) {
                if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException) {
                    throw;
                }
                if (!(e is WebException || e is SecurityException)) {
                    e = new WebException(SR.GetString(SR.net_webclient), e);
                }
                DownloadDataAsyncCallback(null, e, asyncOp);
            }
            if(Logging.On)Logging.Exit(Logging.Web, this, "DownloadDataAsync", null);
        }

        //
        //DownloadFile
        //

        public event AsyncCompletedEventHandler DownloadFileCompleted;
        protected virtual void OnDownloadFileCompleted(AsyncCompletedEventArgs e) {
            if (DownloadFileCompleted != null) {
                DownloadFileCompleted(this, e);
            }
        }
        private SendOrPostCallback downloadFileOperationCompleted;
        private void DownloadFileOperationCompleted(object arg) {
            OnDownloadFileCompleted((AsyncCompletedEventArgs)arg);
        }

        private void DownloadFileAsyncCallback(byte [] returnBytes, Exception exception, Object state) {

            AsyncOperation asyncOp = (AsyncOperation)state; 
            AsyncCompletedEventArgs eventArgs =
                new AsyncCompletedEventArgs(exception, m_Cancelled, asyncOp.UserSuppliedState);

            InvokeOperationCompleted(asyncOp, downloadFileOperationCompleted, eventArgs);
        }


        [HostProtection(ExternalThreading=true)]
        public void DownloadFileAsync(Uri address, string fileName)
        {
            DownloadFileAsync(address, fileName, null);
        }
        [HostProtection(ExternalThreading=true)]
        public void DownloadFileAsync(Uri address, string fileName, object userToken)
        {
            if(Logging.On)Logging.Enter(Logging.Web, this, "DownloadFileAsync", address);
            if (address == null) 
                throw new ArgumentNullException("address");
            if (fileName == null) 
                throw new ArgumentNullException("fileName");
            FileStream fs = null;
            InitWebClientAsync();
            ClearWebClientState();
            AsyncOperation asyncOp = AsyncOperationManager.CreateOperation(userToken);
            m_AsyncOp = asyncOp;
            try {
                fs = new FileStream(fileName, FileMode.Create, FileAccess.Write);
                WebRequest request = m_WebRequest = GetWebRequest(GetUri(address));
                DownloadBits(request, fs, new CompletionDelegate(DownloadFileAsyncCallback), asyncOp);
            } catch (Exception e) {
                if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException) {
                    throw;
                }
                if(fs != null){
                    fs.Close();
                }
                if (!(e is WebException || e is SecurityException)) {
                    e = new WebException(SR.GetString(SR.net_webclient), e);
                }
                DownloadFileAsyncCallback(null, e, asyncOp);
            }
            if(Logging.On)Logging.Exit(Logging.Web, this, "DownloadFileAsync", null);
        }

        //
        //UploadString
        //

        public event UploadStringCompletedEventHandler UploadStringCompleted;
        protected virtual void OnUploadStringCompleted(UploadStringCompletedEventArgs e) {
            if (UploadStringCompleted != null) {
                UploadStringCompleted(this, e);
            }
        }
        private SendOrPostCallback uploadStringOperationCompleted;
        private void UploadStringOperationCompleted(object arg) {
            OnUploadStringCompleted((UploadStringCompletedEventArgs)arg);
        }

        private void StartDownloadAsync(UploadBitsState state)
        {
            try
            {
                DownloadBits(state.Request, null, state.DownloadCompletionDelegate, state.AsyncOp);
            }
            catch (Exception e)
            {
                if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException) {
                    throw;
                }
                if (!(e is WebException || e is SecurityException)) {
                    e = new WebException(SR.GetString(SR.net_webclient), e);
                }
                state.DownloadCompletionDelegate(null, e, state.AsyncOp);
            }            
        }

        private void UploadStringAsyncWriteCallback(byte [] returnBytes, Exception exception, Object state) {
            UploadBitsState uploadState = (UploadBitsState)state;

            if (exception != null){
                UploadStringCompletedEventArgs eventArgs =
                    new UploadStringCompletedEventArgs(null, exception, m_Cancelled, uploadState.AsyncOp.UserSuppliedState);

                InvokeOperationCompleted(uploadState.AsyncOp, uploadStringOperationCompleted, eventArgs);
            } else {
                StartDownloadAsync(uploadState);
            }
        }

        private void UploadStringAsyncReadCallback(byte [] returnBytes, Exception exception, Object state) {
            AsyncOperation asyncOp = (AsyncOperation)state; 
            string stringData = null;

            try {
                if (returnBytes != null) {
                    stringData = GetStringUsingEncoding(m_WebRequest, returnBytes);
                }
            } catch (Exception e) {
                if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException) {
                    throw;
                }
                exception = e;
            }

            UploadStringCompletedEventArgs eventArgs =
                new UploadStringCompletedEventArgs(stringData, exception, m_Cancelled, asyncOp.UserSuppliedState);

            InvokeOperationCompleted(asyncOp, uploadStringOperationCompleted, eventArgs);
        }

        [HostProtection(ExternalThreading=true)]
        public void UploadStringAsync(Uri address, string data) {
            UploadStringAsync(address, null, data, null);
        }

        [HostProtection(ExternalThreading=true)]
        public void UploadStringAsync(Uri address, string method, string data)
        {
            UploadStringAsync(address, method, data, null);
        }

        [HostProtection(ExternalThreading=true)]
        public void UploadStringAsync(Uri address, string method, string data, object userToken)
        {
            if(Logging.On)Logging.Enter(Logging.Web, this, "UploadStringAsync", address);
            if (address == null) 
                throw new ArgumentNullException("address");
            if (data == null) 
                throw new ArgumentNullException("data");
            if (method == null) {
                method = MapToDefaultMethod(address);
            }
            InitWebClientAsync();
            ClearWebClientState();
            AsyncOperation asyncOp = AsyncOperationManager.CreateOperation(userToken);
            m_AsyncOp = asyncOp;
            try {
                byte [] requestData = Encoding.GetBytes(data);
                m_Method = method;
                m_ContentLength = requestData.Length;
                WebRequest request = m_WebRequest = GetWebRequest(GetUri(address));

                //
                // Start async upload. Download will start after upload completes.
                //
                UploadBits(request, null, requestData, 0, null, null, new CompletionDelegate(UploadStringAsyncWriteCallback), 
                    new CompletionDelegate(UploadStringAsyncReadCallback), asyncOp);                
            } catch (Exception e) {
                if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException) {
                    throw;
                }
                if (!(e is WebException || e is SecurityException)) {
                    e = new WebException(SR.GetString(SR.net_webclient), e);
                }

                UploadStringCompletedEventArgs eventArgs =
                    new UploadStringCompletedEventArgs(null, e, m_Cancelled, asyncOp.UserSuppliedState);
                InvokeOperationCompleted(asyncOp, uploadStringOperationCompleted, eventArgs);
            }
            if(Logging.On)Logging.Exit(Logging.Web, this, "UploadStringAsync", null);
        }

        //
        //UploadData
        //

        public event UploadDataCompletedEventHandler UploadDataCompleted;
        protected virtual void OnUploadDataCompleted(UploadDataCompletedEventArgs e) {
            if (UploadDataCompleted != null) {
                UploadDataCompleted(this, e);
            }
        }
        private SendOrPostCallback uploadDataOperationCompleted;
        private void UploadDataOperationCompleted(object arg) {
            OnUploadDataCompleted((UploadDataCompletedEventArgs)arg);
        }


        private void UploadDataAsyncWriteCallback(byte [] returnBytes, Exception exception, Object state) {            
            UploadBitsState uploadState = (UploadBitsState)state;

            if (exception != null){
                UploadDataCompletedEventArgs eventArgs =
                    new UploadDataCompletedEventArgs(returnBytes, exception, m_Cancelled, uploadState.AsyncOp.UserSuppliedState);

                InvokeOperationCompleted(uploadState.AsyncOp, uploadDataOperationCompleted, eventArgs);
            } else {
                StartDownloadAsync(uploadState);
            }        
        }

        private void UploadDataAsyncReadCallback(byte [] returnBytes, Exception exception, Object state) {
            AsyncOperation asyncOp = (AsyncOperation)state; 
            UploadDataCompletedEventArgs eventArgs =
                new UploadDataCompletedEventArgs(returnBytes, exception, m_Cancelled, asyncOp.UserSuppliedState);

            InvokeOperationCompleted(asyncOp, uploadDataOperationCompleted, eventArgs);
        }



        [HostProtection(ExternalThreading=true)]
        public void UploadDataAsync(Uri address, byte[] data) {
            UploadDataAsync(address, null, data, null);
        }

        [HostProtection(ExternalThreading=true)]
        public void UploadDataAsync(Uri address, string method, byte[] data)
        {
            UploadDataAsync(address, method, data, null);
        }

        [HostProtection(ExternalThreading=true)]
        public void UploadDataAsync(Uri address, string method, byte[] data, object userToken)
        {
            if(Logging.On)Logging.Enter(Logging.Web, this, "UploadDataAsync", address +", "+method);
            if (address == null) 
                throw new ArgumentNullException("address");
            if (data == null) 
                throw new ArgumentNullException("data");
            if (method == null) {
                method = MapToDefaultMethod(address);
            }
            InitWebClientAsync();
            ClearWebClientState();
            AsyncOperation asyncOp = AsyncOperationManager.CreateOperation(userToken);
            m_AsyncOp = asyncOp;
            int chunkSize = 0;
            try {
                m_Method = method;
                m_ContentLength = data.Length;
                WebRequest request = m_WebRequest = GetWebRequest(GetUri(address));

                //
                // Start async upload. Download will start after upload completes.
                //
                if (UploadProgressChanged != null) {
                    // If ProgressCallback is requested, we should send the buffer in chunks
                    chunkSize = (int)Math.Min((long)DefaultCopyBufferLength, data.Length);
                }

                UploadBits(request, null, data, chunkSize, null, null, new CompletionDelegate(UploadDataAsyncWriteCallback), 
                    new CompletionDelegate(UploadDataAsyncReadCallback), asyncOp);
            } catch (Exception e) {
                if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException) {
                    throw;
                }
                if (!(e is WebException || e is SecurityException)) {
                    e = new WebException(SR.GetString(SR.net_webclient), e);
                }

                UploadDataCompletedEventArgs eventArgs =
                    new UploadDataCompletedEventArgs(null, e, m_Cancelled, asyncOp.UserSuppliedState);
                InvokeOperationCompleted(asyncOp, uploadDataOperationCompleted, eventArgs);
            }
            if(Logging.On)Logging.Exit(Logging.Web, this, "UploadDataAsync", null);
        }

        //
        //UploadFile
        //

        public event UploadFileCompletedEventHandler UploadFileCompleted;
        protected virtual void OnUploadFileCompleted(UploadFileCompletedEventArgs e) {
            if (UploadFileCompleted != null) {
                UploadFileCompleted(this, e);
            }
        }
        private SendOrPostCallback uploadFileOperationCompleted;
        private void UploadFileOperationCompleted(object arg) {
            OnUploadFileCompleted((UploadFileCompletedEventArgs)arg);
        }

        private void UploadFileAsyncWriteCallback(byte[] returnBytes, Exception exception, Object state) {
            UploadBitsState uploadState = (UploadBitsState)state;

            if (exception != null) {
                UploadFileCompletedEventArgs eventArgs =
                    new UploadFileCompletedEventArgs(returnBytes, exception, m_Cancelled, uploadState.AsyncOp.UserSuppliedState);

                InvokeOperationCompleted(uploadState.AsyncOp, uploadFileOperationCompleted, eventArgs);
            } else {
                StartDownloadAsync(uploadState);
            }
        }

        private void UploadFileAsyncReadCallback(byte[] returnBytes, Exception exception, Object state)
        {
            AsyncOperation asyncOp = (AsyncOperation)state; 
            UploadFileCompletedEventArgs eventArgs =
                new UploadFileCompletedEventArgs(returnBytes, exception, m_Cancelled, asyncOp.UserSuppliedState);

            InvokeOperationCompleted(asyncOp, uploadFileOperationCompleted, eventArgs);
        }


        [HostProtection(ExternalThreading=true)]
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public void UploadFileAsync(Uri address, string fileName) {
            UploadFileAsync(address, null, fileName, null);
        }

        [HostProtection(ExternalThreading=true)]
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public void UploadFileAsync(Uri address, string method, string fileName)
        {
            UploadFileAsync(address, method, fileName, null);
        }

        [HostProtection(ExternalThreading=true)]
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public void UploadFileAsync(Uri address, string method, string fileName, object userToken)
        {
            if(Logging.On)Logging.Enter(Logging.Web, this, "UploadFileAsync", address +", "+method);
            if (address == null) 
                throw new ArgumentNullException("address");
            if (fileName == null) 
                throw new ArgumentNullException("fileName");
            if (method == null) {
                method = MapToDefaultMethod(address);
            }
            InitWebClientAsync();
            ClearWebClientState();
            AsyncOperation asyncOp = AsyncOperationManager.CreateOperation(userToken);
            m_AsyncOp = asyncOp;
            FileStream fs = null;

            try {
                m_Method = method;
                byte [] formHeaderBytes = null, boundaryBytes = null, buffer = null;
                Uri uri = GetUri(address);
                bool needsHeaderAndBoundary = (uri.Scheme != Uri.UriSchemeFile);
                OpenFileInternal(needsHeaderAndBoundary, fileName, ref fs, ref buffer, ref formHeaderBytes, ref boundaryBytes);
                WebRequest request = m_WebRequest = GetWebRequest(uri);

                //
                // Start async upload. Download will start after upload completes.
                //
                UploadBits(request, fs, buffer, 0, formHeaderBytes, boundaryBytes, new CompletionDelegate(UploadFileAsyncWriteCallback), 
                    new CompletionDelegate(UploadFileAsyncReadCallback), asyncOp);
            } catch (Exception e) {
                if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException) {
                    throw;
                }
                if(fs != null){
                   fs.Close();
                }
                if (!(e is WebException || e is SecurityException)) {
                    e = new WebException(SR.GetString(SR.net_webclient), e);
                }

                UploadFileCompletedEventArgs eventArgs =
                    new UploadFileCompletedEventArgs(null, e, m_Cancelled, asyncOp.UserSuppliedState);
                InvokeOperationCompleted(asyncOp, uploadFileOperationCompleted, eventArgs);
            }
            if(Logging.On)Logging.Exit(Logging.Web, this, "UploadFileAsync", null);
        }


        //
        //UploadValues
        //

        public event UploadValuesCompletedEventHandler UploadValuesCompleted;
        protected virtual void OnUploadValuesCompleted(UploadValuesCompletedEventArgs e) {
            if (UploadValuesCompleted != null) {
                UploadValuesCompleted(this, e);
            }
        }
        private SendOrPostCallback uploadValuesOperationCompleted;
        private void UploadValuesOperationCompleted(object arg) {
            OnUploadValuesCompleted((UploadValuesCompletedEventArgs)arg);
        }


        private void UploadValuesAsyncWriteCallback(byte [] returnBytes, Exception exception, Object state) {
            UploadBitsState uploadState = (UploadBitsState)state;

            if (exception != null) {
                UploadValuesCompletedEventArgs eventArgs =
                    new UploadValuesCompletedEventArgs(returnBytes, exception, m_Cancelled, uploadState.AsyncOp.UserSuppliedState);

                InvokeOperationCompleted(uploadState.AsyncOp, uploadValuesOperationCompleted, eventArgs);
            } else {
                StartDownloadAsync(uploadState);
            }
        }

        private void UploadValuesAsyncReadCallback(byte [] returnBytes, Exception exception, Object state) {
            AsyncOperation asyncOp = (AsyncOperation)state; 
            UploadValuesCompletedEventArgs eventArgs =
                new UploadValuesCompletedEventArgs(returnBytes, exception, m_Cancelled, asyncOp.UserSuppliedState);

            InvokeOperationCompleted(asyncOp, uploadValuesOperationCompleted, eventArgs);
        }


        [HostProtection(ExternalThreading=true)]
        public void UploadValuesAsync(Uri address, NameValueCollection data) {
            UploadValuesAsync(address, null, data, null);
        }

        [HostProtection(ExternalThreading=true)]
        public void UploadValuesAsync(Uri address, string method, NameValueCollection data)
        {
            UploadValuesAsync(address, method, data, null);
        }

        [HostProtection(ExternalThreading=true)]
        public void UploadValuesAsync(Uri address, string method, NameValueCollection data, object userToken)
        {
            if(Logging.On)Logging.Enter(Logging.Web, this, "UploadValuesAsync", address +", "+method);
            if (address == null) 
                throw new ArgumentNullException("address");
            if (data == null) 
                throw new ArgumentNullException("data");
            if (method == null) {
                method = MapToDefaultMethod(address);
            }
            InitWebClientAsync();
            ClearWebClientState();
            AsyncOperation asyncOp = AsyncOperationManager.CreateOperation(userToken);
            m_AsyncOp = asyncOp;
            int chunkSize = 0;
            try {
                byte[] buffer = UploadValuesInternal(data);
                m_Method = method;
                WebRequest request = m_WebRequest = GetWebRequest(GetUri(address));
                
                //
                // Start async upload. Download will start after upload completes.
                //
                if (UploadProgressChanged != null) {
                    // If ProgressCallback is requested, we should send the buffer in chunks
                    chunkSize = (int)Math.Min((long)DefaultCopyBufferLength, buffer.Length);
                }

                UploadBits(request, null, buffer, chunkSize, null, null, new CompletionDelegate(UploadValuesAsyncWriteCallback),
                    new CompletionDelegate(UploadValuesAsyncReadCallback), asyncOp);
            } catch (Exception e) {
                if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException) {
                    throw;
                }
                if (!(e is WebException || e is SecurityException)) {
                    e = new WebException(SR.GetString(SR.net_webclient), e);
                }

                UploadValuesCompletedEventArgs eventArgs =
                    new UploadValuesCompletedEventArgs(null, e, m_Cancelled, asyncOp.UserSuppliedState);
                InvokeOperationCompleted(asyncOp, uploadValuesOperationCompleted, eventArgs);
            }
            if(Logging.On)Logging.Exit(Logging.Web, this, "UploadValuesAsync", null);
        }


        public void CancelAsync() {
            WebRequest request = m_WebRequest;
            m_Cancelled = true;
            AbortRequest(request);
        }

        //************* Task-based async public methods *************************
        [HostProtection(ExternalThreading = true)]
        [ComVisible(false)]
        public Task<string> DownloadStringTaskAsync(string address)
        {
            return DownloadStringTaskAsync(this.GetUri(address));
        }

        [HostProtection(ExternalThreading = true)]
        [ComVisible(false)]
        public Task<string> DownloadStringTaskAsync(Uri address)
        {
            // Create the task to be returned
            var tcs = new TaskCompletionSource<string>(address);

            DownloadStringCompletedEventHandler handler = null;
            handler = (sender, e) => HandleCompletion(tcs, e, (args) => args.Result, handler, (webClient, completion) => webClient.DownloadStringCompleted -= completion);
            this.DownloadStringCompleted += handler;

            // Start the async operation.
            try { this.DownloadStringAsync(address, tcs); }
            catch
            {
                this.DownloadStringCompleted -= handler;
                throw;
            }

            // Return the task that represents the async operation
            return tcs.Task;
        }

        [HostProtection(ExternalThreading = true)]
        [ComVisible(false)]
        public Task<Stream> OpenReadTaskAsync(string address)
        {
            return OpenReadTaskAsync(this.GetUri(address));
        }

        [HostProtection(ExternalThreading = true)]
        [ComVisible(false)]
        public Task<Stream> OpenReadTaskAsync(Uri address)
        {
            // Create the task to be returned
            var tcs = new TaskCompletionSource<Stream>(address);

            // Setup the callback event handler
            OpenReadCompletedEventHandler handler = null;
            handler = (sender, e) => HandleCompletion(tcs, e, (args) => args.Result, handler, (webClient, completion) => webClient.OpenReadCompleted -= completion);
            this.OpenReadCompleted += handler;

            // Start the async operation.
            try { this.OpenReadAsync(address, tcs); }
            catch
            {
                this.OpenReadCompleted -= handler;
                throw;
            }

            // Return the task that represents the async operation
            return tcs.Task;
        }

        [HostProtection(ExternalThreading = true)]
        [ComVisible(false)]
        public Task<Stream> OpenWriteTaskAsync(string address)
        {
            return OpenWriteTaskAsync(this.GetUri(address), null);
        }

        [HostProtection(ExternalThreading = true)]
        [ComVisible(false)]
        public Task<Stream> OpenWriteTaskAsync(Uri address)
        {
            return OpenWriteTaskAsync(address, null);
        }

        [HostProtection(ExternalThreading = true)]
        [ComVisible(false)]
        public Task<Stream> OpenWriteTaskAsync(string address, string method)
        {
            return OpenWriteTaskAsync(this.GetUri(address), method);
        }

        [HostProtection(ExternalThreading = true)]
        [ComVisible(false)]
        public Task<Stream> OpenWriteTaskAsync(Uri address, string method)
        {
            // Create the task to be returned
            var tcs = new TaskCompletionSource<Stream>(address);

            // Setup the callback event handler
            OpenWriteCompletedEventHandler handler = null;
            handler = (sender, e) => HandleCompletion(tcs, e, (args) => args.Result, handler, (webClient, completion) => webClient.OpenWriteCompleted -= completion);
            this.OpenWriteCompleted += handler;

            // Start the async operation.
            try { this.OpenWriteAsync(address, method, tcs); }
            catch
            {
                this.OpenWriteCompleted -= handler;
                throw;
            }

            // Return the task that represents the async operation
            return tcs.Task;
        }

        [HostProtection(ExternalThreading = true)]
        [ComVisible(false)]
        [SuppressMessage("Microsoft.Design", "CA1057:StringUriOverloadsCallSystemUriOverloads",
            Justification = "This class uses internal Uri conversion routine")]
        public Task<string> UploadStringTaskAsync(string address, string data)
        {
            return UploadStringTaskAsync(address, null, data);
        }

        [HostProtection(ExternalThreading = true)]
        [ComVisible(false)]
        public Task<string> UploadStringTaskAsync(Uri address, string data)
        {
            return UploadStringTaskAsync(address, null, data);
        }

        [HostProtection(ExternalThreading = true)]
        [ComVisible(false)]
        [SuppressMessage("Microsoft.Design", "CA1057:StringUriOverloadsCallSystemUriOverloads",
            Justification = "This class uses internal Uri conversion routine")]
        public Task<string> UploadStringTaskAsync(string address, string method, string data)
        {
            return UploadStringTaskAsync(this.GetUri(address), method, data);
        }

        [HostProtection(ExternalThreading = true)]
        [ComVisible(false)]
        public Task<string> UploadStringTaskAsync(Uri address, string method, string data)
        {
            // Create the task to be returned
            var tcs = new TaskCompletionSource<string>(address);

            // Setup the callback event handler
            UploadStringCompletedEventHandler handler = null;
            handler = (sender, e) => HandleCompletion(tcs, e, (args) => args.Result, handler, (webClient, completion) => webClient.UploadStringCompleted -= completion);
            this.UploadStringCompleted += handler;

            // Start the async operation.
            try { this.UploadStringAsync(address, method, data, tcs); }
            catch
            {
                this.UploadStringCompleted -= handler;
                throw;
            }

            // Return the task that represents the async operation
            return tcs.Task;
        }

        [HostProtection(ExternalThreading = true)]
        [ComVisible(false)]
        public Task<byte[]> DownloadDataTaskAsync(string address)
        {
            return DownloadDataTaskAsync(this.GetUri(address));
        }

        [HostProtection(ExternalThreading = true)]
        [ComVisible(false)]
        public Task<byte[]> DownloadDataTaskAsync(Uri address)
        {
            // Create the task to be returned
            var tcs = new TaskCompletionSource<byte[]>(address);

            // Setup the callback event handler
            DownloadDataCompletedEventHandler handler = null;
            handler = (sender, e) => HandleCompletion(tcs, e, (args) => args.Result, handler, (webClient, completion) => webClient.DownloadDataCompleted -= completion);
            this.DownloadDataCompleted += handler;

            // Start the async operation.
            try { this.DownloadDataAsync(address, tcs); }
            catch
            {
                this.DownloadDataCompleted -= handler;
                throw;
            }

            // Return the task that represents the async operation
            return tcs.Task;
        }

        [HostProtection(ExternalThreading = true)]
        [ComVisible(false)]
        public Task DownloadFileTaskAsync(string address, string fileName)
        {
            return DownloadFileTaskAsync(this.GetUri(address), fileName);
        }

        [HostProtection(ExternalThreading = true)]
        [ComVisible(false)]
        public Task DownloadFileTaskAsync(Uri address, string fileName)
        {
            // Create the task to be returned
            var tcs = new TaskCompletionSource<object>(address);

            // Setup the callback event handler
            AsyncCompletedEventHandler handler = null;
            handler = (sender, e) => HandleCompletion(tcs, e, (args) => null, handler, (webClient, completion) => webClient.DownloadFileCompleted -= completion);
            this.DownloadFileCompleted += handler;

            // Start the async operation.
            try { this.DownloadFileAsync(address, fileName, tcs); }
            catch
            {
                this.DownloadFileCompleted -= handler;
                throw;
            }

            // Return the task that represents the async operation
            return tcs.Task;
        }

        [HostProtection(ExternalThreading = true)]
        [ComVisible(false)]
        [SuppressMessage("Microsoft.Design", "CA1057:StringUriOverloadsCallSystemUriOverloads", 
            Justification = "This class uses internal Uri conversion routine")]
        public Task<byte[]> UploadDataTaskAsync(string address, byte[] data)
        {
            return UploadDataTaskAsync(this.GetUri(address), null, data);
        }

        [HostProtection(ExternalThreading = true)]
        [ComVisible(false)]
        public Task<byte[]> UploadDataTaskAsync(Uri address, byte[] data)
        {
            return UploadDataTaskAsync(address, null, data);
        }

        [HostProtection(ExternalThreading = true)]
        [ComVisible(false)]
        [SuppressMessage("Microsoft.Design", "CA1057:StringUriOverloadsCallSystemUriOverloads",
            Justification = "We do exactly the rule suggests, but we use our internal routine")]
        public Task<byte[]> UploadDataTaskAsync(string address, string method, byte[] data)
        {
            return UploadDataTaskAsync(this.GetUri(address), method, data);
        }

        [HostProtection(ExternalThreading = true)]
        [ComVisible(false)]
        public Task<byte[]> UploadDataTaskAsync(Uri address, string method, byte[] data)
        {
            // Create the task to be returned
            var tcs = new TaskCompletionSource<byte[]>(address);

            // Setup the callback event handler
            UploadDataCompletedEventHandler handler = null;
            handler = (sender, e) => HandleCompletion(tcs, e, (args) => args.Result, handler, (webClient, completion) => webClient.UploadDataCompleted -= completion);
            this.UploadDataCompleted += handler;

            // Start the async operation.
            try { this.UploadDataAsync(address, method, data, tcs); }
            catch
            {
                this.UploadDataCompleted -= handler;
                throw;
            }

            // Return the task that represents the async operation
            return tcs.Task;
        }

        [HostProtection(ExternalThreading = true)]
        [ComVisible(false)]
        [SuppressMessage("Microsoft.Design", "CA1057:StringUriOverloadsCallSystemUriOverloads",
            Justification = "This class uses internal Uri conversion routine")]
        public Task<byte[]> UploadFileTaskAsync(string address, string fileName)
        {
            return UploadFileTaskAsync(this.GetUri(address), null, fileName);
        }

        [HostProtection(ExternalThreading = true)]
        [ComVisible(false)]
        public Task<byte[]> UploadFileTaskAsync(Uri address, string fileName)
        {
            return UploadFileTaskAsync(address, null, fileName);
        }

        [HostProtection(ExternalThreading = true)]
        [ComVisible(false)]
        [SuppressMessage("Microsoft.Design", "CA1057:StringUriOverloadsCallSystemUriOverloads",
            Justification = "This class uses internal Uri conversion routine")]
        public Task<byte[]> UploadFileTaskAsync(string address, string method, string fileName)
        {
            return UploadFileTaskAsync(this.GetUri(address), method, fileName);
        }

        [HostProtection(ExternalThreading = true)]
        [ComVisible(false)]
        public Task<byte[]> UploadFileTaskAsync(Uri address, string method, string fileName)
        {
            // Create the task to be returned
            var tcs = new TaskCompletionSource<byte[]>(address);

            // Setup the callback event handler
            UploadFileCompletedEventHandler handler = null;
            handler = (sender, e) => HandleCompletion(tcs, e, (args) => args.Result, handler, (webClient, completion) => webClient.UploadFileCompleted -= completion);
            this.UploadFileCompleted += handler;

            // Start the async operation.
            try { this.UploadFileAsync(address, method, fileName, tcs); }
            catch
            {
                this.UploadFileCompleted -= handler;
                throw;
            }

            // Return the task that represents the async operation
            return tcs.Task;
        }


        [HostProtection(ExternalThreading = true)]
        [ComVisible(false)]
        [SuppressMessage("Microsoft.Design", "CA1057:StringUriOverloadsCallSystemUriOverloads",
            Justification = "This class uses internal Uri conversion routine")]
        public Task<byte[]> UploadValuesTaskAsync(string address, NameValueCollection data)
        {
            return UploadValuesTaskAsync(this.GetUri(address), null, data);
        }

        [HostProtection(ExternalThreading = true)]
        [ComVisible(false)]
        [SuppressMessage("Microsoft.Design", "CA1057:StringUriOverloadsCallSystemUriOverloads",
            Justification = "We do exactly the rule suggests, but we use our internal routine")]
        public Task<byte[]> UploadValuesTaskAsync(string address, string method, NameValueCollection data)
        {
            return UploadValuesTaskAsync(this.GetUri(address), method, data);
        }


        [HostProtection(ExternalThreading = true)]
        [ComVisible(false)]
        public Task<byte[]> UploadValuesTaskAsync(Uri address, NameValueCollection data)
        {
            return UploadValuesTaskAsync(address, null, data);
        }


        [HostProtection(ExternalThreading = true)]
        [ComVisible(false)]
        public Task<byte[]> UploadValuesTaskAsync(Uri address, string method, NameValueCollection data)
        {
            // Create the task to be returned
            var tcs = new TaskCompletionSource<byte[]>(address);

            // Setup the callback event handler
            UploadValuesCompletedEventHandler handler = null;
            handler = (sender, e) => HandleCompletion(tcs, e, (args) => args.Result, handler, (webClient, completion) => webClient.UploadValuesCompleted -= completion);
            this.UploadValuesCompleted += handler;

            // Start the async operation.
            try { this.UploadValuesAsync(address, method, data, tcs); }
            catch
            {
                this.UploadValuesCompleted -= handler;
                throw;
            }

            // Return the task that represents the async operation
            return tcs.Task;
        }


        private void HandleCompletion<TAsyncCompletedEventArgs, TCompletionDelegate, T>(TaskCompletionSource<T> tcs, TAsyncCompletedEventArgs e, Func<TAsyncCompletedEventArgs, T> getResult, TCompletionDelegate handler, Action<WebClient, TCompletionDelegate> unregisterHandler)
            where TAsyncCompletedEventArgs : AsyncCompletedEventArgs
        {
            if (e.UserState == tcs)
            {
                try { unregisterHandler(this, handler); }
                finally
                {
                    if (e.Error != null) tcs.TrySetException(e.Error);
                    else if (e.Cancelled) tcs.TrySetCanceled();
                    else tcs.TrySetResult(getResult(e));
                }
            }
        }

        //
        //ProgressChanged event - code for handling progress updates during uploads and downloads.
        //

        public event DownloadProgressChangedEventHandler DownloadProgressChanged;
        public event UploadProgressChangedEventHandler UploadProgressChanged;

        protected virtual void OnDownloadProgressChanged(DownloadProgressChangedEventArgs e) {
            if (DownloadProgressChanged != null) {
                DownloadProgressChanged(this, e);
            }
        }

        protected virtual void OnUploadProgressChanged(UploadProgressChangedEventArgs e) {
            if (UploadProgressChanged != null) {
                UploadProgressChanged(this, e);
            }
        }

        private SendOrPostCallback reportDownloadProgressChanged;
        private void ReportDownloadProgressChanged(object arg) {
            OnDownloadProgressChanged((DownloadProgressChangedEventArgs) arg);
        }

        private SendOrPostCallback reportUploadProgressChanged;
        private void ReportUploadProgressChanged(object arg) {
            OnUploadProgressChanged((UploadProgressChangedEventArgs) arg);
        }

        private void PostProgressChanged(AsyncOperation asyncOp, ProgressData progress) {
            if (asyncOp != null && progress.BytesSent + progress.BytesReceived > 0)
            {
                int progressPercentage;
                if (progress.HasUploadPhase)
                {
                    if (progress.TotalBytesToReceive < 0 && progress.BytesReceived == 0)
                    {
                        progressPercentage = progress.TotalBytesToSend < 0 ? 0 : progress.TotalBytesToSend == 0 ? 50 : (int)((50 * progress.BytesSent) / progress.TotalBytesToSend);
                    }
                    else
                    {
                        progressPercentage = progress.TotalBytesToSend < 0 ? 50 : progress.TotalBytesToReceive == 0 ? 100 : (int) ((50 * progress.BytesReceived) / progress.TotalBytesToReceive + 50);
                    }
                    asyncOp.Post(reportUploadProgressChanged, new UploadProgressChangedEventArgs(progressPercentage, asyncOp.UserSuppliedState, progress.BytesSent, progress.TotalBytesToSend, progress.BytesReceived, progress.TotalBytesToReceive));
                }
                else
                {
                    progressPercentage = progress.TotalBytesToReceive < 0 ? 0 : progress.TotalBytesToReceive == 0 ? 100 : (int) ((100 * progress.BytesReceived) / progress.TotalBytesToReceive);
                    asyncOp.Post(reportDownloadProgressChanged, new DownloadProgressChangedEventArgs(progressPercentage, asyncOp.UserSuppliedState, progress.BytesReceived, progress.TotalBytesToReceive));
                }
            }
        }


        //
        // WebClientWriteStream
        //
        private class WebClientWriteStream : Stream {

            private WebRequest m_request;
            private Stream m_stream;
            private WebClient m_WebClient;

            public WebClientWriteStream(Stream stream, WebRequest request, WebClient webClient) {
                m_request = request;
                m_stream = stream;
                m_WebClient = webClient;
            }

            public override bool CanRead {
                get {
                    return m_stream.CanRead;
                }
            }

            public override bool CanSeek {
                get {
                    return m_stream.CanSeek;
                }
            }

            public override bool CanWrite {
                get {
                    return m_stream.CanWrite;
                }
            }

            public override bool CanTimeout {
                get {
                    return m_stream.CanTimeout;
                }
            }

            public override int ReadTimeout {
                get {
                    return m_stream.ReadTimeout;
                }
                set {
                    m_stream.ReadTimeout = value;
                }
            }

            public override int WriteTimeout {
                get {
                    return m_stream.WriteTimeout;
                }
                set {
                    m_stream.WriteTimeout = value;
                }
            }

            public override long Length {
                get {
                    return m_stream.Length;
                }
            }

            public override long Position {
                get {
                    return m_stream.Position;
                }
                set {
                    m_stream.Position = value;
                }
            }

            [HostProtection(ExternalThreading=true)]
            public override IAsyncResult BeginRead(byte[] buffer, int offset, int size, AsyncCallback callback, object state) {
                return m_stream.BeginRead(buffer, offset, size, callback, state);
            }

            [HostProtection(ExternalThreading=true)]
            public override IAsyncResult BeginWrite(byte[] buffer, int offset, int size, AsyncCallback callback, object state ) {
                return m_stream.BeginWrite(buffer, offset, size, callback, state);
            }

            protected override void Dispose(bool disposing) {
                try {
                    if (disposing) {
                        m_stream.Close();
                        m_WebClient.GetWebResponse(m_request).Close();
                    }
                }
                finally {
                    base.Dispose(disposing);
                }
            }

            public override int EndRead(IAsyncResult result) {
                return m_stream.EndRead(result);
            }

            public override void EndWrite(IAsyncResult result) {
                m_stream.EndWrite(result);
            }

            public override void Flush() {
                m_stream.Flush();
            }

            public override int Read(byte[] buffer, int offset, int count) {
                return m_stream.Read(buffer, offset, count);
            }

            public override long Seek(long offset, SeekOrigin origin) {
                return m_stream.Seek(offset, origin);
            }

            public override void SetLength(long value) {
                m_stream.SetLength(value);
            }

            public override void Write(byte[] buffer, int offset, int count) {
                m_stream.Write(buffer, offset, count);
            }
        }

    }

    //
    // Delegates and supporting CompletedEventArgs classes are used by async code
    //

    // Used by internal Async code to notify that we're done, or have an error
    internal delegate void CompletionDelegate(byte [] responseBytes, Exception exception, Object State);

    public delegate void OpenReadCompletedEventHandler(object sender, OpenReadCompletedEventArgs e);

    public class OpenReadCompletedEventArgs : AsyncCompletedEventArgs {
        private Stream m_Result;
        internal OpenReadCompletedEventArgs(Stream result, Exception exception, bool cancelled, object userToken) :
            base(exception, cancelled, userToken) {
                m_Result = result;
        }
        public Stream Result {
            get {
                RaiseExceptionIfNecessary();
                return m_Result;
            }
        }
    }

    public delegate void OpenWriteCompletedEventHandler(object sender, OpenWriteCompletedEventArgs e);

    public class OpenWriteCompletedEventArgs : AsyncCompletedEventArgs {
        private Stream m_Result;
        internal OpenWriteCompletedEventArgs(Stream result, Exception exception, bool cancelled, object userToken) :
            base(exception, cancelled, userToken) {
                m_Result = result;
        }
        public Stream Result {
            get {
                RaiseExceptionIfNecessary();
                return m_Result;
            }
        }
    }

    public delegate void DownloadStringCompletedEventHandler(object sender, DownloadStringCompletedEventArgs e);

    public class DownloadStringCompletedEventArgs : AsyncCompletedEventArgs {
        string m_Result;
        internal DownloadStringCompletedEventArgs(string result, Exception exception, bool cancelled, object userToken) :
            base(exception, cancelled, userToken) {
                m_Result = result;
        }
        public string Result {
            get {
                RaiseExceptionIfNecessary();
                return m_Result;
            }
        }

    }

    public delegate void DownloadDataCompletedEventHandler(object sender, DownloadDataCompletedEventArgs e);

    public class DownloadDataCompletedEventArgs : AsyncCompletedEventArgs {
        byte [] m_Result;
        internal DownloadDataCompletedEventArgs(byte[] result, Exception exception, bool cancelled, object userToken) :
            base(exception, cancelled, userToken) {
                m_Result = result;
        }
        public byte[] Result {
            get {
                RaiseExceptionIfNecessary();
                return m_Result;
            }
        }

    }

    public delegate void UploadStringCompletedEventHandler(object sender, UploadStringCompletedEventArgs e);

    public class UploadStringCompletedEventArgs : AsyncCompletedEventArgs {
        string m_Result;
        internal UploadStringCompletedEventArgs(string result, Exception exception, bool cancelled, object userToken) :
            base(exception, cancelled, userToken) {
                m_Result = result;
        }
        public string Result {
            get {
                RaiseExceptionIfNecessary();
                return m_Result;
            }
        }

    }

    public delegate void UploadDataCompletedEventHandler(object sender, UploadDataCompletedEventArgs e);

    public class UploadDataCompletedEventArgs : AsyncCompletedEventArgs {
        byte [] m_Result;
        internal UploadDataCompletedEventArgs(byte [] result, Exception exception, bool cancelled, object userToken) :
            base(exception, cancelled, userToken) {
                m_Result = result;
        }

        public byte[] Result {
            get {
                RaiseExceptionIfNecessary();
                return m_Result;
            }
        }
    }

    public delegate void UploadFileCompletedEventHandler(object sender, UploadFileCompletedEventArgs e);

    public class UploadFileCompletedEventArgs : AsyncCompletedEventArgs {
        byte [] m_Result;
        internal UploadFileCompletedEventArgs(byte[] result, Exception exception, bool cancelled, object userToken) :
            base(exception, cancelled, userToken) {
                m_Result = result;
        }

        public byte[] Result {
            get {
                RaiseExceptionIfNecessary();
                return m_Result;
            }
        }
    }

    public delegate void UploadValuesCompletedEventHandler(object sender, UploadValuesCompletedEventArgs e);

    public class UploadValuesCompletedEventArgs : AsyncCompletedEventArgs {
        byte [] m_Result;
        internal UploadValuesCompletedEventArgs(byte [] result, Exception exception, bool cancelled, object userToken) :
            base(exception, cancelled, userToken) {
                m_Result = result;
        }

        public byte[] Result {
            get {
                RaiseExceptionIfNecessary();
                return m_Result;
            }
        }
    }

    public delegate void DownloadProgressChangedEventHandler(object sender, DownloadProgressChangedEventArgs e);

    public class DownloadProgressChangedEventArgs : ProgressChangedEventArgs
    {
        long m_BytesReceived;
        long m_TotalBytesToReceive;

        internal DownloadProgressChangedEventArgs(int progressPercentage, object userToken, long bytesReceived, long totalBytesToReceive) :
            base(progressPercentage, userToken)
        {
            m_BytesReceived = bytesReceived;
            m_TotalBytesToReceive = totalBytesToReceive;
        }

        public long BytesReceived
        {
            get
            {
                return m_BytesReceived;
            }
        }

        public long TotalBytesToReceive
        {
            get
            {
                return m_TotalBytesToReceive;
            }
        }
    }

    public delegate void UploadProgressChangedEventHandler(object sender, UploadProgressChangedEventArgs e);

    public class UploadProgressChangedEventArgs : ProgressChangedEventArgs
    {
        long m_BytesReceived;
        long m_TotalBytesToReceive;
        long m_BytesSent;
        long m_TotalBytesToSend;

        internal UploadProgressChangedEventArgs(int progressPercentage, object userToken, long bytesSent, long totalBytesToSend, long bytesReceived, long totalBytesToReceive) :
            base(progressPercentage, userToken)
        {
            m_BytesReceived = bytesReceived;
            m_TotalBytesToReceive = totalBytesToReceive;
            m_BytesSent = bytesSent;
            m_TotalBytesToSend = totalBytesToSend;
        }

        public long BytesReceived
        {
            get
            {
                return m_BytesReceived;
            }
        }

        public long TotalBytesToReceive
        {
            get
            {
                return m_TotalBytesToReceive;
            }
        }

        public long BytesSent
        {
            get
            {
                return m_BytesSent;
            }
        }

        public long TotalBytesToSend
        {
            get
            {
                return m_TotalBytesToSend;
            }
        }

    }
}

