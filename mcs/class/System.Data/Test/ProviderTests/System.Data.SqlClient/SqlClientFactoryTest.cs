//
// SqlDataAdapterTest.cs - NUnit Test Cases for testing the
//                          SqlDataAdapter class
// Author:
//      Nidhi Rawal (rawalnidhi_rawal@yahoo.com)
//
// Copyright (c) 2007 Novell Inc., and the individuals listed
// on the ChangeLog entries.
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
using System.Data;
using System.Data.SqlClient;
using System.Net;
using NUnit.Framework;
using System.Collections;
using System.Security.Permissions;

using System.Security;

namespace MonoTests.System.Data.Connected.SqlClient
{
	[TestFixture]
	[Category ("CAS")]
	public class SqlClientFactoryTest
	{
		[Test]
		public void CreatePermissionTest ()
		{
			SqlClientFactory factory = SqlClientFactory.Instance;
			CodeAccessPermission permission, perm;
			permission = factory.CreatePermission (PermissionState.None);
			perm = factory.CreatePermission (PermissionState.Unrestricted);
			Assert.AreEqual (false, perm.IsSubsetOf (permission), "#1");
		}
	}
}

