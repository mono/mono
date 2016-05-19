namespace System.Activities.Presentation.PropertyEditing {
    using System;

    using System.Runtime;

    /// <summary>
    /// Concrete implementation of PropertyValueSource for a PropertyEntry that represents a Dependency Property
    /// </summary>
    [Fx.Tag.XamlVisible(false)]
    public class DependencyPropertyValueSource : PropertyValueSource
    {

        private static DependencyPropertyValueSource _dataBound;
        private static DependencyPropertyValueSource _systemResource;
        private static DependencyPropertyValueSource _localDynamicResource;
        private static DependencyPropertyValueSource _localStaticResource;
        private static DependencyPropertyValueSource _templateBinding;
        private static DependencyPropertyValueSource _customMarkupExtension;
        private static DependencyPropertyValueSource _local;
        private static DependencyPropertyValueSource _defaultValue;
        private static DependencyPropertyValueSource _inherited;

        private readonly ValueSource _source;

        /// <summary>
        /// The property is set to a value that is a data binding.
        /// </summary>
        public static DependencyPropertyValueSource DataBound {
            get {
                if (_dataBound == null) _dataBound = new DependencyPropertyValueSource(ValueSource.DataBound);
                return _dataBound;
            }
        }

        /// <summary>
        /// The property is set to a system resource.
        /// </summary>
        public static DependencyPropertyValueSource SystemResource {
            get {
                if (_systemResource == null) _systemResource = new DependencyPropertyValueSource(ValueSource.SystemResource);
                return _systemResource;
            }
        }

        /// <summary>
        /// The property is set to a DynamicResource reference.
        /// </summary>
        public static DependencyPropertyValueSource LocalDynamicResource {
            get {
                if (_localDynamicResource == null) _localDynamicResource = new DependencyPropertyValueSource(ValueSource.LocalDynamicResource);
                return _localDynamicResource;
            }
        }

        /// <summary>
        /// The property is set to a StaticResource reference.
        /// </summary>
        public static DependencyPropertyValueSource LocalStaticResource {
            get {
                if (_localStaticResource == null) _localStaticResource = new DependencyPropertyValueSource(ValueSource.LocalStaticResource);
                return _localStaticResource;
            }
        }

        /// <summary>
        /// The property is set to a TemplateBinding markup extension.
        /// </summary>
        public static DependencyPropertyValueSource TemplateBinding {
            get {
                if (_templateBinding == null) _templateBinding = new DependencyPropertyValueSource(ValueSource.TemplateBinding);
                return _templateBinding;
            }
        }

        /// <summary>
        /// The property is set to a custom markup extension.
        /// </summary>
        public static DependencyPropertyValueSource CustomMarkupExtension {
            get {
                if (_customMarkupExtension == null) _customMarkupExtension = new DependencyPropertyValueSource(ValueSource.CustomMarkupExtension);
                return _customMarkupExtension;
            }
        }

        /// <summary>
        /// The property is set to a local value.
        /// </summary>
        public static DependencyPropertyValueSource Local {
            get {
                if (_local == null) _local = new DependencyPropertyValueSource(ValueSource.Local);
                return _local;
            }
        }

        /// <summary>
        /// The property is set to its default value (ie. it does not have a value set in XAML and 
        /// it's not inheriting any value from its parent)
        /// </summary>
        public static DependencyPropertyValueSource DefaultValue {
            get {
                if (_defaultValue == null) _defaultValue = new DependencyPropertyValueSource(ValueSource.DefaultValue);
                return _defaultValue;
            }
        }

        /// <summary>
        /// The property is inherited from a parent property.
        /// </summary>
        public static DependencyPropertyValueSource Inherited {
            get {
                if (_inherited == null) _inherited = new DependencyPropertyValueSource(ValueSource.Inherited);
                return _inherited;
            }
        }

        private DependencyPropertyValueSource(ValueSource source) {
            _source = source;
        }

        /// <summary>
        /// Read-only property that returns true if the property is set to an expression 
        /// i.e. (DataBound, LocalDynamicResource, LocalStaticResource, SystemResource, TemplateBinding or
        /// CustomMarkupExtension)
        /// </summary>
        public bool IsExpression {
            get {
                return _source == ValueSource.DataBound
                    || _source == ValueSource.LocalDynamicResource
                    || _source == ValueSource.LocalStaticResource
                    || _source == ValueSource.SystemResource
                    || _source == ValueSource.TemplateBinding
                    || _source == ValueSource.CustomMarkupExtension;
            }
        }

        /// <summary>
        /// Read-only property that returns true if the property is set to a system or local resource
        /// </summary>
        public bool IsResource {
            get {
                return _source == ValueSource.SystemResource
                    || _source == ValueSource.LocalDynamicResource
                    || _source == ValueSource.LocalStaticResource;
            }
        }

        /// <summary>
        /// Read-only property that returns true if the property is set to a data binding expression.
        /// </summary>
        public bool IsDataBound {
            get { return _source == ValueSource.DataBound; }
        }

        /// <summary>
        /// Read-only property that returns true if the property is set to a system resource
        /// </summary>
        public bool IsSystemResource {
            get { return _source == ValueSource.SystemResource; }
        }

        /// <summary>
        /// Read-only property that returns true if the property is set to a DynamicResource
        /// </summary>
        public bool IsLocalResource {
            get {
                return _source == ValueSource.LocalDynamicResource
                || _source == ValueSource.LocalStaticResource;
            }
        }

        /// <summary>
        /// Read-only property that returns true if the property is set to a TemplateBinding markup extension
        /// </summary>
        public bool IsTemplateBinding {
            get { return _source == ValueSource.TemplateBinding; }
        }

        /// <summary>
        /// Read-only property that returns true if the property is set to a custom markup extension.
        /// </summary>
        public bool IsCustomMarkupExtension {
            get { return _source == ValueSource.CustomMarkupExtension; }
        }

        /// <summary>
        /// Read-only property that returns true if the property is set to a local value.
        /// </summary>
        public bool IsLocal {
            get { return _source == ValueSource.Local; }
        }

        /// <summary>
        /// Read-only property that returns true if the property is set to its default value 
        /// (ie. it does not have a value set in XAML and it's not inheriting any value from
        /// its parent)
        /// </summary>
        public bool IsDefaultValue {
            get { return _source == ValueSource.DefaultValue; }
        }

        /// <summary>
        /// Read-only property that returns true if the property is inherited.
        /// </summary>
        public bool IsInherited {
            get { return _source == ValueSource.Inherited; }
        }

        private enum ValueSource {
            DataBound,
            SystemResource,
            LocalDynamicResource,
            LocalStaticResource,
            TemplateBinding,
            CustomMarkupExtension,
            Local,
            DefaultValue,
            Inherited
        }
    }
}
