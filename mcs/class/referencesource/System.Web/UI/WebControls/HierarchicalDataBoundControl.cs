//------------------------------------------------------------------------------
// <copyright file="HierarchicalDataBoundControl.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {
    using System.Collections;
    using System.ComponentModel;
    using System.Web.Util;
    using System.Web.UI.WebControls.Adapters;


    [
    Designer("System.Web.UI.Design.WebControls.HierarchicalDataBoundControlDesigner, " + AssemblyRef.SystemDesign)
    ]
    public abstract class HierarchicalDataBoundControl : BaseDataBoundControl {

        private IHierarchicalDataSource _currentHierarchicalDataSource;
        private bool _currentDataSourceIsFromControl;
        private bool _currentDataSourceValid;
        private bool _pagePreLoadFired;
        
        private const string DataBoundViewStateKey = "_!DataBound";


        /// <internalonly />
        [IDReferenceProperty(typeof(HierarchicalDataSourceControl))]
        public override string DataSourceID {
            get {
                return base.DataSourceID;
            }
            set {
                base.DataSourceID = value;
            }
        }

        /// <devdoc>
        /// Connects this hierarchical data bound control to the appropriate
        /// HierarchicalDataSource and hooks up the appropriate event listener
        /// for the DataSourceChanged event. The return value is the new data source
        /// (if any) that was connected to. An exception is thrown if there is
        /// a problem finding the requested data source.
        /// </devdoc>
        private IHierarchicalDataSource ConnectToHierarchicalDataSource() {
            if (_currentDataSourceValid && !DesignMode) {
                // Ensure that both DataSourceID as well as DataSource are not set at the same time
                if (!_currentDataSourceIsFromControl && DataSourceID != null && DataSourceID.Length != 0) {
                    throw new InvalidOperationException(SR.GetString(SR.DataControl_MultipleDataSources, ID));
                }
                // If the current view is correct, there is no need to reconnect
                return _currentHierarchicalDataSource;
            }

            // Disconnect from old view, if necessary
            if ((_currentHierarchicalDataSource != null) && (_currentDataSourceIsFromControl)) {
                // We only care about this event if we are bound through the DataSourceID property
                _currentHierarchicalDataSource.DataSourceChanged -= new EventHandler(OnDataSourceChanged);
            }

            // Connect to new view
            _currentHierarchicalDataSource = GetDataSource();
            _currentDataSourceIsFromControl = IsBoundUsingDataSourceID;

            if (_currentHierarchicalDataSource == null) {
                // HierarchicalDataSource control was not found, construct a temporary data source to wrap the data
                _currentHierarchicalDataSource = new ReadOnlyHierarchicalDataSource(DataSource);
            }
            else {
                // Ensure that both DataSourceID as well as DataSource are not set at the same time
                if (DataSource != null) {
                    throw new InvalidOperationException(SR.GetString(SR.DataControl_MultipleDataSources, ID));
                }
            }

            _currentDataSourceValid = true;
            
            if ((_currentHierarchicalDataSource != null) && (_currentDataSourceIsFromControl)) {
                // We only care about this event if we are bound through the DataSourceID property
                _currentHierarchicalDataSource.DataSourceChanged += new EventHandler(OnDataSourceChanged);
            }

            return _currentHierarchicalDataSource;
        }


        /// <devdoc>
        /// Gets the HierarchicalDataSourceView of the IHierarchicalDataSource
        /// that this control is bound to, if any.
        /// </devdoc>
        protected virtual HierarchicalDataSourceView GetData(string viewPath) {
            string currentViewPath = viewPath;
            IHierarchicalDataSource ds = ConnectToHierarchicalDataSource();
            Debug.Assert(_currentDataSourceValid);

            Debug.Assert(ds != null);

            // IHierarchicalDataSource was found, extract the appropriate view and return it
            HierarchicalDataSourceView view = ds.GetHierarchicalView(currentViewPath);
            if (view == null) {
                throw new InvalidOperationException(SR.GetString(SR.HierarchicalDataControl_ViewNotFound, ID));
            }
            return view;
        }


        /// <devdoc>
        /// Gets the IHierarchicalDataSource that this control is bound to, if any.
        /// </devdoc>
        protected virtual IHierarchicalDataSource GetDataSource() {
            if (!DesignMode && _currentDataSourceValid && (_currentHierarchicalDataSource != null)) {
                return _currentHierarchicalDataSource;
            }

            IHierarchicalDataSource ds = null;
            string dataSourceID = DataSourceID;

            if (dataSourceID.Length != 0) {
                // Try to find a DataSource control with the ID specified in DataSourceID
                Control control = DataBoundControlHelper.FindControl(this, dataSourceID);
                if (control == null) {
                    throw new HttpException(SR.GetString(SR.HierarchicalDataControl_DataSourceDoesntExist, ID, dataSourceID));
                }
                ds = control as IHierarchicalDataSource;
                if (ds == null) {
                    throw new HttpException(SR.GetString(SR.HierarchicalDataControl_DataSourceIDMustBeHierarchicalDataControl, ID, dataSourceID));
                }
            }
            return ds;
        }

        protected void MarkAsDataBound() {
            ViewState[DataBoundViewStateKey] = true;
        }
        

        protected override void OnDataPropertyChanged() {
            _currentDataSourceValid = false;
            base.OnDataPropertyChanged();
        }


        /// <devdoc>
        ///  This method is called when the IHierarchicalDataSource raises a DataSourceChanged event.
        /// </devdoc>
        protected virtual void OnDataSourceChanged(object sender, EventArgs e) {
            RequiresDataBinding = true;
        }
        

        protected internal override void OnLoad(EventArgs e) {
            ConfirmInitState();
            ConnectToHierarchicalDataSource();

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
        protected internal virtual void PerformDataBinding() {
        }

        /// <summary>
        ///  Issues an asynchronous request for data to the data source using the arguments from CreateDataSourceSelectArguments.
        /// </summary>
        protected override void PerformSelect() {
            OnDataBinding(EventArgs.Empty);
            if (AdapterInternal != null) {
                HierarchicalDataBoundControlAdapter hierarchicalAdapter = AdapterInternal as HierarchicalDataBoundControlAdapter;
                if(hierarchicalAdapter != null) {
                    hierarchicalAdapter.PerformDataBinding();
                }
                else {
                    PerformDataBinding();
                }
            }
            else {
                PerformDataBinding();
            }
            RequiresDataBinding = false;
            MarkAsDataBound();
            OnDataBound(EventArgs.Empty);
        }
        

        protected override void ValidateDataSource(object dataSource) {
            if ((dataSource == null) ||
                (dataSource is IHierarchicalEnumerable) ||
                (dataSource is IHierarchicalDataSource)) {
                return;
            }
            throw new InvalidOperationException(SR.GetString(SR.HierarchicalDataBoundControl_InvalidDataSource));
        }
    }
}
