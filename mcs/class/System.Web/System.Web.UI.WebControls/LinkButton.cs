//
// System.Web.UI.WebControls.LinkButton.cs
//
// Authors:
//	Ben Maurer (bmaurer@novell.com)
//
// (C) 2005 Novell, Inc (http://www.novell.com)
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
	[ControlBuilder(typeof(LinkButtonControlBuilder))]
	[DataBindingHandler("System.Web.UI.Design.TextDataBindingHandler, " + Consts.AssemblySystem_Design)]
	[DefaultEvent("Click")]
	[DefaultProperty("Text")]
	[Designer("System.Web.UI.Design.WebControls.LinkButtonDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	[ParseChildren(false)]
#if NET_2_0
	[SupportsEventValidation]
	[ToolboxData("<{0}:LinkButton runat=\"server\">LinkButton</{0}:LinkButton>")]
#else		
	[ToolboxData("<{0}:LinkButton runat=server>LinkButton</{0}:LinkButton>")]
#endif		
	public class LinkButton : WebControl, IPostBackEventHandler
#if NET_2_0
	, IButtonControl
#endif
	{
	
		public LinkButton () : base (HtmlTextWriterTag.A) 
		{
		}
	
	
		protected override void AddAttributesToRender (HtmlTextWriter w)
		{
			if (Page != null)
				Page.VerifyRenderingInServerForm (this);

			base.AddAttributesToRender (w);
			if (Page == null || !Enabled)
				return;
			
			if (CausesValidation && Page.AreValidatorsUplevel ()) {
				ClientScriptManager csm = new ClientScriptManager (Page);
				w.AddAttribute (HtmlTextWriterAttribute.Href,
						String.Format ("javascript:{{if (typeof(Page_ClientValidate) != 'function' ||  Page_ClientValidate()) {0};}}",
							       csm.GetPostBackEventReference (this, String.Empty)));
				w.AddAttribute ("language", "javascript");
			} else {
				w.AddAttribute (HtmlTextWriterAttribute.Href, Page.ClientScript.GetPostBackClientHyperlink (this, ""));
			}
		}

#if NET_2_0
		[MonoTODO]
		protected virtual void RaisePostBackEvent (string eventArgument)
		{
			throw new NotImplementedException ();
		}
#endif		
		
		void IPostBackEventHandler.RaisePostBackEvent (string ea)
		{
			if (CausesValidation)
#if NET_2_0
				Page.Validate (ValidationGroup);
#else
				Page.Validate ();
#endif
			
			OnClick (EventArgs.Empty);
			OnCommand (new CommandEventArgs (CommandName, CommandArgument));
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

#if NET_2_0
		[MonoTODO]
		protected virtual PostBackOptions GetPostBackOptions ()
		{
			throw new NotImplementedException ();
		}
#endif		

		protected override void LoadViewState (object savedState)
		{
			base.LoadViewState (savedState);

			// Make sure we clear child controls when this happens
			if (ViewState ["Text"] != null)
				Text = (string) ViewState ["Text"];
		}

		[MonoTODO ("Why override?")]
#if NET_2_0
		protected internal
#else		
		protected
#endif		
		override void OnPreRender (EventArgs e)
		{
			base.OnPreRender (e);
		}
	
#if NET_2_0
		protected internal
#else		
		protected
#endif		
		override void RenderContents (HtmlTextWriter writer)
		{
			if (HasControls ())
				base.RenderContents (writer);
			else
				writer.Write (Text);
		}
	
	
#if ONLY_1_1
		[Bindable(false)]
#endif		
		[DefaultValue(true)]
		[WebSysDescription ("")]
		[WebCategory ("Behavior")]
#if NET_2_0
		[Themeable (false)]
		public virtual
#else
		public
#endif		
		bool CausesValidation {
			get {
				return ViewState.GetBool ("CausesValidation", true);
			}
			set {
				ViewState ["CausesValidation"] = value;
			}
		
		}

		[Bindable(true)]
		[DefaultValue("")]
		[WebSysDescription ("")]
		[WebCategory ("Behavior")]
#if NET_2_0
		[Themeable (false)]
		public virtual
#else		
		public
#endif		
		string CommandArgument {
			get {
				return ViewState.GetString ("CommandArgument", "");
			}
			set {
				ViewState ["CommandArgument"] = value;
			}
		}

		[DefaultValue("")]
		[WebSysDescription ("")]
		[WebCategory ("Behavior")]
#if NET_2_0
		[Themeable (false)]
		public virtual
#else		
		public
#endif		
		string CommandName {
			get {
				return ViewState.GetString ("CommandName", "");	
			}
		
			set {
				ViewState ["CommandName"] = value;
			}
		
		}

#if NET_2_0
		[DefaultValue ("")]
		[Themeable (false)]
		[MonoTODO]
		[WebSysDescription ("")]
		[WebCategoryAttribute ("Behavior")]
		public virtual string OnClientClick
		{
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

#endif		

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
#if NET_2_0
		[Editor ("System.Web.UI.Design.UrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		[Themeable (false)]
		[UrlProperty ("*.aspx")]
		[DefaultValue ("")]
		[MonoTODO]
		public string PostBackUrl {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[DefaultValue ("")]
		[Themeable (false)]
		[WebSysDescription ("")]
		[WebCategoryAttribute ("Behavior")]
		public string ValidationGroup {
			get {
				return ViewState.GetString ("ValidationGroup", "");	
			}
			set {
				ViewState ["ValidationGroup"] = value;	
			}
		}	
#endif
	}
}

