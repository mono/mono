// ------------------------------------------------------------------------------
// <copyright file="FtpDataStream.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// ------------------------------------------------------------------------------
//

namespace System.Net {

    using System.IO;
    using System.Net.Sockets;
    using System.Security.Permissions;


    /// <devdoc>
    /// <para>
    ///     The FtpDataStream class implements a data FTP connection,
    /// </para>
    /// </devdoc>
    internal class FtpDataStream : Stream, ICloseEx {

        private FtpWebRequest    m_Request;
        private NetworkStream    m_NetworkStream;
        private bool             m_Writeable;
        private bool             m_Readable;
        private bool             m_IsFullyRead = false;
        bool                     m_Closing = false;


        internal FtpDataStream(NetworkStream networkStream, FtpWebRequest request, TriState writeOnly)  {
            GlobalLog.Print("FtpDataStream#" + ValidationHelper.HashString(this) + "::FtpDataStream");
            m_Readable = true;
            m_Writeable = true;
            if (writeOnly == TriState.True) {
                m_Readable = false;
            } else if (writeOnly == TriState.False) {
                m_Writeable = false;
            }
            m_NetworkStream = networkStream;
            m_Request = request;
        }

        protected override void Dispose(bool disposing)
        {
            try {
                if (disposing)
                    ((ICloseEx)this).CloseEx(CloseExState.Normal);
                else
                    ((ICloseEx)this).CloseEx(CloseExState.Abort | CloseExState.Silent);
            }
            finally {
                base.Dispose(disposing);
            }
        }

        void ICloseEx.CloseEx(CloseExState closeState) {
            GlobalLog.Print("FtpDataStream#" + ValidationHelper.HashString(this) + "::CloseEx, state = " + closeState.ToString());

            lock (this)
            {
                if (m_Closing == true)
                    return;
                m_Closing = true;
                m_Writeable = false;
                m_Readable = false;
            }

            try {
                try {
                    if ((closeState & CloseExState.Abort) == 0)
                        m_NetworkStream.Close(Socket.DefaultCloseTimeout);
                    else
                        m_NetworkStream.Close(0);
                } finally {
                    m_Request.DataStreamClosed(closeState);
                }
            }
            catch (Exception exception) {
                bool doThrow = true;
                WebException webException = exception as WebException;
                if (webException != null) {
                    FtpWebResponse response = webException.Response as FtpWebResponse;
                    if (response != null)
                    {
                        if (!m_IsFullyRead
                            && response.StatusCode == FtpStatusCode.ConnectionClosed)
                            doThrow = false;
                    }
                }

                if (doThrow)
                    if ((closeState & CloseExState.Silent) == 0)
                        throw;
            }
        }

        /// <summary>
        ///    <para>Rethrows the exception</para>
        /// </summary>
        private void CheckError() {
            if (m_Request.Aborted) {
                throw new WebException(
                              NetRes.GetWebStatusString(
                                  "net_requestaborted", 
                                  WebExceptionStatus.RequestCanceled),
                              WebExceptionStatus.RequestCanceled);
            }    
        }


        /// <devdoc>
        ///    <para>Indicates that data can be read from the stream.
        /// </devdoc>
        public override bool CanRead {
            get {
                return m_Readable;
            }
        }

        /// <devdoc>
        ///    <para>Indicates that the stream is seekable</para>
        /// </devdoc>
        public override bool CanSeek {
            get {
                return m_NetworkStream.CanSeek;
            }
        }


        /// <devdoc>
        ///    <para>Indicates that the stream is writeable</para>
        /// </devdoc>
        public override bool CanWrite {
            get {
                return m_Writeable;
            }
        }

        /// <devdoc>
        ///    <para>Indicates that the stream is writeable</para>
        /// </devdoc>
        public override long Length {
            get {
                return m_NetworkStream.Length;
            }
        }

        /// <devdoc>
        ///    <para>Gets or sets the position in the stream. Always throws <see cref='NotSupportedException'/>.</para>
        /// </devdoc>
        public override long Position {
            get {
                return m_NetworkStream.Position;
            }

            set {
                m_NetworkStream.Position = value;
            }
        }

        /// <devdoc>
        ///    <para>Seeks a specific position in the stream.</para>
        /// </devdoc>
        public override long Seek(long offset, SeekOrigin origin) {
            CheckError();
            try {
                return m_NetworkStream.Seek(offset, origin);
            } catch {
                CheckError();
                throw;
            }
        }


