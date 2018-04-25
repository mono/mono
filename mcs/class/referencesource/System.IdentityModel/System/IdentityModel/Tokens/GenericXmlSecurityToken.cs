//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    using System;    
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IO;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Policy;
    using System.Security.Cryptography;
    using System.Security.Principal;
    using System.Xml;
    using System.Runtime.Serialization;    
    using System.Collections.Generic;

    public class GenericXmlSecurityToken : SecurityToken
    {
        const int SupportedPersistanceVersion = 1;
        string id;
        SecurityToken proofToken;
        SecurityKeyIdentifierClause internalTokenReference;
        SecurityKeyIdentifierClause externalTokenReference;
        XmlElement tokenXml;
        ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies;
        DateTime effectiveTime;
        DateTime expirationTime;

        public GenericXmlSecurityToken(
            XmlElement tokenXml,
            SecurityToken proofToken,
            DateTime effectiveTime,
            DateTime expirationTime,
            SecurityKeyIdentifierClause internalTokenReference,
            SecurityKeyIdentifierClause externalTokenReference,
            ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies
            )
        {
            if (tokenXml == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("tokenXml");
            }

            this.id = GetId(tokenXml);
            this.tokenXml = tokenXml;
            this.proofToken = proofToken;
            this.effectiveTime = effectiveTime.ToUniversalTime();
            this.expirationTime = expirationTime.ToUniversalTime();

            this.internalTokenReference = internalTokenReference;
            this.externalTokenReference = externalTokenReference;
            this.authorizationPolicies = authorizationPolicies ?? EmptyReadOnlyCollection<IAuthorizationPolicy>.Instance;
        }

        public override string Id
        {
            get { return this.id; }
        }

        public override DateTime ValidFrom
        {
            get { return this.effectiveTime; }
        }

        public override DateTime ValidTo
        {
            get { return this.expirationTime; }
        }

        public SecurityKeyIdentifierClause InternalTokenReference
        {
            get { return this.internalTokenReference; }
        }

        public SecurityKeyIdentifierClause ExternalTokenReference
        {
            get { return this.externalTokenReference; }
        }

        public XmlElement TokenXml
        {
            get { return this.tokenXml;  }
        }

        public SecurityToken ProofToken
        {
            get { return this.proofToken; }
        }

        public ReadOnlyCollection<IAuthorizationPolicy> AuthorizationPolicies
        {
            get { return this.authorizationPolicies; }
        }

        public override ReadOnlyCollection<SecurityKey> SecurityKeys
        {
            get 
            {
                if (this.proofToken != null)
                    return this.proofToken.SecurityKeys;
                else
                    return EmptyReadOnlyCollection<SecurityKey>.Instance;
            }
        }
 
        public override string ToString()
        {
            StringWriter writer = new StringWriter(CultureInfo.InvariantCulture);
            writer.WriteLine("Generic XML token:");
            writer.WriteLine("   validFrom: {0}", this.ValidFrom);
            writer.WriteLine("   validTo: {0}", this.ValidTo);
            if (this.internalTokenReference != null)
                writer.WriteLine("   InternalTokenReference: {0}", this.internalTokenReference);
            if (this.externalTokenReference != null)
                writer.WriteLine("   ExternalTokenReference: {0}", this.externalTokenReference);
            writer.WriteLine("   Token Element: ({0}, {1})", this.tokenXml.LocalName, this.tokenXml.NamespaceURI);
            return writer.ToString();
        }

        static string GetId(XmlElement tokenXml)
        {
            if (tokenXml != null)
            {
                string id = tokenXml.GetAttribute(UtilityStrings.IdAttribute, UtilityStrings.Namespace);
                if ( string.IsNullOrEmpty( id ) )
                {
                    // special case SAML 1.1 as this is the only possible ID as
                    // spec is closed.  SAML 2.0 is xs:ID
                    id = tokenXml.GetAttribute("AssertionID");

                    // if we are still null, "Id"
                    if ( string.IsNullOrEmpty( id ) )
                    {
                        id = tokenXml.GetAttribute("Id");
                    }

                    //This fixes the unecnrypted SAML 2.0 case. Eg: <Assertion ID="_05955298-214f-41e7-b4c3-84dbff7f01b9" 
                    if (string.IsNullOrEmpty(id))
                    {
                        id = tokenXml.GetAttribute("ID");
                    }
                }

                if ( !string.IsNullOrEmpty(id) )
                {
                    return id;
                }
            }

            return null;
        }

        public override bool CanCreateKeyIdentifierClause<T>()
        {
            if (this.internalTokenReference != null && typeof(T) == this.internalTokenReference.GetType())
                return true;

            if (this.externalTokenReference != null && typeof(T) == this.externalTokenReference.GetType())
                return true;

            return false;
        }

        public override T CreateKeyIdentifierClause<T>()
        {
            if (this.internalTokenReference != null && typeof(T) == this.internalTokenReference.GetType())
                return (T)this.internalTokenReference;

            if (this.externalTokenReference != null && typeof(T) == this.externalTokenReference.GetType())
                return (T)this.externalTokenReference;

            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.UnableToCreateTokenReference)));
        }

        public override bool MatchesKeyIdentifierClause(SecurityKeyIdentifierClause keyIdentifierClause)
        {
            if (this.internalTokenReference != null && this.internalTokenReference.Matches(keyIdentifierClause))
            {
                return true;
            }
            else if (this.externalTokenReference != null && this.externalTokenReference.Matches(keyIdentifierClause))
            {
                return true;
            }
            
            return false;
        }
    }
}
