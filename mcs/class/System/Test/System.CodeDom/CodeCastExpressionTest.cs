//
// CodeCastExpressionTest.cs
//	- Unit tests for System.CodeDom.CodeCastExpression
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
	public class CodeCastExpressionTest
	{
		[Test]
		public void Constructor0 ()
		{
			CodeCastExpression cce = new CodeCastExpression ();
			Assert.IsNull (cce.Expression, "#1");
			Assert.IsNotNull (cce.TargetType, "#2");
			Assert.AreEqual (typeof (void).FullName, cce.TargetType.BaseType, "#3");

			CodeExpression expression = new CodeExpression ();
			cce.Expression = expression;
			Assert.IsNotNull (cce.Expression, "#4");
			Assert.AreSame (expression, cce.Expression, "#5");

			cce.Expression = null;
			Assert.IsNull (cce.Expression, "#6");

			CodeTypeReference type = new CodeTypeReference ("mono");
			cce.TargetType = type;
			Assert.IsNotNull (cce.TargetType, "#7");
			Assert.AreSame (type, cce.TargetType, "#8");

			cce.TargetType = null;
			Assert.IsNotNull (cce.TargetType, "#9");
			Assert.AreEqual (typeof (void).FullName, cce.TargetType.BaseType, "#10");
		}

		[Test]
		public void Constructor1 ()
		{
			CodeTypeReference type1 = new CodeTypeReference ("mono1");
			CodeExpression expression1 = new CodeExpression ();

			CodeCastExpression cce = new CodeCastExpression (type1, expression1);
			Assert.IsNotNull (cce.Expression, "#1");
			Assert.AreSame (expression1, cce.Expression, "#2");
			Assert.IsNotNull (cce.TargetType, "#3");
			Assert.AreSame (type1, cce.TargetType, "#4");

			cce.Expression = null;
			Assert.IsNull (cce.Expression, "#5");

			CodeExpression expression2 = new CodeExpression ();
			cce.Expression = expression2;
			Assert.IsNotNull (cce.Expression, "#6");
			Assert.AreSame (expression2, cce.Expression, "#7");

			cce.TargetType = null;
			Assert.IsNotNull (cce.TargetType, "#8");
			Assert.AreEqual (typeof (void).FullName, cce.TargetType.BaseType, "#9");

			CodeTypeReference type2 = new CodeTypeReference ("mono2");
			cce.TargetType = type2;
			Assert.IsNotNull (cce.TargetType, "#10");
			Assert.AreSame (type2, cce.TargetType, "#11");

			cce = new CodeCastExpression ((CodeTypeReference) null, (CodeExpression) null);
			Assert.IsNull (cce.Expression, "#12");
			Assert.IsNotNull (cce.TargetType, "#13");
			Assert.AreEqual (typeof (void).FullName, cce.TargetType.BaseType, "#14");
		}

		[Test]
		public void Constructor2 ()
		{
			string baseType = "mono";
			CodeExpression expression = new CodeExpression ();

			CodeCastExpression cce = new CodeCastExpression (baseType, expression);
			Assert.IsNotNull (cce.Expression, "#1");
			Assert.AreSame (expression, cce.Expression, "#2");
			Assert.IsNotNull (cce.TargetType, "#3");
			Assert.AreEqual (baseType, cce.TargetType.BaseType, "#4");

			cce = new CodeCastExpression ((string) null, expression);
			Assert.IsNotNull (cce.Expression, "#5");
			Assert.AreSame (expression, cce.Expression, "#6");
			Assert.IsNotNull (cce.TargetType, "#7");
			Assert.AreEqual (typeof (void).FullName, cce.TargetType.BaseType, "#8");
		}

		[Test]
		public void Constructor3 ()
		{
			Type type = typeof (int);
			CodeExpression expression = new CodeExpression ();

			CodeCastExpression cce = new CodeCastExpression (type, expression);
			Assert.IsNotNull (cce.Expression, "#1");
			Assert.AreSame (expression, cce.Expression, "#2");
			Assert.IsNotNull (cce.TargetType, "#3");
			Assert.AreEqual (type.FullName, cce.TargetType.BaseType, "#4");

			cce = new CodeCastExpression (type, (CodeExpression) null);
			Assert.IsNull (cce.Expression, "#5");
			Assert.IsNotNull (cce.TargetType, "#6");
			Assert.AreEqual (type.FullName, cce.TargetType.BaseType, "#7");
		}

		[Test]
#if NET_2_0
		[ExpectedException (typeof (ArgumentNullException))]
#else
		[ExpectedException (typeof (NullReferenceException))]
#endif
		public void Constructor3_NullType ()
		{
			CodeCastExpression cce = new CodeCastExpression ((Type) null,
				new CodeExpression ());
		}
	}
}
