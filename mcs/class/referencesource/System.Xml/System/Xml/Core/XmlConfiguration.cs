
using System;
using System.Configuration;
using System.Globalization;
using System.Xml;

namespace System.Xml.XmlConfiguration {
    internal static class XmlConfigurationString {
        internal const string XmlReaderSectionName = "xmlReader";
        internal const string XsltSectionName = "xslt";

        internal const string ProhibitDefaultResolverName = "prohibitDefaultResolver";
        internal const string LimitXPathComplexityName = "limitXPathComplexity";
        internal const string EnableMemberAccessForXslCompiledTransformName = "enableMemberAccessForXslCompiledTransform";

        internal const string XmlConfigurationSectionName = "system.xml";

        internal static string XmlReaderSectionPath = string.Format(CultureInfo.InvariantCulture, @"{0}/{1}", XmlConfigurationSectionName, XmlReaderSectionName);
        internal static string XsltSectionPath = string.Format(CultureInfo.InvariantCulture, @"{0}/{1}", XmlConfigurationSectionName, XsltSectionName);
    }

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public sealed class XmlReaderSection : ConfigurationSection {
        [ConfigurationProperty(XmlConfigurationString.ProhibitDefaultResolverName, DefaultValue = "false")]
        public string ProhibitDefaultResolverString {
            get { return (string)this[XmlConfigurationString.ProhibitDefaultResolverName]; }
            set { this[XmlConfigurationString.ProhibitDefaultResolverName] = value; }
        }

        private bool _ProhibitDefaultResolver {
            get {
                string value = ProhibitDefaultResolverString;
                bool result;
                XmlConvert.TryToBoolean(value, out result);
                return result;
            }
        }

        //check the config every time, otherwise will have problem in different asp.net pages which have different settings.
        //ConfigurationManager will cache the section result, so expect no perf issue.
        internal static bool ProhibitDefaultUrlResolver {
            get {
                XmlReaderSection section = System.Configuration.ConfigurationManager.GetSection(XmlConfigurationString.XmlReaderSectionPath) as XmlReaderSection;
                return (section != null) ? section._ProhibitDefaultResolver : false;
            }
        }

        internal static XmlResolver CreateDefaultResolver() {
                if (ProhibitDefaultUrlResolver)
                    return null;
                else
                    return new XmlUrlResolver();
        }
    }

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public sealed class XsltConfigSection : ConfigurationSection {
        [ConfigurationProperty(XmlConfigurationString.ProhibitDefaultResolverName, DefaultValue = "false")]
        public string ProhibitDefaultResolverString {
            get { return (string)this[XmlConfigurationString.ProhibitDefaultResolverName]; }
            set { this[XmlConfigurationString.ProhibitDefaultResolverName] = value; }
        }

        private bool _ProhibitDefaultResolver {
            get {
                string value = ProhibitDefaultResolverString;
                bool result;
                XmlConvert.TryToBoolean(value, out result);
                return result;
            }
        }

        private static bool s_ProhibitDefaultUrlResolver {
            get {
                XsltConfigSection section = System.Configuration.ConfigurationManager.GetSection(XmlConfigurationString.XsltSectionPath) as XsltConfigSection;
                return (section != null) ? section._ProhibitDefaultResolver : false;
            }
        }

        internal static XmlResolver CreateDefaultResolver() {
                if (s_ProhibitDefaultUrlResolver)
                    return XmlNullResolver.Singleton;
                else
                    return new XmlUrlResolver();
        }

        [ConfigurationProperty(XmlConfigurationString.LimitXPathComplexityName, DefaultValue = "true")]
        internal string LimitXPathComplexityString
        {
            get { return (string)this[XmlConfigurationString.LimitXPathComplexityName]; }
            set { this[XmlConfigurationString.LimitXPathComplexityName] = value; }
        }

        private bool _LimitXPathComplexity
        {
            get
            {
                string value = LimitXPathComplexityString;
                bool result = true;
                XmlConvert.TryToBoolean(value, out result);
                return result;
            }
        }

        internal static bool LimitXPathComplexity
        {
            get
            {
                XsltConfigSection section = System.Configuration.ConfigurationManager.GetSection(XmlConfigurationString.XsltSectionPath) as XsltConfigSection;
                return (section != null) ? section._LimitXPathComplexity : true;
            }
        }

        [ConfigurationProperty(XmlConfigurationString.EnableMemberAccessForXslCompiledTransformName, DefaultValue = "False")]
        internal string EnableMemberAccessForXslCompiledTransformString
        {
            get { return (string)this[XmlConfigurationString.EnableMemberAccessForXslCompiledTransformName]; }
            set { this[XmlConfigurationString.EnableMemberAccessForXslCompiledTransformName] = value; }
        }

        private bool _EnableMemberAccessForXslCompiledTransform
        {
            get
            {
                string value = EnableMemberAccessForXslCompiledTransformString;
                bool result = false;
                XmlConvert.TryToBoolean(value, out result);
                return result;
            }
        }

        internal static bool EnableMemberAccessForXslCompiledTransform
        {
            get
            {
                XsltConfigSection section = System.Configuration.ConfigurationManager.GetSection(XmlConfigurationString.XsltSectionPath) as XsltConfigSection;
                return (section != null) ? section._EnableMemberAccessForXslCompiledTransform : false;
            }
        }
    }
}
