//---------------------------------------------------------------------
// <copyright file="EntityDataSource.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------
using System;
using System.Collections;
using System.Data;
using System.Data.Common;
using System.Data.Objects;
using System.Data.Metadata.Edm;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.DynamicData;
using System.Data.Objects.DataClasses;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Security.Permissions;
using System.Drawing;
using System.Text;
using System.Globalization;
using System.Web.Configuration;
    
[assembly:TagPrefix("System.Web.UI.WebControls", "asp")]
namespace System.Web.UI.WebControls
{
    [
    DefaultEvent("Selecting"),
    DefaultProperty("EntitySetName"),
    Designer("System.Web.UI.Design.WebControls.EntityDataSourceDesigner, " + AssemblyRef.SystemWebEntityDesign),
    ParseChildren(true),
    PersistChildren(false),
    ResourceDescription(WebControlsRes.EntityDataSource_Description),
    ResourceDisplayName(WebControlsRes.EntityDataSource_DisplayName),
    ToolboxBitmap(typeof(EntityDataSource), "EntityDataSource.ico"),
    ]
    public class EntityDataSource : DataSourceControl, System.Web.DynamicData.IDynamicDataSource, IQueryableDataSource
    {
        #region Constants

        private const int ORD_CONTROLSTATE = 0;
        private const int ORD_VIEW = 1;
        private const int ORD_WHERE_PARAMS = 2;
        private const int ORD_COMMAND_PARAMS = 3;
        private const int ORD_ORDERBY_PARAMS = 4;
        private const int ORD_DELETE_PARAMS = 5;
        private const int ORD_INSERT_PARAMS = 6;
        private const int ORD_UPDATE_PARAMS = 7;
        private const int ORD_SELECT_PARAMS = 8;

        #endregion

        #region Private Fields

        private string _contextTypeName;
        private string _entitySetName;
        private string _defaultContainerName;
        private string _where;
        private string _orderBy;
        private string _select;
        private string _commandText;
        private string _groupBy;
        private string _include;
        private string _entityTypeFilter;
        private string _connectionString;

        private ParameterCollection _commandParameters = null;
        private ParameterCollection _whereParameters = null;
        private ParameterCollection _orderByParameters = null;
        private ParameterCollection _deleteParameters = null;
        private ParameterCollection _updateParameters = null;
        private ParameterCollection _insertParameters = null;
        private ParameterCollection _selectParameters = null;

        private string _viewName = "EntityDataSourceView";
        private EntityDataSourceView _view = null;

        private bool _enableUpdate = false;
        private bool _enableDelete = false;
        private bool _enableInsert = false;
        private bool _autoSort = true;
        private bool _autoPage = true;
        private bool _autoGenerateWhereClause = false;
        private bool _autoGenerateOrderByClause = false;
        private bool _enableFlattening = true;
        private bool _storeOriginalValuesInViewState = true;
        private Type _contextType = null;

        private readonly System.Data.EntityClient.EntityConnection _connection;
        private readonly Version _targetFrameworkVersion;
        #endregion

        #region Public Surface

        #region Constructors
        public EntityDataSource()
        {
            _targetFrameworkVersion = HttpRuntime.TargetFramework;
        }

        public EntityDataSource(System.Data.EntityClient.EntityConnection connection)
            : this()
        {
            _connection = connection;
        }
        #endregion

        #region Public Properties

        /// <summary>
        /// Indicates whether the EntityDataSource is to automatically 
        /// generate an OrderBy expression using property name(s) and value(s) from
        /// the OrderByParameters.
        /// </summary>
        [
        DefaultValue(false),
        Category("Behavior"),
        ResourceDescription(WebControlsRes.PropertyDescription_AutoGenerateOrderByClause)
        ]
        public bool AutoGenerateOrderByClause
        {
            get { return _autoGenerateOrderByClause; }
            set
            {
                _autoGenerateOrderByClause = value;
                View.RaiseChangedEvent();
            }
        }


        /// <summary>
        /// Indicates whether the EntityDataSource is to automatically 
        /// generate a Where expression using property name(s) and value(s) from
        /// the WhereParameters.
        /// </summary>
        [
        DefaultValue(false),
        Category("Behavior"),
        ResourceDescription(WebControlsRes.PropertyDescription_AutoGenerateWhereClause)
        ]
        public bool AutoGenerateWhereClause
        {
            get { return _autoGenerateWhereClause; }
            set
            {
                _autoGenerateWhereClause = value;
                View.RaiseChangedEvent();
            }
        }

