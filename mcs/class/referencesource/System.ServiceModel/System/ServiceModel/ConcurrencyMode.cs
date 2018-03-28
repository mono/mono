//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel
{
    using System;

    public enum ConcurrencyMode
    {
        Single, // This is first so it is ConcurrencyMode.default
        Reentrant,
        Multiple
    }

    static class ConcurrencyModeHelper
    {
        static public bool IsDefined(ConcurrencyMode x)
        {
            return
                x == ConcurrencyMode.Single ||
                x == ConcurrencyMode.Reentrant ||
                x == ConcurrencyMode.Multiple ||
                false;
        }
    }
}
