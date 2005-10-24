//
// CodeVariableDeclarationStatementCas.cs 
//	- CAS unit tests for System.CodeDom.CodeVariableDeclarationStatement
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
	public class CodeVariableDeclarationStatementCas {

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
			CodeVariableDeclarationStatement cvds = new CodeVariableDeclarationStatement ();
			Assert.IsNull (cvds.InitExpression, "InitExpression");
			cvds.InitExpression = new CodeExpression ();
			Assert.AreEqual (String.Empty, cvds.Name, "Name");
			cvds.Name = "mono";
			Assert.AreEqual ("System.Void", cvds.Type.BaseType, "Type");
			cvds.Type = new CodeTypeReference ("System.Int32");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Constructor1_Deny_Unrestricted ()
		{
			CodeTypeReference type = new CodeTypeReference ("System.Int32");
			CodeVariableDeclarationStatement cvds = new CodeVariableDeclarationStatement (type, "mono");
			Assert.IsNull (cvds.InitExpression, "InitExpression");
			cvds.InitExpression = new CodeExpression ();
			Assert.AreEqual ("mono", cvds.Name, "Name");
			cvds.Name = String.Empty;
			Assert.AreSame (type, cvds.Type, "Type");
			cvds.Type = new CodeTypeReference ("System.Void");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Constructor2_Deny_Unrestricted ()
		{
			CodeVariableDeclarationStatement cvds = new CodeVariableDeclarationStatement ("System.Int32", "mono");
			Assert.IsNull (cvds.InitExpression, "InitExpression");
			cvds.InitExpression = new CodeExpression ();
			Assert.AreEqual ("mono", cvds.Name, "Name");
			cvds.Name = String.Empty;
			Assert.AreEqual ("System.Int32", cvds.Type.BaseType, "Type");
			cvds.Type = new CodeTypeReference ("System.Void");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Constructor3_Deny_Unrestricted ()
		{
			CodeVariableDeclarationStatement cvds = new CodeVariableDeclarationStatement (typeof (int), "mono");
			Assert.IsNull (cvds.InitExpression, "InitExpression");
			cvds.InitExpression = new CodeExpression ();
			Assert.AreEqual ("mono", cvds.Name, "Name");
			cvds.Name = String.Empty;
			Assert.AreEqual ("System.Int32", cvds.Type.BaseType, "Type");
			cvds.Type = new CodeTypeReference ("System.Void");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Constructor4_Deny_Unrestricted ()
		{
			CodeTypeReference type = new CodeTypeReference ("System.Int32");
			CodeExpression init = new CodeExpression ();
			CodeVariableDeclarationStatement cvds = new CodeVariableDeclarationStatement (type, "mono", init);
			Assert.AreSame (init, cvds.InitExpression, "InitExpression");
			cvds.InitExpression = new CodeExpression ();
			Assert.AreEqual ("mono", cvds.Name, "Name");
			cvds.Name = String.Empty;
			Assert.AreSame (type, cvds.Type, "Type");
			cvds.Type = new CodeTypeReference ("System.Void");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Constructor5_Deny_Unrestricted ()
		{
			CodeExpression init = new CodeExpression ();
			CodeVariableDeclarationStatement cvds = new CodeVariableDeclarationStatement ("System.Int32", "mono", init);
			Assert.AreSame (init, cvds.InitExpression, "InitExpression");
			cvds.InitExpression = new CodeExpression ();
			Assert.AreEqual ("mono", cvds.Name, "Name");
			cvds.Name = String.Empty;
			Assert.AreEqual ("System.Int32", cvds.Type.BaseType, "Type");
			cvds.Type = new CodeTypeReference ("System.Void");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Constructor6_Deny_Unrestricted ()
		{
			CodeExpression init = new CodeExpression ();
			CodeVariableDeclarationStatement cvds = new CodeVariableDeclarationStatement (typeof (int), "mono", init);
			Assert.AreSame (init, cvds.InitExpression, "InitExpression");
			cvds.InitExpression = new CodeExpression ();
			Assert.AreEqual ("mono", cvds.Name, "Name");
			cvds.Name = String.Empty;
			Assert.AreEqual ("System.Int32", cvds.Type.BaseType, "Type");
			cvds.Type = new CodeTypeReference ("System.Void");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void LinkDemand_Deny_Unrestricted ()
		{
			ConstructorInfo ci = typeof (CodeVariableDeclarationStatement).GetConstructor (new Type[0]);
			Assert.IsNotNull (ci, "default .ctor");
			Assert.IsNotNull (ci.Invoke (null), "invoke");
		}
	}
}
