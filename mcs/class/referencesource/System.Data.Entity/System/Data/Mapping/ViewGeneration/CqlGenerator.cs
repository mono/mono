//---------------------------------------------------------------------
// <copyright file="CQLGenerator.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System.Data.Common;
using System.Data.Common.CommandTrees;
using System.Data.Common.Utils;
using System.Data.Mapping.ViewGeneration.Structures;
using System.Collections.Generic;
using System.Data.Mapping.ViewGeneration.CqlGeneration;
using System.Text;
using System.Diagnostics;
using System.Data.Metadata.Edm;

namespace System.Data.Mapping.ViewGeneration
{
    /// <summary>
    /// This class is responsible for generation of CQL after the cell merging process has been done.
    /// </summary>
    internal sealed class CqlGenerator : InternalBase
    {
        #region Constructor
        /// <summary>
        /// Given the generated <paramref name="view"/>, the <paramref name="caseStatements"/> for the multiconstant fields,
        /// the <paramref name="projectedSlotMap"/> that maps different paths of the entityset (for which the view is being generated) to slot indexes in the view,
        /// creates an object that is capable of generating the Cql for <paramref name="view"/>.
        /// </summary>
        internal CqlGenerator(CellTreeNode view,
                              Dictionary<MemberPath,
                              CaseStatement> caseStatements,
                              CqlIdentifiers identifiers,
                              MemberProjectionIndex projectedSlotMap,
                              int numCellsInView,
                              BoolExpression topLevelWhereClause,
                              StorageMappingItemCollection mappingItemCollection)
        {
            m_view = view;
            m_caseStatements = caseStatements;
            m_projectedSlotMap = projectedSlotMap;
            m_numBools = numCellsInView; // We have that many booleans
            m_topLevelWhereClause = topLevelWhereClause;
            m_identifiers = identifiers;
            m_mappingItemCollection = mappingItemCollection;
        }
        #endregion

        #region Fields
        /// <summary>
        /// The generated view from the cells.
        /// </summary>
        private readonly CellTreeNode m_view;
        /// <summary>
        /// Case statements for the multiconstant fields.
        /// </summary>
        private readonly Dictionary<MemberPath, CaseStatement> m_caseStatements;
        /// <summary>
        /// Mapping from member paths to slot indexes.
        /// </summary>
        private MemberProjectionIndex m_projectedSlotMap;
        /// <summary>
        /// Number of booleans in the view, one per cell (from0, from1, etc...)
        /// </summary>
        private readonly int m_numBools;
        /// <summary>
        /// A counter used to generate aliases for blocks.
        /// </summary>
        private int m_currentBlockNum = 0;
        private readonly BoolExpression m_topLevelWhereClause;
        /// <summary>
        /// Identifiers used in the Cql queries.
        /// </summary>
        private readonly CqlIdentifiers m_identifiers;
        private readonly StorageMappingItemCollection m_mappingItemCollection;
        #endregion

        #region Properties
        private int TotalSlots
        {
            get { return m_projectedSlotMap.Count + m_numBools; }
        }
        #endregion

        #region CqlBlock generation methods for all node types
        /// <summary>
        /// Returns eSQL query that represents a query/update mapping view for the view information that was supplied in the constructor.
        /// </summary>
        internal string GenerateEsql()
        {
            // Generate a CqlBlock tree and then convert that to eSQL.
            CqlBlock blockTree = GenerateCqlBlockTree();

            // Create the string builder with 1K so that we don't have to
            // keep growing it
            StringBuilder builder = new StringBuilder(1024);
            blockTree.AsEsql(builder, true, 1);
            return  builder.ToString();
        }

        /// <summary>
        /// Returns Cqtl query that represents a query/update mapping view for the view information that was supplied in the constructor.
        /// </summary>
        internal DbQueryCommandTree GenerateCqt()
        {
            // Generate a CqlBlock tree and then convert that to CQT.
            CqlBlock blockTree = GenerateCqlBlockTree();

            DbExpression query = blockTree.AsCqt(true);
            Debug.Assert(query != null, "Null CQT generated for query/update view.");

            return DbQueryCommandTree.FromValidExpression(m_mappingItemCollection.Workspace, TargetPerspective.TargetPerspectiveDataSpace, query);
        }

