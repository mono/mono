/**
 * Namespace: System.Web.UI.WebControls
 * Class:     CheckBox
 * 
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Status:  100%
 * 
 * (C) Gaurav Vaish (2001)
 * Thanks to Leen Toelen (toelen@hotmail.com)'s classes that helped me
 * to write the contents of the function LoadPostData(...)
 */

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	public class CheckBox : WebControl, IPostBackDataHandler
	{
		private static readonly object CheckedChangedEvent = new object();

		public CheckBox()
		{
			//
		}
		
		public virtual bool AutoPostBack
		{
			get
			{
				object o = ViewState["AutoPostBack"];
				if(o!=null)
					return (bool)AutoPostBack;
				return false;
			}
			set
			{
				ViewState["AutoPostBack"] = value;
			}
		}
		
		public virtual bool Checked
		{
			get
			{
				object o = ViewState["Checked"];
				if(o!=null)
					return (bool)o;
				return false;
			}
			set
			{
				ViewState["Checked"] = value;
			}
		}

		public virtual string Text
		{
			get
			{
				object o = ViewState["Text"];
				if(o!=null)
					return (string)o;
				return String.Empty;
			}
			set
			{
				ViewState["Text"] = value;
			}
		}
		
		public virtual TextAlign TextAlign
		{
			get
			{
				object o = ViewState["TextAlign"];
				if(o!=null)
					return (TextAlign)o;
				return TextAlign.Right;
			}
			set
			{
				if(!System.Enum.IsDefined(typeof(TextAlign), value))
					throw new ArgumentException();
				ViewState["TextAlign"] = value;
			}
		}
		
		public event EventHandler CheckedChanged
		{
			add
			{
				Events.AddHandler(CheckedChangedEvent, value);
			}
			remove
			{
				Events.RemoveHandler(CheckedChangedEvent, value);
			}
		}
		
		protected virtual void OnCheckedChanged(EventArgs e)
		{
			if(Events!=null)
			{
				EventHandler eh = (EventHandler)(Events[CheckedChangedEvent]);
				if(eh!=null)
					eh(this, e);
			}
		}
		
		protected virtual void OnPreRender(EventArgs e)
		{
			throw new NotImplementedException();
		}
		
		protected override void Render(HtmlTextWriter writer)
		{
			//TODO: THE LOST WORLD!
			bool hasBeginRendering = false;
			if(ControlStyleCreated)
			{
				if(!ControlStyle.IsEmpty)
				{
					hasBeginRendering = true;
					ControlStyle.AddAttributesToRender(writer, this);
				}
			}
			if(!Enabled)
			{
				hasBeginRendering = true;
				writer.AddAttribute(HtmlTextWriterAttribute.Disbled, "disabled");
			}
			if(ToolTip.Length > 0)
			{
				hasBeginRendering = true;
				writer.AddAttribute(HtmlTextWriterAttribute.Title, ToolTip);
			}
			if(Attributes.Count > 0)
			{
				hasBeginRendering = true;
				Attributes.AddAttributes(writer);
			}
			if(hasBeginRendering)
				writer.RenderBeginTag(HtmlTextWriterTag.Span);
			if(Text.Length > 0)
			{
				if(TextAlign == TextAlign.Right)
				{
					writer.AddAttribute(HtmlTextWriterAttribute.For, ClientID);
					writer.RenderBeginTag(HtmlTextWriterTag.Label);
					writer.Write(Text);
					writer.RenderEngTag();
					RenderInputTag(writer, ClientID);
				} else
				{
					RenderInputTag(writer, ClientID);
					writer.AddAttribute(HtmlTextWriterAttribute.For, ClientID);
					writer.RenderBeginTag(HtmlTextWriterTag.Label);
					writer.Write(Text);
				}
			}
			if(hasBeginRendering)
				writer.RenderEndTag();
		}
		
		internal virtual void RenderInputTag(HtmlTextWriter writer, string clientId)
		{
			writer.AddAttribute(HtmlTextWriterAttribute.Id, clientId);
			writer.AddAttribute(HtmlTextWriterAttribute.Type, "checkbox");
			writer.AddAttribute(HtmlTextWriterAttribute.Name, UniqueID);
			if(Checked)
			{
				writer.AddAttribute(HtmlTextWriterAttribute.Checked, "checked");
			}
			if(AutoPostBack)
			{
				writer.AddAttribute(HtmlTextWriterAttribute.OnClick,Page.GetPostBackClientEvent(this, String.Empty));
				writer.AddAttribute("language", "javascript");
			}
			if(AccessKey.Length > 0)
			{
				writer.AddAttribute(HtmlTextWriterAttribute.AccessKey, AccessKey);
			}
			if(TabIndex != 0)
			{
				writer.AddAttribute(HtmlTextWriterAttribute.TabIndex, TabIndex.ToString(NumberFormatInfo.InvariantInfo));
			}
			writer.RenderBeginTag(HtmlTextWriterTag.Input);
			writer.RenderEndTag();
		}
		
		public bool LoadPostData(string postDataKey, NameValueCollection postCollection)
		{
			string postedVal = postCollection[postDataKey];
			bool   postChecked = false;
			if(postedVal != null)
			{
				postChecked = postedVal.Length > 0;
			}
			Checked = postChecked;
			return (postChecked == Checked == false);
		}
		
		public void RaisePostDataChangedEvent()
		{
			OnCheckedChanged(EventArgs.Empty);
		}
	}
}
