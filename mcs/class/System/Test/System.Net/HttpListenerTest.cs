//
// HttpListenerTest.cs
//	- Unit tests for System.Net.HttpListener
//
// Author:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
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
#if NET_2_0
using System;
using System.Net;
using System.Threading;
using NUnit.Framework;

namespace MonoTests.System.Net {
	[TestFixture]
#if TARGET_JVM
	[Ignore ("The class HttpListener is not implemented")]
#endif
	public class HttpListenerTest {
#if !TARGET_JVM
		[Test]
		public void DefaultProperties ()
		{
			HttpListener listener = new HttpListener ();
			Assert.AreEqual (AuthenticationSchemes.Anonymous, listener.AuthenticationSchemes, "#01");
			Assert.AreEqual (null, listener.AuthenticationSchemeSelectorDelegate, "#02");
			Assert.AreEqual (false, listener.IgnoreWriteExceptions, "#03");
			Assert.AreEqual (false, listener.IsListening, "#03");
			Assert.AreEqual (0, listener.Prefixes.Count, "#04");
			Assert.AreEqual (null, listener.Realm, "#05");
			Assert.AreEqual (false, listener.UnsafeConnectionNtlmAuthentication, "#06");
		}

		[Test]
		public void Start1 ()
		{
			HttpListener listener = new HttpListener ();
			listener.Start ();
		}

