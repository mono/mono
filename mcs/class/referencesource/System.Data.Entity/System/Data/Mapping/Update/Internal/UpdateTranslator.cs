//---------------------------------------------------------------------
// <copyright file="UpdateTranslator.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System.Collections.Generic;
using System.Data.Objects;
using System.Data.Common.Utils;
using System.Data.Common.CommandTrees;
using System.Data.Common;
using System.Threading;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Data.Metadata.Edm;
using System.Data.EntityClient;
using System.Data.Spatial;
using System.Globalization;
using System.Data.Entity;
using System.Linq;

namespace System.Data.Mapping.Update.Internal
{
    /// <summary>
    /// This class performs to following tasks to persist C-Space changes to the store:
    /// <list>
    /// <item>Extract changes from the entity state manager</item>
    /// <item>Group changes by C-Space extent</item>
    /// <item>For each affected S-Space table, perform propagation (get changes in S-Space terms)</item>
    /// <item>Merge S-Space inserts and deletes into updates where appropriate</item>
    /// <item>Produce S-Space commands implementating the modifications (insert, delete and update SQL statements)</item>
    /// </list>
    /// </summary>
    internal partial class UpdateTranslator
    {
        #region Constructors
        /// <summary>
        /// Constructs a grouper based on the contents of the given entity state manager.
        /// </summary>
        /// <param name="stateManager">Entity state manager containing changes to be processed.</param>
        /// <param name="metadataWorkspace">Metadata workspace.</param>
        /// <param name="connection">Map connection</param>
        /// <param name="commandTimeout">Timeout for update commands; null means 'use provider default'</param>
        private UpdateTranslator(IEntityStateManager stateManager, MetadataWorkspace metadataWorkspace, EntityConnection connection, int? commandTimeout)
        {
            EntityUtil.CheckArgumentNull(stateManager, "stateManager");
            EntityUtil.CheckArgumentNull(metadataWorkspace, "metadataWorkspace");
            EntityUtil.CheckArgumentNull(connection, "connection");

            // propagation state
            m_changes = new Dictionary<EntitySetBase, ChangeNode>();
            m_functionChanges = new Dictionary<EntitySetBase, List<ExtractedStateEntry>>();
            m_stateEntries = new List<IEntityStateEntry>();
            m_knownEntityKeys = new Set<EntityKey>();
            m_requiredEntities = new Dictionary<EntityKey, AssociationSet>();
            m_optionalEntities = new Set<EntityKey>();
            m_includedValueEntities = new Set<EntityKey>();

            // workspace state
            m_metadataWorkspace = metadataWorkspace;
            m_viewLoader = metadataWorkspace.GetUpdateViewLoader();
            m_stateManager = stateManager;

            // ancillary propagation services
            m_recordConverter = new RecordConverter(this);
            m_constraintValidator = new RelationshipConstraintValidator(this);

            m_providerServices = DbProviderServices.GetProviderServices(connection.StoreProviderFactory);
            m_connection = connection;
            m_commandTimeout = commandTimeout;

            // metadata cache
            m_extractorMetadata = new Dictionary<Tuple<EntitySetBase, StructuralType>, ExtractorMetadata>(); ;

            // key management
            KeyManager = new KeyManager(this);
            KeyComparer = CompositeKey.CreateComparer(KeyManager);
        }

        #endregion

        #region Fields
        // propagation state
        private readonly Dictionary<EntitySetBase, ChangeNode> m_changes;
        private readonly Dictionary<EntitySetBase, List<ExtractedStateEntry>> m_functionChanges;
        private readonly List<IEntityStateEntry> m_stateEntries;
        private readonly Set<EntityKey> m_knownEntityKeys;
        private readonly Dictionary<EntityKey, AssociationSet> m_requiredEntities;
        private readonly Set<EntityKey> m_optionalEntities;
        private readonly Set<EntityKey> m_includedValueEntities;

        // workspace state
        private readonly MetadataWorkspace m_metadataWorkspace;
        private readonly ViewLoader m_viewLoader;
        private readonly IEntityStateManager m_stateManager;

        // ancillary propagation services
        private readonly RecordConverter m_recordConverter;
        private readonly RelationshipConstraintValidator m_constraintValidator;

        // provider information
        private readonly DbProviderServices m_providerServices;
        private readonly EntityConnection m_connection;
        private readonly int? m_commandTimeout;
        private Dictionary<StorageModificationFunctionMapping, DbCommandDefinition> m_modificationFunctionCommandDefinitions;

        // metadata cache
        private readonly Dictionary<Tuple<EntitySetBase, StructuralType>, ExtractorMetadata> m_extractorMetadata;

        // static members
        private static readonly List<string> s_emptyMemberList = new List<string>();
        #endregion

        #region Properties
        /// <summary>
        /// Gets workspace used in this session.
        /// </summary>
        internal MetadataWorkspace MetadataWorkspace
        {
            get { return m_metadataWorkspace; }
        }

        /// <summary>
        /// Gets key manager that handles interpretation of keys (including resolution of 
        /// referential-integrity/foreign key constraints)
        /// </summary>
        internal readonly KeyManager KeyManager;
        
        /// <summary>
        /// Gets the view loader metadata wrapper for the current workspace.
        /// </summary>
        internal ViewLoader ViewLoader
        {
            get { return m_viewLoader; }
        }

        /// <summary>
        /// Gets record converter which translates state entry records into propagator results.
        /// </summary>
        internal RecordConverter RecordConverter
        {
            get { return m_recordConverter; }
        }

        /// <summary>
        /// Gets command timeout for update commands. If null, use default.
        /// </summary>
        internal int? CommandTimeout
        {
            get { return m_commandTimeout; }
        }

