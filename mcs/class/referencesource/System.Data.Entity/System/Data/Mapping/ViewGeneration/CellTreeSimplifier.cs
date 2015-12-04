//---------------------------------------------------------------------
// <copyright file="CellTreeSimplifier.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System.Data.Common.Utils;
using System.Data.Mapping.ViewGeneration.Structures;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Data.Metadata.Edm;
using System.Linq;

namespace System.Data.Mapping.ViewGeneration
{

    // This class simplifies an extent's view. Given a view, runs the TM/SP
    // rules to remove unnecessary self-joins or self-unions
    internal class CellTreeSimplifier : InternalBase
    {

        #region Fields
        private ViewgenContext m_viewgenContext;
        #endregion


        #region Constructor
        private CellTreeSimplifier(ViewgenContext context)
        {
            m_viewgenContext = context;
        }
        #endregion

        #region Exposed Methods
        // effects: see CellTreeNode.Simplify below
        internal static CellTreeNode MergeNodes(CellTreeNode rootNode)
        {
            CellTreeSimplifier simplifier = new CellTreeSimplifier(rootNode.ViewgenContext);
            return simplifier.SimplifyTreeByMergingNodes(rootNode);
        }

        // effects: Simplifies the tree rooted at rootNode and returns a new
        // tree -- it ensures that the returned tree has at most one node for
        // any particular extent unless the tree has nodes of the same extent
        // embedded two leaves below LASJ or LOJ, e.g., if we have a tree
        // (where Ni indicates a node for extent i - one Ni can be different
        // from anohter Ni: 
        // [N0 IJ N1] LASJ N0 --> This will not be simplified
        // canBooleansOverlap indicates whether an original input cell
        // contributes to multiple nodes in this tree, e.g., V1 IJ V2 UNION V2 IJ V3
        private CellTreeNode SimplifyTreeByMergingNodes(CellTreeNode rootNode)
        {

            if (rootNode is LeafCellTreeNode)
            { // View already simple!
                return rootNode;
            }
            Debug.Assert(rootNode.OpType == CellTreeOpType.LOJ || rootNode.OpType == CellTreeOpType.IJ ||
                         rootNode.OpType == CellTreeOpType.FOJ || rootNode.OpType == CellTreeOpType.Union ||
                         rootNode.OpType == CellTreeOpType.LASJ,
                         "Only handle these operations");

            // Before we apply any rule, check if we can improve the opportunity to
            // collapse the nodes
            rootNode = RestructureTreeForMerges(rootNode);

            List<CellTreeNode> children = rootNode.Children;
            Debug.Assert(children.Count > 0, "OpCellTreeNode has no children?");

            // Apply recursively
            for (int i = 0; i < children.Count; i++)
            {
                children[i] = SimplifyTreeByMergingNodes(children[i]);
            }

            // Essentially, we have a node with IJ, LOJ, U or FOJ type that
            // has some children. Check if some of the children can be merged
            // with one another using the corresponding TM/SP rule

            // Ops such as IJ, Union and FOJ are associative, i.e., A op (B
            // op C) is the same as (A op B) op C. This is not true for LOJ
            // and LASJ
            bool isAssociativeOp = CellTreeNode.IsAssociativeOp(rootNode.OpType);
            if (isAssociativeOp)
            {
                // Group all the leaf cells of an extent together so that we can
                // later simply run through them without running nested loops
                // We do not do this for LOJ/LASJ nodes since LOJ (LASJ) is not commutative
                // (or associative);
                children = GroupLeafChildrenByExtent(children);
            }
            else
            {
                children = GroupNonAssociativeLeafChildren(children);
            }

            // childrenSet keeps track of the children that need to be procesed/partitioned
            OpCellTreeNode newNode = new OpCellTreeNode(m_viewgenContext, rootNode.OpType);
            CellTreeNode lastChild = null;
            bool skipRest = false;
            foreach (CellTreeNode child in children)
            {
                if (lastChild == null)
                {
                    // First time in the loop. Just set lastChild
                    lastChild = child;
                    continue;
                }

                bool mergedOk = false;
                // try to merge lastChild and child
                if (false == skipRest && lastChild.OpType == CellTreeOpType.Leaf &&
                    child.OpType == CellTreeOpType.Leaf)
                {
                    // Both are cell queries. Can try to merge them
                    // We do not add lastChild since it could merge
                    // further. It will be added in a later loop or outside the loop
                    mergedOk = TryMergeCellQueries(rootNode.OpType, ref lastChild, child);
                }

                if (false == mergedOk)
                {
                    // No merge occurred. Simply add the previous child as it
                    // is (Note lastChild will be added in the next loop or if
                    // the loop finishes, outside the loop
                    newNode.Add(lastChild);
                    lastChild = child;
                    if (false == isAssociativeOp)
                    {
                        // LOJ is not associative:
                        // (P loj PA) loj PO != P loj (PA loj PO). The RHS does not have
                        // Persons who have orders but no addresses
                        skipRest = true;
                    }
                }
            }

            newNode.Add(lastChild);
            CellTreeNode result = newNode.AssociativeFlatten();
            return result;
        }
        #endregion

