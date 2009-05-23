//
// Test.Mono.Messaging
//
// Authors:
//      Michael Barker (mike@middlesoft.co.uk)
//
// (C) 2008 Michael Barker
//

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

using System;
using System.Text.RegularExpressions;

using Mono.Messaging;
using NUnit.Framework;

namespace MonoTests.Mono.Messaging
{
	[TestFixture]
	public class QueueReferenceTest
	{
		[Test]
		public void Equals ()
		{
			QueueReference qr1 = new QueueReference ("abc", "def", false);
			QueueReference qr2 = new QueueReference ("abc", "def", false);
			//Assert.IsTrue(qr1.Equals(qr2), "QueueReferences should be equal");
			Assert.AreEqual (qr1, qr2, "QueueReferences should be equal");
		}
		
		[Test]
		public void Parse ()
		{
			string[] s = @".\def\ghi".Split (new char[] { '\\' }, 3);
			Assert.AreEqual (3, s.Length, "Fail");
			Assert.AreEqual (".", s[0], "Fail");
		
			QueueReference qr0 = QueueReference.Parse (@"\\host\private$\myqueue");
			Assert.AreEqual ("host", qr0.Host);
			Assert.AreEqual (true, qr0.IsPrivate);
			Assert.AreEqual (@"private$\myqueue", qr0.Queue);
			
			QueueReference qr1 = QueueReference.Parse (@"\\host\myqueue");
			Assert.AreEqual ("host", qr1.Host);
			Assert.AreEqual (false, qr1.IsPrivate);
			Assert.AreEqual ("myqueue", qr1.Queue);
			
			QueueReference qr2 = QueueReference.Parse ("myqueue");
			Assert.AreEqual ("localhost", qr2.Host);
			Assert.AreEqual (false, qr2.IsPrivate);
			Assert.AreEqual ("myqueue", qr2.Queue);			
		}
		
		[Test]
		public void StringLeadingChars ()
		{
			Assert.AreEqual (@"asdfb\asdfasd", 
					QueueReference.RemoveLeadingSlashes (@"\\asdfb\asdfasd"), 
					"Failed to removed slashes");
		}
	}

	
}

