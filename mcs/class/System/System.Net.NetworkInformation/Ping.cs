//
// System.Net.NetworkInformation.Ping
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@novell.com)
//	Atsushi Enomoto (atsushi@ximian.com)
//
// Copyright (c) 2006-2007 Novell, Inc. (http://www.novell.com)
// Copyright 2015 Xamarin Inc.
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
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using System.ComponentModel;
using System.Net.Sockets;
using System.Security.Principal;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace System.Net.NetworkInformation {
	[MonoTODO ("IPv6 support is missing")]
	public class Ping : Component, IDisposable
	{
#if !MONOTOUCH
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
		static readonly string [] PingBinPaths = new string [] {
			"/bin/ping",
			"/sbin/ping",
			"/usr/sbin/ping",
#if MONODROID
			"/system/bin/ping"
#endif
		};
		static readonly string PingBinPath;
		static bool canSendPrivileged;
#endif
		const int default_timeout = 4000; // 4 sec.
		ushort identifier;

		// This value is correct as of Linux kernel version 2.6.25.9
		// See /usr/include/linux/capability.h
		const UInt32 linux_cap_version = 0x20071026;
		
		static readonly byte [] default_buffer = new byte [0];
		

		BackgroundWorker worker;
		object user_async_state;
		CancellationTokenSource cts;
		
		public event PingCompletedEventHandler PingCompleted;

#if !MONOTOUCH
		static Ping ()
		{
			if (Environment.OSVersion.Platform == PlatformID.Unix) {
				CheckLinuxCapabilities ();
				if (!canSendPrivileged && WindowsIdentity.GetCurrent ().Name == "root")
					canSendPrivileged = true;
			
				// Since different Unix systems can have different path to bin, we try some
				// of the known ones.
				foreach (string ping_path in PingBinPaths)
					if (File.Exists (ping_path)) {
						PingBinPath = ping_path;
						break;
					}
			}
			else
				canSendPrivileged = true;

			if (PingBinPath == null)
				PingBinPath = "/bin/ping"; // default, fallback value
		}
#endif
		
		public Ping ()
		{
			// Generate a new random 16 bit identifier for every ping
			RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider ();
			byte [] randomIdentifier = new byte [2];
			rng.GetBytes (randomIdentifier);
			identifier = (ushort)(randomIdentifier [0] + (randomIdentifier [1] << 8));
		}

#if !MONOTOUCH
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
#endif
		
		void IDisposable.Dispose ()
		{
		}

		protected void OnPingCompleted (PingCompletedEventArgs e)
		{
			user_async_state = null;
			worker = null;

			if (cts != null) {
				cts.Dispose();
				cts = null;
			}

			if (PingCompleted != null)
				PingCompleted (this, e);
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
			IPAddress [] addresses = Dns.GetHostAddresses (hostNameOrAddress);
			return Send (addresses [0], timeout, buffer, options);
		}

		static IPAddress GetNonLoopbackIPV4 ()
		{
#pragma warning disable 618
			foreach (IPAddress addr in Dns.GetHostByName (Dns.GetHostName ()).AddressList)
				if (!IPAddress.IsLoopback (addr) && addr.AddressFamily == AddressFamily.InterNetwork)
					return addr;
#pragma warning restore 618

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
			if (buffer.Length > 65500)
				throw new ArgumentException ("buffer");
			// options can be null.

#if MONOTOUCH
			throw new InvalidOperationException ();
#else
			if (canSendPrivileged)
				return SendPrivileged (address, timeout, buffer, options);
			return SendUnprivileged (address, timeout, buffer, options);
#endif
		}

#if !MONOTOUCH
		private PingReply SendPrivileged (IPAddress address, int timeout, byte [] buffer, PingOptions options)
		{
			IPEndPoint target = new IPEndPoint (address, 0);
			IPEndPoint client = new IPEndPoint (GetNonLoopbackIPV4 (), 0);

			// FIXME: support IPv6
			using (Socket s = new Socket (AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.Icmp)) {
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
					EndPoint endpoint = client;
					SocketError error = 0;
					int rc = s.ReceiveFrom (bytes, 0, 100, SocketFlags.None,
							ref endpoint, out error);

					if (error != SocketError.Success) {
						if (error == SocketError.TimedOut) {
							return new PingReply (null, new byte [0], options, 0, IPStatus.TimedOut);
						}
						throw new NotSupportedException (String.Format ("Unexpected socket error during ping request: {0}", error));
					}
					long rtt = (long) (DateTime.Now - sentTime).TotalMilliseconds;
					int headerLength = (bytes [0] & 0xF) << 2;
					int bodyLength = rc - headerLength;

					// Ping reply to different request. discard it.
					if (!((IPEndPoint) endpoint).Address.Equals (target.Address)) {
						long t = timeout - rtt;
						if (t <= 0)
							return new PingReply (null, new byte [0], options, 0, IPStatus.TimedOut);
						s.ReceiveTimeout = (int) t;
						continue;
					}

					IcmpMessage recv = new IcmpMessage (bytes, headerLength, bodyLength);

					/* discard ping reply to different request or echo requests if running on same host. */
					if (recv.Identifier != identifier || recv.Type == 8) {
						long t = timeout - rtt;
						if (t <= 0)
							return new PingReply (null, new byte [0], options, 0, IPStatus.TimedOut);
						s.ReceiveTimeout = (int) t;
						continue; 
					}

					return new PingReply (address, recv.Data, options, rtt, recv.IPStatus);
				} while (true);
			}
		}

		private PingReply SendUnprivileged (IPAddress address, int timeout, byte [] buffer, PingOptions options)
		{
#if MONO_FEATURE_PROCESS_START
			DateTime sentTime = DateTime.UtcNow;

			Process ping = new Process ();
			string args = BuildPingArgs (address, timeout, options);
			long trip_time = 0;

			ping.StartInfo.FileName = PingBinPath;
			ping.StartInfo.Arguments = args;

			ping.StartInfo.CreateNoWindow = true;
			ping.StartInfo.UseShellExecute = false;

			ping.StartInfo.RedirectStandardOutput = true;
			ping.StartInfo.RedirectStandardError = true;

			IPStatus status = IPStatus.Unknown;
			try {
				ping.Start ();

#pragma warning disable 219
				string stdout = ping.StandardOutput.ReadToEnd ();
				string stderr = ping.StandardError.ReadToEnd ();
#pragma warning restore 219
				
				trip_time = (long) (DateTime.UtcNow - sentTime).TotalMilliseconds;
				if (!ping.WaitForExit (timeout) || (ping.HasExited && ping.ExitCode == 2))
					status = IPStatus.TimedOut;
				else if (ping.ExitCode == 0)
					status = IPStatus.Success;
				else if (ping.ExitCode == 1)
					status = IPStatus.TtlExpired;
			} catch {
			} finally {
				if (!ping.HasExited)
					ping.Kill ();
				ping.Dispose ();
			}

			return new PingReply (address, buffer, options, trip_time, status);
#else
			throw new PlatformNotSupportedException ("Ping is not supported on this platform.");
#endif // MONO_FEATURE_PROCESS_START
		}
#endif // !MONOTOUCH

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
			if ((worker != null) || (cts != null))
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
			if (cts != null) {
				cts.Cancel ();
				return;
			}

			if (worker == null)
				throw new InvalidOperationException ("SendAsync operation is not in progress");
			worker.CancelAsync ();
		}

