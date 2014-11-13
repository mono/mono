//---------------------------------------------------------------------
// <copyright file="CellCreator.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System.Data.Common.Utils;
using System.Data.Mapping.ViewGeneration.Structures;
using System.Collections.Generic;
using System.Data.Mapping.ViewGeneration.Utils;
using System.Diagnostics;
using System.Data.Metadata.Edm;
using System.Linq;

namespace System.Data.Mapping.ViewGeneration
{
    /// <summary>
    /// A class that handles creation of cells from the meta data information.
    /// </summary>
    internal class CellCreator : InternalBase
    {

        #region Constructors
        // effects: Creates a cell creator object for an entity container's
        // mappings (specified in "maps")
        internal CellCreator(StorageEntityContainerMapping containerMapping)
        {
            m_containerMapping = containerMapping;
            m_identifiers = new CqlIdentifiers();
        }
        #endregion

        #region Fields
        // The mappings from the metadata for different containers
        private StorageEntityContainerMapping m_containerMapping;
        private int m_currentCellNumber;
        private CqlIdentifiers m_identifiers;
        // Keep track of all the identifiers to prevent clashes with _from0,
        // _from1, T, T1, etc
        // Keep track of names of 
        // * Entity Containers
        // * Extent names
        // * Entity Types
        // * Complex Types
        // * Properties
        // * Roles
        #endregion

        #region Properties
        // effects: Returns the set of identifiers used in this
        internal CqlIdentifiers Identifiers
        {
            get { return m_identifiers; }
        }
        #endregion

