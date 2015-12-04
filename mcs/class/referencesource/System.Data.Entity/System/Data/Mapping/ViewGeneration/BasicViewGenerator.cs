//---------------------------------------------------------------------
// <copyright file="BasicViewGenerator.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner [....]
// @backupOwner [....]
//---------------------------------------------------------------------

namespace System.Data.Mapping.ViewGeneration
{
    using System.Collections.Generic;
    using System.Data.Common.Utils;
    using System.Data.Entity;
    using System.Data.Mapping.ViewGeneration.QueryRewriting;
    using System.Data.Mapping.ViewGeneration.Structures;
    using System.Data.Mapping.ViewGeneration.Utils;
    using System.Data.Mapping.ViewGeneration.Validation;
    using System.Data.Metadata.Edm;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;

    // This class generates a view for an extent that may contain self-joins
    // and self-unions -- this can be later simplified or optimized
    // Output: A cell tree with LeftCellWrappers as nodes connected by Union, IJ,
    // LOJ, FOJs
    internal class BasicViewGenerator : InternalBase
    {

        #region Constructor
        // effects: Creates a view generator object that can be used to generate views
        // based on usedCells (projectedSlotMap are useful for deciphering the fields)
        internal BasicViewGenerator(MemberProjectionIndex projectedSlotMap, List<LeftCellWrapper> usedCells, FragmentQuery activeDomain,
                                    ViewgenContext context, MemberDomainMap domainMap, ErrorLog errorLog, ConfigViewGenerator config)
        {
            Debug.Assert(usedCells.Count > 0, "No used cells");
            m_projectedSlotMap = projectedSlotMap;
            m_usedCells = usedCells;
            m_viewgenContext = context;
            m_activeDomain = activeDomain;
            m_errorLog = errorLog;
            m_config = config;
            m_domainMap = domainMap;
        }
        #endregion

        #region Fields
        private MemberProjectionIndex m_projectedSlotMap;
        private List<LeftCellWrapper> m_usedCells;
        // Active domain comprises all multiconstants that need to be reconstructed
        private FragmentQuery m_activeDomain;
        // these two are temporarily needed for checking containment
        private ViewgenContext m_viewgenContext;
        private ErrorLog m_errorLog;
        private ConfigViewGenerator m_config;
        private MemberDomainMap m_domainMap;
        #endregion

        #region Properties
        private FragmentQueryProcessor LeftQP
        {
            get { return m_viewgenContext.LeftFragmentQP; }
        }
        #endregion

        #region Exposed Methods
        // effects: Given the set of used cells for an extent, returns a
        // view to generate that extent
        internal CellTreeNode CreateViewExpression()
        {
            // Create an initial FOJ group with all the used cells as children
            OpCellTreeNode fojNode = new OpCellTreeNode(m_viewgenContext, CellTreeOpType.FOJ);

            // Add all the used cells as children to fojNode. This is a valid
            // view for the extent. We later try to optimize it
            foreach (LeftCellWrapper cell in m_usedCells)
            {
                LeafCellTreeNode cellNode = new LeafCellTreeNode(m_viewgenContext, cell);
                fojNode.Add(cellNode);
            }

            //rootNode = GroupByNesting(rootNode);
            // Group cells by the "right" extent (recall that we are
            // generating the view for the left extent) so that cells of the
            // same extent are in the same subtree
            CellTreeNode rootNode = GroupByRightExtent(fojNode);
            
            // Change some of the FOJs to Unions, IJs and LOJs
            rootNode = IsolateUnions(rootNode);

            // The isolation with Union is different from IsolateUnions --
            // the above isolation finds collections of chidren in a
            // node and connects them by union. The below one only considers
            // two children at a time
            rootNode = IsolateByOperator(rootNode, CellTreeOpType.Union);
            rootNode = IsolateByOperator(rootNode, CellTreeOpType.IJ);
            rootNode = IsolateByOperator(rootNode, CellTreeOpType.LOJ);
            if (m_viewgenContext.ViewTarget == ViewTarget.QueryView)
            {
                rootNode = ConvertUnionsToNormalizedLOJs(rootNode);
            }

            return rootNode;
        }

        #endregion

