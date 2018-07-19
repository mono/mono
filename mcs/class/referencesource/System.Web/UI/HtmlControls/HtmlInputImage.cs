//------------------------------------------------------------------------------
// <copyright file="HtmlInputImage.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * HtmlInputImage.cs
 *
 * Copyright (c) 2000 Microsoft Corporation
 */

namespace System.Web.UI.HtmlControls {

    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.ComponentModel;
    using System.Web;
    using System.Web.UI;
    using System.Security.Permissions;


/// <devdoc>
///    <para>
///       The <see langword='HtmlInputImage'/> class defines the
///       methods, properties and events for the HtmlInputImage control. This class allows
///       programmatic access to the HTML &lt;input type=
///       image&gt; element on the server.
///    </para>
/// </devdoc>
    [
    DefaultEvent("ServerClick"),
    SupportsEventValidation,
    ]
    public class HtmlInputImage : HtmlInputControl,
    IPostBackDataHandler, IPostBackEventHandler {

        private static readonly object EventServerClick = new object();
        private int _x;
        private int _y;


        /*
         * Creates an intrinsic Html INPUT type=image control.
         */

        public HtmlInputImage() : base("image") {
        }


        /// <devdoc>
        ///    <para>
        ///       Gets or sets the image
        ///       alignment within the form's content flow.
        ///    </para>
        /// </devdoc>
        /*
         * Align property.
         */
        [
        WebCategory("Appearance"),
        DefaultValue(""),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public string Align {
            get {
                string s = Attributes["align"];
                return((s != null) ? s : String.Empty);
            }
            set {
                Attributes["align"] = MapStringAttributeToString(value);
            }
        }


        /// <devdoc>
        ///    <para>
        ///       Gets or sets the alternative text
        ///       that the browser should display if the image is either unavailable or has not
        ///       been downloaded yet.
        ///    </para>
        /// </devdoc>
        /*
         * Alt property.
         */
        [
        WebCategory("Appearance"),
        DefaultValue(""),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        Localizable(true)
        ]
        public string Alt {
            get {
                string s = Attributes["alt"];
                return((s != null) ? s : String.Empty);
            }
            set {
                Attributes["alt"] = MapStringAttributeToString(value);
            }
        }


        /// <devdoc>
        ///    <para>
        ///       Gets or sets the
        ///       border width, in pixels, around the image.
        ///    </para>
        /// </devdoc>
        /*
         * Border property, size of border in pixels.
         */
        [
        WebCategory("Appearance"),
        DefaultValue(-1),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public int Border {
            get {
                string s = Attributes["border"];
                return((s != null) ? Int32.Parse(s, CultureInfo.InvariantCulture) : -1);
            }
            set {
                Attributes["border"] = MapIntegerAttributeToString(value);
            }
        }


        /// <devdoc>
        ///    <para>
        ///       Gets or sets the location of
        ///       the image file relative to the page on which it is displayed.
        ///    </para>
        /// </devdoc>
        /*
         * Src property.
         */
        [
        WebCategory("Appearance"),
        DefaultValue(""),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        UrlProperty()
        ]
        public string Src {
            get {
                string s = Attributes["src"];
                return((s != null) ? s : String.Empty);
            }
            set {
                Attributes["src"] = MapStringAttributeToString(value);
            }
        }


        /// <devdoc>
        ///    <para>Gets or sets whether pressing the button causes page validation to fire. This defaults to True so that when
        ///          using validation controls, the validation state of all controls are updated when the button is clicked, both
        ///          on the client and the server. Setting this to False is useful when defining a cancel or reset button on a page
        ///          that has validators.</para>
        /// </devdoc>
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


        [
        WebCategory("Behavior"),
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
        ///    <para>
        ///       Occurs on the server when a user clicks an <see langword='HtmlInputImage'/>
        ///       control.
        ///    </para>
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.HtmlInputImage_OnServerClick)
        ]
        public event ImageClickEventHandler ServerClick {
            add {
                Events.AddHandler(EventServerClick, value);
            }
            remove {
                Events.RemoveHandler(EventServerClick, value);
            }
        }


        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        /*
         * This method is invoked just prior to rendering.
         * Register requires handling postback to determine if image has been clicked.
         */
        protected internal override void OnPreRender(EventArgs e) {
            base.OnPreRender(e);

            if (Page != null) {
                if (!Disabled)
                    Page.RegisterRequiresPostBack(this);
                if (CausesValidation)
                    Page.RegisterPostBackScript();
            }
        }



        /// <devdoc>
        ///    <para>
        ///       Raised on the server when a user clicks an <see langword='HtmlInputImage'/>
        ///       control.
        ///    </para>
        /// </devdoc>
        /*
         * Method used to raise the OnServerClick event.
         */
        protected virtual void OnServerClick(ImageClickEventArgs e) {
            ImageClickEventHandler handler = (ImageClickEventHandler)Events[EventServerClick];
            if (handler != null) handler(this, e);
        }


        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        /*
         * Method of IPostBackEventHandler interface to raise events on post back.
         * HtmlInputImage fires an OnServerClick event.
         */
        void IPostBackEventHandler.RaisePostBackEvent(string eventArgument) {
            RaisePostBackEvent(eventArgument);
        }


        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        protected virtual void RaisePostBackEvent(string eventArgument) {
            if (CausesValidation) {
                Page.Validate(ValidationGroup);
            }
            OnServerClick(new ImageClickEventArgs(_x, _y));
        }
        

        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        /*
         * Method of IPostBackDataHandler interface to process posted data.
         * The image control will check to see if the x and y values were posted,
         * which indicates that the image was clicked by the user.  The image
         * control will then register with the Page that it wants to raise an event
         * during the event processing phase.
         */
        bool IPostBackDataHandler.LoadPostData(string postDataKey, NameValueCollection postCollection) {
            return LoadPostData(postDataKey, postCollection);
        }


        protected virtual bool LoadPostData(string postDataKey, NameValueCollection postCollection) {
            string postX = postCollection[RenderedNameAttribute + ".x"];
            string postY = postCollection[RenderedNameAttribute + ".y"];

            if (postX != null && postY != null &&
                postX.Length > 0 && postY.Length > 0) {

                ValidateEvent(UniqueID);

                _x = Int32.Parse(postX, CultureInfo.InvariantCulture);
                _y = Int32.Parse(postY, CultureInfo.InvariantCulture);

                Page.RegisterRequiresRaiseEvent(this);
            }

            return false;
        }


        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        /*
         * Method of IPostBackDataHandler interface which is invoked whenever posted data
         * for a control has changed.
         */
        void IPostBackDataHandler.RaisePostDataChangedEvent() {
            RaisePostDataChangedEvent();
        }


        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        protected virtual void RaisePostDataChangedEvent() {
        }

        /*
         * Override to render unique name attribute.
         * The name attribute is owned by the framework.
         */

        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        protected override void RenderAttributes(HtmlTextWriter writer) {
            PreProcessRelativeReferenceAttribute(writer, "src");

            if (Page != null) {
                Util.WriteOnClickAttribute(
                    writer, this, true, false,
                    (CausesValidation && Page.GetValidators(ValidationGroup).Count > 0),
                    ValidationGroup);
            }

            base.RenderAttributes(writer);
        }
    }
}
