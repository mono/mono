//
// System.Net.NetworkInformation.Ping
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@novell.com)
//	Atsushi Enomoto (atsushi@ximian.com)
//
// Copyright (c) 2006-2007 Novell, Inc. (http://www.novell.com)
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
using System.ComponentModel;
using System.Net.Sockets;

namespace System.Net.NetworkInformation {
	[MonoTODO ("IPv6 support is missing")]
	public class Ping : Component, IDisposable
	{
		const int default_timeout = 4000; // 4 sec.
		static readonly byte [] default_buffer = new byte [0];
		const int identifier = 1; // no need to be const, but there's no place to change it.

		public event PingCompletedEventHandler PingCompleted;

		public Ping ()
		{
		}

		BackgroundWorker worker;
		object user_async_state;

		void IDisposable.Dispose ()
		{
		}

		protected void OnPingCompleted (PingCompletedEventArgs e)
		{
			if (PingCompleted != null)
				PingCompleted (this, e);
			user_async_state = null;
			worker = null;
		}

		// Sync

		public PingReply Send (IPAddress address)
		{
			return Send (address, default_timeout);
		}

		public PingReply Send (IPAddress address, int timeout)
		{
			return Send (address, timeout, default_buffer);
		}

		public PingReply Send (IPAddress address, int timeout, byte [] buffer)
		{
			return Send (address, timeout, buffer, new PingOptions ());
		}

		public PingReply Send (string hostNameOrAddress)
		{
			return Send (hostNameOrAddress, default_timeout);
		}

		public PingReply Send (string hostNameOrAddress, int timeout)
		{
			return Send (hostNameOrAddress, timeout, default_buffer);
		}

		public PingReply Send (string hostNameOrAddress, int timeout, byte [] buffer)
		{
			return Send (hostNameOrAddress, timeout, buffer, new PingOptions ());
		}

		public PingReply Send (string hostNameOrAddress, int timeout, byte [] buffer, PingOptions options)
		{
			IPAddress address = Dns.GetHostEntry (hostNameOrAddress).AddressList [0];
			return Send (address, timeout, buffer, options);
		}

		IPAddress GetNonLoopbackIP ()
		{
			foreach (IPAddress addr in Dns.GetHostByName (Dns.GetHostName ()).AddressList)
				if (!IPAddress.IsLoopback (addr))
					return addr;
			throw new InvalidOperationException ("Could not resolve non-loopback IP address for localhost");
		}

		public PingReply Send (IPAddress address, int timeout, byte [] buffer, PingOptions options)
		{
			if (address == null)
				throw new ArgumentNullException ("address");
			if (timeout < 0)
				throw new ArgumentOutOfRangeException ("timeout", "timeout must be non-negative integer");
			if (buffer == null)
				throw new ArgumentNullException ("buffer");
			// options can be null.

			IPEndPoint target = new IPEndPoint (address, 0);
			IPEndPoint client = new IPEndPoint (GetNonLoopbackIP (), 0);

			// FIXME: support IPv6
			Socket s = new Socket (AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.Icmp);
			if (options != null) {
				s.DontFragment = options.DontFragment;
				s.Ttl = (short) options.Ttl;
			}
			s.SendTimeout = timeout;
			s.ReceiveTimeout = timeout;
			// not sure why Identifier = 0 is unacceptable ...
			IcmpMessage send = new IcmpMessage (8, 0, identifier, 0, buffer);
			byte [] bytes = send.GetBytes ();
			s.SendBufferSize = bytes.Length;
			s.SendTo (bytes, bytes.Length, SocketFlags.None, target);

			DateTime sentTime = DateTime.Now;

			// receive
			bytes = new byte [100];
			do {
				try {
					EndPoint endpoint = client;
					int rc = s.ReceiveFrom (bytes, 100, SocketFlags.None, ref endpoint);
					long rtt = (long) (DateTime.Now - sentTime).TotalMilliseconds;
					int headerLength = (bytes [0] & 0xF) << 2;
					int bodyLength = rc - headerLength;

					if (!((IPEndPoint) endpoint).Address.Equals (target.Address)) // Ping reply to different request. discard it.
						continue;

					IcmpMessage recv = new IcmpMessage (bytes, headerLength, bodyLength);
					if (recv.Identifier != identifier)
						continue; // ping reply to different request. discard it.

					return new PingReply (address, recv.Data, options, rtt, recv.IPStatus);
				} catch (SocketException ex) {
					IPStatus stat;
					switch (ex.SocketErrorCode) {
					case SocketError.TimedOut:
						stat = IPStatus.TimedOut;
						break;
					default:
						throw new NotSupportedException (String.Format ("Unexpected socket error during ping request: {0}", ex.SocketErrorCode));
					}
					return new PingReply (null, new byte [0], options, 0, stat);
				}
			} while (true);
		}

		// Async

		public void SendAsync (IPAddress address, int timeout, byte [] buffer, object userToken)
		{
			SendAsync (address, default_timeout, default_buffer, new PingOptions (), userToken);
		}

		public void SendAsync (IPAddress address, int timeout, object userToken)
		{
			SendAsync (address, default_timeout, default_buffer, userToken);
		}

		public void SendAsync (IPAddress address, object userToken)
		{
			SendAsync (address, default_timeout, userToken);
		}

