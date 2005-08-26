//
// DataGridColumnTest.cs
//	- Unit tests for System.Web.UI.WebControls.DataGridColumn
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
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
//using System.Drawing;
using SDColor = System.Drawing.Color;

using NUnit.Framework;

namespace MonoTests.System.Web.UI.WebControls {
	public class TestDataGridColumn : DataGridColumn 
	{
		public StateBag GetViewState ()
		{
			return (ViewState);
		}

		public bool GetDesignMode ()
		{
			return (DesignMode);
		}

		public DataGrid GetOwner ()
		{
			return (Owner);
		}
	}
	
	[TestFixture]
	public class DataGridColumnTest {

		[Test]
		public void DefaultProperties ()
		{
			TestDataGridColumn d = new TestDataGridColumn ();

			TableItemStyle tis = d.FooterStyle;
			
			Assert.IsNotNull (tis, "FooterStyle");
			Assert.AreEqual (tis.GetType (), typeof (TableItemStyle), "typeof FooterStyle");
			Assert.AreEqual (SDColor.Empty, tis.BackColor, "FooterStyle.BackColor");
			Assert.AreEqual (SDColor.Empty, tis.BorderColor, "FooterStyle.BorderColor");
			Assert.AreEqual (BorderStyle.NotSet, tis.BorderStyle, "FooterStyle.BorderStyle");
			Assert.AreEqual (Unit.Empty, tis.BorderWidth, "FooterStyle.BorderWidth");
			Assert.IsNull (tis.Container, "FooterStyle.Container");
			Assert.AreEqual (String.Empty, tis.CssClass, "FooterStyle.CssClass");
			FontInfo fi = tis.Font;

			Assert.IsNotNull (fi, "FooterStyle.FontInfo");
			Assert.IsFalse (fi.Bold, "FooterStyle.FontInfo.Bold");
			Assert.IsFalse (fi.Italic, "FooterStyle.FontInfo.Italic");
			Assert.AreEqual (String.Empty, fi.Name, "FooterStyle.FontInfo.Name");
			Assert.AreEqual (0, fi.Names.Length, "FooterStyle.FontInfo.Names.Length");
			Assert.IsFalse (fi.Overline, "FooterStyle.FontInfo.Overline");
			Assert.IsNotNull (fi.Size, "FooterStyle.FontInfo.Size");
			Assert.IsFalse (fi.Strikeout, "FooterStyle.FontInfo.Strikeout");
			Assert.IsFalse (fi.Underline, "FooterStyle.FontInfo.Underline");
			
			
			Assert.AreEqual (String.Empty, d.FooterText, "FooterText");
			Assert.AreEqual (String.Empty, d.HeaderImageUrl, "HeaderImageUrl");

			tis = d.HeaderStyle;
			
			Assert.IsNotNull (tis, "HeaderStyle");
			Assert.AreEqual (tis.GetType (), typeof (TableItemStyle), "typeof HeaderStyle");
			Assert.AreEqual (SDColor.Empty, tis.BackColor, "HeaderStyle.BackColor");
			Assert.AreEqual (SDColor.Empty, tis.BorderColor, "HeaderStyle.BorderColor");
			Assert.AreEqual (BorderStyle.NotSet, tis.BorderStyle, "HeaderStyle.BorderStyle");
			Assert.AreEqual (Unit.Empty, tis.BorderWidth, "HeaderStyle.BorderWidth");
			Assert.IsNull (tis.Container, "HeaderStyle.Container");
			Assert.AreEqual (String.Empty, tis.CssClass, "HeaderStyle.CssClass");
			fi = tis.Font;

			Assert.IsNotNull (fi, "HeaderStyle.FontInfo");
			Assert.IsFalse (fi.Bold, "HeaderStyle.FontInfo.Bold");
			Assert.IsFalse (fi.Italic, "HeaderStyle.FontInfo.Italic");
			Assert.AreEqual (String.Empty, fi.Name, "HeaderStyle.FontInfo.Name");
			Assert.AreEqual (0, fi.Names.Length, "HeaderStyle.FontInfo.Names.Length");
			Assert.IsFalse (fi.Overline, "HeaderStyle.FontInfo.Overline");
			Assert.IsNotNull (fi.Size, "HeaderStyle.FontInfo.Size");
			Assert.IsFalse (fi.Strikeout, "HeaderStyle.FontInfo.Strikeout");
			Assert.IsFalse (fi.Underline, "HeaderStyle.FontInfo.Underline");

			Assert.AreEqual (String.Empty, d.HeaderText, "HeaderText");

			tis = d.ItemStyle;
			
			Assert.IsNotNull (tis, "ItemStyle");
			Assert.AreEqual (tis.GetType (), typeof (TableItemStyle), "typeof ItemStyle");
			Assert.AreEqual (SDColor.Empty, tis.BackColor, "ItemStyle.BackColor");
			Assert.AreEqual (SDColor.Empty, tis.BorderColor, "ItemStyle.BorderColor");
			Assert.AreEqual (BorderStyle.NotSet, tis.BorderStyle, "ItemStyle.BorderStyle");
			Assert.AreEqual (Unit.Empty, tis.BorderWidth, "ItemStyle.BorderWidth");
			Assert.IsNull (tis.Container, "ItemStyle.Container");
			Assert.AreEqual (String.Empty, tis.CssClass, "ItemStyle.CssClass");
			fi = tis.Font;

			Assert.IsNotNull (fi, "ItemStyle.FontInfo");
			Assert.IsFalse (fi.Bold, "ItemStyle.FontInfo.Bold");
			Assert.IsFalse (fi.Italic, "ItemStyle.FontInfo.Italic");
			Assert.AreEqual (String.Empty, fi.Name, "ItemStyle.FontInfo.Name");
			Assert.AreEqual (0, fi.Names.Length, "ItemStyle.FontInfo.Names.Length");
			Assert.IsFalse (fi.Overline, "ItemStyle.FontInfo.Overline");
			Assert.IsNotNull (fi.Size, "ItemStyle.FontInfo.Size");
			Assert.IsFalse (fi.Strikeout, "ItemStyle.FontInfo.Strikeout");
			Assert.IsFalse (fi.Underline, "ItemStyle.FontInfo.Underline");


			Assert.AreEqual (String.Empty, d.SortExpression, "SortExpression");
			Assert.IsTrue (d.Visible, "Visible");
			Assert.IsFalse (d.GetDesignMode (), "DesignMode");
			Assert.IsNull (d.GetOwner (), "Owner");
			Assert.AreEqual (0, d.GetViewState ().Count, "ViewState");
		}
		
		[Test]
		public void NullProperties ()
		{
			TestDataGridColumn d = new TestDataGridColumn ();
			
			d.FooterText = null;
			Assert.AreEqual (String.Empty, d.FooterText, "FooterText");
			d.HeaderImageUrl = null;
			Assert.AreEqual (String.Empty, d.HeaderImageUrl, "HeaderImageUrl");
			d.HeaderText = null;
			Assert.AreEqual (String.Empty, d.HeaderText, "HeaderText");
			d.SortExpression = null;
			Assert.AreEqual (String.Empty, d.SortExpression, "SortExpression");
			d.Visible = false;
			Assert.IsFalse (d.Visible, "Visible");
			
			/* Visible remains in the ViewState */
			Assert.AreEqual (1, d.GetViewState ().Count, "ViewState.Count");
		}

		[Test]
		public void CleanProperties ()
		{
			TestDataGridColumn d = new TestDataGridColumn ();

			d.FooterText = "*footer-text*";
			Assert.AreEqual ("*footer-text*", d.FooterText, "FooterText set");
			d.HeaderImageUrl = "*header-image-url*";
			Assert.AreEqual ("*header-image-url*", d.HeaderImageUrl, "HeaderImageUrl set");
			d.HeaderText = "*header-text*";
			Assert.AreEqual ("*header-text*", d.HeaderText, "HeaderText set");
			d.SortExpression = "*sort-expression*";
			Assert.AreEqual ("*sort-expression*", d.SortExpression, "SortExpression set");
			d.Visible = true;
			Assert.IsTrue (d.Visible, "Visible set");
			
			Assert.AreEqual (5, d.GetViewState().Count, "ViewState.Count");

			d.FooterText = null;
			d.HeaderImageUrl = null;
			d.HeaderText = null;
			d.SortExpression = null;
			d.Visible = false;

			/* Visible remains in the ViewState */
			Assert.AreEqual (1, d.GetViewState ().Count, "ViewState.Count after clear");
		}

