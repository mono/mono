// ------------------------------------------------------------------------------
// <copyright file="FtpWebRequest.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// ------------------------------------------------------------------------------
//

namespace System.Net {
    using System.Collections;
    using System.IO;
    using System.Text;
    using System.Net.Sockets;
    using System.Net.Cache;
    using System.Threading;
    using System.Security;
    using System.Security.Cryptography.X509Certificates ;
    using System.Security.Permissions;
    using System.Security.Authentication;
    using System.Globalization;
    using System.Diagnostics.Tracing;

    /// <summary>
    /// <para>Allows us to control what the request is used for (based on the type of behavior,
    ///     that the command calls for)</para>
    /// </summary>
    internal enum FtpOperation {
        DownloadFile          = 0,
        ListDirectory         = 1,
        ListDirectoryDetails  = 2,
        UploadFile            = 3,
        UploadFileUnique      = 4,
        AppendFile            = 5,
        DeleteFile            = 6,
        GetDateTimestamp      = 7,
        GetFileSize           = 8,
        Rename                = 9,
        MakeDirectory         = 10,
        RemoveDirectory       = 11,
        PrintWorkingDirectory = 12,
        Other                 = 13,
    }

    [Flags]
    internal enum FtpMethodFlags {
        None                      = 0x0,
        IsDownload                = 0x1,
        IsUpload                  = 0x2,
        TakesParameter            = 0x4,
        MayTakeParameter          = 0x8,
        DoesNotTakeParameter      = 0x10,
        ParameterIsDirectory      = 0x20,
        ShouldParseForResponseUri = 0x40,
        HasHttpCommand            = 0x80,
        MustChangeWorkingDirectoryToPath  = 0x100
    }

    internal class FtpMethodInfo {
        internal string          Method;
        internal FtpOperation    Operation;
        internal FtpMethodFlags  Flags;
        internal string          HttpCommand;

        internal FtpMethodInfo(string         method,
                               FtpOperation   operation,
                               FtpMethodFlags flags,
                               string         httpCommand)
        {
            Method      = method;
            Operation   = operation;
            Flags       = flags;
            HttpCommand = httpCommand;
        }

        internal bool HasFlag(FtpMethodFlags flags) {
            return (Flags & flags) != 0;
        }

        internal bool IsCommandOnly {
            get { return (Flags & (FtpMethodFlags.IsDownload | FtpMethodFlags.IsUpload)) == 0; }
        }

        internal bool IsUpload {
            get { return (Flags & FtpMethodFlags.IsUpload) != 0; }
        }

        internal bool IsDownload {
            get { return (Flags & FtpMethodFlags.IsDownload) != 0; }
        }

        internal bool HasHttpCommand {
            get { return (Flags & FtpMethodFlags.HasHttpCommand) != 0; }
        }

        /// <summary>
        ///    <para>True if we should attempt to get a response uri
        ///    out of a server response</para>
        /// </summary>
        internal bool ShouldParseForResponseUri {
            get { return (Flags & FtpMethodFlags.ShouldParseForResponseUri) != 0; }
        }

        internal static FtpMethodInfo GetMethodInfo(string method)  {
            method = method.ToUpper(CultureInfo.InvariantCulture);
            foreach (FtpMethodInfo methodInfo in KnownMethodInfo)
                if (method == methodInfo.Method)
                    return methodInfo;
            // We don't support generic methods
            throw new ArgumentException(SR.GetString(SR.net_ftp_unsupported_method), "method");
        }

        static readonly FtpMethodInfo[] KnownMethodInfo =
        {
            new FtpMethodInfo(WebRequestMethods.Ftp.DownloadFile,
                              FtpOperation.DownloadFile,
                              FtpMethodFlags.IsDownload
                              | FtpMethodFlags.HasHttpCommand
                              | FtpMethodFlags.TakesParameter,
                              "GET"),
            new FtpMethodInfo(WebRequestMethods.Ftp.ListDirectory,
                              FtpOperation.ListDirectory,
                              FtpMethodFlags.IsDownload
                              | FtpMethodFlags.MustChangeWorkingDirectoryToPath
                              | FtpMethodFlags.HasHttpCommand
                              | FtpMethodFlags.MayTakeParameter,
                              "GET"),
            new FtpMethodInfo(WebRequestMethods.Ftp.ListDirectoryDetails,
                              FtpOperation.ListDirectoryDetails,
                              FtpMethodFlags.IsDownload
                              | FtpMethodFlags.MustChangeWorkingDirectoryToPath
                              | FtpMethodFlags.HasHttpCommand
                              | FtpMethodFlags.MayTakeParameter,
                              "GET"),
            new FtpMethodInfo(WebRequestMethods.Ftp.UploadFile,
                              FtpOperation.UploadFile,
                              FtpMethodFlags.IsUpload
                              | FtpMethodFlags.TakesParameter,
                              null),
            new FtpMethodInfo(WebRequestMethods.Ftp.UploadFileWithUniqueName,
                              FtpOperation.UploadFileUnique,
                              FtpMethodFlags.IsUpload
                              | FtpMethodFlags.MustChangeWorkingDirectoryToPath
                              | FtpMethodFlags.DoesNotTakeParameter
                              | FtpMethodFlags.ShouldParseForResponseUri,
                              null),
            new FtpMethodInfo(WebRequestMethods.Ftp.AppendFile,
                              FtpOperation.AppendFile,
                              FtpMethodFlags.IsUpload
                              | FtpMethodFlags.TakesParameter,
                              null),
            new FtpMethodInfo(WebRequestMethods.Ftp.DeleteFile,
                              FtpOperation.DeleteFile,
                              FtpMethodFlags.TakesParameter,
                              null),
            new FtpMethodInfo(WebRequestMethods.Ftp.GetDateTimestamp,
                              FtpOperation.GetDateTimestamp,
                              FtpMethodFlags.TakesParameter,
                              null),
            new FtpMethodInfo(WebRequestMethods.Ftp.GetFileSize,
                              FtpOperation.GetFileSize,
                              FtpMethodFlags.TakesParameter,
                              null),
            new FtpMethodInfo(WebRequestMethods.Ftp.Rename,
                              FtpOperation.Rename,
                              FtpMethodFlags.TakesParameter,
                              null),
            new FtpMethodInfo(WebRequestMethods.Ftp.MakeDirectory,
                              FtpOperation.MakeDirectory,
                              FtpMethodFlags.TakesParameter
                              | FtpMethodFlags.ParameterIsDirectory,
                              null),
            new FtpMethodInfo(WebRequestMethods.Ftp.RemoveDirectory,
                              FtpOperation.RemoveDirectory,
                              FtpMethodFlags.TakesParameter
                              | FtpMethodFlags.ParameterIsDirectory,
                              null),
            new FtpMethodInfo(WebRequestMethods.Ftp.PrintWorkingDirectory,
                              FtpOperation.PrintWorkingDirectory,
                              FtpMethodFlags.DoesNotTakeParameter,
                              null)
        };

    }

    /// <summary>
    /// <para>The FtpWebRequest class implements a basic FTP client
    /// interface.</para>
    /// </summary>
    public sealed class FtpWebRequest : WebRequest {
        private object          m_SyncObject;
        private ICredentials    m_AuthInfo;
        private readonly Uri    m_Uri;
        private FtpMethodInfo   m_MethodInfo;
        private string          m_RenameTo = null;
        private bool            m_GetRequestStreamStarted;
        private bool            m_GetResponseStarted;
        private DateTime        m_StartTime;
        private int             m_Timeout = s_DefaultTimeout;
        private int             m_RemainingTimeout;
        private long            m_ContentLength = 0;
        private long            m_ContentOffset = 0;
        private IWebProxy       m_Proxy;
#if !FEATURE_PAL
        private X509CertificateCollection m_ClientCertificates;
#endif // !FEATURE_PAL
        private bool            m_KeepAlive = true;
        private bool            m_Passive = true;
        private bool            m_Binary = true;
        private string          m_ConnectionGroupName;
        private ServicePoint    m_ServicePoint;

        private bool            m_CacheDone; // Not sure why but the command stream wants to notify the request on every pipiline closure closure by invoking RequestCallback.
                                             // m_CacheDone is to facilitate PutConnection decision and to prevent bothering cache protocol when it's all completed.

        private bool            m_Async;
        private bool            m_Aborted;
        private bool            m_TimedOut;

        private HttpWebRequest  m_HttpWebRequest;
        private Exception       m_Exception;

        private TimerThread.Queue    m_TimerQueue = s_DefaultTimerQueue;
        private TimerThread.Callback m_TimerCallback;

        private bool                 m_EnableSsl;
        private bool                 m_ProxyUserSet;
        private ConnectionPool       m_ConnectionPool;
        private FtpControlStream     m_Connection;
        private Stream               m_Stream;
        private RequestStage         m_RequestStage;
        private bool                 m_OnceFailed;
        private WebHeaderCollection  m_FtpRequestHeaders;
        private FtpWebResponse       m_FtpWebResponse;
        private int                  m_ReadWriteTimeout = 5*60*1000;  //5 minutes.


        private ContextAwareResult  m_WriteAsyncResult;
        private LazyAsyncResult     m_ReadAsyncResult;
        private LazyAsyncResult     m_RequestCompleteAsyncResult;

        private static readonly GeneralAsyncDelegate m_AsyncCallback = new GeneralAsyncDelegate(AsyncCallbackWrapper);
        private static readonly CreateConnectionDelegate m_CreateConnectionCallback = new CreateConnectionDelegate(CreateFtpConnection);
        private static readonly NetworkCredential DefaultFtpNetworkCredential = new NetworkCredential("anonymous", "anonymous@", String.Empty);
        private static readonly int s_DefaultTimeout = WebRequest.DefaultTimeout;
        private static readonly TimerThread.Queue s_DefaultTimerQueue = TimerThread.GetOrCreateQueue(s_DefaultTimeout);

        // Used by FtpControlStream
        internal FtpMethodInfo MethodInfo {
            get {
                return m_MethodInfo;
            }
        }

