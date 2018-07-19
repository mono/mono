//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Runtime
{
    using System;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Security;

    [DataContract]
    class BookmarkCallbackWrapper : CallbackWrapper
    {
        static readonly Type bookmarkCallbackType = typeof(BookmarkCallback);
        static readonly Type[] bookmarkCallbackParameters = new Type[] { typeof(NativeActivityContext), typeof(Bookmark), typeof(object) };

        public BookmarkCallbackWrapper(BookmarkCallback callback, ActivityInstance owningInstance)
            : this(callback, owningInstance, BookmarkOptions.None)
        {           
        }

        public BookmarkCallbackWrapper(BookmarkCallback callback, ActivityInstance owningInstance, BookmarkOptions bookmarkOptions)
            : base(callback, owningInstance)
        {
            Fx.Assert(callback != null || bookmarkOptions == BookmarkOptions.None, "Either we have a callback or we only allow SingleFire, Blocking bookmarks.");

            this.Options = bookmarkOptions;
        }

        BookmarkOptions options;
        public BookmarkOptions Options
        {
            get
            {
                return this.options;
            }
            private set
            {
                this.options = value;
            }
        }

        [DataMember(EmitDefaultValue = false)]
        public Bookmark Bookmark
        {
            get;
            set;
        }

        [DataMember(EmitDefaultValue = false, Name = "Options")]
        internal BookmarkOptions SerializedOptions
        {
            get { return this.Options; }
            set { this.Options = value; }
        }

        [Fx.Tag.SecurityNote(Critical = "Because we are calling EnsureCallback",
            Safe = "Safe because the method needs to be part of an Activity and we are casting to the callback type and it has a very specific signature. The author of the callback is buying into being invoked from PT.")]
        [SecuritySafeCritical]
        public void Invoke(NativeActivityContext context, Bookmark bookmark, object value)
        {
            EnsureCallback(bookmarkCallbackType, bookmarkCallbackParameters);
            BookmarkCallback bookmarkCallback = (BookmarkCallback)this.Callback;
            bookmarkCallback(context, bookmark, value);
        }

        public ActivityExecutionWorkItem CreateWorkItem(ActivityExecutor executor, bool isExternal, Bookmark bookmark, object value)
        {
            if (this.IsCallbackNull)
            {
                return executor.CreateEmptyWorkItem(this.ActivityInstance);
            }
            else
            {
                return new BookmarkWorkItem(executor, isExternal, this, bookmark, value);
            }
        }
    }
}
