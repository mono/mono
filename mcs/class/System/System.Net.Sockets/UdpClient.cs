//
// System.Net.Sockets.UdpClient.cs
//
// Author:
//    Gonzalo Paniagua Javier <gonzalo@ximian.com>
//    Sridhar Kulkarni (sridharkulkarni@gmail.com)
//    Marek Safar (marek.safar@gmail.com)
//
// Copyright (C) Ximian, Inc. http://www.ximian.com
// Copyright 2011 Xamarin Inc.
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

using System;
using System.Net;
using System.Threading.Tasks;

namespace System.Net.Sockets
{
	public class UdpClient : IDisposable
	{
		private bool disposed = false;
		private bool active = false;
		private Socket socket;
		private AddressFamily family = AddressFamily.InterNetwork;
		private byte[] recvbuffer;
	
		public UdpClient () : this(AddressFamily.InterNetwork)
		{
		}

		public UdpClient(AddressFamily family)
		{
			if(family != AddressFamily.InterNetwork && family != AddressFamily.InterNetworkV6)
				throw new ArgumentException ("Family must be InterNetwork or InterNetworkV6", "family");

			this.family = family;
			InitSocket (null);
		}

		public UdpClient (int port)
		{
			if (port < IPEndPoint.MinPort || port > IPEndPoint.MaxPort)
				throw new ArgumentOutOfRangeException ("port");

			this.family = AddressFamily.InterNetwork;

			IPEndPoint localEP = new IPEndPoint (IPAddress.Any, port);
			InitSocket (localEP);
		}

		public UdpClient (IPEndPoint localEP)
		{
			if (localEP == null)
				throw new ArgumentNullException ("localEP");

			this.family = localEP.AddressFamily;

			InitSocket (localEP);
		}

		public UdpClient (int port, AddressFamily family)
		{
			if (family != AddressFamily.InterNetwork && family != AddressFamily.InterNetworkV6)
				throw new ArgumentException ("Family must be InterNetwork or InterNetworkV6", "family");

			if (port < IPEndPoint.MinPort ||
			    port > IPEndPoint.MaxPort) {
				throw new ArgumentOutOfRangeException ("port");
			}

			this.family = family;

			IPEndPoint localEP;

			if (family == AddressFamily.InterNetwork)
				localEP = new IPEndPoint (IPAddress.Any, port);
			else
				localEP = new IPEndPoint (IPAddress.IPv6Any, port);
			InitSocket (localEP);
		}

		public UdpClient (string hostname, int port)
		{
			if (hostname == null)
				throw new ArgumentNullException ("hostname");

			if (port < IPEndPoint.MinPort || port > IPEndPoint.MaxPort)
				throw new ArgumentOutOfRangeException ("port");

			InitSocket (null);
			Connect (hostname, port);
		}

		private void InitSocket (EndPoint localEP)
		{
			if(socket != null) {
				socket.Close();
				socket = null;
			}

			socket = new Socket (family, SocketType.Dgram, ProtocolType.Udp);

			if (localEP != null)
				socket.Bind (localEP);
		}

		public void Close ()
		{
			Dispose ();
		}
#region Connect