        // Used by FtpControlStream
        internal static NetworkCredential DefaultNetworkCredential{
            get {
                return DefaultFtpNetworkCredential;
            }
        }

        // This is a shortcut that would set the default policy for HTTP/HTTPS.
        // The default policy is overridden by any prefix-registered policy.
        // Will demand permission for set{}
        public static new RequestCachePolicy DefaultCachePolicy {
            get {
                RequestCachePolicy policy = RequestCacheManager.GetBinding(Uri.UriSchemeFtp).Policy;
                if (policy == null)
                    return WebRequest.DefaultCachePolicy;
                return policy;
            }
            set {
                // This is a replacement of RequestCachePermission demand since we are not including the latest in the product.
                ExceptionHelper.WebPermissionUnrestricted.Demand();

                RequestCacheBinding binding = RequestCacheManager.GetBinding(Uri.UriSchemeFtp);
                RequestCacheManager.SetBinding(Uri.UriSchemeFtp, new RequestCacheBinding(binding.Cache, binding.Validator, value));
            }
        }

        /// <summary>
        /// <para>
        /// Selects upload or download of files. WebRequestMethods.Ftp.DownloadFile is default.
        /// Not allowed to be changed once request is started.
        /// </para>
        /// </summary>
        public override string Method {
            get {
                return m_MethodInfo.Method;
            }
            set {
                if (String.IsNullOrEmpty(value)) {
                    throw new ArgumentException(SR.GetString(SR.net_ftp_invalid_method_name), "value");
                }
                if (InUse) {
                    throw new InvalidOperationException(SR.GetString(SR.net_reqsubmitted));
                }
                try { 
                    m_MethodInfo = FtpMethodInfo.GetMethodInfo(value); 
                } catch (ArgumentException) {
                    throw new ArgumentException(SR.GetString(SR.net_ftp_unsupported_method), "value");
                }
           }
        }

        /// <summary>
        /// <para>
        /// Sets the target name for the WebRequestMethods.Ftp.Rename command
        /// Not allowed to be changed once request is started.
        /// </para>
        /// </summary>
        public string RenameTo {
            get {
                return m_RenameTo;
            }
            set {
                if (InUse) {
                    throw new InvalidOperationException(SR.GetString(SR.net_reqsubmitted));
                }

                if (String.IsNullOrEmpty(value)) {
                    throw new ArgumentException(SR.GetString(SR.net_ftp_invalid_renameto), "value");
                }

                m_RenameTo = value;
           }
        }

        /// <summary>
        /// <para>Used for clear text authentication with FTP server</para>
        /// </summary>
        public override ICredentials Credentials {
            get {
                return m_AuthInfo;
            }
            set {
                if (InUse) {
                    throw new InvalidOperationException(SR.GetString(SR.net_reqsubmitted));
                }
                if (value == null) {
                    throw new ArgumentNullException("value");
                }
                if (value is SystemNetworkCredential) {
                    throw new ArgumentException(SR.GetString(SR.net_ftp_no_defaultcreds), "value");
                }
                m_AuthInfo = value;
            }
        }

        /// <summary>
        /// <para>Gets the Uri used to make the request</para>
        /// </summary>
        public override Uri RequestUri {
            get {
                return m_Uri;
            }
        }

        /// <summary>
        /// <para>Timeout of the blocking calls such as GetResponse and GetRequestStream (default 100 secs)</para>
        /// </summary>
        public override int Timeout {
            get {
                return m_Timeout;
            }
            set {
                if (InUse) {
                    throw new InvalidOperationException(SR.GetString(SR.net_reqsubmitted));
                }
                if (value<0 && value!=System.Threading.Timeout.Infinite) {
                    throw new ArgumentOutOfRangeException("value", SR.GetString(SR.net_io_timeout_use_ge_zero));
                }
                if (m_Timeout != value)
                {
                    m_Timeout = value;
                    m_TimerQueue = null;
                }
            }
        }

        // This can always be calculaed as Remaining = Timeout - (Now - Start)
        // but we are keeping this for performance reasons (To avoid unnecessary 
        // calculations). This can be removed if the performance gains are 
        // considered negligible and not necessary
        internal int RemainingTimeout {
            get {
                return m_RemainingTimeout;
            }
        }



        /// <devdoc>
        ///    <para>Used to control the Timeout when calling Stream.Read (AND) Stream.Write.
        ///         Effects Streams returned from GetResponse().GetResponseStream() (AND) GetRequestStream().
        ///         Default is 5 mins.
        ///    </para>
        /// </devdoc>
        public int ReadWriteTimeout {
            get {
                return m_ReadWriteTimeout;
            }
            set {
                if (m_GetResponseStarted) {
                    throw new InvalidOperationException(SR.GetString(SR.net_reqsubmitted));
                }
                if (value<=0 && value!=System.Threading.Timeout.Infinite) {
                    throw new ArgumentOutOfRangeException("value", SR.GetString(SR.net_io_timeout_use_gt_zero));
                }
                m_ReadWriteTimeout = value;
            }
        }

        /// <summary>
        /// <para>Used to specify what offset we will read at</para>
        /// </summary>
        public long ContentOffset {
            get {
                return m_ContentOffset;
            }
            set {
                if (InUse) {
                    throw new InvalidOperationException(SR.GetString(SR.net_reqsubmitted));
                }
                if (value<0) {
                    throw new ArgumentOutOfRangeException("value");
                }
                m_ContentOffset = value;
            }
        }



        /// <summary>
        /// <para>Gets or sets the data size of to-be uploaded data</para>
        /// </summary>
        public override long ContentLength {
            get {
                return m_ContentLength;
            }
            set {
                m_ContentLength = value;
            }
        }

        /// <summary>
        /// <para>Uses an HTTP proxy if needed to send FTP request</para>
        /// </summary>
        public override IWebProxy Proxy {
            get {
                ExceptionHelper.WebPermissionUnrestricted.Demand();
                return m_Proxy;
            }
            set {
                ExceptionHelper.WebPermissionUnrestricted.Demand();
                if (InUse) {
                    throw new InvalidOperationException(SR.GetString(SR.net_reqsubmitted));
                }
                m_ProxyUserSet = true;
                m_Proxy = value;
                m_ServicePoint = null;
                ServicePoint refreshIt = ServicePoint ;
            }
        }

        /// <devdoc>
        /// <para>Allows private ConnectionPool(s) to be used</para>
        /// </devdoc>
        public override string ConnectionGroupName {
            get {
                return m_ConnectionGroupName;
            }
            set {
                if (InUse) {
                    throw new InvalidOperationException(SR.GetString(SR.net_reqsubmitted));
                }
                m_ConnectionGroupName = value;
            }
        }

        /// <devdoc>
        /// <para>Generates a service point for this request, and allows setting of Connection settings</para>
        /// </devdoc>
        public ServicePoint ServicePoint {
            get {
                if (m_ServicePoint == null)
                {
                    IWebProxy proxy = m_Proxy;
                    if (!m_ProxyUserSet)
                        proxy = WebRequest.InternalDefaultWebProxy;

                    ServicePoint servicePoint = ServicePointManager.FindServicePoint(m_Uri, proxy);

                    lock (m_SyncObject) {
                        if (m_ServicePoint == null)
                        {
                            m_ServicePoint = servicePoint;
                            m_Proxy = proxy;
                        }
                    }
                }
                return m_ServicePoint;
            }
        }

        internal bool Aborted {
            get {
                return m_Aborted;
            }
        }

        /// <summary>
        /// <para>
        /// Initializes a new instance of the <see cref='System.Net.FtpWebRequest'/>
        /// class.
        /// </para>
        /// </summary>
        internal FtpWebRequest(Uri uri) {
           (new WebPermission(NetworkAccess.Connect, uri)).Demand();

           if (Logging.On) Logging.PrintInfo(Logging.Web, this, ".ctor", uri.ToString());

           if ((object)uri.Scheme != (object)Uri.UriSchemeFtp)
               throw new ArgumentOutOfRangeException("uri");

            m_TimerCallback = new TimerThread.Callback(TimerCallback);
            m_SyncObject = new object();

            NetworkCredential networkCredential = null;
            m_Uri = uri;
            m_MethodInfo = FtpMethodInfo.GetMethodInfo(WebRequestMethods.Ftp.DownloadFile);
            if (m_Uri.UserInfo != null && m_Uri.UserInfo.Length != 0) {
                string userInfo = m_Uri.UserInfo;
                string username = userInfo;
                string password = "";
                int index = userInfo.IndexOf(':');
                if (index != -1) {
                    username = Uri.UnescapeDataString(userInfo.Substring(0, index));
                    index++; // skip ':'
                    password = Uri.UnescapeDataString(userInfo.Substring(index, userInfo.Length - index));
                }
                networkCredential = new NetworkCredential(username, password);
            }
            if (networkCredential == null) {
                networkCredential = DefaultFtpNetworkCredential;
            }
            m_AuthInfo = networkCredential;
            SetupCacheProtocol(m_Uri);
        }


