//
// DynamicDataRouteTest.cs
//
// Author:
//      Marek Habersack <mhabersack@novell.com>
//
// Copyright (C) 2009 Novell Inc. http://novell.com
//

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
using System.Collections.Generic;
using System.Web.DynamicData;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using NUnit.Framework;

namespace MonoTests.System.Web.DynamicData
{
	class MyTemplate : WebControl, ITemplate
	{
		public MyTemplate()
		{
			ID = "MyTemplate";
		}

#region ITemplate Members
		void ITemplate.InstantiateIn(Control container)
		{
			foreach (Control c in Controls)
				container.Controls.Add(c);
		}
#endregion
	}

	class MyBaseDataBoundControl : BaseDataBoundControl
	{
		protected override void PerformSelect()
		{
			// no-op
		}

		protected override void ValidateDataSource(object dataSource)
		{
			// no-op
		}
	}

	class MyDataBoundControl : DataBoundControl
	{ }

	class MyDynamicDataManager : DynamicDataManager
	{
		public void DoLoad()
		{
			OnLoad(EventArgs.Empty);
		}
	}

	[TestFixture]
	public class DynamicDataManagerTests
	{
		[Test]
		public void DefaultValues()
		{
			var ddm = new DynamicDataManager();

			Assert.AreEqual(true, ddm.Visible, "#A1");
			Assert.AreEqual(false, ddm.AutoLoadForeignKeys, "#A2");
		}