#if !MONOTOUCH
		// ICMP message

		class IcmpMessage
		{
			byte [] bytes;

			// received
			public IcmpMessage (byte [] bytes, int offset, int size)
			{
				this.bytes = new byte [size];
				Buffer.BlockCopy (bytes, offset, this.bytes, 0, size);
			}

			// to be sent
			public IcmpMessage (byte type, byte code, ushort identifier, ushort sequence, byte [] data)
			{
				bytes = new byte [data.Length + 8];
				bytes [0] = type;
				bytes [1] = code;
				bytes [4] = (byte) (identifier & 0xFF);
				bytes [5] = (byte) ((int) identifier >> 8);
				bytes [6] = (byte) (sequence & 0xFF);
				bytes [7] = (byte) ((int) sequence >> 8);
				Buffer.BlockCopy (data, 0, bytes, 8, data.Length);

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

			public ushort Identifier {
				get { return (ushort) (bytes [4] + (bytes [5] << 8)); }
			}

			public ushort Sequence {
				get { return (ushort) (bytes [6] + (bytes [7] << 8)); }
			}

			public byte [] Data {
				get {
					byte [] data = new byte [bytes.Length - 8];
					Buffer.BlockCopy (bytes, 8, data, 0, data.Length);
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
					case 8:
						return IPStatus.Success;
					}
					return IPStatus.Unknown;
					//throw new NotSupportedException (String.Format ("Unexpected pair of ICMP message type and code: type is {0} and code is {1}", Type, Code));
				}
			}
		}

		private string BuildPingArgs (IPAddress address, int timeout, PingOptions options)
		{
			CultureInfo culture = CultureInfo.InvariantCulture;
			StringBuilder args = new StringBuilder ();
			uint t = Convert.ToUInt32 (Math.Floor ((timeout + 1000) / 1000.0));
			bool is_mac = Platform.IsMacOS;
			if (!is_mac)
				args.AppendFormat (culture, "-q -n -c {0} -w {1} -t {2} -M ", DefaultCount, t, options.Ttl);
			else
				args.AppendFormat (culture, "-q -n -c {0} -t {1} -o -m {2} ", DefaultCount, t, options.Ttl);
			if (!is_mac)
				args.Append (options.DontFragment ? "do " : "dont ");
			else if (options.DontFragment)
				args.Append ("-D ");

			args.Append (address.ToString ());

			return args.ToString ();
		}