        //
        // Used to query for the Response of an FTP request
        //
        public override WebResponse GetResponse()
        {
            if(Logging.On)Logging.Enter(Logging.Web, this, "GetResponse", "");
            if(Logging.On)Logging.PrintInfo(Logging.Web, this, "GetResponse", SR.GetString(SR.net_log_method_equal, m_MethodInfo.Method));
            GlobalLog.Enter("FtpWebRequest#" + ValidationHelper.HashString(this) + "::GetResponse");
            if (FrameworkEventSource.Log.IsEnabled()) {
                LogBeginGetResponse(success: true, synchronous: true);
            }

            bool success = false;
            int statusCode = -1;
            try {
                CheckError();

                if (m_FtpWebResponse != null) {
                    success = true;
                    statusCode = GetStatusCode(m_FtpWebResponse);
                    return m_FtpWebResponse;
                }

                if (m_GetResponseStarted) {
                    throw new InvalidOperationException(SR.GetString(SR.net_repcall));
                }

                m_GetResponseStarted = true;

                m_StartTime = DateTime.UtcNow;
                m_RemainingTimeout = Timeout;

                // We don't really need this variable, but we just need 
                // to call the property to measure its execution time
                ServicePoint servicePoint = ServicePoint;

                if (Timeout != System.Threading.Timeout.Infinite)
                {
                    m_RemainingTimeout = Timeout - (int)((DateTime.UtcNow - m_StartTime).TotalMilliseconds);

                    if(m_RemainingTimeout <= 0){
                        throw new WebException(NetRes.GetWebStatusString(WebExceptionStatus.Timeout), WebExceptionStatus.Timeout);
                    }
                }

                if (ServicePoint.InternalProxyServicePoint)
                {
                    if (EnableSsl) {
                        m_GetResponseStarted = false;
                        throw new WebException(SR.GetString(SR.net_ftp_proxy_does_not_support_ssl));
                    }

                    try {
                        HttpWebRequest httpWebRequest = GetHttpWebRequest();
                        if (Logging.On) Logging.Associate(Logging.Web, this, httpWebRequest);

                        m_FtpWebResponse = new FtpWebResponse((HttpWebResponse)httpWebRequest.GetResponse());
                    } catch (WebException webException) {
                        if (webException.Response != null &&
                            webException.Response is HttpWebResponse)
                        {
                            webException = new WebException(webException.Message,
                                null,
                                webException.Status,
                                new FtpWebResponse((HttpWebResponse)webException.Response),
                                webException.InternalStatus);
                        }
                        SetException(webException);
                        statusCode = GetStatusCode(webException);
                        throw webException;
                    }
                    // Catch added to address 
                    catch (InvalidOperationException invalidOpException)
                    {
                        SetException(invalidOpException);
                        FinishRequestStage(RequestStage.CheckForError);
                        throw;
                    }
                }
                else
                {
                    RequestStage prev = FinishRequestStage(RequestStage.RequestStarted);
                    if (prev >= RequestStage.RequestStarted)
                    {
                        if (prev < RequestStage.ReadReady)
                        {
                            lock (m_SyncObject)
                            {
                                if (m_RequestStage < RequestStage.ReadReady)
                                    m_ReadAsyncResult = new LazyAsyncResult(null, null, null);
                            }

                            // GetRequeststream or BeginGetRequestStream has not finished yet?
                            if (m_ReadAsyncResult != null)
                                m_ReadAsyncResult.InternalWaitForCompletion();

                            CheckError();
                        }
                    }
                    else
                    {
                        do
                        {
                            SubmitRequest(false);
                            if (m_MethodInfo.IsUpload)
                                FinishRequestStage(RequestStage.WriteReady);
                            else
                                FinishRequestStage(RequestStage.ReadReady);
                            CheckError();
                        } while (!CheckCacheRetrieveOnResponse());

                        EnsureFtpWebResponse(null);
                        // This may update the Stream memeber on m_FtpWebResponse based on the CacheProtocol feedback.
                        CheckCacheUpdateOnResponse();

                        if (m_FtpWebResponse.IsFromCache)
                            FinishRequestStage(RequestStage.ReleaseConnection);
                    }
                }

                statusCode = GetStatusCode(m_FtpWebResponse);
                success = true;
            }
            catch (Exception exception) {
                if (FrameworkEventSource.Log.IsEnabled()) {
                    WebException webException = exception as WebException;
                    if (webException != null) {
                        statusCode = GetStatusCode(webException);        
                    }
                }

                if(Logging.On)Logging.Exception(Logging.Web, this, "GetResponse", exception);

                // if m_Exception == null, we are about to throw an exception to the user
                // and we haven't saved the exception, which also means we haven't dealt
                // with it. So just release the connection and log this for investigation
                if (m_Exception == null) {
                    if(Logging.On)Logging.PrintWarning(Logging.Web, SR.GetString(SR.net_log_unexpected_exception, "GetResponse()"));

                    if (!NclUtilities.IsFatal(exception)){
                        GlobalLog.Assert("Find out why we are getting an unexpected exception.");
                    }
                    SetException(exception);
                    FinishRequestStage(RequestStage.CheckForError);
                }
                throw;
            } finally {

                GlobalLog.Leave("FtpWebRequest#" + ValidationHelper.HashString(this) + "::GetResponse", "returns #"+ValidationHelper.HashString(m_FtpWebResponse));
                if(Logging.On)Logging.Exit(Logging.Web, this, "GetResponse", "");
                if (FrameworkEventSource.Log.IsEnabled()) {
                    LogEndGetResponse(success, synchronous: true, statusCode: statusCode);
                }
            }
            return m_FtpWebResponse;
        }

        /// <include file='doc\FtpWebRequest.uex' path='docs/doc[@for="FtpWebRequest.BeginGetResponse"]/*' />
        /// <summary>
        /// <para>Used to query for the Response of an FTP request [async version]</para>
        /// </summary>
        [HostProtection(ExternalThreading=true)]
        public override IAsyncResult BeginGetResponse(AsyncCallback callback, object state)
        {
            if(Logging.On)Logging.Enter(Logging.Web, this, "BeginGetResponse", "");
            if(Logging.On)Logging.PrintInfo(Logging.Web, this, "BeginGetResponse", SR.GetString(SR.net_log_method_equal, m_MethodInfo.Method));
            GlobalLog.Enter("FtpWebRequest#" + ValidationHelper.HashString(this) + "::BeginGetResponse");

            ContextAwareResult asyncResult;
            bool success = true;
            
            try {
                if (m_FtpWebResponse != null)
                {
                    asyncResult = new ContextAwareResult(this, state, callback);
                    asyncResult.InvokeCallback(m_FtpWebResponse);
                    return asyncResult;
                }

                if (m_GetResponseStarted) {
                    throw new InvalidOperationException(SR.GetString(SR.net_repcall));
                }

                m_GetResponseStarted = true;
                CheckError();

                if (ServicePoint.InternalProxyServicePoint)
                {
                    HttpWebRequest httpWebRequest = GetHttpWebRequest();
                    if (Logging.On) Logging.Associate(Logging.Web, this, httpWebRequest);
                    asyncResult = (ContextAwareResult)httpWebRequest.BeginGetResponse(callback, state);
                }
                else
                {
                    RequestStage prev = FinishRequestStage(RequestStage.RequestStarted);
                    asyncResult = new ContextAwareResult(true, true, this, state, callback);
                    m_ReadAsyncResult = asyncResult;

                    if (prev >= RequestStage.RequestStarted)
                    {
                        // To make sure the context is flowed
                        asyncResult.StartPostingAsyncOp();
                        asyncResult.FinishPostingAsyncOp();

                        if (prev >= RequestStage.ReadReady)
                            asyncResult = null;
                        else
                        {
                            lock (m_SyncObject)
                            {
                                if (m_RequestStage >= RequestStage.ReadReady)
                                    asyncResult = null;;
                            }
                        }

                        if(asyncResult == null)
                        {
                            // need to complete it now
                            asyncResult = (ContextAwareResult)m_ReadAsyncResult;
                            if (!asyncResult.InternalPeekCompleted)
                                asyncResult.InvokeCallback();
                        }
                    }
                    else
                    {
                        // Do internal processing in this handler to optimize context flowing.
                        lock (asyncResult.StartPostingAsyncOp())
                        {
                            SubmitRequest(true);
                            asyncResult.FinishPostingAsyncOp();
                        }
                        FinishRequestStage(RequestStage.CheckForError);
                    }
                }
            } catch (Exception exception) {
                success = false;
                if(Logging.On)Logging.Exception(Logging.Web, this, "BeginGetResponse", exception);
                throw;
            } finally {
                if (FrameworkEventSource.Log.IsEnabled()) {
                    LogBeginGetResponse(success, synchronous: false);
                }
                GlobalLog.Leave("FtpWebRequest#" + ValidationHelper.HashString(this) + "::BeginGetResponse");
                if(Logging.On)Logging.Exit(Logging.Web, this, "BeginGetResponse", "");
            }

            return asyncResult;
        }

        /// <summary>
        /// <para>Returns result of query for the Response of an FTP request [async version]</para>
        /// </summary>
        public override WebResponse EndGetResponse(IAsyncResult asyncResult) {

            if(Logging.On)Logging.Enter(Logging.Web, this, "EndGetResponse", "");
            GlobalLog.Enter("FtpWebRequest#" + ValidationHelper.HashString(this) + "::EndGetResponse");

            bool success = false;
            int statusCode = -1;
            try {
                // parameter validation
                if (asyncResult==null) {
                    throw new ArgumentNullException("asyncResult");
                }
                LazyAsyncResult castedAsyncResult = asyncResult as LazyAsyncResult;
                if (castedAsyncResult==null) {
                    throw new ArgumentException(SR.GetString(SR.net_io_invalidasyncresult), "asyncResult");
                }
                if (HttpProxyMode?(castedAsyncResult.AsyncObject!=this.GetHttpWebRequest()):castedAsyncResult.AsyncObject!=this) {
                    throw new ArgumentException(SR.GetString(SR.net_io_invalidasyncresult), "asyncResult");
                }
                if (castedAsyncResult.EndCalled) {
                    throw new InvalidOperationException(SR.GetString(SR.net_io_invalidendcall, "EndGetResponse"));
                }

                if (HttpProxyMode) {
                    try {
                        CheckError();
                        if (m_FtpWebResponse == null)
                        {
                            m_FtpWebResponse = new FtpWebResponse((HttpWebResponse)GetHttpWebRequest().EndGetResponse(asyncResult));
                            statusCode = GetStatusCode(m_FtpWebResponse);
                        }
                    } catch (WebException webException) {
                        statusCode = GetStatusCode(webException);
                        if (webException.Response != null &&
                            webException.Response is HttpWebResponse)
                        {
                            throw new WebException(webException.Message,
                                                    null,
                                                    webException.Status,
                                                    new FtpWebResponse((HttpWebResponse)webException.Response),
                                                    webException.InternalStatus);
                        }
                        throw;
                    }
                }
                else{
                    castedAsyncResult.InternalWaitForCompletion();
                    castedAsyncResult.EndCalled = true;
                    CheckError();
                }

                success = true;
            }
            catch (Exception exception) {
                if (FrameworkEventSource.Log.IsEnabled()) {
                    WebException webException = exception as WebException;
                    if (webException != null) {
                        statusCode = GetStatusCode(webException);
                    }
                }

                if(Logging.On)Logging.Exception(Logging.Web, this, "EndGetResponse", exception);
                throw;
            } finally {
                GlobalLog.Leave("FtpWebRequest#" + ValidationHelper.HashString(this) + "::EndGetResponse");
                if(Logging.On)Logging.Exit(Logging.Web, this, "EndGetResponse", "");
                if (FrameworkEventSource.Log.IsEnabled()) {
                    LogEndGetResponse(success, synchronous: false, statusCode: statusCode);
                }
            }

            return m_FtpWebResponse;
        }