        /// <summary>
        /// Indicates to the EntityDataSource that the user wishes to perform paging.
        /// </summary>
        [
        DefaultValue(true),
        Category("Behavior"),
        ResourceDescription(WebControlsRes.PropertyDescription_AutoPage)
        ]
        public bool AutoPage
        {
            get { return _autoPage; }
            set
            {
                _autoPage = value;
                View.RaiseChangedEvent();
            }
        }

        /// <summary>
        /// Indicates to the EntityDataSource that the user wishes to perform sorting.
        /// </summary>
        [
        DefaultValue(true),
        Category("Behavior"),
        ResourceDescription(WebControlsRes.PropertyDescription_AutoSort)
        ]
        public bool AutoSort
        {
            get { return _autoSort; }
            set
            {
                _autoSort = value;
                View.RaiseChangedEvent();
            }
        }

        /// <summary>
        /// The name of the container. Required if DefaultConainerName is not set on the ObjectContext.
        /// </summary>        
        // devnote: Design-time attributes are not used here because this property is overridden by one in the designer
        public string DefaultContainerName
        {
            get { return _defaultContainerName; }
            set
            {
                _defaultContainerName = value;
                View.RaiseChangedEvent();
            }
        }

        internal System.Data.EntityClient.EntityConnection Connection
        {
            get { return _connection; }
        }

        /// <summary>
        /// ConnectionString is required if DefaultContainerName is defined and neither
        /// ContextType nor ContextTypeName are defined.
        /// </summary>
        public String ConnectionString
        {
            get { return _connectionString; }
            set
            {
                _connectionString = value;
                View.RaiseChangedEvent();
            }
        }

        
        
        // devnote: Design-time attributes are not used here because this property is not visible in the designer (it is filtered out with PreFilterProperties)
        /// <summary>
        /// Defined by the IDynamicDataSource interface.
        /// Provides a type to be used as the ObjectContext through which the EntityDataSource will 
        /// provide operations to the EntityFramework
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Type ContextType
        {
            get { return _contextType; }
            set
            {
                _contextType = value;
                if (null != value)
                {
                    _contextTypeName = value.FullName;
                }
                else
                {
                    _contextTypeName = null;
                }
                View.RaiseChangedEvent();
            }
        }

        
        /// <summary>
        /// The fully-qualified type name for the ObjectContext through which the EntityDataSource will 
        /// provide operations to the EntityFramework
        /// </summary>
        [
        DefaultValue(null),
        Category("Data"),
        ResourceDescription(WebControlsRes.PropertyDescription_ContextTypeName)
        ]
        public string ContextTypeName
        {
            get { return _contextTypeName; }
            set
            {
                _contextTypeName = value;

                if (!String.IsNullOrEmpty(value) && System.Web.Hosting.HostingEnvironment.IsHosted)
                {
                    _contextType = System.Web.Compilation.BuildManager.GetType(value, /*throwOnError*/false, /*ignoreCase*/true);
                }
                else
                {
                    _contextType = null;
                }
                View.RaiseChangedEvent();
            }
        }

        /// <summary>
        /// Indicates to the EntityDataSource that the user wishes entities to be flattened or not.
        /// </summary>
        [
        DefaultValue(true),
        Category("Behavior"),
        ResourceDescription(WebControlsRes.PropertyDescription_EnableFlattening)
        ]
        public bool EnableFlattening
        {
            get { return _enableFlattening; }
            set
            {
                _enableFlattening = value;
                View.RaiseChangedEvent();
            }
        }

        /// <summary>
        /// Provides default values for entities that are to be deleted.
        /// Sets the named properties to the provided values only if the properties are null 
        /// (not otherwise defined).
        /// </summary>
        [
        DefaultValue(null),
        MergableProperty(false),
        PersistenceMode(PersistenceMode.InnerProperty),
        Browsable(false)
        ]
        public ParameterCollection DeleteParameters
        {
            get
            {
                if (null == _deleteParameters)
                {
                    _deleteParameters = new ParameterCollection();
                    if (UseNetFramework4Behavior)
                    {
                        _deleteParameters.ParametersChanged += new EventHandler(this.OnParametersChanged);
                    }
                }
                return _deleteParameters;
            }
        }

        /// <summary>
        /// Indicates to the EntityDataSource that the user wishes to perform delete operations.
        /// </summary>
        [
        DefaultValue(false),
        Category("Behavior"),
        ResourceDescription(WebControlsRes.PropertyDescription_EnableDelete)
        ]
        public bool EnableDelete
        {
            get { return _enableDelete; }
            set
            {
                _enableDelete = value;
                View.RaiseChangedEvent();
            }
        }

