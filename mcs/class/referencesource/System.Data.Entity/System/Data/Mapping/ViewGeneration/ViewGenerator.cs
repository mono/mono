//---------------------------------------------------------------------
// <copyright file="ViewGenerator.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System.Data.Common.CommandTrees;
using System.Data.Common.Utils;
using System.Data.Common.Utils.Boolean;
using System.Data.Mapping.ViewGeneration.Structures;
using System.Data.Mapping.ViewGeneration.Validation;
using System.Data.Mapping.ViewGeneration.QueryRewriting;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Data.Mapping.ViewGeneration.Utils;
using System.Data.Metadata.Edm;
using System.Linq;

namespace System.Data.Mapping.ViewGeneration
{
    using ViewSet = KeyToListMap<EntitySetBase, GeneratedView>;
    using CellGroup = Set<Cell>;
    using WrapperBoolExpr = BoolExpr<LeftCellWrapper>;
    using WrapperTrueExpr = TrueExpr<LeftCellWrapper>;
    using WrapperFalseExpr = FalseExpr<LeftCellWrapper>;
    using WrapperNotExpr = NotExpr<LeftCellWrapper>;
    using WrapperOrExpr = OrExpr<LeftCellWrapper>;

    // This class is responsible for generating query or update mapping
    // views from the initial cells.
    internal class ViewGenerator : InternalBase
    {
        #region Fields
        private CellGroup m_cellGroup; // The initial cells from which we produce views
        private ConfigViewGenerator m_config; // Configuration variables
        private MemberDomainMap m_queryDomainMap;
        private MemberDomainMap m_updateDomainMap;
        private Dictionary<EntitySetBase, QueryRewriter> m_queryRewriterCache;
        private List<ForeignConstraint> m_foreignKeyConstraints;
        private StorageEntityContainerMapping m_entityContainerMapping;
        #endregion

        #region Internal API - Only Gatekeeper calls it

        // effects: Creates a ViewGenerator object that is capable of
        // producing query or update mapping views given the relevant schema
        // given the "cells"
        internal ViewGenerator(CellGroup cellGroup, ConfigViewGenerator config,
                              List<ForeignConstraint> foreignKeyConstraints,
                              StorageEntityContainerMapping entityContainerMapping)
        {

            m_cellGroup = cellGroup;
            m_config = config;
            m_queryRewriterCache = new Dictionary<EntitySetBase, QueryRewriter>();
            m_foreignKeyConstraints = foreignKeyConstraints;
            m_entityContainerMapping = entityContainerMapping;

            Dictionary<EntityType, Set<EntityType>> inheritanceGraph = MetadataHelper.BuildUndirectedGraphOfTypes(entityContainerMapping.StorageMappingItemCollection.EdmItemCollection);
            SetConfiguration(entityContainerMapping);

            // We fix all the cells at this point
            m_queryDomainMap = new MemberDomainMap(ViewTarget.QueryView, m_config.IsValidationEnabled, cellGroup, entityContainerMapping.StorageMappingItemCollection.EdmItemCollection, m_config, inheritanceGraph);
            m_updateDomainMap = new MemberDomainMap(ViewTarget.UpdateView, m_config.IsValidationEnabled, cellGroup, entityContainerMapping.StorageMappingItemCollection.EdmItemCollection, m_config, inheritanceGraph);

            // We now go and fix the queryDomain map so that it has all the
            // values from the S-side as well -- this is needed for domain
            // constraint propagation, i.e., values from the S-side get
            // propagated to te oneOfConst on the C-side. So we better get
            // the "possiblveValues" stuff to contain those constants as well
            MemberDomainMap.PropagateUpdateDomainToQueryDomain(cellGroup, m_queryDomainMap, m_updateDomainMap);

            UpdateWhereClauseForEachCell(cellGroup, m_queryDomainMap, m_updateDomainMap, m_config);

            // We need to simplify cell queries, yet we don't want the conditions to disappear
            // So, add an extra value to the domain, temporarily
            MemberDomainMap queryOpenDomain = m_queryDomainMap.GetOpenDomain();
            MemberDomainMap updateOpenDomain = m_updateDomainMap.GetOpenDomain();

            // Make sure the WHERE clauses of the cells reflect the changes
            foreach (Cell cell in cellGroup)
            {
                cell.CQuery.WhereClause.FixDomainMap(queryOpenDomain);
                cell.SQuery.WhereClause.FixDomainMap(updateOpenDomain);
                cell.CQuery.WhereClause.ExpensiveSimplify();
                cell.SQuery.WhereClause.ExpensiveSimplify();
                cell.CQuery.WhereClause.FixDomainMap(m_queryDomainMap);
                cell.SQuery.WhereClause.FixDomainMap(m_updateDomainMap);
            }
        }

