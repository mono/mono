//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System;
    using System.ServiceModel;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Xml;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Policy;
    using System.IdentityModel.Tokens;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel.Security.Tokens;
    using HexBinary = System.Runtime.Remoting.Metadata.W3cXsd2001.SoapHexBinary;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security;
    using System.Runtime.Serialization;

    using KeyIdentifierEntry = WSSecurityTokenSerializer.KeyIdentifierEntry;
    using KeyIdentifierClauseEntry = WSSecurityTokenSerializer.KeyIdentifierClauseEntry;
    using TokenEntry = WSSecurityTokenSerializer.TokenEntry;
    using StrEntry = WSSecurityTokenSerializer.StrEntry;

    class WSSecurityXXX2005 : WSSecurityJan2004
    {
        public WSSecurityXXX2005(WSSecurityTokenSerializer tokenSerializer, SamlSerializer samlSerializer)
            : base(tokenSerializer, samlSerializer)
        {
        }

        public override void PopulateTokenEntries(IList<TokenEntry> tokenEntryList)
        {
            PopulateJan2004TokenEntries(tokenEntryList);
            tokenEntryList.Add(new WSSecurityXXX2005.WrappedKeyTokenEntry(this.WSSecurityTokenSerializer));
            tokenEntryList.Add(new WSSecurityXXX2005.SamlTokenEntry(this.WSSecurityTokenSerializer, this.SamlSerializer));
        }

        new class SamlTokenEntry : WSSecurityJan2004.SamlTokenEntry
        {
            public SamlTokenEntry(WSSecurityTokenSerializer tokenSerializer, SamlSerializer samlSerializer)
                : base(tokenSerializer, samlSerializer)
            {
            }

            public override string TokenTypeUri { get { return SecurityXXX2005Strings.SamlTokenType; } }
        }

        new class WrappedKeyTokenEntry : WSSecurityJan2004.WrappedKeyTokenEntry
        {
            public WrappedKeyTokenEntry(WSSecurityTokenSerializer tokenSerializer)
                : base(tokenSerializer)
            {
            }

            public override string TokenTypeUri { get { return SecurityXXX2005Strings.EncryptedKeyTokenType; } }
        }

    }
}
