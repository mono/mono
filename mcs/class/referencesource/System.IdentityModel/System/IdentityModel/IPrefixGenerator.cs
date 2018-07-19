//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel
{
    interface IPrefixGenerator
    {
        string GetPrefix(string namespaceUri, int depth, bool isForAttribute);
    }
}
