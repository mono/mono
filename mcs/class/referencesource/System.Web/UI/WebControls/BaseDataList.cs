//------------------------------------------------------------------------------
// <copyright file="BaseDataList.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Web;
    using System.Web.UI;
    using System.Web.Util;

    /// <devdoc>
    /// <para>Serves as the abstract base class for the <see cref='System.Web.UI.WebControls.DataList'/> and <see cref='System.Web.UI.WebControls.DataGrid'/>
    /// controls and implements the selection semantics which are common to both
    /// controls.</para>
    /// </devdoc>
    [
    DefaultEvent("SelectedIndexChanged"),
    DefaultProperty("DataSource"),
    Designer("System.Web.UI.Design.WebControls.BaseDataListDesigner, " + AssemblyRef.SystemDesign)
    ]
    public abstract class BaseDataList : WebControl {

        private static readonly object EventSelectedIndexChanged = new object();

        internal const string ItemCountViewStateKey = "_!ItemCount";

        private object dataSource;
        private DataKeyCollection dataKeysCollection;
        private bool _requiresDataBinding;
        private bool _inited;
        private bool _throwOnDataPropertyChange;

        private DataSourceView _currentView;
        private bool _currentViewIsFromDataSourceID;
        private bool _currentViewValid;
        private DataSourceSelectArguments _arguments;
        private bool _pagePreLoadFired;


        [
        DefaultValue(""),
        Localizable(true),
        WebCategory("Accessibility"),
        WebSysDescription(SR.DataControls_Caption)
        ]
        public virtual string Caption {
            get {
                string s = (string)ViewState["Caption"];
                return (s != null) ? s : String.Empty;
            }
            set {
                ViewState["Caption"] = value;
            }
        }


        [
        DefaultValue(TableCaptionAlign.NotSet),
        WebCategory("Accessibility"),
        WebSysDescription(SR.WebControl_CaptionAlign)
        ]
        public virtual TableCaptionAlign CaptionAlign {
            get {
                object o = ViewState["CaptionAlign"];
                return (o != null) ? (TableCaptionAlign)o : TableCaptionAlign.NotSet;
            }
            set {
                if ((value < TableCaptionAlign.NotSet) ||
                    (value > TableCaptionAlign.Right)) {
                    throw new ArgumentOutOfRangeException("value");
                }
                ViewState["CaptionAlign"] = value;
            }
        }


        /// <devdoc>
        ///    <para>Indicates the amount of space between cells.</para>
        /// </devdoc>
        [
        WebCategory("Layout"),
        DefaultValue(-1),
        WebSysDescription(SR.BaseDataList_CellPadding)
        ]
        public virtual int CellPadding {
            get {
                if (ControlStyleCreated == false) {
                    return -1;
                }
                return ((TableStyle)ControlStyle).CellPadding;
            }
            set {
                ((TableStyle)ControlStyle).CellPadding = value;
            }
        }


        /// <devdoc>
        ///    <para>Gets or sets the amount of space between the contents of
        ///       a cell and the cell's border.</para>
        /// </devdoc>
        [
        WebCategory("Layout"),
        DefaultValue(0),
        WebSysDescription(SR.BaseDataList_CellSpacing)
        ]
        public virtual int CellSpacing {
            get {
                if (ControlStyleCreated == false) {
                    return 0;
                }
                return ((TableStyle)ControlStyle).CellSpacing;
            }
            set {
                ((TableStyle)ControlStyle).CellSpacing = value;
            }
        }


        public override ControlCollection Controls {
            get {
                EnsureChildControls();
                return base.Controls;
            }
        }


        /// <devdoc>
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        WebSysDescription(SR.BaseDataList_DataKeys)
        ]
        public DataKeyCollection DataKeys {
            get {
                if (dataKeysCollection == null) {
                    dataKeysCollection = new DataKeyCollection(this.DataKeysArray);
                }
                return dataKeysCollection;
            }
        }


        /// <devdoc>
        /// </devdoc>
        protected ArrayList DataKeysArray {
            get {
                object o = ViewState["DataKeys"];
                if (o == null) {
                    o = new ArrayList();
                    ViewState["DataKeys"] = o;
                }
                return(ArrayList)o;
            }
        }



        /// <devdoc>
        /// <para>Indicatesthe primary key field in the data source referenced by <see cref='System.Web.UI.WebControls.BaseDataList.DataSource'/>.</para>
        /// </devdoc>
        [
        DefaultValue(""),
        Themeable(false),
        WebCategory("Data"),
        WebSysDescription(SR.BaseDataList_DataKeyField)
        ]
        public virtual string DataKeyField {
            get {
                object o = ViewState["DataKeyField"];
                if (o != null)
                    return(string)o;
                return String.Empty;
            }
            set {
                ViewState["DataKeyField"] = value;
            }
        }


        /// <devdoc>
        /// </devdoc>
        [
        DefaultValue(""),
        Themeable(false),
        WebCategory("Data"),
        WebSysDescription(SR.BaseDataList_DataMember)
        ]
        public string DataMember {
            get {
                object o = ViewState["DataMember"];
                if (o != null)
                    return (string)o;
                return String.Empty;
            }
            set {
                ViewState["DataMember"] = value;
                OnDataPropertyChanged();
            }
        }


        /// <devdoc>
        ///    <para>Gets
        ///       or sets the source to a list of values used to populate
        ///       the items within the control.</para>
        /// </devdoc>
        [
        Bindable(true),
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        Themeable(false),
        WebCategory("Data"),
        WebSysDescription(SR.BaseDataBoundControl_DataSource),
        ]
        public virtual object DataSource {
            get {
                return dataSource;
            }
            set {
                if ((value == null) || (value is IListSource) || (value is IEnumerable)) {
                    dataSource = value;
                    OnDataPropertyChanged();
                }
                else {
                    throw new ArgumentException(SR.GetString(SR.Invalid_DataSource_Type, ID));
                }
            }
        }


        /// <summary>
        /// The ID of the DataControl that this control should use to retrieve
        /// its data source. When the control is bound to a DataControl, it
        /// can retrieve a data source instance on-demand, and thereby attempt
        /// to work in auto-DataBind mode.
        /// </summary>
        [
        DefaultValue(""),
        IDReferenceProperty(typeof(DataSourceControl)),
        Themeable(false),
        WebCategory("Data"),
        WebSysDescription(SR.BaseDataBoundControl_DataSourceID)
        ]
        public virtual string DataSourceID {
            get {
                object o = ViewState["DataSourceID"];
                if (o != null) {
                    return (string)o;
                }
                return String.Empty;
            }
            set {
                ViewState["DataSourceID"] = value;
                OnDataPropertyChanged();
            }
        }


        /// <devdoc>
        ///    <para>Gets or sets a value that specifies the grid line style.</para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(GridLines.Both),
        WebSysDescription(SR.DataControls_GridLines)
        ]
        public virtual GridLines GridLines {
            get {
                if (ControlStyleCreated == false) {
                    return GridLines.Both;
                }
                return ((TableStyle)ControlStyle).GridLines;
            }
            set {
                ((TableStyle)ControlStyle).GridLines = value;
            }
        }


        /// <devdoc>
        ///    <para>Gets or sets a value that specifies the alignment of a rows with respect
        ///       surrounding text.</para>
        /// </devdoc>
        [
        Category("Layout"),
        DefaultValue(HorizontalAlign.NotSet),
        WebSysDescription(SR.WebControl_HorizontalAlign)
        ]
        public virtual HorizontalAlign HorizontalAlign {
            get {
                if (ControlStyleCreated == false) {
                    return HorizontalAlign.NotSet;
                }
                return ((TableStyle)ControlStyle).HorizontalAlign;
            }
            set {
                ((TableStyle)ControlStyle).HorizontalAlign = value;
            }
        }


        protected bool Initialized {
            get {
                return _inited;
            }
        }


        protected bool IsBoundUsingDataSourceID {
            get {
                return (DataSourceID.Length > 0);
            }
        }

        public override bool SupportsDisabledAttribute {
            get {
                return RenderingCompatibility < VersionUtil.Framework40;
            }
        }

        protected bool RequiresDataBinding {
            get {
                return _requiresDataBinding;
            }
            set {
                _requiresDataBinding = value;
            }
        }

        protected DataSourceSelectArguments SelectArguments {
            get {
                if (_arguments == null) {
                    _arguments = CreateDataSourceSelectArguments();
                }
                return _arguments;
            }
        }


        [
        DefaultValue(false),
        WebCategory("Accessibility"),
        WebSysDescription(SR.Table_UseAccessibleHeader)
        ]
        public virtual bool UseAccessibleHeader {
            get {
                object b = ViewState["UseAccessibleHeader"];
                return (b != null) ? (bool)b : false;
            }
            set {
                ViewState["UseAccessibleHeader"] = value;
            }
        }


        /// <devdoc>
        ///    <para>Occurs when an item on the list is selected.</para>
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.BaseDataList_OnSelectedIndexChanged)
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
        ///    <para> Not coded yet.</para>
        /// </devdoc>
        protected override void AddParsedSubObject(object obj) {
            return;
        }

        /// <devdoc>
        /// Connects this data bound control to the appropriate DataSourceView
        /// and hooks up the appropriate event listener for the
        /// DataSourceViewChanged event. The return value is the new view (if
        /// any) that was connected to. An exception is thrown if there is
        /// a problem finding the requested view or data source.
        /// </devdoc>
        private DataSourceView ConnectToDataSourceView() {
            if (_currentViewValid && !DesignMode) {
                // If the current view is correct, there is no need to reconnect
                return _currentView;
            }

            // Disconnect from old view, if necessary
            if ((_currentView != null) && (_currentViewIsFromDataSourceID)) {
                // We only care about this event if we are bound through the DataSourceID property
                _currentView.DataSourceViewChanged -= new EventHandler(OnDataSourceViewChanged);
            }

            // Connect to new view
            IDataSource ds = null;
            string dataSourceID = DataSourceID;

            if (dataSourceID.Length != 0) {
                // Try to find a DataSource control with the ID specified in DataSourceID
                Control control = DataBoundControlHelper.FindControl(this, dataSourceID);
                if (control == null) {
                    throw new HttpException(SR.GetString(SR.DataControl_DataSourceDoesntExist, ID, dataSourceID));
                }
                ds = control as IDataSource;
                if (ds == null) {
                    throw new HttpException(SR.GetString(SR.DataControl_DataSourceIDMustBeDataControl, ID, dataSourceID));
                }
            }

            if (ds == null) {
                // DataSource control was not found, construct a temporary data source to wrap the data
                ds = new ReadOnlyDataSource(DataSource, DataMember);
            }
            else {
                // Ensure that both DataSourceID as well as DataSource are not set at the same time
                if (DataSource != null) {
                    throw new InvalidOperationException(SR.GetString(SR.DataControl_MultipleDataSources, ID));
                }
            }

            // IDataSource was found, extract the appropriate view and return it
            DataSourceView newView = ds.GetView(DataMember);
            if (newView == null) {
                throw new InvalidOperationException(SR.GetString(SR.DataControl_ViewNotFound, ID));
            }

            _currentViewIsFromDataSourceID = IsBoundUsingDataSourceID;
            _currentView = newView;
            if ((_currentView != null) && (_currentViewIsFromDataSourceID)) {
                // We only care about this event if we are bound through the DataSourceID property
                _currentView.DataSourceViewChanged += new EventHandler(OnDataSourceViewChanged);
            }
            _currentViewValid = true;

            return _currentView;
        }


        /// <internalonly/>
        /// <devdoc>
        ///    <para>Creates a child control using the view state.</para>
        /// </devdoc>
        protected internal override void CreateChildControls() {
            Controls.Clear();

            if (ViewState[ItemCountViewStateKey] == null) {
                if (RequiresDataBinding) {
                    EnsureDataBound();
                }
            }
            else {
                // create the control hierarchy using the view state (and
                // not the datasource)
                CreateControlHierarchy(false);
                ClearChildViewState();
            }
        }


        protected abstract void CreateControlHierarchy(bool useDataSource);


        public override void DataBind() {
            // Don't databind to a data source control when the control is in the designer but not top-level
            if (IsBoundUsingDataSourceID && DesignMode && (Site == null)) {
                return;
            }

            // do our own databinding
            RequiresDataBinding = false;
            OnDataBinding(EventArgs.Empty);
        }

        protected virtual DataSourceSelectArguments CreateDataSourceSelectArguments() {
            return DataSourceSelectArguments.Empty;
        }


        protected void EnsureDataBound() {
            try {
                _throwOnDataPropertyChange = true;
                if (RequiresDataBinding && DataSourceID.Length > 0) {
                    DataBind();
                }
            }
            finally {
                _throwOnDataPropertyChange = false;
            }
        }


        /// <devdoc>
        /// Returns an IEnumerable that is the DataSource, which either came
        /// from the DataSource property or from the control bound via the
        /// DataSourceID property.
        /// </devdoc>
        protected virtual IEnumerable GetData() {
            ConnectToDataSourceView();

            Debug.Assert(_currentViewValid);

            if (_currentView != null) {
                return _currentView.ExecuteSelect(SelectArguments);
            }
            return null;
        }


        /// <devdoc>
        ///    <para>Determines if the specified data type can be bound to.</para>
        /// </devdoc>
        public static bool IsBindableType(Type type) {
            return(type.IsPrimitive ||
                   (type == typeof(string)) ||
                   (type == typeof(DateTime)) ||
                   (type == typeof(Decimal)));
        }


        /// <internalonly/>
        /// <devdoc>
        /// <para>Raises the <see langword='DataBinding '/>event of a <see cref='System.Web.UI.WebControls.BaseDataList'/>
        /// .</para>
        /// </devdoc>
        protected override void OnDataBinding(EventArgs e) {
            base.OnDataBinding(e);

            // reset the control state
            Controls.Clear();
            ClearChildViewState();

            // and create the control hierarchy using the datasource
            dataKeysCollection = null;
            CreateControlHierarchy(true);
            ChildControlsCreated = true;

            TrackViewState();
        }


        /// <devdoc>
        ///  This method is called when DataMember, DataSource, or DataSourceID is changed.
        /// </devdoc>
        protected virtual void OnDataPropertyChanged() {
            if (_throwOnDataPropertyChange) {
                throw new HttpException(SR.GetString(SR.DataBoundControl_InvalidDataPropertyChange, ID));
            }
            
            if (_inited) {
                RequiresDataBinding = true;
            }
            _currentViewValid = false;
        }


        protected virtual void OnDataSourceViewChanged(object sender, EventArgs e) {
            RequiresDataBinding = true;
        }


        protected internal override void OnInit(EventArgs e) {
            base.OnInit(e);

            if (Page != null) {
                Page.PreLoad += new EventHandler(this.OnPagePreLoad);

                if (!IsViewStateEnabled && Page.IsPostBack) {
                    RequiresDataBinding = true;
                }
            }
        }


        protected internal override void OnLoad(EventArgs e) {
            _inited = true; // just in case we were added to the page after PreLoad
            ConnectToDataSourceView();
            if (Page != null &&  !_pagePreLoadFired && ViewState[ItemCountViewStateKey] == null) {
                // If the control was added after PagePreLoad, we still need to databind it because it missed its
                // first change in PagePreLoad.  If this control was created by a call to a parent control's DataBind
                // in Page_Load (with is relatively common), this control will already have been databound even
                // though pagePreLoad never fired and the page isn't a postback.
                if (!Page.IsPostBack) {
                    RequiresDataBinding = true;
                }

                // If the control was added to the page after page.PreLoad, we'll never get the event and we'll
                // never databind the control.  So if we're catching up and Load happens but PreLoad never happened,
                // call DataBind.  This may make the control get databound twice if the user called DataBind on the control
                // directly in Page.OnLoad, but better to bind twice than never to bind at all.
                else if (IsViewStateEnabled) {
                    RequiresDataBinding = true;
                }
            }

            base.OnLoad(e);
        }

        private void OnPagePreLoad(object sender, EventArgs e) {
            _inited = true;

            if (Page != null) {
                Page.PreLoad -= new EventHandler(this.OnPagePreLoad);

                // Setting RequiresDataBinding to true in OnLoad is too late because the OnLoad page event
                // happens before the control.OnLoad method gets called.  So a page_load handler on the page
                // that calls DataBind won't prevent DataBind from getting called again in PreRender.
                if (!Page.IsPostBack) {
                    RequiresDataBinding = true;
                }

                // If this is a postback and viewstate is enabled, but we have never bound the control
                // before, it is probably because its visibility was changed in the postback.  In this
                // case, we need to bind the control or it will never appear.  This is a common scenario
                // for Wizard and MultiView.
                if (Page.IsPostBack && IsViewStateEnabled && ViewState[ItemCountViewStateKey] == null) {
                    RequiresDataBinding = true;
                }
            }
            _pagePreLoadFired = true;
        }


        protected internal override void OnPreRender(EventArgs e) {
            EnsureDataBound();
            base.OnPreRender(e);
        }


        /// <devdoc>
        /// <para>Raises the <see cref='System.Web.UI.WebControls.BaseDataList.SelectedIndexChanged'/>event of a <see cref='System.Web.UI.WebControls.BaseDataList'/>.</para>
        /// </devdoc>
        protected virtual void OnSelectedIndexChanged(EventArgs e) {
            EventHandler handler = (EventHandler)Events[EventSelectedIndexChanged];
            if (handler != null) handler(this, e);
        }


        protected internal abstract void PrepareControlHierarchy();


        /// <internalonly/>
        /// <devdoc>
        ///    <para>Displays the control on the client.</para>
        /// </devdoc>
        protected internal override void Render(HtmlTextWriter writer) {
            PrepareControlHierarchy();
            RenderContents(writer);
        }
    }
}