        /// <summary>
        /// Indicates to the EntityDatSource that the user wishes to perform insert operations.
        /// </summary>
        [
        DefaultValue(false),
        Category("Behavior"),
        ResourceDescription(WebControlsRes.PropertyDescription_EnableInsert)
        ]
        public bool EnableInsert
        {
            get { return _enableInsert; }
            set
            {
                _enableInsert = value;
                View.RaiseChangedEvent();
            }
        }

        /// <summary>
        /// Indicates to the EntityDataSource that the user wishes to perform update operations
        /// </summary>
        [
        DefaultValue(false),
        Category("Behavior"),
        ResourceDescription(WebControlsRes.PropertyDescription_EnableUpdate)
        ]
        public bool EnableUpdate
        {
            get { return _enableUpdate; }
            set
            {
                _enableUpdate = value;
                View.RaiseChangedEvent();
            }
        }


        /// <summary>
        /// The name of the EntitySet used by this instance of the EntityDataSource control.
        /// For editable scenarios, the EntitySetName is used as the EntitySql query expression.
        /// All insert, update and delete operations are restricted to a single EntitySet.
        /// </summary>
        // devnote: Design-time attributes are not used here because this property is overridden by one in the designer
        public string EntitySetName
        {
            get { return _entitySetName; }
            set
            {
                _entitySetName = value;
                View.RaiseChangedEvent();
            }
        }

        /// <summary>
        /// An arbitrary EntitySql CommandText for performing the query.
        /// A query specified with CommandText is not editable.
        /// </summary>
        // devnote: Design-time attributes are not used here because this property is overridden by one in the designer
        public string CommandText
        {
            get { return _commandText; }
            set
            {
                _commandText = value;
                View.RaiseChangedEvent();
            }
        }

        /// <summary>
        /// Named parameters to be used with the CommandText.
        /// Corresponds to the ObjectParameters used in the ObjectQuery<T> query.
        /// Null values are passed into the ObjectParameter collection as the Type of the Parameter.
        /// </summary>
        [
        DefaultValue(null),
        MergableProperty(false),
        PersistenceMode(PersistenceMode.InnerProperty),
        Browsable(false)
        ]
        public ParameterCollection CommandParameters
        {
            get
            {
                if (null == _commandParameters)
                {
                    _commandParameters = new ParameterCollection();
                    _commandParameters.ParametersChanged += new EventHandler(this.OnParametersChanged);
                }
                return _commandParameters;
            }
        }

        internal string FQEntitySetName
        {
            get
            {
                if (!String.IsNullOrEmpty(DefaultContainerName))
                {
                    return DefaultContainerName + "." + EntitySetName;
                }
                return EntitySetName;
            }
        }

        /// <summary>
        /// The expression provided to the GroupBy ObjectQuery<T> builder method.
        /// GroupBy expression requires Select to be defined.
        /// These projections are not editable.
        /// </summary>
        [
        Category("Data"),        
        DefaultValue(null),
        ResourceDescription(WebControlsRes.PropertyDescription_GroupBy),
        ]        
        public string GroupBy
        {
            get { return _groupBy; }
            set
            {
                _groupBy = value;
                View.RaiseChangedEvent();
            }
        }

        /// <summary>
        /// An expression approxaimately corresponding to the Include method on the ObjectQuery<T>.
        /// Gets or sets an expression describing which navigations should be included in the query.
        /// To describe a chain of navigations, use dots (e.g. "Orders.OrderDetails"). To include multiple
        /// paths, use commas (e.g. "Orders.OrderDetails, Supplies").
        /// </summary>
        [
        DefaultValue(null),
        Category("Data"),
        ResourceDescription(WebControlsRes.PropertyDescription_Include)
        ]
        public string Include
        {
            get
            {
                return _include;
            }
            set
            {
                _include = value;
                View.RaiseChangedEvent();
            }
        }


        /// <summary>
        /// Provides default values for inserted entities.
        /// Properties that are null (not otherwise defined) are set to the value specified
        /// by InsertParameters.
        /// </summary>
        [
        DefaultValue(null),
        MergableProperty(false),
        PersistenceMode(PersistenceMode.InnerProperty),
        Browsable(false)
        ]
        public ParameterCollection InsertParameters
        {
            get
            {
                if (null == _insertParameters)
                {
                    _insertParameters = new ParameterCollection();
                    if (UseNetFramework4Behavior)
                    {
                        _insertParameters.ParametersChanged += new EventHandler(this.OnParametersChanged);
                    }
                }
                return _insertParameters;
            }
        }

