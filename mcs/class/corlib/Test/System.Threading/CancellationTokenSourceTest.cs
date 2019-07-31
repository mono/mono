//
// CancellationTokenSourceTest.cs
//
// Authors:
//       Marek Safar (marek.safar@gmail.com)
//       Jeremie Laval (jeremie.laval@gmail.com)
//
// Copyright 2011 Xamarin, Inc (http://www.xamarin.com)
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
using System.Threading;
using NUnit.Framework;
using System.Threading.Tasks;

namespace MonoTests.System.Threading
{
	[TestFixture]
	public class CancellationTokenSourceTest
	{

		[Test]
		public void Ctor_Invalid ()
		{
			try {
				new CancellationTokenSource (-4);
				Assert.Fail ("#1");
			} catch (ArgumentException) {
			}
		}

		[Test]
		[Category ("MultiThreaded")]
		public void Ctor_Timeout ()
		{
			int called = 0;
			var cts = new CancellationTokenSource (TimeSpan.FromMilliseconds (20));
			var mre = new ManualResetEvent (false);
			cts.Token.Register (() => { called++; mre.Set (); });

			Assert.IsTrue (mre.WaitOne (1000), "Not called in 1000ms");
			Assert.AreEqual (1, called, "#1");
		}

		[Test]
		[Category ("MultiThreaded")]
		public void CancelAfter ()
		{
			int called = 0;
			var cts = new CancellationTokenSource ();
			var mre = new ManualResetEvent(false);
			cts.Token.Register (() => { called++; mre.Set (); });
			cts.CancelAfter (20);

			Assert.IsTrue(mre.WaitOne (1000), "Should be cancelled in ~20ms");
			Assert.AreEqual (1, called, "#1");
		}

		[Test]
		public void CancelAfter_Invalid ()
		{
			var cts = new CancellationTokenSource ();
			try {
				cts.CancelAfter (-9);
				Assert.Fail ("#1");
			} catch (ArgumentException) {
			}
		}

		[Test]
		public void CancelAfter_Disposed ()
		{
			int called = 0;
			var cts = new CancellationTokenSource ();
			var mre = new ManualResetEvent (false);
			cts.Token.Register (() => { called++; mre.Set (); });
			cts.CancelAfter (50);
			cts.Dispose ();

			Assert.IsFalse (mre.WaitOne (100), "Shouldn't have been called");
			Assert.AreEqual (0, called, "#1");
		}


		[Test]
		public void Token ()
		{
			CancellationTokenSource cts = new CancellationTokenSource ();
			Assert.IsTrue (cts.Token.CanBeCanceled, "#1");
			Assert.IsFalse (cts.Token.IsCancellationRequested, "#2");
			Assert.IsNotNull (cts.Token.WaitHandle, "#3");
		}

		[Test]
		public void Cancel_NoRegistration ()
		{
			CancellationTokenSource cts = new CancellationTokenSource ();
			cts.Cancel ();
		}

		[Test]
		public void Cancel ()
		{
			var cts = new CancellationTokenSource ();

			int called = 0;
			cts.Token.Register (l => { Assert.AreEqual ("v", l); ++called; }, "v");
			cts.Cancel ();
			Assert.AreEqual (1, called, "#1");

			called = 0;
			cts.Token.Register (() => { called += 12; });
			cts.Cancel ();
			Assert.AreEqual (12, called, "#2");
		}


		[Test]
		public void Cancel_Order ()
		{
			var cts = new CancellationTokenSource ();
			var current = 0;
			Action<object> a = x => { Assert.AreEqual(current, x); current++; };

			cts.Token.Register (a, 2);
			cts.Token.Register (a, 1);
			cts.Token.Register (a, 0);
			cts.Cancel ();
		}


		[Test]
		public void CancelWithDispose ()
		{
			CancellationTokenSource cts = new CancellationTokenSource ();
			CancellationToken c = cts.Token;
			c.Register (() => {
				cts.Dispose ();
			});

			int called = 0;
			c.Register (() => {
				called++;
			});

			cts.Cancel ();
			Assert.AreEqual (1, called, "#1");
		}

		[Test]
		public void Cancel_SingleException ()
		{
			var cts = new CancellationTokenSource ();

			cts.Token.Register (() => { throw new ApplicationException (); });
			try {
				cts.Cancel ();
				Assert.Fail ("#1");
			} catch (AggregateException e) {
				Assert.AreEqual (1, e.InnerExceptions.Count, "#2");
			}

			cts.Cancel ();
		}

