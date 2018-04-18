//---------------------------------------------------------------------
// <copyright file="LeftCellWrapper.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System.Collections.Generic;
using System.Data.Common.Utils;
using System.Text;
using System.Linq;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Data.Metadata.Edm;
using System.Data.Common.Utils.Boolean;
using System.Data.Mapping.ViewGeneration.Validation;
using System.Data.Mapping.ViewGeneration.QueryRewriting;

namespace System.Data.Mapping.ViewGeneration.Structures
{

    // This class essentially stores a cell but in a special form. When we
    // are generating a view for an extent, we denote the extent's side (C or
    // S) as the "left side" and the side being used in the view as the right
    // side. For example, in query views, the C side is the left side.
    // 
    // Each LeftCellWrapper is a cell of the form:
    // Project[A1,...,An] (Select[var IN {domain}] (Extent)) = Expr
    // Where
    // - "domain" is a set of multiconstants that correspond to the different
    //   variable values allowed for the cell query
    // - A1 ... An are denoted by Attributes in this and corresponds to
    //   the list of attributes that are projected
    // - Extent is the extent for which th view is being generated
    // - Expr is the expression on the other side to produce the left side of
    //   the cell
    internal class LeftCellWrapper : InternalBase
    {

        #region Fields
        internal static readonly IEqualityComparer<LeftCellWrapper> BoolEqualityComparer = new BoolWrapperComparer();

        private Set<MemberPath> m_attributes;// project: attributes computed by

        // Expr (projected attributes that get set)
        private MemberMaps m_memberMaps;
        private CellQuery m_leftCellQuery; // expression that computes this portion
        private CellQuery m_rightCellQuery; // expression that computes this portion
        
        private HashSet<Cell> m_mergedCells; // Cells that this LeftCellWrapper (MergedCell) wraps.
                                             // At first it starts off with a single cell and during cell merging
                                             // cells from both LeftCellWrappers are concatenated.
        private ViewTarget m_viewTarget;
        private FragmentQuery m_leftFragmentQuery; // Fragment query corresponding to the left cell query of the cell
        
        internal static readonly IComparer<LeftCellWrapper> Comparer = new LeftCellWrapperComparer();
        internal static readonly IComparer<LeftCellWrapper> OriginalCellIdComparer = new CellIdComparer();
        #endregion


        #region Constructor
        // effects: Creates a LeftCellWrapper of the form:
        // Project[attrs] (Select[var IN {domain}] (Extent)) = cellquery
        // memberMaps is the set of maps used for producing the query or update views
        internal LeftCellWrapper(ViewTarget viewTarget, Set<MemberPath> attrs,
                                 FragmentQuery fragmentQuery,
                                 CellQuery leftCellQuery, CellQuery rightCellQuery, MemberMaps memberMaps, IEnumerable<Cell> inputCells)
        {
            m_leftFragmentQuery = fragmentQuery;
            m_rightCellQuery = rightCellQuery;
            m_leftCellQuery = leftCellQuery;
            m_attributes = attrs;
            m_viewTarget = viewTarget;
            m_memberMaps = memberMaps;
            m_mergedCells = new HashSet<Cell>(inputCells);
        }

        internal LeftCellWrapper(ViewTarget viewTarget, Set<MemberPath> attrs,
                                 FragmentQuery fragmentQuery,
                                 CellQuery leftCellQuery, CellQuery rightCellQuery, MemberMaps memberMaps, Cell inputCell)
            : this(viewTarget, attrs, fragmentQuery, leftCellQuery, rightCellQuery, memberMaps, Enumerable.Repeat(inputCell, 1)) { }

        #endregion



        #region Properties

        internal FragmentQuery FragmentQuery
        {
            get { return m_leftFragmentQuery; }
        }

        // effects: Returns the projected fields on the left side
        internal Set<MemberPath> Attributes
        {
            get { return m_attributes; }
        }

        // effects: Returns the original cell number from which the wrapper came
        internal string OriginalCellNumberString
        {
            get
            {
                return StringUtil.ToSeparatedString(m_mergedCells.Select(cell => cell.CellNumberAsString), "+", "");
            }
        }

        // effects: Returns the right domain map associated with the right query 
        internal MemberDomainMap RightDomainMap
        {
            get { return m_memberMaps.RightDomainMap; }
        }

        [Conditional("DEBUG")]
        internal void AssertHasUniqueCell()
        {
            Debug.Assert(m_mergedCells.Count == 1);
        }

        internal IEnumerable<Cell> Cells
        {
            get { return m_mergedCells; }
        }

