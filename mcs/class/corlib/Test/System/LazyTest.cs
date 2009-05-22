//
// LazyTest.cs - NUnit Test Cases for Lazy
//
// Author:
//	Zoltan Varga (vargaz@gmail.com)
//
// Copyright (C) 2009 Novell, Inc (http://www.novell.com)
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

#if NET_4_0

using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using NUnit.Framework;

#pragma warning disable 219
#pragma warning disable 168

namespace MonoTests.System
{
	[TestFixture]
	public class LazyTest
	{
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Ctor_Null_1 () {
			new Lazy<int> (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Ctor_Null_2 () {
			new Lazy<int> (null, LazyExecutionMode.NotThreadSafe);
		}

		[Test]
		public void IsValueCreated () {
			var l1 = new Lazy<int> ();

			Assert.IsFalse (l1.IsValueCreated);

			int i = l1.Value;

			Assert.IsTrue (l1.IsValueCreated);
		}

		[Test]
		public void DefaultCtor () {
			var l1 = new Lazy<DefaultCtorClass> ();
			
			var o = l1.Value;
			Assert.AreEqual (5, o.Prop);
		}

		class DefaultCtorClass {
			public DefaultCtorClass () {
				Prop = 5;
			}

			public int Prop {
				get; set;
			}
		}

		[Test]
		[ExpectedException (typeof (MissingMemberException))]
		public void NoDefaultCtor () {
			var l1 = new Lazy<NoDefaultCtorClass> ();
			
			var o = l1.Value;
		}

		class NoDefaultCtorClass {
			public NoDefaultCtorClass (int i) {
			}
		}

		[Test]
		public void NotThreadSafe () {
			var l1 = new Lazy<int> ();

			Assert.AreEqual (0, l1.Value);

			var l2 = new Lazy<int> (delegate () { return 42; });

			Assert.AreEqual (42, l2.Value);
		}

		static int counter;

		[Test]
		public void EnsureSingleThreadSafeExecution () {
			counter = 42;

			//var l = new Lazy<int> (delegate () { return counter ++; }, LazyExecutionMode.NotThreadSafe);
			var l = new Lazy<int> (delegate () { return counter ++; }, LazyExecutionMode.EnsureSingleThreadSafeExecution);

			object monitor = new object ();
			var threads = new Thread [10];
			for (int i = 0; i < 10; ++i) {
				threads [i] = new Thread (delegate () {
						lock (monitor) {
							Monitor.Wait (monitor);
						}
						int val = l.Value;
					});
			}
			for (int i = 0; i < 10; ++i)
				threads [i].Start ();
			lock (monitor)
				Monitor.PulseAll (monitor);
			
			Assert.AreEqual (42, l.Value);
		}			
	}
}

#endif
