//
// ResourcePermissionBaseEntryTest.cs -
//	NUnit Test Cases for ResourcePermissionBaseEntry
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
using System.Security.Permissions;

namespace MonoTests.System.Security.Permissions {

	[TestFixture]
	public class ResourcePermissionBaseEntryTest {

		[Test]
		public void Constructor_Default () 
		{
			ResourcePermissionBaseEntry rpbe = new ResourcePermissionBaseEntry ();
			Assert.AreEqual (0, rpbe.PermissionAccess, "PermissionAccess");
			Assert.AreEqual (0, rpbe.PermissionAccessPath.Length, "PermissionAccessPath");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor_Null ()
		{
			ResourcePermissionBaseEntry rpbe = new ResourcePermissionBaseEntry (0, null);
		}

		[Test]
		public void Constructor_Negative ()
		{
			ResourcePermissionBaseEntry rpbe = new ResourcePermissionBaseEntry (Int32.MinValue, new string [1]);
			Assert.AreEqual (Int32.MinValue, rpbe.PermissionAccess, "PermissionAccess");
			Assert.AreEqual (1, rpbe.PermissionAccessPath.Length, "PermissionAccessPath");
		}

		[Test]
		public void Constructor_IntString ()
		{
			ResourcePermissionBaseEntry rpbe = new ResourcePermissionBaseEntry (Int32.MaxValue, new string [10]);
			Assert.AreEqual (Int32.MaxValue, rpbe.PermissionAccess, "PermissionAccess");
			Assert.AreEqual (10, rpbe.PermissionAccessPath.Length, "PermissionAccessPath");
		}
	}
}
