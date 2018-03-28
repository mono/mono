//------------------------------------------------------------------------------
// <copyright file="DataControlField.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Drawing.Design;

    /// <devdoc>
    /// Creates a field and is the base class for all <see cref='System.Web.UI.WebControls.DataControlField'/> types.
    /// </devdoc>
    [
    TypeConverterAttribute(typeof(ExpandableObjectConverter)),
    DefaultProperty("HeaderText")
    ]
    public abstract class DataControlField : IStateManager, IDataSourceViewSchemaAccessor {

        private TableItemStyle _itemStyle;
        private TableItemStyle _headerStyle;
        private TableItemStyle _footerStyle;
        private Style _controlStyle;
        private StateBag _statebag;
        private bool _trackViewState;
        private bool _sortingEnabled;
        private Control _control;
        private object _dataSourceViewSchema;

        internal event EventHandler FieldChanged;



        /// <devdoc>
        /// <para>Initializes a new instance of the System.Web.UI.WebControls.Field class.</para>
        /// </devdoc>
        protected DataControlField() {
            _statebag = new StateBag();
            _dataSourceViewSchema = null;
        }


        /// <devdoc>
        /// <para>Gets or sets the text rendered as the AbbreviatedText in some controls.</para>
        /// </devdoc>
        [
        Localizable(true),
        WebCategory("Accessibility"),
        DefaultValue(""),
        WebSysDescription(SR.DataControlField_AccessibleHeaderText)
        ]
        public virtual string AccessibleHeaderText {
            get {
                object o = ViewState["AccessibleHeaderText"];
                if (o != null)
                    return(string)o;
                return String.Empty;
            }
            set {
                if (!String.Equals(value, ViewState["AccessibleHeaderText"])) {
                    ViewState["AccessibleHeaderText"] = value;
                    OnFieldChanged();
                }
            }
        }


        /// <devdoc>
        /// <para>Gets the style properties for the controls inside this field.</para>
        /// </devdoc>
        [
        WebCategory("Styles"),
        DefaultValue(null),
        WebSysDescription(SR.DataControlField_ControlStyle),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        PersistenceMode(PersistenceMode.InnerProperty)
        ]
        public Style ControlStyle {
            get {
                if (_controlStyle == null) {
                    _controlStyle = new Style();
                    if (IsTrackingViewState)
                        ((IStateManager)_controlStyle).TrackViewState();
                }
                return _controlStyle;
            }
        }

        /// <summary>
        /// This property is accessed by <see cref='System.Web.UI.WebControls.DataControlFieldCell'/>.
        /// Any child classes which define <see cref='System.Web.UI.Control.ValidateRequestMode'/> should be wrapping this so that
        /// <see cref='System.Web.UI.WebControls.DataControlFieldCell'/> gets the correct value.
        /// </summary>
        protected internal virtual ValidateRequestMode ValidateRequestMode {
            get {
                object o = ViewState["ValidateRequestMode"];
                if (o != null)
                    return (ValidateRequestMode)o;
                return ValidateRequestMode.Inherit;
            }
            set {
                if (value < ValidateRequestMode.Inherit || value > ValidateRequestMode.Enabled) {
                    throw new ArgumentOutOfRangeException("value");
                }
                if (value != ValidateRequestMode) {
                    ViewState["ValidateRequestMode"] = value;
                    OnFieldChanged();
                }
            }
        }

        internal Style ControlStyleInternal {
            get {
                return _controlStyle;
            }
        }

        protected Control Control {
            get {
                return _control;
            }
        }


        /// <devdoc>
        /// <para>[To be supplied.]</para>
        /// </devdoc>
        protected bool DesignMode {
            get {
                if (_control != null) {
                    return _control.DesignMode;
                }
                return false;
            }
        }


        /// <devdoc>
        /// <para>Gets the style properties for the footer item.</para>
        /// </devdoc>
        [
        WebCategory("Styles"),
        DefaultValue(null),
        WebSysDescription(SR.DataControlField_FooterStyle),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        PersistenceMode(PersistenceMode.InnerProperty)
        ]
        public TableItemStyle FooterStyle {
            get {
                if (_footerStyle == null) {
                    _footerStyle = new TableItemStyle();
                    if (IsTrackingViewState)
                        ((IStateManager)_footerStyle).TrackViewState();
                }
                return _footerStyle;
            }
        }

        /// <devdoc>
        /// </devdoc>
        internal TableItemStyle FooterStyleInternal {
            get {
                return _footerStyle;
            }
        }


        /// <devdoc>
        /// <para> Gets or sets the text displayed in the footer of the
        /// System.Web.UI.WebControls.Field.</para>
        /// </devdoc>
        [
        Localizable(true),
        WebCategory("Appearance"),
        DefaultValue(""),
        WebSysDescription(SR.DataControlField_FooterText)
        ]
        public virtual string FooterText {
            get {
                object o = ViewState["FooterText"];
                if (o != null)
                    return(string)o;
                return String.Empty;
            }
            set {
                if (!String.Equals(value, ViewState["FooterText"])) {
                    ViewState["FooterText"] = value;
                    OnFieldChanged();
                }
            }
        }


        /// <devdoc>
        /// <para>Gets or sets the URL reference to an image to display
        /// instead of text on the header of this System.Web.UI.WebControls.Field
        /// .</para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(""),
        Editor("System.Web.UI.Design.ImageUrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        UrlProperty(),
        WebSysDescription(SR.DataControlField_HeaderImageUrl)
        ]
        public virtual string HeaderImageUrl {
            get {
                object o = ViewState["HeaderImageUrl"];
                if (o != null)
                    return(string)o;
                return String.Empty;
            }
            set {
                if (!String.Equals(value, ViewState["HeaderImageUrl"])) {
                    ViewState["HeaderImageUrl"] = value;
                    OnFieldChanged();
                }
            }
        }


        /// <devdoc>
        /// <para>Gets the style properties for the header of the System.Web.UI.WebControls.Field. This property is read-only.</para>
        /// </devdoc>
        [
        WebCategory("Styles"),
        DefaultValue(null),
        WebSysDescription(SR.DataControlField_HeaderStyle),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        PersistenceMode(PersistenceMode.InnerProperty)
        ]
        public TableItemStyle HeaderStyle {
            get {
                if (_headerStyle == null) {
                    _headerStyle = new TableItemStyle();
                    if (IsTrackingViewState)
                        ((IStateManager)_headerStyle).TrackViewState();
                }
                return _headerStyle;
            }
        }

        /// <devdoc>
        /// </devdoc>
        internal TableItemStyle HeaderStyleInternal {
            get {
                return _headerStyle;
            }
        }


        /// <devdoc>
        /// <para>Gets or sets the text displayed in the header of the
        /// System.Web.UI.WebControls.Field.</para>
        /// </devdoc>
        [
        Localizable(true),
        WebCategory("Appearance"),
        DefaultValue(""),
        WebSysDescription(SR.DataControlField_HeaderText)
        ]
        public virtual string HeaderText {
            get {
                object o = ViewState["HeaderText"];
                if (o != null)
                    return(string)o;
                return String.Empty;
            }
            set {
                if (!String.Equals(value, ViewState["HeaderText"])) {
                    ViewState["HeaderText"] = value;
                    OnFieldChanged();
                }
            }
        }

        /// <devdoc>
        ///    <para>Gets or sets whether the field is visible in Insert mode.  Turn off for auto-gen'd db fields</para>
        /// </devdoc>
        [
            WebCategory("Behavior"),
            DefaultValue(true),
            WebSysDescription(SR.DataControlField_InsertVisible)
        ]
        public virtual bool InsertVisible {
            get {
                object o = ViewState["InsertVisible"];
                if (o != null)
                    return (bool)o;
                return true;
            }
            set {
                object oldValue = ViewState["InsertVisible"];
                if (oldValue == null || value != (bool)oldValue) {
                    ViewState["InsertVisible"] = value;
                    OnFieldChanged();
                }
            }
        }
        

        /// <devdoc>
        /// <para>Gets the style properties of an item within the System.Web.UI.WebControls.Field. This property is read-only.</para>
        /// </devdoc>
        [
        WebCategory("Styles"),
        DefaultValue(null),
        WebSysDescription(SR.DataControlField_ItemStyle),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        PersistenceMode(PersistenceMode.InnerProperty)
        ]
        public TableItemStyle ItemStyle {
            get {
                if (_itemStyle == null) {
                    _itemStyle = new TableItemStyle();
                    if (IsTrackingViewState)
                        ((IStateManager)_itemStyle).TrackViewState();
                }
                return _itemStyle;
            }
        }

        /// <devdoc>
        /// </devdoc>
        internal TableItemStyle ItemStyleInternal {
            get {
                return _itemStyle;
            }
        }


        [
        WebCategory("Behavior"),
        DefaultValue(true),
        WebSysDescription(SR.DataControlField_ShowHeader)
        ]
        public virtual bool ShowHeader {
            get {
                object o = ViewState["ShowHeader"];
                if (o != null) {
                    return (bool)o;
                }
                return true;
            }
            set {
                object oldValue = ViewState["ShowHeader"];
                if (oldValue == null || (bool)oldValue != value) {
                    ViewState["ShowHeader"] = value;
                    OnFieldChanged();
                }
            }
        }


        /// <devdoc>
        /// <para>Gets or sets the expression used when this field is used to sort the data source> by.</para>
        /// </devdoc>
        [
        WebCategory("Behavior"),
        DefaultValue(""),
        TypeConverter("System.Web.UI.Design.DataSourceViewSchemaConverter, " + AssemblyRef.SystemDesign),
        WebSysDescription(SR.DataControlField_SortExpression)
        ]
        public virtual string SortExpression {
            get {
                object o = ViewState["SortExpression"];
                if (o != null)
                    return(string)o;
                return String.Empty;
            }
            set {
                if (!String.Equals(value, ViewState["SortExpression"])) {
                    ViewState["SortExpression"] = value;
                    OnFieldChanged();
                }
            }
        }


        /// <devdoc>
        /// <para>Gets the statebag for the System.Web.UI.WebControls.Field. This property is read-only.</para>
        /// </devdoc>
        protected StateBag ViewState {
            get {
                return _statebag;
            }
        }


        /// <devdoc>
        /// <para>Gets or sets a value to indicate whether the System.Web.UI.WebControls.Field is visible.</para>
        /// </devdoc>
        [
        WebCategory("Behavior"),
        DefaultValue(true),
        WebSysDescription(SR.DataControlField_Visible)
        ]
        public bool Visible {
            get {
                object o = ViewState["Visible"];
                if (o != null)
                    return(bool)o;
                return true;
            }
            set {
                object oldValue = ViewState["Visible"];
                if (oldValue == null || value != (bool)oldValue) {
                    ViewState["Visible"] = value;
                    OnFieldChanged();
                }
            }
        }

        protected internal DataControlField CloneField() {
            DataControlField newField = CreateField();
            CopyProperties(newField);
            return newField;
        }

        protected virtual void CopyProperties(DataControlField newField) {
            newField.AccessibleHeaderText = AccessibleHeaderText;
            newField.ControlStyle.CopyFrom(ControlStyle);
            newField.FooterStyle.CopyFrom(FooterStyle);
            newField.HeaderStyle.CopyFrom(HeaderStyle);
            newField.ItemStyle.CopyFrom(ItemStyle);
            newField.FooterText = FooterText;
            newField.HeaderImageUrl = HeaderImageUrl;
            newField.HeaderText = HeaderText;
            newField.InsertVisible = InsertVisible;
            newField.ShowHeader = ShowHeader;
            newField.SortExpression = SortExpression;
            newField.Visible = Visible;
            newField.ValidateRequestMode = ValidateRequestMode;
        }

        protected abstract DataControlField CreateField();


        /// <devdoc>
        /// Extracts the value of the databound cell and inserts the value into the given dictionary
        /// </devdoc>
        public virtual void ExtractValuesFromCell(IOrderedDictionary dictionary, DataControlFieldCell cell, DataControlRowState rowState, bool includeReadOnly) {
            return;
        }


        /// <devdoc>
        /// </devdoc>
        public virtual bool Initialize(bool sortingEnabled, Control control) {
            _sortingEnabled = sortingEnabled;
            _control = control;
            return false;
        }


        /// <devdoc>
        /// <para>Initializes a cell in the System.Web.UI.WebControls.Field.</para>
        /// </devdoc>
        public virtual void InitializeCell(DataControlFieldCell cell, DataControlCellType cellType, DataControlRowState rowState, int rowIndex) {
            switch (cellType) {
                case DataControlCellType.Header:
                    {
                        WebControl headerControl = null;
                        string sortExpression = SortExpression;
                        bool sortableHeader = (_sortingEnabled && sortExpression.Length > 0);

                        string headerImageUrl = HeaderImageUrl;
                        string headerText = HeaderText;
                        if (headerImageUrl.Length != 0) {
                            if (sortableHeader) {
                                ImageButton sortButton;
                                IPostBackContainer container = _control as IPostBackContainer;
                                if (container != null) {
                                    sortButton = new DataControlImageButton(container);
                                    ((DataControlImageButton)sortButton).EnableCallback(null);  // no command argument for the callback uses Sort
                                }
                                else {
                                    sortButton = new ImageButton();
                                }

                                sortButton.ImageUrl = HeaderImageUrl;
                                sortButton.CommandName = DataControlCommands.SortCommandName;
                                sortButton.CommandArgument = sortExpression;
                                if (!(sortButton is DataControlImageButton)) {
                                    sortButton.CausesValidation = false;
                                }
                                sortButton.AlternateText = headerText;
                                headerControl = sortButton;
                            }
                            else {
                                Image headerImage = new Image();

                                headerImage.ImageUrl = headerImageUrl;
                                headerControl = headerImage;
                                headerImage.AlternateText = headerText;
                            }
                        }
                        else {
                            if (sortableHeader) {
                                LinkButton sortButton;
                                IPostBackContainer container = _control as IPostBackContainer;
                                if (container != null) {
                                    sortButton = new DataControlLinkButton(container);
                                    ((DataControlLinkButton)sortButton).EnableCallback(null);   // no command argument for the callback uses Sort
                                }
                                else {
                                    sortButton = new LinkButton();
                                }

                                sortButton.Text = headerText;
                                sortButton.CommandName = DataControlCommands.SortCommandName;
                                sortButton.CommandArgument = sortExpression;
                                if (!(sortButton is DataControlLinkButton)) {
                                    sortButton.CausesValidation = false;
                                }
                                headerControl = sortButton;
                            }
                            else {
                                if (headerText.Length == 0) {
                                    // the browser does not render table borders for cells with nothing
                                    // in their content, so we add a non-breaking space.
                                    headerText = "&nbsp;";
                                }
                                cell.Text = headerText;
                            }
                        }

                        if (headerControl != null) {
                            cell.Controls.Add(headerControl);
                        }
                    }
                    break;

                case DataControlCellType.Footer:
                    {
                        string footerText = FooterText;
                        if (footerText.Length == 0) {
                            // the browser does not render table borders for cells with nothing
                            // in their content, so we add a non-breaking space.
                            footerText = "&nbsp;";
                        }

                        cell.Text = footerText;
                    }
                    break;
            }
        }


        /// <devdoc>
        /// <para>Determines if the System.Web.UI.WebControls.Field is marked to save its state.</para>
        /// </devdoc>
        protected bool IsTrackingViewState {
            get {
                return _trackViewState;
            }
        }


        /// <devdoc>
        /// <para>Loads the state of the System.Web.UI.WebControls.Field.</para>
        /// </devdoc>
        protected virtual void LoadViewState(object savedState) {
            if (savedState != null) {
                object[] myState = (object[])savedState;

                if (myState[0] != null)
                    ((IStateManager)ViewState).LoadViewState(myState[0]);
                if (myState[1] != null)
                    ((IStateManager)ItemStyle).LoadViewState(myState[1]);
                if (myState[2] != null)
                    ((IStateManager)HeaderStyle).LoadViewState(myState[2]);
                if (myState[3] != null)
                    ((IStateManager)FooterStyle).LoadViewState(myState[3]);
            }
        }


        /// <devdoc>
        /// <para>Raises the FieldChanged event for a System.Web.UI.WebControls.Field.</para>
        /// </devdoc>
        protected virtual void OnFieldChanged() {
            if (FieldChanged != null) {
                FieldChanged(this, EventArgs.Empty);
            }
        }


        /// <devdoc>
        /// <para>Saves the current state of the System.Web.UI.WebControls.Field.</para>
        /// </devdoc>
        protected virtual object SaveViewState() {
            object propState = ((IStateManager)ViewState).SaveViewState();
            object itemStyleState = (_itemStyle != null) ? ((IStateManager)_itemStyle).SaveViewState() : null;
            object headerStyleState = (_headerStyle != null) ? ((IStateManager)_headerStyle).SaveViewState() : null;
            object footerStyleState = (_footerStyle != null) ? ((IStateManager)_footerStyle).SaveViewState() : null;
            object controlStyleState = (_controlStyle != null) ? ((IStateManager)_controlStyle).SaveViewState() : null;

            if ((propState != null) ||
                (itemStyleState != null) ||
                (headerStyleState != null) ||
                (footerStyleState != null) ||
                (controlStyleState != null)) {
                return new object[5] {
                    propState,
                    itemStyleState,
                    headerStyleState,
                    footerStyleState,
                    controlStyleState
                };
            }

            return null;
        }

        internal void SetDirty() {
            _statebag.SetDirty(true);
            if (_itemStyle != null) {
                _itemStyle.SetDirty();
            }
            if (_headerStyle != null) {
                _headerStyle.SetDirty();
            }
            if (_footerStyle != null) {
                _footerStyle.SetDirty();
            }
            if (_controlStyle != null) {
                _controlStyle.SetDirty();
            }
        }


        /// <internalonly/>
        /// <devdoc>
        /// Return a textual representation of the column for UI-display purposes.
        /// </devdoc>
        public override string ToString() {
            string headerText = HeaderText.Trim();
            return headerText.Length > 0 ? headerText : GetType().Name;
        }


        /// <devdoc>
        /// <para>Marks the starting point to begin tracking and saving changes to the
        /// control as part of the control viewstate.</para>
        /// </devdoc>
        protected virtual void TrackViewState() {
            _trackViewState = true;
            ((IStateManager)ViewState).TrackViewState();
            if (_itemStyle != null)
                ((IStateManager)_itemStyle).TrackViewState();
            if (_headerStyle != null)
                ((IStateManager)_headerStyle).TrackViewState();
            if (_footerStyle != null)
                ((IStateManager)_footerStyle).TrackViewState();
            if (_controlStyle != null)
                ((IStateManager)_controlStyle).TrackViewState();
        }

        /// <devdoc>
        /// <para>Override with an empty body if the field's controls all support callback.
        ///  Otherwise, override and throw a useful error message about why the field can't support callbacks.</para>
        /// </devdoc>
        public virtual void ValidateSupportsCallback() {
            throw new NotSupportedException(SR.GetString(SR.DataControlField_CallbacksNotSupported, Control.ID));
        }


        /// <internalonly/>
        /// <devdoc>
        /// Return true if tracking state changes.
        /// </devdoc>
        bool IStateManager.IsTrackingViewState {
            get {
                return IsTrackingViewState;
            }
        }


        /// <internalonly/>
        /// <devdoc>
        /// Load previously saved state.
        /// </devdoc>
        void IStateManager.LoadViewState(object state) {
            LoadViewState(state);
        }


        /// <internalonly/>
        /// <devdoc>
        /// Start tracking state changes.
        /// </devdoc>
        void IStateManager.TrackViewState() {
            TrackViewState();
        }


        /// <internalonly/>
        /// <devdoc>
        /// Return object containing state changes.
        /// </devdoc>
        object IStateManager.SaveViewState() {
            return SaveViewState();
        }

        #region IDataSourceViewSchemaAccessor implementation

        /// <internalonly/>
        object IDataSourceViewSchemaAccessor.DataSourceViewSchema {
            get {
                return _dataSourceViewSchema;
            }
            set {
                _dataSourceViewSchema = value;
            }
        }
        #endregion
    }
}

