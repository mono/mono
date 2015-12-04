//---------------------------------------------------------------------
// <copyright file="Domain.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System.Collections.Generic;
using System.Data.Common.Utils;
using System.Data.Entity;
using System.Data.Mapping.ViewGeneration.Utils;
using System.Data.Metadata.Edm;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace System.Data.Mapping.ViewGeneration.Structures
{
    using CellConstantSet = Set<Constant>;

    // A set of cell constants -- to keep track of a cell constant's domain
    // values. It encapsulates the notions of NULL, NOT NULL and can be
    // enhanced in the future with more functionality
    // To represent "infinite" domains such as integer, a special constant CellConstant.NotNull is used. 
    // For example: domain of System.Boolean is {true, false}, domain of
    // (nullable) System.Int32 property is {Null, NotNull}.
    internal class Domain : InternalBase
    {

        #region Constructors
        // effects: Creates an "fully-done" set with no values -- possibleDiscreteValues are the values
        // that this domain can take
        internal Domain(Constant value, IEnumerable<Constant> possibleDiscreteValues) :
            this(new Constant[] { value }, possibleDiscreteValues)
        {
        }

        // effects: Creates a domain populated using values -- possibleValues
        // are all possible values that this can take
        internal Domain(IEnumerable<Constant> values,
                                    IEnumerable<Constant> possibleDiscreteValues)
        {
            // Note that the values can contain both null and not null
            Debug.Assert(values != null);
            Debug.Assert(possibleDiscreteValues != null);
            // Determine the possibleValues first and then create the negatedConstant
            m_possibleValues = DeterminePossibleValues(values, possibleDiscreteValues);

            // Now we need to make sure that m_domain is correct. if "values" (v) already has
            // the negated stuff, we need to make sure it is in conformance
            // with what m_possibleValues (p) has

            // For NOT --> Add all constants into d that are present in p but
            // not in the NOT
            // v = 1, NOT(1, 2);                            p = 1, 2, 3         => d = 1, NOT(1, 2, 3), 3
            // v = 1, 2, NOT(1);                            p = 1, 2, 4         => d = 1, 2, 4, NOT(1, 2, 4)
            // v = 1, 2, NOT(1, 2, 4), NOT(1, 2, 4, 5);     p = 1, 2, 4, 5, 6   => d = 1, 2, 5, 6, NOT(1, 2, 4, 5, 6)

            // NotNull works naturally now. If possibleValues has (1, 2, NULL) and values has NOT(NULL), add 1, 2 to m_domain

            m_domain = ExpandNegationsInDomain(values, m_possibleValues);
            AssertInvariant();
        }

        // effects: Creates a copy of the set "domain"
        internal Domain(Domain domain)
        {
            m_domain = new Set<Constant>(domain.m_domain, Constant.EqualityComparer);
            m_possibleValues = new Set<Constant>(domain.m_possibleValues, Constant.EqualityComparer);
            AssertInvariant();
        }
        #endregion

        #region Fields
        // The set of values in the cell constant domain
        private CellConstantSet m_domain; // e.g., 1, 2, NULL, NOT(1, 2, NULL)
        private CellConstantSet m_possibleValues; // e.g., 1, 2, NULL, Undefined
        // Invariant: m_domain is a subset of m_possibleValues except for a
        // negated constant
        #endregion

        #region Properties
        // effects: Returns all the possible values that this can contain (including the negated constants)
        internal IEnumerable<Constant> AllPossibleValues
        {
            get { return AllPossibleValuesInternal; }
        }

        // effects: Returns all the possible values that this can contain (including the negated constants)
        private Set<Constant> AllPossibleValuesInternal
        {
            get
            {
                NegatedConstant negatedPossibleValue = new NegatedConstant(m_possibleValues);
                return m_possibleValues.Union(new Constant[] { negatedPossibleValue });
            }
        }

        // effects: Returns the number of constants in this (including a negated constant)
        internal int Count
        {
            get { return m_domain.Count; }
        }

        /// <summary>
        /// Yields the set of all values in the domain.
        /// </summary>
        internal IEnumerable<Constant> Values
        {
            get { return m_domain; }
        }
        #endregion

        #region Static Helper Methods to create cell constant sets from metadata
        // effects: Given a member, determines all possible values that can be created from Metadata
        internal static CellConstantSet DeriveDomainFromMemberPath(MemberPath memberPath, EdmItemCollection edmItemCollection, bool leaveDomainUnbounded)
        {
            CellConstantSet domain = DeriveDomainFromType(memberPath.EdmType, edmItemCollection, leaveDomainUnbounded);
            if (memberPath.IsNullable)
            {
                domain.Add(Constant.Null);
            }
            return domain;
        }


        // effects: Given a type, determines all possible values that can be created from Metadata
        private static CellConstantSet DeriveDomainFromType(EdmType type, EdmItemCollection edmItemCollection, bool leaveDomainUnbounded)
        {
            CellConstantSet domain = null;

            if (Helper.IsScalarType(type))
            {                
                // Get the domain for scalars -- for booleans, we special case. 
                if (MetadataHelper.HasDiscreteDomain(type))
                {
                    Debug.Assert(Helper.AsPrimitive(type).PrimitiveTypeKind == PrimitiveTypeKind.Boolean, "Only boolean type has discrete domain.");

                    // Closed domain
                    domain = new Set<Constant>(CreateList(true, false), Constant.EqualityComparer);
                }
                else
                {
                    // Unbounded domain
                    domain = new Set<Constant>(Constant.EqualityComparer);
                    if (leaveDomainUnbounded)
                    {
                        domain.Add(Constant.NotNull);
                    }
                }
            }
            else //Type Constants - Domain is all possible concrete subtypes
            {
                Debug.Assert(Helper.IsEntityType(type) || Helper.IsComplexType(type) || Helper.IsRefType(type) || Helper.IsAssociationType(type));

                // Treat ref types as their referenced entity types
                if (Helper.IsRefType(type))
                {
                    type = ((RefType)type).ElementType;
                }

                List<Constant> types = new List<Constant>();
                foreach (EdmType derivedType in MetadataHelper.GetTypeAndSubtypesOf(type, edmItemCollection, false /*includeAbstractTypes*/))
                {
                    TypeConstant derivedTypeConstant = new TypeConstant(derivedType);
                    types.Add(derivedTypeConstant);
                }
                domain = new Set<Constant>(types, Constant.EqualityComparer);
            }

            Debug.Assert(domain != null, "Domain not set up for some type");
            return domain;
        }

        // effect: returns the default value for the member
        // if the member is nullable and has no default, changes default value to CellConstant.NULL and returns true
        // if the mebmer is not nullable and has no default, returns false
        // CHANGE_[....]_FEATURE_DEFAULT_VALUES: return the right default once metadata supports it
        internal static bool TryGetDefaultValueForMemberPath(MemberPath memberPath, out Constant defaultConstant)
        {
            object defaultValue = memberPath.DefaultValue;
            defaultConstant = Constant.Null;
            if (defaultValue != null)
            {
                defaultConstant = new ScalarConstant(defaultValue);
                return true;
            }
            else if (memberPath.IsNullable || memberPath.IsComputed)
            {
                return true;
            }
            return false;
        }

        internal static Constant GetDefaultValueForMemberPath(MemberPath memberPath, IEnumerable<LeftCellWrapper> wrappersForErrorReporting,
                                                                  ConfigViewGenerator config)
        {
            Constant defaultValue = null;
            if (!Domain.TryGetDefaultValueForMemberPath(memberPath, out defaultValue))
            {
                string message = Strings.ViewGen_No_Default_Value(memberPath.Extent.Name, memberPath.PathToString(false));
                ErrorLog.Record record = new ErrorLog.Record(true, ViewGenErrorCode.NoDefaultValue, message, wrappersForErrorReporting, String.Empty);
                ExceptionHelpers.ThrowMappingException(record, config);
            }
            return defaultValue;
        }

        #endregion

        #region External methods
        internal int GetHash()
        {
            int result = 0;
            foreach (Constant constant in m_domain)
            {
                result ^= Constant.EqualityComparer.GetHashCode(constant);
            }
            return result;
        }

        // effects: Returns true iff this domain has the same values as
        // second. Note that this method performs a semantic check not just
        // an element by element check
        internal bool IsEqualTo(Domain second)
        {
            return m_domain.SetEquals(second.m_domain);
        }

        // requires: this is complete
        // effects: Returns true iff this contains NOT(NULL OR ....)
        internal bool ContainsNotNull()
        {
            NegatedConstant negated = GetNegatedConstant(m_domain);
            return negated != null && negated.Contains(Constant.Null);
        }

        /// <summary>
        /// Returns true if the domain contains the given Cell Constant
        /// </summary>
        internal bool Contains(Constant constant)
        {
            return m_domain.Contains(constant);
        }

        // effects: Given a set of values in domain, "normalizes" it, i.e.,
        // all positive constants are seperated out and any negative constant
        // is changed s.t. it is the negative of all positive values
        // extraValues indicates more constants that domain could take, e.g.,
        // domain could be "1, 2, NOT(1, 2)", extraValues could be "3". In
        // this case, we return "1, 2, 3, NOT(1, 2, 3)"
        internal static CellConstantSet ExpandNegationsInDomain(IEnumerable<Constant> domain, IEnumerable<Constant> otherPossibleValues)
        {

            //Finds all constants referenced in (domain UNION extraValues) e.g: 1, NOT(2) =>  1, 2
            CellConstantSet possibleValues = DeterminePossibleValues(domain, otherPossibleValues);

            // For NOT --> Add all constants into d that are present in p but
            // not in the NOT
            // v = 1, NOT(1, 2); p = 1, 2, 3 => d = 1, NOT(1, 2, 3), 3
            // v = 1, 2, NOT(1); p = 1, 2, 4 => d = 1, 2, 4, NOT(1, 2, 4)
            // v = 1, 2, NOT(1, 2, 4), NOT(1, 2, 4, 5); p = 1, 2, 4, 5, 6 => d = 1, 2, 5, 6, NOT(1, 2, 4, 5, 6)

            // NotNull works naturally now. If possibleValues has (1, 2, NULL)
            // and values has NOT(NULL), add 1, 2 to m_domain
            CellConstantSet result = new Set<Constant>(Constant.EqualityComparer);

            foreach (Constant constant in domain)
            {
                NegatedConstant negated = constant as NegatedConstant;
                if (negated != null)
                {
                    result.Add(new NegatedConstant(possibleValues));
                    // Compute all elements in possibleValues that are not present in negated. E.g., if
                    // negated is NOT(1, 2, 3) and possibleValues is 1, 2, 3,
                    // 4, we need to add 4 to result
                    CellConstantSet remainingElements = possibleValues.Difference(negated.Elements);
                    result.AddRange(remainingElements);
                }
                else
                {
                    result.Add(constant);
                }
            }
            return result;

        }

        internal static CellConstantSet ExpandNegationsInDomain(IEnumerable<Constant> domain)
        {
            return ExpandNegationsInDomain(domain, domain);
        }


        // effects: Given a set of values in domain
        // Returns all possible values that are present in domain. 
        static CellConstantSet DeterminePossibleValues(IEnumerable<Constant> domain)
        {

            // E.g., if we have 1, 2, NOT(1) --> Result = 1, 2
            // 1, NOT(1, 2) --> Result = 1, 2
            // 1, 2, NOT(NULL) --> Result = 1, 2, NULL
            // 1, 2, NOT(2), NOT(3, 4) --> Result = 1, 2, 3, 4

            CellConstantSet result = new CellConstantSet(Constant.EqualityComparer);

            foreach (Constant constant in domain)
            {
                NegatedConstant negated = constant as NegatedConstant;

                if (negated != null)
                {

                    // Go through all the constants in negated and add them to domain
                    // We add them to possible values also even if (say) Null is not allowed because we want the complete 
                    // partitioning of the space, e.g., if the values specified by the caller are 1, NotNull -> we want 1, Null
                    foreach (Constant constElement in negated.Elements)
                    {
                        Debug.Assert(constElement as NegatedConstant == null, "Negated cell constant inside NegatedCellConstant");
                        result.Add(constElement);
                    }
                }
                else
                {
                    result.Add(constant);
                }
            }

            return result;
        }
        #endregion

        #region Helper methods for determining domains from cells
        // effects: Given a set of cells, returns all the different values
        // that each memberPath in cells can take
        internal static Dictionary<MemberPath, CellConstantSet>
            ComputeConstantDomainSetsForSlotsInQueryViews(IEnumerable<Cell> cells, EdmItemCollection edmItemCollection, bool isValidationEnabled)
        {

            Dictionary<MemberPath, CellConstantSet> cDomainMap =
                new Dictionary<MemberPath, CellConstantSet>(MemberPath.EqualityComparer);

            foreach (Cell cell in cells)
            {
                CellQuery cQuery = cell.CQuery;
                // Go through the conjuncts to get the constants (e.g., we
                // just don't want to NULL, NOT(NULL). We want to say that
                // the possible values are NULL, 4, NOT(NULL, 4)
                foreach (MemberRestriction restriction in cQuery.GetConjunctsFromWhereClause())
                {
                    MemberProjectedSlot slot = restriction.RestrictedMemberSlot;
                    CellConstantSet cDomain = DeriveDomainFromMemberPath(slot.MemberPath, edmItemCollection, isValidationEnabled);
                    // Now we add the domain of oneConst into this
                    //Isnull=true and Isnull=false conditions should not contribute to a member's domain
                    cDomain.AddRange(restriction.Domain.Values.Where(c => !(c.Equals(Constant.Null) || c.Equals(Constant.NotNull))));
                    CellConstantSet values;
                    bool found = cDomainMap.TryGetValue(slot.MemberPath, out values);
                    if (!found)
                    {
                        cDomainMap[slot.MemberPath] = cDomain;
                    }
                    else
                    {
                        values.AddRange(cDomain);
                    }
                }
            }
            return cDomainMap;
        }

        //True = domain is restricted, False = domain is not restricted (because there is no condition)
        private static bool GetRestrictedOrUnrestrictedDomain(MemberProjectedSlot slot, CellQuery cellQuery, EdmItemCollection edmItemCollection, out CellConstantSet domain)
        {
            CellConstantSet domainValues = DeriveDomainFromMemberPath(slot.MemberPath, edmItemCollection, true /* leaveDomainUnbounded */);

            //Note, out domain is set even in the case where method call returns false
            return TryGetDomainRestrictedByWhereClause(domainValues, slot, cellQuery, out domain);
        }


        // effects: returns a dictionary that maps each S-side slot whose domain can be restricted to such an enumerated domain
        // The resulting domain is a union of
        // (a) constants appearing in conditions on that slot on S-side
        // (b) constants appearing in conditions on the respective slot on C-side, if the given slot 
        //     is projected (on the C-side) and no conditions are placed on it on S-side
        // (c) default value of the slot based on metadata
        internal static Dictionary<MemberPath, CellConstantSet>
            ComputeConstantDomainSetsForSlotsInUpdateViews(IEnumerable<Cell> cells, EdmItemCollection edmItemCollection)
        {

            Dictionary<MemberPath, CellConstantSet> updateDomainMap = new Dictionary<MemberPath, CellConstantSet>(MemberPath.EqualityComparer);

            foreach (Cell cell in cells)
            {

                CellQuery cQuery = cell.CQuery;
                CellQuery sQuery = cell.SQuery;

                foreach (MemberProjectedSlot sSlot in sQuery.GetConjunctsFromWhereClause().Select(oneOfConst => oneOfConst.RestrictedMemberSlot))
                {

                    // obtain initial slot domain and restrict it if the slot has conditions


                    CellConstantSet restrictedDomain;
                    bool wasDomainRestricted = GetRestrictedOrUnrestrictedDomain(sSlot, sQuery, edmItemCollection, out restrictedDomain);

                    // Suppose that we have a cell: 
                    //      Proj(ID, A) WHERE(A=5) FROM E   =   Proj(ID, B) FROM T

                    // In the above cell, B on the S-side is 5 and we add that to its range. But if B had a restriction, 
                    // we do not add 5. Note that do we not have a problem w.r.t. possibleValues since if A=5 and B=1, we have an
                    // empty cell -- we should catch that as an error. If A = 5 and B = 5 is present then restrictedDomain 
                    // and domainValues are the same

                    // if no restriction on the S-side and the slot is projected then take the domain from the C-side
                    if (!wasDomainRestricted)
                    {
                        int projectedPosition = sQuery.GetProjectedPosition(sSlot);
                        if (projectedPosition >= 0)
                        {
                            // get the domain of the respective C-side slot
                            MemberProjectedSlot cSlot = cQuery.ProjectedSlotAt(projectedPosition) as MemberProjectedSlot;
                            Debug.Assert(cSlot != null, "Assuming constants are not projected");

                            wasDomainRestricted = GetRestrictedOrUnrestrictedDomain(cSlot, cQuery, edmItemCollection, out restrictedDomain);

                            if (!wasDomainRestricted)
                            {
                                continue;
                            }
                        }
                    }

                    // Add the default value to the domain
                    MemberPath sSlotMemberPath = sSlot.MemberPath;
                    Constant defaultValue;
                    if (TryGetDefaultValueForMemberPath(sSlotMemberPath, out defaultValue))
                    {
                        restrictedDomain.Add(defaultValue);
                    }

                    // add all constants appearing in the domain to sDomainMap
                    CellConstantSet sSlotDomain;
                    if (!updateDomainMap.TryGetValue(sSlotMemberPath, out sSlotDomain))
                    {
                        updateDomainMap[sSlotMemberPath] = restrictedDomain;
                    }
                    else
                    {
                        sSlotDomain.AddRange(restrictedDomain);
                    }
                }
            }
            return updateDomainMap;
        }

        // requires: domain not have any Negated constants other than NotNull
        // Also, cellQuery contains all final oneOfConsts or all partial oneOfConsts
        // cellquery must contain a whereclause of the form "True", "OneOfConst" or "
        // "OneOfConst AND ... AND OneOfConst"
        // slot must present in cellQuery and incomingDomain is the domain for it
        // effects: Returns the set of values that slot can take as restricted by cellQuery's whereClause
        private static bool TryGetDomainRestrictedByWhereClause(IEnumerable<Constant> domain, MemberProjectedSlot slot, CellQuery cellQuery, out CellConstantSet result)
        {

            var conditionsForSlot = cellQuery.GetConjunctsFromWhereClause()
                                  .Where(restriction => MemberPath.EqualityComparer.Equals(restriction.RestrictedMemberSlot.MemberPath, slot.MemberPath))
                                  .Select(restriction => new CellConstantSet(restriction.Domain.Values, Constant.EqualityComparer));

            //Debug.Assert(!conditionsForSlot.Skip(1).Any(), "More than one Clause with the same path");

            if (!conditionsForSlot.Any())
            {
                // If the slot was not mentioned in the query return the domain without restricting it 
                result = new CellConstantSet(domain);
                return false;
            }



            // Now get all the possible values from domain and conditionValues
            CellConstantSet possibleValues = DeterminePossibleValues(conditionsForSlot.SelectMany(m => m.Select(c => c)), domain);

            Domain restrictedDomain = new Domain(domain, possibleValues);
            foreach (var conditionValues in conditionsForSlot)
            {
                // Domain derived from Edm-Type INTERSECTED with Conditions
                restrictedDomain = restrictedDomain.Intersect(new Domain(conditionValues, possibleValues));
            }

            result = new CellConstantSet(restrictedDomain.Values, Constant.EqualityComparer);
            return !domain.SequenceEqual(result);
        }
        #endregion

        #region Private helper methods
        // effects: Intersects the values in second with this domain and
        // returns the result
        private Domain Intersect(Domain second)
        {
            CheckTwoDomainInvariants(this, second);
            Domain result = new Domain(this);
            result.m_domain.Intersect(second.m_domain);
            return result;
        }

        // requires: constants has at most one NegatedCellConstant
        // effects: Returns the NegatedCellConstant in this if any. Else
        // returns null
        private static NegatedConstant GetNegatedConstant(IEnumerable<Constant> constants)
        {
            NegatedConstant result = null;
            foreach (Constant constant in constants)
            {
                NegatedConstant negated = constant as NegatedConstant;
                if (negated != null)
                {
                    Debug.Assert(result == null, "Multiple negated cell constants?");
                    result = negated;
                }
            }
            return result;
        }

        // effects: Given a set of values in domain1 and domain2,
        // Returns all possible positive values that are present in domain1 and domain2
        private static CellConstantSet DeterminePossibleValues(IEnumerable<Constant> domain1, IEnumerable<Constant> domain2)
        {
            CellConstantSet union = new CellConstantSet(domain1, Constant.EqualityComparer).Union(domain2);
            CellConstantSet result = DeterminePossibleValues(union);
            return result;
        }

        // effects: Checks that two domains, domain1 and domain2, that are being compared/unioned/intersected, etc
        // are compatible with each other
        [Conditional("DEBUG")]
        private static void CheckTwoDomainInvariants(Domain domain1, Domain domain2)
        {
            domain1.AssertInvariant();
            domain2.AssertInvariant();

            // The possible values must match
            Debug.Assert(domain1.m_possibleValues.SetEquals(domain2.m_possibleValues), "domains must be compatible");
        }

        // effects: A helper method. Given two
        // values, yields a list of CellConstants in the order of values
        private static IEnumerable<Constant> CreateList(object value1, object value2)
        {
            yield return new ScalarConstant(value1);
            yield return new ScalarConstant(value2);
        }

        // effects: Checks the invariants in "this"
        internal void AssertInvariant()
        {
            // Make sure m_domain has at most one negatedCellConstant
            //  m_possibleValues has none
            NegatedConstant negated = GetNegatedConstant(m_domain); // Can be null or not-null

            negated = GetNegatedConstant(m_possibleValues);
            Debug.Assert(negated == null, "m_possibleValues cannot contain negated constant");

            Debug.Assert(m_domain.IsSubsetOf(AllPossibleValuesInternal),
                         "All domain values must be contained in possibleValues");
        }
        #endregion

        #region String methods
        // effects: Returns a user-friendly string that can be reported to an end-user
        internal string ToUserString()
        {
            StringBuilder builder = new StringBuilder();
            bool isFirst = true;
            foreach (Constant constant in m_domain)
            {
                if (isFirst == false)
                {
                    builder.Append(", ");
                }
                builder.Append(constant.ToUserString());
                isFirst = false;
            }
            return builder.ToString();
        }

        internal override void ToCompactString(StringBuilder builder)
        {
            builder.Append(ToUserString());
        }
        #endregion

    }
}
