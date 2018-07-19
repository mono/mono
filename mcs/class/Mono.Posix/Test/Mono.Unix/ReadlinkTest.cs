//
// readlink() / readlinkat() Test Cases
//
// Authors:
//  Steffen Kiess (s-kiess@web.de)
//
// Copyright (C) 2013 Steffen Kiess
//

using System;
using System.IO;
using System.Text;

using Mono.Unix;
using Mono.Unix.Native;

using NUnit.Framework;

namespace MonoTests.Mono.Unix
{
	[TestFixture, Category ("NotDotNet"), Category ("NotOnWindows")]
	public class ReadlinkTest {

		static string[] Targets = {
			// Simple test cases
			"a",
			"test",
			// With non-ASCII characters
			"ä",
			"test ö test",
			// With non-UTF8 bytes
			UnixEncoding.Instance.GetString (new byte[] {0xff, 0x80, 0x41, 0x80}),
			// Size is roughly initial size of buffer
			new string ('a', 255),
			new string ('a', 256),
			new string ('a', 257),
			// With non-ASCII characters, size is roughly initial size of buffer
			"ä" + new string ('a', 253), // 254 chars, 255 bytes
			"ä" + new string ('a', 254), // 255 chars, 256 bytes
			"ä" + new string ('a', 255), // 256 chars, 257 bytes
			"ä" + new string ('a', 256), // 257 chars, 258 bytes
			new string ('a', 253) + "ä", // 254 chars, 255 bytes
			new string ('a', 254) + "ä", // 255 chars, 256 bytes
			new string ('a', 255) + "ä", // 256 chars, 257 bytes
			new string ('a', 256) + "ä", // 257 chars, 258 bytes
			// With non-UTF8 bytes, size is roughly initial size of buffer
			"\0\u00ff" + new string ('a', 253), // 255 chars, 254 bytes
			"\0\u00ff" + new string ('a', 254), // 256 chars, 255 bytes
			"\0\u00ff" + new string ('a', 255), // 257 chars, 256 bytes
			"\0\u00ff" + new string ('a', 256), // 258 chars, 257 bytes
			new string ('a', 253) + "\0\u00ff", // 255 chars, 254 bytes
			new string ('a', 254) + "\0\u00ff", // 256 chars, 255 bytes
			new string ('a', 255) + "\0\u00ff", // 257 chars, 256 bytes
			new string ('a', 256) + "\0\u00ff", // 258 chars, 257 bytes
		};

		bool HaveReadlinkAt;
		string TempFolder;
		int TempFD;

		[SetUp]
		public void SetUp ()
		{
			HaveReadlinkAt = false;
			try {
				Syscall.readlinkat (-1, "", new byte[1]);
				HaveReadlinkAt = true;
			} catch (EntryPointNotFoundException) {
			}


			TempFolder = Path.Combine (Path.GetTempPath (), this.GetType ().FullName);

			if (Directory.Exists (TempFolder))
				//Directory.Delete (TempFolder, true); // Fails for long link target paths
				new UnixDirectoryInfo (TempFolder).Delete (true);

			Directory.CreateDirectory (TempFolder);

			TempFD = Syscall.open (TempFolder, OpenFlags.O_RDONLY | OpenFlags.O_DIRECTORY);
			if (TempFD < 0)
				UnixMarshal.ThrowExceptionForLastError ();
		}

		[TearDown]
		public void TearDown()
		{
			if (Syscall.close (TempFD) < 0)
				UnixMarshal.ThrowExceptionForLastError ();

			if (Directory.Exists (TempFolder))
				//Directory.Delete (TempFolder, true); // Fails for long link target paths
				new UnixDirectoryInfo (TempFolder).Delete (true);
		}

		void CreateLink (string s)
		{
				string link = UnixPath.Combine (TempFolder, "link");

				//File.Delete (link); // Fails for long link target paths
				if (Syscall.unlink (link) < 0 && Stdlib.GetLastError () != Errno.ENOENT)
					UnixMarshal.ThrowExceptionForLastError ();

				if (Syscall.symlink (s, link) < 0)
					UnixMarshal.ThrowExceptionForLastError ();
		}

		[Test]
		public void ReadLink ()
		{
			foreach (string s in Targets) {
				string link = UnixPath.Combine (TempFolder, "link");

				CreateLink (s);

				var target = UnixPath.ReadLink (link);
				Assert.AreEqual (s, target);
			}
		}