        // devnote: Design-time attributes are not used here because this property is overridden by one in the designer
        
        /// <summary>
        /// Provides a sort expression corresonding to the OrderBy method on the ObjectQuery<T>
        /// </summary>
        public string OrderBy
        {
            get { return _orderBy; }
            set
            {
                _orderBy = value;
                View.RaiseChangedEvent();
            }
        }

        /// <summary>
        /// Each Parameter is mapped to as named ObjectParameter in the ObjectQuery<T>
        /// If a null value is set on the Parameter, then the Type is passed in as the
        /// ObjectParameter.
        /// </summary>
        [
        DefaultValue(null),
        MergableProperty(false),
        PersistenceMode(PersistenceMode.InnerProperty),
        Browsable(false)
        ]
        public ParameterCollection OrderByParameters
        {
            get
            {
                if (null == _orderByParameters)
                {
                    _orderByParameters = new ParameterCollection();
                    _orderByParameters.ParametersChanged += new EventHandler(this.OnParametersChanged);
                }
                return _orderByParameters;
            }
        }

        // devnote: Design-time attributes are not used here because this property is overridden by one in the designer
        /// <summary>
        /// Forces the EntityDatSource to return entities of only a single derived type.
        /// If the EntitySet provided as the query expression is polymorphic, then EntityTypeFilter
        /// is required if the collection is to be editable.
        /// </summary>
        public string EntityTypeFilter
        {
            get { return _entityTypeFilter; }
            set
            {
                _entityTypeFilter = value;
                View.RaiseChangedEvent();
            }
        }

        /// <summary>
        /// Text for the Select query builder method.
        /// Projections are not editable in the EntityDataSource control.
        /// </summary>
        // devnote: Design-time attributes are not used here because this property is overridden by one in the designer
        public string Select
        {
            get  { return _select;  }
            set
            {
                _select = value;
                View.RaiseChangedEvent();
            }
        }
        /// <summary>
        /// Each Parameter is mapped to an ObjectParameter in the ObjectQuery<T>
        /// If a null value is set on the Parameter, then the Type is passed in as the
        /// named ObjectParameter.
        /// </summary>
        [
        DefaultValue(null),
        MergableProperty(false),
        PersistenceMode(PersistenceMode.InnerProperty),
        Browsable(false)
        ]
        public ParameterCollection SelectParameters
        {
            get
            {
                if (null == _selectParameters)
                {
                    _selectParameters = new ParameterCollection();
                    _selectParameters.ParametersChanged += new EventHandler(this.OnParametersChanged);
                }
                return _selectParameters;
            }
        }

        /// <summary>
        /// Setting this value to false disables storing original values in ViewState.
        /// Setting this value to false implies that the user understands the concurrency model in the 
        /// EntityFramework and the update behavior of the EntityDataSource. Its use should be 
        /// reserved for expert users only.
        /// </summary>
        [
        DefaultValue(true),
        Category("Behavior"),
        ResourceDescription(WebControlsRes.PropertyDescription_StoreOriginalValuesInViewState)
        ]
        public bool StoreOriginalValuesInViewState
        {
            get { return _storeOriginalValuesInViewState; }
            set
            {
                _storeOriginalValuesInViewState = value;
                View.RaiseChangedEvent();
            }
        }

        /// <summary>
        /// Provides default values to be used during updates. The values provided by UpdateParameters
        /// are used for properties on the entity when the properties are null
        /// </summary>
        [
        DefaultValue(null),
        MergableProperty(false),
        PersistenceMode(PersistenceMode.InnerProperty),
        Browsable(false)
        ]
        public ParameterCollection UpdateParameters
        {
            get
            {
                if (null == _updateParameters)
                {
                    _updateParameters = new ParameterCollection();
                    if (UseNetFramework4Behavior)
                    {
                        _updateParameters.ParametersChanged += new EventHandler(this.OnParametersChanged);
                    }
                }
                return _updateParameters;
            }
        }

        /// <summary>
        /// The text provided to the Where method on the ObjectQuery<T>
        /// </summary>
        // devnote: Design-time attributes are not used here because this property is overridden by one in the designer
        public string Where
        {
            get { return _where;  }
            set
            {
                _where = value;
                View.RaiseChangedEvent();
            }
        }

