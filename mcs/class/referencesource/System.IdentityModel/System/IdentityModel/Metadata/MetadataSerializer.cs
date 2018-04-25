//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel.Metadata
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.IdentityModel.Configuration;
    using System.IdentityModel.Diagnostics;
    using System.IdentityModel.Protocols.WSFederation;
    using System.IdentityModel.Protocols.WSTrust;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.IO;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel.Security;
    using System.Text;
    using System.Xml;
    using System.Xml.Schema;

    /// <summary>
    /// Provides support for Metadata Serialization
    /// </summary>
    public class MetadataSerializer
    {
#pragma warning disable 1591
        public const string LanguagePrefix = "xml";
        public const string LanguageLocalName = "lang";
        public const string LanguageAttribute = LanguagePrefix + ":" + LanguageLocalName;
        public const string LanguageNamespaceUri = "http://www.w3.org/XML/1998/namespace";
#pragma warning restore 1591

        const string _uriReference = "_metadata";
        List<string> _trustedIssuers = new List<string>();
        SecurityTokenSerializer _tokenSerializer;

        /// <summary>
        /// Initializes an instance of <see cref="MetadataSerializer"/>
        /// </summary>
        public MetadataSerializer()
            : this(new KeyInfoSerializer(true))
        {
        }


        /// <summary>
        /// Initializes an instance of <see cref="MetadataSerializer"/>
        /// </summary>
        /// <param name="tokenSerializer">Security Token Serializer</param>
        /// <exception cref="ArgumentNullException">The parameter tokenSerializer is null.</exception>
        public MetadataSerializer(SecurityTokenSerializer tokenSerializer)
        {
            if (tokenSerializer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("tokenSerializer");
            }

            _tokenSerializer = tokenSerializer;
            TrustedStoreLocation = IdentityConfiguration.DefaultTrustedStoreLocation;
            CertificateValidationMode = IdentityConfiguration.DefaultCertificateValidationMode;
            RevocationMode = IdentityConfiguration.DefaultRevocationMode;
        }

        /// <summary>
        /// Creates an application service descriptor.
        /// </summary>
        /// <returns>An application service descriptor.</returns>
        protected virtual ApplicationServiceDescriptor CreateApplicationServiceInstance()
        {
            return new ApplicationServiceDescriptor();
        }

        /// <summary>
        /// Creates a contact person.
        /// </summary>
        /// <returns>A contact person.</returns>
        protected virtual ContactPerson CreateContactPersonInstance()
        {
            return new ContactPerson();
        }

        /// <summary>
        /// Creates an endpoint.
        /// </summary>
        /// <returns>An endpoint.</returns>
        protected virtual ProtocolEndpoint CreateProtocolEndpointInstance()
        {
            return new ProtocolEndpoint();
        }

        /// <summary>
        /// Creates entities descriptor.
        /// </summary>
        /// <returns>The entities descriptor. </returns>
        protected virtual EntitiesDescriptor CreateEntitiesDescriptorInstance()
        {
            return new EntitiesDescriptor();
        }

        /// <summary>
        /// Creates an entity descriptor.
        /// </summary>
        /// <returns>The entity descriptor.</returns>
        protected virtual EntityDescriptor CreateEntityDescriptorInstance()
        {
            return new EntityDescriptor();
        }

        /// <summary>
        /// Creates an idpsso descriptor.
        /// </summary>
        /// <returns>An idpsso descriptor.</returns>
        protected virtual IdentityProviderSingleSignOnDescriptor CreateIdentityProviderSingleSignOnDescriptorInstance()
        {
            return new IdentityProviderSingleSignOnDescriptor();
        }

        /// <summary>
        /// Creates an indexed enpoint.
        /// </summary>
        /// <returns>An indexed endpoint.</returns>
        protected virtual IndexedProtocolEndpoint CreateIndexedProtocolEndpointInstance()
        {
            return new IndexedProtocolEndpoint();
        }

        /// <summary>
        /// Creates a key descriptor.
        /// </summary>
        /// <returns>The key descriptor.</returns>
        protected virtual KeyDescriptor CreateKeyDescriptorInstance()
        {
            return new KeyDescriptor();
        }

        /// <summary>
        /// Creates a localized name.
        /// </summary>
        /// <returns>The localized name.</returns>
        protected virtual LocalizedName CreateLocalizedNameInstance()
        {
            return new LocalizedName();
        }

        /// <summary>
        /// Creates a localized uri.
        /// </summary>
        /// <returns>A localized uri.</returns>
        protected virtual LocalizedUri CreateLocalizedUriInstance()
        {
            return new LocalizedUri();
        }

        /// <summary>
        /// Creates an organization.
        /// </summary>
        /// <returns>An organization.</returns>
        protected virtual Organization CreateOrganizationInstance()
        {
            return new Organization();
        }

        /// <summary>
        /// Creates a security token service descriptor.
        /// </summary>
        /// <returns>A security token service descriptor.</returns>
        protected virtual SecurityTokenServiceDescriptor CreateSecurityTokenServiceDescriptorInstance()
        {
            return new SecurityTokenServiceDescriptor();
        }

        /// <summary>
        /// Creates an Spsso descriptor.
        /// </summary>
        /// <returns>An Spsso descriptor.</returns>
        protected virtual ServiceProviderSingleSignOnDescriptor CreateServiceProviderSingleSignOnDescriptorInstance()
        {
            return new ServiceProviderSingleSignOnDescriptor();
        }

        /// <summary>
        /// Returns the respective contact type from a string contact type.
        /// </summary>
        /// <param name="conactType">The string type.</param>
        /// <param name="found">If found a match.</param>
        /// <returns>The contact type.</returns>
        private static ContactType GetContactPersonType(string conactType, out bool found)
        {
            if (conactType == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("conactType");
            }
            found = true;
            if (StringComparer.Ordinal.Equals(conactType, "unspecified"))
            {
                return ContactType.Unspecified;
            }
            else if (StringComparer.Ordinal.Equals(conactType, "administrative"))
            {
                return ContactType.Administrative;
            }
            else if (StringComparer.Ordinal.Equals(conactType, "billing"))
            {
                return ContactType.Billing;
            }
            else if (StringComparer.Ordinal.Equals(conactType, "other"))
            {
                return ContactType.Other;
            }
            else if (StringComparer.Ordinal.Equals(conactType, "support"))
            {
                return ContactType.Support;
            }
            else if (StringComparer.Ordinal.Equals(conactType, "technical"))
            {
                return ContactType.Technical;
            }
            found = false;
            return ContactType.Unspecified;

        }

        private static KeyType GetKeyDescriptorType(string keyType)
        {
            if (keyType == null)
            {
                return KeyType.Unspecified;
            }
            else if (StringComparer.Ordinal.Equals(keyType, "encryption"))
            {
                return KeyType.Encryption;
            }
            else if (StringComparer.Ordinal.Equals(keyType, "signing"))
            {
                return KeyType.Signing;
            }

            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MetadataSerializationException(SR.GetString(SR.ID3202, Saml2MetadataConstants.Attributes.Use, keyType)));
        }

        /// <summary>
        /// Reads application service descriptor.
        /// </summary>
        /// <param name="reader">Xml reader.</param>
        /// <returns>An application service descriptor.</returns>
        /// <exception cref="ArgumentNullException">The parameter reader is null.</exception>
        protected virtual ApplicationServiceDescriptor ReadApplicationServiceDescriptor(XmlReader reader)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            ApplicationServiceDescriptor appService = CreateApplicationServiceInstance();
            ReadWebServiceDescriptorAttributes(reader, appService);
            ReadCustomAttributes<ApplicationServiceDescriptor>(reader, appService);

            bool isEmpty = reader.IsEmptyElement;
            reader.ReadStartElement();
            if (!isEmpty)
            {
                while (reader.IsStartElement())
                {
                    if (reader.IsStartElement(FederationMetadataConstants.Elements.ApplicationServiceEndpoint, FederationMetadataConstants.Namespace))
                    {
                        isEmpty = reader.IsEmptyElement;
                        reader.ReadStartElement();
                        if (!isEmpty && reader.IsStartElement())
                        {
                            EndpointReference address = EndpointReference.ReadFrom(reader);
                            appService.Endpoints.Add(address);
                            reader.ReadEndElement();
                        }
                    }
                    else if (reader.IsStartElement(FederationMetadataConstants.Elements.PassiveRequestorEndpoint, FederationMetadataConstants.Namespace))
                    {
                        isEmpty = reader.IsEmptyElement;
                        reader.ReadStartElement();
                        if (!isEmpty && reader.IsStartElement())
                        {
                            EndpointReference address = EndpointReference.ReadFrom(reader);
                            appService.PassiveRequestorEndpoints.Add(address);
                            reader.ReadEndElement();
                        }
                    }
                    else if (ReadWebServiceDescriptorElement(reader, appService))
                    {
                        // Do nothing
                    }
                    else if (ReadCustomElement<ApplicationServiceDescriptor>(reader, appService))
                    {
                        // Do nothing.
                    }
                    else
                    {
                        reader.Skip();
                    }
                }

                // SecurityTokenService
                reader.ReadEndElement();
            }

            return appService;
        }

        /// <summary>
        /// Reads a contact person.
        /// </summary>
        /// <param name="reader">Xml reader.</param>
        /// <returns>A contact person.</returns>
        /// <exception cref="ArgumentNullException">The parameter reader is null.</exception>
        protected virtual ContactPerson ReadContactPerson(XmlReader reader)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }
            ContactPerson person = CreateContactPersonInstance();

            string contactType = reader.GetAttribute(Saml2MetadataConstants.Attributes.ContactType, null);
            bool foundKey = false;

            person.Type = GetContactPersonType(contactType, out foundKey);
            if (!foundKey)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MetadataSerializationException(SR.GetString(SR.ID3201, typeof(ContactType), contactType)));
            }

            ReadCustomAttributes<ContactPerson>(reader, person);

            bool isEmpty = reader.IsEmptyElement;
            reader.ReadStartElement(); // <ContactPerson>
            if (!isEmpty)
            {
                while (reader.IsStartElement())
                {
                    if (reader.IsStartElement(Saml2MetadataConstants.Elements.Company, Saml2MetadataConstants.Namespace))
                    {
                        person.Company = reader.ReadElementContentAsString(Saml2MetadataConstants.Elements.Company, Saml2MetadataConstants.Namespace);
                    }
                    else if (reader.IsStartElement(Saml2MetadataConstants.Elements.GivenName, Saml2MetadataConstants.Namespace))
                    {
                        person.GivenName = reader.ReadElementContentAsString(Saml2MetadataConstants.Elements.GivenName, Saml2MetadataConstants.Namespace);
                    }
                    else if (reader.IsStartElement(Saml2MetadataConstants.Elements.Surname, Saml2MetadataConstants.Namespace))
                    {
                        person.Surname = reader.ReadElementContentAsString(Saml2MetadataConstants.Elements.Surname, Saml2MetadataConstants.Namespace);
                    }
                    else if (reader.IsStartElement(Saml2MetadataConstants.Elements.EmailAddress, Saml2MetadataConstants.Namespace))
                    {
                        string emailId = reader.ReadElementContentAsString(Saml2MetadataConstants.Elements.EmailAddress, Saml2MetadataConstants.Namespace);
                        if (!String.IsNullOrEmpty(emailId))
                        {
                            person.EmailAddresses.Add(emailId);
                        }
                    }
                    else if (reader.IsStartElement(Saml2MetadataConstants.Elements.TelephoneNumber, Saml2MetadataConstants.Namespace))
                    {
                        string phone = reader.ReadElementContentAsString(Saml2MetadataConstants.Elements.TelephoneNumber, Saml2MetadataConstants.Namespace);
                        if (!String.IsNullOrEmpty(phone))
                        {
                            person.TelephoneNumbers.Add(phone);
                        }
                    }
                    else if (ReadCustomElement<ContactPerson>(reader, person))
                    {
                        // Do nothing
                    }
                    else
                    {
                        reader.Skip();
                    }

                }
                reader.ReadEndElement(); // </ContactPerson>
            }

            // No mandatory elements to be validated.
            return person;
        }

        /// <summary>
        /// Extensibility point for reading custom attributes.
        /// </summary>
        /// <typeparam name="T">The type of element.</typeparam>
        /// <param name="reader">Xml reader.</param>
        /// <param name="target">An object of type T.</param>
        protected virtual void ReadCustomAttributes<T>(XmlReader reader, T target)
        {
            // Extensibility point only. Do Nothing.
        }

        /// <summary>
        /// Extensibility point for reading custom elements. By default this returns false.
        /// </summary>
        /// <typeparam name="T">The type of element.</typeparam>
        /// <param name="reader">Xml reader.</param>
        /// <param name="target">An object of type T.</param>
        /// <returns>True if an element of type T is read, else false.</returns>
        protected virtual bool ReadCustomElement<T>(XmlReader reader, T target)
        {
            // Extensibility point only. Do Nothing.
            return false;
        }

        /// <summary>
        /// Extensibility point for reading custom RoleDescriptors.
        /// </summary>
        /// <param name="xsiType">The xsi type</param>
        /// <param name="reader">Xml reader</param>
        /// <param name="entityDescriptor">The entity descriptor for adding the Role Descriptors</param>
        protected virtual void ReadCustomRoleDescriptor(string xsiType, XmlReader reader, EntityDescriptor entityDescriptor)
        {
            //
            // Extensibility point: Based on the xsiType, overriden implementations have the ability to read the RoleDescriptor 
            // attributes from a (##other) namespace and add the Role Descriptors to the entityDescriptor
            //
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            TraceUtility.TraceString(System.Diagnostics.TraceEventType.Warning, SR.GetString(SR.ID3274, xsiType));
            reader.Skip();
        }

        /// <summary>
        /// Returns the <see cref="DisplayClaim"/> from the <paramref name="reader"/>.
        /// </summary>
        /// <param name="reader">XML reader.</param>
        /// <returns>The display claim.</returns>
        /// <exception cref="ArgumentNullException">The parameter reader is null.</exception>
        /// <exception cref="MetadataSerializationException">Thrown if the XML is not well-formed.</exception>
        protected virtual DisplayClaim ReadDisplayClaim(XmlReader reader)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            //
            // This is out of scope for extensibility.
            //
            string claimType = reader.GetAttribute(WSFederationMetadataConstants.Attributes.Uri, null);
            if (!UriUtil.CanCreateValidUri(claimType, UriKind.Absolute))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MetadataSerializationException(SR.GetString(SR.ID3202, WSAuthorizationConstants.Elements.ClaimType, claimType)));
            }
            DisplayClaim claim = new DisplayClaim(claimType);

            bool isOptional = true;
            string optionalString = reader.GetAttribute(WSFederationMetadataConstants.Attributes.Optional);
            if (!String.IsNullOrEmpty(optionalString))
            {
                try
                {
                    isOptional = XmlConvert.ToBoolean(optionalString.ToLowerInvariant());
                }
                catch (FormatException)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MetadataSerializationException(SR.GetString(SR.ID3202, WSFederationMetadataConstants.Attributes.Optional, optionalString)));
                }
            }
            claim.Optional = isOptional;

            bool isEmpty = reader.IsEmptyElement;
            reader.ReadStartElement();
            if (!isEmpty)
            {
                while (reader.IsStartElement())
                {
                    if (reader.IsStartElement(WSAuthorizationConstants.Elements.DisplayName, WSAuthorizationConstants.Namespace))
                    {
                        claim.DisplayTag = reader.ReadElementContentAsString(WSAuthorizationConstants.Elements.DisplayName, WSAuthorizationConstants.Namespace);
                    }
                    else if (reader.IsStartElement(WSAuthorizationConstants.Elements.Description, WSAuthorizationConstants.Namespace))
                    {
                        claim.Description = reader.ReadElementContentAsString(WSAuthorizationConstants.Elements.Description, WSAuthorizationConstants.Namespace);
                    }
                    else
                    {
                        // Move on
                        reader.Skip();
                    }
                }
                reader.ReadEndElement();
            }
            return claim;
        }

        /// <summary>
        /// Reads entities descriptor.
        /// </summary>
        /// <param name="reader">Xml reader.</param>
        /// <param name="tokenResolver">The security token resolver.</param>
        /// <returns>The entities descriptor.</returns>
        /// <exception cref="ArgumentNullException">The parameter reader is null.</exception>
        /// <exception cref="MetadataSerializationException">Thrown if the XML is not well-formed.</exception>
        protected virtual EntitiesDescriptor ReadEntitiesDescriptor(XmlReader reader, SecurityTokenResolver tokenResolver)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            EntitiesDescriptor resultEntityGroup = CreateEntitiesDescriptorInstance();

            //
            // There may be embedded signed XML elements. So we need to plumb the SecurityTokenSerializer and tokenResolver
            //
            EnvelopedSignatureReader envelopeReader = new EnvelopedSignatureReader(reader, SecurityTokenSerializer, tokenResolver, false, false, true);

            string name = envelopeReader.GetAttribute(Saml2MetadataConstants.Attributes.EntityGroupName, null);
            if (!String.IsNullOrEmpty(name))
            {
                resultEntityGroup.Name = name;
            }

            ReadCustomAttributes<EntitiesDescriptor>(envelopeReader, resultEntityGroup);

            bool isEmpty = envelopeReader.IsEmptyElement;
            envelopeReader.ReadStartElement(); // <EntitiesDescriptor>
            if (!isEmpty)
            {
                while (envelopeReader.IsStartElement())
                {
                    if (envelopeReader.IsStartElement(Saml2MetadataConstants.Elements.EntityDescriptor, Saml2MetadataConstants.Namespace))
                    {
                        resultEntityGroup.ChildEntities.Add(ReadEntityDescriptor(envelopeReader, tokenResolver));
                    }
                    else if (envelopeReader.IsStartElement(Saml2MetadataConstants.Elements.EntitiesDescriptor, Saml2MetadataConstants.Namespace))
                    {
                        resultEntityGroup.ChildEntityGroups.Add(ReadEntitiesDescriptor(envelopeReader, tokenResolver));
                    }
                    else if (envelopeReader.TryReadSignature())
                    {
                        // Do nothng
                    }
                    else if (ReadCustomElement<EntitiesDescriptor>(envelopeReader, resultEntityGroup))
                    {
                        // Do nothing.
                    }
                    else
                    {
                        envelopeReader.Skip();
                    }

                }
                envelopeReader.ReadEndElement(); // </EntitiesDescriptor>
            }

            resultEntityGroup.SigningCredentials = envelopeReader.SigningCredentials;

            if (resultEntityGroup.SigningCredentials != null)
            {
                ValidateSigningCredential(resultEntityGroup.SigningCredentials);
            }

            if (resultEntityGroup.ChildEntityGroups.Count == 0 && resultEntityGroup.ChildEntities.Count == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MetadataSerializationException(SR.GetString(SR.ID3200, Saml2MetadataConstants.Elements.EntityDescriptor)));
            }
            foreach (EntityDescriptor entity in resultEntityGroup.ChildEntities)
            {
                if (!String.IsNullOrEmpty(entity.FederationId))
                {
                    if (!StringComparer.Ordinal.Equals(entity.FederationId, resultEntityGroup.Name))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MetadataSerializationException(SR.GetString(SR.ID3202, WSFederationMetadataConstants.Attributes.FederationId, entity.FederationId)));
                    }
                }
            }
            return resultEntityGroup;
        }

        /// <summary>
        /// Gets or sets the validation mode of the X509 certificate that is used to sign the metadata document.
        /// </summary>
        public X509CertificateValidationMode CertificateValidationMode
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the revocation mode of the X509 certificate that is used to sign the metadata document.
        /// </summary>
        public X509RevocationMode RevocationMode
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the trusted store location of the X509 certificate that is used to sign the metadata document.
        /// </summary>
        public StoreLocation TrustedStoreLocation
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the certificate validator of the X509 certificate that is used to sign the metadata document.
        /// </summary>
        public X509CertificateValidator CertificateValidator
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the list of trusted issuer that this serializer instance trusts to sign the metadata docuemnt.
        /// </summary>
        public List<string> TrustedIssuers
        {
            get { return _trustedIssuers;  }
        }

        /// <summary>
        /// Validates the signing credential of the metadata document.
        /// </summary>
        /// <param name="signingCredentials">The signing credential used to sign the metadata document.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="signingCredentials"/> is null.</exception>
        protected virtual void ValidateSigningCredential(SigningCredentials signingCredentials)
        {
            if (signingCredentials == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("signingCredentials");
            }

            if (CertificateValidationMode != X509CertificateValidationMode.Custom)
            {
                CertificateValidator = X509Util.CreateCertificateValidator(CertificateValidationMode, RevocationMode, TrustedStoreLocation);
            }
            else if (CertificateValidationMode == X509CertificateValidationMode.Custom && CertificateValidator == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ID4280)));
            }

            X509Certificate2 certificate = GetMetadataSigningCertificate(signingCredentials.SigningKeyIdentifier);

            ValidateIssuer(certificate);
            CertificateValidator.Validate(certificate);
        }

        /// <summary>
        /// Validates the certificate that signed the metadata document against the TrustedIssuers. This method is invoked by the ValidateSigningCredential method.
        /// By default, this method does not perform any validation. Provide your own implementation to perform trusted issuer validation.
        /// </summary>
        /// <param name="signingCertificate">The signing certificate.</param>
        protected virtual void ValidateIssuer(X509Certificate2 signingCertificate)
        {
            // No-op by default.
        }

        /// <summary>
        /// Gets the <see cref="X509Certificate2"/> instance created from the <paramref name="ski"/>.
        /// By default, this method only supports <see cref="X509RawDataKeyIdentifierClause"/>. Override this method
        /// to support other key identifier clauses. This method is invoked by the ValidateSigningCredential method.
        /// </summary>
        /// <param name="ski">The security key identifier instance.</param>
        /// <returns>An <see cref="X509Certificate2"/> instance.</returns>
        protected virtual X509Certificate2 GetMetadataSigningCertificate( SecurityKeyIdentifier ski )
        {
            if (ski == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("ski");
            }

            X509RawDataKeyIdentifierClause clause = null;
            if (ski.TryFind<X509RawDataKeyIdentifierClause>(out clause))
            {
                return new X509Certificate2(clause.GetX509RawData());
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ID8029)));
            }
        }

        /// <summary>
        /// Reads an entity descriptor.
        /// </summary>
        /// <param name="inputReader">The xml reader.</param>
        /// <param name="tokenResolver">The security token resolver.</param>
        /// <returns>An entity descriptor.</returns>
        /// <exception cref="ArgumentNullException">The parameter inputReader is null.</exception>
        protected virtual EntityDescriptor ReadEntityDescriptor(XmlReader inputReader, SecurityTokenResolver tokenResolver)
        {
            if (inputReader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("inputReader");
            }

            EntityDescriptor resultEntity = CreateEntityDescriptorInstance();

            //
            // There may be embedded signed XML elements. So we need to plumb the SecurityTokenSerializer and tokenResolver
            // IDFX 

            EnvelopedSignatureReader reader = new EnvelopedSignatureReader(inputReader, SecurityTokenSerializer, tokenResolver, false, false, true);

            // EntityID is mandatory - relaxed as per FIP 9935
            string entityId = reader.GetAttribute(Saml2MetadataConstants.Attributes.EntityId, null);
            if (!String.IsNullOrEmpty(entityId))
            {
                resultEntity.EntityId = new EntityId(entityId);
            }

            // FederationID is optional
            string fedId = reader.GetAttribute(WSFederationMetadataConstants.Attributes.FederationId, WSFederationMetadataConstants.Namespace);
            if (!String.IsNullOrEmpty(fedId))
            {
                resultEntity.FederationId = fedId;
            }

            ReadCustomAttributes<EntityDescriptor>(reader, resultEntity);

            bool isEmpty = reader.IsEmptyElement;
            reader.ReadStartElement();
            if (!isEmpty)
            {
                while (reader.IsStartElement())
                {
                    if (reader.IsStartElement(Saml2MetadataConstants.Elements.SpssoDescriptor, Saml2MetadataConstants.Namespace))
                    {
                        resultEntity.RoleDescriptors.Add(ReadServiceProviderSingleSignOnDescriptor(reader));
                    }
                    else if (reader.IsStartElement(Saml2MetadataConstants.Elements.IdpssoDescriptor, Saml2MetadataConstants.Namespace))
                    {
                        resultEntity.RoleDescriptors.Add(ReadIdentityProviderSingleSignOnDescriptor(reader));
                    }
                    else if (reader.IsStartElement(Saml2MetadataConstants.Elements.RoleDescriptor, Saml2MetadataConstants.Namespace))
                    {
                        string xsiType = reader.GetAttribute("type", XmlSchema.InstanceNamespace);

                        if (!String.IsNullOrEmpty(xsiType))
                        {
                            int index = xsiType.IndexOf(":", 0, StringComparison.Ordinal);
                            if (index < 0)
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MetadataSerializationException(SR.GetString(SR.ID3207, "xsi:type", Saml2MetadataConstants.Elements.RoleDescriptor, xsiType)));
                            }
                            string prefix = xsiType.Substring(0, index);
                            string ns = reader.LookupNamespace(prefix);

                            if (String.IsNullOrEmpty(ns))
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MetadataSerializationException(SR.GetString(SR.ID3202, prefix, ns)));
                            }
                            else if (!StringComparer.Ordinal.Equals(ns, FederationMetadataConstants.Namespace))
                            {
                                ReadCustomRoleDescriptor(xsiType, reader, resultEntity);
                            }
                            else if (StringComparer.Ordinal.Equals(xsiType, prefix + ":" + FederationMetadataConstants.Elements.ApplicationServiceType))
                            {
                                resultEntity.RoleDescriptors.Add(ReadApplicationServiceDescriptor(reader));
                            }
                            else if (StringComparer.Ordinal.Equals(xsiType, prefix + ":" + FederationMetadataConstants.Elements.SecurityTokenServiceType))
                            {
                                resultEntity.RoleDescriptors.Add(ReadSecurityTokenServiceDescriptor(reader));
                            }
                            else
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MetadataSerializationException(SR.GetString(SR.ID3207, "xsi:type", Saml2MetadataConstants.Elements.RoleDescriptor, xsiType)));
                            }
                        }
                        else
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MetadataSerializationException(SR.GetString(SR.ID0001, "xsi:type", Saml2MetadataConstants.Elements.RoleDescriptor)));
                        }
                    }
                    else if (reader.IsStartElement(Saml2MetadataConstants.Elements.Organization, Saml2MetadataConstants.Namespace))
                    {
                        resultEntity.Organization = ReadOrganization(reader);
                    }
                    else if (reader.IsStartElement(Saml2MetadataConstants.Elements.ContactPerson, Saml2MetadataConstants.Namespace))
                    {
                        resultEntity.Contacts.Add(ReadContactPerson(reader));
                    }
                    else if (reader.TryReadSignature())
                    {
                        // Do nothing
                    }
                    else if (ReadCustomElement<EntityDescriptor>(reader, resultEntity))
                    {
                        // Do nothing.
                    }
                    else
                    {
                        reader.Skip();
                    }
                }
                reader.ReadEndElement();
            }

            resultEntity.SigningCredentials = reader.SigningCredentials;
            if (resultEntity.SigningCredentials != null)
            {
                ValidateSigningCredential(resultEntity.SigningCredentials);
            }

            // Elements are optional. Mandatory attributes already validated.

            return resultEntity;
        }

        /// <summary>
        /// Reads an idpsso descriptor.
        /// </summary>
        /// <param name="reader">The xml reader.</param>
        /// <returns>An idpsso descriptor.</returns>
        /// <exception cref="ArgumentNullException">The parameter reader is null.</exception>
        protected virtual IdentityProviderSingleSignOnDescriptor ReadIdentityProviderSingleSignOnDescriptor(XmlReader reader)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            IdentityProviderSingleSignOnDescriptor idpssoDescriptor = CreateIdentityProviderSingleSignOnDescriptorInstance();
            ReadSingleSignOnDescriptorAttributes(reader, idpssoDescriptor);
            ReadCustomAttributes<IdentityProviderSingleSignOnDescriptor>(reader, idpssoDescriptor);

            string wantAuthnRequestSignedAttribute = reader.GetAttribute(Saml2MetadataConstants.Attributes.WantAuthenticationRequestsSigned);
            if (!String.IsNullOrEmpty(wantAuthnRequestSignedAttribute))
            {
                try
                {
                    idpssoDescriptor.WantAuthenticationRequestsSigned = XmlConvert.ToBoolean(wantAuthnRequestSignedAttribute.ToLowerInvariant());
                }
                catch (FormatException)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MetadataSerializationException(SR.GetString(
                        SR.ID3202, Saml2MetadataConstants.Attributes.WantAuthenticationRequestsSigned, wantAuthnRequestSignedAttribute)));
                }
            }

            bool isEmpty = reader.IsEmptyElement;
            reader.ReadStartElement();
            if (!isEmpty)
            {
                while (reader.IsStartElement())
                {
                    if (reader.IsStartElement(Saml2MetadataConstants.Elements.SingleSignOnService, Saml2MetadataConstants.Namespace))
                    {
                        ProtocolEndpoint endpoint = ReadProtocolEndpoint(reader);

                        // Relaxed check for endpoint.ResponseLocation as per FIP 9935
                        idpssoDescriptor.SingleSignOnServices.Add(endpoint);
                    }
                    else if (reader.IsStartElement(Saml2Constants.Elements.Attribute, Saml2Constants.Namespace))
                    {
                        idpssoDescriptor.SupportedAttributes.Add(ReadAttribute(reader));
                    }
                    else if (ReadSingleSignOnDescriptorElement(reader, idpssoDescriptor))
                    {
                        // Do nothing
                    }
                    else if (ReadCustomElement<IdentityProviderSingleSignOnDescriptor>(reader, idpssoDescriptor))
                    {
                        // Do nothing.
                    }
                    else
                    {
                        reader.Skip();
                    }
                }
                reader.ReadEndElement();
            }

            // Relaxed check for( idpssoDescriptor.SingleSignOnServices.Count == 0 ) as per FIP 9935
            return idpssoDescriptor;
        }

        /// <summary>
        /// Reads an indexed endpoint.
        /// </summary>
        /// <param name="reader">The xml reader.</param>
        /// <returns>An indexed endpoint.</returns>
        /// <exception cref="ArgumentNullException">The parameter reader is null.</exception>
        protected virtual IndexedProtocolEndpoint ReadIndexedProtocolEndpoint(XmlReader reader)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            IndexedProtocolEndpoint endpoint = CreateIndexedProtocolEndpointInstance();

            string binding = reader.GetAttribute(Saml2MetadataConstants.Attributes.Binding, null);
            Uri bindingUri;
            if (!UriUtil.TryCreateValidUri(binding, UriKind.RelativeOrAbsolute, out bindingUri))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MetadataSerializationException(SR.GetString(SR.ID3202, Saml2MetadataConstants.Attributes.Binding, binding)));
            }
            endpoint.Binding = bindingUri;

            string location = reader.GetAttribute(Saml2MetadataConstants.Attributes.Location, null);
            Uri locationUri;
            if (!UriUtil.TryCreateValidUri(location, UriKind.RelativeOrAbsolute, out locationUri))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MetadataSerializationException(SR.GetString(SR.ID3202, Saml2MetadataConstants.Attributes.Location, location)));
            }
            endpoint.Location = locationUri;

            string indexStr = reader.GetAttribute(Saml2MetadataConstants.Attributes.EndpointIndex, null);
            int index;
            if (!Int32.TryParse(indexStr, out index))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MetadataSerializationException(SR.GetString(SR.ID3202, Saml2MetadataConstants.Attributes.EndpointIndex, indexStr)));
            }
            endpoint.Index = index;

            // responseLocation is optional
            string responseLocation = reader.GetAttribute(Saml2MetadataConstants.Attributes.ResponseLocation, null);
            if (!String.IsNullOrEmpty(responseLocation))
            {
                Uri responseUri;
                if (!UriUtil.TryCreateValidUri(responseLocation, UriKind.RelativeOrAbsolute, out responseUri))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MetadataSerializationException(SR.GetString(SR.ID3202, Saml2MetadataConstants.Attributes.ResponseLocation, responseLocation)));
                }
                endpoint.ResponseLocation = responseUri;
            }

            // isDefault is optional
            string isDefaultString = reader.GetAttribute(Saml2MetadataConstants.Attributes.EndpointIsDefault, null);
            if (!String.IsNullOrEmpty(isDefaultString))
            {
                try
                {
                    endpoint.IsDefault = XmlConvert.ToBoolean(isDefaultString.ToLowerInvariant());
                }
                catch (FormatException)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MetadataSerializationException(SR.GetString(SR.ID3202, Saml2MetadataConstants.Attributes.EndpointIsDefault, isDefaultString)));
                }
            }

            ReadCustomAttributes<IndexedProtocolEndpoint>(reader, endpoint);

            bool isEmpty = reader.IsEmptyElement;
            reader.ReadStartElement();
            if (!isEmpty)
            {
                while (reader.IsStartElement())
                {
                    if (ReadCustomElement<IndexedProtocolEndpoint>(reader, endpoint))
                    {
                        // Do nothing.
                    }
                    else
                    {
                        // Move on
                        reader.Skip();
                    }
                }
                reader.ReadEndElement();
            }

            // No elements to validate. Attributes are already validated
            return endpoint;
        }

        /// <summary>
        /// Reads a key descriptor.
        /// </summary>
        /// <param name="reader">Xml reader.</param>
        /// <returns>The key descriptor.</returns>
        /// <exception cref="ArgumentNullException">The parameter reader is null.</exception>
        protected virtual KeyDescriptor ReadKeyDescriptor(XmlReader reader)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }
            KeyDescriptor resultKey = CreateKeyDescriptorInstance();

            // Use is optional
            string use = reader.GetAttribute(Saml2MetadataConstants.Attributes.Use, null);
            if (!String.IsNullOrEmpty(use))
            {
                resultKey.Use = GetKeyDescriptorType(use);
            }

            ReadCustomAttributes<KeyDescriptor>(reader, resultKey);

            bool isEmpty = reader.IsEmptyElement;
            reader.ReadStartElement();
            if (!isEmpty)
            {
                while (reader.IsStartElement())
                {
                    if (reader.IsStartElement(XmlSignatureConstants.Elements.KeyInfo, XmlSignatureConstants.Namespace))
                    {
                        resultKey.KeyInfo = SecurityTokenSerializer.ReadKeyIdentifier(reader);
                    }
                    else if (reader.IsStartElement(Saml2MetadataConstants.Elements.EncryptionMethod, Saml2MetadataConstants.Namespace))
                    {
                        // Read the required algorithm attribute - relaxed as per FIP 9935
                        string algorithm = reader.GetAttribute(Saml2MetadataConstants.Attributes.Algorithm);
                        if (!String.IsNullOrEmpty(algorithm) && UriUtil.CanCreateValidUri(algorithm, UriKind.Absolute))
                        {
                            resultKey.EncryptionMethods.Add(new EncryptionMethod(new Uri(algorithm)));
                        }

                        isEmpty = reader.IsEmptyElement;
                        reader.ReadStartElement(Saml2MetadataConstants.Elements.EncryptionMethod, Saml2MetadataConstants.Namespace);
                        if (!isEmpty)
                        {
                            while (reader.IsStartElement())
                            {
                                if (ReadCustomElement<KeyDescriptor>(reader, resultKey))
                                {
                                    // Do nothing for now
                                }
                                else
                                {
                                    reader.Skip();
                                }
                            }
                            reader.ReadEndElement();
                        }
                    }
                    else if (ReadCustomElement<KeyDescriptor>(reader, resultKey))
                    {
                        // Do nothing.
                    }
                    else
                    {
                        reader.Skip();
                    }
                }
                reader.ReadEndElement();
            }

            if (resultKey.KeyInfo == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MetadataSerializationException(SR.GetString(SR.ID3200, XmlSignatureConstants.Elements.KeyInfo)));
            }
            return resultKey;
        }

        /// <summary>
        /// Reads a localized name.
        /// </summary>
        /// <param name="reader">The xml reader.</param>
        /// <returns>A localized name.</returns>
        /// <exception cref="ArgumentNullException">The parameter reader is null.</exception>
        protected virtual LocalizedName ReadLocalizedName(XmlReader reader)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            LocalizedName resultName = CreateLocalizedNameInstance();

            string lang = reader.GetAttribute(LanguageLocalName, LanguageNamespaceUri);
            try
            {
                resultName.Language = CultureInfo.GetCultureInfo(lang);
            }
            catch (ArgumentNullException)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MetadataSerializationException(SR.GetString(SR.ID3202, LanguageLocalName, "null")));
            }
            catch (ArgumentException)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MetadataSerializationException(SR.GetString(SR.ID3202, LanguageLocalName, lang)));
            }

            ReadCustomAttributes<LocalizedName>(reader, resultName);

            bool isEmpty = reader.IsEmptyElement;
            string elementName = reader.Name;
            reader.ReadStartElement();
            if (!isEmpty)
            {
                resultName.Name = reader.ReadContentAsString();
                while (reader.IsStartElement())
                {
                    if (ReadCustomElement<LocalizedName>(reader, resultName))
                    {
                        // Do nothing.
                    }
                    else
                    {
                        // Move on
                        reader.Skip();
                    }
                }
                reader.ReadEndElement();
            }

            if (String.IsNullOrEmpty(resultName.Name))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MetadataSerializationException(SR.GetString(SR.ID3200, elementName)));
            }
            return resultName;
        }

        /// <summary>
        /// Reads a localized uri.
        /// </summary>
        /// <param name="reader">The xml reader.</param>
        /// <returns>A localized uri.</returns>
        /// <exception cref="ArgumentNullException">The parameter reader is null.</exception>
        protected virtual LocalizedUri ReadLocalizedUri(XmlReader reader)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            LocalizedUri resultUri = CreateLocalizedUriInstance();

            string lang = reader.GetAttribute(LanguageLocalName, LanguageNamespaceUri);
            try
            {
                resultUri.Language = CultureInfo.GetCultureInfo(lang);
            }
            catch (ArgumentNullException)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MetadataSerializationException(SR.GetString(SR.ID3202, LanguageLocalName, "null")));
            }
            catch (ArgumentException)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MetadataSerializationException(SR.GetString(SR.ID3202, LanguageLocalName, lang)));
            }

            ReadCustomAttributes<LocalizedUri>(reader, resultUri);

            bool isEmpty = reader.IsEmptyElement;
            string elementName = reader.Name;
            reader.ReadStartElement();
            if (!isEmpty)
            {
                string uriContent = reader.ReadContentAsString();
                Uri uri;
                if (!UriUtil.TryCreateValidUri(uriContent, UriKind.RelativeOrAbsolute, out uri))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MetadataSerializationException(SR.GetString(SR.ID3202, elementName, uriContent)));
                }
                resultUri.Uri = uri;
                while (reader.IsStartElement())
                {
                    if (ReadCustomElement<LocalizedUri>(reader, resultUri))
                    {
                        // Do nothing.
                    }
                    else
                    {
                        reader.Skip();
                    }
                }
                reader.ReadEndElement();
            }

            if (resultUri.Uri == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MetadataSerializationException(SR.GetString(SR.ID3200, elementName)));
            }
            return resultUri;
        }

        /// <summary>
        /// Reads the given stream to deserialize a FederationMetadata instance.
        /// </summary>
        /// <param name="stream">Stream to be read.</param>
        /// <returns>An FederationMetadata instance.</returns>
        /// <exception cref="ArgumentNullException">The parameter stream is null.</exception>
        public MetadataBase ReadMetadata(Stream stream)
        {
            if (stream == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("stream");
            }
            XmlDictionaryReader reader = XmlDictionaryReader.CreateTextReader(stream, XmlDictionaryReaderQuotas.Max);
            return ReadMetadata(reader);
        }

        /// <summary>
        /// Read the given XmlReader to deserialize the FederationMetadata instance.
        /// </summary>
        /// <param name="reader">XmlReader to be read.</param>
        /// <returns>An FederationMetadata instance</returns>
        public MetadataBase ReadMetadata(XmlReader reader)
        {
            return ReadMetadata(reader, EmptySecurityTokenResolver.Instance);
        }

        /// <summary>
        /// Read the given XmlReader to deserialize the FederationMetadata instance.
        /// </summary>
        /// <param name="reader">XmlReader to be read.</param>
        /// <param name="tokenResolver">Token resolver to resolve the signature token.</param>
        /// <returns>An FederationMetadata instance.</returns>
        /// <exception cref="ArgumentNullException">The parameter reader is null.</exception>
        public MetadataBase ReadMetadata(XmlReader reader, SecurityTokenResolver tokenResolver)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }
            if (tokenResolver == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("tokenResolver");
            }
            if (false == (reader is XmlDictionaryReader))
            {
                reader = XmlDictionaryReader.CreateDictionaryReader(reader);
            }

            MetadataBase metadata = ReadMetadataCore(reader, tokenResolver);
            return metadata;
        }

        /// <summary>
        /// Reads metadata.
        /// </summary>
        /// <param name="reader">The xml reader.</param>
        /// <param name="tokenResolver">The security token resolver.</param>
        /// <returns>MetadataBase</returns>
        /// <exception cref="ArgumentNullException">The parameter reader or tokenReolver is null.</exception>
        protected virtual MetadataBase ReadMetadataCore(XmlReader reader, SecurityTokenResolver tokenResolver)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }
            if (tokenResolver == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("tokenResolver");
            }

            MetadataBase metadataBase;

            if (reader.IsStartElement(Saml2MetadataConstants.Elements.EntitiesDescriptor, Saml2MetadataConstants.Namespace))
            {
                metadataBase = ReadEntitiesDescriptor(reader, tokenResolver);
            }
            else if (reader.IsStartElement(Saml2MetadataConstants.Elements.EntityDescriptor, Saml2MetadataConstants.Namespace))
            {
                metadataBase = ReadEntityDescriptor(reader, tokenResolver);
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MetadataSerializationException(SR.GetString(SR.ID3260)));
            }

            return metadataBase;
        }

        /// <summary>
        /// Reads an organization.
        /// </summary>
        /// <param name="reader">The xml reader.</param>
        /// <returns>An organization.</returns>
        /// <exception cref="ArgumentNullException">The parameter reader is null.</exception>
        protected virtual Organization ReadOrganization(XmlReader reader)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }
            Organization resultOrg = CreateOrganizationInstance();

            ReadCustomAttributes<Organization>(reader, resultOrg);

            bool isEmpty = reader.IsEmptyElement;
            reader.ReadStartElement();
            if (!isEmpty)
            {
                while (reader.IsStartElement())
                {
                    if (reader.IsStartElement(Saml2MetadataConstants.Elements.OrganizationName, Saml2MetadataConstants.Namespace))
                    {
                        resultOrg.Names.Add(ReadLocalizedName(reader));
                    }
                    else if (reader.IsStartElement(Saml2MetadataConstants.Elements.OrganizationDisplayName, Saml2MetadataConstants.Namespace))
                    {
                        resultOrg.DisplayNames.Add(ReadLocalizedName(reader));
                    }
                    else if (reader.IsStartElement(Saml2MetadataConstants.Elements.OrganizationUrl, Saml2MetadataConstants.Namespace))
                    {
                        resultOrg.Urls.Add(ReadLocalizedUri(reader));
                    }
                    else if (ReadCustomElement<Organization>(reader, resultOrg))
                    {
                        // Do nothing.
                    }
                    else
                    {
                        reader.Skip();
                    }
                }
                reader.ReadEndElement();
            }

            // Relaxed as per FIP 9935
            // if ( resultOrg.DisplayNames.Count < 1 )
            // if ( resultOrg.Names.Count < 1 )
            // if ( resultOrg.Urls.Count < 1 )

            return resultOrg;
        }

        /// <summary>
        /// Reads an endpoint.
        /// </summary>
        /// <param name="reader">Xml reader.</param>
        /// <returns>An endpoint.</returns>
        /// <exception cref="ArgumentNullException">The parameter reader is null.</exception>
        protected virtual ProtocolEndpoint ReadProtocolEndpoint(XmlReader reader)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            ProtocolEndpoint endpoint = CreateProtocolEndpointInstance();

            string binding = reader.GetAttribute(Saml2MetadataConstants.Attributes.Binding, null);
            Uri bindingUri;
            if (!UriUtil.TryCreateValidUri(binding, UriKind.RelativeOrAbsolute, out bindingUri))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MetadataSerializationException(SR.GetString(SR.ID3202, Saml2MetadataConstants.Attributes.Binding, binding)));
            }
            endpoint.Binding = bindingUri;

            string location = reader.GetAttribute(Saml2MetadataConstants.Attributes.Location, null);
            Uri locationUri;
            if (!UriUtil.TryCreateValidUri(location, UriKind.RelativeOrAbsolute, out locationUri))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MetadataSerializationException(SR.GetString(SR.ID3202, Saml2MetadataConstants.Attributes.Location, location)));
            }
            endpoint.Location = locationUri;

            string responseLocation = reader.GetAttribute(Saml2MetadataConstants.Attributes.ResponseLocation, null);
            if (!String.IsNullOrEmpty(responseLocation))
            {
                Uri responseUri;
                if (!UriUtil.TryCreateValidUri(responseLocation, UriKind.RelativeOrAbsolute, out responseUri))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MetadataSerializationException(SR.GetString(SR.ID3202, Saml2MetadataConstants.Attributes.ResponseLocation, responseLocation)));
                }
                endpoint.ResponseLocation = responseUri;
            }

            ReadCustomAttributes<ProtocolEndpoint>(reader, endpoint);

            bool isEmpty = reader.IsEmptyElement;
            reader.ReadStartElement(); // <Endpoint>
            if (!isEmpty)
            {
                while (reader.IsStartElement())
                {
                    if (!ReadCustomElement<ProtocolEndpoint>(reader, endpoint))
                    {
                        // Move on
                        reader.Skip();
                    }
                }
                reader.ReadEndElement(); // </Endpoint>
            }

            return endpoint;
        }

        /// <summary>
        /// Reads role descriptor attributes.
        /// </summary>
        /// <param name="reader">The xml reader</param>
        /// <param name="roleDescriptor">The role descriptor.</param>
        /// <exception cref="ArgumentNullException">The parameter reader/roleDescriptor/roleDescriptor.ProtocolsSupported is null.</exception>
        protected virtual void ReadRoleDescriptorAttributes(XmlReader reader, RoleDescriptor roleDescriptor)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }
            if (roleDescriptor == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("roleDescriptor");
            }
            if (roleDescriptor.ProtocolsSupported == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("roleDescriptor.ProtocolsSupported");
            }

            // Optional
            string validUntilString = reader.GetAttribute(Saml2MetadataConstants.Attributes.ValidUntil, null);
            if (!String.IsNullOrEmpty(validUntilString))
            {
                DateTime validUntil;
                if (!DateTime.TryParse(validUntilString, out validUntil))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MetadataSerializationException(SR.GetString(SR.ID3202, Saml2MetadataConstants.Attributes.ValidUntil, validUntilString)));
                }
                roleDescriptor.ValidUntil = validUntil;
            }

            // Optional
            string errorUrlString = reader.GetAttribute(Saml2MetadataConstants.Attributes.ErrorUrl, null);
            if (!string.IsNullOrEmpty(errorUrlString))
            {
                Uri errorUrl;
                if (!UriUtil.TryCreateValidUri(errorUrlString, UriKind.RelativeOrAbsolute, out errorUrl))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MetadataSerializationException(SR.GetString(SR.ID3202, Saml2MetadataConstants.Attributes.ErrorUrl, errorUrlString)));
                }
                roleDescriptor.ErrorUrl = errorUrl;
            }

            // Mandatory
            string protocols = reader.GetAttribute(Saml2MetadataConstants.Attributes.ProtocolsSupported, null);
            if (String.IsNullOrEmpty(protocols))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MetadataSerializationException(SR.GetString(SR.ID3202, Saml2MetadataConstants.Attributes.ProtocolsSupported, protocols)));
            }
            foreach (string protocol in protocols.Split(' '))
            {
                string toAdd = protocol.Trim();
                if (!String.IsNullOrEmpty(toAdd))
                {
                    roleDescriptor.ProtocolsSupported.Add(new Uri(toAdd));
                }
            }
            ReadCustomAttributes<RoleDescriptor>(reader, roleDescriptor);
        }

        /// <summary>
        /// Reads role descriptor element.
        /// </summary>
        /// <param name="reader">The xml reader.</param>
        /// <param name="roleDescriptor">The role descriptor.</param>
        /// <returns>True if read.</returns>
        /// <exception cref="ArgumentNullException">The parameter reader/roleDescriptor/roleDescriptor.Contacts/rolDescriptor.Keys is null.</exception>
        protected virtual bool ReadRoleDescriptorElement(XmlReader reader, RoleDescriptor roleDescriptor)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }
            if (roleDescriptor == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("roleDescriptor");
            }
            if (roleDescriptor.Contacts == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("roleDescriptor.Contacts");
            }
            if (roleDescriptor.Keys == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("roleDescriptor.Keys");
            }

            if (reader.IsStartElement(Saml2MetadataConstants.Elements.Organization, Saml2MetadataConstants.Namespace))
            {
                roleDescriptor.Organization = ReadOrganization(reader);
                return true;
            }
            else if (reader.IsStartElement(Saml2MetadataConstants.Elements.KeyDescriptor, Saml2MetadataConstants.Namespace))
            {
                roleDescriptor.Keys.Add(ReadKeyDescriptor(reader));
                return true;
            }
            else if (reader.IsStartElement(Saml2MetadataConstants.Elements.ContactPerson, Saml2MetadataConstants.Namespace))
            {
                roleDescriptor.Contacts.Add(ReadContactPerson(reader));
                return true;
            }
            else
            {
                return ReadCustomElement<RoleDescriptor>(reader, roleDescriptor);
            }

            // No mandatory elements. No validations
        }

        /// <summary>
        /// Reads security token service descriptor.
        /// </summary>
        /// <param name="reader">The xml reader.</param>
        /// <returns>A security token service descriptor.</returns>
        /// <exception cref="ArgumentNullException">The parameter reader is null.</exception>
        protected virtual SecurityTokenServiceDescriptor ReadSecurityTokenServiceDescriptor(XmlReader reader)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            SecurityTokenServiceDescriptor securityTokenServiceDescriptor = CreateSecurityTokenServiceDescriptorInstance();
            ReadWebServiceDescriptorAttributes(reader, securityTokenServiceDescriptor);
            ReadCustomAttributes<SecurityTokenServiceDescriptor>(reader, securityTokenServiceDescriptor);

            bool isEmpty = reader.IsEmptyElement;
            reader.ReadStartElement();
            if (!isEmpty)
            {
                while (reader.IsStartElement())
                {
                    if (reader.IsStartElement(FederationMetadataConstants.Elements.SecurityTokenServiceEndpoint, FederationMetadataConstants.Namespace))
                    {
                        isEmpty = reader.IsEmptyElement;
                        reader.ReadStartElement();
                        if (!isEmpty && reader.IsStartElement())
                        {
                            EndpointReference address = EndpointReference.ReadFrom(reader);
                            securityTokenServiceDescriptor.SecurityTokenServiceEndpoints.Add(address);
                            reader.ReadEndElement();
                        }
                    }
                    else if (reader.IsStartElement(FederationMetadataConstants.Elements.PassiveRequestorEndpoint, FederationMetadataConstants.Namespace))
                    {
                        isEmpty = reader.IsEmptyElement;
                        reader.ReadStartElement();
                        if (!isEmpty && reader.IsStartElement())
                        {
                            EndpointReference address = EndpointReference.ReadFrom(reader);
                            securityTokenServiceDescriptor.PassiveRequestorEndpoints.Add(address);
                            reader.ReadEndElement();
                        }
                    }
                    else if (ReadWebServiceDescriptorElement(reader, securityTokenServiceDescriptor))
                    {
                        // Do nothing
                    }
                    else if (ReadCustomElement<SecurityTokenServiceDescriptor>(reader, securityTokenServiceDescriptor))
                    {
                        // Do nothing.
                    }
                    else
                    {
                        reader.Skip();
                    }
                }

                reader.ReadEndElement(); // SecurityTokenService
            }

            // Relaxed as per FIP 9935
            // if ( securityTokenServiceDescriptor.SecurityTokenServiceEndpoints.Count == 0 )

            return securityTokenServiceDescriptor;
        }

        /// <summary>
        /// Reads spsso descriptor.
        /// </summary>
        /// <param name="reader">The xml reader.</param>
        /// <returns>An spsso descriptor.</returns>
        /// <exception cref="ArgumentNullException">The parameter reader is null.</exception>
        /// <exception cref="MetadataSerializationException">The XML was invalid.</exception>
        protected virtual ServiceProviderSingleSignOnDescriptor ReadServiceProviderSingleSignOnDescriptor(XmlReader reader)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            ServiceProviderSingleSignOnDescriptor spssoDescriptor = CreateServiceProviderSingleSignOnDescriptorInstance();

            string authnRequestsSignedAttribute = reader.GetAttribute(Saml2MetadataConstants.Attributes.AuthenticationRequestsSigned);
            if (!String.IsNullOrEmpty(authnRequestsSignedAttribute))
            {
                try
                {
                    spssoDescriptor.AuthenticationRequestsSigned = XmlConvert.ToBoolean(authnRequestsSignedAttribute.ToLowerInvariant());
                }
                catch (FormatException)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MetadataSerializationException(SR.GetString(
                        SR.ID3202, Saml2MetadataConstants.Attributes.AuthenticationRequestsSigned, authnRequestsSignedAttribute)));
                }
            }

            string wantAssertionsSignedAttribute = reader.GetAttribute(Saml2MetadataConstants.Attributes.WantAssertionsSigned);
            if (!String.IsNullOrEmpty(wantAssertionsSignedAttribute))
            {
                try
                {
                    spssoDescriptor.WantAssertionsSigned = XmlConvert.ToBoolean(wantAssertionsSignedAttribute.ToLowerInvariant());
                }
                catch (FormatException)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MetadataSerializationException(SR.GetString(
                        SR.ID3202, Saml2MetadataConstants.Attributes.WantAssertionsSigned, wantAssertionsSignedAttribute)));
                }
            }

            ReadSingleSignOnDescriptorAttributes(reader, spssoDescriptor);
            ReadCustomAttributes<ServiceProviderSingleSignOnDescriptor>(reader, spssoDescriptor);

            bool isEmpty = reader.IsEmptyElement;
            reader.ReadStartElement();
            if (!isEmpty)
            {
                while (reader.IsStartElement())
                {
                    if (reader.IsStartElement(Saml2MetadataConstants.Elements.AssertionConsumerService, Saml2MetadataConstants.Namespace))
                    {
                        IndexedProtocolEndpoint endpoint = ReadIndexedProtocolEndpoint(reader);
                        spssoDescriptor.AssertionConsumerServices.Add(endpoint.Index, endpoint);
                    }
                    else if (ReadSingleSignOnDescriptorElement(reader, spssoDescriptor))
                    {
                        // Do nothing
                    }
                    else if (ReadCustomElement<ServiceProviderSingleSignOnDescriptor>(reader, spssoDescriptor))
                    {
                        // Do nothing.
                    }
                    else
                    {
                        reader.Skip();
                    }
                }

                reader.ReadEndElement(); // SPSSODescriptor
            }

            // Relaxed as per FIP 9935
            // if ( spssoDescriptor.AssertionConsumerService.Count == 0 )

            return spssoDescriptor;
        }

        /// <summary>
        /// Reads sso descriptor attributes.
        /// </summary>
        /// <param name="reader">The xml reader.</param>
        /// <param name="roleDescriptor">The sso role descriptor.</param>
        protected virtual void ReadSingleSignOnDescriptorAttributes(XmlReader reader, SingleSignOnDescriptor roleDescriptor)
        {
            ReadRoleDescriptorAttributes(reader, roleDescriptor);
            ReadCustomAttributes<SingleSignOnDescriptor>(reader, roleDescriptor);
        }

        /// <summary>
        /// Reads sso descriptor element.
        /// </summary>
        /// <param name="reader">The xml reader.</param>
        /// <param name="singleSignOnDescriptor">The sso descriptor.</param>
        /// <returns>True if read.</returns>
        protected virtual bool ReadSingleSignOnDescriptorElement(XmlReader reader, SingleSignOnDescriptor singleSignOnDescriptor)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }
            if (singleSignOnDescriptor == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("ssoDescriptor");
            }

            if (ReadRoleDescriptorElement(reader, singleSignOnDescriptor))
            {
                return true;
            }
            else if (reader.IsStartElement(Saml2MetadataConstants.Elements.ArtifactResolutionService, Saml2MetadataConstants.Namespace))
            {
                IndexedProtocolEndpoint endpoint = ReadIndexedProtocolEndpoint(reader);

                // Relaxed check for endpoint.ResponseLocation != null as per FIP 9935
                singleSignOnDescriptor.ArtifactResolutionServices.Add(endpoint.Index, endpoint);
                return true;
            }
            else if (reader.IsStartElement(Saml2MetadataConstants.Elements.SingleLogoutService, Saml2MetadataConstants.Namespace))
            {
                singleSignOnDescriptor.SingleLogoutServices.Add(ReadProtocolEndpoint(reader));
                return true;
            }
            else if (reader.IsStartElement(Saml2MetadataConstants.Elements.NameIDFormat, Saml2MetadataConstants.Namespace))
            {
                string nameId = reader.ReadElementContentAsString(Saml2MetadataConstants.Elements.NameIDFormat, Saml2MetadataConstants.Namespace);
                if (!UriUtil.CanCreateValidUri(nameId, UriKind.Absolute))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MetadataSerializationException(SR.GetString(SR.ID0014, Saml2MetadataConstants.Elements.NameIDFormat)));
                }
                singleSignOnDescriptor.NameIdentifierFormats.Add(new Uri(nameId));
                return true;
            }
            else
            {
                return ReadCustomElement<SingleSignOnDescriptor>(reader, singleSignOnDescriptor);
            }
        }

        /// <summary>
        /// Reads web service descriptor attributes.
        /// </summary>
        /// <param name="reader">The xml reader.</param>
        /// <param name="roleDescriptor">The web service descriptor.</param>
        /// <exception cref="ArgumentNullException">The parameter reader/roleDescriptor is null.</exception>
        protected virtual void ReadWebServiceDescriptorAttributes(XmlReader reader, WebServiceDescriptor roleDescriptor)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }
            if (roleDescriptor == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("roleDescriptor");
            }

            ReadRoleDescriptorAttributes(reader, roleDescriptor);
            string displayName = reader.GetAttribute(Saml2MetadataConstants.Attributes.ServiceDisplayName, null);
            if (!String.IsNullOrEmpty(displayName))
            {
                roleDescriptor.ServiceDisplayName = displayName;
            }
            string description = reader.GetAttribute(Saml2MetadataConstants.Attributes.ServiceDescription, null);
            if (!String.IsNullOrEmpty(description))
            {
                roleDescriptor.ServiceDescription = description;
            }
            ReadCustomAttributes<WebServiceDescriptor>(reader, roleDescriptor);

            // All optional no validations
        }

        /// <summary>
        /// Reads web service descriptor element.
        /// </summary>
        /// <param name="reader">The xml reader.</param>
        /// <param name="roleDescriptor">The web service descriptor.</param>
        /// <returns>True if read.</returns>
        /// <exception cref="ArgumentNullException">The parameter reader/roleDescriptor/roleDescriptor.TargetScopes/roleDescriptor.TargetScopes/roleDescriptor.TokenTypesOffered
        /// is null.</exception>
        public virtual bool ReadWebServiceDescriptorElement(XmlReader reader, WebServiceDescriptor roleDescriptor)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }
            if (roleDescriptor == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("roleDescriptor");
            }
            if (roleDescriptor.TargetScopes == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("roleDescriptor.TargetScopes");
            }
            if (roleDescriptor.ClaimTypesOffered == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("roleDescriptor.TargetScopes");
            }
            if (roleDescriptor.TokenTypesOffered == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("roleDescriptor.TokenTypesOffered");
            }

            if (ReadRoleDescriptorElement(reader, roleDescriptor))
            {
                return true;
            }
            else if (reader.IsStartElement(FederationMetadataConstants.Elements.TargetScopes, FederationMetadataConstants.Namespace))
            {
                bool isEmpty = reader.IsEmptyElement;
                reader.ReadStartElement();
                if (!isEmpty)
                {
                    while (reader.IsStartElement())
                    {
                        roleDescriptor.TargetScopes.Add(EndpointReference.ReadFrom(reader));
                    }

                    reader.ReadEndElement();
                }

                return true;
            }
            else if (reader.IsStartElement(FederationMetadataConstants.Elements.ClaimTypesOffered, FederationMetadataConstants.Namespace))
            {
                bool isEmpty = reader.IsEmptyElement;
                reader.ReadStartElement();
                if (!isEmpty)
                {
                    while (reader.IsStartElement())
                    {
                        if (reader.IsStartElement(WSAuthorizationConstants.Elements.ClaimType, WSAuthorizationConstants.Namespace))
                        {
                            roleDescriptor.ClaimTypesOffered.Add(ReadDisplayClaim(reader));
                        }
                        else
                        {
                            // Move on
                            reader.Skip();
                        }
                    }

                    reader.ReadEndElement();
                }

                return true;
            }
            else if (reader.IsStartElement(FederationMetadataConstants.Elements.ClaimTypesRequested, FederationMetadataConstants.Namespace))
            {
                bool isEmpty = reader.IsEmptyElement;
                reader.ReadStartElement();
                if (!isEmpty)
                {
                    while (reader.IsStartElement())
                    {
                        if (reader.IsStartElement(WSAuthorizationConstants.Elements.ClaimType, WSAuthorizationConstants.Namespace))
                        {
                            roleDescriptor.ClaimTypesRequested.Add(ReadDisplayClaim(reader));
                        }
                        else
                        {
                            // Move on
                            reader.Skip();
                        }
                    }

                    reader.ReadEndElement();
                }

                return true;
            }
            else if (reader.IsStartElement(FederationMetadataConstants.Elements.TokenTypesOffered, FederationMetadataConstants.Namespace))
            {
                bool isEmpty = reader.IsEmptyElement;
                reader.ReadStartElement(FederationMetadataConstants.Elements.TokenTypesOffered, FederationMetadataConstants.Namespace);

                if (!isEmpty)
                {
                    while (reader.IsStartElement())
                    {
                        if (reader.IsStartElement(WSFederationMetadataConstants.Elements.TokenType, WSFederationMetadataConstants.Namespace))
                        {
                            string tokenType = reader.GetAttribute(WSFederationMetadataConstants.Attributes.Uri, null);
                            Uri tokenTypeUri;
                            if (!UriUtil.TryCreateValidUri(tokenType, UriKind.Absolute, out tokenTypeUri))
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MetadataSerializationException(SR.GetString(SR.ID3202, WSFederationMetadataConstants.Elements.TokenType, tokenType)));
                            }

                            roleDescriptor.TokenTypesOffered.Add(tokenTypeUri);

                            isEmpty = reader.IsEmptyElement;
                            reader.ReadStartElement(); // TokenType
                            if (!isEmpty)
                            {
                                reader.ReadEndElement(); // TokenType
                            }
                        }
                        else
                        {
                            reader.Skip();
                        }
                    }

                    reader.ReadEndElement(); // TokenTypeOffered
                }
                return true;
            }
            else
            {
                return ReadCustomElement<WebServiceDescriptor>(reader, roleDescriptor);
            }

            // All optional. No Validations needed
        }

        /// <summary>
        /// Gets the SecurityTokenSerializer that this instance is using to serializer 
        /// SecurityTokens.
        /// </summary>
        public SecurityTokenSerializer SecurityTokenSerializer
        {
            get
            {
                return _tokenSerializer;
            }
        }

        /// <summary>
        /// Writes an application service descriptor.
        /// </summary>
        /// <param name="writer">The xml writer.</param>
        /// <param name="appService">The application service descriptor.</param>
        /// <exception cref="ArgumentNullException">The parameter writer/appService/appService.Endpoint/aappService.PassiveRequestorEndpoints is null.</exception>
        protected virtual void WriteApplicationServiceDescriptor(XmlWriter writer, ApplicationServiceDescriptor appService)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (appService == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("appService");
            }

            if (appService.Endpoints == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("appService.Endpoints");
            }

            if (appService.PassiveRequestorEndpoints == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("appService.PassiveRequestorEndpoints");
            }

            writer.WriteStartElement(Saml2MetadataConstants.Elements.RoleDescriptor, Saml2MetadataConstants.Namespace);
            writer.WriteAttributeString("xsi", "type", XmlSchema.InstanceNamespace, FederationMetadataConstants.Prefix + ":" + FederationMetadataConstants.Elements.ApplicationServiceType);

            writer.WriteAttributeString("xmlns", FederationMetadataConstants.Prefix, null, FederationMetadataConstants.Namespace);

            WriteWebServiceDescriptorAttributes(writer, appService);
            WriteCustomAttributes<ApplicationServiceDescriptor>(writer, appService);

            WriteWebServiceDescriptorElements(writer, appService);

            // Optional ApplicationServiceEndpoints
            foreach (EndpointReference epr in appService.Endpoints)
            {
                writer.WriteStartElement(FederationMetadataConstants.Elements.ApplicationServiceEndpoint, FederationMetadataConstants.Namespace);
                epr.WriteTo(writer);
                writer.WriteEndElement();
            }

            // Optional PassiveRequestorEndpoints
            foreach (EndpointReference epr in appService.PassiveRequestorEndpoints)
            {
                writer.WriteStartElement(FederationMetadataConstants.Elements.PassiveRequestorEndpoint, FederationMetadataConstants.Namespace);
                epr.WriteTo(writer);
                writer.WriteEndElement();
            }

            WriteCustomElements<ApplicationServiceDescriptor>(writer, appService);

            writer.WriteEndElement();
        }

        /// <summary>
        /// Writes a contact person.
        /// </summary>
        /// <param name="writer">The xml writer.</param>
        /// <param name="contactPerson">The contact person.</param>
        /// <exception cref="ArgumentNullException">The parameter writer/contactPerson/contactPerson.EmaillAddresses/contactPerson.TelephoneNumbers is null.</exception>
        protected virtual void WriteContactPerson(XmlWriter writer, ContactPerson contactPerson)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (contactPerson == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contactPerson");
            }

            if (contactPerson.EmailAddresses == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contactPerson.EmailAddresses");
            }

            if (contactPerson.TelephoneNumbers == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contactPerson.TelephoneNumbers");
            }

            writer.WriteStartElement(Saml2MetadataConstants.Elements.ContactPerson, Saml2MetadataConstants.Namespace);
            if (contactPerson.Type == ContactType.Unspecified)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MetadataSerializationException(SR.GetString(SR.ID3203, Saml2MetadataConstants.Attributes.ContactType)));
            }

            writer.WriteAttributeString(Saml2MetadataConstants.Attributes.ContactType, null, contactPerson.Type.ToString().ToLowerInvariant());

            WriteCustomAttributes<ContactPerson>(writer, contactPerson);

            if (!String.IsNullOrEmpty(contactPerson.Company))
            {
                writer.WriteElementString(Saml2MetadataConstants.Elements.Company, Saml2MetadataConstants.Namespace, contactPerson.Company);
            }

            if (!String.IsNullOrEmpty(contactPerson.GivenName))
            {
                writer.WriteElementString(Saml2MetadataConstants.Elements.GivenName, Saml2MetadataConstants.Namespace, contactPerson.GivenName);
            }

            if (!String.IsNullOrEmpty(contactPerson.Surname))
            {
                writer.WriteElementString(Saml2MetadataConstants.Elements.Surname, Saml2MetadataConstants.Namespace, contactPerson.Surname);
            }

            foreach (string email in contactPerson.EmailAddresses)
            {
                writer.WriteElementString(Saml2MetadataConstants.Elements.EmailAddress, Saml2MetadataConstants.Namespace, email);
            }

            foreach (string phone in contactPerson.TelephoneNumbers)
            {
                writer.WriteElementString(Saml2MetadataConstants.Elements.TelephoneNumber, Saml2MetadataConstants.Namespace, phone);
            }

            WriteCustomElements<ContactPerson>(writer, contactPerson);

            writer.WriteEndElement();
        }

        /// <summary>
        /// Extensible point to write custom attributes.
        /// </summary>
        /// <typeparam name="T">The type of the element whose attribute is being written</typeparam>
        /// <param name="writer">The xml writer.</param>
        /// <param name="source">The source element of type T.</param>
        protected virtual void WriteCustomAttributes<T>(XmlWriter writer, T source)
        {
            // Extensibility point only. Do Nothing.
        }

        /// <summary>
        /// Extensible point to write custom elements.
        /// </summary>
        /// <typeparam name="T">The type of element being written.</typeparam>
        /// <param name="writer">The xml writer.</param>
        /// <param name="source">The source element of type T.</param>
        protected virtual void WriteCustomElements<T>(XmlWriter writer, T source)
        {
            // Extensibility point only. Do Nothing.
        }

        /// <summary>
        /// Writes an endpoint.
        /// </summary>
        /// <param name="writer">The xml writer.</param>
        /// <param name="endpoint">The endpoint.</param>
        /// <param name="element">The xml qualified name element.</param>
        /// <exception cref="ArgumentNullException">The parameter writer/endpoint/element is null.</exception>
        protected virtual void WriteProtocolEndpoint(XmlWriter writer, ProtocolEndpoint endpoint, XmlQualifiedName element)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (endpoint == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpoint");
            }

            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }

            writer.WriteStartElement(element.Name, element.Namespace);
            if (endpoint.Binding == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MetadataSerializationException(SR.GetString(SR.ID3203, Saml2MetadataConstants.Attributes.Binding)));
            }

            writer.WriteAttributeString(Saml2MetadataConstants.Attributes.Binding, null, (endpoint.Binding.IsAbsoluteUri ? endpoint.Binding.AbsoluteUri : endpoint.Binding.ToString()));

            if (endpoint.Location == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MetadataSerializationException(SR.GetString(SR.ID3203, Saml2MetadataConstants.Attributes.Location)));
            }

            writer.WriteAttributeString(Saml2MetadataConstants.Attributes.Location, null, (endpoint.Location.IsAbsoluteUri ? endpoint.Location.AbsoluteUri : endpoint.Location.ToString()));

            if (endpoint.ResponseLocation != null)
            {
                writer.WriteAttributeString(Saml2MetadataConstants.Attributes.ResponseLocation, null, (endpoint.ResponseLocation.IsAbsoluteUri ? endpoint.ResponseLocation.AbsoluteUri : endpoint.ResponseLocation.ToString()));
            }

            WriteCustomAttributes<ProtocolEndpoint>(writer, endpoint);

            WriteCustomElements<ProtocolEndpoint>(writer, endpoint);
            writer.WriteEndElement();
        }

        /// <summary>
        /// Writes entities descriptor.
        /// </summary>
        /// <param name="writer">The <see cref="XmlWriter"/> to use.</param>
        /// <param name="claim">The <see cref="DisplayClaim"/> to write.</param>
        protected virtual void WriteDisplayClaim(XmlWriter writer, DisplayClaim claim)
        {
            // This is not extensible since it is defined in a different spec.
            writer.WriteStartElement(WSAuthorizationConstants.Prefix, WSAuthorizationConstants.Elements.ClaimType, WSAuthorizationConstants.Namespace);

            // ClaimType is mandatory
            if (String.IsNullOrEmpty(claim.ClaimType))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MetadataSerializationException(SR.GetString(SR.ID3203, WSAuthorizationConstants.Elements.ClaimType)));
            }

            if (!UriUtil.CanCreateValidUri(claim.ClaimType, UriKind.Absolute))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MetadataSerializationException(SR.GetString(SR.ID0014, claim.ClaimType)));
            }

            writer.WriteAttributeString(WSFederationMetadataConstants.Attributes.Uri, claim.ClaimType);

            if (claim.WriteOptionalAttribute)
            {
                writer.WriteAttributeString(WSFederationMetadataConstants.Attributes.Optional, XmlConvert.ToString(claim.Optional));
            }

            if (!String.IsNullOrEmpty(claim.DisplayTag))
            {
                writer.WriteElementString(WSAuthorizationConstants.Prefix, WSAuthorizationConstants.Elements.DisplayName, WSAuthorizationConstants.Namespace, claim.DisplayTag);
            }

            if (!String.IsNullOrEmpty(claim.Description))
            {
                writer.WriteElementString(WSAuthorizationConstants.Prefix, WSAuthorizationConstants.Elements.Description, WSAuthorizationConstants.Namespace, claim.Description);
            }

            writer.WriteEndElement(); // ClaimType
        }

        /// <summary>
        /// Writes entities descriptor.
        /// </summary>
        /// <param name="inputWriter">The <see cref="XmlWriter"/> to use.</param>
        /// <param name="entitiesDescriptor">The entities descriptor.</param>
        /// <exception cref="ArgumentNullException">The parameter inputWriter/entitiesDescriptor/entitiesDescriptor.ChildEntities/entitiesDescriptor.ChildEntityGroups
        /// is null.</exception>
        protected virtual void WriteEntitiesDescriptor(XmlWriter inputWriter, EntitiesDescriptor entitiesDescriptor)
        {
            if (inputWriter == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("inputWriter");
            }

            if (entitiesDescriptor == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("entitiesDescriptor");
            }

            if (entitiesDescriptor.ChildEntities == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("entitiesDescriptor.ChildEntities");
            }

            if (entitiesDescriptor.ChildEntityGroups == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("entitiesDescriptor.ChildEntityGroups");
            }

            string entityReference = "_" + Guid.NewGuid().ToString();
            XmlWriter writer = inputWriter;
            EnvelopedSignatureWriter signedWriter = null;
            if (entitiesDescriptor.SigningCredentials != null)
            {
                signedWriter = new EnvelopedSignatureWriter(inputWriter, entitiesDescriptor.SigningCredentials, entityReference, SecurityTokenSerializer);
                writer = signedWriter;
            }

            writer.WriteStartElement(Saml2MetadataConstants.Elements.EntitiesDescriptor, Saml2MetadataConstants.Namespace);
            writer.WriteAttributeString(Saml2MetadataConstants.Attributes.Id, null, entityReference);

            if (entitiesDescriptor.ChildEntities.Count == 0 && entitiesDescriptor.ChildEntityGroups.Count == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MetadataSerializationException(SR.GetString(SR.ID3203, Saml2MetadataConstants.Elements.EntitiesDescriptor)));
            }

            // Ensure FederationID in all children are valid.
            foreach (EntityDescriptor entity in entitiesDescriptor.ChildEntities)
            {
                if (!String.IsNullOrEmpty(entity.FederationId))
                {
                    if (!StringComparer.Ordinal.Equals(entity.FederationId, entitiesDescriptor.Name))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MetadataSerializationException(SR.GetString(SR.ID3203, WSFederationMetadataConstants.Attributes.FederationId)));
                    }
                }
            }

            if (!String.IsNullOrEmpty(entitiesDescriptor.Name))
            {
                writer.WriteAttributeString(Saml2MetadataConstants.Attributes.EntityGroupName, null, entitiesDescriptor.Name);
            }

            WriteCustomAttributes<EntitiesDescriptor>(writer, entitiesDescriptor);

            // WriteSamlMetadataBaseElements?

            if (null != signedWriter)
            {
                // Write the signature at the top of the sequence
                signedWriter.WriteSignature();
            }

            foreach (EntityDescriptor entity in entitiesDescriptor.ChildEntities)
            {
                WriteEntityDescriptor(writer, entity);
            }

            foreach (EntitiesDescriptor entityGroup in entitiesDescriptor.ChildEntityGroups)
            {
                WriteEntitiesDescriptor(writer, entityGroup);
            }

            WriteCustomElements<EntitiesDescriptor>(writer, entitiesDescriptor);

            writer.WriteEndElement(); // EntitiesDescriptor
        }

        /// <summary>
        /// Writes an entity descriptor.
        /// </summary>
        /// <param name="inputWriter">The xml writer.</param>
        /// <param name="entityDescriptor">The entity descriptor.</param>
        /// <exception cref="ArgumentNullException">The parameter inputWriter/entityDescriptor/entityDescriptor.Contacts/entityDescriptor.RoleDescriptors is null.</exception>
        protected virtual void WriteEntityDescriptor(XmlWriter inputWriter, EntityDescriptor entityDescriptor)
        {
            if (inputWriter == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("inputWriter");
            }

            if (entityDescriptor == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("entityDescriptor");
            }

            if (entityDescriptor.Contacts == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("entityDescriptor.Contacts");
            }

            if (entityDescriptor.RoleDescriptors == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("entityDescriptor.RoleDescriptors");
            }

            string entityReference = "_" + Guid.NewGuid().ToString();
            XmlWriter writer = inputWriter;
            EnvelopedSignatureWriter signedWriter = null;
            if (entityDescriptor.SigningCredentials != null)
            {
                signedWriter = new EnvelopedSignatureWriter(inputWriter, entityDescriptor.SigningCredentials, entityReference, SecurityTokenSerializer);
                writer = signedWriter;
            }

            writer.WriteStartElement(Saml2MetadataConstants.Elements.EntityDescriptor, Saml2MetadataConstants.Namespace);
            writer.WriteAttributeString(Saml2MetadataConstants.Attributes.Id, null, entityReference);

            if (entityDescriptor.EntityId == null || entityDescriptor.EntityId.Id == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MetadataSerializationException(SR.GetString(SR.ID3203, Saml2MetadataConstants.Attributes.EntityId)));
            }

            writer.WriteAttributeString(Saml2MetadataConstants.Attributes.EntityId, null, entityDescriptor.EntityId.Id);

            if (!String.IsNullOrEmpty(entityDescriptor.FederationId))
            {
                writer.WriteAttributeString(WSFederationMetadataConstants.Attributes.FederationId, WSFederationMetadataConstants.Namespace, entityDescriptor.FederationId);
            }

            WriteCustomAttributes<EntityDescriptor>(writer, entityDescriptor);

            if (null != signedWriter)
            {
                // Write the signature at the top of the sequence
                signedWriter.WriteSignature();
            }

            if (entityDescriptor.RoleDescriptors.Count == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MetadataSerializationException(SR.GetString(SR.ID3203, Saml2MetadataConstants.Elements.RoleDescriptor)));
            }

            foreach (RoleDescriptor roleDescriptor in entityDescriptor.RoleDescriptors)
            {
                ServiceProviderSingleSignOnDescriptor spDesc = roleDescriptor as ServiceProviderSingleSignOnDescriptor;
                if (spDesc != null)
                {
                    WriteServiceProviderSingleSignOnDescriptor(writer, spDesc);
                }

                IdentityProviderSingleSignOnDescriptor idpDesc = roleDescriptor as IdentityProviderSingleSignOnDescriptor;
                if (idpDesc != null)
                {
                    WriteIdentityProviderSingleSignOnDescriptor(writer, idpDesc);
                }

                ApplicationServiceDescriptor appService = roleDescriptor as ApplicationServiceDescriptor;
                if (appService != null)
                {
                    WriteApplicationServiceDescriptor(writer, appService);
                }

                SecurityTokenServiceDescriptor stsService = roleDescriptor as SecurityTokenServiceDescriptor;
                if (stsService != null)
                {
                    WriteSecurityTokenServiceDescriptor(writer, stsService);
                }
            }

            if (entityDescriptor.Organization != null)
            {
                WriteOrganization(writer, entityDescriptor.Organization);
            }

            foreach (ContactPerson person in entityDescriptor.Contacts)
            {
                WriteContactPerson(writer, person);
            }

            WriteCustomElements<EntityDescriptor>(writer, entityDescriptor);

            writer.WriteEndElement(); // EntityDescriptor
        }

        /// <summary>
        /// Writes an idpsso descriptor.
        /// </summary>
        /// <param name="writer">The xml writer.</param>
        /// <param name="identityProviderSingleSignOnDescriptor">The idpsso descriptor.</param>
        /// <exception cref="ArgumentNullException">The parameter writer/idpssoDescriptor/idpssoDescriptor.SupportedAttributes/idpssoDescriptor.SingleSignOnServices
        /// is null.</exception>
        protected virtual void WriteIdentityProviderSingleSignOnDescriptor(XmlWriter writer, IdentityProviderSingleSignOnDescriptor identityProviderSingleSignOnDescriptor)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (identityProviderSingleSignOnDescriptor == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("idpssoDescriptor");
            }

            if (identityProviderSingleSignOnDescriptor.SupportedAttributes == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("idpssoDescriptor.SupportedAttributes");
            }

            if (identityProviderSingleSignOnDescriptor.SingleSignOnServices == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("idpssoDescriptor.SingleSignOnServices");
            }

            writer.WriteStartElement(Saml2MetadataConstants.Elements.IdpssoDescriptor, Saml2MetadataConstants.Namespace);
            if (identityProviderSingleSignOnDescriptor.WantAuthenticationRequestsSigned)
            {
                writer.WriteAttributeString(Saml2MetadataConstants.Attributes.WantAuthenticationRequestsSigned, null,
                    XmlConvert.ToString(identityProviderSingleSignOnDescriptor.WantAuthenticationRequestsSigned));
            }

            WriteSingleSignOnDescriptorAttributes(writer, identityProviderSingleSignOnDescriptor);
            WriteCustomAttributes<IdentityProviderSingleSignOnDescriptor>(writer, identityProviderSingleSignOnDescriptor);

            WriteSingleSignOnDescriptorElements(writer, identityProviderSingleSignOnDescriptor);

            // Mandatory SingleSignonServiceEndpoint
            if (identityProviderSingleSignOnDescriptor.SingleSignOnServices.Count == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MetadataSerializationException(SR.GetString(SR.ID3203, Saml2MetadataConstants.Elements.SingleSignOnService)));
            }

            foreach (ProtocolEndpoint endpoint in identityProviderSingleSignOnDescriptor.SingleSignOnServices)
            {
                if (endpoint.ResponseLocation != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MetadataSerializationException(SR.GetString(SR.ID3249, Saml2MetadataConstants.Attributes.ResponseLocation)));
                }

                XmlQualifiedName element = new XmlQualifiedName(Saml2MetadataConstants.Elements.SingleSignOnService, Saml2MetadataConstants.Namespace);
                WriteProtocolEndpoint(writer, endpoint, element);
            }

            // Optional SupportedAttributes
            foreach (Saml2Attribute attribute in identityProviderSingleSignOnDescriptor.SupportedAttributes)
            {
                WriteAttribute(writer, attribute);
            }

            WriteCustomElements<IdentityProviderSingleSignOnDescriptor>(writer, identityProviderSingleSignOnDescriptor);

            writer.WriteEndElement();
        }

        /// <summary>
        /// Writes an indexed endpoint.
        /// </summary>
        /// <param name="writer">The xml writer.</param>
        /// <param name="indexedEP">The indexed endpoint.</param>
        /// <param name="element">The xml qualified element.</param>
        /// <exception cref="ArgumentNullException">The parameter writer/indexedEP/element is null.</exception>
        protected virtual void WriteIndexedProtocolEndpoint(XmlWriter writer, IndexedProtocolEndpoint indexedEP, XmlQualifiedName element)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (indexedEP == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("indexedEP");
            }

            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }

            writer.WriteStartElement(element.Name, element.Namespace);
            if (indexedEP.Binding == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MetadataSerializationException(SR.GetString(SR.ID3203, Saml2MetadataConstants.Attributes.Binding)));
            }

            writer.WriteAttributeString(Saml2MetadataConstants.Attributes.Binding, null, (indexedEP.Binding.IsAbsoluteUri ? indexedEP.Binding.AbsoluteUri : indexedEP.Binding.ToString()));

            if (indexedEP.Location == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MetadataSerializationException(SR.GetString(SR.ID3203, Saml2MetadataConstants.Attributes.Location)));
            }

            writer.WriteAttributeString(Saml2MetadataConstants.Attributes.Location, null, (indexedEP.Location.IsAbsoluteUri ? indexedEP.Location.AbsoluteUri : indexedEP.Location.ToString()));

            if (indexedEP.Index < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MetadataSerializationException(SR.GetString(SR.ID3203, Saml2MetadataConstants.Attributes.EndpointIndex)));
            }

            writer.WriteAttributeString(Saml2MetadataConstants.Attributes.EndpointIndex, null, indexedEP.Index.ToString(CultureInfo.InvariantCulture));

            if (indexedEP.ResponseLocation != null)
            {
                writer.WriteAttributeString(Saml2MetadataConstants.Attributes.ResponseLocation, null, (indexedEP.ResponseLocation.IsAbsoluteUri ? indexedEP.ResponseLocation.AbsoluteUri : indexedEP.ResponseLocation.ToString()));
            }

            if (indexedEP.IsDefault.HasValue)
            {
                writer.WriteAttributeString(Saml2MetadataConstants.Attributes.EndpointIsDefault, null, XmlConvert.ToString(indexedEP.IsDefault.Value));
            }

            WriteCustomAttributes<IndexedProtocolEndpoint>(writer, indexedEP);
            WriteCustomElements<IndexedProtocolEndpoint>(writer, indexedEP);
            writer.WriteEndElement();
        }

        /// <summary>
        /// Writes a key descriptor.
        /// </summary>
        /// <param name="writer">The xml writer.</param>
        /// <param name="keyDescriptor">The key descriptor.</param>
        /// <exception cref="ArgumentNullException">The parameter writer/keyDescriptor is null.</exception>
        protected virtual void WriteKeyDescriptor(XmlWriter writer, KeyDescriptor keyDescriptor)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (keyDescriptor == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("keyDescriptor");
            }

            writer.WriteStartElement(Saml2MetadataConstants.Elements.KeyDescriptor, Saml2MetadataConstants.Namespace);
            if (keyDescriptor.Use == KeyType.Encryption || keyDescriptor.Use == KeyType.Signing)
            {
                writer.WriteAttributeString(Saml2MetadataConstants.Attributes.Use, null, keyDescriptor.Use.ToString().ToLowerInvariant());
            }

            WriteCustomAttributes<KeyDescriptor>(writer, keyDescriptor);

            if (keyDescriptor.KeyInfo == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MetadataSerializationException(SR.GetString(SR.ID3203, XmlSignatureConstants.Elements.KeyInfo)));
            }

            SecurityTokenSerializer.WriteKeyIdentifier(writer, keyDescriptor.KeyInfo);

            // Write the encryption method element.
            if (keyDescriptor.EncryptionMethods != null && keyDescriptor.EncryptionMethods.Count > 0)
            {
                foreach (EncryptionMethod encryptionMethod in keyDescriptor.EncryptionMethods)
                {
                    if (encryptionMethod.Algorithm == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MetadataSerializationException(SR.GetString(SR.ID3203, Saml2MetadataConstants.Attributes.Algorithm)));
                    }

                    if (!encryptionMethod.Algorithm.IsAbsoluteUri)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MetadataSerializationException(SR.GetString(SR.ID0014, Saml2MetadataConstants.Attributes.Algorithm)));
                    }

                    writer.WriteStartElement(Saml2MetadataConstants.Elements.EncryptionMethod, Saml2MetadataConstants.Namespace);
                    writer.WriteAttributeString(Saml2MetadataConstants.Attributes.Algorithm, null, encryptionMethod.Algorithm.AbsoluteUri);
                    writer.WriteEndElement();
                }
            }

            WriteCustomElements<KeyDescriptor>(writer, keyDescriptor);
            writer.WriteEndElement();
        }

        /// <summary>
        /// Writes a localized name.
        /// </summary>
        /// <param name="writer">The xml writer.</param>
        /// <param name="name">The localized name.</param>
        /// <param name="element">The xml qualified name.</param>
        /// <exception cref="ArgumentNullException">The parameter writer/name/element/name.Name is null.</exception>
        protected virtual void WriteLocalizedName(XmlWriter writer, LocalizedName name, XmlQualifiedName element)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (name == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("name");
            }

            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }

            if (name.Name == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("name.Name");
            }

            writer.WriteStartElement(element.Name, element.Namespace);
            if (name.Language == null || String.IsNullOrEmpty(name.Name))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MetadataSerializationException(SR.GetString(SR.ID3203, LanguageLocalName)));
            }

            writer.WriteAttributeString(LanguagePrefix, LanguageLocalName, LanguageNamespaceUri, name.Language.Name);
            WriteCustomAttributes<LocalizedName>(writer, name);
            writer.WriteString(name.Name);
            WriteCustomElements<LocalizedName>(writer, name);
            writer.WriteEndElement();
        }

        /// <summary>
        /// Writes localized uri
        /// </summary>
        /// <param name="writer">The xml writer.</param>
        /// <param name="uri">The localized uri.</param>
        /// <param name="element">The xml qualified name.</param>
        /// <exception cref="ArgumentNullException">The parameter writer/uri/element is null.</exception>
        protected virtual void WriteLocalizedUri(XmlWriter writer, LocalizedUri uri, XmlQualifiedName element)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (uri == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("uri");
            }

            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }

            writer.WriteStartElement(element.Name, element.Namespace);
            if (uri.Language == null || uri.Uri == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MetadataSerializationException(SR.GetString(SR.ID3203, LanguageLocalName)));
            }

            writer.WriteAttributeString(LanguagePrefix, LanguageLocalName, LanguageNamespaceUri, uri.Language.Name);
            WriteCustomAttributes<LocalizedUri>(writer, uri);
            writer.WriteString(uri.Uri.IsAbsoluteUri ? uri.Uri.AbsoluteUri : uri.Uri.ToString());
            WriteCustomElements<LocalizedUri>(writer, uri);
            writer.WriteEndElement();
        }

        /// <summary>
        /// Writes the federation metadata to the given stream.
        /// </summary>
        /// <param name="stream">Stream to write the Federation Metadata.</param>
        /// <param name="metadata">Metadata to write.</param>
        /// <exception cref="ArgumentNullException">The input argument is null.</exception>
        public void WriteMetadata(Stream stream, MetadataBase metadata)
        {
            if (stream == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("stream");
            }

            if (metadata == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("metadata");
            }

            using (XmlDictionaryWriter writer = XmlDictionaryWriter.CreateTextWriter(stream, Encoding.UTF8, false))
            {
                WriteMetadata(writer, metadata);
            }
        }

        /// <summary>
        /// Writes the federation metadata to the given writer.
        /// </summary>
        /// <param name="writer">Writer to which to write the federation Metadata</param>
        /// <param name="metadata">Metadata to write.</param>
        /// <exception cref="ArgumentNullException">The input argument is null.</exception>
        public void WriteMetadata(XmlWriter writer, MetadataBase metadata)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (metadata == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("metadata");
            }

            WriteMetadataCore(writer, metadata);
        }

        /// <summary>
        /// Writes the metadata.
        /// </summary>
        /// <param name="writer">The xml writer.</param>
        /// <param name="metadataBase">The saml metadat base.</param>
        /// <exception cref="ArgumentNullException">The parameter writer/metadataBase is null.</exception>
        protected virtual void WriteMetadataCore(XmlWriter writer, MetadataBase metadataBase)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (metadataBase == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("metadataBase");
            }

            EntitiesDescriptor entitiesDescriptor = metadataBase as EntitiesDescriptor;
            if (entitiesDescriptor != null)
            {
                WriteEntitiesDescriptor(writer, entitiesDescriptor);
            }
            else
            {
                EntityDescriptor entityDescriptor = metadataBase as EntityDescriptor;
                if (entityDescriptor == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MetadataSerializationException(SR.GetString(SR.ID3203, Saml2MetadataConstants.Elements.EntitiesDescriptor)));
                }

                WriteEntityDescriptor(writer, entityDescriptor);
            }
        }

        /// <summary>
        /// Writes an organization.
        /// </summary>
        /// <param name="writer">The xml writer.</param>
        /// <param name="organization">The organization.</param>
        /// <exception cref="ArgumentNullException">The parameter writer/organization is null.</exception>
        protected virtual void WriteOrganization(XmlWriter writer, Organization organization)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (organization == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("organization");
            }

            if (organization.DisplayNames == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("organization.DisplayNames");
            }

            if (organization.Names == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("organization.Names");
            }

            if (organization.Urls == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("organization.Urls");
            }

            writer.WriteStartElement(Saml2MetadataConstants.Elements.Organization, Saml2MetadataConstants.Namespace);

            if (organization.Names.Count < 1)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MetadataSerializationException(SR.GetString(SR.ID3203, Saml2MetadataConstants.Elements.OrganizationName)));
            }

            foreach (LocalizedName name in organization.Names)
            {
                XmlQualifiedName element = new XmlQualifiedName(Saml2MetadataConstants.Elements.OrganizationName, Saml2MetadataConstants.Namespace);
                WriteLocalizedName(writer, name, element);
            }

            if (organization.DisplayNames.Count < 1)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MetadataSerializationException(SR.GetString(SR.ID3203, Saml2MetadataConstants.Elements.OrganizationDisplayName)));
            }

            foreach (LocalizedName displayName in organization.DisplayNames)
            {
                XmlQualifiedName element = new XmlQualifiedName(Saml2MetadataConstants.Elements.OrganizationDisplayName, Saml2MetadataConstants.Namespace);
                WriteLocalizedName(writer, displayName, element);
            }

            if (organization.Urls.Count < 1)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MetadataSerializationException(SR.GetString(SR.ID3203, Saml2MetadataConstants.Elements.OrganizationUrl)));
            }

            foreach (LocalizedUri uri in organization.Urls)
            {
                XmlQualifiedName element = new XmlQualifiedName(Saml2MetadataConstants.Elements.OrganizationUrl, Saml2MetadataConstants.Namespace);
                WriteLocalizedUri(writer, uri, element);
            }

            WriteCustomAttributes<Organization>(writer, organization);
            WriteCustomElements<Organization>(writer, organization);
            writer.WriteEndElement(); // Organization
        }

        /// <summary>
        /// Writes role descriptor attibutes.
        /// </summary>
        /// <param name="writer">The xml writer.</param>
        /// <param name="roleDescriptor">The role descriptor.</param>
        /// <exception cref="ArgumentNullException">The parameter writer/roleDescriptor/roleDescriptor.ProtocolsSupporeted is null.</exception>
        protected virtual void WriteRoleDescriptorAttributes(XmlWriter writer, RoleDescriptor roleDescriptor)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (roleDescriptor == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("roleDescriptor");
            }

            if (roleDescriptor.ProtocolsSupported == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("roleDescriptor.ProtocolsSupported");
            }

            // Optional
            if (roleDescriptor.ValidUntil != null && roleDescriptor.ValidUntil != DateTime.MaxValue)
            {
                // Write the date in a sortable form.
                writer.WriteAttributeString(Saml2MetadataConstants.Attributes.ValidUntil, null, roleDescriptor.ValidUntil.ToString("s", CultureInfo.InvariantCulture));
            }

            // Optional
            if (roleDescriptor.ErrorUrl != null)
            {
                writer.WriteAttributeString(Saml2MetadataConstants.Attributes.ErrorUrl, null, (roleDescriptor.ErrorUrl.IsAbsoluteUri ? roleDescriptor.ErrorUrl.AbsoluteUri : roleDescriptor.ErrorUrl.ToString()));
            }

            // Mandatory
            if (roleDescriptor.ProtocolsSupported.Count == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MetadataSerializationException(SR.GetString(SR.ID3203, Saml2MetadataConstants.Attributes.ProtocolsSupported)));
            }

            StringBuilder sb = new StringBuilder();
            foreach (Uri protocol in roleDescriptor.ProtocolsSupported)
            {
                sb.AppendFormat("{0} ", (protocol.IsAbsoluteUri ? protocol.AbsoluteUri : protocol.ToString()));
            }

            string protocolsString = sb.ToString();
            writer.WriteAttributeString(Saml2MetadataConstants.Attributes.ProtocolsSupported, null, protocolsString.Trim());

            WriteCustomAttributes<RoleDescriptor>(writer, roleDescriptor);
        }

        /// <summary>
        /// Writes the role descriptor element.
        /// </summary>
        /// <param name="writer">The xml writer.</param>
        /// <param name="roleDescriptor">The role descriptor.</param>
        /// <exception cref="ArgumentNullException">The parameter writer/roleDescriptor/roleDescriptor.Contacts/roleDescriptor.Keys is null.</exception>
        protected virtual void WriteRoleDescriptorElements(XmlWriter writer, RoleDescriptor roleDescriptor)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (roleDescriptor == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("roleDescriptor");
            }

            if (roleDescriptor.Contacts == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("roleDescriptor.Contacts");
            }

            if (roleDescriptor.Keys == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("roleDescriptor.Keys");
            }

            // Optional
            if (roleDescriptor.Organization != null)
            {
                WriteOrganization(writer, roleDescriptor.Organization);
            }

            // Optional
            foreach (KeyDescriptor key in roleDescriptor.Keys)
            {
                WriteKeyDescriptor(writer, key);
            }

            // Optional
            foreach (ContactPerson contact in roleDescriptor.Contacts)
            {
                WriteContactPerson(writer, contact);
            }

            WriteCustomElements<RoleDescriptor>(writer, roleDescriptor);
        }

        /// <summary>
        /// Writes a security token service descriptor.
        /// </summary>
        /// <param name="writer">The xml writer.</param>
        /// <param name="securityTokenServiceDescriptor">The <see cref="SecurityTokenServiceDescriptor"/>.</param>
        /// <exception cref="ArgumentNullException">The parameter writer/securityTokenServiceDescriptor/securityTokenServiceDescriptor.Endpoint/
        /// securityTokenServiceDescriptor.PassiveRequestorEndpoints is null.</exception>
        protected virtual void WriteSecurityTokenServiceDescriptor(XmlWriter writer, SecurityTokenServiceDescriptor securityTokenServiceDescriptor)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (securityTokenServiceDescriptor == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("securityTokenServiceDescriptor");
            }

            if (securityTokenServiceDescriptor.SecurityTokenServiceEndpoints == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("securityTokenServiceDescriptor.Endpoints");
            }

            if (securityTokenServiceDescriptor.PassiveRequestorEndpoints == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("securityTokenServiceDescriptor.PassiveRequestorEndpoints");
            }

            writer.WriteStartElement(Saml2MetadataConstants.Elements.RoleDescriptor, Saml2MetadataConstants.Namespace);
            writer.WriteAttributeString("xsi", "type", XmlSchema.InstanceNamespace, FederationMetadataConstants.Prefix + ":" + FederationMetadataConstants.Elements.SecurityTokenServiceType);

            writer.WriteAttributeString("xmlns", FederationMetadataConstants.Prefix, null, FederationMetadataConstants.Namespace);

            WriteWebServiceDescriptorAttributes(writer, securityTokenServiceDescriptor);
            WriteCustomAttributes<SecurityTokenServiceDescriptor>(writer, securityTokenServiceDescriptor);

            WriteWebServiceDescriptorElements(writer, securityTokenServiceDescriptor);

            if (securityTokenServiceDescriptor.SecurityTokenServiceEndpoints.Count == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MetadataSerializationException(SR.GetString(SR.ID3203, FederationMetadataConstants.Elements.SecurityTokenServiceEndpoint)));
            }

            foreach (EndpointReference epr in securityTokenServiceDescriptor.SecurityTokenServiceEndpoints)
            {
                writer.WriteStartElement(FederationMetadataConstants.Elements.SecurityTokenServiceEndpoint, FederationMetadataConstants.Namespace);
                epr.WriteTo(writer);
                writer.WriteEndElement();
            }

            foreach (EndpointReference epr in securityTokenServiceDescriptor.PassiveRequestorEndpoints)
            {
                writer.WriteStartElement(FederationMetadataConstants.Elements.PassiveRequestorEndpoint, FederationMetadataConstants.Namespace);
                epr.WriteTo(writer);
                writer.WriteEndElement();
            }

            WriteCustomElements<SecurityTokenServiceDescriptor>(writer, securityTokenServiceDescriptor);

            writer.WriteEndElement();
        }

        /// <summary>
        /// Writes an spsso descriptor.
        /// </summary>
        /// <param name="writer">The xml writer.</param>
        /// <param name="serviceProviderSingleSignOnDescriptor">The spsso descriptor.</param>
        /// <exception cref="ArgumentNullException">The input parameter is null.</exception>
        protected virtual void WriteServiceProviderSingleSignOnDescriptor(XmlWriter writer, ServiceProviderSingleSignOnDescriptor serviceProviderSingleSignOnDescriptor)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (serviceProviderSingleSignOnDescriptor == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("spssoDescriptor");
            }

            if (serviceProviderSingleSignOnDescriptor.AssertionConsumerServices == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("spssoDescriptor.AssertionConsumerService");
            }

            writer.WriteStartElement(Saml2MetadataConstants.Elements.SpssoDescriptor, Saml2MetadataConstants.Namespace);
            if (serviceProviderSingleSignOnDescriptor.AuthenticationRequestsSigned)
            {
                writer.WriteAttributeString(Saml2MetadataConstants.Attributes.AuthenticationRequestsSigned, null,
                    XmlConvert.ToString(serviceProviderSingleSignOnDescriptor.AuthenticationRequestsSigned));
            }

            if (serviceProviderSingleSignOnDescriptor.WantAssertionsSigned)
            {
                writer.WriteAttributeString(Saml2MetadataConstants.Attributes.WantAssertionsSigned, null,
                    XmlConvert.ToString(serviceProviderSingleSignOnDescriptor.WantAssertionsSigned));
            }

            WriteSingleSignOnDescriptorAttributes(writer, serviceProviderSingleSignOnDescriptor);
            WriteCustomAttributes<ServiceProviderSingleSignOnDescriptor>(writer, serviceProviderSingleSignOnDescriptor);

            WriteSingleSignOnDescriptorElements(writer, serviceProviderSingleSignOnDescriptor);
            if (serviceProviderSingleSignOnDescriptor.AssertionConsumerServices.Count == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MetadataSerializationException(SR.GetString(SR.ID3203, Saml2MetadataConstants.Elements.AssertionConsumerService)));
            }

            foreach (IndexedProtocolEndpoint ep in serviceProviderSingleSignOnDescriptor.AssertionConsumerServices.Values)
            {
                XmlQualifiedName element = new XmlQualifiedName(Saml2MetadataConstants.Elements.AssertionConsumerService, Saml2MetadataConstants.Namespace);
                WriteIndexedProtocolEndpoint(writer, ep, element);
            }

            WriteCustomElements<ServiceProviderSingleSignOnDescriptor>(writer, serviceProviderSingleSignOnDescriptor);
            writer.WriteEndElement(); // SPSSODescriptor
        }

        /// <summary>
        /// Writes the sso descriptor attributers.
        /// </summary>
        /// <param name="writer">The xml writer.</param>
        /// <param name="singleSignOnDescriptor">The sso descriptor.</param>
        protected virtual void WriteSingleSignOnDescriptorAttributes(XmlWriter writer, SingleSignOnDescriptor singleSignOnDescriptor)
        {
            WriteRoleDescriptorAttributes(writer, singleSignOnDescriptor);
            WriteCustomAttributes<SingleSignOnDescriptor>(writer, singleSignOnDescriptor);
        }

        /// <summary>
        /// Writes the sso descriptor element.
        /// </summary>
        /// <param name="writer">The xml writer.</param>
        /// <param name="singleSignOnDescriptor">The sso descriptor.</param>
        protected virtual void WriteSingleSignOnDescriptorElements(XmlWriter writer, SingleSignOnDescriptor singleSignOnDescriptor)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (singleSignOnDescriptor == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("ssoDescriptor");
            }

            WriteRoleDescriptorElements(writer, singleSignOnDescriptor);

            if (singleSignOnDescriptor.ArtifactResolutionServices != null && singleSignOnDescriptor.ArtifactResolutionServices.Count > 0)
            {
                // Write the artifact resolution services
                foreach (IndexedProtocolEndpoint ep in singleSignOnDescriptor.ArtifactResolutionServices.Values)
                {
                    if (ep.ResponseLocation != null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MetadataSerializationException(SR.GetString(SR.ID3249, Saml2MetadataConstants.Attributes.ResponseLocation)));
                    }

                    XmlQualifiedName element = new XmlQualifiedName(Saml2MetadataConstants.Elements.ArtifactResolutionService, Saml2MetadataConstants.Namespace);
                    WriteIndexedProtocolEndpoint(writer, ep, element);
                }
            }

            if (singleSignOnDescriptor.SingleLogoutServices != null && singleSignOnDescriptor.SingleLogoutServices.Count > 0)
            {
                // Write the single logout service endpoints.
                foreach (ProtocolEndpoint endpoint in singleSignOnDescriptor.SingleLogoutServices)
                {
                    XmlQualifiedName element = new XmlQualifiedName(Saml2MetadataConstants.Elements.SingleLogoutService, Saml2MetadataConstants.Namespace);
                    WriteProtocolEndpoint(writer, endpoint, element);
                }
            }

            if (singleSignOnDescriptor.NameIdentifierFormats != null && singleSignOnDescriptor.NameIdentifierFormats.Count > 0)
            {
                // Write the name id formats
                foreach (Uri nameId in singleSignOnDescriptor.NameIdentifierFormats)
                {
                    if (!nameId.IsAbsoluteUri)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MetadataSerializationException(SR.GetString(SR.ID0014, Saml2MetadataConstants.Elements.NameIDFormat)));
                    }

                    writer.WriteStartElement(Saml2MetadataConstants.Elements.NameIDFormat, Saml2MetadataConstants.Namespace);
                    writer.WriteString(nameId.AbsoluteUri);
                    writer.WriteEndElement();
                }
            }

            WriteCustomElements<SingleSignOnDescriptor>(writer, singleSignOnDescriptor);
        }

        /// <summary>
        /// Write a web service description's attributes.
        /// </summary>
        /// <param name="writer">The xml writer.</param>
        /// <param name="wsDescriptor">The web service desriptor.</param>
        /// <exception cref="ArgumentNullException">The input parameter is null.</exception>
        protected virtual void WriteWebServiceDescriptorAttributes(XmlWriter writer, WebServiceDescriptor wsDescriptor)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (wsDescriptor == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("wsDescriptor");
            }

            WriteRoleDescriptorAttributes(writer, wsDescriptor);

            if (!String.IsNullOrEmpty(wsDescriptor.ServiceDisplayName))
            {
                writer.WriteAttributeString(Saml2MetadataConstants.Attributes.ServiceDisplayName, null, wsDescriptor.ServiceDisplayName);
            }

            if (!String.IsNullOrEmpty(wsDescriptor.ServiceDescription))
            {
                writer.WriteAttributeString(Saml2MetadataConstants.Attributes.ServiceDescription, null, wsDescriptor.ServiceDescription);
            }

            WriteCustomAttributes<WebServiceDescriptor>(writer, wsDescriptor);
        }

        /// <summary>
        /// Write a web service description element.
        /// </summary>
        /// <param name="writer">The xml writer.</param>
        /// <param name="wsDescriptor">The web service desriptor.</param>
        /// <exception cref="ArgumentNullException">The input parameter is null.</exception>
        protected virtual void WriteWebServiceDescriptorElements(XmlWriter writer, WebServiceDescriptor wsDescriptor)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (wsDescriptor == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("wsDescriptor");
            }

            if (wsDescriptor.TargetScopes == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("wsDescriptor.TargetScopes");
            }

            if (wsDescriptor.ClaimTypesOffered == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("wsDescriptor.ClaimTypesOffered");
            }

            if (wsDescriptor.TokenTypesOffered == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("wsDescriptor.TokenTypesOffered");
            }

            WriteRoleDescriptorElements(writer, wsDescriptor);

            if (wsDescriptor.TokenTypesOffered.Count > 0)
            {
                writer.WriteStartElement(FederationMetadataConstants.Elements.TokenTypesOffered, FederationMetadataConstants.Namespace);
                foreach (Uri tokenType in wsDescriptor.TokenTypesOffered)
                {
                    writer.WriteStartElement(WSFederationMetadataConstants.Elements.TokenType, WSFederationMetadataConstants.Namespace);
                    if (!tokenType.IsAbsoluteUri)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MetadataSerializationException(SR.GetString(SR.ID3203, WSAuthorizationConstants.Elements.ClaimType)));
                    }

                    writer.WriteAttributeString(WSFederationMetadataConstants.Attributes.Uri, tokenType.AbsoluteUri);
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
            }

            if (wsDescriptor.ClaimTypesOffered.Count > 0)
            {
                writer.WriteStartElement(FederationMetadataConstants.Elements.ClaimTypesOffered, FederationMetadataConstants.Namespace);
                foreach (DisplayClaim claim in wsDescriptor.ClaimTypesOffered)
                {
                    WriteDisplayClaim(writer, claim);
                }

                writer.WriteEndElement();
            }

            if (wsDescriptor.ClaimTypesRequested.Count > 0)
            {
                writer.WriteStartElement(FederationMetadataConstants.Elements.ClaimTypesRequested, FederationMetadataConstants.Namespace);
                foreach (DisplayClaim claim in wsDescriptor.ClaimTypesRequested)
                {
                    WriteDisplayClaim(writer, claim);
                }

                writer.WriteEndElement();
            }

            if (wsDescriptor.TargetScopes.Count > 0)
            {
                writer.WriteStartElement(FederationMetadataConstants.Elements.TargetScopes, FederationMetadataConstants.Namespace);
                foreach (EndpointReference address in wsDescriptor.TargetScopes)
                {
                    address.WriteTo(writer);
                }

                writer.WriteEndElement();
            }

            WriteCustomElements<WebServiceDescriptor>(writer, wsDescriptor);
        }

        /// <summary>
        /// Reads the &lt;saml:Attribute> element.
        /// </summary>
        /// <remarks>
        /// The default implementation requires that the content of the 
        /// Attribute element be a simple string. To handle complex content
        /// or content of declared simple types other than xs:string, override
        /// this method.
        /// </remarks>
        /// <param name="reader">The xml reader.</param>
        /// <returns>A Saml2 attribute.</returns>
        /// <exception cref="ArgumentNullException">The input parameter is null.</exception>
        protected virtual Saml2Attribute ReadAttribute(XmlReader reader)
        {
            if (null == reader)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            // throw if wrong element
            if (!reader.IsStartElement(Saml2Constants.Elements.Attribute, Saml2Constants.Namespace))
            {
                reader.ReadStartElement(Saml2Constants.Elements.Attribute, Saml2Constants.Namespace);
            }

            try
            {
                Saml2Attribute attribute;
                bool isEmpty = reader.IsEmptyElement;

                // @attributes
                string value;

                // @xsi:type 
                XmlUtil.ValidateXsiType(reader, Saml2Constants.Types.AttributeType, Saml2Constants.Namespace);

                // @Name - required
                value = reader.GetAttribute(Saml2Constants.Attributes.Name);
                if (String.IsNullOrEmpty(value))
                {
                    throw DiagnosticUtility.ThrowHelperXml(reader, SR.GetString(SR.ID0001, Saml2Constants.Attributes.Name, Saml2Constants.Elements.Attribute));
                }

                attribute = new Saml2Attribute(value);

                // @NameFormat - optional
                value = reader.GetAttribute(Saml2Constants.Attributes.NameFormat);
                if (!String.IsNullOrEmpty(value))
                {
                    if (!UriUtil.CanCreateValidUri(value, UriKind.Absolute))
                    {
                        throw DiagnosticUtility.ThrowHelperXml(reader, SR.GetString(SR.ID0011, Saml2Constants.Attributes.Namespace, Saml2Constants.Elements.Action));
                    }

                    attribute.NameFormat = new Uri(value);
                }

                // @FriendlyName - optional
                attribute.FriendlyName = reader.GetAttribute(Saml2Constants.Attributes.FriendlyName);

                // content
                reader.Read();
                if (!isEmpty)
                {
                    while (reader.IsStartElement(Saml2Constants.Elements.AttributeValue, Saml2Constants.Namespace))
                    {
                        bool isEmptyValue = reader.IsEmptyElement;
                        bool isNil = XmlUtil.IsNil(reader);

                        // For now, the value must be a string
                        XmlUtil.ValidateXsiType(reader, "string", XmlSchema.Namespace);

                        if (isNil)
                        {
                            reader.Read();
                            if (!isEmptyValue)
                            {
                                reader.ReadEndElement();
                            }

                            attribute.Values.Add(null);
                        }
                        else if (isEmptyValue)
                        {
                            reader.Read();
                            attribute.Values.Add("");
                        }
                        else
                        {
                            attribute.Values.Add(reader.ReadElementString());
                        }
                    }

                    reader.ReadEndElement();
                }

                return attribute;
            }
            catch (Exception e)
            {
                if (System.Runtime.Fx.IsFatal(e))
                    throw;

                Exception wrapped = TryWrapReadException(reader, e);
                if (null == wrapped)
                {
                    throw;
                }
                else
                {
                    throw wrapped;
                }
            }
        }

        /// <summary>
        /// Writes the &lt;saml:Attribute> element.
        /// </summary>
        /// <param name="writer">The xml writer.</param>
        /// <param name="data">The Saml2 attibute.</param>
        /// <exception cref="ArgumentNullException">The input parameter is null.</exception>
        protected virtual void WriteAttribute(XmlWriter writer, Saml2Attribute data)
        {
            if (null == writer)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (null == data)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("data");
            }

            // <Attribute>
            writer.WriteStartElement(Saml2Constants.Elements.Attribute, Saml2Constants.Namespace);

            // @Name - required
            writer.WriteAttributeString(Saml2Constants.Attributes.Name, data.Name);

            // @NameFormat - optional
            if (null != data.NameFormat)
            {
                writer.WriteAttributeString(Saml2Constants.Attributes.NameFormat, data.NameFormat.AbsoluteUri);
            }

            // @FriendlyName - optional
            if (null != data.FriendlyName)
            {
                writer.WriteAttributeString(Saml2Constants.Attributes.FriendlyName, data.FriendlyName);
            }

            // <AttributeValue> 0-OO (nillable)
            foreach (string value in data.Values)
            {
                writer.WriteStartElement(Saml2Constants.Elements.AttributeValue, Saml2Constants.Namespace);

                if (null == value)
                {
                    writer.WriteAttributeString("nil", XmlSchema.InstanceNamespace, XmlConvert.ToString(true));
                }
                else if (value.Length > 0)
                {
                    writer.WriteString(value);
                }

                writer.WriteEndElement();
            }

            // </Attribute>
            writer.WriteEndElement();
        }

        // Wraps common data validation exceptions with an XmlException 
        // associated with the failing reader
        private static Exception TryWrapReadException(XmlReader reader, Exception inner)
        {
            if (inner is FormatException
                || inner is ArgumentException
                || inner is InvalidOperationException
                || inner is OverflowException)
            {
                return DiagnosticUtility.ThrowHelperXml(reader, SR.GetString(SR.ID4125), inner);
            }

            return null;
        }
    }
}
