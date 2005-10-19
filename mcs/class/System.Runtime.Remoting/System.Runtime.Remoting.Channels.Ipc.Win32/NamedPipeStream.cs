//
// System.Runtime.Remoting.Channels.Ipc.Win32.NamedPipeStream.cs
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
using System.Runtime.Remoting.Messaging;

namespace System.Runtime.Remoting.Channels.Ipc.Win32
{
    /// <summary>
    /// Provides the underlying stream of data for local Named Pipe access.
    /// </summary>
    internal class NamedPipeStream : Stream
    {
        readonly NamedPipeSocket socket;
        readonly bool ownsSocket;

        /// <summary>
        /// Creates a new instance of the NamedPipeStream class for the specified NamedPipeSocket.
        /// </summary>
        /// <param name="socket"></param>
        public NamedPipeStream(NamedPipeSocket socket, bool ownsSocket)
        {
            this.socket = socket;
            this.ownsSocket = ownsSocket;
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override long Length
        {
            get { throw new NotSupportedException(); }
        }

        public override long Position
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Close() 
        {
            if (ownsSocket) socket.Close();
        }

        public override void Flush() 
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return socket.Receive(buffer, offset, count);
        }

        delegate int ReadMethod(byte[] buffer, int offset, int count);

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count,
            AsyncCallback callback, object state)
        {
            return new ReadMethod(Read).BeginInvoke(buffer, offset, count, callback, state);
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            AsyncResult ar = asyncResult as AsyncResult;
            return ((ReadMethod)ar.AsyncDelegate).EndInvoke(ar);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            int written = socket.Send(buffer, offset, count);
            if (written != count)
                throw new IOException("Cannot write data");
        }
        
        delegate void WriteMethod(byte[] buffer, int offset, int count);

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count,
            AsyncCallback callback, object state)
        {
            return new WriteMethod(Write).BeginInvoke(buffer, offset, count, callback, state);
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            AsyncResult ar = asyncResult as AsyncResult;
            ((WriteMethod)ar.AsyncDelegate).EndInvoke(asyncResult);
        }

    }
}

#endif
