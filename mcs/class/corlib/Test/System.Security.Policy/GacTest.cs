//
// GacInstalledTest.cs - NUnit Test Cases for GacInstalled
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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

using NUnit.Framework;
using System;
using System.Security;
using System.Security.Policy;
using System.Security.Permissions;

namespace MonoTests.System.Security.Policy {

	[TestFixture]
	public class GacInstalledTest {

		[Test]
		public void Constructor () 
		{
			GacInstalled gac = new GacInstalled ();
			Assert.IsNotNull (gac);
		}

		[Test]
		public void Copy ()
		{
			GacInstalled gac = new GacInstalled ();
			GacInstalled copy = (GacInstalled)gac.Copy ();
			Assert.AreEqual (gac, copy, "Equals");
			Assert.IsFalse (Object.ReferenceEquals (gac, copy), "ReferenceEquals");
		}

		[Test]
		public void CreateIdentityPermission ()
		{
			GacInstalled gac = new GacInstalled ();
			IPermission p = gac.CreateIdentityPermission (null);
			Assert.IsTrue ((p is GacIdentityPermission), "GacIdentityPermission");
			Assert.IsNotNull (p, "CreateIdentityPermission(null)");
			p = gac.CreateIdentityPermission (new Evidence ());
			Assert.IsNotNull (p, "CreateIdentityPermission(Evidence)");
		}

		[Test]
		public void Equals ()
		{
			GacInstalled gac = new GacInstalled ();
			Assert.IsFalse (gac.Equals (null), "Equals(null)");
			GacInstalled g2 = new GacInstalled ();
			Assert.IsTrue (gac.Equals (g2), "Equals(g2)");
			Assert.IsTrue (g2.Equals (gac), "Equals(gac)");
		}

		[Test]
		public void GetHashCode_ ()
		{
			GacInstalled gac = new GacInstalled ();
			Assert.AreEqual (0, gac.GetHashCode ());
		}

		[Test]
		public void ToString_ ()
		{
			GacInstalled gac = new GacInstalled ();
			Assert.IsTrue (gac.ToString ().StartsWith ("<System.Security.Policy.GacInstalled version=\"1\"/>"));
		}
	}
}

#endif