        private void SetConfiguration(StorageEntityContainerMapping entityContainerMapping)
        {
            m_config.IsValidationEnabled = entityContainerMapping.Validate;
            m_config.GenerateUpdateViews = entityContainerMapping.GenerateUpdateViews;
        }

        // effects: Generates views for the particular cellgroup in this. Returns an
        // error log describing the errors that were encountered (if none
        // were encountered, the ErrorLog.Count is 0). Places the generated
        // views in result
        internal ErrorLog GenerateAllBidirectionalViews(ViewSet views, CqlIdentifiers identifiers)
        {

            // Allow missing attributes for now to make entity splitting run through
            // we cannot do this for query views in general: need to obtain the exact enumerated domain

            if (m_config.IsNormalTracing)
            {
                StringBuilder builder = new StringBuilder();
                Cell.CellsToBuilder(builder, m_cellGroup);
                Helpers.StringTraceLine(builder.ToString());
            }

            m_config.SetTimeForFinishedActivity(PerfType.CellCreation);
            // Check if the cellgroup is consistent and all known S constraints are
            // satisified by the known C constraints
            CellGroupValidator validator = new CellGroupValidator(m_cellGroup, m_config);
            ErrorLog errorLog = validator.Validate();

            if (errorLog.Count > 0)
            {
                errorLog.PrintTrace();
                return errorLog;
            }

            m_config.SetTimeForFinishedActivity(PerfType.KeyConstraint);

            // We generate update views first since they perform the main
            // validation checks
            if (m_config.GenerateUpdateViews)
            {
                errorLog = GenerateDirectionalViews(ViewTarget.UpdateView, identifiers, views);
                if (errorLog.Count > 0)
                {
                    return errorLog; // If we have discovered errors here, do not generate query views
                }
            }

            // Make sure that the foreign key constraints are not violated
            if (m_config.IsValidationEnabled)
            {
                CheckForeignKeyConstraints(errorLog);
            }
            m_config.SetTimeForFinishedActivity(PerfType.ForeignConstraint);

            if (errorLog.Count > 0)
            {
                errorLog.PrintTrace();
                return errorLog; // If we have discovered errors here, do not generate query views
            }
            
            // Query views - do not allow missing attributes
            // For the S-side, we add NOT ... for each scalar constant so
            // that if we have C, P in the mapping but the store has C, P, S,
            // we can handle it in the query views
            m_updateDomainMap.ExpandDomainsToIncludeAllPossibleValues();

            errorLog = GenerateDirectionalViews(ViewTarget.QueryView, identifiers, views);

            return errorLog;
        }

        internal ErrorLog GenerateQueryViewForSingleExtent(ViewSet views, CqlIdentifiers identifiers, EntitySetBase entity, EntityTypeBase type, ViewGenMode mode)
        {
            Debug.Assert(mode != ViewGenMode.GenerateAllViews);

            if (m_config.IsNormalTracing)
            {
                StringBuilder builder = new StringBuilder();
                Cell.CellsToBuilder(builder, m_cellGroup);
                Helpers.StringTraceLine(builder.ToString());
            }

            // Check if the cellgroup is consistent and all known S constraints are
            // satisified by the known C constraints
            CellGroupValidator validator = new CellGroupValidator(m_cellGroup, m_config);
            ErrorLog errorLog = validator.Validate();
            if (errorLog.Count > 0)
            {
                errorLog.PrintTrace();
                return errorLog;
            }

            // Make sure that the foreign key constraints are not violated
            if (m_config.IsValidationEnabled)
            {
                CheckForeignKeyConstraints(errorLog);
            }

            if (errorLog.Count > 0)
            {
                errorLog.PrintTrace();
                return errorLog; // If we have discovered errors here, do not generate query views
            }

            // For the S-side, we add NOT ... for each scalar constant so
            // that if we have C, P in the mapping but the store has C, P, S,
            // we can handle it in the query views
            m_updateDomainMap.ExpandDomainsToIncludeAllPossibleValues();
            
            foreach (Cell cell in m_cellGroup)
            {
                cell.SQuery.WhereClause.FixDomainMap(m_updateDomainMap);
            }

            errorLog = GenerateQueryViewForExtentAndType(m_entityContainerMapping, identifiers, views, entity, type, mode);

            return errorLog;
        }


