//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Xml;
    using System.ServiceModel.Description;
    using System.Collections;

    public class CompositeDuplexBindingElementImporter : IPolicyImportExtension
    {
        public CompositeDuplexBindingElementImporter()
        {
        }

        void IPolicyImportExtension.ImportPolicy(MetadataImporter importer, PolicyConversionContext context)
        {
            if (importer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("importer");
            }

            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

            XmlElement compositeDuplexAssertion = PolicyConversionContext.FindAssertion(context.GetBindingAssertions(),
                TransportPolicyConstants.CompositeDuplex, TransportPolicyConstants.CompositeDuplexNamespace, true);

            if (compositeDuplexAssertion != null
                || WsdlImporter.WSAddressingHelper.DetermineSupportedAddressingMode(importer, context) == SupportedAddressingMode.NonAnonymous)
            {
                context.BindingElements.Add(new CompositeDuplexBindingElement());
            }
        }
    }
}
