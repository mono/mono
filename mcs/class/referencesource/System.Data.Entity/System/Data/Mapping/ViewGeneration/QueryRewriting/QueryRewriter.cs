//---------------------------------------------------------------------
// <copyright file="QueryRewriter.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner [....]
// @backupOwner [....]
//---------------------------------------------------------------------

namespace System.Data.Mapping.ViewGeneration.QueryRewriting
{
    using System.Collections.Generic;
    using System.Data.Common.Utils;
    using System.Data.Entity;
    using System.Data.Mapping.ViewGeneration.Structures;
    using System.Data.Mapping.ViewGeneration.Utils;
    using System.Data.Mapping.ViewGeneration.Validation;
    using System.Data.Metadata.Edm;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Uses query rewriting to determine the case statements, top-level WHERE clause, and the "used views"
    /// for a given type to be generated.
    /// 
    /// Step 1: Method "EnsureIsFullyMapped" goes through the (C) schema metadata and checks whether the query for each
    ///         entity shape can be rewritten from the C fragment queries.
    ///         This step tracks the "used views" which will later be passed to "basic view generation" (i.e., creation of the FOJ/LOJ/IJ/Union relational expressions)
    /// Step 2: GetCaseStatements constructs the required case statements and the top-level WHERE clause.
    ///         This may add some extra views to "used views".
    ///         Now we know what views are used overall.
    /// Step 3: We remap _from variables to new _from variables that are renumbered for used views.
    ///         This is done to comply with the numbering scheme in the old algorithm - and to produce more readable views.
    /// Step 4: From the constructed relational expression (OpCellTree), we can tell whether a top-level WHERE clause is needed or not.
    ///         (Usually, it's needed only in certain cases for OfType() views.)
    /// </summary>
    internal class QueryRewriter
    {
        #region Fields

        // The following fields are copied from ViewGenContext
        MemberPath _extentPath;
        MemberDomainMap _domainMap;
        ConfigViewGenerator _config;
        CqlIdentifiers _identifiers;
        ViewgenContext _context;

        // Keeps track of statistics
        RewritingProcessor<Tile<FragmentQuery>> _qp;
        // Key attributes of the current extent in _extentPath
        List<MemberPath> _keyAttributes;
        // Fragment queries, one per LeftCellWrapper
        List<FragmentQuery> _fragmentQueries = new List<FragmentQuery>();
        List<Tile<FragmentQuery>> _views = new List<Tile<FragmentQuery>>();

        FragmentQuery _domainQuery;
        EdmType _generatedType;
        HashSet<FragmentQuery> _usedViews = new HashSet<FragmentQuery>();
        List<LeftCellWrapper> _usedCells = new List<LeftCellWrapper>();
        BoolExpression _topLevelWhereClause;
        CellTreeNode _basicView;
        Dictionary<MemberPath, CaseStatement> _caseStatements = new Dictionary<MemberPath, CaseStatement>();
        ErrorLog _errorLog = new ErrorLog();
        ViewGenMode _typesGenerationMode;

        #endregion

        #region Static variables

        static Tile<FragmentQuery> TrueViewSurrogate = CreateTile(FragmentQuery.Create(BoolExpression.True));

        #endregion

        #region Constructor and main entry point

        internal QueryRewriter(EdmType generatedType, ViewgenContext context, ViewGenMode typesGenerationMode)
        {
            Debug.Assert(typesGenerationMode != ViewGenMode.GenerateAllViews);

            _typesGenerationMode = typesGenerationMode;
            _context = context;
            _generatedType = generatedType;
            _domainMap = context.MemberMaps.LeftDomainMap;
            _config = context.Config;
            _identifiers = context.CqlIdentifiers;
            _qp = new RewritingProcessor<Tile<FragmentQuery>>(new DefaultTileProcessor<FragmentQuery>(context.LeftFragmentQP));
            _extentPath = new MemberPath(context.Extent);
            _keyAttributes = new List<MemberPath>(MemberPath.GetKeyMembers(context.Extent, _domainMap));

            // populate _fragmentQueries and _views
            foreach (LeftCellWrapper leftCellWrapper in _context.AllWrappersForExtent)
            {
                FragmentQuery query = leftCellWrapper.FragmentQuery;
                Tile<FragmentQuery> tile = CreateTile(query);
                _fragmentQueries.Add(query);
                _views.Add(tile);
            }
            Debug.Assert(_views.Count > 0);

            AdjustMemberDomainsForUpdateViews();

            // must be done after adjusting domains
            _domainQuery = GetDomainQuery(FragmentQueries, generatedType);

            _usedViews = new HashSet<FragmentQuery>();
        }

        // Generates the components used to assemble and validate the view:
        // (1) case statements
        // (2) top-level where clause
        // (3) used cells
        // (4) basic view CellTreeNode
        // (5) dictionary<MemberValue, CellTreeNode> for validation
        internal void GenerateViewComponents()
        {
            // make sure everything is mapped (for query views only)
            EnsureExtentIsFullyMapped(_usedViews);
            
            // (1) case statements
            GenerateCaseStatements(_domainMap.ConditionMembers(_extentPath.Extent), _usedViews);

            AddTrivialCaseStatementsForConditionMembers();

            if (_usedViews.Count == 0 || _errorLog.Count > 0)
            {
                // can't continue: no view will be generated, further validation doesn't make sense
                Debug.Assert(_errorLog.Count > 0);
                ExceptionHelpers.ThrowMappingException(_errorLog, _config);
            }

            // (2) top-level where clause
            _topLevelWhereClause = GetTopLevelWhereClause(_usedViews);

            // some tracing
            if (_context.ViewTarget == ViewTarget.QueryView)
            {
                TraceVerbose("Used {0} views of {1} total for rewriting", _usedViews.Count, _views.Count);
            }
            PrintStatistics(_qp);

            // (3) construct the final _from variables
            _usedCells = RemapFromVariables();

            // (4) construct basic view
            BasicViewGenerator basicViewGenerator = new BasicViewGenerator(
                _context.MemberMaps.ProjectedSlotMap, _usedCells,
                _domainQuery, _context, _domainMap, _errorLog, _config);

            _basicView = basicViewGenerator.CreateViewExpression();

            // a top-level WHERE clause is needed only if the simplifiedView still contains extra tuples
            bool noWhereClauseNeeded = _context.LeftFragmentQP.IsContainedIn(_basicView.LeftFragmentQuery, _domainQuery);
            if (noWhereClauseNeeded)
            {
                _topLevelWhereClause = BoolExpression.True;
            }

            if (_errorLog.Count > 0)
            {
                ExceptionHelpers.ThrowMappingException(_errorLog, _config);
            }
        }

        #endregion

        #region Properties

        internal ViewgenContext ViewgenContext
        {
            get { return _context; }
        }

        internal Dictionary<MemberPath, CaseStatement> CaseStatements
        {
            get { return _caseStatements; }
        }

        internal BoolExpression TopLevelWhereClause
        {
            get { return _topLevelWhereClause; }
        }