        /// <summary>
        /// Generates a <see cref="CqlBlock"/> tree that is capable of generating the actual Cql strings.
        /// </summary>
        private CqlBlock GenerateCqlBlockTree()
        {
            // Essentially, we create a block for each CellTreeNode in the
            // tree and then we layer case statements on top of that view --
            // one case statement for each multiconstant entry

            // Dertmine the slots that are projected by the whole tree. Tell
            // the children that they need to produce those slots somehow --
            // if they don't have it, they can produce null
            bool[] requiredSlots = GetRequiredSlots();
            Debug.Assert(requiredSlots.Length == TotalSlots, "Wrong number of requiredSlots");

            List<WithRelationship> withRelationships = new List<WithRelationship>();
            CqlBlock viewBlock = m_view.ToCqlBlock(requiredSlots, m_identifiers, ref m_currentBlockNum, ref withRelationships);

            // Handle case statements for multiconstant entries
            // Right now, we have a simplication step that removes one of the
            // entries and adds ELSE instead
            foreach (CaseStatement statement in m_caseStatements.Values)
            {
                statement.Simplify();
            }

            // Generate the case statements and get the top level block which
            // must correspond to the entity set
            CqlBlock finalViewBlock = ConstructCaseBlocks(viewBlock, withRelationships);
            return finalViewBlock;
        }

        private bool[] GetRequiredSlots()
        {
            bool[] requiredSlots = new bool[TotalSlots];
            // union all slots that are required in case statements
            foreach (CaseStatement caseStatement in m_caseStatements.Values)
            {
                GetRequiredSlotsForCaseMember(caseStatement.MemberPath, requiredSlots);
            }

            // For now, make sure that all booleans are required
            // Reason: OUTER JOINs may introduce an extra CASE statement (in OpCellTreeNode.cs/GetJoinSlotInfo)
            // if a member is projected in both inputs to the join.
            // This case statement may use boolean variables that may not be marked as "required"
            // The problem is that this decision is made _after_ CqlBlocks for children get produced (in OpCellTreeNode.cs/JoinToCqlBlock)
            for (int i = TotalSlots - m_numBools; i < TotalSlots; i++)
            {
                requiredSlots[i] = true;
            }
            // Because of the above we don't need to harvest used booleans from the top-level WHERE clause
            // m_topLevelWhereClause.GetRequiredSlots(m_projectedSlotMap, requiredSlots);

            // Do we require the case statement member slot be produced by the inner queries?
            foreach (CaseStatement caseStatement in m_caseStatements.Values)
            {
                bool notNeeded = !caseStatement.MemberPath.IsPartOfKey && // keys are required in inner queries for joins conditions
                                 !caseStatement.DependsOnMemberValue; // if case statement returns its slot value as one of the options, then we need to produce it
                if (notNeeded)
                {
                    requiredSlots[m_projectedSlotMap.IndexOf(caseStatement.MemberPath)] = false;
                }
            }
            return requiredSlots;
        }
        #endregion

        #region Multiconstant CaseStatement methods
        /// <summary>
        /// Given the <paramref name="viewBlock"/> tree, generates the case statement blocks on top of it (using <see cref="m_caseStatements"/>) and returns the resulting tree.
        /// One block per case statement is generated. Generated blocks are nested, with the <paramref name="viewBlock"/> is the innermost input.
        /// </summary>
        private CqlBlock ConstructCaseBlocks(CqlBlock viewBlock, IEnumerable<WithRelationship> withRelationships)
        {
            // Get the 0th slot only, i.e., the extent
            bool[] topSlots = new bool[TotalSlots];
            topSlots[0] = true;

            // all booleans in the top-level WHERE clause are required and get bubbled up
            // this makes some _fromX booleans be marked as 'required by parent'
            m_topLevelWhereClause.GetRequiredSlots(m_projectedSlotMap, topSlots);
            CqlBlock result = ConstructCaseBlocks(viewBlock, 0, topSlots, withRelationships);
            return result;
        }

