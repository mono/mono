//------------------------------------------------------------------------------
// <copyright file="BaseDataBoundControl.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Web.Util;

    /// <summary>
    /// A BaseDataBoundControl is bound to a data source and generates its
    /// user interface (or child control hierarchy typically), by enumerating
    /// the items in the data source it is bound to.
    /// BaseDataBoundControl is an abstract base class that defines the common
    /// characteristics of all controls that use a list as a data source, such as
    /// DataGrid, DataBoundTable, ListBox etc. It encapsulates the logic
    /// of how a data-bound control binds to collections or DataControl instances.
    /// </summary>

    [
    Designer("System.Web.UI.Design.WebControls.BaseDataBoundControlDesigner, " + AssemblyRef.SystemDesign),
    DefaultProperty("DataSourceID")
    ]
    public abstract class BaseDataBoundControl : WebControl {

        private static readonly object EventDataBound = new object();

        private object _dataSource;
        private bool _requiresDataBinding;
        private bool _inited;
        private bool _preRendered;
        private bool _requiresBindToNull;
        private bool _throwOnDataPropertyChange;


        /// <summary>
        /// The data source to bind to. This allows a BaseDataBoundControl to bind
        /// to arbitrary lists of data items.
        /// </summary>
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
                return _dataSource;
            }
            set {
                if (value != null) {
                    ValidateDataSource(value);
                }
                _dataSource = value;
                OnDataPropertyChanged();
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
                if (String.IsNullOrEmpty(value) && !String.IsNullOrEmpty(DataSourceID)) {
                    _requiresBindToNull = true;
                }

                ViewState["DataSourceID"] = value;
                OnDataPropertyChanged();
            }
        }


        protected bool Initialized {
            get {
                return _inited;
            }
        }

        /// <summary>
        /// Returns true if the DataBoundControl uses Select/Update/Delete/Insert Methods for databinding. 
        /// Implementation on BaseDataBoundControl returns false.
        /// Override in child classes which support the above two properties.
        /// </summary>
        protected virtual bool IsUsingModelBinders {
           get {
               return false;
           }
        }

        /// <summary>
        /// This returns true only if the DataBoundControl is using a DataSourceID.
        /// Use the IsDataBindingAutomatic property to determine if the data bound control's data binding is
        /// automatic. The data binding is automatic if the control is using a DataSourceID or if control
        /// uses Select/Update/Delete/Insert Methods for data binding.
        /// </summary>
        protected bool IsBoundUsingDataSourceID {
            get {
                return (DataSourceID.Length > 0);
            }
        }

        /// <summary>
        /// This property is used by FormView/GridView/DetailsView/ListView in the following scenarios.
        /// 1. Perform an auto data bind (Listen to OnDataSourceViewChanged event and set RequiresDataBinding to true)
        /// 2. Calling the data source view operations
        /// 3. Raising exceptions when there is no DataSourceId (i.e., a DataSource is in use) and an event for Data Operation is not handled
        /// 4. Raising the ModeChanged events for DataControls
        /// This property is true if the control is bound using a DataSourceId or when the control participates in Model Binding.
        /// </summary>
        protected internal bool IsDataBindingAutomatic {
            get {
                return IsBoundUsingDataSourceID || IsUsingModelBinders;
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
                // if we have to play catch-up here because we've already PreRendered, call EnsureDataBound
                if (value && _preRendered && IsDataBindingAutomatic && Page != null && !Page.IsCallback) {
                    _requiresDataBinding = true;
                    EnsureDataBound();
                }
                else {
                    _requiresDataBinding = value;
                }
            }
        }


        [
        WebCategory("Data"),
        WebSysDescription(SR.BaseDataBoundControl_OnDataBound)
        ]
        public event EventHandler DataBound {
            add {
                Events.AddHandler(EventDataBound, value);
            }
            remove {
                Events.RemoveHandler(EventDataBound, value);
            }
        }

        protected void ConfirmInitState() {
            _inited = true; // do this in OnLoad in case we were added to the page after Page.OnPreLoad
        }


        /// <summary>
        /// Overriden by BaseDataBoundControl to use its properties to determine the real
        /// data source that the control should bind to. It then clears the existing
        /// control hierarchy, and calls CreateChildControls to create a new control
        /// hierarchy based on the resolved data source.
        /// The implementation resolves various data source related properties to
        /// arrive at the appropriate IEnumerable implementation to use as the real
        /// data source.
        /// When resolving data sources, the DataSourceID takes highest precedence.
        /// If DataSourceID is not set, the value of the DataSource property is used.
        /// In this second alternative, DataMember is used to extract the appropriate
        /// list if the control has been handed an IListSource as a data source.
        ///
        /// Data bound controls should override PerformDataBinding instead
        /// of DataBind.  If DataBind if overridden, the OnDataBinding and OnDataBound events will
        /// fire in the wrong order.  However, for backwards compat on ListControl and AdRotator, we 
        /// can't seal this method.  It is sealed on all new BaseDataBoundControl-derived controls.
        /// </summary>
        public override void DataBind() {
            // Don't databind when the control is in the designer but not top-level
            if (DesignMode) {
                IDictionary designModeState = GetDesignModeState();
                if (((designModeState == null) || (designModeState["EnableDesignTimeDataBinding"] == null))
                    && (Site == null)) {
                    return;
                }
            }

            PerformSelect();
        }


        protected virtual void EnsureDataBound() {
            try {
                _throwOnDataPropertyChange = true;
                if (RequiresDataBinding && (IsDataBindingAutomatic || _requiresBindToNull)) {
                    DataBind();
                    _requiresBindToNull = false;
                }
            }
            finally {
                _throwOnDataPropertyChange = false;
            }
        }


        protected virtual void OnDataBound(EventArgs e) {
            EventHandler handler = Events[EventDataBound] as EventHandler;
            if (handler != null) {
                handler(this, e);
            }
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

        protected virtual void OnPagePreLoad(object sender, EventArgs e) {
            _inited = true;
            if (Page != null) {
                Page.PreLoad -= new EventHandler(this.OnPagePreLoad);
            }
        }


        protected internal override void OnPreRender(EventArgs e) {
            _preRendered = true;
            EnsureDataBound();
            base.OnPreRender(e);
        }
        
        /// <summary>
        /// Override to control how the data is selected and the control is databound.
        /// </summary>
        protected abstract void PerformSelect();


        protected abstract void ValidateDataSource(object dataSource);
    }
}

