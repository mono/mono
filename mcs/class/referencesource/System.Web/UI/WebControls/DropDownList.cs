//------------------------------------------------------------------------------
// <copyright file="DropDownList.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Collections.Specialized;
    using System.Drawing;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls.Adapters;

    /// <devdoc>
    ///    <para>Creates a control that allows the user to select a single item from a
    ///       drop-down list.</para>
    /// </devdoc>
    [
    SupportsEventValidation,
    ValidationProperty("SelectedItem")
    ]
    public class DropDownList : ListControl, IPostBackDataHandler {


        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.UI.WebControls.DropDownList'/> class.</para>
        /// </devdoc>
        public DropDownList() {
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [
        Browsable(false)
        ]
        public override Color BorderColor {
            get {
                return base.BorderColor;
            }
            set {
                base.BorderColor = value;
            }
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [
        Browsable(false)
        ]
        public override BorderStyle BorderStyle {
            get {
                return base.BorderStyle;
            }
            set {
                base.BorderStyle = value;
            }
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [
        Browsable(false)
        ]
        public override Unit BorderWidth {
            get {
                return base.BorderWidth;
            }
            set {
                base.BorderWidth = value;
            }
        }

        public override bool SupportsDisabledAttribute {
            get {
                return true;
            }
        }

        /// <devdoc>
        ///    <para>Gets or sets the index of the item selected by the user
        ///       from the <see cref='System.Web.UI.WebControls.DropDownList'/>
        ///       control.</para>
        /// </devdoc>
        [
        WebCategory("Behavior"),
        DefaultValue(0),
        WebSysDescription(SR.WebControl_SelectedIndex),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public override int SelectedIndex {
            get {
                int selectedIndex = base.SelectedIndex;
                if (selectedIndex < 0 && Items.Count > 0) {
                    Items[0].Selected = true;
                    selectedIndex = 0;
                }
                return selectedIndex;
            }
            set {
                base.SelectedIndex = value;
            }
        }

        internal override ArrayList SelectedIndicesInternal {
            get {
                int sideEffect = SelectedIndex;
                return base.SelectedIndicesInternal;
            }
        }


        protected override void AddAttributesToRender(HtmlTextWriter writer) {
            string uniqueID = UniqueID;
            if (uniqueID != null) {
                writer.AddAttribute(HtmlTextWriterAttribute.Name, uniqueID);
            }

            base.AddAttributesToRender(writer);
        }


        protected override ControlCollection CreateControlCollection() {
            return new EmptyControlCollection(this);
        }

        /// <internalonly/>
        /// <devdoc>
        /// <para>Process posted data for the <see cref='System.Web.UI.WebControls.DropDownList'/> control.</para>
        /// </devdoc>
        bool IPostBackDataHandler.LoadPostData(String postDataKey, NameValueCollection postCollection) {
            return LoadPostData(postDataKey, postCollection);
        }


        /// <internalonly/>
        /// <devdoc>
        /// <para>Process posted data for the <see cref='System.Web.UI.WebControls.DropDownList'/> control.</para>
        /// </devdoc>
        protected virtual bool LoadPostData(String postDataKey, NameValueCollection postCollection) {

            // When a DropDownList is disabled, then there is no postback data for it.
            // Since DropDownList doesn't call RegisterRequiresPostBack, this method will
            // never be called, so we don't need to worry about ignoring empty postback data.

            string [] selectedItems = postCollection.GetValues(postDataKey);

            EnsureDataBound();
            if (selectedItems != null) {

                ValidateEvent(postDataKey, selectedItems[0]);

                int n = Items.FindByValueInternal(selectedItems[0], false);

                if (SelectedIndex != n) {
                    SetPostDataSelection(n);
                    return true;
                }
            }

            return false;
        }


        /// <internalonly/>
        /// <devdoc>
        /// <para>Raises events for the <see cref='System.Web.UI.WebControls.DropDownList'/> control on post back.</para>
        /// </devdoc>
        void IPostBackDataHandler.RaisePostDataChangedEvent() {
            RaisePostDataChangedEvent();
        }


        /// <internalonly/>
        /// <devdoc>
        /// <para>Raises events for the <see cref='System.Web.UI.WebControls.DropDownList'/> control on post back.</para>
        /// </devdoc>
        protected virtual void RaisePostDataChangedEvent() {
            if (AutoPostBack && !Page.IsPostBackEventControlRegistered) {
                // VSWhidbey 204824
                Page.AutoPostBackControl = this;

                if (CausesValidation) {
                    Page.Validate(ValidationGroup);
                }
            }
            OnSelectedIndexChanged(EventArgs.Empty);
        }

        protected internal override void VerifyMultiSelect() {
            throw new HttpException(SR.GetString(SR.Cant_Multiselect, "DropDownList"));
        }
    }
}
