//---------------------------------------------------------------------
// <copyright file="MemberDomainMap.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

namespace System.Data.Mapping.ViewGeneration.Structures
{
    using System.Collections.Generic;
    using System.Data.Common.Utils;
    using System.Data.Mapping.ViewGeneration.Utils;
    using System.Data.Metadata.Edm;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using CellConstantSet = Common.Utils.Set<Constant>;

    // This class keeps track of the domain values of the different members
    // in a schema. E.g., for a discriminator, it keeps track of "P",
    // "C"; for type of Person, it keeps track of Person, Customer, etc
    // It exposes two concepts -- the domain of a member variable and the
    // different possible values for that member, e.g., the possible values
    // could be 3, 4, 5 but the domain could be 3, 4 (domain is always a
    // subset of possibleVales
    internal class MemberDomainMap : InternalBase
    {

        #region Fields
        // Keep track of the actual domain for each member on which we have conditions
        // Note: some subtleties: For QueryDomainMap it holds just C-side condition members. For UpdateDominMap
        // it now holds S-side condition members as well as members with no s-side condition but C-side condition
        // such that C-side condition restricts the domain of the member(column).
        private Dictionary<MemberPath, CellConstantSet> m_conditionDomainMap;
        // Keep track of the actual domain for each member on which we have no conditions
        // CellConstantSet in m_nonConditionDomainMap is really CellConstantSetInfo
        private Dictionary<MemberPath, CellConstantSet> m_nonConditionDomainMap;

        // members on C-side that are projected, don't have conditions, but the respective S-side members do
        // we need to threat those just as regular members except in validation, where S-side conditions are 
        // projected to C-side. For that, KB needs to add the respective constraints involving this members
        // For example: CPerson1.Phone IN {?, NOT(?, NULL)) on C-side. We need to know that
        // type(CPerson1)=Customer <-> !(CPerson1.Phone IN {?}) for validation of domain constraints
        private Set<MemberPath> m_projectedConditionMembers = new Set<MemberPath>();

        private EdmItemCollection m_edmItemCollection;

        #endregion


