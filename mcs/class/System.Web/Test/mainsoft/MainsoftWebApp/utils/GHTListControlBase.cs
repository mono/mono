//
// Authors:
//   Rafael Mizrahi   <rafim@mainsoft.com>
//   Erez Lotan       <erezl@mainsoft.com>
//   Vladimir Krasnov <vladimirk@mainsoft.com>
//   
// 
// Copyright (c) 2002-2005 Mainsoft Corporation.
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
using System.ComponentModel;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using GHTWebControls;
using System.Drawing;
using System.Data;
 
namespace GHTTests
{
	/// <summary>
	/// Summary description for GHTListControlBase.
	/// </summary>
	public class GHTListControlBase : GHTBaseWeb
	{
		#region "Tests"
		protected void ListControl_AutoPostBack(Type ctrlType)
		{
			#region "Setting to true"
			GHTListContorlSubTestBegin(ctrlType, "AutoPostBack = True");
			try
			{
				m_lcToTest.AutoPostBack = true;
			}
			catch (Exception ex)
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}
			GHTSubTestEnd();
			#endregion
			#region "Getting true:"
			GHTListContorlSubTestBegin(ctrlType, "Get AutoPostBack true");
			try
			{
				m_lcToTest.AutoPostBack = true;
				Compare(m_lcToTest.AutoPostBack, true);
			}
			catch (Exception ex)
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}
			#endregion
			#region "Setting to false"
			GHTListContorlSubTestBegin(ctrlType, "AutoPostBack = false");
			try
			{
				m_lcToTest.AutoPostBack = false;
			}
			catch (Exception ex)
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}
			GHTSubTestEnd();
			#endregion
			#region "Getting false:"
			GHTListContorlSubTestBegin(ctrlType, "Get AutoPostBack false");
			try
			{
				m_lcToTest.AutoPostBack = false;
				Compare(m_lcToTest.AutoPostBack, false);
			}
			catch (Exception ex)
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}
			#endregion
		}
		protected void ListControl_ClearSelection(Type ctrlType)
		{
			#region "No item selected"
			GHTListContorlSubTestBegin(ctrlType, "No ite, selected");
			try
			{
				m_lcToTest.Items.Add("A");
				m_lcToTest.Items.Add("B");
				m_lcToTest.Items.Add("C");
				m_lcToTest.ClearSelection();
			}
			catch (Exception ex)
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}
			GHTSubTestEnd();
			#endregion
			#region "One item selected"
			GHTListContorlSubTestBegin(ctrlType, "One item selected");
			try
			{
				m_lcToTest.Items.Add("A");
				m_lcToTest.Items.Add("B");
				m_lcToTest.Items.Add("C");
				m_lcToTest.SelectedIndex = 1;
				m_lcToTest.ClearSelection();
			}
			catch (Exception ex)
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}
			#endregion
		}
		protected void ListControl_DataMember(Type ctrlType)
		{
			InitDataSet();
			#region "Existing Table"
			GHTListContorlSubTestBegin(ctrlType, "Existing table");
			
			try
			{
				m_lcToTest.DataSource = m_dsData;
				m_lcToTest.DataMember = "Second";
				m_lcToTest.DataTextField = "double Column";
				m_lcToTest.DataBind();
				Compare(m_lcToTest.DataMember, "Second");
			}
			catch (Exception ex)
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}

			GHTSubTestEnd();
			#endregion
			#region "Null"
			GHTListContorlSubTestBegin(ctrlType, "Null");
			try
			{
				m_lcToTest.DataSource = m_dsData;
				m_lcToTest.DataMember = null;
				m_lcToTest.DataTextField = "char Column";
				m_lcToTest.DataBind();
			}
			catch (Exception ex)
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}

			#endregion
			#region "Non existing table in a dataset"
