//
// System.Web.UI.WebControls.Button.cs
//
// Authors:
//	Jordi Mas i Hernandez (jordi@ximian.com)
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
using System.ComponentModel.Design;
using System.Security.Permissions;

namespace System.Web.UI.WebControls {

	// CAS
	[AspNetHostingPermissionAttribute (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermissionAttribute (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	// attributes
	[DefaultEvent ("Click")]
	[DataBindingHandler ("System.Web.UI.Design.TextDataBindingHandler, " + Consts.AssemblySystem_Design)]
	[DefaultProperty ("Text")]
	[Designer ("System.Web.UI.Design.WebControls.ButtonDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]

#if NET_2_0
	[ToolboxDataAttribute ("<{0}:Button runat=\"server\" Text=\"Button\"></{0}:Button>")]
	[SupportsEventValidation]
#else
	[ToolboxDataAttribute ("<{0}:Button runat=server Text=\"Button\"></{0}:Button>")]
#endif

	public class Button : WebControl, IPostBackEventHandler
#if NET_2_0
	, IButtonControl
#endif
	{
		static readonly object ClickEvent = new object ();
		static readonly object CommandEvent = new object ();

		public Button () : base (HtmlTextWriterTag.Input)
		{
		}

#if ONLY_1_1
		[Bindable (false)]
#endif		
		[WebSysDescription ("")]
		[WebCategory ("Behavior")]
		[DefaultValue (true)]
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

		[DefaultValue ("")]
		[Bindable (true)]
		[WebSysDescription ("")]
		[WebCategory ("Behavior")]
#if NET_2_0
		[Themeable (false)]
#endif		
		public string CommandArgument {
			get {
				return ViewState.GetString ("CommandArgument", "");
			}
			set {
				ViewState ["CommandArgument"] = value;
			}
		}

		[DefaultValue ("")]
		[WebSysDescription ("")]
		[WebCategory ("Behavior")]
#if NET_2_0
		[Themeable (false)]
#endif		
		public string CommandName {
			get {
				return ViewState.GetString ("CommandName", "");
			}
			set {
				ViewState ["CommandName"] = value;
			}
		}

#if NET_2_0
		[Themeable (false)]
		[DefaultValue ("")]
		[WebSysDescription ("")]
		[WebCategoryAttribute ("Behavior")]
		public virtual string OnClientClick 
		{
			get {
				return ViewState.GetString ("OnClientClick", "");
			}
			set {
				ViewState ["OnClientClick"] = value;
			}
		}

#endif		

		[DefaultValue ("")]
		[Bindable (true)]
		[WebSysDescription ("")]
		[WebCategory ("Appearance")]
#if NET_2_0
		[Localizable (true)]
#endif		
		public string Text {
			get {
				return ViewState.GetString ("Text", "");
			}
			set {
				ViewState ["Text"] = value;
			}
		}

#if NET_2_0
		[DefaultValue (true)]
		[Themeable (false)]
		[WebSysDescription ("")]
		[WebCategoryAttribute ("Behavior")]
		public virtual bool UseSubmitBehavior 
		{
			get {
				return ViewState.GetBool ("UseSubmitBehavior", true);
			}
			set {
				ViewState ["UseSubmitBehavior"] = value;
			}
		}
#endif		

		protected override void AddAttributesToRender (HtmlTextWriter writer) {
			if (Page != null)
				Page.VerifyRenderingInServerForm (this);
			
#if NET_2_0
			writer.AddAttribute (HtmlTextWriterAttribute.Type, UseSubmitBehavior ? "submit" : "button", false);
			writer.AddAttribute (HtmlTextWriterAttribute.Name, UniqueID);
			writer.AddAttribute (HtmlTextWriterAttribute.Value, Text);

			string onclick = OnClientClick;
			onclick = ClientScriptManager.EnsureEndsWithSemicolon (onclick);
			if (HasAttributes && Attributes ["onclick"] != null) {
				onclick = ClientScriptManager.EnsureEndsWithSemicolon (onclick + Attributes ["onclick"]);
				Attributes.Remove ("onclick");
			}

			if (Page != null) {
				onclick += GetClientScriptEventReference ();
			}

			if (onclick.Length > 0)
				writer.AddAttribute (HtmlTextWriterAttribute.Onclick, onclick);
#else
			writer.AddAttribute (HtmlTextWriterAttribute.Type, "submit");
			writer.AddAttribute (HtmlTextWriterAttribute.Name, UniqueID);
			writer.AddAttribute (HtmlTextWriterAttribute.Value, Text);

			if (CausesValidation && Page != null && Page.AreValidatorsUplevel ()) {
				string onclick = Attributes["onclick"];
				if (onclick != null) {
					Attributes.Remove("onclick");
					int len = onclick.Length;
					if (len > 0 && onclick[len - 1] != ';')
						onclick += ";";
				}
				ClientScriptManager csm = new ClientScriptManager (Page);
				writer.AddAttribute (HtmlTextWriterAttribute.Onclick, onclick + csm.GetClientValidationEvent ());
				writer.AddAttribute ("language", "javascript");
			}
#endif

			base.AddAttributesToRender (writer);
		}

#if NET_2_0
		internal virtual string GetClientScriptEventReference ()
		{
			PostBackOptions options = GetPostBackOptions ();
			return Page.ClientScript.GetPostBackEventReference (options, true);
		}

		protected virtual PostBackOptions GetPostBackOptions () 
		{
			PostBackOptions options = new PostBackOptions (this);
			options.ActionUrl = (PostBackUrl.Length > 0 ? 
#if TARGET_J2EE
				CreateActionUrl(PostBackUrl)
#else
				Page.ResolveClientUrl (PostBackUrl) 
#endif
				: null);
			options.ValidationGroup = null;
			options.Argument = String.Empty;
			options.RequiresJavaScriptProtocol = false;
			options.ClientSubmit = !UseSubmitBehavior;
			options.PerformValidation = CausesValidation && Page != null && Page.AreValidatorsUplevel (ValidationGroup);
			if (options.PerformValidation)
				options.ValidationGroup = ValidationGroup;

			return options;
		}
#endif		

		void IPostBackEventHandler.RaisePostBackEvent (string eventArgument)
		{
			RaisePostBackEvent (eventArgument);
		}

		protected virtual void OnClick (EventArgs e)
		{
			if (Events != null) {
				EventHandler eh = (EventHandler) (Events [ClickEvent]);
				if (eh != null)
					eh (this, e);
			}
		}

		protected virtual void OnCommand (CommandEventArgs e)
		{
			if (Events != null) {
				CommandEventHandler eh = (CommandEventHandler) (Events [CommandEvent]);
				if (eh != null)
					eh (this, e);
			}

			RaiseBubbleEvent (this, e);
		}

#if NET_2_0
		protected virtual
#endif
		void RaisePostBackEvent (string eventArgument)
		{
#if NET_2_0
			ValidateEvent (UniqueID, eventArgument);
#endif
			if (CausesValidation)
#if NET_2_0
				Page.Validate (ValidationGroup);
#else
				Page.Validate ();
#endif

			OnClick (EventArgs.Empty);
			OnCommand (new CommandEventArgs (CommandName, CommandArgument));
		}

#if NET_2_0
		protected internal override void OnPreRender (EventArgs e)
		{
			// Why override?
			base.OnPreRender (e);
		}
#endif
		
#if NET_2_0
		protected internal
#else		
		protected
#endif
		override void RenderContents (HtmlTextWriter writer)
                {
                }
		
		[WebSysDescription ("")]
		[WebCategory ("Action")]
		public event EventHandler Click
		{
			add {
				Events.AddHandler (ClickEvent, value);
			}
			remove {
				Events.RemoveHandler (ClickEvent, value);
			}
		}

		[WebSysDescription ("")]
		[WebCategory ("Action")]
		public event CommandEventHandler Command
		{
			add {
				Events.AddHandler (CommandEvent, value);
			}
			remove {
				Events.RemoveHandler (CommandEvent, value);
			}
		}

#if NET_2_0
		[DefaultValue ("")]
		[Editor ("System.Web.UI.Design.UrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		[Themeable (false)]
		[UrlProperty("*.aspx")]
		public virtual string PostBackUrl {
			get {
				return ViewState.GetString ("PostBackUrl", "");
			}
			set {
				ViewState ["PostBackUrl"] = value;
			}
		}

		[DefaultValue ("")]
		[Themeable (false)]
		[WebSysDescription ("")]
		[WebCategoryAttribute ("Behavior")]
		public virtual string ValidationGroup {
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


