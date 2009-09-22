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
	public class AsyncReceiveTest {

		private Message m;
		private string failureMessage = null;
		private string state = null;
		
		private void HandleMessage (object source, ReceiveCompletedEventArgs args) {
			try {
				MessageQueue q = (MessageQueue) source;
				m = q.EndReceive (args.AsyncResult);
				state = (string) args.AsyncResult.AsyncState;
			} catch (Exception e) {
				failureMessage = e.Message;
			}
		}

		[Test]
		public void BeginReceive()
		{
			MessageQueue q = MQUtil.GetQueue ();
			Message s = new Message (new BinaryMessageFormatter ());
			string body = "foo-" + DateTime.Now.ToString ();
			s.Body = body;
			q.Send (s);
			
			q.ReceiveCompleted += new ReceiveCompletedEventHandler (HandleMessage);
			IAsyncResult result = q.BeginReceive ();
			result.AsyncWaitHandle.WaitOne ();
			Message rMsg = q.EndReceive (result);
			Assert.IsNotNull (rMsg, "No message received");
			Assert.AreEqual (body, rMsg.Body, "Async Send Failed, bodies not equal");
		}
		
		[Test]
		public void BeginReceiveWithTimeout()
		{
			MessageQueue q = MQUtil.GetQueue ();
			Message s = new Message (new BinaryMessageFormatter ());
			string body = "foo-" + DateTime.Now.ToString ();
			s.Body = body;
			q.Send (s);
			
			IAsyncResult result = q.BeginReceive (new TimeSpan (0, 0, 2));
			result.AsyncWaitHandle.WaitOne ();
			Message rMsg = q.EndReceive (result);
			Assert.AreEqual (body, rMsg.Body, "Async Send Failed, bodies not equal");
		}
		
		[Test]
		public void BeginReceiveWithStateAndTimeout()
		{
			MessageQueue q = MQUtil.GetQueue ();
			Message s = new Message (new BinaryMessageFormatter ());
			string body = "foo-" + DateTime.Now.ToString ();
			s.Body = body;
			q.Send (s);
			
			IAsyncResult result = q.BeginReceive (new TimeSpan (0, 0, 2), "foo");
			result.AsyncWaitHandle.WaitOne ();
			Message rMsg = q.EndReceive (result);
			Assert.AreEqual (body, rMsg.Body, "Async Send Failed, bodies not equal");
			Assert.AreEqual ("foo", result.AsyncState, "State not passed properly");
		}
		
		private bool success = false;
		
		public void TestCallback (IAsyncResult result)
		{
			success = true;
		}
		
		[Test]
		public void BeginReceiveWithStateAndTimeoutAndCallback()
		{
			MessageQueue q = MQUtil.GetQueue ();
			Message s = new Message (new BinaryMessageFormatter ());
			string body = "foo-" + DateTime.Now.ToString ();
			s.Body = body;
			q.Send (s);
			AsyncCallback ac = new AsyncCallback (TestCallback);
			IAsyncResult result = q.BeginReceive (new TimeSpan (0, 0, 2), "foo", ac);
			result.AsyncWaitHandle.WaitOne ();
			Message rMsg = q.EndReceive (result);
			Assert.AreEqual (body, rMsg.Body, "Async Send Failed, bodies not equal");
			Assert.AreEqual ("foo", result.AsyncState, "State not passed properly");
			Assert.IsTrue (success, "Callback not run");
		}
		
		[Test]
		[ExpectedException (typeof (MessageQueueException))]
		public void BeginReceiveWithException()
		{
			MessageQueue q = MQUtil.GetQueue ();
			IAsyncResult result = q.BeginReceive (new TimeSpan (0, 0, 2));
			result.AsyncWaitHandle.WaitOne ();
			q.EndReceive (result);
		}
	}
}
