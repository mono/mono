//---------------------------------------------------------------------
// <copyright file="LeafCellTreeNode.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System.Collections.Generic;
using System.Data.Common.Utils;
using System.Data.Mapping.ViewGeneration.CqlGeneration;
using System.Data.Mapping.ViewGeneration.QueryRewriting;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Data.Metadata.Edm;


namespace System.Data.Mapping.ViewGeneration.Structures
{
    // This class represents the nodes that reside at the leaves of the tree
    internal class LeafCellTreeNode : CellTreeNode
    {

        #region Constructor
        // effects: Encapsulate the cell wrapper in the node
        internal LeafCellTreeNode(ViewgenContext context, LeftCellWrapper cellWrapper)
            : base(context)
        {
            m_cellWrapper = cellWrapper;
            m_leftFragmentQuery = cellWrapper.FragmentQuery;
            cellWrapper.AssertHasUniqueCell();
            m_rightFragmentQuery = FragmentQuery.Create(
                                        cellWrapper.OriginalCellNumberString,
                                        cellWrapper.CreateRoleBoolean(),
                                        cellWrapper.RightCellQuery);
        }
        internal LeafCellTreeNode(ViewgenContext context, LeftCellWrapper cellWrapper, FragmentQuery rightFragmentQuery)
            : base(context)
        {
            m_cellWrapper = cellWrapper;
            m_leftFragmentQuery = cellWrapper.FragmentQuery;
            m_rightFragmentQuery = rightFragmentQuery;
        }
        #endregion

        #region Fields
        internal static readonly IEqualityComparer<LeafCellTreeNode> EqualityComparer = new LeafCellTreeNodeComparer();

        // The cell at the leaf level
        private LeftCellWrapper m_cellWrapper;
        private FragmentQuery m_leftFragmentQuery;
        private FragmentQuery m_rightFragmentQuery;
        #endregion

        #region Properties
        internal LeftCellWrapper LeftCellWrapper
        {
            get { return m_cellWrapper; }
        }

        internal override MemberDomainMap RightDomainMap
        {
            get { return m_cellWrapper.RightDomainMap; }
        }

        // effects: See CellTreeNode.FragmentQuery
        internal override FragmentQuery LeftFragmentQuery { get { return m_cellWrapper.FragmentQuery; } }

        internal override FragmentQuery RightFragmentQuery
        {
            get
            {
                Debug.Assert(m_rightFragmentQuery != null, "Unassigned right fragment query");
                return m_rightFragmentQuery;
            }
        }

        // effects: See CellTreeNode.Attributes
        internal override Set<MemberPath> Attributes { get { return m_cellWrapper.Attributes; } }

        // effects: See CellTreeNode.Children
        internal override List<CellTreeNode> Children { get { return new List<CellTreeNode>(); } }

        // effects: See CellTreeNode.OpType
        internal override CellTreeOpType OpType { get { return CellTreeOpType.Leaf; } }

        internal override int NumProjectedSlots
        {
            get { return LeftCellWrapper.RightCellQuery.NumProjectedSlots; }
        }

        internal override int NumBoolSlots
        {
            get { return LeftCellWrapper.RightCellQuery.NumBoolVars; }
        }
        #endregion

        #region Methods
        internal override TOutput Accept<TInput, TOutput>(CellTreeVisitor<TInput, TOutput> visitor, TInput param)
        {
            return visitor.VisitLeaf(this, param);
        }

        internal override TOutput Accept<TInput, TOutput>(SimpleCellTreeVisitor<TInput, TOutput> visitor, TInput param)
        {
            return visitor.VisitLeaf(this, param);
        }

        internal override bool IsProjectedSlot(int slot)
        {
            CellQuery cellQuery = LeftCellWrapper.RightCellQuery;
            if (IsBoolSlot(slot))
            {
                return cellQuery.GetBoolVar(SlotToBoolIndex(slot)) != null;
            }
            else
            {
                return cellQuery.ProjectedSlotAt(slot) != null;
            }
        }
        #endregion

