//------------------------------------------------------------------------------
// <copyright file="RootProfilePropertySettingsCollection.cs" company="Microsoft">
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

    // class ProfileSection

    // ProfileGroupSettingsCollection

    [ConfigurationCollection(typeof(ProfilePropertySettings))]
    public sealed class RootProfilePropertySettingsCollection : ProfilePropertySettingsCollection {
        private ProfileGroupSettingsCollection _propGroups = new ProfileGroupSettingsCollection();
        private static ConfigurationPropertyCollection _properties;

        static RootProfilePropertySettingsCollection() {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        public RootProfilePropertySettingsCollection() {
        }

        protected override bool AllowClear {
            get {
                return true;
            }
        }

        protected override bool ThrowOnDuplicate {
            get {
                return true;
            }
        }

        protected override bool OnDeserializeUnrecognizedElement(String elementName, XmlReader reader) {
            bool handled = false;

            // Deal with the "group" element
            if (elementName == "group") {
                ProfileGroupSettingsCollection groupCollection;
                ProfileGroupSettings newGroupSettings;
                ProfileGroupSettings curGroupSettings = null;
                string name = reader.GetAttribute("name");

                groupCollection = GroupSettings;

                if (name != null) {
                    curGroupSettings = groupCollection[name];
                }

                newGroupSettings = new ProfileGroupSettings();
                newGroupSettings.InternalReset(curGroupSettings);
                newGroupSettings.InternalDeserialize(reader, false);

                groupCollection.AddOrReplace(newGroupSettings);

                handled = true;
            }
            else {
                if (elementName == "clear") {
                    GroupSettings.Clear();
                }

                // Let the base class deal with "add, remove, clear"
                handled = base.OnDeserializeUnrecognizedElement(elementName, reader);
            }

            return handled;
        }

        protected override bool IsModified() {
            return base.IsModified() || GroupSettings.InternalIsModified();
        }

        protected override void ResetModified() {
            base.ResetModified();
            GroupSettings.InternalResetModified();
        }

        public override bool Equals(object rootProfilePropertySettingsCollection) {
            RootProfilePropertySettingsCollection o = rootProfilePropertySettingsCollection as RootProfilePropertySettingsCollection;
            return (o != null && Object.Equals(this, o) && Object.Equals(GroupSettings, o.GroupSettings));
        }

        public override int GetHashCode() {
            return HashCodeCombiner.CombineHashCodes(base.GetHashCode(), GroupSettings.GetHashCode());
        }

        protected override void Reset(ConfigurationElement parentElement) {
            RootProfilePropertySettingsCollection parent = parentElement as RootProfilePropertySettingsCollection;
            base.Reset(parentElement);
            GroupSettings.InternalReset(parent.GroupSettings);
        }

        protected override void Unmerge(ConfigurationElement sourceElement,
                                        ConfigurationElement parentElement,
                                        ConfigurationSaveMode saveMode) {
            RootProfilePropertySettingsCollection parent = parentElement as RootProfilePropertySettingsCollection;
            RootProfilePropertySettingsCollection source = sourceElement as RootProfilePropertySettingsCollection;

            base.Unmerge(sourceElement, parentElement, saveMode);
            GroupSettings.InternalUnMerge(source.GroupSettings, (parent != null) ? parent.GroupSettings : null, saveMode);
        }

        protected override bool SerializeElement(XmlWriter writer, bool serializeCollectionKey) {
            bool DataToWrite = false;
            if (base.SerializeElement(null, false) == true ||
                GroupSettings.InternalSerialize(null, false) == true) {
                DataToWrite |= base.SerializeElement(writer, false);
                DataToWrite |= GroupSettings.InternalSerialize(writer, false);
            }
            return DataToWrite;
        }

        [ConfigurationProperty("group")]
        public ProfileGroupSettingsCollection GroupSettings {
            get {
                return _propGroups;
            }
        }
    }
}
