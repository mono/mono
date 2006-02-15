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
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Collections;
using System.Data;

namespace GHTTests
{
	/// <summary>
	/// Provides basic functionalities for testing System.Web.UI.Control derived classes.
	/// </summary>
	public class GHTControlBase : GHTBaseWeb
	{
		#region "Data members"
		protected Control m_cToTest;				//The control that is currently being tested.
		protected TextBox m_tbToValidate;		//will be used by validation controls as the control to validate.
		protected Item[] m_aDataSource;			//Array data source to use in data bound objects.
		protected DataTable m_dtDataSource;//DataTable data source to use in data bound objects.
		protected  ArrayList m_derivedTypes;	//The array that wil contain all types that are derived from Control, and need to be tested.
		protected long m_controlsCounter;		//Used to generate a unique id for each of the controls created using GHTActiveSubTestControlClone
		#endregion

		#region "Construction"
		/// <summary>
		/// Default c'tor.
		/// handles basic initialization of the page, and contents.
		/// </summary>
		public GHTControlBase()
		{
			InitTypes();
			InitDataSource();
			InitTbToValidate();
			m_controlsCounter = 0;
		}
		#endregion

		#region "Properties"
		protected Control TestedControl
		{
			get
			{
				return m_cToTest;
			}
		}
		public  Type[] TypesToTest
		{
			get
			{
				return (System.Type[])(m_derivedTypes.ToArray(typeof(System.Type)));
			}
		}
		#endregion

