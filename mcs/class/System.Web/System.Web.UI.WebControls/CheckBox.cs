/**
 * Namespace: System.Web.UI.WebControls
 * Class:     CheckBox
 *
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Implementation: yes
 * Status:  100%
 *
 * (C) Gaurav Vaish (2001)
 * Thanks to Leen Toelen (toelen@hotmail.com)'s classes that helped me
 * to write the contents of the function LoadPostData(...)
 */

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Globalization;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	public class CheckBox : WebControl, IPostBackDataHandler
	{
		private static readonly object CheckedChangedEvent = new object();
		
		public CheckBox(): base(HtmlTextWriterTag.Input)
		{
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
		
		private bool SaveCheckedViewState
		{
			get
			{
				if(Events[CheckedChangedEvent] != null)
				{
					if(!Enabled)
						return true;
					if(GetType() == typeof(CheckBox))
					{
						return false;
					}
					if(GetType() == typeof(RadioButton))
					{
						return false;
					}
				}
				return true;
				
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
		
		protected override void OnPreRender(EventArgs e)
		{
			if(Page!=null)
			{
				if(Enabled)
				{
					Page.RegisterRequiresPostBack(this);
				}
			}
			if(SaveCheckedViewState)
			{
				ViewState.SetItemDirty("checked", false);
			}
		}
		
		protected override void Render(HtmlTextWriter writer)
		{
			bool hasBeginRendering = false;
			if(ControlStyleCreated)
				{
					//TODO: Uncomment this in final version
					/*
					if(!ControlStyle.IsEmpty)
						{
							hasBeginRendering = true;
							ControlStyle.AddAttributesToRender(writer, this);
						}
						*/
				}
				if(!Enabled)
					{
						hasBeginRendering = true;
						writer.AddAttribute(HtmlTextWriterAttribute.Disabled, "disabled");
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
							writer.RenderEndTag();
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
			throw new NotImplementedException("Calling some internal functions");
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
						writer.AddAttribute(HtmlTextWriterAttribute.Onclick,Page.GetPostBackClientEvent(this, String.Empty));
						writer.AddAttribute("language", "javascript");
					}
					if(AccessKey.Length > 0)
						{
							writer.AddAttribute(HtmlTextWriterAttribute.Accesskey, AccessKey);
						}
						if(TabIndex != 0)
							{
								writer.AddAttribute(HtmlTextWriterAttribute.Tabindex, TabIndex.ToString(NumberFormatInfo.InvariantInfo));
							}
							writer.RenderBeginTag(HtmlTextWriterTag.Input);
			writer.RenderEndTag();
		}
		
		bool IPostBackDataHandler.LoadPostData(string postDataKey, NameValueCollection postCollection)
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
		
		void IPostBackDataHandler.RaisePostDataChangedEvent()
		{
			OnCheckedChanged(EventArgs.Empty);
		}
	}
}
