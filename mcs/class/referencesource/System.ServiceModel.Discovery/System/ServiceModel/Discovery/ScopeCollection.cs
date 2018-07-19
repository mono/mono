//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery
{
    using System;
    using SR2 = System.ServiceModel.Discovery.SR;

    // Scope collection is used to store a set of absolute URI's. It
    // overrides the InsertItem and SetItem to check for relative URI's.
    // An exception is thrown when a relative URI is found.
    internal class ScopeCollection : NonNullItemCollection<Uri>
    {
        protected override void InsertItem(int index, Uri item)
        {
            if (item != null && !item.IsAbsoluteUri)
            {
                throw FxTrace.Exception.Argument("item", SR2.DiscoveryArgumentInvalidScopeUri(item));
            }
            base.InsertItem(index, item);
        }

        protected override void SetItem(int index, Uri item)
        {
            if (item != null && !item.IsAbsoluteUri)
            {
                throw FxTrace.Exception.Argument("item", SR2.DiscoveryArgumentInvalidScopeUri(item));
            }
            base.SetItem(index, item);
        }
    }
}