#if !NET_2_0
			GHTListContorlSubTestBegin(ctrlType, "Non existing table in a dataset");
			
			try
			{
				m_lcToTest.DataSource = m_dsData;
				m_lcToTest.DataMember = "not a table name";
				m_lcToTest.DataBind();
				GHTSubTestExpectedExceptionNotCaught("HttpException");
			}
			catch (HttpException ex)
			{
				GHTSubTestExpectedExceptionCaught(ex);
			}
			catch (Exception ex)
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}
#endif
			#endregion
			#region "DataSource is not a dataset - set"
			GHTListContorlSubTestBegin(ctrlType, "DataSource is not a dataset");
			
			try
			{
				InitArray();
				m_lcToTest.DataSource = m_items;
				m_lcToTest.DataMember = "not a table name";
				m_lcToTest.DataBind();
			}
			catch (Exception ex)
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}
			#endregion
		}

		protected void ListControl_DataSource(Type ctrlType)
		{
			InitDataSet();
			InitArray();
			#region "DataSource that implements IEnumerable"
			GHTListContorlSubTestBegin(ctrlType, "DataSource that implements IEnumerable");
			
			try
			{
				IEnumerable dataSource = m_items;
				m_lcToTest.DataSource = dataSource;
				m_lcToTest.DataBind();
			}
			catch (Exception ex)
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}

			GHTSubTestEnd();
			#endregion
			#region "DataSource that implements IListSource"
			GHTListContorlSubTestBegin(ctrlType, "DataSource that implements IListSource");
			
			try
			{
				IListSource dataSource = m_dsData;
				m_lcToTest.DataSource = dataSource;
				m_lcToTest.DataTextField = "char Column";
				m_lcToTest.DataBind();
			}
			catch (Exception ex)
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}

			GHTSubTestEnd();
			#endregion
			#region "DataSource that does not implement IListSource or IEnumerable"
			GHTListContorlSubTestBegin(ctrlType, "DataSource that does not implement IListSource or IEnumerable");
			
			try
			{
				DataItem dataSource = new DataItem(1, "aaa");
				m_lcToTest.DataSource = dataSource;
				m_lcToTest.DataBind();
				GHTSubTestExpectedExceptionNotCaught("ArgumentException");
			}
#if NET_2_0
			catch (InvalidOperationException ex)
			{
				GHTSubTestExpectedExceptionCaught(ex);
			}
#endif
			catch (ArgumentException ex)
			{
				GHTSubTestExpectedExceptionCaught(ex);
			}
			catch (Exception ex)
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}

			GHTSubTestEnd();
			#endregion
		}

		protected void ListControl_DataTextField(Type ctrlType)
		{
			InitArray();
			InitDataSet();
			#region "string.empty - user defined items"
			GHTListContorlSubTestBegin(ctrlType, "string.empty");
			
			try
			{
				m_lcToTest.DataSource = m_items;
				m_lcToTest.DataTextField = string.Empty;
				m_lcToTest.DataBind();
			}
			catch (Exception ex)
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}

			GHTSubTestEnd();
			#endregion

