//------------------------------------------------------------------------------
// <copyright file="SettingsAttributes.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Configuration
{
    using System;

    /// <devdoc>
    ///     Indicates that a setting is to be stored on a per-application basis.
    /// </devdoc>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ApplicationScopedSettingAttribute : SettingAttribute {
    }

    /// <devdoc>
    ///     Indicates to the provider what default value to use for this setting when no stored value
    ///     is found. The value should be encoded into a string and is interpreted based on the SerializeAs
    ///     value for this setting. For example, if SerializeAs is Xml, the default value will be
    ///     "stringified" Xml.
    /// </devdoc>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class DefaultSettingValueAttribute : Attribute {
        private readonly string _value;

        /// <devdoc>
        ///     Constructor takes the default value as string.
        /// </devdoc>
        public DefaultSettingValueAttribute(string value) {
            _value = value;
        }

        /// <devdoc>
        ///     Default value.
        /// </devdoc>
        public string Value {
            get {
                return _value;
            }
        }
    }

    /// <devdoc>
    ///     Indicates that the provider should disable any logic that gets invoked when an application
    ///     upgrade is detected.
    /// </devdoc>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class NoSettingsVersionUpgradeAttribute : Attribute {
    }

    /// <devdoc>
    ///     Use this attribute to mark properties on a settings class that are to be treated
    ///     as settings. ApplicationSettingsBase will ignore all properties not marked with
    ///     this or a derived attribute.
    /// </devdoc>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes")]
    [AttributeUsage(AttributeTargets.Property)]
    public class SettingAttribute : Attribute {
    }

    /// <devdoc>
    ///     Description for a particular setting.
    /// </devdoc>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class SettingsDescriptionAttribute : Attribute {
        private readonly string _desc;

        /// <devdoc>
        ///     Constructor takes the description string.
        /// </devdoc>
        public SettingsDescriptionAttribute(string description) {
            _desc = description;
        }
    
        /// <devdoc>
        ///     Description string.
        /// </devdoc>
        public string Description {
            get {
                return _desc;
            }
        }
    }

    /// <devdoc>
    ///     Description for a particular settings group.
    /// </devdoc>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class SettingsGroupDescriptionAttribute : Attribute {
        private readonly string _desc;

        /// <devdoc>
        ///     Constructor takes the description string.
        /// </devdoc>
        public SettingsGroupDescriptionAttribute(string description) {
            _desc = description;
        }
    
        /// <devdoc>
        ///     Description string.
        /// </devdoc>
        public string Description {
            get {
                return _desc;
            }
        }
    }

    /// <devdoc>
    ///     Name of a particular settings group.
    /// </devdoc>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class SettingsGroupNameAttribute : Attribute {
        private readonly string _groupName;

        /// <devdoc>
        ///     Constructor takes the group name.
        /// </devdoc>
        public SettingsGroupNameAttribute(string groupName) {
            _groupName = groupName;
        }
    
        /// <devdoc>
        ///     Name of the settings group.
        /// </devdoc>
        public string GroupName {
            get {
                return _groupName;
            }
        }
    }

    /// <devdoc>
    ///     Indicates the SettingsManageability for a group of/individual setting.
    /// </devdoc>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public sealed class SettingsManageabilityAttribute : Attribute {
        private readonly SettingsManageability _manageability;

        /// <devdoc>
        ///     Constructor takes a SettingsManageability enum value.
        /// </devdoc>
        public SettingsManageabilityAttribute(SettingsManageability manageability) {
            _manageability = manageability;
        }
    
        /// <devdoc>
        ///     SettingsManageability value to use
        /// </devdoc>
        public SettingsManageability Manageability {
            get {
                return _manageability;
            }
        }
    }

    /// <devdoc>
    ///     Indicates the provider associated with a group of/individual setting.
    /// </devdoc>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public sealed class SettingsProviderAttribute : Attribute {
        private readonly string _providerTypeName;

        /// <devdoc>
        ///     Constructor takes the provider's assembly qualified type name.
        /// </devdoc>
        public SettingsProviderAttribute(string providerTypeName) {
            _providerTypeName = providerTypeName;
        }

        /// <devdoc>
        ///     Constructor takes the provider's type.
        /// </devdoc>
        public SettingsProviderAttribute(Type providerType) {
            if (providerType != null) {
                _providerTypeName = providerType.AssemblyQualifiedName;
            }
        }
    
        /// <devdoc>
        ///     Type name of the provider
        /// </devdoc>
        public string ProviderTypeName {
            get {
                return _providerTypeName;
            }
        }
    }

    /// <devdoc>
    ///     Indicates the SettingsSerializeAs for a group of/individual setting.
    /// </devdoc>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public sealed class SettingsSerializeAsAttribute : Attribute {
        private readonly SettingsSerializeAs _serializeAs;

        /// <devdoc>
        ///     Constructor takes a SettingsSerializeAs enum value.
        /// </devdoc>
        public SettingsSerializeAsAttribute(SettingsSerializeAs serializeAs) {
            _serializeAs = serializeAs;
        }
    
        /// <devdoc>
        ///     SettingsSerializeAs value to use
        /// </devdoc>
        public SettingsSerializeAs SerializeAs {
            get {
                return _serializeAs;
            }
        }
    }

    /// <devdoc>
    ///     Indicates the SpecialSetting for a group of/individual setting.
    /// </devdoc>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public sealed class SpecialSettingAttribute : Attribute {
        private readonly SpecialSetting _specialSetting;

        /// <devdoc>
        ///     Constructor takes a SpecialSetting enum value.
        /// </devdoc>
        public SpecialSettingAttribute(SpecialSetting specialSetting) {
            _specialSetting = specialSetting;
        }
    
        /// <devdoc>
        ///     SpecialSetting value to use
        /// </devdoc>
        public SpecialSetting SpecialSetting {
            get {
                return _specialSetting;
            }
        }
    }

    /// <devdoc>
    ///     Indicates that a setting is to be stored on a per-user basis.
    /// </devdoc>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class UserScopedSettingAttribute : SettingAttribute {
    }

    public enum  SettingsManageability {
       Roaming = 0
    }

    /// <devdoc>
    ///     Indicates settings that are to be treated "specially".
    /// </devdoc>
    public enum  SpecialSetting {
       ConnectionString = 0,
       WebServiceUrl = 1
    }
}

