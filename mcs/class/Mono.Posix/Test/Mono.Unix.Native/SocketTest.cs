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
			Timeval timeout = new Timeval { tv_sec = 0, tv_usec = 500000 };
			if (Syscall.setsockopt (socket, SockProtocol.SOL_SOCKET, SockOptName.SO_RCVTIMEO, timeout) < 0)
				UnixMarshal.ThrowExceptionForLastError ();
			if (Syscall.setsockopt (socket, SockProtocol.SOL_SOCKET, SockOptName.SO_SNDTIMEO, timeout) < 0)
				UnixMarshal.ThrowExceptionForLastError ();
		}

		void WithSocketPair (Action<int, int> f)
		{
			var sockets = new int[2];
			if (Syscall.socketpair (AddrFamily.AF_UNIX, SockType.SOCK_STREAM, 0, sockets) < 0)
				UnixMarshal.ThrowExceptionForLastError ();
			try {
				SetTimeout (sockets[0]);
				SetTimeout (sockets[1]);

				f (sockets[0], sockets[1]);
			} finally {
				if (Syscall.close (sockets[0]) < 0)
					UnixMarshal.ThrowExceptionForLastError ();
				if (Syscall.close (sockets[1]) < 0)
					UnixMarshal.ThrowExceptionForLastError ();
			}
		}

		void WithSockets (AddrFamily af, SockType type, SockProtocol protocol, Action<int, int> f)
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
			if ((socket = Syscall.socket (AddrFamily.AF_UNIX, SockType.SOCK_STREAM, 0)) < 0)
				UnixMarshal.ThrowExceptionForLastError ();
			
			if (Syscall.close (socket) < 0)
				UnixMarshal.ThrowExceptionForLastError ();
		}

		[Test]
		public void SocketPair ()
		{
			var sockets = new int[2];
			if (Syscall.socketpair (AddrFamily.AF_UNIX, SockType.SOCK_STREAM, 0, sockets) < 0)
				UnixMarshal.ThrowExceptionForLastError ();
			
			if (Syscall.close (sockets[0]) < 0)
				UnixMarshal.ThrowExceptionForLastError ();
			if (Syscall.close (sockets[1]) < 0)
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
			WithSocketPair ((so1, so2) => {
				// Set SO_PASSCRED to 1
				if (Syscall.setsockopt (so1, SockProtocol.SOL_SOCKET, SockOptName.SO_PASSCRED, 1) < 0)
					UnixMarshal.ThrowExceptionForLastError ();
				
				// Get and check SO_PASSCRED
				int value;
				if (Syscall.getsockopt (so1, SockProtocol.SOL_SOCKET, SockOptName.SO_PASSCRED, out value) < 0)
					UnixMarshal.ThrowExceptionForLastError ();
				Assert.AreEqual (value, 1);

				// Set SO_PASSCRED to 0
				if (Syscall.setsockopt (so1, SockProtocol.SOL_SOCKET, SockOptName.SO_PASSCRED, new byte[10], 4) < 0)
					UnixMarshal.ThrowExceptionForLastError ();

				// Get and check SO_PASSCRED
				var buffer = new byte[15];
				long size = 12;
				if (Syscall.getsockopt (so1, SockProtocol.SOL_SOCKET, SockOptName.SO_PASSCRED, buffer, ref size) < 0)
					UnixMarshal.ThrowExceptionForLastError ();
				Assert.AreEqual (size, 4);
				for (int i = 0; i < size; i++)
					Assert.AreEqual (buffer[i], 0);
				});
		}

		[Test]
		public void SockOptLinger ()
		{
			WithSockets (AddrFamily.AF_INET, SockType.SOCK_STREAM, SockProtocol.IPPROTO_TCP, (so1, so2) => {
				Linger linger = new Linger {
					l_onoff = 1,
					l_linger = 42,
				};
				// Set SO_LINGER
				if (Syscall.setsockopt (so1, SockProtocol.SOL_SOCKET, SockOptName.SO_LINGER, linger) < 0)
					UnixMarshal.ThrowExceptionForLastError ();
				
				// Get and check SO_LINGER
				Linger value;
				if (Syscall.getsockopt (so1, SockProtocol.SOL_SOCKET, SockOptName.SO_LINGER, out value) < 0)
					UnixMarshal.ThrowExceptionForLastError ();
				Assert.AreEqual (linger, value);
				});
		}

		[Test]
		public void BindConnect ()
		{
			WithSockets (AddrFamily.AF_INET, SockType.SOCK_DGRAM, SockProtocol.IPPROTO_UDP, (so1, so2) => {
				// Bind UDP socket so1 to 127.0.0.1 with dynamic port
				var address = new SockaddrIn { sin_family = AddrFamily.AF_INET, sin_port = 0, sin_addr = new InAddr (127, 0, 0, 1) };
				if (Syscall.bind (so1, address) < 0)
					UnixMarshal.ThrowExceptionForLastError ();

				var buffer = address.ToNativeArray ();
				Assert.AreEqual (Sockaddr.FromNativeArray (buffer).sa_family, AddrFamily.AF_INET);

				Assert.LessOrEqual (SockaddrIn.Size, Sockaddr.SizeofSockaddrStorage);

				// Get actual port number using getsockname()
				var actualAddress = new byte[Sockaddr.SizeofSockaddrStorage];
				long len = actualAddress.Length;
				if (Syscall.getsockname (so1, actualAddress, ref len) < 0)
					UnixMarshal.ThrowExceptionForLastError ();
				Assert.AreEqual (len, SockaddrIn.Size);
				Assert.AreEqual (Sockaddr.FromNativeArray (actualAddress).sa_family, AddrFamily.AF_INET);
				var port = SockaddrIn.FromNativeArray (actualAddress).sin_port;
				Assert.IsTrue (port != 0);

				
				// Connect so2 to so1
				var remoteAddress = new SockaddrIn { sin_family = AddrFamily.AF_INET, sin_port = port, sin_addr = new InAddr (127, 0, 0, 1) };
				if (Syscall.connect (so2, remoteAddress) < 0)
					UnixMarshal.ThrowExceptionForLastError ();

				// Verify peer address using getpeername()
				var address2Buf = new byte[Sockaddr.SizeofSockaddrStorage];
				len = address2Buf.Length;
				if (Syscall.getpeername (so2, address2Buf, ref len) < 0)
					UnixMarshal.ThrowExceptionForLastError ();
				Assert.AreEqual (len, SockaddrIn.Size);
				Assert.AreEqual (Sockaddr.FromNativeArray (address2Buf).sa_family, AddrFamily.AF_INET);
				var address2 = SockaddrIn.FromNativeArray (address2Buf);
				Assert.AreEqual (remoteAddress, address2);
				
				// Send and receive a few bytes
				long ret;
				var buffer1 = new byte[] { 42, 43, 44 };
				ret = Syscall.send (so2, buffer1, (ulong) buffer1.Length, 0);
				if (ret < 0)
					UnixMarshal.ThrowExceptionForLastError ();
				
				var buffer2 = new byte[1024];
				ret = Syscall.recv (so1, buffer2, (ulong) buffer2.Length, 0);
				if (ret < 0)
					UnixMarshal.ThrowExceptionForLastError ();
				
				Assert.AreEqual (buffer1.Length, ret);
				for (int i = 0; i < buffer1.Length; i++)
					Assert.AreEqual (buffer1[i], buffer2[i]);
				});
		}
		
		[Test]
		public void IPv6 ()
		{
			var address = new SockaddrIn6 { sin6_family = AddrFamily.AF_INET6, sin6_port = 1234, sin6_flowinfo = 2, sin6_addr = new In6Addr (), sin6_scope_id = 3 };
			var buffer = address.ToNativeArray ();
			var address2 = SockaddrIn6.FromNativeArray (buffer);
			Assert.AreEqual (address, address2);
		}
		
		[Test]
		public void UnixAccept ()
		{
			var address = new SockaddrUn (TempFolder + "/socket1");
			var buffer = address.ToNativeArray ();
			var address2 = SockaddrUn.FromNativeArray (buffer);
			Assert.AreEqual (address, address2);

			WithSockets (AddrFamily.AF_UNIX, SockType.SOCK_STREAM, 0, (so1, so2) => {
				if (Syscall.bind (so1, address) < 0)
					UnixMarshal.ThrowExceptionForLastError ();

				if (Syscall.listen (so1, 5) < 0)
					UnixMarshal.ThrowExceptionForLastError ();
				
				if (Syscall.connect (so2, address) < 0)
					UnixMarshal.ThrowExceptionForLastError ();

				int so3;
				var remoteBuf = new byte[Sockaddr.SizeofSockaddrStorage];
				long len = remoteBuf.Length;
				if ((so3 = Syscall.accept (so1, remoteBuf, ref len)) < 0)
					UnixMarshal.ThrowExceptionForLastError ();
				try {
					// Send and receive a few bytes
					long ret;
					var buffer1 = new byte[] { 42, 43, 44 };
					ret = Syscall.send (so2, buffer1, (ulong) buffer1.Length, 0);
					if (ret < 0)
						UnixMarshal.ThrowExceptionForLastError ();
				
					var buffer2 = new byte[1024];
					ret = Syscall.recv (so3, buffer2, (ulong) buffer2.Length, 0);
					if (ret < 0)
						UnixMarshal.ThrowExceptionForLastError ();
				
					Assert.AreEqual (buffer1.Length, ret);
					for (int i = 0; i < buffer1.Length; i++)
						Assert.AreEqual (buffer1[i], buffer2[i]);
				} finally {
					if (Syscall.close (so3) < 0)
						UnixMarshal.ThrowExceptionForLastError ();
				}
				});
		}
		
		[Test]
		public void Accept4 ()
		{
			WithSockets (AddrFamily.AF_UNIX, SockType.SOCK_STREAM, 0, (so1, so2) => {
				var address = new SockaddrUn (TempFolder + "/socket2");
				if (Syscall.bind (so1, address) < 0)
					UnixMarshal.ThrowExceptionForLastError ();
				if (Syscall.listen (so1, 5) < 0)
					UnixMarshal.ThrowExceptionForLastError ();
				if (Syscall.connect (so2, address) < 0)
					UnixMarshal.ThrowExceptionForLastError ();

				int so3;
				var remoteBuf = new byte[Sockaddr.SizeofSockaddrStorage];
				long len = remoteBuf.Length;
				if ((so3 = Syscall.accept4 (so1, remoteBuf, ref len, SockFlags.SOCK_CLOEXEC | SockFlags.SOCK_NONBLOCK)) < 0)
					UnixMarshal.ThrowExceptionForLastError ();
				try {
					int _flags;
					if ((_flags = Syscall.fcntl (so3, FcntlCommand.F_GETFL)) < 0)
						UnixMarshal.ThrowExceptionForLastError ();
					var flags = NativeConvert.ToOpenFlags (_flags);
					Assert.IsTrue ((flags & OpenFlags.O_NONBLOCK) != 0);

					int _flagsFD;
					if ((_flagsFD = Syscall.fcntl (so3, FcntlCommand.F_GETFD)) < 0)
						UnixMarshal.ThrowExceptionForLastError ();
					// FD_CLOEXEC must be set
					//var flagsFD = NativeConvert.ToFdFlags (_flagsFD);
					//Assert.IsTrue ((flagsFD & FdFlags.FD_CLOEXEC) != 0);
					Assert.IsTrue (_flagsFD != 0);
				} finally {
					if (Syscall.close (so3) < 0)
						UnixMarshal.ThrowExceptionForLastError ();
				}
				});
		}

		[Test]
		public void SendToRecvFrom ()
		{
			WithSockets (AddrFamily.AF_INET, SockType.SOCK_DGRAM, SockProtocol.IPPROTO_UDP, (so1, so2) => {
				// Bind UDP socket so1 to 127.0.0.1 with dynamic port
				var address = new SockaddrIn { sin_family = AddrFamily.AF_INET, sin_port = 0, sin_addr = new InAddr (127, 0, 0, 1) };
				if (Syscall.bind (so1, address) < 0)
					UnixMarshal.ThrowExceptionForLastError ();

				// Get actual port number using getsockname()
				var actualAddress = new byte[Sockaddr.SizeofSockaddrStorage];
				long len = actualAddress.Length;
				if (Syscall.getsockname (so1, actualAddress, ref len) < 0)
					UnixMarshal.ThrowExceptionForLastError ();
				Assert.AreEqual (len, SockaddrIn.Size);
				Assert.AreEqual (Sockaddr.FromNativeArray (actualAddress).sa_family, AddrFamily.AF_INET);
				var port = SockaddrIn.FromNativeArray (actualAddress).sin_port;
				Assert.IsTrue (port != 0);


				var remoteAddress = new SockaddrIn { sin_family = AddrFamily.AF_INET, sin_port = port, sin_addr = new InAddr (127, 0, 0, 1) };
				
				// Send and receive a few bytes
				long ret;
				var buffer1 = new byte[] { 42, 43, 44 };
				ret = Syscall.sendto (so2, buffer1, (ulong) buffer1.Length, 0, remoteAddress);
				if (ret < 0)
					UnixMarshal.ThrowExceptionForLastError ();

				var senderAddressBuf = new byte[Sockaddr.SizeofSockaddrStorage];
				len = senderAddressBuf.Length;
				var buffer2 = new byte[1024];
				ret = Syscall.recvfrom (so1, buffer2, (ulong) buffer2.Length, 0, senderAddressBuf, ref len);
				if (ret < 0)
					UnixMarshal.ThrowExceptionForLastError ();
				Assert.AreEqual (len, SockaddrIn.Size);
				Assert.AreEqual (Sockaddr.FromNativeArray (senderAddressBuf).sa_family, AddrFamily.AF_INET);
				var senderAddress = SockaddrIn.FromNativeArray (senderAddressBuf);
				Assert.AreEqual (senderAddress.sin_addr, new InAddr (127, 0, 0, 1));
				
				Assert.AreEqual (buffer1.Length, ret);
				for (int i = 0; i < buffer1.Length; i++)
					Assert.AreEqual (buffer1[i], buffer2[i]);
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

		[Test]
		public unsafe void SendMsgRecvMsg ()
		{
			WithSocketPair ((so1, so2) => {
				long ret;
				var buffer1 = new byte[] { 42, 43, 44 };
				fixed (byte* ptr_buffer1 = buffer1) {
					var iovecs1 = new Iovec[] {
						new Iovec { iov_base = (IntPtr) ptr_buffer1, iov_len = (ulong) buffer1.Length },
					};
					var msghdr1 = new Msghdr { msg_iov = iovecs1, msg_iovlen = 1 };
					ret = Syscall.sendmsg (so1, msghdr1, 0);
				}
				if (ret < 0)
					UnixMarshal.ThrowExceptionForLastError ();
				
				var buffer2 = new byte[1024];
				fixed (byte* ptr_buffer2 = buffer2) {
					var iovecs2 = new Iovec[] {
						new Iovec { iov_base = (IntPtr) ptr_buffer2, iov_len = (ulong) buffer2.Length },
					};
					var msghdr2 = new Msghdr { msg_iov = iovecs2, msg_iovlen = 1 };
					ret = Syscall.recvmsg (so2, msghdr2, 0);
				}
				if (ret < 0)
					UnixMarshal.ThrowExceptionForLastError ();
				
				Assert.AreEqual (buffer1.Length, ret);
				for (int i = 0; i < buffer1.Length; i++)
					Assert.AreEqual (buffer1[i], buffer2[i]);
				});
		}

		[Test]
		public unsafe void ControlMsg ()
		{
			// Create two socket pairs and send inner_so1 and inner_so2 over the other socket pair using SCM_RIGHTS
			WithSocketPair ((inner_so1, inner_so2) => {
				WithSocketPair ((so1, so2) => {
					// Create two SCM_RIGHTS control messages
					var cmsg = new byte[2 * Syscall.CMSG_SPACE (sizeof (int))];
					var hdr = new Cmsghdr {
						cmsg_len = (long) Syscall.CMSG_LEN (sizeof (int)),
						cmsg_level = SockProtocol.SOL_SOCKET,
						cmsg_type = SockCtrlMsg.SCM_RIGHTS,
					};
					var msghdr1 = new Msghdr { msg_control = cmsg, msg_controllen = cmsg.Length };
					long offset = 0;
					hdr.SetTo (msghdr1, offset);
					var dataOffset = Syscall.CMSG_DATA (msghdr1, offset);
					fixed (byte* ptr = msghdr1.msg_control) {
						((int*) (ptr + dataOffset))[0] = inner_so1;
					}
					offset = (long) Syscall.CMSG_SPACE (sizeof (int));
					hdr.SetTo (msghdr1, offset);
					dataOffset = Syscall.CMSG_DATA (msghdr1, offset);
					fixed (byte* ptr = msghdr1.msg_control) {
						((int*) (ptr + dataOffset))[0] = inner_so2;
					}
					
					long ret;
					var buffer1 = new byte[] { 42, 43, 44 };
					fixed (byte* ptr_buffer1 = buffer1) {
						var iovecs1 = new Iovec[] {
							new Iovec { iov_base = (IntPtr) ptr_buffer1, iov_len = (ulong) buffer1.Length },
						};
						msghdr1.msg_iov = iovecs1;
						msghdr1.msg_iovlen = 1;
						// Send message twice
						ret = Syscall.sendmsg (so1, msghdr1, 0);
						if (ret < 0)
							UnixMarshal.ThrowExceptionForLastError ();
						ret = Syscall.sendmsg (so1, msghdr1, 0);
						if (ret < 0)
							UnixMarshal.ThrowExceptionForLastError ();
					}

					// Receive without control message buffer
					var buffer2 = new byte[1024];
					var msghdr2 = new Msghdr { };
					fixed (byte* ptr_buffer2 = buffer2) {
						var iovecs2 = new Iovec[] {
							new Iovec { iov_base = (IntPtr) ptr_buffer2, iov_len = (ulong) buffer2.Length },
						};
						msghdr2.msg_iov = iovecs2;
						msghdr2.msg_iovlen = 1;
						ret = Syscall.recvmsg (so2, msghdr2, 0);
					}
					if (ret < 0)
						UnixMarshal.ThrowExceptionForLastError ();

					Assert.IsTrue ((msghdr2.msg_flags & MessageFlags.MSG_CTRUNC) != 0); // Control message has been truncated
					
					Assert.AreEqual (buffer1.Length, ret);
					for (int i = 0; i < buffer1.Length; i++)
						Assert.AreEqual (buffer1[i], buffer2[i]);
					
					// Receive with control message buffer
					buffer2 = new byte[1024];
					var cmsg2 = new byte[1024];
					msghdr2 = new Msghdr { msg_control = cmsg2, msg_controllen = cmsg2.Length };
					fixed (byte* ptr_buffer2 = buffer2) {
						var iovecs2 = new Iovec[] {
							new Iovec { iov_base = (IntPtr) ptr_buffer2, iov_len = (ulong) buffer2.Length },
						};
						msghdr2.msg_iov = iovecs2;
						msghdr2.msg_iovlen = 1;
						ret = Syscall.recvmsg (so2, msghdr2, 0);
					}
					if (ret < 0)
						UnixMarshal.ThrowExceptionForLastError ();

					var fds = new global::System.Collections.Generic.List<int> ();
					for (offset = Syscall.CMSG_FIRSTHDR (msghdr2); offset != -1; offset = Syscall.CMSG_NXTHDR (msghdr2, offset)) {
						var recvHdr = Cmsghdr.Get (msghdr2, offset);
						var recvDataOffset = Syscall.CMSG_DATA (msghdr2, offset);
						var bytes = recvHdr.cmsg_len - (recvDataOffset - offset);
						Assert.AreEqual (bytes % sizeof (int), 0);
						var fdCount = bytes / sizeof (int);
						fixed (byte* ptr = msghdr2.msg_control)
							for (int i = 0; i < fdCount; i++)
								fds.Add (((int*) (ptr + recvDataOffset))[i]);
					}
					try {
						Assert.IsTrue ((msghdr2.msg_flags & MessageFlags.MSG_CTRUNC) == 0); // Control message has not been truncated

						Assert.AreEqual (buffer1.Length, ret);
						for (int i = 0; i < buffer1.Length; i++)
							Assert.AreEqual (buffer1[i], buffer2[i]);

						Assert.AreEqual (fds.Count, 2);

						// Send message over the first received fd and receive it over inner_so2
						var buffer3 = new byte[] { 16, 17 };
						ret = Syscall.send (fds[0], buffer3, (ulong) buffer3.Length, 0);
						if (ret < 0)
							UnixMarshal.ThrowExceptionForLastError ();
				
						var buffer4 = new byte[1024];
						ret = Syscall.recv (inner_so2, buffer4, (ulong) buffer4.Length, 0);
						if (ret < 0)
							UnixMarshal.ThrowExceptionForLastError ();
				
						Assert.AreEqual (buffer3.Length, ret);
						for (int i = 0; i < buffer3.Length; i++)
							Assert.AreEqual (buffer3[i], buffer4[i]);

						// Send message over inner_so1 and receive it second received fd
						var buffer5 = new byte[] { 10, 40, 0, 1 };
						ret = Syscall.send (inner_so1, buffer5, (ulong) buffer5.Length, 0);
						if (ret < 0)
							UnixMarshal.ThrowExceptionForLastError ();
				
						var buffer6 = new byte[1024];
						ret = Syscall.recv (fds[1], buffer6, (ulong) buffer6.Length, 0);
						if (ret < 0)
							UnixMarshal.ThrowExceptionForLastError ();
				
						Assert.AreEqual (buffer5.Length, ret);
						for (int i = 0; i < buffer5.Length; i++)
							Assert.AreEqual (buffer5[i], buffer6[i]);
					} finally {
						foreach (var fd in fds)
							if (Syscall.close (fd) < 0)
								UnixMarshal.ThrowExceptionForLastError ();
					}
					});
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
