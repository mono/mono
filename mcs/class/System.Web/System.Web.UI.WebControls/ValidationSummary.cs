//
// System.Web.UI.WebControls.ValidationSummary.cs
//
// Authors:
//   Gaurav Vaish (gvaish@iitk.ac.in)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) Gaurav Vaish (2002)
// (C) 2003 Andreas Nahr
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
using System.ComponentModel;
using System.Drawing;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	public class ValidationSummary : WebControl
	{
		private bool uplevelRender;

		public ValidationSummary(): base(HtmlTextWriterTag.Div)
		{
			uplevelRender = false;
			ForeColor     = Color.Red;
		}

#if !NET_2_0
		[Bindable (true)]
#endif
		[DefaultValue (typeof (ValidationSummaryDisplayMode), "BulletList"), WebCategory ("Appearance")]
		[WebSysDescription ("The type of how validation summaries should be displayed.")]
		public ValidationSummaryDisplayMode DisplayMode
		{
			get
			{
				object o = ViewState["DisplayMode"];
				if(o != null)
					return (ValidationSummaryDisplayMode)o;
				return ValidationSummaryDisplayMode.BulletList;
			}
			set
			{
				if(!Enum.IsDefined(typeof(ValidationSummaryDisplayMode), value))
					throw new ArgumentException();
				ViewState["DisplayMode"] = value;
			}
		}

#if NET_2_0
	    [ThemeableAttribute (false)]
#endif
		[DefaultValue (true), WebCategory ("Behavior")]
		[WebSysDescription ("Determines if the validation summary should be updated directly on the client using script code.")]
		public bool EnableClientScript
		{
			get
			{
				object o = ViewState["EnableClientScript"];
				if(o != null)
					return (bool)o;
				return true;
			}
			set
			{
				ViewState["EnableClientScript"] = value;
			}
		}

		[DefaultValue (null)]
		public override Color ForeColor
		{
			get
			{
				return base.ForeColor;
			}
			set
			{
				base.ForeColor = value;
			}
		}

#if !NET_2_0
		[Bindable (true)]
#endif
		[DefaultValue (false), WebCategory ("Behavior")]
		[WebSysDescription ("Determines if the validation summary should display a message box on the client if an uplevel browser is used.")]
		public bool ShowMessageBox
		{
			get
			{
				object o = ViewState["ShowMessageBox"];
				if(o != null)
					return (bool)o;
				return false;
			}
			set
			{
				ViewState["ShowMessageBox"] = value;
			}
		}

#if !NET_2_0
		[Bindable (true)]
#endif
		[DefaultValue (true), WebCategory ("Behavior")]
		[WebSysDescription ("Determines if the validation summary should display a summary.")]
		public bool ShowSummary
		{
			get
			{
				object o = ViewState["ShowSummary"];
				if(o != null)
					return (bool)o;
				return true;
			}
			set
			{
				ViewState["ShowSummary"] = value;
			}
		}

#if NET_2_0
		[Localizable (true)]
#endif
		[DefaultValue (""), Bindable (true), WebCategory ("Appearance")]
		[WebSysDescription ("A text that is diplayed as a header for the validation report.")]
		public string HeaderText
		{
			get
			{
				object o = ViewState["HeaderText"];
				if(o != null)
					return (string)o;
				return String.Empty;
			}
			set
			{
				ViewState["HeaderText"] = value;
			}
		}

		protected override void AddAttributesToRender(HtmlTextWriter writer)
		{
			base.AddAttributesToRender(writer);
			if(uplevelRender)
			{
				if(ID == null || ID.Length == 0)
					writer.AddAttribute("id", ClientID);
				if(HeaderText.Length > 0)
					writer.AddAttribute("headertext", HeaderText, true);
				if(ShowMessageBox)
					writer.AddAttribute("showmessagebox", "True");
				if(!ShowSummary)
					writer.AddAttribute("showsummary", "False");
				if(DisplayMode != ValidationSummaryDisplayMode.BulletList)
				{
					writer.AddAttribute("displaymode", PropertyConverter.EnumToString(typeof(ValidationSummaryDisplayMode), DisplayMode));
				}
			}
		}
                
                protected override void OnPreRender(EventArgs e)
                {
                }
                
                protected override void Render(HtmlTextWriter writer)
		{
			if (!base.Enabled) return;
	
			string[] messages;
			bool toDisplay;
			bool showSummary;
	
			if (base.Site != null && base.Site.DesignMode)
			{
				showSummary = true;
				messages = new string[]{HttpRuntime.FormatResourceString("ValSummary_error_message_1"),
							HttpRuntime.FormatResourceString("ValSummary_error_message_2")};
				toDisplay = true;
				uplevelRender = false;
			}
			else
			{
				showSummary = false;
				messages = null;
	
				//Messages count
				int numOfMsg = 0;
				for (int i = 0; i < base.Page.Validators.Count; i++)
				{
					IValidator currentValidator = base.Page.Validators[i];
					if (!currentValidator.IsValid)
					{
						showSummary = true;
						if (currentValidator.ErrorMessage != null &&
						    currentValidator.ErrorMessage.Length != 0)
							numOfMsg++;
					}
				}
	
				if (numOfMsg != 0)
				{
					messages = new string[(int)numOfMsg];
					int cur_msg = 0;
					for (int i = 0; i < base.Page.Validators.Count; i++)
					{
						IValidator currentValidator = base.Page.Validators[i];
						if (!currentValidator.IsValid &&
						     currentValidator.ErrorMessage != null &&
						     currentValidator.ErrorMessage.Length != 0) 
							messages[cur_msg++] = String.Copy(currentValidator.ErrorMessage);
					}
				}
	
				toDisplay = ShowSummary ? showSummary : false;
				if (!toDisplay && uplevelRender)
					base.Style["display"] = "none";
			}
			if (base.Page != null)
			{
				base.Page.VerifyRenderingInServerForm(this);
			}
			bool tagRequired = !uplevelRender ? toDisplay : true;
			if (tagRequired)
			{
				base.RenderBeginTag(writer);
			}
			if (toDisplay)
			{
				string str1, str2, str3, str4, str5;
			       
	
				switch (DisplayMode)
				{
				case ValidationSummaryDisplayMode.List:
					str1 = "<br>";
					str2 = "";
					str3 = "";
					str4 = "<br>";
					str5 = "";
					break;
					
				case ValidationSummaryDisplayMode.SingleParagraph:
					str1 = " ";
					str2 = "";
					str3 = "";
					str4 = " ";
					str5 = "<br>";
					break;
					
				default:
					str1 = "";
					str2 = "<ul>";
					str3 = "<li>";
					str4 = "</li>";
					str5 = "</ul>";
					break;
				}
				if (HeaderText.Length > 0)
				{
					writer.Write(HeaderText);
					writer.Write(str1);
				}
				writer.Write(str2);
				if (messages != null)
				{
					for (int i = 0; i < (int)messages.Length; i++)
					{
						writer.Write(str3);
						writer.Write(messages[i]);
						writer.Write(str4);
					}
				}
				writer.Write(str5);
			}
			if (tagRequired)
			{
				base.RenderEndTag(writer);
			}
		}
	}
}
