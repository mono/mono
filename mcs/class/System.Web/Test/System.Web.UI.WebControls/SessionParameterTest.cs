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
using MonoTests.SystemWeb.Framework;
using MonoTests.stand_alone.WebHarness;

namespace MonoTests.System.Web.UI.WebControls
{
	public class SessionParameterPoker : SessionParameter
	{
		public SessionParameterPoker()
		{
			TrackViewState();
		}

		public SessionParameterPoker(SessionParameter param)
			: base(param)
		{
		}

		public SessionParameterPoker(string name, string sessionField)
			: base(name, sessionField)
		{
		}
		public SessionParameterPoker(string name, TypeCode type, string sessionField)
			: base(name, type, sessionField)
		{
		}

		public object DoEvaluate(HttpContext context, Control control)
		{
			return base.Evaluate(context, control);
		}

		public Parameter DoClone()
		{
			return base.Clone();
		}

		public object SaveState()
		{
			return SaveViewState();
		}
		public void LoadState(object o)
		{
			LoadViewState(o);
		}

		public StateBag StateBag
		{
			get { return base.ViewState; }
		}

	}

	[TestFixture]
	public class SessionParameterTest
	{
		[Test]
		public void SessionParameter_DefaultProperties()
		{
			SessionParameterPoker sessionParam1 = new SessionParameterPoker();
			Assert.AreEqual("", sessionParam1.SessionField, "DefaultSessionField");
			SessionParameterPoker sessionParam2 = new SessionParameterPoker("Name", "id");
			Assert.AreEqual("Name", sessionParam2.Name, "OverloadConstructorName1");
			Assert.AreEqual("id", sessionParam2.SessionField, "OverloadConstructorSessionField1");
			SessionParameterPoker sessionParam3 = new SessionParameterPoker("Name", TypeCode.Int64, "id");
			Assert.AreEqual("Name", sessionParam3.Name, "OverloadConstructorName2");
			Assert.AreEqual("id", sessionParam3.SessionField, "OverloadConstructorsessionField2");
			Assert.AreEqual(TypeCode.Int64, sessionParam3.Type, "OverloadConstructorType2");
			SessionParameterPoker sessionParam4 = new SessionParameterPoker(sessionParam3);
			Assert.AreEqual("Name", sessionParam4.Name, "OverloadConstructorName3");
			Assert.AreEqual("id", sessionParam4.SessionField, "OverloadConstructorSessionField3");
			Assert.AreEqual(TypeCode.Int64, sessionParam4.Type, "OverloadConstructorType3");


		}

		[Test]
		public void SessionParameter_AssignToDefaultProperties()
		{
			SessionParameterPoker sessionParam = new SessionParameterPoker();
			sessionParam.SessionField = "Test";
			Assert.AreEqual("Test", sessionParam.SessionField, "AssignToSessionField");

		}

		//Protected Methods

		[Test]
		public void SessionParameter_Clone()
		{
	                 SessionParameterPoker sessionParam = new SessionParameterPoker("EmployeeName", TypeCode.String, "Name");
			 SessionParameter clonedParam = (SessionParameter)sessionParam.DoClone();
			 Assert.AreEqual("EmployeeName", clonedParam.Name, "SessionParameterCloneName");
			 Assert.AreEqual(TypeCode.String, clonedParam.Type, "SessionParameterCloneType");
			
		}

		[Test]
		[Category("NunitWeb")]
#if !TARGET_JVM
		[Category("NotWorking")]
#endif
		public void SessionParameter_Evaluate()
		{
			SessionParameterPoker sessionParam = new SessionParameterPoker("employee",TypeCode.String ,"id") ;
			Button b = new Button();
			string value = (string)sessionParam.DoEvaluate(null, b);
			Assert.AreEqual(null, value, "EvaluateSessionWhenNullContext");
			WebTest t = new WebTest();
			PageDelegates pd = new PageDelegates();
			pd.Init = InitSesssion;
			pd.Load = EvaluateSession;
			t.Invoker = new PageInvoker(pd);
			string html = t.Run();
			WebTest.Unload(); 
			

		}

		public static void InitSesssion(Page p)
		{
			p.Session["key"] = "Key1"; 
		}

		public static void EvaluateSession(Page p)
		{
			SessionParameterPoker sessionParam = new SessionParameterPoker();
			sessionParam.SessionField = "key";
			sessionParam.Type = TypeCode.String;
			TextBox tb = new TextBox();
			p.Controls.Add(tb); 
			string value = (string)sessionParam.DoEvaluate(HttpContext.Current, tb);
			Assert.AreEqual("Key1", value, "EvaluateSessionParameter");
		}

	}
}
#endif