//The result is ToString of DataRow which is default Object.ToString()
//In Java we get @addres at the end.
//			#region "string.empty - bound to a table"
//			GHTListContorlSubTestBegin(ctrlType, "string.empty - bound to a table");
//			
//			try
//			{
//				m_lcToTest.DataSource = m_dsData;
//				m_lcToTest.DataTextField = string.Empty;
//				m_lcToTest.DataBind();
//			}
//			catch (Exception ex)
//			{
//				GHTSubTestUnexpectedExceptionCaught(ex);
//			}
//
//			GHTSubTestEnd();
//			#endregion
			#region "Name of an items property"
			GHTListContorlSubTestBegin(ctrlType, "Name of an items property");
			
			try
			{
				m_lcToTest.DataSource = m_items;
				m_lcToTest.DataTextField = "Name";
				m_lcToTest.DataBind();
			}
			catch (Exception ex)
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}

			GHTSubTestEnd();
			#endregion
			#region "Name of an item non-existing property"
			GHTListContorlSubTestBegin(ctrlType, "Name of an item non-existing property");
			
			try
			{
				m_lcToTest.DataSource = m_items;
				m_lcToTest.DataTextField = "non-existing property";
				m_lcToTest.DataBind();
				GHTSubTestExpectedExceptionNotCaught("HttpException");
			}
			catch (HttpException ex)
			{
				GHTSubTestExpectedExceptionCaught(ex);
			}
			catch (Exception ex)
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}

			GHTSubTestEnd();
			#endregion
			#region "Name of a column"
			GHTListContorlSubTestBegin(ctrlType, "Name of a column");
			
			try
			{
				m_lcToTest.DataSource = m_dsData;
				m_lcToTest.DataTextField = "int Column";
				m_lcToTest.DataBind();
			}
			catch (Exception ex)
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}

			GHTSubTestEnd();
			#endregion
			#region "Name of a non-existing column"
			GHTListContorlSubTestBegin(ctrlType, "Name of a non-existing column");
			
			try
			{
				m_lcToTest.DataSource = m_dsData;
				m_lcToTest.DataTextField = "non-existing column";
				m_lcToTest.DataBind();
				GHTSubTestExpectedExceptionNotCaught("HttpException");
			}
			catch (HttpException ex)
			{
				GHTSubTestExpectedExceptionCaught(ex);
			}
			catch (Exception ex)
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}

			GHTSubTestEnd();
			#endregion
		}

		protected void ListControl_DataTextFormatString(Type ctrlType)
		{
			InitArray();
			#region "string.empty"
			GHTListContorlSubTestBegin(ctrlType, "string.empty");
			
			try
			{
				m_lcToTest.DataSource = m_items;
				m_lcToTest.DataTextField = "Name";
				m_lcToTest.DataTextFormatString = string.Empty;
				m_lcToTest.DataBind();
			}
			catch (Exception ex)
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}

			GHTSubTestEnd();
			#endregion
			#region "Valid format"
			GHTListContorlSubTestBegin(ctrlType, "Valid format");
			
			try
			{
				m_lcToTest.DataSource = m_items;
				m_lcToTest.DataTextField = "Name";
				m_lcToTest.DataTextFormatString = "format {0} format";
				m_lcToTest.DataBind();
			}
			catch (Exception ex)
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}

			GHTSubTestEnd();
			#endregion
			#region "Invalid format"
			GHTListContorlSubTestBegin(ctrlType, "Invalid format");
			
			try
			{
				m_lcToTest.DataSource = m_items;
				m_lcToTest.DataTextField = "Name";
				m_lcToTest.DataTextFormatString = "{invalid format}";
				m_lcToTest.DataBind();
				GHTSubTestExpectedExceptionNotCaught("FormatException");
			}
			catch (FormatException ex)
			{
				GHTSubTestExpectedExceptionCaught(ex);
			}
			catch (Exception ex)
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}

			GHTSubTestEnd();
			#endregion
		}

		protected void ListControl_Items(Type ctrlType)
		{
			InitArray();
			GHTListContorlSubTestBegin(ctrlType, "Type & contents");
			
			try
			{
				m_lcToTest.DataSource = m_items;
				m_lcToTest.DataTextField = "Name";
				m_lcToTest.DataValueField = "Id";
				m_lcToTest.DataBind();
				//Check the type:
				Compare(m_lcToTest.Items.GetType().ToString(), typeof(ListItemCollection).ToString());
				//Check all the items.
				for (int i=0; i<7; i++)
				{
					Compare(m_lcToTest.Items[i].Text, m_items[i].Name);
					Compare(m_lcToTest.Items[i].Value.ToString(), m_items[i].Id.ToString());
				}
			}
			catch (Exception ex)
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}

			GHTSubTestEnd();
		}

		protected void ListControl_DataValueField(Type ctrlType)
		{
			InitArray();
			InitDataSet();
			#region "string.empty - user defined items"
			GHTListContorlSubTestBegin(ctrlType, "string.empty");
			
			try
			{
				m_lcToTest.DataSource = m_items;
				m_lcToTest.DataValueField = string.Empty;
				m_lcToTest.DataBind();
			}
			catch (Exception ex)
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}

			GHTSubTestEnd();
			#endregion
			#region "string.empty - bound to a table"
			GHTListContorlSubTestBegin(ctrlType, "string.empty - bound to a table");
			
			try
			{
				m_lcToTest.DataSource = m_dsData;
				m_lcToTest.DataValueField= string.Empty;
				m_lcToTest.DataTextField = "char Column";
				m_lcToTest.DataBind();
			}
			catch (Exception ex)
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}

			GHTSubTestEnd();
			#endregion
			#region "Name of an items property"
			GHTListContorlSubTestBegin(ctrlType, "Name of an items property");
			
			try
			{
				m_lcToTest.DataSource = m_items;
				m_lcToTest.DataValueField = "Id";
				m_lcToTest.DataBind();
			}
			catch (Exception ex)
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}

			GHTSubTestEnd();
			#endregion
			#region "Name of an item non-existing property"
			GHTListContorlSubTestBegin(ctrlType, "Name of an item non-existing property");
			
			try
			{
				m_lcToTest.DataSource = m_items;
				m_lcToTest.DataValueField = "non-existing property";
				m_lcToTest.DataBind();
				GHTSubTestExpectedExceptionNotCaught("HttpException");
			}
			catch (HttpException ex)
			{
				GHTSubTestExpectedExceptionCaught(ex);
			}
			catch (Exception ex)
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}

			GHTSubTestEnd();
			#endregion
			#region "Name of a column"
			GHTListContorlSubTestBegin(ctrlType, "Name of a column");
			
			try
			{
				m_lcToTest.DataSource = m_dsData;
				m_lcToTest.DataValueField = "int Column";
				m_lcToTest.DataBind();
			}
			catch (Exception ex)
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}

			GHTSubTestEnd();
			#endregion
			#region "Name of a non-existing column"
			GHTListContorlSubTestBegin(ctrlType, "Name of a non-existing column");
			
			try
			{
				m_lcToTest.DataSource = m_dsData;
				m_lcToTest.DataValueField = "non-existing column";
				m_lcToTest.DataBind();
				GHTSubTestExpectedExceptionNotCaught("HttpException");
			}
			catch (HttpException ex)
			{
				GHTSubTestExpectedExceptionCaught(ex);
			}
			catch (Exception ex)
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}

			GHTSubTestEnd();
			#endregion
		}
		protected void ListControl_SelectedIndex(Type ctrlType)
		{
			InitArray();
			#region "None selected"
			GHTListContorlSubTestBegin(ctrlType, "None selected");
			
			try
			{
				m_lcToTest.DataSource = m_items;
				m_lcToTest.DataBind();
				m_lcToTest.SelectedIndex = -1;
				GHTSubTestAddResult(m_lcToTest.SelectedIndex.ToString());
			}
			catch (Exception ex)
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}
			#endregion
			#region "valid value"
			GHTListContorlSubTestBegin(ctrlType, "valid value");
			
			try
			{
				m_lcToTest.DataSource = m_items;
				m_lcToTest.DataBind();
				m_lcToTest.SelectedIndex = 5;
				GHTSubTestAddResult(m_lcToTest.SelectedIndex.ToString());
			}
			catch (Exception ex)
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}
			#endregion
			#region "Invalid value - above length of items."
			GHTListContorlSubTestBegin(ctrlType, "Invalid value - above length of items.");
			
			try
			{
				m_lcToTest.DataSource = m_items;
				m_lcToTest.DataBind();
				m_lcToTest.SelectedIndex = 10;
				GHTSubTestExpectedExceptionNotCaught("ArgumentOutOfRangeException");
			}
			catch (ArgumentOutOfRangeException ex)
			{
				GHTSubTestExpectedExceptionCaught(ex);
			}
			catch (Exception ex)
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}
			#endregion
			#region "Invalid value - below -1."
			GHTListContorlSubTestBegin(ctrlType, "Invalid value - below -1.");
			
			try
			{
				m_lcToTest.DataSource = m_items;
				m_lcToTest.DataBind();
				m_lcToTest.SelectedIndex = -2;
				GHTSubTestExpectedExceptionNotCaught("ArgumentOutOfRangeException");
			}
			catch (ArgumentOutOfRangeException ex)
			{
				GHTSubTestExpectedExceptionCaught(ex);
			}
			catch (Exception ex)
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}
			#endregion
		}

		protected void ListControl_SelectedItem(Type ctrlType)
		{
			InitArray();
			#region "None selected"
			GHTListContorlSubTestBegin(ctrlType, "None selected");
			
			try
			{
				m_lcToTest.DataSource = m_items;
				m_lcToTest.DataTextField = "Name";
				m_lcToTest.DataBind();
				m_lcToTest.SelectedIndex = -1;
				if (m_lcToTest.SelectedItem == null)
				{
					GHTSubTestAddResult("Test passed: SelectedItem is null");
				}
				else
				{
					GHTSubTestAddResult("Test failede: SelectedItem is not null");
				}
			}
			catch (Exception ex)
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}
			#endregion
			#region "Single Item selected"
			GHTListContorlSubTestBegin(ctrlType, "valid value");
			
			try
			{
				m_lcToTest.DataSource = m_items;
				m_lcToTest.DataTextField = "Name";
				m_lcToTest.DataBind();
				m_lcToTest.SelectedIndex = 5;
				Compare(m_lcToTest.SelectedItem.Text, m_items[5].Name );
			}
			catch (Exception ex)
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}
			#endregion
			#region "multiple Items selected"
			GHTListContorlSubTestBegin(ctrlType, "multiple Items selected");
			if (m_lcToTest is DropDownList)
			{
				return;
			}
			if (m_lcToTest is ListBox)
			{
				((ListBox)m_lcToTest).SelectionMode = ListSelectionMode.Multiple;
			}
			try
			{
				m_lcToTest.DataSource = m_items;
				m_lcToTest.DataTextField = "Name";
				m_lcToTest.DataBind();
				m_lcToTest.Items[2].Selected = true;
				m_lcToTest.Items[4].Selected = true;
				m_lcToTest.Items[6].Selected = true;
				Compare(m_lcToTest.SelectedItem.Text, m_items[2].Name );
			}
			catch (Exception ex)
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}

			#endregion
		}

		protected void ListControl_SelectedValue(Type ctrlType)
		{
			InitArray();
			#region "None selected - get"
			GHTListContorlSubTestBegin(ctrlType, "None selected - get");
			
			try
			{
				m_lcToTest.DataSource = m_items;
				m_lcToTest.DataValueField = "Id";
				m_lcToTest.DataBind();
				m_lcToTest.SelectedIndex = -1;
				Compare(m_lcToTest.SelectedValue, string.Empty);
			}
			catch (Exception ex)
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}
			#endregion
			#region "valid value - get"
			GHTListContorlSubTestBegin(ctrlType, "valid value - get");
			
			try
			{
				m_lcToTest.DataSource = m_items;
				m_lcToTest.DataValueField = "Id";
				m_lcToTest.DataBind();
				m_lcToTest.SelectedIndex = 5;
				Compare(m_lcToTest.SelectedValue, m_items[5].Id.ToString());
			}
			catch (Exception ex)
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}
			#endregion
			#region "valid value - set"
			GHTListContorlSubTestBegin(ctrlType, "valid value - set");
			
			try
			{
				m_lcToTest.DataSource = m_items;
				m_lcToTest.DataValueField = "Id";
				m_lcToTest.DataBind();
				m_lcToTest.SelectedValue = "5";
			}
			catch (Exception ex)
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}
			#endregion
			#region "Invalid value - set"
			GHTListContorlSubTestBegin(ctrlType, "Invalid value - set");
			
			try
			{
				m_lcToTest.DataSource = m_items;
				m_lcToTest.DataBind();
				m_lcToTest.DataValueField = "Id";
				m_lcToTest.SelectedValue = "10";
				GHTSubTestExpectedExceptionNotCaught("ArgumentOutOfRangeException");
			}
			catch (ArgumentOutOfRangeException ex)
			{
				GHTSubTestExpectedExceptionCaught(ex);
			}
			catch (Exception ex)
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}
			#endregion
		}

		#endregion

		#region "Construction"
		/// <summary>
		/// Default constructor.
		/// </summary>
		public GHTListControlBase()
		{
		}
		/// <summary>
		/// Static constructor.
		/// Initializes the static field m_types[].
		/// </summary>
		static GHTListControlBase()
		{
			initTypes();
		}
		#endregion
		
		#region "properties"
		public static Type[] TestedTypes
		{
			get
			{
				return (Type[])(m_types.ToArray(typeof(Type)));
			}
		}
		#endregion

		#region "members"
		/// <summary>
		/// Holds the ListControl that is tested in the current subtest.
		/// </summary>
		private ListControl m_lcToTest;
		private int m_controlsCounter = 0;

		/// <summary>
		/// Holds all the types that are derived from ListControl, and should be tested.
		/// </summary>
		private  static readonly  ArrayList m_types = new ArrayList();
		/// <summary>
		/// Two data set to use while testing.
		/// Initialize this dataset using InitDataSet()
		/// </summary>
		private DataSet m_dsData;
		private DataTable m_dtFirst;
		private DataTable m_dtSecond;

		/// <summary>
		/// An array of Item objects that can be used as a datasource.
		/// initialize the array using InitArray()
		/// </summary>
		private DataItem[] m_items;
		#endregion

		#region "Private methods"
		/// <summary>
		/// Initializes the collection of types derived from ListControl.
		/// </summary>
		private static void initTypes()
		{
			m_types.Add(typeof(ListBox));
			m_types.Add(typeof(DropDownList));
			m_types.Add(typeof(RadioButtonList));
			m_types.Add(typeof(CheckBoxList));
		}
		/// <summary>
		/// Creates new sub test and adds a new ListControl to it.
		/// </summary>
		/// <param name="ctrlType">Actual type of the tested control</param>
		/// <param name="description">subtests description</param>
		private void GHTListContorlSubTestBegin(Type ctrlType, string description)
		{
			m_lcToTest = (ListControl)GHTElementClone(ctrlType);
			m_lcToTest.ID = "_ctrl" + m_controlsCounter;
			m_controlsCounter++;
			GHTSubTestBegin(description);
			GHTActiveSubTest.Controls.Add(m_lcToTest);
		}
		
		/// <summary>
		/// Initializes both m_dtFirst, and m_dtSecond with names ("First", "Second"), columns, and data.
		/// </summary>
		private void InitDataSet()
		{
			m_dtFirst = new DataTable("First");
			m_dtFirst.Columns.Add("int Column", typeof(int));
			m_dtFirst.Columns.Add("bool Column", typeof(bool));
			m_dtFirst.Columns.Add("char Column", typeof(char));

			m_dtSecond = new DataTable("Second");
			m_dtSecond.Columns.Add("double Column", typeof(int));
			m_dtSecond.Columns.Add("byte Column", typeof(bool));
			m_dtSecond.Columns.Add("time Column", typeof(DateTime));

			for (int i=0; i<10; i++)
			{
				DataRow dr1 = m_dtFirst.NewRow();
				dr1["int Column"] = i;
				dr1["bool Column"] = ( i % 2  == 0 ) ? true : false;
				dr1["char Column"] = (char)(i + 'a');
				m_dtFirst.Rows.Add(dr1);

				DataRow dr2 = m_dtSecond.NewRow();
				dr2["double Column"] = double.Epsilon * i;
				dr2["byte Column"] = (byte)( i % 10);
				dr2["time Column"] = DateTime.Now;
				m_dtSecond.Rows.Add(dr2);
			}

			m_dsData = new DataSet("Test dataset");
			m_dsData.Tables.Add(m_dtFirst);
			m_dsData.Tables.Add(m_dtSecond);
		}


		private void InitArray()
		{
			m_items = new DataItem[] {	new DataItem(1, "aaa"),
																					new DataItem(2, "bbb"),
																					new DataItem(3, "ccc"),
																					new DataItem(4, "ddd"),
																					new DataItem(5, "eee"),
																					new DataItem(6, "fff"),
																					new DataItem(7, "ggg")};
		}

		/// <summary>
		/// Nested class, to use as the items of the m_items array.
		/// </summary>
		private class DataItem
		{
			public DataItem(int a_id, string a_name)
			{
				id = a_id;
				name = a_name;
			}

			private int id;
			private string name;

			public int Id
			{
				get
				{
					return id;
				}
				set
				{
					id = value;
				}
			}

			public string Name
			{
				get
				{
					return name;
				}
				set
				{
					name = value;
				}

			}


			public override string ToString()
			{
				return id.ToString() + name;
			}
		}
	}
	#endregion

}
