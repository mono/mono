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
using System.Threading.Tasks;

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
		public void ClearCachedData ()
		{
			var dt = DateTime.Now;
			old_culture.ClearCachedData (); // It can be any culture instance as the method should be static
			dt = DateTime.Now;
		}

		[Test]
		public void CreateSpecificCulture ()
		{
			var ci = CultureInfo.CreateSpecificCulture ("en");
			Assert.AreEqual ("en-US", ci.Name, "#1");

			ci = CultureInfo.CreateSpecificCulture ("en-GB");
			Assert.AreEqual ("en-GB", ci.Name, "#2");

			ci = CultureInfo.CreateSpecificCulture ("en-----");
			Assert.AreEqual ("en-US", ci.Name, "#3");

			ci = CultureInfo.CreateSpecificCulture ("en-GB-");
			Assert.AreEqual ("en-US", ci.Name, "#4");

			ci = CultureInfo.CreateSpecificCulture ("");
			Assert.AreEqual (CultureInfo.InvariantCulture, ci, "#5");

			ci = CultureInfo.CreateSpecificCulture ("zh-hant");
			Assert.AreEqual ("zh-HK", ci.Name, "#6");

			ci = CultureInfo.CreateSpecificCulture ("zh-hans");
			Assert.AreEqual ("zh-CN", ci.Name, "#7");

			ci = CultureInfo.CreateSpecificCulture ("zh-hans-CN");
			Assert.AreEqual ("zh-CN", ci.Name, "#8");

			ci = CultureInfo.CreateSpecificCulture ("zh-hant-US");
			Assert.AreEqual ("zh-HK", ci.Name, "#9");

			ci = CultureInfo.CreateSpecificCulture ("az-CyrlM-BR");
			Assert.AreEqual ("az-Latn-AZ", ci.Name, "#10");
		}

		[Test]
		public void CreateSpecificCulture_Invalid ()
		{
			try {
				CultureInfo.CreateSpecificCulture ("uy32");
				Assert.Fail ("#1");
			} catch (CultureNotFoundException) {
			}

			try {
				CultureInfo.CreateSpecificCulture (null);
				Assert.Fail ("#2");
			} catch (ArgumentNullException) {
				// .NET throws NRE which is lame
			}
		}

		[Test]
		public void DateTimeFormat_Neutral_Culture ()
		{
			CultureInfo ci = new CultureInfo ("nl");
			try {
				DateTimeFormatInfo dfi = ci.DateTimeFormat;
				Assert.IsNotNull (dfi, "#1");
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
		public void GetAllCultures_Specific ()
		{
			CultureInfo [] infos = CultureInfo.GetCultures (CultureTypes.SpecificCultures);
			foreach (CultureInfo ci in infos) {
				Assert.IsNotNull (ci.DateTimeFormat);
			}
		}

		[Test]
		public void TrySetNeutralCultureNotInvariant ()
		{
			Thread.CurrentThread.CurrentCulture = new CultureInfo ("ar");
		}

		[Test]
		// make sure that all CultureInfo holds non-null calendars.
		public void OptionalCalendars ()
		{
#if MOBILE
			// ensure the linker does not remove them so we can test them
			Assert.IsNotNull (typeof (UmAlQuraCalendar), "UmAlQuraCalendar");
#endif
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

		[Test]
		public void IsNeutral ()
		{
			var ci = new CultureInfo (0x6C1A);
			Assert.IsTrue (ci.IsNeutralCulture, "#1");
			Assert.AreEqual ("srp", ci.ThreeLetterISOLanguageName, "#2");

			ci = new CultureInfo ("en-US");
			Assert.IsFalse (ci.IsNeutralCulture, "#1a");
			Assert.AreEqual ("eng", ci.ThreeLetterISOLanguageName, "#2a");
		}

		[Test] // bug #81930
		[Category ("NotWasm")]
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
				Assert.IsNotNull (nfi, "#1");
			} catch (NotSupportedException ex) {
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

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
		public void GetCultureInfo_Identifier_Nonpositive ()
		{
			try {
				CultureInfo.GetCultureInfo (0);
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
				Assert.AreEqual (typeof (CultureNotFoundException), ex.GetType (), "#2");
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
				Assert.AreEqual (typeof (CultureNotFoundException), ex.GetType (), "#2");
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

		[Test]
		[Category ("NotWasm")]
		public void UseUserOverride_CurrentCulture ()
		{
			CultureInfo ci = CultureInfo.CurrentCulture;
			bool expected = (ci.LCID != CultureInfo.InvariantCulture.LCID);
			Assert.AreEqual (expected, ci.UseUserOverride, "#1");

			ci = (CultureInfo) ci.Clone ();
			Assert.AreEqual (expected, ci.UseUserOverride, "#2");
		}

		[Test]
		[Category ("NotWasm")]
		public void UseUserOverride_CurrentUICulture ()
		{
			CultureInfo ci = CultureInfo.CurrentCulture;
			bool expected = (ci.LCID != CultureInfo.InvariantCulture.LCID);
			Assert.AreEqual (expected, ci.UseUserOverride, "#1");

			ci = (CultureInfo) ci.Clone ();
			Assert.AreEqual (expected, ci.UseUserOverride, "#2");
		}

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

		[Test]
		public void UseUserOverride_GetCultures ()
		{
			foreach (CultureInfo ci in CultureInfo.GetCultures (CultureTypes.AllCultures)) {
				string cultureMsg = String.Format ("{0} {1}", ci.LCID, ci.Name);
				if (ci.LCID == CultureInfo.InvariantCulture.LCID)
					Assert.IsFalse (ci.UseUserOverride, cultureMsg);
				else
					Assert.IsTrue (ci.UseUserOverride, cultureMsg);
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
			Assert.AreEqual (31748, CultureInfo.GetCultureInfo ("zh-Hant").LCID);
			Assert.AreEqual (31748, new CultureInfo ("zh-CHT").LCID);
			Assert.AreEqual (31748, new CultureInfo ("zh-CHT").Parent.LCID);
		}

		[Test]
		public void ZhHans ()
		{
			Assert.AreEqual (4, new CultureInfo ("zh-Hans").LCID);
			Assert.AreEqual (4, CultureInfo.GetCultureInfo ("zh-Hans").LCID);
			Assert.AreEqual (4, new CultureInfo ("zh-CHS").LCID);
			Assert.AreEqual (4, new CultureInfo ("zh-CHS").Parent.LCID);
		}

		[Test]
		[SetCulture ("zh-TW")]
		public void ParentOfZh ()
		{
			Assert.AreEqual (31748, CultureInfo.CurrentCulture.Parent.LCID);
			Assert.AreEqual (31748, CultureInfo.CurrentCulture.Parent.Parent.LCID);
		}
		
		[Test]
		public void CurrentCulture ()
		{
			Assert.IsNotNull (CultureInfo.CurrentCulture, "CurrentCulture");
		}
		
		[Test]
		[ExpectedException (typeof (CultureNotFoundException))]
		public void CultureNotFound ()
		{
			new CultureInfo ("en-HKX");
		}

		[Test]
		public void ChineseSimplifiedDontEqual ()
		{
			CultureInfo zh1 = new CultureInfo ("zh-Hans");
			CultureInfo zh2 = new CultureInfo ("zh-CHS");

			Assert.IsFalse (zh1.Equals (zh2), "#1");
			Assert.IsFalse (zh2.Equals (zh1), "#2");
		}

		CountdownEvent barrier = new CountdownEvent (3);
		AutoResetEvent[] evt = new AutoResetEvent [] { new AutoResetEvent (false), new AutoResetEvent (false), new AutoResetEvent (false), new AutoResetEvent (false)};

		CultureInfo[] initial_culture = new CultureInfo[4];
		CultureInfo[] changed_culture = new CultureInfo[4];
		CultureInfo[] changed_culture2 = new CultureInfo[4];
		CultureInfo alternative_culture = new CultureInfo("pt-BR");

		void StepAllPhases (int index)
		{
			initial_culture [index] = CultureInfo.CurrentCulture;
			/*Phase 1 - we witness the original value */
			barrier.Signal ();

			/*Phase 2 - main thread changes culture */
			evt [index].WaitOne ();

			/*Phase 3 - we witness the new value */
			changed_culture [index] = CultureInfo.CurrentCulture;
			barrier.Signal ();

			/* Phase 4 - main thread changes culture back */
			evt [index].WaitOne ();

			/*Phase 5 - we witness the new value */
			changed_culture2 [index] = CultureInfo.CurrentCulture;
			barrier.Signal ();
		}

		void ThreadWithoutChange () {
			StepAllPhases (1);
		}

		void ThreadWithChange () {
			Thread.CurrentThread.CurrentCulture = alternative_culture;
			StepAllPhases (2);
		}

		void ThreadPoolWithoutChange () {
			StepAllPhases (3);
		}

		[Test]
		[Category ("MultiThreaded")]
		public void DefaultThreadCurrentCulture () {

			Action c = () => {
				var orig_culture = CultureInfo.CurrentCulture;
				var new_culture = new CultureInfo("fr-FR");

				/* Phase 0 - warm up */
				new Thread (ThreadWithoutChange).Start ();
				new Thread (ThreadWithChange).Start ();
				Action x = ThreadPoolWithoutChange;
				x.BeginInvoke (null, null);

				/* Phase 1 - let everyone witness initial values */
				initial_culture [0] = CultureInfo.CurrentCulture;
				barrier.Wait ();
				barrier.Reset ();

				/* Phase 2 - change the default culture*/
				CultureInfo.DefaultThreadCurrentCulture = new_culture;
				evt [1].Set ();
				evt [2].Set ();
				evt [3].Set ();

				/* Phase 3 - let everyone witness the new value */
				changed_culture [0] = CultureInfo.CurrentCulture;
				barrier.Wait ();
				barrier.Reset ();

				/* Phase 4 - revert the default culture back to null */
				CultureInfo.DefaultThreadCurrentCulture = null;
				evt [1].Set ();
				evt [2].Set ();
				evt [3].Set ();

				/* Phase 5 - let everyone witness the new value */
				changed_culture2 [0] = CultureInfo.CurrentCulture;
				barrier.Wait ();
				barrier.Reset ();

				CultureInfo.DefaultThreadCurrentCulture = null;

				Assert.AreEqual (orig_culture, initial_culture [0], "#1");
				Assert.AreEqual (orig_culture, initial_culture [1], "#2");
				Assert.AreEqual (alternative_culture, initial_culture [2], "#3");
				Assert.AreEqual (orig_culture, initial_culture [3], "#4");

				Assert.AreEqual (new_culture, changed_culture [0], "#5");
				Assert.AreEqual (new_culture, changed_culture [1], "#6");
				Assert.AreEqual (alternative_culture, changed_culture [2], "#7");
				Assert.AreEqual (new_culture, changed_culture [3], "#8");

				Assert.AreEqual (orig_culture, changed_culture2 [0], "#9");
				Assert.AreEqual (orig_culture, changed_culture2 [1], "#10");
				Assert.AreEqual (alternative_culture, changed_culture2 [2], "#11");
				Assert.AreEqual (orig_culture, changed_culture2 [3], "#12");
			};
			var ar = c.BeginInvoke (null, null);
			ar.AsyncWaitHandle.WaitOne ();
		}

		[Test]
		[Category ("MultiThreaded")]
		public void DefaultThreadCurrentCultureIsIgnoredWhenCultureFlowsToThread ()
		{
			string us_str = null;
			string br_str = null;

			/* explicitly set CurrentCulture, as the documentation states:
			 * > If you have not explicitly set the culture of any existing
			 * > threads executing in an application domain, setting the
			 * > P:System.Globalization.CultureInfo.DefaultThreadCurrentCulture
			 * > property also changes the culture of these threads.
			 */
			Thread.CurrentThread.CurrentCulture = old_culture;

			var thread = new Thread (() => {
				CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");
				us_str = 100000.ToString ("C");
				CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("pt-BR");
				br_str = 100000.ToString ("C");
			});

			var expected = 100000.ToString ("C");

			thread.Start ();
			Assert.IsTrue (thread.Join (5000), "#0");
			CultureInfo.DefaultThreadCurrentCulture = null;
			Assert.AreEqual (expected, us_str, "#1");
			Assert.AreEqual (expected, br_str, "#2");
		}

		[Test]
		[Category ("MultiThreaded")]
		public void FlowCultureInfoFromParentThreadSinceNet46 ()
		{
			if (SynchronizationContext.Current != null) {
				Assert.Ignore ();
				return;
			}

			Func<Task> f = async () => {
				Thread.CurrentThread.CurrentUICulture = new CultureInfo ("pt-BR");
				await Task.Yield ();
				Assert.AreEqual ("pt-BR", Thread.CurrentThread.CurrentUICulture.Name);
			};

			Assert.IsTrue (f ().Wait (5 * 1000), "#1");
		}

		[Test]
		public void SpanToUpperInvariantDoesntUseCurrentCulture ()
		{
			string testStr = "test";
			var dst = new Span<char> (new char [testStr.Length]);
			CultureInfo savedCulture = CultureInfo.CurrentCulture;
			CultureInfo.CurrentCulture = new InterceptingLocale ();
			testStr.AsSpan ().ToUpperInvariant (dst); // should not throw InvalidOperationException ("Shouldn't be called.")
			CultureInfo.CurrentCulture = savedCulture;
			Assert.AreEqual ("TEST", dst.ToString ());
		}

		private class InterceptingLocale : CultureInfo
		{
			public InterceptingLocale () : base (string.Empty) { }
			public override TextInfo TextInfo => throw new InvalidOperationException ("Shouldn't be called.");
		}
	}
}