        #endregion



        #region Private Methods

        // effects: Given the extent cells and a map for the domains of all
        // variables in it, fixes the cell constant domains of the where
        // clauses in the left queries of cells (left is defined using viewTarget)
        private static void UpdateWhereClauseForEachCell(IEnumerable<Cell> extentCells, MemberDomainMap queryDomainMap,
                                                            MemberDomainMap updateDomainMap, ConfigViewGenerator config)
        {
            foreach (Cell cell in extentCells)
            {
                cell.CQuery.UpdateWhereClause(queryDomainMap);
                cell.SQuery.UpdateWhereClause(updateDomainMap);
            }

            // Fix enumerable domains - currently it is only applicable to boolean type. Note that it is 
            // not applicable to enumerated types since we allow any value of the underlying type of the enum type.
            queryDomainMap.ReduceEnumerableDomainToEnumeratedValues(ViewTarget.QueryView, config);
            updateDomainMap.ReduceEnumerableDomainToEnumeratedValues(ViewTarget.UpdateView, config);
        }


        private ErrorLog GenerateQueryViewForExtentAndType(StorageEntityContainerMapping entityContainerMapping, CqlIdentifiers identifiers, ViewSet views, EntitySetBase entity, EntityTypeBase type, ViewGenMode mode)
        {
            Debug.Assert(mode != ViewGenMode.GenerateAllViews);

            // Keep track of the mapping exceptions that we have generated
            ErrorLog errorLog = new ErrorLog();

            if (m_config.IsViewTracing)
            {
                Helpers.StringTraceLine(String.Empty);
                Helpers.StringTraceLine(String.Empty);
                Helpers.FormatTraceLine("================= Generating {0} Query View for: {1} ===========================",
                                    (mode == ViewGenMode.OfTypeViews) ? "OfType" : "OfTypeOnly",
                                    entity.Name);
                Helpers.StringTraceLine(String.Empty);
                Helpers.StringTraceLine(String.Empty);
            }

            try
            {
                // (1) view generation (checks that extents are fully mapped)
                ViewgenContext context = CreateViewgenContext(entity, ViewTarget.QueryView, identifiers);
                QueryRewriter queryRewriter = GenerateViewsForExtentAndType(type, context, identifiers, views, mode);
            }
            catch (InternalMappingException exception)
            {
                // All exceptions have mapping errors in them
                Debug.Assert(exception.ErrorLog.Count > 0, "Incorrectly created mapping exception");
                errorLog.Merge(exception.ErrorLog);
            }

            return errorLog;
        }


