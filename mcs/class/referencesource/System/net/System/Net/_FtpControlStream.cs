// ------------------------------------------------------------------------------
// <copyright file="FtpControlStream.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// ------------------------------------------------------------------------------
//

namespace System.Net {

    using System.Collections;
    using System.IO;
    using System.Security.Cryptography.X509Certificates ;
    using System.Net.Sockets;
    using System.Security.Permissions;
    using System.Text;
    using System.Diagnostics;
    using System.Globalization;
    using System.Net.Cache;

    internal enum FtpPrimitive {
        Upload = 0,
        Download = 1,
        CommandOnly = 2
    };

    internal enum FtpLoginState:byte {
        NotLoggedIn,
        LoggedIn,
        LoggedInButNeedsRelogin,
        ReloginFailed
    };


    /// <devdoc>
    /// <para>
    ///     The FtpControlStream class implements a basic FTP connection,
    ///     This means basic command sending and parsing.
    ///     Queuing is handled by the ConnectionPool, so that a Request is guarenteed
    ///     exclusive access to the Connection.
    ///     This is a pooled object, that will be stored in a pool when idle.
    /// </para>
    /// </devdoc>
    internal class FtpControlStream : CommandStream {

        private Socket         m_DataSocket;
        private IPEndPoint     m_PassiveEndPoint;
        private TlsStream  m_TlsStream;

        private StringBuilder  m_BannerMessage;
        private StringBuilder  m_WelcomeMessage;
        private StringBuilder  m_ExitMessage;
        private WeakReference  m_Credentials;
        private string         m_CurrentTypeSetting = string.Empty;

        private long      m_ContentLength = -1;
        private DateTime  m_LastModified;
        private bool      m_DataHandshakeStarted = false;
        private string    m_LoginDirectory = null;
        private string    m_EstablishedServerDirectory = null;
        private string    m_RequestedServerDirectory = null;
        private Uri       m_ResponseUri;

        private FtpLoginState m_LoginState = FtpLoginState.NotLoggedIn;

        internal FtpStatusCode StatusCode;
        internal string StatusLine;

        internal NetworkCredential Credentials {
            get {
                if (m_Credentials != null && m_Credentials.IsAlive) {
                    return (NetworkCredential) m_Credentials.Target;
                } else {
                    return null;
                }
            }
            set {
                if (m_Credentials == null) {
                    m_Credentials = new WeakReference(null);
                }
                m_Credentials.Target = value;
            }
        }

        private static readonly AsyncCallback m_AcceptCallbackDelegate = new AsyncCallback(AcceptCallback);
        private static readonly AsyncCallback m_ConnectCallbackDelegate = new AsyncCallback(ConnectCallback);
        private static readonly AsyncCallback m_SSLHandshakeCallback = new AsyncCallback(SSLHandshakeCallback);

        /// <devdoc>
        ///    <para>
        ///     Setups and Creates a NetworkStream connection to the server
        ///     perform any initalization if needed
        ///    </para>
        /// </devdoc>
        internal FtpControlStream(
            ConnectionPool connectionPool,
            TimeSpan lifetime,
            bool checkLifetime
            ) : base(connectionPool, lifetime, checkLifetime) {
        }

        /// <summary>
        ///    <para>Closes the connecting socket to generate an error.</para>
        /// </summary>
        internal void AbortConnect() {
            Socket socket = m_DataSocket;
            if (socket != null) {
                try {
                    socket.Close();
                }
                catch (ObjectDisposedException) {
                }
            }
        }

        /// <summary>
        ///    <para>Provides a wrapper for the async accept operations
        /// </summary>
        private static void AcceptCallback(IAsyncResult asyncResult) {
            FtpControlStream connection = (FtpControlStream)asyncResult.AsyncState;
            LazyAsyncResult castedAsyncResult = asyncResult as LazyAsyncResult;
            Socket listenSocket = (Socket)castedAsyncResult.AsyncObject;
            try {
                connection.m_DataSocket = listenSocket.EndAccept(asyncResult);
                if (!connection.ServerAddress.Equals(((IPEndPoint)connection.m_DataSocket.RemoteEndPoint).Address))
                {
                    connection.m_DataSocket.Close();
                    throw new WebException(SR.GetString(SR.net_ftp_active_address_different), WebExceptionStatus.ProtocolError);
                }
                connection.ContinueCommandPipeline();

            } catch (Exception e) {
                connection.CloseSocket();
                connection.InvokeRequestCallback(e);
            } finally {
                listenSocket.Close();
            }
        }
        /// <summary>
        ///    <para>Provides a wrapper for the async accept operations</para>
        /// </summary>
        private static void ConnectCallback(IAsyncResult asyncResult) {
            FtpControlStream connection = (FtpControlStream)asyncResult.AsyncState;
            try {
                LazyAsyncResult castedAsyncResult = asyncResult as LazyAsyncResult;
                Socket dataSocket = (Socket)castedAsyncResult.AsyncObject;
                dataSocket.EndConnect(asyncResult);
                connection.ContinueCommandPipeline();
            } catch (Exception e) {
                connection.CloseSocket();
                connection.InvokeRequestCallback(e);
            }
        }

