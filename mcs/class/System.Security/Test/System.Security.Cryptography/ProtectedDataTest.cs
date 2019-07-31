//
// ProtectedDataTest.cs - NUnit Test Cases for ProtectedData
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//


using NUnit.Framework;

using System;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
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

#if !MOBILE // System.PlatformNotSupportedException: Operation is not supported on this platform.
		[Test] // https://bugzilla.xamarin.com/show_bug.cgi?id=38933
		public void ProtectCurrentUserMultiThread ()
		{
			string data = "Hello World";
			string entropy = "This is a long string with no meaningful content.";
			var entropyBytes = Encoding.UTF8.GetBytes (entropy);
			var dataBytes = Encoding.UTF8.GetBytes (data);
			var tasks = new List<Task> ();

			for (int i = 0; i < 20; i++)
			{
				tasks.Add (new Task (() => {
					byte[] encryptedBytes = ProtectedData.Protect (dataBytes, entropyBytes, DataProtectionScope.CurrentUser);
					Assert.IsFalse (IsEmpty (encryptedBytes), "#1");

					byte[] decryptedBytes = ProtectedData.Unprotect (encryptedBytes, entropyBytes, DataProtectionScope.CurrentUser);
					string decryptedString = Encoding.UTF8.GetString(decryptedBytes);
					Assert.AreEqual (data, decryptedString, "#2");
				}, TaskCreationOptions.LongRunning));
			}

			foreach (var t in tasks) t.Start ();
			Task.WaitAll (tasks.ToArray ());
		}
#endif

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
		public void UnprotectNotProtectedData () 
		{
			try {
				byte[] baddata = new byte [16];
				ProtectedData.Unprotect (baddata, notMuchEntropy, DataProtectionScope.CurrentUser);
			}
			catch (PlatformNotSupportedException) {
				Assert.Ignore ("Only supported under Windows 2000 and later");
			}
			catch (CryptographicException) {
				Assert.Pass ();
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