#endif // !MONOTOUCH

		public Task<PingReply> SendPingAsync (IPAddress address, int timeout, byte [] buffer)
		{
			return SendPingAsync (address, default_timeout, default_buffer, new PingOptions ());
		}

		public Task<PingReply> SendPingAsync (IPAddress address, int timeout)
		{
			return SendPingAsync (address, default_timeout, default_buffer);
		}

		public Task<PingReply> SendPingAsync (IPAddress address)
		{
			return SendPingAsync (address, default_timeout);
		}

		public Task<PingReply> SendPingAsync (string hostNameOrAddress, int timeout, byte [] buffer)
		{
			return SendPingAsync (hostNameOrAddress, timeout, buffer, new PingOptions ());
		}

		public Task<PingReply> SendPingAsync (string hostNameOrAddress, int timeout, byte [] buffer, PingOptions options)
		{
			IPAddress address = Dns.GetHostEntry (hostNameOrAddress).AddressList [0];
			return SendPingAsync (address, timeout, buffer, options);
		}

		public Task<PingReply> SendPingAsync (string hostNameOrAddress, int timeout)
		{
			return SendPingAsync (hostNameOrAddress, timeout, default_buffer);
		}

		public Task<PingReply> SendPingAsync (string hostNameOrAddress)
		{
			return SendPingAsync (hostNameOrAddress, default_timeout);
		}

		public Task<PingReply> SendPingAsync (IPAddress address, int timeout, byte [] buffer, PingOptions options)
		{
			if ((worker != null) || (cts != null))
				throw new InvalidOperationException ("Another SendAsync operation is in progress");

			cts = new CancellationTokenSource();

			var task = Task<PingReply>.Factory.StartNew (
				() => Send (address, timeout, buffer, options), cts.Token);

			task.ContinueWith ((t) => {
				if (t.IsCanceled)
					OnPingCompleted (new PingCompletedEventArgs (null, true, null, null));
				else if (t.IsFaulted)
					OnPingCompleted (new PingCompletedEventArgs (t.Exception, false, null, null));
				else
					OnPingCompleted (new PingCompletedEventArgs (null, false, null, t.Result));
			});

			return task;
		}
	}
}