        #region Private Methods
        // effects: Restructure tree so that it is better positioned for merges
        private CellTreeNode RestructureTreeForMerges(CellTreeNode rootNode)
        {
            List<CellTreeNode> children = rootNode.Children;
            if (CellTreeNode.IsAssociativeOp(rootNode.OpType) == false || children.Count <= 1)
            {
                return rootNode;
            }

            // If this node's operator is associative and each child's
            // operator is also associative, check if there is a common set
            // of leaf nodes across all grandchildren

            Set<LeafCellTreeNode> commonGrandChildren = GetCommonGrandChildren(children);
            if (commonGrandChildren == null)
            {
                return rootNode;
            }

            CellTreeOpType commonChildOpType = children[0].OpType;

            //  We do have the structure that we are looking for
            // (common op2 gc2) op1 (common op2 gc3) op1 (common op2 gc4) becomes
            // common op2 (gc2 op1 gc3 op1 gc4)
            // e.g., (A IJ B IJ X IJ Y) UNION (A IJ B IJ Y IJ Z) UNION (A IJ B IJ R IJ S)
            // becomes A IJ B IJ ((X IJ Y) UNION (Y IJ Z) UNION (R IJ S))

            // From each child in children, get the nodes other than commonGrandChildren - these are gc2, gc3, ...
            // Each gc2 must be connected by op2 as before, i.e., ABC + ACD = A(BC + CD)

            // All children must be OpCellTreeNodes!
            List<OpCellTreeNode> newChildren = new List<OpCellTreeNode>(children.Count);
            foreach (OpCellTreeNode child in children)
            {
                // Remove all children in child that belong to commonGrandChildren
                // All grandChildren must be leaf nodes at this point
                List<LeafCellTreeNode> newGrandChildren = new List<LeafCellTreeNode>(child.Children.Count);
                foreach (LeafCellTreeNode grandChild in child.Children)
                {
                    if (commonGrandChildren.Contains(grandChild) == false)
                    {
                        newGrandChildren.Add(grandChild);
                    }
                }
                // In the above example, child.OpType is IJ
                Debug.Assert(child.OpType == commonChildOpType);
                OpCellTreeNode newChild = new OpCellTreeNode(m_viewgenContext, child.OpType,
                                                             Helpers.AsSuperTypeList<LeafCellTreeNode, CellTreeNode>(newGrandChildren));
                newChildren.Add(newChild);
            }
            // Connect gc2 op1 gc3 op1 gc4 - op1 is UNION in this
            // ((X IJ Y) UNION (Y IJ Z) UNION (R IJ S))
            // rootNode.Type is UNION
            CellTreeNode remainingNodes = new OpCellTreeNode(m_viewgenContext, rootNode.OpType,
                                                             Helpers.AsSuperTypeList<OpCellTreeNode, CellTreeNode>(newChildren));
            // Take the common grandchildren and connect via commonChildType
            // i.e., A IJ B
            CellTreeNode commonNodes = new OpCellTreeNode(m_viewgenContext, commonChildOpType,
                                                            Helpers.AsSuperTypeList<LeafCellTreeNode, CellTreeNode>(commonGrandChildren));

            // Connect both by commonChildType
            CellTreeNode result = new OpCellTreeNode(m_viewgenContext, commonChildOpType,
                                                     new CellTreeNode[] { commonNodes, remainingNodes });

            result = result.AssociativeFlatten();
            return result;
        }

