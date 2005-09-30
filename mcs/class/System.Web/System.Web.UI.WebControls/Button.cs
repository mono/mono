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

		private static readonly object ClickEvent = new object ();
		private static readonly object CommandEvent = new object ();

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

		[DefaultValue ("")]
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
		[Themeable (false)]
		[DefaultValue ("")]
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

		[DefaultValue ("")]
		[Bindable (true)]
		[WebSysDescription ("")]
		[WebCategory ("Appearance")]
#if NET_2_0
		[Localizable (true)]
		public virtual
#else		
		public
#endif		
		string Text {
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
		[MonoTODO]
		[WebSysDescription ("")]
		[WebCategoryAttribute ("Behavior")]
		public virtual bool UseSubmitBehavior 
		{
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
#endif		

		protected override void AddAttributesToRender (HtmlTextWriter writer)
		{
			if (Page != null)
				Page.VerifyRenderingInServerForm (this);

			writer.AddAttribute (HtmlTextWriterAttribute.Type, "submit");
#if NET_2_0
			if (ID != null)
				writer.AddAttribute (HtmlTextWriterAttribute.Name, UniqueID);
#else
			writer.AddAttribute (HtmlTextWriterAttribute.Name, UniqueID);
#endif
			writer.AddAttribute (HtmlTextWriterAttribute.Value, Text);

			if (CausesValidation && Page != null && Page.AreValidatorsUplevel ()) {
				ClientScriptManager csm = new ClientScriptManager (Page);
				writer.AddAttribute (HtmlTextWriterAttribute.Onclick, csm.GetClientValidationEvent ());
				writer.AddAttribute ("language", "javascript");
			}

			base.AddAttributesToRender (writer);
		}

#if NET_2_0
		[MonoTODO]
		protected virtual PostBackOptions GetPostBackOptions ()
		{
			throw new NotImplementedException ();
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

