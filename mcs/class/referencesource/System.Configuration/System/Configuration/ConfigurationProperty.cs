//------------------------------------------------------------------------------
// <copyright file="ConfigurationProperty.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel;
using System.Security.Permissions;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;
using System.Text;
using System.Diagnostics.CodeAnalysis;

namespace System.Configuration {

    public sealed class ConfigurationProperty {
        internal static readonly ConfigurationValidatorBase NonEmptyStringValidator = new StringValidator(1);
        private readonly static ConfigurationValidatorBase DefaultValidatorInstance = new DefaultValidator();
        internal static readonly string DefaultCollectionPropertyName = "";
        private string _name;
        private string _providedName;
        private string _description;
        private Type _type;
        private Object _defaultValue;
        private TypeConverter _converter;
        private ConfigurationPropertyOptions _options;
        private ConfigurationValidatorBase _validator;
        private String _addElementName = null;
        private String _removeElementName = null;
        private String _clearElementName = null;
        private volatile bool _isTypeInited;
        private volatile bool _isConfigurationElementType;

        public ConfigurationProperty(String name, Type type) {
            object defaultValue = null;

            ConstructorInit(name, type, ConfigurationPropertyOptions.None, null, null);

            if (type == typeof(string)) {
                defaultValue = String.Empty;
            }
            else if (type.IsValueType) {
                defaultValue = TypeUtil.CreateInstanceWithReflectionPermission(type);
            }
            SetDefaultValue(defaultValue);
        }

        public ConfigurationProperty(String name, Type type, Object defaultValue)
            : this(name, type, defaultValue, ConfigurationPropertyOptions.None) {
        }

        public ConfigurationProperty(String name, Type type, Object defaultValue, ConfigurationPropertyOptions options)
            : this(name, type, defaultValue, null, null, options) {
        }

        public ConfigurationProperty(String name,
                                     Type type,
                                     Object defaultValue,
                                     TypeConverter typeConverter,
                                     ConfigurationValidatorBase validator,
                                     ConfigurationPropertyOptions options)
            : this(name, type, defaultValue, typeConverter, validator, options, null) {
        }

        public ConfigurationProperty(String name,
                                     Type type,
                                     Object defaultValue,
                                     TypeConverter typeConverter,
                                     ConfigurationValidatorBase validator,
                                     ConfigurationPropertyOptions options,
                                     string description) {
            ConstructorInit(name, type, options, validator, typeConverter);

            SetDefaultValue(defaultValue);
        }

        internal ConfigurationProperty(PropertyInfo info) {
            Debug.Assert(info != null, "info != null");

            // Bellow are the attributes we handle
            TypeConverterAttribute attribConverter = null;
            ConfigurationPropertyAttribute attribProperty = null;
            ConfigurationValidatorAttribute attribValidator = null;

            // Compatability attributes
            // If the approprite data is provided in the ConfigPropAttribute then the one bellow will be ignored
            DescriptionAttribute attribStdDescription = null;
            DefaultValueAttribute attribStdDefault = null;

            TypeConverter typeConverter = null;
            ConfigurationValidatorBase validator = null;

            // Find the interesting attributes in the collection
            foreach (Attribute attribute in Attribute.GetCustomAttributes(info)) {
                if (attribute is TypeConverterAttribute) {
                    attribConverter = (TypeConverterAttribute)attribute;
                    typeConverter = TypeUtil.CreateInstanceRestricted<TypeConverter>(info.DeclaringType, attribConverter.ConverterTypeName);
                }
                else if (attribute is ConfigurationPropertyAttribute) {
                    attribProperty = (ConfigurationPropertyAttribute)attribute;
                }
                else if (attribute is ConfigurationValidatorAttribute) {
                    // There could be more then one validator attribute specified on a property
                    // Currently we consider this an error since it's too late to fix it for whidbey
                    // but the right thing to do is to introduce new validator type ( CompositeValidator ) that is a list of validators and executes
                    // them all

                    if (validator != null) {
                        throw new ConfigurationErrorsException(SR.GetString(SR.Validator_multiple_validator_attributes, info.Name));
                    }

                    attribValidator = (ConfigurationValidatorAttribute)attribute;
                    attribValidator.SetDeclaringType(info.DeclaringType);
                    validator = attribValidator.ValidatorInstance;
                }
                else if (attribute is DescriptionAttribute) {
                    attribStdDescription = (DescriptionAttribute)attribute;
                }
                else if (attribute is DefaultValueAttribute) {
                    attribStdDefault = (DefaultValueAttribute)attribute;
                }

            }

            Type propertyType = info.PropertyType;
            // Collections need some customization when the collection attribute is present
            if (typeof(ConfigurationElementCollection).IsAssignableFrom(propertyType)) {
                ConfigurationCollectionAttribute attribCollection =
                    Attribute.GetCustomAttribute(info,
                                                    typeof(ConfigurationCollectionAttribute)) as ConfigurationCollectionAttribute;

                // If none on the property - see if there is an attribute on the collection type itself
                if (attribCollection == null) {
                    attribCollection =
                        Attribute.GetCustomAttribute(propertyType,
                                                        typeof(ConfigurationCollectionAttribute)) as ConfigurationCollectionAttribute;
                }
                if (attribCollection != null) {
                    if (attribCollection.AddItemName.IndexOf(',') == -1) {
                        _addElementName = attribCollection.AddItemName;
                    }
                    _removeElementName = attribCollection.RemoveItemName;
                    _clearElementName = attribCollection.ClearItemsName;

                }

            }

            // This constructor shouldnt be invoked if the reflection info is not for an actual config property
            Debug.Assert(attribProperty != null, "attribProperty != null");

            ConstructorInit(attribProperty.Name,
                                info.PropertyType,
                                attribProperty.Options,
                                validator,
                                typeConverter);

            // Figure out the default value
            InitDefaultValueFromTypeInfo(attribProperty, attribStdDefault);

            // Get the description
            if ((attribStdDescription != null) && !string.IsNullOrEmpty(attribStdDescription.Description)) {
                _description = attribStdDescription.Description;
            }
        }

