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
		[Category ("NotWorkingRuntimeInterpreter")]
		public void ReRegisterForFinalizeTest ()
		{
			var thread =  new Thread (Run_ReRegisterForFinalizeTest);
			thread.Start ();
			thread.Join ();

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
