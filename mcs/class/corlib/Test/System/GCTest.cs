//
// GCTest.cs - NUnit Test Cases for GC
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2016 Xamarin Inc (http://www.xamarin.com)
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
using System.Threading.Tasks;

using NUnit.Framework;

namespace MonoTests.System {

	[TestFixture]
	public class GCTest {
		static class FinalizerHelpers {
			private static IntPtr aptr;

			private static unsafe void NoPinActionHelper (int depth, Action act)
			{
				// Avoid tail calls
				int* values = stackalloc int [20];
				aptr = new IntPtr (values);

				if (depth <= 0) {
					//
					// When the action is called, this new thread might have not allocated
					// anything yet in the nursery. This means that the address of the first
					// object that would be allocated would be at the start of the tlab and
					// implicitly the end of the previous tlab (address which can be in use
					// when allocating on another thread, at checking if an object fits in
					// this other tlab). We allocate a new dummy object to avoid this type
					// of false pinning for most common cases.
					//
					new object ();
					act ();
				} else {
					NoPinActionHelper (depth - 1, act);
				}
			}

			public static void PerformNoPinAction (Action act)
			{
				Thread thr = new Thread (() => NoPinActionHelper (128, act));
				thr.Start ();
				thr.Join ();
			}
		}

		class MyFinalizeObject
		{
			public volatile static int finalized;

			~MyFinalizeObject ()
			{
				if (finalized++ == 0) {
					GC.ReRegisterForFinalize (this);
				}
			}
		}

		static void Run_ReRegisterForFinalizeTest ()
		{
			var m = new WeakReference<MyFinalizeObject> (new MyFinalizeObject ());
			m.SetTarget (null);
		}

		[Test]
		public void ReRegisterForFinalizeTest ()
		{
			FinalizerHelpers.PerformNoPinAction (delegate () {
				Run_ReRegisterForFinalizeTest ();
			});
			var t = Task.Factory.StartNew (() => {
				do {
					GC.Collect ();
					GC.WaitForPendingFinalizers ();
					Task.Yield ();
				} while (MyFinalizeObject.finalized != 2);
			});

			Assert.IsTrue (t.Wait (5000));
		}
	}
}
