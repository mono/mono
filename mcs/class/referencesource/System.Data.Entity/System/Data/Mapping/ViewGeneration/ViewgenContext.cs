//---------------------------------------------------------------------
// <copyright file="ViewgenContext.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

namespace System.Data.Mapping.ViewGeneration
{
    using System.Collections.Generic;
    using System.Data.Common.Utils;
    using System.Data.Entity;
    using System.Data.Mapping.ViewGeneration.QueryRewriting;
    using System.Data.Mapping.ViewGeneration.Structures;
    using System.Data.Mapping.ViewGeneration.Utils;
    using System.Data.Metadata.Edm;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;

    internal class ViewgenContext : InternalBase
    {

        #region Fields
        
        private ConfigViewGenerator m_config;
        private ViewTarget m_viewTarget;

        // Extent for which the view is being generated
        private EntitySetBase m_extent;
        
        // Different maps for members
        private MemberMaps m_memberMaps;
        private EdmItemCollection m_edmItemCollection;
        private StorageEntityContainerMapping m_entityContainerMapping;

        // The normalized cells that are created
        private List<LeftCellWrapper> m_cellWrappers;
        
        // Implicit constraints between members in queries based on schema. E.g., p.Addr IS NOT NULL <=> p IS OF Customer
        private FragmentQueryProcessor m_leftFragmentQP;
        
        // In addition to constraints for each right extent contains constraints due to associations
        private FragmentQueryProcessor m_rightFragmentQP;
        
        private CqlIdentifiers m_identifiers;
        
        // Maps (left) queries to their rewritings in terms of views
        private Dictionary<FragmentQuery, Tile<FragmentQuery>> m_rewritingCache;

        
        #endregion

        #region Constructors
        internal ViewgenContext(ViewTarget viewTarget, EntitySetBase extent, IEnumerable<Cell> extentCells,
                                CqlIdentifiers identifiers, ConfigViewGenerator config, MemberDomainMap queryDomainMap,
                                MemberDomainMap updateDomainMap, StorageEntityContainerMapping entityContainerMapping)
        {
            foreach (Cell cell in extentCells)
            {
                Debug.Assert(extent.Equals(cell.GetLeftQuery(viewTarget).Extent));
                Debug.Assert(cell.CQuery.NumProjectedSlots == cell.SQuery.NumProjectedSlots);
            }

            m_extent = extent;
            m_viewTarget = viewTarget;
            m_config = config;
            m_edmItemCollection = entityContainerMapping.StorageMappingItemCollection.EdmItemCollection;
            m_entityContainerMapping = entityContainerMapping;
            m_identifiers = identifiers;

            // create a copy of updateDomainMap so generation of query views later on is not affected
            // it is modified in QueryRewriter.AdjustMemberDomainsForUpdateViews
            updateDomainMap = updateDomainMap.MakeCopy();

            // Create a signature generator that handles all the
            // multiconstant work and generating the signatures
            MemberDomainMap domainMap = viewTarget == ViewTarget.QueryView ? queryDomainMap : updateDomainMap;

            m_memberMaps = new MemberMaps(viewTarget, MemberProjectionIndex.Create(extent, m_edmItemCollection), queryDomainMap, updateDomainMap);

            // Create left fragment KB: includes constraints for the extent to be constructed
            FragmentQueryKB leftKB = new FragmentQueryKB();
            leftKB.CreateVariableConstraints(extent, domainMap, m_edmItemCollection);
            m_leftFragmentQP = new FragmentQueryProcessor(leftKB);
            m_rewritingCache = new Dictionary<FragmentQuery, Tile<FragmentQuery>>(
                FragmentQuery.GetEqualityComparer(m_leftFragmentQP));

            // Now using the signatures, create new cells such that
            // "extent's" query (C or S) is described in terms of multiconstants
            if (!CreateLeftCellWrappers(extentCells, viewTarget))
            {
                return;
            }

            // Create right fragment KB: includes constraints for all extents and association roles of right queries
            FragmentQueryKB rightKB = new FragmentQueryKB();
            MemberDomainMap rightDomainMap = viewTarget == ViewTarget.QueryView ? updateDomainMap : queryDomainMap;
            foreach (LeftCellWrapper leftCellWrapper in m_cellWrappers)
            {
                EntitySetBase rightExtent = leftCellWrapper.RightExtent;
                rightKB.CreateVariableConstraints(rightExtent, rightDomainMap, m_edmItemCollection);
                rightKB.CreateAssociationConstraints(rightExtent, rightDomainMap, m_edmItemCollection);
            }
            
            if (m_viewTarget == ViewTarget.UpdateView)
            {
                CreateConstraintsForForeignKeyAssociationsAffectingThisWarapper(rightKB, rightDomainMap);
            }

            m_rightFragmentQP = new FragmentQueryProcessor(rightKB);

            // Check for concurrency control tokens
            if (m_viewTarget == ViewTarget.QueryView)
            {
                CheckConcurrencyControlTokens();
            }
            // For backward compatibility -
            // order wrappers by increasing domain size, decreasing number of attributes
            m_cellWrappers.Sort(LeftCellWrapper.Comparer);
        }