        /// <summary>
        /// Each Parameter is mapped to an ObjectParameter in the ObjectQuery<T>
        /// If a null value is set on the Parameter, then the Type is passed in as the
        /// named ObjectParameter.
        /// </summary>
        [
        DefaultValue(null),
        MergableProperty(false),
        PersistenceMode(PersistenceMode.InnerProperty),
        Browsable(false)
        ]
        public ParameterCollection WhereParameters
        {
            get
            {
                if (null == _whereParameters)
                {
                    _whereParameters = new ParameterCollection();
                    _whereParameters.ParametersChanged += new EventHandler(this.OnParametersChanged);
                }
                return _whereParameters;
            }
        }

        #endregion

        #endregion

        #region Property Getters

        private ObjectParameter[] CreateObjectParametersFromParameterCollection(ParameterCollection paramColl)
        {
            IOrderedDictionary paramValues = paramColl.GetValues(HttpContext, this);

            List<ObjectParameter>  objectParameters = new List<ObjectParameter>();
            foreach (Parameter parameter in paramColl)
            {
                if (!string.IsNullOrEmpty(parameter.Name))
                {
                    WebControlParameterProxy wcParam = new WebControlParameterProxy(parameter, paramColl, this);

                    if (wcParam.Value != null)
                    {
                        objectParameters.Add(new ObjectParameter(wcParam.Name, wcParam.Value));
                    }
                    else
                    {
                        objectParameters.Add(new ObjectParameter(wcParam.Name, wcParam.ClrType));
                    }
                }
            }
            return objectParameters.ToArray();
        }


        internal ObjectParameter[] GetOrderByParameters()
        {
            return CreateObjectParametersFromParameterCollection(OrderByParameters);
        }

        internal ObjectParameter[] GetWhereParameters()
        {
            return CreateObjectParametersFromParameterCollection(WhereParameters);
        }

        // CommandParameters may be set in selectArgs
        internal ObjectParameter[] GetCommandParameters()
        {
            return CreateObjectParametersFromParameterCollection(CommandParameters);
        }

        internal ObjectParameter[] GetSelectParameters()
        {
            return CreateObjectParametersFromParameterCollection(SelectParameters);
        }

        #endregion

        #region DataSourceControl overrides
        protected override DataSourceView GetView(string viewName)
        {
            return View;
        }
        protected override ICollection GetViewNames()
        {
            return new string[] { this._viewName };
        }

        #endregion

        #region Private Properties
        private EntityDataSourceView View
        {
            get
            {
                if (null == _view)
                {
                    _view = CreateView();
                    if (IsTrackingViewState)
                    {
                        ((IStateManager)_view).TrackViewState();
                    }
                }
                return _view;
            }
        }

        /// <summary>
        /// Users can override this method to control the creation of the data source view.
        /// </summary>
        /// <returns>An instance of EntityDataSourceView</returns>
        protected virtual EntityDataSourceView CreateView()
        {
            return new EntityDataSourceView(this, _viewName);
        }

        internal HttpContext HttpContext
        {
            get
            {
                return base.Context;
            }
        }

        private bool UseNetFramework4Behavior
        {
            get
            {
                return _targetFrameworkVersion == new Version(4, 0);
            }
        }

        #endregion Private Properties

        #region IStateManager overrides

        protected override object SaveControlState()
        {
            // Order is sensitive, referenced by LoadControlState.
            var state = new object[9];
            state[ORD_CONTROLSTATE] = base.SaveControlState();
            state[ORD_VIEW] = _view == null ? null : ((IStateManager)_view).SaveViewState();
            state[ORD_WHERE_PARAMS] = SaveParametersViewState(_whereParameters);
            state[ORD_COMMAND_PARAMS] = SaveParametersViewState(_commandParameters);
            state[ORD_ORDERBY_PARAMS] = SaveParametersViewState(_orderByParameters);

            if (UseNetFramework4Behavior)
            {
                state[ORD_DELETE_PARAMS] = SaveParametersViewState(_deleteParameters);
                state[ORD_INSERT_PARAMS] = SaveParametersViewState(_insertParameters);
                state[ORD_UPDATE_PARAMS] = SaveParametersViewState(_updateParameters);
            }

            state[ORD_SELECT_PARAMS] = SaveParametersViewState(_selectParameters);
            return state;
        }

        private object SaveParametersViewState(ParameterCollection parameters)
        {
            if (parameters != null) 
            {
                return ((IStateManager)parameters).SaveViewState();
            }
            return null;
        }

