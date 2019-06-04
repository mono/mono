//
// ProtectedMemoryCas.cs 
//	- CAS unit tests for System.Security.Cryptography.ProtectedMemory
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
	public class ProtectedMemoryCas {

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
		[DataProtectionPermission (SecurityAction.PermitOnly, ProtectMemory = true, UnprotectMemory = true)]
		public void UnitTestReuse ()
		{
			ProtectedMemoryTest unit = new ProtectedMemoryTest ();
			unit.ProtectSameProcess ();
			unit.ProtectSameLogon ();
			unit.ProtectCrossProcess ();
			unit.MemoryProtectionScope_All ();
		}

		[Test]
		[DataProtectionPermission (SecurityAction.PermitOnly, ProtectMemory = true)]
		// note: this implies that UnmanagedCode isn't allowed
		public void Protect_PermitOnly_Protect ()
		{
			try {
				byte[] data = new byte[16];
				ProtectedMemory.Protect (data, MemoryProtectionScope.SameProcess);
				Assert.IsFalse (IsEmpty (data), "SameProcess");

				data = new byte[16];
				ProtectedMemory.Protect (data, MemoryProtectionScope.SameLogon);
				Assert.IsFalse (IsEmpty (data), "SameLogon");

				data = new byte[16];
				ProtectedMemory.Protect (data, MemoryProtectionScope.CrossProcess);
				Assert.IsFalse (IsEmpty (data), "CrossProcess");
			}
			catch (PlatformNotSupportedException) {
				Assert.Ignore ("Only supported under Windows 2000 SP3 and later");
			}
		}

		[Test]
		[DataProtectionPermission (SecurityAction.Deny, ProtectMemory = true)]
		[ExpectedException (typeof (SecurityException))]
		public void Protect_Deny_Protect ()
		{
			try {
				ProtectedMemory.Protect (new byte[16], MemoryProtectionScope.SameProcess);
			}
			catch (PlatformNotSupportedException) {
				Assert.Ignore ("Only supported under Windows 2000 SP3 and later");
			}
		}

		[Test]
		[DataProtectionPermission (SecurityAction.PermitOnly, UnprotectMemory = true)]
		// note: this implies that UnmanagedCode isn't allowed
		public void Unprotect_PermitOnly_Unprotect ()
		{
			try {
				byte[] data = new byte[16];
				ProtectedMemory.Unprotect (data, MemoryProtectionScope.SameProcess);
				Assert.IsFalse (IsEmpty (data), "Unprotect unprotected");
			}
			catch (PlatformNotSupportedException) {
				Assert.Ignore ("Only supported under Windows 2000 SP3 and later");
			}
		}

		[Test]
		[DataProtectionPermission (SecurityAction.Deny, UnprotectMemory = true)]
		[ExpectedException (typeof (SecurityException))]
		public void Unprotect_Deny_Unprotect ()
		{
			try {
				ProtectedMemory.Unprotect (new byte[16], MemoryProtectionScope.SameProcess);
			}
			catch (PlatformNotSupportedException) {
				Assert.Ignore ("Only supported under Windows 2000 SP3 and later");
			}
		}

		[Test]
		[DataProtectionPermission (SecurityAction.PermitOnly, ProtectMemory = true, UnprotectMemory = true)]
		public void LinkDemand_PermitOnly_DataProtection ()
		{
			Type pm = typeof (ProtectedMemory);
			byte[] data = new byte[16];
			object[] parameters = new object[2] { data, MemoryProtectionScope.SameProcess };

			try {
				MethodInfo mi = pm.GetMethod ("Protect");
				Assert.IsNotNull (mi, "Protect");
				mi.Invoke (null, parameters);
				Assert.IsFalse (IsEmpty (data), "Encrypted");

				mi = pm.GetMethod ("Unprotect");
				Assert.IsNotNull (mi, "Unprotect");
				mi.Invoke (null, parameters);
				Assert.IsTrue (IsEmpty (data), "Decrypted");

				// so no LinkDemand are required (Demand are enough) and
				// no check for UnmanagedCode are required
			}
			catch (TargetInvocationException tie) {
				if (tie.InnerException is PlatformNotSupportedException)
					Assert.Ignore ("Only supported under Windows 2000 SP 3 and later");
			}
		}
	}
}
#endif
