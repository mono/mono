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
    using HexBinary = System.Runtime.Remoting.Metadata.W3cXsd2001.SoapHexBinary;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security;
    using System.ServiceModel.Security.Tokens;
    using System.Runtime.Serialization;
    using KeyIdentifierEntry = WSSecurityTokenSerializer.KeyIdentifierEntry;
    using KeyIdentifierClauseEntry = WSSecurityTokenSerializer.KeyIdentifierClauseEntry;
    using StrEntry = WSSecurityTokenSerializer.StrEntry;
    using TokenEntry = WSSecurityTokenSerializer.TokenEntry;
    
    class WSSecureConversationFeb2005 : WSSecureConversation
    {
        SecurityStateEncoder securityStateEncoder;
        IList<Type> knownClaimTypes;

        public WSSecureConversationFeb2005(WSSecurityTokenSerializer tokenSerializer, SecurityStateEncoder securityStateEncoder, IEnumerable<Type> knownTypes,
            int maxKeyDerivationOffset, int maxKeyDerivationLabelLength, int maxKeyDerivationNonceLength)
            : base(tokenSerializer, maxKeyDerivationOffset, maxKeyDerivationLabelLength, maxKeyDerivationNonceLength)
        {
            if (securityStateEncoder != null)
            {
                this.securityStateEncoder = securityStateEncoder;
            }
            else
            {
                this.securityStateEncoder = new DataProtectionSecurityStateEncoder();
            }

            this.knownClaimTypes = new List<Type>();
            if (knownTypes != null)
            {
                // Clone this collection.
                foreach (Type knownType in knownTypes)
                {
                    this.knownClaimTypes.Add(knownType);
                }
            }
        }

        public override SecureConversationDictionary SerializerDictionary
        {
            get { return XD.SecureConversationFeb2005Dictionary; }
        }
        
        public override void PopulateTokenEntries(IList<TokenEntry> tokenEntryList)
        {
            base.PopulateTokenEntries(tokenEntryList);
            tokenEntryList.Add(new SecurityContextTokenEntryFeb2005(this, this.securityStateEncoder, this.knownClaimTypes));
        }

        class SecurityContextTokenEntryFeb2005 : SecurityContextTokenEntry
        {
            public SecurityContextTokenEntryFeb2005(WSSecureConversationFeb2005 parent, SecurityStateEncoder securityStateEncoder, IList<Type> knownClaimTypes)
                : base(parent, securityStateEncoder, knownClaimTypes)
            {
            }
            
            protected override bool CanReadGeneration(XmlDictionaryReader reader)
            {
                return reader.IsStartElement(DXD.SecureConversationDec2005Dictionary.Instance, XD.SecureConversationFeb2005Dictionary.Namespace);
            }
            
            protected override bool CanReadGeneration(XmlElement element)
            {
                return (element.LocalName == DXD.SecureConversationDec2005Dictionary.Instance.Value &&
                    element.NamespaceURI == XD.SecureConversationFeb2005Dictionary.Namespace.Value);
            }
            
            protected override UniqueId ReadGeneration(XmlDictionaryReader reader)
            {
                return reader.ReadElementContentAsUniqueId();
            }

            protected override UniqueId ReadGeneration(XmlElement element)
            {
                return XmlHelper.ReadTextElementAsUniqueId(element);
            }
            
            protected override void WriteGeneration(XmlDictionaryWriter writer, SecurityContextSecurityToken sct)
            {
                // serialize the generation
                if (sct.KeyGeneration != null)
                {
                    writer.WriteStartElement(XD.SecureConversationFeb2005Dictionary.Prefix.Value, DXD.SecureConversationDec2005Dictionary.Instance,
                        XD.SecureConversationFeb2005Dictionary.Namespace);
                    XmlHelper.WriteStringAsUniqueId(writer, sct.KeyGeneration);
                    writer.WriteEndElement();
                }
            }
        }

        public class DriverFeb2005 : Driver
        {
            public DriverFeb2005()
            {
            }

            protected override SecureConversationDictionary DriverDictionary
            {
                get { return XD.SecureConversationFeb2005Dictionary; }
            }

            public override XmlDictionaryString CloseAction
            {
                get { return XD.SecureConversationFeb2005Dictionary.RequestSecurityContextClose; }
            }

            public override XmlDictionaryString CloseResponseAction
            {
                get { return XD.SecureConversationFeb2005Dictionary.RequestSecurityContextCloseResponse; }
            }

            public override bool IsSessionSupported
            {
                get { return true; }
            }

            public override XmlDictionaryString RenewAction
            {
                get { return XD.SecureConversationFeb2005Dictionary.RequestSecurityContextRenew; }
            }

            public override XmlDictionaryString RenewResponseAction
            {
                get { return XD.SecureConversationFeb2005Dictionary.RequestSecurityContextRenewResponse; }
            }

            public override XmlDictionaryString Namespace
            {
                get { return XD.SecureConversationFeb2005Dictionary.Namespace; }
            }

            public override string TokenTypeUri
            {
                get { return XD.SecureConversationFeb2005Dictionary.SecurityContextTokenType.Value; }
            }
        }
    }
}
