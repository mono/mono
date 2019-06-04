//
// TempFileCollectionCas.cs 
//	- CAS unit tests for System.CodeDom.Compiler.TempFileCollection
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
using System.CodeDom.Compiler;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Security;
using System.Security.Permissions;

using MonoTests.Helpers;

namespace MonoCasTests.System.CodeDom.Compiler {

	[TestFixture]
	[Category ("CAS")]
	public class TempFileCollectionCas {

		private TempDirectory _temp;
		private string temp;
		private string[] array;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			// at full trust
			_temp = new TempDirectory ();
			temp = _temp.Path;
			array = new string[1];
		}

		[SetUp]
		public void SetUp ()
		{
			if (!SecurityManager.SecurityEnabled)
				Assert.Ignore ("SecurityManager.SecurityEnabled is OFF");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Constructor0_Deny_Unrestricted ()
		{
			TempFileCollection tfc = new TempFileCollection ();
			Assert.AreEqual (0, tfc.Count, "Count");
			Assert.IsFalse (tfc.KeepFiles, "KeepFiles");
			Assert.AreEqual (String.Empty, tfc.TempDir, "TempDir");
			tfc.AddFile ("main.cs", false);
			tfc.CopyTo (array, 0);
			tfc.Delete ();
			(tfc as IDisposable).Dispose ();
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Constructor1_Deny_Unrestricted ()
		{
			TempFileCollection tfc = new TempFileCollection (temp);
			Assert.AreEqual (0, tfc.Count, "Count");
			Assert.IsFalse (tfc.KeepFiles, "KeepFiles");
			Assert.AreEqual (temp, tfc.TempDir, "TempDir");
			tfc.AddFile ("main.cs", false);
			tfc.CopyTo (array, 0);
			tfc.Delete ();
			(tfc as IDisposable).Dispose ();
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Constructor2_Deny_Unrestricted ()
		{
			TempFileCollection tfc = new TempFileCollection (temp, true);
			Assert.AreEqual (0, tfc.Count, "Count");
			Assert.IsTrue (tfc.KeepFiles, "KeepFiles");
			Assert.AreEqual (temp, tfc.TempDir, "TempDir");
			tfc.AddFile ("main.cs", false);
			tfc.CopyTo (array, 0);
			tfc.Delete ();
			(tfc as IDisposable).Dispose ();
		}


		[EnvironmentPermission (SecurityAction.PermitOnly, Unrestricted = true)]
		private void Cache (TempFileCollection tfc)
		{
			Assert.IsNotNull (tfc.BasePath, "BasePath-Restricted");
			// no failure because the BasePath was calculated when 
			// unrestricted and cached for further calls. This is
			// design tradeoff between security and performance but
			// shouldn't occurs much in real life (but it's worth
			// keeping in mind ;-).
		}

		[Test]
		public void BasePath_Caching ()
		{
			TempFileCollection tfc = new TempFileCollection ();
			Assert.IsNotNull (tfc.BasePath, "BasePath-Unrestricted");
			Cache (tfc);
		}

		[Test]
		[EnvironmentPermission (SecurityAction.PermitOnly, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
		public void BasePath_PermitOnly_EnvironmentPermission ()
		{
			TempFileCollection tfc = new TempFileCollection ();
			// good but not enough
			Assert.IsNotNull (tfc.BasePath, "BasePath");
		}

		[Test]
		[EnvironmentPermission (SecurityAction.Deny, Read = "Mono")]
		[ExpectedException (typeof (SecurityException))]
		public void BasePath_Deny_EnvironmentPermission ()
		{
			TempFileCollection tfc = new TempFileCollection ();
			// requires Unrestricted Environment access
			Assert.IsNotNull (tfc.BasePath, "BasePath");
		}

		[Test]
		[FileIOPermission (SecurityAction.PermitOnly, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
		public void BasePath_PermitOnly_FileIOPermission ()
		{
			TempFileCollection tfc = new TempFileCollection ();
			// good but not enough
			Assert.IsNotNull (tfc.BasePath, "BasePath");
		}

		[Test]
		[FileIOPermission (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
		public void BasePath_Deny_FileIOPermission ()
		{
			TempFileCollection tfc = new TempFileCollection ();
			// requires path access
			Assert.IsNotNull (tfc.BasePath, "BasePath");
		}

		[Test]
		[EnvironmentPermission (SecurityAction.PermitOnly, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
		public void AddExtension_PermitOnly_EnvironmentPermission ()
		{
			TempFileCollection tfc = new TempFileCollection ();
			// good but not enough
			tfc.AddExtension (".cs");
		}

		[Test]
		[EnvironmentPermission (SecurityAction.Deny, Read = "Mono")]
		[ExpectedException (typeof (SecurityException))]
		public void AddExtension_Deny_EnvironmentPermission ()
		{
			TempFileCollection tfc = new TempFileCollection ();
			// requires Unrestricted Environment access
			tfc.AddExtension (".cs");
		}

		[Test]
		[FileIOPermission (SecurityAction.PermitOnly, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
		public void AddExtension_PermitOnly_FileIOPermission ()
		{
			TempFileCollection tfc = new TempFileCollection ();
			// good but not enough
			tfc.AddExtension (".cs");
		}

		[Test]
		[FileIOPermission (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
		public void AddExtension_Deny_FileIOPermission ()
		{
			TempFileCollection tfc = new TempFileCollection ();
			// requires path access
			tfc.AddExtension (".cs");
		}

		[Test]
		[EnvironmentPermission (SecurityAction.PermitOnly, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
		public void AddExtension2_PermitOnly_EnvironmentPermission ()
		{
			TempFileCollection tfc = new TempFileCollection ();
			// good but not enough
			tfc.AddExtension (".cx", true);
		}

		[Test]
		[EnvironmentPermission (SecurityAction.Deny, Read = "Mono")]
		[ExpectedException (typeof (SecurityException))]
		public void AddExtension2_Deny_EnvironmentPermission ()
		{
			TempFileCollection tfc = new TempFileCollection ();
			// requires Unrestricted Environment access
			tfc.AddExtension (".cx", true);
		}

		[Test]
		[FileIOPermission (SecurityAction.PermitOnly, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
		public void AddExtension2_PermitOnly_FileIOPermission ()
		{
			TempFileCollection tfc = new TempFileCollection ();
			// good but not enough
			tfc.AddExtension (".cx", true);
		}

		[Test]
		[FileIOPermission (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
		public void AddExtension2_Deny_FileIOPermission ()
		{
			TempFileCollection tfc = new TempFileCollection ();
			// requires path access
			tfc.AddExtension (".cx", true);
		}

		[Test]
		[FileIOPermission (SecurityAction.PermitOnly, Unrestricted = true)]
		[EnvironmentPermission (SecurityAction.PermitOnly, Unrestricted = true)]
		public void PermitOnly_FileIOPermission_EnvironmentPermission ()
		{
			TempFileCollection tfc = new TempFileCollection ();
			// ok
			Assert.IsNotNull (tfc.BasePath, "BasePath");
			tfc.AddExtension (".cs");
			tfc.AddExtension (".cx", true);
			// both AddExtension methods depends on BasePath
			Assert.AreEqual (String.Empty, tfc.TempDir, "TempDir");
			// and that last one is *important*
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void ICollection_Deny_Unrestricted ()
		{
			ICollection coll = (ICollection) new TempFileCollection ();
			Assert.AreEqual (0, coll.Count, "Count");
			Assert.IsNull (coll.SyncRoot, "SyncRoot");
			Assert.IsFalse (coll.IsSynchronized, "IsSynchronized");
			coll.CopyTo (array, 0);
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void IEnumerable_Deny_Unrestricted ()
		{
			IEnumerable e = (IEnumerable) new TempFileCollection ();
			Assert.IsNotNull (e.GetEnumerator (), "GetEnumerator");
		}

		[Test]
		public void LinkDemand_No_Restriction ()
		{
			ConstructorInfo ci = typeof (TempFileCollection).GetConstructor (new Type[0]);
			Assert.IsNotNull (ci, "default .ctor");
			Assert.IsNotNull (ci.Invoke (null), "invoke");
		}

		[Test]
		[EnvironmentPermission (SecurityAction.Deny, Read = "Mono")]
		[ExpectedException (typeof (SecurityException))]
		public void LinkDemand_Deny_Unrestricted ()
		{
			// denying anything results in a non unrestricted permission set
			ConstructorInfo ci = typeof (TempFileCollection).GetConstructor (new Type[0]);
			Assert.IsNotNull (ci, "default .ctor");
			Assert.IsNotNull (ci.Invoke (null), "invoke");
		}
	}
}
