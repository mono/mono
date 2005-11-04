//
// CodeObjectCreateExpressionTest.cs
//	- Unit tests for System.CodeDom.CodeObjectCreateExpression
//
// Author:
//	Gert Driesen  <drieseng@users.sourceforge.net>
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

namespace MonoTests.System.CodeDom
{
	[TestFixture]
	public class CodeObjectCreateExpressionTest
	{
		[Test]
		public void Constructor0 ()
		{
			CodeObjectCreateExpression coce = new CodeObjectCreateExpression ();

			Assert.IsNotNull (coce.CreateType, "#1");
			Assert.AreEqual (typeof (void).FullName, coce.CreateType.BaseType, "#2");
			Assert.IsNotNull (coce.Parameters, "#3");
			Assert.AreEqual (0, coce.Parameters.Count, "#4");

			CodeTypeReference type = new CodeTypeReference ("mono");
			coce.CreateType = type;
			Assert.IsNotNull (coce.CreateType, "#8");
			Assert.AreSame (type, coce.CreateType, "#9");

			coce.CreateType = null;
			Assert.IsNotNull (coce.CreateType, "#10");
			Assert.AreEqual (typeof (void).FullName, coce.CreateType.BaseType, "#11");
		}

		[Test]
		public void Constructor1 ()
		{
			CodeTypeReference type = new CodeTypeReference ("mono");
			CodeExpression expression1 = new CodeExpression ();
			CodeExpression expression2 = new CodeExpression ();

			CodeObjectCreateExpression coce = new CodeObjectCreateExpression (
				type, expression1, expression2);
			Assert.IsNotNull (coce.CreateType, "#1");
			Assert.AreSame (type, coce.CreateType, "#2");
			Assert.IsNotNull (coce.Parameters, "#3");
			Assert.AreEqual (2, coce.Parameters.Count, "#4");
			Assert.AreEqual (0, coce.Parameters.IndexOf(expression1), "#5");
			Assert.AreEqual (1, coce.Parameters.IndexOf(expression2), "#6");

			coce = new CodeObjectCreateExpression ((CodeTypeReference) null, 
				expression1);
			Assert.IsNotNull (coce.CreateType, "#7");
			Assert.AreEqual (typeof (void).FullName, coce.CreateType.BaseType, "#8");
			Assert.IsNotNull (coce.Parameters, "#9");
			Assert.AreEqual (1, coce.Parameters.Count, "#10");
			Assert.AreEqual (0, coce.Parameters.IndexOf(expression1), "#11");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor1_NullParameter ()
		{
			CodeTypeReference type = new CodeTypeReference ("mono");
			CodeExpression expression1 = new CodeExpression ();

			CodeObjectCreateExpression coce = new CodeObjectCreateExpression (
				type, expression1, (CodeExpression) null);
		}

		[Test]
		public void Constructor2 ()
		{
			string baseType = "mono";
			CodeExpression expression1 = new CodeExpression ();
			CodeExpression expression2 = new CodeExpression ();

			CodeObjectCreateExpression coce = new CodeObjectCreateExpression (
				baseType, expression1, expression2);
			Assert.IsNotNull (coce.CreateType, "#1");
			Assert.AreSame (baseType, coce.CreateType.BaseType, "#2");
			Assert.IsNotNull (coce.Parameters, "#3");
			Assert.AreEqual (2, coce.Parameters.Count, "#4");
			Assert.AreEqual (0, coce.Parameters.IndexOf(expression1), "#5");
			Assert.AreEqual (1, coce.Parameters.IndexOf(expression2), "#6");

			coce = new CodeObjectCreateExpression ((string) null, expression2);
			Assert.IsNotNull (coce.CreateType, "#7");
			Assert.AreEqual (typeof (void).FullName, coce.CreateType.BaseType, "#8");
			Assert.IsNotNull (coce.Parameters, "#9");
			Assert.AreEqual (1, coce.Parameters.Count, "#10");
			Assert.AreEqual (0, coce.Parameters.IndexOf(expression2), "#11");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor2_NullParameter ()
		{
			string baseType = "mono";
			CodeExpression expression1 = new CodeExpression ();

			CodeObjectCreateExpression coce = new CodeObjectCreateExpression (
				baseType, expression1, (CodeExpression) null);
		}

		[Test]
		public void Constructor3 ()
		{
			Type baseType = typeof (int);
			CodeExpression expression1 = new CodeExpression ();
			CodeExpression expression2 = new CodeExpression ();
			CodeExpression expression3 = new CodeExpression ();

			CodeObjectCreateExpression coce = new CodeObjectCreateExpression (
				baseType, expression1, expression2, expression3);
			Assert.IsNotNull (coce.CreateType, "#1");
			Assert.AreEqual (baseType.FullName, coce.CreateType.BaseType, "#2");
			Assert.IsNotNull (coce.Parameters, "#3");
			Assert.AreEqual (3, coce.Parameters.Count, "#4");
			Assert.AreEqual (0, coce.Parameters.IndexOf(expression1), "#5");
			Assert.AreEqual (1, coce.Parameters.IndexOf(expression2), "#6");
			Assert.AreEqual (2, coce.Parameters.IndexOf(expression3), "#7");
		}

		[Test]
#if NET_2_0
		[ExpectedException (typeof (ArgumentNullException))]
#else
		[ExpectedException (typeof (NullReferenceException))]
#endif
		public void Constructor3_NullType ()
		{
			CodeObjectCreateExpression coce = new CodeObjectCreateExpression (
				(Type) null, new CodeExpression ());
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor3_NullParameter ()
		{
			Type baseType = typeof (int);
			CodeExpression expression1 = new CodeExpression ();

			CodeObjectCreateExpression coce = new CodeObjectCreateExpression (
				baseType, expression1, (CodeExpression) null);
		}
	}
}
