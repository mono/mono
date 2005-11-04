//
// CodeNamespaceImportTest.cs
//	- Unit tests for System.CodeDom.CodeNamespaceImport
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
	public class CodeNamespaceImportTest
	{
		[Test]
		public void Constructor0 ()
		{
			CodeNamespaceImport cni = new CodeNamespaceImport ();
			Assert.IsNull (cni.LinePragma, "#1");
			Assert.IsNotNull (cni.Namespace, "#2");
			Assert.AreEqual (string.Empty, cni.Namespace, "#3");

			CodeLinePragma linePragma = new CodeLinePragma ("a", 5);
			cni.LinePragma = linePragma;
			Assert.IsNotNull (cni.LinePragma, "#4");
			Assert.AreSame (linePragma, cni.LinePragma, "#5");

			cni.LinePragma = null;
			Assert.IsNull (cni.LinePragma, "#6");

			string ns = "mono";
			cni.Namespace = ns;
			Assert.AreSame (ns, cni.Namespace, "#7");

			cni.Namespace = null;
			Assert.IsNotNull (cni.Namespace, "#8");
			Assert.AreEqual (string.Empty, cni.Namespace, "#9");
		}

		[Test]
		public void Constructor1 ()
		{
			string ns = "mono";

			CodeNamespaceImport cni = new CodeNamespaceImport (ns);
			Assert.IsNull (cni.LinePragma, "#1");
			Assert.IsNotNull (cni.Namespace, "#2");
			Assert.AreSame (ns, cni.Namespace, "#3");

			cni = new CodeNamespaceImport ((string) null);
			Assert.IsNotNull (cni.Namespace, "#4");
			Assert.AreEqual (string.Empty, cni.Namespace, "#5");
		}
	}
}
