//
// System.Web.UI.WebControls.Label.cs
//
// Authors:
//	Miguel de Icaza (miguel@novell.com)
//
// (C) 2005-2010 Novell, Inc (http://www.novell.com)
//
// TODO: Are we missing something in LoadViewState?
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

using System.ComponentModel;
using System.Security.Permissions;

namespace System.Web.UI.WebControls
{
	// CAS
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	// attributes
	[ControlBuilder(typeof(LabelControlBuilder))]
	[DataBindingHandler("System.Web.UI.Design.TextDataBindingHandler, " + Consts.AssemblySystem_Design)]
	[DefaultProperty("Text")]
	[Designer("System.Web.UI.Design.WebControls.LabelDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	[ParseChildren (false)]
	[ToolboxData("<{0}:Label runat=\"server\" Text=\"Label\"></{0}:Label>")]
	[ControlValueProperty ("Text", null)]
	public class Label : WebControl, ITextControl
	{
		[Bindable(true)]
		[DefaultValue("")]
		[PersistenceMode(PersistenceMode.InnerDefaultProperty)]
		[Localizable (true)]
		[WebSysDescription ("")]
		[WebCategory ("Appearance")]
		public virtual string Text {
			get { return ViewState.GetString ("Text", String.Empty); }
			set {
				ViewState ["Text"] = value;
				if (HasControls ())
					Controls.Clear ();
			}
		}

		[IDReferenceProperty (typeof (Control))]
		[TypeConverter (typeof (AssociatedControlConverter))]
		[Themeable (false)]
		[DefaultValue("")]
		[WebSysDescription ("")]
		[WebCategory ("Accessibility")]
		public virtual string AssociatedControlID {
			get { return ViewState.GetString ("AssociatedControlID", String.Empty); }
			set { ViewState ["AssociatedControlID"] = value; }
		}
#if NET_4_0
		public override bool SupportsDisabledAttribute {
			get { return RenderingCompatibilityLessThan40; }
		}
#endif
		protected override void LoadViewState (object savedState)
		{
			base.LoadViewState (savedState);

			// Make sure we clear child controls when this happens
			if (ViewState ["Text"] != null)
				Text = (string) ViewState ["Text"];
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
		
		protected internal override void RenderContents (HtmlTextWriter writer)
		{
			if (HasControls () || HasRenderMethodDelegate ())
				base.RenderContents (writer);
			else
				writer.Write (Text);
		}

		protected override HtmlTextWriterTag TagKey {
			get { return String.IsNullOrEmpty (AssociatedControlID) ? HtmlTextWriterTag.Span : HtmlTextWriterTag.Label; }
		}

		protected override void AddAttributesToRender (HtmlTextWriter writer)
		{
			base.AddAttributesToRender (writer);
			if (!String.IsNullOrEmpty (AssociatedControlID))
				writer.AddAttribute (HtmlTextWriterAttribute.For, NamingContainer.FindControl (AssociatedControlID).ClientID);
		}
	}
}