        //
        // We issue a dummy read on the the SSL data stream to force SSL handshake
        // This callback will will get stream to the user.
        //
        private static void SSLHandshakeCallback(IAsyncResult asyncResult)
        {
            FtpControlStream connection = (FtpControlStream)asyncResult.AsyncState;
            try {
                connection.ContinueCommandPipeline();
            } catch (Exception e) {
                connection.CloseSocket();
                connection.InvokeRequestCallback(e);
            }
        }
        //    Creates a FtpDataStream object, constructs a TLS stream if needed.
        //    In case SSL we issue a 0 bytes read on that stream to force handshake.
        //    In case SSL and ASYNC we delay sigaling the user stream until the handshake is done.
        //
        private PipelineInstruction QueueOrCreateFtpDataStream(ref Stream stream)
        {
            if (m_DataSocket == null)
                throw new InternalException();

            //
            // Re-entered pipeline with completed read on the TlsStream
            //
            if (this.m_TlsStream != null )
            {
                stream = new FtpDataStream(this.m_TlsStream, (FtpWebRequest) m_Request, IsFtpDataStreamWriteable());
                this.m_TlsStream = null;
                return  PipelineInstruction.GiveStream;
            }

            NetworkStream networkStream = new NetworkStream(m_DataSocket, true);

#if !FEATURE_PAL
            if (UsingSecureStream)
            {
                FtpWebRequest request = (FtpWebRequest) m_Request;

                TlsStream tlsStream = new TlsStream(request.RequestUri.Host, networkStream, request.ClientCertificates, Pool.ServicePoint, request, m_Async ? request.GetWritingContext().ContextCopy : null);
                networkStream = tlsStream;

                if (m_Async)
                {
                    this.m_TlsStream = tlsStream;
                    LazyAsyncResult handshakeResult = new LazyAsyncResult(null, this, m_SSLHandshakeCallback);
                    tlsStream.ProcessAuthentication(handshakeResult);
                    return PipelineInstruction.Pause;
                }
                else
                {
                    tlsStream.ProcessAuthentication(null);
                }
            }
#endif // !FEATURE_PAL
            stream = new FtpDataStream(networkStream, (FtpWebRequest) m_Request, IsFtpDataStreamWriteable());
            return  PipelineInstruction.GiveStream;
        }

        /// <summary>
        ///    <para>Cleans up state variables for reuse of the connection</para>
        /// </summary>
        protected override void ClearState() {
            m_ContentLength = -1;
            m_LastModified = DateTime.MinValue;
            m_ResponseUri = null;
            m_DataHandshakeStarted = false;
            StatusCode = FtpStatusCode.Undefined;
            StatusLine = null;

            m_DataSocket = null;
            m_PassiveEndPoint = null;
            m_TlsStream = null;

            base.ClearState();
        }
        //
        //    This is called by underlying base class code, each time a new response is received from the wire or a protocol stage is resumed.
        //    This function controls the seting up of a data socket/connection, and of saving off the server responses
        //
        protected override PipelineInstruction PipelineCallback(PipelineEntry entry, ResponseDescription response, bool timeout, ref Stream stream)
        {
            GlobalLog.Print("FtpControlStream#" + ValidationHelper.HashString(this) + ">" + (entry == null? "null" : entry.Command));
            GlobalLog.Print("FtpControlStream#" + ValidationHelper.HashString(this) + ">" + ((response == null) ? "null" : response.StatusDescription));

            // null response is not expected
            if (response == null)
                return PipelineInstruction.Abort;

            FtpStatusCode status = (FtpStatusCode) response.Status;

            //
            // Update global "current status" for FtpWebRequest
            //
            if (status != FtpStatusCode.ClosingControl)
            {
                // A 221 status won't be reflected on the user FTP response
                // Anything else will (by design?)
                StatusCode = status;
                StatusLine = response.StatusDescription;
            }

            // If the status code is outside the range defined in RFC (1xx to 5xx) throw
            if (response.InvalidStatusCode)
                throw new WebException(SR.GetString(SR.net_InvalidStatusCode), WebExceptionStatus.ProtocolError);
                
            // Update the banner message if any, this is a little hack because the "entry" param is null
            if (m_Index == -1) {
                if (status == FtpStatusCode.SendUserCommand)
                {
                    m_BannerMessage = new StringBuilder();
                    m_BannerMessage.Append(StatusLine);
                    return PipelineInstruction.Advance;
                }
                else if (status == FtpStatusCode.ServiceTemporarilyNotAvailable)
                {
                    return PipelineInstruction.Reread;
                }
                else
                    throw GenerateException(status,response.StatusDescription, null);
            }

            //
            // Check for the result of our attempt to use UTF8
            // Condsider: optimize this for speed (avoid string compare) as that is the only command that may fail
            //
            if (entry.Command == "OPTS utf8 on\r\n")
            {
                if (response.PositiveCompletion) {
                    Encoding = Encoding.UTF8;
                } else {
                    Encoding = Encoding.Default;
                }
                return PipelineInstruction.Advance;
            }

            // If we are already logged in and the server returns 530 then
            // the server does not support re-issuing a USER command,
            // tear down the connection and start all over again
            if (entry.Command.IndexOf("USER") != -1)
            {
                // The server may not require a password for this user, so bypass the password command
                if (status == FtpStatusCode.LoggedInProceed)
                {
                    m_LoginState = FtpLoginState.LoggedIn;
                    m_Index++;
                }
                // The server does not like re-login 
                // (We are logged in already but want to re-login under a different user)
                else if (status == FtpStatusCode.NotLoggedIn && 
                         m_LoginState != FtpLoginState.NotLoggedIn)
                {
                    m_LoginState = FtpLoginState.ReloginFailed;
                    throw ExceptionHelper.IsolatedException;
                }
            }

            //
            // Throw on an error with possible recovery option
            //
            if (response.TransientFailure || response.PermanentFailure) {
                if (status == FtpStatusCode.ServiceNotAvailable) {
                    MarkAsRecoverableFailure();
                }
                throw GenerateException(status,response.StatusDescription, null);
            }

            if (m_LoginState != FtpLoginState.LoggedIn
                && entry.Command.IndexOf("PASS") != -1)
            {
                // Note the fact that we logged in
                if (status == FtpStatusCode.NeedLoginAccount || status == FtpStatusCode.LoggedInProceed)
                    m_LoginState = FtpLoginState.LoggedIn;
                else 
                    throw GenerateException(status,response.StatusDescription, null);
            }

            //
            // Parse special cases
            //
            if (entry.HasFlag(PipelineEntryFlags.CreateDataConnection) && (response.PositiveCompletion || response.PositiveIntermediate))
            {
                bool isSocketReady;
                PipelineInstruction result = QueueOrCreateDataConection(entry, response, timeout, ref stream, out isSocketReady);
                if (!isSocketReady)
                    return result;
                // otheriwse we have a stream to create
            }
            //
            // This is part of the above case and it's all about giving data stream back
            //
            if (status == FtpStatusCode.OpeningData || status == FtpStatusCode.DataAlreadyOpen)
            {
                if (m_DataSocket == null)
                {
                    // a better diagnostic?
                    return PipelineInstruction.Abort;
                }
                if (!entry.HasFlag(PipelineEntryFlags.GiveDataStream))
                {
                    m_AbortReason = SR.GetString(SR.net_ftp_invalid_status_response, status, entry.Command);
                    return PipelineInstruction.Abort;
                }

                // Parse out the Content length, if we can
                TryUpdateContentLength(response.StatusDescription);

                // Parse out the file name, when it is returned and use it for our ResponseUri
                FtpWebRequest request = (FtpWebRequest) m_Request;
                if (request.MethodInfo.ShouldParseForResponseUri)
                {
                    TryUpdateResponseUri(response.StatusDescription, request);
                }

                return QueueOrCreateFtpDataStream(ref stream);
            }


            //
            // Parse responses by status code exclusivelly
            //

            //Update welcome message
            if (status == FtpStatusCode.LoggedInProceed)
            {
                m_WelcomeMessage.Append(StatusLine);
            }
            // OR set the user response ExitMessage
            else if (status == FtpStatusCode.ClosingControl)
            {
                m_ExitMessage.Append(response.StatusDescription);
                // And close the control stream socket on "QUIT"
                CloseSocket();
            }
#if !FEATURE_PAL
            // OR set us up for SSL/TLS, after this we'll be writing securely
            else if (status == FtpStatusCode.ServerWantsSecureSession)
            {
                FtpWebRequest request = (FtpWebRequest) m_Request;
                TlsStream tlsStream = new TlsStream(request.RequestUri.Host, NetworkStream, request.ClientCertificates, Pool.ServicePoint, request, m_Async ? request.GetWritingContext().ContextCopy : null);
                NetworkStream = tlsStream;
            }
#endif // !FEATURE_PAL
            // OR parse out the file size or file time, usually a result of sending SIZE/MDTM commands
            else if (status == FtpStatusCode.FileStatus)
            {
                FtpWebRequest request = (FtpWebRequest) m_Request;
                if (entry.Command.StartsWith("SIZE ")) {
                    m_ContentLength = GetContentLengthFrom213Response(response.StatusDescription);
                } else if (entry.Command.StartsWith("MDTM ")) {
                    m_LastModified = GetLastModifiedFrom213Response(response.StatusDescription);
                }
            }
            // OR parse out our login directory
            else if (status == FtpStatusCode.PathnameCreated)
            {
                if (entry.Command == "PWD\r\n" && !entry.HasFlag(PipelineEntryFlags.UserCommand))
                {
                    m_LoginDirectory = GetLoginDirectory(response.StatusDescription);
                }
            }
            // Asserting we have some positive response
            else
            {
                // We only use CWD to reset ourselves back to the login directory.
                if (entry.Command.IndexOf("CWD") != -1)
                {
                    m_EstablishedServerDirectory = m_RequestedServerDirectory;
                }
            }

            // Intermediate responses require rereading
            if (response.PositiveIntermediate || (!UsingSecureStream && entry.Command == "AUTH TLS\r\n"))
            {
                return PipelineInstruction.Reread;
            }

            return PipelineInstruction.Advance;
        }

