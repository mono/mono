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
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

using GHTWebControls;

namespace GHTTests
{
    /// <summary>
    /// Summary description for GHTBaseWeb.
    /// </summary>
    public class GHTBaseWeb : System.Web.UI.Page
    {
        public Control GHTActiveForm;
        public Control GHTActiveSubTest;
        public int GHTActiveSubTestId = 0;

        protected override void OnInit(System.EventArgs e)
        {
            base.OnInit(e);
        }

        /// <summary>
        /// Save a reference to the Form.
        /// </summary>
        public virtual void GHTTestBegin(Control theForm)
        {
            GHTActiveForm = theForm;
        }

        /// <summary>
        /// not implemented yet. use it as a stub
        /// </summary>
        public void GHTTestEnd()
        {
            
        }
 
        /// <summary>
        /// 
        /// </summary>
        public void GHTSubTestBegin()
        {
            GHTSubTestBegin("");
        }
        public void GHTSubTestBegin(string Description)
        {
            GHTSubTestCreateNew(Description);
        }

        /// <summary>
        /// not implemented yet. use it as a stub
        /// </summary>
        public void GHTSubTestEnd()
        {
            
        }
 
        /// <summary>
        /// Add a result to Sub Test.
        /// </summary>
        /// <param name="SubTestControl">The SubTestControl to add the result to.</param>
        /// <param name="TraceText">The text to add.</param>
        /// <param name="asText">Whether to include as plain text, or to interpret special HTML charachters as HTML.</param>
		public void GHTSubTestAddResult(Control SubTestControl, string TraceText, bool asText)
        {
			if (asText)
			{
				TraceText = GHTHtmlToText(TraceText);
			}
			
			Label lbl = new Label();
			lbl.Text = "<br>" + TraceText;
			SubTestControl.Controls.Add(lbl);

//				TableCell tempCell = new TableCell();
//				TableRow tempRow = new TableRow();
//				tempCell.Controls.Add( new LiteralControl(TraceText));
//				tempRow.Controls.Add(tempCell); 
//				SubTestControl.Controls.Add(tempRow);

        }
		public void GHTSubTestAddResult(string TraceText)
		{
			GHTSubTestAddResult(TraceText, false);
		}
		public void GHTSubTestAddResult(string TraceText, bool asText)
		{
			GHTSubTestAddResult(GHTActiveSubTest, TraceText, asText);
		}
		public void GHTSubTestAddResult(Control SubTestControl, string TraceText)
		{
			GHTSubTestAddResult(SubTestControl, TraceText,false);
		}
		// Replaces Html special charachters to escape charachters in the returned string:
		// Orig --> Result
		//-----		   ------
		//  <				&lt;
		//	 >				&gt;
		//	 "				&quot;
		// &				&amp;
		public string GHTHtmlToText(string a_text)
		{
			string res = string.Empty;
			res = a_text.Replace("<", "&lt;");
			res = res.Replace(">", "&gt;");
			res = res.Replace("\"", "&quot;");
			return res;
		}

		protected void GHTSubTestExpectedExceptionCaught(System.Exception ex)
		{
			GHTSubTestAddResult("Test passed. Expected exception was caught.");
		}
		protected void GHTSubTestExpectedExceptionNotCaught(string ExceptionName)
		{
			GHTSubTestAddResult("Test failed. Expected " + ExceptionName + " exception was not caught.");
		}
		protected void GHTSubTestUnexpectedExceptionCaught(System.Exception ex)
		{
			string traceText = string.Empty;
			traceText += "Test Failed. Unxpected ";
			traceText += ex.GetType().Name;
			traceText += " exception was caught";
			traceText += "<br>Stack Trace: ";
			traceText += ex.ToString();

			GHTSubTestAddResult(traceText);
		}

