/**
 * Namespace: System.Web.UI.WebControls
 * Class:     ImageButton
 *
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Implementation: yes
 * Status:  100%
 *
 * (C) Gaurav Vaish (2001)
 */

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
