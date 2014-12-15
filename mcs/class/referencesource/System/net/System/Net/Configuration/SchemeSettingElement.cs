using System;
using System.Diagnostics;

namespace System.Configuration
{
    public sealed class SchemeSettingElement : ConfigurationElement
    {
        private static readonly ConfigurationPropertyCollection properties;
        private static readonly ConfigurationProperty name;
        private static readonly ConfigurationProperty genericUriParserOptions;

        static SchemeSettingElement()
        {
            name = new ConfigurationProperty(CommonConfigurationStrings.SchemeName, typeof(string), null,
                ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey);

            genericUriParserOptions = new ConfigurationProperty(CommonConfigurationStrings.GenericUriParserOptions,
                typeof(GenericUriParserOptions), GenericUriParserOptions.Default, 
                ConfigurationPropertyOptions.IsRequired);

            properties = new ConfigurationPropertyCollection();
            properties.Add(name);
            properties.Add(genericUriParserOptions);
        }

        [ConfigurationProperty(CommonConfigurationStrings.SchemeName,
            DefaultValue = null, IsRequired = true, IsKey = true)]
        public string Name
        {
            get { return (string)this[name]; }
        }

        [ConfigurationProperty(CommonConfigurationStrings.GenericUriParserOptions,
            DefaultValue = ConfigurationPropertyOptions.None, IsRequired = true)]
        public GenericUriParserOptions GenericUriParserOptions
        {
            get { return (GenericUriParserOptions)this[genericUriParserOptions]; }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get { return properties; }
        }
    }

    internal sealed class SchemeSettingInternal
    {
        private string name;
        private GenericUriParserOptions options;

        public SchemeSettingInternal(string name, GenericUriParserOptions options)
        {
            Debug.Assert(name != null, "'name' must not be null.");

            this.name = name.ToLowerInvariant();
            this.options = options;
        }

        public string Name
        {
            get { return name; }
        }

        public GenericUriParserOptions Options
        {
            get { return options; }
        }
    }
}
