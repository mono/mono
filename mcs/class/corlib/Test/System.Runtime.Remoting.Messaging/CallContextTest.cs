//
// CallContexTest.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//  Chris F Carroll <chris.carroll@unforgettable.me.uk>
//
// Copyright (C) 2014 Xamarin Inc (http://www.xamarin.com)
// Copyright (C) 2013 Chris F Carroll (http://cafe-encounter.net)
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
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using System.Runtime.Remoting.Messaging;
using System.Threading;
#if NET_4_0
using System.Threading.Tasks;
#endif

namespace MonoTests.System.Runtime.Remoting.Messaging
{
	[TestFixture]
	public class CallContextTest
	{
		public class Holder : ILogicalThreadAffinative
		{
			public string Value { get; set; }
		}

		const string SlotName = "Test";

		[Test]
		public void CallContextPropagation_Thread ()
		{
			bool passed = false;
			var t = new Thread (() => {
				var h = CallContext.GetData (SlotName) as Holder;
				passed = h == null;
				CallContext.FreeNamedDataSlot (SlotName);
			});
			t.Start ();
			t.Join ();
			Assert.IsTrue (passed, "#1");

			var holder = new Holder {
				Value = "Hello World"
			};
			CallContext.SetData (SlotName, holder);

			t = new Thread (() => {
				var h = CallContext.GetData (SlotName) as Holder;
				passed = h == holder;
				CallContext.FreeNamedDataSlot (SlotName);
			});
			t.Start ();
			t.Join ();
			CallContext.FreeNamedDataSlot (SlotName);

			Assert.IsTrue (passed, "#2");
		}

		[Test]
		public void CallContextPropagation_ThreadPool ()
		{
			var holder = new Holder {
				Value = "Hello World"
			};
			CallContext.SetData (SlotName, holder);

			bool passed = false;
			var mre = new ManualResetEvent (false);
			ThreadPool.QueueUserWorkItem(x => {
				var h = CallContext.GetData (SlotName) as Holder;
				passed = h == holder;
				CallContext.FreeNamedDataSlot (SlotName);
				mre.Set ();
			});

			Assert.IsTrue (mre.WaitOne (3000), "#1");
			Assert.IsTrue (passed, "#2");

			CallContext.FreeNamedDataSlot (SlotName);
		}

		[Test]
		public void CallContextPropagation_Not_ThreadPool ()
		{
			CallContext.SetData (SlotName, "x");

			bool passed = false;
			var mre = new ManualResetEvent (false);
			ThreadPool.QueueUserWorkItem(x => {
				var h = (string)CallContext.GetData (SlotName);
				passed = h == null;
				CallContext.FreeNamedDataSlot (SlotName);
				mre.Set ();
			});

			Assert.IsTrue (mre.WaitOne (3000), "#1");
			Assert.IsTrue (passed, "#2");

			CallContext.FreeNamedDataSlot (SlotName);
		}

#if NET_4_0
		[Test]
		public void CallContextPropagation_Task ()
		{
			var holder = new Holder {
				Value = "Hello World"
			};
			CallContext.SetData (SlotName, holder);
			
			bool passed = false;
			var t = Task.Factory.StartNew(() => {
				var h = CallContext.GetData (SlotName) as Holder;
				passed = h == holder;
				CallContext.FreeNamedDataSlot (SlotName);
			});

			Assert.IsTrue (t.Wait (3000), "#1");
			Assert.IsTrue (passed, "#2");

			CallContext.FreeNamedDataSlot (SlotName);
		}

		[Test]
		public void CallContextPropagation_TaskContinuation ()
		{
			string d1 = null;
			string d2 = null;
			Console.WriteLine("Current thread: {0}", Thread.CurrentThread.ManagedThreadId);

			var ct = Thread.CurrentThread.ManagedThreadId;
			CallContext.LogicalSetData ("d1", "logicalData");
			CallContext.SetData ("d2", "data2");
			var t = Task.Factory.StartNew (() => {
				}).ContinueWith (task => {
					d1 = (string) CallContext.LogicalGetData ("d1");
					d2 = (string) CallContext.GetData ("d2");
				}, TaskContinuationOptions.ExecuteSynchronously);

			Assert.IsTrue (t.Wait (3000), "#0");
			Assert.AreEqual ("logicalData", d1, "#1");
			Assert.IsNull (d2, "#2");

			CallContext.FreeNamedDataSlot ("d1");
			CallContext.FreeNamedDataSlot ("d2");
		}
#endif

		[Test]
		public void FreeNamedDataSlot_ShouldClearLogicalData ()
		{
			CallContext.LogicalSetData ("slotkey", "illogical");
			CallContext.FreeNamedDataSlot ("slotkey");

			Assert.IsNull (CallContext.LogicalGetData ("slotkey"), "Illogical slot should be null");
			Assert.IsNull (CallContext.GetData ("slotkey"), "Illogical slot should be null");
		}

		[Test]
		public void FreeNamedDataSlot_ShouldClearIllogicalData ()
		{
			CallContext.SetData ("slotkey", "illogical");
			CallContext.FreeNamedDataSlot ("slotkey");

			Assert.IsNull (CallContext.LogicalGetData ("slotkey"), "Illogical slot should be null");
			Assert.IsNull (CallContext.GetData ("slotkey"), "Illogical slot should be null");
		}

		[Test]
		public void FreeNamedDataSlot_ShouldClearBothLogicalAndIllogicalData ()
		{
			CallContext.LogicalSetData ("slotkey","logical");
			CallContext.SetData ("slotkey", "illogical");
			CallContext.FreeNamedDataSlot ("slotkey");

			Assert.IsNull (CallContext.LogicalGetData ("slotkey"), "Illogical slot should be null");
			Assert.IsNull (CallContext.GetData ("slotkey"), "Illogical slot should be null");
		}
	}
}
