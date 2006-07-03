//
// Tests for System.Web.UI.WebControls.ImageFieldTest.cs
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
using System.Collections;
using System.Collections.Specialized;
using NUnit.Framework;
using System.Data;


namespace MonoTests.System.Web.UI.WebControls
{
	class PokerImageField : ImageField
	{
		// View state Stuff
		public PokerImageField ()
			: base ()
		{
			TrackViewState ();
		}

		public object SaveState ()
		{
			return SaveViewState ();
		}

		public void LoadState (object o)
		{
			LoadViewState (o);
		}

		public StateBag StateBag
		{
			get { return base.ViewState; }
		}

		public Control GetControl
		{
			get { return base.Control; }
		}

		public void DoInitializeDataCell (DataControlFieldCell cell, DataControlRowState rowState)
		{
			this.InitializeDataCell (cell, rowState);
		}

		public void DoCopyProperties (DataControlField newField)
		{
			base.CopyProperties (newField);
		}

		public DataControlField DoCreateField ()
		{
			return base.CreateField ();
		}

		public string DoFormatImageUrlValue (object dataValue)
		{
			return base.FormatImageUrlValue (dataValue);
		}

		public string DoGetDesignTimeValue ()
		{
			return base.GetDesignTimeValue ();
		}

		public string DoGetFormattedAlternateText (Control controlContainer)
		{
			return base.GetFormattedAlternateText (controlContainer);
		}
	}

	[TestFixture]
	public class ImageFieldTest
	{
		[Test]
		public void ImageField_DefaultProperty ()
		{
			ImageField field = new ImageField ();
			Assert.AreEqual ("!", ImageField.ThisExpression, "ThisExpression");
			Assert.AreEqual ("", field.AlternateText, "AlternateText ");
			Assert.AreEqual (true, field.ConvertEmptyStringToNull, "ConvertEmptyStringToNull");
			Assert.AreEqual ("", field.DataAlternateTextField, "DataAlternateTextField");
			Assert.AreEqual ("", field.DataImageUrlField, "DataImageUrlField");
			Assert.AreEqual ("", field.DataImageUrlFormatString, "DataImageUrlFormatString");
			Assert.AreEqual ("", field.NullDisplayText, "NullDisplayText");
			Assert.AreEqual ("", field.NullImageUrl, "NullImageUrl");
			Assert.AreEqual (false, field.ReadOnly, "ReadOnly");
		}

		[Test]
		public void ImageField_AssignProperty ()
		{
			ImageField field = new ImageField ();
			field.AlternateText = "test";
			Assert.AreEqual ("test", field.AlternateText, "AlternateText ");
			field.ConvertEmptyStringToNull = false;
			Assert.AreEqual (false, field.ConvertEmptyStringToNull, "ConvertEmptyStringToNull");
			field.DataAlternateTextField = "test";
			Assert.AreEqual ("test", field.DataAlternateTextField, "DataAlternateTextField");
			field.DataImageUrlField = "test";
			Assert.AreEqual ("test", field.DataImageUrlField, "DataImageUrlField");
			field.DataImageUrlFormatString = "test";
			Assert.AreEqual ("test", field.DataImageUrlFormatString, "DataImageUrlFormatString");
			field.NullDisplayText = "test";
			Assert.AreEqual ("test", field.NullDisplayText, "NullDisplayText");
			field.NullImageUrl = "test";
			Assert.AreEqual ("test", field.NullImageUrl, "NullImageUrl");
			field.ReadOnly = true;
			Assert.AreEqual (true, field.ReadOnly, "ReadOnly");
		}

		[Test]
		[Category ("NotWorking")]
		public void ImageField_ExtractValuesFromCell ()
		{
			PokerImageField field = new PokerImageField ();
			OrderedDictionary dictionary = new OrderedDictionary ();
			Image image = new Image ();
			image.ImageUrl = "test";
			DataControlFieldCell cell = new DataControlFieldCell (null);
			cell.Controls.Add (image);
			field.ExtractValuesFromCell (dictionary, cell, DataControlRowState.Normal, true);
			Assert.AreEqual (1, dictionary.Count, "ExtractValuesFromCellCount");
			Assert.AreEqual ("test", dictionary[0].ToString (), "ExtractValuesFromCellValue");
			cell.Controls.Clear ();
			TextBox box = new TextBox ();
			box.Text = "test";
			cell.Controls.Add (box);
			field.ExtractValuesFromCell (dictionary, cell, DataControlRowState.Normal, true);
			Assert.AreEqual (1, dictionary.Count, "ExtractValuesFromCellCount");
			Assert.AreEqual ("test", dictionary[0].ToString (), "ExtractValuesFromCellValue");
		}

		[Test]
		public void ImageField_Initialize ()
		{
			Control control = new Control ();
			control.ID = "test";
			PokerBoundField field = new PokerBoundField ();
			bool result = field.Initialize (true, control);
			Assert.AreEqual (false, result, "Initialize");
			Assert.AreEqual ("test", field.GetControl.ID, "InitializeControl");
		}

