//
// Tests for System.Web.UI.WebControls.WebControl.cs 
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
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using MonoTests.stand_alone.WebHarness;
#if NET_2_0
using System.Web.UI.Adapters;
using System.Web.UI.WebControls.Adapters;
#endif

namespace MonoTests.System.Web.UI.WebControls
{
	[TestFixture]	
	public class WebControlTest {
		private static HtmlTextWriter GetWriter () {
			StringWriter sw = new StringWriter ();
			sw.NewLine = "\n";
			return new HtmlTextWriter (sw);
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

		public class CustomControl : WebControl
		{
			public virtual string CustomProperty {
				get {
					return (string) ViewState ["CustomProperty"];
				}
				set {
					ViewState ["CustomProperty"] = value;
				}
			}

			protected override Style CreateControlStyle () {
				return new Style ();
			}

			public void DoTrackViewState () {
				TrackViewState ();
			}

			public object DoSaveViewState () {
				return SaveViewState ();
			}

			public void DoLoadViewState (object state) {
				LoadViewState (state);
			}
		}
		
		public class CustomControl2 : CustomControl
		{
			protected override Style CreateControlStyle () {
				Style style = new Style (ViewState);
				style.BackColor = Color.Blue;
				return style;
			}
		}

		public class NamingContainer : WebControl, INamingContainer {

		}

		public class WebControlTestClass : WebControl {
			public WebControlTestClass() : base() {
			}

			public WebControlTestClass(string tag) : base(tag) {
			}

			public WebControlTestClass(HtmlTextWriterTag tag) : base(tag) {
			}

			public new HtmlTextWriterTag TagKey {
				get {
					return base.TagKey;
				}
			}

			public new string TagName {
				get {
					return base.TagName;
				}
			}

			public StateBag Bag {
				get {
					return base.ViewState;
				}
			}

			public override bool EnableViewState {
				get {
					return base.EnableViewState;
				}
			}

			public string Render () {
				HtmlTextWriter	writer;

				writer = WebControlTest.GetWriter();
				base.Render (writer);
				return writer.InnerWriter.ToString ();
			}

			public bool IsTrackingVS ()
			{
				return IsTrackingViewState;
			}

			public void SetTrackingVS ()
			{
				TrackViewState ();
			}

			public object Save() {
				return base.SaveViewState();
			}

			public void Load(object o) {
				base.LoadViewState(o);
			}

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

			public Style DoCreateControlStyle () {
				return base.CreateControlStyle ();
			}
		}

		[Test]
		public void CreateControlStyle () {
			WebControlTestClass w = new WebControlTestClass ();
			Assert.AreEqual (false, w.ControlStyleCreated, "CreateControlStyle#1");
			Style s = w.DoCreateControlStyle ();
			Assert.AreEqual (false, w.ControlStyleCreated, "CreateControlStyle#2");
			s = w.ControlStyle;
			Assert.AreEqual (true, w.ControlStyleCreated, "CreateControlStyle#3");
		}

		[Test]
		public void Constructors ()
		{
			WebControlTestClass	w;

			w = new WebControlTestClass();
			Assert.AreEqual(Color.Empty, w.BackColor, "C1");
			Assert.AreEqual(HtmlTextWriterTag.Span, w.TagKey, "C2");
			Assert.AreEqual("span", w.TagName, "C3");

			w = new WebControlTestClass("Small");
			Assert.AreEqual(HtmlTextWriterTag.Unknown, w.TagKey, "C4");
			Assert.AreEqual("Small", w.TagName, "C5");

			w = new WebControlTestClass(HtmlTextWriterTag.Small);
			Assert.AreEqual(HtmlTextWriterTag.Small, w.TagKey, "C5");
			Assert.AreEqual("small", w.TagName, "C6");
		}

		[Test]
		public void StyleCreation () {
			WebControlTestClass	w;

			w = new WebControlTestClass(HtmlTextWriterTag.Small);
			Assert.AreEqual(HtmlTextWriterTag.Small, w.TagKey, "C5");
			Assert.AreEqual("small", w.TagName, "C6");

			Assert.AreEqual(false, w.ControlStyleCreated, "C7");	// No style
			Assert.AreEqual(Color.Empty, w.BackColor, "C8");	// Force style creation?
			Assert.AreEqual(false, w.ControlStyleCreated, "C9");	// Nope, 'get' access didn't create it

			w.BackColor = Color.Red;				// Forces style creation!
			Assert.AreEqual(Color.Red, w.BackColor, "C10");
			Assert.AreEqual(true, w.ControlStyleCreated, "C11");	// Now we have a style

			w = new WebControlTestClass(HtmlTextWriterTag.Script);
			Assert.AreEqual(HtmlTextWriterTag.Script, w.TagKey, "C12");
			Assert.AreEqual("script", w.TagName, "C13");
			Assert.AreEqual(false, w.ControlStyleCreated, "C14");	// Double-check
			Assert.IsNotNull(w.ControlStyle, "C15");		// Grab style, forcing creation
			Assert.AreEqual(true, w.ControlStyleCreated, "C16");	// Double-check
		}

		[Test]
		public void Defaults () {
			WebControlTestClass	w;

			w = new WebControlTestClass(HtmlTextWriterTag.Small);

			Assert.AreEqual ("", w.AccessKey, "D1");
			Assert.AreEqual (0, w.Attributes.Count, "D2");
			Assert.AreEqual (Color.Empty, w.BackColor, "D3");
			Assert.AreEqual (Color.Empty, w.BorderColor, "D4");
			Assert.AreEqual (BorderStyle.NotSet, w.BorderStyle, "D5");
			Assert.AreEqual (Unit.Empty, w.BorderWidth, "D6");
			Assert.AreEqual (string.Empty, w.CssClass, "D7");
			Assert.AreEqual (true, w.Enabled, "D8");
			Assert.AreEqual (Color.Empty, w.ForeColor, "D9");
			Assert.AreEqual (Unit.Empty, w.Height, "D10");
			Assert.AreEqual (0, w.Style.Count, "D11");
			Assert.AreEqual (0, w.TabIndex, "D12");
			Assert.AreEqual ("", w.ToolTip, "D13");
			Assert.AreEqual (Unit.Empty, w.Width, "D14");
		}

		[Test]
		public void Assignment () 
		{
			CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
			CultureInfo currentUICulture = Thread.CurrentThread.CurrentUICulture;
			try {
				CultureInfo ciUS = CultureInfo.GetCultureInfo ("en-US");

				Thread.CurrentThread.CurrentCulture = ciUS;
				Thread.CurrentThread.CurrentUICulture = ciUS;
				RunAssignmentTests ();
			} finally {
				Thread.CurrentThread.CurrentCulture = currentCulture;
				Thread.CurrentThread.CurrentUICulture = currentUICulture;
			}
		}

		void RunAssignmentTests ()
		{
			WebControlTestClass	w;

			w = new WebControlTestClass(HtmlTextWriterTag.Small);

			w.BackColor = Color.Red;
			Assert.AreEqual (Color.Red, w.BackColor, "A1");

			w.Attributes["test"] = "testme";
			Assert.AreEqual (1, w.Attributes.Count, "A2");
			Assert.AreEqual ("testme", w.Attributes["test"], "A3");

			w.BorderColor = Color.Green;
			Assert.AreEqual (Color.Green, w.BorderColor, "A4");

			w.BorderStyle = BorderStyle.Dotted;
			Assert.AreEqual (BorderStyle.Dotted, w.BorderStyle, "A5");

			w.BorderWidth = new Unit("12px");
			Assert.AreEqual (12, w.BorderWidth.Value, "A6");

			Assert.AreEqual (string.Empty, w.CssClass, "A7");

			w.Enabled = false;
			Assert.AreEqual (false, w.Enabled, "A8");

			w.ForeColor = Color.BlueViolet;
			Assert.AreEqual (Color.BlueViolet, w.ForeColor, "A9");

			w.Height = new Unit(6.5);
			Assert.AreEqual (6, w.Height.Value, "A10");

			Assert.AreEqual (0, w.Style.Count, "A11");

			w.TabIndex = 10;
			Assert.AreEqual (10, w.TabIndex, "A12");

			w.ToolTip = "I am a tip";
			Assert.AreEqual ("I am a tip", w.ToolTip, "A13");

			w.Width = new Unit(6.5, UnitType.Cm);
			Assert.AreEqual (6.5, w.Width.Value, "A14");

			Assert.AreEqual(false, w.IsTrackingVS (), "A15");
			w.SetTrackingVS ();
			Assert.AreEqual(true, w.IsTrackingVS (), "A16");

			w.Enabled = true;
			Assert.AreEqual(true, w.Enabled, "A17");
			w.Save();

			w.Attributes["PrivateTag"] = "blah";
			Assert.AreEqual(2, w.Attributes.Count, "A18");

			w.Attributes.Clear();
			w.Attributes["Style"] = "background-color: #ff00ff";
			Assert.AreEqual(1, w.Attributes.Count, "A19");
			Assert.AreEqual(1, w.Style.Count, "A20");

			w.Attributes.Clear();
			w.Attributes.Add("Style", "foreground-color=#ff00ff");
			Assert.AreEqual(1, w.Attributes.Count, "A21");
			Assert.AreEqual(0, w.Style.Count, "A22");

			w.Attributes.Clear();
			w.Attributes.Add("Style", "background: black; text-align: left;");
			Assert.AreEqual(1, w.Attributes.Count, "A23");
			Assert.AreEqual(2, w.Style.Count, "A24");

			w.Attributes["Style"] = "background: black; text-align: left; foreground: white;";
			Assert.AreEqual(1, w.Attributes.Count, "A25");
			Assert.AreEqual(3, w.Style.Count, "A26");

			w.Style["background-color"] = Color.Purple.ToString();
			Assert.AreEqual(4, w.Style.Count, "A27");

			w.AccessKey = "I";
			Assert.AreEqual("I", w.AccessKey, "A28");

			// Check the bag
			string[]	expect = {
							  "BorderStyle=Dotted",
							  "Width=6.5cm",
							  "Height=6px",
							  "BorderWidth=12px",
							  "ForeColor=Color [BlueViolet]",
							  "BorderColor=Color [Green]",
							  "ToolTip=I am a tip",
							  "BackColor=Color [Red]",
							  "TabIndex=10",
							  "AccessKey=I",
							  "Enabled=True" };

			IsEqual(expect, w.KeyValuePairs(), "A29");
		}

		[Test]
		public void Methods() {
			WebControlTestClass	w;
			WebControlTestClass	w_copy;

			w = new WebControlTestClass(HtmlTextWriterTag.Xml);
			w_copy = new WebControlTestClass(HtmlTextWriterTag.Xml);

			w.Enabled = true;
			w.AccessKey = "I";
			w.Attributes["Argl"] = "Arglbla";
			w.Attributes.Add("Style", "background: black; text-align: left;");
			Assert.AreEqual(2, w.Attributes.Count, "M1");
			Assert.AreEqual(2, w.Style.Count, "M2");
			w_copy.TabIndex = 10;
			w_copy.Attributes["Blah"] = "blahblah";

			Assert.AreEqual(0, w.TabIndex, "M3");
			Assert.AreEqual(10, w_copy.TabIndex, "M4");
			w_copy.CopyBaseAttributes(w);

			Assert.AreEqual(10, w_copy.TabIndex, "M5");
			Assert.AreEqual("blahblah", w_copy.Attributes["Blah"], "M6");
			Assert.AreEqual("Arglbla", w_copy.Attributes["Argl"], "M7");

			// Styles should make it over, too
			Assert.AreEqual("black", w_copy.Style["background"], "M8");

			// Check the bag
			string[]	expect = {
							 "TabIndex=10",
							 "AccessKey=I" };

			IsEqual(expect, w_copy.KeyValuePairs(), "M9");

			Assert.AreEqual("<xml accesskey=\"I\" Argl=\"Arglbla\" style=\"background: black; text-align: left;\">\n\n</xml>", w.Render(), "M10");
			Assert.AreEqual("<xml accesskey=\"I\" tabindex=\"10\" Blah=\"blahblah\" Argl=\"Arglbla\" style=\"background: black; text-align: left;\">\n\n</xml>", w_copy.Render(), "M11");
		}

		[Test]
		public void CopyEnabled ()
		{
			Label l = new Label (), ll = new Label ();
			l.Enabled = false;
			ll.CopyBaseAttributes (l);
			Assert.IsFalse (ll.Enabled, "enabled should be copied");

			WebControlTestClass c = new WebControlTestClass ();
			c.SetTrackingVS ();
			c.CopyBaseAttributes (l);
			object o = c.Save ();
			c = new WebControlTestClass ();
			c.Load (o);
			Assert.IsFalse (c.Enabled, "enabled should be copied#2");
		}
		

		[Test]
		public void RenderClientId ()
		{
			NamingContainer container = new NamingContainer ();
			WebControlTestClass child = new WebControlTestClass ();

			container.Controls.Add (child);
			container.ID = "naming";
			child.ID = "fooid";

			Assert.AreEqual ("<span id=\"naming_fooid\"></span>", child.Render (), "A1");
		}

		
		[Test]
		public void ViewState() {
			WebControlTestClass	w;
			WebControlTestClass	w_copy;
			object			state;

			w = new WebControlTestClass(HtmlTextWriterTag.Xml);
			w_copy = new WebControlTestClass(HtmlTextWriterTag.Xml);

			w.SetTrackingVS ();
			w.BackColor = Color.Red;
			w.Attributes["test"] = "testme";
			w.BorderColor = Color.Green;
			w.BorderStyle = BorderStyle.Dotted;
			w.BorderWidth = new Unit("12px");
			w.Enabled = false;
			w.ForeColor = Color.BlueViolet;
			w.Height = new Unit(6.5);
			w.TabIndex = 10;
			w.ToolTip = "I am a tip";
			w.Width = new Unit(6.5, UnitType.Cm);
			w.Enabled = true;
			w.Attributes["PrivateTag"] = "blah";
			w.Attributes["Style"] = "background-color: #ff00ff";

			state = w.Save();

			w_copy.Load(state);
			w_copy.SetTrackingVS();
			/* MS: <xml tabindex="10" title="I am a tip" test="testme" PrivateTag="blah" 
				style="color:BlueViolet;background-color:Red;border-color:Green;border-width:12px;border-style:Dotted;height:6px;width:6.5cm;background-color: #ff00ff">
				</xml>
			*/
			HtmlDiff.AssertAreEqual (w.Render(), w_copy.Render(), "VS1");
		}

		[Test]
		public void ViewState2 () {
			CustomControl c = new CustomControl ();
			CustomControl copy = new CustomControl ();
			object state;

			c.DoTrackViewState ();
			c.CustomProperty = "CustomProperty";
			c.ControlStyle.BackColor = Color.Red;
			c.ControlStyle.BorderColor = Color.Green;
			c.ControlStyle.BorderStyle = BorderStyle.Dotted;

			state = c.DoSaveViewState ();

			copy.DoLoadViewState (state);

			Assert.IsFalse (copy.ControlStyleCreated, "copy.ControlStyleCreated");
		}
		
		[Test]
		public void ViewState3 () {
			CustomControl2 c = new CustomControl2 ();
			CustomControl2 copy = new CustomControl2 ();
			object state;

			c.DoTrackViewState ();
			c.ControlStyle.BackColor = Color.Red;

			state = c.DoSaveViewState ();

			copy.DoLoadViewState (state);

			Assert.AreEqual (Color.Blue, copy.ControlStyle.BackColor, "copy.BackColor");

		}

		[Test]
		public void RenderBeginTag_TagOnly ()
		{
			HtmlTextWriter writer = WebControlTest.GetWriter ();
			WebControl wc = new WebControl (HtmlTextWriterTag.Table);
			wc.RenderBeginTag (writer);
			string s = writer.InnerWriter.ToString ();
			Assert.AreEqual ("<table>\n", s, "table");
		}

		[Test]
		public void RenderBeginTag_Attributes ()
		{
			HtmlTextWriter writer = WebControlTest.GetWriter ();
			WebControl wc = new WebControl (HtmlTextWriterTag.Table);
			wc.ID = "test1";
			wc.RenderBeginTag (writer);
			string s = writer.InnerWriter.ToString ();
			Assert.AreEqual ("<table id=\"test1\">\n", s, "ID");

			writer = WebControlTest.GetWriter ();
			wc.ID = null;
			Assert.IsNull (wc.ID, "ID");
			wc.RenderBeginTag (writer);
			s = writer.InnerWriter.ToString ();
			Assert.AreEqual ("<table>\n", s, "-ID");
		}

		[Test]
		public void RenderBeginTag_Style ()
		{
			HtmlTextWriter writer = WebControlTest.GetWriter ();
			WebControl wc = new WebControl (HtmlTextWriterTag.Table);
			wc.BackColor = Color.Aqua;
			wc.RenderBeginTag (writer);
			string s = writer.InnerWriter.ToString ();
			Assert.AreEqual ("<table style=\"background-color:Aqua;\">\n", s, "BackColor");

			writer = WebControlTest.GetWriter ();
			wc.BackColor = new Color ();
			Assert.IsTrue (wc.BackColor.IsEmpty, "IsEmpty");
			wc.RenderBeginTag (writer);
			s = writer.InnerWriter.ToString ();
			Assert.AreEqual ("<table>\n", s, "-BackColor");
		}

		[Test]
		public void RenderBeginTag_BorderWidth_span ()
		{
			HtmlTextWriter writer = WebControlTest.GetWriter ();
			WebControl wc = new WebControl (HtmlTextWriterTag.Span);
			wc.BorderWidth = Unit.Pixel (1);
			wc.RenderBeginTag (writer);
			string s = writer.InnerWriter.ToString ();
#if NET_2_0
			Assert.AreEqual ("<span style=\"display:inline-block;border-width:1px;border-style:solid;\">", s, "BorderWidth");
#else
			Assert.AreEqual ("<span style=\"border-width:1px;border-style:solid;\">", s, "BorderWidth");
#endif
		}

		[Test]
		public void RenderBeginTag_BorderWidth_table ()
		{
			HtmlTextWriter writer = WebControlTest.GetWriter ();
			WebControl wc = new WebControl (HtmlTextWriterTag.Table);
			wc.BorderWidth = Unit.Pixel (1);
			wc.RenderBeginTag (writer);
			string s = writer.InnerWriter.ToString ();
			Assert.AreEqual ("<table style=\"border-width:1px;border-style:solid;\">\n", s, "BorderWidth");
		}

		[Test]
		public void EmptyStringTag ()
		{
			WebControlTestClass wc = new WebControlTestClass (String.Empty);
			Assert.AreEqual ("<>\n\n</>", wc.Render ());
		}

		[Test]
		public void NullStringTag ()
		{
			WebControlTestClass wc = new WebControlTestClass (null);
			Assert.AreEqual ("<>\n\n</>", wc.Render ());
		}

		[Test]
		public void UnknownTag ()
		{
			WebControlTestClass wc = new WebControlTestClass (HtmlTextWriterTag.Unknown);
			Assert.AreEqual ("<>\n\n</>", wc.Render ());
		}

		[Test]
		public void EnabledViewState ()
		{
			WebControlTestClass c = new WebControlTestClass ();
			c.SetTrackingVS ();
			c.Enabled = false;
			object o = c.Save ();
			c = new WebControlTestClass ();
			c.Load (o);
			Assert.IsFalse (c.Enabled, "not enabled");
		}

		[Test]
		public void EnabledViewState2 () {
			WebControlTestClass c = new WebControlTestClass ();
			c.Enabled = false;
			c.SetTrackingVS ();
			c.Width = 100; // to cause saveviewstate return not null
			object o = c.Save ();
			c = new WebControlTestClass ();
			c.Enabled = false;
			c.Load (o);
			Assert.IsFalse (c.Enabled, "not enabled");
		}

		[Test]
		public void AttributeIsCaseInsensitive ()
		{
			WebControlTestClass c = new WebControlTestClass ();
			c.Attributes ["hola"] = "hello";
			c.Attributes ["HOla"] = "hi";
			Assert.AreEqual ("hi", c.Attributes ["hoLA"], "#01");
		}

#if NET_2_0
		class MyControlAdapter : ControlAdapter
		{
			protected override void Render (HtmlTextWriter w)
			{
				w.WriteLine("MyControlAdapter.Render");
			}

		}
		
		class MyWebControlAdapter : WebControlAdapter
		{
			protected override void RenderBeginTag (HtmlTextWriter w)
			{
				w.WriteLine("RenderBeginTag");
			}

			protected override void RenderContents (HtmlTextWriter w)
			{
				w.WriteLine("RenderContents");
			}

			protected override void RenderEndTag (HtmlTextWriter w)
			{
				w.WriteLine("RenderEndTag");
			}
		}
		
		class MyWebControl : WebControl
		{
			public ControlAdapter my_control_adapter = new MyWebControlAdapter();
			protected override global::System.Web.UI.Adapters.ControlAdapter ResolveAdapter ()
			{
				return my_control_adapter;
			}
	    public void DoRender(HtmlTextWriter writer)
	    {
		Render(writer);
	    }
	    public bool GetIsEnabled
	    {
		get
		{
		    return IsEnabled;
		}
	    }
		}
		
		[Test]
		[Category ("NotDotNet")] // .NET doesn't use ResolveAdapter
		public void RenderWithWebControlAdapter ()
		{
			MyWebControl c = new MyWebControl ();
			StringWriter sw = new StringWriter ();
			HtmlTextWriter w = new HtmlTextWriter (sw);			
			c.DoRender (w);
			Assert.AreEqual ("RenderBeginTag\nRenderContents\nRenderEndTag\n", sw.ToString ().Replace ("\r\n", "\n"), "RenderWithWebControlAdapter #1");
		}

		[Test]
		[Category ("NotDotNet")] // .NET doesn't use ResolveAdapter
		public void RenderWithControlAdapter ()
		{
			MyWebControl c = new MyWebControl ();
			c.my_control_adapter = new MyControlAdapter ();
			StringWriter sw = new StringWriter ();
			HtmlTextWriter w = new HtmlTextWriter (sw);			
			c.DoRender (w);
			Assert.AreEqual ("MyControlAdapter.Render\n", sw.ToString ().Replace ("\r\n", "\n"), "RenderWithControlAdapter #1");
		}

		[Test]
		public void IsEnabled ()
		{
			MyWebControl parent = new MyWebControl ();
			MyWebControl child = new MyWebControl ();
			parent.Controls.Add (child);
			Assert.IsTrue (child.GetIsEnabled, "IsEnabled #1");
			parent.Enabled = false;
			Assert.IsFalse (child.GetIsEnabled, "IsEnabled #2");
			parent.Enabled = true;
			child.Enabled = false;
			Assert.IsFalse (child.GetIsEnabled, "IsEnabled #3");
		}
#endif
	}
}
