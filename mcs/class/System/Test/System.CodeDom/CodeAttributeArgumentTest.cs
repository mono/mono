//
// CodeAttributeArgumentTest.cs
//	- Unit tests for System.CodeDom.CodeAttributeArgument
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
	public class CodeAttributeArgumentTest
	{
		[Test]
		public void DefaultConstructor ()
		{
			CodeAttributeArgument caa = new CodeAttributeArgument ();
			Assert.IsNotNull (caa.Name, "#1");
			Assert.AreEqual (string.Empty, caa.Name, "#2");
			Assert.IsNull (caa.Value, "#3");
		}

		[Test]
		public void NullName ()
		{
			CodeAttributeArgument caa = new CodeAttributeArgument ((string) null, (CodeExpression) null);
			Assert.IsNotNull (caa.Name, "#1");
			Assert.AreEqual (string.Empty, caa.Name, "#2");
			Assert.IsNull (caa.Value, "#3");

			caa.Name = null;
			Assert.IsNotNull (caa.Name, "#4");
			Assert.AreEqual (string.Empty, caa.Name, "#5");
		}
	}
}
