namespace System.Net.Mail
{

    using System;
    using System.IO;
    using System.Net;
    using System.ComponentModel;
    using System.Net.Configuration;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Security;
    using System.Security.Permissions;
    using System.Security.Authentication;
    using System.Security.Cryptography.X509Certificates;
    using System.Net.NetworkInformation;
    using System.Runtime.Versioning;
    using System.Text;
    using System.Globalization;

    public delegate void SendCompletedEventHandler(object sender, AsyncCompletedEventArgs e);

    public enum SmtpDeliveryMethod {
        Network,
        SpecifiedPickupDirectory,
#if !FEATURE_PAL
        PickupDirectoryFromIis
#endif
    }

    // EAI Settings
    public enum SmtpDeliveryFormat {
        SevenBit = 0, // Legacy
        International = 1, // SMTPUTF8 - Email Address Internationalization (EAI)
    }

    public class SmtpClient : IDisposable {

        string host;
        int port;
        bool inCall;
        bool cancelled;
        bool timedOut;
        string targetName = null;
        SmtpDeliveryMethod deliveryMethod = SmtpDeliveryMethod.Network;
        SmtpDeliveryFormat deliveryFormat = SmtpDeliveryFormat.SevenBit; // Non-EAI default
        string pickupDirectoryLocation = null;
        SmtpTransport transport;
        MailMessage message; //required to prevent premature finalization
        MailWriter writer;
        MailAddressCollection recipients;
        SendOrPostCallback onSendCompletedDelegate;
        Timer timer;
        static volatile MailSettingsSectionGroupInternal mailConfiguration;
        ContextAwareResult operationCompletedResult = null;
        AsyncOperation asyncOp = null;
        static AsyncCallback _ContextSafeCompleteCallback = new AsyncCallback(ContextSafeCompleteCallback);
        static int defaultPort = 25;
        internal string clientDomain = null;
        bool disposed = false;
        // true if the host and port change between calls to send or GetServicePoint
        bool servicePointChanged = false;
        ServicePoint servicePoint = null;
        // (async only) For when only some recipients fail.  We still send the e-mail to the others.
        SmtpFailedRecipientException failedRecipientException;
        // ports above this limit are invalid
        const int maxPortValue = 65535;

        public event SendCompletedEventHandler SendCompleted;

        public SmtpClient() {
            if (Logging.On) Logging.Enter(Logging.Web, "SmtpClient", ".ctor", "");
            try {
                Initialize();
            } finally {
                if (Logging.On) Logging.Exit(Logging.Web, "SmtpClient", ".ctor", this);
            }
        }

        public SmtpClient(string host) {
            if (Logging.On) Logging.Enter(Logging.Web, "SmtpClient", ".ctor", "host=" + host);
            try {
                this.host = host;
                Initialize();
            } finally {
                if (Logging.On) Logging.Exit(Logging.Web, "SmtpClient", ".ctor", this);
            }
        }

        //?? should port throw or just default on 0?
        public SmtpClient(string host, int port) {
            if (Logging.On) Logging.Enter(Logging.Web, "SmtpClient", ".ctor", "host=" + host + ", port=" + port);
            try {
                if (port < 0) {
                    throw new ArgumentOutOfRangeException("port");
                }

                this.host = host;
                this.port = port;
                Initialize();
            } finally {
                if (Logging.On) Logging.Exit(Logging.Web, "SmtpClient", ".ctor", this);
            }
        }

