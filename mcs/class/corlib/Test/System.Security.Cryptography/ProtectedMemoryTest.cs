//
// ProtectedMemoryTest.cs - NUnit Test Cases for ProtectedMemory
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

#if NET_2_0

using NUnit.Framework;

using System;
using System.Security.Cryptography;

namespace MonoTests.System.Security.Cryptography {

	// References:
	// a.	

	[TestFixture]
	public class ProtectedMemoryTest : Assertion {

		private void ProtectUnprotect (MemoryProtectionScope scope) 
		{
			byte[] data = new byte [16];
			ProtectedMemory.Protect (data, scope);
			int total = 0;
			for (int i=0; i < 16; i++)
				total += data [i];
			Assert ("Protect", (total != 0));

			ProtectedMemory.Unprotect (data, scope);
			total = 0;
			for (int i=0; i < 16; i++)
				total += data [i];
			Assert ("Unprotect", (total == 0));
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
				Console.WriteLine ("Only supported under Windows XP and later");
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
				Console.WriteLine ("Only supported under Windows XP and later");
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
				Console.WriteLine ("Only supported under Windows XP and later");
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