        internal readonly IEqualityComparer<CompositeKey> KeyComparer;
        #endregion

        #region Methods
        /// <summary>
        /// Registers any referential constraints contained in the state entry (so that
        /// constrained members have the same identifier values). Only processes relationships
        /// with referential constraints defined.
        /// </summary>
        /// <param name="stateEntry">State entry</param>
        internal void RegisterReferentialConstraints(IEntityStateEntry stateEntry)
        {
            if (stateEntry.IsRelationship)
            {
                AssociationSet associationSet = (AssociationSet)stateEntry.EntitySet;
                if (0 < associationSet.ElementType.ReferentialConstraints.Count)
                {
                    DbDataRecord record = stateEntry.State == EntityState.Added ?
                        (DbDataRecord)stateEntry.CurrentValues : stateEntry.OriginalValues;
                    foreach (ReferentialConstraint constraint in associationSet.ElementType.ReferentialConstraints)
                    {
                        // retrieve keys at the ends
                        EntityKey principalKey = (EntityKey)record[constraint.FromRole.Name];
                        EntityKey dependentKey = (EntityKey)record[constraint.ToRole.Name];

                        // associate keys, where the from side 'owns' the to side
                        using (ReadOnlyMetadataCollection<EdmProperty>.Enumerator principalPropertyEnum = constraint.FromProperties.GetEnumerator())
                        using (ReadOnlyMetadataCollection<EdmProperty>.Enumerator dependentPropertyEnum = constraint.ToProperties.GetEnumerator())
                        {
                            while (principalPropertyEnum.MoveNext() && dependentPropertyEnum.MoveNext())
                            {
                                int principalKeyMemberCount;
                                int dependentKeyMemberCount;

                                // get offsets for from and to key properties
                                int principalOffset = GetKeyMemberOffset(constraint.FromRole, principalPropertyEnum.Current,
                                    out principalKeyMemberCount);
                                int dependentOffset = GetKeyMemberOffset(constraint.ToRole, dependentPropertyEnum.Current,
                                    out dependentKeyMemberCount);

                                int principalIdentifier = this.KeyManager.GetKeyIdentifierForMemberOffset(principalKey, principalOffset, principalKeyMemberCount);
                                int dependentIdentifier = this.KeyManager.GetKeyIdentifierForMemberOffset(dependentKey, dependentOffset, dependentKeyMemberCount);

                                // register equivalence of identifiers
                                this.KeyManager.AddReferentialConstraint(stateEntry, dependentIdentifier, principalIdentifier);
                            }
                        }
                    }
                }
            }
            else if (!stateEntry.IsKeyEntry)
            {
                if (stateEntry.State == EntityState.Added || stateEntry.State == EntityState.Modified)
                {
                    RegisterEntityReferentialConstraints(stateEntry, true);
                }
                if (stateEntry.State == EntityState.Deleted || stateEntry.State == EntityState.Modified)
                {
                    RegisterEntityReferentialConstraints(stateEntry, false);
                }
            }
        }

