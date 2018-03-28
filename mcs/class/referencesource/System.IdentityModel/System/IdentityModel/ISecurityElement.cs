//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel
{
    using System.Xml;

    interface ISecurityElement
    {
        bool HasId { get; }

        string Id { get; }

        void WriteTo(XmlDictionaryWriter writer, DictionaryManager dictionaryManager);
    }
}
