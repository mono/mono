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
	[ToolboxData ("<{0}:Panel runat=server>Panel</{0}:Panel>")]
	public class Panel : WebControl {

		public Panel () : base (HtmlTextWriterTag.Div) 
		{
		}
		
		protected override void AddAttributesToRender (HtmlTextWriter w)
		{
			base.AddAttributesToRender (w);
			
			string image = BackImageUrl;
			if (image != "") {
				image = String.Format ("url({0})", image);
				w.AddStyleAttribute (HtmlTextWriterStyle.BackgroundImage, image);
			}

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
				w.AddAttribute (HtmlTextWriterAttribute.Align, align);
		}
		
		[Bindable(true)]
		[DefaultValue("")]
		[Editor("System.Web.UI.Design.ImageUrlEditor, " + Consts.AssemblySystem_Design, typeof(System.Drawing.Design.UITypeEditor))]
		[WebSysDescription ("")]
		[WebCategory ("Appearance")]
		public virtual string BackImageUrl {
			get {
				return ViewState.GetString ("BackImageUrl", "");
			}
			
			set {
				ViewState ["BackImageUrl"] = value;
			}
		}
		
		[Bindable(true)]
		[DefaultValue(HorizontalAlign.NotSet)]
		[WebSysDescription ("")]
		[WebCategory ("Layout")]
		public virtual HorizontalAlign HorizontalAlign {
			get {
				return (HorizontalAlign) ViewState.GetInt ("HorizontalAlign", (int) HorizontalAlign.NotSet);
			}
			set {
				ViewState ["HorizontalAlign"] = (int) value;
			}
		}
		
		[Bindable(true)]
		[DefaultValue(true)]
		[WebSysDescription ("")]
		[WebCategory ("Layout")]
		public virtual bool Wrap {
			get {
				return ViewState.GetBool ("Wrap", true);
			}
			set {
				ViewState ["Wrap"] = value;
			}
		}
	}
}
