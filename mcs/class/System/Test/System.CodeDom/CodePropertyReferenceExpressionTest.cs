//
// CodePropertyReferenceExpressionTest.cs
//	- Unit tests for System.CodeDom.CodePropertyReferenceExpression
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
	public class CodePropertyReferenceExpressionTest
	{
		[Test]
		public void Constructor0 ()
		{
			CodePropertyReferenceExpression cpre = new CodePropertyReferenceExpression ();

			Assert.IsNotNull (cpre.PropertyName, "#1");
			Assert.AreEqual (string.Empty, cpre.PropertyName, "#2");
			Assert.IsNull (cpre.TargetObject, "#3");

			string propertyName = "mono";
			cpre.PropertyName = propertyName;
			Assert.IsNotNull (cpre.PropertyName, "#4");
			Assert.AreSame (propertyName, cpre.PropertyName, "#5");

			cpre.PropertyName = null;
			Assert.IsNotNull (cpre.PropertyName, "#6");
			Assert.AreEqual (string.Empty, cpre.PropertyName, "#7");

			CodeExpression expression = new CodeExpression ();
			cpre.TargetObject = expression;
			Assert.IsNotNull (cpre.TargetObject, "#8");
			Assert.AreSame (expression, cpre.TargetObject, "#9");

			cpre.TargetObject = null;
			Assert.IsNull (cpre.TargetObject, "#10");
		}

		[Test]
		public void Constructor1 ()
		{
			CodeExpression expression = new CodeExpression ();
			string propertyName = "mono";

			CodePropertyReferenceExpression cpre = new CodePropertyReferenceExpression (
				expression, propertyName);
			Assert.IsNotNull (cpre.PropertyName, "#1");
			Assert.AreSame (propertyName, cpre.PropertyName, "#2");
			Assert.IsNotNull (cpre.TargetObject, "#3");
			Assert.AreSame (expression, cpre.TargetObject, "#4");

			cpre = new CodePropertyReferenceExpression ((CodeExpression) null,
				(string) null);
			Assert.IsNotNull (cpre.PropertyName, "#5");
			Assert.AreEqual (string.Empty, cpre.PropertyName, "#6");
			Assert.IsNull (cpre.TargetObject, "#7");
		}
	}
}
