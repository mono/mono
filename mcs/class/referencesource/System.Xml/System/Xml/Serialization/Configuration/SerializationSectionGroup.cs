//------------------------------------------------------------------------------
// <copyright file="SerializationSectionGroup.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>                                                                
//------------------------------------------------------------------------------

namespace System.Xml.Serialization.Configuration
{
    using System.Configuration;

    /// <summary>
    /// Summary description for SerializationSectionGroup.
    /// </summary>
    public sealed class SerializationSectionGroup : ConfigurationSectionGroup
    {
        public SerializationSectionGroup() {}

        // public properties
        [ConfigurationProperty(ConfigurationStrings.SchemaImporterExtensionsSectionName)]
        public SchemaImporterExtensionsSection SchemaImporterExtensions
        {
            get { return (SchemaImporterExtensionsSection)Sections[ConfigurationStrings.SchemaImporterExtensionsSectionName]; }
        }

        [ConfigurationProperty(ConfigurationStrings.DateTimeSerializationSectionName)]
        public DateTimeSerializationSection DateTimeSerialization 
        {
            get { return (DateTimeSerializationSection)Sections[ConfigurationStrings.DateTimeSerializationSectionName]; }
        }
        
        public XmlSerializerSection XmlSerializer 
        {
            get { return (XmlSerializerSection)Sections[ConfigurationStrings.XmlSerializerSectionName]; }
        }
    }
}
