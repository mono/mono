/**
 * Namespace: System.Web.UI.WebControls
 * Class:     WebControl
 *
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Implementation: yes
 * Status:  40%
 *
 * (C) Gaurav Vaish (2001)
 */

using System;
using System.Collections;
using System.Web;
using System.Web.UI;
using System.Drawing;
using System.Collections.Specialized;

namespace System.Web.UI.WebControls
{
	[PersistChildrenAttribute(false)]
	[ParseChildrenAttribute(true)]
	public class WebControl : Control, IAttributeAccessor
	{
		//TODO: A list of private members may be incomplete

		private HtmlTextWriterTag   tagKey;
		private string              stringTag;
		private AttributeCollection attributes;
		private StateBag            attributeState;
		private Style               controlStyle;
		private bool                enabled;
		private string              tagName;

		// TODO: The constructors definitions
		protected WebControl () : this (HtmlTextWriterTag.Span)
		{
		}

		public WebControl(HtmlTextWriterTag tag): base()
		{
			//FIXME: am i right?
			tagKey = tag;
			//stringTag = null;
			Initialize();
		}

		protected WebControl(string tag): base()
		{
			//FIXME: am i right?
			stringTag = tag;
			Initialize();
		}

		private void Initialize()
		{
			controlStyle   = null;
			enabled        = true;
			tagName        = stringTag;
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

		[MonoTODO("FIXME_Internal_method_calls")]
		public AttributeCollection Attributes
		{
			get
			{
				if(attributes==null)
				{
					//FIXME: From where to get StateBag and how? I think this method is OK!
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

		public virtual Color BackColor
		{
			get
			{
				object o = ViewState["BackColor"];
				if(o != null)
				{
					return (Color)o;
				}
				return Color.Empty;
			}
			set
			{
				ViewState["BackColor"] = value;
			}
		}

		public virtual Color BorderColor
		{
			get
			{
				object o = ViewState["BorderColor"];
				if(o != null)
				{
					return (Color)o;
				}
				return Color.Empty;
			}
			set
			{
				ViewState["BorderColor"] = value;
			}
		}

		[MonoTODO("FIXME_Internal_method_calls")]
		public virtual BorderStyle BorderStyle
		{
			get
			{
				object o = ViewState["BorderStyle"];
				if(o != null)
				{
					return (BorderStyle)o;
				}
				return BorderStyle.NotSet;
			}
			set
			{
				if(!Enum.IsDefined(typeof(BorderStyle), value))
				{
					throw new ArgumentException();
				}
				ViewState["BorderStyle"] = value;
			}
		}

		public virtual Unit BorderWidth
		{
			get
			{
				object o = ViewState["BorderWidth"];
				if(o != null)
				{
					return (Unit)o;
				}
				return Unit.Empty;
			}
			set
			{
				if(value.Value < 0)
				{
					throw new ArgumentException();
				}
				ViewState["BorderWidth"] = value;
			}
		}

		[MonoTODO("FIXME_Internal_method_calls")]
		public Style ControlStyle
		{
			get
			{
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

		[MonoTODO("FIXME_Internal_method_calls")]
		public void ApplyStyle(Style s)
		{
			/* FIXME: Again internal problem
			if(!ControlStyle.IsEmpty)
			{
			*/
				ControlStyle.CopyFrom(s);
			//}
		}

		public void CopyBaseAttributes(WebControl controlSrc)
		{
			/*
			 * AccessKey, Enabled, ToolTip, TabIndex, Attributes
			*/
			AccessKey  = controlSrc.AccessKey;
			Enabled    = controlSrc.Enabled;
			ToolTip    = controlSrc.ToolTip;
			TabIndex   = controlSrc.TabIndex;
			attributes = controlSrc.Attributes;
			AttributeCollection otherAtt = controlSrc.Attributes;
			foreach (string key in controlSrc.Attributes.Keys)
				Attributes [key] = otherAtt [key];
		}

		public void MergeStyle(Style s)
		{
			ControlStyle.MergeWith(s);
		}

		public virtual void RenderBeginTag(HtmlTextWriter writer)
		{
			AddAttributesToRender(writer);
			writer.RenderBeginTag(TagName);
		}

		public virtual void RenderEndTag(HtmlTextWriter writer)
		{
			writer.RenderEndTag();
		}

		protected virtual HtmlTextWriterTag TagKey
		{
			get
			{
				return tagKey;
			}
		}

		protected virtual string TagName
		{
			get
			{
				if(tagName == null && TagKey != 0)
				{
					tagName = Enum.Format(typeof(HtmlTextWriterTag), TagKey, "G").ToString();
				}
				// What if tagName is null and tagKey 0?
				return tagName;
			}
		}

		protected virtual void AddAttributesToRender(HtmlTextWriter writer)
		{
			if (Page != null)
				Page.VerifyRenderingInServerForm (this);

			if(ID!=null)
			{
				writer.AddAttribute(HtmlTextWriterAttribute.Id, ClientID);
			}
			if(AccessKey.Length>0)
			{
				writer.AddAttribute(HtmlTextWriterAttribute.Accesskey, AccessKey);
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
				writer.AddAttribute(HtmlTextWriterAttribute.Tabindex, TabIndex.ToString());
			}
			if(ControlStyleCreated)
			{
				if(!ControlStyle.IsEmpty)
				{
					ControlStyle.AddAttributesToRender(writer, this);
				}
			}
			if(attributeState != null){
				IEnumerator ie = Attributes.Keys.GetEnumerator ();
				while (ie.MoveNext ()){
					string key = (string) ie.Current;
					writer.AddAttribute (key, Attributes [key]);
				}
			}
		}

		protected virtual Style CreateControlStyle()
		{
			return new Style(ViewState);
		}

		[MonoTODO]
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
			RenderEndTag(writer);
		}

		protected virtual void RenderContents(HtmlTextWriter writer)
		{
			base.Render(writer);
		}

		[MonoTODO]
		protected override object SaveViewState()
		{
			throw new NotImplementedException();
			//TODO: Implement me!
		}

		protected override void TrackViewState()
		{
			TrackViewState();
			if(ControlStyleCreated)
			{
				ControlStyle.TrackViewState();
			}
			if(attributeState!=null)
			{
				attributeState.TrackViewState();
			}
		}

		string IAttributeAccessor.GetAttribute(string key)
		{
			if(Attributes!=null)
				return (string)Attributes[key];
			return null;
		}

		void IAttributeAccessor.SetAttribute(string key, string value)
		{
			Attributes[key] = value;
		}
	}
}