        // effects: Given a set of nodes, determines if all nodes are the exact same associative opType AND
        // there are leaf children that are common across the children "nodes". If there are any,
        // returns them. Else return null
        private static Set<LeafCellTreeNode> GetCommonGrandChildren(List<CellTreeNode> nodes)
        {
            Set<LeafCellTreeNode> commonLeaves = null;

            // We could make this general and apply recursively but we don't for now

            // Look for a tree of the form: (common op2 gc2) op1 (common op2 gc3) op1 (common op2 gc4)
            // e.g., (A IJ B IJ X IJ Y) UNION (A IJ B IJ Y IJ Z) UNION (A IJ B IJ R IJ S)
            // Where op1 and op2 are associative and common, gc2 etc are leaf nodes
            CellTreeOpType commonChildOpType = CellTreeOpType.Leaf;

            foreach (CellTreeNode node in nodes)
            {
                OpCellTreeNode opNode = node as OpCellTreeNode;
                if (opNode == null)
                {
                    return null;
                }
                Debug.Assert(opNode.OpType != CellTreeOpType.Leaf, "Leaf type for op cell node?");
                // Now check for whether the op is associative and the same as the previous one
                if (commonChildOpType == CellTreeOpType.Leaf)
                {
                    commonChildOpType = opNode.OpType;
                }
                else if (CellTreeNode.IsAssociativeOp(opNode.OpType) == false || commonChildOpType != opNode.OpType)
                {
                    return null;
                }

                // Make sure all the children are leaf children
                Set<LeafCellTreeNode> nodeChildrenSet = new Set<LeafCellTreeNode>(LeafCellTreeNode.EqualityComparer);
                foreach (CellTreeNode grandChild in opNode.Children)
                {
                    LeafCellTreeNode leafGrandChild = grandChild as LeafCellTreeNode;
                    if (leafGrandChild == null)
                    {
                        return null;
                    }
                    nodeChildrenSet.Add(leafGrandChild);
                }

                if (commonLeaves == null)
                {
                    commonLeaves = nodeChildrenSet;
                }
                else
                {
                    commonLeaves.Intersect(nodeChildrenSet);
                }
            }

            if (commonLeaves.Count == 0)
            {
                // No restructuring possible
                return null;
            }
            return commonLeaves;
        }
        // effects: Given a list of node, produces a new list in which all
        // leaf nodes of the same extent are adjacent to each other. Non-leaf
        // nodes are also adjacent to each other. CHANGE_[....]_IMPROVE: Merge with GroupByRightExtent
        private static List<CellTreeNode> GroupLeafChildrenByExtent(List<CellTreeNode> nodes)
        {
            // Keep track of leaf cells for each extent
            KeyToListMap<EntitySetBase, CellTreeNode> extentMap =
                new KeyToListMap<EntitySetBase, CellTreeNode>(EqualityComparer<EntitySetBase>.Default);

            List<CellTreeNode> newNodes = new List<CellTreeNode>();
            foreach (CellTreeNode node in nodes)
            {
                LeafCellTreeNode leafNode = node as LeafCellTreeNode;
                // All non-leaf nodes are added to the result now
                // leaf nodes are added outside the loop
                if (leafNode != null)
                {
                    extentMap.Add(leafNode.LeftCellWrapper.RightCellQuery.Extent, leafNode);
                }
                else
                {
                    newNodes.Add(node);
                }
            }
            // Go through the map and add the leaf children
            newNodes.AddRange(extentMap.AllValues);
            return newNodes;
        }

