//------------------------------------------------------------------------------
// <copyright file="LinkButton.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Web;
    using System.Web.UI;
    using System.Web.Util;

    /// <devdoc>
    /// <para>Interacts with the parser to build a <see cref='System.Web.UI.WebControls.LinkButton'/> control.</para>
    /// </devdoc>
    public class LinkButtonControlBuilder : ControlBuilder {


        /// <internalonly/>
        /// <devdoc>
        ///    <para>Specifies whether white space literals are allowed.</para>
        /// </devdoc>
        public override bool AllowWhitespaceLiterals() {
            return false;
        }
    }



    /// <devdoc>
    ///    <para>Constructs a link button and defines its properties.</para>
    /// </devdoc>
    [
    ControlBuilderAttribute(typeof(LinkButtonControlBuilder)),
    DataBindingHandler("System.Web.UI.Design.TextDataBindingHandler, " + AssemblyRef.SystemDesign),
    DefaultEvent("Click"),
    DefaultProperty("Text"),
    ToolboxData("<{0}:LinkButton runat=\"server\">LinkButton</{0}:LinkButton>"),
    Designer("System.Web.UI.Design.WebControls.LinkButtonDesigner, " + AssemblyRef.SystemDesign),
    ParseChildren(false),
    SupportsEventValidation
    ]
    public class LinkButton : WebControl, IButtonControl, IPostBackEventHandler {

        private bool _textSetByAddParsedSubObject = false;
        private static readonly object EventClick = new object();
        private static readonly object EventCommand = new object();


        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.UI.WebControls.LinkButton'/> class.</para>
        /// </devdoc>
        public LinkButton() : base(HtmlTextWriterTag.A) {
        }


        /// <devdoc>
        ///    <para>Specifies the command name that is propagated in the
        ///    <see cref='System.Web.UI.WebControls.LinkButton.Command'/>event along with the associated <see cref='System.Web.UI.WebControls.LinkButton.CommandArgument'/>
        ///    property.</para>
        /// </devdoc>
        [
        DefaultValue(""),
        Themeable(false),
        WebCategory("Behavior"),
        WebSysDescription(SR.WebControl_CommandName)
        ]
        public string CommandName {
            get {
                string s = (string)ViewState["CommandName"];
                return((s == null) ? String.Empty : s);
            }
            set {
                ViewState["CommandName"] = value;
            }
        }



        /// <devdoc>
        ///    <para>Specifies the command argument that is propagated in the
        ///    <see langword='Command '/>event along with the associated <see cref='System.Web.UI.WebControls.LinkButton.CommandName'/>
        ///    property.</para>
        /// </devdoc>
        [
        Bindable(true),
        DefaultValue(""),
        Themeable(false),
        WebCategory("Behavior"),
        WebSysDescription(SR.WebControl_CommandArgument)
        ]
        public string CommandArgument {
            get {
                string s = (string)ViewState["CommandArgument"];
                return((s == null) ? String.Empty : s);
            }
            set {
                ViewState["CommandArgument"] = value;
            }
        }


        /// <devdoc>
        ///    <para>Gets or sets whether pressing the button causes page validation to fire. This defaults to True so that when
        ///          using validation controls, the validation state of all controls are updated when the button is clicked, both
        ///          on the client and the server. Setting this to False is useful when defining a cancel or reset button on a page
        ///          that has validators.</para>
        /// </devdoc>
        [
        DefaultValue(true),
        Themeable(false),
        WebCategory("Behavior"),
        WebSysDescription(SR.Button_CausesValidation)
        ]
        public virtual bool CausesValidation {
            get {
                object b = ViewState["CausesValidation"];
                return((b == null) ? true : (bool)b);
            }
            set {
                ViewState["CausesValidation"] = value;
            }
        }

        /// <devdoc>
        ///    The script that is executed on a client-side click.
        /// </devdoc>
        [
        DefaultValue(""),
        Themeable(false),
        WebCategory("Behavior"),
        WebSysDescription(SR.Button_OnClientClick)
        ]
        public virtual string OnClientClick {
            get {
                string s = (string)ViewState["OnClientClick"];
                if (s == null) {
                    return String.Empty;
                }
                return s;
            }
            set {
                ViewState["OnClientClick"] = value;
            }
        }

        public override bool SupportsDisabledAttribute {
            get {
                return RenderingCompatibility < VersionUtil.Framework40;
            }
        }

        internal override bool RequiresLegacyRendering {
            get {
                return true;
            }
        }


        /// <devdoc>
        ///    <para>Gets or sets the text display for the link button.</para>
        /// </devdoc>
        [
        Localizable(true),
        Bindable(true),
        WebCategory("Appearance"),
        DefaultValue(""),
        WebSysDescription(SR.LinkButton_Text),
        PersistenceMode(PersistenceMode.InnerDefaultProperty)
        ]
        public virtual string Text {
            get {
                object o = ViewState["Text"];
                return((o == null) ? String.Empty : (string)o);
            }
            set {
                if (HasControls()) {
                    Controls.Clear();
                }
                ViewState["Text"] = value;
            }
        }


        [
        DefaultValue(""),
        Editor("System.Web.UI.Design.UrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        Themeable(false),
        UrlProperty("*.aspx"),
        WebCategory("Behavior"),
        WebSysDescription(SR.Button_PostBackUrl)
        ]
        public virtual string PostBackUrl {
            get {
                string s = (string)ViewState["PostBackUrl"];
                return s == null? String.Empty : s;
            }
            set {
                ViewState["PostBackUrl"] = value;
            }
        }

        [
        WebCategory("Behavior"),
        Themeable(false),
        DefaultValue(""),
        WebSysDescription(SR.PostBackControl_ValidationGroup)
        ]
        public virtual string ValidationGroup {
            get {
                string s = (string)ViewState["ValidationGroup"];
                return((s == null) ? String.Empty : s);
            }
            set {
                ViewState["ValidationGroup"] = value;
            }
        }


        /// <devdoc>
        ///    <para>Occurs when the link button is clicked.</para>
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.LinkButton_OnClick)
        ]
        public event EventHandler Click {
            add {
                Events.AddHandler(EventClick, value);
            }
            remove {
                Events.RemoveHandler(EventClick, value);
            }
        }



        /// <devdoc>
        /// <para>Occurs when any item is clicked within the <see cref='System.Web.UI.WebControls.LinkButton'/> control tree.</para>
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.Button_OnCommand)
        ]
        public event CommandEventHandler Command {
            add {
                Events.AddHandler(EventCommand, value);
            }
            remove {
                Events.RemoveHandler(EventCommand, value);
            }
        }


        /// <internalonly/>
        /// <devdoc>
        ///    Render the attributes on the begin tag.
        /// </devdoc>
        protected override void AddAttributesToRender(HtmlTextWriter writer) {
            // Make sure we are in a form tag with runat=server.
            if (Page != null) {
                Page.VerifyRenderingInServerForm(this);
            }

            // Need to merge the onclick attribute with the OnClientClick
            string onClick = Util.EnsureEndWithSemiColon(OnClientClick);

            if (HasAttributes) {
                string userOnClick = Attributes["onclick"];
                if (userOnClick != null) {
                    // We don't use Util.MergeScript because OnClientClick or
                    // onclick attribute are set by page developer directly.  We
                    // should preserve the value without adding javascript prefix.
                    onClick += Util.EnsureEndWithSemiColon(userOnClick);
                    Attributes.Remove("onclick");
                }
            }

            if (onClick.Length > 0) {
                writer.AddAttribute(HtmlTextWriterAttribute.Onclick, onClick);
            }

            bool effectiveEnabled = IsEnabled;
            if (Enabled && !effectiveEnabled && SupportsDisabledAttribute) {
                // We need to do the cascade effect on the server, because the browser
                // only renders as disabled, but doesn't disable the functionality.
                writer.AddAttribute(HtmlTextWriterAttribute.Disabled, "disabled");
            }

            base.AddAttributesToRender(writer);

            if (effectiveEnabled && Page != null) {
                // 

                PostBackOptions options = GetPostBackOptions();
                string postBackEventReference = null;
                if (options != null) {
                    postBackEventReference = Page.ClientScript.GetPostBackEventReference(options, true);
                }

                // If the postBackEventReference is empty, use a javascript no-op instead, since
                // <a href="" /> is a link to the root of the current directory.
                if (String.IsNullOrEmpty(postBackEventReference)) {
                    postBackEventReference = "javascript:void(0)";
                }

                writer.AddAttribute(HtmlTextWriterAttribute.Href, postBackEventReference);
            }
        }


        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        protected override void AddParsedSubObject(object obj) {
            if (HasControls()) {
                base.AddParsedSubObject(obj);
            }
            else {
                if (obj is LiteralControl) {
                    if (_textSetByAddParsedSubObject) {
                        Text += ((LiteralControl)obj).Text;
                    }
                    else {
                        Text = ((LiteralControl)obj).Text;
                    }
                    _textSetByAddParsedSubObject = true;
                }
                else {
                    string currentText = Text;
                    if (currentText.Length != 0) {
                        Text = String.Empty;
                        base.AddParsedSubObject(new LiteralControl(currentText));
                    }
                    base.AddParsedSubObject(obj);
                }
            }
        }

        // Returns the client post back options.
        protected virtual PostBackOptions GetPostBackOptions() {
            PostBackOptions options = new PostBackOptions(this, String.Empty);
            options.RequiresJavaScriptProtocol = true;

            if (!String.IsNullOrEmpty(PostBackUrl)) {
                // VSWhidbey 424614: Since the url is embedded as javascript in attribute,
                // we should match the same encoding as done on HyperLink.NavigateUrl value.
                options.ActionUrl = HttpUtility.UrlPathEncode(ResolveClientUrl(PostBackUrl));

                // Also, there is a specific behavior in IE that when the script
                // is triggered in href attribute, the whole string will be
                // decoded once before the code is run.  This doesn't happen to
                // onclick or other event attributes.  So here we do an extra
                // encoding to compensate the weird behavior on IE.
                if (!DesignMode && Page != null &&
                    String.Equals(Page.Request.Browser.Browser, "IE", StringComparison.OrdinalIgnoreCase)) {
                    options.ActionUrl = Util.QuoteJScriptString(options.ActionUrl, true);
                }
            }

            if (CausesValidation && Page.GetValidators(ValidationGroup).Count > 0) {
                options.PerformValidation = true;
                options.ValidationGroup = ValidationGroup;
            }

            return options;
        }


        /// <internalonly/>
        /// <devdoc>
        ///    Load previously saved state.
        ///    Overridden to synchronize Text property with LiteralContent.
        /// </devdoc>
        protected override void LoadViewState(object savedState) {
            if (savedState != null) {
                base.LoadViewState(savedState);
                string s = (string)ViewState["Text"];
                // Dev10 703061 If Text is set, we want to clear out any child controls, but not dirty viewstate
                if (s != null && HasControls()) {
                    Controls.Clear();
                }
            }
        }


        /// <devdoc>
        /// <para>Raises the <see langword='Click '/> event.</para>
        /// </devdoc>
        protected virtual void OnClick(EventArgs e) {
            EventHandler onClickHandler = (EventHandler)Events[EventClick];
            if (onClickHandler != null) onClickHandler(this,e);
        }


        /// <devdoc>
        /// <para>Raises the <see langword='Command'/> event.</para>
        /// </devdoc>
        protected virtual void OnCommand(CommandEventArgs e) {
            CommandEventHandler onCommandHandler = (CommandEventHandler)Events[EventCommand];
            if (onCommandHandler != null)
                onCommandHandler(this,e);

            // Command events are bubbled up the control heirarchy
            RaiseBubbleEvent(this, e);
        }


        /// <internalonly/>
        /// <devdoc>
        /// <para>Raises a <see langword='Click '/>event upon postback
        /// to the server, and a <see langword='Command'/> event if the <see cref='System.Web.UI.WebControls.LinkButton.CommandName'/>
        /// is defined.</para>
        /// </devdoc>
        void IPostBackEventHandler.RaisePostBackEvent(string eventArgument) {
            RaisePostBackEvent(eventArgument);
        }


        /// <internalonly/>
        /// <devdoc>
        /// <para>Raises a <see langword='Click '/>event upon postback
        /// to the server, and a <see langword='Command'/> event if the <see cref='System.Web.UI.WebControls.LinkButton.CommandName'/>
        /// is defined.</para>
        /// </devdoc>
        protected virtual void RaisePostBackEvent(string eventArgument) {
            ValidateEvent(this.UniqueID, eventArgument);
            if (CausesValidation) {
                Page.Validate(ValidationGroup);
            }
            OnClick(EventArgs.Empty);
            OnCommand(new CommandEventArgs(CommandName, CommandArgument));
        }


        /// <internalonly/>
        protected internal override void OnPreRender(EventArgs e) {
            base.OnPreRender(e);
            if (Page != null && Enabled) {
                Page.RegisterPostBackScript();

                if ((CausesValidation && Page.GetValidators(ValidationGroup).Count > 0) ||
                     !String.IsNullOrEmpty(PostBackUrl)) {
                    Page.RegisterWebFormsScript();  // VSWhidbey 489577
                }
            }
        }


        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        protected internal override void RenderContents(HtmlTextWriter writer) {
            if (HasRenderingData()) {
                base.RenderContents(writer);
            }
            else {
                writer.Write(Text);
            }
        }
    }
}

