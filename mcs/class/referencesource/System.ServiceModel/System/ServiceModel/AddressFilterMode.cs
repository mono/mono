//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel
{
    using System;

    public enum AddressFilterMode
    {
        Exact, // AddressFilterMode.default
        Prefix,
        Any,
    }

    static class AddressFilterModeHelper
    {
        static public bool IsDefined(AddressFilterMode x)
        {
            return
                x == AddressFilterMode.Exact ||
                x == AddressFilterMode.Prefix ||
                x == AddressFilterMode.Any;
        }
    }
}
