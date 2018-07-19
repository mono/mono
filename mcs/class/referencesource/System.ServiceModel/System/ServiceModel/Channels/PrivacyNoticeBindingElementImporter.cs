//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Xml;
    using System.ServiceModel.Description;
    using System.ServiceModel.Configuration;

    static class PrivacyNoticePolicyStrings
    {
        public const string PrivacyNoticeName = "PrivacyNotice";
        public const string PrivacyNoticeVersionAttributeName = "Version";
        public const string PrivacyNoticeNamespace = "http://schemas.xmlsoap.org/ws/2005/05/identity";
        public const string PrivacyNoticePrefix = "ic";
    }

    public sealed class PrivacyNoticeBindingElementImporter : IPolicyImportExtension
    {
        void IPolicyImportExtension.ImportPolicy(MetadataImporter importer, PolicyConversionContext policyContext)
        {
            if (policyContext == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("policyContext");

            XmlElement privacyNoticeAssertion = PolicyConversionContext.FindAssertion(policyContext.GetBindingAssertions(),
                PrivacyNoticePolicyStrings.PrivacyNoticeName, PrivacyNoticePolicyStrings.PrivacyNoticeNamespace, true);
            if (privacyNoticeAssertion != null)
            {
                PrivacyNoticeBindingElement settings =
                    policyContext.BindingElements.Find<PrivacyNoticeBindingElement>();

                if (null == settings)
                {
                    settings = new PrivacyNoticeBindingElement();
                    policyContext.BindingElements.Add(settings);
                }

                settings.Url = new Uri(privacyNoticeAssertion.InnerText);
                string versionString = privacyNoticeAssertion.GetAttribute(PrivacyNoticePolicyStrings.PrivacyNoticeVersionAttributeName, PrivacyNoticePolicyStrings.PrivacyNoticeNamespace);
                if (string.IsNullOrEmpty(versionString))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.CannotImportPrivacyNoticeElementWithoutVersionAttribute)));
                }

                int version = 0;
                if (!Int32.TryParse(versionString, out version))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.PrivacyNoticeElementVersionAttributeInvalid)));
                }
                settings.Version = version;
            }
        }
    }
}
