//---------------------------------------------------------------------
// <copyright file="ObjectContext.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

namespace System.Data.Objects
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Configuration;
    using System.Data.Common;
    using System.Data.Common.CommandTrees;
    using System.Data.Common.CommandTrees.ExpressionBuilder;
    using System.Data.Common.Internal.Materialization;
    using System.Data.Common.Utils;
    using System.Data.Entity;
    using System.Data.EntityClient;
    using System.Data.Metadata.Edm;
    using System.Data.Objects.DataClasses;
    using System.Data.Objects.ELinq;
    using System.Data.Objects.Internal;
    using System.Data.Query.InternalTrees;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Runtime.Versioning;
    using System.Text;
    using System.Transactions;

    /// <summary>
    /// Defines options that affect the behavior of the ObjectContext.
    /// </summary>
    public sealed class ObjectContextOptions
    {
        private bool _lazyLoadingEnabled;
        private bool _proxyCreationEnabled = true;
        private bool _useLegacyPreserveChangesBehavior = false;
        private bool _useConsistentNullReferenceBehavior;
        private bool _useCSharpNullComparisonBehavior = false;

        internal ObjectContextOptions()
        {
        }

        /// <summary>
        /// Get or set boolean that determines if related ends can be loaded on demand 
        /// when they are accessed through a navigation property.
        /// </summary>
        /// <value>
        /// True if related ends can be loaded on demand; otherwise false.
        /// </value>
        public bool LazyLoadingEnabled
        {
            get { return _lazyLoadingEnabled; }
            set { _lazyLoadingEnabled = value; }
        }

        /// <summary>
        /// Get or set boolean that determines whether proxy instances will be create
        /// for CLR types with a corresponding proxy type.
        /// </summary>
        /// <value>
        /// True if proxy instances should be created; otherwise false to create "normal" instances of the type.
        /// </value>
        public bool ProxyCreationEnabled
        {
            get { return _proxyCreationEnabled; }
            set { _proxyCreationEnabled = value; }
        }

        /// <summary>
        /// Get or set a boolean that determines whether to use the legacy MergeOption.PreserveChanges behavior
        /// when querying for entities using MergeOption.PreserveChanges
        /// </summary>
        /// <value>
        /// True if the legacy MergeOption.PreserveChanges behavior should be used; otherwise false.
        /// </value>
        public bool UseLegacyPreserveChangesBehavior
        {
            get { return _useLegacyPreserveChangesBehavior; }
            set { _useLegacyPreserveChangesBehavior = value; }
        }

        /// <summary>
        /// If this flag is set to false then setting the Value property of the <see cref="EntityReference{T}"/> for an
        /// FK relationship to null when it is already null will have no effect. When this flag is set to true, then
        /// setting the value to null will always cause the FK to be nulled and the relationship to be deleted
        /// even if the value is currently null. The default value is false when using ObjectContext and true
        /// when using DbContext.
        /// </summary>
        public bool UseConsistentNullReferenceBehavior
        {
            get { return _useConsistentNullReferenceBehavior; }
            set { _useConsistentNullReferenceBehavior = value; }
        }

        /// <summary>
        /// This flag determines whether C# behavior should be exhibited when comparing null values in LinqToEntities. 
        /// If this flag is set, then any equality comparison between two operands, both of which are potentially 
        /// nullable, will be rewritten to show C# null comparison semantics. As an example: 
        /// (operand1 = operand2) will be rewritten as 
        /// (((operand1 = operand2) AND NOT (operand1 IS NULL OR operand2 IS NULL)) || (operand1 IS NULL && operand2 IS NULL))
        /// The default value is false when using <see cref="ObjectContext"/>.
        /// </summary>
        public bool UseCSharpNullComparisonBehavior
        {
            get { return _useCSharpNullComparisonBehavior; }
            set { _useCSharpNullComparisonBehavior = value; }
        }
    }

    /// <summary>
    /// ObjectContext is the top-level object that encapsulates a connection between the CLR and the database,
    /// serving as a gateway for Create, Read, Update, and Delete operations.
    /// </summary>
    public class ObjectContext : IDisposable
    {
        #region Fields
        private IEntityAdapter _adapter;

        // Connection may be null if used by ObjectMaterializer for detached ObjectContext,
        // but those code paths should not touch the connection.
        //
        // If the connection is null, this indicates that this object has been disposed.
        // Disposal for this class doesn't mean complete disposal, 
        // but rather the disposal of the underlying connection object if the ObjectContext owns the connection,
        // or the separation of the underlying connection object from the ObjectContext if the ObjectContext does not own the connection.
        //
        // Operations that require a connection should throw an ObjectDiposedException if the connection is null.
        // Other operations that do not need a connection should continue to work after disposal.
        private EntityConnection _connection;

        private readonly MetadataWorkspace _workspace;
        private ObjectStateManager _cache;
        private ClrPerspective _perspective;
        private readonly bool _createdConnection;
        private bool _openedConnection;             // whether or not the context opened the connection to do an operation
        private int _connectionRequestCount;        // the number of active requests for an open connection
        private int? _queryTimeout;
        private Transaction _lastTransaction;

        private bool _disallowSettingDefaultContainerName;

        private EventHandler _onSavingChanges;

        private ObjectMaterializedEventHandler _onObjectMaterialized;

        private ObjectQueryProvider _queryProvider;

        private readonly ObjectContextOptions _options = new ObjectContextOptions();

        private readonly string s_UseLegacyPreserveChangesBehavior = "EntityFramework_UseLegacyPreserveChangesBehavior";

        #endregion Fields

        #region Constructors
        /// <summary>
        /// Creates an ObjectContext with the given connection and metadata workspace.
        /// </summary>
        /// <param name="connection">connection to the store</param>
        public ObjectContext(EntityConnection connection)
            : this(EntityUtil.CheckArgumentNull(connection, "connection"), true)
        {
        }

        /// <summary>
        /// Creates an ObjectContext with the given connection string and
        /// default entity container name.  This constructor
        /// creates and initializes an EntityConnection so that the context is
        /// ready to use; no other initialization is necessary.  The given
        /// connection string must be valid for an EntityConnection; connection
        /// strings for other connection types are not supported.
        /// </summary>
        /// <param name="connectionString">the connection string to use in the underlying EntityConnection to the store</param>
        /// <exception cref="ArgumentNullException">connectionString is null</exception>
        /// <exception cref="ArgumentException">if connectionString is invalid</exception>
        [ResourceExposure(ResourceScope.Machine)] //Exposes the file names as part of ConnectionString which are a Machine resource
        [ResourceConsumption(ResourceScope.Machine)] //For CreateEntityConnection method. But the paths are not created in this method.
        public ObjectContext(string connectionString)
            : this(CreateEntityConnection(connectionString), false)
        {
            _createdConnection = true;
        }


        /// <summary>
        /// Creates an ObjectContext with the given connection string and
        /// default entity container name.  This protected constructor creates and initializes an EntityConnection so that the context 
        /// is ready to use; no other initialization is necessary.  The given connection string must be valid for an EntityConnection; 
        /// connection strings for other connection types are not supported.
        /// </summary>
        /// <param name="connectionString">the connection string to use in the underlying EntityConnection to the store</param>
        /// <param name="defaultContainerName">the name of the default entity container</param>
        /// <exception cref="ArgumentNullException">connectionString is null</exception>
        /// <exception cref="ArgumentException">either connectionString or defaultContainerName is invalid</exception>
        [ResourceExposure(ResourceScope.Machine)] //Exposes the file names as part of ConnectionString which are a Machine resource
        [ResourceConsumption(ResourceScope.Machine)] //For ObjectContext method. But the paths are not created in this method.
        protected ObjectContext(string connectionString, string defaultContainerName)
            : this(connectionString)
        {
            DefaultContainerName = defaultContainerName;
            if (!string.IsNullOrEmpty(defaultContainerName))
            {
                _disallowSettingDefaultContainerName = true;
            }
        }

        /// <summary>
        /// Creates an ObjectContext with the given connection and metadata workspace.
        /// </summary>
        /// <param name="connection">connection to the store</param>
        /// <param name="defaultContainerName">the name of the default entity container</param>
        protected ObjectContext(EntityConnection connection, string defaultContainerName)
            : this(connection)
        {
            DefaultContainerName = defaultContainerName;
            if (!string.IsNullOrEmpty(defaultContainerName))
            {
                _disallowSettingDefaultContainerName = true;
            }
        }

        private ObjectContext(EntityConnection connection, bool isConnectionConstructor)
        {
            Debug.Assert(null != connection, "null connection");
            _connection = connection;

            _connection.StateChange += ConnectionStateChange;

            // Ensure a valid connection
            string connectionString = connection.ConnectionString;
            if (connectionString == null || connectionString.Trim().Length == 0)
            {
                throw EntityUtil.InvalidConnection(isConnectionConstructor, null);
            }

            try
            {
                _workspace = RetrieveMetadataWorkspaceFromConnection();
            }
            catch (InvalidOperationException e)
            {
                // Intercept exceptions retrieving workspace, and wrap exception in appropriate
                // message based on which constructor pattern is being used.
                throw EntityUtil.InvalidConnection(isConnectionConstructor, e);
            }

            // Register the O and OC metadata
            if (null != _workspace)
            {
                // register the O-Loader
                if (!_workspace.IsItemCollectionAlreadyRegistered(DataSpace.OSpace))
                {
                    ObjectItemCollection itemCollection = new ObjectItemCollection();
                    _workspace.RegisterItemCollection(itemCollection);
                }

                // have the OC-Loader registered by asking for it
                _workspace.GetItemCollection(DataSpace.OCSpace);
            }

            // load config file properties
            string value = ConfigurationManager.AppSettings[s_UseLegacyPreserveChangesBehavior];
            bool useV35Behavior = false;
            if (Boolean.TryParse(value, out useV35Behavior))
            {
                ContextOptions.UseLegacyPreserveChangesBehavior = useV35Behavior;
            }
        }

        #endregion //Constructors

        #region Properties
        /// <summary>
        /// Gets the connection to the store.
        /// </summary>
        /// <exception cref="ObjectDisposedException">If the <see cref="ObjectContext"/> instance has been disposed.</exception>
        public DbConnection Connection
        {
            get
            {
                if (_connection == null)
                {
                    throw EntityUtil.ObjectContextDisposed();
                }

                return _connection;
            }
        }

        /// <summary>
        /// Gets or sets the default container name.
        /// </summary>
        public string DefaultContainerName
        {
            get
            {
                EntityContainer container = Perspective.GetDefaultContainer();
                return ((null != container) ? container.Name : String.Empty);
            }
            set
            {
                if (!_disallowSettingDefaultContainerName)
                {
                    Perspective.SetDefaultContainer(value);
                }
                else
                {
                    throw EntityUtil.CannotSetDefaultContainerName();
                }
            }
        }

        /// <summary>
        /// Gets the metadata workspace associated with this ObjectContext.
        /// </summary>
        [CLSCompliant(false)]
        public MetadataWorkspace MetadataWorkspace
        {
            get
            {
                return _workspace;
            }
        }

        /// <summary>
        /// Gets the ObjectStateManager used by this ObjectContext.
        /// </summary>
        public ObjectStateManager ObjectStateManager
        {
            get
            {
                if (_cache == null)
                {
                    _cache = new ObjectStateManager(_workspace);
                }
                return _cache;
            }
        }

        /// <summary>
        /// ClrPerspective based on the MetadataWorkspace.
        /// </summary>
        internal ClrPerspective Perspective
        {
            get
            {
                if (_perspective == null)
                {
                    _perspective = new ClrPerspective(_workspace);
                }
                return _perspective;
            }
        }

        /// <summary>
        /// Gets and sets the timeout value used for queries with this ObjectContext.
        /// A null value indicates that the default value of the underlying provider
        /// will be used.
        /// </summary>
        public int? CommandTimeout
        {
            get
            {
                return _queryTimeout;
            }
            set
            {
                if (value.HasValue && value < 0)
                {
                    throw EntityUtil.InvalidCommandTimeout("value");
                }
                _queryTimeout = value;
            }
        }

        /// <summary>
        /// Gets the LINQ query provider associated with this object context.
        /// </summary>
        internal protected IQueryProvider QueryProvider
        {
            get
            {
                if (null == _queryProvider)
                {
                    _queryProvider = new ObjectQueryProvider(this);
                }
                return _queryProvider;
            }
        }

        /// <summary>
        /// Whether or not we are in the middle of materialization
        /// Used to suppress operations such as lazy loading that are not allowed during materialization
        /// </summary>
        internal bool InMaterialization { get; set; }

        /// <summary>
        /// Get <see cref="ObjectContextOptions"/> instance that contains options 
        /// that affect the behavior of the ObjectContext.
        /// </summary>
        /// <value>
        /// Instance of <see cref="ObjectContextOptions"/> for the current ObjectContext.
        /// This value will never be null.
        /// </value>
        public ObjectContextOptions ContextOptions
        {
            get { return _options; }
        }

        #endregion //Properties

        #region Events
        /// <summary>
        /// Property for adding a delegate to the SavingChanges Event.
        /// </summary>
        public event EventHandler SavingChanges
        {
            add { _onSavingChanges += value; }
            remove { _onSavingChanges -= value; }
        }
        /// <summary>
        /// A private helper function for the _savingChanges/SavingChanges event.
        /// </summary>
        private void OnSavingChanges()
        {
            if (null != _onSavingChanges)
            {
                _onSavingChanges(this, new EventArgs());
            }
        }

        /// <summary>
        /// Event raised when a new entity object is materialized.  That is, the event is raised when
        /// a new entity object is created from data in the store as part of a query or load operation.
        /// </summary>
        /// <remarks>
        /// Note that the event is raised after included (spanned) referenced objects are loaded, but
        /// before included (spanned) collections are loaded.  Also, for independent associations,
        /// any stub entities for related objects that have not been loaded will also be created before
        /// the event is raised.
        /// 
        /// It is possible for an entity object to be created and then thrown away if it is determined
        /// that an entity with the same ID already exists in the Context.  This event is not raised
        /// in those cases.
        /// </remarks>
        public event ObjectMaterializedEventHandler ObjectMaterialized
        {
            add { _onObjectMaterialized += value; }
            remove { _onObjectMaterialized -= value; }
        }

        internal void OnObjectMaterialized(object entity)
        {
            if (null != _onObjectMaterialized)
            {
                _onObjectMaterialized(this, new ObjectMaterializedEventArgs(entity));
            }
        }

        /// <summary>
        /// Returns true if any handlers for the ObjectMaterialized event exist.  This is
        /// used for perf reasons to avoid collecting the information needed for the event
        /// if there is no point in firing it.
        /// </summary>
        internal bool OnMaterializedHasHandlers
        {
            get { return _onObjectMaterialized != null && _onObjectMaterialized.GetInvocationList().Length != 0; }
        }

        #endregion //Events

        #region Methods
        /// <summary>
        /// AcceptChanges on all associated entries in the ObjectStateManager so their resultant state is either unchanged or detached.
        /// </summary>
        /// <returns></returns>
        public void AcceptAllChanges()
        {
            ObjectStateManager.AssertAllForeignKeyIndexEntriesAreValid();

            if (ObjectStateManager.SomeEntryWithConceptualNullExists())
            {
                throw new InvalidOperationException(Strings.ObjectContext_CommitWithConceptualNull);
            }

            // There are scenarios in which order of calling AcceptChanges does matter:
            // in case there is an entity in Deleted state and another entity in Added state with the same ID -
            // it is necessary to call AcceptChanges on Deleted entity before calling AcceptChanges on Added entity
            // (doing this in the other order there is conflict of keys).
            foreach (ObjectStateEntry entry in ObjectStateManager.GetObjectStateEntries(EntityState.Deleted))
            {
                entry.AcceptChanges();
            }

            foreach (ObjectStateEntry entry in ObjectStateManager.GetObjectStateEntries(EntityState.Added | EntityState.Modified))
            {
                entry.AcceptChanges();
            }

            ObjectStateManager.AssertAllForeignKeyIndexEntriesAreValid();
        }

        private void VerifyRootForAdd(bool doAttach, string entitySetName, IEntityWrapper wrappedEntity, EntityEntry existingEntry, out EntitySet entitySet, out bool isNoOperation)
        {
            isNoOperation = false;

            EntitySet entitySetFromName = null;

            if (doAttach)
            {
                // For AttachTo the entity set name is optional
                if (!String.IsNullOrEmpty(entitySetName))
                {
                    entitySetFromName = this.GetEntitySetFromName(entitySetName);
                }
            }
            else
            {
                // For AddObject the entity set name is obligatory
                entitySetFromName = this.GetEntitySetFromName(entitySetName);
            }

            // Find entity set using entity key
            EntitySet entitySetFromKey = null;

            EntityKey key = existingEntry != null ? existingEntry.EntityKey : wrappedEntity.GetEntityKeyFromEntity();
            if (null != (object)key)
            {
                entitySetFromKey = key.GetEntitySet(this.MetadataWorkspace);

                if (entitySetFromName != null)
                {
                    // both entity sets are not null, compare them
                    EntityUtil.ValidateEntitySetInKey(key, entitySetFromName, "entitySetName");
                }
                key.ValidateEntityKey(_workspace, entitySetFromKey);
            }

            entitySet = entitySetFromKey ?? entitySetFromName;

            // Check if entity set was found
            if (entitySet == null)
            {
                throw EntityUtil.EntitySetNameOrEntityKeyRequired();
            }

            this.ValidateEntitySet(entitySet, wrappedEntity.IdentityType);

            // If in the middle of Attach, try to find the entry by key
            if (doAttach && existingEntry == null)
            {
                // If we don't already have a key, create one now
                if (null == (object)key)
                {
                    key = this.ObjectStateManager.CreateEntityKey(entitySet, wrappedEntity.Entity);
                }
                existingEntry = this.ObjectStateManager.FindEntityEntry(key);
            }

            if (null != existingEntry && !(doAttach && existingEntry.IsKeyEntry))
            {
                if (!Object.ReferenceEquals(existingEntry.Entity, wrappedEntity.Entity))
                {
                    throw EntityUtil.ObjectStateManagerContainsThisEntityKey();
                }
                else
                {
                    EntityState exptectedState = doAttach ? EntityState.Unchanged : EntityState.Added;

                    if (existingEntry.State != exptectedState)
                    {
                        throw doAttach ?
                            EntityUtil.EntityAlreadyExistsInObjectStateManager() :
                            EntityUtil.ObjectStateManagerDoesnotAllowToReAddUnchangedOrModifiedOrDeletedEntity(existingEntry.State);
                    }
                    else
                    {
                        // AttachTo:
                        // Attach is no-op when the existing entry is not a KeyEntry
                        // and it's entity is the same entity instance and it's state is Unchanged

                        // AddObject:
                        // AddObject is no-op when the existing entry's entity is the same entity 
                        // instance and it's state is Added
                        isNoOperation = true;
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Adds an object to the cache.  If it doesn't already have an entity key, the
        /// entity set is determined based on the type and the O-C map.
        /// If the object supports relationships (i.e. it implements IEntityWithRelationships),
        /// this also sets the context onto its RelationshipManager object.
        /// </summary>
        /// <param name="entitySetName">entitySetName the Object to be added. It might be qualifed with container name </param>
        /// <param name="entity">Object to be added.</param>
        public void AddObject(string entitySetName, object entity)
        {
            Debug.Assert(!(entity is IEntityWrapper), "Object is an IEntityWrapper instance instead of the raw entity.");
            ObjectStateManager.AssertAllForeignKeyIndexEntriesAreValid();
            EntityUtil.CheckArgumentNull(entity, "entity");

            EntityEntry existingEntry;
            IEntityWrapper wrappedEntity = EntityWrapperFactory.WrapEntityUsingContextGettingEntry(entity, this, out existingEntry);

            if (existingEntry == null)
            {
                // If the exact object being added is already in the context, there there is no way we need to
                // load the type for it, and since this is expensive, we only do the load if we have to.

                // SQLBUDT 480919: Ensure the assembly containing the entity's CLR type is loaded into the workspace.
                // If the schema types are not loaded: metadata, cache & query would be unable to reason about the type.
                // We will auto-load the entity type's assembly into the ObjectItemCollection.
                // We don't need the user's calling assembly for LoadAssemblyForType since entityType is sufficient.
                MetadataWorkspace.ImplicitLoadAssemblyForType(wrappedEntity.IdentityType, null);
            }
            else
            {
                Debug.Assert((object)existingEntry.Entity == (object)entity, "FindEntityEntry should return null if existing entry contains a different object.");
            }

            EntitySet entitySet;
            bool isNoOperation;

            this.VerifyRootForAdd(false, entitySetName, wrappedEntity, existingEntry, out entitySet, out isNoOperation);
            if (isNoOperation)
            {
                return;
            }

            System.Data.Objects.Internal.TransactionManager transManager = ObjectStateManager.TransactionManager;
            transManager.BeginAddTracking();

            try
            {
                RelationshipManager relationshipManager = wrappedEntity.RelationshipManager;
                Debug.Assert(relationshipManager != null, "Entity wrapper returned a null RelationshipManager");

                bool doCleanup = true;
                try
                {
                    // Add the root of the graph to the cache.
                    AddSingleObject(entitySet, wrappedEntity, "entity");
                    doCleanup = false;
                }
                finally
                {
                    // If we failed after adding the entry but before completely attaching the related ends to the context, we need to do some cleanup.
                    // If the context is null, we didn't even get as far as trying to attach the RelationshipManager, so something failed before the entry
                    // was even added, therefore there is nothing to clean up.
                    if (doCleanup && wrappedEntity.Context == this)
                    {
                        // If the context is not null, it be because the failure happened after it was attached, or it
                        // could mean that this entity was already attached, in which case we don't want to clean it up
                        // If we find the entity in the context and its key is temporary, we must have just added it, so remove it now.
                        EntityEntry entry = this.ObjectStateManager.FindEntityEntry(wrappedEntity.Entity);
                        if (entry != null && entry.EntityKey.IsTemporary)
                        {
                            // devnote: relationshipManager is valid, so entity must be IEntityWithRelationships and casting is safe
                            relationshipManager.NodeVisited = true;
                            // devnote: even though we haven't added the rest of the graph yet, we need to go through the related ends and
                            //          clean them up, because some of them could have been attached to the context before the failure occurred
                            RelationshipManager.RemoveRelatedEntitiesFromObjectStateManager(wrappedEntity);
                            RelatedEnd.RemoveEntityFromObjectStateManager(wrappedEntity);
                        }
                        // else entry was not added or the key is not temporary, so it must have already been in the cache before we tried to add this product, so don't remove anything
                    }
                }
                relationshipManager.AddRelatedEntitiesToObjectStateManager(/*doAttach*/false);
            }
            finally
            {
                transManager.EndAddTracking();
                ObjectStateManager.AssertAllForeignKeyIndexEntriesAreValid();
            }
        }
        /// <summary>
        /// Adds an object to the cache without adding its related
        /// entities.
        /// </summary>
        /// <param name="entity">Object to be added.</param>
        /// <param name="setName">EntitySet name for the Object to be added. It may be qualified with container name</param>
        /// <param name="containerName">Container name for the Object to be added.</param>
        /// <param name="argumentName">Name of the argument passed to a public method, for use in exceptions.</param>
        internal void AddSingleObject(EntitySet entitySet, IEntityWrapper wrappedEntity, string argumentName)
        {
            Debug.Assert(entitySet != null, "The extent for an entity must be a non-null entity set.");
            Debug.Assert(wrappedEntity != null, "The entity wrapper must not be null.");
            Debug.Assert(wrappedEntity.Entity != null, "The entity must not be null.");

            EntityKey key = wrappedEntity.GetEntityKeyFromEntity();
            if (null != (object)key)
            {
                EntityUtil.ValidateEntitySetInKey(key, entitySet);
                key.ValidateEntityKey(_workspace, entitySet);
            }

            VerifyContextForAddOrAttach(wrappedEntity);
            wrappedEntity.Context = this;
            EntityEntry entry = this.ObjectStateManager.AddEntry(wrappedEntity, (EntityKey)null, entitySet, argumentName, true);

            // If the entity supports relationships, AttachContext on the
            // RelationshipManager object - with load option of
            // AppendOnly (if adding a new object to a context, set
            // the relationships up to cache by default -- load option
            // is only set to other values when AttachContext is
            // called by the materializer). Also add all related entitites to
            // cache.
            //
            // NOTE: AttachContext must be called after adding the object to
            // the cache--otherwise the object might not have a key
            // when the EntityCollections expect it to.            
            Debug.Assert(this.ObjectStateManager.TransactionManager.TrackProcessedEntities, "Expected tracking processed entities to be true when adding.");
            Debug.Assert(this.ObjectStateManager.TransactionManager.ProcessedEntities != null, "Expected non-null collection when flag set.");

            this.ObjectStateManager.TransactionManager.ProcessedEntities.Add(wrappedEntity);

            wrappedEntity.AttachContext(this, entitySet, MergeOption.AppendOnly);

            // Find PK values in referenced principals and use these to set FK values
            entry.FixupFKValuesFromNonAddedReferences();

            _cache.FixupReferencesByForeignKeys(entry);
            wrappedEntity.TakeSnapshotOfRelationships(entry);
        }

        /// <summary>
        /// Explicitly loads a referenced entity or collection of entities into the given entity.
        /// </summary>
        /// <remarks>
        /// After loading, the referenced entity or collection can be accessed through the properties
        /// of the source entity.
        /// </remarks>
        /// <param name="entity">The source entity on which the relationship is defined</param>
        /// <param name="navigationProperty">The name of the property to load</param>
        public void LoadProperty(object entity, string navigationProperty)
        {
            IEntityWrapper wrappedEntity = WrapEntityAndCheckContext(entity, "property");
            wrappedEntity.RelationshipManager.GetRelatedEnd(navigationProperty).Load();
        }

        /// <summary>
        /// Explicitly loads a referenced entity or collection of entities into the given entity.
        /// </summary>
        /// <remarks>
        /// After loading, the referenced entity or collection can be accessed through the properties
        /// of the source entity.
        /// </remarks>
        /// <param name="entity">The source entity on which the relationship is defined</param>
        /// <param name="navigationProperty">The name of the property to load</param>
        /// <param name="mergeOption">The merge option to use for the load</param>
        public void LoadProperty(object entity, string navigationProperty, MergeOption mergeOption)
        {
            IEntityWrapper wrappedEntity = WrapEntityAndCheckContext(entity, "property");
            wrappedEntity.RelationshipManager.GetRelatedEnd(navigationProperty).Load(mergeOption);
        }

        /// <summary>
        /// Explicitly loads a referenced entity or collection of entities into the given entity.
        /// </summary>
        /// <remarks>
        /// After loading, the referenced entity or collection can be accessed through the properties
        /// of the source entity.
        /// The property to load is specified by a LINQ expression which must be in the form of
        /// a simple property member access.  For example, <code>(entity) => entity.PropertyName</code>
        /// where PropertyName is the navigation property to be loaded.  Other expression forms will
        /// be rejected at runtime.
        /// </remarks>
        /// <param name="entity">The source entity on which the relationship is defined</param>
        /// <param name="selector">A LINQ expression specifying the property to load</param>
        public void LoadProperty<TEntity>(TEntity entity, Expression<Func<TEntity, object>> selector)
        {
            // We used to throw an ArgumentException if the expression contained a Convert.  Now we remove the convert,
            // but if we still need to throw, then we should still throw an ArgumentException to avoid a breaking change.
            // Therefore, we keep track of whether or not we removed the convert.
            bool removedConvert;
            var navProp = ParsePropertySelectorExpression<TEntity>(selector, out removedConvert);
            IEntityWrapper wrappedEntity = WrapEntityAndCheckContext(entity, "property");
            wrappedEntity.RelationshipManager.GetRelatedEnd(navProp, throwArgumentException: removedConvert).Load();
        }

        /// <summary>
        /// Explicitly loads a referenced entity or collection of entities into the given entity.
        /// </summary>
        /// <remarks>
        /// After loading, the referenced entity or collection can be accessed through the properties
        /// of the source entity.
        /// The property to load is specified by a LINQ expression which must be in the form of
        /// a simple property member access.  For example, <code>(entity) => entity.PropertyName</code>
        /// where PropertyName is the navigation property to be loaded.  Other expression forms will
        /// be rejected at runtime.
        /// </remarks>
        /// <param name="entity">The source entity on which the relationship is defined</param>
        /// <param name="selector">A LINQ expression specifying the property to load</param>
        /// <param name="mergeOption">The merge option to use for the load</param>
        public void LoadProperty<TEntity>(TEntity entity, Expression<Func<TEntity, object>> selector, MergeOption mergeOption)
        {
            // We used to throw an ArgumentException if the expression contained a Convert.  Now we remove the convert,
            // but if we still need to throw, then we should still throw an ArgumentException to avoid a breaking change.
            // Therefore, we keep track of whether or not we removed the convert.
            bool removedConvert;
            var navProp = ParsePropertySelectorExpression<TEntity>(selector, out removedConvert);
            IEntityWrapper wrappedEntity = WrapEntityAndCheckContext(entity, "property");
            wrappedEntity.RelationshipManager.GetRelatedEnd(navProp, throwArgumentException: removedConvert).Load(mergeOption);
        }

        // Wraps the given entity and checks that it has a non-null context (i.e. that is is not detached).
        private IEntityWrapper WrapEntityAndCheckContext(object entity, string refType)
        {
            IEntityWrapper wrappedEntity = EntityWrapperFactory.WrapEntityUsingContext(entity, this);
            if (wrappedEntity.Context == null)
            {
                throw new InvalidOperationException(System.Data.Entity.Strings.ObjectContext_CannotExplicitlyLoadDetachedRelationships(refType));
            }
            if (wrappedEntity.Context != this)
            {
                throw new InvalidOperationException(System.Data.Entity.Strings.ObjectContext_CannotLoadReferencesUsingDifferentContext(refType));
            }
            return wrappedEntity;
        }

        // Validates that the given property selector may represent a navigation property and returns the nav prop string.
        // The actual check that the navigation property is valid is performed by the
        // RelationshipManager while loading the RelatedEnd.
        internal static string ParsePropertySelectorExpression<TEntity>(Expression<Func<TEntity, object>> selector, out bool removedConvert)
        {
            EntityUtil.CheckArgumentNull(selector, "selector");

            // We used to throw an ArgumentException if the expression contained a Convert.  Now we remove the convert,
            // but if we still need to throw, then we should still throw an ArgumentException to avoid a breaking change.
            // Therefore, we keep track of whether or not we removed the convert.
            removedConvert = false;
            var body = selector.Body;
            while (body.NodeType == ExpressionType.Convert || body.NodeType == ExpressionType.ConvertChecked)
            {
                removedConvert = true;
                body = ((UnaryExpression)body).Operand;
            }

            var bodyAsMember = body as MemberExpression;
            if (bodyAsMember == null ||
                !bodyAsMember.Member.DeclaringType.IsAssignableFrom(typeof(TEntity)) ||
                bodyAsMember.Expression.NodeType != ExpressionType.Parameter)
            {
                throw new ArgumentException(System.Data.Entity.Strings.ObjectContext_SelectorExpressionMustBeMemberAccess);
            }
            return bodyAsMember.Member.Name;
        }

        /// <summary>
        /// Apply modified properties to the original object.
        /// This API is obsolete.  Please use ApplyCurrentValues instead.
        /// </summary>
        /// <param name="entitySetName">name of EntitySet of entity to be updated</param>
        /// <param name="changed">object with modified properties</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Browsable(false)]
        [Obsolete("Use ApplyCurrentValues instead")]
        public void ApplyPropertyChanges(string entitySetName, object changed)
        {
            EntityUtil.CheckStringArgument(entitySetName, "entitySetName");
            EntityUtil.CheckArgumentNull(changed, "changed");

            this.ApplyCurrentValues(entitySetName, changed);
        }

        /// <summary>
        /// Apply modified properties to the original object.
        /// </summary>
        /// <param name="entitySetName">name of EntitySet of entity to be updated</param>
        /// <param name="currentEntity">object with modified properties</param>
        public TEntity ApplyCurrentValues<TEntity>(string entitySetName, TEntity currentEntity) where TEntity : class
        {
            EntityUtil.CheckStringArgument(entitySetName, "entitySetName");
            EntityUtil.CheckArgumentNull(currentEntity, "currentEntity");
            IEntityWrapper wrappedEntity = EntityWrapperFactory.WrapEntityUsingContext(currentEntity, this);

            // SQLBUDT 480919: Ensure the assembly containing the entity's CLR type is loaded into the workspace.
            // If the schema types are not loaded: metadata, cache & query would be unable to reason about the type.
            // We will auto-load the entity type's assembly into the ObjectItemCollection.
            // We don't need the user's calling assembly for LoadAssemblyForType since entityType is sufficient.
            MetadataWorkspace.ImplicitLoadAssemblyForType(wrappedEntity.IdentityType, null);

            EntitySet entitySet = this.GetEntitySetFromName(entitySetName);

            EntityKey key = wrappedEntity.EntityKey;
            if (null != (object)key)
            {
                EntityUtil.ValidateEntitySetInKey(key, entitySet, "entitySetName");
                key.ValidateEntityKey(_workspace, entitySet);
            }
            else
            {
                key = this.ObjectStateManager.CreateEntityKey(entitySet, currentEntity);
            }

            // Check if entity is already in the cache
            EntityEntry ose = this.ObjectStateManager.FindEntityEntry(key);
            if (ose == null || ose.IsKeyEntry)
            {
                throw EntityUtil.EntityNotTracked();
            }

            ose.ApplyCurrentValuesInternal(wrappedEntity);

            return (TEntity)ose.Entity;
        }

        /// <summary>
        /// Apply original values to the entity.
        /// The entity to update is found based on key values of the <paramref name="originalEntity"/> entity and the given <paramref name="entitySetName"/>.
        /// </summary>
        /// <param name="entitySetName">name of EntitySet of entity to be updated</param>
        /// <param name="originalEntity">object with original values</param>
        /// <returns>updated entity</returns>
        public TEntity ApplyOriginalValues<TEntity>(string entitySetName, TEntity originalEntity) where TEntity : class
        {
            EntityUtil.CheckStringArgument(entitySetName, "entitySetName");
            EntityUtil.CheckArgumentNull(originalEntity, "originalEntity");

            IEntityWrapper wrappedOriginalEntity = EntityWrapperFactory.WrapEntityUsingContext(originalEntity, this);

            // SQLBUDT 480919: Ensure the assembly containing the entity's CLR type is loaded into the workspace.
            // If the schema types are not loaded: metadata, cache & query would be unable to reason about the type.
            // We will auto-load the entity type's assembly into the ObjectItemCollection.
            // We don't need the user's calling assembly for LoadAssemblyForType since entityType is sufficient.
            MetadataWorkspace.ImplicitLoadAssemblyForType(wrappedOriginalEntity.IdentityType, null);

            EntitySet entitySet = this.GetEntitySetFromName(entitySetName);

            EntityKey key = wrappedOriginalEntity.EntityKey;
            if (null != (object)key)
            {
                EntityUtil.ValidateEntitySetInKey(key, entitySet, "entitySetName");
                key.ValidateEntityKey(_workspace, entitySet);
            }
            else
            {
                key = this.ObjectStateManager.CreateEntityKey(entitySet, originalEntity);
            }

            // Check if the entity is already in the cache
            EntityEntry ose = this.ObjectStateManager.FindEntityEntry(key);
            if (ose == null || ose.IsKeyEntry)
            {
                throw EntityUtil.EntityNotTrackedOrHasTempKey();
            }

            if (ose.State != EntityState.Modified &&
                ose.State != EntityState.Unchanged &&
                ose.State != EntityState.Deleted)
            {
                throw EntityUtil.EntityMustBeUnchangedOrModifiedOrDeleted(ose.State);
            }

            if (ose.WrappedEntity.IdentityType != wrappedOriginalEntity.IdentityType)
            {
                throw EntityUtil.EntitiesHaveDifferentType(ose.Entity.GetType().FullName, originalEntity.GetType().FullName);
            }

            ose.CompareKeyProperties(originalEntity);

            // The ObjectStateEntry.UpdateModifiedFields uses a variation of Shaper.UpdateRecord method 
            // which additionaly marks properties as modified as necessary.
            ose.UpdateOriginalValues(wrappedOriginalEntity.Entity);

            // return the current entity
            return (TEntity)ose.Entity;
        }


        /// <summary>
        /// Attach entity graph into the context in the Unchanged state.
        /// This version takes entity which doesn't have to have a Key.
        /// </summary>
        /// <param name="entitySetName">EntitySet name for the Object to be attached. It may be qualified with container name</param>        
        /// <param name="entity"></param>
        public void AttachTo(string entitySetName, object entity)
        {
            Debug.Assert(!(entity is IEntityWrapper), "Object is an IEntityWrapper instance instead of the raw entity.");
            EntityUtil.CheckArgumentNull(entity, "entity");
            ObjectStateManager.AssertAllForeignKeyIndexEntriesAreValid();

            EntityEntry existingEntry;
            IEntityWrapper wrappedEntity = EntityWrapperFactory.WrapEntityUsingContextGettingEntry(entity, this, out existingEntry);

            if (existingEntry == null)
            {
                // If the exact object being added is already in the context, there there is no way we need to
                // load the type for it, and since this is expensive, we only do the load if we have to.

                // SQLBUDT 480919: Ensure the assembly containing the entity's CLR type is loaded into the workspace.
                // If the schema types are not loaded: metadata, cache & query would be unable to reason about the type.
                // We will auto-load the entity type's assembly into the ObjectItemCollection.
                // We don't need the user's calling assembly for LoadAssemblyForType since entityType is sufficient.
                MetadataWorkspace.ImplicitLoadAssemblyForType(wrappedEntity.IdentityType, null);
            }
            else
            {
                Debug.Assert((object)existingEntry.Entity == (object)entity, "FindEntityEntry should return null if existing entry contains a different object.");
            }

            EntitySet entitySet;
            bool isNoOperation;

            this.VerifyRootForAdd(true, entitySetName, wrappedEntity, existingEntry, out entitySet, out isNoOperation);
            if (isNoOperation)
            {
                return;
            }

            System.Data.Objects.Internal.TransactionManager transManager = ObjectStateManager.TransactionManager;
            transManager.BeginAttachTracking();

            try
            {
                this.ObjectStateManager.TransactionManager.OriginalMergeOption = wrappedEntity.MergeOption;
                RelationshipManager relationshipManager = wrappedEntity.RelationshipManager;
                Debug.Assert(relationshipManager != null, "Entity wrapper returned a null RelationshipManager");

                bool doCleanup = true;
                try
                {
                    // Attach the root of entity graph to the cache.
                    AttachSingleObject(wrappedEntity, entitySet, "entity");
                    doCleanup = false;
                }
                finally
                {
                    // SQLBU 555615 Be sure that wrappedEntity.Context == this to not try to detach 
                    // entity from context if it was already attached to some other context.
                    // It's enough to check this only for the root of the graph since we can assume that all entities
                    // in the graph are attached to the same context (or none of them is attached).
                    if (doCleanup && wrappedEntity.Context == this)
                    {
                        // SQLBU 509900 RIConstraints: Entity still exists in cache after Attach fails
                        //
                        // Cleaning up is needed only when root of the graph violates some referential constraint.
                        // Normal cleaning is done in RelationshipManager.AddRelatedEntitiesToObjectStateManager()
                        // (referential constraints properties are checked in AttachSingleObject(), before
                        // AddRelatedEntitiesToObjectStateManager is called, that's why normal cleaning
                        // doesn't work in this case)

                        relationshipManager.NodeVisited = true;
                        // devnote: even though we haven't attached the rest of the graph yet, we need to go through the related ends and
                        //          clean them up, because some of them could have been attached to the context.
                        RelationshipManager.RemoveRelatedEntitiesFromObjectStateManager(wrappedEntity);
                        RelatedEnd.RemoveEntityFromObjectStateManager(wrappedEntity);
                    }
                }
                relationshipManager.AddRelatedEntitiesToObjectStateManager(/*doAttach*/true);
            }
            finally
            {
                transManager.EndAttachTracking();
                ObjectStateManager.AssertAllForeignKeyIndexEntriesAreValid();
            }
        }
        /// <summary>
        /// Attach entity graph into the context in the Unchanged state.
        /// This version takes entity which does have to have a non-temporary Key.
        /// </summary>
        /// <param name="entity"></param>        
        public void Attach(IEntityWithKey entity)
        {
            EntityUtil.CheckArgumentNull(entity, "entity");

            if (null == (object)entity.EntityKey)
            {
                throw EntityUtil.CannotAttachEntityWithoutKey();
            }

            this.AttachTo(null, entity);
        }
        /// <summary>
        /// Attaches single object to the cache without adding its related entities.
        /// </summary>
        /// <param name="entity">Entity to be attached.</param>
        /// <param name="entitySet">"Computed" entity set.</param>
        /// <param name="argumentName">Name of the argument passed to a public method, for use in exceptions.</param>
        internal void AttachSingleObject(IEntityWrapper wrappedEntity, EntitySet entitySet, string argumentName)
        {
            Debug.Assert(wrappedEntity != null, "entity wrapper shouldn't be null");
            Debug.Assert(wrappedEntity.Entity != null, "entity shouldn't be null");
            Debug.Assert(entitySet != null, "entitySet shouldn't be null");

            // Try to detect if the entity is invalid as soon as possible
            // (before adding the entity to the ObjectStateManager)
            RelationshipManager relationshipManager = wrappedEntity.RelationshipManager;
            Debug.Assert(relationshipManager != null, "Entity wrapper returned a null RelationshipManager");

            EntityKey key = wrappedEntity.GetEntityKeyFromEntity();
            if (null != (object)key)
            {
                EntityUtil.ValidateEntitySetInKey(key, entitySet);
                key.ValidateEntityKey(_workspace, entitySet);
            }
            else
            {
                key = this.ObjectStateManager.CreateEntityKey(entitySet, wrappedEntity.Entity);
            }

            Debug.Assert(key != null, "GetEntityKey should have returned a non-null key");

            // Temporary keys are not allowed
            if (key.IsTemporary)
            {
                throw EntityUtil.CannotAttachEntityWithTemporaryKey();
            }

            if (wrappedEntity.EntityKey != key)
            {
                wrappedEntity.EntityKey = key;
            }

            // Check if entity already exists in the cache.
            // NOTE: This check could be done earlier, but this way I avoid creating key twice.
            EntityEntry entry = ObjectStateManager.FindEntityEntry(key);

            if (null != entry)
            {
                if (entry.IsKeyEntry)
                {
                    // devnote: SQLBU 555615. This method was extracted from PromoteKeyEntry to have consistent
                    // behavior of AttachTo in case of attaching entity which is already attached to some other context.
                    // We can not detect if entity is attached to another context until we call SetChangeTrackerOntoEntity
                    // which throws exception if the change tracker is already set.  
                    // SetChangeTrackerOntoEntity is now called from PromoteKeyEntryInitialization(). 
                    // Calling PromoteKeyEntryInitialization() before calling relationshipManager.AttachContext prevents
                    // overriding Context property on relationshipManager (and attaching relatedEnds to current context).
                    this.ObjectStateManager.PromoteKeyEntryInitialization(this, entry, wrappedEntity, /*shadowValues*/ null, /*replacingEntry*/ false);

                    Debug.Assert(this.ObjectStateManager.TransactionManager.TrackProcessedEntities, "Expected tracking processed entities to be true when adding.");
                    Debug.Assert(this.ObjectStateManager.TransactionManager.ProcessedEntities != null, "Expected non-null collection when flag set.");

                    this.ObjectStateManager.TransactionManager.ProcessedEntities.Add(wrappedEntity);

                    wrappedEntity.TakeSnapshotOfRelationships(entry);

                    this.ObjectStateManager.PromoteKeyEntry(entry,
                        wrappedEntity,
                        /*shadowValues*/ null,
                        /*replacingEntry*/ false,
                        /*setIsLoaded*/ false,
                        /*keyEntryInitialized*/ true,
                        "Attach");

                    ObjectStateManager.FixupReferencesByForeignKeys(entry);

                    relationshipManager.CheckReferentialConstraintProperties(entry);
                }
                else
                {
                    Debug.Assert(!Object.ReferenceEquals(entry.Entity, wrappedEntity.Entity));
                    throw EntityUtil.ObjectStateManagerContainsThisEntityKey();
                }
            }
            else
            {
                VerifyContextForAddOrAttach(wrappedEntity);
                wrappedEntity.Context = this;
                entry = this.ObjectStateManager.AttachEntry(key, wrappedEntity, entitySet, argumentName);

                Debug.Assert(this.ObjectStateManager.TransactionManager.TrackProcessedEntities, "Expected tracking processed entities to be true when adding.");
                Debug.Assert(this.ObjectStateManager.TransactionManager.ProcessedEntities != null, "Expected non-null collection when flag set.");

                this.ObjectStateManager.TransactionManager.ProcessedEntities.Add(wrappedEntity);

                wrappedEntity.AttachContext(this, entitySet, MergeOption.AppendOnly);

                ObjectStateManager.FixupReferencesByForeignKeys(entry);
                wrappedEntity.TakeSnapshotOfRelationships(entry);

                relationshipManager.CheckReferentialConstraintProperties(entry);
            }
        }

        /// <summary>
        /// When attaching we need to check that the entity is not already attached to a different context
        /// before we wipe away that context.
        /// </summary>
        private void VerifyContextForAddOrAttach(IEntityWrapper wrappedEntity)
        {
            if (wrappedEntity.Context != null &&
                wrappedEntity.Context != this &&
                !wrappedEntity.Context.ObjectStateManager.IsDisposed &&
                wrappedEntity.MergeOption != MergeOption.NoTracking)
            {
                throw EntityUtil.EntityCantHaveMultipleChangeTrackers();
            }
        }

        /// <summary>
        /// Create entity key based on given entity set and values of given entity.
        /// </summary>
        /// <param name="entitySetName">entity set of the entity</param>
        /// <param name="entity">entity</param>
        /// <returns>new instance of entity key</returns>
        public EntityKey CreateEntityKey(string entitySetName, object entity)
        {
            Debug.Assert(!(entity is IEntityWrapper), "Object is an IEntityWrapper instance instead of the raw entity.");
            EntityUtil.CheckStringArgument(entitySetName, "entitySetName");
            EntityUtil.CheckArgumentNull(entity, "entity");

            // SQLBUDT 480919: Ensure the assembly containing the entity's CLR type is loaded into the workspace.
            // If the schema types are not loaded: metadata, cache & query would be unable to reason about the type.
            // We will auto-load the entity type's assembly into the ObjectItemCollection.
            // We don't need the user's calling assembly for LoadAssemblyForType since entityType is sufficient.
            MetadataWorkspace.ImplicitLoadAssemblyForType(EntityUtil.GetEntityIdentityType(entity.GetType()), null);

            EntitySet entitySet = this.GetEntitySetFromName(entitySetName);

            return this.ObjectStateManager.CreateEntityKey(entitySet, entity);
        }

        internal EntitySet GetEntitySetFromName(string entitySetName)
        {
            string setName;
            string containerName;

            ObjectContext.GetEntitySetName(entitySetName, "entitySetName", this, out setName, out containerName);

            // Find entity set using entitySetName and entityContainerName
            return this.GetEntitySet(setName, containerName);
        }

        private void AddRefreshKey(object entityLike, Dictionary<EntityKey, EntityEntry> entities, Dictionary<EntitySet, List<EntityKey>> currentKeys)
        {
            Debug.Assert(!(entityLike is IEntityWrapper), "Object is an IEntityWrapper instance instead of the raw entity.");
            if (null == entityLike)
            {
                throw EntityUtil.NthElementIsNull(entities.Count);
            }

            IEntityWrapper wrappedEntity = EntityWrapperFactory.WrapEntityUsingContext(entityLike, this);
            EntityKey key = wrappedEntity.EntityKey;
            RefreshCheck(entities, entityLike, key);

            // Retrieve the EntitySet for the EntityKey and add an entry in the dictionary
            // that maps a set to the keys of entities that should be refreshed from that set.
            EntitySet entitySet = key.GetEntitySet(this.MetadataWorkspace);

            List<EntityKey> setKeys = null;
            if (!currentKeys.TryGetValue(entitySet, out setKeys))
            {
                setKeys = new List<EntityKey>();
                currentKeys.Add(entitySet, setKeys);
            }

            setKeys.Add(key);
        }

        /// <summary>
        /// Creates an ObjectSet based on the EntitySet that is defined for TEntity.
        /// Requires that the DefaultContainerName is set for the context and that there is a
        /// single EntitySet for the specified type. Throws exception if more than one type is found.
        /// </summary>
        /// <typeparam name="TEntity">Entity type for the requested ObjectSet</typeparam>
        public ObjectSet<TEntity> CreateObjectSet<TEntity>()
            where TEntity : class
        {
            EntitySet entitySet = GetEntitySetForType(typeof(TEntity), "TEntity");
            return new ObjectSet<TEntity>(entitySet, this);
        }

        /// <summary>
        /// Find the EntitySet in the default EntityContainer for the specified CLR type.
        /// Must be a valid mapped entity type and must be mapped to exactly one EntitySet across all of the EntityContainers in the metadata for this context.
        /// </summary>
        /// <param name="entityCLRType">CLR type to use for EntitySet lookup.</param>
        /// <returns></returns>
        private EntitySet GetEntitySetForType(Type entityCLRType, string exceptionParameterName)
        {
            EntitySet entitySetForType = null;

            EntityContainer defaultContainer = this.Perspective.GetDefaultContainer();
            if (defaultContainer == null)
            {
                // We don't have a default container, so look through all EntityContainers in metadata to see if
                // we can find exactly one EntitySet that matches the specified CLR type.
                System.Collections.ObjectModel.ReadOnlyCollection<EntityContainer> entityContainers = this.MetadataWorkspace.GetItems<EntityContainer>(DataSpace.CSpace);
                foreach (EntityContainer entityContainer in entityContainers)
                {
                    // See if this container has exactly one EntitySet for this type
                    EntitySet entitySetFromContainer = GetEntitySetFromContainer(entityContainer, entityCLRType, exceptionParameterName);

                    if (entitySetFromContainer != null)
                    {
                        // Verify we haven't already found a matching EntitySet in some other container
                        if (entitySetForType != null)
                        {
                            // There is more than one EntitySet for this type across all containers in metadata, so we can't determine which one the user intended
                            throw EntityUtil.MultipleEntitySetsFoundInAllContainers(entityCLRType.FullName, exceptionParameterName);
                        }

                        entitySetForType = entitySetFromContainer;
                    }
                }
            }
            else
            {
                // There is a default container, so restrict the search to EntitySets within it
                entitySetForType = GetEntitySetFromContainer(defaultContainer, entityCLRType, exceptionParameterName);
            }

            // We still may not have found a matching EntitySet for this type
            if (entitySetForType == null)
            {
                throw EntityUtil.NoEntitySetFoundForType(entityCLRType.FullName, exceptionParameterName);
            }

            return entitySetForType;
        }

        private EntitySet GetEntitySetFromContainer(EntityContainer container, Type entityCLRType, string exceptionParameterName)
        {
            // Verify that we have an EdmType mapping for the specified CLR type
            EdmType entityEdmType = GetTypeUsage(entityCLRType).EdmType;

            // Try to find a single EntitySet for the specified type
            EntitySet entitySet = null;
            foreach (EntitySetBase es in container.BaseEntitySets)
            {
                // This is a match if the set is an EntitySet (not an AssociationSet) and the EntitySet
                // is defined for the specified entity type. Must be an exact match, not a base type. 
                if (es.BuiltInTypeKind == BuiltInTypeKind.EntitySet && es.ElementType == entityEdmType)
                {
                    if (entitySet != null)
                    {
                        // There is more than one EntitySet for this type, so we can't determine which one the user intended
                        throw EntityUtil.MultipleEntitySetsFoundInSingleContainer(entityCLRType.FullName, container.Name, exceptionParameterName);
                    }

                    entitySet = (EntitySet)es;
                }
            }

            return entitySet;
        }

        /// <summary>
        /// Creates an ObjectSet based on the specified EntitySet name.
        /// </summary>
        /// <typeparam name="TEntity">Expected type of the EntitySet</typeparam>
        /// <param name="entitySetName">
        /// EntitySet to use for the ObjectSet. Can be fully-qualified or unqualified if the DefaultContainerName is set.
        /// </param>
        public ObjectSet<TEntity> CreateObjectSet<TEntity>(string entitySetName)
            where TEntity : class
        {
            EntitySet entitySet = GetEntitySetForNameAndType(entitySetName, typeof(TEntity), "TEntity");
            return new ObjectSet<TEntity>(entitySet, this);
        }

        /// <summary>
        /// Finds an EntitySet with the specified name and verifies that its type matches the specified type.
        /// </summary>
        /// <param name="entitySetName">
        /// Name of the EntitySet to find. Can be fully-qualified or unqualified if the DefaultContainerName is set
        /// </param>
        /// <param name="entityCLRType">
        /// Expected CLR type of the EntitySet. Must exactly match the type for the EntitySet, base types are not valid.
        /// </param>
        /// <param name="exceptionParameterName">Argument name to use if an exception occurs.</param>
        /// <returns>EntitySet that was found in metadata with the specified parameters</returns>
        private EntitySet GetEntitySetForNameAndType(string entitySetName, Type entityCLRType, string exceptionParameterName)
        {
            // Verify that the specified entitySetName exists in metadata
            EntitySet entitySet = GetEntitySetFromName(entitySetName);

            // Verify that the EntitySet type matches the specified type exactly (a base type is not valid)
            EdmType entityEdmType = GetTypeUsage(entityCLRType).EdmType;
            if (entitySet.ElementType != entityEdmType)
            {
                throw EntityUtil.InvalidEntityTypeForObjectSet(entityCLRType.FullName, entitySet.ElementType.FullName, entitySetName, exceptionParameterName);
            }

            return entitySet;
        }

        #region Connection Management

        /// <summary>
        /// Ensures that the connection is opened for an operation that requires an open connection to the store.
        /// Calls to EnsureConnection MUST be matched with a single call to ReleaseConnection.
        /// </summary>
        /// <exception cref="ObjectDisposedException">If the <see cref="ObjectContext"/> instance has been disposed.</exception>
        internal void EnsureConnection()
        {
            if (_connection == null)
            {
                throw EntityUtil.ObjectContextDisposed();
            }

            if (ConnectionState.Closed == Connection.State)
            {
                Connection.Open();
                _openedConnection = true;
            }

            if (_openedConnection)
            {
                _connectionRequestCount++;
            }

            // Check the connection was opened correctly
            if ((_connection.State == ConnectionState.Closed) || (_connection.State == ConnectionState.Broken))
            {
                string message = System.Data.Entity.Strings.EntityClient_ExecutingOnClosedConnection(
                       _connection.State == ConnectionState.Closed ? System.Data.Entity.Strings.EntityClient_ConnectionStateClosed : System.Data.Entity.Strings.EntityClient_ConnectionStateBroken);
                throw EntityUtil.InvalidOperation(message);
            }

            try
            {
                // Make sure the necessary metadata is registered
                EnsureMetadata();

                #region EnsureContextIsEnlistedInCurrentTransaction

                // The following conditions are no longer valid since Metadata Independence.
                Debug.Assert(ConnectionState.Open == _connection.State, "Connection must be open.");

                // IF YOU MODIFIED THIS TABLE YOU MUST UPDATE TESTS IN SaveChangesTransactionTests SUITE ACCORDINGLY AS SOME CASES REFER TO NUMBERS IN THIS TABLE
                //
                // TABLE OF ACTIONS WE PERFORM HERE:
                //
                //  #  lastTransaction     currentTransaction         ConnectionState   WillClose      Action                                  Behavior when no explicit transaction (started with .ElistTransaction())     Behavior with explicit transaction (started with .ElistTransaction())
                //  1   null                null                       Open              No             no-op;                                  implicit transaction will be created and used                                explicit transaction should be used
                //  2   non-null tx1        non-null tx1               Open              No             no-op;                                  the last transaction will be used                                            N/A - it is not possible to EnlistTransaction if another transaction has already enlisted
                //  3   null                non-null                   Closed            Yes            connection.Open();                      Opening connection will automatically enlist into Transaction.Current        N/A - cannot enlist in transaction on a closed connection
                //  4   null                non-null                   Open              No             connection.Enlist(currentTransaction);  currentTransaction enlisted and used                                         N/A - it is not possible to EnlistTransaction if another transaction has already enlisted
                //  5   non-null            null                       Open              No             no-op;                                  implicit transaction will be created and used                                explicit transaction should be used
                //  6   non-null            null                       Closed            Yes            no-op;                                  implicit transaction will be created and used                                N/A - cannot enlist in transaction on a closed connection
                //  7   non-null tx1        non-null tx2               Open              No             connection.Enlist(currentTransaction);  currentTransaction enlisted and used                                         N/A - it is not possible to EnlistTransaction if another transaction has already enlisted
                //  8   non-null tx1        non-null tx2               Open              Yes            connection.Close(); connection.Open();  Re-opening connection will automatically enlist into Transaction.Current     N/A - only applies to TransactionScope - requires two transactions and CommitableTransaction and TransactionScope cannot be mixed
                //  9   non-null tx1        non-null tx2               Closed            Yes            connection.Open();                      Opening connection will automatcially enlist into Transaction.Current        N/A - cannot enlist in transaction on a closed connection

                Transaction currentTransaction = Transaction.Current;

                bool transactionHasChanged = (null != currentTransaction && !currentTransaction.Equals(_lastTransaction))
                                          || (null != _lastTransaction && !_lastTransaction.Equals(currentTransaction));

                if (transactionHasChanged)
                {
                    if (!_openedConnection)
                    {
                        // We didn't open the connection so, just try to enlist the connection in the current transaction. 
                        // Note that the connection can already be enlisted in a transaction (since the user opened 
                        // it s/he could enlist it manually using EntityConnection.EnlistTransaction() method). If the 
                        // transaction the connection is enlisted in has not completed (e.g. nested transaction) this call 
                        // will fail (throw). Also currentTransaction can be null here which means that the transaction
                        // used in the previous operation has completed. In this case we should not enlist the connection
                        // in "null" transaction as the user might have enlisted in a transaction manually between calls by 
                        // calling EntityConnection.EnlistTransaction() method. Enlisting with null would in this case mean "unenlist" 
                        // and would cause an exception (see above). Had the user not enlisted in a transaction between the calls
                        // enlisting with null would be a no-op - so again no reason to do it. 
                        if (currentTransaction != null)
                        {
                            _connection.EnlistTransaction(currentTransaction);
                        }
                    }
                    else if (_connectionRequestCount > 1)
                    {
                        // We opened the connection. In addition we are here because there are multiple
                        // active requests going on (read: enumerators that has not been disposed yet) 
                        // using the same connection. (If there is only one active request e.g. like SaveChanges
                        // or single enumerator there is no need for any specific transaction handling - either
                        // we use the implicit ambient transaction (Transaction.Current) if one exists or we 
                        // will create our own local transaction. Also if there is only one active request
                        // the user could not enlist it in a transaction using EntityConnection.EnlistTransaction()
                        // because we opened the connection).
                        // If there are multiple active requests the user might have "played" with transactions
                        // after the first transaction. This code tries to deal with this kind of changes.

                        if (null == _lastTransaction)
                        {
                            Debug.Assert(currentTransaction != null, "transaction has changed and the lastTransaction was null");

                            // Two cases here: 
                            // - the previous operation was not run inside a transaction created by the user while this one is - just
                            //   enlist the connection in the transaction
                            // - the previous operation ran withing explicit transaction started with EntityConnection.EnlistTransaction()
                            //   method - try enlisting the connection in the transaction. This may fail however if the transactions 
                            //   are nested as you cannot enlist the connection in the transaction until the previous transaction has
                            //   completed.
                            _connection.EnlistTransaction(currentTransaction);
                        }
                        else
                        {
                            // We'll close and reopen the connection to get the benefit of automatic transaction enlistment.
                            // Remarks: We get here only if there is more than one active query (e.g. nested foreach or two subsequent queries or SaveChanges
                            // inside a for each) and each of these queries are using a different transaction (note that using TransactionScopeOption.Required 
                            // will not create a new transaction if an ambient transaction already exists - the ambient transaction will be used and we will 
                            // not end up in this code path). If we get here we are already in a loss-loss situation - we cannot enlist to the second transaction
                            // as this would cause an exception saying that there is already an active transaction that needs to be committed or rolled back
                            // before we can enlist the connection to a new transaction. The other option (and this is what we do here) is to close and reopen
                            // the connection. This will enlist the newly opened connection to the second transaction but will also close the reader being used
                            // by the first active query. As a result when trying to continue reading results from the first query the user will get an exception
                            // saying that calling "Read" on a closed data reader is not a valid operation.
                            _connection.Close();
                            _connection.Open();
                            _openedConnection = true;
                            _connectionRequestCount++;
                        }
                    }
                }
                else
                {
                    // we don't need to do anything, nothing has changed.
                }

                // If we get here, we have an open connection, either enlisted in the current
                // transaction (if it's non-null) or unenlisted from all transactions (if the
                // current transaction is null)
                _lastTransaction = currentTransaction;

                #endregion
            }
            catch (Exception)
            {
                // when the connection is unable to enlist properly or another error occured, be sure to release this connection
                ReleaseConnection();
                throw;
            }

        }

        /// <summary>
        /// Resets the state of connection management when the connection becomes closed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConnectionStateChange(object sender, StateChangeEventArgs e)
        {
            if (e.CurrentState == ConnectionState.Closed)
            {
                _connectionRequestCount = 0;
                _openedConnection = false;
            }
        }

        /// <summary>
        /// Releases the connection, potentially closing the connection if no active operations
        /// require the connection to be open. There should be a single ReleaseConnection call
        /// for each EnsureConnection call.
        /// </summary>
        /// <exception cref="ObjectDisposedException">If the <see cref="ObjectContext"/> instance has been disposed.</exception>
        internal void ReleaseConnection()
        {
            if (_connection == null)
            {
                throw EntityUtil.ObjectContextDisposed();
            }

            if (_openedConnection)
            {
                Debug.Assert(_connectionRequestCount > 0, "_connectionRequestCount is zero or negative");
                if (_connectionRequestCount > 0)
                {
                    _connectionRequestCount--;
                }

                // When no operation is using the connection and the context had opened the connection
                // the connection can be closed
                if (_connectionRequestCount == 0)
                {
                    Connection.Close();
                    _openedConnection = false;
                }
            }
        }

        internal void EnsureMetadata()
        {
            if (!MetadataWorkspace.IsItemCollectionAlreadyRegistered(DataSpace.SSpace))
            {
                Debug.Assert(!MetadataWorkspace.IsItemCollectionAlreadyRegistered(DataSpace.CSSpace), "ObjectContext has C-S metadata but not S?");

                // Only throw an ObjectDisposedException if an attempt is made to access the underlying connection object.
                if (_connection == null)
                {
                    throw EntityUtil.ObjectContextDisposed();
                }

                MetadataWorkspace connectionWorkspace = _connection.GetMetadataWorkspace();

                Debug.Assert(connectionWorkspace.IsItemCollectionAlreadyRegistered(DataSpace.CSpace) &&
                             connectionWorkspace.IsItemCollectionAlreadyRegistered(DataSpace.SSpace) &&
                             connectionWorkspace.IsItemCollectionAlreadyRegistered(DataSpace.CSSpace),
                            "EntityConnection.GetMetadataWorkspace() did not return an initialized workspace?");

                // Validate that the context's MetadataWorkspace and the underlying connection's MetadataWorkspace
                // have the same CSpace collection. Otherwise, an error will occur when trying to set the SSpace
                // and CSSpace metadata
                ItemCollection connectionCSpaceCollection = connectionWorkspace.GetItemCollection(DataSpace.CSpace);
                ItemCollection contextCSpaceCollection = MetadataWorkspace.GetItemCollection(DataSpace.CSpace);
                if (!object.ReferenceEquals(connectionCSpaceCollection, contextCSpaceCollection))
                {
                    throw EntityUtil.ContextMetadataHasChanged();
                }
                MetadataWorkspace.RegisterItemCollection(connectionWorkspace.GetItemCollection(DataSpace.SSpace));
                MetadataWorkspace.RegisterItemCollection(connectionWorkspace.GetItemCollection(DataSpace.CSSpace));
            }
        }

        #endregion

        /// <summary>
        /// Creates an ObjectQuery<typeparamref name="T"/> over the store, ready to be executed.
        /// </summary>
        /// <typeparam name="T">type of the query result</typeparam>
        /// <param name="queryString">the query string to be executed</param>
        /// <param name="parameters">parameters to pass to the query</param>
        /// <returns>an ObjectQuery instance, ready to be executed</returns>
        public ObjectQuery<T> CreateQuery<T>(string queryString, params ObjectParameter[] parameters)
        {
            EntityUtil.CheckArgumentNull(queryString, "queryString");
            EntityUtil.CheckArgumentNull(parameters, "parameters");

            // SQLBUDT 447285: Ensure the assembly containing the entity's CLR type is loaded into the workspace.
            // If the schema types are not loaded: metadata, cache & query would be unable to reason about the type.
            // We either auto-load <T>'s assembly into the ObjectItemCollection or we auto-load the user's calling assembly and its referenced assemblies.
            // If the entities in the user's result spans multiple assemblies, the user must manually call LoadFromAssembly.
            // *GetCallingAssembly returns the assembly of the method that invoked the currently executing method.
            MetadataWorkspace.ImplicitLoadAssemblyForType(typeof(T), System.Reflection.Assembly.GetCallingAssembly());

            // create a ObjectQuery<T> with default settings
            ObjectQuery<T> query = new ObjectQuery<T>(queryString, this, MergeOption.AppendOnly);

            foreach (ObjectParameter parameter in parameters)
            {
                query.Parameters.Add(parameter);
            }

            return query;
        }
        /// <summary>
        /// Creates an EntityConnection from the given connection string.
        /// </summary>
        /// <param name="connectionString">the connection string</param>
        /// <returns>the newly created connection</returns>
        [ResourceExposure(ResourceScope.Machine)] //Exposes the file names as part of ConnectionString which are a Machine resource
        [ResourceConsumption(ResourceScope.Machine)] //For EntityConnection constructor. But the paths are not created in this method.
        private static EntityConnection CreateEntityConnection(string connectionString)
        {
            EntityUtil.CheckStringArgument(connectionString, "connectionString");

            // create the connection
            EntityConnection connection = new EntityConnection(connectionString);

            return connection;
        }
        /// <summary>
        /// Given an entity connection, returns a copy of its MetadataWorkspace. Ensure we get
        /// all of the metadata item collections by priming the entity connection.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ObjectDisposedException">If the <see cref="ObjectContext"/> instance has been disposed.</exception>
        private MetadataWorkspace RetrieveMetadataWorkspaceFromConnection()
        {
            if (_connection == null)
            {
                throw EntityUtil.ObjectContextDisposed();
            }

            MetadataWorkspace connectionWorkspace = _connection.GetMetadataWorkspace(false /* initializeAllConnections */);
            Debug.Assert(connectionWorkspace != null, "EntityConnection.MetadataWorkspace is null.");

            // Create our own workspace
            MetadataWorkspace workspace = connectionWorkspace.ShallowCopy();

            return workspace;
        }
        /// <summary>
        /// Marks an object for deletion from the cache.
        /// </summary>
        /// <param name="entity">Object to be deleted.</param>
        public void DeleteObject(object entity)
        {
            ObjectStateManager.AssertAllForeignKeyIndexEntriesAreValid();
            // This method and ObjectSet.DeleteObject are expected to have identical behavior except for the extra validation ObjectSet
            // requests by passing a non-null expectedEntitySetName. Any changes to this method are expected to be made in the common
            // internal overload below that ObjectSet also uses, unless there is a specific reason why a behavior is desired when the
            // call comes from ObjectContext only.
            DeleteObject(entity, null /*expectedEntitySetName*/);
            ObjectStateManager.AssertAllForeignKeyIndexEntriesAreValid();
        }

        /// <summary>
        /// Common DeleteObject method that is used by both ObjectContext.DeleteObject and ObjectSet.DeleteObject.
        /// </summary>
        /// <param name="entity">Object to be deleted.</param>
        /// <param name="expectedEntitySet">
        /// EntitySet that the specified object is expected to be in. Null if the caller doesn't want to validate against a particular EntitySet.
        /// </param>
        internal void DeleteObject(object entity, EntitySet expectedEntitySet)
        {
            Debug.Assert(!(entity is IEntityWrapper), "Object is an IEntityWrapper instance instead of the raw entity.");
            EntityUtil.CheckArgumentNull(entity, "entity");

            EntityEntry cacheEntry = this.ObjectStateManager.FindEntityEntry(entity);
            if (cacheEntry == null || !object.ReferenceEquals(cacheEntry.Entity, entity))
            {
                throw EntityUtil.CannotDeleteEntityNotInObjectStateManager();
            }

            if (expectedEntitySet != null)
            {
                EntitySetBase actualEntitySet = cacheEntry.EntitySet;
                if (actualEntitySet != expectedEntitySet)
                {
                    throw EntityUtil.EntityNotInObjectSet_Delete(actualEntitySet.EntityContainer.Name, actualEntitySet.Name, expectedEntitySet.EntityContainer.Name, expectedEntitySet.Name);
                }
            }

            cacheEntry.Delete();
            // Detaching from the context happens when the object
            // actually detaches from the cache (not just when it is
            // marked for deletion).
        }

        /// <summary>
        /// Detach entity from the cache.
        /// </summary>
        /// <param name="entity">Object to be detached.</param>
        public void Detach(object entity)
        {
            ObjectStateManager.AssertAllForeignKeyIndexEntriesAreValid();
            // This method and ObjectSet.DetachObject are expected to have identical behavior except for the extra validation ObjectSet
            // requests by passing a non-null expectedEntitySetName. Any changes to this method are expected to be made in the common
            // internal overload below that ObjectSet also uses, unless there is a specific reason why a behavior is desired when the
            // call comes from ObjectContext only.
            Detach(entity, null /*expectedEntitySet*/);
            ObjectStateManager.AssertAllForeignKeyIndexEntriesAreValid();
        }

        /// <summary>
        /// Common Detach method that is used by both ObjectContext.Detach and ObjectSet.Detach.
        /// </summary>
        /// <param name="entity">Object to be detached.</param>
        /// <param name="expectedEntitySet">
        /// EntitySet that the specified object is expected to be in. Null if the caller doesn't want to validate against a particular EntitySet.
        /// </param>        
        internal void Detach(object entity, EntitySet expectedEntitySet)
        {
            Debug.Assert(!(entity is IEntityWrapper), "Object is an IEntityWrapper instance instead of the raw entity.");
            EntityUtil.CheckArgumentNull(entity, "entity");

            EntityEntry cacheEntry = this.ObjectStateManager.FindEntityEntry(entity);
            if (cacheEntry == null ||
                !object.ReferenceEquals(cacheEntry.Entity, entity) ||
                cacheEntry.Entity == null) // this condition includes key entries and relationship entries
            {
                throw EntityUtil.CannotDetachEntityNotInObjectStateManager();
            }

            if (expectedEntitySet != null)
            {
                EntitySetBase actualEntitySet = cacheEntry.EntitySet;
                if (actualEntitySet != expectedEntitySet)
                {
                    throw EntityUtil.EntityNotInObjectSet_Detach(actualEntitySet.EntityContainer.Name, actualEntitySet.Name, expectedEntitySet.EntityContainer.Name, expectedEntitySet.Name);
                }
            }

            cacheEntry.Detach();
        }

        /// <summary>
        /// Disposes this ObjectContext.
        /// </summary>
        public void Dispose()
        {
            // Technically, calling GC.SuppressFinalize is not required because the class does not
            // have a finalizer, but it does no harm, protects against the case where a finalizer is added
            // in the future, and prevents an FxCop warning.
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        /// <summary>
        /// Disposes this ObjectContext.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Release managed resources here.

                if (_connection != null)
                {
                    _connection.StateChange -= ConnectionStateChange;

                    // Dispose the connection the ObjectContext created
                    if (_createdConnection)
                    {
                        _connection.Dispose();
                    }
                }
                _connection = null; // Marks this object as disposed.
                _adapter = null;
                if (_cache != null)
                {
                    _cache.Dispose();
                }
            }
            // Release unmanaged resources here (none for this class).
        }

        #region GetEntitySet

        /// <summary>
        /// Returns the EntitySet with the given name from given container.
        /// </summary>
        /// <param name="entitySetName">name of entity set</param>
        /// <param name="entityContainerName">name of container</param>
        /// <returns>the appropriate EntitySet</returns>
        /// <exception cref="InvalidOperationException">the entity set could not be found for the given name</exception>
        /// <exception cref="InvalidOperationException">the entity container could not be found for the given name</exception>
        internal EntitySet GetEntitySet(string entitySetName, string entityContainerName)
        {
            Debug.Assert(entitySetName != null, "entitySetName should be not null");

            EntityContainer container = null;

            if (String.IsNullOrEmpty(entityContainerName))
            {
                container = this.Perspective.GetDefaultContainer();
                Debug.Assert(container != null, "Problem with metadata - default container not found");
            }
            else
            {
                if (!this.MetadataWorkspace.TryGetEntityContainer(entityContainerName, DataSpace.CSpace, out container))
                {
                    throw EntityUtil.EntityContainterNotFoundForName(entityContainerName);
                }
            }

            EntitySet entitySet = null;

            if (!container.TryGetEntitySetByName(entitySetName, false, out entitySet))
            {
                throw EntityUtil.EntitySetNotFoundForName(TypeHelpers.GetFullName(container.Name, entitySetName));
            }

            return entitySet;
        }

        private static void GetEntitySetName(string qualifiedName, string parameterName, ObjectContext context, out  string entityset, out string container)
        {
            entityset = null;
            container = null;
            EntityUtil.CheckStringArgument(qualifiedName, parameterName);

            string[] result = qualifiedName.Split('.');
            if (result.Length > 2)
            {
                throw EntityUtil.QualfiedEntitySetName(parameterName);
            }
            if (result.Length == 1) // if not '.' at all
            {
                entityset = result[0];
            }
            else
            {
                container = result[0];
                entityset = result[1];
                if (container == null || container.Length == 0) // if it starts with '.'
                {
                    throw EntityUtil.QualfiedEntitySetName(parameterName);
                }
            }
            if (entityset == null || entityset.Length == 0) // if it's not in the form "ES name . containername"
            {
                throw EntityUtil.QualfiedEntitySetName(parameterName);
            }

            if (context != null && String.IsNullOrEmpty(container) && (context.Perspective.GetDefaultContainer() == null))
            {
                throw EntityUtil.ContainerQualifiedEntitySetNameRequired(parameterName);
            }
        }

        /// <summary>
        /// Validate that an EntitySet is compatible with a given entity instance's CLR type.
        /// </summary>
        /// <param name="entitySet">an EntitySet</param>
        /// <param name="entityType">The CLR type of an entity instance</param>
        private void ValidateEntitySet(EntitySet entitySet, Type entityType)
        {
            TypeUsage entityTypeUsage = GetTypeUsage(entityType);
            if (!entitySet.ElementType.IsAssignableFrom(entityTypeUsage.EdmType))
            {
                throw EntityUtil.InvalidEntitySetOnEntity(entitySet.Name, entityType, "entity");
            }
        }

        internal TypeUsage GetTypeUsage(Type entityCLRType)
        {
            // Register the assembly so the type information will be sure to be loaded in metadata
            this.MetadataWorkspace.ImplicitLoadAssemblyForType(entityCLRType, System.Reflection.Assembly.GetCallingAssembly());

            TypeUsage entityTypeUsage = null;
            if (!this.Perspective.TryGetType(entityCLRType, out entityTypeUsage) ||
                !TypeSemantics.IsEntityType(entityTypeUsage))
            {
                throw EntityUtil.InvalidEntityType(entityCLRType);
            }
            Debug.Assert(entityTypeUsage != null, "entityTypeUsage is null");
            return entityTypeUsage;
        }
        #endregion

        /// <summary>
        /// Retrieves an object from the cache if present or from the
        /// store if not.
        /// </summary>
        /// <param name="key">Key of the object to be found.</param>
        /// <returns>Entity object.</returns>
        public object GetObjectByKey(EntityKey key)
        {
            EntityUtil.CheckArgumentNull(key, "key");

            EntitySet entitySet = key.GetEntitySet(this.MetadataWorkspace);
            Debug.Assert(entitySet != null, "Key's EntitySet should not be null in the MetadataWorkspace");

            // SQLBUDT 447285: Ensure the assembly containing the entity's CLR type is loaded into the workspace.
            // If the schema types are not loaded: metadata, cache & query would be unable to reason about the type.
            // Either the entity type's assembly is already in the ObjectItemCollection or we auto-load the user's calling assembly and its referenced assemblies.
            // *GetCallingAssembly returns the assembly of the method that invoked the currently executing method.
            MetadataWorkspace.ImplicitLoadFromEntityType(entitySet.ElementType, System.Reflection.Assembly.GetCallingAssembly());

            object entity;
            if (!TryGetObjectByKey(key, out entity))
            {
                throw EntityUtil.ObjectNotFound();
            }
            return entity;
        }

        #region Refresh
        /// <summary>
        /// Refreshing cache data with store data for specific entities.
        /// The order in which entites are refreshed is non-deterministic.
        /// </summary>
        /// <param name="refreshMode">Determines how the entity retrieved from the store is merged with the entity in the cache</param>
        /// <param name="collection">must not be null and all entities must be attached to this context. May be empty.</param>
        /// <exception cref="ArgumentOutOfRangeException">if refreshMode is not valid</exception>
        /// <exception cref="ArgumentNullException">collection is null</exception>
        /// <exception cref="ArgumentException">collection contains null or non entities or entities not attached to this context</exception>
        public void Refresh(RefreshMode refreshMode, IEnumerable collection)
        {
            ObjectStateManager.AssertAllForeignKeyIndexEntriesAreValid();
            try
            {
                EntityUtil.CheckArgumentRefreshMode(refreshMode);
                EntityUtil.CheckArgumentNull(collection, "collection");

                // collection may not contain any entities -- this is valid for this overload
                RefreshEntities(refreshMode, collection);
            }
            finally
            {
                ObjectStateManager.AssertAllForeignKeyIndexEntriesAreValid();
            }
        }
        /// <summary>
        /// Refreshing cache data with store data for a specific entity.
        /// </summary>
        /// <param name="refreshMode">Determines how the entity retrieved from the store is merged with the entity in the cache</param>
        /// <param name="entity">The entity to refresh. This must be a non-null entity that is attached to this context</param>
        /// <exception cref="ArgumentOutOfRangeException">if refreshMode is not valid</exception>
        /// <exception cref="ArgumentNullException">entity is null</exception>
        /// <exception cref="ArgumentException">entity is not attached to this context</exception>
        public void Refresh(RefreshMode refreshMode, object entity)
        {
            Debug.Assert(!(entity is IEntityWrapper), "Object is an IEntityWrapper instance instead of the raw entity.");
            ObjectStateManager.AssertAllForeignKeyIndexEntriesAreValid();
            try
            {
                EntityUtil.CheckArgumentRefreshMode(refreshMode);
                EntityUtil.CheckArgumentNull(entity, "entity");

                RefreshEntities(refreshMode, new object[] { entity });
            }
            finally
            {
                ObjectStateManager.AssertAllForeignKeyIndexEntriesAreValid();
            }
        }

        /// <summary>
        /// Validates that the given entity/key pair has an ObjectStateEntry
        /// and that entry is not in the added state.
        /// 
        /// The entity is added to the entities dictionary, and checked for duplicates.
        /// </summary>
        /// <param name="entities">on exit, entity is added to this dictionary.</param>
        /// <param name="entity">An object reference that is not "Added," has an ObjectStateEntry and is not in the entities list.</param>
        /// <param name="key"></param>
        private void RefreshCheck(
            Dictionary<EntityKey, EntityEntry> entities,
            object entity, EntityKey key)
        {
            Debug.Assert(!(entity is IEntityWrapper), "Object is an IEntityWrapper instance instead of the raw entity.");
            Debug.Assert(entity != null, "The entity is null.");

            EntityEntry entry = ObjectStateManager.FindEntityEntry(key);
            if (null == entry)
            {
                throw EntityUtil.NthElementNotInObjectStateManager(entities.Count);
            }
            if (EntityState.Added == entry.State)
            {
                throw EntityUtil.NthElementInAddedState(entities.Count);
            }
            Debug.Assert(EntityState.Added != entry.State, "not expecting added");
            Debug.Assert(EntityState.Detached != entry.State, "not expecting detached");

            try
            {
                entities.Add(key, entry); // don't ignore duplicates
            }
            catch (ArgumentException)
            {
                throw EntityUtil.NthElementIsDuplicate(entities.Count);
            }

            Debug.Assert(null != entity, "null entity");
            Debug.Assert(null != (object)key, "null entity.Key");
            Debug.Assert(null != key.EntitySetName, "null entity.Key.EntitySetName");
        }

        private void RefreshEntities(RefreshMode refreshMode, IEnumerable collection)
        {
            // refreshMode and collection should already be validated prior to this call -- collection can be empty in one Refresh overload
            // but not in the other, so we need to do that check before we get to this common method
            Debug.Assert(collection != null, "collection may not contain any entities but should never be null");

            bool openedConnection = false;

            try
            {
                Dictionary<EntityKey, EntityEntry> entities = new Dictionary<EntityKey, EntityEntry>(RefreshEntitiesSize(collection));

                #region 1) Validate and bucket the entities by entity set
                Dictionary<EntitySet, List<EntityKey>> refreshKeys = new Dictionary<EntitySet, List<EntityKey>>();
                foreach (object entity in collection) // anything other than object risks InvalidCastException
                {
                    AddRefreshKey(entity, entities, refreshKeys);
                }

                // The collection is no longer required at this point.
                collection = null;
                #endregion

                #region 2) build and execute the query for each set of entities
                if (refreshKeys.Count > 0)
                {
                    EnsureConnection();
                    openedConnection = true;

                    // All entities from a single set can potentially be refreshed in the same query.
                    // However, the refresh operations are batched in an attempt to avoid the generation
                    // of query trees or provider SQL that exhaust available client or server resources.
                    foreach (EntitySet targetSet in refreshKeys.Keys)
                    {
                        List<EntityKey> setKeys = refreshKeys[targetSet];
                        int refreshedCount = 0;
                        while (refreshedCount < setKeys.Count)
                        {
                            refreshedCount = BatchRefreshEntitiesByKey(refreshMode, entities, targetSet, setKeys, refreshedCount);
                        }
                    }
                }

                // The refreshKeys list is no longer required at this point.
                refreshKeys = null;
                #endregion

                #region 3) process the unrefreshed entities
                if (RefreshMode.StoreWins == refreshMode)
                {
                    // remove all entites that have been removed from the store, not added by client
                    foreach (KeyValuePair<EntityKey, EntityEntry> item in entities)
                    {
                        Debug.Assert(EntityState.Added != item.Value.State, "should not be possible");
                        if (EntityState.Detached != item.Value.State)
                        {
                            // We set the detaching flag here even though we are deleting because we are doing a
                            // Delete/AcceptChanges cycle to simulate a Detach, but we can't use Detach directly
                            // because legacy behavior around cascade deletes should be preserved.  However, we
                            // do want to prevent FK values in dependents from being nulled, which is why we
                            // need to set the detaching flag.
                            ObjectStateManager.TransactionManager.BeginDetaching();
                            try
                            {
                                item.Value.Delete();
                            }
                            finally
                            {
                                ObjectStateManager.TransactionManager.EndDetaching();
                            }
                            Debug.Assert(EntityState.Detached != item.Value.State, "not expecting detached");

                            item.Value.AcceptChanges();
                        }
                    }
                }
                else if ((RefreshMode.ClientWins == refreshMode) && (0 < entities.Count))
                {
                    // throw an exception with all appropriate entity keys in text
                    string prefix = String.Empty;
                    StringBuilder builder = new StringBuilder();
                    foreach (KeyValuePair<EntityKey, EntityEntry> item in entities)
                    {
                        Debug.Assert(EntityState.Added != item.Value.State, "should not be possible");
                        if (item.Value.State == EntityState.Deleted)
                        {
                            // Detach the deleted items because this is the client changes and the server
                            // does not have these items any more
                            item.Value.AcceptChanges();
                        }
                        else
                        {
                            builder.Append(prefix).Append(Environment.NewLine);
                            builder.Append('\'').Append(item.Key.ConcatKeyValue()).Append('\'');
                            prefix = ",";
                        }
                    }
                    // If there were items that could not be found, throw an exception
                    if (builder.Length > 0)
                    {
                        throw EntityUtil.ClientEntityRemovedFromStore(builder.ToString());
                    }
                }
                #endregion
            }
            finally
            {
                if (openedConnection)
                {
                    ReleaseConnection();
                }
            }
        }

        private int BatchRefreshEntitiesByKey(RefreshMode refreshMode, Dictionary<EntityKey, EntityEntry> trackedEntities, EntitySet targetSet, List<EntityKey> targetKeys, int startFrom)
        {
            //
            // A single refresh query can be built for all entities from the same set.
            // For each entity set, a DbFilterExpression is constructed that
            // expresses the equivalent of:
            //
            // SELECT VALUE e
            // FROM <entityset> AS e
            // WHERE
            // GetRefKey(GetEntityRef(e)) == <ref1>.KeyValues
            // [OR GetRefKey(GetEntityRef(e)) == <ref2>.KeyValues
            // [..OR GetRefKey(GetEntityRef(e)) == <refN>.KeyValues]]
            //
            // Note that a LambdaFunctionExpression is used so that instead
            // of repeating GetRefKey(GetEntityRef(e)) a VariableReferenceExpression
            // to a Lambda argument with the value GetRefKey(GetEntityRef(e)) is used instead.
            // The query is therefore logically equivalent to:
            //
            // SELECT VALUE e
            // FROM <entityset> AS e
            // WHERE
            //   LET(x = GetRefKey(GetEntityRef(e)) IN (
            //      x == <ref1>.KeyValues
            //     [OR x == <ref2>.KeyValues
            //     [..OR x == <refN>.KeyValues]]
            //   )
            //

            // The batch size determines the maximum depth of the predicate OR tree and
            // also limits the size of the generated provider SQL that is sent to the server.
            const int maxBatch = 250;

            // Bind the target EntitySet under the name "EntitySet".
            DbExpressionBinding entitySetBinding = targetSet.Scan().BindAs("EntitySet");

            // Use the variable from the set binding as the 'e' in a new GetRefKey(GetEntityRef(e)) expression.
            DbExpression sourceEntityKey = entitySetBinding.Variable.GetEntityRef().GetRefKey();

            // Build the where predicate as described above. A maximum of <batchsize> entity keys will be included
            // in the predicate, starting from position <startFrom> in the list of entity keys. As each key is
            // included, both <batchsize> and <startFrom> are incremented to ensure that the batch size is
            // correctly constrained and that the new starting position for the next call to this method is calculated.
            int batchSize = Math.Min(maxBatch, (targetKeys.Count - startFrom));
            DbExpression[] keyFilters = new DbExpression[batchSize];
            for (int idx = 0; idx < batchSize; idx++)
            {
                // Create a row constructor expression based on the key values of the EntityKey.
                KeyValuePair<string, DbExpression>[] keyValueColumns = targetKeys[startFrom++].GetKeyValueExpressions(targetSet);
                DbExpression keyFilter = DbExpressionBuilder.NewRow(keyValueColumns);

                // Create an equality comparison between the row constructor and the lambda variable
                // that refers to GetRefKey(GetEntityRef(e)), which also produces a row
                // containing key values, but for the current entity from the entity set.
                keyFilters[idx] = sourceEntityKey.Equal(keyFilter);
            }

            // Sanity check that the batch includes at least one element.
            Debug.Assert(batchSize > 0, "Didn't create a refresh expression?");

            // Build a balanced binary tree that OR's the key filters together.
            DbExpression entitySetFilter = Helpers.BuildBalancedTreeInPlace(keyFilters, DbExpressionBuilder.Or);

            // Create a FilterExpression based on the EntitySet binding and the Lambda predicate.
            // This FilterExpression encapsulated the logic required for the refresh query as described above.
            DbExpression refreshQuery = entitySetBinding.Filter(entitySetFilter);

            // Initialize the command tree used to issue the refresh query.
            DbQueryCommandTree tree = DbQueryCommandTree.FromValidExpression(this.MetadataWorkspace, DataSpace.CSpace, refreshQuery);

            // Evaluate the refresh query using ObjectQuery<T> and process the results to update the ObjectStateManager.
            MergeOption mergeOption = (RefreshMode.StoreWins == refreshMode ?
                                       MergeOption.OverwriteChanges :
                                       MergeOption.PreserveChanges);

            // The connection will be released by ObjectResult when enumeration is complete.
            this.EnsureConnection();

            try
            {
                ObjectResult<object> results = ObjectQueryExecutionPlan.ExecuteCommandTree<object>(this, tree, mergeOption);

                foreach (object entity in results)
                {
                    // There is a risk that, during an event, the Entity removed itself from the cache.
                    EntityEntry entry = ObjectStateManager.FindEntityEntry(entity);
                    if ((null != entry) && (EntityState.Modified == entry.State))
                    {   // this is 'ForceChanges' - which is the same as PreserveChanges, except all properties are marked modified.
                        Debug.Assert(RefreshMode.ClientWins == refreshMode, "StoreWins always becomes unchanged");
                        entry.SetModifiedAll();
                    }

                    IEntityWrapper wrappedEntity = EntityWrapperFactory.WrapEntityUsingContext(entity, this);
                    EntityKey key = wrappedEntity.EntityKey;
                    EntityUtil.CheckEntityKeyNull(key);

                    // Dev10#673631 - An incorrectly returned entity should result in an exception to avoid further corruption to the OSM.
                    if (!trackedEntities.Remove(key))
                    {
                        throw EntityUtil.StoreEntityNotPresentInClient();
                    }
                }
            }
            catch
            {
                // Enumeration did not complete, so the connection must be explicitly released.
                this.ReleaseConnection();
                throw;
            }

            // Return the position in the list from which the next refresh operation should start.
            // This will be equal to the list count if all remaining entities in the list were
            // refreshed during this call.
            return startFrom;
        }

        private static int RefreshEntitiesSize(IEnumerable collection)
        {
            ICollection list = collection as ICollection;
            return ((null != list) ? list.Count : 0);
        }
        #endregion

        #region SaveChanges
        /// <summary>
        /// Persists all updates to the store.
        /// </summary>
        /// <returns>
        ///   the number of dirty (i.e., Added, Modified, or Deleted) ObjectStateEntries
        ///   in the ObjectStateManager when SaveChanges was called.
        /// </returns>
        public Int32 SaveChanges()
        {
            return SaveChanges(SaveOptions.DetectChangesBeforeSave | SaveOptions.AcceptAllChangesAfterSave);
        }


        /// <summary>
        ///   Persists all updates to the store.
        ///   This API is obsolete.  Please use SaveChanges(SaveOptions options) instead.
        ///   SaveChanges(true) is equivalent to SaveChanges() -- That is it detects changes and
        ///      accepts all changes after save.
        ///   SaveChanges(false) detects changes but does not accept changes after save.
        /// </summary>
        /// <param name="acceptChangesDuringSave">if false, user must call AcceptAllChanges</param>/>
        /// <returns>
        ///   the number of dirty (i.e., Added, Modified, or Deleted) ObjectStateEntries
        ///   in the ObjectStateManager when SaveChanges was called.
        /// </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Browsable(false)]
        [Obsolete("Use SaveChanges(SaveOptions options) instead.")]
        public Int32 SaveChanges(bool acceptChangesDuringSave)
        {
            return this.SaveChanges(acceptChangesDuringSave ? SaveOptions.DetectChangesBeforeSave | SaveOptions.AcceptAllChangesAfterSave
                                                            : SaveOptions.DetectChangesBeforeSave);
        }

        /// <summary>
        /// Persists all updates to the store.
        /// </summary>
        /// <param name="options">describes behavior options of SaveChanges</param>
        /// <returns>
        ///   the number of dirty (i.e., Added, Modified, or Deleted) ObjectStateEntries
        ///   in the ObjectStateManager processed by SaveChanges.
        /// </returns>
        public virtual Int32 SaveChanges(SaveOptions options)
        {
            ObjectStateManager.AssertAllForeignKeyIndexEntriesAreValid();

            OnSavingChanges();

            if ((SaveOptions.DetectChangesBeforeSave & options) != 0)
            {
                this.ObjectStateManager.DetectChanges();
            }

            if (ObjectStateManager.SomeEntryWithConceptualNullExists())
            {
                throw new InvalidOperationException(Strings.ObjectContext_CommitWithConceptualNull);
            }

            bool mustReleaseConnection = false;
            Int32 entriesAffected = ObjectStateManager.GetObjectStateEntriesCount(EntityState.Added | EntityState.Deleted | EntityState.Modified);
            EntityConnection connection = (EntityConnection)Connection;

            if (0 < entriesAffected)
            {   // else fast exit if no changes to save to avoids interacting with or starting of new transactions

                // get data adapter
                if (_adapter == null)
                {
                    IServiceProvider sp = DbProviderFactories.GetFactory(connection) as IServiceProvider;
                    if (sp != null)
                    {
                        _adapter = sp.GetService(typeof(IEntityAdapter)) as IEntityAdapter;
                    }
                    if (_adapter == null)
                    {
                        throw EntityUtil.InvalidDataAdapter();
                    }
                }
                // only accept changes after the local transaction commits
                _adapter.AcceptChangesDuringUpdate = false;
                _adapter.Connection = connection;
                _adapter.CommandTimeout = this.CommandTimeout;

                try
                {
                    EnsureConnection();
                    mustReleaseConnection = true;

                    // determine what transaction to enlist in
                    bool needLocalTransaction = false;

                    if (null == connection.CurrentTransaction && !connection.EnlistedInUserTransaction)
                    {
                        // If there isn't a local transaction started by the user, we'll attempt to enlist 
                        // on the current SysTx transaction so we don't need to construct a local
                        // transaction.

                        needLocalTransaction = (null == _lastTransaction);
                    }
                    // else the user already has his own local transaction going; user will do the abort or commit.

                    DbTransaction localTransaction = null;
                    try
                    {
                        // EntityConnection tracks the CurrentTransaction we don't need to pass it around
                        if (needLocalTransaction)
                        {
                            localTransaction = connection.BeginTransaction();
                        }
                        entriesAffected = _adapter.Update(ObjectStateManager);

                        if (null != localTransaction)
                        {   // we started the local transaction; so we also commit it
                            localTransaction.Commit();
                        }
                        // else on success with no exception is thrown, user generally commits the transaction
                    }
                    finally
                    {
                        if (null != localTransaction)
                        {   // we started the local transaction; so it requires disposal (rollback if not previously committed
                            localTransaction.Dispose();
                        }
                        // else on failure with an exception being thrown, user generally aborts (default action with transaction without an explict commit)
                    }
                }
                finally
                {
                    if (mustReleaseConnection)
                    {
                        // Release the connection when we are done with the save
                        ReleaseConnection();
                    }
                }

                if ((SaveOptions.AcceptAllChangesAfterSave & options) != 0)
                {   // only accept changes after the local transaction commits

                    try
                    {
                        AcceptAllChanges();
                    }
                    catch (Exception e)
                    {
                        // If AcceptAllChanges throw - let's inform user that changes in database were committed 
                        // and that Context and Database can be in inconsistent state.

                        // We should not be wrapping all exceptions
                        if (EntityUtil.IsCatchableExceptionType(e))
                        {
                            throw EntityUtil.AcceptAllChangesFailure(e);
                        }
                        throw;
                    }
                }
            }
            ObjectStateManager.AssertAllForeignKeyIndexEntriesAreValid();
            return entriesAffected;
        }
        #endregion

        /// <summary>
        /// For every tracked entity which doesn't implement IEntityWithChangeTracker detect changes in the entity's property values
        /// and marks appropriate ObjectStateEntry as Modified.
        /// For every tracked entity which doesn't implement IEntityWithRelationships detect changes in its relationships.
        /// 
        /// The method is used inter----ly by ObjectContext.SaveChanges() but can be also used if user wants to detect changes 
        /// and have ObjectStateEntries in appropriate state before the SaveChanges() method is called.
        /// </summary>
        public void DetectChanges()
        {
            ObjectStateManager.AssertAllForeignKeyIndexEntriesAreValid();
            this.ObjectStateManager.DetectChanges();
            ObjectStateManager.AssertAllForeignKeyIndexEntriesAreValid();
        }

        /// <summary>
        /// Attempts to retrieve an object from the cache or the store.
        /// </summary>
        /// <param name="key">Key of the object to be found.</param>
        /// <param name="value">Out param for the object.</param>
        /// <returns>True if the object was found, false otherwise.</returns>
        public bool TryGetObjectByKey(EntityKey key, out object value)
        {
            // try the cache first
            EntityEntry entry;
            ObjectStateManager.TryGetEntityEntry(key, out entry); // this will check key argument
            if (entry != null)
            {
                // can't find keys
                if (!entry.IsKeyEntry)
                {
                    // SQLBUDT 511296 returning deleted object.
                    value = entry.Entity;
                    return value != null;
                }
            }

            if (key.IsTemporary)
            {
                // If the key is temporary, we cannot find a corresponding object in the store.
                value = null;
                return false;
            }

            EntitySet entitySet = key.GetEntitySet(this.MetadataWorkspace);
            Debug.Assert(entitySet != null, "Key's EntitySet should not be null in the MetadataWorkspace");

            // Validate the EntityKey values against the EntitySet
            key.ValidateEntityKey(_workspace, entitySet, true /*isArgumentException*/, "key");

            // SQLBUDT 447285: Ensure the assembly containing the entity's CLR type is loaded into the workspace.
            // If the schema types are not loaded: metadata, cache & query would be unable to reason about the type.
            // Either the entity type's assembly is already in the ObjectItemCollection or we auto-load the user's calling assembly and its referenced assemblies.
            // *GetCallingAssembly returns the assembly of the method that invoked the currently executing method.
            MetadataWorkspace.ImplicitLoadFromEntityType(entitySet.ElementType, System.Reflection.Assembly.GetCallingAssembly());

            // Execute the query:
            // SELECT VALUE X FROM [EC].[ES] AS X
            // WHERE X.KeyProp0 = @p0 AND X.KeyProp1 = @p1 AND ... 
            // parameters are the key values 

            // Build the Entity SQL query
            StringBuilder esql = new StringBuilder();
            esql.AppendFormat("SELECT VALUE X FROM {0}.{1} AS X WHERE ", EntityUtil.QuoteIdentifier(entitySet.EntityContainer.Name), EntityUtil.QuoteIdentifier(entitySet.Name));
            EntityKeyMember[] members = key.EntityKeyValues;
            ReadOnlyMetadataCollection<EdmMember> keyMembers = entitySet.ElementType.KeyMembers;
            ObjectParameter[] parameters = new ObjectParameter[members.Length];

            for (int i = 0; i < members.Length; i++)
            {
                if (i > 0)
                {
                    esql.Append(" AND ");
                }
                string parameterName = string.Format(CultureInfo.InvariantCulture, "p{0}", i.ToString(CultureInfo.InvariantCulture));
                esql.AppendFormat("X.{0} = @{1}", EntityUtil.QuoteIdentifier(members[i].Key), parameterName);
                parameters[i] = new ObjectParameter(parameterName, members[i].Value);

                // Try to set the TypeUsage on the ObjectParameter
                EdmMember keyMember = null;
                if (keyMembers.TryGetValue(members[i].Key, true, out keyMember))
                {
                    parameters[i].TypeUsage = keyMember.TypeUsage;
                }
            }

            // Execute the query
            object entity = null;
            ObjectResult<object> results = CreateQuery<object>(esql.ToString(), parameters).Execute(MergeOption.AppendOnly);
            foreach (object queriedEntity in results)
            {
                Debug.Assert(entity == null, "Query for a key returned more than one entity!");
                entity = queriedEntity;
            }

            value = entity;
            return value != null;
        }


        /// <summary>
        /// Executes the given function on the default container. 
        /// </summary>
        /// <typeparam name="TElement">Element type for function results.</typeparam>
        /// <param name="functionName">Name of function. May include container (e.g. ContainerName.FunctionName)
        /// or just function name when DefaultContainerName is known.</param>
        /// <param name="parameters"></param>
        /// <exception cref="ArgumentException">If function is null or empty</exception>
        /// <exception cref="InvalidOperationException">If function is invalid (syntax,
        /// does not exist, refers to a function with return type incompatible with T)</exception>
        public ObjectResult<TElement> ExecuteFunction<TElement>(string functionName, params ObjectParameter[] parameters)
        {
            return ExecuteFunction<TElement>(functionName, MergeOption.AppendOnly, parameters);
        }

        /// <summary>
        /// Executes the given function on the default container. 
        /// </summary>
        /// <typeparam name="TElement">Element type for function results.</typeparam>
        /// <param name="functionName">Name of function. May include container (e.g. ContainerName.FunctionName)
        /// or just function name when DefaultContainerName is known.</param>
        /// <param name="mergeOption"></param>
        /// <param name="parameters"></param>
        /// <exception cref="ArgumentException">If function is null or empty</exception>
        /// <exception cref="InvalidOperationException">If function is invalid (syntax,
        /// does not exist, refers to a function with return type incompatible with T)</exception>
        public ObjectResult<TElement> ExecuteFunction<TElement>(string functionName, MergeOption mergeOption, params ObjectParameter[] parameters)
        {
            EntityUtil.CheckStringArgument(functionName, "function");
            EntityUtil.CheckArgumentNull(parameters, "parameters");

            EdmFunction functionImport;
            EntityCommand entityCommand = CreateEntityCommandForFunctionImport(functionName, out functionImport, parameters);
            int returnTypeCount = Math.Max(1, functionImport.ReturnParameters.Count);
            EdmType[] expectedEdmTypes = new EdmType[returnTypeCount];
            expectedEdmTypes[0] = MetadataHelper.GetAndCheckFunctionImportReturnType<TElement>(functionImport, 0, this.MetadataWorkspace);
            for (int i = 1; i < returnTypeCount; i++)
            {
                if (!MetadataHelper.TryGetFunctionImportReturnType<EdmType>(functionImport, i, out expectedEdmTypes[i]))
                {
                    throw EntityUtil.ExecuteFunctionCalledWithNonReaderFunction(functionImport);
                }
            }

            return CreateFunctionObjectResult<TElement>(entityCommand, functionImport.EntitySets, expectedEdmTypes, mergeOption);
        }

        /// <summary>
        /// Executes the given function on the default container and discard any results returned from the function.
        /// </summary>
        /// <param name="functionName">Name of function. May include container (e.g. ContainerName.FunctionName)
        /// or just function name when DefaultContainerName is known.</param>
        /// <param name="parameters"></param>
        /// <returns>Number of rows affected</returns>
        /// <exception cref="ArgumentException">If function is null or empty</exception>
        /// <exception cref="InvalidOperationException">If function is invalid (syntax,
        /// does not exist, refers to a function with return type incompatible with T)</exception>
        public int ExecuteFunction(string functionName, params ObjectParameter[] parameters)
        {
            EntityUtil.CheckStringArgument(functionName, "function");
            EntityUtil.CheckArgumentNull(parameters, "parameters");

            EdmFunction functionImport;
            EntityCommand entityCommand = CreateEntityCommandForFunctionImport(functionName, out functionImport, parameters);

            EnsureConnection();

            // Prepare the command before calling ExecuteNonQuery, so that exceptions thrown during preparation are not wrapped in CommandCompilationException
            entityCommand.Prepare();

            try
            {
                return entityCommand.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                if (EntityUtil.IsCatchableEntityExceptionType(e))
                {
                    throw EntityUtil.CommandExecution(System.Data.Entity.Strings.EntityClient_CommandExecutionFailed, e);
                }
                throw;
            }
            finally
            {
                this.ReleaseConnection();
            }
        }

        private EntityCommand CreateEntityCommandForFunctionImport(string functionName, out EdmFunction functionImport, params ObjectParameter[] parameters)
        {
            for (int i = 0; i < parameters.Length; i++)
            {
                ObjectParameter parameter = parameters[i];
                if (null == parameter)
                {
                    throw EntityUtil.InvalidOperation(System.Data.Entity.Strings.ObjectContext_ExecuteFunctionCalledWithNullParameter(i));
                }
            }

            string containerName;
            string functionImportName;

            functionImport =
                MetadataHelper.GetFunctionImport(
                functionName, this.DefaultContainerName, this.MetadataWorkspace,
                out containerName, out functionImportName);


            EntityConnection connection = (EntityConnection)this.Connection;

            // create query
            EntityCommand entityCommand = new EntityCommand();
            entityCommand.CommandType = CommandType.StoredProcedure;
            entityCommand.CommandText = containerName + "." + functionImportName;
            entityCommand.Connection = connection;
            if (this.CommandTimeout.HasValue)
            {
                entityCommand.CommandTimeout = this.CommandTimeout.Value;
            }

            PopulateFunctionImportEntityCommandParameters(parameters, functionImport, entityCommand);

            return entityCommand;
        }

        private ObjectResult<TElement> CreateFunctionObjectResult<TElement>(EntityCommand entityCommand, ReadOnlyMetadataCollection<EntitySet> entitySets, EdmType[] edmTypes, MergeOption mergeOption)
        {
            Debug.Assert(edmTypes != null && edmTypes.Length > 0);
            EnsureConnection();

            EntityCommandDefinition commandDefinition = entityCommand.GetCommandDefinition();

            // get store data reader
            DbDataReader storeReader;
            try
            {
                storeReader = commandDefinition.ExecuteStoreCommands(entityCommand, CommandBehavior.Default);
            }
            catch (Exception e)
            {
                this.ReleaseConnection();
                if (EntityUtil.IsCatchableEntityExceptionType(e))
                {
                    throw EntityUtil.CommandExecution(System.Data.Entity.Strings.EntityClient_CommandExecutionFailed, e);
                }
                throw;
            }

            return MaterializedDataRecord<TElement>(entityCommand, storeReader, 0, entitySets, edmTypes, mergeOption);
        }

        /// <summary>
        ///  Get the materializer for the resultSetIndexth result set of storeReader.
        /// </summary>
        internal ObjectResult<TElement> MaterializedDataRecord<TElement>(EntityCommand entityCommand, DbDataReader storeReader, int resultSetIndex, ReadOnlyMetadataCollection<EntitySet> entitySets, EdmType[] edmTypes, MergeOption mergeOption)
        {
            EntityCommandDefinition commandDefinition = entityCommand.GetCommandDefinition();
            try
            {
                // We want the shaper to close the reader if it is the last result set.
                bool shaperOwnsReader = edmTypes.Length <= resultSetIndex + 1;
                EdmType edmType = edmTypes[resultSetIndex];

                //Note: Defensive check for historic reasons, we expect entitySets.Count > resultSetIndex 
                EntitySet entitySet = entitySets.Count > resultSetIndex ? entitySets[resultSetIndex] : null;

                // create the shaper
                System.Data.Common.QueryCache.QueryCacheManager cacheManager = this.Perspective.MetadataWorkspace.GetQueryCacheManager();
                ShaperFactory<TElement> shaperFactory = Translator.TranslateColumnMap<TElement>(cacheManager, commandDefinition.CreateColumnMap(storeReader, resultSetIndex), this.MetadataWorkspace, null, mergeOption, false);
                Shaper<TElement> shaper = shaperFactory.Create(storeReader, this, this.MetadataWorkspace, mergeOption, shaperOwnsReader);

                NextResultGenerator nextResultGenerator;

                // We need to run notifications when the data reader is closed in order to propagate any out parameters.
                // We do this whenever the last (declared) result set's enumerator is disposed (this calls Finally on the shaper)
                // or when the underlying reader is closed as a result of the ObjectResult itself getting disposed.   
                // We use onReaderDisposeHasRun to ensure that this notification is only called once.   
                // the alternative approach of not making the final ObjectResult's disposal result do cleanup doesn't work in the case where
                // its GetEnumerator is called explicitly, and the resulting enumerator is never disposed.
                bool onReaderDisposeHasRun = false; 
                Action<object, EventArgs> onReaderDispose = (object sender, EventArgs e) =>
                    {
                        if (!onReaderDisposeHasRun)
                        {
                            onReaderDisposeHasRun = true;
                            // consume the store reader
                            CommandHelper.ConsumeReader(storeReader);
                            // trigger event callback
                            entityCommand.NotifyDataReaderClosing();
                        }
                    };

                if (shaperOwnsReader)
                {
                    shaper.OnDone += new EventHandler(onReaderDispose);
                    nextResultGenerator = null;
                }
                else
                {
                    nextResultGenerator = new NextResultGenerator(this, entityCommand, edmTypes, entitySets, mergeOption, resultSetIndex + 1);
                }

                // We want the ObjectResult to close the reader in its Dispose method, even if it is not the last result set.
                // This is to allow users to cancel reading results without the unnecessary iteration thru all the result sets.
                return new ObjectResult<TElement>(shaper, entitySet, TypeUsage.Create(edmTypes[resultSetIndex]), true, nextResultGenerator, onReaderDispose);
            }
            catch
            {
                this.ReleaseConnection();
                storeReader.Dispose();
                throw;
            }
        }


        private void PopulateFunctionImportEntityCommandParameters(ObjectParameter[] parameters, EdmFunction functionImport, EntityCommand command)
        {
            // attach entity parameters
            for (int i = 0; i < parameters.Length; i++)
            {
                ObjectParameter objectParameter = parameters[i];
                EntityParameter entityParameter = new EntityParameter();

                FunctionParameter functionParameter = FindParameterMetadata(functionImport, parameters, i);

                if (null != functionParameter)
                {
                    entityParameter.Direction = MetadataHelper.ParameterModeToParameterDirection(
                       functionParameter.Mode);
                    entityParameter.ParameterName = functionParameter.Name;
                }
                else
                {
                    entityParameter.ParameterName = objectParameter.Name;
                }

                entityParameter.Value = objectParameter.Value ?? DBNull.Value;

                if (DBNull.Value == entityParameter.Value ||
                    entityParameter.Direction != ParameterDirection.Input)
                {
                    TypeUsage typeUsage;
                    if (functionParameter != null)
                    {
                        // give precedence to the statically declared type usage
                        typeUsage = functionParameter.TypeUsage;
                    }
                    else if (null == objectParameter.TypeUsage)
                    {
                        Debug.Assert(objectParameter.MappableType != null, "MappableType must not be null");
                        Debug.Assert(Nullable.GetUnderlyingType(objectParameter.MappableType) == null, "Nullable types not expected here.");

                        // since ObjectParameters do not allow users to especify 'facets', make 
                        // sure that the parameter typeusage is not populated with the provider
                        // dafault facet values.
                        // Try getting the type from the workspace. This may fail however for one of the following reasons:
                        // - the type is not a model type
                        // - the types were not loaded into the workspace yet
                        // If the types were not loaded into the workspace we try loading types from the assembly the type lives in and re-try
                        // loading the type. We don't care if the type still cannot be loaded - in this case the result TypeUsage will be null
                        // which we handle later.
                        if (!this.Perspective.TryGetTypeByName(objectParameter.MappableType.FullName, /*ignoreCase */ false, out typeUsage))
                        {
                            this.MetadataWorkspace.ImplicitLoadAssemblyForType(objectParameter.MappableType, null);
                            this.Perspective.TryGetTypeByName(objectParameter.MappableType.FullName, /*ignoreCase */ false, out typeUsage);
                        }
                    }
                    else
                    {
                        typeUsage = objectParameter.TypeUsage;
                    }

                    // set type information (if the provider cannot determine it from the actual value)
                    EntityCommandDefinition.PopulateParameterFromTypeUsage(entityParameter, typeUsage, entityParameter.Direction != ParameterDirection.Input);
                }

                if (entityParameter.Direction != ParameterDirection.Input)
                {
                    ParameterBinder binder = new ParameterBinder(entityParameter, objectParameter);
                    command.OnDataReaderClosing += new EventHandler(binder.OnDataReaderClosingHandler);
                }
                command.Parameters.Add(entityParameter);
            }
        }

        private static FunctionParameter FindParameterMetadata(EdmFunction functionImport, ObjectParameter[] parameters, int ordinal)
        {
            // Retrieve parameter information from functionImport.
            // We first attempt to resolve by case-sensitive name. If there is no exact match,
            // check if there is a case-insensitive match. Case insensitive matches are only permitted
            // when a single parameter would match.
            FunctionParameter functionParameter;
            string parameterName = parameters[ordinal].Name;
            if (!functionImport.Parameters.TryGetValue(parameterName, false, out functionParameter))
            {
                // if only one parameter has this name, try a case-insensitive lookup
                int matchCount = 0;
                for (int i = 0; i < parameters.Length && matchCount < 2; i++)
                {
                    if (StringComparer.OrdinalIgnoreCase.Equals(parameters[i].Name, parameterName))
                    {
                        matchCount++;
                    }
                }
                if (matchCount == 1)
                {
                    functionImport.Parameters.TryGetValue(parameterName, true, out functionParameter);
                }
            }
            return functionParameter;
        }

        /// <summary>
        /// Attempt to generate a proxy type for each type in the supplied enumeration.
        /// </summary>
        /// <param name="types">
        /// Enumeration of Type objects that should correspond to O-Space types.
        /// </param>
        /// <remarks>
        /// Types in the enumeration that do not map to an O-Space type are ignored.
        /// Also, there is no guarantee that a proxy type will be created for a given type,
        /// only that if a proxy can be generated, then it will be generated.
        /// 
        /// See <see cref="EntityProxyFactory"/> class for more information about proxy type generation.
        /// </remarks>

        // Use one of the following methods to retrieve an enumeration of all CLR types mapped to O-Space EntityType objects:
        // 










        // 

        public void CreateProxyTypes(IEnumerable<Type> types)
        {
            ObjectItemCollection ospaceItems = (ObjectItemCollection)MetadataWorkspace.GetItemCollection(DataSpace.OSpace);

            // Ensure metadata is loaded for each type,
            // and attempt to create proxy type only for types that have a mapping to an O-Space EntityType.
            EntityProxyFactory.TryCreateProxyTypes(
                types.Select(type =>
                {
                    // Ensure the assembly containing the entity's CLR type is loaded into the workspace.
                    MetadataWorkspace.ImplicitLoadAssemblyForType(type, null);

                    EntityType entityType;
                    ospaceItems.TryGetItem<EntityType>(type.FullName, out entityType);
                    return entityType;
                }).Where(entityType => entityType != null)
            );
        }

        /// <summary>
        /// Return an enumerable of the current set of CLR proxy types.
        /// </summary>
        /// <returns>
        /// Enumerable of the current set of CLR proxy types.
        /// This will never be null.
        /// </returns>
        public static IEnumerable<Type> GetKnownProxyTypes()
        {
            return EntityProxyFactory.GetKnownProxyTypes();
        }

        /// <summary>
        /// Given a type that may represent a known proxy type, 
        /// return the corresponding type being proxied.
        /// </summary>
        /// <param name="type">Type that may represent a proxy type.</param>
        /// <returns>
        /// Non-proxy type that corresponds to the supplied proxy type,
        /// or the supplied type if it is not a known proxy type.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// If the value of the type parameter is null.
        /// </exception
        public static Type GetObjectType(Type type)
        {
            EntityUtil.CheckArgumentNull(type, "type");

            return EntityProxyFactory.IsProxyType(type) ? type.BaseType : type;
        }

        /// <summary>
        /// Create an appropriate instance of the type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">
        /// Type of object to be returned.
        /// </typeparam>
        /// <returns>
        /// An instance of an object of type <typeparamref name="T"/>.
        /// The object will either be an instance of the exact type <typeparamref name="T"/>,
        /// or possibly an instance of the proxy type that corresponds to <typeparamref name="T"/>.
        /// </returns>
        /// <remarks>
        /// The type <typeparamref name="T"/> must have an OSpace EntityType representation.
        /// </remarks>
        public T CreateObject<T>()
            where T : class
        {
            T instance = null;
            Type clrType = typeof(T);

            // Ensure the assembly containing the entity's CLR type is loaded into the workspace.
            MetadataWorkspace.ImplicitLoadAssemblyForType(clrType, null);

            // Retrieve the OSpace EntityType that corresponds to the supplied CLR type.
            // This call ensure that this mapping exists.
            ClrEntityType entityType = MetadataWorkspace.GetItem<ClrEntityType>(clrType.FullName, DataSpace.OSpace);
            EntityProxyTypeInfo proxyTypeInfo = null;

            if (ContextOptions.ProxyCreationEnabled && ((proxyTypeInfo = EntityProxyFactory.GetProxyType(entityType)) != null))
            {
                instance = (T)proxyTypeInfo.CreateProxyObject();

                // After creating the proxy we need to add additional state to the proxy such
                // that it is able to function correctly when returned.  In particular, it needs
                // an initialized set of RelatedEnd objects because it will not be possible to
                // create these for convention based mapping once the metadata in the context has
                // been lost.
                IEntityWrapper wrappedEntity = EntityWrapperFactory.CreateNewWrapper(instance, null);
                wrappedEntity.InitializingProxyRelatedEnds = true;
                try
                {
                    // We're setting the context temporarily here so that we can go through the process
                    // of creating RelatedEnds even with convention-based mapping.
                    // However, we also need to tell the wrapper that we're doing this so that we don't
                    // try to do things that we normally do when we have a context, such as adding the
                    // context to the RelatedEnds.  We can't do these things since they require an
                    // EntitySet, and, because of MEST, we don't have one.
                    wrappedEntity.AttachContext(this, null, MergeOption.NoTracking);
                    proxyTypeInfo.SetEntityWrapper(wrappedEntity);
                    if (proxyTypeInfo.InitializeEntityCollections != null)
                    {
                        proxyTypeInfo.InitializeEntityCollections.Invoke(null, new object[] { wrappedEntity });
                    }
                }
                finally
                {
                    wrappedEntity.InitializingProxyRelatedEnds = false;
                    wrappedEntity.DetachContext();
                }
            }
            else
            {
                Func<object> ctor = LightweightCodeGenerator.GetConstructorDelegateForType(entityType) as Func<object>;
                Debug.Assert(ctor != null, "Could not find entity constructor");
                instance = ctor() as T;
            }

            return instance;
        }

        /// <summary>
        /// Execute a command against the database server that does not return a sequence of objects.
        /// The command is specified using the server's native query language, such as SQL.
        /// </summary>
        /// <param name="command">The command specified in the server's native query language.</param>
        /// <param name="parameters">The parameter values to use for the query.</param>
        /// <returns>A single integer return value</returns>
        public int ExecuteStoreCommand(string commandText, params object[] parameters)
        {
            this.EnsureConnection();

            try
            {
                DbCommand command = CreateStoreCommand(commandText, parameters);
                return command.ExecuteNonQuery();
            }
            finally
            {
                this.ReleaseConnection();
            }
        }

        /// <summary>
        /// Execute the sequence returning query against the database server. 
        /// The query is specified using the server's native query language, such as SQL.
        /// </summary>
        /// <typeparam name="TElement">The element type of the result sequence.</typeparam>
        /// <param name="query">The query specified in the server's native query language.</param>
        /// <param name="parameters">The parameter values to use for the query.</param>
        /// <returns>An IEnumerable sequence of objects.</returns>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Microsoft: Generic parameters are required for strong-typing of the return type.")]
        public ObjectResult<TElement> ExecuteStoreQuery<TElement>(string commandText, params object[] parameters)
        {
            return ExecuteStoreQueryInternal<TElement>(commandText, null /*entitySetName*/, MergeOption.AppendOnly, parameters);
        }

        /// <summary>
        /// Execute the sequence returning query against the database server. 
        /// The query is specified using the server's native query language, such as SQL.
        /// </summary>
        /// <typeparam name="TEntity">The element type of the resulting sequence</typeparam>
        /// <param name="reader">The DbDataReader to translate</param>
        /// <param name="entitySetName">The entity set in which results should be tracked. Null indicates there is no entity set.</param>
        /// <param name="mergeOption">Merge option to use for entity results.</param>
        /// <returns>The translated sequence of objects</returns>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Microsoft: Generic parameters are required for strong-typing of the return type.")]
        public ObjectResult<TEntity> ExecuteStoreQuery<TEntity>(string commandText, string entitySetName, MergeOption mergeOption, params object[] parameters)
        {
            EntityUtil.CheckStringArgument(entitySetName, "entitySetName");
            return ExecuteStoreQueryInternal<TEntity>(commandText, entitySetName, mergeOption, parameters);
        }

        /// <summary>
        /// See ExecuteStoreQuery method.
        /// </summary>
        private ObjectResult<TElement> ExecuteStoreQueryInternal<TElement>(string commandText, string entitySetName, MergeOption mergeOption, params object[] parameters)
        {
            // SQLBUDT 447285: Ensure the assembly containing the entity's CLR type
            // is loaded into the workspace. If the schema types are not loaded
            // metadata, cache & query would be unable to reason about the type. We
            // either auto-load <TElement>'s assembly into the ObjectItemCollection or we
            // auto-load the user's calling assembly and its referenced assemblies.
            // If the entities in the user's result spans multiple assemblies, the
            // user must manually call LoadFromAssembly. *GetCallingAssembly returns
            // the assembly of the method that invoked the currently executing method.
            this.MetadataWorkspace.ImplicitLoadAssemblyForType(typeof(TElement), System.Reflection.Assembly.GetCallingAssembly());

            this.EnsureConnection();
            DbDataReader reader = null;

            try
            {
                DbCommand command = CreateStoreCommand(commandText, parameters);
                reader = command.ExecuteReader();
            }
            catch
            {
                // We only release the connection when there is an exception. Otherwise, the ObjectResult is
                // in charge of releasing it.
                this.ReleaseConnection();
                throw;
            }

            try
            {
                return InternalTranslate<TElement>(reader, entitySetName, mergeOption, true);
            }
            catch
            {
                reader.Dispose();
                this.ReleaseConnection();
                throw;
            }
        }

        /// <summary>
        /// Translates the data from a DbDataReader into sequence of objects.
        /// </summary>
        /// <typeparam name="TElement">The element type of the resulting sequence</typeparam>
        /// <param name="reader">The DbDataReader to translate</param>
        /// <param name="mergeOption">Merge option to use for entity results.</param>
        /// <returns>The translated sequence of objects</returns>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Microsoft: Generic parameters are required for strong-typing of the return type.")]
        public ObjectResult<TElement> Translate<TElement>(DbDataReader reader)
        {
            // SQLBUDT 447285: Ensure the assembly containing the entity's CLR type
            // is loaded into the workspace. If the schema types are not loaded
            // metadata, cache & query would be unable to reason about the type. We
            // either auto-load <TElement>'s assembly into the ObjectItemCollection or we
            // auto-load the user's calling assembly and its referenced assemblies.
            // If the entities in the user's result spans multiple assemblies, the
            // user must manually call LoadFromAssembly. *GetCallingAssembly returns
            // the assembly of the method that invoked the currently executing method.
            this.MetadataWorkspace.ImplicitLoadAssemblyForType(typeof(TElement), System.Reflection.Assembly.GetCallingAssembly());

            return InternalTranslate<TElement>(reader, null /*entitySetName*/, MergeOption.AppendOnly, false);
        }

        /// <summary>
        /// Translates the data from a DbDataReader into sequence of entities.
        /// </summary>
        /// <typeparam name="TEntity">The element type of the resulting sequence</typeparam>
        /// <param name="reader">The DbDataReader to translate</param>
        /// <param name="entitySetName">The entity set in which results should be tracked. Null indicates there is no entity set.</param>
        /// <param name="mergeOption">Merge option to use for entity results.</param>
        /// <returns>The translated sequence of objects</returns>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Microsoft: Generic parameters are required for strong-typing of the return type.")]
        public ObjectResult<TEntity> Translate<TEntity>(DbDataReader reader, string entitySetName, MergeOption mergeOption)
        {
            EntityUtil.CheckStringArgument(entitySetName, "entitySetName");

            // SQLBUDT 447285: Ensure the assembly containing the entity's CLR type
            // is loaded into the workspace. If the schema types are not loaded
            // metadata, cache & query would be unable to reason about the type. We
            // either auto-load <TEntity>'s assembly into the ObjectItemCollection or we
            // auto-load the user's calling assembly and its referenced assemblies.
            // If the entities in the user's result spans multiple assemblies, the
            // user must manually call LoadFromAssembly. *GetCallingAssembly returns
            // the assembly of the method that invoked the currently executing method.
            this.MetadataWorkspace.ImplicitLoadAssemblyForType(typeof(TEntity), System.Reflection.Assembly.GetCallingAssembly());

            return InternalTranslate<TEntity>(reader, entitySetName, mergeOption, false);
        }

        private ObjectResult<TElement> InternalTranslate<TElement>(DbDataReader reader, string entitySetName, MergeOption mergeOption, bool readerOwned)
        {
            EntityUtil.CheckArgumentNull(reader, "reader");
            EntityUtil.CheckArgumentMergeOption(mergeOption);
            EntitySet entitySet = null;
            if (!string.IsNullOrEmpty(entitySetName))
            {
                entitySet = this.GetEntitySetFromName(entitySetName);
            }

            // make sure all metadata is available (normally this is handled by the call to EntityConnection.Open,
            // but translate does not necessarily use the EntityConnection)
            EnsureMetadata();

            // get the expected EDM type
            EdmType modelEdmType;
            Type unwrappedTElement = Nullable.GetUnderlyingType(typeof(TElement)) ?? typeof(TElement);
            CollectionColumnMap columnMap;
            // for enums that are not in the model we use the enum underlying type
            if (MetadataHelper.TryDetermineCSpaceModelType<TElement>(this.MetadataWorkspace, out modelEdmType) ||
                (unwrappedTElement.IsEnum && MetadataHelper.TryDetermineCSpaceModelType(unwrappedTElement.GetEnumUnderlyingType(), this.MetadataWorkspace, out modelEdmType)))
            {
                if (entitySet != null && !entitySet.ElementType.IsAssignableFrom(modelEdmType))
                {
                    throw EntityUtil.InvalidOperation(Strings.ObjectContext_InvalidEntitySetForStoreQuery(entitySet.EntityContainer.Name,
                        entitySet.Name, typeof(TElement)));
                }

                columnMap = ColumnMapFactory.CreateColumnMapFromReaderAndType(reader, modelEdmType, entitySet, null);
            }
            else
            {
                columnMap = ColumnMapFactory.CreateColumnMapFromReaderAndClrType(reader, typeof(TElement), this.MetadataWorkspace);
            }

            // build a shaper for the column map to produce typed results
            System.Data.Common.QueryCache.QueryCacheManager cacheManager = this.MetadataWorkspace.GetQueryCacheManager();
            ShaperFactory<TElement> shaperFactory = Translator.TranslateColumnMap<TElement>(cacheManager, columnMap, this.MetadataWorkspace, null, mergeOption, false);
            Shaper<TElement> shaper = shaperFactory.Create(reader, this, this.MetadataWorkspace, mergeOption, readerOwned);
            return new ObjectResult<TElement>(shaper, entitySet, MetadataHelper.GetElementType(columnMap.Type), readerOwned);
        }

        private DbCommand CreateStoreCommand(string commandText, params object[] parameters)
        {
            DbCommand command = this._connection.StoreConnection.CreateCommand();
            command.CommandText = commandText;

            // get relevant state from the object context
            if (this.CommandTimeout.HasValue)
            {
                command.CommandTimeout = this.CommandTimeout.Value;
            }
            EntityTransaction entityTransaction = this._connection.CurrentTransaction;
            if (null != entityTransaction)
            {
                command.Transaction = entityTransaction.StoreTransaction;
            }

            if (null != parameters && parameters.Length > 0)
            {
                DbParameter[] dbParameters = new DbParameter[parameters.Length];

                // three cases: all explicit DbParameters, no explicit DbParameters
                // or a mix of the two (throw in the last case)
                if (parameters.All(p => p is DbParameter))
                {
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        dbParameters[i] = (DbParameter)parameters[i];
                    }
                }
                else if (!parameters.Any(p => p is DbParameter))
                {
                    string[] parameterNames = new string[parameters.Length];
                    string[] parameterSql = new string[parameters.Length];
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        parameterNames[i] = string.Format(CultureInfo.InvariantCulture, "p{0}", i);
                        dbParameters[i] = command.CreateParameter();
                        dbParameters[i].ParameterName = parameterNames[i];
                        dbParameters[i].Value = parameters[i] ?? DBNull.Value;

                        // By default, we attempt to swap in a SQL Server friendly representation of the parameter.
                        // For other providers, users may write:
                        //
                        //      ExecuteStoreQuery("select * from foo f where f.X = ?", 1);
                        //
                        // rather than:
                        //
                        //      ExecuteStoreQuery("select * from foo f where f.X = {0}", 1);
                        parameterSql[i] = "@" + parameterNames[i];
                    }
                    command.CommandText = string.Format(CultureInfo.InvariantCulture, command.CommandText, parameterSql);
                }
                else
                {
                    throw EntityUtil.InvalidOperation(System.Data.Entity.Strings.ObjectContext_ExecuteCommandWithMixOfDbParameterAndValues);
                }

                command.Parameters.AddRange(dbParameters);
            }

            return command;
        }

        /// <summary>
        /// Creates the database using the current store connection and the metadata in the StoreItemCollection. Most of the actual work
        /// is done by the DbProviderServices implementation for the current store connection.
        /// </summary>
        public void CreateDatabase()
        {
            DbConnection storeConnection = this._connection.StoreConnection;
            DbProviderServices services = DbProviderServices.GetProviderServices(this.GetStoreItemCollection().StoreProviderFactory);
            services.CreateDatabase(storeConnection, this.CommandTimeout, this.GetStoreItemCollection());
        }

        /// <summary>
        /// Deletes the database that is specified as the database in the current store connection. Most of the actual work
        /// is done by the DbProviderServices implementation for the current store connection.
        /// </summary>
        public void DeleteDatabase()
        {
            DbConnection storeConnection = this._connection.StoreConnection;
            DbProviderServices services = DbProviderServices.GetProviderServices(this.GetStoreItemCollection().StoreProviderFactory);
            services.DeleteDatabase(storeConnection, this.CommandTimeout, this.GetStoreItemCollection());
        }

        /// <summary>
        /// Checks if the database that is specified as the database in the current store connection exists on the store. Most of the actual work
        /// is done by the DbProviderServices implementation for the current store connection.
        /// </summary>
        public bool DatabaseExists()
        {
            DbConnection storeConnection = this._connection.StoreConnection;
            DbProviderServices services = DbProviderServices.GetProviderServices(this.GetStoreItemCollection().StoreProviderFactory);
            return services.DatabaseExists(storeConnection, this.CommandTimeout, this.GetStoreItemCollection());
        }

        /// <summary>
        /// Creates the sql script that can be used to create the database for the metadata in the StoreItemCollection. Most of the actual work
        /// is done by the DbProviderServices implementation for the current store connection.
        /// </summary>
        public String CreateDatabaseScript()
        {
            DbProviderServices services = DbProviderServices.GetProviderServices(this.GetStoreItemCollection().StoreProviderFactory);
            string targetProviderManifestToken = this.GetStoreItemCollection().StoreProviderManifestToken;
            return services.CreateDatabaseScript(targetProviderManifestToken, this.GetStoreItemCollection());
        }

        private StoreItemCollection GetStoreItemCollection()
        {
            var entityConnection = (EntityConnection)this.Connection;
            // retrieve the item collection from the entity connection rather than the context since:
            // a) it forces creation of the metadata workspace if it's not already there
            // b) the store item collection isn't guaranteed to exist on the context.MetadataWorkspace
            return (StoreItemCollection)entityConnection.GetMetadataWorkspace().GetItemCollection(DataSpace.SSpace);
        }

        #endregion //Methods

        #region Nested types
        /// <summary>
        /// Supports binding EntityClient parameters to Object Services parameters.
        /// </summary>
        private class ParameterBinder
        {
            private readonly EntityParameter _entityParameter;
            private readonly ObjectParameter _objectParameter;

            internal ParameterBinder(EntityParameter entityParameter, ObjectParameter objectParameter)
            {
                _entityParameter = entityParameter;
                _objectParameter = objectParameter;
            }

            internal void OnDataReaderClosingHandler(object sender, EventArgs args)
            {
                // When the reader is closing, out/inout parameter values are set on the EntityParameter
                // instance. Pass this value through to the corresponding ObjectParameter.
                if (_entityParameter.Value != DBNull.Value && _objectParameter.MappableType.IsEnum)
                {
                    _objectParameter.Value = Enum.ToObject(_objectParameter.MappableType, _entityParameter.Value);
                }
                else
                {
                    _objectParameter.Value = _entityParameter.Value;
                }
            }
        }
        #endregion

        internal CollectionColumnMap ColumnMapBuilder { get; set; }
    }
}