        /// <devdoc>
        ///    <para> Reads data from the stream. </para>
        /// </devdoc>
        public override int Read(byte[] buffer, int offset, int size) {
            CheckError();
            int readBytes;
            try {
                readBytes = m_NetworkStream.Read(buffer, offset, size);
            } catch {
                CheckError();
                throw;
            }
            if (readBytes == 0)
            {
                m_IsFullyRead = true;
                Close();
            }
            return readBytes;
        }


        /// <devdoc>
        ///    <para>Writes data to the stream.</para>
        /// </devdoc>
       public override void Write(byte[] buffer, int offset, int size) {
            CheckError();
            try {
                m_NetworkStream.Write(buffer, offset, size);
            } catch {
                CheckError();
                throw;
            }
        }


        private void AsyncReadCallback(IAsyncResult ar)
        {
            LazyAsyncResult userResult = (LazyAsyncResult) ar.AsyncState;
            try {
                try {
                    int readBytes = m_NetworkStream.EndRead(ar);
                    if (readBytes == 0)
                    {
                        m_IsFullyRead = true;
                        Close(); // This should block for pipeline completion
                    }
                    userResult.InvokeCallback(readBytes);
                }
                catch (Exception exception) {
                    // Complete with error. If already completed rethrow on the worker thread
                    if (!userResult.IsCompleted)
                        userResult.InvokeCallback(exception);
                }
            } catch {}
        }

        /// <devdoc>
        ///    <para>
        ///       Begins an asychronous read from a stream.
        ///    </para>
        /// </devdoc>
        [HostProtection(ExternalThreading=true)]
        public override IAsyncResult BeginRead(byte[] buffer, int offset, int size, AsyncCallback callback, Object state) {
            CheckError();
            LazyAsyncResult userResult = new LazyAsyncResult(this, state, callback);
            try {
                m_NetworkStream.BeginRead(buffer, offset, size, new AsyncCallback(AsyncReadCallback), userResult);
            } catch {
                CheckError();
                throw;
            }
            return userResult;
        }

        /// <devdoc>
        ///    <para>
        ///       Handle the end of an asynchronous read.
        ///    </para>
        /// </devdoc>
        public override int EndRead(IAsyncResult ar) {
            try {
                object result = ((LazyAsyncResult)ar).InternalWaitForCompletion();
                
                if (result is Exception)
                    throw (Exception) result;

                return (int)result;
                    
            }
            finally {
                CheckError();
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Begins an asynchronous write to a stream.
        ///    </para>
        /// </devdoc>
        [HostProtection(ExternalThreading=true)]
        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int size, AsyncCallback callback, Object state) {
            CheckError();
            try {
                return m_NetworkStream.BeginWrite(buffer, offset, size, callback, state);
            } catch {
                CheckError();
                throw;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Handle the end of an asynchronous write.
        ///    </para>
        /// </devdoc>
        public override void EndWrite(IAsyncResult asyncResult) {
            try {
                m_NetworkStream.EndWrite(asyncResult);
            }
            finally {
                CheckError();
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Flushes data from the stream.
        ///    </para>
        /// </devdoc>
        public override void Flush() {
            m_NetworkStream.Flush();
        }

        /// <devdoc>
        ///    <para>
        ///       Sets the length of the stream. Always throws <see cref='NotSupportedException'/>
        ///       .
        ///    </para>
        /// </devdoc>
        public override void SetLength(long value) {
            m_NetworkStream.SetLength(value);
        }

        /// <devdoc>
        ///    <para>Indicates whether we can timeout</para>
        /// </devdoc>
        public override bool CanTimeout {
            get {
                return m_NetworkStream.CanTimeout;
            }
        }


        /// <devdoc>
        ///    <para>Set/Get ReadTimeout</para>
        /// </devdoc>
        public override int ReadTimeout {
            get {
                return m_NetworkStream.ReadTimeout;
            }
            set {
                m_NetworkStream.ReadTimeout = value;
            }
        }

        /// <devdoc>
        ///    <para>Set/Get WriteTimeout</para>
        /// </devdoc>
        public override int WriteTimeout {
            get {
                return m_NetworkStream.WriteTimeout;
            }
            set {
                m_NetworkStream.WriteTimeout = value;
            }
        }

        internal void SetSocketTimeoutOption(SocketShutdown mode, int timeout, bool silent) {
            m_NetworkStream.SetSocketTimeoutOption(mode, timeout, silent);
        }
    }
}
