//
// ParallelTests.cs
//
// Copyright (c) 2008 Jérémie "Garuma" Laval
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
//
//

#if NET_4_0

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
using System.Collections.Concurrent;

using NUnit;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace MonoTests.System.Threading.Tasks
{
	[TestFixture]
	public class ParallelTests
	{
		[Test]
		public void ParallelForTestCase ()
		{
			int[] expected = Enumerable.Range (1, 1000).Select ((e) => e * 2).ToArray ();
			
			ParallelTestHelper.Repeat (() => {	
				int[] actual = Enumerable.Range (1, 1000).ToArray ();
				SpinWait sw = new SpinWait ();
				
				Parallel.For (0, actual.Length, (i) => { actual[i] *= 2; sw.SpinOnce (); });

				Assert.That (actual, new CollectionEquivalentConstraint (expected), "#1, same");
				Assert.That (actual, new EqualConstraint (expected), "#2, in order");
			});
		}

		[Test, ExpectedException (typeof (AggregateException))]
		public void ParallelForExceptionTestCase ()
		{
			Parallel.For(1, 100, delegate (int i) { throw new Exception("foo"); });
		}
		
		[Test]
		public void ParallelForSmallRangeTest ()
		{
			ParallelTestHelper.Repeat (() => {
				int test = -1;
				Parallel.For (0, 1, (i) => test = i);
				
				Assert.AreEqual (0, test, "#1");
			});
		}
		
		[Test]
		public void ParallelForNoOperationTest ()
		{
			bool launched = false;
			Parallel.For (4, 1, (i) => launched = true);
			Assert.IsFalse (launched, "#1");
		}

		[Test]
		public void ParallelForNestedTest ()
		{
			bool[] launched = new bool[6 * 20 * 10];
			Parallel.For (0, 6, delegate (int i) {
				Parallel.For (0, 20, delegate (int j) {
					Parallel.For (0, 10, delegate (int k) {
							launched[i * 20 * 10 + j * 10 + k] = true;
					});
				});
		    });

			Assert.IsTrue (launched.All ((_) => _), "All true");
		}
		
		[Test]
		public void ParallelForEachTestCase ()
		{
			ParallelTestHelper.Repeat (() => {
				IEnumerable<int> e = Enumerable.Repeat(1, 500);
				ConcurrentQueue<int> queue = new ConcurrentQueue<int> ();
				SpinWait sw = new SpinWait ();
				int count = 0;
				
				Parallel.ForEach (e, (element) => { Interlocked.Increment (ref count); queue.Enqueue (element); sw.SpinOnce (); });
				
				Assert.AreEqual (500, count, "#1");

				Assert.That (queue, new CollectionEquivalentConstraint (e), "#2");
			});
		}

		class ValueAndSquare
		{ 
			public float Value { get; set; }
			public float Square { get; set; }
		}

		[Test]
		public void ParallerForEach_UserType ()
		{
			var values = new[] {
				new ValueAndSquare() { Value = 1f },
				new ValueAndSquare() { Value = 2f },
				new ValueAndSquare() { Value = 3f },
				new ValueAndSquare() { Value = 4f },
				new ValueAndSquare() { Value = 5f },
				new ValueAndSquare() { Value = 6f },
				new ValueAndSquare() { Value = 7f },
				new ValueAndSquare() { Value = 8f },
				new ValueAndSquare() { Value = 9f },
				new ValueAndSquare() { Value = 10f }
			};

			Parallel.ForEach (Partitioner.Create (values), l => l.Square = l.Value * l.Value);

			foreach (var item in values) {
				Assert.AreEqual (item.Square, item.Value * item.Value);
			}
		}
			
		[Test, ExpectedException (typeof (AggregateException))]
		public void ParallelForEachExceptionTestCase ()
		{
			IEnumerable<int> e = Enumerable.Repeat (1, 10);
			Parallel.ForEach (e, delegate (int element) { throw new Exception ("foo"); });
		}

		[Test]
		public void BasicInvokeTest ()
		{
			int val1 = 0, val2 = 0;

			Parallel.Invoke (() => Interlocked.Increment (ref val1), () => Interlocked.Increment (ref val2));
			Assert.AreEqual (1, val1, "#1");
			Assert.AreEqual (1, val2, "#2");
		}

		[Test]
		public void InvokeWithOneNullActionTest ()
		{
			int val1 = 0, val2 = 0;

			try {
				Parallel.Invoke (() => Interlocked.Increment (ref val1), null, () => Interlocked.Increment (ref val2));
			} catch (ArgumentException ex) {
				Assert.AreEqual (0, val1, "#1");
				Assert.AreEqual (0, val2, "#2");
				return;
			}
			Assert.Fail ("Shouldn't be there");
		}

		[Test]
		public void OneActionInvokeTest ()
		{
			int val = 0;

			Parallel.Invoke (() => Interlocked.Increment (ref val));
			Assert.AreEqual (1, val, "#1");
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void InvokeWithNullActions ()
		{
			Parallel.Invoke ((Action[])null);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void InvokeWithNullOptions ()
		{
			Parallel.Invoke ((ParallelOptions)null, () => Thread.Sleep (100));
		}

		[Test]
		public void InvokeWithExceptions ()
		{
			try {
				Parallel.Invoke (() => { throw new ApplicationException ("foo"); }, () => { throw new IOException ("foo"); });
			} catch (AggregateException ex) {
				Assert.AreEqual (2, ex.InnerExceptions.Count);
				foreach (var e in ex.InnerExceptions)
					Console.WriteLine (e.GetType ());
				Assert.IsTrue (ex.InnerExceptions.Any (e => e.GetType () == typeof (ApplicationException)));
				Assert.IsTrue (ex.InnerExceptions.Any (e => e.GetType () == typeof (IOException)));
				return;
			}
			Assert.Fail ("Shouldn't go there");
		}
	}
}
#endif
