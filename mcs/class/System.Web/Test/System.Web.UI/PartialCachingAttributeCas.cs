//
// PartialCachingAttributeCas.cs 
//	- CAS unit tests for System.Web.UI.PartialCachingAttribute
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
using System.Security.Permissions;
using System.Web;
using System.Web.UI;

namespace MonoCasTests.System.Web.UI {

	[TestFixture]
	[Category ("CAS")]
	public class PartialCachingAttributeCas : AspNetHostingMinimal {

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Ctor1_Deny_Unrestricted ()
		{
			PartialCachingAttribute pca = new PartialCachingAttribute (Int32.MinValue);
			Assert.AreEqual (Int32.MinValue, pca.Duration, "Duration");
			Assert.IsNull (pca.VaryByControls, "VaryByControls");
			Assert.IsNull (pca.VaryByCustom, "VaryByCustom");
			Assert.IsNull (pca.VaryByParams, "VaryByParams");
#if NET_2_0
			Assert.IsNull (pca.SqlDependency, "SqlDependency");
#endif
			Assert.IsFalse (pca.Shared, "Shared");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Ctor4_Deny_Unrestricted ()
		{
			PartialCachingAttribute pca = new PartialCachingAttribute (Int32.MaxValue, "1", "2", "3");
			Assert.AreEqual (Int32.MaxValue, pca.Duration, "Duration");
			Assert.AreEqual ("2", pca.VaryByControls, "VaryByControls");
			Assert.AreEqual ("3", pca.VaryByCustom, "VaryByCustom");
			Assert.AreEqual ("1", pca.VaryByParams, "VaryByParams");
#if NET_2_0
			Assert.IsNull (pca.SqlDependency, "SqlDependency");
#endif
			Assert.IsFalse (pca.Shared, "Shared");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Ctor5_Deny_Unrestricted ()
		{
			PartialCachingAttribute pca = new PartialCachingAttribute (0, "a", "b", "c", true);
			Assert.AreEqual (0, pca.Duration, "Duration");
			Assert.AreEqual ("b", pca.VaryByControls, "VaryByControls");
			Assert.AreEqual ("c", pca.VaryByCustom, "VaryByCustom");
			Assert.AreEqual ("a", pca.VaryByParams, "VaryByParams");
#if NET_2_0
			Assert.IsNull (pca.SqlDependency, "SqlDependency");
#endif
			Assert.IsTrue (pca.Shared, "Shared");
		}

#if NET_2_0
		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Ctor6_Deny_Unrestricted ()
		{
			PartialCachingAttribute pca = new PartialCachingAttribute (0, "a", "b", "c", "sql", false);
			Assert.AreEqual (0, pca.Duration, "Duration");
			Assert.AreEqual ("b", pca.VaryByControls, "VaryByControls");
			Assert.AreEqual ("c", pca.VaryByCustom, "VaryByCustom");
			Assert.AreEqual ("a", pca.VaryByParams, "VaryByParams");
			Assert.AreEqual ("sql", pca.SqlDependency, "SqlDependency");
			Assert.IsFalse (pca.Shared, "Shared");
		}
#endif

		// LinkDemand

		public override object CreateControl (SecurityAction action, AspNetHostingPermissionLevel level)
		{
			ConstructorInfo ci = this.Type.GetConstructor (new Type[1] { typeof (int) });
			Assert.IsNotNull (ci, ".ctor(int)");
			return ci.Invoke (new object[1] { 0 });
		}

		public override Type Type {
			get { return typeof (PartialCachingAttribute); }
		}
	}
}