        #region External methods
        // effects: Generates the cells for all the entity containers
        // specified in this. The generated cells are geared for query view generation
        internal List<Cell> GenerateCells(ConfigViewGenerator config)
        {
            List<Cell> cells = new List<Cell>();

            // Get the cells from the entity container metadata
            ExtractCells(cells);

            ExpandCells(cells);

            // Get the identifiers from the cells
            m_identifiers.AddIdentifier(m_containerMapping.EdmEntityContainer.Name);
            m_identifiers.AddIdentifier(m_containerMapping.StorageEntityContainer.Name);
            foreach (Cell cell in cells)
            {
                cell.GetIdentifiers(m_identifiers);
            }

            return cells;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Boolean members have a closed domain and are enumerated when domains are established i.e. (T, F) instead of (notNull). 
        /// Query Rewriting is exercised over every domain of the condition member. If the member contains not_null condition 
        /// for example, it cannot generate a view for partitions (member=T), (Member=F). For this reason we need to expand the cells 
        /// in a predefined situation (below) to include sub-fragments mapping individual elements of the closed domain.  
        /// Enums (a planned feature) need to be handled in a similar fashion.
        /// 
        /// Find booleans that are projected with a not_null condition 
        /// Expand ALL cells where they are projected. Why? See Unit Test case NullabilityConditionOnBoolean5.es
        /// Validation will fail because it will not be able to validate rewritings for partitions on the 'other' cells.
        /// </summary>
        private void ExpandCells(List<Cell> cells)
        {
            var sSideMembersToBeExpanded = new Set<MemberPath>();

            foreach (Cell cell in cells)
            {
                //Find Projected members that are Boolean AND are mentioned in the Where clause with not_null condition
                foreach (var memberToExpand in cell.SQuery.GetProjectedMembers()
                                            .Where(member => IsBooleanMember(member))
                                            .Where(boolMember => cell.SQuery.GetConjunctsFromWhereClause()
                                                                    .Where(restriction => restriction.Domain.Values.Contains(Constant.NotNull))
                                                                    .Select(restriction => restriction.RestrictedMemberSlot.MemberPath).Contains(boolMember)))
                {
                    sSideMembersToBeExpanded.Add(memberToExpand);
                }
            }

            //Foreach s-side members, find all c-side members it is mapped to
            //  We need these because we need to expand all cells where the boolean candidate is projected or mapped member is projected, e.g:
            //   (1) C[id, cdisc] WHERE d=true   <=>   T1[id, sdisc] WHERE sdisc=NOTNULL
            //   (2) C[id, cdisc] WHERE d=false  <=>   T2[id, sdisc]
            //  Here we need to know that because of T1.sdisc, we need to expand T2.sdisc.
            //  This is done by tracking cdisc, and then seeing in cell 2 that it is mapped to T2.sdisc

            var cSideMembersForSSideExpansionCandidates = new Dictionary<MemberPath, Set<MemberPath>>();
            foreach (Cell cell in cells)
            {
                foreach (var sSideMemberToExpand in sSideMembersToBeExpanded)
                {
                    var cSideMembers = cell.SQuery.GetProjectedPositions(sSideMemberToExpand).Select(pos => ((MemberProjectedSlot)cell.CQuery.ProjectedSlotAt(pos)).MemberPath);

                    Set<MemberPath> cSidePaths = null;
                    if (!cSideMembersForSSideExpansionCandidates.TryGetValue(sSideMemberToExpand, out cSidePaths))
                    {
                        cSidePaths = new Set<MemberPath>();
                        cSideMembersForSSideExpansionCandidates[sSideMemberToExpand] = cSidePaths;
                    }

                    cSidePaths.AddRange(cSideMembers);
                }
            }

            // Expand cells that project members collected earlier with T/F conditiions
            foreach (Cell cell in cells.ToArray())
            {
                //Each member gets its own expansion. Including multiple condition candidates in one SQuery
                // "... <=> T[..] WHERE a=notnull AND b=notnull" means a and b get their own independent expansions
                // Note: this is not a cross-product
                foreach (var memberToExpand in sSideMembersToBeExpanded)
                {
                    var mappedCSideMembers = cSideMembersForSSideExpansionCandidates[memberToExpand];

                    //Check if member is projected in this cell.
                    if (cell.SQuery.GetProjectedMembers().Contains(memberToExpand))
                    {
                        // Creationg additional cel can fail when the condition to be appended contradicts existing condition in the CellQuery
                        // We don't add contradictions because they seem to cause unrelated problems in subsequent validation routines
                        Cell resultCell = null;
                        if (TryCreateAdditionalCellWithCondition(cell, memberToExpand, true  /*condition value*/, ViewTarget.UpdateView /*s-side member*/, out resultCell))
                        {
                            cells.Add(resultCell);
                        }
                        if (TryCreateAdditionalCellWithCondition(cell, memberToExpand, false /*condition value*/, ViewTarget.UpdateView /*s-side member*/, out resultCell))
                        {
                            cells.Add(resultCell);
                        }
                    }
                    else
                    {  //If the s-side member is not projected, see if the mapped C-side member(s) is projected
                        foreach (var cMemberToExpand in cell.CQuery.GetProjectedMembers().Intersect(mappedCSideMembers))
                        {
                            Cell resultCell = null;
                            if (TryCreateAdditionalCellWithCondition(cell, cMemberToExpand, true  /*condition value*/, ViewTarget.QueryView /*c-side member*/, out resultCell))
                            {
                                cells.Add(resultCell);
                            }

                            if (TryCreateAdditionalCellWithCondition(cell, cMemberToExpand, false /*condition value*/, ViewTarget.QueryView /*c-side member*/, out resultCell))
                            {
                                cells.Add(resultCell);
                            }
                        }
                    }
                }
            }

        }

        /// <summary>
        /// Given a cell, a member and a boolean condition on that member, creates additional cell
        /// which with the specified restriction on the member in addition to original condition.
        /// e.i conjunction of original condition AND member in newCondition
        /// 
        /// Creation fails when the original condition contradicts new boolean condition
        /// 
        /// ViewTarget tells whether MemberPath is in Cquery or SQuery
        /// </summary>
        private bool TryCreateAdditionalCellWithCondition(Cell originalCell, MemberPath memberToExpand, bool conditionValue, ViewTarget viewTarget, out Cell result)
        {
            Debug.Assert(originalCell != null);
            Debug.Assert(memberToExpand != null);
            result = null;

            //Create required structures
            MemberPath leftExtent = originalCell.GetLeftQuery(viewTarget).SourceExtentMemberPath;
            MemberPath rightExtent = originalCell.GetRightQuery(viewTarget).SourceExtentMemberPath;

            //Now for the given left-side projected member, find corresponding right-side member that it is mapped to 
            int indexOfBooLMemberInProjection = originalCell.GetLeftQuery(viewTarget).GetProjectedMembers().TakeWhile(path => !path.Equals(memberToExpand)).Count();
            MemberProjectedSlot rightConditionMemberSlot = ((MemberProjectedSlot)originalCell.GetRightQuery(viewTarget).ProjectedSlotAt(indexOfBooLMemberInProjection));
            MemberPath rightSidePath = rightConditionMemberSlot.MemberPath;

            List<ProjectedSlot> leftSlots = new List<ProjectedSlot>();
            List<ProjectedSlot> rightSlots = new List<ProjectedSlot>();

            //Check for impossible conditions (otehrwise we get inaccurate pre-validation errors)
            ScalarConstant negatedCondition = new ScalarConstant(!conditionValue);

            if (originalCell.GetLeftQuery(viewTarget).Conditions
                    .Where(restriction => restriction.RestrictedMemberSlot.MemberPath.Equals(memberToExpand))
                    .Where(restriction => restriction.Domain.Values.Contains(negatedCondition)).Any()
                || originalCell.GetRightQuery(viewTarget).Conditions
                    .Where(restriction => restriction.RestrictedMemberSlot.MemberPath.Equals(rightSidePath))
                    .Where(restriction => restriction.Domain.Values.Contains(negatedCondition)).Any())
            {
                return false;
            }
            //End check

            //Create Projected Slots
            // Map all slots in original cell (not just keys) because some may be required (non nullable and no default)
            // and others may have not_null condition so MUST be projected. Rely on the user doing the right thing, otherwise
            // they will get the error message anyway
            for (int i = 0; i < originalCell.GetLeftQuery(viewTarget).NumProjectedSlots; i++)
            {
                leftSlots.Add(originalCell.GetLeftQuery(viewTarget).ProjectedSlotAt(i));
            }

            for (int i = 0; i < originalCell.GetRightQuery(viewTarget).NumProjectedSlots; i++)
            {
                rightSlots.Add(originalCell.GetRightQuery(viewTarget).ProjectedSlotAt(i));
            }

            //Create condition boolena expressions
            BoolExpression leftQueryWhereClause = BoolExpression.CreateLiteral(new ScalarRestriction(memberToExpand, new ScalarConstant(conditionValue)), null);
            leftQueryWhereClause = BoolExpression.CreateAnd(originalCell.GetLeftQuery(viewTarget).WhereClause, leftQueryWhereClause);

            BoolExpression rightQueryWhereClause = BoolExpression.CreateLiteral(new ScalarRestriction(rightSidePath, new ScalarConstant(conditionValue)), null);
            rightQueryWhereClause = BoolExpression.CreateAnd(originalCell.GetRightQuery(viewTarget).WhereClause, rightQueryWhereClause);

            //Create additional Cells
            CellQuery rightQuery = new CellQuery(rightSlots, rightQueryWhereClause, rightExtent, originalCell.GetRightQuery(viewTarget).SelectDistinctFlag);
            CellQuery leftQuery = new CellQuery(leftSlots, leftQueryWhereClause, leftExtent, originalCell.GetLeftQuery(viewTarget).SelectDistinctFlag);

            Cell newCell;
            if (viewTarget == ViewTarget.UpdateView)
            {
                newCell = Cell.CreateCS(rightQuery, leftQuery, originalCell.CellLabel, m_currentCellNumber);
            }
            else
            {
                newCell = Cell.CreateCS(leftQuery, rightQuery, originalCell.CellLabel, m_currentCellNumber);
            }

            m_currentCellNumber++;
            result = newCell;
            return true;
        }

        // effects: Given the metadata information for a container in
        // containerMap, generate the cells for it and modify cells to
        // contain the newly-generated cells
        private void ExtractCells(List<Cell> cells)
        {
            // extract entity mappings, i.e., for CPerson1, COrder1, etc
            foreach (StorageSetMapping extentMap in m_containerMapping.AllSetMaps)
            {

                // Get each type map in an entity set mapping, i.e., for
                // CPerson, CCustomer, etc in CPerson1
                foreach (StorageTypeMapping typeMap in extentMap.TypeMappings)
                {

                    StorageEntityTypeMapping entityTypeMap = typeMap as StorageEntityTypeMapping;
                    Debug.Assert(entityTypeMap != null ||
                                 typeMap is StorageAssociationTypeMapping, "Invalid typemap");

                    // A set for all the types in this type mapping
                    Set<EdmType> allTypes = new Set<EdmType>();

                    if (entityTypeMap != null)
                    {
                        // Gather a set of all explicit types for an entity
                        // type mapping in allTypes. Note that we do not have
                        // subtyping in association sets
                        allTypes.AddRange(entityTypeMap.Types);
                        foreach (EdmType type in entityTypeMap.IsOfTypes)
                        {
                            IEnumerable<EdmType> typeAndSubTypes = MetadataHelper.GetTypeAndSubtypesOf(type, m_containerMapping.StorageMappingItemCollection.EdmItemCollection, false /*includeAbstractTypes*/);
                            allTypes.AddRange(typeAndSubTypes);
                        }
                    }

                    EntitySetBase extent = extentMap.Set;
                    Debug.Assert(extent != null, "Extent map for a null extent or type of extentMap.Exent " +
                                 "is not Extent");

                    // For each table mapping for the type mapping, we create cells
                    foreach (StorageMappingFragment fragmentMap in typeMap.MappingFragments)
                    {
                        ExtractCellsFromTableFragment(extent, fragmentMap, allTypes, cells);
                    }
                }
            }
        }

        // effects: Given an extent's ("extent") table fragment that is
        // contained inside typeMap, determine the cells that need to be
        // created and add them to cells
        // allTypes corresponds to all the different types that the type map
        // represents -- this parameter has something useful only if extent
        // is an entity set
        private void ExtractCellsFromTableFragment(EntitySetBase extent, StorageMappingFragment fragmentMap,
                                                   Set<EdmType> allTypes, List<Cell> cells)
        {

            // create C-query components
            MemberPath cRootExtent = new MemberPath(extent);
            BoolExpression cQueryWhereClause = BoolExpression.True;
            List<ProjectedSlot> cSlots = new List<ProjectedSlot>();

            if (allTypes.Count > 0)
            {
                // Create a type condition for the extent, i.e., "extent in allTypes"
                cQueryWhereClause = BoolExpression.CreateLiteral(new TypeRestriction(cRootExtent, allTypes), null);
            }

            // create S-query components
            MemberPath sRootExtent = new MemberPath(fragmentMap.TableSet);
            BoolExpression sQueryWhereClause = BoolExpression.True;
            List<ProjectedSlot> sSlots = new List<ProjectedSlot>();

            // Association or entity set
            // Add the properties and the key properties to a list and
            // then process them in ExtractProperties
            ExtractProperties(fragmentMap.AllProperties, cRootExtent, cSlots, ref cQueryWhereClause, sRootExtent, sSlots, ref sQueryWhereClause);

            // limitation of MSL API: cannot assign constant values to table columns
            CellQuery cQuery = new CellQuery(cSlots, cQueryWhereClause, cRootExtent, CellQuery.SelectDistinct.No /*no distinct flag*/);
            CellQuery sQuery = new CellQuery(sSlots, sQueryWhereClause, sRootExtent,
                                        fragmentMap.IsSQueryDistinct ? CellQuery.SelectDistinct.Yes : CellQuery.SelectDistinct.No);

            StorageMappingFragment fragmentInfo = fragmentMap as StorageMappingFragment;
            Debug.Assert((fragmentInfo != null), "CSMappingFragment should support Line Info");
            CellLabel label = new CellLabel(fragmentInfo);
            Cell cell = Cell.CreateCS(cQuery, sQuery, label, m_currentCellNumber);
            m_currentCellNumber++;
            cells.Add(cell);
        }

        // requires: "properties" corresponds to all the properties that are
        // inside cNode.Value, e.g., cNode corresponds to an extent Person,
        // properties contains all the properties inside Person (recursively)
        // effects: Given C-side and S-side Cell Query for a cell, generates
        // the projected slots on both sides corresponding to
        // properties. Also updates the C-side whereclause corresponding to
        // discriminator properties on the C-side, e.g, isHighPriority
        private void ExtractProperties(IEnumerable<StoragePropertyMapping> properties,
                                       MemberPath cNode, List<ProjectedSlot> cSlots,
                                       ref BoolExpression cQueryWhereClause,
                                       MemberPath sRootExtent,
                                       List<ProjectedSlot> sSlots,
                                       ref BoolExpression sQueryWhereClause)
        {
            // For each property mapping, we add an entry to the C and S cell queries
            foreach (StoragePropertyMapping propMap in properties)
            {
                StorageScalarPropertyMapping scalarPropMap = propMap as StorageScalarPropertyMapping;
                StorageComplexPropertyMapping complexPropMap = propMap as StorageComplexPropertyMapping;
                StorageEndPropertyMapping associationEndPropertypMap = propMap as StorageEndPropertyMapping;
                StorageConditionPropertyMapping conditionMap = propMap as StorageConditionPropertyMapping;

                Debug.Assert(scalarPropMap != null ||
                             complexPropMap != null ||
                             associationEndPropertypMap != null ||
                             conditionMap != null, "Unimplemented property mapping");

                if (scalarPropMap != null)
                {
                    Debug.Assert(scalarPropMap.ColumnProperty != null, "ColumnMember for a Scalar Property can not be null");
                    // Add an attribute node to node

                    MemberPath cAttributeNode = new MemberPath(cNode, scalarPropMap.EdmProperty);
                    // Add a column (attribute) node the sQuery
                    // unlike the C side, there is no nesting. Hence we
                    // did not need an internal node
                    MemberPath sAttributeNode = new MemberPath(sRootExtent, scalarPropMap.ColumnProperty);
                    cSlots.Add(new MemberProjectedSlot(cAttributeNode));
                    sSlots.Add(new MemberProjectedSlot(sAttributeNode));
                }

                // Note: S-side constants are not allowed since they can cause
                // problems -- for example, if such a cell says 5 for the
                // third field, we cannot guarantee the fact that an
                // application may not set that field to 7 in the C-space

                // Check if the property mapping is for a complex types
                if (complexPropMap != null)
                {
                    foreach (StorageComplexTypeMapping complexTypeMap in complexPropMap.TypeMappings)
                    {
                        // Create a node for the complex type property and call recursively
                        MemberPath complexMemberNode = new MemberPath(cNode, complexPropMap.EdmProperty);
                        //Get the list of types that this type map represents
                        Set<EdmType> allTypes = new Set<EdmType>();
                        // Gather a set of all explicit types for an entity
                        // type mapping in allTypes.
                        IEnumerable<EdmType> exactTypes = Helpers.AsSuperTypeList<ComplexType, EdmType>(complexTypeMap.Types);
                        allTypes.AddRange(exactTypes);
                        foreach (EdmType type in complexTypeMap.IsOfTypes)
                        {
                            allTypes.AddRange(MetadataHelper.GetTypeAndSubtypesOf(type, m_containerMapping.StorageMappingItemCollection.EdmItemCollection, false /*includeAbstractTypes*/));
                        }
                        BoolExpression complexInTypes = BoolExpression.CreateLiteral(new TypeRestriction(complexMemberNode, allTypes), null);
                        cQueryWhereClause = BoolExpression.CreateAnd(cQueryWhereClause, complexInTypes);
                        // Now extract the properties of the complex type
                        // (which could have other complex types)
                        ExtractProperties(complexTypeMap.AllProperties, complexMemberNode, cSlots,
                                          ref cQueryWhereClause, sRootExtent, sSlots, ref sQueryWhereClause);
                    }
                }

                // Check if the property mapping is for an associaion
                if (associationEndPropertypMap != null)
                {
                    // create join tree node representing this relation end
                    MemberPath associationEndNode = new MemberPath(cNode, associationEndPropertypMap.EndMember);
                    // call recursively
                    ExtractProperties(associationEndPropertypMap.Properties, associationEndNode, cSlots,
                                      ref cQueryWhereClause, sRootExtent, sSlots, ref sQueryWhereClause);
                }

                //Check if the this is a condition and add it to the Where clause
                if (conditionMap != null)
                {
                    if (conditionMap.ColumnProperty != null)
                    {
                        //Produce a Condition Expression for the Condition Map.
                        BoolExpression conditionExpression = GetConditionExpression(sRootExtent, conditionMap);
                        //Add the condition expression to the exisiting S side Where clause using an "And"
                        sQueryWhereClause = BoolExpression.CreateAnd(sQueryWhereClause, conditionExpression);
                    }
                    else
                    {
                        Debug.Assert(conditionMap.EdmProperty != null);
                        //Produce a Condition Expression for the Condition Map.
                        BoolExpression conditionExpression = GetConditionExpression(cNode, conditionMap);
                        //Add the condition expression to the exisiting C side Where clause using an "And"
                        cQueryWhereClause = BoolExpression.CreateAnd(cQueryWhereClause, conditionExpression);
                    }

                }
            }
        }

        /// <summary>
        /// Takes in a JoinTreeNode and a Contition Property Map and creates an BoolExpression
        /// for the Condition Map.
        /// </summary>
        /// <param name="joinTreeNode"></param>
        /// <param name="conditionMap"></param>
        /// <returns></returns>
        private static BoolExpression GetConditionExpression(MemberPath member, StorageConditionPropertyMapping conditionMap)
        {
            //Get the member for which the condition is being specified
            EdmMember conditionMember = (conditionMap.ColumnProperty != null) ? conditionMap.ColumnProperty : conditionMap.EdmProperty;

            MemberPath conditionMemberNode = new MemberPath(member, conditionMember);
            //Check if this is a IsNull condition
            MemberRestriction conditionExpression = null;
            if (conditionMap.IsNull.HasValue)
            {
                // for conditions on scalars, create NodeValue nodes, otherwise NodeType
                Constant conditionConstant = (true == conditionMap.IsNull.Value) ? Constant.Null : Constant.NotNull;
                if (true == MetadataHelper.IsNonRefSimpleMember(conditionMember))
                {
                    conditionExpression = new ScalarRestriction(conditionMemberNode, conditionConstant);
                }
                else
                {
                    conditionExpression = new TypeRestriction(conditionMemberNode, conditionConstant);
                }
            }
            else
            {
                conditionExpression = new ScalarRestriction(conditionMemberNode, new ScalarConstant(conditionMap.Value));
            }

            Debug.Assert(conditionExpression != null);

            return BoolExpression.CreateLiteral(conditionExpression, null);
        }

        private static bool IsBooleanMember(MemberPath path)
        {
            PrimitiveType primitive = path.EdmType as PrimitiveType;
            return (primitive != null && primitive.PrimitiveTypeKind == PrimitiveTypeKind.Boolean);
        }
        #endregion

        #region String methods
        internal override void ToCompactString(System.Text.StringBuilder builder)
        {
            builder.Append("CellCreator"); // No state to really show i.e., m_maps
        }

        #endregion

    }
}