		public void SendAsync (string hostNameOrAddress, int timeout, byte [] buffer, object userToken)
		{
			SendAsync (hostNameOrAddress, timeout, buffer, new PingOptions (), userToken);
		}

		public void SendAsync (string hostNameOrAddress, int timeout, byte [] buffer, PingOptions options, object userToken)
		{
			IPAddress address = Dns.GetHostEntry (hostNameOrAddress).AddressList [0];
			SendAsync (address, timeout, buffer, options, userToken);
		}

		public void SendAsync (string hostNameOrAddress, int timeout, object userToken)
		{
			SendAsync (hostNameOrAddress, timeout, default_buffer, userToken);
		}

		public void SendAsync (string hostNameOrAddress, object userToken)
		{
			SendAsync (hostNameOrAddress, default_timeout, userToken);
		}

		public void SendAsync (IPAddress address, int timeout, byte [] buffer, PingOptions options, object userToken)
		{
			if (worker != null)
				throw new InvalidOperationException ("Another SendAsync operation is in progress");

			worker = new BackgroundWorker ();
			worker.DoWork += delegate (object o, DoWorkEventArgs ea) {
				try {
					user_async_state = ea.Argument;
					ea.Result = Send (address, timeout, buffer, options);
				} catch (Exception ex) {
					ea.Result = ex;
				}
			};
			worker.WorkerSupportsCancellation = true;
			worker.RunWorkerCompleted += delegate (object o, RunWorkerCompletedEventArgs ea) {
				if (ea.Result is PingReply)
					OnPingCompleted (new PingCompletedEventArgs (null, false, user_async_state, (PingReply) ea.Result));
				else
					OnPingCompleted (new PingCompletedEventArgs ((Exception) ea.Result, false, user_async_state, null));
			};
			worker.RunWorkerAsync (userToken);
		}

		// SendAsyncCancel

		public void SendAsyncCancel ()
		{
			if (worker == null)
				throw new InvalidOperationException ("Another SendAsync operation is in progress");
			worker.CancelAsync ();
			OnPingCompleted (new PingCompletedEventArgs (null, true, user_async_state, null));
		}

		// ICMP message

		class IcmpMessage
		{
			byte [] bytes;

			// received
			public IcmpMessage (byte [] bytes, int offset, int size)
			{
				this.bytes = new byte [size];
				Array.Copy (bytes, offset, this.bytes, 0, size);
			}

			// to be sent
			public IcmpMessage (byte type, byte code, short identifier, short sequence, byte [] data)
			{
				bytes = new byte [data.Length + 8];
				bytes [0] = type;
				bytes [1] = code;
				bytes [4] = (byte) (identifier & 0xFF);
				bytes [5] = (byte) ((int) identifier >> 8);
				bytes [6] = (byte) (sequence & 0xFF);
				bytes [7] = (byte) ((int) sequence >> 8);
				Array.Copy (data, 0, bytes, 8, data.Length);

				ushort checksum = ComputeChecksum (bytes);
				bytes [2] = (byte) (checksum & 0xFF);
				bytes [3] = (byte) ((int) checksum >> 8);
			}

			public byte Type {
				get { return bytes [0]; }
			}

			public byte Code {
				get { return bytes [1]; }
			}

			public byte Identifier {
				get { return (byte) (bytes [4] + (bytes [5] << 8)); }
			}

			public byte Sequence {
				get { return (byte) (bytes [6] + (bytes [7] << 8)); }
			}

			public byte [] Data {
				get {
					byte [] data = new byte [bytes.Length - 8];
					Array.Copy (bytes, 0, data, 0, data.Length);
					return data;
				}
			}

			public byte [] GetBytes ()
			{
				return bytes;
			}

			static ushort ComputeChecksum (byte [] data)
			{
				uint ret = 0;
				for (int i = 0; i < data.Length; i += 2) {
					ushort us = i + 1 < data.Length ? data [i + 1] : (byte) 0;
					us <<= 8;
					us += data [i];
					ret += us;
				}
				ret = (ret >> 16) + (ret & 0xFFFF);
				return (ushort) ~ ret;
			}

			public IPStatus IPStatus {
				get {
					switch (Type) {
					case 0:
						return IPStatus.Success;
					case 3: // destination unreacheable
						switch (Code) {
						case 0:
							return IPStatus.DestinationNetworkUnreachable;
						case 1:
							return IPStatus.DestinationHostUnreachable;
						case 2:
							return IPStatus.DestinationProtocolUnreachable;
						case 3:
							return IPStatus.DestinationPortUnreachable;
						case 4:
							return IPStatus.BadOption; // FIXME: likely wrong
						case 5:
							return IPStatus.BadRoute; // not sure if it is correct
						}
						break;
					case 11:
						switch (Code) {
						case 0:
							return IPStatus.TimeExceeded;
						case 1:
							return IPStatus.TtlReassemblyTimeExceeded;
						}
						break;
					case 12:
						return IPStatus.ParameterProblem;
					case 4:
						return IPStatus.SourceQuench;
					}
					throw new NotSupportedException (String.Format ("Unexpected pair of ICMP message type and code: type is {0} and code is {1}", Type, Code));
				}
			}
		}

	}
}
#endif