        internal CellTreeNode BasicView
        {
            get
            {
                // create a copy so the original won't get modified when Simplifier.Simplify is called on it
                return _basicView.MakeCopy();
            }
        }

        internal List<LeftCellWrapper> UsedCells
        {
            get { return _usedCells; }
        }

        private IEnumerable<FragmentQuery> FragmentQueries
        {
            get { return _fragmentQueries; }
        }

        #endregion

        #region Main logic

        private IEnumerable<Constant> GetDomain(MemberPath currentPath)
        {
            if (_context.ViewTarget == ViewTarget.QueryView && MemberPath.EqualityComparer.Equals(currentPath, _extentPath))
            {
                IEnumerable<EdmType> types;
                if (_typesGenerationMode == ViewGenMode.OfTypeOnlyViews)
                {
                    Debug.Assert(!Helper.IsRefType(_generatedType));
                    HashSet<EdmType> type = new HashSet<EdmType>();
                    type.Add(_generatedType);
                    types = type;
                }
                else
                {
                    types = MetadataHelper.GetTypeAndSubtypesOf(_generatedType, _context.EdmItemCollection, false /* don't include abstract types */);
                }
                return GetTypeConstants(types);
            }
            return _domainMap.GetDomain(currentPath);
        }

        // NULL/default and NOT(...) values in cell constant domains for update views may be unused.
        // If we don't detect that and remove them, we can suboptimal (but still correct) update views.
        // (For example, SProducts1 in NotNullCorrect.msl has an unused constant NOT("Camera", NULL), which results in a gratuitous join.
        // That join could be eliminated due to 1:1 association on C side).
        // To determine that a constant is unused, we first try to obtain the S-side rewriting for it.
        // If that succeeds, we unfold C-queries, i.e., create OpCellTree for found rewritings,
        // and check whether these are unsatisfiable.
        // If they indeed are unsatisfiable, we eliminate the constants from the domainMap.
        private void AdjustMemberDomainsForUpdateViews()
        {
            switch (_context.ViewTarget)
            {
                case ViewTarget.UpdateView:
                    {
                        // materialize members in a list so we can modify _domainMap later on
                        List<MemberPath> members = new List<MemberPath>(_domainMap.ConditionMembers(_extentPath.Extent));
                        foreach (MemberPath currentPath in members)
                        {
                            // try to remove default value followed by negated value, in this order
                            IEnumerable<Constant> oldDomain = _domainMap.GetDomain(currentPath);
                            Constant defaultValue = oldDomain.FirstOrDefault(domainValue => IsDefaultValue(domainValue, currentPath));
                            if (defaultValue != null)
                            {
                                RemoveUnusedValueFromStoreDomain(defaultValue, currentPath);
                            }
                            oldDomain = _domainMap.GetDomain(currentPath); // is case has changed
                            Constant negatedValue = oldDomain.FirstOrDefault(domainValue => domainValue is NegatedConstant);
                            if (negatedValue != null)
                            {
                                RemoveUnusedValueFromStoreDomain(negatedValue, currentPath);
                            }
                        }
                        break;
                    }
            }
        }

        private void RemoveUnusedValueFromStoreDomain(Constant domainValue, MemberPath currentPath)
        {
            // construct WHERE clause for this value
            BoolExpression domainWhereClause = CreateMemberCondition(currentPath, domainValue);

            // get a rewriting for CASE statements by not requesting any attributes beyond key 
            Tile<FragmentQuery> caseRewriting;
            HashSet<FragmentQuery> outputUsedViews = new HashSet<FragmentQuery>();
            bool isUsedValue = false;
            if (FindRewritingAndUsedViews(_keyAttributes, domainWhereClause, outputUsedViews, out caseRewriting))
            {
                // check whether this rewriting is indeed satisfiable using C-side fragment views
                // If we wanted to force retention of all negated constants, we could use:
                // if (domainValue is NegatedCellConstant) { isUsedValue = true; } else {...}
                CellTreeNode cellTree = TileToCellTree((Tile<FragmentQuery>)caseRewriting, _context);
                isUsedValue = !cellTree.IsEmptyRightFragmentQuery;
            }

            if (!isUsedValue)
            {
                Set<Constant> newDomain = new Set<Constant>(_domainMap.GetDomain(currentPath), Constant.EqualityComparer);
                newDomain.Remove(domainValue);
                TraceVerbose("Shrunk domain of column {0} from {1} to {2}", currentPath, _domainMap.GetDomain(currentPath), newDomain);
                _domainMap.UpdateConditionMemberDomain(currentPath, newDomain);
                // Update the WHERE clauses of all fragment queries
                // Since these are pointers to the respective WHERE clauses in S-side cell queries, those get updated automatically
                foreach (FragmentQuery query in _fragmentQueries)
                {
                    query.Condition.FixDomainMap(_domainMap);
                }
            }
        }

        // determine the domain query, i.e., the query that returns all keys of the extent to be populated
        internal FragmentQuery GetDomainQuery(IEnumerable<FragmentQuery> fragmentQueries, EdmType generatedType)
        {
            BoolExpression domainQueryCondition = null;
            if (_context.ViewTarget == ViewTarget.QueryView)
            {
                if (generatedType == null)
                {
                    // domainQuery for entire extent: True
                    domainQueryCondition = BoolExpression.True;
                }
                else // domainQuery for specific type: WHERE type(path) IS OF (Type)
                {
                    //If Mode is OFTypeOnlyViews then don't get subtypes
                    IEnumerable<EdmType> derivedTypes;
                    if (_typesGenerationMode == ViewGenMode.OfTypeOnlyViews)
                    {
                        Debug.Assert(!Helper.IsRefType(_generatedType));
                        HashSet<EdmType> type = new HashSet<EdmType>();
                        type.Add(_generatedType);
                        derivedTypes = type;
                    }
                    else
                    {
                        derivedTypes = MetadataHelper.GetTypeAndSubtypesOf(generatedType, _context.EdmItemCollection, false /* don't include abstract types */);
                    }

                    Domain typeDomain = new Domain(GetTypeConstants(derivedTypes), _domainMap.GetDomain(_extentPath));
                    domainQueryCondition = BoolExpression.CreateLiteral(new TypeRestriction(new MemberProjectedSlot(_extentPath), typeDomain), _domainMap);
                }
                return FragmentQuery.Create(_keyAttributes, domainQueryCondition);
            }
            else // for update views, domain query = exposed tiles
            {
                IEnumerable<BoolExpression> whereClauses = from fragmentQuery in fragmentQueries
                                                           select fragmentQuery.Condition;

                BoolExpression exposedRegionCondition = BoolExpression.CreateOr(whereClauses.ToArray());
                return FragmentQuery.Create(_keyAttributes, exposedRegionCondition);
            }
        }

