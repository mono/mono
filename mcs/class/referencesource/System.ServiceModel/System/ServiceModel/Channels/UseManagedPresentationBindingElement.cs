//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.ServiceModel.Configuration;
    using System.ServiceModel.Description;
    using System.Xml;

    public sealed class UseManagedPresentationBindingElement : BindingElement, IPolicyExportExtension
    {
        public UseManagedPresentationBindingElement()
        {
        }

        public override BindingElement Clone()
        {
            return new UseManagedPresentationBindingElement();
        }

        public override T GetProperty<T>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            return context.GetInnerProperty<T>();
        }

        void IPolicyExportExtension.ExportPolicy(MetadataExporter exporter, PolicyConversionContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

            if (context.BindingElements != null)
            {
                UseManagedPresentationBindingElement settings =
                    context.BindingElements.Find<UseManagedPresentationBindingElement>();

                if (settings != null)
                {
                    XmlDocument doc = new XmlDocument();

                    // UseUseManagedPresentation assertion
                    XmlElement assertion = doc.CreateElement(UseManagedPresentationPolicyStrings.UseManagedPresentationPrefix,
                                                              UseManagedPresentationPolicyStrings.RequireFederatedIdentityProvisioningName,
                                                              UseManagedPresentationPolicyStrings.UseManagedPresentationNamespace);

                    context.GetBindingAssertions().Add(assertion);
                }
            }
        }
    }
}
