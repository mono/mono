/**
 * Namespace: System.Web.UI.WebControls
 * Class:     WebControl
 * 
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Status:  40%
 * 
 * (C) Gaurav Vaish (2001)
 */

using System;
using System.Web;
using System.Web.UI;
using System.Drawing;
using System.Collections.Specialized;

namespace System.Web.UI.WebControls
{
	public class WebControl : Control, IAttributeAccessor
	{
		//TODO: A list of private members may be incomplete

		private HtmlTextWriterTag   writerTag;
		private string              stringTag;
		private AttributeCollection attributes;
		private StateBag            attributeState;
		private Style               controlStyle;
		private bool                enabled;
		private HtmlTextWriterTag   tagKey;
		private string              tagName;

		// TODO: The constructors definitions
		protected WebControl()
		{
			//todo: what now? To be rendered as SPAN tag!
			base();
			Initialize();
		}
		
		public WebControl(HtmlTextWriterTag tag)
		{
			//FIXME: am i right?
			base();
			writerTag = tag;
			//stringTag = null;
			Initialize();
		}

		protected WebControl(string tag)
		{
			//FIXME: am i right?
			base();
			stringTag = tag;
			//writerTag = null;
			Initialize();
		}
		
		private void Initialize()
		{
			controlStyle   = null;
			enabled        = true;
			tagName        = null;
			attributeState = null;
		}
		
		public virtual string AccessKey
		{
			get
			{
				object o = ViewState["AccessKey"];
				if(o!=null)
					return (string)o;
				return String.Empty;
			}
			set
			{
				ViewState["AccessKey"] = value;
			}
		}
		
		public AttributeCollection Attributes
		{
			get
			{
				throw new NotImplementedException("FIXME: \"Internal\" method calls");
				if(attributes==null)
				{
					//TODO: From where to get StateBag and how? I think this method is OK!
					if(attributeState == null)
					{
						attributeState = new StateBag(true);
						//FIXME: Uncomment the following in the final release
						// commented because of the assembly problem.
						//The function TrackViewState() is internal
						/*
						if(IsTrackingViewState)
						{
							attributeState.TrackViewState();
						}
						*/
					}
					attributes = new AttributeCollection(attributeState);
				}
				return attributes;
			}
		}
		
		public Style ControlStyle		
		{
			get
			{
				throw new NotImplementedException("FIXME: \"Internal\" method calls");
				if(controlStyle == null)
				{
					controlStyle = CreateControlStyle();
					//FIXME: Uncomment the following in the final release
					// commented because of the assembly problem.
					//The functions TrackViewState() and LoadViewState() are internal
					/*
					if(IsTrackingViewState)
					{
						controlStyle.TrackViewState();
					}
					controlStyle.LoadViewState(null);
					*/
				}
				return controlStyle;
			}
		}
		
		public bool ControlStyleCreated
		{
			get
			{
				return (controlStyle!=null);
			}
		}
		
		public virtual string CssClass
		{
			get
			{
				return ControlStyle.CssClass;
			}
			set
			{
				ControlStyle.CssClass = value;
			}
		}
		
		public virtual bool Enabled
		{
			get
			{
				return enabled;
			}
			set
			{
				enabled = value;
			}
		}

		public virtual FontInfo Font
		{
			get
			{
				return ControlStyle.Font;
			}
		}
		
		public virtual Color ForeColor
		{
			get
			{
				return ControlStyle.ForeColor;
			}
			set
			{
				ControlStyle.ForeColor = value;
			}
		}
		
		public virtual Unit Height
		{
			get
			{
				return ControlStyle.Height;
			}
			set
			{
				ControlStyle.Height = value;
			}
		}
		
		public CssStyleCollection Style
		{
			get
			{
				return Attributes.CssStyle;
			}
		}
		
		public virtual short TabIndex
		{
			get
			{
				object o = ViewState["TabIndex"];
				if(o!=null)
					return (short)o;
				return 0;
			}
			set
			{
				if(value < -32768 || value > 32767)
					throw new ArgumentException();
				ViewState["TabIndex"] = value;
			}
		}
		
		public virtual string ToolTip
		{
			get
			{
				object o = ViewState["ToolTip"];
				if(o!=null)
					return (string)o;
				return String.Empty;
			}
			set
			{
				ViewState["ToolTip"] = value;
			}
		}
		
		public virtual Unit Width
		{
			get
			{
				return ControlStyle.Width;
			}
			set
			{
				ControlStyle.Width = value;
			}
		}
		
