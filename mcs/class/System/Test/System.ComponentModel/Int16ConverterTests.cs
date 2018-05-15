//
// System.ComponentModel.Int16Converter test cases
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
	public class Int16ConverterTests
	{
		private Int16Converter converter;
		
		[SetUp]
		public void SetUp ()
		{
			converter = new Int16Converter ();
		}

		[Test]
		public void CanConvertFrom ()
		{
			Assert.IsTrue (converter.CanConvertFrom (typeof (string)), "#1");
			Assert.IsFalse (converter.CanConvertFrom (typeof (short)), "#2");
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
			Assert.AreEqual (short.MinValue, converter.ConvertFrom (null, CultureInfo.InvariantCulture, "#8000"), "#1");
			Assert.AreEqual (short.MinValue, converter.ConvertFrom (null, CultureInfo.InvariantCulture, "#0x8000"), "#2");
			Assert.AreEqual (short.MinValue, converter.ConvertFrom (null, CultureInfo.InvariantCulture, "#0X8000"), "#3");
			Assert.AreEqual (short.MinValue, converter.ConvertFrom (null, CultureInfo.InvariantCulture, "0x8000"), "#4");
			Assert.AreEqual (short.MinValue, converter.ConvertFrom (null, CultureInfo.InvariantCulture, "0X8000"), "#5");
		}

		[Test]
		public void ConvertFrom_MaxValue ()
		{
			Assert.AreEqual (short.MaxValue, converter.ConvertFrom (null, CultureInfo.InvariantCulture, "#7fff"), "#1");
			Assert.AreEqual (short.MaxValue, converter.ConvertFrom (null, CultureInfo.InvariantCulture, "#7FFF"), "#2");
			Assert.AreEqual (short.MaxValue, converter.ConvertFrom (null, CultureInfo.InvariantCulture, "#0x7fff"), "#3");
			Assert.AreEqual (short.MaxValue, converter.ConvertFrom (null, CultureInfo.InvariantCulture, "#0X7FFF"), "#4");
			Assert.AreEqual (short.MaxValue, converter.ConvertFrom (null, CultureInfo.InvariantCulture, "0x7fff"), "#5");
			Assert.AreEqual (short.MaxValue, converter.ConvertFrom (null, CultureInfo.InvariantCulture, "0X7FFF"), "#6");
		}

		[Test]
		public void ConvertToString ()
		{
			CultureInfo culture = new MyCultureInfo ();
			NumberFormatInfo numberFormatInfo = (NumberFormatInfo) culture.GetFormat (typeof (NumberFormatInfo));

			Assert.AreEqual (numberFormatInfo.NegativeSign + "5", converter.ConvertToString (null, culture, (short) -5), "#1");
			Assert.AreEqual (culture.NumberFormat.NegativeSign + "5", converter.ConvertToString (null, culture, (int) -5), "#2");
		}

		[Test]
		public void ConvertFromString ()
		{
			CultureInfo culture = new MyCultureInfo ();
			NumberFormatInfo numberFormatInfo = (NumberFormatInfo) culture.GetFormat (typeof (NumberFormatInfo));

			Assert.AreEqual (-5, converter.ConvertFrom (null, culture, numberFormatInfo.NegativeSign + "5"));
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ConvertFrom_Object ()
		{
			converter.ConvertFrom (new object ());
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ConvertFrom_Int16 ()
		{
			converter.ConvertFrom ((short) 10);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ConvertFrom_Int32 ()
		{
			converter.ConvertFrom (10);
		}

		[Test]
		public void ConvertTo_MinValue ()
		{
			Assert.AreEqual (short.MinValue.ToString (CultureInfo.InvariantCulture),
				converter.ConvertTo (null, CultureInfo.InvariantCulture, short.MinValue,
				typeof (string)), "#1");
			Assert.AreEqual (short.MinValue.ToString (CultureInfo.CurrentCulture),
				converter.ConvertTo (null, CultureInfo.CurrentCulture, short.MinValue,
				typeof (string)), "#2");
			Assert.AreEqual (short.MinValue.ToString (CultureInfo.CurrentCulture),
				converter.ConvertTo (short.MinValue, typeof (string)), "#3");
		}

		[Test]
		public void ConvertTo_MaxValue ()
		{
			Assert.AreEqual (short.MaxValue.ToString (CultureInfo.InvariantCulture),
				converter.ConvertTo (null, CultureInfo.InvariantCulture, short.MaxValue,
				typeof (string)), "#1");
			Assert.AreEqual (short.MaxValue.ToString (CultureInfo.CurrentCulture),
				converter.ConvertTo (null, CultureInfo.CurrentCulture, short.MaxValue,
				typeof (string)), "#2");
			Assert.AreEqual (short.MaxValue.ToString (CultureInfo.CurrentCulture),
				converter.ConvertTo (short.MaxValue, typeof (string)), "#3");
		}

		[Test]
		public void ConvertToString_MinValue ()
		{
			Assert.AreEqual (short.MinValue.ToString (CultureInfo.InvariantCulture),
				converter.ConvertToString (null, CultureInfo.InvariantCulture,
				short.MinValue), "#1");

			Assert.AreEqual (short.MinValue.ToString (CultureInfo.CurrentCulture),
				converter.ConvertToString (null, short.MinValue), "#2");
			Assert.AreEqual (short.MinValue.ToString (CultureInfo.CurrentCulture),
				converter.ConvertToString (null, CultureInfo.CurrentCulture,
				short.MinValue), "#3");
			Assert.AreEqual (short.MinValue.ToString (CultureInfo.CurrentCulture),
				converter.ConvertToString (short.MinValue), "#4");
		}

		[Test]
		public void ConvertToString_MaxValue ()
		{
			Assert.AreEqual (short.MaxValue.ToString (CultureInfo.InvariantCulture),
				converter.ConvertToString (null, CultureInfo.InvariantCulture,
				short.MaxValue), "#1");

			Assert.AreEqual (short.MaxValue.ToString (CultureInfo.CurrentCulture),
				converter.ConvertToString (null, short.MaxValue), "#2");
			Assert.AreEqual (short.MaxValue.ToString (CultureInfo.CurrentCulture),
				converter.ConvertToString (null, CultureInfo.CurrentCulture,
				short.MaxValue), "#3");
			Assert.AreEqual (short.MaxValue.ToString (CultureInfo.CurrentCulture),
				converter.ConvertToString (short.MaxValue), "#4");
		}

		[Test]
		public void ConvertFrom_InvalidValue ()
		{
			try {
				converter.ConvertFrom ("*1");
				Assert.Fail ("#1");
			} catch (AssertionException) {
				throw;
			} catch (Exception ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.AreEqual (typeof (FormatException), ex.InnerException.GetType (), "#3");
			}
		}

		[Test]
		public void ConvertFrom_InvalidValue_Invariant ()
		{
			try {
				converter.ConvertFrom (null, CultureInfo.InvariantCulture, "*1");
				Assert.Fail ("#1");
			} catch (AssertionException) {
				throw;
			} catch (Exception ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.AreEqual (typeof (FormatException), ex.InnerException.GetType (), "#3");
			}
		}

		[Test]
		public void ConvertFrom_Base10_MinOverflow ()
		{
			string minOverflow = ((int) (short.MinValue - 1)).ToString (
				CultureInfo.CurrentCulture);

			try {
				converter.ConvertFrom (minOverflow);
				Assert.Fail ("#1");
			} catch (AssertionException) {
				throw;
			} catch (Exception ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.AreEqual (typeof (OverflowException), ex.InnerException.GetType (), "#3");
			}
		}

		[Test]
		public void ConvertFrom_Base10_MinOverflow_Invariant ()
		{
			string minOverflow = ((int) (short.MinValue - 1)).ToString (
				CultureInfo.InvariantCulture);

			try {
				converter.ConvertFrom (null, CultureInfo.InvariantCulture,
					minOverflow);
				Assert.Fail ("#1");
			} catch (AssertionException) {
				throw;
			} catch (Exception ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.AreEqual (typeof (OverflowException), ex.InnerException.GetType (), "#3");
			}
		}

		[Test]
		public void ConvertFrom_Base10_MaxOverflow ()
		{
			string maxOverflow = ((int) (short.MaxValue + 1)).ToString (
				CultureInfo.CurrentCulture);

			try {
				converter.ConvertFrom (maxOverflow);
				Assert.Fail ("#1");
			} catch (AssertionException) {
				throw;
			} catch (Exception ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.AreEqual (typeof (OverflowException), ex.InnerException.GetType (), "#3");
			}
		}

		[Test]
		public void ConvertFrom_Base10_MaxOverflow_Invariant ()
		{
			string maxOverflow = ((int) (short.MaxValue + 1)).ToString (
				CultureInfo.InvariantCulture);

			try {
				converter.ConvertFrom (null, CultureInfo.InvariantCulture,
					maxOverflow);
				Assert.Fail ("#1");
			} catch (AssertionException) {
				throw;
			} catch (Exception ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.AreEqual (typeof (OverflowException), ex.InnerException.GetType (), "#3");
			}
		}

		[Test]
		public void ConvertFrom_Base16_MinOverflow ()
		{
			string minOverflow = ((int) (short.MinValue - 1)).ToString ("x",
				CultureInfo.CurrentCulture);

			try {
				converter.ConvertFrom ("#" + minOverflow);
				Assert.Fail ("#1");
			} catch (AssertionException) {
				throw;
			} catch (Exception ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.AreEqual (typeof (OverflowException), ex.InnerException.GetType (), "#3");
			}
		}

		[Test]
		public void ConvertFrom_Base16_MinOverflow_Invariant ()
		{
			string minOverflow = ((int) (short.MinValue - 1)).ToString ("x",
				CultureInfo.InvariantCulture);

			try {
				converter.ConvertFrom (null, CultureInfo.InvariantCulture,
					"#" + minOverflow);
				Assert.Fail ("#1");
			} catch (AssertionException) {
				throw;
			} catch (Exception ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.AreEqual (typeof (OverflowException), ex.InnerException.GetType (), "#4");
			}
		}

		[Test]
		public void ConvertFrom_Base16_MaxOverflow ()
		{
			string maxOverflow = ((int) (short.MaxValue + 1)).ToString ("x",
				CultureInfo.CurrentCulture);
			Assert.AreEqual (-32768, converter.ConvertFrom (null, CultureInfo.CurrentCulture,
				"#" + maxOverflow), "#1");

			maxOverflow = ((int) (ushort.MaxValue)).ToString ("x",
				CultureInfo.CurrentCulture);
			Assert.AreEqual (-1, converter.ConvertFrom (null, CultureInfo.CurrentCulture,
				"#" + maxOverflow), "#2");

			maxOverflow = (ushort.MaxValue + 1).ToString ("x",
				CultureInfo.CurrentCulture);

			try {
				converter.ConvertFrom (null, CultureInfo.CurrentCulture,
					"#" + maxOverflow);
				Assert.Fail ("#3");
			} catch (AssertionException) {
				throw;
			} catch (Exception ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#4");
				Assert.IsNotNull (ex.InnerException, "#5");
				Assert.AreEqual (typeof (OverflowException), ex.InnerException.GetType (), "#6");
			}
		}

		[Test]
		public void ConvertFrom_Base16_MaxOverflow_Invariant ()
		{
			string maxOverflow = ((int) (short.MaxValue + 1)).ToString ("x",
				CultureInfo.InvariantCulture);
			Assert.AreEqual (-32768, converter.ConvertFrom (null, CultureInfo.InvariantCulture,
				"#" + maxOverflow), "#1");

			maxOverflow = ((int) (ushort.MaxValue)).ToString ("x",
				CultureInfo.InvariantCulture);
			Assert.AreEqual (-1, converter.ConvertFrom (null, CultureInfo.InvariantCulture,
				"#" + maxOverflow), "#2");

			maxOverflow = (ushort.MaxValue + 1).ToString ("x",
				CultureInfo.InvariantCulture);

			try {
				converter.ConvertFrom (null, CultureInfo.InvariantCulture,
					"#" + maxOverflow);
				Assert.Fail ("#3");
			} catch (AssertionException) {
				throw;
			} catch (Exception ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#4");
				Assert.IsNotNull (ex.InnerException, "#5");
				Assert.AreEqual (typeof (OverflowException), ex.InnerException.GetType (), "#6");
			}
		}

		[Test]
		public void ConvertFromString_InvalidValue ()
		{
			try {
				converter.ConvertFromString ("*1");
				Assert.Fail ("#1");
			} catch (AssertionException) {
				throw;
			} catch (Exception ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.AreEqual (typeof (FormatException), ex.InnerException.GetType (), "#3");
			}
		}

		[Test]
		public void ConvertFromString_InvalidValue_Invariant ()
		{
			try {
				converter.ConvertFromString (null, CultureInfo.InvariantCulture, "*1");
				Assert.Fail ("#1");
			} catch (AssertionException) {
				throw;
			} catch (Exception ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.AreEqual (typeof (FormatException), ex.InnerException.GetType (), "#3");
			}
		}

		[Test]
		public void ConvertFromString_Base10_MinOverflow ()
		{
			string minOverflow = ((int) (short.MinValue - 1)).ToString (
				CultureInfo.CurrentCulture);

			try {
				converter.ConvertFromString (minOverflow);
				Assert.Fail ("#1");
			} catch (AssertionException) {
				throw;
			} catch (Exception ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.AreEqual (typeof (OverflowException), ex.InnerException.GetType (), "#3");
			}
		}

		[Test]
		public void ConvertFromString_Base10_MinOverflow_Invariant ()
		{
			string minOverflow = ((int) (short.MinValue - 1)).ToString (
				CultureInfo.InvariantCulture);

			try {
				converter.ConvertFromString (null, CultureInfo.InvariantCulture,
					minOverflow);
				Assert.Fail ("#1");
			} catch (AssertionException) {
				throw;
			} catch (Exception ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.AreEqual (typeof (OverflowException), ex.InnerException.GetType (), "#3");
			}
		}

		[Test]
		public void ConvertFromString_Base10_MaxOverflow ()
		{
			string maxOverflow = ((int) (short.MaxValue + 1)).ToString (
				CultureInfo.CurrentCulture);

			try {
				converter.ConvertFromString (maxOverflow);
				Assert.Fail ("#1");
			} catch (AssertionException) {
				throw;
			} catch (Exception ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.AreEqual (typeof (OverflowException), ex.InnerException.GetType (), "#3");
			}
		}

		[Test]
		public void ConvertFromString_Base10_MaxOverflow_Invariant ()
		{
			string maxOverflow = ((int) (short.MaxValue + 1)).ToString (
				CultureInfo.InvariantCulture);

			try {
				converter.ConvertFromString (null, CultureInfo.InvariantCulture,
					maxOverflow);
				Assert.Fail ("#1");
			} catch (AssertionException) {
				throw;
			} catch (Exception ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.AreEqual (typeof (OverflowException), ex.InnerException.GetType (), "#3");
			}
		}

		[Test]
		public void ConvertFromString_Base16_MinOverflow ()
		{
			string minOverflow = ((int) (short.MinValue - 1)).ToString ("x",
				CultureInfo.CurrentCulture);

			try {
				converter.ConvertFromString ("#" + minOverflow);
				Assert.Fail ("#1");
			} catch (AssertionException) {
				throw;
			} catch (Exception ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.AreEqual (typeof (OverflowException), ex.InnerException.GetType (), "#3");
			}
		}

		[Test]
		public void ConvertFromString_Base16_MinOverflow_Invariant ()
		{
			string minOverflow = ((int) (short.MinValue - 1)).ToString ("x",
				CultureInfo.InvariantCulture);

			try {
				converter.ConvertFromString (null, CultureInfo.InvariantCulture,
					"#" + minOverflow);
				Assert.Fail ("#1");
			} catch (AssertionException) {
				throw;
			} catch (Exception ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.AreEqual (typeof (OverflowException), ex.InnerException.GetType (), "#3");
			}
		}

		[Test]
		public void ConvertFromString_Base16_MaxOverflow ()
		{
			string maxOverflow = ((int) (short.MaxValue + 1)).ToString ("x",
				CultureInfo.CurrentCulture);
			Assert.AreEqual (-32768, converter.ConvertFromString ("#" + maxOverflow), "#1");

			maxOverflow = (ushort.MaxValue).ToString ("x",
				CultureInfo.CurrentCulture);
			Assert.AreEqual (-1, converter.ConvertFromString ("#" + maxOverflow), "#2");

			maxOverflow = (ushort.MaxValue + 1).ToString ("x",
				CultureInfo.CurrentCulture);

			try {
				converter.ConvertFromString ("#" + maxOverflow);
				Assert.Fail ("#3");
			} catch (AssertionException) {
				throw;
			} catch (Exception ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#4");
				Assert.IsNotNull (ex.InnerException, "#5");
				Assert.AreEqual (typeof (OverflowException), ex.InnerException.GetType (), "#6");
			}
		}

		[Test]
		public void ConvertFromString_Base16_MaxOverflow_Invariant ()
		{
			string maxOverflow = ((int) (short.MaxValue + 1)).ToString ("x",
				CultureInfo.CurrentCulture);
			Assert.AreEqual (-32768, converter.ConvertFromString ("#" + maxOverflow), "#1");

			maxOverflow = (ushort.MaxValue + 1).ToString ("x",
				CultureInfo.CurrentCulture);

			try {
				converter.ConvertFromString (null, CultureInfo.InvariantCulture,
					"#" + maxOverflow);
				Assert.Fail ("#2");
			} catch (AssertionException) {
				throw;
			} catch (Exception ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#3");
				Assert.IsNotNull (ex.InnerException, "#4");
				Assert.AreEqual (typeof (OverflowException), ex.InnerException.GetType (), "#5");
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

// adding this override in 1.x shows different result in .NET (it is ignored).
// Some compatibility kids might want to fix this issue.
			public override NumberFormatInfo NumberFormat {
				get {
					NumberFormatInfo nfi = (NumberFormatInfo) base.NumberFormat.Clone ();
					nfi.NegativeSign = "myNegativeSign";
					return nfi;
				}
				set { throw new NotSupportedException (); }
			}
		}
	}
}