        private void RegisterEntityReferentialConstraints(IEntityStateEntry stateEntry, bool currentValues)
        {
            IExtendedDataRecord record = currentValues
                ? (IExtendedDataRecord)stateEntry.CurrentValues
                : (IExtendedDataRecord)stateEntry.OriginalValues;
            EntitySet entitySet = (EntitySet)stateEntry.EntitySet;
            EntityKey dependentKey = stateEntry.EntityKey;

            foreach (var foreignKey in entitySet.ForeignKeyDependents)
            {
                AssociationSet associationSet = foreignKey.Item1;
                ReferentialConstraint constraint = foreignKey.Item2;
                EntityType dependentType = MetadataHelper.GetEntityTypeForEnd((AssociationEndMember)constraint.ToRole);
                if (dependentType.IsAssignableFrom(record.DataRecordInfo.RecordType.EdmType))
                {
                    EntityKey principalKey = null;

                    // First, check for an explicit reference
                    if (!currentValues || !m_stateManager.TryGetReferenceKey(dependentKey, (AssociationEndMember)constraint.FromRole, out principalKey))
                    {
                        // build a key based on the foreign key values
                        EntityType principalType = MetadataHelper.GetEntityTypeForEnd((AssociationEndMember)constraint.FromRole);
                        bool hasNullValue = false;
                        object[] keyValues = new object[principalType.KeyMembers.Count];
                        for (int i = 0, n = keyValues.Length; i < n; i++)
                        {
                            EdmProperty keyMember = (EdmProperty)principalType.KeyMembers[i];

                            // Find corresponding foreign key value
                            int constraintOrdinal = constraint.FromProperties.IndexOf((EdmProperty)keyMember);
                            int recordOrdinal = record.GetOrdinal(constraint.ToProperties[constraintOrdinal].Name);
                            if (record.IsDBNull(recordOrdinal))
                            {
                                hasNullValue = true;
                                break;
                            }
                            keyValues[i] = record.GetValue(recordOrdinal);
                        }

                        if (!hasNullValue)
                        {
                            EntitySet principalSet = associationSet.AssociationSetEnds[constraint.FromRole.Name].EntitySet;
                            if (1 == keyValues.Length)
                            {
                                principalKey = new EntityKey(principalSet, keyValues[0]);
                            }
                            else
                            {
                                principalKey = new EntityKey(principalSet, keyValues);
                            }
                        }
                    }

                    if (null != principalKey)
                    {
                        // find the right principal key... (first, existing entities; then, added entities; finally, just the key)
                        IEntityStateEntry existingPrincipal;
                        EntityKey tempKey;
                        if (m_stateManager.TryGetEntityStateEntry(principalKey, out existingPrincipal))
                        {
                            // nothing to do. the principal key will resolve to the existing entity
                        }
                        else if (currentValues && this.KeyManager.TryGetTempKey(principalKey, out tempKey))
                        {
                            // if we aren't dealing with current values, we cannot resolve to a temp key (original values
                            // cannot indicate a relationship to an 'added' entity).
                            if (null == tempKey)
                            {
                                throw EntityUtil.Update(Strings.Update_AmbiguousForeignKey(constraint.ToRole.DeclaringType.FullName), null, stateEntry);
                            }
                            else
                            {
                                principalKey = tempKey;
                            }
                        }

                        // pull the principal end into the update pipeline (supports value propagation)
                        AddValidAncillaryKey(principalKey, m_optionalEntities);

                        // associate keys, where the from side 'owns' the to side
                        for (int i = 0, n = constraint.FromProperties.Count; i < n; i++)
                        {
                            var principalProperty = constraint.FromProperties[i];
                            var dependentProperty = constraint.ToProperties[i];

                            int principalKeyMemberCount;

                            // get offsets for from and to key properties
                            int principalOffset = GetKeyMemberOffset(constraint.FromRole, principalProperty, out principalKeyMemberCount);
                            int principalIdentifier = this.KeyManager.GetKeyIdentifierForMemberOffset(principalKey, principalOffset, principalKeyMemberCount);
                            int dependentIdentifier;

                            if (entitySet.ElementType.KeyMembers.Contains(dependentProperty))
                            {
                                int dependentKeyMemberCount;
                                int dependentOffset = GetKeyMemberOffset(constraint.ToRole, dependentProperty,
                                    out dependentKeyMemberCount);
                                dependentIdentifier = this.KeyManager.GetKeyIdentifierForMemberOffset(dependentKey, dependentOffset, dependentKeyMemberCount);
                            }
                            else
                            {
                                dependentIdentifier = this.KeyManager.GetKeyIdentifierForMember(dependentKey, dependentProperty.Name, currentValues);
                            }

                            // don't allow the user to insert or update an entity that refers to a deleted principal
                            if (currentValues && null != existingPrincipal && existingPrincipal.State == EntityState.Deleted &&
                                (stateEntry.State == EntityState.Added || stateEntry.State == EntityState.Modified))
                            {
                                throw EntityUtil.Update(
                                    Strings.Update_InsertingOrUpdatingReferenceToDeletedEntity(associationSet.ElementType.FullName),
                                    null,
                                    stateEntry,
                                    existingPrincipal);
                            }

                            // register equivalence of identifiers
                            this.KeyManager.AddReferentialConstraint(stateEntry, dependentIdentifier, principalIdentifier);
                        }
                    }
                }
            }
        }

        // requires: role must not be null and property must be a key member for the role end
        private static int GetKeyMemberOffset(RelationshipEndMember role, EdmProperty property, out int keyMemberCount)
        {
            Debug.Assert(null != role);
            Debug.Assert(null != property);
            Debug.Assert(BuiltInTypeKind.RefType == role.TypeUsage.EdmType.BuiltInTypeKind,
                "relationship ends must be of RefType");
            RefType endType = (RefType)role.TypeUsage.EdmType;
            Debug.Assert(BuiltInTypeKind.EntityType == endType.ElementType.BuiltInTypeKind,
                "relationship ends must reference EntityType");
            EntityType entityType = (EntityType)endType.ElementType;
            keyMemberCount = entityType.KeyMembers.Count;
            return entityType.KeyMembers.IndexOf(property);
        }

        /// <summary>
        /// Yields all relationship state entries with the given key as an end.
        /// </summary>
        /// <param name="entityKey"></param>
        /// <returns></returns>
        internal IEnumerable<IEntityStateEntry> GetRelationships(EntityKey entityKey)
        {
            return m_stateManager.FindRelationshipsByKey(entityKey);
        }

        /// <summary>
        /// Persists stateManager changes to the store.
        /// </summary>
        /// <param name="stateManager">StateManager containing changes to persist.</param>
        /// <param name="adapter">Map adapter requesting the changes.</param>
        /// <returns>Total number of state entries affected</returns>
        internal static Int32 Update(IEntityStateManager stateManager, IEntityAdapter adapter)
        {
            // provider/connection details
            EntityConnection connection = (EntityConnection)adapter.Connection;
            MetadataWorkspace metadataWorkspace = connection.GetMetadataWorkspace();
            int? commandTimeout = adapter.CommandTimeout;

            UpdateTranslator translator = new UpdateTranslator(stateManager, metadataWorkspace, connection, commandTimeout);
                
            // tracks values for identifiers in this session
            Dictionary<int, object> identifierValues = new Dictionary<int, object>();

            // tracks values for generated values in this session
            List<KeyValuePair<PropagatorResult, object>> generatedValues = new List<KeyValuePair<PropagatorResult, object>>();

            IEnumerable<UpdateCommand> orderedCommands = translator.ProduceCommands();

            // used to track the source of commands being processed in case an exception is thrown
            UpdateCommand source = null;
            try
            {
                foreach (UpdateCommand command in orderedCommands)
                {
                    // Remember the data sources so that we can throw meaningful exception
                    source = command;
                    long rowsAffected = command.Execute(translator, connection, identifierValues, generatedValues);
                    translator.ValidateRowsAffected(rowsAffected, source);
                }
            }
            catch (Exception e)
            {
                // we should not be wrapping all exceptions
                if (UpdateTranslator.RequiresContext(e))
                {
                    throw EntityUtil.Update(System.Data.Entity.Strings.Update_GeneralExecutionException, e, translator.DetermineStateEntriesFromSource(source));
                }
                throw;
            }

            translator.BackPropagateServerGen(generatedValues);

            int totalStateEntries = translator.AcceptChanges(adapter);

            return totalStateEntries;
        }

