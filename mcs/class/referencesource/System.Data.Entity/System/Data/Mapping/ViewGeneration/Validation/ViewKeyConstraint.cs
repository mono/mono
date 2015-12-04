//---------------------------------------------------------------------
// <copyright file="ViewKeyConstraint.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner [....]
// @backupOwner [....]
//---------------------------------------------------------------------

namespace System.Data.Mapping.ViewGeneration.Validation
{
    using System.Collections.Generic;
    using System.Data.Common.Utils;
    using System.Data.Entity;
    using System.Data.Mapping.ViewGeneration.Structures;
    using System.Data.Mapping.ViewGeneration.Utils;
    using System.Data.Metadata.Edm;
    using System.Text;

    // Class representing a key constraint on the view cell relations
    internal class ViewKeyConstraint : KeyConstraint<ViewCellRelation, ViewCellSlot>
    {


        #region Constructor
        //  effects: Constructs a key constraint for the given relation and keyslots
        internal ViewKeyConstraint(ViewCellRelation relation, IEnumerable<ViewCellSlot> keySlots) :
            base(relation, keySlots, ProjectedSlot.EqualityComparer)
        {
        }
        #endregion

        #region Properties
        // effects: Returns the cell corresponding to this constraint
        internal Cell Cell
        {
            get { return CellRelation.Cell; }
        }
        #endregion

        #region Methods
        internal bool Implies(ViewKeyConstraint second)
        {
            if (false == Object.ReferenceEquals(CellRelation, second.CellRelation))
            {
                return false;
            }
            // Check if the slots in this key are a subset of slots in
            // second. If it is a key in this e.g., <A.pid> then <A.pid,
            // A.foo> is certainly a key as well

            if (KeySlots.IsSubsetOf(second.KeySlots))
            {
                return true;
            }

            // Now check for subsetting taking referential constraints into account
            // Check that each slot in KeySlots can be found in second.KeySlots if we take
            // slot equivalence into account

            Set<ViewCellSlot> secondKeySlots = new Set<ViewCellSlot>(second.KeySlots);

            foreach (ViewCellSlot firstSlot in KeySlots)
            {
                bool found = false; // Need to find a match for firstSlot

                foreach (ViewCellSlot secondSlot in secondKeySlots)
                {
                    if (ProjectedSlot.EqualityComparer.Equals(firstSlot.SSlot, secondSlot.SSlot))
                    {
                        // S-side is the same. Check if C-side is the same as well. If so, remove it
                        // from secondKeySlots
                        // We have to check for C-side equivalence in terms of actual equality
                        // and equivalence via ref constraints. The former is needed since the
                        // S-side key slots would typically be mapped to the same C-side slot.
                        // The latter is needed since the same S-side key slot could be mapped
                        // into two slots on the C-side that are connected via a ref constraint
                        MemberPath path1 = firstSlot.CSlot.MemberPath;
                        MemberPath path2 = secondSlot.CSlot.MemberPath;
                        if (MemberPath.EqualityComparer.Equals(path1, path2) || path1.IsEquivalentViaRefConstraint(path2))
                        {
                            secondKeySlots.Remove(secondSlot);
                            found = true;
                            break;
                        }
                    }
                }
                if (found == false)
                {
                    return false;
                }

            }

            // The subsetting holds when referential constraints are taken into account
            return true;
        }

