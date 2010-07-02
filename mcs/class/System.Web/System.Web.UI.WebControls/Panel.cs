//
// System.Web.UI.WebControls.Panel.cs
//
// Authors:
//	Ben Maurer (bmaurer@novell.com)
//
// (C) 2005 Novell, Inc (http://www.novell.com)
//
// TODO: Are we missing something in LoadViewState?
// What to do in AddParsedSubObject
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

using System.ComponentModel;
using System.Security.Permissions;

namespace System.Web.UI.WebControls {

	// CAS
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	// attributes
	[Designer ("System.Web.UI.Design.WebControls.PanelDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	[ParseChildren (false)]
	[PersistChildren (true)]
#if !NET_4_0
	[ToolboxData ("<{0}:Panel runat=server>Panel</{0}:Panel>")]
#endif
	public class Panel : WebControl {

		public Panel () : base (HtmlTextWriterTag.Div) 
		{
		}
		
		protected override void AddAttributesToRender (HtmlTextWriter w)
		{
			base.AddAttributesToRender (w);
			
			string image = BackImageUrl;
			if (image != "") {
				image = ResolveClientUrl (image);
#if !NET_2_0 // see HtmlTextWriter.WriteStyleAttribute(string, string, bool) 
				image = String.Concat ("url(", image, ")");
#endif
				w.AddStyleAttribute (HtmlTextWriterStyle.BackgroundImage, image);
			}

#if NET_2_0
			if (!String.IsNullOrEmpty (DefaultButton) && Page != null) {
				Control button = FindControl (DefaultButton);
				if (button == null || !(button is IButtonControl))
					throw new InvalidOperationException (String.Format ("The DefaultButton of '{0}' must be the ID of a control of type IButtonControl.", ID));

				Page.ClientScript.RegisterWebFormClientScript ();

				w.AddAttribute ("onkeypress",
						"javascript:return " + Page.WebFormScriptReference + ".WebForm_FireDefaultButton(event, '" + button.ClientID + "')");
			}

			if (Direction != ContentDirection.NotSet) {
				w.AddAttribute (HtmlTextWriterAttribute.Dir, Direction == ContentDirection.RightToLeft ? "rtl" : "ltr", false);
			}

			switch (ScrollBars) {
			case ScrollBars.Auto:
				w.AddStyleAttribute (HtmlTextWriterStyle.Overflow, "auto");
				break;
			case ScrollBars.Both:
				w.AddStyleAttribute (HtmlTextWriterStyle.Overflow, "scroll");
				break;
			case ScrollBars.Horizontal:
				w.AddStyleAttribute (HtmlTextWriterStyle.OverflowX, "scroll");
				break;
			case ScrollBars.Vertical:
				w.AddStyleAttribute (HtmlTextWriterStyle.OverflowY, "scroll");
				break;
			}

#endif

			if (!Wrap) {
#if NET_2_0
				w.AddStyleAttribute (HtmlTextWriterStyle.WhiteSpace, "nowrap");
#else
				w.AddAttribute (HtmlTextWriterAttribute.Nowrap, "nowrap");
#endif
			}

			string align = "";

			switch (HorizontalAlign) {
			case HorizontalAlign.Center: align = "center"; break;
			case HorizontalAlign.Justify: align = "justify"; break;
			case HorizontalAlign.Left: align = "left"; break;
			case HorizontalAlign.Right: align = "right"; break;
			}

			if (align != "")
#if NET_2_0
				w.AddStyleAttribute (HtmlTextWriterStyle.TextAlign, align);
#else
				w.AddAttribute (HtmlTextWriterAttribute.Align, align);
#endif
		}
#if NET_2_0
		PanelStyle PanelStyle {
			get { return (ControlStyle as PanelStyle); }
		}
#if NET_4_0
		[UrlProperty]
#else
		[Bindable (true)]
#endif
		[DefaultValue ("")]
		[Editor ("System.Web.UI.Design.ImageUrlEditor, " + Consts.AssemblySystem_Design, typeof (System.Drawing.Design.UITypeEditor))]
		[WebSysDescription ("")]
		[WebCategory ("Appearance")]
		public virtual string BackImageUrl {
			get {
				if (ControlStyleCreated) {
					if (PanelStyle != null)
						return PanelStyle.BackImageUrl;
					else
						return ViewState.GetString ("BackImageUrl", String.Empty);
				}
				return String.Empty;
			}
			set {
				if(PanelStyle!=null)
					PanelStyle.BackImageUrl = value;
				else
					ViewState ["BackImageUrl"] = value;
			}
		}
#if !NET_4_0
		[Bindable (true)]
#endif
		[DefaultValue (HorizontalAlign.NotSet)]
		[WebSysDescription ("")]
		[WebCategory ("Layout")]
		public virtual HorizontalAlign HorizontalAlign {
			get {
				if (ControlStyleCreated) {
					if (PanelStyle != null)
						return PanelStyle.HorizontalAlign;
					else
						return ViewState ["HorizontalAlign"] != null ? (HorizontalAlign) ViewState ["HorizontalAlign"] : HorizontalAlign.NotSet;
				}
				return HorizontalAlign.NotSet;
			}
			set {
				if (PanelStyle != null)
					PanelStyle.HorizontalAlign = value;
				else
					ViewState ["HorizontalAlign"] = value;
			}
		}
#if !NET_4_0
		[Bindable (true)]
#endif
		[DefaultValue (true)]
		[WebSysDescription ("")]
		[WebCategory ("Layout")]
		public virtual bool Wrap {
			get {
				if (ControlStyleCreated) {
					if (PanelStyle != null)
						return PanelStyle.Wrap;
					else
						return ViewState.GetBool ("Wrap", true);
				}
				return true;
			}
			set {
				if (PanelStyle != null)
					PanelStyle.Wrap = value;
				else
					ViewState ["Wrap"] = value;
			}
		}
		
		[ThemeableAttribute (false)]
#if NET_4_0
		[DefaultValue ("")]
#endif
		public virtual string DefaultButton {
			get {
				return ViewState.GetString ("DefaultButton", String.Empty);
			}
			set {
				ViewState ["DefaultButton"] = value;
			}
		}
#if NET_4_0
		[DefaultValue (ContentDirection.NotSet)]
#endif
		public virtual ContentDirection Direction {
			get {
				if (ControlStyleCreated) {
					if (PanelStyle != null)
						return PanelStyle.Direction;
					else
						return ViewState ["Direction"] != null ? (ContentDirection) ViewState ["Direction"] : ContentDirection.NotSet;
				}
				return ContentDirection.NotSet;
			}
			set {
				if (PanelStyle != null)
					PanelStyle.Direction = value;
				else
					ViewState ["Direction"] = value;
			}
		}

		[LocalizableAttribute (true)]
#if NET_4_0
		[DefaultValue ("")]
#endif
		public virtual string GroupingText {
			get {
				return ViewState.GetString ("GroupingText", String.Empty);
			}
			set {
				ViewState ["GroupingText"] = value;
			}
		}
#if NET_4_0
		[DefaultValue (ScrollBars.None)]
#endif
		public virtual ScrollBars ScrollBars {
			get {
				if (ControlStyleCreated) {
					if (PanelStyle != null)
						return PanelStyle.ScrollBars;
					else
						return ViewState ["ScrollBars"] != null ? (ScrollBars) ViewState ["Direction"] : ScrollBars.None;
				}
				return ScrollBars.None;
			}
			set {
				if (PanelStyle != null)
					PanelStyle.ScrollBars = value;
				else
					ViewState ["ScrollBars"] = value;
			}
		}
#if NET_4_0
		public override bool SupportsDisabledAttribute {
			get { return RenderingCompatibilityLessThan40; }
		}
#endif
		protected override Style CreateControlStyle ()
		{
			return new PanelStyle (ViewState);
		}

		public override void RenderBeginTag (HtmlTextWriter writer)
		{
			base.RenderBeginTag (writer);
			if (!String.IsNullOrEmpty (GroupingText)) {
				writer.RenderBeginTag (HtmlTextWriterTag.Fieldset);
				writer.RenderBeginTag (HtmlTextWriterTag.Legend);
				writer.Write (GroupingText);
				writer.RenderEndTag ();
			}
		}

		public override void RenderEndTag (HtmlTextWriter writer)
		{
			if (!String.IsNullOrEmpty (GroupingText)) {
				writer.RenderEndTag (); // Fieldset
			}
			base.RenderEndTag (writer);
		}
#endif
	}
}
