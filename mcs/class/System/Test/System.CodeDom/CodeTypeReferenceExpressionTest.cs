//
// CodeTypeReferenceExpressionTest.cs
//	- Unit tests for System.CodeDom.CodeTypeReferenceExpression
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
	public class CodeTypeReferenceExpressionTest
	{
#if NET_2_0
		[Test]
		public void Constructor0 ()
		{
			CodeTypeReferenceExpression ctre = new CodeTypeReferenceExpression ();
			Assert.IsNotNull (ctre.Type, "#1");
			Assert.AreEqual (typeof (void).FullName, ctre.Type.BaseType, "#2");
		}
#endif

		[Test]
		public void Constructor1 ()
		{
			CodeTypeReference type1 = new CodeTypeReference ("mono1");

			CodeTypeReferenceExpression ctre = new CodeTypeReferenceExpression (type1);
			Assert.IsNotNull (ctre.Type, "#1");
			Assert.AreSame (type1, ctre.Type, "#2");

			ctre.Type = null;
			Assert.IsNotNull (ctre.Type, "#3");
			Assert.AreEqual (typeof (void).FullName, ctre.Type.BaseType, "#4");

			CodeTypeReference type2 = new CodeTypeReference ("mono2");
			ctre.Type = type2;
			Assert.IsNotNull (ctre.Type, "#5");
			Assert.AreSame (type2, ctre.Type, "#6");

			ctre = new CodeTypeReferenceExpression ((CodeTypeReference) null);
			Assert.IsNotNull (ctre.Type, "#7");
			Assert.AreEqual (typeof (void).FullName, ctre.Type.BaseType, "#8");
		}

		[Test]
		public void Constructor2 ()
		{
			string baseType = "mono";

			CodeTypeReferenceExpression ctre = new CodeTypeReferenceExpression (baseType);
			Assert.IsNotNull (ctre.Type, "#1");
			Assert.AreEqual (baseType, ctre.Type.BaseType, "#2");

			ctre = new CodeTypeReferenceExpression ((string) null);
			Assert.IsNotNull (ctre.Type, "#3");
			Assert.AreEqual (typeof (void).FullName, ctre.Type.BaseType, "#4");
		}

		[Test]
		public void Constructor3 ()
		{
			Type type = typeof (int);

			CodeTypeReferenceExpression ctre = new CodeTypeReferenceExpression (type);
			Assert.IsNotNull (ctre.Type, "#1");
			Assert.AreEqual (type.FullName, ctre.Type.BaseType, "#2");
		}

		[Test]
#if NET_2_0
		[ExpectedException (typeof (ArgumentNullException))]
#else
		[ExpectedException (typeof (NullReferenceException))]
#endif
		public void Constructor3_NullType ()
		{
			CodeTypeReferenceExpression ctre = new CodeTypeReferenceExpression ((Type) null);
		}
	}
}