        // returns true when the case statement is completed
        private bool AddRewritingToCaseStatement(Tile<FragmentQuery> rewriting, CaseStatement caseStatement, MemberPath currentPath, Constant domainValue)
        {
            BoolExpression whenCondition = BoolExpression.True;
            // check whether the rewriting is always true or always false
            // if it's always true, we don't need any other WHEN clauses in the case statement
            // if it's always false, we don't need to add this WHEN clause to the case statement
            // given: domainQuery is satisfied. Check (domainQuery -> rewriting)
            bool isAlwaysTrue = _qp.IsContainedIn(CreateTile(_domainQuery), rewriting);
            bool isAlwaysFalse = _qp.IsDisjointFrom(CreateTile(_domainQuery), rewriting);
            Debug.Assert(!(isAlwaysTrue && isAlwaysFalse));
            if (isAlwaysFalse)
            {
                return false; // don't need an unsatisfiable WHEN clause
            }
            if (isAlwaysTrue)
            {
                Debug.Assert(caseStatement.Clauses.Count == 0);
            }

            ProjectedSlot projectedSlot;
            if (domainValue.HasNotNull())
            {
                projectedSlot = new MemberProjectedSlot(currentPath);
            }
            else
            {
                projectedSlot = new ConstantProjectedSlot(domainValue, currentPath);
            }

            if (!isAlwaysTrue)
            {
                whenCondition = TileToBoolExpr((Tile<FragmentQuery>)rewriting);
            }
            else
            {
                whenCondition = BoolExpression.True;
            }
            caseStatement.AddWhenThen(whenCondition, projectedSlot);

            return isAlwaysTrue;
        }

        // make sure that we can find a rewriting for each possible entity shape appearing in an extent
        // Possible optimization for OfType view generation:
        // Cache "used views" for each (currentPath, domainValue) combination
        private void EnsureConfigurationIsFullyMapped(MemberPath currentPath,
                                                      BoolExpression currentWhereClause,
                                                      HashSet<FragmentQuery> outputUsedViews,
                                                      ErrorLog errorLog)
        {
            foreach (Constant domainValue in GetDomain(currentPath))
            {
                if (domainValue == Constant.Undefined)
                {
                    continue; // no point in trying to recover a situation that can never happen
                }
                TraceVerbose("REWRITING FOR {0}={1}", currentPath, domainValue);

                // construct WHERE clause for this value
                BoolExpression domainAddedWhereClause = CreateMemberCondition(currentPath, domainValue);
                // AND the current where clause to it
                BoolExpression domainWhereClause = BoolExpression.CreateAnd(currentWhereClause, domainAddedWhereClause);

                // first check whether we can recover instances of this type - don't care about the attributes - to produce a helpful error message
                Tile<FragmentQuery> rewriting;
                if (false == FindRewritingAndUsedViews(_keyAttributes, domainWhereClause, outputUsedViews, out rewriting))
                {
                    if (!ErrorPatternMatcher.FindMappingErrors(_context, _domainMap, _errorLog))
                    {
                        StringBuilder builder = new StringBuilder();
                        string extentName = StringUtil.FormatInvariant("{0}", _extentPath);
                        BoolExpression whereClause = rewriting.Query.Condition;
                        whereClause.ExpensiveSimplify();
                        if (whereClause.RepresentsAllTypeConditions)
                        {
                            string tableString = Strings.ViewGen_Extent;
                            builder.AppendLine(Strings.ViewGen_Cannot_Recover_Types(tableString, extentName));
                        }
                        else
                        {
                            string entitiesString = Strings.ViewGen_Entities;
                            builder.AppendLine(Strings.ViewGen_Cannot_Disambiguate_MultiConstant(entitiesString, extentName));
                        }
                        RewritingValidator.EntityConfigurationToUserString(whereClause, builder);
                        ErrorLog.Record record = new ErrorLog.Record(true, ViewGenErrorCode.AmbiguousMultiConstants, builder.ToString(), _context.AllWrappersForExtent, String.Empty);
                        errorLog.AddEntry(record);
                    }
                }
                else
                {
                    TypeConstant typeConstant = domainValue as TypeConstant;
                    if (typeConstant != null)
                    {
                        // we are enumerating types
                        EdmType edmType = typeConstant.EdmType;
                        // If can recover the type, make sure can get all the necessary attributes (key is included for EntityTypes)

                        List<MemberPath> nonConditionalAttributes = GetNonConditionalScalarMembers(edmType, currentPath, _domainMap).Union(GetNonConditionalComplexMembers(edmType, currentPath, _domainMap)).ToList();
                        IEnumerable<MemberPath> notCoverdAttributes;
                        if (nonConditionalAttributes.Count > 0 &&
                            !FindRewritingAndUsedViews(nonConditionalAttributes, domainWhereClause, outputUsedViews, out rewriting, out notCoverdAttributes))
                        {
                            //Error: No mapping specified for some attributes
                            // remove keys
                            nonConditionalAttributes = new List<MemberPath>(nonConditionalAttributes.Where(a => !a.IsPartOfKey));
                            Debug.Assert(nonConditionalAttributes.Count > 0, "Must have caught key-only case earlier");

                            AddUnrecoverableAttributesError(notCoverdAttributes, domainAddedWhereClause, errorLog);
                        }
                        else
                        {
                            // recurse into complex members
                            foreach (MemberPath complexMember in GetConditionalComplexMembers(edmType, currentPath, _domainMap))
                            {
                                EnsureConfigurationIsFullyMapped(complexMember, domainWhereClause, outputUsedViews, errorLog);
                            }
                            // recurse into scalar members
                            foreach (MemberPath scalarMember in GetConditionalScalarMembers(edmType, currentPath, _domainMap))
                            {
                                EnsureConfigurationIsFullyMapped(scalarMember, domainWhereClause, outputUsedViews, errorLog);
                            }
                        }
                    }
                }
            }
        }

        private static List<String> GetTypeBasedMemberPathList(IEnumerable<MemberPath> nonConditionalScalarAttributes)
        {
            Debug.Assert(nonConditionalScalarAttributes != null);
            List<String> typeBasedMembers = new List<string>();
            foreach (MemberPath memberPath in nonConditionalScalarAttributes)
            {
                EdmMember member = memberPath.LeafEdmMember;
                typeBasedMembers.Add(member.DeclaringType.Name + "." + member);
            }
            return typeBasedMembers;
        }

        private void AddUnrecoverableAttributesError(IEnumerable<MemberPath> attributes, BoolExpression domainAddedWhereClause, ErrorLog errorLog)
        {
            StringBuilder builder = new StringBuilder();
            string extentName = StringUtil.FormatInvariant("{0}", _extentPath);
            string tableString = Strings.ViewGen_Extent;
            string attributesString = StringUtil.ToCommaSeparatedString(GetTypeBasedMemberPathList(attributes));
            builder.AppendLine(Strings.ViewGen_Cannot_Recover_Attributes(attributesString, tableString, extentName));
            RewritingValidator.EntityConfigurationToUserString(domainAddedWhereClause, builder);
            ErrorLog.Record record = new ErrorLog.Record(true, ViewGenErrorCode.AttributesUnrecoverable, builder.ToString(), _context.AllWrappersForExtent, String.Empty);
            errorLog.AddEntry(record);
        }


