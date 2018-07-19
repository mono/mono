//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel
{
    public enum ReceiveErrorHandling
    {
        Fault,
        Drop,
        Reject,
        Move
    }

    static class ReceiveErrorHandlingHelper
    {
        internal static bool IsDefined(ReceiveErrorHandling value)
        {
            return value == ReceiveErrorHandling.Fault ||
                value == ReceiveErrorHandling.Drop ||
                value == ReceiveErrorHandling.Reject ||
                value == ReceiveErrorHandling.Move;
        }
    }
}
