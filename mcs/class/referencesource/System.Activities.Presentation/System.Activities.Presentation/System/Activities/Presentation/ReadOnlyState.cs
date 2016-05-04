//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.Hosting
{
    using System.Activities.Presentation.View;

    public sealed class ReadOnlyState : ContextItem
    {
        public bool IsReadOnly { get; set; }

        public override Type ItemType
        {
            get
            {
                return typeof(ReadOnlyState);
            }
        }
    }
}
