//
// Test.Mono.Messaging.RabbitMQ
//
// Authors:
//	  Michael Barker (mike@middlesoft.co.uk)
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
using System.Messaging;
using System.Reflection;
using System.Threading;
using System.Text.RegularExpressions;

using NUnit.Framework;

namespace MonoTests.System.Messaging
{
	[TestFixture]
	public class AsyncPeekTest {

		bool eventCalled = false;
		
		private void HandleMessage (object source, PeekCompletedEventArgs args) {
			eventCalled = true;
		}

		[Test]
		public void BeginPeek()
		{
			MessageQueue q = MQUtil.GetQueue ();
			Message s = new Message (new BinaryMessageFormatter ());
			string body = "foo-" + DateTime.Now.ToString ();
			s.Body = body;
			q.Send (s);
			
			q.PeekCompleted += new PeekCompletedEventHandler (HandleMessage);
			IAsyncResult result = q.BeginPeek ();
			result.AsyncWaitHandle.WaitOne ();
			Message rMsg = q.EndPeek (result);
			Assert.AreEqual (body, rMsg.Body, "Async Send Failed, bodies not equal");
			Assert.IsTrue (eventCalled, "Handle Message not called");
			
			Assert.IsNotNull (q.Receive (), "Message not peeked");
		}
		
		[Test]
		public void BeginPeekWithTimeout()
		{
			MessageQueue q = MQUtil.GetQueue ();
			Message s = new Message (new BinaryMessageFormatter ());
			string body = "foo-" + DateTime.Now.ToString ();
			s.Body = body;
			q.Send (s);
			
			IAsyncResult result = q.BeginPeek (new TimeSpan (0, 0, 2));
			result.AsyncWaitHandle.WaitOne ();
			Message rMsg = q.EndPeek (result);
			Assert.AreEqual (body, rMsg.Body, "Async Send Failed, bodies not equal");
			
			Assert.IsNotNull (q.Receive (), "Message not peeked");
		}
		
		[Test]
		public void BeginPeekWithStateAndTimeout()
		{
			MessageQueue q = MQUtil.GetQueue ();
			Message s = new Message (new BinaryMessageFormatter ());
			string body = "foo-" + DateTime.Now.ToString ();
			s.Body = body;
			q.Send (s);
			
			IAsyncResult result = q.BeginPeek (new TimeSpan (0, 0, 2), "foo");
			result.AsyncWaitHandle.WaitOne ();
			Message rMsg = q.EndPeek (result);
			Assert.AreEqual (body, rMsg.Body, "Async Send Failed, bodies not equal");
			Assert.AreEqual ("foo", result.AsyncState, "State not passed properly");
			
			Assert.IsNotNull (q.Receive (), "Message not peeked");
		}
		
		private bool success = false;
		
		public void TestCallback (IAsyncResult result)
		{
			success = true;
		}
		
		[Test]
		public void BeginPeekWithStateAndTimeoutAndCallback()
		{
			MessageQueue q = MQUtil.GetQueue ();
			Message s = new Message (new BinaryMessageFormatter ());
			string body = "foo-" + DateTime.Now.ToString ();
			s.Body = body;
			q.Send (s);
			AsyncCallback ac = new AsyncCallback (TestCallback);
			IAsyncResult result = q.BeginPeek (new TimeSpan (0, 0, 2), "foo", ac);
			result.AsyncWaitHandle.WaitOne ();
			Message rMsg = q.EndPeek (result);
			Assert.AreEqual (body, rMsg.Body, "Async Send Failed, bodies not equal");
			Assert.AreEqual ("foo", result.AsyncState, "State not passed properly");
			Assert.IsTrue (success, "Callback not run");
			
			Assert.IsNotNull (q.Receive (), "Message not peeked");
		}
		
		[Test]
		[ExpectedException (typeof (MessageQueueException))]
		public void BeginPeekWithException()
		{
			MessageQueue q = MQUtil.GetQueue ();
			IAsyncResult result = q.BeginPeek (new TimeSpan (0, 0, 2));
			result.AsyncWaitHandle.WaitOne ();
			q.EndPeek (result);
		}		
	}
}
