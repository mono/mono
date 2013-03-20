//
// CheckoutExceptionTest.cs - NUnit tests for CheckoutException
//
// Author:
//	Gert Driesen  <drieseng@users.sourceforge.net>
//
// Copyright (C) 2007 Gert Driesen
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

#if !MOBILE

using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;

using NUnit.Framework;

namespace MonoTests.System.ComponentModel.Design
{
	[TestFixture]
	public class CheckoutExceptionTest
	{
		[Test] // ctor ()
		public void Constructor0 ()
		{
			CheckoutException ex = new CheckoutException ();
			Assert.AreEqual (-2147467259, ex.ErrorCode, "#1");
			Assert.IsNull (ex.InnerException, "#2");
			Assert.IsNotNull (ex.Message, "#3");
			Assert.IsTrue (ex.Message.IndexOf (ex.GetType ().FullName) == -1, "#4");
			Assert.AreEqual (new ExternalException ().Message, ex.Message, "#5");
		}

		[Test] // ctor (string)
		public void Constructor1 ()
		{
			CheckoutException ex;
			string msg = "ERROR";

			ex = new CheckoutException (msg);
			Assert.AreEqual (-2147467259, ex.ErrorCode, "#A1");
			Assert.IsNull (ex.InnerException, "#A2");
			Assert.AreSame (msg, ex.Message, "#A3");

			ex = new CheckoutException ((string) null);
			Assert.AreEqual (-2147467259, ex.ErrorCode, "#B1");
			Assert.IsNull (ex.InnerException, "#B2");
			Assert.IsNotNull (msg, ex.Message, "#B3");
			Assert.IsTrue (ex.Message.IndexOf (ex.GetType ().FullName) != -1, "#B4");

			ex = new CheckoutException (string.Empty);
			Assert.AreEqual (-2147467259, ex.ErrorCode, "#C1");
			Assert.IsNull (ex.InnerException, "#C2");
			Assert.IsNotNull (msg, ex.Message, "#C3");
			Assert.AreEqual (string.Empty, ex.Message, "#C4");
		}

#if NET_2_0
		[Test] // ctor (string, Exception)
		public void Constructor3 ()
		{
			CheckoutException ex;
			string msg = "ERROR";
			Exception inner = new Exception ();

			ex = new CheckoutException (msg, inner);
			Assert.AreEqual (-2147467259, ex.ErrorCode, "#A1");
			Assert.AreSame (inner, ex.InnerException, "#A2");
			Assert.AreSame (msg, ex.Message, "#A3");

			ex = new CheckoutException ((string) null, inner);
			Assert.AreEqual (-2147467259, ex.ErrorCode, "#B1");
			Assert.AreSame (inner, ex.InnerException, "#B2");
			Assert.IsNotNull (msg, ex.Message, "#B3");
			Assert.AreEqual (new CheckoutException (null).Message, ex.Message, "#B4");

			ex = new CheckoutException (msg, (Exception) null);
			Assert.AreEqual (-2147467259, ex.ErrorCode, "#C1");
			Assert.IsNull (ex.InnerException, "#C2");
			Assert.AreSame (msg, ex.Message, "#C3");

			ex = new CheckoutException (string.Empty, (Exception) null);
			Assert.AreEqual (-2147467259, ex.ErrorCode, "#D1");
			Assert.IsNull (ex.InnerException, "#D2");
			Assert.IsNotNull (ex.Message, "#D3");
			Assert.AreEqual (string.Empty, ex.Message, "#D4");
		}
#endif

		[Test] // ctor (string, int)
		public void Constructor4 ()
		{
			CheckoutException ex;
			string msg = "ERROR";

			ex = new CheckoutException (msg, int.MinValue);
			Assert.AreEqual (int.MinValue, ex.ErrorCode, "#A1");
			Assert.IsNull (ex.InnerException, "#A2");
			Assert.AreSame (msg, ex.Message, "#A3");

			ex = new CheckoutException ((string) null, int.MaxValue);
			Assert.AreEqual (int.MaxValue, ex.ErrorCode, "#B1");
			Assert.IsNull (ex.InnerException, "#B2");
			Assert.IsNotNull (msg, ex.Message, "#B3");
			Assert.AreEqual (new CheckoutException (null).Message, ex.Message, "#B4");

			ex = new CheckoutException (msg, 0);
			Assert.AreEqual (0, ex.ErrorCode, "#C1");
			Assert.IsNull (ex.InnerException, "#C2");
			Assert.AreSame (msg, ex.Message, "#C3");

			ex = new CheckoutException (string.Empty, 0);
			Assert.AreEqual (0, ex.ErrorCode, "#D1");
			Assert.IsNull (ex.InnerException, "#D2");
			Assert.IsNotNull (ex.Message, "#D3");
			Assert.AreEqual (string.Empty, ex.Message, "#D4");
		}

		[Test]
		public void Canceled ()
		{
			CheckoutException ex = CheckoutException.Canceled;
#if NET_2_0
			Assert.AreEqual (-2147467260, ex.ErrorCode, "#1");
#else
			Assert.AreEqual (-2147467259, ex.ErrorCode, "#1");
#endif
			Assert.IsNull (ex.InnerException, "#2");
			Assert.IsNotNull (ex.Message, "#3");
			Assert.IsTrue (ex.Message.IndexOf (ex.GetType ().FullName) == -1, "#4");
		}
	}
}

#endif