//------------------------------------------------------------------------------
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using System.IdentityModel.Configuration;
using System.IdentityModel.Diagnostics.Application;
using System.Runtime.Diagnostics;

namespace System.IdentityModel.Tokens
{
    /// <summary>
    /// Implements a name service that resolves issuer tokens to strings. This class maintains a
    /// list of trusted issuers dictionary that maps the trust issuer certificate thumbpring to a
    /// issuer name. The class can only resolve X.509Certificates. The map can be configured in 
    /// App.config/Web.Config using the following configuration settings.
    /// &lt;system.identityModel>
    ///     &lt;issuerNameRegistry type='ConfigurationBasedIssuerNameRegistry'>
    ///         &lt;trustedIssuers>
    ///             &lt;add thumbprint='ASN.1EncodedFormOfTheThumbprint' name='MappedName' />
    ///             &lt;add thumbprint='ASN.1EncodedFormOfTheThumbprint' />
    ///             &lt;remove thumbprint='ASN.1EncodedFormOfTheThumbprint' />
    ///             &lt; clear/>
    ///         &lt;trustedIssuers/>
    ///     &lt;/issuerNameRegistry>
    /// &lt;/system.identityModel>
    /// </summary>
    public class ConfigurationBasedIssuerNameRegistry : IssuerNameRegistry
    {
        Dictionary<string, string> _configuredTrustedIssuers = new Dictionary<string, string>(new ThumbprintKeyComparer());

        /// <summary>
        /// Creates an instance of <see cref="ConfigurationBasedIssuerNameRegistry"/>
        /// </summary>
        public ConfigurationBasedIssuerNameRegistry()
        {
        }

        /// <summary>
        /// Custom handling of configuration elements
        /// </summary>
        /// <param name="customConfiguration">Custom configuration to be loaded. This is the XmlElement 
        /// that represents the map that is specified in App.config.</param>
        /// <exception cref="ArgumentNullException">The input parameter 'customConfiguration' is null.</exception>
        /// <exception cref="InvalidOperationException">The configuration contains element that is not 
        /// recognized.</exception>
        public override void LoadCustomConfiguration(XmlNodeList customConfiguration)
        {
            if (customConfiguration == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("customConfiguration");
            }
            //
            // We only expect a single child here - TrustedIssuers
            //
            List<XmlElement> configNodes = XmlUtil.GetXmlElements(customConfiguration);

            if (configNodes.Count != 1)
            {
                throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID7019, typeof(ConfigurationBasedIssuerNameRegistry).Name));
            }

            XmlElement customConfigElement = configNodes[0];

