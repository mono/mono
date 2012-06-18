//
// DynamicValidatorTest.cs
//
// Authors:
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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Permissions;
using System.Security.Principal;
using System.Web;
using System.Web.UI;
using System.Web.DynamicData;
using System.Web.DynamicData.ModelProviders;
using System.Web.Routing;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.IO;

using NUnit.Framework;
using NUnit.Mocks;
using MonoTests.stand_alone.WebHarness;
using MonoTests.SystemWeb.Framework;
using MonoTests.Common;
using MonoTests.DataSource;
using MonoTests.DataObjects;
using MonoTests.ModelProviders;

using MetaModel = System.Web.DynamicData.MetaModel;
using MetaTable = System.Web.DynamicData.MetaTable;

namespace MonoTests.System.Web.DynamicData
{
	[TestFixture]
	public class DynamicValidatorTest
	{
		DynamicDataContainerModelProvider<TestDataContext> dynamicModelProvider;

		[TestFixtureSetUp]
		public void SetUp ()
		{
			Type type = GetType ();
			WebTest.CopyResource (type, "MonoTests.WebPages.ListView_DynamicControl_04.aspx", "ListView_DynamicControl_04.aspx");
			WebTest.CopyResource (type, "MonoTests.WebPages.ListView_DynamicControl_04.aspx.cs", "ListView_DynamicControl_04.aspx.cs");
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicValidator_01.aspx", "DynamicValidator_01.aspx");
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicValidator_01.aspx.cs", "DynamicValidator_01.aspx.cs");
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicValidator_02.aspx", "DynamicValidator_02.aspx");
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicValidator_02.aspx.cs", "DynamicValidator_02.aspx.cs");

			dynamicModelProvider = new DynamicDataContainerModelProvider<TestDataContext> ();
			Utils.RegisterContext (dynamicModelProvider, new ContextConfiguration () { ScaffoldAllTables = true });
		}

		[Test]
		public void Defaults ()
		{
			var dv = new PokerDynamicValidator ();

			Assert.IsNull (dv.Column, "#A1");
			Assert.IsNotNull (dv.ColumnName, "#A2");
			Assert.AreEqual (String.Empty, dv.ColumnName, "#A2-1");
			Assert.IsNull (dv.GetValidationException (), "#A3");
		}

		[Test]
		public void Column ()
		{
			MetaModel m = Utils.CommonInitialize ();
			MetaTable t = m.Tables[TestDataContext.TableBazColumnAttributes];
			MetaColumn mc = t.GetColumn ("ColumnNoAttributes");

			var dv = new PokerDynamicValidator ();
			Assert.IsNull (dv.Column, "#A1");

			dv.Column = mc;
			Assert.IsNotNull (dv.Column, "#B1");
			Assert.AreEqual (mc, dv.Column, "#B1-1");

			dv.Column = null;
			Assert.IsNull (dv.Column, "#C1");
		}

