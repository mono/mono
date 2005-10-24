//
// CodeArrayCreateExpressionCas.cs
//	- CAS unit tests for System.CodeDom.CodeArrayCreateExpression
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
	public class CodeArrayCreateExpressionCas {

		private CodeTypeReference ctr;
		private CodeExpression ce;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			// at fulltrust
			ctr = new CodeTypeReference ("System.Void");
			ce = new CodeExpression ();
		}

		[SetUp]
		public void SetUp ()
		{
			if (!SecurityManager.SecurityEnabled)
				Assert.Ignore ("SecurityManager.SecurityEnabled is OFF");
		}

		private void CheckProperties (CodeArrayCreateExpression cace)
		{
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Constructor0_Deny_Unrestricted ()
		{
			CodeArrayCreateExpression cace = new CodeArrayCreateExpression ();
			Assert.AreEqual ("System.Void", cace.CreateType.BaseType, "CreateType.BaseType");
			Assert.AreEqual (0, cace.Initializers.Count, "Initializers");
			Assert.AreEqual (0, cace.Size, "Size");
			Assert.IsNull (cace.SizeExpression, "SizeExpression");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Constructor1_Deny_Unrestricted ()
		{
			CodeArrayCreateExpression cace = new CodeArrayCreateExpression (ctr, ce);
			Assert.AreEqual ("System.Void", cace.CreateType.BaseType, "CreateType.BaseType");
			Assert.AreEqual (0, cace.Initializers.Count, "Initializers");
			Assert.AreEqual (0, cace.Size, "Size");
			Assert.AreSame (ce, cace.SizeExpression, "SizeExpression");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Constructor2_Deny_Unrestricted ()
		{
			CodeExpression[] parameters = new CodeExpression[1] { ce };
			CodeArrayCreateExpression cace = new CodeArrayCreateExpression (ctr, parameters);
			Assert.AreEqual ("System.Void", cace.CreateType.BaseType, "CreateType.BaseType");
			Assert.AreEqual (1, cace.Initializers.Count, "Initializers");
			Assert.AreEqual (0, cace.Size, "Size");
			Assert.IsNull (cace.SizeExpression, "SizeExpression");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Constructor3_Deny_Unrestricted ()
		{
			CodeArrayCreateExpression cace = new CodeArrayCreateExpression (ctr, Int32.MinValue);
			Assert.AreEqual ("System.Void", cace.CreateType.BaseType, "CreateType.BaseType");
			Assert.AreEqual (0, cace.Initializers.Count, "Initializers");
			Assert.AreEqual (Int32.MinValue, cace.Size, "Size");
			Assert.IsNull (cace.SizeExpression, "SizeExpression");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Constructor4_Deny_Unrestricted ()
		{
			CodeArrayCreateExpression cace = new CodeArrayCreateExpression ("System.Void", ce);
			Assert.AreEqual ("System.Void", cace.CreateType.BaseType, "CreateType.BaseType");
			Assert.AreEqual (0, cace.Initializers.Count, "Initializers");
			Assert.AreEqual (0, cace.Size, "Size");
			Assert.AreSame (ce, cace.SizeExpression, "SizeExpression");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Constructor5_Deny_Unrestricted ()
		{
			CodeExpression[] parameters = new CodeExpression[1] { ce };
			CodeArrayCreateExpression cace = new CodeArrayCreateExpression ("System.Void", parameters);
			Assert.AreEqual ("System.Void", cace.CreateType.BaseType, "CreateType.BaseType");
			Assert.AreEqual (1, cace.Initializers.Count, "Initializers");
			Assert.AreEqual (0, cace.Size, "Size");
			Assert.IsNull (cace.SizeExpression, "SizeExpression");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Constructor6_Deny_Unrestricted ()
		{
			CodeArrayCreateExpression cace = new CodeArrayCreateExpression ("System.Void", Int32.MinValue);
			Assert.AreEqual ("System.Void", cace.CreateType.BaseType, "CreateType.BaseType");
			Assert.AreEqual (0, cace.Initializers.Count, "Initializers");
			Assert.AreEqual (Int32.MinValue, cace.Size, "Size");
			Assert.IsNull (cace.SizeExpression, "SizeExpression");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Constructor7_Deny_Unrestricted ()
		{
			CodeArrayCreateExpression cace = new CodeArrayCreateExpression (typeof(void), ce);
			Assert.AreEqual ("System.Void", cace.CreateType.BaseType, "CreateType.BaseType");
			Assert.AreEqual (0, cace.Initializers.Count, "Initializers");
			Assert.AreEqual (0, cace.Size, "Size");
			Assert.AreSame (ce, cace.SizeExpression, "SizeExpression");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Constructor8_Deny_Unrestricted ()
		{
			CodeExpression[] parameters = new CodeExpression[1] { ce };
			CodeArrayCreateExpression cace = new CodeArrayCreateExpression (typeof (void), parameters);
			Assert.AreEqual ("System.Void", cace.CreateType.BaseType, "CreateType.BaseType");
			Assert.AreEqual (1, cace.Initializers.Count, "Initializers");
			Assert.AreEqual (0, cace.Size, "Size");
			Assert.IsNull (cace.SizeExpression, "SizeExpression");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Constructor9_Deny_Unrestricted ()
		{
			CodeArrayCreateExpression cace = new CodeArrayCreateExpression (typeof (void), Int32.MinValue);
			Assert.AreEqual ("System.Void", cace.CreateType.BaseType, "CreateType.BaseType");
			Assert.AreEqual (0, cace.Initializers.Count, "Initializers");
			Assert.AreEqual (Int32.MinValue, cace.Size, "Size");
			Assert.IsNull (cace.SizeExpression, "SizeExpression");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void LinkDemand_Deny_Unrestricted ()
		{
			ConstructorInfo ci = typeof (CodeArrayCreateExpression).GetConstructor (new Type[0]);
			Assert.IsNotNull (ci, "default .ctor");
			Assert.IsNotNull (ci.Invoke (null), "invoke");
		}
	}
}