        #region Private Methods
        // requires: The tree rooted at cellTreeNode is an FOJ tree of
        // LeafCellTreeNodes only, i.e., there is an FOJ node with the
        // children being LeafCellTreeNodes
        // 
        // effects: Given a tree rooted at rootNode, ensures that cells
        // of the same right extent are placed in their own subtree below
        // cellTreeNode. That is, if there are 3 cells of extent A and 2 of
        // extent B (i.e., 5 cells with an FOJ on it), the resulting tree has
        // an FOJ node with two children -- FOJ nodes. These FOJ nodes have 2
        // and 3 children
        internal CellTreeNode GroupByRightExtent(CellTreeNode rootNode)
        {
            // A dictionary that maps an extent to the nodes are from that extent
            // We want a ref comparer here
            KeyToListMap<EntitySetBase, LeafCellTreeNode> extentMap =
                new KeyToListMap<EntitySetBase, LeafCellTreeNode>(EqualityComparer<EntitySetBase>.Default);

            // CR_Meek_Low: method can be simplified (Map<Extent, OpCellTreeNode>, populate as you go)
            // (becomes self-documenting)
            // For each leaf child, find the extent of the child and place it
            // in extentMap
            foreach (LeafCellTreeNode childNode in rootNode.Children)
            {
                // A cell may contain P, P.PA -- we return P
                // CHANGE_[....]_FEATURE_COMPOSITION Need to fix for composition!!
                EntitySetBase extent = childNode.LeftCellWrapper.RightCellQuery.Extent; // relation or extent to group by
                Debug.Assert(extent != null, "Each cell must have a right extent");

                // Add the childNode as a child of the FOJ tree for "extent"
                extentMap.Add(extent, childNode);
            }
            // Now go through the extent map and create FOJ nodes for each extent
            // Place the nodes for that extent in the newly-created FOJ subtree
            // Also add the op node for every node as a child of the final result
            OpCellTreeNode result = new OpCellTreeNode(m_viewgenContext, CellTreeOpType.FOJ);

            foreach (EntitySetBase extent in extentMap.Keys)
            {
                OpCellTreeNode extentFojNode = new OpCellTreeNode(m_viewgenContext, CellTreeOpType.FOJ);
                foreach (LeafCellTreeNode childNode in extentMap.ListForKey(extent))
                {
                    extentFojNode.Add(childNode);
                }
                result.Add(extentFojNode);
            }
            // We call Flatten to remove any unnecessary nestings
            // where an OpNode has only 1 child.
            return result.Flatten();
        }

        // requires: cellTreeNode has a tree such that all its intermediate nodes
        // are FOJ nodes only
        // effects: Converts the tree rooted at rootNode (recursively) in
        // following way and returns a new rootNode -- it partitions
        // rootNode's children such that no two different partitions have
        // any overlapping constants. These partitions are connected by Union
        // nodes (since there is no overlapping).
        // Note: Method may modify rootNode's contents and children
        private CellTreeNode IsolateUnions(CellTreeNode rootNode)
        {
            if (rootNode.Children.Count <= 1)
            {
                // No partitioning of children needs to be done
                return rootNode;
            }

            Debug.Assert(rootNode.OpType == CellTreeOpType.FOJ, "So far, we have FOJs only");

            // Recursively, transform the subtrees rooted at cellTreeNode's children
            for (int i = 0; i < rootNode.Children.Count; i++)
            {
                // Method modifies input as well
                rootNode.Children[i] = IsolateUnions(rootNode.Children[i]);
            }

            // Different children groups are connected by a Union
            // node -- the secltion domain of one group is disjoint from
            // another group's selection domain, i.e., group A1 contributes
            // tuples to the extent which are disjoint from the tuples by
            // A2. So we can connect these groups by union alls.
            // Inside each group, we continue to connect children of the same
            // group using FOJ
            OpCellTreeNode unionNode = new OpCellTreeNode(m_viewgenContext, CellTreeOpType.Union);

            // childrenSet keeps track of the children that need to be procesed/partitioned
            ModifiableIteratorCollection<CellTreeNode> childrenSet = new ModifiableIteratorCollection<CellTreeNode>(rootNode.Children);

            while (false == childrenSet.IsEmpty)
            {
                // Start a new group
                // Make an FOJ node to connect children of the same group
                OpCellTreeNode fojNode = new OpCellTreeNode(m_viewgenContext, CellTreeOpType.FOJ);

                // Add one of the root's children as a child to the foj node
                CellTreeNode someChild = childrenSet.RemoveOneElement();
                fojNode.Add(someChild);

                // We now want a transitive closure of the overlap between the 
                // the children node. We keep checking each child with the
                // fojNode and add it as a child of fojNode if there is an
                // overlap. Note that when a node is added to the fojNode,
                // its constants are propagated to the fojNode -- so we do
                // get transitive closure in terms of intersection 
                foreach (CellTreeNode child in childrenSet.Elements())
                {
                    if (!IsDisjoint(fojNode, child))
                    {
                        fojNode.Add(child);
                        childrenSet.RemoveCurrentOfIterator();
                        // To ensure that we get all overlapping node, we
                        // need to restart checking all the children
                        childrenSet.ResetIterator();
                    }
                }
                // Now we have a group of children nodes rooted at
                // fojNode. Add this fojNode to the union
                unionNode.Add(fojNode);
            }

            // The union node as the root of the view
            CellTreeNode result = unionNode.Flatten();
            return result;
        }