        private void GenerateCaseStatements(IEnumerable<MemberPath> members,
                                            HashSet<FragmentQuery> outputUsedViews)
        {
            // Compute right domain query - non-simplified version of "basic view"
            // It is used below to check whether we need a default value in a case statement
            IEnumerable<LeftCellWrapper> usedCells = _context.AllWrappersForExtent.Where(w => _usedViews.Contains(w.FragmentQuery));
            CellTreeNode rightDomainQuery = new OpCellTreeNode(
                _context, CellTreeOpType.Union,
                usedCells.Select(wrapper => new LeafCellTreeNode(_context, wrapper)).ToArray());

            foreach (MemberPath currentPath in members)
            {
                // Add the types can member have, i.e., its type and its subtypes
                List<Constant> domain = GetDomain(currentPath).ToList();
                CaseStatement caseStatement = new CaseStatement(currentPath);

                Tile<FragmentQuery> unionCaseRewriting = null;

                // optimization for domain = {NULL, NOT_NULL}
                // Create a single case: WHEN True THEN currentPath
                // Reason: if the WHEN condition is not satisfied (say because of LOJ), then currentPath = NULL
                bool needCaseStatement =
                  !(domain.Count == 2 &&
                    domain.Contains(Constant.Null, Constant.EqualityComparer) &&
                    domain.Contains(Constant.NotNull, Constant.EqualityComparer));
                {
                    // go over the domain
                    foreach (Constant domainValue in domain)
                    {
                        if (domainValue == Constant.Undefined && _context.ViewTarget == ViewTarget.QueryView)
                        {
                            // we cannot assume closed domain for query views;
                            // if obtaining undefined is possible, we need to account for that
                            caseStatement.AddWhenThen(BoolExpression.False /* arbitrary condition */,
                                                      new ConstantProjectedSlot(Constant.Undefined, currentPath));
                            continue;
                        }
                        TraceVerbose("CASE STATEMENT FOR {0}={1}", currentPath, domainValue);

                        // construct WHERE clause for this value
                        FragmentQuery memberConditionQuery = CreateMemberConditionQuery(currentPath, domainValue);

                        Tile<FragmentQuery> caseRewriting;
                        if (FindRewritingAndUsedViews(memberConditionQuery.Attributes, memberConditionQuery.Condition, outputUsedViews, out caseRewriting))
                        {
                            if (_context.ViewTarget == ViewTarget.UpdateView)
                            {
                                unionCaseRewriting = (unionCaseRewriting != null) ? _qp.Union(unionCaseRewriting, caseRewriting) : caseRewriting;
                            }

                            if (needCaseStatement)
                            {
                                bool isAlwaysTrue = AddRewritingToCaseStatement(caseRewriting, caseStatement, currentPath, domainValue);
                                if (isAlwaysTrue)
                                {
                                    break;
                                }
                            }
                        }
                        else
                        {
                            if (!IsDefaultValue(domainValue, currentPath))
                            {
                                Debug.Assert(_context.ViewTarget == ViewTarget.UpdateView || !_config.IsValidationEnabled);


                                if (!ErrorPatternMatcher.FindMappingErrors(_context, _domainMap, _errorLog))
                                {
                                    StringBuilder builder = new StringBuilder();
                                    string extentName = StringUtil.FormatInvariant("{0}", _extentPath);
                                    string objectString = _context.ViewTarget == ViewTarget.QueryView ?
                                        Strings.ViewGen_Entities : Strings.ViewGen_Tuples;
                                    
                                    if (_context.ViewTarget == ViewTarget.QueryView)
                                    {
                                        builder.AppendLine(Strings.Viewgen_CannotGenerateQueryViewUnderNoValidation(extentName));
                                    }
                                    else
                                    {
                                        builder.AppendLine(Strings.ViewGen_Cannot_Disambiguate_MultiConstant(objectString, extentName));
                                    }
                                    RewritingValidator.EntityConfigurationToUserString(memberConditionQuery.Condition, builder, _context.ViewTarget == ViewTarget.UpdateView);
                                    ErrorLog.Record record = new ErrorLog.Record(true, ViewGenErrorCode.AmbiguousMultiConstants, builder.ToString(), _context.AllWrappersForExtent, String.Empty);
                                    _errorLog.AddEntry(record);
                                }
                            }
                        }
                    }
                }

                if (_errorLog.Count == 0)
                {
                    // for update views, add WHEN True THEN defaultValue
                    // which will ultimately be translated into a (possibly implicit) ELSE clause
                    if (_context.ViewTarget == ViewTarget.UpdateView && needCaseStatement)
                    {
                        AddElseDefaultToCaseStatement(currentPath, caseStatement, domain, rightDomainQuery, unionCaseRewriting);
                    }

                    if (caseStatement.Clauses.Count > 0)
                    {
                        TraceVerbose("{0}", caseStatement.ToString());
                        _caseStatements[currentPath] = caseStatement;
                    }
                }
            }
        }

        private void AddElseDefaultToCaseStatement(MemberPath currentPath, CaseStatement caseStatement, List<Constant> domain,
                                                   CellTreeNode rightDomainQuery, Tile<FragmentQuery> unionCaseRewriting)
        {
            Debug.Assert(_context.ViewTarget == ViewTarget.UpdateView, "Used for update views only");

            Constant defaultValue;
            bool hasDefaultValue = Domain.TryGetDefaultValueForMemberPath(currentPath, out defaultValue);

            if (false == hasDefaultValue || false == domain.Contains(defaultValue))
            {
                Debug.Assert(unionCaseRewriting != null, "No union of rewritings for case statements");
                CellTreeNode unionTree = TileToCellTree(unionCaseRewriting, _context);
                FragmentQuery configurationNeedsDefault = _context.RightFragmentQP.Difference(rightDomainQuery.RightFragmentQuery, unionTree.RightFragmentQuery);

                if (_context.RightFragmentQP.IsSatisfiable(configurationNeedsDefault))
                {
                    if (hasDefaultValue)
                    {
                        caseStatement.AddWhenThen(BoolExpression.True, new ConstantProjectedSlot(defaultValue, currentPath));
                    }
                    else
                    {
                        configurationNeedsDefault.Condition.ExpensiveSimplify();
                        StringBuilder builder = new StringBuilder();
                        builder.AppendLine(Strings.ViewGen_No_Default_Value_For_Configuration(currentPath.PathToString(false /* for alias */)));
                        RewritingValidator.EntityConfigurationToUserString(configurationNeedsDefault.Condition, builder);
                        _errorLog.AddEntry(new ErrorLog.Record(true, ViewGenErrorCode.NoDefaultValue, builder.ToString(), _context.AllWrappersForExtent, String.Empty));
                    }
                }
            }
        }

        // construct top-level WHERE clause
        private BoolExpression GetTopLevelWhereClause(HashSet<FragmentQuery> outputUsedViews)
        {
            BoolExpression topLevelWhereClause = BoolExpression.True;
            if (_context.ViewTarget == ViewTarget.QueryView)
            {
                // check whether a top-level query is needed
                if (!_domainQuery.Condition.IsTrue)
                {
                    Tile<FragmentQuery> topLevelRewriting;
                    if (FindRewritingAndUsedViews(_keyAttributes, _domainQuery.Condition, outputUsedViews, out topLevelRewriting))
                    {
                        topLevelWhereClause = TileToBoolExpr(topLevelRewriting);
                        topLevelWhereClause.ExpensiveSimplify();
                    }
                    else
                    {
                        Debug.Fail("Can't happen if EnsureExtentIsFullyMapped succeeded");
                    }
                }
            }
            return topLevelWhereClause;
        }