		[Test]
		[Category ("NotWorking")]
		public void ImageField_InitializeDataCell()
		{
			PokerImageField field = new PokerImageField ();
			DataControlFieldCell cell = new DataControlFieldCell(null);
		        field.DoInitializeDataCell(cell ,DataControlRowState.Normal);
			Assert.AreEqual (0, cell.Controls.Count, "InitializeDataCellNormalNoData");
			field.DoInitializeDataCell (cell, DataControlRowState.Alternate);
			Assert.AreEqual (0, cell.Controls.Count, "InitializeDataCellAlternateNoData");
			field.DoInitializeDataCell (cell, DataControlRowState.Edit);
			Assert.AreEqual (1, cell.Controls.Count, "InitializeDataCellEditNoData");
			TextBox box = cell.Controls[0] as TextBox;
			if (box == null)
				Assert.Fail ("TextBox does not created on cell initilize");

			cell.Controls.Clear ();
			field.DataImageUrlField = "test";
			field.DoInitializeDataCell (cell, DataControlRowState.Normal);
			Assert.AreEqual (2, cell.Controls.Count, "InitializeDataCellEditWithData");

			Image image = cell.Controls[0] as Image;
			if (image==null)
				Assert.Fail ("Image does not created on cell initilize");

			Label label = cell.Controls[1] as Label;
			if (label == null)
				Assert.Fail ("Label does not created on cell initilize");

			cell.Controls.Clear ();
			field.DoInitializeDataCell (cell, DataControlRowState.Insert);
			Assert.AreEqual (1, cell.Controls.Count, "InitializeDataCellInsertWithData");
			box = cell.Controls[0] as TextBox;
			if (box == null)
				Assert.Fail ("Text does not created on cell initilize Insert RowState");

			cell.Controls.Clear ();
			field.DoInitializeDataCell (cell, DataControlRowState.Selected);
			Assert.AreEqual (2, cell.Controls.Count, "InitializeDataCellSelectedWithData");
		}

		[Test]
		public void ImageField_ValidateSupportsCallback ()
		{
			//This method has been implemented as an empty method    	
		}

		[Test]
		public void ImageField_CopyProperties ()
		{
			PokerImageField field = new PokerImageField ();
			ImageField copy = new ImageField ();
			field.AlternateText = "test";
			field.ConvertEmptyStringToNull = true;
			field.DataAlternateTextField = "test";
			field.DataAlternateTextFormatString = "test";
			field.DataImageUrlField = "test";
			field.DataImageUrlFormatString = "test";
			field.NullDisplayText = "test";
			field.NullImageUrl = "test";
			field.ReadOnly = true;

			field.DoCopyProperties (copy);
			Assert.AreEqual ("test", copy.AlternateText, "AlternateText");
			Assert.AreEqual (true, copy.ConvertEmptyStringToNull, "ConvertEmptyStringToNull");
			Assert.AreEqual ("test", copy.DataAlternateTextField, "DataAlternateTextField");
			Assert.AreEqual ("test", copy.DataImageUrlField, "DataImageUrlField");
			Assert.AreEqual ("test", copy.DataAlternateTextFormatString, "DataAlternateTextFormatString");
			Assert.AreEqual ("test", copy.DataImageUrlField, "DataImageUrlField");
			Assert.AreEqual ("test", copy.DataImageUrlFormatString, "DataImageUrlFormatString");
			Assert.AreEqual ("test", copy.NullDisplayText, "NullDisplayText");
			Assert.AreEqual ("test", copy.NullImageUrl, "NullImageUrl");
			Assert.AreEqual (true, copy.ReadOnly, "ReadOnly");
		}

		[Test]
		public void ImageField_CreateField ()
		{
			PokerImageField field = new PokerImageField ();
			DataControlField newfield = field.DoCreateField ();
			if (!(newfield is ImageField)) {
				Assert.Fail ("New ImageField was not created");
			}
		}

		[Test]
		[Category ("NotWorking")]
		public void ImageField_FormatDataNavigateUrlValue ()
		{
			PokerImageField field = new PokerImageField ();
			string result = field.DoFormatImageUrlValue (null);
			Assert.AreEqual (null, result, "DoFormatImageUrlValueEmpty");
			field.DataImageUrlFormatString = "-{0,8:G}-";
			result = field.DoFormatImageUrlValue (10);
			Assert.AreEqual ("-      10-", result, "FormatImageUrlValueWithData");
		}

		[Test]
		public void ImageField_GetDesignTimeValue ()
		{
			PokerImageField field = new PokerImageField ();
			string result = field.DoGetDesignTimeValue ();
			Assert.AreEqual ("Databound", result, "GetDesignTimeValue");
		}

		[Test]
		public void ImageField_GetFormattedAlternateTextEmptyDs ()
		{
			PokerImageField field = new PokerImageField ();
			field.AlternateText = "test";
			string result = field.DoGetFormattedAlternateText (new Control());
			Assert.AreEqual ("test", result, "GetFormattedAlternateText");
		}

		[Test]
		public void ImageField_GetFormattedAlternateTextWithDs ()
		{
			// This functionality will be tested integration
			// Look GridView Integration aspx
		}

		[Test]
		public void ImageField_OnDataBindField ()
		{
			// This functionality will be tested integration
			// Look GridView Integration aspx
		}
	}
}
#endif