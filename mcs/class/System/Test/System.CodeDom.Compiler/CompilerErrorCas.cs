//
// CompilerErrorCas.cs 
//	- CAS unit tests for System.CodeDom.Compiler.CompilerError
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
using System.IO;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Security;
using System.Security.Permissions;

namespace MonoCasTests.System.CodeDom.Compiler {

	[TestFixture]
	[Category ("CAS")]
	public class CompilerErrorCas {

		[SetUp]
		public void SetUp ()
		{
			if (!SecurityManager.SecurityEnabled)
				Assert.Ignore ("SecurityManager.SecurityEnabled is OFF");
		}

		private string fname;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			// at full trust
			fname = Path.GetTempFileName ();
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Constructor0_Deny_Unrestricted ()
		{
			CompilerError ce = new CompilerError ();
			Assert.AreEqual (0, ce.Column, "Column");
			ce.Column = 1;
			Assert.AreEqual (String.Empty, ce.ErrorNumber, "ErrorNumber");
			ce.ErrorNumber = "cs0000";
			Assert.AreEqual (String.Empty, ce.ErrorText, "ErrorText");
			ce.ErrorText = "error text";
			Assert.AreEqual (String.Empty, ce.FileName, "FileName");
			ce.FileName = fname;
			Assert.IsFalse (ce.IsWarning, "IsWarning");
			ce.IsWarning = true;
			Assert.AreEqual (0, ce.Line, "Line");
			ce.Line = 1;
			Assert.IsNotNull (ce.ToString (), "ToString");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Constructor5_Deny_Unrestricted ()
		{
			CompilerError ce = new CompilerError (fname, 1, 1, "cs0000", "error text");
			Assert.IsTrue ((ce.ToString ().IndexOf (fname) >= 0), "ToString");
			Assert.AreEqual (1, ce.Column, "Column");
			ce.Column = Int32.MinValue;
			Assert.AreEqual ("cs0000", ce.ErrorNumber, "ErrorNumber");
			ce.ErrorNumber = String.Empty;
			Assert.AreEqual ("error text", ce.ErrorText, "ErrorText");
			ce.ErrorText = String.Empty;
			Assert.AreEqual (fname, ce.FileName, "FileName");
			ce.FileName = String.Empty;
			Assert.IsFalse (ce.IsWarning, "IsWarning");
			ce.IsWarning = true;
			Assert.AreEqual (1, ce.Line, "Line");
			ce.Line = Int32.MinValue;
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
#if ONLY_1_1
		[ExpectedException (typeof (SecurityException))]
#endif
		public void LinkDemand_Deny_Unrestricted ()
		{
			ConstructorInfo ci = typeof (CompilerError).GetConstructor (new Type [0]);
			Assert.IsNotNull (ci, "default .ctor()");
			Assert.IsNotNull (ci.Invoke (null), "invoke");
		}
	}
}
