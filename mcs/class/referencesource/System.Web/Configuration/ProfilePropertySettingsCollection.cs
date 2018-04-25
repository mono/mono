//------------------------------------------------------------------------------
// <copyright file="ProfilePropertySettingsCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {
    using System;
    using System.Xml;
    using System.Configuration;
    using System.Collections.Specialized;
    using System.Collections;
    using System.IO;
    using System.Text;
    using System.Web.Util;
    using System.Security.Permissions;

    [ConfigurationCollection(typeof(ProfilePropertySettings))]
    public class ProfilePropertySettingsCollection : ConfigurationElementCollection {
        private static ConfigurationPropertyCollection _properties;

        static ProfilePropertySettingsCollection() {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        public ProfilePropertySettingsCollection() {
        }

        protected virtual bool AllowClear {
            get {
                return false;
            }
        }

        protected override bool ThrowOnDuplicate {
            get {
                return true;
            }
        }

        protected override bool OnDeserializeUnrecognizedElement(String elementName, XmlReader reader) {
            if (!AllowClear) {
                if (elementName == "clear") {
                    throw new ConfigurationErrorsException(SR.GetString(SR.Clear_not_valid), reader);

                }
            }

            if (elementName == "group") {
                throw new ConfigurationErrorsException(SR.GetString(SR.Nested_group_not_valid), reader);
            }

            return base.OnDeserializeUnrecognizedElement(elementName, reader);
        }

        // public properties
        public String[] AllKeys {
            get {
                return StringUtil.ObjectArrayToStringArray(BaseGetAllKeys());
            }
        }

        public new ProfilePropertySettings this[string name] {
            get {
                return (ProfilePropertySettings)BaseGet(name);
            }
        }
        
        public ProfilePropertySettings this[int index] {
            get {
                return (ProfilePropertySettings)BaseGet(index);
            }
            set {
                if (BaseGet(index) != null) {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }
        // Protected Overrides
        protected override ConfigurationElement CreateNewElement() {
            return new ProfilePropertySettings();
        }

        protected override Object GetElementKey(ConfigurationElement element) {
            return ((ProfilePropertySettings)element).Name;
        }

        public void Add(ProfilePropertySettings propertySettings) {
            BaseAdd(propertySettings);
        }
        
        public ProfilePropertySettings Get(int index) {
            return (ProfilePropertySettings)BaseGet(index);
        }
        
        public ProfilePropertySettings Get(string name) {
            return (ProfilePropertySettings)BaseGet(name);
        }

        public String GetKey(int index) {
            return (String) BaseGetKey(index);
        }
        
        public void Remove(string name) {
            BaseRemove(name);
        }
        
        public void RemoveAt(int index) {
            BaseRemoveAt(index);
        }
        
        public void Set(ProfilePropertySettings propertySettings) {
            BaseAdd(propertySettings, false);
        }
        
        public int IndexOf(ProfilePropertySettings propertySettings) {
            return BaseIndexOf(propertySettings);
        }

        public void Clear() {
            BaseClear();
        }
    }
}
