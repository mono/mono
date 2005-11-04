//
// CodeTypeConstructorTest.cs
//	- Unit tests for System.CodeDom.CodeTypeConstructor
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
	public class CodeTypeConstructorTest
	{
		[Test]
		public void Constructor0 ()
		{
			CodeTypeConstructor ctc = new CodeTypeConstructor ();

			Assert.AreEqual (MemberAttributes.Private | MemberAttributes.Final,
				ctc.Attributes, "#1");

			Assert.IsNotNull (ctc.Comments, "#2");
			Assert.AreEqual (0, ctc.Comments.Count, "#3");

			Assert.IsNotNull (ctc.CustomAttributes, "#4");
			Assert.AreEqual (0, ctc.CustomAttributes.Count, "#5");

#if NET_2_0
			Assert.IsNotNull (ctc.StartDirectives, "#6");
			Assert.AreEqual (0, ctc.StartDirectives.Count, "#7");

			Assert.IsNotNull (ctc.EndDirectives, "#8");
			Assert.AreEqual (0, ctc.EndDirectives.Count, "#9");

			Assert.IsNotNull (ctc.TypeParameters, "#10");
			Assert.AreEqual (0, ctc.TypeParameters.Count, "#11");
#endif

			Assert.IsNull (ctc.LinePragma, "#12");

			Assert.IsNotNull (ctc.Name, "#13");
			Assert.AreEqual (".cctor", ctc.Name, "#14");

			Assert.IsNotNull (ctc.UserData, "#15");
			Assert.AreEqual (typeof(ListDictionary), ctc.UserData.GetType (), "#16");
			Assert.AreEqual (0, ctc.UserData.Count, "#17");

			Assert.IsNotNull (ctc.ImplementationTypes, "#18");
			Assert.AreEqual (0, ctc.ImplementationTypes.Count, "#19");

			Assert.IsNotNull (ctc.Parameters, "#20");
			Assert.AreEqual (0, ctc.Parameters.Count, "#21");

			Assert.IsNull (ctc.PrivateImplementationType, "#22");

			Assert.IsNotNull (ctc.ReturnType, "#23");
			Assert.AreEqual (typeof(void).FullName, ctc.ReturnType.BaseType, "#24");

			Assert.IsNotNull (ctc.ReturnTypeCustomAttributes, "#25");
			Assert.AreEqual (0, ctc.ReturnTypeCustomAttributes.Count, "#26");

			Assert.IsNotNull (ctc.Statements, "#27");
			Assert.AreEqual (0, ctc.Statements.Count, "#28");

			string name = "mono";
			ctc.Name = name;
			Assert.IsNotNull (ctc.Name, "#29");
			Assert.AreSame (name, ctc.Name, "#30");

			ctc.Name = null;
			Assert.IsNotNull (ctc.Name, "#31");
			Assert.AreEqual (string.Empty, ctc.Name, "#32");

			CodeLinePragma clp = new CodeLinePragma ("mono", 10);
			ctc.LinePragma = clp;
			Assert.IsNotNull (ctc.LinePragma, "#33");
			Assert.AreSame (clp, ctc.LinePragma, "#34");

			ctc.LinePragma = null;
			Assert.IsNull (ctc.LinePragma, "#35");
		}
	}
}
