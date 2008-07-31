//
// HtmlSelectTest.cs
//	- Unit tests for System.Web.UI.HtmlControls.HtmlSelect
//
// Author:
//	Dick Porter  <dick@ximian.com>
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
using System.IO;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Data;
using NUnit.Framework;
using MonoTests.stand_alone.WebHarness;

namespace MonoTests.System.Web.UI.HtmlControls {

	public class TestHtmlSelect : HtmlSelect {
		public TestHtmlSelect () 
		{
		}
		
		public StateBag GetViewState ()
		{
			return (ViewState);
		}
		
		public HtmlTextWriter GetWriter ()
		{
			StringWriter text = new StringWriter ();
			HtmlTextWriter writer = new HtmlTextWriter (text);
			base.RenderAttributes (writer);
			return (writer);
		}

		public int[] GetIndices ()
		{
			return (SelectedIndices);
		}

		public void Clear ()
		{
			ClearSelection ();
		}
		
		public void SetIndices (int[] indices)
		{
			Select (indices);
		}
		public string Render ()
		{
			StringWriter sw = new StringWriter ();
			sw.NewLine = "\n";
			HtmlTextWriter writer = new HtmlTextWriter (sw);
			base.Render (writer);
			return writer.InnerWriter.ToString ();
		}	
	}

	[TestFixture]
	public class HtmlSelectTest {

