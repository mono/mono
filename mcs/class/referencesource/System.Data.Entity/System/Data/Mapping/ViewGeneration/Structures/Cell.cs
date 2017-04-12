//---------------------------------------------------------------------
// <copyright file="Cell.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System.Data.Common.Utils;
using System.Collections.Generic;
using System.Data.Mapping.ViewGeneration.Validation;
using System.Text;
using System.Diagnostics;
using System.Data.Metadata.Edm;

namespace System.Data.Mapping.ViewGeneration.Structures
{

    /// <summary>
    /// This class contains a pair of cell queries which is essentially a
    /// constraint that they are equal. A cell is initialized with a C or an
    /// S Query which it exposes as properties but it also has the notion of
    /// "Left" and "Right" queries -- left refers to the side for which a
    /// view is being generated 
    /// For example, to
    /// specify a mapping for CPerson to an SPerson table, we have
    ///
    /// [(p type Person) in P : SPerson]
    /// (p.pid, pid)
    /// (p.name, name)
    ///
    /// This really denotes the equality of two queries:
    /// (C) SELECT (p type Person) AS D1, p.pid, p.name FROM p in P WHERE D1 
    /// (S) SELECT True AS D1, pid, name FROM SPerson WHERE D1  
    ///
    /// For more details, see the design doc
    /// </summary>
    internal class Cell : InternalBase
    {
        #region Constructor
        // effects: Creates a cell with the C and S queries 
        private Cell(CellQuery cQuery, CellQuery sQuery, CellLabel label, int cellNumber)
        {
            Debug.Assert(label != null, "Cell lacks label");
            m_cQuery = cQuery;
            m_sQuery = sQuery;
            m_label = label;
            m_cellNumber = cellNumber;
            Debug.Assert(m_sQuery.NumProjectedSlots == m_cQuery.NumProjectedSlots,
                         "Cell queries disagree on the number of projected fields");
        }
        /// <summary>
        /// Copy Constructor
        /// </summary>
        internal Cell(Cell source)
        {
            m_cQuery = new CellQuery(source.m_cQuery);
            m_sQuery = new CellQuery(source.m_sQuery);
            m_label = new CellLabel(source.m_label);
            m_cellNumber = source.m_cellNumber;
        }

        #endregion

        #region Fields
        private CellQuery m_cQuery;
        private CellQuery m_sQuery;
        private int m_cellNumber; // cell number that identifies this cell
        private CellLabel m_label; // The File and Path Info for the CSMappingFragment
        // that the Cell was constructed over.
        // The view cell relation for all projected slots in this
        private ViewCellRelation m_viewCellRelation;
        #endregion

        #region Properties
        // effects: Returns the C query
        internal CellQuery CQuery
        {
            get { return m_cQuery; }
        }

        // effects: Returns the S query
        internal CellQuery SQuery
        {
            get { return m_sQuery; }
        }

        // effects: Returns the CSMappingFragment (if any)
        // that the Cell was constructed over.
        internal CellLabel CellLabel
        {
            get { return m_label; }
        }

        // effects: Returns the cell label (if any)
        internal int CellNumber
        {
            get { return m_cellNumber; }
        }

        internal string CellNumberAsString
        {
            get { return StringUtil.FormatInvariant("V{0}", CellNumber); }
        }
        #endregion

        #region Methods
        // effects: Determines all the identifiers used in this and adds them to identifiers
        internal void GetIdentifiers(CqlIdentifiers identifiers)
        {
            m_cQuery.GetIdentifiers(identifiers);
            m_sQuery.GetIdentifiers(identifiers);
        }

        // effects: Given a cell, determines the paths to which the paths in
        // columns map to in the C-space and returns them. If some columns
        // are not projected in the cell, or if the corresponding properties
        // are not mapped into C-space, returns null
        internal Set<EdmProperty> GetCSlotsForTableColumns(IEnumerable<MemberPath> columns)
        {
            List<int> fieldNums = SQuery.GetProjectedPositions(columns);
            if (fieldNums == null)
            {
                return null;
            }

            // The fields are mapped -- see if they are mapped on the
            // cSide and they correspond to the primary key of the
            // entity set

            Set<EdmProperty> cSideMembers = new Set<EdmProperty>();
            foreach (int fieldNum in fieldNums)
            {
                ProjectedSlot projectedSlot = CQuery.ProjectedSlotAt(fieldNum);
                MemberProjectedSlot slot = projectedSlot as MemberProjectedSlot;
                if (slot != null)
                {
                    // We can call LastMember since columns do not map to
                    // extents or memberEnds. Can cast to EdmProperty since it
                    // cannot be an association end
                    cSideMembers.Add((EdmProperty)slot.MemberPath.LeafEdmMember);
                }
                else
                {
                    return null;
                }
            }
            return cSideMembers;
        }