		[Test]
		public void Stop1 ()
		{
			HttpListener listener = new HttpListener ();
			listener.Stop ();
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void GetContext1 ()
		{
			HttpListener listener = new HttpListener ();
			// "Please call Start () before calling this method"
			listener.GetContext ();
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void GetContext2 ()
		{
			HttpListener listener = new HttpListener ();
			listener.Start ();
			// "Please call AddPrefix () before calling this method"
			listener.GetContext ();
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void BeginGetContext1 ()
		{
			HttpListener listener = new HttpListener ();
			// "Please call Start () before calling this method"
			listener.BeginGetContext (null, null);
		}

		[Test]
		public void BeginGetContext2 ()
		{
			HttpListener listener = new HttpListener ();
			listener.Start ();
			// One would expect this to fail as BeginGetContext1 does not fail and
			// calling EndGetContext will wait forever.
			// Lame. They should check that we have no prefixes.
			IAsyncResult ares = listener.BeginGetContext (null, null);
			Assert.IsFalse (ares.IsCompleted);
		}

		[Test]
		public void TwoListeners_SameAddress ()
		{
			HttpListener listener1 = new HttpListener ();
			listener1.Prefixes.Add ("http://127.0.0.1:7777/");
			HttpListener listener2 = new HttpListener ();
			listener2.Prefixes.Add ("http://127.0.0.1:7777/hola/");
			listener1.Start ();
			listener2.Start ();
		}

		[Test]
		[ExpectedException (typeof (HttpListenerException))]
		public void TwoListeners_SameURL ()
		{
			HttpListener listener1 = new HttpListener ();
			listener1.Prefixes.Add ("http://127.0.0.1:7777/hola/");
			HttpListener listener2 = new HttpListener ();
			listener2.Prefixes.Add ("http://127.0.0.1:7777/hola/");
			listener1.Start ();
			listener2.Start ();
		}

		[Test]
		[ExpectedException (typeof (HttpListenerException))]
		public void MultipleSlashes ()
		{
			HttpListener listener = new HttpListener ();
			listener.Prefixes.Add ("http://localhost:7777/hola////");
			// this one throws on Start(), not when adding it.
			listener.Start ();
		}

		[Test]
		[ExpectedException (typeof (HttpListenerException))]
		public void PercentSign ()
		{
			HttpListener listener = new HttpListener ();
			listener.Prefixes.Add ("http://localhost:7777/hola%3E/");
			// this one throws on Start(), not when adding it.
			listener.Start ();
		}

		[Test]
		public void CloseBeforeStart ()
		{
			HttpListener listener = new HttpListener ();
			listener.Close ();
		}

		[Test]
		public void CloseTwice ()
		{
			HttpListener listener = new HttpListener ();
			listener.Prefixes.Add ("http://localhost:7777/hola/");
			listener.Start ();
			listener.Close ();
			listener.Close ();
		}

		[Test]
		public void StartStopStart ()
		{
			HttpListener listener = new HttpListener ();
			listener.Prefixes.Add ("http://localhost:7777/hola/");
			listener.Start ();
			listener.Stop ();
			listener.Start ();
			listener.Close ();
		}

		[Test]
		public void StartStopDispose ()
		{
			using (HttpListener listener = new HttpListener ()){
				listener.Prefixes.Add ("http://localhost:7777/hola/");
				listener.Start ();
				listener.Stop ();
			}
		}
		
		[Test]
		public void AbortBeforeStart ()
		{
			HttpListener listener = new HttpListener ();
			listener.Abort ();
		}

		[Test]
		public void AbortTwice ()
		{
			HttpListener listener = new HttpListener ();
			listener.Prefixes.Add ("http://localhost:7777/hola/");
			listener.Start ();
			listener.Abort ();
			listener.Abort ();
		}

		[Test]
		public void PropertiesWhenClosed1 ()
		{
			HttpListener listener = new HttpListener ();
			listener.Close ();
			Assert.AreEqual (AuthenticationSchemes.Anonymous, listener.AuthenticationSchemes, "#01");
			Assert.AreEqual (null, listener.AuthenticationSchemeSelectorDelegate, "#02");
			Assert.AreEqual (false, listener.IgnoreWriteExceptions, "#03");
			Assert.AreEqual (false, listener.IsListening, "#03");
			Assert.AreEqual (null, listener.Realm, "#05");
			Assert.AreEqual (false, listener.UnsafeConnectionNtlmAuthentication, "#06");
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void PropertiesWhenClosed2 ()
		{
			HttpListener listener = new HttpListener ();
			listener.Close ();
			HttpListenerPrefixCollection p = listener.Prefixes;
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void PropertiesWhenClosedSet1 ()
		{
			HttpListener listener = new HttpListener ();
			listener.Close ();
			listener.AuthenticationSchemes = AuthenticationSchemes.None;
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void PropertiesWhenClosedSet2 ()
		{
			HttpListener listener = new HttpListener ();
			listener.Close ();
			listener.AuthenticationSchemeSelectorDelegate = null;
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void PropertiesWhenClosedSet3 ()
		{
			HttpListener listener = new HttpListener ();
			listener.Close ();
			listener.IgnoreWriteExceptions = true;
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void PropertiesWhenClosedSet4 ()
		{
			HttpListener listener = new HttpListener ();
			listener.Close ();
			listener.Realm = "hola";
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void PropertiesWhenClosedSet5 ()
		{
			HttpListener listener = new HttpListener ();
			listener.Close ();
			listener.UnsafeConnectionNtlmAuthentication = true;
		}

		[Test]
		public void PropertiesWhenClosed3 ()
		{
			HttpListener listener = new HttpListener ();
			listener.Close ();
			Assert.IsFalse (listener.IsListening);
		}

		[Test]
		public void CloseWhileBegin ()
		{
			HttpListener listener = new HttpListener ();
			listener.Prefixes.Add ("http://127.0.0.1:9001/closewhilebegin/");
			listener.Start ();
			CallMe cm = new CallMe ();
			listener.BeginGetContext (cm.Callback, listener);
			listener.Close ();
			if (false == cm.Event.WaitOne (3000, false))
				Assert.Fail ("This should not time out.");
			Assert.IsNotNull (cm.Error);
			Assert.AreEqual (typeof (ObjectDisposedException), cm.Error.GetType (), "Exception type");
			cm.Dispose ();
		}

		[Test]
		public void AbortWhileBegin ()
		{
			HttpListener listener = new HttpListener ();
			listener.Prefixes.Add ("http://127.0.0.1:9001/abortwhilebegin/");
			listener.Start ();
			CallMe cm = new CallMe ();
			listener.BeginGetContext (cm.Callback, listener);
			listener.Abort ();
			if (false == cm.Event.WaitOne (3000, false))
				Assert.Fail ("This should not time out.");
			Assert.IsNotNull (cm.Error);
			Assert.AreEqual (typeof (ObjectDisposedException), cm.Error.GetType (), "Exception type");
			cm.Dispose ();
		}

		[Test]
		[ExpectedException (typeof (HttpListenerException))]
		public void CloseWhileGet ()
		{
			// "System.Net.HttpListener Exception : The I/O operation has been aborted
			// because of either a thread exit or an application request
			//   at System.Net.HttpListener.GetContext()
			//   at MonoTests.System.Net.HttpListenerTest.CloseWhileGet()

			HttpListener listener = new HttpListener ();
			listener.Prefixes.Add ("http://127.0.0.1:9001/closewhileget/");
			listener.Start ();
			RunMe rm = new RunMe (1000, new ThreadStart (listener.Close), new object [0]);
			rm.Start ();
			HttpListenerContext ctx = listener.GetContext ();
		}

		[Test]
		[ExpectedException (typeof (HttpListenerException))]
		public void AbortWhileGet ()
		{
			// "System.Net.HttpListener Exception : The I/O operation has been aborted
			// because of either a thread exit or an application request
			//   at System.Net.HttpListener.GetContext()
			//   at MonoTests.System.Net.HttpListenerTest.CloseWhileGet()

			HttpListener listener = new HttpListener ();
			listener.Prefixes.Add ("http://127.0.0.1:9001/abortwhileget/");
			listener.Start ();
			RunMe rm = new RunMe (1000, new ThreadStart (listener.Abort), new object [0]);
			rm.Start ();
			HttpListenerContext ctx = listener.GetContext ();
		}

		class RunMe {
			Delegate d;
			int delay_ms;
			object [] args;
			public object Result;

			public RunMe (int delay_ms, Delegate d, object [] args)
			{
				this.delay_ms = delay_ms;
				this.d = d;
				this.args = args;
			}

			public void Start ()
			{
				Thread th = new Thread (new ThreadStart (Run));
				th.Start ();
			}

			void Run ()
			{
				Thread.Sleep (delay_ms);
				Result = d.DynamicInvoke (args);
			}
		}

		class CallMe {
			public ManualResetEvent Event = new ManualResetEvent (false);
			public bool Called;
			public HttpListenerContext Context;
			public Exception Error;

			public void Reset ()
			{
				Called = false;
				Context = null;
				Error = null;
				Event.Reset ();
			}

			public void Callback (IAsyncResult ares)
			{
				Called = true;
				if (ares == null) {
					Error = new ArgumentNullException ("ares");
					return;
				}
				
				try {
					HttpListener listener = (HttpListener) ares.AsyncState;
					Context = listener.EndGetContext (ares);
				} catch (Exception e) {
					Error = e;
				}
				Event.Set ();
			}

			public void Dispose ()
			{
				Event.Close ();
			}
		}
#endif
	}
}
#endif

