//---------------------------------------------------------------------
// <copyright file="UpdateCommandOrderer.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

namespace System.Data.Mapping.Update.Internal
{
    using System.Collections.Generic;
    using System.Data.Common.Utils;
    using System.Data.Metadata.Edm;
    using System.Diagnostics;
    using System.Linq;

    internal class UpdateCommandOrderer : Graph<UpdateCommand>
    {
        /// <summary>
        /// Gets comparer used to resolve identifiers to actual 'owning' key values (e.g. across referential constraints)
        /// </summary>
        private readonly ForeignKeyValueComparer _keyComparer;

        /// <summary>
        /// Maps from tables to all "source" referential constraints (where the table declares
        /// foreign keys)
        /// </summary>
        private readonly KeyToListMap<EntitySetBase, ReferentialConstraint> _sourceMap;

        /// <summary>
        /// Maps from tables to all "target" referential constraints (where the table is
        /// referenced by a foreign key)
        /// </summary>
        private readonly KeyToListMap<EntitySetBase, ReferentialConstraint> _targetMap;

        /// <summary>
        /// Tracks whether any function commands exist in the current payload.
        /// </summary>
        private readonly bool _hasFunctionCommands;

        /// <summary>
        /// Gets translator producing this graph.
        /// </summary>
        private readonly UpdateTranslator _translator;

        internal UpdateCommandOrderer(IEnumerable<UpdateCommand> commands, UpdateTranslator translator)
            : base(EqualityComparer<UpdateCommand>.Default)
        {
            _translator = translator;
            _keyComparer = new ForeignKeyValueComparer(_translator.KeyComparer);

            HashSet<EntitySet> tables = new HashSet<EntitySet>();
            HashSet<EntityContainer> containers = new HashSet<EntityContainer>();

            // add all vertices (one vertex for every command)
            foreach (UpdateCommand command in commands)
            {
                if (null != command.Table)
                {
                    tables.Add(command.Table);
                    containers.Add(command.Table.EntityContainer);
                }
                AddVertex(command);
                if (command.Kind == UpdateCommandKind.Function)
                {
                    _hasFunctionCommands = true;
                }
            }

            // figure out which foreign keys are interesting in this scope
            InitializeForeignKeyMaps(containers, tables, out _sourceMap, out _targetMap);

            // add edges for each ordering dependency amongst the commands
            AddServerGenDependencies();
            AddForeignKeyDependencies();
            if (_hasFunctionCommands)
            {
                AddModelDependencies();
            }
        }

        private static void InitializeForeignKeyMaps(HashSet<EntityContainer> containers, HashSet<EntitySet> tables, out KeyToListMap<EntitySetBase, ReferentialConstraint> sourceMap, out KeyToListMap<EntitySetBase, ReferentialConstraint> targetMap)
        {
            sourceMap = new KeyToListMap<EntitySetBase, ReferentialConstraint>(EqualityComparer<EntitySetBase>.Default);
            targetMap = new KeyToListMap<EntitySetBase, ReferentialConstraint>(EqualityComparer<EntitySetBase>.Default);

            // Retrieve relationship ends from each container to populate edges in dependency
            // graph
            foreach (EntityContainer container in containers)
            {
                foreach (EntitySetBase extent in container.BaseEntitySets)
                {
                    AssociationSet associationSet = extent as AssociationSet;

                    if (null != associationSet)
                    {
                        AssociationSetEnd source = null;
                        AssociationSetEnd target = null;

                        var ends = associationSet.AssociationSetEnds;

                        if (2 == ends.Count)
                        {
                            // source is equivalent to the "to" end of relationship, target is "from"
                            AssociationType associationType = associationSet.ElementType;
                            bool constraintFound = false;
                            ReferentialConstraint fkConstraint = null;
                            foreach (ReferentialConstraint constraint in associationType.ReferentialConstraints)
                            {
                                if (constraintFound) { Debug.Fail("relationship set should have at most one constraint"); }
                                else { constraintFound = true; }
                                source = associationSet.AssociationSetEnds[constraint.ToRole.Name];
                                target = associationSet.AssociationSetEnds[constraint.FromRole.Name];
                                fkConstraint = constraint;
                            }

                            Debug.Assert(constraintFound && null != target && null != source, "relationship set must have at least one constraint");
                            // only understand binary (foreign key) relationships between entity sets
                            if (null != target && null != source)
                            {
                                if (tables.Contains(target.EntitySet)&&
                                    tables.Contains(source.EntitySet))
                                {
                                    // Remember metadata
                                    sourceMap.Add(source.EntitySet, fkConstraint);
                                    targetMap.Add(target.EntitySet, fkConstraint);
                                }
                            }
                        }
                    }
                }
            }
        }

