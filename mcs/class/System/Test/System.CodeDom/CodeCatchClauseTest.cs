//
// CodeCatchClauseTest.cs
//	- Unit tests for System.CodeDom.CodeCatchClause
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
	public class CodeCatchClauseTest
	{
		[Test]
		public void Constructor0 ()
		{
			CodeCatchClause ccc = new CodeCatchClause ();

			Assert.IsNotNull (ccc.CatchExceptionType, "#1");
			Assert.AreEqual (typeof(Exception).FullName, ccc.CatchExceptionType.BaseType, "#2");

			Assert.IsNotNull (ccc.LocalName, "#3");
			Assert.AreEqual (string.Empty, ccc.LocalName, "#4");

			Assert.IsNotNull (ccc.Statements, "#5");
			Assert.AreEqual (0, ccc.Statements.Count, "#6");

			ccc.LocalName = null;
			Assert.IsNotNull (ccc.LocalName, "#7");
			Assert.AreEqual (string.Empty, ccc.LocalName, "#8");

			string localName = "mono";
			ccc.LocalName = localName;
			Assert.AreSame (localName, ccc.LocalName, "#9");

			ccc.CatchExceptionType = null;
			Assert.IsNotNull (ccc.CatchExceptionType, "#10");
			Assert.AreEqual (typeof(Exception).FullName, ccc.CatchExceptionType.BaseType, "#11");

			CodeTypeReference cet = new CodeTypeReference("SomeException");
			ccc.CatchExceptionType = cet;
			Assert.AreSame (cet, ccc.CatchExceptionType, "#12");
		}

		[Test]
		public void Constructor1 () {
			string localName = "mono";

			CodeCatchClause ccc = new CodeCatchClause (localName);

			Assert.IsNotNull (ccc.CatchExceptionType, "#1");
			Assert.AreEqual (typeof(Exception).FullName, ccc.CatchExceptionType.BaseType, "#2");

			Assert.IsNotNull (ccc.LocalName, "#3");
			Assert.AreEqual (localName, ccc.LocalName, "#4");
			Assert.AreSame (localName, ccc.LocalName, "#5");

			Assert.IsNotNull (ccc.Statements, "#6");
			Assert.AreEqual (0, ccc.Statements.Count, "#7");

			ccc.LocalName = null;
			Assert.IsNotNull (ccc.LocalName, "#8");
			Assert.AreEqual (string.Empty, ccc.LocalName, "#9");

			ccc = new CodeCatchClause ((string) null);
			Assert.IsNotNull (ccc.LocalName, "#10");
			Assert.AreEqual (string.Empty, ccc.LocalName, "#22");
		}

		[Test]
		public void Constructor2 () {
			string localName = "mono";
			CodeTypeReference cet = new CodeTypeReference("SomeException");

			CodeCatchClause ccc = new CodeCatchClause (localName, cet);

			Assert.IsNotNull (ccc.CatchExceptionType, "#1");
			Assert.AreSame (cet, ccc.CatchExceptionType, "#2");

			Assert.IsNotNull (ccc.LocalName, "#3");
			Assert.AreEqual (localName, ccc.LocalName, "#4");
			Assert.AreSame (localName, ccc.LocalName, "#5");

			Assert.IsNotNull (ccc.Statements, "#6");
			Assert.AreEqual (0, ccc.Statements.Count, "#7");

			ccc.LocalName = null;
			Assert.IsNotNull (ccc.LocalName, "#8");
			Assert.AreEqual (string.Empty, ccc.LocalName, "#9");

			ccc.CatchExceptionType = null;
			Assert.IsNotNull (ccc.CatchExceptionType, "#10");
			Assert.AreEqual (typeof(Exception).FullName, ccc.CatchExceptionType.BaseType, "#11");

			ccc = new CodeCatchClause ((string) null, (CodeTypeReference) null);
			Assert.IsNotNull (ccc.LocalName, "#12");
			Assert.AreEqual (string.Empty, ccc.LocalName, "#13");
			Assert.IsNotNull (ccc.CatchExceptionType, "#14");
			Assert.AreEqual (typeof(Exception).FullName, ccc.CatchExceptionType.BaseType, "#15");
		}

		[Test]
		public void Constructor3 () {
			string localName = "mono";
			CodeTypeReference cet = new CodeTypeReference("SomeException");
			CodeStatement cs1 = new CodeStatement ();
			CodeStatement cs2 = new CodeStatement ();

			CodeCatchClause ccc = new CodeCatchClause (localName, cet, cs1, cs2);

			Assert.IsNotNull (ccc.CatchExceptionType, "#1");
			Assert.AreSame (cet, ccc.CatchExceptionType, "#2");

			Assert.IsNotNull (ccc.LocalName, "#3");
			Assert.AreEqual (localName, ccc.LocalName, "#4");
			Assert.AreSame (localName, ccc.LocalName, "#5");

			Assert.IsNotNull (ccc.Statements, "#6");
			Assert.AreEqual (2, ccc.Statements.Count, "#7");
			Assert.AreSame (cs1, ccc.Statements[0], "#8");
			Assert.AreSame (cs2, ccc.Statements[1], "#9");

			ccc.LocalName = null;
			Assert.IsNotNull (ccc.LocalName, "#8");
			Assert.AreEqual (string.Empty, ccc.LocalName, "#9");

			ccc.CatchExceptionType = null;
			Assert.IsNotNull (ccc.CatchExceptionType, "#10");
			Assert.AreEqual (typeof(Exception).FullName, ccc.CatchExceptionType.BaseType, "#11");

			ccc = new CodeCatchClause ((string) null, (CodeTypeReference) null, cs1);
			Assert.IsNotNull (ccc.LocalName, "#12");
			Assert.AreEqual (string.Empty, ccc.LocalName, "#13");
			Assert.IsNotNull (ccc.CatchExceptionType, "#14");
			Assert.AreEqual (typeof(Exception).FullName, ccc.CatchExceptionType.BaseType, "#15");
			Assert.AreEqual (1, ccc.Statements.Count, "#16");
			Assert.AreSame (cs1, ccc.Statements[0], "#17");
		}
	}
}