        private IEnumerable<UpdateCommand> ProduceCommands()
        {
            // load all modified state entries
            PullModifiedEntriesFromStateManager();
            PullUnchangedEntriesFromStateManager();

            // check constraints
            m_constraintValidator.ValidateConstraints();
            this.KeyManager.ValidateReferentialIntegrityGraphAcyclic();
            
            // gather all commands (aggregate in a dependency orderer to determine operation order
            IEnumerable<UpdateCommand> dynamicCommands = this.ProduceDynamicCommands();
            IEnumerable<UpdateCommand> functionCommands = this.ProduceFunctionCommands();
            UpdateCommandOrderer orderer = new UpdateCommandOrderer(dynamicCommands.Concat(functionCommands), this);
            IEnumerable<UpdateCommand> orderedCommands;
            IEnumerable<UpdateCommand> remainder;
            if (!orderer.TryTopologicalSort(out orderedCommands, out remainder))
            {
                // throw an exception if it is not possible to perform dependency ordering
                throw DependencyOrderingError(remainder);
            }

            return orderedCommands;
        }

        // effects: given rows affected, throws if the count suggests a concurrency failure.
        // Throws a concurrency exception based on the current command sources (which allow
        // us to populated the EntityStateEntries on UpdateException)
        private void ValidateRowsAffected(long rowsAffected, UpdateCommand source)
        {
            // 0 rows affected indicates a concurrency failure; negative values suggest rowcount is off; 
            // positive values suggest at least one row was affected (we generally expect exactly one, 
            // but triggers/view logic/logging may change this value)
            if (0 == rowsAffected)
            {
                var stateEntries = DetermineStateEntriesFromSource(source);
                throw EntityUtil.UpdateConcurrency(rowsAffected, null, stateEntries);
            }
        }

        private IEnumerable<IEntityStateEntry> DetermineStateEntriesFromSource(UpdateCommand source)
        {
            if (null == source)
            {
                return Enumerable.Empty<IEntityStateEntry>();
            }
            return source.GetStateEntries(this);
        }

        // effects: Given a list of pairs describing the contexts for server generated values and their actual
        // values, backpropagates to the relevant state entries
        private void BackPropagateServerGen(List<KeyValuePair<PropagatorResult, object>> generatedValues)
        {
            foreach (KeyValuePair<PropagatorResult, object> generatedValue in generatedValues)
            {
                PropagatorResult context;

                // check if a redirect to "owner" result is possible
                if (PropagatorResult.NullIdentifier == generatedValue.Key.Identifier ||
                    !KeyManager.TryGetIdentifierOwner(generatedValue.Key.Identifier, out context))                
                {
                    // otherwise, just use the straightforward context
                    context = generatedValue.Key;
                }

                object value = generatedValue.Value;
                if (context.Identifier == PropagatorResult.NullIdentifier)
                {
                    SetServerGenValue(context, value);
                }
                else
                {
                    // check if we need to back propagate this value to any other positions (e.g. for foreign keys)
                    foreach (int dependent in this.KeyManager.GetDependents(context.Identifier))
                    {
                        if (this.KeyManager.TryGetIdentifierOwner(dependent, out context))
                        {
                            SetServerGenValue(context, value);
                        }
                    }
                }
            }
        }

        private void SetServerGenValue(PropagatorResult context, object value)
        {
            if (context.RecordOrdinal != PropagatorResult.NullOrdinal)
            {
                CurrentValueRecord targetRecord = context.Record;

                // determine if type compensation is required
                IExtendedDataRecord recordWithMetadata = (IExtendedDataRecord)targetRecord;
                EdmMember member = recordWithMetadata.DataRecordInfo.FieldMetadata[context.RecordOrdinal].FieldType;

                value = value ?? DBNull.Value; // records expect DBNull rather than null
                value = AlignReturnValue(value, member, context);
                targetRecord.SetValue(context.RecordOrdinal, value);
            }
        }

        /// <summary>
        /// Aligns a value returned from the store with the expected type for the member.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <param name="member">Metadata for the member being set.</param>
        /// <param name="context">The context generating the return value.</param>
        /// <returns>Converted return value</returns>
        private object AlignReturnValue(object value, EdmMember member, PropagatorResult context)
        {
            if (DBNull.Value.Equals(value))
            {
                // check if there is a nullability constraint on the value
                if (BuiltInTypeKind.EdmProperty == member.BuiltInTypeKind &&
                    !((EdmProperty)member).Nullable)
                {
                    throw EntityUtil.Update(System.Data.Entity.Strings.Update_NullReturnValueForNonNullableMember(
                        member.Name, 
                        member.DeclaringType.FullName), null);
                }
            }
            else if (!Helper.IsSpatialType(member.TypeUsage))
            {
                Type clrType;
                Type clrEnumType = null;
                if (Helper.IsEnumType(member.TypeUsage.EdmType))
                {
                    PrimitiveType underlyingType = Helper.AsPrimitive(member.TypeUsage.EdmType);
                    clrEnumType = context.Record.GetFieldType(context.RecordOrdinal);
                    clrType = underlyingType.ClrEquivalentType;
                    Debug.Assert(clrEnumType.IsEnum); 
                }
                else
                {
                    // convert the value to the appropriate CLR type
                    Debug.Assert(BuiltInTypeKind.PrimitiveType == member.TypeUsage.EdmType.BuiltInTypeKind,
                        "we only allow return values that are instances of EDM primitive or enum types");
                    PrimitiveType primitiveType = (PrimitiveType)member.TypeUsage.EdmType;
                    clrType = primitiveType.ClrEquivalentType;
                }

                try
                {
                    value = Convert.ChangeType(value, clrType, CultureInfo.InvariantCulture);
                    if (clrEnumType != null)
                    {
                        value = Enum.ToObject(clrEnumType, value);
                    }
                }
                catch (Exception e)
                {
                    // we should not be wrapping all exceptions
                    if (UpdateTranslator.RequiresContext(e)) 
                    {
                        Type userClrType = clrEnumType ?? clrType;
                        throw EntityUtil.Update(System.Data.Entity.Strings.Update_ReturnValueHasUnexpectedType(
                            value.GetType().FullName,
                            userClrType.FullName,
                            member.Name,
                            member.DeclaringType.FullName), e);
                    }
                    throw;
                }
            }

