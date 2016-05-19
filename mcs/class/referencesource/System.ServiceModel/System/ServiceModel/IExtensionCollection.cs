//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    public interface IExtensionCollection<T> : ICollection<IExtension<T>>
    where T : IExtensibleObject<T>
    {
        E Find<E>();
        Collection<E> FindAll<E>();
    }
}
