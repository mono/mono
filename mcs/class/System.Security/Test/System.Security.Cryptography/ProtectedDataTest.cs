//
// ProtectedDataTest.cs - NUnit Test Cases for ProtectedData
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
	public class ProtectedDataTest : Assertion {

		private void ProtectUnprotect (byte[] entropy, DataProtectionScope scope) 
		{
			byte[] data = new byte [16];
			byte[] encdata = ProtectedData.Protect (data, entropy, scope);
			int total = 0;
			for (int i=0; i < 16; i++)
				total += encdata [i];
			Assert ("Protect", (total != 0));

			byte[] decdata = ProtectedData.Unprotect (encdata, entropy, scope);
			total = 0;
			for (int i=0; i < 16; i++)
				total += decdata [i];
			Assert ("Unprotect", (total == 0));
		}

		[Test]
		public void ProtectCurrentUser () 
		{
			try {
				byte[] notMuchEntropy = new byte [16];
				// we're testing the DataProtectionScope definition but
				// not if it's really limited to the scope specified
				ProtectUnprotect (notMuchEntropy, DataProtectionScope.CurrentUser);
			}
			catch (PlatformNotSupportedException) {
				Console.WriteLine ("Only supported under Windows 2000 and later");
			}
		}

		[Test]
		public void ProtectLocalMachine () 
		{
			try {
				byte[] notMuchEntropy = new byte [16];
				// we're testing the DataProtectionScope definition but
				// not if it's really limited to the scope specified
				ProtectUnprotect (notMuchEntropy, DataProtectionScope.LocalMachine);
			}
			catch (PlatformNotSupportedException) {
				Console.WriteLine ("Only supported under Windows 2000 and later");
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ProtectNull () 
		{
			byte[] notMuchEntropy = new byte [16];
			ProtectedData.Protect (null, notMuchEntropy, DataProtectionScope.CurrentUser);
		}

		[Test]
		public void ProtectNullEntropy () 
		{
			try {
				// we're testing the DataProtectionScope definition but
				// not if it's really limited to the scope specified
				ProtectUnprotect (null, DataProtectionScope.LocalMachine);
			}
			catch (PlatformNotSupportedException) {
				Console.WriteLine ("Only supported under Windows 2000 and later");
			}
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void UnprotectNotProtectedData () 
		{
			byte[] baddata = new byte [16];
			byte[] notMuchEntropy = new byte [16];
			ProtectedData.Unprotect (baddata, notMuchEntropy, DataProtectionScope.CurrentUser);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void UnprotectNull () 
		{
			byte[] notMuchEntropy = new byte [16];
			ProtectedData.Unprotect (null, notMuchEntropy, DataProtectionScope.CurrentUser);
		}
	}
}

#endif