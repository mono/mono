//----------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    enum ReceiveSecurityHeaderElementCategory
    {
        Signature,
        EncryptedData,
        EncryptedKey,
        SignatureConfirmation,
        ReferenceList,
        SecurityTokenReference,
        Timestamp,
        Token
    }
}
