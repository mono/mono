//------------------------------------------------------------------------------
// <copyright file="Button.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Web;
    using System.Web.UI;
    using System.Web.Util;


    /// <devdoc>
    ///    <para>Represents a Windows button control.</para>
    /// </devdoc>
    [
    DataBindingHandler("System.Web.UI.Design.TextDataBindingHandler, " + AssemblyRef.SystemDesign),
    DefaultEvent("Click"),
    DefaultProperty("Text"),
    Designer("System.Web.UI.Design.WebControls.ButtonDesigner, " + AssemblyRef.SystemDesign),
    ToolboxData("<{0}:Button runat=\"server\" Text=\"Button\"></{0}:Button>"),
    SupportsEventValidation
    ]
    public class Button : WebControl, IButtonControl, IPostBackEventHandler {

        private static readonly object EventClick = new object();
        private static readonly object EventCommand = new object();


        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.UI.WebControls.Button'/> class.</para>
        /// </devdoc>
        public Button() : base(HtmlTextWriterTag.Input) {
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
        WebSysDescription(SR.Button_CausesValidation),
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
        /// <para>Gets or sets the command associated with a <see cref='System.Web.UI.WebControls.Button'/> propogated in the <see langword='Command'/> event along with the <see cref='System.Web.UI.WebControls.Button.CommandArgument'/>
        /// property.</para>
        /// </devdoc>
        [
        DefaultValue(""),
        Themeable(false),
        WebCategory("Behavior"),
        WebSysDescription(SR.WebControl_CommandName),
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
        ///    <para>Gets or sets the property propogated in
        ///       the <see langword='Command'/> event with the associated <see cref='System.Web.UI.WebControls.Button.CommandName'/>
        ///       property.</para>
        /// </devdoc>
        [
        Bindable(true),
        DefaultValue(""),
        Themeable(false),
        WebCategory("Behavior"),
        WebSysDescription(SR.WebControl_CommandArgument),
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
        ///    The script that is executed on a client-side click.
        /// </devdoc>
        [
        DefaultValue(""),
        Themeable(false),
        WebCategory("Behavior"),
        WebSysDescription(SR.Button_OnClientClick),
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


        [
        DefaultValue(""),
        Editor("System.Web.UI.Design.UrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        Themeable(false),
        UrlProperty("*.aspx"),
        WebCategory("Behavior"),
        WebSysDescription(SR.Button_PostBackUrl),
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


        /// <devdoc>
        /// <para>Gets or sets the text caption displayed on the <see cref='System.Web.UI.WebControls.Button'/> .</para>
        /// </devdoc>
        [
        Bindable(true),
        Localizable(true),
        WebCategory("Appearance"),
        DefaultValue(""),
        WebSysDescription(SR.Button_Text),
        ]
        public string Text {
            get {
                string s = (string)ViewState["Text"];
                return((s == null) ? String.Empty : s);
            }
            set {
                ViewState["Text"] = value;
            }
        }


        /// <devdoc>
        /// Whether the button should use the client's submit mechanism to implement its
        /// behavior, or whether it should use the ASP.NET postback mechanism similar
        /// to LinkButton. By default, it uses the browser's submit mechanism.
        /// </devdoc>
        [
        DefaultValue(true),
        Themeable(false),
        WebCategory("Behavior"),
        WebSysDescription(SR.Button_UseSubmitBehavior),
        ]
        public virtual bool UseSubmitBehavior {
            get {
                object b = ViewState["UseSubmitBehavior"];
                return ((b == null) ? true : (bool)b);
            }
            set {
                ViewState["UseSubmitBehavior"] = value;
            }
        }


        [
        DefaultValue(""),
        Themeable(false),
        WebCategory("Behavior"),
        WebSysDescription(SR.PostBackControl_ValidationGroup),
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
        /// <para>Occurs when the <see cref='System.Web.UI.WebControls.Button'/> is clicked.</para>
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.Button_OnClick)
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
        /// <para>Occurs when the <see cref='System.Web.UI.WebControls.Button'/> is clicked.</para>
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
        /// <para>Adds the attributes of the <see cref='System.Web.UI.WebControls.Button'/> control to the output stream for rendering
        ///    on the client.</para>
        /// </devdoc>
        protected override void AddAttributesToRender(HtmlTextWriter writer) {
            bool submitButton = UseSubmitBehavior;

            // Make sure we are in a form tag with runat=server.
            if (Page != null) {
                Page.VerifyRenderingInServerForm(this);
            }

            if (submitButton) {
                writer.AddAttribute(HtmlTextWriterAttribute.Type, "submit");
            }
            else {
                writer.AddAttribute(HtmlTextWriterAttribute.Type, "button");
            }

            PostBackOptions options = GetPostBackOptions();
            string uniqueID = UniqueID;
            
            // Don't render Name on a button if __doPostBack is posting back to a different control
            // because Page will register this control as requiring post back notification even though
            // it's not the target of the postback.  If the TargetControl isn't this control, this control's
            // RaisePostBackEvent should never get called.  See VSWhidbey 477095.
            if (uniqueID != null && (options == null || options.TargetControl == this)) {
                writer.AddAttribute(HtmlTextWriterAttribute.Name, uniqueID);
            }


            writer.AddAttribute(HtmlTextWriterAttribute.Value, Text);

            // 

            bool effectiveEnabled = IsEnabled;

            string onClick = String.Empty;

            if (effectiveEnabled) {
                // Need to merge the onclick attribute with the OnClientClick

                // VSWhidbey 111791: Defensively add a ';' in case it is
                // missing in user customized onClick value above.
                onClick = Util.EnsureEndWithSemiColon(OnClientClick);

                if (HasAttributes) {
                    string userOnClick = Attributes["onclick"];
                    if (userOnClick != null) {
                        onClick += Util.EnsureEndWithSemiColon(userOnClick);
                        Attributes.Remove("onclick");
                    }
                }

                if (Page != null) {
                    string reference = Page.ClientScript.GetPostBackEventReference(options, false);
                    if (reference != null) {
                        onClick = Util.MergeScript(onClick, reference);
                    }
                }
            }

            if (Page != null) {
                Page.ClientScript.RegisterForEventValidation(options);
            }

            if (onClick.Length > 0) {
                writer.AddAttribute(HtmlTextWriterAttribute.Onclick, onClick);

                if (EnableLegacyRendering) {
                    writer.AddAttribute("language", "javascript", false);
                }
            }

            if (Enabled && !effectiveEnabled && SupportsDisabledAttribute) {
                // We need to do the cascade effect on the server, because the browser
                // only renders as disabled, but doesn't disable the functionality.
                writer.AddAttribute(HtmlTextWriterAttribute.Disabled, "disabled");
            }

            base.AddAttributesToRender(writer);
        }

        protected virtual PostBackOptions GetPostBackOptions() {
            PostBackOptions options = new PostBackOptions(this, String.Empty);
            options.ClientSubmit = false;

            if (Page != null) {
                if (CausesValidation && Page.GetValidators(ValidationGroup).Count > 0) {
                    options.PerformValidation = true;
                    options.ValidationGroup = ValidationGroup;
                }

                if (!String.IsNullOrEmpty(PostBackUrl)) {
                    options.ActionUrl = HttpUtility.UrlPathEncode(ResolveClientUrl(PostBackUrl));
                }
                options.ClientSubmit = !UseSubmitBehavior;
            }

            return options;
        }


        /// <devdoc>
        /// <para>Raises the <see langword='Click '/>event of a <see cref='System.Web.UI.WebControls.Button'/>
        /// .</para>
        /// </devdoc>
        protected virtual void OnClick(EventArgs e) {
            EventHandler handler = (EventHandler)Events[EventClick];
            if (handler != null) handler(this,e);
        }


        /// <devdoc>
        /// <para>Raises the <see langword='Command '/>event of a <see cref='System.Web.UI.WebControls.Button'/>
        /// .</para>
        /// </devdoc>
        protected virtual void OnCommand(CommandEventArgs e) {
            CommandEventHandler handler = (CommandEventHandler)Events[EventCommand];
            if (handler != null)
                handler(this,e);

            // Command events are bubbled up the control heirarchy
            RaiseBubbleEvent(this, e);
        }

        protected internal override void OnPreRender(EventArgs e) {
            base.OnPreRender(e);

            // VSWhidbey 489577
            if (Page != null && IsEnabled) {
                if ((CausesValidation && Page.GetValidators(ValidationGroup).Count > 0) ||
                     !String.IsNullOrEmpty(PostBackUrl)) {
                    Page.RegisterWebFormsScript();
                }
                else if (!UseSubmitBehavior) {
                    Page.RegisterPostBackScript();
                }
            }
        }

        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        protected internal override void RenderContents(HtmlTextWriter writer) {
            // Do not render the children of a button since it does not
            // make sense to have children of an <input> tag.
        }


        /// <internalonly/>
        /// <devdoc>
        /// <para>Raises events for the <see cref='System.Web.UI.WebControls.Button'/>
        /// control on post back.</para>
        /// </devdoc>
        void IPostBackEventHandler.RaisePostBackEvent(string eventArgument) {
            RaisePostBackEvent(eventArgument);
        }


        /// <internalonly/>
        /// <devdoc>
        /// <para>Raises events for the <see cref='System.Web.UI.WebControls.Button'/>
        /// control on post back.</para>
        /// </devdoc>
        protected virtual void RaisePostBackEvent(string eventArgument) {
            ValidateEvent(this.UniqueID, eventArgument);
            if (CausesValidation) {
                Page.Validate(ValidationGroup);
            }
            OnClick(EventArgs.Empty);
            OnCommand(new CommandEventArgs(CommandName, CommandArgument));
        }
    }
}

