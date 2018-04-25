using System;
using System.Globalization;
using System.Xml;

#if CONFIGURATION_DEP
using System.Configuration;
#endif

namespace System.Xml.XmlConfiguration {
    internal static class XmlConfigurationString {
        internal const string XmlReaderSectionName = "xmlReader";
        internal const string XsltSectionName = "xslt";

        internal const string ProhibitDefaultResolverName = "prohibitDefaultResolver";
        internal const string LimitXPathComplexityName = "limitXPathComplexity";
        internal const string EnableMemberAccessForXslCompiledTransformName = "enableMemberAccessForXslCompiledTransform";
        internal const string CollapseWhiteSpaceIntoEmptyStringName = "CollapseWhiteSpaceIntoEmptyString";

        internal const string XmlConfigurationSectionName = "system.xml";

        internal static string XmlReaderSectionPath = string.Format(CultureInfo.InvariantCulture, @"{0}/{1}", XmlConfigurationSectionName, XmlReaderSectionName);
        internal static string XsltSectionPath = string.Format(CultureInfo.InvariantCulture, @"{0}/{1}", XmlConfigurationSectionName, XsltSectionName);
    }

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public sealed class XmlReaderSection
#if CONFIGURATION_DEP
		: ConfigurationSection
#endif
	{
#if CONFIGURATION_DEP
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
#endif
        //check the config every time, otherwise will have problem in different asp.net pages which have different settings.
        //ConfigurationManager will cache the section result, so expect no perf issue.
        internal static bool ProhibitDefaultUrlResolver {
            get {
#if CONFIGURATION_DEP
                XmlReaderSection section = System.Configuration.ConfigurationManager.GetSection(XmlConfigurationString.XmlReaderSectionPath) as XmlReaderSection;
                return (section != null) ? section._ProhibitDefaultResolver : false;
#else
			return false;
#endif
            }
        }

        internal static XmlResolver CreateDefaultResolver() {
                if (ProhibitDefaultUrlResolver)
                    return null;
                else
                    return new XmlUrlResolver();
        }

#if CONFIGURATION_DEP
        [ConfigurationProperty(XmlConfigurationString.CollapseWhiteSpaceIntoEmptyStringName, DefaultValue = "false")]
#endif
        public string CollapseWhiteSpaceIntoEmptyStringString {
            get {
#if CONFIGURATION_DEP
                return (string)this[XmlConfigurationString.CollapseWhiteSpaceIntoEmptyStringName];
#else
                return null;
#endif
            }
            set {
#if CONFIGURATION_DEP
                this[XmlConfigurationString.CollapseWhiteSpaceIntoEmptyStringName] = value;
#endif
            }
        }

        private bool _CollapseWhiteSpaceIntoEmptyString {
            get {
                string value = CollapseWhiteSpaceIntoEmptyStringString;
                bool result;
                XmlConvert.TryToBoolean(value, out result);
                return result;
            }
        }

        //check the config every time, otherwise will have problem in different asp.net pages which have different settings.
        //ConfigurationManager will cache the section result, so expect no perf issue.
        internal static bool CollapseWhiteSpaceIntoEmptyString {
            get {
#if CONFIGURATION_DEP
                XmlReaderSection section = System.Configuration.ConfigurationManager.GetSection(XmlConfigurationString.XmlReaderSectionPath) as XmlReaderSection;
                return (section != null) ? section._CollapseWhiteSpaceIntoEmptyString : false;
#else
                return false;
#endif
            }
        }
    }

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public sealed class XsltConfigSection
#if CONFIGURATION_DEP
		: ConfigurationSection
#endif
	{
#if CONFIGURATION_DEP
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
#endif
        private static bool s_ProhibitDefaultUrlResolver {
            get {
#if CONFIGURATION_DEP
                XsltConfigSection section = System.Configuration.ConfigurationManager.GetSection(XmlConfigurationString.XsltSectionPath) as XsltConfigSection;
                return (section != null) ? section._ProhibitDefaultResolver : false;
#else
			return false;
#endif
            }
        }

        internal static XmlResolver CreateDefaultResolver() {
                if (s_ProhibitDefaultUrlResolver)
                    return XmlNullResolver.Singleton;
                else
                    return new XmlUrlResolver();
        }
#if CONFIGURATION_DEP
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
#endif
        internal static bool LimitXPathComplexity
        {
            get
            {
#if CONFIGURATION_DEP
                XsltConfigSection section = System.Configuration.ConfigurationManager.GetSection(XmlConfigurationString.XsltSectionPath) as XsltConfigSection;
                return (section != null) ? section._LimitXPathComplexity : true;
#else
				return true;
#endif
            }
        }
#if CONFIGURATION_DEP
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
#endif
        internal static bool EnableMemberAccessForXslCompiledTransform
        {
            get
            {
#if CONFIGURATION_DEP
                XsltConfigSection section = System.Configuration.ConfigurationManager.GetSection(XmlConfigurationString.XsltSectionPath) as XsltConfigSection;
                return (section != null) ? section._EnableMemberAccessForXslCompiledTransform : false;
#else
				return false;
#endif
            }
        }
    }
}
