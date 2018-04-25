//---------------------------------------------------------------------
// <copyright file="ErrorPatternMatcher.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System.Collections.Generic;
using System.Data.Common.Utils;
using System.Data.Entity;
using System.Data.Mapping.ViewGeneration.QueryRewriting;
using System.Data.Mapping.ViewGeneration.Structures;
using System.Data.Mapping.ViewGeneration.Utils;
using System.Data.Metadata.Edm;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace System.Data.Mapping.ViewGeneration.Validation
{
    using CompositeCondition = Dictionary<MemberPath, Set<Constant>>;
    delegate bool LCWComparer(FragmentQuery query1, FragmentQuery query2);

    internal class ErrorPatternMatcher
    {

        private ViewgenContext m_viewgenContext;
        private MemberDomainMap m_domainMap;
        private IEnumerable<MemberPath> m_keyAttributes;
        private ErrorLog m_errorLog;
        private int m_originalErrorCount;
        private const int NUM_PARTITION_ERR_TO_FIND = 5;


        #region Constructor
        private ErrorPatternMatcher(ViewgenContext context, MemberDomainMap domainMap, ErrorLog errorLog)
        {
            m_viewgenContext = context;
            m_domainMap = domainMap;
            m_keyAttributes = MemberPath.GetKeyMembers(context.Extent, domainMap);
            m_errorLog = errorLog;
            m_originalErrorCount = m_errorLog.Count;
        }

        public static bool FindMappingErrors(ViewgenContext context, MemberDomainMap domainMap, ErrorLog errorLog)
        {
            //Can't get here if Update Views have validation disabled
            Debug.Assert(context.ViewTarget == ViewTarget.QueryView || context.Config.IsValidationEnabled);

            if (context.ViewTarget == ViewTarget.QueryView && !context.Config.IsValidationEnabled)
            {
                return false; // Rules for QV under no validation are different
            }

            ErrorPatternMatcher matcher = new ErrorPatternMatcher(context, domainMap, errorLog);

            matcher.MatchMissingMappingErrors();
            matcher.MatchConditionErrors();
            matcher.MatchSplitErrors();

            if (matcher.m_errorLog.Count == matcher.m_originalErrorCount)
            {   //this will generate redundant errors if one of the above routine finds an error
                // so execute it only when we dont have any other errors
                matcher.MatchPartitionErrors();
            }

            if (matcher.m_errorLog.Count > matcher.m_originalErrorCount)
            {
                ExceptionHelpers.ThrowMappingException(matcher.m_errorLog, matcher.m_viewgenContext.Config);
            }

            return false;
        }
        #endregion

        #region Error Matching Routines


        /// <summary>
        /// Finds Types (possibly without any members) that have no mapping specified
        /// </summary>
        private void MatchMissingMappingErrors()
        {
            if (m_viewgenContext.ViewTarget == ViewTarget.QueryView)
            {
                //Find all types for the given EntitySet
                Set<EdmType> unmapepdTypesInExtent = new Set<EdmType>(MetadataHelper.GetTypeAndSubtypesOf(m_viewgenContext.Extent.ElementType, m_viewgenContext.EdmItemCollection, false /*isAbstract*/));

                //Figure out which type has no Cell mapped to it
                foreach (var fragment in m_viewgenContext.AllWrappersForExtent)
                {
                    foreach (Cell cell in fragment.Cells)
                    {
                        foreach (var restriction in cell.CQuery.Conditions)
                        {
                            foreach (var cellConst in restriction.Domain.Values)
                            {
                                //if there is a mapping to this type...
                                TypeConstant typeConst = cellConst as TypeConstant;
                                if (typeConst != null)
                                {
                                    unmapepdTypesInExtent.Remove(typeConst.EdmType);
                                }
                            }
                        }
                    }
                }

                //We are left with a type that has no mapping
                if (unmapepdTypesInExtent.Count > 0)
                {
                    //error unmapped type
                    m_errorLog.AddEntry(new ErrorLog.Record(true, ViewGenErrorCode.ErrorPatternMissingMappingError,
                            Strings.ViewGen_Missing_Type_Mapping(BuildCommaSeparatedErrorString<EdmType>(unmapepdTypesInExtent)), m_viewgenContext.AllWrappersForExtent, ""));
                }
            }
        }

        private static bool HasNotNullCondition(CellQuery cellQuery, MemberPath member)
        {
            foreach (MemberRestriction condition in cellQuery.GetConjunctsFromWhereClause())
            {
                if (condition.RestrictedMemberSlot.MemberPath.Equals(member))
                {
                    if (condition.Domain.Values.Contains(Constant.NotNull))
                    {
                        return true;
                    }

                    //Not Null may have been optimized into NOT(1, 2, NULL). SO look into negated cell constants
                    foreach (NegatedConstant negatedConst in condition.Domain.Values.Select(cellConstant => cellConstant as NegatedConstant).Where(negated => negated != null))
                    {
                        if (negatedConst.Elements.Contains(Constant.Null))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private static bool IsMemberPartOfNotNullCondition(IEnumerable<LeftCellWrapper> wrappers, MemberPath leftMember, ViewTarget viewTarget)
        {

            foreach (var leftCellWrapper in wrappers)
            {
                CellQuery leftCellQuery = leftCellWrapper.OnlyInputCell.GetLeftQuery(viewTarget);

                if (HasNotNullCondition(leftCellQuery, leftMember))
                {
                    return true;
                }

                //Now figure out corresponding right side MemberPath
                CellQuery rightCellQuery = leftCellWrapper.OnlyInputCell.GetRightQuery(viewTarget);
                int indexOfMemberInProjection = leftCellQuery.GetProjectedMembers().TakeWhile(path => !path.Equals(leftMember)).Count();

                //Member with condition is projected, so check opposite CellQuery's condition
                if (indexOfMemberInProjection < leftCellQuery.GetProjectedMembers().Count())
                {
                    MemberPath rightmember = ((MemberProjectedSlot)rightCellQuery.ProjectedSlotAt(indexOfMemberInProjection)).MemberPath;

                    if (HasNotNullCondition(rightCellQuery, rightmember))
                    {
                        return true;
                    }
                }

            }
            return false;
        }

        /// <summary>
        /// Finds errors related to splitting Conditions
        /// 1. Condition value is repeated across multiple types
        /// 2. A Column/attribute is mapped but also used as a condition
        /// </summary>
        private void MatchConditionErrors()
        {
            List<LeftCellWrapper> leftCellWrappers = m_viewgenContext.AllWrappersForExtent;

            //Stores violating Discriminator (condition member) so that we dont repeat the same error
            Set<MemberPath> mappedConditionMembers = new Set<MemberPath>();

            //Both of these data-structs help in finding duplicate conditions
            Set<CompositeCondition> setOfconditions = new Set<CompositeCondition>(new ConditionComparer());
            Dictionary<CompositeCondition, LeftCellWrapper> firstLCWForCondition = new Dictionary<CompositeCondition, LeftCellWrapper>(new ConditionComparer());

            foreach (var leftCellWrapper in leftCellWrappers)
            {
                CompositeCondition condMembersValues = new CompositeCondition();

                CellQuery cellQuery = leftCellWrapper.OnlyInputCell.GetLeftQuery(m_viewgenContext.ViewTarget);

                foreach (MemberRestriction condition in cellQuery.GetConjunctsFromWhereClause())
                {
                    MemberPath memberPath = condition.RestrictedMemberSlot.MemberPath;

                    if (!m_domainMap.IsConditionMember(memberPath))
                    {
                        continue;
                    }

                    ScalarRestriction scalarCond = condition as ScalarRestriction;
                    //Check for mapping of Scalar member condition, ignore type conditions
                    if (scalarCond != null &&
                        !mappedConditionMembers.Contains(memberPath) && /* prevents duplicate errors */
                        !leftCellWrapper.OnlyInputCell.CQuery.WhereClause.Equals(leftCellWrapper.OnlyInputCell.SQuery.WhereClause) && /* projection allowed when both conditions are equal */
                        !IsMemberPartOfNotNullCondition(leftCellWrappers, memberPath, m_viewgenContext.ViewTarget))
                    {
                        //This member should not be mapped
                        CheckThatConditionMemberIsNotMapped(memberPath, leftCellWrappers, mappedConditionMembers);
                    }

                    //If a not-null condition is specified on a nullable column,
                    //check that the property it is mapped to in the fragment is non-nullable,
                    //unless there is a not null condition on the property that is being mapped it self.
                    //Otherwise return an error.
                    if (m_viewgenContext.ViewTarget == ViewTarget.UpdateView)
                    {
                        if (scalarCond != null &&
                            memberPath.IsNullable && IsMemberPartOfNotNullCondition(new LeftCellWrapper[] { leftCellWrapper }, memberPath, m_viewgenContext.ViewTarget))                        
                        {
                            MemberPath rightMemberPath = GetRightMemberPath(memberPath, leftCellWrapper);
                            if (rightMemberPath != null && rightMemberPath.IsNullable &&
                                !IsMemberPartOfNotNullCondition(new LeftCellWrapper[] { leftCellWrapper }, rightMemberPath, m_viewgenContext.ViewTarget))
                            {
                                m_errorLog.AddEntry(new ErrorLog.Record(true, ViewGenErrorCode.ErrorPatternConditionError,
                                        Strings.Viewgen_ErrorPattern_NotNullConditionMappedToNullableMember(
                                                memberPath, rightMemberPath
                                            ), leftCellWrapper.OnlyInputCell, ""));
                            }
                        }
                    }

                    //CheckForDuplicateConditionValue
                    //discover a composite condition of the form {path1=x, path2=y, ...}
                    foreach (var element in condition.Domain.Values)
                    {
                        Set<Constant> values;
                        //if not in the dict, add it
                        if (!condMembersValues.TryGetValue(memberPath, out values))
                        {
                            values = new Set<Constant>(Constant.EqualityComparer);
                            condMembersValues.Add(memberPath, values);
                        }
                        values.Add(element);
                    }

                } //foreach condition

                if (condMembersValues.Count > 0) //it is possible that there are no condition members
                {
                    //Check if the composite condition has been encountered before
                    if (setOfconditions.Contains(condMembersValues))
                    {
                        //Extents may be Equal on right side (e.g: by some form of Refconstraint)
                        if (!RightSideEqual(firstLCWForCondition[condMembersValues], leftCellWrapper))
                        {
                            //error duplicate conditions
                            m_errorLog.AddEntry(new ErrorLog.Record(true, ViewGenErrorCode.ErrorPatternConditionError,
                                    Strings.Viewgen_ErrorPattern_DuplicateConditionValue(
                                            BuildCommaSeparatedErrorString<MemberPath>(condMembersValues.Keys)
                                        ),
                                    ToIEnum(firstLCWForCondition[condMembersValues].OnlyInputCell, leftCellWrapper.OnlyInputCell), ""));
                        }
                    }
                    else
                    {
                        setOfconditions.Add(condMembersValues);

                        //Remember which cell the condition came from.. used for error reporting
                        firstLCWForCondition.Add(condMembersValues, leftCellWrapper);
                    }
                }
            } //foreach fragment related to the Extent we are working on

        }

        private MemberPath GetRightMemberPath(MemberPath conditionMember,LeftCellWrapper leftCellWrapper)
        {
            CellQuery rightCellQuery = leftCellWrapper.OnlyInputCell.GetRightQuery(ViewTarget.QueryView);
            var projectPositions = rightCellQuery.GetProjectedPositions(conditionMember);
            //Make the case simple. If the member is mapped more than once in the same cell wrapper
            //we are not going try and guess the pattern
            if (projectPositions.Count != 1)
            {
                return null;
            }
            int firstProjectedPosition = projectPositions.First();
            CellQuery leftCellQuery = leftCellWrapper.OnlyInputCell.GetLeftQuery(ViewTarget.QueryView);
            return ((MemberProjectedSlot)leftCellQuery.ProjectedSlotAt(firstProjectedPosition)).MemberPath;
        }

        /// <summary>
        /// When we are dealing with an update view, this method
        /// finds out if the given Table is mapped to different EntitySets
        /// </summary>
        private void MatchSplitErrors()
        {
            List<LeftCellWrapper> leftCellWrappers = m_viewgenContext.AllWrappersForExtent;

            //Check that the given Table is mapped to only one EntitySet (avoid AssociationSets)
            var nonAssociationWrappers = leftCellWrappers.Where(r => !(r.LeftExtent is AssociationSet) && !(r.RightCellQuery.Extent is AssociationSet));

            if (m_viewgenContext.ViewTarget == ViewTarget.UpdateView && nonAssociationWrappers.Any())
            {
                LeftCellWrapper firstLeftCWrapper = nonAssociationWrappers.First();
                EntitySetBase rightExtent = firstLeftCWrapper.RightCellQuery.Extent;

                foreach (var leftCellWrapper in nonAssociationWrappers)
                {
                    //!(leftCellWrapper.RightCellQuery.Extent is AssociationSet) &&
                    if (!leftCellWrapper.RightCellQuery.Extent.EdmEquals(rightExtent))
                    {
                        //A Table may be mapped to two extents but the extents may be Equal (by some form of Refconstraint)
                        if (!RightSideEqual(leftCellWrapper, firstLeftCWrapper))
                        {
                            //Report Error
                            m_errorLog.AddEntry(new ErrorLog.Record(true, ViewGenErrorCode.ErrorPatternSplittingError,
                                Strings.Viewgen_ErrorPattern_TableMappedToMultipleES(leftCellWrapper.LeftExtent.ToString(), leftCellWrapper.RightCellQuery.Extent.ToString(), rightExtent.ToString()),
                                leftCellWrapper.Cells.First(), ""));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Finds out whether fragments (partitions) violate constraints that would produce an invalid mapping.
        /// We compare equality/disjointness/containment for all 2-combinations of fragments.
        /// Error is reported if given relationship on S side is not maintained on the C side.
        /// If we know nothing about S-side then any relationship on C side is valid.
        /// </summary>
        private void MatchPartitionErrors()
        {
            List<LeftCellWrapper> mappingFragments = m_viewgenContext.AllWrappersForExtent;

            //for every 2-combination nC2  (n choose 2) 
            int i = 0;
            foreach (var fragment1 in mappingFragments)
            {
                foreach (var fragment2 in mappingFragments.Skip(++i))
                {
                    FragmentQuery rightFragmentQuery1 = CreateRightFragmentQuery(fragment1);
                    FragmentQuery rightFragmentQuery2 = CreateRightFragmentQuery(fragment2);

                    bool isSDisjoint = CompareS(ComparisonOP.IsDisjointFrom, m_viewgenContext, fragment1, fragment2, rightFragmentQuery1, rightFragmentQuery2);
                    bool isCDisjoint = CompareC(ComparisonOP.IsDisjointFrom, m_viewgenContext, fragment1, fragment2, rightFragmentQuery1, rightFragmentQuery2);

                    bool is1SubsetOf2_C;
                    bool is2SubsetOf1_C;
                    bool is1SubsetOf2_S;
                    bool is2SubsetOf1_S;
                    bool isSEqual;
                    bool isCEqual;

                    if (isSDisjoint)
                    {
                        if (isCDisjoint)
                        {
                            continue;
                        }
                        else
                        {
                            //Figure out more info for accurate message
                            is1SubsetOf2_C = CompareC(ComparisonOP.IsContainedIn, m_viewgenContext, fragment1, fragment2, rightFragmentQuery1, rightFragmentQuery2);
                            is2SubsetOf1_C = CompareC(ComparisonOP.IsContainedIn, m_viewgenContext, fragment2, fragment1, rightFragmentQuery2, rightFragmentQuery1);
                            isCEqual = is1SubsetOf2_C && is2SubsetOf1_C;

                            StringBuilder errorString = new StringBuilder();
                            //error
                            if (isCEqual) //equal
                            {
                                //MSG:  These two fragments are disjoint on the S-side but equal on the C-side. 
                                //      Ensure disjointness on C-side by mapping them to different types within the same EntitySet
                                //      or by mapping them to the same type but with a C-side discriminator.
                                //TestCase (1)
                                errorString.Append(Strings.Viewgen_ErrorPattern_Partition_Disj_Eq);
                            }
                            else if (is1SubsetOf2_C || is2SubsetOf1_C)
                            {
                                //Really overlap is not accurate term (should be contianed in or subset of), but its easiest to read.

                                if (CSideHasDifferentEntitySets(fragment1, fragment2))
                                {
                                    //MSG:  These two fragments are disjoint on the S-side but overlap on the C-side via a Referential constraint.
                                    //      Ensure disjointness on C-side by mapping them to different types within the same EntitySet
                                    //      or by mapping them to the same type but with a C-side discriminator.

                                    //TestCase (Not possible because all PKs must be mapped)
                                    errorString.Append(Strings.Viewgen_ErrorPattern_Partition_Disj_Subs_Ref);
                                }
                                else
                                {
                                    //MSG:  These two fragments are disjoint on the S-side but overlap on the C-side. 
                                    //      Ensure disjointness on C-side. You may be using IsTypeOf() quantifier to 
                                    //      map multiple types within one of these fragments.
                                    //TestCase (2)
                                    errorString.Append(Strings.Viewgen_ErrorPattern_Partition_Disj_Subs);
                                }
                            }
                            else //relationship is unknown
                            {
                                //MSG:  These two fragments are disjoint on the S-side but not so on the C-side. 
                                //      Ensure disjointness on C-side by mapping them to different types within the same EntitySet
                                //      or by mapping them to the same type but with a C-side discriminator.

                                //TestCase (4)
                                errorString.Append(Strings.Viewgen_ErrorPattern_Partition_Disj_Unk);
                            }

                            m_errorLog.AddEntry(new ErrorLog.Record(true, ViewGenErrorCode.ErrorPatternInvalidPartitionError, errorString.ToString(), ToIEnum(fragment1.OnlyInputCell, fragment2.OnlyInputCell), ""));

                            if (FoundTooManyErrors())
                            {
                                return;
                            }
                        }
                    }
                    else
                    {
                        is1SubsetOf2_C = CompareC(ComparisonOP.IsContainedIn, m_viewgenContext, fragment1, fragment2, rightFragmentQuery1, rightFragmentQuery2);
                        is2SubsetOf1_C = CompareC(ComparisonOP.IsContainedIn, m_viewgenContext, fragment2, fragment1, rightFragmentQuery2, rightFragmentQuery1);
                    }
                    is1SubsetOf2_S = CompareS(ComparisonOP.IsContainedIn, m_viewgenContext, fragment1, fragment2, rightFragmentQuery1, rightFragmentQuery2);
                    is2SubsetOf1_S = CompareS(ComparisonOP.IsContainedIn, m_viewgenContext, fragment2, fragment1, rightFragmentQuery2, rightFragmentQuery1);

                    isCEqual = is1SubsetOf2_C && is2SubsetOf1_C;
                    isSEqual = is1SubsetOf2_S && is2SubsetOf1_S;


                    if (isSEqual)
                    {
                        if (isCEqual) //c-side equal
                        {
                            continue;
                        }
                        else
                        {
                            //error
                            StringBuilder errorString = new StringBuilder();

                            if (isCDisjoint)
                            {
                                //MSG:  These two fragments are equal on the S-side but disjoint on the C-side. 
                                //      Either partition the S-side by adding a condition or remove any C-side conditions along with resulting redundant mapping fragments.
                                //      You may also map these two disjoint C-side partitions to different tables.
                                //TestCase (5)
                                errorString.Append(Strings.Viewgen_ErrorPattern_Partition_Eq_Disj);
                            }
                            else if (is1SubsetOf2_C || is2SubsetOf1_C)
                            {
                                if (CSideHasDifferentEntitySets(fragment1, fragment2))
                                {
                                    //MSG:  These two fragments are equal on the S-side but overlap on the C-side. 
                                    //      It is likely that you have not added Referential Integrity constriaint for all Key attributes of both EntitySets.
                                    //      Doing so would ensure equality on the C-side.
                                    //TestCase (Not possible, right?)
                                    errorString.Append(Strings.Viewgen_ErrorPattern_Partition_Eq_Subs_Ref);
                                }
                                else
                                {
                                    //MSG:  These two fragments are equal on the S-side but overlap on the C-side. 
                                    //      If you are using IsTypeOf() quantifier ensure both mapping fragments capture same types on the C-side.
                                    //      Otherwise you may have intended to partition the S-side.
                                    //TestCase (6)

                                    //Check for the specific case
                                    //where there are mapping fragments with different types on C side
                                    //mapped to same table on the Store side but not all the fragments have 
                                    //a condition. Ignore the cases where any of the fragments have C side conditions.
                                    if (fragment1.LeftExtent.Equals(fragment2.LeftExtent))
                                    {
                                        bool firstCellWrapperHasCondition;
                                        List<EdmType> edmTypesForFirstCellWrapper;
                                        bool secondCellWrapperHasCondition;
                                        List<EdmType> edmTypesForSecondCellWrapper;
                                        GetTypesAndConditionForWrapper(fragment1, out firstCellWrapperHasCondition, out edmTypesForFirstCellWrapper);
                                        GetTypesAndConditionForWrapper(fragment2, out secondCellWrapperHasCondition, out edmTypesForSecondCellWrapper);
                                        if (!firstCellWrapperHasCondition && !secondCellWrapperHasCondition)
                                        {
                                            if (((edmTypesForFirstCellWrapper.Except(edmTypesForSecondCellWrapper)).Count() != 0 )
                                                || ((edmTypesForSecondCellWrapper.Except(edmTypesForFirstCellWrapper)).Count() != 0 ))
                                            {
                                                if (!CheckForStoreConditions(fragment1) || !CheckForStoreConditions(fragment2))
                                                {
                                                    IEnumerable<string> edmTypesForErrorString = edmTypesForFirstCellWrapper.Select(it => it.FullName).Union(edmTypesForSecondCellWrapper.Select(it => it.FullName));
                                                    m_errorLog.AddEntry(new ErrorLog.Record(true, ViewGenErrorCode.ErrorPatternConditionError,
                                                Strings.Viewgen_ErrorPattern_Partition_MultipleTypesMappedToSameTable_WithoutCondition(
                                                        StringUtil.ToCommaSeparatedString(edmTypesForErrorString), fragment1.LeftExtent
                                                    ), ToIEnum(fragment1.OnlyInputCell, fragment2.OnlyInputCell), ""));
                                                    return;
                                                }
                                            }
                                        }
                                    }

                                    errorString.Append(Strings.Viewgen_ErrorPattern_Partition_Eq_Subs);
                                }
                            }
                            else //unknown
                            {
                                //S-side equal, C-side Unknown
                                if (!IsQueryView() &&
                                    (fragment1.OnlyInputCell.CQuery.Extent is AssociationSet ||
                                     fragment2.OnlyInputCell.CQuery.Extent is AssociationSet))
                                {
                                    //one side is an association set
                                    errorString.Append(Strings.Viewgen_ErrorPattern_Partition_Eq_Unk_Association);
                                }
                                else
                                {

                                    //MSG:  These two fragments are equal on the S-side but not so on the C-side. 
                                    //      Try adding an Association with Referntial Integrity constraint if they are
                                    //      mapped to different EntitySets in order to make theme equal on the C-side.
                                    //TestCase (no need, Table mapped to multiple ES tests cover this scenario)
                                    errorString.Append(Strings.Viewgen_ErrorPattern_Partition_Eq_Unk);
                                }
                            }

                            m_errorLog.AddEntry(new ErrorLog.Record(true, ViewGenErrorCode.ErrorPatternInvalidPartitionError, errorString.ToString(), ToIEnum(fragment1.OnlyInputCell, fragment2.OnlyInputCell), ""));

                            if (FoundTooManyErrors())
                            {
                                return;
                            }
                        }
                    }
                    else if (is1SubsetOf2_S || is2SubsetOf1_S) //proper subset - note: else if ensures inverse need not be checked
                    {
                        //C-side proper subset (c side must not be equal)
                        if ((is1SubsetOf2_S && is1SubsetOf2_C == true && !(is2SubsetOf1_C == true)) || (is2SubsetOf1_S && is2SubsetOf1_C == true && !(is1SubsetOf2_C == true)))
                        {
                            continue;
                        }
                        else
                        {   //error

                            StringBuilder errorString = new StringBuilder();

                            if (isCDisjoint)
                            {
                                //MSG:  One of the fragments is a subset of the other on the S-side but they are disjoint on the C-side.
                                //      If you intended overlap on the S-side ensure they have similar relationship on teh C-side.
                                //      You may need to use IsTypeOf() quantifier or loosen conditions in one of the fragments.
                                //TestCase (9, 10)
                                errorString.Append(Strings.Viewgen_ErrorPattern_Partition_Sub_Disj);
                            }
                            else if (isCEqual) //equal
                            {
                                //MSG:  One of the fragments is a subset of the other on the S-side but they are equal on the C-side.
                                //      If you intended overlap on the S-side ensure they have similar relationship on teh C-side.
                                //TestCase (10)


                                if (CSideHasDifferentEntitySets(fragment1, fragment2))
                                {
                                    // If they are equal via a Referential integrity constraint try making one a subset of the other by
                                    // not including all primary keys in the constraint.
                                    //TestCase (Not possible)
                                    errorString.Append(" " + Strings.Viewgen_ErrorPattern_Partition_Sub_Eq_Ref);
                                }
                                else
                                {
                                    //      You may need to modify conditions in one of the fragments.
                                    //TestCase (10)
                                    errorString.Append(Strings.Viewgen_ErrorPattern_Partition_Sub_Eq);
                                }
                            }
                            else
                            {   //unknown
                                //MSG:  One of the fragments is a subset of the other on the S-side but they are disjoint on the C-side.
                                //      If you intended overlap on the S-side ensure they have similar relationship on teh C-side.
                                //TestCase (no need, Table mapped to multiple ES tests cover this scenario)
                                errorString.Append(Strings.Viewgen_ErrorPattern_Partition_Sub_Unk);
                            }

                            m_errorLog.AddEntry(new ErrorLog.Record(true, ViewGenErrorCode.ErrorPatternInvalidPartitionError, errorString.ToString(), ToIEnum(fragment1.OnlyInputCell, fragment2.OnlyInputCell), ""));

                            if (FoundTooManyErrors())
                            {
                                return;
                            }
                        }
                    }
                    //else unknown relationship on the S-side
                }
            }   //end looping over every 2-combination of fragment
        }

        /// <summary>
        /// Gets the types on the Edm side mapped in this fragment wrapper.
        /// It also returns an out parameter indicating whether there were any C side conditions.
        /// </summary>
        private void GetTypesAndConditionForWrapper(LeftCellWrapper wrapper, out bool hasCondition, out List<EdmType> edmTypes)
        {
            hasCondition = false;
            edmTypes = new List<EdmType>();
            //Figure out which type has no Cell mapped to it
            foreach (Cell cell in wrapper.Cells)
            {
                foreach (var restriction in cell.CQuery.Conditions)
                {
                    foreach (var cellConst in restriction.Domain.Values)
                    {
                        //if there is a mapping to this type...
                        TypeConstant typeConst = cellConst as TypeConstant;
                        if (typeConst != null)
                        {
                            edmTypes.Add(typeConst.EdmType);
                        }
                        else
                        {
                            hasCondition = true;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Return true if there were any Store conditions on this cell wrapper.
        /// </summary>
        /// <param name="wrapper"></param>
        /// <returns></returns>
        private bool CheckForStoreConditions(LeftCellWrapper wrapper)
        {
            return wrapper.Cells.SelectMany(c => c.SQuery.Conditions).Any();
        }


        private void CheckThatConditionMemberIsNotMapped(MemberPath conditionMember, List<LeftCellWrapper> mappingFragments, Set<MemberPath> mappedConditionMembers)
        {

            //Make sure memberPath is not mapped (in any other cells)
            foreach (var anotherFragment in mappingFragments)
            {
                foreach (var anotherCell in anotherFragment.Cells)
                {
                    CellQuery anotherCellQuery = anotherCell.GetLeftQuery(m_viewgenContext.ViewTarget);
                    if (anotherCellQuery.GetProjectedMembers().Contains(conditionMember))
                    {
                        mappedConditionMembers.Add(conditionMember);
                        //error condition memer is projected somewhere
                        m_errorLog.AddEntry(new ErrorLog.Record(true, ViewGenErrorCode.ErrorPatternConditionError, Strings.Viewgen_ErrorPattern_ConditionMemberIsMapped(conditionMember.ToString()), anotherCell, ""));
                    }
                }
            }
        }

        #endregion

        private bool FoundTooManyErrors()
        {
            return (m_errorLog.Count > m_originalErrorCount + NUM_PARTITION_ERR_TO_FIND);
        }

        #region Private Helpers

        private string BuildCommaSeparatedErrorString<T>(IEnumerable<T> members)
        {
            StringBuilder builder = new StringBuilder();

            var firstMember = members.First();
            foreach (var member in members)
            {
                if (!member.Equals(firstMember))
                {
                    builder.Append(", ");
                }
                builder.Append("'" + member.ToString() + "'");
            }
            return builder.ToString();
        }

        private bool CSideHasDifferentEntitySets(LeftCellWrapper a, LeftCellWrapper b)
        {
            if (IsQueryView())
            {
                return a.LeftExtent == b.LeftExtent;
            }
            else
            {
                return a.RightCellQuery == b.RightCellQuery;
            }
        }

        private bool CompareC(ComparisonOP op, ViewgenContext context, LeftCellWrapper leftWrapper1, LeftCellWrapper leftWrapper2, FragmentQuery rightQuery1, FragmentQuery rightQuery2)
        {
            return Compare(true /*lookingForCSide*/, op, context, leftWrapper1, leftWrapper2, rightQuery1, rightQuery2);
        }

        private bool CompareS(ComparisonOP op, ViewgenContext context, LeftCellWrapper leftWrapper1, LeftCellWrapper leftWrapper2, FragmentQuery rightQuery1, FragmentQuery rightQuery2)
        {
            return Compare(false/*lookingForCSide*/, op, context, leftWrapper1, leftWrapper2, rightQuery1, rightQuery2);
        }

        private bool Compare(bool lookingForC, ComparisonOP op, ViewgenContext context, LeftCellWrapper leftWrapper1, LeftCellWrapper leftWrapper2, FragmentQuery rightQuery1, FragmentQuery rightQuery2)
        {
            LCWComparer comparer;

            if ((lookingForC && IsQueryView()) || (!lookingForC && !IsQueryView()))
            {
                if (op == ComparisonOP.IsContainedIn)
                {
                    comparer = context.LeftFragmentQP.IsContainedIn;
                }
                else if (op == ComparisonOP.IsDisjointFrom)
                {
                    comparer = context.LeftFragmentQP.IsDisjointFrom;
                }
                else
                {
                    Debug.Fail("Unexpected comparison operator, only IsDisjointFrom and IsContainedIn are expected");
                    return false;
                }

                return comparer(leftWrapper1.FragmentQuery, leftWrapper2.FragmentQuery);
            }
            else
            {
                if (op == ComparisonOP.IsContainedIn)
                {
                    comparer = context.RightFragmentQP.IsContainedIn;
                }
                else if (op == ComparisonOP.IsDisjointFrom)
                {
                    comparer = context.RightFragmentQP.IsDisjointFrom;
                }
                else
                {
                    Debug.Fail("Unexpected comparison operator, only IsDisjointFrom and IsContainedIn are expected");
                    return false;
                }

                return comparer(rightQuery1, rightQuery2);
            }
        }

        private bool RightSideEqual(LeftCellWrapper wrapper1, LeftCellWrapper wrapper2)
        {
            FragmentQuery rightFragmentQuery1 = CreateRightFragmentQuery(wrapper1);
            FragmentQuery rightFragmentQuery2 = CreateRightFragmentQuery(wrapper2);

            return m_viewgenContext.RightFragmentQP.IsEquivalentTo(rightFragmentQuery1, rightFragmentQuery2);
        }

        private FragmentQuery CreateRightFragmentQuery(LeftCellWrapper wrapper)
        {
            return FragmentQuery.Create(wrapper.OnlyInputCell.CellLabel.ToString(), wrapper.CreateRoleBoolean(), wrapper.OnlyInputCell.GetRightQuery(m_viewgenContext.ViewTarget));
        }

        private IEnumerable<Cell> ToIEnum(Cell one, Cell two)
        {
            List<Cell> cells = new List<Cell>();
            cells.Add(one);
            cells.Add(two);
            return cells;
        }

        private bool IsQueryView()
        {
            return (m_viewgenContext.ViewTarget == ViewTarget.QueryView);
        }

        #endregion

        enum ComparisonOP
        {
            IsContainedIn,
            IsDisjointFrom
        }
    }

    class ConditionComparer : IEqualityComparer<Dictionary<MemberPath, Set<Constant>>>
    {
        public bool Equals(Dictionary<MemberPath, Set<Constant>> one, Dictionary<MemberPath, Set<Constant>> two)
        {
            Set<MemberPath> keysOfOne = new Set<MemberPath>(one.Keys, MemberPath.EqualityComparer);
            Set<MemberPath> keysOfTwo = new Set<MemberPath>(two.Keys, MemberPath.EqualityComparer);

            if (!keysOfOne.SetEquals(keysOfTwo))
            {
                return false;
            }

            foreach (var member in keysOfOne)
            {
                Set<Constant> constantsOfOne = one[member];
                Set<Constant> constantsOfTwo = two[member];

                if (!constantsOfOne.SetEquals(constantsOfTwo))
                {
                    return false;
                }
            }
            return true;
        }

        public int GetHashCode(Dictionary<MemberPath, Set<Constant>> obj)
        {
            StringBuilder builder = new StringBuilder();
            foreach (var key in obj.Keys)
            {
                builder.Append(key.ToString());
            }

            return builder.ToString().GetHashCode();
        }

    }
}
