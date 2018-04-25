//------------------------------------------------------------------------------
// <copyright file="Listbox.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Drawing;
    using System.Globalization;
    using System.Web;
    using System.Web.UI;

    /// <devdoc>
    ///    <para>Constructs a list box and defines its
    ///       properties.</para>
    /// </devdoc>
    [
    ValidationProperty("SelectedItem"),
    SupportsEventValidation
    ]
    public class ListBox : ListControl, IPostBackDataHandler {


        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.UI.WebControls.ListBox'/> class.</para>
        /// </devdoc>
        public ListBox() {
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

        internal override bool IsMultiSelectInternal  {
            get  {
                return SelectionMode == ListSelectionMode.Multiple;
            }
        }


        /// <devdoc>
        ///    <para> Gets or
        ///       sets the display height (in rows) of the list box.</para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(4),
        WebSysDescription(SR.ListBox_Rows)
        ]
        public virtual int Rows {
            get {
                object n = ViewState["Rows"];
                return((n == null) ? 4 : (int)n);
            }
            set {
                if (value < 1) {
                    throw new ArgumentOutOfRangeException("value");
                }
                ViewState["Rows"] = value;
            }
        }


        /// <devdoc>
        ///    <para>Gets or sets
        ///       the selection behavior of the list box.</para>
        /// </devdoc>
        [
        WebCategory("Behavior"),
        DefaultValue(ListSelectionMode.Single),
        WebSysDescription(SR.ListBox_SelectionMode)
        ]
        public virtual ListSelectionMode SelectionMode {
            get {
                object sm = ViewState["SelectionMode"];
                return((sm == null) ? ListSelectionMode.Single : (ListSelectionMode)sm);
            }
            set {
                if (value < ListSelectionMode.Single || value > ListSelectionMode.Multiple) {
                    throw new ArgumentOutOfRangeException("value");
                }
                ViewState["SelectionMode"] = value;
            }
        }


        /// <internalonly/>
        protected override void AddAttributesToRender(HtmlTextWriter writer) {
            writer.AddAttribute(HtmlTextWriterAttribute.Size,
                                        Rows.ToString(NumberFormatInfo.InvariantInfo));

            string uniqueID = UniqueID;
            if (uniqueID != null) {
                writer.AddAttribute(HtmlTextWriterAttribute.Name, uniqueID);
            }

            base.AddAttributesToRender(writer);
        }


        public virtual int[] GetSelectedIndices() {
            return (int[])SelectedIndicesInternal.ToArray(typeof(int));
        }


        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        protected internal override void OnPreRender(EventArgs e) {
            base.OnPreRender(e);
            if (Page != null && SelectionMode == ListSelectionMode.Multiple && Enabled) {

                // ensure postback when no item is selected
                Page.RegisterRequiresPostBack(this);
            }
        }


        /// <internalonly/>
        /// <devdoc>
        /// <para>Loads the posted content of the list control if it is different from the last
        /// posting.</para>
        /// </devdoc>
        bool IPostBackDataHandler.LoadPostData(String postDataKey, NameValueCollection postCollection) {
            return LoadPostData(postDataKey, postCollection);
        }


        /// <internalonly/>
        /// <devdoc>
        /// <para>Loads the posted content of the list control if it is different from the last
        /// posting.</para>
        /// </devdoc>
        protected virtual bool LoadPostData(String postDataKey, NameValueCollection postCollection) {
            if (IsEnabled == false) {
                // When a ListBox is disabled, then there is no postback
                // data for it. Any checked state information has been loaded
                // via view state.
                return false;
            }

            string[] selectedItems = postCollection.GetValues(postDataKey);
            bool selectionChanged = false;

            EnsureDataBoundInLoadPostData();
            if (selectedItems != null) {
                if (SelectionMode == ListSelectionMode.Single) {

                    ValidateEvent(postDataKey, selectedItems[0]);

                    int n = Items.FindByValueInternal(selectedItems[0], false);
                    if (SelectedIndex != n) {
                        SetPostDataSelection(n);
                        selectionChanged = true;
                    }
                }
                else { // multiple selection
                    int count = selectedItems.Length;
                    ArrayList oldSelectedIndices = SelectedIndicesInternal;
                    ArrayList newSelectedIndices = new ArrayList(count);
                    for (int i=0; i < count; i++) {

                        ValidateEvent(postDataKey, selectedItems[i]);

                        // create array of new indices from posted values
                        newSelectedIndices.Add(Items.FindByValueInternal(selectedItems[i], false));
                    }

                    int oldcount = 0;
                    if (oldSelectedIndices != null)
                        oldcount = oldSelectedIndices.Count;

                    if (oldcount == count) {
                        // check new indices against old indices
                        // assumes selected values are posted in order
                        for (int i=0; i < count; i++) {
                            if (((int)newSelectedIndices[i]) != ((int)oldSelectedIndices[i])) {
                                selectionChanged = true;
                                break;
                            }
                        }
                    }
                    else {
                        // indices must have changed if count is different
                        selectionChanged = true;
                    }

                    if (selectionChanged) {
                        // select new indices
                        SelectInternal(newSelectedIndices);
                    }
                }
            }
            else { // no items selected
                if (SelectedIndex != -1) {
                    SetPostDataSelection(-1);
                    selectionChanged = true;
                }
            }

            return selectionChanged;
        }


        /// <internalonly/>
        /// <devdoc>
        /// <para>Invokes the OnSelectedIndexChanged method whenever posted data
        /// for the <see cref='System.Web.UI.WebControls.ListBox'/> control has changed.</para>
        /// </devdoc>
        void IPostBackDataHandler.RaisePostDataChangedEvent() {
            RaisePostDataChangedEvent();
        }


        /// <internalonly/>
        /// <devdoc>
        /// <para>Invokes the OnSelectedIndexChanged method whenever posted data
        /// for the <see cref='System.Web.UI.WebControls.ListBox'/> control has changed.</para>
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
    }
}