        // effects: Given the fact that rightKeyConstraint is not implied by a
        // leftSide key constraint, return a useful error message -- some S
        // was not implied by the C key constraints
        internal static ErrorLog.Record GetErrorRecord(ViewKeyConstraint rightKeyConstraint)
        {
            List<ViewCellSlot> keySlots = new List<ViewCellSlot>(rightKeyConstraint.KeySlots);
            EntitySetBase table = keySlots[0].SSlot.MemberPath.Extent;
            EntitySetBase cSet = keySlots[0].CSlot.MemberPath.Extent;

            MemberPath tablePrefix = new MemberPath(table);
            MemberPath cSetPrefix = new MemberPath(cSet);

            ExtentKey tableKey = ExtentKey.GetPrimaryKeyForEntityType(tablePrefix, (EntityType)table.ElementType);
            ExtentKey cSetKey = null;
            if (cSet is EntitySet)
            {
                cSetKey = ExtentKey.GetPrimaryKeyForEntityType(cSetPrefix, (EntityType)cSet.ElementType);
            }
            else
            {
                cSetKey = ExtentKey.GetKeyForRelationType(cSetPrefix, (AssociationType)cSet.ElementType);
            }

            string message = Strings.ViewGen_KeyConstraint_Violation(
                                           table.Name,
                                           ViewCellSlot.SlotsToUserString(rightKeyConstraint.KeySlots, false /*isFromCside*/),
                                           tableKey.ToUserString(),
                                           cSet.Name,
                                           ViewCellSlot.SlotsToUserString(rightKeyConstraint.KeySlots, true /*isFromCside*/),
                                           cSetKey.ToUserString());

            string debugMessage = StringUtil.FormatInvariant("PROBLEM: Not implied {0}", rightKeyConstraint);
            return new ErrorLog.Record(true, ViewGenErrorCode.KeyConstraintViolation, message, rightKeyConstraint.CellRelation.Cell, debugMessage);
        }

        // effects: Given the fact that none of the rightKeyConstraint are not implied by a
        // leftSide key constraint, return a useful error message (used for
        // the Update requirement 
        internal static ErrorLog.Record GetErrorRecord(IEnumerable<ViewKeyConstraint> rightKeyConstraints)
        {
            ViewKeyConstraint rightKeyConstraint = null;
            StringBuilder keyBuilder = new StringBuilder();
            bool isFirst = true;
            foreach (ViewKeyConstraint rightConstraint in rightKeyConstraints)
            {
                string keyMsg = ViewCellSlot.SlotsToUserString(rightConstraint.KeySlots, true /*isFromCside*/);
                if (isFirst == false)
                {
                    keyBuilder.Append("; ");
                }
                isFirst = false;
                keyBuilder.Append(keyMsg);
                rightKeyConstraint = rightConstraint;
            }

            List<ViewCellSlot> keySlots = new List<ViewCellSlot>(rightKeyConstraint.KeySlots);
            EntitySetBase table = keySlots[0].SSlot.MemberPath.Extent;
            EntitySetBase cSet = keySlots[0].CSlot.MemberPath.Extent;

            MemberPath tablePrefix = new MemberPath(table);
            ExtentKey tableKey = ExtentKey.GetPrimaryKeyForEntityType(tablePrefix, (EntityType)table.ElementType);

            string message;
            if (cSet is EntitySet)
            {
                message = System.Data.Entity.Strings.ViewGen_KeyConstraint_Update_Violation_EntitySet(keyBuilder.ToString(), cSet.Name,
                                        tableKey.ToUserString(), table.Name);
            }
            else
            {
                //For a 1:* or 0..1:* association, the * side has to be mapped to the
                //key properties of the table. Fior this specific case, we give out a specific message
                //that is specific for this case.
                AssociationSet associationSet = (AssociationSet)cSet;
                AssociationEndMember endMember = Helper.GetEndThatShouldBeMappedToKey(associationSet.ElementType);
                if(endMember != null)
                {
                    message = System.Data.Entity.Strings.ViewGen_AssociationEndShouldBeMappedToKey(endMember.Name,
                                        table.Name);
                }
                else
                {
                    message = System.Data.Entity.Strings.ViewGen_KeyConstraint_Update_Violation_AssociationSet(cSet.Name,
                                        tableKey.ToUserString(), table.Name);
                }
            }

            string debugMessage = StringUtil.FormatInvariant("PROBLEM: Not implied {0}", rightKeyConstraint);
            return new ErrorLog.Record(true, ViewGenErrorCode.KeyConstraintUpdateViolation, message, rightKeyConstraint.CellRelation.Cell, debugMessage);
        }
        #endregion
    }
}
