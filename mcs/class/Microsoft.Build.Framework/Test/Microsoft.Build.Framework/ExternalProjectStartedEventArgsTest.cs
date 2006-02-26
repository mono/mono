//
// ExternalProjectStartedEventArgsTest.cs
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
	public class ExternalProjectStartedEventArgsTest {
		[Test]
		public void AssignmentTest ()
		{
			ExternalProjectStartedEventArgs epsea;
			string message = "message";
			string helpKeyword = "helpKeyword";
			string senderName = "senderName";
			string projectFile = "projectFile";
			string targetNames = "a;b;c";
			
			epsea = new ExternalProjectStartedEventArgs (message, helpKeyword, senderName, projectFile, targetNames);
			
			Assert.AreEqual (message, epsea.Message, "Message");
			Assert.AreEqual (helpKeyword, epsea.HelpKeyword, "HelpKeyword");
			Assert.AreEqual (senderName, epsea.SenderName, "SenderName");
			Assert.AreEqual (projectFile, epsea.ProjectFile, "ProjectFile");
			Assert.AreEqual (targetNames, epsea.TargetNames, "TargetNames");
		}
	}
}