            // return the adjusted value
            return value;
        }

        /// <summary>
        /// Accept changes to entities and relationships processed by this translator instance.
        /// </summary>
        /// <param name="adapter">Data adapter</param>
        /// <returns>Number of state entries affected.</returns>
        private int AcceptChanges(IEntityAdapter adapter)
        {
            int affectedCount = 0;
            foreach (IEntityStateEntry stateEntry in m_stateEntries)
            {
                // only count and accept changes for state entries that are being explicitly modified
                if (EntityState.Unchanged != stateEntry.State)
                {
                    if (adapter.AcceptChangesDuringUpdate)
                    {
                        stateEntry.AcceptChanges();
                    }
                    affectedCount++;
                }
            }
            return affectedCount;
        }

        /// <summary>
        /// Gets extents for which this translator has identified changes to be handled
        /// by the standard update pipeline.
        /// </summary>
        /// <returns>Enumeration of modified C-Space extents.</returns>
        private IEnumerable<EntitySetBase> GetDynamicModifiedExtents()
        {
            return m_changes.Keys;
        }

        /// <summary>
        /// Gets extents for which this translator has identified changes to be handled
        /// by function mappings.
        /// </summary>
        /// <returns>Enumreation of modified C-Space extents.</returns>
        private IEnumerable<EntitySetBase> GetFunctionModifiedExtents()
        {
            return m_functionChanges.Keys;
        }

        /// <summary>
        /// Produce dynamic store commands for this translator's changes.
        /// </summary>
        /// <returns>Database commands in a safe order</returns>
        private IEnumerable<UpdateCommand> ProduceDynamicCommands()
        {
            // Initialize DBCommand update compiler
            UpdateCompiler updateCompiler = new UpdateCompiler(this);
            
            // Determine affected
            Set<EntitySet> tables = new Set<EntitySet>();

            foreach (EntitySetBase extent in GetDynamicModifiedExtents())
            {
                Set<EntitySet> affectedTables = m_viewLoader.GetAffectedTables(extent, m_metadataWorkspace);
                //Since these extents don't have Functions defined for update operations,
                //the affected tables should be provided via MSL.
                //If we dont find any throw an exception
                if (affectedTables.Count == 0)
                {
                    throw EntityUtil.Update(System.Data.Entity.Strings.Update_MappingNotFound(
                        extent.Name), null /*stateEntries*/);
                }

                foreach (EntitySet table in affectedTables)
                {
                    tables.Add(table);
                }
            }

            // Determine changes to apply to each table
            foreach (EntitySet table in tables)
            {
                DbQueryCommandTree umView = m_connection.GetMetadataWorkspace().GetCqtView(table);
                
                // Propagate changes to root of tree (at which point they are S-Space changes)
                ChangeNode changeNode = Propagator.Propagate(this, table, umView);
                
                // Process changes for the table
                TableChangeProcessor change = new TableChangeProcessor(table);
                foreach (UpdateCommand command in change.CompileCommands(changeNode, updateCompiler))
                {
                    yield return command;
                }
            }
        }

        // Generates and caches a command definition for the given function
        internal DbCommandDefinition GenerateCommandDefinition(StorageModificationFunctionMapping functionMapping)
        {
            if (null == m_modificationFunctionCommandDefinitions) 
            { 
                m_modificationFunctionCommandDefinitions = new Dictionary<StorageModificationFunctionMapping,DbCommandDefinition>();
            }
            DbCommandDefinition commandDefinition;
            if (!m_modificationFunctionCommandDefinitions.TryGetValue(functionMapping, out commandDefinition))
            {
                // synthesize a RowType for this mapping
                TypeUsage resultType = null;
                if (null != functionMapping.ResultBindings && 0 < functionMapping.ResultBindings.Count)
                {
                    List<EdmProperty> properties = new List<EdmProperty>(functionMapping.ResultBindings.Count);
                    foreach (StorageModificationFunctionResultBinding resultBinding in functionMapping.ResultBindings)
                    {
                        properties.Add(new EdmProperty(resultBinding.ColumnName, resultBinding.Property.TypeUsage));
                    }
                    RowType rowType = new RowType(properties);
                    CollectionType collectionType = new CollectionType(rowType);
                    resultType = TypeUsage.Create(collectionType);
                }

                // add function parameters
                IEnumerable<KeyValuePair<string, TypeUsage>> functionParams =
                    functionMapping.Function.Parameters.Select(paramInfo => new KeyValuePair<string, TypeUsage>(paramInfo.Name, paramInfo.TypeUsage));
                
                // construct DbFunctionCommandTree including implict return type
                DbFunctionCommandTree tree = new DbFunctionCommandTree(m_metadataWorkspace, DataSpace.SSpace,
                    functionMapping.Function, resultType, functionParams);
                                
                commandDefinition = m_providerServices.CreateCommandDefinition(tree);
            }
            return commandDefinition;
        }

