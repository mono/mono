/**
 * Namespace: System.Web.UI.WebControls
 * Class:     Button
 * 
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Status:  20%
 * 
 * (C) Gaurav Vaish (2001)
 */

using System;
using System.ComponentModel;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	public class Button : WebControl, IPostBackEventHandler
	{
		private static readonly object ClickEvent   = new object();
		private static readonly object CommandEvent = new object();

		//private EventHandlerList ehList;

		public Button()
		{
			// TODO: Initialization
		}

		[Bindable(true)]
		public bool CausesValidation
		{
			get
			{
				Object cv = ViewState["CausesValidation"];
				if(cv!=null)
					return (Boolean)cv;
				return true;
			}
			set
			{
				//causesValidation = value;
				ViewState["CausesValidation"] = value;
			}
		}

		[Bindable(true)]
		public string CommandArgument
		{
			get
			{
				//return commandArgument;
				string ca = (string) ViewState["CommandArgument"];
				if(ca!=null)
					return ca;
				return String.Empty;
			}
			set
			{
				//commandArgument = value;
				ViewState["CommandArgument"] = value;
			}
		}

		public string CommandName
		{
			get
			{
				string cn = (string)ViewState["CommandName"];
				if(cn!=null)
					return cn;
				return String.Empty;
			}
			set
			{
				ViewState["CommandArgument"] = value;
			}
		}

		[
			Bindable(true),
			DefaultValueAttribute("")
		]
		//[WebSysDescriptionAttribute("Button_Text")]
		//[WebCategoryAttribute("Appearance")]
		public string Text
		{
			get
			{
				string text = (string)ViewState["Text"];
				if(text!=null)
					return text;
				return String.Empty;
			}
			set
			{
				ViewState["Text"] = value;
			}
		}

		public event EventHandler Click
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
		
		protected virtual void OnClick(EventArgs e)
		{
			if(Events != null)
			{
				EventHandler eh = (EventHandler)(Events[ClickEvent]);
				if(eh!= null)
					eh(this,e);
			}
		}

		protected virtual void OnCommand(CommandEventArgs e)
		{
			if(Events != null)
			{
				EventHandler eh = (EventHandler)(Events[CommandEvent]);
				if(eh!= null)
					eh(this,e);
			}
		}
		
		public void RaisePostBackEvent(string eventArgument)
		{
			/* Will have to see what work needs to be done before I actually call OnClick
			 * Basically I have to see what is to be done with the string argument
			*/
			if(CausesValidation)
			{
				Page.Validate();
				OnClick(new EventArgs());
				OnCommand(new CommandEventArgs(CommandName, CommandArgument));
			}
		}
		
		protected override void AddAttributesToRender(HtmlTextWriter writer)
		{
			//??
			writer.AddAttribute(HtmlTextWriterAttribute.Type,"submit");
			writer.AddAttribute(HtmlTextWriterAttribute.Name,base.UniqueID);
			writer.AddAttribute(HtmlTextWriterAttribute.Value,Text);
			if(Page!=null)
			{
				if(CausesValidation)
				{
					//Page.Validators.Count
					//writer.AddAttribute(HtmlTextWriterAttribute.OnClick, <<The validationcode>>);
					writer.AddAttribute("language","javascript");
				}
			}
		}
		
		protected override void RenderContents(HtmlTextWriter writer)
		{
			// Preventing
		}
	}
}
