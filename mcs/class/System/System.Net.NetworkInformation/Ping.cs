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
using System.Text;
using System.Diagnostics;
using System.Globalization;
using System.ComponentModel;
using System.Net.Sockets;
using System.Security.Principal;
using System.Runtime.InteropServices;

namespace System.Net.NetworkInformation {
	[MonoTODO ("IPv6 support is missing")]
	public class Ping : Component, IDisposable
	{
		[StructLayout(LayoutKind.Sequential)]
		struct cap_user_header_t
		{
			public UInt32 version;
			public Int32 pid;
		};

		[StructLayout(LayoutKind.Sequential)]
		struct cap_user_data_t
		{
			public UInt32 effective;
			public UInt32 permitted;
			public UInt32 inheritable;
		}
		
		const int DefaultCount = 1;
		const string PingBinPath = "/bin/ping";
		const int default_timeout = 4000; // 4 sec.
		const int identifier = 1; // no need to be const, but there's no place to change it.

		// This value is correct as of Linux kernel version 2.6.25.9
		// See /usr/include/linux/capability.h
		const UInt32 linux_cap_version = 0x20071026;
		
		static readonly byte [] default_buffer = new byte [0];
		static bool canSendPrivileged;
		

		BackgroundWorker worker;
		object user_async_state;
		
		public event PingCompletedEventHandler PingCompleted;
		
		static Ping ()
		{
			if (Environment.OSVersion.Platform == PlatformID.Unix) {
				CheckLinuxCapabilities ();
				if (!canSendPrivileged && WindowsIdentity.GetCurrent ().Name == "root")
					canSendPrivileged = true;
			}
			else
				canSendPrivileged = true;
		}
		
		public Ping ()
		{
		}
  
		[DllImport ("libc", EntryPoint="capget")]
		static extern int capget (ref cap_user_header_t header, ref cap_user_data_t data);

		static void CheckLinuxCapabilities ()
		{
			try {
				cap_user_header_t header = new cap_user_header_t ();
				cap_user_data_t data = new cap_user_data_t ();

				header.version = linux_cap_version;

				int ret = -1;

				try {
					ret = capget (ref header, ref data);
				} catch (Exception) {
				}

				if (ret == -1)
					return;

				canSendPrivileged = (data.effective & (1 << 13)) != 0;
			} catch {
				canSendPrivileged = false;
			}
		}
		
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

			if (canSendPrivileged)
				return SendPrivileged (address, timeout, buffer, options);
			else
				return SendUnprivileged (address, timeout, buffer, options);
		}

		private PingReply SendPrivileged (IPAddress address, int timeout, byte [] buffer, PingOptions options)
		{
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

		private PingReply SendUnprivileged (IPAddress address, int timeout, byte [] buffer, PingOptions options)
		{
			DateTime sentTime = DateTime.Now;

			Process ping = new Process ();
			string args = BuildPingArgs (address, timeout, options);
			long trip_time = 0;

			ping.StartInfo.FileName = PingBinPath;
			ping.StartInfo.Arguments = args;

			ping.StartInfo.CreateNoWindow = true;
			ping.StartInfo.UseShellExecute = false;

			ping.StartInfo.RedirectStandardOutput = true;
			ping.StartInfo.RedirectStandardError = true;

			try {
				ping.Start ();

#pragma warning disable 219
				string stdout = ping.StandardOutput.ReadToEnd ();
				string stderr = ping.StandardError.ReadToEnd ();
#pragma warning restore 219
				
				trip_time = (long) (DateTime.Now - sentTime).TotalMilliseconds;
				if (!ping.WaitForExit (timeout) || ping.ExitCode == 2)
					return new PingReply (address, buffer, options, trip_time, IPStatus.TimedOut); 

				if (ping.ExitCode == 1)
					return new PingReply (address, buffer, options, trip_time, IPStatus.TtlExpired);
			} catch (Exception) {
				return new PingReply (address, buffer, options, trip_time, IPStatus.Unknown);
			} finally {
				if (ping != null) {
					if (!ping.HasExited)
						ping.Kill ();
					ping.Dispose ();
				}
			}

			return new PingReply (address, buffer, options, trip_time, IPStatus.Success);
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
				// Note that RunWorkerCompletedEventArgs.UserState cannot be used (LAMESPEC)
				OnPingCompleted (new PingCompletedEventArgs (ea.Error, ea.Cancelled, user_async_state, ea.Result as PingReply));
			};
			worker.RunWorkerAsync (userToken);
		}

		// SendAsyncCancel

		public void SendAsyncCancel ()
		{
			if (worker == null)
				throw new InvalidOperationException ("SendAsync operation is not in progress");
			worker.CancelAsync ();
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

		private string BuildPingArgs (IPAddress address, int timeout, PingOptions options)
		{
			StringBuilder args = new StringBuilder ();

			args.AppendFormat ("-c {0} ", DefaultCount, CultureInfo.InvariantCulture);
			args.AppendFormat ("-w {0} ", (double)timeout / 1000.0, CultureInfo.InvariantCulture);
			args.AppendFormat ("-t {0} ", options.Ttl, CultureInfo.InvariantCulture);
			args.Append ("-M ");
			args.Append (options.DontFragment ? "do " : "dont ");

			args.Append (address.ToString ());

			return args.ToString ();
		
		}

	}
}
#endif

