//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Statements
{
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Windows.Markup;
    using System.Activities;
    using System.Activities.Validation;
    using SA = System.Activities;

    [ContentProperty("Body")]
    public sealed class ForEach<T> : NativeActivity
    {
        Variable<IEnumerator<T>> valueEnumerator;
        CompletionCallback onChildComplete;

        public ForEach()
            : base()
        {
            this.valueEnumerator = new Variable<IEnumerator<T>>();
        }

        [DefaultValue(null)]
        public ActivityAction<T> Body
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

        CompletionCallback OnChildComplete
        {
            get
            {
                if (this.onChildComplete == null)
                {
                    this.onChildComplete = new CompletionCallback(GetStateAndExecute);
                }

                return this.onChildComplete;
            }
        }

        protected override void OnCreateDynamicUpdateMap(DynamicUpdate.NativeActivityUpdateMapMetadata metadata, Activity originalActivity)
        {
            metadata.AllowUpdateInsideThisActivity();
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            RuntimeArgument valuesArgument = new RuntimeArgument("Values", typeof(IEnumerable<T>), ArgumentDirection.In, true);
            metadata.Bind(this.Values, valuesArgument);

            metadata.AddArgument(valuesArgument);
            metadata.AddDelegate(this.Body);
            metadata.AddImplementationVariable(this.valueEnumerator);
        }

        protected override void Execute(NativeActivityContext context)
        {
            IEnumerable<T> values = this.Values.Get(context);
            if (values == null)
            {
                throw SA.FxTrace.Exception.AsError(new InvalidOperationException(SA.SR.ForEachRequiresNonNullValues(this.DisplayName)));
            }

            IEnumerator<T> valueEnumerator = values.GetEnumerator();
            this.valueEnumerator.Set(context, valueEnumerator);

            if (this.Body == null || this.Body.Handler == null)
            {
                while (valueEnumerator.MoveNext())
                {
                    // do nothing                
                };
                valueEnumerator.Dispose();
                return;
            }
            InternalExecute(context, null, valueEnumerator);
        }

        void GetStateAndExecute(NativeActivityContext context, ActivityInstance completedInstance)
        {
            IEnumerator<T> valueEnumerator = this.valueEnumerator.Get(context);
            Fx.Assert(valueEnumerator != null, "GetStateAndExecute");
            InternalExecute(context, completedInstance, valueEnumerator);
        }

        void InternalExecute(NativeActivityContext context, ActivityInstance completedInstance, IEnumerator<T> valueEnumerator)
        {
            Fx.Assert(this.Body != null && this.Body.Handler != null, "Body and Body.Handler should not be null");

            if (!valueEnumerator.MoveNext())
            {
                if (completedInstance != null)
                {
                    if (completedInstance.State == ActivityInstanceState.Canceled ||
                        (context.IsCancellationRequested && completedInstance.State == ActivityInstanceState.Faulted))
                    {
                        context.MarkCanceled();
                    }
                }
                valueEnumerator.Dispose();
                return;
            }

            // After making sure there is another value, let's check for cancelation
            if (context.IsCancellationRequested)
            {
                context.MarkCanceled();
                valueEnumerator.Dispose();
                return;
            }

            context.ScheduleAction(this.Body, valueEnumerator.Current, this.OnChildComplete);
        }
    }
}
