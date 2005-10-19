//
// System.Runtime.Remoting.Channels.Ipc.Win32.NamedPipeSocket.cs
//
// Author: Robert Jordan (robertj@gmx.net)
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

#if NET_2_0

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Text;

namespace System.Runtime.Remoting.Channels.Ipc.Win32
{
    /// <summary>
    /// Implements a local Named Pipe socket.
    /// </summary>
    internal class NamedPipeSocket : IDisposable
    {
        IntPtr hPipe;

        /// <summary>
        /// Creates a new socket instance form the specified local Named Pipe handle.
        /// </summary>
        /// <param name="hPipe">The handle.</param>
        internal NamedPipeSocket(IntPtr hPipe)
        {
            this.hPipe = hPipe;
            this.info = new NamedPipeSocketInfo(hPipe);
        }

        ~NamedPipeSocket() 
        {
            ((IDisposable)this).Dispose();
        }

        /// <summary>
        /// Gets the NamedPipeSocketInfo of this instance.
        /// </summary>
        /// <returns></returns>
        public NamedPipeSocketInfo Info
        {
            get 
            {
                return info;
            }
        }

        NamedPipeSocketInfo info;

        /// <summary>
        /// Closes the socket.
        /// </summary>
        public void Close() 
        {
            ((IDisposable)this).Dispose();
        }

