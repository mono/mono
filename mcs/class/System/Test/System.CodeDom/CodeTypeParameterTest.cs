//
// CodeTypeParameterTest.cs
//	- Unit tests for System.CodeDom.CodeTypeParameter
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

#if NET_2_0

using NUnit.Framework;

using System;
using System.CodeDom;

namespace MonoTests.System.CodeDom
{
	[TestFixture]
	public class CodeTypeParameterTest
	{
		[Test]
		public void Constructor0 ()
		{
			CodeTypeParameter ctp = new CodeTypeParameter ();

			Assert.IsNotNull (ctp.Constraints, "#1");
			Assert.AreEqual (0, ctp.Constraints.Count, "#2");
			Assert.IsNotNull (ctp.CustomAttributes, "#3");
			Assert.AreEqual (0, ctp.CustomAttributes.Count, "#4");
			Assert.IsFalse (ctp.HasConstructorConstraint, "#5");
			Assert.IsNotNull (ctp.Name, "#6");
			Assert.AreEqual (string.Empty, ctp.Name, "#7");

			ctp.Name = null;
			Assert.IsNotNull (ctp.Name, "#8");
			Assert.AreEqual (string.Empty, ctp.Name, "#9");

			string name = "mono";
			ctp.Name = name;
			Assert.AreSame (name, ctp.Name, "#10");

			ctp.HasConstructorConstraint = true;
			Assert.IsTrue (ctp.HasConstructorConstraint, "#11");
		}

		[Test]
		public void Constructor1 () {
			string parameterName = "mono";

			CodeTypeParameter ctp = new CodeTypeParameter (parameterName);

			Assert.IsNotNull (ctp.Constraints, "#1");
			Assert.AreEqual (0, ctp.Constraints.Count, "#2");
			Assert.IsNotNull (ctp.CustomAttributes, "#3");
			Assert.AreEqual (0, ctp.CustomAttributes.Count, "#4");
			Assert.IsFalse (ctp.HasConstructorConstraint, "#5");
			Assert.IsNotNull (ctp.Name, "#6");
			Assert.AreSame (parameterName, ctp.Name, "#7");

			ctp.Name = null;
			Assert.IsNotNull (ctp.Name, "#8");
			Assert.AreEqual (string.Empty, ctp.Name, "#9");

			ctp = new CodeTypeParameter ((string) null);
			Assert.IsNotNull (ctp.Name, "#10");
			Assert.AreEqual (string.Empty, ctp.Name, "#11");
		}
	}
}

#endif
