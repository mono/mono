//------------------------------------------------------------------------------
// <copyright file="ProviderSettings.cs" company="Microsoft">
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
    using System.Globalization;

    public sealed class ProviderSettings : ConfigurationElement
    {
        private readonly ConfigurationProperty _propName =
            new ConfigurationProperty(  "name",
                                        typeof( string ),
                                        null,   // no reasonable default
                                        null,   // use default converter
                                        ConfigurationProperty.NonEmptyStringValidator,
                                        ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey);
        private readonly ConfigurationProperty _propType = new ConfigurationProperty("type", typeof(String), "",
                                                                                     ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsTypeStringTransformationRequired);
        private ConfigurationPropertyCollection _properties;
        private NameValueCollection _PropertyNameCollection = null;
        public ProviderSettings()
        {
            _properties = new ConfigurationPropertyCollection();
            _properties.Add(_propName);
            _properties.Add(_propType);
            _PropertyNameCollection = null;
        }
        public ProviderSettings(String name, String type) : this()
        {
            Name = name;
            Type = type;
        }

        protected internal override ConfigurationPropertyCollection Properties
        {
            get
            {
                UpdatePropertyCollection();
                return _properties;
            }
        }

        protected internal override void Unmerge(ConfigurationElement sourceElement,
                                                ConfigurationElement parentElement,
                                                ConfigurationSaveMode saveMode)
        {
            ProviderSettings parentProviders = parentElement as ProviderSettings;
            if (parentProviders != null)
                parentProviders.UpdatePropertyCollection(); // before reseting make sure the bag is filled in

            ProviderSettings sourceProviders = sourceElement as ProviderSettings;
            if (sourceProviders != null)
                sourceProviders.UpdatePropertyCollection(); // before reseting make sure the bag is filled in

            base.Unmerge(sourceElement, parentElement, saveMode);
            UpdatePropertyCollection();
        }

        protected internal override void Reset(ConfigurationElement parentElement)
        {
            ProviderSettings parentProviders = parentElement as ProviderSettings;
            if (parentProviders != null)
                parentProviders.UpdatePropertyCollection(); // before reseting make sure the bag is filled in

            base.Reset(parentElement);
        }

        internal bool UpdatePropertyCollection()
        {
            bool bIsModified = false;
            ArrayList removeList = null;

            if (_PropertyNameCollection != null)
            {
                // remove any data that has been delete from the collection
                foreach (ConfigurationProperty prop in _properties)
                {
                    if (prop.Name != "name" && prop.Name != "type")
                    {
                        if (_PropertyNameCollection.Get(prop.Name) == null)
                        {
                            // _properties.Remove(prop.Name);
                            if (removeList == null)
                                removeList = new ArrayList();

                            if ((Values.GetConfigValue(prop.Name).ValueFlags & ConfigurationValueFlags.Locked) == 0) {
                                removeList.Add(prop.Name);
                                bIsModified = true;
                            }
                        }
                    }
                }

                if (removeList != null)
                {
                    foreach (string propName in removeList)
                    {
                        _properties.Remove(propName);
                    }
                }

                // then copy any data that has been changed in the collection
                foreach (string Key in _PropertyNameCollection)
                {
                    string valueInCollection = _PropertyNameCollection[Key];
                    string valueInBag = GetProperty(Key);

                    if (valueInBag == null || valueInCollection != valueInBag) // add new property
                    {
                        SetProperty(Key, valueInCollection);
                        bIsModified = true;
                    }
                }
            }
            _PropertyNameCollection = null;
            return bIsModified;
        }

        protected internal override bool IsModified()
        {
            return UpdatePropertyCollection() || base.IsModified();
        }

        [ConfigurationProperty("name", IsRequired = true, IsKey=true)]
        public String Name
        {
            get
            {
                return (String)base[_propName];
            }
            set
            {
                base[_propName] = value;
            }
        }

        [ConfigurationProperty("type", IsRequired = true)]
        public String Type
        {
            get
            {
                return (String)base[_propType];
            }
            set
            {
                base[_propType] = value;
            }
        }

        public NameValueCollection Parameters
        {
            get
            {
                if (_PropertyNameCollection == null)
                {
                    lock (this)
                    {
                        if (_PropertyNameCollection == null)
                        {
                            _PropertyNameCollection = new NameValueCollection(StringComparer.Ordinal);

                            foreach (object de in _properties)
                            {
                                ConfigurationProperty prop = (ConfigurationProperty)de;
                                if (prop.Name != "name" && prop.Name != "type")
                                    _PropertyNameCollection.Add(prop.Name, (string)base[prop]);
                            }
                        }
                    }
                }
                return (NameValueCollection)_PropertyNameCollection;
            }
        }


        private string GetProperty(string PropName)
        {
            if (_properties.Contains(PropName))
            {
                ConfigurationProperty prop = _properties[PropName];
                if(prop != null)
                    return (string)base[prop];
            }
            return null;
        }

        private bool SetProperty(string PropName,string value)
        {
            ConfigurationProperty SetPropName = null;
            if (_properties.Contains(PropName))
                SetPropName = _properties[PropName];
            else
            {
                SetPropName = new ConfigurationProperty(PropName, typeof(string), null);
                _properties.Add(SetPropName);
            }
            if (SetPropName != null)
            {
                base[SetPropName] = value;
//                Parameters[PropName] = value;
                return true;
            }
            else
                return false;
        }

        protected override bool OnDeserializeUnrecognizedAttribute(String name, String value)
        {
            ConfigurationProperty _propName = new ConfigurationProperty(name, typeof(string), value);
            _properties.Add(_propName);
            base[_propName] = value; // Add them to the property bag
            Parameters[name] = value;
            return true;
        }

    }
}
