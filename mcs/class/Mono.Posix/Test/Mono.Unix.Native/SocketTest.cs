//
// socket-related test cases
//
// Authors:
//  Steffen Kiess (s-kiess@web.de)
//
// Copyright (C) 2015 Steffen Kiess
//

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

using Mono.Unix;
using Mono.Unix.Native;

using NUnit.Framework;

namespace MonoTests.Mono.Unix.Native
{
	[TestFixture, Category ("NotDotNet")]
	public class SocketTest {

		string TempFolder;

		[SetUp]
		public void SetUp ()
		{
			TempFolder = Path.Combine (Path.GetTempPath (), this.GetType ().FullName);

			if (Directory.Exists (TempFolder))
				Directory.Delete (TempFolder, true);

			Directory.CreateDirectory (TempFolder);
		}

		[TearDown]
		public void TearDown()
		{
			if (Directory.Exists (TempFolder))
				Directory.Delete (TempFolder, true);
		}

		// Set a timeout on all sockets to make sure that if a test fails it
		// won't cause the program to hang
		void SetTimeout (int socket)
		{
			var timeout = new Timeval {
				tv_sec = 0,
				tv_usec = 500000,
			};
			if (Syscall.setsockopt (socket, UnixSocketProtocol.SOL_SOCKET, UnixSocketOptionName.SO_RCVTIMEO, timeout) < 0 ||
					Syscall.setsockopt (socket, UnixSocketProtocol.SOL_SOCKET, UnixSocketOptionName.SO_SNDTIMEO, timeout) < 0)
				UnixMarshal.ThrowExceptionForLastError ();
		}

		void WithSocketPair (Action<int, int> f)
		{
			int socket1, socket2;
			if (Syscall.socketpair (UnixAddressFamily.AF_UNIX, UnixSocketType.SOCK_STREAM, 0, out socket1, out socket2) < 0)
				UnixMarshal.ThrowExceptionForLastError ();
			try {
				SetTimeout (socket1);
				SetTimeout (socket2);

				f (socket1, socket2);
			} finally {
				int r0 = Syscall.close (socket1);
				int r1 = Syscall.close (socket2);
				if (r0 < 0 || r1 < 0)
					UnixMarshal.ThrowExceptionForLastError ();
			}
		}

		void WithSockets (UnixAddressFamily af, UnixSocketType type, UnixSocketProtocol protocol, Action<int, int> f)
		{
			int so1, so2;
			if ((so1 = Syscall.socket (af, type, protocol)) < 0)
				UnixMarshal.ThrowExceptionForLastError ();
			try {
				if ((so2 = Syscall.socket (af, type, protocol)) < 0)
					UnixMarshal.ThrowExceptionForLastError ();
				try {
					SetTimeout (so1);
					SetTimeout (so2);

					f (so1, so2);
				} finally {
					if (Syscall.close (so2) < 0)
						UnixMarshal.ThrowExceptionForLastError ();
				}
			} finally {
				if (Syscall.close (so1) < 0)
					UnixMarshal.ThrowExceptionForLastError ();
			}
		}

		[Test]
		public void Socket ()
		{
			int socket;
			if ((socket = Syscall.socket (UnixAddressFamily.AF_UNIX, UnixSocketType.SOCK_STREAM, 0)) < 0)
				UnixMarshal.ThrowExceptionForLastError ();

			if (Syscall.close (socket) < 0)
				UnixMarshal.ThrowExceptionForLastError ();
		}

		[Test]
		public void SocketPair ()
		{
			int socket1, socket2;
			if (Syscall.socketpair (UnixAddressFamily.AF_UNIX, UnixSocketType.SOCK_STREAM, 0, out socket1, out socket2) < 0)
				UnixMarshal.ThrowExceptionForLastError ();

			int r0 = Syscall.close (socket1);
			int r1 = Syscall.close (socket2);
			if (r0 < 0 || r1 < 0)
				UnixMarshal.ThrowExceptionForLastError ();
		}

