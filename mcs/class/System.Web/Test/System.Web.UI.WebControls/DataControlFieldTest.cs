//
// Tests for System.Web.UI.WebControls.DataControlFieldTest.cs
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




namespace MonoTests.System.Web.UI.WebControls
{
	[TestFixture]
	public class DataControlFieldTest
	{
		class DerivedDataControlField : DataControlField
		{
			private bool _fieldChanged;
			public bool FieldChanged
			{
				get { return _fieldChanged; }
			}

			protected override DataControlField CreateField ()
			{
				throw new NotImplementedException();
			}
			public Control DoControl ()
			{
				return base.Control;
			}
			public bool DoDesignMode ()
			{
				return base.DesignMode;
			}
			public bool DoIsTrackingViewState ()
			{
				return base.IsTrackingViewState;
			}

			public StateBag StateBag
			{
				get { return base.ViewState; }
			}
			
			public void DoTrackViewState ()
			{
				base.TrackViewState ();
			}
			
			public void DoCopyProperties (DataControlField newField)
			{
				base.CopyProperties (newField);
			}
			public DataControlField DoCloneField ()
			{
				return	base.CloneField ();
			}

			protected override void OnFieldChanged ()
			{
				base.OnFieldChanged ();
				_fieldChanged = true;
			}

			public object DoSaveViewState ()
			{
				return base.SaveViewState ();
			}

			public void DoLoadViewState (object savedState)
			{
				base.LoadViewState (savedState);
			}
		}

		[Test]
		public void DataControlField_DefaultProperty ()
		{
			DerivedDataControlField field = new DerivedDataControlField ();
			Assert.AreEqual ("", field.AccessibleHeaderText, "AccessibleHeaderText");
			Assert.AreEqual ("System.Web.UI.WebControls.Style", field.ControlStyle.ToString (), "ControlStyle");
			Assert.AreEqual ("System.Web.UI.WebControls.TableItemStyle", field.FooterStyle.ToString (), "FooterStyle");
			Assert.AreEqual ("", field.FooterText, "FooterText");
			Assert.AreEqual ("", field.HeaderImageUrl, "HeaderImageUrl");
			Assert.AreEqual ("System.Web.UI.WebControls.TableItemStyle", field.HeaderStyle.ToString (), "HeaderStyle");
			Assert.AreEqual ("", field.HeaderText, "HeaderText");
			Assert.AreEqual (true, field.InsertVisible, "InsertVisible");
			Assert.AreEqual ("System.Web.UI.WebControls.TableItemStyle", field.ItemStyle.ToString (), "ItemStyle");
			Assert.AreEqual (true, field.ShowHeader, "ShowHeader");
			Assert.AreEqual ("", field.SortExpression, "SortExpression");
			Assert.AreEqual (true, field.Visible, "Visible");

			//protected properties
			Assert.AreEqual (null, field.DoControl (), "Control");
			Assert.AreEqual (false, field.DoDesignMode (), "DesignMode");
			Assert.AreEqual (false, field.DoIsTrackingViewState (), "IsTrackingViewState");
			Assert.AreEqual ("System.Web.UI.StateBag", field.StateBag.ToString (), "StateBag");
		}

		[Test]
		public void DataControlField_AssignProperty ()
		{
			DerivedDataControlField field = new DerivedDataControlField ();
			field.AccessibleHeaderText = "test";
			Assert.AreEqual ("test", field.AccessibleHeaderText, "AccessibleHeaderText");
			field.ControlStyle.BackColor = Color.Red;
			Assert.AreEqual (Color.Red, field.ControlStyle.BackColor, "ControlStyle");
			field.FooterStyle.BackColor = Color.Red;
			Assert.AreEqual (Color.Red, field.FooterStyle.BackColor, "FooterStyle");
			field.FooterText = "test";
			Assert.AreEqual ("test", field.FooterText, "FooterText");
			field.HeaderImageUrl = "test";
			Assert.AreEqual ("test", field.HeaderImageUrl, "HeaderImageUrl");
			field.HeaderStyle.BackColor = Color.Red;
			Assert.AreEqual (Color.Red, field.HeaderStyle.BackColor, "HeaderStyle");
			field.HeaderText = "test";
			Assert.AreEqual ("test", field.HeaderText, "HeaderText");
			field.ItemStyle.BackColor = Color.Red;
			Assert.AreEqual (Color.Red, field.ItemStyle.BackColor, "ItemStyle");
			field.ShowHeader = false;
			Assert.AreEqual (false, field.ShowHeader, "ShowHeader");
			field.SortExpression = "test";
			Assert.AreEqual ("test", field.SortExpression, "SortExpression");
			field.Visible = false;
			Assert.AreEqual (false, field.Visible, "Visible");
		}