        // requires: schema refers to C-side or S-side schema for the cells
        // inside this. if schema.IsQueryView is true, the left side of cells refers
        // to the C side (and vice-versa for the right side)
        // effects: Generates the relevant views for the schema side and
        // returns them. If allowMissingAttributes is true and attributes
        // are missing on the schema side, substitutes them with NULL
        // Modifies views to contain the generated views for different
        // extents specified by cells and the the schemaContext
        private ErrorLog GenerateDirectionalViews(ViewTarget viewTarget, CqlIdentifiers identifiers, ViewSet views)
        {
            bool isQueryView = viewTarget == ViewTarget.QueryView;

            // Partition cells by extent.
            KeyToListMap<EntitySetBase, Cell> extentCellMap = GroupCellsByExtent(m_cellGroup, viewTarget);

            // Keep track of the mapping exceptions that we have generated
            ErrorLog errorLog = new ErrorLog();

            // Generate views for each extent
            foreach (EntitySetBase extent in extentCellMap.Keys)
            {
                if (m_config.IsViewTracing)
                {
                    Helpers.StringTraceLine(String.Empty);
                    Helpers.StringTraceLine(String.Empty);
                    Helpers.FormatTraceLine("================= Generating {0} View for: {1} ===========================",
                                     isQueryView ? "Query" : "Update", extent.Name);
                    Helpers.StringTraceLine(String.Empty);
                    Helpers.StringTraceLine(String.Empty);
                }
                try
                {
                    // (1) view generation (checks that extents are fully mapped)
                    QueryRewriter queryRewriter = GenerateDirectionalViewsForExtent(viewTarget, extent, identifiers, views);

                    // (2) validation for update views
                    if (viewTarget == ViewTarget.UpdateView &&
                        m_config.IsValidationEnabled)
                    {
                        if (m_config.IsViewTracing)
                        {
                            Helpers.StringTraceLine(String.Empty);
                            Helpers.StringTraceLine(String.Empty);
                            Helpers.FormatTraceLine("----------------- Validation for generated update view for: {0} -----------------",
                                             extent.Name);
                            Helpers.StringTraceLine(String.Empty);
                            Helpers.StringTraceLine(String.Empty);
                        }

                        RewritingValidator validator = new RewritingValidator(queryRewriter.ViewgenContext, queryRewriter.BasicView);
                        validator.Validate();
                    }

                }
                catch (InternalMappingException exception)
                {
                    // All exceptions have mapping errors in them
                    Debug.Assert(exception.ErrorLog.Count > 0,
                                 "Incorrectly created mapping exception");
                    errorLog.Merge(exception.ErrorLog);
                }
            }
            return errorLog;
        }


        // effects: Generates a view for an extent "extent" that belongs to
        // schema "schema". extentCells are the cells for this extent.
        // Adds the view corrsponding to the extent to "views"
        private QueryRewriter GenerateDirectionalViewsForExtent(ViewTarget viewTarget, EntitySetBase extent, CqlIdentifiers identifiers, ViewSet views)
        {

            // First normalize the cells in terms of multiconstants, etc
            // and then generate the view for the extent
            ViewgenContext context = CreateViewgenContext(extent, viewTarget, identifiers);
            QueryRewriter queryRewriter = null;

            if (m_config.GenerateViewsForEachType)
            {
                // generate views for each OFTYPE(Extent, Type) combination
                foreach (EdmType type in MetadataHelper.GetTypeAndSubtypesOf(extent.ElementType, m_entityContainerMapping.StorageMappingItemCollection.EdmItemCollection, false /*includeAbstractTypes*/))
                {
                    if (m_config.IsViewTracing && false == type.Equals(extent.ElementType))
                    {
                        Helpers.FormatTraceLine("CQL View for {0} and type {1}", extent.Name, type.Name);
                    }
                    queryRewriter = GenerateViewsForExtentAndType(type, context, identifiers, views, ViewGenMode.OfTypeViews);
                }
            }
            else
            {
                // generate the view for Extent only
                queryRewriter = GenerateViewsForExtentAndType(extent.ElementType, context, identifiers, views, ViewGenMode.OfTypeViews);
            }
            if (viewTarget == ViewTarget.QueryView)
            {
                m_config.SetTimeForFinishedActivity(PerfType.QueryViews);
            }
            else
            {
                m_config.SetTimeForFinishedActivity(PerfType.UpdateViews);
            }

            // cache this rewriter (and context inside it) for future use in FK checking
            m_queryRewriterCache[extent] = queryRewriter;
            return queryRewriter;
        }

        // effects: Returns a context corresponding to extent (if one does not exist, creates one)
        private ViewgenContext CreateViewgenContext(EntitySetBase extent, ViewTarget viewTarget, CqlIdentifiers identifiers)
        {
            QueryRewriter queryRewriter;
            if (!m_queryRewriterCache.TryGetValue(extent, out queryRewriter))
            {
                // collect the cells that belong to this extent (just a few of them since we segment the mapping first)
                var cellsForExtent = m_cellGroup.Where(c => c.GetLeftQuery(viewTarget).Extent == extent);
                
                return new ViewgenContext(viewTarget, extent, cellsForExtent, identifiers, m_config, m_queryDomainMap, m_updateDomainMap, m_entityContainerMapping);
            }
            else
            {
                return queryRewriter.ViewgenContext;
            }
        }