        private void ConstructorInit(string name,
                                        Type type,
                                        ConfigurationPropertyOptions options,
                                        ConfigurationValidatorBase validator,
                                        TypeConverter converter) {
            if (typeof(ConfigurationSection).IsAssignableFrom(type)) {
                throw new ConfigurationErrorsException(SR.GetString(SR.Config_properties_may_not_be_derived_from_configuration_section, name));
            }

            _providedName = name; // save the provided name so we can check for default collection names
            if ((options & ConfigurationPropertyOptions.IsDefaultCollection) != 0 &&
                String.IsNullOrEmpty(name)) {
                name = DefaultCollectionPropertyName;
            }
            else {
                ValidatePropertyName(name);
            }

            _name = name;
            _type = type;
            _options = options;
            _validator = validator;
            _converter = converter;

            // Use the default validator if none was supplied
            if (_validator == null) {
                _validator = DefaultValidatorInstance;
            }
            else {
                // Make sure the supplied validator supports the type of this property
                if (!_validator.CanValidate(_type)) {
                    throw new ConfigurationErrorsException(SR.GetString(SR.Validator_does_not_support_prop_type, _name));
                }
            }
        }

        private void ValidatePropertyName(string name) {
            if (string.IsNullOrEmpty(name)) {
                throw new ArgumentException(SR.GetString(SR.String_null_or_empty), "name");
            }

            if (BaseConfigurationRecord.IsReservedAttributeName(name)) {
                throw new ArgumentException(SR.GetString(SR.Property_name_reserved, name));
            }
        }

        private void SetDefaultValue(object value) {
            // Validate the default value if any. This should make errors from invalid defaults easier to catch
            if (value != null && value != ConfigurationElement.s_nullPropertyValue) {
                bool canAssign = _type.IsAssignableFrom(value.GetType());
                if (!canAssign && this.Converter.CanConvertFrom(value.GetType())) {
                    value = this.Converter.ConvertFrom(value);
                }
                else if (!canAssign) {
                    throw new ConfigurationErrorsException(SR.GetString(SR.Default_value_wrong_type, _name));
                }

                Validate(value);

                _defaultValue = value;
            }
        }

        private void InitDefaultValueFromTypeInfo(ConfigurationPropertyAttribute attribProperty,
                                                    DefaultValueAttribute attribStdDefault) {
            object defaultValue = attribProperty.DefaultValue;

            // If there is no default value there - try the other attribute ( the clr standard one )
            if ((defaultValue == null || defaultValue == ConfigurationElement.s_nullPropertyValue) &&
                (attribStdDefault != null)) {
                defaultValue = attribStdDefault.Value;
            }

            // If there was a default value in the prop attribute - check if we need to convert it from string
            if ((defaultValue != null) && (defaultValue is string) && (_type != typeof(string))) {
                // Use the converter to parse this property default value
                try {
                    defaultValue = Converter.ConvertFromInvariantString((string)defaultValue);
                }
                catch (Exception ex) {
                    throw new ConfigurationErrorsException(SR.GetString(SR.Default_value_conversion_error_from_string, _name, ex.Message));
                }
            }
            if (defaultValue == null || defaultValue == ConfigurationElement.s_nullPropertyValue) {
                if (_type == typeof(string)) {
                    defaultValue = String.Empty;
                }
                else if (_type.IsValueType) {
                    defaultValue = TypeUtil.CreateInstanceWithReflectionPermission(_type);
                }
            }
            SetDefaultValue(defaultValue);
        }

        public string Name {
            [SuppressMessage("Microsoft.Concurrency", "CA8001", Justification = "_name is actually immutable once constructed")]
            get {
                return _name;
            }
        }

        public string Description {
            [SuppressMessage("Microsoft.Concurrency", "CA8001", Justification = "_description is actually immutable once constructed")]
            get {
                return _description;
            }
        }

