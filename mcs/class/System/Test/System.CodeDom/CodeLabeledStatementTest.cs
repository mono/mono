//
// CodeLabeledStatementTest.cs
//	- Unit tests for System.CodeDom.CodeLabeledStatement
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
	public class CodeLabeledStatementTest
	{
		[Test]
		public void Constructor0 ()
		{
			CodeLabeledStatement cls = new CodeLabeledStatement ();
			Assert.IsNull (cls.LinePragma, "#1");

			Assert.IsNotNull (cls.Label, "#2");
			Assert.AreEqual (string.Empty, cls.Label, "#3");

#if NET_2_0
			Assert.IsNotNull (cls.StartDirectives, "#4");
			Assert.AreEqual (0, cls.StartDirectives.Count, "#5");

			Assert.IsNotNull (cls.EndDirectives, "#6");
			Assert.AreEqual (0, cls.EndDirectives.Count, "#7");
#endif

			Assert.IsNotNull (cls.UserData, "#8");
			Assert.AreEqual (typeof(ListDictionary), cls.UserData.GetType (), "#9");
			Assert.AreEqual (0, cls.UserData.Count, "#10");
			
			string label = "mono";
			cls.Label = label;
			Assert.IsNotNull (cls.Label, "#11");
			Assert.AreSame (label, cls.Label, "#12");

			cls.Label = null;
			Assert.IsNotNull (cls.Label, "#13");
			Assert.AreEqual (string.Empty, cls.Label, "#14");

			CodeLinePragma clp = new CodeLinePragma ("mono", 10);
			cls.LinePragma = clp;
			Assert.IsNotNull (cls.LinePragma, "#15");
			Assert.AreSame (clp, cls.LinePragma, "#16");

			cls.LinePragma = null;
			Assert.IsNull (cls.LinePragma, "#17");

			Assert.IsNull (cls.Statement, "#18");

			CodeStatement stmt = new CodeStatement ();
			cls.Statement = stmt;
			Assert.IsNotNull (cls.Statement, "#19");
			Assert.AreSame (stmt, cls.Statement);
		}

		[Test]
		public void Constructor1 ()
		{
			string label = "mono";

			CodeLabeledStatement cls = new CodeLabeledStatement (label);
			Assert.IsNull (cls.LinePragma, "#1");

			Assert.IsNotNull (cls.Label, "#2");
			Assert.AreSame (label, cls.Label, "#3");

#if NET_2_0
			Assert.IsNotNull (cls.StartDirectives, "#4");
			Assert.AreEqual (0, cls.StartDirectives.Count, "#5");

			Assert.IsNotNull (cls.EndDirectives, "#6");
			Assert.AreEqual (0, cls.EndDirectives.Count, "#7");
#endif

			Assert.IsNotNull (cls.UserData, "#8");
			Assert.AreEqual (typeof(ListDictionary), cls.UserData.GetType (), "#9");
			Assert.AreEqual (0, cls.UserData.Count, "#10");

			Assert.IsNull (cls.Statement, "#11");
			
			cls = new CodeLabeledStatement ((string) null);
			Assert.IsNotNull (cls.Label, "#12");
			Assert.AreEqual (string.Empty, cls.Label, "#13");
		}

		[Test]
		public void Constructor2 () {
			string label = "mono";
			CodeStatement stmt = new CodeStatement ();

			CodeLabeledStatement cls = new CodeLabeledStatement (label, stmt);
			Assert.IsNull (cls.LinePragma, "#1");

			Assert.IsNotNull (cls.Label, "#2");
			Assert.AreSame (label, cls.Label, "#3");

#if NET_2_0
			Assert.IsNotNull (cls.StartDirectives, "#4");
			Assert.AreEqual (0, cls.StartDirectives.Count, "#5");

			Assert.IsNotNull (cls.EndDirectives, "#6");
			Assert.AreEqual (0, cls.EndDirectives.Count, "#7");
#endif

			Assert.IsNotNull (cls.UserData, "#8");
			Assert.AreEqual (typeof(ListDictionary), cls.UserData.GetType (), "#9");
			Assert.AreEqual (0, cls.UserData.Count, "#10");

			Assert.IsNotNull (cls.Statement, "#11");
			Assert.AreSame (stmt, cls.Statement, "#12");
			
			cls = new CodeLabeledStatement ((string) null, (CodeStatement) null);
			Assert.IsNotNull (cls.Label, "#13");
			Assert.AreEqual (string.Empty, cls.Label, "#14");
			Assert.IsNull (cls.Statement, "#15");
		}
	}
}
