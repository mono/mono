//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Description
{
    using System;

    public enum ListenUriMode
    {
        Explicit,
        Unique,
    }

    internal static class ListenUriModeHelper
    {
        static public bool IsDefined(ListenUriMode mode)
        {
            return mode == ListenUriMode.Explicit
                || mode == ListenUriMode.Unique;
        }
    }
}
