//
// CodeArrayCreateExpressionTest.cs
//	- Unit tests for System.CodeDom.CodeArrayCreateExpression
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
	public class CodeArrayCreateExpressionTest
	{
		[Test]
		public void DefaultConstructor ()
		{
			CodeArrayCreateExpression cace = new CodeArrayCreateExpression ();
			Assert.IsNotNull (cace.CreateType, "#1");
			Assert.AreEqual (typeof (void).FullName, cace.CreateType.BaseType, "#2");
		}

		[Test]
		public void NullCreateType ()
		{
			CodeArrayCreateExpression cace = new CodeArrayCreateExpression ((CodeTypeReference) null, 0);
			Assert.IsNotNull (cace.CreateType, "#1");
			Assert.AreEqual (typeof (void).FullName, cace.CreateType.BaseType, "#2");

			cace.CreateType = null;
			Assert.IsNotNull (cace.CreateType, "#3");
			Assert.AreEqual (typeof (void).FullName, cace.CreateType.BaseType, "#4");
		}
	}
}
