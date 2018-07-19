//------------------------------------------------------------------------------
// <copyright file="Column.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.Collections;    
    using System.ComponentModel;
    using System.Web;
    using System.Web.UI;

    /// <devdoc>
    ///    Creates a column and is the base class for all <see cref='System.Web.UI.WebControls.DataGrid'/> column types.
    /// </devdoc>
    [
    TypeConverterAttribute(typeof(ExpandableObjectConverter))
    ]
    public abstract class DataGridColumn : IStateManager {

        private DataGrid owner;
        private TableItemStyle itemStyle;
        private TableItemStyle headerStyle;
        private TableItemStyle footerStyle;
        private StateBag statebag;
        private bool marked;



        /// <devdoc>
        /// <para>Initializes a new instance of the System.Web.UI.WebControls.Column class.</para>
        /// </devdoc>
        protected DataGridColumn() {
            statebag = new StateBag();
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected bool DesignMode {
            get {
                if (owner != null) {
                    return owner.DesignMode;
                }
                return false;
            }
        }
        

        /// <devdoc>
        ///    <para>Gets the style properties for the footer item.</para>
        /// </devdoc>
        [
        WebCategory("Styles"),
        DefaultValue(null),
        WebSysDescription(SR.DataGridColumn_FooterStyle),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        PersistenceMode(PersistenceMode.InnerProperty)
        ]
        public virtual TableItemStyle FooterStyle {
            get {
                if (footerStyle == null) {
                    footerStyle = new TableItemStyle();
                    if (IsTrackingViewState)
                        ((IStateManager)footerStyle).TrackViewState();
                }
                return footerStyle;
            }
        }


        /// <devdoc>
        /// </devdoc>
        internal TableItemStyle FooterStyleInternal {
            get {
                return footerStyle;
            }
        }


        /// <devdoc>
        ///    <para> Gets or sets the text displayed in the footer of the 
        ///    System.Web.UI.WebControls.Column.</para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(""),
        WebSysDescription(SR.DataGridColumn_FooterText)
        ]
        public virtual string FooterText {
            get {
                object o = ViewState["FooterText"];
                if (o != null)
                    return(string)o;
                return String.Empty;
            }
            set {
                ViewState["FooterText"] = value;
                OnColumnChanged();
            }
        }


        /// <devdoc>
        ///    <para>Gets or sets the URL reference to an image to display 
        ///       instead of text on the header of this System.Web.UI.WebControls.Column
        ///       .</para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(""),
        UrlProperty(),
        WebSysDescription(SR.DataGridColumn_HeaderImageUrl)
        ]
        public virtual string HeaderImageUrl {
            get {
                object o = ViewState["HeaderImageUrl"];
                if (o != null)
                    return(string)o;
                return String.Empty;
            }
            set {
                ViewState["HeaderImageUrl"] = value;
                OnColumnChanged();
            }
        }


        /// <devdoc>
        /// <para>Gets the style properties for the header of the System.Web.UI.WebControls.Column. This property is read-only.</para>
        /// </devdoc>
        [
        WebCategory("Styles"),
        DefaultValue(null),
        WebSysDescription(SR.DataGridColumn_HeaderStyle),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        PersistenceMode(PersistenceMode.InnerProperty)
        ]
        public virtual TableItemStyle HeaderStyle {
            get {
                if (headerStyle == null) {
                    headerStyle = new TableItemStyle();
                    if (IsTrackingViewState)
                        ((IStateManager)headerStyle).TrackViewState();
                }
                return headerStyle;
            }
        }


        /// <devdoc>
        /// </devdoc>
        internal TableItemStyle HeaderStyleInternal {
            get {
                return headerStyle;
            }
        }


        /// <devdoc>
        ///    <para>Gets or sets the text displayed in the header of the 
        ///    System.Web.UI.WebControls.Column.</para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(""),
        WebSysDescription(SR.DataGridColumn_HeaderText)
        ]
        public virtual string HeaderText {
            get {
                object o = ViewState["HeaderText"];
                if (o != null)
                    return(string)o;
                return String.Empty;
            }
            set {
                ViewState["HeaderText"] = value;
                OnColumnChanged();
            }
        }


        /// <devdoc>
        /// <para>Gets the style properties of an item within the System.Web.UI.WebControls.Column. This property is read-only.</para>
        /// </devdoc>
        [
        WebCategory("Styles"),
        DefaultValue(null),
        WebSysDescription(SR.DataGridColumn_ItemStyle),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        PersistenceMode(PersistenceMode.InnerProperty)
        ]
        public virtual TableItemStyle ItemStyle {
            get {
                if (itemStyle == null) {
                    itemStyle = new TableItemStyle();
                    if (IsTrackingViewState)
                        ((IStateManager)itemStyle).TrackViewState();
                }
                return itemStyle;
            }
        }


        /// <devdoc>
        /// </devdoc>
        internal TableItemStyle ItemStyleInternal {
            get {
                return itemStyle;
            }
        }


        /// <devdoc>
        /// <para>Gets the System.Web.UI.WebControls.DataGrid that the System.Web.UI.WebControls.Column is a part of. This property is read-only.</para>
        /// </devdoc>
        protected DataGrid Owner {
            get {
                return owner;
            }
        }


        /// <devdoc>
        /// <para>Gets or sets the expression used when this column is used to sort the data source> by.</para>
        /// </devdoc>
        [
        WebCategory("Behavior"),
        DefaultValue(""),
        WebSysDescription(SR.DataGridColumn_SortExpression)
        ]
        public virtual string SortExpression {
            get {
                object o = ViewState["SortExpression"];
                if (o != null)
                    return(string)o;
                return String.Empty;
            }
            set {
                ViewState["SortExpression"] = value;
                OnColumnChanged();
            }
        }


        /// <devdoc>
        /// <para>Gets the statebag for the System.Web.UI.WebControls.Column. This property is read-only.</para>
        /// </devdoc>
        protected StateBag ViewState {
            get {
                return statebag;
            }
        }


        /// <devdoc>
        /// <para>Gets or sets a value to indicate whether the System.Web.UI.WebControls.Column is visible.</para>
        /// </devdoc>
        [
        WebCategory("Behavior"),
        DefaultValue(true),
        WebSysDescription(SR.DataGridColumn_Visible)
        ]
        public bool Visible {
            get {
                object o = ViewState["Visible"];
                if (o != null)
                    return(bool)o;
                return true;
            }
            set {
                ViewState["Visible"] = value;
                OnColumnChanged();
            }
        }


        /// <devdoc>
        /// </devdoc>
        public virtual void Initialize() {
        }


        /// <devdoc>
        /// <para>Initializes a cell in the System.Web.UI.WebControls.Column.</para>
        /// </devdoc>
        public virtual void InitializeCell(TableCell cell, int columnIndex, ListItemType itemType) {
            switch (itemType) {
                case ListItemType.Header:
                    {
                        WebControl headerControl = null;
                        bool sortableHeader = true;
                        string sortExpression = null;

                        if ((owner != null) && (owner.AllowSorting == false)) {
                            sortableHeader = false;
                        }
                        if (sortableHeader) {
                            sortExpression = SortExpression;
                            if (sortExpression.Length == 0)
                                sortableHeader = false;
                        }

                        string headerImageUrl = HeaderImageUrl;
                        if (headerImageUrl.Length != 0) {
                            if (sortableHeader) {
                                ImageButton sortButton = new ImageButton();

                                sortButton.ImageUrl = HeaderImageUrl;
                                sortButton.CommandName = DataGrid.SortCommandName;
                                sortButton.CommandArgument = sortExpression;
                                sortButton.CausesValidation = false;
                                headerControl = sortButton;
                            }
                            else {
                                Image headerImage = new Image();

                                headerImage.ImageUrl = headerImageUrl;
                                headerControl = headerImage;
                            }
                        }
                        else {
                            string headerText = HeaderText;
                            if (sortableHeader) {
                                LinkButton sortButton = new DataGridLinkButton();

                                sortButton.Text = headerText;
                                sortButton.CommandName = DataGrid.SortCommandName;
                                sortButton.CommandArgument = sortExpression;
                                sortButton.CausesValidation = false;
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

                case ListItemType.Footer:
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
        /// <para>Determines if the System.Web.UI.WebControls.Column is marked to save its state.</para>
        /// </devdoc>
        protected bool IsTrackingViewState {
            get {
                return marked;
            }
        }


        /// <devdoc>
        /// <para>Loads the state of the System.Web.UI.WebControls.Column.</para>
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
        ///    <para>Marks the starting point to begin tracking and saving changes to the 
        ///       control as part of the control viewstate.</para>
        /// </devdoc>
        protected virtual void TrackViewState() {
            marked = true;
            ((IStateManager)ViewState).TrackViewState();
            if (itemStyle != null)
                ((IStateManager)itemStyle).TrackViewState();
            if (headerStyle != null)
                ((IStateManager)headerStyle).TrackViewState();
            if (footerStyle != null)
                ((IStateManager)footerStyle).TrackViewState();
        }


        /// <devdoc>
        /// <para>Raises the ColumnChanged event for a System.Web.UI.WebControls.Column.</para>
        /// </devdoc>
        protected virtual void OnColumnChanged() {
            if (owner != null) {
                owner.OnColumnsChanged();
            }
        }


        /// <devdoc>
        /// <para>Saves the current state of the System.Web.UI.WebControls.Column.</para>
        /// </devdoc>
        protected virtual object SaveViewState() {
            object propState = ((IStateManager)ViewState).SaveViewState();
            object itemStyleState = (itemStyle != null) ? ((IStateManager)itemStyle).SaveViewState() : null;
            object headerStyleState = (headerStyle != null) ? ((IStateManager)headerStyle).SaveViewState() : null;
            object footerStyleState = (footerStyle != null) ? ((IStateManager)footerStyle).SaveViewState() : null;

            if ((propState != null) ||
                (itemStyleState != null) ||
                (headerStyleState != null) ||
                (footerStyleState != null)) {
                return new object[4] {
                    propState,
                    itemStyleState,
                    headerStyleState,
                    footerStyleState
                };
            }

            return null;
        }


        /// <devdoc>
        /// </devdoc>
        internal void SetOwner(DataGrid owner) {
            this.owner = owner;
        }


        /// <devdoc>
        /// <para>Converts the System.Web.UI.WebControls.Column to string.</para>
        /// </devdoc>
        public override string ToString() {
            return String.Empty;
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
    }
}