        private QueryRewriter GenerateViewsForExtentAndType(EdmType generatedType, ViewgenContext context, CqlIdentifiers identifiers, ViewSet views, ViewGenMode mode)
        {

            Debug.Assert(mode != ViewGenMode.GenerateAllViews, "By definition this method can not handle generating views for all extents");

            QueryRewriter queryRewriter = new QueryRewriter(generatedType, context, mode);
            queryRewriter.GenerateViewComponents();

            // Get the basic view
            CellTreeNode basicView = queryRewriter.BasicView;

            if (m_config.IsNormalTracing)
            {
                Helpers.StringTrace("Basic View: ");
                Helpers.StringTraceLine(basicView.ToString());
            }

            CellTreeNode simplifiedView = GenerateSimplifiedView(basicView, queryRewriter.UsedCells);

            if (m_config.IsNormalTracing)
            {
                Helpers.StringTraceLine(String.Empty);
                Helpers.StringTrace("Simplified View: ");
                Helpers.StringTraceLine(simplifiedView.ToString());
            }

            CqlGenerator cqlGen = new CqlGenerator(simplifiedView,
                                                   queryRewriter.CaseStatements, 
                                                   identifiers,
                                                   context.MemberMaps.ProjectedSlotMap,
                                                   queryRewriter.UsedCells.Count, 
                                                   queryRewriter.TopLevelWhereClause,
                                                   m_entityContainerMapping.StorageMappingItemCollection);

            string eSQLView ;
            DbQueryCommandTree commandTree;
            if (m_config.GenerateEsql)
            {
                eSQLView = cqlGen.GenerateEsql();
                commandTree = null;
            }
            else
            {
                eSQLView = null;
                commandTree = cqlGen.GenerateCqt();
            }

            GeneratedView generatedView = GeneratedView.CreateGeneratedView(context.Extent, generatedType, commandTree, eSQLView, m_entityContainerMapping.StorageMappingItemCollection, m_config);
            views.Add(context.Extent, generatedView);

            return queryRewriter;
        }

        private CellTreeNode GenerateSimplifiedView(CellTreeNode basicView, List<LeftCellWrapper> usedCells)
        {
            Debug.Assert(false == basicView.IsEmptyRightFragmentQuery, "Basic view is empty?");

            // create 'joined' variables, one for each cell
            // We know (say) that out of the 10 cells that we were given, only 7 (say) were
            // needed to construct the view for this extent.
            int numBoolVars = usedCells.Count;
            // We need the boolean expressions in Simplify. Precisely ont boolean expression is set to
            // true in each cell query

            for (int i = 0; i < numBoolVars; i++)
            {
                // In the ith cell, set its boolean to be true (i.e., ith boolean)
                usedCells[i].RightCellQuery.InitializeBoolExpressions(numBoolVars, i);
            }

            CellTreeNode simplifiedView = CellTreeSimplifier.MergeNodes(basicView);
            return simplifiedView;
        }

        private void CheckForeignKeyConstraints(ErrorLog errorLog)
        {
            foreach (ForeignConstraint constraint in m_foreignKeyConstraints)
            {
                QueryRewriter childRewriter = null;
                QueryRewriter parentRewriter = null;
                m_queryRewriterCache.TryGetValue(constraint.ChildTable, out childRewriter);
                m_queryRewriterCache.TryGetValue(constraint.ParentTable, out parentRewriter);
                constraint.CheckConstraint(m_cellGroup, childRewriter, parentRewriter, errorLog, m_config);
            }
        }

        // effects: Given all the cells for a container, groups the cells by
        // the left query's extent and returns a dictionary for it
        private static KeyToListMap<EntitySetBase, Cell> GroupCellsByExtent(IEnumerable<Cell> cells, ViewTarget viewTarget)
        {

            // Partition cells by extent -- extent is the top node in
            // the tree. Even for compositions for now? CHANGE_[....]_FEATURE_COMPOSITION
            KeyToListMap<EntitySetBase, Cell> extentCellMap =
                new KeyToListMap<EntitySetBase, Cell>(EqualityComparer<EntitySetBase>.Default);
            foreach (Cell cell in cells)
            {
                // Get the cell query and determine its extent
                CellQuery cellQuery = cell.GetLeftQuery(viewTarget);
                extentCellMap.Add(cellQuery.Extent, cell);
            }
            return extentCellMap;
        }

        #endregion

        #region String Methods
        internal override void ToCompactString(StringBuilder builder)
        {
            Cell.CellsToBuilder(builder, m_cellGroup);
        }
        #endregion


    }

}
