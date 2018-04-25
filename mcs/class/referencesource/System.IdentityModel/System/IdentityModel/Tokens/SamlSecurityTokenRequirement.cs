//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.IdentityModel.Tokens
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IdentityModel.Configuration;
    using System.IdentityModel.Diagnostics;
    using System.IdentityModel.Selectors;
    using System.Security.Claims;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel.Security;
    using System.Text;
    using System.Xml;

    /// <summary>
    /// Extends SecurityTokenRequirement by adding new properties which are
    /// useful for issued tokens.
    /// </summary>
    public class SamlSecurityTokenRequirement
    {

        //
        // The below defaults will only be used if some verification properties are set in config and others are not
        //
        static X509RevocationMode DefaultRevocationMode = X509RevocationMode.Online;
        static X509CertificateValidationMode DefaultValidationMode = X509CertificateValidationMode.PeerOrChainTrust;
        static StoreLocation DefaultStoreLocation = StoreLocation.LocalMachine;

        string _nameClaimType = ClaimsIdentity.DefaultNameClaimType;
        string _roleClaimType = ClaimTypes.Role;

        bool _mapToWindows;
        
        X509CertificateValidator _certificateValidator;

        /// <summary>
        /// Creates an instance of <see cref="SamlSecurityTokenRequirement"/>
        /// </summary>
        public SamlSecurityTokenRequirement()
        {
        }

        /// <summary>
        /// Creates an instance of <see cref="SamlSecurityTokenRequirement"/>
        /// <param name="element">The XmlElement from which the instance is to be loaded.</param>
        /// </summary>
        public SamlSecurityTokenRequirement(XmlElement element)
        {
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }

            if (element.LocalName != ConfigurationStrings.SamlSecurityTokenRequirement)
            {
                throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID7000, ConfigurationStrings.SamlSecurityTokenRequirement, element.LocalName));
            }

            bool foundCustomX509Validator = false;
            X509RevocationMode revocationMode = DefaultRevocationMode;
            X509CertificateValidationMode certificateValidationMode = DefaultValidationMode;
            StoreLocation trustedStoreLocation = DefaultStoreLocation;
            string customValidator = null;

            foreach (XmlAttribute attribute in element.Attributes)
            {
                if (StringComparer.OrdinalIgnoreCase.Equals(attribute.LocalName, ConfigurationStrings.MapToWindows))
                {
                    bool outMapToWindows = false;
                    if (!bool.TryParse(attribute.Value, out outMapToWindows))
                    {
                        throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID7022, attribute.Value));
                    }
                    this.MapToWindows = outMapToWindows;
                }                
                else if (StringComparer.OrdinalIgnoreCase.Equals(attribute.LocalName, ConfigurationStrings.IssuerCertificateValidator))
                {
                    customValidator = attribute.Value.ToString();
                }
                else if (StringComparer.OrdinalIgnoreCase.Equals(attribute.LocalName, ConfigurationStrings.IssuerCertificateRevocationMode))
                {
                    foundCustomX509Validator = true;

                    string revocationModeString = attribute.Value.ToString();

                    if (StringComparer.OrdinalIgnoreCase.Equals(revocationModeString, ConfigurationStrings.X509RevocationModeNoCheck))
                    {
                        revocationMode = X509RevocationMode.NoCheck;
                    }

                    else if (StringComparer.OrdinalIgnoreCase.Equals(revocationModeString, ConfigurationStrings.X509RevocationModeOffline))
                    {
                        revocationMode = X509RevocationMode.Offline;
                    }

                    else if (StringComparer.OrdinalIgnoreCase.Equals(revocationModeString, ConfigurationStrings.X509RevocationModeOnline))
                    {
                        revocationMode = X509RevocationMode.Online;
                    }

                    else
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ID7011, attribute.LocalName, element.LocalName)));
                    }
                }
                else if (StringComparer.OrdinalIgnoreCase.Equals(attribute.LocalName, ConfigurationStrings.IssuerCertificateValidationMode))
                {
                    foundCustomX509Validator = true;

                    string validationModeString = attribute.Value.ToString();

                    if (StringComparer.OrdinalIgnoreCase.Equals(validationModeString, ConfigurationStrings.X509CertificateValidationModeChainTrust))
                    {
                        certificateValidationMode = X509CertificateValidationMode.ChainTrust;
                    }

                    else if (StringComparer.OrdinalIgnoreCase.Equals(validationModeString, ConfigurationStrings.X509CertificateValidationModePeerOrChainTrust))
                    {
                        certificateValidationMode = X509CertificateValidationMode.PeerOrChainTrust;
                    }

                    else if (StringComparer.OrdinalIgnoreCase.Equals(validationModeString, ConfigurationStrings.X509CertificateValidationModePeerTrust))
                    {
                        certificateValidationMode = X509CertificateValidationMode.PeerTrust;
                    }

                    else if (StringComparer.OrdinalIgnoreCase.Equals(validationModeString, ConfigurationStrings.X509CertificateValidationModeNone))
                    {
                        certificateValidationMode = X509CertificateValidationMode.None;
                    }

                    else if (StringComparer.OrdinalIgnoreCase.Equals(validationModeString, ConfigurationStrings.X509CertificateValidationModeCustom))
                    {
                        certificateValidationMode = X509CertificateValidationMode.Custom;
                    }

                    else
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ID7011, attribute.LocalName, element.LocalName)));
                    }
                }
                else if (StringComparer.OrdinalIgnoreCase.Equals(attribute.LocalName, ConfigurationStrings.IssuerCertificateTrustedStoreLocation))
                {
                    foundCustomX509Validator = true;

                    string trustedStoreLocationString = attribute.Value.ToString();

                    if (StringComparer.OrdinalIgnoreCase.Equals(trustedStoreLocationString, ConfigurationStrings.X509TrustedStoreLocationCurrentUser))
                    {
                        trustedStoreLocation = StoreLocation.CurrentUser;
                    }

                    else if (StringComparer.OrdinalIgnoreCase.Equals(trustedStoreLocationString, ConfigurationStrings.X509TrustedStoreLocationLocalMachine))
                    {
                        trustedStoreLocation = StoreLocation.LocalMachine;
                    }

                    else
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ID7011, attribute.LocalName, element.LocalName)));
                    }
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ID7004, attribute.LocalName, element.LocalName)));
                }
            }

            List<XmlElement> configElements = XmlUtil.GetXmlElements(element.ChildNodes);

            foreach (XmlElement childElement in configElements)
            {
                if (StringComparer.Ordinal.Equals(childElement.LocalName, ConfigurationStrings.NameClaimType))
                {
                    if (childElement.Attributes.Count != 1 || !StringComparer.Ordinal.Equals(childElement.Attributes[0].LocalName, ConfigurationStrings.Value))
                    {
                        throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID7001, String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}/{1}", element.LocalName, childElement.LocalName), ConfigurationStrings.Value));
                    }
                    this.NameClaimType = childElement.Attributes[0].Value;
                }
                else if (StringComparer.Ordinal.Equals(childElement.LocalName, ConfigurationStrings.RoleClaimType))
                {
                    if (childElement.Attributes.Count != 1 || !StringComparer.Ordinal.Equals(childElement.Attributes[0].LocalName, ConfigurationStrings.Value))
                    {
                        throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID7001, String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}/{1}", element.LocalName, childElement.LocalName), ConfigurationStrings.Value));
                    }
                    this.RoleClaimType = childElement.Attributes[0].Value;
                }
                else
                {
                    throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID7002, childElement.LocalName, ConfigurationStrings.SamlSecurityTokenRequirement));
                }
            }

            if (certificateValidationMode == X509CertificateValidationMode.Custom)
            {
                if (string.IsNullOrEmpty(customValidator))
                {
                    throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID7028));
                }

                Type customValidatorType = Type.GetType(customValidator, true);

                if (customValidatorType == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("value", SR.GetString(SR.ID7007, customValidatorType));
                }

                _certificateValidator = CustomTypeElement.Resolve<X509CertificateValidator>(new CustomTypeElement(customValidatorType));
            }
            else if (foundCustomX509Validator)
            {
                _certificateValidator = X509Util.CreateCertificateValidator(certificateValidationMode, revocationMode, trustedStoreLocation);
            }
        }

        /// <summary>
        /// Gets/sets the X509CertificateValidator associated with this token requirement
        /// </summary>
        public X509CertificateValidator CertificateValidator
        {
            get
            {
                return _certificateValidator;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                _certificateValidator = value;
            }
        }

        /// <summary>
        /// Gets or sets the Claim Type that will be used to generate the 
        /// FederatedIdentity.Name property.
        /// </summary>
        public string NameClaimType
        {
            get
            {
                return _nameClaimType;
            }
            set
            {
                _nameClaimType = value;
            }
        }

        /// <summary>
        /// Gets the Claim Types that are used to generate the
        /// FederatedIdentity.Roles property.
        /// </summary>
        public string RoleClaimType
        {
            get
            {
                return _roleClaimType;
            }
            set
            {
                _roleClaimType = value;
            }
        }

        /// <summary>
        /// Determines if the token handler will attempt to map the SAML identity to a
        /// Windows identity via the unique principal name (UPN) claim.
        /// </summary>
        public bool MapToWindows
        {
            get { return _mapToWindows; }
            set { _mapToWindows = value; }
        }

        /// <summary>
        /// Checks if Audience Enforcement checks are required for the given token 
        /// based on this SamlSecurityTokenRequirement settings.
        /// </summary>
        /// <param name="audienceUriMode">
        /// The <see cref="AudienceUriMode"/> defining the audience requirement.
        /// </param>
        /// <param name="token">The Security token to be tested for Audience 
        /// Enforcement.</param>
        /// <returns>True if Audience Enforcement should be applied.</returns>
        /// <exception cref="ArgumentNullException">The input argument 'token' is null.</exception>
        public virtual bool ShouldEnforceAudienceRestriction(AudienceUriMode audienceUriMode, SecurityToken token)
        {
            if (null == token)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("token");
            }

            //
            // Use AudienceUriMode to determine whether the audience 
            // should be enforced
            //
            switch (audienceUriMode)
            {
                case AudienceUriMode.Always:
                    return true;

                case AudienceUriMode.Never:
                    return false;

                case AudienceUriMode.BearerKeyOnly:
#pragma warning suppress 56506
                    return (null == token.SecurityKeys || 0 == token.SecurityKeys.Count);

                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ID4025, audienceUriMode)));
            }
        }

        /// <summary>
        /// Checks the given list of Audience URIs with the AllowedAudienceUri list.
        /// </summary>
        /// <param name="allowedAudienceUris">Collection of AudienceUris.</param>
        /// <param name="tokenAudiences">Collection of audience URIs the token applies to.</param>
        /// <exception cref="ArgumentNullException">The input argument 'allowedAudienceUris' is null.</exception>
        /// <exception cref="ArgumentNullException">The input argument 'tokenAudiences' is null.</exception>
        /// <exception cref="AudienceUriValidationFailedException">Either the input argument 'tokenAudiences' or the configured
        /// 'AudienceUris' collection is empty.</exception>
        public virtual void ValidateAudienceRestriction(IList<Uri> allowedAudienceUris, IList<Uri> tokenAudiences)
        {
            if (null == allowedAudienceUris)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("allowedAudienceUris");
            }

            if (null == tokenAudiences)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("tokenAudiences");
            }

            if (0 == tokenAudiences.Count)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new AudienceUriValidationFailedException(
                    SR.GetString(SR.ID1036)));
            }

            if (0 == allowedAudienceUris.Count)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new AudienceUriValidationFailedException(
                    SR.GetString(SR.ID1043)));
            }

            bool found = false;
            foreach (Uri audience in tokenAudiences)
            {
                if (audience != null)
                {
                    // Strip off any query string or fragment. This is necessary because the 
                    // CardSpace uses the raw Request-URI to form the audience when issuing 
                    // tokens for personal cards, but we clearly don't want things like the 
                    // ReturnUrl parameter affecting the audience matching.
                    Uri audienceLeftPart;
                    if (audience.IsAbsoluteUri)
                    {
                        audienceLeftPart = new Uri(audience.GetLeftPart(UriPartial.Path));
                    }
                    else
                    {
                        Uri baseUri = new Uri("http://www.example.com");
                        Uri resolved = new Uri(baseUri, audience);
                        audienceLeftPart = baseUri.MakeRelativeUri(new Uri(resolved.GetLeftPart(UriPartial.Path)));
                    }

                    if (allowedAudienceUris.Contains(audienceLeftPart))
                    {
                        found = true;
                        break;
                    }
                }
            }

            if (!found)
            {
#pragma warning suppress 56506
                if (1 == tokenAudiences.Count || null != tokenAudiences[0])
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new AudienceUriValidationFailedException(
                        SR.GetString(SR.ID1038, tokenAudiences[0].OriginalString)));
                }
                else
                {
                    StringBuilder sb = new StringBuilder(SR.GetString(SR.ID8007));
                    bool first = true;

                    foreach (Uri a in tokenAudiences)
                    {
                        if (a != null)
                        {
                            if (first)
                            {
                                first = false;
                            }
                            else
                            {
                                sb.Append(", ");
                            }

                            sb.Append(a.OriginalString);
                        }
                    }

                    TraceUtility.TraceString(TraceEventType.Error, sb.ToString());

                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new AudienceUriValidationFailedException(SR.GetString(SR.ID1037)));
                }
            }
        }
    }
}
