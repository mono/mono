//------------------------------------------------------------------------------
// <copyright file="ConfigurationSectionGroupCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Configuration {
    using System.Collections;
    using System.Collections.Specialized;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable()]
    public sealed class ConfigurationSectionGroupCollection : NameObjectCollectionBase {

        private MgmtConfigurationRecord     _configRecord;
        private ConfigurationSectionGroup   _configSectionGroup;

        //
        // Create the collection of all section groups in the section group.
        //
        internal ConfigurationSectionGroupCollection(MgmtConfigurationRecord configRecord, ConfigurationSectionGroup configSectionGroup) :
                base(StringComparer.Ordinal) {
            _configRecord = configRecord;
            _configSectionGroup = configSectionGroup;

            foreach (DictionaryEntry de in _configRecord.SectionGroupFactories) {
                FactoryId factoryId = (FactoryId) de.Value;
                if (factoryId.Group == _configSectionGroup.SectionGroupName) {
                    BaseAdd(factoryId.Name, factoryId.Name);
                }
            }
        }

        [SecurityPermissionAttribute(SecurityAction.Demand,SerializationFormatter=true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context) {
            base.GetObjectData(info, context);
        }

        //
        // Remove the collection from configuration system, and remove all entries
        // in the base collection so that enumeration will return an empty collection.
        //
        internal void DetachFromConfigurationRecord() {
            _configRecord = null;
            BaseClear();
        }

        private void VerifyIsAttachedToConfigRecord() {
            if (_configRecord == null) {
                throw new InvalidOperationException(SR.GetString(SR.Config_cannot_edit_configurationsectiongroup_when_not_attached));
            }
        }

        //
        // Public Properties
        //

        // Indexer via name
        public ConfigurationSectionGroup this[string name] {
            get {
                return Get(name);
            }
        }

        // Indexer via integer index.
        public ConfigurationSectionGroup this[int index] {
            get {
                return Get(index);
            }
        }

        //
        // Public methods
        //

        //
        // Add a new section group to the collection. This will result in a new declaration and definition.
        //
        // It is an error if the section already exists.
        //
        public void Add(string name, ConfigurationSectionGroup sectionGroup) {
            VerifyIsAttachedToConfigRecord();
            _configRecord.AddConfigurationSectionGroup(_configSectionGroup.SectionGroupName, name, sectionGroup);
            BaseAdd(name, name);
        }

        //
        // Remove all section groups from the collection.
        //
        public void Clear() {
            VerifyIsAttachedToConfigRecord();

            //
            // If this is the root section group, do not require the location section to be written
            // to the file.
            //
            if (_configSectionGroup.IsRoot) {
                _configRecord.RemoveLocationWriteRequirement();
            }
            
            string[] allKeys = BaseGetAllKeys();
            foreach (string key in allKeys) {
                Remove(key);
            }
        }

        //
        // Return the number of section groups in the collection.
        //
        public override int Count {
            get {
                return base.Count;
            }
        }

        //
        // Copy all section groups to an array.
        //
        public void CopyTo(ConfigurationSectionGroup[] array, int index) {
            if (array == null) {
                throw new ArgumentNullException("array");
            }

            int c = Count;
            if (array.Length < c + index) {
                throw new ArgumentOutOfRangeException("index");
            }

            for (int i = 0, j = index; i < c; i++, j++) {
                array[j] = Get(i);
            }
        }

        //
        // Get the section group at a given index.
        //
        public ConfigurationSectionGroup Get(int index) {
            return Get(GetKey(index));
        }

        //
        // Get the section group with a given name.
        //
        public ConfigurationSectionGroup Get(string name) {
            VerifyIsAttachedToConfigRecord();

            // validate name
            if (String.IsNullOrEmpty(name))
                throw ExceptionUtil.ParameterNullOrEmpty("name");

            // prevent GetConfig from returning config not in this collection
            if (name.IndexOf('/') >= 0)
                return null;

            // get the section group
            string configKey = BaseConfigurationRecord.CombineConfigKey(_configSectionGroup.SectionGroupName, name);
            return _configRecord.GetSectionGroup(configKey);
        }

        // Get an enumerator
        public override IEnumerator GetEnumerator() {
            int c = Count;
            for (int i = 0; i < c; i++) {
                yield return this[i];
            }
        }

        // Get the string key at a given index.
        public string GetKey(int index) {
            return (string) BaseGetKey(index);
        }

        // Return the string keys of the collection.
        public override KeysCollection Keys {
            get {
                return base.Keys;
            }
        }

        //
        // Remove the declaration and definition of a section in this config file, including any 
        // location sections in the file. This will also remove any descendant sections and 
        // section groups.
        //
        // Note that if the section group is declared in a parent, we still remove the declaration and
        // definition, and the instance of ConfigurationSectionGroup will be detached from the collection.
        // However, the collection will still have a ConfigurationSectionGroup of that name in the collection,
        // only it will have the value of the immediate parent.
        //
        public void Remove(string name) {
            VerifyIsAttachedToConfigRecord();

            _configRecord.RemoveConfigurationSectionGroup(_configSectionGroup.SectionGroupName, name);

            //
            // Remove the section from the collection if it is no longer in the list of all SectionGroupFactories.
            //
            string configKey = BaseConfigurationRecord.CombineConfigKey(_configSectionGroup.SectionGroupName, name);
            if (!_configRecord.SectionFactories.Contains(configKey)) {
                BaseRemove(name);
            }
        }

        //
        // Remove the section at that index.
        //
        public void RemoveAt(int index) {
            VerifyIsAttachedToConfigRecord();

            Remove(GetKey(index));
        }
    }
}