        /// <summary>
        /// <para>Used to query for the Request stream of an FTP Request</para>
        /// </summary>
        public override Stream GetRequestStream() {
            if(Logging.On)Logging.Enter(Logging.Web, this, "GetRequestStream", "");
            if(Logging.On)Logging.PrintInfo(Logging.Web, this, "GetRequestStream", SR.GetString(SR.net_log_method_equal, m_MethodInfo.Method));
            GlobalLog.Enter("FtpWebRequest#" + ValidationHelper.HashString(this) + "::GetRequestStream");
            if (FrameworkEventSource.Log.IsEnabled()) {
                LogBeginGetRequestStream(success: true, synchronous: true);
            }

            bool success = false;
            try {
                if (m_GetRequestStreamStarted) {
                    throw new InvalidOperationException(SR.GetString(SR.net_repcall));
                }
                m_GetRequestStreamStarted = true;
                if (!m_MethodInfo.IsUpload) {
                    throw new ProtocolViolationException(SR.GetString(SR.net_nouploadonget));
                }
                CheckError();

                m_StartTime = DateTime.UtcNow;
                m_RemainingTimeout = Timeout;

                // We don't really need this variable, but we just need 
                // to call the property to measure its execution time
                ServicePoint servicePoint = ServicePoint;

                if (Timeout != System.Threading.Timeout.Infinite)
                {
                    m_RemainingTimeout = Timeout - (int)((DateTime.UtcNow - m_StartTime).TotalMilliseconds);

                    if(m_RemainingTimeout <= 0){
                        throw new WebException(NetRes.GetWebStatusString(WebExceptionStatus.Timeout), WebExceptionStatus.Timeout);
                    }
                }


                if (ServicePoint.InternalProxyServicePoint)
                {
                    HttpWebRequest httpWebRequest = GetHttpWebRequest();
                    if (Logging.On) Logging.Associate(Logging.Web, this, httpWebRequest);
                    m_Stream = httpWebRequest.GetRequestStream();
                } else {
                    FinishRequestStage(RequestStage.RequestStarted);
                    SubmitRequest(false);
                    FinishRequestStage(RequestStage.WriteReady);
                    CheckError();
                }

                if(m_Stream.CanTimeout) {
                    m_Stream.WriteTimeout = ReadWriteTimeout;
                    m_Stream.ReadTimeout = ReadWriteTimeout;
                }

                success = true;
            } catch (Exception exception) {
                if(Logging.On)Logging.Exception(Logging.Web, this, "GetRequestStream", exception);
                throw;
            } finally {
                GlobalLog.Leave("FtpWebRequest#" + ValidationHelper.HashString(this) + "::GetRequestStream");
                if(Logging.On)Logging.Exit(Logging.Web, this, "GetRequestStream", "");
                if (FrameworkEventSource.Log.IsEnabled()) {
                    LogEndGetRequestStream(success, synchronous: true);
                }
            }
            return m_Stream;
        }

        /// <summary>
        /// <para>Used to query for the Request stream of an FTP Request [async version]</para>
        /// </summary>
        [HostProtection(ExternalThreading=true)]
        public override IAsyncResult BeginGetRequestStream(AsyncCallback callback, object state) {
            if(Logging.On)Logging.Enter(Logging.Web, this, "BeginGetRequestStream", "");
            if(Logging.On)Logging.PrintInfo(Logging.Web, this, "BeginGetRequestStream", SR.GetString(SR.net_log_method_equal, m_MethodInfo.Method));
            GlobalLog.Enter("FtpWebRequest#" + ValidationHelper.HashString(this) + "::BeginGetRequestStream");

            ContextAwareResult asyncResult = null;
            bool success = false;
            try {
                if (m_GetRequestStreamStarted) {
                    throw new InvalidOperationException(SR.GetString(SR.net_repcall));
                }
                m_GetRequestStreamStarted = true;
                if (!m_MethodInfo.IsUpload) {
                    throw new ProtocolViolationException(SR.GetString(SR.net_nouploadonget));
                }
                CheckError();

                if (ServicePoint.InternalProxyServicePoint)
                {
                    HttpWebRequest httpWebRequest = GetHttpWebRequest();
                    if (Logging.On) Logging.Associate(Logging.Web, this, httpWebRequest);
                    asyncResult = (ContextAwareResult)httpWebRequest.BeginGetRequestStream(callback, state);
                }
                else
                {
                    FinishRequestStage(RequestStage.RequestStarted);
                    asyncResult = new ContextAwareResult(true, true, this, state, callback);
                    lock (asyncResult.StartPostingAsyncOp())
                    {
                        m_WriteAsyncResult = asyncResult;
                        SubmitRequest(true);
                        asyncResult.FinishPostingAsyncOp();
                        FinishRequestStage(RequestStage.CheckForError);
                    }
                }

                success = true;
            } catch (Exception exception) {
                if(Logging.On)Logging.Exception(Logging.Web, this, "BeginGetRequestStream", exception);
                throw;
            } finally {
                
                if (FrameworkEventSource.Log.IsEnabled()){
                    LogBeginGetRequestStream(success, synchronous: false);
                }
                GlobalLog.Leave("FtpWebRequest#" + ValidationHelper.HashString(this) + "::BeginGetRequestStream");
                if(Logging.On)Logging.Exit(Logging.Web, this, "BeginGetRequestStream", "");
            }

            return asyncResult;
        }

        public override Stream EndGetRequestStream(IAsyncResult asyncResult) {

            if(Logging.On)Logging.Enter(Logging.Web, this, "EndGetRequestStream", "");
            GlobalLog.Enter("FtpWebRequest#" + ValidationHelper.HashString(this) + "::EndGetRequestStream");

            Stream requestStream = null;
            bool success = false;

            try {
                // parameter validation
                if (asyncResult==null) {
                    throw new ArgumentNullException("asyncResult");
                }

                LazyAsyncResult castedAsyncResult = asyncResult as LazyAsyncResult;
                if ((castedAsyncResult==null) ||
                    (HttpProxyMode?(castedAsyncResult.AsyncObject!=this.GetHttpWebRequest()):castedAsyncResult.AsyncObject!=this)) {
                    throw new ArgumentException(SR.GetString(SR.net_io_invalidasyncresult), "asyncResult");
                }

                if (castedAsyncResult.EndCalled) {
                    throw new InvalidOperationException(SR.GetString(SR.net_io_invalidendcall, "EndGetResponse"));
                }

                if (HttpProxyMode) {
                    requestStream = GetHttpWebRequest().EndGetRequestStream(asyncResult);
                } else {
                    castedAsyncResult.InternalWaitForCompletion();
                    castedAsyncResult.EndCalled = true;
                    CheckError();
                    requestStream = m_Stream;
                    castedAsyncResult.EndCalled = true;
                }

                if(requestStream.CanTimeout) {
                    requestStream.WriteTimeout = ReadWriteTimeout;
                    requestStream.ReadTimeout = ReadWriteTimeout;
                }

                success = true;
            } catch (Exception exception) {
                if(Logging.On)Logging.Exception(Logging.Web, this, "EndGetRequestStream", exception);
                throw;
            } finally {
                GlobalLog.Leave("FtpWebRequest#" + ValidationHelper.HashString(this) + "::EndGetRequestStream");
                if(Logging.On)Logging.Exit(Logging.Web, this, "EndGetRequestStream", "");
            }

            if (FrameworkEventSource.Log.IsEnabled()) {
                LogEndGetRequestStream(success, synchronous: false);
            }

            return requestStream;
        }

        //
        // NOTE1: The caller must synchronize access to SubmitRequest(), only one call is even allowed for a particular request!
        // NOTE2: This method eats all exceptions so the caller must rethrow them
        //
        private void SubmitRequest(bool async) {
            try {
                m_Async = async;

                if (CheckCacheRetrieveBeforeSubmit())
                {
                    RequestCallback(null);
                    return;
                }

                //  This is the only place touching m_ConnectionPool
                if (m_ConnectionPool == null)
                    m_ConnectionPool = ConnectionPoolManager.GetConnectionPool(ServicePoint, GetConnectionGroupLine(), m_CreateConnectionCallback);

                //
                // FYI: Will do 2 attempts max as per AttemptedRecovery
                //
                Stream  stream;

                while(true)
                {
                    FtpControlStream connection = m_Connection;

                    if (connection == null)
                    {
                        connection = QueueOrCreateConnection();
                        if (connection == null)
                            return;
                    }

                    if(!async){
                        if (Timeout != System.Threading.Timeout.Infinite)
                        {
                            m_RemainingTimeout = Timeout - (int)((DateTime.UtcNow - m_StartTime).TotalMilliseconds);

                            if(m_RemainingTimeout <= 0){
                                throw new WebException(NetRes.GetWebStatusString(WebExceptionStatus.Timeout), WebExceptionStatus.Timeout);
                            }
                        }
                    }

                    GlobalLog.Print("Request being submitted"+ValidationHelper.HashString(this));
                    connection.SetSocketTimeoutOption(SocketShutdown.Both, RemainingTimeout, false);

                    try {
                        stream = TimedSubmitRequestHelper(async);
                    } catch (Exception e) {
                        if (AttemptedRecovery(e)){
                            if(!async){
                                if (Timeout != System.Threading.Timeout.Infinite)
                                {
                                    m_RemainingTimeout = Timeout - (int)((DateTime.UtcNow - m_StartTime).TotalMilliseconds);
                                    if(m_RemainingTimeout <= 0){
                                        throw;
                                    }
                                }
                            }
                            continue;
                        }
                        throw;
                    }
                    // no retry needed
                    break;
                }
            } catch (WebException webException) {
                //if this was a timeout, throw a timeout exception
                IOException ioEx = webException.InnerException as IOException;
                if(ioEx != null){
                    SocketException sEx = ioEx.InnerException as SocketException;
                     if(sEx != null){
                        if (sEx.ErrorCode == (int)SocketError.TimedOut) {
                            SetException(new WebException(SR.GetString(SR.net_timeout), WebExceptionStatus.Timeout));
                        }
                    }
                }

                SetException(webException);
            }
            catch (Exception exception) {
                SetException(exception);
            }
        }

