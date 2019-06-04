//
// System.ComponentModel.ByteConverter test cases
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
	public class ByteConverterTests
	{
		private ByteConverter converter;
		
		[SetUp]
		public void SetUp ()
		{
			converter = new ByteConverter ();
		}

		[Test]
		public void CanConvertFrom ()
		{
			Assert.IsTrue (converter.CanConvertFrom (typeof (string)), "#1");
			Assert.IsFalse (converter.CanConvertFrom (typeof (byte)), "#2");
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
			Assert.AreEqual (byte.MinValue, converter.ConvertFrom (null, CultureInfo.InvariantCulture, "#0"), "#1");
			Assert.AreEqual (byte.MinValue, converter.ConvertFrom (null, CultureInfo.InvariantCulture, "#0x0"), "#2");
			Assert.AreEqual (byte.MinValue, converter.ConvertFrom (null, CultureInfo.InvariantCulture, "#0X0"), "#3");
			Assert.AreEqual (byte.MinValue, converter.ConvertFrom (null, CultureInfo.InvariantCulture, "0x0"), "#4");
			Assert.AreEqual (byte.MinValue, converter.ConvertFrom (null, CultureInfo.InvariantCulture, "0X0"), "#5");
		}

		[Test]
		public void ConvertFrom_MaxValue ()
		{
			Assert.AreEqual (byte.MaxValue, converter.ConvertFrom (null, CultureInfo.InvariantCulture, "#ff"), "#1");
			Assert.AreEqual (byte.MaxValue, converter.ConvertFrom (null, CultureInfo.InvariantCulture, "#FF"), "#2");
			Assert.AreEqual (byte.MaxValue, converter.ConvertFrom (null, CultureInfo.InvariantCulture, "#0xff"), "#3");
			Assert.AreEqual (byte.MaxValue, converter.ConvertFrom (null, CultureInfo.InvariantCulture, "#0XFF"), "#4");
			Assert.AreEqual (byte.MaxValue, converter.ConvertFrom (null, CultureInfo.InvariantCulture, "0xff"), "#5");
			Assert.AreEqual (byte.MaxValue, converter.ConvertFrom (null, CultureInfo.InvariantCulture, "0XFF"), "#6");
		}

		[Test]
		public void ConvertToString ()
		{
			CultureInfo culture = new MyCultureInfo ();
			NumberFormatInfo numberFormatInfo = (NumberFormatInfo) culture.GetFormat (typeof (NumberFormatInfo));

			Assert.AreEqual (culture.NumberFormat.NegativeSign + "5", converter.ConvertToString (null, culture, -5));
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ConvertFrom_Object ()
		{
			converter.ConvertFrom (new object ());
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ConvertFrom_Byte ()
		{
			converter.ConvertFrom (byte.MaxValue);
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
			Assert.AreEqual (byte.MinValue.ToString (CultureInfo.InvariantCulture),
				converter.ConvertTo (null, CultureInfo.InvariantCulture, byte.MinValue,
				typeof (string)), "#1");
			Assert.AreEqual (byte.MinValue.ToString (CultureInfo.CurrentCulture),
				converter.ConvertTo (null, CultureInfo.CurrentCulture, byte.MinValue,
				typeof (string)), "#2");
			Assert.AreEqual (byte.MinValue.ToString (CultureInfo.CurrentCulture),
				converter.ConvertTo (byte.MinValue, typeof (string)), "#3");
		}

		[Test]
		public void ConvertTo_MaxValue ()
		{
			Assert.AreEqual (byte.MaxValue.ToString (CultureInfo.InvariantCulture),
				converter.ConvertTo (null, CultureInfo.InvariantCulture, byte.MaxValue,
				typeof (string)), "#1");
			Assert.AreEqual (byte.MaxValue.ToString (CultureInfo.CurrentCulture),
				converter.ConvertTo (null, CultureInfo.CurrentCulture, byte.MaxValue,
				typeof (string)), "#2");
			Assert.AreEqual (byte.MaxValue.ToString (CultureInfo.CurrentCulture),
				converter.ConvertTo (byte.MaxValue, typeof (string)), "#3");
		}

		[Test]
		public void ConvertToString_MinValue ()
		{
			Assert.AreEqual (byte.MinValue.ToString (CultureInfo.InvariantCulture),
				converter.ConvertToString (null, CultureInfo.InvariantCulture,
				byte.MinValue), "#1");

			Assert.AreEqual (byte.MinValue.ToString (CultureInfo.CurrentCulture),
				converter.ConvertToString (null, byte.MinValue), "#2");
			Assert.AreEqual (byte.MinValue.ToString (CultureInfo.CurrentCulture),
				converter.ConvertToString (null, CultureInfo.CurrentCulture,
				byte.MinValue), "#3");
			Assert.AreEqual (byte.MinValue.ToString (CultureInfo.CurrentCulture),
				converter.ConvertToString (byte.MinValue), "#4");
		}

		[Test]
		public void ConvertToString_MaxValue ()
		{
			Assert.AreEqual (byte.MaxValue.ToString (CultureInfo.InvariantCulture),
				converter.ConvertToString (null, CultureInfo.InvariantCulture,
				byte.MaxValue), "#1");

			Assert.AreEqual (byte.MaxValue.ToString (CultureInfo.CurrentCulture),
				converter.ConvertToString (null, byte.MaxValue), "#2");
			Assert.AreEqual (byte.MaxValue.ToString (CultureInfo.CurrentCulture),
				converter.ConvertToString (null, CultureInfo.CurrentCulture,
				byte.MaxValue), "#3");
			Assert.AreEqual (byte.MaxValue.ToString (CultureInfo.CurrentCulture),
				converter.ConvertToString (byte.MaxValue), "#4");
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
			string minOverflow = ((int) (byte.MinValue - 1)).ToString (
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
			string minOverflow = ((int) (byte.MinValue - 1)).ToString (
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
			string maxOverflow = ((int) (byte.MaxValue + 1)).ToString (
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
			string maxOverflow = ((int) (byte.MaxValue + 1)).ToString (
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
			string minOverflow = ((int) (byte.MinValue - 1)).ToString ("x", 
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
			string minOverflow = ((int) (byte.MinValue - 1)).ToString ("x",
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
				Assert.AreEqual (typeof (OverflowException), ex.InnerException.GetType (), "#3");
			}
		}

		[Test]
		public void ConvertFrom_Base16_MaxOverflow ()
		{
			string maxOverflow = ((int) (byte.MaxValue + 1)).ToString ("x",
				CultureInfo.CurrentCulture);

			try {
				converter.ConvertFrom ("#" + maxOverflow);
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
		public void ConvertFrom_Base16_MaxOverflow_Invariant ()
		{
			string maxOverflow = ((int) (byte.MaxValue + 1)).ToString ("x",
				CultureInfo.InvariantCulture);

			try {
				converter.ConvertFrom (null, CultureInfo.InvariantCulture, 
					"#" + maxOverflow);
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
			string minOverflow = ((int) (byte.MinValue - 1)).ToString (
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
			string minOverflow = ((int) (byte.MinValue - 1)).ToString (
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
			string maxOverflow = ((int) (byte.MaxValue + 1)).ToString (
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
			string maxOverflow = ((int) (byte.MaxValue + 1)).ToString (
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
			string minOverflow = ((int) (byte.MinValue - 1)).ToString ("x",
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
			string minOverflow = ((int) (byte.MinValue - 1)).ToString ("x",
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
			string maxOverflow = ((int) (byte.MaxValue + 1)).ToString ("x",
				CultureInfo.CurrentCulture);

			try {
				converter.ConvertFromString ("#" + maxOverflow);
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
		public void ConvertFromString_Base16_MaxOverflow_Invariant ()
		{
			string maxOverflow = ((int) (byte.MaxValue + 1)).ToString ("x",
				CultureInfo.InvariantCulture);

			try {
				converter.ConvertFromString (null, CultureInfo.InvariantCulture,
					"#" + maxOverflow);
				Assert.Fail ("#1");
			} catch (AssertionException) {
				throw;
			} catch (Exception ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.AreEqual (typeof (OverflowException), ex.InnerException.GetType (), "#3");
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
