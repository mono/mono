//
// System.ComponentModel.Int32Converter test cases
//
// Authors:
// 	Gert Driesen (drieseng@users.sourceforge.net)
//
// (c) 2005 Novell, Inc. (http://www.ximian.com)
//

using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;

using NUnit.Framework;

namespace MonoTests.System.ComponentModel
{
	[TestFixture]
	public class Int32ConverterTests
	{
		private Int32Converter converter;
		
		[SetUp]
		public void SetUp ()
		{
			converter = new Int32Converter ();
		}

		[Test]
		public void CanConvertFrom ()
		{
			Assert.IsTrue (converter.CanConvertFrom (typeof (string)), "#1");
			Assert.IsFalse (converter.CanConvertFrom (typeof (int)), "#2");
			Assert.IsFalse (converter.CanConvertFrom (typeof (object)), "#3");
			Assert.IsTrue (converter.CanConvertFrom (typeof (InstanceDescriptor)), "#4");
		}

		[Test]
		public void CanConvertTo ()
		{
			Assert.IsTrue (converter.CanConvertTo (typeof (string)), "#1");
			Assert.IsFalse (converter.CanConvertTo (typeof (object)), "#2");
			Assert.IsTrue (converter.CanConvertTo (typeof (int)), "#3");
		}

		[Test]
		public void ConvertFrom_MinValue ()
		{
			Assert.AreEqual (int.MinValue, converter.ConvertFrom (null, CultureInfo.InvariantCulture, "#80000000"), "#1");
			Assert.AreEqual (int.MinValue, converter.ConvertFrom (null, CultureInfo.InvariantCulture, "#0x80000000"), "#2");
			Assert.AreEqual (int.MinValue, converter.ConvertFrom (null, CultureInfo.InvariantCulture, "#0X80000000"), "#3");
			Assert.AreEqual (int.MinValue, converter.ConvertFrom (null, CultureInfo.InvariantCulture, "0x80000000"), "#4");
			Assert.AreEqual (int.MinValue, converter.ConvertFrom (null, CultureInfo.InvariantCulture, "0X80000000"), "#5");
		}

		[Test]
		public void ConvertFrom_MaxValue ()
		{
			Assert.AreEqual (int.MaxValue, converter.ConvertFrom (null, CultureInfo.InvariantCulture, "#7fffffff"), "#1");
			Assert.AreEqual (int.MaxValue, converter.ConvertFrom (null, CultureInfo.InvariantCulture, "#7FFFFFFF"), "#2");
			Assert.AreEqual (int.MaxValue, converter.ConvertFrom (null, CultureInfo.InvariantCulture, "#0x7fffffff"), "#3");
			Assert.AreEqual (int.MaxValue, converter.ConvertFrom (null, CultureInfo.InvariantCulture, "#0X7FFFFFFF"), "#4");
			Assert.AreEqual (int.MaxValue, converter.ConvertFrom (null, CultureInfo.InvariantCulture, "0x7fffffff"), "#5");
			Assert.AreEqual (int.MaxValue, converter.ConvertFrom (null, CultureInfo.InvariantCulture, "0X7FFFFFFF"), "#6");
		}

