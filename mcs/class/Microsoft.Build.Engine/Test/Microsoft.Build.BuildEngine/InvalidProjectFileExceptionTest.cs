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
			
			Assert.AreEqual (projectFile, ipfe.ProjectFile, "ProjectFile");
			Assert.AreEqual (lineNumber, ipfe.LineNumber, "LineNumber");
			Assert.AreEqual (columnNumber, ipfe.ColumnNumber, "ColumnNumber");
			Assert.AreEqual (endLineNumber, ipfe.EndLineNumber, "EndLineNumber");
			Assert.AreEqual (endColumnNumber, ipfe.EndColumnNumber, "EndColumnNumber");
			Assert.AreEqual (message, ipfe.Message, "Message");
			Assert.AreEqual (errorSubcategory, ipfe.ErrorSubcategory, "ErrorSubcategory");
			Assert.AreEqual (errorCode, ipfe.ErrorCode, "ErrorCode");
			Assert.AreEqual (helpKeyword, ipfe.HelpKeyword, "HelpKeyword");
		}
		
		[Test]
		public void TestCtorMessageException ()
		{
			InvalidProjectFileException ipfe;
			string message = "message";
			Exception e = new Exception ("Exception message");
			
			ipfe = new InvalidProjectFileException (message, e);
		}
		
		[Test]
		public void TestCtorNode ()
		{
			/*
			XmlDocument xd = new XmlDocument ();
			
			InvalidProjectFileException ipfe;

			string message = "message";
			string errorSubcategory = "errorSubcategory";
			string errorCode = "CS0000";
			string helpKeyword = "helpKeyword";
			
			ipfe = new InvalidProjectFileException (null, message, errorSubcategory, errorCode, helpKeyword);
			
			Assert.AreEqual (message, ipfe.Message, "Message");
			Assert.AreEqual (errorSubcategory, ipfe.ErrorSubcategory, "ErrorSubcategory");
			Assert.AreEqual (errorCode, ipfe.ErrorCode, "ErrorCode");
			Assert.AreEqual (helpKeyword, ipfe.HelpKeyword, "HelpKeyword");
			*/
		}
	}
}
