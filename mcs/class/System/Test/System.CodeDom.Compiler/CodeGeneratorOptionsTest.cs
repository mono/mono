//
// CodeGeneratorOptionsTest.cs 
//	- Unit tests for System.CodeDom.Compiler.CodeGeneratorOptions
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
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
using System.CodeDom.Compiler;

namespace MonoTests.System.CodeDom.Compiler {

	[TestFixture]
	public class CodeGeneratorOptionsTest {

		[Test]
		public void Defaults ()
		{
			CodeGeneratorOptions cgo = new CodeGeneratorOptions ();
			Assert.IsTrue (cgo.BlankLinesBetweenMembers, "BlankLinesBetweenMembers");
			Assert.AreEqual ("Block", cgo.BracingStyle, "BracingStyle");
			Assert.IsFalse (cgo.ElseOnClosing, "ElseOnClosing");
			Assert.AreEqual ("    ", cgo.IndentString, "IndentString");
#if NET_2_0
			Assert.IsFalse (cgo.VerbatimOrder, "VerbatimOrder");
#endif
			Assert.IsNull (cgo["BlankLinesBetweenMembers"], "this[BlankLinesBetweenMembers]");
			Assert.IsNull (cgo["BracingStyle"], "this[BracingStyle]");
			Assert.IsNull (cgo["ElseOnClosing"], "this[ElseOnClosing]");
			Assert.IsNull (cgo["IndentString"], "this[IndentString]");
#if NET_2_0
			Assert.IsNull (cgo["VerbatimOrder"], "this[VerbatimOrder]");
#endif
		}

		[Test]
		public void ReSetDefault ()
		{
			CodeGeneratorOptions cgo = new CodeGeneratorOptions ();

			cgo.BlankLinesBetweenMembers = cgo.BlankLinesBetweenMembers;
			Assert.IsNotNull (cgo["BlankLinesBetweenMembers"], "this[BlankLinesBetweenMembers]");
			cgo.BracingStyle = cgo.BracingStyle;
			Assert.IsNotNull (cgo["BracingStyle"], "this[BracingStyle]");
			cgo.ElseOnClosing = cgo.ElseOnClosing;
			Assert.IsNotNull (cgo["ElseOnClosing"], "this[ElseOnClosing]");
			cgo.IndentString = cgo.IndentString;
			Assert.IsNotNull (cgo["IndentString"], "this[IndentString]");
#if NET_2_0
			cgo.VerbatimOrder = cgo.VerbatimOrder;
			Assert.IsNotNull (cgo["VerbatimOrder"], "this[VerbatimOrder]");
#endif
		}

		[Test]
		public void Nullify ()
		{
			CodeGeneratorOptions cgo = new CodeGeneratorOptions ();
			cgo.BlankLinesBetweenMembers = false;
			Assert.IsFalse (cgo.BlankLinesBetweenMembers, "BlankLinesBetweenMembers-1");
			cgo["BlankLinesBetweenMembers"] = null;
			Assert.IsTrue (cgo.BlankLinesBetweenMembers, "BlankLinesBetweenMembers-2");

			cgo.BracingStyle = "C";
			Assert.AreEqual ("C", cgo.BracingStyle, "BracingStyle-1");
			cgo["BracingStyle"] = null;
			Assert.AreEqual ("Block", cgo.BracingStyle, "BracingStyle-2");

			cgo.ElseOnClosing = true;
			Assert.IsTrue (cgo.ElseOnClosing, "ElseOnClosing-1");
			cgo["ElseOnClosing"] = null;
			Assert.IsFalse (cgo.ElseOnClosing, "ElseOnClosing-2");

			cgo.IndentString = "\t";
			Assert.AreEqual ("\t", cgo.IndentString, "IndentString-1");
			cgo["IndentString"] = null;
			Assert.AreEqual ("    ", cgo.IndentString, "IndentString-2");
#if NET_2_0
			cgo.VerbatimOrder = true;
			Assert.IsTrue (cgo.VerbatimOrder, "VerbatimOrder-1");
			cgo["VerbatimOrder"] = null;
			Assert.IsFalse (cgo.VerbatimOrder, "VerbatimOrder-2");
#endif
		}
	}
}
