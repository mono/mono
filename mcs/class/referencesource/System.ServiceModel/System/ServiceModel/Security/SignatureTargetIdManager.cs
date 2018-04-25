//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.Xml;

    abstract class SignatureTargetIdManager
    {
        protected SignatureTargetIdManager()
        {
        }

        public abstract string DefaultIdNamespacePrefix { get; }

        public abstract string DefaultIdNamespaceUri { get; }

        public abstract string ExtractId(XmlDictionaryReader reader);

        public abstract void WriteIdAttribute(XmlDictionaryWriter writer, string id);
    }
}