        /// <summary>
        ///    <para>Creates an array of commands, that will be sent to the server</para>
        /// </summary>
        protected override PipelineEntry [] BuildCommandsList(WebRequest req) {
            bool resetLoggedInState = false;
            FtpWebRequest request = (FtpWebRequest) req;
            GlobalLog.Print("FtpControlStream#" + ValidationHelper.HashString(this) + "BuildCommandsList");
            m_ResponseUri = request.RequestUri;
            ArrayList commandList = new ArrayList();

#if DEBUG
            // the Credentials.IsEqualTo method is only compiled in DEBUG so the assert must be restricted to DEBUG
            // as well

            // While some FTP servers support it, in general, the RFC's don't allow re-issuing the USER command to 
            // change the authentication context of an existing logged in connection.  We prevent re-using existing 
            // connections if the credentials are different from the previous FtpWebRequest.   Let's make sure that 
            // our connection pooling code is working correctly.
            Debug.Assert(Credentials == null || 
                Credentials.IsEqualTo(request.Credentials.GetCredential(request.RequestUri, "basic")),
                "Should not be re-using an existing connection with different credentials");
#endif

            if (request.EnableSsl && !UsingSecureStream) {
                commandList.Add(new PipelineEntry(FormatFtpCommand("AUTH", "TLS")));
                // According to RFC we need to re-authorize with USER/PASS after we re-authenticate.
                resetLoggedInState = true;
            }

            if (resetLoggedInState) {
                m_LoginDirectory = null;
                m_EstablishedServerDirectory = null;
                m_RequestedServerDirectory = null;
                m_CurrentTypeSetting = string.Empty;
                if (m_LoginState == FtpLoginState.LoggedIn)
                    m_LoginState = FtpLoginState.LoggedInButNeedsRelogin;
            }

            if (m_LoginState != FtpLoginState.LoggedIn) {
                Credentials = request.Credentials.GetCredential(request.RequestUri, "basic");
                m_WelcomeMessage = new StringBuilder();
                m_ExitMessage = new StringBuilder();

                string domainUserName = string.Empty;
                string password = string.Empty;

                if (Credentials != null)
                {
                    domainUserName = Credentials.InternalGetDomainUserName();
                    password       = Credentials.InternalGetPassword();
                }

                if (domainUserName.Length == 0 && password.Length == 0)
                {
                    domainUserName = "anonymous";
                    password       = "anonymous@";
                }

                commandList.Add(new PipelineEntry(FormatFtpCommand("USER", domainUserName)));
                commandList.Add(new PipelineEntry(FormatFtpCommand("PASS", password), PipelineEntryFlags.DontLogParameter));

                // If SSL, always configure data channel encryption after authentication to maximum RFC compatibility.   The RFC allows for
                // PBSZ/PROT commands to come either before or after the USER/PASS, but some servers require USER/PASS immediately after
                // the AUTH TLS command.
                if (request.EnableSsl && !UsingSecureStream)
                {
                    commandList.Add(new PipelineEntry(FormatFtpCommand("PBSZ", "0")));
                    commandList.Add(new PipelineEntry(FormatFtpCommand("PROT", "P")));
                }
                
                commandList.Add(new PipelineEntry(FormatFtpCommand("OPTS", "utf8 on")));
                commandList.Add(new PipelineEntry(FormatFtpCommand("PWD", null)));
            }

            GetPathOption getPathOption = GetPathOption.Normal;

            if (request.MethodInfo.HasFlag(FtpMethodFlags.DoesNotTakeParameter))
            {
                getPathOption = GetPathOption.AssumeNoFilename;
            }
            else if (request.MethodInfo.HasFlag(FtpMethodFlags.ParameterIsDirectory))
            {
                getPathOption = GetPathOption.AssumeFilename;
            }

            string requestPath;
            string requestDirectory;
            string requestFilename;

            GetPathInfo(getPathOption, request.RequestUri, out requestPath, out requestDirectory, out requestFilename);

            if (requestFilename.Length == 0 && request.MethodInfo.HasFlag(FtpMethodFlags.TakesParameter))
                throw new WebException(SR.GetString(SR.net_ftp_invalid_uri));

            // We optimize for having the current working directory staying at the login directory.  This ensure that
            // our relative paths work right and reduces unnecessary CWD commands.
            // Usually, we don't change the working directory except for some FTP commands.  If necessary,
            // we need to reset our working directory back to the login directory.
            if (m_EstablishedServerDirectory != null && m_LoginDirectory != null && m_EstablishedServerDirectory != m_LoginDirectory)
            {
                commandList.Add(new PipelineEntry(FormatFtpCommand("CWD", m_LoginDirectory), PipelineEntryFlags.UserCommand));
                m_RequestedServerDirectory = m_LoginDirectory;
            }

            // For most commands, we don't need to navigate to the directory since we pass in the full
            // path as part of the FTP protocol command.   However,  some commands require it.
            if (request.MethodInfo.HasFlag(FtpMethodFlags.MustChangeWorkingDirectoryToPath) && requestDirectory.Length > 0)
            {
                commandList.Add(new PipelineEntry(FormatFtpCommand("CWD", requestDirectory), PipelineEntryFlags.UserCommand));
                m_RequestedServerDirectory = requestDirectory;
            }
            
            if (request.CacheProtocol != null && request.CacheProtocol.ProtocolStatus == CacheValidationStatus.DoNotTakeFromCache && request.MethodInfo.Operation == FtpOperation.DownloadFile)
                commandList.Add(new PipelineEntry(FormatFtpCommand("MDTM", requestPath)));

            if (!request.MethodInfo.IsCommandOnly)
            {
                // This is why having a protocol logic on the connection is a bad idea
                if (request.CacheProtocol == null || request.CacheProtocol.ProtocolStatus != CacheValidationStatus.Continue)
                {
                    string requestedTypeSetting = request.UseBinary ? "I" : "A";
                    if (m_CurrentTypeSetting != requestedTypeSetting) {
                        commandList.Add(new PipelineEntry(FormatFtpCommand("TYPE", requestedTypeSetting)));
                        m_CurrentTypeSetting = requestedTypeSetting;                        
                    }

                    if (request.UsePassive) {
                        string passiveCommand = (ServerAddress.AddressFamily == AddressFamily.InterNetwork) ? "PASV" : "EPSV";
                        commandList.Add(new PipelineEntry(FormatFtpCommand(passiveCommand, null), PipelineEntryFlags.CreateDataConnection));
                    } else {
                        string portCommand = (ServerAddress.AddressFamily == AddressFamily.InterNetwork) ? "PORT" : "EPRT";
                        CreateFtpListenerSocket(request);
                        commandList.Add(new PipelineEntry(FormatFtpCommand(portCommand, GetPortCommandLine(request))));
                    }

                    if (request.CacheProtocol != null && request.CacheProtocol.ProtocolStatus == CacheValidationStatus.CombineCachedAndServerResponse)
                    {
                        // Combining partial cache with the reminder using "REST"
                        if (request.CacheProtocol.Validator.CacheEntry.StreamSize > 0)
                            commandList.Add(new PipelineEntry(FormatFtpCommand("REST", request.CacheProtocol.Validator.CacheEntry.StreamSize.ToString(CultureInfo.InvariantCulture))));
                    }
                    else if (request.ContentOffset > 0) {
                        // REST command must always be the last sent before the main file command is sent.
                        commandList.Add(new PipelineEntry(FormatFtpCommand("REST", request.ContentOffset.ToString(CultureInfo.InvariantCulture))));
                    }
                }
                else
                {
                    // revalidating GetFileSize = "SIZE" GetDateTimeStamp = "MDTM"
                    commandList.Add(new PipelineEntry(FormatFtpCommand("SIZE", requestPath)));
                    commandList.Add(new PipelineEntry(FormatFtpCommand("MDTM", requestPath)));
                }
            }

            //
            // Suppress the data file if this is a revalidation request
            //
            if (request.CacheProtocol == null || request.CacheProtocol.ProtocolStatus != CacheValidationStatus.Continue)
            {
                PipelineEntryFlags flags = PipelineEntryFlags.UserCommand;
                if (!request.MethodInfo.IsCommandOnly)
                {
                    flags |= PipelineEntryFlags.GiveDataStream;
                    if (!request.UsePassive)
                        flags |= PipelineEntryFlags.CreateDataConnection;
                }

                if (request.MethodInfo.Operation == FtpOperation.Rename)
                {
                    string baseDir = (requestDirectory == string.Empty) 
                        ? string.Empty : requestDirectory + "/";
                    commandList.Add(new PipelineEntry(FormatFtpCommand("RNFR", baseDir + requestFilename), flags));

                    string renameTo;
                    if (!string.IsNullOrEmpty(request.RenameTo)
                        && request.RenameTo.StartsWith("/", StringComparison.OrdinalIgnoreCase))
                    {
                        renameTo = request.RenameTo; // Absolute path
                    }
                    else
                    {
                        renameTo = baseDir + request.RenameTo; // Relative path
                    }
                    commandList.Add(new PipelineEntry(FormatFtpCommand("RNTO", renameTo), flags));
                }
                else if (request.MethodInfo.HasFlag(FtpMethodFlags.DoesNotTakeParameter))
                {
                    commandList.Add(new PipelineEntry(FormatFtpCommand(request.Method, string.Empty), flags));
                }
                else if (request.MethodInfo.HasFlag(FtpMethodFlags.MustChangeWorkingDirectoryToPath))
                {
                    commandList.Add(new PipelineEntry(FormatFtpCommand(request.Method, requestFilename), flags));
                }
                else
                {
                    commandList.Add(new PipelineEntry(FormatFtpCommand(request.Method, requestPath), flags));
                }
            
                if (!request.KeepAlive)
                {
                    commandList.Add(new PipelineEntry(FormatFtpCommand("QUIT", null)));
                }
            }

            return (PipelineEntry []) commandList.ToArray(typeof(PipelineEntry));
        }

