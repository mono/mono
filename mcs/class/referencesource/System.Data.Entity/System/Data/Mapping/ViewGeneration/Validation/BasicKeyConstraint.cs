//---------------------------------------------------------------------
// <copyright file="BasicKeyConstraint.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner [....]
// @backupOwner [....]
//---------------------------------------------------------------------


using System.Data.Mapping.ViewGeneration.Structures;
using System.Collections.Generic;

namespace System.Data.Mapping.ViewGeneration.Validation
{

    using BasicSchemaConstraints = SchemaConstraints<BasicKeyConstraint>;

    // Class representing a key constraint on the basic cell relations
    internal class BasicKeyConstraint : KeyConstraint<BasicCellRelation, MemberProjectedSlot>
    {

        #region Constructor
        //  Constructs a key constraint for the given relation and keyslots
        internal BasicKeyConstraint(BasicCellRelation relation, IEnumerable<MemberProjectedSlot> keySlots)
            : base(relation, keySlots, ProjectedSlot.EqualityComparer)
        { }
        #endregion

        #region Methods
        // effects: Propagates this constraint from the basic cell relation
        // to the corresponding view cell relation and returns the new constraint
        // If all the key slots are not being projected, returns null
        internal ViewKeyConstraint Propagate()
        {
            ViewCellRelation viewCellRelation = CellRelation.ViewCellRelation;
            // If all slots appear in the projection, propagate key constraint
            List<ViewCellSlot> viewSlots = new List<ViewCellSlot>();
            foreach (MemberProjectedSlot keySlot in KeySlots)
            {
                ViewCellSlot viewCellSlot = viewCellRelation.LookupViewSlot(keySlot);
                if (viewCellSlot == null)
                {
                    // Slot is missing -- no key constraint on the view relation
                    return null;
                }
                viewSlots.Add(viewCellSlot);
            }

            // Create a key on view relation
            ViewKeyConstraint viewKeyConstraint = new ViewKeyConstraint(viewCellRelation, viewSlots);
            return viewKeyConstraint;
        }
        #endregion
    }
}
