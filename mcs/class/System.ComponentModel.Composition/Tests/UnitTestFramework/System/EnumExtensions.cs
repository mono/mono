// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
#if !CLR40
using System;

namespace System
{
    public static class EnumExtensions
    {
        public static bool HasFlag(this Enum enumRef, Enum flag)
        {
            long value = Convert.ToInt64(enumRef);
            long flagVal = Convert.ToInt64(flag);

            return (value & flagVal) == flagVal;
        }
    }
}
#endif
