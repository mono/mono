//
// CodeSnippetStatementTest.cs
//	- Unit tests for System.CodeDom.CodeSnippetStatement
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
	public class CodeSnippetStatementTest
	{
		[Test]
		public void Constructor0 ()
		{
			CodeSnippetStatement css = new CodeSnippetStatement ();
			Assert.IsNull (css.LinePragma, "#1");

			Assert.IsNotNull (css.Value, "#2");
			Assert.AreEqual (string.Empty, css.Value, "#3");

#if NET_2_0
			Assert.IsNotNull (css.StartDirectives, "#4");
			Assert.AreEqual (0, css.StartDirectives.Count, "#5");

			Assert.IsNotNull (css.EndDirectives, "#6");
			Assert.AreEqual (0, css.EndDirectives.Count, "#7");
#endif

			Assert.IsNotNull (css.UserData, "#8");
			Assert.AreEqual (typeof(ListDictionary), css.UserData.GetType (), "#9");
			Assert.AreEqual (0, css.UserData.Count, "#10");
			
			css.Value = null;
			Assert.IsNotNull (css.Value, "#11");
			Assert.AreEqual (string.Empty, css.Value, "#12");

			CodeLinePragma clp = new CodeLinePragma ("mono", 10);
			css.LinePragma = clp;
			Assert.IsNotNull (css.LinePragma, "#13");
			Assert.AreSame (clp, css.LinePragma, "#14");

			css.LinePragma = null;
			Assert.IsNull (css.LinePragma, "#15");
		}

		[Test]
		public void Constructor1 ()
		{
			string stmt = "mono";

			CodeSnippetStatement css = new CodeSnippetStatement (stmt);
			Assert.IsNull (css.LinePragma, "#1");

			Assert.IsNotNull (css.Value, "#2");
			Assert.AreEqual (stmt, css.Value, "#3");
			Assert.AreSame (stmt, css.Value, "#4");

#if NET_2_0
			Assert.IsNotNull (css.StartDirectives, "#5");
			Assert.AreEqual (0, css.StartDirectives.Count, "#6");

			Assert.IsNotNull (css.EndDirectives, "#7");
			Assert.AreEqual (0, css.EndDirectives.Count, "#8");
#endif

			Assert.IsNotNull (css.UserData, "#9");
			Assert.AreEqual (typeof(ListDictionary), css.UserData.GetType (), "#10");
			Assert.AreEqual (0, css.UserData.Count, "#11");
			
			css.Value = null;
			Assert.IsNotNull (css.Value, "#12");
			Assert.AreEqual (string.Empty, css.Value, "#13");

			CodeLinePragma clp = new CodeLinePragma ("mono", 10);
			css.LinePragma = clp;
			Assert.IsNotNull (css.LinePragma, "#14");
			Assert.AreSame (clp, css.LinePragma, "#15");

			css = new CodeSnippetStatement ((string) null);
			Assert.IsNotNull (css.Value, "#16");
			Assert.AreEqual (string.Empty, css.Value, "#17");
		}
	}
}