		#region "Methods"
		/// <summary>
		/// Initializes all the derived types that need to be tested.
		/// </summary>
		protected virtual  void InitTypes()
		{
			m_derivedTypes = new ArrayList();

			//System.Web.UI:
			//				typeof(System.Web.UI.PartialCachingControl),						Excluded from test.
			//				typeof(System.Web.UI.StaticPartialCachingControl),		Excluded from test.
			//				typeof(System.Web.UI.DataBoundLiteralControl),				Excluded from test.
			m_derivedTypes.Add(typeof(System.Web.UI.LiteralControl));
			m_derivedTypes.Add(typeof(System.Web.UI.Page));
			m_derivedTypes.Add(typeof(System.Web.UI.UserControl));

			//System.Web.UI.HtmlControls:
			m_derivedTypes.Add(typeof(System.Web.UI.HtmlControls.HtmlAnchor));
			m_derivedTypes.Add(typeof(System.Web.UI.HtmlControls.HtmlButton));
			//				typeof(System.Web.UI.HtmlControls.HtmlForm),				Excluded from test.
			m_derivedTypes.Add(typeof(System.Web.UI.HtmlControls.HtmlGenericControl));
			m_derivedTypes.Add(typeof(System.Web.UI.HtmlControls.HtmlImage));
			m_derivedTypes.Add(typeof(System.Web.UI.HtmlControls.HtmlInputButton));
			m_derivedTypes.Add(typeof(System.Web.UI.HtmlControls.HtmlInputCheckBox));
			m_derivedTypes.Add(typeof(System.Web.UI.HtmlControls.HtmlInputFile));
			m_derivedTypes.Add(typeof(System.Web.UI.HtmlControls.HtmlInputHidden));
			m_derivedTypes.Add(typeof(System.Web.UI.HtmlControls.HtmlInputImage));
			m_derivedTypes.Add(typeof(System.Web.UI.HtmlControls.HtmlInputRadioButton));
			m_derivedTypes.Add(typeof(System.Web.UI.HtmlControls.HtmlInputText));
			m_derivedTypes.Add(typeof(System.Web.UI.HtmlControls.HtmlSelect));
			m_derivedTypes.Add(typeof(System.Web.UI.HtmlControls.HtmlTable));
			m_derivedTypes.Add(typeof(System.Web.UI.HtmlControls.HtmlTableCell));
			m_derivedTypes.Add(typeof(System.Web.UI.HtmlControls.HtmlTableRow));
			m_derivedTypes.Add(typeof(System.Web.UI.HtmlControls.HtmlTextArea));

			//System.Web.UI.WebControls basic:
			m_derivedTypes.Add(typeof(System.Web.UI.WebControls.Button));
			m_derivedTypes.Add(typeof(System.Web.UI.WebControls.CheckBox));
			m_derivedTypes.Add(typeof(System.Web.UI.WebControls.HyperLink));
			m_derivedTypes.Add(typeof(System.Web.UI.WebControls.Image));
			m_derivedTypes.Add(typeof(System.Web.UI.WebControls.ImageButton));
			m_derivedTypes.Add(typeof(System.Web.UI.WebControls.Label));
			m_derivedTypes.Add(typeof(System.Web.UI.WebControls.LinkButton));
			m_derivedTypes.Add(typeof(System.Web.UI.WebControls.Literal));
			m_derivedTypes.Add(typeof(System.Web.UI.WebControls.Panel));
			m_derivedTypes.Add(typeof(System.Web.UI.WebControls.PlaceHolder));
			m_derivedTypes.Add(typeof(System.Web.UI.WebControls.RadioButton));
			m_derivedTypes.Add(typeof(System.Web.UI.WebControls.TextBox));

			//System.Web.UI.WebControls basic list controls:
			m_derivedTypes.Add(typeof(System.Web.UI.WebControls.DropDownList));
			m_derivedTypes.Add(typeof(System.Web.UI.WebControls.ListBox));
			m_derivedTypes.Add(typeof(System.Web.UI.WebControls.RadioButtonList));
			m_derivedTypes.Add(typeof(System.Web.UI.WebControls.CheckBoxList));

			//System.Web.UI.WebControls validation controls:
#if KNOWN_BUG //BUG_NUM:935
			m_derivedTypes.Add(typeof(System.Web.UI.WebControls.CompareValidator));
			m_derivedTypes.Add(typeof(System.Web.UI.WebControls.CustomValidator));
			m_derivedTypes.Add(typeof(System.Web.UI.WebControls.RangeValidator));
			m_derivedTypes.Add(typeof(System.Web.UI.WebControls.RegularExpressionValidator));
			m_derivedTypes.Add(typeof(System.Web.UI.WebControls.RequiredFieldValidator));
#endif
#if KNOWN_BUG //BUG_NUM:1195
			m_derivedTypes.Add(typeof(System.Web.UI.WebControls.ValidationSummary));
#endif
			//System.Web.UI.WebControls rich controls (currently not supported):
//			m_derivedTypes.Add(typeof(System.Web.UI.WebControls.AdRotator));
//			m_derivedTypes.Add(typeof(System.Web.UI.WebControls.Calendar));

			//System.Web.UI.WebControls advanced list controls:
			m_derivedTypes.Add(typeof(System.Web.UI.WebControls.DataGrid));
			m_derivedTypes.Add(typeof(System.Web.UI.WebControls.DataGridItem));
			m_derivedTypes.Add(typeof(System.Web.UI.WebControls.DataList));
			m_derivedTypes.Add(typeof(System.Web.UI.WebControls.DataListItem));
			m_derivedTypes.Add(typeof(System.Web.UI.WebControls.Repeater));
			m_derivedTypes.Add(typeof(System.Web.UI.WebControls.RepeaterItem));
			m_derivedTypes.Add(typeof(System.Web.UI.WebControls.Table));
			m_derivedTypes.Add(typeof(System.Web.UI.WebControls.TableCell));
			m_derivedTypes.Add(typeof(System.Web.UI.WebControls.TableHeaderCell));
			m_derivedTypes.Add(typeof(System.Web.UI.WebControls.TableRow));
			//m_derivedTypes.Add(typeof( System.Web.UI.WebControls.Xml));
		}

		/// <summary>
		/// Adds a control to page.
		/// If the control is in the context of other control (Have a parent) 
		/// (e.g.) TableCell, then the parent control is added to the page.
		/// </summary>
		/// <param name="a_toAdd">The control to add to the page.</param>
		protected void GHTAddToActiveForm(Control a_toAdd)
		{
			if (a_toAdd.Parent == null)
			{
				GHTActiveForm.Controls.Add(a_toAdd);
			}
			else
			{
				GHTAddToActiveForm(a_toAdd.Parent);
			}
		}
		

		/// <summary>
		/// Creates a control to test, and adds it to a new subtest.
		/// </summary>
		/// <param name="ctrlType">Type of control to test.</param>
		/// <param name="description">description to add to subtest.</param>
		protected void GHTSubTestBegin(Type ctrlType, string description)
		{
			GHTSubTestBegin(ctrlType, description, true);
		}