        /// <summary>
        /// Traverse the tree and perform the following rewrites:
        ///     1. Flatten unions contained as left children of LOJs: LOJ(A, Union(B, C)) -> LOJ(A, B, C).
        ///     2. Rewrite flat LOJs into nested LOJs. The nesting is determined by FKs between right cell table PKs.
        ///        Example: if we have an LOJ(A, B, C, D) and we know there are FKs from C.PK and D.PK to B.PK,
        ///        we want to rewrite into this - LOJ(A, LOJ(B, C, D)).
        ///     3. As a special case we also look into LOJ driving node (left most child in LOJ) and if it is an IJ,
        ///        then we consider attaching LOJ children to nodes inside IJ based on the same principle as above.
        ///        Example: LOJ(IJ(A, B, C), D, E, F) -> LOJ(IJ(LOJ(A, D), B, LOJ(C, E)), F) iff D has FK to A and E has FK to C.
        ///        
        /// This normalization enables FK-based join elimination in plan compiler, so for a query such as
        /// "select e.ID from ABCDSet" we want plan compiler to produce "select a.ID from A" instead of 
        /// "select a.ID from A LOJ B LOJ C LOJ D".
        /// </summary>
        private CellTreeNode ConvertUnionsToNormalizedLOJs(CellTreeNode rootNode)
        {
            // Recursively, transform the subtrees rooted at rootNode's children.
            for (int i = 0; i < rootNode.Children.Count; i++)
            {
                // Method modifies input as well.
                rootNode.Children[i] = ConvertUnionsToNormalizedLOJs(rootNode.Children[i]);
            }

            // We rewrite only LOJs.
            if (rootNode.OpType != CellTreeOpType.LOJ || rootNode.Children.Count < 2)
            {
                return rootNode;
            }

            // Create the resulting LOJ node.
            var result = new OpCellTreeNode(m_viewgenContext, rootNode.OpType);

            // Create working collection for the LOJ children.
            var children = new List<CellTreeNode>();

            // If rootNode looks something like ((V0 IJ V1) LOJ V2 LOJ V3),
            // and it turns out that there are FK associations from V2 or V3 pointing, let's say at V0,
            // then we want to rewrite the result as (V1 IJ (V0 LOJ V2 LOJ V3)).
            // If we don't do this, then plan compiler won't have a chance to eliminate LOJ V2 LOJ V3.
            // Hence, flatten the first child or rootNode if it's IJ, but remember that its parts are driving nodes for the LOJ,
            // so that we don't accidentally nest them.
            OpCellTreeNode resultIJDriver = null;
            HashSet<CellTreeNode> resultIJDriverChildren = null;
            if (rootNode.Children[0].OpType == CellTreeOpType.IJ)
            {
                // Create empty resultIJDriver node and add it as the first child (driving) into the LOJ result.
                resultIJDriver = new OpCellTreeNode(m_viewgenContext, rootNode.Children[0].OpType);
                result.Add(resultIJDriver);

                children.AddRange(rootNode.Children[0].Children);
                resultIJDriverChildren = new HashSet<CellTreeNode>(rootNode.Children[0].Children);
            }
            else
            {
                result.Add(rootNode.Children[0]);
            }

            // Flatten unions in non-driving nodes: (V0 LOJ (V1 Union V2 Union V3)) -> (V0 LOJ V1 LOJ V2 LOJ V3) 
            foreach (var child in rootNode.Children.Skip(1))
            {
                var opNode = child as OpCellTreeNode;
                if (opNode != null && opNode.OpType == CellTreeOpType.Union)
                {
                    children.AddRange(opNode.Children);
                }
                else
                {
                    children.Add(child);
                }
            }

            // A dictionary that maps an extent to the nodes that are from that extent.
            // We want a ref comparer here.
            var extentMap = new KeyToListMap<EntitySet, LeafCellTreeNode>(EqualityComparer<EntitySet>.Default);
            // Note that we skip non-leaf nodes (non-leaf nodes don't have FKs) and attach them directly to the result.
            foreach (var child in children)
            {
                var leaf = child as LeafCellTreeNode;
                if (leaf != null)
                {
                    EntitySetBase extent = GetLeafNodeTable(leaf);
                    if (extent != null)
                    {
                        extentMap.Add((EntitySet)extent, leaf);
                    }
                }
                else
                {
                    if (resultIJDriverChildren != null && resultIJDriverChildren.Contains(child))
                    {
                        resultIJDriver.Add(child);
                    }
                    else
                    {
                        result.Add(child);
                    }
                }
            }
            
            // We only deal with simple cases - one node per extent, remove the rest from children and attach directly to result.
            var nonTrivial = extentMap.KeyValuePairs.Where(m => m.Value.Count > 1).ToArray();
            foreach (var m in nonTrivial)
            {
                extentMap.RemoveKey(m.Key);
                foreach (var n in m.Value)
                {
                    if (resultIJDriverChildren != null && resultIJDriverChildren.Contains(n))
                    {
                        resultIJDriver.Add(n);
                    }
                    else
                    {
                        result.Add(n);
                    }
                }
            }
            Debug.Assert(extentMap.KeyValuePairs.All(m => m.Value.Count == 1), "extentMap must map to single nodes only.");

            // Walk the extents in extentMap and for each extent build PK -> FK1(PK1), FK2(PK2), ... map
            // where PK is the primary key of the left extent, and FKn(PKn) is an FK of a right extent that 
            // points to the PK of the left extent and is based on the PK columns of the right extent.
            // Example:
            //           table tBaseType(Id int, c1 int), PK = (tBaseType.Id)
            //           table tDerivedType1(Id int, c2 int), PK1 = (tDerivedType1.Id), FK1 = (tDerivedType1.Id -> tBaseType.Id)
            //           table tDerivedType2(Id int, c3 int), PK2 = (tDerivedType2.Id), FK2 = (tDerivedType2.Id -> tBaseType.Id)
            // Will produce:
            //           (tBaseType) -> (tDerivedType1, tDerivedType2)
            var pkFkMap = new KeyToListMap<EntitySet, EntitySet>(EqualityComparer<EntitySet>.Default);
            // Also for each extent in extentMap, build another map (extent) -> (LOJ node).
            // It will be used to construct the nesting in the next step.
            var extentLOJs = new Dictionary<EntitySet, OpCellTreeNode>(EqualityComparer<EntitySet>.Default);
            foreach (var extentInfo in extentMap.KeyValuePairs)
            {
                var principalExtent = extentInfo.Key;
                foreach (var fkExtent in GetFKOverPKDependents(principalExtent))
                {
                    // Only track fkExtents that are in extentMap.
                    System.Collections.ObjectModel.ReadOnlyCollection<LeafCellTreeNode> nodes;
                    if (extentMap.TryGetListForKey(fkExtent, out nodes))
                    {
                        // Make sure that we are not adding resultIJDriverChildren as FK dependents - we do not want them to get nested.
                        if (resultIJDriverChildren == null || !resultIJDriverChildren.Contains(nodes.Single()))
                        {
                            pkFkMap.Add(principalExtent, fkExtent);
                        }
                    }
                }
                var extentLojNode = new OpCellTreeNode(m_viewgenContext, CellTreeOpType.LOJ);
                extentLojNode.Add(extentInfo.Value.Single());
                extentLOJs.Add(principalExtent, extentLojNode);
            }

            // Construct LOJ nesting inside extentLOJs based on the information in pkFkMap.
            // Also, track nested extents using nestedExtents.
            // Example:
            // We start with nestedExtents empty extentLOJs as such:
            //      tBaseType -> LOJ(BaseTypeNode)
            //      tDerivedType1 -> LOJ(DerivedType1Node)*
            //      tDerivedType2 -> LOJ(DerivedType2Node)**
            // Note that * and ** represent object references. So each time something is nested, 
            // we don't clone, but nest the original LOJ. When we get to processing the extent of that LOJ,
            // we might add other children to that nested LOJ.
            // As we walk pkFkMap, we end up with this:
            //      tBaseType -> LOJ(BaseTypeNode, LOJ(DerivedType1Node)*, LOJ(DerivedType2Node)**)
            //      tDerivedType1 -> LOJ(DerivedType1Node)*
            //      tDerivedType2 -> LOJ(DerivedType2Node)**
            // nestedExtens = (tDerivedType1, tDerivedType2)
            var nestedExtents = new Dictionary<EntitySet, EntitySet>(EqualityComparer<EntitySet>.Default);
            foreach (var m in pkFkMap.KeyValuePairs)
            {
                var principalExtent = m.Key;
                foreach (var fkExtent in m.Value)
                {
                    OpCellTreeNode fkExtentLOJ;
                    if (extentLOJs.TryGetValue(fkExtent, out fkExtentLOJ) &&
                        // make sure we don't nest twice and we don't create a cycle.
                        !nestedExtents.ContainsKey(fkExtent) && !CheckLOJCycle(fkExtent, principalExtent, nestedExtents))
                    {
                        extentLOJs[m.Key].Add(fkExtentLOJ);
                        nestedExtents.Add(fkExtent, principalExtent);
                    }
                }
            }

            // Now we need to grab the LOJs that have not been nested and add them to the result.
            // All LOJs that have been nested must be somewhere inside the LOJs that have not been nested,
            // so they as well end up in the result as part of the unnested ones.
            foreach (var m in extentLOJs)
            {
                if (!nestedExtents.ContainsKey(m.Key))
                {
                    // extentLOJ represents (Vx LOJ Vy LOJ(Vm LOJ Vn)) where Vx is the original node from rootNode.Children or resultIJDriverChildren.
                    var extentLOJ = m.Value;
                    if (resultIJDriverChildren != null && resultIJDriverChildren.Contains(extentLOJ.Children[0]))
                    {
                        resultIJDriver.Add(extentLOJ);
                    }
                    else
                    {
                        result.Add(extentLOJ);
                    }
                }
            }

            return result.Flatten();
        }