		protected void Compare(Object Result, Object ExpectedResult)
		{
			if (Result.Equals(ExpectedResult))
				GHTSubTestAddResult("Test Passed.");
			else
				GHTSubTestAddResult("Test Failed. Result:" + GHTNormalizeToString(Result.ToString()) + ". Expected Result:" + GHTNormalizeToString(ExpectedResult.ToString()));
											 
		}
        /// <summary>
        /// Create a new SubTest conteiner.
        /// </summary>
        protected GHTWebControls.GHTSubTest GHTSubTestCreateNew()
        {
            return GHTSubTestCreateNew("");
        }
        protected GHTWebControls.GHTSubTest GHTSubTestCreateNew(string Description)
        {
            return GHTSubTestCreateNew(GHTActiveForm, Description);
        }
        protected GHTWebControls.GHTSubTest GHTSubTestCreateNew(Control theForm, string Description)
        {
			if (GHTActiveSubTestId == 0)
			{
				GHTActiveSubTestId++;
			}

			while (theForm.FindControl("GHTSubTest" + GHTActiveSubTestId) != null)
			{
				GHTActiveSubTestId++;
			}

            GHTActiveForm = theForm;
            GHTWebControls.GHTSubTest subtest = new GHTWebControls.GHTSubTest();
            subtest.ID = "GHTSubTest" + GHTActiveSubTestId;
			subtest.Description = Description;
			subtest.Attributes.Add("TestName",Description);
            theForm.Controls.Add(subtest);
            GHTActiveSubTest = subtest;
            return subtest;
        }
        
        /// <summary>
        /// Create a new Element (e.g. Control).
        /// </summary>
        protected Object GHTElementClone(Type type)
        {
            return type.GetConstructor(Type.EmptyTypes).Invoke(null);
        }