        // This makes sure that the mapping describes how to store all C-side data,
        // i.e., the view given by C-side cell queries is injective
        internal void EnsureExtentIsFullyMapped(HashSet<FragmentQuery> outputUsedViews)
        {
            if (_context.ViewTarget == ViewTarget.QueryView && _config.IsValidationEnabled)
            {

                // Run the check below for OfType views too so we can determine
                // what views are used (low overhead due to caching of rewritings)
                EnsureConfigurationIsFullyMapped(_extentPath, BoolExpression.True, outputUsedViews, _errorLog);
                if (_errorLog.Count > 0)
                {
                    ExceptionHelpers.ThrowMappingException(_errorLog, _config);
                }

            }
            else
            {
                if (_config.IsValidationEnabled)
                {
                    // Ensure that non-nullable, no-default attributes are always populated properly
                    foreach (MemberPath memberPath in _context.MemberMaps.ProjectedSlotMap.Members)
                    {
                        Constant defaultConstant;
                        if (memberPath.IsScalarType() &&
                            !memberPath.IsPartOfKey &&
                            !_domainMap.IsConditionMember(memberPath) &&
                            !Domain.TryGetDefaultValueForMemberPath(memberPath, out defaultConstant))
                        {
                            HashSet<MemberPath> attributes = new HashSet<MemberPath>(_keyAttributes);
                            attributes.Add(memberPath);
                            foreach (LeftCellWrapper leftCellWrapper in _context.AllWrappersForExtent)
                            {
                                FragmentQuery fragmentQuery = leftCellWrapper.FragmentQuery;

                                FragmentQuery tileQuery = new FragmentQuery(fragmentQuery.Description, fragmentQuery.FromVariable,
                                                                            attributes, fragmentQuery.Condition);
                                Tile<FragmentQuery> noNullToAvoid = CreateTile(FragmentQuery.Create(_keyAttributes, BoolExpression.CreateNot(fragmentQuery.Condition)));
                                Tile<FragmentQuery> noNullRewriting;
                                IEnumerable<MemberPath> notCoveredAttributes;
                                if (!RewriteQuery(CreateTile(tileQuery), noNullToAvoid, /*_views,*/ out noNullRewriting, out notCoveredAttributes, false /* isRelaxed */))
                                {
                                    // force error
                                    Domain.GetDefaultValueForMemberPath(memberPath, new LeftCellWrapper[] { leftCellWrapper }, _config);
                                }
                            }
                        }
                    }
                }

                // find a rewriting for each tile
                // some of the views may be redundant and unused
                foreach (Tile<FragmentQuery> toFill in _views)
                {
                    Tile<FragmentQuery> rewriting;
                    Tile<FragmentQuery> toAvoid = CreateTile(FragmentQuery.Create(_keyAttributes, BoolExpression.CreateNot(toFill.Query.Condition)));
                    IEnumerable<MemberPath> notCoveredAttributes;
                    bool found = RewriteQuery(toFill, toAvoid, out rewriting, out notCoveredAttributes, true /* isRelaxed */);

                    //Must be able to find the rewriting since the query is one of the views
                    // otherwise it means condition on the fragment is not satisfiable 
                    if (!found)
                    {
                        LeftCellWrapper fragment = _context.AllWrappersForExtent.First(lcr => lcr.FragmentQuery.Equals(toFill.Query));
                        Debug.Assert(fragment != null);

                        ErrorLog.Record record = new ErrorLog.Record(true, ViewGenErrorCode.ImpopssibleCondition, Strings.Viewgen_QV_RewritingNotFound(fragment.RightExtent.ToString()), fragment.Cells, String.Empty);
                        _errorLog.AddEntry(record);
                    }
                    else
                    {
                        outputUsedViews.UnionWith(rewriting.GetNamedQueries());
                    }
                }

            }

        }

        // Modifies _caseStatements and _topLevelWhereClause
        private List<LeftCellWrapper> RemapFromVariables()
        {
            List<LeftCellWrapper> usedCells = new List<LeftCellWrapper>();
            // remap CellIdBooleans appearing in WHEN clauses and in topLevelWhereClause so the first used cell = 0, second = 1, etc.
            // This ordering is exploited in CQL generation
            int newNumber = 0;
            Dictionary<BoolLiteral, BoolLiteral> literalRemap = new Dictionary<BoolLiteral, BoolLiteral>(BoolLiteral.EqualityIdentifierComparer);
            foreach (LeftCellWrapper leftCellWrapper in _context.AllWrappersForExtent)
            {
                if (_usedViews.Contains(leftCellWrapper.FragmentQuery))
                {
                    usedCells.Add(leftCellWrapper);
                    int oldNumber = leftCellWrapper.OnlyInputCell.CellNumber;
                    if (newNumber != oldNumber)
                    {
                        literalRemap[new CellIdBoolean(_identifiers, oldNumber)] = new CellIdBoolean(_identifiers, newNumber);
                    }
                    newNumber++;
                }
            }

            if (literalRemap.Count > 0)
            {
                // Remap _from literals in WHERE clause
                _topLevelWhereClause = _topLevelWhereClause.RemapLiterals(literalRemap);

                // Remap _from literals in case statements
                Dictionary<MemberPath, CaseStatement> newCaseStatements = new Dictionary<MemberPath, CaseStatement>();
                foreach (var entry in _caseStatements)
                {
                    CaseStatement newCaseStatement = new CaseStatement(entry.Key);
                    Debug.Assert(entry.Value.ElseValue == null);
                    foreach (CaseStatement.WhenThen clause in entry.Value.Clauses)
                    {
                        newCaseStatement.AddWhenThen(clause.Condition.RemapLiterals(literalRemap), clause.Value);
                    }
                    newCaseStatements[entry.Key] = newCaseStatement;
                }
                _caseStatements = newCaseStatements;
            }
            return usedCells;
        }

        // for backward compatibility: add (WHEN True THEN Type) for non-scalar types
        internal void AddTrivialCaseStatementsForConditionMembers()
        {
            for (int memberNum = 0; memberNum < _context.MemberMaps.ProjectedSlotMap.Count; memberNum++)
            {
                MemberPath memberPath = _context.MemberMaps.ProjectedSlotMap[memberNum];
                if (!memberPath.IsScalarType() && !_caseStatements.ContainsKey(memberPath))
                {
                    Constant typeConstant = new TypeConstant(memberPath.EdmType);
                    {
                        CaseStatement caseStmt = new CaseStatement(memberPath);
                        caseStmt.AddWhenThen(BoolExpression.True, new ConstantProjectedSlot(typeConstant, memberPath));
                        _caseStatements[memberPath] = caseStmt;
                    }
                }
            }
        }