        // Adds edges to dependency graph for server-generated values.
        //
        // Determines which commands produce identifiers (key parts) and which commands
        // consume them. Producers are potentially edge predecessors and consumers are potentially
        // edge successors. The command objects report the identifiers they produce (OutputIdentifiers)
        // and the identifiers they consume (InputIdentifiers)
        private void AddServerGenDependencies()
        {
            // Identify all "shared" output parameters (e.g., SQL Server identifiers)
            Dictionary<int, UpdateCommand> predecessors = new Dictionary<int, UpdateCommand>();
            foreach (UpdateCommand command in this.Vertices)
            {
                foreach (int output in command.OutputIdentifiers)
                {
                    try
                    {
                        predecessors.Add(output, command);
                    }
                    catch (ArgumentException duplicateKey)
                    {
                        // throw an exception indicating that a key value is generated in two locations
                        // in the store
                        throw EntityUtil.Update(System.Data.Entity.Strings.Update_AmbiguousServerGenIdentifier, duplicateKey, command.GetStateEntries(_translator));
                    }
                }
            }

            // Identify all dependent input parameters
            foreach (UpdateCommand command in this.Vertices)
            {
                foreach (int input in command.InputIdentifiers)
                {
                    UpdateCommand from;
                    if (predecessors.TryGetValue(input, out from))
                    {
                        AddEdge(from, command);
                    }
                }
            }
        }

        // Adds edges to dependency graph based on foreign keys.
        private void AddForeignKeyDependencies()
        {
            KeyToListMap<ForeignKeyValue, UpdateCommand> predecessors = DetermineForeignKeyPredecessors();
            AddForeignKeyEdges(predecessors);
        }

