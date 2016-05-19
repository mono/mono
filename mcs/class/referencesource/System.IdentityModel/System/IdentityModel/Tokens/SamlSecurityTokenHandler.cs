//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IdentityModel.Configuration;
    using System.IdentityModel.Diagnostics;
    using System.IdentityModel.Protocols.WSTrust;
    using System.IdentityModel.Selectors;
    using System.IO;
    using System.Linq;
    using System.Runtime;
    using System.Security.Claims;
    using System.Security.Cryptography;
    using System.Security.Principal;
    using System.Text;
    using System.Xml;
    using System.Xml.Schema;
    using Claim = System.Security.Claims.Claim;
    using ClaimTypes = System.Security.Claims.ClaimTypes;

    /// <summary>
    /// This class implements a SecurityTokenHandler for a Saml11 token.  It contains functionality for: Creating, Serializing and Validating 
    /// a Saml 11 Token.
    /// </summary>
    public class SamlSecurityTokenHandler : SecurityTokenHandler
    {
#pragma warning disable 1591
        public const string Namespace = "urn:oasis:names:tc:SAML:1.0";
        public const string BearerConfirmationMethod = Namespace + ":cm:bearer";
        public const string UnspecifiedAuthenticationMethod = Namespace + ":am:unspecified";
        public const string Assertion = Namespace + ":assertion";
#pragma warning restore 1591

        const string Attribute = "saml:Attribute";
        const string Actor = "Actor";
        const string ClaimType2009Namespace = "http://schemas.xmlsoap.org/ws/2009/09/identity/claims";

        // Below are WCF DateTime values for Min and Max. SamlConditions when new'ed up will
        // have these values as default. To maintin compatability with WCF behavior we will 
        // not write out SamlConditions NotBefore and NotOnOrAfter times which match the below
        // values.
        static DateTime WCFMinValue = new DateTime(DateTime.MinValue.Ticks + TimeSpan.TicksPerDay, DateTimeKind.Utc);
        static DateTime WCFMaxValue = new DateTime(DateTime.MaxValue.Ticks - TimeSpan.TicksPerDay, DateTimeKind.Utc);

        static string[] _tokenTypeIdentifiers = new string[] { SecurityTokenTypes.SamlTokenProfile11, SecurityTokenTypes.OasisWssSamlTokenProfile11 };

        SamlSecurityTokenRequirement _samlSecurityTokenRequirement;

        SecurityTokenSerializer _keyInfoSerializer;

        object _syncObject = new object();

        /// <summary>
        /// Initializes an instance of <see cref="SamlSecurityTokenHandler"/>
        /// </summary>
        public SamlSecurityTokenHandler()
            : this(new SamlSecurityTokenRequirement())
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="SamlSecurityTokenHandler"/>
        /// </summary>
        /// <param name="samlSecurityTokenRequirement">The SamlSecurityTokenRequirement to be used by the Saml11SecurityTokenHandler instance when validating tokens.</param>
        public SamlSecurityTokenHandler(SamlSecurityTokenRequirement samlSecurityTokenRequirement)
        {
            if (samlSecurityTokenRequirement == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("samlSecurityTokenRequirement");
            }
            _samlSecurityTokenRequirement = samlSecurityTokenRequirement;
        }

        /// <summary>
        /// Load custom configuration from Xml
        /// </summary>
        /// <param name="customConfigElements">Custom configuration that describes SamlSecurityTokenRequirement.</param>
        /// <exception cref="ArgumentNullException">Input parameter 'customConfigElements' is null.</exception>
        /// <exception cref="InvalidOperationException">Custom configuration specified was invalid.</exception>
        public override void LoadCustomConfiguration(XmlNodeList customConfigElements)
        {
            if (customConfigElements == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("customConfigElements");
            }

            List<XmlElement> configNodes = XmlUtil.GetXmlElements(customConfigElements);

            bool foundValidConfig = false;

            foreach (XmlElement configElement in configNodes)
            {
                if (configElement.LocalName != ConfigurationStrings.SamlSecurityTokenRequirement)
                {
                    continue;
                }

                if (foundValidConfig)
                {
                    throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID7026, ConfigurationStrings.SamlSecurityTokenRequirement));
                }

                _samlSecurityTokenRequirement = new SamlSecurityTokenRequirement(configElement);

                foundValidConfig = true;
            }

            if (!foundValidConfig)
            {
                _samlSecurityTokenRequirement = new SamlSecurityTokenRequirement();
            }
        }

        #region TokenCreation

        /// <summary>
        /// Creates the security token based on the tokenDescriptor passed in.
        /// </summary>
        /// <param name="tokenDescriptor">The security token descriptor that contains the information to build a token.</param>
        /// <exception cref="ArgumentNullException">Thrown if 'tokenDescriptor' is null.</exception>
        public override SecurityToken CreateToken(SecurityTokenDescriptor tokenDescriptor)
        {
            if (tokenDescriptor == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("tokenDescriptor");
            }

            IEnumerable<SamlStatement> statements = CreateStatements(tokenDescriptor);

            // - NotBefore / NotAfter
            // - Audience Restriction
            SamlConditions conditions = CreateConditions(tokenDescriptor.Lifetime, tokenDescriptor.AppliesToAddress, tokenDescriptor);

            SamlAdvice advice = CreateAdvice(tokenDescriptor);

            string issuerName = tokenDescriptor.TokenIssuerName;

            SamlAssertion assertion = CreateAssertion(issuerName, conditions, advice, statements);
            if (assertion == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ID4013)));
            }

            assertion.SigningCredentials = GetSigningCredentials(tokenDescriptor);

            SecurityToken token = new SamlSecurityToken(assertion);

            //
            // Encrypt the token if encrypting credentials are set
            //

            EncryptingCredentials encryptingCredentials = GetEncryptingCredentials(tokenDescriptor);
            if (encryptingCredentials != null)
            {
                token = new EncryptedSecurityToken(token, encryptingCredentials);
            }

            return token;
        }

        /// <summary>
        /// Gets the credentials for encrypting the token.  Override this method to provide custom encrypting credentials. 
        /// </summary>
        /// <param name="tokenDescriptor">The Scope property provides access to the encrypting credentials.</param>
        /// <returns>The token encrypting credentials.</returns>
        /// <exception cref="ArgumentNullException">Thrown when 'tokenDescriptor' is null.</exception>
        /// <remarks>The default behavior is to return the SecurityTokenDescriptor.Scope.EncryptingCredentials
        /// If this key is ----ymmetric, a symmetric key will be generated and wrapped with the asymmetric key.</remarks>
        protected virtual EncryptingCredentials GetEncryptingCredentials(SecurityTokenDescriptor tokenDescriptor)
        {
            if (null == tokenDescriptor)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("tokenDescriptor");
            }

            EncryptingCredentials encryptingCredentials = null;

            if (null != tokenDescriptor.EncryptingCredentials)
            {
                encryptingCredentials = tokenDescriptor.EncryptingCredentials;

                if (encryptingCredentials.SecurityKey is AsymmetricSecurityKey)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                                        new SecurityTokenException(SR.GetString(SR.ID4178)));
                }
            }

            return encryptingCredentials;
        }

        /// <summary>
        /// Gets the credentials for the signing the assertion.  Override this method to provide custom signing credentials.
        /// </summary>
        /// <param name="tokenDescriptor">The Scope property provides access to the signing credentials.</param>
        /// <exception cref="ArgumentNullException">Thrown when 'tokenDescriptor' is null.</exception>
        /// <returns>The assertion signing credentials.</returns>
        /// <remarks>The default behavior is to return the SecurityTokenDescriptor.Scope.SigningCredentials.</remarks>
        protected virtual SigningCredentials GetSigningCredentials(SecurityTokenDescriptor tokenDescriptor)
        {
            if (null == tokenDescriptor)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("tokenDescriptor");
            }

            return tokenDescriptor.SigningCredentials;
        }

        /// <summary>
        /// Override this method to provide a SamlAdvice to place in the Samltoken. 
        /// </summary>
        /// <param name="tokenDescriptor">Contains informaiton about the token.</param>
        /// <returns>SamlAdvice, default is null.</returns>
        protected virtual SamlAdvice CreateAdvice(SecurityTokenDescriptor tokenDescriptor)
        {
            return null;
        }

        /// <summary>
        /// Override this method to customize the parameters to create a SamlAssertion. 
        /// </summary>
        /// <param name="issuer">The Issuer of the Assertion.</param>
        /// <param name="conditions">The SamlConditions to add.</param>
        /// <param name="advice">The SamlAdvice to add.</param>
        /// <param name="statements">The SamlStatements to add.</param>
        /// <returns>A SamlAssertion.</returns>
        /// <remarks>A unique random id is created for the assertion
        /// IssueInstance is set to DateTime.UtcNow.</remarks>
        protected virtual SamlAssertion CreateAssertion(string issuer, SamlConditions conditions, SamlAdvice advice, IEnumerable<SamlStatement> statements)
        {
            return new SamlAssertion(System.IdentityModel.UniqueId.CreateRandomId(), issuer, DateTime.UtcNow, conditions, advice, statements);
        }

        /// <summary>
        /// Creates the security token reference when the token is not attached to the message.
        /// </summary>
        /// <param name="token">The saml token.</param>
        /// <param name="attached">Boolean that indicates if a attached or unattached
        /// reference needs to be created.</param>
        /// <returns>A SamlAssertionKeyIdentifierClause.</returns>
        public override SecurityKeyIdentifierClause CreateSecurityTokenReference(SecurityToken token, bool attached)
        {
            if (token == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("token");
            }

            return token.CreateKeyIdentifierClause<SamlAssertionKeyIdentifierClause>();
        }

        /// <summary>
        /// Generates all the conditions for saml
        /// 
        /// 1. Lifetime condition
        /// 2. AudienceRestriction condition
        /// 
        /// </summary>
        /// <param name="tokenLifetime">Lifetime of the Token.</param>
        /// <param name="relyingPartyAddress">The endpoint address to who the token is created. The address
        /// is modelled as an AudienceRestriction condition.</param>
        /// <param name="tokenDescriptor">Contains all the other information that is used in token issuance.</param>
        /// <returns>SamlConditions</returns>
        protected virtual SamlConditions CreateConditions(Lifetime tokenLifetime, string relyingPartyAddress, SecurityTokenDescriptor tokenDescriptor)
        {
            SamlConditions conditions = new SamlConditions();
            if (tokenLifetime != null)
            {
                if (tokenLifetime.Created != null)
                {
                    conditions.NotBefore = tokenLifetime.Created.Value;
                }

                if (tokenLifetime.Expires != null)
                {
                    conditions.NotOnOrAfter = tokenLifetime.Expires.Value;
                }
            }

            if (!string.IsNullOrEmpty(relyingPartyAddress))
            {
                conditions.Conditions.Add(new SamlAudienceRestrictionCondition(new Uri[] { new Uri(relyingPartyAddress) }));
            }

            return conditions;
        }

        /// <summary>
        /// Generates an enumeration of SamlStatements from a SecurityTokenDescriptor.
        /// Only SamlAttributeStatements and SamlAuthenticationStatements are generated.
        /// Overwrite this method to customize the creation of statements.
        /// <para>
        /// Calls in order (all are virtual):
        /// 1. CreateSamlSubject
        /// 2. CreateAttributeStatements
        /// 3. CreateAuthenticationStatements
        /// </para>
        /// </summary>
        /// <param name="tokenDescriptor">The SecurityTokenDescriptor to use to build the statements.</param>
        /// <returns>An enumeration of SamlStatement.</returns>
        protected virtual IEnumerable<SamlStatement> CreateStatements(SecurityTokenDescriptor tokenDescriptor)
        {
            Collection<SamlStatement> statements = new Collection<SamlStatement>();

            SamlSubject subject = CreateSamlSubject(tokenDescriptor);
            SamlAttributeStatement attributeStatement = CreateAttributeStatement(subject, tokenDescriptor.Subject, tokenDescriptor);
            if (attributeStatement != null)
            {
                statements.Add(attributeStatement);
            }

            SamlAuthenticationStatement authnStatement = CreateAuthenticationStatement(subject, tokenDescriptor.AuthenticationInfo, tokenDescriptor);
            if (authnStatement != null)
            {
                statements.Add(authnStatement);
            }

            return statements;
        }

        /// <summary>
        /// Creates a SamlAuthenticationStatement for each AuthenticationInformation found in AuthenticationInformation. 
        /// Override this method to provide a custom implementation.
        /// </summary>
        /// <param name="samlSubject">The SamlSubject of the Statement.</param>
        /// <param name="authInfo">AuthenticationInformation from which to generate the SAML Authentication statement.</param>
        /// <param name="tokenDescriptor">Contains all the other information that is used in token issuance.</param>
        /// <returns>SamlAuthenticationStatement</returns>
        /// <exception cref="ArgumentNullException">Thrown when 'samlSubject' or 'authInfo' is null.</exception>
        protected virtual SamlAuthenticationStatement CreateAuthenticationStatement(
                                                                SamlSubject samlSubject,
                                                                AuthenticationInformation authInfo,
                                                                SecurityTokenDescriptor tokenDescriptor)
        {
            if (samlSubject == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("samlSubject");
            }

            if (tokenDescriptor == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("tokenDescriptor");
            }

            if (tokenDescriptor.Subject == null)
            {
                return null;
            }
            string authenticationMethod = null;
            string authenticationInstant = null;

            // Search for an Authentication Claim.
            IEnumerable<Claim> claimCollection = (from c in tokenDescriptor.Subject.Claims
                                                  where c.Type == ClaimTypes.AuthenticationMethod
                                                  select c);
            if (claimCollection.Count<Claim>() > 0)
            {
                // We support only one authentication statement and hence we just pick the first authentication type
                // claim found in the claim collection. Since the spec allows multiple Auth Statements 
                // we do not throw an error.
                authenticationMethod = claimCollection.First<Claim>().Value;
            }

            claimCollection = (from c in tokenDescriptor.Subject.Claims
                               where c.Type == ClaimTypes.AuthenticationInstant
                               select c);
            if (claimCollection.Count<Claim>() > 0)
            {
                authenticationInstant = claimCollection.First<Claim>().Value;
            }

            if (authenticationMethod == null && authenticationInstant == null)
            {
                return null;
            }
            else if (authenticationMethod == null)
            {
                throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID4270, "AuthenticationMethod", "SAML11"));
            }
            else if (authenticationInstant == null)
            {
                throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID4270, "AuthenticationInstant", "SAML11"));
            }

            DateTime authInstantTime = DateTime.ParseExact(authenticationInstant,
                                                            DateTimeFormats.Accepted,
                                                            DateTimeFormatInfo.InvariantInfo,
                                                            DateTimeStyles.None).ToUniversalTime();
            if (authInfo == null)
            {
                return new SamlAuthenticationStatement(samlSubject, DenormalizeAuthenticationType(authenticationMethod), authInstantTime, null, null, null);
            }
            else
            {
                return new SamlAuthenticationStatement(samlSubject, DenormalizeAuthenticationType(authenticationMethod), authInstantTime, authInfo.DnsName, authInfo.Address, null);
            }
        }

        /// <summary>
        /// Creates SamlAttributeStatements and adds them to a collection.
        /// Override this method to provide a custom implementation.
        /// <para>
        /// Default behavior is to create a new SamlAttributeStatement for each Subject in the tokenDescriptor.Subjects collection.
        /// </para>
        /// </summary>
        /// <param name="samlSubject">The SamlSubject to use in the SamlAttributeStatement that are created.</param>
        /// <param name="subject">The ClaimsIdentity that contains claims which will be converted to SAML Attributes.</param>
        /// <param name="tokenDescriptor">Contains all the other information that is used in token issuance.</param>
        /// <returns>SamlAttributeStatement</returns>
        /// <exception cref="ArgumentNullException">Thrown when 'samlSubject' is null.</exception>
        protected virtual SamlAttributeStatement CreateAttributeStatement(
            SamlSubject samlSubject,
            ClaimsIdentity subject,
            SecurityTokenDescriptor tokenDescriptor)
        {
            if (subject == null)
            {
                return null;
            }

            if (samlSubject == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("samlSubject");
            }

            if (subject.Claims != null)
            {

                List<SamlAttribute> attributes = new List<SamlAttribute>();
                foreach (Claim claim in subject.Claims)
                {
                    if (claim != null && claim.Type != ClaimTypes.NameIdentifier)
                    {
                        //
                        // NameIdentifier claim is already processed while creating the samlsubject
                        // AuthenticationInstant and AuthenticationType are not converted to Claims
                        //
                        switch (claim.Type)
                        {
                            case ClaimTypes.AuthenticationInstant:
                            case ClaimTypes.AuthenticationMethod:
                                break;
                            default:
                                attributes.Add(CreateAttribute(claim, tokenDescriptor));
                                break;
                        }
                    }
                }

                AddDelegateToAttributes(subject, attributes, tokenDescriptor);

                ICollection<SamlAttribute> collectedAttributes = CollectAttributeValues(attributes);
                if (collectedAttributes.Count > 0)
                {
                    return new SamlAttributeStatement(samlSubject, collectedAttributes);
                }
            }

            return null;
        }

        /// <summary>
        /// Collects attributes with a common claim type, claim value type, and original issuer into a
        /// single attribute with multiple values.
        /// </summary>
        /// <param name="attributes">List of attributes generated from claims.</param>
        /// <returns>List of attribute values with common attributes collected into value lists.</returns>
        protected virtual ICollection<SamlAttribute> CollectAttributeValues(ICollection<SamlAttribute> attributes)
        {
            Dictionary<SamlAttributeKeyComparer.AttributeKey, SamlAttribute> distinctAttributes = new Dictionary<SamlAttributeKeyComparer.AttributeKey, SamlAttribute>(attributes.Count, new SamlAttributeKeyComparer());

            foreach (SamlAttribute attribute in attributes)
            {
                SamlAttribute SamlAttribute = attribute as SamlAttribute;
                if (SamlAttribute != null)
                {
                    // Use unique attribute if name, value type, or issuer differ
                    SamlAttributeKeyComparer.AttributeKey attributeKey = new SamlAttributeKeyComparer.AttributeKey(SamlAttribute);

                    if (distinctAttributes.ContainsKey(attributeKey))
                    {
                        foreach (string attributeValue in SamlAttribute.AttributeValues)
                        {
                            distinctAttributes[attributeKey].AttributeValues.Add(attributeValue);
                        }
                    }
                    else
                    {
                        distinctAttributes.Add(attributeKey, SamlAttribute);
                    }
                }
            }

            return distinctAttributes.Values;
        }

        /// <summary>
        /// Adds all the delegates associated with the ActAs subject into the attribute collection.
        /// </summary>
        /// <param name="subject">The delegate of this ClaimsIdentity will be serialized into a SamlAttribute.</param>
        /// <param name="attributes">Attribute collection to which the ActAs token will be serialized.</param>
        /// <param name="tokenDescriptor">Contains all the information that is used in token issuance.</param>
        protected virtual void AddDelegateToAttributes(
            ClaimsIdentity subject,
            ICollection<SamlAttribute> attributes,
            SecurityTokenDescriptor tokenDescriptor)
        {
            if (subject == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("subject");
            }
            if (tokenDescriptor == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("tokenDescriptor");
            }
            if (subject.Actor == null)
            {
                return;
            }

            List<SamlAttribute> actingAsAttributes = new List<SamlAttribute>();

            foreach (Claim claim in subject.Actor.Claims)
            {
                if (claim != null)
                {
                    actingAsAttributes.Add(CreateAttribute(claim, tokenDescriptor));
                }
            }

            // perform depth first recursion
            AddDelegateToAttributes(subject.Actor, actingAsAttributes, tokenDescriptor);

            ICollection<SamlAttribute> collectedAttributes = CollectAttributeValues(actingAsAttributes);
            attributes.Add(CreateAttribute(new Claim(ClaimTypes.Actor, CreateXmlStringFromAttributes(collectedAttributes), ClaimValueTypes.String), tokenDescriptor));
        }

        /// <summary>
        /// Returns the SamlSubject to use for all the statements that will be created.
        /// Overwrite this method to customize the creation of the SamlSubject.
        /// </summary>
        /// <param name="tokenDescriptor">Contains all the information that is used in token issuance.</param>
        /// <returns>A SamlSubject created from the first subject found in the tokenDescriptor as follows:
        /// <para>
        /// 1. Claim of Type NameIdentifier is searched. If found, SamlSubject.Name is set to claim.Value.
        /// 2. If a non-null tokenDescriptor.proof is found then SamlSubject.KeyIdentifier = tokenDescriptor.Proof.KeyIdentifier AND SamlSubject.ConfirmationMethod is set to 'HolderOfKey'.
        /// 3. If a null tokenDescriptor.proof is found then SamlSubject.ConfirmationMethod is set to 'BearerKey'.
        /// </para>
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when 'tokenDescriptor' is null.</exception>
        protected virtual SamlSubject CreateSamlSubject(SecurityTokenDescriptor tokenDescriptor)
        {
            if (tokenDescriptor == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("tokenDescriptor");
            }

            SamlSubject samlSubject = new SamlSubject();

            Claim identityClaim = null;
            if (tokenDescriptor.Subject != null && tokenDescriptor.Subject.Claims != null)
            {
                foreach (Claim claim in tokenDescriptor.Subject.Claims)
                {
                    if (claim.Type == ClaimTypes.NameIdentifier)
                    {
                        // Do not allow multiple name identifier claim.
                        if (null != identityClaim)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                                new InvalidOperationException(SR.GetString(SR.ID4139)));
                        }
                        identityClaim = claim;
                    }
                }
            }

            if (identityClaim != null)
            {
                samlSubject.Name = identityClaim.Value;

                if (identityClaim.Properties.ContainsKey(ClaimProperties.SamlNameIdentifierFormat))
                {
                    samlSubject.NameFormat = identityClaim.Properties[ClaimProperties.SamlNameIdentifierFormat];
                }

                if (identityClaim.Properties.ContainsKey(ClaimProperties.SamlNameIdentifierNameQualifier))
                {
                    samlSubject.NameQualifier = identityClaim.Properties[ClaimProperties.SamlNameIdentifierNameQualifier];
                }
            }

            if (tokenDescriptor.Proof != null)
            {
                //
                // Add the key and the Holder-Of-Key confirmation method
                // for both symmetric and asymmetric key case
                //
                samlSubject.KeyIdentifier = tokenDescriptor.Proof.KeyIdentifier;
                samlSubject.ConfirmationMethods.Add(SamlConstants.HolderOfKey);
            }
            else
            {
                //
                // This is a bearer token
                //
                samlSubject.ConfirmationMethods.Add(BearerConfirmationMethod);
            }

            return samlSubject;
        }

        /// <summary>
        /// Builds an XML formated string from a collection of saml attributes that represend the Actor. 
        /// </summary>
        /// <param name="attributes">An enumeration of Saml Attributes.</param>
        /// <returns>A well formed XML string.</returns>
        /// <remarks>The string is of the form "&lt;Actor&gt;&lt;SamlAttribute name, ns&gt;&lt;SamlAttributeValue&gt;...&lt;/SamlAttributeValue&gt;, ...&lt;/SamlAttribute&gt;...&lt;/Actor&gt;"</remarks>        
        protected virtual string CreateXmlStringFromAttributes(IEnumerable<SamlAttribute> attributes)
        {
            bool actorElementWritten = false;

            using (MemoryStream ms = new MemoryStream())
            {
                using (XmlDictionaryWriter dicWriter = XmlDictionaryWriter.CreateTextWriter(ms, Encoding.UTF8, false))
                {
                    foreach (SamlAttribute samlAttribute in attributes)
                    {
                        if (samlAttribute != null)
                        {
                            if (!actorElementWritten)
                            {
                                dicWriter.WriteStartElement(Actor);
                                actorElementWritten = true;
                            }
                            WriteAttribute(dicWriter, samlAttribute);
                        }
                    }

                    if (actorElementWritten)
                    {
                        dicWriter.WriteEndElement();
                    }

                    dicWriter.Flush();
                }
                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }

        /// <summary>
        /// Generates a SamlAttribute from a claim.
        /// </summary>
        /// <param name="claim">Claim from which to generate a SamlAttribute.</param>
        /// <param name="tokenDescriptor">Contains all the information that is used in token issuance.</param>
        /// <returns>The SamlAttribute.</returns>
        /// <exception cref="ArgumentNullException">The parameter 'claim' is null.</exception>
        protected virtual SamlAttribute CreateAttribute(Claim claim, SecurityTokenDescriptor tokenDescriptor)
        {
            if (claim == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("claim");
            }

            int lastSlashIndex = claim.Type.LastIndexOf('/');
            string attributeNamespace = null;
            string attributeName = null;

            if ((lastSlashIndex == 0) || (lastSlashIndex == -1))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("claimType", SR.GetString(SR.ID4216, claim.Type));
            }
            else if (lastSlashIndex == claim.Type.Length - 1)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("claimType", SR.GetString(SR.ID4216, claim.Type));
            }
            else
            {
                attributeNamespace = claim.Type.Substring(0, lastSlashIndex);
                //
                // The WCF SamlAttribute requires that the attributeNamespace and attributeName are both non-null and non-empty. 
                // Furthermore, on deserialization / construction it considers the claimType associated with the SamlAttribute to be attributeNamespace + "/" + attributeName. 
                //
                // IDFX extends the WCF SamlAttribute and hence has to work with an attributeNamespace and attributeName that are both non-null and non-empty. 
                // On serialization, we identify the last slash in the claimtype, and treat everything before the slash as the attributeNamespace and everything after the slash as the attributeName. 
                // On deserialization, we don't always insert a "/" between the attributeNamespace and attributeName (like WCF does); we only do so if the attributeNamespace doesn't have a trailing slash.
                //
                // Send     Receive     Behavior
                // =============================
                // WCF      WCF         Works as expected
                //
                // WCF      IDFX        In the common case (http://www.claimtypes.com/foo), WCF will not send a trailing slash in the attributeNamespace. IDFX will add one upon deserialization.
                //                      In the edge case (http://www.claimtypes.com//foo), WCF will send a trailing slash in the attributeNamespace. IDFX will not add one upon deserialization.
                //
                // IDFX     WCF         In the common case (http://www.claimtypes.com/foo), IDFX will not send a trailing slash. WCF will add one upon deserialization.
                //                      In the edge case (http://www.claimtypes.com//foo), IDFX will throw (which is what the fix for FIP 6301 is about).
                //
                // IDFX     IDFX        Works as expected
                //
                if (attributeNamespace.EndsWith("/", StringComparison.Ordinal))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("claim", SR.GetString(SR.ID4213, claim.Type));
                }
                attributeName = claim.Type.Substring(lastSlashIndex + 1, claim.Type.Length - (lastSlashIndex + 1));
            }

            SamlAttribute attribute = new SamlAttribute(attributeNamespace, attributeName, new string[] { claim.Value });
            if (!StringComparer.Ordinal.Equals(ClaimsIdentity.DefaultIssuer, claim.OriginalIssuer))
            {
                attribute.OriginalIssuer = claim.OriginalIssuer;
            }
            attribute.AttributeValueXsiType = claim.ValueType;

            return attribute;
        }

        #endregion

        #region TokenValidation

        /// <summary>
        /// Returns value indicates if this handler can validate tokens of type
        /// SamlSecurityToken.
        /// </summary>
        public override bool CanValidateToken
        {
            get { return true; }
        }

        /// <summary>
        /// Gets or sets the X509CeritificateValidator that is used by the current instance.
        /// </summary>
        public X509CertificateValidator CertificateValidator
        {
            get
            {
                if (_samlSecurityTokenRequirement.CertificateValidator == null)
                {
                    if (Configuration != null)
                    {
                        return Configuration.CertificateValidator;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return _samlSecurityTokenRequirement.CertificateValidator;
                }
            }
            set
            {
                _samlSecurityTokenRequirement.CertificateValidator = value;
            }
        }

        /// <summary>
        /// Throws if a token is detected as being replayed. If the token is not found it is added to the <see cref="TokenReplayCache" />.
        /// </summary>
        /// <exception cref="ArgumentNullException">The input argument 'token' is null.</exception>
        /// <exception cref="InvalidOperationException">Configuration or Configuration.TokenReplayCache property is null.</exception>
        /// <exception cref="ArgumentException">The input argument 'token' is not a SamlSecurityToken.</exception>
        /// <exception cref="SecurityTokenValidationException">SamlSecurityToken.Assertion.Id is null or empty.</exception>
        /// <exception cref="SecurityTokenReplayDetectedException">If the token is found in the <see cref="TokenReplayCache" />.</exception>
        /// <remarks>The default behavior is to only check tokens bearer tokens (tokens that do not have keys).</remarks>
        protected override void DetectReplayedToken(SecurityToken token)
        {
            if (token == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("token");
            }

            SamlSecurityToken samlToken = token as SamlSecurityToken;
            if (null == samlToken)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("token", SR.GetString(SR.ID1067, token.GetType().ToString()));
            }

            //
            // by default we only check bearer tokens.
            //

            if (samlToken.SecurityKeys.Count != 0)
            {
                return;
            }

            if (Configuration == null)
            {
                throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID4274));
            }

            if (Configuration.Caches.TokenReplayCache == null)
            {
                throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID4278));
            }

            if (string.IsNullOrEmpty(samlToken.Assertion.AssertionId))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenValidationException(SR.GetString(SR.ID1063)));
            }

            StringBuilder stringBuilder = new StringBuilder();

            string key;

            using (HashAlgorithm hashAlgorithm = CryptoHelper.NewSha256HashAlgorithm())
            {
                if (string.IsNullOrEmpty(samlToken.Assertion.Issuer))
                {
                    stringBuilder.AppendFormat("{0}{1}", samlToken.Assertion.AssertionId, _tokenTypeIdentifiers[0]);
                }
                else
                {
                    stringBuilder.AppendFormat("{0}{1}{2}", samlToken.Assertion.AssertionId, samlToken.Assertion.Issuer, _tokenTypeIdentifiers[0]);
                }

                key = Convert.ToBase64String(hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(stringBuilder.ToString())));
            }

            if (Configuration.Caches.TokenReplayCache.Contains(key))
            {
                if (string.IsNullOrEmpty(samlToken.Assertion.Issuer))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                             new SecurityTokenReplayDetectedException(SR.GetString(SR.ID1062, typeof(SamlSecurityToken).ToString(), samlToken.Assertion.AssertionId, "")));
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                            new SecurityTokenReplayDetectedException(SR.GetString(SR.ID1062, typeof(SamlSecurityToken).ToString(), samlToken.Assertion.AssertionId, samlToken.Assertion.Issuer)));
                }
            }
            else
            {
                Configuration.Caches.TokenReplayCache.AddOrUpdate(key, token, DateTimeUtil.Add(GetTokenReplayCacheEntryExpirationTime(samlToken), Configuration.MaxClockSkew));
            }
        }

        /// <summary>
        /// Returns the time until which the token should be held in the token replay cache.
        /// </summary>
        /// <param name="token">The token to return an expiration time for.</param>
        /// <exception cref="ArgumentNullException">The input argument 'token' is null.</exception>
        /// <exception cref="SecurityTokenValidationException">The SamlSecurityToken's validity period is greater than the expiration period set to TokenReplayCache.</exception>
        /// <returns>A DateTime representing the expiration time.</returns>
        protected virtual DateTime GetTokenReplayCacheEntryExpirationTime(SamlSecurityToken token)
        {
            if (token == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("token");
            }

            //
            //  DateTimeUtil handles overflows
            //
            DateTime maximumExpirationTime = DateTimeUtil.Add(DateTime.UtcNow, Configuration.TokenReplayCacheExpirationPeriod);

            // If the token validity period is greater than the TokenReplayCacheExpirationPeriod, throw
            if (DateTime.Compare(maximumExpirationTime, token.ValidTo) < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new SecurityTokenValidationException(SR.GetString(SR.ID1069, token.ValidTo.ToString(), Configuration.TokenReplayCacheExpirationPeriod.ToString())));
            }

            return token.ValidTo;
        }

        /// <summary>
        /// Rejects tokens that are not valid. 
        /// </summary>
        /// <remarks>
        /// The token may be invalid for a number of reasons. For example, the 
        /// current time may not be within the token's validity period, the 
        /// token may contain invalid or contradictory data, or the token 
        /// may contain unsupported SAML elements.
        /// </remarks>
        /// <param name="conditions">SAML condition to be validated.</param>
        /// <param name="enforceAudienceRestriction">True to check for Audience Restriction condition.</param>
        protected virtual void ValidateConditions(SamlConditions conditions, bool enforceAudienceRestriction)
        {
            if (null != conditions)
            {
                DateTime now = DateTime.UtcNow;

                if (null != conditions.NotBefore
                    && DateTimeUtil.Add(now, Configuration.MaxClockSkew) < conditions.NotBefore)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new SecurityTokenNotYetValidException(SR.GetString(SR.ID4222, conditions.NotBefore, now)));
                }

                if (null != conditions.NotOnOrAfter
                    && DateTimeUtil.Add(now, Configuration.MaxClockSkew.Negate()) >= conditions.NotOnOrAfter)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new SecurityTokenExpiredException(SR.GetString(SR.ID4223, conditions.NotOnOrAfter, now)));
                }
            }

            //
            // Enforce the audience restriction
            //
            if (enforceAudienceRestriction)
            {
                if (this.Configuration == null || this.Configuration.AudienceRestriction.AllowedAudienceUris.Count == 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ID1032)));
                }

                //
                // Process each condition, enforcing the AudienceRestrictionConditions
                //
                bool foundAudienceRestriction = false;

                if (null != conditions && null != conditions.Conditions)
                {
                    foreach (SamlCondition condition in conditions.Conditions)
                    {
                        SamlAudienceRestrictionCondition audienceRestriction = condition as SamlAudienceRestrictionCondition;
                        if (null == audienceRestriction)
                        {
                            // Skip other conditions
                            continue;
                        }

                        _samlSecurityTokenRequirement.ValidateAudienceRestriction(this.Configuration.AudienceRestriction.AllowedAudienceUris, audienceRestriction.Audiences);
                        foundAudienceRestriction = true;
                    }
                }

                if (!foundAudienceRestriction)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new AudienceUriValidationFailedException(SR.GetString(SR.ID1035)));
                }
            }
        }

        /// <summary>
        /// Validates a <see cref="SamlSecurityToken"/>.
        /// </summary>
        /// <param name="token">The <see cref="SamlSecurityToken"/> to validate.</param>
        /// <returns>The <see cref="ReadOnlyCollection{T}"/> of <see cref="ClaimsIdentity"/> representing the identities contained in the token.</returns>
        /// <exception cref="ArgumentNullException">The parameter 'token' is null.</exception>
        /// <exception cref="ArgumentException">The token is not assignable from <see cref="SamlSecurityToken"/>.</exception>
        /// <exception cref="InvalidOperationException">Configuration <see cref="SecurityTokenHandlerConfiguration"/>is null.</exception>
        /// <exception cref="ArgumentException">SamlSecurityToken.Assertion is null.</exception>
        /// <exception cref="SecurityTokenValidationException">Thrown if SamlSecurityToken.Assertion.SigningToken is null.</exception>
        /// <exception cref="SecurityTokenValidationException">Thrown if the certificate associated with the token issuer does not pass validation.</exception>
        public override ReadOnlyCollection<ClaimsIdentity> ValidateToken(SecurityToken token)
        {
            if (token == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("token");
            }

            SamlSecurityToken samlToken = token as SamlSecurityToken;
            if (samlToken == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("token", SR.GetString(SR.ID1033, token.GetType().ToString()));
            }

            if (this.Configuration == null)
            {
                throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID4274));
            }

            try
            {
                if (samlToken.Assertion == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("token", SR.GetString(SR.ID1034));
                }

                TraceUtility.TraceEvent(TraceEventType.Verbose, TraceCode.Diagnostics, SR.GetString(SR.TraceValidateToken), new SecurityTraceRecordHelper.TokenTraceRecord(token), null, null);

                // Ensure token was signed and verified at some point
                if (samlToken.Assertion.SigningToken == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenValidationException(SR.GetString(SR.ID4220)));
                }

                this.ValidateConditions(samlToken.Assertion.Conditions, _samlSecurityTokenRequirement.ShouldEnforceAudienceRestriction(this.Configuration.AudienceRestriction.AudienceMode, samlToken));

                // We need something like AudienceUriMode and have a setting on Configuration to allow extensibility and custom settings
                // By default we only check bearer tokens
                if (this.Configuration.DetectReplayedTokens)
                {
                    this.DetectReplayedToken(samlToken);
                }

                //
                // If the backing token is x509, validate trust
                //
                X509SecurityToken x509IssuerToken = samlToken.Assertion.SigningToken as X509SecurityToken;
                if (x509IssuerToken != null)
                {
                    try
                    {
                        CertificateValidator.Validate(x509IssuerToken.Certificate);
                    }
                    catch (SecurityTokenValidationException e)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenValidationException(SR.GetString(SR.ID4257,
                                X509Util.GetCertificateId(x509IssuerToken.Certificate)), e));
                    }
                }

                //
                // Create the claims
                //
                ClaimsIdentity claimsIdentity = CreateClaims(samlToken);

                if (_samlSecurityTokenRequirement.MapToWindows)
                {
                    // TFS: 153865, [....] WindowsIdentity does not set Authtype. I don't think that authtype should be set here anyway.
                    // The authtype will be S4U (kerberos) it doesn't really matter that the upn arrived in a SAML token.
                    WindowsIdentity windowsIdentity = CreateWindowsIdentity(FindUpn(claimsIdentity));

                    // PARTIAL TRUST: will fail when adding claims, AddClaims is SecurityCritical.
                    windowsIdentity.AddClaims(claimsIdentity.Claims);
                    claimsIdentity = windowsIdentity;
                }

                if (this.Configuration.SaveBootstrapContext)
                {
                    claimsIdentity.BootstrapContext = new BootstrapContext(token, this);
                }

                this.TraceTokenValidationSuccess(token);

                List<ClaimsIdentity> identities = new List<ClaimsIdentity>(1);
                identities.Add(claimsIdentity);
                return identities.AsReadOnly();
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }

                this.TraceTokenValidationFailure(token, e.Message);
                throw e;
            }
        }

        /// <summary>
        /// Creates a <see cref="WindowsIdentity"/> object using the <paramref name="upn"/> value.
        /// </summary>
        /// <param name="upn">The upn name.</param>
        /// <returns>A <see cref="WindowsIdentity"/> object.</returns>
        /// <exception cref="ArgumentException">If <paramref name="upn"/> is null or empty.</exception>
        protected virtual WindowsIdentity CreateWindowsIdentity(string upn)
        {
            if (string.IsNullOrEmpty(upn))
            {
                throw DiagnosticUtility.ThrowHelperArgumentNullOrEmptyString("upn");
            }

            WindowsIdentity wi = new WindowsIdentity(upn);

            return new WindowsIdentity(wi.Token, AuthenticationTypes.Federation, WindowsAccountType.Normal, true);
        }
        
        /// <summary>
        /// Finds the UPN claim value in the provided <see cref="ClaimsIdentity" /> object for the purpose
        /// of mapping the identity to a <see cref="WindowsIdentity" /> object.
        /// </summary>
        /// <param name="claimsIdentity">The claims identity object containing the desired UPN claim.</param>
        /// <returns>The UPN claim value found.</returns>
        /// <exception cref="InvalidOperationException">If more than one UPN claim is contained in 
        /// <paramref name="claimsIdentity"/></exception>
        protected virtual string FindUpn(ClaimsIdentity claimsIdentity)
        {
            return ClaimsHelper.FindUpn(claimsIdentity);
        }

        /// <summary>
        /// Generates SubjectCollection that represents a SamlToken.
        /// Only SamlAttributeStatements processed.
        /// Overwrite this method to customize the creation of statements.
        /// <para>
        /// Calls:
        /// 1. ProcessAttributeStatement for SamlAttributeStatements.
        /// 2. ProcessAuthenticationStatement for SamlAuthenticationStatements.
        /// 3. ProcessAuthorizationDecisionStatement for SamlAuthorizationDecisionStatements.
        /// 4. ProcessCustomStatement for other SamlStatements.
        /// </para>
        /// </summary>
        /// <param name="samlSecurityToken">The token used to generate the SubjectCollection.</param>
        /// <returns>ClaimsIdentity representing the subject of the SamlToken.</returns>
        /// <exception cref="ArgumentNullException">Thrown if 'samlSecurityToken' is null.</exception>
        protected virtual ClaimsIdentity CreateClaims(SamlSecurityToken samlSecurityToken)
        {
            if (samlSecurityToken == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("samlSecurityToken");
            }

            if (samlSecurityToken.Assertion == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("samlSecurityToken", SR.GetString(SR.ID1034));
            }

            //
            // Construct the subject and issuer identities.
            // Use claim types specified in the security token requirements used for IPrincipal.Role and IIdentity.Name 
            //
            ClaimsIdentity subject = new ClaimsIdentity(AuthenticationTypes.Federation,
                                                         _samlSecurityTokenRequirement.NameClaimType,
                                                         _samlSecurityTokenRequirement.RoleClaimType);

            string issuer = null;

            if (this.Configuration == null)
            {
                throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID4274));
            }

            if (this.Configuration.IssuerNameRegistry == null)
            {
                throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID4277));
            }

            // SamlAssertion. The SigningToken may or may not be null.
            // The default IssuerNameRegistry will throw if null.
            // This callout is provided for extensibility scenarios with custom IssuerNameRegistry.
            issuer = this.Configuration.IssuerNameRegistry.GetIssuerName(samlSecurityToken.Assertion.SigningToken, samlSecurityToken.Assertion.Issuer);

            if (string.IsNullOrEmpty(issuer))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.ID4175)));
            }

            ProcessStatement(samlSecurityToken.Assertion.Statements, subject, issuer);
            return subject;
        }

        /// <summary>
        /// Returns the Saml11 AuthenticationMethod matching a normalized value.
        /// </summary>
        /// <param name="normalizedAuthenticationType">Normalized value.</param>
        /// <returns><see cref="SamlConstants.AuthenticationMethods"/></returns>
        protected virtual string DenormalizeAuthenticationType(string normalizedAuthenticationType)
        {
            return AuthenticationTypeMaps.Denormalize(normalizedAuthenticationType, AuthenticationTypeMaps.Saml);
        }

        /// <summary>
        /// Returns the normalized value matching a Saml11 AuthenticationMethod.
        /// </summary>
        /// <param name="saml11AuthenticationMethod"><see cref="SamlConstants.AuthenticationMethods"/></param>
        /// <returns>Normalized value.</returns>
        protected virtual string NormalizeAuthenticationType(string saml11AuthenticationMethod)
        {
            return AuthenticationTypeMaps.Normalize(saml11AuthenticationMethod, AuthenticationTypeMaps.Saml);
        }

        /// <summary>
        /// Processes all statements to generate claims.
        /// </summary>
        /// <param name="statements">A collection of Saml2Statement.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="issuer">The issuer.</param>
        protected virtual void ProcessStatement(IList<SamlStatement> statements, ClaimsIdentity subject, string issuer)
        {
            if (statements == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("statements");
            }

            Collection<SamlAuthenticationStatement> authStatementCollection = new Collection<SamlAuthenticationStatement>();

            //
            // Validate that the Saml subjects in all the statements are the same.
            //
            ValidateStatements(statements);

            foreach (SamlStatement samlStatement in statements)
            {
                SamlAttributeStatement attrStatement = samlStatement as SamlAttributeStatement;
                if (attrStatement != null)
                {
                    ProcessAttributeStatement(attrStatement, subject, issuer);
                }
                else
                {
                    SamlAuthenticationStatement authenStatement = samlStatement as SamlAuthenticationStatement;
                    if (authenStatement != null)
                    {
                        authStatementCollection.Add(authenStatement);
                    }
                    else
                    {
                        SamlAuthorizationDecisionStatement decisionStatement = samlStatement as SamlAuthorizationDecisionStatement;
                        if (decisionStatement != null)
                        {
                            ProcessAuthorizationDecisionStatement(decisionStatement, subject, issuer);
                        }
                        else
                        {
                            // We don't process custom statements. Just fall through.
                        }
                    }
                }
            }

            // Processing Authentication statement(s) should be done at the last phase to add the authentication
            // information as claims to the ClaimsIdentity
            foreach (SamlAuthenticationStatement authStatement in authStatementCollection)
            {
                if (authStatement != null)
                {
                    ProcessAuthenticationStatement(authStatement, subject, issuer);
                }
            }
        }

        /// <summary>
        /// Override this virtual to provide custom processing of SamlAttributeStatements.
        /// </summary>
        /// <param name="samlStatement">The SamlAttributeStatement to process.</param>
        /// <param name="subject">The identity that should be modified to reflect the statement.</param>
        /// <param name="issuer">The subject that identifies the issuer.</param>
        /// <exception cref="ArgumentNullException">The input parameter 'samlStatement' or 'subject' is null.</exception>
        protected virtual void ProcessAttributeStatement(SamlAttributeStatement samlStatement, ClaimsIdentity subject, string issuer)
        {
            if (samlStatement == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("samlStatement");
            }

            if (subject == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("subject");
            }

            // We will be adding the nameid claim only once for multiple attribute and/or authn statements. 
            // As of now, we put the nameId claim both inside the saml subject and the saml attribute statement as assertion. 
            // When generating claims, we will only pick up the saml subject of a saml statement, not the attribute statement value.
            ProcessSamlSubject(samlStatement.SamlSubject, subject, issuer);

            foreach (SamlAttribute attr in samlStatement.Attributes)
            {
                string claimType = null;
                if (string.IsNullOrEmpty(attr.Namespace))
                {
                    claimType = attr.Name;
                }
                else if (StringComparer.Ordinal.Equals(attr.Name, SamlConstants.ElementNames.NameIdentifier))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.ID4094)));
                }
                else
                {
                    // check if Namespace end with slash don't add it
                    // If no slash or last char is not a slash, add it.
                    int lastSlashIndex = attr.Namespace.LastIndexOf('/');
                    if ((lastSlashIndex == -1) || (!(lastSlashIndex == attr.Namespace.Length - 1)))
                    {
                        claimType = attr.Namespace + "/" + attr.Name;
                    }
                    else
                    {
                        claimType = attr.Namespace + attr.Name;
                    }

                }

                if (claimType == ClaimTypes.Actor)
                {
                    if (subject.Actor != null)
                    {
                        throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID4034));
                    }

                    SetDelegateFromAttribute(attr, subject, issuer);
                }
                else
                {
                    for (int k = 0; k < attr.AttributeValues.Count; ++k)
                    {
                        // Check if we already have a nameId claim.
                        if (StringComparer.Ordinal.Equals(ClaimTypes.NameIdentifier, claimType) && GetClaim(subject, ClaimTypes.NameIdentifier) != null)
                        {
                            continue;
                        }
                        string originalIssuer = issuer;
                        SamlAttribute SamlAttribute = attr as SamlAttribute;
                        if ((SamlAttribute != null) && (SamlAttribute.OriginalIssuer != null))
                        {
                            originalIssuer = SamlAttribute.OriginalIssuer;
                        }
                        string claimValueType = ClaimValueTypes.String;
                        if (SamlAttribute != null)
                        {
                            claimValueType = SamlAttribute.AttributeValueXsiType;
                        }
                        subject.AddClaim(new Claim(claimType, attr.AttributeValues[k], claimValueType, issuer, originalIssuer));
                    }
                }
            }

        }

        /// <summary>
        /// Gets a specific claim of type claimType from the subject's claims collection.
        /// </summary>
        /// <param name="subject">The subject.</param>
        /// <param name="claimType">The type of the claim.</param>
        /// <returns>The claim of type claimType if present, else null.</returns>
        private static Claim GetClaim(ClaimsIdentity subject, string claimType)
        {
            foreach (Claim claim in subject.Claims)
            {
                if (StringComparer.Ordinal.Equals(claimType, claim.Type))
                {
                    return claim;
                }
            }
            return null;
        }

        /// <summary>
        /// For each saml statement (attribute/authentication/authz/custom), we will check if we need to create
        /// a nameid claim or a key identifier claim out of its SamlSubject.
        /// </summary>
        /// <remarks>
        /// To make sure that the saml subject within each saml statement are the same, this method does the following comparisons.
        /// 1. All the saml subjects' contents are the same.
        /// 2. The name identifiers (if present) are the same. The name identifier comparison is done for the name identifier value,
        ///    name identifier format (if present), and name identifier qualifier (if present).
        /// 3. The key identifiers (if present) are the same.
        /// </remarks>
        /// <param name="samlSubject">The SamlSubject to extract claims from.</param>
        /// <param name="subject">The identity that should be modified to reflect the SamlSubject.</param>
        /// <param name="issuer">The Issuer claims of the SAML token.</param>
        /// <exception cref="ArgumentNullException">The parameter 'samlSubject' is null.</exception>
        protected virtual void ProcessSamlSubject(SamlSubject samlSubject, ClaimsIdentity subject, string issuer)
        {
            if (samlSubject == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("samlSubject");
            }

            Claim nameIdentifierClaim = GetClaim(subject, ClaimTypes.NameIdentifier);

            if (nameIdentifierClaim == null)
            {
                // first saml subject. so we will create claims for this subject.
                // subsequent subjects must have the same content.

                // add name identifier claim if present.
                if (!string.IsNullOrEmpty(samlSubject.Name))
                {
                    Claim claim = new Claim(ClaimTypes.NameIdentifier, samlSubject.Name, ClaimValueTypes.String, issuer);

                    if (samlSubject.NameFormat != null)
                    {
                        claim.Properties[ClaimProperties.SamlNameIdentifierFormat] = samlSubject.NameFormat;
                    }

                    if (samlSubject.NameQualifier != null)
                    {
                        claim.Properties[ClaimProperties.SamlNameIdentifierNameQualifier] = samlSubject.NameQualifier;
                    }

                    subject.AddClaim(claim);
                }
            }
        }

        /// <summary>
        /// Override this virtual to provide custom processing of the SamlAuthenticationStatement.
        /// By default it adds authentication type and instant to each claim.
        /// </summary>
        /// <param name="samlStatement">The SamlAuthenticationStatement to process</param>
        /// <param name="subject">The identity that should be modified to reflect the statement</param>
        /// <param name="issuer">issuer Identity.</param>
        /// <exception cref="ArgumentNullException">The parameter 'samlSubject' or 'subject' is null.</exception>
        protected virtual void ProcessAuthenticationStatement(SamlAuthenticationStatement samlStatement, ClaimsIdentity subject, string issuer)
        {
            if (samlStatement == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("samlStatement");
            }

            if (subject == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("subject");
            }

            // When there is only a authentication statement present inside a saml assertion, we need to generate
            // a nameId claim. See FIP 4848. We do not support any saml assertion without a attribute statement, but
            // we might receive a saml assertion with only a authentication statement.
            ProcessSamlSubject(samlStatement.SamlSubject, subject, issuer);

            subject.AddClaim(new Claim(ClaimTypes.AuthenticationMethod, NormalizeAuthenticationType(samlStatement.AuthenticationMethod), ClaimValueTypes.String, issuer));
            subject.AddClaim(new Claim(ClaimTypes.AuthenticationInstant, XmlConvert.ToString(samlStatement.AuthenticationInstant.ToUniversalTime(), DateTimeFormats.Generated), ClaimValueTypes.DateTime, issuer));
        }

        /// <summary>
        /// Override this virtual to provide custom processing of SamlAuthorizationDecisionStatement.
        /// By default no processing is performed, you will need to access the token for SamlAuthorizationDecisionStatement information.
        /// </summary>
        /// <param name="samlStatement">The SamlAuthorizationDecisionStatement to process.</param>
        /// <param name="subject">The identity that should be modified to reflect the statement.</param>
        /// <param name="issuer">The subject that identifies the issuer.</param>
        protected virtual void ProcessAuthorizationDecisionStatement(SamlAuthorizationDecisionStatement samlStatement, ClaimsIdentity subject, string issuer)
        {
        }

        /// <summary>
        /// This method gets called when a special type of SamlAttribute is detected. The SamlAttribute passed in wraps a SamlAttribute 
        /// that contains a collection of AttributeValues, each of which are mapped to a claim.  All of the claims will be returned
        /// in an ClaimsIdentity with the specified issuer.
        /// </summary>
        /// <param name="attribute">The SamlAttribute to be processed.</param>
        /// <param name="subject">The identity that should be modified to reflect the SamlAttribute.</param>
        /// <param name="issuer">Issuer Identity.</param>
        /// <exception cref="InvalidOperationException">Will be thrown if the SamlAttribute does not contain any valid SamlAttributeValues.</exception>
        protected virtual void SetDelegateFromAttribute(SamlAttribute attribute, ClaimsIdentity subject, string issuer)
        {
            // bail here nothing to add.
            if (subject == null || attribute == null || attribute.AttributeValues == null || attribute.AttributeValues.Count < 1)
            {
                return;
            }

            Collection<Claim> claims = new Collection<Claim>();
            SamlAttribute actingAsAttribute = null;

            foreach (string attributeValue in attribute.AttributeValues)
            {
                if (attributeValue != null && attributeValue.Length > 0)
                {

                    using (XmlDictionaryReader xmlReader = XmlDictionaryReader.CreateTextReader(Encoding.UTF8.GetBytes(attributeValue), XmlDictionaryReaderQuotas.Max))
                    {
                        xmlReader.MoveToContent();
                        xmlReader.ReadStartElement(Actor);

                        while (xmlReader.IsStartElement(Attribute))
                        {
                            SamlAttribute innerAttribute = ReadAttribute(xmlReader);
                            if (innerAttribute != null)
                            {
                                string claimType = string.IsNullOrEmpty(innerAttribute.Namespace) ? innerAttribute.Name : innerAttribute.Namespace + "/" + innerAttribute.Name;
                                if (claimType == ClaimTypes.Actor)
                                {
                                    // In this case we have two delegates acting as an identity, we do not allow this
                                    if (actingAsAttribute != null)
                                    {
                                        throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID4034));
                                    }

                                    actingAsAttribute = innerAttribute;
                                }
                                else
                                {
                                    string claimValueType = ClaimValueTypes.String;
                                    string originalIssuer = null;
                                    SamlAttribute SamlAttribute = innerAttribute as SamlAttribute;
                                    if (SamlAttribute != null)
                                    {
                                        claimValueType = SamlAttribute.AttributeValueXsiType;
                                        originalIssuer = SamlAttribute.OriginalIssuer;
                                    }
                                    for (int k = 0; k < innerAttribute.AttributeValues.Count; ++k)
                                    {
                                        Claim claim = null;
                                        if (string.IsNullOrEmpty(originalIssuer))
                                        {
                                            claim = new Claim(claimType, innerAttribute.AttributeValues[k], claimValueType, issuer);
                                        }
                                        else
                                        {
                                            claim = new Claim(claimType, innerAttribute.AttributeValues[k], claimValueType, issuer, originalIssuer);
                                        }
                                        claims.Add(claim);
                                    }
                                }
                            }
                        }

                        xmlReader.ReadEndElement(); // Actor
                    }
                }
            }

            subject.Actor = new ClaimsIdentity(claims, AuthenticationTypes.Federation);

            SetDelegateFromAttribute(actingAsAttribute, subject.Actor, issuer);
        }

        #endregion

        #region TokenSerialization

        /// <summary>
        /// Indicates whether the current XML element can be read as a token 
        /// of the type handled by this instance.
        /// </summary>
        /// <param name="reader">An XML reader positioned at a start 
        /// element. The reader should not be advanced.</param>
        /// <returns>'True' if the ReadToken method can the element.</returns>
        public override bool CanReadToken(XmlReader reader)
        {
            if (reader == null)
            {
                return false;
            }

            return reader.IsStartElement(SamlConstants.ElementNames.Assertion, SamlConstants.Namespace);
        }

        /// <summary>
        /// Deserializes from XML a token of the type handled by this instance.
        /// </summary>
        /// <param name="reader">An XML reader positioned at the token's start 
        /// element.</param>
        /// <returns>An instance of <see cref="SamlSecurityToken"/>.</returns>
        /// <exception cref="InvalidOperationException">Is thrown if 'Configuration' or 'Configruation.IssuerTokenResolver' is null.</exception>
        public override SecurityToken ReadToken(XmlReader reader)
        {
            if (Configuration == null)
            {
                throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID4274));
            }

            if (Configuration.IssuerTokenResolver == null)
            {
                throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID4275));
            }

            SamlAssertion assertion = ReadAssertion(reader);
            //
            // Resolve signing token if one is present. It may be deferred and signed by reference.
            //
            SecurityToken token;

            TryResolveIssuerToken(assertion, Configuration.IssuerTokenResolver, out token);

            assertion.SigningToken = token;

            return new SamlSecurityToken(assertion);
        }

        /// <summary>
        /// Read saml:Action element.
        /// </summary>
        /// <param name="reader">XmlReader positioned at saml:Action element.</param>
        /// <returns>SamlAction</returns>
        /// <exception cref="ArgumentNullException">The parameter 'reader' is null.</exception>
        /// <exception cref="XmlException">The saml:Action element contains unknown elements.</exception>
        protected virtual SamlAction ReadAction(XmlReader reader)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            if (reader.IsStartElement(SamlConstants.ElementNames.Action, SamlConstants.Namespace))
            {
                // The Namespace attribute is optional.
                string ns = reader.GetAttribute(SamlConstants.AttributeNames.Namespace, null);

                reader.MoveToContent();
                string action = reader.ReadString();
                if (string.IsNullOrEmpty(action))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.ID4073)));
                }

                reader.MoveToContent();
                reader.ReadEndElement();

                return new SamlAction(action, ns);
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.ID4065, SamlConstants.ElementNames.Action, SamlConstants.Namespace, reader.LocalName, reader.NamespaceURI)));
            }
        }

        /// <summary>
        /// Writes the given SamlAction to the XmlWriter.
        /// </summary>
        /// <param name="writer">XmlWriter to serialize the SamlAction into.</param>
        /// <param name="action">SamlAction to serialize.</param>
        /// <exception cref="ArgumentNullException">The parameter 'writer' or 'action' is null.</exception>
        protected virtual void WriteAction(XmlWriter writer, SamlAction action)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (action == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("action");
            }

            writer.WriteStartElement(SamlConstants.Prefix, SamlConstants.ElementNames.Action, SamlConstants.Namespace);
            if (!string.IsNullOrEmpty(action.Namespace))
            {
                writer.WriteAttributeString(SamlConstants.AttributeNames.Namespace, null, action.Namespace);
            }
            writer.WriteString(action.Action);
            writer.WriteEndElement();
        }

        /// <summary>
        /// Read saml:Advice element from the given XmlReader.
        /// </summary>
        /// <param name="reader">XmlReader positioned at a SAML Advice element.</param>
        /// <returns>SamlAdvice</returns>
        /// <exception cref="ArgumentNullException">Parameter 'reader' is null.</exception>
        /// <exception cref="XmlException">The reder is not positioned at a saml:Advice element.</exception>
        protected virtual SamlAdvice ReadAdvice(XmlReader reader)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            if (!reader.IsStartElement(SamlConstants.ElementNames.Advice, SamlConstants.Namespace))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.ID4065, SamlConstants.ElementNames.Advice, SamlConstants.Namespace, reader.LocalName, reader.NamespaceURI)));
            }

            // SAML Advice is an optional element and all its child elements are optional 
            // too. So we may have an empty saml:Advice element in the saml token.
            if (reader.IsEmptyElement)
            {
                // Just issue a read for the empty element.
                reader.MoveToContent();
                reader.Read();
                return new SamlAdvice();
            }

            reader.MoveToContent();
            reader.Read();
            Collection<string> assertionIdReferences = new Collection<string>();
            Collection<SamlAssertion> assertions = new Collection<SamlAssertion>();
            while (reader.IsStartElement())
            {

                if (reader.IsStartElement(SamlConstants.ElementNames.AssertionIdReference, SamlConstants.Namespace))
                {
                    assertionIdReferences.Add(reader.ReadString());
                    reader.ReadEndElement();
                }
                else if (reader.IsStartElement(SamlConstants.ElementNames.Assertion, SamlConstants.Namespace))
                {
                    SamlAssertion assertion = ReadAssertion(reader);
                    assertions.Add(assertion);
                }
                else
                {
                    TraceUtility.TraceString(TraceEventType.Warning, SR.GetString(SR.ID8005, reader.LocalName, reader.NamespaceURI));
                    reader.Skip();
                }

            }

            reader.MoveToContent();
            reader.ReadEndElement();

            return new SamlAdvice(assertionIdReferences, assertions);
        }


        /// <summary>
        /// Serialize the given SamlAdvice to the given XmlWriter.
        /// </summary>
        /// <param name="writer">XmlWriter to serialize the SamlAdvice.</param>
        /// <param name="advice">SamlAdvice to be serialized.</param>
        /// <exception cref="ArgumentNullException">The input parameter 'writer' or 'advice' is null.</exception>
        protected virtual void WriteAdvice(XmlWriter writer, SamlAdvice advice)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (advice == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("advice");
            }

            writer.WriteStartElement(SamlConstants.Prefix, SamlConstants.ElementNames.Advice, SamlConstants.Namespace);
            if (advice.AssertionIdReferences.Count > 0)
            {
                foreach (string assertionIdReference in advice.AssertionIdReferences)
                {
                    if (string.IsNullOrEmpty(assertionIdReference))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.ID4079)));
                    }
                    writer.WriteElementString(SamlConstants.Prefix, SamlConstants.ElementNames.AssertionIdReference, SamlConstants.Namespace, assertionIdReference);
                }
            }

            if (advice.Assertions.Count > 0)
            {
                foreach (SamlAssertion assertion in advice.Assertions)
                {
                    WriteAssertion(writer, assertion);
                }
            }

            writer.WriteEndElement();
        }

        /// <summary>
        /// Read saml:Assertion element from the given reader.
        /// </summary>
        /// <param name="reader">XmlReader to deserialize the Assertion from.</param>
        /// <returns>SamlAssertion</returns>
        /// <exception cref="ArgumentNullException">The parameter 'reader' is null.</exception>
        /// <exception cref="XmlException">The XmlReader is not positioned at a saml:Assertion element or the Assertion
        /// contains unknown child elements.</exception>
        protected virtual SamlAssertion ReadAssertion(XmlReader reader)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            if (this.Configuration == null)
            {
                throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID4274));
            }

            if (this.Configuration.IssuerTokenResolver == null)
            {
                throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID4275));
            }

            SamlAssertion assertion = new SamlAssertion();

            EnvelopedSignatureReader wrappedReader = new EnvelopedSignatureReader(reader, new WrappedSerializer(this, assertion), this.Configuration.IssuerTokenResolver, false, true, false);


            if (!wrappedReader.IsStartElement(SamlConstants.ElementNames.Assertion, SamlConstants.Namespace))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.ID4065, SamlConstants.ElementNames.Assertion, SamlConstants.Namespace, wrappedReader.LocalName, wrappedReader.NamespaceURI)));
            }

            string attributeValue = wrappedReader.GetAttribute(SamlConstants.AttributeNames.MajorVersion, null);
            if (string.IsNullOrEmpty(attributeValue))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.ID4075, SamlConstants.AttributeNames.MajorVersion)));
            }

            int majorVersion = XmlConvert.ToInt32(attributeValue);

            attributeValue = wrappedReader.GetAttribute(SamlConstants.AttributeNames.MinorVersion, null);
            if (string.IsNullOrEmpty(attributeValue))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.ID4075, SamlConstants.AttributeNames.MinorVersion)));
            }

            int minorVersion = XmlConvert.ToInt32(attributeValue);

            if ((majorVersion != SamlConstants.MajorVersionValue) || (minorVersion != SamlConstants.MinorVersionValue))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.ID4076, majorVersion, minorVersion, SamlConstants.MajorVersionValue, SamlConstants.MinorVersionValue)));
            }

            attributeValue = wrappedReader.GetAttribute(SamlConstants.AttributeNames.AssertionId, null);
            if (string.IsNullOrEmpty(attributeValue))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.ID4075, SamlConstants.AttributeNames.AssertionId)));
            }

            if (!XmlUtil.IsValidXmlIDValue(attributeValue))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.ID4077, attributeValue)));
            }

            assertion.AssertionId = attributeValue;

            attributeValue = wrappedReader.GetAttribute(SamlConstants.AttributeNames.Issuer, null);
            if (string.IsNullOrEmpty(attributeValue))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.ID4075, SamlConstants.AttributeNames.Issuer)));
            }

            assertion.Issuer = attributeValue;

            attributeValue = wrappedReader.GetAttribute(SamlConstants.AttributeNames.IssueInstant, null);
            if (!string.IsNullOrEmpty(attributeValue))
            {
                assertion.IssueInstant = DateTime.ParseExact(
                    attributeValue, DateTimeFormats.Accepted, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None).ToUniversalTime();
            }

            wrappedReader.MoveToContent();
            wrappedReader.Read();

            if (wrappedReader.IsStartElement(SamlConstants.ElementNames.Conditions, SamlConstants.Namespace))
            {
                assertion.Conditions = ReadConditions(wrappedReader);
            }

            if (wrappedReader.IsStartElement(SamlConstants.ElementNames.Advice, SamlConstants.Namespace))
            {
                assertion.Advice = ReadAdvice(wrappedReader);
            }

            while (wrappedReader.IsStartElement())
            {
                assertion.Statements.Add(ReadStatement(wrappedReader));
            }

            if (assertion.Statements.Count == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.ID4078)));
            }

            wrappedReader.MoveToContent();
            wrappedReader.ReadEndElement();

            // Reading the end element will complete the signature; 
            // capture the signing creds
            assertion.SigningCredentials = wrappedReader.SigningCredentials;

            // Save the captured on-the-wire data, which can then be used
            // to re-emit this assertion, preserving the same signature.
            assertion.CaptureSourceData(wrappedReader);

            return assertion;
        }

        /// <summary>
        /// Serializes a given SamlAssertion to the XmlWriter.
        /// </summary>
        /// <param name="writer">XmlWriter to use for the serialization.</param>
        /// <param name="assertion">Assertion to be serialized into the XmlWriter.</param>
        /// <exception cref="ArgumentNullException">The input parameter 'writer' or 'assertion' is null.</exception>
        protected virtual void WriteAssertion(XmlWriter writer, SamlAssertion assertion)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (assertion == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("assertion");
            }

            SamlAssertion SamlAssertion = assertion as SamlAssertion;
            if (SamlAssertion != null)
            {
                if (SamlAssertion.CanWriteSourceData)
                {
                    SamlAssertion.WriteSourceData(writer);
                    return;
                }
            }

            if (assertion.SigningCredentials != null)
            {
                writer = new EnvelopedSignatureWriter(writer, assertion.SigningCredentials, assertion.AssertionId, new WrappedSerializer(this, assertion));
            }
            writer.WriteStartElement(SamlConstants.Prefix, SamlConstants.ElementNames.Assertion, SamlConstants.Namespace);

            writer.WriteAttributeString(SamlConstants.AttributeNames.MajorVersion, null, Convert.ToString(SamlConstants.MajorVersionValue, CultureInfo.InvariantCulture));
            writer.WriteAttributeString(SamlConstants.AttributeNames.MinorVersion, null, Convert.ToString(SamlConstants.MinorVersionValue, CultureInfo.InvariantCulture));
            writer.WriteAttributeString(SamlConstants.AttributeNames.AssertionId, null, assertion.AssertionId);
            writer.WriteAttributeString(SamlConstants.AttributeNames.Issuer, null, assertion.Issuer);
            writer.WriteAttributeString(SamlConstants.AttributeNames.IssueInstant, null, assertion.IssueInstant.ToUniversalTime().ToString(DateTimeFormats.Generated, CultureInfo.InvariantCulture));

            // Write out conditions
            if (assertion.Conditions != null)
            {
                WriteConditions(writer, assertion.Conditions);
            }

            // Write out advice if there is one
            if (assertion.Advice != null)
            {
                WriteAdvice(writer, assertion.Advice);
            }

            // Write statements.
            for (int i = 0; i < assertion.Statements.Count; i++)
            {
                WriteStatement(writer, assertion.Statements[i]);
            }

            writer.WriteEndElement();
        }

        /// <summary>
        /// Read saml:Conditions from the given XmlReader.
        /// </summary>
        /// <param name="reader">XmlReader to read the SAML conditions from.</param>
        /// <returns>SamlConditions</returns>
        /// <exception cref="ArgumentNullException">The parameter 'reader' is null.</exception>
        /// <exception cref="XmlException">The reader is not positioned at saml:Conditions element or contains 
        /// elements that are not recognized.</exception>
        protected virtual SamlConditions ReadConditions(XmlReader reader)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            SamlConditions conditions = new SamlConditions();
            string time = reader.GetAttribute(SamlConstants.AttributeNames.NotBefore, null);
            if (!string.IsNullOrEmpty(time))
            {
                conditions.NotBefore = DateTime.ParseExact(
                    time, DateTimeFormats.Accepted, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None).ToUniversalTime();
            }

            time = reader.GetAttribute(SamlConstants.AttributeNames.NotOnOrAfter, null);
            if (!string.IsNullOrEmpty(time))
            {
                conditions.NotOnOrAfter = DateTime.ParseExact(
                    time, DateTimeFormats.Accepted, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None).ToUniversalTime();
            }

            // Saml Conditions element is an optional element and all its child element
            // are optional as well. So we can have a empty <saml:Conditions /> element
            // in a valid Saml token.
            if (reader.IsEmptyElement)
            {
                // Just issue a read to read the Empty element.
                reader.MoveToContent();
                reader.Read();
                return conditions;
            }

            reader.ReadStartElement();

            while (reader.IsStartElement())
            {
                conditions.Conditions.Add(ReadCondition(reader));
            }

            reader.ReadEndElement();

            return conditions;
        }

        /// <summary>
        /// Serialize SamlConditions to the given XmlWriter.
        /// </summary>
        /// <param name="writer">XmlWriter to which the SamlConditions is serialized.</param>
        /// <param name="conditions">SamlConditions to be serialized.</param>
        /// <exception cref="ArgumentNullException">The parameter 'writer' or 'conditions' is null.</exception>
        protected virtual void WriteConditions(XmlWriter writer, SamlConditions conditions)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (conditions == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("conditions");
            }

            writer.WriteStartElement(SamlConstants.Prefix, SamlConstants.ElementNames.Conditions, SamlConstants.Namespace);

            // SamlConditions when new'ed up will have the min and max values defined in WCF
            // which is different than our defaults. To maintin compatability with WCF behavior we will 
            // not write out SamlConditions NotBefore and NotOnOrAfter times which match the WCF
            // min and max default values as well.
            if (conditions.NotBefore != DateTimeUtil.GetMinValue(DateTimeKind.Utc) &&
                conditions.NotBefore != WCFMinValue)
            {
                writer.WriteAttributeString(
                    SamlConstants.AttributeNames.NotBefore,
                    null,
                    conditions.NotBefore.ToUniversalTime().ToString(DateTimeFormats.Generated, DateTimeFormatInfo.InvariantInfo));
            }

            if (conditions.NotOnOrAfter != DateTimeUtil.GetMaxValue(DateTimeKind.Utc) &&
                conditions.NotOnOrAfter != WCFMaxValue)
            {
                writer.WriteAttributeString(
                    SamlConstants.AttributeNames.NotOnOrAfter,
                    null,
                    conditions.NotOnOrAfter.ToUniversalTime().ToString(DateTimeFormats.Generated, DateTimeFormatInfo.InvariantInfo));
            }

            for (int i = 0; i < conditions.Conditions.Count; i++)
            {
                WriteCondition(writer, conditions.Conditions[i]);
            }

            writer.WriteEndElement();
        }

        /// <summary>
        /// Read saml:AudienceRestrictionCondition or saml:DoNotCacheCondition from the given reader.
        /// </summary>
        /// <param name="reader">XmlReader to read the SamlCondition from.</param>
        /// <returns>SamlCondition</returns>
        /// <exception cref="ArgumentNullException">The parameter 'reader' is null.</exception>
        /// <exception cref="XmlException">XmlReader is positioned at an unknown element.</exception>
        protected virtual SamlCondition ReadCondition(XmlReader reader)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            if (reader.IsStartElement(SamlConstants.ElementNames.AudienceRestrictionCondition, SamlConstants.Namespace))
            {
                return ReadAudienceRestrictionCondition(reader);
            }
            else if (reader.IsStartElement(SamlConstants.ElementNames.DoNotCacheCondition, SamlConstants.Namespace))
            {
                return ReadDoNotCacheCondition(reader);
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.ID4080, reader.LocalName, reader.NamespaceURI)));
            }
        }

        /// <summary>
        /// Serializes the given SamlCondition to the given XmlWriter. 
        /// </summary>
        /// <param name="writer">XmlWriter to serialize the condition.</param>
        /// <param name="condition">SamlConditon to be serialized.</param>
        /// <exception cref="ArgumentNullException">The parameter 'condition' is null.</exception>
        /// <exception cref="SecurityTokenException">The given condition is unknown. By default only SamlAudienceRestrictionCondition
        /// and SamlDoNotCacheCondition are serialized.</exception>
        protected virtual void WriteCondition(XmlWriter writer, SamlCondition condition)
        {
            if (condition == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("condition");
            }

            SamlAudienceRestrictionCondition audienceRestrictionCondition = condition as SamlAudienceRestrictionCondition;
            if (audienceRestrictionCondition != null)
            {
                WriteAudienceRestrictionCondition(writer, audienceRestrictionCondition);
                return;
            }

            SamlDoNotCacheCondition doNotCacheCondition = condition as SamlDoNotCacheCondition;
            if (doNotCacheCondition != null)
            {
                WriteDoNotCacheCondition(writer, doNotCacheCondition);
                return;
            }

            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.ID4081, condition.GetType())));
        }

        /// <summary>
        /// Read saml:AudienceRestrictionCondition from the given XmlReader.
        /// </summary>
        /// <param name="reader">XmlReader positioned at a saml:AudienceRestrictionCondition.</param>
        /// <returns>SamlAudienceRestrictionCondition</returns>
        /// <exception cref="ArgumentNullException">The inpur parameter 'reader' is null.</exception>
        /// <exception cref="XmlException">The XmlReader is not positioned at saml:AudienceRestrictionCondition.</exception>
        protected virtual SamlAudienceRestrictionCondition ReadAudienceRestrictionCondition(XmlReader reader)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            if (!reader.IsStartElement(SamlConstants.ElementNames.AudienceRestrictionCondition, SamlConstants.Namespace))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.ID4082, SamlConstants.ElementNames.AudienceRestrictionCondition, SamlConstants.Namespace, reader.LocalName, reader.NamespaceURI)));
            }

            reader.ReadStartElement();

            SamlAudienceRestrictionCondition audienceRestrictionCondition = new SamlAudienceRestrictionCondition();
            while (reader.IsStartElement())
            {
                if (reader.IsStartElement(SamlConstants.ElementNames.Audience, SamlConstants.Namespace))
                {
                    string audience = reader.ReadString();
                    if (string.IsNullOrEmpty(audience))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.ID4083)));
                    }

                    audienceRestrictionCondition.Audiences.Add(new Uri(audience, UriKind.RelativeOrAbsolute));
                    reader.MoveToContent();
                    reader.ReadEndElement();
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.ID4082, SamlConstants.ElementNames.Audience, SamlConstants.Namespace, reader.LocalName, reader.NamespaceURI)));
                }
            }

            if (audienceRestrictionCondition.Audiences.Count == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.ID4084)));
            }

            reader.MoveToContent();
            reader.ReadEndElement();

            return audienceRestrictionCondition;
        }

        /// <summary>
        /// Serialize SamlAudienceRestrictionCondition to a XmlWriter.
        /// </summary>
        /// <param name="writer">XmlWriter to serialize the SamlAudienceRestrictionCondition.</param>
        /// <param name="condition">SamlAudienceRestrictionCondition to serialize.</param>
        /// <exception cref="ArgumentNullException">The parameter 'writer' or 'condition' is null.</exception>
        protected virtual void WriteAudienceRestrictionCondition(XmlWriter writer, SamlAudienceRestrictionCondition condition)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (condition == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("condition");
            }

            // Schema requires at least one audience.
            if (condition.Audiences == null || condition.Audiences.Count == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new InvalidOperationException(SR.GetString(SR.ID4269)));
            }

            writer.WriteStartElement(SamlConstants.Prefix, SamlConstants.ElementNames.AudienceRestrictionCondition, SamlConstants.Namespace);

            for (int i = 0; i < condition.Audiences.Count; i++)
            {
                // When writing out the audience uri we use the OriginalString property to preserve the value that was initially passed down during token creation as-is. 
                writer.WriteElementString(SamlConstants.ElementNames.Audience, SamlConstants.Namespace, condition.Audiences[i].OriginalString);
            }

            writer.WriteEndElement();
        }

        /// <summary>
        /// Read saml:DoNotCacheCondition from the given XmlReader.
        /// </summary>
        /// <param name="reader">XmlReader positioned at a saml:DoNotCacheCondition element.</param>
        /// <returns>SamlDoNotCacheCondition</returns>
        /// <exception cref="ArgumentNullException">The inpur parameter 'reader' is null.</exception>
        /// <exception cref="XmlException">The XmlReader is not positioned at saml:DoNotCacheCondition.</exception>
        protected virtual SamlDoNotCacheCondition ReadDoNotCacheCondition(XmlReader reader)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            if (!reader.IsStartElement(SamlConstants.ElementNames.DoNotCacheCondition, SamlConstants.Namespace))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.ID4082, SamlConstants.ElementNames.DoNotCacheCondition, SamlConstants.Namespace, reader.LocalName, reader.NamespaceURI)));
            }

            SamlDoNotCacheCondition doNotCacheCondition = new SamlDoNotCacheCondition();
            // saml:DoNotCacheCondition is a empty element. So just issue a read for
            // the empty element.
            if (reader.IsEmptyElement)
            {
                reader.MoveToContent();
                reader.Read();
                return doNotCacheCondition;
            }

            reader.MoveToContent();
            reader.ReadStartElement();
            reader.ReadEndElement();

            return doNotCacheCondition;
        }

        /// <summary>
        /// Serialize SamlDoNotCacheCondition to a XmlWriter.
        /// </summary>
        /// <param name="writer">XmlWriter to serialize the SamlDoNotCacheCondition.</param>
        /// <param name="condition">SamlDoNotCacheCondition to serialize.</param>
        /// <exception cref="ArgumentNullException">The parameter 'writer' or 'condition' is null.</exception>
        protected virtual void WriteDoNotCacheCondition(XmlWriter writer, SamlDoNotCacheCondition condition)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (condition == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("condition");
            }

            writer.WriteStartElement(SamlConstants.Prefix, SamlConstants.ElementNames.DoNotCacheCondition, SamlConstants.Namespace);
            writer.WriteEndElement();
        }

        /// <summary>
        /// Read a SamlStatement from the given XmlReader.
        /// </summary>
        /// <param name="reader">XmlReader positioned at a SamlStatement.</param>
        /// <returns>SamlStatement</returns>
        /// <exception cref="ArgumentNullException">The inpur parameter 'reader' is null.</exception>
        /// <exception cref="XmlException">The XmlReader is not positioned at recognized SamlStatement. By default,
        /// only saml:AuthenticationStatement, saml:AttributeStatement and saml:AuthorizationDecisionStatement.</exception>
        protected virtual SamlStatement ReadStatement(XmlReader reader)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            if (reader.IsStartElement(SamlConstants.ElementNames.AuthenticationStatement, SamlConstants.Namespace))
            {
                return ReadAuthenticationStatement(reader);
            }
            else if (reader.IsStartElement(SamlConstants.ElementNames.AttributeStatement, SamlConstants.Namespace))
            {
                return ReadAttributeStatement(reader);
            }
            else if (reader.IsStartElement(SamlConstants.ElementNames.AuthorizationDecisionStatement, SamlConstants.Namespace))
            {
                return ReadAuthorizationDecisionStatement(reader);
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.ID4085, reader.LocalName, reader.NamespaceURI)));
            }

        }

        /// <summary>
        /// Serialize the SamlStatement to the XmlWriter.
        /// </summary>
        /// <param name="writer">XmlWriter to serialize the SamlStatement.</param>
        /// <param name="statement">The SamlStatement to serialize.</param>
        /// <exception cref="ArgumentNullException">The parameter 'writer' or 'statement' is null.</exception>
        /// <exception cref="SecurityTokenException">The SamlStatement is not recognized. Only SamlAuthenticationStatement,
        /// SamlAuthorizationStatement and SamlAttributeStatement are recognized.</exception>
        protected virtual void WriteStatement(XmlWriter writer, SamlStatement statement)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (statement == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("statement");
            }

            SamlAuthenticationStatement authnStatement = statement as SamlAuthenticationStatement;
            if (authnStatement != null)
            {
                WriteAuthenticationStatement(writer, authnStatement);
                return;
            }

            SamlAuthorizationDecisionStatement authzStatement = statement as SamlAuthorizationDecisionStatement;
            if (authzStatement != null)
            {
                WriteAuthorizationDecisionStatement(writer, authzStatement);
                return;
            }

            SamlAttributeStatement attributeStatement = statement as SamlAttributeStatement;
            if (attributeStatement != null)
            {
                WriteAttributeStatement(writer, attributeStatement);
                return;
            }

            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.ID4086, statement.GetType())));
        }

        /// <summary>
        /// Read the SamlSubject from the XmlReader.
        /// </summary>
        /// <param name="reader">XmlReader to read the SamlSubject from.</param>
        /// <returns>SamlSubject</returns>
        /// <exception cref="ArgumentNullException">The input argument 'reader' is null.</exception>
        /// <exception cref="XmlException">The reader is not positioned at a SamlSubject.</exception>
        protected virtual SamlSubject ReadSubject(XmlReader reader)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            if (!reader.IsStartElement(SamlConstants.ElementNames.Subject, SamlConstants.Namespace))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.ID4082, SamlConstants.ElementNames.Subject, SamlConstants.Namespace, reader.LocalName, reader.NamespaceURI)));
            }

            SamlSubject subject = new SamlSubject();

            reader.ReadStartElement(SamlConstants.ElementNames.Subject, SamlConstants.Namespace);
            if (reader.IsStartElement(SamlConstants.ElementNames.NameIdentifier, SamlConstants.Namespace))
            {
                subject.NameFormat = reader.GetAttribute(SamlConstants.AttributeNames.NameIdentifierFormat, null);
                subject.NameQualifier = reader.GetAttribute(SamlConstants.AttributeNames.NameIdentifierNameQualifier, null);

                reader.MoveToContent();
                subject.Name = reader.ReadElementString();

                if (string.IsNullOrEmpty(subject.Name))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.ID4087)));
                }
            }

            if (reader.IsStartElement(SamlConstants.ElementNames.SubjectConfirmation, SamlConstants.Namespace))
            {
                reader.ReadStartElement();

                while (reader.IsStartElement(SamlConstants.ElementNames.SubjectConfirmationMethod, SamlConstants.Namespace))
                {
                    string method = reader.ReadElementString();
                    if (string.IsNullOrEmpty(method))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.ID4088)));
                    }

                    subject.ConfirmationMethods.Add(method);
                }

                if (subject.ConfirmationMethods.Count == 0)
                {
                    // A SubjectConfirmaton clause should specify at least one 
                    // ConfirmationMethod.
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.ID4088)));
                }

                if (reader.IsStartElement(SamlConstants.ElementNames.SubjectConfirmationData, SamlConstants.Namespace))
                {
                    // An Authentication protocol specified in the confirmation method might need this
                    // data. Just store this content value as string.
                    subject.SubjectConfirmationData = reader.ReadElementString();
                }

                if (reader.IsStartElement(XmlSignatureConstants.Elements.KeyInfo, XmlSignatureConstants.Namespace))
                {
                    subject.KeyIdentifier = ReadSubjectKeyInfo(reader);
                    SecurityKey key = ResolveSubjectKeyIdentifier(subject.KeyIdentifier);
                    if (key != null)
                    {
                        subject.Crypto = key;
                    }
                    else
                    {
                        subject.Crypto = new SecurityKeyElement(subject.KeyIdentifier, this.Configuration.ServiceTokenResolver);
                    }
                }


                if ((subject.ConfirmationMethods.Count == 0) && (string.IsNullOrEmpty(subject.Name)))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.ID4089)));
                }

                reader.MoveToContent();
                reader.ReadEndElement();
            }

            reader.MoveToContent();
            reader.ReadEndElement();

            return subject;
        }

        /// <summary>
        /// Serialize the given SamlSubject into an XmlWriter.
        /// </summary>
        /// <param name="writer">XmlWriter into which the SamlSubject is serialized.</param>
        /// <param name="subject">SamlSubject to be serialized.</param>
        /// <exception cref="ArgumentNullException">The input parameter 'subject' or 'writer' is null.</exception>
        protected virtual void WriteSubject(XmlWriter writer, SamlSubject subject)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (subject == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("subject");
            }

            writer.WriteStartElement(SamlConstants.Prefix, SamlConstants.ElementNames.Subject, SamlConstants.Namespace);
            if (!string.IsNullOrEmpty(subject.Name))
            {
                writer.WriteStartElement(SamlConstants.Prefix, SamlConstants.ElementNames.NameIdentifier, SamlConstants.Namespace);
                if (!string.IsNullOrEmpty(subject.NameFormat))
                {
                    writer.WriteAttributeString(SamlConstants.AttributeNames.NameIdentifierFormat, null, subject.NameFormat);
                }
                if (subject.NameQualifier != null)
                {
                    writer.WriteAttributeString(SamlConstants.AttributeNames.NameIdentifierNameQualifier, null, subject.NameQualifier);
                }
                writer.WriteString(subject.Name);
                writer.WriteEndElement();
            }

            if (subject.ConfirmationMethods.Count > 0)
            {
                writer.WriteStartElement(SamlConstants.Prefix, SamlConstants.ElementNames.SubjectConfirmation, SamlConstants.Namespace);

                foreach (string method in subject.ConfirmationMethods)
                {
                    writer.WriteElementString(SamlConstants.ElementNames.SubjectConfirmationMethod, SamlConstants.Namespace, method);
                }

                if (!string.IsNullOrEmpty(subject.SubjectConfirmationData))
                {
                    writer.WriteElementString(SamlConstants.ElementNames.SubjectConfirmationData, SamlConstants.Namespace, subject.SubjectConfirmationData);
                }

                if (subject.KeyIdentifier != null)
                {
                    WriteSubjectKeyInfo(writer, subject.KeyIdentifier);
                }

                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        /// <summary>
        /// Read the SamlSubject KeyIdentifier from a XmlReader.
        /// </summary>
        /// <param name="reader">XmlReader positioned at the SamlSubject KeyIdentifier.</param>
        /// <returns>SamlSubject Key as a SecurityKeyIdentifier.</returns>
        /// <exception cref="ArgumentNullException">Input parameter 'reader' is null.</exception>
        /// <exception cref="XmlException">XmlReader is not positioned at a valid SecurityKeyIdentifier.</exception>
        protected virtual SecurityKeyIdentifier ReadSubjectKeyInfo(XmlReader reader)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            if (KeyInfoSerializer.CanReadKeyIdentifier(reader))
            {
                return KeyInfoSerializer.ReadKeyIdentifier(reader);
            }

            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.ID4090)));
        }

        /// <summary>
        /// Write the SamlSubject SecurityKeyIdentifier to the XmlWriter.
        /// </summary>
        /// <param name="writer">XmlWriter to write the SecurityKeyIdentifier.</param>
        /// <param name="subjectSki">SecurityKeyIdentifier to serialize.</param>
        /// <exception cref="ArgumentNullException">The inpur parameter 'writer' or 'subjectSki' is null.</exception>
        protected virtual void WriteSubjectKeyInfo(XmlWriter writer, SecurityKeyIdentifier subjectSki)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (subjectSki == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("subjectSki");
            }

            if (KeyInfoSerializer.CanWriteKeyIdentifier(subjectSki))
            {
                KeyInfoSerializer.WriteKeyIdentifier(writer, subjectSki);
                return;
            }

            throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("subjectSki", SR.GetString(SR.ID4091, subjectSki.GetType()));
        }

        /// <summary>
        /// Read saml:AttributeStatement from the given XmlReader.
        /// </summary>
        /// <param name="reader">XmlReader positioned at a saml:AttributeStatement element.</param>
        /// <returns>SamlAttributeStatement</returns>
        /// <exception cref="ArgumentNullException">Input parameter 'reader' is null.</exception>
        /// <exception cref="XmlException">XmlReader is not positioned at a saml:AttributeStatement element or
        /// the AttributeStatement contains unrecognized elements.</exception>
        protected virtual SamlAttributeStatement ReadAttributeStatement(XmlReader reader)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            if (!reader.IsStartElement(SamlConstants.ElementNames.AttributeStatement, SamlConstants.Namespace))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.ID4082, SamlConstants.ElementNames.AttributeStatement, SamlConstants.Namespace, reader.LocalName, reader.NamespaceURI)));
            }

            reader.ReadStartElement();

            SamlAttributeStatement attributeStatement = new SamlAttributeStatement();
            if (reader.IsStartElement(SamlConstants.ElementNames.Subject, SamlConstants.Namespace))
            {
                attributeStatement.SamlSubject = ReadSubject(reader);
            }
            else
            {
                // SAML Subject is a required Attribute Statement clause.
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.ID4092)));
            }

            while (reader.IsStartElement())
            {
                if (reader.IsStartElement(SamlConstants.ElementNames.Attribute, SamlConstants.Namespace))
                {
                    // SAML Attribute is a extensibility point. So ask the SAML serializer 
                    // to load this part.
                    attributeStatement.Attributes.Add(ReadAttribute(reader));
                }
                else
                {
                    break;
                }
            }

            if (attributeStatement.Attributes.Count == 0)
            {
                // Each Attribute statement should have at least one attribute.
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.ID4093)));
            }

            reader.MoveToContent();
            reader.ReadEndElement();

            return attributeStatement;
        }

        /// <summary>
        /// Serialize a SamlAttributeStatement.
        /// </summary>
        /// <param name="writer">XmlWriter to serialize the given statement.</param>
        /// <param name="statement">SamlAttributeStatement to write to the XmlWriter.</param>
        /// <exception cref="ArgumentNullException">The input parameter 'writer' or 'statement' is null.</exception>
        protected virtual void WriteAttributeStatement(XmlWriter writer, SamlAttributeStatement statement)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (statement == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("statement");
            }

            writer.WriteStartElement(SamlConstants.Prefix, SamlConstants.ElementNames.AttributeStatement, SamlConstants.Namespace);

            WriteSubject(writer, statement.SamlSubject);

            for (int i = 0; i < statement.Attributes.Count; i++)
            {
                WriteAttribute(writer, statement.Attributes[i]);
            }

            writer.WriteEndElement();
        }

        /// <summary>
        /// Read an saml:Attribute element.
        /// </summary>
        /// <param name="reader">XmlReader positioned at a saml:Attribute element.</param>
        /// <returns>SamlAttribute</returns>
        /// <exception cref="ArgumentNullException">The input parameter 'reader' is null.</exception>
        /// <exception cref="XmlException">The XmlReader is not positioned on a valid saml:Attribute element.</exception>
        protected virtual SamlAttribute ReadAttribute(XmlReader reader)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            SamlAttribute attribute = new SamlAttribute();

            attribute.Name = reader.GetAttribute(SamlConstants.AttributeNames.AttributeName, null);
            if (string.IsNullOrEmpty(attribute.Name))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.ID4094)));
            }

            attribute.Namespace = reader.GetAttribute(SamlConstants.AttributeNames.AttributeNamespace, null);
            if (string.IsNullOrEmpty(attribute.Namespace))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.ID4095)));
            }

            //
            // OriginalIssuer is an optional attribute.
            // We are lax on read here, and will accept the following namespaces for original issuer, in order:
            // http://schemas.xmlsoap.org/ws/2009/09/identity/claims
            // http://schemas.microsoft.com/ws/2008/06/identity
            //
            string originalIssuer = reader.GetAttribute(SamlConstants.AttributeNames.OriginalIssuer, ClaimType2009Namespace);

            if (originalIssuer == null)
            {
                originalIssuer = reader.GetAttribute(SamlConstants.AttributeNames.OriginalIssuer, ProductConstants.NamespaceUri);
            }

            if (originalIssuer == String.Empty)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.ID4252)));
            }
            attribute.OriginalIssuer = originalIssuer;

            reader.MoveToContent();
            reader.Read();
            while (reader.IsStartElement(SamlConstants.ElementNames.AttributeValue, SamlConstants.Namespace))
            {
                // FIP 9570 - ENTERPRISE SCENARIO: Saml11SecurityTokenHandler.ReadAttribute is not checking the AttributeValue XSI type correctly.
                // Lax on receive. If we dont find the AttributeValueXsiType in the format we are looking for in the xml, we default to string.
                // Read the xsi:type. We are expecting a value of the form "some-non-empty-string" or "some-non-empty-local-prefix:some-non-empty-string".
                // ":some-non-empty-string" and "some-non-empty-string:" are edge-cases where defaulting to string is reasonable.
                string attributeValueXsiTypePrefix = null;
                string attributeValueXsiTypeSuffix = null;
                string attributeValueXsiTypeSuffixWithLocalPrefix = reader.GetAttribute("type", XmlSchema.InstanceNamespace);
                if (!string.IsNullOrEmpty(attributeValueXsiTypeSuffixWithLocalPrefix))
                {
                    if (attributeValueXsiTypeSuffixWithLocalPrefix.IndexOf(":", StringComparison.Ordinal) == -1) // "some-non-empty-string" case
                    {
                        attributeValueXsiTypePrefix = reader.LookupNamespace(String.Empty);
                        attributeValueXsiTypeSuffix = attributeValueXsiTypeSuffixWithLocalPrefix;
                    }
                    else if (attributeValueXsiTypeSuffixWithLocalPrefix.IndexOf(":", StringComparison.Ordinal) > 0 &&
                              attributeValueXsiTypeSuffixWithLocalPrefix.IndexOf(":", StringComparison.Ordinal) < attributeValueXsiTypeSuffixWithLocalPrefix.Length - 1) // "some-non-empty-local-prefix:some-non-empty-string" case
                    {
                        string localPrefix = attributeValueXsiTypeSuffixWithLocalPrefix.Substring(0, attributeValueXsiTypeSuffixWithLocalPrefix.IndexOf(":", StringComparison.Ordinal));
                        attributeValueXsiTypePrefix = reader.LookupNamespace(localPrefix);
                        // For attributeValueXsiTypeSuffix, we want the portion after the local prefix in "some-non-empty-local-prefix:some-non-empty-string"
                        attributeValueXsiTypeSuffix = attributeValueXsiTypeSuffixWithLocalPrefix.Substring(attributeValueXsiTypeSuffixWithLocalPrefix.IndexOf(":", StringComparison.Ordinal) + 1);
                    }
                }
                if (attributeValueXsiTypePrefix != null && attributeValueXsiTypeSuffix != null)
                {
                    attribute.AttributeValueXsiType = String.Concat(attributeValueXsiTypePrefix, "#", attributeValueXsiTypeSuffix);
                }

                if (reader.IsEmptyElement)
                {
                    reader.Read();
                    attribute.AttributeValues.Add(string.Empty);
                }
                else
                {
                    attribute.AttributeValues.Add(ReadAttributeValue(reader, attribute));
                }
            }

            if (attribute.AttributeValues.Count == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.ID4212)));
            }

            reader.MoveToContent();
            reader.ReadEndElement();

            return attribute;
        }


        /// <summary>
        /// Reads an attribute value.
        /// </summary>
        /// <param name="reader">XmlReader to read from.</param>
        /// <param name="attribute">The current attribute that is being read.</param>
        /// <returns>The attribute value as a string.</returns>
        /// <exception cref="ArgumentNullException">The input parameter 'reader' is null.</exception>
        protected virtual string ReadAttributeValue(XmlReader reader, SamlAttribute attribute)
        {
            // This code was designed realizing that the writter of the xml controls how our
            // reader will report the NodeType. A completely differnet system (IBM, etc) could write the values. 
            // Considering NodeType is important, because we need to read the entire value, end element and not loose anything significant.
            // 
            // Couple of cases to help understand the design choices.
            //
            // 1. 
            // "<MyElement xmlns=""urn:mynamespace""><another>complex</another></MyElement><sibling>value</sibling>"
            // Could result in the our reader reporting the NodeType as Text OR Element, depending if '<' was entitized to '&lt;'
            //
            // 2. 
            // " <MyElement xmlns=""urn:mynamespace""><another>complex</another></MyElement><sibling>value</sibling>"
            // Could result in the our reader reporting the NodeType as Text OR Whitespace.  Post Whitespace processing, the NodeType could be 
            // reported as Text or Element, depending if '<' was entitized to '&lt;'
            //
            // 3. 
            // "/r/n/t   "
            // Could result in the our reader reporting the NodeType as whitespace.
            //
            // Since an AttributeValue with ONLY Whitespace and a complex Element proceeded by whitespace are reported as the same NodeType (2. and 3.)
            // the whitespace is remembered and discarded if an found is found, otherwise it becomes the value. This is to help users who accidently put a space when adding claims in ADFS
            // If we just skipped the Whitespace, then an AttributeValue that started with Whitespace would loose that part and claims generated from the AttributeValue
            // would be missing that part.
            // 

            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            string result = String.Empty;
            string whiteSpace = String.Empty;

            reader.ReadStartElement(Saml2Constants.Elements.AttributeValue, SamlConstants.Namespace);

            while (reader.NodeType == XmlNodeType.Whitespace)
            {
                whiteSpace += reader.Value;
                reader.Read();
            }

            reader.MoveToContent();
            if (reader.NodeType == XmlNodeType.Element)
            {
                while (reader.NodeType == XmlNodeType.Element)
                {
                    result += reader.ReadOuterXml();
                    reader.MoveToContent();
                }
            }
            else
            {
                result = whiteSpace;
                result += reader.ReadContentAsString();
            }

            reader.ReadEndElement();
            return result;
        }

        /// <summary>
        /// Serializes a given SamlAttribute.
        /// </summary>
        /// <param name="writer">XmlWriter to serialize the SamlAttribute.</param>
        /// <param name="attribute">SamlAttribute to be serialized.</param>
        /// <exception cref="ArgumentNullException">The input parameter 'writer' or 'attribute' is null.</exception>
        protected virtual void WriteAttribute(XmlWriter writer, SamlAttribute attribute)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (attribute == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("attribute");
            }

            writer.WriteStartElement(SamlConstants.Prefix, SamlConstants.ElementNames.Attribute, SamlConstants.Namespace);

            writer.WriteAttributeString(SamlConstants.AttributeNames.AttributeName, null, attribute.Name);
            writer.WriteAttributeString(SamlConstants.AttributeNames.AttributeNamespace, null, attribute.Namespace);

            SamlAttribute SamlAttribute = attribute as SamlAttribute;
            if ((SamlAttribute != null) && (SamlAttribute.OriginalIssuer != null))
            {
                writer.WriteAttributeString(SamlConstants.AttributeNames.OriginalIssuer, ClaimType2009Namespace, SamlAttribute.OriginalIssuer);
            }

            string xsiTypePrefix = null;
            string xsiTypeSuffix = null;
            if (SamlAttribute != null && !StringComparer.Ordinal.Equals(SamlAttribute.AttributeValueXsiType, ClaimValueTypes.String))
            {
                // ClaimValueTypes are URIs of the form prefix#suffix, while xsi:type should be a QName.
                // Hence, the tokens-to-claims spec requires that ClaimValueTypes be serialized as xmlns:tn="prefix" xsi:type="tn:suffix"
                int indexOfHash = SamlAttribute.AttributeValueXsiType.IndexOf('#');
                xsiTypePrefix = SamlAttribute.AttributeValueXsiType.Substring(0, indexOfHash);
                xsiTypeSuffix = SamlAttribute.AttributeValueXsiType.Substring(indexOfHash + 1);
            }

            for (int i = 0; i < attribute.AttributeValues.Count; i++)
            {
                if (attribute.AttributeValues[i] == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.ID4096)));
                }

                writer.WriteStartElement(SamlConstants.Prefix, SamlConstants.ElementNames.AttributeValue, SamlConstants.Namespace);
                if ((xsiTypePrefix != null) && (xsiTypeSuffix != null))
                {
                    writer.WriteAttributeString("xmlns", ProductConstants.ClaimValueTypeSerializationPrefix, null, xsiTypePrefix);
                    writer.WriteAttributeString("type", XmlSchema.InstanceNamespace, String.Concat(ProductConstants.ClaimValueTypeSerializationPrefixWithColon, xsiTypeSuffix));
                }
                WriteAttributeValue(writer, attribute.AttributeValues[i], attribute);
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        /// <summary>
        /// Writes the saml:Attribute value.
        /// </summary>
        /// <param name="writer">XmlWriter to which to write.</param>
        /// <param name="value">Attribute value to be written.</param>
        /// <param name="attribute">The SAML attribute whose value is being written.</param>
        /// <remarks>By default the method writes the value as a string.</remarks>
        /// <exception cref="ArgumentNullException">The input parameter 'writer' is null.</exception>
        protected virtual void WriteAttributeValue(XmlWriter writer, string value, SamlAttribute attribute)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            writer.WriteString(value);
        }

        /// <summary>
        /// Read the saml:AuthenticationStatement.
        /// </summary>
        /// <param name="reader">XmlReader positioned at a saml:AuthenticationStatement.</param>
        /// <returns>SamlAuthenticationStatement</returns>
        /// <exception cref="ArgumentNullException">The input parameter 'reader' is null.</exception>
        /// <exception cref="XmlException">The XmlReader is not positioned on a saml:AuthenticationStatement
        /// or the statement contains a unknown child element.</exception>
        protected virtual SamlAuthenticationStatement ReadAuthenticationStatement(XmlReader reader)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            if (!reader.IsStartElement(SamlConstants.ElementNames.AuthenticationStatement, SamlConstants.Namespace))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.ID4082, SamlConstants.ElementNames.AuthenticationStatement, SamlConstants.Namespace, reader.LocalName, reader.NamespaceURI)));
            }

            SamlAuthenticationStatement authnStatement = new SamlAuthenticationStatement();
            string authInstance = reader.GetAttribute(SamlConstants.AttributeNames.AuthenticationInstant, null);
            if (string.IsNullOrEmpty(authInstance))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.ID4097)));
            }
            authnStatement.AuthenticationInstant = DateTime.ParseExact(
                authInstance, DateTimeFormats.Accepted, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None).ToUniversalTime();

            authnStatement.AuthenticationMethod = reader.GetAttribute(SamlConstants.AttributeNames.AuthenticationMethod, null);
            if (string.IsNullOrEmpty(authnStatement.AuthenticationMethod))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.ID4098)));
            }

            reader.MoveToContent();
            reader.Read();

            if (reader.IsStartElement(SamlConstants.ElementNames.Subject, SamlConstants.Namespace))
            {
                authnStatement.SamlSubject = ReadSubject(reader);
            }
            else
            {
                // Subject is a required element for a Authentication Statement clause.
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.ID4099)));
            }

            if (reader.IsStartElement(SamlConstants.ElementNames.SubjectLocality, SamlConstants.Namespace))
            {
                authnStatement.DnsAddress = reader.GetAttribute(SamlConstants.AttributeNames.SubjectLocalityDNSAddress, null);
                authnStatement.IPAddress = reader.GetAttribute(SamlConstants.AttributeNames.SubjectLocalityIPAddress, null);

                if (reader.IsEmptyElement)
                {
                    reader.MoveToContent();
                    reader.Read();
                }
                else
                {
                    reader.MoveToContent();
                    reader.Read();
                    reader.ReadEndElement();
                }
            }

            while (reader.IsStartElement())
            {
                if (reader.IsStartElement(SamlConstants.ElementNames.AuthorityBinding, SamlConstants.Namespace))
                {
                    authnStatement.AuthorityBindings.Add(ReadAuthorityBinding(reader));
                }
                else
                {
                    // We do not understand this element.
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.ID4082, SamlConstants.ElementNames.AuthorityBinding, SamlConstants.Namespace, reader.LocalName, reader.NamespaceURI)));
                }
            }

            reader.MoveToContent();
            reader.ReadEndElement();

            return authnStatement;
        }

        /// <summary>
        /// Serializes a given SamlAuthenticationStatement.
        /// </summary>
        /// <param name="writer">XmlWriter to which SamlAuthenticationStatement is serialized.</param>
        /// <param name="statement">SamlAuthenticationStatement to be serialized.</param>
        /// <exception cref="ArgumentNullException">The input parameter 'writer' or 'statement' is null.</exception>
        protected virtual void WriteAuthenticationStatement(XmlWriter writer, SamlAuthenticationStatement statement)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (statement == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("statement");
            }

            writer.WriteStartElement(SamlConstants.Prefix, SamlConstants.ElementNames.AuthenticationStatement, SamlConstants.Namespace);

            writer.WriteAttributeString(SamlConstants.AttributeNames.AuthenticationMethod, null, statement.AuthenticationMethod);

            writer.WriteAttributeString(SamlConstants.AttributeNames.AuthenticationInstant, null,
                             XmlConvert.ToString(statement.AuthenticationInstant.ToUniversalTime(), DateTimeFormats.Generated));


            WriteSubject(writer, statement.SamlSubject);

            if ((statement.IPAddress != null) || (statement.DnsAddress != null))
            {
                writer.WriteStartElement(SamlConstants.Prefix, SamlConstants.ElementNames.SubjectLocality, SamlConstants.Namespace);

                if (statement.IPAddress != null)
                {
                    writer.WriteAttributeString(SamlConstants.AttributeNames.SubjectLocalityIPAddress, null, statement.IPAddress);
                }

                if (statement.DnsAddress != null)
                {
                    writer.WriteAttributeString(SamlConstants.AttributeNames.SubjectLocalityDNSAddress, null, statement.DnsAddress);
                }

                writer.WriteEndElement();
            }

            for (int i = 0; i < statement.AuthorityBindings.Count; i++)
            {
                WriteAuthorityBinding(writer, statement.AuthorityBindings[i]);
            }

            writer.WriteEndElement();
        }

        /// <summary>
        /// Read the saml:AuthorityBinding element.
        /// </summary>
        /// <param name="reader">XmlReader positioned at the saml:AuthorityBinding element.</param>
        /// <returns>SamlAuthorityBinding</returns>
        /// <exception cref="ArgumentNullException">The inpur parameter 'reader' is null.</exception>
        /// <exception cref="XmlException">XmlReader is not positioned at a saml:AuthorityBinding element or
        /// contains a unrecognized or invalid child element.</exception>
        protected virtual SamlAuthorityBinding ReadAuthorityBinding(XmlReader reader)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            SamlAuthorityBinding authorityBinding = new SamlAuthorityBinding();
            string authKind = reader.GetAttribute(SamlConstants.AttributeNames.AuthorityKind, null);
            if (string.IsNullOrEmpty(authKind))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.ID4200)));
            }

            string[] authKindParts = authKind.Split(':');
            if (authKindParts.Length > 2)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.ID4201, authKind)));
            }

            string localName;
            string prefix;
            string nameSpace;
            if (authKindParts.Length == 2)
            {
                prefix = authKindParts[0];
                localName = authKindParts[1];
            }
            else
            {
                prefix = String.Empty;
                localName = authKindParts[0];
            }

            nameSpace = reader.LookupNamespace(prefix);

            authorityBinding.AuthorityKind = new XmlQualifiedName(localName, nameSpace);

            authorityBinding.Binding = reader.GetAttribute(SamlConstants.AttributeNames.Binding, null);
            if (string.IsNullOrEmpty(authorityBinding.Binding))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.ID4202)));
            }

            authorityBinding.Location = reader.GetAttribute(SamlConstants.AttributeNames.Location, null);
            if (string.IsNullOrEmpty(authorityBinding.Location))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.ID4203)));
            }

            if (reader.IsEmptyElement)
            {
                reader.MoveToContent();
                reader.Read();
            }
            else
            {
                reader.MoveToContent();
                reader.Read();
                reader.ReadEndElement();
            }

            return authorityBinding;
        }

        /// <summary>
        /// Serialize a SamlAuthorityBinding.
        /// </summary>
        /// <param name="writer">XmlWriter to serialize the SamlAuthorityBinding</param>
        /// <param name="authorityBinding">SamlAuthoriyBinding to be serialized.</param>
        protected virtual void WriteAuthorityBinding(XmlWriter writer, SamlAuthorityBinding authorityBinding)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (authorityBinding == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("statement");
            }

            writer.WriteStartElement(SamlConstants.Prefix, SamlConstants.ElementNames.AuthorityBinding, SamlConstants.Namespace);

            string prefix = null;
            if (!string.IsNullOrEmpty(authorityBinding.AuthorityKind.Namespace))
            {
                writer.WriteAttributeString(String.Empty, SamlConstants.AttributeNames.NamespaceAttributePrefix, null, authorityBinding.AuthorityKind.Namespace);
                prefix = writer.LookupPrefix(authorityBinding.AuthorityKind.Namespace);
            }

            writer.WriteStartAttribute(SamlConstants.AttributeNames.AuthorityKind, null);
            if (string.IsNullOrEmpty(prefix))
            {
                writer.WriteString(authorityBinding.AuthorityKind.Name);
            }
            else
            {
                writer.WriteString(prefix + ":" + authorityBinding.AuthorityKind.Name);
            }
            writer.WriteEndAttribute();

            writer.WriteAttributeString(SamlConstants.AttributeNames.Location, null, authorityBinding.Location);

            writer.WriteAttributeString(SamlConstants.AttributeNames.Binding, null, authorityBinding.Binding);

            writer.WriteEndElement();
        }

        /// <summary>
        /// Gets a boolean indicating if the SecurityTokenHandler can Serialize Tokens. Return true by default.
        /// </summary>
        public override bool CanWriteToken
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Serializes the given SecurityToken to the XmlWriter.
        /// </summary>
        /// <param name="writer">XmlWriter into which the token is serialized.</param>
        /// <param name="token">SecurityToken to be serialized.</param>
        /// <exception cref="ArgumentNullException">Input parameter 'writer' or 'token' is null.</exception>
        /// <exception cref="SecurityTokenException">The given 'token' is not a SamlSecurityToken.</exception>
        public override void WriteToken(XmlWriter writer, SecurityToken token)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (token == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("token");
            }

            SamlSecurityToken samlSecurityToken = token as SamlSecurityToken;
            if (samlSecurityToken == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.ID4217, token.GetType(), typeof(SamlSecurityToken))));
            }

            WriteAssertion(writer, samlSecurityToken.Assertion);
        }

        /// <summary>
        /// Read the saml:AuthorizationDecisionStatement element.
        /// </summary>
        /// <param name="reader">XmlReader position at saml:AuthorizationDecisionStatement.</param>
        /// <returns>SamlAuthorizationDecisionStatement</returns>
        /// <exception cref="ArgumentNullException">The inpur parameter 'reader' is null.</exception>
        /// <exception cref="XmlException">The XmlReader is not positioned at a saml:AuthorizationDecisionStatement or
        /// the statement contains child elments that are unknown or invalid.</exception>
        protected virtual
        SamlAuthorizationDecisionStatement ReadAuthorizationDecisionStatement(XmlReader reader)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            if (!reader.IsStartElement(SamlConstants.ElementNames.AuthorizationDecisionStatement, SamlConstants.Namespace))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.ID4082, SamlConstants.ElementNames.AuthorizationDecisionStatement, SamlConstants.Namespace, reader.LocalName, reader.NamespaceURI)));
            }

            SamlAuthorizationDecisionStatement authzStatement = new SamlAuthorizationDecisionStatement();
            authzStatement.Resource = reader.GetAttribute(SamlConstants.AttributeNames.Resource, null);
            if (string.IsNullOrEmpty(authzStatement.Resource))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.ID4205)));
            }

            string decisionString = reader.GetAttribute(SamlConstants.AttributeNames.Decision, null);
            if (string.IsNullOrEmpty(decisionString))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.ID4204)));
            }

            if (decisionString.Equals(SamlAccessDecision.Deny.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                authzStatement.AccessDecision = SamlAccessDecision.Deny;
            }
            else if (decisionString.Equals(SamlAccessDecision.Permit.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                authzStatement.AccessDecision = SamlAccessDecision.Permit;
            }
            else
            {
                authzStatement.AccessDecision = SamlAccessDecision.Indeterminate;
            }

            reader.MoveToContent();
            reader.Read();

            if (reader.IsStartElement(SamlConstants.ElementNames.Subject, SamlConstants.Namespace))
            {
                authzStatement.SamlSubject = ReadSubject(reader);
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.ID4206)));
            }

            while (reader.IsStartElement())
            {
                if (reader.IsStartElement(SamlConstants.ElementNames.Action, SamlConstants.Namespace))
                {
                    authzStatement.SamlActions.Add(ReadAction(reader));
                }
                else if (reader.IsStartElement(SamlConstants.ElementNames.Evidence, SamlConstants.Namespace))
                {
                    if (authzStatement.Evidence != null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.ID4207)));
                    }

                    authzStatement.Evidence = ReadEvidence(reader);
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.ID4208, reader.LocalName, reader.NamespaceURI)));
                }
            }

            if (authzStatement.SamlActions.Count == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.ID4209)));
            }

            reader.MoveToContent();
            reader.ReadEndElement();

            return authzStatement;
        }

        /// <summary>
        /// Serialize a SamlAuthorizationDecisionStatement.
        /// </summary>
        /// <param name="writer">XmlWriter to which the SamlAuthorizationStatement is serialized.</param>
        /// <param name="statement">SamlAuthorizationDecisionStatement to serialize.</param>
        /// <exception cref="ArgumentNullException">The input parameter 'writer' or 'statement' is null.</exception>
        protected virtual void WriteAuthorizationDecisionStatement(XmlWriter writer, SamlAuthorizationDecisionStatement statement)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (statement == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("statement");
            }

            writer.WriteStartElement(SamlConstants.Prefix, SamlConstants.ElementNames.AuthorizationDecisionStatement, SamlConstants.Namespace);

            writer.WriteAttributeString(SamlConstants.AttributeNames.Decision, null, statement.AccessDecision.ToString());

            writer.WriteAttributeString(SamlConstants.AttributeNames.Resource, null, statement.Resource);

            WriteSubject(writer, statement.SamlSubject);

            foreach (SamlAction action in statement.SamlActions)
            {
                WriteAction(writer, action);
            }

            if (statement.Evidence != null)
            {
                WriteEvidence(writer, statement.Evidence);
            }

            writer.WriteEndElement();
        }

        /// <summary>
        /// Read the saml:Evidence element.
        /// </summary>
        /// <param name="reader">XmlReader positioned at saml:Evidence element.</param>
        /// <returns>SamlEvidence</returns>
        /// <exception cref="ArgumentNullException">The input parameter 'reader' is null.</exception>
        /// <exception cref="XmlException">The XmlReader is not positioned at a saml:Evidence element or 
        /// the element contains unrecognized or invalid child elements.</exception>
        protected virtual SamlEvidence ReadEvidence(XmlReader reader)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            if (!reader.IsStartElement(SamlConstants.ElementNames.Evidence, SamlConstants.Namespace))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.ID4082, SamlConstants.ElementNames.Evidence, SamlConstants.Namespace, reader.LocalName, reader.NamespaceURI)));
            }

            SamlEvidence evidence = new SamlEvidence();
            reader.ReadStartElement();

            while (reader.IsStartElement())
            {
                if (reader.IsStartElement(SamlConstants.ElementNames.AssertionIdReference, SamlConstants.Namespace))
                {
                    evidence.AssertionIdReferences.Add(reader.ReadElementString());
                }
                else if (reader.IsStartElement(SamlConstants.ElementNames.Assertion, SamlConstants.Namespace))
                {
                    evidence.Assertions.Add(ReadAssertion(reader));
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.ID4210, reader.LocalName, reader.NamespaceURI)));
                }
            }

            if ((evidence.AssertionIdReferences.Count == 0) && (evidence.Assertions.Count == 0))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.ID4211)));
            }

            reader.MoveToContent();
            reader.ReadEndElement();

            return evidence;
        }

        /// <summary>
        /// Serializes a given SamlEvidence.
        /// </summary>
        /// <param name="writer">XmlWriter to serialize the SamlEvidence.</param>
        /// <param name="evidence">SamlEvidence to be serialized.</param>
        /// <exception cref="ArgumentNullException">The input parameter 'evidence' is null.</exception>
        protected virtual void WriteEvidence(XmlWriter writer, SamlEvidence evidence)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (evidence == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("evidence");
            }

            writer.WriteStartElement(SamlConstants.Prefix, SamlConstants.ElementNames.Evidence, SamlConstants.Namespace);

            for (int i = 0; i < evidence.AssertionIdReferences.Count; i++)
            {
                writer.WriteElementString(SamlConstants.Prefix, SamlConstants.ElementNames.AssertionIdReference, SamlConstants.Namespace, evidence.AssertionIdReferences[i]);
            }

            for (int i = 0; i < evidence.Assertions.Count; i++)
            {
                WriteAssertion(writer, evidence.Assertions[i]);
            }

            writer.WriteEndElement();
        }

        /// <summary>
        /// Resolves the SecurityKeyIdentifier specified in a saml:Subject element. 
        /// </summary>
        /// <param name="subjectKeyIdentifier">SecurityKeyIdentifier to resolve into a key.</param>
        /// <returns>SecurityKey</returns>
        /// <exception cref="ArgumentNullException">The input parameter 'subjectKeyIdentifier' is null.</exception>
        protected virtual SecurityKey ResolveSubjectKeyIdentifier(SecurityKeyIdentifier subjectKeyIdentifier)
        {
            if (subjectKeyIdentifier == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("subjectKeyIdentifier");
            }

            if (this.Configuration == null)
            {
                throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID4274));
            }

            if (this.Configuration.ServiceTokenResolver == null)
            {
                throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID4276));
            }

            SecurityKey key = null;
            foreach (SecurityKeyIdentifierClause clause in subjectKeyIdentifier)
            {
                if (this.Configuration.ServiceTokenResolver.TryResolveSecurityKey(clause, out key))
                {
                    return key;
                }
            }

            if (subjectKeyIdentifier.CanCreateKey)
            {
                return subjectKeyIdentifier.CreateKey();
            }

            return null;
        }

        /// <summary>
        /// Resolves the Signing Key Identifier to a SecurityToken.
        /// </summary>
        /// <param name="assertion">The Assertion for which the Issuer token is to be resolved.</param>
        /// <param name="issuerResolver">The current SecurityTokenResolver associated with this handler.</param>
        /// <returns>Instance of SecurityToken</returns>
        /// <exception cref="ArgumentNullException">Input parameter 'assertion' is null.</exception>
        /// <exception cref="ArgumentNullException">Input parameter 'issuerResolver' is null.</exception>/// 
        /// <exception cref="SecurityTokenException">Unable to resolve token.</exception>
        protected virtual SecurityToken ResolveIssuerToken(SamlAssertion assertion, SecurityTokenResolver issuerResolver)
        {
            if (null == assertion)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("assertion");
            }

            if (null == issuerResolver)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("issuerResolver");
            }

            SecurityToken token;
            if (TryResolveIssuerToken(assertion, issuerResolver, out token))
            {
                return token;
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.ID4220)));
            }
        }

        /// <summary>
        /// Resolves the Signing Key Identifier to a SecurityToken.
        /// </summary>
        /// <param name="assertion">The Assertion for which the Issuer token is to be resolved.</param>
        /// <param name="issuerResolver">The current SecurityTokenResolver associated with this handler.</param>
        /// <param name="token">Resolved token.</param>
        /// <returns>True if token is resolved.</returns>
        /// <exception cref="ArgumentNullException">Input parameter 'assertion' is null.</exception>
        protected virtual bool TryResolveIssuerToken(SamlAssertion assertion, SecurityTokenResolver issuerResolver, out SecurityToken token)
        {
            if (null == assertion)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("assertion");
            }

            if (assertion.SigningCredentials != null
                && assertion.SigningCredentials.SigningKeyIdentifier != null
                && issuerResolver != null)
            {
                SecurityKeyIdentifier keyIdentifier = assertion.SigningCredentials.SigningKeyIdentifier;
                return issuerResolver.TryResolveToken(keyIdentifier, out token);
            }
            else
            {
                token = null;
                return false;
            }
        }

        /// <summary>
        /// Reads the ds:KeyInfo element inside the Saml Signature.
        /// </summary>
        /// <param name="reader">An XmlReader that can be positioned at a ds:KeyInfo element.</param>
        /// <param name="assertion">The assertion that is having the signature checked.</param>
        /// <returns>The <see cref="SecurityKeyIdentifier"/> that defines the key to use to check the signature.</returns>
        /// <exception cref="ArgumentNullException">The input parameter 'reader' is null.</exception>
        /// <exception cref="InvalidOperationException">Unable to read the KeyIdentifier from the XmlReader.</exception>
        /// <remarks>If the reader is not positioned at a ds:KeyInfo element, the <see cref="SecurityKeyIdentifier"/> returned will
        /// contain a single <see cref="SecurityKeyIdentifierClause"/> of type <see cref="EmptySecurityKeyIdentifierClause"/></remarks>
        protected virtual SecurityKeyIdentifier ReadSigningKeyInfo(XmlReader reader, SamlAssertion assertion)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            SecurityKeyIdentifier ski;

            if (KeyInfoSerializer.CanReadKeyIdentifier(reader))
            {
                ski = KeyInfoSerializer.ReadKeyIdentifier(reader);
            }
            else
            {
                KeyInfo keyInfo = new KeyInfo(KeyInfoSerializer);
                keyInfo.ReadXml(XmlDictionaryReader.CreateDictionaryReader(reader));
                ski = keyInfo.KeyIdentifier;
            }

            // no key info
            if (ski.Count == 0)
            {
                return new SecurityKeyIdentifier(new SamlSecurityKeyIdentifierClause(assertion));
            }

            return ski;
        }

        /// <summary>
        /// Serializes the Signing SecurityKeyIdentifier.
        /// </summary>
        /// <param name="writer">XmlWriter to serialize the SecurityKeyIdentifier.</param>
        /// <param name="signingKeyIdentifier">Signing SecurityKeyIdentifier.</param>
        /// <exception cref="ArgumentNullException">The input parameter 'writer' or 'signingKeyIdentifier' is null.</exception>
        protected virtual void WriteSigningKeyInfo(XmlWriter writer, SecurityKeyIdentifier signingKeyIdentifier)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (signingKeyIdentifier == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("signingKeyIdentifier");
            }

            if (KeyInfoSerializer.CanWriteKeyIdentifier(signingKeyIdentifier))
            {
                KeyInfoSerializer.WriteKeyIdentifier(writer, signingKeyIdentifier);
                return;
            }

            throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID4221, signingKeyIdentifier));
        }

        /// <summary>
        /// Validates the subject in each statement in the collection of Saml statements.
        /// </summary>
        /// <param name="statements"></param>
        private void ValidateStatements(IList<SamlStatement> statements)
        {
            if (statements == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("statements");
            }
            List<SamlSubject> subjects = new List<SamlSubject>();
            foreach (SamlStatement statement in statements)
            {
                if (statement is SamlAttributeStatement)
                {
                    subjects.Add((statement as SamlAttributeStatement).SamlSubject);
                }

                if (statement is SamlAuthenticationStatement)
                {
                    subjects.Add((statement as SamlAuthenticationStatement).SamlSubject);
                }

                if (statement is SamlAuthorizationDecisionStatement)
                {
                    subjects.Add((statement as SamlAuthorizationDecisionStatement).SamlSubject);
                }
                //
                // skip all custom statements
                //

            }

            if (subjects.Count == 0)
            {
                //
                // All statements are custom and we cannot validate
                //
                return;
            }
            string requiredSubjectName = subjects[0].Name;
            string requiredSubjectFormat = subjects[0].NameFormat;
            string requiredSubjectQualifier = subjects[0].NameQualifier;

            foreach (SamlSubject subject in subjects)
            {
                if (!StringComparer.Ordinal.Equals(subject.Name, requiredSubjectName) ||
                     !StringComparer.Ordinal.Equals(subject.NameFormat, requiredSubjectFormat) ||
                     !StringComparer.Ordinal.Equals(subject.NameQualifier, requiredSubjectQualifier))
                {
                    //
                    // The SamlSubjects in the statements do not match
                    //
                    throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID4225, subject));
                }
            }
        }

        #endregion

        /// <summary>
        /// Returns the saml token's token type that is supported by this handler.
        /// </summary>
        public override string[] GetTokenTypeIdentifiers()
        {
            return _tokenTypeIdentifiers;
        }

        /// <summary>
        /// Gets or Sets a SecurityTokenSerializers that will be used to serialize and deserializer
        /// SecurtyKeyIdentifier. For example, SamlSubject SecurityKeyIdentifier or Signature 
        /// SecurityKeyIdentifier.
        /// </summary>
        public SecurityTokenSerializer KeyInfoSerializer
        {
            get
            {
                if (_keyInfoSerializer == null)
                {
                    lock (_syncObject)
                    {
                        if (_keyInfoSerializer == null)
                        {
                            SecurityTokenHandlerCollection sthc = (ContainingCollection != null) ?
                                ContainingCollection : SecurityTokenHandlerCollection.CreateDefaultSecurityTokenHandlerCollection();
                            _keyInfoSerializer = new SecurityTokenSerializerAdapter(sthc);
                        }
                    }
                }

                return _keyInfoSerializer;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }

                _keyInfoSerializer = value;
            }
        }

        /// <summary>
        /// Gets the System.Type of the SecurityToken is supported by ththis handler.
        /// </summary>
        public override Type TokenType
        {
            get { return typeof(SamlSecurityToken); }
        }

        /// <summary>
        /// Gets or sets the <see cref="SamlSecurityTokenRequirement"/>
        /// </summary>
        public SamlSecurityTokenRequirement SamlSecurityTokenRequirement
        {
            get
            {
                return _samlSecurityTokenRequirement;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                _samlSecurityTokenRequirement = value;
            }
        }

        // This thin wrapper is used to pass a serializer down into the 
        // EnvelopedSignatureReader that will use the Saml11SecurityTokenHandlers's
        // ReadKeyInfo method to read the KeyInfo.
        class WrappedSerializer : SecurityTokenSerializer
        {
            SamlSecurityTokenHandler _parent;
            SamlAssertion _assertion;

            public WrappedSerializer(SamlSecurityTokenHandler parent, SamlAssertion assertion)
            {
                if (parent == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("parent");
                }

                _parent = parent;
                _assertion = assertion;
            }

            protected override bool CanReadKeyIdentifierClauseCore(XmlReader reader)
            {
                return false;
            }

            protected override bool CanReadKeyIdentifierCore(XmlReader reader)
            {
                return true;
            }

            protected override bool CanReadTokenCore(XmlReader reader)
            {
                return false;
            }

            protected override bool CanWriteKeyIdentifierClauseCore(SecurityKeyIdentifierClause keyIdentifierClause)
            {
                return false;
            }

            protected override bool CanWriteKeyIdentifierCore(SecurityKeyIdentifier keyIdentifier)
            {
                return false;
            }

            protected override bool CanWriteTokenCore(SecurityToken token)
            {
                return false;
            }

            protected override SecurityKeyIdentifierClause ReadKeyIdentifierClauseCore(XmlReader reader)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }

            protected override SecurityKeyIdentifier ReadKeyIdentifierCore(XmlReader reader)
            {
                return _parent.ReadSigningKeyInfo(reader, _assertion);
            }

            protected override SecurityToken ReadTokenCore(XmlReader reader, SecurityTokenResolver tokenResolver)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }

            protected override void WriteKeyIdentifierClauseCore(XmlWriter writer, SecurityKeyIdentifierClause keyIdentifierClause)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }

            protected override void WriteKeyIdentifierCore(XmlWriter writer, SecurityKeyIdentifier keyIdentifier)
            {
                _parent.WriteSigningKeyInfo(writer, keyIdentifier);
            }

            protected override void WriteTokenCore(XmlWriter writer, SecurityToken token)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }
        }
    }
}
