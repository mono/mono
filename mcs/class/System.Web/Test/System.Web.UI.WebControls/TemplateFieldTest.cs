//
// Tests for System.Web.UI.WebControls.TemplateFieldTest.cs
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
	class PokerTemplateField : TemplateField
	{
		// View state Stuff
		public PokerTemplateField ()
			: base () {
			TrackViewState ();
		}

		public object SaveState () {
			return SaveViewState ();
		}

		public void LoadState (object o) {
			LoadViewState (o);
		}

		public StateBag StateBag {
			get { return base.ViewState; }
		}

		public Control control () {
			return base.Control;
		}

		public void DoCopyProperties (DataControlField newField) {
			base.CopyProperties (newField);
		}

		public DataControlField DoCreateField () {
			return base.CreateField ();
		}
	}

	[TestFixture]
	public class TemplateFieldTest
	{
		[Test]
		public void TemplateField_DefaultProperty () {
			TemplateField field = new TemplateField ();
			Assert.AreEqual (null, field.AlternatingItemTemplate, "AlternatingItemTemplate");
			Assert.AreEqual (true, field.ConvertEmptyStringToNull, "ConvertEmptyStringToNull");
			Assert.AreEqual (null, field.EditItemTemplate, "EditItemTemplate");
			Assert.AreEqual (null, field.FooterTemplate, "FooterTemplate");
			Assert.AreEqual (null, field.HeaderTemplate, "HeaderTemplate");
			Assert.AreEqual (null, field.InsertItemTemplate, "InsertItemTemplate");
			Assert.AreEqual (null, field.ItemTemplate, "ItemTemplate");
		}

		[Test]
		public void TemplateField_AssignProperty () {
			PokerTemplateField field = new PokerTemplateField ();
			field.AlternatingItemTemplate = new Ibutton ();
			Assert.IsNotNull (field.AlternatingItemTemplate, "AlternatingItemTemplateAssigned");
			Assert.AreEqual (typeof (Ibutton), field.AlternatingItemTemplate.GetType (), "AlternatingItemTemplateType");
			field.ConvertEmptyStringToNull = false;
			Assert.AreEqual (false, field.ConvertEmptyStringToNull, "ConvertEmptyStringToNull");
			field.EditItemTemplate = new IImage ();
			Assert.IsNotNull (field.EditItemTemplate, "EditItemTemplateAssigning");
			Assert.AreEqual (typeof (IImage), field.EditItemTemplate.GetType (), "EditItemTemplateType");
			field.FooterTemplate = new Ibutton ();
			Assert.IsNotNull (field.FooterTemplate, "FooterTemplateAssigning");
			Assert.AreEqual (typeof (Ibutton), field.FooterTemplate.GetType (), "FooterTemplateType");
			field.HeaderTemplate = new IImage ();
			Assert.IsNotNull (field.HeaderTemplate, "HeaderTemplateAssigning");
			Assert.AreEqual (typeof (IImage), field.HeaderTemplate.GetType (), "HeaderTemplateType");
			field.InsertItemTemplate = new Ibutton ();
			Assert.IsNotNull (field.InsertItemTemplate, "InsertItemTemplateAssigning");
			Assert.AreEqual (typeof (Ibutton), field.InsertItemTemplate.GetType (), "InsertItemTemplateType");
			field.ItemTemplate = new IImage ();
			Assert.IsNotNull (field.ItemTemplate, "ItemTemplateAssigning");
			Assert.AreEqual (typeof (IImage), field.ItemTemplate.GetType (), "ItemTemplateType");
		}

		[Test]
		public void TemplateField_ExtractValuesFromCell () {
			TemplateField field = new TemplateField ();
			OrderedDictionary dictionrary = new OrderedDictionary ();
			DataControlFieldCell cell = new DataControlFieldCell (null);
			field.ExtractValuesFromCell (dictionrary, cell, DataControlRowState.Normal, true);
			Assert.AreEqual (0, dictionrary.Count, "ExtractValuesFromCellNoTemplates");
			// This is testing only base functionality and flow with no exceptions
			// The rest functionality will tested on integration test
		}

		[Test]
		[ExpectedException(typeof(NullReferenceException))]
		public void TemplateField_InitializeCell_Null () {
			PokerTemplateField field = new PokerTemplateField ();
			field.InitializeCell (null, DataControlCellType.DataCell, DataControlRowState.Normal, 0);
		}

		[Test]
		public void TemplateField_InitializeCell () {
			PokerTemplateField field = new PokerTemplateField ();
			DataControlFieldCell cell = new DataControlFieldCell (null);
			field.InitializeCell (cell, DataControlCellType.DataCell, DataControlRowState.Normal, 0);
			Assert.AreEqual ("&nbsp;", cell.Text, "InitializeCellEmpty");
			field.ItemTemplate = new IImage ("test");
			field.InitializeCell (cell, DataControlCellType.DataCell, DataControlRowState.Normal, 0);
			Assert.AreEqual ("", cell.Text, "InitializeCellWithItemTemplate");
			Assert.AreEqual (1, cell.Controls.Count, "InitializeCellWithItemTemplate#1");
			Assert.AreEqual ("test", ((IImage) cell.Controls [0]).ImageUrl, "InitializeCellWithItemTemplate#2");
		}

		[Test]
		public void TemplateField_FooterTemplate () {
			PokerTemplateField field = new PokerTemplateField ();
			DataControlFieldCell cell = new DataControlFieldCell (null);
			cell.Text = "text";
			field.InitializeCell (cell, DataControlCellType.Footer, DataControlRowState.Normal, 0);
			Assert.AreEqual ("&nbsp;", cell.Text, "#1");
			Assert.AreEqual (0, cell.Controls.Count, "#2");
			field.FooterTemplate = new Ibutton ("test");
			field.InitializeCell (cell, DataControlCellType.Footer, DataControlRowState.Normal, 0);
			Assert.AreEqual ("", cell.Text, "#3");
			Assert.AreEqual (1, cell.Controls.Count, "#4");
			Assert.AreEqual ("test", ((Ibutton) cell.Controls [0]).Text, "#5");
		}


		[Test]
		public void TemplateField_HeaderTemplate () {
			PokerTemplateField field = new PokerTemplateField ();
			DataControlFieldCell cell = new DataControlFieldCell (null);
			cell.Text = "text";
			field.InitializeCell (cell, DataControlCellType.Header, DataControlRowState.Normal, 0);
			Assert.AreEqual ("&nbsp;", cell.Text, "#1");
			Assert.AreEqual (0, cell.Controls.Count, "#2");
			field.HeaderTemplate = new Ibutton ("test");
			field.InitializeCell (cell, DataControlCellType.Header, DataControlRowState.Normal, 0);
			Assert.AreEqual ("", cell.Text, "#3");
			Assert.AreEqual (1, cell.Controls.Count, "#4");
			Assert.AreEqual ("test", ((Ibutton) cell.Controls [0]).Text, "#5");
		}

		[Test]
		public void TemplateField_EditItemTemplate () {
			PokerTemplateField field = new PokerTemplateField ();
			DataControlFieldCell cell = new DataControlFieldCell (null);
			field.ItemTemplate = new Ibutton ("test");
			field.InitializeCell (cell, DataControlCellType.DataCell, DataControlRowState.Edit, 0);
			Assert.AreEqual ("test", ((Ibutton) cell.Controls [0]).Text, "#1");
			cell = new DataControlFieldCell (null);
			field.EditItemTemplate = new Ibutton ("edit");
			field.InitializeCell (cell, DataControlCellType.DataCell, DataControlRowState.Edit, 0);
			Assert.AreEqual ("edit", ((Ibutton) cell.Controls [0]).Text, "#2");
			cell = new DataControlFieldCell (null);
			field.InitializeCell (cell, DataControlCellType.DataCell, DataControlRowState.Alternate, 0);
			Assert.AreEqual ("test", ((Ibutton) cell.Controls [0]).Text, "#3");
			cell = new DataControlFieldCell (null);
			field.ItemTemplate = null;
			field.InitializeCell (cell, DataControlCellType.DataCell, DataControlRowState.Alternate, 0);
			Assert.IsTrue (cell.Controls.Count == 0, "#4");
			Assert.AreEqual ("&nbsp;", cell.Text, "#5");
		}

		[Test]
		public void TemplateField_InsertItemTemplate () {
			PokerTemplateField field = new PokerTemplateField ();
			DataControlFieldCell cell = new DataControlFieldCell (null);
			field.ItemTemplate = new Ibutton ("test");
			field.InitializeCell (cell, DataControlCellType.DataCell, DataControlRowState.Insert, 0);
			Assert.AreEqual ("test", ((Ibutton) cell.Controls [0]).Text, "#1");
			cell = new DataControlFieldCell (null);
			field.InsertItemTemplate = new Ibutton ("insert");
			field.InitializeCell (cell, DataControlCellType.DataCell, DataControlRowState.Insert, 0);
			Assert.AreEqual ("insert", ((Ibutton) cell.Controls [0]).Text, "#2");
			cell = new DataControlFieldCell (null);
			field.InitializeCell (cell, DataControlCellType.DataCell, DataControlRowState.Alternate, 0);
			Assert.AreEqual ("test", ((Ibutton) cell.Controls [0]).Text, "#3");
			cell = new DataControlFieldCell (null);
			field.ItemTemplate = null;
			field.InitializeCell (cell, DataControlCellType.DataCell, DataControlRowState.Edit, 0);
			Assert.IsTrue (cell.Controls.Count == 0, "#4");
			Assert.AreEqual ("&nbsp;", cell.Text, "#5");
		}

		[Test]
		public void TemplateField_AlternatingItemTemplate () {
			PokerTemplateField field = new PokerTemplateField ();
			DataControlFieldCell cell = new DataControlFieldCell (null);
			field.ItemTemplate = new Ibutton ("test");
			field.InitializeCell (cell, DataControlCellType.DataCell, DataControlRowState.Alternate, 0);
			Assert.AreEqual ("test", ((Ibutton) cell.Controls [0]).Text, "#1");
			cell = new DataControlFieldCell (null);
			field.AlternatingItemTemplate = new Ibutton ("Alternate");
			field.InitializeCell (cell, DataControlCellType.DataCell, DataControlRowState.Alternate, 0);
			Assert.AreEqual ("Alternate", ((Ibutton) cell.Controls [0]).Text, "#2");
			cell = new DataControlFieldCell (null);
			field.InitializeCell (cell, DataControlCellType.DataCell, DataControlRowState.Insert, 0);
			Assert.AreEqual ("test", ((Ibutton) cell.Controls [0]).Text, "#3");
			cell = new DataControlFieldCell (null);
			field.ItemTemplate = null;
			field.InitializeCell (cell, DataControlCellType.DataCell, DataControlRowState.Edit, 0);
			Assert.IsTrue (cell.Controls.Count == 0, "#4");
			Assert.AreEqual ("&nbsp;", cell.Text, "#5");
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void TemplateField_ValidateSupportsCallbackException () {
			TemplateField field = new TemplateField ();
			field.Initialize (false, new Control ());
			field.ValidateSupportsCallback ();
		}

		[Test]
		public void TemplateField_Copy () {
			PokerTemplateField field = new PokerTemplateField ();
			TemplateField copy = new TemplateField ();
			field.ConvertEmptyStringToNull = true;
			field.AlternatingItemTemplate = new Ibutton ();
			field.ItemTemplate = new Ibutton ();
			field.FooterTemplate = new Ibutton ();
			field.EditItemTemplate = new Ibutton ();
			field.HeaderTemplate = new Ibutton ();
			field.InsertItemTemplate = new Ibutton ();
			field.DoCopyProperties (copy);
			Assert.AreEqual (true, copy.ConvertEmptyStringToNull, "ConvertEmptyStringToNull");
			Assert.IsNotNull (copy.AlternatingItemTemplate, "AlternatingItemTemplate");
			Assert.IsNotNull (copy.ItemTemplate, "ItemTemplate");
			Assert.IsNotNull (copy.FooterTemplate, "FooterTemplate");
			Assert.IsNotNull (copy.EditItemTemplate, "EditItemTemplate");
			Assert.IsNotNull (copy.HeaderTemplate, "HeaderTemplate");
			Assert.IsNotNull (copy.InsertItemTemplate, "InsertItemTemplate");
		}

		[Test]
		public void TemplateField_CreateField () {
			PokerTemplateField field = new PokerTemplateField ();
			DataControlField newfield = field.DoCreateField ();
			if (!(newfield is TemplateField)) {
				Assert.Fail ("New TemplateField was not created");
			}
		}


		// A simple Template class to wrap an image.
		public class IImage : Image, ITemplate
		{
			public IImage ()
				: base () {

			}
			public IImage (string text)
				: base () {
				this.ImageUrl = text;
			}

			public void InstantiateIn (Control container) {
				container.Controls.Add (this);
			}

		}

		private class Ibutton : Button, ITemplate
		{
			public Ibutton ()
				: base () {
			}

			public Ibutton (string text)
				: base () {
				this.Text = text;
			}

			void ITemplate.InstantiateIn (Control container) {
				container.Controls.Add (this);
			}
		}
	}
}
#endif
