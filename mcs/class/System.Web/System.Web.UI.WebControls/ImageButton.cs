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
using System.Collections.Specialized;
using System.Web;
using System.Web.UI;
using System.ComponentModel;

namespace System.Web.UI.WebControls
{
#if NET_2_0
	[DesignerAttribute ("System.Web.UI.Design.WebControls.PreviewControlDesigner, System.Design, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.ComponentModel.Design.IDesigner")]
#endif
	[DefaultEvent("Click")]
	public class ImageButton: Image, IPostBackDataHandler, IPostBackEventHandler
	{
		private static readonly object ClickEvent   = new object();
		private static readonly object CommandEvent = new object();

		private int x, y;

		public ImageButton(): base()
		{
		}

#if NET_2_0
		[ThemeableAttribute (false)]
#else
		[Bindable (false)]
#endif
		[DefaultValue (true), WebCategory ("Behavior")]
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

#if NET_2_0
	    [ThemeableAttribute (false)]
#endif
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

#if NET_2_0
	    [ThemeableAttribute (false)]
#endif
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

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
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
			string id = postCollection[UniqueID];
			if(xCoord != null && yCoord != null && xCoord.Length > 0 && yCoord.Length > 0)
			{
				x = Int32.Parse(xCoord);
				y = Int32.Parse(yCoord);
				Page.RegisterRequiresRaiseEvent(this);
			} else if (id != null)
			{
                                //
                                // This is a workaround for bug #49819. It appears that the .x and .y
                                // values are not being posted, and only the x value is being posted
                                // with the ctrl's id as the key.
                                //
				x = Int32.Parse (id);
				Page.RegisterRequiresRaiseEvent (this);
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
