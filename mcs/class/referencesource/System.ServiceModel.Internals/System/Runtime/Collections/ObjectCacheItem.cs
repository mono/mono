//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Runtime.Collections
{
    abstract class ObjectCacheItem<T>
        where T : class
    {
        // only valid when you've called TryAddReference successfully
        public abstract T Value { get; }
        public abstract bool TryAddReference();
        public abstract void ReleaseReference();
    }
}