        private static IEnumerable<EntitySet> GetFKOverPKDependents(EntitySet principal)
        {
            foreach (var pkFkInfo in principal.ForeignKeyPrincipals)
            {
                // If principal has a related extent with FK pointing to principal and the FK is based on PK columns of the related extent,
                // then add it.
                var pkColumns = pkFkInfo.Item2.ToRole.GetEntityType().KeyMembers;
                var fkColumns = pkFkInfo.Item2.ToProperties;
                if (pkColumns.Count == fkColumns.Count)
                {
                    // Compare PK to FK columns, order is important (otherwise it's not an FK over PK).
                    int i = 0;
                    for (; i < pkColumns.Count && pkColumns[i].EdmEquals(fkColumns[i]); ++i);
                    if (i == pkColumns.Count)
                    {
                        yield return pkFkInfo.Item1.AssociationSetEnds.Where(ase => ase.Name == pkFkInfo.Item2.ToRole.Name).Single().EntitySet;
                    }
                }
            }
        }

        private static EntitySet GetLeafNodeTable(LeafCellTreeNode leaf)
        {
            return leaf.LeftCellWrapper.RightCellQuery.Extent as EntitySet;
        }

        private static bool CheckLOJCycle(EntitySet child, EntitySet parent, Dictionary<EntitySet, EntitySet> nestedExtents)
        {
            do
            {
                if (EqualityComparer<EntitySet>.Default.Equals(parent, child))
                {
                    return true;
                }
            }
            while (nestedExtents.TryGetValue(parent, out parent));
            return false;
        }

