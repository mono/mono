//
// System.Web.HttpContext.cs - Unit tests for System.Web.HttpContext
//
// Author:
//	Miguel de Icaza  <miguel@novell.com.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
using System.Text;
using System.Web;
using System.Collections.Specialized;
using NUnit.Framework;

namespace MonoTests.System.Web {

	[TestFixture]
	public class Test_HttpContext {

		HttpContext Cook (int re)
		{
			FakeHttpWorkerRequest2 f = new FakeHttpWorkerRequest2 (re);
			HttpContext c = new HttpContext (f);

			return c;
		}
			
		[Test] public void Properties ()
		{
			HttpContext c;

			c = Cook (10);
			Assert.AreEqual (null, c.AllErrors, "P1");
		}
		
		[Test] public void Validation_Test_Cookies ()
		{
			HttpContext c;

			c = Cook (10);
			
		}

		[Test] public void Items ()
		{
			HttpContext c = Cook (10);

			Console.WriteLine ("TTTTTTTTTTT: " + c.Items.GetType ().ToString ());
			Assert.AreEqual (false, c.Items.IsReadOnly, "it1");
			Assert.AreEqual (false, c.Items.IsFixedSize, "it1");
		}

		[Test]
		public void NullConstructor ()
		{
			HttpContext ctx = new HttpContext (null);
			Assert.IsNotNull (ctx.Request, "Request");
			Assert.IsNotNull (ctx.Response, "Response");
		}
	}
}
