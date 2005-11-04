//
// CodeConstructorTest.cs
//	- Unit tests for System.CodeDom.CodeConstructor
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
	public class CodeConstructorTest
	{
		[Test]
		public void Constructor0 ()
		{
			CodeConstructor cc = new CodeConstructor ();

			Assert.AreEqual (MemberAttributes.Private | MemberAttributes.Final,
				cc.Attributes, "#1");

			Assert.IsNotNull (cc.Comments, "#2");
			Assert.AreEqual (0, cc.Comments.Count, "#3");

			Assert.IsNotNull (cc.CustomAttributes, "#4");
			Assert.AreEqual (0, cc.CustomAttributes.Count, "#5");

#if NET_2_0
			Assert.IsNotNull (cc.StartDirectives, "#6");
			Assert.AreEqual (0, cc.StartDirectives.Count, "#7");

			Assert.IsNotNull (cc.EndDirectives, "#8");
			Assert.AreEqual (0, cc.EndDirectives.Count, "#9");

			Assert.IsNotNull (cc.TypeParameters, "#10");
			Assert.AreEqual (0, cc.TypeParameters.Count, "#11");
#endif

			Assert.IsNull (cc.LinePragma, "#12");

			Assert.IsNotNull (cc.Name, "#13");
			Assert.AreEqual (".ctor", cc.Name, "#14");

			Assert.IsNotNull (cc.UserData, "#15");
			Assert.AreEqual (typeof(ListDictionary), cc.UserData.GetType (), "#16");
			Assert.AreEqual (0, cc.UserData.Count, "#17");

			Assert.IsNotNull (cc.ImplementationTypes, "#18");
			Assert.AreEqual (0, cc.ImplementationTypes.Count, "#19");

			Assert.IsNotNull (cc.Parameters, "#20");
			Assert.AreEqual (0, cc.Parameters.Count, "#21");

			Assert.IsNull (cc.PrivateImplementationType, "#22");

			Assert.IsNotNull (cc.ReturnType, "#23");
			Assert.AreEqual (typeof(void).FullName, cc.ReturnType.BaseType, "#24");

			Assert.IsNotNull (cc.ReturnTypeCustomAttributes, "#25");
			Assert.AreEqual (0, cc.ReturnTypeCustomAttributes.Count, "#26");

			Assert.IsNotNull (cc.Statements, "#27");
			Assert.AreEqual (0, cc.Statements.Count, "#28");

			string name = "mono";
			cc.Name = name;
			Assert.IsNotNull (cc.Name, "#29");
			Assert.AreSame (name, cc.Name, "#30");

			cc.Name = null;
			Assert.IsNotNull (cc.Name, "#31");
			Assert.AreEqual (string.Empty, cc.Name, "#32");

			CodeLinePragma clp = new CodeLinePragma ("mono", 10);
			cc.LinePragma = clp;
			Assert.IsNotNull (cc.LinePragma, "#33");
			Assert.AreSame (clp, cc.LinePragma, "#34");

			cc.LinePragma = null;
			Assert.IsNull (cc.LinePragma, "#35");

			Assert.IsNotNull (cc.BaseConstructorArgs, "#36");
			Assert.AreEqual (0, cc.BaseConstructorArgs.Count, "#37");

			Assert.IsNotNull (cc.ChainedConstructorArgs, "#38");
			Assert.AreEqual (0, cc.ChainedConstructorArgs.Count, "#37");
		}
	}
}
