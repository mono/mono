namespace System.Web.Services.Description {
    using System.Xml;
    using System.IO;
    using System.Xml.Schema;
    using System.Xml.Serialization;
    using System.ComponentModel;
    using System.Text;
    using System.Web.Services.Configuration;

    /// <include file='doc\SoapFormatExtensions.uex' path='docs/doc[@for="SoapBinding"]/*' />
    [XmlFormatExtension("binding", SoapBinding.Namespace, typeof(Binding))]
    [XmlFormatExtensionPrefix("soap", SoapBinding.Namespace)]
    [XmlFormatExtensionPrefix("soapenc", "http://schemas.xmlsoap.org/soap/encoding/")]
    public class SoapBinding : ServiceDescriptionFormatExtension {
        SoapBindingStyle style = SoapBindingStyle.Document;
        string transport;
        static XmlSchema schema = null;

        /// <include file='doc\SoapFormatExtensions.uex' path='docs/doc[@for="SoapBinding.Namespace"]/*' />
        public const string Namespace = "http://schemas.xmlsoap.org/wsdl/soap/";
        /// <include file='doc\SoapFormatExtensions.uex' path='docs/doc[@for="SoapBinding.HttpTransport"]/*' />
        public const string HttpTransport = "http://schemas.xmlsoap.org/soap/http";

        /// <include file='doc\SoapFormatExtensions.uex' path='docs/doc[@for="SoapBinding.Transport"]/*' />
        [XmlAttribute("transport")]
        public string Transport {
            get { return transport == null ? string.Empty : transport; }
            set { transport = value; }
        }

        /// <include file='doc\SoapFormatExtensions.uex' path='docs/doc[@for="SoapBinding.Style"]/*' />
        [XmlAttribute("style"), DefaultValue(SoapBindingStyle.Document)]
        public SoapBindingStyle Style {
            get { return style; }
            set { style = value; }
        }

        /// <include file='doc\SoapFormatExtensions.uex' path='docs/doc[@for="SoapFormatExtensions.Schema"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static XmlSchema Schema {
            get {
                if (schema == null) {
                    using (XmlTextReader reader = new XmlTextReader(new StringReader(Schemas.Soap)))
                    {
                        reader.DtdProcessing = DtdProcessing.Ignore;
                        schema = XmlSchema.Read(reader, null);
                    }
                }
                return schema;
            }
        }
    }

    /// <include file='doc\SoapFormatExtensions.uex' path='docs/doc[@for="SoapBindingStyle"]/*' />
    public enum SoapBindingStyle {
        /// <include file='doc\SoapFormatExtensions.uex' path='docs/doc[@for="SoapBindingStyle.Default"]/*' />
        [XmlIgnore]
        Default,
        /// <include file='doc\SoapFormatExtensions.uex' path='docs/doc[@for="SoapBindingStyle.Document"]/*' />
        [XmlEnum("document")]
        Document,
        /// <include file='doc\SoapFormatExtensions.uex' path='docs/doc[@for="SoapBindingStyle.Rpc"]/*' />
        [XmlEnum("rpc")]
        Rpc,
    }

    /// <include file='doc\SoapFormatExtensions.uex' path='docs/doc[@for="SoapOperationBinding"]/*' />
    [XmlFormatExtension("operation", SoapBinding.Namespace, typeof(OperationBinding))]
    public class SoapOperationBinding : ServiceDescriptionFormatExtension {
        string soapAction;
        SoapBindingStyle style;

        /// <include file='doc\SoapFormatExtensions.uex' path='docs/doc[@for="SoapOperationBinding.SoapAction"]/*' />
        [XmlAttribute("soapAction")]
        public string SoapAction {
            get { return soapAction == null ? string.Empty : soapAction; }
            set { soapAction = value; }
        }

        /// <include file='doc\SoapFormatExtensions.uex' path='docs/doc[@for="SoapOperationBinding.Style"]/*' />
        [XmlAttribute("style"), DefaultValue(SoapBindingStyle.Default)]
        public SoapBindingStyle Style {
            get { return style; }
            set { style = value; }
        }
    }

    /// <include file='doc\SoapFormatExtensions.uex' path='docs/doc[@for="SoapBodyBinding"]/*' />
    [XmlFormatExtension("body", SoapBinding.Namespace, typeof(InputBinding), typeof(OutputBinding), typeof(MimePart))]
    public class SoapBodyBinding : ServiceDescriptionFormatExtension {
        SoapBindingUse use;
        string ns;
        string encoding;
        string[] parts;

        /// <include file='doc\SoapFormatExtensions.uex' path='docs/doc[@for="SoapBodyBinding.Use"]/*' />
        [XmlAttribute("use"), DefaultValue(SoapBindingUse.Default)]
        public SoapBindingUse Use {
            get { return use; }
            set { use = value; }
        }

        /// <include file='doc\SoapFormatExtensions.uex' path='docs/doc[@for="SoapBodyBinding.Namespace"]/*' />
        [XmlAttribute("namespace"), DefaultValue("")]
        public string Namespace {
            get { return ns == null ? string.Empty : ns; }
            set { ns = value; }
        }

        /// <include file='doc\SoapFormatExtensions.uex' path='docs/doc[@for="SoapBodyBinding.Encoding"]/*' />
        [XmlAttribute("encodingStyle"), DefaultValue("")]
        public string Encoding {
            get { return encoding == null ? string.Empty : encoding; }
            set { encoding = value; }
        }

        /// <include file='doc\SoapFormatExtensions.uex' path='docs/doc[@for="SoapBodyBinding.PartsString"]/*' />
        [XmlAttribute("parts")]
        public string PartsString {
            get { 
                if (parts == null) 
                    return null;
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < parts.Length; i++) {
                    if (i > 0) builder.Append(' ');
                    builder.Append(parts[i]);
                }
                return builder.ToString(); 
            }
            set {
                if (value == null)
                    parts = null;
                else
                    parts = value.Split(new char[] { ' ' });
            }
        }

        /// <include file='doc\SoapFormatExtensions.uex' path='docs/doc[@for="SoapBodyBinding.Parts"]/*' />
        [XmlIgnore]
        public string[] Parts {
            get { return parts; }
            set { parts = value; }
        }
    }

    /// <include file='doc\SoapFormatExtensions.uex' path='docs/doc[@for="SoapBindingUse"]/*' />
    public enum SoapBindingUse {
        /// <include file='doc\SoapFormatExtensions.uex' path='docs/doc[@for="SoapBindingUse.Default"]/*' />
        [XmlIgnore]
        Default,
        /// <include file='doc\SoapFormatExtensions.uex' path='docs/doc[@for="SoapBindingUse.Encoded"]/*' />
        [XmlEnum("encoded")]
        Encoded,
        /// <include file='doc\SoapFormatExtensions.uex' path='docs/doc[@for="SoapBindingUse.Literal"]/*' />
        [XmlEnum("literal")]
        Literal,
    }

    /// <include file='doc\SoapFormatExtensions.uex' path='docs/doc[@for="SoapFaultBinding"]/*' />
    [XmlFormatExtension("fault", SoapBinding.Namespace, typeof(FaultBinding))]
    public class SoapFaultBinding : ServiceDescriptionFormatExtension {
        SoapBindingUse use;
        string ns;
        string encoding;
        string name;

        /// <include file='doc\SoapFormatExtensions.uex' path='docs/doc[@for="SoapFaultBinding.Use"]/*' />
        [XmlAttribute("use"), DefaultValue(SoapBindingUse.Default)]
        public SoapBindingUse Use {
            get { return use; }
            set { use = value; }
        }

        /// <include file='doc\SoapFormatExtensions.uex' path='docs/doc[@for="SoapFaultBinding.Use"]/*' />
        [XmlAttribute("name")]
        public string Name {
            get { return name; }
            set { name = value; }
        }
        /// <include file='doc\SoapFormatExtensions.uex' path='docs/doc[@for="SoapFaultBinding.Namespace"]/*' />
        [XmlAttribute("namespace")]
        public string Namespace {
            get { return ns == null ? string.Empty : ns; }
            set { ns = value; }
        }

        /// <include file='doc\SoapFormatExtensions.uex' path='docs/doc[@for="SoapFaultBinding.Encoding"]/*' />
        [XmlAttribute("encodingStyle"), DefaultValue("")]
        public string Encoding {
            get { return encoding == null ? string.Empty : encoding; }
            set { encoding = value; }
        }
    }

    /// <include file='doc\SoapFormatExtensions.uex' path='docs/doc[@for="SoapHeaderBinding"]/*' />
    [XmlFormatExtension("header", SoapBinding.Namespace, typeof(InputBinding), typeof(OutputBinding))]
    public class SoapHeaderBinding : ServiceDescriptionFormatExtension {
        XmlQualifiedName message = XmlQualifiedName.Empty;
        string part;
        SoapBindingUse use;
        string encoding;
        string ns;
        bool mapToProperty;
        SoapHeaderFaultBinding fault;

        /// <include file='doc\SoapFormatExtensions.uex' path='docs/doc[@for="SoapHeaderBinding.MapToProperty"]/*' />
        [XmlIgnore]
        public bool MapToProperty {
            get { return mapToProperty; }
            set { mapToProperty = value; }
        }

        /// <include file='doc\SoapFormatExtensions.uex' path='docs/doc[@for="SoapHeaderBinding.Message"]/*' />
        [XmlAttribute("message")]
        public XmlQualifiedName Message {
            get { return message; }
            set { message = value; }
        }

        /// <include file='doc\SoapFormatExtensions.uex' path='docs/doc[@for="SoapHeaderBinding.Part"]/*' />
        [XmlAttribute("part")]
        public string Part {
            get { return part; }
            set { part = value; }
        }

        /// <include file='doc\SoapFormatExtensions.uex' path='docs/doc[@for="SoapHeaderBinding.Use"]/*' />
        [XmlAttribute("use"), DefaultValue(SoapBindingUse.Default)]
        public SoapBindingUse Use {
            get { return use; }
            set { use = value; }
        }

        /// <include file='doc\SoapFormatExtensions.uex' path='docs/doc[@for="SoapHeaderBinding.Encoding"]/*' />
        [XmlAttribute("encodingStyle"), DefaultValue("")]
        public string Encoding {
            get { return encoding == null ? string.Empty : encoding; }
            set { encoding = value; }
        }

        /// <include file='doc\SoapFormatExtensions.uex' path='docs/doc[@for="SoapHeaderBinding.Namespace"]/*' />
        [XmlAttribute("namespace"), DefaultValue("")]
        public string Namespace {
            get { return ns == null ? string.Empty : ns; }
            set { ns = value; }
        }
        
        /// <include file='doc\SoapFormatExtensions.uex' path='docs/doc[@for="SoapHeaderBinding.Fault"]/*' />
        [XmlElement("headerfault")]
        public SoapHeaderFaultBinding Fault {
            get { return fault; }
            set { fault = value; }
        }
    }

    /// <include file='doc\SoapFormatExtensions.uex' path='docs/doc[@for="SoapHeaderFaultBinding"]/*' />
    public class SoapHeaderFaultBinding : ServiceDescriptionFormatExtension {
        XmlQualifiedName message = XmlQualifiedName.Empty;
        string part;
        SoapBindingUse use;
        string encoding;
        string ns;

       /// <include file='doc\SoapFormatExtensions.uex' path='docs/doc[@for="SoapHeaderFaultBinding.Message"]/*' />
        [XmlAttribute("message")]
        public XmlQualifiedName Message {
            get { return message; }
            set { message = value; }
        }

       /// <include file='doc\SoapFormatExtensions.uex' path='docs/doc[@for="SoapHeaderFaultBinding.Part"]/*' />
        [XmlAttribute("part")]
        public string Part {
            get { return part; }
            set { part = value; }
        }

        /// <include file='doc\SoapFormatExtensions.uex' path='docs/doc[@for="SoapHeaderFaultBinding.Use"]/*' />
        [XmlAttribute("use"), DefaultValue(SoapBindingUse.Default)]
        public SoapBindingUse Use {
            get { return use; }
            set { use = value; }
        }

        /// <include file='doc\SoapFormatExtensions.uex' path='docs/doc[@for="SoapHeaderFaultBinding.Encoding"]/*' />
        [XmlAttribute("encodingStyle"), DefaultValue("")]
        public string Encoding {
            get { return encoding == null ? string.Empty : encoding; }
            set { encoding = value; }
        }

        /// <include file='doc\SoapFormatExtensions.uex' path='docs/doc[@for="SoapHeaderFaultBinding.Namespace"]/*' />
        [XmlAttribute("namespace"), DefaultValue("")]
        public string Namespace {
            get { return ns == null ? string.Empty : ns; }
            set { ns = value; }
        }
    }

    /// <include file='doc\SoapFormatExtensions.uex' path='docs/doc[@for="SoapAddressBinding"]/*' />
    [XmlFormatExtension("address", SoapBinding.Namespace, typeof(Port))]
    public class SoapAddressBinding : ServiceDescriptionFormatExtension {
        string location;

        /// <include file='doc\SoapFormatExtensions.uex' path='docs/doc[@for="SoapAddressBinding.Location"]/*' />
        [XmlAttribute("location")]
        public string Location {
            get { return location == null ? string.Empty : location; }
            set { location = value; }
        }
    }
}
