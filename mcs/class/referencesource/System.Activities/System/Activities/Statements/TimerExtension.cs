//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Statements
{
    using System;

    public abstract class TimerExtension
    {
        protected TimerExtension()
        {
        }

        public void RegisterTimer(TimeSpan timeout, Bookmark bookmark)
        {
            this.OnRegisterTimer(timeout, bookmark);
        }

        public void CancelTimer(Bookmark bookmark)
        {
            this.OnCancelTimer(bookmark);
        }

        protected abstract void OnRegisterTimer(TimeSpan timeout, Bookmark bookmark);
        protected abstract void OnCancelTimer(Bookmark bookmark);
    }
}
