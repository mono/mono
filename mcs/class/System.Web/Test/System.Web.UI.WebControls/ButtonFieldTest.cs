//
// Tests for System.Web.UI.WebControls.ButtonFieldTest.cs
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
using NUnit.Framework;
using System.Data;

namespace MonoTests.System.Web.UI.WebControls
{
	class PokerButtonField : ButtonField
	{
		// View state Stuff
		public PokerButtonField ()
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

		public void DoCopyProperties (DataControlField newField)
		{
			base.CopyProperties (newField);
		}

		public DataControlField DoCreateField ()
		{
			return base.CreateField ();
		}

		public string DoFormatDataTextValue (object dataTextValue)
		{
			return base.FormatDataTextValue (dataTextValue);
		}

		public Control GetControl
		{
			get { return base.Control; }
		} 
	}

	[TestFixture]
	public class ButtonFieldTest
	{
		[Test]
		public void ButtonField_DefaultProperty ()
		{
			PokerButtonField button = new PokerButtonField ();
			Assert.AreEqual ("", button.CommandName, "CommandName");
			Assert.AreEqual ("", button.DataTextField, "DataTextField");
			Assert.AreEqual ("", button.DataTextFormatString, "DataTextFormatString");
			Assert.AreEqual ("", button.ImageUrl, "ImageUrl");
			Assert.AreEqual ("", button.Text, "Text");
		}

		[Test]
		public void ButtonField_AssignProperty ()
		{
			PokerButtonField button = new PokerButtonField ();
			button.CommandName = "test";
			Assert.AreEqual ("test", button.CommandName, "CommandName");
			button.DataTextField = "test";
			Assert.AreEqual ("test", button.DataTextField, "DataTextField");
			button.DataTextFormatString = "test";
			Assert.AreEqual ("test", button.DataTextFormatString, "DataTextFormatString");
			button.ImageUrl = "test";
			Assert.AreEqual ("test", button.ImageUrl, "ImageUrl");
			button.Text = "test";
			Assert.AreEqual ("test", button.Text, "Text");
		}

		[Test]
		public void ButtonField_Initialize ()
		{
			Control control = new Control ();
			control.ID = "test";
			PokerButtonField button = new PokerButtonField ();
			bool result = button.Initialize (true, control);
			Assert.AreEqual (false, result, "Initialize");
			Assert.AreEqual ("test", button.GetControl.ID, "InitializeControl");
		}

		[Test]
		public void ButtonField_InitializeCell ()
		{
			ButtonField field = new ButtonField();
			field.Text = "FieldText";
			field.HeaderText = "HeaderText";
			field.FooterText = "FooterText";
			field.CommandName = "Commandname";
			DataControlFieldCell cell = new DataControlFieldCell (null);
			field.InitializeCell (cell, DataControlCellType.Header, DataControlRowState.Normal, 0);
			Assert.AreEqual ("HeaderText", cell.Text, "HeaderText");
			field.InitializeCell (cell, DataControlCellType.Footer, DataControlRowState.Normal, 0);
			Assert.AreEqual ("FooterText", cell.Text, "FooterText");
			Assert.AreEqual (0, cell.Controls.Count, "BeforeInitilizeDataField");
			field.InitializeCell (cell, DataControlCellType.DataCell, DataControlRowState.Normal, 0);
			Assert.AreEqual (1, cell.Controls.Count, "AfterInitilizeDataField");
			Assert.AreEqual ("FieldText",((IButtonControl)cell.Controls[0]).Text ,"FieldText" );
			Assert.AreEqual ("Commandname", ((IButtonControl) cell.Controls[0]).CommandName , "Commandname");
			Assert.AreEqual ("0", ((IButtonControl) cell.Controls[0]).CommandArgument, "CommandArgument");
			//Assert.AreEqual ("System.Web.UI.WebControls.DataControlLinkButton", ((IButtonControl) cell.Controls[0]).GetType ().ToString(), "TypeOfDataControlLinkButton");
			cell.Controls.Clear ();
			field.ButtonType = ButtonType.Image;
			field.InitializeCell (cell, DataControlCellType.DataCell, DataControlRowState.Normal, 0);
			//Assert.AreEqual ("System.Web.UI.WebControls.ImageButton", ((IButtonControl) cell.Controls[0]).GetType ().ToString (), "TypeOfDataControlLinkButton");
			cell.Controls.Clear ();
			field.ButtonType = ButtonType.Button;
			field.InitializeCell (cell, DataControlCellType.DataCell, DataControlRowState.Normal, 0);
			//Assert.AreEqual ("System.Web.UI.WebControls.Button", ((IButtonControl) cell.Controls[0]).GetType ().ToString (), "TypeOfDataControlLinkButton");
		}

		[Test]
		public void ButtonField_ValidateSupportsCallback ()
		{
			//This method has been implemented as an empty method 
		}

		[Test]
		public void ButtonField_CopyProperties ()
		{
			PokerButtonField button = new PokerButtonField ();
			ButtonField copy = new ButtonField ();
			button.CommandName = "CommandName";
			button.DataTextField = "DataTextField";
			button.DataTextFormatString = "DataTextFormatString";
			button.ImageUrl = "ImageUrl";
			button.Text = "Text";

			button.DoCopyProperties (copy);
			Assert.AreEqual ("CommandName", copy.CommandName, "CommandName");
			Assert.AreEqual ("DataTextField", copy.DataTextField, "DataTextField");
			Assert.AreEqual ("DataTextFormatString", copy.DataTextFormatString, "DataTextFormatString");
			Assert.AreEqual ("ImageUrl", copy.ImageUrl, "ImageUrl");
			Assert.AreEqual ("Text", copy.Text, "Text");
		}

		[Test]
		public void ButtonField_CreateField ()
		{
			PokerButtonField button = new PokerButtonField ();
			DataControlField newfield = button.DoCreateField ();
			if (!(newfield is ButtonField)) {
				Assert.Fail ("New buttonfield was not created");
			}

		}

		[Test]
		public void ButtonField_FormatDataTextValue ()
		{
			PokerButtonField button = new PokerButtonField ();
			button.DataTextFormatString = "-{0,8:G}-";
			string result = button.DoFormatDataTextValue(10);
			Assert.AreEqual ("-      10-", result, "FormatDataValueWithFormat");
		}
	}
}
#endif
