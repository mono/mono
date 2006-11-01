//
// XsltCompileExceptionTests.cs 
//	- Unit tests for System.Xml.Xsl.XsltCompileException
//
// Author:
//	Gert Driesen <drieseng@users.sourceforge.net>
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
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;
using System.Xml.Xsl;

namespace MonoCasTests.System.Xml.Xsl
{
	[TestFixture]
	public class XsltCompileExceptionTests {
#if NET_2_0
		[Test]
		public void Constructor0 ()
		{
			XsltCompileException xe = new XsltCompileException ();
			Assert.AreEqual (0, xe.LineNumber, "#A1");
			Assert.AreEqual (0, xe.LinePosition, "#A2");
			Assert.IsNotNull (xe.Message, "#A3");
			Assert.AreEqual (string.Empty, xe.Message, "#A4");
			Assert.IsNull (xe.SourceUri, "#A5");
			Assert.IsNull (xe.InnerException, "#A6");
			Assert.IsNull (xe.Source, "#A7");
			Assert.IsNull (xe.StackTrace, "#A8");
			Assert.IsNull (xe.TargetSite, "#A9");
		}

		[Test]
		public void Constructor1 ()
		{
			string msg = "mono";

			XsltCompileException xe = new XsltCompileException (msg);
			Assert.AreEqual (0, xe.LineNumber, "#A1");
			Assert.AreEqual (0, xe.LinePosition, "#A2");
			Assert.IsNotNull (xe.Message, "#A3");
			Assert.AreEqual ("mono", xe.Message, "#A4");
			Assert.IsNull (xe.SourceUri, "#A5");
			Assert.IsNull (xe.InnerException, "#A6");
			Assert.IsNull (xe.Source, "#A7");
			Assert.IsNull (xe.StackTrace, "#A8");
			Assert.IsNull (xe.TargetSite, "#A9");
		}

		[Test]
		public void Constructor2 ()
		{
			string msg = "mono";
			Exception cause = new ApplicationException ("cause");

			XsltCompileException xe = new XsltCompileException (msg, cause);
			Assert.AreEqual (0, xe.LineNumber, "#A1");
			Assert.AreEqual (0, xe.LinePosition, "#A2");
			Assert.IsNotNull (xe.Message, "#A3");
			Assert.AreEqual ("mono", xe.Message, "#A4");
			Assert.IsNull (xe.SourceUri, "#A5");
			Assert.AreSame (cause, xe.InnerException, "#A6");
			Assert.IsNull (xe.Source, "#A7");
			Assert.IsNull (xe.StackTrace, "#A8");
			Assert.IsNull (xe.TargetSite, "#A9");
		}
#endif