        void Initialize() {
            if (port == defaultPort || port == 0) {
                new SmtpPermission(SmtpAccess.Connect).Demand();
            }
            else {
                new SmtpPermission(SmtpAccess.ConnectToUnrestrictedPort).Demand();
            }

            transport = new SmtpTransport(this);
            if (Logging.On) Logging.Associate(Logging.Web, this, transport);
            onSendCompletedDelegate = new SendOrPostCallback(SendCompletedWaitCallback);

            if (MailConfiguration.Smtp != null) 
            {
                if (MailConfiguration.Smtp.Network != null) 
                {
                    if (host == null || host.Length == 0) {
                        host = MailConfiguration.Smtp.Network.Host;
                    }
                    if (port == 0) {
                        port = MailConfiguration.Smtp.Network.Port;
                    }

                    transport.Credentials = MailConfiguration.Smtp.Network.Credential;
                    transport.EnableSsl = MailConfiguration.Smtp.Network.EnableSsl;

                    if (MailConfiguration.Smtp.Network.TargetName != null)
                        targetName = MailConfiguration.Smtp.Network.TargetName;

                    // If the config file contains a domain to be used for the 
                    // domain element in the client's EHLO or HELO message, 
                    // use it.
                    //
                    // We do not validate whether the domain specified is valid.
                    // It is up to the administrators or user to use the right
                    // value for their scenario. 
                    //
                    // Note: per section 4.1.4 of RFC2821, the domain element of 
                    // the HELO/EHLO should be used for logging purposes. An 
                    // SMTP server should not decide to route an email based on
                    // this value.
                    clientDomain = MailConfiguration.Smtp.Network.ClientDomain;
                }

                deliveryFormat = MailConfiguration.Smtp.DeliveryFormat;

                deliveryMethod = MailConfiguration.Smtp.DeliveryMethod;
                if (MailConfiguration.Smtp.SpecifiedPickupDirectory != null)
                    pickupDirectoryLocation = MailConfiguration.Smtp.SpecifiedPickupDirectory.PickupDirectoryLocation;
            }

            if (host != null && host.Length != 0) {
                host = host.Trim();
            }

            if (port == 0) {
                port = defaultPort;
            }

            if (this.targetName == null)
                targetName = "SMTPSVC/" + host;

            if (clientDomain == null) {
                // We use the local host name as the default client domain
                // for the client's EHLO or HELO message. This limits the 
                // information about the host that we share. Additionally, the 
                // FQDN is not available to us or useful to the server (internal
                // machine connecting to public server).

                // SMTP RFC's require ASCII only host names in the HELO/EHLO message.
                string clientDomainRaw = IPGlobalProperties.InternalGetIPGlobalProperties().HostName;
                IdnMapping mapping = new IdnMapping();
                try
                {
                    clientDomainRaw = mapping.GetAscii(clientDomainRaw);
                }
                catch (ArgumentException) { }

                // For some inputs GetAscii may fail (bad Unicode, etc).  If that happens
                // we must strip out any non-ASCII characters.
                // If we end up with no characters left, we use the string "LocalHost".  This 
                // matches Outlook behavior.
                StringBuilder sb = new StringBuilder();
                char ch;
                for (int i = 0; i < clientDomainRaw.Length; i++) {
                    ch = clientDomainRaw[i];
                    if ((ushort)ch <= 0x7F)
                        sb.Append(ch);
                }
                if (sb.Length > 0)
                    clientDomain = sb.ToString();
                else
                    clientDomain = "LocalHost";
            }
        }


        public string Host {
            get {
                return host;
            }
            set {

                if (InCall) {
                    throw new InvalidOperationException(SR.GetString(SR.SmtpInvalidOperationDuringSend));
                }

                if (value == null) 
                {
                    throw new ArgumentNullException("value");
                }

                if (value == String.Empty) 
                {
                    throw new ArgumentException(SR.GetString(SR.net_emptystringset), "value");
                }

                value = value.Trim();

                if (value != host)
                {
                    host = value;
                    servicePointChanged = true;
                }
            }
        }


        public int Port {
            get {
                return port;
            }
            set {
                if (InCall) {
                    throw new InvalidOperationException(SR.GetString(SR.SmtpInvalidOperationDuringSend));
                }

                if (value <= 0) {
                    throw new ArgumentOutOfRangeException("value");
                }

                if (value != defaultPort) {
                    new SmtpPermission(SmtpAccess.ConnectToUnrestrictedPort).Demand();
                }

                if (value != port) {
                    port = value;
                    servicePointChanged = true;
                }
            }
        }


        public bool UseDefaultCredentials {
            get {
                return (transport.Credentials is SystemNetworkCredential) ? true : false;
            }
            set {
                if (InCall) {
                    throw new InvalidOperationException(SR.GetString(SR.SmtpInvalidOperationDuringSend));
                }

                transport.Credentials = value ? CredentialCache.DefaultNetworkCredentials : null;
            }
        }


        public ICredentialsByHost Credentials {
            get {
                return transport.Credentials;
            }
            set {
                if (InCall) {
                    throw new InvalidOperationException(SR.GetString(SR.SmtpInvalidOperationDuringSend));
                }

                transport.Credentials = value;
            }
        }



        public int Timeout {
            get {
                return transport.Timeout;
            }
            set {
                if (InCall) {
                    throw new InvalidOperationException(SR.GetString(SR.SmtpInvalidOperationDuringSend));
                }

                if (value < 0) 
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                transport.Timeout = value;
            }
        }


