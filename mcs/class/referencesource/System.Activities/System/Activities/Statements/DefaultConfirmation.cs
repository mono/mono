//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Statements
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime;

    sealed class DefaultConfirmation : NativeActivity
    {
        Activity body;
        Variable<CompensationToken> toConfirmToken;
        CompletionCallback onChildConfirmed;

        public DefaultConfirmation()
            : base()
        {
            this.toConfirmToken = new Variable<CompensationToken>();

            this.body = new InternalConfirm()
                {
                    Target = new InArgument<CompensationToken>(toConfirmToken),
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

            metadata.SetImplementationVariablesCollection(new Collection<Variable> { this.toConfirmToken });

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
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.ConfirmWithoutCompensableActivity(this.DisplayName)));
            }

            CompensationToken token = Target.Get(context);
            CompensationTokenData tokenData = token == null ? null : compensationExtension.Get(token.CompensationId);

            Fx.Assert(tokenData != null, "CompensationTokenData must be valid");

            if (tokenData.ExecutionTracker.Count > 0)
            {
                if (this.onChildConfirmed == null)
                {
                    this.onChildConfirmed = new CompletionCallback(InternalExecute);
                }

                this.toConfirmToken.Set(context, new CompensationToken(tokenData.ExecutionTracker.Get()));

                Fx.Assert(Body != null, "Body must be valid");
                context.ScheduleActivity(Body, this.onChildConfirmed);
            }
        }

        protected override void Cancel(NativeActivityContext context)
        {
            // Suppress Cancel   
        }
    }

}

