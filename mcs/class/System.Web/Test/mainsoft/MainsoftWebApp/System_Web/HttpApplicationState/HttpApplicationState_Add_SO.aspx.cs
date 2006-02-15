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
using System.Collections;

namespace GHTTests.System_Web_dll.System_Web
{
	public class HttpApplicationState_Add_SO
		: GHTBaseWeb 
	{
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

		private class CustomClass
		{
			public string str = "";
			public int num = 0;
			public string Print()
			{
				return str;
			}
		}

		private void Page_Load(object sender, EventArgs e)
		{
			HtmlForm form1 = (HtmlForm) (HtmlForm)this.FindControl("Form1");
			this.GHTTestBegin(form1);
			this.GHTSubTestBegin("GHTSubTest1");
			try
			{
				IEnumerator enumerator5 = null;
				this.Application.Clear();
				this.Application.Add("var1", "variable1");
				try
				{
					enumerator5 = this.Application.GetEnumerator();
					while (enumerator5.MoveNext())
					{
						string text1 = (string)(enumerator5.Current);
						if (this.Application[text1] == null)
						{
							this.GHTSubTestAddResult("Application(\"" + text1 + "\") = Nothing");
							continue;
						}
						this.GHTSubTestAddResult((string)(("Application(\"" + text1) + "\") = " + this.Application[text1]));
					}
				}
				finally
				{
					if (enumerator5 is IDisposable)
					{
						((IDisposable) enumerator5).Dispose();
					}
				}
			}
			catch (Exception exception8)
			{
				// ProjectData.SetProjectError(exception8);
				Exception exception1 = exception8;
				this.GHTSubTestUnexpectedExceptionCaught(exception1);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			this.GHTSubTestBegin("GHTSubTest2");
			try
			{
				IEnumerator enumerator4 = null;
				this.Application.Add("var2", "variable2");
				this.Application.Add("var3", "variable3");
				try
				{
					enumerator4 = this.Application.GetEnumerator();
					while (enumerator4.MoveNext())
					{
						string text2 = (string)(enumerator4.Current);
						if (this.Application[text2] == null)
						{
							this.GHTSubTestAddResult("Application(\"" + text2 + "\") = Nothing");
							continue;
						}
						this.GHTSubTestAddResult((string)("Application(\"" + text2 + "\") = " + this.Application[text2]));
					}
				}
				finally
				{
					if (enumerator4 is IDisposable)
					{
						((IDisposable) enumerator4).Dispose();
					}
				}
			}
			catch (Exception exception9)
			{
				// ProjectData.SetProjectError(exception9);
				Exception exception2 = exception9;
				this.GHTSubTestUnexpectedExceptionCaught(exception2);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			this.GHTSubTestBegin("GHTSubTest3");
			try
			{
				IEnumerator enumerator3 = null;
				this.Application.Add("var4", "");
				this.Application.Add("", "variable5");
				try
				{
					enumerator3 = this.Application.GetEnumerator();
					while (enumerator3.MoveNext())
					{
						string text3 = (string)(enumerator3.Current);
						if (this.Application[text3] == null)
						{
							this.GHTSubTestAddResult("Application(\"" + text3 + "\") = Nothing");
							continue;
						}
						this.GHTSubTestAddResult((string)("Application(\"" + text3 + "\") = " + this.Application[text3]));
					}
				}
				finally
				{
					if (enumerator3 is IDisposable)
					{
						((IDisposable) enumerator3).Dispose();
					}
				}
			}
			catch (Exception exception10)
			{
				// ProjectData.SetProjectError(exception10);
				Exception exception3 = exception10;
				this.GHTSubTestUnexpectedExceptionCaught(exception3);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			this.GHTSubTestBegin("GHTSubTest4");
			try
			{
				IEnumerator enumerator2 = null;
				this.Application.Add("var2", "variable2");
				this.Application.Add("var3", "variable3");
				try
				{
					enumerator2 = this.Application.GetEnumerator();
					while (enumerator2.MoveNext())
					{
						string text4 = (string)(enumerator2.Current);
						if (this.Application[text4] == null)
						{
							this.GHTSubTestAddResult("Application(\"" + text4 + "\") = Nothing");
							continue;
						}
						this.GHTSubTestAddResult((string)("Application(\"" + text4 + "\") = " + this.Application[text4]));
					}
				}
				finally
				{
					if (enumerator2 is IDisposable)
					{
						((IDisposable) enumerator2).Dispose();
					}
				}
			}
			catch (Exception exception11)
			{
				// ProjectData.SetProjectError(exception11);
				Exception exception4 = exception11;
				this.GHTSubTestUnexpectedExceptionCaught(exception4);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			this.GHTSubTestBegin("GHTSubTest5");
			try
			{
				this.Application.Add("int1", 1);
				this.Application.Add("int2", -1);
				this.Application.Add("nothing1", null);
				IEnumerator enumerator1 = this.Application.GetEnumerator();
				try
				{
					while (enumerator1.MoveNext())
					{
						string text5 = (string)(enumerator1.Current);
						if (this.Application[text5] == null)
						{
							this.GHTSubTestAddResult("Application(\"" + text5 + "\") = Nothing");
							continue;
						}
						this.GHTSubTestAddResult((string)("Application(\"" + text5 + "\") = " + this.Application[text5]));
					}
				}
				finally
				{
					if (enumerator1 is IDisposable)
					{
						((IDisposable) enumerator1).Dispose();
					}
				}
			}
			catch (Exception exception12)
			{
				// ProjectData.SetProjectError(exception12);
				Exception exception5 = exception12;
				this.GHTSubTestUnexpectedExceptionCaught(exception5);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			this.GHTSubTestBegin("GHTSubTest6");
			try
			{
				DateTime time1 = DateTime.Now;
				this.Application.Add("date", time1);
				this.GHTSubTestAddResult(this.Application["date"].GetType().ToString());
			}
			catch (Exception exception13)
			{
				// ProjectData.SetProjectError(exception13);
				Exception exception6 = exception13;
				this.GHTSubTestUnexpectedExceptionCaught(exception6);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			this.GHTSubTestBegin("GHTSubTest7");
			try
			{
				HttpApplicationState_Add_SO.CustomClass class1 = new HttpApplicationState_Add_SO.CustomClass();
				this.Application.Add("c", class1);
				this.GHTSubTestAddResult(this.Application["c"].GetType().ToString());
			}
			catch (Exception exception14)
			{
				// ProjectData.SetProjectError(exception14);
				Exception exception7 = exception14;
				this.GHTSubTestUnexpectedExceptionCaught(exception7);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			this.GHTTestEnd();
		}
 
	}
}