        /// <summary>
        /// Find the Foreign Key Associations that relate EntitySets used in these left cell wrappers and 
        /// add any equivalence facts between sets implied by 1:1 associations.
        /// We can collect other implication facts but we don't have a scenario that needs them( yet ).
        /// </summary>
        /// <param name="rightKB"></param>
        /// <param name="rightDomainMap"></param>
        private void CreateConstraintsForForeignKeyAssociationsAffectingThisWarapper(FragmentQueryKB rightKB, MemberDomainMap rightDomainMap)
        {
            //First find the entity types of the sets in these cell wrappers.
            var entityTypes = m_cellWrappers.Select(it => it.RightExtent).OfType<EntitySet>().Select(it => it.ElementType);
            //Get all the foreign key association sets in these entity sets
            var allForeignKeyAssociationSets = this.m_entityContainerMapping.EdmEntityContainer.BaseEntitySets.OfType<AssociationSet>().Where(it => it.ElementType.IsForeignKey);
            //Find all the foreign key associations that have corresponding sets
            var oneToOneForeignKeyAssociationsForThisWrapper = allForeignKeyAssociationSets.Select(it => it.ElementType);
            //Find all the 1:1 associations from the above list
            oneToOneForeignKeyAssociationsForThisWrapper = oneToOneForeignKeyAssociationsForThisWrapper.Where(it => (it.AssociationEndMembers.All(endMember => endMember.RelationshipMultiplicity == RelationshipMultiplicity.One)));
            //Filter the 1:1 foreign key associations to the ones relating the sets used in these cell wrappers.
            oneToOneForeignKeyAssociationsForThisWrapper = oneToOneForeignKeyAssociationsForThisWrapper.Where(it => (it.AssociationEndMembers.All(endMember => entityTypes.Contains(endMember.GetEntityType()))));

            //filter foreign key association sets to the sets that are 1:1 and affecting this wrapper.
            var oneToOneForeignKeyAssociationSetsForThisWrapper = allForeignKeyAssociationSets.Where(it => oneToOneForeignKeyAssociationsForThisWrapper.Contains(it.ElementType));

            //Collect the facts for the foreign key association sets that are 1:1 and affecting this wrapper
            foreach (var assocSet in oneToOneForeignKeyAssociationSetsForThisWrapper)
            {
                rightKB.CreateEquivalenceConstraintForOneToOneForeignKeyAssociation(assocSet, rightDomainMap, m_edmItemCollection);
            }
        }

        #endregion


        #region Properties
        internal ViewTarget ViewTarget
        {
            get
            {
                return m_viewTarget;
            }
        }

        internal MemberMaps MemberMaps
        {
            get
            {
                return m_memberMaps;
            }
        }

        // effects: Returns the extent for which the cells have been normalized
        internal EntitySetBase Extent
        {
            get { return m_extent; }
        }

        internal ConfigViewGenerator Config
        {
            get { return m_config; }
        }

        internal CqlIdentifiers CqlIdentifiers
        {
            get { return m_identifiers; }
        }