		public void ApplyStyle(Style s)
		{
			/* FIXME: Again internal problem
			if(!ControlStyle.IsEmpty)
			{
			*/
				ControlStyle.CopyFrom(s);
			//}
			throw new NotImplementedException("FIXME: \"Internal\" method calls");
		}
		
		public void CopyBaseAttributes(WebControl controlSrc)
		{
			//TODO: tocopy
			/*
			 * AccessKey, Enabled, ToolTip, TabIndex, Attributes
			*/
			AccessKey  = controlSrc.AccessKey;
			Enabled    = controlSrc.Enabled;
			ToolTip    = controlSrc.ToolTip;
			TabIndex   = controlSrc.TabIndex;
			Attributes = controlSrc.Attributes;
			throw new NotImplementedException();
		}
		
		public void MergeStyle(Style s)
		{
			ControlStyle.MergeWith(s);
		}
		
		public virtual void RenderBeginTag(HtmlTextWriter writer)
		{
			AddAttributesToRender(writer);
			if(TagKey!=null)
			{
				writer.RenderBeginTag(TagKey);
			}
			writer.RenderBeginTag(tagName);
		}
		
		public virtual void RenderEndTag(HtmlTextWriter writer)
		{
			writer.RenderEndTag();
		}
		
		protected virtual HtmlTextWriterTag TagKey
		{
			get
			{
				//FIXME: should I do new HtmlTextWriter()?
				throw new NotImplementedException();
				return tagKey;
			}
		}
		
		protected virtual string TagName
		{
			get
			{
				if(tagName==null)
				{
					if(tagKey == null)
					{
						//FIXME: If it is null, is this the right way? I don't think
						throw new NotImplementedException();
						tagKey = new HtmlTextWriter();
					}
					tagName = Enum.Format(typeof(tagKey), tagKey, "G").ToLower();
				}
				return tagName;
			}
		}
		
		protected virtual void AddAttributesToRender(HtmlTextWriter writer)
		{
			if(ID!=null)
			{
				writer.AddAttribute(HtmlTextWriterAttribute.Id, ClientID);
			}
			if(AccessKey.Length>0)
			{
				writer.AddAttribute(HtmlTextWriterAttribute.AccessKey, AccessKey);
			}
			if(!Enabled)
			{
				writer.AddAttribute(HtmlTextWriterAttribute.Disabled, "disabled");
			}
			if(ToolTip.Length>0)
			{
				writer.AddAttribute(HtmlTextWriterAttribute.Title, ToolTip);
			}
			if(TabIndex != 0)
			{
				writer.AddAttribute(HtmlTextWriterAttribute.TabIndex, TabIndex.ToString());
			}
			if(ControlStyleCreated)
			{
				if(!ControlStyle.IsEmpty)
				{
					ControlStyle.AddAttributesToRender(writer, this);
				}
			}
			if(attributeState!=null)
			{
				IEnumerator ie = Attributes.Keys.GetEnumerator();
				do
				{
					writer.AddAttribute((string)ie.Current, Attributes.Item[(string)ie.Current]);
				} while(ie.MoveNext());
			}
		}
		
		protected virtual Style CreateControlStyle()
		{
			return new Style(ViewState);
		}
		
		protected override void LoadViewState(object savedState)
		{
			throw new NotImplementedException();
			//TODO: Load viewStates
			/*
			 * May be will have to first look at Control::LoadViewState 
			*/
		}
		
		protected override void Render(HtmlTextWriter writer)
		{
			RenderBeginTag(writer);
			RenderContents(writer);
			RenderEngTags(writer);
		}
		
		protected override void RenderContents(HtmlTextWriter writer)
		{
			base.RenderContents(writer);
		}
		
		protected override object SaveViewState()
		{
			throw new NotImplementedException();
			//TODO: Implement me!
		}
		
		protected override void TrackViewState()
		{
			base.TrackViewState();
			if(ControlStyleCreated)
			{
				ControlStyle.TrackViewState();
			}
			if(attributeState!=null)
			{
				attributeState.TrackViewState();
			}
		}
		
		// Implemented procedures
		//TODO: The scope of the functions - is public valid. Test thru Reflection
		public string GetAttribute(string key)
		{
			if(Attributes!=null)
				return (string)Attributes[key];
		}
		
		public void SetAttribute(string key, string val)
		{
			Attributes[key] = value;
		}

	}
}
