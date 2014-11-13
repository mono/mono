//------------------------------------------------------------------------------
// <copyright file="FolderLevelBuildProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {
    using System;
    using System.Configuration;
    using System.Globalization;
    using System.Web.Compilation;
    using System.Web.Util;

    public sealed class FolderLevelBuildProvider : ConfigurationElement {
        private static ConfigurationPropertyCollection _properties;
        private static readonly ConfigurationProperty _propName =
            new ConfigurationProperty("name",
                                        typeof(string),
                                        null,
                                        null,
                                        StdValidatorsAndConverters.NonEmptyStringValidator,
                                        ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey);
        private static readonly ConfigurationProperty _propType =
            new ConfigurationProperty("type",
                                        typeof(string),
                                        null,
                                        null,
                                        StdValidatorsAndConverters.NonEmptyStringValidator,
                                        ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsTypeStringTransformationRequired);

        private Type _type;

        // AppliesTo value from the FolderLevelBuildProviderAppliesToAttribute
        private FolderLevelBuildProviderAppliesTo _appliesToInternal;

        static FolderLevelBuildProvider() {
            _properties = new ConfigurationPropertyCollection();
            _properties.Add(_propName);
            _properties.Add(_propType);
        }

        public FolderLevelBuildProvider(String name, String type)
            : this() {
            Name = name;
            Type = type;
        }
        internal FolderLevelBuildProvider() {
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        // this override is required because AppliesTo may be in any order in the
        // property string but it still and the default equals operator would consider
        // them different depending on order in the persisted string.
        public override bool Equals(object provider) {
            FolderLevelBuildProvider o = provider as FolderLevelBuildProvider;
            return (o != null && StringUtil.EqualsIgnoreCase(Name, o.Name) && Type == o.Type);
        }
        public override int GetHashCode() {
            return HashCodeCombiner.CombineHashCodes(Name.ToLower(CultureInfo.InvariantCulture).GetHashCode(),
                                                     Type.GetHashCode());
        }


        [ConfigurationProperty("name", IsRequired = true, IsKey = true, DefaultValue = "")]
        [StringValidator(MinLength = 1)]
        public string Name {
            get {
                return (string)base[_propName];
            }
            set {
                base[_propName] = value;
            }
        }

        [ConfigurationProperty("type", IsRequired = true, DefaultValue = "")]
        [StringValidator(MinLength = 1)]
        public string Type {
            get {
                return (string)base[_propType];
            }
            set {
                base[_propType] = value;
            }
        }

        internal Type TypeInternal {
            get {
                if (_type == null) {
                    lock (this) {
                        if (_type == null) {
                            _type = CompilationUtil.LoadTypeWithChecks(Type, typeof(System.Web.Compilation.BuildProvider), null, this, "type");
                        }
                    }
                }

                return _type;
            }
        }

        internal FolderLevelBuildProviderAppliesTo AppliesToInternal {
            get {
                if (_appliesToInternal != 0)
                    return _appliesToInternal;

                // Check whether the control builder's class exposes an AppliesTo attribute
                object[] attrs = TypeInternal.GetCustomAttributes(
                    typeof(FolderLevelBuildProviderAppliesToAttribute), /*inherit*/ true);

                if ((attrs != null) && (attrs.Length > 0)) {
                    Debug.Assert(attrs[0] is FolderLevelBuildProviderAppliesToAttribute);
                    _appliesToInternal = ((FolderLevelBuildProviderAppliesToAttribute)attrs[0]).AppliesTo;
                }
                else {
                    // Default to applying to None
                    _appliesToInternal = FolderLevelBuildProviderAppliesTo.None;
                }

                return _appliesToInternal;
            }
        }
    }
}
