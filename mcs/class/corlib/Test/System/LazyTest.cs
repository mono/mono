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

using System;
using System.Reflection;
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
			new Lazy<int> (null, false);
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
		public void NoDefaultCtor () {
			var l1 = new Lazy<NoDefaultCtorClass> ();
			
			try {
				var o = l1.Value;
				Assert.Fail ();
			} catch (MissingMemberException) {
			}
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
		[Category ("MultiThreaded")]
		public void EnsureSingleThreadSafeExecution ()
		{
			counter = 42;
			bool started = false;

			var l = new Lazy<int> (delegate () { return counter ++; }, true);
			bool failed = false;
			object monitor = new object ();
			var threads = new Thread [4];
			for (int i = 0; i < threads.Length; ++i) {
				threads [i] = new Thread (delegate () {
						lock (monitor) {
							if (!started) {
								if (!Monitor.Wait (monitor, 2000))
									failed = true;
							}
						}
						int val = l.Value;
					});
			}
			for (int i = 0; i < threads.Length; ++i)
				threads [i].Start ();
			lock (monitor) {
				started = true;
				Monitor.PulseAll (monitor);
			}

			for (int i = 0; i < threads.Length; ++i)
				threads [i].Join ();

			Assert.IsFalse (failed);
			Assert.AreEqual (42, l.Value);
		}
		
		[Test]
		public void InitRecursion ()
		{
			Lazy<DefaultCtorClass> c = null;
			c = new Lazy<DefaultCtorClass> (() => { Console.WriteLine (c.Value); return null; });
			
			try {
				var r = c.Value;
				Assert.Fail ();
			} catch (InvalidOperationException) {
			}
		}

		[Test]
		public void ModeNone ()
		{
			int x;
			bool fail = true;
			Lazy<int> lz = new Lazy<int> (() => { if (fail) throw new Exception (); else return 99; }, LazyThreadSafetyMode.None);
			try {
				x = lz.Value;
				Assert.Fail ("#1");
				Console.WriteLine (x);
			} catch (Exception ex) { }

			try {
				x = lz.Value;
				Assert.Fail ("#2");
			} catch (Exception ex) { }

			fail = false;
			try {
				x = lz.Value;
				Assert.Fail ("#3");
			} catch (Exception ex) { }

			bool rec = true;
			lz = new Lazy<int> (() => rec ? lz.Value : 99, LazyThreadSafetyMode.None);

			try {
				x = lz.Value;
				Assert.Fail ("#4");
			} catch (InvalidOperationException ex) { }

			rec = false;
			try {
				x = lz.Value;
				Assert.Fail ("#5");
			} catch (InvalidOperationException ex) { }
		}

		[Test]
		public void ModePublicationOnly () {
			bool fail = true;
			int invoke = 0;
			Lazy<int> lz = new Lazy<int> (() => { ++invoke; if (fail) throw new Exception (); else return 99; }, LazyThreadSafetyMode.PublicationOnly);

			try {
				int x = lz.Value;
				Assert.Fail ("#1");
				Console.WriteLine (x);
			} catch (Exception ex) { }

			try {
				int x = lz.Value;
				Assert.Fail ("#2");
			} catch (Exception ex) { }


			Assert.AreEqual (2, invoke, "#3");
			fail = false;
			Assert.AreEqual (99,  lz.Value, "#4");
			Assert.AreEqual (3, invoke, "#5");

			invoke = 0;
			bool rec = true;
			lz = new Lazy<int> (() => { ++invoke; bool r = rec; rec = false; return r ? lz.Value : 88; }, 	LazyThreadSafetyMode.PublicationOnly);

			Assert.AreEqual (88,  lz.Value, "#6");
			Assert.AreEqual (2, invoke, "#7");
		}

		[Test]
		public void ModeExecutionAndPublication () {
			int invoke = 0;
			bool fail = true;
			Lazy<int> lz = new Lazy<int> (() => { ++invoke; if (fail) throw new Exception (); else return 99; }, LazyThreadSafetyMode.ExecutionAndPublication);

			try {
				int x = lz.Value;
				Assert.Fail ("#1");
				Console.WriteLine (x);
			} catch (Exception ex) { }
			Assert.AreEqual (1, invoke, "#2");

			try {
				int x = lz.Value;
				Assert.Fail ("#3");
			} catch (Exception ex) { }
			Assert.AreEqual (1, invoke, "#4");

			fail = false;
			try {
				int x = lz.Value;
				Assert.Fail ("#5");
			} catch (Exception ex) { }
			Assert.AreEqual (1, invoke, "#6");

			bool rec = true;
			lz = new Lazy<int> (() => rec ? lz.Value : 99, LazyThreadSafetyMode.ExecutionAndPublication);

			try {
				int x = lz.Value;
				Assert.Fail ("#7");
			} catch (InvalidOperationException ex) { }

			rec = false;
			try {
				int x = lz.Value;
				Assert.Fail ("#8");
			} catch (InvalidOperationException ex) { }
		}

		static int Return22 () {
			return 22;
		}

		[Test]
		public void Trivial_Lazy () {
			var x = new Lazy<int> (Return22, false);
			Assert.AreEqual (22, x.Value, "#1");
		}

		[Test]
		[Category ("MultiThreaded")]
		public void ConcurrentInitialization ()
		{
			var init = new AutoResetEvent (false);
			var e1_set = new AutoResetEvent (false);

			var lazy = new Lazy<string> (() => {
				init.Set ();
				Thread.Sleep (10);
				throw new ApplicationException ();
			});

			Exception e1 = null;
			var thread = new Thread (() => {
				try {
					string value = lazy.Value;
				} catch (Exception ex) {
					e1 = ex;
					e1_set.Set ();
				}
			});
			thread.Start ();

			Assert.IsTrue (init.WaitOne (3000), "#1");

			Exception e2 = null;
			try {
				string value = lazy.Value;
			} catch (Exception ex) {
				e2 = ex;
			}

			Exception e3 = null;
			try {
				string value = lazy.Value;
			} catch (Exception ex) {
				e3 = ex;
			}

			Assert.IsTrue (e1_set.WaitOne (3000), "#2");
			Assert.AreSame (e1, e2, "#3");
			Assert.AreSame (e1, e3, "#4");
		}

	}
}
