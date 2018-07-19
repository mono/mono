//------------------------------------------------------------------------------
// <copyright file="Repeater.cs" company="Microsoft">
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
    /// <para>Defines the properties, methods, and events of a <see cref='System.Web.UI.WebControls.Repeater'/> class.</para>
    /// </devdoc>
    [
    DefaultEvent("ItemCommand"),
    DefaultProperty("DataSource"),
    Designer("System.Web.UI.Design.WebControls.RepeaterDesigner, " + AssemblyRef.SystemDesign),
    ParseChildren(true),
    PersistChildren(false)
    ]
    public class Repeater : Control, INamingContainer {

        private static readonly object EventItemCreated = new object();
        private static readonly object EventItemDataBound = new object();
        private static readonly object EventItemCommand = new object();
        private static readonly object EventCreatingModelDataSource = new object();
        private static readonly object EventCallingDataMethods = new object();

        internal const string ItemCountViewStateKey = "_!ItemCount";

        private object dataSource;
        private ITemplate headerTemplate;
        private ITemplate footerTemplate;
        private ITemplate itemTemplate;
        private ITemplate alternatingItemTemplate;
        private ITemplate separatorTemplate;

        private ArrayList itemsArray;
        private RepeaterItemCollection itemsCollection;

        private bool _requiresDataBinding;
        private bool _inited;
        private bool _throwOnDataPropertyChange;

        private DataSourceView _currentView;
        private bool _currentViewIsFromDataSourceID;
        private bool _currentViewValid;
        private DataSourceSelectArguments _arguments;
        private bool _pagePreLoadFired;

        private string _itemType;
        private string _selectMethod;
        private ModelDataSource _modelDataSource;
        private bool _asyncSelectPending;


        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.UI.WebControls.Repeater'/> class.</para>
        /// </devdoc>
        public Repeater() {
        }

        private bool IsUsingModelBinders {
            get {
                return !String.IsNullOrEmpty(SelectMethod);
            }
        }

        private void UpdateModelDataSourceProperties(ModelDataSource modelDataSource) {
            if (modelDataSource == null) {
                throw new ArgumentNullException("modelDataSource");
            }

            modelDataSource.UpdateProperties(ItemType, SelectMethod);
        }

        private ModelDataSource ModelDataSource {
            get {
                if (_modelDataSource == null) {
                    _modelDataSource = new ModelDataSource(this);
                }
                return _modelDataSource;
            }
            set {
                if (value == null) {
                    throw new ArgumentNullException("value");
                }

                _modelDataSource = value;
            }
        }

        protected virtual void OnCreatingModelDataSource(CreatingModelDataSourceEventArgs e) {
            CreatingModelDataSourceEventHandler handler = Events[EventCreatingModelDataSource] as CreatingModelDataSourceEventHandler;
            if (handler != null) {
                handler(this, e);
            }
        }

        [
        WebCategory("Data"),
        WebSysDescription(SR.DataBoundControl_OnCreatingModelDataSource)
        ]
        public event CreatingModelDataSourceEventHandler CreatingModelDataSource {
            add {
                Events.AddHandler(EventCreatingModelDataSource, value);
            }
            remove {
                Events.RemoveHandler(EventCreatingModelDataSource, value);
            }
        }

        /// <summary>
        /// The name of the model type used in the SelectMethod, InsertMethod, UpdateMethod, and DeleteMethod.
        /// </summary>
        [
        DefaultValue(""),
        Themeable(false),
        WebCategory("Data"),
        WebSysDescription(SR.DataBoundControl_ItemType)
        ]
        public virtual string ItemType {
            get {
                return _itemType ?? String.Empty;
            }
            set {
                if (!String.Equals(_itemType, value, StringComparison.OrdinalIgnoreCase)) {
                    _itemType = value;
                    OnDataPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The name of the method on the page which is called when this Control does a select operation.
        /// </summary>
        [
        DefaultValue(""),
        Themeable(false),
        WebCategory("Data"),
        WebSysDescription(SR.DataBoundControl_SelectMethod)
        ]
        public virtual string SelectMethod {
            get {
                return _selectMethod ?? String.Empty;
            }
            set {
                if (!String.Equals(_selectMethod, value, StringComparison.OrdinalIgnoreCase)) {
                    _selectMethod = value;
                    OnDataPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Occurs before model methods are invoked for data operations. 
        /// Handle this event if the model methods are defined on a custom type other than the code behind file.
        /// </summary>
        [
        WebCategory("Data"),
        WebSysDescription(SR.DataBoundControl_CallingDataMethods)
        ]
        public event CallingDataMethodsEventHandler CallingDataMethods {
            add {
                Events.AddHandler(EventCallingDataMethods, value);
            }
            remove {
                Events.RemoveHandler(EventCallingDataMethods, value);
                if (_modelDataSource != null) {
                    _modelDataSource.CallingDataMethods -= value;
                }
            }
        }

        /// <devdoc>
        /// <para>Gets or sets the <see cref='System.Web.UI.ITemplate' qualify='true'/> that defines how alternating (even-indexed) items
        ///    are rendered. </para>
        /// </devdoc>
        [
            Browsable(false),
            DefaultValue(null),
            PersistenceMode(PersistenceMode.InnerProperty),
            TemplateContainer(typeof(RepeaterItem)),
            WebSysDescription(SR.Repeater_AlternatingItemTemplate)
        ]
        public virtual ITemplate AlternatingItemTemplate {
            get {
                return alternatingItemTemplate;
            }
            set {
                alternatingItemTemplate = value;
            }
        }


        public override ControlCollection Controls {
            get {
                EnsureChildControls();
                return base.Controls;
            }
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [
        DefaultValue(""),
        WebCategory("Data"),
        WebSysDescription(SR.Repeater_DataMember)
        ]
        public virtual string DataMember {
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
        ///    <para> Gets or sets the data source that provides data for
        ///       populating the list.</para>
        /// </devdoc>
        [
            Bindable(true),
            WebCategory("Data"),
            DefaultValue(null),
            DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
            WebSysDescription(SR.BaseDataBoundControl_DataSource)
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
        ///    <para>Gets and sets a value indicating whether theme is enabled.</para>
        /// </devdoc>
        [
        Browsable(true)
        ]
        public override bool EnableTheming {
            get {
                return base.EnableTheming;
            }
            set {
                base.EnableTheming = value;
            }
        }


        /// <devdoc>
        /// <para>Gets or sets the <see cref='System.Web.UI.ITemplate' qualify='true'/> that defines how the control footer is
        ///    rendered. </para>
        /// </devdoc>
        [
            Browsable(false),
            DefaultValue(null),
            PersistenceMode(PersistenceMode.InnerProperty),
            TemplateContainer(typeof(RepeaterItem)),
            WebSysDescription(SR.Repeater_FooterTemplate)
        ]
        public virtual ITemplate FooterTemplate {
            get {
                return footerTemplate;
            }
            set {
                footerTemplate = value;
            }
        }


        /// <devdoc>
        /// <para>Gets or sets the <see cref='System.Web.UI.ITemplate' qualify='true'/> that defines how the control header is rendered. </para>
        /// </devdoc>
        [
            Browsable(false),
            DefaultValue(null),
            PersistenceMode(PersistenceMode.InnerProperty),
            TemplateContainer(typeof(RepeaterItem)),
            WebSysDescription(SR.WebControl_HeaderTemplate)
        ]
        public virtual ITemplate HeaderTemplate {
            get {
                return headerTemplate;
            }
            set {
                headerTemplate = value;
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

        protected bool IsDataBindingAutomatic {
            get {
                return IsBoundUsingDataSourceID || IsUsingModelBinders;
            }
        }


        /// <devdoc>
        ///    Gets the <see cref='System.Web.UI.WebControls.RepeaterItem'/> collection.
        /// </devdoc>
        [
            Browsable(false),
            DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
            WebSysDescription(SR.Repeater_Items)
        ]
        public virtual RepeaterItemCollection Items {
            get {
                if (itemsCollection == null) {
                    if (itemsArray == null) {
                        EnsureChildControls();
                    }
                    itemsCollection = new RepeaterItemCollection(itemsArray);
                }
                return itemsCollection;
            }
        }


        /// <devdoc>
        /// <para>Gets or sets the <see cref='System.Web.UI.ITemplate' qualify='true'/> that defines how items are rendered. </para>
        /// </devdoc>
        [
            Browsable(false),
            DefaultValue(null),
            PersistenceMode(PersistenceMode.InnerProperty),
            TemplateContainer(typeof(RepeaterItem)),
            WebSysDescription(SR.Repeater_ItemTemplate)
        ]
        public virtual ITemplate ItemTemplate {
            get {
                return itemTemplate;
            }
            set {
                itemTemplate = value;
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


        /// <devdoc>
        /// <para>Gets or sets the <see cref='System.Web.UI.ITemplate' qualify='true'/> that defines how separators
        ///    in between items are rendered.</para>
        /// </devdoc>
        [
            Browsable(false),
            DefaultValue(null),
            PersistenceMode(PersistenceMode.InnerProperty),
            TemplateContainer(typeof(RepeaterItem)),
            WebSysDescription(SR.Repeater_SeparatorTemplate)
        ]
        public virtual ITemplate SeparatorTemplate {
            get {
                return separatorTemplate;
            }
            set {
                separatorTemplate = value;
            }
        }



        /// <devdoc>
        /// <para>Occurs when a button is clicked within the <see cref='System.Web.UI.WebControls.Repeater'/> control tree.</para>
        /// </devdoc>
       [
        WebCategory("Action"),
        WebSysDescription(SR.Repeater_OnItemCommand)
        ]
        public event RepeaterCommandEventHandler ItemCommand {
            add {
                Events.AddHandler(EventItemCommand, value);
            }
            remove {
                Events.RemoveHandler(EventItemCommand, value);
            }
        }



        /// <devdoc>
        /// <para> Occurs when an item is created within the <see cref='System.Web.UI.WebControls.Repeater'/> control tree.</para>
        /// </devdoc>
        [
        WebCategory("Behavior"),
        WebSysDescription(SR.DataControls_OnItemCreated)
        ]
        public event RepeaterItemEventHandler ItemCreated {
            add {
                Events.AddHandler(EventItemCreated, value);
            }
            remove {
                Events.RemoveHandler(EventItemCreated, value);
            }
        }


        /// <devdoc>
        /// <para>Occurs when an item is databound within a <see cref='System.Web.UI.WebControls.Repeater'/> control tree.</para>
        /// </devdoc>
        [
        WebCategory("Behavior"),
        WebSysDescription(SR.DataControls_OnItemDataBound)
        ]
        public event RepeaterItemEventHandler ItemDataBound {
            add {
                Events.AddHandler(EventItemDataBound, value);
            }
            remove {
                Events.RemoveHandler(EventItemDataBound, value);
            }
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
            if (!DesignMode && IsUsingModelBinders) {
                if (DataSourceID.Length != 0 || DataSource != null) {
                    throw new InvalidOperationException(SR.GetString(SR.DataControl_ItemType_MultipleDataSources, ID));
                }
                //Let the developer choose a custom ModelDataSource.
                CreatingModelDataSourceEventArgs e = new CreatingModelDataSourceEventArgs();
                OnCreatingModelDataSource(e);
                if (e.ModelDataSource != null) {
                    ModelDataSource = e.ModelDataSource;
                }

                //Update the properties of ModelDataSource so that it's ready for data-binding.
                UpdateModelDataSourceProperties(ModelDataSource);

                //Add the CallingDataMethodsEvent
                CallingDataMethodsEventHandler handler = Events[EventCallingDataMethods] as CallingDataMethodsEventHandler;
                if (handler != null) {
                    ModelDataSource.CallingDataMethods += handler;
                }

                ds = ModelDataSource;
            }
            else {
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

            _currentViewIsFromDataSourceID = IsDataBindingAutomatic;
            _currentView = newView;
            if ((_currentView != null) && (_currentViewIsFromDataSourceID)) {
                // We only care about this event if we are bound through the DataSourceID property
                _currentView.DataSourceViewChanged += new EventHandler(OnDataSourceViewChanged);
            }
            _currentViewValid = true;

            return _currentView;
        }


        /// <devdoc>
        /// </devdoc>
        protected internal override void CreateChildControls() {
            Controls.Clear();

            if (ViewState[ItemCountViewStateKey] != null) {
                // create the control hierarchy using the view state (and
                // not the datasource)
                CreateControlHierarchy(false);
            }
            else {
                itemsArray = new ArrayList();
            }
            ClearChildViewState();
        }



        /// <devdoc>
        ///    A protected method. Creates a control
        ///    hierarchy, with or without the data source as specified.
        /// </devdoc>
        protected virtual void CreateControlHierarchy(bool useDataSource) {
            IEnumerable dataSource = null;

            if (itemsArray != null) {
                itemsArray.Clear();
            }
            else {
                itemsArray = new ArrayList();
            }

            if (!useDataSource) {
                // ViewState must have a non-null value for ItemCount because we check for
                // this in CreateChildControls
                int count = (int)ViewState[ItemCountViewStateKey];
                if (count != -1) {
                    dataSource = new DummyDataSource(count);
                    itemsArray.Capacity = count;
                }

                AddDataItemsIntoItemsArray(dataSource, useDataSource);
            }
            else {
                dataSource = GetData();
                PostGetDataAction(dataSource);
            }
        }

        private void OnDataSourceViewSelectCallback(IEnumerable data) {
            _asyncSelectPending = false;
            PostGetDataAction(data);
        }

        private void PostGetDataAction(IEnumerable dataSource) {
            if (_asyncSelectPending)
                return;

            ICollection collection = dataSource as ICollection;
            if (collection != null) {
                itemsArray.Capacity = collection.Count;
            }

            int addedItemCount = AddDataItemsIntoItemsArray(dataSource, true/*useDataSource*/);
            ViewState[ItemCountViewStateKey] = addedItemCount;
        }

        private int AddDataItemsIntoItemsArray(IEnumerable dataSource, bool useDataSource) {
            int dataItemCount = -1;
            if (dataSource != null) {
                RepeaterItem item;
                ListItemType itemType;
                int index = 0;

                bool hasSeparators = (separatorTemplate != null);
                dataItemCount = 0;

                if (headerTemplate != null) {
                    CreateItem(-1, ListItemType.Header, useDataSource, null);
                }

                foreach (object dataItem in dataSource) {
                    // rather than creating separators after the item, we create the separator
                    // for the previous item in all iterations of this loop.
                    // this is so that we don't create a separator for the last item
                    if (hasSeparators && (dataItemCount > 0)) {
                        CreateItem(index - 1, ListItemType.Separator, useDataSource, null);
                    }

                    itemType = (index % 2 == 0) ? ListItemType.Item : ListItemType.AlternatingItem;
                    item = CreateItem(index, itemType, useDataSource, dataItem);
                    itemsArray.Add(item);

                    dataItemCount++;
                    index++;
                }

                if (footerTemplate != null) {
                    CreateItem(-1, ListItemType.Footer, useDataSource, null);
                }
            }

            return dataItemCount;
        }

        protected virtual DataSourceSelectArguments CreateDataSourceSelectArguments() {
            return DataSourceSelectArguments.Empty;
        }


        /// <devdoc>
        /// </devdoc>
        private RepeaterItem CreateItem(int itemIndex, ListItemType itemType, bool dataBind, object dataItem) {
            RepeaterItem item = CreateItem(itemIndex, itemType);
            RepeaterItemEventArgs e = new RepeaterItemEventArgs(item);

            InitializeItem(item);
            if (dataBind) {
                item.DataItem = dataItem;
            }
            OnItemCreated(e);
            Controls.Add(item);

            if (dataBind) {
                item.DataBind();
                OnItemDataBound(e);

                item.DataItem = null;
            }

            return item;
        }


        /// <devdoc>
        /// <para>A protected method. Creates a <see cref='System.Web.UI.WebControls.RepeaterItem'/> with the specified item type and
        ///    location within the <see cref='System.Web.UI.WebControls.Repeater'/>.</para>
        /// </devdoc>
        protected virtual RepeaterItem CreateItem(int itemIndex, ListItemType itemType) {
            return new RepeaterItem(itemIndex, itemType);
        }


        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        public override void DataBind() {
            // Don't databind to a data source control when the control is in the designer but not top-level
            if (IsDataBindingAutomatic && DesignMode && (Site == null)) {
                return;
            }

            // do our own databinding
            RequiresDataBinding = false;
            OnDataBinding(EventArgs.Empty);

            // contained items will be databound after they have been created,
            // so we don't want to walk the hierarchy here.
        }


        protected void EnsureDataBound() {
            try {
                _throwOnDataPropertyChange = true;
                if (RequiresDataBinding && IsDataBindingAutomatic) {
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
            DataSourceView view = ConnectToDataSourceView();

            Debug.Assert(_currentViewValid);

            if (view != null) {
                // DevDiv 1070535: enable async model binding for Repeater
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
                    return view.ExecuteSelect(SelectArguments);
                }
            }

            return null;
        }


        /// <devdoc>
        /// <para>A protected method. Populates iteratively the specified <see cref='System.Web.UI.WebControls.RepeaterItem'/> with a
        ///    sub-hierarchy of child controls.</para>
        /// </devdoc>
        protected virtual void InitializeItem(RepeaterItem item) {
            ITemplate contentTemplate = null;

            switch (item.ItemType) {
                case ListItemType.Header:
                    contentTemplate = headerTemplate;
                    break;

                case ListItemType.Footer:
                    contentTemplate = footerTemplate;
                    break;

                case ListItemType.Item:
                    contentTemplate = itemTemplate;
                    break;

                case ListItemType.AlternatingItem:
                    contentTemplate = alternatingItemTemplate;
                    if (contentTemplate == null)
                        goto case ListItemType.Item;
                    break;

                case ListItemType.Separator:
                    contentTemplate = separatorTemplate;
                    break;
            }

            if (contentTemplate != null) {
                contentTemplate.InstantiateIn(item);
            }
        }


        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        protected override bool OnBubbleEvent(object sender, EventArgs e) {
            bool handled = false;

            if (e is RepeaterCommandEventArgs) {
                OnItemCommand((RepeaterCommandEventArgs)e);
                handled = true;
            }

            return handled;
        }


        /// <internalonly/>
        /// <devdoc>
        /// <para>A protected method. Raises the <see langword='DataBinding'/> event.</para>
        /// </devdoc>
        protected override void OnDataBinding(EventArgs e) {
            base.OnDataBinding(e);

            // reset the control state
            Controls.Clear();
            ClearChildViewState();

            // and then create the control hierarchy using the datasource
            CreateControlHierarchy(true);
            ChildControlsCreated = true;
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

            if (!DesignMode && !String.IsNullOrEmpty(ItemType)) {
                DataBoundControlHelper.EnableDynamicData(this, ItemType);
            }
        }


        /// <devdoc>
        /// <para>A protected method. Raises the <see langword='ItemCommand'/> event.</para>
        /// </devdoc>
        protected virtual void OnItemCommand(RepeaterCommandEventArgs e) {
            RepeaterCommandEventHandler onItemCommandHandler = (RepeaterCommandEventHandler)Events[EventItemCommand];
            if (onItemCommandHandler != null) onItemCommandHandler(this, e);
        }

        /// <devdoc>
        /// <para>A protected method. Raises the <see langword='ItemCreated'/> event.</para>
        /// </devdoc>
        protected virtual void OnItemCreated(RepeaterItemEventArgs e) {
            RepeaterItemEventHandler onItemCreatedHandler = (RepeaterItemEventHandler)Events[EventItemCreated];
            if (onItemCreatedHandler != null) onItemCreatedHandler(this, e);
        }


        /// <devdoc>
        /// <para>A protected method. Raises the <see langword='ItemDataBound'/>
        /// event.</para>
        /// </devdoc>
        protected virtual void OnItemDataBound(RepeaterItemEventArgs e) {
            RepeaterItemEventHandler onItemDataBoundHandler = (RepeaterItemEventHandler)Events[EventItemDataBound];
            if (onItemDataBoundHandler != null) onItemDataBoundHandler(this, e);
        }


        protected internal override void OnLoad(EventArgs e) {
            _inited = true; // just in case we were added to the page after PreLoad
            ConnectToDataSourceView();
            if (Page != null && !_pagePreLoadFired && ViewState[ItemCountViewStateKey] == null) {
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
            _pagePreLoadFired = true;

            }
        }


        protected internal override void OnPreRender(EventArgs e) {
            EnsureDataBound();
            base.OnPreRender(e);
        }

        /// <devdoc>
        /// Loads view state.
        /// </devdoc>
        protected override void LoadViewState(object savedState) {
            if (IsUsingModelBinders) {
                Pair myState = (Pair)savedState;

                if (savedState == null) {
                    base.LoadViewState(null);
                }
                else {
                    base.LoadViewState(myState.First);

                    if (myState.Second != null) {
                        ((IStateManager)ModelDataSource).LoadViewState(myState.Second);
                    }
                }
            }
            else {
                base.LoadViewState(savedState);
            }
        }

        /// <devdoc>
        /// Saves view state.
        /// </devdoc>
        protected override object SaveViewState() {
            // Bug 322689: In the web farms scenario, if a web site is hosted in 4.0 and 4.5 servers
            // (though this is not a really supported scenario, we are fixing this instance), 
            // the View state created by 4.0 should be able to be understood by 4.5 controls.
            // So, we create a Pair only if we are using model binding and otherwise fallback to 4.0 behavior.

            object baseViewState = base.SaveViewState();

            if (IsUsingModelBinders) {
                Pair myState = new Pair();

                myState.First = baseViewState;
                myState.Second = ((IStateManager)ModelDataSource).SaveViewState();

                if ((myState.First == null) &&
                    (myState.Second == null)) {
                    return null;
                }

                return myState;
            }
            else {
                return baseViewState;
            }
        }

        /// <devdoc>
        /// Starts tracking view state.
        /// </devdoc>
        protected override void TrackViewState() {
            base.TrackViewState();

            if (IsUsingModelBinders) {
                ((IStateManager)ModelDataSource).TrackViewState();
            }
        }
    }
}

