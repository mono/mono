//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    public enum SupportedAddressingMode
    {
        Anonymous,
        NonAnonymous,
        Mixed
    }

    static class SupportedAddressingModeHelper
    {
        internal static bool IsDefined(SupportedAddressingMode value)
        {
            return (value == SupportedAddressingMode.Anonymous ||
                value == SupportedAddressingMode.NonAnonymous ||
                value == SupportedAddressingMode.Mixed);
        }
    }
}
