//
// Tests for System.Web.UI.WebControls.BoundFieldTest.cs
//
// Author:
//	Yoni Klein (yonik@mainsoft.com)
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


#if NET_2_0

using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using System.Drawing;
using System.Collections;
using System.Collections.Specialized;
using Image = System.Web.UI.WebControls.Image;
using NUnit.Framework;
using System.Globalization;
using MonoTests.SystemWeb.Framework;
using MonoTests.stand_alone.WebHarness;



namespace MonoTests.System.Web.UI.WebControls
{
	class PokerBoundField : BoundField
	{
		public Button bindbutoon;

		public PokerBoundField () {
			TrackViewState ();
			bindbutoon = new Button ();
			bindbutoon.DataBinding += new EventHandler (OnDataBindField);
		}


		public StateBag StateBag {
			get { return base.ViewState; }
		}

		public bool DoSupportsHtmlEncode {
			get {
				return base.SupportsHtmlEncode;
			}
		}

		public void DoCopyProperties (DataControlField newField) {
			base.CopyProperties (newField);
		}

		public DataControlField DoCreateField () {
			return base.CreateField ();
		}

		public string DoFormatDataValue (object dataValue, bool encode) {
			return this.FormatDataValue (dataValue, encode);
		}

		public object DoGetDesignTimeValue () {
			return base.GetDesignTimeValue ();
		}

		public object DoGetValue (Control controlContainer) {
			return base.GetValue (controlContainer);
		}

		public void DoInitializeDataCell (DataControlFieldCell cell, DataControlRowState rowState) {
			base.InitializeDataCell (cell, rowState);
		}

		public void DoOnDataBindField (object sender, EventArgs e) {
			base.OnDataBindField (sender, e);
		}

		public Control GetControl {
			get { return base.Control; }
		}
	}


	[Serializable]
	[TestFixture]
	public class BoundFieldTest
	{
		[Test]
		public void BoundField_DefaultProperty () {
			PokerBoundField bf = new PokerBoundField ();
			Assert.AreEqual ("!", PokerBoundField.ThisExpression, "StaticThisExpression");
			Assert.AreEqual ("", bf.DataField, "DataField");
			Assert.AreEqual ("", bf.DataFormatString, "DataFormatString");
			Assert.AreEqual ("", bf.HeaderText, "HeaderText");
			Assert.AreEqual (true, bf.HtmlEncode, "HtmlEncode");
			Assert.AreEqual ("", bf.NullDisplayText, "NullDisplayText");
			Assert.AreEqual (false, bf.ReadOnly, "ReadOnly");

			//Protected 
			Assert.AreEqual (true, bf.DoSupportsHtmlEncode, "SupportsHtmlEncode");
		}

		[Test]
		public void BoundField_DefaultPropertyNotWorking () {
			PokerBoundField bf = new PokerBoundField ();
			Assert.AreEqual (false, bf.ApplyFormatInEditMode, "ApplyFormatInEditMode");
			Assert.AreEqual (true, bf.ConvertEmptyStringToNull, "ConvertEmptyStringToNull");
		}

		[Test]
		public void BoundField_AssignProperty () {
			PokerBoundField bf = new PokerBoundField ();
			bf.ConvertEmptyStringToNull = false;
			Assert.AreEqual (false, bf.ConvertEmptyStringToNull, "ConvertEmptyStringToNull");
			bf.DataField = "test";
			Assert.AreEqual ("test", bf.DataField, "DataField");
			bf.DataFormatString = "test";
			Assert.AreEqual ("test", bf.DataFormatString, "DataFormatString");
			bf.HeaderText = "test";
			Assert.AreEqual ("test", bf.HeaderText, "HeaderText");
			bf.HtmlEncode = false;
			Assert.AreEqual (false, bf.HtmlEncode, "HtmlEncode");
			bf.NullDisplayText = "test";
			Assert.AreEqual ("test", bf.NullDisplayText, "NullDisplayText");
			bf.ReadOnly = true;
			Assert.AreEqual (true, bf.ReadOnly, "ReadOnly");
		}

