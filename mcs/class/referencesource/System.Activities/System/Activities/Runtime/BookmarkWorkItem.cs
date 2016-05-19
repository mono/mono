//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Runtime
{
    using System;
    using System.Activities.Hosting;
    using System.Runtime;
    using System.Runtime.Serialization;

    [DataContract]
    class BookmarkWorkItem : ActivityExecutionWorkItem
    {
        BookmarkCallbackWrapper callbackWrapper;
        Bookmark bookmark;
        object state;

        public BookmarkWorkItem(ActivityExecutor executor, bool isExternal, BookmarkCallbackWrapper callbackWrapper, Bookmark bookmark, object value)
            : this(callbackWrapper, bookmark, value)
        {
            if (isExternal)
            {
                executor.EnterNoPersist();
                this.ExitNoPersistRequired = true;
            }
        }

        // This ctor is only used by subclasses which make their own determination about no persist or not
        protected BookmarkWorkItem(BookmarkCallbackWrapper callbackWrapper, Bookmark bookmark, object value)
            : base(callbackWrapper.ActivityInstance)
        {
            this.callbackWrapper = callbackWrapper;
            this.bookmark = bookmark;
            this.state = value;
        }

        [DataMember(Name = "callbackWrapper")]
        internal BookmarkCallbackWrapper SerializedCallbackWrapper
        {
            get { return this.callbackWrapper; }
            set { this.callbackWrapper = value; }
        }

        [DataMember(Name = "bookmark")]
        internal Bookmark SerializedBookmark
        {
            get { return this.bookmark; }
            set { this.bookmark = value; }
        }

        [DataMember(EmitDefaultValue = false, Name = "state")]
        internal object SerializedState
        {
            get { return this.state; }
            set { this.state = value; }
        }

        public override void TraceCompleted()
        {
            if (TD.CompleteBookmarkWorkItemIsEnabled())
            {
                TD.CompleteBookmarkWorkItem(this.ActivityInstance.Activity.GetType().ToString(), this.ActivityInstance.Activity.DisplayName, this.ActivityInstance.Id, ActivityUtilities.GetTraceString(this.bookmark), ActivityUtilities.GetTraceString(this.bookmark.Scope));
            }
        }

        public override void TraceScheduled()
        {
            if (TD.ScheduleBookmarkWorkItemIsEnabled())
            {
                TD.ScheduleBookmarkWorkItem(this.ActivityInstance.Activity.GetType().ToString(), this.ActivityInstance.Activity.DisplayName, this.ActivityInstance.Id, ActivityUtilities.GetTraceString(this.bookmark), ActivityUtilities.GetTraceString(this.bookmark.Scope));
            }
        }

        public override void TraceStarting()
        {
            if (TD.StartBookmarkWorkItemIsEnabled())
            {
                TD.StartBookmarkWorkItem(this.ActivityInstance.Activity.GetType().ToString(), this.ActivityInstance.Activity.DisplayName, this.ActivityInstance.Id, ActivityUtilities.GetTraceString(this.bookmark), ActivityUtilities.GetTraceString(this.bookmark.Scope));
            }
        }

        public override bool Execute(ActivityExecutor executor, BookmarkManager bookmarkManager)
        {
            NativeActivityContext nativeContext = executor.NativeActivityContextPool.Acquire();

            try
            {
                nativeContext.Initialize(this.ActivityInstance, executor, bookmarkManager);
                this.callbackWrapper.Invoke(nativeContext, this.bookmark, this.state);
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }

                this.ExceptionToPropagate = e;
            }
            finally
            {
                nativeContext.Dispose();
                executor.NativeActivityContextPool.Release(nativeContext);
            }

            return true;
        }
    }
}
