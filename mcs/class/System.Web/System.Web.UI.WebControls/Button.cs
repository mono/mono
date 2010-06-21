//
// System.Web.UI.WebControls.Button.cs
//
// Authors:
//	Jordi Mas i Hernandez (jordi@ximian.com)
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
using System.ComponentModel.Design;
using System.Security.Permissions;

namespace System.Web.UI.WebControls
{
	// CAS
	[AspNetHostingPermissionAttribute (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermissionAttribute (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	// attributes
	[DefaultEvent ("Click")]
	[DataBindingHandler ("System.Web.UI.Design.TextDataBindingHandler, " + Consts.AssemblySystem_Design)]
	[DefaultProperty ("Text")]
	[Designer ("System.Web.UI.Design.WebControls.ButtonDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	[ToolboxDataAttribute ("<{0}:Button runat=\"server\" Text=\"Button\"></{0}:Button>")]
	[SupportsEventValidation]
	public class Button : WebControl, IPostBackEventHandler, IButtonControl
	{
		static readonly object ClickEvent = new object ();
		static readonly object CommandEvent = new object ();

		public Button () : base (HtmlTextWriterTag.Input)
		{
		}

		[WebSysDescription ("")]
		[WebCategory ("Behavior")]
		[DefaultValue (true)]
		[Themeable (false)]
		public virtual bool CausesValidation {
			get { return ViewState.GetBool ("CausesValidation", true); }
			set { ViewState ["CausesValidation"] = value; }
		}

		[DefaultValue ("")]
		[Bindable (true)]
		[WebSysDescription ("")]
		[WebCategory ("Behavior")]
		[Themeable (false)]
		public string CommandArgument {
			get { return ViewState.GetString ("CommandArgument", String.Empty); }
			set { ViewState ["CommandArgument"] = value; }
		}

		[DefaultValue ("")]
		[WebSysDescription ("")]
		[WebCategory ("Behavior")]
		[Themeable (false)]
		public string CommandName {
			get { return ViewState.GetString ("CommandName", String.Empty); }
			set { ViewState ["CommandName"] = value; }
		}

		[Themeable (false)]
		[DefaultValue ("")]
		[WebSysDescription ("")]
		[WebCategoryAttribute ("Behavior")]
		public virtual string OnClientClick {
			get { return ViewState.GetString ("OnClientClick", String.Empty); }
			set { ViewState ["OnClientClick"] = value; }
		}

		[DefaultValue ("")]
		[Bindable (true)]
		[WebSysDescription ("")]
		[WebCategory ("Appearance")]
		[Localizable (true)]
		public string Text {
			get { return ViewState.GetString ("Text", String.Empty); }
			set { ViewState ["Text"] = value; }
		}

		[DefaultValue (true)]
		[Themeable (false)]
		[WebSysDescription ("")]
		[WebCategoryAttribute ("Behavior")]
		public virtual bool UseSubmitBehavior  {
			get { return ViewState.GetBool ("UseSubmitBehavior", true); }
			set { ViewState ["UseSubmitBehavior"] = value; }
		}

		protected override void AddAttributesToRender (HtmlTextWriter writer)
		{
			Page page = Page;
			if (page != null)
				page.VerifyRenderingInServerForm (this);
			
			writer.AddAttribute (HtmlTextWriterAttribute.Type, UseSubmitBehavior ? "submit" : "button", false);
			writer.AddAttribute (HtmlTextWriterAttribute.Name, UniqueID);
			writer.AddAttribute (HtmlTextWriterAttribute.Value, Text);

			string onclick = OnClientClick;
			onclick = ClientScriptManager.EnsureEndsWithSemicolon (onclick);
			if (HasAttributes && Attributes ["onclick"] != null) {
				onclick = ClientScriptManager.EnsureEndsWithSemicolon (onclick + Attributes ["onclick"]);
				Attributes.Remove ("onclick");
			}

			if (page != null)
				onclick += GetClientScriptEventReference ();

			if (onclick.Length > 0)
				writer.AddAttribute (HtmlTextWriterAttribute.Onclick, onclick);

			base.AddAttributesToRender (writer);
		}

		internal virtual string GetClientScriptEventReference ()
		{
			PostBackOptions options = GetPostBackOptions ();
			Page page = Page;
			if (page != null)
				return page.ClientScript.GetPostBackEventReference (options, true);
			else
				return String.Empty;
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

			Page page = Page;
			options.PerformValidation = CausesValidation && page != null && page.AreValidatorsUplevel (ValidationGroup);
			if (options.PerformValidation)
				options.ValidationGroup = ValidationGroup;

			return options;
		}

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

		protected internal override void OnPreRender (EventArgs e)
		{
			// Why override?
			base.OnPreRender (e);
		}

		protected internal override void RenderContents (HtmlTextWriter writer)
                {
                }
		
		[WebSysDescription ("")]
		[WebCategory ("Action")]
		public event EventHandler Click {
			add { Events.AddHandler (ClickEvent, value); }
			remove { Events.RemoveHandler (ClickEvent, value); }
		}

		[WebSysDescription ("")]
		[WebCategory ("Action")]
		public event CommandEventHandler Command {
			add { Events.AddHandler (CommandEvent, value); }
			remove { Events.RemoveHandler (CommandEvent, value); }
		}

		[DefaultValue ("")]
		[Editor ("System.Web.UI.Design.UrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		[Themeable (false)]
		[UrlProperty("*.aspx")]
		public virtual string PostBackUrl {
			get { return ViewState.GetString ("PostBackUrl", String.Empty); }
			set { ViewState ["PostBackUrl"] = value; }
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
