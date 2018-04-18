namespace System.Net.Mail
{
    using System;
    using System.Net;
    using System.Security.Cryptography.X509Certificates;
    using System.IO;
    using System.Threading;
    using System.Globalization;
    using System.Security.Principal;
    using System.Security.Permissions;
    using System.Security.Authentication.ExtendedProtection;
    using System.Diagnostics;



    class SmtpConnection
    {

        private static PooledStream CreateSmtpPooledStream(ConnectionPool pool)         {
            return (PooledStream)new SmtpPooledStream(pool, TimeSpan.MaxValue, false);
        }


        private static readonly CreateConnectionDelegate m_CreateConnectionCallback = new CreateConnectionDelegate(CreateSmtpPooledStream);
        private static readonly ContextCallback s_AuthenticateCallback = new ContextCallback(AuthenticateCallback);

        BufferBuilder bufferBuilder = new BufferBuilder();
        bool isConnected;
        bool isClosed;
        bool isStreamOpen;
        bool sawNegotiate;
        EventHandler onCloseHandler;
        internal SmtpTransport parent;
        internal SmtpClient client;
        SmtpReplyReaderFactory responseReader;

        // accounts for the '=' or ' ' character after AUTH
        const int sizeOfAuthString = 5;
        const int sizeOfAuthExtension = 4;
        // string comparisons for these MUST be case-insensitive
        const string authExtension = "auth";
        const string authLogin = "login";
        const string authNtlm = "ntlm";
        const string authGssapi = "gssapi";
        const string authWDigest = "wdigest";

        PooledStream pooledStream;
        ConnectionPool connectionPool;
        SupportedAuth supportedAuth = SupportedAuth.None;
        bool serverSupportsStartTls = false;
        ISmtpAuthenticationModule[] authenticationModules;
        ICredentialsByHost credentials;
        int timeout = 100000;
        string[] extensions;
        private ChannelBinding channelBindingToken = null;

        bool enableSsl;
        X509CertificateCollection clientCertificates;

        internal SmtpConnection(SmtpTransport parent, SmtpClient client, ICredentialsByHost credentials, ISmtpAuthenticationModule[] authenticationModules)
        {
            this.client = client;
            this.credentials = credentials;
            this.authenticationModules = authenticationModules;
            this.parent = parent;
            onCloseHandler = new EventHandler(OnClose);
        }

        internal BufferBuilder BufferBuilder
        {
            get
            {
                return bufferBuilder;
            }
        }

        internal bool IsConnected
        {
            get
            {
                return isConnected;
            }
        }

        internal bool IsStreamOpen
        {
            get
            {
                return isStreamOpen;
            }
        }

        internal bool DSNEnabled
        {
            get
            {
                if (pooledStream != null)
                    return ((SmtpPooledStream)pooledStream).dsnEnabled;
                else
                    return false;
            }
        }

        internal SmtpReplyReaderFactory Reader
        {
            get
            {
                return responseReader;
            }
        }

        internal bool EnableSsl
        {
            get
            {
                return enableSsl;
            }
            set
            {
#if !FEATURE_PAL
                enableSsl = value;
#else
                throw new NotImplementedException("ROTORTODO");
#endif
            }
        }

        internal int Timeout
        {
            get
            {
                return timeout;
            }
            set
            {
                timeout = value;
            }
        }


        internal X509CertificateCollection ClientCertificates
        {
            get
            {
                return clientCertificates;
            }
            set
            {
                clientCertificates = value;
            }
        }

        internal bool ServerSupportsEai
        {
            get 
            { 
                SmtpPooledStream smtpPooledStream = (SmtpPooledStream)pooledStream;
                Debug.Assert(smtpPooledStream != null, "PooledStream not yet set");
                return smtpPooledStream.serverSupportsEai; 
            }
        }

        internal IAsyncResult BeginGetConnection(ServicePoint servicePoint, ContextAwareResult outerResult, AsyncCallback callback, object state)
        {
            if (Logging.On) Logging.Associate(Logging.Web, this, servicePoint);
            Debug.Assert(servicePoint != null, "servicePoint was null from SmtpTransport");

            if (EnableSsl && ClientCertificates != null && ClientCertificates.Count > 0)
                connectionPool = ConnectionPoolManager.GetConnectionPool(servicePoint, ClientCertificates.GetHashCode().ToString(NumberFormatInfo.InvariantInfo), m_CreateConnectionCallback);
            else
                connectionPool = ConnectionPoolManager.GetConnectionPool(servicePoint, "", m_CreateConnectionCallback);

            ConnectAndHandshakeAsyncResult result = new ConnectAndHandshakeAsyncResult(this, servicePoint.Host, servicePoint.Port, outerResult, callback, state);
            result.GetConnection(false);
            return result;
        }


        internal IAsyncResult BeginFlush(AsyncCallback callback, object state)
        {
            return pooledStream.UnsafeBeginWrite(bufferBuilder.GetBuffer(), 0, bufferBuilder.Length, callback, state);
        }

        internal void EndFlush(IAsyncResult result)
        {
            pooledStream.EndWrite(result);
            bufferBuilder.Reset();
        }

        internal void Flush()
        {
            pooledStream.Write(bufferBuilder.GetBuffer(), 0, bufferBuilder.Length);
            bufferBuilder.Reset();
        }

        internal void ReleaseConnection()
        {
            if (!isClosed) {
                lock (this) {

                    if (!isClosed && pooledStream != null) {
                        
                        //free cbt buffer
                        if (channelBindingToken != null){
                            channelBindingToken.Close();
                        }

                        GlobalLog.Print("SmtpConnectiont#" + ValidationHelper.HashString(this) + "::Close Transport#" + ValidationHelper.HashString(parent) + "putting back pooledStream#" + ValidationHelper.HashString(pooledStream));

                        ((SmtpPooledStream)pooledStream).previouslyUsed = true;
                        connectionPool.PutConnection(pooledStream, pooledStream.Owner, Timeout);
                    }
                    isClosed = true;
                }
            }
            isConnected = false;
        }

        internal void Abort()
        {
            if (!isClosed) {
                lock (this) {
                    if (!isClosed && pooledStream != null){

                        GlobalLog.Print("SmtpConnectiont#" + ValidationHelper.HashString(this) + "::Close Transport#" + ValidationHelper.HashString(parent) + "closing and putting back pooledStream#" + ValidationHelper.HashString(pooledStream));
                        
                        //free CBT buffer
                        if (this.channelBindingToken != null){
                            channelBindingToken.Close();
                        }

                        // must destroy manually since sending a QUIT here might not be 
                        // interpreted correctly by the server if it's in the middle of a
                        // DATA command or some similar situation.  This may send a RST
                        // but this is ok in this situation.  Do not reuse this connection
                        pooledStream.Close(0);
                        connectionPool.PutConnection(pooledStream, pooledStream.Owner, Timeout, false);
                    }
                    isClosed = true;
                }
            }
            isConnected = false;
        }

        internal void ParseExtensions(string[] extensions) {
            supportedAuth = SupportedAuth.None;
            foreach (string extension in extensions) {
                if (String.Compare(extension, 0, authExtension, 0, 
                    sizeOfAuthExtension, StringComparison.OrdinalIgnoreCase) == 0)  {
                    // remove the AUTH text including the following character 
                    // to ensure that split only gets the modules supported
                    string[] authTypes = 
                        extension.Remove(0, sizeOfAuthExtension).Split(new char[] { ' ', '=' },
                        StringSplitOptions.RemoveEmptyEntries);
                    foreach (string authType in authTypes) {
                        if (String.Compare(authType, authLogin, StringComparison.OrdinalIgnoreCase) == 0) {
                            supportedAuth |= SupportedAuth.Login;
                        }
#if !FEATURE_PAL
                        else if (String.Compare(authType, authNtlm, StringComparison.OrdinalIgnoreCase) == 0) {
                            supportedAuth |= SupportedAuth.NTLM;
                        }
                        else if (String.Compare(authType, authGssapi, StringComparison.OrdinalIgnoreCase) == 0) {
                            supportedAuth |= SupportedAuth.GSSAPI;
                        }
                        else if (String.Compare(authType, authWDigest, StringComparison.OrdinalIgnoreCase) == 0) {
                            supportedAuth |= SupportedAuth.WDigest;
                        }
#endif // FEATURE_PAL
                    }
                }
                else if (String.Compare(extension, 0, "dsn ", 0, 3, StringComparison.OrdinalIgnoreCase) == 0) {
                    ((SmtpPooledStream)pooledStream).dsnEnabled = true;
                }
                else if (String.Compare(extension, 0, "STARTTLS", 0, 8, StringComparison.OrdinalIgnoreCase) == 0) {
                    serverSupportsStartTls = true;
                }
                else if (String.Compare(extension, 0, "SMTPUTF8", 0, 8, StringComparison.OrdinalIgnoreCase) == 0) {
                    ((SmtpPooledStream)pooledStream).serverSupportsEai = true;
                }
            }
        }

        internal bool AuthSupported(ISmtpAuthenticationModule module){
            if (module is SmtpLoginAuthenticationModule) {
                if ((supportedAuth & SupportedAuth.Login) > 0) {
                    return true;
                }
            }
#if !FEATURE_PAL
            else if (module is SmtpNegotiateAuthenticationModule) {
                if ((supportedAuth & SupportedAuth.GSSAPI) > 0) {
                    sawNegotiate = true;
                    return true;
                }
            }
            else if (module is SmtpNtlmAuthenticationModule) {
                //don't try ntlm if negotiate has been tried
                if ((!sawNegotiate && (supportedAuth & SupportedAuth.NTLM) > 0)) {
                    return true;
                }
            }
            else if (module is SmtpDigestAuthenticationModule) {
                if ((supportedAuth & SupportedAuth.WDigest) > 0) {
                    return true;
                }
            }
#endif // FEATURE_PAL

            return false;
        }


        internal void GetConnection(ServicePoint servicePoint)
        {
            if (isConnected)
            {
                throw new InvalidOperationException(SR.GetString(SR.SmtpAlreadyConnected));
            }

            if (Logging.On) Logging.Associate(Logging.Web, this, servicePoint);
            Debug.Assert(servicePoint != null, "servicePoint was null from SmtpTransport");
            connectionPool = ConnectionPoolManager.GetConnectionPool(servicePoint, "", m_CreateConnectionCallback);

            PooledStream pooledStream = connectionPool.GetConnection((object)this, null, Timeout);

            while (((SmtpPooledStream)pooledStream).creds != null && ((SmtpPooledStream)pooledStream).creds != credentials) {
                // destroy this connection so that a new connection can be created 
                // in order to use the proper credentials.  Do not just close the 
                // connection since it's in a state where a QUIT could be sent
                connectionPool.PutConnection(pooledStream, pooledStream.Owner, Timeout, false);
                pooledStream = connectionPool.GetConnection((object)this, null, Timeout);
            }
            if (Logging.On) Logging.Associate(Logging.Web, this, pooledStream);

            lock (this) {
                this.pooledStream = pooledStream;
            }

            ((SmtpPooledStream)pooledStream).creds = credentials;

            responseReader = new SmtpReplyReaderFactory(pooledStream.NetworkStream);

            //set connectionlease
            pooledStream.UpdateLifetime();

            //if the stream was already used, then we've already done the handshake
            if (((SmtpPooledStream)pooledStream).previouslyUsed == true) {
                isConnected = true;
                return;
            }

            LineInfo info = responseReader.GetNextReplyReader().ReadLine();

            switch (info.StatusCode) 
            {
                case SmtpStatusCode.ServiceReady: 
                    {
                        break;
                    }
                default: 
                    {
                        throw new SmtpException(info.StatusCode, info.Line, true);
                    }
            }

            try
            {
                extensions = EHelloCommand.Send(this, client.clientDomain);
                ParseExtensions(extensions);
            }
            catch (SmtpException e)
            {
                if ((e.StatusCode != SmtpStatusCode.CommandUnrecognized)
                    && (e.StatusCode != SmtpStatusCode.CommandNotImplemented)) {
                    throw e;
                }

                HelloCommand.Send(this, client.clientDomain);
                //if ehello isn't supported, assume basic login
                supportedAuth = SupportedAuth.Login;
            }

#if !FEATURE_PAL
            // Establish TLS
            if (enableSsl) 
            {
                if (!serverSupportsStartTls) 
                {
                    // Either TLS is already established or server does not support TLS
                    if (!(pooledStream.NetworkStream is TlsStream)) 
                    {
                        throw new SmtpException(SR.GetString(SR.MailServerDoesNotSupportStartTls));
                    }
                }
                StartTlsCommand.Send(this);
                TlsStream TlsStream = new TlsStream(servicePoint.Host, pooledStream.NetworkStream, clientCertificates, servicePoint, client, null);

                pooledStream.NetworkStream = TlsStream;

                //for SMTP, the CBT should be unique
                this.channelBindingToken = TlsStream.GetChannelBinding(ChannelBindingKind.Unique);

                responseReader = new SmtpReplyReaderFactory(pooledStream.NetworkStream);

                // According to RFC 3207: The client SHOULD send an EHLO command 
                // as the first command after a successful TLS negotiation.
                extensions = EHelloCommand.Send(this, client.clientDomain);
                ParseExtensions(extensions);
            }
#endif // !FEATURE_PAL

            //if no credentials were supplied, try anonymous
            //servers don't appear to anounce that they support anonymous login.
            if (credentials != null) {

                for (int i = 0; i < authenticationModules.Length; i++) 
                {

                    //only authenticate if the auth protocol is supported  - Microsoft
                    if (!AuthSupported(authenticationModules[i])) {
                        continue;
                    }

                    NetworkCredential credential = credentials.GetCredential(servicePoint.Host, 
                        servicePoint.Port, authenticationModules[i].AuthenticationType);
                    if (credential == null)
                        continue;

                    Authorization auth = SetContextAndTryAuthenticate(authenticationModules[i], credential, null);

                    if (auth != null && auth.Message != null) 
                    {
                        info = AuthCommand.Send(this, authenticationModules[i].AuthenticationType, auth.Message);

                        if (info.StatusCode == SmtpStatusCode.CommandParameterNotImplemented) 
                        {
                            continue;
                        }

                        while ((int)info.StatusCode == 334) 
                        {
                            auth = authenticationModules[i].Authenticate(info.Line, null, this, this.client.TargetName, this.channelBindingToken);
                            if (auth == null)
                            {
                                throw new SmtpException(SR.GetString(SR.SmtpAuthenticationFailed));
                            }
                            info = AuthCommand.Send(this, auth.Message);

                            if ((int)info.StatusCode == 235)
                            {
                                authenticationModules[i].CloseContext(this);
                                isConnected = true;
                                return;
                            }
                        }
                    }
                }
            }
            isConnected = true;
        }

        //
        // We may need to impersonate in this method
        //
        [SecurityPermission(SecurityAction.Assert, Flags = SecurityPermissionFlag.ControlPrincipal)]
        private Authorization SetContextAndTryAuthenticate(ISmtpAuthenticationModule module, NetworkCredential credential, ContextAwareResult context)
        {
#if !FEATURE_PAL
            // We may need to restore user thread token here
            if (credential is SystemNetworkCredential)
            {
                // 
#if DEBUG
                GlobalLog.Assert(context == null || context.IdentityRequested, "SmtpConnection#{0}::SetContextAndTryAuthenticate|Authentication required when it wasn't expected.  (Maybe Credentials was changed on another thread?)", ValidationHelper.HashString(this));
#endif

                WindowsIdentity w = context == null ? null : context.Identity;
                try
                {
                    IDisposable ctx = w == null ? null : w.Impersonate();
                    if (ctx != null)
                    {
                        using (ctx)
                        {
                            return module.Authenticate(null, credential, this, this.client.TargetName, this.channelBindingToken);
                        }
                    }
                    else
                    {
                        ExecutionContext x = context == null ? null : context.ContextCopy;
                        if (x != null)
                        {
                            AuthenticateCallbackContext authenticationContext =
                                new AuthenticateCallbackContext(this, module, credential, this.client.TargetName, this.channelBindingToken);

                            ExecutionContext.Run(x, s_AuthenticateCallback, authenticationContext);
                            return authenticationContext.result;
                        }
                        else
                        {
                            return module.Authenticate(null, credential, this, this.client.TargetName, this.channelBindingToken);
                        }
                    }
                }
                catch
                {
                    // Prevent the impersonation from leaking to upstack exception filters.
                    throw;
                }
            }
#endif // !FEATURE_PAL
            return module.Authenticate(null, credential, this, this.client.TargetName, this.channelBindingToken);
        }

        private static void AuthenticateCallback(object state)
        {
            AuthenticateCallbackContext context = (AuthenticateCallbackContext)state;
            context.result = context.module.Authenticate(null, context.credential, context.thisPtr, context.spn, context.token);
        }

        private class AuthenticateCallbackContext
        {
            internal AuthenticateCallbackContext(SmtpConnection thisPtr, ISmtpAuthenticationModule module, NetworkCredential credential, string spn, ChannelBinding Token)
            {
                this.thisPtr = thisPtr;
                this.module = module;
                this.credential = credential;
                this.spn = spn;
                this.token = Token;

                this.result = null;
            }

            internal readonly SmtpConnection thisPtr;
            internal readonly ISmtpAuthenticationModule module;
            internal readonly NetworkCredential credential;
            internal readonly string spn;
            internal readonly ChannelBinding token;

            internal Authorization result;
        }

        internal void EndGetConnection(IAsyncResult result)
        {
            ConnectAndHandshakeAsyncResult.End(result);
        }

        internal Stream GetClosableStream()
        {
            ClosableStream cs = new ClosableStream(pooledStream.NetworkStream, onCloseHandler);
            isStreamOpen = true;
            return cs;
        }

        void OnClose(object sender, EventArgs args)
        {
            isStreamOpen = false;

            DataStopCommand.Send(this);
        }

        class ConnectAndHandshakeAsyncResult : LazyAsyncResult
        {

            private static readonly GeneralAsyncDelegate m_ConnectionCreatedCallback = new GeneralAsyncDelegate(ConnectionCreatedCallback);
            string authResponse;
            SmtpConnection connection;
            int currentModule = -1;
            int port;
            static AsyncCallback handshakeCallback = new AsyncCallback(HandshakeCallback);
            static AsyncCallback sendEHelloCallback = new AsyncCallback(SendEHelloCallback);
            static AsyncCallback sendHelloCallback = new AsyncCallback(SendHelloCallback);
            static AsyncCallback authenticateCallback = new AsyncCallback(AuthenticateCallback);
            static AsyncCallback authenticateContinueCallback = new AsyncCallback(AuthenticateContinueCallback);
            string host;

            private readonly ContextAwareResult m_OuterResult;


            internal ConnectAndHandshakeAsyncResult(SmtpConnection connection, string host, int port, ContextAwareResult outerResult, AsyncCallback callback, object state) :
                base(null, state, callback)
            {
                this.connection = connection;
                this.host = host;
                this.port = port;

                m_OuterResult = outerResult;
            }


            private static void ConnectionCreatedCallback(object request, object state) {
                GlobalLog.Enter("ConnectAndHandshakeAsyncResult#" + ValidationHelper.HashString(request) + "::ConnectionCreatedCallback");
                ConnectAndHandshakeAsyncResult ConnectAndHandshakeAsyncResult = (ConnectAndHandshakeAsyncResult)request;
                if (state is Exception) {
                    ConnectAndHandshakeAsyncResult.InvokeCallback((Exception)state);
                    return;
                }
                SmtpPooledStream pooledStream = (SmtpPooledStream)(PooledStream)state;


                try
                {
                    while (pooledStream.creds != null && pooledStream.creds != ConnectAndHandshakeAsyncResult.connection.credentials) {
                        GlobalLog.Print("ConnectAndHandshakeAsyncResult#" + ValidationHelper.HashString(request) + "::Connect pooledStream has wrong creds " + ValidationHelper.HashString(pooledStream));
                        ConnectAndHandshakeAsyncResult.connection.connectionPool.PutConnection(pooledStream,
                            pooledStream.Owner, ConnectAndHandshakeAsyncResult.connection.Timeout, false);
                        pooledStream = (SmtpPooledStream)ConnectAndHandshakeAsyncResult.connection.connectionPool.GetConnection((object)ConnectAndHandshakeAsyncResult, ConnectAndHandshakeAsyncResult.m_ConnectionCreatedCallback, ConnectAndHandshakeAsyncResult.connection.Timeout);
                        if (pooledStream == null) {
                            GlobalLog.Leave("ConnectAndHandshakeAsyncResult#" + ValidationHelper.HashString(request) + "::Connect returning asynchronously");
                            return;
                        }
                    }
                    if (Logging.On) Logging.Associate(Logging.Web, ConnectAndHandshakeAsyncResult.connection, pooledStream);
                    pooledStream.Owner = ConnectAndHandshakeAsyncResult.connection; //needs to be updated for gc reasons
                    pooledStream.creds = ConnectAndHandshakeAsyncResult.connection.credentials;


                    lock (ConnectAndHandshakeAsyncResult.connection) {

                        //if we were cancelled while getting the connection, we should close and return
                        if (ConnectAndHandshakeAsyncResult.connection.isClosed) {
                            ConnectAndHandshakeAsyncResult.connection.connectionPool.PutConnection(pooledStream, pooledStream.Owner, ConnectAndHandshakeAsyncResult.connection.Timeout, false);
                            GlobalLog.Print("ConnectAndHandshakeAsyncResult#" + ValidationHelper.HashString(request) + "::ConnectionCreatedCallback Connect was aborted " + ValidationHelper.HashString(pooledStream));
                            ConnectAndHandshakeAsyncResult.InvokeCallback(null);
                            return;
                        }
                        ConnectAndHandshakeAsyncResult.connection.pooledStream = pooledStream;
                    }

                    ConnectAndHandshakeAsyncResult.Handshake();
                }
                catch (Exception e)
                {
                    ConnectAndHandshakeAsyncResult.InvokeCallback(e);
                }
                GlobalLog.Leave("ConnectAndHandshakeAsyncResult#" + ValidationHelper.HashString(request) + "::ConnectionCreatedCallback pooledStream#" + ValidationHelper.HashString(pooledStream));
            }


            internal static void End(IAsyncResult result)
            {
                ConnectAndHandshakeAsyncResult thisPtr = (ConnectAndHandshakeAsyncResult)result;
                object connectResult = thisPtr.InternalWaitForCompletion();
                if (connectResult is Exception){
                    throw (Exception)connectResult;
                }
            }

            internal void GetConnection(bool synchronous)
            {

                GlobalLog.Enter("ConnectAndHandshakeAsyncResult#" + ValidationHelper.HashString(this) + "::Connect: sync=" + (synchronous ? "true" : "false"));
                if (connection.isConnected)
                {
                    throw new InvalidOperationException(SR.GetString(SR.SmtpAlreadyConnected));
                }


                SmtpPooledStream pooledStream = (SmtpPooledStream)connection.connectionPool.GetConnection((object)this, (synchronous ? null : m_ConnectionCreatedCallback), connection.Timeout);
                GlobalLog.Print("ConnectAndHandshakeAsyncResult#" + ValidationHelper.HashString(this) + "::Connect returned" + ValidationHelper.HashString(this));

                if (pooledStream != null) {
                    try
                    {
                        while (pooledStream.creds != null && pooledStream.creds != connection.credentials) {
                            GlobalLog.Print("ConnectAndHandshakeAsyncResult#" + ValidationHelper.HashString(this) + "::Connect pooledStream has wrong creds " + ValidationHelper.HashString(pooledStream));
                            connection.connectionPool.PutConnection(pooledStream, pooledStream.Owner, connection.Timeout, false);
                            pooledStream = (SmtpPooledStream)connection.connectionPool.GetConnection((object)this, (synchronous ? null : m_ConnectionCreatedCallback), connection.Timeout);
                            if (pooledStream == null) {
                                GlobalLog.Leave("ConnectAndHandshakeAsyncResult#" + ValidationHelper.HashString(this) + "::Connect returning asynchronously");
                                return;
                            }
                        }
                        pooledStream.creds = connection.credentials;
                        pooledStream.Owner = this.connection; //needs to be updated for gc reasons

                        lock (connection) {
                            connection.pooledStream = pooledStream;
                        }
                        Handshake();
                    }
                    catch (Exception e)
                    {
                        InvokeCallback(e);
                    }
                }
                GlobalLog.Leave("ConnectAndHandshakeAsyncResult#" + ValidationHelper.HashString(this) + "::Connect pooledStream#" + ValidationHelper.HashString(pooledStream));
            }


            void Handshake()
            {
                connection.responseReader = new SmtpReplyReaderFactory(connection.pooledStream.NetworkStream);


                //if we've already used this stream, then we've already done the handshake

                //set connectionlease
                connection.pooledStream.UpdateLifetime();

                if (((SmtpPooledStream)connection.pooledStream).previouslyUsed == true) {
                    connection.isConnected = true;
                    InvokeCallback();
                    return;
                }


                SmtpReplyReader reader = connection.Reader.GetNextReplyReader();
                IAsyncResult result = reader.BeginReadLine(handshakeCallback, this);
                if (!result.CompletedSynchronously)
                {
                    return;
                }

                LineInfo info = reader.EndReadLine(result);

                if (info.StatusCode != SmtpStatusCode.ServiceReady)
                {
                    throw new SmtpException(info.StatusCode, info.Line, true);
                }
                try
                {
                    if (!SendEHello())
                    {
                        return;
                    }
                }
                catch
                {
                    if (!SendHello())
                    {
                        return;
                    }
                }
            }

            static void HandshakeCallback(IAsyncResult result)   //3
            {
                if (!result.CompletedSynchronously)
                {
                    ConnectAndHandshakeAsyncResult thisPtr = (ConnectAndHandshakeAsyncResult)result.AsyncState;
                    try
                    {
                        try
                        {
                            LineInfo info = thisPtr.connection.Reader.CurrentReader.EndReadLine(result);
                            if (info.StatusCode != SmtpStatusCode.ServiceReady)
                            {
                                thisPtr.InvokeCallback(new SmtpException(info.StatusCode, info.Line, true));
                                return;
                            }
                            if (!thisPtr.SendEHello())
                            {
                                return;
                            }
                        }
                        catch (SmtpException)
                        {
                            if (!thisPtr.SendHello())
                            {
                                return;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        thisPtr.InvokeCallback(e);
                    }
                }
            }

            bool SendEHello()//4
            {
                IAsyncResult result = EHelloCommand.BeginSend(connection, connection.client.clientDomain, sendEHelloCallback, this);
                if (result.CompletedSynchronously)
                {
                    connection.extensions = EHelloCommand.EndSend(result);
                    connection.ParseExtensions(connection.extensions);
                    // If we already have a TlsStream, this is the second EHLO cmd
                    // that we sent after TLS handshake compelted. So skip TLS and
                    // continue with Authenticate.
                    if (connection.pooledStream.NetworkStream is TlsStream)
                    {
                        Authenticate();
                        return true;
                    }

                    if (connection.EnableSsl) {
#if !FEATURE_PAL
                        if (!connection.serverSupportsStartTls)
                        {
                            // Either TLS is already established or server does not support TLS
                            if (!(connection.pooledStream.NetworkStream is TlsStream))
                            {
                                throw new SmtpException(SR.GetString(SR.MailServerDoesNotSupportStartTls));
                            }
                        }

                        SendStartTls();
#else // FEATURE_PAL
                        throw new NotSupportedException("ROTORTODO");
#endif // !FEATURE_PAL
                    }
                    else {
                        Authenticate();
                    }
                    return true;
                }
                return false;
            }

            static void SendEHelloCallback(IAsyncResult result)//5
            {
                if (!result.CompletedSynchronously)
                {
                    ConnectAndHandshakeAsyncResult thisPtr = (ConnectAndHandshakeAsyncResult)result.AsyncState;
                    try
                    {
                        try
                        {
                            thisPtr.connection.extensions = EHelloCommand.EndSend(result);
                            thisPtr.connection.ParseExtensions(thisPtr.connection.extensions);

                            // If we already have a TlsStream, this is the second EHLO cmd
                            // that we sent after TLS handshake compelted. So skip TLS and
                            // continue with Authenticate.
                            if (thisPtr.connection.pooledStream.NetworkStream is TlsStream)
                            {
                                thisPtr.Authenticate();
                                return;
                            }
                        }

                        catch (SmtpException e)
                        {
                            if ((e.StatusCode != SmtpStatusCode.CommandUnrecognized)
                                && (e.StatusCode != SmtpStatusCode.CommandNotImplemented)){
                                throw e;
                            }

                            if (!thisPtr.SendHello()) {
                                return;
                            }
                        }


                        if (thisPtr.connection.EnableSsl) {
#if !FEATURE_PAL
                            if (!thisPtr.connection.serverSupportsStartTls)
                            {
                                // Either TLS is already established or server does not support TLS
                                if (!(thisPtr.connection.pooledStream.NetworkStream is TlsStream))
                                {
                                    throw new SmtpException(SR.GetString(SR.MailServerDoesNotSupportStartTls));
                                }
                            }

                            thisPtr.SendStartTls();
#else // FEATURE_PAL
                            throw new NotSupportedException("ROTORTODO");
#endif // !FEATURE_PAL
                        }
                        else {
                            thisPtr.Authenticate();
                        }
                    }
                    catch (Exception e)
                    {
                        thisPtr.InvokeCallback(e);
                    }
                }
            }

            bool SendHello()//6
            {
                IAsyncResult result = HelloCommand.BeginSend(connection, connection.client.clientDomain, sendHelloCallback, this);
                //if ehello isn't supported, assume basic auth
                if (result.CompletedSynchronously)
                {
                    connection.supportedAuth = SupportedAuth.Login;
                    HelloCommand.EndSend(result);
                    Authenticate();
                    return true;
                }
                return false;
            }

            static void SendHelloCallback(IAsyncResult result)     //7
            {
                if (!result.CompletedSynchronously)
                {
                    ConnectAndHandshakeAsyncResult thisPtr = (ConnectAndHandshakeAsyncResult)result.AsyncState;
                    try
                    {
                        HelloCommand.EndSend(result);
                        thisPtr.Authenticate();
                    }
                    catch (Exception e)
                    {
                        thisPtr.InvokeCallback(e);
                    }
                }
            }

#if !FEATURE_PAL
            bool SendStartTls()//6
            {
                IAsyncResult result = StartTlsCommand.BeginSend(connection, SendStartTlsCallback, this);
                if (result.CompletedSynchronously)
                {
                    StartTlsCommand.EndSend(result);
                    TlsStream TlsStream = new TlsStream(connection.pooledStream.ServicePoint.Host, connection.pooledStream.NetworkStream, connection.ClientCertificates, connection.pooledStream.ServicePoint, connection.client, m_OuterResult.ContextCopy);
                    connection.pooledStream.NetworkStream = TlsStream;
                    connection.responseReader = new SmtpReplyReaderFactory(connection.pooledStream.NetworkStream);
                    SendEHello();
                    return true;
                }
                return false;
            }

            static void SendStartTlsCallback(IAsyncResult result)     //7
            {
                if (!result.CompletedSynchronously)
                {
                    ConnectAndHandshakeAsyncResult thisPtr = (ConnectAndHandshakeAsyncResult)result.AsyncState;
                    try
                    {
                        StartTlsCommand.EndSend(result);
                        TlsStream TlsStream = new TlsStream(thisPtr.connection.pooledStream.ServicePoint.Host, thisPtr.connection.pooledStream.NetworkStream, thisPtr.connection.ClientCertificates, thisPtr.connection.pooledStream.ServicePoint, thisPtr.connection.client, thisPtr.m_OuterResult.ContextCopy);
                        thisPtr.connection.pooledStream.NetworkStream = TlsStream;
                        thisPtr.connection.responseReader = new SmtpReplyReaderFactory(thisPtr.connection.pooledStream.NetworkStream);
                        thisPtr.SendEHello();
                    }
                    catch (Exception e)
                    {
                        thisPtr.InvokeCallback(e);
                    }
                }
            }
#endif // !FEATURE_PAL

            void Authenticate() //8
            {
                //if no credentials were supplied, try anonymous
                //servers don't appear to anounce that they support anonymous login.
                if (connection.credentials != null) {
                    while (++currentModule < connection.authenticationModules.Length)
                    {
                        //only authenticate if the auth protocol is supported
                        ISmtpAuthenticationModule module = connection.authenticationModules[currentModule];
                        if (!connection.AuthSupported(module)) {
                            continue;
                        }

                        NetworkCredential credential = connection.credentials.GetCredential(host, port, module.AuthenticationType);
                        if (credential == null)
                            continue;
                        Authorization auth = connection.SetContextAndTryAuthenticate(module, credential, m_OuterResult);

                        if (auth != null && auth.Message != null)
                        {
                            IAsyncResult result = AuthCommand.BeginSend(connection, connection.authenticationModules[currentModule].AuthenticationType, auth.Message, authenticateCallback, this);
                            if (!result.CompletedSynchronously)
                            {
                                return;
                            }

                            LineInfo info = AuthCommand.EndSend(result);

                            if ((int)info.StatusCode == 334)
                            {
                                authResponse = info.Line;
                                if (!AuthenticateContinue())
                                {
                                    return;
                                }
                            }
                            else if ((int)info.StatusCode == 235)
                            {
                                module.CloseContext(connection);
                                connection.isConnected = true;
                                break;
                            }
                        }
                    }

                    //try anonymous if didn't authenticate
                    //if (!connection.isConnected) {
                    //    throw new SmtpException(SR.GetString(SR.SmtpAuthenticationFailed));
                    // }
                }

                connection.isConnected = true;
                InvokeCallback();
            }

            static void AuthenticateCallback(IAsyncResult result) //9
            {
                if (!result.CompletedSynchronously)
                {
                    ConnectAndHandshakeAsyncResult thisPtr = (ConnectAndHandshakeAsyncResult)result.AsyncState;
                    try
                    {
                        LineInfo info = AuthCommand.EndSend(result);

                        if ((int)info.StatusCode == 334)
                        {
                            thisPtr.authResponse = info.Line;
                            if (!thisPtr.AuthenticateContinue())
                            {
                                return;
                            }
                        }
                        else if ((int)info.StatusCode == 235)
                        {
                            thisPtr.connection.authenticationModules[thisPtr.currentModule].CloseContext(thisPtr.connection);
                            thisPtr.connection.isConnected = true;
                            thisPtr.InvokeCallback();
                            return;
                        }

                        thisPtr.Authenticate();
                    }
                    catch (Exception e)
                    {
                        thisPtr.InvokeCallback(e);
                    }
                }
            }

            bool AuthenticateContinue()        //10
            {
                for (; ; )
                {
                    // We don't need credential on the continued auth assuming they were captured on the first call.
                    // That should always work, otherwise what if a new credential has been returned?
                    Authorization auth = connection.authenticationModules[currentModule].Authenticate(authResponse, null, connection, connection.client.TargetName, connection.channelBindingToken);
                    if (auth == null)
                    {
                        throw new SmtpException(SR.GetString(SR.SmtpAuthenticationFailed));
                    }

                    IAsyncResult result = AuthCommand.BeginSend(connection, auth.Message, authenticateContinueCallback, this);
                    if (!result.CompletedSynchronously)
                    {
                        return false;
                    }

                    LineInfo info = AuthCommand.EndSend(result);
                    if ((int)info.StatusCode == 235)
                    {
                        connection.authenticationModules[currentModule].CloseContext(connection);
                        connection.isConnected = true;
                        InvokeCallback();
                        return false;
                    }
                    else if ((int)info.StatusCode != 334)
                    {
                        return true;
                    }
                    authResponse = info.Line;
                }
            }

            static void AuthenticateContinueCallback(IAsyncResult result)     //11
            {
                if (!result.CompletedSynchronously)
                {
                    ConnectAndHandshakeAsyncResult thisPtr = (ConnectAndHandshakeAsyncResult)result.AsyncState;
                    try
                    {
                        LineInfo info = AuthCommand.EndSend(result);
                        if ((int)info.StatusCode == 235)
                        {
                            thisPtr.connection.authenticationModules[thisPtr.currentModule].CloseContext(thisPtr.connection);
                            thisPtr.connection.isConnected = true;
                            thisPtr.InvokeCallback();
                            return;
                        }
                        else if ((int)info.StatusCode == 334)
                        {
                            thisPtr.authResponse = info.Line;
                            if (!thisPtr.AuthenticateContinue())
                            {
                                return;
                            }
                        }
                        thisPtr.Authenticate();
                    }
                    catch (Exception e)
                    {
                        thisPtr.InvokeCallback(e);
                    }
                }
            }

        }
    }
}