        #region Constructor
        private MemberDomainMap(Dictionary<MemberPath, CellConstantSet> domainMap,
                                Dictionary<MemberPath, CellConstantSet> nonConditionDomainMap, EdmItemCollection edmItemCollection)
        {
            m_conditionDomainMap = domainMap;
            m_nonConditionDomainMap = nonConditionDomainMap;
            m_edmItemCollection = edmItemCollection;
        }
        // effects: Creates a map with all the condition member constants
        // from extentCells. viewtarget determines whether the view is an
        // update or query view
        internal MemberDomainMap(ViewTarget viewTarget, bool isValidationEnabled, IEnumerable<Cell> extentCells, EdmItemCollection edmItemCollection, ConfigViewGenerator config, Dictionary<EntityType, Set<EntityType>> inheritanceGraph)
        {
            m_conditionDomainMap = new Dictionary<MemberPath, CellConstantSet>(MemberPath.EqualityComparer);
            m_edmItemCollection = edmItemCollection;

            Dictionary<MemberPath, CellConstantSet> domainMap = null;
            if (viewTarget == ViewTarget.UpdateView)
            {
                domainMap = Domain.ComputeConstantDomainSetsForSlotsInUpdateViews(extentCells, m_edmItemCollection);
            }
            else
            {
                domainMap = Domain.ComputeConstantDomainSetsForSlotsInQueryViews(extentCells, m_edmItemCollection, isValidationEnabled);
            }

            foreach (Cell cell in extentCells)
            {
                CellQuery cellQuery = cell.GetLeftQuery(viewTarget);
                // Get the atoms from cellQuery and only keep the ones that
                // are condition members
                foreach (MemberRestriction condition in cellQuery.GetConjunctsFromWhereClause())
                {
                    // Note: TypeConditions are created using OneOfTypeConst and
                    // scalars are created using OneOfScalarConst
                    MemberPath memberPath = condition.RestrictedMemberSlot.MemberPath;

                    Debug.Assert(condition is ScalarRestriction || condition is TypeRestriction,
                                 "Unexpected restriction");

                    // Take the narrowed domain from domainMap, if any
                    CellConstantSet domainValues;
                    if (!domainMap.TryGetValue(memberPath, out domainValues))
                    {
                        domainValues = Domain.DeriveDomainFromMemberPath(memberPath, edmItemCollection, isValidationEnabled);
                    }

                    //Don't count conditions that are satisfied through IsNull=false 
                    if (!domainValues.Contains(Constant.Null))
                    {
                        //multiple values of condition represent disjunction in conditions (not currently supported)
                        // if there is any condition constant that is NotNull
                        if (condition.Domain.Values.All(conditionConstant => (conditionConstant.Equals(Constant.NotNull))))
                        {
                            continue;
                        }
                        //else there is atleast one condition value that is allowed, continue view generation
                    }

                    //------------------------------------------
                    //|  Nullable  |   IsNull  |   Test case   |
                    //|     T      |     T     |       T       |
                    //|     T      |     F     |       T       |
                    //|     F      |     T     |       F       |
                    //|     F      |     F     |       T       |
                    //------------------------------------------
                    //IsNull condition on a member that is non nullable is an invalid condition
                    if (domainValues.Count <= 0 || (!domainValues.Contains(Constant.Null) && condition.Domain.Values.Contains(Constant.Null)))
                    {
                        string message = System.Data.Entity.Strings.ViewGen_InvalidCondition(memberPath.PathToString(false));
                        ErrorLog.Record record = new ErrorLog.Record(true, ViewGenErrorCode.InvalidCondition, message, cell, String.Empty);
                        ExceptionHelpers.ThrowMappingException(record, config);
                    }
                    if (memberPath.IsAlwaysDefined(inheritanceGraph) == false)
                    {
                        domainValues.Add(Constant.Undefined);
                    }

                    AddToDomainMap(memberPath, domainValues);
                }
            }

            // Fill up the domains for the remaining slots as well
            m_nonConditionDomainMap = new Dictionary<MemberPath, CellConstantSet>(MemberPath.EqualityComparer);
            foreach (Cell cell in extentCells)
            {
                CellQuery cellQuery = cell.GetLeftQuery(viewTarget);
                // Get the atoms from cellQuery and only keep the ones that
                // are condition members
                foreach (MemberProjectedSlot slot in cellQuery.GetAllQuerySlots())
                {
                    MemberPath member = slot.MemberPath;
                    if (m_conditionDomainMap.ContainsKey(member) == false && m_nonConditionDomainMap.ContainsKey(member) == false)
                    {
                        CellConstantSet memberSet = Domain.DeriveDomainFromMemberPath(member, m_edmItemCollection, true /* Regardless of validation, leave the domain unbounded because this is not a condition member */);
                        if (member.IsAlwaysDefined(inheritanceGraph) == false)
                        { // nonConditionMember may belong to subclass
                            memberSet.Add(Constant.Undefined);
                        }
                        memberSet = Domain.ExpandNegationsInDomain(memberSet, memberSet);
                        m_nonConditionDomainMap.Add(member, new CellConstantSetInfo(memberSet, slot));
                    }
                }
            }
        }

        #endregion


        #region Properties
        internal bool IsProjectedConditionMember(MemberPath memberPath)
        {
            return m_projectedConditionMembers.Contains(memberPath);
        }

        #endregion

        #region Methods
        // effects: Returns an "open-world" domain, i.e.,
        // one in which not-null constants are used to represent some other value from the domain
        internal MemberDomainMap GetOpenDomain()
        {
            var domainMap = m_conditionDomainMap.ToDictionary(p => p.Key, p => new Set<Constant>(p.Value, Constant.EqualityComparer));
            ExpandDomainsIfNeeded(domainMap);
            return new MemberDomainMap(domainMap, m_nonConditionDomainMap, m_edmItemCollection);
        }

        // effects: Creates a deep copy of MemberDomainMap
        // nonConditionDomainMap is read-only so it is reused without cloning
        internal MemberDomainMap MakeCopy()
        {
            var domainMap = m_conditionDomainMap.ToDictionary(p => p.Key, p => new Set<Constant>(p.Value, Constant.EqualityComparer));
            return new MemberDomainMap(domainMap, m_nonConditionDomainMap, m_edmItemCollection);
        }

        // effects: Adds negated constants to the possible set of values if none exists in that set.
        // Needed so that we can handle cases when discriminator in the store as P, C but could have other values
        // as well.
        internal void ExpandDomainsToIncludeAllPossibleValues()
        {
            ExpandDomainsIfNeeded(m_conditionDomainMap);
        }

