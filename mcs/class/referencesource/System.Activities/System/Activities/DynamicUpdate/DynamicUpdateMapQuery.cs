// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.Activities.DynamicUpdate
{
    using System;
    using System.Activities.DynamicUpdate;
    using System.Activities.XamlIntegration;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.Serialization;

    public class DynamicUpdateMapQuery
    {
        private DynamicUpdateMap map;
        private Activity updatedWorkflowDefinition;
        private Activity originalWorkflowDefinition;

        internal DynamicUpdateMapQuery(DynamicUpdateMap map, Activity updatedWorkflowDefinition, Activity originalWorkflowDefinition)
        {
            Fx.Assert(updatedWorkflowDefinition == updatedWorkflowDefinition.RootActivity, "This parameter must be root of workflow");
            Fx.Assert(originalWorkflowDefinition == originalWorkflowDefinition.RootActivity, "This parameter must be root of workflow");

            this.map = map;
            this.updatedWorkflowDefinition = updatedWorkflowDefinition;
            this.originalWorkflowDefinition = originalWorkflowDefinition;
        }

        public Activity FindMatch(Activity activity)
        {
            if (activity == null)
            {
                throw FxTrace.Exception.ArgumentNull("activity");
            }

            if (IsInNewDefinition(activity))
            {
                return this.MatchNewToOld(activity);
            }
            else
            {
                return this.MatchOldToNew(activity);
            }
        }

        public Variable FindMatch(Variable variable)
        {
            if (variable == null)
            {
                throw FxTrace.Exception.ArgumentNull("variable");
            }            

            if (IsInNewDefinition(variable))
            {
                return this.MatchNewToOld(variable);
            }
            else
            {
                return this.MatchOldToNew(variable);
            }
        }

        public bool CanApplyUpdateWhileRunning(Activity activity)
        {
            if (activity == null)
            {
                throw FxTrace.Exception.ArgumentNull("activity");
            }

            return this.CanApplyUpdateWhileRunning(activity, IsInNewDefinition(activity));
        }

        private Activity MatchNewToOld(Activity newActivity)
        {
            DynamicUpdateMapEntry entry;
            return this.MatchNewToOld(newActivity, out entry);
        }

        private Activity MatchNewToOld(Activity newActivity, out DynamicUpdateMapEntry entry)
        {
            entry = null;
            if (this.map.TryGetUpdateEntryByNewId(newActivity.InternalId, out entry))
            {
                IdSpace rootIdSpace;
                if (this.map.IsForImplementation)
                {
                    rootIdSpace = this.originalWorkflowDefinition.ParentOf;                    
                }
                else
                {
                    rootIdSpace = this.originalWorkflowDefinition.MemberOf;
                }

                if (rootIdSpace != null)
                {
                    return rootIdSpace[entry.OldActivityId];
                }        
            }            

            return null;
        }

        private Variable MatchNewToOld(Variable newVariable)
        {
            if (!newVariable.IsPublic)
            {
                return null;
            }

            DynamicUpdateMapEntry entry;
            Activity oldOwner = this.MatchNewToOld(newVariable.Owner, out entry);
            if (oldOwner == null)
            {
                return null;
            }

            int newIndex = newVariable.Owner.RuntimeVariables.IndexOf(newVariable);
            int? oldIndex = entry.HasEnvironmentUpdates ?
                entry.EnvironmentUpdateMap.GetOldVariableIndex(newIndex) :
                newIndex;

            return oldIndex.HasValue ? oldOwner.RuntimeVariables[oldIndex.Value] : null;
        }

        private Activity MatchOldToNew(Activity oldActivity)
        {
            DynamicUpdateMapEntry entry;
            return this.MatchOldToNew(oldActivity, out entry);
        }

        private Activity MatchOldToNew(Activity oldActivity, out DynamicUpdateMapEntry entry)
        {
            entry = null;

            if (this.map.TryGetUpdateEntry(oldActivity.InternalId, out entry) && entry.NewActivityId > 0)
            {
                IdSpace rootIdSpace;
                if (this.map.IsForImplementation)
                {
                    rootIdSpace = this.updatedWorkflowDefinition.ParentOf;                    
                }
                else
                {
                    rootIdSpace = this.updatedWorkflowDefinition.MemberOf;
                }

                if (rootIdSpace != null)
                {
                    return rootIdSpace[entry.NewActivityId];
                }        
            }

            return null;
        }

        private Variable MatchOldToNew(Variable oldVariable)
        {
            if (!oldVariable.IsPublic)
            {
                return null;
            }

            DynamicUpdateMapEntry entry;
            Activity newOwner = this.MatchOldToNew(oldVariable.Owner, out entry);
            if (newOwner == null)
            {
                return null;
            }

            int oldIndex = oldVariable.Owner.RuntimeVariables.IndexOf(oldVariable);
            int? newIndex = entry.HasEnvironmentUpdates ?
                entry.EnvironmentUpdateMap.GetNewVariableIndex(oldIndex) :
                oldIndex;

            return newIndex.HasValue ? newOwner.RuntimeVariables[newIndex.Value] : null;
        }

        private bool CanApplyUpdateWhileRunning(Activity activity, bool isInNewDefinition)
        {
            Activity currentActivity = activity;
            IdSpace rootIdSpace = activity.MemberOf;
            do
            {
                DynamicUpdateMapEntry entry = null;
                if (isInNewDefinition)
                {
                    this.map.TryGetUpdateEntryByNewId(currentActivity.InternalId, out entry);
                }
                else if (currentActivity.MemberOf == rootIdSpace)
                {
                    this.map.TryGetUpdateEntry(currentActivity.InternalId, out entry);
                }

                if (entry != null &&
                    (entry.NewActivityId < 1 ||
                     entry.IsRuntimeUpdateBlocked ||
                     entry.IsUpdateBlockedByUpdateAuthor))
                {
                    return false;
                }

                currentActivity = currentActivity.Parent;
            }
            while (currentActivity != null && currentActivity.MemberOf == rootIdSpace);

            return true;
        }

        private bool IsInNewDefinition(Activity activity, bool isVariableOwner = false)
        {
            bool result = false;
            if (activity.RootActivity == this.updatedWorkflowDefinition)
            {
                result = true;
            }
            else if (activity.RootActivity == this.originalWorkflowDefinition)
            {
                result = false;
            }
            else
            {
                ThrowNotInDefinition(isVariableOwner, 
                    SR.QueryVariableIsNotInDefinition,
                    SR.QueryActivityIsNotInDefinition);
            }

            // We only support either the public or the implementation IdSpace at the root of the workflow.
            // The user does not have visibility into nested IdSpaces so should not be querying into them.
            if (this.map.IsForImplementation)
            {
                if (activity.MemberOf.Owner != activity.RootActivity)
                {
                    ThrowNotInDefinition(isVariableOwner,
                        SR.QueryVariableIsPublic(activity.RootActivity),
                        SR.QueryActivityIsPublic(activity.RootActivity));
                }
            }
            else if (activity.MemberOf != activity.RootActivity.MemberOf)
            {
                ThrowNotInDefinition(isVariableOwner,
                    SR.QueryVariableIsInImplementation(activity.MemberOf.Owner),
                    SR.QueryActivityIsInImplementation(activity.MemberOf.Owner));
            }

            return result;
        }

        private bool IsInNewDefinition(Variable variable)
        {
            if (variable.Owner == null)
            {
                throw FxTrace.Exception.Argument("variable", SR.QueryVariableIsNotInDefinition);
            }
            if (!variable.IsPublic)
            {
                throw FxTrace.Exception.Argument("variable", SR.QueryVariableIsNotPublic);
            }
            return IsInNewDefinition(variable.Owner, true);
        }

        private void ThrowNotInDefinition(bool isVariableOwner, string variableMessage, string activityMessage)
        {
            if (isVariableOwner)
            {
                throw FxTrace.Exception.Argument("variable", variableMessage);
            }
            else
            {
                throw FxTrace.Exception.Argument("activity", activityMessage);
            }
        }
    }
}
