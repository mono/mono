//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    using System.IdentityModel;
    using System.IdentityModel.Selectors;
    using System.Xml.Serialization;

    public abstract class SamlCondition
    {
        public abstract bool IsReadOnly
        {
            get;
        }
        public abstract void MakeReadOnly();
        public abstract void ReadXml(System.Xml.XmlDictionaryReader reader, SamlSerializer samlSerializer, SecurityTokenSerializer keyInfoSerializer, SecurityTokenResolver outOfBandTokenResolver);
        public abstract void WriteXml(System.Xml.XmlDictionaryWriter writer, SamlSerializer samlSerializer, SecurityTokenSerializer keyInfoSerializer);
    }
}
