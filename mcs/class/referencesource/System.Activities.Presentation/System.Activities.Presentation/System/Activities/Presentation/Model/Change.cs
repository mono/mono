//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.Model
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public abstract class Change
    {
        public abstract string Description { get; }
        public abstract bool Apply();
        public abstract Change GetInverse();
    }
}
