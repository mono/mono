//
// System.Web.UI.WebControls.Button.cs
//
// Authors:
//   Gaurav Vaish (gvaish@iitk.ac.in)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) Gaurav Vaish (2002)
// (C) 2003 Andreas Nahr
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

using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	[DefaultEvent("Click")]
	[DefaultProperty("Text")]
	[Designer("System.Web.UI.Design.WebControls.ButtonDesigner, " + Consts.AssemblySystem_Design, typeof (IDesigner))]
	[DataBindingHandler("System.Web.UI.Design.TextDataBindingHandler, " + Consts.AssemblySystem_Design)]
	[ToolboxData("<{0}:Button runat=\"server\" Text=\"Button\"></{0}:Button>")]
	public class Button : WebControl, IPostBackEventHandler
#if NET_2_0
		, IButtonControl
#endif
	{
		private static readonly object ClickEvent   = new object();
		private static readonly object CommandEvent = new object();

		//private EventHandlerList ehList;

		public Button(): base (HtmlTextWriterTag.Input)
		{
		}

#if NET_2_0
    	[ThemeableAttribute (false)]
#else
		[Bindable (false)]
#endif
		[DefaultValue (true), WebCategory ("Behavior")]
		[WebSysDescription ("Determines if validation is performed when clicked.")]
		public bool CausesValidation
		{
			get
			{
				Object cv = ViewState["CausesValidation"];
				if(cv!=null)
					return (Boolean)cv;
				return true;
			}
			set
			{
				ViewState["CausesValidation"] = value;
			}
		}

#if NET_2_0
    	[ThemeableAttribute (false)]
#endif
		[DefaultValue (""), Bindable (true), WebCategory ("Behavior")]
		[WebSysDescription ("An argument for the Command of this control.")]
		public string CommandArgument
		{
			get
			{
				string ca = (string) ViewState["CommandArgument"];
				if(ca!=null)
					return ca;
				return String.Empty;
			}
			set
			{
				ViewState["CommandArgument"] = value;
			}
		}

#if NET_2_0
    	[ThemeableAttribute (false)]
#endif
		[DefaultValue (""), WebCategory ("Behavior")]
		[WebSysDescription ("The name of the Command of this control.")]
		public string CommandName
		{
			get
			{
				string cn = (string)ViewState["CommandName"];
				if(cn!=null)
					return cn;
				return String.Empty;
			}
			set
			{
				ViewState["CommandName"] = value;
			}
		}

#if NET_2_0
    	[Localizable (true)]
#endif
		[DefaultValue (""), Bindable (true), WebCategory ("Appearance")]
		[WebSysDescription ("The text that should be shown on this Button.")]
		public string Text
		{
			get
			{
				string text = (string)ViewState["Text"];
				if(text!=null)
					return text;
				return String.Empty;
			}
			set
			{
				ViewState["Text"] = value;
			}
		}
		
#if NET_2_0
		[ThemeableAttribute (false)]
		[EditorAttribute ("System.Web.UI.Design.UrlEditor, System.Design, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		[DefaultValueAttribute ("")]
		[UrlProperty]
		public string PostBackUrl {
			get {
				string text = (string)ViewState["PostBackUrl"];
				if (text!=null) return text;
				return String.Empty;
			}
			set {
				ViewState["PostBackUrl"] = value;
			}
		}
		
		[BindableAttribute (true)]
		[LocalizableAttribute (true)]
		[DefaultValueAttribute ("")]
		public string SoftkeyLabel {
			get {
				string text = (string)ViewState["SoftkeyLabel"];
				if (text!=null) return text;
				return String.Empty;
			}
			set {
				ViewState["SoftkeyLabel"] = value;
			}
		}
		
		[DefaultValueAttribute ("")]
		[ThemeableAttribute (false)]
		[WebCategoryAttribute ("Behavior")]
		public string ValidationGroup {
			get {
				string text = (string)ViewState["ValidationGroup"];
				if (text!=null) return text;
				return String.Empty;
			}
			set {
				ViewState["ValidationGroup"] = value;
			}
		}
		
		[DefaultValueAttribute ("")]
		[ThemeableAttribute (false)]
		[WebCategoryAttribute ("Behavior")]
		public string OnClientClick {
			get {
				string text = (string)ViewState["OnClientClick"];
				if (text!=null) return text;
				return String.Empty;
			}
			set {
				ViewState["OnClientClick"] = value;
			}
		}
		
		[DefaultValueAttribute (true)]
		[ThemeableAttribute (false)]
		[WebCategoryAttribute ("Behavior")]
		public bool UseSubmitBehavior {
			get {
				object b = ViewState["UseSubmitBehavior"];
				if (b != null) return (bool)b;
				return true;
			}
			set {
				ViewState["UseSubmitBehavior"] = value;
			}
		}
#endif

		[WebCategory ("Action")]
		[WebSysDescription ("Raised when the Button is clicked.")]
		public event EventHandler Click
		{
			add
			{
				Events.AddHandler(ClickEvent, value);
			}
			remove
			{
				Events.RemoveHandler(ClickEvent, value);
			}
		}

		[WebCategory ("Action")]
		[WebSysDescription ("Raised when a Button Command is executed.")]
		public event CommandEventHandler Command
		{
			add
			{
				Events.AddHandler(CommandEvent, value);
			}
			remove
			{
				Events.RemoveHandler(CommandEvent, value);
			}
		}

		protected virtual void OnClick(EventArgs e)
		{
			if(Events != null)
			{
				EventHandler eh = (EventHandler)(Events[ClickEvent]);
				if(eh!= null)
					eh(this,e);
			}
		}

		protected virtual void OnCommand(CommandEventArgs e)
		{
			if(Events != null)
			{
				CommandEventHandler eh = (CommandEventHandler)(Events[CommandEvent]);
				if(eh!= null)
					eh(this,e);
			}
			RaiseBubbleEvent(this, e);
		}

#if NET_2_0
		void IPostBackEventHandler.RaisePostBackEvent (string eventArgument)
		{
			RaisePostBackEvent (eventArgument);
		}
		
		protected virtual void RaisePostBackEvent (string eventArgument)
		{
			if (CausesValidation)
				Page.Validate (ValidationGroup);

			OnClick (EventArgs.Empty);
			OnCommand (new CommandEventArgs (CommandName, CommandArgument));
		}
#else
		void IPostBackEventHandler.RaisePostBackEvent (string eventArgument)
		{
			if (CausesValidation)
				Page.Validate ();

			OnClick (EventArgs.Empty);
			OnCommand (new CommandEventArgs (CommandName, CommandArgument));
		}
#endif
		
#if NET_2_0
		protected virtual PostBackOptions GetPostBackOptions ()
		{
			PostBackOptions ops = new PostBackOptions (this);
			if (PostBackUrl != "")
				ops.ActionUrl = PostBackUrl;
			ops.PerformValidation = Page.Validators.Count > 0 && CausesValidation;
			if (ops.PerformValidation)
				ops.ValidationGroup = ValidationGroup;
			ops.ClientSubmit = UseSubmitBehavior;
			ops.RequiresJavaScriptProtocol = false;
			return ops;
		}
#endif

		protected override void AddAttributesToRender (HtmlTextWriter writer)
		{
			if (Page != null)
				Page.VerifyRenderingInServerForm (this);

			writer.AddAttribute (HtmlTextWriterAttribute.Type, "submit");
			writer.AddAttribute (HtmlTextWriterAttribute.Name, base.UniqueID);
			writer.AddAttribute (HtmlTextWriterAttribute.Value, Text);

			if (Page != null) {
				string script = "";
#if NET_2_0
				script = OnClientClick;
				if (script.Length > 0) script += ";";
				
				PostBackOptions ops = GetPostBackOptions ();
				if (ops != null && ops.RequiresSpecialPostBack) {
					script += Page.GetPostBackEventReference (ops);
				}
				else if (CausesValidation && Page.Validators.Count > 0) {
					if (UseSubmitBehavior)
						script += Utils.GetClientValidatedEvent (Page);
					else
						script += Utils.GetClientValidatedPostBack (this);
				}
				else if (!UseSubmitBehavior) {
					script += Page.GetPostBackClientEvent (this,"");
				}
#else
				if (CausesValidation && Page.Validators.Count > 0) {
					script += Utils.GetClientValidatedEvent (Page);
				}
#endif
				
				if (script != "") {
					writer.AddAttribute (System.Web.UI.HtmlTextWriterAttribute.Onclick, script);
					writer.AddAttribute ("language", "javascript");
				}
			}
			
			base.AddAttributesToRender (writer);
		}

		protected override void RenderContents(HtmlTextWriter writer)
		{
			// Preventing base classes to do anything
		}
	}
}
