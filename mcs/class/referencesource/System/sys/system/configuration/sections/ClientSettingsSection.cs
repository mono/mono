//------------------------------------------------------------------------------
// <copyright file="ClientSettingsSection.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Configuration
{
    using System;
    using System.Xml;
    using System.Configuration;
    using System.Collections.Specialized;
    using System.Collections;
    using System.IO;
    using System.Text;
     
    /// <devdoc>
    ///     ConfigurationSection class for sections that store client settings. 
    /// </devdoc>
    public sealed class ClientSettingsSection : ConfigurationSection
    {
        private static ConfigurationPropertyCollection _properties;
        private static readonly ConfigurationProperty _propSettings = new ConfigurationProperty(null, typeof(SettingElementCollection), null, ConfigurationPropertyOptions.IsDefaultCollection);

        static ClientSettingsSection () {
            _properties = new ConfigurationPropertyCollection();
            _properties.Add(_propSettings);
        }
        
        public ClientSettingsSection () {
        }
         
        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }
         
        /// <include file='doc\ClientSettingsSection.uex' path='docs/doc[@for="ClientSettingsSection.Settings]/*' />
        [ConfigurationProperty("", IsDefaultCollection = true)]
        public SettingElementCollection Settings {
            get {
                return (SettingElementCollection) base[_propSettings];
            }
        }
    } 
     
    public sealed class SettingElementCollection : ConfigurationElementCollection {
        public override ConfigurationElementCollectionType CollectionType {
            get {
                return ConfigurationElementCollectionType.BasicMap;
            }
        }

        protected override string ElementName {
            get {
                return "setting";
            }
        }

        protected override ConfigurationElement CreateNewElement() {
            return new SettingElement();
        }

        protected override object GetElementKey(ConfigurationElement element) {
            return ((SettingElement)element).Key;
        }

        public SettingElement Get(string elementKey) {
            return (SettingElement) BaseGet(elementKey);
        }

        public void Add(SettingElement element) {
            BaseAdd(element);
        }

        public void Remove(SettingElement element) {
            BaseRemove(GetElementKey(element));
        }

        public void Clear() {
            BaseClear();
        }
    } 

    public sealed class SettingElement : ConfigurationElement {
        private static ConfigurationPropertyCollection _properties;
        private static readonly ConfigurationProperty _propName = new ConfigurationProperty("name", typeof(string),"",ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey);
        private static readonly ConfigurationProperty _propSerializeAs = new ConfigurationProperty("serializeAs", typeof(SettingsSerializeAs),SettingsSerializeAs.String,ConfigurationPropertyOptions.IsRequired);
        private static readonly ConfigurationProperty _propValue = new ConfigurationProperty("value", typeof(SettingValueElement),null,ConfigurationPropertyOptions.IsRequired);
        private static XmlDocument doc = new XmlDocument();

        static SettingElement() {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();

            _properties.Add(_propName);
            _properties.Add(_propSerializeAs);
            _properties.Add(_propValue);
            
        }
        
        public SettingElement() {
        }
        
        public SettingElement(String name, SettingsSerializeAs serializeAs) : this() {
            Name = name;
            SerializeAs = serializeAs;
        }

        internal string Key {
            get {
                return Name;
            }
        }
        
        public override bool Equals(object settings) {
            SettingElement u = settings as SettingElement;
            return (u != null && base.Equals(settings) && Object.Equals(u.Value, Value));
        }

        public override int GetHashCode() {
            return base.GetHashCode() ^ Value.GetHashCode(); 
        }
         
         
        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }
         
        [ConfigurationProperty("name", IsRequired = true, IsKey = true, DefaultValue = "")]
        public string Name {
            get {
                return (string)base[_propName];
            }
            set {
                base[_propName] = value;
            }
        }
         
        [ConfigurationProperty("serializeAs", IsRequired = true, DefaultValue = SettingsSerializeAs.String)]
        public SettingsSerializeAs SerializeAs {
            get {
                return (SettingsSerializeAs) base[_propSerializeAs];
            }
            set {
                base[_propSerializeAs] = value;
            }
        }

        [ConfigurationProperty("value", IsRequired = true, DefaultValue = null)]
        public SettingValueElement Value {
            get {
                return (SettingValueElement) base[_propValue];
            }
            set {
                base[_propValue] = value;
            }
        }
    } 

    public sealed class SettingValueElement : ConfigurationElement {
        private static volatile ConfigurationPropertyCollection _properties;
        private static XmlDocument doc = new XmlDocument();

        private XmlNode _valueXml;
        private bool isModified = false;
         
        protected override ConfigurationPropertyCollection Properties {
            get {
                if (_properties == null) {
                    _properties = new ConfigurationPropertyCollection();
                }

                return _properties;
            }
        }
         
        public XmlNode ValueXml {
            get {
                return _valueXml;
            }
            set {
                _valueXml = value;
                isModified = true;
            }
        }

        protected override void DeserializeElement(XmlReader reader, bool serializeCollectionKey)
        {
            ValueXml = doc.ReadNode(reader);
        }

        public override bool Equals(object settingValue) {
            SettingValueElement u = settingValue as SettingValueElement;
            return (u != null && Object.Equals(u.ValueXml, ValueXml)); 
        }

        public override int GetHashCode() {
            return ValueXml.GetHashCode();
        }

        protected override bool IsModified() {
            return isModified;
        }

        protected override void ResetModified() {
            isModified = false;
        }

        protected override bool SerializeToXmlElement(XmlWriter writer, string elementName) {
            if (ValueXml != null) {
                if (writer != null) {
                    ValueXml.WriteTo(writer);
                }
                return true;
            }

            return false;
        }

        protected override void Reset(ConfigurationElement parentElement) {
            base.Reset(parentElement);
            ValueXml = ((SettingValueElement) parentElement).ValueXml;
        }

        protected override void Unmerge(ConfigurationElement sourceElement, ConfigurationElement parentElement, 
                                                ConfigurationSaveMode saveMode) {
            base.Unmerge(sourceElement, parentElement, saveMode);
            ValueXml = ((SettingValueElement) sourceElement).ValueXml;
        }
    } 
}
