//---------------------------------------------------------------------
// <copyright file="OpCellTreeNode.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common.Utils;
using System.Data.Entity;
using System.Data.Mapping.ViewGeneration.CqlGeneration;
using System.Data.Mapping.ViewGeneration.QueryRewriting;
using System.Data.Mapping.ViewGeneration.Utils;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace System.Data.Mapping.ViewGeneration.Structures
{

    using AttributeSet = Set<MemberPath>;

    // This class represents th intermediate nodes in the tree (non-leaf nodes)
    internal class OpCellTreeNode : CellTreeNode
    {

        #region Constructors
        // effects: Creates a node with operation opType and no children
        internal OpCellTreeNode(ViewgenContext context, CellTreeOpType opType)
            : base(context)
        {
            m_opType = opType;
            m_attrs = new AttributeSet(MemberPath.EqualityComparer);
            m_children = new List<CellTreeNode>();
        }

        internal OpCellTreeNode(ViewgenContext context, CellTreeOpType opType, params CellTreeNode[] children)
            : this(context, opType, (IEnumerable<CellTreeNode>)children) { }

        // effects: Given a sequence of children node and the opType, creates
        // an OpCellTreeNode and returns it
        internal OpCellTreeNode(ViewgenContext context, CellTreeOpType opType, IEnumerable<CellTreeNode> children)
            : this(context, opType)
        {
            // Add the children one by one so that we can get the attrs etc fixed
            foreach (CellTreeNode child in children)
            {
                Add(child);
            }
        }

        #endregion

        #region Fields
        private Set<MemberPath> m_attrs; // attributes from whole subtree below
        private List<CellTreeNode> m_children;
        private CellTreeOpType m_opType;
        private FragmentQuery m_leftFragmentQuery;
        private FragmentQuery m_rightFragmentQuery;
        #endregion

        #region Properties
        // effects: See CellTreeNode.OpType
        internal override CellTreeOpType OpType { get { return m_opType; } }

        // Lazily create FragmentQuery when required
        internal override FragmentQuery LeftFragmentQuery
        {
            get
            {
                if (m_leftFragmentQuery == null)
                {
                    m_leftFragmentQuery = GenerateFragmentQuery(Children, true /*isLeft*/, ViewgenContext, OpType);
                }
                return m_leftFragmentQuery;
            }
        }

        internal override FragmentQuery RightFragmentQuery
        {
            get
            {
                if (m_rightFragmentQuery == null)
                {
                    m_rightFragmentQuery = GenerateFragmentQuery(Children, false /*isLeft*/, ViewgenContext, OpType);
                }
                return m_rightFragmentQuery;
            }
        }

        // effects: See CellTreeNode.RightDomainMap
        internal override MemberDomainMap RightDomainMap
        {
            get
            {
                // Get the information from one of the children
                Debug.Assert(m_children[0].RightDomainMap != null, "EdmMember domain map missing");
                return m_children[0].RightDomainMap;
            }
        }

        // effects: See CellTreeNode.Attributes
        internal override Set<MemberPath> Attributes { get { return m_attrs; } }

        // effects: See CellTreeNode.Children
        internal override List<CellTreeNode> Children { get { return m_children; } }

        internal override int NumProjectedSlots
        {
            get
            {
                // All children have the same number of slots
                Debug.Assert(m_children.Count > 1, "No children for op node?");
                return m_children[0].NumProjectedSlots;
            }
        }

        internal override int NumBoolSlots
        {
            get
            {
                Debug.Assert(m_children.Count > 1, "No children for op node?");
                return m_children[0].NumBoolSlots;
            }
        }
        #endregion

        #region Methods
        internal override TOutput Accept<TInput, TOutput>(SimpleCellTreeVisitor<TInput, TOutput> visitor, TInput param)
        {
            return visitor.VisitOpNode(this, param);
        }

        internal override TOutput Accept<TInput, TOutput>(CellTreeVisitor<TInput, TOutput> visitor, TInput param)
        {
            switch (OpType)
            {
                case CellTreeOpType.IJ:
                    return visitor.VisitInnerJoin(this, param);
                case CellTreeOpType.LOJ:
                    return visitor.VisitLeftOuterJoin(this, param);
                case CellTreeOpType.Union:
                    return visitor.VisitUnion(this, param);
                case CellTreeOpType.FOJ:
                    return visitor.VisitFullOuterJoin(this, param);
                case CellTreeOpType.LASJ:
                    return visitor.VisitLeftAntiSemiJoin(this, param);
                default:
                    Debug.Fail("Unexpected optype: " + OpType);
                    // To satsfy the compiler
                    return visitor.VisitInnerJoin(this, param);
            }
        }

        // effects: Add child to the end of the current children list
        // while ensuring the constants and attributes of the child are
        // propagated into this (i.e., unioned)
        internal void Add(CellTreeNode child)
        {
            Insert(m_children.Count, child);
        }

        // effects: Add child at the beginning of the current children list
        // while ensuring the constants and attributes of the child are
        // propagated into this (i.e., unioned)
        internal void AddFirst(CellTreeNode child)
        {
            Insert(0, child);
        }

        // effects: Inserts child at "index" while ensuring the constants
        // and attributes of the child are propagated into this
        private void Insert(int index, CellTreeNode child)
        {
            m_attrs.Unite(child.Attributes);
            m_children.Insert(index, child);
            // reset fragmentQuery so it's recomputed when property FragmentQuery is accessed
            m_leftFragmentQuery = null;
            m_rightFragmentQuery = null;
        }

        // effects: Given the required slots by the parent,
        // generates a CqlBlock tree for the tree rooted below node
        internal override CqlBlock ToCqlBlock(bool[] requiredSlots, CqlIdentifiers identifiers, ref int blockAliasNum,
            ref List<WithRelationship> withRelationships)
        {
            // Dispatch depending on whether we have a union node or join node
            CqlBlock result;
            if (OpType == CellTreeOpType.Union)
            {
                result = UnionToCqlBlock(requiredSlots, identifiers, ref blockAliasNum, ref withRelationships);
            }
            else
            {
                result = JoinToCqlBlock(requiredSlots, identifiers, ref blockAliasNum, ref withRelationships);
            }
            return result;
        }

        internal override bool IsProjectedSlot(int slot)
        {
            // If any childtree projects it, return true
            foreach (CellTreeNode childNode in Children)
            {
                if (childNode.IsProjectedSlot(slot))
                {
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region Union CqlBLock Methods
        // requires: node corresponds to a Union node
        // effects: Given a union node and the slots required by the parent,
        // generates a CqlBlock for the subtree rooted at node
        private CqlBlock UnionToCqlBlock(bool[] requiredSlots, CqlIdentifiers identifiers, ref int blockAliasNum, ref List<WithRelationship> withRelationships)
        {
            Debug.Assert(OpType == CellTreeOpType.Union);

            List<CqlBlock> children = new List<CqlBlock>();
            List<Tuple<CqlBlock, SlotInfo>> additionalChildSlots = new List<Tuple<CqlBlock, SlotInfo>>();

            int totalSlots = requiredSlots.Length;
            foreach (CellTreeNode child in Children)
            {
                // Unlike Join, we pass the requiredSlots from the parent as the requirement.
                bool[] childProjectedSlots = child.GetProjectedSlots();
                AndWith(childProjectedSlots, requiredSlots);
                CqlBlock childBlock = child.ToCqlBlock(childProjectedSlots, identifiers, ref blockAliasNum, ref withRelationships);
                for (int qualifiedSlotNumber = childProjectedSlots.Length; qualifiedSlotNumber < childBlock.Slots.Count; qualifiedSlotNumber++)
                {
                    additionalChildSlots.Add(Tuple.Create(childBlock, childBlock.Slots[qualifiedSlotNumber]));
                }

                // if required, but not projected, add NULL
                SlotInfo[] paddedSlotInfo = new SlotInfo[childBlock.Slots.Count];
                for (int slotNum = 0; slotNum < totalSlots; slotNum++)
                {
                    if (requiredSlots[slotNum] && !childProjectedSlots[slotNum])
                    {
                        if (IsBoolSlot(slotNum))
                        {
                            paddedSlotInfo[slotNum] = new SlotInfo(true /* is required */, true /* is projected */,
                                                            new BooleanProjectedSlot(BoolExpression.False, identifiers, SlotToBoolIndex(slotNum)), null /* member path*/);
                        }
                        else
                        {
                            // NULL as projected slot
                            MemberPath memberPath = childBlock.MemberPath(slotNum);
                            paddedSlotInfo[slotNum] = new SlotInfo(true /* is required */, true /* is projected */,
                                                             new ConstantProjectedSlot(Constant.Null, memberPath), memberPath);
                        }
                    }
                    else
                    {
                        paddedSlotInfo[slotNum] = childBlock.Slots[slotNum];
                    }
                }
                childBlock.Slots = new ReadOnlyCollection<SlotInfo>(paddedSlotInfo);
                children.Add(childBlock);
                Debug.Assert(totalSlots == child.NumBoolSlots + child.NumProjectedSlots,
                    "Number of required slots is different from what each node in the tree has?");
            }

            // We need to add the slots added by each child uniformly for others (as nulls) since this is a union operation.
            if (additionalChildSlots.Count != 0)
            {
                foreach (CqlBlock childBlock in children)
                {
                    SlotInfo[] childSlots = new SlotInfo[totalSlots + additionalChildSlots.Count];
                    childBlock.Slots.CopyTo(childSlots, 0);
                    int index = totalSlots;
                    foreach (var addtionalChildSlotInfo in additionalChildSlots)
                    {
                        var slotInfo = addtionalChildSlotInfo.Item2;
                        if (addtionalChildSlotInfo.Item1.Equals(childBlock))
                        {
                            childSlots[index] = new SlotInfo(true /* is required */, true /* is projected */, slotInfo.SlotValue, slotInfo.OutputMember);
                        }
                        else
                        {
                            childSlots[index] = new SlotInfo(true /* is required */, true /* is projected */, 
                                new ConstantProjectedSlot(Constant.Null, slotInfo.OutputMember), slotInfo.OutputMember);

                        }
                        //move on to the next slot added by children.
                        index++;
                    }
                    childBlock.Slots = new ReadOnlyCollection<SlotInfo>(childSlots);
                }
            }

            // Create the slotInfos and then Union CqlBlock
            SlotInfo[] slotInfos = new SlotInfo[totalSlots + additionalChildSlots.Count];

            // We pick the slot references from the first child, just as convention
            // In a union, values come from both sides
            CqlBlock firstChild = children[0];

            for (int slotNum = 0; slotNum < totalSlots; slotNum++)
            {
                SlotInfo slotInfo = firstChild.Slots[slotNum];
                // A required slot is somehow projected by a child in Union, so set isProjected to be the same as isRequired.
                bool isRequired = requiredSlots[slotNum];
                slotInfos[slotNum] = new SlotInfo(isRequired, isRequired, slotInfo.SlotValue, slotInfo.OutputMember);
            }

            for (int slotNum = totalSlots; slotNum < totalSlots + additionalChildSlots.Count; slotNum++)
            {
                var aslot = firstChild.Slots[slotNum];
                slotInfos[slotNum] = new SlotInfo(true, true, aslot.SlotValue, aslot.OutputMember);
            }

            CqlBlock block = new UnionCqlBlock(slotInfos, children, identifiers, ++blockAliasNum);
            return block;
        }
        private static void AndWith(bool[] boolArray, bool[] another)
        {
            Debug.Assert(boolArray.Length == another.Length);
            for (int i = 0; i < boolArray.Length; i++)
            {
                boolArray[i] &= another[i];
            }
        }
        #endregion

        #region Join CqlBLock Methods
        // requires: node corresponds to an IJ, LOJ, FOJ node
        // effects: Given a union node and the slots required by the parent,
        // generates a CqlBlock for the subtree rooted at node
        private CqlBlock JoinToCqlBlock(bool[] requiredSlots, CqlIdentifiers identifiers, ref int blockAliasNum, ref List<WithRelationship> withRelationships)
        {
            int totalSlots = requiredSlots.Length;

            Debug.Assert(OpType == CellTreeOpType.IJ ||
                         OpType == CellTreeOpType.LOJ ||
                         OpType == CellTreeOpType.FOJ, "Only these join operations handled");

            List<CqlBlock> children = new List<CqlBlock>();
            List<Tuple<QualifiedSlot, MemberPath>> additionalChildSlots = new List<Tuple<QualifiedSlot,MemberPath>>();

            // First get the children nodes (FROM part)
            foreach (CellTreeNode child in Children)
            {
                // Determine the slots that are projected by this child.
                // These are the required slots as well - unlike Union, we do not need the child to project any extra nulls.
                bool[] childProjectedSlots = child.GetProjectedSlots();
                AndWith(childProjectedSlots, requiredSlots);
                CqlBlock childBlock = child.ToCqlBlock(childProjectedSlots, identifiers, ref blockAliasNum, ref withRelationships);
                children.Add(childBlock);
                for (int qualifiedSlotNumber = childProjectedSlots.Length; qualifiedSlotNumber < childBlock.Slots.Count; qualifiedSlotNumber++)
                {
                    additionalChildSlots.Add(Tuple.Create(childBlock.QualifySlotWithBlockAlias(qualifiedSlotNumber), childBlock.MemberPath(qualifiedSlotNumber)));
                }
                Debug.Assert(totalSlots == child.NumBoolSlots + child.NumProjectedSlots,
                    "Number of required slots is different from what each node in the tree has?");
            }

            // Now get the slots that are projected out by this node (SELECT part)
            SlotInfo[] slotInfos = new SlotInfo[totalSlots + additionalChildSlots.Count];
            for (int slotNum = 0; slotNum < totalSlots; slotNum++)
            {
                // Note: this call could create a CaseStatementSlot (i.e., slotInfo.SlotValue is CaseStatementSlot)
                // which uses "from" booleans that need to be projected by children
                SlotInfo slotInfo = GetJoinSlotInfo(OpType, requiredSlots[slotNum], children, slotNum, identifiers);
                slotInfos[slotNum] = slotInfo;
            }

            for (int i = 0, slotNum = totalSlots; slotNum < totalSlots + additionalChildSlots.Count; slotNum++, i++)
            {
                slotInfos[slotNum] = new SlotInfo(true, true, additionalChildSlots[i].Item1, additionalChildSlots[i].Item2);
            }

            // Generate the ON conditions: For each child, generate an ON
            // clause with the 0th child on the key fields
            List<JoinCqlBlock.OnClause> onClauses = new List<JoinCqlBlock.OnClause>();

            for (int i = 1; i < children.Count; i++)
            {
                CqlBlock child = children[i];
                JoinCqlBlock.OnClause onClause = new JoinCqlBlock.OnClause();
                foreach (int keySlotNum in this.KeySlots)
                {
                    if (ViewgenContext.Config.IsValidationEnabled)
                    {
                        Debug.Assert(children[0].IsProjected(keySlotNum), "Key is not in 0th child");
                        Debug.Assert(child.IsProjected(keySlotNum), "Key is not in child");
                    }
                    else
                    {
                        if (!child.IsProjected(keySlotNum) || !children[0].IsProjected(keySlotNum))
                        {
                            ErrorLog errorLog = new ErrorLog();
                            errorLog.AddEntry(new ErrorLog.Record(true, ViewGenErrorCode.NoJoinKeyOrFKProvidedInMapping,
                                    Strings.Viewgen_NoJoinKeyOrFK, ViewgenContext.AllWrappersForExtent, String.Empty));
                            ExceptionHelpers.ThrowMappingException(errorLog, ViewgenContext.Config);
                        }
                    }
                    var firstSlot = children[0].QualifySlotWithBlockAlias(keySlotNum);
                    var secondSlot = child.QualifySlotWithBlockAlias(keySlotNum);
                    var outputMember = slotInfos[keySlotNum].OutputMember;
                    onClause.Add(firstSlot, outputMember, secondSlot, outputMember);
                }
                onClauses.Add(onClause);
            }

            CqlBlock result = new JoinCqlBlock(OpType, slotInfos, children, onClauses, identifiers, ++blockAliasNum);
            return result;
        }

        // effects: Generates a SlotInfo object for a slot of a join node. It
        // uses the type of the join operation (opType), whether the slot is
        // required by the parent or not (isRequiredSlot), the children of
        // this node (children) and the number of the slotNum
        private SlotInfo GetJoinSlotInfo(CellTreeOpType opType, bool isRequiredSlot,
                                         List<CqlBlock> children, int slotNum, CqlIdentifiers identifiers)
        {
            if (false == isRequiredSlot)
            {
                // The slot will not be used. So we can set the projected slot to be null
                SlotInfo unrequiredSlotInfo = new SlotInfo(false, false, null, GetMemberPath(slotNum));
                return unrequiredSlotInfo;
            }

            // For a required slot, determine the child who is contributing to this value
            int childDefiningSlot = -1;
            CaseStatement caseForOuterJoins = null;

            for (int childNum = 0; childNum < children.Count; childNum++)
            {
                CqlBlock child = children[childNum];
                if (false == child.IsProjected(slotNum))
                {
                    continue;
                }
                // For keys, we can pick any child block. So the first
                // one that we find is fine as well
                if (IsKeySlot(slotNum))
                {
                    childDefiningSlot = childNum;
                    break;
                }
                else if (opType == CellTreeOpType.IJ)
                {
                    // For Inner Joins, most of the time, the entries will be
                    // the same in all the children. However, in some cases,
                    // we will end up with NULL in one child and an actual
                    // value in another -- we should pick up the actual value in that case
                    childDefiningSlot = GetInnerJoinChildForSlot(children, slotNum);
                    break;
                }
                else
                {
                    // For LOJs, we generate a case statement if more than
                    // one child generates the value - until then we do not
                    // create the caseForOuterJoins object
                    if (childDefiningSlot != -1)
                    {
                        // We really need a case statement now
                        // We have the value being generated by another child
                        // We need to fetch the variable from the appropriate child
                        Debug.Assert(false == IsBoolSlot(slotNum), "Boolean slots cannot come from two children");
                        if (caseForOuterJoins == null)
                        {
                            MemberPath outputMember = GetMemberPath(slotNum);
                            caseForOuterJoins = new CaseStatement(outputMember);
                            // Add the child that we had not added in the first shot
                            AddCaseForOuterJoins(caseForOuterJoins, children[childDefiningSlot], slotNum, identifiers);
                        }
                        AddCaseForOuterJoins(caseForOuterJoins, child, slotNum, identifiers);
                    }
                    childDefiningSlot = childNum;
                }
            }

            MemberPath memberPath = GetMemberPath(slotNum);
            ProjectedSlot slot = null;

            // Generate the slot value -- case statement slot, or a qualified slot or null or false.
            // If case statement slot has nothing, treat it as null/empty.
            if (caseForOuterJoins != null && (caseForOuterJoins.Clauses.Count > 0 || caseForOuterJoins.ElseValue != null))
            {
                caseForOuterJoins.Simplify();
                slot = new CaseStatementProjectedSlot(caseForOuterJoins, null);
            }
            else if (childDefiningSlot >= 0)
            {
                slot = children[childDefiningSlot].QualifySlotWithBlockAlias(slotNum);
            }
            else
            {
                // need to produce output slot, but don't have a value
                // output NULL for fields or False for bools
                if (IsBoolSlot(slotNum))
                {
                    slot = new BooleanProjectedSlot(BoolExpression.False, identifiers, SlotToBoolIndex(slotNum));
                }
                else
                {
                    slot = new ConstantProjectedSlot(Domain.GetDefaultValueForMemberPath(memberPath, GetLeaves(), ViewgenContext.Config), memberPath);
                }
            }

            // We need to ensure that _from variables are never null since
            // view generation uses 2-valued boolean logic.
            // They can become null in outer joins. We compensate for it by
            // adding AND NOT NULL condition on boolean slots coming from outer joins.
            bool enforceNotNull = IsBoolSlot(slotNum) &&
                                  ((opType == CellTreeOpType.LOJ && childDefiningSlot > 0) ||
                                   opType == CellTreeOpType.FOJ);
            // We set isProjected to be true since we have come up with some value for it
            SlotInfo slotInfo = new SlotInfo(true, true, slot, memberPath, enforceNotNull);
            return slotInfo;
        }

        // requires: children to be a list of nodes that are children of an
        // Inner Join node. slotNum does not correspond to the key slot
        // effects: Determines the child number from which the slot should be
        // picked up. 
        private static int GetInnerJoinChildForSlot(List<CqlBlock> children, int slotNum)
        {
            // Picks the child with the non-constant slot first. If none, picks a non-null constant slot.
            // If not een that, picks any one
            int result = -1;
            for (int i = 0; i < children.Count; i++)
            {
                CqlBlock child = children[i];
                if (false == child.IsProjected(slotNum))
                {
                    continue;
                }
                ProjectedSlot slot = child.SlotValue(slotNum);
                ConstantProjectedSlot constantSlot = slot as ConstantProjectedSlot;
                MemberProjectedSlot joinSlot = slot as MemberProjectedSlot;
                if (joinSlot != null)
                { // Pick the non-constant slot
                    result = i;
                }
                else if (constantSlot != null && constantSlot.CellConstant.IsNull())
                {
                    if (result == -1)
                    { // In case, all are null
                        result = i;
                    }
                }
                else
                {
                    // Just pick anything
                    result = i;
                }
            }
            return result;
        }

        // requires: caseForOuterJoins corresponds the slot "slotNum"
        // effects: Adds a WhenThen corresponding to child to caseForOuterJoins.
        private void AddCaseForOuterJoins(CaseStatement caseForOuterJoins, CqlBlock child, int slotNum, CqlIdentifiers identifiers)
        {
            // Determine the cells that the slot comes from
            // and make an OR expression, e.g., WHEN _from0 or _from2 or ... THEN child[slotNum]

            ProjectedSlot childSlot = child.SlotValue(slotNum);
            ConstantProjectedSlot constantSlot = childSlot as ConstantProjectedSlot;
            if (constantSlot != null && constantSlot.CellConstant.IsNull())
            {
                // NULL being generated by a child - don't need to project
                return;
            }

            BoolExpression originBool = BoolExpression.False;
            for (int i = 0; i < NumBoolSlots; i++)
            {
                int boolSlotNum = BoolIndexToSlot(i);
                if (child.IsProjected(boolSlotNum))
                {
                    // OR it to the expression
                    QualifiedCellIdBoolean boolExpr = new QualifiedCellIdBoolean(child, identifiers, i);
                    originBool = BoolExpression.CreateOr(originBool, BoolExpression.CreateLiteral(boolExpr, RightDomainMap));
                }
            }
            // Qualify the slotNum with the child.CqlAlias for the THEN
            QualifiedSlot slot = child.QualifySlotWithBlockAlias(slotNum);
            caseForOuterJoins.AddWhenThen(originBool, slot);
        }

        private static FragmentQuery GenerateFragmentQuery(IEnumerable<CellTreeNode> children, bool isLeft, ViewgenContext context, CellTreeOpType OpType)
        {
            Debug.Assert(children.Any());
            FragmentQuery fragmentQuery = isLeft ? children.First().LeftFragmentQuery : children.First().RightFragmentQuery;

            FragmentQueryProcessor qp = isLeft ? context.LeftFragmentQP : context.RightFragmentQP;
            foreach (var child in children.Skip(1))
            {
                FragmentQuery nextQuery = isLeft ? child.LeftFragmentQuery : child.RightFragmentQuery;
                switch (OpType)
                {
                    case CellTreeOpType.IJ:
                        fragmentQuery = qp.Intersect(fragmentQuery, nextQuery);
                        break;
                    case CellTreeOpType.LOJ:
                        // Left outer join means keeping the domain of the leftmost child
                        break;
                    case CellTreeOpType.LASJ:
                        // not used in basic view generation but current validation calls Simplify, so add this for debugging
                        fragmentQuery = qp.Difference(fragmentQuery, nextQuery);
                        break;
                    default:
                        // All other operators (Union, FOJ) require union of the domains
                        fragmentQuery = qp.Union(fragmentQuery, nextQuery);
                        break;
                }
            }
            return fragmentQuery;
        }


        #endregion

        #region String methods
        /// <summary>
        /// Given the <paramref name="opType"/>, returns eSQL string corresponding to the op.
        /// </summary>
        internal static string OpToEsql(CellTreeOpType opType)
        {
            switch (opType)
            {
                case CellTreeOpType.FOJ:    return "FULL OUTER JOIN";
                case CellTreeOpType.IJ:     return "INNER JOIN";
                case CellTreeOpType.LOJ:    return "LEFT OUTER JOIN";
                case CellTreeOpType.Union:  return "UNION ALL";
                default:
                    Debug.Fail("Unknown operator");
                    return null;
            }
        }

        internal override void ToCompactString(StringBuilder stringBuilder)
        {
            //            Debug.Assert(m_children.Count > 1, "Tree not flattened?");
            stringBuilder.Append("(");
            for (int i = 0; i < m_children.Count; i++)
            {
                CellTreeNode child = m_children[i];
                child.ToCompactString(stringBuilder);
                if (i != m_children.Count - 1)
                {
                    StringUtil.FormatStringBuilder(stringBuilder, " {0} ", OpType);
                }
            }
            stringBuilder.Append(")");
        }
        #endregion
    }
}
