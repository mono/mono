// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.Activities.DynamicUpdate
{
    using System;
    using System.Activities.DynamicUpdate;
    using System.Activities.Validation;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Runtime;
    using System.Runtime.CompilerServices;

    static class ActivityComparer
    {
        public static bool HasPrivateMemberOtherThanArgumentsChanged(DynamicUpdateMapBuilder.NestedIdSpaceFinalizer nestedFinalizer, Activity currentElement, Activity originalElement, bool isMemberOfUpdatedIdSpace, out DynamicUpdateMap argumentChangesMap)
        {
            Fx.Assert(currentElement != null && originalElement != null, "Both activities must be non-null.");
         
            argumentChangesMap = null;
            IdSpace currentPrivateIdSpace = currentElement.ParentOf;
            IdSpace originalPrivateIdSpace = originalElement.ParentOf;

            // for the implementation of an activity in the IdSpace being updated--but not anywhere deeper
            // in the tree--we allow adding, removing or rearranging named private variables.
            // We don't support matching unnamed private variables, and we don't support declaring new
            // default variable expressions. (That would would offset the private IdSpace, which will be caught by the subsquent checks.)
            if ((!isMemberOfUpdatedIdSpace || IsAnyNameless(currentElement.ImplementationVariables) || IsAnyNameless(originalElement.ImplementationVariables)) &&
                !ListEquals(currentElement.ImplementationVariables, originalElement.ImplementationVariables, CompareVariableEquality))
            {
                return true;
            }

            if (currentPrivateIdSpace == null && originalPrivateIdSpace == null)
            {
                return false;
            }
            else if ((currentPrivateIdSpace != null && originalPrivateIdSpace == null) || (currentPrivateIdSpace == null && originalPrivateIdSpace != null))
            {
                return true;
            }            

            if (!ListEquals<ActivityDelegate>(currentElement.ImplementationDelegates, originalElement.ImplementationDelegates, CompareDelegateEquality))
            {
                return true;
            }

            // compare structural equality of members in the private IdSpaces                              
            PrivateIdSpaceMatcher privateIdSpaceMatcher = new PrivateIdSpaceMatcher(nestedFinalizer, originalPrivateIdSpace, currentPrivateIdSpace);
            return !privateIdSpaceMatcher.Match(out argumentChangesMap);
        }

        public static bool ListEquals(IList<RuntimeDelegateArgument> currentArguments, IList<RuntimeDelegateArgument> originalArguments)
        {
            return ListEquals(currentArguments, originalArguments, CompareRuntimeDelegateArgumentEquality);
        }

        public static bool ListEquals(IList<ArgumentInfo> currentArguments, IList<ArgumentInfo> originalArguments)
        {
            return ListEquals(currentArguments, originalArguments, ArgumentInfo.Equals);
        }

        public static bool ListEquals<T>(IList<T> currentMembers, IList<T> originalMembers, Func<T, T, bool> comparer)
        {
            if (currentMembers == null)
            {
                return originalMembers == null || originalMembers.Count == 0;
            }

            if (originalMembers == null)
            {
                return currentMembers.Count == 0;
            }

            if (currentMembers.Count != originalMembers.Count)
            {
                return false;
            }

            if (comparer != null)
            {
                for (int i = 0; i < currentMembers.Count; i++)
                {
                    if (!comparer(currentMembers[i], originalMembers[i]))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public static bool SignatureEquals(Variable leftVar, Variable rightVar)
        {
            return AreVariableNamesIdentical(leftVar.Name, rightVar.Name) && leftVar.Type == rightVar.Type && leftVar.Modifiers == rightVar.Modifiers;
        }

        static bool AreVariableNamesIdentical(string leftName, string rightName)
        {
            if (string.IsNullOrEmpty(leftName) && string.IsNullOrEmpty(rightName))
            {
                return true;
            }

            return leftName == rightName;
        }

        static bool IsAnyNameless(IEnumerable<Variable> variables)
        {
            return variables.Any(v => string.IsNullOrEmpty(v.Name));
        }        

        static IList<Activity> GetDeclaredChildren(IList<Activity> collection, Activity parent)
        {
            return collection.Where(a => a.Parent == parent).ToList();
        }

        static IList<ActivityDelegate> GetDeclaredDelegates(IList<ActivityDelegate> collection, Activity parentActivity)
        {
            return collection.Where(d => d.Owner == parentActivity).ToList();
        }

        static bool CompareChildEquality(Activity currentChild, IdSpace currentIdSpace, Activity originalChild, IdSpace originalIdSpace)
        {
            if (currentChild == null && originalChild == null)
            {
                return true;
            }
            else if ((currentChild == null && originalChild != null) || (currentChild != null && originalChild == null))
            {
                return false;
            }

            if ((currentChild.MemberOf == currentIdSpace && originalChild.MemberOf != originalIdSpace) || (currentChild.MemberOf != currentIdSpace && originalChild.MemberOf == originalIdSpace))
            {
                return false;
            }

            return true;
        }

        static bool CompareDelegateEquality(ActivityDelegate currentDelegate, ActivityDelegate originalDelegate)
        {
            Fx.Assert(currentDelegate != null && originalDelegate != null, "Both currentDelegate and originalDelegate must be non-null.");

            if (!ListEquals(currentDelegate.RuntimeDelegateArguments, originalDelegate.RuntimeDelegateArguments))
            {
                return false;
            }

            bool isImplementation = currentDelegate.ParentCollectionType == ActivityCollectionType.Implementation;
            Fx.Assert(originalDelegate.ParentCollectionType == currentDelegate.ParentCollectionType, "Mismatched delegates");
            IdSpace currentIdSpace = isImplementation ? currentDelegate.Owner.ParentOf : currentDelegate.Owner.MemberOf;
            IdSpace originalIdSpace = isImplementation ? originalDelegate.Owner.ParentOf : originalDelegate.Owner.MemberOf;

            return CompareChildEquality(currentDelegate.Handler, currentIdSpace, originalDelegate.Handler, originalIdSpace);
        }

        static bool CompareVariableEquality(Variable currentVariable, Variable originalVariable)
        {
            Fx.Assert(currentVariable != null && originalVariable != null, "Both currentVariable and originalVariable must be non-null.");

            if (!SignatureEquals(currentVariable, originalVariable))
            {
                return false;
            }

            Fx.Assert(currentVariable.IsPublic == originalVariable.IsPublic, "Mismatched variables");
            IdSpace currentIdSpace = currentVariable.IsPublic ? currentVariable.Owner.MemberOf : currentVariable.Owner.ParentOf;
            IdSpace originalIdSpace = originalVariable.IsPublic ? originalVariable.Owner.MemberOf : originalVariable.Owner.ParentOf;
            return CompareChildEquality(currentVariable.Default, currentIdSpace, originalVariable.Default, originalIdSpace);
        }

        static bool CompareArgumentEquality(RuntimeArgument currentArgument, RuntimeArgument originalArgument)
        {
            Fx.Assert(currentArgument != null && originalArgument != null, "Both currentArgument and originalArgument must be non-null.");

            if (currentArgument.Name != originalArgument.Name || 
                currentArgument.Type != originalArgument.Type ||
                currentArgument.Direction != originalArgument.Direction)
            {
                return false;
            }

            if (currentArgument.BoundArgument == null && originalArgument.BoundArgument == null)
            {
                return true;
            }
            else if ((currentArgument.BoundArgument != null && originalArgument.BoundArgument == null) || (currentArgument.BoundArgument == null && originalArgument.BoundArgument != null))
            {
                return false;
            }

            return CompareChildEquality(currentArgument.BoundArgument.Expression, currentArgument.Owner.MemberOf, originalArgument.BoundArgument.Expression, originalArgument.Owner.MemberOf);
        }

        static bool CompareRuntimeDelegateArgumentEquality(RuntimeDelegateArgument newRuntimeDelegateArgument, RuntimeDelegateArgument oldRuntimeDelegateArgument)
        {
            // compare Name, Type and Direction
            if (!newRuntimeDelegateArgument.Name.Equals(oldRuntimeDelegateArgument.Name) ||
                (newRuntimeDelegateArgument.Type != oldRuntimeDelegateArgument.Type) ||
                (newRuntimeDelegateArgument.Direction != oldRuntimeDelegateArgument.Direction))
            {
                return false;
            }

            return CompareDelegateArgumentEquality(newRuntimeDelegateArgument.BoundArgument, oldRuntimeDelegateArgument.BoundArgument);
        }

        static bool CompareDelegateArgumentEquality(DelegateArgument newBoundArgument, DelegateArgument oldBoundArgument)
        {
            if (newBoundArgument == null)
            {
                return oldBoundArgument == null;
            }
            else if (oldBoundArgument == null)
            {
                return false;
            }

            return (newBoundArgument.Name == oldBoundArgument.Name) &&
                (newBoundArgument.Type == oldBoundArgument.Type) &&
                (newBoundArgument.Direction == oldBoundArgument.Direction);
        }

        // this class helps to determine if anything in the private IdSpace has changed or not.
        // The only exception is addition/removal/rearrangement of RuntimeArguments and their Expressions.
        // Addition or removal of the RuntimeArguments with non-null Expressions will cause Id shift in the private IdSpace.
        // In such case, this PrivateIdSpaceMatcher returns an implementation Map which represents the id shift and RuntimeArguments change.
        class PrivateIdSpaceMatcher
        {
            DynamicUpdateMap privateMap;
            DynamicUpdateMapBuilder.NestedIdSpaceFinalizer nestedFinalizer;            

            IdSpace originalPrivateIdSpace;
            IdSpace updatedPrivateIdSpace;

            // As PrivateIdSpaceMatcher progresses through the IdSpace pair, 
            // only the structurally equal activity pairs that are members of the IdSpaces are enqueued to this queue.
            Queue<Tuple<Activity, Activity>> matchedActivities;

            bool argumentChangeDetected;

            public PrivateIdSpaceMatcher(DynamicUpdateMapBuilder.NestedIdSpaceFinalizer nestedFinalizer, IdSpace originalPrivateIdSpace, IdSpace updatedPrivateIdSpace)
            {
                this.privateMap = new DynamicUpdateMap()
                {
                    IsForImplementation = true,
                    NewDefinitionMemberCount = updatedPrivateIdSpace.MemberCount
                };

                this.nestedFinalizer = nestedFinalizer;
                this.argumentChangeDetected = false;
                
                this.originalPrivateIdSpace = originalPrivateIdSpace;
                this.updatedPrivateIdSpace = updatedPrivateIdSpace;
                
                this.matchedActivities = new Queue<Tuple<Activity, Activity>>();
            }

            public bool Match(out DynamicUpdateMap argumentChangesMap)
            {
                argumentChangesMap = null;
                
                int nextOriginalSubrootId = 0;
                int nextUpdatedSubrootId = 0;
                bool allSubtreeRootsEnqueued = false;

                // enqueue all subtree root pairs first
                while (!allSubtreeRootsEnqueued)
                {
                    nextOriginalSubrootId = GetIndexOfNextSubtreeRoot(this.originalPrivateIdSpace, nextOriginalSubrootId);
                    nextUpdatedSubrootId = GetIndexOfNextSubtreeRoot(this.updatedPrivateIdSpace, nextUpdatedSubrootId);

                    if (nextOriginalSubrootId != -1 && nextUpdatedSubrootId != -1)
                    {
                        // found next disjoint subtree pair to match
                        this.PrepareToMatchSubtree(this.updatedPrivateIdSpace[nextUpdatedSubrootId], this.originalPrivateIdSpace[nextOriginalSubrootId]);
                    }
                    else if (nextOriginalSubrootId == -1 && nextUpdatedSubrootId == -1)
                    {
                        // there are no more subtree root pair to process.
                        allSubtreeRootsEnqueued = true;
                    }
                    else
                    {
                        // something other than Arguments must have changed
                        return false;
                    }
                }                

                while (this.matchedActivities.Count > 0)
                {
                    Tuple<Activity, Activity> pair = this.matchedActivities.Dequeue();
                    Activity originalActivity = pair.Item1;
                    Activity currentActivity = pair.Item2;

                    Fx.Assert(originalActivity.MemberOf == this.originalPrivateIdSpace && currentActivity.MemberOf == this.updatedPrivateIdSpace, "neither activities must be a reference.");

                    if (currentActivity.GetType() != originalActivity.GetType() || currentActivity.RelationshipToParent != originalActivity.RelationshipToParent)
                    {
                        return false;
                    }

                    // there is no need to perform CompareChildEquality since we already compare ActivityId and activity.GetType() as above for all activities in the IdSpace, and check on the collection count
                    if (!ActivityComparer.ListEquals(
                        ActivityComparer.GetDeclaredChildren(currentActivity.Children, currentActivity),
                        ActivityComparer.GetDeclaredChildren(originalActivity.Children, originalActivity),
                        this.AddEqualChildren))
                    {
                        return false;
                    }

                    // there is no need to perform CompareChildEquality since we already compare ActivityId and activity.GetType() as above for all activities in the IdSpace, and check on the collection count
                    if (!ActivityComparer.ListEquals(
                        ActivityComparer.GetDeclaredChildren(currentActivity.ImportedChildren, currentActivity),
                        ActivityComparer.GetDeclaredChildren(originalActivity.ImportedChildren, originalActivity),
                        this.AddEqualChildren))
                    {
                        return false;
                    }

                    if (!ActivityComparer.ListEquals<ActivityDelegate>(
                        ActivityComparer.GetDeclaredDelegates(currentActivity.Delegates, currentActivity),
                        ActivityComparer.GetDeclaredDelegates(originalActivity.Delegates, originalActivity),
                        this.CompareDelegateEqualityAndAddActivitiesPair))
                    {
                        return false;
                    }

                    if (!ActivityComparer.ListEquals<ActivityDelegate>(
                        ActivityComparer.GetDeclaredDelegates(currentActivity.ImportedDelegates, currentActivity),
                        ActivityComparer.GetDeclaredDelegates(originalActivity.ImportedDelegates, originalActivity),
                        this.CompareDelegateEqualityAndAddActivitiesPair))
                    {
                        return false;
                    }

                    if (!ActivityComparer.ListEquals<Variable>(currentActivity.RuntimeVariables, originalActivity.RuntimeVariables, this.CompareVariableEqualityAndAddActivitiesPair))
                    {
                        return false;
                    }

                    // with all runtime metadata except arguments matching, 
                    // the current activities pair qualifies as a matching entry 
                    // let's create an entry
                    DynamicUpdateMapEntry entry = new DynamicUpdateMapEntry(originalActivity.InternalId, currentActivity.InternalId);
                    this.privateMap.AddEntry(entry);

                    if (!this.TryMatchingArguments(entry, originalActivity, currentActivity))
                    {
                        return false;
                    }                    
                }

                // there are no more activities-pair to process.
                // if we are here, it means we have successfully matched the private IdSpace pair
                if (this.argumentChangeDetected)
                {
                    // return the generated map only if we have argument entries
                    argumentChangesMap = this.privateMap;
                }

                return true;
            }

            // return -1 if no subroot is found since the previous index
            // idspace.Owner will always be non-null.
            static int GetIndexOfNextSubtreeRoot(IdSpace idspace, int previousIndex)
            {
                for (int i = previousIndex + 1; i <= idspace.MemberCount; i++)
                {
                    if (object.ReferenceEquals(idspace[i].Parent, idspace.Owner))
                    {
                        return i;
                    }
                }

                return -1;
            }            

            bool TryMatchingArguments(DynamicUpdateMapEntry entry, Activity originalActivity, Activity currentActivity)
            {
                // now, let's try creating argument entries
                IList<ArgumentInfo> oldArguments = ArgumentInfo.List(originalActivity);
                this.nestedFinalizer.CreateArgumentEntries(entry, currentActivity.RuntimeArguments, oldArguments);
                if (entry.HasEnvironmentUpdates)
                {
                    if (entry.EnvironmentUpdateMap.HasArgumentEntries)
                    {
                        foreach (EnvironmentUpdateMapEntry argumentEntry in entry.EnvironmentUpdateMap.ArgumentEntries)
                        {
                            if (!argumentEntry.IsAddition)
                            {
                                // if it is a matching argument pair,
                                // let's add them to the lists for further matching process.                            
                                RuntimeArgument originalArg = originalActivity.RuntimeArguments[argumentEntry.OldOffset];
                                RuntimeArgument updatedArg = currentActivity.RuntimeArguments[argumentEntry.NewOffset];
                                if (!this.TryPreparingArgumentExpressions(originalArg, updatedArg))
                                {
                                    return false;
                                }
                            }
                        }
                    }

                    // we need to also visit subtrees of Expressions of removed arguments
                    IList<ArgumentInfo> newArgumentInfos = ArgumentInfo.List(currentActivity);
                    foreach (RuntimeArgument oldRuntimeArgument in originalActivity.RuntimeArguments)
                    {
                        if (newArgumentInfos.IndexOf(new ArgumentInfo(oldRuntimeArgument)) == EnvironmentUpdateMapEntry.NonExistent)
                        {
                            // this is a removed argument.
                            if (oldRuntimeArgument.IsBound && oldRuntimeArgument.BoundArgument.Expression != null && oldRuntimeArgument.BoundArgument.Expression.MemberOf == originalActivity.MemberOf)
                            {
                                // create an entry for removal of this expression
                                this.privateMap.AddEntry(new DynamicUpdateMapEntry(oldRuntimeArgument.BoundArgument.Expression.InternalId, 0));
                            }
                        }
                    }

                    DynamicUpdateMapBuilder.Finalizer.FillEnvironmentMapMemberCounts(entry.EnvironmentUpdateMap, currentActivity, originalActivity, oldArguments);
                    this.argumentChangeDetected = true;
                }
                else if (currentActivity.RuntimeArguments != null && currentActivity.RuntimeArguments.Count > 0)
                {
                    Fx.Assert(currentActivity.RuntimeArguments.Count == originalActivity.RuntimeArguments.Count, "RuntimeArguments.Count for both currentActivity and originalActivity must be equal.");

                    // if we are here, we know RuntimeArguments matched between currentActivity and originalActivity
                    // but we still need to prepare their Expressions for matching
                    for (int i = 0; i < currentActivity.RuntimeArguments.Count; i++)
                    {
                        if (!this.TryPreparingArgumentExpressions(originalActivity.RuntimeArguments[i], currentActivity.RuntimeArguments[i]))
                        {
                            return false;
                        }
                    }
                }

                if (entry.IsRuntimeUpdateBlocked)
                {
                    entry.EnvironmentUpdateMap = null;
                }

                return true;
            }

            bool TryPreparingArgumentExpressions(RuntimeArgument originalArg, RuntimeArgument updatedArg)
            {
                if (!ActivityComparer.CompareArgumentEquality(updatedArg, originalArg))
                {
                    return false;
                }

                if (updatedArg.BoundArgument != null && updatedArg.BoundArgument.Expression != null)
                {
                    Fx.Assert(originalArg.BoundArgument != null && originalArg.BoundArgument.Expression != null, "both Expressions are either non-null or null.");
                    this.PrepareToMatchSubtree(updatedArg.BoundArgument.Expression, originalArg.BoundArgument.Expression);
                }

                return true;
            }

            void PrepareToMatchSubtree(Activity currentActivity, Activity originalActivity)
            {
                Fx.Assert(currentActivity != null && originalActivity != null, "both activities must not be null.");

                if (originalActivity.MemberOf != this.originalPrivateIdSpace && currentActivity.MemberOf != this.updatedPrivateIdSpace)
                {
                    // we ignore references from other IdSpaces
                    return;
                }

                Fx.Assert(originalActivity.MemberOf == this.originalPrivateIdSpace && currentActivity.MemberOf == this.updatedPrivateIdSpace, "neither activities must be a reference.");                

                // add originalActivity and currentActivity to the pair queue so that their subtrees are further processed for matching
                this.matchedActivities.Enqueue(new Tuple<Activity, Activity>(originalActivity, currentActivity));
            }

            bool AddEqualChildren(Activity currentActivity, Activity originalActivity)
            {
                this.PrepareToMatchSubtree(currentActivity, originalActivity);
                return true;
            }

            bool CompareDelegateEqualityAndAddActivitiesPair(ActivityDelegate currentDelegate, ActivityDelegate originalDelegate)
            {
                if (!ActivityComparer.CompareDelegateEquality(currentDelegate, originalDelegate))
                {
                    return false;
                }

                if (currentDelegate.Handler != null)
                {
                    Fx.Assert(originalDelegate.Handler != null, "both handlers are either non-null or null.");
                    this.PrepareToMatchSubtree(currentDelegate.Handler, originalDelegate.Handler);
                }

                return true;
            }

            bool CompareVariableEqualityAndAddActivitiesPair(Variable currentVariable, Variable originalVariable)
            {
                if (!ActivityComparer.CompareVariableEquality(currentVariable, originalVariable))
                {
                    return false;
                }

                if (currentVariable.Default != null)
                {
                    Fx.Assert(originalVariable.Default != null, "both Defaults are either non-null or null.");
                    this.PrepareToMatchSubtree(currentVariable.Default, originalVariable.Default);
                }

                return true;
            }
        }
    }    
}
