//
// CodeParameterDeclarationExpressionTest.cs
//	- Unit tests for System.CodeDom.CodeParameterDeclarationExpression
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
	public class CodeParameterDeclarationExpressionTest
	{
		[Test]
		public void Constructor0 ()
		{
			CodeParameterDeclarationExpression cpde = new CodeParameterDeclarationExpression ();
			Assert.IsNotNull (cpde.CustomAttributes, "#1");
			Assert.AreEqual (0, cpde.CustomAttributes.Count, "#2");
			Assert.AreEqual (FieldDirection.In, cpde.Direction, "#3");
			Assert.IsNotNull (cpde.Name, "#4");
			Assert.AreEqual (string.Empty, cpde.Name, "#5");
			Assert.IsNotNull (cpde.Type, "#6");
			Assert.AreEqual (typeof (void).FullName, cpde.Type.BaseType, "#7");

			cpde.Direction = FieldDirection.Out;
			Assert.AreEqual (FieldDirection.Out, cpde.Direction, "#8");

			string name = "mono";
			cpde.Name = name;
			Assert.AreSame (name, cpde.Name, "#9");

			cpde.Name = null;
			Assert.IsNotNull (cpde.Name, "#10");
			Assert.AreEqual (string.Empty, cpde.Name, "#11");

			CodeTypeReference type = new CodeTypeReference ("mono");
			cpde.Type = type;
			Assert.AreSame (type, cpde.Type, "#12");

			cpde.Type = null;
			Assert.IsNotNull (cpde.Type, "#13");
			Assert.AreEqual (typeof (void).FullName, cpde.Type.BaseType, "#14");
		}

		[Test]
		public void Constructor1 ()
		{
			CodeTypeReference type = new CodeTypeReference ("mono");
			string name = "mono";

			CodeParameterDeclarationExpression cpde = new CodeParameterDeclarationExpression (
				type, name);
			Assert.IsNotNull (cpde.CustomAttributes, "#1");
			Assert.AreEqual (0, cpde.CustomAttributes.Count, "#2");
			Assert.AreEqual (FieldDirection.In, cpde.Direction, "#3");
			Assert.IsNotNull (cpde.Name, "#4");
			Assert.AreSame (name, cpde.Name, "#5");
			Assert.IsNotNull (cpde.Type, "#6");
			Assert.AreSame (type, cpde.Type, "#7");

			cpde = new CodeParameterDeclarationExpression ((CodeTypeReference) null,
				(string) null);
			Assert.IsNotNull (cpde.Name, "#8");
			Assert.AreEqual (string.Empty, cpde.Name, "#9");
			Assert.IsNotNull (cpde.Type, "#10");
			Assert.AreEqual (typeof (void).FullName, cpde.Type.BaseType, "#11");
		}

		[Test]
		public void Constructor2 ()
		{
			string baseType = "monotype";
			string name = "mono";

			CodeParameterDeclarationExpression cpde = new CodeParameterDeclarationExpression (
				baseType, name);
			Assert.IsNotNull (cpde.CustomAttributes, "#1");
			Assert.AreEqual (0, cpde.CustomAttributes.Count, "#2");
			Assert.AreEqual (FieldDirection.In, cpde.Direction, "#3");
			Assert.IsNotNull (cpde.Name, "#4");
			Assert.AreSame (name, cpde.Name, "#5");
			Assert.IsNotNull (cpde.Type, "#6");
			Assert.AreSame (baseType, cpde.Type.BaseType, "#7");

			cpde = new CodeParameterDeclarationExpression ((string) null,
				(string) null);
			Assert.IsNotNull (cpde.Name, "#8");
			Assert.AreEqual (string.Empty, cpde.Name, "#9");
			Assert.IsNotNull (cpde.Type, "#10");
			Assert.AreEqual (typeof (void).FullName, cpde.Type.BaseType, "#11");
		}

		[Test]
		public void Constructor3 ()
		{
			Type baseType = typeof (int);
			string name = "mono";

			CodeParameterDeclarationExpression cpde = new CodeParameterDeclarationExpression (
				baseType, name);
			Assert.IsNotNull (cpde.CustomAttributes, "#1");
			Assert.AreEqual (0, cpde.CustomAttributes.Count, "#2");
			Assert.AreEqual (FieldDirection.In, cpde.Direction, "#3");
			Assert.IsNotNull (cpde.Name, "#4");
			Assert.AreSame (name, cpde.Name, "#5");
			Assert.IsNotNull (cpde.Type, "#6");
			Assert.AreEqual (baseType.FullName, cpde.Type.BaseType, "#7");

			cpde = new CodeParameterDeclarationExpression (baseType,
				(string) null);
			Assert.IsNotNull (cpde.Name, "#8");
			Assert.AreEqual (string.Empty, cpde.Name, "#9");
			Assert.IsNotNull (cpde.Type, "#10");
			Assert.AreEqual (baseType.FullName, cpde.Type.BaseType, "#11");
		}

		[Test]
#if NET_2_0
		[ExpectedException (typeof (ArgumentNullException))]
#else
		[ExpectedException (typeof (NullReferenceException))]
#endif
		public void Constructor3_NullType ()
		{
			CodeParameterDeclarationExpression cpde = new CodeParameterDeclarationExpression (
				(Type) null, "mono");
		}
	}
}
