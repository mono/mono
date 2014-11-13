//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Configuration;

    [ConfigurationCollection(typeof(X509ScopedServiceCertificateElement))]
    public sealed class X509ScopedServiceCertificateElementCollection : ServiceModelConfigurationElementCollection<X509ScopedServiceCertificateElement>
    {
        public X509ScopedServiceCertificateElementCollection()
            : base()
        { }

        protected override object GetElementKey(ConfigurationElement element)
        {
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }
            X509ScopedServiceCertificateElement scopedCertificateElement = (X509ScopedServiceCertificateElement)element;
            return scopedCertificateElement.TargetUri;
        }
    }
}
