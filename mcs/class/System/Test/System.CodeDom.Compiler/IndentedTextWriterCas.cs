//
// IndentedTextWriterCas.cs 
//	- CAS unit tests for System.CodeDom.Compiler.IndentedTextWriter
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
using System.IO;
using System.Reflection;
using System.Security;
using System.Security.Permissions;

namespace MonoCasTests.System.CodeDom.Compiler {

	[TestFixture]
	[Category ("CAS")]
	public class IndentedTextWriterCas {

		private TextWriter writer;

		[SetUp]
		public void SetUp ()
		{
			if (!SecurityManager.SecurityEnabled)
				Assert.Ignore ("SecurityManager.SecurityEnabled is OFF");
			writer = new StringWriter ();
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Defaults_Deny_Unrestricted ()
		{
			// note: CAS doesn't apply to fields
			Assert.AreEqual ("    ", IndentedTextWriter.DefaultTabString, "DefaultTabString");
		}

		private void TouchEverything (IndentedTextWriter itw)
		{
			Assert.AreSame (writer.Encoding, itw.Encoding, "Encoding");
			Assert.AreEqual (0, itw.Indent, "Indent");
			itw.Indent = 1;
			Assert.AreSame (writer, itw.InnerWriter, "InnerWriter");
			Assert.AreEqual (writer.NewLine, itw.NewLine, "NewLine");

			itw.Write (true);
			itw.Write (Char.MinValue);
			itw.Write (Path.InvalidPathChars); // char[]
			itw.Write (Double.MinValue);
			itw.Write (Int32.MinValue);
			itw.Write (Int64.MaxValue);
			itw.Write (new object ());
			itw.Write (Single.MinValue);
			itw.Write (String.Empty);
			itw.Write ("{0}", String.Empty);
			itw.Write ("{0}{1}", Int32.MinValue, Int32.MaxValue);
			itw.Write ("{0}{1}{2}", Int32.MinValue, 0, Int32.MaxValue);
			itw.Write (Path.InvalidPathChars, 0, Path.InvalidPathChars.Length);
			itw.WriteLine ();
			itw.WriteLine (true);
			itw.WriteLine (Char.MinValue);
			itw.WriteLine (Path.InvalidPathChars); // char[]
			itw.WriteLine (Double.MinValue);
			itw.WriteLine (Int32.MinValue);
			itw.WriteLine (Int64.MaxValue);
			itw.WriteLine (new object ());
			itw.WriteLine (Single.MinValue);
			itw.WriteLine (String.Empty);
			itw.WriteLine (UInt32.MaxValue);
			itw.WriteLine ("{0}", String.Empty);
			itw.WriteLine ("{0}{1}", Int32.MinValue, Int32.MaxValue);
			itw.WriteLine ("{0}{1}{2}", Int32.MinValue, 0, Int32.MaxValue);
			itw.WriteLine (Path.InvalidPathChars, 0, Path.InvalidPathChars.Length);
			itw.WriteLineNoTabs (String.Empty);
			itw.Flush ();
			itw.Close ();
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Constructor1_Deny_Unrestricted ()
		{
			IndentedTextWriter itw = new IndentedTextWriter (writer);
			TouchEverything (itw);
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Constructor2_Deny_Unrestricted ()
		{
			IndentedTextWriter itw = new IndentedTextWriter (writer, "\t");
			TouchEverything (itw);
		}

		[Test]
		public void LinkDemand_No_Restriction ()
		{
			Type[] types = new Type[1] { typeof (TextWriter) };
			ConstructorInfo ci = typeof (IndentedTextWriter).GetConstructor (types);
			Assert.IsNotNull (ci, ".ctor(TextWriter)");
			Assert.IsNotNull (ci.Invoke (new object[1] { writer }), "invoke");
		}

		[Test]
		[EnvironmentPermission (SecurityAction.Deny, Read = "Mono")]
		[ExpectedException (typeof (SecurityException))]
		public void LinkDemand_Deny_Anything ()
		{
			// denying anything results in a non unrestricted permission set
			Type[] types = new Type[1] { typeof (TextWriter) };
			ConstructorInfo ci = typeof (IndentedTextWriter).GetConstructor (types);
			Assert.IsNotNull (ci, ".ctor(TextWriter)");
			Assert.IsNotNull (ci.Invoke (new object[1] { writer }), "invoke");
		}
	}
}
