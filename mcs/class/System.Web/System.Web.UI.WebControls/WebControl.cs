//
// System.Web.UI.WebControls.WebControl.cs
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
using System.ComponentModel;
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

		[DefaultValue (""), Bindable (true), WebCategory ("Behavior")]
		[WebSysDescription ("A keyboard shortcut for the WebControl.")]
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
		[Browsable (false), DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[WebSysDescription ("Attribute tags for the Webcontrol.")]
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
						if(IsTrackingViewState)
						{
							attributeState.TrackViewState();
						}
					}
					attributes = new AttributeCollection(attributeState);
				}
				return attributes;
			}
		}

		[DefaultValue (null), Bindable (true), WebCategory ("Appearance")]
		[TypeConverter (typeof (WebColorConverter))]
		[WebSysDescription ("The background color for the WebControl.")]
		public virtual Color BackColor
		{
			get {
				if (!ControlStyleCreated)
					return Color.Empty;
				return ControlStyle.BackColor;
			}

			set {
				ControlStyle.BackColor = value;
			}
		}

		[DefaultValue (null), Bindable (true), WebCategory ("Appearance")]
		[TypeConverter (typeof (WebColorConverter))]
		[WebSysDescription ("The border color for the WebControl.")]
		public virtual Color BorderColor
		{
			get {
				if (!ControlStyleCreated)
					return Color.Empty;
				return ControlStyle.BorderColor;
			}

			set {
				ControlStyle.BorderColor = value;
			}
		}

		[DefaultValue (typeof(BorderStyle), "NotSet"), Bindable (true), WebCategory ("Appearance")]
		[WebSysDescription ("The style/type of the border used for the WebControl.")]
		public virtual BorderStyle BorderStyle
		{
			get {
				if (!ControlStyleCreated)
					return BorderStyle.NotSet;
				return ControlStyle.BorderStyle;
			}

			set {
				ControlStyle.BorderStyle = value;
			}
		}

		[DefaultValue (null), Bindable (true), WebCategory ("Appearance")]
		[WebSysDescription ("The width of the border used for the WebControl.")]
		public virtual Unit BorderWidth
		{
			get {
				if (!ControlStyleCreated)
					return Unit.Empty;
				return ControlStyle.BorderWidth;
			}

			set {
				if (value.Value < 0)
					throw new ArgumentException();
				ControlStyle.BorderWidth = value;
			}
		}

		[Browsable (false), DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[WebSysDescription ("The style used to display this Webcontrol.")]
		public Style ControlStyle
		{
			get
			{
				if(controlStyle == null)
				{
					controlStyle = CreateControlStyle();
					if(IsTrackingViewState)
					{
						controlStyle.TrackViewState();
					}
					controlStyle.LoadViewState(null);
				}
				return controlStyle;
			}
		}

		[Browsable (false), DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[WebSysDescription ("Determines if a style exists for this Webcontrol.")]
		public bool ControlStyleCreated
		{
			get
			{
				return (controlStyle!=null);
			}
		}

		[DefaultValue (""), Bindable (true), WebCategory ("Appearance")]
		[WebSysDescription ("The cascading stylesheet class that is associated with this WebControl.")]
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

		[DefaultValue (true), Bindable (true), WebCategory ("Behavior")]
		[WebSysDescription ("The activation state of this WebControl.")]
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

		[DefaultValue (null), NotifyParentProperty (true), WebCategory ("Appearance")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[WebSysDescription ("The font of this WebControl.")]
		public virtual FontInfo Font
		{
			get
			{
				return ControlStyle.Font;
			}
		}

		[DefaultValue (null), Bindable (true), WebCategory ("Appearance")]
		[TypeConverter (typeof (WebColorConverter))]
		[WebSysDescription ("The color that is used to paint the primary display of the WebControl.")]
		public virtual Color ForeColor
		{
			get {
				if (!ControlStyleCreated)
					return Color.Empty;
				return ControlStyle.ForeColor;
			}

			set {
				ControlStyle.ForeColor = value;
			}
		}

		[DefaultValue (null), Bindable (true), WebCategory ("Layout")]
		[WebSysDescription ("The height of this WebControl.")]
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

		[Browsable (false), DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[WebSysDescription ("Direct access to the styles used for this Webcontrol.")]
		public CssStyleCollection Style
		{
			get
			{
				return Attributes.CssStyle;
			}
		}

		[DefaultValue (0), WebCategory ("Behavior")]
		[WebSysDescription ("The order in which this WebControl gets tabbed through.")]
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

		[DefaultValue (""), Bindable (true), WebCategory ("Behavior")]
		[WebSysDescription ("A tooltip that is shown when hovering the mouse above the WebControl.")]
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

		[DefaultValue (null), Bindable (true), WebCategory ("Layout")]
		[WebSysDescription ("The width of this WebControl.")]
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
			if (s != null && !s.IsEmpty)
				ControlStyle.CopyFrom (s);
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

		protected override void LoadViewState (object savedState)
		{
			if (savedState != null) {
				Triplet saved = (Triplet) savedState;
				base.LoadViewState (saved.First);
				if (ControlStyleCreated)
					ControlStyle.LoadViewState (saved.Second);
				if (attributeState != null)
					attributeState.LoadViewState (saved.Third);
			}
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

		protected override object SaveViewState()
		{
			object baseView = base.SaveViewState ();
			object controlView = null;
			if (ControlStyleCreated)
				controlView = ControlStyle.SaveViewState();

			object attrView = null;
			if (attributeState != null)
				attrView = attributeState.SaveViewState();

			return new Triplet (baseView, controlView, attrView);
		}

		protected override void TrackViewState()
		{
			base.TrackViewState();
			if (ControlStyleCreated)
				ControlStyle.TrackViewState ();
			if (attributeState != null)
				attributeState.TrackViewState ();
		}

		string IAttributeAccessor.GetAttribute(string key)
		{
			if(Attributes!=null)
				return Attributes[key] as string;
			return null;
		}

		void IAttributeAccessor.SetAttribute(string key, string value)
		{
			Attributes[key] = value;
		}
	}
}
