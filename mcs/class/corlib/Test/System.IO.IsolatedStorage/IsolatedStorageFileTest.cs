//
// IsolatedStorageFileTest.cs 
//	- Unit Tests for abstract IsolatedStorageFile class
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2005 Novell Inc. (http://www.novell.com)
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

using System;
using System.Collections;
using System.IO;
using System.IO.IsolatedStorage;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;

using NUnit.Framework;

namespace MonoTests.System.IO.IsolatedStorageTest {

	[TestFixture]
	public class IsolatedStorageFileTest {

		private const string dirname = "mono-unit-test";

		private void CheckEnumerated (int n, IsolatedStorageScope scope, IsolatedStorageFile isf)
		{
			string prefix = n.ToString () + " - " + scope.ToString () + " - ";
			Assert.IsNotNull (isf, prefix + "IsolatedStorageFile");
			Assert.IsTrue (((scope & isf.Scope) != 0), prefix + "Scope");

			if ((isf.Scope & IsolatedStorageScope.Assembly) != 0)
				Assert.IsNotNull (isf.AssemblyIdentity, prefix + "AssemblyIdentity");
			if ((isf.Scope & IsolatedStorageScope.Domain) != 0)
				Assert.IsNotNull (isf.DomainIdentity, prefix + "DomainIdentity");
#if NET_2_0
			if ((isf.Scope & IsolatedStorageScope.Application) != 0)
				Assert.IsNotNull (isf.ApplicationIdentity, prefix + "ApplicationIdentity");
#endif
		}

		private void GetEnumerator (IsolatedStorageScope scope)
		{
			IEnumerator e = IsolatedStorageFile.GetEnumerator (scope);
			int n = 0;
			while (e.MoveNext ())
			{
				IsolatedStorageFile isf = (IsolatedStorageFile)e.Current;
				CheckEnumerated (++n, scope, isf);
			}
		}

		[Test]
		public void GetEnumerator_User ()
		{
			GetEnumerator (IsolatedStorageScope.User);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void GetEnumerator_User_Details ()
		{
			// giving more details is bad
			GetEnumerator (IsolatedStorageScope.User | IsolatedStorageScope.Assembly | IsolatedStorageScope.Domain);
		}

		[Test]
		public void GetEnumerator_UserRoaming ()
		{
			GetEnumerator (IsolatedStorageScope.User | IsolatedStorageScope.Roaming);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void GetEnumerator_UserRoaming_Details ()
		{
			// giving more details is bad
			GetEnumerator (IsolatedStorageScope.User | IsolatedStorageScope.Assembly | IsolatedStorageScope.Domain | IsolatedStorageScope.Roaming);
		}
#if NET_2_0
		[Test]
		public void GetEnumerator_Machine ()
		{
			GetEnumerator (IsolatedStorageScope.Machine);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void GetEnumerator_Machine_Details ()
		{
			GetEnumerator (IsolatedStorageScope.Machine | IsolatedStorageScope.Assembly);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void GetEnumerator_Application ()
		{
			// we can't enum application
			GetEnumerator (IsolatedStorageScope.Application);
		}
#endif
		[Test]
		public void GetUserStoreForAssembly ()
		{
			IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForAssembly ();
			Assert.AreEqual (Int64.MaxValue, isf.MaximumSize, "MaximumSize");
			Assert.AreEqual (IsolatedStorageScope.User | IsolatedStorageScope.Assembly, isf.Scope, "Scope");
			Assert.IsTrue ((isf.AssemblyIdentity is Url), "AssemblyIdentity");
			// note: mono transforms the CodeBase into uppercase
			// for net 1.1 which uses file:// and not file:///
			string codebase = Assembly.GetExecutingAssembly ().CodeBase.ToUpper ().Substring (8);
			Assert.IsTrue ((isf.AssemblyIdentity.ToString ().ToUpper ().IndexOf (codebase) > 0), "Url");
			Assert.IsTrue ((isf.AssemblyIdentity.ToString ().ToUpper ().IndexOf (codebase) > 0), "Url");
			Assert.IsTrue ((isf.CurrentSize >= 0), "CurrentSize");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void GetUserStoreForAssembly_DomainIdentity ()
		{
			IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForAssembly ();
			object o = isf.DomainIdentity;
		}

#if NET_2_0
		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void GetUserStoreForAssembly_ApplicationIdentity ()
		{
			IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForAssembly ();
			object o = isf.ApplicationIdentity;
		}
#endif

		[Test]
		public void GetUserStoreForDomain ()
		{
			IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForDomain ();
			Assert.AreEqual (Int64.MaxValue, isf.MaximumSize, "MaximumSize");
			Assert.AreEqual (IsolatedStorageScope.User | IsolatedStorageScope.Domain | IsolatedStorageScope.Assembly, isf.Scope, "Scope");
			Assert.IsTrue ((isf.AssemblyIdentity is Url), "AssemblyIdentity");
			// note: mono transforms the CodeBase into uppercase
			// for net 1.1 which uses file:// and not file:///
			string codebase = Assembly.GetExecutingAssembly ().CodeBase.ToUpper ().Substring (8);
			Assert.IsTrue ((isf.AssemblyIdentity.ToString ().ToUpper ().IndexOf (codebase) > 0), "Url");
			Assert.IsTrue ((isf.DomainIdentity is Url), "DomainIdentity");
			// note: with MS Assembly.GetEntryAssembly () only works in the default (first) AppDomain
			// so we're using the first parameter to GetCommandLineArgs
			string exe = Environment.GetCommandLineArgs ()[0].Replace ("\\", "/").ToUpper ();
			Assert.IsTrue ((isf.DomainIdentity.ToString ().ToUpper ().IndexOf (exe) > 0), exe + "\n" + isf.DomainIdentity.ToString ().ToUpper ()); //"Url - Domain");
			Assert.IsTrue ((isf.CurrentSize >= 0), "CurrentSize");
		}

#if NET_2_0
		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void GetUserStoreForDomain_ApplicationIdentity ()
		{
			IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForDomain ();
			object o = isf.ApplicationIdentity;
		}

		[Test]
		[ExpectedException (typeof (IsolatedStorageException))]
		public void GetUserStoreForApplication_WithoutApplicationIdentity ()
		{
			// note: a manifest is required
			IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication ();
		}

		[Test]
		[ExpectedException (typeof (IsolatedStorageException))]
		public void GetUserStoreForApplication ()
		{
			IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication ();
			Assert.AreEqual (Int64.MaxValue, isf.MaximumSize, "MaximumSize");
			Assert.AreEqual (IsolatedStorageScope.User | IsolatedStorageScope.Assembly, isf.Scope, "Scope");
			Assert.IsTrue ((isf.AssemblyIdentity is Url), "AssemblyIdentity");
			Assert.IsTrue ((isf.AssemblyIdentity.ToString ().IndexOf (Assembly.GetExecutingAssembly ().CodeBase) > 0), "Url");
			Assert.IsTrue ((isf.CurrentSize >= 0), "CurrentSize");
		}

		[Test]
		[ExpectedException (typeof (IsolatedStorageException))]
		public void GetUserStoreForApplication_AssemblyIdentity ()
		{
			IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication ();
			object o = isf.AssemblyIdentity;
		}

		[Test]
		[ExpectedException (typeof (IsolatedStorageException))]
		public void GetUserStoreForApplication_DomainIdentity ()
		{
			IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication ();
			object o = isf.DomainIdentity;
		}
#endif

		[Test]
		public void GetStore_Domain_Zone ()
		{
			IsolatedStorageScope scope = IsolatedStorageScope.User | IsolatedStorageScope.Domain | IsolatedStorageScope.Assembly;
			IsolatedStorageFile isf = IsolatedStorageFile.GetStore (scope, typeof (Zone), typeof (Zone));
			Assert.AreEqual (Int64.MaxValue, isf.MaximumSize, "MaximumSize");
			Assert.AreEqual (IsolatedStorageScope.User | IsolatedStorageScope.Domain | IsolatedStorageScope.Assembly, isf.Scope, "Scope");
			Assert.IsTrue ((isf.AssemblyIdentity is Zone), "AssemblyIdentity");
			Assert.IsTrue ((isf.AssemblyIdentity.ToString ().IndexOf ("MyComputer") > 0), "Zone - Assembly");
			Assert.IsTrue ((isf.DomainIdentity is Zone), "DomainIdentity");
			Assert.IsTrue ((isf.DomainIdentity.ToString ().IndexOf ("MyComputer") > 0), "Zone - Domain");
			Assert.IsTrue ((isf.CurrentSize >= 0), "CurrentSize");
		}

		[Test]
		[ExpectedException (typeof (IsolatedStorageException))]
		public void GetStore_Domain_NonPresentEvidences ()
		{
			IsolatedStorageScope scope = IsolatedStorageScope.User | IsolatedStorageScope.Domain | IsolatedStorageScope.Assembly;
			IsolatedStorageFile isf = IsolatedStorageFile.GetStore (scope, typeof (StrongName), typeof (Publisher));
		}

		[Test]
		public void GetStore_Assembly_NonPresentDomainEvidences ()
		{
			IsolatedStorageScope scope = IsolatedStorageScope.User | IsolatedStorageScope.Assembly;
			IsolatedStorageFile isf = IsolatedStorageFile.GetStore (scope, typeof (StrongName), typeof (Url));
			Assert.AreEqual (Int64.MaxValue, isf.MaximumSize, "MaximumSize");
			Assert.AreEqual (scope, isf.Scope, "Scope");
			Assert.IsTrue ((isf.AssemblyIdentity is Url), "AssemblyIdentity");
			// note: mono transforms the CodeBase into uppercase
			// for net 1.1 which uses file:// and not file:///
			string codebase = Assembly.GetExecutingAssembly ().CodeBase.ToUpper ().Substring (8);
			Assert.IsTrue ((isf.AssemblyIdentity.ToString ().ToUpper ().IndexOf (codebase) > 0), "Url");
			// DomainIdentity throws a InvalidOperationException
			Assert.IsTrue ((isf.CurrentSize >= 0), "CurrentSize");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void GetStore_Domain_DomainNullObject ()
		{
			IsolatedStorageScope scope = IsolatedStorageScope.User | IsolatedStorageScope.Domain | IsolatedStorageScope.Assembly;
			IsolatedStorageFile isf = IsolatedStorageFile.GetStore (scope, (object)null, new Zone (SecurityZone.MyComputer));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void GetStore_Domain_AssemblyNullObject ()
		{
			IsolatedStorageScope scope = IsolatedStorageScope.User | IsolatedStorageScope.Domain | IsolatedStorageScope.Assembly;
			IsolatedStorageFile isf = IsolatedStorageFile.GetStore (scope, new Zone (SecurityZone.MyComputer), (object)null);
		}

		[Test]
		public void GetStore_Assembly_DomainNullObject ()
		{
			IsolatedStorageScope scope = IsolatedStorageScope.User | IsolatedStorageScope.Assembly;
			IsolatedStorageFile isf = IsolatedStorageFile.GetStore (scope, (object)null, new Zone (SecurityZone.Internet));
			Assert.AreEqual (Int64.MaxValue, isf.MaximumSize, "MaximumSize");
			Assert.AreEqual (scope, isf.Scope, "Scope");
			Assert.IsTrue ((isf.AssemblyIdentity is Zone), "AssemblyIdentity");
			Assert.IsTrue ((isf.AssemblyIdentity.ToString ().IndexOf ("Internet") > 0), "Zone - Assembly");
			Assert.IsTrue ((isf.CurrentSize >= 0), "CurrentSize");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void GetStore_Assembly_AssemblyNullObject ()
		{
			IsolatedStorageScope scope = IsolatedStorageScope.User | IsolatedStorageScope.Assembly;
			IsolatedStorageFile isf = IsolatedStorageFile.GetStore (scope, new Zone (SecurityZone.MyComputer), (object)null);
		}

		[Test]
		public void GetStore_Domain_ZoneObjectZoneObject ()
		{
			IsolatedStorageScope scope = IsolatedStorageScope.User | IsolatedStorageScope.Domain | IsolatedStorageScope.Assembly;
			IsolatedStorageFile isf = IsolatedStorageFile.GetStore (scope, new Zone (SecurityZone.Internet), new Zone (SecurityZone.Internet));
			Assert.AreEqual (Int64.MaxValue, isf.MaximumSize, "MaximumSize");
			Assert.AreEqual (scope, isf.Scope, "Scope");
			Assert.IsTrue ((isf.AssemblyIdentity is Zone), "AssemblyIdentity");
			Assert.IsTrue ((isf.AssemblyIdentity.ToString ().IndexOf ("Internet") > 0), "Zone - Assembly");
			Assert.IsTrue ((isf.DomainIdentity is Zone), "DomainIdentity");
			Assert.IsTrue ((isf.DomainIdentity.ToString ().IndexOf ("Internet") > 0), "Zone - Domain");
			Assert.IsTrue ((isf.CurrentSize >= 0), "CurrentSize");
		}
#if NET_2_0
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void GetStore_Application_NullObject ()
		{
			IsolatedStorageScope scope = IsolatedStorageScope.User | IsolatedStorageScope.Application;
			IsolatedStorageFile isf = IsolatedStorageFile.GetStore (scope, (object)null);
		}

		[Test]
		[ExpectedException (typeof (IsolatedStorageException))]
		public void GetStore_Application_NullType ()
		{
			IsolatedStorageScope scope = IsolatedStorageScope.User | IsolatedStorageScope.Application;
			IsolatedStorageFile isf = IsolatedStorageFile.GetStore (scope, (Type)null);
			// again it's the lack of a manifest
		}
#endif

		[Test]
		public void GetStore_DomainScope_Evidences ()
		{
			IsolatedStorageScope scope = IsolatedStorageScope.User | IsolatedStorageScope.Domain | IsolatedStorageScope.Assembly;

			Evidence de = new Evidence ();
			de.AddHost (new Zone (SecurityZone.Internet));
			Evidence ae = new Evidence ();
			ae.AddHost (new Zone (SecurityZone.Intranet));
			IsolatedStorageFile isf = IsolatedStorageFile.GetStore (scope, de, typeof (Zone), ae, typeof (Zone));

			// Maximum size for Internet isn't (by default) Int64.MaxValue
			Assert.AreEqual (scope, isf.Scope, "Scope");
			Assert.IsTrue ((isf.AssemblyIdentity is Zone), "AssemblyIdentity");
			Assert.IsTrue ((isf.AssemblyIdentity.ToString ().IndexOf ("Intranet") > 0), "Zone - Assembly");
			Assert.IsTrue ((isf.DomainIdentity is Zone), "DomainIdentity");
			Assert.IsTrue ((isf.DomainIdentity.ToString ().IndexOf ("Internet") > 0), isf.DomainIdentity.ToString ()); //"Zone - Domain");
			Assert.IsTrue ((isf.CurrentSize >= 0), "CurrentSize");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void GetStore_DomainScope_Evidence_NullAssemblyEvidence ()
		{
			IsolatedStorageScope scope = IsolatedStorageScope.User | IsolatedStorageScope.Domain | IsolatedStorageScope.Assembly;

			Evidence de = new Evidence ();
			de.AddHost (new Zone (SecurityZone.Internet));
			IsolatedStorageFile isf = IsolatedStorageFile.GetStore (scope, de, typeof (Zone), null, null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void GetStore_DomainScope_Evidence_NullDomainEvidence ()
		{
			IsolatedStorageScope scope = IsolatedStorageScope.User | IsolatedStorageScope.Domain | IsolatedStorageScope.Assembly;

			Evidence ae = new Evidence ();
			ae.AddHost (new Zone (SecurityZone.Internet));
			IsolatedStorageFile isf = IsolatedStorageFile.GetStore (scope, null, null, ae, typeof (Zone));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void GetStore_AssemblyScope_Evidence_NullAssemblyEvidence ()
		{
			IsolatedStorageScope scope = IsolatedStorageScope.User | IsolatedStorageScope.Assembly;

			Evidence de = new Evidence ();
			de.AddHost (new Zone (SecurityZone.Internet));
			IsolatedStorageFile isf = IsolatedStorageFile.GetStore (scope, de, typeof (Zone), null, null);
		}

		[Test]
		public void GetStore_AssemblyScope_Evidence_NullDomainEvidence ()
		{
			IsolatedStorageScope scope = IsolatedStorageScope.User | IsolatedStorageScope.Assembly;

			Evidence ae = new Evidence ();
			ae.AddHost (new Zone (SecurityZone.Internet));
			IsolatedStorageFile isf = IsolatedStorageFile.GetStore (scope, null, null, ae, typeof (Zone));
		}

		[Test]
		public void RegressionBNC354539 ()
		{
			string filename = "test-bnc-354539";
			byte[] expected = new byte[] { 0x01, 0x42, 0x00 };
			byte[] actual = new byte [expected.Length];

			using (IsolatedStorageFile file = IsolatedStorageFile.GetStore (IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null)) {
				using (IsolatedStorageFileStream stream = new IsolatedStorageFileStream (filename, FileMode.Create, FileAccess.Write, FileShare.None, file)) {
					stream.Write (expected, 0, expected.Length);
				}
			}

			using (IsolatedStorageFile file = IsolatedStorageFile.GetUserStoreForAssembly ()) {
				using (IsolatedStorageFileStream stream = new IsolatedStorageFileStream (filename, FileMode.Open, FileAccess.Read, FileShare.Read, file)) {
					stream.Read (actual, 0, actual.Length);
				}

				file.DeleteFile (filename);
			}
			
			Assert.AreEqual (expected, actual);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void CreateDirectory_Null ()
		{
			IsolatedStorageFile.GetUserStoreForAssembly ().CreateDirectory (null);
		}

		[Test]
		public void CreateDirectory_FileWithSameNameExists ()
		{
			string path = "bug374377";
			using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForDomain ()) {
				using (IsolatedStorageFileStream fs = new IsolatedStorageFileStream (path, FileMode.OpenOrCreate, isf)) {
				}
				try {
					isf.CreateDirectory (path);
				}
				catch (IOException ex) {
					Assert.AreEqual (typeof (IOException), ex.GetType (), "Type");
					// don't leak path information
					Assert.IsFalse (ex.Message.IndexOf (path) >= 0, "Message");
					Assert.IsNull (ex.InnerException, "InnerException");
				}
			}
		}

		[Test]
		public void CreateDirectory_DirectoryWithSameNameExists ()
		{
			string dir = "new-dir";
			string file = Path.Combine (dir, "new-file");
			IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForAssembly ();
			try {
				isf.CreateDirectory (dir);
				using (IsolatedStorageFileStream isfs = new IsolatedStorageFileStream (file, FileMode.OpenOrCreate, isf)) {
					isfs.WriteByte (0);
				}
				string pattern = Path.Combine (dir, "*");
				Assert.AreEqual (1, isf.GetFileNames (file).Length, "file exists");

				// create again directory
				isf.CreateDirectory (dir);
				Assert.AreEqual (1, isf.GetFileNames (file).Length, "file still exists");
			}
			finally {
				isf.DeleteFile (file);
				isf.DeleteDirectory (dir);
			}
		}

		[Test]
		[ExpectedException (typeof (SecurityException))]
		public void GetFilesInSubdirs ()
		{
			IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForAssembly ();
			string pattern = Path.Combine ("..", "*");
			isf.GetFileNames (pattern);
		}

		[Test] // https://bugzilla.novell.com/show_bug.cgi?id=376188
		public void CreateSubDirectory ()
		{
			IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForAssembly ();
			isf.CreateDirectory ("subdir");
			isf.CreateDirectory ("subdir/subdir2");
			Assert.AreEqual (1, isf.GetDirectoryNames ("*").Length, "subdir");
			Assert.AreEqual (1, isf.GetDirectoryNames ("subdir/*").Length, "subdir/subdir2");
			isf.DeleteDirectory ("subdir/subdir2");
			isf.DeleteDirectory ("subdir");
		}

		[Test]
		[ExpectedException (typeof (IsolatedStorageException))]
		public void DeleteDirectory_NonEmpty ()
		{
			IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForAssembly ();
			isf.CreateDirectory ("subdir");
			isf.CreateDirectory ("subdir/subdir2");
			isf.DeleteDirectory ("subdir");
		}

		[Test]
		public void GetStore_NullTypes ()
		{
			IsolatedStorageScope scope = IsolatedStorageScope.User | IsolatedStorageScope.Roaming | IsolatedStorageScope.Assembly | IsolatedStorageScope.Domain;
			IsolatedStorageFile isf = IsolatedStorageFile.GetStore (scope, null, null);
			Assert.AreEqual (typeof (Url), isf.AssemblyIdentity.GetType (), "AssemblyIdentity");
			Assert.AreEqual (typeof (Url), isf.DomainIdentity.GetType (), "DomainIdentity");
		}

#if NET_4_0
		[Test]
		public void DirectoryExists ()
		{
			IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForAssembly ();
			isf.CreateDirectory ("subdir");
			isf.CreateDirectory ("subdir/subdir2");
			isf.CreateDirectory ("subdir3");

			Assert.AreEqual (true, isf.DirectoryExists ("subdir/"), "#A0");
			Assert.AreEqual (true, isf.DirectoryExists ("subdir/subdir2/"), "#A1");
			Assert.AreEqual (true, isf.DirectoryExists ("subdir3"), "#A2");
			Assert.AreEqual (true, isf.DirectoryExists (String.Empty), "#A3"); // Weird
			Assert.AreEqual (false, isf.DirectoryExists ("subdir99"), "#A4");
			Assert.AreEqual (false, isf.DirectoryExists ("../../subdir"), "#A5");
			Assert.AreEqual (false, isf.DirectoryExists ("*"), "#A5");
			Assert.AreEqual (false, isf.DirectoryExists ("subdir*"), "#A6");

			isf.DeleteDirectory ("subdir3");
			Assert.AreEqual (false, isf.DirectoryExists ("subdir3"), "#B0");

			isf.DeleteDirectory ("subdir/subdir2");
			isf.DeleteDirectory ("subdir");

			try {
				isf.DirectoryExists (null);
				Assert.Fail ("#Exc1");
			} catch (ArgumentNullException) {
			}

			isf.Close ();
			try {
				isf.DirectoryExists ("subdir");
				Assert.Fail ("#Exc2");
			} catch (InvalidOperationException) {
			}

			isf.Dispose ();
			try {
				isf.DirectoryExists ("subdir");
				Assert.Fail ("#Exc3");
			} catch (ObjectDisposedException) {
			}

			// We want to be sure that if not closing but disposing
			// should fire ObjectDisposedException instead of InvalidOperationException
			isf = IsolatedStorageFile.GetUserStoreForAssembly ();
			isf.Dispose ();

			try {
				isf.DirectoryExists ("subdir");
				Assert.Fail ("#Exc4");
			} catch (ObjectDisposedException) {
			}
		}

		[Test]
		public void FileExists ()
		{
			IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForAssembly ();
			IsolatedStorageFileStream file_a = new IsolatedStorageFileStream ("file-a", FileMode.Create, isf);
			IsolatedStorageFileStream file_b = new IsolatedStorageFileStream ("file-b", FileMode.Create, isf);
			file_a.Close ();
			file_b.Close ();

			Assert.AreEqual (true, isf.FileExists ("file-a"), "#A0");
			Assert.AreEqual (true, isf.FileExists ("file-b"), "#A1");
			Assert.AreEqual (false, isf.FileExists (String.Empty), "#A2");
			Assert.AreEqual (false, isf.FileExists ("file-"), "#A3");
			Assert.AreEqual (false, isf.FileExists ("file-*"), "#A4");
			Assert.AreEqual (false, isf.FileExists ("../../file-a"), "#A5");

			isf.CreateDirectory ("subdir");
			Assert.AreEqual (false, isf.FileExists ("subdir"), "#B0");

			try {
				isf.FileExists (null);
				Assert.Fail ("#Exc1");
			} catch (ArgumentNullException) {
			}

			isf.Close ();
			try {
				isf.FileExists ("file-a");
				Assert.Fail ("#Exc2");
			} catch (InvalidOperationException) {
			}

			isf.Dispose ();
			try {
				isf.FileExists ("file-a");
				Assert.Fail ("#Exc3");
			} catch (ObjectDisposedException) {
			}
		}
#endif
	}
}
