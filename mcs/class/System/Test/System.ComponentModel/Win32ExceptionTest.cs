//
// Win32ExceptionTest.cs - NUnit tests for Win32Exception
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

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

using NUnit.Framework;

namespace MonoTests.System.ComponentModel
{
	[TestFixture]
	public class Win32ExceptionTest
	{
		[Test] // ctor ()
		public void Constructor0 ()
		{
			int native_error = Marshal.GetLastWin32Error ();

			Win32Exception ex = new Win32Exception ();
			Assert.AreEqual (-2147467259, ex.ErrorCode, "#1");
			Assert.IsNull (ex.InnerException, "#2");
			Assert.IsNotNull (ex.Message, "#3");
			Assert.IsFalse (ex.Message.IndexOf (ex.GetType ().FullName) != -1, "#4");
			Assert.AreEqual (native_error, ex.NativeErrorCode, "#5");
		}

		[Test] // ctor (int)
		public void Constructor1 ()
		{
			Win32Exception ex;

			ex = new Win32Exception (0);
			Assert.AreEqual (-2147467259, ex.ErrorCode, "#A1");
			Assert.IsNull (ex.InnerException, "#A2");
			Assert.IsNotNull (ex.Message, "#A3");
			Assert.IsFalse (ex.Message.IndexOf (ex.GetType ().FullName) != -1, "#A4");
			Assert.AreEqual (0, ex.NativeErrorCode, "#A5");

			ex = new Win32Exception (int.MinValue);
			Assert.AreEqual (-2147467259, ex.ErrorCode, "#B1");
			Assert.IsNull (ex.InnerException, "#B2");
			Assert.IsNotNull (ex.Message, "#B3");
			Assert.IsFalse (ex.Message.IndexOf (ex.GetType ().FullName) != -1, "#B4");
			Assert.AreEqual (int.MinValue, ex.NativeErrorCode, "#B5");

			ex = new Win32Exception (int.MaxValue);
			Assert.AreEqual (-2147467259, ex.ErrorCode, "#C1");
			Assert.IsNull (ex.InnerException, "#C2");
			Assert.IsNotNull (ex.Message, "#C3");
			Assert.IsFalse (ex.Message.IndexOf (ex.GetType ().FullName) != -1, "#C4");
			Assert.AreEqual (int.MaxValue, ex.NativeErrorCode, "#C5");
		}

#if NET_2_0
		[Test] // ctor (string)
		public void Constructor2 ()
		{
			Win32Exception ex;
			string msg = "ERROR";
			int native_error = Marshal.GetLastWin32Error ();

			ex = new Win32Exception (msg);
			Assert.AreEqual (-2147467259, ex.ErrorCode, "#A1");
			Assert.IsNull (ex.InnerException, "#A2");
			Assert.AreSame (msg, ex.Message, "#A3");
			Assert.AreEqual (native_error, ex.NativeErrorCode, "#A4");

			ex = new Win32Exception ((string) null);
			Assert.AreEqual (-2147467259, ex.ErrorCode, "#B1");
			Assert.IsNull (ex.InnerException, "#B2");
			Assert.IsNotNull (msg, ex.Message, "#B3");
			Assert.IsTrue (ex.Message.IndexOf (ex.GetType ().FullName) != -1, "#B4");
			Assert.AreEqual (native_error, ex.NativeErrorCode, "#B5");

			ex = new Win32Exception (string.Empty);
			Assert.AreEqual (-2147467259, ex.ErrorCode, "#C1");
			Assert.IsNull (ex.InnerException, "#C2");
			Assert.IsNotNull (msg, ex.Message, "#C3");
			Assert.AreEqual (string.Empty, ex.Message, "#C4");
			Assert.AreEqual (native_error, ex.NativeErrorCode, "#C5");
		}
#endif

		[Test] // ctor (int, string)
		public void Constructor3 ()
		{
			Win32Exception ex;
			string msg = "ERROR";

			ex = new Win32Exception (int.MinValue, msg);
			Assert.AreEqual (-2147467259, ex.ErrorCode, "#A1");
			Assert.IsNull (ex.InnerException, "#A2");
			Assert.AreSame (msg, ex.Message, "#A3");
			Assert.AreEqual (int.MinValue, ex.NativeErrorCode, "#A4");

			ex = new Win32Exception (int.MaxValue, (string) null);
			Assert.AreEqual (-2147467259, ex.ErrorCode, "#B1");
			Assert.IsNull (ex.InnerException, "#B2");
			Assert.IsNotNull (msg, ex.Message, "#B3");
			Assert.IsTrue (ex.Message.IndexOf (ex.GetType ().FullName) != -1, "#B4");
			Assert.AreEqual (int.MaxValue, ex.NativeErrorCode, "#B5");

			ex = new Win32Exception (0, msg);
			Assert.AreEqual (-2147467259, ex.ErrorCode, "#C1");
			Assert.IsNull (ex.InnerException, "#C2");
			Assert.AreSame (msg, ex.Message, "#C3");
			Assert.AreEqual (0, ex.NativeErrorCode, "#C4");

			ex = new Win32Exception (5, string.Empty);
			Assert.AreEqual (-2147467259, ex.ErrorCode, "#C1");
			Assert.IsNull (ex.InnerException, "#C2");
			Assert.IsNotNull (ex.Message, "#C3");
			Assert.AreEqual (string.Empty, ex.Message, "#C4");
			Assert.AreEqual (5, ex.NativeErrorCode, "#C5");
		}

		[Test] // ctor (SerializationInfo, StreamingContext)
		public void Constructor4 ()
		{
			// TODO
		}

#if NET_2_0
		[Test] // ctor (string, Exception)
		public void Constructor5 ()
		{
			Win32Exception ex;
			string msg = "ERROR";
			int native_error = Marshal.GetLastWin32Error ();
			Exception inner = new Exception ();

			ex = new Win32Exception (msg, inner);
			Assert.AreEqual (-2147467259, ex.ErrorCode, "#A1");
			Assert.AreSame (inner, ex.InnerException, "#A2");
			Assert.AreSame (msg, ex.Message, "#A3");
			Assert.AreEqual (native_error, ex.NativeErrorCode, "#A4");

			ex = new Win32Exception ((string) null, inner);
			Assert.AreEqual (-2147467259, ex.ErrorCode, "#B1");
			Assert.AreSame (inner, ex.InnerException, "#B2");
			Assert.IsNotNull (msg, ex.Message, "#B3");
			Assert.AreEqual (new Win32Exception ((string) null).Message, ex.Message, "#B4");
			Assert.AreEqual (native_error, ex.NativeErrorCode, "#B5");

			ex = new Win32Exception (msg, (Exception) null);
			Assert.AreEqual (-2147467259, ex.ErrorCode, "#C1");
			Assert.IsNull (ex.InnerException, "#C2");
			Assert.AreSame (msg, ex.Message, "#C3");
			Assert.AreEqual (native_error, ex.NativeErrorCode, "#C4");

			ex = new Win32Exception (string.Empty, (Exception) null);
			Assert.AreEqual (-2147467259, ex.ErrorCode, "#D1");
			Assert.IsNull (ex.InnerException, "#D2");
			Assert.IsNotNull (ex.Message, "#D3");
			Assert.AreEqual (string.Empty, ex.Message, "#D4");
			Assert.AreEqual (native_error, ex.NativeErrorCode, "#D5");
		}
#endif
	}
}
