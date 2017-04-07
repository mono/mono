//---------------------------------------------------------------------
// <copyright file="ViewValidator.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

namespace System.Data.Mapping
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Common.CommandTrees;
    using System.Data.Common.Utils;
    using System.Data.Metadata.Edm;
    using System.Diagnostics;
    using System.Linq;

    /// <summary>
    /// Verifies that only legal expressions exist in a user-defined query mapping view.
    /// </summary>
    internal static class ViewValidator
    {
        /// <summary>
        /// Determines whether the given view is valid.
        /// </summary>
        /// <param name="view">Query view to validate.</param>
        /// <param name="storeItemCollection">Store item collection.</param>
        /// <param name="setMapping">Mapping in which view is declared.</param>
        /// <returns>Errors in view definition.</returns>
        internal static IEnumerable<EdmSchemaError> ValidateQueryView(DbQueryCommandTree view, StorageSetMapping setMapping, EntityTypeBase elementType, bool includeSubtypes)
        {
            ViewExpressionValidator validator = new ViewExpressionValidator(setMapping, elementType, includeSubtypes);
            validator.VisitExpression(view.Query);
            if (validator.Errors.Count() == 0)
            {
                //For AssociationSet views, we have to check for a specific pattern of errors where
                //the Ref expression passed into the construcor might use an EntitySet that is different from
                //the EntitySet defined in the CSDL.
                if (setMapping.Set.BuiltInTypeKind == BuiltInTypeKind.AssociationSet)
                {
                    AssociationSetViewValidator refValidator = new AssociationSetViewValidator(setMapping);
                    refValidator.VisitExpression(view.Query);
                    return refValidator.Errors;
                }
            }
            return validator.Errors;
        }

        private sealed class ViewExpressionValidator : BasicExpressionVisitor
        {
            private readonly StorageSetMapping _setMapping;
            private readonly List<EdmSchemaError> _errors;
            private readonly EntityTypeBase _elementType;
            private readonly bool _includeSubtypes;

            private EdmItemCollection EdmItemCollection { get { return _setMapping.EntityContainerMapping.StorageMappingItemCollection.EdmItemCollection; } }
            private StoreItemCollection StoreItemCollection { get { return _setMapping.EntityContainerMapping.StorageMappingItemCollection.StoreItemCollection; } }

            internal ViewExpressionValidator(StorageSetMapping setMapping, EntityTypeBase elementType, bool includeSubtypes)
            {
                Debug.Assert(null != setMapping);
                Debug.Assert(null != elementType);

                _setMapping = setMapping;
                _elementType = elementType;
                _includeSubtypes = includeSubtypes;

                _errors = new List<EdmSchemaError>();
            }

            internal IEnumerable<EdmSchemaError> Errors { get { return _errors; } }

            public override void VisitExpression(DbExpression expression)
            {
                if (null != expression)
                {
                    ValidateExpressionKind(expression.ExpressionKind);
                }
                base.VisitExpression(expression);
            }

            private void ValidateExpressionKind(DbExpressionKind expressionKind)
            {
                switch (expressionKind)
                {
                    // Supported expression kinds
                    case DbExpressionKind.Constant:
                    case DbExpressionKind.Property:
                    case DbExpressionKind.Null:
                    case DbExpressionKind.VariableReference:
                    case DbExpressionKind.Cast:
                    case DbExpressionKind.Case:
                    case DbExpressionKind.Not:
                    case DbExpressionKind.Or:
                    case DbExpressionKind.And:
                    case DbExpressionKind.IsNull:
                    case DbExpressionKind.Equals:
                    case DbExpressionKind.NotEquals:
                    case DbExpressionKind.LessThan:
                    case DbExpressionKind.LessThanOrEquals:
                    case DbExpressionKind.GreaterThan:
                    case DbExpressionKind.GreaterThanOrEquals:
                    case DbExpressionKind.Project:
                    case DbExpressionKind.NewInstance:
                    case DbExpressionKind.Filter:
                    case DbExpressionKind.Ref:
                    case DbExpressionKind.UnionAll:
                    case DbExpressionKind.Scan:
                    case DbExpressionKind.FullOuterJoin:
                    case DbExpressionKind.LeftOuterJoin:
                    case DbExpressionKind.InnerJoin:
                    case DbExpressionKind.EntityRef:
                    case DbExpressionKind.Function:
                        break;
                    default:
                        string elementString = (_includeSubtypes) ? "IsTypeOf(" + _elementType.ToString() + ")" : _elementType.ToString();
                        _errors.Add(new EdmSchemaError(System.Data.Entity.Strings.Mapping_UnsupportedExpressionKind_QueryView(
                            _setMapping.Set.Name, elementString, expressionKind), (int)StorageMappingErrorCode.MappingUnsupportedExpressionKindQueryView,
                            EdmSchemaErrorSeverity.Error, _setMapping.EntityContainerMapping.SourceLocation, _setMapping.StartLineNumber,
                            _setMapping.StartLinePosition));
                        break;
                }
            }

            public override void Visit(DbPropertyExpression expression)
            {
                base.Visit(expression);
                if (expression.Property.BuiltInTypeKind != BuiltInTypeKind.EdmProperty)
                {
                    _errors.Add(new EdmSchemaError(System.Data.Entity.Strings.Mapping_UnsupportedPropertyKind_QueryView(
                        _setMapping.Set.Name, expression.Property.Name, expression.Property.BuiltInTypeKind), (int)StorageMappingErrorCode.MappingUnsupportedPropertyKindQueryView,
                        EdmSchemaErrorSeverity.Error, _setMapping.EntityContainerMapping.SourceLocation, _setMapping.StartLineNumber,
                        _setMapping.StartLinePosition));

                }
            }

            public override void Visit(DbNewInstanceExpression expression)
            {
                base.Visit(expression);
                EdmType type = expression.ResultType.EdmType;
                if (type.BuiltInTypeKind != BuiltInTypeKind.RowType)
                {
                    // restrict initialization of non-row types to the target of the view or complex types
                    // in the target
                    if (!(type == _elementType || (_includeSubtypes && _elementType.IsAssignableFrom(type))) &&
                        !(type.BuiltInTypeKind == BuiltInTypeKind.ComplexType && GetComplexTypes().Contains((ComplexType)type)))
                    {
                        _errors.Add(new EdmSchemaError(System.Data.Entity.Strings.Mapping_UnsupportedInitialization_QueryView(
                            _setMapping.Set.Name, type.FullName), (int)StorageMappingErrorCode.MappingUnsupportedInitializationQueryView,
                            EdmSchemaErrorSeverity.Error, _setMapping.EntityContainerMapping.SourceLocation, _setMapping.StartLineNumber,
                            _setMapping.StartLinePosition));
                    }
                }
            }

            /// <summary>
            /// Retrieves all complex types that can be constructed as part of the view.
            /// </summary>
            private IEnumerable<ComplexType> GetComplexTypes()
            {
                // Retrieve all top-level properties of entity types constructed in the view.
                IEnumerable<EdmProperty> properties = GetEntityTypes().SelectMany(entityType => entityType.Properties).Distinct();
                return GetComplexTypes(properties);
            }

            /// <summary>
            /// Recursively identify complex types.
            /// </summary>
            private IEnumerable<ComplexType> GetComplexTypes(IEnumerable<EdmProperty> properties)
            {
                // 
                foreach (ComplexType complexType in properties.Select(p => p.TypeUsage.EdmType).OfType<ComplexType>())
                {
                    yield return complexType;
                    foreach (ComplexType nestedComplexType in GetComplexTypes(complexType.Properties))
                    {
                        yield return nestedComplexType;
                    }
                }
            }

            /// <summary>
            /// Gets all entity types in scope for this view.
            /// </summary>
            private IEnumerable<EntityType> GetEntityTypes()
            {
                if (_includeSubtypes)
                {
                    // Return all entity types in the hierarchy for OfType or 'complete' views.
                    return MetadataHelper.GetTypeAndSubtypesOf(_elementType, this.EdmItemCollection, true).OfType<EntityType>();
                }
                else if (_elementType.BuiltInTypeKind == BuiltInTypeKind.EntityType)
                {
                    // Yield single entity type for OfType(only ) views.
                    return Enumerable.Repeat((EntityType)_elementType, 1);
                }
                else
                {
                    // For association set views, there are no entity types involved.
                    return Enumerable.Empty<EntityType>();
                }
            }

            public override void Visit(DbFunctionExpression expression)
            {
                base.Visit(expression);

                // Verify function is defined in S-space or it is a built-in canonical function.
                if (!IsStoreSpaceOrCanonicalFunction(this.StoreItemCollection, expression.Function))
                {
                    _errors.Add(new EdmSchemaError(System.Data.Entity.Strings.Mapping_UnsupportedFunctionCall_QueryView(
                        _setMapping.Set.Name, expression.Function.Identity), (int)StorageMappingErrorCode.UnsupportedFunctionCallInQueryView,
                        EdmSchemaErrorSeverity.Error, _setMapping.EntityContainerMapping.SourceLocation, _setMapping.StartLineNumber,
                        _setMapping.StartLinePosition));
                }
            }
            internal static bool IsStoreSpaceOrCanonicalFunction(StoreItemCollection sSpace, EdmFunction function)
            {
                if (TypeHelpers.IsCanonicalFunction(function))
                {
                    return true;
                }
                else
                {
                    // Even if function is declared in s-space, view expression will contain the version of the function
                    // in c-space terms, thus checking function.DataSpace will always give c-space.
                    // In order to determine if the function originates in s-space we need to get check if it belongs
                    // to the list of c-space conversions.
                    var cTypeFunctions = sSpace.GetCTypeFunctions(function.FullName, false);
                    return cTypeFunctions.Contains(function);
                }
            }

            public override void Visit(DbScanExpression expression)
            {
                base.Visit(expression);
                Debug.Assert(null != expression.Target);

                // Verify scan target is in S-space.
                EntitySetBase target = expression.Target;
                EntityContainer targetContainer = target.EntityContainer;
                Debug.Assert(null != target.EntityContainer);

                if ((targetContainer.DataSpace != DataSpace.SSpace))
                {
                    _errors.Add(new EdmSchemaError(System.Data.Entity.Strings.Mapping_UnsupportedScanTarget_QueryView(
                        _setMapping.Set.Name, target.Name), (int)StorageMappingErrorCode.MappingUnsupportedScanTargetQueryView,
                        EdmSchemaErrorSeverity.Error, _setMapping.EntityContainerMapping.SourceLocation, _setMapping.StartLineNumber,
                        _setMapping.StartLinePosition));
                }
            }
        }

        /// <summary>
        /// The visitor validates that the QueryView for an AssociationSet uses the same EntitySets when
        /// creating the ends that were used in CSDL. Since the Query View is already validated, we can expect to
        /// see only a very restricted set of expressions in the tree.
        /// </summary>
        private class AssociationSetViewValidator : DbExpressionVisitor<DbExpressionEntitySetInfo>
        {
            private readonly Stack<KeyValuePair<string, DbExpressionEntitySetInfo>> variableScopes = new Stack<KeyValuePair<string, DbExpressionEntitySetInfo>>();
            private StorageSetMapping _setMapping;
            private List<EdmSchemaError> _errors = new List<EdmSchemaError>();

            internal AssociationSetViewValidator(StorageSetMapping setMapping)
                : base()
            {
                Debug.Assert(setMapping != null);
                _setMapping = setMapping;
            }

            internal List<EdmSchemaError> Errors
            {
                get { return _errors; }
            }

            internal DbExpressionEntitySetInfo VisitExpression(DbExpression expression)
            {
                return expression.Accept(this);
            }

            private DbExpressionEntitySetInfo VisitExpressionBinding(DbExpressionBinding binding)
            {
                DbExpressionBinding result = binding;
                if (binding != null)
                {
                    return this.VisitExpression(binding.Expression);
                }
                return null;
            }

            private void VisitExpressionBindingEnterScope(DbExpressionBinding binding)
            {
                DbExpressionEntitySetInfo info = this.VisitExpressionBinding(binding);
                this.variableScopes.Push(new KeyValuePair<string, DbExpressionEntitySetInfo>(binding.VariableName, info));
            }

            private void VisitExpressionBindingExitScope()
            {
                this.variableScopes.Pop();
            }

            //Verifies that the Sets we got from visiting the tree( under AssociationType constructor) match the ones 
            //defined in CSDL
            private void ValidateEntitySetsMappedForAssociationSetMapping(DbExpressionStructuralTypeEntitySetInfo setInfos)
            {
                AssociationSet associationSet = _setMapping.Set as AssociationSet;
                int i = 0;
                //While we should be able to find the EntitySets in all cases, since this is a user specified
                //query view, it is better to be defensive since we might have missed some path up the tree
                //while computing the sets
                if (setInfos.SetInfos.All(it => ((it.Value != null) && (it.Value is DbExpressionSimpleTypeEntitySetInfo)))
                    && setInfos.SetInfos.Count() == 2)
                {
                    foreach (DbExpressionSimpleTypeEntitySetInfo setInfo in setInfos.SetInfos.Select(it => it.Value))
                    {
                        AssociationSetEnd setEnd = associationSet.AssociationSetEnds[i];
                        EntitySet declaredSet = setEnd.EntitySet;
                        if (!declaredSet.Equals(setInfo.EntitySet))
                        {
                            _errors.Add(new EdmSchemaError(System.Data.Entity.Strings.Mapping_EntitySetMismatchOnAssociationSetEnd_QueryView(
                            setInfo.EntitySet.Name, declaredSet.Name, setEnd.Name, _setMapping.Set.Name), (int)StorageMappingErrorCode.MappingUnsupportedInitializationQueryView,
                            EdmSchemaErrorSeverity.Error, _setMapping.EntityContainerMapping.SourceLocation, _setMapping.StartLineNumber,
                            _setMapping.StartLinePosition));
                        }
                        i++;
                    }
                }
            }


            #region DbExpressionVisitor<DbExpression> Members
            public override DbExpressionEntitySetInfo Visit(DbExpression expression)
            {
                return null;
            }

            public override DbExpressionEntitySetInfo Visit(DbVariableReferenceExpression expression)
            {
                return this.variableScopes.Where(it => (it.Key == expression.VariableName)).Select(it => it.Value).FirstOrDefault();
            }

            public override DbExpressionEntitySetInfo Visit(DbPropertyExpression expression)
            {
                DbExpressionStructuralTypeEntitySetInfo setInfos = VisitExpression(expression.Instance) as DbExpressionStructuralTypeEntitySetInfo;
                if (setInfos != null)
                {
                    return setInfos.GetEntitySetInfoForMember(expression.Property.Name);
                }
                return null;
            }

            public override DbExpressionEntitySetInfo Visit(DbProjectExpression expression)
            {
                this.VisitExpressionBindingEnterScope(expression.Input);
                DbExpressionEntitySetInfo setInfo = VisitExpression(expression.Projection);
                this.VisitExpressionBindingExitScope();
                return setInfo;
            }

            public override DbExpressionEntitySetInfo Visit(DbNewInstanceExpression expression)
            {
                DbExpressionMemberCollectionEntitySetInfo argumentSetInfos = VisitExpressionList(expression.Arguments);
                StructuralType structuralType = (expression.ResultType.EdmType as StructuralType);
                if (argumentSetInfos != null && structuralType != null)
                {
                    DbExpressionStructuralTypeEntitySetInfo structuralTypeSetInfos = new DbExpressionStructuralTypeEntitySetInfo();                    
                    int i = 0;
                    foreach (DbExpressionEntitySetInfo info in argumentSetInfos.entitySetInfos)
                    {
                        structuralTypeSetInfos.Add(structuralType.Members[i].Name, info);
                        i++;
                    }
                    //Since we already validated the query view, the only association type that
                    //can be constructed is the type for the set we are validating the mapping for.
                    if (expression.ResultType.EdmType.BuiltInTypeKind == BuiltInTypeKind.AssociationType)
                    {
                        ValidateEntitySetsMappedForAssociationSetMapping(structuralTypeSetInfos);
                    }
                    return structuralTypeSetInfos;
                }
                return null;
            }

            private DbExpressionMemberCollectionEntitySetInfo VisitExpressionList(IList<DbExpression> list)
            {
                return new DbExpressionMemberCollectionEntitySetInfo(list.Select(it => (VisitExpression(it))));
            }


            public override DbExpressionEntitySetInfo Visit(DbRefExpression expression)
            {
                return new DbExpressionSimpleTypeEntitySetInfo(expression.EntitySet);
            }


            public override DbExpressionEntitySetInfo Visit(DbComparisonExpression expression)
            {
                return null;
            }

            public override DbExpressionEntitySetInfo Visit(DbLikeExpression expression)
            {
                return null;
            }

            public override DbExpressionEntitySetInfo Visit(DbLimitExpression expression)
            {
                return null;
            }

            public override DbExpressionEntitySetInfo Visit(DbIsNullExpression expression)
            {
                return null;
            }

            public override DbExpressionEntitySetInfo Visit(DbArithmeticExpression expression)
            {
                return null;
            }

            public override DbExpressionEntitySetInfo Visit(DbAndExpression expression)
            {
                return null;
            }

            public override DbExpressionEntitySetInfo Visit(DbOrExpression expression)
            {
                return null;
            }

            public override DbExpressionEntitySetInfo Visit(DbNotExpression expression)
            {
                return null;
            }

            public override DbExpressionEntitySetInfo Visit(DbDistinctExpression expression)
            {
                return null;
            }

            public override DbExpressionEntitySetInfo Visit(DbElementExpression expression)
            {
                return null;
            }

            public override DbExpressionEntitySetInfo Visit(DbIsEmptyExpression expression)
            {
                return null;
            }

            public override DbExpressionEntitySetInfo Visit(DbUnionAllExpression expression)
            {
                return null;
            }

            public override DbExpressionEntitySetInfo Visit(DbIntersectExpression expression)
            {
                return null;
            }

            public override DbExpressionEntitySetInfo Visit(DbExceptExpression expression)
            {
                return null;
            }

            public override DbExpressionEntitySetInfo Visit(DbTreatExpression expression)
            {
                return null;
            }

            public override DbExpressionEntitySetInfo Visit(DbIsOfExpression expression)
            {
                return null;
            }

            public override DbExpressionEntitySetInfo Visit(DbCastExpression expression)
            {
                return null;
            }

            public override DbExpressionEntitySetInfo Visit(DbCaseExpression expression)
            {
                return null;
            }

            public override DbExpressionEntitySetInfo Visit(DbOfTypeExpression expression)
            {
                return null;
            }

            public override DbExpressionEntitySetInfo Visit(DbRelationshipNavigationExpression expression)
            {
                return null;
            }

            public override DbExpressionEntitySetInfo Visit(DbDerefExpression expression)
            {
                return null;
            }

            public override DbExpressionEntitySetInfo Visit(DbRefKeyExpression expression)
            {
                return null;
            }

            public override DbExpressionEntitySetInfo Visit(DbEntityRefExpression expression)
            {
                return null;
            }

            public override DbExpressionEntitySetInfo Visit(DbScanExpression expression)
            {
                return null;
            }

            public override DbExpressionEntitySetInfo Visit(DbFilterExpression expression)
            {
                return null;
            }

            public override DbExpressionEntitySetInfo Visit(DbConstantExpression expression)
            {
                return null;
            }

            public override DbExpressionEntitySetInfo Visit(DbNullExpression expression)
            {
                return null;
            }

            public override DbExpressionEntitySetInfo Visit(DbCrossJoinExpression expression)
            {
                return null;
            }

            public override DbExpressionEntitySetInfo Visit(DbJoinExpression expression)
            {
                return null;
            }

            public override DbExpressionEntitySetInfo Visit(DbParameterReferenceExpression expression)
            {
                return null;
            }

            public override DbExpressionEntitySetInfo Visit(DbFunctionExpression expression)
            {
                return null;
            }

            public override DbExpressionEntitySetInfo Visit(DbLambdaExpression expression)
            {
                return null;
            }


            public override DbExpressionEntitySetInfo Visit(DbApplyExpression expression)
            {
                return null;
            }

            public override DbExpressionEntitySetInfo Visit(DbGroupByExpression expression)
            {
                return null;
            }

            public override DbExpressionEntitySetInfo Visit(DbSkipExpression expression)
            {
                return null;
            }

            public override DbExpressionEntitySetInfo Visit(DbSortExpression expression)
            {
                return null;
            }

            public override DbExpressionEntitySetInfo Visit(DbQuantifierExpression expression)
            {
                return null;
            }

            #endregion

        }

        internal abstract class DbExpressionEntitySetInfo { }
        #region DbExpressionEntitySetInfo implementations
        private class DbExpressionSimpleTypeEntitySetInfo : DbExpressionEntitySetInfo
        {
            private EntitySet m_entitySet;

            internal EntitySet EntitySet
            {
                get { return m_entitySet; }
            }

            internal DbExpressionSimpleTypeEntitySetInfo(EntitySet entitySet)
            {
                m_entitySet = entitySet;
            }
        }

        private class DbExpressionStructuralTypeEntitySetInfo : DbExpressionEntitySetInfo
        {
            private Dictionary<String, DbExpressionEntitySetInfo> m_entitySetInfos;

            internal DbExpressionStructuralTypeEntitySetInfo()
            {
                m_entitySetInfos = new Dictionary<string, DbExpressionEntitySetInfo>();
            }

            internal void Add(string key, DbExpressionEntitySetInfo value)
            {
                m_entitySetInfos.Add(key, value);
            }

            internal IEnumerable<KeyValuePair<string, DbExpressionEntitySetInfo>> SetInfos
            {
                get
                {
                    return m_entitySetInfos;
                }
            }

            internal DbExpressionEntitySetInfo GetEntitySetInfoForMember(string memberName)
            {
                return m_entitySetInfos[memberName];
            }
        }

        private class DbExpressionMemberCollectionEntitySetInfo : DbExpressionEntitySetInfo
        {
            private IEnumerable<DbExpressionEntitySetInfo> m_entitySets;

            internal DbExpressionMemberCollectionEntitySetInfo(IEnumerable<DbExpressionEntitySetInfo> entitySetInfos)
            {
                m_entitySets = entitySetInfos;
            }

            internal IEnumerable<DbExpressionEntitySetInfo> entitySetInfos
            {
                get
                {
                    return m_entitySets;
                }
            }
        }
        #endregion
    }
}