		[Test]
		public void BoundField_AssignPropertyNotWorking () {
			PokerBoundField bf = new PokerBoundField ();
			bf.ApplyFormatInEditMode = true;
			Assert.AreEqual (true, bf.ApplyFormatInEditMode, "ApplyFormatInEditMode");
		}

		[Test]
		public void BoundField_ExtractValuesFromCell () {
			PokerBoundField bf = new PokerBoundField ();
			OrderedDictionary dictionary = new OrderedDictionary ();
			DataControlFieldCell cell = new DataControlFieldCell (null);
			cell.Text = "test";
			bf.ExtractValuesFromCell (dictionary, cell, DataControlRowState.Normal, true);
			Assert.AreEqual (1, dictionary.Count, "ExtractValuesFromCellCount");
			Assert.AreEqual ("test", dictionary [0].ToString (), "ExtractValuesFromCellValue");
		}

		[Test]
		public void BoundField_Initialize () {
			// This method initilize to private fields in a base class DataControlField 
			// Always return false
			PokerBoundField bf = new PokerBoundField ();
			Control control = new Control ();
			control.ID = "test";
			bool res = bf.Initialize (true, control);
			// Assert.AreEqual (false, res, "InitializeResult");
			Assert.AreEqual ("test", bf.GetControl.ID, "InitializeControl");
		}

		[Test]
		public void BoundField_InitializeCell () {
			PokerBoundField bf = new PokerBoundField ();
			// Header text
			DataControlFieldCell cell = new DataControlFieldCell (null);
			bf.HeaderText = "headertext";

			bf.InitializeCell (cell, DataControlCellType.Header, DataControlRowState.Edit, 1);
			Assert.AreEqual ("headertext", cell.Text, "InitializeCellHeaderText");
			// Empty header text
			bf.HeaderText = "";
			bf.InitializeCell (cell, DataControlCellType.Header, DataControlRowState.Edit, 1);
			Assert.AreEqual ("&nbsp;", cell.Text, "InitializeCellEmpty");

			bf.HeaderText = "headertext";
			// Header image url not empty
			bf.HeaderImageUrl = "headerurl";
			bf.InitializeCell (cell, DataControlCellType.Header, DataControlRowState.Edit, 1);
			if (cell.Controls [0] is Image) {
				Image image = (Image) cell.Controls [0];
				Assert.AreEqual ("headerurl", image.ImageUrl, "InitializeCellHeaderImageUrl");
			}
			else {
				Assert.Fail ("Header Image dos not created");
			}

			// Footer empty
			bf.FooterText = "footertext";
			bf.InitializeCell (cell, DataControlCellType.Footer, DataControlRowState.Edit, 1);
			Assert.AreEqual ("footertext", cell.Text, "InitializeCellFooterText");
			bf.FooterText = "";
			bf.InitializeCell (cell, DataControlCellType.Footer, DataControlRowState.Edit, 1);
			Assert.AreEqual ("&nbsp;", cell.Text, "InitializeCellFooterEmpty");
		}

		[Test]
		public void BoundField_ValidateSupportsCallback () {
			//This method has been implemented as an empty method    	
		}

		[Test]
		public void BoundField_SortExpression () {
			// Sorting enable = true , link button must be created
			PokerBoundField bf = new PokerBoundField ();
			// Header text
			DataControlFieldCell cell = new DataControlFieldCell (null);

			bf.HeaderImageUrl = "";
			bf.SortExpression = "a";
			bf.HeaderText = "sortbutton";
			bf.Initialize (true, new Control ());    // _base._sortingenable set to true
			bf.InitializeCell (cell, DataControlCellType.Header, DataControlRowState.Edit, 1);
			if (cell.Controls [0] is Button) { // mono
				Button lb = (Button) cell.Controls [0];
				Assert.AreEqual ("Sort", lb.CommandName, "InitializeCellHeaderSortButtonCommand");
				Assert.AreEqual ("a", lb.CommandArgument, "InitializeCellHeaderSortButtonArgument");
				Assert.AreEqual ("sortbutton", lb.Text, "InitializeCellHeaderSortButtonText");

			}
			else if (cell.Controls [0] is LinkButton) { // .net
				LinkButton lb = (LinkButton) cell.Controls [0];
				Assert.AreEqual ("Sort", lb.CommandName, "InitializeCellHeaderSortButtonCommand");
				Assert.AreEqual ("a", lb.CommandArgument, "InitializeCellHeaderSortButtonArgument");
				Assert.AreEqual ("sortbutton", lb.Text, "InitializeCellHeaderSortButtonText");

			}
			else {
				Assert.Fail ("Sort button does not created");
			}
		}


