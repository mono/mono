//------------------------------------------------------------------------------
// <copyright file="ListControl.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Globalization;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.Adapters;
    using System.Web.Util;
    using System.Drawing;
    using System.Drawing.Design;


    /// <devdoc>
    ///    <para>An abstract base class. Defines the common
    ///       properties, methods, and events for all list-type controls.</para>
    /// </devdoc>
    [
    ControlValueProperty("SelectedValue"),
    DataBindingHandler("System.Web.UI.Design.WebControls.ListControlDataBindingHandler, " + AssemblyRef.SystemDesign),
    DefaultEvent("SelectedIndexChanged"),
    ParseChildren(true, "Items"),
    Designer("System.Web.UI.Design.WebControls.ListControlDesigner, " + AssemblyRef.SystemDesign)
    ]
    public abstract class ListControl : DataBoundControl, IEditableTextControl {

        private static readonly object EventSelectedIndexChanged = new object();
        private static readonly object EventTextChanged = new object();

        private ListItemCollection items;
        private int cachedSelectedIndex;
        private string cachedSelectedValue;
        private ArrayList cachedSelectedIndices;
        private bool _stateLoaded;
        private bool _asyncSelectPending;


        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.UI.WebControls.ListControl'/> class.</para>
        /// </devdoc>
        public ListControl() {
            cachedSelectedIndex = -1;
        }

        /// <devdoc>
        ///    <para> Gets or sets a value
        ///       indicating whether databound items will be added to the list of staticly-declared
        ///       items in the list.</para>
        /// </devdoc>
        [
        DefaultValue(false),
        Themeable(false),
        WebCategory("Behavior"),
        WebSysDescription(SR.ListControl_AppendDataBoundItems),
        ]
        public virtual bool AppendDataBoundItems {
            get {
                object o = ViewState["AppendDataBoundItems"];
                if (o != null) {
                    return (bool)o;
                }
                return false;
            }
            set {
                ViewState["AppendDataBoundItems"] = value;
                if (Initialized) {
                    RequiresDataBinding = true;
                }
            }
        }


        /// <devdoc>
        ///    <para> Gets or sets a value
        ///       indicating whether an automatic postback to the server will occur whenever the
        ///       user changes the selection of the list.</para>
        /// </devdoc>
        [
        DefaultValue(false),
        WebCategory("Behavior"),
        WebSysDescription(SR.ListControl_AutoPostBack),
        Themeable(false),
        ]
        public virtual bool AutoPostBack {
            get {
                object b = ViewState["AutoPostBack"];
                return((b == null) ? false : (bool)b);
            }
            set {
                ViewState["AutoPostBack"] = value;
            }
        }


        [
        DefaultValue(false),
        Themeable(false),
        WebCategory("Behavior"),
        WebSysDescription(SR.AutoPostBackControl_CausesValidation)
        ]
        public virtual bool CausesValidation {
            get {
                object b = ViewState["CausesValidation"];
                return((b == null) ? false : (bool)b);
            }
            set {
                ViewState["CausesValidation"] = value;
            }
        }


        /// <devdoc>
        ///    <para> Indicates the field of the
        ///       data source that provides the text content of the list items.</para>
        /// </devdoc>
        [
        DefaultValue(""),
        Themeable(false),
        WebCategory("Data"),
        WebSysDescription(SR.ListControl_DataTextField)
        ]
        public virtual string DataTextField {
            get {
                object s = ViewState["DataTextField"];
                return((s == null) ? String.Empty : (string)s);
            }
            set {
                ViewState["DataTextField"] = value;
                if (Initialized) {
                    RequiresDataBinding = true;
                }
            }
        }


        /// <devdoc>
        /// </devdoc>
        [
        DefaultValue(""),
        Themeable(false),
        WebCategory("Data"),
        WebSysDescription(SR.ListControl_DataTextFormatString)
        ]
        public virtual string DataTextFormatString {
            get {
                object s = ViewState["DataTextFormatString"];
                return ((s == null) ? String.Empty : (string)s);
            }
            set {
                ViewState["DataTextFormatString"] = value;
                if (Initialized) {
                    RequiresDataBinding = true;
                }
            }
        }


        /// <devdoc>
        ///    <para>Indicates the field of the data source that provides the value content of the
        ///       list items.</para>
        /// </devdoc>
        [
        DefaultValue(""),
        Themeable(false),
        WebCategory("Data"),
        WebSysDescription(SR.ListControl_DataValueField)
        ]
        public virtual string DataValueField {
            get {
                object s = ViewState["DataValueField"];
                return((s == null) ? String.Empty : (string)s);
            }
            set {
                ViewState["DataValueField"] = value;
                if (Initialized) {
                    RequiresDataBinding = true;
                }
            }
        }

        /// <devdoc>
        ///    <para>A protected property. Indicates if the ListControl supports multiple selections</para>
        /// </devdoc>
        internal virtual bool IsMultiSelectInternal {
            get  {
                return false;
            }
        }



        /// <devdoc>
        ///    <para>
        ///       Indicates the collection of items within the list.
        ///       This property
        ///       is read-only.</para>
        /// </devdoc>
        [
        WebCategory("Default"),
        DefaultValue(null),
        Editor("System.Web.UI.Design.WebControls.ListItemsCollectionEditor," + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        MergableProperty(false),
        WebSysDescription(SR.ListControl_Items),
        PersistenceMode(PersistenceMode.InnerDefaultProperty)
        ]
        public virtual ListItemCollection Items {
            get {
                if (items == null) {
                    items = new ListItemCollection();
                    if (IsTrackingViewState)
                        items.TrackViewState();
                }
                return items;
            }
        }


        /// <devdoc>
        ///    Determines whether the SelectedIndices must be stored in view state, to
        ///    optimize the size of the saved state.
        /// </devdoc>
        internal bool SaveSelectedIndicesViewState {
            get {
                // Must be saved when
                // 1. There is a registered event handler for SelectedIndexChanged or TextChanged.  
                //    For our controls, we know for sure that there is no event handler registered for 
                //    SelectedIndexChanged or TextChanged so we can short-circuit that check.
                // 2. Control is not enabled or visible, because the browser's post data will not include this control
                // 3. The instance is a derived instance, which might be overriding the OnSelectedIndexChanged method
                //    This is a bit hacky, since we have to cover all the four derived classes we have...
                // 4. AutoPostBack is true and Adapter doesn't support JavaScript
                //    For ListControls to behave the same on mobile devices
                //    that simulate AutoPostBack by rendering a command button, we need to save
                //    state.
                // 5. The control is paginated.
                // 6. The control contains items that are disabled.  The browser's post data will not
                //    include this data for disabled items, so we need to save those selected indices.
                //    

                if ((Events[EventSelectedIndexChanged] != null) ||
                    (Events[EventTextChanged] != null) ||
                    (IsEnabled == false) ||
                    (Visible == false) ||
                    (AutoPostBack == true && ((Page != null) && !Page.ClientSupportsJavaScript)) ) {
                    return true;
                }

                foreach (ListItem item in Items) {
                    if (item.Enabled == false) {
                        return true;
                    }
                }

                // Note that we added BulletedList that inherits ListControl in
                // Whidbey, but since it doesn't support selected index, we don't
                // need to check it here.
                Type t = this.GetType();
                if ((t == typeof(DropDownList)) ||
                    (t == typeof(ListBox)) ||
                    (t == typeof(CheckBoxList)) ||
                    (t == typeof(RadioButtonList))) {
                    return false;
                }

                return true;
            }
        }


        /// <devdoc>
        ///    <para>Indicates the ordinal index of the first selected item within the
        ///       list.</para>
        /// </devdoc>
        [
        Bindable(true),
        Browsable(false),
        DefaultValue(0),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        Themeable(false),
        WebCategory("Behavior"),
        WebSysDescription(SR.WebControl_SelectedIndex),
        ]
        public virtual int SelectedIndex {
            get {
                for (int i=0; i < Items.Count; i++) {
                    if (Items[i].Selected)
                        return i;
                }
                return -1;
            }
            set {
                if (value < -1) {
                    if (Items.Count == 0) {
                        // VSW 540083: If there are no items, setting SelectedIndex < -1 is the same as setting it to -1.  Don't throw.
                        value = -1;
                    }
                    else {
                        throw new ArgumentOutOfRangeException("value", SR.GetString(SR.ListControl_SelectionOutOfRange, ID, "SelectedIndex"));
                    }
                }

                if ((Items.Count != 0 && value < Items.Count) || value == -1) {
                    ClearSelection();
                    if (value >= 0) {
                        Items[value].Selected = true;
                    }
                }
                else {
                    // if we're in a postback and our state is loaded but the selection doesn't exist in the list of items,
                    // throw saying we couldn't find the selected item.
                    if (_stateLoaded) {
                        throw new ArgumentOutOfRangeException("value", SR.GetString(SR.ListControl_SelectionOutOfRange, ID, "SelectedIndex"));
                    }
                }
                // always save the selectedindex
                // When we've databound, we'll have items from viewstate on the next postback.
                // If we don't cache the selected index and reset it after we databind again,
                // the selection goes away.  So we always have to save the selectedIndex for restore
                // after databind.
                cachedSelectedIndex = value;
            }
        }


        /// <devdoc>
        ///    <para>A protected property. Gets an array of selected
        ///       indexes within the list. This property is read-only.</para>
        /// </devdoc>
        internal virtual ArrayList SelectedIndicesInternal {
            get {
                cachedSelectedIndices = new ArrayList(3);
                for (int i=0; i < Items.Count; i++) {
                    if (Items[i].Selected)  {
                        cachedSelectedIndices.Add(i);
                    }
                }
                return cachedSelectedIndices;
            }
        }



        /// <devdoc>
        ///    <para>Indicates the first selected item within the list.
        ///       This property is read-only.</para>
        /// </devdoc>
        [
        WebCategory("Behavior"),
        Browsable(false),
        DefaultValue(null),
        WebSysDescription(SR.ListControl_SelectedItem),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public virtual ListItem SelectedItem{
            get {
                int i = SelectedIndex;
                return(i < 0) ? null : Items[i];
            }
        }



        /// <devdoc>
        ///    <para>Indicates the value of the first selected item within the
        ///       list.</para>
        /// </devdoc>
        [
        Bindable(true, BindingDirection.TwoWay),
        Browsable(false),
        DefaultValue(""),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        Themeable(false),
        WebSysDescription(SR.ListControl_SelectedValue),
        WebCategory("Behavior"),
        ]
        public virtual string SelectedValue {
            get {
                int i = SelectedIndex;
                return (i < 0) ? String.Empty : Items[i].Value;
            }
            set {
                if (Items.Count != 0) {
                    // at design time, a binding on SelectedValue will be reset to the default value on OnComponentChanged
                    if (value == null || (DesignMode && value.Length == 0)) {
                        ClearSelection();
                        return;
                    }

                    ListItem selectItem = Items.FindByValue(value);
                    // if we're in a postback and our state is loaded or the page isn't a postback but all persistance is loaded
                    // but the selection doesn't exist in the list of items,
                    // throw saying we couldn't find the selected value.
                    bool loaded = Page != null && Page.IsPostBack && _stateLoaded;

                    if (loaded && selectItem == null) {
                        throw new ArgumentOutOfRangeException("value", SR.GetString(SR.ListControl_SelectionOutOfRange, ID, "SelectedValue"));
                    }

                    if (selectItem != null) {
                        ClearSelection();
                        selectItem.Selected = true;
                    }
                }
                // always save the selectedvalue
                // for later databinding in case we have viewstate items or static items
                cachedSelectedValue = value;
            }
        }

        [
        Browsable(false),
        Themeable(false),
        DefaultValue(""),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        WebSysDescription(SR.ListControl_Text),
        WebCategory("Behavior"),
        ]
        public virtual string Text {
            get {
                return SelectedValue;
            }
            set {
                SelectedValue = value;
            }
        }


        protected override HtmlTextWriterTag TagKey {
            get {
                return HtmlTextWriterTag.Select;
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
                return((s == null) ? string.Empty : s);
            }
            set {
                ViewState["ValidationGroup"] = value;
            }
        }


        /// <devdoc>
        ///    Occurs when the list selection is changed upon server
        ///    postback.
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.ListControl_OnSelectedIndexChanged)
        ]
        public event EventHandler SelectedIndexChanged {
            add {
                Events.AddHandler(EventSelectedIndexChanged, value);
            }
            remove {
                Events.RemoveHandler(EventSelectedIndexChanged, value);
            }
        }


        /// <devdoc>
        ///    <para>Occurs when the content of the text box is
        ///       changed upon server postback.</para>
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.ListControl_TextChanged)
        ]
        public event EventHandler TextChanged {
            add {
                Events.AddHandler(EventTextChanged, value);
            }
            remove {
                Events.RemoveHandler(EventTextChanged, value);
            }
        }


        protected override void AddAttributesToRender(HtmlTextWriter writer) {
            // Make sure we are in a form tag with runat=server.
            if (Page != null) {
                Page.VerifyRenderingInServerForm(this);
            }

            if (IsMultiSelectInternal)  {
                writer.AddAttribute(HtmlTextWriterAttribute.Multiple, "multiple");
            }

            if (AutoPostBack && (Page != null) && Page.ClientSupportsJavaScript) {
                string onChange = null;
                if (HasAttributes) {
                    onChange = Attributes["onchange"];
                    if (onChange != null) {
                        onChange = Util.EnsureEndWithSemiColon(onChange);
                        Attributes.Remove("onchange");
                    }
                }

                PostBackOptions options = new PostBackOptions(this, String.Empty);

                // ASURT 98368
                // Need to merge the autopostback script with the user script
                if (CausesValidation) {
                    options.PerformValidation = true;
                    options.ValidationGroup = ValidationGroup;
                }

                if (Page.Form != null) {
                    options.AutoPostBack = true;
                }

                onChange = Util.MergeScript(onChange, Page.ClientScript.GetPostBackEventReference(options, true));

                writer.AddAttribute(HtmlTextWriterAttribute.Onchange, onChange);
                if (EnableLegacyRendering) {
                    writer.AddAttribute("language", "javascript", false);
                }
            }

            if (Enabled && !IsEnabled & SupportsDisabledAttribute) {
                // We need to do the cascade effect on the server, because the browser
                // only renders as disabled, but doesn't disable the functionality.
                writer.AddAttribute(HtmlTextWriterAttribute.Disabled, "disabled");
            }

            base.AddAttributesToRender(writer);
        }


        /// <devdoc>
        ///    <para> Clears out the list selection and sets the
        ///    <see cref='System.Web.UI.WebControls.ListItem.Selected'/> property
        ///       of all items to false.</para>
        /// </devdoc>
        public virtual void ClearSelection() {
            for (int i=0; i < Items.Count; i++)
                Items[i].Selected = false;
            // Don't clear cachedSelectedIndices here because some databound controls (such as SiteMapPath)
            // call databind on all child controls when restoring from viewstate.  We need to preserve the
            // cachedSelectedIndices and restore them again for the second databinding.
        }


        /// <internalonly/>
        /// <devdoc>
        ///    Load previously saved state.
        ///    Overridden to restore selection.
        /// </devdoc>
        protected override void LoadViewState(object savedState) {
            if (savedState != null) {
                Triplet stateTriplet = (Triplet)savedState;
                base.LoadViewState(stateTriplet.First);

                // restore state of items
                Items.LoadViewState(stateTriplet.Second);

                // restore selected indices
                ArrayList selectedIndices = stateTriplet.Third as ArrayList;
                if (selectedIndices != null) {
                    SelectInternal(selectedIndices);
                }
            }
            else {
                base.LoadViewState(null);
            }

            _stateLoaded = true;
        }


        private void OnDataSourceViewSelectCallback(IEnumerable data) {
            _asyncSelectPending = false;
            PerformDataBinding(data);
            PostPerformDataBindingAction();
        }

        protected override void OnDataBinding(EventArgs e) {
            base.OnDataBinding(e);
            DataSourceView view = GetData();

            // view could be null when a user implements his own GetData().
            if (null == view) {
                throw new InvalidOperationException(SR.GetString(SR.DataControl_ViewNotFound, ID));
            }

            // DevDiv 1036362: enable async model binding for ListControl
            bool useAsyncSelect = false;
            if (AppSettings.EnableAsyncModelBinding) {
                var modelDataView = view as ModelDataSourceView;
                useAsyncSelect = modelDataView != null && modelDataView.IsSelectMethodAsync;
            }

            if (useAsyncSelect) {
                _asyncSelectPending = true; // disable post data binding action until the callback is invoked
                view.Select(SelectArguments, OnDataSourceViewSelectCallback);
            }
            else {
                IEnumerable data = view.ExecuteSelect(DataSourceSelectArguments.Empty);
                PerformDataBinding(data);
            }
        }

        internal void EnsureDataBoundInLoadPostData() {
            if (!SkipEnsureDataBoundInLoadPostData) {
                EnsureDataBound();
            }
        }

        internal bool SkipEnsureDataBoundInLoadPostData {
            get;
            set;
        }


        /// <internalonly/>
        protected internal override void OnPreRender(EventArgs e) {
            base.OnPreRender(e);
            if (Page != null && IsEnabled) {
                if (AutoPostBack) {
                    Page.RegisterPostBackScript();
                    Page.RegisterFocusScript();

                    // VSWhidbey 489577
                    if (CausesValidation && Page.GetValidators(ValidationGroup).Count > 0) {
                        Page.RegisterWebFormsScript();
                    }
                }

                if (SaveSelectedIndicesViewState == false) {
                    // Store a client-side array of enabled control, so we can re-enable them on
                    // postback (in case they are disabled client-side)
                    // Postback is needed when the SelectedIndices are not stored in view state.
                    Page.RegisterEnabledControl(this);
                }
            }
        }


        /// <devdoc>
        ///    <para> A protected method. Raises the
        ///    <see langword='SelectedIndexChanged'/> event.</para>
        /// </devdoc>
        protected virtual void OnSelectedIndexChanged(EventArgs e) {
            EventHandler onChangeHandler = (EventHandler)Events[EventSelectedIndexChanged];
            if (onChangeHandler != null) onChangeHandler(this, e);

            OnTextChanged(e);
        }

        protected virtual void OnTextChanged(EventArgs e) {
            EventHandler onChangeHandler = (EventHandler)Events[EventTextChanged];
            if (onChangeHandler != null) onChangeHandler(this,e);
        }


        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        protected internal override void PerformDataBinding(IEnumerable dataSource) {
            base.PerformDataBinding(dataSource);

            if (dataSource != null) {
                bool fieldsSpecified = false;
                bool formatSpecified = false;

                string textField = DataTextField;
                string valueField = DataValueField;
                string textFormat = DataTextFormatString;

                if (!AppendDataBoundItems) {
                    Items.Clear();
                }

                ICollection collection = dataSource as ICollection;
                if (collection != null) {
                    Items.Capacity = collection.Count + Items.Count;
                }

                if ((textField.Length != 0) || (valueField.Length != 0)) {
                    fieldsSpecified = true;
                }
                if (textFormat.Length != 0) {
                    formatSpecified = true;
                }

                foreach (object dataItem in dataSource) {
                    ListItem item = new ListItem();

                    if (fieldsSpecified) {
                        if (textField.Length > 0) {
                            item.Text = DataBinder.GetPropertyValue(dataItem, textField, textFormat);
                        }
                        if (valueField.Length > 0) {
                            item.Value = DataBinder.GetPropertyValue(dataItem, valueField, null);
                        }
                    }
                    else {
                        if (formatSpecified) {
                            item.Text = String.Format(CultureInfo.CurrentCulture, textFormat, dataItem);
                        }
                        else {
                            item.Text = dataItem.ToString();
                        }
                        item.Value = dataItem.ToString();
                    }

                    Items.Add(item);
                }
            }

            // try to apply the cached SelectedIndex and SelectedValue now
            if (cachedSelectedValue != null) {
                int cachedSelectedValueIndex = -1;

                cachedSelectedValueIndex = Items.FindByValueInternal(cachedSelectedValue, true);
                if (-1 == cachedSelectedValueIndex) {
                    throw new ArgumentOutOfRangeException("value", SR.GetString(SR.ListControl_SelectionOutOfRange, ID, "SelectedValue"));
                }

                if ((cachedSelectedIndex != -1) && (cachedSelectedIndex != cachedSelectedValueIndex)) {
                    throw new ArgumentException(SR.GetString(SR.Attributes_mutually_exclusive, "SelectedIndex", "SelectedValue"));
                }

                SelectedIndex = cachedSelectedValueIndex;
                cachedSelectedValue = null;
                cachedSelectedIndex = -1;
            }
            else {
                if (cachedSelectedIndex != -1) {
                    SelectedIndex = cachedSelectedIndex;
                    cachedSelectedIndex = -1;
                }
            }
        }

        protected override void PerformSelect() {
            // Override PerformSelect and call OnDataBinding because in V1 OnDataBinding was the function that
            // performed the databind, and we need to maintain backward compat.  OnDataBinding will retrieve the
            // data from the view synchronously and call PerformDataBinding with the data, preserving the OM.
            OnDataBinding(EventArgs.Empty);
            PostPerformDataBindingAction();
        }

        private void PostPerformDataBindingAction() {
            if (_asyncSelectPending)
                return;

            RequiresDataBinding = false;
            MarkAsDataBound();
            OnDataBound(EventArgs.Empty);
        }

        /// <devdoc>
        /// <para>This method is used by controls and adapters
        /// to render the options inside a select statement.</para>
        /// </devdoc>
        protected internal override void RenderContents(HtmlTextWriter writer) {
            ListItemCollection liCollection = Items;
            int n = liCollection.Count;

            if (n > 0) {
                bool selected = false;
                for (int i=0; i < n; i++) {
                    ListItem li = liCollection[i];

                    if (li.Enabled == false) {
                        // the only way to disable an item in a select
                        // is to hide it
                        continue;
                    }

                    writer.WriteBeginTag("option");
                    if (li.Selected) {
                        if (selected) {
                            VerifyMultiSelect();
                        }
                        selected = true;
                        writer.WriteAttribute("selected", "selected");
                    }

                    writer.WriteAttribute("value", li.Value, true /*fEncode*/);

                    // VSWhidbey 163920 Render expando attributes.
                    if (li.HasAttributes) {
                        li.Attributes.Render(writer);
                    }

                    if (Page != null) {
                        Page.ClientScript.RegisterForEventValidation(UniqueID, li.Value);
                    }

                    writer.Write(HtmlTextWriter.TagRightChar);
                    HttpUtility.HtmlEncode(li.Text, writer);
                    writer.WriteEndTag("option");
                    writer.WriteLine();
                }
            }
        }


        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        protected override object SaveViewState() {
            object baseState = base.SaveViewState();
            object items = Items.SaveViewState();
            object selectedIndicesState = null;

            if (SaveSelectedIndicesViewState) {
                selectedIndicesState = SelectedIndicesInternal;
            }

            if (selectedIndicesState != null || items != null || baseState != null) {
                return new Triplet(baseState, items, selectedIndicesState);
            }
            return null;
        }


        /// <devdoc>
        ///    Sets items within the
        ///    list to be selected according to the specified array of indexes.
        /// </devdoc>
        internal void SelectInternal(ArrayList selectedIndices) {
            ClearSelection();
            for (int i=0; i < selectedIndices.Count; i++) {
                int n = (int) selectedIndices[i];
                if (n >= 0 && n < Items.Count)
                    Items[n].Selected = true;
            }
            cachedSelectedIndices = selectedIndices;
        }

        internal static void SetControlToRepeatID(Control owner, Control controlToRepeat, int index) {
            string idSuffix = index.ToString(NumberFormatInfo.InvariantInfo);
            if (owner.EffectiveClientIDMode == ClientIDMode.Static) {
                if (String.IsNullOrEmpty(owner.ID)) {
                    // When IDMode=Static but has no ID, what should the item IDs be? Reverting to AutoID behavior.
                    controlToRepeat.ID = idSuffix;
                    controlToRepeat.ClientIDMode = ClientIDMode.AutoID;
                }
                else {
                    controlToRepeat.ID = owner.ID + "_" + idSuffix;
                    controlToRepeat.ClientIDMode = ClientIDMode.Inherit;
                }
            }
            else {
                controlToRepeat.ID = idSuffix;
                controlToRepeat.ClientIDMode = ClientIDMode.Inherit;
            }
        }

        /// <devdoc>
        ///    Sets items within the list to be selected from post data.
        ///    The difference is that these items won't be cached and reset after a databind.
        /// </devdoc>
        protected void SetPostDataSelection(int selectedIndex) {
            if (Items.Count != 0) {
                if (selectedIndex < Items.Count) {
                    ClearSelection();
                    if (selectedIndex >= 0) {
                        Items[selectedIndex].Selected = true;
                    }
                }
            }
        }


        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        protected override void TrackViewState() {
            base.TrackViewState();
            Items.TrackViewState();
        }


        protected internal virtual void VerifyMultiSelect() {
            if (!IsMultiSelectInternal) {
                throw new HttpException(SR.GetString(SR.Cant_Multiselect_In_Single_Mode));
            }
        }
    }
}