        private PipelineInstruction QueueOrCreateDataConection(PipelineEntry entry, ResponseDescription response, bool timeout, ref Stream stream, out bool isSocketReady)
        {
            isSocketReady = false;
            if (m_DataHandshakeStarted)
            {
                isSocketReady = true;
                return PipelineInstruction.Pause; //if we already started then this is re-entering into the callback where we proceed with the stream
            }

            m_DataHandshakeStarted = true;

            // handle passive responses by parsing the port and later doing a Connect(...)
            bool isPassive = false;
            int port = -1;
            if (entry.Command == "PASV\r\n" || entry.Command == "EPSV\r\n")
            {
                if (!response.PositiveCompletion)
                {
                    m_AbortReason = SR.GetString(SR.net_ftp_server_failed_passive, response.Status);
                    return PipelineInstruction.Abort;
                }
                if (entry.Command == "PASV\r\n")
                {
                    port = GetPortV4(response.StatusDescription);
                } else {
                    port = GetPortV6(response.StatusDescription);
                }

                isPassive = true;
            }

            new SocketPermission(PermissionState.Unrestricted).Assert();

            try {
                if (isPassive)
                {
                    GlobalLog.Assert(port != -1, "FtpControlStream#{0}|'port' not set.", ValidationHelper.HashString(this));

                    try {
                        m_DataSocket = CreateFtpDataSocket((FtpWebRequest)m_Request, Socket);
                    } catch (ObjectDisposedException) {
                        throw ExceptionHelper.RequestAbortedException;
                    }

                    IPEndPoint localEndPoint = new IPEndPoint(((IPEndPoint)Socket.LocalEndPoint).Address, 0);
                    m_DataSocket.Bind(localEndPoint);

                    m_PassiveEndPoint = new IPEndPoint(ServerAddress, port);
                }

                PipelineInstruction result;

                if (m_PassiveEndPoint != null)
                {
                    IPEndPoint passiveEndPoint = m_PassiveEndPoint;
                    m_PassiveEndPoint = null;
                    GlobalLog.Print("FtpControlStream#" + ValidationHelper.HashString(this) + "starting Connect()");
                    if (m_Async)
                    {
                        m_DataSocket.BeginConnect(passiveEndPoint, m_ConnectCallbackDelegate, this);
                        result = PipelineInstruction.Pause;
                    }
                    else
                    {
                        m_DataSocket.Connect(passiveEndPoint);
                        result = PipelineInstruction.Advance; // for passive mode we end up going to the next command
                    }
                }
                else
                {
                    GlobalLog.Print("FtpControlStream#" + ValidationHelper.HashString(this) + "starting Accept()");
                    if (m_Async)
                    {
                        m_DataSocket.BeginAccept(m_AcceptCallbackDelegate, this);
                        result = PipelineInstruction.Pause;
                    }
                    else
                    {
                        Socket listenSocket = m_DataSocket;
                        try {
                            m_DataSocket = m_DataSocket.Accept();
                            if (!ServerAddress.Equals(((IPEndPoint)m_DataSocket.RemoteEndPoint).Address))
                            {
                                m_DataSocket.Close();
                                throw new WebException(SR.GetString(SR.net_ftp_active_address_different), WebExceptionStatus.ProtocolError);
                            }
                            isSocketReady = true;   // for active mode we end up creating a stream before advancing the pipeline
                            result = PipelineInstruction.Pause;
                        } finally {
                            listenSocket.Close();
                        }
                    }
                }
                return result;
            } finally {
                SocketPermission.RevertAssert();
            }
        }