		[Test]
		public void SendRecv ()
		{
			WithSocketPair ((so1, so2) => {
				long ret;
				var buffer1 = new byte[] { 42, 43, 44 };
				ret = Syscall.send (so1, buffer1, (ulong) buffer1.Length, 0);
				if (ret < 0)
					UnixMarshal.ThrowExceptionForLastError ();

				var buffer2 = new byte[1024];
				ret = Syscall.recv (so2, buffer2, (ulong) buffer2.Length, 0);
				if (ret < 0)
					UnixMarshal.ThrowExceptionForLastError ();

				Assert.AreEqual (buffer1.Length, ret);
				for (int i = 0; i < buffer1.Length; i++)
					Assert.AreEqual (buffer1[i], buffer2[i]);
			});
		}

		[Test]
		public void SockOpt ()
		{
			WithSockets (UnixAddressFamily.AF_UNIX, UnixSocketType.SOCK_STREAM, 0, (so1, so2) => {
				int value;
				if (Syscall.getsockopt (so1, UnixSocketProtocol.SOL_SOCKET, UnixSocketOptionName.SO_REUSEADDR, out value) < 0)
					UnixMarshal.ThrowExceptionForLastError ();
				Assert.AreEqual (0, value);

				// Set SO_REUSEADDR to 1
				if (Syscall.setsockopt (so1, UnixSocketProtocol.SOL_SOCKET, UnixSocketOptionName.SO_REUSEADDR, 1) < 0)
					UnixMarshal.ThrowExceptionForLastError ();

				// Get and check SO_REUSEADDR
				if (Syscall.getsockopt (so1, UnixSocketProtocol.SOL_SOCKET, UnixSocketOptionName.SO_REUSEADDR, out value) < 0)
					UnixMarshal.ThrowExceptionForLastError ();
				Assert.AreNotEqual (0, value);

				// Set SO_REUSEADDR to 0
				if (Syscall.setsockopt (so1, UnixSocketProtocol.SOL_SOCKET, UnixSocketOptionName.SO_REUSEADDR, new byte[10], 4) < 0)
					UnixMarshal.ThrowExceptionForLastError ();

				// Get and check SO_REUSEADDR
				var buffer = new byte[15];
				long size = 12;
				if (Syscall.getsockopt (so1, UnixSocketProtocol.SOL_SOCKET, UnixSocketOptionName.SO_REUSEADDR, buffer, ref size) < 0)
					UnixMarshal.ThrowExceptionForLastError ();
				Assert.AreEqual (size, 4);
				for (int i = 0; i < size; i++)
					Assert.AreEqual (buffer[i], 0);
			});
		}

		[Test]
		public void SockOptLinger ()
		{
			WithSockets (UnixAddressFamily.AF_INET, UnixSocketType.SOCK_STREAM, UnixSocketProtocol.IPPROTO_TCP, (so1, so2) => {
				Linger linger = new Linger {
					l_onoff = 1,
					l_linger = 42,
				};
				// Set SO_LINGER
				if (Syscall.setsockopt (so1, UnixSocketProtocol.SOL_SOCKET, UnixSocketOptionName.SO_LINGER, linger) < 0)
					UnixMarshal.ThrowExceptionForLastError ();

				// Get and check SO_LINGER
				Linger value;
				if (Syscall.getsockopt (so1, UnixSocketProtocol.SOL_SOCKET, UnixSocketOptionName.SO_LINGER, out value) < 0)
					UnixMarshal.ThrowExceptionForLastError ();
				if (value.l_onoff == 0)
					Assert.Fail ("Linger not enabled");
				Assert.AreEqual (linger.l_linger, value.l_linger);
			});
		}

		[Test]
		public void Shutdown ()
		{
			WithSocketPair ((so1, so2) => {
				if (Syscall.shutdown (so1, ShutdownOption.SHUT_WR) < 0)
					UnixMarshal.ThrowExceptionForLastError ();

				var buffer2 = new byte[1024];
				long ret = Syscall.recv (so2, buffer2, (ulong) buffer2.Length, 0);
				if (ret < 0)
					UnixMarshal.ThrowExceptionForLastError ();

				Assert.AreEqual (ret, 0);
			});
		}
	}
}

// vim: noexpandtab
// Local Variables: 
// tab-width: 4
// c-basic-offset: 4
// indent-tabs-mode: t
// End: 
