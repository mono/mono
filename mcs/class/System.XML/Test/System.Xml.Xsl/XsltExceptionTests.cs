//
// XsltExceptionTests.cs - Unit tests for System.Xml.Xsl.XsltException
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
using System.Xml.Xsl;

namespace MonoCasTests.System.Xml.Xsl {
	[TestFixture]
	public class XsltExceptionTests
	{
#if NET_2_0
		[Test]
		public void Constructor0 ()
		{
			XsltException xsltException = new XsltException ();
			Assert.AreEqual (0, xsltException.LineNumber, "#1");
			Assert.AreEqual (0, xsltException.LinePosition, "#2");
			Assert.AreEqual (string.Empty, xsltException.Message, "#3");
			Assert.IsNull (xsltException.SourceUri, "#4");
			Assert.IsNull (xsltException.InnerException, "#5");
			Assert.IsNull (xsltException.Source, "#6");
#if !TARGET_JVM
			Assert.IsNull (xsltException.StackTrace, "#7");
			Assert.IsNull (xsltException.TargetSite, "#8");
#endif
		}

		[Test]
		public void Constructor1 ()
		{
			string msg = "mono";

			XsltException xsltException = new XsltException (msg);
			Assert.AreEqual (0, xsltException.LineNumber, "#1");
			Assert.AreEqual (0, xsltException.LinePosition, "#2");
			Assert.AreEqual (msg, xsltException.Message, "#3");
			Assert.IsNull (xsltException.SourceUri, "#4");
			Assert.IsNull (xsltException.InnerException, "#5");
			Assert.IsNull (xsltException.Source, "#6");
#if !TARGET_JVM
			Assert.IsNull (xsltException.StackTrace, "#7");
			Assert.IsNull (xsltException.TargetSite, "#8");
#endif
		}
#endif

		[Test]
		public void Constructor2 ()
		{
			string msg = "mono";
			Exception cause = new ApplicationException ("cause");

			XsltException xsltException = new XsltException (msg, cause);
			Assert.AreEqual (0, xsltException.LineNumber, "#A1");
			Assert.AreEqual (0, xsltException.LinePosition, "#A2");
			Assert.AreEqual (msg, xsltException.Message, "#A3");
			Assert.IsNull (xsltException.SourceUri, "#A4");
			Assert.AreSame (cause, xsltException.InnerException, "#A5");
			Assert.IsNull (xsltException.Source, "#A6");
#if !TARGET_JVM
			Assert.IsNull (xsltException.StackTrace, "#A7");
			Assert.IsNull (xsltException.TargetSite, "#A8");
#endif
			xsltException = new XsltException ((string) null, cause);
			Assert.AreEqual (0, xsltException.LineNumber, "#B1");
			Assert.AreEqual (0, xsltException.LinePosition, "#B2");
			Assert.IsNotNull (xsltException.Message, "#B3");
			Assert.AreEqual (string.Empty, xsltException.Message, "#B4");
			Assert.IsNull (xsltException.SourceUri, "#B5");
			Assert.AreSame (cause, xsltException.InnerException, "#B6");
			Assert.IsNull (xsltException.Source, "#B7");
#if !TARGET_JVM
			Assert.IsNull (xsltException.StackTrace, "#B8");
			Assert.IsNull (xsltException.TargetSite, "#B9");
#endif
			xsltException = new XsltException (msg, (Exception) null);
			Assert.AreEqual (0, xsltException.LineNumber, "#C1");
			Assert.AreEqual (0, xsltException.LinePosition, "#C2");
			Assert.AreEqual (msg, xsltException.Message, "#C3");
			Assert.IsNull (xsltException.SourceUri, "#C4");
			Assert.IsNull (xsltException.InnerException, "#C5");
			Assert.IsNull (xsltException.Source, "#C6");
#if !TARGET_JVM
			Assert.IsNull (xsltException.StackTrace, "#C7");
			Assert.IsNull (xsltException.TargetSite, "#C8");
#endif
			xsltException = new XsltException ((string) null, (Exception) null);
			Assert.AreEqual (0, xsltException.LineNumber, "#D1");
			Assert.AreEqual (0, xsltException.LinePosition, "#D2");
			Assert.AreEqual (string.Empty, xsltException.Message, "#D3");
			Assert.IsNull (xsltException.SourceUri, "#D4");
			Assert.IsNull (xsltException.InnerException, "#D5");
			Assert.IsNull (xsltException.Source, "#D6");
#if !TARGET_JVM
			Assert.IsNull (xsltException.StackTrace, "#D7");
			Assert.IsNull (xsltException.TargetSite, "#D8");
#endif
		}
	}
}