        //
        // A door into protected CloseSocket() method
        //
        internal void Quit()
        {
            CloseSocket();
        }

        private enum GetPathOption {
            Normal,
            AssumeFilename,
            AssumeNoFilename
        }

        /// <summary>
        ///    <para>Gets the path componet of the Uri</para>
        /// </summary>
        private static void GetPathInfo(GetPathOption pathOption,
                                                           Uri uri,
                                                           out string path,
                                                           out string directory,
                                                           out string filename)
        {
            path = uri.GetComponents(UriComponents.Path,UriFormat.Unescaped);
            int index = path.LastIndexOf('/');

            if (pathOption == GetPathOption.AssumeFilename &&
                index != -1 && index == path.Length-1) {
                // Remove last '/' and continue normal processing
                path = path.Substring(0, path.Length-1);
                index = path.LastIndexOf('/');
            }

            // split path into directory and filename
            if (pathOption == GetPathOption.AssumeNoFilename) {
                directory = path;
                filename = string.Empty;
            } else {
                directory = path.Substring(0, index+1);
                filename = path.Substring(index+1, path.Length-(index+1));
            }
            
            // strip off trailing '/' on directory if present
            if (directory.Length > 1 && directory[directory.Length-1] == '/')
                directory = directory.Substring(0, directory.Length-1);
        }