		[Test]
		public void DefaultProperties ()
		{
			HtmlSelect sel = new HtmlSelect ();

			Assert.AreEqual (0, sel.Attributes.Count, "Attributes.Count");

			Assert.AreEqual (String.Empty, sel.DataMember, "DataMember");
			Assert.AreEqual (null, sel.DataSource, "DataSource");
			Assert.AreEqual (String.Empty, sel.DataTextField, "DataTextField");
			Assert.AreEqual (String.Empty, sel.DataValueField, "DataValueField");
			Assert.IsTrue (sel.Items.GetType() == typeof (ListItemCollection), "Items");
			Assert.AreEqual (false, sel.Multiple, "Multiple");
			Assert.AreEqual (0, sel.SelectedIndex, "SelectedIndex");
			Assert.AreEqual (-1, sel.Size, "Size");
			Assert.AreEqual (String.Empty, sel.Value, "Value");

			Assert.AreEqual ("select", sel.TagName, "TagName");
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void InnerHtmlGet ()
		{
			HtmlSelect sel = new HtmlSelect ();
			Assert.IsNotNull (sel.InnerHtml);
		}
		

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void InnerTextGet ()
		{
			HtmlSelect sel = new HtmlSelect ();
			sel.InnerText = null;
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void InnerHtmlSet ()
		{
			HtmlSelect sel = new HtmlSelect ();
			sel.InnerHtml = null;
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void InnerTextSet ()
		{
			HtmlSelect sel = new HtmlSelect ();
			Assert.IsNotNull (sel.InnerText);
		}
		

		[Test]
		public void NullProperties ()
		{
			TestHtmlSelect sel = new TestHtmlSelect ();

			sel.DataMember = null;
			Assert.AreEqual (String.Empty, sel.DataMember, "DataMember");
			sel.DataSource = null;
			Assert.AreEqual (null, sel.DataSource, "DataSource");
			sel.DataTextField = null;
			Assert.AreEqual (String.Empty, sel.DataTextField, "DataTextField");
			sel.DataValueField = null;
			Assert.AreEqual (String.Empty, sel.DataValueField, "DataValueField");
			sel.Multiple = false;
			Assert.AreEqual (false, sel.Multiple, "Multiple");
			sel.SelectedIndex = -1;
			Assert.AreEqual (0, sel.SelectedIndex, "SelectedIndex");
			sel.Size = -1;
			Assert.AreEqual (-1, sel.Size, "Size");
			sel.Value = null;
			Assert.AreEqual (String.Empty, sel.Value, "Value");

			Assert.AreEqual (0, sel.Attributes.Count, "Attributes.Count");

			StateBag sb = sel.GetViewState ();
			Assert.AreEqual (0, sb.Count, "ViewState Count");
		}

		[Test]
		public void SourceType ()
		{
			HtmlSelect sel = new HtmlSelect ();
			int[] source = new int[] {1,2,3,4};

			sel.DataSource = source;
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void BadSourceType ()
		{
			HtmlSelect sel = new HtmlSelect ();
			object source = new object ();

			sel.DataSource = source;
		}
		
		[Test]
		public void ViewStateCount ()
		{
			TestHtmlSelect sel = new TestHtmlSelect ();
			int[] source = new int[] {1,2,3,4,5,6,7,8};
			
			sel.DataMember = "*1*";
			Assert.AreEqual ("*1*", sel.DataMember, "DataMember");
			sel.DataSource = source;
			Assert.AreEqual (source, sel.DataSource, "DataSource");
			sel.DataTextField = "*3*";
			Assert.AreEqual ("*3*", sel.DataTextField, "DataTextField");
			sel.DataValueField = "*4*";
			Assert.AreEqual ("*4*", sel.DataValueField, "DataValueField");
			sel.Multiple = true;
			Assert.AreEqual (true, sel.Multiple, "Multiple");
			sel.SelectedIndex = 6;
			/* This doesn't stick */
			Assert.AreEqual (-1, sel.SelectedIndex, "SelectedIndex");
			sel.Size = 7;
			Assert.AreEqual (7, sel.Size, "Size");
			sel.Value = "*8*";
			/* Neither does this */
			Assert.AreEqual (String.Empty, sel.Value, "Value");

			Assert.AreEqual (5, sel.Attributes.Count, "Attributes.Count");
			/*
			foreach (string key in sel.Attributes.Keys) {
				Console.WriteLine ("attr key {0} is {1}", key, sel.Attributes[key]);
			}
			*/
			
			StateBag sb = sel.GetViewState ();
			Assert.AreEqual (5, sb.Count, "ViewState Count");

			/*
			foreach (string key in sb.Keys) {
				Console.WriteLine ("vs key {0} is {1}", key, sb[key]);
			}
			*/
		}

		[Test]
		public void InternalDetails ()
		{
			/* Test the undocumented but visible
			 * properties and methods SelectedIndices,
			 * ClearSelection () and Select ()
			 */
			TestHtmlSelect sel = new TestHtmlSelect ();
			ListItemCollection items;

			items = sel.Items;

			ListItem item1 = new ListItem ("text1", "value1");
			items.Add (item1);

			ListItem item2 = new ListItem ("text2", "value2");
			item2.Selected = true;
			items.Add (item2);

			ListItem item3 = new ListItem ("text3", "value3");
			items.Add (item3);

			ListItem item4 = new ListItem ("text4", "value4");
			item4.Selected = true;
			items.Add (item4);

			ListItem item5 = new ListItem ("text5", "value5");
			item5.Selected = true;
			items.Add (item5);

			ListItem item6 = new ListItem ("text6", "value6");
			items.Add (item6);

			int[] indices = sel.GetIndices ();

			Assert.AreEqual (1, sel.SelectedIndex, "SelectedIndex");
			Assert.AreEqual (3, indices.Length, "SelectIndices Length");
			Assert.AreEqual (1, indices[0], "SelectIndices 0");
			Assert.AreEqual (3, indices[1], "SelectIndices 1");
			Assert.AreEqual (4, indices[2], "SelectIndices 2");
			Assert.IsFalse (item1.Selected, "Item1");
			Assert.IsTrue (item2.Selected, "Item2");
			Assert.IsFalse (item3.Selected, "Item3");
			Assert.IsTrue (item4.Selected, "Item4");
			Assert.IsTrue (item5.Selected, "Item5");
			Assert.IsFalse (item6.Selected, "Item6");
			
			sel.Clear ();
			Assert.IsFalse (item1.Selected, "Item1 after clear but before SelectedIndices and SelectedIndex");

			indices = sel.GetIndices ();
			Assert.IsFalse (item1.Selected, "Item1 after clear but between SelectedIndices and SelectedIndex");
			Assert.AreEqual (0, sel.SelectedIndex, "SelectedIndex after clear");
			Assert.AreEqual (0, indices.Length, "SelectIndices Length after clear");

			/* NB: !multiple and size <= 1, therefore
			 * there must be one selected after
			 * SelectedIndex is called
			 */
			Assert.IsTrue (item1.Selected, "Item1 after clear");
			Assert.IsFalse (item2.Selected, "Item2 after clear");
			Assert.IsFalse (item3.Selected, "Item3 after clear");
			Assert.IsFalse (item4.Selected, "Item4 after clear");
			Assert.IsFalse (item5.Selected, "Item5 after clear");
			Assert.IsFalse (item6.Selected, "Item6 after clear");

			int[] new_indices = new int[]{2, 4, 5};

			sel.SetIndices (new_indices);

			indices = sel.GetIndices ();

			Assert.AreEqual (2, sel.SelectedIndex, "SelectedIndex after set");
			Assert.AreEqual (3, indices.Length, "SelectIndices Length after set");
			Assert.AreEqual (2, indices[0], "SelectIndices 0 after set");
			Assert.AreEqual (4, indices[1], "SelectIndices 1 after set");
			Assert.AreEqual (5, indices[2], "SelectIndices 2 after set");
			Assert.IsFalse (item1.Selected, "Item1 after set");
			Assert.IsFalse (item2.Selected, "Item2 after set");
			Assert.IsTrue (item3.Selected, "Item3 after set");
			Assert.IsFalse (item4.Selected, "Item4 after set");
			Assert.IsTrue (item5.Selected, "Item5 after set");
			Assert.IsTrue (item6.Selected, "Item6 after set");

			new_indices = new int[]{2};

			sel.SetIndices (new_indices);

			indices = sel.GetIndices ();

			Assert.AreEqual (2, sel.SelectedIndex, "SelectedIndex after short set");
			Assert.AreEqual (1, indices.Length, "SelectIndices Length after short set");
			Assert.AreEqual (2, indices[0], "SelectIndices 0 after short set");
			Assert.IsFalse (item1.Selected, "Item1 after short set");
			Assert.IsFalse (item2.Selected, "Item2 after short set");
			Assert.IsTrue (item3.Selected, "Item3 after short set");
			Assert.IsFalse (item4.Selected, "Item4 after short set");
			Assert.IsFalse (item5.Selected, "Item5 after short set");
			Assert.IsFalse (item6.Selected, "Item6 after short set");

			new_indices = new int[]{-2, -1, 2, 4, 5, 6, 7, 8, 9, 10};

			sel.SetIndices (new_indices);

			indices = sel.GetIndices ();

			Assert.AreEqual (2, sel.SelectedIndex, "SelectedIndex after long set");
			Assert.AreEqual (3, indices.Length, "SelectIndices Length after long set");
			Assert.AreEqual (2, indices[0], "SelectIndices 0 after long set");
			Assert.AreEqual (4, indices[1], "SelectIndices 1 after long set");
			Assert.AreEqual (5, indices[2], "SelectIndices 2 after long set");
			Assert.IsFalse (item1.Selected, "Item1 after long set");
			Assert.IsFalse (item2.Selected, "Item2 after long set");
			Assert.IsTrue (item3.Selected, "Item3 after long set");
			Assert.IsFalse (item4.Selected, "Item4 after long set");
			Assert.IsTrue (item5.Selected, "Item5 after long set");
			Assert.IsTrue (item6.Selected, "Item6 after long set");
		}
		
		[Test]
		public void Multiple ()
		{
			HtmlSelect sel = new HtmlSelect ();

			sel.Multiple = true;
			Assert.AreEqual (-1, sel.SelectedIndex, "SelectedIndex");
		}
		
		[Test]
		public void Big ()
		{
			HtmlSelect sel = new HtmlSelect ();

			sel.Size = 5;
			Assert.AreEqual (-1, sel.SelectedIndex, "SelectedIndex");
		}
		
		[Test]
		public void OneRowIndividual ()
		{
			HtmlSelect sel = new HtmlSelect ();

			Assert.AreEqual (0, sel.SelectedIndex, "SelectedIndex");
		}
		
		[Test]
		public void RenderAttributes ()
		{
			TestHtmlSelect sel = new TestHtmlSelect ();
			object source = new object ();
			
			sel.DataMember = "*1*";
			//sel.DataSource = source;
			sel.DataTextField = "*3*";
			sel.DataValueField = "*4*";
			sel.Multiple = true;
			sel.Name = "*6*";
			sel.SelectedIndex = 7;
			sel.Size = 8;
			sel.Value = "*9*";
			
			Assert.AreEqual (5, sel.Attributes.Count, "Attributes.Count");

			HtmlTextWriter writer = sel.GetWriter ();
			Assert.AreEqual (" name multiple=\"multiple\" size=\"8\"", writer.InnerWriter.ToString ());
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
		public void DataBind1 ()
		{
			TestHtmlSelect s = new TestHtmlSelect ();
			s.DataSource = GetExampleData ();
			s.DataTextField = "Symbol";
			s.DataBind ();
			string exp = @"<select name>
	<option value=""NOVL"">NOVL</option>
	<option value=""MSFT"">MSFT</option>
	<option value=""GOOG"">GOOG</option>
</select>";
			HtmlDiff.AssertAreEqual (exp, s.Render (), "DataBind1");
		}

		DataSet GetExampleDataWithEmpty ()
		{
			DataSet ds = new DataSet ();
			ds.ReadXml (new StringReader (@"
<DataSet>
	<Stocks Company='Novell Inc.'     Symbol='NOVL' Price='6.14'   />
	<Stocks Company=''                Symbol='MSFT' Price='25.92'  />
	<Stocks Company='Google'          Symbol='GOOG' Price='291.60' />
</DataSet>
"));
			return ds;
		}

		[Test]
		public void DataBind2 ()
		{
			TestHtmlSelect s = new TestHtmlSelect ();
			s.DataSource = GetExampleDataWithEmpty ();
			s.DataTextField = "Company";
			s.DataValueField = "Symbol";
			s.DataBind ();
			string exp = @"<select name>
	<option value=""NOVL"">Novell Inc.</option>
	<option value=""MSFT""></option>
	<option value=""GOOG"">Google</option>
</select>";
			HtmlDiff.AssertAreEqual (exp, s.Render (), "DataBind2");
		}

		[Test]
		public void DataBind3 ()
		{
			TestHtmlSelect s = new TestHtmlSelect ();
			s.DataSource = new string [] { "A", "B", "C" };
			s.DataBind ();
			string exp = @"<select name>
	<option value=""A"">A</option>
	<option value=""B"">B</option>
	<option value=""C"">C</option>
</select>";
			HtmlDiff.AssertAreEqual (exp, s.Render (), "DataBind3");
		}

		[Test]
		public void DataBindDoubleCall ()
		{
			TestHtmlSelect s = new TestHtmlSelect ();
			s.DataSource = new string [] { "A", "B", "C" };
			s.DataBind ();
			s.DataBind ();
			string exp = @"<select name>
	<option value=""A"">A</option>
	<option value=""B"">B</option>
	<option value=""C"">C</option>
</select>";
			HtmlDiff.AssertAreEqual (exp, s.Render (), "DataBindDoubleCall");
		}
		
		[Test]
		public void HtmlEncodeValues ()
		{
			TestHtmlSelect s = new TestHtmlSelect ();
			s.DataSource = new string [] { "&", "<" };
			s.DataBind ();
			string exp = @"<select name>
	<option value=""&amp;"">&amp;</option>
	<option value=""&lt;"">&lt;</option>
</select>";
			HtmlDiff.AssertAreEqual (exp, s.Render (), "HtmlEncodeValues");
		}
	}
}