        protected override void LoadControlState(object savedState)
        {
            if (null == savedState)
            {
                base.LoadControlState(null);
            }
            else // (savedState != null)
            {
                // Order is sensitive, referenced by SaveControlState.
                var state = (object[])savedState;
                if (state[ORD_CONTROLSTATE] != null)
                {
                    base.LoadControlState(state[ORD_CONTROLSTATE]);
                }
                if (state[ORD_VIEW] != null)
                {
                    ((IStateManager)View).LoadViewState(state[ORD_VIEW]);
                }
                if (state[ORD_WHERE_PARAMS] != null)
                {
                    ((IStateManager)WhereParameters).LoadViewState(state[ORD_WHERE_PARAMS]);
                }
                if (state[ORD_COMMAND_PARAMS] != null)
                {
                    ((IStateManager)CommandParameters).LoadViewState(state[ORD_COMMAND_PARAMS]);
                }
                if (state[ORD_ORDERBY_PARAMS] != null)
                {
                    ((IStateManager)OrderByParameters).LoadViewState(state[ORD_ORDERBY_PARAMS]);
                }
                if (UseNetFramework4Behavior)
                {
                    if (state[ORD_DELETE_PARAMS] != null)
                    {
                        ((IStateManager)DeleteParameters).LoadViewState(state[ORD_DELETE_PARAMS]);
                    }
                    if (state[ORD_INSERT_PARAMS] != null)
                    {
                        ((IStateManager)InsertParameters).LoadViewState(state[ORD_INSERT_PARAMS]);
                    }
                    if (state[ORD_UPDATE_PARAMS] != null)
                    {
                        ((IStateManager)UpdateParameters).LoadViewState(state[ORD_UPDATE_PARAMS]);
                    }
                }
                if (state[ORD_SELECT_PARAMS] != null)
                {
                    ((IStateManager)SelectParameters).LoadViewState(state[ORD_SELECT_PARAMS]);
                }
            }
        }

        protected override void TrackViewState()
        {
            base.TrackViewState();
            ((IStateManager)View).TrackViewState();
            ((IStateManager)WhereParameters).TrackViewState();
            ((IStateManager)CommandParameters).TrackViewState();
            ((IStateManager)OrderByParameters).TrackViewState();
            if (UseNetFramework4Behavior)
            {
                ((IStateManager)DeleteParameters).TrackViewState();
                ((IStateManager)InsertParameters).TrackViewState();
                ((IStateManager)UpdateParameters).TrackViewState();
            }
            ((IStateManager)SelectParameters).TrackViewState();
        }

        #endregion

        #region Events
        event EventHandler<DynamicValidatorEventArgs> IDynamicDataSource.Exception
        {
            add { View.Exception += value; }
            remove { View.Exception -= value; }
        }

        /// <summary>
        /// An event that is fired just prior to the creation of the ObjectContext.
        /// The user can provide their own context here.
        /// </summary>
        [
        Category("Data"),
        ResourceDescription(WebControlsRes.PropertyDescription_ContextCreating)
        ]
        public event EventHandler<EntityDataSourceContextCreatingEventArgs> ContextCreating
        {
            add { View.ContextCreating += value; }
            remove { View.ContextCreating -= value; }
        }

        /// <summary>
        /// An event that is fired just following the creation of the ObjectContext to provide
        /// the user with a reference to the created context.
        /// </summary>
        [
        Category("Data"),
        ResourceDescription(WebControlsRes.PropertyDescription_ContextCreated)
        ]
        public event EventHandler<EntityDataSourceContextCreatedEventArgs> ContextCreated
        {
            add { View.ContextCreated += value; }
            remove { View.ContextCreated -= value; }
        }

        /// <summary>
        /// An event fired just prior to the ObjectContext being disposed.
        /// It is cancellable in case the user needs to hold onto a reference to the Context.
        /// </summary>
        [
        Category("Data"),
        ResourceDescription(WebControlsRes.PropertyDescription_ContextDisposing)
        ]
        public event EventHandler<EntityDataSourceContextDisposingEventArgs> ContextDisposing
        {
            add { View.ContextDisposing += value; }
            remove { View.ContextDisposing -= value; }
        }

        /// <summary>
        /// An event fired prior to the execution of the query in the ExecuteSelect method. 
        /// The user can modify the properties of the
        /// EntityDataSource to modify its behavior.
        /// The user can cancel the execution of the query in this event.
        /// </summary>
        [
        Category("Data"),
        ResourceDescription(WebControlsRes.PropertyDescription_Selecting)
        ]
        public event EventHandler<EntityDataSourceSelectingEventArgs> Selecting
        {
            add { View.Selecting += value; }
            remove { View.Selecting -= value; }
        }