        //
        /// <summary>
        ///    <para>Formats an IP address (contained in a UInt32) to a FTP style command string</para>
        /// </summary>
        private String FormatAddress(IPAddress address, int Port )
        {
            byte [] localAddressInBytes = address.GetAddressBytes();

            // produces a string in FTP IPAddress/Port encoding (a1, a2, a3, a4, p1, p2), for sending as a parameter
            // to the port command.
            StringBuilder sb = new StringBuilder(32);
            foreach (byte element in localAddressInBytes) {
                sb.Append(element);
                sb.Append(',');
            }
            sb.Append(Port / 256 );
            sb.Append(',');
            sb.Append(Port % 256 );
            return sb.ToString();
        }

        /// <summary>
        ///    <para>Formats an IP address (v6) to a FTP style command string
        ///    Looks something in this form: |2|1080::8:800:200C:417A|5282| <para>
        ///    |2|4567::0123:5678:0123:5678|0123|
        /// </summary>
        private string FormatAddressV6(IPAddress address, int port) {
            StringBuilder sb = new StringBuilder(43); // based on max size of IPv6 address + port + seperators
            String addressString = address.ToString();
            sb.Append("|2|");
            sb.Append(addressString);
            sb.Append('|');
            sb.Append(port.ToString(NumberFormatInfo.InvariantInfo));
            sb.Append('|');
            return sb.ToString();
        }

        internal long ContentLength {
            get {
                return m_ContentLength;
            }
        }

        internal DateTime LastModified {
            get {
                return m_LastModified;
            }
        }

        internal Uri ResponseUri {
            get {
                return m_ResponseUri;
            }
        }

        /// <summary>
        ///    <para>Returns the server message sent before user credentials are sent</para>
        /// </summary>
        internal string BannerMessage {
            get {
                return (m_BannerMessage != null) ? m_BannerMessage.ToString() : null;
            }
        }

        /// <summary>
        ///    <para>Returns the server message sent after user credentials are sent</para>
        /// </summary>
        internal string WelcomeMessage {
            get {
                return (m_WelcomeMessage != null) ? m_WelcomeMessage.ToString() : null;
            }
        }

        /// <summary>
        ///    <para>Returns the exit sent message on shutdown</para>
        /// </summary>
        internal string ExitMessage {
            get {
                return (m_ExitMessage != null) ? m_ExitMessage.ToString() : null;
            }
        }

        /// <summary>
        ///    <para>Parses a response string for content length</para>
        /// </summary>
        private long GetContentLengthFrom213Response(string responseString) {
            string [] parsedList = responseString.Split(new char [] {' '});
            if (parsedList.Length < 2) 
                throw new FormatException(SR.GetString(SR.net_ftp_response_invalid_format, responseString));
            return Convert.ToInt64(parsedList[1], NumberFormatInfo.InvariantInfo);
        }

        /// <summary>
        ///    <para>Parses a response string for last modified time</para>
        /// </summary>
        private DateTime GetLastModifiedFrom213Response(string str) {
            DateTime dateTime = m_LastModified;
            string [] parsedList = str.Split(new char [] {' ', '.'});
            if (parsedList.Length < 2) {
                return dateTime;
            }
            string dateTimeLine = parsedList[1];
            if (dateTimeLine.Length < 14) {
                return dateTime;
            }
            int year = Convert.ToInt32(dateTimeLine.Substring(0, 4), NumberFormatInfo.InvariantInfo);
            int month = Convert.ToInt16(dateTimeLine.Substring(4, 2), NumberFormatInfo.InvariantInfo);
            int day = Convert.ToInt16(dateTimeLine.Substring(6, 2), NumberFormatInfo.InvariantInfo);
            int hour = Convert.ToInt16(dateTimeLine.Substring(8, 2), NumberFormatInfo.InvariantInfo);
            int minute = Convert.ToInt16(dateTimeLine.Substring(10, 2), NumberFormatInfo.InvariantInfo);
            int second = Convert.ToInt16(dateTimeLine.Substring(12, 2), NumberFormatInfo.InvariantInfo);
            int millisecond = 0;
            if (parsedList.Length > 2) {
                millisecond = Convert.ToInt16(parsedList[2], NumberFormatInfo.InvariantInfo);
            }
            try {
                dateTime = new DateTime(year, month, day, hour, minute, second, millisecond);
                dateTime = dateTime.ToLocalTime(); // must be handled in local time
            } catch (ArgumentOutOfRangeException) {
            } catch (ArgumentException) {
            }
            return dateTime;
        }

