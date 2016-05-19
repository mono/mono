//------------------------------------------------------------------------------
// <copyright file="HtmlAnchor.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * HtmlAnchor.cs
 *
 * Copyright (c) 2000 Microsoft Corporation
 */

namespace System.Web.UI.HtmlControls {
    using System.ComponentModel;
    using System;
    using System.Collections;
    using System.Web;
    using System.Web.UI;
    using System.Web.Util;
    using System.Security.Permissions;


    /// <devdoc>
    /// <para>The <see langword='HtmlAnchor'/>
    /// class defines the methods, properties, and
    /// events for the HtmlAnchor control.
    /// This
    /// class
    /// allows programmatic access to the
    /// HTML &lt;a&gt; element on the server.</para>
    /// </devdoc>
    [
    DefaultEvent("ServerClick"),
    SupportsEventValidation,
    ]
    public class HtmlAnchor : HtmlContainerControl, IPostBackEventHandler {

        private static readonly object EventServerClick = new object();

        /*
         *  Creates an intrinsic Html A control.
         */

        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.UI.HtmlControls.HtmlAnchor'/> class.</para>
        /// </devdoc>
        public HtmlAnchor() : base("a") {
        }

        [
        WebCategory("Behavior"),
        DefaultValue(true),
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

        /*
         * Href property.
         */

        /// <devdoc>
        ///    <para>Gets or sets the URL target of the link specified in the
        ///    <see cref='System.Web.UI.HtmlControls.HtmlAnchor'/>
        ///    server control.</para>
        /// </devdoc>
        [
        WebCategory("Navigation"),
        DefaultValue(""),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        UrlProperty()
        ]
        public string HRef {
            get {
                string s = Attributes["href"];
                return((s != null) ? s : String.Empty);
            }
            set {
                Attributes["href"] = MapStringAttributeToString(value);
            }
        }

        /*
         * Name of group this radio is in.
         */

        /// <devdoc>
        /// <para>Gets or sets the bookmark name defined in the <see cref='System.Web.UI.HtmlControls.HtmlAnchor'/>
        /// server
        /// control.</para>
        /// </devdoc>
        [
        WebCategory("Navigation"),
        DefaultValue(""),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public string Name {
            get {
                string s = Attributes["name"];
                return((s != null) ? s : String.Empty);
            }
            set {
                Attributes["name"] = MapStringAttributeToString(value);
            }
        }

        /*
         * Target window property.
         */

        /// <devdoc>
        ///    <para>
        ///       Gets or
        ///       sets the target window or frame
        ///       to load linked Web page content into.
        ///    </para>
        /// </devdoc>
        [
        WebCategory("Navigation"),
        DefaultValue(""),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public string Target {
            get {
                string s = Attributes["target"];
                return((s != null) ? s : String.Empty);
            }
            set {
                Attributes["target"] = MapStringAttributeToString(value);
            }
        }

        /*
         * Title property.
         */

        /// <devdoc>
        ///    <para> Gets or sets the title that
        ///       the browser displays when identifying linked content.</para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        Localizable(true),
        DefaultValue(""),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public string Title {
            get {
                string s = Attributes["title"];
                return((s != null) ? s : String.Empty);
            }
            set {
                Attributes["title"] = MapStringAttributeToString(value);
            }
        }

        [
        WebCategory("Behavior"),
        DefaultValue(""),
        WebSysDescription(SR.PostBackControl_ValidationGroup)
        ]
        public virtual string ValidationGroup {
            get {
                string s = (string)ViewState["ValidationGroup"];
                return((s == null) ? string.Empty : s);
            }
            set {
                ViewState["ValidationGroup"] = value;
            }
        }

        /// <devdoc>
        /// <para>Occurs on the server when a user clicks the <see cref='System.Web.UI.HtmlControls.HtmlAnchor'/> control on the
        ///    browser.</para>
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.HtmlControl_OnServerClick)
        ]
        public event EventHandler ServerClick {
            add {
                Events.AddHandler(EventServerClick, value);
            }
            remove {
                Events.RemoveHandler(EventServerClick, value);
            }
        }

        private PostBackOptions GetPostBackOptions() {
            PostBackOptions options = new PostBackOptions(this, string.Empty);
            options.RequiresJavaScriptProtocol = true;

            if (CausesValidation && Page.GetValidators(ValidationGroup).Count > 0) {
                options.PerformValidation = true;
                options.ValidationGroup = ValidationGroup;
            }

            return options;
        }

        /// <internalonly/>
        protected internal override void OnPreRender(EventArgs e) {
            base.OnPreRender(e);
            if (Page != null && Events[EventServerClick] != null) {
                Page.RegisterPostBackScript();

                // VSWhidbey 489577
                if (CausesValidation && Page.GetValidators(ValidationGroup).Count > 0) {
                    Page.RegisterWebFormsScript();
                }
            }
        }

        /*
         * Override to generate postback code for onclick.
         */

        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        protected override void RenderAttributes(HtmlTextWriter writer) {
            if (Events[EventServerClick] != null) {
                Attributes.Remove("href");
                base.RenderAttributes(writer);

                PostBackOptions options = GetPostBackOptions();
                Debug.Assert(options != null);
                string postBackEventReference = Page.ClientScript.GetPostBackEventReference(options, true);

                Debug.Assert(!string.IsNullOrEmpty(postBackEventReference));
                writer.WriteAttribute("href", postBackEventReference, true);
            }
            else {
                PreProcessRelativeReferenceAttribute(writer, "href");
                base.RenderAttributes(writer);
            }
        }

        /*
         * Method used to raise the OnServerClick event.
         */

        /// <devdoc>
        /// <para>Raises the <see langword='ServerClick'/>
        /// event.</para>
        /// </devdoc>
        protected virtual void OnServerClick(EventArgs e) {
            EventHandler handler = (EventHandler)Events[EventServerClick];
            if (handler != null) handler(this, e);
        }

        /*
         * Method of IPostBackEventHandler interface to raise events on post back.
         * Button fires an OnServerClick event.
         */

        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        void IPostBackEventHandler.RaisePostBackEvent(string eventArgument) {
            RaisePostBackEvent(eventArgument);
        }


        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        protected virtual void RaisePostBackEvent(string eventArgument) {
            ValidateEvent(UniqueID, eventArgument);

            if (CausesValidation) {
                Page.Validate(ValidationGroup);
            }
            OnServerClick(EventArgs.Empty);
        }
    }
}
