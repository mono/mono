//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Statements
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Collections.ObjectModel;
    using SA = System.Activities;

    sealed class DefaultCompensation : NativeActivity
    {
        Activity body;

        Variable<CompensationToken> toCompensateToken;

        CompletionCallback onChildCompensated;

        public DefaultCompensation()
            : base()
        {
            this.toCompensateToken = new Variable<CompensationToken>();

            this.body = new InternalCompensate()
                {
                    Target = new InArgument<CompensationToken>(toCompensateToken),
                };
        }

        public InArgument<CompensationToken> Target
        {
            get;
            set;
        }

        Activity Body
        {
            get { return this.body; }
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            RuntimeArgument targetArgument = new RuntimeArgument("Target", typeof(CompensationToken), ArgumentDirection.In);
            metadata.Bind(this.Target, targetArgument);

            metadata.SetArgumentsCollection(new Collection<RuntimeArgument> { targetArgument });

            metadata.SetImplementationVariablesCollection(new Collection<Variable> { this.toCompensateToken });

            Fx.Assert(this.Body != null, "Body must be valid");
            metadata.SetImplementationChildrenCollection(new Collection<Activity> { this.Body });
        }

        protected override void Execute(NativeActivityContext context)
        {
            InternalExecute(context, null);
        }

        void InternalExecute(NativeActivityContext context, ActivityInstance completedInstance)
        {
            CompensationExtension compensationExtension = context.GetExtension<CompensationExtension>();
            if (compensationExtension == null)
            {
                throw SA.FxTrace.Exception.AsError(new InvalidOperationException(SA.SR.CompensateWithoutCompensableActivity(this.DisplayName)));
            }

            CompensationToken token = Target.Get(context);
            CompensationTokenData tokenData = token == null ? null : compensationExtension.Get(token.CompensationId);

            Fx.Assert(tokenData != null, "CompensationTokenData must be valid");

            if (tokenData.ExecutionTracker.Count > 0)
            {
                if (this.onChildCompensated == null)
                {
                    this.onChildCompensated = new CompletionCallback(InternalExecute);
                }

                this.toCompensateToken.Set(context, new CompensationToken(tokenData.ExecutionTracker.Get()));

                Fx.Assert(Body != null, "Body must be valid");
                context.ScheduleActivity(Body, this.onChildCompensated);
            }     
        }

        protected override void Cancel(NativeActivityContext context)
        {
            // Suppress Cancel   
        }

    }

}
