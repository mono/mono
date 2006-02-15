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

namespace GHTTests.System_Web_dll.System_Web_UI_WebControls
{
	public class BaseDataList_IsBindableType_T
		: GHTDataListBase
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

		private Type [] marrBindaleType = {typeof(System.Boolean), 
											  typeof(System.Byte), 
											  typeof(System.Int16),
											  typeof(System.UInt16),
											  typeof(System.Int32), 
											  typeof(System.UInt32),
											  typeof(System.Int64), 
											  typeof(System.UInt64), 
											  typeof(System.Char), 
											  typeof(System.Double), 
											  typeof(System.Single), 
											  typeof(System.DateTime), 
											  typeof(System.Decimal), 
											  typeof(System.SByte), 
											  typeof(MyPrivateClass), 
											  typeof(System.String)};

		private void Page_Load(object sender, System.EventArgs e) 
		{
			//Put user code to initialize the page here
			HtmlForm frm = (HtmlForm)FindControl("form1");
			GHTTestBegin(frm);
			Test(typeof(System.Web.UI.WebControls.DataGrid));
			Test(typeof(System.Web.UI.WebControls.DataList));
			GHTTestEnd();

		}
		private void Test(Type CtlType)
		{
			Type type1;
			bool flag1;
			try
			{
				this.GHTSubTestBegin("BaseDataList_" + CtlType.Name + "_IsBindableType1");
				Type[] typeArray1 = this.marrBindaleType;
				for (int num1 = 0; num1 < typeArray1.Length; num1++)
				{
					type1 = typeArray1[num1];
					flag1 = BaseDataList.IsBindableType(type1);
					this.GHTSubTestAddResult(type1.ToString() + " is bindable = " + flag1.ToString());
				}
			}
			catch (Exception exception4)
			{
				this.GHTSubTestUnexpectedExceptionCaught(exception4);
			}
			this.GHTSubTestEnd();
			try
			{
				this.GHTSubTestBegin("BaseDataList_" + CtlType.Name + "_IsBindableType2");
				type1 = null;
				flag1 = BaseDataList.IsBindableType(null);
				this.GHTSubTestAddResult("Nothing is bindable = " + flag1.ToString());
				this.GHTSubTestExpectedExceptionNotCaught("NullReferenceException");
			}
			catch (NullReferenceException exception5)
			{
				this.GHTSubTestExpectedExceptionCaught(exception5);
				return;
			}
			catch (Exception exception6)
			{
				this.GHTSubTestUnexpectedExceptionCaught(exception6);
				return;
			}
		}
 
		private class MyPrivateClass
		{
		}
	}
}
