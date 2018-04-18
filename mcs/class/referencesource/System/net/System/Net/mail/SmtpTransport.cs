//-----------------------------------------------------------------------------
// <copyright file="SmtpTransport.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------------

namespace System.Net.Mail
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Net;
    using System.Security.Cryptography.X509Certificates;
    using System.Net.Mime;
    using System.Diagnostics;

    internal enum SupportedAuth{
        None = 0, Login = 1,
#if !FEATURE_PAL
        NTLM = 2, GSSAPI = 4, WDigest = 8
#endif
    };

    internal class SmtpPooledStream:PooledStream{
        internal bool previouslyUsed;
        internal bool dsnEnabled;  //delivery  status notification
        internal bool serverSupportsEai;
        internal ICredentialsByHost creds;
        internal SmtpPooledStream(ConnectionPool connectionPool, TimeSpan lifetime, bool checkLifetime) : base (connectionPool,lifetime,checkLifetime) {
        }

        // maximum line length in SMTP response is 76 so this is a bit more conservative
        const int safeBufferLength = 80;
        
        // Cleans up an open connection to an SMTP server by sending the QUIT 
        // response, reading the server's response, and disposing the base stream
        protected override void Dispose(bool disposing)
        {
            if (Logging.On) {
                Logging.Enter(Logging.Web, "SmtpPooledStream::Dispose #" + ValidationHelper.HashString(this));
            }
            if (disposing) {
                if (this.NetworkStream.Connected) {
                    this.Write(SmtpCommands.Quit, 0, SmtpCommands.Quit.Length);
                    this.Flush();

                    // read the response - this is a formality since the connection is shut down
                    // immediately by the server so this data can safely be ignored but buffer
                    // must be read to ensure a FIN is sent instead of a RST
                    byte[] buffer = new byte[safeBufferLength];
                    int result = this.Read(buffer, 0, safeBufferLength);
                }
            }
            base.Dispose(disposing);
            if (Logging.On) {
                Logging.Exit(Logging.Web, "SmtpPooledStream::Dispose #" + ValidationHelper.HashString(this));
            }
        }        
    }

    internal class SmtpTransport
    {
        internal const int DefaultPort = 25;
        
        ISmtpAuthenticationModule[] authenticationModules;
        SmtpConnection connection;
        SmtpClient client;
        ICredentialsByHost credentials;
        int timeout = 100000; // seconds
        ArrayList failedRecipientExceptions = new ArrayList();
        bool m_IdentityRequired;

        bool enableSsl = false;
        X509CertificateCollection clientCertificates = null;

        ServicePoint lastUsedServicePoint;

        internal SmtpTransport(SmtpClient client) : this(client, SmtpAuthenticationManager.GetModules()) {
        }


        internal SmtpTransport(SmtpClient client, ISmtpAuthenticationModule[] authenticationModules)
        {
            this.client = client;

            if (authenticationModules == null)
            {
                throw new ArgumentNullException("authenticationModules");
            }

            this.authenticationModules = authenticationModules;
        }

        internal ICredentialsByHost Credentials
        {
            get
            {
                return credentials;
            }
            set
            {
                credentials = value;
            }
        }

        internal bool IdentityRequired
        {
            get
            {
                return m_IdentityRequired;
            }

            set
            {
                m_IdentityRequired = value;
            }
        }

        internal bool IsConnected
        {
            get
            {
                return connection != null && connection.IsConnected;
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
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                timeout = value;
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

        internal X509CertificateCollection ClientCertificates
        {
            get {
                if (clientCertificates == null) {
                    clientCertificates = new X509CertificateCollection();
                }
                return clientCertificates;
            }
        }

        internal bool ServerSupportsEai
        {
            get { return connection != null && connection.ServerSupportsEai; }
        }

        // check to see if we're using a different servicepoint than the last
        // servicepoint used to get a connectionpool
        //
        // preconditions: servicePoint must have valid host and port (checked in SmtpClient)
        //
        // postconditions: if servicePoint is different than the last servicePoint used by this object,
        // the connection pool for the previous servicepoint will be cleaned up and servicePoint will be
        // cached to identify if it has changed in future uses of this SmtpTransport object
        private void UpdateServicePoint(ServicePoint servicePoint)
        {
            if (lastUsedServicePoint == null) {
                lastUsedServicePoint = servicePoint;
            }
            else if (lastUsedServicePoint.Host != servicePoint.Host
               || lastUsedServicePoint.Port != servicePoint.Port) {
                ConnectionPoolManager.CleanupConnectionPool(servicePoint, "");
                lastUsedServicePoint = servicePoint;
            }
        }

        internal void GetConnection(ServicePoint servicePoint)
        {           
            try {
                Debug.Assert(servicePoint != null, "no ServicePoint provided by SmtpClient");
                // check to see if we have a different connection than last time
                UpdateServicePoint(servicePoint);
                connection = new SmtpConnection(this, client, credentials, authenticationModules);
                connection.Timeout = timeout;
                if(Logging.On)Logging.Associate(Logging.Web, this, connection);
                
                if (EnableSsl)
                {
                    connection.EnableSsl = true;
                    connection.ClientCertificates = ClientCertificates;
                }
                connection.GetConnection(servicePoint);
            }
            finally {
                
            }
        }


        internal IAsyncResult BeginGetConnection(ServicePoint servicePoint, ContextAwareResult outerResult, AsyncCallback callback, object state)
        {
            GlobalLog.Enter("SmtpTransport#" + ValidationHelper.HashString(this) + "::BeginConnect");
            IAsyncResult result = null;
            try{
                UpdateServicePoint(servicePoint);
                connection = new SmtpConnection(this, client, credentials, authenticationModules);
                connection.Timeout = timeout;
                if(Logging.On)Logging.Associate(Logging.Web, this, connection);
                if (EnableSsl)
                {
                    connection.EnableSsl = true;
                    connection.ClientCertificates = ClientCertificates;
                }

                result = connection.BeginGetConnection(servicePoint, outerResult, callback, state);
            }
            catch(Exception innerException){
                throw new SmtpException(SR.GetString(SR.MailHostNotFound), innerException);
            }
            GlobalLog.Leave("SmtpTransport#" + ValidationHelper.HashString(this) + "::BeginConnect Sync Completion");
            return result;
        }


        internal void EndGetConnection(IAsyncResult result)
        {
            GlobalLog.Enter("SmtpTransport#" + ValidationHelper.HashString(this) + "::EndGetConnection");
            try {
                connection.EndGetConnection(result);
            }
            finally {
                
                GlobalLog.Leave("SmtpTransport#" + ValidationHelper.HashString(this) + "::EndConnect");
            }
        }


        internal IAsyncResult BeginSendMail(MailAddress sender, MailAddressCollection recipients, 
            string deliveryNotify, bool allowUnicode, AsyncCallback callback, object state)
        {
            if (sender == null)
            {
                throw new ArgumentNullException("sender");
            }

            if (recipients == null)
            {
                throw new ArgumentNullException("recipients");
            }

            GlobalLog.Assert(recipients.Count > 0, "SmtpTransport::BeginSendMail()|recepients.Count <= 0");
            
            SendMailAsyncResult result = new SendMailAsyncResult(connection, sender, recipients,
                allowUnicode, connection.DSNEnabled ? deliveryNotify : null, 
                callback, state);
            result.Send();
            return result;
        }

        
        internal void ReleaseConnection() {
            if(connection != null){
                connection.ReleaseConnection();
            }
        }

        internal void Abort() {
            if(connection != null){
                connection.Abort();
            }
        }


        internal MailWriter EndSendMail(IAsyncResult result)
        {
            try {
                return SendMailAsyncResult.End(result);
            }
            finally {
                
            }
        }

        internal MailWriter SendMail(MailAddress sender, MailAddressCollection recipients, string deliveryNotify, 
            bool allowUnicode, out SmtpFailedRecipientException exception)
        {
            if (sender == null)
            {
                throw new ArgumentNullException("sender");
            }

            if (recipients == null)
            {
                throw new ArgumentNullException("recipients");
            }

            GlobalLog.Assert(recipients.Count > 0, "SmtpTransport::SendMail()|recepients.Count <= 0");

            MailCommand.Send(connection, SmtpCommands.Mail, sender, allowUnicode);
            failedRecipientExceptions.Clear();

            exception = null;
            string response;
            foreach (MailAddress address in recipients) {
                string smtpAddress = address.GetSmtpAddress(allowUnicode);
                string to = smtpAddress + (connection.DSNEnabled ? deliveryNotify : String.Empty);
                if (!RecipientCommand.Send(connection, to, out response)) {
                    failedRecipientExceptions.Add(
                        new SmtpFailedRecipientException(connection.Reader.StatusCode, smtpAddress, response));
                }
            }

            if (failedRecipientExceptions.Count > 0)
            {
                if (failedRecipientExceptions.Count == 1)
                {
                    exception = (SmtpFailedRecipientException) failedRecipientExceptions[0];
                }
                else
                {
                    exception = new SmtpFailedRecipientsException(failedRecipientExceptions, failedRecipientExceptions.Count == recipients.Count);
                }

                if (failedRecipientExceptions.Count == recipients.Count){
                    exception.fatal = true;
                    throw exception;
                }
            }

            DataCommand.Send(connection);
            return new MailWriter(connection.GetClosableStream());
        }
        
        internal void CloseIdleConnections(ServicePoint servicePoint)
        {
            ConnectionPoolManager.CleanupConnectionPool(servicePoint, "");
        }
    }


    class SendMailAsyncResult : LazyAsyncResult
    {
        SmtpConnection connection;
        MailAddress from;
        string deliveryNotify;
        static AsyncCallback sendMailFromCompleted = new AsyncCallback(SendMailFromCompleted);
        static AsyncCallback sendToCollectionCompleted = new AsyncCallback(SendToCollectionCompleted);
        static AsyncCallback sendDataCompleted = new AsyncCallback(SendDataCompleted);
        ArrayList failedRecipientExceptions = new ArrayList();
        Stream stream;
        MailAddressCollection toCollection;
        int toIndex;
        private bool allowUnicode;


        internal SendMailAsyncResult(SmtpConnection connection, MailAddress from, MailAddressCollection toCollection, 
            bool allowUnicode, string deliveryNotify, AsyncCallback callback, object state) 
            : base(null, state, callback)
        {
            this.toCollection = toCollection;
            this.connection = connection;
            this.from = from;
            this.deliveryNotify = deliveryNotify;
            this.allowUnicode = allowUnicode;
        }

        internal void Send(){
            SendMailFrom();
        }

        internal static MailWriter End(IAsyncResult result)
        {
            SendMailAsyncResult thisPtr = (SendMailAsyncResult)result;
            object sendMailResult = thisPtr.InternalWaitForCompletion();
            
            // Note the difference between the singular and plural FailedRecipient exceptions.
            // Only fail immediately if we couldn't send to any recipients.
            if ((sendMailResult is Exception)
                && (!(sendMailResult is SmtpFailedRecipientException) 
                    || ((SmtpFailedRecipientException)sendMailResult).fatal))
            {
                throw (Exception)sendMailResult;
            }            
            
            return new MailWriter(thisPtr.stream);
        }
        void SendMailFrom()
        {
            IAsyncResult result = MailCommand.BeginSend(connection, SmtpCommands.Mail, from, allowUnicode, 
                sendMailFromCompleted, this);
            if (!result.CompletedSynchronously)
            {
                return;
            }

            MailCommand.EndSend(result);
            SendToCollection();
        }

        static void SendMailFromCompleted(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                SendMailAsyncResult thisPtr = (SendMailAsyncResult)result.AsyncState;
                try
                {
                    MailCommand.EndSend(result);
                    thisPtr.SendToCollection();
                }
                catch (Exception e)
                {
                    thisPtr.InvokeCallback(e);
                }
            }
        }
        
        void SendToCollection()
        {
            while (toIndex < toCollection.Count)
            {
                MultiAsyncResult result = (MultiAsyncResult)RecipientCommand.BeginSend(connection, 
                    toCollection[toIndex++].GetSmtpAddress(allowUnicode) + deliveryNotify, 
                    sendToCollectionCompleted, this);
                if (!result.CompletedSynchronously)
                {
                    return;
                }
                string response;
                if (!RecipientCommand.EndSend(result, out response)){
                    failedRecipientExceptions.Add(new SmtpFailedRecipientException(connection.Reader.StatusCode, 
                        toCollection[toIndex - 1].GetSmtpAddress(allowUnicode), response));
                }
            }
            SendData();
        }

        static void SendToCollectionCompleted(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                SendMailAsyncResult thisPtr = (SendMailAsyncResult)result.AsyncState;
                try
                {
                    string response;
                    if (!RecipientCommand.EndSend(result, out response))
                    {
                        thisPtr.failedRecipientExceptions.Add(
                            new SmtpFailedRecipientException(thisPtr.connection.Reader.StatusCode,
                                thisPtr.toCollection[thisPtr.toIndex - 1].GetSmtpAddress(thisPtr.allowUnicode), 
                                response));

                        if (thisPtr.failedRecipientExceptions.Count == thisPtr.toCollection.Count)
                        {
                            SmtpFailedRecipientException exception = null;
                            if (thisPtr.toCollection.Count == 1)
                            {
                                exception = (SmtpFailedRecipientException)thisPtr.failedRecipientExceptions[0];
                            }
                            else
                            {
                                exception = new SmtpFailedRecipientsException(thisPtr.failedRecipientExceptions, true);
                            }
                            exception.fatal = true;
                            thisPtr.InvokeCallback(exception);
                            return;
                        }
                    }
                    thisPtr.SendToCollection();
                }
                catch (Exception e)
                {
                    thisPtr.InvokeCallback(e);
                }
            }
        }

        void SendData()
        {
            IAsyncResult result = DataCommand.BeginSend(connection, sendDataCompleted, this);
            if (!result.CompletedSynchronously)
            {
                return;
            }
            DataCommand.EndSend(result);
            stream = connection.GetClosableStream();
            if (failedRecipientExceptions.Count > 1)
            {
                InvokeCallback(new SmtpFailedRecipientsException(failedRecipientExceptions, failedRecipientExceptions.Count == toCollection.Count));
            }
            else if (failedRecipientExceptions.Count == 1)
            {
                InvokeCallback(failedRecipientExceptions[0]);
            }
            else
            {
                InvokeCallback();
            }
        }

        static void SendDataCompleted(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                SendMailAsyncResult thisPtr = (SendMailAsyncResult)result.AsyncState;
                try
                {
                    DataCommand.EndSend(result);
                    thisPtr.stream = thisPtr.connection.GetClosableStream();
                    if (thisPtr.failedRecipientExceptions.Count > 1)
                    {
                        thisPtr.InvokeCallback(new SmtpFailedRecipientsException(thisPtr.failedRecipientExceptions, thisPtr.failedRecipientExceptions.Count == thisPtr.toCollection.Count));
                    }
                    else if (thisPtr.failedRecipientExceptions.Count == 1)
                    {
                        thisPtr.InvokeCallback(thisPtr.failedRecipientExceptions[0]);
                    }
                    else
                    {
                        thisPtr.InvokeCallback();
                    }
                }
                catch (Exception e)
                {
                    thisPtr.InvokeCallback(e);
                }
            }
        }

        // Return the list of non-terminal failures (some recipients failed but not others).
        internal SmtpFailedRecipientException GetFailedRecipientException()
        {
            if (failedRecipientExceptions.Count == 1)
            {
                return (SmtpFailedRecipientException)failedRecipientExceptions[0];
            }
            else if (failedRecipientExceptions.Count > 1)
            {
                // Aggregate exception, multiple failures
                return new SmtpFailedRecipientsException(failedRecipientExceptions, false);
            }
            return null;
        }
    }
 }
