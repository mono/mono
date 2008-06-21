//
// PathCas.cs -CAS unit tests for System.IO.Path
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
using System.Reflection;
using System.Security;
using System.Security.Permissions;

namespace MonoCasTests.System.IO {

	[TestFixture]
	[Category ("CAS")]
	public class PathCas {

		private MonoTests.System.IO.PathTest pt;
		private string cd;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			// this occurs with a "clean" stack (full trust)
			pt  = new MonoTests.System.IO.PathTest ();
			cd = Environment.CurrentDirectory;
		}

		[SetUp]
		public void SetUp ()
		{
			if (!SecurityManager.SecurityEnabled)
				Assert.Ignore ("SecurityManager.SecurityEnabled is OFF");
			pt.SetUp ();
		}

		// Partial Trust Tests - i.e. call "normal" unit with reduced privileges

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void PartialTrust_DenyUnrestricted_Success ()
		{
			// some calls do not require any permissions...
			pt.ChangeExtension ();
			pt.ChangeExtension_Extension_InvalidPathChars ();
			pt.GetDirectoryName ();
			pt.GetExtension ();
			pt.GetFileName ();
			pt.GetFileNameWithoutExtension ();
			pt.GetPathRoot2 ();
			pt.HasExtension ();
			pt.IsPathRooted ();
		}

		[Test]
		[EnvironmentPermission (SecurityAction.PermitOnly, Unrestricted = true)]
		public void PartialTrust_PermitOnlyEnvironment ()
		{
			// ... some methods (or tests) require to read environment variables...
			pt.GetTempPath ();
		}

		[Test]
		[EnvironmentPermission (SecurityAction.PermitOnly, Unrestricted = true)]
		[FileIOPermission (SecurityAction.PermitOnly, Unrestricted = true)]
		public void PartialTrust_PermitOnlyEnvironmentFileIO ()
		{
			// ... some methods (or tests) require to read environment variables
			// and file i/o permissions ...
			pt.Combine ();
			pt.GetTempFileName ();
		}

		[Test]
		[FileIOPermission (SecurityAction.PermitOnly, Unrestricted = true)]
		public void PartialTrust_PermitOnlyFileIO ()
		{
			// ... while others do need only FileIOPermission
			pt.GetFullPath2 ();
			pt.CanonicalizeDots ();	// only calls Path.GetFullPath
		}

		// test Demand by denying the required permissions

		[Test]
		[FileIOPermission (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
		public void GetFullPath1 ()
		{
			Assert.IsNotNull (Path.GetFullPath (cd));
		}

		[Test]
		[FileIOPermission (SecurityAction.PermitOnly, Unrestricted = true)]
		public void GetFullPath2 ()
		{
			Assert.IsNotNull (Path.GetFullPath (cd));
		}

		[Test]
		[EnvironmentPermission (SecurityAction.PermitOnly, Unrestricted = true)]
		public void GetTempFileName1 ()
		{
			Assert.IsNotNull (Path.GetTempFileName ());
			// i.e. no FileIOPermission is required to get a temporary filename
		}

		[Test]
		[EnvironmentPermission (SecurityAction.Deny, Write = "USERNAME")]
		[ExpectedException (typeof (SecurityException))]
		public void GetTempFileName2 ()
		{
			// yep, Write USERNAME don't make sense - unless the callee
			// (indirectly) requires Unrestricted access for EnvironmentPermission 
			Assert.IsNotNull (Path.GetTempFileName ());
		}

		[Test]
		[EnvironmentPermission (SecurityAction.PermitOnly, Unrestricted = true)]
		public void GetTempPath1 ()
		{
			Assert.IsNotNull (Path.GetTempPath ());
			// i.e. no FileIOPermission is required to get the temporary directory
		}

		[Test]
		[EnvironmentPermission (SecurityAction.Deny, Write = "USERNAME")]
		[ExpectedException (typeof (SecurityException))]
		public void GetTempPath2 ()
		{
			// yep, Write USERNAME don't make sense - unless the callee
			// requires Unrestricted access for EnvironmentPermission 
			Assert.IsNotNull (Path.GetTempPath ());
		}

		// many calls work only on strings (i.e. they dont access the file system)
		// so no Demand for FileIOPermission (or any other) are required

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void NoFileIOPermission ()
		{
			Assert.IsNotNull (Path.ChangeExtension ("test.doc", "txt"), "ChangeExtension");
			string combine = Path.Combine ("dir", "test.txt");
			Assert.IsNotNull (combine, "Combine");
			Assert.IsNotNull (Path.GetDirectoryName (combine), "GetDirectoryName");
			Assert.IsNotNull (Path.GetExtension ("test.txt"), "GetExtension");
			Assert.IsNotNull (Path.GetFileName ("test.txt"), "GetFileName");
			Assert.IsNotNull (Path.GetFileNameWithoutExtension ("test.txt"), "GetFileNameWithoutExtension");
			Assert.IsNotNull (Path.GetPathRoot (cd), "GetPathRoot");
			Assert.IsTrue (Path.HasExtension ("test.txt"), "HasExtension");
			Assert.IsFalse (Path.IsPathRooted (combine), "IsPathRooted");
		}
	}
}
