//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public interface ICompositeViewEvents
    {
        void RegisterDefaultCompositeView(ICompositeView container);
        void UnregisterDefaultCompositeView(ICompositeView container);
        void RegisterCompositeView(ICompositeView container);
        void UnregisterCompositeView(ICompositeView container);

    }
}