		void DoConnect (IPEndPoint endPoint)
		{
			/* Catch EACCES and turn on SO_BROADCAST then,
			 * as UDP sockets don't have it set by default
			 */
			try {
				socket.Connect (endPoint);
			} catch (SocketException ex) {
				if (ex.ErrorCode == (int)SocketError.AccessDenied) {
					socket.SetSocketOption (SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
					
					socket.Connect (endPoint);
				} else {
					throw;
				}
			}
		}
		
		public void Connect (IPEndPoint endPoint)
		{
			CheckDisposed ();
			if (endPoint == null)
				throw new ArgumentNullException ("endPoint");

			DoConnect (endPoint);
			active = true;
		}

		public void Connect (IPAddress addr, int port)
		{
			if (addr == null)
				throw new ArgumentNullException ("addr");

			if (port < IPEndPoint.MinPort || port > IPEndPoint.MaxPort)
				throw new ArgumentOutOfRangeException ("port");


			Connect (new IPEndPoint (addr, port));
		}

		public void Connect (string hostname, int port)
		{
			if (port < IPEndPoint.MinPort || port > IPEndPoint.MaxPort)
				throw new ArgumentOutOfRangeException ("port");

			IPAddress[] addresses = Dns.GetHostAddresses (hostname);
			for(int i=0; i<addresses.Length; i++) {
				try {
					this.family = addresses[i].AddressFamily;
					Connect (new IPEndPoint (addresses[i], port));
					break;
				} catch(Exception e) {
					if(i == addresses.Length - 1){
						if(socket != null) {
							socket.Close();
							socket = null;
						}
						/// This is the last entry, re-throw the exception
						throw e;
					}
				}
			}
		}
#endregion
		#region Multicast methods
		public void DropMulticastGroup (IPAddress multicastAddr)
		{
			CheckDisposed ();
			if (multicastAddr == null)
				throw new ArgumentNullException ("multicastAddr");

			if(family == AddressFamily.InterNetwork)
				socket.SetSocketOption (SocketOptionLevel.IP, SocketOptionName.DropMembership,
					new MulticastOption (multicastAddr));
			else
				socket.SetSocketOption (SocketOptionLevel.IPv6, SocketOptionName.DropMembership,
					new IPv6MulticastOption (multicastAddr));
		}

		public void DropMulticastGroup (IPAddress multicastAddr,
						int ifindex)
		{
			CheckDisposed ();

			/* LAMESPEC: exceptions haven't been specified
			 * for this overload.
			 */
			if (multicastAddr == null) {
				throw new ArgumentNullException ("multicastAddr");
			}

			/* Does this overload only apply to IPv6?
			 * Only the IPv6MulticastOption has an
			 * ifindex-using constructor.  The MS docs
			 * don't say.
			 */
			if (family == AddressFamily.InterNetworkV6) {
				socket.SetSocketOption (SocketOptionLevel.IPv6, SocketOptionName.DropMembership, new IPv6MulticastOption (multicastAddr, ifindex));
			}
		}
		
		public void JoinMulticastGroup (IPAddress multicastAddr)
		{
			CheckDisposed ();

			if (multicastAddr == null)
				throw new ArgumentNullException ("multicastAddr");

			if(family == AddressFamily.InterNetwork)
				socket.SetSocketOption (SocketOptionLevel.IP, SocketOptionName.AddMembership,
					new MulticastOption (multicastAddr));
			else
				socket.SetSocketOption (SocketOptionLevel.IPv6, SocketOptionName.AddMembership,
					new IPv6MulticastOption (multicastAddr));
		}

		public void JoinMulticastGroup (int ifindex,
						IPAddress multicastAddr)
		{
			CheckDisposed ();

			if (multicastAddr == null)
				throw new ArgumentNullException ("multicastAddr");

			if (family == AddressFamily.InterNetworkV6)
				socket.SetSocketOption (SocketOptionLevel.IPv6, SocketOptionName.AddMembership, new IPv6MulticastOption (multicastAddr, ifindex));
			else
				throw new SocketException ((int) SocketError.OperationNotSupported);
		}

		public void JoinMulticastGroup (IPAddress multicastAddr, int timeToLive)
		{
			CheckDisposed ();
			if (multicastAddr == null)
				throw new ArgumentNullException ("multicastAddr");
			if (timeToLive < 0 || timeToLive > 255)
				throw new ArgumentOutOfRangeException ("timeToLive");

			JoinMulticastGroup (multicastAddr);
			if(family == AddressFamily.InterNetwork)
				socket.SetSocketOption (SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive,
					timeToLive);
			else
				socket.SetSocketOption (SocketOptionLevel.IPv6, SocketOptionName.MulticastTimeToLive,
					timeToLive);
		}

		public void JoinMulticastGroup (IPAddress multicastAddr,
						IPAddress localAddress)
		{
			CheckDisposed ();

			if (family == AddressFamily.InterNetwork)
				socket.SetSocketOption (SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption (multicastAddr, localAddress));
			else
				throw new SocketException ((int) SocketError.OperationNotSupported);
		}

		#endregion
		#region Data I/O
		public byte [] Receive (ref IPEndPoint remoteEP)
		{
			CheckDisposed ();

			byte [] recBuffer = new byte [65536]; // Max. size
			EndPoint endPoint = (EndPoint) remoteEP;
			int dataRead = socket.ReceiveFrom (recBuffer, ref endPoint);
			if (dataRead < recBuffer.Length)
				recBuffer = CutArray (recBuffer, dataRead);

			remoteEP = (IPEndPoint) endPoint;
			return recBuffer;
		}

		int DoSend (byte[] dgram, int bytes, IPEndPoint endPoint)
		{
			/* Catch EACCES and turn on SO_BROADCAST then,
			 * as UDP sockets don't have it set by default
			 */
			try {
				if (endPoint == null) {
					return(socket.Send (dgram, 0, bytes,
							    SocketFlags.None));
				} else {
					return(socket.SendTo (dgram, 0, bytes,
							      SocketFlags.None,
							      endPoint));
				}
			} catch (SocketException ex) {
				if (ex.ErrorCode == (int)SocketError.AccessDenied) {
					socket.SetSocketOption (SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
					if (endPoint == null) {
						return(socket.Send (dgram, 0, bytes, SocketFlags.None));
					} else {
						return(socket.SendTo (dgram, 0, bytes, SocketFlags.None, endPoint));
					}
				} else {
					throw;
				}
			}
		}
		
		public int Send (byte [] dgram, int bytes)
		{
			CheckDisposed ();
			if (dgram == null)
				throw new ArgumentNullException ("dgram");

			if (!active)
				throw new InvalidOperationException ("Operation not allowed on " + 
								     "non-connected sockets.");

			return(DoSend (dgram, bytes, null));
		}

		public int Send (byte [] dgram, int bytes, IPEndPoint endPoint)
		{
			CheckDisposed ();
			if (dgram == null)
				throw new ArgumentNullException ("dgram is null");
			
			if (active) {
				if (endPoint != null)
					throw new InvalidOperationException ("Cannot send packets to an " +
									     "arbitrary host while connected.");

				return(DoSend (dgram, bytes, null));
			}

			return(DoSend (dgram, bytes, endPoint));
		}

		public int Send (byte [] dgram, int bytes, string hostname, int port)
		{
			return Send (dgram, bytes, 
				     new IPEndPoint (Dns.GetHostAddresses (hostname) [0], port));
		}

		private byte [] CutArray (byte [] orig, int length)
		{
			byte [] newArray = new byte [length];
			Buffer.BlockCopy (orig, 0, newArray, 0, length);

			return newArray;
		}
#endregion

		IAsyncResult DoBeginSend (byte[] datagram, int bytes,
					  IPEndPoint endPoint,
					  AsyncCallback requestCallback,
					  object state)
		{
			/* Catch EACCES and turn on SO_BROADCAST then,
			 * as UDP sockets don't have it set by default
			 */
			try {
				if (endPoint == null) {
					return(socket.BeginSend (datagram, 0, bytes, SocketFlags.None, requestCallback, state));
				} else {
					return(socket.BeginSendTo (datagram, 0, bytes, SocketFlags.None, endPoint, requestCallback, state));
				}
			} catch (SocketException ex) {
				if (ex.ErrorCode == (int)SocketError.AccessDenied) {
					socket.SetSocketOption (SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
					if (endPoint == null) {
						return(socket.BeginSend (datagram, 0, bytes, SocketFlags.None, requestCallback, state));
					} else {
						return(socket.BeginSendTo (datagram, 0, bytes, SocketFlags.None, endPoint, requestCallback, state));
					}
				} else {
					throw;
				}
			}
		}
		
		public IAsyncResult BeginSend (byte[] datagram, int bytes,
					       AsyncCallback requestCallback,
					       object state)
		{
			return(BeginSend (datagram, bytes, null,
					  requestCallback, state));
		}
		
		public IAsyncResult BeginSend (byte[] datagram, int bytes,
					       IPEndPoint endPoint,
					       AsyncCallback requestCallback,
					       object state)
		{
			CheckDisposed ();

			if (datagram == null) {
				throw new ArgumentNullException ("datagram");
			}
			
			return(DoBeginSend (datagram, bytes, endPoint,
					    requestCallback, state));
		}
		
		public IAsyncResult BeginSend (byte[] datagram, int bytes,
					       string hostname, int port,
					       AsyncCallback requestCallback,
					       object state)
		{
			return(BeginSend (datagram, bytes, new IPEndPoint (Dns.GetHostAddresses (hostname) [0], port), requestCallback, state));
		}
		
		public int EndSend (IAsyncResult asyncResult)
		{
			CheckDisposed ();
			
			if (asyncResult == null) {
				throw new ArgumentNullException ("asyncResult is a null reference");
			}
			
			return(socket.EndSend (asyncResult));
		}

		public IAsyncResult BeginReceive (AsyncCallback requestCallback, object state)
		{
			CheckDisposed ();

			recvbuffer = new byte[8192];
			
			EndPoint ep;
			
			if (family == AddressFamily.InterNetwork) {
				ep = new IPEndPoint (IPAddress.Any, 0);
			} else {
				ep = new IPEndPoint (IPAddress.IPv6Any, 0);
			}
			
			return(socket.BeginReceiveFrom (recvbuffer, 0, 8192,
							SocketFlags.None,
							ref ep,
							requestCallback, state));
		}
		
		public byte[] EndReceive (IAsyncResult asyncResult, ref IPEndPoint remoteEP)
		{
			CheckDisposed ();
			
			if (asyncResult == null) {
				throw new ArgumentNullException ("asyncResult is a null reference");
			}
			
			EndPoint ep;
			
			if (family == AddressFamily.InterNetwork) {
				ep = new IPEndPoint (IPAddress.Any, 0);
			} else {
				ep = new IPEndPoint (IPAddress.IPv6Any, 0);
			}
			
			int bytes = socket.EndReceiveFrom (asyncResult,
							   ref ep);
			remoteEP = (IPEndPoint)ep;

			/* Need to copy into a new array here, because
			 * otherwise the returned array length is not
			 * 'bytes'
			 */
			byte[] buf = new byte[bytes];
			Array.Copy (recvbuffer, buf, bytes);
			
			return(buf);
		}
				
#region Properties
		protected bool Active {
			get { return active; }
			set { active = value; }
		}

		public Socket Client {
			get { return socket; }
			set { socket = value; }
		}

		public int Available
		{
			get {
				return(socket.Available);
			}
		}
		
		public bool DontFragment
		{
			get {
				return(socket.DontFragment);
			}
			set {
				socket.DontFragment = value;
			}
		}

		public bool EnableBroadcast
		{
			get {
				return(socket.EnableBroadcast);
			}
			set {
				socket.EnableBroadcast = value;
			}
		}
		
		public bool ExclusiveAddressUse
		{
			get {
				return(socket.ExclusiveAddressUse);
			}
			set {
				socket.ExclusiveAddressUse = value;
			}
		}
		
		public bool MulticastLoopback
		{
			get {
				return(socket.MulticastLoopback);
			}
			set {
				socket.MulticastLoopback = value;
			}
		}
		
		public short Ttl
		{
			get {
				return(socket.Ttl);
			}
			set {
				socket.Ttl = value;
			}
		}

#endregion
#region Disposing
		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (disposed)
				return;
			disposed = true;

			if (disposing){
				if (socket != null)
					socket.Close ();

				socket = null;
			}
		}
		
		~UdpClient ()
		{
			Dispose (false);
		}
		
		private void CheckDisposed ()
		{
			if (disposed)
				throw new ObjectDisposedException (GetType().FullName);
		}		
#endregion

		
		public Task<UdpReceiveResult> ReceiveAsync ()
		{
			return Task<UdpReceiveResult>.Factory.FromAsync (BeginReceive, r => {
				IPEndPoint remoteEndPoint = null;
				return new UdpReceiveResult (EndReceive (r, ref remoteEndPoint), remoteEndPoint);
			}, null);
		}

		public Task<int> SendAsync (byte[] datagram, int bytes)
		{
			return Task<int>.Factory.FromAsync (BeginSend, EndSend, datagram, bytes, null);
		}

		public Task<int> SendAsync (byte[] datagram, int bytes, IPEndPoint endPoint)
		{
			return Task<int>.Factory.FromAsync (BeginSend, EndSend, datagram, bytes, endPoint, null);
		}

		public Task<int> SendAsync (byte[] datagram, int bytes, string hostname, int port)
		{
			var t = Tuple.Create (datagram, bytes, hostname, port, this);

			return Task<int>.Factory.FromAsync ((callback, state) => {
				var d = (Tuple<byte[], int, string, int, UdpClient>) state;
				return d.Item5.BeginSend (d.Item1, d.Item2, d.Item3, d.Item4, callback, null);
			}, EndSend, t);
 
		}
	}
}

