//------------------------------------------------------------------------------
// <copyright file="ImageButton.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 */

namespace System.Web.UI.WebControls {

    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Web;
    using System.Web.UI;
    using System.Web.Util;


    /// <devdoc>
    ///    <para>Creates a control that displays an image, responds to mouse clicks,
    ///       and records the mouse pointer position.</para>
    /// </devdoc>
    [
    DefaultEvent("Click"),
    Designer("System.Web.UI.Design.WebControls.PreviewControlDesigner, " + AssemblyRef.SystemDesign),
    SupportsEventValidation,
    ]
    public class ImageButton : Image, IPostBackDataHandler, IPostBackEventHandler, IButtonControl {

        private static readonly object EventClick = new object();
        private static readonly object EventButtonClick = new object();
        private static readonly object EventCommand = new object();

        private int x = 0;
        private int y = 0;
        private double xRaw = 0;
        private double yRaw = 0;

        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.UI.WebControls.ImageButton'/> class.</para>
        /// </devdoc>
        public ImageButton() {
        }


        /// <devdoc>
        /// <para>Gets or sets the command associated with the <see cref='System.Web.UI.WebControls.ImageButton'/> that is propogated in the <see langword='Command'/> event along with the <see cref='System.Web.UI.WebControls.ImageButton.CommandArgument'/>
        /// property.</para>
        /// </devdoc>
        [
        DefaultValue(""),
        WebCategory("Behavior"),
        WebSysDescription(SR.WebControl_CommandName),
        Themeable(false),
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
        ///    <para>Gets or sets an optional argument that is propogated in
        ///       the <see langword='Command'/> event with the associated
        ///    <see cref='System.Web.UI.WebControls.ImageButton.CommandName'/>
        ///    property.</para>
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
        ///    <para>Gets or sets whether pressing the button causes page validation to fire. This defaults to True so that when
        ///          using validation controls, the validation state of all controls are updated when the button is clicked, both
        ///          on the client and the server. Setting this to False is useful when defining a cancel or reset button on a page
        ///          that has validators.</para>
        /// </devdoc>
        [
        Themeable(false),
        DefaultValue(true),
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


        [
        Browsable(true),
        EditorBrowsableAttribute(EditorBrowsableState.Always),
        Bindable(true),
        WebCategory("Behavior"),
        DefaultValue(true),
        WebSysDescription(SR.WebControl_Enabled)
        ]
        public override bool Enabled {
            get {
                return base.Enabled;
            }
            set {
                base.Enabled = value;
            }
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        EditorBrowsable(EditorBrowsableState.Never),
        Themeable(false),
        ]
        public override bool GenerateEmptyAlternateText {
            get {
                return base.GenerateEmptyAlternateText;
            }
            set {
                throw new NotSupportedException(SR.GetString(SR.Property_Set_Not_Supported, "GenerateEmptyAlternateText", this.GetType().ToString()));
            }
        }


        /// <devdoc>
        ///    The script that is executed on a client-side click.
        /// </devdoc>
        [
        DefaultValue(""),
        WebCategory("Behavior"),
        WebSysDescription(SR.Button_OnClientClick),
        Themeable(false),
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

        public override bool SupportsDisabledAttribute {
            get {
                return true;
            }
        }

        /// <devdoc>
        ///    <para> Gets a value that represents the tag HtmlTextWriterTag.Input. This property is read-only.</para>
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        protected override HtmlTextWriterTag TagKey {
            get {
                return HtmlTextWriterTag.Input;
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
        /// <para>Represents the method that will handle the <see langword='ImageClick'/> event of an <see cref='System.Web.UI.WebControls.ImageButton'/>.</para>
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.ImageButton_OnClick)
        ]
        public event ImageClickEventHandler Click {
            add {
                Events.AddHandler(EventClick, value);
            }
            remove {
                Events.RemoveHandler(EventClick, value);
            }
        }


        /// <devdoc>
        /// <para>Represents the method that will handle the <see langword='Click'/> event of an <see cref='System.Web.UI.WebControls.ImageButton'/>.</para>
        /// </devdoc>
        event EventHandler IButtonControl.Click {
            add {
                Events.AddHandler(EventButtonClick, value);
            }
            remove {
                Events.RemoveHandler(EventButtonClick, value);
            }
        }


        /// <devdoc>
        /// <para>Represents the method that will handle the <see langword='Command'/> event of an <see cref='System.Web.UI.WebControls.ImageButton'/>.</para>
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.ImageButton_OnCommand)
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
        /// <para>Adds the attributes of an <see cref='System.Web.UI.WebControls.ImageButton'/> to the output
        ///    stream for rendering on the client.</para>
        /// </devdoc>
        protected override void AddAttributesToRender(HtmlTextWriter writer) {
            Page page = Page;

            // Make sure we are in a form tag with runat=server.
            if (page != null) {
                page.VerifyRenderingInServerForm(this);
            }

            writer.AddAttribute(HtmlTextWriterAttribute.Type,"image");

            string uniqueID = UniqueID;
            PostBackOptions options = GetPostBackOptions();

            // Don't render Name on a button if __doPostBack is posting back to a different control
            // because Page will register this control as requiring post back notification even though
            // it's not the target of the postback.  If the TargetControl isn't this control, this control's
            // RaisePostBackEvent should never get called.  See VSWhidbey 477095.
            if (uniqueID != null && (options == null || options.TargetControl == this)) {
                writer.AddAttribute(HtmlTextWriterAttribute.Name, uniqueID);
            }

            // 

            bool effectiveEnabled = IsEnabled;


            string onClick = String.Empty;

            if (effectiveEnabled) {
                // Need to merge the onclick attribute with the OnClientClick
                onClick = Util.EnsureEndWithSemiColon(OnClientClick);
                if (HasAttributes) {
                    string userOnClick = Attributes["onclick"];
                    if (userOnClick != null) {
                        onClick += Util.EnsureEndWithSemiColon(userOnClick);
                        Attributes.Remove("onclick");
                    }
                }
            }

            if (Enabled && !effectiveEnabled && SupportsDisabledAttribute) {
                // We need to do the cascade effect on the server, because the browser
                // only renders as disabled, but doesn't disable the functionality.
                writer.AddAttribute(HtmlTextWriterAttribute.Disabled, "disabled");
            }

            base.AddAttributesToRender(writer);

            if (page != null && options != null) {
                page.ClientScript.RegisterForEventValidation(options);

                if (effectiveEnabled) {
                    string postBackEventReference = page.ClientScript.GetPostBackEventReference(options, false);
                    if (!String.IsNullOrEmpty(postBackEventReference)) {
                        onClick = Util.MergeScript(onClick, postBackEventReference);
                        if (options.ClientSubmit) {
                            // Dev10 Bugs 584471
                            // a derived control may return PostBackOptions with ClientSubmit=true,
                            // such as DataControlImageButton. Without a return false, there would be
                            // a double submit.
                            onClick = Util.EnsureEndWithSemiColon(onClick) + "return false;";
                        }
                    }
                }
            }

            if (onClick.Length > 0) {
                writer.AddAttribute(HtmlTextWriterAttribute.Onclick, onClick);

                if (EnableLegacyRendering) {
                    writer.AddAttribute("language", "javascript", false);
                }
            }
        }

        // Returns the client post back options.
        protected virtual PostBackOptions GetPostBackOptions() {
            PostBackOptions options = new PostBackOptions(this, String.Empty);
            // The postback script ends up in onclick, not href,
            // so we shouldn't force the javascript protocol like we do in LinkButton.
            options.ClientSubmit = false;

            if (!String.IsNullOrEmpty(PostBackUrl)) {
                options.ActionUrl = HttpUtility.UrlPathEncode(ResolveClientUrl(PostBackUrl));
            }

            if (CausesValidation && Page != null && Page.GetValidators(ValidationGroup).Count > 0) {
                options.PerformValidation = true;
                options.ValidationGroup = ValidationGroup;
            }

            return options;
        }


        /// <internalonly/>
        /// <devdoc>
        /// <para>Processes posted data for the <see cref='System.Web.UI.WebControls.ImageButton'/> control.</para>
        /// </devdoc>
        bool IPostBackDataHandler.LoadPostData(string postDataKey, NameValueCollection postCollection) {
            return LoadPostData(postDataKey, postCollection);
        }

        /// <internalonly/>
        /// <devdoc>
        /// <para>Processes posted data for the <see cref='System.Web.UI.WebControls.ImageButton'/> control.</para>
        /// </devdoc>
        protected virtual bool LoadPostData(string postDataKey, NameValueCollection postCollection) {
            string name = UniqueID;
            string postX = postCollection[name + ".x"];
            string postY = postCollection[name + ".y"];
            if (!String.IsNullOrEmpty(postX) && !String.IsNullOrEmpty(postY)) {
                xRaw = ReadPositionFromPost(postX);
                yRaw = ReadPositionFromPost(postY);
                x = (int)xRaw;
                y = (int)yRaw;
                
                if (Page != null) {
                    Page.RegisterRequiresRaiseEvent(this);
                }
            }
            return false;
        }

        internal static double ReadPositionFromPost(string requestValue) {
            double doubleValue;
            if (HttpUtility.TryParseCoordinates(requestValue, out doubleValue)) {
                return doubleValue;
            }
            return 0;
        }

        /// <devdoc>
        /// <para>Raises the <see langword='Click'/> event.</para>
        /// </devdoc>
        protected virtual void OnClick(ImageClickEventArgs e) {
            ImageClickEventHandler onClickHandler = (ImageClickEventHandler)Events[EventClick];
            if (onClickHandler != null) onClickHandler(this,e);

            EventHandler onButtonClickHandler = (EventHandler)Events[EventButtonClick];
            if (onButtonClickHandler != null) onButtonClickHandler(this,(EventArgs)e);
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
        ///    <para>
        ///       Determine
        ///       if the image has been clicked prior to rendering on the client. </para>
        /// </devdoc>
        protected internal override void OnPreRender(EventArgs e) {
            base.OnPreRender(e);
            if (Page != null) {
                Page.RegisterRequiresPostBack(this);

                if (IsEnabled && 
                    ((CausesValidation && Page.GetValidators(ValidationGroup).Count > 0) ||
                     !String.IsNullOrEmpty(PostBackUrl))) {
                    Page.RegisterWebFormsScript();  // VSWhidbey 489577
                }
            }
        }


        /// <internalonly/>
        /// <devdoc>
        /// <para>Raises events on post back for the <see cref='System.Web.UI.WebControls.ImageButton'/>
        /// control.</para>
        /// </devdoc>
        void IPostBackEventHandler.RaisePostBackEvent(string eventArgument) {
            RaisePostBackEvent(eventArgument);
        }


        /// <internalonly/>
        /// <devdoc>
        /// <para>Raises events on post back for the <see cref='System.Web.UI.WebControls.ImageButton'/>
        /// control.</para>
        /// </devdoc>
        protected virtual void RaisePostBackEvent(string eventArgument) {
            ValidateEvent(this.UniqueID, eventArgument);

            if (CausesValidation) {
                Page.Validate(ValidationGroup);
            }
            OnClick(new ImageClickEventArgs(x, y, xRaw, yRaw));
            OnCommand(new CommandEventArgs(CommandName, CommandArgument));

        }


        /// <internalonly/>
        /// <devdoc>
        /// Raised when posted data for a control has changed.
        /// </devdoc>
        void IPostBackDataHandler.RaisePostDataChangedEvent() {
            RaisePostDataChangedEvent();
        }


        /// <internalonly/>
        /// <devdoc>
        /// Raised when posted data for a control has changed.
        /// </devdoc>
        protected virtual void RaisePostDataChangedEvent() {
        }


        /// <internalonly/>
        /// <devdoc>
        /// IButtonControl.Text implementation.  IButtonControl is used internally for adaptive rendering, but
        /// property is explicitly implemented because the developer should use AlternateText, as in V1.
        /// </devdoc>
        string IButtonControl.Text {
            get {
                return Text;
            }
            set {
                Text = value;
            }
        }


        /// <internalonly/>
        /// <devdoc>
        /// IButtonControl.Text implementation.  IButtonControl is used internally for adaptive rendering, but
        /// property is explicitly implemented because the developer should use AlternateText, as in V1.
        /// </devdoc>
        protected virtual string Text {
            get {
                return AlternateText;
            }
            set {
                AlternateText = value;
            }
        }
    }
}
