namespace System.Web.DynamicData {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Globalization;
    using System.Security.Permissions;
    using System.Web.Resources;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using System.Web.DynamicData.Util;
    using System.Data.Objects;
    using IDataBoundControlInterface = System.Web.UI.WebControls.IDataBoundControl;

    /// <summary>
    /// Adds behavior to certain control to make them work with Dynamic Data 
    /// </summary>
    [NonVisualControl()]
    [ParseChildren(true)]
    [PersistChildren(false)]
    [ToolboxBitmap(typeof(DynamicDataManager), "DynamicDataManager.bmp")]
    [Designer("System.Web.DynamicData.Design.DynamicDataManagerDesigner, " + AssemblyRef.SystemWebDynamicDataDesign)]
    public class DynamicDataManager : Control {
        private DataControlReferenceCollection _dataControls;
        // Key is used as the set of registered data source controls.  Value is ignored.
        private Dictionary<IDynamicDataSource, object> _dataSources = new Dictionary<IDynamicDataSource, object>();

        /// <summary>
        /// Causes foreign entities to be loaded as well setting the proper DataLoadOptions.
        /// Only works with Linq To Sql.
        /// </summary>
        [
        Category("Behavior"),
        DefaultValue(false),
        ResourceDescription("DynamicDataManager_AutoLoadForeignKeys")
        ]
        public bool AutoLoadForeignKeys {
            get;
            set;
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        EditorBrowsable(EditorBrowsableState.Never)
        ]
        public override string ClientID {
            get {
                return base.ClientID;
            }
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        EditorBrowsable(EditorBrowsableState.Never)
        ]
        public override ClientIDMode ClientIDMode {
            get {
                return base.ClientIDMode;
            }
            set {
                throw new NotImplementedException();
            }
        }

        [
        Category("Behavior"),
        DefaultValue(null),
        PersistenceMode(PersistenceMode.InnerProperty),
        MergableProperty(false),
        ]
        public DataControlReferenceCollection DataControls {
            get {
                if (_dataControls == null) {
                    _dataControls = new DataControlReferenceCollection(this);
                }
                return _dataControls;
            }
        }

