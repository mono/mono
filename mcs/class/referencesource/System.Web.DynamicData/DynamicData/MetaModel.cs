using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Web.DynamicData.ModelProviders;
using System.Web.Resources;
using System.Collections.Concurrent;


namespace System.Web.DynamicData {
    /// <summary>
    /// Object that represents a database or a number of databases used by the dynamic data. It can have multiple different data contexts registered on it.
    /// </summary>
    public class MetaModel : IMetaModel {
        private List<Type> _contextTypes = new List<Type>();
        private static object _lock = new object();
        private List<MetaTable> _tables = new List<MetaTable>();
        private ReadOnlyCollection<MetaTable> _tablesRO;
        private Dictionary<string, MetaTable> _tablesByUniqueName = new Dictionary<string, MetaTable>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<ContextTypeTableNamePair, MetaTable> _tablesByContextAndName = new Dictionary<ContextTypeTableNamePair, MetaTable>();
        private SchemaCreator _schemaCreator;
        private EntityTemplateFactory _entityTemplateFactory;
        private IFieldTemplateFactory _fieldTemplateFactory;
        private FilterFactory _filterFactory;
        private static Exception s_registrationException;
        private static MetaModel s_defaultModel;
        private string _dynamicDataFolderVirtualPath;
        private HttpContextBase _context;
        private readonly static ConcurrentDictionary<Type, bool> s_registeredMetadataTypes = new ConcurrentDictionary<Type, bool>();

        // Use global registration is true by default
        private bool _registerGlobally = true;

        internal virtual int RegisteredDataModelsCount {
            get {
                return _contextTypes.Count;
            }
        }

        /// <summary>
        /// ctor
        /// </summary>
        public MetaModel()
            : this(true /* registerGlobally */) {
        }

        public MetaModel(bool registerGlobally)
            : this(SchemaCreator.Instance, registerGlobally) {
        }

        // constructor for testing purposes
        internal MetaModel(SchemaCreator schemaCreator, bool registerGlobally) {
            // Create a readonly wrapper for handing out
            _tablesRO = new ReadOnlyCollection<MetaTable>(_tables);
            _schemaCreator = schemaCreator;
            _registerGlobally = registerGlobally;

            // Don't touch Default.Model when we're not using global registration
            if (registerGlobally) {
                lock (_lock) {
                    if (Default == null) {
                        Default = this;
                    }
                }
            }
        }

        internal HttpContextBase Context {
            get {
                return _context ?? HttpContext.Current.ToWrapper();
            }
            set {
                _context = value;
            }
        }

        /// <summary>
        /// allows for setting of the DynamicData folder for this mode. The default is ~/DynamicData/
        /// </summary>
        public string DynamicDataFolderVirtualPath {
            get {
                if (_dynamicDataFolderVirtualPath == null) {
                    _dynamicDataFolderVirtualPath = "~/DynamicData/";
                }

                return _dynamicDataFolderVirtualPath;
            }
            set {
                // Make sure it ends with a slash
                _dynamicDataFolderVirtualPath = VirtualPathUtility.AppendTrailingSlash(value);
            }
        }

        /// <summary>
        /// Returns a reference to the first instance of MetaModel that is created in an app. Provides a simple way of referencing
        /// the default MetaModel instance. Applications that will use multiple models will have to provide their own way of storing
        /// references to any additional meta models. One way of looking them up is by using the GetModel method.
        /// </summary>
        public static MetaModel Default {
            get {
                CheckForRegistrationException();
                return s_defaultModel;
            }
            internal set { s_defaultModel = value; }
        }

        /// <summary>
        /// Gets the model instance that had the contextType registered with it
        /// </summary>
        /// <param name="contextType">A DataContext or ObjectContext type (e.g. NorthwindDataContext)</param>
        /// <returns>a model</returns>
        public static MetaModel GetModel(Type contextType) {
            CheckForRegistrationException();
            if (contextType == null) {
                throw new ArgumentNullException("contextType");
            }
            MetaModel model;
            if (MetaModelManager.TryGetModel(contextType, out model)) {
                return model;
            }
            else {
                throw new InvalidOperationException(String.Format(
                    CultureInfo.CurrentCulture,
                    DynamicDataResources.MetaModel_ContextDoesNotBelongToModel,
                    contextType.FullName));
            }
        }

        /// <summary>
        /// Registers a context. Uses the default ContextConfiguration options.
        /// </summary>
        /// <param name="contextType"></param>
        public void RegisterContext(Type contextType) {
            RegisterContext(contextType, new ContextConfiguration());
        }

