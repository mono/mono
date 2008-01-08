//
// Tests for System.Web.UI.WebControls.DropDownList.cs 
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


using System;
using System.Collections;
using System.Drawing;
using System.IO;
using System.Data;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using NUnit.Framework;
using MonoTests.stand_alone.WebHarness;
#if NET_2_0
using System.Collections.Generic;
using MonoTests.SystemWeb.Framework;
#endif

namespace MonoTests.System.Web.UI.WebControls
{
	[TestFixture]	
	public class DropDownListTest {
		public class NamingContainer : WebControl, INamingContainer {

		}

		public class DropDownListTestClass : DropDownList {

			public DropDownListTestClass ()
				: base () {
			}

			public StateBag StateBag {
				get { return base.ViewState; }
			}

			public string Render () {
				HtmlTextWriter	writer;

				writer = DropDownListTest.GetWriter();
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
				IEnumerator     e;
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


#if NET_2_0
		public class DS : ObjectDataSource
		{
			public static List<string> GetList ()
			{
				List<string> list = new List<string> ();
				list.Add ("Norway");
				list.Add ("Sweden");
				list.Add ("France");
				list.Add ("Italy");
				list.Add ("Israel");
				list.Add ("Russia");
				return list;
			}

			public void DoRaiseDataSourceChangedEvent (EventArgs e)
			{
				RaiseDataSourceChangedEvent (e);
			}
		}

		[Test]
		[Category ("NunitWeb")]
		public void DropDownList_DataSourceChangedEvent ()
		{
			WebTest t = new WebTest ();
			PageDelegates pd = new PageDelegates ();
			pd.Load = DropDownList_Init;
			pd.PreRenderComplete = DropDownList_Load;
			t.Invoker = new PageInvoker (pd);
			t.Run ();
			FormRequest fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");
			fr.Controls["__EVENTTARGET"].Value = "";
			fr.Controls["__EVENTARGUMENT"].Value = "";
			t.Request = fr;
			t.Run ();
			if (t.UserData == null)
				Assert.Fail ("DataSourceChangedEvent#1");
			Assert.AreEqual ("Data_rebounded", t.UserData.ToString (), "DataSourceChangedEvent#2");
		}

		#region DropDownList_DataSourceChangedEvent
		public static void DropDownList_Init (Page p)
		{
			DropDownListTestClass dl = new DropDownListTestClass ();
			DS data = new DS ();
			p.Controls.Add (dl);
			p.Controls.Add (data);
			data.TypeName = typeof (DS).AssemblyQualifiedName;
			data.SelectMethod = "GetList";
			data.ID = "Data";
			dl.DataBinding += new EventHandler (data_DataBinding);
			dl.DataSourceID = "Data";
		}

		public static void DropDownList_Load (Page p)
		{
			if (p.IsPostBack) {
				DS data = (DS) p.FindControl ("Data");
				if (data == null)
					Assert.Fail ("Data soource control not created#1");
				data.DoRaiseDataSourceChangedEvent (new EventArgs ());
			}
		}

		public static void data_DataBinding (object sender, EventArgs e)
		{
			if (((WebControl) sender).Page.IsPostBack)
				WebTest.CurrentTest.UserData = "Data_rebounded";
		}
		#endregion
#endif
		[Test]
		public void DropDownList_Defaults ()
		{
			DropDownListTestClass d = new DropDownListTestClass ();

			Assert.AreEqual (Color.Empty, d.BackColor, "D1");
			Assert.AreEqual (Color.Empty, d.BorderColor, "D2");
			Assert.AreEqual (BorderStyle.NotSet, d.BorderStyle, "D3");
			Assert.AreEqual (Unit.Empty, d.BorderWidth, "D4");
			Assert.AreEqual (-1, d.SelectedIndex, "D5");
			Assert.AreEqual (string.Empty, d.ToolTip, "D6");
			Assert.AreEqual (0, d.Items.Count, "D7");
		}

		[Test]
		public void DropDownListBasic () {
			DropDownListTestClass d = new DropDownListTestClass ();
#if NET_2_0
			Assert.AreEqual ("<select>\n\n</select>", d.Render (), "B1");
#else
			Assert.AreEqual("<select name>\n\n</select>", d.Render(), "B1");
#endif
			d.ID = "blah";
			Assert.AreEqual("<select name=\"blah\" id=\"blah\">\n\n</select>", d.Render(), "B2");

			Assert.AreEqual(false, d.IsTrackingVS(), "B3");
			d.SetTrackingVS();
			Assert.AreEqual(true, d.IsTrackingVS(), "B4");

			d.Items.Add(new ListItem("text1", "value1"));
			Assert.AreEqual(1, d.Items.Count, "B5");
			d.Items.Add(new ListItem("text2", "value2"));
			Assert.AreEqual(2, d.Items.Count, "B6");
			d.SelectedIndex = 1;

			Assert.AreEqual("<select name=\"blah\" id=\"blah\">\n\t<option value=\"value1\">text1</option>\n\t<option selected=\"selected\" value=\"value2\">text2</option>\n\n</select>", d.Render(), "B7");
		}

		[Test]
		public void DropDownListProperties () {
			DropDownListTestClass	d;

			d = new DropDownListTestClass ();

			Assert.AreEqual(Color.Empty, d.BorderColor, "P1");
			d.BorderColor = Color.Red;
			Assert.AreEqual(Color.Red, d.BorderColor, "P2");

			Assert.AreEqual(BorderStyle.NotSet, d.BorderStyle, "P3");
			d.BorderStyle = BorderStyle.Dotted;
			Assert.AreEqual(BorderStyle.Dotted, d.BorderStyle, "P4");

			Assert.AreEqual(Unit.Empty, d.BorderWidth, "P5");
			d.BorderWidth = new Unit(1);
			Assert.AreEqual("1px", d.BorderWidth.ToString(), "P6");

			Assert.AreEqual(-1, d.SelectedIndex, "P7");
			d.Items.Add(new ListItem("text1", "value1"));
			d.Items.Add(new ListItem("text2", "value2"));
			d.SelectedIndex = 1;
			Assert.AreEqual(1, d.SelectedIndex, "P8");

			Assert.AreEqual(string.Empty, d.ToolTip, "P9");
			d.ToolTip = "blah";
#if NET_2_0
			Assert.AreEqual ("blah", d.ToolTip, "P10");
#else
			Assert.AreEqual(string.Empty, d.ToolTip, "P10");
#endif
		}

		[Test]
		[ExpectedException(typeof(HttpException))]
		public void DropDownListDoubleSelectCheck () {
			DropDownListTestClass	d;

			d = new DropDownListTestClass ();
			d.Items.Add(new ListItem("text1", "value1"));
			d.Items.Add(new ListItem("text2", "value2"));
			d.SelectedIndex = 1;
			Assert.AreEqual(1, d.SelectedIndex, "DS1");
			d.Items[0].Selected = true;
			d.Items[1].Selected = true;
			Assert.AreEqual("<select name>\n\t<option selected=\"selected\" value=\"value1\">text1</option>\n\t<option selected=\"selected\" value=\"value2\">text2</option>\n\n</select>", d.Render(), "DS1");
		}

		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void DropDownListBorderStyleCheck () {
			DropDownListTestClass	d;

			d = new DropDownListTestClass ();
			d.BorderStyle = (BorderStyle)Int32.MinValue;
		}

		[Test]
		public void DropDownListSelectedCheck () {
			DropDownListTestClass	d;

			d = new DropDownListTestClass ();
			d.SelectedIndex = 2;	// No exception thrown!!!
			Assert.AreEqual(-1, d.SelectedIndex, "S1");
		}

		[Test]
#if ONLY_1_1
		[ExpectedException(typeof(NullReferenceException))]
#endif
		public void DropDownNullWriterTest () {
			DropDownListTestClass	d;

			d = new DropDownListTestClass ();
			d.AddAttributesToRender(null);
		}

		[Test]
		public void DropDownListNull () {
			DropDownListTestClass	d;

			d = new DropDownListTestClass ();

			// We want to surve the next two calls
			d.RenderContents(null);
		}

#if not
		[Test]
		public void HtmlWriter () {
			HtmlTextWriter	writer;

			writer = DropDownListTest.GetWriter();
			writer.RenderBeginTag(HtmlTextWriterTag.Select);
			writer.AddAttribute(HtmlTextWriterAttribute.Value, "MeValue", true);
			writer.RenderBeginTag(HtmlTextWriterTag.Option);
			writer.Write("MeText");
			writer.RenderEndTag();
			writer.RenderEndTag();
			Assert.AreEqual("<select>\n\t<option value=\"MeValue\">\n\t\tMeText\n\t</option>\n</select>", writer.InnerWriter.ToString(), "H1");
		}
#endif

		[Test]
		public void DropDownNamingTest () {
			NamingContainer container = new NamingContainer ();
			DropDownListTestClass child = new DropDownListTestClass ();
#if NET_2_0
			Assert.AreEqual ("<select>\n\n</select>", child.Render (), "N1");
#else
			Assert.AreEqual ("<select name>\n\n</select>", child.Render (), "N1");
#endif
			container.Controls.Add (child);
			// don't assume the generated id
			string s = child.Render ();
			Assert.IsTrue (s.StartsWith ("<select name=\""), "N2a");
			Assert.IsTrue (s.EndsWith ("\">\n\n</select>"),  "N2b");

			container.ID = "naming";
			s = child.Render ();
			Assert.IsTrue (s.StartsWith ("<select name=\"naming"), "N3a");
			Assert.IsTrue (s.EndsWith ("\">\n\n</select>"), "N3b");

			child.ID = "fooid";
			s = child.Render ();
			Assert.IsTrue (s.StartsWith ("<select name=\"naming"), "N4a");
			Assert.IsTrue (s.EndsWith ("fooid\" id=\"naming_fooid\">\n\n</select>"), "N4b");
		}

		[Test]
		public void InitialSelectionMade ()
		{
			DropDownList ddl = new DropDownList ();
			ddl.Items.Add ("a");
			ddl.Items.Add ("b");

			Assert.IsNotNull (ddl.SelectedItem, "need a selected item");
			Assert.AreEqual ("a", ddl.SelectedItem.Text);
		}

		DataSet GetExampleData ()
		{
			DataSet ds = new DataSet ();
			ds.ReadXml (new StringReader (@"
<DataSet>
	<Stocks Company='Novell Inc.'     Symbol='NOVL' Price='6.14'   />
	<Stocks Company='Microsoft Corp.' Symbol='MSFT' Price='25.92'  />
	<Stocks Company='Google'          Symbol='GOOG' Price='291.60' />
</DataSet>
"));
			return ds;
		}
		
		
		[Test]
		public void TestValueFieldAndTextFormat ()
		{
			DropDownListTestClass ddl = new DropDownListTestClass ();
			ddl.DataSource = GetExampleData ();
			ddl.DataValueField = "Company";
			ddl.DataTextFormatString = "This shouldn't show up = {0}";
			ddl.DataBind ();
#if NET_2_0
			string exp = @"<select>
	<option value=""Novell Inc."">Novell Inc.</option>
	<option value=""Microsoft Corp."">Microsoft Corp.</option>
	<option value=""Google"">Google</option>

</select>";
#else
			string exp = @"<select name>
	<option value=""Novell Inc."">Novell Inc.</option>
	<option value=""Microsoft Corp."">Microsoft Corp.</option>
	<option value=""Google"">Google</option>

</select>";
#endif
            HtmlDiff.AssertAreEqual(exp, ddl.Render(), "TestValueFieldAndTextFormat");
		}

		[Test]
#if ONLY_1_1
        [Category("NotWorking")]
#endif
		public void HtmlEncodeItem ()
		{
			DropDownListTestClass d = new DropDownListTestClass ();
			d.Items.Add(new ListItem ("text1", "<hola>"));
            string html = d.Render();
			Assert.IsTrue (html.IndexOf ("<hola>") == -1, "#01");
			Assert.IsTrue (html.IndexOf ("&lt;hola>") != -1, "#02");
		}

#if NET_2_0
        class VerifyMultiSelectDropDownList : DropDownList
        {
            public new virtual void VerifyMultiSelect()
            {
                base.VerifyMultiSelect();
            }
        }

        [Test]
        [ExpectedException(typeof(HttpException))]
        public void VerifyMultiSelectTest()
        {
            VerifyMultiSelectDropDownList list = new VerifyMultiSelectDropDownList();
            list.VerifyMultiSelect();
        }
#endif
        
        

	}
}