        /// <summary>
        /// See base class documentation
        /// </summary>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        EditorBrowsable(EditorBrowsableState.Never)
        ]
        public override bool Visible {
            get {
                return base.Visible;
            }
            set {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// See base class documentation
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers")]
        protected override void OnInit(EventArgs e) {
            base.OnInit(e);

            // Initialize the collection
            DataControls.Initialize();

            // Subscribe to the Page's Init to register the controls set in the DataControls collection
            Page.Init += OnPageInit;
        }


        private void OnPageInit(object sender, EventArgs e) {
            foreach (DataControlReference controlReference in DataControls) {
                Control targetControl = Misc.FindControl(this, controlReference.ControlID);
                if (targetControl == null) {
                    throw new InvalidOperationException(
                        String.Format(CultureInfo.CurrentCulture,
                        DynamicDataResources.DynamicDataManager_ControlNotFound,
                        controlReference.ControlID));
                }

                RegisterControl(targetControl);
            }
        }

        /// <summary>
        /// See base class documentation
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers")]
        protected override void OnLoad(EventArgs e) {
            base.OnLoad(e);

            // Go through all the registered data sources
            foreach (IDynamicDataSource dataSource in _dataSources.Keys) {

                // Expand any dynamic where parameters that they may use
                dataSource.ExpandDynamicWhereParameters();
            }
        }

        /// <summary>
        /// Register a data control to give it Dynamic Data behavior
        /// </summary>
        /// <param name="control"></param>
        public void RegisterControl(Control control) {
            RegisterControl(control, false);
        }

        /// <summary>
        /// Register a data control to give it Dynamic Data behavior
        /// </summary>
        /// <param name="setSelectionFromUrl">When true, if a primary key is found in the route values
        ///     (typically on the query string), it will get be set as the selected item. This only applies
        ///     to list controls.</param>
        public void RegisterControl(Control control, bool setSelectionFromUrl) {
            // 
            if (DesignMode) {
                return;
            }

            IDataBoundControlInterface dataBoundControl = DataControlHelper.GetDataBoundControl(control, true /*failIfNotFound*/);

            // If we can't get an associated IDynamicDataSource, don't do anything
            IDynamicDataSource dataSource = dataBoundControl.DataSourceObject as IDynamicDataSource;
            if (dataSource == null) {
                return;
            }
            // If we can't get a MetaTable from the data source, don't do anything
            MetaTable table = MetaTableHelper.GetTableWithFullFallback(dataSource, Context.ToWrapper());
            
            // Save the datasource so we can process its parameters in OnLoad. The value we set is irrelevant
            _dataSources[dataSource] = null;

            ((INamingContainer)control).SetMetaTable(table);

            BaseDataBoundControl baseDataBoundControl = control as BaseDataBoundControl;
            if (baseDataBoundControl != null) {
                EnablePersistedSelection(baseDataBoundControl, table);
            }

            RegisterControlInternal(dataBoundControl, dataSource, table, setSelectionFromUrl, Page.IsPostBack);
        }

        internal static void EnablePersistedSelection(BaseDataBoundControl baseDataBoundControl, IMetaTable table) {
            Debug.Assert(baseDataBoundControl != null, "NULL!");
            // Make the persisted selection [....] up with the selected index if possible
            if (!table.IsReadOnly) {
                DynamicDataExtensions.EnablePersistedSelectionInternal(baseDataBoundControl);
            }
        }

        internal void RegisterControlInternal(IDataBoundControlInterface dataBoundControl, IDynamicDataSource dataSource, IMetaTable table, bool setSelectionFromUrl, bool isPostBack) {
            // Set the auto field generator (for controls that support it - GridView and DetailsView)
            IFieldControl fieldControl = dataBoundControl as IFieldControl;
            if (fieldControl != null) {
                fieldControl.FieldsGenerator = new DefaultAutoFieldGenerator(table);
            }
            var linqDataSource = dataSource as LinqDataSource;
            var entityDataSource = dataSource as EntityDataSource;
            // If the context type is not set, we need to set it
            if (dataSource.ContextType == null) {
                dataSource.ContextType = table.DataContextType;

                // If it's a LinqDataSurce, register for ContextCreating so the context gets created using the correct ctor
                // Ideally, this would work with other datasource, but we just don't have the right abstraction
                if (linqDataSource != null) {
                    linqDataSource.ContextCreating += delegate(object sender, LinqDataSourceContextEventArgs e) {
                        e.ObjectInstance = table.CreateContext();
                    };
                }

                if (entityDataSource != null) {
                    entityDataSource.ContextCreating += delegate(object sender, EntityDataSourceContextCreatingEventArgs e) {
                        e.Context = (ObjectContext)table.CreateContext();
                    };
                }
            }

            // If the datasource doesn't have an EntitySetName (aka TableName), set it from the meta table
            if (String.IsNullOrEmpty(dataSource.EntitySetName)) {
                dataSource.EntitySetName = table.DataContextPropertyName;
            }

            // If there is no Where clause, turn on auto generate
            if (String.IsNullOrEmpty(dataSource.Where)) {
                dataSource.AutoGenerateWhereClause = true;
            }

            // If it's a LinqDataSource and the flag is set, pre load the foreign keys
            if (AutoLoadForeignKeys && linqDataSource != null) {
                linqDataSource.LoadWithForeignKeys(table.EntityType);
            }

            if (!isPostBack) {
                if (table.HasPrimaryKey) {
                    dataBoundControl.DataKeyNames = table.PrimaryKeyNames;

                    // Set the virtual selection from the URL if needed
                    var dataKeySelector = dataBoundControl as IPersistedSelector;
                    if (dataKeySelector != null && setSelectionFromUrl) {
                        DataKey dataKey = table.GetDataKeyFromRoute();
                        if (dataKey != null) {
                            dataKeySelector.DataKey = dataKey;
                        }
                    }
                }
            }
        }


        internal static IControlParameterTarget GetControlParameterTarget(Control control) {
            return (control as IControlParameterTarget) ?? new DataBoundControlParameterTarget(control);
        }
    }
}
