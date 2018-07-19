//---------------------------------------------------------------------
// <copyright file="FunctionImportMappingNonComposable.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner willa
//---------------------------------------------------------------------

namespace System.Data.Mapping
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Common.Utils;
    using System.Data.Metadata.Edm;
    using System.Diagnostics;
    using System.Linq;
    using OM = System.Collections.ObjectModel;

    /// <summary>
    /// Represents a mapping from a model function import to a store non-composable function.
    /// </summary>
    internal sealed class FunctionImportMappingNonComposable : FunctionImportMapping
    {
        internal FunctionImportMappingNonComposable(
            EdmFunction functionImport,
            EdmFunction targetFunction,
            List<List<FunctionImportStructuralTypeMapping>> structuralTypeMappingsList,
            ItemCollection itemCollection)
            : base(functionImport, targetFunction)
        {
            EntityUtil.CheckArgumentNull(structuralTypeMappingsList, "structuralTypeMappingsList");
            EntityUtil.CheckArgumentNull(itemCollection, "itemCollection");
            Debug.Assert(!functionImport.IsComposableAttribute, "!functionImport.IsComposableAttribute");
            Debug.Assert(!targetFunction.IsComposableAttribute, "!targetFunction.IsComposableAttribute");

            if (structuralTypeMappingsList.Count == 0)
            {
                this.ResultMappings = new OM.ReadOnlyCollection<FunctionImportStructuralTypeMappingKB>(
                    new FunctionImportStructuralTypeMappingKB[] { 
                        new FunctionImportStructuralTypeMappingKB(new List<FunctionImportStructuralTypeMapping>(), itemCollection) });
                this.noExplicitResultMappings = true;
            }
            else
            {
                Debug.Assert(functionImport.ReturnParameters.Count == structuralTypeMappingsList.Count);
                this.ResultMappings = new OM.ReadOnlyCollection<FunctionImportStructuralTypeMappingKB>(
                    EntityUtil.CheckArgumentNull(structuralTypeMappingsList, "structuralTypeMappingsList")
                        .Select((structuralTypeMappings) => new FunctionImportStructuralTypeMappingKB(
                            EntityUtil.CheckArgumentNull(structuralTypeMappings, "structuralTypeMappings"),
                            itemCollection))
                        .ToArray());
                this.noExplicitResultMappings = false;
            }
        }

        private bool noExplicitResultMappings;

        /// <summary>
        /// Gets function import return type mapping knowledge bases.
        /// </summary>
        internal readonly OM.ReadOnlyCollection<FunctionImportStructuralTypeMappingKB> ResultMappings;

        /// <summary>
        /// If no return mappings were specified in the MSL return an empty return type mapping knowledge base.
        /// Otherwise return the resultSetIndexth return type mapping knowledge base, or throw if resultSetIndex is out of range
        /// </summary>
        internal FunctionImportStructuralTypeMappingKB GetResultMapping(int resultSetIndex)
        {
            Debug.Assert(resultSetIndex >= 0, "resultSetIndex >= 0");
            if (this.noExplicitResultMappings)
            {
                Debug.Assert(this.ResultMappings.Count == 1, "this.ResultMappings.Count == 1");
                return this.ResultMappings[0];
            }
            else
            {
                if (ResultMappings.Count <= resultSetIndex)
                {
                    EntityUtil.ThrowArgumentOutOfRangeException("resultSetIndex");
                }
                return this.ResultMappings[resultSetIndex];
            }
        }

        /// <summary>
        /// Gets the disctriminator columns resultSetIndexth result set, or an empty array if the index is not in range
        /// </summary>
        internal IList<string> GetDiscriminatorColumns(int resultSetIndex)
        {
            FunctionImportStructuralTypeMappingKB resultMapping = this.GetResultMapping(resultSetIndex);
            return resultMapping.DiscriminatorColumns;
        }

        /// <summary>
        /// Given discriminator values (ordinally aligned with DiscriminatorColumns), determines 
        /// the entity type to return. Throws a CommandExecutionException if the type is ambiguous.
        /// </summary>
        internal EntityType Discriminate(object[] discriminatorValues, int resultSetIndex)
        {
            FunctionImportStructuralTypeMappingKB resultMapping = this.GetResultMapping(resultSetIndex);
            Debug.Assert (resultMapping != null);

            // initialize matching types bit map
            BitArray typeCandidates = new BitArray(resultMapping.MappedEntityTypes.Count, true);

            foreach (var typeMapping in resultMapping.NormalizedEntityTypeMappings)
            {
                // check if this type mapping is matched
                bool matches = true;
                var columnConditions = typeMapping.ColumnConditions;
                for (int i = 0; i < columnConditions.Count; i++)
                {
                    if (null != columnConditions[i] && // this discriminator doesn't matter for the given condition
                        !columnConditions[i].ColumnValueMatchesCondition(discriminatorValues[i]))
                    {
                        matches = false;
                        break;
                    }
                }

                if (matches)
                {
                    // if the type condition is met, narrow the set of type candidates
                    typeCandidates = typeCandidates.And(typeMapping.ImpliedEntityTypes);
                }
                else
                {
                    // if the type condition fails, all implied types are eliminated
                    // (the type mapping fragment is a co-implication, so a type is no longer
                    // a candidate if any condition referring to it is false)
                    typeCandidates = typeCandidates.And(typeMapping.ComplementImpliedEntityTypes);
                }
            }

            // find matching type condition
            EntityType entityType = null;
            for (int i = 0; i < typeCandidates.Length; i++)
            {
                if (typeCandidates[i])
                {
                    if (null != entityType)
                    {
                        throw EntityUtil.CommandExecution(System.Data.Entity.Strings.ADP_InvalidDataReaderUnableToDetermineType);
                    }
                    entityType = resultMapping.MappedEntityTypes[i];
                }
            }

            // if there is no match, raise an exception
            if (null == entityType)
            {
                throw EntityUtil.CommandExecution(System.Data.Entity.Strings.ADP_InvalidDataReaderUnableToDetermineType);
            }

            return entityType;
        }

        /// <summary>
        /// Determines the expected shape of store results. We expect a column for every property
        /// of the mapped type (or types) and a column for every discriminator column. We make no
        /// assumptions about the order of columns: the provider is expected to determine appropriate
        /// types by looking at the names of the result columns, not the order of columns, which is
        /// different from the typical handling of row types in the EF.
        /// </summary>
        /// <remarks>
        /// Requires that the given function import mapping refers to a Collection(Entity) or Collection(ComplexType) CSDL
        /// function.
        /// </remarks>
        /// <returns>Row type.</returns>
        internal TypeUsage GetExpectedTargetResultType(MetadataWorkspace workspace, int resultSetIndex)
        {
            FunctionImportStructuralTypeMappingKB resultMapping = this.GetResultMapping(resultSetIndex);
            
            // Collect all columns as name-type pairs.
            Dictionary<string, TypeUsage> columns = new Dictionary<string, TypeUsage>();

            // Figure out which entity types we expect to yield from the function.
            IEnumerable<StructuralType> structuralTypes;
            if (0 == resultMapping.NormalizedEntityTypeMappings.Count)
            {
                // No explicit type mappings; just use the type specified in the ReturnType attribute on the function.
                StructuralType structuralType;
                MetadataHelper.TryGetFunctionImportReturnType<StructuralType>(this.FunctionImport, resultSetIndex, out structuralType);
                Debug.Assert(null != structuralType, "this method must be called only for entity/complextype reader function imports");
                structuralTypes = new StructuralType[] { structuralType };
            }
            else
            {
                // Types are explicitly mapped.
                structuralTypes = resultMapping.MappedEntityTypes.Cast<StructuralType>();
            }

            // Gather columns corresponding to all properties.
            foreach (StructuralType structuralType in structuralTypes)
            {
                foreach (EdmProperty property in TypeHelpers.GetAllStructuralMembers(structuralType))
                {
                    // NOTE: if a complex type is encountered, the column map generator will
                    // throw. For now, we just let them through.

                    // We expect to see each property multiple times, so we use indexer rather than
                    // .Add.
                    columns[property.Name] = property.TypeUsage;
                }
            }

            // Gather discriminator columns.
            foreach (string discriminatorColumn in this.GetDiscriminatorColumns(resultSetIndex))
            {
                if (!columns.ContainsKey(discriminatorColumn))
                {
                    // 






                    TypeUsage type = TypeUsage.CreateStringTypeUsage(workspace.GetModelPrimitiveType(PrimitiveTypeKind.String), true, false);
                    columns.Add(discriminatorColumn, type);
                }
            }

            // Expected type is a collection of rows
            RowType rowType = new RowType(columns.Select(c => new EdmProperty(c.Key, c.Value)));
            TypeUsage result = TypeUsage.Create(new CollectionType(TypeUsage.Create(rowType)));
            return result;
        }
    }
}
