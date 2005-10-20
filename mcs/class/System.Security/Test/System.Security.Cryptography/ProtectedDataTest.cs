//
// ProtectedDataTest.cs - NUnit Test Cases for ProtectedData
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
	public class ProtectedDataTest {

		private byte[] notMuchEntropy = new byte[16];

		private bool IsEmpty (byte[] array)
		{
			int total = 0;
			for (int i = 0; i < array.Length; i++)
				total += array[i];
			return (total == 0);
		}

		private void ProtectUnprotect (byte[] entropy, DataProtectionScope scope) 
		{
			try {
				byte[] data = new byte [16];
				byte[] encdata = ProtectedData.Protect (data, entropy, scope);
				Assert.IsFalse (IsEmpty (encdata), "Protect");

				byte[] decdata = ProtectedData.Unprotect (encdata, entropy, scope);
				Assert.IsTrue (IsEmpty (decdata), "Unprotect");
			}
			catch (CryptographicException ce) {
				if (ce.InnerException is UnauthorizedAccessException)
					Assert.Ignore ("The machine key store hasn't yet been created (as root).");
			}
			catch (PlatformNotSupportedException) {
				Assert.Ignore ("Only supported under Windows 2000 and later");
			}
		}

		[Test]
		public void ProtectCurrentUser () 
		{
			// we're testing the DataProtectionScope definition but
			// not if it's really limited to the scope specified
			ProtectUnprotect (notMuchEntropy, DataProtectionScope.CurrentUser);
		}

		[Test]
		public void ProtectLocalMachine () 
		{
			// we're testing the DataProtectionScope definition but
			// not if it's really limited to the scope specified
			ProtectUnprotect (notMuchEntropy, DataProtectionScope.LocalMachine);
		}

		[Test]
		public void DataProtectionScope_All ()
		{
			byte[] data = new byte[16];
			try {
				foreach (DataProtectionScope dps in Enum.GetValues (typeof (DataProtectionScope))) {
					byte[] encdata = ProtectedData.Protect (data, notMuchEntropy, dps);
					Assert.IsFalse (IsEmpty (encdata), "Protect");
					Assert.IsTrue (IsEmpty (data), "Protect(original unmodified)");
					byte[] decdata = ProtectedData.Unprotect (encdata, notMuchEntropy, dps);
					Assert.IsTrue (IsEmpty (decdata), "Unprotect");
				}
			}
			catch (CryptographicException ce) {
				if (ce.InnerException is UnauthorizedAccessException)
					Assert.Ignore ("The machine key store hasn't yet been created (as root).");
			}
			catch (PlatformNotSupportedException) {
				Assert.Ignore ("Only supported under Windows 2000 and later");
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		[Category ("NotDotNet")]
		public void Protect_InvalidDataProtectionScope ()
		{
			try {
				byte[] data = new byte[16];
				ProtectedData.Protect (data, notMuchEntropy, (DataProtectionScope) Int32.MinValue);
				// MS doesn't throw an ArgumentException but returning from
				// this method will throw an UnhandledException in NUnit
			}
			catch (PlatformNotSupportedException) {
				Assert.Ignore ("Only supported under Windows 2000 and later");
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ProtectNull () 
		{
			ProtectedData.Protect (null, notMuchEntropy, DataProtectionScope.CurrentUser);
		}

		[Test]
		public void ProtectNullEntropy () 
		{
			// we're testing the DataProtectionScope definition but
			// not if it's really limited to the scope specified
			ProtectUnprotect (null, DataProtectionScope.CurrentUser);
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void UnprotectNotProtectedData () 
		{
			try {
				byte[] baddata = new byte [16];
				ProtectedData.Unprotect (baddata, notMuchEntropy, DataProtectionScope.CurrentUser);
			}
			catch (PlatformNotSupportedException) {
				Assert.Ignore ("Only supported under Windows 2000 and later");
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		[Category ("NotDotNet")]
		public void Unprotect_InvalidDataProtectionScope ()
		{
			try {
				byte[] data = new byte[16];
				byte[] encdata = ProtectedData.Protect (data, notMuchEntropy, DataProtectionScope.CurrentUser);
				ProtectedData.Unprotect (encdata, notMuchEntropy, (DataProtectionScope) Int32.MinValue);
				// MS doesn't throw an ArgumentException but returning from
				// this method will throw an UnhandledException in NUnit
			}
			catch (PlatformNotSupportedException) {
				Assert.Ignore ("Only supported under Windows 2000 and later");
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void UnprotectNull () 
		{
			ProtectedData.Unprotect (null, notMuchEntropy, DataProtectionScope.CurrentUser);
		}
	}
}

#endif