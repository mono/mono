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
			Assert.IsFalse (converter.CanConvertFrom (typeof (object)), "#2");
		}

		[Test]
		public void CanConvertTo ()
		{
			Assert.IsTrue (converter.CanConvertTo (typeof (string)), "#1");
			Assert.IsFalse (converter.CanConvertTo (typeof (object)), "#2");
		}

		[Test]
		public void ConvertFrom_String ()
		{
			Assert.AreEqual ((short) 10, converter.ConvertFrom (null, CultureInfo.InvariantCulture, "10"), "#1");
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
			converter.ConvertFrom ((byte) 254);
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
		public void ConvertTo_Invariant ()
		{
			Assert.AreEqual ("255", converter.ConvertTo (null, CultureInfo.InvariantCulture, 
				(byte) 255, typeof (string)), "#1");
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
				converter.ConvertFromString ("256");
				Assert.Fail ("#1");
			} catch (Exception ex) {
				Assert.AreEqual (typeof (Exception), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.AreEqual (typeof (OverflowException), ex.InnerException.GetType (), "#3");
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
				converter.ConvertFrom (null, CultureInfo.InvariantCulture, "256");
				Assert.Fail ("#1");
			} catch (Exception ex) {
				Assert.AreEqual (typeof (Exception), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.AreEqual (typeof (OverflowException), ex.InnerException.GetType (), "#3");
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
				converter.ConvertFrom ("256");
				Assert.Fail ("#1");
			} catch (Exception ex) {
				Assert.AreEqual (typeof (Exception), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.AreEqual (typeof (OverflowException), ex.InnerException.GetType (), "#3");
			}
		}
	}
}