        #endregion

        #region Computing rewriting

        // Find rewriting for query SELECT <attributes> WHERE <whereClause> FROM _extentPath
        // and add view appearing in rewriting to outputUsedViews
        private bool FindRewritingAndUsedViews(IEnumerable<MemberPath> attributes, BoolExpression whereClause,
                                               HashSet<FragmentQuery> outputUsedViews, out Tile<FragmentQuery> rewriting)
        {
            IEnumerable<MemberPath> notCoveredAttributes;
            return FindRewritingAndUsedViews(attributes, whereClause, outputUsedViews, out rewriting,
                                               out notCoveredAttributes);
        }

        // Find rewriting for query SELECT <attributes> WHERE <whereClause> FROM _extentPath
        // and add view appearing in rewriting to outputUsedViews
        private bool FindRewritingAndUsedViews(IEnumerable<MemberPath> attributes, BoolExpression whereClause,
                                               HashSet<FragmentQuery> outputUsedViews, out Tile<FragmentQuery> rewriting,
                                               out IEnumerable<MemberPath> notCoveredAttributes)
        {
            if (FindRewriting(attributes, whereClause, out rewriting, out notCoveredAttributes))
            {
                outputUsedViews.UnionWith(rewriting.GetNamedQueries());
                return true;
            }
            return false;
        }

        // Find rewriting for query SELECT <attributes> WHERE <whereClause> FROM _extentPath
        private bool FindRewriting(IEnumerable<MemberPath> attributes, BoolExpression whereClause,
                                   out Tile<FragmentQuery> rewriting, out IEnumerable<MemberPath> notCoveredAttributes)
        {
            Tile<FragmentQuery> toFill = CreateTile(FragmentQuery.Create(attributes, whereClause));
            Debug.Assert(toFill.Query.Attributes.Count > 0, "Query has no attributes?");
            Tile<FragmentQuery> toAvoid = CreateTile(FragmentQuery.Create(_keyAttributes, BoolExpression.CreateNot(whereClause)));

            bool isRelaxed = (_context.ViewTarget == ViewTarget.UpdateView);
            bool found = RewriteQuery(toFill, toAvoid, out rewriting, out notCoveredAttributes, isRelaxed);
            Debug.Assert(!found || rewriting.GetNamedQueries().All(q => q != TrueViewSurrogate.Query),
                         "TrueViewSurrogate should have been substituted");
            return found;
        }

        private bool RewriteQuery(Tile<FragmentQuery> toFill, Tile<FragmentQuery> toAvoid, out Tile<FragmentQuery> rewriting, out IEnumerable<MemberPath> notCoveredAttributes,
            bool isRelaxed)
        {
            notCoveredAttributes = new List<MemberPath>();
            // first, find a rewriting for WHERE clause only
            FragmentQuery toFillQuery = toFill.Query;
            if (_context.TryGetCachedRewriting(toFillQuery, out rewriting))
            {
                TraceVerbose("Cached rewriting {0}: {1}", toFill, rewriting);
                return true; // query with attributes is already cached
            }

            // Filter the relevant views. These may include a TrueSurrogate view
            IEnumerable<Tile<FragmentQuery>> relevantViews = GetRelevantViews(toFillQuery, isRelaxed);
            FragmentQuery originalToFillQuery = toFillQuery;

            if (!RewriteQueryCached(CreateTile(FragmentQuery.Create(toFillQuery.Condition)), toAvoid, relevantViews, out rewriting))
            {
                if (isRelaxed)
                {
                    // don't give up quite yet
                    toFillQuery = FragmentQuery.Create(toFillQuery.Attributes, BoolExpression.CreateAndNot(toFillQuery.Condition, rewriting.Query.Condition));
                    if (_qp.IsEmpty(CreateTile(toFillQuery)) ||
                        !RewriteQueryCached(CreateTile(FragmentQuery.Create(toFillQuery.Condition)), toAvoid, relevantViews, out rewriting))
                    {
                        return false; // finally give up
                    }
                }
                else
                {
                    return false;
                }
            }
            if (toFillQuery.Attributes.Count == 0)
            {
                // return w/o trying to remove TrueSurrogate from view - it's an attribute-less view
                // we keep TrueSurrogate there because it may be expanded in various ways for
                // different projected attributes
                return true;
            }

            // now we have the rewriting for WHERE
            Dictionary<MemberPath, FragmentQuery> attributeConditions = new Dictionary<MemberPath, FragmentQuery>();
            foreach (MemberPath attribute in NonKeys(toFillQuery.Attributes))
            {
                attributeConditions[attribute] = toFillQuery;
            }
            if (attributeConditions.Count == 0 || CoverAttributes(ref rewriting, toFillQuery, attributeConditions))
            {
                GetUsedViewsAndRemoveTrueSurrogate(ref rewriting);
                _context.SetCachedRewriting(originalToFillQuery, rewriting);
                return true; // all attributes are covered
            }
            else if (isRelaxed)
            {
                // re-initialize attributeConditions by subtracting the remaining attributes to cover
                foreach (MemberPath attribute in NonKeys(toFillQuery.Attributes))
                {
                    FragmentQuery remainingCondition;
                    if (attributeConditions.TryGetValue(attribute, out remainingCondition))
                    {
                        attributeConditions[attribute] = FragmentQuery.Create(BoolExpression.CreateAndNot(toFillQuery.Condition, remainingCondition.Condition));
                    }
                    else
                    {
                        attributeConditions[attribute] = toFillQuery;
                    }
                }
                if (CoverAttributes(ref rewriting, toFillQuery, attributeConditions))
                {
                    GetUsedViewsAndRemoveTrueSurrogate(ref rewriting);
                    _context.SetCachedRewriting(originalToFillQuery, rewriting);
                    return true;
                }
            }
            notCoveredAttributes = attributeConditions.Keys;
            return false;
        }

        // input views may contain TrueSurrogate
        private bool RewriteQueryCached(Tile<FragmentQuery> toFill, Tile<FragmentQuery> toAvoid,
                                        IEnumerable<Tile<FragmentQuery>> views, out Tile<FragmentQuery> rewriting)
        {
            Debug.Assert(toFill.Query.Attributes.Count == 0, "This method is used for attribute-less queries only");

            if (!_context.TryGetCachedRewriting(toFill.Query, out rewriting))
            {
                bool hasRewriting = _qp.RewriteQuery(toFill, toAvoid, views, out rewriting);
                TraceVerbose("Computed rewriting {0}: {1}", toFill, rewriting);
                if (hasRewriting)
                {
                    _context.SetCachedRewriting(toFill.Query, rewriting);
                }
                return hasRewriting;
            }
            TraceVerbose("Cached rewriting {0}: {1}", toFill, rewriting);
            return true;
        }

