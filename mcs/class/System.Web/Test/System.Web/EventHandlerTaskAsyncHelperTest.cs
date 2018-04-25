//
// MonoTests.System.Web.EventHandlerTaskAsyncHelperTest.cs
//
// Author:
//   Kornel Pal (kornelpal@gmail.com)
//
// Copyright (C) 2014 Kornel Pal
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
using System.Threading.Tasks;
using System.Web;
using NUnit.Framework;

namespace MonoTests.System.Web
{
	[TestFixture]
	public sealed class EventHandlerTaskAsyncHelperTest : TaskAsyncResultTest
	{
		EventHandlerTaskAsyncHelper helper;
		object expectedSender;
		EventArgs expectedEventArgs;

		static Task DummyTaskEventHandler (object sender, EventArgs e)
		{
			throw new AssertionException ("Should not be called.");
		}

		protected override void SetNullArguments ()
		{
			expectedSender = null;
			expectedEventArgs = null;
		}

		protected override IAsyncResult GetAsyncResult (Func<Task> taskFactory, AsyncCallback callback, object state)
		{
			Assert.IsNull (helper, "GetAsyncResult#A01");

			TaskEventHandler handler = (sender, e) => {
				Assert.AreSame (expectedSender, sender, "GetAsyncResult#A02");
				Assert.AreSame (expectedEventArgs, e, "GetAsyncResult#A03");

				return taskFactory ();
			};

			helper = new EventHandlerTaskAsyncHelper (handler);
			return helper.BeginEventHandler (expectedSender, expectedEventArgs, callback, state);
		}

		protected override void Wait (IAsyncResult result)
		{
			Assert.IsNotNull (helper, "Wait#A01");

			helper.EndEventHandler (result);
		}

		protected override void TestSetUp ()
		{
			base.TestSetUp ();

			helper = null;
			expectedSender = new object ();
			expectedEventArgs = new EventArgs ();
		}

		[Test]
		public void Constructor ()
		{
			var helper = new EventHandlerTaskAsyncHelper (DummyTaskEventHandler);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor_NullHandler ()
		{
			var helper = new EventHandlerTaskAsyncHelper (null);
		}

		[Test]
		public void BeginEventHandler ()
		{
			var helper = new EventHandlerTaskAsyncHelper (DummyTaskEventHandler);

			Assert.IsNotNull (helper.BeginEventHandler, "#A01");
		}

		[Test]
		public void EndEventHandler ()
		{
			var helper = new EventHandlerTaskAsyncHelper (DummyTaskEventHandler);

			Assert.IsNotNull (helper.EndEventHandler, "#A01");
		}
	}
}