        // requires: There is only one input cell in this
        // effects: Returns the  input cell provided to view generation as part of the mapping
        internal Cell OnlyInputCell
        {
            get
            {
                AssertHasUniqueCell();
                return m_mergedCells.First();
            }
        }

        // effects: Returns the right CellQuery
        internal CellQuery RightCellQuery
        {
            get { return m_rightCellQuery; }
        }

        internal CellQuery LeftCellQuery
        {
            get { return m_leftCellQuery; }
        }


        // effects: Returns the extent for which the wrapper was built
        internal EntitySetBase LeftExtent
        {
            get
            {
                return m_mergedCells.First().GetLeftQuery(m_viewTarget).Extent;
            }
        }

        // effects: Returns the extent of the right cellquery
        internal EntitySetBase RightExtent
        {
            get
            {
                EntitySetBase result = m_rightCellQuery.Extent;
                Debug.Assert(result != null, "Bad root value in join tree");
                return result;
            }
        }

        #endregion

        #region Methods

        // effects: Yields the input cells in wrappers
        internal static IEnumerable<Cell> GetInputCellsForWrappers(IEnumerable<LeftCellWrapper> wrappers)
        {
            foreach (LeftCellWrapper wrapper in wrappers)
            {
                foreach (Cell cell in wrapper.m_mergedCells)
                {
                    yield return cell;
                }
            }
        }

        // effects: Creates a boolean variable representing the right extent or association end
        internal RoleBoolean CreateRoleBoolean()
        {
            if (RightExtent is AssociationSet)
            {
                Set<AssociationEndMember> ends = GetEndsForTablePrimaryKey();
                if (ends.Count == 1)
                {
                    AssociationSetEnd setEnd = ((AssociationSet)RightExtent).AssociationSetEnds[ends.First().Name];
                    return new RoleBoolean(setEnd);
                }
            }
            return new RoleBoolean(RightExtent);
        }

        // effects: Given a set of wrappers, returns a string that contains the list of extents in the 
        // rightcellQueries of the wrappers
        internal static string GetExtentListAsUserString(IEnumerable<LeftCellWrapper> wrappers)
        {
            Set<EntitySetBase> extents = new Set<EntitySetBase>(EqualityComparer<EntitySetBase>.Default);
            foreach (LeftCellWrapper wrapper in wrappers)
            {
                extents.Add(wrapper.RightExtent);
            }

            StringBuilder builder = new StringBuilder();
            bool isFirst = true;
            foreach (EntitySetBase extent in extents)
            {
                if (isFirst == false)
                {
                    builder.Append(", ");
                }
                isFirst = false;
                builder.Append(extent.Name);
            }
            return builder.ToString();
        }

        internal override void ToFullString(StringBuilder builder)
        {
            builder.Append("P[");
            StringUtil.ToSeparatedString(builder, m_attributes, ",");
            builder.Append("] = ");
            m_rightCellQuery.ToFullString(builder);
        }

        // effects: Modifies stringBuilder to contain the view corresponding
        // to the right cellquery
        internal override void ToCompactString(StringBuilder stringBuilder)
        {
            stringBuilder.Append(OriginalCellNumberString);
        }

        // effects: Writes m_cellWrappers to builder
        internal static void WrappersToStringBuilder(StringBuilder builder, List<LeftCellWrapper> wrappers,
                                                     string header)
        {
            builder.AppendLine()
                   .Append(header)
                   .AppendLine();
            // Sort them according to the original cell number
            LeftCellWrapper[] cellWrappers = wrappers.ToArray();
            Array.Sort(cellWrappers, LeftCellWrapper.OriginalCellIdComparer);

            foreach (LeftCellWrapper wrapper in cellWrappers)
            {
                wrapper.ToCompactString(builder);
                builder.Append(" = ");
                wrapper.ToFullString(builder);
                builder.AppendLine();
            }
        }


