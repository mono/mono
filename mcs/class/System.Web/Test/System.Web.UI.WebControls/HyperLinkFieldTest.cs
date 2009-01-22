//
// Tests for System.Web.UI.WebControls.HyperLinkFieldTest.cs
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
	class PokerHyperLinkField : HyperLinkField
	{
		// View state Stuff
		public PokerHyperLinkField ()
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
		
		public void DoCopyProperties (DataControlField newField)
		{
			base.CopyProperties (newField);
		}

		public DataControlField DoCreateField ()
		{
			return base.CreateField ();
		}

		public string DoFormatDataNavigateUrlValue (object[] dataUrlValues)
		{
			return base.FormatDataNavigateUrlValue (dataUrlValues);
		}

		public string DoFormatDataTextValue (object dataTextValue)
		{
			return base.FormatDataTextValue (dataTextValue);
		}
	}

	[TestFixture]
	public class HyperLinkFieldTest
	{
		[Test]
		public void HyperLinkField_DefaultProperty ()
		{
			HyperLinkField field = new HyperLinkField ();
			Assert.AreEqual (0, ((Array) field.DataNavigateUrlFields).Length, "DataNavigateUrlFields");
			Assert.AreEqual ("", field.DataNavigateUrlFormatString, "DataNavigateUrlFormatString");
			Assert.AreEqual ("", field.DataTextField, "DataTextField");
			Assert.AreEqual ("", field.DataTextFormatString, "DataTextFormatString");
			Assert.AreEqual ("", field.NavigateUrl, "NavigateUrl");
			Assert.AreEqual ("", field.Target, "Target");
			Assert.AreEqual ("", field.Text, "Text");
		}

		[Test]
		public void HyperLinkField_AssignProperty ()
		{
			HyperLinkField field = new HyperLinkField ();
			field.DataNavigateUrlFields = new string[] { "test1", "test2" };
			Assert.AreEqual (2, ((Array) field.DataNavigateUrlFields).Length, "DataNavigateUrlFieldsCount");
			Assert.AreEqual ("test1", field.DataNavigateUrlFields[0], "DataNavigateUrlFields#1");
			Assert.AreEqual ("test2", field.DataNavigateUrlFields[1], "DataNavigateUrlFields#2");
			field.DataNavigateUrlFormatString = "test";
			Assert.AreEqual ("test", field.DataNavigateUrlFormatString, "DataNavigateUrlFormatString");
			field.DataTextField = "test";
			Assert.AreEqual ("test", field.DataTextField, "DataTextField");
			field.DataTextFormatString = "test";
			Assert.AreEqual ("test", field.DataTextFormatString, "DataTextFormatString");
			field.NavigateUrl = "test";
			Assert.AreEqual ("test", field.NavigateUrl, "NavigateUrl");
			field.Target = "test";
			Assert.AreEqual ("test", field.Target, "Target");
			field.Text = "test";
			Assert.AreEqual ("test", field.Text, "Text");
		}

		[Test]
		public void HyperLinkField_Initialize ()
		{
			PokerHyperLinkField field = new PokerHyperLinkField ();
			Control control = new Control ();
			control.ID = "test";
			bool result = field.Initialize (true,control);
			Assert.AreEqual (false, result, "InitializeResult");
			Assert.AreEqual ("test", field.GetControl.ID, "InitializeControl");
		}

		[Test]
		public void HyperLinkField_InitializeCell ()
		{
			HyperLinkField field = new HyperLinkField ();
			DataControlFieldCell cell = new DataControlFieldCell (null);
			field.Text = "Text";
			field.NavigateUrl = "NavigateUrl";
			field.Target = "Target"; 
			field.InitializeCell (cell, DataControlCellType.DataCell, DataControlRowState.Normal, 0);
			Assert.AreEqual(1,cell.Controls.Count , "CellControlsCountAfterCreation");
			if (cell.Controls[0] is HyperLink) {
				Assert.AreEqual ("Text", ((HyperLink) cell.Controls[0]).Text, "HyperLinkText");
				Assert.AreEqual ("NavigateUrl", ((HyperLink) cell.Controls[0]).NavigateUrl, "HyperLinkNavigateUrl");
				Assert.AreEqual ("Target", ((HyperLink) cell.Controls[0]).Target, "Target");
			}
			else
				Assert.Fail ("HyperLink was not been created");
		}

		[Test]
		public void HyperLinkField_ValidateSupportsCallback ()
		{
			//This method has been implemented as an empty method    	
		}

		[Test]
		public void HyperLinkField_CopyProperty ()
		{
			PokerHyperLinkField field = new PokerHyperLinkField ();
			HyperLinkField copy = new HyperLinkField ();
			field.DataNavigateUrlFields = new string[] { "test1", "test2" };
			field.DataNavigateUrlFormatString = "test";
			field.DataTextField = "test";
			field.DataTextFormatString = "test";
			field.NavigateUrl = "test";
			field.Target = "test";
			field.Text = "test";
			field.DoCopyProperties (copy);
			Assert.AreEqual (2, ((Array) copy.DataNavigateUrlFields).Length, "DataNavigateUrlFieldsCount");
			Assert.AreEqual ("test1", copy.DataNavigateUrlFields[0], "DataNavigateUrlFields#1");
			Assert.AreEqual ("test2", copy.DataNavigateUrlFields[1], "DataNavigateUrlFields#2");
			Assert.AreEqual ("test", copy.DataNavigateUrlFormatString, "DataNavigateUrlFormatString ");
			Assert.AreEqual ("test", copy.DataTextField, "DataTextField");
			Assert.AreEqual ("test", copy.DataTextFormatString, "DataTextFormatString");
			Assert.AreEqual ("test", copy.NavigateUrl, "NavigateUrl");
			Assert.AreEqual ("test", copy.Target, "Target");
			Assert.AreEqual ("test", copy.Text, "Text");
		}

		[Test]
		public void HyperLinkField_CreateField ()
		{
			PokerHyperLinkField field = new PokerHyperLinkField ();
			DataControlField newfield = field.DoCreateField ();
			if (!(newfield is HyperLinkField)) {
				Assert.Fail ("New HyperLinkField was not created");
			}
		}

		[Test]
		public void HyperLinkField_FormatDataNavigateUrlValue ()
		{
			PokerHyperLinkField field = new PokerHyperLinkField ();
			string result =  field.DoFormatDataNavigateUrlValue (null);
			Assert.AreEqual ("", result, "FormatDataNavigateUrlValueNoArgs");
			field.DataNavigateUrlFormatString = "-{0,8:G}-";
			object[] ob = new object[] { 10 };
			result = field.DoFormatDataNavigateUrlValue(ob);
			Assert.AreEqual ("-      10-", result, "FormatDataNavigateUrlValue");
		}

		[Test]
		public void HyperLinkField_FormatDataTextValue ()
		{
			PokerHyperLinkField field = new PokerHyperLinkField ();
			string result = field.DoFormatDataTextValue (null);
			Assert.AreEqual ("", result, "FormatDataTextValueNoArgs");
			field.DataTextFormatString = "-{0,8:G}-";
			result = field.DoFormatDataTextValue (10);
			Assert.AreEqual ("-      10-", result, "FormatDataTextValueNoArgs");
		}

		[Test]
		[ExpectedException(typeof(IndexOutOfRangeException))]
		public void HyperLinkField_AssignPropertyExepton ()
		{
			PokerHyperLinkField field = new PokerHyperLinkField ();
			field.DataNavigateUrlFields[0] = "test";
		}
	}
}
#endif
