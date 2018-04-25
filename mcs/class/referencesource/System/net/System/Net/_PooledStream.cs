//------------------------------------------------------------------------------
// <copyright file="PooledStream.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net
{
    using System;
    using System.Net.Sockets;
    using System.IO;
    using System.Diagnostics;
    using System.Security.Permissions;
    using System.Threading;
    using System.Threading.Tasks;

    internal class PooledStream : Stream {
        // managed pooling lifetime controls
        private bool                m_CheckLifetime;     // true when the connection is only to live for a specific timespan
        private TimeSpan            m_Lifetime;          // the timespan the connection is to live for
        private DateTime            m_CreateTime;        // when the connection was created.
        private bool                m_ConnectionIsDoomed;// true when the connection should no longer be used.

        // managed pooling
        private ConnectionPool      m_ConnectionPool;   // the pooler that the connection came from
        private WeakReference       m_Owner;            // the owning object, when not in the pool.
        private int                 m_PooledCount;      // the number of times this object has been pushed into the pool less the number of times it's been popped (0 == inPool)

        // connection info
        private bool                m_Initalizing;      // true while we're creating the stream
        private IPAddress           m_ServerAddress;    // IP address of server we're connected to

        private NetworkStream       m_NetworkStream;    // internal stream for socket

        private Socket              m_AbortSocket;      // in abort scenarios, used to abort connect
        private Socket              m_AbortSocket6;     // in abort scenarios, used to abort connect
        private bool                m_JustConnected;

        internal PooledStream(object owner) : base() {  // non-pooled constructor
            m_Owner = new WeakReference(owner);
            m_PooledCount = -1;
            m_Initalizing = true;
            m_NetworkStream = new NetworkStream();
            m_CreateTime = DateTime.UtcNow;
        }

        internal PooledStream(ConnectionPool connectionPool, TimeSpan lifetime, bool checkLifetime) : base () { // pooled constructor
            m_ConnectionPool = connectionPool;
            m_Lifetime = lifetime;
            m_CheckLifetime = checkLifetime;
            m_Initalizing = true;
            m_NetworkStream = new NetworkStream();
            m_CreateTime = DateTime.UtcNow;
        }


        internal bool JustConnected {
            get{
               if (m_JustConnected)
               {
                   m_JustConnected = false;
                   return true;
               }
               return false;
            }
        }

        internal IPAddress ServerAddress {
            get {
                return m_ServerAddress;
            }
        }

        internal bool IsInitalizing {
            get {
                return m_Initalizing;
            }
        }

        internal bool CanBePooled {
            get {
                if (m_Initalizing) {
                    GlobalLog.Print("PooledStream#" + ValidationHelper.HashString(this) + "::CanBePooled " + "init: true");
                    return true;
                }
                if (!m_NetworkStream.Connected) {
                    GlobalLog.Print("PooledStream#" + ValidationHelper.HashString(this) + "::CanBePooled " + "not-connected: false");
                    return false;
                }

                WeakReference weakref = m_Owner;
                bool flag = (!m_ConnectionIsDoomed && ((null == weakref) || !weakref.IsAlive));
                GlobalLog.Print("PooledStream#" + ValidationHelper.HashString(this) + "::CanBePooled " + "flag: " + flag.ToString());
                return (flag);
            }
            set {
                m_ConnectionIsDoomed |= !value;
            }
        }

        internal bool IsEmancipated {
            get {
                WeakReference owner = m_Owner;
                bool value = (0 >= m_PooledCount) && (null == owner || !owner.IsAlive);
                return value;
            }
        }

        internal object Owner {
            // We use a weak reference to the owning object so we can identify when
            // it has been garbage collected without thowing exceptions.
            get {
                WeakReference weakref = m_Owner;
                if ((null != weakref) && weakref.IsAlive) {
                    return weakref.Target;
                }
                return null;
            }
            set{
                lock(this){
                    if(null != m_Owner){
                        m_Owner.Target = value;
                    }
                }
            }
        }

        internal ConnectionPool Pool {
            get { return m_ConnectionPool; }
        }

        internal virtual ServicePoint ServicePoint {
            get { return Pool.ServicePoint; }
        }

        private GeneralAsyncDelegate m_AsyncCallback;

        internal bool Activate(object owningObject, GeneralAsyncDelegate asyncCallback)
        {
            return Activate(owningObject, asyncCallback != null, asyncCallback);
        }

        protected bool Activate(object owningObject, bool async, GeneralAsyncDelegate asyncCallback)
        {
            GlobalLog.Assert(owningObject == Owner || Owner == null, "PooledStream::Activate|Owner is not the same as expected.");
            try {
                if (m_Initalizing) {
                    IPAddress address = null;
                    m_AsyncCallback = asyncCallback;
                    Socket socket = ServicePoint.GetConnection(this, owningObject, async, out address, ref m_AbortSocket, ref m_AbortSocket6);
                    if (socket != null) {
                        if (Logging.On) {
                            Logging.PrintInfo(Logging.Web, this, 
                                SR.GetString(SR.net_log_socket_connected, socket.LocalEndPoint, 
                                    socket.RemoteEndPoint));
                        }
                        m_NetworkStream.InitNetworkStream(socket, FileAccess.ReadWrite);
                        m_ServerAddress = address;
                        m_Initalizing = false;
                        m_JustConnected = true;
                        m_AbortSocket = null;
                        m_AbortSocket6 = null;
                        return true;
                    }
                    return false;
                }
                else if (async && asyncCallback != null)
                {
                    asyncCallback(owningObject, this);
                }
                return true;
            } catch {
                m_Initalizing = false;
                throw;
            }
        }

        internal void Deactivate() {
            // Called when the connection is about to be placed back into the pool; this

            m_AsyncCallback = null;

            if (!m_ConnectionIsDoomed && m_CheckLifetime) {
                // check lifetime here - as a side effect it will doom connection if
                // it's lifetime has elapsed
                CheckLifetime();
            }
        }

        internal virtual void ConnectionCallback(object owningObject, Exception e, Socket socket, IPAddress address)
        {
            GlobalLog.Assert(owningObject == Owner || Owner == null, "PooledStream::ConnectionCallback|Owner is not the same as expected.");
            object result = null;
            if (e != null) {
                m_Initalizing = false;
                result = e;
            } else {
                try {
                    if (Logging.On) {
                        Logging.PrintInfo(Logging.Web, this, 
                            SR.GetString(SR.net_log_socket_connected, socket.LocalEndPoint, socket.RemoteEndPoint));
                    }
                    m_NetworkStream.InitNetworkStream(socket, FileAccess.ReadWrite);
                    result = this;
                }
                catch (Exception ex)
                {
                    if (NclUtilities.IsFatal(ex))
                        throw;
                    result = ex;
                }
                m_ServerAddress = address;
                m_Initalizing = false;
                m_JustConnected = true;
            }
            if (m_AsyncCallback != null) {
                m_AsyncCallback(owningObject, result);
            }
            m_AbortSocket = null;
            m_AbortSocket6 = null;
        }

        protected void CheckLifetime() {
            bool okay = !m_ConnectionIsDoomed;
            if (okay) {
                // Returns whether or not this object's lifetime has had expired.
                // True means the object is still good, false if it has timed out.

                // obtain current time
                DateTime utcNow = DateTime.UtcNow;

                // obtain timespan
                TimeSpan timeSpan = utcNow.Subtract(m_CreateTime);

                // compare timeSpan with lifetime, if equal or less,
                // designate this object to be killed
                m_ConnectionIsDoomed = (0 < TimeSpan.Compare(m_Lifetime, timeSpan));
            }
        }

        /// <devdoc>
        ///    <para>Updates the lifetime of the time for this stream to live</para>
        /// </devdoc>
        internal void UpdateLifetime() {
            int timeout = ServicePoint.ConnectionLeaseTimeout;
            TimeSpan connectionLifetime;

            if (timeout == System.Threading.Timeout.Infinite) {
                connectionLifetime = TimeSpan.MaxValue;
                m_CheckLifetime = false;
            } else {
                connectionLifetime = new TimeSpan(0, 0, 0, 0, timeout);
                m_CheckLifetime = true;
            }

            if (connectionLifetime != m_Lifetime) {
                m_Lifetime = connectionLifetime;
            }
        }

        internal void PrePush(object expectedOwner) {
            lock (this) {
                //3 // The following tests are retail assertions of things we can't allow to happen.
                if (null == expectedOwner) {
                    if (null != m_Owner && null != m_Owner.Target)
                        throw new InternalException();      // new unpooled object has an owner
                } else {
                    if (null == m_Owner || m_Owner.Target != expectedOwner)
                        throw new InternalException();      // unpooled object has incorrect owner
                }

                m_PooledCount++;

                if (1 != m_PooledCount)
                    throw new InternalException();          // pushing object onto stack a second time

                if (null != m_Owner)
                    m_Owner.Target = null;
            }
        }

        internal void PostPop (object newOwner) {
            GlobalLog.Assert(!IsEmancipated, "Pooled object not in pool.");
            GlobalLog.Assert(CanBePooled, "Pooled object is not poolable.");


            lock (this) {
                if (null == m_Owner)
                    m_Owner = new WeakReference(newOwner);
                else {
                    if (null != m_Owner.Target)
                        throw new InternalException();        // pooled connection already has an owner!

                    m_Owner.Target = newOwner;
                }

                m_PooledCount--;

                if (null != Pool) {
                    if (0 != m_PooledCount)
                        throw new InternalException();  // popping object off stack with multiple pooledCount
                } else {
                    if (-1 != m_PooledCount)
                        throw new InternalException();  // popping object off stack with multiple pooledCount
                }
            }
        }

        /// <devdoc>
        ///    <para>True if we're using a TlsStream</para>
        /// </devdoc>
        protected bool UsingSecureStream {
            get {
#if !FEATURE_PAL
                return (m_NetworkStream is TlsStream);
#else
                return false;
#endif // !FEATURE_PAL
            }
        }

        /// <devdoc>
        ///    <para>Allows inherited objects to modify NetworkStream</para>
        /// </devdoc>
        internal NetworkStream NetworkStream {
            get {
                return m_NetworkStream;
            }
            set {
                m_Initalizing = false;
                m_NetworkStream = value;
            }
        }

        /// <devdoc>
        ///    <para>Gives the socket for internal use.</para>
        /// </devdoc>
        protected Socket Socket {
            get {
                return m_NetworkStream.InternalSocket;
            }
        }

        /// <devdoc>
        ///    <para>Indicates that data can be read from the stream.
        /// </devdoc>
        public override bool CanRead {
            get {
                return m_NetworkStream.CanRead;
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
                return m_NetworkStream.CanWrite;
            }
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

        /*
        // Consider removing.
        /// <devdoc>
        ///    <para>Indicates the avail bytes</para>
        /// </devdoc>
        public bool DataAvailable {
            get {
                return m_NetworkStream.DataAvailable;
            }
        }
        */

        /// <devdoc>
        ///    <para>Seeks a specific position in the stream.</para>
        /// </devdoc>
        public override long Seek(long offset, SeekOrigin origin) {
            return m_NetworkStream.Seek(offset, origin);
        }


        /// <devdoc>
        ///    <para> Reads data from the stream. </para>
        /// </devdoc>
        public override int Read(byte[] buffer, int offset, int size) {
            int result = m_NetworkStream.Read(buffer, offset, size);
            GlobalLog.Dump(buffer, offset, result);
            return result;
        }


        /// <devdoc>
        ///    <para>Writes data to the stream.</para>
        /// </devdoc>
        public override void Write(byte[] buffer, int offset, int size) {
            GlobalLog.Dump(buffer, offset, size);
            m_NetworkStream.Write(buffer, offset, size);
        }

        /// <devdoc>
        ///    <para>Writes multiple buffers at once</para>
        /// </devdoc>
        internal void MultipleWrite(BufferOffsetSize[] buffers) {
#if TRAVE
            for (int i = 0; i < buffers.Length; ++i)
            {
                GlobalLog.Dump(buffers[i].Buffer, buffers[i].Offset, buffers[i].Size);
            }
#endif
            m_NetworkStream.MultipleWrite(buffers);
        }

        /// <devdoc>
        ///    <para>
        ///       Closes the stream, and then closes the underlying socket.
        ///    </para>
        /// </devdoc>
        protected override void Dispose(bool disposing) {
            try {
                if (disposing) {
                    m_Owner = null;
                    m_ConnectionIsDoomed = true;
                    // no timeout so that socket will close gracefully
                    CloseSocket();
                }
            }
            finally {
                base.Dispose(disposing);
            }
        }

        internal void CloseSocket() {
            Socket socket = m_AbortSocket;
            Socket socket6 = m_AbortSocket6;

            m_NetworkStream.Close();
            if (socket != null) {
                socket.Close();
            }
            if (socket6 != null) {
                socket6.Close();
            }
        }

        public void Close(int timeout) {
            Socket socket = m_AbortSocket;
            Socket socket6 = m_AbortSocket6;
            m_NetworkStream.Close(timeout);
            if (socket != null) {
                socket.Close(timeout);
            }
            if (socket6 != null) {
                socket6.Close(timeout);
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Begins an asychronous read from a stream.
        ///    </para>
        /// </devdoc>
        [HostProtection(ExternalThreading=true)]
        public override IAsyncResult BeginRead(byte[] buffer, int offset, int size, AsyncCallback callback, Object state) {
            return m_NetworkStream.BeginRead(buffer, offset, size, callback, state);
        }

        internal virtual IAsyncResult UnsafeBeginRead(byte[] buffer, int offset, int size, AsyncCallback callback, Object state)
        {
            return m_NetworkStream.UnsafeBeginRead(buffer, offset, size, callback, state);
        }

        /// <devdoc>
        ///    <para>
        ///       Handle the end of an asynchronous read.
        ///    </para>
        /// </devdoc>
        public override int EndRead(IAsyncResult asyncResult) {
            // only caller can recover the debug dump for the read result
            return m_NetworkStream.EndRead(asyncResult);
        }

        /// <devdoc>
        ///    <para>
        ///       Begins an asynchronous write to a stream.
        ///    </para>
        /// </devdoc>
        [HostProtection(ExternalThreading=true)]
        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int size, AsyncCallback callback, Object state) {
            GlobalLog.Dump(buffer, offset, size);
            return m_NetworkStream.BeginWrite(buffer, offset, size, callback, state);
        }

        internal virtual IAsyncResult UnsafeBeginWrite(byte[] buffer, int offset, int size, AsyncCallback callback, Object state) {
            GlobalLog.Dump(buffer, offset, size);
            return m_NetworkStream.UnsafeBeginWrite(buffer, offset, size, callback, state);
        }


        /// <devdoc>
        ///    <para>
        ///       Handle the end of an asynchronous write.
        ///    </para>
        /// </devdoc>
        public override void EndWrite(IAsyncResult asyncResult) {
            m_NetworkStream.EndWrite(asyncResult);
        }

        /// <devdoc>
        ///    <para>
        ///       Begins an asynchronous write to a stream.
        ///    </para>
        /// </devdoc>
        [HostProtection(ExternalThreading=true)]
        internal IAsyncResult BeginMultipleWrite(BufferOffsetSize[] buffers, AsyncCallback callback, object state) {
#if TRAVE
            for (int i = 0; i < buffers.Length; ++i)
            {
                GlobalLog.Dump(buffers[i].Buffer, buffers[i].Offset, buffers[i].Size);
            }
#endif
            return m_NetworkStream.BeginMultipleWrite(buffers, callback, state);
        }

        /*
        // Consider removing.
        internal IAsyncResult UnsafeBeginMultipleWrite(BufferOffsetSize[] buffers, AsyncCallback callback, object state) {
#if TRAVE
            for (int i = 0; i < buffers.Length; ++i)
            {
                GlobalLog.Dump(buffers[i].Buffer, buffers[i].Offset, buffers[i].Size);
            }
#endif
            return m_NetworkStream.UnsafeBeginMultipleWrite(buffers, callback, state);
        }
        */


        /// <devdoc>
        ///    <para>
        ///       Handle the end of an asynchronous write.
        ///    </para>
        /// </devdoc>
        internal void EndMultipleWrite(IAsyncResult asyncResult) {
            m_NetworkStream.EndMultipleWrite(asyncResult);
        }

        /// <devdoc>
        ///    <para>
        ///       Flushes data from the stream.
        ///    </para>
        /// </devdoc>
        public override void Flush() {
            m_NetworkStream.Flush();
        }

        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            return m_NetworkStream.FlushAsync(cancellationToken);
        }

        /// <devdoc>
        ///    <para>Sets the length of the stream.</para>
        /// </devdoc>
        public override void SetLength(long value) {
            m_NetworkStream.SetLength(value);
        }

        internal void SetSocketTimeoutOption(SocketShutdown mode, int timeout, bool silent) {
            m_NetworkStream.SetSocketTimeoutOption(mode, timeout, silent);
        }

        internal bool Poll(int microSeconds, SelectMode mode) {
            return m_NetworkStream.Poll(microSeconds, mode);
        }

        internal bool PollRead() {
            return m_NetworkStream.PollRead();
        }
    }
} // System.Net


