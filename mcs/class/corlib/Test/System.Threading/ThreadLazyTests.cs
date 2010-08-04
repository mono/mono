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
using NUnit.Core;
using NUnit.Framework;

namespace MonoTests.System.Threading
{
	[TestFixtureAttribute]
	public class ThreadLazyTests
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
					return 43;
				});

			Exception exception = null;

			try {
				var foo = threadLocal.Value;
			} catch (Exception e) {
				exception = e;
			}

			Assert.IsNotNull (exception, "#1");
			Assert.IsInstanceOfType (typeof (ApplicationException), exception, "#2");
			Assert.AreEqual (1, callTime, "#3");

			exception = null;

			try {
				var foo = threadLocal.Value;
			} catch (Exception e) {
				exception = e;
			}

			Assert.IsNotNull (exception, "#4");
			Assert.IsInstanceOfType (typeof (ApplicationException), exception, "#5");
			Assert.AreEqual (1, callTime, "#6");
		}

		[Test, ExpectedException (typeof (InvalidOperationException))]
		public void MultipleReferenceToValueTest ()
		{
			threadLocal = new ThreadLocal<int> (() => threadLocal.Value + 1);

			var value = threadLocal.Value;
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