        /// <summary>
        /// Given the <paramref name="viewBlock"/> tree generated by the cell merging process and the <paramref name="parentRequiredSlots"/>,
        /// generates the block tree for the case statement at or past the startSlotNum, i.e., only for case statements that are beyond startSlotNum.
        /// </summary>
        private CqlBlock ConstructCaseBlocks(CqlBlock viewBlock, int startSlotNum, bool[] parentRequiredSlots, IEnumerable<WithRelationship> withRelationships)
        {
            int numMembers = m_projectedSlotMap.Count;
            // Find the next slot for which we have a case statement, i.e.,
            // which was in the multiconstants
            int foundSlot = FindNextCaseStatementSlot(startSlotNum, parentRequiredSlots, numMembers);

            if (foundSlot == -1)
            {
                // We have bottomed out - no more slots to generate cases for
                // Just get the base view block
                return viewBlock;
            }

            // Compute the requiredSlots for this member, i.e., what slots are needed to produce this member.
            MemberPath thisMember = m_projectedSlotMap[foundSlot];
            bool[] thisRequiredSlots = new bool[TotalSlots];
            GetRequiredSlotsForCaseMember(thisMember, thisRequiredSlots);
            Debug.Assert(thisRequiredSlots.Length == parentRequiredSlots.Length &&
                         thisRequiredSlots.Length == TotalSlots,
                         "Number of slots in array should not vary across blocks");

            // Merge parent's requirements with this requirements
            for (int i = 0; i < TotalSlots; i++)
            {
                // We do ask the children to generate the slot that we are
                // producing if it is available
                if (parentRequiredSlots[i])
                {
                    thisRequiredSlots[i] = true;
                }
            }

            // If current case statement depends on its slot value, then make sure the value is produced by the child block.
            CaseStatement thisCaseStatement = m_caseStatements[thisMember];
            thisRequiredSlots[foundSlot] = thisCaseStatement.DependsOnMemberValue;

            // Recursively, determine the block tree for slots beyond foundSlot.
            CqlBlock childBlock = ConstructCaseBlocks(viewBlock, foundSlot + 1, thisRequiredSlots, null);

            // For each slot, create a SlotInfo object
            SlotInfo[] slotInfos = CreateSlotInfosForCaseStatement(parentRequiredSlots, foundSlot, childBlock, thisCaseStatement, withRelationships);
            m_currentBlockNum++;

            // We have a where clause only at the top level
            BoolExpression whereClause = startSlotNum == 0 ? m_topLevelWhereClause : BoolExpression.True;
            if (startSlotNum == 0)
            {
                // only slot #0 is required by parent; reset all 'required by parent' booleans introduced above
                for (int i = 1; i < slotInfos.Length; i++)
                {
                    slotInfos[i].ResetIsRequiredByParent();
                }
            }

            CaseCqlBlock result = new CaseCqlBlock(slotInfos, foundSlot, childBlock, whereClause, m_identifiers, m_currentBlockNum);
            return result;
        }