        #region Leaf CqlBlock Methods
        internal override CqlBlock ToCqlBlock(bool[] requiredSlots, CqlIdentifiers identifiers, ref int blockAliasNum, ref List<WithRelationship> withRelationships)
        {
            // Get the projected slots and the boolean expressions
            int totalSlots = requiredSlots.Length;
            CellQuery cellQuery = LeftCellWrapper.RightCellQuery;

            SlotInfo[] projectedSlots = new SlotInfo[totalSlots];
            Debug.Assert(cellQuery.NumProjectedSlots + cellQuery.NumBoolVars == totalSlots,
                         "Wrong number of projected slots in node");

            Debug.Assert(cellQuery.NumProjectedSlots == ProjectedSlotMap.Count,
                         "Different number of slots in cell query and what we have mappings for");
            // Add the regular fields
            for (int i = 0; i < cellQuery.NumProjectedSlots; i++)
            {
                ProjectedSlot slot = cellQuery.ProjectedSlotAt(i);
                // If the slot is not null, we will project it
                // For extents, we say that all requiredlots are the only the
                // ones that are CLR non-null. Recall that "real" nulls are
                // handled by having a CellConstant.Null in ConstantSlot
                if (requiredSlots[i] && slot == null)
                {
                    MemberPath memberPath = ProjectedSlotMap[i];
                    ConstantProjectedSlot defaultValue = new ConstantProjectedSlot(Domain.GetDefaultValueForMemberPath(memberPath, GetLeaves(), ViewgenContext.Config), memberPath);
                    cellQuery.FixMissingSlotAsDefaultConstant(i, defaultValue);
                    slot = defaultValue;
                }
                SlotInfo slotInfo = new SlotInfo(requiredSlots[i], slot != null,
                                                 slot, ProjectedSlotMap[i]);
                projectedSlots[i] = slotInfo;
            }

            // Add the boolean fields
            for (int boolNum = 0; boolNum < cellQuery.NumBoolVars; boolNum++)
            {
                BoolExpression expr = cellQuery.GetBoolVar(boolNum);
                BooleanProjectedSlot boolSlot;
                if (expr != null)
                {
                    boolSlot = new BooleanProjectedSlot(expr, identifiers, boolNum);
                }
                else
                {
                    boolSlot = new BooleanProjectedSlot(BoolExpression.False, identifiers, boolNum);
                }
                int slotIndex = BoolIndexToSlot(boolNum);
                SlotInfo slotInfo = new SlotInfo(requiredSlots[slotIndex], expr != null,
                                                 boolSlot, null);
                projectedSlots[slotIndex] = slotInfo;
            }

            // See if we are generating a query view and whether there are any colocated foreign keys for which
            // we have to add With statements.
            IEnumerable<SlotInfo> totalProjectedSlots = projectedSlots;
            if ((cellQuery.Extent.EntityContainer.DataSpace == DataSpace.SSpace)
                && (this.m_cellWrapper.LeftExtent.BuiltInTypeKind == BuiltInTypeKind.EntitySet))
            {
                IEnumerable<StorageAssociationSetMapping> associationSetMaps =
                    this.ViewgenContext.EntityContainerMapping.GetRelationshipSetMappingsFor(this.m_cellWrapper.LeftExtent, cellQuery.Extent);
                List<SlotInfo> foreignKeySlots = new List<SlotInfo>();
                foreach (StorageAssociationSetMapping colocatedAssociationSetMap in associationSetMaps)
                {
                    WithRelationship withRelationship;
                    if (TryGetWithRelationship(colocatedAssociationSetMap, this.m_cellWrapper.LeftExtent, cellQuery.SourceExtentMemberPath, ref foreignKeySlots, out withRelationship))
                    {
                        withRelationships.Add(withRelationship);
                        totalProjectedSlots = projectedSlots.Concat(foreignKeySlots);
                    }
                }
            }
            ExtentCqlBlock result = new ExtentCqlBlock(cellQuery.Extent, cellQuery.SelectDistinctFlag, totalProjectedSlots.ToArray(),
                                                       cellQuery.WhereClause, identifiers, ++blockAliasNum);
            return result;
        }