		[Test]
		public void Cancel_MultipleExceptions ()
		{
			var cts = new CancellationTokenSource ();

			cts.Token.Register (() => { throw new ApplicationException ("1"); });
			cts.Token.Register (() => { throw new ApplicationException ("2"); });
			cts.Token.Register (() => { throw new ApplicationException ("3"); });

			try {
				cts.Cancel ();
				Assert.Fail ("#1");
			} catch (AggregateException e) {
				Assert.AreEqual (3, e.InnerExceptions.Count, "#2");
			}

			cts.Cancel ();

			try {
				cts.Token.Register (() => { throw new ApplicationException ("1"); });
				Assert.Fail ("#11");
			} catch (ApplicationException) {
			}

			cts.Cancel ();
		}

		[Test]
		public void Cancel_ExceptionOrder ()
		{
			var cts = new CancellationTokenSource ();

			cts.Token.Register (() => { throw new ApplicationException ("1"); });
			cts.Token.Register (() => { throw new ApplicationException ("2"); });
			cts.Token.Register (() => { throw new ApplicationException ("3"); });

			try {
				cts.Cancel ();
			} catch (AggregateException e) {
				Assert.AreEqual (3, e.InnerExceptions.Count, "#2");
				Assert.AreEqual ("3", e.InnerExceptions[0].Message, "#3");
				Assert.AreEqual ("2", e.InnerExceptions[1].Message, "#4");
				Assert.AreEqual ("1", e.InnerExceptions[2].Message, "#5");
			}
		}

		[Test]
		public void Cancel_MultipleException_Recursive ()
		{
			CancellationTokenSource cts = new CancellationTokenSource ();
			CancellationToken c = cts.Token;
			c.Register (() => {
				cts.Cancel ();
			});

			c.Register (() => {
				throw new ApplicationException ();
			});

			c.Register (() => {
				throw new NotSupportedException ();
			});

			try {
				cts.Cancel (false);
				Assert.Fail ("#1");
			} catch (AggregateException e) {
				Assert.AreEqual (2, e.InnerExceptions.Count, "#2");
			}
		}

		[Test]
		public void Cancel_MultipleExceptionsFirstThrows ()
		{
			var cts = new CancellationTokenSource ();

			cts.Token.Register (() => { throw new ApplicationException ("1"); });
			cts.Token.Register (() => { throw new ApplicationException ("2"); });
			cts.Token.Register (() => { throw new ApplicationException ("3"); });

			try {
				cts.Cancel (true);
				Assert.Fail ("#1");
			} catch (ApplicationException) {
			}

			cts.Cancel ();
		}

