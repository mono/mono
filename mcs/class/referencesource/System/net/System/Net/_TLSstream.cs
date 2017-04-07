//------------------------------------------------------------------------------
// <copyright file="_TLSstream.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net {
    using System.IO;
    using System.Text;
    using System.Net.Sockets;
    using System.Threading;
    using System.Security.Cryptography.X509Certificates;
    using System.ComponentModel;
    using System.Collections;
    using System.Net.Security;
    using System.Globalization;
    using System.Security.Authentication.ExtendedProtection;
    using System.Net.Configuration;

    internal class TlsStream : NetworkStream, IDisposable {
        private SslState m_Worker;
        private WebExceptionStatus m_ExceptionStatus;
        private string m_DestinationHost;
        private X509CertificateCollection m_ClientCertificates;
        private static AsyncCallback _CompleteIOCallback = new AsyncCallback(CompleteIOCallback);

        private ExecutionContext _ExecutionContext;
        private ChannelBinding m_CachedChannelBinding;

        //
        // This version of an Ssl Stream is for internal HttpWebrequest use.
        // This Ssl client owns the underlined socket
        // The TlsStream will own secured read/write and disposal of the passed "networkStream" stream.
        //
        public TlsStream(string destinationHost, NetworkStream networkStream, X509CertificateCollection clientCertificates, ServicePoint servicePoint, object initiatingRequest, ExecutionContext executionContext)
               :base(networkStream, true) {

        // WebRequest manages the execution context manually so we have to ensure we get one for SSL client certificate demand
        _ExecutionContext = executionContext;
        if (_ExecutionContext == null)
        {
            _ExecutionContext = ExecutionContext.Capture();
        }

        // 


         GlobalLog.Enter("TlsStream::TlsStream", "host="+destinationHost+", #certs="+((clientCertificates == null) ? "none" : clientCertificates.Count.ToString(NumberFormatInfo.InvariantInfo)));
         if (Logging.On) Logging.PrintInfo(Logging.Web, this, ".ctor", "host="+destinationHost+", #certs="+((clientCertificates == null) ? "null" : clientCertificates.Count.ToString(NumberFormatInfo.InvariantInfo)));

         m_ExceptionStatus = WebExceptionStatus.SecureChannelFailure;
         m_Worker = new SslState(networkStream, initiatingRequest is HttpWebRequest, SettingsSectionInternal.Section.EncryptionPolicy);

         m_DestinationHost = destinationHost;
         m_ClientCertificates = clientCertificates;

         RemoteCertValidationCallback certValidationCallback = servicePoint.SetupHandshakeDoneProcedure(this, initiatingRequest);
         m_Worker.SetCertValidationDelegate(certValidationCallback);

         // The Handshake is NOT done at this point
         GlobalLog.Leave("TlsStream::TlsStream (Handshake is not done)");
        }

        //
        // HttpWebRequest as a consumer of this class will ignore any write error, by relying on the read side exception.
        // We want to keep the right failure status for a user application.
        //
        internal WebExceptionStatus ExceptionStatus {
            get {
                return m_ExceptionStatus;
            }
        }

        // This implements the IDisposable contract from the NetworkStream base class
        // Note that finalizer on the base class WILL call us unless there was explicit disposal.
        int m_ShutDown = 0;
        protected override void Dispose(bool disposing) {
            GlobalLog.Print("TlsStream::Dispose()");
            if ( Interlocked.Exchange( ref m_ShutDown,  1) == 1 ) {
                return;
            }
            try {
                if (disposing) {
                    // When KeepAlive is turned off, the TlsStream will be closed before the auth headers for the next request
                    // are computed.  We cannot retrieve the ChannelBinding from the TlsStream after closing it, so we need to
                    // cache it now.
                    m_CachedChannelBinding = GetChannelBinding(ChannelBindingKind.Endpoint);

                    // Note this will not close the underlined socket, only security context
                    m_Worker.Close();
                }
                else {
                    m_Worker = null;
                }
            }
            finally {
                //This will close the underlined socket
                base.Dispose(disposing);
            }
        }

        public override bool DataAvailable {
            get {
                 return m_Worker.DataAvailable || base.DataAvailable;
            }
        }

        //
        // Sync Read version
        //
        public override int Read(byte[] buffer, int offset, int size) {
            GlobalLog.Print("TlsStream#" + ValidationHelper.HashString(this) + "::Read() SecureWorker#" + ValidationHelper.HashString(m_Worker) + " offset:" + offset.ToString() + " size:" + size.ToString());

            if (!m_Worker.IsAuthenticated)
                ProcessAuthentication(null);

            try {
                return m_Worker.SecureStream.Read(buffer, offset, size);

            }
            catch {
                if (m_Worker.IsCertValidationFailed) {
                    m_ExceptionStatus = WebExceptionStatus.TrustFailure;
                }
                else if (m_Worker.LastSecurityStatus != SecurityStatus.OK) {
                    m_ExceptionStatus = WebExceptionStatus.SecureChannelFailure;
                }
                else {
                    m_ExceptionStatus = WebExceptionStatus.ReceiveFailure;
                }
                throw;
            }
        }
        //
        // Async Read version
        //
        public override IAsyncResult BeginRead(byte[] buffer, int offset, int size, AsyncCallback asyncCallback, object asyncState) {
            GlobalLog.Print("TlsStream#" + ValidationHelper.HashString(this) + "::BeginRead() SecureWorker#" + ValidationHelper.HashString(m_Worker) + " offset:" + offset.ToString() + " size:" + size.ToString());

            if (!m_Worker.IsAuthenticated)
            {
                BufferAsyncResult result = new BufferAsyncResult(this, buffer, offset, size, false, asyncState, asyncCallback);
                if (ProcessAuthentication(result))
                    return result;
            }

            try {
                return m_Worker.SecureStream.BeginRead(buffer, offset, size, asyncCallback, asyncState);
            }
            catch {
                if (m_Worker.IsCertValidationFailed) {
                    m_ExceptionStatus = WebExceptionStatus.TrustFailure;
                }
                else if (m_Worker.LastSecurityStatus != SecurityStatus.OK) {
                    m_ExceptionStatus = WebExceptionStatus.SecureChannelFailure;
                }
                else {
                    m_ExceptionStatus = WebExceptionStatus.ReceiveFailure;
                }
                throw;
            }
        }

        // 
        internal override IAsyncResult UnsafeBeginRead(byte[] buffer, int offset, int size, AsyncCallback asyncCallback, object asyncState)
        {
            return BeginRead(buffer, offset, size, asyncCallback, asyncState);
        }

        //
        public override int EndRead(IAsyncResult asyncResult) {
            GlobalLog.Print("TlsStream#" + ValidationHelper.HashString(this) + "::EndRead() IAsyncResult#" + ValidationHelper.HashString(asyncResult));
            try {

                BufferAsyncResult bufferResult = asyncResult as BufferAsyncResult;

                if (bufferResult == null || (object)bufferResult.AsyncObject != this)
                    return m_Worker.SecureStream.EndRead(asyncResult);

                // we have wrapped user IO in case when handshake was not done as the Begin call
                bufferResult.InternalWaitForCompletion();
                Exception e = bufferResult.Result as Exception;
                if (e != null)
                    throw e;

                return (int) bufferResult.Result;
            }
            catch {
                if (m_Worker.IsCertValidationFailed) {
                    m_ExceptionStatus = WebExceptionStatus.TrustFailure;
                }
                else if (m_Worker.LastSecurityStatus != SecurityStatus.OK) {
                    m_ExceptionStatus = WebExceptionStatus.SecureChannelFailure;
                }
                else {
                    m_ExceptionStatus = WebExceptionStatus.ReceiveFailure;
                }
                throw;
            }
        }


        //
        // Write, all flavours: synchrnous and asynchrnous
        //
        public override void Write(byte[] buffer, int offset, int size) {
            GlobalLog.Print("TlsStream#" + ValidationHelper.HashString(this) + "::Write() SecureWorker#" + ValidationHelper.HashString(m_Worker) + " offset:" + offset.ToString() + " size:" + size.ToString());

            if (!m_Worker.IsAuthenticated)
                ProcessAuthentication(null);

            try {
                m_Worker.SecureStream.Write(buffer, offset, size);
            }
            catch {
                // We preserve the original status of a failure because the read
                // side will now fail with object dispose error.
                if (m_Worker.IsCertValidationFailed) {
                    m_ExceptionStatus = WebExceptionStatus.TrustFailure;
                }
                else if (m_Worker.LastSecurityStatus != SecurityStatus.OK) {
                    m_ExceptionStatus = WebExceptionStatus.SecureChannelFailure;
                }
                else {
                    m_ExceptionStatus = WebExceptionStatus.SendFailure;
                }
                //HttpWbeRequest depends on the phyical stream to be dropped on a write error.
                Socket chkSocket = this.Socket;
                if(chkSocket != null) {
                    chkSocket.InternalShutdown(SocketShutdown.Both);
                }
                throw;
            }
        }

        //
        // BeginWrite -
        //
        // Write the bytes to the write - while encrypting
        //
        // copy plain text data to a temporary buffer
        // encrypt the data
        // once the data is encrypted clear the plain text for security
        //
        public override IAsyncResult BeginWrite( byte[] buffer, int offset, int size, AsyncCallback asyncCallback, object asyncState) {
            GlobalLog.Print("TlsStream#" + ValidationHelper.HashString(this) + "::BeginWrite() SecureWorker#" + ValidationHelper.HashString(m_Worker) + " offset:" + offset.ToString() + " size:" + size.ToString());
            if (!m_Worker.IsAuthenticated)
            {
                BufferAsyncResult result = new BufferAsyncResult(this, buffer, offset, size, true, asyncState, asyncCallback);
                if (ProcessAuthentication(result))
                    return result;
            }

            try {
                return m_Worker.SecureStream.BeginWrite(buffer, offset, size, asyncCallback, asyncState);
            }
            catch {
                if (m_Worker.IsCertValidationFailed) {
                    m_ExceptionStatus = WebExceptionStatus.TrustFailure;
                }
                else if (m_Worker.LastSecurityStatus != SecurityStatus.OK) {
                    m_ExceptionStatus = WebExceptionStatus.SecureChannelFailure;
                }
                else {
                    m_ExceptionStatus = WebExceptionStatus.SendFailure;
                }
                throw;
            }
        }


        internal override IAsyncResult UnsafeBeginWrite( byte[] buffer, int offset, int size, AsyncCallback asyncCallback, object asyncState) {
            return BeginWrite(buffer,offset,size,asyncCallback,asyncState);
        }



        public override void EndWrite(IAsyncResult asyncResult) {
            GlobalLog.Print("TlsStream#" + ValidationHelper.HashString(this) + "::EndWrite() IAsyncResult#" + ValidationHelper.HashString(asyncResult));

            try {
                BufferAsyncResult bufferResult = asyncResult as BufferAsyncResult;

                if (bufferResult == null || (object)bufferResult.AsyncObject != this)
                {
                    m_Worker.SecureStream.EndWrite(asyncResult);
                }
                else
                {
                    // we have wrapped user IO in case when handshake was not done as the Begin call
                    bufferResult.InternalWaitForCompletion();
                    Exception e = bufferResult.Result as Exception;
                    if (e != null)
                        throw e;
                }
            }
            catch {
                //HttpWebRequest depends on the stream to be dropped on a write error.
                Socket chkSocket = this.Socket;
                if(chkSocket != null) {
                    chkSocket.InternalShutdown(SocketShutdown.Both);
                }
                // We also preserve the original status of a failure because the read
                // side will now fail with object dispose error.
                if (m_Worker.IsCertValidationFailed) {
                    m_ExceptionStatus = WebExceptionStatus.TrustFailure;
                }
                else if (m_Worker.LastSecurityStatus != SecurityStatus.OK) {
                    m_ExceptionStatus = WebExceptionStatus.SecureChannelFailure;
                }
                else {
                    m_ExceptionStatus = WebExceptionStatus.SendFailure;
                }
                throw;
            }
        }

        internal override void MultipleWrite(BufferOffsetSize[] buffers) {
            GlobalLog.Print("TlsStream#" + ValidationHelper.HashString(this) + "::MultipleWrite() SecureWorker#" + ValidationHelper.HashString(m_Worker) + " buffers.Length:" + buffers.Length.ToString());

            if (!m_Worker.IsAuthenticated)
                ProcessAuthentication(null);

            try {
                m_Worker.SecureStream.Write(buffers);
            }
            catch {
                //HttpWbeRequest depends on the physical stream to be dropped on a write error.
                Socket chkSocket = this.Socket;
                if(chkSocket != null) {
                    chkSocket.InternalShutdown(SocketShutdown.Both);
                }
                // We preserve the original status of a failure because the read
                // side will now fail with object dispose error.
                if (m_Worker.IsCertValidationFailed) {
                    m_ExceptionStatus = WebExceptionStatus.TrustFailure;
                }
                else if (m_Worker.LastSecurityStatus != SecurityStatus.OK) {
                    m_ExceptionStatus = WebExceptionStatus.SecureChannelFailure;
                }
                else {
                    m_ExceptionStatus = WebExceptionStatus.SendFailure;
                }
                throw;
            }
        }

        internal override IAsyncResult BeginMultipleWrite(BufferOffsetSize[] buffers, AsyncCallback callback, object state) {
            GlobalLog.Print("TlsStream#" + ValidationHelper.HashString(this) + "::BeginMultipleWrite() SecureWorker#" + ValidationHelper.HashString(m_Worker) + " buffers.Length:" + buffers.Length.ToString());
            if (!m_Worker.IsAuthenticated)
            {
                BufferAsyncResult result = new BufferAsyncResult(this, buffers, state, callback);
                if (ProcessAuthentication(result))
                    return result;
            }

            try {
                return m_Worker.SecureStream.BeginWrite(buffers, callback, state);
            }
            catch {
                if (m_Worker.IsCertValidationFailed) {
                    m_ExceptionStatus = WebExceptionStatus.TrustFailure;
                }
                else if (m_Worker.LastSecurityStatus != SecurityStatus.OK) {
                    m_ExceptionStatus = WebExceptionStatus.SecureChannelFailure;
                }
                else {
                    m_ExceptionStatus = WebExceptionStatus.SendFailure;
                }
                throw;
            }
        }

        internal override IAsyncResult UnsafeBeginMultipleWrite(BufferOffsetSize[] buffers, AsyncCallback callback, object state) {
            return BeginMultipleWrite(buffers,callback,state);
        }

        // overrides base.EndMultipeWrite
        internal override void EndMultipleWrite(IAsyncResult asyncResult)
        {
            GlobalLog.Print("TlsStream#" + ValidationHelper.HashString(this) + "::EndMultipleWrite() IAsyncResult#" + ValidationHelper.HashString(asyncResult));
            EndWrite(asyncResult);
        }


        public X509Certificate ClientCertificate {
            get {
                return m_Worker.InternalLocalCertificate;
            }
        }

        internal ChannelBinding GetChannelBinding(ChannelBindingKind kind)
        {
            if (kind == ChannelBindingKind.Endpoint && m_CachedChannelBinding != null)
            {
                return m_CachedChannelBinding;
            }

            return m_Worker.GetChannelBinding(kind);
        }

        //
        // This methods ensures that IO is only issued when the handshake is completed in ether way
        // The very first coming IO will initiate the handshake and define it's flavor (sync/async).
        //
        // Returns false if the handshake was already done.
        // Returns true  if the user IO is queued and the handshake is started.
        // Return value is not applicable in sync case.
        //
        private ArrayList m_PendingIO = new ArrayList();
        internal bool ProcessAuthentication(LazyAsyncResult result)
        {
            bool doHandshake = false;
            bool isSyncCall = result == null;

            lock (m_PendingIO)
            {
                // do we have handshake as already done before we grabbed a lock?
                if (m_Worker.IsAuthenticated)
                    return false;

                if (m_PendingIO.Count == 0)
                {
                    doHandshake = true;
                }

                if (isSyncCall)
                {
                    // we will wait on this guy in this method for the handshake to complete
                    result = new LazyAsyncResult(this, null, null);
                }

                m_PendingIO.Add(result);
            }

            try {
                if (doHandshake)
                {
                    bool success = true;
                    LazyAsyncResult handshakeResult = null;
                    try
                    {
                        m_Worker.ValidateCreateContext(false,
                                                       m_DestinationHost,
                                                       (System.Security.Authentication.SslProtocols)ServicePointManager.SecurityProtocol,
                                                       null, m_ClientCertificates,
                                                       true,
                                                       ServicePointManager.CheckCertificateRevocationList,
                                                       ServicePointManager.CheckCertificateName);


                        if (!isSyncCall)
                        {
                            // wrap a user async IO/Handshake request into auth request
                            handshakeResult = new LazyAsyncResult(m_Worker, null, new AsyncCallback(WakeupPendingIO));
#if DEBUG
                            result._DebugAsyncChain = handshakeResult;
#endif
                        }

                        //
                        // TlsStream is used by classes that manually control ExecutionContext, so set it here if we need to.
                        //
                        if (_ExecutionContext != null)
                        {
                            ExecutionContext.Run(_ExecutionContext.CreateCopy(), new ContextCallback(CallProcessAuthentication), handshakeResult);
                        }
                        else
                        {
                            m_Worker.ProcessAuthentication(handshakeResult);
                        }
                    }
                    catch
                    {
                        success = false;
                        throw;
                    }
                    finally
                    {
                        if (isSyncCall || !success)
                        {
                            lock (m_PendingIO)
                            {
                                if(m_PendingIO.Count > 1)
                                {
                                    // It was a real sync handshake (now completed) and another IO came in.
                                    // It's now waiting on us so resume.
                                    ThreadPool.QueueUserWorkItem(new WaitCallback(StartWakeupPendingIO), null);
                                }
                                else {
                                    m_PendingIO.Clear();
                                }
                            }
                        }
                    }
                }
                else if (isSyncCall)
                {
                    GlobalLog.Assert(result != null, "TlsStream::ProcessAuthentication() this is a Sync call and it did not started the handshake hence null result must be wrapped into LazyAsyncResult");
                    Exception e = result.InternalWaitForCompletion() as Exception;
                    if (e != null)
                        throw e;
                }
            }
            catch {
                if (m_Worker.IsCertValidationFailed) {
                    m_ExceptionStatus = WebExceptionStatus.TrustFailure;
                }
                else if (m_Worker.LastSecurityStatus != SecurityStatus.OK) {
                    m_ExceptionStatus = WebExceptionStatus.SecureChannelFailure;
                }
                else {
                    m_ExceptionStatus = WebExceptionStatus.ReceiveFailure;
                }
                throw;
            }

            // Here in the async case a user IO has been queued (and may be already completed)
            // For sync case it does not matter since the caller will resume IO upon return
            return true;
        }
        //
        void CallProcessAuthentication(object state)
        {
            m_Worker.ProcessAuthentication((LazyAsyncResult)state);
        }

        //
        private void StartWakeupPendingIO(object nullState)
        {
            // state must be is  null here
            GlobalLog.Assert(nullState == null, "TlsStream::StartWakeupPendingIO|Expected null state but got {0}.", nullState == null ? "null" : (nullState.GetType().FullName));
            WakeupPendingIO(null);
        }
        //
        // This is proven to be called without any user stack or it was just ONE IO queued.
        //
        private void WakeupPendingIO(IAsyncResult ar)
        {
            Exception exception = null;
            try {
                if (ar != null)
                    m_Worker.EndProcessAuthentication(ar);
            }
            catch (Exception e) {
                // This method does not throw because it job is to notify everyon waiting on the result
                // NOTE: SSL engine remembers the exception and will rethrow it on any access for SecureStream
                // property means on  any IO attempt.
                exception = e;

                if (m_Worker.IsCertValidationFailed) {
                    m_ExceptionStatus = WebExceptionStatus.TrustFailure;
                }
                else if (m_Worker.LastSecurityStatus != SecurityStatus.OK) {
                    m_ExceptionStatus = WebExceptionStatus.SecureChannelFailure;
                }
                else {
                    m_ExceptionStatus = WebExceptionStatus.ReceiveFailure;
                }
            }

            lock (m_PendingIO)
            {
                while(m_PendingIO.Count != 0)
                {
                    LazyAsyncResult lazyResult = (LazyAsyncResult )m_PendingIO[m_PendingIO.Count-1];

                    m_PendingIO.RemoveAt(m_PendingIO.Count-1);

                    if (lazyResult is BufferAsyncResult)
                    {
                        if (m_PendingIO.Count == 0)
                        {
                            // Resume the LAST IO on that thread and offload other IOs on worker threads
                            ResumeIOWorker(lazyResult);
                        }
                        else
                        {
                            ThreadPool.QueueUserWorkItem(new WaitCallback(ResumeIOWorker), lazyResult);
                        }
                    }
                    else
                    {
                        //resume sync IO waiting on other thread or signal waiting async handshake result.
                        try {
                            lazyResult.InvokeCallback(exception);
                        }
                        catch {
                            // this method never throws unles the failure is catastrophic
                        }
                    }
                }
            }
        }

        private void ResumeIOWorker(object result)
        {
            BufferAsyncResult bufferResult = (BufferAsyncResult) result;
            try
            {
                ResumeIO(bufferResult);
            }
            catch (Exception exception)
            {
                if (exception is OutOfMemoryException || exception is StackOverflowException || exception is ThreadAbortException)
                {
                    throw;
                }
                if (bufferResult.InternalPeekCompleted)
                    throw;
                bufferResult.InvokeCallback(exception);
            }
        }

        //
        // Resumes async Read or Write after the handshake is done
        //
        private void ResumeIO(BufferAsyncResult bufferResult)
        {
            IAsyncResult result;
            if (bufferResult.IsWrite)
            {
                if (bufferResult.Buffers != null)
                    result = m_Worker.SecureStream.BeginWrite(bufferResult.Buffers, _CompleteIOCallback, bufferResult);
                else
                    result = m_Worker.SecureStream.BeginWrite(bufferResult.Buffer, bufferResult.Offset, bufferResult.Count, _CompleteIOCallback, bufferResult);
            }
            else
            {
                result = m_Worker.SecureStream.BeginRead(bufferResult.Buffer, bufferResult.Offset, bufferResult.Count, _CompleteIOCallback, bufferResult);
            }

            if (result.CompletedSynchronously)
            {
                CompleteIO(result);
            }
        }

        //
        //
        //
        private static void CompleteIOCallback(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
            {
                return;
            }

            try
            {
                CompleteIO(result);
            }
            catch (Exception exception)
            {
                if (exception is OutOfMemoryException || exception is StackOverflowException || exception is ThreadAbortException)
                {
                    throw;
                }

                if (((LazyAsyncResult) result.AsyncState).InternalPeekCompleted)
                    throw;
                ((LazyAsyncResult) result.AsyncState).InvokeCallback(exception);
            }
        }

        private static void CompleteIO(IAsyncResult result)
        {
            BufferAsyncResult bufferResult = (BufferAsyncResult) result.AsyncState;

            object readBytes = null;
            if (bufferResult.IsWrite)
                ((TlsStream)bufferResult.AsyncObject).m_Worker.SecureStream.EndWrite(result);
            else
                readBytes = ((TlsStream)bufferResult.AsyncObject).m_Worker.SecureStream.EndRead(result);

            bufferResult.InvokeCallback(readBytes);
        }

        //IT should be virtual but we won't keep internal virtual even in debug version
#if TRAVE
        [System.Diagnostics.Conditional("TRAVE")]
        internal new void DebugMembers() {
            GlobalLog.Print("m_ExceptionStatus: " + m_ExceptionStatus);
            GlobalLog.Print("m_DestinationHost: " + m_DestinationHost);
            GlobalLog.Print("m_Worker:");
            m_Worker.DebugMembers();
            base.DebugMembers();
        }
#endif


    }; // class TlsStream

} // namespace System.Net