        //
        //
        //
        private FtpControlStream QueueOrCreateConnection()
        {
            FtpControlStream connection = (FtpControlStream) m_ConnectionPool.GetConnection((object)this, (m_Async ? m_AsyncCallback : null), (m_Async ? -1: RemainingTimeout));

            if (connection == null)
            {
                GlobalLog.Assert(m_Async, "QueueOrCreateConnection|m_ConnectionPool.GetConnection() returned null on a Sync Request.");
                return null;
            }

            lock (m_SyncObject)
            {
                if (m_Aborted)
                {
                    if (Logging.On) Logging.PrintInfo(Logging.Web, this, "", SR.GetString(SR.net_log_releasing_connection, ValidationHelper.HashString(connection)));
                    m_ConnectionPool.PutConnection(connection, this, RemainingTimeout);
                    CheckError(); //must throw
                    throw new InternalException();
                }
                m_Connection = connection;
                if (Logging.On) Logging.Associate(Logging.Web, this, m_Connection);
            }
            return connection;
        }
        //
        //
        private Stream TimedSubmitRequestHelper(bool async) 
        {
            if(async) {
                // non-null in the case of re-submit (recovery)
                if (m_RequestCompleteAsyncResult == null)
                    m_RequestCompleteAsyncResult = new LazyAsyncResult(null, null, null);
                return m_Connection.SubmitRequest(this, true, true);
            }

            Stream stream = null;
            bool timedOut = false;
            TimerThread.Timer timer = TimerQueue.CreateTimer(m_TimerCallback, null);
            try {
                stream = m_Connection.SubmitRequest(this, false, true);
            }
            catch (Exception exception) {
                if (!(exception is SocketException || exception is ObjectDisposedException) || !timer.HasExpired) {
                    timer.Cancel();
                    throw;
                }

                timedOut = true;
            }

            if (timedOut || !timer.Cancel()) {
                m_TimedOut = true;
                throw new WebException(NetRes.GetWebStatusString(WebExceptionStatus.Timeout), WebExceptionStatus.Timeout);
            }

            if (stream != null)
            {
                lock (m_SyncObject)
                {
                    if (m_Aborted)
                    {
                        ((ICloseEx)stream).CloseEx(CloseExState.Abort|CloseExState.Silent);
                        CheckError(); //must throw
                        throw new InternalException(); //consider replacing this on Assert
                    }
                    m_Stream = stream;
                }
            }

            return stream;
        }

        /// <summary>
        ///    <para>Because this is called from the timer thread, neither it nor any methods it calls can call user code.</para>
        /// </summary>
        private void TimerCallback(TimerThread.Timer timer, int timeNoticed, object context) {
            GlobalLog.Print("FtpWebRequest#" + ValidationHelper.HashString(this) + "::TimerCallback");
            FtpControlStream connection = m_Connection;
            if (connection != null) {
                GlobalLog.Print("FtpWebRequest#" + ValidationHelper.HashString(this) + "::TimerCallback aborting connection");
                connection.AbortConnect();
            }
        }

        private TimerThread.Queue TimerQueue {
            get {
                if (m_TimerQueue == null) {
                    m_TimerQueue = TimerThread.GetOrCreateQueue(RemainingTimeout);
                }

                return m_TimerQueue;
            }
        }

