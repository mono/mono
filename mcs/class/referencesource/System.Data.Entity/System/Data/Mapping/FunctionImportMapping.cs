//---------------------------------------------------------------------
// <copyright file="FunctionImportMapping.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner willa
//---------------------------------------------------------------------

namespace System.Data.Mapping
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Common.Utils;
    using System.Data.Common.Utils.Boolean;
    using System.Data.Entity;
    using System.Data.Metadata.Edm;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Xml;
    using System.Xml.XPath;
    using OM = System.Collections.ObjectModel;

    /// <summary>
    /// Represents a mapping from a model function import to a store composable or non-composable function.
    /// </summary>
    internal abstract class FunctionImportMapping
    {
        internal FunctionImportMapping(EdmFunction functionImport, EdmFunction targetFunction)
        {
            this.FunctionImport = EntityUtil.CheckArgumentNull(functionImport, "functionImport");
            this.TargetFunction = EntityUtil.CheckArgumentNull(targetFunction, "targetFunction");
        }

        /// <summary>
        /// Gets model function (or source of the mapping)
        /// </summary>
        internal readonly EdmFunction FunctionImport;

        /// <summary>
        /// Gets store function (or target of the mapping)
        /// </summary>
        internal readonly EdmFunction TargetFunction;
    }

    internal sealed class FunctionImportStructuralTypeMappingKB
    {
        internal FunctionImportStructuralTypeMappingKB(
            IEnumerable<FunctionImportStructuralTypeMapping> structuralTypeMappings,
            ItemCollection itemCollection)
        {
            EntityUtil.CheckArgumentNull(structuralTypeMappings, "structuralTypeMappings");
            m_itemCollection = EntityUtil.CheckArgumentNull(itemCollection, "itemCollection");

            // If no specific type mapping.
            if (structuralTypeMappings.Count() == 0)
            {
                // Initialize with defaults.
                this.ReturnTypeColumnsRenameMapping = new Dictionary<string, FunctionImportReturnTypeStructuralTypeColumnRenameMapping>();
                this.NormalizedEntityTypeMappings = new OM.ReadOnlyCollection<FunctionImportNormalizedEntityTypeMapping>(new List<FunctionImportNormalizedEntityTypeMapping>());
                this.DiscriminatorColumns = new OM.ReadOnlyCollection<string>(new List<string>());
                this.MappedEntityTypes = new OM.ReadOnlyCollection<EntityType>(new List<EntityType>());
                return;
            }

            IEnumerable<FunctionImportEntityTypeMapping> entityTypeMappings = structuralTypeMappings.OfType<FunctionImportEntityTypeMapping>();

            // FunctionImportEntityTypeMapping
            if (null != entityTypeMappings && null != entityTypeMappings.FirstOrDefault<FunctionImportEntityTypeMapping>())
            {
                var isOfTypeEntityTypeColumnsRenameMapping = new Dictionary<EntityType, OM.Collection<FunctionImportReturnTypePropertyMapping>>();
                var entityTypeColumnsRenameMapping = new Dictionary<EntityType, OM.Collection<FunctionImportReturnTypePropertyMapping>>();
                var normalizedEntityTypeMappings = new List<FunctionImportNormalizedEntityTypeMapping>();

                // Collect all mapped entity types.
                this.MappedEntityTypes = entityTypeMappings
                    .SelectMany(mapping => mapping.GetMappedEntityTypes(m_itemCollection))
                    .Distinct()
                    .ToList()
                    .AsReadOnly();

                // Collect all discriminator columns.
                this.DiscriminatorColumns = entityTypeMappings
                    .SelectMany(mapping => mapping.GetDiscriminatorColumns())
                    .Distinct()
                    .ToList()
                    .AsReadOnly();

                m_entityTypeLineInfos = new KeyToListMap<EntityType, LineInfo>(EqualityComparer<EntityType>.Default);
                m_isTypeOfLineInfos = new KeyToListMap<EntityType, LineInfo>(EqualityComparer<EntityType>.Default);

                foreach (var entityTypeMapping in entityTypeMappings)
                {
                    // Remember LineInfos for error reporting.
                    foreach (var entityType in entityTypeMapping.EntityTypes)
                    {
                        m_entityTypeLineInfos.Add(entityType, entityTypeMapping.LineInfo);
                    }
                    foreach (var isTypeOf in entityTypeMapping.IsOfTypeEntityTypes)
                    {
                        m_isTypeOfLineInfos.Add(isTypeOf, entityTypeMapping.LineInfo);
                    }

                    // Create map from column name to condition.
                    var columnMap = entityTypeMapping.Conditions.ToDictionary(
                        condition => condition.ColumnName,
                        condition => condition);

                    // Align conditions with discriminator columns.
                    var columnMappings = new List<FunctionImportEntityTypeMappingCondition>(this.DiscriminatorColumns.Count);
                    for (int i = 0; i < this.DiscriminatorColumns.Count; i++)
                    {
                        string discriminatorColumn = this.DiscriminatorColumns[i];
                        FunctionImportEntityTypeMappingCondition mappingCondition;
                        if (columnMap.TryGetValue(discriminatorColumn, out mappingCondition))
                        {
                            columnMappings.Add(mappingCondition);
                        }
                        else
                        {
                            // Null indicates the value for this discriminator doesn't matter.
                            columnMappings.Add(null);
                        }
                    }

                    // Create bit map for implied entity types.
                    bool[] impliedEntityTypesBitMap = new bool[this.MappedEntityTypes.Count];
                    var impliedEntityTypesSet = new Set<EntityType>(entityTypeMapping.GetMappedEntityTypes(m_itemCollection));
                    for (int i = 0; i < this.MappedEntityTypes.Count; i++)
                    {
                        impliedEntityTypesBitMap[i] = impliedEntityTypesSet.Contains(this.MappedEntityTypes[i]);
                    }

                    // Construct normalized mapping.
                    normalizedEntityTypeMappings.Add(new FunctionImportNormalizedEntityTypeMapping(this, columnMappings, new BitArray(impliedEntityTypesBitMap)));

                    // Construct the rename mappings by adding isTypeOf types and specific entity types to the corresponding lists.
                    foreach (var isOfType in entityTypeMapping.IsOfTypeEntityTypes)
                    {
                        if (!isOfTypeEntityTypeColumnsRenameMapping.Keys.Contains(isOfType))
                        {
                            isOfTypeEntityTypeColumnsRenameMapping.Add(isOfType, new OM.Collection<FunctionImportReturnTypePropertyMapping>());
                        }
                        foreach (var rename in entityTypeMapping.ColumnsRenameList)
                        {
                            isOfTypeEntityTypeColumnsRenameMapping[isOfType].Add(rename);
                        }
                    }
                    foreach (var entityType in entityTypeMapping.EntityTypes)
                    {
                        if (!entityTypeColumnsRenameMapping.Keys.Contains(entityType))
                        {
                            entityTypeColumnsRenameMapping.Add(entityType, new OM.Collection<FunctionImportReturnTypePropertyMapping>());
                        }
                        foreach (var rename in entityTypeMapping.ColumnsRenameList)
                        {
                            entityTypeColumnsRenameMapping[entityType].Add(rename);
                        }
                    }
                }

                this.ReturnTypeColumnsRenameMapping = new FunctionImportReturnTypeEntityTypeColumnsRenameBuilder(isOfTypeEntityTypeColumnsRenameMapping,
                                                                                                                 entityTypeColumnsRenameMapping)
                                                      .ColumnRenameMapping;

                this.NormalizedEntityTypeMappings = new OM.ReadOnlyCollection<FunctionImportNormalizedEntityTypeMapping>(
                    normalizedEntityTypeMappings);
            }
            else
            {
                // FunctionImportComplexTypeMapping
                Debug.Assert(structuralTypeMappings.First() is FunctionImportComplexTypeMapping, "only two types can have renames, complexType and entityType");
                IEnumerable<FunctionImportComplexTypeMapping> complexTypeMappings = structuralTypeMappings.Cast<FunctionImportComplexTypeMapping>();

                Debug.Assert(complexTypeMappings.Count() == 1, "how come there are more than 1, complex type cannot derive from other complex type");

                this.ReturnTypeColumnsRenameMapping = new Dictionary<string, FunctionImportReturnTypeStructuralTypeColumnRenameMapping>();
                foreach (var rename in complexTypeMappings.First().ColumnsRenameList)
                {
                    FunctionImportReturnTypeStructuralTypeColumnRenameMapping columnRenameMapping = new FunctionImportReturnTypeStructuralTypeColumnRenameMapping(rename.CMember);
                    columnRenameMapping.AddRename(new FunctionImportReturnTypeStructuralTypeColumn(
                            rename.SColumn,
                            complexTypeMappings.First().ReturnType,
                            false,
                            rename.LineInfo));
                    this.ReturnTypeColumnsRenameMapping.Add(rename.CMember, columnRenameMapping);
                }

                // Initialize the entity mapping data as empty.
                this.NormalizedEntityTypeMappings = new OM.ReadOnlyCollection<FunctionImportNormalizedEntityTypeMapping>(new List<FunctionImportNormalizedEntityTypeMapping>());
                this.DiscriminatorColumns = new OM.ReadOnlyCollection<string>(new List<string>() { });
                this.MappedEntityTypes = new OM.ReadOnlyCollection<EntityType>(new List<EntityType>() { });
            }
        }

        private readonly ItemCollection m_itemCollection;
        private readonly KeyToListMap<EntityType, LineInfo> m_entityTypeLineInfos;
        private readonly KeyToListMap<EntityType, LineInfo> m_isTypeOfLineInfos;

        /// <summary>
        /// Gets all types in scope for this mapping.
        /// </summary>
        internal readonly OM.ReadOnlyCollection<EntityType> MappedEntityTypes;

        /// <summary>
        /// Gets a list of all discriminator columns used in this mapping.
        /// </summary>
        internal readonly OM.ReadOnlyCollection<string> DiscriminatorColumns;

        /// <summary>
        /// Gets normalized representation of all EntityTypeMapping fragments for this
        /// function import mapping.
        /// </summary>
        internal readonly OM.ReadOnlyCollection<FunctionImportNormalizedEntityTypeMapping> NormalizedEntityTypeMappings;

        /// <summary>
        /// Get the columns rename mapping for return type, the first string is the member name
        /// the second one is column names for different types that mentioned in the mapping.
        /// </summary>
        internal readonly Dictionary<string, FunctionImportReturnTypeStructuralTypeColumnRenameMapping> ReturnTypeColumnsRenameMapping;

        internal bool ValidateTypeConditions(bool validateAmbiguity, IList<EdmSchemaError> errors, string sourceLocation)
        {
            // Verify that all types can be produced
            KeyToListMap<EntityType, LineInfo> unreachableEntityTypes;
            KeyToListMap<EntityType, LineInfo> unreachableIsTypeOfs;
            GetUnreachableTypes(validateAmbiguity, out unreachableEntityTypes, out unreachableIsTypeOfs);

            bool valid = true;
            foreach (var unreachableEntityType in unreachableEntityTypes.KeyValuePairs)
            {
                var lineInfo = unreachableEntityType.Value.First();
                string lines = StringUtil.ToCommaSeparatedString(unreachableEntityType.Value.Select(li => li.LineNumber));
                EdmSchemaError error = new EdmSchemaError(
                    Strings.Mapping_FunctionImport_UnreachableType(unreachableEntityType.Key.FullName, lines),
                    (int)StorageMappingErrorCode.MappingFunctionImportAmbiguousTypeConditions,
                    EdmSchemaErrorSeverity.Error,
                    sourceLocation,
                    lineInfo.LineNumber, 
                    lineInfo.LinePosition);
                errors.Add(error);
                valid = false;
            }
            foreach (var unreachableIsTypeOf in unreachableIsTypeOfs.KeyValuePairs)
            {
                var lineInfo = unreachableIsTypeOf.Value.First();
                string lines = StringUtil.ToCommaSeparatedString(unreachableIsTypeOf.Value.Select(li => li.LineNumber));
                string isTypeOfDescription = StorageMslConstructs.IsTypeOf + unreachableIsTypeOf.Key.FullName + StorageMslConstructs.IsTypeOfTerminal;
                EdmSchemaError error = new EdmSchemaError(
                    Strings.Mapping_FunctionImport_UnreachableIsTypeOf(isTypeOfDescription, lines),
                    (int)StorageMappingErrorCode.MappingFunctionImportAmbiguousTypeConditions,
                    EdmSchemaErrorSeverity.Error,
                    sourceLocation,
                    lineInfo.LineNumber,
                    lineInfo.LinePosition);
                errors.Add(error);
                valid = false;
            }

            return valid;
        }

        /// <summary>
        /// Determines which explicitly mapped types in the function import mapping cannot be generated.
        /// For IsTypeOf declarations, reports if no type in hierarchy can be produced.
        /// 
        /// Works by:
        /// 
        /// - Converting type mapping conditions into vertices
        /// - Checking that some assignment satisfies 
        /// </summary>
        private void GetUnreachableTypes(
            bool validateAmbiguity,
            out KeyToListMap<EntityType, LineInfo> unreachableEntityTypes,
            out KeyToListMap<EntityType, LineInfo> unreachableIsTypeOfs)
        {
            // Contains, for each DiscriminatorColumn, a domain variable where the domain values are
            // integers representing the ordinal within discriminatorDomains.
            DomainVariable<string, ValueCondition>[] variables = ConstructDomainVariables();

            // Convert type mapping conditions to decision diagram vertices.
            var converter = new DomainConstraintConversionContext<string, ValueCondition>();
            Vertex[] mappingConditions = ConvertMappingConditionsToVertices(converter, variables);

            // Find reachable types.
            Set<EntityType> reachableTypes = validateAmbiguity ?
                FindUnambiguouslyReachableTypes(converter, mappingConditions) :
                FindReachableTypes(converter, mappingConditions);

            CollectUnreachableTypes(reachableTypes, out unreachableEntityTypes, out unreachableIsTypeOfs);
        }

        private DomainVariable<string, ValueCondition>[] ConstructDomainVariables()
        {
            // Determine domain for each discriminator column, including "other" and "null" placeholders.
            var discriminatorDomains = new Set<ValueCondition>[this.DiscriminatorColumns.Count];
            for (int i = 0; i < discriminatorDomains.Length; i++)
            {
                discriminatorDomains[i] = new Set<ValueCondition>();
                discriminatorDomains[i].Add(ValueCondition.IsOther);
                discriminatorDomains[i].Add(ValueCondition.IsNull);
            }

            // Collect all domain values.
            foreach (var typeMapping in this.NormalizedEntityTypeMappings)
            {
                for (int i = 0; i < this.DiscriminatorColumns.Count; i++)
                {
                    var discriminatorValue = typeMapping.ColumnConditions[i];
                    if (null != discriminatorValue &&
                        !discriminatorValue.ConditionValue.IsNotNullCondition) // NotNull is a special range (everything but IsNull)
                    {
                        discriminatorDomains[i].Add(discriminatorValue.ConditionValue);
                    }
                }
            }

            var discriminatorVariables = new DomainVariable<string, ValueCondition>[discriminatorDomains.Length];
            for (int i = 0; i < discriminatorVariables.Length; i++)
            {
                // domain variable is identified by the column name and takes all collected domain values
                discriminatorVariables[i] = new DomainVariable<string, ValueCondition>(
                    this.DiscriminatorColumns[i], discriminatorDomains[i].MakeReadOnly());
            }

            return discriminatorVariables;
        }

        private Vertex[] ConvertMappingConditionsToVertices(
            ConversionContext<DomainConstraint<string, ValueCondition>> converter,
            DomainVariable<string, ValueCondition>[] variables)
        {
            Vertex[] conditions = new Vertex[this.NormalizedEntityTypeMappings.Count];
            for (int i = 0; i < conditions.Length; i++)
            {
                var typeMapping = this.NormalizedEntityTypeMappings[i];

                // create conjunction representing the condition
                Vertex condition = Vertex.One;
                for (int j = 0; j < this.DiscriminatorColumns.Count; j++)
                {
                    var columnCondition = typeMapping.ColumnConditions[j];
                    if (null != columnCondition)
                    {
                        var conditionValue = columnCondition.ConditionValue;
                        if (conditionValue.IsNotNullCondition)
                        {
                            // the 'not null' condition is not actually part of the domain (since it
                            // covers other elements), so create a Not(value in {null}) condition
                            var isNull = new TermExpr<DomainConstraint<string, ValueCondition>>(
                                new DomainConstraint<string, ValueCondition>(variables[j], ValueCondition.IsNull));
                            Vertex isNullVertex = converter.TranslateTermToVertex(isNull);
                            condition = converter.Solver.And(condition, converter.Solver.Not(isNullVertex));
                        }
                        else
                        {
                            var hasValue = new TermExpr<DomainConstraint<string, ValueCondition>>(
                                new DomainConstraint<string, ValueCondition>(variables[j], conditionValue));
                            condition = converter.Solver.And(condition, converter.TranslateTermToVertex(hasValue));
                        }
                    }
                }
                conditions[i] = condition;
            }
            return conditions;
        }

        /// <summary>
        /// Determines which types are produced by this mapping.
        /// </summary>
        private Set<EntityType> FindReachableTypes(DomainConstraintConversionContext<string, ValueCondition> converter, Vertex[] mappingConditions)
        {
            // For each entity type, create a candidate function that evaluates to true given
            // discriminator assignments iff. all of that type's conditions evaluate to true
            // and its negative conditions evaluate to false.
            Vertex[] candidateFunctions = new Vertex[this.MappedEntityTypes.Count];
            for (int i = 0; i < candidateFunctions.Length; i++)
            {
                // Seed the candidate function conjunction with 'true'.
                Vertex candidateFunction = Vertex.One;
                for (int j = 0; j < this.NormalizedEntityTypeMappings.Count; j++)
                {
                    var entityTypeMapping = this.NormalizedEntityTypeMappings[j];

                    // Determine if this mapping is a positive or negative case for the current type.
                    if (entityTypeMapping.ImpliedEntityTypes[i])
                    {
                        candidateFunction = converter.Solver.And(candidateFunction, mappingConditions[j]);
                    }
                    else
                    {
                        candidateFunction = converter.Solver.And(candidateFunction, converter.Solver.Not(mappingConditions[j]));
                    }
                }
                candidateFunctions[i] = candidateFunction;
            }

            // Make sure that for each type there is an assignment that resolves to only that type.
            var reachableTypes = new Set<EntityType>();
            for (int i = 0; i < candidateFunctions.Length; i++)
            {
                // Create a function that evaluates to true iff. the current candidate function is true
                // and every other candidate function is false.
                Vertex isExactlyThisTypeCondition = converter.Solver.And(
                    candidateFunctions.Select((typeCondition, ordinal) => ordinal == i ?
                        typeCondition :
                        converter.Solver.Not(typeCondition)));

                // If the above conjunction is satisfiable, it means some row configuration exists producing the type.
                if (!isExactlyThisTypeCondition.IsZero())
                {
                    reachableTypes.Add(this.MappedEntityTypes[i]);
                }
            }

            return reachableTypes;
        }

        /// <summary>
        /// Determines which types are produced by this mapping.
        /// </summary>
        private Set<EntityType> FindUnambiguouslyReachableTypes(DomainConstraintConversionContext<string, ValueCondition> converter, Vertex[] mappingConditions)
        {
            // For each entity type, create a candidate function that evaluates to true given
            // discriminator assignments iff. all of that type's conditions evaluate to true.
            Vertex[] candidateFunctions = new Vertex[this.MappedEntityTypes.Count];
            for (int i = 0; i < candidateFunctions.Length; i++)
            {
                // Seed the candidate function conjunction with 'true'.
                Vertex candidateFunction = Vertex.One;
                for (int j = 0; j < this.NormalizedEntityTypeMappings.Count; j++)
                {
                    var entityTypeMapping = this.NormalizedEntityTypeMappings[j];

                    // Determine if this mapping is a positive or negative case for the current type.
                    if (entityTypeMapping.ImpliedEntityTypes[i])
                    {
                        candidateFunction = converter.Solver.And(candidateFunction, mappingConditions[j]);
                    }
                }
                candidateFunctions[i] = candidateFunction;
            }

            // Make sure that for each type with satisfiable candidateFunction all assignments for the type resolve to only that type.
            var unambigouslyReachableMap = new BitArray(candidateFunctions.Length, true);
            for (int i = 0; i < candidateFunctions.Length; ++i)
            {
                if (candidateFunctions[i].IsZero())
                {
                    // The i-th type is unreachable regardless of other types.
                    unambigouslyReachableMap[i] = false;
                }
                else
                {
                    for (int j = i + 1; j < candidateFunctions.Length; ++j)
                    {
                        if (!converter.Solver.And(candidateFunctions[i], candidateFunctions[j]).IsZero())
                        {
                            // The i-th and j-th types have common assignments, hence they aren't unambiguously reachable.
                            unambigouslyReachableMap[i] = false;
                            unambigouslyReachableMap[j] = false;
                        }
                    }
                }
            }
            var reachableTypes = new Set<EntityType>();
            for (int i = 0; i < candidateFunctions.Length; ++i)
            {
                if (unambigouslyReachableMap[i])
                {
                    reachableTypes.Add(this.MappedEntityTypes[i]);
                }
            }

            return reachableTypes;
        }

        private void CollectUnreachableTypes(Set<EntityType> reachableTypes, out KeyToListMap<EntityType, LineInfo> entityTypes, out KeyToListMap<EntityType, LineInfo> isTypeOfEntityTypes)
        {
            // Collect line infos for types in violation
            entityTypes = new KeyToListMap<EntityType, LineInfo>(EqualityComparer<EntityType>.Default);
            isTypeOfEntityTypes = new KeyToListMap<EntityType, LineInfo>(EqualityComparer<EntityType>.Default);

            if (reachableTypes.Count == this.MappedEntityTypes.Count)
            {
                // All types are reachable; nothing to check
                return;
            }

            // Find IsTypeOf mappings where no type in hierarchy can generate a row
            foreach (var isTypeOf in m_isTypeOfLineInfos.Keys)
            {
                if (!MetadataHelper.GetTypeAndSubtypesOf(isTypeOf, m_itemCollection, false)
                    .Cast<EntityType>()
                    .Intersect(reachableTypes)
                    .Any())
                {
                    // no type in the hierarchy is reachable...
                    isTypeOfEntityTypes.AddRange(isTypeOf, m_isTypeOfLineInfos.EnumerateValues(isTypeOf));
                }
            }

            // Find explicit types not generating a value
            foreach (var entityType in m_entityTypeLineInfos.Keys)
            {
                if (!reachableTypes.Contains(entityType))
                {
                    entityTypes.AddRange(entityType, m_entityTypeLineInfos.EnumerateValues(entityType));
                }
            }
        }
    }

    internal sealed class FunctionImportNormalizedEntityTypeMapping
    {
        internal FunctionImportNormalizedEntityTypeMapping(FunctionImportStructuralTypeMappingKB parent, 
            List<FunctionImportEntityTypeMappingCondition> columnConditions, BitArray impliedEntityTypes)
        {
            // validate arguments
            EntityUtil.CheckArgumentNull(parent, "parent");
            EntityUtil.CheckArgumentNull(columnConditions, "discriminatorValues");
            EntityUtil.CheckArgumentNull(impliedEntityTypes, "impliedEntityTypes");

            Debug.Assert(columnConditions.Count == parent.DiscriminatorColumns.Count,
                "discriminator values must be ordinally aligned with discriminator columns");
            Debug.Assert(impliedEntityTypes.Count == parent.MappedEntityTypes.Count,
                "implied entity types must be ordinally aligned with mapped entity types");

            this.ColumnConditions = new OM.ReadOnlyCollection<FunctionImportEntityTypeMappingCondition>(columnConditions.ToList());
            this.ImpliedEntityTypes = impliedEntityTypes;
            this.ComplementImpliedEntityTypes = (new BitArray(this.ImpliedEntityTypes)).Not();
        }

        /// <summary>
        /// Gets discriminator values aligned with DiscriminatorColumns of the parent FunctionImportMapping.
        /// A null ValueCondition indicates 'anything goes'.
        /// </summary>
        internal readonly OM.ReadOnlyCollection<FunctionImportEntityTypeMappingCondition> ColumnConditions;

        /// <summary>
        /// Gets bit array with 'true' indicating the corresponding MappedEntityType of the parent
        /// FunctionImportMapping is implied by this fragment.
        /// </summary>
        internal readonly BitArray ImpliedEntityTypes;

        /// <summary>
        /// Gets the complement of the ImpliedEntityTypes BitArray.
        /// </summary>
        internal readonly BitArray ComplementImpliedEntityTypes;

        public override string ToString()
        {
            return String.Format(CultureInfo.InvariantCulture, "Values={0}, Types={1}",
                StringUtil.ToCommaSeparatedString(this.ColumnConditions), StringUtil.ToCommaSeparatedString(this.ImpliedEntityTypes));
        }
    }

    internal abstract class FunctionImportEntityTypeMappingCondition
    {
        protected FunctionImportEntityTypeMappingCondition(string columnName, LineInfo lineInfo)
        {
            this.ColumnName = EntityUtil.CheckArgumentNull(columnName, "columnName");
            this.LineInfo = lineInfo;
        }

        internal readonly string ColumnName;
        internal readonly LineInfo LineInfo;

        internal abstract ValueCondition ConditionValue { get; }

        internal abstract bool ColumnValueMatchesCondition(object columnValue);

        public override string ToString()
        {
            return this.ConditionValue.ToString();
        }
    }

    internal sealed class FunctionImportEntityTypeMappingConditionValue : FunctionImportEntityTypeMappingCondition
    {
        internal FunctionImportEntityTypeMappingConditionValue(string columnName, XPathNavigator columnValue, LineInfo lineInfo)
            : base(columnName, lineInfo)
        {
            this._xPathValue = EntityUtil.CheckArgumentNull(columnValue, "columnValue");
            this._convertedValues = new Memoizer<Type, object>(this.GetConditionValue, null);
        }

        private readonly XPathNavigator _xPathValue;
        private readonly Memoizer<Type, object> _convertedValues;

        internal override ValueCondition ConditionValue
        {
            get { return new ValueCondition(_xPathValue.Value); }
        }

        internal override bool ColumnValueMatchesCondition(object columnValue)
        {
            if (null == columnValue || Convert.IsDBNull(columnValue))
            {
                // only FunctionImportEntityTypeMappingConditionIsNull can match a null
                // column value
                return false;
            }

            Type columnValueType = columnValue.GetType();

            // check if we've interpreted this column type yet
            object conditionValue = _convertedValues.Evaluate(columnValueType);
            return ByValueEqualityComparer.Default.Equals(columnValue, conditionValue);
        }

        private object GetConditionValue(Type columnValueType)
        {
            return GetConditionValue(
                columnValueType,
                handleTypeNotComparable: () =>
                {
                    throw EntityUtil.CommandExecution(Strings.Mapping_FunctionImport_UnsupportedType(this.ColumnName, columnValueType.FullName));
                },
                handleInvalidConditionValue: () =>
                {
                    throw EntityUtil.CommandExecution(Strings.Mapping_FunctionImport_ConditionValueTypeMismatch(StorageMslConstructs.FunctionImportMappingElement, this.ColumnName, columnValueType.FullName));
                });
        }

        internal object GetConditionValue(Type columnValueType, Action handleTypeNotComparable, Action handleInvalidConditionValue)
        {
            // Check that the type is supported and comparable.
            PrimitiveType primitiveType;
            if (!ClrProviderManifest.Instance.TryGetPrimitiveType(columnValueType, out primitiveType) ||
                !StorageMappingItemLoader.IsTypeSupportedForCondition(primitiveType.PrimitiveTypeKind))
            {
                handleTypeNotComparable();
                return null;
            }

            try
            {
                return _xPathValue.ValueAs(columnValueType);
            }
            catch (FormatException)
            {
                handleInvalidConditionValue();
                return null;
            }
        }
    }

    internal sealed class FunctionImportEntityTypeMappingConditionIsNull : FunctionImportEntityTypeMappingCondition
    {
        internal FunctionImportEntityTypeMappingConditionIsNull(string columnName, bool isNull, LineInfo lineInfo)
            : base(columnName, lineInfo)
        {
            this.IsNull = isNull;
        }

        internal readonly bool IsNull;

        internal override ValueCondition ConditionValue
        {
            get { return IsNull ? ValueCondition.IsNull : ValueCondition.IsNotNull; }
        }

        internal override bool ColumnValueMatchesCondition(object columnValue)
        {
            bool valueIsNull = null == columnValue || Convert.IsDBNull(columnValue);
            return valueIsNull == this.IsNull;
        }
    }

    /// <summary>
    /// Represents a simple value condition of the form (value IS NULL), (value IS NOT NULL)
    /// or (value EQ X). Supports IEquatable(Of ValueCondition) so that equivalent conditions
    /// can be identified.
    /// </summary>
    internal class ValueCondition : IEquatable<ValueCondition>
    {
        internal readonly string Description;
        internal readonly bool IsSentinel;

        internal const string IsNullDescription = "NULL";
        internal const string IsNotNullDescription = "NOT NULL";
        internal const string IsOtherDescription = "OTHER";

        internal readonly static ValueCondition IsNull = new ValueCondition(IsNullDescription, true);
        internal readonly static ValueCondition IsNotNull = new ValueCondition(IsNotNullDescription, true);
        internal readonly static ValueCondition IsOther = new ValueCondition(IsOtherDescription, true);

        private ValueCondition(string description, bool isSentinel)
        {
            Description = description;
            IsSentinel = isSentinel;
        }

        internal ValueCondition(string description)
            : this(description, false)
        {
        }

        internal bool IsNotNullCondition { get { return object.ReferenceEquals(this, IsNotNull); } }

        public bool Equals(ValueCondition other)
        {
            return other.IsSentinel == this.IsSentinel &&
                other.Description == this.Description;
        }

        public override int GetHashCode()
        {
            return Description.GetHashCode();
        }

        public override string ToString()
        {
            return this.Description;
        }
    }

    internal sealed class LineInfo: IXmlLineInfo
    {
        private readonly bool m_hasLineInfo;
        private readonly int m_lineNumber;
        private readonly int m_linePosition;

        internal LineInfo(XPathNavigator nav)
            : this((IXmlLineInfo)nav)
        { }

        internal LineInfo(IXmlLineInfo lineInfo)
        {
            m_hasLineInfo = lineInfo.HasLineInfo();
            m_lineNumber = lineInfo.LineNumber;
            m_linePosition = lineInfo.LinePosition;
        }

        internal static readonly LineInfo Empty = new LineInfo();
        private LineInfo()
        {
            m_hasLineInfo = false;
            m_lineNumber = default(int);
            m_linePosition = default(int);
        }

        public int LineNumber
        { get { return m_lineNumber; } }

        public int LinePosition
        { get { return m_linePosition; } }

        public bool HasLineInfo()
        {
            return m_hasLineInfo;
        }
    }
}