        internal EdmItemCollection EdmItemCollection
        {
            get { return m_edmItemCollection; }
        }

        internal FragmentQueryProcessor LeftFragmentQP
        {
            get { return m_leftFragmentQP; }
        }

        internal FragmentQueryProcessor RightFragmentQP
        {
            get { return m_rightFragmentQP; }
        }

        // effects: Returns all wrappers that were originally relevant for
        // this extent
        internal List<LeftCellWrapper> AllWrappersForExtent
        {
            get
            {
                return m_cellWrappers;
            }
        }

        internal StorageEntityContainerMapping EntityContainerMapping
        {
            get { return m_entityContainerMapping; }
        }
        #endregion

        #region InternalMethods

        // effects: Returns the cached rewriting of (left) queries in terms of views, if any
        internal bool TryGetCachedRewriting(FragmentQuery query, out Tile<FragmentQuery> rewriting)
        {   
            return m_rewritingCache.TryGetValue(query, out rewriting);
        }

        // effects: Records the cached rewriting of (left) queries in terms of views
        internal void SetCachedRewriting(FragmentQuery query, Tile<FragmentQuery> rewriting)
        {
            m_rewritingCache[query] = rewriting;
        }

        #endregion

        #region Private Methods

        
        /// <summary>
        /// Checks:
        ///  1) Concurrency token is not defined in this Extent's ElementTypes' derived types
        ///  2) Members with concurrency token should not have conditions specified
        /// </summary>
        private void CheckConcurrencyControlTokens()
        {
            Debug.Assert(m_viewTarget == ViewTarget.QueryView);
            // Get the token fields for this extent

            EntityTypeBase extentType = m_extent.ElementType;
            Set<EdmMember> tokenMembers = MetadataHelper.GetConcurrencyMembersForTypeHierarchy(extentType, m_edmItemCollection);
            Set<MemberPath> tokenPaths = new Set<MemberPath>(MemberPath.EqualityComparer);
            foreach (EdmMember tokenMember in tokenMembers)
            {
                if (!tokenMember.DeclaringType.IsAssignableFrom(extentType))
                {
                    string message = System.Data.Entity.Strings.ViewGen_Concurrency_Derived_Class(tokenMember.Name, tokenMember.DeclaringType.Name, m_extent);
                    ErrorLog.Record record = new ErrorLog.Record(true, ViewGenErrorCode.ConcurrencyDerivedClass, message, m_cellWrappers, String.Empty);
                    ExceptionHelpers.ThrowMappingException(record, m_config);
                }
                tokenPaths.Add(new MemberPath(m_extent, tokenMember));
            }

            if (tokenMembers.Count > 0)
            {
                foreach (LeftCellWrapper wrapper in m_cellWrappers)
                {
                    Set<MemberPath> conditionMembers = new Set<MemberPath>(
                                                            wrapper.OnlyInputCell.CQuery.WhereClause.MemberRestrictions.Select(oneOf => oneOf.RestrictedMemberSlot.MemberPath),
                                                            MemberPath.EqualityComparer);
                    conditionMembers.Intersect(tokenPaths);
                    if (conditionMembers.Count > 0)
                    {
                        // There is a condition on concurrency tokens. Throw an exception.
                        StringBuilder builder = new StringBuilder();
                        builder.AppendLine(Strings.ViewGen_Concurrency_Invalid_Condition(MemberPath.PropertiesToUserString(conditionMembers, false), m_extent.Name));
                        ErrorLog.Record record = new ErrorLog.Record(true, ViewGenErrorCode.ConcurrencyTokenHasCondition, builder.ToString(), new LeftCellWrapper[] { wrapper }, String.Empty);
                        ExceptionHelpers.ThrowMappingException(record, m_config);
                    }
                }
            }
        }

