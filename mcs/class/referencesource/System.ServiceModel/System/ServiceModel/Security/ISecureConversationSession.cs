//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------
namespace System.ServiceModel.Security
{
    using System.Xml;
    using System.ServiceModel;

    public interface ISecureConversationSession : ISecuritySession
    {
        void WriteSessionTokenIdentifier(XmlDictionaryWriter writer);
        bool TryReadSessionTokenIdentifier(XmlReader reader);
    }
}
