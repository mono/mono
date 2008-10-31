// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// Copyright (c) 2006 Novell, Inc.
//

#if NET_2_0

using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;
using System.Windows.Forms.Layout;

using NUnit.Framework;
using System.Collections.Generic;

namespace MonoTests.System.Windows.Forms.Layout {

	[TestFixture]
	public class TableLayoutSettingsTypeConverterTest : TestHelper {
		
		[Test]
		public void CanConvertFrom ()
		{
			TableLayoutSettingsTypeConverter c = new TableLayoutSettingsTypeConverter ();

			Assert.IsTrue (c.CanConvertFrom (null, typeof (string)), "1");
			Assert.IsFalse (c.CanConvertFrom (null, typeof (int)), "2");
			Assert.IsFalse (c.CanConvertFrom (null, typeof (float)), "3");
			Assert.IsFalse (c.CanConvertFrom (null, typeof (object)), "4");
		}

		[Test]
		public void CanConvertTo ()
		{
			TableLayoutSettingsTypeConverter c = new TableLayoutSettingsTypeConverter ();

			Assert.IsTrue (c.CanConvertTo (null, typeof (string)), "1");
			Assert.IsFalse (c.CanConvertTo (null, typeof (int)), "2");
			Assert.IsFalse (c.CanConvertTo (null, typeof (float)), "3");
			Assert.IsFalse (c.CanConvertTo (null, typeof (object)), "4");
		}

		[Test]
		public void Roundtrip ()
		{
			TableLayoutSettingsTypeConverter c = new TableLayoutSettingsTypeConverter ();
			object result;

			string sv = @"<?xml version=""1.0"" encoding=""utf-16""?><TableLayoutSettings>"
				+ @"<Controls>"
				+   @"<Control Name=""userNameLabel"" Row=""0"" RowSpan=""1"" Column=""0"" ColumnSpan=""1"" />"
				+   @"<Control Name=""savePassword"" Row=""2"" RowSpan=""1"" Column=""1"" ColumnSpan=""1"" />"
				+   @"<Control Name=""userName"" Row=""0"" RowSpan=""1"" Column=""1"" ColumnSpan=""1"" />"
				+   @"<Control Name=""password"" Row=""1"" RowSpan=""1"" Column=""1"" ColumnSpan=""1"" />"
				+   @"<Control Name=""passwordLabel"" Row=""1"" RowSpan=""1"" Column=""0"" ColumnSpan=""1"" />"
				+ @"</Controls><Columns Styles=""AutoSize,0,Percent,100"" />"
				+ @"<Rows Styles=""AutoSize,0,AutoSize,0,AutoSize,0"" />"
				+ @"</TableLayoutSettings>";

			result = c.ConvertFrom (null, null, sv);

			Assert.AreEqual (typeof (TableLayoutSettings), result.GetType(), "1");

			TableLayoutSettings ts = (TableLayoutSettings)result;

			Assert.AreEqual (2, ts.ColumnStyles.Count, "2");
			Assert.AreEqual (SizeType.AutoSize, ts.ColumnStyles[0].SizeType, "3");
			Assert.AreEqual (0.0f, ts.ColumnStyles[0].Width, "4");
			Assert.AreEqual (SizeType.Percent, ts.ColumnStyles[1].SizeType, "5");
			Assert.AreEqual (100.0f, ts.ColumnStyles[1].Width, "6");

			Assert.AreEqual (3, ts.RowStyles.Count, "7");

			Assert.AreEqual (SizeType.AutoSize, ts.RowStyles[0].SizeType, "8");
			Assert.AreEqual (0.0f, ts.RowStyles[0].Height, "9");
			Assert.AreEqual (SizeType.AutoSize, ts.RowStyles[1].SizeType, "10");
			Assert.AreEqual (0.0f, ts.RowStyles[1].Height, "11");
			Assert.AreEqual (SizeType.AutoSize, ts.RowStyles[2].SizeType, "12");
			Assert.AreEqual (0.0f, ts.RowStyles[2].Height, "13");

			string rv = (string)c.ConvertTo (null, null, ts, typeof (string));

			// We do not guarantee the order of <Controls>, but the length should be the same
			Assert.AreEqual (sv.Length, rv.Length, "roundtrip");
		}

