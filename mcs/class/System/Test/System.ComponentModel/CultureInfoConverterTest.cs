//
// System.ComponentModel.CultureInfoConverter test cases
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
	public class CultureInfoConverterTest
	{
		private CultureInfoConverter converter;
		
		[SetUp]
		public void SetUp ()
		{
			converter = new CultureInfoConverter ();
		}

		[Test]
		public void CanConvertFrom ()
		{
			Assert.IsTrue (converter.CanConvertFrom (typeof (string)), "#1");
			Assert.IsFalse (converter.CanConvertFrom (typeof (CultureInfo)), "#2");
			Assert.IsFalse (converter.CanConvertFrom (typeof (object)), "#3");
			Assert.IsFalse (converter.CanConvertFrom (typeof (int)), "#4");
			Assert.IsTrue (converter.CanConvertFrom (typeof (InstanceDescriptor)), "#5");
		}

		[Test]
		public void CanConvertTo ()
		{
			Assert.IsTrue (converter.CanConvertTo (typeof (string)), "#1");
			Assert.IsFalse (converter.CanConvertTo (typeof (object)), "#2");
			Assert.IsFalse (converter.CanConvertTo (typeof (CultureInfo)), "#3");
			Assert.IsFalse (converter.CanConvertTo (typeof (int)), "#4");
			Assert.IsTrue (converter.CanConvertTo (typeof (InstanceDescriptor)), "#5");
		}

		[Test]
		public void ConvertFrom_String ()
		{
			CultureInfo c;

			c = (CultureInfo) converter.ConvertFrom (null, CultureInfo.InvariantCulture,
				String.Empty);
			Assert.AreEqual (CultureInfo.InvariantCulture, c, "#1");

			c = (CultureInfo) converter.ConvertFrom (null, CultureInfo.InvariantCulture,
				"nl-BE");
			Assert.AreEqual (new CultureInfo ("nl-BE"), c, "#2");

			c = (CultureInfo) converter.ConvertFrom (null, CultureInfo.InvariantCulture,
				"Dut");
			Assert.AreEqual (new CultureInfo ("nl"), c, "#3");

			c = (CultureInfo) converter.ConvertFrom (null, CultureInfo.InvariantCulture,
				"Dutch (Bel");
			Assert.AreEqual (new CultureInfo ("nl-BE"), c, "#4");

			c = (CultureInfo) converter.ConvertFrom (null, CultureInfo.InvariantCulture,
				"duTcH (Bel");
			Assert.AreEqual (new CultureInfo ("nl-BE"), c, "#5");

			c = (CultureInfo) converter.ConvertFrom (null, CultureInfo.InvariantCulture,
				"(Default)");
			Assert.AreEqual (CultureInfo.InvariantCulture, c, "#6");

#if ONLY_1_1
			c = (CultureInfo) converter.ConvertFrom (null, CultureInfo.InvariantCulture,
				"(defAuLt)");
			Assert.AreEqual (CultureInfo.InvariantCulture, c, "#6");
#endif
		}

		[Test]
		public void ConvertFrom_String_IncompleteName ()
		{
			try {
				converter.ConvertFrom (null, CultureInfo.InvariantCulture,
					"nl-B");
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// The nl-B culture cannot be converted to a
				// CultureInfo object on this computer
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsTrue (ex.Message.IndexOf (typeof (CultureInfo).Name) != -1, "#5");
				Assert.IsTrue (ex.Message.IndexOf ("nl-B") != -1, "#6");
				Assert.IsNull (ex.ParamName, "#7");
			}
		}

		[Test]
		public void ConvertFrom_String_InvalidCulture ()
		{
#if NET_2_0
			try {
				converter.ConvertFrom (null, CultureInfo.InvariantCulture,
					"(default)");
				Assert.Fail ("#A1");
			} catch (ArgumentException ex) {
				// The (default) culture cannot be converted to
				// a CultureInfo object on this computer
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsTrue (ex.Message.IndexOf (typeof (CultureInfo).Name) != -1, "#A5");
				Assert.IsTrue (ex.Message.IndexOf ("(default)") != -1, "#A6");
				Assert.IsNull (ex.ParamName, "#A7");
			}
#endif

			try {
				converter.ConvertFrom (null, CultureInfo.InvariantCulture,
					" ");
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				// The   culture cannot be converted to
				// a CultureInfo object on this computer
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsTrue (ex.Message.IndexOf (typeof (CultureInfo).Name) != -1, "#B5");
				Assert.IsTrue (ex.Message.IndexOf ("   ") != -1, "#B6");
				Assert.IsNull (ex.ParamName, "#B7");
			}

			try {
				converter.ConvertFrom (null, CultureInfo.InvariantCulture,
					"\r\n");
				Assert.Fail ("#C1");
			} catch (ArgumentException ex) {
				// The \r\n culture cannot be converted to
				// a CultureInfo object on this computer
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#C2");
				Assert.IsNull (ex.InnerException, "#C3");
				Assert.IsNotNull (ex.Message, "#C4");
				Assert.IsTrue (ex.Message.IndexOf (typeof (CultureInfo).Name) != -1, "#C5");
				Assert.IsTrue (ex.Message.IndexOf ("\r\n") != -1, "#C6");
				Assert.IsNull (ex.ParamName, "#C7");
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
				// CultureInfoConverter cannot convert from (null)
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsTrue (ex.Message.IndexOf (typeof (CultureInfoConverter).Name) != -1, "#5");
				Assert.IsTrue (ex.Message.IndexOf ("(null)") != -1, "#6");
			}
		}

		[Test]
		public void ConvertToString ()
		{
			string result;

			result = converter.ConvertToString (null, CultureInfo.InvariantCulture,
				new MyCultureInfo ());
			Assert.AreEqual ("display", result, "#1");

			result = converter.ConvertToString (null, CultureInfo.InvariantCulture,
				null);
			Assert.AreEqual ("(Default)", result, "#2");

			result = converter.ConvertToString (null, CultureInfo.InvariantCulture,
				CultureInfo.InvariantCulture);
			Assert.AreEqual ("(Default)", result, "#3");

			result = converter.ConvertToString (null, CultureInfo.InvariantCulture,
				new CultureInfo ("nl-BE"));
			Assert.AreEqual ("Dutch (Belgium)", result, "#4");
		}

		[Serializable]
		private sealed class MyCultureInfo : CultureInfo
		{
			internal MyCultureInfo () : base ("nl-BE")
			{
			}

			public override string DisplayName {
				get { return "display"; }
			}

			public override string EnglishName {
				get { return "english"; }
			}
		}

#if NET_4_0
		[Test]
		public void GetCultureName ()
		{
			CustomCultureInfoConverter custom_converter = new CustomCultureInfoConverter ();

			CultureInfo fr_culture = CultureInfo.GetCultureInfo ("fr-FR");
			Assert.AreEqual (fr_culture.Name, custom_converter.GetCultureName (fr_culture), "#A1");

			CultureInfo es_culture = CultureInfo.GetCultureInfo ("es-MX");
			Assert.AreEqual (es_culture.Name, custom_converter.GetCultureName (es_culture), "#A2");
		}

		class CustomCultureInfoConverter : CultureInfoConverter
		{
			public new string GetCultureName (CultureInfo culture)
			{
				return base.GetCultureName (culture);
			}
		}
#endif
	}
}
