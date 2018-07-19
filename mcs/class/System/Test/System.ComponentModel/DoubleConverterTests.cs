//
// System.ComponentModel.DoubleConverter test cases
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
	public class DoubleConverterTests
	{
		private DoubleConverter converter;
		
		[SetUp]
		public void SetUp ()
		{
			converter = new DoubleConverter ();
		}

		[Test]
		public void CanConvertFrom ()
		{
			Assert.IsTrue (converter.CanConvertFrom (typeof (string)), "#1");
			Assert.IsFalse (converter.CanConvertFrom (typeof (double)), "#2");
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
		public void ConvertFrom_String ()
		{
			Assert.AreEqual (10, converter.ConvertFrom (null, CultureInfo.InvariantCulture, "10"), "#1");
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
		public void ConvertTo_Negative ()
		{
			Assert.AreEqual ((-1.5D).ToString (CultureInfo.InvariantCulture),
				converter.ConvertTo (null, CultureInfo.InvariantCulture, -1.5D,
				typeof (string)), "#1");
			Assert.AreEqual ((-1.5D).ToString (CultureInfo.CurrentCulture),
				converter.ConvertTo (null, CultureInfo.CurrentCulture, -1.5D,
				typeof (string)), "#2");
			Assert.AreEqual ((-1.5D).ToString (CultureInfo.CurrentCulture),
				converter.ConvertTo (-1.5D, typeof (string)), "#3");
		}

		[Test]
		public void ConvertTo_Positive ()
		{
			Assert.AreEqual (1.5D.ToString (CultureInfo.InvariantCulture),
				converter.ConvertTo (null, CultureInfo.InvariantCulture, 1.5D, 
				typeof (string)), "#1");
			Assert.AreEqual (1.5D.ToString (CultureInfo.CurrentCulture),
				converter.ConvertTo (null, CultureInfo.CurrentCulture, 1.5D, 
				typeof (string)), "#2");
			Assert.AreEqual (1.5D.ToString (CultureInfo.CurrentCulture),
				converter.ConvertTo (1.5D, typeof (string)), "#3");
		}

		[Test]
		public void ConvertToString_Negative ()
		{
			Assert.AreEqual ((-1.5D).ToString (CultureInfo.InvariantCulture),
				converter.ConvertToString (null, CultureInfo.InvariantCulture,
				-1.5D), "#1");

			Assert.AreEqual ((-1.5D).ToString (CultureInfo.CurrentCulture),
				converter.ConvertToString (null, -1.5D), "#2");
			Assert.AreEqual ((-1.5D).ToString (CultureInfo.CurrentCulture),
				converter.ConvertToString (null, CultureInfo.CurrentCulture,
				-1.5D), "#3");
			Assert.AreEqual ((-1.5D).ToString (CultureInfo.CurrentCulture),
				converter.ConvertToString (-1.5D), "#4");
		}

		[Test]
		public void ConvertToString_Positive ()
		{
			Assert.AreEqual (1.5D.ToString (CultureInfo.InvariantCulture),
				converter.ConvertToString (null, CultureInfo.InvariantCulture,
				1.5D), "#1");

			Assert.AreEqual (1.5D.ToString (CultureInfo.CurrentCulture),
				converter.ConvertToString (null, 1.5D), "#2");
			Assert.AreEqual (1.5D.ToString (CultureInfo.CurrentCulture),
				converter.ConvertToString (null, CultureInfo.CurrentCulture,
				1.5D), "#3");
			Assert.AreEqual (1.5D.ToString (CultureInfo.CurrentCulture),
				converter.ConvertToString (1.5D), "#4");
		}

		[Test]
		public void ConvertToString ()
		{
			CultureInfo culture = new MyCultureInfo ();
			NumberFormatInfo numberFormatInfo = (NumberFormatInfo) culture.GetFormat (typeof (NumberFormatInfo));

			Assert.AreEqual (numberFormatInfo.NegativeSign + "5", converter.ConvertToString (null, culture, (double) -5), "#1");
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
			string minOverflow = double.MinValue.ToString (
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
			string minOverflow = double.MinValue.ToString (
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
			string maxOverflow = double.MaxValue.ToString (
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
			string maxOverflow = double.MaxValue.ToString (
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
			string minOverflow = double.MinValue.ToString (
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
			string minOverflow = double.MinValue.ToString (
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
			string maxOverflow = double.MaxValue.ToString (
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
			string maxOverflow = double.MaxValue.ToString (
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
