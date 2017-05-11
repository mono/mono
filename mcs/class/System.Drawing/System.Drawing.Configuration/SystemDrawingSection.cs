using System;
using System.Configuration;

namespace System.Drawing.Configuration
{
    public sealed class SystemDrawingSection : ConfigurationSection
    {
        private const string BitmapSuffixSectionName = "bitmapSuffix";

        private static readonly ConfigurationPropertyCollection properties;

        private static readonly ConfigurationProperty bitmapSuffix;

        [ConfigurationProperty("bitmapSuffix")]
        public string BitmapSuffix
        {
            get
            {
                return (string)base[SystemDrawingSection.bitmapSuffix];
            }
            set
            {
                base[SystemDrawingSection.bitmapSuffix] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return SystemDrawingSection.properties;
            }
        }

        static SystemDrawingSection()
        {
            SystemDrawingSection.properties = new ConfigurationPropertyCollection();
            SystemDrawingSection.bitmapSuffix = new ConfigurationProperty("bitmapSuffix", typeof(string), null, ConfigurationPropertyOptions.None);
            SystemDrawingSection.properties.Add(SystemDrawingSection.bitmapSuffix);
        }
    }
}