        public ServicePoint ServicePoint {
            get {
                CheckHostAndPort();
                if (servicePoint == null || servicePointChanged) {
                    servicePoint = ServicePointManager.FindServicePoint(host, port);
                    // servicePoint is now correct for current host and port
                    servicePointChanged = false;
                }
                return servicePoint;
            }
        }

        public SmtpDeliveryMethod DeliveryMethod {
            get {
                return deliveryMethod;
            }
            set {
                deliveryMethod = value;
            }
        }

        // Should we use EAI formats?
        public SmtpDeliveryFormat DeliveryFormat {
            get {
                return deliveryFormat;
            }
            set {
                deliveryFormat = value;
            }
        }

        public string PickupDirectoryLocation {
            [ResourceExposure(ResourceScope.Machine)]
            [ResourceConsumption(ResourceScope.Machine)]
            get {
                return pickupDirectoryLocation;
            }
            [ResourceExposure(ResourceScope.Machine)]
            [ResourceConsumption(ResourceScope.Machine)]
            set {
                pickupDirectoryLocation = value;
            }
        }

        /// <summary>
        ///    <para>Set to true if we need SSL</para>
        /// </summary>
        public bool EnableSsl {
            get {
                return transport.EnableSsl;
            }
            set {
                transport.EnableSsl = value;
            }
        }

        /// <summary>
        /// Certificates used by the client for establishing an SSL connection with the server. 
        /// </summary>
        public X509CertificateCollection ClientCertificates {
            get {
                return transport.ClientCertificates;
            }
        }

        public string TargetName {
            set { this.targetName = value; }
            get { return this.targetName; }
        }

        private bool ServerSupportsEai {
            get { 
                return transport.ServerSupportsEai; 
            }
        }

        private bool IsUnicodeSupported() {
            if (DeliveryMethod == SmtpDeliveryMethod.Network) {
                return (ServerSupportsEai && (DeliveryFormat == SmtpDeliveryFormat.International));
            }
            else { // Pickup directories
                return (DeliveryFormat == SmtpDeliveryFormat.International);
            }
        }

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        internal MailWriter GetFileMailWriter(string pickupDirectory) 
        {
            if (Logging.On) Logging.PrintInfo(Logging.Web, "SmtpClient.Send() pickupDirectory=" + pickupDirectory);
            if (!Path.IsPathRooted(pickupDirectory))
                throw new SmtpException(SR.GetString(SR.SmtpNeedAbsolutePickupDirectory));
            string filename;
            string pathAndFilename;
            while (true) {
                filename = Guid.NewGuid().ToString() + ".eml";
                pathAndFilename = Path.Combine(pickupDirectory, filename);
                if (!File.Exists(pathAndFilename))
                    break;
            }

            FileStream fileStream = new FileStream(pathAndFilename, FileMode.CreateNew);
            return new MailWriter(fileStream);
        }

        protected void OnSendCompleted(AsyncCompletedEventArgs e) 
        {
            if (SendCompleted != null) {
                SendCompleted(this, e);
            }
        }

        void SendCompletedWaitCallback(object operationState) {
            OnSendCompleted((AsyncCompletedEventArgs)operationState);
        }


