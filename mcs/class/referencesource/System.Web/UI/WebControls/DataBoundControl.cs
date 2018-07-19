//------------------------------------------------------------------------------
// <copyright file="DataBoundControl.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Web.Util;
    using System.Web.UI.WebControls.Adapters;


    /// <summary>
    /// A DataBoundControl is bound to a data source and generates its
    /// user interface (or child control hierarchy typically), by enumerating
    /// the items in the data source it is bound to.
    /// DataBoundControl is an abstract base class that defines the common
    /// characteristics of all controls that use a list as a data source, such as
    /// DataGrid, DataBoundTable, ListBox etc. It encapsulates the logic
    /// of how a data-bound control binds to collections or DataControl instances.
    /// </summary>

    [
    Designer("System.Web.UI.Design.WebControls.DataBoundControlDesigner, " + AssemblyRef.SystemDesign),
    ]
    public abstract class DataBoundControl : BaseDataBoundControl {

        private DataSourceView _currentView;
        private bool _currentViewIsFromDataSourceID;
        private bool _currentViewValid;
        private IDataSource _currentDataSource;
        private bool _currentDataSourceValid;
        private DataSourceSelectArguments _arguments;
        private bool _pagePreLoadFired;
        private bool _ignoreDataSourceViewChanged;

        private string _itemType;
        private string _selectMethod;
        private ModelDataSource _modelDataSource;

        private const string DataBoundViewStateKey = "_!DataBound";
        private static readonly object EventCreatingModelDataSource = new object();
        private static readonly object EventCallingDataMethods = new object();


        /// <summary>
        /// The name of the list that the DataBoundControl should bind to when
        /// its data source contains more than one list of data items.
        /// </summary>
        [
        DefaultValue(""),
        Themeable(false),
        WebCategory("Data"),
        WebSysDescription(SR.DataBoundControl_DataMember)
        ]
        public virtual string DataMember {
            get {
                object o = ViewState["DataMember"];
                if (o != null) {
                    return (string)o;
                }
                return String.Empty;
            }
            set {
                ViewState["DataMember"] = value;
                OnDataPropertyChanged();
            }
        }


        protected override bool IsUsingModelBinders {
            get {
                return !String.IsNullOrEmpty(SelectMethod);
            }
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

        protected virtual void OnCreatingModelDataSource(CreatingModelDataSourceEventArgs e) {
            CreatingModelDataSourceEventHandler handler = Events[EventCreatingModelDataSource] as CreatingModelDataSourceEventHandler;
            if (handler != null) {
                handler(this, e);
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

        /// <internalonly />
        [
        IDReferenceProperty(typeof(DataSourceControl))
        ]
        public override string DataSourceID {
            get {
                return base.DataSourceID;
            }
            set {
                base.DataSourceID = value;
            }
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public IDataSource DataSourceObject {
            get {
                return GetDataSource();
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

        internal void EnsureSingleDataSource() {
            //Ensure that only one of data binding methods are opted-in.
            if (!DesignMode) {
                if (IsUsingModelBinders) {
                    if (DataSourceID.Length != 0 || DataSource != null) {
                        throw new InvalidOperationException(SR.GetString(SR.DataControl_ItemType_MultipleDataSources, ID));
                    }
                }
                else if (DataSourceID.Length != 0 && DataSource != null) {
                    throw new InvalidOperationException(SR.GetString(SR.DataControl_MultipleDataSources, ID));
                }
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

            EnsureSingleDataSource();
            // Connect to new view
            _currentDataSource = GetDataSource();
            string dataMember = DataMember;

            if (_currentDataSource == null) {
                // DataSource control was not found, construct a temporary data source to wrap the data
                _currentDataSource = new ReadOnlyDataSource(DataSource, dataMember);
            }
            _currentDataSourceValid = true;

            // IDataSource was found, extract the appropriate view and return it
            DataSourceView newView = _currentDataSource.GetView(dataMember);
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

        /// <summary>
        ///  Override to create the DataSourceSelectArguments that will be passed to the view's Select command.
        /// </summary>
        protected virtual DataSourceSelectArguments CreateDataSourceSelectArguments() {
            return DataSourceSelectArguments.Empty;
        }


        /// <devdoc>
        /// Gets the DataSourceView of the IDataSource that this control is
        /// bound to, if any.
        /// </devdoc>
        protected virtual DataSourceView GetData() {
            DataSourceView view = ConnectToDataSourceView();

            Debug.Assert(_currentViewValid);

            return view;
        }


        /// <devdoc>
        /// Gets the IDataSource that this control is bound to, if any.
        /// Because this method can be called directly by derived classes, it's virtual so data can be retrieved
        /// from data sources that don't live on the page.
        /// </devdoc>
        protected virtual IDataSource GetDataSource() {
            if (!DesignMode && IsUsingModelBinders) {
                //Let the developer choose a custom ModelDataSource.
                CreatingModelDataSourceEventArgs e = new CreatingModelDataSourceEventArgs();
                OnCreatingModelDataSource(e);
                if (e.ModelDataSource != null) {
                    ModelDataSource = e.ModelDataSource;
                }

                //Update the properties of ModelDataSource so that it's ready for data-binding.
                UpdateModelDataSourceProperties(ModelDataSource);

                CallingDataMethodsEventHandler handler = Events[EventCallingDataMethods] as CallingDataMethodsEventHandler;
                if (handler != null) {
                    ModelDataSource.CallingDataMethods += handler;
                }

                return ModelDataSource;
            }

            if (!DesignMode && _currentDataSourceValid && (_currentDataSource != null)) {
                return _currentDataSource;
            }
            
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
            return ds;
        }

        protected void MarkAsDataBound() {
            ViewState[DataBoundViewStateKey] = true;
        }
        

        protected override void OnDataPropertyChanged() {
            _currentViewValid = false;
            _currentDataSourceValid = false;
            base.OnDataPropertyChanged();
        }


        /// <devdoc>
        ///  This method is called when the DataSourceView raises a DataSourceViewChanged event.
        /// </devdoc>
        protected virtual void OnDataSourceViewChanged(object sender, EventArgs e) {
            if (!_ignoreDataSourceViewChanged) {
                RequiresDataBinding = true;
            }
        }

        private void OnDataSourceViewSelectCallback(IEnumerable data) {
            _ignoreDataSourceViewChanged = false;
            // We only call OnDataBinding here if we haven't done it already in PerformSelect().
            if (IsDataBindingAutomatic) {
                OnDataBinding(EventArgs.Empty);
            }
            
            if (AdapterInternal != null) {
                DataBoundControlAdapter dataBoundControlAdapter = AdapterInternal as DataBoundControlAdapter;
                if(dataBoundControlAdapter != null) {
                    dataBoundControlAdapter.PerformDataBinding(data);
                }
                else {
                    PerformDataBinding(data);
                }
            }
            else {
                PerformDataBinding(data);
            }
            OnDataBound(EventArgs.Empty);
        }


        protected internal override void OnLoad(EventArgs e) {
            ConfirmInitState();
            ConnectToDataSourceView();

            if (Page != null && !_pagePreLoadFired && ViewState[DataBoundViewStateKey] == null) {
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

        protected override void OnPagePreLoad(object sender, EventArgs e) {
            base.OnPagePreLoad(sender, e);
            
            if (Page != null) {
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
                else if (IsViewStateEnabled && ViewState[DataBoundViewStateKey] == null) {
                    RequiresDataBinding = true;
                }
            }
            _pagePreLoadFired = true;
        }



        /// <devdoc>
        ///  This method should be overridden by databound controls to perform their databinding.
        ///  Overriding this method instead of DataBind() will allow the DataBound control developer
        ///  to not worry about DataBinding events to be called in the right order.
        /// </devdoc>
        protected internal virtual void PerformDataBinding(IEnumerable data) {
        }

        /// <summary>
        ///  Issues an asynchronous request for data to the data source using the arguments from CreateDataSourceSelectArguments.
        /// </summary>
        protected override void PerformSelect() {
            // We need to call OnDataBinding here if we're potentially bound to a DataSource (instead of a DataSourceID)
            // because the databinding statement that is the datasource needs to be evaluated before the call to GetData()
            // happens, because we don't rebind when the datasource is changed unless DataSourceID.Length > 0.
            if (!IsDataBindingAutomatic) {
                OnDataBinding(EventArgs.Empty);
            }
            DataSourceView view = GetData();
            _arguments = CreateDataSourceSelectArguments();
            _ignoreDataSourceViewChanged = true;
            RequiresDataBinding = false;
            MarkAsDataBound();
            view.Select(_arguments, OnDataSourceViewSelectCallback);
        }


        protected override void ValidateDataSource(object dataSource) {
            if ((dataSource == null) ||
                (dataSource is IListSource) ||
                (dataSource is IEnumerable) ||
                (dataSource is IDataSource)) {
                return;
            }
            throw new InvalidOperationException(SR.GetString(SR.DataBoundControl_InvalidDataSourceType));
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

        /// <summary>
        /// Updates the necessary properties on provided ModelDataSource with control's properties.
        /// This step must be done before the data source can do data-binding using data operations.
        /// </summary>
        /// <param name="modelDataSource"></param>
        internal virtual void UpdateModelDataSourceProperties(ModelDataSource modelDataSource) {
            Debug.Assert(modelDataSource != null, "A non-null ModelDataSource should be passed in");
            modelDataSource.UpdateProperties(ItemType, SelectMethod);
        }
    }
}

