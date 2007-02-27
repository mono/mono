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
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

using System.Reflection;
using System.Collections;
using System.Data;

namespace GHTTests.System_Web_dll.System_Web_UI
{
	public class Control_ViewState
		: GHTControlBase
	{
		protected System.Web.UI.WebControls.Button Button1;
		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e) 
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			InitializeComponent();
			base.OnInit(e);
		}
		
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() 
		{    
			this.Load += new System.EventHandler(this.Page_Load);

		}
		#endregion

		// =======================
		// Dynamic Control.ID
		// =======================
		// In order to catch these values the dynamically generated controls 
		// needs to be re-generated at Page_Load. 
		// The important thing is to assign the same ID to each control. 
		// The ViewState uses the ID property of the Control objects to reinstate the values. 
		//
		// =======================
		// Page.IsPostBack
		// =======================
		// We set the contrro//s tested member with a value 
		// only at the first time the page is loaded
		//
		//
		private void Page_Load(object sender, EventArgs e)
		{
			HtmlForm form1 = (HtmlForm) (HtmlForm)this.FindControl("Form1");
			this.GHTTestBegin(form1);
			this.GHTSubTestBegin("Check PostBack");
			try
			{
				if (this.Page.IsPostBack)
				{
					this.GHTSubTestAddResult("PostBack Worked!!!");
				}
			}
			catch (Exception exception49)
			{
				// ProjectData.SetProjectError(exception49);
				Exception exception1 = exception49;
				this.GHTSubTestAddResult("Unxpected " + exception1.GetType().Name + " exception was caught-" + exception1.Message);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			this.GHTSubTestBegin("AdRotator.KeywordFilter,Target");
			try
			{
				AdRotator rotator1 = new AdRotator();
				rotator1.ID = "objAdRotatorAll";
				base.GHTActiveForm.Controls.Add(rotator1);
				if (!this.Page.IsPostBack)
				{
					rotator1.KeywordFilter = "test";
					rotator1.Target = "_blank";
				}
				else
				{
					this.GHTSubTestAddResult(rotator1.KeywordFilter);
					this.GHTSubTestAddResult(rotator1.Target);
				}
			}
			catch (Exception exception50)
			{
				// ProjectData.SetProjectError(exception50);
				Exception exception2 = exception50;
				this.GHTSubTestAddResult("Unxpected " + exception2.GetType().Name + " exception was caught-" + exception2.Message);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			Label label1 = new Label();
			this.GHTSubTestBegin("Style.BorderColor,BorderWidth,BorderStyle,CssClass,ForeColor,Height,Width,BackColor");
			try
			{
				label1.ID = "objStyleLabelAll";
				base.GHTActiveForm.Controls.Add(label1);
				if (!this.Page.IsPostBack)
				{
					label1.Style["BorderColor"] = "ffffff";
					label1.Style["ForeColor"] = "ffffff";
					label1.Style["BackColor"] = "ffffff";
					label1.Style["BorderWidth"] = "2";
					label1.Style["BorderStyle"] = "3";
					label1.Style["CssClass"] = "CssClass";
					label1.Style["Height"] = "2";
					label1.Style["Width"] = "2";
				}
				else
				{
					this.GHTSubTestAddResult(label1.Style["BorderColor"]);
					this.GHTSubTestAddResult(label1.Style["ForeColor"]);
					this.GHTSubTestAddResult(label1.Style["BackColor"]);
					this.GHTSubTestAddResult(label1.Style["BorderWidth"]);
					this.GHTSubTestAddResult(label1.Style["BorderStyle"]);
					this.GHTSubTestAddResult(label1.Style["CssClass"]);
					this.GHTSubTestAddResult(label1.Style["Height"]);
					this.GHTSubTestAddResult(label1.Style["Width"]);
				}
			}
			catch (Exception exception51)
			{
				// ProjectData.SetProjectError(exception51);
				Exception exception3 = exception51;
				this.GHTSubTestAddResult("Unxpected " + exception3.GetType().Name + " exception was caught-" + exception3.Message);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			this.GHTSubTestBegin("FontInfo.Underline,Italic,Names,Overline,Size,Strikeout,Bold");
			try
			{
				if (!this.Page.IsPostBack)
				{
					label1.Font.Underline = true;
					label1.Font.Italic = true;
					label1.Font.Names.SetValue("myfont", 1);
					label1.Font.Overline = true;
					label1.Font.Size = FontUnit.Medium;
					label1.Font.Strikeout = true;
					label1.Font.Bold = true;
				}
				else
				{
					this.GHTSubTestAddResult(label1.Font.Underline.ToString());
					this.GHTSubTestAddResult(label1.Font.Italic.ToString());
					this.GHTSubTestAddResult((string)(label1.Font.Names.GetValue(1)));
					this.GHTSubTestAddResult(label1.Font.Overline.ToString());
					this.GHTSubTestAddResult(label1.Font.Size.ToString());
					this.GHTSubTestAddResult(label1.Font.Strikeout.ToString());
					this.GHTSubTestAddResult(label1.Font.Bold.ToString());
				}
			}
#if NET_2_0
			catch (IndexOutOfRangeException exception52)
			{
				this.GHTSubTestAddResult("Test passed");
			}
#else
			catch (Exception exception52)
			{
				// ProjectData.SetProjectError(exception52);
				Exception exception4 = exception52;
				this.GHTSubTestAddResult("Unxpected " + exception4.GetType().Name + " exception was caught-" + exception4.Message);
				// ProjectData.ClearProjectError();
			}
#endif
			this.GHTSubTestEnd();
			this.GHTSubTestBegin("Control.Visible");
			try
			{
				Button button1 = new Button();
				button1.ID = "objControlAll";
				base.GHTActiveForm.Controls.Add(button1);
				if (!this.Page.IsPostBack)
				{
					button1.Visible = false;
				}
				else
				{
					this.GHTSubTestAddResult(button1.Visible.ToString());
				}
			}
			catch (Exception exception53)
			{
				// ProjectData.SetProjectError(exception53);
				Exception exception5 = exception53;
				this.GHTSubTestAddResult("Unxpected " + exception5.GetType().Name + " exception was caught-" + exception5.Message);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			this.GHTSubTestBegin("WebControl.AccessKey,Enabled,TabIndex,ToolTip");
			try
			{
				Button button2 = new Button();
				button2.ID = "objWebControlAll";
				base.GHTActiveForm.Controls.Add(button2);
				if (!this.Page.IsPostBack)
				{
					button2.AccessKey = "F";
					button2.Enabled = false;
					button2.TabIndex = 100;
					button2.ToolTip = "ToolTip";
				}
				else
				{
					this.GHTSubTestAddResult(button2.AccessKey);
					this.GHTSubTestAddResult(button2.Enabled.ToString());
					this.GHTSubTestAddResult(button2.TabIndex.ToString());
					this.GHTSubTestAddResult(button2.ToolTip);
				}
			}
			catch (Exception exception54)
			{
				// ProjectData.SetProjectError(exception54);
				Exception exception6 = exception54;
				this.GHTSubTestAddResult("Unxpected " + exception6.GetType().Name + " exception was caught-" + exception6.Message);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			this.GHTSubTestBegin("Button.CausesValidation,CommandArgument,CommandName,Text");
			try
			{
				Button button3 = new Button();
				button3.ID = "objButtonAll";
				base.GHTActiveForm.Controls.Add(button3);
				if (!this.Page.IsPostBack)
				{
					button3.CausesValidation = true;
					button3.CommandArgument = "test";
					button3.CommandName = "test";
					button3.Text = "test";
				}
				else
				{
					this.GHTSubTestAddResult(button3.CausesValidation.ToString());
					this.GHTSubTestAddResult(button3.CommandArgument);
					this.GHTSubTestAddResult(button3.CommandName);
					this.GHTSubTestAddResult(button3.Text);
				}
			}
			catch (Exception exception55)
			{
				// ProjectData.SetProjectError(exception55);
				Exception exception7 = exception55;
				this.GHTSubTestAddResult("Unxpected " + exception7.GetType().Name + " exception was caught-" + exception7.Message);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			DataGrid grid1 = new DataGrid();
			grid1.ID = "objDataGrid";
			grid1.AutoGenerateColumns = false;
			BoundColumn column1 = new BoundColumn();
			column1.HeaderText = "IntegerValue";
			column1.DataField = "IntegerValue";
			grid1.Columns.Add(column1);
			column1 = new BoundColumn();
			column1.HeaderText = "StringValue";
			column1.DataField = "StringValue";
			grid1.Columns.Add(column1);
			column1 = new BoundColumn();
			column1.HeaderText = "CurrencyValue";
			column1.DataField = "CurrencyValue";
			grid1.Columns.Add(column1);
			HyperLinkColumn column4 = new HyperLinkColumn();
			column4.HeaderText = "objHyperLinkColumn";
			grid1.Columns.Add(column4);
			ButtonColumn column2 = new ButtonColumn();
			column2.HeaderText = "ButtonColumn";
			grid1.Columns.Add(column2);
			EditCommandColumn column3 = new EditCommandColumn();
			column3.HeaderText = "EditCommandColumn";
			grid1.Columns.Add(column3);
			grid1.DataSource = this.CreateDataSource();
			grid1.DataBind();
			base.GHTActiveForm.Controls.Add(grid1);
			this.GHTSubTestBegin("BoundColumn.All");
			try
			{
				column1 = (BoundColumn) grid1.Columns[2];
				if (!this.Page.IsPostBack)
				{
					column1.DataFormatString = "{0:C}";
					column1.ReadOnly = true;
					column1.DataField = "IntegerValue";
				}
				else
				{
					this.GHTSubTestAddResult(column1.DataFormatString);
					this.GHTSubTestAddResult(column1.ReadOnly.ToString());
					this.GHTSubTestAddResult(column1.DataField);
				}
			}
			catch (Exception exception56)
			{
				// ProjectData.SetProjectError(exception56);
				Exception exception8 = exception56;
				this.GHTSubTestAddResult("Unxpected " + exception8.GetType().Name + " exception was caught-" + exception8.Message);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			this.GHTSubTestBegin("HyperLinkColumn.All");
			try
			{
				if (!this.Page.IsPostBack)
				{
					column4.Text = "test";
					column4.DataNavigateUrlFormatString = "test.aspx?id={0}";
					column4.Target = "_blank";
					column4.NavigateUrl = "test";
					column4.DataTextField = "StringValue";
					column4.DataNavigateUrlField = "StringValue";
					column4.DataTextFormatString = "{0:C}";
				}
				else
				{
					this.GHTSubTestAddResult(column4.Text);
					this.GHTSubTestAddResult(column4.DataNavigateUrlFormatString);
					this.GHTSubTestAddResult(column4.Target);
					this.GHTSubTestAddResult(column4.NavigateUrl);
					this.GHTSubTestAddResult(column4.DataTextField);
					this.GHTSubTestAddResult(column4.DataNavigateUrlField);
					this.GHTSubTestAddResult(column4.DataTextFormatString);
				}
			}
			catch (Exception exception57)
			{
				// ProjectData.SetProjectError(exception57);
				Exception exception9 = exception57;
				this.GHTSubTestAddResult("Unxpected " + exception9.GetType().Name + " exception was caught-" + exception9.Message);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			this.GHTSubTestBegin("ButtonColumn.All");
			try
			{
				column2 = (ButtonColumn) grid1.Columns[4];
				if (!this.Page.IsPostBack)
				{
					column2.DataTextField = "StringValue";
					//column2.ButtonType = (ButtonColumnType) "test";
					column2.DataTextFormatString = "{0:C}";
				}
				else
				{
					this.GHTSubTestAddResult(column2.DataTextField);
					this.GHTSubTestAddResult(((int) column2.ButtonType).ToString());
					this.GHTSubTestAddResult(column2.DataTextFormatString);
				}
			}
			catch (Exception exception58)
			{
				// ProjectData.SetProjectError(exception58);
				Exception exception10 = exception58;
				this.GHTSubTestAddResult("Unxpected " + exception10.GetType().Name + " exception was caught-" + exception10.Message);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			this.GHTSubTestBegin("EditCommandColumn.All");
			try
			{
				column3 = (EditCommandColumn) grid1.Columns[5];
				if (!this.Page.IsPostBack)
				{
					column3.UpdateText = "test";
					column3.CancelText = "test";
					column3.EditText = "test";
					column3.ButtonType = ButtonColumnType.PushButton;
				}
				else
				{
					this.GHTSubTestAddResult(column3.UpdateText);
					this.GHTSubTestAddResult(column3.CancelText);
					this.GHTSubTestAddResult(column3.EditText);
					this.GHTSubTestAddResult(((int) column3.ButtonType).ToString());
				}
			}
			catch (Exception exception59)
			{
				// ProjectData.SetProjectError(exception59);
				Exception exception11 = exception59;
				this.GHTSubTestAddResult("Unxpected " + exception11.GetType().Name + " exception was caught-" + exception11.Message);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			this.GHTSubTestBegin("Calendar.All");
			try
			{
				Calendar calendar1 = new Calendar();
				calendar1.ID = "objCalendarAll";
				base.GHTActiveForm.Controls.Add(calendar1);
				if (!this.Page.IsPostBack)
				{
					calendar1.ShowDayHeader = true;
					calendar1.FirstDayOfWeek = FirstDayOfWeek.Tuesday;
					calendar1.SelectWeekText = "SelectWeekText";
					calendar1.CellSpacing = 4;
					calendar1.CellPadding = 6;
					calendar1.SelectMonthText = "SelectMonthText";
					calendar1.VisibleDate = DateTime.Now;
					calendar1.DayNameFormat = DayNameFormat.FirstTwoLetters;
					calendar1.ShowGridLines = true;
					calendar1.TodaysDate = DateTime.Now.AddDays(1);
					calendar1.ShowNextPrevMonth = true;
					calendar1.ShowTitle = true;
					calendar1.TitleFormat = TitleFormat.MonthYear;
					calendar1.NextMonthText = "NextMonthText";
					calendar1.NextPrevFormat = NextPrevFormat.FullMonth;
					calendar1.PrevMonthText = "PrevMonthText";
					calendar1.SelectionMode = CalendarSelectionMode.DayWeekMonth;
				}
				else
				{
					this.GHTSubTestAddResult(calendar1.ShowDayHeader.ToString());
					this.GHTSubTestAddResult(((int) calendar1.FirstDayOfWeek).ToString());
					this.GHTSubTestAddResult(calendar1.SelectWeekText);
					this.GHTSubTestAddResult(calendar1.CellSpacing.ToString());
					this.GHTSubTestAddResult(calendar1.CellPadding.ToString());
					this.GHTSubTestAddResult(calendar1.SelectMonthText);
					this.GHTSubTestAddResult(calendar1.VisibleDate.ToString());
					this.GHTSubTestAddResult(((int) calendar1.DayNameFormat).ToString());
					this.GHTSubTestAddResult(calendar1.ShowGridLines.ToString());
					this.GHTSubTestAddResult(calendar1.TodaysDate.ToString());
					this.GHTSubTestAddResult(calendar1.ShowNextPrevMonth.ToString());
					this.GHTSubTestAddResult(calendar1.ShowTitle.ToString());
					this.GHTSubTestAddResult(((int) calendar1.TitleFormat).ToString());
					this.GHTSubTestAddResult(calendar1.NextMonthText);
					this.GHTSubTestAddResult(((int) calendar1.NextPrevFormat).ToString());
					this.GHTSubTestAddResult(calendar1.PrevMonthText);
					this.GHTSubTestAddResult(((int) calendar1.SelectionMode).ToString());
				}
			}
			catch (Exception exception60)
			{
				// ProjectData.SetProjectError(exception60);
				Exception exception12 = exception60;
				this.GHTSubTestAddResult("Unxpected " + exception12.GetType().Name + " exception was caught-" + exception12.Message);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			this.GHTSubTestBegin("CheckBox.TextAlign,Text,Checked,AutoPostBack");
			try
			{
				CheckBox box2 = new CheckBox();
				box2.ID = "objCheckBoxAll";
				base.GHTActiveForm.Controls.Add(box2);
				if (!this.Page.IsPostBack)
				{
					box2.TextAlign = TextAlign.Left;
					box2.Text = "test";
					box2.Checked = true;
					box2.Checked = true;
				}
				else
				{
					this.GHTSubTestAddResult(((int) box2.TextAlign).ToString());
					this.GHTSubTestAddResult(box2.Text);
					this.GHTSubTestAddResult(box2.Checked.ToString());
					this.GHTSubTestAddResult(box2.Checked.ToString());
				}
			}
			catch (Exception exception61)
			{
				// ProjectData.SetProjectError(exception61);
				Exception exception13 = exception61;
				this.GHTSubTestAddResult("Unxpected " + exception13.GetType().Name + " exception was caught-" + exception13.Message);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			this.GHTSubTestBegin("CheckBoxList.RepeatColumns");
			try
			{
				CheckBoxList list1 = new CheckBoxList();
				list1.ID = "objCheckBoxListRepeatColumns";
				base.GHTActiveForm.Controls.Add(list1);
				if (!this.Page.IsPostBack)
				{
					list1.RepeatColumns = 2;
				}
				else
				{
					this.GHTSubTestAddResult(list1.RepeatColumns.ToString());
				}
			}
			catch (Exception exception62)
			{
				// ProjectData.SetProjectError(exception62);
				Exception exception14 = exception62;
				this.GHTSubTestAddResult("Unxpected " + exception14.GetType().Name + " exception was caught-" + exception14.Message);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			this.GHTSubTestBegin("CheckBoxList.TextAlign");
			try
			{
				CheckBoxList list2 = new CheckBoxList();
				list2.ID = "objCheckBoxListTextAlign";
				base.GHTActiveForm.Controls.Add(list2);
				if (!this.Page.IsPostBack)
				{
					list2.TextAlign = TextAlign.Right;
				}
				else
				{
					this.GHTSubTestAddResult(((int) list2.TextAlign).ToString());
				}
			}
			catch (Exception exception63)
			{
				// ProjectData.SetProjectError(exception63);
				Exception exception15 = exception63;
				this.GHTSubTestAddResult("Unxpected " + exception15.GetType().Name + " exception was caught-" + exception15.Message);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			this.GHTSubTestBegin("CheckBoxList.RepeatDirection");
			try
			{
				CheckBoxList list3 = new CheckBoxList();
				list3.ID = "objCheckBoxListRepeatDirection";
				base.GHTActiveForm.Controls.Add(list3);
				if (!this.Page.IsPostBack)
				{
					list3.RepeatDirection = RepeatDirection.Horizontal;
				}
				else
				{
					this.GHTSubTestAddResult(((int) list3.RepeatDirection).ToString());
				}
			}
			catch (Exception exception64)
			{
				// ProjectData.SetProjectError(exception64);
				Exception exception16 = exception64;
				this.GHTSubTestAddResult("Unxpected " + exception16.GetType().Name + " exception was caught-" + exception16.Message);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			this.GHTSubTestBegin("CheckBoxList.RepeatLayout");
			try
			{
				CheckBoxList list4 = new CheckBoxList();
				list4.ID = "objCheckBoxListRepeatLayout";
				base.GHTActiveForm.Controls.Add(list4);
				if (!this.Page.IsPostBack)
				{
					list4.RepeatLayout = RepeatLayout.Table;
				}
				else
				{
					this.GHTSubTestAddResult(((int) list4.RepeatLayout).ToString());
				}
			}
			catch (Exception exception65)
			{
				// ProjectData.SetProjectError(exception65);
				Exception exception17 = exception65;
				this.GHTSubTestAddResult("Unxpected " + exception17.GetType().Name + " exception was caught-" + exception17.Message);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			TextBox box1 = new TextBox();
			box1.ID = "objControlToValidate";
			base.GHTActiveForm.Controls.Add(box1);
			this.GHTSubTestBegin("CompareValidator.Operator");
			try
			{
				CompareValidator validator1 = new CompareValidator();
				validator1.ID = "objCompareValidatorOperator";
				validator1.ControlToValidate = "objControlToValidate";
				base.GHTActiveForm.Controls.Add(validator1);
				if (!this.Page.IsPostBack)
				{
					validator1.Operator = ValidationCompareOperator.GreaterThan;
				}
				else
				{
					this.GHTSubTestAddResult(((int) validator1.Operator).ToString());
				}
			}
			catch (Exception exception66)
			{
				// ProjectData.SetProjectError(exception66);
				Exception exception18 = exception66;
				this.GHTSubTestAddResult("Unxpected " + exception18.GetType().Name + " exception was caught-" + exception18.Message);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			this.GHTSubTestBegin("CompareValidator.ControlToCompare");
			try
			{
				CompareValidator validator2 = new CompareValidator();
				validator2.ID = "objCompareValidatorControlToCompare";
				validator2.ControlToValidate = "objControlToValidate";
				base.GHTActiveForm.Controls.Add(validator2);
				if (!this.Page.IsPostBack)
				{
					validator2.ControlToValidate = "objControlToValidate";
				}
				else
				{
					this.GHTSubTestAddResult(validator2.ControlToValidate);
				}
			}
			catch (Exception exception67)
			{
				// ProjectData.SetProjectError(exception67);
				Exception exception19 = exception67;
				this.GHTSubTestAddResult("Unxpected " + exception19.GetType().Name + " exception was caught-" + exception19.Message);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			this.GHTSubTestBegin("CompareValidator.ValueToCompare");
			try
			{
				CompareValidator validator3 = new CompareValidator();
				validator3.ID = "objCompareValidatorValueToCompare";
				validator3.ControlToValidate = "objControlToValidate";
				base.GHTActiveForm.Controls.Add(validator3);
				if (!this.Page.IsPostBack)
				{
					validator3.ValueToCompare = "test";
				}
				else
				{
					this.GHTSubTestAddResult(validator3.ValueToCompare);
				}
			}
			catch (Exception exception68)
			{
				// ProjectData.SetProjectError(exception68);
				Exception exception20 = exception68;
				this.GHTSubTestAddResult("Unxpected " + exception20.GetType().Name + " exception was caught-" + exception20.Message);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			this.GHTSubTestBegin("HtmlButton.CausesValidation");
			try
			{
				HtmlButton button4 = new HtmlButton();
				button4.ID = "objHtmlButtonCausesValidation";
				base.GHTActiveForm.Controls.Add(button4);
				if (!this.Page.IsPostBack)
				{
					button4.CausesValidation = true;
				}
				else
				{
					this.GHTSubTestAddResult(button4.CausesValidation.ToString());
				}
			}
			catch (Exception exception69)
			{
				// ProjectData.SetProjectError(exception69);
				Exception exception21 = exception69;
				this.GHTSubTestAddResult("Unxpected " + exception21.GetType().Name + " exception was caught-" + exception21.Message);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			this.GHTSubTestBegin("HtmlInputImage.CausesValidation");
			try
			{
				HtmlInputImage image1 = new HtmlInputImage();
				image1.ID = "objHtmlInputImageCausesValidation";
				base.GHTActiveForm.Controls.Add(image1);
				if (!this.Page.IsPostBack)
				{
					image1.CausesValidation = true;
				}
				else
				{
					this.GHTSubTestAddResult(image1.CausesValidation.ToString());
				}
			}
			catch (Exception exception70)
			{
				// ProjectData.SetProjectError(exception70);
				Exception exception22 = exception70;
				this.GHTSubTestAddResult("Unxpected " + exception22.GetType().Name + " exception was caught-" + exception22.Message);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			this.GHTSubTestBegin("HtmlInputButton.CausesValidation");
			try
			{
				HtmlInputButton button5 = new HtmlInputButton();
				button5.ID = "objHtmlInputButtonCausesValidation";
				base.GHTActiveForm.Controls.Add(button5);
				if (!this.Page.IsPostBack)
				{
					button5.CausesValidation = true;
				}
				else
				{
					this.GHTSubTestAddResult(button5.CausesValidation.ToString());
				}
			}
			catch (Exception exception71)
			{
				// ProjectData.SetProjectError(exception71);
				Exception exception23 = exception71;
				this.GHTSubTestAddResult("Unxpected " + exception23.GetType().Name + " exception was caught-" + exception23.Message);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			this.GHTSubTestBegin("HyperLink.Text");
			try
			{
				HyperLink link1 = new HyperLink();
				link1.ID = "objHyperLinkText";
				base.GHTActiveForm.Controls.Add(link1);
				if (!this.Page.IsPostBack)
				{
					link1.Text = "test";
				}
				else
				{
					this.GHTSubTestAddResult(link1.Text);
				}
			}
			catch (Exception exception72)
			{
				// ProjectData.SetProjectError(exception72);
				Exception exception24 = exception72;
				this.GHTSubTestAddResult("Unxpected " + exception24.GetType().Name + " exception was caught-" + exception24.Message);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			this.GHTSubTestBegin("HyperLink.Target");
			try
			{
				HyperLink link2 = new HyperLink();
				link2.ID = "objHyperLinkTarget";
				base.GHTActiveForm.Controls.Add(link2);
				if (!this.Page.IsPostBack)
				{
					link2.Target = "_blank";
				}
				else
				{
					this.GHTSubTestAddResult(link2.Target);
				}
			}
			catch (Exception exception73)
			{
				// ProjectData.SetProjectError(exception73);
				Exception exception25 = exception73;
				this.GHTSubTestAddResult("Unxpected " + exception25.GetType().Name + " exception was caught-" + exception25.Message);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			this.GHTSubTestBegin("HyperLink.ImageUrl");
			try
			{
				HyperLink link3 = new HyperLink();
				link3.ID = "objHyperLinkImageUrl";
				base.GHTActiveForm.Controls.Add(link3);
				if (!this.Page.IsPostBack)
				{
					link3.ImageUrl = "test";
				}
				else
				{
					this.GHTSubTestAddResult(link3.ImageUrl);
				}
			}
			catch (Exception exception74)
			{
				// ProjectData.SetProjectError(exception74);
				Exception exception26 = exception74;
				this.GHTSubTestAddResult("Unxpected " + exception26.GetType().Name + " exception was caught-" + exception26.Message);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			this.GHTSubTestBegin("HyperLink.NavigateUrl");
			try
			{
				HyperLink link4 = new HyperLink();
				link4.ID = "objHyperLinkNavigateUrl";
				base.GHTActiveForm.Controls.Add(link4);
				if (!this.Page.IsPostBack)
				{
					link4.NavigateUrl = "test";
				}
				else
				{
					this.GHTSubTestAddResult(link4.NavigateUrl);
				}
			}
			catch (Exception exception75)
			{
				// ProjectData.SetProjectError(exception75);
				Exception exception27 = exception75;
				this.GHTSubTestAddResult("Unxpected " + exception27.GetType().Name + " exception was caught-" + exception27.Message);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			this.GHTSubTestBegin("Image.AlternateText");
			try
			{
				Image image2 = new Image();
				image2.ID = "objImageAlternateText";
				base.GHTActiveForm.Controls.Add(image2);
				if (!this.Page.IsPostBack)
				{
					image2.AlternateText = "test";
				}
				else
				{
					this.GHTSubTestAddResult(image2.AlternateText);
				}
			}
			catch (Exception exception76)
			{
				// ProjectData.SetProjectError(exception76);
				Exception exception28 = exception76;
				this.GHTSubTestAddResult("Unxpected " + exception28.GetType().Name + " exception was caught-" + exception28.Message);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			this.GHTSubTestBegin("Image.ImageAlign");
			try
			{
				Image image3 = new Image();
				image3.ID = "objImageImageAlign";
				base.GHTActiveForm.Controls.Add(image3);
				if (!this.Page.IsPostBack)
				{
					image3.ImageAlign = ImageAlign.Right;
				}
				else
				{
					this.GHTSubTestAddResult(((int) image3.ImageAlign).ToString());
				}
			}
			catch (Exception exception77)
			{
				// ProjectData.SetProjectError(exception77);
				Exception exception29 = exception77;
				this.GHTSubTestAddResult("Unxpected " + exception29.GetType().Name + " exception was caught-" + exception29.Message);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			this.GHTSubTestBegin("Image.ImageUrl");
			try
			{
				Image image4 = new Image();
				image4.ID = "objImageImageUrl";
				base.GHTActiveForm.Controls.Add(image4);
				if (!this.Page.IsPostBack)
				{
					image4.ImageUrl = "test";
				}
				else
				{
					this.GHTSubTestAddResult(image4.ImageUrl);
				}
			}
			catch (Exception exception78)
			{
				// ProjectData.SetProjectError(exception78);
				Exception exception30 = exception78;
				this.GHTSubTestAddResult("Unxpected " + exception30.GetType().Name + " exception was caught-" + exception30.Message);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			this.GHTSubTestBegin("ImageButton.CommandName");
			try
			{
				ImageButton button6 = new ImageButton();
				button6.ID = "objImageButtonCommandName";
				base.GHTActiveForm.Controls.Add(button6);
				if (!this.Page.IsPostBack)
				{
					button6.CommandName = "test";
				}
				else
				{
					this.GHTSubTestAddResult(button6.CommandName);
				}
			}
			catch (Exception exception79)
			{
				// ProjectData.SetProjectError(exception79);
				Exception exception31 = exception79;
				this.GHTSubTestAddResult("Unxpected " + exception31.GetType().Name + " exception was caught-" + exception31.Message);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			this.GHTSubTestBegin("ImageButton.CommandArgument");
			try
			{
				ImageButton button7 = new ImageButton();
				button7.ID = "objImageButtonCommandArgument";
				base.GHTActiveForm.Controls.Add(button7);
				if (!this.Page.IsPostBack)
				{
					button7.CommandArgument = "test";
				}
				else
				{
					this.GHTSubTestAddResult(button7.CommandArgument);
				}
			}
			catch (Exception exception80)
			{
				// ProjectData.SetProjectError(exception80);
				Exception exception32 = exception80;
				this.GHTSubTestAddResult("Unxpected " + exception32.GetType().Name + " exception was caught-" + exception32.Message);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			this.GHTSubTestBegin("ImageButton.CommandName");
			try
			{
				ImageButton button8 = new ImageButton();
				button8.ID = "objImageButtonCausesValidation";
				base.GHTActiveForm.Controls.Add(button8);
				if (!this.Page.IsPostBack)
				{
					button8.CausesValidation = true;
				}
				else
				{
					this.GHTSubTestAddResult(button8.CausesValidation.ToString());
				}
			}
			catch (Exception exception81)
			{
				// ProjectData.SetProjectError(exception81);
				Exception exception33 = exception81;
				this.GHTSubTestAddResult("Unxpected " + exception33.GetType().Name + " exception was caught-" + exception33.Message);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			this.GHTSubTestBegin("Label.Text");
			try
			{
				Label label2 = new Label();
				label2.ID = "objLabelText";
				base.GHTActiveForm.Controls.Add(label2);
				if (!this.Page.IsPostBack)
				{
					label2.Text = "test";
				}
				else
				{
					this.GHTSubTestAddResult(label2.Text);
				}
			}
			catch (Exception exception82)
			{
				// ProjectData.SetProjectError(exception82);
				Exception exception34 = exception82;
				this.GHTSubTestAddResult("Unxpected " + exception34.GetType().Name + " exception was caught-" + exception34.Message);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			this.GHTSubTestBegin("LinkButton.CausesValidation");
			try
			{
				LinkButton button9 = new LinkButton();
				button9.ID = "objLinkButtonCausesValidation";
				base.GHTActiveForm.Controls.Add(button9);
				if (!this.Page.IsPostBack)
				{
					button9.CausesValidation = true;
				}
				else
				{
					this.GHTSubTestAddResult(button9.CausesValidation.ToString());
				}
			}
			catch (Exception exception83)
			{
				// ProjectData.SetProjectError(exception83);
				Exception exception35 = exception83;
				this.GHTSubTestAddResult("Unxpected " + exception35.GetType().Name + " exception was caught-" + exception35.Message);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			this.GHTSubTestBegin("LinkButton.CommandName");
			try
			{
				LinkButton button10 = new LinkButton();
				button10.ID = "objLinkButtonCommandName";
				base.GHTActiveForm.Controls.Add(button10);
				if (!this.Page.IsPostBack)
				{
					button10.CommandName = "test";
				}
				else
				{
					this.GHTSubTestAddResult(button10.CommandName);
				}
			}
			catch (Exception exception84)
			{
				// ProjectData.SetProjectError(exception84);
				Exception exception36 = exception84;
				this.GHTSubTestAddResult("Unxpected " + exception36.GetType().Name + " exception was caught-" + exception36.Message);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			this.GHTSubTestBegin("LinkButton.CommandArgument");
			try
			{
				LinkButton button11 = new LinkButton();
				button11.ID = "objLinkButtonCommandArgument";
				base.GHTActiveForm.Controls.Add(button11);
				if (!this.Page.IsPostBack)
				{
					button11.CommandArgument = "test";
				}
				else
				{
					this.GHTSubTestAddResult(button11.CommandArgument);
				}
			}
			catch (Exception exception85)
			{
				// ProjectData.SetProjectError(exception85);
				Exception exception37 = exception85;
				this.GHTSubTestAddResult("Unxpected " + exception37.GetType().Name + " exception was caught-" + exception37.Message);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			this.GHTSubTestBegin("LinkButton.Text");
			try
			{
				LinkButton button12 = new LinkButton();
				button12.ID = "objLinkButtonText";
				base.GHTActiveForm.Controls.Add(button12);
				if (!this.Page.IsPostBack)
				{
					button12.Text = "test";
				}
				else
				{
					this.GHTSubTestAddResult(button12.Text);
				}
			}
			catch (Exception exception86)
			{
				// ProjectData.SetProjectError(exception86);
				Exception exception38 = exception86;
				this.GHTSubTestAddResult("Unxpected " + exception38.GetType().Name + " exception was caught-" + exception38.Message);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			this.GHTSubTestBegin("ListBox.All");
			try
			{
				ListBox box3 = new ListBox();
				box3.ID = "objListBoxAll";
				base.GHTActiveForm.Controls.Add(box3);
				if (!this.Page.IsPostBack)
				{
					box3.SelectionMode = ListSelectionMode.Multiple;
					box3.Rows = 2;
				}
				else
				{
					this.GHTSubTestAddResult(((int) box3.SelectionMode).ToString());
					this.GHTSubTestAddResult(box3.Rows.ToString());
				}
			}
			catch (Exception exception87)
			{
				// ProjectData.SetProjectError(exception87);
				Exception exception39 = exception87;
				this.GHTSubTestAddResult("Unxpected " + exception39.GetType().Name + " exception was caught-" + exception39.Message);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			this.GHTSubTestBegin("ListControl.All");
			try
			{
				ListBox box4 = new ListBox();
				box4.ID = "objListControlAll";
				box4.DataSource = this.CreateDataSource();
				box4.DataBind();
				base.GHTActiveForm.Controls.Add(box4);
				if (!this.Page.IsPostBack)
				{
					box4.AutoPostBack = true;
					box4.DataMember = "test";
					box4.DataTextField = "StringValue";
					box4.DataTextFormatString = "{0:C}";
					box4.DataValueField = "StringValue";
				}
				else
				{
					this.GHTSubTestAddResult(box4.AutoPostBack.ToString());
					this.GHTSubTestAddResult(box4.DataMember);
					this.GHTSubTestAddResult(box4.DataTextField);
					this.GHTSubTestAddResult(box4.DataTextFormatString);
					this.GHTSubTestAddResult(box4.DataValueField);
				}
			}
			catch (Exception exception88)
			{
				// ProjectData.SetProjectError(exception88);
				Exception exception40 = exception88;
				this.GHTSubTestAddResult("Unxpected " + exception40.GetType().Name + " exception was caught-" + exception40.Message);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			this.GHTSubTestBegin("Literal.Text");
			try
			{
				Literal literal1 = new Literal();
				literal1.ID = "objLiteralText";
				base.GHTActiveForm.Controls.Add(literal1);
				if (!this.Page.IsPostBack)
				{
					literal1.Text = "test";
				}
				else
				{
					this.GHTSubTestAddResult(literal1.Text);
				}
			}
			catch (Exception exception89)
			{
				// ProjectData.SetProjectError(exception89);
				Exception exception41 = exception89;
				this.GHTSubTestAddResult("Unxpected " + exception41.GetType().Name + " exception was caught-" + exception41.Message);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			this.GHTSubTestBegin("Panel.All");
			try
			{
				Panel panel1 = new Panel();
				panel1.ID = "objPanelAll";
				base.GHTActiveForm.Controls.Add(panel1);
				if (!this.Page.IsPostBack)
				{
					panel1.BackImageUrl = "test";
					panel1.HorizontalAlign = HorizontalAlign.Right;
					panel1.Wrap = true;
				}
				else
				{
					this.GHTSubTestAddResult(panel1.BackImageUrl);
					this.GHTSubTestAddResult(((int) panel1.HorizontalAlign).ToString());
					this.GHTSubTestAddResult(panel1.Wrap.ToString());
				}
			}
			catch (Exception exception90)
			{
				// ProjectData.SetProjectError(exception90);
				Exception exception42 = exception90;
				this.GHTSubTestAddResult("Unxpected " + exception42.GetType().Name + " exception was caught-" + exception42.Message);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			this.GHTSubTestBegin("RadioButton.GroupName");
			try
			{
				RadioButton button13 = new RadioButton();
				button13.ID = "objRadioButtonGroupName";
				base.GHTActiveForm.Controls.Add(button13);
				if (!this.Page.IsPostBack)
				{
					button13.GroupName = "test";
				}
				else
				{
					this.GHTSubTestAddResult(button13.GroupName);
				}
			}
			catch (Exception exception91)
			{
				// ProjectData.SetProjectError(exception91);
				Exception exception43 = exception91;
				this.GHTSubTestAddResult("Unxpected " + exception43.GetType().Name + " exception was caught-" + exception43.Message);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			this.GHTSubTestBegin("RadioButtonList.RepeatColumns");
			try
			{
				RadioButtonList list5 = new RadioButtonList();
				list5.ID = "objRadioButtonListRepeatColumns";
				base.GHTActiveForm.Controls.Add(list5);
				if (!this.Page.IsPostBack)
				{
					list5.RepeatColumns = 2;
				}
				else
				{
					this.GHTSubTestAddResult(list5.RepeatColumns.ToString());
				}
			}
			catch (Exception exception92)
			{
				// ProjectData.SetProjectError(exception92);
				Exception exception44 = exception92;
				this.GHTSubTestAddResult("Unxpected " + exception44.GetType().Name + " exception was caught-" + exception44.Message);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			this.GHTSubTestBegin("RadioButtonList.RepeatDirection");
			try
			{
				RadioButtonList list6 = new RadioButtonList();
				list6.ID = "objRadioButtonListRepeatDirection";
				base.GHTActiveForm.Controls.Add(list6);
				if (!this.Page.IsPostBack)
				{
					list6.RepeatDirection = RepeatDirection.Horizontal;
				}
				else
				{
					this.GHTSubTestAddResult(((int) list6.RepeatDirection).ToString());
				}
			}
			catch (Exception exception93)
			{
				// ProjectData.SetProjectError(exception93);
				Exception exception45 = exception93;
				this.GHTSubTestAddResult("Unxpected " + exception45.GetType().Name + " exception was caught-" + exception45.Message);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			this.GHTSubTestBegin("RadioButtonList.TextAlign");
			try
			{
				RadioButtonList list7 = new RadioButtonList();
				list7.ID = "objRadioButtonListTextAlign";
				base.GHTActiveForm.Controls.Add(list7);
				if (!this.Page.IsPostBack)
				{
					list7.TextAlign = TextAlign.Right;
				}
				else
				{
					this.GHTSubTestAddResult(((int) list7.TextAlign).ToString());
				}
			}
			catch (Exception exception94)
			{
				// ProjectData.SetProjectError(exception94);
				Exception exception46 = exception94;
				this.GHTSubTestAddResult("Unxpected " + exception46.GetType().Name + " exception was caught-" + exception46.Message);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			this.GHTSubTestBegin("RadioButtonList.RepeatLayout");
			try
			{
				RadioButtonList list8 = new RadioButtonList();
				list8.ID = "objRadioButtonListRepeatLayout";
				base.GHTActiveForm.Controls.Add(list8);
				if (!this.Page.IsPostBack)
				{
					list8.RepeatLayout = RepeatLayout.Flow;
				}
				else
				{
					this.GHTSubTestAddResult(((int) list8.RepeatLayout).ToString());
				}
			}
			catch (Exception exception95)
			{
				// ProjectData.SetProjectError(exception95);
				Exception exception47 = exception95;
				this.GHTSubTestAddResult("Unxpected " + exception47.GetType().Name + " exception was caught-" + exception47.Message);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			this.GHTSubTestBegin("TextBox.ReadOnly,AutoPostBack,Columns,Wrap,Text,Rows,MaxLength,TextMode");
			try
			{
				TextBox box5 = new TextBox();
				box5.ID = "objTextBoxAll";
				base.GHTActiveForm.Controls.Add(box5);
				if (!this.Page.IsPostBack)
				{
					box5.ReadOnly = true;
					box5.AutoPostBack = true;
					box5.Columns = 2;
					box5.Wrap = true;
					box5.Text = "test";
					box5.Rows = 2;
					box5.MaxLength = 10;
					box5.TextMode = TextBoxMode.MultiLine;
				}
				else
				{
					this.GHTSubTestAddResult(box5.ReadOnly.ToString());
					this.GHTSubTestAddResult(box5.ReadOnly.ToString());
					this.GHTSubTestAddResult(box5.AutoPostBack.ToString());
					this.GHTSubTestAddResult(box5.Columns.ToString());
					this.GHTSubTestAddResult(box5.Wrap.ToString());
					this.GHTSubTestAddResult(box5.Text);
					this.GHTSubTestAddResult(box5.Rows.ToString());
					this.GHTSubTestAddResult(box5.MaxLength.ToString());
					this.GHTSubTestAddResult(((int) box5.TextMode).ToString());
				}
			}
			catch (Exception exception96)
			{
				// ProjectData.SetProjectError(exception96);
				Exception exception48 = exception96;
				this.GHTSubTestAddResult("Unxpected " + exception48.GetType().Name + " exception was caught-" + exception48.Message);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			this.GHTTestEnd();
		}
 
		private ICollection CreateDataSource()
		{
			DataTable table1 = new DataTable();
			table1.TableName = "test";
			table1.Columns.Add(new DataColumn("IntegerValue", typeof(int)));
			table1.Columns.Add(new DataColumn("StringValue", typeof(string)));
			table1.Columns.Add(new DataColumn("CurrencyValue", typeof(double)));
			int num1 = 0;
			do
			{
				DataRow row1 = table1.NewRow();
				row1[0] = num1;
				row1[1] = "Item " + num1.ToString();
				row1[2] = 1.23 * (num1 + 1);
				table1.Rows.Add(row1);
				num1++;
			}
			while (num1 <= 8);
			return new DataView(table1);
		}
 

	}
}
