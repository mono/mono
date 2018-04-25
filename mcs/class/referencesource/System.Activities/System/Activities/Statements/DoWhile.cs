//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Statements
{
    using System.Activities;
    using System.Activities.Expressions;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq.Expressions;
    using System.Runtime;
    using System.Runtime.Collections;
    using System.Windows.Markup;
    using SA = System.Activities;

    [ContentProperty("Body")]
    public sealed class DoWhile : NativeActivity
    {
        CompletionCallback onBodyComplete;
        CompletionCallback<bool> onConditionComplete;

        Collection<Variable> variables;

        public DoWhile()
            : base()
        {
        }

        public DoWhile(Expression<Func<ActivityContext, bool>> condition)
            : this()
        {
            if (condition == null)
            {
                throw SA.FxTrace.Exception.ArgumentNull("condition");
            }

            this.Condition = new LambdaValue<bool>(condition);
        }

        public DoWhile(Activity<bool> condition)
            : this()
        {
            if (condition == null)
            {
                throw SA.FxTrace.Exception.ArgumentNull("condition");
            }

            this.Condition = condition;
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
                                throw SA.FxTrace.Exception.ArgumentNull("item");
                            }
                        }
                    };
                }
                return this.variables;
            }
        }

        [DefaultValue(null)]
        [DependsOn("Variables")]
        public Activity<bool> Condition
        {
            get;
            set;
        }

        [DefaultValue(null)]
        [DependsOn("Condition")]
        public Activity Body
        {
            get;
            set;
        }

        protected override void OnCreateDynamicUpdateMap(DynamicUpdate.NativeActivityUpdateMapMetadata metadata, Activity originalActivity)
        {
            metadata.AllowUpdateInsideThisActivity();
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            metadata.SetVariablesCollection(this.Variables);

            if (this.Condition == null)
            {
                metadata.AddValidationError(SA.SR.DoWhileRequiresCondition(this.DisplayName));
            }
            else
            {
                metadata.AddChild(this.Condition);
            }

            metadata.AddChild(this.Body);
        }

        protected override void Execute(NativeActivityContext context)
        {
            // initial logic is the same as when the condition completes with true
            OnConditionComplete(context, null, true);
        }

        void ScheduleCondition(NativeActivityContext context)
        {
            Fx.Assert(this.Condition != null, "validated in OnOpen");
            if (this.onConditionComplete == null)
            {
                this.onConditionComplete = new CompletionCallback<bool>(OnConditionComplete);
            }

            context.ScheduleActivity(this.Condition, this.onConditionComplete);
        }

        void OnConditionComplete(NativeActivityContext context, ActivityInstance completedInstance, bool result)
        {
            if (result)
            {
                if (this.Body != null)
                {
                    if (this.onBodyComplete == null)
                    {
                        this.onBodyComplete = new CompletionCallback(OnBodyComplete);
                    }

                    context.ScheduleActivity(this.Body, this.onBodyComplete);
                }
                else
                {
                    ScheduleCondition(context);
                }
            }
        }

        void OnBodyComplete(NativeActivityContext context, ActivityInstance completedInstance)
        {
            ScheduleCondition(context);
        }
    }
}
