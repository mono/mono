//
// Tests for System.Web.UI.WebControls.ValidationSummary
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
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MonoTests.System.Web.UI.WebControls
{
	[TestFixture]	
	public class ValidationSummaryTest : ValidatorTest {
		public class NamingContainer : WebControl, INamingContainer {

		}

		public class ValidationSummaryTestClass : ValidationSummary {

			public ValidationSummaryTestClass ()
				: base () {
			}

			public StateBag StateBag {
				get { return base.ViewState; }
			}

			public string Render () {
				HtmlTextWriter	writer;

				writer = ValidationSummaryTest.GetWriter();
				base.Render (writer);
				return writer.InnerWriter.ToString ();
			}

			public bool IsTrackingVS () {
				return IsTrackingViewState;
			}

			public void SetTrackingVS () {
				TrackViewState ();
			}

			public object Save() {
				return base.SaveViewState();
			}

			public void Load(object o) {
				base.LoadViewState(o);
			}

			public void CallInit() {
				base.OnInit(EventArgs.Empty);
			}
	
			public new void RenderContents(HtmlTextWriter writer) {
				base.RenderContents(writer);
			}

			public new void CreateControlCollection() {
				base.CreateControlCollection();
			}

			public new void AddAttributesToRender(HtmlTextWriter writer) {
				base.AddAttributesToRender(writer);
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
		}

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

		[Test]
		public void ValidationSummary_Defaults () {
			ValidationSummaryTestClass v = new ValidationSummaryTestClass ();

			Assert.AreEqual (ValidationSummaryDisplayMode.BulletList, v.DisplayMode, "D1");
			Assert.AreEqual (true, v.EnableClientScript, "D2");
			Assert.AreEqual (Color.Red, v.ForeColor, "D3");
			Assert.AreEqual (string.Empty, v.HeaderText, "D4");
			Assert.AreEqual (true, v.ShowSummary, "D5");
		}

#if NET_2_0
		[Test]
		public void ValidationSummary_ValidationGroup () {
			ValidationSummaryTestClass v = new ValidationSummaryTestClass ();
			v.SetTrackingVS();
			Assert.AreEqual ("", v.ValidationGroup, "VG1");

			v.ValidationGroup = "group";
			Assert.AreEqual ("group", v.ValidationGroup, "VG2");

			/* make sure ValidationGroup is stored in the view state */
			object state = v.Save ();

			ValidationSummaryTestClass v2 = new ValidationSummaryTestClass ();
			v2.SetTrackingVS();
			v2.Load (state);

			Assert.AreEqual ("group", v2.ValidationGroup, "VG3");
		}
#endif

		[Test]
		public void ValidationSummaryRenderTest () {
			ValidationSummaryTestClass	v;
			RangeValidatorTest.RangeValidatorTestClass		p;
			RangeValidatorTest.RangeValidatorTestClass		p2;
			TextBox				t1;
			TextBox				t2;

			v = new ValidationSummaryTestClass ();
			p = new RangeValidatorTest.RangeValidatorTestClass();

			v.HeaderText = "I am the header text";

			StartValidationTest(p);
			p.SetTrackingVS();
			p.Type = ValidationDataType.Integer;
			p.MinimumValue = "2";
			p.MaximumValue = "4";
			p.ErrorMessage = "aw shucks";
			p.Enabled = true;
			p.EnableViewState = true;
			p.CallInit();
			p.ID = "moep";

			t1 = SetValidationTextBox("textbox", "1");
			Assert.AreEqual(false, p.DoEvaluateIsValid(), "R1");

			p2 = new RangeValidatorTest.RangeValidatorTestClass();
			Page.Controls.Add(p2);
			p2.SetTrackingVS();
			p2.Type = ValidationDataType.Integer;
			p2.MinimumValue = "6";
			p2.MaximumValue = "7";
			p2.ErrorMessage = "WhamBamThankYouMam";
			p2.Enabled = true;
			p2.EnableViewState = true;
			p2.CallInit();
			p2.ID = "moep2";

			t2 = this.AddTextBox("textbox2", "2");
			p2.ControlToValidate = "textbox2";

			p.Validate();
			p2.Validate();

			Page.Controls.Add(v);

			// Default DisplayMode
			Assert.AreEqual("<div style=\"color:Red;\">\n\tI am the header text<ul><li>aw shucks</li><li>WhamBamThankYouMam</li></ul>\n</div>", v.Render(), "R2");

			v.DisplayMode = ValidationSummaryDisplayMode.BulletList;
			Assert.AreEqual("<div style=\"color:Red;\">\n\tI am the header text<ul><li>aw shucks</li><li>WhamBamThankYouMam</li></ul>\n</div>", v.Render(), "R3");

			v.DisplayMode = ValidationSummaryDisplayMode.List;
#if NET_2_0
			Assert.AreEqual("<div style=\"color:Red;\">\n\tI am the header text<br />aw shucks<br />WhamBamThankYouMam<br />\n</div>", v.Render(), "R4");
#else
			Assert.AreEqual("<div style=\"color:Red;\">\n\tI am the header text<br>aw shucks<br>WhamBamThankYouMam<br>\n</div>", v.Render(), "R4");
#endif

			v.DisplayMode = ValidationSummaryDisplayMode.SingleParagraph;
#if NET_2_0
			Assert.AreEqual("<div style=\"color:Red;\">\n\tI am the header text aw shucks WhamBamThankYouMam <br />\n</div>", v.Render(), "R5");
#else
			Assert.AreEqual("<div style=\"color:Red;\">\n\tI am the header text aw shucks WhamBamThankYouMam <br>\n</div>", v.Render(), "R5");
#endif

			v.ShowSummary = false;
			v.DisplayMode = ValidationSummaryDisplayMode.BulletList;
			Assert.AreEqual("", v.Render(), "R6");

			v.ShowSummary = true;
			v.EnableClientScript = true;
			v.ShowMessageBox = true;
			v.DisplayMode = ValidationSummaryDisplayMode.SingleParagraph;
#if NET_2_0
			Assert.AreEqual("<div style=\"color:Red;\">\n\tI am the header text aw shucks WhamBamThankYouMam <br />\n</div>", v.Render(), "R7");
#else
			Assert.AreEqual("<div style=\"color:Red;\">\n\tI am the header text aw shucks WhamBamThankYouMam <br>\n</div>", v.Render(), "R7");
#endif

			StopValidationTest();
		}
#if NET_4_0
		[Test]
		public void SupportsDisabledAttribute ()
		{
			var ver40 = new Version (4, 0);
			var ver35 = new Version (3, 5);
			var p = new ValidationSummaryTestClass ();
			Assert.AreEqual (ver40, p.RenderingCompatibility, "#A1-1");
			Assert.IsFalse (p.SupportsDisabledAttribute, "#A1-2");

			p.RenderingCompatibility = new Version (3, 5);
			Assert.AreEqual (ver35, p.RenderingCompatibility, "#A2-1");
			Assert.IsTrue (p.SupportsDisabledAttribute, "#A2-2");
		}
#endif
	}
}