		[Test]
		public void BoundField_CopyProperties () {
			PokerBoundField bf = new PokerBoundField ();
			BoundField copy = new BoundField ();
			// Look not working property
			// bf.ApplyFormatInEditMode = true;
			bf.ConvertEmptyStringToNull = true;
			bf.DataField = "test";
			bf.DataFormatString = "test";
			bf.HtmlEncode = true;
			bf.NullDisplayText = "test";
			bf.ReadOnly = true;
			bf.DoCopyProperties (copy);
			// Look not working property
			// Assert.AreEqual (true, copy.ApplyFormatInEditMode, "ApplyFormatInEditMode");
			Assert.AreEqual (true, copy.ConvertEmptyStringToNull, "ConvertEmptyStringToNull");
			Assert.AreEqual ("test", copy.DataField, "DataField");
			Assert.AreEqual ("test", copy.DataFormatString, "DataFormatString");
			Assert.AreEqual (true, copy.HtmlEncode, "HtmlEncode");
			Assert.AreEqual ("test", copy.NullDisplayText, "NullDisplayText");
			Assert.AreEqual (true, copy.ReadOnly, "ReadOnly");
		}

		[Test]
		public void BoundField_CreateField () {
			PokerBoundField bf = new PokerBoundField ();
			BoundField newfield = (BoundField) bf.DoCreateField ();
			Assert.IsNotNull (newfield, "CreateField");
		}

		[Test]
		public void BoundField_FormatDataValue () {
			string result;
			PokerBoundField bf = new PokerBoundField ();

			bf.NullDisplayText = "NullDisplayText";
			result = bf.DoFormatDataValue (null, false);
			Assert.AreEqual ("NullDisplayText", result, "FormatDataValueNullDataValue");

			result = bf.DoFormatDataValue ("test", true);
			Assert.AreEqual ("test", result, "FormatDataValueTextDataValue");

			result = bf.DoFormatDataValue ("", true);
			Assert.AreEqual ("NullDisplayText", result, "FormatEmptyDataValue");

			bf.DataFormatString = "-{0,8:G}-";
			result = bf.DoFormatDataValue (10, false);
			Assert.AreEqual ("-      10-", result, "FormatDataValueWithFormat");
		}

		[Test]
		public void BoundField_GetDesignTimeValue () {
			string result;
			PokerBoundField bf = new PokerBoundField ();
			result = (string) bf.DoGetDesignTimeValue ();
			Assert.AreEqual ("Databound", result, "GetDesignTimeValue");
		}

		[Test]
		[ExpectedException(typeof(HttpException), ExpectedMessage="A data item was not found in the container. The container must either implement IDataItemContainer, or have a property named DataItem.")]
		public void BoundField_GetValueNull () {
			PokerBoundField bf = new PokerBoundField ();
			SimpleSpreadsheetRow ds = new SimpleSpreadsheetRow (0, null);
			bf.DataField = PokerBoundField.ThisExpression;
			string result = (string) bf.DoGetValue (ds);
		}

		[Test]
		public void BoundField_GetValue () {
			PokerBoundField bf = new PokerBoundField ();
			SimpleSpreadsheetRow ds = new SimpleSpreadsheetRow (0, "test");
			bf.DataField = PokerBoundField.ThisExpression;
			string result = (string) bf.DoGetValue (ds);
			Assert.AreEqual ("test", result, "GetValueFromIDataItemContainer");
		}

		[Test]
		public void BoundField_GetValueDataItem () {
			PokerBoundField bf = new PokerBoundField ();
			ControlWithDataItem ds = new ControlWithDataItem ("test");
			bf.DataField = PokerBoundField.ThisExpression;
			string result = (string) bf.DoGetValue (ds);
			Assert.AreEqual ("test", result, "GetValueFromIDataItemContainer");
		}