        // requires: RightCellQuery.Extent corresponds to a relationship set
        // effects: Returns the ends to which the key of the corresponding
        // table (i.e., the left query) maps to in the relationship set. For
        // example, if RightCellQuery.Extent is OrderOrders and it maps to
        // <oid, otherOid> of table SOrders with key oid, this returns the
        // end to which oid is mapped. Similarly, if we have a link table
        // with the whole key mapped to two ends of the association set, it
        // returns both ends
        private Set<AssociationEndMember> GetEndsForTablePrimaryKey()
        {
            CellQuery rightQuery = RightCellQuery;
            Set<AssociationEndMember> result = new Set<AssociationEndMember>(EqualityComparer<AssociationEndMember>.Default);
            // Get the key slots for the table (they are in the slotMap) and
            // check for that slot on the C-side
            foreach (int keySlot in m_memberMaps.ProjectedSlotMap.KeySlots)
            {
                MemberProjectedSlot slot = (MemberProjectedSlot)rightQuery.ProjectedSlotAt(keySlot);
                MemberPath path = slot.MemberPath;
                // See what end it maps to in the relationSet
                AssociationEndMember endMember = (AssociationEndMember)path.RootEdmMember;
                Debug.Assert(endMember != null, "Element in path before scalar path is not end property?");
                result.Add(endMember);
            }
            Debug.Assert(result != null, "No end found for keyslots of table?");
            return result;
        }


        internal MemberProjectedSlot GetLeftSideMappedSlotForRightSideMember(MemberPath member)
        {
            int projectedPosition = RightCellQuery.GetProjectedPosition(new MemberProjectedSlot(member));
            if (projectedPosition == -1)
            {
                return null;
            }

            ProjectedSlot slot = LeftCellQuery.ProjectedSlotAt(projectedPosition);

            if (slot == null || slot is ConstantProjectedSlot)
            {
                return null;
            }

            return slot as MemberProjectedSlot;            
        }

        internal MemberProjectedSlot GetRightSideMappedSlotForLeftSideMember(MemberPath member)
        {
            int projectedPosition = LeftCellQuery.GetProjectedPosition(new MemberProjectedSlot(member));
            if (projectedPosition == -1)
            {
                return null;
            }

            ProjectedSlot slot = RightCellQuery.ProjectedSlotAt(projectedPosition);

            if (slot == null || slot is ConstantProjectedSlot)
            {
                return null;
            }

            return slot as MemberProjectedSlot;
        }

        internal MemberProjectedSlot GetCSideMappedSlotForSMember(MemberPath member)
        {
            if (m_viewTarget == ViewTarget.QueryView)
            {
                return GetLeftSideMappedSlotForRightSideMember(member);
            }
            else
            {
                return GetRightSideMappedSlotForLeftSideMember(member);
            }
        }

        #endregion

        #region Equality Comparer class
        // This class compares wrappers based on the Right Where Clause and
        // Extent -- needed for the boolean engine
        private class BoolWrapperComparer : IEqualityComparer<LeftCellWrapper>
        {

            public bool Equals(LeftCellWrapper left, LeftCellWrapper right)
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
                bool whereClauseEqual = BoolExpression.EqualityComparer.Equals(left.RightCellQuery.WhereClause,
                                                                               right.RightCellQuery.WhereClause);

                return left.RightExtent.Equals(right.RightExtent) && whereClauseEqual;
            }

            public int GetHashCode(LeftCellWrapper wrapper)
            {
                return BoolExpression.EqualityComparer.GetHashCode(wrapper.RightCellQuery.WhereClause) ^ wrapper.RightExtent.GetHashCode();
            }
        }
        #endregion

        #region Comparer
        // A class that compares two cell wrappers. Useful for guiding heuristics
        // and to ensure that the largest selection domain (i.e., the number of
        // multiconstants in "mc in {...}") is first in the list
        private class LeftCellWrapperComparer : IComparer<LeftCellWrapper>
        {

            public int Compare(LeftCellWrapper x, LeftCellWrapper y)
            {

                // More attributes first -- so that we get most attributes
                // with very few intersections (when we use the sortings for
                // that). When we are subtracting, attributes are not important

                // Use FragmentQuery's attributes instead of LeftCellWrapper's original attributes in the comparison
                // since the former might have got extended to include all attributes whose value is determined
                // by the WHERE clause (e.g., if we have WHERE ProductName='Camera' we can assume ProductName is projected)

                if (x.FragmentQuery.Attributes.Count > y.FragmentQuery.Attributes.Count)
                {
                    return -1;
                }
                else if (x.FragmentQuery.Attributes.Count < y.FragmentQuery.Attributes.Count)
                {
                    return 1;
                }
                // Since the sort may not be stable, we use the original cell number string to break the tie
                return String.CompareOrdinal(x.OriginalCellNumberString, y.OriginalCellNumberString);
            }
        }

        // A class that compares two cell wrappers based on original cell number
        internal class CellIdComparer : IComparer<LeftCellWrapper>
        {

            public int Compare(LeftCellWrapper x, LeftCellWrapper y)
            {
                return StringComparer.Ordinal.Compare(x.OriginalCellNumberString, y.OriginalCellNumberString);
            }
        }

        #endregion
    }
}
