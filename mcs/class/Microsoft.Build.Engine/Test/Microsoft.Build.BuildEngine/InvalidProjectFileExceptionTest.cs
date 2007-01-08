//
// InvalidProjectFileExceptionTest.cs:
//
// Author:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
// 
// (C) 2005 Marek Sieradzki
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

using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml;
using Microsoft.Build.BuildEngine;
using NUnit.Framework;

namespace MonoTests.Microsoft.Build.BuildEngine {
	[TestFixture]
	public class InvalidProjectFileExceptionTest {
		[Test]
		public void TestCtorMessage ()
		{
			InvalidProjectFileException ipfe;
			string message = "message";
			
			ipfe = new InvalidProjectFileException (message);
			
			Assert.AreEqual (message, ipfe.Message, "Message");
		}
		
		[Test]
		public void TestCtorProjectFile ()
		{
			InvalidProjectFileException ipfe;
			string projectFile = "projectFile";
			int lineNumber = 1;
			int columnNumber = 2;
			int endLineNumber = 3;
			int endColumnNumber = 4;
			string message = "message";
			string errorSubcategory = "errorSubcategory";
			string errorCode = "CS0000";
			string helpKeyword = "helpKeyword";
			
			ipfe = new InvalidProjectFileException (projectFile, lineNumber, columnNumber, endLineNumber, endColumnNumber,
				message, errorSubcategory, errorCode, helpKeyword);
			
			Assert.AreEqual (projectFile, ipfe.ProjectFile, "A1");
			Assert.AreEqual (lineNumber, ipfe.LineNumber, "A2");
			Assert.AreEqual (columnNumber, ipfe.ColumnNumber, "A3");
			Assert.AreEqual (endLineNumber, ipfe.EndLineNumber, "A4");
			Assert.AreEqual (endColumnNumber, ipfe.EndColumnNumber, "A5");
			Assert.AreEqual (message, ipfe.BaseMessage, "A6");
			Assert.AreEqual (message + "  " + projectFile, ipfe.Message, "A7");
			Assert.AreEqual (errorSubcategory, ipfe.ErrorSubcategory, "A8");
			Assert.AreEqual (errorCode, ipfe.ErrorCode, "A9");
			Assert.AreEqual (helpKeyword, ipfe.HelpKeyword, "A10");
		}
		
		[Test]
		public void TestCtorMessageException ()
		{
			string message = "message";
			Exception e = new Exception ("Exception message");
			
			new InvalidProjectFileException (message, e);
		}
		
		[Test]
		public void TestCtorNode ()
		{
			// FIXME: we need to load xml file to load something with non-empty XmlElement.BaseUri
			/*
			XmlDocument xd = new XmlDocument ();
			
			InvalidProjectFileException ipfe;

			string message = "message";
			string errorSubcategory = "errorSubcategory";
			string errorCode = "CS0000";
			string helpKeyword = "helpKeyword";
			
			ipfe = new InvalidProjectFileException (null, message, errorSubcategory, errorCode, helpKeyword);
			
			Assert.AreEqual (message, ipfe.BaseMessage, "A1");
			Assert.AreEqual (errorSubcategory, ipfe.ErrorSubcategory, "A2");
			Assert.AreEqual (errorCode, ipfe.ErrorCode, "A3");
			Assert.AreEqual (helpKeyword, ipfe.HelpKeyword, "A4");
			*/
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestGetObjectData1 ()
		{
			InvalidProjectFileException ipfe = new InvalidProjectFileException ();
			ipfe.GetObjectData (null, new StreamingContext ());
		}

		[Test]
		public void TestGetObjectData2 ()
		{
			StreamingContext sc = new StreamingContext ();
			SerializationInfo si = new SerializationInfo (typeof (InvalidProjectFileException), new FormatterConverter ());
			InvalidProjectFileException ipfe;
			string projectFile = "projectFile";
			int lineNumber = 1;
			int columnNumber = 2;
			int endLineNumber = 3;
			int endColumnNumber = 4;
			string message = "message";
			string errorSubcategory = "errorSubcategory";
			string errorCode = "CS0000";
			string helpKeyword = "helpKeyword";

			ipfe = new InvalidProjectFileException (projectFile, lineNumber, columnNumber, endLineNumber, endColumnNumber,
				message, errorSubcategory, errorCode, helpKeyword);
			ipfe.GetObjectData (si, sc);

			Assert.AreEqual (projectFile, si.GetString ("projectFile"), "A1");
			Assert.AreEqual (lineNumber, si.GetInt32 ("lineNumber"), "A2");
			Assert.AreEqual (columnNumber, si.GetInt32 ("columnNumber"), "A3");
			Assert.AreEqual (endLineNumber, si.GetInt32 ("endLineNumber"), "A4");
			Assert.AreEqual (endColumnNumber, si.GetInt32 ("endColumnNumber"), "A5");
			Assert.AreEqual (message, si.GetString ("Message"), "A6");
			Assert.AreEqual (errorSubcategory, si.GetString ("errorSubcategory"), "A7");
			Assert.AreEqual (errorCode, si.GetString ("errorCode"), "A8");
			Assert.AreEqual (helpKeyword, si.GetString ("helpKeyword"), "A9");
		}
	}
}
