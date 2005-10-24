//
// CodeConditionStatementCas.cs
//	- CAS unit tests for System.CodeDom.CodeConditionStatement
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
	public class CodeConditionStatementCas {

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
			CodeConditionStatement css = new CodeConditionStatement ();
			Assert.IsNull (css.Condition, "Condition");
			css.Condition = new CodeExpression ();
			Assert.AreEqual (0, css.FalseStatements.Count, "FalseStatements");
			Assert.AreEqual (0, css.TrueStatements.Count, "TrueStatements");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Constructor1_Deny_Unrestricted ()
		{
			CodeExpression condition = new CodeExpression ();
			CodeStatement[] cs = new CodeStatement[1] { new CodeStatement () };
			CodeConditionStatement css = new CodeConditionStatement (condition, cs);
			Assert.AreSame (condition, css.Condition, "Condition");
			css.Condition = new CodeExpression ();
			Assert.AreEqual (0, css.FalseStatements.Count, "FalseStatements");
			Assert.AreEqual (1, css.TrueStatements.Count, "TrueStatements");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Constructor2_Deny_Unrestricted ()
		{
			CodeExpression condition = new CodeExpression ();
			CodeStatement[] cst = new CodeStatement[1] { new CodeStatement () };
			CodeStatement[] csf = new CodeStatement[2] { new CodeStatement (), new CodeStatement () };
			CodeConditionStatement css = new CodeConditionStatement (condition, cst, csf);
			Assert.AreSame (condition, css.Condition, "Condition");
			css.Condition = new CodeExpression ();
			Assert.AreEqual (2, css.FalseStatements.Count, "FalseStatements");
			Assert.AreEqual (1, css.TrueStatements.Count, "TrueStatements");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void LinkDemand_Deny_Unrestricted ()
		{
			ConstructorInfo ci = typeof (CodeConditionStatement).GetConstructor (new Type[0]);
			Assert.IsNotNull (ci, "default .ctor");
			Assert.IsNotNull (ci.Invoke (null), "invoke");
		}
	}
}
