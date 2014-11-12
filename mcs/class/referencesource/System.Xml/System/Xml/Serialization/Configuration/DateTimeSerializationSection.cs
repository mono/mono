//------------------------------------------------------------------------------
// <copyright file="DateTimeSerializationSection.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>                                                                
//------------------------------------------------------------------------------

namespace System.Xml.Serialization.Configuration
{
    using System;
    using System.Configuration;
    using System.ComponentModel;
    using System.Globalization;
    using System.Reflection;

    public sealed class DateTimeSerializationSection : ConfigurationSection
    {
        public enum DateTimeSerializationMode
        {
            Default = 0,
            Roundtrip = 1,
            Local = 2,
        }

        public DateTimeSerializationSection()
        {
            this.properties.Add(this.mode);
        }

        protected override ConfigurationPropertyCollection Properties 
        {
            get 
            {
                return this.properties;
            }
        }

        [ConfigurationProperty(ConfigurationStrings.Mode, DefaultValue=DateTimeSerializationMode.Roundtrip)]
        public DateTimeSerializationMode Mode
        {
            get { return (DateTimeSerializationMode) this[this.mode]; }
            set { this[this.mode] = value; }
        }

        ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();

        // Supply a type converter, even though it's a plain type converter, to get around ConfigurationProperty's internal
        // Enum conversion routine.  The internal one is case-sensitive, we want this to be case-insensitive.
        readonly ConfigurationProperty mode =
            new ConfigurationProperty(ConfigurationStrings.Mode, typeof(DateTimeSerializationMode), DateTimeSerializationMode.Roundtrip,
                    new EnumConverter(typeof(DateTimeSerializationMode)), null, ConfigurationPropertyOptions.None);

    }

}

