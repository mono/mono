//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.Security
{
    public enum MessageProtectionOrder
    {
        SignBeforeEncrypt,
        SignBeforeEncryptAndEncryptSignature,
        EncryptBeforeSign,
    }

    static class MessageProtectionOrderHelper
    {
        internal static bool IsDefined(MessageProtectionOrder value)
        {
            return value == MessageProtectionOrder.SignBeforeEncrypt
                || value == MessageProtectionOrder.SignBeforeEncryptAndEncryptSignature
                || value == MessageProtectionOrder.EncryptBeforeSign;
        }
    }
}