        // Produces all function commands in a safe order
        private IEnumerable<UpdateCommand> ProduceFunctionCommands()
        {
            foreach (EntitySetBase extent in GetFunctionModifiedExtents())
            {
                // Get a handle on the appropriate translator
                ModificationFunctionMappingTranslator translator = m_viewLoader.GetFunctionMappingTranslator(extent, m_metadataWorkspace);

                if (null != translator)
                {
                    // Compile commands
                    foreach (ExtractedStateEntry stateEntry in GetExtentFunctionModifications(extent))
                    {
                        FunctionUpdateCommand command = translator.Translate(this, stateEntry);
                        if (null != command) 
                        {
                            yield return command;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets a metadata wrapper for the given type. The wrapper makes
        /// certain tasks in the update pipeline more efficient.
        /// </summary>
        /// <param name="type">Structural type</param>
        /// <returns>Metadata wrapper</returns>
        internal ExtractorMetadata GetExtractorMetadata(EntitySetBase entitySetBase, StructuralType type)
        {
            ExtractorMetadata metadata;
            var key = Tuple.Create(entitySetBase, type);
            if (!m_extractorMetadata.TryGetValue(key, out metadata))
            {
                metadata = new ExtractorMetadata(entitySetBase, type, this);
                m_extractorMetadata.Add(key, metadata);
            }
            return metadata;
        }

        /// <summary>
        /// Returns error when it is not possible to order update commands. Argument is the 'remainder', or commands
        /// that could not be ordered due to a cycle.
        /// </summary>
        private UpdateException DependencyOrderingError(IEnumerable<UpdateCommand> remainder)
        {
            Debug.Assert(null != remainder && remainder.Count() > 0, "must provide non-empty remainder");

            HashSet<IEntityStateEntry> stateEntries = new HashSet<IEntityStateEntry>();

            foreach (UpdateCommand command in remainder)
            {
                stateEntries.UnionWith(command.GetStateEntries(this));
            }

            // throw exception containing all related state entries
            throw EntityUtil.Update(System.Data.Entity.Strings.Update_ConstraintCycle, null, stateEntries);
        }

        /// <summary>
        /// Creates a command in the current context.
        /// </summary>
        /// <param name="commandTree">DbCommand tree</param>
        /// <returns>DbCommand produced by the current provider.</returns>
        internal DbCommand CreateCommand(DbModificationCommandTree commandTree)
        {
            DbCommand command;
            Debug.Assert(null != m_providerServices, "constructor ensures either the command definition " +
                    "builder or provider service is available");
            Debug.Assert(null != m_connection.StoreConnection, "EntityAdapter.Update ensures the store connection is set");
            try 
            {
                command = m_providerServices.CreateCommand(commandTree);
            }
            catch (Exception e) 
            {
                // we should not be wrapping all exceptions
                if (UpdateTranslator.RequiresContext(e))
                {
                    // we don't wan't folks to have to know all the various types of exceptions that can 
                    // occur, so we just rethrow a CommandDefinitionException and make whatever we caught  
                    // the inner exception of it.
                    throw EntityUtil.CommandCompilation(System.Data.Entity.Strings.EntityClient_CommandDefinitionPreparationFailed, e);
                }
                throw;
            }
            return command;
        }

        /// <summary>
        /// Helper method to allow the setting of parameter values to update stored procedures.
        /// Allows the DbProvider an opportunity to rewrite the parameter to suit provider specific needs.
        /// </summary>
        /// <param name="parameter">Parameter to set.</param>
        /// <param name="typeUsage">The type of the parameter.</param>
        /// <param name="value">The value to which to set the parameter.</param>
        internal void SetParameterValue(DbParameter parameter, TypeUsage typeUsage, object value)
        {
            m_providerServices.SetParameterValue(parameter, typeUsage, value);
        }

        /// <summary>
        /// Determines whether the given exception requires additional context from the update pipeline (in other
        /// words, whether the exception should be wrapped in an UpdateException).
        /// </summary>
        /// <param name="e">Exception to test.</param>
        /// <returns>true if exception should be wrapped; false otherwise</returns>
        internal static bool RequiresContext(Exception e)
        {
            // if the exception isn't catchable, never wrap
            if (!EntityUtil.IsCatchableExceptionType(e)) { return false; }

            // update and incompatible provider exceptions already contain the necessary context
            return !(e is UpdateException) && !(e is ProviderIncompatibleException);
        }

        #region Private initialization methods
        /// <summary>
        /// Retrieve all modified entries from the state manager.
        /// </summary>
        private void PullModifiedEntriesFromStateManager()
        {
            // do a first pass over added entries to register 'by value' entity key targets that may be resolved as 
            // via a foreign key
            foreach (IEntityStateEntry addedEntry in m_stateManager.GetEntityStateEntries(EntityState.Added))
            {
                if (!addedEntry.IsRelationship && !addedEntry.IsKeyEntry)
                {
                    this.KeyManager.RegisterKeyValueForAddedEntity(addedEntry);
                }
            }

            // do a second pass over entries to register referential integrity constraints
            // for server-generation
            foreach (IEntityStateEntry modifiedEntry in m_stateManager.GetEntityStateEntries(EntityState.Modified | EntityState.Added | EntityState.Deleted))
            {
                RegisterReferentialConstraints(modifiedEntry);
            }

            foreach (IEntityStateEntry modifiedEntry in m_stateManager.GetEntityStateEntries(EntityState.Modified | EntityState.Added | EntityState.Deleted))
            {
                LoadStateEntry(modifiedEntry);
            }
        }


        /// <summary>
        /// Retrieve all required/optional/value entries into the state manager. These are entries that --
        /// although unmodified -- affect or are affected by updates.
        /// </summary>
        private void PullUnchangedEntriesFromStateManager()
        {
            foreach (KeyValuePair<EntityKey, AssociationSet> required in m_requiredEntities)
            {
                EntityKey key = required.Key;

                if (!m_knownEntityKeys.Contains(key))
                {
                    // pull the value into the translator if we don't already it
                    IEntityStateEntry requiredEntry;

                    if (m_stateManager.TryGetEntityStateEntry(key, out requiredEntry) && !requiredEntry.IsKeyEntry)
                    {
                        // load the object as a no-op update
                        LoadStateEntry(requiredEntry);
                    }
                    else
                    {
                        // throw an exception
                        throw EntityUtil.UpdateMissingEntity(required.Value.Name, TypeHelpers.GetFullName(key.EntityContainerName, key.EntitySetName));
                    }
                }
            }

            foreach (EntityKey key in m_optionalEntities)
            {
                if (!m_knownEntityKeys.Contains(key))
                {
                    IEntityStateEntry optionalEntry;

                    if (m_stateManager.TryGetEntityStateEntry(key, out optionalEntry) && !optionalEntry.IsKeyEntry)
                    {
                        // load the object as a no-op update
                        LoadStateEntry(optionalEntry);
                    }
                }
            }

            foreach (EntityKey key in m_includedValueEntities)
            {
                if (!m_knownEntityKeys.Contains(key))
                {
                    IEntityStateEntry valueEntry;

                    if (m_stateManager.TryGetEntityStateEntry(key, out valueEntry))
                    {
                        // Convert state entry so that its values are known to the update pipeline.
                        var result = m_recordConverter.ConvertCurrentValuesToPropagatorResult(valueEntry, ModifiedPropertiesBehavior.NoneModified);
                    }
                }
            }
        }

        /// <summary>
        /// Validates and tracks a state entry being processed by this translator.
        /// </summary>
        /// <param name="stateEntry"></param>
        private void ValidateAndRegisterStateEntry(IEntityStateEntry stateEntry)
        {
            EntityUtil.CheckArgumentNull(stateEntry, "stateEntry");

            EntitySetBase extent = stateEntry.EntitySet;
            if (null == extent) 
            {
                throw EntityUtil.InternalError(EntityUtil.InternalErrorCode.InvalidStateEntry, 1);
            }

            // Determine the key. May be null if the state entry does not represent an entity.
            EntityKey entityKey = stateEntry.EntityKey;
            IExtendedDataRecord record = null;

            // verify the structure of the entry values
            if (0 != ((EntityState.Added | EntityState.Modified | EntityState.Unchanged) & stateEntry.State))
            {
                // added, modified and unchanged entries have current values
                record = (IExtendedDataRecord)stateEntry.CurrentValues;
                ValidateRecord(extent, record, stateEntry);
            }
            if (0 != ((EntityState.Modified | EntityState.Deleted | EntityState.Unchanged) & stateEntry.State))
            {
                // deleted, modified and unchanged entries have original values
                record = (IExtendedDataRecord)stateEntry.OriginalValues;
                ValidateRecord(extent, record, stateEntry);
            }
            Debug.Assert(null != record, "every state entry must contain a record");

            // check for required ends of relationships
            AssociationSet associationSet = extent as AssociationSet;
            if (null != associationSet)
            {
                AssociationSetMetadata associationSetMetadata = m_viewLoader.GetAssociationSetMetadata(associationSet, m_metadataWorkspace);

                if (associationSetMetadata.HasEnds)
                {
                    foreach (FieldMetadata field in record.DataRecordInfo.FieldMetadata)
                    {
                        // ends of relationship record must be EntityKeys
                        EntityKey end = (EntityKey)record.GetValue(field.Ordinal);

                        // ends of relationships must have AssociationEndMember metadata
                        AssociationEndMember endMetadata = (AssociationEndMember)field.FieldType;

                        if (associationSetMetadata.RequiredEnds.Contains(endMetadata))
                        {
                            if (!m_requiredEntities.ContainsKey(end))
                            {
                                m_requiredEntities.Add(end, associationSet);
                            }
                        }

                        else if (associationSetMetadata.OptionalEnds.Contains(endMetadata))
                        {
                            AddValidAncillaryKey(end, m_optionalEntities);
                        }

                        else if (associationSetMetadata.IncludedValueEnds.Contains(endMetadata))
                        {
                            AddValidAncillaryKey(end, m_includedValueEntities);
                        }
                    }
                }

                // register relationship with validator
                m_constraintValidator.RegisterAssociation(associationSet, record, stateEntry);
            }
            else
            {
                // register entity with validator
                m_constraintValidator.RegisterEntity(stateEntry);
            }

            // add to the list of entries being tracked
            m_stateEntries.Add(stateEntry);
            if (null != (object)entityKey) { m_knownEntityKeys.Add(entityKey); }
        }

        /// <summary>
        /// effects: given an entity key and a set, adds key to the set iff. the corresponding entity
        /// is:
        /// 
        ///     not a stub (or 'key') entry, and;
        ///     not a core element in the update pipeline (it's not being directly modified)
        /// </summary>
        private void AddValidAncillaryKey(EntityKey key, Set<EntityKey> keySet)
        {
            // Note: an entity is ancillary iff. it is unchanged (otherwise it is tracked as a "standard" changed entity)
            IEntityStateEntry endEntry;
            if (m_stateManager.TryGetEntityStateEntry(key, out endEntry) && // make sure the entity is tracked
                !endEntry.IsKeyEntry && // make sure the entity is not a stub
                endEntry.State == EntityState.Unchanged) // if the entity is being modified, it's already included anyways
            {
                keySet.Add(key);
            }
        }

        private void ValidateRecord(EntitySetBase extent, IExtendedDataRecord record, IEntityStateEntry entry)
        {
            Debug.Assert(null != extent, "must be verified by caller");

            DataRecordInfo recordInfo;
            if ((null == record) ||
                (null == (recordInfo = record.DataRecordInfo)) ||
                (null == recordInfo.RecordType))
            {
                throw EntityUtil.InternalError(EntityUtil.InternalErrorCode.InvalidStateEntry, 2);
            }

            VerifyExtent(MetadataWorkspace, extent);

            // additional validation happens lazily as values are loaded from the record
        }

        // Verifies the given extent is present in the given workspace.
        private static void VerifyExtent(MetadataWorkspace workspace, EntitySetBase extent)
        {
            // get the container to which the given extent belongs
            EntityContainer actualContainer = extent.EntityContainer;

            // try to retrieve the container in the given workspace
            EntityContainer referenceContainer = null;
            if (null != actualContainer)
            {
                workspace.TryGetEntityContainer(
                    actualContainer.Name, actualContainer.DataSpace, out referenceContainer);
            }
            
            // determine if the given extent lives in a container from the given workspace
            // (the item collections for each container are reference equivalent when they are declared in the
            // same item collection)
            if (null == actualContainer || null == referenceContainer ||
                !Object.ReferenceEquals(actualContainer, referenceContainer)) 
            {
                // 



                throw EntityUtil.Update(System.Data.Entity.Strings.Update_WorkspaceMismatch, null);
            }
        }

        private void LoadStateEntry(IEntityStateEntry stateEntry)
        {
            Debug.Assert(null != stateEntry, "state entry must exist");

            // make sure the state entry doesn't contain invalid data and register it with the
            // update pipeline
            ValidateAndRegisterStateEntry(stateEntry);

            // use data structure internal to the update pipeline instead of the raw state entry
            ExtractedStateEntry extractedStateEntry = new ExtractedStateEntry(this, stateEntry);

            // figure out if this state entry is being handled by a function (stored procedure) or
            // through dynamic SQL
            EntitySetBase extent = stateEntry.EntitySet;
            if (null == m_viewLoader.GetFunctionMappingTranslator(extent, m_metadataWorkspace))
            {
                // if there is no function mapping, register a ChangeNode (used for update
                // propagation and dynamic SQL generation)
                ChangeNode changeNode = GetExtentModifications(extent);
                if (null != extractedStateEntry.Original)
                {
                    changeNode.Deleted.Add(extractedStateEntry.Original);
                }
                if (null != extractedStateEntry.Current)
                {
                    changeNode.Inserted.Add(extractedStateEntry.Current);
                }
            }
            else
            {
                // for function updates, store off the extracted state entry in its entirety
                // (used when producing FunctionUpdateCommands)
                List<ExtractedStateEntry> functionEntries = GetExtentFunctionModifications(extent);
                functionEntries.Add(extractedStateEntry);
            }
        }



        /// <summary>
        /// Retrieve a change node for an extent. If none exists, creates and registers a new one.
        /// </summary>
        /// <param name="extent">Extent for which to return a change node.</param>
        /// <returns>Change node for requested extent.</returns>
        internal ChangeNode GetExtentModifications(EntitySetBase extent)
        {
            EntityUtil.CheckArgumentNull(extent, "extent");
            Debug.Assert(null != m_changes, "(UpdateTranslator/GetChangeNodeForExtent) method called before translator initialized");

            ChangeNode changeNode;

            if (!m_changes.TryGetValue(extent, out changeNode))
            {
                changeNode = new ChangeNode(TypeUsage.Create(extent.ElementType));
                m_changes.Add(extent, changeNode);
            }

            return changeNode;
        }
        
        /// <summary>
        /// Retrieve a list of state entries being processed by custom user functions.
        /// </summary>
        /// <param name="extent">Extent for which to return entries.</param>
        /// <returns>List storing the entries.</returns>
        internal List<ExtractedStateEntry> GetExtentFunctionModifications(EntitySetBase extent)
        {
            EntityUtil.CheckArgumentNull(extent, "extent");
            Debug.Assert(null != m_functionChanges, "method called before translator initialized");

            List<ExtractedStateEntry> entries;

            if (!m_functionChanges.TryGetValue(extent, out entries))
            {
                entries = new List<ExtractedStateEntry>();
                m_functionChanges.Add(extent, entries);
            }

            return entries;
        }
        #endregion
        #endregion
    }

    /// <summary>
    /// Enumeration of possible operators. 
    /// </summary>
    /// <remarks>
    /// The values are used to determine the order of operations (in the absence of any strong dependencies). 
    /// The chosen order is based on the observation that hidden dependencies (e.g. due to temporary keys in 
    /// the state manager or unknown FKs) favor deletes before inserts and updates before deletes. For instance, 
    /// a deleted entity may have the same real key value as an inserted entity. Similarly, a self-reference 
    /// may require a new dependent row to be updated before the prinpical row is inserted. Obviously, the actual
    /// constraints are required to make reliable decisions so this ordering is merely a heuristic.
    /// </remarks>
    internal enum ModificationOperator : byte
    {
        Update = 0,
        Delete = 1,
        Insert = 2,
    }
}
