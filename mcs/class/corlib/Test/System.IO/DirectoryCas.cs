//
// DirectoryCas.cs - CAS unit tests for System.IO.Directory
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
using System.Collections;
using System.IO;
using System.Security;
using System.Security.Permissions;

namespace MonoCasTests.System.IO {

	[TestFixture]
	[Category ("CAS")]
	public class DirectoryCas {

		private MonoTests.System.IO.DirectoryTest dt;
		private string dir;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			// this occurs with a "clean" stack (full trust)
			dt = new MonoTests.System.IO.DirectoryTest ();
			dir = Path.Combine (Path.GetTempPath (), "MonoCasTests.System.IO");
		}

		[SetUp]
		public void SetUp ()
		{
			if (!SecurityManager.SecurityEnabled)
				Assert.Ignore ("SecurityManager.SecurityEnabled is OFF");
			dt.SetUp ();
		}

		[TearDown]
		public void TearDown () 
		{
			dt.TearDown ();
		}

		[TestFixtureTearDown]
		public void FixtureTearDown ()
		{
			if (Directory.Exists (dir))
				Directory.Delete (dir, true);
		}

		private bool RunningOnWindows {
			get {
				// check for non-Unix platforms - see FAQ for more details
				// http://www.mono-project.com/FAQ:_Technical#How_to_detect_the_execution_platform_.3F
				int platform = (int) Environment.OSVersion.Platform;
				return ((platform != 4) && (platform != 128));
			}
		}

		// Partial Trust Tests - i.e. call "normal" unit with reduced privileges

		[Test]
		[FileIOPermission (SecurityAction.PermitOnly, Unrestricted = true)]
		public void PartialTrust_PermitOnly_FileIOPermission ()
		{
			// test under limited permissions (only FileIOPermission)
			dt.CreateDirectory ();
			dt.Delete ();
			dt.Exists ();
			dt.MoveDirectory ();
			dt.LastAccessTime ();
			dt.LastWriteTime ();
			dt.GetDirectories ();
			dt.GetFiles ();
			dt.GetNoFiles ();
		}

		// test Demand by denying the required permissions

		[Test]
		[FileIOPermission (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
		public void CreateDirectory ()
		{
			// FIXME: Change Deny to imperative when supported
			Directory.CreateDirectory (dir);
		}

		[Test]
		[ExpectedException (typeof (SecurityException))]
		public void SetCurrentDirectory_DoesntExist ()
		{
			string cd = null;
			try {
				cd = Directory.GetCurrentDirectory ();
				// this will change the current directory (to / or C:\) 
				// and cause tests failures elsewhere...
				SetCurrentDirectory_DoesntExist_Restricted ();
			}
			finally {
				// ... unless we return to the original directory
				Directory.SetCurrentDirectory (cd);
			}
		}

		[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
		private void SetCurrentDirectory_DoesntExist_Restricted ()
		{
			if (RunningOnWindows) {
				Directory.SetCurrentDirectory ("C:\\D0ES-N0T-EX1ST\\");
			} else {
				Directory.SetCurrentDirectory ("/d0es-n0t-ex1st/");
			}
			// SecurityPermission before DirectoryNotFoundException
		}
	}
}
