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
// Copyright (c) 2004 Novell, Inc.
//
// Authors:
//	Ravindra (rkumar@novell.com)
//
// $Revision: 1.1 $
// $Modtime: $
// $Log: TestImageIndexConverter.cs,v $
// Revision 1.1  2004/08/27 22:17:37  ravindra
// Adding test for ImageIndexConverter.cs
//
//
//

using NUnit.Framework;
using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Globalization;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]	
	public class ImageIndexConverterTest
	{
		ToolBarButton button;
		PropertyDescriptorCollection pdc;
		ImageIndexConverter ic;

		public ImageIndexConverterTest ()
		{
			button = new ToolBarButton ();
			pdc = TypeDescriptor.GetProperties (button);
			ic = (ImageIndexConverter) pdc.Find ("ImageIndex", true).Converter;
		}

		[TearDown]
		public void TearDown () { }

		[SetUp]
		public void SetUp () { }

		[Test]
		public void TestCanConvertFrom ()
		{
			Assert.IsFalse (ic.CanConvertFrom (typeof (byte)), "CanConvertFromByte must be false.");
			Assert.IsFalse (ic.CanConvertFrom (typeof (char)), "CanConvertFromChar must be false.");
			Assert.IsFalse (ic.CanConvertFrom (typeof (int)), "CanConvertFromInt must be false.");
			Assert.IsFalse (ic.CanConvertFrom (typeof (float)), "CanConvertFromFloat must be false.");
			Assert.IsFalse (ic.CanConvertFrom (typeof (long)), "CanConvertFromLong must be false.");
			Assert.IsFalse (ic.CanConvertFrom (typeof (double)), "CanConvertFromDouble must be false.");
			Assert.IsTrue (ic.CanConvertFrom (typeof (string)), "CanConvertFromString must be true.");
			Assert.IsFalse (ic.CanConvertFrom (typeof (object)), "CanConvertFromObject must be false.");
		}

		[Test]
		public void TestCanConvertTo ()
		{
			Assert.IsTrue (ic.CanConvertTo (typeof (byte)), "CanConvertToByte must be true.");
			Assert.IsTrue (ic.CanConvertTo (typeof (char)), "CanConvertToChar must be true.");
			Assert.IsTrue (ic.CanConvertTo (typeof (int)), "CanConvertToInt must be true.");
			Assert.IsTrue (ic.CanConvertTo (typeof (float)), "CanConvertToFloat must be true.");
			Assert.IsTrue (ic.CanConvertTo (typeof (long)), "CanConvertToLong must be true.");
			Assert.IsTrue (ic.CanConvertTo (typeof (double)), "CanConvertToDouble must be true.");
			Assert.IsTrue (ic.CanConvertTo (typeof (string)), "CanConvertToString must be true.");
			Assert.IsFalse (ic.CanConvertTo (typeof (object)), "CanConvertToObject must be false.");
		}

		[Test]
		public void TestConvertFrom ()
		{
			Assert.AreEqual (-12, (int) ic.ConvertFrom (null, CultureInfo.InvariantCulture, "-12"), "ConvertFromStr -12");
			Assert.AreEqual (-1, (int) ic.ConvertFrom (null, CultureInfo.InvariantCulture, "-1"), "ConvertFromStr -1");
			Assert.AreEqual (1, (int) ic.ConvertFrom (null, CultureInfo.InvariantCulture, "1"), "ConvertFromStr 1");

			try {
				ic.ConvertFrom (null, CultureInfo.InvariantCulture, 1.2f);
				Assert.Fail ("ConvertFromFloat did not throw exception.");
			} catch (Exception e) {
				Assert.IsTrue (e is NotSupportedException, "ConvertFromFloat did not throw NotSupportedException.");
			}

			try {
				ic.ConvertFrom (null, CultureInfo.InvariantCulture, 1);
				Assert.Fail ("ConvertFromInt did not throw exception.");
			} catch (Exception e) {
				Assert.IsTrue (e is NotSupportedException, "ConvertFromInt did not throw NotSupportedException.");
			}
		}

		[Test]
		public void TestConvertTo ()
		{
			Assert.AreEqual ("(none)", ic.ConvertTo (null, CultureInfo.InvariantCulture, -1, typeof (string)), "ConvertInt_Minus1_ToStr");
			Assert.AreEqual ("0", ic.ConvertTo (null, CultureInfo.InvariantCulture, 0, typeof (string)), "ConvertInt_0_ToStr");
			Assert.AreEqual ("1", ic.ConvertTo (null, CultureInfo.InvariantCulture, 1, typeof (string)), "ConvertInt_1_ToStr");
			Assert.AreEqual (0, ic.ConvertTo (null, CultureInfo.InvariantCulture, 0, typeof (int)), "ConvertInt_0_ToInt");
			Assert.AreEqual ("(none)", ic.ConvertTo (null, CultureInfo.InvariantCulture, "(none)", typeof (string)), "ConvertStr_none_ToStr");
			Assert.AreEqual ("-1", ic.ConvertTo (null, CultureInfo.InvariantCulture, "-1", typeof (string)), "ConvertStr_Minus1_ToStr");
			Assert.AreEqual (-1, ic.ConvertTo (null, CultureInfo.InvariantCulture, -1, typeof (int)), "ConvertInt_Minus1_ToInt");
			Assert.AreEqual (1, ic.ConvertTo (null, CultureInfo.InvariantCulture, 1, typeof (int)), "ConvertInt_1_ToInt");
			Assert.AreEqual (-1, ic.ConvertTo (null, CultureInfo.InvariantCulture, "-1", typeof (int)), "ConvertStr_Minus1_ToInt");
			Assert.AreEqual (0, ic.ConvertTo (null, CultureInfo.InvariantCulture, "0", typeof (int)), "ConvertStr_0_ToInt");
			Assert.AreEqual (1, ic.ConvertTo (null, CultureInfo.InvariantCulture, "1", typeof (int)), "ConvertStr_1_ToInt");

			Assert.AreEqual (2, ic.ConvertTo (null, CultureInfo.InvariantCulture, 1.5f, typeof (int)), "ConvertFloat_1_5_ToInt must return 2.");

			try {
				ic.ConvertTo (null, CultureInfo.InvariantCulture, "-1.5f", typeof (int));
				Assert.Fail("ConvertFloatStrToInt must throw exception.");
			} catch (Exception e) {
				Assert.IsTrue (e is FormatException, "ConvertFloatStrToInt must throw FormatException.");
			}

			Assert.AreEqual (1.5, ic.ConvertTo (null, CultureInfo.InvariantCulture, 1.5f, typeof (float)), "ConvertFloat_1_5_ToFloat");
		}

		[Test]
		public void TestGetCreateInstanceSupported ()
		{
			Assert.IsFalse (ic.GetCreateInstanceSupported (), "GetCreateInstance must return false.");
		}

		[Test]
		public void TestGetStandardValuesSupported ()
		{
			Assert.IsTrue (ic.GetStandardValuesSupported (), "GetStandardValuesSupported must return true.");
		}

		[Test]
		public void TestGetStandardValuesExclusive ()
		{
			Assert.IsFalse (ic.GetStandardValuesExclusive (), "GetStandardValuesExclusive must return false.");
		}

		[Test]
		public void TestGetStandardValues ()
		{
			TypeConverter.StandardValuesCollection stdVals = ic.GetStandardValues (null);
			Assert.AreEqual (1, stdVals.Count, "StandardValues count must be 1.");
			Assert.AreEqual (-1, stdVals [0], "Standard Value count must be -1.");
		}
	}
}
