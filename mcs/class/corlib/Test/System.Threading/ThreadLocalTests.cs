#if NET_4_0
// 
// ThreadLazyTests.cs
//  
// Author:
//       Jérémie "Garuma" Laval <jeremie.laval@gmail.com>
// 
// Copyright (c) 2009 Jérémie "Garuma" Laval
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Threading;

using NUnit;
using NUnit.Framework;
#if !MOBILE
using NUnit.Framework.SyntaxHelpers;
#endif

namespace MonoTests.System.Threading
{
	[TestFixtureAttribute]
	public class ThreadLocalTests
	{
		ThreadLocal<int> threadLocal;
		int nTimes;
		
		[SetUp]
		public void Setup ()
		{
			nTimes = 0;
			threadLocal = new ThreadLocal<int> (() => { Interlocked.Increment (ref nTimes); return 42; });
		}

		[Test]
		public void SingleThreadTest ()
		{
			AssertThreadLocal ();
		}
		
		[Test]
		public void ThreadedTest ()
		{
			AssertThreadLocal ();
			
			Thread t = new Thread ((object o) => { Interlocked.Decrement (ref nTimes); AssertThreadLocal (); });
			t.Start ();
			t.Join ();
		}

		[Test]
		public void InitializeThrowingTest ()
		{
			int callTime = 0;
			threadLocal = new ThreadLocal<int> (() => {
					Interlocked.Increment (ref callTime);
					throw new ApplicationException ("foo");
				});

			Exception exception = null;

			try {
				var foo = threadLocal.Value;
			} catch (Exception e) {
				exception = e;
			}

			Assert.IsNotNull (exception, "#1");
			Assert.That (exception, Is.TypeOf (typeof (ApplicationException)), "#2");
			Assert.AreEqual (1, callTime, "#3");

			exception = null;

			try {
				var foo = threadLocal.Value;
			} catch (Exception e) {
				exception = e;
			}

			Assert.IsNotNull (exception, "#4");
			Assert.That (exception, Is.TypeOf (typeof (ApplicationException)), "#5");
			Assert.AreEqual (1, callTime, "#6");
		}

		[Test, ExpectedException (typeof (InvalidOperationException))]
		[Category ("NotDotNet")] // nunit results in stack overflow
		public void MultipleReferenceToValueTest ()
		{
			threadLocal = new ThreadLocal<int> (() => threadLocal.Value + 1);

			var value = threadLocal.Value;
		}

		[Test]
		public void DefaultThreadLocalInitTest ()
		{
			var local = new ThreadLocal<DateTime> ();
			var local2 = new ThreadLocal<object> ();

			Assert.AreEqual (default (DateTime), local.Value);
			Assert.AreEqual (default (object), local2.Value);
		}

		[Test, ExpectedException (typeof (ObjectDisposedException))]
		public void DisposedOnValueTest ()
		{
			var tl = new ThreadLocal<int> ();
			tl.Dispose ();
			var value = tl.Value;
		}

		[Test, ExpectedException (typeof (ObjectDisposedException))]
		public void DisposedOnIsValueCreatedTest ()
		{
			var tl = new ThreadLocal<int> ();
			tl.Dispose ();
			var value = tl.IsValueCreated;
		}

		[Test]
		public void PerThreadException ()
		{
			int callTime = 0;
			threadLocal = new ThreadLocal<int> (() => {
					if (callTime == 1)
						throw new ApplicationException ("foo");
					Interlocked.Increment (ref callTime);
					return 43;
				});

			Exception exception = null;

			var foo = threadLocal.Value;
			bool thread_value_created = false;
			Assert.AreEqual (43, foo, "#3");
			Thread t = new Thread ((object o) => {
				try {
					var foo2 = threadLocal.Value;
				} catch (Exception e) {
					exception = e;
				}
				// should be false and not throw
				thread_value_created = threadLocal.IsValueCreated;
			});
			t.Start ();
			t.Join ();
			Assert.AreEqual (false, thread_value_created, "#4");
			Assert.IsNotNull (exception, "#5");
			Assert.That (exception, Is.TypeOf (typeof (ApplicationException)), "#6");
		}

		void AssertThreadLocal ()
		{
			Assert.IsFalse (threadLocal.IsValueCreated, "#1");
			Assert.AreEqual (42, threadLocal.Value, "#2");
			Assert.IsTrue (threadLocal.IsValueCreated, "#3");
			Assert.AreEqual (42, threadLocal.Value, "#4");
			Assert.AreEqual (1, nTimes, "#5");
		}
	}
}
#endif
