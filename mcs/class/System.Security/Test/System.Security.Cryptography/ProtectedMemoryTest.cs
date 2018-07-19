//
// ProtectedMemoryTest.cs - NUnit Test Cases for ProtectedMemory
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//
#if !MOBILE

using NUnit.Framework;

using System;
using System.Security.Cryptography;

namespace MonoTests.System.Security.Cryptography {

	[TestFixture]
	public class ProtectedMemoryTest {

		private bool IsEmpty (byte[] array)
		{
			int total = 0;
			for (int i = 0; i < array.Length; i++)
				total += array [i];
			return (total == 0);
		}

		private void ProtectUnprotect (MemoryProtectionScope scope) 
		{
			try {
				byte[] data = new byte [16];
				ProtectedMemory.Protect (data, scope);
				Assert.IsFalse (IsEmpty (data), "Protect");

				ProtectedMemory.Unprotect (data, scope);
				Assert.IsTrue (IsEmpty (data), "Unprotect");
			}
			catch (PlatformNotSupportedException) {
				Assert.Ignore ("Only supported under Windows 2000 SP3 and later");
			}
		}

		[Test]
		public void ProtectSameProcess () 
		{
			// we're testing the MemoryProtectionScope definition but
			// not if it's really limited to the scope specified
			ProtectUnprotect (MemoryProtectionScope.SameProcess);
		}

		[Test]
		public void ProtectSameLogon () 
		{
			// we're testing the MemoryProtectionScope definition but
			// not if it's really limited to the scope specified
			ProtectUnprotect (MemoryProtectionScope.SameLogon);
		}

		[Test]
		public void ProtectCrossProcess () 
		{
			// we're testing the MemoryProtectionScope definition but
			// not if it's really limited to the scope specified
			ProtectUnprotect (MemoryProtectionScope.CrossProcess);
		}

		[Test]
		public void MemoryProtectionScope_All ()
		{
			byte[] data = new byte[16];
			try {
				foreach (MemoryProtectionScope mps in Enum.GetValues (typeof (MemoryProtectionScope))) {
					ProtectedMemory.Protect (data, mps);
					Assert.IsFalse (IsEmpty (data), "Protect");
					ProtectedMemory.Unprotect (data, mps);
					Assert.IsTrue (IsEmpty (data), "Unprotect");
				}
			}
			catch (PlatformNotSupportedException) {
				Assert.Ignore ("Only supported under Windows 2000 SP3 and later");
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Protect_InvalidMemoryProtectionScope ()
		{
			byte[] data = new byte[16];
			ProtectedMemory.Protect (data, (MemoryProtectionScope) Int32.MinValue);
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void ProtectBadDataLength () 
		{
			byte[] data = new byte [15];
			try {
				ProtectedMemory.Protect (data, MemoryProtectionScope.SameProcess);
			}
			catch (PlatformNotSupportedException) {
				Assert.Ignore ("Only supported under Windows 2000 SP3 and later");
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ProtectNull () 
		{
			ProtectedMemory.Protect (null, MemoryProtectionScope.SameProcess);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Unprotect_InvalidMemoryProtectionScope ()
		{
			byte[] data = new byte[16];
			ProtectedMemory.Unprotect (data, (MemoryProtectionScope) Int32.MinValue);
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void UnprotectBadDataLength () 
		{
			byte[] data = new byte [15];
			try {
				ProtectedMemory.Unprotect (data, MemoryProtectionScope.SameProcess);
			}
			catch (PlatformNotSupportedException) {
				Assert.Ignore ("Only supported under Windows 2000 SP3 and later");
			}
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
