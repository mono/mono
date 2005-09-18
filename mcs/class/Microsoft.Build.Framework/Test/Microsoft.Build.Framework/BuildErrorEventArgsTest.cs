//
// AnyEventHandlerTest.cs:
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

using Microsoft.Build.Framework;
using NUnit.Framework;

namespace MonoTests.Microsoft.Build.Framework {
	[TestFixture]
	public class BuildErrorEventArgsTest {
		[Test]
		public void AssignmentTest ()
		{
			BuildErrorEventArgs beea;
			string subcategory = "subcategory";
			string code = "CS0000";
			string file = "file";
			int lineNumber = 1;
			int columnNumber = 2;
			int endLineNumber = 3;
			int endColumnNumber = 4;
			string message = "message";
			string helpKeyword = "helpKeyword";
			string senderName = "MSBuild";
			
			beea = new BuildErrorEventArgs (subcategory, code, file, lineNumber, columnNumber, endLineNumber,
				endColumnNumber, message, helpKeyword, senderName);
			
			Assert.AreEqual (subcategory, beea.Subcategory, "Subcategory");
			Assert.AreEqual (code, beea.Code, "Code");
			Assert.AreEqual (file, beea.File, "File");
			Assert.AreEqual (lineNumber, beea.LineNumber, "LineNumber");
			Assert.AreEqual (columnNumber, beea.ColumnNumber, "ColumnNumber");
			Assert.AreEqual (endLineNumber, beea.EndLineNumber, "EndLineNumber");
			Assert.AreEqual (endColumnNumber, beea.EndColumnNumber, "EndColumnNumber");
			Assert.AreEqual (message, beea.Message, "Message");
			Assert.AreEqual (helpKeyword, beea.HelpKeyword, "HelpKeyword");
			Assert.AreEqual (senderName, beea.SenderName, "SenderName");
		}
	}
}