        /// <summary>
        ///    <para>Attempts to find the response Uri
        ///     Typical string looks like this, need to get trailing filename
        ///     "150 Opening BINARY mode data connection for FTP46.tmp."</para>
        /// </summary>
        private void TryUpdateResponseUri(string str, FtpWebRequest request)
        {
            Uri baseUri = request.RequestUri;
            //
            // Not sure what we are doing here but I guess the logic is IIS centric
            //
            int start = str.IndexOf("for ");
            if (start == -1)
                return;
            start += 4;
            int end =  str.LastIndexOf('(');
            if (end == -1)
                end = str.Length;
            if (end <= start)
                return;

            string filename = str.Substring(start, end-start);
            filename = filename.TrimEnd(new char [] {' ', '.','\r','\n'});
            // Do minimal escaping that we need to get a valid Uri
            // when combined with the baseUri
            string escapedFilename;
            escapedFilename = filename.Replace("%", "%25");
            escapedFilename = escapedFilename.Replace("#", "%23");

            // help us out if the user forgot to add a slash to the directory name
            string orginalPath = baseUri.AbsolutePath;
            if (orginalPath.Length > 0 && orginalPath[orginalPath.Length-1] != '/') {
                UriBuilder uriBuilder = new UriBuilder(baseUri);
                uriBuilder.Path = orginalPath + "/";
                baseUri = uriBuilder.Uri;
            }

            Uri newUri;
            if (!Uri.TryCreate(baseUri, escapedFilename, out newUri))
            {
                throw new FormatException(SR.GetString(SR.net_ftp_invalid_response_filename, filename));
            } else {
                if (!baseUri.IsBaseOf(newUri) ||
                     baseUri.Segments.Length != newUri.Segments.Length-1)
                {
                    throw new FormatException(SR.GetString(SR.net_ftp_invalid_response_filename, filename));
                }
                else
                {
                    m_ResponseUri = newUri;
                }
            }
        }

        /// <summary>
        ///    <para>Parses a response string for content length</para>
        /// </summary>
        private void TryUpdateContentLength(string str)
        {
            int pos1 = str.LastIndexOf("(");
            if (pos1 != -1)
            {
                int pos2 = str.IndexOf(" bytes).");
                if (pos2 != -1 && pos2 > pos1)
                {
                    pos1++;
                    long result;
                    if (Int64.TryParse (str.Substring(pos1, pos2-pos1),
                                        NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite,
                                        NumberFormatInfo.InvariantInfo, out result))
                    {
                        m_ContentLength = result;
                    }
                }
            }
        }

        /// <summary>
        ///    <para>Parses a response string for a an IP Address</para>
        /// </summary>
        /*
        private string GetIPAddress(string str)
        {
            StringBuilder IPstr=new StringBuilder(32);
            string Substr = null;
            int pos1 = str.IndexOf("(")+1;
            int pos2 = str.IndexOf(",");
            for(int i =0; i<3;i++)
            {
                Substr = str.Substring(pos1,pos2-pos1)+".";
                IPstr.Append(Substr);
                pos1 = pos2+1;
                pos2 = str.IndexOf(",",pos1);
            }
            Substr = str.Substring(pos1,pos2-pos1);
            IPstr.Append(Substr);
            return IPstr.ToString();
        }
        */

        /// <summary>
        ///    <para>Parses a response string for our login dir in " "</para>
        /// </summary>
        private string GetLoginDirectory(string str) {
            int firstQuote = str.IndexOf('"');
            int lastQuote = str.LastIndexOf('"');
            if (firstQuote != -1 && lastQuote != -1 && firstQuote != lastQuote) {
                return str.Substring(firstQuote+1, lastQuote-firstQuote-1);
            } else {
                return String.Empty;
            }
        }

        /// <summary>
        ///    <para>Parses a response string for a port number</para>
        /// </summary>
        private int GetPortV4(string responseString)
        {
            string [] parsedList = responseString.Split(new char [] {' ', '(', ',', ')'});
            
            // We need at least the status code and the port
            if (parsedList.Length <= 7) {
                throw new FormatException(SR.GetString(SR.net_ftp_response_invalid_format, responseString));
            }

            int index = parsedList.Length-1;
            // skip the last non-number token (e.g. terminating '.')
            if (!Char.IsNumber(parsedList[index], 0))
                index--;

            int port = Convert.ToByte(parsedList[index--], NumberFormatInfo.InvariantInfo);
            port = port |
                   (Convert.ToByte(parsedList[index--], NumberFormatInfo.InvariantInfo) << 8);
           
            return port;
        }


        /// <summary>
        ///    <para>Parses a response string for a port number</para>
        /// </summary>
        private int GetPortV6(string responseString)
        {
            int pos1 = responseString.LastIndexOf("(");
            int pos2 = responseString.LastIndexOf(")");
            if (pos1 == -1 || pos2 <= pos1) 
                throw new FormatException(SR.GetString(SR.net_ftp_response_invalid_format, responseString));

            // addressInfo will contain a string of format "|||<tcp-port>|"
            string addressInfo = responseString.Substring(pos1+1, pos2-pos1-1);

            // Although RFC2428 recommends using "|" as the delimiter,
            // It allows ASCII characters in range 33-126 inclusive.
            // We should consider allowing the full range.

            string [] parsedList = addressInfo.Split(new char [] {'|'});
            if (parsedList.Length < 4) 
                throw new FormatException(SR.GetString(SR.net_ftp_response_invalid_format, responseString));
                
            return Convert.ToInt32(parsedList[3], NumberFormatInfo.InvariantInfo);
        }

