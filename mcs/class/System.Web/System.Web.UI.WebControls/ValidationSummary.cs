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

		[DefaultValue (typeof (ValidationSummaryDisplayMode), "BulletList"), Bindable (true), WebCategory ("Appearance")]
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
				return ForeColor;
			}
			set
			{
				ForeColor = value;
			}
		}

		[DefaultValue (false), Bindable (true), WebCategory ("Behavior")]
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

		[DefaultValue (true), Bindable (true), WebCategory ("Behavior")]
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

		[MonoTODO("FIXME_See_Comments")]
		protected override void AddAttributesToRender(HtmlTextWriter writer)
		{
			AddAttributesToRender(writer);
			if(uplevelRender)
			{
				//FIXME: This is not the case always. I forgot the case when it is absent.
				// something to do with the ID's value? or ClienID's value itself?
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
	}
}
