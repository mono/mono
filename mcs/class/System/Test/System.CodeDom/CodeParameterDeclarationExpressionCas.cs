//
// CodeParameterDeclarationExpressionCas.cs
//	- CAS unit tests for System.CodeDom.CodeParameterDeclarationExpression
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
	public class CodeParameterDeclarationExpressionCas {

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
			CodeParameterDeclarationExpression cpde = new CodeParameterDeclarationExpression ();
			Assert.AreEqual (0, cpde.CustomAttributes.Count, "CustomAttributes");
			cpde.CustomAttributes = new CodeAttributeDeclarationCollection ();
			Assert.AreEqual (FieldDirection.In, cpde.Direction, "Direction");
			cpde.Direction = FieldDirection.Out;
			Assert.AreEqual (String.Empty, cpde.Name, "Name");
			cpde.Name = "mono";
			Assert.AreEqual ("System.Void", cpde.Type.BaseType, "Type");
			cpde.Type = new CodeTypeReference ("System.Int32");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Constructor1_Deny_Unrestricted ()
		{
			CodeTypeReference type = new CodeTypeReference ("System.Void");
			CodeParameterDeclarationExpression cpde = new CodeParameterDeclarationExpression (type, "mono");
			Assert.AreEqual (0, cpde.CustomAttributes.Count, "CustomAttributes");
			cpde.CustomAttributes = new CodeAttributeDeclarationCollection ();
			Assert.AreEqual (FieldDirection.In, cpde.Direction, "Direction");
			cpde.Direction = FieldDirection.Out;
			Assert.AreEqual ("mono", cpde.Name, "Name");
			cpde.Name = String.Empty;
			Assert.AreSame (type, cpde.Type, "Type");
			cpde.Type = new CodeTypeReference ("System.Int32");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Constructor2_Deny_Unrestricted ()
		{
			CodeParameterDeclarationExpression cpde = new CodeParameterDeclarationExpression ("System.Int32", "mono");
			Assert.AreEqual (0, cpde.CustomAttributes.Count, "CustomAttributes");
			cpde.CustomAttributes = new CodeAttributeDeclarationCollection ();
			Assert.AreEqual (FieldDirection.In, cpde.Direction, "Direction");
			cpde.Direction = FieldDirection.Out;
			Assert.AreEqual ("mono", cpde.Name, "Name");
			cpde.Name = String.Empty;
			Assert.AreEqual ("System.Int32", cpde.Type.BaseType, "Type");
			cpde.Type = new CodeTypeReference ("System.Int32");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Constructor3_Deny_Unrestricted ()
		{
			CodeParameterDeclarationExpression cpde = new CodeParameterDeclarationExpression (typeof (int), "mono");
			Assert.AreEqual (0, cpde.CustomAttributes.Count, "CustomAttributes");
			cpde.CustomAttributes = new CodeAttributeDeclarationCollection ();
			Assert.AreEqual (FieldDirection.In, cpde.Direction, "Direction");
			cpde.Direction = FieldDirection.Out;
			Assert.AreEqual ("mono", cpde.Name, "Name");
			cpde.Name = String.Empty;
			Assert.AreEqual ("System.Int32", cpde.Type.BaseType, "Type");
			cpde.Type = new CodeTypeReference ("System.Int32");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void LinkDemand_Deny_Unrestricted ()
		{
			ConstructorInfo ci = typeof (CodeParameterDeclarationExpression).GetConstructor (new Type[0]);
			Assert.IsNotNull (ci, "default .ctor");
			Assert.IsNotNull (ci.Invoke (null), "invoke");
		}
	}
}
