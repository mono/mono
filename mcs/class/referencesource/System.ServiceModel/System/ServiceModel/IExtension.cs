//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel
{
    using System;

    public interface IExtension<T> where T : IExtensibleObject<T>
    {
        void Attach(T owner);
        void Detach(T owner);
    }
}
