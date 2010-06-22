//
// System.Web.UI.WebControls.LinkButton.cs
//
// Authors:
//	Ben Maurer (bmaurer@novell.com)
//
// (C) 2005-2010 Novell, Inc (http://www.novell.com)
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
	[AspNetHostingPermissionAttribute (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermissionAttribute (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	// attributes
	[ControlBuilder(typeof(LinkButtonControlBuilder))]
	[DataBindingHandler("System.Web.UI.Design.TextDataBindingHandler, " + Consts.AssemblySystem_Design)]
	[DefaultEvent("Click")]
	[DefaultProperty("Text")]
	[Designer("System.Web.UI.Design.WebControls.LinkButtonDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	[ParseChildren(false)]
	[SupportsEventValidation]
	[ToolboxData("<{0}:LinkButton runat=\"server\">LinkButton</{0}:LinkButton>")]
	public class LinkButton : WebControl, IPostBackEventHandler, IButtonControl
	{
		public LinkButton () : base (HtmlTextWriterTag.A) 
		{
		}
	
		protected override void AddAttributesToRender (HtmlTextWriter w)
		{
			Page page = Page;
			if (page != null)
				page.VerifyRenderingInServerForm (this);

			bool enabled = IsEnabled;
			string onclick = OnClientClick;
			onclick = ClientScriptManager.EnsureEndsWithSemicolon (onclick);
			if (HasAttributes && Attributes ["onclick"] != null) {
				onclick = ClientScriptManager.EnsureEndsWithSemicolon (onclick + Attributes ["onclick"]);
				Attributes.Remove ("onclick");
			}

			if (onclick.Length > 0)
				w.AddAttribute (HtmlTextWriterAttribute.Onclick, onclick);
			
			if (enabled && page != null) {
				PostBackOptions options = GetPostBackOptions ();
				string href = page.ClientScript.GetPostBackEventReference (options, true);
				w.AddAttribute (HtmlTextWriterAttribute.Href, href);
			}
			base.AddAttributesToRender (w);
			AddDisplayStyleAttribute (w);
		}

		protected virtual void RaisePostBackEvent (string eventArgument)
		{
			ValidateEvent (UniqueID, eventArgument);
			if (CausesValidation) {
				Page page = Page;
				if (page != null)
					page.Validate (ValidationGroup);
			}
			
			OnClick (EventArgs.Empty);
			OnCommand (new CommandEventArgs (CommandName, CommandArgument));
		}
		
		void IPostBackEventHandler.RaisePostBackEvent (string ea)
		{
			RaisePostBackEvent (ea);
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
			} else
				Text = lc.Text;
		}

		protected virtual PostBackOptions GetPostBackOptions ()
		{
			PostBackOptions options = new PostBackOptions (this);
			Page page = Page;
			
			options.ActionUrl = (PostBackUrl.Length > 0 ?
#if TARGET_J2EE
					     CreateActionUrl (PostBackUrl)
#else
					     page != null ? page.ResolveClientUrl (PostBackUrl) : PostBackUrl
#endif
					     : null);
			options.ValidationGroup = null;
			options.Argument = String.Empty;
			options.ClientSubmit = true;
			options.RequiresJavaScriptProtocol = true;
			options.PerformValidation = CausesValidation && page != null && page.AreValidatorsUplevel (ValidationGroup);
			if (options.PerformValidation)
				options.ValidationGroup = ValidationGroup;

			return options;
		}

		protected override void LoadViewState (object savedState)
		{
			base.LoadViewState (savedState);

			// Make sure we clear child controls when this happens
			if (ViewState ["Text"] != null)
				Text = (string) ViewState ["Text"];
		}

		[MonoTODO ("Why override?")]
		protected internal override void OnPreRender (EventArgs e)
		{
			base.OnPreRender (e);
		}
	
		protected internal override void RenderContents (HtmlTextWriter writer)
		{
			if (HasControls () || HasRenderMethodDelegate ())
				base.RenderContents (writer);
			else
				writer.Write (Text);
		}

		[DefaultValue(true)]
		[WebSysDescription ("")]
		[WebCategory ("Behavior")]
		[Themeable (false)]
		public virtual bool CausesValidation {
			get { return ViewState.GetBool ("CausesValidation", true); }
			set { ViewState ["CausesValidation"] = value; }
		
		}

		[Bindable(true)]
		[DefaultValue("")]
		[WebSysDescription ("")]
		[WebCategory ("Behavior")]
		[Themeable (false)]
		public string CommandArgument {
			get { return ViewState.GetString ("CommandArgument", String.Empty); }
			set { ViewState ["CommandArgument"] = value; }
		}

		[DefaultValue("")]
		[WebSysDescription ("")]
		[WebCategory ("Behavior")]
		[Themeable (false)]
		public string CommandName {
			get { return ViewState.GetString ("CommandName", String.Empty); }
			set { ViewState ["CommandName"] = value; }
		
		}

		[DefaultValue ("")]
		[Themeable (false)]
		[WebSysDescription ("")]
		[WebCategoryAttribute ("Behavior")]
		public virtual string OnClientClick
		{
			get { return ViewState.GetString ("OnClientClick", String.Empty); }
			set { ViewState ["OnClientClick"] = value; }
		}

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

		protected virtual void OnClick (EventArgs e)
		{
			EventHandler h = (EventHandler) Events [ClickEvent];
			if (h != null)
				h (this, e);
		}
		static readonly object ClickEvent = new object ();

		[WebSysDescription ("")]
		[WebCategory ("Action")]
		public event EventHandler Click {
			add { Events.AddHandler (ClickEvent, value); }
			remove { Events.RemoveHandler (ClickEvent, value); }
		}

		protected virtual void OnCommand (CommandEventArgs e)
		{
			CommandEventHandler h = (CommandEventHandler) Events [CommandEvent];
			if (h != null)
				h (this, e);

			RaiseBubbleEvent (this, e);
		}
		static readonly object CommandEvent = new object ();

		[WebSysDescription ("")]
		[WebCategory ("Action")]
		public event CommandEventHandler Command {
			add { Events.AddHandler (CommandEvent, value); }
			remove { Events.RemoveHandler (CommandEvent, value); }
		}

		[Editor ("System.Web.UI.Design.UrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		[Themeable (false)]
		[UrlProperty ("*.aspx")]
		[DefaultValue ("")]
		public virtual string PostBackUrl {
			get { return ViewState.GetString ("PostBackUrl", String.Empty); }
			set { ViewState["PostBackUrl"] = value; }
		}

		[DefaultValue ("")]
		[Themeable (false)]
		[WebSysDescription ("")]
		[WebCategoryAttribute ("Behavior")]
		public virtual string ValidationGroup {
			get { return ViewState.GetString ("ValidationGroup", String.Empty); }
			set { ViewState ["ValidationGroup"] = value; }
		}
	}
}