        // effects: A restrictive version of GroupLeafChildrenByExtent --
        // only for LASJ and LOJ nodes (works for LOJ only when A LOJ B LOJ C
        // s.t., B and C are subsets of A -- in our case that is how LOJs are constructed
        private static List<CellTreeNode> GroupNonAssociativeLeafChildren(List<CellTreeNode> nodes)
        {
            // Keep track of leaf cells for each extent ignoring the 0th child
            KeyToListMap<EntitySetBase, CellTreeNode> extentMap =
                new KeyToListMap<EntitySetBase, CellTreeNode>(EqualityComparer<EntitySetBase>.Default);

            List<CellTreeNode> newNodes = new List<CellTreeNode>();
            List<CellTreeNode> nonLeafNodes = new List<CellTreeNode>();
            // Add the 0th child
            newNodes.Add(nodes[0]);
            for (int i = 1; i < nodes.Count; i++)
            {
                CellTreeNode node = nodes[i];
                LeafCellTreeNode leafNode = node as LeafCellTreeNode;
                // All non-leaf nodes are added to the result now
                // leaf nodes are added outside the loop
                if (leafNode != null)
                {
                    extentMap.Add(leafNode.LeftCellWrapper.RightCellQuery.Extent, leafNode);
                }
                else
                {
                    nonLeafNodes.Add(node);
                }
            }
            // Go through the map and add the leaf children
            // If a group of nodes exists for the 0th node's extent -- place
            // that group first
            LeafCellTreeNode firstNode = nodes[0] as LeafCellTreeNode;
            if (firstNode != null)
            {
                EntitySetBase firstExtent = firstNode.LeftCellWrapper.RightCellQuery.Extent;
                if (extentMap.ContainsKey(firstExtent))
                {
                    newNodes.AddRange(extentMap.ListForKey(firstExtent));
                    // Remove this set from the map
                    extentMap.RemoveKey(firstExtent);
                }
            }
            newNodes.AddRange(extentMap.AllValues);
            newNodes.AddRange(nonLeafNodes);
            return newNodes;
        }

        // requires: node1 and node2 are two children of the same parent
        // connected by opType
        // effects: Given two cell tree nodes, node1 and node2, runs the
        // TM/SP rule on them to merge them (if they belong to the same
        // extent). Returns true if the merge succeeds
        private bool TryMergeCellQueries(CellTreeOpType opType, ref CellTreeNode node1,
                                         CellTreeNode node2)
        {

            LeafCellTreeNode leaf1 = node1 as LeafCellTreeNode;
            LeafCellTreeNode leaf2 = node2 as LeafCellTreeNode;

            Debug.Assert(leaf1 != null, "Merge only possible on leaf nodes (1)");
            Debug.Assert(leaf2 != null, "Merge only possible on leaf nodes (2)");

            CellQuery mergedLeftCellQuery;
            CellQuery mergedRightCellQuery;
            if (!TryMergeTwoCellQueries(leaf1.LeftCellWrapper.RightCellQuery, leaf2.LeftCellWrapper.RightCellQuery, opType, m_viewgenContext.MemberMaps.RightDomainMap, out mergedRightCellQuery))
            {
                return false;
            }

            if (!TryMergeTwoCellQueries(leaf1.LeftCellWrapper.LeftCellQuery, leaf2.LeftCellWrapper.LeftCellQuery, opType, m_viewgenContext.MemberMaps.LeftDomainMap, out mergedLeftCellQuery))
            {
                return false;
            }

            // Create a temporary node and add the two children
            // so that we can get the merged selectiondomains and attributes
            // Note that temp.SelectionDomain below determines the domain
            // based on the opType, e.g., for IJ, it intersects the
            // multiconstants of all the children
            OpCellTreeNode temp = new OpCellTreeNode(m_viewgenContext, opType);
            temp.Add(node1);
            temp.Add(node2);
            // Note: We are losing the original cell number information here and the line number information
            // But we will not raise any

            // We do not create CellExpressions with LOJ, FOJ - canBooleansOverlap is true for validation
            CellTreeOpType inputOpType = opType;
            if (opType == CellTreeOpType.FOJ || opType == CellTreeOpType.LOJ)
            {
                inputOpType = CellTreeOpType.IJ;
            }

            LeftCellWrapper wrapper = new LeftCellWrapper(m_viewgenContext.ViewTarget, temp.Attributes,
                                                          temp.LeftFragmentQuery,
                                                          mergedLeftCellQuery,
                                                          mergedRightCellQuery,
                                                          m_viewgenContext.MemberMaps,
                                                          leaf1.LeftCellWrapper.Cells.Concat(leaf2.LeftCellWrapper.Cells));
            node1 = new LeafCellTreeNode(m_viewgenContext, wrapper, temp.RightFragmentQuery);
            return true;
        }


