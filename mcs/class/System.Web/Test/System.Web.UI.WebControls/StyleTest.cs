//
// Tests for System.Web.UI.WebControls.Style.cs 
//
// Author:
//	Peter Dennis Bartok (pbartok@novell.com)
//

//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//
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

using NUnit.Framework;
using System;
using System.Collections;
using System.Drawing;
using System.IO;
using System.Globalization;
using refl = System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MonoTests.System.Web.UI.WebControls
{
	public class StyleTestClass : Style {

		public StyleTestClass ()
			: base ()
		{
		}

		public StyleTestClass (StateBag bag)
			: base (bag)
		{
		}

		public StateBag StateBag {
			get { return base.ViewState; }
		}

		public bool Empty {
			get { return base.IsEmpty; }
		}

		public bool IsTracking {
			get { return base.IsTrackingViewState; }
		}
#if NET_2_0
		public void SetCssClass(string name) {
			Type style = Type.GetType("System.Web.UI.WebControls.Style, System.Web");
			if (style != null) {	
				refl.MethodInfo methodInfo = style.GetMethod("SetRegisteredCssClass",refl.BindingFlags.NonPublic | refl.BindingFlags.Instance);
				if (methodInfo != null) {
					object[] parameters =  new object[] {name};
					methodInfo.Invoke(this, parameters);
				}
			}
		}
#endif

		public string[] KeyValuePairs() {
			IEnumerator	e;
			string[]	result;
			int		item;

			e = ViewState.GetEnumerator();
			result = new string[ViewState.Keys.Count];
			item = 0;

			while (e.MoveNext()) {
				DictionaryEntry	d;
				StateItem	si;

				d = (DictionaryEntry)e.Current;
				si = (StateItem)d.Value;

				if (si.Value is String[]) {
					string[] values;

					values = (string[]) si.Value;
					result[item] = d.Key.ToString() + "=";
					if (values.Length > 0) {
						result[item] += values[0];

						for (int i = 1; i < values.Length; i++) {
							result[item] += ", " + values[i];
						}
					}
				} else {
					result[item] =  d.Key.ToString() + "=" + si.Value;
				}
				item++;
			}

			return result;
		}
	}

	[TestFixture]	
	public class StyleTest {
		private static HtmlTextWriter GetWriter () {
			StringWriter sw = new StringWriter ();
			sw.NewLine = "\n";
			return new HtmlTextWriter (sw);
		}

		private void SetSomeValues(Style s) {
			s.BackColor = Color.Red;
			s.ForeColor = Color.Green;
			s.Width = new Unit(10);
			s.Font.Bold = true;
		}

		private void SetAllValues(Style s) {
			s.BackColor = Color.Red;
			s.BorderColor = Color.Green;
			s.BorderStyle = BorderStyle.None;
			s.BorderWidth = new Unit(1);
			s.CssClass = "Boing";
			s.Font.Bold = true;
			s.Font.Italic = true;
			s.Font.Names = new string[2] {"namelist1", "namelist2"};
			//s.Font.Name = string.Empty;
			//s.Font.Names = new string[0];
			//Console.WriteLine("Font name after setting name: {0}", s.Font.ToString());
			s.Font.Overline = true;
			s.Font.Size = new FontUnit(1);
			//Console.WriteLine("Font name after setting size: {0}", s.Font.ToString());
			s.Font.Strikeout = true;
			s.Font.Underline = true;
			s.ForeColor = Color.Blue;
			s.Height = new Unit(2);
			s.Width = new Unit(3);
		}

		private bool IsEqual(object[] a1, object[] a2, string assertion) {
			int	matches;
			bool[]	notfound;	

			if (a1.Length != a2.Length) {
				if (assertion != null) {
					Assert.Fail(assertion + "( different length )");
				}
				return false;
			}

			matches = 0;
			notfound = new bool[a1.Length];

			for (int i = 0; i < a1.Length; i++) {
				for (int j = 0; j < a2.Length; j++) {
					if (a1[i].Equals(a2[j])) {
						matches++;
						break;
					}
				}
				if ((assertion != null) && (matches != i+1)) {
					Assert.Fail(assertion + "( missing " + a1[i].ToString() + " )");
				}
			}

			return matches == a1.Length;
		}

		[Test]
		public void Style_Defaults ()
		{
			Style s = new Style ();

			Assert.AreEqual (s.BackColor, Color.Empty, "Default1");
			Assert.AreEqual (s.BorderColor, Color.Empty, "Default22");
			Assert.AreEqual (s.BorderStyle, BorderStyle.NotSet, "Default3");
			Assert.AreEqual (s.BorderWidth, Unit.Empty, "Default4");
			Assert.AreEqual (s.CssClass, string.Empty, "Default5");
			Assert.AreEqual (s.ForeColor, Color.Empty, "Default6");
			Assert.AreEqual (s.Height, Unit.Empty, "Default7");
			Assert.AreEqual (s.Width, Unit.Empty, "Default8");
		}


		[Test]
		public void Style_State () {
			string[]	keyvalues;
			string[]	expect1 = {
						"BorderStyle=None",
						"Font_Bold=True",
						"Font_Italic=True",
						"Height=2px",
						"CssClass=Boing",
						"BorderWidth=1px",
						"ForeColor=Color [Blue]",
						"Font_Size=1pt",
						"Font_Overline=True",
						"Width=3px",
						"BorderColor=Color [Green]",
						"Font_Names=namelist1, namelist2",
						"Font_Underline=True",
						"BackColor=Color [Red]",
						"Font_Strikeout=True" };
			string[]	expect2 = {
						"BorderStyle=None",
						"Font_Bold=True",
						"Font_Italic=True",
						"Height=2px",
						"CssClass=Boing",
						"BorderWidth=1px",
						"ForeColor=Color [Blue]",
						"Font_Size=1pt",
						"Font_Overline=True",
						"Width=3px",
						"BorderColor=Color [Green]",
						"Font_Underline=True",
						"BackColor=Color [Red]",
						"Font_Strikeout=True" };
			string[]	expect3 = {
						"BorderStyle=None",
						"Font_Bold=True",
						"Font_Italic=True",
						"Height=2px",
						"CssClass=Boing",
						"BorderWidth=1px",
						"ForeColor=Color [Blue]",
						"Font_Size=1pt",
						"Font_Overline=True",
						"Width=3px",
						"BorderColor=Color [Green]",
						"Font_Names=",
						"Font_Underline=True",
						"BackColor=Color [Red]",
						"Font_Strikeout=True" };
			StyleTestClass	s;
			StyleTestClass	copy;

			s = new StyleTestClass();
			SetAllValues(s);
			keyvalues = s.KeyValuePairs();
			
			Assert.AreEqual (15, keyvalues.Length, "State1");
			IsEqual(keyvalues, expect1, "State2");

			s.Font.Name = string.Empty;
			keyvalues = s.KeyValuePairs();
			Assert.AreEqual (expect2.Length, keyvalues.Length, "State3");
			IsEqual(keyvalues, expect2, "State4");

			s.Font.Names = null;
			keyvalues = s.KeyValuePairs();
			Assert.AreEqual (expect2.Length, keyvalues.Length, "State5");
			IsEqual(keyvalues, expect2, "State6");

			copy = new StyleTestClass();
			copy.CopyFrom(s);
			keyvalues = copy.KeyValuePairs();
			Assert.AreEqual (expect3.Length, keyvalues.Length, "State7");
			IsEqual(keyvalues, expect3, "State8");

			Assert.AreEqual (false, copy.IsTracking, "State9");

		}

		[Test]
		public void Style_Merge ()
		{
			Style s = new Style ();
			Style copy = new Style ();

			SetSomeValues(s);
			copy.ForeColor = Color.Blue;

			copy.MergeWith(s);
			Assert.AreEqual (Color.Red, copy.BackColor, "Merge1");
			Assert.AreEqual (Color.Blue, copy.ForeColor, "Merge2");

			// Don't fail here
			copy.MergeWith(null);
		}

		[Test]
		public void Style_Copy ()
		{
			Style s = new Style ();
			Style copy = new Style ();

			SetSomeValues(s);

			copy.CopyFrom (s);
			Assert.AreEqual (Color.Red, s.BackColor, "Copy1");
		}

#if NET_2_0
		[Test]
		public void Style_CssClass ()
		{
			StyleTestClass s = new StyleTestClass ();

			Assert.AreEqual (String.Empty, s.RegisteredCssClass, "Css1");

			s.SetCssClass ("blah");
			Assert.AreEqual (String.Empty, s.RegisteredCssClass, "Css2");

			s.BackColor = Color.AliceBlue;
			Assert.AreEqual (String.Empty, s.RegisteredCssClass, "Css3");
		}
#endif

		[Test]
		public void StyleFonts () {
			Style s = new Style ();

			Assert.AreEqual(new string[0], s.Font.Names, "F1");

			s.Font.Name = string.Empty;
			Assert.AreEqual(new string[0], s.Font.Names, "F2");

			s.Font.Names = null;
			Assert.AreEqual(new string[0], s.Font.Names, "F3");
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void NullException1 ()
		{
			Style s = new Style ();

			s.Font.Name = null;
		}

		private Style GetStyle ()
		{
			Style s = new Style ();
			s.BackColor = Color.Aqua;
			s.BorderWidth = Unit.Pixel (1);
			return s;
		}

		private void CheckStyle (Style s)
		{
			Assert.AreEqual (Color.Aqua, s.BackColor, "BackColor");
			Assert.AreEqual (Unit.Pixel (1), s.BorderWidth, "BorderWidth");
		}


		[Test]
		public void CopyFrom_Null ()
		{
			Style s = GetStyle ();
			s.CopyFrom (null);
			CheckStyle (s);
		}

		[Test]
		public void CopyFrom_Self ()
		{
			Style s = GetStyle ();
			s.CopyFrom (s);
			CheckStyle (s);
		}

		[Test]
		public void CopyFrom_Empty ()
		{
			StyleTestClass s = new StyleTestClass ();
			s.CopyFrom (new Style ());
			Assert.IsTrue (s.Empty, "Empty");
		}

		[Test]
		public void CopyFrom ()
		{
			Style c = GetStyle ();
			Style s = GetStyle ();

			c.BorderColor = Color.Azure;
			c.BorderWidth = Unit.Empty;

			c.CopyFrom (s);
			CheckStyle (c);

			Assert.AreEqual (Color.Azure, c.BorderColor, "BorderColor");
			// CopyFrom doesn't do a Reset
		}

		[Test]
		public void CopyFrom_IsEmpty ()
		{
			StyleTestClass c = new StyleTestClass ();
			Style s = GetStyle ();

			s.BorderColor = Color.Azure;
			s.BorderWidth = Unit.Empty;

			c.CopyFrom (s);

			Assert.IsFalse (c.Empty, "IsEmpty");
		}

		[Test]
		public void Constructor_StateBag_Null ()
		{
			StyleTestClass s = new StyleTestClass (null);
			Assert.IsNotNull (s.StateBag, "StateBag");
			s.CssClass = "mono";
			Assert.AreEqual ("mono", s.CssClass, "CssClass");
		}

		[Test]
		public void Empty ()
		{
			StyleTestClass s = new StyleTestClass ();
			Assert.IsTrue (s.Empty, "Empty");
			Assert.AreEqual (0, s.StateBag.Count, "Count");
			s.StateBag["Mono"] = "go!";
			Assert.IsTrue (s.Empty, "Still Empty");
			Assert.AreEqual (1, s.StateBag.Count, "Count");
		}

		[Test]
		public void FontInfo_Empty ()
		{
			FontInfo f;
			StyleTestClass s = new StyleTestClass ();
			f = s.Font;
			Assert.IsTrue (s.Empty, "Empty after getter");
			s.Font.Name = "Arial";
			Assert.IsFalse (s.Empty, "No longer empty");
		}

		public void Render ()
		{
			HtmlTextWriter	writer;
			StyleTestClass	s;

			writer = StyleTest.GetWriter();
			s  = new StyleTestClass ();

			s.BorderColor = Color.BlueViolet;
			s.AddAttributesToRender(writer);
			// Figure out an order-independent way to verify rendered results
		}
	}
}
