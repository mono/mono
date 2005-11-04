//
// CodeNamespaceTest.cs
//	- Unit tests for System.CodeDom.CodeNamespace
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
using System.Collections.Specialized;

namespace MonoTests.System.CodeDom
{
	[TestFixture]
	public class CodeNamespaceTest
	{
		[Test]
		public void Constructor0 ()
		{
			CodeNamespace cn = new CodeNamespace ();
			Assert.IsNotNull (cn.Comments, "#1");
			Assert.AreEqual (0, cn.Comments.Count, "#2");

			Assert.IsNotNull (cn.Imports, "#3");
			Assert.AreEqual (0, cn.Imports.Count, "#4");

			Assert.IsNotNull (cn.Name, "#5");
			Assert.AreEqual (string.Empty, cn.Name, "#6");

			Assert.IsNotNull (cn.Types, "#7");
			Assert.AreEqual (0, cn.Types.Count, "#8");

			Assert.IsNotNull (cn.UserData, "#9");
			Assert.AreEqual (typeof(ListDictionary), cn.UserData.GetType (), "#10");
			Assert.AreEqual (0, cn.UserData.Count, "#11");
			
			cn.Name = null;
			Assert.IsNotNull (cn.Name, "#12");
			Assert.AreEqual (string.Empty, cn.Name, "#13");
		}

		[Test]
		public void Constructor1 ()
		{
			string ns = "mono";

			CodeNamespace cn = new CodeNamespace (ns);
			Assert.IsNotNull (cn.Comments, "#1");
			Assert.AreEqual (0, cn.Comments.Count, "#2");

			Assert.IsNotNull (cn.Imports, "#3");
			Assert.AreEqual (0, cn.Imports.Count, "#4");

			Assert.IsNotNull (cn.Name, "#5");
			Assert.AreEqual (ns, cn.Name, "#6");
			Assert.AreSame (ns, cn.Name, "#7");

			Assert.IsNotNull (cn.Types, "#8");
			Assert.AreEqual (0, cn.Types.Count, "#9");

			Assert.IsNotNull (cn.UserData, "#10");
			Assert.AreEqual (typeof(ListDictionary), cn.UserData.GetType (), "#11");
			Assert.AreEqual (0, cn.UserData.Count, "#12");
			
			cn.Name = null;
			Assert.IsNotNull (cn.Name, "#13");
			Assert.AreEqual (string.Empty, cn.Name, "#14");

			cn = new CodeNamespace ((string) null);
			Assert.IsNotNull (cn.Name, "#15");
			Assert.AreEqual (string.Empty, cn.Name, "#16");
		}
	}
}
