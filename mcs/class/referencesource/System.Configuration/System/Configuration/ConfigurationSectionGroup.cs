//------------------------------------------------------------------------------
// <copyright file="ConfigurationSectionGroup.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Configuration {
    using System;
    using System.Configuration.Internal;
    using System.Runtime.Versioning;

    public class ConfigurationSectionGroup {
        string                              _configKey              = String.Empty;
        string                              _group                  = String.Empty;
        string                              _name                   = String.Empty;
        ConfigurationSectionCollection      _configSections;
        ConfigurationSectionGroupCollection _configSectionGroups;
        MgmtConfigurationRecord             _configRecord;
        string                              _typeName;
        bool                                _declared;
        bool                                _declarationRequired;
        bool                                _isRoot;


        public ConfigurationSectionGroup() {
        }

        internal void AttachToConfigurationRecord(MgmtConfigurationRecord configRecord, FactoryRecord factoryRecord) {
            _configRecord = configRecord;
            _configKey = factoryRecord.ConfigKey;
            _group = factoryRecord.Group;
            _name = factoryRecord.Name;
            _typeName = factoryRecord.FactoryTypeName;

            if (_typeName != null) {
                 FactoryRecord parentFactoryRecord = null;
                 if (!configRecord.Parent.IsRootConfig) {
                     parentFactoryRecord = configRecord.Parent.FindFactoryRecord(factoryRecord.ConfigKey, true);
                 }

                 _declarationRequired = (parentFactoryRecord == null || parentFactoryRecord.FactoryTypeName == null);
                 _declared            = configRecord.GetFactoryRecord(factoryRecord.ConfigKey, true) != null;
            }
        }

        internal void RootAttachToConfigurationRecord(MgmtConfigurationRecord configRecord) {
            _configRecord = configRecord;

            _isRoot = true;
        }

        internal void DetachFromConfigurationRecord() {
            if (_configSections != null) {
                _configSections.DetachFromConfigurationRecord();
            }

            if (_configSectionGroups != null) {
                _configSectionGroups.DetachFromConfigurationRecord();
            }

            _configRecord = null;
        }

        internal bool Attached {
            get {return _configRecord != null;}
        }

        private FactoryRecord FindParentFactoryRecord(bool permitErrors) {
            FactoryRecord factoryRecord = null;

            if (_configRecord != null && !_configRecord.Parent.IsRootConfig) {
                factoryRecord = _configRecord.Parent.FindFactoryRecord(_configKey, permitErrors);
            }

            return factoryRecord;
        }

        private void VerifyIsAttachedToConfigRecord() {
            if (_configRecord == null) {
                throw new InvalidOperationException(SR.GetString(SR.Config_cannot_edit_configurationsectiongroup_when_not_attached));
            }
        }

        // IsDeclared
        //
        // Is the Declaration in this config file?
        //
        public bool IsDeclared {
            get {
                return _declared;
            }
        }

        // IsDeclarationRequired
        //
        // Is the Declaration Required.  It is required if it is not set
        // int a parent, or the parent entry does not have the type
        //
        public bool IsDeclarationRequired {
            get {
                return _declarationRequired;
            }
        }

        // ForceDeclaration
        //
        // Force the declaration to be written.
        //
        public void ForceDeclaration() {
            ForceDeclaration(true);
        }

        // ForceDeclaration
        //
        // Force the declaration to be written.  If this is false, it
        // may be ignored depending on if it is Required
        //
        public void ForceDeclaration(bool force) {
            if (_isRoot) {
                throw new InvalidOperationException(SR.GetString(SR.Config_root_section_group_cannot_be_edited));
            }

            if (_configRecord != null && _configRecord.IsLocationConfig) {
                throw new InvalidOperationException(SR.GetString(SR.Config_cannot_edit_configurationsectiongroup_in_location_config));
            }

            if (!force && _declarationRequired ) {
                // Since it is required, we can not remove it
            }
            else {
                _declared = force;
            }
        }

        public string SectionGroupName {
            get {return _configKey;}
        }

        public string Name {
            get {return _name;}
        }

        public string Type {
            get {return _typeName;}
            set {
                if (_isRoot) {
                    throw new InvalidOperationException(SR.GetString(SR.Config_root_section_group_cannot_be_edited));
                }

                // Since type is optional for a section group, allow it to be removed.
                // Note that a typename of "" is not permitted in the config file.
                string typeName = value;
                if (String.IsNullOrEmpty(typeName)) {
                    typeName = null;
                }

                if (_configRecord != null) {
                    if (_configRecord.IsLocationConfig) {
                        throw new InvalidOperationException(SR.GetString(SR.Config_cannot_edit_configurationsectiongroup_in_location_config));
                    }

                    // allow type to be different from current type,
                    // so long as it doesn't conflict with a type already defined
                    if (typeName != null) {
                        FactoryRecord factoryRecord = FindParentFactoryRecord(false);
                        if (factoryRecord != null && !factoryRecord.IsEquivalentType(_configRecord.Host, typeName)) {
                            throw new ConfigurationErrorsException(SR.GetString(SR.Config_tag_name_already_defined, _configKey));
                        }
                    }
                }

                _typeName = typeName;
            }
        }

        public ConfigurationSectionCollection Sections {
            get {
                if (_configSections == null) {
                    VerifyIsAttachedToConfigRecord();
                    _configSections = new ConfigurationSectionCollection(_configRecord, this);
                }

                return _configSections;
            }
        }

        public ConfigurationSectionGroupCollection SectionGroups {
            get {
                if (_configSectionGroups == null) {
                    VerifyIsAttachedToConfigRecord();
                    _configSectionGroups = new ConfigurationSectionGroupCollection(_configRecord, this);
                }

                return _configSectionGroups;
            }
        }

        internal bool IsRoot {
            get {return _isRoot;}
        }

        protected internal virtual bool ShouldSerializeSectionGroupInTargetVersion(FrameworkName targetFramework) {
            return true;
        }
    }
}