		[Test]
		public void DataControlField_Initilize ()
		{
			DerivedDataControlField field = new DerivedDataControlField ();
			bool res = field.Initialize (false, new Control ());
			Assert.AreEqual (false, res, "Initilize");
		}

		[Test]
		public void DataControlField_InitilizeCell ()
		{
			DerivedDataControlField field = new DerivedDataControlField ();
			field.HeaderText = "test";
			field.HeaderStyle.BackColor = Color.Red;
			field.HeaderImageUrl = "test";
			DataControlFieldCell cell = new DataControlFieldCell (field);
			field.InitializeCell (cell, DataControlCellType.Header, DataControlRowState.Normal, 1);
			Assert.AreEqual ("test", cell.ContainingField.HeaderText, "HeaderText");
			Assert.AreEqual ("test", cell.ContainingField.HeaderImageUrl, "HeaderImageUrl");
			Assert.AreEqual (Color.Red, cell.ContainingField.HeaderStyle.BackColor, "BackColor");
		}

		[Test]
		public void DataControlField_CopyProperties ()
		{
			DerivedDataControlField field = new DerivedDataControlField ();
			DerivedDataControlField newField = new DerivedDataControlField ();

			field.AccessibleHeaderText = "test";
			field.ControlStyle.BackColor = Color.Red;
			field.FooterStyle.BackColor = Color.Red;
			field.HeaderStyle.BackColor = Color.Red;
			field.ItemStyle.BackColor = Color.Red;
			field.FooterText = "test";
			field.HeaderImageUrl = "test";
			field.HeaderText = "test";
			field.InsertVisible = false;
			field.ShowHeader = false;
			field.SortExpression = "test";
			field.Visible = false;
						
			field.DoCopyProperties (newField);
			
			Assert.AreEqual ("test", newField.AccessibleHeaderText, "AccessibleHeaderText");
			Assert.AreEqual (Color.Red, newField.ControlStyle.BackColor, "ControlStyle");
			Assert.AreEqual (Color.Red, newField.FooterStyle.BackColor, "FooterStyle");
			Assert.AreEqual (Color.Red, newField.HeaderStyle.BackColor, "HeaderStyle");
			Assert.AreEqual (Color.Red, newField.ItemStyle.BackColor, "ItemStyle");
			Assert.AreEqual ("test", newField.FooterText, "FooterText"); 
			Assert.AreEqual ("test", newField.HeaderImageUrl,"HeaderImageUrl");
			Assert.AreEqual ("test", newField.HeaderText, "HeaderText ");
			Assert.AreEqual (false, newField.InsertVisible, "InsertVisible");
			Assert.AreEqual (false, newField.ShowHeader, "ShowHeader");
			Assert.AreEqual ("test", newField.SortExpression, "SortExpression");
			Assert.AreEqual (false, newField.Visible, "Visible"); 
		}

		[Test]
		public void DataControlField_Events ()
		{
			DerivedDataControlField field = new DerivedDataControlField ();
			Assert.AreEqual (false, field.FieldChanged, "BeforeChangingProperty");
			field.FooterText = "test";
			Assert.AreEqual (true, field.FieldChanged, "AfterChangingProperty");
		}

		[Test]
		public void DataControlField_ViewState ()
		{
			DerivedDataControlField field = new DerivedDataControlField ();
			DerivedDataControlField newField = new DerivedDataControlField ();
			field.DoTrackViewState ();
			field.FooterStyle.BackColor = Color.Red;
			field.ItemStyle.BackColor = Color.Red;
			field.HeaderStyle.BackColor = Color.Red;
			object state = field.DoSaveViewState();
			newField.DoLoadViewState (state);
			Assert.AreEqual (Color.Red, newField.HeaderStyle.BackColor, "HeaderStyle");
			Assert.AreEqual (Color.Red, newField.ItemStyle.BackColor, "ItemStyle");
			Assert.AreEqual (Color.Red, newField.FooterStyle.BackColor, "FooterStyle");
		}

		[Test]
		[ExpectedException (typeof(NotImplementedException))]
		public void DataControlField_CloneField ()
		{
			DerivedDataControlField field = new DerivedDataControlField ();
			DerivedDataControlField newField = new DerivedDataControlField ();
			newField = (DerivedDataControlField)field.DoCloneField ();
		}
	}
}
#endif
