//------------------------------------------------------------------------------
// <copyright file="ConfigurationSection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Configuration {
    using System;
    using System.Collections.Specialized;
    using System.Configuration.Internal;
    using System.IO;
    using System.Reflection;
    using System.Security;
    using System.Text;
    using System.Xml;
    using System.Globalization;
    using System.Runtime.Versioning;

    public abstract class ConfigurationSection : ConfigurationElement {

        private SectionInformation _section;

        // Constructor
        //
        protected ConfigurationSection() {
            _section = new SectionInformation( this );
        }

        // SectionInformation property
        //
        // Retrieve the class associated with the Section information
        //
        public SectionInformation SectionInformation {
            get {
                return _section;
            }
        }

        // GetRuntimeObject
        //
        // Return the Runtime Object for this Section
        //
        protected internal virtual object GetRuntimeObject() {
            return this;
        }


        protected internal override bool IsModified() {
            return ( SectionInformation.IsModifiedFlags() ||
                     base.IsModified() );
        }

        protected internal override void ResetModified() {
            SectionInformation.ResetModifiedFlags();
            base.ResetModified();
        }

        protected internal virtual void DeserializeSection(XmlReader reader) {
            if (!reader.Read() || reader.NodeType != XmlNodeType.Element) {
                throw new ConfigurationErrorsException(SR.GetString(SR.Config_base_expected_to_find_element), reader);
            }
            DeserializeElement(reader, false);
        }

        protected internal virtual string SerializeSection(ConfigurationElement parentElement, string name, ConfigurationSaveMode saveMode) {
            if (CurrentConfiguration != null &&
                CurrentConfiguration.TargetFramework != null &&
                !ShouldSerializeSectionInTargetVersion(CurrentConfiguration.TargetFramework))
            {
                return string.Empty;
            }

            ValidateElement(this, null, true);

            ConfigurationElement TempElement = CreateElement(this.GetType());
            TempElement.Unmerge(this, parentElement, saveMode);

            StringWriter strWriter = new StringWriter(CultureInfo.InvariantCulture);
            XmlTextWriter writer = new XmlTextWriter(strWriter);
            writer.Formatting = Formatting.Indented;
            writer.Indentation = 4;
            writer.IndentChar = ' ';
            TempElement.DataToWriteInternal = (saveMode != ConfigurationSaveMode.Minimal);

            if (CurrentConfiguration != null && CurrentConfiguration.TargetFramework != null)
                _configRecord.SectionsStack.Push(this);

            TempElement.SerializeToXmlElement(writer, name);

            if (CurrentConfiguration != null && CurrentConfiguration.TargetFramework != null)
                _configRecord.SectionsStack.Pop();

            writer.Flush();
            return strWriter.ToString();
        }

        protected internal virtual bool ShouldSerializePropertyInTargetVersion(ConfigurationProperty property, string propertyName, FrameworkName targetFramework, ConfigurationElement parentConfigurationElement) {
            return true;
        }

        protected internal virtual bool ShouldSerializeElementInTargetVersion(ConfigurationElement element, string elementName, FrameworkName targetFramework) {
            return true;
        }

        protected internal virtual bool ShouldSerializeSectionInTargetVersion(FrameworkName targetFramework) {
            return true;
        }
    }
}