        /// <summary>
        /// Disposes the object.
        /// </summary>
        void IDisposable.Dispose()
        {
            if (hPipe != IntPtr.Zero) 
            {
                try 
                {
                    // disconnect the pipe
                    if (Info.IsServer) 
                    {
                        NamedPipeHelper.FlushFileBuffers(hPipe);
                        NamedPipeHelper.DisconnectNamedPipe(hPipe);
                    }
                }
                catch (NamedPipeException) 
                {
                }

                NamedPipeHelper.CloseHandle(hPipe);
                hPipe = IntPtr.Zero;
                GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        /// Returns the stream used to send and receive data.
        /// </summary>
        /// <returns></returns>
        public Stream GetStream() 
        {
            if (hPipe == IntPtr.Zero)
                throw new ObjectDisposedException(GetType().FullName);

            return stream == null
                ? stream = new NamedPipeStream(this, false)
                : stream;
        }

        Stream stream;

        /// <summary>
        /// Returns the stream used to send and receive data. The stream disposes
        /// the socket on close.
        /// </summary>
        /// <returns></returns>
        public Stream GetClosingStream() 
        {
            if (hPipe == IntPtr.Zero)
                throw new ObjectDisposedException(GetType().FullName);
            
            return new NamedPipeStream(this, true);
        }

        /// <summary>
        /// Flushes the socket.
        /// </summary>
        public void Flush() 
        {
            if (hPipe == IntPtr.Zero)
                throw new ObjectDisposedException(GetType().FullName);

            NamedPipeHelper.FlushFileBuffers(hPipe);
        }

        /// <summary>
        /// Receives the specified number of bytes from a socket into
        /// the specified offset position of the receive buffer.
        /// </summary>
        /// <param name="buffer">An array of type Byte that is the storage
        /// location for received data.</param>
        /// <param name="offset">The location in buffer to store the received data.</param>
        /// <param name="count">The number of bytes to receive.</param>
        /// <returns>The number of bytes received.</returns>
        public int Receive(byte[] buffer, int offset, int count) 
        {
            if (hPipe == IntPtr.Zero)
                throw new ObjectDisposedException(GetType().FullName);
            if (buffer == null)
                throw new ArgumentNullException("buffer");
            if (offset < 0 || count < 0)
                throw new ArgumentOutOfRangeException("offset and/or count");
            if (buffer.Length - offset < count)
                throw new ArgumentException();

            uint read = 0;

            GCHandle gch  = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            try 
            {
                bool res = NamedPipeHelper.ReadFile(
                    hPipe,
                    Marshal.UnsafeAddrOfPinnedArrayElement(buffer, offset),
                    (uint) count,
                    out read,
                    IntPtr.Zero
                    );

                if (!res && read == 0) throw new NamedPipeException();

                return (int) read;
            }
            finally 
            {
                gch.Free();
            }
        }

        delegate int ReceiveMethod(byte[] buffer, int offset, int count);

        public IAsyncResult BeginReceive(byte[] buffer, int offset, int count,
            AsyncCallback callback, object state)
        {
            return new ReceiveMethod(Receive).BeginInvoke(buffer, offset, count, callback, state);
        }

        public int EndReceive(IAsyncResult asyncResult) 
        {
            AsyncResult ar = asyncResult as AsyncResult;
            return ((ReceiveMethod)ar.AsyncDelegate).EndInvoke(asyncResult);
        }

        /// <summary>
        /// Sends the specified number of bytes of data to a connected socket,
        /// starting at the specified offset.
        /// </summary>
        /// <param name="buffer">An array of type Byte that contains the data to be sent.</param>
        /// <param name="offset">The position in the data buffer at which to begin sending data. </param>
        /// <param name="count">The number of bytes to send.</param>
        /// <returns>The number of bytes sent.</returns>
        public int Send(byte[] buffer, int offset, int count) 
        {
            if (hPipe == IntPtr.Zero)
                throw new ObjectDisposedException(GetType().FullName);
            if (buffer == null)
                throw new ArgumentNullException("buffer");
            if (offset < 0 || count < 0)
                throw new ArgumentOutOfRangeException("offset and/or count");
            if (buffer.Length - offset < count)
                throw new ArgumentException();

            uint written = 0;

            GCHandle gch  = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            try 
            {
                bool res = NamedPipeHelper.WriteFile(
                    hPipe,
                    Marshal.UnsafeAddrOfPinnedArrayElement(buffer, offset),
                    (uint) count,
                    out written,
                    IntPtr.Zero
                    );

                if (!res) throw new NamedPipeException();
                return (int) written;
            }
            finally 
            {
                gch.Free();
            }
        }

        delegate int SendMethod(byte[] buffer, int offset, int count);

        public IAsyncResult BeginSend(byte[] buffer, int offset, int count,
            AsyncCallback callback, object state)
        {
            return new SendMethod(Send).BeginInvoke(buffer, offset, count, callback, state);
        }

        public int EndSend(IAsyncResult asyncResult) 
        {
            AsyncResult ar = asyncResult as AsyncResult;
            return ((SendMethod)ar.AsyncDelegate).EndInvoke(asyncResult);
        }

        /// <summary>
        /// Returns the current NamedPipeSocketState of this instance.
        /// </summary>
        /// <returns></returns>
        public NamedPipeSocketState GetSocketState() 
        {
            if (hPipe == IntPtr.Zero)
                throw new ObjectDisposedException(GetType().FullName);

            return new NamedPipeSocketState(hPipe);
        }

        /// <summary>
        /// Impersonates the client.
        /// </summary>
        public void Impersonate() 
        {
            if (hPipe == IntPtr.Zero)
                throw new ObjectDisposedException(GetType().FullName);

            if (Info.IsServer)
                if (!NamedPipeHelper.ImpersonateNamedPipeClient(hPipe))
                    throw new NamedPipeException();
        }

        /// <summary>
        /// Reverts the impersonation.
        /// </summary>
        public static bool RevertToSelf() 
        {
            return NamedPipeHelper.RevertToSelf();
        }

    }


    /// <summary>
    /// Represents local Named Pipe informations.
    /// </summary>
    internal class NamedPipeSocketInfo
    {
        public readonly int Flags;
        public readonly int OutBufferSize;
        public readonly int InBufferSize;
        public readonly int MaxInstances;

        public bool IsServer 
        {
            get 
            {
                return (Flags & NamedPipeHelper.PIPE_SERVER_END) != 0;
            }
        }

        internal NamedPipeSocketInfo(IntPtr hPipe) 
        {
            bool res = NamedPipeHelper.GetNamedPipeInfo(
                hPipe,
                out Flags,
                out OutBufferSize,
                out InBufferSize,
                out MaxInstances
                );
            
            if (!res) 
            {
                throw new NamedPipeException();
            }
        }
    }


    /// <summary>
    /// Represents local Named Pipe state informations.
    /// </summary>
    internal class NamedPipeSocketState 
    {
        public readonly int State;
        public readonly int CurrentInstances;
        public readonly int MaxCollectionCount;
        public readonly int CollectDataTimeout;
        public readonly string UserName;

        internal NamedPipeSocketState(IntPtr hPipe) 
        {
            StringBuilder userName = new StringBuilder(256);
            bool res = NamedPipeHelper.GetNamedPipeHandleState(
                hPipe,
                out State,
                out CurrentInstances,
                out MaxCollectionCount,
                out CollectDataTimeout,
                userName,
                userName.Capacity
                );
            
            if (res) 
            {
                UserName = userName.ToString();
            }
            else 
            {
                throw new NamedPipeException();
            }
        }
    }
}

#endif