        private bool TryGetWithRelationship(StorageAssociationSetMapping colocatedAssociationSetMap,
                                            EntitySetBase thisExtent,
                                            MemberPath sRootNode,
                                            ref List<SlotInfo> foreignKeySlots,
                                            out WithRelationship withRelationship)
        {
            Debug.Assert(foreignKeySlots != null);
            withRelationship = null;

            //Get the map for foreign key end
            StorageEndPropertyMapping foreignKeyEndMap = GetForeignKeyEndMapFromAssocitionMap(colocatedAssociationSetMap, thisExtent);
            if (foreignKeyEndMap == null || foreignKeyEndMap.EndMember.RelationshipMultiplicity == RelationshipMultiplicity.Many)
            {
                return false;
            }

            AssociationEndMember toEnd = (AssociationEndMember)foreignKeyEndMap.EndMember;
            AssociationEndMember fromEnd = MetadataHelper.GetOtherAssociationEnd(toEnd);
            EntityType toEndEntityType = (EntityType)((RefType)(toEnd.TypeUsage.EdmType)).ElementType;
            EntityType fromEndEntityType = (EntityType)(((RefType)fromEnd.TypeUsage.EdmType).ElementType);

            // Get the member path for AssociationSet
            AssociationSet associationSet = (AssociationSet)colocatedAssociationSetMap.Set;
            MemberPath prefix = new MemberPath(associationSet, toEnd);

            // Collect the member paths for edm scalar properties that belong to the target entity key.
            // These will be used as part of WITH RELATIONSHIP.
            // Get the key properties from edm type since the query parser depends on the order of key members
            IEnumerable<StorageScalarPropertyMapping> propertyMaps = foreignKeyEndMap.Properties.Cast<StorageScalarPropertyMapping>();
            List<MemberPath> toEndEntityKeyMemberPaths = new List<MemberPath>();
            foreach (EdmProperty edmProperty in toEndEntityType.KeyMembers)
            {
                IEnumerable<StorageScalarPropertyMapping> scalarPropertyMaps = propertyMaps.Where(propMap => (propMap.EdmProperty.Equals(edmProperty)));
                Debug.Assert(scalarPropertyMaps.Count() == 1, "Can't Map the same column multiple times in the same end");
                StorageScalarPropertyMapping scalarPropertyMap = scalarPropertyMaps.First();
                
                // Create SlotInfo for Freign Key member that needs to be projected.
                MemberProjectedSlot sSlot = new MemberProjectedSlot(new MemberPath(sRootNode, scalarPropertyMap.ColumnProperty));
                MemberPath endMemberKeyPath = new MemberPath(prefix, edmProperty);
                toEndEntityKeyMemberPaths.Add(endMemberKeyPath);
                foreignKeySlots.Add(new SlotInfo(true, true, sSlot, endMemberKeyPath));
            }

            // Parent assignable from child: Ensures they are in the same hierarchy.
            if (thisExtent.ElementType.IsAssignableFrom(fromEndEntityType))
            {
                // Now create the WITH RELATIONSHIP with all the needed info.
                withRelationship = new WithRelationship(associationSet, fromEnd, fromEndEntityType, toEnd, toEndEntityType, toEndEntityKeyMemberPaths);
                return true;
            }
            else
            {
                return false;
            }
        }

        //Gets the end that is not mapped to the primary key of the table
        private StorageEndPropertyMapping GetForeignKeyEndMapFromAssocitionMap(StorageAssociationSetMapping colocatedAssociationSetMap, EntitySetBase thisExtent)
        {
            StorageMappingFragment mapFragment = colocatedAssociationSetMap.TypeMappings.First().MappingFragments.First();
            EntitySet storeEntitySet = (EntitySet)(colocatedAssociationSetMap.StoreEntitySet);
            IEnumerable<EdmMember> keyProperties = storeEntitySet.ElementType.KeyMembers;
            //Find the end that's mapped to primary key
            foreach (StorageEndPropertyMapping endMap in mapFragment.Properties)
            {
                IEnumerable<EdmMember> endStoreMembers = endMap.StoreProperties;
                if (endStoreMembers.SequenceEqual(keyProperties, EqualityComparer<EdmMember>.Default))
                {
                    //Return the map for the other end since that is the foreign key end
                    IEnumerable<StorageEndPropertyMapping> otherEnds = mapFragment.Properties.OfType<StorageEndPropertyMapping>().Where(eMap => (!eMap.Equals(endMap)));
                    Debug.Assert(otherEnds.Count() == 1);
                    return otherEnds.First();
                }
            }
            //This is probably defensive, but there should be no problem in falling back on the 
            //AssociationSetMap if colocated foreign key is not found for some reason.
            return null;
        }
        #endregion

        #region String Methods
        // effects: See CellTreeNode.ToString
        internal override void ToCompactString(StringBuilder stringBuilder)
        {
            m_cellWrapper.ToCompactString(stringBuilder);
        }

        #endregion

        #region IEqualityComparer<LeafCellTreeNode>
        // A comparer that equates leaf nodes if the wrapper is the same
        private class LeafCellTreeNodeComparer : IEqualityComparer<LeafCellTreeNode>
        {

            public bool Equals(LeafCellTreeNode left, LeafCellTreeNode right)
            {
                // Quick check with references
                if (object.ReferenceEquals(left, right))
                {
                    // Gets the Null and Undefined case as well
                    return true;
                }
                // One of them is non-null at least
                if (left == null || right == null)
                {
                    return false;
                }
                // Both are non-null at this point
                return left.m_cellWrapper.Equals(right.m_cellWrapper);
            }

            public int GetHashCode(LeafCellTreeNode node)
            {
                return node.m_cellWrapper.GetHashCode();
            }
        }
        #endregion
    }
}
