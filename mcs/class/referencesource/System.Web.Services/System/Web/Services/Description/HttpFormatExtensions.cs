namespace System.Web.Services.Description {
    using System.Xml.Serialization;
    using System.Web.Services.Configuration;

    /// <include file='doc\HttpFormatExtensions.uex' path='docs/doc[@for="HttpAddressBinding"]/*' />
    [XmlFormatExtension("address", HttpBinding.Namespace, typeof(Port))]
    public sealed class HttpAddressBinding : ServiceDescriptionFormatExtension {
        string location;

        /// <include file='doc\HttpFormatExtensions.uex' path='docs/doc[@for="HttpAddressBinding.Location"]/*' />
        [XmlAttribute("location")]
        public string Location {
            get { return location == null ? string.Empty : location; }
            set { location = value; }
        }
    }

    /// <include file='doc\HttpFormatExtensions.uex' path='docs/doc[@for="HttpBinding"]/*' />
    [XmlFormatExtension("binding", HttpBinding.Namespace, typeof(Binding))]
    [XmlFormatExtensionPrefix("http", HttpBinding.Namespace)]
    public sealed class HttpBinding : ServiceDescriptionFormatExtension {
        string verb;

        /// <include file='doc\HttpFormatExtensions.uex' path='docs/doc[@for="HttpBinding.Namespace"]/*' />
        public const string Namespace = "http://schemas.xmlsoap.org/wsdl/http/";

        /// <include file='doc\HttpFormatExtensions.uex' path='docs/doc[@for="HttpBinding.Verb"]/*' />
        [XmlAttribute("verb")]
        public string Verb {
            get { return verb; }
            set { verb = value; }
        }
    }

    /// <include file='doc\HttpFormatExtensions.uex' path='docs/doc[@for="HttpOperationBinding"]/*' />
    [XmlFormatExtension("operation", HttpBinding.Namespace, typeof(OperationBinding))]
    public sealed class HttpOperationBinding : ServiceDescriptionFormatExtension {
        string location;

        /// <include file='doc\HttpFormatExtensions.uex' path='docs/doc[@for="HttpOperationBinding.Location"]/*' />
        [XmlAttribute("location")]
        public string Location {
            get { return location == null ? string.Empty : location; }
            set { location = value; }
        }
    }

    /// <include file='doc\HttpFormatExtensions.uex' path='docs/doc[@for="HttpUrlEncodedBinding"]/*' />
    [XmlFormatExtension("urlEncoded", HttpBinding.Namespace, typeof(InputBinding))]
    public sealed class HttpUrlEncodedBinding : ServiceDescriptionFormatExtension {
    }

    /// <include file='doc\HttpFormatExtensions.uex' path='docs/doc[@for="HttpUrlReplacementBinding"]/*' />
    [XmlFormatExtension("urlReplacement", HttpBinding.Namespace, typeof(InputBinding))]
    public sealed class HttpUrlReplacementBinding : ServiceDescriptionFormatExtension {
    }
}