        // requires: opTypeToIsolate must be LOJ, IJ, or Union
        // effects: Given a tree rooted at rootNode, determines if there
        // are any FOJs that can be replaced by opTypeToIsolate. If so,
        // does that and a returns a new tree with the replaced operators
        // Note: Method may modify rootNode's contents and children
        internal CellTreeNode IsolateByOperator(CellTreeNode rootNode, CellTreeOpType opTypeToIsolate)
        {
            Debug.Assert(opTypeToIsolate == CellTreeOpType.IJ || opTypeToIsolate == CellTreeOpType.LOJ
                         || opTypeToIsolate == CellTreeOpType.Union,
                         "IsolateJoins can only be called for IJs, LOJs, and Unions");

            List<CellTreeNode> children = rootNode.Children;
            if (children.Count <= 1)
            {
                // No child or one child -  do nothing
                return rootNode;
            }

            // Replace the FOJs with IJs/LOJs/Unions in the children's subtrees first
            for (int i = 0; i < children.Count; i++)
            {
                // Method modifies input as well
                children[i] = IsolateByOperator(children[i], opTypeToIsolate);
            }
            // Only FOJs and LOJs can be coverted (to IJs, Unions, LOJs) --
            // so if the node is not that, we can ignore it (or if the node is already of
            // the same type that we want)
            if (rootNode.OpType != CellTreeOpType.FOJ && rootNode.OpType != CellTreeOpType.LOJ ||
                rootNode.OpType == opTypeToIsolate)
            {
                return rootNode;
            }

            // Create a new node with the same type as the input cell node type
            OpCellTreeNode newRootNode = new OpCellTreeNode(m_viewgenContext, rootNode.OpType);

            // We start a new "group" with one of the children X - we create
            // a newChildNode with type "opTypeToIsolate". Then we
            // determine if any of the remaining children should be in the
            // same group as X.

            // childrenSet keeps track of the children that need to be procesed/partitioned
            ModifiableIteratorCollection<CellTreeNode> childrenSet = new ModifiableIteratorCollection<CellTreeNode>(children);

            // Find groups with same or subsumed constants and create a join
            // or union node for them. We do this so that some of the FOJs
            // can be replaced by union and join nodes
            // 
            while (false == childrenSet.IsEmpty)
            {
                // Start a new "group" with some child  node (for the opTypeToIsolate node type)

                OpCellTreeNode groupNode = new OpCellTreeNode(m_viewgenContext, opTypeToIsolate);
                CellTreeNode someChild = childrenSet.RemoveOneElement();
                groupNode.Add(someChild);

                // Go through the remaining children and determine if their
                // constants are subsets/equal/disjoint w.r.t the joinNode
                // constants.

                foreach (CellTreeNode child in childrenSet.Elements())
                {
                    // Check if we can add the child as part of this
                    // groupNode (with opTypeToIsolate being LOJ, IJ, or Union)
                    if (TryAddChildToGroup(opTypeToIsolate, child, groupNode))
                    {
                        childrenSet.RemoveCurrentOfIterator();

                        // For LOJ, suppose that child A did not subsume B or
                        // vice-versa. But child C subsumes both. To ensure
                        // that we can get A, B, C in the same group, we
                        // reset the iterator so that when C is added in B's
                        // loop, we can reconsider A.
                        //
                        // For IJ, adding a child to groupNode does not change the range of it,
                        // so there is no need to reconsider previously skipped children.
                        //
                        // For Union, adding a child to groupNode increases the range of the groupNode,
                        // hence previously skipped (because they weren't disjoint with groupNode) children will continue 
                        // being ignored because they would still have an overlap with one of the nodes inside groupNode.

                        if (opTypeToIsolate == CellTreeOpType.LOJ)
                        {
                            childrenSet.ResetIterator();
                        }
                    }
                }
                // The new Union/LOJ/IJ node needs to be connected to the root
                newRootNode.Add(groupNode);
            }
            return newRootNode.Flatten();
        }

