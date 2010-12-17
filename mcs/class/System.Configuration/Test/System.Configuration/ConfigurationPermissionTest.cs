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
		public void Intersect ()
		{
			ConfigurationPermission p1 = new ConfigurationPermission (PermissionState.Unrestricted);
			ConfigurationPermission p2 = new ConfigurationPermission (PermissionState.None);

			IPermission p3 = p1.Intersect (p2);

			Assert.AreEqual (typeof (ConfigurationPermission), p3.GetType(), "A1");

			Assert.IsFalse (((ConfigurationPermission)p3).IsUnrestricted(), "A2");
		}

		[Test]
		public void Intersect_null ()
		{
			ConfigurationPermission p1 = new ConfigurationPermission (PermissionState.Unrestricted);

			IPermission p3 = p1.Intersect (null);

			Assert.IsNull (p3, "A1");
		}

#if !TARGET_JVM
		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Intersect_wrongtype ()
		{
			ConfigurationPermission p1 = new ConfigurationPermission (PermissionState.Unrestricted);

			IPermission p3 = p1.Intersect (new StrongNameIdentityPermission (PermissionState.Unrestricted));
		}
#endif

		[Test]
		public void Union ()
		{
			ConfigurationPermission p1 = new ConfigurationPermission (PermissionState.Unrestricted);
			ConfigurationPermission p2 = new ConfigurationPermission (PermissionState.None);

			IPermission p3 = p1.Union (p2);

			Assert.AreEqual (typeof (ConfigurationPermission), p3.GetType(), "A1");

			Assert.IsTrue (((ConfigurationPermission)p3).IsUnrestricted(), "A2");
		}

		[Test]
		public void Union_null ()
		{
			ConfigurationPermission p1 = new ConfigurationPermission (PermissionState.Unrestricted);

			IPermission p3 = p1.Union (null);

			Assert.AreEqual (typeof (ConfigurationPermission), p3.GetType(), "A1");

			Assert.IsTrue (((ConfigurationPermission)p3).IsUnrestricted(), "A2");
		}

#if !TARGET_JVM
		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Union_wrongtypee ()
		{
			ConfigurationPermission p1 = new ConfigurationPermission (PermissionState.Unrestricted);

			IPermission p3 = p1.Union (new StrongNameIdentityPermission (PermissionState.Unrestricted));
		}
#endif

		[Test]
		public void Subset ()
		{
			ConfigurationPermission p1 = new ConfigurationPermission (PermissionState.Unrestricted);
			ConfigurationPermission p2 = new ConfigurationPermission (PermissionState.None);

			Assert.IsFalse (p1.IsSubsetOf (p2), "A1");
			Assert.IsTrue  (p1.IsSubsetOf (p1), "A2");
			Assert.IsTrue  (p2.IsSubsetOf (p1), "A3");
			Assert.IsTrue  (p2.IsSubsetOf (p2), "A4");

			Assert.IsFalse (p1.IsSubsetOf (null), "A5");
			Assert.IsTrue (p2.IsSubsetOf (null), "A6");
		}

#if !TARGET_JVM
		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Subset_wrongtype ()
		{
			ConfigurationPermission p1 = new ConfigurationPermission (PermissionState.Unrestricted);

			Assert.IsFalse (p1.IsSubsetOf (new StrongNameIdentityPermission (PermissionState.Unrestricted)));
		}
#endif

		[Test]
#if TARGET_JVM
		[Category("NotWorking")]
#endif
		public void ToXml ()
		{
			ConfigurationPermission p = new ConfigurationPermission (PermissionState.Unrestricted);

#if NET_4_0
			Assert.AreEqual(
					"<IPermission class=\"System.Configuration.ConfigurationPermission, System.Configuration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a\"\nversion=\"1\"\nUnrestricted=\"true\"/>\n",
					p.ToString().Replace ("\r\n", "\n"), "A1");
#else
			Assert.AreEqual (
					"<IPermission class=\"System.Configuration.ConfigurationPermission, System.Configuration, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a\"\nversion=\"1\"\nUnrestricted=\"true\"/>\n",
					p.ToString ().Replace ("\r\n", "\n"), "A1");
#endif

			p = new ConfigurationPermission (PermissionState.None);

#if NET_4_0
			Assert.AreEqual (
					 "<IPermission class=\"System.Configuration.ConfigurationPermission, System.Configuration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a\"\nversion=\"1\"/>\n",
					 p.ToString().Replace ("\r\n", "\n"), "A2");
#else
			Assert.AreEqual (
					 "<IPermission class=\"System.Configuration.ConfigurationPermission, System.Configuration, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a\"\nversion=\"1\"/>\n",
					 p.ToString ().Replace ("\r\n", "\n"), "A2");
#endif
		}
	}
}