		[Test]
		public void ReadLinkAt ()
		{
			if (!HaveReadlinkAt)
				Assert.Ignore ("No ReadlinkAt.");

			foreach (string s in Targets) {
				CreateLink (s);

				var target = UnixPath.ReadLinkAt (TempFD, "link");
				Assert.AreEqual (s, target);
			}
		}

		[Test]
		public void TryReadLink ()
		{
			foreach (string s in Targets) {
				string link = UnixPath.Combine (TempFolder, "link");

				CreateLink (s);

				var target = UnixPath.TryReadLink (link);
				Assert.AreEqual (s, target);
			}
		}

		[Test]
		public void TryReadLinkAt ()
		{
			if (!HaveReadlinkAt)
				Assert.Ignore ("No ReadlinkAt.");

			foreach (string s in Targets) {
				CreateLink (s);

				var target = UnixPath.TryReadLinkAt (TempFD, "link");
				Assert.AreEqual (s, target);
			}
		}

		[Test]
		public void readlink_byte ()
		{
			foreach (string s in Targets) {
				string link = UnixPath.Combine (TempFolder, "link");

				CreateLink (s);

				string target = null;
				byte[] buf = new byte[256];
				do {
					long r = Syscall.readlink (link, buf);
					if (r < 0)
						UnixMarshal.ThrowExceptionForLastError ();
					Assert.That(buf.Length, Is.GreaterThanOrEqualTo(r));
					if (r == buf.Length)
						buf = new byte[checked (buf.Length * 2)];
					else
						target = UnixEncoding.Instance.GetString (buf, 0, checked ((int) r));
				} while (target == null);

				Assert.AreEqual (s, target);
			}
		}

		[Test]
		public void readlinkat_byte ()
		{
			if (!HaveReadlinkAt)
				Assert.Ignore ("No ReadlinkAt.");

			foreach (string s in Targets) {
				CreateLink (s);

				string target = null;
				byte[] buf = new byte[256];
				do {
					long r = Syscall.readlinkat (TempFD, "link", buf);
					if (r < 0)
						UnixMarshal.ThrowExceptionForLastError ();
					Assert.That(buf.Length, Is.GreaterThanOrEqualTo(r));
					if (r == buf.Length)
						buf = new byte[checked (buf.Length * 2)];
					else
						target = UnixEncoding.Instance.GetString (buf, 0, checked ((int) r));
				} while (target == null);

				Assert.AreEqual (s, target);
			}
		}

		[Test]
		public void readlink_char ()
		{
			foreach (string s in Targets) {
				string link = UnixPath.Combine (TempFolder, "link");

				CreateLink (s);

				var sb = new StringBuilder (256);
				do {
					int oldCapacity = sb.Capacity;
					int r = Syscall.readlink (link, sb);
					Assert.AreEqual (oldCapacity, sb.Capacity);
					if (r < 0)
						UnixMarshal.ThrowExceptionForLastError ();
					Assert.AreEqual (r, sb.Length);
					Assert.That(sb.Capacity, Is.GreaterThanOrEqualTo(r));
					if (r == sb.Capacity)
						checked { sb.Capacity *= 2; }
					else
						break;
				} while (true);
				var target = sb.ToString ();

				Assert.AreEqual (s, target);
			}
		}

		[Test]
		public void readlinkat_char ()
		{
			if (!HaveReadlinkAt)
				Assert.Ignore ("No ReadlinkAt.");

			foreach (string s in Targets) {
				CreateLink (s);

				var sb = new StringBuilder (256);
				do {
					int oldCapacity = sb.Capacity;
					int r = Syscall.readlinkat (TempFD, "link", sb);
					Assert.AreEqual (oldCapacity, sb.Capacity);
					if (r < 0)
						UnixMarshal.ThrowExceptionForLastError ();
					Assert.AreEqual (r, sb.Length);
					Assert.That(sb.Capacity, Is.GreaterThanOrEqualTo(r));
					if (r == sb.Capacity)
						checked { sb.Capacity *= 2; }
					else
						break;
				} while (true);
				var target = sb.ToString ();

				Assert.AreEqual (s, target);
			}
		}

		[Test]
		public void ReadlinkMultiByteChar ()
		{
			string link = UnixPath.Combine (TempFolder, "link");

			CreateLink ("á");

			var sb = new StringBuilder (2);
			int res = Syscall.readlink (link, sb);
			if (res < 0)
				UnixMarshal.ThrowExceptionForLastError ();

			Assert.AreEqual (res, 2);
			Assert.AreEqual (sb.Length, 2);
			Assert.AreEqual (sb.Capacity, 2);
			Assert.AreEqual (sb.ToString (), "á\u0000");
		}
	}
}
