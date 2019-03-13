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
		[ExpectedException (typeof (InvalidOperationException))]
		public void DynamicDataExtensions_GetTable_Test2()
		{
			HttpContext.Current = new HttpContext(new FakeHttpWorkerRequest());
			var dds = new LinqDataSource();
			dds.TableName = "test";

			dds.GetTable();
		}

		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void DynamicDataExtensions_GetTable_Test3()
		{
			HttpContext.Current = new HttpContext(new FakeHttpWorkerRequest());
			var dds = new LinqDataSource();

			dds.GetTable();
		}
	}
}
