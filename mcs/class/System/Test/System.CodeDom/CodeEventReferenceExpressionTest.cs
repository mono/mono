//
// CodeEventReferenceExpressionTest.cs
//	- Unit tests for System.CodeDom.CodeEventReferenceExpression
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
	public class CodeEventReferenceExpressionTest
	{
		[Test]
		public void Constructor0 ()
		{
			CodeEventReferenceExpression cere = new CodeEventReferenceExpression ();
			Assert.IsNotNull (cere.EventName, "#1");
			Assert.AreEqual (string.Empty, cere.EventName, "#2");
			Assert.IsNull (cere.TargetObject, "#3");

			string eventName = "mono";
			cere.EventName = eventName;
			Assert.IsNotNull (cere.EventName, "#4");
			Assert.AreSame (eventName, cere.EventName, "#5");

			cere.EventName = null;
			Assert.IsNotNull (cere.EventName, "#6");
			Assert.AreEqual (string.Empty, cere.EventName, "#7");

			CodeExpression expression = new CodeExpression ();
			cere.TargetObject = expression;
			Assert.IsNotNull (cere.TargetObject, "#8");
			Assert.AreSame (expression, cere.TargetObject, "#9");

			cere.TargetObject = null;
			Assert.IsNull (cere.TargetObject, "#10");
		}

		[Test]
		public void Constructor1 ()
		{
			CodeSnippetExpression expression = new CodeSnippetExpression("exp");

			CodeEventReferenceExpression cere = new CodeEventReferenceExpression (
				expression, "mono");
			Assert.AreEqual ("mono", cere.EventName, "#1");
			Assert.IsNotNull (cere.TargetObject, "#2");
			Assert.AreSame (expression, cere.TargetObject, "#3");

			cere.EventName = null;
			Assert.IsNotNull (cere.EventName, "#4");
			Assert.AreEqual (string.Empty, cere.EventName, "#5");

			cere.TargetObject = null;
			Assert.IsNull (cere.TargetObject, "#6");

			cere = new CodeEventReferenceExpression ((CodeExpression) null,
				(string) null);
			Assert.IsNotNull (cere.EventName, "#7");
			Assert.AreEqual (string.Empty, cere.EventName, "#8");
			Assert.IsNull (cere.TargetObject, "#9");
		}
	}
}