        private bool CoverAttributes(ref Tile<FragmentQuery> rewriting, FragmentQuery toFillQuery,
            Dictionary<MemberPath, FragmentQuery> attributeConditions)
        {
            // first, account for already used views
            HashSet<FragmentQuery> usedViews = new HashSet<FragmentQuery>(rewriting.GetNamedQueries());
            Debug.Assert(usedViews.Count > 0);
            //List<FragmentQuery> usedViewsList = new List<FragmentQuery>(usedViews);
            //usedViewsList.Sort(FragmentQuery.GetComparer(toFillQuery.Attributes));
            foreach (FragmentQuery view in usedViews)
            {
                foreach (MemberPath projectedAttribute in NonKeys(view.Attributes))
                {
                    CoverAttribute(projectedAttribute, view, attributeConditions, toFillQuery);
                }
                if (attributeConditions.Count == 0)
                {
                    return true; // we are done
                }
            }
            // still need to fill some attributes
            Tile<FragmentQuery> attributeTile = null;
            foreach (FragmentQuery view in _fragmentQueries)
            {
                foreach (MemberPath projectedAttribute in NonKeys(view.Attributes))
                {
                    if (CoverAttribute(projectedAttribute, view, attributeConditions, toFillQuery))
                    {
                        attributeTile = (attributeTile == null) ? CreateTile(view) : _qp.Union(attributeTile, CreateTile(view));
                    }
                }
                if (attributeConditions.Count == 0)
                {
                    break; // we are done!
                }
            }
            if (attributeConditions.Count == 0)
            {
                // yes, we covered all attributes
                Debug.Assert(attributeTile != null);
                rewriting = _qp.Join(rewriting, attributeTile);
                return true;
            }
            else
            {
                // create rewriting that we couldn't satisfy
                return false; // couldn't cover some attribute(s)
            }
        }

        // returns true if the view is useful for covering the projected attribute
        private bool CoverAttribute(MemberPath projectedAttribute, FragmentQuery view, Dictionary<MemberPath, FragmentQuery> attributeConditions, FragmentQuery toFillQuery)
        {
            FragmentQuery currentAttributeCondition;
            if (attributeConditions.TryGetValue(projectedAttribute, out currentAttributeCondition))
            {
                currentAttributeCondition = FragmentQuery.Create(BoolExpression.CreateAndNot(currentAttributeCondition.Condition, view.Condition));
                if (_qp.IsEmpty(CreateTile(currentAttributeCondition)))
                {
                    // this attribute is covered! remove it from the list
                    attributeConditions.Remove(projectedAttribute);
                }
                else
                {
                    attributeConditions[projectedAttribute] = currentAttributeCondition;
                }
                return true;
            }
            return false;
        }

        private IEnumerable<Tile<FragmentQuery>> GetRelevantViews(FragmentQuery query, bool isRelaxed)
        {
            // Step 1:
            // Determine connected and directly/indirectly connected variables
            // Directly connected variables: those that appear in query's WHERE clause
            // Indirectly connected variables: directly connected variables + variables in all views that contain directly connected variables
            // Disconnected variables: those that appear in some view's WHERE clause but are not indirectly connected
            Set<MemberPath> connectedVariables = GetVariables(query);

            // Step 2:
            // Take a union of all views that contain connected variables
            // If it evaluates to True, we can discard all other views; no special True-view is needed
            // Otherwise:
            //   If isRelaxed == false:
            //       Take a union of all views. If it yields True, than assume that True-view is available.
            //       Later, try to pick a smaller subset (instead of all views) once we know that attributes are needed
            //   If isRelaxed == true:
            //       Discard all views that don't contain connected variables; assume that True-view is available
            Tile<FragmentQuery> unionOfConnectedViews = null;
            List<Tile<FragmentQuery>> connectedViews = new List<Tile<FragmentQuery>>();
            Tile<FragmentQuery> firstTrueView = null;
            foreach (Tile<FragmentQuery> tile in _views)
            {
                // notice: this is a syntactic check. We assume that if the variable is not present in the condition,
                // its value is unrestricted (which in general may not be true because the KB may have e.g., X=1 => Y=1,
                // so even if condition on Y is absent, the view would still be relevant
                if (GetVariables(tile.Query).Overlaps(connectedVariables))
                {
                    unionOfConnectedViews = (unionOfConnectedViews == null) ? tile : _qp.Union(unionOfConnectedViews, tile);
                    connectedViews.Add(tile);
                }
                else if (IsTrue(tile.Query) && firstTrueView == null)
                {
                    firstTrueView = tile; // don't add True views; only one of them might be needed, if at all
                }

            }
            if (unionOfConnectedViews != null &&
                IsTrue(unionOfConnectedViews.Query)) // the collected views give us "True"
            {
                return connectedViews;
            }
            if (firstTrueView == null)
            {
                // can we obtain True at all?
                Tile<FragmentQuery> unionTile = null;
                foreach (FragmentQuery view in _fragmentQueries)
                {
                    unionTile = (unionTile == null) ? CreateTile(view) : _qp.Union(unionTile, CreateTile(view));
                    if (IsTrue(unionTile.Query))
                    {
                        // yes, we can; use a surrogate view - replace it later
                        firstTrueView = TrueViewSurrogate;
                        break;
                    }
                }
            }

            if (firstTrueView != null) // the collected views don't give us True, but 
            {
                connectedViews.Add(firstTrueView);
                return connectedViews;
            }

            // Step 3:
            // For each indirectly-connected variable x:
            // Union all views that contain x. The condition on x must disappear, i.e., union must imply that x is in Domain(x)
            // That is, the union must be equivalent to the expression in which all conditions on x have been eliminated.
            // If that's not the case (i.e., can't get rid of x), remove all these views from consideration.

            return _views;
        }

        private HashSet<FragmentQuery> GetUsedViewsAndRemoveTrueSurrogate(ref Tile<FragmentQuery> rewriting)
        {
            HashSet<FragmentQuery> usedViews = new HashSet<FragmentQuery>(rewriting.GetNamedQueries());
            if (!usedViews.Contains(TrueViewSurrogate.Query))
            {
                return usedViews; // no surrogate
            }
            // remove the surrogate
            usedViews.Remove(TrueViewSurrogate.Query);

            // first, try to union usedViews to see whether we can get True
            Tile<FragmentQuery> unionTile = null;
            IEnumerable<FragmentQuery> usedFollowedByUnusedViews = usedViews.Concat(_fragmentQueries);
            foreach (FragmentQuery view in usedFollowedByUnusedViews)
            {
                unionTile = (unionTile == null) ? CreateTile(view) : _qp.Union(unionTile, CreateTile(view));
                usedViews.Add(view);
                if (IsTrue(unionTile.Query))
                {
                    // we found a true rewriting
                    rewriting = rewriting.Replace(TrueViewSurrogate, unionTile);
                    return usedViews;
                }
            }
            // now we either found the rewriting or we can just take all views because we are in relaxed mode for update views
            Debug.Fail("Shouldn't happen");
            return usedViews;
        }

        #endregion

        #region Helper methods

        private BoolExpression CreateMemberCondition(MemberPath path, Constant domainValue)
        {
            return FragmentQuery.CreateMemberCondition(path, domainValue, _domainMap);
        }

        private FragmentQuery CreateMemberConditionQuery(MemberPath currentPath, Constant domainValue)
        {
            return CreateMemberConditionQuery(currentPath, domainValue, _keyAttributes, _domainMap);
        }

