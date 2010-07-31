//
// IsolatedStorageFileCas.cs - CAS unit tests for 
//	System.IO.IsolatedStorage.IsolatedStorageFile
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

using NUnit.Framework;

using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;

namespace MonoCasTests.System.IO.IsolatedStorageTest {

	[TestFixture]
	[Category ("CAS")]
	public class IsolatedStorageFileCas {

		[SetUp]
		public void SetUp ()
		{
			if (!SecurityManager.SecurityEnabled)
				Assert.Ignore ("SecurityManager.SecurityEnabled is OFF");
		}

		// use the caller stack to execute some read operations
		private void Read (IsolatedStorageFile isf)
		{
			Assert.IsNotNull (isf.GetDirectoryNames ("*"), "GetDirectoryNames");
			Assert.IsNotNull (isf.GetFileNames ("*"), "GetFileNames");
			try {
				Assert.IsTrue (isf.CurrentSize >= 0, "CurrentSize");
				Assert.IsTrue (isf.MaximumSize >= isf.CurrentSize, "MaximumSize");
			}
			catch (InvalidOperationException) {
				// roaming
			}
		}

		// use the caller stack to execute some write operations
		private void Write (IsolatedStorageFile isf)
		{
			isf.CreateDirectory ("testdir");

			string filename = Path.Combine ("testdir", "file");
			using (IsolatedStorageFileStream isfs = new IsolatedStorageFileStream (filename, FileMode.Create, isf)) {
			}
			isf.DeleteFile (filename);

			isf.DeleteDirectory ("testdir");
			try {
				isf.Remove ();
			}
			catch (IsolatedStorageException) {
				// fx 1.x doesn't like removing when things "could" still be in use
			}
		}

#if NET_2_0
		[Test]
		[IsolatedStorageFilePermission (SecurityAction.Deny, UsageAllowed = IsolatedStorageContainment.ApplicationIsolationByMachine)]
		[ExpectedException (typeof (SecurityException))]
		[Ignore ("no manifest")]
		public void GetMachineStoreForApplication_Fail ()
		{
			IsolatedStorageFile isf = IsolatedStorageFile.GetMachineStoreForApplication ();
		}

		[Test]
		[Ignore ("no manifest")]
		[IsolatedStorageFilePermission (SecurityAction.PermitOnly, UsageAllowed = IsolatedStorageContainment.ApplicationIsolationByMachine)]
		public void GetMachineStoreForApplication_Pass ()
		{
			IsolatedStorageFile isf = IsolatedStorageFile.GetMachineStoreForApplication ();
		}

		[Test]
		[IsolatedStorageFilePermission (SecurityAction.Deny, UsageAllowed = IsolatedStorageContainment.AssemblyIsolationByMachine)]
		[ExpectedException (typeof (SecurityException))]
		public void GetMachineStoreForAssembly_Fail ()
		{
			IsolatedStorageFile isf = IsolatedStorageFile.GetMachineStoreForAssembly ();
		}

		[Test]
		[IsolatedStorageFilePermission (SecurityAction.PermitOnly, UsageAllowed = IsolatedStorageContainment.AssemblyIsolationByMachine)]
		public void GetMachineStoreForAssembly_Pass ()
		{
			IsolatedStorageFile isf = IsolatedStorageFile.GetMachineStoreForAssembly ();
			Read (isf);
			Write (isf);
			isf.Dispose ();
			isf.Close ();
		}

		[Test]
		[IsolatedStorageFilePermission (SecurityAction.PermitOnly, UsageAllowed = IsolatedStorageContainment.AdministerIsolatedStorageByUser)]
		public void GetMachineStoreForAssembly_Administer ()
		{
			IsolatedStorageFile isf = IsolatedStorageFile.GetMachineStoreForAssembly ();
			Read (isf);
			Write (isf);
			isf.Dispose ();
			isf.Close ();
		}

		[Test]
		[IsolatedStorageFilePermission (SecurityAction.Deny, UsageAllowed = IsolatedStorageContainment.DomainIsolationByMachine)]
		[ExpectedException (typeof (SecurityException))]
		public void GetMachineStoreForDomain_Fail ()
		{
			IsolatedStorageFile isf = IsolatedStorageFile.GetMachineStoreForDomain ();
		}

		[Test]
		[IsolatedStorageFilePermission (SecurityAction.PermitOnly, UsageAllowed = IsolatedStorageContainment.DomainIsolationByMachine)]
		public void GetMachineStoreForDomain_Pass ()
		{
			IsolatedStorageFile isf = IsolatedStorageFile.GetMachineStoreForDomain ();
			Read (isf);
			Write (isf);
			isf.Dispose ();
			isf.Close ();
		}

		[Test]
		[IsolatedStorageFilePermission (SecurityAction.PermitOnly, UsageAllowed = IsolatedStorageContainment.AdministerIsolatedStorageByUser)]
		public void GetMachineStoreForDomain_Administer ()
		{
			IsolatedStorageFile isf = IsolatedStorageFile.GetMachineStoreForDomain ();
			Read (isf);
			Write (isf);
			isf.Dispose ();
			isf.Close ();
		}

		[Test]
		[IsolatedStorageFilePermission (SecurityAction.Deny, UsageAllowed = IsolatedStorageContainment.ApplicationIsolationByUser)]
		[ExpectedException (typeof (SecurityException))]
		[Ignore ("no manifest")]
		public void GetUserStoreForApplication_Fail ()
		{
			IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication ();
		}

