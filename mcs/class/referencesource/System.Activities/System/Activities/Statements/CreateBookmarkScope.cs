//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Statements
{
    using System;
    using System.Activities;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Diagnostics.CodeAnalysis;
    using System.Collections.Generic;

    public sealed class CreateBookmarkScope : NativeActivity<BookmarkScope>
    {
        public CreateBookmarkScope()
        {
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            // NoOp - we override this to suppress reflection. The base class
            // takes care of adding the Result argument.
        }

        protected override void Execute(NativeActivityContext context)
        {
            context.SetValue(this.Result, context.CreateBookmarkScope());
        }
    }
}