		[Test]
		[Category ("NotWorking")]
		public void Column_1 ()
		{
			var test = new WebTest ("ListView_DynamicControl_04.aspx");
			var p = test.Run ();

			// Fake post-back
			var delegates = new PageDelegates ();
			delegates.PreRenderComplete = Column_1_OnPreRenderComplete;
			test.Invoker = new PageInvoker (delegates);
			var fr = new FormRequest (test.Response, "form1");
#if TARGET_DOTNET
			fr.Controls.Add ("ListView4$ctrl0$editMe");
			fr.Controls["ListView4$ctrl0$editMe"].Value = "Edit";
#else
			fr.Controls.Add ("ListView4$ctl01$editMe");
			fr.Controls["ListView4$ctl01$editMe"].Value = "Edit";
#endif
			test.Request = fr;
			p = test.Run ();

			Assert.IsNotNull (test.Response, "#X1");
			Assert.AreNotEqual (HttpStatusCode.NotFound, test.Response.StatusCode, "#X1-1{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.AreNotEqual (HttpStatusCode.InternalServerError, test.Response.StatusCode, "#X1-2{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.IsFalse (String.IsNullOrEmpty (p), "#X1-3");
		}

		static void Column_1_OnPreRenderComplete (Page p)
		{
			var lc = p.FindControl ("ListView4") as ListView;
			Assert.IsNotNull (lc, "#A1");

			var dc = lc.FindChild<DynamicControl> ("String_Column");
			Assert.IsNotNull (dc, "#B1");

			var field = dc.FieldTemplate as FieldTemplateUserControl;
			Assert.IsNotNull (field, "#C1");
			Assert.AreEqual ("~/DynamicData/FieldTemplates/Text_Edit.ascx", field.AppRelativeVirtualPath, "#C1-1");

			var dv = field.FindChild<DynamicValidator> ("DynamicValidator1");
			Assert.IsNotNull (dv, "#D1");
			Assert.AreEqual (dc.Column, dv.Column, "#D1-1");
		}

		[Test]
		public void ColumnName ()
		{
			MetaModel m = Utils.CommonInitialize ();
			MetaTable t = m.Tables[TestDataContext.TableBazColumnAttributes];
			MetaColumn mc = t.GetColumn ("ColumnNoAttributes");

			var dv = new PokerDynamicValidator ();
			Assert.AreEqual (String.Empty, dv.ColumnName, "#A1");

			dv.Column = mc;
			Assert.AreEqual (mc.Name, dv.ColumnName, "#B1");
		}

		[Test]
		[Category ("NotWorking")]
		public void ColumnName_1 ()
		{
			var test = new WebTest ("ListView_DynamicControl_04.aspx");
			var p = test.Run ();

			// Fake post-back
			var delegates = new PageDelegates ();
			delegates.PreRenderComplete = ColumnName_1_OnPreRenderComplete;
			test.Invoker = new PageInvoker (delegates);
			var fr = new FormRequest (test.Response, "form1");
#if TARGET_DOTNET
			fr.Controls.Add ("ListView4$ctrl0$editMe");
			fr.Controls["ListView4$ctrl0$editMe"].Value = "Edit";
#else
			fr.Controls.Add ("ListView4$ctl01$editMe");
			fr.Controls["ListView4$ctl01$editMe"].Value = "Edit";
#endif
			test.Request = fr;
			p = test.Run ();

			Assert.IsNotNull (test.Response, "#X1");
			Assert.AreNotEqual (HttpStatusCode.NotFound, test.Response.StatusCode, "#X1-1{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.AreNotEqual (HttpStatusCode.InternalServerError, test.Response.StatusCode, "#X1-2{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.IsFalse (String.IsNullOrEmpty (p), "#X1-3");
		}

		static void ColumnName_1_OnPreRenderComplete (Page p)
		{
			var lc = p.FindControl ("ListView4") as ListView;
			Assert.IsNotNull (lc, "#A1");

			var dc = lc.FindChild<DynamicControl> ("String_Column");
			Assert.IsNotNull (dc, "#B1");

			var field = dc.FieldTemplate as FieldTemplateUserControl;
			Assert.IsNotNull (field, "#C1");
			Assert.AreEqual ("~/DynamicData/FieldTemplates/Text_Edit.ascx", field.AppRelativeVirtualPath, "#C1-1");

			var dv = field.FindChild<PokerDynamicValidator> ("DynamicValidator1");
			Assert.IsNotNull (dv, "#D1");
			Assert.AreEqual (dc.Column.Name, dv.ColumnName, "#D1-1");
		}

		[Test]
		public void ControlPropertiesValid ()
		{
			var dv = new PokerDynamicValidator ();

			// Nice...:
			//
			// MonoTests.System.Web.DynamicData.DynamicValidatorTest.ControlPropertiesValid:
			// System.NullReferenceException : Object reference not set to an instance of an object.
			// at System.Web.DynamicData.DynamicDataExtensions.FindDataSourceControl(Control current)
			// at System.Web.DynamicData.DynamicValidator.get_DynamicDataSource()
			// at System.Web.DynamicData.DynamicValidator.ControlPropertiesValid()
			// at MonoTests.Common.PokerDynamicValidator.CallControlPropertiesValid() in C:\Users\grendel\Documents\Visual Studio 2008\Projects\DynamicDataNunit\DynamicDataNunitTests\Common\PokerDynamicValidator.cs:line 20
			// at MonoTests.System.Web.DynamicData.DynamicValidatorTest.ControlPropertiesValid() in C:\Users\grendel\Documents\Visual Studio 2008\Projects\DynamicDataNunit\DynamicDataNunitTests\System.Web.DynamicData\DynamicValidatorTest.cs:line 220
			// Assert.IsFalse (dv.CallControlPropertiesValid (), "#A1");
		}

		[Test]
		public void ControlPropertiesValid_01 ()
		{
			var test = new WebTest ("DynamicValidator_01.aspx");
			var p = test.Run ();
			test.Invoker = PageInvoker.CreateOnLoad (ControlPropertiesValid_01_OnLoad);
			Assert.AreNotEqual (HttpStatusCode.NotFound, test.Response.StatusCode, "#X1-1{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.AreNotEqual (HttpStatusCode.InternalServerError, test.Response.StatusCode, "#X1-2{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.IsFalse (String.IsNullOrEmpty (p), "#X1-3");
		}

		static void ControlPropertiesValid_01_OnLoad (Page p)
		{
			var dv = p.FindChild<PokerDynamicValidator> ("dynamicValidator1");
			Assert.IsNotNull (dv, "#A1");

			Assert.IsFalse (dv.CallControlPropertiesValid (), "#A2");
			Assert.IsNull (dv.Column, "#A2-1");
			Assert.IsNull (dv.ColumnName, "#A2-2");

			MetaModel m = Utils.CommonInitialize ();
			MetaTable t = m.Tables[TestDataContext.TableBar];
			MetaColumn mc = t.GetColumn ("Column1");

			dv.Column = mc;
			Assert.IsFalse (dv.CallControlPropertiesValid (), "#A3");
			Assert.IsNotNull (dv.Column, "#A3-1");
			Assert.IsNull (dv.ColumnName, "#A3-2");
		}

		[Test]
		[Category ("NotWorking")]
		public void ControlPropertiesValid_2 ()
		{
			var test = new WebTest ("ListView_DynamicControl_04.aspx");
			var p = test.Run ();

			// Fake post-back
			var delegates = new PageDelegates ();
			delegates.PreRenderComplete = ControlPropertiesValid_2_OnPreRenderComplete;
			test.Invoker = new PageInvoker (delegates);
			var fr = new FormRequest (test.Response, "form1");
#if TARGET_DOTNET
			fr.Controls.Add ("ListView4$ctrl0$editMe");
			fr.Controls["ListView4$ctrl0$editMe"].Value = "Edit";
#else
			fr.Controls.Add ("ListView4$ctl01$editMe");
			fr.Controls["ListView4$ctl01$editMe"].Value = "Edit";
#endif
			test.Request = fr;
			p = test.Run ();

			Assert.IsNotNull (test.Response, "#X1");
			Assert.AreNotEqual (HttpStatusCode.NotFound, test.Response.StatusCode, "#X1-1{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.AreNotEqual (HttpStatusCode.InternalServerError, test.Response.StatusCode, "#X1-2{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.IsFalse (String.IsNullOrEmpty (p), "#X1-3");
		}

		static void ControlPropertiesValid_2_OnPreRenderComplete (Page p)
		{
			var lc = p.FindControl ("ListView4") as ListView;
			Assert.IsNotNull (lc, "#A1");

			var dc = lc.FindChild<DynamicControl> ("String_Column");
			Assert.IsNotNull (dc, "#B1");

			var field = dc.FieldTemplate as FieldTemplateUserControl;
			Assert.IsNotNull (field, "#C1");
			Assert.AreEqual ("~/DynamicData/FieldTemplates/Text_Edit.ascx", field.AppRelativeVirtualPath, "#C1-1");

			var dv = field.FindChild<PokerDynamicValidator> ("DynamicValidator1");
			Assert.IsNotNull (dv, "#D1");
			Assert.IsTrue (dv.CallControlPropertiesValid (), "#D1-2");
			Assert.IsNotNull (dv.Column, "#D1-3");
			Assert.IsNotNull (dv.ColumnName, "#D1-4");
		}

		[Test]
		public void EvaluateIsValid ()
		{
			var dv = new PokerDynamicValidator ();

			Assert.IsTrue (dv.CallEvaluateIsValid (), "#A1");
			Assert.IsNotNull (dv.ControlToValidate, "#A1-1");
			Assert.AreEqual (String.Empty, dv.ControlToValidate, "#A1-2");
			Assert.AreEqual (String.Empty, dv.ErrorMessage, "#A1-3");

			dv.SetValidationException (new Exception ("test message"));
			Assert.IsFalse (dv.CallEvaluateIsValid (), "#A2");
			Assert.AreEqual (dv.GetValidationException ().Message, dv.ErrorMessage, "#A2-1");

			dv.SetValidationException (new Exception ("<script>message</script>"));
			Assert.IsFalse (dv.CallEvaluateIsValid (), "#A3");
			Assert.AreEqual (HttpUtility.HtmlEncode (dv.GetValidationException ().Message), dv.ErrorMessage, "#A3-1");
		}

		[Test]
		[Category ("NotWorking")]
		public void EvaluateIsValid_1 ()
		{
			var test = new WebTest ("DynamicValidator_02.aspx");
			var p = test.Run ();

			// Fake post-back
			var delegates = new PageDelegates ();
			delegates.PreRenderComplete = EvaluateIsValid_1_OnPreRenderComplete;
			test.Invoker = new PageInvoker (delegates);
			var fr = new FormRequest (test.Response, "form1");
#if TARGET_DOTNET
			fr.Controls.Add ("ListView1$ctrl0$editMe");
			fr.Controls["ListView1$ctrl0$editMe"].Value = "Edit";
#else
			fr.Controls.Add ("ListView1$ctl01$editMe");
			fr.Controls["ListView1$ctl01$editMe"].Value = "Edit";
#endif
			test.Request = fr;
			p = test.Run ();

			Assert.IsNotNull (test.Response, "#X1");
			Assert.AreNotEqual (HttpStatusCode.NotFound, test.Response.StatusCode, "#X1-1{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.AreNotEqual (HttpStatusCode.InternalServerError, test.Response.StatusCode, "#X1-2{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.IsFalse (String.IsNullOrEmpty (p), "#X1-3");
		}

		static void EvaluateIsValid_1_OnPreRenderComplete (Page p)
		{
			var lc = p.FindControl ("ListView1") as ListView;
			Assert.IsNotNull (lc, "#A1");

			var dc = lc.FindChild<PokerDynamicControl> ("Column1");
			Assert.IsNotNull (dc, "#B1");

			var field = dc.FieldTemplate as FieldTemplateUserControl;
			Assert.IsNotNull (field, "#C1");
			Assert.AreEqual ("~/DynamicData/FieldTemplates/Integer_Edit.ascx", field.AppRelativeVirtualPath, "#C1-1");

			var dv = field.FindChild<PokerDynamicValidator> ("DynamicValidator1");
			Assert.IsNotNull (dv, "#D1");

			var tb = field.FindChild<TextBox> ("TextBox1");
			Assert.IsNotNull (tb, "#D1-2");
			Assert.AreEqual ("0", tb.Text, "#D1-3");

			Assert.IsTrue (dv.CallEvaluateIsValid (), "#E1");
		}

		[Test]
		public void ValidationException ()
		{
			var dv = new PokerDynamicValidator ();

			Assert.IsNull (dv.GetValidationException (), "#A1");

			Exception ex = new Exception ();
			dv.SetValidationException (ex);
			Assert.AreEqual (ex, dv.GetValidationException (), "#B1");

			ex = new ArgumentNullException ();
			dv.SetValidationException (ex);
			Assert.AreEqual (ex, dv.GetValidationException (), "#C1");

			dv.SetValidationException (null);
			Assert.IsNull (dv.GetValidationException (), "#D1");
		}

		[Test]
		public void ValidationException_1 ()
		{
			var test = new WebTest ("ListView_DynamicControl_04.aspx");
			var p = test.Run ();

			// Fake post-back
			var delegates = new PageDelegates ();
			delegates.PreRenderComplete = ValidationException_1_OnPreRenderComplete;
			test.Invoker = new PageInvoker (delegates);
			var fr = new FormRequest (test.Response, "form1");
#if TARGET_DOTNET
			fr.Controls.Add ("ListView4$ctrl0$editMe");
			fr.Controls["ListView4$ctrl0$editMe"].Value = "Edit";
#else
			fr.Controls.Add ("ListView4$ctl01$editMe");
			fr.Controls["ListView4$ctl01$editMe"].Value = "Edit";
#endif
			test.Request = fr;
			p = test.Run ();

			Assert.IsNotNull (test.Response, "#X1");
			Assert.AreNotEqual (HttpStatusCode.NotFound, test.Response.StatusCode, "#X1-1{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.AreNotEqual (HttpStatusCode.InternalServerError, test.Response.StatusCode, "#X1-2{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.IsFalse (String.IsNullOrEmpty (p), "#X1-3");
		}

		static void ValidationException_1_OnPreRenderComplete (Page p)
		{
			var lc = p.FindControl ("ListView4") as ListView;
			Assert.IsNotNull (lc, "#A1");

			var dc = lc.FindChild<DynamicControl> ("String_Column");
			Assert.IsNotNull (dc, "#B1");

			var field = dc.FieldTemplate as FieldTemplateUserControl;
			Assert.IsNotNull (field, "#C1");
			Assert.AreEqual ("~/DynamicData/FieldTemplates/Text_Edit.ascx", field.AppRelativeVirtualPath, "#C1-1");

			var dv = field.FindChild<PokerDynamicValidator> ("DynamicValidator1");
			Assert.IsNotNull (dv, "#D1");
			Assert.IsNull (dv.GetValidationException (), "#D1-1");

			Exception ex = new Exception ();
			dv.SetValidationException (ex);
			Assert.AreEqual (ex, dv.GetValidationException (), "#E1");

			ex = new ArgumentNullException ();
			dv.SetValidationException (ex);
			Assert.AreEqual (ex, dv.GetValidationException (), "#F1");

			dv.SetValidationException (null);
			Assert.IsNull (dv.GetValidationException (), "#G1");
		}

		[Test]
		[Category ("NotWorking")]
		public void ValidateException ()
		{
			var dv = new PokerDynamicValidator ();

			dv.CallValidateException (null);

			Assert.IsNull (dv.GetValidationException (), "#A1");
			Exception ex = new Exception ();
			dv.CallValidateException (ex);
			Assert.IsNull (dv.GetValidationException (), "#B1");

			ex = new ValidationException ("test message");
			dv.CallValidateException (ex);
			Assert.IsNotNull (dv.GetValidationException (), "#C1");
			Assert.AreEqual (ex, dv.GetValidationException (), "#C1-1");
			Assert.AreEqual ("test message", dv.GetValidationException ().Message, "#C1-2");
		}
	}
}