        // effects: Determines if the childNode can be added as a child of the
        // groupNode using te operation "opTypeToIsolate". E.g., if
        // opTypeToIsolate is inner join, we can add child to group node if
        // childNode and groupNode have the same multiconstantsets, i.e., they have
        // the same selection condition
        // Modifies groupNode to contain groupNode at the appropriate
        // position (for LOJs, the child could be added to the beginning)
        private bool TryAddChildToGroup(CellTreeOpType opTypeToIsolate, CellTreeNode childNode,
                                        OpCellTreeNode groupNode)
        {
            switch (opTypeToIsolate)
            {
                case CellTreeOpType.IJ:
                    // For Inner join, the constants of the node and
                    // the child must be the same, i.e., if the cells
                    // are producing exactly same tuples (same selection)
                    if (IsEquivalentTo(childNode, groupNode))
                    {
                        groupNode.Add(childNode);
                        return true;
                    }
                    break;

                case CellTreeOpType.LOJ:
                    // If one cell's selection condition subsumes
                    // another, we can use LOJ. We need to check for
                    // "subsumes" on both sides
                    if (IsContainedIn(childNode, groupNode))
                    {
                        groupNode.Add(childNode);
                        return true;
                    }
                    else if (IsContainedIn(groupNode, childNode))
                    {
                        // child subsumes the whole group -- add it first
                        groupNode.AddFirst(childNode);
                        return true;
                    }
                    break;

                case CellTreeOpType.Union:
                    // If the selection conditions are disjoint, we can use UNION ALL
                    // We cannot use active domain here; disjointness is guaranteed only
                    // if we check the entire selection domain
                    if (IsDisjoint(childNode, groupNode))
                    {
                        groupNode.Add(childNode);
                        return true;
                    }
                    break;
            }
            return false;
        }