    [Test]
    public void IsValid ()
    {
      Assert.IsTrue (converter.IsValid("1"));
      Assert.IsTrue (converter.IsValid("545"));
      Assert.IsFalse (converter.IsValid("fred"));
    }
    
		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ConvertFrom_Object ()
		{
			converter.ConvertFrom (new object ());
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ConvertFrom_Int32 ()
		{
			converter.ConvertFrom (int.MaxValue);
		}

		[Test]
		public void ConvertTo_MinValue ()
		{
			Assert.AreEqual (int.MinValue.ToString (CultureInfo.InvariantCulture),
				converter.ConvertTo (null, CultureInfo.InvariantCulture, int.MinValue,
				typeof (string)), "#1");
			Assert.AreEqual (int.MinValue.ToString (CultureInfo.CurrentCulture),
				converter.ConvertTo (null, CultureInfo.CurrentCulture, int.MinValue,
				typeof (string)), "#2");
			Assert.AreEqual (int.MinValue.ToString (CultureInfo.CurrentCulture),
				converter.ConvertTo (int.MinValue, typeof (string)), "#3");
		}

		[Test]
		public void ConvertTo_MaxValue ()
		{
			Assert.AreEqual (int.MaxValue.ToString (CultureInfo.InvariantCulture),
				converter.ConvertTo (null, CultureInfo.InvariantCulture, int.MaxValue,
				typeof (string)), "#1");
			Assert.AreEqual (int.MaxValue.ToString (CultureInfo.CurrentCulture),
				converter.ConvertTo (null, CultureInfo.CurrentCulture, int.MaxValue,
				typeof (string)), "#2");
			Assert.AreEqual (int.MaxValue.ToString (CultureInfo.CurrentCulture),
				converter.ConvertTo (int.MaxValue, typeof (string)), "#3");
		}

		[Test]
		public void ConvertToString ()
		{
			CultureInfo culture = new MyCultureInfo ();
			NumberFormatInfo numberFormatInfo = (NumberFormatInfo) culture.GetFormat (typeof (NumberFormatInfo));

			Assert.AreEqual (numberFormatInfo.NegativeSign + "5", converter.ConvertToString (null, culture, -5));
		}

		[Test]
		public void ConvertFromString ()
		{
			CultureInfo culture = new MyCultureInfo ();
			NumberFormatInfo numberFormatInfo = (NumberFormatInfo) culture.GetFormat (typeof (NumberFormatInfo));

			Assert.AreEqual (-5, converter.ConvertFrom (null, culture, numberFormatInfo.NegativeSign + "5"));
		}

		[Test]
		public void ConvertFromString_Invalid1 ()
		{
			try {
				converter.ConvertFromString (null, CultureInfo.InvariantCulture, "*1");
				Assert.Fail ("#1");
			} catch (Exception ex) {
				Assert.AreEqual (typeof (Exception), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.AreEqual (typeof (FormatException), ex.InnerException.GetType (), "#3");
			}
		}

		[Test]
		public void ConvertFromString_Invalid2 ()
		{
			try {
				converter.ConvertFromString (null, CultureInfo.InvariantCulture,
					double.MaxValue.ToString(CultureInfo.InvariantCulture));
				Assert.Fail ("#1");
			} catch (Exception ex) {
				Assert.AreEqual (typeof (Exception), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.AreEqual (typeof (FormatException), ex.InnerException.GetType (), "#3");
			}
		}

		[Test]
		public void ConvertFromString_Invalid3 ()
		{
			try {
				converter.ConvertFromString ("*1");
				Assert.Fail ("#1");
			} catch (Exception ex) {
				Assert.AreEqual (typeof (Exception), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.AreEqual (typeof (FormatException), ex.InnerException.GetType (), "#3");
			}
		}

		[Test]
		public void ConvertFromString_Invalid4 ()
		{
			try {
				converter.ConvertFromString (double.MaxValue.ToString (CultureInfo.CurrentCulture));
				Assert.Fail ("#1");
			} catch (Exception ex) {
				Assert.AreEqual (typeof (Exception), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.AreEqual (typeof (FormatException), ex.InnerException.GetType (), "#3");
			}
		}

		[Test]
		public void ConvertFrom_InvalidString1 ()
		{
			try {
				converter.ConvertFrom (null, CultureInfo.InvariantCulture, "*1");
				Assert.Fail ("#1");
			} catch (Exception ex) {
				Assert.AreEqual (typeof (Exception), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.AreEqual (typeof (FormatException), ex.InnerException.GetType (), "#3");
			}
		}

		[Test]
		public void ConvertFrom_InvalidString2 ()
		{
			try {
				converter.ConvertFrom (null, CultureInfo.InvariantCulture,
					double.MaxValue.ToString (CultureInfo.InvariantCulture));
				Assert.Fail ("#1");
			} catch (Exception ex) {
				Assert.AreEqual (typeof (Exception), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.AreEqual (typeof (FormatException), ex.InnerException.GetType (), "#3");
			}
		}

		[Test]
		public void ConvertFrom_InvalidString3 ()
		{
			try {
				converter.ConvertFrom ("*1");
				Assert.Fail ("#1");
			} catch (Exception ex) {
				Assert.AreEqual (typeof (Exception), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.AreEqual (typeof (FormatException), ex.InnerException.GetType (), "#3");
			}
		}

		[Test]
		public void ConvertFrom_InvalidString4 ()
		{
			try {
				converter.ConvertFrom (double.MaxValue.ToString (CultureInfo.CurrentCulture));
				Assert.Fail ("#1");
			} catch (Exception ex) {
				Assert.AreEqual (typeof (Exception), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.AreEqual (typeof (FormatException), ex.InnerException.GetType (), "#3");
			}
		}

		[Serializable]
		private sealed class MyCultureInfo : CultureInfo
		{
			internal MyCultureInfo ()
				: base ("en-US")
			{
			}

			public override object GetFormat (Type formatType)
			{
				if (formatType == typeof (NumberFormatInfo)) {
					NumberFormatInfo nfi = (NumberFormatInfo) ((NumberFormatInfo) base.GetFormat (formatType)).Clone ();

					nfi.NegativeSign = "myNegativeSign";
					return NumberFormatInfo.ReadOnly (nfi);
				} else {
					return base.GetFormat (formatType);
				}
			}
		}
	}
}