		[Test]
		[Ignore ("no manifest")]
		[IsolatedStorageFilePermission (SecurityAction.PermitOnly, UsageAllowed = IsolatedStorageContainment.ApplicationIsolationByUser)]
		public void GetUserStoreForApplication_Pass ()
		{
			IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication ();
		}
#endif
		[Test]
		[IsolatedStorageFilePermission (SecurityAction.Deny, UsageAllowed = IsolatedStorageContainment.AssemblyIsolationByUser)]
		[ExpectedException (typeof (SecurityException))]
		public void GetUserStoreForAssembly_Fail ()
		{
			IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForAssembly ();
		}

		[Test]
		[IsolatedStorageFilePermission (SecurityAction.PermitOnly, UsageAllowed = IsolatedStorageContainment.AssemblyIsolationByUser)]
		public void GetUserStoreForAssembly_Pass ()
		{
			IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForAssembly ();
			Read (isf);
			Write (isf);
			isf.Dispose ();
			isf.Close ();
		}

		[Test]
		[IsolatedStorageFilePermission (SecurityAction.Deny, UsageAllowed = IsolatedStorageContainment.AssemblyIsolationByRoamingUser)]
		[ExpectedException (typeof (SecurityException))]
		public void GetUserStoreForAssembly_Roaming_Fail ()
		{
			IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForAssembly ();
		}

		[Test]
		[IsolatedStorageFilePermission (SecurityAction.PermitOnly, UsageAllowed = IsolatedStorageContainment.AdministerIsolatedStorageByUser)]
		public void GetUserStoreForAssembly_Roaming_Pass ()
		{
			IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForAssembly ();
			Read (isf);
			Write (isf);
			isf.Dispose ();
			isf.Close ();
		}

		[Test]
		[IsolatedStorageFilePermission (SecurityAction.PermitOnly, UsageAllowed = IsolatedStorageContainment.AssemblyIsolationByRoamingUser)]
		public void GetUserStoreForAssembly_Administer ()
		{
			IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForAssembly ();
			Read (isf);
			Write (isf);
			isf.Dispose ();
			isf.Close ();
		}

		[Test]
		[IsolatedStorageFilePermission (SecurityAction.Deny, UsageAllowed = IsolatedStorageContainment.DomainIsolationByUser)]
		[ExpectedException (typeof (SecurityException))]
		public void GetUserStoreForDomain_Fail ()
		{
			IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForDomain ();
		}

		[Test]
		[IsolatedStorageFilePermission (SecurityAction.PermitOnly, UsageAllowed = IsolatedStorageContainment.DomainIsolationByUser)]
		public void GetUserStoreForDomain_Pass ()
		{
			IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForDomain ();
			Read (isf);
			Write (isf);
			isf.Dispose ();
			isf.Close ();
		}

		[Test]
		[IsolatedStorageFilePermission (SecurityAction.Deny, UsageAllowed = IsolatedStorageContainment.DomainIsolationByRoamingUser)]
		[ExpectedException (typeof (SecurityException))]
		public void GetUserStoreForDomain_Roaming_Fail ()
		{
			IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForDomain ();
		}

		[Test]
		[IsolatedStorageFilePermission (SecurityAction.PermitOnly, UsageAllowed = IsolatedStorageContainment.DomainIsolationByRoamingUser)]
		public void GetUserStoreForDomain_Roaming_Pass ()
		{
			IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForDomain ();
			Read (isf);
			Write (isf);
			isf.Dispose ();
			isf.Close ();
		}

		[Test]
		[IsolatedStorageFilePermission (SecurityAction.PermitOnly, UsageAllowed = IsolatedStorageContainment.AdministerIsolatedStorageByUser)]
		public void GetUserStoreForDomain_Administer ()
		{
			IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForDomain ();
			Read (isf);
			Write (isf);
			isf.Dispose ();
			isf.Close ();
		}


		private ulong MaximumSize (SecurityZone zone)
		{
			IsolatedStorageScope scope = IsolatedStorageScope.User | IsolatedStorageScope.Assembly;

			Evidence ae = new Evidence ();
			ae.AddHost (new Zone (zone));
			IsolatedStorageFile isf = IsolatedStorageFile.GetStore (scope, null, null, ae, typeof (Zone));
			return isf.MaximumSize;
		}

		[Test]
		public void MaximumSize ()
		{
			Assert.AreEqual (Int64.MaxValue, MaximumSize (SecurityZone.MyComputer), "MyComputer");
			Assert.AreEqual (Int64.MaxValue, MaximumSize (SecurityZone.Intranet), "Intranet");
#if NET_2_0
			Assert.AreEqual (512000, MaximumSize (SecurityZone.Internet), "Internet");
			Assert.AreEqual (512000, MaximumSize (SecurityZone.Trusted), "Trusted");
#else
			Assert.AreEqual (10240, MaximumSize (SecurityZone.Internet), "Internet");
			Assert.AreEqual (10240, MaximumSize (SecurityZone.Trusted), "Trusted");
#endif
		}

		[Test]
		[ExpectedException (typeof (PolicyException))]
		public void MaximumSize_Untrusted ()
		{
			Assert.AreEqual (Int64.MaxValue, MaximumSize (SecurityZone.Untrusted), "Untrusted");
		}

		[Test]
		[ExpectedException (typeof (PolicyException))]
		public void MaximumSize_NoZone ()
		{
			Assert.AreEqual (Int64.MaxValue, MaximumSize (SecurityZone.NoZone), "NoZone");
		}
	}
}
