//
// BuildEventArgsTest.cs:
//
// Author:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
//
// (C) 2006 Marek Sieradzki
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
using System.Threading;
using Microsoft.Build.Framework;
using NUnit.Framework;

namespace MonoTests.Microsoft.Build.Framework {

	class TestClass : BuildEventArgs {
		public TestClass (string message, string helpKeyword, string senderName)
			: base (message, helpKeyword, senderName)
		{
		}
	}
	
	[TestFixture]
	public class BuildEventArgsTest {
		[Test]
		public void AssignmentTest ()
		{
			DateTime before, after;
			
			string message = "message";
			string helpKeyword = "helpKeyword";
			string senderName = "senderName";
			
			before = DateTime.Now;
			
			TestClass tc = new TestClass (message, helpKeyword, senderName);
			
			after = DateTime.Now;
			
			Assert.AreEqual (message, tc.Message, "A1");
			Assert.AreEqual (helpKeyword, tc.HelpKeyword, "A2");
			Assert.AreEqual (senderName, tc.SenderName, "A3");
			Assert.AreEqual (Thread.CurrentThread.GetHashCode (), tc.ThreadId, "A4");
			Assert.IsTrue (before <= tc.Timestamp, "A5");
			Assert.IsTrue (after >= tc.Timestamp, "A6");
		}
	}
}
