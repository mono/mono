/**
 * Namespace: System.Web.UI.WebControls
 * Class:     ValidationSummary
 *
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Implementation: yes
 * Status:  ??%
 *
 * (C) Gaurav Vaish (2002)
 */

using System;
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
