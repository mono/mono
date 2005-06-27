//
// ProtectedMemoryTest.cs - NUnit Test Cases for ProtectedMemory
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//

#if NET_2_0

using NUnit.Framework;

using System;
using System.Security.Cryptography;

namespace MonoTests.System.Security.Cryptography {

	[TestFixture]
	public class ProtectedMemoryTest {

		private void ProtectUnprotect (MemoryProtectionScope scope) 
		{
			byte[] data = new byte [16];
			ProtectedMemory.Protect (data, scope);
			int total = 0;
			for (int i=0; i < 16; i++)
				total += data [i];
			Assert.IsFalse ((total == 0), "Protect");

			ProtectedMemory.Unprotect (data, scope);
			total = 0;
			for (int i=0; i < 16; i++)
				total += data [i];
			Assert.IsTrue ((total == 0), "Unprotect");
		}

		[Test]
		public void ProtectSameProcess () 
		{
			try {
				// we're testing the MemoryProtectionScope definition but
				// not if it's really limited to the scope specified
				ProtectUnprotect (MemoryProtectionScope.SameProcess);
			}
			catch (PlatformNotSupportedException) {
				Assert.Ignore ("Only supported under Windows 2000 and later");
			}
		}

		[Test]
		public void ProtectSameLogon () 
		{
			try {
				// we're testing the MemoryProtectionScope definition but
				// not if it's really limited to the scope specified
				ProtectUnprotect (MemoryProtectionScope.SameLogon);
			}
			catch (PlatformNotSupportedException) {
				Assert.Ignore ("Only supported under Windows 2000 and later");
			}
		}

		[Test]
		public void ProtectCrossProcess () 
		{
			try {
				// we're testing the MemoryProtectionScope definition but
				// not if it's really limited to the scope specified
				ProtectUnprotect (MemoryProtectionScope.CrossProcess);
			}
			catch (PlatformNotSupportedException) {
				Assert.Ignore ("Only supported under Windows 2000 and later");
			}
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void ProtectBadDataLength () 
		{
			byte[] data = new byte [15];
			ProtectedMemory.Protect (data, MemoryProtectionScope.SameProcess);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ProtectNull () 
		{
			ProtectedMemory.Protect (null, MemoryProtectionScope.SameProcess);
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void UnprotectBadDataLength () 
		{
			byte[] data = new byte [15];
			ProtectedMemory.Unprotect (data, MemoryProtectionScope.SameProcess);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void UnprotectNull () 
		{
			ProtectedMemory.Unprotect (null, MemoryProtectionScope.SameProcess);
		}
	}
}

#endif