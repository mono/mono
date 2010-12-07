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
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace Mono.Unix {

	public class UnixListener : MarshalByRefObject, IDisposable {
		bool disposed;
		bool listening;
		Socket server;
		EndPoint savedEP;
 
		void Init (UnixEndPoint ep)
		{
			listening = false;
			string filename = ep.Filename;
			if (File.Exists (filename)) {
				Socket conn = new Socket (AddressFamily.Unix, SocketType.Stream, 0);
				try {
					conn.Connect (ep);
					conn.Close ();
					throw new InvalidOperationException ("There's already a server listening on " + filename);
				} catch (SocketException) {
				}
				File.Delete (filename);
			}

			server = new Socket (AddressFamily.Unix, SocketType.Stream, 0);
			server.Bind (ep);
			savedEP = server.LocalEndPoint;
		}
        
		public UnixListener (string path)
		{
			if (!Directory.Exists (Path.GetDirectoryName (path)))
				Directory.CreateDirectory (Path.GetDirectoryName (path));
            
			Init (new UnixEndPoint (path));
		}

		public UnixListener (UnixEndPoint localEndPoint)
		{
			if (localEndPoint == null)
				throw new ArgumentNullException ("localendPoint");

			Init (localEndPoint);
		}
        
		public EndPoint LocalEndpoint {
			get { return savedEP; }
		}
        
		protected Socket Server {
			get { return server; }
		}
        
		public Socket AcceptSocket ()
		{
			CheckDisposed ();
			if (!listening)
				throw new InvalidOperationException ("Socket is not listening");

			return server.Accept ();
		}
        
		public UnixClient AcceptUnixClient ()
		{
			CheckDisposed ();
			if (!listening)
				throw new InvalidOperationException ("Socket is not listening");

			return new UnixClient (AcceptSocket ());
		}
        
		~UnixListener ()
		{
			Dispose (false);
		}
    
		public bool Pending ()
		{
			CheckDisposed ();
			if (!listening)
				throw new InvalidOperationException ("Socket is not listening");

			return server.Poll (1000, SelectMode.SelectRead);
		}
        
		public void Start ()
		{
			Start (5);
		}
        
		public void Start (int backlog)
		{
			CheckDisposed ();
			if (listening)
				return;

			server.Listen (backlog);
			listening = true;
		}

		public void Stop ()
		{
			CheckDisposed ();
			Dispose (true);
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected void Dispose (bool disposing)
		{
			if (disposed)
				return;

			if (disposing) {
				try {
					File.Delete (((UnixEndPoint) savedEP).Filename);
				} catch {
				}
				if (server != null)
					server.Close ();

				server = null;
			}

			disposed = true;
		}
        
		void CheckDisposed ()
		{
			if (disposed)
				throw new ObjectDisposedException (GetType().FullName);
		}        
	}

}