            if (!StringComparer.Ordinal.Equals(customConfigElement.LocalName, ConfigurationStrings.TrustedIssuers))
            {
                throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID7002, customConfigElement.LocalName, ConfigurationStrings.TrustedIssuers));
            }

            foreach (XmlNode node in customConfigElement.ChildNodes)
            {
                XmlElement childElement = node as XmlElement;
                if (childElement != null)
                {
                    if (StringComparer.Ordinal.Equals(childElement.LocalName, ConfigurationStrings.Add))
                    {
                        var thumbprintAttribute = childElement.Attributes.GetNamedItem(ConfigurationStrings.Thumbprint);
                        var nameAttribute = childElement.Attributes.GetNamedItem(ConfigurationStrings.Name);

                        if (childElement.Attributes.Count > 2 || thumbprintAttribute == null)
                        {
                            throw DiagnosticUtility.ThrowHelperInvalidOperation(
                                SR.GetString(
                                SR.ID7010,
                                String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}/{1}", customConfigElement.LocalName, childElement.LocalName),
                                String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0} and {1}", ConfigurationStrings.Thumbprint, ConfigurationStrings.Name)));
                        }

                        string thumbprint = thumbprintAttribute.Value;
                        thumbprint = thumbprint.Replace(" ", "");
                        // add issuer name to interned strings since it will show up in many claims
                        string issuerName = ((nameAttribute == null) || string.IsNullOrEmpty(nameAttribute.Value)) ? String.Empty : String.Intern(nameAttribute.Value);
                        _configuredTrustedIssuers.Add(thumbprint, issuerName);
                    }
                    else if (StringComparer.Ordinal.Equals(childElement.LocalName, ConfigurationStrings.Remove))
                    {
                        if (childElement.Attributes.Count != 1 || !StringComparer.Ordinal.Equals(childElement.Attributes[0].LocalName, ConfigurationStrings.Thumbprint))
                        {
                            throw DiagnosticUtility.ThrowHelperInvalidOperation(
                                SR.GetString(
                                SR.ID7010,
                                String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}/{1}", customConfigElement.LocalName, childElement.LocalName),
                                ConfigurationStrings.Thumbprint));
                        }

                        string thumbprint = childElement.Attributes.GetNamedItem(ConfigurationStrings.Thumbprint).Value;
                        thumbprint = thumbprint.Replace(" ", "");
                        _configuredTrustedIssuers.Remove(thumbprint);
                    }
                    else if (StringComparer.Ordinal.Equals(childElement.LocalName, ConfigurationStrings.Clear))
                    {
                        _configuredTrustedIssuers.Clear();
                    }
                    else
                    {
                        throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID7002, customConfigElement.LocalName, childElement.LocalName));
                    }
                }
            }
        }

        /// <summary>
        /// Returns the issuer name of the given X509SecurityToken mapping the Certificate Thumbprint to 
        /// a name in the configured map.
        /// </summary>
        /// <param name="securityToken">SecurityToken for which the issuer name is requested.</param>
        /// <returns>Issuer name if the token was registered, null otherwise.</returns>
        /// <exception cref="ArgumentNullException">The input parameter 'securityToken' is null.</exception>
        public override string GetIssuerName(SecurityToken securityToken)
        {
            if (securityToken == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("securityToken");
            }

            X509SecurityToken x509SecurityToken = securityToken as X509SecurityToken;
            if (x509SecurityToken != null)
            {
                string thumbprint = x509SecurityToken.Certificate.Thumbprint;
                if (_configuredTrustedIssuers.ContainsKey(thumbprint))
                {
                    string issuerName = _configuredTrustedIssuers[thumbprint];
                    issuerName = string.IsNullOrEmpty(issuerName) ? x509SecurityToken.Certificate.Subject : issuerName;

                    if (TD.GetIssuerNameSuccessIsEnabled())
                    {
                        TD.GetIssuerNameSuccess(EventTraceActivity.GetFromThreadOrCreate(), issuerName, securityToken.Id);
                    }

                    return issuerName;
                }
            }

            if (TD.GetIssuerNameFailureIsEnabled())
            {
                TD.GetIssuerNameFailure(EventTraceActivity.GetFromThreadOrCreate(), securityToken.Id);
            }

            return null;
        }

        /// <summary>
        /// Gets the Dictionary of Configured Trusted Issuers. The key
        /// to the dictionary is the ASN.1 encoded form of the Thumbprint 
        /// of the trusted issuer's certificate and the value is the issuer name. 
        /// </summary>
        public IDictionary<string, string> ConfiguredTrustedIssuers
        {
            get { return _configuredTrustedIssuers; }
        }

        /// <summary>
        /// Adds a trusted issuer to the collection.
        /// </summary>
        /// <param name="certificateThumbprint">ASN.1 encoded form of the trusted issuer's certificate Thumbprint.</param>
        /// <param name="name">Name of the trusted issuer.</param>
        /// <exception cref="ArgumentException">The argument 'certificateThumbprint' or 'name' is either null or Empty.</exception>
        /// <exception cref="InvalidOperationException">The issuer specified by 'certificateThumbprint' argument has already been configured.</exception>
        public void AddTrustedIssuer(string certificateThumbprint, string name)
        {
            if (string.IsNullOrEmpty(certificateThumbprint))
            {
                throw DiagnosticUtility.ThrowHelperArgumentNullOrEmptyString("certificateThumbprint");
            }

            if (string.IsNullOrEmpty(name))
            {
                throw DiagnosticUtility.ThrowHelperArgumentNullOrEmptyString("name");
            }

            if (_configuredTrustedIssuers.ContainsKey(certificateThumbprint))
            {
                throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID4265, certificateThumbprint));
            }

            certificateThumbprint = certificateThumbprint.Replace(" ", "");

            _configuredTrustedIssuers.Add(certificateThumbprint, name);
        }

        class ThumbprintKeyComparer : IEqualityComparer<string>
        {
            #region IEqualityComparer<string> Members

            public bool Equals(string x, string y)
            {
                return StringComparer.OrdinalIgnoreCase.Equals(x, y);
            }

            public int GetHashCode(string obj)
            {
                return obj.ToUpper(CultureInfo.InvariantCulture).GetHashCode();
            }

            #endregion
        }
    }
}