        // effects: Returns the C query for ViewTarget.QueryView and S query for ViewTarget.UpdateView
        internal CellQuery GetLeftQuery(ViewTarget side)
        {
            return side == ViewTarget.QueryView ? m_cQuery : m_sQuery;
        }

        // effects: Returns the S query for ViewTarget.QueryView and C query for ViewTarget.UpdateView
        internal CellQuery GetRightQuery(ViewTarget side)
        {
            return side == ViewTarget.QueryView ? m_sQuery : m_cQuery;
        }

        // effects: Returns the relation that contains all the slots being
        // projected in this cell 
        internal ViewCellRelation CreateViewCellRelation(int cellNumber)
        {
            if (m_viewCellRelation != null)
            {
                return m_viewCellRelation;
            }
            GenerateCellRelations(cellNumber);
            return m_viewCellRelation;
        }

        private void GenerateCellRelations(int cellNumber)
        {
            // Generate the view cell relation
            List<ViewCellSlot> projectedSlots = new List<ViewCellSlot>();
            // construct a ViewCellSlot for each slot
            Debug.Assert(CQuery.NumProjectedSlots == SQuery.NumProjectedSlots,
                         "Cell queries in cell have a different number of slots");
            for (int i = 0; i < CQuery.NumProjectedSlots; i++)
            {
                ProjectedSlot cSlot = CQuery.ProjectedSlotAt(i);
                ProjectedSlot sSlot = SQuery.ProjectedSlotAt(i);
                Debug.Assert(cSlot != null, "Has cell query been normalized?");
                Debug.Assert(sSlot != null, "Has cell query been normalized?");

                // These slots better be MemberProjectedSlots. We do not have constants etc at this point.
                Debug.Assert(cSlot is MemberProjectedSlot, "cSlot is expected to be MemberProjectedSlot");
                Debug.Assert(sSlot is MemberProjectedSlot, "sSlot is expected to be MemberProjectedSlot");

                MemberProjectedSlot cJoinSlot = (MemberProjectedSlot)cSlot;
                MemberProjectedSlot sJoinSlot = (MemberProjectedSlot)sSlot;

                ViewCellSlot slot = new ViewCellSlot(i, cJoinSlot, sJoinSlot);
                projectedSlots.Add(slot);
            }
            m_viewCellRelation = new ViewCellRelation(this, projectedSlots, cellNumber);
        }

        internal override void ToCompactString(StringBuilder builder)
        {
            CQuery.ToCompactString(builder);
            builder.Append(" = ");
            SQuery.ToCompactString(builder);
        }

        internal override void ToFullString(StringBuilder builder)
        {
            CQuery.ToFullString(builder);
            builder.Append(" = ");
            SQuery.ToFullString(builder);
        }

        public override string ToString()
        {
            return ToFullString();
        }

        // effects: Prints the cells in some human-readable form
        internal static void CellsToBuilder(StringBuilder builder, IEnumerable<Cell> cells)
        {
            // Print mapping
            builder.AppendLine();
            builder.AppendLine("=========================================================================");
            foreach (Cell cell in cells)
            {
                builder.AppendLine();
                StringUtil.FormatStringBuilder(builder, "Mapping Cell V{0}:", cell.CellNumber);
                builder.AppendLine();

                builder.Append("C: ");
                cell.CQuery.ToFullString(builder);
                builder.AppendLine();
                builder.AppendLine();

                builder.Append("S: ");
                cell.SQuery.ToFullString(builder);
                builder.AppendLine();
            }
        }

        #endregion

        #region Factory methods
        internal static Cell CreateCS(CellQuery cQuery, CellQuery sQuery, CellLabel label, int cellNumber)
        {
            return new Cell(cQuery, sQuery, label, cellNumber);
        }
        #endregion
    }
}
