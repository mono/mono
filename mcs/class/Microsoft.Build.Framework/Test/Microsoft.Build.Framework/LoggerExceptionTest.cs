//
// LoggerExceptionTest.cs:
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
using System.Runtime.Serialization;
using Microsoft.Build.Framework;
using NUnit.Framework;

namespace MonoTests.Microsoft.Build.Framework {
	[TestFixture]
	public class LoggerExceptionTest {
		[Test]
		public void CtorMessageTest ()
		{
			LoggerException le;
			string message = "message";	
		
			le = new LoggerException (message);
			
			Assert.AreEqual (message, le.Message, "Message");
			Assert.AreEqual (null, le.ErrorCode, "ErrorCode");
			Assert.AreEqual (null, le.HelpKeyword, "HelpKeyword");
		}
		
		[Test]
		public void CtorMessageExceptionTest ()
		{
			LoggerException le;
			string message = "message";
			Exception e = new Exception ("Inner exception message");	
		
			le = new LoggerException (message, e);
			
			Assert.AreEqual (message, le.Message, "Message");
			Assert.AreEqual (e, le.InnerException, "InnerException");
			Assert.AreEqual (null, le.ErrorCode, "ErrorCode");
			Assert.AreEqual (null, le.HelpKeyword, "HelpKeyword");
		}
		
		[Test]
		public void CtorMessageExceptionCodeTest ()
		{
			LoggerException le;
			string message = "message";
			string errorCode = "CS0000";
			string helpKeyword = "helpKeyword";
			Exception e = new Exception ("Inner exception message");	
		
			le = new LoggerException (message, e, errorCode, helpKeyword);
			
			Assert.AreEqual (message, le.Message, "Message");
			Assert.AreEqual (e, le.InnerException, "InnerException");
			Assert.AreEqual (errorCode, le.ErrorCode, "ErrorCode");
			Assert.AreEqual (helpKeyword, le.HelpKeyword, "HelpKeyword");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestGetObjectData1 ()
		{
			LoggerException le = new LoggerException ();
			le.GetObjectData (null, new StreamingContext ());
		}

		[Test]
		public void TestGetObjectData2 ()
		{
			StreamingContext sc = new StreamingContext ();
			SerializationInfo si = new SerializationInfo (typeof (LoggerException), new FormatterConverter ());

			LoggerException le;
			string message = "message";
			string errorCode = "CS0000";
			string helpKeyword = "helpKeyword";
			Exception e = new Exception ("Inner exception message");

			le = new LoggerException (message, e, errorCode, helpKeyword);
			le.GetObjectData (si, sc);

			Assert.AreEqual (errorCode, si.GetString ("errorCode"), "A1");
			Assert.AreEqual (helpKeyword, si.GetString ("helpKeyword"), "A2");
			
		}
	}
}
