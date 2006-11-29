//
// Tests for System.Web.UI.WebControls.FormView.cs 
//
// Author:
//	Merav Sudri (meravs@mainsoft.com)
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
//

#if NET_2_0

using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using MonoTests.SystemWeb.Framework;
using MonoTests.stand_alone.WebHarness;

namespace MonoTests.System.Web.UI.WebControls
{
	public class FormParameterPoker : FormParameter
	{
		public FormParameterPoker (FormParameter param)
			: base (param)
		{
			
		}

		public FormParameterPoker (string name, TypeCode type, string FormField)
			: base (name, type, FormField)
		{
		}

		public FormParameterPoker (string name, string FormField)
			: base (name, FormField)
		{
		}

		public FormParameterPoker () // constructor       
		{
			TrackViewState ();
		}

		public object DoEvaluate (HttpContext context, Control control)
		{
			return base.Evaluate (context, control);
		}

		public Parameter DoClone ()
		{
			return base.Clone ();
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

	}

	[TestFixture]
	public class FormParameterTest
	{
		[TestFixtureTearDown]
		public void Unload ()
		{
			WebTest.Unload ();
		}

		[Test]
		public void FormParameter_DefaultProperties ()
		{
			FormParameterPoker FormParam1 = new FormParameterPoker ();
			Assert.AreEqual ("", FormParam1.FormField , "DefaultFormField");
			FormParameterPoker FormParam2 = new FormParameterPoker ("FirstName", "TextBox1");
			Assert.AreEqual ("FirstName", FormParam2.Name, "OverloadContructorName1");
			Assert.AreEqual ("TextBox1", FormParam2.FormField, "OverloadContructorFormField1");
			FormParameterPoker FormParam3 = new FormParameterPoker ("Salary", TypeCode.UInt64, "SalaryTextBox");
			Assert.AreEqual ("Salary", FormParam3.Name, "OverloadContructorName2");
			Assert.AreEqual ("SalaryTextBox", FormParam3.FormField, "OverloadContructorFormField2");
			Assert.AreEqual (TypeCode.UInt64, FormParam3.Type, "OverloadContructorType");
			FormParameterPoker FormParam4 = new FormParameterPoker (FormParam3);
			Assert.AreEqual ("Salary", FormParam4.Name, "OverloadContructorName2");
			Assert.AreEqual ("SalaryTextBox", FormParam4.FormField, "OverloadContructorFormName2");
			Assert.AreEqual (TypeCode.UInt64, FormParam4.Type, "OverloadContructorType");


		}

		[Test]
		public void FormParameter_AssignToDefaultProperties ()
		{
			FormParameterPoker FormParam = new FormParameterPoker ();
			FormParam.FormField = "FormFieldTest";
			Assert.AreEqual ("FormFieldTest", FormParam.FormField, "AssignToFormName");
		}

		//Protected Methods

		[Test]
		public void FormParameter_Clone ()
		{
			
			FormParameterPoker FormParam = new FormParameterPoker ("EmployeeName", TypeCode.String, "EmployeeLabel");
			FormParameter clonedParam = (FormParameter) FormParam.DoClone ();
			Assert.AreEqual ("EmployeeName", clonedParam.Name, "FormParameterCloneName");
			Assert.AreEqual (TypeCode.String, clonedParam.Type, "FormParameterCloneType");
			Assert.AreEqual ("EmployeeLabel", clonedParam.FormField, "FormParameterCloneFormField");
		}		

		[Test]
		[Category("NunitWeb")]
		public void FormParameter_Evaluate()
		{
			WebTest t = new WebTest(PageInvoker.CreateOnInit(InitForm));
			t.Run();
			FormRequest fr = new FormRequest(t.Response, "form1");
			fr.Controls.Add("key");
			fr.Controls["key"].Value = "Key1";
			t.Request = fr;
			PageDelegates pd = new PageDelegates();
			pd.Load = EvaluateForm;
			t.Invoker = new PageInvoker(pd);
			t.Run();
		 }

		public static void InitForm(Page p)
		{
			LiteralControl ltl = new LiteralControl(@"<input id=""key"" type=""text"" value=""Key1""/>");
			p.Form.Controls.Add(ltl);
		}

		public static void EvaluateForm(Page p)
		{
			FormParameterPoker formParam = new FormParameterPoker();
			formParam.FormField = "key";
			formParam.Type = TypeCode.String;
			formParam.DefaultValue = "Default"; 
			TextBox tb = new TextBox();
			p.Form.Controls.Add(tb);			
			string value = (string)formParam.DoEvaluate(HttpContext.Current, tb);
			Assert.AreEqual("Key1", value, "EvaluateSessionParameter");
		}


	}
}
#endif
