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
using MonoTests.Helpers;
using NUnit.Framework;

namespace MonoTests.System {

	public interface SizedObject
	{
		long ExpectedSize();
	}

	public class NoPointer : SizedObject
	{
		public long ExpectedSize()
		{
			return 2 * IntPtr.Size;
		}
	}

	public class TwoPointer : SizedObject
	{
		public object field1;
		public object field2;

		public long ExpectedSize()
		{
			return 4*IntPtr.Size;
		}
	}

	public class FourPointer : SizedObject
	{
		public object field1;
		public object field2;
		public object field3;
		public object field4;

		public long ExpectedSize()
		{
			return 6*IntPtr.Size;
		}
	}

	public class EightPointer : SizedObject
	{
		public object field1;
		public object field2;
		public object fiedl3;
		public object field4;
		public object field5;
		public object field6;
		public object field7;
		public object field8;

		public long ExpectedSize()
		{
			return (long)(10*IntPtr.Size);
		}
	}

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
		[Category ("MultiThreaded")]
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


		[Test]
		public static void TestGetBytesAllocatedForCurrentThread()
		{

			Func<SizedObject>[] objectAllocators = {
						  							() => new NoPointer(),
						  							() => new TwoPointer(),
						  							() => new FourPointer(),
					  								() => new EightPointer()
			     								};

			Random r = new Random();

			// Methods trigger allocation when first run.
			// So this code warms up allocation before measuring.
			for (int i = 0; i < objectAllocators.Length; i++)
			{
				Console.WriteLine(objectAllocators[i]().ExpectedSize());
			}

			Console.WriteLine(r.Next(1, 10));

			Assert.AreEqual(1L, 1L);

			// End warmup

			long expectedSize = 0;
			long bytesBeforeAlloc = GC.GetAllocatedBytesForCurrentThread();

			for (int i = 0; i < 10000000; i++)
			{
				expectedSize += objectAllocators[r.Next(0, objectAllocators.Length) ]().ExpectedSize();
			}

			long bytesAfterAlloc = GC.GetAllocatedBytesForCurrentThread();

			Assert.AreEqual(expectedSize, bytesAfterAlloc - bytesBeforeAlloc);
		}
	}	
}
