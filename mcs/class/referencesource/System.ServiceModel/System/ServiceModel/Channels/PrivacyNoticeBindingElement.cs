//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.ServiceModel.Configuration;
    using System.ServiceModel.Description;
    using System.Xml;


    public sealed class PrivacyNoticeBindingElement : BindingElement, IPolicyExportExtension
    {
        Uri url;
        int version;

        public PrivacyNoticeBindingElement()
        {
            this.url = null;
        }

        public PrivacyNoticeBindingElement(PrivacyNoticeBindingElement elementToBeCloned)
            : base(elementToBeCloned)
        {
            this.url = elementToBeCloned.url;
            this.version = elementToBeCloned.version;
        }

        public Uri Url
        {
            get
            {
                return this.url;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }

                this.url = value;
            }
        }

        public int Version
        {
            get
            {
                return this.version;
            }
            set
            {
                if (value < 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value,
                                                                        SR.GetString(SR.ValueMustBePositive)));
                }
                this.version = value;
            }
        }

        public override BindingElement Clone()
        {
            return new PrivacyNoticeBindingElement(this);
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
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");

            if (context.BindingElements != null)
            {
                PrivacyNoticeBindingElement settings =
                    context.BindingElements.Find<PrivacyNoticeBindingElement>();

                if (settings != null)
                {
                    XmlDocument doc = new XmlDocument();

                    // PrivacyNotice assertion
                    XmlElement assertion = doc.CreateElement(PrivacyNoticePolicyStrings.PrivacyNoticePrefix,
                                                              PrivacyNoticePolicyStrings.PrivacyNoticeName,
                                                              PrivacyNoticePolicyStrings.PrivacyNoticeNamespace);

                    assertion.InnerText = settings.Url.ToString();
                    assertion.SetAttribute(PrivacyNoticePolicyStrings.PrivacyNoticeVersionAttributeName, PrivacyNoticePolicyStrings.PrivacyNoticeNamespace, XmlConvert.ToString(settings.Version));

                    context.GetBindingAssertions().Add(assertion);
                }
            }
        }

        internal override bool IsMatch(BindingElement b)
        {
            if (b == null)
                return false;
            PrivacyNoticeBindingElement privacy = b as PrivacyNoticeBindingElement;
            if (privacy == null)
                return false;
            return (this.url == privacy.url && this.version == privacy.version);
        }
    }
}
