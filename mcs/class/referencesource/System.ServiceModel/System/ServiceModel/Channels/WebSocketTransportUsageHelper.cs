// <copyright>
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.ServiceModel.Channels
{
    using System.ComponentModel;
    
    static class WebSocketTransportUsageHelper
    {
        internal static bool IsDefined(WebSocketTransportUsage value)
        {
            return value == WebSocketTransportUsage.WhenDuplex
                || value == WebSocketTransportUsage.Never
                || value == WebSocketTransportUsage.Always;
        }

        internal static void Validate(WebSocketTransportUsage value)
        {
            if (!IsDefined(value))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new InvalidEnumArgumentException("value", (int)value, typeof(WebSocketTransportUsage)));
            }
        }
    }
}
