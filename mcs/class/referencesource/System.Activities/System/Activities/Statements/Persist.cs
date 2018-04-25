//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Statements
{
    using System;
    using System.Activities;
    using System.Collections.Generic;

    public sealed class Persist : NativeActivity
    {
        static BookmarkCallback onPersistCompleteCallback;

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
        }

        protected override bool CanInduceIdle
        {
            get
            {
                return true;
            }
        }

        protected override void Execute(NativeActivityContext context)
        {
            if (context.IsInNoPersistScope)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.CannotPersistInsideNoPersist));
            }

            if (onPersistCompleteCallback == null)
            {
                onPersistCompleteCallback = new BookmarkCallback(OnPersistComplete);
            }

            context.RequestPersist(onPersistCompleteCallback);
        }

        static void OnPersistComplete(NativeActivityContext context, Bookmark bookmark, object value)
        {
            // No-op.  This is here to keep the activity from completing.
        }
    }
}