        /// <summary>
        /// Registers a context. Uses the the given ContextConfiguration options.
        /// </summary>
        /// <param name="contextType"></param>
        /// <param name="configuration"></param>
        public void RegisterContext(Type contextType, ContextConfiguration configuration) {
            if (contextType == null) {
                throw new ArgumentNullException("contextType");
            }
            RegisterContext(() => Activator.CreateInstance(contextType), configuration);
        }

        /// <summary>
        /// Registers a context. Uses default ContextConfiguration. Accepts a context factory that is a delegate used for
        /// instantiating the context. This allows developers to instantiate context using a custom constructor.
        /// </summary>
        /// <param name="contextFactory"></param>
        public void RegisterContext(Func<object> contextFactory) {
            RegisterContext(contextFactory, new ContextConfiguration());
        }

        /// <summary>
        /// Registers a context. Uses given ContextConfiguration. Accepts a context factory that is a delegate used for
        /// instantiating the context. This allows developers to instantiate context using a custom constructor.
        /// </summary>
        /// <param name="contextFactory"></param>
        /// <param name="configuration"></param>
        public void RegisterContext(Func<object> contextFactory, ContextConfiguration configuration) {
            object contextInstance = null;
            try {
                if (contextFactory == null) {
                    throw new ArgumentNullException("contextFactory");
                }
                contextInstance = contextFactory();
                if (contextInstance == null) {
                    throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, DynamicDataResources.MetaModel_ContextFactoryReturnsNull), "contextFactory");
                }
                Type contextType = contextInstance.GetType();
                if (!_schemaCreator.ValidDataContextType(contextType)) {
                    throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, DynamicDataResources.MetaModel_ContextTypeNotSupported, contextType.FullName));
                }
            }
            catch (Exception e) {
                s_registrationException = e;
                throw;
            }

            // create model abstraction
            RegisterContext(_schemaCreator.CreateDataModel(contextInstance, contextFactory), configuration);
        }

        /// <summary>
        /// Register context using give model provider. Uses default context configuration.
        /// </summary>
        /// <param name="dataModelProvider"></param>
        public void RegisterContext(DataModelProvider dataModelProvider) {
            RegisterContext(dataModelProvider, new ContextConfiguration());
        }

        /// <summary>
        /// Register context using give model provider. Uses given context configuration.
        /// </summary>
        /// <param name="dataModelProvider"></param>
        /// <param name="configuration"></param>
        [SuppressMessage("Microsoft.Security", "CA2119:SealMethodsThatSatisfyPrivateInterfaces",
            Justification = "Interface is not used in any security sesitive code paths.")]
        public virtual void RegisterContext(DataModelProvider dataModelProvider, ContextConfiguration configuration) {
            if (dataModelProvider == null) {
                throw new ArgumentNullException("dataModelProvider");
            }

            if (configuration == null) {
                throw new ArgumentNullException("configuration");
            }

            if (_registerGlobally) {
                CheckForRegistrationException();
            }

            // check if context has already been registered
            if (_contextTypes.Contains(dataModelProvider.ContextType)) {
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, DynamicDataResources.MetaModel_ContextAlreadyRegistered, dataModelProvider.ContextType.FullName));
            }

            try {
                IEnumerable<TableProvider> tableProviders = dataModelProvider.Tables;

                // create and validate model
                var tablesToInitialize = new List<MetaTable>();
                foreach (TableProvider tableProvider in tableProviders) {
                    RegisterMetadataTypeDescriptionProvider(tableProvider, configuration.MetadataProviderFactory);

                    MetaTable table = CreateTable(tableProvider);
                    table.CreateColumns();

                    var tableNameAttribute = tableProvider.Attributes.OfType<TableNameAttribute>().SingleOrDefault();
                    string nameOverride = tableNameAttribute != null ? tableNameAttribute.Name : null;
                    table.SetScaffoldAndName(configuration.ScaffoldAllTables, nameOverride);

                    CheckTableNameConflict(table, nameOverride, tablesToInitialize);

                    tablesToInitialize.Add(table);
                }

                _contextTypes.Add(dataModelProvider.ContextType);

                if (_registerGlobally) {
                    MetaModelManager.AddModel(dataModelProvider.ContextType, this);
                }

                foreach (MetaTable table in tablesToInitialize) {
                    AddTable(table);
                }
                // perform initialization at the very end to ensure all references will be properly registered
                foreach (MetaTable table in tablesToInitialize) {
                    table.Initialize();
                }
            }
            catch (Exception e) {
                if (_registerGlobally) {
                    s_registrationException = e;
                }
                throw;
            }
        }

        internal static void CheckForRegistrationException() {
            if (s_registrationException != null) {
                throw new InvalidOperationException(
                    String.Format(CultureInfo.CurrentCulture, DynamicDataResources.MetaModel_RegistrationErrorOccurred),
                    s_registrationException);
            }
        }

        /// <summary>
        /// Reset any previous registration error that may have happened. Normally, the behavior is that when an error
        /// occurs during registration, the exception is cached and rethrown on all subsequent operations. This is done
        /// so that if an error occurs in Application_Start, it shows up on every request.  Calling this method clears
        /// out the error and potentially allows new RegisterContext calls.
        /// </summary>
        public static void ResetRegistrationException() {
            s_registrationException = null;
        }

        // Used  for unit tests
        internal static void ClearSimpleCache() {
            s_registeredMetadataTypes.Clear();
        }

        internal static MetaModel CreateSimpleModel(Type entityType) {
            // Never register a TDP more than once for a type
            if (!s_registeredMetadataTypes.ContainsKey(entityType)) {
                var provider = new AssociatedMetadataTypeTypeDescriptionProvider(entityType);
                TypeDescriptor.AddProviderTransparent(provider, entityType);
                s_registeredMetadataTypes.TryAdd(entityType, true);
            }

            MetaModel model = new MetaModel(false /* registerGlobally */);

            // Pass a null provider factory since we registered the provider ourselves
            model.RegisterContext(new SimpleDataModelProvider(entityType), new ContextConfiguration { MetadataProviderFactory = null });
            return model;
        }

        internal static MetaModel CreateSimpleModel(ICustomTypeDescriptor descriptor) {
            MetaModel model = new MetaModel(false /* registerGlobally */);
            // 
            model.RegisterContext(new SimpleDataModelProvider(descriptor));
            return model;
        }

        /// <summary>
        /// Instantiate a MetaTable object. Can be overridden to instantiate a derived type 
        /// </summary>
        /// <returns></returns>
        protected virtual MetaTable CreateTable(TableProvider provider) {
            return new MetaTable(this, provider);
        }

        private void AddTable(MetaTable table) {
            _tables.Add(table);
            _tablesByUniqueName.Add(table.Name, table);
            if (_registerGlobally) {
                MetaModelManager.AddTable(table.EntityType, table);
            }

            if (table.DataContextType != null) {
                // need to use the name from the provider since the name from the table could have been modified by use of TableNameAttribute
                _tablesByContextAndName.Add(new ContextTypeTableNamePair(table.DataContextType, table.Provider.Name), table);
            }
        }

        private void CheckTableNameConflict(MetaTable table, string nameOverride, List<MetaTable> tablesToInitialize) {
            // try to find name conflict in tables from other context, or already processed tables in current context
            MetaTable nameConflictTable;
            if (!_tablesByUniqueName.TryGetValue(table.Name, out nameConflictTable)) {
                nameConflictTable = tablesToInitialize.Find(t => t.Name.Equals(table.Name, StringComparison.CurrentCulture));
            }
            if (nameConflictTable != null) {
                if (String.IsNullOrEmpty(nameOverride)) {
                    throw new ArgumentException(String.Format(
                        CultureInfo.CurrentCulture,
                        DynamicDataResources.MetaModel_EntityNameConflict,
                        table.EntityType.FullName,
                        table.DataContextType.FullName,
                        nameConflictTable.EntityType.FullName,
                        nameConflictTable.DataContextType.FullName));
                }
                else {
                    throw new ArgumentException(String.Format(
                        CultureInfo.CurrentCulture,
                        DynamicDataResources.MetaModel_EntityNameOverrideConflict,
                        nameOverride,
                        table.EntityType.FullName,
                        table.DataContextType.FullName,
                        nameConflictTable.EntityType.FullName,
                        nameConflictTable.DataContextType.FullName));
                }
            }
        }

        private static void RegisterMetadataTypeDescriptionProvider(TableProvider entity, Func<Type, TypeDescriptionProvider> providerFactory) {
            if (providerFactory != null) {
                Type entityType = entity.EntityType;
                // Support for type-less MetaTable
                if (entityType != null) {
                    TypeDescriptionProvider provider = providerFactory(entityType);
                    if (provider != null) {
                        TypeDescriptor.AddProviderTransparent(provider, entityType);
                    }
                }
            }
        }

        /// <summary>
        /// Returns a collection of all the tables that are part of the context, regardless of whether they are visible or not.
        /// </summary>
        public ReadOnlyCollection<MetaTable> Tables {
            get {
                CheckForRegistrationException();
                return _tablesRO;
            }
        }

        /// <summary>
        /// Returns a collection of the currently visible tables for this context. Currently visible is defined as:
        /// - a table whose EntityType is not abstract
        /// - a table with scaffolding enabled
        /// - a table for which a custom page for the list action can be found and that can be read by the current User
        /// </summary>
        public List<MetaTable> VisibleTables {
            get {
                CheckForRegistrationException();
                return Tables.Where(IsTableVisible).OrderBy(t => t.DisplayName).ToList();
            }
        }

        private bool IsTableVisible(MetaTable table) {
            return !table.EntityType.IsAbstract && !String.IsNullOrEmpty(table.ListActionPath) && table.CanRead(Context.User);
        }

        /// <summary>
        /// Looks up a MetaTable by the entity type. Throws an exception if one is not found.
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "We really want this to be a Type.")]
        public MetaTable GetTable(Type entityType) {
            MetaTable table;
            if (!TryGetTable(entityType, out table)) {
                throw new ArgumentException(String.Format(
                    CultureInfo.CurrentCulture,
                    DynamicDataResources.MetaModel_UnknownEntityType,
                    entityType.FullName));
            }

            return table;
        }

        /// <summary>
        /// Tries to look up a MetaTable by the entity type.
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        public bool TryGetTable(Type entityType, out MetaTable table) {
            CheckForRegistrationException();

            if (entityType == null) {
                throw new ArgumentNullException("entityType");
            }

            if (!_registerGlobally) {
                table = Tables.SingleOrDefault(t => t.EntityType == entityType);
                return table != null;
            }

            return MetaModelManager.TryGetTable(entityType, out table);
        }

        /// <summary>
        /// Looks up a MetaTable by unique name. Throws if one is not found. The unique name defaults to the table name, or an override
        /// can be provided via ContextConfiguration when the context that contains the table is registered. The unique name uniquely
        /// identifies a table within a give MetaModel. It is used for URL generation.
        /// </summary>
        /// <param name="uniqueTableName"></param>
        /// <returns></returns>
        public MetaTable GetTable(string uniqueTableName) {
            CheckForRegistrationException();

            MetaTable table;
            if (!TryGetTable(uniqueTableName, out table)) {
                throw new ArgumentException(String.Format(
                    CultureInfo.CurrentCulture,
                    DynamicDataResources.MetaModel_UnknownTable,
                    uniqueTableName));
            }

            return table;
        }

        /// <summary>
        /// Tries to look up a MetaTable by unique name. Doe
        /// </summary>
        /// <param name="uniqueTableName"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        public bool TryGetTable(string uniqueTableName, out MetaTable table) {
            CheckForRegistrationException();

            if (uniqueTableName == null) {
                throw new ArgumentNullException("uniqueTableName");
            }

            return _tablesByUniqueName.TryGetValue(uniqueTableName, out table);
        }

        /// <summary>
        /// Looks up a MetaTable by the contextType/tableName combination. Throws if one is not found.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="contextType"></param>
        /// <returns></returns>
        public MetaTable GetTable(string tableName, Type contextType) {
            CheckForRegistrationException();

            if (tableName == null) {
                throw new ArgumentNullException("tableName");
            }

            if (contextType == null) {
                throw new ArgumentNullException("contextType");
            }

            MetaTable table;
            if (!_tablesByContextAndName.TryGetValue(new ContextTypeTableNamePair(contextType, tableName), out table)) {
                if (!_contextTypes.Contains(contextType)) {
                    throw new ArgumentException(String.Format(CultureInfo.CurrentCulture,
                        DynamicDataResources.MetaModel_UnknownContextType,
                        contextType.FullName));
                }
                else {
                    throw new ArgumentException(String.Format(CultureInfo.CurrentCulture,
                        DynamicDataResources.MetaModel_UnknownTableInContext,
                        contextType.FullName,
                        tableName));
                }
            }
            return table;
        }

        /// <summary>
        /// Lets you set a custom IFieldTemplateFactory. An IFieldTemplateFactor lets you customize which field templates are created
        /// for the various columns.
        /// </summary>
        public IFieldTemplateFactory FieldTemplateFactory {
            get {
                // If no custom factory was set, use our default
                if (_fieldTemplateFactory == null) {
                    FieldTemplateFactory = new FieldTemplateFactory();
                }

                return _fieldTemplateFactory;
            }
            set {
                _fieldTemplateFactory = value;

                // Give the model to the factory
                if (_fieldTemplateFactory != null) {
                    _fieldTemplateFactory.Initialize(this);
                }
            }
        }

        public EntityTemplateFactory EntityTemplateFactory {
            get {
                if (_entityTemplateFactory == null) {
                    EntityTemplateFactory = new EntityTemplateFactory();
                }

                return _entityTemplateFactory;
            }
            set {
                _entityTemplateFactory = value;
                if (_entityTemplateFactory != null) {
                    _entityTemplateFactory.Initialize(this);
                }
            }
        }

        public FilterFactory FilterFactory {
            get {
                if (_filterFactory == null) {
                    FilterFactory = new FilterFactory();
                }
                return _filterFactory;
            }
            set {
                _filterFactory = value;
                if (_filterFactory != null) {
                    _filterFactory.Initialize(this);
                }
            }
        }

        private string _queryStringKeyPrefix = String.Empty;

        /// <summary>
        /// Lets you get an action path (URL) to an action for a particular table/action/entity instance combo.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="action"></param>
        /// <param name="row">An object representing a single row of data in a table. Used to provide values for query string parameters.</param>
        /// <returns></returns>
        public string GetActionPath(string tableName, string action, object row) {
            return GetTable(tableName).GetActionPath(action, row);
        }

        private class ContextTypeTableNamePair : IEquatable<ContextTypeTableNamePair> {
            public ContextTypeTableNamePair(Type contextType, string tableName) {
                Debug.Assert(contextType != null);
                Debug.Assert(tableName != null);

                ContextType = contextType;
                TableName = tableName;

                HashCode = ContextType.GetHashCode() ^ TableName.GetHashCode();
            }

            private int HashCode { get; set; }
            public Type ContextType { get; private set; }
            public string TableName { get; private set; }

            public bool Equals(ContextTypeTableNamePair other) {
                if (other == null) {
                    return false;
                }
                return ContextType == other.ContextType && TableName.Equals(other.TableName, StringComparison.Ordinal);
            }

            public override int GetHashCode() {
                return HashCode;
            }

            public override bool Equals(object obj) {
                return Equals(obj as ContextTypeTableNamePair);
            }
        }

        internal static class MetaModelManager {
            private static Hashtable s_modelByContextType = new Hashtable();
            private static Hashtable s_tableByEntityType = new Hashtable();

            internal static void AddModel(Type contextType, MetaModel model) {
                Debug.Assert(contextType != null);
                Debug.Assert(model != null);
                lock (s_modelByContextType) {
                    s_modelByContextType.Add(contextType, model);
                }
            }

            internal static bool TryGetModel(Type contextType, out MetaModel model) {
                model = (MetaModel)s_modelByContextType[contextType];
                return model != null;
            }

            internal static void AddTable(Type entityType, MetaTable table) {
                Debug.Assert(entityType != null);
                Debug.Assert(table != null);
                lock (s_tableByEntityType) {
                    s_tableByEntityType[entityType] = table;
                }
            }

            internal static void Clear() {
                lock (s_modelByContextType) {
                    s_modelByContextType.Clear();
                }
                lock (s_tableByEntityType) {
                    s_tableByEntityType.Clear();
                }
            }

            internal static bool TryGetTable(Type type, out MetaTable table) {
                table = (MetaTable)s_tableByEntityType[type];
                return table != null;
            }
        }

        ReadOnlyCollection<IMetaTable> IMetaModel.Tables {
            get {
                return Tables.OfType<IMetaTable>().ToList().AsReadOnly();
            }
        }

        bool IMetaModel.TryGetTable(string uniqueTableName, out IMetaTable table) {
            MetaTable metaTable;
            table = null;
            if (TryGetTable(uniqueTableName, out metaTable)) {
                table = metaTable;
                return true;
            }
            return false;
        }

        bool IMetaModel.TryGetTable(Type entityType, out IMetaTable table) {
            MetaTable metaTable;
            table = null;
            if (TryGetTable(entityType, out metaTable)) {
                table = metaTable;
                return true;
            }
            return false;
        }

        List<IMetaTable> IMetaModel.VisibleTables {
            get {
                return VisibleTables.OfType<IMetaTable>().ToList();
            }
        }

        IMetaTable IMetaModel.GetTable(string tableName, Type contextType) {
            return GetTable(tableName, contextType);
        }

        IMetaTable IMetaModel.GetTable(string uniqueTableName) {
            return GetTable(uniqueTableName);
        }

        IMetaTable IMetaModel.GetTable(Type entityType) {
            return GetTable(entityType);
        }
    }
}
