//
// System.Web.UI.WebControls.ImageButton.cs
//
// Authors:
//   Gaurav Vaish (gvaish@iitk.ac.in)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) Gaurav Vaish (2002)
// (C) 2003 Andreas Nahr
//

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Web;
using System.Web.UI;
using System.ComponentModel;

namespace System.Web.UI.WebControls
{
	[DefaultEvent("Click")]
	public class ImageButton: Image, IPostBackDataHandler, IPostBackEventHandler
	{
		private static readonly object ClickEvent   = new object();
		private static readonly object CommandEvent = new object();

		private int x, y;

		public ImageButton(): base()
		{
		}

		[DefaultValue (true), Bindable (false), WebCategory ("Behavior")]
		[WebSysDescription ("Determines if validation is performed when clicked.")]
		public bool CausesValidation
		{
			get
			{
				object o = ViewState["CausesValidation"];
				if(o!=null)
					return (bool)o;
				return true;
			}
			set
			{
				ViewState["CausesValidation"] = value;
			}
		}

		[DefaultValue (""), Bindable (true), WebCategory ("Behavior")]
		[WebSysDescription ("An argument for the Command of this control.")]
		public string CommandArgument
		{
			get
			{
				object o = ViewState["CommandArgument"];
				if(o!=null)
					return (string)o;
				return String.Empty;
			}
			set
			{
				ViewState["CommandArgument"] = value;
			}
		}

		[DefaultValue (""), WebCategory ("Behavior")]
		[WebSysDescription ("The name of the Command of this control.")]
		public string CommandName
		{
			get
			{
				object o = ViewState["CommandName"];
				if(o!=null)
					return (string)o;
				return String.Empty;
			}
			set
			{
				ViewState["CommandName"] = value;
			}
		}

		protected override HtmlTextWriterTag TagKey
		{
			get
			{
				return HtmlTextWriterTag.Input;
			}
		}

		[WebCategory ("Action")]
		[WebSysDescription ("Raised when the LinkButton is clicked.")]
		public event ImageClickEventHandler Click
		{
			add
			{
				Events.AddHandler(ClickEvent, value);
			}
			remove
			{
				Events.RemoveHandler(ClickEvent, value);
			}
		}

		[WebCategory ("Action")]
		[WebSysDescription ("Raised when a LinkButton Command is executed.")]
		public event CommandEventHandler Command
		{
			add
			{
				Events.AddHandler(CommandEvent, value);
			}
			remove
			{
				Events.RemoveHandler(CommandEvent, value);
			}
		}

		protected override void AddAttributesToRender(HtmlTextWriter writer)
		{
			writer.AddAttribute(HtmlTextWriterAttribute.Type, "image");
			writer.AddAttribute(HtmlTextWriterAttribute.Name, UniqueID);
			if(Page != null && CausesValidation)
			{
				if(Page.Validators.Count > 0)
				{
					writer.AddAttribute(HtmlTextWriterAttribute.Onclick, Utils.GetClientValidatedEvent(Page));
					writer.AddAttribute("language", "javascript");
				}
			}
			base.AddAttributesToRender(writer);
		}

		protected virtual void OnClick(ImageClickEventArgs e)
		{
			if(Events != null)
			{
				ImageClickEventHandler iceh = (ImageClickEventHandler)(Events[ClickEvent]);
				if(iceh != null)
					iceh(this, e);
			}
		}

		protected virtual void OnCommand(CommandEventArgs e)
		{
			if(Events != null)
			{
				CommandEventHandler ceh = (CommandEventHandler)(Events[CommandEvent]);
				if(ceh != null)
					ceh(this, e);
				RaiseBubbleEvent(this, e);
			}
		}

		protected override void OnPreRender(EventArgs e)
		{
			if(Page != null)
			{
				Page.RegisterRequiresPostBack(this);
			}
		}

		bool IPostBackDataHandler.LoadPostData(string postDataKey, NameValueCollection postCollection)
		{
			string xCoord = postCollection[UniqueID + ".x"];
			string yCoord = postCollection[UniqueID + ".y"];
			if(xCoord != null && yCoord != null && xCoord.Length > 0 && yCoord.Length > 0)
			{
				x = Int32.Parse(xCoord);
				y = Int32.Parse(yCoord);
				Page.RegisterRequiresRaiseEvent(this);
			}
			return false;
		}

		void IPostBackDataHandler.RaisePostDataChangedEvent()
		{
		}

		void IPostBackEventHandler.RaisePostBackEvent(string eventArgument)
		{
			if(CausesValidation)
				Page.Validate();
			OnClick(new ImageClickEventArgs(x, y));
			OnCommand(new CommandEventArgs(CommandName, CommandArgument));
		}
	}
}
