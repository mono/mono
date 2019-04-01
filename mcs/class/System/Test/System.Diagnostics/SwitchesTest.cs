//
// SwitchesTest.cs:
// 		NUnit Test Cases for System.Diagnostics.BooleanSwitch and
// 		System.Diagnostics.TraceSwitch
//
// Authors:
//   Jonathan Pryor (jonpryor@vt.edu)
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2002 Jonathan Pryor
// (C) 2003 Martin Willemoes Hansen
//

#if !MOBILE && !XAMMAC_4_5

using NUnit.Framework;
using System;
using System.Text;
using System.Collections;
using System.Configuration;
using System.Diagnostics;

namespace MonoTests.System.Diagnostics {

	class TestNewSwitch : Switch {
		private string v;
		private StringBuilder ops = new StringBuilder ();
		private const string expected = 
			".ctor\n" +
			"get_TestValue\n" +
			"OnSwitchSettingChanged\n" +
			"GetSetting\n";

		public TestNewSwitch (string name, string desc)
			: base (name, desc)
		{
			ops.Append (".ctor\n");
		}

		public string TestValue {
			get {
				ops.Append ("get_TestValue\n");
				// ensure that the .config file is read in
				int n = base.SwitchSetting;
				// remove warning about unused variable
				n = 5;
				return v;
			}
		}

		public string [] ExposeSupportedAttributes ()
		{
			return GetSupportedAttributes ();
		}

		public bool Validate ()
		{
			return expected == ops.ToString();
		}

		private void GetSetting ()
		{
			ops.Append ("GetSetting\n");
			IDictionary d = (IDictionary) ConfigurationSettings.GetConfig ("system.diagnostics");
			if (d != null) {
				d = (IDictionary) d ["switches"];
				if (d != null) {
					v = d [DisplayName].ToString();
				}
			}
		}

		protected override void OnSwitchSettingChanged ()
		{
			ops.Append ("OnSwitchSettingChanged\n");

			GetSetting ();
		}
	}

	class TestNullSwitch : Switch {
		public TestNullSwitch () : base (null, null)
		{
		}
	}

	[TestFixture]
	public class SwitchesTest {
    
		private static BooleanSwitch bon = new BooleanSwitch ("bool-true", "");
		private static BooleanSwitch bon2 = new BooleanSwitch ("bool-true-2", "");
		private static BooleanSwitch bon3 = new BooleanSwitch ("bool-true-3", "");
		private static BooleanSwitch boff = new BooleanSwitch ("bool-false", "");
		private static BooleanSwitch boff2 = new BooleanSwitch ("bool-default", "");

		private static TraceSwitch toff = new TraceSwitch ("trace-off", "");
		private static TraceSwitch terror = new TraceSwitch ("trace-error", "");
		private static TraceSwitch twarning = new TraceSwitch ("trace-warning", "");
		private static TraceSwitch tinfo = new TraceSwitch ("trace-info", "");
		private static TraceSwitch tverbose = new TraceSwitch ("trace-verbose", "");
		private static TraceSwitch tdefault = new TraceSwitch ("no-value", "");
		private static TraceSwitch tsv = new TraceSwitch ("string-value", "");
		private static TraceSwitch tnegative = new TraceSwitch ("trace-negative", "");

		private static TestNewSwitch tns = new TestNewSwitch ("custom-switch", "");

		[Test]
		public void BooleanSwitches ()
		{
			Assert.IsTrue (bon.Enabled, "#BS:T:1");
			Assert.IsTrue (bon2.Enabled, "#BS:T:2");
			Assert.IsTrue (bon3.Enabled, "#BS:T:3");
			Assert.IsTrue (!boff.Enabled, "#BS:F:1");
			Assert.IsTrue (!boff2.Enabled, "#BS:F:2");
		}

		[Test]
		public void TraceSwitches ()
		{
			// The levels 0..4:
			CheckTraceSwitch (toff,      false, false, false, false);
			CheckTraceSwitch (terror,    true,  false, false, false);
			CheckTraceSwitch (twarning,  true,  true,  false, false);
			CheckTraceSwitch (tinfo,     true,  true,  true,  false);
			CheckTraceSwitch (tverbose,  true,  true,  true,  true);

			// Default value is 0
			CheckTraceSwitch (tdefault,  false, false, false, false);

			// string value can't be converted to int, so default is 0
			CheckTraceSwitch (tsv,       false, false, false, false);

			// negative number is < 0, so all off
			CheckTraceSwitch (tnegative, false, false, false, false);
		}

		private void CheckTraceSwitch (TraceSwitch ts, bool te, bool tw, bool ti, bool tv)
		{
			string desc = string.Format ("#TS:{0}", ts.DisplayName);
			Assert.AreEqual (te, ts.TraceError, desc + ":TraceError");
			Assert.AreEqual (tw, ts.TraceWarning, desc + ":TraceWarning");
			Assert.AreEqual (ti, ts.TraceInfo, desc + ":TraceInfo");
			Assert.AreEqual (tv, ts.TraceVerbose, desc + ":TraceVerbose");
		}

		[Test]
		[Ignore ("this test depends on 1.x configuration type")]
		public void NewSwitch ()
		{
			Assert.AreEqual ("42", tns.TestValue, "#NS:TestValue");
			Assert.IsTrue (tns.Validate(), "#NS:Validate");
		}

		[Test]
		public void GetSupportedAttributes ()
		{
			Assert.IsNull (tns.ExposeSupportedAttributes ());
		}

		[Test] // no ArgumentNullException happens...
		public void BooleanSwitchNullDefaultValue ()
		{
			new BooleanSwitch ("test", "", null);
		}

		[Test]
		public void BooleanSwitchValidDefaultValue ()
		{
			BooleanSwitch s = new BooleanSwitch ("test", "", "2");
			Assert.IsTrue (s.Enabled, "#1");
			s = new BooleanSwitch ("test", "", "0");
			Assert.IsTrue (!s.Enabled, "#2");
			s = new BooleanSwitch ("test", "", "true");
			Assert.IsTrue (s.Enabled, "#3");
			s = new BooleanSwitch ("test", "", "True");
			Assert.IsTrue (s.Enabled, "#4");
			s = new BooleanSwitch ("test", "", "truE");
			Assert.IsTrue (s.Enabled, "#5");
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void BooleanSwitchInvalidDefaultValue ()
		{
			BooleanSwitch s = new BooleanSwitch ("test", "", "hoge");
			Assert.IsTrue (!s.Enabled);
		}

		[Test]
		public void NullSwitchHasEmptyDisplayNameAndDescription ()
		{
			var s = new TestNullSwitch ();
			AssertHelper.IsEmpty (s.DisplayName);
			AssertHelper.IsEmpty (s.Description);
		}
	}
}

#endif
