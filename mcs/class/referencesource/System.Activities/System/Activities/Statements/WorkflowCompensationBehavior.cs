//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Statements
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Threading;
    using System.Collections.ObjectModel;

    sealed class WorkflowCompensationBehavior : NativeActivity
    {
        Variable<CompensationToken> currentCompensationToken;

        public WorkflowCompensationBehavior()
            : base()
        {
            currentCompensationToken = new Variable<CompensationToken>
                {
                    Name = "currentCompensationToken",
                };

            DefaultCompensation = new DefaultCompensation()
                {
                    Target = new InArgument<CompensationToken>(this.currentCompensationToken),
                };

            DefaultConfirmation = new DefaultConfirmation()
                {
                    Target = new InArgument<CompensationToken>(this.currentCompensationToken),
                };
        }

        Activity DefaultCompensation
        {
            get;
            set;
        }

        Activity DefaultConfirmation
        {
            get;
            set;
        }

        protected override bool CanInduceIdle
        {
            get
            {
                return true;
            }
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            Fx.Assert(this.DefaultCompensation != null, "DefaultCompensation must be valid");
            Fx.Assert(this.DefaultConfirmation != null, "DefaultConfirmation must be valid");
            metadata.SetImplementationChildrenCollection(
                new Collection<Activity>
                {
                    this.DefaultCompensation, 
                    this.DefaultConfirmation
                });

            metadata.SetImplementationVariablesCollection(new Collection<Variable> { this.currentCompensationToken });
        }

        protected override void Execute(NativeActivityContext context)
        {
            Bookmark mainRootCompleteBookmark = context.CreateBookmark(OnMainRootComplete, BookmarkOptions.NonBlocking);
            context.RegisterMainRootCompleteCallback(mainRootCompleteBookmark);

            CompensationExtension compensationExtension = context.GetExtension<CompensationExtension>();
            Fx.Assert(compensationExtension != null, "CompensationExtension must be valid");

            compensationExtension.WorkflowCompensation = context.CreateBookmark(new BookmarkCallback(OnCompensate));
            compensationExtension.WorkflowConfirmation = context.CreateBookmark(new BookmarkCallback(OnConfirm));

            Fx.Assert(compensationExtension.WorkflowCompensationScheduled != null, "compensationExtension.WorkflowCompensationScheduled bookmark must be setup by now");
            context.ResumeBookmark(compensationExtension.WorkflowCompensationScheduled, null);
        }

        protected override void Cancel(NativeActivityContext context)
        {
            context.CancelChildren();
        }

        void OnMainRootComplete(NativeActivityContext context, Bookmark bookmark, object value)
        {
            CompensationExtension compensationExtension = context.GetExtension<CompensationExtension>();
            Fx.Assert(compensationExtension != null, "CompensationExtension must be valid");

            CompensationTokenData rootHandle = compensationExtension.Get(CompensationToken.RootCompensationId);
            Fx.Assert(rootHandle != null, "rootToken must be valid");

            ActivityInstanceState completionState = (ActivityInstanceState)value;

            if (completionState == ActivityInstanceState.Closed)
            {
                context.ResumeBookmark(compensationExtension.WorkflowConfirmation, new CompensationToken(rootHandle));
            }
            else if (completionState == ActivityInstanceState.Canceled)
            {
                context.ResumeBookmark(compensationExtension.WorkflowCompensation, new CompensationToken(rootHandle));
            }
            else if (completionState == ActivityInstanceState.Faulted)
            {
                // Do nothing. Neither Compensate nor Confirm.
                // Remove the bookmark to complete the WorkflowCompensationBehavior execution. 
               context.RemoveBookmark(compensationExtension.WorkflowConfirmation); 
               context.RemoveBookmark(compensationExtension.WorkflowCompensation);
            }
        }

        void OnCompensate(NativeActivityContext context, Bookmark bookmark, object value)
        {
            CompensationExtension compensationExtension = context.GetExtension<CompensationExtension>();
            Fx.Assert(compensationExtension != null, "CompensationExtension must be valid");

            CompensationToken rootToken = (CompensationToken)value;
            Fx.Assert(rootToken != null, "rootToken must be passed");

            this.currentCompensationToken.Set(context, rootToken);

            CompensationTokenData rootTokenData = compensationExtension.Get(rootToken.CompensationId);
            if (rootTokenData.ExecutionTracker.Count > 0)
            {
                context.ScheduleActivity(DefaultCompensation, new CompletionCallback(OnCompensationComplete));
            }
            else
            {
                OnCompensationComplete(context, null);
            }
        }

        void OnCompensationComplete(NativeActivityContext context, ActivityInstance completedInstance)
        {
            // Remove bookmark.... have a cleanup book mark method...
            CompensationExtension compensationExtension = context.GetExtension<CompensationExtension>();
            Fx.Assert(compensationExtension != null, "CompensationExtension must be valid");

            context.RemoveBookmark(compensationExtension.WorkflowConfirmation);
        }

        void OnConfirm(NativeActivityContext context, Bookmark bookmark, object value)
        {
            CompensationExtension compensationExtension = context.GetExtension<CompensationExtension>();
            Fx.Assert(compensationExtension != null, "CompensationExtension must be valid");

            CompensationToken rootToken = (CompensationToken)value;
            Fx.Assert(rootToken != null, "rootToken must be passed");

            this.currentCompensationToken.Set(context, rootToken);

            CompensationTokenData rootTokenData = compensationExtension.Get(rootToken.CompensationId);
            if (rootTokenData.ExecutionTracker.Count > 0)
            {
                context.ScheduleActivity(DefaultConfirmation, new CompletionCallback(OnConfirmationComplete));
            }
            else
            {
                OnConfirmationComplete(context, null);
            }
        }

        void OnConfirmationComplete(NativeActivityContext context, ActivityInstance completedInstance)
        {
            CompensationExtension compensationExtension = context.GetExtension<CompensationExtension>();
            Fx.Assert(compensationExtension != null, "CompensationExtension must be valid");

            context.RemoveBookmark(compensationExtension.WorkflowCompensation);
        }
    }
}


