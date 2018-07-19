//
// DynamicDataRouteTest.cs
//
// Author:
//      Marek Habersack <mhabersack@novell.com>
//
// Copyright (C) 2009 Novell Inc. http://novell.com
//

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
using System.Collections.Generic;
using System.Web.DynamicData;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using NUnit.Framework;

using MonoTests.Common;

namespace MonoTests.System.Web.DynamicData
{
	class FakeWorkerRequest : HttpWorkerRequest
	{
		public override string GetUriPath()
		{
			return "/";
		}

		public override string GetQueryString()
		{
			return "GetQueryString";
		}

		public override string GetRawUrl()
		{
			return "GetRawUrl";
		}

		public override string GetHttpVerbName()
		{
			return "GetVerbName";
		}

		public override string GetHttpVersion()
		{
			return "GetHttpVersion";
		}

		public override string GetRemoteAddress()
		{
			return "__GetRemoteAddress";
		}

		public override int GetRemotePort()
		{
			return 1010;
		}

		public override string GetLocalAddress()
		{
			return "GetLocalAddress";
		}

		public override int GetLocalPort()
		{
			return 2020;
		}

		public override void SendStatus(int s, string x)
		{
		}

		public override void SendKnownResponseHeader(int x, string j)
		{
		}

		public override void SendUnknownResponseHeader(string a, string b)
		{
		}

		public override void SendResponseFromMemory(byte[] arr, int x)
		{
		}

		public override void SendResponseFromFile(string a, long b, long c)
		{
		}

		public override void SendResponseFromFile(IntPtr a, long b, long c)
		{
		}

		public override void FlushResponse(bool x)
		{
		}

		public override void EndOfRequest()
		{
		}
	}
	
	[TestFixture]
	public class DynamicDataExtensionsTest
	{
		[Test]
		public void ConvertEditedValue ()
		{
			FieldFormattingOptions fld = null;

			Assert.Throws<NullReferenceException> (() => {
				fld.ConvertEditedValue (null);
			}, "#A1");

			fld = new FieldFormattingOptions ();
			fld.SetProperty ("ConvertEmptyStringToNull", false);
			Assert.AreEqual (null, fld.ConvertEditedValue (null), "#A2");
			Assert.AreEqual (String.Empty, fld.ConvertEditedValue (String.Empty), "#A2-1");
			Assert.AreEqual ("stuff", fld.ConvertEditedValue ("stuff"), "#A2-2");

			fld.SetProperty ("ConvertEmptyStringToNull", true);
			Assert.AreEqual (null, fld.ConvertEditedValue (null), "#A3");
			Assert.AreEqual (null, fld.ConvertEditedValue (String.Empty), "#A3-1");
			Assert.AreEqual ("stuff", fld.ConvertEditedValue ("stuff"), "#A3-2");

			fld.SetProperty ("ConvertEmptyStringToNull", false);
			fld.SetProperty ("NullDisplayText", "NULL");
			Assert.AreEqual (null, fld.ConvertEditedValue (null), "#A4");
			Assert.AreEqual (String.Empty, fld.ConvertEditedValue (String.Empty), "#A4-1");
			Assert.AreEqual (null, fld.ConvertEditedValue ("NULL"), "#A4-2");
			Assert.AreEqual ("stuff", fld.ConvertEditedValue ("stuff"), "#A4-3");

			fld.SetProperty ("ConvertEmptyStringToNull", false);
			fld.SetProperty ("NullDisplayText", String.Empty);
			Assert.AreEqual (String.Empty, fld.ConvertEditedValue (String.Empty), "#A5");
		}

		[Test]
		public void DynamicDataExtensions_GetTable_Test ()
		{
			IDynamicDataSource dds = null;

			dds.GetTable ();
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void DynamicDataExtensions_GetTable_Test2()
		{
			HttpContext.Current = new HttpContext(new FakeWorkerRequest());
			var dds = new LinqDataSource();
			dds.TableName = "test";

			dds.GetTable();
		}

		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void DynamicDataExtensions_GetTable_Test3()
		{
			HttpContext.Current = new HttpContext(new FakeWorkerRequest());
			var dds = new LinqDataSource();

			dds.GetTable();
		}
	}
}
