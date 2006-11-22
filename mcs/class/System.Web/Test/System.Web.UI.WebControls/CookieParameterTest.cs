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

namespace MonoTests.System.Web.UI.WebControls
{
	public class CookieParameterPoker : CookieParameter
	{
		public CookieParameterPoker (CookieParameter param)
			: base (param)
		{
		}

		public CookieParameterPoker (string name, TypeCode type, string cookieName)
			: base (name, type, cookieName)
		{
		}

		public CookieParameterPoker (string name, string cookieName)
			: base (name, cookieName)
		{
		}		

		public CookieParameterPoker () // constructor       
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
	public class CookieParameterTest
	{
		[Test]
		public void CookieParameter_DefaultProperties ()
		{
			CookieParameterPoker cookieParam1 = new CookieParameterPoker ();
			Assert.AreEqual ("", cookieParam1.CookieName, "DefaultCookieName"); 
			CookieParameterPoker cookieParam2 = new CookieParameterPoker ("CookieTest", "CookieName");
			Assert.AreEqual ("CookieTest", cookieParam2.Name, "OverloadContructorName1");
			Assert.AreEqual ("CookieName", cookieParam2.CookieName, "OverloadContructorCookieName1"); 
			CookieParameterPoker cookieParam3 = new CookieParameterPoker ("Salary", TypeCode.UInt64, "SalaryCookie");
			Assert.AreEqual ("Salary", cookieParam3.Name, "OverloadContructorName2");
			Assert.AreEqual ("SalaryCookie",cookieParam3.CookieName ,"OverloadContructorCookieName2");
			Assert.AreEqual (TypeCode.UInt64, cookieParam3.Type, "OverloadContructorType");  
			CookieParameterPoker cookieParam4 = new CookieParameterPoker (cookieParam3);
			Assert.AreEqual ("Salary", cookieParam4.Name, "OverloadContructorName2");
			Assert.AreEqual ("SalaryCookie", cookieParam4.CookieName, "OverloadContructorCookieName2");
			Assert.AreEqual (TypeCode.UInt64, cookieParam4.Type, "OverloadContructorType");  		
			
			
		}

		[Test]
		public void CookieParameter_AssignToDefaultProperties ()
		{
			CookieParameterPoker cookieParam = new CookieParameterPoker ();
			cookieParam.CookieName = "CookieNameTest";
			Assert.AreEqual ("CookieNameTest", cookieParam.CookieName, "AssignToCookieName");			
		}

		//Protected Methods

		[Test]
		public void CookieParameter_Clone ()
		{
			HttpCookie cookie = new HttpCookie ("EmployeeCookie");
			CookieParameterPoker cookieParam = new CookieParameterPoker ("Employee", TypeCode.String ,"EmployeeCookie");
			CookieParameter clonedParam = (CookieParameter) cookieParam.DoClone ();
			Assert.AreEqual ("Employee", clonedParam.Name, "CookieParameterCloneName");
			Assert.AreEqual (TypeCode.String, clonedParam.Type, "CookieParameterCloneType");
			Assert.AreEqual ("EmployeeCookie", clonedParam.CookieName, "CookieParameterCloneCookieName");			
		}

		[Test]
		public void CookieParameter_Evaluate ()
		{
			CookieParameterPoker cookieParam = new CookieParameterPoker ("Salary", TypeCode.Int64, "SalaryCookie");
			HttpRequest request = new HttpRequest (String.Empty, "http://www.mono-project.com", String.Empty);
			HttpResponse response= new HttpResponse (new StringWriter());			
			HttpCookie cookie = new HttpCookie ("SalaryCookie", "1000");			
			Label lbl = new Label ();
			string value = (string) cookieParam.DoEvaluate (null, lbl);
			Assert.AreEqual (null, value, "EvaluateWhenNullContext");
			HttpContext context = new HttpContext (request,response);
			response.Cookies.Add (cookie) ;
			request.Cookies.Add (cookie);			
			value = (string)cookieParam.DoEvaluate (context, lbl);
			Assert.AreEqual ("1000", value, "EvaluateCookieValue1");
			cookie.Value = "2000";
			value = (string) cookieParam.DoEvaluate (context, lbl);
			Assert.AreEqual ("2000", value, "EvaluateCookieValue2");
			
		}

	}
}
#endif