        // effects: Merges query2 with this according to the TM/SP rules for opType and
        // returns the merged result. canBooleansOverlap indicates whether the bools in this and query2 can overlap, i.e.
        // the same cells may have contributed to query2 and this earlier in the merge process
        internal bool TryMergeTwoCellQueries(CellQuery query1, CellQuery query2, CellTreeOpType opType,
                               MemberDomainMap memberDomainMap, out CellQuery mergedQuery)
        {

            mergedQuery = null;
            // Initialize g1 and g2 according to the TM/SP rules for IJ, LOJ, Union, FOJ cases
            BoolExpression g1 = null;
            BoolExpression g2 = null;
            switch (opType)
            {
                case CellTreeOpType.IJ:
                    break;
                case CellTreeOpType.LOJ:
                case CellTreeOpType.LASJ:
                    g2 = BoolExpression.True;
                    break;
                case CellTreeOpType.FOJ:
                case CellTreeOpType.Union:
                    g1 = BoolExpression.True;
                    g2 = BoolExpression.True;
                    break;
                default:
                    Debug.Fail("Unsupported operator");
                    break;
            }

            Dictionary<MemberPath, MemberPath> remap =
                new Dictionary<MemberPath, MemberPath>(MemberPath.EqualityComparer);

            //Continue merging only if both queries are over the same source
            MemberPath newRoot;
            if (!query1.Extent.Equals(query2.Extent))
            { // could not merge
                return false;
            }
            else
            {
                newRoot = query1.SourceExtentMemberPath;
            }

            // Conjuncts for ANDing with the previous whereClauses
            BoolExpression conjunct1 = BoolExpression.True;
            BoolExpression conjunct2 = BoolExpression.True;
            BoolExpression whereClause = null;

            switch (opType)
            {
                case CellTreeOpType.IJ:
                    // Project[D1, D2, A, B, C] Select[cond1 and cond2] (T)
                    // We simply merge the two lists of booleans -- no conjuct is added
                    // conjunct1 and conjunct2 don't change

                    // query1.WhereCaluse AND query2.WhereCaluse
                    Debug.Assert(g1 == null && g2 == null, "IJ does not affect g1 and g2");
                    whereClause = BoolExpression.CreateAnd(query1.WhereClause, query2.WhereClause);
                    break;

                case CellTreeOpType.LOJ:
                    // conjunct1 does not change since D1 remains as is
                    // Project[D1, (expr2 and cond2 and G2) as D2, A, B, C] Select[cond1] (T)
                    // D1 does not change. New d2 is the list of booleans expressions
                    // for query2 ANDed with g2 AND query2.WhereClause
                    Debug.Assert(g1 == null, "LOJ does not affect g1");
                    conjunct2 = BoolExpression.CreateAnd(query2.WhereClause, g2);
                    // Just query1's whereclause
                    whereClause = query1.WhereClause;
                    break;

                case CellTreeOpType.FOJ:
                case CellTreeOpType.Union:
                    // Project[(expr1 and cond1 and G1) as D1, (expr2 and cond2 and G2) as D2, A, B, C] Select[cond1] (T)
                    // New D1 is a list -- newD1 = D1 AND query1.WhereClause AND g1
                    // New D1 is a list -- newD2 = D2 AND query2.WhereClause AND g2
                    conjunct1 = BoolExpression.CreateAnd(query1.WhereClause, g1);
                    conjunct2 = BoolExpression.CreateAnd(query2.WhereClause, g2);

                    // The new whereClause -- g1 AND query1.WhereCaluse OR g2 AND query2.WhereClause
                    whereClause = BoolExpression.CreateOr(BoolExpression.CreateAnd(query1.WhereClause, g1),
                                                          BoolExpression.CreateAnd(query2.WhereClause, g2));
                    break;

                case CellTreeOpType.LASJ:
                    // conjunct1 does not change since D1 remains as is
                    // Project[D1, (expr2 and cond2 and G2) as D2, A, B, C] Select[cond1] (T)
                    // D1 does not change. New d2 is the list of booleans expressions
                    // for query2 ANDed with g2 AND NOT query2.WhereClause
                    Debug.Assert(g1 == null, "LASJ does not affect g1");
                    conjunct2 = BoolExpression.CreateAnd(query2.WhereClause, g2);
                    whereClause = BoolExpression.CreateAnd(query1.WhereClause, BoolExpression.CreateNot(conjunct2));
                    break;
                default:
                    Debug.Fail("Unsupported operator");
                    break;
            }

            // Create the various remapped parts for the cell query --
            // boolean expressions, merged slots, whereclause, duplicate
            // elimination, join tree
            List<BoolExpression> boolExprs =
                MergeBoolExpressions(query1, query2, conjunct1, conjunct2, opType);
            //BoolExpression.RemapBools(boolExprs, remap);

            ProjectedSlot[] mergedSlots;
            if (false == ProjectedSlot.TryMergeRemapSlots(query1.ProjectedSlots, query2.ProjectedSlots, out mergedSlots))
            {
                // merging failed because two different right slots go to same left slot
                return false;
            }

            whereClause = whereClause.RemapBool(remap);

            CellQuery.SelectDistinct elimDupl = MergeDupl(query1.SelectDistinctFlag, query2.SelectDistinctFlag);

            whereClause.ExpensiveSimplify();
            mergedQuery = new CellQuery(mergedSlots, whereClause,
                                                  boolExprs, elimDupl, newRoot);
            return true;
        }

