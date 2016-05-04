//------------------------------------------------------------------------------
// <copyright file="HtmlInputRadioButton.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * HtmlInputRadioButton.cs
 *
 * Copyright (c) 2000 Microsoft Corporation
 */

namespace System.Web.UI.HtmlControls {
    using System.ComponentModel;
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Web;
    using System.Web.UI;
    using System.Security.Permissions;


/// <devdoc>
///    <para>
///       The <see langword='HtmlInputRadioButton'/> class defines the methods,
///       properties, and events for the HtmlInputRadio control. This class allows
///       programmatic access to the HTML &lt;input type=
///       radio&gt;
///       element on the server.
///    </para>
/// </devdoc>
    [
    DefaultEvent("ServerChange"),
    SupportsEventValidation,
    ]
    public class HtmlInputRadioButton : HtmlInputControl, IPostBackDataHandler {

        private static readonly object EventServerChange = new object();

        /*
         * Creates an intrinsic Html INPUT type=radio control.
         */

        public HtmlInputRadioButton() : base("radio") {
        }

        /*
         * Checked property.
         */

        /// <devdoc>
        ///    <para>
        ///       Gets or sets a value indicating whether a radio button is
        ///       currently selected or not.
        ///    </para>
        /// </devdoc>
        [
        WebCategory("Default"),
        DefaultValue(""),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public bool Checked {
            get {
                string s = Attributes["checked"];
                return((s != null) ? (s.Equals("checked")) : false);
            }
            set {
                if (value)
                    Attributes["checked"] = "checked";
                else
                    Attributes["checked"] = null;
            }
        }


        /// <devdoc>
        ///    <para>
        ///       Gets the value of the HTML
        ///       Name attribute that will be rendered to the browser.
        ///    </para>
        /// </devdoc>
        public override string Name {
            get {
                string s = Attributes["name"];
                return ((s != null) ? s : String.Empty);
            }
            set {
                Attributes["name"] = MapStringAttributeToString(value);
            }
        }


        /// <devdoc>
        ///    <para>
        ///       Gets or sets the contents of a text box.
        ///    </para>
        /// </devdoc>
        public override string Value {
            get {
                string val = base.Value;

                if (val.Length != 0)
                    return val;

                val = ID;
                if (val != null)
                    return val;

                // if specific value is not provided, use the UniqueID
                return UniqueID;
            }
            set {
                base.Value = value;
            }
        }

        // Value that gets rendered for the Name attribute
        internal override string RenderedNameAttribute {
            get {
                // For radio buttons, we must make the name unique, but can't just use the
                // UniqueID because all buttons in a group must have the same name.  So
                // we replace the last part of the UniqueID with the group Name.
                string name = base.RenderedNameAttribute;
                string uid = UniqueID;
                int lastColon = uid.LastIndexOf(IdSeparator);
                if (lastColon >= 0)
                    name = uid.Substring(0, lastColon+1) + name;
                return name;
            }
        }


        [
        WebCategory("Action"),
        WebSysDescription(SR.Control_OnServerCheckChanged)
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
         * This method is invoked just prior to rendering.
         */

        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        protected internal override void OnPreRender(EventArgs e) {
            base.OnPreRender(e);

            if (Page != null && !Disabled) {
                Page.RegisterRequiresPostBack(this);

                Page.RegisterEnabledControl(this);
            }

            // if no change handler, no need to save posted property
            if (Events[EventServerChange] == null && !Disabled) {
                ViewState.SetItemDirty("checked",false);
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


        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        protected override void RenderAttributes(HtmlTextWriter writer) {
            if (Page != null) {
                Page.ClientScript.RegisterForEventValidation(Value, RenderedNameAttribute);
            }

            writer.WriteAttribute("value", Value);
            Attributes.Remove("value");
            base.RenderAttributes(writer);
        }

        /*
         * Method of IPostBackDataHandler interface to process posted data.
         * RadioButton determines the posted radio group state.
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
            string postValue = postCollection[RenderedNameAttribute];
            bool valueChanged = false;
            if ((postValue != null) && postValue.Equals(Value)) {
                if (Checked == false) {

                    ValidateEvent(Value, RenderedNameAttribute);

                    Checked = true;
                    // only fire change event for RadioButton that is being checked
                    valueChanged = true;
                }
            }
            else {
                if (Checked == true) {
                    Checked = false;
                }
            }

            return valueChanged;
        }

        /*
         * Method of IPostBackDataHandler interface which is invoked whenever posted data
         * for a control has changed.  RadioButton fires an OnServerChange event.
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
