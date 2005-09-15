//
// RootBuilderCas.cs - CAS unit tests for System.Web.UI.RootBuilder
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
using System.Reflection;
using System.Security.Permissions;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;

namespace MonoCasTests.System.Web.UI {

	[TestFixture]
	[Category ("CAS")]
	public class RootBuilderCas : AspNetHostingMinimal {

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Ctor1_Deny_Unrestricted ()
		{
			RootBuilder rb = new RootBuilder (new PageParser ());
			try {
				rb.GetChildControlType (null, null);
			}
			catch (ArgumentNullException) {
				// mono and ms 1.x
			}
			catch (NullReferenceException) {
				// ms 2.0 - more likely parameters don't change this result
			}
#if NET_2_0
			Assert.IsNotNull (rb.BuiltObjects, "BuiltObjects");
#endif
		}

#if NET_2_0
		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Ctor0_Deny_Unrestricted ()
		{
			RootBuilder rb = new RootBuilder ();
			Assert.IsNotNull (rb.BuiltObjects, "BuiltObjects");
		}
#endif

		// LinkDemand

		public override object CreateControl (SecurityAction action, AspNetHostingPermissionLevel level)
		{
			ConstructorInfo ci = this.Type.GetConstructor (new Type[1] { typeof (TemplateParser) });
			Assert.IsNotNull (ci, ".ctor(TemplateParser)");
			return ci.Invoke (new object[1] { null });
		}

		public override Type Type {
			get { return typeof (RootBuilder); }
		}
	}
}
