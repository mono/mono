//
// CodeSnippetTypeMemberTest.cs
//	- Unit tests for System.CodeDom.CodeSnippetTypeMember
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
	public class CodeSnippetTypeMemberTest
	{
		[Test]
		public void Constructor0 ()
		{
			CodeSnippetTypeMember cstm = new CodeSnippetTypeMember ();

			Assert.AreEqual (MemberAttributes.Private | MemberAttributes.Final,
				cstm.Attributes, "#1");

			Assert.IsNotNull (cstm.Comments, "#2");
			Assert.AreEqual (0, cstm.Comments.Count, "#3");

			Assert.IsNotNull (cstm.CustomAttributes, "#4");
			Assert.AreEqual (0, cstm.CustomAttributes.Count, "#5");

#if NET_2_0
			Assert.IsNotNull (cstm.StartDirectives, "#6");
			Assert.AreEqual (0, cstm.StartDirectives.Count, "#7");

			Assert.IsNotNull (cstm.EndDirectives, "#8");
			Assert.AreEqual (0, cstm.EndDirectives.Count, "#9");
#endif

			Assert.IsNotNull (cstm.Text, "#10");
			Assert.AreEqual (string.Empty, cstm.Text, "#11");

			Assert.IsNull (cstm.LinePragma, "#12");

			Assert.IsNotNull (cstm.Name, "#13");
			Assert.AreEqual (string.Empty, cstm.Name, "#14");

			Assert.IsNotNull (cstm.UserData, "#15");
			Assert.AreEqual (typeof(ListDictionary), cstm.UserData.GetType (), "#16");
			Assert.AreEqual (0, cstm.UserData.Count, "#17");

			cstm.Name = null;
			Assert.IsNotNull (cstm.Name, "#18");
			Assert.AreEqual (string.Empty, cstm.Name, "#19");

			CodeLinePragma clp = new CodeLinePragma ("mono", 10);
			cstm.LinePragma = clp;
			Assert.IsNotNull (cstm.LinePragma, "#20");
			Assert.AreSame (clp, cstm.LinePragma, "#21");
		}

		[Test]
		public void Constructor1 () {
			string text = "mono";

			CodeSnippetTypeMember cstm = new CodeSnippetTypeMember (text);
			Assert.IsNotNull (cstm.Text, "#1");
			Assert.AreSame (text, cstm.Text, "#2");

			cstm = new CodeSnippetTypeMember ((string) null);
			Assert.IsNotNull (cstm.Text, "#3");
			Assert.AreEqual (string.Empty, cstm.Text, "#4");
		}
	}
}
