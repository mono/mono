//------------------------------------------------------------------------------
// <copyright file="ConfigurationSectionCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Configuration {
    using System.Collections;
    using System.Collections.Specialized;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable()]
    public sealed class ConfigurationSectionCollection : NameObjectCollectionBase {

        private MgmtConfigurationRecord     _configRecord;
        private ConfigurationSectionGroup   _configSectionGroup;

        //
        // Create the collection of all sections in the section group.
        //
        internal ConfigurationSectionCollection(MgmtConfigurationRecord configRecord, ConfigurationSectionGroup configSectionGroup) :
                base(StringComparer.Ordinal) {

            _configRecord = configRecord;
            _configSectionGroup = configSectionGroup;

            foreach (DictionaryEntry de in _configRecord.SectionFactories) {
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
        public ConfigurationSection this[string name] {
            get {
                return Get(name);
            }
        }

        // Indexer via integer index.
        public ConfigurationSection this[int index] {
            get {
                return Get(index);
            }
        }

        //
        // Public methods
        //

        //
        // Add a new section to the collection. This will result in a new declaration and definition.
        //
        // It is an error if the section already exists.
        //
        public void Add(string name, ConfigurationSection section) {
            VerifyIsAttachedToConfigRecord();

            _configRecord.AddConfigurationSection(_configSectionGroup.SectionGroupName, name, section);
            BaseAdd(name, name);
        }

        //
        // Remove all sections from the collection.
        //
        public void Clear() {
            VerifyIsAttachedToConfigRecord();

            string[] allKeys = BaseGetAllKeys();
            foreach (string key in allKeys) {
                Remove(key);
            }
        }

        //
        // Return the number of sections in the collection.
        //
        public override int Count {
            get {
                return base.Count;
            }
        }

        //
        // Copy all the sections to an array.
        //
        public void CopyTo(ConfigurationSection[] array, int index) {
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
        // Get the section at a given index.
        //
        public ConfigurationSection Get(int index) {
            return Get(GetKey(index));
        }

        //
        // Get the section with a given name.
        //
        public ConfigurationSection Get(string name) {
            VerifyIsAttachedToConfigRecord();

            // validate name
            if (String.IsNullOrEmpty(name))
                throw ExceptionUtil.ParameterNullOrEmpty("name");

            // prevent GetConfig from returning config not in this collection
            if (name.IndexOf('/') >= 0)
                return null;

            // get the section from the config record
            string configKey = BaseConfigurationRecord.CombineConfigKey(_configSectionGroup.SectionGroupName, name);
            return (ConfigurationSection)_configRecord.GetSection(configKey);
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
            return BaseGetKey(index);
        }

        // Return the string keys of the collection.
        public override KeysCollection Keys {
            get {
                return base.Keys;
            }
        }

        //
        // Remove the declaration and definition of a section in this config file, including any 
        // location sections in the file.
        //
        // Note that if the section is declared in a parent, we still remove the declaration and
        // definition, and the instance of ConfigurationSection will be detached from the collection.
        // However, the collection will still have a ConfigurationSection of that name in the collection,
        // only it will have the value of the immediate parent.
        //
        public void Remove(string name) {
            VerifyIsAttachedToConfigRecord();

            //
            // Remove the factory and section from this record, so that when config is written,
            // it will contain neither a declaration or definition.
            //
            _configRecord.RemoveConfigurationSection(_configSectionGroup.SectionGroupName, name);

            //
            // Remove the section from the collection if it is no longer in the list of all SectionFactories.
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
