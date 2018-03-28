//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel
{
    using System.Xml;

    interface ISignatureValueSecurityElement : ISecurityElement
    {
        byte[] GetSignatureValue();
    }
}
