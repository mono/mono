//---------------------------------------------------------------------
// <copyright file="ViewCellRelation.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System.Collections.Generic;
using System.Data.Mapping.ViewGeneration.Structures;
using System.Text;
using System.Data.Common.Utils;


namespace System.Data.Mapping.ViewGeneration.Validation
{
    /// <summary>
    /// Represents a relation signature that lists all projected
    /// slots of two cell queries in a cell after projection. So if
    /// SPerson1.Disc is present in the cellquery (and part of the where
    /// clause) but not in the projected slots, it is missing from a ViewCellRelation
    /// </summary>
    internal class ViewCellRelation : CellRelation
    {

        #region Constructor
        // effects: Creates a view cell relation for "cell" with the
        // projected slots given by slots -- cellNumber is the number of the
        // cell for debugging purposes
        // Also creates the BasicCellRelations for the left and right cell queries
        internal ViewCellRelation(Cell cell, List<ViewCellSlot> slots, int cellNumber)
            : base(cellNumber)
        {
            m_cell = cell;
            m_slots = slots;
            // We create the basiccellrelations  passing this to it so that we have
            // a reference from the basiccellrelations to this
            m_cell.CQuery.CreateBasicCellRelation(this);
            m_cell.SQuery.CreateBasicCellRelation(this);
        }
        #endregion

        #region Fields
        private Cell m_cell; // The cell for which this relation exists
        private List<ViewCellSlot> m_slots; // Slots projected from both cell queries
        #endregion

        #region Properties
        internal Cell Cell
        {
            get { return m_cell; }
        }
        #endregion


        #region Methods
        // requires: slot corresponds to a slot in the corresponding
        // BasicCellRelation
        // effects: Given a slot in the corresponding basicCellRelation,
        // looks up the slot in this viewcellrelation and returns it. Returns
        // null if it does not find the slot in the left or right side of the viewrelation
        internal ViewCellSlot LookupViewSlot(MemberProjectedSlot slot)
        {
            // CHANGE_Microsoft_IMPROVE: We could have a dictionary to speed this up
            foreach (ViewCellSlot viewSlot in m_slots)
            {
                // If the left or right slots are equal, return the viewSlot
                if (ProjectedSlot.EqualityComparer.Equals(slot, viewSlot.CSlot) ||
                    ProjectedSlot.EqualityComparer.Equals(slot, viewSlot.SSlot))
                {
                    return viewSlot;
                }
            }
            return null;
        }

        protected override int GetHash()
        {
            // Note: Using CLR-Hashcode
            return m_cell.GetHashCode();
            // We need not hash the slots, etc - cell should give us enough
            // differentiation and land the relation into the same bucket
        }

        internal override void ToCompactString(StringBuilder builder)
        {
            builder.Append("ViewRel[");
            m_cell.ToCompactString(builder);
            // StringUtil.ToSeparatedStringSorted(builder, m_slots, ", ");
            builder.Append(']');
        }
        #endregion
    }
}
