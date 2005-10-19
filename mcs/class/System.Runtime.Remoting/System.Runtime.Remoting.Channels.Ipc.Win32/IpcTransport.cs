//
// System.Runtime.Remoting.Channels.Ipc.Win32.IpcTransport.cs
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
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization.Formatters.Binary;

namespace System.Runtime.Remoting.Channels.Ipc.Win32
{
    /// <summary>
    /// IPC transport helper
    /// </summary>
    internal class IpcTransport
    {
        readonly NamedPipeSocket socket;
        readonly BinaryFormatter formatter = new BinaryFormatter();

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="socket">The named pipe.</param>
        public IpcTransport(NamedPipeSocket socket)
        {
            this.socket = socket;
        }

        /// <summary>
        /// Writes a request.
        /// </summary>
        /// <param name="header"></param>
        /// <param name="requestStream"></param>
        public void Write(ITransportHeaders header, Stream requestStream) 
        {
            Stream bs = socket.GetStream();

            MemoryStream m = new MemoryStream();
            formatter.Serialize(m, header);
            m.Position = 0;
            byte[] bytes = BitConverter.GetBytes((int)m.Length);
            bs.Write(bytes, 0, bytes.Length);
            m.WriteTo(bs);

            try 
            {
                bytes = BitConverter.GetBytes((int)requestStream.Length);
                bs.Write(bytes, 0, bytes.Length);
                IpcChannelHelper.Copy(requestStream, bs);
            }
            catch 
            {
                bs.Write(bytes, 0, bytes.Length);
            }

            bs.Flush();
        }

        /// <summary>
        /// Reads a response.
        /// </summary>
        /// <param name="headers"></param>
        /// <param name="responseStream"></param>
        public void Read(out ITransportHeaders headers, out Stream responseStream)
        {
            byte[] bytes;
            long length;

            bytes = BitConverter.GetBytes(int.MaxValue);
            socket.Receive(bytes, 0, bytes.Length);
            length = BitConverter.ToInt32(bytes, 0);

            if (length != int.MaxValue && length > 0) 
            {
                bytes = new byte[length];
                socket.Receive(bytes, 0, (int)length);

                MemoryStream m = new MemoryStream(bytes);
                headers = (ITransportHeaders) formatter.Deserialize(m);
            }
            else 
            {
                headers = new TransportHeaders();
            }

            bytes = BitConverter.GetBytes(int.MaxValue);
            socket.Receive(bytes, 0, bytes.Length);
            length = BitConverter.ToInt32(bytes, 0);
            if (length != int.MaxValue && length > 0) 
            {
                bytes = new byte[length];
                socket.Receive(bytes, 0, (int)length);
                responseStream = new MemoryStream(bytes);
            }
            else 
            {
                responseStream = new MemoryStream(new byte[0]);
            }
        }

        /// <summary>
        /// Closes the unterlying named pipe socket.
        /// </summary>
        public void Close() 
        {
            socket.Close();
        }
    }
}

#endif
