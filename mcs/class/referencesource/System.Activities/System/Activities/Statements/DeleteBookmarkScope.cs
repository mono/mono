//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Statements
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using SA = System.Activities;

    public sealed class DeleteBookmarkScope : NativeActivity
    {
        public DeleteBookmarkScope()
        {
        }

        public InArgument<BookmarkScope> Scope
        {
            get;
            set;
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            RuntimeArgument subInstanceArgument = new RuntimeArgument("Scope", typeof(BookmarkScope), ArgumentDirection.In);
            metadata.Bind(this.Scope, subInstanceArgument);

            metadata.SetArgumentsCollection(new Collection<RuntimeArgument> { subInstanceArgument });
        }

        protected override void Execute(NativeActivityContext context)
        {
            BookmarkScope toUnregister = this.Scope.Get(context);

            if (toUnregister == null)
            {
                throw SA.FxTrace.Exception.AsError(new InvalidOperationException(SA.SR.CannotUnregisterNullBookmarkScope));
            }

            if (toUnregister.Equals(context.DefaultBookmarkScope))
            {
                throw SA.FxTrace.Exception.AsError(new InvalidOperationException(SA.SR.CannotUnregisterDefaultBookmarkScope));
            }

            context.UnregisterBookmarkScope(toUnregister);
        }
    }
}
