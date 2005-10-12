//
// System.Configuration.ConfigurationPermissionTest.cs - Unit tests
// for System.Configuration.ConfigurationPermission.
//
// Author:
//	Chris Toshok  <toshok@ximian.com>
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

#if NET_2_0

using System;
using System.Configuration;
using System.Security;
using System.Security.Permissions;
using NUnit.Framework;

namespace MonoTests.System.Configuration {
	[TestFixture]
	public class ConfigurationPermissionTest
	{
		[Test]
		public void Unrestricted ()
		{
			ConfigurationPermission p = new ConfigurationPermission (PermissionState.Unrestricted);
			Assert.IsTrue (p.IsUnrestricted(), "A1");

			p = new ConfigurationPermission (PermissionState.None);
			Assert.IsFalse (p.IsUnrestricted(), "A2");
		}

		[Test]
		public void ToXml ()
		{
			ConfigurationPermission p = new ConfigurationPermission (PermissionState.Unrestricted);

			Assert.AreEqual(
					"<IPermission class=\"System.Configuration.ConfigurationPermission, System.Configuration, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a\"\r\nversion=\"1\"\r\nUnrestricted=\"true\"/>\r\n",
					p.ToString(), "A1");


			p = new ConfigurationPermission (PermissionState.None);

			Assert.AreEqual (
					 "<IPermission class=\"System.Configuration.ConfigurationPermission, System.Configuration, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a\"\r\nversion=\"1\"/>\r\n",
					 p.ToString(), "A2");
		}
	}
}

#endif