        /// <summary>
        /// An event that is fired after the query has been executed in the ExecuteSelect method.
        /// The event provides the collection of returned entities for inspection or modification prior to display.
        /// </summary>
        [
        Category("Data"),
        ResourceDescription(WebControlsRes.PropertyDescription_Selected)
        ]
        public event EventHandler<EntityDataSourceSelectedEventArgs> Selected
        {
            add { View.Selected += value; }
            remove { View.Selected -= value; }
        }

        /// <summary>
        /// An event fired just prior to deleting an object from the database.
        /// The object is provided so the user can inspect or modify it.
        /// The user can cancel the deletion.
        /// </summary>
        [
        Category("Data"),
        ResourceDescription(WebControlsRes.PropertyDescription_Deleting)
        ]
        public event EventHandler<EntityDataSourceChangingEventArgs> Deleting
        {
            add { View.Deleting += value; }
            remove { View.Deleting -= value; }
        }

        /// <summary>
        /// An event fired just after the entity has been deleted from the database.
        /// </summary>
        [
        Category("Data"),
        ResourceDescription(WebControlsRes.PropertyDescription_Deleted)
        ]
        public event EventHandler<EntityDataSourceChangedEventArgs> Deleted
        {
            add { View.Deleted += value; }
            remove { View.Deleted -= value; }
        }

        /// <summary>
        /// An event fired just prior to the insertion of an entity into the database.
        /// The user is provided with the entity for modification prior to insertion.
        /// The insertion is cancellable.
        /// </summary>
        [
        Category("Data"),
        ResourceDescription(WebControlsRes.PropertyDescription_Inserting)
        ]
        public event EventHandler<EntityDataSourceChangingEventArgs> Inserting
        {
            add { View.Inserting += value; }
            remove { View.Inserting -= value; }
        }

        /// <summary>
        /// An event fired just after the entity has been inserted into the database.
        /// </summary>
        [
        Category("Data"),
        ResourceDescription(WebControlsRes.PropertyDescription_Inserted)
        ]
        public event EventHandler<EntityDataSourceChangedEventArgs> Inserted
        {
            add { View.Inserted += value; }
            remove { View.Inserted -= value; }
        }

        /// <summary>
        /// An event fired just after a modified entity has been updated in the database.
        /// </summary>
        [
        Category("Data"),
        ResourceDescription(WebControlsRes.PropertyDescription_Updated)
        ]
        public event EventHandler<EntityDataSourceChangedEventArgs> Updated
        {
            add { View.Updated += value; }
            remove { View.Updated -= value; }
        }

        /// <summary>
        /// An event fired just prior to saving a modified entity to the database.
        /// The entity is provided to the event for modification.
        /// The update is cancellable.
        /// </summary>
        [
        Category("Data"),
        ResourceDescription(WebControlsRes.PropertyDescription_Updating)
        ]
        public event EventHandler<EntityDataSourceChangingEventArgs> Updating
        {
            add { View.Updating += value; }
            remove { View.Updating -= value; }
        }

        #region IQueryableDataSource Members

        [
        Category("Data"),
        ResourceDescription(WebControlsRes.PropertyDescription_QueryCreated)
        ]
        public event EventHandler<QueryCreatedEventArgs> QueryCreated
        {
            add { View.QueryCreated += value; }
            remove { View.QueryCreated -= value; }
        }

        void IQueryableDataSource.RaiseViewChanged()
        {
            View.RaiseChangedEvent();
        }

        #endregion
        
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", MessageId = "0#")]
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            Debug.Assert(Page != null);
            Page.LoadComplete += new EventHandler(this.OnPageLoadComplete);
            if (StoreOriginalValuesInViewState && (View.CanDelete || View.CanUpdate))
            {
                Page.RegisterRequiresViewStateEncryption();
            }
            Page.RegisterRequiresControlState(this);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", MessageId = "0#")]
        protected override void OnUnload(EventArgs e)
        {
            base.OnUnload(e);
            if (null != _view) //Don't want to call View and create a new view during unload.
            {
                _view.DisposeContext();
            }
        }

        private void OnPageLoadComplete(object sender, EventArgs e)
        {
            CommandParameters.UpdateValues(HttpContext, this);
            WhereParameters.UpdateValues(HttpContext, this);
            OrderByParameters.UpdateValues(HttpContext, this);
            SelectParameters.UpdateValues(HttpContext, this);
        }