        internal string ProvidedName {
            [SuppressMessage("Microsoft.Concurrency", "CA8001", Justification = "_providedName is actually immutable once constructed")]
            get { return _providedName; }
        }

        internal bool IsConfigurationElementType {
            [SuppressMessage("Microsoft.Concurrency", "CA8001", Justification = "_type is effectively readonly")]
            get {
                if (!_isTypeInited) {
                    _isConfigurationElementType = typeof(ConfigurationElement).IsAssignableFrom(_type);
                    _isTypeInited = true;
                }
                return _isConfigurationElementType;
            }
        }

        public Type Type {
            [SuppressMessage("Microsoft.Concurrency", "CA8001", Justification = "_type is actually immutable once constructed")]
            get {
                return _type;
            }
        }

        public Object DefaultValue {
            [SuppressMessage("Microsoft.Concurrency", "CA8001", Justification = "_defaultValue is actually immutable once constructed")]
            get {
                return _defaultValue;
            }
        }

        public bool IsRequired {
            [SuppressMessage("Microsoft.Concurrency", "CA8001", Justification = "_options is actually immutable once constructed")]
            get {
                return (_options & ConfigurationPropertyOptions.IsRequired) != 0;
            }
        }

        public bool IsKey {
            [SuppressMessage("Microsoft.Concurrency", "CA8001", Justification = "_options is actually immutable once constructed")]
            get {
                return (_options & ConfigurationPropertyOptions.IsKey) != 0;
            }
        }

        public bool IsDefaultCollection {
            [SuppressMessage("Microsoft.Concurrency", "CA8001", Justification = "_options is actually immutable once constructed")]
            get {
                return ((_options & ConfigurationPropertyOptions.IsDefaultCollection) != 0);
            }
        }

        public bool IsTypeStringTransformationRequired  {
            [SuppressMessage("Microsoft.Concurrency", "CA8001", Justification = "_options is actually immutable once constructed")]
            get {
                return (_options & ConfigurationPropertyOptions.IsTypeStringTransformationRequired) != 0;
            }
        }

        public bool IsAssemblyStringTransformationRequired {
            [SuppressMessage("Microsoft.Concurrency", "CA8001", Justification = "_options is actually immutable once constructed")]
            get {
                return (_options & ConfigurationPropertyOptions.IsAssemblyStringTransformationRequired) != 0;
            }
        }

        public bool IsVersionCheckRequired {
            [SuppressMessage("Microsoft.Concurrency", "CA8001", Justification = "_options is actually immutable once constructed")]
            get {
                return (_options & ConfigurationPropertyOptions.IsVersionCheckRequired) != 0;
            }
        }

        public TypeConverter Converter {
            get {
                CreateConverter();

                return _converter;
            }
        }

        public ConfigurationValidatorBase Validator {
            get {
                return _validator;
            }
        }

        internal String AddElementName {
            get {
                return _addElementName;
            }
        }
        internal String RemoveElementName {
            get {
                return _removeElementName;
            }
        }
        internal String ClearElementName {
            get {
                return _clearElementName;
            }
        }

        internal Object ConvertFromString(string value) {
            object result = null;

            try {
                result = Converter.ConvertFromInvariantString(value);
            }
            catch (Exception ex) {
                throw new ConfigurationErrorsException(SR.GetString(SR.Top_level_conversion_error_from_string, _name, ex.Message));
            }

            return result;
        }

        internal string ConvertToString(Object value) {
            string result = null;

            try {
                if (_type == typeof(bool)) {
                    result = ((bool)value) ? "true" : "false"; // the converter will break 1.1 compat for bool
                }
                else {
                    result = Converter.ConvertToInvariantString(value);
                }
            }
            catch (Exception ex) {
                throw new ConfigurationErrorsException(SR.GetString(SR.Top_level_conversion_error_to_string, _name, ex.Message));
            }

            return result;
        }
        [SuppressMessage("Microsoft.Concurrency", "CA8001", Justification = "_name and _validator are effectively readonly")]
        internal void Validate(object value) {
            try {
                _validator.Validate(value);
            }
            catch (Exception ex) {
                throw new ConfigurationErrorsException(SR.GetString(SR.Top_level_validation_error, _name, ex.Message),ex);
            }
        }
        private void CreateConverter() {
            // Some properties cannot have type converters.
            // Such examples are properties that are ConfigurationElement ( derived classes )
            // or properties which are user-defined and the user code handles serialization/desirialization so
            // the property itself is never converted to/from string

            if (_converter == null) {
                // Enums are exception. We use our custom converter for all enums
                if (_type.IsEnum) {
                    _converter = new GenericEnumConverter(_type);
                }
                else if (!_type.IsSubclassOf(typeof(ConfigurationElement))) {
                    _converter = TypeDescriptor.GetConverter(_type);

                    if ((_converter == null) ||
                            !_converter.CanConvertFrom(typeof(String)) ||
                            !_converter.CanConvertTo(typeof(String))) {
                        throw new ConfigurationErrorsException(SR.GetString(SR.No_converter, _name, _type.Name));
                    }
                }
            }
        }
    }
}