        /// <summary>
        /// Common Tests of WebControl.
        /// </summary>
        protected void GHTTestCommon_WebControl(WebControl obj, string MemberName)
        {
            WebControl objNew;
            bool bExceptionCaught = false;
            MemberName = MemberName.ToLower();

            //AccessKey
            if (MemberName.CompareTo("AccessKey".ToLower())== 0)
            {
                //Press Alt-Y to get focus here
                obj.AccessKey = ""; //empty is ok
                obj.AccessKey = "Y";
                objNew = (WebControl)GHTElementClone(obj.GetType());
                // test Exceptions
                try
                {
                    objNew.AccessKey = "XX";
                    GHTActiveSubTest.Controls.Add(objNew);
                }
                catch (ArgumentException)
                {
                    bExceptionCaught = true;
                }
                if (bExceptionCaught == true)
                {
                    GHTSubTestAddResult(GHTActiveSubTest, MemberName + " ArgumentException OK");
                }
                bExceptionCaught = false;
                
            }

            //Attributes tested at System.Web.UI.AttributeCollection

            //BackColor
            if (MemberName.CompareTo("BackColor".ToLower())== 0)
            {
                obj.BackColor = System.Drawing.Color.AliceBlue;
                
                objNew = (WebControl)GHTElementClone(obj.GetType());
                objNew.BackColor = System.Drawing.Color.Magenta;
                GHTActiveSubTest.Controls.Add(objNew);
            }

            //BorderColor
            if (MemberName.CompareTo("BorderColor".ToLower())== 0)
            {
                objNew = (WebControl)GHTElementClone(obj.GetType());
                objNew.BorderColor = System.Drawing.Color.AliceBlue;
                GHTActiveSubTest.Controls.Add(objNew);
            }

            //BorderStyle
            if (MemberName.CompareTo("BorderStyle".ToLower())== 0)
            {
                objNew = (WebControl)GHTElementClone(obj.GetType());
                objNew.BorderStyle = System.Web.UI.WebControls.BorderStyle.Dotted;
                GHTActiveSubTest.Controls.Add(objNew);
            }

            //BorderWidth
            if (MemberName.CompareTo("BorderWidth".ToLower())== 0)
            {
                objNew = (WebControl)GHTElementClone(obj.GetType());
                objNew.BorderWidth = 10;
                GHTActiveSubTest.Controls.Add(objNew);
            }

            // ControlStyle
            // ControlStyleCreated
            
            //CssClass
            if (MemberName.CompareTo("CssClass".ToLower())== 0)
            {
                objNew = (WebControl)GHTElementClone(obj.GetType());
                objNew.CssClass = "zoobie";
                GHTActiveSubTest.Controls.Add(objNew);
            }

            //Enabled
            if (MemberName.CompareTo("Enabled".ToLower())== 0)
            {
                objNew = (WebControl)GHTElementClone(obj.GetType());
                objNew.Enabled = true;
                GHTActiveSubTest.Controls.Add(objNew);
            }

            //Font
            if (MemberName.CompareTo("Font".ToLower())== 0)
            {
                GHTSubTestAddResult(GHTActiveSubTest, MemberName + " " + obj.Font.ToString());
            }

            //ForeColor
            if (MemberName.CompareTo("ForeColor".ToLower())== 0)
            {
                objNew = (WebControl)GHTElementClone(obj.GetType());
                objNew.ForeColor = System.Drawing.Color.Cornsilk;
                GHTActiveSubTest.Controls.Add(objNew);
            }

            //Height
            if (MemberName.CompareTo("Height".ToLower())== 0)
            {
                objNew = (WebControl)GHTElementClone(obj.GetType());
                objNew.Height = 30;
                GHTActiveSubTest.Controls.Add(objNew);
            }

            //Style
            if (MemberName.CompareTo("Style".ToLower())== 0)
            {
                GHTSubTestAddResult(GHTActiveSubTest, MemberName + " " + obj.Style.ToString());
            }

            //            if (MemberName.CompareTo("TagKey".ToLower())== 0)
            //            {
            //                obj.TagKey = "TagKey";
            //            }
            //            if (MemberName.CompareTo("TagName".ToLower())== 0)
            //            {
            //                obj.TagName = "TagName";
            //            }

            //TabIndex
            if (MemberName.CompareTo("TabIndex".ToLower())== 0)
            {
                objNew = (WebControl)GHTElementClone(obj.GetType());
                objNew.TabIndex = 2;
                GHTActiveSubTest.Controls.Add(objNew);
            }

            //ToolTip
            if (MemberName.CompareTo("ToolTip".ToLower())== 0)
            {
                objNew = (WebControl)GHTElementClone(obj.GetType());
                objNew.ToolTip = "ToolTip";
                GHTActiveSubTest.Controls.Add(objNew);
            }

            //Width
            if (MemberName.CompareTo("Width".ToLower())== 0)
            {
                objNew = (WebControl)GHTElementClone(obj.GetType());
                objNew.Width = 60;
                GHTActiveSubTest.Controls.Add(objNew);
            }

        }

		protected void GHTHeader(string text)
			{
				Label  header = new Label();
				header.Text = "<br>" + text + "<br>";
				header.Font.Bold = true;
				header.Font.Underline = true;
				header.Font.Size = new FontUnit(FontSize.Larger);
				GHTActiveForm.Controls.Add(header);
			}

		/// <summary>
		/// Removes tarailing @... and preceding '_' from strings if they are found
		/// </summary>
		/// <param name="orig">The original ToString result.</param>
		/// <returns>Normalized string.</returns>
		public string GHTNormalizeToString(string a_toNormalize)
		{
			//Remove the @ at the end of the tostring.
			int atIndex = a_toNormalize.LastIndexOf('@');
			if (atIndex > -1)
			{
				a_toNormalize = a_toNormalize.Remove(atIndex, a_toNormalize.Length - atIndex);
			}
			//remove the preceding '_'
			if (a_toNormalize.StartsWith("_") )
			{
				a_toNormalize = a_toNormalize.Substring(1);
			}
			return a_toNormalize;
		}
		/// <summary>
		/// Returns the normalized to string of this page.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return GHTNormalizeToString(base.ToString());
		}
    }
}

