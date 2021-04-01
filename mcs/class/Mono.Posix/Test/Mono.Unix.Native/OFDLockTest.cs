//
// Tests for FcntlCommand.F_OFD_{GETLK,SETLK,SETLKW}
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
	public class OFDLockTest {

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

		[Test]
		public void TestOFDLock ()
		{
			int fd1 = Syscall.open (TempFolder + "/testfile", OpenFlags.O_RDWR | OpenFlags.O_CREAT | OpenFlags.O_EXCL, FilePermissions.DEFFILEMODE);
			if (fd1 < 0)
				UnixMarshal.ThrowExceptionForLastError ();
			int fd2 = Syscall.open (TempFolder + "/testfile", OpenFlags.O_RDWR);
			if (fd2 < 0)
				UnixMarshal.ThrowExceptionForLastError ();
			int fd3 = Syscall.open (TempFolder + "/testfile", OpenFlags.O_RDWR);
			if (fd3 < 0)
				UnixMarshal.ThrowExceptionForLastError ();

			// Get read lock for first 100 bytes on fd1
			var flock1 = new Flock {
				l_type = LockType.F_RDLCK,
				l_whence = SeekFlags.SEEK_SET,
				l_start = 0,
				l_len = 100,
			};
			if (Syscall.fcntl (fd1, FcntlCommand.F_OFD_SETLKW, ref flock1) < 0) {
				// Old kernels and non-linux systems should return EINVAL
				if (Stdlib.GetLastError () == Errno.EINVAL)
					Assert.Ignore ("F_OFD_SETLKW does not seem to be supported.");
				UnixMarshal.ThrowExceptionForLastError ();
			}

			// Get read lock for first 100 bytes on fd2
			var flock2 = new Flock {
				l_type = LockType.F_RDLCK,
				l_whence = SeekFlags.SEEK_SET,
				l_start = 0,
				l_len = 100,
			};
			if (Syscall.fcntl (fd2, FcntlCommand.F_OFD_SETLK, ref flock2) < 0)
				UnixMarshal.ThrowExceptionForLastError ();

			// Get write lock for remaining bytes on fd1
			var flock3 = new Flock {
				l_type = LockType.F_WRLCK,
				l_whence = SeekFlags.SEEK_SET,
				l_start = 100,
				l_len = 0,
			};
			if (Syscall.fcntl (fd1, FcntlCommand.F_OFD_SETLK, ref flock3) < 0)
				UnixMarshal.ThrowExceptionForLastError ();

			// Close fd3, should not release lock
			if (Syscall.close (fd3) < 0)
				UnixMarshal.ThrowExceptionForLastError ();
			
			// Get lock status for byte 150 from fd2
			var flock4 = new Flock {
				l_type = LockType.F_RDLCK,
				l_whence = SeekFlags.SEEK_SET,
				l_start = 150,
				l_len = 1,
			};
			if (Syscall.fcntl (fd2, FcntlCommand.F_OFD_GETLK, ref flock4) < 0)
				UnixMarshal.ThrowExceptionForLastError ();
			// There should be a conflicting write lock
			Assert.AreEqual (LockType.F_WRLCK, flock4.l_type);

			// Get write byte 0 on fd1, should fail with EAGAIN
			var flock5 = new Flock {
				l_type = LockType.F_WRLCK,
				l_whence = SeekFlags.SEEK_SET,
				l_start = 0,
				l_len = 1,
			};
			var res = Syscall.fcntl (fd1, FcntlCommand.F_OFD_SETLK, ref flock5);
			Assert.AreEqual (-1, res);
			Assert.AreEqual (Errno.EAGAIN, Stdlib.GetLastError ());
			
			if (Syscall.close (fd1) < 0)
				UnixMarshal.ThrowExceptionForLastError ();
			if (Syscall.close (fd2) < 0)
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
