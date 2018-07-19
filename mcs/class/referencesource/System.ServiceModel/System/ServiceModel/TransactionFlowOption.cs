//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel
{
    public enum TransactionFlowOption
    {
        NotAllowed,
        Allowed,
        Mandatory,
    }

    static class TransactionFlowOptionHelper
    {
        public static bool IsDefined(TransactionFlowOption option)
        {
            return (option == TransactionFlowOption.NotAllowed ||
                    option == TransactionFlowOption.Allowed ||
                    option == TransactionFlowOption.Mandatory);
            //option == TransactionFlowOption.Ignore);
        }
        internal static bool AllowedOrRequired(TransactionFlowOption option)
        {
            return (option == TransactionFlowOption.Allowed ||
                    option == TransactionFlowOption.Mandatory);
        }
    }
}
