//
// System.Web.UI.WebControls.Label.cs
//
// Authors:
//	Ben Maurer (bmaurer@novell.com)
//
// (C) 2005 Novell, Inc (http://www.novell.com)
//
// TODO: Are we missing something in LoadViewState?
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
	[AspNetHostingPermissionAttribute (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermissionAttribute (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	// attributes
	[ControlBuilder(typeof(HyperLinkControlBuilder))]
	[DataBindingHandler("System.Web.UI.Design.HyperLinkDataBindingHandler, " + Consts.AssemblySystem_Design)]
	[ParseChildren (false)]
#if NET_2_0
	[ToolboxData("<{0}:HyperLink runat=\"server\">HyperLink</{0}:HyperLink>")]
#else		
	[ToolboxData("<{0}:HyperLink runat=server>HyperLink</{0}:HyperLink>")]
#endif		
	[DefaultProperty("Text")]
	[Designer("System.Web.UI.Design.WebControls.HyperLinkDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	public class HyperLink : WebControl {

		public HyperLink () : base (HtmlTextWriterTag.A)
		{
		}
				
		protected override void AddAttributesToRender (HtmlTextWriter w)
		{
			base.AddAttributesToRender (w);
#if NET_2_0
			AddDisplayStyleAttribute (w);
#endif
			if (!IsEnabled)
				return;
			// add attributes - only if they're not empty
			string t = Target;
			string s = NavigateUrl;
			if (s.Length > 0)
#if TARGET_J2EE
				w.AddAttribute (HtmlTextWriterAttribute.Href, ResolveClientUrl (s, String.Compare (t, "_blank", StringComparison.InvariantCultureIgnoreCase) != 0));
#else
				w.AddAttribute (HtmlTextWriterAttribute.Href, ResolveClientUrl (s));
#endif
			if (t.Length > 0)
				w.AddAttribute (HtmlTextWriterAttribute.Target, t);
		}
		
		protected override void AddParsedSubObject (object obj)
		{
			if (HasControls ()) {
				base.AddParsedSubObject (obj);
				return;
			}
			
			LiteralControl lc = obj as LiteralControl;

			if (lc == null) {
				string s = Text;
				if (s.Length != 0) {
					Text = null;
					Controls.Add (new LiteralControl (s));
				}
				base.AddParsedSubObject (obj);
			} else {
				Text = lc.Text;
			}
		}
		

		[MonoTODO ("Why override?")]
		protected override void LoadViewState (object savedState)
		{
			base.LoadViewState (savedState);
		}
		
#if NET_2_0
		protected internal
#else		
		protected
#endif		
		override void RenderContents (HtmlTextWriter w)	
		{
			if (HasControls () || HasRenderMethodDelegate ()) {
				base.RenderContents (w);
				return;
			}
			string image_url = ImageUrl;
			if (image_url != "") {
				Image img = new Image ();
				img.ImageUrl = ResolveClientUrl (image_url);
				string str = Text;
				if (str != "")
					img.AlternateText = str;
				str = ToolTip;
				if (str != "")
					img.ToolTip = str;
				img.RenderControl (w);
			} else {
				w.Write (Text);
			}
			
		}

		[Bindable(true)]
		[DefaultValue("")]
		[Editor("System.Web.UI.Design.ImageUrlEditor, " + Consts.AssemblySystem_Design, typeof(System.Drawing.Design.UITypeEditor))]
#if NET_2_0
		[UrlProperty]
#endif		
		[WebSysDescription ("")]
		[WebCategory ("Appearance")]
		public virtual string ImageUrl {
			get {
				return ViewState.GetString ("ImageUrl", "");
			}
			set {
				ViewState ["ImageUrl"] = value;
			}	
		}
		
		[Bindable(true)]
		[DefaultValue("")]
		[Editor("System.Web.UI.Design.UrlEditor, " + Consts.AssemblySystem_Design, typeof(System.Drawing.Design.UITypeEditor))]
#if NET_2_0
		[UrlProperty]
#endif		
		[WebSysDescription ("")]
		[WebCategory ("Navigation")]
		public string NavigateUrl {
			get {
				return ViewState.GetString ("NavigateUrl", "");
			}
			set {
				ViewState ["NavigateUrl"] = value;
			}	
		}
		
#if ONLY_1_1
		[Bindable(true)]
#endif		
		[DefaultValue("")]
		[TypeConverter(typeof(System.Web.UI.WebControls.TargetConverter))]
		[WebSysDescription ("")]
		[WebCategory ("Navigation")]
		public string Target {
			get {
				return ViewState.GetString ("Target", "");
			}
			set {
				ViewState ["Target"] = value;
			}
		}
		
		[Bindable(true)]
		[DefaultValue("")]
		[PersistenceMode(PersistenceMode.InnerDefaultProperty)]
#if NET_2_0
		[Localizable (true)]
#endif		
		[WebSysDescription ("")]
		[WebCategory ("Appearance")]
		public virtual string Text {
			get {
				return ViewState.GetString ("Text", "");
			}
			set {
				ViewState ["Text"] = value;
				if (HasControls ())
					Controls.Clear ();
			}
		}
	}
}