        private void ExpandDomainsIfNeeded(Dictionary<MemberPath, CellConstantSet> domainMapForMembers)
        {
            // For the S-side, we always says that NOT(...) is
            // present. For example, if we are told "C", "P", we assume
            // that NOT(C, P) is possibly present in that column
            foreach (MemberPath path in domainMapForMembers.Keys)
            {
                CellConstantSet possibleValues = domainMapForMembers[path];
                if (path.IsScalarType() &&
                    possibleValues.Any(c => c is NegatedConstant) == false)
                {
                    if (MetadataHelper.HasDiscreteDomain(path.EdmType))
                    {
                        // for a discrete domain, add all values that are not currently represented
                        // in the domain
                        Set<Constant> completeDomain = Domain.DeriveDomainFromMemberPath(path, m_edmItemCollection, true /* leaveDomainUnbounded */);
                        possibleValues.Unite(completeDomain);
                    }
                    else
                    {
                        // for a non-discrete domain, add NOT("C", "P")
                        NegatedConstant negatedConstant = new NegatedConstant(possibleValues);
                        possibleValues.Add(negatedConstant);
                    }
                }
            }
        }

        // effects: Shrinks the domain of members whose types can be enumerated - currently it applies 
        // only to boolean type as for enums we don't restrict enum values to specified members only. 
        // For example NOT(False, True, Null) for a boolean domain should be removed
        internal void ReduceEnumerableDomainToEnumeratedValues(ViewTarget target, ConfigViewGenerator config)
        {
            // Go through the two maps

            ReduceEnumerableDomainToEnumeratedValues(target, m_conditionDomainMap, config, m_edmItemCollection);
            ReduceEnumerableDomainToEnumeratedValues(target, m_nonConditionDomainMap, config, m_edmItemCollection);
        }

        // effects: Fixes the domains of variables in this as specified in FixEnumerableDomains
        private static void ReduceEnumerableDomainToEnumeratedValues(ViewTarget target, Dictionary<MemberPath, CellConstantSet> domainMap, ConfigViewGenerator config,
                                                      EdmItemCollection edmItemCollection)
        {
            foreach (MemberPath member in domainMap.Keys)
            {
                if (MetadataHelper.HasDiscreteDomain(member.EdmType) == false)
                {
                    continue;
                }
                CellConstantSet domain = Domain.DeriveDomainFromMemberPath(member, edmItemCollection, true /* leaveDomainUnbounded */);
                CellConstantSet extra = domainMap[member].Difference(domain);
                extra.Remove(Constant.Undefined);
                if (extra.Count > 0)
                { // domainMap has extra members -- we should get rid of them
                    if (config.IsNormalTracing)
                    {
                        Helpers.FormatTraceLine("Changed domain of {0} from {1} - subtract {2}", member, domainMap[member], extra);
                    }
                    domainMap[member].Subtract(extra);
                }
            }
        }

        // requires: this domainMap has been created for the C-side
        // effects: Fixes the mergedDomain map in this by merging entries
        // available in updateDomainMap
        internal static void PropagateUpdateDomainToQueryDomain(IEnumerable<Cell> cells, MemberDomainMap queryDomainMap, MemberDomainMap updateDomainMap)
        {

            foreach (Cell cell in cells)
            {
                CellQuery cQuery = cell.CQuery;
                CellQuery sQuery = cell.SQuery;

                for (int i = 0; i < cQuery.NumProjectedSlots; i++)
                {
                    MemberProjectedSlot cSlot = cQuery.ProjectedSlotAt(i) as MemberProjectedSlot;
                    MemberProjectedSlot sSlot = sQuery.ProjectedSlotAt(i) as MemberProjectedSlot;

                    if (cSlot == null || sSlot == null)
                    {
                        continue;
                    }

                    // Get the domain for sSlot and merge with cSlot's
                    MemberPath cPath = cSlot.MemberPath;
                    MemberPath sPath = sSlot.MemberPath;
                    CellConstantSet cDomain = queryDomainMap.GetDomainInternal(cPath);
                    CellConstantSet sDomain = updateDomainMap.GetDomainInternal(sPath);

                    // skip NULL because if c-side member is nullable, it's already there, and otherwise can't be taken
                    // skip negated because negated values are translated in a special way
                    cDomain.Unite(sDomain.Where(constant => !constant.IsNull() && !(constant is NegatedConstant)));

                    if (updateDomainMap.IsConditionMember(sPath) && !queryDomainMap.IsConditionMember(cPath))
                    {
                        // record this member so KB knows we have to generate constraints for it
                        queryDomainMap.m_projectedConditionMembers.Add(cPath);
                    }
                }
            }

            ExpandNegationsInDomainMap(queryDomainMap.m_conditionDomainMap);
            ExpandNegationsInDomainMap(queryDomainMap.m_nonConditionDomainMap);
        }

