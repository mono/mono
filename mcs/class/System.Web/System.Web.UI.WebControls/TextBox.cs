//
// System.Web.UI.WebControls.TextBox.cs
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

using System.Collections.Specialized;
using System.ComponentModel;
using System.Security.Permissions;
using System.Text;

namespace System.Web.UI.WebControls {

	// CAS
	[AspNetHostingPermissionAttribute (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermissionAttribute (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	// attributes
	[DataBindingHandler ("System.Web.UI.Design.TextDataBindingHandler, " + Consts.AssemblySystem_Design)]
	[DefaultEvent ("TextChanged")]
	[DefaultProperty ("Text")]
	[ValidationProperty ("Text")]
	[ControlBuilder (typeof (TextBoxControlBuilder))]
#if NET_2_0
	[Designer ("System.Web.UI.Design.WebControls.PreviewControlDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	[ParseChildren (true, "Text")]
	[ControlValueProperty ("Text", null)]
	[SupportsEventValidation]
#else
	[ParseChildren (false)]
#endif		
	public class TextBox : WebControl, IPostBackDataHandler
#if NET_2_0
	, IEditableTextControl, ITextControl
#endif		  
	{
#if NET_2_0
		readonly static string [] VCardValues = new string [] {
			null,
			null,
			"vCard.Cellular",
			"vCard.Company",
			"vCard.Department",
			"vCard.DisplayName",
			"vCard.Email",
			"vCard.FirstName",
			"vCard.Gender",
			"vCard.Home.City",
			"HomeCountry",
			"vCard.Home.Fax",
			"vCard.Home.Phone",
			"vCard.Home.State",
			"vCard.Home.StreetAddress",
			"vCard.Home.ZipCode",
			"vCard.Home.page",
			"vCard.JobTitle",
			"vCard.LastName",
			"vCard.MiddleName",
			"vCard.Notes",
			"vCard.Office",
			"vCard.Pager",
			"vCard.Business.City",
			"BusinessCountry",
			"vCard.Business.Fax",
			"vCard.Business.Phone",
			"vCard.Business.State",
			"vCard.Business.StreetAddress",
			"vCard.Business.Url",
			"vCard.Business.ZipCode",
			"search"
		};
#endif

		protected override void AddAttributesToRender (HtmlTextWriter w)
		{
			Page page = Page;
			if (page != null)
				page.VerifyRenderingInServerForm (this);
			
			switch (TextMode) {
			case TextBoxMode.MultiLine:
				if (Columns != 0)
					w.AddAttribute (HtmlTextWriterAttribute.Cols, Columns.ToString (), false);
#if NET_2_0
				else
					w.AddAttribute (HtmlTextWriterAttribute.Cols, "20", false);
#endif
				
				if (Rows != 0)
					w.AddAttribute (HtmlTextWriterAttribute.Rows, Rows.ToString (), false);
#if NET_2_0
				else
					w.AddAttribute (HtmlTextWriterAttribute.Rows, "2", false);
#endif

				if (!Wrap)
					w.AddAttribute (HtmlTextWriterAttribute.Wrap, "off", false);
				
				break;
				
			case TextBoxMode.SingleLine:
			case TextBoxMode.Password:
				
				if (TextMode == TextBoxMode.Password)
					w.AddAttribute (HtmlTextWriterAttribute.Type, "password", false);
				else {
					w.AddAttribute (HtmlTextWriterAttribute.Type, "text", false);
					if (Text.Length > 0)
						w.AddAttribute (HtmlTextWriterAttribute.Value, Text);
				}
				
				if (Columns != 0)
					w.AddAttribute (HtmlTextWriterAttribute.Size, Columns.ToString (), false);
		
				if (MaxLength != 0)
					w.AddAttribute (HtmlTextWriterAttribute.Maxlength, MaxLength.ToString (), false);

#if NET_2_0
				if (AutoCompleteType != AutoCompleteType.None && TextMode == TextBoxMode.SingleLine)
					if (AutoCompleteType != AutoCompleteType.Disabled)
						w.AddAttribute (HtmlTextWriterAttribute.VCardName, VCardValues [(int) AutoCompleteType]);
					else
						w.AddAttribute (HtmlTextWriterAttribute.AutoComplete, "off", false);
#endif
				break;	
			}

#if NET_2_0
			if (AutoPostBack) {
				w.AddAttribute ("onkeypress", "if (WebForm_TextBoxKeyHandler(event) == false) return false;", false);

				if (page != null) {
					string onchange = page.ClientScript.GetPostBackEventReference (GetPostBackOptions (), true);
					onchange = String.Concat ("setTimeout('", onchange.Replace ("\\", "\\\\").Replace ("'", "\\'"), "', 0)");
					w.AddAttribute (HtmlTextWriterAttribute.Onchange, BuildScriptAttribute ("onchange", onchange));
				}
			} else if (page != null)
				page.ClientScript.RegisterForEventValidation (UniqueID, String.Empty);
				
#else		
			if (page != null && AutoPostBack)
				w.AddAttribute (HtmlTextWriterAttribute.Onchange,
						BuildScriptAttribute ("onchange",
							page.ClientScript.GetPostBackClientHyperlink (this, "")));
#endif

			if (ReadOnly)
				w.AddAttribute (HtmlTextWriterAttribute.ReadOnly, "ReadOnly", false);

			w.AddAttribute (HtmlTextWriterAttribute.Name, UniqueID);
			
			base.AddAttributesToRender (w);
		}

		protected override void AddParsedSubObject (object obj)
		{
			LiteralControl l = obj as LiteralControl;
			if (l != null)
				Text = l.Text;
		}

#if NET_2_0
		protected internal
#else		
		protected
#endif		
		override void OnPreRender (EventArgs e)
		{
			// What do i do here?
			base.OnPreRender (e);
#if NET_2_0
			if (AutoPostBack) {
				RegisterKeyHandlerClientScript ();
			}

			Page page = Page;
			if (page != null && IsEnabled)
				page.RegisterEnabledControl (this);
#endif
		}

#if NET_2_0
		protected internal
#else		
		protected
#endif		
		override void Render (HtmlTextWriter w)
		{
			// Why didn't msft just override RenderContents!?
			RenderBeginTag (w);
			if (TextMode == TextBoxMode.MultiLine)
				HttpUtility.HtmlEncode (Text, w);
			RenderEndTag (w);
		}
		
#if NET_2_0
		protected virtual
#endif
		bool LoadPostData (string postDataKey, NameValueCollection postCollection)
		{
#if NET_2_0
			ValidateEvent (postDataKey, String.Empty);
#endif
			if (Text != postCollection [postDataKey]) {
				Text = postCollection [postDataKey];
				return true;
			}
			
			return false;
		}

#if NET_2_0
		protected virtual
#endif
		void RaisePostDataChangedEvent ()
		{
#if NET_2_0
			if (CausesValidation)
				Page.Validate (ValidationGroup);
#endif
			OnTextChanged (EventArgs.Empty);
		}

		bool IPostBackDataHandler.LoadPostData (string postDataKey, NameValueCollection postCollection)
		{
			return LoadPostData (postDataKey, postCollection);
		}
	
		void IPostBackDataHandler.RaisePostDataChangedEvent ()
		{
			RaisePostDataChangedEvent ();
		}

#if NET_2_0
		protected override object SaveViewState ()
		{
			if (TextMode == TextBoxMode.Password)
				ViewState.SetItemDirty ("Text", false);
			return base.SaveViewState ();
		}
#endif		
	
#if NET_2_0
		PostBackOptions GetPostBackOptions () {
			PostBackOptions options = new PostBackOptions (this);
			options.ActionUrl = null;
			options.ValidationGroup = null;
			options.Argument = String.Empty;
			options.RequiresJavaScriptProtocol = false;
			options.ClientSubmit = true;
			
			Page page = Page;
			options.PerformValidation = CausesValidation && page != null && page.AreValidatorsUplevel (ValidationGroup);
			if (options.PerformValidation)
				options.ValidationGroup = ValidationGroup;

			return options;
		}

		void RegisterKeyHandlerClientScript () {

			if (!Page.ClientScript.IsClientScriptBlockRegistered (typeof (TextBox), "KeyHandler")) {
				StringBuilder script=new StringBuilder();
				script.AppendLine ("function WebForm_TextBoxKeyHandler(event) {");
				script.AppendLine ("\tvar target = event.target;");
				script.AppendLine ("\tif ((target == null) || (typeof(target) == \"undefined\")) target = event.srcElement;");
				script.AppendLine ("\tif (event.keyCode == 13) {");
				script.AppendLine ("\t\tif ((typeof(target) != \"undefined\") && (target != null)) {");
				script.AppendLine ("\t\t\tif (typeof(target.onchange) != \"undefined\") {");
				script.AppendLine ("\t\t\t\ttarget.onchange();");
				script.AppendLine ("\t\t\t\tevent.cancelBubble = true;");
				script.AppendLine ("\t\t\t\tif (event.stopPropagation) event.stopPropagation();");
				script.AppendLine ("\t\t\t\treturn false;");
				script.AppendLine ("\t\t\t}");
				script.AppendLine ("\t\t}");
				script.AppendLine ("\t}");
				script.AppendLine ("\treturn true;");
				script.AppendLine ("}");
				Page.ClientScript.RegisterClientScriptBlock (typeof (TextBox), "KeyHandler", script.ToString(), true);
			}
		}
#endif

#if NET_2_0
		[DefaultValue (AutoCompleteType.None)]
		[Themeable (false)]
		public virtual AutoCompleteType AutoCompleteType 
		{
			get {
				object o = ViewState ["AutoCompleteType"];
				return o != null ? (AutoCompleteType) o : AutoCompleteType.None;
			}
			set {
				ViewState ["AutoCompleteType"] = value;
			}
		}
#endif		
		
		[DefaultValue(false)]
#if NET_2_0
		[Themeable (false)]
#endif		
		[WebSysDescription ("")]
		[WebCategory ("Behavior")]
		public virtual bool AutoPostBack {
			get {
				return ViewState.GetBool ("AutoPostBack", false);
			}
			set {
				ViewState ["AutoPostBack"] = value;
			}
		}

#if NET_2_0
		[DefaultValue (false)]
		[Themeable (false)]
		public virtual bool CausesValidation
		{
			get {
				return ViewState.GetBool ("CausesValidation", false);
			}
			set {
				ViewState["CausesValidation"] = value;
			}
		}
#endif		

#if ONLY_1_1
		[Bindable(true)]
#endif		
		[DefaultValue(0)]
		[WebSysDescription ("")]
		[WebCategory ("Appearance")]
		public virtual int Columns {
			get {
				return ViewState.GetInt ("Columns", 0);
			}
			set {
				if (value < 0)
					throw new ArgumentOutOfRangeException("value", "Columns value has to be 0 for 'not set' or bigger than 0.");
				else
					ViewState ["Columns"] = value;
			}
		}

#if ONLY_1_1
		[Bindable(true)]
#endif		
		[DefaultValue(0)]
#if NET_2_0
		[Themeable (false)]
#endif
		[WebSysDescription ("")]
		[WebCategory ("Behavior")]
		public virtual int MaxLength {
			get {
				return ViewState.GetInt ("MaxLength", 0);
			}
			set {
				if (value < 0)
					throw new ArgumentOutOfRangeException("value", "MaxLength value has to be 0 for 'not set' or bigger than 0.");
				else
					ViewState ["MaxLength"] = value;
			}
		}

		[Bindable(true)]
		[DefaultValue(false)]
#if NET_2_0
		[Themeable (false)]
#endif
		[WebSysDescription ("")]
		[WebCategory ("Behavior")]
		public virtual bool ReadOnly {
			get {
				return ViewState.GetBool ("ReadOnly", false);
			}
			set {
				ViewState ["ReadOnly"] = value;
			}
		}

#if ONLY_1_1
		[Bindable(true)]
#endif		
		[DefaultValue(0)]
#if NET_2_0
		[Themeable (false)]
#endif
		[WebSysDescription ("")]
		[WebCategory ("Behavior")]
		public virtual int Rows {
			get {
				return ViewState.GetInt ("Rows", 0);
			}
			set {
				if (value < 0)
					throw new ArgumentOutOfRangeException("value", "Rows value has to be 0 for 'not set' or bigger than 0.");
				else
					ViewState ["Rows"] = value;
			}
		}
	
#if NET_2_0 && HAVE_CONTROL_ADAPTERS
		protected virtual new
#else		
		protected override
#endif
		HtmlTextWriterTag TagKey {
			get {
				return TextMode == TextBoxMode.MultiLine ? HtmlTextWriterTag.Textarea : HtmlTextWriterTag.Input;
			}
		}

#if NET_2_0
		[Bindable(true, BindingDirection.TwoWay)]
#else
		[Bindable(true)]
#endif		
		[DefaultValue("")]
		[PersistenceMode(PersistenceMode.EncodedInnerDefaultProperty)]
#if NET_2_0
		[Localizable (true)]
		[Editor ("System.ComponentModel.Design.MultilineStringEditor," + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
#endif
		[WebSysDescription ("")]
		[WebCategory ("Appearance")]
		public virtual string Text {
			get {
				return ViewState.GetString ("Text", "");
			}
			set {
				ViewState ["Text"] = value;
#if ONLY_1_1
				if (TextMode == TextBoxMode.Password)
					ViewState.SetItemDirty ("Text", false);
#endif
			}
		}
	
		[DefaultValue(TextBoxMode.SingleLine)]
#if NET_2_0
		[Themeable (false)]
#endif
		[WebSysDescription ("")]
		[WebCategory ("Behavior")]
		public virtual TextBoxMode TextMode {
			get {
				return (TextBoxMode) ViewState.GetInt ("TextMode", (int) TextBoxMode.SingleLine);
			}
			set {
				ViewState ["TextMode"] = (int) value;
			}
		}

#if NET_2_0
		[Themeable (false)]
		[DefaultValue ("")]
		public virtual string ValidationGroup
		{
			get {
				return ViewState.GetString ("ValidationGroup", "");
			}
			set {
				ViewState ["ValidationGroup"] = value;
			}
		}
#endif		
	
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

		protected virtual void OnTextChanged (EventArgs e)
		{
			EventHandler h = (EventHandler) Events [TextChangedEvent];
			if (h != null)
				h (this, e);
		}
		
		static readonly object TextChangedEvent = new object ();

		[WebSysDescription ("")]
		[WebCategory ("Action")]
		public event EventHandler TextChanged {
			add { Events.AddHandler (TextChangedEvent, value); }
			remove { Events.RemoveHandler (TextChangedEvent, value); }
		}
	}
}
