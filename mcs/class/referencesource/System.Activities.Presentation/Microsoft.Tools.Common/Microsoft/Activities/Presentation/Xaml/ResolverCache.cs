//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace Microsoft.Activities.Presentation.Xaml
{
    using System;
    using System.Collections.Generic;

    internal class ResolverCache
    {
        private Dictionary<Type, WeakReference> cache;

        public ResolverCache()
        {
            this.cache = new Dictionary<Type, WeakReference>();
        }

        public void Update(Type type, ResolverResult result)
        {
            SharedFx.Assert(type != null, "type should not be null");
            SharedFx.Assert(result != null, "result should not be null");

            if (this.cache.ContainsKey(type))
            {
                this.cache[type] = new WeakReference(result);
            }
            else
            {
                this.cache.Add(type, new WeakReference(result));
            }
        }

        public ResolverResult Lookup(Type type)
        {
            SharedFx.Assert(type != null, "type should not be null");

            WeakReference value;
            if (this.cache.TryGetValue(type, out value))
            {
                return value.Target as ResolverResult;
            }

            return null;
        }
    }
}
