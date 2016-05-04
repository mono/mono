//------------------------------------------------------------------------------
// <copyright file="ProfileGroupSettingsCollection.cs" company="Microsoft">
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

    [ConfigurationCollection(typeof(ProfileGroupSettings), AddItemName = "group")]
    public sealed class ProfileGroupSettingsCollection : ConfigurationElementCollection {
        private static ConfigurationPropertyCollection _properties;
        private bool bModified = false;

        static ProfileGroupSettingsCollection() {
            _properties = new ConfigurationPropertyCollection();
        }

        public ProfileGroupSettingsCollection() {
            AddElementName = "group";
            ClearElementName = String.Empty; // This collection does not support Clear tags
            EmitClear = false;
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        // public properties
        public String[] AllKeys {
            get {
                return StringUtil.ObjectArrayToStringArray(BaseGetAllKeys());
            }
        }
        
        public new ProfileGroupSettings this[string name] {
            get {
                return (ProfileGroupSettings)BaseGet(name);
            }
        }
        
        public ProfileGroupSettings this[int index] {
            get {
                return (ProfileGroupSettings)BaseGet(index);
            }
            set {
                if (BaseGet(index) != null) {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        internal void AddOrReplace(ProfileGroupSettings groupSettings) {
            BaseAdd(groupSettings, false);
        }

        // Protected Overrides
        protected override ConfigurationElement CreateNewElement() {
            return new ProfileGroupSettings();
        }

        protected override Object GetElementKey(ConfigurationElement element) {
            return ((ProfileGroupSettings)element).Name;
        }

        // Internal access to ConfigurationElement methods
        internal bool InternalIsModified() {
            return IsModified();
        }

        internal void InternalResetModified() {
            ResetModified();
        }

        internal void InternalReset(ConfigurationElement parentElement) {
            Reset(parentElement);
        }

        internal void InternalUnMerge(ConfigurationElement sourceElement, ConfigurationElement parentElement,
                                      ConfigurationSaveMode saveMode) {
            // This requires a special unmerge because it should not act like an element in a collection
            // which is the default behavior
            Unmerge(sourceElement, parentElement, saveMode); // Base merge to get locks merged
            BaseClear(); // don't use the merged data however

            ProfileGroupSettingsCollection source = sourceElement as ProfileGroupSettingsCollection;
            ProfileGroupSettingsCollection parent = parentElement as ProfileGroupSettingsCollection;

            // foreach group in the source we should unmerge the individual settings.
            foreach (ProfileGroupSettings settings in source) {
                ProfileGroupSettings settingsFromParent = parent.Get(settings.Name);
                ProfileGroupSettings deltaSettings = new ProfileGroupSettings();

                deltaSettings.InternalUnmerge(settings, settingsFromParent, saveMode);
                BaseAdd(deltaSettings);
            }
            
        }

        internal bool InternalSerialize(XmlWriter writer, bool serializeCollectionKey) {
            if (EmitClear == true) {
                throw new ConfigurationErrorsException(SR.GetString(SR.Clear_not_valid));
            }

            return SerializeElement(writer, serializeCollectionKey);
        }

        // public methods
        public void Add(ProfileGroupSettings group) {
            BaseAdd(group);
        }
        
        public ProfileGroupSettings Get(int index) {
            return (ProfileGroupSettings)BaseGet(index);
        }
        
        public ProfileGroupSettings Get(string name) {
            return (ProfileGroupSettings)BaseGet(name);
        }

        public String GetKey(int index) {
            return (String) BaseGetKey(index);
        }
        
        public void Set(ProfileGroupSettings group) {
            BaseAdd(group, false);
        }

        public int IndexOf(ProfileGroupSettings group) {
            return BaseIndexOf(group);
        }

        public void Remove(string name) {
            ConfigurationElement elem = BaseGet(name);
            if (elem != null) {
                ElementInformation elemInfo = elem.ElementInformation;
                if (elemInfo.IsPresent) {
                    BaseRemove(name);
                }
                else {
                    throw new ConfigurationErrorsException(SR.GetString(SR.Config_base_cannot_remove_inherited_items));
                }
            }
        }

        public void RemoveAt(int index) {
            ConfigurationElement elem = BaseGet(index);
            if (elem != null) {
                ElementInformation elemInfo = elem.ElementInformation;
                if (elemInfo.IsPresent) {
                    BaseRemoveAt(index);
                }
                else {
                    throw new ConfigurationErrorsException(SR.GetString(SR.Config_base_cannot_remove_inherited_items));
                }
            }
        }

        public void Clear() {
            int index = Count-1;
            bModified = true;

            for (int i = index; i >= 0; i--) {
                ConfigurationElement elem = BaseGet(i);
                if (elem != null) {
                    ElementInformation elemInfo = elem.ElementInformation;
                    if (elemInfo.IsPresent) {
                        BaseRemoveAt(i);
                    }
                }
            }
        }
        
        protected override void ResetModified() {
            bModified = false;
            base.ResetModified();
        }

        protected override bool IsModified() {
            if (bModified == true) {
                return true;
            }
            return base.IsModified();
        }

    }
}
