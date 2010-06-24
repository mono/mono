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
using MonoTests.SystemWeb.Framework;
using MonoTests.stand_alone.WebHarness;

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
			Type style = typeof (Style);
			if (style != null) {	
				refl.MethodInfo methodInfo = style.GetMethod("SetRegisteredCssClass",refl.BindingFlags.NonPublic | refl.BindingFlags.Instance);
				if (methodInfo != null) {
					object[] parameters =  new object[] {name};
					methodInfo.Invoke(this, parameters);
				}
			}
		}

		public override void AddAttributesToRender (HtmlTextWriter writer, WebControl owner) {
			base.AddAttributesToRender (writer, owner);
		}

		protected override void FillStyleAttributes (CssStyleCollection attributes, IUrlResolutionService urlResolver) {
			base.FillStyleAttributes (attributes, urlResolver);
			attributes.Add ("FillStyleAttributes", "FillStyleAttributes");
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

		public bool SetBitCalledFlag = false;
		public int SetBitCalledValue = 0;
		protected override void SetBit (int bit) {
			SetBitCalledFlag = true;
			SetBitCalledValue = bit;
			base.SetBit (bit);
		}
	}

	[TestFixture]	
	public class StyleTest {

		[TestFixtureTearDown]
		public void TearDown ()
		{
			WebTest.Unload ();
		}

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
		public void Style_ViewState () {
			Style s = new Style ();
			((IStateManager) s).TrackViewState ();
			SetSomeValues (s);
			object state = ((IStateManager) s).SaveViewState ();

			Style copy = new Style ();
			((IStateManager) copy).LoadViewState (state);

			Assert.AreEqual (Color.Red, copy.BackColor, "ViewState1");
			Assert.AreEqual (Color.Green, copy.ForeColor, "ViewState2");
			Assert.AreEqual (new Unit (10), copy.Width, "ViewState3");
			Assert.AreEqual (true, copy.Font.Bold, "ViewState4");
		}

		[Test]
		public void Style_ViewState2 () {
			Style s = new Style (null);
			((IStateManager) s).TrackViewState ();
			SetSomeValues (s);
			object state = ((IStateManager) s).SaveViewState ();

			Assert.AreEqual (null, state, "ViewState2");
		}

		[Test]
		public void Style_ViewState3 () {
			Style s = new Style (new StateBag());
			((IStateManager) s).TrackViewState ();
			SetSomeValues (s);
			object state = ((IStateManager) s).SaveViewState ();

			Assert.AreEqual (null, state, "ViewState3");
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

		[Test]
		public void Style_CssClass ()
		{
			StyleTestClass s = new StyleTestClass ();
			Assert.AreEqual (String.Empty, s.CssClass, "#A1");

			s.CssClass = "css1";
			Assert.AreEqual ("css1", s.CssClass, "#A2");

			s.CssClass = String.Empty;
			Assert.AreEqual (String.Empty, s.CssClass, "#A3");

			s.CssClass = null;
			Assert.AreEqual (String.Empty, s.CssClass, "#A4");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Style_BorderStyle_InvalidValue1 ()
		{
			StyleTestClass s = new StyleTestClass ();
			s.BorderStyle = (BorderStyle)(BorderStyle.NotSet - 1);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Style_BorderStyle_InvalidValue2 ()
		{
			StyleTestClass s = new StyleTestClass ();
			s.BorderStyle = (BorderStyle)(BorderStyle.Outset + 1);
		}
		
#if NET_2_0
		[Test]
		public void Style_RegisteredCssClass ()
		{
			StyleTestClass s = new StyleTestClass ();

			Assert.AreEqual (String.Empty, s.RegisteredCssClass, "Css1");

			s.SetCssClass ("blah");
			Assert.AreEqual ("blah", s.RegisteredCssClass, "Css2");

			s.BackColor = Color.AliceBlue;
			Assert.AreEqual ("blah", s.RegisteredCssClass, "Css3");
		}
		
		[Test]
		[Category ("NunitWeb")]
		public void Style_AddRegisteredCssClassAttribute () {
			new WebTest (PageInvoker.CreateOnLoad (Style_AddRegisteredCssClassAttribute_Load)).Run ();
		}
		
		public static void Style_AddRegisteredCssClassAttribute_Load (Page p) {
			StringWriter sw = new StringWriter ();
			HtmlTextWriter tw = new HtmlTextWriter (sw);
			Style s = new Style ();
			s.CssClass = "MyClass";
			s.BackColor = Color.AliceBlue;
			s.AddAttributesToRender (tw);
			tw.RenderBeginTag ("span");
			tw.RenderEndTag ();
			Assert.AreEqual (true, sw.ToString ().Contains ("class=\"MyClass\""), "AddRegisteredCssClassAttribute#1");
			Assert.AreEqual (true, sw.ToString ().Contains ("style"), "AddRegisteredCssClassAttribute#2");
			
			sw = new StringWriter ();
			tw = new HtmlTextWriter (sw);
			s = new Style ();
			s.BackColor = Color.AliceBlue;
			p.Header.StyleSheet.RegisterStyle (s, p);
			s.AddAttributesToRender (tw);
			tw.RenderBeginTag ("span");
			tw.RenderEndTag ();
			Assert.AreEqual (true, sw.ToString ().Contains ("class"), "AddRegisteredCssClassAttribute#3");
			Assert.AreEqual (false, sw.ToString ().Contains ("style"), "AddRegisteredCssClassAttribute#4");
			
			sw = new StringWriter ();
			tw = new HtmlTextWriter (sw);
			s = new Style ();
			s.BackColor = Color.AliceBlue;
			s.CssClass = "MyClass";
			p.Header.StyleSheet.RegisterStyle (s, p);
			s.AddAttributesToRender (tw);
			tw.RenderBeginTag ("span");
			tw.RenderEndTag ();
			Assert.AreEqual (sw.ToString ().LastIndexOf ("class"), sw.ToString ().IndexOf ("class"), "AddRegisteredCssClassAttribute#5");
			Assert.AreEqual (false, sw.ToString ().Contains ("style"), "AddRegisteredCssClassAttribute#6");
			Assert.AreEqual (true, sw.ToString ().Contains ("class=\"MyClass "), "AddRegisteredCssClassAttribute#7");

			s = new Style ();
			p.Header.StyleSheet.RegisterStyle (s, p);
			Assert.AreEqual (false, s.IsEmpty, "AddRegisteredCssClassAttribute#8");
		}

		[Test]
		public void Style_AddAttributesToRender_use_FillStyleAttributes () {
			StringWriter sw = new StringWriter ();
			HtmlTextWriter tw = new HtmlTextWriter (sw);
			StyleTestClass s = new StyleTestClass ();
			s.AddAttributesToRender (tw);
			tw.RenderBeginTag ("span");
			tw.RenderEndTag ();

			string origHtml = "<span style=\"FillStyleAttributes:FillStyleAttributes;\"></span>";
			string renderedHtml = sw.ToString ();
			HtmlDiff.AssertAreEqual (origHtml, renderedHtml, "AddAttributesToRender_use_FillStyleAttributes#2");
		}

		[Test]
		public void Style_GetStyleAttributes () {
			Style s;
			CssStyleCollection css;

			s = new Style ();
			css = s.GetStyleAttributes (null);
			Assert.AreEqual (0, css.Count, "GetStyleAttributes#1");

			s.Font.Bold = true;
			s.Font.Italic = true;
			s.Font.Size = 10;
			s.Font.Names = new string [] { "Arial", "Veranda" };
			s.Font.Overline = true;
			s.Font.Strikeout = true;
			s.Font.Underline = true;
			css = s.GetStyleAttributes (null);
			Assert.AreEqual ("bold", css ["font-weight"], "GetStyleAttributes#2");
			Assert.AreEqual ("italic", css ["font-style"], "GetStyleAttributes#3");
			Assert.AreEqual ("10pt", css ["font-size"], "GetStyleAttributes#4");
			Assert.AreEqual ("Arial,Veranda", css ["font-family"], "GetStyleAttributes#5");
			Assert.AreEqual (true, css ["text-decoration"].Contains ("overline"), "GetStyleAttributes#6");
			Assert.AreEqual (true, css ["text-decoration"].Contains ("line-through"), "GetStyleAttributes#7");
			Assert.AreEqual (true, css ["text-decoration"].Contains ("underline"), "GetStyleAttributes#8");

			s.Font.Names = null;
			css = s.GetStyleAttributes (null);
			Assert.AreEqual (null, css ["font-family"], "GetStyleAttributes#9");

			s.Font.Name = "Arial, Veranda";
			css = s.GetStyleAttributes (null);
			Assert.AreEqual ("Arial, Veranda", css ["font-family"], "GetStyleAttributes#10");

			s.Font.Name = "";
			css = s.GetStyleAttributes (null);
			Assert.AreEqual (null, css ["font-family"], "GetStyleAttributes#11");

			s.Font.Bold = false;
			s.Font.Italic = false;
			s.Font.Size = FontUnit.Empty;
			s.Font.Overline = false;
			s.Font.Strikeout = false;
			s.Font.Underline = false;
			css = s.GetStyleAttributes (null);
			Assert.AreEqual ("normal", css ["font-weight"], "GetStyleAttributes#12");
			Assert.AreEqual ("normal", css ["font-style"], "GetStyleAttributes#13");
			Assert.AreEqual (null, css ["font-size"], "GetStyleAttributes#14");
			Assert.AreEqual ("none", css ["text-decoration"], "GetStyleAttributes#15");

			s.Reset ();
			css = s.GetStyleAttributes (null);
			Assert.AreEqual (0, css.Count, "GetStyleAttributes#16");

			s.Reset ();
			s.Font.Underline = false;
			css = s.GetStyleAttributes (null);
			Assert.AreEqual ("none", css ["text-decoration"], "GetStyleAttributes#17");

			s.Reset ();
			s.BorderWidth = 1;
			s.BorderStyle = BorderStyle.Dashed;
			css = s.GetStyleAttributes (null);
			Assert.AreEqual ("Dashed", css ["border-style"], "GetStyleAttributes#18");
			Assert.AreEqual ("1px", css ["border-width"], "GetStyleAttributes#19");

			s.BorderStyle = BorderStyle.NotSet;
			css = s.GetStyleAttributes (null);
			Assert.AreEqual ("solid", css ["border-style"], "GetStyleAttributes#20");
			Assert.AreEqual ("1px", css ["border-width"], "GetStyleAttributes#21");

			s.BorderWidth = 0;
			css = s.GetStyleAttributes (null);
			Assert.AreEqual (null, css ["border-style"], "GetStyleAttributes#22");
			Assert.AreEqual ("0px", css ["border-width"], "GetStyleAttributes#23");
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
		
		[Test]
		public void SetBitCalledWhenSetProperty () {
			StyleTestClass s = new StyleTestClass ();

			s.SetBitCalledFlag = false;
			s.BackColor = Color.Aqua;
			Assert.IsTrue (s.SetBitCalledFlag, "SetBit() was not called : BackColor");
			Assert.AreEqual (0x08, s.SetBitCalledValue, "SetBit() was called with wrong argument : BackColor");

			s.SetBitCalledFlag = false;
			s.BorderColor = Color.Blue;
			Assert.IsTrue (s.SetBitCalledFlag, "SetBit() was not called : BorderColor");
			Assert.AreEqual (0x10, s.SetBitCalledValue, "SetBit() was called with wrong argument : BorderColor");

			s.SetBitCalledFlag = false;
			s.BorderStyle = BorderStyle.Dashed;
			Assert.IsTrue (s.SetBitCalledFlag, "SetBit() was not called : BorderStyle");
			Assert.AreEqual (0x40, s.SetBitCalledValue, "SetBit() was called with wrong argument : BorderStyle");

			s.SetBitCalledFlag = false;
			s.BorderWidth = 1;
			Assert.IsTrue (s.SetBitCalledFlag, "SetBit() was not called : BorderWidth");
			Assert.AreEqual (0x20, s.SetBitCalledValue, "SetBit() was called with wrong argument : BorderWidth");

			s.SetBitCalledFlag = false;
			s.CssClass = "class";
			Assert.IsTrue (s.SetBitCalledFlag, "SetBit() was not called : CssClass");
			Assert.AreEqual (0x02, s.SetBitCalledValue, "SetBit() was called with wrong argument : CssClass");

			s.SetBitCalledFlag = false;
			s.ForeColor = Color.Red;
			Assert.IsTrue (s.SetBitCalledFlag, "SetBit() was not called : ForeColor");
			Assert.AreEqual (0x04, s.SetBitCalledValue, "SetBit() was called with wrong argument : ForeColor");

			s.SetBitCalledFlag = false;
			s.Height = 1;
			Assert.IsTrue (s.SetBitCalledFlag, "SetBit() was not called : Height");
			Assert.AreEqual (0x80, s.SetBitCalledValue, "SetBit() was called with wrong argument : Height");

			s.SetBitCalledFlag = false;
			s.Width = 1;
			Assert.IsTrue (s.SetBitCalledFlag, "SetBit() was not called : Width");
			Assert.AreEqual (0x100, s.SetBitCalledValue, "SetBit() was called with wrong argument : Width");
			
			s.SetBitCalledFlag = false;
			s.Font.Bold = true;
			Assert.IsTrue (s.SetBitCalledFlag, "SetBit() was not called : Font.Bold");
			Assert.AreEqual (0x800, s.SetBitCalledValue, "SetBit() was called with wrong argument : Font.Bold");

			s.SetBitCalledFlag = false;
			s.Font.Italic = true;
			Assert.IsTrue (s.SetBitCalledFlag, "SetBit() was not called : Font.Italic");
			Assert.AreEqual (0x1000, s.SetBitCalledValue, "SetBit() was called with wrong argument : Font.Italic");

			s.SetBitCalledFlag = false;
			s.Font.Overline = true;
			Assert.IsTrue (s.SetBitCalledFlag, "SetBit() was not called : Font.Overline");
			Assert.AreEqual (0x4000, s.SetBitCalledValue, "SetBit() was called with wrong argument : Font.Overline");

			s.SetBitCalledFlag = false;
			s.Font.Underline = true;
			Assert.IsTrue (s.SetBitCalledFlag, "SetBit() was not called : Font.Underline");
			Assert.AreEqual (0x2000, s.SetBitCalledValue, "SetBit() was called with wrong argument : Font.Underline");

			s.SetBitCalledFlag = false;
			s.Font.Strikeout = true;
			Assert.IsTrue (s.SetBitCalledFlag, "SetBit() was not called : Font.Strikeout");
			Assert.AreEqual (0x8000, s.SetBitCalledValue, "SetBit() was called with wrong argument : Font.Strikeout");

			s.SetBitCalledFlag = false;
			s.Font.Names = new string [] { "Arial" };
			Assert.IsTrue (s.SetBitCalledFlag, "SetBit() was not called : Font.Names");
			Assert.AreEqual (0x200, s.SetBitCalledValue, "SetBit() was called with wrong argument : Font.Strikeout");

			s.SetBitCalledFlag = false;
			s.Font.Size = new FontUnit (10);
			Assert.IsTrue (s.SetBitCalledFlag, "SetBit() was not called : Font.Size");
			Assert.AreEqual (0x400, s.SetBitCalledValue, "SetBit() was called with wrong argument : Font.Size");
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
#if NET_2_0
		class PokerStyle : Style
		{
			public IUrlResolutionService UrlResolver;

			protected override void FillStyleAttributes (CssStyleCollection attributes, IUrlResolutionService urlResolver)
			{
				UrlResolver = urlResolver;
				base.FillStyleAttributes (attributes, urlResolver);
			}
		}
		
		class PokerWebControl : WebControl
		{
			protected override Style CreateControlStyle ()
			{
				return new PokerStyle ();
			}
		}

		[Test]
		public void FillStyleAttributes_UrlResolver ()
		{
			PokerWebControl c = new PokerWebControl ();
			c.BackColor = Color.AliceBlue;
			c.RenderControl (new HtmlTextWriter (new StringWriter ()));

			Assert.AreEqual (c, ((PokerStyle) c.ControlStyle).UrlResolver);
		}

#endif
	}
}
