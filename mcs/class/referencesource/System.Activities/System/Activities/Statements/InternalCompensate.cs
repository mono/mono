//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Statements
{
    using System;
    using System.Collections.ObjectModel;
    using System.Collections.Generic;
    using System.Runtime;

    sealed class InternalCompensate : NativeActivity
    {
        public InternalCompensate()
            : base()
        {
        }

        public InArgument<CompensationToken> Target
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
            RuntimeArgument targetArgument = new RuntimeArgument("Target", typeof(CompensationToken), ArgumentDirection.In);
            metadata.Bind(this.Target, targetArgument);
            metadata.SetArgumentsCollection(new Collection<RuntimeArgument> { targetArgument });
        }

        protected override void Execute(NativeActivityContext context)
        {
            CompensationExtension compensationExtension = context.GetExtension<CompensationExtension>();
            Fx.Assert(compensationExtension != null, "CompensationExtension must be valid");

            CompensationToken compensationToken = Target.Get(context);
            Fx.Assert(compensationToken != null, "CompensationToken must be valid");

            // The compensationToken should be a valid one at this point. Ensure its validated in Compensate activity.
            CompensationTokenData tokenData = compensationExtension.Get(compensationToken.CompensationId);
            Fx.Assert(tokenData != null, "The compensationToken should be a valid one at this point. Ensure its validated in Compensate activity.");

            Fx.Assert(tokenData.BookmarkTable[CompensationBookmarkName.Compensated] == null, "Bookmark should not be already initialized in the bookmark table.");
            tokenData.BookmarkTable[CompensationBookmarkName.Compensated] = context.CreateBookmark(new BookmarkCallback(OnCompensated));

            tokenData.CompensationState = CompensationState.Compensating;
            compensationExtension.NotifyMessage(context, tokenData.CompensationId, CompensationBookmarkName.OnCompensation);
        }

        // Successfully received Compensated response. 
        void OnCompensated(NativeActivityContext context, Bookmark bookmark, object value)
        {
            CompensationExtension compensationExtension = context.GetExtension<CompensationExtension>();
            Fx.Assert(compensationExtension != null, "CompensationExtension must be valid");

            CompensationToken compensationToken = Target.Get(context);
            Fx.Assert(compensationToken != null, "CompensationToken must be valid");

            CompensationTokenData tokenData = compensationExtension.Get(compensationToken.CompensationId);
            Fx.Assert(tokenData != null, "The compensationToken should be a valid one at this point. Ensure its validated in Compensate activity.");

            tokenData.CompensationState = CompensationState.Compensated;
            if (TD.CompensationStateIsEnabled())
            {
                TD.CompensationState(tokenData.DisplayName, tokenData.CompensationState.ToString());
            }

            // Remove the token from the parent! 
            if (tokenData.ParentCompensationId != CompensationToken.RootCompensationId)
            {
                CompensationTokenData parentToken = compensationExtension.Get(tokenData.ParentCompensationId);
                Fx.Assert(parentToken != null, "parentToken must be valid");

                parentToken.ExecutionTracker.Remove(tokenData);
            }
            else
            {
                // remove from workflow root...
                CompensationTokenData parentToken = compensationExtension.Get(CompensationToken.RootCompensationId);
                Fx.Assert(parentToken != null, "parentToken must be valid");

                parentToken.ExecutionTracker.Remove(tokenData);
            }

            tokenData.RemoveBookmark(context, CompensationBookmarkName.Compensated);

            // Remove the token from the extension...
            compensationExtension.Remove(compensationToken.CompensationId);
        }

        protected override void Cancel(NativeActivityContext context)
        {
            // Suppress Cancel   
        }
    }
}

