/**
 * Namespace: System.Web.UI.WebControls
 * Class:     LinkButton
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
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	[DefaultEvent("Click")]
	[DefaultProperty("Text")]
	//[Designer("??")]
	[ControlBuilder(typeof(LinkButtonControlBuilder))]
	//[DataBindingHandler("??")]
	[ParseChildren(false)]
	[ToolboxData("<{0}:LinkButton runat=\"server\">LinkButton</{0}:LinkButton>")]
	public class LinkButton : WebControl, IPostBackEventHandler
	{
		private static readonly object ClickEvent   = new object();
		private static readonly object CommandEvent = new object();

		public LinkButton(): base(HtmlTextWriterTag.A)
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

		public virtual string Text
		{
			get
			{
				object o = ViewState["Text"];
				if(o != null)
				{
					return (string)o;
				}
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
				if(eh != null)
					eh(this, e);
			}
		}

		protected virtual void OnCommand(CommandEventArgs e)
		{
			if(Events != null)
			{
				CommandEventHandler ceh = (CommandEventHandler)(Events[CommandEvent]);
				if(ceh != null)
					ceh(this, e);
			}
		}

		void IPostBackEventHandler.RaisePostBackEvent(string eventArgument)
		{
			if(CausesValidation)
			{
				Page.Validate();
			}
			OnClick(new EventArgs());
			OnCommand( new CommandEventArgs(CommandName, CommandArgument));
		}

		protected override void AddAttributesToRender(HtmlTextWriter writer)
		{
			base.AddAttributesToRender(writer);
			if(Enabled && Page != null)
			{
				if(CausesValidation && Page.Validators.Count > 0)
				{
					writer.AddAttribute(HtmlTextWriterAttribute.Href, "javscript:" + Utils.GetClientValidatedPostBack(this));
					return;
				}
				writer.AddAttribute(HtmlTextWriterAttribute.Href, Page.GetPostBackClientHyperlink(this, ""));
			}
		}

		protected override void AddParsedSubObject(object obj)
		{
			if(HasControls())
			{
				AddParsedSubObject(obj);
				return;
			}
			if(obj is LiteralControl)
			{
				Text = ((LiteralControl)obj).Text;
				return;
			}
			if(Text.Length > 0)
			{
				AddParsedSubObject(Text);
				Text = String.Empty;
			}
			AddParsedSubObject(obj);
		}

		protected override void LoadViewState(object savedState)
		{
			if(savedState != null)
			{
				base.LoadViewState(savedState);
				string savedText = (string)ViewState["Text"];
				if(savedText != null)
					Text = savedText;
			}
		}

		protected override void RenderContents(HtmlTextWriter writer)
		{
			if(HasControls())
			{
				RenderContents(writer);
				return;
			}
			writer.Write(Text);
		}
	}
}
