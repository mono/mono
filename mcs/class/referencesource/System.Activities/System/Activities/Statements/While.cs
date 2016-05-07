//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Statements
{
    using System.Activities.Expressions;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq.Expressions;
    using System.Runtime;
    using System.Runtime.Collections;
    using System.Windows.Markup;

    [SuppressMessage(FxCop.Category.Naming, FxCop.Rule.IdentifiersShouldNotMatchKeywords, Justification = "Optimizing for XAML naming. VB imperative users will [] qualify (e.g. New [While])")]
    [ContentProperty("Body")]
    public sealed class While : NativeActivity
    {
        CompletionCallback onBodyComplete;
        CompletionCallback<bool> onConditionComplete;

        Collection<Variable> variables;

        public While()
            : base()
        {
        }

        public While(Expression<Func<ActivityContext, bool>> condition)
            : this()
        {
            if (condition == null)
            {
                throw FxTrace.Exception.ArgumentNull("condition");
            }

            this.Condition = new LambdaValue<bool>(condition);
        }

        public While(Activity<bool> condition)
            : this()
        {
            if (condition == null)
            {
                throw FxTrace.Exception.ArgumentNull("condition");
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

            metadata.AddChild(this.Body);

            if (this.Condition == null)
            {
                metadata.AddValidationError(SR.WhileRequiresCondition(this.DisplayName));
            }
            else
            {
                metadata.AddChild(this.Condition);
            }
        }

        protected override void Execute(NativeActivityContext context)
        {
            ScheduleCondition(context);
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
