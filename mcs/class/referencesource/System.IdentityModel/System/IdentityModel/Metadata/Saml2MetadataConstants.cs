//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel.Metadata
{
    /// <summary>
    /// Defines the constants for Saml2 Metadata.
    /// </summary>
    internal static class Saml2MetadataConstants
    {
#pragma warning disable 1591
        public static class Attributes
        {
            // ID
            public const string Id = "ID";

            // ContactPerson
            public const string ContactType = "contactType";

            // KeyDescriptor
            public const string Algorithm = "Algorithm";
            public const string Use = "use";

            // Endpoint
            public const string Binding = "Binding";
            public const string EndpointIndex = "index";
            public const string EndpointIsDefault = "isDefault";
            public const string Location = "Location";
            public const string ResponseLocation = "ResponseLocation";

            // EntityDescriptor
            public const string EntityId = "entityID";

            // RoleDescriptor
            public const string ErrorUrl = "errorURL";
            public const string ProtocolsSupported = "protocolSupportEnumeration";
            public const string ValidUntil = "validUntil";

            // EntitiesDescriptor
            public const string EntityGroupName = "Name";

            // WebServiceDescriptor
            public const string ServiceDescription = "ServiceDescription";
            public const string ServiceDisplayName = "ServiceDisplayName";

            // IDPSSODescriptor
            public const string WantAuthenticationRequestsSigned = "WantAuthnRequestsSigned";

            // SPSSODescriptor
            public const string AuthenticationRequestsSigned = "AuthnRequestsSigned";
            public const string WantAssertionsSigned = "WantAssertionsSigned";

        }

        public static class Elements
        {
            public const string EntitiesDescriptor = "EntitiesDescriptor";
            public const string EntityDescriptor = "EntityDescriptor";

            public const string IdpssoDescriptor = "IDPSSODescriptor";
            public const string RoleDescriptor = "RoleDescriptor";
            public const string SpssoDescriptor = "SPSSODescriptor";

            // ContactPerson
            public const string Company = "Company";
            public const string ContactPerson = "ContactPerson";
            public const string EmailAddress = "EmailAddress";
            public const string GivenName = "GivenName";
            public const string Surname = "SurName";
            public const string TelephoneNumber = "TelephoneNumber";

            // Organization
            public const string Organization = "Organization";
            public const string OrganizationDisplayName = "OrganizationDisplayName";
            public const string OrganizationName = "OrganizationName";
            public const string OrganizationUrl = "OrganizationURL";

            // KeyDescriptor
            public const string EncryptionMethod = "EncryptionMethod";
            public const string KeyDescriptor = "KeyDescriptor";

            // SSODescriptor
            public const string ArtifactResolutionService = "ArtifactResolutionService";
            public const string NameIDFormat = "NameIDFormat";
            public const string SingleLogoutService = "SingleLogoutService";

            // IDPSSODescriptor
            public const string SingleSignOnService = "SingleSignOnService";

            // SPSSODescriptor   
            public const string AssertionConsumerService = "AssertionConsumerService";
        }

        public const string Namespace = "urn:oasis:names:tc:SAML:2.0:metadata";
#pragma warning restore 1591
    }
}
