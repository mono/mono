//---------------------------------------------------------------------
// <copyright file="FunctionMappingTranslator.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner [....]
// @backupOwner [....]
//---------------------------------------------------------------------

namespace System.Data.Mapping.Update.Internal
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity;
    using System.Data.Metadata.Edm;
    using System.Diagnostics;
    using System.Linq;

    /// <summary>
    /// Modification function mapping translators are defined per extent (entity set
    /// or association set) and manage the creation of function commands.
    /// </summary>
    internal abstract class ModificationFunctionMappingTranslator
    {
        /// <summary>
        /// Requires: this translator must be registered to handle the entity set
        /// for the given state entry.
        /// 
        /// Translates the given state entry to a command.
        /// </summary>
        /// <param name="translator">Parent update translator (global state for the workload)</param>
        /// <param name="stateEntry">State entry to translate. Must belong to the 
        /// entity/association set handled by this translator</param>
        /// <returns>Command corresponding to the given state entry</returns>
        internal abstract FunctionUpdateCommand Translate(
            UpdateTranslator translator,
            ExtractedStateEntry stateEntry);

        /// <summary>
        /// Initialize a translator for the given entity set mapping.
        /// </summary>
        /// <param name="setMapping">Entity set mapping.</param>
        /// <returns>Translator.</returns>
        internal static ModificationFunctionMappingTranslator CreateEntitySetTranslator(
            StorageEntitySetMapping setMapping)
        {
            return new EntitySetTranslator(setMapping);
        }

        /// <summary>
        /// Initialize a translator for the given association set mapping.
        /// </summary>
        /// <param name="setMapping">Association set mapping.</param>
        /// <returns>Translator.</returns>
        internal static ModificationFunctionMappingTranslator CreateAssociationSetTranslator(
            StorageAssociationSetMapping setMapping)
        {
            return new AssociationSetTranslator(setMapping);
        }

        private sealed class EntitySetTranslator : ModificationFunctionMappingTranslator
        {
            private readonly Dictionary<EntityType, StorageEntityTypeModificationFunctionMapping> m_typeMappings;

            internal EntitySetTranslator(StorageEntitySetMapping setMapping)
            {
                Debug.Assert(null != setMapping && null != setMapping.ModificationFunctionMappings &&
                    0 < setMapping.ModificationFunctionMappings.Count, "set mapping must exist and must specify function mappings");
                m_typeMappings = new Dictionary<EntityType, StorageEntityTypeModificationFunctionMapping>();
                foreach (StorageEntityTypeModificationFunctionMapping typeMapping in setMapping.ModificationFunctionMappings)
                {
                    m_typeMappings.Add(typeMapping.EntityType, typeMapping);
                }
            }

            internal override FunctionUpdateCommand Translate(
                UpdateTranslator translator, 
                ExtractedStateEntry stateEntry)
            {
                var mapping = GetFunctionMapping(stateEntry);
                StorageEntityTypeModificationFunctionMapping typeMapping = mapping.Item1;
                StorageModificationFunctionMapping functionMapping = mapping.Item2;
                EntityKey entityKey = stateEntry.Source.EntityKey;

                var stateEntries = new HashSet<IEntityStateEntry> { stateEntry.Source };

                // gather all referenced association ends
                var collocatedEntries =
                    // find all related entries corresponding to collocated association types
                    from end in functionMapping.CollocatedAssociationSetEnds
                    join candidateEntry in translator.GetRelationships(entityKey)
                    on end.CorrespondingAssociationEndMember.DeclaringType equals candidateEntry.EntitySet.ElementType
                    select Tuple.Create(end.CorrespondingAssociationEndMember, candidateEntry);

                var currentReferenceEnds = new Dictionary<AssociationEndMember, IEntityStateEntry>();
                var originalReferenceEnds = new Dictionary<AssociationEndMember, IEntityStateEntry>();

                foreach (var candidate in collocatedEntries)
                {
                    ProcessReferenceCandidate(entityKey, stateEntries, currentReferenceEnds, originalReferenceEnds, candidate.Item1, candidate.Item2);
                }
                
                // create function object
                FunctionUpdateCommand command;

                // consider the following scenario, we need to loop through all the state entries that is correlated with entity2 and make sure it is not changed.
                // entity1 <-- Independent Association <-- entity2 <-- Fk association <-- entity 3
                //                                           |
                //              entity4 <-- Fk association <--
                if (stateEntries.All(e => e.State == EntityState.Unchanged))
                {
                    // we shouldn't update the entity if it is unchanged, only update when referenced association is changed.
                    // if not, then this will trigger a fake update for principal end as describe in bug 894569.
                    command = null;
                }
                else
                {
                    command = new FunctionUpdateCommand(functionMapping, translator, stateEntries.ToList().AsReadOnly(), stateEntry);

                    // bind all function parameters
                    BindFunctionParameters(translator, stateEntry, functionMapping, command, currentReferenceEnds, originalReferenceEnds);

                    // interpret all result bindings
                    if (null != functionMapping.ResultBindings)
                    {
                        foreach (StorageModificationFunctionResultBinding resultBinding in functionMapping.ResultBindings)
                        {
                            PropagatorResult result = stateEntry.Current.GetMemberValue(resultBinding.Property);
                            command.AddResultColumn(translator, resultBinding.ColumnName, result);
                        }
                    }
                }

                return command;
            }

            private static void ProcessReferenceCandidate(
                EntityKey source, 
                HashSet<IEntityStateEntry> stateEntries, 
                Dictionary<AssociationEndMember, IEntityStateEntry> currentReferenceEnd, 
                Dictionary<AssociationEndMember, IEntityStateEntry> originalReferenceEnd, 
                AssociationEndMember endMember,
                IEntityStateEntry candidateEntry)
            {
                Func<DbDataRecord, int, EntityKey> getEntityKey = (record, ordinal) => (EntityKey)record[ordinal];
                Action<DbDataRecord, Action<IEntityStateEntry>> findMatch = (record, registerTarget) =>
                {
                    // find the end corresponding to the 'to' end
                    int toOrdinal = record.GetOrdinal(endMember.Name);
                    Debug.Assert(-1 != toOrdinal, "to end of relationship doesn't exist in record");

                    // the 'from' end must be the other end
                    int fromOrdinal = 0 == toOrdinal ? 1 : 0;

                    if (getEntityKey(record, fromOrdinal) == source)
                    {
                        stateEntries.Add(candidateEntry);
                        registerTarget(candidateEntry);
                    }
                };

                switch (candidateEntry.State)
                {
                    case EntityState.Unchanged:
                        findMatch(
                            candidateEntry.CurrentValues,
                            (target) =>
                            {
                                currentReferenceEnd.Add(endMember, target);
                                originalReferenceEnd.Add(endMember, target);
                            });
                        break;
                    case EntityState.Added:
                        findMatch(
                            candidateEntry.CurrentValues,
                            (target) => currentReferenceEnd.Add(endMember, target));
                        break;
                    case EntityState.Deleted:
                        findMatch(
                            candidateEntry.OriginalValues,
                            (target) => originalReferenceEnd.Add(endMember, target));
                        break;
                    default:
                        break;
                }
            }

            private Tuple<StorageEntityTypeModificationFunctionMapping, StorageModificationFunctionMapping> GetFunctionMapping(ExtractedStateEntry stateEntry)
            {
                // choose mapping based on type and operation
                StorageModificationFunctionMapping functionMapping;
                EntityType entityType;
                if (null != stateEntry.Current)
                {
                    entityType = (EntityType)stateEntry.Current.StructuralType;
                }
                else
                {
                    entityType = (EntityType)stateEntry.Original.StructuralType;
                }
                StorageEntityTypeModificationFunctionMapping typeMapping = m_typeMappings[entityType];
                switch (stateEntry.State)
                {
                    case EntityState.Added:
                        functionMapping = typeMapping.InsertFunctionMapping;
                        EntityUtil.ValidateNecessaryModificationFunctionMapping(functionMapping, "Insert", stateEntry.Source, "EntityType", entityType.Name);
                        break;
                    case EntityState.Deleted:
                        functionMapping = typeMapping.DeleteFunctionMapping;
                        EntityUtil.ValidateNecessaryModificationFunctionMapping(functionMapping, "Delete", stateEntry.Source, "EntityType", entityType.Name);
                        break;
                    case EntityState.Unchanged:
                    case EntityState.Modified:
                        functionMapping = typeMapping.UpdateFunctionMapping;
                        EntityUtil.ValidateNecessaryModificationFunctionMapping(functionMapping, "Update", stateEntry.Source, "EntityType", entityType.Name);
                        break;
                    default:
                        functionMapping = null;
                        Debug.Fail("unexpected state");
                        break;
                }
                return Tuple.Create(typeMapping, functionMapping);
            }

            // Walks through all parameter bindings in the function mapping and binds the parameters to the
            // requested properties of the given state entry.
            private void BindFunctionParameters(UpdateTranslator translator, ExtractedStateEntry stateEntry, StorageModificationFunctionMapping functionMapping, FunctionUpdateCommand command, Dictionary<AssociationEndMember, IEntityStateEntry> currentReferenceEnds, Dictionary<AssociationEndMember, IEntityStateEntry> originalReferenceEnds)
            {
                // bind all parameters
                foreach (StorageModificationFunctionParameterBinding parameterBinding in functionMapping.ParameterBindings)
                {
                    PropagatorResult result;

                    // extract value
                    if (null != parameterBinding.MemberPath.AssociationSetEnd)
                    {
                        // find the relationship entry corresponding to the navigation
                        AssociationEndMember endMember = parameterBinding.MemberPath.AssociationSetEnd.CorrespondingAssociationEndMember;
                        IEntityStateEntry relationshipEntry;
                        bool hasTarget = parameterBinding.IsCurrent
                            ? currentReferenceEnds.TryGetValue(endMember, out relationshipEntry)
                            : originalReferenceEnds.TryGetValue(endMember, out relationshipEntry);
                        if (!hasTarget)
                        {
                            if (endMember.RelationshipMultiplicity == RelationshipMultiplicity.One)
                            {
                                string entitySetName = stateEntry.Source.EntitySet.Name;
                                string associationSetName = parameterBinding.MemberPath.AssociationSetEnd.ParentAssociationSet.Name;
                                throw EntityUtil.Update(Strings.Update_MissingRequiredRelationshipValue(entitySetName, associationSetName),
                                    null,
                                    command.GetStateEntries(translator));
                            }
                            else
                            {
                                result = PropagatorResult.CreateSimpleValue(PropagatorFlags.NoFlags, null);
                            }
                        }
                        else
                        {
                            // get the actual value
                            PropagatorResult relationshipResult = parameterBinding.IsCurrent ?
                                translator.RecordConverter.ConvertCurrentValuesToPropagatorResult(relationshipEntry, ModifiedPropertiesBehavior.AllModified) :
                                translator.RecordConverter.ConvertOriginalValuesToPropagatorResult(relationshipEntry, ModifiedPropertiesBehavior.AllModified);
                            PropagatorResult endResult = relationshipResult.GetMemberValue(endMember);
                            EdmProperty keyProperty = (EdmProperty)parameterBinding.MemberPath.Members[0]; 
                            result = endResult.GetMemberValue(keyProperty);
                        }
                    }
                    else
                    {
                        // walk through the member path to find the appropriate propagator results
                        result = parameterBinding.IsCurrent ? stateEntry.Current : stateEntry.Original;
                        for (int i = parameterBinding.MemberPath.Members.Count; i > 0;)
                        {
                            --i;
                            EdmMember member = parameterBinding.MemberPath.Members[i];
                            result = result.GetMemberValue(member);
                        }
                    }

                    // create DbParameter
                    command.SetParameterValue(result, parameterBinding, translator);
                }
                // Add rows affected parameter
                command.RegisterRowsAffectedParameter(functionMapping.RowsAffectedParameter);
            }
        }

        private sealed class AssociationSetTranslator : ModificationFunctionMappingTranslator
        {
            // If this value is null, it indicates that the association set is
            // only implicitly mapped as part of an entity set
            private readonly StorageAssociationSetModificationFunctionMapping m_mapping;

            internal AssociationSetTranslator(StorageAssociationSetMapping setMapping)
            {
                if (null != setMapping)
                {
                    m_mapping = setMapping.ModificationFunctionMapping;
                }
            }

            internal override FunctionUpdateCommand Translate(
                UpdateTranslator translator, 
                ExtractedStateEntry stateEntry)
            {
                if (null == m_mapping) { return null; }

                bool isInsert = EntityState.Added == stateEntry.State;

                EntityUtil.ValidateNecessaryModificationFunctionMapping(
                    isInsert ? m_mapping.InsertFunctionMapping : m_mapping.DeleteFunctionMapping, 
                    isInsert ? "Insert" : "Delete", 
                    stateEntry.Source, "AssociationSet", m_mapping.AssociationSet.Name);

                // initialize a new command
                StorageModificationFunctionMapping functionMapping = isInsert ? m_mapping.InsertFunctionMapping : m_mapping.DeleteFunctionMapping;
                FunctionUpdateCommand command = new FunctionUpdateCommand(functionMapping, translator, new [] { stateEntry.Source }.ToList().AsReadOnly(), stateEntry);

                // extract the relationship values from the state entry
                PropagatorResult recordResult;
                if (isInsert) 
                {
                    recordResult = stateEntry.Current;
                }
                else 
                { 
                    recordResult = stateEntry.Original; 
                }

                // bind parameters
                foreach (StorageModificationFunctionParameterBinding parameterBinding in functionMapping.ParameterBindings)
                {
                    // extract the relationship information
                    Debug.Assert(2 == parameterBinding.MemberPath.Members.Count, "relationship parameter binding member " +
                        "path should include the relationship end and key property only");

                    EdmProperty keyProperty = (EdmProperty)parameterBinding.MemberPath.Members[0];
                    AssociationEndMember endMember = (AssociationEndMember)parameterBinding.MemberPath.Members[1];

                    // get the end member
                    PropagatorResult endResult = recordResult.GetMemberValue(endMember);
                    PropagatorResult keyResult = endResult.GetMemberValue(keyProperty);

                    command.SetParameterValue(keyResult, parameterBinding, translator);
                }
                // add rows affected output parameter
                command.RegisterRowsAffectedParameter(functionMapping.RowsAffectedParameter);

                return command;
            }
        }
    }
}
