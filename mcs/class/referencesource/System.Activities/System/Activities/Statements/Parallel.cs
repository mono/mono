//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Statements
{
    using System.Activities;
    using System.Activities.DynamicUpdate;
    using System.Activities.Validation;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Windows.Markup;
    using System.Runtime.Collections;

    [ContentProperty("Branches")]
    public sealed class Parallel : NativeActivity
    {
        CompletionCallback<bool> onConditionComplete;
        Collection<Activity> branches;
        Collection<Variable> variables;

        Variable<bool> hasCompleted;

        public Parallel()
            : base()
        {
        }

        public Collection<Variable> Variables
        {
            get
            {
                if (this.variables == null)
                {
                    this.variables = new ValidatingCollection<Variable>
                    {
                        // disallow null values
                        OnAddValidationCallback = item =>
                        {
                            if (item == null)
                            {
                                throw FxTrace.Exception.ArgumentNull("item");
                            }
                        }
                    };
                }
                return this.variables;
            }
        }

        [DefaultValue(null)]
        [DependsOn("Variables")]
        public Activity<bool> CompletionCondition
        {
            get;
            set;
        }

        [DependsOn("CompletionCondition")]
        public Collection<Activity> Branches
        {
            get
            {
                if (this.branches == null)
                {
                    this.branches = new ValidatingCollection<Activity>
                    {
                        // disallow null values
                        OnAddValidationCallback = item =>
                        {
                            if (item == null)
                            {
                                throw FxTrace.Exception.ArgumentNull("item");
                            }
                        }
                    };
                }
                return this.branches;
            }
        }
        
        protected override void OnCreateDynamicUpdateMap(NativeActivityUpdateMapMetadata metadata, Activity originalActivity)
        {
            metadata.AllowUpdateInsideThisActivity();
        }

        protected override void UpdateInstance(NativeActivityUpdateContext updateContext)
        {
            if (updateContext.IsCancellationRequested || this.branches == null)
            {
                return;
            }

            if (this.CompletionCondition != null && updateContext.GetValue(this.hasCompleted))
            {
                // when CompletionCondition exists, schedule newly added branches only if "hasCompleted" variable evaluates to false
                return;
            }           

            CompletionCallback onBranchComplete = new CompletionCallback(OnBranchComplete);

            foreach (Activity branch in this.branches)
            {
                if (updateContext.IsNewlyAdded(branch))
                {
                    updateContext.ScheduleActivity(branch, onBranchComplete);
                }
            }
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            Collection<Activity> children = new Collection<Activity>();

            foreach (Activity branch in this.Branches)
            {
                children.Add(branch);
            }

            if (this.CompletionCondition != null)
            {
                children.Add(this.CompletionCondition);
            }

            metadata.SetChildrenCollection(children);

            metadata.SetVariablesCollection(this.Variables);

            if (this.CompletionCondition != null)
            {
                if (this.hasCompleted == null)
                {
                    this.hasCompleted = new Variable<bool>("hasCompletedVar");
                }

                metadata.AddImplementationVariable(this.hasCompleted);
            }
        }

        protected override void Execute(NativeActivityContext context)
        {
            if (this.branches != null && this.Branches.Count != 0)
            {
                CompletionCallback onBranchComplete = new CompletionCallback(OnBranchComplete);

                for (int i = this.Branches.Count - 1; i >= 0; i--)
                {
                    context.ScheduleActivity(this.Branches[i], onBranchComplete);
                }
            }
        }

        protected override void Cancel(NativeActivityContext context)
        {
            // If we don't have a completion condition then we can just
            // use default logic.
            if (this.CompletionCondition == null)
            {
                base.Cancel(context);
            }
            else
            {
                context.CancelChildren();
            }
        }

        void OnBranchComplete(NativeActivityContext context, ActivityInstance completedInstance)
        {
            if (this.CompletionCondition != null && !this.hasCompleted.Get(context))
            {
                // If we haven't completed, we've been requested to cancel, and we've had a child
                // end in a non-Closed state then we should cancel ourselves.
                if (completedInstance.State != ActivityInstanceState.Closed && context.IsCancellationRequested)
                {
                    context.MarkCanceled();
                    this.hasCompleted.Set(context, true);
                    return;
                }

                if (this.onConditionComplete == null)
                {
                    this.onConditionComplete = new CompletionCallback<bool>(OnConditionComplete);
                }

                context.ScheduleActivity(this.CompletionCondition, this.onConditionComplete);
            }
        }

        void OnConditionComplete(NativeActivityContext context, ActivityInstance completedInstance, bool result)
        {
            if (result)
            {
                context.CancelChildren();
                this.hasCompleted.Set(context, true);
            }
        }
    }
}
