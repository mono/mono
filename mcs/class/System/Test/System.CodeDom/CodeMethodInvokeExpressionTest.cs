//
// CodeMethodInvokeExpressionTest.cs
//	- Unit tests for System.CodeDom.CodeMethodInvokeExpression
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
	public class CodeMethodInvokeExpressionTest
	{
		[Test]
		public void Constructor0 ()
		{
			CodeMethodInvokeExpression cmie = new CodeMethodInvokeExpression ();
			Assert.IsNotNull (cmie.Method, "#1");
			Assert.AreEqual (string.Empty, cmie.Method.MethodName, "#2");
			Assert.IsNull (cmie.Method.TargetObject, "#3");
#if NET_2_0
			Assert.AreEqual (0, cmie.Method.TypeArguments.Count, "#4");
#endif
			Assert.IsNotNull (cmie.Parameters, "#5");
			Assert.AreEqual (0, cmie.Parameters.Count, "#6");

			CodeMethodReferenceExpression method = new CodeMethodReferenceExpression ();
			cmie.Method = method;
			Assert.IsNotNull (cmie.Method, "#7");
			Assert.AreSame (method, cmie.Method, "#8");

			cmie.Method = null;
			Assert.IsNotNull (cmie.Method, "#9");
			Assert.AreEqual (string.Empty, cmie.Method.MethodName, "#10");
			Assert.IsNull (cmie.Method.TargetObject, "#11");
#if NET_2_0
			Assert.AreEqual (0, cmie.Method.TypeArguments.Count, "#12");
#endif
		}

		[Test]
		public void Constructor1 ()
		{
			CodeMethodReferenceExpression method1 = new CodeMethodReferenceExpression ();
			CodeExpression param1 = new CodeExpression ();
			CodeExpression param2 = new CodeExpression ();

			CodeMethodInvokeExpression cmie = new CodeMethodInvokeExpression (
				method1, param1, param2);
			Assert.IsNotNull (cmie.Method, "#1");
			Assert.AreSame (method1, cmie.Method, "#2");

			cmie.Method = null;
			Assert.IsNotNull (cmie.Method, "#3");
			Assert.AreEqual (string.Empty, cmie.Method.MethodName, "#4");
			Assert.IsNull (cmie.Method.TargetObject, "#5");
#if NET_2_0
			Assert.AreEqual (0, cmie.Method.TypeArguments.Count, "#6");
#endif

			CodeMethodReferenceExpression method2 = new CodeMethodReferenceExpression ();
			cmie.Method = method2;
			Assert.IsNotNull (cmie.Method, "#7");
			Assert.AreSame (method2, cmie.Method, "#8");

			Assert.IsNotNull (cmie.Parameters, "#9");
			Assert.AreEqual (2, cmie.Parameters.Count, "#10");
			Assert.AreEqual (0, cmie.Parameters.IndexOf (param1), "#11");
			Assert.AreEqual (1, cmie.Parameters.IndexOf (param2), "#12");

			cmie = new CodeMethodInvokeExpression ((CodeMethodReferenceExpression) null, 
				param2);
			Assert.IsNotNull (cmie.Method, "#13");
			Assert.AreEqual (string.Empty, cmie.Method.MethodName, "#14");
			Assert.IsNull (cmie.Method.TargetObject, "#15");
#if NET_2_0
			Assert.AreEqual (0, cmie.Method.TypeArguments.Count, "#16");
#endif

			Assert.IsNotNull (cmie.Parameters, "#17");
			Assert.AreEqual (1, cmie.Parameters.Count, "#18");
			Assert.AreEqual (0, cmie.Parameters.IndexOf (param2), "#19");
		}

		[Test]
		[ExpectedException (typeof(ArgumentNullException))]
		public void Constructor1_NullParam ()
		{
			CodeMethodReferenceExpression method1 = new CodeMethodReferenceExpression ();
			CodeMethodInvokeExpression cmie = new CodeMethodInvokeExpression (
				method1, (CodeExpression) null);
		}

		[Test]
		public void Constructor2 ()
		{
			string methodName = "mono";
			CodeExpression targetObject = new CodeExpression ();
			CodeExpression param1 = new CodeExpression ();
			CodeExpression param2 = new CodeExpression ();

			CodeMethodInvokeExpression cmie = new CodeMethodInvokeExpression (
				targetObject, methodName, param1, param2);
			Assert.IsNotNull (cmie.Method, "#1");
			Assert.AreSame (methodName, cmie.Method.MethodName, "#2");
			Assert.AreSame (targetObject, cmie.Method.TargetObject, "#3");
			Assert.IsNotNull (cmie.Parameters, "#4");
			Assert.AreEqual (2, cmie.Parameters.Count, "#5");
			Assert.AreEqual (0, cmie.Parameters.IndexOf (param1), "#6");
			Assert.AreEqual (1, cmie.Parameters.IndexOf (param2), "#7");

			cmie = new CodeMethodInvokeExpression ((CodeExpression) null,
				(string) null, param2);
			Assert.IsNotNull (cmie.Method, "#8");
			Assert.AreEqual (string.Empty, cmie.Method.MethodName, "#9");
			Assert.IsNull (cmie.Method.TargetObject, "#10");
			Assert.AreEqual (1, cmie.Parameters.Count, "#11");
			Assert.AreEqual (0, cmie.Parameters.IndexOf (param2), "#12");
		}

		[Test]
		[ExpectedException (typeof(ArgumentNullException))]
		public void Constructor2_NullParam () {
			CodeMethodReferenceExpression method1 = new CodeMethodReferenceExpression ();
			CodeMethodInvokeExpression cmie = new CodeMethodInvokeExpression (
				new CodeExpression(), "mono", (CodeExpression) null);
		}
	}
}