        private bool IsDisjoint(CellTreeNode n1, CellTreeNode n2)
        {
            bool isQueryView = (m_viewgenContext.ViewTarget == ViewTarget.QueryView);

            bool isDisjointLeft = LeftQP.IsDisjointFrom(n1.LeftFragmentQuery, n2.LeftFragmentQuery);

            if (isDisjointLeft && m_viewgenContext.ViewTarget == ViewTarget.QueryView)
            {
                return true;
            }

            CellTreeNode n = new OpCellTreeNode(m_viewgenContext, CellTreeOpType.IJ, n1, n2);
            bool isDisjointRight = n.IsEmptyRightFragmentQuery;

            if (m_viewgenContext.ViewTarget == ViewTarget.UpdateView &&
                isDisjointLeft && !isDisjointRight)
            {

                if (ErrorPatternMatcher.FindMappingErrors(m_viewgenContext, m_domainMap, m_errorLog))
                {
                    return false;
                }


                StringBuilder builder = new StringBuilder(Strings.Viewgen_RightSideNotDisjoint(m_viewgenContext.Extent.ToString()));
                builder.AppendLine();

                //Retrieve the offending state
                FragmentQuery intersection = LeftQP.Intersect(n1.RightFragmentQuery, n2.RightFragmentQuery);
                if (LeftQP.IsSatisfiable(intersection))
                {
                    intersection.Condition.ExpensiveSimplify();
                    RewritingValidator.EntityConfigurationToUserString(intersection.Condition, builder);
                }

                //Add Error
                m_errorLog.AddEntry(new ErrorLog.Record(true, ViewGenErrorCode.DisjointConstraintViolation,
                        builder.ToString(), m_viewgenContext.AllWrappersForExtent, String.Empty));

                ExceptionHelpers.ThrowMappingException(m_errorLog, m_config);


                return false;
            }

            return (isDisjointLeft || isDisjointRight);
        }

        private bool IsContainedIn(CellTreeNode n1, CellTreeNode n2)
        {
            // Decide whether to IJ or LOJ using the domains that are filtered by the active domain
            // The net effect is that some unneeded multiconstants will be pruned away in IJ/LOJ
            // It is desirable to do so since we are only interested in the active domain
            FragmentQuery n1Active = LeftQP.Intersect(n1.LeftFragmentQuery, m_activeDomain);
            FragmentQuery n2Active = LeftQP.Intersect(n2.LeftFragmentQuery, m_activeDomain);

            bool isContainedLeft = LeftQP.IsContainedIn(n1Active, n2Active);

            if (isContainedLeft)
            {
                return true;
            }

            CellTreeNode n = new OpCellTreeNode(m_viewgenContext, CellTreeOpType.LASJ, n1, n2);
            bool isContainedRight = n.IsEmptyRightFragmentQuery;

            return isContainedRight;
        }

        private bool IsEquivalentTo(CellTreeNode n1, CellTreeNode n2)
        {
            return IsContainedIn(n1, n2) && IsContainedIn(n2, n1);
        }

        #endregion

        #region String methods
        internal override void ToCompactString(StringBuilder builder)
        {
            // We just print the slotmap for now
            m_projectedSlotMap.ToCompactString(builder);
        }
        #endregion
    }
}