        // effects: Given the cells for the extent (extentCells) along with
        // the signatures (multiconstants + needed attributes) for this extent, generates
        // the left cell wrappers for it extent (viewTarget indicates whether
        // the view is for querying or update purposes
        // Modifies m_cellWrappers to contain this list
        private bool CreateLeftCellWrappers(IEnumerable<Cell> extentCells, ViewTarget viewTarget)
        {

            List<Cell> extentCellsList = new List<Cell>(extentCells);
            List<Cell> alignedCells = AlignFields(extentCellsList, m_memberMaps.ProjectedSlotMap, viewTarget);
            Debug.Assert(alignedCells.Count == extentCellsList.Count, "Cell counts disagree");

            // Go through all the cells and create cell wrappers that can be used for generating the view
            m_cellWrappers = new List<LeftCellWrapper>();

            for (int i = 0; i < alignedCells.Count; i++)
            {
                Cell alignedCell = alignedCells[i];
                CellQuery left = alignedCell.GetLeftQuery(viewTarget);
                CellQuery right = alignedCell.GetRightQuery(viewTarget);

                // Obtain the non-null projected slots into attributes
                Set<MemberPath> attributes = left.GetNonNullSlots();

                BoolExpression fromVariable = BoolExpression.CreateLiteral(new CellIdBoolean(m_identifiers, extentCellsList[i].CellNumber), m_memberMaps.LeftDomainMap);
                FragmentQuery leftFragmentQuery = FragmentQuery.Create(fromVariable, left);
                FragmentQuery rightFragmentQuery = FragmentQuery.Create(fromVariable, right);
                if (viewTarget == ViewTarget.UpdateView)
                {
                    leftFragmentQuery = m_leftFragmentQP.CreateDerivedViewBySelectingConstantAttributes(leftFragmentQuery) ?? leftFragmentQuery;
                }

                LeftCellWrapper leftWrapper = new LeftCellWrapper(m_viewTarget, attributes, leftFragmentQuery, left, right, m_memberMaps,
                                                                  extentCellsList[i]);
                m_cellWrappers.Add(leftWrapper);
            }
            return true;
        }

        // effects: Align the fields of each cell in mapping using projectedSlotMap that has a mapping 
        // for each member of this extent to the slot number of that member in the projected slots
        // example:
        //    input:  Proj[A,B,"5"] = Proj[F,"7",G]
        //            Proj[C,B]     = Proj[H,I]
        //   output:  m_projectedSlotMap: A -> 0, B -> 1, C -> 2
        //            Proj[A,B,null] = Proj[F,"7",null]
        //            Proj[null,B,C] = Proj[null,I,H]
        private static List<Cell> AlignFields(IEnumerable<Cell> cells, MemberProjectionIndex projectedSlotMap,
                                              ViewTarget viewTarget)
        {

            List<Cell> outputCells = new List<Cell>();

            // Determine the aligned field for each cell
            // The new cells have ProjectedSlotMap.Count number of fields
            foreach (Cell cell in cells)
            {

                // If isQueryView is true, we need to consider the C side of
                // the cells; otherwise, we look at the S side. Note that we
                // CANNOT use cell.LeftQuery since that is determined by
                // cell's isQueryView

                // The query for which we are constructing the extent
                CellQuery mainQuery = cell.GetLeftQuery(viewTarget);
                CellQuery otherQuery = cell.GetRightQuery(viewTarget);

                CellQuery newMainQuery;
                CellQuery newOtherQuery;
                // Create both queries where the projected slot map is used
                // to determine the order of the fields of the mainquery (of
                // course, the otherQuery's fields are aligned automatically)
                mainQuery.CreateFieldAlignedCellQueries(otherQuery, projectedSlotMap,
                                                        out newMainQuery, out newOtherQuery);

                Cell outputCell = viewTarget == ViewTarget.QueryView ?
                    Cell.CreateCS(newMainQuery, newOtherQuery, cell.CellLabel, cell.CellNumber) :
                    Cell.CreateCS(newOtherQuery, newMainQuery, cell.CellLabel, cell.CellNumber);
                outputCells.Add(outputCell);
            }
            return outputCells;
        }

        #endregion

        #region String Methods
        internal override void ToCompactString(StringBuilder builder)
        {
            LeftCellWrapper.WrappersToStringBuilder(builder, m_cellWrappers, "Left Celll Wrappers");
        }

        #endregion
    }
}
