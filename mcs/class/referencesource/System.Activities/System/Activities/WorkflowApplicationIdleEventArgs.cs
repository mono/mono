//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities
{
    using System.Activities.Hosting;
    using System.Collections.ObjectModel;
    using System.Runtime;

    [Fx.Tag.XamlVisible(false)]
    public class WorkflowApplicationIdleEventArgs : WorkflowApplicationEventArgs
    {
        ReadOnlyCollection<BookmarkInfo> bookmarks;

        internal WorkflowApplicationIdleEventArgs(WorkflowApplication application)
            : base(application)
        {
        }

        public ReadOnlyCollection<BookmarkInfo> Bookmarks
        {
            get
            {
                if (this.bookmarks == null)
                {
                    this.bookmarks = this.Owner.GetBookmarksForIdle();
                }

                return this.bookmarks;
            }
        }
    }
}
