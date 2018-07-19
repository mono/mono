//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Xml;
    using System.ServiceModel.Description;
    using System.ServiceModel.Configuration;

    static class UseManagedPresentationPolicyStrings
    {
        public const string UseManagedPresentationName = "UseManagedPresentation";
        public const string RequireFederatedIdentityProvisioningName = "RequireFederatedIdentityProvisioning";
        public const string UseManagedPresentationNamespace = "http://schemas.xmlsoap.org/ws/2005/05/identity";
        public const string UseManagedPresentationPrefix = "ic";
    }

    public sealed class UseManagedPresentationBindingElementImporter : IPolicyImportExtension
    {
        void IPolicyImportExtension.ImportPolicy(MetadataImporter importer, PolicyConversionContext policyContext)
        {
            if (policyContext == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("policyContext");

            XmlElement useManagedPresentationAssertion = PolicyConversionContext.FindAssertion(policyContext.GetBindingAssertions(),
                UseManagedPresentationPolicyStrings.RequireFederatedIdentityProvisioningName, UseManagedPresentationPolicyStrings.UseManagedPresentationNamespace, true);
            if (useManagedPresentationAssertion != null)
            {
                UseManagedPresentationBindingElement settings =
                    policyContext.BindingElements.Find<UseManagedPresentationBindingElement>();

                if (null == settings)
                {
                    settings = new UseManagedPresentationBindingElement();
                    policyContext.BindingElements.Add(settings);
                }
            }

        }
    }
}
