//
// System.Globalization.CultureInfo Test Cases
//
// Authors:
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) 2005 Novell, Inc. (http://www.novell.com)
//

using System;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

using NUnit.Framework;

namespace MonoTests.System.Globalization
{
	[TestFixture]
	public class CultureInfoTest
	{
		CultureInfo old_culture;

		[SetUp]
		public void Setup ()
		{
			old_culture = Thread.CurrentThread.CurrentCulture;
		}

		[TearDown]
		public void TearDown ()
		{
			Thread.CurrentThread.CurrentCulture = old_culture;
		}

		[Test]
		public void Constructor0 ()
		{
			CultureInfo ci = new CultureInfo (2067);
			Assert.IsFalse (ci.IsReadOnly, "#1");
			Assert.AreEqual (2067, ci.LCID, "#2");
			Assert.AreEqual ("nl-BE", ci.Name, "#3");
			Assert.IsTrue (ci.UseUserOverride, "#4");
		}

		[Test]
		public void Constructor0_Identifier_Negative ()
		{
			try {
				new CultureInfo (-1);
				Assert.Fail ("#1");
			} catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("culture", ex.ParamName, "#6");
			}
		}

		[Test]
		public void Constructor1 ()
		{
			CultureInfo ci = new CultureInfo ("nl-BE");
			Assert.IsFalse (ci.IsReadOnly, "#1");
			Assert.AreEqual (2067, ci.LCID, "#2");
			Assert.AreEqual ("nl-BE", ci.Name, "#3");
			Assert.IsTrue (ci.UseUserOverride, "#4");
		}

		[Test]
		public void Constructor1_Name_Null ()
		{
			try {
				new CultureInfo ((string) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("name", ex.ParamName, "#6");
			}
		}

		[Test]
		public void Constructor2 ()
		{
			CultureInfo ci = new CultureInfo (2067, false);
			Assert.IsFalse (ci.IsReadOnly, "#A1");
			Assert.AreEqual (2067, ci.LCID, "#A2");
			Assert.AreEqual ("nl-BE", ci.Name, "#A3");
			Assert.IsFalse (ci.UseUserOverride, "#A4");

			ci = new CultureInfo (2067, true);
			Assert.IsFalse (ci.IsReadOnly, "#B1");
			Assert.AreEqual (2067, ci.LCID, "#B2");
			Assert.AreEqual ("nl-BE", ci.Name, "#B3");
			Assert.IsTrue (ci.UseUserOverride, "#B4");
		}

		[Test]
		public void Constructor2_Identifier_Negative ()
		{
			try {
				new CultureInfo (-1, false);
				Assert.Fail ("#1");
			} catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("culture", ex.ParamName, "#6");
			}
		}

		[Test]
		public void Constructor3 ()
		{
			CultureInfo ci = new CultureInfo ("nl-BE", false);
			Assert.IsFalse (ci.IsReadOnly, "#A1");
			Assert.AreEqual (2067, ci.LCID, "#A2");
			Assert.AreEqual ("nl-BE", ci.Name, "#A3");
			Assert.IsFalse (ci.UseUserOverride, "#A4");

			ci = new CultureInfo ("nl-BE", true);
			Assert.IsFalse (ci.IsReadOnly, "#B1");
			Assert.AreEqual (2067, ci.LCID, "#B2");
			Assert.AreEqual ("nl-BE", ci.Name, "#B3");
			Assert.IsTrue (ci.UseUserOverride, "#B4");
		}

		[Test]
		public void Constructor3_Name_Null ()
		{
			try {
				new CultureInfo ((string) null, false);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("name", ex.ParamName, "#6");
			}
		}

		[Test]
		public void DateTimeFormat_Neutral_Culture ()
		{
			CultureInfo ci = new CultureInfo ("nl");
			try {
				DateTimeFormatInfo dfi = ci.DateTimeFormat;
#if NET_4_0
				Assert.IsNotNull (dfi, "#1");
#else
				Assert.Fail ("#1:" + (dfi != null));
#endif
			} catch (NotSupportedException ex) {
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test] // bug #72081
		public void GetAllCulturesInvariant ()
		{
			CultureInfo invariant = CultureInfo.InvariantCulture;
			CultureInfo [] infos = CultureInfo.GetCultures (CultureTypes.AllCultures);
			foreach (CultureInfo culture in infos) {
				if (culture.Equals (invariant))
					return;
			}

			Assert.Fail ("InvariantCulture not found in the array from GetCultures()");
		}

		[Test]
#if !NET_4_0
		[ExpectedException (typeof (NotSupportedException))]
#endif
		public void TrySetNeutralCultureNotInvariant ()
		{
			Thread.CurrentThread.CurrentCulture = new CultureInfo ("ar");
		}

		[Test]
		[Category ("TargetJvmNotWorking")] //OptionalCalendars not yet supported for TARGET_JVM.
		// make sure that all CultureInfo holds non-null calendars.
		public void OptionalCalendars ()
		{
			foreach (CultureInfo ci in CultureInfo.GetCultures (
				CultureTypes.AllCultures))
				Assert.IsNotNull (ci.OptionalCalendars, String.Format ("{0} {1}",
					ci.LCID, ci.Name));
		}

		[Test] // bug #77347
		public void CloneNeutral ()
		{
			CultureInfo culture = new CultureInfo ("en");
			CultureInfo cultureClone = culture.Clone () as CultureInfo;
			Assert.IsTrue (culture.Equals (cultureClone));
		}

		[Test] // bug #81930
		public void IsReadOnly ()
		{
			CultureInfo ci;

			ci = new CultureInfo ("en-US");
			Assert.IsFalse (ci.IsReadOnly, "#A1");
			Assert.IsFalse (ci.NumberFormat.IsReadOnly, "#A2");
			Assert.IsFalse (ci.DateTimeFormat.IsReadOnly, "#A3");
			ci.NumberFormat.NumberGroupSeparator = ",";
			ci.NumberFormat = new NumberFormatInfo ();
			ci.DateTimeFormat.DateSeparator = "/";
			ci.DateTimeFormat = new DateTimeFormatInfo ();
			Assert.IsFalse (ci.NumberFormat.IsReadOnly, "#A4");
			Assert.IsFalse (ci.DateTimeFormat.IsReadOnly, "#A5");

			ci = new CultureInfo (CultureInfo.InvariantCulture.LCID);
			Assert.IsFalse (ci.IsReadOnly, "#B1");
			Assert.IsFalse (ci.NumberFormat.IsReadOnly, "#B2");
			Assert.IsFalse (ci.DateTimeFormat.IsReadOnly, "#B3");
			ci.NumberFormat.NumberGroupSeparator = ",";
			ci.NumberFormat = new NumberFormatInfo ();
			ci.DateTimeFormat.DateSeparator = "/";
			ci.DateTimeFormat = new DateTimeFormatInfo ();
			Assert.IsFalse (ci.NumberFormat.IsReadOnly, "#B4");
			Assert.IsFalse (ci.DateTimeFormat.IsReadOnly, "#B5");

			ci = CultureInfo.CurrentCulture;
			Assert.IsTrue (ci.IsReadOnly, "#C1:" + ci.DisplayName);
			Assert.IsTrue (ci.NumberFormat.IsReadOnly, "#C2");
			Assert.IsTrue (ci.DateTimeFormat.IsReadOnly, "#C3");
			try {
				ci.NumberFormat.NumberGroupSeparator = ",";
				Assert.Fail ("#C4");
			} catch (InvalidOperationException ex) {
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#C5");
				Assert.IsNull (ex.InnerException, "#C6");
				Assert.IsNotNull (ex.Message, "#C7");
			}

			ci = CultureInfo.CurrentUICulture;
			Assert.IsTrue (ci.IsReadOnly, "#D1");
			Assert.IsTrue (ci.NumberFormat.IsReadOnly, "#D2");
			Assert.IsTrue (ci.DateTimeFormat.IsReadOnly, "#D3");
			try {
				ci.NumberFormat.NumberGroupSeparator = ",";
				Assert.Fail ("#D4");
			} catch (InvalidOperationException ex) {
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#D5");
				Assert.IsNull (ex.InnerException, "#D6");
				Assert.IsNotNull (ex.Message, "#D7");
			}

			ci = CultureInfo.InvariantCulture;
			Assert.IsTrue (ci.IsReadOnly, "#F1");
			Assert.IsTrue (ci.NumberFormat.IsReadOnly, "#F2");
			Assert.IsTrue (ci.DateTimeFormat.IsReadOnly, "#F3");
			try {
				ci.NumberFormat.NumberGroupSeparator = ",";
				Assert.Fail ("#F4");
			} catch (InvalidOperationException ex) {
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#F5");
				Assert.IsNull (ex.InnerException, "#F6");
				Assert.IsNotNull (ex.Message, "#F7");
			}

			ci = new CultureInfo (string.Empty);
			Assert.IsFalse (ci.IsReadOnly, "#G1");
			Assert.IsFalse (ci.NumberFormat.IsReadOnly, "#G2");
			Assert.IsFalse (ci.DateTimeFormat.IsReadOnly, "#G3");
			ci.NumberFormat.NumberGroupSeparator = ",";
			ci.NumberFormat = new NumberFormatInfo ();
			ci.DateTimeFormat.DateSeparator = "/";
			ci.DateTimeFormat = new DateTimeFormatInfo ();
			Assert.IsFalse (ci.NumberFormat.IsReadOnly, "#G4");
			Assert.IsFalse (ci.DateTimeFormat.IsReadOnly, "#G5");
		}

		[Test]
		public void IsReadOnly_GetCultures ()
		{
			foreach (CultureInfo ci in CultureInfo.GetCultures (CultureTypes.AllCultures)) {
				string cultureMsg = String.Format ("{0} {1}", ci.LCID, ci.Name);
				Assert.IsFalse (ci.IsReadOnly, "#1:" + cultureMsg);
				if (ci.IsNeutralCulture)
					continue;
				Assert.IsFalse (ci.NumberFormat.IsReadOnly, "#2:" + cultureMsg);
				Assert.IsFalse (ci.DateTimeFormat.IsReadOnly, "#3:" + cultureMsg);
			}
		}

		[Test]
		[Category ("NotWorking")]
		public void IsReadOnly_InstalledUICulture ()
		{
			CultureInfo ci = CultureInfo.InstalledUICulture;
			Assert.IsTrue (ci.IsReadOnly, "#1");
			Assert.IsTrue (ci.NumberFormat.IsReadOnly, "#2");
			Assert.IsTrue (ci.DateTimeFormat.IsReadOnly, "#3");
			try {
				ci.NumberFormat.NumberGroupSeparator = ",";
				Assert.Fail ("#4");
			} catch (InvalidOperationException ex) {
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#5");
				Assert.IsNull (ex.InnerException, "#6");
				Assert.IsNotNull (ex.Message, "#7");
			}
		}

		[Test] // bug #69652
		public void Norwegian ()
		{
			new CultureInfo ("no");
			new CultureInfo ("nb-NO");
			new CultureInfo ("nn-NO");
		}

		[Test]
		public void NumberFormat_Neutral_Culture ()
		{
			CultureInfo ci = new CultureInfo ("nl");
			try {
				NumberFormatInfo nfi = ci.NumberFormat;
#if NET_4_0
				Assert.IsNotNull (nfi, "#1");
#else
				Assert.Fail ("#1:" + (nfi != null));
#endif
			} catch (NotSupportedException ex) {
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

#if NET_2_0
		[Test]
		[Category ("NotDotNet")] // On MS, the NumberFormatInfo of the CultureInfo matching the current locale is not read-only
		public void GetCultureInfo_Identifier ()
		{
			foreach (CultureInfo ci in CultureInfo.GetCultures (CultureTypes.AllCultures)) {
				string cultureMsg = String.Format ("{0} {1}", ci.LCID, ci.Name);
				CultureInfo culture = CultureInfo.GetCultureInfo (ci.LCID);
				Assert.IsTrue (culture.IsReadOnly, "#1:" + cultureMsg);
				if (culture.IsNeutralCulture)
					continue;
				Assert.IsTrue (culture.NumberFormat.IsReadOnly, "#2:" + cultureMsg);
				Assert.IsTrue (culture.DateTimeFormat.IsReadOnly, "#3:" + cultureMsg);
			}
		}

		[Test]
		public void GetCultureInfo_Identifier_Negative ()
		{
			try {
				CultureInfo.GetCultureInfo (-1);
				Assert.Fail ("#1");
			} catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("culture", ex.ParamName, "#6");
			}
		}

		[Test]
		public void GetCultureInfo_Identifier_NotSupported ()
		{
			try {
				CultureInfo.GetCultureInfo (666);
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
#if NET_4_0
				Assert.AreEqual (typeof (CultureNotFoundException), ex.GetType (), "#2");
#else
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
#endif
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("culture", ex.ParamName, "#6");
			}
		}

		[Test]
		[Category ("NotDotNet")] // On MS, the NumberFormatInfo of the CultureInfo matching the current locale is not read-only
		public void GetCultureInfo_Name ()
		{
			foreach (CultureInfo ci in CultureInfo.GetCultures (CultureTypes.AllCultures)) {
				string cultureMsg = String.Format ("{0} {1}", ci.LCID, ci.Name);
				CultureInfo culture = CultureInfo.GetCultureInfo (ci.Name);
				Assert.IsTrue (culture.IsReadOnly, "#1:" + cultureMsg);
				if (culture.IsNeutralCulture)
					continue;
				Assert.IsTrue (culture.NumberFormat.IsReadOnly, "#2:" + cultureMsg);
				Assert.IsTrue (culture.DateTimeFormat.IsReadOnly, "#3:" + cultureMsg);
			}
		}

		[Test]
		public void GetCultureInfo_Name_NotSupported ()
		{
			try {
				CultureInfo.GetCultureInfo ("666");
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
#if NET_4_0
				Assert.AreEqual (typeof (CultureNotFoundException), ex.GetType (), "#2");
#else
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
#endif
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("name", ex.ParamName, "#6");
			}
		}

		[Test]
		public void GetCultureInfo_Name_Null ()
		{
			try {
				CultureInfo.GetCultureInfo ((string) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("name", ex.ParamName, "#6");
			}
		}
#endif

		[Test]
		public void UseUserOverride_CurrentCulture ()
		{
			CultureInfo ci = CultureInfo.CurrentCulture;
			bool expected = (ci.LCID != CultureInfo.InvariantCulture.LCID);
			Assert.AreEqual (expected, ci.UseUserOverride, "#1");

			ci = (CultureInfo) ci.Clone ();
			Assert.AreEqual (expected, ci.UseUserOverride, "#2");
		}

		[Test]
		public void UseUserOverride_CurrentUICulture ()
		{
			CultureInfo ci = CultureInfo.CurrentCulture;
			bool expected = (ci.LCID != CultureInfo.InvariantCulture.LCID);
			Assert.AreEqual (expected, ci.UseUserOverride, "#1");

			ci = (CultureInfo) ci.Clone ();
			Assert.AreEqual (expected, ci.UseUserOverride, "#2");
		}

#if NET_2_0
		[Test]
		public void UseUserOverride_GetCultureInfo ()
		{
			CultureInfo culture;

			foreach (CultureInfo ci in CultureInfo.GetCultures (CultureTypes.AllCultures)) {
				string cultureMsg = String.Format ("{0} {1}", ci.LCID, ci.Name);
				culture = CultureInfo.GetCultureInfo (ci.Name);
				Assert.IsFalse (culture.UseUserOverride, "#1: " + cultureMsg);
				culture = CultureInfo.GetCultureInfo (ci.LCID);
				Assert.IsFalse (culture.UseUserOverride, "#2: " + cultureMsg);
			}
		}
#endif

		[Test]
		public void UseUserOverride_GetCultures ()
		{
			foreach (CultureInfo ci in CultureInfo.GetCultures (CultureTypes.AllCultures)) {
				string cultureMsg = String.Format ("{0} {1}", ci.LCID, ci.Name);
#if NET_2_0
				if (ci.LCID == CultureInfo.InvariantCulture.LCID)
					Assert.IsFalse (ci.UseUserOverride, cultureMsg);
				else
					Assert.IsTrue (ci.UseUserOverride, cultureMsg);
#else
				Assert.IsTrue (ci.UseUserOverride, cultureMsg);
#endif
			}
		}

		[Test]
		public void UseUserOverride_InvariantCulture ()
		{
			CultureInfo ci = CultureInfo.InvariantCulture;
			Assert.IsFalse (ci.UseUserOverride, "#21");

			ci = (CultureInfo) ci.Clone ();
			Assert.IsFalse (ci.UseUserOverride, "#2");
		}

		[Test]
		public void Bug402128 ()
		{
			var culture = new CultureInfo ("en-US");
			var ms = new MemoryStream ();
			var formatter = new BinaryFormatter ();
			formatter.Serialize (ms, culture);
			ms.Seek (0, SeekOrigin.Begin);
			var deserializedCulture = (CultureInfo) formatter.Deserialize (ms);
		}

		[Test]
		public void ZhHant ()
		{
			Assert.AreEqual (31748, new CultureInfo ("zh-Hant").LCID);
#if NET_2_0
			Assert.AreEqual (31748, CultureInfo.GetCultureInfo ("zh-Hant").LCID);
#endif
		}
	}
}