		[Test]
		public void CreateLinkedTokenSource_InvalidArguments ()
		{
			var cts = new CancellationTokenSource ();
			var token = cts.Token;

			try {
				CancellationTokenSource.CreateLinkedTokenSource (null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException) {
			}

			try {
				CancellationTokenSource.CreateLinkedTokenSource (new CancellationToken[0]);
				Assert.Fail ("#2");
			} catch (ArgumentException) {
			}
		}

		[Test]
		public void CreateLinkedTokenSource ()
		{
			var cts = new CancellationTokenSource ();
			cts.Cancel ();

			var linked = CancellationTokenSource.CreateLinkedTokenSource (cts.Token);
			Assert.IsTrue (linked.IsCancellationRequested, "#1");

			linked = CancellationTokenSource.CreateLinkedTokenSource (new CancellationToken ());
			Assert.IsFalse (linked.IsCancellationRequested, "#2");
		}

		[Test]
		public void Dispose ()
		{
			var cts = new CancellationTokenSource ();
			var token = cts.Token;

			cts.Dispose ();
			cts.Dispose ();
			var b = cts.IsCancellationRequested;
			token.ThrowIfCancellationRequested ();

			try {
				cts.Cancel ();
				Assert.Fail ("#1");
			} catch (ObjectDisposedException) {
			}

			try {
				var t = cts.Token;
				Assert.Fail ("#2");
			} catch (ObjectDisposedException) {
			}

			bool throwOnDispose = false;
			AppContext.TryGetSwitch ("Switch.System.Threading.ThrowExceptionIfDisposedCancellationTokenSource", out throwOnDispose);
			if (throwOnDispose) { 
				try {
					token.Register (() => { });
					Assert.Fail ("#3");
				} catch (ObjectDisposedException) {
				}
			}

			try {
				var wh = token.WaitHandle;
				Assert.Fail ("#4");
			} catch (ObjectDisposedException) {
			}

			if (throwOnDispose) {
				try {
					CancellationTokenSource.CreateLinkedTokenSource (token);
					Assert.Fail ("#5");
				} catch (ObjectDisposedException) {
				}
			}

			try {
				cts.CancelAfter (1);
				Assert.Fail ("#6");
			} catch (ObjectDisposedException) {
			}
		}

		[Test]
		public void RegisterThenDispose ()
		{
			var cts1 = new CancellationTokenSource ();
			var reg1 = cts1.Token.Register (() => { throw new ApplicationException (); });

			var cts2 = new CancellationTokenSource ();
			var reg2 = cts2.Token.Register (() => { throw new ApplicationException (); });

			Assert.AreNotEqual (cts1, cts2, "#1");
			Assert.AreNotSame (cts1, cts2, "#2");

			reg1.Dispose ();
			cts1.Cancel ();

			try {
				cts2.Cancel ();
				Assert.Fail ("#3");
			} catch (AggregateException) {
			}
		}

		[Test]
		[Category ("MultiThreaded")]
		public void RegisterWhileCancelling ()
		{
			var cts = new CancellationTokenSource ();
			var mre = new ManualResetEvent (false);
			var mre2 = new ManualResetEvent (false);
			int called = 0;

			cts.Token.Register (() => {
				Assert.IsTrue (cts.IsCancellationRequested, "#10");
				Assert.IsTrue (cts.Token.WaitHandle.WaitOne (0), "#11");
				mre2.Set ();
				mre.WaitOne (3000);
				called += 11;
			});

			var t = Task.Factory.StartNew (() => { cts.Cancel (); });

			Assert.IsTrue (mre2.WaitOne (1000), "#0");
			cts.Token.Register (() => { called++; });
			Assert.AreEqual (1, called, "#1");
			Assert.IsFalse (t.IsCompleted, "#2");

			mre.Set ();
			Assert.IsTrue (t.Wait (1000), "#3");
			Assert.AreEqual (12, called, "#4");
		}

		[Test]
		public void ReEntrantRegistrationTest ()
		{
			bool unregister = false;
			bool register = false;
			var source = new CancellationTokenSource ();
			var token = source.Token;

			Console.WriteLine ("Test1");
			var reg = token.Register (() => unregister = true);
			token.Register (() => reg.Dispose ());
			token.Register (() => { Console.WriteLine ("Gnyah"); token.Register (() => register = true); });
			source.Cancel ();

			Assert.IsFalse (unregister);
			Assert.IsTrue (register);
		}

		[Test]
		public void DisposeAfterRegistrationTest ()
		{
			var source = new CancellationTokenSource ();
			bool ran = false;
			var req = source.Token.Register (() => ran = true);
			source.Dispose ();
			req.Dispose ();
			Assert.IsFalse (ran);
		}

		[Test]
		public void CancelLinkedTokenSource ()
		{
			var cts = new CancellationTokenSource ();
			bool canceled = false;
			cts.Token.Register (() => canceled = true);

			using (var linked = CancellationTokenSource.CreateLinkedTokenSource (cts.Token)) {
				;
			}

			Assert.IsFalse (canceled, "#1");
			Assert.IsFalse (cts.IsCancellationRequested, "#2");

			cts.Cancel ();

			Assert.IsTrue (canceled, "#3");
		}

		[Category ("NotWorking")] // why would linked token be imune to ObjectDisposedException on Cancel?
		[Test]
		public void ConcurrentCancelLinkedTokenSourceWhileDisposing ()
		{
			for (int i = 0, total = 500; i < total; ++i) {
				var src = new CancellationTokenSource ();
				var linked = CancellationTokenSource.CreateLinkedTokenSource (src.Token);
				var cntd = new CountdownEvent (2);

				var t1 = new Thread (() => {
					if (!cntd.Signal ())
						cntd.Wait (200);
					src.Cancel ();
				});
				var t2 = new Thread (() => {
					if (!cntd.Signal ())
						cntd.Wait (200);
					linked.Dispose ();
				});

				t1.Start ();
				t2.Start ();
				t1.Join (500);
				t2.Join (500);
			}
		}

		[Test]
		public void DisposeRace ()
		{
			for (int i = 0, total = 1000; i < total; ++i) {
				var c1 = new CancellationTokenSource ();
				var wh = c1.Token.WaitHandle;
				c1.CancelAfter (1);
				Thread.Sleep (1);
				c1.Dispose ();
			}
		}

		[Test] // https://github.com/mono/mono/issues/12421
		public void EnsurePostIsNotCalled ()
		{
			SynchronizationContext mainContext = SynchronizationContext.Current;
			var asc = new AssertSyncContext ();
			SynchronizationContext.SetSynchronizationContext (asc);
			var ct = new CancellationTokenSource ();
			var tcs = new TaskCompletionSource<bool> ();
			ct.Token.Register (() => tcs.TrySetCanceled ());

			bool taskIsCancelled = false;
			Action awaitAction = async () => {
					try { await tcs.Task; }
					catch (OperationCanceledException) { 
						taskIsCancelled = true;
					}
				};
			awaitAction ();
			ct.Cancel (); // should not trigger SynchronizationContext.Post
			Assert.IsTrue (taskIsCancelled);
			SynchronizationContext.SetSynchronizationContext (mainContext);
		}

		class AssertSyncContext : SynchronizationContext
		{
			public override void Post (SendOrPostCallback d, object state) =>
				throw new InvalidOperationException ("SynchronizationContext.Post was not expected.");
		}
	}
}