		[Test]
		[ExpectedException (typeof (NotImplementedException))]
		public void VisibleSet()
		{
			var ddm = new DynamicDataManager();

			ddm.Visible = false;
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void RegisterControl_NullControl()
		{
			var ddm = new DynamicDataManager();
			ddm.RegisterControl(null);
		}

		[Test]
		[ExpectedException(typeof(NullReferenceException))]
		public void RegisterControl_NullControl2()
		{
			var ddm = new DynamicDataManager();
			ddm.RegisterControl(null, false);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void RegisterControl_ControlIsNotDataBoundControl()
		{
			var ddm = new DynamicDataManager();
			var control = new MyBaseDataBoundControl ();
			ddm.RegisterControl(control);
		}

		[Test]
		public void RegisterControl_ControlIsDataBoundControl()
		{
			var ddm = new DynamicDataManager();
			var control = new MyDataBoundControl();
			ddm.RegisterControl(control);
		}

		[Test]
		public void RegisterControl_ControlIsDataBoundControl2()
		{
			var ddm = new DynamicDataManager();
			var control = new MyDataBoundControl();
			ddm.RegisterControl(control, false);
			ddm.RegisterControl(control, true);
		}

#region Supported controls
		// Checks for which controls are supported
		[Test]
		public void DynamicManagerRegisterControl_System_Web_UI_WebControls_AdRotator_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (AdRotator)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		public void DynamicManagerRegisterControl_System_Web_UI_WebControls_BulletedList_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (BulletedList)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		public void DynamicManagerRegisterControl_System_Web_UI_WebControls_CheckBoxList_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (CheckBoxList)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		public void DynamicManagerRegisterControl_System_Web_UI_WebControls_DetailsView_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (DetailsView)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		public void DynamicManagerRegisterControl_System_Web_UI_WebControls_DropDownList_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (DropDownList)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		public void DynamicManagerRegisterControl_System_Web_UI_WebControls_FormView_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (FormView)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		public void DynamicManagerRegisterControl_System_Web_UI_WebControls_GridView_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (GridView)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		public void DynamicManagerRegisterControl_System_Web_UI_WebControls_ListBox_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (ListBox)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		public void DynamicManagerRegisterControl_System_Web_UI_WebControls_RadioButtonList_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (RadioButtonList)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[Category ("NotWorking")]
		public void DynamicManagerRegisterControl_System_Web_UI_WebControls_Repeater_Test2()
		{
			var ddm = new DynamicDataManager();
			var control = Activator.CreateInstance(typeof(Repeater)) as Control;
			var page = new Page();
			page.Controls.Add(control);
			ddm.RegisterControl(control);
		}
		
		[Test]
		[ExpectedException (typeof (HttpException))]
		public void DynamicManagerRegisterControl_System_Web_UI_WebControls_Repeater_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (Repeater)) as Control;
			ddm.RegisterControl (control);
		}
#endregion
		
#region Unsupported controls
		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_Control_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (Control)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_DesignerDataBoundLiteralControl_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (DesignerDataBoundLiteralControl)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		[Category ("NotWorking")]
		public void DynamicManagerRegisterControl_System_Web_UI_Page_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (Page)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_HtmlControls_HtmlForm_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (HtmlForm)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_LiteralControl_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (LiteralControl)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_UserControl_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (UserControl)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_MasterPage_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (MasterPage)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_HtmlControls_HtmlAnchor_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (HtmlAnchor)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_HtmlControls_HtmlButton_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (HtmlButton)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_HtmlControls_HtmlGenericControl_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (HtmlGenericControl)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_HtmlControls_HtmlHead_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (HtmlHead)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_HtmlControls_HtmlImage_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (HtmlImage)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_HtmlControls_HtmlInputButton_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (HtmlInputButton)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_HtmlControls_HtmlInputCheckBox_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (HtmlInputCheckBox)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_HtmlControls_HtmlInputFile_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (HtmlInputFile)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_HtmlControls_HtmlInputHidden_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (HtmlInputHidden)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_HtmlControls_HtmlInputImage_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (HtmlInputImage)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_HtmlControls_HtmlInputText_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (HtmlInputText)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_HtmlControls_HtmlInputPassword_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (HtmlInputPassword)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_HtmlControls_HtmlInputRadioButton_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (HtmlInputRadioButton)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_HtmlControls_HtmlInputReset_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (HtmlInputReset)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_HtmlControls_HtmlInputSubmit_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (HtmlInputSubmit)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_HtmlControls_HtmlLink_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (HtmlLink)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_HtmlControls_HtmlMeta_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (HtmlMeta)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_HtmlControls_HtmlSelect_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (HtmlSelect)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_HtmlControls_HtmlTable_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (HtmlTable)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_HtmlControls_HtmlTableCell_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (HtmlTableCell)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_HtmlControls_HtmlTableRow_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (HtmlTableRow)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_HtmlControls_HtmlTextArea_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (HtmlTextArea)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_HtmlControls_HtmlTitle_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (HtmlTitle)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_WebControls_SqlDataSource_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (SqlDataSource)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_WebControls_AccessDataSource_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (AccessDataSource)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_WebControls_Label_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (Label)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_WebControls_Button_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (Button)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_WebControls_Calendar_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (Calendar)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_WebControls_ChangePassword_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (ChangePassword)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_WebControls_TableRow_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (TableRow)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_WebControls_CheckBox_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (CheckBox)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_WebControls_Table_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (Table)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_WebControls_CompareValidator_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (CompareValidator)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_WebControls_View_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (View)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_WebControls_TemplatedWizardStep_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (TemplatedWizardStep)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_WebControls_CompleteWizardStep_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (CompleteWizardStep)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_WebControls_Content_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (Content)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_WebControls_ContentPlaceHolder_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (ContentPlaceHolder)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_WebControls_Wizard_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (Wizard)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_WebControls_TableCell_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (TableCell)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_WebControls_CreateUserWizard_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (CreateUserWizard)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_WebControls_CreateUserWizardStep_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (CreateUserWizardStep)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_WebControls_CustomValidator_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (CustomValidator)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_WebControls_Image_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (Image)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_WebControls_ImageButton_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (ImageButton)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_WebControls_LinkButton_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (LinkButton)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_WebControls_DataGrid_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (DataGrid)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_WebControls_DataList_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (DataList)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_WebControls_FileUpload_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (FileUpload)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_WebControls_HiddenField_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (HiddenField)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_WebControls_HyperLink_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (HyperLink)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_WebControls_ImageMap_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (ImageMap)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_WebControls_Literal_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (Literal)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_WebControls_Localize_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (Localize)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_WebControls_Login_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (Login)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_WebControls_LoginName_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (LoginName)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_WebControls_LoginStatus_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (LoginStatus)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_WebControls_LoginView_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (LoginView)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_WebControls_Menu_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (Menu)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_WebControls_MultiView_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (MultiView)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_WebControls_ObjectDataSource_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (ObjectDataSource)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_WebControls_Panel_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (Panel)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_WebControls_PasswordRecovery_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (PasswordRecovery)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_WebControls_PlaceHolder_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (PlaceHolder)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_WebControls_RadioButton_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (RadioButton)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_WebControls_RangeValidator_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (RangeValidator)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_WebControls_RegularExpressionValidator_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (RegularExpressionValidator)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_WebControls_RequiredFieldValidator_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (RequiredFieldValidator)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_WebControls_SiteMapDataSource_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (SiteMapDataSource)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_WebControls_SiteMapPath_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (SiteMapPath)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_WebControls_Substitution_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (Substitution)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_WebControls_TableFooterRow_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (TableFooterRow)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_WebControls_TableHeaderCell_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (TableHeaderCell)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_WebControls_TableHeaderRow_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (TableHeaderRow)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_WebControls_TextBox_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (TextBox)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_WebControls_TreeView_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (TreeView)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_WebControls_ValidationSummary_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (ValidationSummary)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_WebControls_WizardStep_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (WizardStep)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_WebControls_Xml_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (Xml)) as Control;
			ddm.RegisterControl (control);
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DynamicManagerRegisterControl_System_Web_UI_WebControls_XmlDataSource_Test ()
		{
			var ddm = new DynamicDataManager ();
			var control = Activator.CreateInstance (typeof (XmlDataSource)) as Control;
			ddm.RegisterControl (control);
		}
#endregion
	}	
}
