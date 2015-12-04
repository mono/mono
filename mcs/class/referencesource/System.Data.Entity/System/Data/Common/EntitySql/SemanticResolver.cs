//---------------------------------------------------------------------
// <copyright file="SemanticResolver.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  [....]
// @backupOwner [....]
//---------------------------------------------------------------------

namespace System.Data.Common.EntitySql
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common.CommandTrees;
    using System.Data.Common.CommandTrees.ExpressionBuilder;
    using System.Data.Entity;
    using System.Data.Metadata.Edm;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;

    /// <summary>
    /// Represents eSQL expression class.
    /// </summary>
    internal enum ExpressionResolutionClass
    {
        /// <summary>
        /// A value expression such as a literal, variable or a value-returning expression.
        /// </summary>
        Value,
        /// <summary>
        /// An expression returning an entity container.
        /// </summary>
        EntityContainer,
        /// <summary>
        /// An expression returning a metadata member such as a type, function group or namespace.
        /// </summary>
        MetadataMember
    }

    /// <summary>
    /// Abstract class representing the result of an eSQL expression classification.
    /// </summary>
    internal abstract class ExpressionResolution
    {
        protected ExpressionResolution(ExpressionResolutionClass @class)
        {
            ExpressionClass = @class;
        }

        internal readonly ExpressionResolutionClass ExpressionClass;
        internal abstract string ExpressionClassName { get; }
    }

    /// <summary>
    /// Represents an eSQL expression classified as <see cref="ExpressionResolutionClass.Value"/>.
    /// </summary>
    internal sealed class ValueExpression : ExpressionResolution
    {
        internal ValueExpression(DbExpression value)
            : base(ExpressionResolutionClass.Value)
        {
            Value = value;
        }

        internal override string ExpressionClassName { get { return ValueClassName; } }
        internal static string ValueClassName { get { return Strings.LocalizedValueExpression; } }

        /// <summary>
        /// Null if <see cref="ValueExpression"/> represents the untyped null.
        /// </summary>
        internal readonly DbExpression Value;
    }

    /// <summary>
    /// Represents an eSQL expression classified as <see cref="ExpressionResolutionClass.EntityContainer"/>.
    /// </summary>
    internal sealed class EntityContainerExpression : ExpressionResolution
    {
        internal EntityContainerExpression(EntityContainer entityContainer)
            : base(ExpressionResolutionClass.EntityContainer)
        {
            EntityContainer = entityContainer;
        }

        internal override string ExpressionClassName { get { return EntityContainerClassName; } }
        internal static string EntityContainerClassName { get { return Strings.LocalizedEntityContainerExpression; } }

        internal readonly EntityContainer EntityContainer;
    }

    /// <summary>
    /// Implements the semantic resolver in the context of a metadata workspace and typespace.
    /// </summary>
    /// <remarks>not thread safe</remarks>
    internal sealed class SemanticResolver
    {
        #region Fields
        private readonly ParserOptions _parserOptions;
        private readonly Dictionary<string, DbParameterReferenceExpression> _parameters;
        private readonly Dictionary<string, DbVariableReferenceExpression> _variables;
        private readonly TypeResolver _typeResolver;
        private readonly ScopeManager _scopeManager;
        private readonly List<ScopeRegion> _scopeRegions = new List<ScopeRegion>();
        private bool _ignoreEntityContainerNameResolution = false;
        private GroupAggregateInfo _currentGroupAggregateInfo = null;
        private uint _namegenCounter = 0;
        #endregion

        #region Constructors
        /// <summary>
        /// Creates new instance of <see cref="SemanticResolver"/>.
        /// </summary>
        internal static SemanticResolver Create(Perspective perspective,
                                                ParserOptions parserOptions,
                                                IEnumerable<DbParameterReferenceExpression> parameters,
                                                IEnumerable<DbVariableReferenceExpression> variables)
        {
            EntityUtil.CheckArgumentNull(perspective, "perspective");
            EntityUtil.CheckArgumentNull(parserOptions, "parserOptions");

            return new SemanticResolver(
                parserOptions, 
                ProcessParameters(parameters, parserOptions), 
                ProcessVariables(variables, parserOptions), 
                new TypeResolver(perspective, parserOptions));
        }

        /// <summary>
        /// Creates a copy of <see cref="SemanticResolver"/> with clean scopes and shared inline function definitions inside of the type resolver.
        /// </summary>
        internal SemanticResolver CloneForInlineFunctionConversion()
        {
            return new SemanticResolver(
                 _parserOptions,
                 _parameters,
                 _variables,
                 _typeResolver);
        }

        private SemanticResolver(ParserOptions parserOptions,
                                 Dictionary<string, DbParameterReferenceExpression> parameters,
                                 Dictionary<string, DbVariableReferenceExpression> variables,
                                 TypeResolver typeResolver)
        {
            _parserOptions = parserOptions;
            _parameters = parameters;
            _variables = variables;
            _typeResolver = typeResolver;

            //
            // Creates Scope manager
            //
            _scopeManager = new ScopeManager(this.NameComparer);

            //
            // Push a root scope region
            //
            EnterScopeRegion();

            //
            // Add command free variables to the root scope
            //
            foreach (DbVariableReferenceExpression variable in _variables.Values)
            {
                this.CurrentScope.Add(variable.VariableName, new FreeVariableScopeEntry(variable));
            }
        }

        /// <summary>
        /// Validates that the specified parameters have valid, non-duplicated names
        /// </summary>
        /// <param name="paramDefs">The set of query parameters</param>
        /// <returns>A valid dictionary that maps parameter names to <see cref="DbParameterReferenceExpression"/>s using the current NameComparer</returns>
        private static Dictionary<string, DbParameterReferenceExpression> ProcessParameters(IEnumerable<DbParameterReferenceExpression> paramDefs, ParserOptions parserOptions)
        {
            Dictionary<string, DbParameterReferenceExpression> retParams = new Dictionary<string, DbParameterReferenceExpression>(parserOptions.NameComparer);

            if (paramDefs != null)
            {
                foreach (DbParameterReferenceExpression paramDef in paramDefs)
                {
                    if (retParams.ContainsKey(paramDef.ParameterName))
                    {
                        throw EntityUtil.EntitySqlError(Strings.MultipleDefinitionsOfParameter(paramDef.ParameterName));
                    }

                    Debug.Assert(paramDef.ResultType.IsReadOnly, "paramDef.ResultType.IsReadOnly must be set");

                    retParams.Add(paramDef.ParameterName, paramDef);
                }
            }

            return retParams;
        }

        /// <summary>
        /// Validates that the specified variables have valid, non-duplicated names
        /// </summary>
        /// <param name="varDefs">The set of free variables</param>
        /// <returns>A valid dictionary that maps variable names to <see cref="DbVariableReferenceExpression"/>s using the current NameComparer</returns>
        private static Dictionary<string, DbVariableReferenceExpression> ProcessVariables(IEnumerable<DbVariableReferenceExpression> varDefs, ParserOptions parserOptions)
        {
            Dictionary<string, DbVariableReferenceExpression> retVars = new Dictionary<string, DbVariableReferenceExpression>(parserOptions.NameComparer);

            if (varDefs != null)
            {
                foreach (DbVariableReferenceExpression varDef in varDefs)
                {
                    if (retVars.ContainsKey(varDef.VariableName))
                    {
                        throw EntityUtil.EntitySqlError(Strings.MultipleDefinitionsOfVariable(varDef.VariableName));
                    }

                    Debug.Assert(varDef.ResultType.IsReadOnly, "varDef.ResultType.IsReadOnly must be set");

                    retVars.Add(varDef.VariableName, varDef);
                }
            }

            return retVars;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Returns ordinary command parameters. Empty dictionary in case of no parameters.
        /// </summary>
        internal Dictionary<string, DbParameterReferenceExpression> Parameters
        {
            get { return _parameters; }
        }

        /// <summary>
        /// Returns command free variables. Empty dictionary in case of no variables.
        /// </summary>
        internal Dictionary<string, DbVariableReferenceExpression> Variables
        {
            get { return _variables; }
        }

        /// <summary>
        /// TypeSpace/Metadata/Perspective dependent type resolver.
        /// </summary>
        internal TypeResolver TypeResolver
        {
            get { return _typeResolver; }
        }

        /// <summary>
        /// Returns current Parser Options.
        /// </summary>
        internal ParserOptions ParserOptions
        {
            get { return _parserOptions; }
        }

        /// <summary>
        /// Returns the current string comparer.
        /// </summary>
        internal StringComparer NameComparer
        {
            get { return _parserOptions.NameComparer; }
        }

        /// <summary>
        /// Returns the list of scope regions: outer followed by inner.
        /// </summary>
        internal IEnumerable<ScopeRegion> ScopeRegions
        {
            get { return _scopeRegions; }
        }

        /// <summary>
        /// Returns the current scope region.
        /// </summary>
        internal ScopeRegion CurrentScopeRegion
        {
            get { return _scopeRegions[_scopeRegions.Count - 1]; }
        }

        /// <summary>
        /// Returns the current scope.
        /// </summary>
        internal Scope CurrentScope
        {
            get { return _scopeManager.CurrentScope; }
        }

        /// <summary>
        /// Returns index of the current scope.
        /// </summary>
        internal int CurrentScopeIndex
        {
            get { return _scopeManager.CurrentScopeIndex; }
        }

        /// <summary>
        /// Returns the current group aggregate info when processing group aggregate argument.
        /// </summary>
        internal GroupAggregateInfo CurrentGroupAggregateInfo
        {
            get { return _currentGroupAggregateInfo; }
        }
        #endregion

        #region GetExpressionFromScopeEntry
        /// <summary>
        /// Returns the appropriate expression from a given scope entry.
        /// May return null for scope entries like <see cref="InvalidGroupInputRefScopeEntry"/>.
        /// </summary>
        private DbExpression GetExpressionFromScopeEntry(ScopeEntry scopeEntry, int scopeIndex, string varName, ErrorContext errCtx)
        {
            //
            // If
            //      1) we are in the context of a group aggregate or group key, 
            //      2) and the scopeEntry can have multiple interpretations depending on the aggregation context,
            //      3) and the defining scope region of the scopeEntry is outer or equal to the defining scope region of the group aggregate,
            //      4) and the defining scope region of the scopeEntry is not performing conversion of a group key definition,
            // Then the expression that corresponds to the scopeEntry is either the GroupVarBasedExpression or the GroupAggBasedExpression.
            // Otherwise the default expression that corresponds to the scopeEntry is provided by scopeEntry.GetExpression(...) call.
            //
            // Explanation for #2 from the list above:
            // A scope entry may have multiple aggregation-context interpretations:
            //      - An expression in the context of a group key definition, obtained by scopeEntry.GetExpression(...);
            //        Example: select k1 from {0} as a group by a%2 as k1
            //                                                  ^^^
            //      - An expression in the context of a function aggregate, provided by iGroupExpressionExtendedInfo.GroupVarBasedExpression;
            //        Example: select max( a ) from {0} as a group by a%2 as k1
            //                            ^^^
            //      - An expression in the context of a group partition, provided by iGroupExpressionExtendedInfo.GroupAggBasedExpression;
            //        Example: select GroupPartition( a ) from {0} as a group by a%2 as k1
            //                                       ^^^
            // Note that expressions obtained from aggregation-context-dependent scope entries outside of the three contexts mentioned above
            // will default to the value returned by the scopeEntry.GetExpression(...) call. This value is the same as in the group key definition context.
            // These expressions have correct result types which enables partial expression validation. 
            // However the contents of the expressions are invalid outside of the group key definitions, hence they can not appear in the final expression tree.
            // SemanticAnalyzer.ProcessGroupByClause(...) method guarantees that such expressions are only temporarily used during GROUP BY clause processing and
            // dropped afterwards.
            // Example: select a, k1 from {0} as a group by a%2 as k1
            //                 ^^^^^ - these expressions are processed twice: once during GROUP BY and then SELECT clause processing,
            //                         the expressions obtained during GROUP BY clause processing are dropped and only
            //                         the ones obtained during SELECT clause processing are accepted.
            //
            // Explanation for #3 from the list above:
            //      - An outer scope entry referenced inside of an aggregate may lift the aggregate to the outer scope region for evaluation, 
            //        hence such a scope entry must be interpreted in the aggregation context. See explanation for #4 below for more info.
            //        Example: 
            //
            //          select 
            //              (select max(x) from {1} as y) 
            //          from {0} as x
            //
            //      - If a scope entry is defined inside of a group aggregate, then the scope entry is not affected by the aggregate, 
            //        hence such a scope entry is not interpreted in the aggregation context.
            //        Example:
            //
            //          select max(
            //                       anyelement( select b from {1} as b )  
            //                    )
            //          from {0} as a group by a %2 as a1
            //
            //        In this query the aggregate argument contains a nested query expression.
            //        The nested query references b. Because b is defined inside of the aggregate it is not interpreted in the aggregation context and
            //        the expression for b should not be GroupVar/GroupAgg based, even though the reference to b appears inside of an aggregate.
            //
            // Explanation for #4 from the list above:
            // An aggregate evaluating on a particular scope region defines the interpretation of scope entries defined on that scope region.
            // In the case when an inner aggregate references a scope entry belonging to the evaluating region of an outer aggregate, the interpretation
            // of the scope entry is controlled by the outer aggregate, otherwise it is controlled by the inner aggregate.
            // Example:
            //
            //      select a1
            //      from {0} as a group by 
            //                                anyelement(select value max(a + b) from {1} as b)
            //                          as a1
            //
            // In this query the aggregate inside of a1 group key definition, the max(a + b), references scope entry a.
            // Because a is referenced inside of the group key definition (which serves as an outer aggregate) and the key definition belongs to
            // the same scope region as a, a is interpreted in the context of the group key definition, not the function aggregate and
            // the expression for a is obtained by scopeEntry.GetExpression(...) call, not iGroupExpressionExtendedInfo.GroupVarBasedExpression.
            //

            DbExpression expr = scopeEntry.GetExpression(varName, errCtx);
            Debug.Assert(expr != null, "scopeEntry.GetExpression(...) returned null");

            if (_currentGroupAggregateInfo != null)
            {
                //
                // Make sure defining scope regions agree as described above.
                // Outer scope region has smaller index value than the inner.
                //
                ScopeRegion definingScopeRegionOfScopeEntry = GetDefiningScopeRegion(scopeIndex);
                if (definingScopeRegionOfScopeEntry.ScopeRegionIndex <= _currentGroupAggregateInfo.DefiningScopeRegion.ScopeRegionIndex)
                {
                    //
                    // Let the group aggregate know the scope of the scope entry it references.
                    // This affects the scope region that will evaluate the group aggregate.
                    //
                    _currentGroupAggregateInfo.UpdateScopeIndex(scopeIndex, this);

                    IGroupExpressionExtendedInfo iGroupExpressionExtendedInfo = scopeEntry as IGroupExpressionExtendedInfo;
                    if (iGroupExpressionExtendedInfo != null)
                    {
                        //
                        // Find the aggregate that controls interpretation of the current scope entry.
                        // This would be a containing aggregate with the defining scope region matching definingScopeRegionOfScopeEntry.
                        // If there is no such aggregate, then the current containing aggregate controls interpretation.
                        //
                        GroupAggregateInfo expressionInterpretationContext;
                        for (expressionInterpretationContext = _currentGroupAggregateInfo; 
                             expressionInterpretationContext != null &&
                             expressionInterpretationContext.DefiningScopeRegion.ScopeRegionIndex >= definingScopeRegionOfScopeEntry.ScopeRegionIndex;
                             expressionInterpretationContext = expressionInterpretationContext.ContainingAggregate)
                        {
                            if (expressionInterpretationContext.DefiningScopeRegion.ScopeRegionIndex == definingScopeRegionOfScopeEntry.ScopeRegionIndex)
                            {
                                break;
                            }
                        }
                        if (expressionInterpretationContext == null ||
                            expressionInterpretationContext.DefiningScopeRegion.ScopeRegionIndex < definingScopeRegionOfScopeEntry.ScopeRegionIndex)
                        {
                            expressionInterpretationContext = _currentGroupAggregateInfo;
                        }

                        switch (expressionInterpretationContext.AggregateKind)
                        {
                            case GroupAggregateKind.Function:
                                if (iGroupExpressionExtendedInfo.GroupVarBasedExpression != null)
                                {
                                    expr = iGroupExpressionExtendedInfo.GroupVarBasedExpression;
                                }
                                break;

                            case GroupAggregateKind.Partition:
                                if (iGroupExpressionExtendedInfo.GroupAggBasedExpression != null)
                                {
                                    expr = iGroupExpressionExtendedInfo.GroupAggBasedExpression;
                                }
                                break;

                            case GroupAggregateKind.GroupKey:
                                //
                                // User the current expression obtained from scopeEntry.GetExpression(...)
                                //
                                break;

                            default:
                                Debug.Fail("Unexpected group aggregate kind.");
                                break;
                        }
                    }
                }
            }

            return expr;
        }
        #endregion

        #region Name resolution
        #region Resolve simple / metadata member name
        internal IDisposable EnterIgnoreEntityContainerNameResolution()
        {
            Debug.Assert(!_ignoreEntityContainerNameResolution, "EnterIgnoreEntityContainerNameResolution() is not reentrant.");
            _ignoreEntityContainerNameResolution = true;
            return new Disposer(delegate
            {
                Debug.Assert(this._ignoreEntityContainerNameResolution, "_ignoreEntityContainerNameResolution must be true.");
                this._ignoreEntityContainerNameResolution = false;
            });
        }

        internal ExpressionResolution ResolveSimpleName(string name, bool leftHandSideOfMemberAccess, ErrorContext errCtx)
        {
            Debug.Assert(!String.IsNullOrEmpty(name), "name must not be null or empty");

            //
            // Try resolving as a scope entry.
            //
            ScopeEntry scopeEntry;
            int scopeIndex;
            if (TryScopeLookup(name, out scopeEntry, out scopeIndex))
            {
                //
                // Check for invalid join left expression correlation.
                //
                if (scopeEntry.EntryKind == ScopeEntryKind.SourceVar && ((SourceScopeEntry)scopeEntry).IsJoinClauseLeftExpr)
                {
                    throw EntityUtil.EntitySqlError(errCtx, Strings.InvalidJoinLeftCorrelation);
                }

                //
                // Set correlation flag.
                //
                SetScopeRegionCorrelationFlag(scopeIndex);

                return new ValueExpression(GetExpressionFromScopeEntry(scopeEntry, scopeIndex, name, errCtx));
            }

            // 
            // Try resolving as a member of the default entity container.
            //
            EntityContainer defaultEntityContainer = this.TypeResolver.Perspective.GetDefaultContainer();
            ExpressionResolution defaultEntityContainerResolution;
            if (defaultEntityContainer != null && TryResolveEntityContainerMemberAccess(defaultEntityContainer, name, errCtx, out defaultEntityContainerResolution))
            {
                return defaultEntityContainerResolution;
            }

            if (!_ignoreEntityContainerNameResolution)
            {
                // 
                // Try resolving as an entity container.
                //
                EntityContainer entityContainer;
                if (this.TypeResolver.Perspective.TryGetEntityContainer(name, _parserOptions.NameComparisonCaseInsensitive /*ignoreCase*/, out entityContainer))
                {
                    return new EntityContainerExpression(entityContainer);
                }
            }

            //
            // Otherwise, resolve as an unqualified name. 
            //
            return this.TypeResolver.ResolveUnqualifiedName(name, leftHandSideOfMemberAccess /* partOfQualifiedName */, errCtx);
        }

        internal MetadataMember ResolveSimpleFunctionName(string name, ErrorContext errCtx)
        {
            //
            // "Foo()" represents a simple function name. Resolve it as an unqualified name by calling the type resolver directly.
            // Note that calling type resolver directly will avoid resolution of the identifier as a local variable or entity container
            // (these resolutions are performed only by ResolveSimpleName(...)).
            //
            var resolution = this.TypeResolver.ResolveUnqualifiedName(name, false /* partOfQualifiedName */, errCtx);
            if (resolution.MetadataMemberClass == MetadataMemberClass.Namespace)
            {
                // 
                // Try resolving as a function import inside the default entity container.
                //
                EntityContainer defaultEntityContainer = this.TypeResolver.Perspective.GetDefaultContainer();
                ExpressionResolution defaultEntityContainerResolution;
                if (defaultEntityContainer != null &&
                    TryResolveEntityContainerMemberAccess(defaultEntityContainer, name, errCtx, out defaultEntityContainerResolution) &&
                    defaultEntityContainerResolution.ExpressionClass == ExpressionResolutionClass.MetadataMember)
                {
                    resolution = (MetadataMember)defaultEntityContainerResolution;
                }
            }
            return resolution;
        }


        /// <summary>
        /// Performs scope lookup returning the scope entry and its index.
        /// </summary>
        private bool TryScopeLookup(string key, out ScopeEntry scopeEntry, out int scopeIndex)
        {
            scopeEntry = null;
            scopeIndex = -1;

            for (int i = CurrentScopeIndex; i >= 0; i--)
            {
                if (_scopeManager.GetScopeByIndex(i).TryLookup(key, out scopeEntry))
                {
                    scopeIndex = i;
                    return true;
                }
            }

            return false;
        }

        internal MetadataMember ResolveMetadataMemberName(string[] name, ErrorContext errCtx)
        {
            return this.TypeResolver.ResolveMetadataMemberName(name, errCtx);
        }
        #endregion

        #region Resolve member name in member access
        #region Resolve property access
        /// <summary>
        /// Resolve property <paramref name="name"/> off the <paramref name="valueExpr"/>.
        /// </summary>
        internal ValueExpression ResolvePropertyAccess(DbExpression valueExpr, string name, ErrorContext errCtx)
        {
            DbExpression propertyExpr;

            if (TryResolveAsPropertyAccess(valueExpr, name, errCtx, out propertyExpr))
            {
                return new ValueExpression(propertyExpr);
            }

            if (TryResolveAsRefPropertyAccess(valueExpr, name, errCtx, out propertyExpr))
            {
                return new ValueExpression(propertyExpr);
            }

            if (TypeSemantics.IsCollectionType(valueExpr.ResultType))
            {
                throw EntityUtil.EntitySqlError(errCtx, Strings.NotAMemberOfCollection(name, valueExpr.ResultType.EdmType.FullName));
            }
            else
            {
                throw EntityUtil.EntitySqlError(errCtx, Strings.NotAMemberOfType(name, valueExpr.ResultType.EdmType.FullName));
            }
        }

        /// <summary>
        /// Try resolving <paramref name="name"/> as a property of the value returned by the <paramref name="valueExpr"/>.
        /// </summary>
        private bool TryResolveAsPropertyAccess(DbExpression valueExpr, string name, ErrorContext errCtx, out DbExpression propertyExpr)
        {
            Debug.Assert(valueExpr != null, "valueExpr != null");

            propertyExpr = null;

            if (Helper.IsStructuralType(valueExpr.ResultType.EdmType))
            {
                EdmMember member;
                if (TypeResolver.Perspective.TryGetMember((StructuralType)valueExpr.ResultType.EdmType, name, _parserOptions.NameComparisonCaseInsensitive /*ignoreCase*/, out member))
                {
                    Debug.Assert(member != null, "member != null");
                    Debug.Assert(this.NameComparer.Equals(name, member.Name), "this.NameComparer.Equals(name, member.Name)");
                    propertyExpr = DbExpressionBuilder.CreatePropertyExpressionFromMember(valueExpr, member);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// If <paramref name="valueExpr"/> returns a reference, then deref and try resolving <paramref name="name"/> as a property of the dereferenced value.
        /// </summary>
        private bool TryResolveAsRefPropertyAccess(DbExpression valueExpr, string name, ErrorContext errCtx, out DbExpression propertyExpr)
        {
            Debug.Assert(valueExpr != null, "valueExpr != null");

            propertyExpr = null;

            if (TypeSemantics.IsReferenceType(valueExpr.ResultType))
            {
                DbExpression derefExpr = valueExpr.Deref();
                TypeUsage derefExprType = derefExpr.ResultType;

                if (TryResolveAsPropertyAccess(derefExpr, name, errCtx, out propertyExpr))
                {
                    return true;
                }
                else
                {
                    throw EntityUtil.EntitySqlError(errCtx, Strings.InvalidDeRefProperty(name, derefExprType.EdmType.FullName, valueExpr.ResultType.EdmType.FullName));
                }
            }

            return false;
        }
        #endregion

        #region Resolve entity container member access
        /// <summary>
        /// Resolve entity set or function import <paramref name="name"/> in the <paramref name="entityContainer"/>
        /// </summary>
        internal ExpressionResolution ResolveEntityContainerMemberAccess(EntityContainer entityContainer, string name, ErrorContext errCtx)
        {
            ExpressionResolution resolution;
            if (TryResolveEntityContainerMemberAccess(entityContainer, name, errCtx, out resolution))
            {
                return resolution;
            }
            else
            {
                throw EntityUtil.EntitySqlError(errCtx, Strings.MemberDoesNotBelongToEntityContainer(name, entityContainer.Name));
            }
        }

        private bool TryResolveEntityContainerMemberAccess(EntityContainer entityContainer, string name, ErrorContext errCtx, out ExpressionResolution resolution)
        {
            EntitySetBase entitySetBase;
            EdmFunction functionImport;
            if (this.TypeResolver.Perspective.TryGetExtent(entityContainer, name, _parserOptions.NameComparisonCaseInsensitive /*ignoreCase*/, out entitySetBase))
            {
                resolution = new ValueExpression(entitySetBase.Scan());
                return true;
            }
            else if (this.TypeResolver.Perspective.TryGetFunctionImport(entityContainer, name, _parserOptions.NameComparisonCaseInsensitive /*ignoreCase*/, out functionImport))
            {
                resolution = new MetadataFunctionGroup(functionImport.FullName, new EdmFunction[] { functionImport });
                return true;
            }
            else
            {
                resolution = null;
                return false;
            }
        }
        #endregion

        #region Resolve metadata member access
        /// <summary>
        /// Resolve namespace, type or function <paramref name="name"/> in the <paramref name="metadataMember"/>
        /// </summary>
        internal MetadataMember ResolveMetadataMemberAccess(MetadataMember metadataMember, string name, ErrorContext errCtx)
        {
            return this.TypeResolver.ResolveMetadataMemberAccess(metadataMember, name, errCtx);
        }
        #endregion
        #endregion

        #region Resolve internal aggregate name / alternative group key name
        /// <summary>
        /// Try resolving an internal aggregate name.
        /// </summary>
        internal bool TryResolveInternalAggregateName(string name, ErrorContext errCtx, out DbExpression dbExpression)
        {
            ScopeEntry scopeEntry;
            int scopeIndex;
            if (TryScopeLookup(name, out scopeEntry, out scopeIndex))
            {
                //
                // Set the correlation flag.
                //
                SetScopeRegionCorrelationFlag(scopeIndex);

                dbExpression = scopeEntry.GetExpression(name, errCtx);
                return true;
            }
            else
            {
                dbExpression = null;
                return false;
            }
        }
        /// <summary>
        /// Try resolving multipart identifier as an alternative name of a group key (see SemanticAnalyzer.ProcessGroupByClause(...) for more info).
        /// </summary>
        internal bool TryResolveDotExprAsGroupKeyAlternativeName(AST.DotExpr dotExpr, out ValueExpression groupKeyResolution)
        {
            groupKeyResolution = null;

            string[] names;
            ScopeEntry scopeEntry;
            int scopeIndex;
            if (IsInAnyGroupScope() &&
                dotExpr.IsMultipartIdentifier(out names) &&
                TryScopeLookup(TypeResolver.GetFullName(names), out scopeEntry, out scopeIndex))
            {
                IGetAlternativeName iGetAlternativeName = scopeEntry as IGetAlternativeName;

                //
                // Accept only if names[] match alternative name part by part.
                //
                if (iGetAlternativeName != null && iGetAlternativeName.AlternativeName != null &&
                    names.SequenceEqual(iGetAlternativeName.AlternativeName, this.NameComparer))
                {
                    //
                    // Set correlation flag
                    //
                    SetScopeRegionCorrelationFlag(scopeIndex);

                    groupKeyResolution = new ValueExpression(GetExpressionFromScopeEntry(scopeEntry, scopeIndex, TypeResolver.GetFullName(names), dotExpr.ErrCtx));
                    return true;
                }
            }
            return false;
        }
        #endregion
        #endregion

        #region Name generation utils (GenerateInternalName, CreateNewAlias, InferAliasName)
        /// <summary>
        /// Generates unique internal name.
        /// </summary>
        internal string GenerateInternalName(string hint)
        {
            // string concat is much faster than String.Format
            return "_##" + hint + unchecked(_namegenCounter++).ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Creates a new alias name based on the <paramref name="expr"/> information.
        /// </summary>
        private string CreateNewAlias(DbExpression expr)
        {
            DbScanExpression extent = expr as DbScanExpression;
            if (null != extent)
            {
                return extent.Target.Name;
            }

            DbPropertyExpression property = expr as DbPropertyExpression;
            if (null != property)
            {
                return property.Property.Name;
            }

            DbVariableReferenceExpression varRef = expr as DbVariableReferenceExpression;
            if (null != varRef)
            {
                return varRef.VariableName;
            }

            return GenerateInternalName(String.Empty);
        }

        /// <summary>
        /// Returns alias name from <paramref name="aliasedExpr"/> ast node if it contains an alias,
        /// otherwise creates a new alias name based on the <paramref name="aliasedExpr"/>.Expr or <paramref name="convertedExpression"/> information.
        /// </summary>
        internal string InferAliasName(AST.AliasedExpr aliasedExpr, DbExpression convertedExpression)
        {
            if (aliasedExpr.Alias != null)
            {
                return aliasedExpr.Alias.Name;
            }

            AST.Identifier id = aliasedExpr.Expr as AST.Identifier;
            if (null != id)
            {
                return id.Name;
            }

            AST.DotExpr dotExpr = aliasedExpr.Expr as AST.DotExpr;
            string[] names;
            if (null != dotExpr && dotExpr.IsMultipartIdentifier(out names))
            {
                return names[names.Length - 1];
            }

            return CreateNewAlias(convertedExpression);
        }
        #endregion

        #region Scope/ScopeRegion utils
        /// <summary>
        /// Enters a new scope region.
        /// </summary>
        internal IDisposable EnterScopeRegion()
        {
            //
            // Push new scope (the first scope in the new scope region)
            //
            _scopeManager.EnterScope();

            //
            // Create new scope region and push it
            //
            ScopeRegion scopeRegion = new ScopeRegion(_scopeManager, CurrentScopeIndex, _scopeRegions.Count);
            _scopeRegions.Add(scopeRegion);

            //
            // Return scope region disposer that rolls back the scope.
            //
            return new Disposer(delegate
                {
                    Debug.Assert(this.CurrentScopeRegion == scopeRegion, "Scope region stack is corrupted.");

                    //
                    // Root scope region is permanent.
                    //
                    Debug.Assert(this._scopeRegions.Count > 1, "_scopeRegionFlags.Count > 1");

                    //
                    // Reset aggregate info of AST nodes of aggregates resolved to the CurrentScopeRegion.
                    //
                    this.CurrentScopeRegion.GroupAggregateInfos.ForEach(groupAggregateInfo => groupAggregateInfo.DetachFromAstNode());

                    //
                    // Rollback scopes of the region.
                    //
                    this.CurrentScopeRegion.RollbackAllScopes();

                    //
                    // Remove the scope region.
                    //
                    this._scopeRegions.Remove(CurrentScopeRegion);
                });
        }

        /// <summary>
        /// Rollback all scopes above the <paramref name="scopeIndex"/>.
        /// </summary>
        internal void RollbackToScope(int scopeIndex)
        {
            _scopeManager.RollbackToScope(scopeIndex);
        }

        /// <summary>
        /// Enter a new scope.
        /// </summary>
        internal void EnterScope()
        {
            _scopeManager.EnterScope();
        }

        /// <summary>
        /// Leave the current scope.
        /// </summary>
        internal void LeaveScope()
        {
            _scopeManager.LeaveScope();
        }

        /// <summary>
        /// Returns true if any of the ScopeRegions from the closest to the outermost has IsAggregating = true
        /// </summary>
        internal bool IsInAnyGroupScope()
        {
            for (int i = 0; i < _scopeRegions.Count; i++)
            {
                if (_scopeRegions[i].IsAggregating)
                {
                    return true;
                }
            }
            return false;
        }

        internal ScopeRegion GetDefiningScopeRegion(int scopeIndex)
        {
            //
            // Starting from the innermost, find the outermost scope region that contains the scope.
            //
            for (int i = _scopeRegions.Count - 1; i >= 0; --i)
            {
                if (_scopeRegions[i].ContainsScope(scopeIndex))
                {
                    return _scopeRegions[i];
                }
            }
            Debug.Fail("Failed to find the defining scope region for the given scope.");
            return null;
        }

        /// <summary>
        /// Sets the scope region correlation flag based on the scope index of the referenced scope entry.
        /// </summary>
        private void SetScopeRegionCorrelationFlag(int scopeIndex)
        {
            GetDefiningScopeRegion(scopeIndex).WasResolutionCorrelated = true;
        }
        #endregion

        #region Group aggregate utils
        /// <summary>
        /// Enters processing of a function group aggregate.
        /// </summary>
        internal IDisposable EnterFunctionAggregate(AST.MethodExpr methodExpr, ErrorContext errCtx, out FunctionAggregateInfo aggregateInfo)
        {
            aggregateInfo = new FunctionAggregateInfo(methodExpr, errCtx, _currentGroupAggregateInfo, CurrentScopeRegion);
            return EnterGroupAggregate(aggregateInfo);
        }

        /// <summary>
        /// Enters processing of a group partition aggregate.
        /// </summary>
        internal IDisposable EnterGroupPartition(AST.GroupPartitionExpr groupPartitionExpr, ErrorContext errCtx, out GroupPartitionInfo aggregateInfo)
        {
            aggregateInfo = new GroupPartitionInfo(groupPartitionExpr, errCtx, _currentGroupAggregateInfo, CurrentScopeRegion);
            return EnterGroupAggregate(aggregateInfo);
        }

        /// <summary>
        /// Enters processing of a group partition aggregate.
        /// </summary>
        internal IDisposable EnterGroupKeyDefinition(GroupAggregateKind aggregateKind, ErrorContext errCtx, out GroupKeyAggregateInfo aggregateInfo)
        {
            aggregateInfo = new GroupKeyAggregateInfo(aggregateKind, errCtx, _currentGroupAggregateInfo, CurrentScopeRegion);
            return EnterGroupAggregate(aggregateInfo);
        }

        private IDisposable EnterGroupAggregate(GroupAggregateInfo aggregateInfo)
        {
            _currentGroupAggregateInfo = aggregateInfo;
            return new Disposer(delegate
                {
                    //
                    // First, pop the element from the stack to keep the stack valid...
                    //
                    Debug.Assert(this._currentGroupAggregateInfo == aggregateInfo, "Aggregare info stack is corrupted.");
                    this._currentGroupAggregateInfo = aggregateInfo.ContainingAggregate;

                    //
                    // ...then validate and seal the aggregate info.
                    // Note that this operation may throw an EntitySqlException.
                    //
                    aggregateInfo.ValidateAndComputeEvaluatingScopeRegion(this);
                });
        }
        #endregion

        #region Function overload resolution (untyped null aware)
        internal static EdmFunction ResolveFunctionOverloads(IList<EdmFunction> functionsMetadata,
                                                             IList<TypeUsage> argTypes,
                                                             bool isGroupAggregateFunction,
                                                             out bool isAmbiguous)
        {
            return FunctionOverloadResolver.ResolveFunctionOverloads(
                functionsMetadata,
                argTypes,
                UntypedNullAwareFlattenArgumentType,
                UntypedNullAwareFlattenParameterType,
                UntypedNullAwareIsPromotableTo,
                UntypedNullAwareIsStructurallyEqual,
                isGroupAggregateFunction,
                out isAmbiguous);
        }

        internal static TFunctionMetadata ResolveFunctionOverloads<TFunctionMetadata, TFunctionParameterMetadata>(
            IList<TFunctionMetadata> functionsMetadata,
            IList<TypeUsage> argTypes,
            Func<TFunctionMetadata, IList<TFunctionParameterMetadata>> getSignatureParams,
            Func<TFunctionParameterMetadata, TypeUsage> getParameterTypeUsage,
            Func<TFunctionParameterMetadata, ParameterMode> getParameterMode,
            bool isGroupAggregateFunction,
            out bool isAmbiguous) where TFunctionMetadata : class
        {
            return FunctionOverloadResolver.ResolveFunctionOverloads(
                functionsMetadata,
                argTypes,
                getSignatureParams,
                getParameterTypeUsage,
                getParameterMode,
                UntypedNullAwareFlattenArgumentType,
                UntypedNullAwareFlattenParameterType,
                UntypedNullAwareIsPromotableTo,
                UntypedNullAwareIsStructurallyEqual,
                isGroupAggregateFunction,
                out isAmbiguous);
        }

        private static IEnumerable<TypeUsage> UntypedNullAwareFlattenArgumentType(TypeUsage argType)
        {
            return argType != null ? TypeSemantics.FlattenType(argType) : new TypeUsage[] { null };
        }
        private static IEnumerable<TypeUsage> UntypedNullAwareFlattenParameterType(TypeUsage paramType, TypeUsage argType)
        {
            return argType != null ? TypeSemantics.FlattenType(paramType) : new TypeUsage[] { paramType };
        }
        private static bool UntypedNullAwareIsPromotableTo(TypeUsage fromType, TypeUsage toType)
        {
            if (fromType == null)
            {
                //
                // We can implicitly promote null to any type except collection.
                //
                return !Helper.IsCollectionType(toType.EdmType);
            }
            else
            {
                return TypeSemantics.IsPromotableTo(fromType, toType);
            }
        }
        private static bool UntypedNullAwareIsStructurallyEqual(TypeUsage fromType, TypeUsage toType)
        {
            if (fromType == null)
            {
                return UntypedNullAwareIsPromotableTo(fromType, toType);
            }
            else
            {
                return TypeSemantics.IsStructurallyEqual(fromType, toType);
            }
        }
        #endregion
    }

    /// <summary>
    /// Represents an utility for creating anonymous IDisposable implementations.
    /// </summary>
    internal class Disposer : IDisposable
    {
        private readonly Action _action;

        internal Disposer(Action action)
        {
            Debug.Assert(action != null, "action != null");
            _action = action;
        }

        public void Dispose()
        {
            _action();
            GC.SuppressFinalize(this);
        }
    }

    internal enum GroupAggregateKind
    {
        None,
        /// <summary>
        /// Inside of an aggregate function (Max, Min, etc).
        /// All range variables originating on the defining scope of this aggregate should yield <see cref="IGroupExpressionExtendedInfo.GroupVarBasedExpression"/>.
        /// </summary>
        Function,
        /// <summary>
        /// Inside of GROUPPARTITION expression.
        /// All range variables originating on the defining scope of this aggregate should yield <see cref="IGroupExpressionExtendedInfo.GroupAggBasedExpression"/>.
        /// </summary>
        Partition,
        /// <summary>
        /// Inside of a group key definition
        /// All range variables originating on the defining scope of this aggregate should yield <see cref="ScopeEntry.GetExpression"/>.
        /// </summary>
        GroupKey
    }

    /// <summary>
    /// Represents group aggregate information during aggregate construction/resolution.
    /// </summary>
    internal abstract class GroupAggregateInfo
    {
        protected GroupAggregateInfo(
            GroupAggregateKind aggregateKind, 
            AST.GroupAggregateExpr astNode,
            ErrorContext errCtx,
            GroupAggregateInfo containingAggregate,
            ScopeRegion definingScopeRegion)
        {
            Debug.Assert(aggregateKind != GroupAggregateKind.None, "aggregateKind != GroupAggregateKind.None");
            Debug.Assert(errCtx != null, "errCtx != null");
            Debug.Assert(definingScopeRegion != null, "definingScopeRegion != null");

            AggregateKind = aggregateKind;
            AstNode = astNode;
            ErrCtx = errCtx;
            DefiningScopeRegion = definingScopeRegion;
            SetContainingAggregate(containingAggregate);
        }

        protected void AttachToAstNode(string aggregateName, TypeUsage resultType)
        {
            Debug.Assert(AstNode != null, "AstNode must be set.");
            Debug.Assert(aggregateName != null && resultType != null, "aggregateName and aggregateDefinition must not be null.");
            Debug.Assert(AggregateName == null && AggregateStubExpression == null, "Cannot reattach.");

            AggregateName = aggregateName;
            AggregateStubExpression = resultType.Null();

            // Attach group aggregate info to the ast node.
            AstNode.AggregateInfo = this;
        }

        internal void DetachFromAstNode()
        {
            Debug.Assert(AstNode != null, "AstNode must be set.");
            AstNode.AggregateInfo = null;
        }

        /// <summary>
        /// Updates referenced scope index of the aggregate.
        /// Function call is not allowed after <see cref="ValidateAndComputeEvaluatingScopeRegion"/> has been called.
        /// </summary>
        internal void UpdateScopeIndex(int referencedScopeIndex, SemanticResolver sr)
        {
            Debug.Assert(_evaluatingScopeRegion == null, "Can not update referenced scope index after _evaluatingScopeRegion have been computed.");

            ScopeRegion referencedScopeRegion = sr.GetDefiningScopeRegion(referencedScopeIndex);

            if (_innermostReferencedScopeRegion == null ||
                _innermostReferencedScopeRegion.ScopeRegionIndex < referencedScopeRegion.ScopeRegionIndex)
            {
                _innermostReferencedScopeRegion = referencedScopeRegion;
            }
        }

        /// <summary>
        /// Gets/sets the innermost referenced scope region of the current aggregate.
        /// This property is used to save/restore the scope region value during a potentially throw-away attempt to
        /// convert an <see cref="AST.MethodExpr"/> as a collection function in the <see cref="SemanticAnalyzer.ConvertAggregateFunctionInGroupScope"/> method.
        /// Setting the value is not allowed after <see cref="ValidateAndComputeEvaluatingScopeRegion"/> has been called.
        /// </summary>
        internal ScopeRegion InnermostReferencedScopeRegion
        {
            get { return _innermostReferencedScopeRegion; }
            set
            {
                Debug.Assert(_evaluatingScopeRegion == null, "Can't change _innermostReferencedScopeRegion after _evaluatingScopeRegion has been initialized.");
                _innermostReferencedScopeRegion = value;
            }
        }
        private ScopeRegion _innermostReferencedScopeRegion;

        /// <summary>
        /// Validates the aggregate info and computes <see cref="EvaluatingScopeRegion"/> property.
        /// Seals the aggregate info object (no more AddContainedAggregate(...), RemoveContainedAggregate(...) and UpdateScopeIndex(...) calls allowed).
        /// </summary>
        internal void ValidateAndComputeEvaluatingScopeRegion(SemanticResolver sr)
        {
            Debug.Assert(_evaluatingScopeRegion == null, "_evaluatingScopeRegion has already been initialized");
            //
            // If _innermostReferencedScopeRegion is null, it means the aggregate is not correlated (a constant value),
            // so resolve it to the DefiningScopeRegion.
            //
            _evaluatingScopeRegion = _innermostReferencedScopeRegion ?? DefiningScopeRegion;

            if (!_evaluatingScopeRegion.IsAggregating)
            {
                //
                // In some cases the found scope region does not aggregate (has no grouping). So adding the aggregate to that scope won't work.
                // In this situation we need to backtrack from the found region to the first inner region that performs aggregation.
                // Example:
                // select yy.cx, yy.cy, yy.cz
                // from {1, 2} as x cross apply (select zz.cx, zz.cy, zz.cz
                //                               from {3, 4} as y cross apply (select Count(x) as cx, Count(y) as cy, Count(z) as cz
                //                                                             from {5, 6} as z) as zz
                //                              ) as yy
                // Note that Count aggregates cx and cy refer to scope regions that do aggregate. All three aggregates needs to be added to the only
                // aggregating region - the innermost.
                //
                int scopeRegionIndex = _evaluatingScopeRegion.ScopeRegionIndex;
                _evaluatingScopeRegion = null;
                foreach (ScopeRegion innerSR in sr.ScopeRegions.Skip(scopeRegionIndex))
                {
                    if (innerSR.IsAggregating)
                    {
                        _evaluatingScopeRegion = innerSR;
                        break;
                    }
                }
                if (_evaluatingScopeRegion == null)
                {
                    throw EntityUtil.EntitySqlError(Strings.GroupVarNotFoundInScope);
                }
            }

            //
            // Validate all the contained aggregates for violation of the containment rule:
            // None of the nested (contained) aggregates must be evaluating on a scope region that is 
            //      a. equal or inner to the evaluating scope of the current aggregate and
            //      b. equal or outer to the defining scope of the current aggregate.
            //
            // Example of a disallowed query:
            //
            //      select 
            //              (select max(x + max(y))
            //               from {1} as y)
            //      from {0} as x
            //
            // Example of an allowed query where the ESR of the nested aggregate is outer to the ESR of the outer aggregate:
            //
            //      select 
            //              (select max(y + max(x))
            //               from {1} as y)
            //      from {0} as x
            //
            // Example of an allowed query where the ESR of the nested aggregate is inner to the DSR of the outer aggregate:
            //
            //      select max(x + anyelement(select value max(y) from {1} as y))
            //      from {0} as x
            //
            Debug.Assert(_evaluatingScopeRegion.IsAggregating, "_evaluatingScopeRegion.IsAggregating must be true");
            Debug.Assert(_evaluatingScopeRegion.ScopeRegionIndex <= DefiningScopeRegion.ScopeRegionIndex, "_evaluatingScopeRegion must outer to the DefiningScopeRegion");
            ValidateContainedAggregates(_evaluatingScopeRegion.ScopeRegionIndex, DefiningScopeRegion.ScopeRegionIndex);
        }

        /// <summary>
        /// Recursively validates that <see cref="GroupAggregateInfo.EvaluatingScopeRegion"/> of all contained aggregates 
        /// is outside of the range of scope regions defined by <paramref name="outerBoundaryScopeRegionIndex"/> and <paramref name="innerBoundaryScopeRegionIndex"/>.
        /// Throws in the case of violation.
        /// </summary>
        private void ValidateContainedAggregates(int outerBoundaryScopeRegionIndex, int innerBoundaryScopeRegionIndex)
        {
            if (_containedAggregates != null)
            {
                foreach (GroupAggregateInfo containedAggregate in _containedAggregates)
                {
                    if (containedAggregate.EvaluatingScopeRegion.ScopeRegionIndex >= outerBoundaryScopeRegionIndex &&
                        containedAggregate.EvaluatingScopeRegion.ScopeRegionIndex <= innerBoundaryScopeRegionIndex)
                    {
                        int line, column;
                        string currentAggregateInfo = EntitySqlException.FormatErrorContext(
                            ErrCtx.CommandText,
                            ErrCtx.InputPosition,
                            ErrCtx.ErrorContextInfo,
                            ErrCtx.UseContextInfoAsResourceIdentifier,
                            out line, out column);

                        string nestedAggregateInfo = EntitySqlException.FormatErrorContext(
                            containedAggregate.ErrCtx.CommandText,
                            containedAggregate.ErrCtx.InputPosition,
                            containedAggregate.ErrCtx.ErrorContextInfo,
                            containedAggregate.ErrCtx.UseContextInfoAsResourceIdentifier,
                            out line, out column);

                        throw EntityUtil.EntitySqlError(Strings.NestedAggregateCannotBeUsedInAggregate(nestedAggregateInfo, currentAggregateInfo));
                    }

                    //
                    // We need to check the full subtree in order to catch this case:
                    //      select max(x +
                    //                     anyelement(select max(y + 
                    //                                               anyelement(select value max(x)
                    //                                               from {2} as z))
                    //                                from {1} as y))
                    //      from {0} as x
                    //
                    containedAggregate.ValidateContainedAggregates(outerBoundaryScopeRegionIndex, innerBoundaryScopeRegionIndex);
                }
            }
        }

        internal void SetContainingAggregate(GroupAggregateInfo containingAggregate)
        {
            if (_containingAggregate != null)
            {
                //
                // Aggregates in this query
                //
                //      select value max(anyelement(select value max(b + max(a + anyelement(select value c1 
                //                                                                          from {2} as c group by c as c1))) 
                //                                  from {1} as b group by b as b1)) 
                //
                //      from {0} as a group by a as a1
                //
                // are processed in the following steps:
                // 1.  the outermost aggregate (max1) begins processing as a collection function;
                // 2.  the middle aggregate (max2) begins processing as a collection function;
                // 3.  the innermost aggregate (max3) is processed as a collection function;
                // 4.  max3 is reprocessed as an aggregate; it does not see any containing aggregates at this point, so it's not wired up;
                //     max3 is validated and sealed;
                //     evaluating scope region for max3 is the outermost scope region, to which it gets assigned;
                //     max3 aggregate info object is attached to the corresponding AST node;
                // 5.  max2 completes processing as a collection function and begins processing as an aggregate;
                // 6.  max3 is reprocessed as an aggregate in the SemanticAnalyzer.TryConvertAsResolvedGroupAggregate(...) method, and 
                //     wired up to max2 as contained/containing;
                // 7.  max2 completes processing as an aggregate;
                //     max2 is validated and sealed;
                //     note that max2 does not see any containing aggregates at this point, so it's wired up only to max3;
                //     evaluating scope region for max2 is the middle scope region to which it gets assigned;
                // 6.  middle scope region completes processing, yields a DbExpression and cleans up all aggregate info objects assigned to it (max2);
                //     max2 is detached from the corresponding AST node;
                //     at this point max3 is still assigned to the outermost scope region and still wired to the dropped max2 as containing/contained;
                // 7.  max1 completes processing as a collection function and begins processing as an aggregate;
                // 8.  max2 is revisited and begins processing as a collection function (note that because the old aggregate info object for max2 was dropped 
                //     and detached from the AST node in step 6, SemanticAnalyzer.TryConvertAsResolvedGroupAggregate(...) does not recognize max2 as an aggregate);
                // 9.  max3 is recognized as an aggregate in the SemanticAnalyzer.TryConvertAsResolvedGroupAggregate(...) method;
                //     max3 is rewired from the dropped max2 (step 6) to max1 as contained/containing, now max1 and max3 are wired as containing/contained;
                // 10. max2 completes processing as a collection function and begins processing as an aggregate;
                //     max2 sees max1 as a containing aggregate and wires to it;
                // 11. max3 is reprocessed as resolved aggregate inside of TryConvertAsResolvedGroupAggregate(...) method;
                //     max3 is rewired from max1 to max2 as containing/contained aggregate;
                // 12. at this point max1 is wired to max2 and max2 is wired to max3, the tree is correct;
                //
                // ... both max1 and max3 are assigned to the same scope for evaluation, this is detected and an error is reported;
                //

                //
                // Remove this aggregate from the old containing aggregate before rewiring to the new parent.
                //
                _containingAggregate.RemoveContainedAggregate(this);
            }

            //
            // Accept the new parent and wire to it as a contained aggregate.
            //
            _containingAggregate = containingAggregate;
            if (_containingAggregate != null)
            {
                _containingAggregate.AddContainedAggregate(this);
            }
        }

        /// <summary>
        /// Function call is not allowed after <see cref="ValidateAndComputeEvaluatingScopeRegion"/> has been called.
        /// Adding new contained aggregate may invalidate the current aggregate.
        /// </summary>
        private void AddContainedAggregate(GroupAggregateInfo containedAggregate)
        {
            Debug.Assert(_evaluatingScopeRegion == null, "Can not add contained aggregate after _evaluatingScopeRegion have been computed.");

            if (_containedAggregates == null)
            {
                _containedAggregates = new List<GroupAggregateInfo>();
            }
            Debug.Assert(_containedAggregates.Contains(containedAggregate) == false, "containedAggregate is already registered");
            _containedAggregates.Add(containedAggregate);
        }
        private List<GroupAggregateInfo> _containedAggregates;

        /// <summary>
        /// Function call is _allowed_ after <see cref="ValidateAndComputeEvaluatingScopeRegion"/> has been called.
        /// Removing contained aggregates cannot invalidate the current aggregate.
        /// 
        /// Consider the following query:
        /// 
        ///   select value max(a + anyelement(select value max(b + max(a + anyelement(select value c1 
        ///                                                                           from {2} as c group by c as c1))) 
        ///                                   from {1} as b group by b as b1)) 
        ///   from {0} as a group by a as a1
        ///   
        /// Outer aggregate - max1, middle aggregate - max2, inner aggregate - max3.
        /// In this query after max1 have been processed as a collection function, max2 and max3 are wired as containing/contained.
        /// There is a point later when max1 is processed as an aggregate, max2 is processed as a collection function and max3 is processed as
        /// an aggregate. Note that at this point the "aggregate" version of max2 is dropped and detached from the AST node when the middle scope region 
        /// completes processing; also note that because evaluating scope region of max3 is the outer scope region, max3 aggregate info is still attached to 
        /// the AST node and it is still wired to the dropped aggregate info object of max2. At this point max3 does not see new max2 as a containing aggregate, 
        /// and it rewires to max1, during this rewiring it needs to to remove itself from the old max2 and add itself to max1.
        /// The old max2 at this point is sealed, so the removal is performed on the sealed object.
        /// </summary>
        private void RemoveContainedAggregate(GroupAggregateInfo containedAggregate)
        {
            Debug.Assert(_containedAggregates != null && _containedAggregates.Contains(containedAggregate), "_containedAggregates.Contains(containedAggregate)");

            _containedAggregates.Remove(containedAggregate);
        }

        internal readonly GroupAggregateKind AggregateKind;
        
        /// <summary>
        /// Null when <see cref="GroupAggregateInfo"/> is created for a group key processing.
        /// </summary>
        internal readonly AST.GroupAggregateExpr AstNode;
        
        internal readonly ErrorContext ErrCtx;
        
        /// <summary>
        /// Scope region that contains the aggregate expression.
        /// </summary>
        internal readonly ScopeRegion DefiningScopeRegion;

        /// <summary>
        /// Scope region that evaluates the aggregate expression.
        /// </summary>
        internal ScopeRegion EvaluatingScopeRegion
        {
            get
            {
                //
                // _evaluatingScopeRegion is initialized in the ValidateAndComputeEvaluatingScopeRegion(...) method.
                //
                Debug.Assert(_evaluatingScopeRegion != null, "_evaluatingScopeRegion is not initialized");
                return _evaluatingScopeRegion;
            }
        }
        private ScopeRegion _evaluatingScopeRegion;

        /// <summary>
        /// Parent aggregate expression that contains the current aggregate expression.
        /// May be null.
        /// </summary>
        internal GroupAggregateInfo ContainingAggregate
        {
            get { return _containingAggregate; }
        }
        private GroupAggregateInfo _containingAggregate;

        internal string AggregateName;
        internal DbNullExpression AggregateStubExpression;
    }

    internal sealed class FunctionAggregateInfo : GroupAggregateInfo
    {
        internal FunctionAggregateInfo(AST.MethodExpr methodExpr, ErrorContext errCtx, GroupAggregateInfo containingAggregate, ScopeRegion definingScopeRegion)
            : base(GroupAggregateKind.Function, methodExpr, errCtx, containingAggregate, definingScopeRegion) 
        {
            Debug.Assert(methodExpr != null, "methodExpr != null");
        }

        internal void AttachToAstNode(string aggregateName, DbAggregate aggregateDefinition)
        {
            Debug.Assert(aggregateDefinition != null, "aggregateDefinition != null");
            base.AttachToAstNode(aggregateName, aggregateDefinition.ResultType);
            AggregateDefinition = aggregateDefinition;
        }

        internal DbAggregate AggregateDefinition;
    }

    internal sealed class GroupPartitionInfo : GroupAggregateInfo
    {
        internal GroupPartitionInfo(AST.GroupPartitionExpr groupPartitionExpr, ErrorContext errCtx, GroupAggregateInfo containingAggregate, ScopeRegion definingScopeRegion)
            : base(GroupAggregateKind.Partition, groupPartitionExpr, errCtx, containingAggregate, definingScopeRegion)
        {
            Debug.Assert(groupPartitionExpr != null, "groupPartitionExpr != null");
        }

        internal void AttachToAstNode(string aggregateName, DbExpression aggregateDefinition)
        {
            Debug.Assert(aggregateDefinition != null, "aggregateDefinition != null");
            base.AttachToAstNode(aggregateName, aggregateDefinition.ResultType);
            AggregateDefinition = aggregateDefinition;
        }

        internal DbExpression AggregateDefinition;
    }

    internal sealed class GroupKeyAggregateInfo : GroupAggregateInfo
    {
        internal GroupKeyAggregateInfo(GroupAggregateKind aggregateKind, ErrorContext errCtx, GroupAggregateInfo containingAggregate, ScopeRegion definingScopeRegion)
            : base(aggregateKind, null /* there is no AST.GroupAggregateExpression corresponding to the group key */, errCtx, containingAggregate, definingScopeRegion)
        { }
    }

    internal abstract class InlineFunctionInfo
    {
        internal InlineFunctionInfo(AST.FunctionDefinition functionDef, List<DbVariableReferenceExpression> parameters)
        {
            FunctionDefAst = functionDef;
            Parameters = parameters;
        }

        internal readonly AST.FunctionDefinition FunctionDefAst;
        internal readonly List<DbVariableReferenceExpression> Parameters;

        internal abstract DbLambda GetLambda(SemanticResolver sr);
    }

    internal sealed class ScopeRegion
    {
        private readonly ScopeManager _scopeManager;

        internal ScopeRegion(ScopeManager scopeManager, int firstScopeIndex, int scopeRegionIndex)
        {
            _scopeManager = scopeManager;
            _firstScopeIndex = firstScopeIndex;
            _scopeRegionIndex = scopeRegionIndex;
        }

        /// <summary>
        /// First scope of the region.
        /// </summary>
        internal int FirstScopeIndex
        {
            get { return _firstScopeIndex; }
        }
        private readonly int _firstScopeIndex;

        /// <summary>
        /// Index of the scope region. 
        /// Outer scope regions have smaller index value than inner scope regions.
        /// </summary>
        internal int ScopeRegionIndex
        {
            get { return _scopeRegionIndex; }
        }
        private readonly int _scopeRegionIndex;

        /// <summary>
        /// True if given scope is in the current scope region.
        /// </summary>
        internal bool ContainsScope(int scopeIndex)
        {
            return (scopeIndex >= _firstScopeIndex);
        }

        /// <summary>
        /// Marks current scope region as performing group/folding operation.
        /// </summary>
        internal void EnterGroupOperation(DbExpressionBinding groupAggregateBinding)
        {
            Debug.Assert(!IsAggregating, "Scope region group operation is not reentrant.");
            _groupAggregateBinding = groupAggregateBinding;
        }
        /// <summary>
        /// Clears the <see cref="IsAggregating"/> flag on the group scope.
        /// </summary>
        internal void RollbackGroupOperation()
        {
            Debug.Assert(IsAggregating, "Scope region must inside group operation in order to leave it.");
            _groupAggregateBinding = null;
        }
        /// <summary>
        /// True when the scope region performs group/folding operation.
        /// </summary>
        internal bool IsAggregating
        {
            get { return _groupAggregateBinding != null; }
        }
        internal DbExpressionBinding GroupAggregateBinding
        {
            get
            {
                Debug.Assert(IsAggregating, "IsAggregating must be true.");
                return _groupAggregateBinding;
            }
        }
        private DbExpressionBinding _groupAggregateBinding;

        /// <summary>
        /// Returns list of group aggregates evaluated on the scope region.
        /// </summary>
        internal List<GroupAggregateInfo> GroupAggregateInfos
        {
            get { return _groupAggregateInfos; }
        }
        private List<GroupAggregateInfo> _groupAggregateInfos = new List<GroupAggregateInfo>();

        /// <summary>
        /// Adds group aggregate name to the scope region.
        /// </summary>
        internal void RegisterGroupAggregateName(string groupAggregateName)
        {
            Debug.Assert(!_groupAggregateNames.Contains(groupAggregateName), "!_groupAggregateNames.ContainsKey(groupAggregateName)");
            _groupAggregateNames.Add(groupAggregateName);
        }
        internal bool ContainsGroupAggregate(string groupAggregateName)
        {
            return _groupAggregateNames.Contains(groupAggregateName);
        }
        private HashSet<string> _groupAggregateNames = new HashSet<string>();

        /// <summary>
        /// True if a recent expression resolution was correlated.
        /// </summary>
        internal bool WasResolutionCorrelated
        {
            get { return _wasResolutionCorrelated; }
            set { _wasResolutionCorrelated = value; }
        }
        private bool _wasResolutionCorrelated = false;

        /// <summary>
        /// Applies <paramref name="action"/> to all scope entries in the current scope region.
        /// </summary>
        internal void ApplyToScopeEntries(Action<ScopeEntry> action)
        {
            Debug.Assert(FirstScopeIndex <= _scopeManager.CurrentScopeIndex, "FirstScopeIndex <= CurrentScopeIndex");

            for (int i = FirstScopeIndex; i <= _scopeManager.CurrentScopeIndex; ++i)
            {
                foreach (KeyValuePair<string, ScopeEntry> scopeEntry in _scopeManager.GetScopeByIndex(i))
                {
                    action(scopeEntry.Value);
                }
            }
        }

        /// <summary>
        /// Applies <paramref name="action"/> to all scope entries in the current scope region.
        /// </summary>
        internal void ApplyToScopeEntries(Func<ScopeEntry, ScopeEntry> action)
        {
            Debug.Assert(FirstScopeIndex <= _scopeManager.CurrentScopeIndex, "FirstScopeIndex <= CurrentScopeIndex");

            for (int i = FirstScopeIndex; i <= _scopeManager.CurrentScopeIndex; ++i)
            {
                Scope scope = _scopeManager.GetScopeByIndex(i);
                List<KeyValuePair<string, ScopeEntry>> updatedEntries = null;
                foreach (KeyValuePair<string, ScopeEntry> scopeEntry in scope)
                {
                    ScopeEntry newScopeEntry = action(scopeEntry.Value);
                    Debug.Assert(newScopeEntry != null, "newScopeEntry != null");
                    if (scopeEntry.Value != newScopeEntry)
                    {
                        if (updatedEntries == null)
                        {
                            updatedEntries = new List<KeyValuePair<string, ScopeEntry>>();
                        }
                        updatedEntries.Add(new KeyValuePair<string, ScopeEntry>(scopeEntry.Key, newScopeEntry));
                    }
                }
                if (updatedEntries != null)
                {
                    updatedEntries.ForEach((updatedScopeEntry) => scope.Replace(updatedScopeEntry.Key, updatedScopeEntry.Value));
                }
            }
        }

        internal void RollbackAllScopes()
        {
            _scopeManager.RollbackToScope(FirstScopeIndex - 1);
        }
    }

    /// <summary>
    /// Represents a pair of types to avoid uncessary enumerations to split kvp elements
    /// </summary>
    internal sealed class Pair<L, R>
    {
        internal Pair(L left, R right)
        {
            Left = left;
            Right = right;
        }

        internal L Left;
        internal R Right;

        internal KeyValuePair<L, R> GetKVP()
        {
            return new KeyValuePair<L, R>(Left, Right);
        }
    }
}