		/// <summary>
		/// Creates a new subtest, and a control to test.
		/// </summary>
		/// <param name="ctrlType">Type of control to test.</param>
		/// <param name="description">description to add to subtest.</param>
		/// <param name="a_AddToPage">Whether to add the control to the subtest continer.</param>
		protected void GHTSubTestBegin(Type a_ctrlType, string description, bool a_addToPage)
		{
			base.GHTSubTestBegin(a_ctrlType.ToString() + ": " + description);
			GHTActiveSubtestControlClone(a_ctrlType, a_addToPage);
		}
		/// <summary>
		/// Clones a control within its context, and adds it to the active subtest.
		/// e.g. if the control is a table cell, then a table will be created to wrap the table cell.
		/// The table will be added to the active subtest, and the cell will be referened by m_m_cToTest.
		/// </summary>
		/// <param name="a_ctrlType">Type of control to test.</param>
		/// <param name="a_addToPage">Whether to add the control to the subtest continer.</param>
		protected void GHTActiveSubtestControlClone(Type a_ctrlType, bool a_addToPage)
		{
			if (IsListControlDerived(a_ctrlType))
			{
				m_cToTest = GetListControlDerived(a_ctrlType, a_addToPage);
			}
			else if (IsIterativeControlControl(a_ctrlType))
			{
				m_cToTest = GetIterativeControl(a_ctrlType, a_addToPage);
			}
			else if (IsIterativeControlControlItem(a_ctrlType))
			{
				m_cToTest = GetIterativeControlItem(a_ctrlType, a_addToPage);
			}
			else if (IsTableRelated(a_ctrlType))
			{
				m_cToTest = GetTableRelated(a_ctrlType, a_addToPage);
			}
			else if (IsHTMLTableRelated(a_ctrlType))
			{
				m_cToTest = GetHtmlTableRelated(a_ctrlType, a_addToPage);
			}
			else
			{
				m_cToTest = (Control)GHTElementClone(a_ctrlType);
				if (a_addToPage)
				{
					GHTActiveSubTest.Controls.Add(m_cToTest);
				}
			}
			HandleValidationControls();	
			HandleCausesValidation();
			SetId();	//Set a unique id to the control.
		}

		/// <summary>
		/// checks if a given type is derived from ListControl, and thus should be bound to data in order to display content.
		/// </summary>
		/// <param name="ctrlType">The type to test.</param>
		/// <returns>True if the ctrlType is derived from ListControl, otherwise false.</returns>
		private bool IsListControlDerived(Type ctrlType)
		{
			return ctrlType.IsSubclassOf(typeof(ListControl));
		}

		/// <summary>
		/// Creates a control of the specified type, if it is a ListControl derived, then bounds it to data source.
		/// </summary>
		/// <param name="ctrlType">Type of control to create.</param>
		private Control GetListControlDerived(Type ctrlType, bool a_AddToPage)
		{
			ListControl l_listcontrol = GHTElementClone(ctrlType) as ListControl;
			if (l_listcontrol != null)
			{
				l_listcontrol.DataSource = m_aDataSource;
				l_listcontrol.DataTextField = "Description";
				l_listcontrol.DataValueField = "Id";
				l_listcontrol.DataBind();
				if (a_AddToPage)
				{
					GHTActiveSubTest.Controls.Add(l_listcontrol);
				}
				return l_listcontrol;
			}
			else
			{
				throw new ArgumentOutOfRangeException("ctrlType", ctrlType, "Must be a ListControl derived type.");
			}
		}

