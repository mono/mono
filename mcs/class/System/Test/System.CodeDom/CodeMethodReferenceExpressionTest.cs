//
// CodeMethodReferenceExpressionTest.cs
//	- Unit tests for System.CodeDom.CodeMethodReferenceExpression
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
	public class CodeMethodReferenceExpressionTest
	{
		[Test]
		public void Constructor0 ()
		{
			CodeMethodReferenceExpression cmre = new CodeMethodReferenceExpression ();

			Assert.IsNotNull (cmre.MethodName, "#1");
			Assert.AreEqual (string.Empty, cmre.MethodName, "#2");
			Assert.IsNull (cmre.TargetObject, "#3");

#if NET_2_0
			Assert.IsNotNull (cmre.TypeArguments, "#4");
			Assert.AreEqual (0, cmre.TypeArguments.Count, "#5");
#endif

			string methodName = "mono";
			cmre.MethodName = methodName;
			Assert.IsNotNull (cmre.MethodName, "#6");
			Assert.AreSame (methodName, cmre.MethodName, "#7");

			cmre.MethodName = null;
			Assert.IsNotNull (cmre.MethodName, "#8");
			Assert.AreEqual (string.Empty, cmre.MethodName, "#9");

			CodeExpression expression = new CodeExpression ();
			cmre.TargetObject = expression;
			Assert.IsNotNull (cmre.TargetObject, "#10");
			Assert.AreSame (expression, cmre.TargetObject, "#11");

			cmre.TargetObject = null;
			Assert.IsNull (cmre.TargetObject, "#12");
		}

		[Test]
		public void Constructor1 ()
		{
			CodeExpression expression = new CodeExpression ();
			string methodName = "mono";

			CodeMethodReferenceExpression cmre = new CodeMethodReferenceExpression (
				expression, methodName);
			Assert.IsNotNull (cmre.MethodName, "#1");
			Assert.AreSame (methodName, cmre.MethodName, "#2");
			Assert.IsNotNull (cmre.TargetObject, "#3");
			Assert.AreSame (expression, cmre.TargetObject, "#4");

#if NET_2_0
			Assert.IsNotNull (cmre.TypeArguments, "#5");
			Assert.AreEqual (0, cmre.TypeArguments.Count, "#6");
#endif

			cmre = new CodeMethodReferenceExpression ((CodeExpression) null,
				(string) null);
			Assert.IsNotNull (cmre.MethodName, "#7");
			Assert.AreEqual (string.Empty, cmre.MethodName, "#8");
			Assert.IsNull (cmre.TargetObject, "#9");
		}

#if NET_2_0
		[Test]
		public void Constructor2 ()
		{
			CodeExpression expression = new CodeExpression ();
			string methodName = "mono";
			CodeTypeReference arg1 = new CodeTypeReference ("arg1");
			CodeTypeReference arg2 = new CodeTypeReference ("arg2");

			CodeMethodReferenceExpression cmre = new CodeMethodReferenceExpression (
				expression, methodName, arg1, arg2);

			Assert.IsNotNull (cmre.MethodName, "#1");
			Assert.AreSame (methodName, cmre.MethodName, "#2");
			Assert.IsNotNull (cmre.TargetObject, "#3");
			Assert.AreSame (expression, cmre.TargetObject, "#4");
			Assert.IsNotNull (cmre.TypeArguments, "#5");
			Assert.AreEqual (2, cmre.TypeArguments.Count, "#6");
			Assert.AreSame (arg1, cmre.TypeArguments[0], "#7");
			Assert.AreSame (arg2, cmre.TypeArguments[1], "#8");

			cmre = new CodeMethodReferenceExpression ((CodeExpression) null,
				(string) null, arg2);
			Assert.IsNotNull (cmre.MethodName, "#9");
			Assert.AreEqual (string.Empty, cmre.MethodName, "#10");
			Assert.IsNull (cmre.TargetObject, "#11");
			Assert.IsNotNull (cmre.TypeArguments, "#12");
			Assert.AreEqual (1, cmre.TypeArguments.Count, "#13");
			Assert.AreSame (arg2, cmre.TypeArguments[0], "#14");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor2_NullArg ()
		{
			CodeExpression expression = new CodeExpression ();
			string methodName = "mono";

			CodeMethodReferenceExpression cmre = new CodeMethodReferenceExpression (
				expression, methodName, (CodeTypeReference) null);
		}
#endif
	}
}