		//--------------------------------------------------------------
		private static TableLayoutSettings CreateSettingsEmpty()
		{
			return new TableLayoutPanel().LayoutSettings;
		}
		const String XmlSettingsEmpty
			= "<?xml version=\"1.0\" encoding=\"utf-16\"?>"
			+ "<TableLayoutSettings>"
			+ "<Controls />"
			+ "<Columns Styles=\"\" /><Rows Styles=\"\" />"
			+ "</TableLayoutSettings>";

		private static TableLayoutSettings CreateSettingsB()
		{
			TableLayoutPanel tlp = new TableLayoutPanel();
			tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
			tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
			tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 10F));
			tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 40F));
			tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 35F));
			return tlp.LayoutSettings;
		}
		const String XmlSettingsB
			= "<?xml version=\"1.0\" encoding=\"utf-16\"?>"
			+ "<TableLayoutSettings>"
			+ "<Controls />"
			+ "<Columns Styles=\"Percent,50,Percent,40,Absolute,10\" />"
			+ "<Rows Styles=\"Percent,40,Percent,35\" />"
			+ "</TableLayoutSettings>";

		private static TableLayoutSettings CreateSettingsC()
		{
			TableLayoutPanel tlp = new TableLayoutPanel();
			tlp.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
			tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 400));
			tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 40F));
			tlp.RowStyles.Add(new RowStyle(SizeType.AutoSize, 35F));
			tlp.RowStyles.Add(new RowStyle(SizeType.AutoSize));
			return tlp.LayoutSettings;
		}
		const String XmlSettingsC
			= "<?xml version=\"1.0\" encoding=\"utf-16\"?>"
			+ "<TableLayoutSettings>"
			+ "<Controls />"
			+ "<Columns Styles=\"AutoSize,0,Absolute,400\" />"
			+ "<Rows Styles=\"Percent,40,AutoSize,35,AutoSize,0\" />"
			+ "</TableLayoutSettings>";

		private static TableLayoutSettings CreateSettingsD()
		{
			TableLayoutPanel tlp = new TableLayoutPanel();
			tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
			tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
			return tlp.LayoutSettings;
		}
		const String XmlSettingsD
			= "<?xml version=\"1.0\" encoding=\"utf-16\"?>"
			+ "<TableLayoutSettings>"
			+ "<Controls />"
			+ "<Columns Styles=\"Percent,100\" />"
			+ "<Rows Styles=\"Percent,100\" />"
			+ "</TableLayoutSettings>";

		private static TableLayoutSettings CreateSettingsEDecimalOneItem()
		{
			TableLayoutPanel tlp = new TableLayoutPanel();
			tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 98.9F));
			tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 99.9F));
			return tlp.LayoutSettings;
		}
		const String XmlSettingsEDecimalOneItem
			= "<?xml version=\"1.0\" encoding=\"utf-16\"?>"
			+ "<TableLayoutSettings>"
			+ "<Controls />"
			+ "<Columns Styles=\"Percent,98.9\" />"
			+ "<Rows Styles=\"Percent,99.9\" />"
			+ "</TableLayoutSettings>";
		// See https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=156278&wa=wsignin1.0
		// and e.g. http://www.microsoft.com/communities/newsgroups/en-us/default.aspx?dg=microsoft.public.dotnet.internationalization&tid=f11516e2-33da-4047-8e2f-205df4ab09e5&cat=en_US_3fcb35c8-ccb3-4554-bd55-8038c0ecc923&lang=en&cr=US&sloc=&p=1
		// Uses comma as separator, but uses current cultures number formatting thus 99,9%
		const String XmlSettingsEDecimalOneItemCommaDecimalPoint
			= "<?xml version=\"1.0\" encoding=\"utf-16\"?>"
			+ "<TableLayoutSettings>"
			+ "<Controls />"
			+ "<Columns Styles=\"Percent,98,9\" />"
			+ "<Rows Styles=\"Percent,99,9\" />"
			+ "</TableLayoutSettings>";
		const String XmlSettingsEDecimalOneItemCommaDecimalPoint_RowDataFirst
			= "<?xml version=\"1.0\" encoding=\"utf-16\"?>"
			+ "<TableLayoutSettings>"
			+ "<Controls />"
			+ "<Rows Styles=\"Percent,99,9\" />"
			+ "<Columns Styles=\"Percent,98,9\" />"
			+ "</TableLayoutSettings>";

		private static TableLayoutSettings CreateSettingsF()
		{
			TableLayoutPanel tlp = new TableLayoutPanel();
			tlp.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
			tlp.RowStyles.Add(new RowStyle(SizeType.AutoSize));
			return tlp.LayoutSettings;
		}
		const String XmlSettingsF
			= "<?xml version=\"1.0\" encoding=\"utf-16\"?>"
			+ "<TableLayoutSettings>"
			+ "<Controls />"
			+ "<Columns Styles=\"AutoSize,0\" />"
			+ "<Rows Styles=\"AutoSize,0\" />"
			+ "</TableLayoutSettings>";
		const String XmlSettingsF_EmptyNumericElement
			= "<?xml version=\"1.0\" encoding=\"utf-16\"?>"
			+ "<TableLayoutSettings>"
			+ "<Controls />"
			+ "<Columns Styles=\"AutoSize,\" />"
			+ "<Rows Styles=\"AutoSize,\" />"
			+ "</TableLayoutSettings>";
		const String XmlSettingsF_NoNumericElement
			= "<?xml version=\"1.0\" encoding=\"utf-16\"?>"
			+ "<TableLayoutSettings>"
			+ "<Controls />"
			+ "<Columns Styles=\"AutoSize\" />"
			+ "<Rows Styles=\"AutoSize\" />"
			+ "</TableLayoutSettings>";

		private static TableLayoutSettings CreateSettingsGDecimalTwoItems()
		{
			TableLayoutPanel tlp = new TableLayoutPanel();
			tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12.3456F));
			tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3333F));
			tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 98.7654F));
			tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 33.3333F));
			return tlp.LayoutSettings;
		}
		const String XmlSettingsGDecimalTwoItems
			= "<?xml version=\"1.0\" encoding=\"utf-16\"?>"
			+ "<TableLayoutSettings>"
			+ "<Controls />"
			+ "<Columns Styles=\"Percent,12.3456,Percent,33.3333\" />"
			+ "<Rows Styles=\"Percent,98.7654,Percent,33.3333\" />"
			+ "</TableLayoutSettings>";
		// See https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=156278&wa=wsignin1.0
		// and e.g. http://www.microsoft.com/communities/newsgroups/en-us/default.aspx?dg=microsoft.public.dotnet.internationalization&tid=f11516e2-33da-4047-8e2f-205df4ab09e5&cat=en_US_3fcb35c8-ccb3-4554-bd55-8038c0ecc923&lang=en&cr=US&sloc=&p=1
		// Uses comma as separator, but uses current cultures number formatting thus 99,9%
		// !!!! e.g. <Columns Styles="Percent,99,9" />
		const String XmlSettingsGDecimalTwoItems_CommaDecimalPoint
			= "<?xml version=\"1.0\" encoding=\"utf-16\"?>"
			+ "<TableLayoutSettings>"
			+ "<Controls />"
			+ "<Columns Styles=\"Percent,12,3456,Percent,33,3333\" />"
			+ "<Rows Styles=\"Percent,98,7654,Percent,33,3333\" />"
			+ "</TableLayoutSettings>";
		const String XmlSettingsGDecimalTwoItems_CommaDecimalPoint_RowDataFirst
			= "<?xml version=\"1.0\" encoding=\"utf-16\"?>"
			+ "<TableLayoutSettings>"
			+ "<Controls />"
			+ "<Rows Styles=\"Percent,98,7654,Percent,33,3333\" />"
			+ "<Columns Styles=\"Percent,12,3456,Percent,33,3333\" />"
			+ "</TableLayoutSettings>";

		//--------------------------------------------------------------
		[Test]
		public void ConvertTo()
		{
			TableLayoutSettingsTypeConverter c = new TableLayoutSettingsTypeConverter();

			TableLayoutSettings tls;
			String result;
			//
			tls = CreateSettingsEmpty();
			result = c.ConvertToInvariantString(tls);
			Assert.AreEqual(XmlSettingsEmpty, result, "#1-empty");
			//
			tls = CreateSettingsB();
			result = c.ConvertToInvariantString(tls);
			Assert.AreEqual(XmlSettingsB, result, "#B");
			//
			tls = CreateSettingsC();
			result = c.ConvertToInvariantString(tls);
			Assert.AreEqual(XmlSettingsC, result, "#C");
			//
			tls = CreateSettingsD();
			result = c.ConvertToInvariantString(tls);
			Assert.AreEqual(XmlSettingsD, result, "#D");
			//
			tls = CreateSettingsEDecimalOneItem();
			result = c.ConvertToInvariantString(tls);
			Assert.AreEqual(XmlSettingsEDecimalOneItem, result, "#DDecimal");
			//
			tls = CreateSettingsF();
			result = c.ConvertToInvariantString(tls);
			Assert.AreEqual(XmlSettingsF, result, "#E");
		}

		[Test]
		public void ConvertTo_CurrentCultureHasDot_IsInvariantCulture()
		{
			CultureInfo culture = CultureInfo.InvariantCulture;
			ConvertTo_WithCurrentCulture(culture, XmlSettingsEDecimalOneItem);
		}

		[Test]
		public void ConvertTo_CurrentCultureHasDot()
		{
			CultureInfo culture = CultureInfo.GetCultureInfo("en-US");
			ConvertTo_WithCurrentCulture(culture, XmlSettingsEDecimalOneItem);
		}

		[Test]
		public void ConvertTo_CurrentCultureHasComma()
		{
			if (!IsMonoRuntime) {
				Assert.Ignore("Remember MSFT still uses the current culture decimal mark!");
			}
			//
			// Changed to always use dot-decimal-point float format
			//
			// This test is a test of current behaviour, and NOT *desired* behaviour.
			// It would likely be preferable for the float values (and everything) 
			// to be written using InvariantCulture, e.g. style.Width.ToString (CultureInfo.InvariantCulture)

			// The bug in MSFT's output mentioned above causes 99.9% to be written
			// culture sensitively as "99,9".
			// The bug depends on the *current* culture and NOT the culture passed-in 
			// to ConvertTo!!!
			//
			// de-DE or fr-FR would work here too!
			CultureInfo culture = CultureInfo.GetCultureInfo("ca-ES");
			ConvertTo_WithCurrentCulture(culture, XmlSettingsEDecimalOneItem);
			// was:	XmlSettingsEDecimalOneItemCommaDecimalPoint
			//
			// Just check that this culture normal produces "99,9"
			Assert.AreEqual("99,9", String.Format(culture, "{0}", 99.9d), "ToString d");
			Assert.AreEqual("99,9", String.Format(culture, "{0}", 99.9f), "ToString f");
			Assert.AreEqual("99,9", String.Format(culture, "{0}", 99.9m), "ToString m");
		}

		private static void ConvertTo_WithCurrentCulture(CultureInfo culture, String expectedXml)
		{
			TableLayoutSettingsTypeConverter c = new TableLayoutSettingsTypeConverter();

			String result;
			//
			TableLayoutSettings tls = CreateSettingsEDecimalOneItem();
			CultureInfo previous = global::System.Threading.Thread.CurrentThread.CurrentCulture;
			global::System.Threading.Thread.CurrentThread.CurrentCulture = culture;
			try {
				result = c.ConvertToString(null, culture, tls);
				Assert.AreEqual(expectedXml, result, "Culture passed");
				//
				result = c.ConvertToString(null, null, tls);
				Assert.AreEqual(expectedXml, result, "No culture passed");
				//
				result = c.ConvertToInvariantString(tls);
				Assert.AreEqual(expectedXml, result, "Invariant");
			} finally {
				global::System.Threading.Thread.CurrentThread.CurrentCulture = previous;
			}
		}

		//--------------------------------------------------------------
		void Assert_IsInstanceOfType(Type expected, Object value, String message)
		{
			if (expected == null)
				throw new ArgumentNullException("expected");
			//Assert.AreEqual(expected, value == null ? null : value.GetType(), message);
			if (!expected.IsInstanceOfType(value))
				throw new Exception("Booooo: " + message + Environment.NewLine
					+ "expected: '" + expected + "'" + Environment.NewLine
					+ "actual:   '" + (value == null ? "(null)" : value.GetType().ToString()) + "'");
		}

		[Test]
		public void ConvertFrom_NOT()
		{
			TableLayoutSettingsTypeConverter c = new TableLayoutSettingsTypeConverter();

			try {
				c.ConvertFrom(9999);
				Assert.Fail("should have thrown -- " + "#1");
			} catch (Exception ex) {
				Assert_IsInstanceOfType(typeof(NotSupportedException), ex, "ExType -- " + "#1");
			}
			try {
				c.ConvertFrom(null);
				Assert.Fail("should have thrown -- " + "#2");
			} catch (Exception ex) {
				Assert_IsInstanceOfType(typeof(NotSupportedException), ex, "ExType -- " + "#2");
			}
			try {
				c.ConvertFrom(String.Empty);
				Assert.Fail("should have thrown -- " + "#3");
			} catch (Exception ex) {
				Assert_IsInstanceOfType(typeof(global::System.Xml.XmlException), ex, "ExType -- " + "#3");
			}
		}

		[Test]
		public void ConvertFrom_notCheckResults()
		{
			TableLayoutSettingsTypeConverter c = new TableLayoutSettingsTypeConverter();

			c.ConvertFromInvariantString(XmlSettingsB);
			c.ConvertFromInvariantString(XmlSettingsC);
			c.ConvertFromInvariantString(XmlSettingsD);
			c.ConvertFromInvariantString(XmlSettingsEDecimalOneItem);
			c.ConvertFromInvariantString(XmlSettingsF);
		}

		[Test]
		public void ConvertFrom_notCheckResults_Empty()
		{
			TableLayoutSettingsTypeConverter c = new TableLayoutSettingsTypeConverter();

			c.ConvertFromInvariantString(XmlSettingsEmpty);
			// Mono fails (reports problem with value in Enum.Parse).
			// This is valid content (as output both platforms) so it should work.
		}

		[Test]
		public void ConvertFrom_notCheckResults_Bad_EmptyNumericElement()
		{
			TableLayoutSettingsTypeConverter c = new TableLayoutSettingsTypeConverter();

			try {
				// It's manually broken content, so it's ok to fail...
				c.ConvertFromInvariantString(XmlSettingsF_EmptyNumericElement);
			} catch (IndexOutOfRangeException) {
				// MSFT fails here.  Mono doesn't
			}
		}

		[Test]
		public void ConvertFrom_notCheckResults_Bad_NoNumericElement()
		{
			TableLayoutSettingsTypeConverter c = new TableLayoutSettingsTypeConverter();

			try {
				// It's manually broken content, so it's ok to fail...
				c.ConvertFromInvariantString(XmlSettingsF_NoNumericElement);
			} catch (IndexOutOfRangeException) {
				// Both fail here.
			}
		}

		[Test]
		public void ConvertFrom_notCheckResults_CommaDecimalPoint()
		{
			// The bug in MSFT's output mentioned above causes 99.9% to be written
			// culture sensitively as "99,9".
			// Test whether ConvertFrom crashes on reading such bad content.
			//
			TableLayoutSettingsTypeConverter c = new TableLayoutSettingsTypeConverter();

			c.ConvertFromInvariantString(XmlSettingsEDecimalOneItemCommaDecimalPoint);
		}

		[Test]
		public void ConvertFrom_notCheckResults_CommaDecimalPoint_RowDataFirst()
		{
			// The bug in MSFT's output mentioned above causes 99.9% to be written
			// culture sensitively as "99,9".
			// Test whether ConvertFrom crashes on reading such bad content.
			//
			TableLayoutSettingsTypeConverter c = new TableLayoutSettingsTypeConverter();

			c.ConvertFromInvariantString(XmlSettingsEDecimalOneItemCommaDecimalPoint_RowDataFirst);
		}

		//------
		struct TestRow
		{
			public String Name;
			public TableLayoutSettings Settings;
			public String XmlContent;
			public Directions Directions;

			public TestRow(TableLayoutSettings settings, String xmlContent, String name, Directions directions)
			{
				this.Name = name;
				this.Settings = settings;
				this.XmlContent = xmlContent;
				this.Directions = directions;
			}
		}
		[Flags]
		enum Directions
		{
			None = 0,
			To = 1,
			From = 2, 
			Both
		}

		IList<TestRow> tests;

		public TableLayoutSettingsTypeConverterTest()
		{
			BuildLoopTestCaseList();
		}

		private void  BuildLoopTestCaseList()
		{
			tests = new List<TestRow>();
			//==================
			// **very** basic test of equality checking methods...
			//-tests.Add(new TestRow(CreateSettingsC(), XmlSettingsB, "XmlSettingsB vs C!!!!", Directions.From));
			//==================
			//--------
			tests.Add(new TestRow(CreateSettingsB(), XmlSettingsB, "XmlSettingsB", Directions.Both));
			//--------
			tests.Add(new TestRow(CreateSettingsC(), XmlSettingsC, "XmlSettingsC", Directions.Both));
			//--------
			tests.Add(new TestRow(CreateSettingsD(), XmlSettingsD, "XmlSettingsD", Directions.Both));
			//--------
			tests.Add(new TestRow(CreateSettingsEDecimalOneItem(), XmlSettingsEDecimalOneItem, 
				"XmlSettingsEDecimalOneItem", 
				Directions.Both));
			tests.Add(new TestRow(CreateSettingsEDecimalOneItem(), XmlSettingsEDecimalOneItemCommaDecimalPoint, 
				"XmlSettingsEDecimalOneItem_CommaDecimalPoint", 
				Directions.From));	// 'To' case is the one above.
			tests.Add(new TestRow(CreateSettingsEDecimalOneItem(), XmlSettingsEDecimalOneItemCommaDecimalPoint_RowDataFirst, 
				"XmlSettingsEDecimalOneItem_CommaDecimalPoint_RowDataFirst", 
				Directions.From));	// 'To' case is the one above.
			//--------
			tests.Add(new TestRow(CreateSettingsF(), XmlSettingsF, "XmlSettingsF", Directions.Both));
			tests.Add(new TestRow(CreateSettingsF(), XmlSettingsF_EmptyNumericElement, 
				"XmlSettingsF_EmptyNumericElement", 
				Directions.None));	// Fails on both platforms, see individual test methods.
			tests.Add(new TestRow(CreateSettingsF(), XmlSettingsF_NoNumericElement, 
				"XmlSettingsF_NoNumericElement",
				Directions.None));	// Fails on both platforms, see individual test methods.
			//--------
			tests.Add(new TestRow(CreateSettingsGDecimalTwoItems(), XmlSettingsGDecimalTwoItems, 
				"XmlSettingsGDecimalTwoItems", 
				Directions.Both));
			tests.Add(new TestRow(CreateSettingsGDecimalTwoItems(), XmlSettingsGDecimalTwoItems_CommaDecimalPoint,
				"XmlSettingsGDecimalTwoItems_CommaDecimalPoint", 
				Directions.From));	// 'To' case is the one above.
			tests.Add(new TestRow(CreateSettingsGDecimalTwoItems(), XmlSettingsGDecimalTwoItems_CommaDecimalPoint_RowDataFirst,
				"XmlSettingsGDecimalTwoItems_CommaDecimalPoint_RowDataFirst", 
				Directions.From));	// 'To' case is the one above.
			//--------
			tests.Add(new TestRow(CreateSettingsEmpty(), XmlSettingsEmpty, 
				"XmlSettingsEmpty", Directions.Both));
		}


		[Test]
		public void ConvertTo_Loop_InvariantCulture()
		{
			ConvertTo_Loop(CultureInfo.InvariantCulture);
		}

		[Test]
		public void ConvertTo_Loop_CultureWithComma()
		{
			ConvertTo_Loop(CultureInfo.GetCultureInfo("de-DE"));
		}

		[Test]
		public void ConvertTo_Loop_CurrentCultureWithComma_CultureWithComma()
		{
			if (!IsMonoRuntime) {
				Assert.Ignore("Remember MSFT still uses the current culture decimal mark!");
			}
			//
			CultureInfo previous = global::System.Threading.Thread.CurrentThread.CurrentCulture;
			global::System.Threading.Thread.CurrentThread.CurrentCulture
				= CultureInfo.GetCultureInfo("de-DE");
			try {
				ConvertTo_Loop(CultureInfo.GetCultureInfo("de-DE"));
			} finally {
				global::System.Threading.Thread.CurrentThread.CurrentCulture = previous;
			}
		}

		private bool IsMonoRuntime
		{
			get { return Type.GetType("Mono.Runtime") != null; }
		}


		// Ohh for a more recent version on NUnit, (/and addin) to do row test cases!
		private void ConvertTo_Loop(CultureInfo culturePassedToConvertTo)
		{
			TableLayoutSettingsTypeConverter c = new TableLayoutSettingsTypeConverter();

			int i = 0;
			foreach (TestRow row in tests) {
				if ((row.Directions & Directions.To) == 0) {
					//Console.WriteLine("ConvertTo_Loop skipping: " + row.Name);
					continue;
				}
				//
				String title = String.Format("#{0}, {1}", i, row.Name);
				try {
					String result = (String)c.ConvertTo(null, culturePassedToConvertTo, row.Settings, typeof(String));
					Assert.AreEqual(row.XmlContent, result, row.Name);
					title = null;
				} finally {
					if (title != null)
						Console.WriteLine("ConvertTo_Loop row that failed ** : " + title);
				}
				++i;
			}
		}

		//--------
		[Test]
		public void ConvertFrom_Loop_InvariantCulture()
		{
			ConvertFrom_Loop(CultureInfo.InvariantCulture);
		}

		[Test]
		public void ConvertFrom_Loop_CultureWithComma()
		{
			ConvertFrom_Loop(CultureInfo.GetCultureInfo("de-DE"));
		}
		[Test]
		public void ConvertFrom_Loop_CurrentCultureWithComma_CultureWithComma()
		{
			CultureInfo previous = global::System.Threading.Thread.CurrentThread.CurrentCulture;
			global::System.Threading.Thread.CurrentThread.CurrentCulture
				= CultureInfo.GetCultureInfo("de-DE");
			try {
				ConvertFrom_Loop(CultureInfo.GetCultureInfo("de-DE"));
			} finally {
				global::System.Threading.Thread.CurrentThread.CurrentCulture = previous;
			}
		}
		[Test]
		public void ConvertFrom_Loop_CurrentCultureWithComma_InvariantCulture()
		{
			CultureInfo previous = global::System.Threading.Thread.CurrentThread.CurrentCulture;
			global::System.Threading.Thread.CurrentThread.CurrentCulture
				= CultureInfo.GetCultureInfo("de-DE");
			try {
				ConvertFrom_Loop(CultureInfo.InvariantCulture);
			} finally {
				global::System.Threading.Thread.CurrentThread.CurrentCulture = previous;
			}
		}

		// Ohh for a more recent version on NUnit, (/and addin) to do row test cases!
		private void ConvertFrom_Loop(CultureInfo culturePassedToConvertFrom)
		{
			TableLayoutSettingsTypeConverter c = new TableLayoutSettingsTypeConverter();

			int i = 0;
			foreach (TestRow row in tests) {
				if ((row.Directions & Directions.From) == 0) {
					//Console.WriteLine("ConvertFrom_Loop skipping: " + row.Name);
					continue;
				}
				//
				String title = String.Format("#{0}, {1}", i, row.Name);
				try {
					TableLayoutSettings result = (TableLayoutSettings)
						c.ConvertFrom(null, culturePassedToConvertFrom, row.XmlContent);
					Assert_AreEqual(row.Settings, result, row.Name);
					title = null;
				} finally {
					if (title != null)
						Console.WriteLine("ConvertFrom_Loop row that failed ** : " + title);
				}
				++i;
			}
		}

		//--------
		static void Assert_AreEqual(TableLayoutSettings expected, TableLayoutSettings actual, String message)
		{
			Assert_AreEqual(expected.ColumnStyles, actual.ColumnStyles, "ColumnStyles -- " + message);
			Assert_AreEqual(expected.RowStyles, actual.RowStyles, "RowStyles -- " + message);
		}

		static void Assert_AreEqual(TableLayoutColumnStyleCollection expected, TableLayoutColumnStyleCollection actual, String message)
		{
			for (int i = 0; i < Math.Min(expected.Count, actual.Count); ++i) {
				ColumnStyle expectedCur = expected[i];
				ColumnStyle actualCur = actual[i];
				Assert_AreEqual(expectedCur, actualCur, "TableLayoutColumnStyleCollection[" + i + "] -- " + message);
			}
			// Check this *after*, so that if the initial values in the lists don't match 
			// that's reported instead -- it makes it more obvious what is being mis-parsed.
			Assert.AreEqual(expected.Count, actual.Count, "TableLayoutColumnStyleCollection.Count -- " + message);
		}
		static void Assert_AreEqual(TableLayoutRowStyleCollection expected, TableLayoutRowStyleCollection actual, String message)
		{
			for (int i = 0; i < Math.Min(expected.Count, actual.Count); ++i) {
				RowStyle expectedCur = expected[i];
				RowStyle actualCur = actual[i];
				Assert_AreEqual(expectedCur, actualCur, "TableLayoutRowStyleCollection[" + i + "] -- " + message);
			}
			// Check this *after*, so that if the initial values in the lists don't match 
			// that's reported instead -- it makes it more obvious what is being mis-parsed.
			Assert.AreEqual(expected.Count, actual.Count, "TableLayoutRowStyleCollection.Count -- " + message);
		}

		static void Assert_AreEqual(ColumnStyle expected, ColumnStyle actual, String message)
		{
			Assert.AreEqual(expected.SizeType, actual.SizeType, "ColumnStyle.SizeType -- " + message);
			Assert.AreEqual(expected.Width, actual.Width, "ColumnStyle.Width -- " + message);
		}
		static void Assert_AreEqual(RowStyle expected, RowStyle actual, String message)
		{
			Assert.AreEqual(expected.SizeType, actual.SizeType, "RowStyle.SizeType -- " + message);
			Assert.AreEqual(expected.Height, actual.Height, "RowStyle.Height -- " + message);
		}

	}
}

#endif
