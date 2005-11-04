//
// CodeMemberMethodTest.cs
//	- Unit tests for System.CodeDom.CodeMemberMethod
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
	public class CodeMemberMethodTest
	{
		[Test]
		public void Constructor0 ()
		{
			CodeMemberMethod cmm = new CodeMemberMethod ();

			Assert.AreEqual (MemberAttributes.Private | MemberAttributes.Final,
				cmm.Attributes, "#1");

			Assert.IsNotNull (cmm.Comments, "#2");
			Assert.AreEqual (0, cmm.Comments.Count, "#3");

			Assert.IsNotNull (cmm.CustomAttributes, "#4");
			Assert.AreEqual (0, cmm.CustomAttributes.Count, "#5");

#if NET_2_0
			Assert.IsNotNull (cmm.StartDirectives, "#6");
			Assert.AreEqual (0, cmm.StartDirectives.Count, "#7");

			Assert.IsNotNull (cmm.EndDirectives, "#8");
			Assert.AreEqual (0, cmm.EndDirectives.Count, "#9");

			Assert.IsNotNull (cmm.TypeParameters, "#10");
			Assert.AreEqual (0, cmm.TypeParameters.Count, "#11");
#endif

			Assert.IsNull (cmm.LinePragma, "#12");

			Assert.IsNotNull (cmm.Name, "#13");
			Assert.AreEqual (string.Empty, cmm.Name, "#14");

			Assert.IsNotNull (cmm.UserData, "#15");
			Assert.AreEqual (typeof(ListDictionary), cmm.UserData.GetType (), "#16");
			Assert.AreEqual (0, cmm.UserData.Count, "#17");

			Assert.IsNotNull (cmm.ImplementationTypes, "#18");
			Assert.AreEqual (0, cmm.ImplementationTypes.Count, "#19");

			Assert.IsNotNull (cmm.Parameters, "#20");
			Assert.AreEqual (0, cmm.Parameters.Count, "#21");

			Assert.IsNull (cmm.PrivateImplementationType, "#22");

			Assert.IsNotNull (cmm.ReturnType, "#23");
			Assert.AreEqual (typeof(void).FullName, cmm.ReturnType.BaseType, "#24");

			Assert.IsNotNull (cmm.ReturnTypeCustomAttributes, "#25");
			Assert.AreEqual (0, cmm.ReturnTypeCustomAttributes.Count, "#26");

			Assert.IsNotNull (cmm.Statements, "#27");
			Assert.AreEqual (0, cmm.Statements.Count, "#28");

			string name = "mono";
			cmm.Name = name;
			Assert.IsNotNull (cmm.Name, "#29");
			Assert.AreSame (name, cmm.Name, "#30");

			cmm.Name = null;
			Assert.IsNotNull (cmm.Name, "#31");
			Assert.AreEqual (string.Empty, cmm.Name, "#32");

			CodeLinePragma clp = new CodeLinePragma ("mono", 10);
			cmm.LinePragma = clp;
			Assert.IsNotNull (cmm.LinePragma, "#31");
			Assert.AreSame (clp, cmm.LinePragma, "#32");

			cmm.LinePragma = null;
			Assert.IsNull (cmm.LinePragma, "#33");
		}
	}
}