		[Test]
		public void BoundField_InitializeDataCell () {
			PokerBoundField bf = new PokerBoundField ();
			bf.HeaderText = "headertest";
			DataControlFieldCell cell = new DataControlFieldCell (null);
			DataControlRowState state = DataControlRowState.Edit;
			Assert.AreEqual (0, cell.Controls.Count, "InitializeDataCellControlsBeforeInit");
			bf.DoInitializeDataCell (cell, state);
			Assert.AreEqual (1, cell.Controls.Count, "InitializeDataCellControlsAfterInit");
		}

		[Test]
		[ExpectedException (typeof (HttpException))]
		public void BoundField_OnDataBindFieldExeption () {
			PokerBoundField bf = new PokerBoundField ();
			bf.bindbutoon.DataBind ();

		}

		[Test]
		[ExpectedException (typeof (HttpException))]
		public void BoundField_GetValueExeption () {
			PokerBoundField bf = new PokerBoundField ();
			bf.DoGetValue (null);
		}

		[Test]
		[Category ("NunitWeb")]
		public void BoundField_NullValueRender ()
		{
			string html = new WebTest (PageInvoker.CreateOnLoad (new PageDelegate (BasicRenderTestInit))).Run ();
			string orightml = "<div>\r\n\t<table cellspacing=\"0\" rules=\"all\" border=\"1\" style=\"border-collapse:collapse;\">\r\n\t\t<tr>\r\n\t\t\t<th scope=\"col\">&nbsp;</th><th scope=\"col\">&nbsp;</th>\r\n\t\t</tr><tr>\r\n\t\t\t<td>Norway</td><td>Norway</td>\r\n\t\t</tr><tr>\r\n\t\t\t<td>Sweden</td><td>Sweden</td>\r\n\t\t</tr><tr>\r\n\t\t\t<td>EMPTY</td><td>&nbsp;</td>\r\n\t\t</tr><tr>\r\n\t\t\t<td>Italy</td><td>Italy</td>\r\n\t\t</tr>\r\n\t</table>\r\n</div>";
			html = HtmlDiff.GetControlFromPageHtml (html);
			HtmlDiff.AssertAreEqual (orightml, html, "NullValueRender");
		}

		public static void BasicRenderTestInit (Page p)
		{
			ArrayList myds = new ArrayList ();
			myds.Add (new myds_data ("Norway"));
			myds.Add (new myds_data ("Sweden"));
			myds.Add (new myds_data (""));
			myds.Add (new myds_data ("Italy"));

			BoundField bf = new BoundField ();
			bf.DataField = "Field1";
			bf.NullDisplayText = "EMPTY";

			BoundField bf2 = new BoundField ();
			bf2.DataField = "Field1";

			GridView GridView1 = new GridView();
			GridView1.AutoGenerateColumns = false;
			GridView1.Columns.Add (bf);
			GridView1.Columns.Add (bf2);
			GridView1.DataSource = myds;
			GridView1.DataBind ();

			LiteralControl lcb = new LiteralControl (HtmlDiff.BEGIN_TAG);
			LiteralControl lce = new LiteralControl (HtmlDiff.END_TAG);

			p.Form.Controls.Add (lcb);
			p.Form.Controls.Add (GridView1);
			p.Form.Controls.Add (lce);
		}

		class myds_data
		{
			string _s = "";
			public myds_data (string s)
			{
				_s = s;
			}

			public string Field1
			{
				get { return _s; }
			}
		}

		
		class ControlWithDataItem : Control
		{
			readonly object _data;
			public ControlWithDataItem (object data) {
				_data = data;
			}

			public object DataItem {
				get {
					return _data;
				}
			}
		}

		public class SimpleSpreadsheetRow : TableRow, IDataItemContainer
		{
			private object data;
			private int _itemIndex;

			public SimpleSpreadsheetRow (int itemIndex, object o) {
				data = o;
				_itemIndex = itemIndex;
			}

			public virtual object Data {
				get {
					return data;
				}
			}

			object IDataItemContainer.DataItem {
				get {
					return Data;
				}
			}

			int IDataItemContainer.DataItemIndex {
				get {
					return _itemIndex;
				}
			}

			int IDataItemContainer.DisplayIndex {
				get {
					return _itemIndex;
				}
			}
		}
	}
}
#endif