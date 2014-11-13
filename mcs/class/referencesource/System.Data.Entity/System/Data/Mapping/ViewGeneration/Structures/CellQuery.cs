//---------------------------------------------------------------------
// <copyright file="CellQuery.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common.Utils;
using System.Data.Mapping.ViewGeneration.CqlGeneration;
using System.Data.Mapping.ViewGeneration.Utils;
using System.Data.Mapping.ViewGeneration.Validation;
using System.Data.Metadata.Edm;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace System.Data.Mapping.ViewGeneration.Structures
{
    using System.Data.Entity;
    using AttributeSet = Set<MemberPath>;

    /// <summary>
    /// This class stores the C or S query. For example, 
    /// (C) SELECT (p type Person) AS D1, p.pid, p.name FROM p in P WHERE D1 
    /// (S) SELECT True AS D1, pid, name FROM SPerson WHERE D1
    /// 
    /// The cell query is stored in a "factored" manner for ease of
    /// cell-merging and cell manipulation. It contains:
    /// * Projection: A sequence of slots and a sequence of boolean slots (one
    ///   for each cell in the extent)
    /// * A From part represented as a Join tree
    /// * A where clause 
    /// </summary>
    internal class CellQuery : InternalBase
    {
        #region Fields
        /// <summary>
        /// Whether query has a 'SELECT DISTINCT' on top.
        /// </summary>
        internal enum SelectDistinct
        {
            Yes,
            No
        }

        // The boolean expressions that essentially capture the type information
        // Fixed-size list; NULL in the list means 'unused'
        private List<BoolExpression> m_boolExprs;
        // The fields including the key fields
        // May contain NULLs - means 'not in the projection'
        private ProjectedSlot[] m_projectedSlots;
        // where clause: An expression formed using the boolExprs
        private BoolExpression m_whereClause;
        private BoolExpression m_originalWhereClause; // m_originalWhereClause is not changed
        
        private SelectDistinct m_selectDistinct;
        // The from part of the query
        private MemberPath m_extentMemberPath;
        // The basic cell relation for all slots in this
        private BasicCellRelation m_basicCellRelation;
        #endregion

        #region Constructors
        // effects: Creates a cell query with the given projection (slots),
        // from part (joinTreeRoot) and the predicate (whereClause)
        // Used for cell creation
        internal CellQuery(List<ProjectedSlot> slots, BoolExpression whereClause, MemberPath rootMember, SelectDistinct eliminateDuplicates)
            : this(slots.ToArray(), whereClause, new List<BoolExpression>(), eliminateDuplicates, rootMember)
        {
        }



        // effects: Given all the fields, just sets them. 
        internal CellQuery(ProjectedSlot[] projectedSlots,
                          BoolExpression whereClause,
                          List<BoolExpression> boolExprs,
                          SelectDistinct elimDupl, MemberPath rootMember)
        {
            
            m_boolExprs = boolExprs;
            m_projectedSlots = projectedSlots;
            m_whereClause = whereClause;
            m_originalWhereClause = whereClause;
            m_selectDistinct = elimDupl;
            m_extentMemberPath = rootMember;
        }

        /// <summary>
        /// Copy Constructor
        /// </summary>
        internal CellQuery(CellQuery source)
        {
            this.m_basicCellRelation = source.m_basicCellRelation;
            this.m_boolExprs = source.m_boolExprs;
            this.m_selectDistinct = source.m_selectDistinct;
            this.m_extentMemberPath = source.m_extentMemberPath;
            this.m_originalWhereClause = source.m_originalWhereClause;
            this.m_projectedSlots = source.m_projectedSlots;
            this.m_whereClause = source.m_whereClause;
        }

        // effects: Given an existing cellquery, makes a new one based on it
        // but uses the slots as specified with newSlots
        private CellQuery(CellQuery existing, ProjectedSlot[] newSlots) :
            this(newSlots, existing.m_whereClause, existing.m_boolExprs,
                 existing.m_selectDistinct, existing.m_extentMemberPath)
        {
        }
        #endregion

        #region Properties

        internal SelectDistinct SelectDistinctFlag
        {
            get { return m_selectDistinct; }
        }

        // effects: Returns the top levelextent corresponding to this cell query
        internal EntitySetBase Extent
        {
            get
            {
                EntitySetBase extent = m_extentMemberPath.Extent as EntitySetBase;
                Debug.Assert(extent != null, "JoinTreeRoot in cellquery must be an extent");
                return extent;
            }
        }

        // effects: Returns the number of slots projected in the query
        internal int NumProjectedSlots
        {
            get { return m_projectedSlots.Length; }
        }

        internal ProjectedSlot[] ProjectedSlots
        {
            get { return m_projectedSlots; }
        }

        internal List<BoolExpression> BoolVars
        {
            get { return m_boolExprs; }
        }

        // effects: Returns the number of boolean expressions projected in the query
        internal int NumBoolVars
        {
            get { return m_boolExprs.Count; }
        }

        internal BoolExpression WhereClause
        {
            get { return m_whereClause; }
        }

        // effects: Returns the root of the join tree
        internal MemberPath SourceExtentMemberPath
        {
            get { return m_extentMemberPath; }
        }

        // effects: Returns the relation that contains all the slots present
        // in this cell query
        internal BasicCellRelation BasicCellRelation
        {
            get
            {
                Debug.Assert(m_basicCellRelation != null, "BasicCellRelation must be created first");
                return m_basicCellRelation;
            }
        }

        /// <summary>
        /// [WARNING}
        /// After cell merging boolean expression can (most likely) have disjunctions (OR node)
        /// to represent the condition that a tuple came from either of the merged cells.
        /// In this case original where clause IS MERGED CLAUSE with OR!!!
        /// So don't call this after merging. It'll throw or debug assert from within GetConjunctsFromWC()
        /// </summary>
        internal IEnumerable<MemberRestriction> Conditions
        {
            get { return GetConjunctsFromOriginalWhereClause(); }
        }

        #endregion

        #region ProjectedSlots related methods
        // effects: Returns the slotnum projected slot
        internal ProjectedSlot ProjectedSlotAt(int slotNum)
        {
            Debug.Assert(slotNum < m_projectedSlots.Length, "Slot number too high");
            return m_projectedSlots[slotNum];
        }

        // requires: All slots in this are join tree slots
        // This method is called for an S-side query
        // cQuery is the corresponding C-side query in the cell
        // sourceCell is the original cell for "this" and cQuery
        // effects: Checks if any of the columns in "this" are mapped to multiple properties in cQuery. If so,
        // returns an error record about the duplicated slots
        internal ErrorLog.Record CheckForDuplicateFields(CellQuery cQuery, Cell sourceCell)
        {
            // slotMap stores the slots on the S-side and the
            // C-side properties that it maps to
            KeyToListMap<MemberProjectedSlot, int> slotMap = new KeyToListMap<MemberProjectedSlot, int>(ProjectedSlot.EqualityComparer);

            // Note that this does work for self-association. In the manager
            // employee example, ManagerId and EmployeeId from the SEmployee
            // table map to the two ends -- Manager.ManagerId and
            // Employee.EmployeeId in the C Space

            for (int i = 0; i < m_projectedSlots.Length; i++)
            {
                ProjectedSlot projectedSlot = m_projectedSlots[i];
                MemberProjectedSlot slot = projectedSlot as MemberProjectedSlot;
                Debug.Assert(slot != null, "All slots for this method must be JoinTreeSlots");
                slotMap.Add(slot, i);
            }

            StringBuilder builder = null;

            // Now determine the entries that have more than one integer per slot
            bool isErrorSituation = false;

            foreach (MemberProjectedSlot slot in slotMap.Keys)
            {
                ReadOnlyCollection<int> indexes = slotMap.ListForKey(slot);
                Debug.Assert(indexes.Count >= 1, "Each slot must have one index at least");

                if (indexes.Count > 1 &&
                    cQuery.AreSlotsEquivalentViaRefConstraints(indexes) == false)
                {
                    // The column is mapped to more than one property and it
                    // failed the "association corresponds to referential
                    // constraints" check

                    isErrorSituation = true;
                    if (builder == null)
                    {
                        builder = new StringBuilder(System.Data.Entity.Strings.ViewGen_Duplicate_CProperties(Extent.Name));
                        builder.AppendLine();
                    }
                    StringBuilder tmpBuilder = new StringBuilder();
                    for (int i = 0; i < indexes.Count; i++)
                    {
                        int index = indexes[i];
                        if (i != 0)
                        {
                            tmpBuilder.Append(", ");
                        }
                        // The slot must be a JoinTreeSlot. If it isn't it is an internal error
                        MemberProjectedSlot cSlot = (MemberProjectedSlot)cQuery.m_projectedSlots[index];
                        tmpBuilder.Append(cSlot.ToUserString());
                    }
                    builder.AppendLine(Strings.ViewGen_Duplicate_CProperties_IsMapped(slot.ToUserString(), tmpBuilder.ToString()));
                }
            }

            if (false == isErrorSituation)
            {
                return null;
            }

            ErrorLog.Record record = new ErrorLog.Record(true, ViewGenErrorCode.DuplicateCPropertiesMapped, builder.ToString(), sourceCell, String.Empty);
            return record;
        }

        // requires: "this" is a query on the C-side
        // and cSideSlotIndexes corresponds to the indexes
        // (into "this") that the slot is being mapped into
        // cSideSlotIndexes.Count > 1 - that is, a particular column in "this"'s corresponding S-Query 
        // has been mapped to more than one property in "this"
        //
        // effects: Checks that the multiple mappings on the C-side are
        // backed by an appropriate Referential constraint
        // If a column is mapped to two properties <A, B> in a single cell:
        // (a) Must be an association
        // (b) The two properties must be on opposite ends of the association
        // (c) The association must have a RI constraint
        // (d) Ordinal[A] == Ordinal[B] in the RI constraint
        // (c) and (d) can be stated as - the slots are equivalent, i.e.,
        // kept equal via an RI constraint
        private bool AreSlotsEquivalentViaRefConstraints(ReadOnlyCollection<int> cSideSlotIndexes)
        {

            // Check (a): Must be an association
            AssociationSet assocSet = Extent as AssociationSet;
            if (assocSet == null)
            {
                return false;
            }

            // Check (b): The two properties must be on opposite ends of the association
            // There better be exactly two properties!
            Debug.Assert(cSideSlotIndexes.Count > 1, "Method called when no duplicate mapping");
            if (cSideSlotIndexes.Count > 2)
            {
                return false;
            }

            // They better be join tree slots (if they are mapped!) and map to opposite ends 
            MemberProjectedSlot slot0 = (MemberProjectedSlot)m_projectedSlots[cSideSlotIndexes[0]];
            MemberProjectedSlot slot1 = (MemberProjectedSlot)m_projectedSlots[cSideSlotIndexes[1]];

            return slot0.MemberPath.IsEquivalentViaRefConstraint(slot1.MemberPath);
        }

        // requires: The Where clause satisfies the same requirements a GetConjunctsFromWhereClause
        // effects: For each slot that has a NotNull condition in the where
        // clause, checks if it is projected. If all such slots are
        // projected, returns null. Else returns an error record
        internal ErrorLog.Record CheckForProjectedNotNullSlots(Cell sourceCell, IEnumerable<Cell> associationSets)
        {
            StringBuilder builder = new StringBuilder();
            bool foundError = false;

            foreach (MemberRestriction restriction in Conditions)
            {
                if (restriction.Domain.ContainsNotNull())
                {
                    MemberProjectedSlot slot = MemberProjectedSlot.GetSlotForMember(m_projectedSlots, restriction.RestrictedMemberSlot.MemberPath);
                    if (slot == null) //member with not null condition is not mapped in this extent
                    {
                        bool missingMapping = true;
                        if(Extent is EntitySet)
                        {
                            bool isCQuery = sourceCell.CQuery == this;
                            ViewTarget target = isCQuery ? ViewTarget.QueryView : ViewTarget.UpdateView;
                            CellQuery rightCellQuery = isCQuery? sourceCell.SQuery : sourceCell.CQuery;
                            
                            //Find out if there is an association mapping but only if the current Not Null condition is on an EntitySet
                            EntitySet rightExtent = rightCellQuery.Extent as EntitySet;
                            if (rightExtent != null)
                            {
                                List<AssociationSet> associations = MetadataHelper.GetAssociationsForEntitySet(rightCellQuery.Extent as EntitySet);
                                foreach (var association in associations.Where(association => association.AssociationSetEnds.Any(end => ( end.CorrespondingAssociationEndMember.RelationshipMultiplicity == RelationshipMultiplicity.One && 
                                    (MetadataHelper.GetOppositeEnd(end).EntitySet.EdmEquals(rightExtent))))))
                                {
                                    foreach (var associationCell in associationSets.Where(c => c.GetRightQuery(target).Extent.EdmEquals(association)))
                                    {
                                        if (MemberProjectedSlot.GetSlotForMember(associationCell.GetLeftQuery(target).ProjectedSlots, restriction.RestrictedMemberSlot.MemberPath) != null)
                                        {
                                            missingMapping = false;
                                        }
                                    }
                                }
                            }
                        }

                        if (missingMapping)
                        {
                            // condition of NotNull and slot not being projected
                            builder.AppendLine(System.Data.Entity.Strings.ViewGen_NotNull_No_Projected_Slot(
                                               restriction.RestrictedMemberSlot.MemberPath.PathToString(false)));
                            foundError = true;
                        }
                    }
                }
            }
            if (false == foundError)
            {
                return null;
            }
            ErrorLog.Record record = new ErrorLog.Record(true, ViewGenErrorCode.NotNullNoProjectedSlot, builder.ToString(), sourceCell, String.Empty);
            return record;
        }

        internal void FixMissingSlotAsDefaultConstant(int slotNumber, ConstantProjectedSlot slot)
        {
            Debug.Assert(m_projectedSlots[slotNumber] == null, "Another attempt to plug in a default value");
            m_projectedSlots[slotNumber] = slot;
        }

        // requires: projectedSlotMap which contains a mapping of the fields
        // for "this" to integers 
        // effects: Align the fields of this cell query using the
        // projectedSlotMap and generates a new query into newMainQuery
        // Based on the re-aligned fields in this, re-aligns the
        // corresponding fields in otherQuery as well and modifies
        // newOtherQuery to contain it
        // Example:
        //    input:  Proj[A,B,"5"] = Proj[F,"7",G]
        //            Proj[C,B]     = Proj[H,I]
        //            projectedSlotMap: A -> 0, B -> 1, C -> 2
        //   output:  Proj[A,B,null] = Proj[F,"7",null]
        //            Proj[null,B,C] = Proj[null,I,H]
        internal void CreateFieldAlignedCellQueries(CellQuery otherQuery, MemberProjectionIndex projectedSlotMap,
                                                    out CellQuery newMainQuery, out CellQuery newOtherQuery)
        {

            // mainSlots and otherSlots hold the new slots for two queries
            int numAlignedSlots = projectedSlotMap.Count;
            ProjectedSlot[] mainSlots = new ProjectedSlot[numAlignedSlots];
            ProjectedSlot[] otherSlots = new ProjectedSlot[numAlignedSlots];

            // Go through the slots for this query and find the new slot for them
            for (int i = 0; i < m_projectedSlots.Length; i++)
            {

                MemberProjectedSlot slot = m_projectedSlots[i] as MemberProjectedSlot;
                Debug.Assert(slot != null, "All slots during cell normalization must field slots");
                // Get the the ith slot's variable and then get the
                // new slot number from the field map
                int newSlotNum = projectedSlotMap.IndexOf(slot.MemberPath);
                Debug.Assert(newSlotNum >= 0, "Field projected but not in projectedSlotMap");
                mainSlots[newSlotNum] = m_projectedSlots[i];
                otherSlots[newSlotNum] = otherQuery.m_projectedSlots[i];

                // We ignore constants -- note that this is not the
                // isHighpriority or discriminator case.  An example of this
                // is when (say) Address does not have zip but USAddress
                // does.  Then the constraint looks like Pi_NULL, A, B(E) =
                // Pi_x, y, z(S)

                // We don't care about this null in the view generation of
                // the left side. Note that this could happen in inheritance
                // or in cases when say the S side has 20 fields but the C
                // side has only 3 - the other 17 are null or default.

                // NOTE: We allow such constants only on the C side and not
                // ont the S side. Otherwise, we can have a situation Pi_A,
                // B, C(E) = Pi_5, y, z(S) Then someone can set A to 7 and we
                // will not roundtrip. We check for this in validation
            }

            // Make the new cell queries with the new slots
            newMainQuery = new CellQuery(this, mainSlots);
            newOtherQuery = new CellQuery(otherQuery, otherSlots);
        }

        // requires: All slots in this are null or non-constants
        // effects: Returns the non-null slots of this
        internal AttributeSet GetNonNullSlots()
        {
            AttributeSet attributes = new AttributeSet(MemberPath.EqualityComparer);
            foreach (ProjectedSlot projectedSlot in m_projectedSlots)
            {
                // null means 'unused' slot -- we ignore those
                if (projectedSlot != null)
                {
                    MemberProjectedSlot projectedVar = projectedSlot as MemberProjectedSlot;
                    Debug.Assert(projectedVar != null, "Projected slot must not be a constant");
                    attributes.Add(projectedVar.MemberPath);
                }
            }
            return attributes;
        }

        // effects: Returns an error record if the keys of the extent/associationSet being mapped  are
        // present in the projected slots of this query. Returns null
        // otherwise. ownerCell indicates the cell that owns this and
        // resourceString is a resource used for error messages
        internal ErrorLog.Record VerifyKeysPresent(Cell ownerCell, Func<object, object, string> formatEntitySetMessage,
            Func<object, object, object, string> formatAssociationSetMessage, ViewGenErrorCode errorCode)
        {
            List<MemberPath> prefixes = new List<MemberPath>(1);
            // Keep track of the key corresponding to each prefix
            List<ExtentKey> keys = new List<ExtentKey>(1);

            if (Extent is EntitySet)
            {
                // For entity set just get the full path of the key properties
                MemberPath prefix = new MemberPath(Extent);
                prefixes.Add(prefix);
                EntityType entityType = (EntityType)Extent.ElementType;
                List<ExtentKey> entitySetKeys = ExtentKey.GetKeysForEntityType(prefix, entityType);
                Debug.Assert(entitySetKeys.Count == 1, "Currently, we only support primary keys");
                keys.Add(entitySetKeys[0]);

            }
            else
            {
                AssociationSet relationshipSet = (AssociationSet)Extent;
                // For association set, get the full path of the key
                // properties of each end

                foreach (AssociationSetEnd relationEnd in relationshipSet.AssociationSetEnds)
                {
                    AssociationEndMember assocEndMember = relationEnd.CorrespondingAssociationEndMember;
                    MemberPath prefix = new MemberPath(relationshipSet, assocEndMember);
                    prefixes.Add(prefix);
                    List<ExtentKey> endKeys = ExtentKey.GetKeysForEntityType(prefix,
                                                                             MetadataHelper.GetEntityTypeForEnd(assocEndMember));
                    Debug.Assert(endKeys.Count == 1, "Currently, we only support primary keys");
                    keys.Add(endKeys[0]);
                }
            }

            for (int i = 0; i < prefixes.Count; i++)
            {
                MemberPath prefix = prefixes[i];
                // Get all or none key slots that are being projected in this cell query
                List<MemberProjectedSlot> keySlots = MemberProjectedSlot.GetKeySlots(GetMemberProjectedSlots(), prefix);
                if (keySlots == null)
                {
                    ExtentKey key = keys[i];
                    string message;
                    if (Extent is EntitySet)
                    {
                        string keyPropertiesString = MemberPath.PropertiesToUserString(key.KeyFields, true);
                        message = formatEntitySetMessage(keyPropertiesString, Extent.Name);
                    }
                    else
                    {
                        string endName = prefix.RootEdmMember.Name;
                        string keyPropertiesString = MemberPath.PropertiesToUserString(key.KeyFields, false);
                        message = formatAssociationSetMessage(keyPropertiesString, endName, Extent.Name);
                    }
                    ErrorLog.Record error = new ErrorLog.Record(true, errorCode, message, ownerCell, String.Empty);
                    return error;
                }
            }
            return null;
        }

        internal IEnumerable<MemberPath> GetProjectedMembers()
        {
            foreach (MemberProjectedSlot slot in this.GetMemberProjectedSlots())
            {
                yield return slot.MemberPath;
            }
        }

        // effects: Returns the fields in this, i.e., not constants or null slots
        private IEnumerable<MemberProjectedSlot> GetMemberProjectedSlots()
        {
            foreach (ProjectedSlot slot in m_projectedSlots)
            {
                MemberProjectedSlot memberSlot = slot as MemberProjectedSlot;
                if (memberSlot != null)
                {
                    yield return memberSlot;
                }
            }
        }

        // effects: Returns the fields that are used in the query (both projected and non-projected)
        // Output list is a copy, i.e., can be modified by the caller
        internal List<MemberProjectedSlot> GetAllQuerySlots()
        {
            HashSet<MemberProjectedSlot> slots = new HashSet<MemberProjectedSlot>(GetMemberProjectedSlots());
            slots.Add(new MemberProjectedSlot(SourceExtentMemberPath));
            foreach (var restriction in Conditions)
            {
                slots.Add(restriction.RestrictedMemberSlot);
            }
            return new List<MemberProjectedSlot>(slots);
        }

        // effects: returns the index at which this slot appears in the projection
        // or -1 if it is not projected
        internal int GetProjectedPosition(MemberProjectedSlot slot)
        {
            for (int i = 0; i < m_projectedSlots.Length; i++)
            {
                if (MemberProjectedSlot.EqualityComparer.Equals(slot, m_projectedSlots[i]))
                {
                    return i;
                }
            }
            return -1;
        }

        // effects: returns the List of indexes at which this member appears in the projection
        // or empty list if it is not projected
        internal List<int> GetProjectedPositions(MemberPath member)
        {
            List<int> pathIndexes = new List<int>();
            for (int i = 0; i < m_projectedSlots.Length; i++)
            {
                MemberProjectedSlot slot = m_projectedSlots[i] as MemberProjectedSlot;
                if (slot != null && MemberPath.EqualityComparer.Equals(member, slot.MemberPath))
                {
                    pathIndexes.Add(i);
                }
            }
            return pathIndexes;
        }

        // effects: Determines the slot numbers for members in cellQuery
        // Returns a set of those paths in the same order as paths. If even
        // one of the path entries is not projected in the cellquery, returns null
        internal List<int> GetProjectedPositions(IEnumerable<MemberPath> paths)
        {
            List<int> pathIndexes = new List<int>();
            foreach (MemberPath member in paths)
            {
                // Get the index in checkQuery and add to pathIndexes
                List<int> slotIndexes = GetProjectedPositions(member);
                Debug.Assert(slotIndexes != null);
                if (slotIndexes.Count == 0)
                { // member is not projected
                    return null;
                }
                Debug.Assert(slotIndexes.Count == 1, "Expecting the path to be projected only once");
                pathIndexes.Add(slotIndexes[0]);
            }
            return pathIndexes;
        }

        // effects : Return the slot numbers for members in Cell Query that 
        //           represent the association end member passed in.
        internal List<int> GetAssociationEndSlots(AssociationEndMember endMember)
        {
            List<int> slotIndexes = new List<int>();
            Debug.Assert(this.Extent is AssociationSet);
            for (int i = 0; i < m_projectedSlots.Length; i++)
            {
                MemberProjectedSlot slot = m_projectedSlots[i] as MemberProjectedSlot;
                if (slot != null && slot.MemberPath.RootEdmMember.Equals(endMember))
                {
                    slotIndexes.Add(i);
                }
            }
            return slotIndexes;
        }

        // effects: Determines the slot numbers for members in cellQuery
        // Returns a set of those paths in the same order as paths. If even
        // one of the path entries is not projected in the cellquery, returns null
        // If a path is projected more than once, than we choose the one from the
        // slotsToSearchFrom domain. 
        internal List<int> GetProjectedPositions(IEnumerable<MemberPath> paths, List<int> slotsToSearchFrom)
        {
            List<int> pathIndexes = new List<int>();
            foreach (MemberPath member in paths)
            {
                // Get the index in checkQuery and add to pathIndexes
                List<int> slotIndexes = GetProjectedPositions(member);
                Debug.Assert(slotIndexes != null);
                if (slotIndexes.Count == 0)
                { // member is not projected
                    return null;
                }
                int slotIndex = -1;
                if (slotIndexes.Count > 1)
                {
                    for (int i = 0; i < slotIndexes.Count; i++)
                    {
                        if (slotsToSearchFrom.Contains(slotIndexes[i]))
                        {
                            Debug.Assert(slotIndex == -1, "Should be projected only once");
                            slotIndex = slotIndexes[i];
                        }
                    }
                    if (slotIndex == -1)
                    {
                        return null;
                    }
                }
                else
                {
                    slotIndex = slotIndexes[0];
                }
                pathIndexes.Add(slotIndex);
            }
            return pathIndexes;
        }


        // requires: The CellConstantDomains in the OneOfConsts of the where
        // clause are partially done
        // effects: Given the domains of different variables in domainMap,
        // fixes the whereClause of this such that all the
        // CellConstantDomains in OneOfConsts are complete
        internal void UpdateWhereClause(MemberDomainMap domainMap)
        {
            List<BoolExpression> atoms = new List<BoolExpression>();
            foreach (BoolExpression atom in WhereClause.Atoms)
            {
                BoolLiteral literal = atom.AsLiteral;
                MemberRestriction restriction = literal as MemberRestriction;
                Debug.Assert(restriction != null, "All bool literals must be OneOfConst at this point");
                // The oneOfConst needs to be fixed with the new possible values from the domainMap.
                IEnumerable<Constant> possibleValues = domainMap.GetDomain(restriction.RestrictedMemberSlot.MemberPath);
                MemberRestriction newOneOf = restriction.CreateCompleteMemberRestriction(possibleValues);

                // Prevent optimization of single constraint e.g: "300 in (300)"
                // But we want to optimize type constants e.g: "category in (Category)"
                // To prevent optimization of bool expressions we add a Sentinel OneOF

                ScalarRestriction scalarConst = restriction as ScalarRestriction;
                bool addSentinel =
                    scalarConst != null &&
                    !scalarConst.Domain.Contains(Constant.Null) &&
                    !scalarConst.Domain.Contains(Constant.NotNull) &&
                    !scalarConst.Domain.Contains(Constant.Undefined);

                if (addSentinel)
                {
                    domainMap.AddSentinel(newOneOf.RestrictedMemberSlot.MemberPath);
                }

                atoms.Add(BoolExpression.CreateLiteral(newOneOf, domainMap));

                if (addSentinel)
                {
                    domainMap.RemoveSentinel(newOneOf.RestrictedMemberSlot.MemberPath);
                }

            }
            // We create a new whereClause that has the memberDomainMap set
            if (atoms.Count > 0)
            {
                m_whereClause = BoolExpression.CreateAnd(atoms.ToArray());
            }
        }
        #endregion

        #region BooleanExprs related Methods
        // effects: Returns a boolean expression corresponding to the
        // "varNum" boolean in this.
        internal BoolExpression GetBoolVar(int varNum)
        {
            return m_boolExprs[varNum];
        }

        // effects: Initalizes the booleans of this cell query to be
        // true. Creates numBoolVars booleans and sets the cellNum boolean to true
        internal void InitializeBoolExpressions(int numBoolVars, int cellNum)
        {
            //Debug.Assert(m_boolExprs.Count == 0, "Overwriting existing booleans");
            m_boolExprs = new List<BoolExpression>(numBoolVars);
            for (int i = 0; i < numBoolVars; i++)
            {
                m_boolExprs.Add(null);
            }
            Debug.Assert(cellNum < numBoolVars, "Trying to set boolean with too high an index");
            m_boolExprs[cellNum] = BoolExpression.True;
        }

        #endregion
        #region WhereClause related methods
        // requires: The current whereClause corresponds to "True", "OneOfConst" or "
        // "OneOfConst AND ... AND OneOfConst"
        // effects: Yields all the conjuncts (OneOfConsts) in this (i.e., if the whereClause is
        // just True, yields nothing
        internal IEnumerable<MemberRestriction> GetConjunctsFromWhereClause()
        {
            return GetConjunctsFromWhereClause(m_whereClause);
        }

        internal IEnumerable<MemberRestriction> GetConjunctsFromOriginalWhereClause()
        {
            return GetConjunctsFromWhereClause(m_originalWhereClause);
        }


        private IEnumerable<MemberRestriction> GetConjunctsFromWhereClause(BoolExpression whereClause)
        {
            foreach (BoolExpression boolExpr in whereClause.Atoms)
            {
                if (boolExpr.IsTrue)
                {
                    continue;
                }
                MemberRestriction result = boolExpr.AsLiteral as MemberRestriction;
                Debug.Assert(result != null, "Atom must be restriction");
                yield return result;
            }
        }

        // requires: whereClause is of the form specified in GetConjunctsFromWhereClause
        // effects: Converts the whereclause to a user-readable string
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal void WhereClauseToUserString(StringBuilder builder, MetadataWorkspace workspace)
        {
            bool isFirst = true;
            foreach (MemberRestriction restriction in GetConjunctsFromWhereClause())
            {
                if (isFirst == false)
                {
                    builder.Append(System.Data.Entity.Strings.ViewGen_AND);
                }
                restriction.ToUserString(false, builder, workspace);
            }
        }
        #endregion

        #region Full CellQuery methods
        // effects: Determines all the identifiers used in this and adds them to identifiers
        internal void GetIdentifiers(CqlIdentifiers identifiers)
        {
            foreach (ProjectedSlot projectedSlot in m_projectedSlots)
            {
                MemberProjectedSlot slot = projectedSlot as MemberProjectedSlot;
                if (slot != null)
                {
                    slot.MemberPath.GetIdentifiers(identifiers);
                }
            }
            m_extentMemberPath.GetIdentifiers(identifiers);
        }

        internal void CreateBasicCellRelation(ViewCellRelation viewCellRelation)
        {
            List<MemberProjectedSlot> slots = GetAllQuerySlots();
            // Create a base cell relation that has all the scalar slots of this
            m_basicCellRelation = new BasicCellRelation(this, viewCellRelation, slots);
        }

        

        #endregion

        #region String Methods
        // effects: Modifies stringBuilder to contain a string representation
        // of the cell query in terms of the original cells that are being used
        internal override void ToCompactString(StringBuilder stringBuilder)
        {
            // This could be a simplified view where a number of cells
            // got merged or it could be one of the original booleans. So
            // determine their numbers using the booleans in m_cellWrapper
            List<BoolExpression> boolExprs = m_boolExprs;
            int i = 0;
            bool first = true;
            foreach (BoolExpression boolExpr in boolExprs)
            {
                if (boolExpr != null)
                {
                    if (false == first)
                    {
                        stringBuilder.Append(",");
                    }
                    else
                    {
                        stringBuilder.Append("[");
                    }
                    StringUtil.FormatStringBuilder(stringBuilder, "C{0}", i);
                    first = false;
                }
                i++;
            }
            if (true == first)
            {
                // No booleans, i.e., no compact representation. Use full string to avoid empty output
                ToFullString(stringBuilder);
            }
            else
            {
                stringBuilder.Append("]");
            }
        }

        internal override void ToFullString(StringBuilder builder)
        {
            builder.Append("SELECT ");

            if (m_selectDistinct == SelectDistinct.Yes)
            {
                builder.Append("DISTINCT ");
            }

            StringUtil.ToSeparatedString(builder, m_projectedSlots, ", ", "_");

            if (m_boolExprs.Count > 0)
            {
                builder.Append(", Bool[");
                StringUtil.ToSeparatedString(builder, m_boolExprs, ", ", "_");
                builder.Append("]");
            }

            builder.Append(" FROM ");
            m_extentMemberPath.ToFullString(builder);

            if (false == m_whereClause.IsTrue)
            {
                builder.Append(" WHERE ");
                m_whereClause.ToFullString(builder);
            }
        }

        public override string ToString()
        {
            return ToFullString();
        }

        // eSQL representation of cell query
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal string ToESqlString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("\n\tSELECT ");

            if (m_selectDistinct == SelectDistinct.Yes)
            {
                builder.Append("DISTINCT ");
            }

            foreach (ProjectedSlot ps in m_projectedSlots)
            {
                MemberProjectedSlot jtn = ps as MemberProjectedSlot;
                StructuralType st = jtn.MemberPath.LeafEdmMember.DeclaringType;
                StringBuilder sb = new StringBuilder();
                jtn.MemberPath.AsEsql(sb, "e");
                builder.AppendFormat("{0}, ", sb.ToString());
            }
            //remove the extra-comma after the last slot         
            builder.Remove(builder.Length - 2, 2);

            builder.Append("\n\tFROM ");
            EntitySetBase extent = m_extentMemberPath.Extent;
            CqlWriter.AppendEscapedQualifiedName(builder, extent.EntityContainer.Name, extent.Name);
            builder.Append(" AS e");

            if (m_whereClause.IsTrue == false)
            {
                builder.Append("\n\tWHERE ");

                StringBuilder qbuilder = new StringBuilder();
                m_whereClause.AsEsql(qbuilder, "e");

                builder.Append(qbuilder.ToString());
            }
            builder.Append("\n    ");

            return builder.ToString();
        }

        #endregion

    }

}
