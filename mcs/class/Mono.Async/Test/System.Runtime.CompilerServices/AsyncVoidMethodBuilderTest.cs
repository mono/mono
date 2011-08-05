//
// AsyncVoidMethodBuilderTest.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2011 Xamarin, Inc (http://www.xamarin.com)
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
using System.Runtime.CompilerServices;

namespace MonoTests.System.Runtime.CompilerServices
{
	[TestFixture]
	public class AsyncVoidMethodBuilderTest
	{
		class MyContext : SynchronizationContext
		{
			public int Started;
			public int Completed;
			public int PostCounter;
			public int SendCounter;

			public override void OperationStarted ()
			{
				++Started;
				base.OperationStarted ();
			}

			public override void OperationCompleted ()
			{
				++Completed;
				base.OperationCompleted ();
			}

			public override void Post (SendOrPostCallback d, object state)
			{
				if (state is Exception) {
					++PostCounter;
					base.Post (d, state);
				}
			}

			public override void Send (SendOrPostCallback d, object state)
			{
				if (state is Exception) {
					++SendCounter;
					base.Send (d, state);
				}
			}
		}

		[Test]
		public void SetResult ()
		{
			var awaiter = AsyncVoidMethodBuilder.Create ();
			awaiter.SetResult ();
		}

		[Test]
		public void SetException ()
		{
			var context = new MyContext ();
			try {
				SynchronizationContext.SetSynchronizationContext (context);
				var awaiter = AsyncVoidMethodBuilder.Create ();

				Assert.AreEqual (1, context.Started, "#1");
				Assert.AreEqual (0, context.Completed, "#2");
				Assert.AreEqual (0, context.SendCounter, "#3");
				Assert.AreEqual (0, context.PostCounter, "#4");

				awaiter.SetException (new ApplicationException ());

				Assert.AreEqual (1, context.Started, "#5");
				Assert.AreEqual (1, context.Completed, "#6");
				Assert.AreEqual (0, context.SendCounter, "#7");
				Assert.AreEqual (1, context.PostCounter, "#8");

				awaiter.SetResult ();

				Assert.AreEqual (1, context.Started, "#9");
				Assert.AreEqual (2, context.Completed, "#10");
				Assert.AreEqual (0, context.SendCounter, "#11");
				Assert.AreEqual (1, context.PostCounter, "#12");
			} finally {
				SynchronizationContext.SetSynchronizationContext (null);
			}
		}
	}
}