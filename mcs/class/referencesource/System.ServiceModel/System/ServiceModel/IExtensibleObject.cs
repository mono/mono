//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel
{
    using System;
    using System.Collections.Generic;

    public interface IExtensibleObject<T>
    where T : IExtensibleObject<T>
    {
        IExtensionCollection<T> Extensions { get; }
    }
}