        internal static FragmentQuery CreateMemberConditionQuery(MemberPath currentPath, Constant domainValue,
                                                                 IEnumerable<MemberPath> keyAttributes, MemberDomainMap domainMap)
        {
            // construct WHERE clause for this value
            BoolExpression domainWhereClause = FragmentQuery.CreateMemberCondition(currentPath, domainValue, domainMap);

            // get a rewriting for CASE statements by not requesting any attributes beyond key
            IEnumerable<MemberPath> attributes = keyAttributes;
            if (domainValue is NegatedConstant)
            {
                // we need the attribute value
                attributes = keyAttributes.Concat(new MemberPath[] { currentPath });
            }
            return FragmentQuery.Create(attributes, domainWhereClause);
        }

        private static TileNamed<FragmentQuery> CreateTile(FragmentQuery query)
        {
            return new TileNamed<FragmentQuery>(query);
        }

        private static IEnumerable<Constant> GetTypeConstants(IEnumerable<EdmType> types)
        {
            foreach (EdmType type in types)
            {
                yield return new TypeConstant(type);
            }
        }

        private static IEnumerable<MemberPath> GetNonConditionalScalarMembers(EdmType edmType, MemberPath currentPath, MemberDomainMap domainMap)
        {
            return currentPath.GetMembers(edmType, true /* isScalar */, false /* isConditional */, null /* isPartOfKey */, domainMap);
        }

        private static IEnumerable<MemberPath> GetConditionalComplexMembers(EdmType edmType, MemberPath currentPath, MemberDomainMap domainMap)
        {
            return currentPath.GetMembers(edmType, false /* isScalar */, true /* isConditional */, null /* isPartOfKey */, domainMap);
        }

        private static IEnumerable<MemberPath> GetNonConditionalComplexMembers(EdmType edmType, MemberPath currentPath, MemberDomainMap domainMap)
        {
            return currentPath.GetMembers(edmType, false /* isScalar */, false /* isConditional */, null /* isPartOfKey */, domainMap);
        }

        private static IEnumerable<MemberPath> GetConditionalScalarMembers(EdmType edmType, MemberPath currentPath, MemberDomainMap domainMap)
        {
            return currentPath.GetMembers(edmType, true /* isScalar */, true /* isConditional */, null /* isPartOfKey */, domainMap);
        }

        private IEnumerable<MemberPath> NonKeys(IEnumerable<MemberPath> attributes)
        {
            return attributes.Where(attr => !attr.IsPartOfKey);
        }

        // allows us to check whether a found rewriting is satisfiable
        // by taking into account the "other side" of mapping constraints
        // (Ultimately, should produce a CQT and use general-purpose query containment)
        internal static CellTreeNode TileToCellTree(Tile<FragmentQuery> tile, ViewgenContext context)
        {
            if (tile.OpKind == TileOpKind.Named)
            {
                FragmentQuery view = ((TileNamed<FragmentQuery>)tile).NamedQuery;
                LeftCellWrapper leftCellWrapper = context.AllWrappersForExtent.First(w => w.FragmentQuery == view);
                return new LeafCellTreeNode(context, leftCellWrapper);
            }
            CellTreeOpType opType;
            switch (tile.OpKind)
            {
                case TileOpKind.Join: opType = CellTreeOpType.IJ; break;
                case TileOpKind.AntiSemiJoin: opType = CellTreeOpType.LASJ; break;
                case TileOpKind.Union: opType = CellTreeOpType.Union; break;
                default:
                    Debug.Fail("unexpected");
                    return null;
            }
            return new OpCellTreeNode(context, opType,
                                      TileToCellTree(tile.Arg1, context),
                                      TileToCellTree(tile.Arg2, context));
        }

        private static BoolExpression TileToBoolExpr(Tile<FragmentQuery> tile)
        {
            switch (tile.OpKind)
            {
                case TileOpKind.Named:
                    FragmentQuery view = ((TileNamed<FragmentQuery>)tile).NamedQuery;
                    if (view.Condition.IsAlwaysTrue())
                    {
                        return BoolExpression.True;
                    }
                    else
                    {
                        Debug.Assert(view.FromVariable != null);
                        return view.FromVariable;
                    }
                case TileOpKind.Join:
                    return BoolExpression.CreateAnd(TileToBoolExpr(tile.Arg1), TileToBoolExpr(tile.Arg2));
                case TileOpKind.AntiSemiJoin:
                    return BoolExpression.CreateAnd(TileToBoolExpr(tile.Arg1), BoolExpression.CreateNot(TileToBoolExpr(tile.Arg2)));
                case TileOpKind.Union:
                    return BoolExpression.CreateOr(TileToBoolExpr(tile.Arg1), TileToBoolExpr(tile.Arg2));
                default:
                    Debug.Fail("unexpected");
                    return null;
            }
        }

        private static bool IsDefaultValue(Constant domainValue, MemberPath path)
        {
            if (domainValue.IsNull() && path.IsNullable)
            {
                return true;
            }
            if (path.DefaultValue != null)
            {
                ScalarConstant scalarConstant = domainValue as ScalarConstant;
                return scalarConstant.Value == path.DefaultValue;
            }
            return false;
        }

        // Returns MemberPaths which have conditions in the where clause
        // Filters out all trivial conditions (e.g., num=1 where dom(num)={1})
        // i.e., where all constants from the domain are contained in range
        private Set<MemberPath> GetVariables(FragmentQuery query)
        {
            IEnumerable<MemberPath> memberVariables =
                from domainConstraint in query.Condition.VariableConstraints
                where domainConstraint.Variable.Identifier is MemberRestriction &&
                      false == domainConstraint.Variable.Domain.All(constant => domainConstraint.Range.Contains(constant))
                select ((MemberRestriction)domainConstraint.Variable.Identifier).RestrictedMemberSlot.MemberPath;

            return new Set<MemberPath>(memberVariables, MemberPath.EqualityComparer);
        }

        private bool IsTrue(FragmentQuery query)
        {
            return !_context.LeftFragmentQP.IsSatisfiable(FragmentQuery.Create(BoolExpression.CreateNot(query.Condition)));
        }

        [Conditional("DEBUG")]
        private void PrintStatistics(RewritingProcessor<Tile<FragmentQuery>> qp)
        {
            int numSATChecks;
            int numIntersection;
            int numDifference;
            int numUnion;
            int numErrors;
            qp.GetStatistics(out numSATChecks, out numIntersection, out numUnion, out numDifference, out numErrors);
            TraceVerbose("{0} containment checks, {4} set operations ({1} intersections + {2} unions + {3} differences)",
                numSATChecks, numIntersection, numUnion, numDifference,
                                numIntersection + numUnion + numDifference);
            TraceVerbose("{0} errors", numErrors);
        }

        [Conditional("DEBUG")]
        internal void TraceVerbose(string msg, params object[] parameters)
        {
            if (_config.IsVerboseTracing)
            {
                Helpers.FormatTraceLine(msg, parameters);
            }
        }

        #endregion
    }
}