        private void OnParametersChanged(object sender, EventArgs e)
        {
            View.RaiseChangedEvent();
        }
        #endregion

        #region Error Checking
        internal bool ValidateUpdatableConditions()
        {
            bool anyEditablesEnabled = EnableInsert || EnableUpdate || EnableDelete;

            // Cannot edit of EntitySetName has not been set.
            // Cannot edit if CommandText has been set.
            // Cannot edit if all EnableDelete/Insert/Update are false.
            // Cannot edit if Select has been set
            // Note that neither EntitySetName nor CommandText are strictly required if the user provides a query from OnSelecting.
            bool disableUpdatableness =
                String.IsNullOrEmpty(EntitySetName) ||
                !String.IsNullOrEmpty(CommandText) ||
                !anyEditablesEnabled ||
                !String.IsNullOrEmpty(Select) ||
                !String.IsNullOrEmpty(GroupBy);

            if (!String.IsNullOrEmpty(CommandText) &&
                !String.IsNullOrEmpty(EntitySetName))
            {
                throw new InvalidOperationException(Strings.EntityDataSource_CommandTextOrEntitySetName);
            }

            if (String.IsNullOrEmpty(CommandText) && 
                String.IsNullOrEmpty(EntitySetName))
            {
                throw new InvalidOperationException(Strings.EntityDataSource_CommandTextOrEntitySetNameRequired);
            }

            if (anyEditablesEnabled && !String.IsNullOrEmpty(CommandText))
            {
                throw new InvalidOperationException(Strings.EntityDataSource_CommandTextNotEditable);
            }

            if (anyEditablesEnabled && !String.IsNullOrEmpty(Select))
            {
                throw new InvalidOperationException(Strings.EntityDataSource_SelectNotEditable);
            }

            if (anyEditablesEnabled && !String.IsNullOrEmpty(GroupBy))
            {
                throw new InvalidOperationException(Strings.EntityDataSource_GroupByNotEditable);
            }

            if (!String.IsNullOrEmpty(Where) && AutoGenerateWhereClause)
            {
                throw new InvalidOperationException(Strings.EntityDataSource_AutoGenerateWhereNotAllowedIfWhereDefined);
            }

            if (!String.IsNullOrEmpty(OrderBy) && AutoGenerateOrderByClause)
            {
                throw new InvalidOperationException(Strings.EntityDataSource_AutoGenerateOrderByNotAllowedIfOrderByIsDefined);
            }

            if (0 < WhereParameters.Count && !AutoGenerateWhereClause && String.IsNullOrEmpty(Where))
            {
                throw new InvalidOperationException(Strings.EntityDataSource_WhereParametersNeedsWhereOrAutoGenerateWhere);
            }

            if (0 < OrderByParameters.Count && !AutoGenerateOrderByClause && String.IsNullOrEmpty(OrderBy))
            {
                throw new InvalidOperationException(Strings.EntityDataSource_OrderByParametersNeedsOrderByOrAutoGenerateOrderBy);
            }

            if (0 < CommandParameters.Count && String.IsNullOrEmpty(CommandText))
            {
                throw new InvalidOperationException(Strings.EntityDataSource_CommandParametersNeedCommandText);
            }

            if (0 < SelectParameters.Count && String.IsNullOrEmpty(Select))
            {
                throw new InvalidOperationException(Strings.EntityDataSource_SelectParametersNeedSelect);
            }

            if (!String.IsNullOrEmpty(GroupBy) && String.IsNullOrEmpty(Select))
            {
                throw new InvalidOperationException(Strings.EntityDataSource_GroupByNeedsSelect);
            }

            if (!String.IsNullOrEmpty(EntityTypeFilter) && !String.IsNullOrEmpty(CommandText))
            {
                throw new InvalidOperationException(Strings.EntityDataSource_CommandTextCantHaveEntityTypeFilter);
            }

            if (!String.IsNullOrEmpty(EntitySetName))
            {
                View.ValidateEntitySetName();
            }

            return disableUpdatableness;
        }

        internal bool ValidateWrappable()
        {
            return
                EnableFlattening &&
                HasIdentity();
        }

        /// <summary>
        /// Determines if the EntityDataSource is configured to return results that have an identity or not
        /// (i.e. the entities have some set of primary keys)
        /// </summary>
        internal bool HasIdentity()
        {
            return
                String.IsNullOrEmpty(CommandText) &&
                String.IsNullOrEmpty(Select) &&
                String.IsNullOrEmpty(GroupBy);
        }

        #endregion Error Checking
    }
}
