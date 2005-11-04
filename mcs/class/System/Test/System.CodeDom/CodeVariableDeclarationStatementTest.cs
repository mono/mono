//
// CodeVariableDeclarationStatementTest.cs
//	- Unit tests for System.CodeDom.CodeVariableDeclarationStatement
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
	public class CodeVariableDeclarationStatementTest
	{
		[Test]
		public void Constructor0 ()
		{
			CodeVariableDeclarationStatement cvds = new CodeVariableDeclarationStatement ();
			Assert.IsNull (cvds.InitExpression, "#1");
			Assert.IsNotNull (cvds.Name, "#2");
			Assert.AreEqual (string.Empty, cvds.Name, "#3");
			Assert.IsNotNull (cvds.Type, "#4");
			Assert.AreEqual (typeof (void).FullName, cvds.Type.BaseType, "#5");

			string name = "mono";
			cvds.Name = name;
			Assert.AreSame (name, cvds.Name, "#6");

			cvds.Name = null;
			Assert.IsNotNull (cvds.Name, "#7");
			Assert.AreEqual (string.Empty, cvds.Name, "#8");

			CodeExpression expression = new CodeExpression ();
			cvds.InitExpression = expression;
			Assert.AreSame (expression, cvds.InitExpression, "#9");

			CodeTypeReference type = new CodeTypeReference ("mono");
			cvds.Type = type;
			Assert.AreSame (type, cvds.Type, "#10");

			cvds.Type = null;
			Assert.IsNotNull (cvds.Type, "#11");
			Assert.AreEqual (typeof (void).FullName, cvds.Type.BaseType, "#12");

#if NET_2_0
			Assert.IsNotNull (cvds.StartDirectives, "#13");
			Assert.AreEqual (0, cvds.StartDirectives.Count, "#14");

			Assert.IsNotNull (cvds.EndDirectives, "#15");
			Assert.AreEqual (0, cvds.EndDirectives.Count, "#16");
#endif

			Assert.IsNull (cvds.LinePragma, "#17");

			CodeLinePragma clp = new CodeLinePragma ("mono", 10);
			cvds.LinePragma = clp;
			Assert.IsNotNull (cvds.LinePragma, "#18");
			Assert.AreSame (clp, cvds.LinePragma, "#19");

			cvds.LinePragma = null;
			Assert.IsNull (cvds.LinePragma, "#20");
		}

		[Test]
		public void Constructor1 ()
		{
			CodeTypeReference type = new CodeTypeReference ("mono");
			string name = "mono";

			CodeVariableDeclarationStatement cvds = new CodeVariableDeclarationStatement (
				type, name);
			Assert.IsNull (cvds.InitExpression, "#1");
			Assert.IsNotNull (cvds.Name, "#2");
			Assert.AreSame (name, cvds.Name, "#3");
			Assert.IsNotNull (cvds.Type, "#4");
			Assert.AreSame (type, cvds.Type, "#5");

			cvds = new CodeVariableDeclarationStatement ((CodeTypeReference) null,
				(string) null);
			Assert.IsNotNull (cvds.Name, "#6");
			Assert.AreEqual (string.Empty, cvds.Name, "#7");
			Assert.IsNotNull (cvds.Type, "#8");
			Assert.AreEqual (typeof (void).FullName, cvds.Type.BaseType, "#9");
		}

		[Test]
		public void Constructor2 ()
		{
			string baseType = "monotype";
			string name = "mono";

			CodeVariableDeclarationStatement cvds = new CodeVariableDeclarationStatement (
				baseType, name);
			Assert.IsNull (cvds.InitExpression, "#1");
			Assert.IsNotNull (cvds.Name, "#2");
			Assert.AreSame (name, cvds.Name, "#3");
			Assert.IsNotNull (cvds.Type, "#4");
			Assert.AreSame (baseType, cvds.Type.BaseType, "#5");

			cvds = new CodeVariableDeclarationStatement ((string) null, 
				(string) null);
			Assert.IsNotNull (cvds.Name, "#6");
			Assert.AreEqual (string.Empty, cvds.Name, "#7");
			Assert.IsNotNull (cvds.Type, "#8");
			Assert.AreEqual (typeof (void).FullName, cvds.Type.BaseType, "#9");
		}

		[Test]
		public void Constructor3 ()
		{
			Type baseType = typeof (int);
			string name = "mono";

			CodeVariableDeclarationStatement cvds = new CodeVariableDeclarationStatement (
				baseType, name);
			Assert.IsNull (cvds.InitExpression, "#1");
			Assert.IsNotNull (cvds.Name, "#2");
			Assert.AreSame (name, cvds.Name, "#3");
			Assert.IsNotNull (cvds.Type, "#4");
			Assert.AreEqual (baseType.FullName, cvds.Type.BaseType, "#5");

			cvds = new CodeVariableDeclarationStatement (baseType, 
				(string) null);
			Assert.IsNotNull (cvds.Name, "#6");
			Assert.AreEqual (string.Empty, cvds.Name, "#7");
			Assert.IsNotNull (cvds.Type, "#8");
			Assert.AreEqual (baseType.FullName, cvds.Type.BaseType, "#9");
		}

		[Test]
#if NET_2_0
		[ExpectedException (typeof (ArgumentNullException))]
#else
		[ExpectedException (typeof (NullReferenceException))]
#endif
		public void Constructor3_NullType ()
		{
			CodeVariableDeclarationStatement cvds = new CodeVariableDeclarationStatement (
				(Type) null, "mono");
		}

		[Test]
		public void Constructor4 ()
		{
			CodeTypeReference type = new CodeTypeReference ("mono");
			string name = "mono";
			CodeExpression expression = new CodeExpression ();

			CodeVariableDeclarationStatement cvds = new CodeVariableDeclarationStatement (
				type, name, expression);
			Assert.IsNotNull (cvds.InitExpression, "#1");
			Assert.AreSame (expression, cvds.InitExpression, "#2");
			Assert.IsNotNull (cvds.Name, "#3");
			Assert.AreSame (name, cvds.Name, "#4");
			Assert.IsNotNull (cvds.Type, "#5");
			Assert.AreSame (type, cvds.Type, "#6");

			cvds = new CodeVariableDeclarationStatement ((CodeTypeReference) null, 
				(string) null, (CodeExpression) null);
			Assert.IsNull (cvds.InitExpression, "#7");
			Assert.IsNotNull (cvds.Name, "#8");
			Assert.AreEqual (string.Empty, cvds.Name, "#9");
			Assert.IsNotNull (cvds.Type, "#10");
			Assert.AreEqual (typeof (void).FullName, cvds.Type.BaseType, "#11");
		}

		[Test]
		public void Constructor5 ()
		{
			string baseType = "monotype";
			string name = "mono";
			CodeExpression expression = new CodeExpression ();

			CodeVariableDeclarationStatement cvds = new CodeVariableDeclarationStatement (
				baseType, name, expression);
			Assert.IsNotNull (cvds.InitExpression, "#1");
			Assert.AreSame (expression, cvds.InitExpression, "#2");
			Assert.IsNotNull (cvds.Name, "#3");
			Assert.AreSame (name, cvds.Name, "#4");
			Assert.IsNotNull (cvds.Type, "#5");
			Assert.AreEqual (baseType, cvds.Type.BaseType, "#6");

			cvds = new CodeVariableDeclarationStatement ((string) null, 
				(string) null, (CodeExpression) null);
			Assert.IsNull (cvds.InitExpression, "#7");
			Assert.IsNotNull (cvds.Name, "#8");
			Assert.AreEqual (string.Empty, cvds.Name, "#9");
			Assert.IsNotNull (cvds.Type, "#10");
			Assert.AreEqual (typeof (void).FullName, cvds.Type.BaseType, "#11");
		}

		[Test]
		public void Constructor6 ()
		{
			Type baseType = typeof (int);
			string name = "mono";
			CodeExpression expression = new CodeExpression ();

			CodeVariableDeclarationStatement cvds = new CodeVariableDeclarationStatement (
				baseType, name, expression);
			Assert.IsNotNull (cvds.InitExpression, "#1");
			Assert.AreSame (expression, cvds.InitExpression, "#2");
			Assert.IsNotNull (cvds.Name, "#3");
			Assert.AreSame (name, cvds.Name, "#4");
			Assert.IsNotNull (cvds.Type, "#5");
			Assert.AreEqual (baseType.FullName, cvds.Type.BaseType, "#6");

			cvds = new CodeVariableDeclarationStatement (baseType, 
				(string) null, (CodeExpression) null);
			Assert.IsNull (cvds.InitExpression, "#7");
			Assert.IsNotNull (cvds.Name, "#8");
			Assert.AreEqual (string.Empty, cvds.Name, "#9");
			Assert.IsNotNull (cvds.Type, "#10");
			Assert.AreEqual (baseType.FullName, cvds.Type.BaseType, "#11");
		}

		[Test]
#if NET_2_0
		[ExpectedException (typeof (ArgumentNullException))]
#else
		[ExpectedException (typeof (NullReferenceException))]
#endif
		public void Constructor6_NullType ()
		{
			CodeVariableDeclarationStatement cvds = new CodeVariableDeclarationStatement (
				(Type) null, "mono", new CodeExpression ());
		}
	}
}