        /// <summary>
        ///    <para>Creates the Listener socket</para>
        /// </summary>
        private void CreateFtpListenerSocket(FtpWebRequest request) {
            // see \\index1\sdnt\inetcore\wininet\ftp
            // gets an IPEndPoint for the local host for the data socket to bind to.
            IPEndPoint epListener = new IPEndPoint(((IPEndPoint)Socket.LocalEndPoint).Address, 0);
            try {
                m_DataSocket = CreateFtpDataSocket(request, Socket);
            } catch (ObjectDisposedException) {
                throw ExceptionHelper.RequestAbortedException;
            }

            // SECURITY:
            // Since we are doing WebRequest, we don't require SocketPermissions
            // Consider V.Next: Change to declarative form (10x faster) but
            // SocketPermission must be moved out of System.dll for this to work
            new SocketPermission(PermissionState.Unrestricted).Assert();

            try {
                // binds the data socket to the local end point.
                m_DataSocket.Bind(epListener);
                m_DataSocket.Listen(1); // Put the dataSocket * & in Listen mode
            } finally {
                SocketPermission.RevertAssert();
            }
        }


        /// <summary>
        ///    <para>Builds a command line to send to the server with proper port and IP address of client</para>
        /// </summary>
        private string GetPortCommandLine(FtpWebRequest request) {
            try
            {
                // retrieves the IP address of the local endpoint
                IPEndPoint localEP = (IPEndPoint) m_DataSocket.LocalEndPoint;
                if (ServerAddress.AddressFamily == AddressFamily.InterNetwork) {
                    return FormatAddress(localEP.Address, localEP.Port);
                } else if (ServerAddress.AddressFamily == AddressFamily.InterNetworkV6) {
                    return FormatAddressV6(localEP.Address, localEP.Port);
                } else {
                    throw new InternalException();
                }
            }
            catch(Exception e)
            {
                throw GenerateException(WebExceptionStatus.ProtocolError, e); // could not open data connection
            }
        }

        /// <summary>
        ///    <para>Formats a simple FTP command + parameter in correct pre-wire format</para>
        /// </summary>
        private string FormatFtpCommand(string command, string parameter)
        {
            StringBuilder stringBuilder = new StringBuilder(command.Length + ((parameter != null) ? parameter.Length : 0) + 3 /*size of ' ' \r\n*/);
            stringBuilder.Append(command);
            if(!ValidationHelper.IsBlankString(parameter)) {
                stringBuilder.Append(' ');
                stringBuilder.Append(parameter);
            }
            stringBuilder.Append("\r\n");
            return stringBuilder.ToString();
        }


        /// <devdoc>
        ///    <para>
        ///     This will handle either connecting to a port or listening for one
        ///    </para>
        /// </devdoc>
        protected Socket CreateFtpDataSocket(FtpWebRequest request, Socket templateSocket)
        {
            // Safe to be called under an Assert.
            Socket socket = new Socket( templateSocket.AddressFamily, templateSocket.SocketType, templateSocket.ProtocolType );
            return socket;
        }

        /// <summary>
        /// This function is called by the GeneralWebRequest superclass to determine whether a response is valid, and when it is complete.
        /// It also gives the response description a
        /// </summary>
        protected override bool CheckValid(ResponseDescription response, ref int validThrough, ref int completeLength) {
            GlobalLog.Print("FtpControlStream#" + ValidationHelper.HashString(this) + "CheckValid(" + response.StatusBuffer.ToString() + ")" );
             // If the response is less than 4 bytes long, it is too short to tell, so return true, valid so far.
            if(response.StatusBuffer.Length < 4) {
                return true;
            }
            string responseString = response.StatusBuffer.ToString();

            // Otherwise, if there is no status code for this response yet, get one.
            if(response.Status == ResponseDescription.NoStatus)
            {
                // If the response does not start with three digits, then it is not a valid response from an FTP server.
                if(!(Char.IsDigit(responseString[0]) && Char.IsDigit(responseString[1]) && Char.IsDigit(responseString[2]) && (responseString[3] == ' ' || responseString[3] == '-'))) {
                    return false;
                } else {
                    response.StatusCodeString = responseString.Substring(0, 3);
                    response.Status = Convert.ToInt16(response.StatusCodeString, NumberFormatInfo.InvariantInfo);
                }

                // IF a hyphen follows the status code on the first line of the response, then we have a multiline response coming.
                if (responseString[3] == '-') {
                    response.Multiline = true;
                }
            }

            // If a complete line of response has been received from the server, then see if the
            // overall response is complete.
            // If this was not a multiline response, then the response is complete at the end of the line.

            // If this was a multiline response (indicated by three digits followed by a '-' in the first line,
            // then we see if the last line received started with the same three digits followed by a space.
            // If it did, then this is the sign of a complete multiline response.
            // If the line contained three other digits followed by the response, then this is a violation of the
            // FTP protocol for multiline responses.
            // All other cases indicate that the response is not yet complete.
            int index = 0;
            while((index = responseString.IndexOf("\r\n", validThrough)) != -1)  // gets the end line.
            {
                int lineStart = validThrough;
                validThrough = index + 2;  // validThrough now marks the end of the line being examined.
                if(!response.Multiline)
                {
                    completeLength = validThrough;
                    return true;
                } // same here

                if(responseString.Length > lineStart + 4)
                {
                    // if the first three characters of the the response line currently being examined
                    // match the status code, then if they are followed by a space, then we
                    // have reached the end of the reply.
                    if(responseString.Substring(lineStart, 3) == response.StatusCodeString)
                    {
                        if(responseString[lineStart + 3] == ' ')
                        {
                            completeLength = validThrough;
                            return true;
                        }
                    }
                }
            }
            return true;
        }

        /// <summary>
        ///    <para>Determnines whether the stream we return is Writeable or Readable</para>
        /// </summary>
        private TriState IsFtpDataStreamWriteable() {
            FtpWebRequest request = m_Request as FtpWebRequest;
            if (request != null) {
                if (request.MethodInfo.IsUpload) {
                    return TriState.True;
                } else if (request.MethodInfo.IsDownload) {
                    return TriState.False;
                }
            }
            return TriState.Unspecified;
        }

    } // class FtpControlStream

} // namespace System.Net

