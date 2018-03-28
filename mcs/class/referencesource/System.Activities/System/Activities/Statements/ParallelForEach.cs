//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Statements
{
    using System.Activities;
    using System.Activities.DynamicUpdate;
    using System.Activities.Validation;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Windows.Markup;

    [ContentProperty("Body")]
    public sealed class ParallelForEach<T> : NativeActivity
    {
        Variable<bool> hasCompleted;
        CompletionCallback<bool> onConditionComplete;

        public ParallelForEach()
            : base()
        {
        }

        [DefaultValue(null)]
        public ActivityAction<T> Body
        {
            get;
            set;
        }

        [DefaultValue(null)]
        public Activity<bool> CompletionCondition
        {
            get;
            set;
        }

        [RequiredArgument]
        [DefaultValue(null)]
        public InArgument<IEnumerable<T>> Values
        {
            get;
            set;
        }

        protected override void OnCreateDynamicUpdateMap(NativeActivityUpdateMapMetadata metadata, Activity originalActivity)
        {
            metadata.AllowUpdateInsideThisActivity();
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            RuntimeArgument valuesArgument = new RuntimeArgument("Values", typeof(IEnumerable<T>), ArgumentDirection.In, true);
            metadata.Bind(this.Values, valuesArgument);
            metadata.SetArgumentsCollection(new Collection<RuntimeArgument> { valuesArgument });

            // declare the CompletionCondition as a child
            if (this.CompletionCondition != null)
            {
                metadata.SetChildrenCollection(new Collection<Activity> { this.CompletionCondition });
            }

            // declare the hasCompleted variable
            if (this.CompletionCondition != null)
            {
                if (this.hasCompleted == null)
                {
                    this.hasCompleted = new Variable<bool>("hasCompletedVar");
                }

                metadata.AddImplementationVariable(this.hasCompleted);
            }

            metadata.AddDelegate(this.Body);
        }

        protected override void Execute(NativeActivityContext context)
        {
            IEnumerable<T> values = this.Values.Get(context);
            if (values == null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.ParallelForEachRequiresNonNullValues(this.DisplayName)));
            }

            IEnumerator<T> valueEnumerator = values.GetEnumerator();

            CompletionCallback onBodyComplete = new CompletionCallback(OnBodyComplete);
            while (valueEnumerator.MoveNext())
            {
                if (this.Body != null)
                {
                    context.ScheduleAction(this.Body, valueEnumerator.Current, onBodyComplete);
                }
            }
            valueEnumerator.Dispose();
        }

        void OnBodyComplete(NativeActivityContext context, ActivityInstance completedInstance)
        {
            // for the completion condition, we handle cancelation ourselves
            if (this.CompletionCondition != null && !this.hasCompleted.Get(context))
            {
                if (completedInstance.State != ActivityInstanceState.Closed && context.IsCancellationRequested)
                {
                    // If we hadn't completed before getting canceled
                    // or one of our iteration of body cancels then we'll consider
                    // ourself canceled.
                    context.MarkCanceled();
                    this.hasCompleted.Set(context, true);
                }            
                else 
                {
                    if (this.onConditionComplete == null)
                    {
                        this.onConditionComplete = new CompletionCallback<bool>(OnConditionComplete);
                    }
                    context.ScheduleActivity(CompletionCondition, this.onConditionComplete);              
                }
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
