//------------------------------------------------------------------------------
// <copyright file="HtmlInputText.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * HtmlInputText.cs
 *
 * Copyright (c) 2000 Microsoft Corporation
 */

namespace System.Web.UI.HtmlControls {
    using System.ComponentModel;
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Web;
    using System.Web.UI;
    using System.Security.Permissions;


/// <devdoc>
///    <para>
///       The <see langword='HtmlInputText'/>
///       class defines the methods, properties, and events for the HtmlInputText server
///       control. This class allows programmatic access to the HTML &lt;input type=
///       text&gt;
///       and &lt;input type=
///       password&gt; elements on the server.
///    </para>
/// </devdoc>
    [
    DefaultEvent("ServerChange"),
    SupportsEventValidation,
    ValidationProperty("Value"),
    ]
    public class HtmlInputText : HtmlInputControl, IPostBackDataHandler {

        private static readonly object EventServerChange = new object();

        /*
         * Creates an intrinsic Html INPUT type=text control.
         */

        public HtmlInputText() : base("text") {
        }

        /*
         * Creates an intrinsic Html INPUT type=text control.
         */

        /// <devdoc>
        /// </devdoc>
        public HtmlInputText(string type) : base(type) {
        }

        /*
         * The property for the maximum characters allowed.
         */

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the maximum number of characters that
        ///       can be typed into the text box.
        ///    </para>
        /// </devdoc>
        [
        WebCategory("Behavior"),
        DefaultValue(""),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public int MaxLength {
            get {
                string s = (string)ViewState["maxlength"];
                return((s != null) ? Int32.Parse(s, CultureInfo.InvariantCulture) : -1);
            }

            set {
                Attributes["maxlength"] = MapIntegerAttributeToString(value);
            }
        }

        // 

        /*
         * The property for the width of the TextBox in characters.
         */

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the width of a text box, in characters.
        ///    </para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(-1),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public int Size {
            get {
                string s = Attributes["size"];
                return((s != null) ? Int32.Parse(s, CultureInfo.InvariantCulture) : -1);
            }
            set {
                Attributes["size"] = MapIntegerAttributeToString(value);
            }
        }

        /*
         * Value property.
         */

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the
        ///       contents of a text box.
        ///    </para>
        /// </devdoc>
        public override string Value {
            get {
                string s = Attributes["value"];
                return((s != null) ? s : String.Empty);
            }
            set {
                Attributes["value"] = MapStringAttributeToString(value);
            }
        }


        [
        WebCategory("Action"),
        WebSysDescription(SR.HtmlInputText_ServerChange)
        ]
        public event EventHandler ServerChange {
            add {
                Events.AddHandler(EventServerChange, value);
            }
            remove {
                Events.RemoveHandler(EventServerChange, value);
            }
        }

        /*
         * Method used to raise the OnServerChange event.
         */

        /// <devdoc>
        /// </devdoc>
        protected virtual void OnServerChange(EventArgs e) {
            EventHandler handler = (EventHandler)Events[EventServerChange];
            if (handler != null) handler(this, e);
        }

        /*
         *
         */

        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        protected internal override void OnPreRender(EventArgs e) {
            base.OnPreRender(e);

            bool disabled = Disabled;
            if (!disabled && Page != null) {
                Page.RegisterEnabledControl(this);
            }

            // if no change handler, no need to save posted property unless we are disabled;
            // VSWhidbey 419040: We should never save password value in ViewState
            if ((!disabled && Events[EventServerChange] == null) ||
                Type.Equals("password", StringComparison.OrdinalIgnoreCase)) {
                ViewState.SetItemDirty("value", false);
            }
        }

        protected override void RenderAttributes(HtmlTextWriter writer) {
            base.RenderAttributes(writer);

            if (Page != null) {
                Page.ClientScript.RegisterForEventValidation(RenderedNameAttribute);
            }
        }

        /*
         * Method of IPostBackDataHandler interface to process posted data.
         * InputText process a newly posted value.
         */

        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        bool IPostBackDataHandler.LoadPostData(string postDataKey, NameValueCollection postCollection) {
            return LoadPostData(postDataKey, postCollection);
        }


        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        protected virtual bool LoadPostData(string postDataKey, NameValueCollection postCollection) {
            string current = Value;
            string inputString = postCollection.GetValues(postDataKey)[0];

            if (!current.Equals(inputString)) {
                ValidateEvent(postDataKey);

                Value = inputString;
                return true;
            }

            return false;
        }

        /*
         * Method of IPostBackDataHandler interface which is invoked whenever posted data
         * for a control has changed.  InputText fires an OnServerChange event.
         */

        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        void IPostBackDataHandler.RaisePostDataChangedEvent() {
            RaisePostDataChangedEvent();
        }


        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        protected virtual void RaisePostDataChangedEvent() {
            OnServerChange(EventArgs.Empty);
        }
    }
}
