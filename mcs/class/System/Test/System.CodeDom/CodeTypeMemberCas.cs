//
// CodeTypeMemberCas.cs 
//	- CAS unit tests for System.CodeDom.CodeTypeMember
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
	public class CodeTypeMemberCas {

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
			CodeTypeMember ctm = new CodeTypeMember ();
			Assert.AreEqual (MemberAttributes.Private | MemberAttributes.Final, ctm.Attributes, "Attributes");
			ctm.Attributes = MemberAttributes.Public;
			Assert.AreEqual (0, ctm.Comments.Count, "Comments");
			Assert.AreEqual (0, ctm.CustomAttributes.Count, "CustomAttributes");
			ctm.CustomAttributes = new CodeAttributeDeclarationCollection ();
			Assert.IsNull (ctm.LinePragma, "LinePragma");
			ctm.LinePragma = new CodeLinePragma (String.Empty, Int32.MaxValue);
			Assert.AreEqual (String.Empty, ctm.Name, "Name");
			ctm.Name = "mono";
			Assert.AreEqual (0, ctm.StartDirectives.Count, "StartDirectives");
			Assert.AreEqual (0, ctm.EndDirectives.Count, "EndDirectives");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void LinkDemand_Deny_Unrestricted ()
		{
			ConstructorInfo ci = typeof (CodeTypeMember).GetConstructor (new Type[0]);
			Assert.IsNotNull (ci, "default .ctor");
			Assert.IsNotNull (ci.Invoke (null), "invoke");
		}
	}
}