        private static void ExpandNegationsInDomainMap(Dictionary<MemberPath, Set<Constant>> domainMap)
        {
            foreach (var path in domainMap.Keys.ToArray())
            {
                domainMap[path] = Domain.ExpandNegationsInDomain(domainMap[path]);
            }
        }

        internal bool IsConditionMember(MemberPath path)
        {
            return m_conditionDomainMap.ContainsKey(path);
        }

        internal IEnumerable<MemberPath> ConditionMembers(EntitySetBase extent)
        {
            foreach (MemberPath path in m_conditionDomainMap.Keys)
            {
                if (path.Extent.Equals(extent))
                {
                    yield return path;
                }
            }
        }


        internal IEnumerable<MemberPath> NonConditionMembers(EntitySetBase extent)
        {
            foreach (MemberPath path in m_nonConditionDomainMap.Keys)
            {
                if (path.Extent.Equals(extent))
                {
                    yield return path;
                }
            }
        }


        /// <summary>
        /// Adds AllOtherConstants element to the domain set given by MemberPath
        /// </summary>
        internal void AddSentinel(MemberPath path)
        {
            CellConstantSet set = GetDomainInternal(path);
            set.Add(Constant.AllOtherConstants);
        }

        /// <summary>
        /// Removes AllOtherConstant element from the domain set given by MemberPath
        /// </summary>
        internal void RemoveSentinel(MemberPath path)
        {
            CellConstantSet set = GetDomainInternal(path);
            set.Remove(Constant.AllOtherConstants);
        }

        // requires member exist in this
        // effects: Returns the possible values/domain for that member
        internal IEnumerable<Constant> GetDomain(MemberPath path)
        {
            return GetDomainInternal(path);
        }

        private CellConstantSet GetDomainInternal(MemberPath path)
        {
            CellConstantSet result;
            bool found = m_conditionDomainMap.TryGetValue(path, out result);
            if (!found)
            {
                result = m_nonConditionDomainMap[path];  // It better be in this one!
            }
            return result;
        }

        // keeps the same set identity for the updated cell constant domain
        internal void UpdateConditionMemberDomain(MemberPath path, IEnumerable<Constant> domainValues)
        {
            // update domainMap
            Set<Constant> oldDomain = m_conditionDomainMap[path];
            oldDomain.Clear();
            oldDomain.Unite(domainValues);
        }

        // effects: For member, adds domainValues as the set of values that
        // member can take. Merges them with any existing values if present
        private void AddToDomainMap(MemberPath member, IEnumerable<Constant> domainValues)
        {
            CellConstantSet possibleValues;
            if (false == m_conditionDomainMap.TryGetValue(member, out possibleValues))
            {
                possibleValues = new CellConstantSet(Constant.EqualityComparer);
            }
            possibleValues.Unite(domainValues);
            // Add the normalized domain to the map so that later uses of the
            // domain are consistent
            m_conditionDomainMap[member] = Domain.ExpandNegationsInDomain(possibleValues, possibleValues);
        }

        internal override void ToCompactString(StringBuilder builder)
        {
            foreach (MemberPath memberPath in m_conditionDomainMap.Keys)
            {
                builder.Append('(');
                memberPath.ToCompactString(builder);
                IEnumerable<Constant> domain = GetDomain(memberPath);
                builder.Append(": ");
                StringUtil.ToCommaSeparatedStringSorted(builder, domain);
                builder.Append(") ");
            }
        }
        #endregion

        // struct to keep track of the constant set for a particular slot
        private class CellConstantSetInfo : CellConstantSet
        {
            internal CellConstantSetInfo(Set<Constant> iconstants, MemberProjectedSlot islot)
                : base(iconstants)
            {
                slot = islot;
            }

            internal MemberProjectedSlot slot;

            public override string ToString()
            {
                return base.ToString();
            }
        }
    }
}