		[Test]
		public void TestToString ()
		{
			TestDataGridColumn d = new TestDataGridColumn ();

			Assert.AreEqual (String.Empty, d.ToString (), "ToString");
		}

		[Test]
		public void TestInitialize ()
		{
			DataGrid grid = new DataGrid ();
			TestDataGridColumn d = new TestDataGridColumn ();
			
			/* Test DesignMode if I find a class that
			 * implements ISite
			 */
			Assert.IsNull (d.GetOwner (), "Owner before Add");
			
			grid.Columns.Add (d);
			Assert.AreEqual (grid, d.GetOwner (), "Owner after Add");
		}

		[Test]
		public void TestInitializeCell ()
		{
			DataGrid grid = new DataGrid ();
			TestDataGridColumn d = new TestDataGridColumn ();

			
			TableItemStyle footer_style = d.FooterStyle;
			footer_style.CssClass = "*footer-style*";
			footer_style.BackColor = SDColor.YellowGreen;
			
			TableItemStyle header_style = d.HeaderStyle;
			header_style.CssClass = "*header-style*";
			header_style.BackColor = SDColor.ForestGreen;
			
			TableItemStyle item_style = d.ItemStyle;
			item_style.CssClass = "*item-style*";
			item_style.BackColor = SDColor.RoyalBlue;
			
			grid.Columns.Add (d);
			grid.AllowSorting = false;

			Assert.AreEqual ("*footer-style*", d.FooterStyle.CssClass, "Footer style");
			Assert.AreEqual (SDColor.YellowGreen, d.FooterStyle.BackColor, "Footer background");
			Assert.AreEqual ("*header-style*", d.HeaderStyle.CssClass, "Header style");
			Assert.AreEqual (SDColor.ForestGreen, d.HeaderStyle.BackColor, "Header background");
			Assert.AreEqual ("*item-style*", d.ItemStyle.CssClass, "Item style");
			Assert.AreEqual (SDColor.RoyalBlue, d.ItemStyle.BackColor, "Item background");
			
			/* not sorted, text, no sort expression, no
			 * header/footer text
			 */
			TableCell header_cell_ns_t_ne_nhft = new TableCell ();
			TableCell footer_cell_ns_t_ne_nhft = new TableCell ();
			TableCell item_cell_ns_t_ne_nhft = new TableCell ();
			TableCell alternatingitem_cell_ns_t_ne_nhft = new TableCell ();
			TableCell selecteditem_cell_ns_t_ne_nhft = new TableCell ();
			TableCell edititem_cell_ns_t_ne_nhft = new TableCell ();
			TableCell separator_cell_ns_t_ne_nhft = new TableCell ();
			TableCell pager_cell_ns_t_ne_nhft = new TableCell ();

			d.InitializeCell (header_cell_ns_t_ne_nhft, 0, ListItemType.Header);
			d.InitializeCell (footer_cell_ns_t_ne_nhft, 0, ListItemType.Footer);
			d.InitializeCell (item_cell_ns_t_ne_nhft, 0, ListItemType.Item);
			d.InitializeCell (alternatingitem_cell_ns_t_ne_nhft, 0, ListItemType.AlternatingItem);
			d.InitializeCell (selecteditem_cell_ns_t_ne_nhft, 0, ListItemType.SelectedItem);
			d.InitializeCell (edititem_cell_ns_t_ne_nhft, 0, ListItemType.EditItem);
			d.InitializeCell (separator_cell_ns_t_ne_nhft, 0, ListItemType.Separator);
			d.InitializeCell (pager_cell_ns_t_ne_nhft, 0, ListItemType.Pager);

			Assert.AreEqual (String.Empty, header_cell_ns_t_ne_nhft.ControlStyle.CssClass, "Header ns_t_ne_nhft control style");
			Assert.AreEqual (String.Empty, header_cell_ns_t_ne_nhft.CssClass, "Header ns_t_ne_nhft style");
			Assert.AreEqual (SDColor.Empty, header_cell_ns_t_ne_nhft.ControlStyle.BackColor, "Header ns_t_ne_nhft control background");
			Assert.AreEqual (SDColor.Empty, header_cell_ns_t_ne_nhft.BackColor, "Header ns_t_ne_nhft background");
			Assert.AreEqual (String.Empty, footer_cell_ns_t_ne_nhft.ControlStyle.CssClass, "Footer ns_t_ne_nhft control style");
			Assert.AreEqual (String.Empty, footer_cell_ns_t_ne_nhft.CssClass, "Footer ns_t_ne_nhft style");
			Assert.AreEqual (SDColor.Empty, footer_cell_ns_t_ne_nhft.ControlStyle.BackColor, "Footer ns_t_ne_nhft control background");
			Assert.AreEqual (SDColor.Empty, footer_cell_ns_t_ne_nhft.BackColor, "Footer ns_t_ne_nhft background");
			Assert.AreEqual (String.Empty, item_cell_ns_t_ne_nhft.ControlStyle.CssClass, "Item ns_t_ne_nhft control style");
			Assert.AreEqual (String.Empty, item_cell_ns_t_ne_nhft.CssClass, "Item ns_t_ne_nhft style");
			Assert.AreEqual (SDColor.Empty, item_cell_ns_t_ne_nhft.ControlStyle.BackColor, "Item ns_t_ne_nhft control background");
			Assert.AreEqual (SDColor.Empty, item_cell_ns_t_ne_nhft.BackColor, "Item ns_t_ne_nhft background");
			Assert.AreEqual (String.Empty, alternatingitem_cell_ns_t_ne_nhft.ControlStyle.CssClass, "AlternatingItem ns_t_ne_nhft control style");
			Assert.AreEqual (String.Empty, alternatingitem_cell_ns_t_ne_nhft.CssClass, "AlternatingItem ns_t_ne_nhft style");
			Assert.AreEqual (SDColor.Empty, alternatingitem_cell_ns_t_ne_nhft.ControlStyle.BackColor, "AlternatingItem ns_t_ne_nhft control background");
			Assert.AreEqual (SDColor.Empty, alternatingitem_cell_ns_t_ne_nhft.BackColor, "AlternatingItem ns_t_ne_nhft background");
			Assert.AreEqual (String.Empty, selecteditem_cell_ns_t_ne_nhft.ControlStyle.CssClass, "SelectedItem ns_t_ne_nhft control style");
			Assert.AreEqual (String.Empty, selecteditem_cell_ns_t_ne_nhft.CssClass, "SelectedItem ns_t_ne_nhft style");
			Assert.AreEqual (SDColor.Empty, selecteditem_cell_ns_t_ne_nhft.ControlStyle.BackColor, "SelectedItem ns_t_ne_nhft control background");
			Assert.AreEqual (SDColor.Empty, selecteditem_cell_ns_t_ne_nhft.BackColor, "SelectedItem ns_t_ne_nhft background");
			Assert.AreEqual (String.Empty, edititem_cell_ns_t_ne_nhft.ControlStyle.CssClass, "EditItem ns_t_ne_nhft control style");
			Assert.AreEqual (String.Empty, edititem_cell_ns_t_ne_nhft.CssClass, "EditItem ns_t_ne_nhft style");
			Assert.AreEqual (SDColor.Empty, edititem_cell_ns_t_ne_nhft.ControlStyle.BackColor, "EditItem ns_t_ne_nhft control background");
			Assert.AreEqual (SDColor.Empty, edititem_cell_ns_t_ne_nhft.BackColor, "EditItem ns_t_ne_nhft background");
			Assert.AreEqual (String.Empty, separator_cell_ns_t_ne_nhft.ControlStyle.CssClass, "Separator ns_t_ne_nhft control style");
			Assert.AreEqual (String.Empty, separator_cell_ns_t_ne_nhft.CssClass, "Separator ns_t_ne_nhft style");
			Assert.AreEqual (SDColor.Empty, separator_cell_ns_t_ne_nhft.ControlStyle.BackColor, "Separator ns_t_ne_nhft control background");
			Assert.AreEqual (SDColor.Empty, separator_cell_ns_t_ne_nhft.BackColor, "Separator ns_t_ne_nhft background");
			Assert.AreEqual (String.Empty, pager_cell_ns_t_ne_nhft.ControlStyle.CssClass, "Pager ns_t_ne_nhft control style");
			Assert.AreEqual (String.Empty, pager_cell_ns_t_ne_nhft.CssClass, "Pager ns_t_ne_nhft style");
			Assert.AreEqual (SDColor.Empty, pager_cell_ns_t_ne_nhft.ControlStyle.BackColor, "Pager ns_t_ne_nhft control background");
			Assert.AreEqual (SDColor.Empty, pager_cell_ns_t_ne_nhft.BackColor, "Pager ns_t_ne_nhft background");

			Assert.AreEqual (0, header_cell_ns_t_ne_nhft.Controls.Count, "Header ns_t_ne_nhft controls count");
			Assert.AreEqual (0, footer_cell_ns_t_ne_nhft.Controls.Count, "Footer ns_t_ne_nhft controls count");
			Assert.AreEqual (0, item_cell_ns_t_ne_nhft.Controls.Count, "Item ns_t_ne_nhft controls count");
			Assert.AreEqual (0, alternatingitem_cell_ns_t_ne_nhft.Controls.Count, "AlternatingItem ns_t_ne_nhft controls count");
			Assert.AreEqual (0, selecteditem_cell_ns_t_ne_nhft.Controls.Count, "SelectedItem ns_t_ne_nhft controls count");
			Assert.AreEqual (0, edititem_cell_ns_t_ne_nhft.Controls.Count, "EditItem ns_t_ne_nhft controls count");
			Assert.AreEqual (0, separator_cell_ns_t_ne_nhft.Controls.Count, "Separator ns_t_ne_nhft controls count");
			Assert.AreEqual (0, pager_cell_ns_t_ne_nhft.Controls.Count, "Pager ns_t_ne_nhft controls count");

			Assert.AreEqual ("&nbsp;", header_cell_ns_t_ne_nhft.Text, "Header ns_t_ne_nhft text");
			Assert.AreEqual ("&nbsp;", footer_cell_ns_t_ne_nhft.Text, "Footer ns_t_ne_nhft text");
			Assert.AreEqual (String.Empty, item_cell_ns_t_ne_nhft.Text, "Item ns_t_ne_nhft text");
			Assert.AreEqual (String.Empty, alternatingitem_cell_ns_t_ne_nhft.Text, "AlternatingItem ns_t_ne_nhft text");
			Assert.AreEqual (String.Empty, selecteditem_cell_ns_t_ne_nhft.Text, "SelectedItem ns_t_ne_nhft text");
			Assert.AreEqual (String.Empty, edititem_cell_ns_t_ne_nhft.Text, "EditItem ns_t_ne_nhft text");
			Assert.AreEqual (String.Empty, separator_cell_ns_t_ne_nhft.Text, "Separator ns_t_ne_nhft text");
			Assert.AreEqual (String.Empty, pager_cell_ns_t_ne_nhft.Text, "Pager ns_t_ne_nhft text");


			
			/* sorted, text, no sort expression, no
			 * header/footer text
			 */

			grid.AllowSorting = true;
			
			TableCell header_cell_s_t_ne_nhft = new TableCell ();
			TableCell footer_cell_s_t_ne_nhft = new TableCell ();
			TableCell item_cell_s_t_ne_nhft = new TableCell ();
			TableCell alternatingitem_cell_s_t_ne_nhft = new TableCell ();
			TableCell selecteditem_cell_s_t_ne_nhft = new TableCell ();
			TableCell edititem_cell_s_t_ne_nhft = new TableCell ();
			TableCell separator_cell_s_t_ne_nhft = new TableCell ();
			TableCell pager_cell_s_t_ne_nhft = new TableCell ();

			d.InitializeCell (header_cell_s_t_ne_nhft, 0, ListItemType.Header);
			d.InitializeCell (footer_cell_s_t_ne_nhft, 0, ListItemType.Footer);
			d.InitializeCell (item_cell_s_t_ne_nhft, 0, ListItemType.Item);
			d.InitializeCell (alternatingitem_cell_s_t_ne_nhft, 0, ListItemType.AlternatingItem);
			d.InitializeCell (selecteditem_cell_s_t_ne_nhft, 0, ListItemType.SelectedItem);
			d.InitializeCell (edititem_cell_s_t_ne_nhft, 0, ListItemType.EditItem);
			d.InitializeCell (separator_cell_s_t_ne_nhft, 0, ListItemType.Separator);
			d.InitializeCell (pager_cell_s_t_ne_nhft, 0, ListItemType.Pager);

			Assert.AreEqual (String.Empty, header_cell_s_t_ne_nhft.ControlStyle.CssClass, "Header s_t_ne_nhft control style");
			Assert.AreEqual (String.Empty, header_cell_s_t_ne_nhft.CssClass, "Header s_t_ne_nhft style");
			Assert.AreEqual (SDColor.Empty, header_cell_s_t_ne_nhft.ControlStyle.BackColor, "Header s_t_ne_nhft control background");
			Assert.AreEqual (SDColor.Empty, header_cell_s_t_ne_nhft.BackColor, "Header s_t_ne_nhft background");
			Assert.AreEqual (String.Empty, footer_cell_s_t_ne_nhft.ControlStyle.CssClass, "Footer s_t_ne_nhft control style");
			Assert.AreEqual (String.Empty, footer_cell_s_t_ne_nhft.CssClass, "Footer s_t_ne_nhft style");
			Assert.AreEqual (SDColor.Empty, footer_cell_s_t_ne_nhft.ControlStyle.BackColor, "Footer s_t_ne_nhft control background");
			Assert.AreEqual (SDColor.Empty, footer_cell_s_t_ne_nhft.BackColor, "Footer s_t_ne_nhft background");
			Assert.AreEqual (String.Empty, item_cell_s_t_ne_nhft.ControlStyle.CssClass, "Item s_t_ne_nhft control style");
			Assert.AreEqual (String.Empty, item_cell_s_t_ne_nhft.CssClass, "Item s_t_ne_nhft style");
			Assert.AreEqual (SDColor.Empty, item_cell_s_t_ne_nhft.ControlStyle.BackColor, "Item s_t_ne_nhft control background");
			Assert.AreEqual (SDColor.Empty, item_cell_s_t_ne_nhft.BackColor, "Item s_t_ne_nhft background");
			Assert.AreEqual (String.Empty, alternatingitem_cell_s_t_ne_nhft.ControlStyle.CssClass, "AlternatingItem s_t_ne_nhft control style");
			Assert.AreEqual (String.Empty, alternatingitem_cell_s_t_ne_nhft.CssClass, "AlternatingItem s_t_ne_nhft style");
			Assert.AreEqual (SDColor.Empty, alternatingitem_cell_s_t_ne_nhft.ControlStyle.BackColor, "AlternatingItem s_t_ne_nhft control background");
			Assert.AreEqual (SDColor.Empty, alternatingitem_cell_s_t_ne_nhft.BackColor, "AlternatingItem s_t_ne_nhft background");
			Assert.AreEqual (String.Empty, selecteditem_cell_s_t_ne_nhft.ControlStyle.CssClass, "SelectedItem s_t_ne_nhft control style");
			Assert.AreEqual (String.Empty, selecteditem_cell_s_t_ne_nhft.CssClass, "SelectedItem s_t_ne_nhft style");
			Assert.AreEqual (SDColor.Empty, selecteditem_cell_s_t_ne_nhft.ControlStyle.BackColor, "SelectedItem s_t_ne_nhft control background");
			Assert.AreEqual (SDColor.Empty, selecteditem_cell_s_t_ne_nhft.BackColor, "SelectedItem s_t_ne_nhft background");
			Assert.AreEqual (String.Empty, edititem_cell_s_t_ne_nhft.ControlStyle.CssClass, "EditItem s_t_ne_nhft control style");
			Assert.AreEqual (String.Empty, edititem_cell_s_t_ne_nhft.CssClass, "EditItem s_t_ne_nhft style");
			Assert.AreEqual (SDColor.Empty, edititem_cell_s_t_ne_nhft.ControlStyle.BackColor, "EditItem s_t_ne_nhft control background");
			Assert.AreEqual (SDColor.Empty, edititem_cell_s_t_ne_nhft.BackColor, "EditItem s_t_ne_nhft background");
			Assert.AreEqual (String.Empty, separator_cell_s_t_ne_nhft.ControlStyle.CssClass, "Separator s_t_ne_nhft control style");
			Assert.AreEqual (String.Empty, separator_cell_s_t_ne_nhft.CssClass, "Separator s_t_ne_nhft style");
			Assert.AreEqual (SDColor.Empty, separator_cell_s_t_ne_nhft.ControlStyle.BackColor, "Separator s_t_ne_nhft control background");
			Assert.AreEqual (SDColor.Empty, separator_cell_s_t_ne_nhft.BackColor, "Separator s_t_ne_nhft background");
			Assert.AreEqual (String.Empty, pager_cell_s_t_ne_nhft.ControlStyle.CssClass, "Pager s_t_ne_nhft control style");
			Assert.AreEqual (String.Empty, pager_cell_s_t_ne_nhft.CssClass, "Pager s_t_ne_nhft style");
			Assert.AreEqual (SDColor.Empty, pager_cell_s_t_ne_nhft.ControlStyle.BackColor, "Pager s_t_ne_nhft control background");
			Assert.AreEqual (SDColor.Empty, pager_cell_s_t_ne_nhft.BackColor, "Pager s_t_ne_nhft background");

			Assert.AreEqual (0, header_cell_s_t_ne_nhft.Controls.Count, "Header s_t_ne_nhft controls count");
			Assert.AreEqual (0, footer_cell_s_t_ne_nhft.Controls.Count, "Footer s_t_ne_nhft controls count");
			Assert.AreEqual (0, item_cell_s_t_ne_nhft.Controls.Count, "Item s_t_ne_nhft controls count");
			Assert.AreEqual (0, alternatingitem_cell_s_t_ne_nhft.Controls.Count, "AlternatingItem s_t_ne_nhft controls count");
			Assert.AreEqual (0, selecteditem_cell_s_t_ne_nhft.Controls.Count, "SelectedItem s_t_ne_nhft controls count");
			Assert.AreEqual (0, edititem_cell_s_t_ne_nhft.Controls.Count, "EditItem s_t_ne_nhft controls count");
			Assert.AreEqual (0, separator_cell_s_t_ne_nhft.Controls.Count, "Separator s_t_ne_nhft controls count");
			Assert.AreEqual (0, pager_cell_s_t_ne_nhft.Controls.Count, "Pager s_t_ne_nhft controls count");

			Assert.AreEqual ("&nbsp;", header_cell_s_t_ne_nhft.Text, "Header s_t_ne_nhft text");
			Assert.AreEqual ("&nbsp;", footer_cell_s_t_ne_nhft.Text, "Footer s_t_ne_nhft text");
			Assert.AreEqual (String.Empty, item_cell_s_t_ne_nhft.Text, "Item s_t_ne_nhft text");
			Assert.AreEqual (String.Empty, alternatingitem_cell_s_t_ne_nhft.Text, "AlternatingItem s_t_ne_nhft text");
			Assert.AreEqual (String.Empty, selecteditem_cell_s_t_ne_nhft.Text, "SelectedItem s_t_ne_nhft text");
			Assert.AreEqual (String.Empty, edititem_cell_s_t_ne_nhft.Text, "EditItem s_t_ne_nhft text");
			Assert.AreEqual (String.Empty, separator_cell_s_t_ne_nhft.Text, "Separator s_t_ne_nhft text");
			Assert.AreEqual (String.Empty, pager_cell_s_t_ne_nhft.Text, "Pager s_t_ne_nhft text");
			

			
			/* sorted, text, sort expression, no
			 * header/footer text
			 */

			d.SortExpression = "*sort_expression*";
			
			TableCell header_cell_s_t_e_nhft = new TableCell ();
			TableCell footer_cell_s_t_e_nhft = new TableCell ();
			TableCell item_cell_s_t_e_nhft = new TableCell ();
			TableCell alternatingitem_cell_s_t_e_nhft = new TableCell ();
			TableCell selecteditem_cell_s_t_e_nhft = new TableCell ();
			TableCell edititem_cell_s_t_e_nhft = new TableCell ();
			TableCell separator_cell_s_t_e_nhft = new TableCell ();
			TableCell pager_cell_s_t_e_nhft = new TableCell ();

			d.InitializeCell (header_cell_s_t_e_nhft, 0, ListItemType.Header);
			d.InitializeCell (footer_cell_s_t_e_nhft, 0, ListItemType.Footer);
			d.InitializeCell (item_cell_s_t_e_nhft, 0, ListItemType.Item);
			d.InitializeCell (alternatingitem_cell_s_t_e_nhft, 0, ListItemType.AlternatingItem);
			d.InitializeCell (selecteditem_cell_s_t_e_nhft, 0, ListItemType.SelectedItem);
			d.InitializeCell (edititem_cell_s_t_e_nhft, 0, ListItemType.EditItem);
			d.InitializeCell (separator_cell_s_t_e_nhft, 0, ListItemType.Separator);
			d.InitializeCell (pager_cell_s_t_e_nhft, 0, ListItemType.Pager);

			Assert.AreEqual (String.Empty, header_cell_s_t_e_nhft.ControlStyle.CssClass, "Header s_t_e_nhft control style");
			Assert.AreEqual (String.Empty, header_cell_s_t_e_nhft.CssClass, "Header s_t_e_nhft style");
			Assert.AreEqual (SDColor.Empty, header_cell_s_t_e_nhft.ControlStyle.BackColor, "Header s_t_e_nhft control background");
			Assert.AreEqual (SDColor.Empty, header_cell_s_t_e_nhft.BackColor, "Header s_t_e_nhft background");
			Assert.AreEqual (String.Empty, footer_cell_s_t_e_nhft.ControlStyle.CssClass, "Footer s_t_e_nhft control style");
			Assert.AreEqual (String.Empty, footer_cell_s_t_e_nhft.CssClass, "Footer s_t_e_nhft style");
			Assert.AreEqual (SDColor.Empty, footer_cell_s_t_e_nhft.ControlStyle.BackColor, "Footer s_t_e_nhft control background");
			Assert.AreEqual (SDColor.Empty, footer_cell_s_t_e_nhft.BackColor, "Footer s_t_e_nhft background");
			Assert.AreEqual (String.Empty, item_cell_s_t_e_nhft.ControlStyle.CssClass, "Item s_t_e_nhft control style");
			Assert.AreEqual (String.Empty, item_cell_s_t_e_nhft.CssClass, "Item s_t_e_nhft style");
			Assert.AreEqual (SDColor.Empty, item_cell_s_t_e_nhft.ControlStyle.BackColor, "Item s_t_e_nhft control background");
			Assert.AreEqual (SDColor.Empty, item_cell_s_t_e_nhft.BackColor, "Item s_t_e_nhft background");
			Assert.AreEqual (String.Empty, alternatingitem_cell_s_t_e_nhft.ControlStyle.CssClass, "AlternatingItem s_t_e_nhft control style");
			Assert.AreEqual (String.Empty, alternatingitem_cell_s_t_e_nhft.CssClass, "AlternatingItem s_t_e_nhft style");
			Assert.AreEqual (SDColor.Empty, alternatingitem_cell_s_t_e_nhft.ControlStyle.BackColor, "AlternatingItem s_t_e_nhft control background");
			Assert.AreEqual (SDColor.Empty, alternatingitem_cell_s_t_e_nhft.BackColor, "AlternatingItem s_t_e_nhft background");
			Assert.AreEqual (String.Empty, selecteditem_cell_s_t_e_nhft.ControlStyle.CssClass, "SelectedItem s_t_e_nhft control style");
			Assert.AreEqual (String.Empty, selecteditem_cell_s_t_e_nhft.CssClass, "SelectedItem s_t_e_nhft style");
			Assert.AreEqual (SDColor.Empty, selecteditem_cell_s_t_e_nhft.ControlStyle.BackColor, "SelectedItem s_t_e_nhft control background");
			Assert.AreEqual (SDColor.Empty, selecteditem_cell_s_t_e_nhft.BackColor, "SelectedItem s_t_e_nhft background");
			Assert.AreEqual (String.Empty, edititem_cell_s_t_e_nhft.ControlStyle.CssClass, "EditItem s_t_e_nhft control style");
			Assert.AreEqual (String.Empty, edititem_cell_s_t_e_nhft.CssClass, "EditItem s_t_e_nhft style");
			Assert.AreEqual (SDColor.Empty, edititem_cell_s_t_e_nhft.ControlStyle.BackColor, "EditItem s_t_e_nhft control background");
			Assert.AreEqual (SDColor.Empty, edititem_cell_s_t_e_nhft.BackColor, "EditItem s_t_e_nhft background");
			Assert.AreEqual (String.Empty, separator_cell_s_t_e_nhft.ControlStyle.CssClass, "Separator s_t_e_nhft control style");
			Assert.AreEqual (String.Empty, separator_cell_s_t_e_nhft.CssClass, "Separator s_t_e_nhft style");
			Assert.AreEqual (SDColor.Empty, separator_cell_s_t_e_nhft.ControlStyle.BackColor, "Separator s_t_e_nhft control background");
			Assert.AreEqual (SDColor.Empty, separator_cell_s_t_e_nhft.BackColor, "Separator s_t_e_nhft background");
			Assert.AreEqual (String.Empty, pager_cell_s_t_e_nhft.ControlStyle.CssClass, "Pager s_t_e_nhft control style");
			Assert.AreEqual (String.Empty, pager_cell_s_t_e_nhft.CssClass, "Pager s_t_e_nhft style");
			Assert.AreEqual (SDColor.Empty, pager_cell_s_t_e_nhft.ControlStyle.BackColor, "Pager s_t_e_nhft control background");
			Assert.AreEqual (SDColor.Empty, pager_cell_s_t_e_nhft.BackColor, "Pager s_t_e_nhft background");

			Assert.AreEqual (1, header_cell_s_t_e_nhft.Controls.Count, "Header s_t_e_nhft controls count");
			Assert.AreEqual (0, footer_cell_s_t_e_nhft.Controls.Count, "Footer s_t_e_nhft controls count");
			Assert.AreEqual (0, item_cell_s_t_e_nhft.Controls.Count, "Item s_t_e_nhft controls count");
			Assert.AreEqual (0, alternatingitem_cell_s_t_e_nhft.Controls.Count, "AlternatingItem s_t_e_nhft controls count");
			Assert.AreEqual (0, selecteditem_cell_s_t_e_nhft.Controls.Count, "SelectedItem s_t_e_nhft controls count");
			Assert.AreEqual (0, edititem_cell_s_t_e_nhft.Controls.Count, "EditItem s_t_e_nhft controls count");
			Assert.AreEqual (0, separator_cell_s_t_e_nhft.Controls.Count, "Separator s_t_e_nhft controls count");
			Assert.AreEqual (0, pager_cell_s_t_e_nhft.Controls.Count, "Pager s_t_e_nhft controls count");

			Assert.AreEqual (String.Empty, header_cell_s_t_e_nhft.Text, "Header s_t_e_nhft text");
			Assert.AreEqual ("&nbsp;", footer_cell_s_t_e_nhft.Text, "Footer s_t_e_nhft text");
			Assert.AreEqual (String.Empty, item_cell_s_t_e_nhft.Text, "Item s_t_e_nhft text");
			Assert.AreEqual (String.Empty, alternatingitem_cell_s_t_e_nhft.Text, "AlternatingItem s_t_e_nhft text");
			Assert.AreEqual (String.Empty, selecteditem_cell_s_t_e_nhft.Text, "SelectedItem s_t_e_nhft text");
			Assert.AreEqual (String.Empty, edititem_cell_s_t_e_nhft.Text, "EditItem s_t_e_nhft text");
			Assert.AreEqual (String.Empty, separator_cell_s_t_e_nhft.Text, "Separator s_t_e_nhft text");
			Assert.AreEqual (String.Empty, pager_cell_s_t_e_nhft.Text, "Pager s_t_e_nhft text");

			LinkButton link = header_cell_s_t_e_nhft.Controls[0] as LinkButton;
			Assert.IsNotNull (link, "Header s_t_e_nhft LinkButton");
			Assert.AreEqual (String.Empty, link.Text, "Header s_t_e_nhft LinkButton text");
			Assert.AreEqual ("Sort", link.CommandName, "Header s_t_e_nhft LinkButton command name");
			Assert.AreEqual ("*sort_expression*", link.CommandArgument, "Header s_t_e_nhft LinkButton command argument");



			/* XXXXXX  Image starts here XXXXXX */



			
			/* not sorted, image, no sort expression, no
			 * header/footer text
			 */

			d.HeaderImageUrl = "*header_image_url*";
			d.SortExpression = null;
			grid.AllowSorting = false;

			TableCell header_cell_ns_i_ne_nhft = new TableCell ();
			TableCell footer_cell_ns_i_ne_nhft = new TableCell ();
			TableCell item_cell_ns_i_ne_nhft = new TableCell ();
			TableCell alternatingitem_cell_ns_i_ne_nhft = new TableCell ();
			TableCell selecteditem_cell_ns_i_ne_nhft = new TableCell ();
			TableCell edititem_cell_ns_i_ne_nhft = new TableCell ();
			TableCell separator_cell_ns_i_ne_nhft = new TableCell ();
			TableCell pager_cell_ns_i_ne_nhft = new TableCell ();

			d.InitializeCell (header_cell_ns_i_ne_nhft, 0, ListItemType.Header);
			d.InitializeCell (footer_cell_ns_i_ne_nhft, 0, ListItemType.Footer);
			d.InitializeCell (item_cell_ns_i_ne_nhft, 0, ListItemType.Item);
			d.InitializeCell (alternatingitem_cell_ns_i_ne_nhft, 0, ListItemType.AlternatingItem);
			d.InitializeCell (selecteditem_cell_ns_i_ne_nhft, 0, ListItemType.SelectedItem);
			d.InitializeCell (edititem_cell_ns_i_ne_nhft, 0, ListItemType.EditItem);
			d.InitializeCell (separator_cell_ns_i_ne_nhft, 0, ListItemType.Separator);
			d.InitializeCell (pager_cell_ns_i_ne_nhft, 0, ListItemType.Pager);

			Assert.AreEqual (String.Empty, header_cell_ns_i_ne_nhft.ControlStyle.CssClass, "Header ns_i_ne_nhft control style");
			Assert.AreEqual (String.Empty, header_cell_ns_i_ne_nhft.CssClass, "Header ns_i_ne_nhft style");
			Assert.AreEqual (SDColor.Empty, header_cell_ns_i_ne_nhft.ControlStyle.BackColor, "Header ns_i_ne_nhft control background");
			Assert.AreEqual (SDColor.Empty, header_cell_ns_i_ne_nhft.BackColor, "Header ns_i_ne_nhft background");
			Assert.AreEqual (String.Empty, footer_cell_ns_i_ne_nhft.ControlStyle.CssClass, "Footer ns_i_ne_nhft control style");
			Assert.AreEqual (String.Empty, footer_cell_ns_i_ne_nhft.CssClass, "Footer ns_i_ne_nhft style");
			Assert.AreEqual (SDColor.Empty, footer_cell_ns_i_ne_nhft.ControlStyle.BackColor, "Footer ns_i_ne_nhft control background");
			Assert.AreEqual (SDColor.Empty, footer_cell_ns_i_ne_nhft.BackColor, "Footer ns_i_ne_nhft background");
			Assert.AreEqual (String.Empty, item_cell_ns_i_ne_nhft.ControlStyle.CssClass, "Item ns_i_ne_nhft control style");
			Assert.AreEqual (String.Empty, item_cell_ns_i_ne_nhft.CssClass, "Item ns_i_ne_nhft style");
			Assert.AreEqual (SDColor.Empty, item_cell_ns_i_ne_nhft.ControlStyle.BackColor, "Item ns_i_ne_nhft control background");
			Assert.AreEqual (SDColor.Empty, item_cell_ns_i_ne_nhft.BackColor, "Item ns_i_ne_nhft background");
			Assert.AreEqual (String.Empty, alternatingitem_cell_ns_i_ne_nhft.ControlStyle.CssClass, "AlternatingItem ns_i_ne_nhft control style");
			Assert.AreEqual (String.Empty, alternatingitem_cell_ns_i_ne_nhft.CssClass, "AlternatingItem ns_i_ne_nhft style");
			Assert.AreEqual (SDColor.Empty, alternatingitem_cell_ns_i_ne_nhft.ControlStyle.BackColor, "AlternatingItem ns_i_ne_nhft control background");
			Assert.AreEqual (SDColor.Empty, alternatingitem_cell_ns_i_ne_nhft.BackColor, "AlternatingItem ns_i_ne_nhft background");
			Assert.AreEqual (String.Empty, selecteditem_cell_ns_i_ne_nhft.ControlStyle.CssClass, "SelectedItem ns_i_ne_nhft control style");
			Assert.AreEqual (String.Empty, selecteditem_cell_ns_i_ne_nhft.CssClass, "SelectedItem ns_i_ne_nhft style");
			Assert.AreEqual (SDColor.Empty, selecteditem_cell_ns_i_ne_nhft.ControlStyle.BackColor, "SelectedItem ns_i_ne_nhft control background");
			Assert.AreEqual (SDColor.Empty, selecteditem_cell_ns_i_ne_nhft.BackColor, "SelectedItem ns_i_ne_nhft background");
			Assert.AreEqual (String.Empty, edititem_cell_ns_i_ne_nhft.ControlStyle.CssClass, "EditItem ns_i_ne_nhft control style");
			Assert.AreEqual (String.Empty, edititem_cell_ns_i_ne_nhft.CssClass, "EditItem ns_i_ne_nhft style");
			Assert.AreEqual (SDColor.Empty, edititem_cell_ns_i_ne_nhft.ControlStyle.BackColor, "EditItem ns_i_ne_nhft control background");
			Assert.AreEqual (SDColor.Empty, edititem_cell_ns_i_ne_nhft.BackColor, "EditItem ns_i_ne_nhft background");
			Assert.AreEqual (String.Empty, separator_cell_ns_i_ne_nhft.ControlStyle.CssClass, "Separator ns_i_ne_nhft control style");
			Assert.AreEqual (String.Empty, separator_cell_ns_i_ne_nhft.CssClass, "Separator ns_i_ne_nhft style");
			Assert.AreEqual (SDColor.Empty, separator_cell_ns_i_ne_nhft.ControlStyle.BackColor, "Separator ns_i_ne_nhft control background");
			Assert.AreEqual (SDColor.Empty, separator_cell_ns_i_ne_nhft.BackColor, "Separator ns_i_ne_nhft background");
			Assert.AreEqual (String.Empty, pager_cell_ns_i_ne_nhft.ControlStyle.CssClass, "Pager ns_i_ne_nhft control style");
			Assert.AreEqual (String.Empty, pager_cell_ns_i_ne_nhft.CssClass, "Pager ns_i_ne_nhft style");
			Assert.AreEqual (SDColor.Empty, pager_cell_ns_i_ne_nhft.ControlStyle.BackColor, "Pager ns_i_ne_nhft control background");
			Assert.AreEqual (SDColor.Empty, pager_cell_ns_i_ne_nhft.BackColor, "Pager ns_i_ne_nhft background");

			Assert.AreEqual (1, header_cell_ns_i_ne_nhft.Controls.Count, "Header ns_i_ne_nhft controls count");
			Assert.AreEqual (0, footer_cell_ns_i_ne_nhft.Controls.Count, "Footer ns_i_ne_nhft controls count");
			Assert.AreEqual (0, item_cell_ns_i_ne_nhft.Controls.Count, "Item ns_i_ne_nhft controls count");
			Assert.AreEqual (0, alternatingitem_cell_ns_i_ne_nhft.Controls.Count, "AlternatingItem ns_i_ne_nhft controls count");
			Assert.AreEqual (0, selecteditem_cell_ns_i_ne_nhft.Controls.Count, "SelectedItem ns_i_ne_nhft controls count");
			Assert.AreEqual (0, edititem_cell_ns_i_ne_nhft.Controls.Count, "EditItem ns_i_ne_nhft controls count");
			Assert.AreEqual (0, separator_cell_ns_i_ne_nhft.Controls.Count, "Separator ns_i_ne_nhft controls count");
			Assert.AreEqual (0, pager_cell_ns_i_ne_nhft.Controls.Count, "Pager ns_i_ne_nhft controls count");

			Assert.AreEqual (String.Empty, header_cell_ns_i_ne_nhft.Text, "Header ns_i_ne_nhft text");
			Assert.AreEqual ("&nbsp;", footer_cell_ns_i_ne_nhft.Text, "Footer ns_i_ne_nhft text");
			Assert.AreEqual (String.Empty, item_cell_ns_i_ne_nhft.Text, "Item ns_i_ne_nhft text");
			Assert.AreEqual (String.Empty, alternatingitem_cell_ns_i_ne_nhft.Text, "AlternatingItem ns_i_ne_nhft text");
			Assert.AreEqual (String.Empty, selecteditem_cell_ns_i_ne_nhft.Text, "SelectedItem ns_i_ne_nhft text");
			Assert.AreEqual (String.Empty, edititem_cell_ns_i_ne_nhft.Text, "EditItem ns_i_ne_nhft text");
			Assert.AreEqual (String.Empty, separator_cell_ns_i_ne_nhft.Text, "Separator ns_i_ne_nhft text");
			Assert.AreEqual (String.Empty, pager_cell_ns_i_ne_nhft.Text, "Pager ns_i_ne_nhft text");


			Image image = header_cell_ns_i_ne_nhft.Controls[0] as Image;
			Assert.IsNotNull (link, "Header ns_i_ne_nhft Image");
			Assert.AreEqual (String.Empty, link.Text, "Header ns_i_ne_nhft Image text");
			Assert.AreEqual ("*header_image_url*", image.ImageUrl, "Header ns_i_ne_nhft Image command name");

			
			/* sorted, image, no sort expression, no
			 * header/footer text
			 */

			grid.AllowSorting = true;
			
			TableCell header_cell_s_i_ne_nhft = new TableCell ();
			TableCell footer_cell_s_i_ne_nhft = new TableCell ();
			TableCell item_cell_s_i_ne_nhft = new TableCell ();
			TableCell alternatingitem_cell_s_i_ne_nhft = new TableCell ();
			TableCell selecteditem_cell_s_i_ne_nhft = new TableCell ();
			TableCell edititem_cell_s_i_ne_nhft = new TableCell ();
			TableCell separator_cell_s_i_ne_nhft = new TableCell ();
			TableCell pager_cell_s_i_ne_nhft = new TableCell ();

			d.InitializeCell (header_cell_s_i_ne_nhft, 0, ListItemType.Header);
			d.InitializeCell (footer_cell_s_i_ne_nhft, 0, ListItemType.Footer);
			d.InitializeCell (item_cell_s_i_ne_nhft, 0, ListItemType.Item);
			d.InitializeCell (alternatingitem_cell_s_i_ne_nhft, 0, ListItemType.AlternatingItem);
			d.InitializeCell (selecteditem_cell_s_i_ne_nhft, 0, ListItemType.SelectedItem);
			d.InitializeCell (edititem_cell_s_i_ne_nhft, 0, ListItemType.EditItem);
			d.InitializeCell (separator_cell_s_i_ne_nhft, 0, ListItemType.Separator);
			d.InitializeCell (pager_cell_s_i_ne_nhft, 0, ListItemType.Pager);

			Assert.AreEqual (String.Empty, header_cell_s_i_ne_nhft.ControlStyle.CssClass, "Header s_i_ne_nhft control style");
			Assert.AreEqual (String.Empty, header_cell_s_i_ne_nhft.CssClass, "Header s_i_ne_nhft style");
			Assert.AreEqual (SDColor.Empty, header_cell_s_i_ne_nhft.ControlStyle.BackColor, "Header s_i_ne_nhft control background");
			Assert.AreEqual (SDColor.Empty, header_cell_s_i_ne_nhft.BackColor, "Header s_i_ne_nhft background");
			Assert.AreEqual (String.Empty, footer_cell_s_i_ne_nhft.ControlStyle.CssClass, "Footer s_i_ne_nhft control style");
			Assert.AreEqual (String.Empty, footer_cell_s_i_ne_nhft.CssClass, "Footer s_i_ne_nhft style");
			Assert.AreEqual (SDColor.Empty, footer_cell_s_i_ne_nhft.ControlStyle.BackColor, "Footer s_i_ne_nhft control background");
			Assert.AreEqual (SDColor.Empty, footer_cell_s_i_ne_nhft.BackColor, "Footer s_i_ne_nhft background");
			Assert.AreEqual (String.Empty, item_cell_s_i_ne_nhft.ControlStyle.CssClass, "Item s_i_ne_nhft control style");
			Assert.AreEqual (String.Empty, item_cell_s_i_ne_nhft.CssClass, "Item s_i_ne_nhft style");
			Assert.AreEqual (SDColor.Empty, item_cell_s_i_ne_nhft.ControlStyle.BackColor, "Item s_i_ne_nhft control background");
			Assert.AreEqual (SDColor.Empty, item_cell_s_i_ne_nhft.BackColor, "Item s_i_ne_nhft background");
			Assert.AreEqual (String.Empty, alternatingitem_cell_s_i_ne_nhft.ControlStyle.CssClass, "AlternatingItem s_i_ne_nhft control style");
			Assert.AreEqual (String.Empty, alternatingitem_cell_s_i_ne_nhft.CssClass, "AlternatingItem s_i_ne_nhft style");
			Assert.AreEqual (SDColor.Empty, alternatingitem_cell_s_i_ne_nhft.ControlStyle.BackColor, "AlternatingItem s_i_ne_nhft control background");
			Assert.AreEqual (SDColor.Empty, alternatingitem_cell_s_i_ne_nhft.BackColor, "AlternatingItem s_i_ne_nhft background");
			Assert.AreEqual (String.Empty, selecteditem_cell_s_i_ne_nhft.ControlStyle.CssClass, "SelectedItem s_i_ne_nhft control style");
			Assert.AreEqual (String.Empty, selecteditem_cell_s_i_ne_nhft.CssClass, "SelectedItem s_i_ne_nhft style");
			Assert.AreEqual (SDColor.Empty, selecteditem_cell_s_i_ne_nhft.ControlStyle.BackColor, "SelectedItem s_i_ne_nhft control background");
			Assert.AreEqual (SDColor.Empty, selecteditem_cell_s_i_ne_nhft.BackColor, "SelectedItem s_i_ne_nhft background");
			Assert.AreEqual (String.Empty, edititem_cell_s_i_ne_nhft.ControlStyle.CssClass, "EditItem s_i_ne_nhft control style");
			Assert.AreEqual (String.Empty, edititem_cell_s_i_ne_nhft.CssClass, "EditItem s_i_ne_nhft style");
			Assert.AreEqual (SDColor.Empty, edititem_cell_s_i_ne_nhft.ControlStyle.BackColor, "EditItem s_i_ne_nhft control background");
			Assert.AreEqual (SDColor.Empty, edititem_cell_s_i_ne_nhft.BackColor, "EditItem s_i_ne_nhft background");
			Assert.AreEqual (String.Empty, separator_cell_s_i_ne_nhft.ControlStyle.CssClass, "Separator s_i_ne_nhft control style");
			Assert.AreEqual (String.Empty, separator_cell_s_i_ne_nhft.CssClass, "Separator s_i_ne_nhft style");
			Assert.AreEqual (SDColor.Empty, separator_cell_s_i_ne_nhft.ControlStyle.BackColor, "Separator s_i_ne_nhft control background");
			Assert.AreEqual (SDColor.Empty, separator_cell_s_i_ne_nhft.BackColor, "Separator s_i_ne_nhft background");
			Assert.AreEqual (String.Empty, pager_cell_s_i_ne_nhft.ControlStyle.CssClass, "Pager s_i_ne_nhft control style");
			Assert.AreEqual (String.Empty, pager_cell_s_i_ne_nhft.CssClass, "Pager s_i_ne_nhft style");
			Assert.AreEqual (SDColor.Empty, pager_cell_s_i_ne_nhft.ControlStyle.BackColor, "Pager s_i_ne_nhft control background");
			Assert.AreEqual (SDColor.Empty, pager_cell_s_i_ne_nhft.BackColor, "Pager s_i_ne_nhft background");

			Assert.AreEqual (1, header_cell_s_i_ne_nhft.Controls.Count, "Header s_i_ne_nhft controls count");
			Assert.AreEqual (0, footer_cell_s_i_ne_nhft.Controls.Count, "Footer s_i_ne_nhft controls count");
			Assert.AreEqual (0, item_cell_s_i_ne_nhft.Controls.Count, "Item s_i_ne_nhft controls count");
			Assert.AreEqual (0, alternatingitem_cell_s_i_ne_nhft.Controls.Count, "AlternatingItem s_i_ne_nhft controls count");
			Assert.AreEqual (0, selecteditem_cell_s_i_ne_nhft.Controls.Count, "SelectedItem s_i_ne_nhft controls count");
			Assert.AreEqual (0, edititem_cell_s_i_ne_nhft.Controls.Count, "EditItem s_i_ne_nhft controls count");
			Assert.AreEqual (0, separator_cell_s_i_ne_nhft.Controls.Count, "Separator s_i_ne_nhft controls count");
			Assert.AreEqual (0, pager_cell_s_i_ne_nhft.Controls.Count, "Pager s_i_ne_nhft controls count");

			Assert.AreEqual (String.Empty, header_cell_s_i_ne_nhft.Text, "Header s_i_ne_nhft text");
			Assert.AreEqual ("&nbsp;", footer_cell_s_i_ne_nhft.Text, "Footer s_i_ne_nhft text");
			Assert.AreEqual (String.Empty, item_cell_s_i_ne_nhft.Text, "Item s_i_ne_nhft text");
			Assert.AreEqual (String.Empty, alternatingitem_cell_s_i_ne_nhft.Text, "AlternatingItem s_i_ne_nhft text");
			Assert.AreEqual (String.Empty, selecteditem_cell_s_i_ne_nhft.Text, "SelectedItem s_i_ne_nhft text");
			Assert.AreEqual (String.Empty, edititem_cell_s_i_ne_nhft.Text, "EditItem s_i_ne_nhft text");
			Assert.AreEqual (String.Empty, separator_cell_s_i_ne_nhft.Text, "Separator s_i_ne_nhft text");
			Assert.AreEqual (String.Empty, pager_cell_s_i_ne_nhft.Text, "Pager s_i_ne_nhft text");
			

			Image image2 = header_cell_s_i_ne_nhft.Controls[0] as Image;
			Assert.IsNotNull (image2, "Header s_i_ne_nhft Image");
			Assert.AreEqual ("*header_image_url*", image2.ImageUrl, "Header s_i_ne_nhft Image command name");


			
			/* sorted, image, sort expression, no
			 * header/footer text
			 */

			d.SortExpression = "*sort_expression*";
			
			TableCell header_cell_s_i_e_nhft = new TableCell ();
			TableCell footer_cell_s_i_e_nhft = new TableCell ();
			TableCell item_cell_s_i_e_nhft = new TableCell ();
			TableCell alternatingitem_cell_s_i_e_nhft = new TableCell ();
			TableCell selecteditem_cell_s_i_e_nhft = new TableCell ();
			TableCell edititem_cell_s_i_e_nhft = new TableCell ();
			TableCell separator_cell_s_i_e_nhft = new TableCell ();
			TableCell pager_cell_s_i_e_nhft = new TableCell ();

			d.InitializeCell (header_cell_s_i_e_nhft, 0, ListItemType.Header);
			d.InitializeCell (footer_cell_s_i_e_nhft, 0, ListItemType.Footer);
			d.InitializeCell (item_cell_s_i_e_nhft, 0, ListItemType.Item);
			d.InitializeCell (alternatingitem_cell_s_i_e_nhft, 0, ListItemType.AlternatingItem);
			d.InitializeCell (selecteditem_cell_s_i_e_nhft, 0, ListItemType.SelectedItem);
			d.InitializeCell (edititem_cell_s_i_e_nhft, 0, ListItemType.EditItem);
			d.InitializeCell (separator_cell_s_i_e_nhft, 0, ListItemType.Separator);
			d.InitializeCell (pager_cell_s_i_e_nhft, 0, ListItemType.Pager);

			Assert.AreEqual (String.Empty, header_cell_s_i_e_nhft.ControlStyle.CssClass, "Header s_i_e_nhft control style");
			Assert.AreEqual (String.Empty, header_cell_s_i_e_nhft.CssClass, "Header s_i_e_nhft style");
			Assert.AreEqual (SDColor.Empty, header_cell_s_i_e_nhft.ControlStyle.BackColor, "Header s_i_e_nhft control background");
			Assert.AreEqual (SDColor.Empty, header_cell_s_i_e_nhft.BackColor, "Header s_i_e_nhft background");
			Assert.AreEqual (String.Empty, footer_cell_s_i_e_nhft.ControlStyle.CssClass, "Footer s_i_e_nhft control style");
			Assert.AreEqual (String.Empty, footer_cell_s_i_e_nhft.CssClass, "Footer s_i_e_nhft style");
			Assert.AreEqual (SDColor.Empty, footer_cell_s_i_e_nhft.ControlStyle.BackColor, "Footer s_i_e_nhft control background");
			Assert.AreEqual (SDColor.Empty, footer_cell_s_i_e_nhft.BackColor, "Footer s_i_e_nhft background");
			Assert.AreEqual (String.Empty, item_cell_s_i_e_nhft.ControlStyle.CssClass, "Item s_i_e_nhft control style");
			Assert.AreEqual (String.Empty, item_cell_s_i_e_nhft.CssClass, "Item s_i_e_nhft style");
			Assert.AreEqual (SDColor.Empty, item_cell_s_i_e_nhft.ControlStyle.BackColor, "Item s_i_e_nhft control background");
			Assert.AreEqual (SDColor.Empty, item_cell_s_i_e_nhft.BackColor, "Item s_i_e_nhft background");
			Assert.AreEqual (String.Empty, alternatingitem_cell_s_i_e_nhft.ControlStyle.CssClass, "AlternatingItem s_i_e_nhft control style");
			Assert.AreEqual (String.Empty, alternatingitem_cell_s_i_e_nhft.CssClass, "AlternatingItem s_i_e_nhft style");
			Assert.AreEqual (SDColor.Empty, alternatingitem_cell_s_i_e_nhft.ControlStyle.BackColor, "AlternatingItem s_i_e_nhft control background");
			Assert.AreEqual (SDColor.Empty, alternatingitem_cell_s_i_e_nhft.BackColor, "AlternatingItem s_i_e_nhft background");
			Assert.AreEqual (String.Empty, selecteditem_cell_s_i_e_nhft.ControlStyle.CssClass, "SelectedItem s_i_e_nhft control style");
			Assert.AreEqual (String.Empty, selecteditem_cell_s_i_e_nhft.CssClass, "SelectedItem s_i_e_nhft style");
			Assert.AreEqual (SDColor.Empty, selecteditem_cell_s_i_e_nhft.ControlStyle.BackColor, "SelectedItem s_i_e_nhft control background");
			Assert.AreEqual (SDColor.Empty, selecteditem_cell_s_i_e_nhft.BackColor, "SelectedItem s_i_e_nhft background");
			Assert.AreEqual (String.Empty, edititem_cell_s_i_e_nhft.ControlStyle.CssClass, "EditItem s_i_e_nhft control style");
			Assert.AreEqual (String.Empty, edititem_cell_s_i_e_nhft.CssClass, "EditItem s_i_e_nhft style");
			Assert.AreEqual (SDColor.Empty, edititem_cell_s_i_e_nhft.ControlStyle.BackColor, "EditItem s_i_e_nhft control background");
			Assert.AreEqual (SDColor.Empty, edititem_cell_s_i_e_nhft.BackColor, "EditItem s_i_e_nhft background");
			Assert.AreEqual (String.Empty, separator_cell_s_i_e_nhft.ControlStyle.CssClass, "Separator s_i_e_nhft control style");
			Assert.AreEqual (String.Empty, separator_cell_s_i_e_nhft.CssClass, "Separator s_i_e_nhft style");
			Assert.AreEqual (SDColor.Empty, separator_cell_s_i_e_nhft.ControlStyle.BackColor, "Separator s_i_e_nhft control background");
			Assert.AreEqual (SDColor.Empty, separator_cell_s_i_e_nhft.BackColor, "Separator s_i_e_nhft background");
			Assert.AreEqual (String.Empty, pager_cell_s_i_e_nhft.ControlStyle.CssClass, "Pager s_i_e_nhft control style");
			Assert.AreEqual (String.Empty, pager_cell_s_i_e_nhft.CssClass, "Pager s_i_e_nhft style");
			Assert.AreEqual (SDColor.Empty, pager_cell_s_i_e_nhft.ControlStyle.BackColor, "Pager s_i_e_nhft control background");
			Assert.AreEqual (SDColor.Empty, pager_cell_s_i_e_nhft.BackColor, "Pager s_i_e_nhft background");

			Assert.AreEqual (1, header_cell_s_i_e_nhft.Controls.Count, "Header s_i_e_nhft controls count");
			Assert.AreEqual (0, footer_cell_s_i_e_nhft.Controls.Count, "Footer s_i_e_nhft controls count");
			Assert.AreEqual (0, item_cell_s_i_e_nhft.Controls.Count, "Item s_i_e_nhft controls count");
			Assert.AreEqual (0, alternatingitem_cell_s_i_e_nhft.Controls.Count, "AlternatingItem s_i_e_nhft controls count");
			Assert.AreEqual (0, selecteditem_cell_s_i_e_nhft.Controls.Count, "SelectedItem s_i_e_nhft controls count");
			Assert.AreEqual (0, edititem_cell_s_i_e_nhft.Controls.Count, "EditItem s_i_e_nhft controls count");
			Assert.AreEqual (0, separator_cell_s_i_e_nhft.Controls.Count, "Separator s_i_e_nhft controls count");
			Assert.AreEqual (0, pager_cell_s_i_e_nhft.Controls.Count, "Pager s_i_e_nhft controls count");

			Assert.AreEqual (String.Empty, header_cell_s_i_e_nhft.Text, "Header s_i_e_nhft text");
			Assert.AreEqual ("&nbsp;", footer_cell_s_i_e_nhft.Text, "Footer s_i_e_nhft text");
			Assert.AreEqual (String.Empty, item_cell_s_i_e_nhft.Text, "Item s_i_e_nhft text");
			Assert.AreEqual (String.Empty, alternatingitem_cell_s_i_e_nhft.Text, "AlternatingItem s_i_e_nhft text");
			Assert.AreEqual (String.Empty, selecteditem_cell_s_i_e_nhft.Text, "SelectedItem s_i_e_nhft text");
			Assert.AreEqual (String.Empty, edititem_cell_s_i_e_nhft.Text, "EditItem s_i_e_nhft text");
			Assert.AreEqual (String.Empty, separator_cell_s_i_e_nhft.Text, "Separator s_i_e_nhft text");
			Assert.AreEqual (String.Empty, pager_cell_s_i_e_nhft.Text, "Pager s_i_e_nhft text");

			ImageButton butt = header_cell_s_i_e_nhft.Controls[0] as ImageButton;
			Assert.IsNotNull (butt, "Header s_i_e_nhft ImageButton");
			Assert.AreEqual ("Sort", butt.CommandName, "Header s_i_e_nhft ImageButton command name");
			Assert.AreEqual ("*sort_expression*", butt.CommandArgument, "Header s_i_e_nhft ImageButton command argument");
			
		}
	}
}
