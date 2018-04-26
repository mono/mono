//
// ProtectedDataCas.cs 
//	- CAS unit tests for System.Security.Cryptography.ProtectedData
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
#if !MOBILE

using NUnit.Framework;

using System;
using System.Reflection;
using System.Security;
using System.Security.Cryptography;
using System.Security.Permissions;

using MonoTests.System.Security.Cryptography;

namespace MonoCasTests.System.Security.Cryptography {

	[TestFixture]
	[Category ("CAS")]
	// problem with CSC when an assembly use permissions defined within itself
	[Category ("NotWorking")] 
	public class ProtectedDataCas {

		[SetUp]
		public virtual void SetUp ()
		{
			if (!SecurityManager.SecurityEnabled)
				Assert.Ignore ("SecurityManager.SecurityEnabled is OFF");
		}

		private bool IsEmpty (byte[] array)
		{
			int total = 0;
			for (int i = 0; i < array.Length; i++)
				total += array[i];
			return (total == 0);
		}

		[Test]
		[DataProtectionPermission (SecurityAction.PermitOnly, ProtectData = true, UnprotectData = true)]
		public void UnitTestReuse ()
		{
			ProtectedDataTest unit = new ProtectedDataTest ();
			unit.ProtectCurrentUser ();
			unit.ProtectLocalMachine ();
			unit.DataProtectionScope_All ();
			unit.ProtectNullEntropy ();
		}

		[Test]
		[DataProtectionPermission (SecurityAction.PermitOnly, ProtectData = true)]
		// note: this implies that UnmanagedCode isn't allowed
		public void Protect_PermitOnly_Protect ()
		{
			byte[] data = new byte[8];
			byte[] entropy = new byte[1];

			try {
				byte[] encdata = ProtectedData.Protect (data, null, DataProtectionScope.CurrentUser);
				Assert.IsFalse (IsEmpty (encdata), "null-CurrentUser");

				encdata = ProtectedData.Protect (data, entropy, DataProtectionScope.CurrentUser);
				Assert.IsFalse (IsEmpty (encdata), "entropy-CurrentUser");

				encdata = ProtectedData.Protect (data, null, DataProtectionScope.LocalMachine);
				Assert.IsFalse (IsEmpty (encdata), "null-LocalMachine");

				encdata = ProtectedData.Protect (data, entropy, DataProtectionScope.LocalMachine);
				Assert.IsFalse (IsEmpty (encdata), "entropy-LocalMachine");
			}
			catch (PlatformNotSupportedException) {
				Assert.Ignore ("Only supported under Windows 2000 and later");
			}
		}

		[Test]
		[DataProtectionPermission (SecurityAction.Deny, ProtectData = true)]
		[ExpectedException (typeof (SecurityException))]
		public void Protect_Deny_Protect ()
		{
			try {
				ProtectedData.Protect (new byte[8], null, DataProtectionScope.CurrentUser);
			}
			catch (PlatformNotSupportedException) {
				Assert.Ignore ("Only supported under Windows 2000 and later");
			}
		}

		[Test]
		[DataProtectionPermission (SecurityAction.PermitOnly, UnprotectData = true)]
		// note: this implies that UnmanagedCode isn't allowed
		[ExpectedException (typeof (CryptographicException))]
		public void Unprotect_PermitOnly_Unprotect ()
		{
			try {
				ProtectedData.Unprotect (new byte[8], null, DataProtectionScope.CurrentUser);
			}
			catch (PlatformNotSupportedException) {
				Assert.Ignore ("Only supported under Windows 2000 and later");
			}
		}

		[Test]
		[DataProtectionPermission (SecurityAction.Deny, UnprotectData = true)]
		[ExpectedException (typeof (SecurityException))]
		public void Unprotect_Deny_Unprotect ()
		{
			try {
				ProtectedData.Unprotect (new byte[8], null, DataProtectionScope.CurrentUser);
			}
			catch (PlatformNotSupportedException) {
				Assert.Ignore ("Only supported under Windows 2000 and later");
			}
		}

		[Test]
		[DataProtectionPermission (SecurityAction.PermitOnly, ProtectData = true, UnprotectData = true)]
		public void LinkDemand_PermitOnly_DataProtection ()
		{
			Type pd = typeof (ProtectedData);
			object[] parameters = new object[3] { new byte[8], null, DataProtectionScope.CurrentUser };

			try {
				MethodInfo mi = pd.GetMethod ("Protect");
				Assert.IsNotNull (mi, "Protect");
				byte[] encdata = (byte[]) mi.Invoke (null, parameters);
				Assert.IsNotNull (encdata, "Invoke Protect");
				Assert.IsFalse (IsEmpty (encdata), "Encrypted");

				mi = pd.GetMethod ("Unprotect");
				Assert.IsNotNull (mi, "Unprotect");
				parameters[0] = encdata;
				byte[] decdata = (byte[]) mi.Invoke (null, parameters);
				Assert.IsNotNull (decdata, "Invoke Unprotect");
				Assert.IsTrue (IsEmpty (decdata), "Decrypted");

				// so no LinkDemand are required (Demand are enough) and
				// no check for UnmanagedCode are required
			}
			catch (TargetInvocationException tie) {
				if (tie.InnerException is PlatformNotSupportedException)
					Assert.Ignore ("Only supported under Windows 2000 and later");
			}
		}
	}
}
#endif