        /// <summary>
        /// Given the slot (<paramref name="foundSlot"/>) and its corresponding case statement (<paramref name="thisCaseStatement"/>),
        /// generates the slotinfos for the cql block producing the case statement.
        /// </summary>
        private SlotInfo[] CreateSlotInfosForCaseStatement(bool[] parentRequiredSlots, 
                                                           int foundSlot,
                                                           CqlBlock childBlock, 
                                                           CaseStatement thisCaseStatement,
                                                           IEnumerable<WithRelationship> withRelationships)
        {
            int numSlotsAddedByChildBlock = childBlock.Slots.Count - TotalSlots;
            SlotInfo[] slotInfos = new SlotInfo[TotalSlots + numSlotsAddedByChildBlock];
            for (int slotNum = 0; slotNum < TotalSlots; slotNum++)
            {
                bool isProjected = childBlock.IsProjected(slotNum);
                bool isRequiredByParent = parentRequiredSlots[slotNum];
                ProjectedSlot slot = childBlock.SlotValue(slotNum);
                MemberPath outputMember = GetOutputMemberPath(slotNum);
                if (slotNum == foundSlot)
                {
                    // We need a case statement instead for this slot that we
                    // are handling right now
                    Debug.Assert(isRequiredByParent, "Case result not needed by parent");

                    // Get a case statement with all slots replaced by aliases slots
                    CaseStatement newCaseStatement = thisCaseStatement.DeepQualify(childBlock);
                    slot = new CaseStatementProjectedSlot(newCaseStatement, withRelationships);
                    isProjected = true; // We are projecting this slot now
                }
                else if (isProjected && isRequiredByParent)
                {
                    // We only alias something that is needed and is being projected by the child.
                    // It is a qualified slot into the child block.
                    slot = childBlock.QualifySlotWithBlockAlias(slotNum);
                }
                // For slots, if it is not required by the parent, we want to
                // set the isRequiredByParent for this slot to be
                // false. Furthermore, we do not want to introduce any "NULL
                // AS something" at this stage for slots not being
                // projected. So if the child does not project that slot, we
                // declare it as not being required by the parent (if such a
                // NULL was needed, it would have been pushed all the way
                // down to a non-case block.
                // Essentially, from a Case statement's parent perspective,
                // it is saying "If you can produce a slot either by yourself
                // or your children, please do. Otherwise, do not concoct anything"
                SlotInfo slotInfo = new SlotInfo(isRequiredByParent && isProjected, isProjected, slot, outputMember);
                slotInfos[slotNum] = slotInfo;
            }
            for (int i = TotalSlots; i < TotalSlots + numSlotsAddedByChildBlock; i++)
            {
                QualifiedSlot childAddedSlot = childBlock.QualifySlotWithBlockAlias(i);
                slotInfos[i] = new SlotInfo(true, true, childAddedSlot, childBlock.MemberPath(i));
            }
            return slotInfos;
        }

        /// <summary>
        /// Returns the next slot starting at <paramref name="startSlotNum"/> that is present in the <see cref="m_caseStatements"/>.
        /// </summary>
        private int FindNextCaseStatementSlot(int startSlotNum, bool[] parentRequiredSlots, int numMembers)
        {
            int foundSlot = -1;
            // Simply go through the slots and check the m_caseStatements map
            for (int slotNum = startSlotNum; slotNum < numMembers; slotNum++)
            {
                MemberPath member = m_projectedSlotMap[slotNum];
                if (parentRequiredSlots[slotNum] && m_caseStatements.ContainsKey(member))
                {
                    foundSlot = slotNum;
                    break;
                }
            }
            return foundSlot;
        }

