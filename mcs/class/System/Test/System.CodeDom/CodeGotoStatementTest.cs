//
// CodeGotoStatementTest.cs
//	- Unit tests for System.CodeDom.CodeGotoStatement
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
	public class CodeGotoStatementTest
	{
#if NET_2_0
		[Test]
		public void Constructor0 ()
		{
			CodeGotoStatement cgs = new CodeGotoStatement ();
			Assert.IsNull (cgs.Label, "#1");

			Assert.IsNotNull (cgs.StartDirectives, "#2");
			Assert.AreEqual (0, cgs.StartDirectives.Count, "#3");

			Assert.IsNotNull (cgs.EndDirectives, "#4");
			Assert.AreEqual (0, cgs.EndDirectives.Count, "#5");

			Assert.IsNotNull (cgs.UserData, "#6");
			Assert.AreEqual (typeof(ListDictionary), cgs.UserData.GetType (), "#7");
			Assert.AreEqual (0, cgs.UserData.Count, "#8");

			Assert.IsNull (cgs.LinePragma, "#9");

			CodeLinePragma clp = new CodeLinePragma ("mono", 10);
			cgs.LinePragma = clp;
			Assert.IsNotNull (cgs.LinePragma, "#10");
			Assert.AreSame (clp, cgs.LinePragma, "#11");

			cgs.LinePragma = null;
			Assert.IsNull (cgs.LinePragma, "#12");

			string label = "mono";
			cgs.Label = label;
			Assert.AreSame (label, cgs.Label, "#13");
		}
#endif

		[Test]
		public void Constructor1 ()
		{
			string label1 = "mono1";

			CodeGotoStatement cgs = new CodeGotoStatement (label1);
			Assert.IsNotNull (cgs.Label, "#1");
			Assert.AreSame (label1, cgs.Label, "#2");

#if NET_2_0
			Assert.IsNotNull (cgs.StartDirectives, "#3");
			Assert.AreEqual (0, cgs.StartDirectives.Count, "#4");

			Assert.IsNotNull (cgs.EndDirectives, "#5");
			Assert.AreEqual (0, cgs.EndDirectives.Count, "#6");
#endif

			Assert.IsNotNull (cgs.UserData, "#7");
			Assert.AreEqual (typeof(ListDictionary), cgs.UserData.GetType (), "#8");
			Assert.AreEqual (0, cgs.UserData.Count, "#9");

			Assert.IsNull (cgs.LinePragma, "#10");

			string label2 = "mono2";
			cgs.Label = label2;
			Assert.AreSame (label2, cgs.Label, "#11");
		}

		[Test]
#if NET_2_0
		[ExpectedException (typeof (ArgumentNullException))]
#endif
		public void Constructor1_NullLabel ()
		{
			CodeGotoStatement cgs = new CodeGotoStatement ((string) null);
#if ONLY_1_1
			Assert.IsNull (cgs.Label, "#1");
#endif
		}

		[Test]
#if NET_2_0
		[ExpectedException (typeof (ArgumentNullException))]
#endif
		public void Constructor1_EmptyLabel () {
			CodeGotoStatement cgs = new CodeGotoStatement (string.Empty);
#if ONLY_1_1
			Assert.IsNotNull (cgs.Label, "#1");
			Assert.AreEqual (string.Empty, cgs.Label, "#2");
#endif
		}

		[Test]
#if NET_2_0
		[ExpectedException (typeof (ArgumentNullException))]
#endif
		public void Label_Null ()
		{
			CodeGotoStatement cgs = new CodeGotoStatement ("mono");
			cgs.Label = null;
#if ONLY_1_1
			Assert.IsNull (cgs.Label, "#1");
#endif
		}

		[Test]
#if NET_2_0
		[ExpectedException (typeof (ArgumentNullException))]
#endif
		public void Label_Empty () {
			CodeGotoStatement cgs = new CodeGotoStatement ("mono");
			cgs.Label = string.Empty;
#if ONLY_1_1
			Assert.IsNotNull (cgs.Label, "#1");
			Assert.AreEqual (string.Empty, cgs.Label, "#2");
#endif
		}
	}
}
