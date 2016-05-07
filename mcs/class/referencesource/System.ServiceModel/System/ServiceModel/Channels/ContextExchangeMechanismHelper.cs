//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System;

    static class ContextExchangeMechanismHelper
    {
        public static bool IsDefined(ContextExchangeMechanism value)
        {
            return value == ContextExchangeMechanism.ContextSoapHeader ||
                value == ContextExchangeMechanism.HttpCookie;
        }
    }
}
