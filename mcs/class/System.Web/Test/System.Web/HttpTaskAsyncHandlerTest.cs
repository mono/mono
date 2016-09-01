//
// MonoTests.System.Web.HttpTaskAsyncHandlerTest.cs
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
using System.IO;
using System.Threading.Tasks;
using System.Web;
using NUnit.Framework;

namespace MonoTests.System.Web
{
	[TestFixture]
	public sealed class HttpTaskAsyncHandlerTest : TaskAsyncResultTest
	{
		sealed class DummyHttpTaskAsyncHandler : HttpTaskAsyncHandler
		{
			public DummyHttpTaskAsyncHandler ()
			{
			}

			public override Task ProcessRequestAsync (HttpContext context)
			{
				throw new AssertionException ("Should not be called.");
			}
		}

		sealed class TestHttpTaskAsyncHandler : HttpTaskAsyncHandler
		{
			readonly Func<Task> taskFactory;
			readonly HttpContext expectedContext;

			public TestHttpTaskAsyncHandler (Func<Task> taskFactory, HttpContext expectedContext)
			{
				this.taskFactory = taskFactory;
				this.expectedContext = expectedContext;
			}

			public override Task ProcessRequestAsync (HttpContext context)
			{
				Assert.AreSame (expectedContext, context, "TestHttpTaskAsyncHandler#A01");

				return taskFactory ();
			}
		}

		IHttpAsyncHandler handler;
		HttpContext expectedContext;

		protected override void SetNullArguments ()
		{
			expectedContext = null;
		}

		protected override IAsyncResult GetAsyncResult (Func<Task> taskFactory, AsyncCallback callback, object state)
		{
			Assert.IsNull (handler, "GetAsyncResult#A01");

			handler = new TestHttpTaskAsyncHandler (taskFactory, expectedContext);
			return handler.BeginProcessRequest (expectedContext, callback, state);
		}

		protected override void Wait (IAsyncResult result)
		{
			Assert.IsNotNull (handler, "Wait#A01");

			handler.EndProcessRequest (result);
		}

		protected override void TestSetUp ()
		{
			base.TestSetUp ();

			handler = null;

			var request = new HttpRequest (string.Empty, "http://localhost/", string.Empty);
			var response = new HttpResponse (TextWriter.Null);
			expectedContext = new HttpContext (request, response);
		}

		[Test]
		public void IsReusable ()
		{
			var handler = new DummyHttpTaskAsyncHandler ();
			Assert.IsFalse (handler.IsReusable, "#A01");
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ProcessRequest ()
		{
			var handler = new DummyHttpTaskAsyncHandler ();
			handler.ProcessRequest (expectedContext);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ProcessRequest_NullContext ()
		{
			var handler = new DummyHttpTaskAsyncHandler ();
			handler.ProcessRequest (null);
		}
	}
}

