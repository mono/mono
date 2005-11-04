//
// CodeSnippetCompileUnitTest.cs
//	- Unit tests for System.CodeDom.CodeSnippetCompileUnit
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
	public class CodeSnippetCompileUnitTest
	{
#if NET_2_0
		[Test]
		public void Constructor0 ()
		{
			CodeSnippetCompileUnit cscu = new CodeSnippetCompileUnit ();
			Assert.IsNull (cscu.LinePragma, "#1");

			Assert.IsNotNull (cscu.Value, "#2");
			Assert.AreEqual (string.Empty, cscu.Value, "#3");

			Assert.IsNotNull (cscu.AssemblyCustomAttributes, "#4");
			Assert.AreEqual (0, cscu.AssemblyCustomAttributes.Count, "#5");

			Assert.IsNotNull (cscu.EndDirectives, "#6");
			Assert.AreEqual (0, cscu.EndDirectives.Count, "#7");

			Assert.IsNotNull (cscu.Namespaces, "#8");
			Assert.AreEqual (0, cscu.Namespaces.Count, "#9");

			Assert.IsNotNull (cscu.ReferencedAssemblies, "#10");
			Assert.AreEqual (0, cscu.ReferencedAssemblies.Count, "#11");

			Assert.IsNotNull (cscu.StartDirectives, "#12");
			Assert.AreEqual (0, cscu.StartDirectives.Count, "#13");

			Assert.IsNotNull (cscu.UserData, "#14");
			Assert.AreEqual (typeof(ListDictionary), cscu.UserData.GetType (), "#15");
			Assert.AreEqual (0, cscu.UserData.Count, "#16");
			
			cscu.Value = null;
			Assert.IsNotNull (cscu.Value, "#17");
			Assert.AreEqual (string.Empty, cscu.Value, "#18");

			CodeLinePragma clp = new CodeLinePragma ("mono", 10);
			cscu.LinePragma = clp;
			Assert.IsNotNull (cscu.LinePragma, "#19");
			Assert.AreSame (clp, cscu.LinePragma, "#20");
		}
#endif

		[Test]
		public void Constructor1 ()
		{
			string value = "mono";

			CodeSnippetCompileUnit cscu = new CodeSnippetCompileUnit (value);
			Assert.IsNull (cscu.LinePragma, "#1");

			Assert.IsNotNull (cscu.Value, "#2");
			Assert.AreEqual (value, cscu.Value, "#3");
			Assert.AreSame (value, cscu.Value, "#4"); 

			Assert.IsNotNull (cscu.AssemblyCustomAttributes, "#5");
			Assert.AreEqual (0, cscu.AssemblyCustomAttributes.Count, "#6");

			Assert.IsNotNull (cscu.Namespaces, "#7");
			Assert.AreEqual (0, cscu.Namespaces.Count, "#8");

			Assert.IsNotNull (cscu.ReferencedAssemblies, "#9");
			Assert.AreEqual (0, cscu.ReferencedAssemblies.Count, "#10");

#if NET_2_0
			Assert.IsNotNull (cscu.StartDirectives, "#11");
			Assert.AreEqual (0, cscu.StartDirectives.Count, "#12");

			Assert.IsNotNull (cscu.EndDirectives, "#13");
			Assert.AreEqual (0, cscu.EndDirectives.Count, "#14");
#endif

			Assert.IsNotNull (cscu.UserData, "#15");
			Assert.AreEqual (typeof(ListDictionary), cscu.UserData.GetType (), "#16");
			Assert.AreEqual (0, cscu.UserData.Count, "#17");
			
			cscu.Value = null;
			Assert.IsNotNull (cscu.Value, "#18");
			Assert.AreEqual (string.Empty, cscu.Value, "#19");

			CodeLinePragma clp = new CodeLinePragma ("mono", 10);
			cscu.LinePragma = clp;
			Assert.IsNotNull (cscu.LinePragma, "#20");
			Assert.AreSame (clp, cscu.LinePragma, "#21");

			cscu = new CodeSnippetCompileUnit ((string) null);

			Assert.IsNotNull (cscu.Value, "#22");
			Assert.AreEqual (string.Empty, cscu.Value, "#23");
		}
	}
}
