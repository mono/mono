//
// Tests for memfd_create and file sealing
//
// Authors:
//  Steffen Kiess (kiess@ki4.de)
//
// Copyright (C) 2019 Steffen Kiess
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
	[TestFixture, Category ("NotDotNet"), Category ("NotOnWindows"), Category ("NotOnMac")]
	public class MemfdTest {
		[Test]
		public unsafe void TestMemfd ()
		{
			int fd;
			try {
				fd = Syscall.memfd_create ("mono-test", 0);
			} catch (EntryPointNotFoundException) {
				Assert.Ignore ("memfd_create() not available");
				return;
			}
			if (fd < 0 && Stdlib.GetLastError () == Errno.ENOSYS)
				// Might happen on a new libc + old kernel
				Assert.Ignore ("memfd_create() returns ENOSYS");
			if (fd < 0)
				UnixMarshal.ThrowExceptionForLastError ();

			byte b = 42;
			if (Syscall.write (fd, &b, 1) < 0)
				UnixMarshal.ThrowExceptionForLastError ();

			// Should fail with EPERM because MFD_ALLOW_SEALING was not used
			var res = Syscall.fcntl(fd, FcntlCommand.F_ADD_SEALS, SealType.F_SEAL_WRITE);
			Assert.AreEqual (-1, res);
			Assert.AreEqual (Errno.EPERM, Stdlib.GetLastError ());

			//Stdlib.system ("ls -l /proc/$PPID/fd/");

			if (Syscall.close (fd) < 0)
				UnixMarshal.ThrowExceptionForLastError ();

			// Call memfd_create with MFD_ALLOW_SEALING
			fd = Syscall.memfd_create ("mono-test", MemfdFlags.MFD_CLOEXEC | MemfdFlags.MFD_ALLOW_SEALING);
			if (fd < 0)
				UnixMarshal.ThrowExceptionForLastError ();

			if (Syscall.write (fd, &b, 1) < 0)
				UnixMarshal.ThrowExceptionForLastError ();

			res = Syscall.fcntl(fd, FcntlCommand.F_GET_SEALS);
			if (res < 0)
				UnixMarshal.ThrowExceptionForLastError ();
			// Need to convert the result to SealType
			SealType sealType = NativeConvert.ToSealType (res);
			Assert.AreEqual ((SealType)0, sealType);

			if (Syscall.fcntl(fd, FcntlCommand.F_ADD_SEALS, SealType.F_SEAL_WRITE) < 0)
				UnixMarshal.ThrowExceptionForLastError ();

			// Should fail with EPERM because the file was sealed for writing
			var lres = Syscall.write (fd, &b, 1);
			Assert.AreEqual (-1, lres);
			Assert.AreEqual (Errno.EPERM, Stdlib.GetLastError ());

			res = Syscall.fcntl(fd, FcntlCommand.F_GET_SEALS);
			if (res < 0)
				UnixMarshal.ThrowExceptionForLastError ();
			// Need to convert the result to SealType
			sealType = NativeConvert.ToSealType (res);
			Assert.AreEqual (SealType.F_SEAL_WRITE, sealType);

			//Stdlib.system ("ls -l /proc/$PPID/fd/");

			if (Syscall.close (fd) < 0)
				UnixMarshal.ThrowExceptionForLastError ();
		}
	}
}

// vim: noexpandtab
// Local Variables:
// tab-width: 4
// c-basic-offset: 4
// indent-tabs-mode: t
// End:
