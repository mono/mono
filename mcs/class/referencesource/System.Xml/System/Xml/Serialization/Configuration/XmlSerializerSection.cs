namespace System.Xml.Serialization.Configuration
{
    using System;
    using System.IO;
    using System.Web;
    using System.Configuration;
    using System.ComponentModel;
    using System.Globalization;
    using System.Reflection;
    using System.Resources;

    public sealed class XmlSerializerSection : ConfigurationSection
    {
        public XmlSerializerSection()
        {
            this.properties.Add(this.checkDeserializeAdvances);
            this.properties.Add(this.tempFilesLocation);
            this.properties.Add(this.useLegacySerializerGeneration);
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return this.properties;
            }
        }

        [ConfigurationProperty(ConfigurationStrings.CheckDeserializeAdvances, DefaultValue = false)]
        public bool CheckDeserializeAdvances
        {
            get { return (bool)this[this.checkDeserializeAdvances]; }
            set { this[this.checkDeserializeAdvances] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.TempFilesLocation, DefaultValue = null)]
        public string TempFilesLocation
        {
            get { return (string)this[this.tempFilesLocation]; }
            set { this[this.tempFilesLocation] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.UseLegacySerializerGeneration, DefaultValue = false)]
        public bool UseLegacySerializerGeneration
        {
            get { return (bool)this[this.useLegacySerializerGeneration]; }
            set { this[this.useLegacySerializerGeneration] = value; }
        }

        ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();

        // Supply a type converter, even though it's a plain type converter, to get around ConfigurationProperty's internal
        // Enum conversion routine.  The internal one is case-sensitive, we want this to be case-insensitive.
        readonly ConfigurationProperty checkDeserializeAdvances =
            new ConfigurationProperty(ConfigurationStrings.CheckDeserializeAdvances, typeof(bool), false,
                    ConfigurationPropertyOptions.None);

        readonly ConfigurationProperty tempFilesLocation =
            new ConfigurationProperty(ConfigurationStrings.TempFilesLocation, typeof(string), null, null,
            new RootedPathValidator(),
            ConfigurationPropertyOptions.None);

        readonly ConfigurationProperty useLegacySerializerGeneration =
            new ConfigurationProperty(ConfigurationStrings.UseLegacySerializerGeneration, typeof(bool), false,
                ConfigurationPropertyOptions.None);
    }


    public class RootedPathValidator : ConfigurationValidatorBase
    {
        public override bool CanValidate(Type type)
        {
            return (type == typeof(string));
        }

        public override void Validate(object value)
        {
            string tempDirectory = value as string;
            if (string.IsNullOrEmpty(tempDirectory))
                return;
            tempDirectory = tempDirectory.Trim();
            if (string.IsNullOrEmpty(tempDirectory))
                return;
            if (!Path.IsPathRooted(tempDirectory))
            {
                // Make sure the path is not relative (VSWhidbey 260075)
                throw new ConfigurationErrorsException();
            }
            char firstChar = tempDirectory[0];
            if (firstChar == Path.DirectorySeparatorChar || firstChar == Path.AltDirectorySeparatorChar)
            {
                // Make sure the path is explicitly rooted
                throw new ConfigurationErrorsException();
            }
        }
    }
}


