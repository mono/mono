//
// System.ComponentModel.CharConverter test cases
//
// Authors:
// 	Gert Driesen (drieseng@users.sourceforge.net)
//
// (c) 2008 Gert Driesen
//

using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;

using NUnit.Framework;

namespace MonoTests.System.ComponentModel
{
	[TestFixture]
	public class CharConverterTest
	{
		private CharConverter converter;
		private string pattern;
		
		[SetUp]
		public void SetUp ()
		{
			converter = new CharConverter ();

			DateTimeFormatInfo info = CultureInfo.CurrentCulture.DateTimeFormat;
			pattern = info.ShortDatePattern + " " + info.ShortTimePattern;
		}

		[Test]
		public void CanConvertFrom ()
		{
			Assert.IsTrue (converter.CanConvertFrom (typeof (string)), "#1");
			Assert.IsFalse (converter.CanConvertFrom (typeof (char)), "#2");
			Assert.IsFalse (converter.CanConvertFrom (typeof (object)), "#3");
			Assert.IsFalse (converter.CanConvertFrom (typeof (int)), "#4");
			Assert.IsFalse (converter.CanConvertFrom (typeof (char [])), "#5");
			Assert.IsTrue (converter.CanConvertFrom (typeof (InstanceDescriptor)), "#6");
		}

		[Test]
		public void CanConvertTo ()
		{
			Assert.IsTrue (converter.CanConvertTo (typeof (string)), "#1");
			Assert.IsFalse (converter.CanConvertTo (typeof (char)), "#2");
			Assert.IsFalse (converter.CanConvertTo (typeof (object)), "#3");
			Assert.IsFalse (converter.CanConvertTo (typeof (int)), "#4");
			Assert.IsFalse (converter.CanConvertTo (typeof (char [])), "#5");
			Assert.IsFalse (converter.CanConvertTo (typeof (InstanceDescriptor)), "#6");
		}

		[Test]
		public void ConvertFrom_String ()
		{
			char c;

			c = (char) converter.ConvertFrom (null, CultureInfo.InvariantCulture,
				String.Empty);
			Assert.AreEqual ('\0', c, "#1");

			c = (char) converter.ConvertFrom (null, CultureInfo.InvariantCulture,
				"e");
			Assert.AreEqual ('e', c, "#2");

			c = (char) converter.ConvertFrom (null, CultureInfo.InvariantCulture,
				"\t f\r\n ");
			Assert.AreEqual ('f', c, "#3");
		}

		[Test]
		public void ConvertFrom_String_Invalid ()
		{
			try {
				converter.ConvertFrom (null, CultureInfo.InvariantCulture,
					"ef");
				Assert.Fail ("#A1");
			} catch (FormatException ex) {
				// ef is not a valid value for Char
				Assert.AreEqual (typeof (FormatException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsTrue (ex.Message.IndexOf (typeof (char).Name) != -1, "#A5");
				Assert.IsTrue (ex.Message.IndexOf ("ef") != -1, "#A6");
			}

			try {
				converter.ConvertFrom (null, CultureInfo.InvariantCulture,
					"\ref \n");
				Assert.Fail ("#B1");
			} catch (FormatException ex) {
				// \ref\n is not a valid value for Char
				Assert.AreEqual (typeof (FormatException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsTrue (ex.Message.IndexOf (typeof (char).Name) != -1, "#B5");
				Assert.IsTrue (ex.Message.IndexOf ("ef") != -1, "#B6");
			}
		}

		[Test]
		public void ConvertFrom_Value_Null ()
		{
			try {
				converter.ConvertFrom (null, CultureInfo.InvariantCulture,
					(string) null);
				Assert.Fail ("#1");
			} catch (NotSupportedException ex) {
				// CharConverter cannot convert from (null)
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsTrue (ex.Message.IndexOf (typeof (CharConverter).Name) != -1, "#5");
				Assert.IsTrue (ex.Message.IndexOf ("(null)") != -1, "#6");
			}
		}

		[Test]
		public void ConvertToString ()
		{
			string result;

			result = converter.ConvertToString (null, CultureInfo.InvariantCulture,
				' ');
			Assert.AreEqual (" ", result, "#1");

			result = converter.ConvertToString (null, CultureInfo.InvariantCulture,
				'\0');
			Assert.AreEqual (string.Empty, result, "#2");

			result = converter.ConvertToString (null, CultureInfo.InvariantCulture,
				'f');
			Assert.AreEqual ("f", result, "#3");

			result = converter.ConvertToString (null, CultureInfo.InvariantCulture,
				null);
			Assert.AreEqual (string.Empty, result, "#4");

			result = converter.ConvertToString (null, CultureInfo.InvariantCulture,
				new char [] { 'a', 'f' });
			Assert.AreEqual ("System.Char[]", result, "#5");
		}
	}
}