		/// <summary>
		/// checks if a given type is an item of an iterative control, and thus should be checked in tis controls context.
		/// </summary>
		/// <param name="ctrlType">The type to test.</param>
		/// <returns>True if the ctrlType is an iteative control item, otherwise false.</returns>
		private bool IsIterativeControlControlItem(Type ctrlType)
		{
			if ( ctrlType.Equals(typeof(RepeaterItem)))
			{
				return true;
			}
			else if (ctrlType.Equals(typeof(DataListItem)))
			{
				return true;
			}
			else if (ctrlType.Equals(typeof(DataGridItem)))
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// Creates an iterative control Item, included inside an iterative control..
		/// </summary>
		/// <param name="ctrlType">Type of control to create.</param>
		private Control GetIterativeControlItem(Type ctrlType, bool a_AddToPage)
		{
			if (ctrlType.Equals(typeof(RepeaterItem)))
			{
				Repeater l_rep = GetIterativeControl(typeof(Repeater), a_AddToPage) as Repeater;
				SetId(l_rep);
				return l_rep.Items[0];
			}

			if (ctrlType.Equals(typeof(DataListItem)))
			{
				DataList l_datalist = GetIterativeControl(typeof(DataList), a_AddToPage) as DataList;
				SetId(l_datalist);
				return l_datalist.Items[0];
			}
		
			if (ctrlType.Equals(typeof(DataGridItem)))
			{
				DataGrid l_datagrid = GetIterativeControl(typeof(DataGrid), a_AddToPage) as DataGrid;
				SetId(l_datagrid);
				return l_datagrid.Items[0];
			}
			else
			{
				throw new ArgumentOutOfRangeException("ctrlType", ctrlType, "Allowed types are RepeaterItem, DataListItem or DataGridItem");
			}
		}

		/// <summary>
		/// Checks if a given type is an iterative control, and thus should be boud to data.
		/// </summary>
		/// <param name="ctrlType">The type to test.</param>
		/// <returns>True if the ctrlType is an iteative control, otherwise false.</returns>
		private bool IsIterativeControlControl(Type ctrlType)
		{
			if ( ctrlType.Equals(typeof(Repeater)))
			{
				return true;
			}
			else if (ctrlType.Equals(typeof(DataList)))
			{
				return true;
			}
			else if (ctrlType.Equals(typeof(DataGrid)))
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// Creates an iterative control.
		/// </summary>
		/// <param name="ctrlType">Type of control to create.</param>
		protected Control GetIterativeControl(Type ctrlType, bool a_AddToPage)
		{
			if (ctrlType.Equals(typeof(Repeater)))
			{
					Repeater l_rep = new Repeater();
					l_rep.ItemTemplate = new RepeaterTemplate();
					l_rep.DataSource = m_aDataSource;
					l_rep.DataBind();
					if (a_AddToPage)
					{
						GHTActiveSubTest.Controls.Add(l_rep);
					}
					return l_rep;
			}
			else if (ctrlType.Equals(typeof(DataList)))
			{
				DataList l_dataList = new DataList();
				l_dataList.ItemTemplate =  new DataListTemplate();
				l_dataList.DataSource = m_aDataSource;
				l_dataList.DataBind();
				if (a_AddToPage)
				{
					GHTActiveSubTest.Controls.Add(l_dataList);
				}
				return l_dataList;
			}
			else if (ctrlType.Equals(typeof(DataGrid)))
			{
				DataGrid l_dataGrid = new DataGrid();
				l_dataGrid.DataSource = m_dtDataSource;
				l_dataGrid.DataBind();
				if (a_AddToPage)
				{
					GHTActiveSubTest.Controls.Add(l_dataGrid);
				}
				return l_dataGrid;
			}
			else
			{
				throw new ArgumentOutOfRangeException("ctrlType", ctrlType, "Allowed types are Repeater, DataList or DataGrid");
			}
		}

		/// <summary>
		/// checks if a given type is a control that should be tested in the context of a HtmlTable.
		/// </summary>
		/// <param name="ctrlType">The type to test.</param>
		/// <returns>True if the ctrlType is a HtmlTable related type, otherwise false.</returns>
		private bool IsHTMLTableRelated(Type ctrlType)
		{
			if ( ctrlType.Equals(typeof(HtmlTable)))
			{
				return true;
			}
			else if (ctrlType.Equals(typeof(HtmlTableRow)))
			{
				return true;
			}
			else if (ctrlType.Equals(typeof(HtmlTableCell)))
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// Creates a new table, and adds it to the GHTActivesubTest.
		/// </summary>
		/// <param name="ctrlType">The type of the ctrl to test. must be one of </param>
		/// <param name="description"></param>
		/// <returns></returns>
		protected Control GetHtmlTableRelated(Type ctrlType, bool a_AddToPage)
		{
			HtmlTable l_table = new HtmlTable();
			HtmlTableRow l_row = new HtmlTableRow();
			HtmlTableCell l_cell = new HtmlTableCell();
			
			if (a_AddToPage)
			{
				GHTActiveSubTest.Controls.Add(l_table);
			}
			l_table.Rows.Add(l_row);
			l_row.Cells.Add(l_cell);			

			l_cell.InnerText = "Cell";

			if ( l_table.GetType() == ctrlType)
			{
				return l_table;
			}
			else if (l_row.GetType() == ctrlType)
			{
				return l_row;
			}
			else if (l_cell.GetType() == ctrlType)
			{
				return l_cell;
			}
			else
			{
				throw new ArgumentException("Should be HtmlTable related type.", "ctrlType = " + ctrlType.ToString() );
			}

		}
		/// <summary>
		/// checks if a given type is a control that should be tested in the context of a table.
		/// </summary>
		/// <param name="ctrlType">The type to test.</param>
		/// <returns>True if the ctrlType is a table related type, otherwise false.</returns>
		private bool IsTableRelated(Type ctrlType)
		{
			if ( ctrlType.Equals(typeof(Table)))
			{
				return true;
			}
			else if (ctrlType.Equals(typeof(TableRow)))
			{
				return true;
			}
			else if (ctrlType.Equals(typeof(TableCell)))
			{
				return true;
			}
			else if(ctrlType.Equals(typeof(TableHeaderCell)))
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// Creates a new table, and adds it to the GHTActivesubTest.
		/// </summary>
		/// <param name="ctrlType">The type of the ctrl to test. must be one of </param>
		/// <param name="description"></param>
		/// <returns></returns>
		protected Control GetTableRelated(Type ctrlType, bool a_AddToPage)
		{
			Table l_table = new Table();
			TableRow l_headerRow = new TableRow();
			TableRow l_row = new TableRow();
			TableCell l_cell = new TableCell();
			TableHeaderCell l_headerCell = new TableHeaderCell();
			
			if (a_AddToPage)
			{
				GHTActiveSubTest.Controls.Add(l_table);
			}

			l_table.Rows.Add(l_headerRow);
			l_table.Rows.Add(l_row);

			l_headerRow.Cells.Add(l_headerCell);
			l_row.Cells.Add(l_cell);		
	
			l_headerCell.Text = "Header cell";
			l_cell.Text = "Table cell";

			if ( l_table.GetType() == ctrlType)
			{
				return l_table;
			}
			else if (l_row.GetType() == ctrlType)
			{
				return l_row;
			}
			else if (l_cell.GetType() == ctrlType)
			{
				return l_cell;
			}
			else if(l_headerCell.GetType() == ctrlType)
			{
				return l_headerCell;
			}
			else
			{
				throw new ArgumentException("Should be table related type.", "ctrlType = " + ctrlType.ToString() );
			}

		}

		/// <summary>
		/// All validation controls must be assigned a control to validate before the page is loaded:
		/// </summary>
		protected void HandleValidationControls(Control l_toTest)
		{
			BaseValidator l_validator = l_toTest as BaseValidator;
			if (l_validator == null)
			{
				return;
			}
			l_validator.ControlToValidate = "m_tbToValidate";
			HandleCompareValidator(l_toTest);
		}
		/// <summary>
		/// All validation controls must be assigned a control to validate before the page is loaded:
		/// </summary>
		protected void HandleValidationControls()
		{
			HandleValidationControls(m_cToTest);
		}

		/// <summary>
		/// Compare validator must have either ValueToCompare or ControlToCompare set, before the page is loaded.
		/// </summary>
		private void HandleCompareValidator(Control l_cToTest)
		{
			CompareValidator l_compareValidator = l_cToTest as CompareValidator;
			if (l_compareValidator == null)
			{
				return;
			}
			l_compareValidator.ValueToCompare = "value to compare";
		}
		/// <summary>
		/// Compare validator must have either ValueToCompare or ControlToCompare set, before the page is loaded.
		/// </summary>
		private void HandleCompareValidator()
		{
			HandleCompareValidator(m_cToTest);
		}

		/// <summary>
		/// Sets the causes validation property to false, if such a property exists in the m_cToTest.
		/// This is needed because for controls that cause validation, the ValidationControls on the page
		/// generate a client side script.
		/// That feature is not working properly in GrassHopper, and is not the main issue of this test.
		/// It will be testes specificly in the Validation control tests.
		/// </summary>
		private void HandleCausesValidation()
		{
			//HtmlButton:
			HtmlButton hButton  = m_cToTest as HtmlButton;
			if (hButton != null)
			{
				hButton.CausesValidation = false;
			}
			//HtmlInputButton
			HtmlInputButton hInputButton  = m_cToTest as HtmlInputButton;
			if (hInputButton != null)
			{
				hInputButton.CausesValidation = false;
			}
			//HtmlInputImage
			HtmlInputImage hInputImage = m_cToTest as HtmlInputImage;
			if (hInputImage != null)
			{
				hInputImage.CausesValidation = false;
			}
			//Button:
			Button button  = m_cToTest as Button;
			if (button != null)
			{
				button.CausesValidation = false;
			}
			//ImageButton
			ImageButton imageButton  = m_cToTest as ImageButton;
			if (imageButton != null)
			{
				imageButton.CausesValidation = false;
			}
			//LinkButton
			LinkButton linkButton  = m_cToTest as LinkButton;
			if (linkButton != null)
			{
				linkButton.CausesValidation = false;
			}
		}
		/// <summary>
		/// Creates a textbox, and adds it to the page as not visable.
		/// Creates a Required Field validator, and adds it to the page as not visible.
		/// </summary>
		private void InitTbToValidate()
		{
			m_tbToValidate = new TextBox();
			m_tbToValidate.Visible = false;
			m_tbToValidate.ID = "m_tbToValidate";
			this.Controls.Add(m_tbToValidate);
		}

		/// <summary>
		/// Creates the data sources to bind to.
		/// </summary>
		private void InitDataSource()
		{
			InitDataArray();
			InitDataTable();
		}

		/// <summary>
		/// Creates an array to be used as a data source.
		/// </summary>
		private void InitDataArray()
		{
			m_aDataSource = new Item[] {	new Item(1,"aaaa"),
										  new Item(2,"bbbb"),
										  new Item(3,"cccc"),
										  new Item(4,"dddd")};
		}

		/// <summary>
		/// creates a data table to be used as a data source.
		/// </summary>
		private void InitDataTable()
		{
			m_dtDataSource = new DataTable("SourceTable");

			DataColumn l_dcId = new DataColumn("Id", typeof(int));
			DataColumn l_dcDescription = new DataColumn("Description", typeof(string));

			m_dtDataSource.Columns.Add(l_dcId);
			m_dtDataSource.Columns.Add(l_dcDescription);
			
			for (int i=0; i<m_aDataSource.Length; i++)
			{
				DataRow l_drCurrent = m_dtDataSource.NewRow();
				l_drCurrent["Id"] = m_aDataSource[i].Id;
				l_drCurrent["Description"] = m_aDataSource[i].Description;
				m_dtDataSource.Rows.Add(l_drCurrent);
			}
		}
		
		/// <summary>
		/// Sets a unique unified id to the tested control based on the count of controls produced by this base.
		/// </summary>
		private void SetId()
		{
			SetId(m_cToTest);
		}

		/// <summary>
		/// Sets a unique unified id to a control based on the count of controls produced by this base.
		/// </summary>
		private void SetId(Control a_toSet)
		{
			a_toSet.ID = "ctrl_" + m_controlsCounter.ToString();
			m_controlsCounter++;
		}
		#endregion

		/// <summary>
		/// Used as an array item, for the data source of data bound controls in this test.
		/// </summary>
		public class Item
		{
			#region "Construction"
			public Item() : this(0, String.Empty)
			{
			}

			public Item(int a_id, string a_description)
			{
				m_id = a_id;
				m_description = a_description;
			}
			#endregion

			#region "Data Members"
			private int m_id;
			private string m_description;
			#endregion

			#region "Properties"
			public int Id 
			{
				get
				{
					return m_id;
				}
				set
				{
					m_id = value;
				}
			}

			public string Description
			{
				get
				{
					return m_description;
				}
				set
				{
					m_description = value;
				}
			}

			#endregion

			#region "Overrides"
			public override string ToString()
			{
				return this.Id + " " + this.Description;
			}

#endregion
		}
	
		/// <summary>
		/// The templte used by repeater to render its items.
		/// </summary>
		public class RepeaterTemplate : ITemplate
		{
			#region ITemplate Members
			/// <summary>
			/// Implements ITemplate.instantiateIn(..)
			/// Adds an item to the repeater.
			/// </summary>
			/// <param name="a_container">The controls to add the templated item to.</param>
			public void InstantiateIn(Control a_container)
			{
				//Header labels:				
				//~~~~~~~~~~~
				Label l_lIdHeader = new Label();
				l_lIdHeader.Text = "ID: ";
				l_lIdHeader.Font.Bold = true;
				Label l_lDescrptionHeader = new Label();
				l_lDescrptionHeader.Text = "	Description: ";
				l_lDescrptionHeader.Font.Bold = true;

				//Data labels:
				//~~~~~~~~~
				Label l_lIdData = new Label();
				l_lIdData.DataBinding += new EventHandler(this.BindId);
				Label l_lDescriptionData = new Label();
				l_lDescriptionData.DataBinding += new EventHandler(this.BindDescription);

				//The complete panel.
				Panel l_pItemPanel = new Panel();
				l_pItemPanel.Controls.Add(l_lIdHeader);
				l_pItemPanel.Controls.Add(l_lIdData);
				l_pItemPanel.Controls.Add(l_lDescrptionHeader);
				l_pItemPanel.Controls.Add(l_lDescriptionData);

				a_container.Controls.Add(l_pItemPanel);
			}
			#endregion

			/// <summary>
			/// Handles the data binding event of the Id label in the templated item.
			/// </summary>
			/// <param name="sender">The source of the event.</param>
			/// <param name="args">Additional information about the event.</param>
			private void BindId(Object sender, EventArgs args)
			{
				Label l_lIddata = (Label)sender;
				RepeaterItem l_riContainer = (RepeaterItem)l_lIddata.NamingContainer;
				l_lIddata.Text = (DataBinder.Eval(l_riContainer.DataItem, "Id")).ToString();
			}

			/// <summary>
			/// Handles the data binding event of the Description label in the templated item.
			/// </summary>
			/// <param name="sender">The source of the event.</param>
			/// <param name="args">Additional information about the event.</param>
			private void BindDescription(Object sender, EventArgs args)
			{
				Label l_lDescriptionData = (Label)sender;
				RepeaterItem l_riContainer = (RepeaterItem)l_lDescriptionData.NamingContainer;
				l_lDescriptionData.Text = (DataBinder.Eval(l_riContainer.DataItem, "Description")).ToString();
			}
		}

		/// <summary>
		/// The templte used by DataList to render its items.
		/// </summary>
		public class DataListTemplate : ITemplate
		{
			#region ITemplate Members
			/// <summary>
			/// Implements ITemplate.instantiateIn(..)
			/// Adds an item to the data list.
			/// </summary>
			/// <param name="a_container">The controls to add the templated item to.</param>
			public void InstantiateIn(Control a_container)
			{
				//Header labels:				
				//~~~~~~~~~~~
				Label l_lIdHeader = new Label();
				l_lIdHeader.Text = "ID: ";
				l_lIdHeader.Font.Bold = true;
				Label l_lDescrptionHeader = new Label();
				l_lDescrptionHeader.Text = "	Description: ";
				l_lDescrptionHeader.Font.Bold = true;

				//Data labels:
				//~~~~~~~~~
				Label l_lIdData = new Label();
				l_lIdData.DataBinding += new EventHandler(this.BindId);
				Label l_lDescriptionData = new Label();
				l_lDescriptionData.DataBinding += new EventHandler(this.BindDescription);

				//The complete panel.
				Panel l_pItemPanel = new Panel();
				l_pItemPanel.Controls.Add(l_lIdHeader);
				l_pItemPanel.Controls.Add(l_lIdData);
				l_pItemPanel.Controls.Add(l_lDescrptionHeader);
				l_pItemPanel.Controls.Add(l_lDescriptionData);

				a_container.Controls.Add(l_pItemPanel);
			}
			#endregion

			/// <summary>
			/// Handles the data binding event of the Id label in the templated item.
			/// </summary>
			/// <param name="sender">The source of the event.</param>
			/// <param name="args">Additional information about the event.</param>
			private void BindId(Object sender, EventArgs args)
			{
				Label l_lIddata = (Label)sender;
				DataListItem l_dliContainer = (DataListItem)l_lIddata.NamingContainer;
				l_lIddata.Text = (DataBinder.Eval(l_dliContainer.DataItem, "Id")).ToString();
			}

			/// <summary>
			/// Handles the data binding event of the Description label in the templated item.
			/// </summary>
			/// <param name="sender">The source of the event.</param>
			/// <param name="args">Additional information about the event.</param>
			private void BindDescription(Object sender, EventArgs args)
			{
				Label l_lDescriptionData = (Label)sender;
				DataListItem l_dliContainer = (DataListItem)l_lDescriptionData.NamingContainer;
				l_lDescriptionData.Text = (DataBinder.Eval(l_dliContainer.DataItem, "Description")).ToString();
			}
		}

	}
}