		[Test]
		public void Constructor3 ()
		{
			string sourceUri = "http://local/test.xsl";
			Exception cause = new ApplicationException ("cause");

			// null uri, line 0, pos 0, innerexception set
			XsltCompileException xe = new XsltCompileException (cause, (string) null, 0, 0);
			Assert.AreEqual (0, xe.LineNumber, "#A1");
			Assert.AreEqual (0, xe.LinePosition, "#A2");
			Assert.IsNotNull (xe.Message, "#A3");
			Assert.IsTrue (xe.Message.Length > 0, "#A4");
#if NET_2_0
			// exact us-english error message
			// Assert.AreEqual ("XSLT compile error.", xe.Message, "#A5-US-ENGLISH");
			Assert.AreEqual (-1, xe.Message.IndexOf("(0,0) :\n"), "#A5");
#else
			Assert.AreEqual ("(0,0) :\n", xe.Message, "#A5");
#endif
			Assert.IsNull (xe.SourceUri, "#A6");
			Assert.AreSame (cause, xe.InnerException, "#A7");
			Assert.IsNull (xe.Source, "#A8");
			Assert.IsNull (xe.StackTrace, "#A9");
			Assert.IsNull (xe.TargetSite, "#A10");

			// null uri, line 1, pos 0, innerexception set
			xe = new XsltCompileException (cause, (string) null, 1, 0);
			Assert.AreEqual (1, xe.LineNumber, "#B1");
			Assert.AreEqual (0, xe.LinePosition, "#B2");
			Assert.IsNotNull (xe.Message, "#B3");
			Assert.IsTrue (xe.Message.Length > 0, "#B4");
#if NET_2_0
			// exact us-english error message
			// Assert.AreEqual ("XSLT compile error at (1,0). See InnerException for details.", xe.Message, "#B5-US-ENGLISH");
			Assert.IsTrue (xe.Message.IndexOf("(1,0).") != -1, "#B5");
#else
			Assert.AreEqual ("(1,0) :\n", xe.Message, "#B5");
#endif
			Assert.IsNull (xe.SourceUri, "#B6");
			Assert.AreSame (cause, xe.InnerException, "#B7");
			Assert.IsNull (xe.Source, "#B8");
			Assert.IsNull (xe.StackTrace, "#B9");
			Assert.IsNull (xe.TargetSite, "#B10");

			// null uri, line 0, pos 1, innerexception set
			xe = new XsltCompileException (cause, (string) null, 0, 1);
			Assert.AreEqual (0, xe.LineNumber, "#C1");
			Assert.AreEqual (1, xe.LinePosition, "#C2");
			Assert.IsNotNull (xe.Message, "#C3");
			Assert.IsTrue (xe.Message.Length > 0, "#C4");
#if NET_2_0
			// exact us-english error message
			// Assert.AreEqual ("XSLT compile error.", xe.Message, "#AC-US-ENGLISH");
			Assert.AreEqual (-1, xe.Message.IndexOf("(0,1)"), "#C5");
#else
			Assert.AreEqual ("(0,1) :\n", xe.Message, "#C5");
#endif
			Assert.IsNull (xe.SourceUri, "#C6");
			Assert.AreSame (cause, xe.InnerException, "#C7");
			Assert.IsNull (xe.Source, "#C8");
			Assert.IsNull (xe.StackTrace, "#C9");
			Assert.IsNull (xe.TargetSite, "#C10");

			// uri set, line 0, pos 0, innerexception set
			xe = new XsltCompileException (cause, sourceUri, 0, 0);
			Assert.AreEqual (0, xe.LineNumber, "#D1");
			Assert.AreEqual (0, xe.LinePosition, "#D2");
			Assert.IsNotNull (xe.Message, "#D3");
			Assert.IsTrue (xe.Message.Length > 0, "#D4");
#if NET_2_0
			// exact us-english error message
			// Assert.AreEqual ("XSLT compile error.", xe.Message, "#D5-US-ENGLISH");
			Assert.AreEqual (-1, xe.Message.IndexOf("(0,0)"), "#D5");
#else
			Assert.AreEqual ("http://local/test.xsl(0,0) :\n", xe.Message, "#D5");
#endif
			Assert.AreSame (sourceUri, xe.SourceUri, "#D6");
			Assert.AreSame (cause, xe.InnerException, "#D7");
			Assert.IsNull (xe.Source, "#D8");
			Assert.IsNull (xe.StackTrace, "#D9");
			Assert.IsNull (xe.TargetSite, "#D10");

			// uri set, line 1, pos 0, innerexception set
			xe = new XsltCompileException (cause, sourceUri, 1, 0);
			Assert.AreEqual (1, xe.LineNumber, "#E1");
			Assert.AreEqual (0, xe.LinePosition, "#E2");
			Assert.IsNotNull (xe.Message, "#E3");
			Assert.IsTrue (xe.Message.Length > 0, "#E4");
#if NET_2_0
			// exact us-english error message
			// Assert.AreEqual ("XSLT compile error at http://local/test.xsl(1,0). See InnerException for details.", xe.Message, "#E5-US-ENGLISH");
			Assert.IsTrue (xe.Message.IndexOf("http://local/test.xsl(1,0)") != -1, "#E5");
#else
			Assert.AreEqual ("http://local/test.xsl(1,0) :\n", xe.Message, "#E5");
#endif
			Assert.AreSame (sourceUri, xe.SourceUri, "#E6");
			Assert.AreSame (cause, xe.InnerException, "#E7");
			Assert.IsNull (xe.Source, "#E8");
			Assert.IsNull (xe.StackTrace, "#E9");
			Assert.IsNull (xe.TargetSite, "#E10");

			// uri set, line 0, pos 1, innerexception set
			xe = new XsltCompileException (cause, sourceUri, 0, 1);
			Assert.AreEqual (0, xe.LineNumber, "#F1");
			Assert.AreEqual (1, xe.LinePosition, "#F2");
			Assert.IsNotNull (xe.Message, "#F3");
			Assert.IsTrue (xe.Message.Length > 0, "#F4");
#if NET_2_0
			// exact us-english error message
			// Assert.AreEqual ("XSLT compile error.", xe.Message, "#F5-US-ENGLISH");
			Assert.AreEqual (-1, xe.Message.IndexOf("(0,1)"), "#F5");
#else
			Assert.AreEqual ("http://local/test.xsl(0,1) :\n", xe.Message, "#F5");
#endif
			Assert.AreSame (sourceUri, xe.SourceUri, "#F6");
			Assert.AreSame (cause, xe.InnerException, "#F7");
			Assert.IsNull (xe.Source, "#F8");
			Assert.IsNull (xe.StackTrace, "#F9");
			Assert.IsNull (xe.TargetSite, "#F10");

			// uri set, line 1, pos 2, innerexception set
			xe = new XsltCompileException (cause, sourceUri, 1, 2);
			Assert.AreEqual (1, xe.LineNumber, "#G1");
			Assert.AreEqual (2, xe.LinePosition, "#G2");
			Assert.IsNotNull (xe.Message, "#G3");
			Assert.IsTrue (xe.Message.Length > 0, "#G4");
#if NET_2_0
			// exact us-english error message
			// Assert.AreEqual ("XSLT compile error at http://local/test.xsl(1,2). See InnerException for details.", xe.Message, "#G5-US-ENGLISH");
			Assert.IsTrue (xe.Message.IndexOf("http://local/test.xsl(1,2)") != -1, "#G5");
#else
			Assert.AreEqual ("http://local/test.xsl(1,2) :\n", xe.Message, "#G5");
#endif
			Assert.AreSame (sourceUri, xe.SourceUri, "#G5");
			Assert.AreSame (cause, xe.InnerException, "#G6");
			Assert.IsNull (xe.Source, "#G7");
			Assert.IsNull (xe.StackTrace, "#G8");
			Assert.IsNull (xe.TargetSite, "#G9");

			// uri set, line 3, pos 4, innerexception null
			xe = new XsltCompileException ((Exception) null, sourceUri, 3, 4);
			Assert.AreEqual (3, xe.LineNumber, "#H1");
			Assert.AreEqual (4, xe.LinePosition, "#H2");
			Assert.IsNotNull (xe.Message, "#H3");
			Assert.IsTrue (xe.Message.Length > 0, "#H4");
#if NET_2_0
			// exact us-english error message
			// Assert.AreEqual ("XSLT compile error at http://local/test.xsl(3,4). See InnerException for details.", xe.Message, "#H5-US-ENGLISH");
			Assert.IsTrue (xe.Message.IndexOf("http://local/test.xsl(3,4)") != -1, "#H5");
#else
			Assert.AreEqual ("http://local/test.xsl(3,4) :\n", xe.Message, "#H5");
#endif
			Assert.AreSame (sourceUri, xe.SourceUri, "#H6");
			Assert.IsNull (xe.InnerException, "#H7");
			Assert.IsNull (xe.Source, "#H8");
			Assert.IsNull (xe.StackTrace, "#H9");
			Assert.IsNull (xe.TargetSite, "#H10");

			// uri set, line 0, pos 0, innerexception null
			xe = new XsltCompileException ((Exception) null, sourceUri, 0, 0);
			Assert.AreEqual (0, xe.LineNumber, "#I1");
			Assert.AreEqual (0, xe.LinePosition, "#I2");
			Assert.IsNotNull (xe.Message, "#I3");
			Assert.IsTrue (xe.Message.Length > 0, "#I4");
#if NET_2_0
			// exact us-english error message
			// Assert.AreEqual ("XSLT compile error.", xe.Message, "#I5-US-ENGLISH");
			Assert.AreEqual (-1, xe.Message.IndexOf("http://local/test.xsl(0,0) :\n"), "#I5");
#else
			Assert.AreEqual ("http://local/test.xsl(0,0) :\n", xe.Message, "#I5");
#endif
			Assert.AreSame (sourceUri, xe.SourceUri, "#I6");
			Assert.IsNull (xe.InnerException, "#I7");
			Assert.IsNull (xe.Source, "#I8");
			Assert.IsNull (xe.StackTrace, "#I9");
			Assert.IsNull (xe.TargetSite, "#I10");
		}
	}
}
