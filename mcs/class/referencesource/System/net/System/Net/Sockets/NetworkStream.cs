//------------------------------------------------------------------------------
// <copyright file="NetworkStream.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net.Sockets {
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Security.Permissions;
    using System.Threading.Tasks;

    /// <devdoc>
    ///    <para>
    ///       Provides the underlying stream of data for network access.
    ///    </para>
    /// </devdoc>
    public class NetworkStream : Stream {
        /// <devdoc>
        ///    <para>
        ///       Used by the class to hold the underlying socket the stream uses.
        ///    </para>
        /// </devdoc>
        private Socket    m_StreamSocket;

        /// <devdoc>
        ///    <para>
        ///       Used by the class to indicate that the stream is m_Readable.
        ///    </para>
        /// </devdoc>
        private bool      m_Readable;

        /// <devdoc>
        ///    <para>
        ///       Used by the class to indicate that the stream is writable.
        ///    </para>
        /// </devdoc>
        private bool      m_Writeable;

        private bool      m_OwnsSocket;

        /// <devdoc>
        /// <para>Creates a new instance of the <see cref='System.Net.Sockets.NetworkStream'/> without initalization.</para>
        /// </devdoc>
        internal NetworkStream() {
            m_OwnsSocket = true;
        }


        // Can be constructed directly out of a socket
        /// <devdoc>
        /// <para>Creates a new instance of the <see cref='System.Net.Sockets.NetworkStream'/> class for the specified <see cref='System.Net.Sockets.Socket'/>.</para>
        /// </devdoc>
        public NetworkStream(Socket socket) {
#if DEBUG
            using (GlobalLog.SetThreadKind(ThreadKinds.User)) {
#endif
            if (socket == null) {
                throw new ArgumentNullException("socket");
            }
            InitNetworkStream(socket, FileAccess.ReadWrite);
#if DEBUG
            }
#endif
        }

        //UEUE (see FileStream)
        // ownsHandle: true if the file handle will be owned by this NetworkStream instance; otherwise, false.
        public NetworkStream(Socket socket, bool ownsSocket) {
#if DEBUG
            using (GlobalLog.SetThreadKind(ThreadKinds.User)) {
#endif
            if (socket == null) {
                throw new ArgumentNullException("socket");
            }
            InitNetworkStream(socket, FileAccess.ReadWrite);
            m_OwnsSocket = ownsSocket;
#if DEBUG
            }
#endif
        }

        internal NetworkStream(NetworkStream networkStream, bool ownsSocket) {
            Socket socket = networkStream.Socket;
            if (socket == null) {
                throw new ArgumentNullException("networkStream");
            }
            InitNetworkStream(socket, FileAccess.ReadWrite);
            m_OwnsSocket = ownsSocket;
        }

        // Create with a socket and access mode
        /// <devdoc>
        /// <para>Creates a new instance of the <see cref='System.Net.Sockets.NetworkStream'/> class for the specified <see cref='System.Net.Sockets.Socket'/> with the specified access rights.</para>
        /// </devdoc>
        public NetworkStream(Socket socket, FileAccess access) {
#if DEBUG
            using (GlobalLog.SetThreadKind(ThreadKinds.User)) {
#endif
            if (socket == null) {
                throw new ArgumentNullException("socket");
            }
            InitNetworkStream(socket, access);
#if DEBUG
            }
#endif
        }
        public NetworkStream(Socket socket, FileAccess access, bool ownsSocket) {
#if DEBUG
            using (GlobalLog.SetThreadKind(ThreadKinds.User)) {
#endif
            if (socket == null) {
                throw new ArgumentNullException("socket");
            }
            InitNetworkStream(socket, access);
            m_OwnsSocket = ownsSocket;
#if DEBUG
            }
#endif
        }

        //
        // Socket - provides access to socket for stream closing
        //
        protected Socket Socket {
            get {
                return m_StreamSocket;
            }
        }

        internal Socket InternalSocket {
            get {
                Socket chkSocket = m_StreamSocket;
                if (m_CleanedUp || chkSocket == null) {
                    throw new ObjectDisposedException(this.GetType().FullName);
                }

                return chkSocket;
            }
        }

        internal void InternalAbortSocket() 
        {
            if (!m_OwnsSocket)
            {
                throw new InvalidOperationException();
            }

            Socket chkSocket = m_StreamSocket;
            if (m_CleanedUp || chkSocket == null)
            {
                return;
            }

            try
            {
                chkSocket.Close(0);
            }
            catch (ObjectDisposedException)
            {
            }
        }

        internal void ConvertToNotSocketOwner() {
            m_OwnsSocket = false;
            // Suppress for finialization still allow proceed the requests
            GC.SuppressFinalize(this);
        }

        /// <devdoc>
        ///    <para>
        ///       Used by the class to indicate that the stream is m_Readable.
        ///    </para>
        /// </devdoc>
        protected bool Readable {
            get {
                return m_Readable;
            }
            set {
                m_Readable = value;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Used by the class to indicate that the stream is writable.
        ///    </para>
        /// </devdoc>
        protected bool Writeable {
            get {
                return m_Writeable;
            }
            set {
                m_Writeable = value;
            }
        }


        /// <devdoc>
        ///    <para>
        ///       Indicates that data can be read from the stream.
        ///         We return the readability of this stream. This is a read only property.
        ///    </para>
        /// </devdoc>
        public override bool CanRead {
            get {
                return m_Readable;
            }
        }


        /// <devdoc>
        ///    <para>
        ///       Indicates that the stream can seek a specific location
        ///       in the stream. This property always returns <see langword='false'/>
        ///       .
        ///    </para>
        /// </devdoc>
        public override bool CanSeek {
            get {
                return false;
            }
        }


        /// <devdoc>
        ///    <para>
        ///       Indicates that data can be written to the stream.
        ///    </para>
        /// </devdoc>
        public override bool CanWrite {
            get {
                return m_Writeable;
            }
        }


        /// <devdoc>
        ///    <para>Indicates whether we can timeout</para>
        /// </devdoc>
        public override bool CanTimeout { 
            get {
                return true; // should we check for Connected state?
            }
        }


        /// <devdoc>
        ///    <para>Set/Get ReadTimeout, note of a strange behavior, 0 timeout == infinite for sockets,
        ///         so we map this to -1, and if you set 0, we cannot support it</para>
        /// </devdoc>
        public override int ReadTimeout { 
            get {
#if DEBUG
                using (GlobalLog.SetThreadKind(ThreadKinds.User | ThreadKinds.Async)) {
#endif
                int timeout = (int)m_StreamSocket.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout);
                if (timeout == 0) {
                    return -1;
                }
                return timeout;
#if DEBUG
                }
#endif
            }
            set {
#if DEBUG
                using (GlobalLog.SetThreadKind(ThreadKinds.User | ThreadKinds.Async)) {
#endif
                if (value<=0 && value!=System.Threading.Timeout.Infinite) {
                    throw new ArgumentOutOfRangeException("value", SR.GetString(SR.net_io_timeout_use_gt_zero));
                }
                SetSocketTimeoutOption(SocketShutdown.Receive, value, false);
#if DEBUG
                }
#endif
            }
        }

        /// <devdoc>
        ///    <para>Set/Get WriteTimeout, note of a strange behavior, 0 timeout == infinite for sockets,
        ///         so we map this to -1, and if you set 0, we cannot support it</para>
        /// </devdoc>
	    public override int WriteTimeout { 
            get {
#if DEBUG
                using (GlobalLog.SetThreadKind(ThreadKinds.User | ThreadKinds.Async)) {
#endif
                int timeout = (int)m_StreamSocket.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout);
                if (timeout == 0) {
                    return -1;
                }
                return timeout;
#if DEBUG
                }
#endif
            }
            set {
#if DEBUG
                using (GlobalLog.SetThreadKind(ThreadKinds.User | ThreadKinds.Async)) {
#endif
                if (value <= 0 && value != System.Threading.Timeout.Infinite) {
                    throw new ArgumentOutOfRangeException("value", SR.GetString(SR.net_io_timeout_use_gt_zero));
                }
                SetSocketTimeoutOption(SocketShutdown.Send, value, false);
#if DEBUG
                }
#endif
            }
        }        

        /// <devdoc>
        ///    <para>
        ///       Indicates data is available on the stream to be read.
        ///         This property checks to see if at least one byte of data is currently available            
        ///    </para>
        /// </devdoc>
        public virtual bool DataAvailable {
            get {
#if DEBUG
                using (GlobalLog.SetThreadKind(ThreadKinds.User | ThreadKinds.Async)) {
#endif
                if (m_CleanedUp){
                    throw new ObjectDisposedException(this.GetType().FullName);
                }

                Socket chkStreamSocket = m_StreamSocket;
                if(chkStreamSocket == null) {
                    throw new IOException(SR.GetString(SR.net_io_readfailure, SR.GetString(SR.net_io_connectionclosed)));
                }

                // Ask the socket how many bytes are available. If it's
                // not zero, return true.

                return chkStreamSocket.Available != 0;
#if DEBUG
                }
#endif
            }
        }


        /// <devdoc>
        ///    <para>
        ///       The length of data available on the stream. Always throws <see cref='NotSupportedException'/>.
        ///    </para>
        /// </devdoc>
        public override long Length {
            get {
                throw new NotSupportedException(SR.GetString(SR.net_noseek));
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the position in the stream. Always throws <see cref='NotSupportedException'/>.
        ///    </para>
        /// </devdoc>
        public override long Position {
            get {
                throw new NotSupportedException(SR.GetString(SR.net_noseek));
            }

            set {
                throw new NotSupportedException(SR.GetString(SR.net_noseek));
            }
        }


        /// <devdoc>
        ///    <para>
        ///       Seeks a specific position in the stream. This method is not supported by the
        ///    <see cref='NetworkStream'/> class.
        ///    </para>
        /// </devdoc>
        public override long Seek(long offset, SeekOrigin origin) {
            throw new NotSupportedException(SR.GetString(SR.net_noseek));
        }


        /*++

            InitNetworkStream - initialize a network stream.

            This is the common NetworkStream constructor, called whenever a
            network stream is created. We validate the socket, set a few
            options, and call our parent's initializer.

            Input:

                S           - Socket to be used.
                Access      - Access type desired.


            Returns:

                Nothing, but may throw an exception.
        --*/

        internal void InitNetworkStream(Socket socket, FileAccess Access) {
            //
            // parameter validation
            //
            if (!socket.Blocking) {
                throw new IOException(SR.GetString(SR.net_sockets_blocking));
            }
            if (!socket.Connected) {
                throw new IOException(SR.GetString(SR.net_notconnected));
            }
            if (socket.SocketType != SocketType.Stream) {
                throw new IOException(SR.GetString(SR.net_notstream));
            }

            m_StreamSocket = socket;

            switch (Access) {
                case FileAccess.Read:
                    m_Readable = true;
                    break;
                case FileAccess.Write:
                    m_Writeable = true;
                    break;
                case FileAccess.ReadWrite:
                default: // assume FileAccess.ReadWrite
                    m_Readable = true;
                    m_Writeable = true;
                    break;
            }

        }

        internal bool PollRead() {
            if (m_CleanedUp) {
                return false;
            }
            Socket chkStreamSocket = m_StreamSocket;
            if (chkStreamSocket == null) {
                return false;
            }
            return chkStreamSocket.Poll(0, SelectMode.SelectRead);
        }

        internal bool Poll(int microSeconds, SelectMode mode) {
            if (m_CleanedUp){
                throw new ObjectDisposedException(this.GetType().FullName);
            }

            Socket chkStreamSocket = m_StreamSocket;
            if (chkStreamSocket == null) {
                throw new IOException(SR.GetString(SR.net_io_readfailure, SR.GetString(SR.net_io_connectionclosed)));
            }

            return chkStreamSocket.Poll(microSeconds, mode);
        }


        /*++
            Read - provide core Read functionality.

            Provide core read functionality. All we do is call through to the
            socket Receive functionality.

            Input:

                Buffer  - Buffer to read into.
                Offset  - Offset into the buffer where we're to read.
                Count   - Number of bytes to read.

            Returns:

                Number of bytes we read, or 0 if the socket is closed.

        --*/

        /// <devdoc>
        ///    <para>
        ///       Reads data from the stream.
        ///    </para>
        /// </devdoc>
        //UEUE
        public override int Read([In, Out] byte[] buffer, int offset, int size) {
#if DEBUG
            using (GlobalLog.SetThreadKind(ThreadKinds.User | ThreadKinds.Sync)) {
#endif
            bool canRead = CanRead;  // Prevent race with Dispose.
            if (m_CleanedUp){
                throw new ObjectDisposedException(this.GetType().FullName);
            }
            if (!canRead) {    
                throw new InvalidOperationException(SR.GetString(SR.net_writeonlystream));
            }
            //
            // parameter validation
            //
            if (buffer==null) {
                throw new ArgumentNullException("buffer");
            }
            if (offset<0 || offset>buffer.Length) {
                throw new ArgumentOutOfRangeException("offset");
            }
            if (size<0 || size>buffer.Length-offset) {
                throw new ArgumentOutOfRangeException("size");
            }


            Socket chkStreamSocket = m_StreamSocket;
            if (chkStreamSocket == null) {
                throw new IOException(SR.GetString(SR.net_io_readfailure, SR.GetString(SR.net_io_connectionclosed)));
            }

            try {
                int bytesTransferred = chkStreamSocket.Receive(buffer, offset, size, 0);
                return bytesTransferred;
            }
            catch (Exception exception) {
                if (exception is ThreadAbortException || exception is StackOverflowException || exception is OutOfMemoryException) {                                       
                    throw;
	            }

                //
                // some sort of error occured on the socket call,
                // set the SocketException as InnerException and throw
                //
                throw new IOException(SR.GetString(SR.net_io_readfailure, exception.Message), exception);
            }
#if DEBUG
            }
#endif
        }

        /*++
            Write - provide core Write functionality.

            Provide core write functionality. All we do is call through to the
            socket Send method..

            Input:

                Buffer  - Buffer to write from.
                Offset  - Offset into the buffer from where we'll start writing.
                Count   - Number of bytes to write.

            Returns:

                Number of bytes written. We'll throw an exception if we
                can't write everything. It's brutal, but there's no other
                way to indicate an error.
        --*/

        /// <devdoc>
        ///    <para>
        ///       Writes data to the stream..
        ///    </para>
        /// </devdoc>
        public override void Write(byte[] buffer, int offset, int size) {
#if DEBUG
            using (GlobalLog.SetThreadKind(ThreadKinds.User | ThreadKinds.Sync)) {
#endif
            bool canWrite = CanWrite; // Prevent race with Dispose.
            if (m_CleanedUp){
                throw new ObjectDisposedException(this.GetType().FullName);
            }
            if (!canWrite) {    
                throw new InvalidOperationException(SR.GetString(SR.net_readonlystream));
            }
            //
            // parameter validation
            //
            if (buffer==null) {
                throw new ArgumentNullException("buffer");
            }
            if (offset<0 || offset>buffer.Length) {
                throw new ArgumentOutOfRangeException("offset");
            }
            if (size<0 || size>buffer.Length-offset) {
                throw new ArgumentOutOfRangeException("size");
            }


            Socket chkStreamSocket = m_StreamSocket;
            if(chkStreamSocket == null) {
                throw new IOException(SR.GetString(SR.net_io_writefailure, SR.GetString(SR.net_io_connectionclosed)));
            }

            try {
                //
                // since the socket is in blocking mode this will always complete
                // after ALL the requested number of bytes was transferred
                //
                chkStreamSocket.Send(buffer, offset, size, SocketFlags.None);
            }
            catch (Exception exception) {
                if (exception is ThreadAbortException || exception is StackOverflowException || exception is OutOfMemoryException) {                                       
                    throw;
	            }

                //
                // some sort of error occured on the socket call,
                // set the SocketException as InnerException and throw
                //
                throw new IOException(SR.GetString(SR.net_io_writefailure, exception.Message), exception);
            }
#if DEBUG
            }
#endif
        }

        private int m_CloseTimeout = Socket.DefaultCloseTimeout; // 1 ms; -1 = respect linger options

        public void Close(int timeout) {
#if DEBUG
            using (GlobalLog.SetThreadKind(ThreadKinds.User | ThreadKinds.Sync)) {
#endif
            if (timeout < -1) {
                throw new ArgumentOutOfRangeException("timeout");
            }
            m_CloseTimeout = timeout;
            Close();
#if DEBUG
            }
#endif
        }

        private volatile bool m_CleanedUp = false;
        protected override void Dispose(bool disposing) {
#if DEBUG
            using (GlobalLog.SetThreadKind(ThreadKinds.User)) {
#endif
            // Mark this as disposed before changing anything else.
            bool cleanedUp = m_CleanedUp;
            m_CleanedUp = true; 
            if (!cleanedUp && disposing) {
                //
                // only resource we need to free is the network stream, since this
                // is based on the client socket, closing the stream will cause us
                // to flush the data to the network, close the stream and (in the
                // NetoworkStream code) close the socket as well.
                //
                if (m_StreamSocket!=null) {
                    m_Readable = false;
                    m_Writeable = false;
                    if (m_OwnsSocket) {
                        //
                        // if we own the Socket (false by default), close it
                        // ignoring possible exceptions (eg: the user told us
                        // that we own the Socket but it closed at some point of time,
                        // here we would get an ObjectDisposedException)
                        //
                        Socket chkStreamSocket = m_StreamSocket;
                        if (chkStreamSocket!=null) {
                            chkStreamSocket.InternalShutdown(SocketShutdown.Both);
                            chkStreamSocket.Close(m_CloseTimeout);
                        }
                    }
                }
            }
#if DEBUG
            }
#endif
            base.Dispose(disposing);
        }

        ~NetworkStream() {
#if DEBUG
            GlobalLog.SetThreadSource(ThreadKinds.Finalization);
           // using (GlobalLog.SetThreadKind(ThreadKinds.System | ThreadKinds.Async)) {
#endif
            Dispose(false);
#if DEBUG
           // }
#endif
        }

        /// <devdoc>
        ///    <para>
        ///       Indicates whether the stream is still connected
        ///    </para>
        /// </devdoc>
        internal bool Connected {
            get {
                Socket socket = m_StreamSocket;
                if (!m_CleanedUp && socket !=null && socket.Connected) {
                    return true;
                } else {
                    return false;
                }
            }
        }


        /*++
            BeginRead - provide async read functionality.

            This method provides async read functionality. All we do is
            call through to the underlying socket async read.

            Input:

                buffer  - Buffer to read into.
                offset  - Offset into the buffer where we're to read.
                size   - Number of bytes to read.

            Returns:

                An IASyncResult, representing the read.

        --*/

        /// <devdoc>
        ///    <para>
        ///       Begins an asychronous read from a stream.
        ///    </para>
        /// </devdoc>
        [HostProtection(ExternalThreading=true)]
        public override IAsyncResult BeginRead(byte[] buffer, int offset, int size, AsyncCallback callback, Object state) {
#if DEBUG
            using (GlobalLog.SetThreadKind(ThreadKinds.User | ThreadKinds.Async)) {
#endif
            bool canRead = CanRead; // Prevent race with Dispose.
            if (m_CleanedUp){
                throw new ObjectDisposedException(this.GetType().FullName);
            }
            if (!canRead) {    
                throw new InvalidOperationException(SR.GetString(SR.net_writeonlystream));
            }
            //
            // parameter validation
            //
            if (buffer==null) {
                throw new ArgumentNullException("buffer");
            }
            if (offset<0 || offset>buffer.Length) {
                throw new ArgumentOutOfRangeException("offset");
            }
            if (size<0 || size>buffer.Length-offset) {
                throw new ArgumentOutOfRangeException("size");
            }

            Socket chkStreamSocket = m_StreamSocket;
            if(chkStreamSocket == null) {
                throw new IOException(SR.GetString(SR.net_io_readfailure, SR.GetString(SR.net_io_connectionclosed)));
            }

            try {
                IAsyncResult asyncResult =
                    chkStreamSocket.BeginReceive(
                        buffer,
                        offset,
                        size,
                        SocketFlags.None,
                        callback,
                        state);

                return asyncResult;
            }
            catch (Exception exception) {
                if (exception is ThreadAbortException || exception is StackOverflowException || exception is OutOfMemoryException) {                                       
                    throw;
	            }

                //
                // some sort of error occured on the socket call,
                // set the SocketException as InnerException and throw
                //
                throw new IOException(SR.GetString(SR.net_io_readfailure, exception.Message), exception);
            }
#if DEBUG
            }
#endif
        }

        internal virtual IAsyncResult UnsafeBeginRead(byte[] buffer, int offset, int size, AsyncCallback callback, Object state)
        {
            bool canRead = CanRead; // Prevent race with Dispose.
            if (m_CleanedUp)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
            if (!canRead)
            {
                throw new InvalidOperationException(SR.GetString(SR.net_writeonlystream));
            }

            Socket chkStreamSocket = m_StreamSocket;
            if (chkStreamSocket == null)
            {
                throw new IOException(SR.GetString(SR.net_io_readfailure, SR.GetString(SR.net_io_connectionclosed)));
            }

            try
            {
                IAsyncResult asyncResult = chkStreamSocket.UnsafeBeginReceive(
                    buffer,
                    offset,
                    size,
                    SocketFlags.None,
                    callback,
                    state);

                return asyncResult;
            }
            catch (Exception exception)
            {
                if (NclUtilities.IsFatal(exception)) throw;

                //
                // some sort of error occured on the socket call,
                // set the SocketException as InnerException and throw
                //
                throw new IOException(SR.GetString(SR.net_io_readfailure, exception.Message), exception);
            }
        }

        /*++
            EndRead - handle the end of an async read.

            This method is called when an async read is completed. All we
            do is call through to the core socket EndReceive functionality.
            Input:

                buffer  - Buffer to read into.
                offset  - Offset into the buffer where we're to read.
                size   - Number of bytes to read.

            Returns:

                The number of bytes read. May throw an exception.

        --*/

        /// <devdoc>
        ///    <para>
        ///       Handle the end of an asynchronous read.
        ///    </para>
        /// </devdoc>
        public override int EndRead(IAsyncResult asyncResult) {
#if DEBUG
            using (GlobalLog.SetThreadKind(ThreadKinds.User)) {
#endif
            if (m_CleanedUp){
                throw new ObjectDisposedException(this.GetType().FullName);
            }

            //
            // parameter validation
            //
            if (asyncResult==null) {
                throw new ArgumentNullException("asyncResult");
            }

            Socket chkStreamSocket = m_StreamSocket;
            if(chkStreamSocket == null) {
                throw new IOException(SR.GetString(SR.net_io_readfailure, SR.GetString(SR.net_io_connectionclosed)));
            }

            try {
                int bytesTransferred = chkStreamSocket.EndReceive(asyncResult);
                return bytesTransferred;
            }
            catch (Exception exception) {
                if (exception is ThreadAbortException || exception is StackOverflowException || exception is OutOfMemoryException) {                                       
                    throw;
	            }

                //
                // some sort of error occured on the socket call,
                // set the SocketException as InnerException and throw
                //
                throw new IOException(SR.GetString(SR.net_io_readfailure, exception.Message), exception);
            }
#if DEBUG
            }
#endif
        }

        /*++
            BeginWrite - provide async write functionality.

            This method provides async write functionality. All we do is
            call through to the underlying socket async send.

            Input:

                buffer  - Buffer to write into.
                offset  - Offset into the buffer where we're to write.
                size   - Number of bytes to written.

            Returns:

                An IASyncResult, representing the write.

        --*/

        /// <devdoc>
        ///    <para>
        ///       Begins an asynchronous write to a stream.
        ///    </para>
        /// </devdoc>
        [HostProtection(ExternalThreading=true)]
        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int size, AsyncCallback callback, Object state) {
#if DEBUG
            using (GlobalLog.SetThreadKind(ThreadKinds.User | ThreadKinds.Async)) {
#endif
            bool canWrite = CanWrite; // Prevent race with Dispose.
            if (m_CleanedUp){
                throw new ObjectDisposedException(this.GetType().FullName);
            }
            if (!canWrite) {    
                throw new InvalidOperationException(SR.GetString(SR.net_readonlystream));
            }
            //
            // parameter validation
            //
            if (buffer==null) {
                throw new ArgumentNullException("buffer");
            }
            if (offset<0 || offset>buffer.Length) {
                throw new ArgumentOutOfRangeException("offset");
            }
            if (size<0 || size>buffer.Length-offset) {
                throw new ArgumentOutOfRangeException("size");
            }

            Socket chkStreamSocket = m_StreamSocket;
            if(chkStreamSocket == null) {
                throw new IOException(SR.GetString(SR.net_io_writefailure, SR.GetString(SR.net_io_connectionclosed)));
            }

            try {
                //
                // call BeginSend on the Socket.
                //
                IAsyncResult asyncResult =
                    chkStreamSocket.BeginSend(
                        buffer,
                        offset,
                        size,
                        SocketFlags.None,
                        callback,
                        state);

                return asyncResult;
            }
            catch (Exception exception) {
                if (exception is ThreadAbortException || exception is StackOverflowException || exception is OutOfMemoryException) {                                       
                    throw;
	            }

                //
                // some sort of error occured on the socket call,
                // set the SocketException as InnerException and throw
                //
                throw new IOException(SR.GetString(SR.net_io_writefailure, exception.Message), exception);
            }
#if DEBUG
            }
#endif
        }

               

        internal virtual IAsyncResult UnsafeBeginWrite(byte[] buffer, int offset, int size, AsyncCallback callback, Object state) {
#if DEBUG
            using (GlobalLog.SetThreadKind(ThreadKinds.User | ThreadKinds.Async)) {
#endif
                bool canWrite = CanWrite; // Prevent race with Dispose.
                if (m_CleanedUp){
                    throw new ObjectDisposedException(this.GetType().FullName);
                }
                
                if (!canWrite) {    
                    throw new InvalidOperationException(SR.GetString(SR.net_readonlystream));
                }
    
                Socket chkStreamSocket = m_StreamSocket;
                if(chkStreamSocket == null) {
                    throw new IOException(SR.GetString(SR.net_io_writefailure, SR.GetString(SR.net_io_connectionclosed)));
                }
    
                try {
                    //
                    // call BeginSend on the Socket.
                    //
                    IAsyncResult asyncResult =
                        chkStreamSocket.UnsafeBeginSend(
                            buffer,
                            offset,
                            size,
                            SocketFlags.None,
                            callback,
                            state);
    
                    return asyncResult;
                }
                catch (Exception exception) {
                    if (exception is ThreadAbortException || exception is StackOverflowException || exception is OutOfMemoryException) {                                       
                        throw;
    	            }
    
                    //
                    // some sort of error occured on the socket call,
                    // set the SocketException as InnerException and throw
                    //
                    throw new IOException(SR.GetString(SR.net_io_writefailure, exception.Message), exception);
                }
#if DEBUG
            }
#endif
        }



        /// <devdoc>
        ///    <para>
        ///       Handle the end of an asynchronous write.
        ///       This method is called when an async write is completed. All we
        ///       do is call through to the core socket EndSend functionality.
        ///       Returns:  The number of bytes read. May throw an exception.
        ///    </para>
        /// </devdoc>
        public override void EndWrite(IAsyncResult asyncResult) {
#if DEBUG
            using (GlobalLog.SetThreadKind(ThreadKinds.User)) {
#endif
            if (m_CleanedUp){
                throw new ObjectDisposedException(this.GetType().FullName);
            }

            //
            // parameter validation
            //
            if (asyncResult==null) {
                throw new ArgumentNullException("asyncResult");
            }

            Socket chkStreamSocket = m_StreamSocket;
            if(chkStreamSocket == null) {
                throw new IOException(SR.GetString(SR.net_io_writefailure, SR.GetString(SR.net_io_connectionclosed)));
            }

            try {
                chkStreamSocket.EndSend(asyncResult);
            }
            catch (Exception exception) {
                if (exception is ThreadAbortException || exception is StackOverflowException || exception is OutOfMemoryException) {                                       
                    throw;
	            }

                //
                // some sort of error occured on the socket call,
                // set the SocketException as InnerException and throw
                //
                throw new IOException(SR.GetString(SR.net_io_writefailure, exception.Message), exception);
            }
#if DEBUG
            }
#endif
        }


        /// <devdoc>
        ///    <para>
        ///       Performs a [....] Write of an array of buffers.
        ///    </para>
        /// </devdoc>
        internal virtual void MultipleWrite(BufferOffsetSize[] buffers)
        {
            GlobalLog.ThreadContract(ThreadKinds.Sync, "NetworkStream#" + ValidationHelper.HashString(this) + "::MultipleWrite");

            //
            // parameter validation
            //
            if (buffers == null) {
                throw new ArgumentNullException("buffers");
            }

            Socket chkStreamSocket = m_StreamSocket;
            if(chkStreamSocket == null) {
                throw new IOException(SR.GetString(SR.net_io_writefailure, SR.GetString(SR.net_io_connectionclosed)));
            }

            try {

                chkStreamSocket.MultipleSend(
                    buffers,
                    SocketFlags.None);

            }
            catch (Exception exception) {
                if (exception is ThreadAbortException || exception is StackOverflowException || exception is OutOfMemoryException) {                                       
                    throw;
	            }

                //
                // some sort of error occured on the socket call,
                // set the SocketException as InnerException and throw
                //
                throw new IOException(SR.GetString(SR.net_io_writefailure, exception.Message), exception);
            }
        }


        /// <devdoc>
        ///    <para>
        ///       Starts off an async Write of an array of buffers.
        ///    </para>
        /// </devdoc>
        internal virtual IAsyncResult BeginMultipleWrite(
            BufferOffsetSize[] buffers,
            AsyncCallback callback,
            Object state)
        {
#if DEBUG
            GlobalLog.ThreadContract(ThreadKinds.Unknown, "NetworkStream#" + ValidationHelper.HashString(this) + "::BeginMultipleWrite");
            using (GlobalLog.SetThreadKind(ThreadKinds.Async)) {
#endif

            //
            // parameter validation
            //
            if (buffers == null) {
                throw new ArgumentNullException("buffers");
            }

            Socket chkStreamSocket = m_StreamSocket;
            if(chkStreamSocket == null) {
                throw new IOException(SR.GetString(SR.net_io_writefailure, SR.GetString(SR.net_io_connectionclosed)));
            }

            try {

                //
                // call BeginMultipleSend on the Socket.
                //
                IAsyncResult asyncResult =
                    chkStreamSocket.BeginMultipleSend(
                        buffers,
                        SocketFlags.None,
                        callback,
                        state);

                return asyncResult;
            }
            catch (Exception exception) {

                if (exception is ThreadAbortException || exception is StackOverflowException || exception is OutOfMemoryException) {                                       
                    throw;
	            }

                //
                // some sort of error occured on the socket call,
                // set the SocketException as InnerException and throw
                //
                throw new IOException(SR.GetString(SR.net_io_writefailure, exception.Message), exception);
            }
#if DEBUG
            }
#endif
        }


        internal virtual IAsyncResult UnsafeBeginMultipleWrite(
            BufferOffsetSize[] buffers,
            AsyncCallback callback,
            Object state)
        {
#if DEBUG
            GlobalLog.ThreadContract(ThreadKinds.Unknown, "NetworkStream#" + ValidationHelper.HashString(this) + "::BeginMultipleWrite");
            using (GlobalLog.SetThreadKind(ThreadKinds.Async)) {
#endif

            //
            // parameter validation
            //
            if (buffers == null) {
                throw new ArgumentNullException("buffers");
            }

            Socket chkStreamSocket = m_StreamSocket;
            if(chkStreamSocket == null) {
                throw new IOException(SR.GetString(SR.net_io_writefailure, SR.GetString(SR.net_io_connectionclosed)));
            }

            try {

                //
                // call BeginMultipleSend on the Socket.
                //
                IAsyncResult asyncResult =
                    chkStreamSocket.UnsafeBeginMultipleSend(
                        buffers,
                        SocketFlags.None,
                        callback,
                        state);

                return asyncResult;
            }
            catch (Exception exception) {

                if (exception is ThreadAbortException || exception is StackOverflowException || exception is OutOfMemoryException) {                                       
                    throw;
	            }

                //
                // some sort of error occured on the socket call,
                // set the SocketException as InnerException and throw
                //
                throw new IOException(SR.GetString(SR.net_io_writefailure, exception.Message), exception);
            }
#if DEBUG
            }
#endif
        }


        internal virtual void EndMultipleWrite(IAsyncResult asyncResult) {
            GlobalLog.ThreadContract(ThreadKinds.Unknown, "NetworkStream#" + ValidationHelper.HashString(this) + "::EndMultipleWrite");

            //
            // parameter validation
            //
            if (asyncResult == null) {
                throw new ArgumentNullException("asyncResult");
            }

            Socket chkStreamSocket = m_StreamSocket;
            if(chkStreamSocket == null) {
                throw new IOException(SR.GetString(SR.net_io_writefailure, SR.GetString(SR.net_io_connectionclosed)));
            }

            try {
                chkStreamSocket.EndMultipleSend(asyncResult);
            }
            catch (Exception exception) {
                if (exception is ThreadAbortException || exception is StackOverflowException || exception is OutOfMemoryException) {                                       
                    throw;
	            }

                //
                // some sort of error occured on the socket call,
                // set the SocketException as InnerException and throw
                //
                throw new IOException(SR.GetString(SR.net_io_writefailure, exception.Message), exception);
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Flushes data from the stream.  This is meaningless for us, so it does nothing.
        ///    </para>
        /// </devdoc>
        public override void Flush() {
        }

        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <devdoc>
        ///    <para>
        ///       Sets the length of the stream. Always throws <see cref='NotSupportedException'/>
        ///    </para>
        /// </devdoc>
        public override void SetLength(long value) {
            throw new NotSupportedException(SR.GetString(SR.net_noseek));
        }

        int m_CurrentReadTimeout = -1;
        int m_CurrentWriteTimeout = -1;
        internal void SetSocketTimeoutOption(SocketShutdown mode, int timeout, bool silent) {
            GlobalLog.Print("NetworkStream#" + ValidationHelper.HashString(this) + "::SetSocketTimeoutOption() mode:" + mode + " silent:" + silent + " timeout:" + timeout + " m_CurrentReadTimeout:" + m_CurrentReadTimeout + " m_CurrentWriteTimeout:" + m_CurrentWriteTimeout);
            GlobalLog.ThreadContract(ThreadKinds.Unknown, "NetworkStream#" + ValidationHelper.HashString(this) + "::SetSocketTimeoutOption");

            if (timeout < 0) {
                timeout = 0; // -1 becomes 0 for the winsock stack
            }

            Socket chkStreamSocket = m_StreamSocket;
            if (chkStreamSocket==null) {
                return;
            }
            if (mode==SocketShutdown.Send || mode==SocketShutdown.Both) {
                if (timeout!=m_CurrentWriteTimeout) {
                    chkStreamSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, timeout, silent);
                    m_CurrentWriteTimeout = timeout;
                }
            }
            if (mode==SocketShutdown.Receive || mode==SocketShutdown.Both) {
                if (timeout!=m_CurrentReadTimeout) {
                    chkStreamSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, timeout, silent);
                    m_CurrentReadTimeout = timeout;
                }
            }
        }

#if TRAVE
        [System.Diagnostics.Conditional("TRAVE")]
        internal void DebugMembers() {
            if (m_StreamSocket != null) {
                GlobalLog.Print("m_StreamSocket:");
                m_StreamSocket.DebugMembers();
            }
        }
#endif
    }
}