        // effects: Given two duplicate eliination choices, returns an OR of them
        static private CellQuery.SelectDistinct MergeDupl(CellQuery.SelectDistinct d1, CellQuery.SelectDistinct d2)
        {
            if (d1 == CellQuery.SelectDistinct.Yes || d2 == CellQuery.SelectDistinct.Yes)
            {
                return CellQuery.SelectDistinct.Yes;
            }
            else
            {
                return CellQuery.SelectDistinct.No;
            }
        }

        // requires: query1 has the same number of boolean expressions as
        // query2. There should be no  index i for which query1's bools[i] !=
        // null and query2's bools[i] != null
        // effects: Given two cellqueries query1 and query2, merges their
        // boolean expressions while ANDING query1 bools with conjunct1 and
        // query2's bools with conjunct2 and returns the result
        static private List<BoolExpression>
        MergeBoolExpressions(CellQuery query1, CellQuery query2,
                             BoolExpression conjunct1, BoolExpression conjunct2, CellTreeOpType opType)
        {

            List<BoolExpression> bools1 = query1.BoolVars;
            List<BoolExpression> bools2 = query2.BoolVars;

            // Add conjuncts to both sets if needed
            if (false == conjunct1.IsTrue)
            {
                bools1 = BoolExpression.AddConjunctionToBools(bools1, conjunct1);
            }

            if (false == conjunct2.IsTrue)
            {
                bools2 = BoolExpression.AddConjunctionToBools(bools2, conjunct2);
            }

            // Perform merge
            Debug.Assert(bools1.Count == bools2.Count);
            List<BoolExpression> bools = new List<BoolExpression>();
            // Both bools1[i] and bools2[i] be null for some of the i's. When
            // we merge two (leaf) cells (say), only one boolean each is set
            // in it; the rest are all nulls. If the SP/TM rules have been
            // applied, more than one boolean may be non-null in a cell query
            for (int i = 0; i < bools1.Count; i++)
            {
                BoolExpression merged = null;
                if (bools1[i] == null)
                {
                    merged = bools2[i];
                }
                else if (bools2[i] == null)
                {
                    merged = bools1[i];
                }
                else
                {
                    if (opType == CellTreeOpType.IJ)
                    {
                        merged = BoolExpression.CreateAnd(bools1[i], bools2[i]);
                    }
                    else if (opType == CellTreeOpType.Union)
                    {
                        merged = BoolExpression.CreateOr(bools1[i], bools2[i]);
                    }
                    else if (opType == CellTreeOpType.LASJ)
                    {
                        merged = BoolExpression.CreateAnd(bools1[i],
                                                          BoolExpression.CreateNot(bools2[i]));
                    }
                    else
                    {
                        Debug.Fail("No other operation expected for boolean merge");
                    }
                }
                if (merged != null)
                {
                    merged.ExpensiveSimplify();
                }
                bools.Add(merged);
            }
            return bools;
        }

        #endregion

        #region String methods
        internal override void ToCompactString(StringBuilder builder)
        {
            m_viewgenContext.MemberMaps.ProjectedSlotMap.ToCompactString(builder);
        }
        #endregion

    }
}
