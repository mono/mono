//
// Authors:
//	Marek Habersack <mhabersack@novell.com>
//
// Copyright (C) 2010 Novell, Inc (http://novell.com)
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
#if NET_2_0
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Text;
using System.Web.UI;

using NUnit.Framework;
using MonoTests.Common;

namespace MonoTests.System.Web.UI
{
	[TestFixture]
	public class DataSourceCacheDurationConverterTest
	{
		[Test]
		public void CanConvertFrom ()
		{
			var cvt = new DataSourceCacheDurationConverter ();
			Assert.IsTrue (cvt.CanConvertFrom (null, typeof (string)), "#A1-1");
			Assert.IsFalse (cvt.CanConvertFrom (null, null), "#A1-2");
			Assert.IsFalse (cvt.CanConvertFrom (null, typeof (uint)), "#A1-3");
			Assert.IsFalse (cvt.CanConvertFrom (null, typeof (int)), "#A1-4");
			Assert.IsFalse (cvt.CanConvertFrom (null, typeof (float)), "#A1-5");
			Assert.IsFalse (cvt.CanConvertFrom (null, typeof (short)), "#A1-6");
			Assert.IsFalse (cvt.CanConvertFrom (null, typeof (double)), "#A1-7");
			Assert.IsTrue (cvt.CanConvertFrom (null, typeof (InstanceDescriptor)), "#A1-8");
			Assert.IsFalse (cvt.CanConvertFrom (null, typeof (decimal)), "#A1-9");
		}

		[Test]
		public void CanConvertTo ()
		{
			var cvt = new DataSourceCacheDurationConverter ();
			Assert.IsTrue (cvt.CanConvertTo (null, typeof (uint)), "#A1-1");
			Assert.IsTrue (cvt.CanConvertTo (null, typeof (string)), "#A1-2");
			Assert.IsFalse (cvt.CanConvertTo (null, typeof (InstanceDescriptor)), "#A1-3");
			Assert.IsTrue (cvt.CanConvertTo (null, typeof (int)), "#A1-4");
			Assert.IsTrue (cvt.CanConvertTo (null, typeof (float)), "#A1-5");
			Assert.IsTrue (cvt.CanConvertTo (null, typeof (short)), "#A1-6");
			Assert.IsTrue (cvt.CanConvertTo (null, typeof (double)), "#A1-7");
			Assert.IsFalse (cvt.CanConvertTo (null, typeof (decimal)), "#A1-8");
		}

		[Test]
		public void ConvertFrom ()
		{
			var cvt = new DataSourceCacheDurationConverter ();
			Assert.AreEqual (null, cvt.ConvertFrom (null, null, null), "#A1-1");
			Assert.AreEqual (0, cvt.ConvertFrom (null, null, String.Empty), "#A1-2");
			Assert.AreEqual (0, cvt.ConvertFrom (null, null, "infinite"), "#A1-3");
			Assert.AreEqual (0, cvt.ConvertFrom (null, null, "INfINiTE"), "#A1-4");
			AssertExtensions.Throws<Exception> (() => {
				cvt.ConvertFrom (null, null, "dummy");
			}, "#A1-5");
			Assert.AreEqual (5, cvt.ConvertFrom (null, null, "5"), "#A1-6");
			Assert.AreEqual (-5, cvt.ConvertFrom (null, null, "-5"), "#A1-7");

			Assert.AreEqual (typeof (Int32), cvt.ConvertFrom (null, null, "5").GetType (), "#A2-1");
			Assert.AreEqual (typeof (Int32), cvt.ConvertFrom (null, null, "infinite").GetType (), "#A2-2");
		}

		[Test]
		public void ConvertTo ()
		{
			var cvt = new DataSourceCacheDurationConverter ();
			Assert.AreEqual ("Infinite", cvt.ConvertTo (null, null, 0, typeof (string)), "#A1-1");
			Assert.AreEqual (0, cvt.ConvertTo (null, null, 0, typeof (int)), "#A1-2");
			Assert.AreEqual (0.0f, cvt.ConvertTo (null, null, 0, typeof (float)), "#A1-3");
			Assert.AreEqual (String.Empty, cvt.ConvertTo (null, null, null, typeof (string)), "#A1-4");
			Assert.AreEqual ("10", cvt.ConvertTo (null, null, 10, typeof (string)), "#A1-5");
		}

		[Test]
		public void GetStandardValues ()
		{
			var cvt = new DataSourceCacheDurationConverter ();
			TypeConverter.StandardValuesCollection coll = cvt.GetStandardValues (null);
			Assert.IsNotNull (coll, "#A1-1");
			Assert.AreEqual (1, coll.Count, "#A1-2");

			Assert.AreEqual (0, coll [0], "#A2-1");
			Assert.AreEqual (typeof (int), coll [0].GetType (), "#A2-2");
		}

		[Test]
		public void GetStandardValuesExclusive ()
		{
			var cvt = new DataSourceCacheDurationConverter ();
			Assert.IsFalse (cvt.GetStandardValuesExclusive (null), "#A1");
		}

		[Test]
		public void GetStandardValuesSupported ()
		{
			var cvt = new DataSourceCacheDurationConverter ();
			Assert.IsTrue (cvt.GetStandardValuesSupported (null), "#A1");
		}
	}
}
#endif