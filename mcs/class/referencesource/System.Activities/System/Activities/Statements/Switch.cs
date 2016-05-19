//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Statements
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq.Expressions;
    using System.Windows.Markup;
    using System.Runtime;
    using System.Runtime.Collections;

    [ContentProperty("Cases")]
    public sealed class Switch<T> : NativeActivity  
    {
        IDictionary<T, Activity> cases;

        public Switch()
        {
        }

        public Switch(Expression<Func<ActivityContext, T>> expression)
            : this()
        {
            if (expression == null)
            {
                throw FxTrace.Exception.ArgumentNull("expression");
            }

            this.Expression = new InArgument<T>(expression);
        }

        public Switch(Activity<T> expression)
            : this()
        {
            if (expression == null)
            {
                throw FxTrace.Exception.ArgumentNull("expression");
            }

            this.Expression = new InArgument<T>(expression);
        }

        public Switch(InArgument<T> expression)
            : this()
        {
            if (expression == null)
            {
                throw FxTrace.Exception.ArgumentNull("expression");
            }

            this.Expression = expression;
        }

        [RequiredArgument]
        [DefaultValue(null)]
        public InArgument<T> Expression
        {
            get; 
            set;
        }

        public IDictionary<T, Activity> Cases
        {
            get
            {
                if (this.cases == null)
                {
                    this.cases = new NullableKeyDictionary<T, Activity>();
                }
                return this.cases;
            }
        }

        [DefaultValue(null)]
        public Activity Default
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
            RuntimeArgument expressionArgument = new RuntimeArgument("Expression", typeof(T), ArgumentDirection.In, true);
            metadata.Bind(Expression, expressionArgument);
            metadata.SetArgumentsCollection(new Collection<RuntimeArgument> { expressionArgument });

            Collection<Activity> children = new Collection<Activity>();

            foreach (Activity child in Cases.Values)
            {
                children.Add(child);
            }

            if (Default != null)
            {
                children.Add(Default);
            }

            metadata.SetChildrenCollection(children);
        }

        protected override void Execute(NativeActivityContext context)
        {
            T result = Expression.Get(context);
            Activity selection = null;

            if (!Cases.TryGetValue(result, out selection))
            {
                if (this.Default != null)
                {
                    selection = this.Default;
                }
                else
                {
                    if (TD.SwitchCaseNotFoundIsEnabled())
                    {
                        TD.SwitchCaseNotFound(this.DisplayName);
                    }
                }
            }

            if (selection != null)
            {
                context.ScheduleActivity(selection);
            }
        }
    }
}
