//
// CodeTypeOfExpressionTest.cs
//	- Unit tests for System.CodeDom.CodeTypeOfExpression
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
	public class CodeTypeOfExpressionTest
	{
		[Test]
		public void Constructor0 ()
		{
			CodeTypeOfExpression ctoe = new CodeTypeOfExpression ();
			Assert.IsNotNull (ctoe.Type, "#1");
			Assert.AreEqual (typeof (void).FullName, ctoe.Type.BaseType, "#2");

			CodeTypeReference type = new CodeTypeReference ("mono");
			ctoe.Type = type;
			Assert.IsNotNull (ctoe.Type, "#3");
			Assert.AreSame (type, ctoe.Type, "#4");

			ctoe.Type = null;
			Assert.IsNotNull (ctoe.Type, "#5");
			Assert.AreEqual (typeof (void).FullName, ctoe.Type.BaseType, "#6");
		}

		[Test]
		public void Constructor1 ()
		{
			CodeTypeReference type1 = new CodeTypeReference ("mono1");

			CodeTypeOfExpression ctoe = new CodeTypeOfExpression (type1);
			Assert.IsNotNull (ctoe.Type, "#1");
			Assert.AreSame (type1, ctoe.Type, "#2");

			ctoe.Type = null;
			Assert.IsNotNull (ctoe.Type, "#3");
			Assert.AreEqual (typeof (void).FullName, ctoe.Type.BaseType, "#4");

			CodeTypeReference type2 = new CodeTypeReference ("mono2");
			ctoe.Type = type2;
			Assert.IsNotNull (ctoe.Type, "#5");
			Assert.AreSame (type2, ctoe.Type, "#6");

			ctoe = new CodeTypeOfExpression ((CodeTypeReference) null);
			Assert.IsNotNull (ctoe.Type, "#7");
			Assert.AreEqual (typeof (void).FullName, ctoe.Type.BaseType, "#8");
		}

		[Test]
		public void Constructor2 ()
		{
			string baseType = "mono";

			CodeTypeOfExpression ctoe = new CodeTypeOfExpression (baseType);
			Assert.IsNotNull (ctoe.Type, "#1");
			Assert.AreEqual (baseType, ctoe.Type.BaseType, "#2");

			ctoe = new CodeTypeOfExpression ((string) null);
			Assert.IsNotNull (ctoe.Type, "#3");
			Assert.AreEqual (typeof (void).FullName, ctoe.Type.BaseType, "#4");
		}

		[Test]
		public void Constructor3 ()
		{
			Type type = typeof (int);

			CodeTypeOfExpression ctoe = new CodeTypeOfExpression (type);
			Assert.IsNotNull (ctoe.Type, "#1");
			Assert.AreEqual (type.FullName, ctoe.Type.BaseType, "#2");
		}

		[Test]
#if NET_2_0
		[ExpectedException (typeof (ArgumentNullException))]
#else
		[ExpectedException (typeof (NullReferenceException))]
#endif
		public void Constructor3_NullType ()
		{
			CodeTypeOfExpression ctoe = new CodeTypeOfExpression ((Type) null);
		}
	}
}