        /// <summary>
        /// Returns an array of size <see cref="TotalSlots"/> which indicates the slots that are needed to constuct value at <paramref name="caseMemberPath"/>,
        /// e.g., CPerson may need pid and name (say slots 2 and 5 - then bools[2] and bools[5] will be true.
        /// </summary>
        /// <param name="caseMemberPath">must be part of <see cref="m_caseStatements"/></param>
        private void GetRequiredSlotsForCaseMember(MemberPath caseMemberPath, bool[] requiredSlots)
        {
            Debug.Assert(true == m_caseStatements.ContainsKey(caseMemberPath), "Constructing case for regular field?");
            Debug.Assert(requiredSlots.Length == TotalSlots, "Invalid array size for populating required slots");

            CaseStatement statement = m_caseStatements[caseMemberPath];

            // Find the required slots from the when then clause conditions
            // and values
            bool requireThisSlot = false;
            foreach (CaseStatement.WhenThen clause in statement.Clauses)
            {
                clause.Condition.GetRequiredSlots(m_projectedSlotMap, requiredSlots);
                ProjectedSlot slot = clause.Value;
                if (!(slot is ConstantProjectedSlot))
                {
                    // If this slot is a scalar and a non-constant, 
                    // we need the lower down blocks to generate it for us
                    requireThisSlot = true;
                }
            }

            EdmType edmType = caseMemberPath.EdmType;
            if (Helper.IsEntityType(edmType) || Helper.IsComplexType(edmType))
            {
                foreach (EdmType instantiatedType in statement.InstantiatedTypes)
                {
                    foreach (EdmMember childMember in Helper.GetAllStructuralMembers(instantiatedType) )
                    {
                        int slotNum = GetSlotIndex(caseMemberPath, childMember);
                        requiredSlots[slotNum] = true;
                    }
                }
            }
            else if (caseMemberPath.IsScalarType())
            {
                // A scalar does not need anything per se to be constructed
                // unless it is referring to a field in the tree below, i.e., the THEN
                // slot is not a constant slot
                if (requireThisSlot)
                {
                    int caseMemberSlotNum = m_projectedSlotMap.IndexOf(caseMemberPath);
                    requiredSlots[caseMemberSlotNum] = true;
                }
            }
            else if (Helper.IsAssociationType(edmType))
            {
                // For an association, get the indices of the ends, e.g.,
                // CProduct and CCategory in CProductCategory1
                // Need just it's ends
                AssociationSet associationSet = (AssociationSet)caseMemberPath.Extent;
                AssociationType associationType = associationSet.ElementType;
                foreach (AssociationEndMember endMember in associationType.AssociationEndMembers)
                {
                    int slotNum = GetSlotIndex(caseMemberPath, endMember);
                    requiredSlots[slotNum] = true;
                }
            }
            else
            {
                // For a reference, all we need are the keys
                RefType refType = edmType as RefType;
                Debug.Assert(refType != null, "What other non scalars do we have? Relation end must be a reference type");

                EntityTypeBase refElementType = refType.ElementType;
                // Go through all the members of elementType and get the key properties

                EntitySet entitySet = MetadataHelper.GetEntitySetAtEnd((AssociationSet)caseMemberPath.Extent,
                                                                       (AssociationEndMember)caseMemberPath.LeafEdmMember);
                foreach (EdmMember entityMember in refElementType.KeyMembers)
                {
                    int slotNum = GetSlotIndex(caseMemberPath, entityMember);
                    requiredSlots[slotNum] = true;
                }
            }
        }
        #endregion

        #region Helper methods
        /// <summary>
        /// Given the <paramref name="slotNum"/>, returns the output member path that this slot contributes/corresponds to in the extent view.
        /// If the slot corresponds to one of the boolean variables, returns null.
        /// </summary>
        private MemberPath GetOutputMemberPath(int slotNum)
        {
            return m_projectedSlotMap.GetMemberPath(slotNum, TotalSlots - m_projectedSlotMap.Count);
        }

        /// <summary>
        /// Returns the slot index for the following member path: <paramref name="member"/>.<paramref name="child"/>, e.g., CPerson1.pid
        /// </summary>
        private int GetSlotIndex(MemberPath member, EdmMember child)
        {
            MemberPath fullMember = new MemberPath(member, child);
            int index = m_projectedSlotMap.IndexOf(fullMember);
            Debug.Assert(index != -1, "Couldn't locate " + fullMember.ToString() + " in m_projectedSlotMap");
            return index;
        }
        #endregion

        #region String methods
        internal override void ToCompactString(StringBuilder builder)
        {
            builder.Append("View: ");
            m_view.ToCompactString(builder);
            builder.Append("ProjectedSlotMap: ");
            m_projectedSlotMap.ToCompactString(builder);
            builder.Append("Case statements: ");
            foreach (MemberPath member in m_caseStatements.Keys)
            {
                CaseStatement statement = m_caseStatements[member];
                statement.ToCompactString(builder);
                builder.AppendLine();
            }
        }
        #endregion
    }
}
