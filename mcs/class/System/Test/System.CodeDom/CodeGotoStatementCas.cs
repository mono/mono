//
// CodeGotoStatementCas.cs
//	- CAS unit tests for System.CodeDom.CodeGotoStatement
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
using System.CodeDom;
using System.Reflection;
using System.Security;
using System.Security.Permissions;

namespace MonoCasTests.System.CodeDom {

	[TestFixture]
	[Category ("CAS")]
	public class CodeGotoStatementCas {

		[SetUp]
		public void SetUp ()
		{
			if (!SecurityManager.SecurityEnabled)
				Assert.Ignore ("SecurityManager.SecurityEnabled is OFF");
		}
		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Constructor0_Deny_Unrestricted ()
		{
			CodeGotoStatement cgs = new CodeGotoStatement ();
			Assert.IsNull (cgs.Label, "Label");
			cgs.Label = "mono";
		}
		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Constructor1_Deny_Unrestricted ()
		{
			CodeGotoStatement cgs = new CodeGotoStatement ("mono");
			Assert.AreEqual ("mono", cgs.Label, "Label");
			cgs.Label = "monkey"; // doesn't accept String.Empty
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void LinkDemand_Deny_Unrestricted ()
		{
			// the default .ctor was added in 2.0
			ConstructorInfo ci = typeof (CodeGotoStatement).GetConstructor (new Type[1] { typeof (string) });
			Assert.IsNotNull (ci, ".ctor(string)");
			Assert.IsNotNull (ci.Invoke (new object[1] { "mono" }), "invoke");
		}
	}
}
