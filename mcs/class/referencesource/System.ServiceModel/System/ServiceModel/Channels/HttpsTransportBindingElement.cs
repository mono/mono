//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.ServiceModel.Description;
    using System.Net;
    using System.Net.Security;
    using System.Runtime.Serialization;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Security;
    using System.Xml;
    using System.ComponentModel;

    public class HttpsTransportBindingElement
        : HttpTransportBindingElement, ITransportTokenAssertionProvider
    {
        bool requireClientCertificate;
        MessageSecurityVersion messageSecurityVersion;

        public HttpsTransportBindingElement()
            : base()
        {
            this.requireClientCertificate = TransportDefaults.RequireClientCertificate;
        }

        protected HttpsTransportBindingElement(HttpsTransportBindingElement elementToBeCloned)
            : base(elementToBeCloned)
        {
            this.requireClientCertificate = elementToBeCloned.requireClientCertificate;
            this.messageSecurityVersion = elementToBeCloned.messageSecurityVersion;
        }

        HttpsTransportBindingElement(HttpTransportBindingElement elementToBeCloned)
            : base(elementToBeCloned)
        {
        }

        [DefaultValue(TransportDefaults.RequireClientCertificate)]
        public bool RequireClientCertificate
        {
            get
            {
                return this.requireClientCertificate;
            }
            set
            {
                this.requireClientCertificate = value;
            }
        }

        public override string Scheme
        {
            get { return "https"; }
        }

        public override BindingElement Clone()
        {
            return new HttpsTransportBindingElement(this);
        }

        internal override bool GetSupportsClientAuthenticationImpl(AuthenticationSchemes effectiveAuthenticationSchemes)
        {
            return this.requireClientCertificate || base.GetSupportsClientAuthenticationImpl(effectiveAuthenticationSchemes);
        }

        internal override bool GetSupportsClientWindowsIdentityImpl(AuthenticationSchemes effectiveAuthenticationSchemes)
        {
            return this.requireClientCertificate || base.GetSupportsClientWindowsIdentityImpl(effectiveAuthenticationSchemes);
        }

        // In order to generate sp:HttpsToken with the right policy.
        // See CSD 3105 for detail.
        internal MessageSecurityVersion MessageSecurityVersion
        {
            get
            {
                return this.messageSecurityVersion;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("value"));
                }
                this.messageSecurityVersion = value;
            }
        }

        public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

            if (this.MessageHandlerFactory != null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.GetString(SR.HttpPipelineNotSupportedOnClientSide, "MessageHandlerFactory")));
            }

            if (!this.CanBuildChannelFactory<TChannel>(context))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("TChannel", SR.GetString(SR.ChannelTypeNotSupported, typeof(TChannel)));
            }

            return (IChannelFactory<TChannel>)(object)new HttpsChannelFactory<TChannel>(this, context);
        }

        public override IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

            HttpChannelListener listener;

            if (!this.CanBuildChannelListener<TChannel>(context))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("TChannel", SR.GetString(SR.ChannelTypeNotSupported, typeof(TChannel)));
            }

            this.UpdateAuthenticationSchemes(context);

            listener = new HttpsChannelListener<TChannel>(this, context);
            AspNetEnvironment.Current.ApplyHostedContext(listener, context);
            return (IChannelListener<TChannel>)(object)listener;
        }

        internal static HttpsTransportBindingElement CreateFromHttpBindingElement(HttpTransportBindingElement elementToBeCloned)
        {
            return new HttpsTransportBindingElement(elementToBeCloned);
        }

        public override T GetProperty<T>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            if (typeof(T) == typeof(ISecurityCapabilities))
            {
                AuthenticationSchemes effectiveAuthenticationSchemes = HttpTransportBindingElement.GetEffectiveAuthenticationSchemes(this.AuthenticationScheme,
                    context.BindingParameters);

                return (T)(object)new SecurityCapabilities(this.GetSupportsClientAuthenticationImpl(effectiveAuthenticationSchemes),
                    true,
                    this.GetSupportsClientWindowsIdentityImpl(effectiveAuthenticationSchemes),
                    ProtectionLevel.EncryptAndSign,
                    ProtectionLevel.EncryptAndSign);
            }
            else
            {
                return base.GetProperty<T>(context);
            }
        }

        internal override void OnExportPolicy(MetadataExporter exporter, PolicyConversionContext context)
        {
            base.OnExportPolicy(exporter, context);
            SecurityBindingElement.ExportPolicyForTransportTokenAssertionProviders(exporter, context);
        }


        internal override void OnImportPolicy(MetadataImporter importer, PolicyConversionContext policyContext)
        {
            base.OnImportPolicy(importer, policyContext);

            WSSecurityPolicy sp = null;
            if (WSSecurityPolicy.TryGetSecurityPolicyDriver(policyContext.GetBindingAssertions(), out sp))
                sp.TryImportWsspHttpsTokenAssertion(importer, policyContext.GetBindingAssertions(), this);
        }

        #region ITransportTokenAssertionProvider Members

        public XmlElement GetTransportTokenAssertion()
        {
            return null;
        }

        #endregion
    }
}