        /// <summary>
        ///    <para>Returns true if we should restart the request after an error</para>
        /// </summary>
        private bool AttemptedRecovery(Exception e) {
            // The first 'if' is just checking whether the exception is thrown due to the
            // relogin failure which is a recoverable error
            if (!(e is WebException && ((WebException)e).InternalStatus == WebExceptionInternalStatus.Isolated))
            {
                if (e is ThreadAbortException
                    || e is StackOverflowException
                    || e is OutOfMemoryException
                    || m_OnceFailed
                    || m_Aborted
                    || m_TimedOut
                    || m_Connection==null
                    || !m_Connection.RecoverableFailure)
                {
                    return false;
                }
                m_OnceFailed = true;
            }

            lock (m_SyncObject) {
                if (m_ConnectionPool != null && m_Connection != null) {
                    m_Connection.CloseSocket();
                    if (Logging.On) Logging.PrintInfo(Logging.Web, this, "", SR.GetString(SR.net_log_releasing_connection, ValidationHelper.HashString(m_Connection)));
                    m_ConnectionPool.PutConnection(m_Connection, this, RemainingTimeout);
                    m_Connection = null;
                } else {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        ///    <para>Updates and sets our exception to be thrown</para>
        /// </summary>
        private void SetException(Exception exception) {

            GlobalLog.Print("FtpWebRequest#" + ValidationHelper.HashString(this) + "::SetException");

            if (exception is ThreadAbortException || exception is StackOverflowException || exception is OutOfMemoryException) {
                m_Exception = exception;
                throw exception;
            }

            FtpControlStream connection = m_Connection;
            if (m_Exception == null) {
                if (exception is WebException)
                {
                    EnsureFtpWebResponse(exception);
                    m_Exception = new WebException(exception.Message, null, ((WebException)exception).Status, m_FtpWebResponse);
                }
                else if (exception is AuthenticationException || exception is SecurityException)
                {
                    m_Exception = exception;
                }
                else if (connection!= null && connection.StatusCode != FtpStatusCode.Undefined)
                {
                    EnsureFtpWebResponse(exception);
                    m_Exception = new WebException(SR.GetString(SR.net_servererror, connection.StatusLine), exception, WebExceptionStatus.ProtocolError, m_FtpWebResponse);
                } else
                {
                    m_Exception = new WebException(exception.Message, exception);
                }

                if (connection != null && m_FtpWebResponse != null)
                    m_FtpWebResponse.UpdateStatus(connection.StatusCode, connection.StatusLine, connection.ExitMessage);
            }
        }

        /// <summary>
        ///    <para>Opposite of SetException, rethrows the exception</para>
        /// </summary>
        private void CheckError() {
            if (m_Exception != null) {
                throw m_Exception;
            }
        }

        // Return null only on Sync (if we're on the Sync thread).  Otherwise throw if no context is available.
        //
        // 

        internal override ContextAwareResult GetWritingContext()
        {
            if (m_ReadAsyncResult != null && m_ReadAsyncResult is ContextAwareResult)
                return (ContextAwareResult)m_ReadAsyncResult;
            else if (m_WriteAsyncResult != null)
                return m_WriteAsyncResult;

            // Sync.
            GlobalLog.ThreadContract(ThreadKinds.User | ThreadKinds.Sync, "FtpWebRequest#" + ValidationHelper.HashString(this) + "::GetWritingContext");
            return null;
        }

        //
        //    Provides an abstract way of having Async code callback into the request (saves a delegate)
        //
        //    ATTN this method is also called on sync path when either command or data stream gets closed
        //    Consider: Revisit the design of ftp streams
        //
        internal override void RequestCallback(object obj)
        {
            if (m_Async)
                AsyncRequestCallback(obj);
            else
                SyncRequestCallback(obj);
        }
        //
        // Only executed for Sync requests when the pipline is completed
        //
        private void SyncRequestCallback(object obj)
        {
            GlobalLog.Enter("FtpWebRequest#" + ValidationHelper.HashString(this) + "::SyncRequestCallback", "#"+ValidationHelper.HashString(obj));
            RequestStage stageMode = RequestStage.CheckForError;
            try {

                bool completedRequest = obj == null;
                Exception exception = obj as Exception;

                GlobalLog.Print("SyncRequestCallback() exp:"+ValidationHelper.HashString(exception)+" completedRequest:"+ValidationHelper.HashString(completedRequest));


                if (exception != null)
                {
                    SetException(exception);
                }
                else if (!completedRequest)
                {
                    throw new InternalException();
                }
                else
                {
                    // a signal on current pipeline completion
                    FtpControlStream connection = m_Connection;

                    bool isRevalidatedOrRetried = false;
                    if (connection != null)
                    {
                        EnsureFtpWebResponse(null);

                        // This to update response status and exit message if any
                        // Note that due to a design of FtpControlStream the status 221 "Service closing control connection" is always suppresses.
                        m_FtpWebResponse.UpdateStatus(connection.StatusCode, connection.StatusLine, connection.ExitMessage);

                        isRevalidatedOrRetried =!m_CacheDone &&
                                                (CacheProtocol.ProtocolStatus == CacheValidationStatus.Continue || CacheProtocol.ProtocolStatus == CacheValidationStatus.RetryResponseFromServer);

                        // This is for sync Upload commands that do not get chance hit GetResponse loop
                        if (m_MethodInfo.IsUpload)
                        {
                            CheckCacheRetrieveOnResponse();
                            CheckCacheUpdateOnResponse();
                        }
                    }

                    if (!isRevalidatedOrRetried)
                        stageMode = RequestStage.ReleaseConnection;
                }
            }
            catch (Exception exception) {
                SetException(exception);
            }
            finally {
                FinishRequestStage(stageMode);
                GlobalLog.Leave("FtpWebRequest#" + ValidationHelper.HashString(this) + "::SyncRequestCallback");
                CheckError(); //will throw on error
            }
        }
        //
        // Only executed for Async requests
        //
        private void AsyncRequestCallback(object obj)
        {
            GlobalLog.Enter("FtpWebRequest#" + ValidationHelper.HashString(this) + "::AsyncRequestCallback", "#"+ValidationHelper.HashString(obj));
            RequestStage stageMode = RequestStage.CheckForError;

            try {

                FtpControlStream connection;
                connection = obj as FtpControlStream;
                FtpDataStream stream = obj as FtpDataStream;
                Exception exception = obj as Exception;

                bool completedRequest = (obj == null);

                GlobalLog.Print("AsyncRequestCallback()  stream:"+ValidationHelper.HashString(stream)+" conn:"+ValidationHelper.HashString(connection)+" exp:"+ValidationHelper.HashString(exception)+" completedRequest:"+ValidationHelper.HashString(completedRequest));

                while (true)
                {
                    if (exception != null)
                    {
                        if (AttemptedRecovery(exception))
                        {
                            connection = QueueOrCreateConnection();
                            if (connection == null)
                                return;
                            exception = null;
                        }
                        if (exception != null)
                        {
                            SetException(exception);
                            break;
                        }
                    }

                    if (connection != null)
                    {
                        lock(m_SyncObject)
                        {
                            if (m_Aborted)
                            {
                                if (Logging.On) Logging.PrintInfo(Logging.Web, this, "", SR.GetString(SR.net_log_releasing_connection, ValidationHelper.HashString(connection)));
                                m_ConnectionPool.PutConnection(connection, this, Timeout);
                                break;
                            }
                            m_Connection = connection;
                            if (Logging.On) Logging.Associate(Logging.Web, this, m_Connection);
                        }

                        try {
                            stream = (FtpDataStream)TimedSubmitRequestHelper(true);
                        } catch (Exception e) {
                            exception = e;
                            continue;
                        }
                        return;
                    }
                    else if (stream != null)
                    {
                        lock (m_SyncObject)
                        {
                            if (m_Aborted)
                            {
                                ((ICloseEx)stream).CloseEx(CloseExState.Abort|CloseExState.Silent);
                                break;
                            }
                            m_Stream = stream;
                        }

                        stream.SetSocketTimeoutOption(SocketShutdown.Both, Timeout, true);
                        EnsureFtpWebResponse(null);
                        // This one may update the Stream member on m_FtpWebResponse based on the CacheProtocol feedback.
                        CheckCacheRetrieveOnResponse();
                        CheckCacheUpdateOnResponse();

                        stageMode = stream.CanRead? RequestStage.ReadReady: RequestStage.WriteReady;
                    }
                    else if (completedRequest)
                    {
                        connection = m_Connection;

                        bool isRevalidatedOrRetried = false;
                        if (connection != null)
                        {
                            EnsureFtpWebResponse(null);

                            // This to update response status and exit message if any
                            // Note that due to a design of FtpControlStream the status 221 "Service closing control connection" is always suppresses.
                            m_FtpWebResponse.UpdateStatus(connection.StatusCode, connection.StatusLine, connection.ExitMessage);

                            isRevalidatedOrRetried =!m_CacheDone &&
                                                    (CacheProtocol.ProtocolStatus == CacheValidationStatus.Continue || CacheProtocol.ProtocolStatus == CacheValidationStatus.RetryResponseFromServer);

                            lock (m_SyncObject)
                            {
                                if(!CheckCacheRetrieveOnResponse())
                                    continue;

                                if (m_FtpWebResponse.IsFromCache)
                                    isRevalidatedOrRetried = false;

                                CheckCacheUpdateOnResponse();
                            }
                        }

                        if (!isRevalidatedOrRetried)
                            stageMode = RequestStage.ReleaseConnection;
                    }
                    else
                    {
                        throw new InternalException();
                    }
                    break;
                }
            }
            catch (Exception exception)
            {
                SetException(exception);
            }            
            finally {
                FinishRequestStage(stageMode);
                GlobalLog.Leave("FtpWebRequest#" + ValidationHelper.HashString(this) + "::AsyncRequestCallback");
            }
        }

        //
        //
        //
        private enum RequestStage {
            CheckForError   = 0,// Do nothing except if there is an error then auto promote to ReleaseConnection
            RequestStarted,     // Mark this request as started
            WriteReady,         // First half is done, i.e. either writer or response stream. This is always assumed unless Started or CheckForError
            ReadReady,          // Second half is done, i.e. the read stream can be accesses.
            ReleaseConnection   // Release the control connection (request is read i.e. done-done)
        }
        //
        // Returns a previous stage
        //
        private RequestStage FinishRequestStage(RequestStage stage)
        {
            GlobalLog.Print("FtpWebRequest#" + ValidationHelper.HashString(this) + "::FinishRequestStage : stage="+stage);
            if (m_Exception != null)
                stage = RequestStage.ReleaseConnection;

            RequestStage     prev;
            LazyAsyncResult  writeResult;
            LazyAsyncResult  readResult;
            FtpControlStream connection;

            lock (m_SyncObject)
            {
                prev = m_RequestStage;

                if (stage == RequestStage.CheckForError)
                    return prev;

                if (prev == RequestStage.ReleaseConnection && 
                    stage == RequestStage.ReleaseConnection)
                {
                    return RequestStage.ReleaseConnection;
                }

                if (stage > prev)
                    m_RequestStage = stage;

                if (stage <= RequestStage.RequestStarted)
                    return prev;

                writeResult  = m_WriteAsyncResult;
                readResult   = m_ReadAsyncResult;
                connection   = m_Connection;

                if (stage == RequestStage.ReleaseConnection)
                {
                    if (m_Exception == null &&
                        !m_Aborted &&
                        prev != RequestStage.ReadReady &&
                        this.m_MethodInfo.IsDownload && 
                        !m_FtpWebResponse.IsFromCache)
                    {
                        return prev;
                    }
                    if (m_Exception != null || !(m_FtpWebResponse.IsFromCache && !KeepAlive))
                        m_Connection = null;
                }
            }

            try {
                // First check to see on releasing the connection
                if ((stage == RequestStage.ReleaseConnection ||
                     prev  == RequestStage.ReleaseConnection)
                    && connection != null)
                {
                    try {
                        if (m_Exception != null)
                        {
                            connection.Abort(m_Exception);
                        }
                        else if (m_FtpWebResponse.IsFromCache && !KeepAlive)
                        {
                            // This means the response has been revalidated and found as good means the commands pipleline is completed.
                            // Now if this request was NOT KeepAlive we want to be fair and close the control connection.
                            // That becomes unnecessary complicated in the async case to support "QUIT" command semantic, so simply close the socket
                            // and let pool collect the object.
                            connection.Quit();
                        }
                    } finally {
                        if (Logging.On) Logging.PrintInfo(Logging.Web, this, "", SR.GetString(SR.net_log_releasing_connection, ValidationHelper.HashString(connection)));
                        m_ConnectionPool.PutConnection(connection, this, RemainingTimeout);
                        if (m_Async)
                            if (m_RequestCompleteAsyncResult != null)
                                m_RequestCompleteAsyncResult.InvokeCallback();
                    }
                }
                return prev;
            }
            finally {
                try {
                    // In any case we want to signal the writer if came here
                    if (stage >= RequestStage.WriteReady) {
                        // If writeResult == null and this is an upload request, it means
                        // that the user has called GetResponse() without calling 
                        // GetRequestStream() first. So they are not interested in a 
                        // stream. Therefore we close the stream so that the 
                        // request/pipeline can continue
                        if (m_MethodInfo.IsUpload && !m_GetRequestStreamStarted)
                        {
                            if (m_Stream != null)
                                m_Stream.Close();
                        }
                        else if (writeResult != null && !writeResult.InternalPeekCompleted)
                            writeResult.InvokeCallback();
                    }
                }
                finally {
                    // The response is ready either with or without a stream
                    if (stage >= RequestStage.ReadReady && readResult != null && !readResult.InternalPeekCompleted)
                        readResult.InvokeCallback();
                }
            }
        }

        //
        // Used only in the async case and only for the initial callback from the pool when connection is established.
        //
        private static void AsyncCallbackWrapper(object request, object state) {
            FtpWebRequest ftpWebRequest = (FtpWebRequest) request;
            ftpWebRequest.RequestCallback(state);
        }

        /// <summary>
        ///    <para>builds networkStream from Socket</para>
        /// </summary>
        private static PooledStream CreateFtpConnection(ConnectionPool pool) {
            return (PooledStream) new FtpControlStream(pool, TimeSpan.MaxValue, false);
        }

        /// <summary>
        /// <para>Aborts underlying connection to FTP server (command & data)</para>
        /// </summary>
        public override void Abort()
        {
            if (m_Aborted)
                return;

            if(Logging.On)Logging.Enter(Logging.Web, this, "Abort", "");

            try {

                GlobalLog.Print("FtpWebRequest#"+ValidationHelper.HashString(this)+"::Abort()");

                if (HttpProxyMode) {
                    GetHttpWebRequest().Abort();
                    return;
                }

                if (CacheProtocol != null)
                    CacheProtocol.Abort();

                Stream stream;
                FtpControlStream connection;
                lock (m_SyncObject)
                {
                    if (m_RequestStage >= RequestStage.ReleaseConnection)
                        return;
                    m_Aborted = true;
                    stream  = m_Stream;
                    connection = m_Connection;
                    m_Exception =  new WebException(NetRes.GetWebStatusString("net_requestaborted", WebExceptionStatus.RequestCanceled),
                                                    WebExceptionStatus.RequestCanceled) ;
                }

                if (stream != null)
                {
                    GlobalLog.Assert(stream is ICloseEx, "FtpWebRequest.Abort()|The m_Stream member is not CloseEx hence the risk of connection been orphaned.");
                    ((ICloseEx)stream).CloseEx(CloseExState.Abort|CloseExState.Silent);
                }
                if (connection != null)
                    connection.Abort(ExceptionHelper.RequestAbortedException);

            } catch (Exception exception) {
                if(Logging.On)Logging.Exception(Logging.Web, this, "Abort", exception);
                throw;
            } finally {
                if(Logging.On)Logging.Exit(Logging.Web, this, "Abort", "");
            }
        }

        /// <include file='doc\FtpWebRequest.uex' path='docs/doc[@for="FtpWebRequest.KeepAlive"]/*' />
        /// <summary>
        /// <para>
        /// If KeepAlive is set to false, then the control connection to the server will be closed when the request completes.
        /// Default is true
        /// </para>
        /// </summary>
        public bool KeepAlive {
            get {
                return m_KeepAlive;
            }
            set {
                if (InUse) {
                    throw new InvalidOperationException(SR.GetString(SR.net_reqsubmitted));
                }
                m_KeepAlive = value;
            }
        }

        /// <summary>
        /// <para>True by default, false allows transmission using text mode</para>
        /// </summary>
        public bool UseBinary {
            get {
                return m_Binary;
            }
            set {
                if (InUse) {
                    throw new InvalidOperationException(SR.GetString(SR.net_reqsubmitted));
                }
                m_Binary = value;
            }
        }

        /// <summary>
        /// <para>False by default, true enables passive mode communication with server.
        /// This alters the way the client talks with the server, allowing the client
        /// to initiate the data connection with the server</para>
        /// </summary>
        public bool UsePassive {
            get {
                return m_Passive;
            }
            set {
                if (InUse) {
                    throw new InvalidOperationException(SR.GetString(SR.net_reqsubmitted));
                }
                m_Passive = value;
            }
        }

#if !FEATURE_PAL
        /// <summary>
        /// <para>
        /// ClientCertificates - sets our certs for our reqest,
        /// uses a hash of the collection to create a private connection
        /// group, to prevent us from using the same Connection as
        /// non-Client Authenticated requests.
        /// </para>
        /// </summary>
        public X509CertificateCollection ClientCertificates {
            get {
                if (m_ClientCertificates == null) {
                    lock (m_SyncObject) {
                        if (m_ClientCertificates == null) {
                            m_ClientCertificates = new X509CertificateCollection();
                        }
                    }
                }
                return m_ClientCertificates;
            }
            set {
                if (value==null) {
                    throw new ArgumentNullException("value");
                }
                m_ClientCertificates = value;
            }
        }
#endif // !FEATURE_PAL

        /// <summary>
        ///    <para>Set to true if we need SSL</para>
        /// </summary>
        public bool EnableSsl {
            get {
                return m_EnableSsl;
            }
            set {
                if (InUse) {
                    throw new InvalidOperationException(SR.GetString(SR.net_reqsubmitted));
                }
                m_EnableSsl = value;
            }
        }


        /// <devdoc>
        ///    <para>
        ///       A collection of headers, currently nothing is return except an empty collection
        ///    </para>
        /// </devdoc>
        public override WebHeaderCollection Headers {
            get {
                if (HttpProxyMode) {
                    return GetHttpWebRequest().Headers;
                }
                if (m_FtpRequestHeaders == null) {
                    m_FtpRequestHeaders         = new WebHeaderCollection(WebHeaderCollectionType.FtpWebRequest);
                }
                return m_FtpRequestHeaders;
            }
            set {
                if (HttpProxyMode) {
                    GetHttpWebRequest().Headers = value;
                }
                m_FtpRequestHeaders = value;
            }
        }

        // NOT SUPPORTED method
        public override string ContentType {
            get {
                throw ExceptionHelper.PropertyNotSupportedException;
            }
            set {
                throw ExceptionHelper.PropertyNotSupportedException;
            }
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

        // NOT SUPPORTED method
        public override bool PreAuthenticate {
            get {
                throw ExceptionHelper.PropertyNotSupportedException;
            }
            set {
                throw ExceptionHelper.PropertyNotSupportedException;
            }
        }

        /// <summary>
        ///    <para>True if a request has been submitted (ie already active)</para>
        /// </summary>
        private bool InUse {
            get {
                if (m_GetRequestStreamStarted || m_GetResponseStarted) {
                    return true;
                } else {
                    return false;
                }
            }
        }

        /// <summary>
        ///    <para>True if request is just wrapping HttpWebRequest</para>
        /// </summary>
        private bool HttpProxyMode {
            get {
                return (m_HttpWebRequest != null);
            }
        }


        /// <summary>
        ///    <para>Creates an FTP WebResponse based off the responseStream and our active Connection</para>
        /// </summary>
        private void EnsureFtpWebResponse(Exception exception)
        {
            if (m_FtpWebResponse == null || (m_FtpWebResponse.GetResponseStream() is FtpWebResponse.EmptyStream && m_Stream != null))
            {
                lock (m_SyncObject) {
                    if (m_FtpWebResponse == null || (m_FtpWebResponse.GetResponseStream() is FtpWebResponse.EmptyStream && m_Stream != null))
                    {
                        Stream responseStream = m_Stream;

                        if (m_MethodInfo.IsUpload) {
                            responseStream = null;
                        }

                        if(m_Stream != null && m_Stream.CanRead && m_Stream.CanTimeout)
                        {
                            m_Stream.ReadTimeout = ReadWriteTimeout;
                            m_Stream.WriteTimeout = ReadWriteTimeout;
                        }

                        FtpControlStream connection = m_Connection;
                        long contentLength = connection != null? connection.ContentLength: -1;

                        if (responseStream == null)
                        {

                            // If the last command was SIZE, we set the ContentLength on
                            // the FtpControlStream to be the size of the file returned in the
                            // response. We should propagate that file size to the response so
                            // users can access it. This also maintains the compatibility with
                            // HTTP when returning size instead of content.
                            if (contentLength < 0)
                                contentLength = 0;
                        }

                        if (m_FtpWebResponse != null)
                        {
                            m_FtpWebResponse.SetResponseStream(responseStream);
                        }
                        else
                        {
                            if (connection != null)
                                m_FtpWebResponse = new FtpWebResponse(responseStream, contentLength, connection.ResponseUri, connection.StatusCode, connection.StatusLine, connection.LastModified, connection.BannerMessage, connection.WelcomeMessage, connection.ExitMessage);
                            else
                                m_FtpWebResponse = new FtpWebResponse(responseStream, -1, m_Uri, FtpStatusCode.Undefined, null, DateTime.Now, null, null, null);
                        }
                    }
                }
            }

            GlobalLog.Print("FtpWebRequest#"+ValidationHelper.HashString(this)+"::EnsureFtpWebResponse returns #"+ValidationHelper.HashString(m_FtpWebResponse)+" with stream#"+ValidationHelper.HashString(m_FtpWebResponse.m_ResponseStream));
            return;
        }

        /// <summary>
        ///    <para>Creates a HttpWebRequest</para>
        /// </summary>
        private HttpWebRequest GetHttpWebRequest() {
            lock (m_SyncObject) {
                if (m_HttpWebRequest == null) {
                    if (m_ContentOffset > 0) {
                        throw new InvalidOperationException(SR.GetString(SR.net_ftp_no_offsetforhttp));
                    }

                    if (!m_MethodInfo.HasHttpCommand)
                        throw new InvalidOperationException(SR.GetString(SR.net_ftp_no_http_cmd));

                    m_HttpWebRequest = new HttpWebRequest(m_Uri, ServicePoint);
                    m_HttpWebRequest.Credentials = Credentials;
                    m_HttpWebRequest.InternalProxy = m_Proxy;
                    m_HttpWebRequest.KeepAlive = KeepAlive;
                    m_HttpWebRequest.Timeout = Timeout;
                    m_HttpWebRequest.Method = m_MethodInfo.HttpCommand;
                    m_HttpWebRequest.CacheProtocol = CacheProtocol;
                    RequestCacheLevel effectiveLevel;
                    if (CachePolicy == null)
                        effectiveLevel = RequestCacheLevel.BypassCache;
                    else
                        effectiveLevel = CachePolicy.Level;

                    // Cannot support revalidate through the proxy
                    if (effectiveLevel == RequestCacheLevel.Revalidate)
                        effectiveLevel = RequestCacheLevel.Reload;
                    m_HttpWebRequest.CachePolicy = new HttpRequestCachePolicy((HttpRequestCacheLevel)effectiveLevel);

                    //disable cache protocol on that class since we are proxying through HTTP
                    CacheProtocol = null;

                }
            }
            return m_HttpWebRequest;
        }


        /// <devdoc>
        ///    <para>Generates a string that
        ///     allows a Connection to remain unique for user
        ///     this is needed to prevent multiple users from
        ///     using the same sockets after they mess with things</para>
        /// </devdoc>
        private string GetConnectionGroupLine() {
            GlobalLog.Print("GetConnectionGroupLine");
            return ConnectionGroupName + "_" + GetUserString();
        }


        /// <summary>
        ///    <para>Returns username string</para>
        /// </summary>
        internal string GetUserString() {
            string name = null;
            if (this.Credentials != null) {
                NetworkCredential networkCreds = this.Credentials.GetCredential(m_Uri, "basic");
                if (networkCreds != null) {
                    name = networkCreds.InternalGetUserName();
                    string domain = networkCreds.InternalGetDomain();
                    if (!ValidationHelper.IsBlankString(domain)) {
                        name = domain+"\\"+name;
                    }
                }
            }
            return name == null? null: (String.Compare(name,"anonymous", StringComparison.InvariantCultureIgnoreCase) == 0? null: name);
        }

        //
        // This method may be invoked as part of the request submission but
        // before the response is received
        // Return:
        // - True       = Use CacheProtocol properties to create the cached response
        // - False      = Proceed with the request submission
        private bool CheckCacheRetrieveBeforeSubmit() {

            if (CacheProtocol == null || m_CacheDone) {
                m_CacheDone = true;
                return false;
            }

            if (CacheProtocol.ProtocolStatus == CacheValidationStatus.CombineCachedAndServerResponse ||
                CacheProtocol.ProtocolStatus == CacheValidationStatus.DoNotTakeFromCache)
            {
                // Re-entry into a new pipeline on failed revalidate or combining cached and live streams
                return false;
            }

            Uri cacheUri = RequestUri;
            string username = GetUserString();
            if(username != null)
                username = Uri.EscapeDataString(username);

            if (cacheUri.Fragment.Length != 0 || username != null)
            {
                if (username == null)
                    cacheUri = new Uri(cacheUri.GetParts(UriComponents.AbsoluteUri & ~(UriComponents.Fragment|UriComponents.UserInfo), UriFormat.SafeUnescaped));
                else
                {
                    username =  cacheUri.GetParts((UriComponents.Scheme | UriComponents.KeepDelimiter), UriFormat.SafeUnescaped) + username + '@';
                    username += cacheUri.GetParts((UriComponents.Host | UriComponents.Port | UriComponents.Path | UriComponents.Query), UriFormat.SafeUnescaped);
                    cacheUri = new Uri(username);
                }
            }

            CacheProtocol.GetRetrieveStatus(cacheUri, this);

            if (CacheProtocol.ProtocolStatus == CacheValidationStatus.Fail) {
                throw CacheProtocol.ProtocolException;
            }

            if (CacheProtocol.ProtocolStatus != CacheValidationStatus.ReturnCachedResponse) {
                return false;
            }

            if (m_MethodInfo.Operation != FtpOperation.DownloadFile) {
                throw new NotSupportedException(SR.GetString(SR.net_cache_not_supported_command));
            }

            if (CacheProtocol.ProtocolStatus == CacheValidationStatus.ReturnCachedResponse)
            {
                // If we take it from cache, we have to kick in response processing
                // The _CacheStream is good to return as the response stream.
                FtpRequestCacheValidator ctx = (FtpRequestCacheValidator) CacheProtocol.Validator;
                m_FtpWebResponse = new FtpWebResponse(CacheProtocol.ResponseStream,
                                                      CacheProtocol.ResponseStreamLength,
                                                      RequestUri,
                                                      UsePassive? FtpStatusCode.DataAlreadyOpen: FtpStatusCode.OpeningData,
                                                      (UsePassive? FtpStatusCode.DataAlreadyOpen: FtpStatusCode.OpeningData).ToString(),
                                                      ctx.CacheEntry.LastModifiedUtc == DateTime.MinValue? DateTime.Now: ctx.CacheEntry.LastModifiedUtc.ToLocalTime(),
                                                      string.Empty,
                                                      string.Empty,
                                                      string.Empty);

                m_FtpWebResponse.InternalSetFromCache = true;
                m_FtpWebResponse.InternalSetIsCacheFresh = (ctx.CacheFreshnessStatus != CacheFreshnessStatus.Stale);
            }
            return true;
        }

        //
        // This method has to be invoked as part of the wire response processing.
        // The wire response can be replaced on return
        //
        // ATTN: If the method returns false, the response is invalid and should be retried
        //
        private bool CheckCacheRetrieveOnResponse() {

            if (CacheProtocol == null || m_CacheDone) {
                return true;
            }

            if (CacheProtocol.ProtocolStatus != CacheValidationStatus.Continue)
            {
                // cache has been already revalidated proceed with cache update
                return true;
            }

            if (CacheProtocol.ProtocolStatus == CacheValidationStatus.Fail) {
                if(Logging.On)Logging.Exception(Logging.Web, this, "CheckCacheRetrieveOnResponse", CacheProtocol.ProtocolException);
                throw CacheProtocol.ProtocolException;
            }

            // At this point we dont have the real data stream, hence passing null and updating it later
            CacheProtocol.GetRevalidateStatus(m_FtpWebResponse, null);

            if (CacheProtocol.ProtocolStatus == CacheValidationStatus.RetryResponseFromServer)
            {
                if (m_FtpWebResponse != null)
                    m_FtpWebResponse.SetResponseStream(null); //prevent from advancing commands pipeline
                // Try to resubmit or fail
                return false;
            }

            if (CacheProtocol.ProtocolStatus != CacheValidationStatus.ReturnCachedResponse)
            {
                // Proceed with the requesting the real server data stream
                return false;
            }

            if (m_MethodInfo.Operation != FtpOperation.DownloadFile)
            {
                // This should never happen in real life
                throw new NotSupportedException(SR.GetString(SR.net_cache_not_supported_command));
            }


            FtpRequestCacheValidator ctx = (FtpRequestCacheValidator) CacheProtocol.Validator;

            FtpWebResponse oldResponse = m_FtpWebResponse;
            m_Stream = CacheProtocol.ResponseStream;
            m_FtpWebResponse = new FtpWebResponse(CacheProtocol.ResponseStream,
                                                  CacheProtocol.ResponseStreamLength,
                                                  RequestUri,
                                                  UsePassive? FtpStatusCode.DataAlreadyOpen: FtpStatusCode.OpeningData,
                                                  (UsePassive? FtpStatusCode.DataAlreadyOpen: FtpStatusCode.OpeningData).ToString(),
                                                  ctx.CacheEntry.LastModifiedUtc == DateTime.MinValue? DateTime.Now: ctx.CacheEntry.LastModifiedUtc.ToLocalTime(),
                                                  string.Empty,
                                                  string.Empty,
                                                  string.Empty);

            m_FtpWebResponse.InternalSetFromCache = true;
            m_FtpWebResponse.InternalSetIsCacheFresh = CacheProtocol.IsCacheFresh;

            oldResponse.Close();
            return true;
        }



        //
        // This will decide on cache update and construct the effective response stream
        //
        private void CheckCacheUpdateOnResponse()
        {
            if (CacheProtocol == null || m_CacheDone) {
                return;
            }
            m_CacheDone = true;

            if (m_Connection != null)
            {
                m_FtpWebResponse.UpdateStatus(m_Connection.StatusCode, m_Connection.StatusLine, m_Connection.ExitMessage);
                if (m_Connection.StatusCode == FtpStatusCode.OpeningData && m_FtpWebResponse.ContentLength == 0)
                    m_FtpWebResponse.SetContentLength(m_Connection.ContentLength);
            }

            if (CacheProtocol.ProtocolStatus == CacheValidationStatus.CombineCachedAndServerResponse)
            {
                // Note we already asked for a file restart
                // The only problem is that we could not create the combined stream sooner.
                m_Stream = new CombinedReadStream(CacheProtocol.Validator.CacheStream, m_FtpWebResponse.GetResponseStream());
                //
                // For consistent user experience we always supply DataAlreadyOpen status for a cached response.
                //
                FtpStatusCode rightStatus = UsePassive? FtpStatusCode.DataAlreadyOpen: FtpStatusCode.OpeningData;

                m_FtpWebResponse.UpdateStatus(rightStatus, rightStatus.ToString(), string.Empty);
                m_FtpWebResponse.SetResponseStream(m_Stream);
            }

            if (CacheProtocol.GetUpdateStatus(m_FtpWebResponse, m_FtpWebResponse.GetResponseStream()) == CacheValidationStatus.UpdateResponseInformation)
            {
                m_Stream = CacheProtocol.ResponseStream;
                m_FtpWebResponse.SetResponseStream(m_Stream);
            }
            else if (CacheProtocol.ProtocolStatus == CacheValidationStatus.Fail)
                throw CacheProtocol.ProtocolException;
        }

        internal void DataStreamClosed(CloseExState closeState)
        {
            if ((closeState & CloseExState.Abort) == 0)
            {
                if (!m_Async)
                {
                    if (m_Connection != null)
                        m_Connection.CheckContinuePipeline();
                }
                else
                {
                    m_RequestCompleteAsyncResult.InternalWaitForCompletion();
                    CheckError();
                }
            }
            else
            {
                FtpControlStream connection = m_Connection;
                if (connection != null)
                    connection.Abort(ExceptionHelper.RequestAbortedException);
            }
        }

        private static int GetStatusCode(WebException webException)
        {
            int result = -1;

            // we are calculating statusCode only when FrameworkEventSource logging is enabled.
            if (FrameworkEventSource.Log.IsEnabled() && webException != null && webException.Response != null) {
                HttpWebResponse httpWebResponse = webException.Response as HttpWebResponse;
                if (httpWebResponse != null) {
                    try {
                        result = (int)httpWebResponse.StatusCode;
                    }
                    catch (ObjectDisposedException) {
                        // ObjectDisposedException is expected here in the following sequuence: ftpWebRequest.GetResponse().Dispose() -> ftpWebRequest.GetResponse()
                        // on the second call to GetResponse() we cannot determine the statusCode.
                    }
                }
                else {
                    var ftpWebResponse = webException.Response as FtpWebResponse;
                    result = GetStatusCode(ftpWebResponse);
                }
            }

            return result;
        }

        private static int GetStatusCode(FtpWebResponse ftpWebResponse)
        {
            int result = -1;

            if (FrameworkEventSource.Log.IsEnabled() && ftpWebResponse != null) {
                try {
                    result = (int)ftpWebResponse.StatusCode;
                }
                catch (ObjectDisposedException) {
                    // ObjectDisposedException is expected here in the following sequuence: ftpWebRequest.GetResponse().Dispose() -> ftpWebRequest.GetResponse()
                    // on the second call to GetResponse() we cannot determine the statusCode.
                }
            }

            return result;
        }

    }  // class FtpWebRequest

    //
    // Class used by the WebRequest.Create factory to create FTP requests
    //
    internal class FtpWebRequestCreator : IWebRequestCreate {
        internal FtpWebRequestCreator() {
        }
        public WebRequest Create(Uri uri) {
            return new FtpWebRequest(uri);
        }
    } // class FtpWebRequestCreator

} //namespace System.Net



