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

		[SetUp]
		public void SetUp ()
		{
			if (!SecurityManager.SecurityEnabled)
				Assert.Ignore ("SecurityManager.SecurityEnabled is OFF");
		}

		private bool RunningOnWindows {
			get { return ((int) Environment.OSVersion.Platform == 128); }
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
		[ExpectedException (typeof (SecurityException))]
		public void SetCurrentDirectory_DoesntExist ()
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