        public void Send(string from, string recipients, string subject, string body) {
            if (disposed) {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
            //validation happends in MailMessage constructor
            MailMessage mailMessage = new MailMessage(from, recipients, subject, body);
            Send(mailMessage);
        }


        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        public void Send(MailMessage message) {
            if (Logging.On) Logging.Enter(Logging.Web, this, "Send", message);
            if (disposed) {
                throw new ObjectDisposedException(this.GetType().FullName);
            }           
            try {
                if (Logging.On) Logging.PrintInfo(Logging.Web, this, "Send", "DeliveryMethod=" + DeliveryMethod.ToString());
                if (Logging.On) Logging.Associate(Logging.Web, this, message);
                SmtpFailedRecipientException recipientException = null;

                if (InCall) {
                    throw new InvalidOperationException(SR.GetString(SR.net_inasync));
                }

                if (message == null) {
                    throw new ArgumentNullException("message");
                }

                if (DeliveryMethod == SmtpDeliveryMethod.Network)
                    CheckHostAndPort();

                MailAddressCollection recipients = new MailAddressCollection();

                if (message.From == null) {
                    throw new InvalidOperationException(SR.GetString(SR.SmtpFromRequired));
                }

                if (message.To != null) {
                    foreach (MailAddress address in message.To) {
                        recipients.Add(address);
                    }
                }
                if (message.Bcc != null) {
                    foreach (MailAddress address in message.Bcc) {
                        recipients.Add(address);
                    }
                }
                if (message.CC != null) {
                    foreach (MailAddress address in message.CC) {
                        recipients.Add(address);
                    }
                }

                if (recipients.Count == 0) {
                    throw new InvalidOperationException(SR.GetString(SR.SmtpRecipientRequired));
                }

                transport.IdentityRequired = false;  // everything completes on the same thread.

                try {
                    InCall = true;
                    timedOut = false;
                    timer = new Timer(new TimerCallback(this.TimeOutCallback), null, Timeout, Timeout);
                    bool allowUnicode = false;
                    string pickupDirectory = PickupDirectoryLocation;

                    MailWriter writer;
                    switch (DeliveryMethod) {
#if !FEATURE_PAL
                        case SmtpDeliveryMethod.PickupDirectoryFromIis:
                            pickupDirectory = IisPickupDirectory.GetPickupDirectory();
                            goto case SmtpDeliveryMethod.SpecifiedPickupDirectory;
#endif // !FEATURE_PAL
                        case SmtpDeliveryMethod.SpecifiedPickupDirectory:
                            if (EnableSsl)
                                throw new SmtpException(SR.GetString(SR.SmtpPickupDirectoryDoesnotSupportSsl));
                            allowUnicode = IsUnicodeSupported(); // Determend by the DeliveryFormat paramiter
                            ValidateUnicodeRequirement(message, recipients, allowUnicode);
                            writer = GetFileMailWriter(pickupDirectory);
                            break;

                        case SmtpDeliveryMethod.Network:
                        default:
                            GetConnection();
                            // Detected durring GetConnection(), restrictable using the DeliveryFormat paramiter
                            allowUnicode = IsUnicodeSupported();
                            ValidateUnicodeRequirement(message, recipients, allowUnicode);
                            writer = transport.SendMail(message.Sender ?? message.From, recipients, 
                                message.BuildDeliveryStatusNotificationString(), allowUnicode, out recipientException);
                            break;
                    }
                    this.message = message;
                    message.Send(writer, DeliveryMethod != SmtpDeliveryMethod.Network, allowUnicode);
                    writer.Close();
                    transport.ReleaseConnection();

                    //throw if we couldn't send to any of the recipients
                    if (DeliveryMethod == SmtpDeliveryMethod.Network && recipientException != null) {
                        throw recipientException;
                    }
                }
                catch (Exception e) {

                    if (Logging.On) Logging.Exception(Logging.Web, this, "Send", e);


                    if (e is SmtpFailedRecipientException && !((SmtpFailedRecipientException)e).fatal) {
                        throw;
                    }


                    Abort();
                    if (timedOut) {
                        throw new SmtpException(SR.GetString(SR.net_timeout));
                    }

                    if (e is SecurityException ||
                        e is AuthenticationException ||
                        e is SmtpException) 
                    {
                        throw;
                    }

                    throw new SmtpException(SR.GetString(SR.SmtpSendMailFailure), e);
                } 
                finally {
                    InCall = false;
                    if (timer != null) {
                        timer.Dispose();
                    }
                }
            } finally {
                if (Logging.On) Logging.Exit(Logging.Web, this, "Send", null);
            }
        }

        [HostProtection(ExternalThreading = true)]
        public void SendAsync(string from, string recipients, string subject, string body, object userToken) {
            if (disposed) {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
            SendAsync(new MailMessage(from, recipients, subject, body), userToken);
        }


        [HostProtection(ExternalThreading = true)]
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        public void SendAsync(MailMessage message, object userToken) {
            if (disposed) {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
            if (Logging.On) Logging.Enter(Logging.Web, this, "SendAsync", "DeliveryMethod=" + DeliveryMethod.ToString());
            GlobalLog.Enter("SmtpClient#" + ValidationHelper.HashString(this) + "::SendAsync Transport#" + ValidationHelper.HashString(transport));
            try {
                if (InCall) {
                    throw new InvalidOperationException(SR.GetString(SR.net_inasync));
                }

                if (message == null) {
                    throw new ArgumentNullException("message");
                }

                if (DeliveryMethod == SmtpDeliveryMethod.Network)
                    CheckHostAndPort();

                recipients = new MailAddressCollection();

                if (message.From == null) {
                    throw new InvalidOperationException(SR.GetString(SR.SmtpFromRequired));
                }

                if (message.To != null) {
                    foreach (MailAddress address in message.To) {
                        recipients.Add(address);
                    }
                }
                if (message.Bcc != null) {
                    foreach (MailAddress address in message.Bcc) {
                        recipients.Add(address);
                    }
                }
                if (message.CC != null) {
                    foreach (MailAddress address in message.CC) {
                        recipients.Add(address);
                    }
                }

                if (recipients.Count == 0) {
                    throw new InvalidOperationException(SR.GetString(SR.SmtpRecipientRequired));
                }

                try {
                    InCall = true;
                    cancelled = false;
                    this.message = message;
                    string pickupDirectory = PickupDirectoryLocation;

#if !FEATURE_PAL
                    CredentialCache cache;
                    // Skip token capturing if no credentials are used or they don't include a default one.
                    // Also do capture the token if ICredential is not of CredentialCache type so we don't know what the exact credential response will be.
                    transport.IdentityRequired = Credentials != null && (Credentials is SystemNetworkCredential || (cache = Credentials as CredentialCache) == null || cache.IsDefaultInCache);
#endif // !FEATURE_PAL

                    asyncOp = AsyncOperationManager.CreateOperation(userToken);
                    switch (DeliveryMethod) {
#if !FEATURE_PAL
                        case SmtpDeliveryMethod.PickupDirectoryFromIis:
                            pickupDirectory = IisPickupDirectory.GetPickupDirectory();
                            goto case SmtpDeliveryMethod.SpecifiedPickupDirectory;
#endif // !FEATURE_PAL
                        case SmtpDeliveryMethod.SpecifiedPickupDirectory: 
                            {
                                if (EnableSsl)
                                    throw new SmtpException(SR.GetString(SR.SmtpPickupDirectoryDoesnotSupportSsl));
                                writer = GetFileMailWriter(pickupDirectory);
                                bool allowUnicode = IsUnicodeSupported();
                                ValidateUnicodeRequirement(message, recipients, allowUnicode);
                                message.Send(writer, true, allowUnicode);

                                if (writer != null)
                                    writer.Close();

                                transport.ReleaseConnection();
                                AsyncCompletedEventArgs eventArgs = new AsyncCompletedEventArgs(null, false, asyncOp.UserSuppliedState);
                                InCall = false;
                                asyncOp.PostOperationCompleted(onSendCompletedDelegate, eventArgs);
                                break;
                            }

                        case SmtpDeliveryMethod.Network:
                        default:
                            operationCompletedResult = new ContextAwareResult(transport.IdentityRequired, true, null, this, _ContextSafeCompleteCallback);
                            lock (operationCompletedResult.StartPostingAsyncOp()) 
                            {
                                GlobalLog.Print("SmtpClient#" + ValidationHelper.HashString(this) + "::SendAsync calling BeginConnect.  Transport#" + ValidationHelper.HashString(transport));
                                transport.BeginGetConnection(ServicePoint, operationCompletedResult, ConnectCallback, operationCompletedResult);
                                operationCompletedResult.FinishPostingAsyncOp();
                            }
                            break;
                    }

                }
                catch (Exception e) {
                    InCall = false;

                    if (Logging.On) Logging.Exception(Logging.Web, this, "Send", e);

                    if (e is SmtpFailedRecipientException && !((SmtpFailedRecipientException)e).fatal) {
                        throw;
                    }

                    Abort();
                    if (timedOut) {
                        throw new SmtpException(SR.GetString(SR.net_timeout));
                    }

                    if (e is SecurityException ||
                        e is AuthenticationException ||
                        e is SmtpException) 
                    {
                        throw;
                    }

                    throw new SmtpException(SR.GetString(SR.SmtpSendMailFailure), e);
                }
            } finally {
                if (Logging.On) Logging.Exit(Logging.Web, this, "SendAsync", null);
                GlobalLog.Leave("SmtpClient#" + ValidationHelper.HashString(this) + "::SendAsync");
            }
        }


        public void SendAsyncCancel() {
            if (disposed) {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
            if (Logging.On) Logging.Enter(Logging.Web, this, "SendAsyncCancel", null);
            GlobalLog.Enter("SmtpClient#" + ValidationHelper.HashString(this) + "::SendAsyncCancel");
            try {
                if (!InCall || cancelled) {
                    return;
                }

                cancelled = true;
                Abort();
            } finally {
                if (Logging.On) Logging.Exit(Logging.Web, this, "SendAsyncCancel", null);
                GlobalLog.Leave("SmtpClient#" + ValidationHelper.HashString(this) + "::SendAsyncCancel");
            }
        }


        //************* Task-based async public methods *************************
        [HostProtection(ExternalThreading = true)]
        public Task SendMailAsync(string from, string recipients, string subject, string body)
        {
            var message = new MailMessage(from, recipients, subject, body);
            return SendMailAsync(message);
        }

        [HostProtection(ExternalThreading = true)]
        public Task SendMailAsync(MailMessage message)
        {
            // Create a TaskCompletionSource to represent the operation
            var tcs = new TaskCompletionSource<object>();

            // Register a handler that will transfer completion results to the TCS Task
            SendCompletedEventHandler handler = null;
            handler = (sender, e) => HandleCompletion(tcs, e, handler);
            this.SendCompleted += handler;

            // Start the async operation.
            try { this.SendAsync(message, tcs); }
            catch
            {
                this.SendCompleted -= handler;
                throw;
            }

            // Return the task to represent the asynchronous operation
            return tcs.Task;
        }

        private void HandleCompletion(TaskCompletionSource<object> tcs, AsyncCompletedEventArgs e, SendCompletedEventHandler handler)
        {
            if (e.UserState == tcs)
            {
                try { this.SendCompleted -= handler; }
                finally
                {
                    if (e.Error != null) tcs.TrySetException(e.Error);
                    else if (e.Cancelled) tcs.TrySetCanceled();
                    else tcs.TrySetResult(null);
                }
            }
        }


        //*********************************
        // private methods
        //********************************
        internal bool InCall {
            get {
                return inCall;
            }
            set {
                inCall = value;
            }
        }

        internal static MailSettingsSectionGroupInternal MailConfiguration {
            get {
                if (mailConfiguration == null) {
                    mailConfiguration = MailSettingsSectionGroupInternal.GetSection();
                }
                return mailConfiguration;
            }
        }


        void CheckHostAndPort() {

            if (host == null || host.Length == 0) {
                throw new InvalidOperationException(SR.GetString(SR.UnspecifiedHost));
            }
            if (port <= 0 || port > maxPortValue) {
                throw new InvalidOperationException(SR.GetString(SR.InvalidPort));
            }
        }


        void TimeOutCallback(object state) {
            if (!timedOut) {
                timedOut = true;
                Abort();
            }
        }


        void Complete(Exception exception, IAsyncResult result) {
            ContextAwareResult operationCompletedResult = (ContextAwareResult)result.AsyncState;
            GlobalLog.Enter("SmtpClient#" + ValidationHelper.HashString(this) + "::Complete");
            try {
                if (cancelled) {
                    //any exceptions were probably caused by cancellation, clear it.
                    exception = null;
                    Abort();
                }
                // An individual failed recipient exception is benign, only abort here if ALL the recipients failed.
                else if (exception != null && (!(exception is SmtpFailedRecipientException) || ((SmtpFailedRecipientException)exception).fatal)) 
                {
                    GlobalLog.Print("SmtpClient#" + ValidationHelper.HashString(this) + "::Complete Exception: " + exception.ToString());
                    Abort();

                    if (!(exception is SmtpException)) {
                        exception = new SmtpException(SR.GetString(SR.SmtpSendMailFailure), exception);
                    }
                }
                else {
                    if (writer != null) {
                        try {
                            writer.Close();
                        }
                        // Close may result in a DataStopCommand and the server may return error codes at this time.
                        catch (SmtpException se) {
                            exception = se;
                        }
                    }
                    transport.ReleaseConnection();
                }
            }
            finally {
                operationCompletedResult.InvokeCallback(exception);
            }
            GlobalLog.Leave("SmtpClient#" + ValidationHelper.HashString(this) + "::Complete");
        }

        static void ContextSafeCompleteCallback(IAsyncResult ar) 
        {
            ContextAwareResult result = (ContextAwareResult)ar;
            SmtpClient client = (SmtpClient)ar.AsyncState;
            Exception exception = result.Result as Exception;
            AsyncOperation asyncOp = client.asyncOp;
            AsyncCompletedEventArgs eventArgs = new AsyncCompletedEventArgs(exception, client.cancelled, asyncOp.UserSuppliedState);
            client.InCall = false;
            client.failedRecipientException = null; // Reset before the next send.
            asyncOp.PostOperationCompleted(client.onSendCompletedDelegate, eventArgs);
        }

        void SendMessageCallback(IAsyncResult result) {
            GlobalLog.Enter("SmtpClient#" + ValidationHelper.HashString(this) + "::SendMessageCallback");
            try {
                message.EndSend(result);
                // If some recipients failed but not others, throw AFTER sending the message.
                Complete(failedRecipientException, result);
            }
            catch (Exception e) {
                Complete(e, result);
            }
            GlobalLog.Leave("SmtpClient#" + ValidationHelper.HashString(this) + "::SendMessageCallback");
        }


        void SendMailCallback(IAsyncResult result) {
            GlobalLog.Enter("SmtpClient#" + ValidationHelper.HashString(this) + "::SendMailCallback");
            try {
                writer = transport.EndSendMail(result);
                // If some recipients failed but not others, send the e-mail anyways, but then return the
                // "Non-fatal" exception reporting the failures.  The [....] code path does it this way.
                // Fatal exceptions would have thrown above at transport.EndSendMail(...)
                SendMailAsyncResult sendResult = (SendMailAsyncResult)result;
                // Save these and throw them later in SendMessageCallback, after the message has sent.
                failedRecipientException = sendResult.GetFailedRecipientException();
            }
            catch (Exception e) 
            {
                Complete(e, result);
                GlobalLog.Leave("SmtpClient#" + ValidationHelper.HashString(this) + "::SendMailCallback");
                return;
            }
            try {
                if (cancelled) {
                    Complete(null, result);
                }
                else {
                    message.BeginSend(writer, DeliveryMethod != SmtpDeliveryMethod.Network,
                        ServerSupportsEai, new AsyncCallback(SendMessageCallback), result.AsyncState);
                }
            }
            catch (Exception e) {
                Complete(e, result);
            }
            GlobalLog.Leave("SmtpClient#" + ValidationHelper.HashString(this) + "::SendMailCallback");
        }


        void ConnectCallback(IAsyncResult result) {
            GlobalLog.Enter("SmtpClient#" + ValidationHelper.HashString(this) + "::ConnectCallback");
            try {
                transport.EndGetConnection(result);
                if (cancelled) {
                    Complete(null, result);
                }
                else {
                    // Detected durring Begin/EndGetConnection, restrictable using DeliveryFormat
                    bool allowUnicode = IsUnicodeSupported(); 
                    ValidateUnicodeRequirement(message, recipients, allowUnicode);
                    transport.BeginSendMail(message.Sender ?? message.From, recipients,
                        message.BuildDeliveryStatusNotificationString(), allowUnicode,
                        new AsyncCallback(SendMailCallback), result.AsyncState);
                }
            }
            catch (Exception e) {
                Complete(e, result);
            }
            GlobalLog.Leave("SmtpClient#" + ValidationHelper.HashString(this) + "::ConnectCallback");
        }

        // After we've estabilished a connection and initilized ServerSupportsEai,
        // check all the addresses for one that contains unicode in the username/localpart.
        // The localpart is the only thing we cannot succesfully downgrade.
        private void ValidateUnicodeRequirement(MailMessage message, 
            MailAddressCollection recipients, bool allowUnicode)
        {
            // Check all recipients, to, from, sender, bcc, cc, etc...
            // GetSmtpAddress will throw if !allowUnicode and the username contains non-ascii
            foreach (MailAddress address in recipients)
            {
                address.GetSmtpAddress(allowUnicode);
            }
            if (message.Sender != null)
            {
                message.Sender.GetSmtpAddress(allowUnicode);
            }
            message.From.GetSmtpAddress(allowUnicode);
        }


        void GetConnection() {
            if (!transport.IsConnected) {
                transport.GetConnection(ServicePoint);
            }
        }


        void Abort() {
            try {
                transport.Abort();
            }
            catch {
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (disposing && !disposed ) {
                if (InCall && !cancelled) {
                    cancelled = true;
                    Abort();
                }

                if ((transport != null) && (servicePoint != null)) {
                    transport.CloseIdleConnections(servicePoint);
                }
                
                if (timer != null) {
                    timer.Dispose();
                }
              
                disposed = true;
            }
        }
    }
}
