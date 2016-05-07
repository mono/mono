//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel
{
    using System;

    public enum ReleaseInstanceMode
    {
        None = 0,
        BeforeCall = 1,
        AfterCall = 2,
        BeforeAndAfterCall = 3,
    }

    static class ReleaseInstanceModeHelper
    {
        static public bool IsDefined(ReleaseInstanceMode x)
        {
            return
                x == ReleaseInstanceMode.None ||
                x == ReleaseInstanceMode.BeforeCall ||
                x == ReleaseInstanceMode.AfterCall ||
                x == ReleaseInstanceMode.BeforeAndAfterCall ||
                false;
        }
    }

}