        // Finds all successors to the given predecessors and registers the resulting dependency edges in this
        // graph.
        //
        // - Commands (updates or inserts) inserting FK "sources" (referencing foreign key)
        // - Commands (updates or deletes) deleting FK "targets" (referenced by the foreign key)
        //
        // To avoid violating constraints, FK references must be created before their referees, and
        // cannot be deleted before their references.
        private void AddForeignKeyEdges(KeyToListMap<ForeignKeyValue, UpdateCommand> predecessors)
        {
            foreach (DynamicUpdateCommand command in this.Vertices.OfType<DynamicUpdateCommand>())
            {
                // register all source successors
                if (ModificationOperator.Update == command.Operator ||
                    ModificationOperator.Insert == command.Operator)
                {
                    foreach (ReferentialConstraint fkConstraint in _sourceMap.EnumerateValues(command.Table))
                    {
                        ForeignKeyValue fk;
                        if (ForeignKeyValue.TryCreateSourceKey(fkConstraint, command.CurrentValues, true, out fk))
                        {
                            // if this is an update and the source key is unchanged, there is no 
                            // need to add a dependency (from the perspective of the target, the update
                            // is a no-op)
                            ForeignKeyValue originalFK;
                            if (ModificationOperator.Update != command.Operator ||
                                !ForeignKeyValue.TryCreateSourceKey(fkConstraint, command.OriginalValues, true, out originalFK) ||
                                !_keyComparer.Equals(originalFK, fk))
                            {
                                foreach (UpdateCommand predecessor in predecessors.EnumerateValues(fk))
                                {
                                    // don't add self-edges for FK dependencies, since a single operation
                                    // in the store is atomic
                                    if (predecessor != command)
                                    {
                                        AddEdge(predecessor, command);
                                    }
                                }
                            }
                        }
                    }
                }

                // register all target successors
                if (ModificationOperator.Update == command.Operator ||
                    ModificationOperator.Delete == command.Operator)
                {
                    foreach (ReferentialConstraint fkConstraint in _targetMap.EnumerateValues(command.Table))
                    {
                        ForeignKeyValue fk;
                        if (ForeignKeyValue.TryCreateTargetKey(fkConstraint, command.OriginalValues, false, out fk))
                        {
                            // if this is an update and the target key is unchanged, there is no 
                            // need to add a dependency (from the perspective of the source, the update
                            // is a no-op)
                            ForeignKeyValue currentFK;
                            if (ModificationOperator.Update != command.Operator ||
                                !ForeignKeyValue.TryCreateTargetKey(fkConstraint, command.CurrentValues, false, out currentFK) ||
                                !_keyComparer.Equals(currentFK, fk))
                            {

                                foreach (UpdateCommand predecessor in predecessors.EnumerateValues(fk))
                                {
                                    // don't add self-edges for FK dependencies, since a single operation
                                    // in the store is atomic
                                    if (predecessor != command)
                                    {
                                        AddEdge(predecessor, command);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        // Builds a map from foreign key instances to commands, with an entry for every command that may need to
        // precede some other operation.
        //
        // Predecessor commands must precede other commands using those values. There are two kinds of
        // predecessor:
        //
        // - Commands (updates or inserts) inserting FK "targets" (referenced by the foreign key)
        // - Commands (updates or deletes) deleting FK "sources" (referencing the foreign key)
        //
        // To avoid violating constraints, FK values must be created before they are referenced, and
        // cannot be deleted before their references
        private KeyToListMap<ForeignKeyValue, UpdateCommand> DetermineForeignKeyPredecessors()
        {
            KeyToListMap<ForeignKeyValue, UpdateCommand> predecessors = new KeyToListMap<ForeignKeyValue, UpdateCommand>(
                _keyComparer);

            foreach (DynamicUpdateCommand command in this.Vertices.OfType<DynamicUpdateCommand>())
            {
                if (ModificationOperator.Update == command.Operator ||
                    ModificationOperator.Insert == command.Operator)
                {
                    foreach (ReferentialConstraint fkConstraint in _targetMap.EnumerateValues(command.Table))
                    {
                        ForeignKeyValue fk;
                        if (ForeignKeyValue.TryCreateTargetKey(fkConstraint, command.CurrentValues, true, out fk))
                        {
                            // if this is an update and the target key is unchanged, there is no 
                            // need to add a dependency (from the perspective of the target, the update
                            // is a no-op)
                            ForeignKeyValue originalFK;
                            if (ModificationOperator.Update != command.Operator ||
                                !ForeignKeyValue.TryCreateTargetKey(fkConstraint, command.OriginalValues, true, out originalFK) ||
                                !_keyComparer.Equals(originalFK, fk))
                            {
                                predecessors.Add(fk, command);
                            }
                        }
                    }
                }

                // register all source predecessors
                if (ModificationOperator.Update == command.Operator ||
                    ModificationOperator.Delete == command.Operator)
                {
                    foreach (ReferentialConstraint fkConstraint in _sourceMap.EnumerateValues(command.Table))
                    {
                        ForeignKeyValue fk;
                        if (ForeignKeyValue.TryCreateSourceKey(fkConstraint, command.OriginalValues, false, out fk))
                        {
                            // if this is an update and the source key is unchanged, there is no 
                            // need to add a dependency (from the perspective of the source, the update
                            // is a no-op)
                            ForeignKeyValue currentFK;
                            if (ModificationOperator.Update != command.Operator ||
                                !ForeignKeyValue.TryCreateSourceKey(fkConstraint, command.CurrentValues, false, out currentFK) ||
                                !_keyComparer.Equals(currentFK, fk))
                            {
                                predecessors.Add(fk, command);
                            }
                        }
                    }
                }
            }
            return predecessors;
        }

        /// <summary>
        /// For function commands, we infer constraints based on relationships and entities. For instance,
        /// we always insert an entity before inserting a relationship referencing that entity. When dynamic
        /// and function UpdateCommands are mixed, we also fall back on this same interpretation.
        /// </summary>
        private void AddModelDependencies()
        {
            KeyToListMap<EntityKey, UpdateCommand> addedEntities = new KeyToListMap<EntityKey, UpdateCommand>(EqualityComparer<EntityKey>.Default);
            KeyToListMap<EntityKey, UpdateCommand> deletedEntities = new KeyToListMap<EntityKey, UpdateCommand>(EqualityComparer<EntityKey>.Default);
            KeyToListMap<EntityKey, UpdateCommand> addedRelationships = new KeyToListMap<EntityKey, UpdateCommand>(EqualityComparer<EntityKey>.Default);
            KeyToListMap<EntityKey, UpdateCommand> deletedRelationships = new KeyToListMap<EntityKey, UpdateCommand>(EqualityComparer<EntityKey>.Default);

            foreach (UpdateCommand command in this.Vertices)
            {
                command.GetRequiredAndProducedEntities(_translator, addedEntities, deletedEntities, addedRelationships, deletedRelationships);
            }

            // Add entities before adding dependent relationships
            AddModelDependencies(producedMap: addedEntities, requiredMap: addedRelationships);

            // Delete dependent relationships before deleting entities
            AddModelDependencies(producedMap: deletedRelationships, requiredMap: deletedEntities);
        }

        private void AddModelDependencies(KeyToListMap<EntityKey, UpdateCommand> producedMap, KeyToListMap<EntityKey, UpdateCommand> requiredMap)
        {
            foreach (var keyAndCommands in requiredMap.KeyValuePairs)
            {
                EntityKey key = keyAndCommands.Key;
                List<UpdateCommand> commandsRequiringKey = keyAndCommands.Value;

                foreach (UpdateCommand commandProducingKey in producedMap.EnumerateValues(key))
                {
                    foreach (UpdateCommand commandRequiringKey in commandsRequiringKey)
                    {
                        // command cannot depend on itself and only function commands
                        // need to worry about model dependencies (dynamic commands know about foreign keys)
                        if (!object.ReferenceEquals(commandProducingKey, commandRequiringKey) &&
                            (commandProducingKey.Kind == UpdateCommandKind.Function ||
                             commandRequiringKey.Kind == UpdateCommandKind.Function))
                        {
                            // add a dependency
                            AddEdge(commandProducingKey, commandRequiringKey);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Describes an update command's foreign key (source or target)
        /// </summary>
        private struct ForeignKeyValue
        {
            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="metadata">Sets Metadata</param>
            /// <param name="record">Record containing key value</param>
            /// <param name="isTarget">Indicates whether the source or target end of the constraint
            /// is being pulled</param>
            /// <param name="isInsert">Indicates whether this is an insert dependency or a delete
            /// dependency</param>
            private ForeignKeyValue(ReferentialConstraint metadata, PropagatorResult record,
                bool isTarget, bool isInsert)
            {
                Metadata = metadata;

                // construct key
                IList<EdmProperty> keyProperties = isTarget ? metadata.FromProperties :
                    metadata.ToProperties;
                PropagatorResult[] keyValues = new PropagatorResult[keyProperties.Count];
                bool hasNullMember = false;
                for (int i = 0; i < keyValues.Length; i++)
                {
                    keyValues[i] = record.GetMemberValue(keyProperties[i]);
                    if (keyValues[i].IsNull)
                    {
                        hasNullMember = true;
                        break;
                    }
                }

                if (hasNullMember)
                {
                    // set key to null to indicate that it is not behaving as a key
                    // (in SQL, keys with null parts do not participate in constraints)
                    Key = null;
                }
                else
                {
                    Key = new CompositeKey(keyValues);
                }

                IsInsert = isInsert;
            }

            /// <summary>
            /// Initialize foreign key object for the target of a foreign key.
            /// </summary>
            /// <param name="metadata">Sets Metadata</param>
            /// <param name="record">Record containing key value</param>
            /// <param name="isInsert">Indicates whether the key value is being inserted or deleted</param>
            /// <param name="key">Outputs key object</param>
            /// <returns>true if the record contains key values for this constraint; false otherwise</returns>
            internal static bool TryCreateTargetKey(ReferentialConstraint metadata, PropagatorResult record, bool isInsert, out ForeignKeyValue key)
            {
                key = new ForeignKeyValue(metadata, record, true, isInsert);
                if (null == key.Key)
                {
                    return false;
                }
                return true;
            }

            /// <summary>
            /// Initialize foreign key object for the source of a foreign key.
            /// </summary>
            /// <param name="metadata">Sets Metadata</param>
            /// <param name="record">Record containing key value</param>
            /// <param name="isInsert">Indicates whether the key value is being inserted or deleted</param>
            /// <param name="key">Outputs key object</param>
            /// <returns>true if the record contains key values for this constraint; false otherwise</returns>
            internal static bool TryCreateSourceKey(ReferentialConstraint metadata, PropagatorResult record, bool isInsert, out ForeignKeyValue key)
            {
                key = new ForeignKeyValue(metadata, record, false, isInsert);
                if (null == key.Key)
                {
                    return false;
                }
                return true;
            }

            /// <summary>
            /// Foreign key metadata.
            /// </summary>
            internal readonly ReferentialConstraint Metadata;

            /// <summary>
            /// Foreign key value. 
            /// </summary>
            internal readonly CompositeKey Key;

            /// <summary>
            /// Indicates whether this is an inserted or deleted key value.
            /// </summary>
            internal readonly bool IsInsert;
        }

        /// <summary>
        /// Equality comparer for ForeignKey class.
        /// </summary>
        private class ForeignKeyValueComparer : IEqualityComparer<ForeignKeyValue>
        {
            private readonly IEqualityComparer<CompositeKey> _baseComparer;

            internal ForeignKeyValueComparer(IEqualityComparer<CompositeKey> baseComparer)
            {
                _baseComparer = EntityUtil.CheckArgumentNull(baseComparer, "baseComparer");
            }

            public bool Equals(ForeignKeyValue x, ForeignKeyValue y)
            {
                return x.IsInsert == y.IsInsert && x.Metadata == y.Metadata &&
                    _baseComparer.Equals(x.Key, y.Key);
            }

            public int GetHashCode(ForeignKeyValue obj)
            {
                return _baseComparer.GetHashCode(obj.Key);
            }
        }
    }
}
