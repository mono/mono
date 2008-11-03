//
// UnixListener.cs
//
// Authors:
//	Joe Shaw (joeshaw@novell.com)
//
// Copyright (C) 2004-2005 Novell, Inc.
//

//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
//

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace Mono.Unix {

	public class UnixClient : MarshalByRefObject, IDisposable {
		NetworkStream stream;
		Socket client;
		bool disposed;

		public UnixClient ()
		{
			if (client != null) {
				client.Close ();
				client = null;
			}

			client = new Socket (AddressFamily.Unix, SocketType.Stream, 0);
		}

		public UnixClient (string path) : this ()
		{
			if (path == null)
				throw new ArgumentNullException ("ep");

			Connect (path);
		}

		public UnixClient (UnixEndPoint ep) : this ()
		{
			if (ep == null)
				throw new ArgumentNullException ("ep");

			Connect (ep);
		}

		// UnixListener uses this when accepting a connection.
		internal UnixClient (Socket sock)
		{
			Client = sock;
		}

#if NET_2_0
		public
#else
		protected
#endif
		Socket Client {
			get { return client; }
			set {
				client = value;
				stream = null;
			}
		}

		public PeerCred PeerCredential {
			get {
				CheckDisposed ();
				return new PeerCred (client);
			}
		}
        
		public LingerOption LingerState {
			get {
				CheckDisposed ();
				return (LingerOption) client.GetSocketOption (SocketOptionLevel.Socket,
									      SocketOptionName.Linger);
			}

			set {
				CheckDisposed ();
				client.SetSocketOption (SocketOptionLevel.Socket,
							SocketOptionName.Linger, value);
			}
		}

		public int ReceiveBufferSize {
			get {
				CheckDisposed ();
				return (int) client.GetSocketOption (SocketOptionLevel.Socket,
								     SocketOptionName.ReceiveBuffer);
			}

			set {
				CheckDisposed ();
				client.SetSocketOption (SocketOptionLevel.Socket,
							SocketOptionName.ReceiveBuffer, value);
			}
		}
            
		public int ReceiveTimeout {
			get {
				CheckDisposed ();
				return (int) client.GetSocketOption (SocketOptionLevel.Socket,
								     SocketOptionName.ReceiveTimeout);
			}

			set {
				CheckDisposed ();
				client.SetSocketOption (SocketOptionLevel.Socket,
							SocketOptionName.ReceiveTimeout, value);
			}
		}
        
		public int SendBufferSize {
			get {
				CheckDisposed ();
				return (int) client.GetSocketOption (SocketOptionLevel.Socket,
								     SocketOptionName.SendBuffer);
			}

			set {
				CheckDisposed ();
				client.SetSocketOption (SocketOptionLevel.Socket,
							SocketOptionName.SendBuffer, value);
			}
		}
        
		public int SendTimeout {
			get {
				CheckDisposed ();
				return (int) client.GetSocketOption (SocketOptionLevel.Socket,
								     SocketOptionName.SendTimeout);
			}

			set {
				CheckDisposed ();
				client.SetSocketOption (SocketOptionLevel.Socket,
							SocketOptionName.SendTimeout, value);
			}
		}
        
		public void Close ()
		{
			CheckDisposed ();
			Dispose ();
		}
        
		public void Connect (UnixEndPoint remoteEndPoint)
		{
			CheckDisposed ();
			client.Connect (remoteEndPoint);
			stream = new NetworkStream (client, true);
		}
        
		public void Connect (string path)
		{
			CheckDisposed ();
			Connect (new UnixEndPoint (path));
		}
        
		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (disposed)
				return;

			if (disposing) {
				// release managed resources
				NetworkStream s = stream;
				stream = null;
				if (s != null) {
					// This closes the socket as well, as the NetworkStream
					// owns the socket.
					s.Close();
					s = null;
				} else if (client != null){
					client.Close ();
				}
				client = null;
			}

			disposed = true;
		}

		public NetworkStream GetStream ()
		{
			CheckDisposed ();
			if (stream == null)
				stream = new NetworkStream (client, true);

			return stream;
		}
        
		void CheckDisposed ()
		{
			if (disposed)
				throw new ObjectDisposedException (GetType().FullName);
		}        

		~UnixClient ()
		{
			Dispose (false);
		}
	}
}

