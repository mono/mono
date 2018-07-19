//------------------------------------------------------------------------------
// <copyright file="MobileControlsSection.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;

namespace System.Web.UI.MobileControls
{
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public sealed class MobileControlsSection : ConfigurationSection
    {
        private ControlsConfig _controlConfig;
        private object _lock = new object();
        internal static readonly TypeConverter              StdTypeNameConverter        = new MobileTypeNameConverter();
        internal static readonly ConfigurationValidatorBase NonEmptyStringValidator     = new StringValidator( 1 );
        
        private static ConfigurationPropertyCollection _properties;

        #region Property Declarations
        private static readonly ConfigurationProperty   _propHistorySize = 
            new ConfigurationProperty(  "sessionStateHistorySize",
                                        typeof( int ),
                                        Constants.DefaultSessionsStateHistorySize,
                                        null,
                                        new IntegerValidator( 0 ,int.MaxValue),
                                        ConfigurationPropertyOptions.None );
        private static readonly ConfigurationProperty   _propDictType =
            new ConfigurationProperty(  "cookielessDataDictionaryType",
                                        typeof( Type ),
                                        typeof( System.Web.Mobile.CookielessData ),
                                        MobileControlsSection.StdTypeNameConverter,
                                        new SubclassTypeValidator( typeof( IDictionary ) ),
                                        ConfigurationPropertyOptions.None );
        private static readonly ConfigurationProperty   _propAllowCustomAttributes =
            new ConfigurationProperty(  "allowCustomAttributes",
                                        typeof( bool ),
                                        false,
                                        ConfigurationPropertyOptions.None );
        private static readonly ConfigurationProperty   _propDevices =
            new ConfigurationProperty( null,
                                       typeof( DeviceElementCollection ),
                                       null,
                                       ConfigurationPropertyOptions.IsDefaultCollection );
        #endregion

        static MobileControlsSection()
        {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();
            _properties.Add( _propHistorySize );
            _properties.Add( _propDevices );
            _properties.Add( _propDictType );
            _properties.Add( _propAllowCustomAttributes );
        }

        public MobileControlsSection()
        {
        }

        // VSWhidbey 450801. Only create one ControlsConfig per MobileControlsSection instance.
        internal ControlsConfig GetControlsConfig() {
            if (_controlConfig == null) {
                lock (_lock) {
                    if (_controlConfig == null) {
                        _controlConfig = MobileControlsSectionHelper.CreateControlsConfig(this);
                    }
                }
            }
            return _controlConfig;
        }

        protected override ConfigurationPropertyCollection Properties 
        {
            get
            {
                return _properties;
            }
        }

        [ConfigurationProperty("sessionStateHistorySize", DefaultValue = 6)]
        [IntegerValidator(MinValue = 0)]
        public int SessionStateHistorySize
        {
            get
            {
                return (int)base[ _propHistorySize ];
            }
            set
            {
                base[ _propHistorySize ] = value;
            }
        }

        [ConfigurationProperty("cookielessDataDictionaryType", DefaultValue = typeof(System.Web.Mobile.CookielessData))]
        [TypeConverter(typeof(MobileTypeNameConverter))]
        [SubclassTypeValidator(typeof(IDictionary))]
        public Type CookielessDataDictionaryType
        {
            get
            {
                return (Type)base[ _propDictType ];
            }
            set
            {
                base[ _propDictType ] = value;
            }          
        }

        [ConfigurationProperty("allowCustomAttributes", DefaultValue = false)]
        public bool AllowCustomAttributes
        {
            get
            {
                return (bool)base[ _propAllowCustomAttributes ];
            }
            set
            {
                base[ _propAllowCustomAttributes ] = value;
            }          
        }

        [ConfigurationProperty("", IsDefaultCollection = true)]
        public DeviceElementCollection Devices
        {
            get
            {
                return (DeviceElementCollection)base[ _propDevices ];
            }
        }
    }


    [ConfigurationCollection(typeof(DeviceElement), AddItemName="device")]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public sealed class DeviceElementCollection : ConfigurationElementCollection
    {
        private static readonly ConfigurationPropertyCollection _properties;

        static DeviceElementCollection()
        {
            _properties = new ConfigurationPropertyCollection();
        }
        public DeviceElementCollection()
        {
        }

        protected override ConfigurationPropertyCollection Properties 
        {
            get
            {
                return _properties;
            }   
        }

        public object[] AllKeys
        {
            get
            {
                return BaseGetAllKeys();
            }
        }

        public void Add( DeviceElement deviceElement )
        {
            BaseAdd( deviceElement );
        }

        public void Remove( string name ) 
        {
            BaseRemove( name );
        }
        public void Remove( DeviceElement deviceElement ) 
        { 
            BaseRemove( GetElementKey( deviceElement ) );
        }
        public void RemoveAt( int index )
        {
            BaseRemoveAt( index );
        }
        public new DeviceElement this[ string name ] 
        {
            get 
            {
                return (DeviceElement)BaseGet( name );
            }
        }
        public DeviceElement this[ int index ] 
        {
            get
            {
                return (DeviceElement)BaseGet( index );
            }
            set
            {
                if ( BaseGet( index ) != null)
                {
                    BaseRemoveAt( index );
                }

                BaseAdd( index, value );
            }
        }
        public void Clear()
        {
            BaseClear();
        }

        protected override ConfigurationElement CreateNewElement() 
        {
            return new DeviceElement();
        }

        protected override Object GetElementKey( ConfigurationElement element )
        {
            return ( (DeviceElement)element ).Name;
        }

        protected override string ElementName
        {
            get
            {
                return "device";
            }
        }

        protected override bool ThrowOnDuplicate
        {
            get
            {
                return true;
            }
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMapAlternate;
            }
        }
    }


    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public sealed class DeviceElement : ConfigurationElement
    {
        private static readonly ConfigurationElementProperty s_elemProperty = new ConfigurationElementProperty( new CallbackValidator( typeof( DeviceElement ), ValidateElement ) );
        private static ConfigurationPropertyCollection _properties;

        #region Property Declarations
        private static readonly ConfigurationProperty   _propName =
            new ConfigurationProperty(  "name", 
                                        typeof( string ),
                                        null,
                                        null,
                                        MobileControlsSection.NonEmptyStringValidator,
                                        ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey );
        private static readonly ConfigurationProperty   _propInheritsFrom = 
            new ConfigurationProperty(  "inheritsFrom", 
                                        typeof( string ),
                                        null,
                                        null,
                                        MobileControlsSection.NonEmptyStringValidator,
                                        ConfigurationPropertyOptions.None );
        private static readonly ConfigurationProperty   _propPredicateClass =
            new ConfigurationProperty(  "predicateClass",
                                        typeof( Type ),
                                        null,
                                        MobileControlsSection.StdTypeNameConverter,
                                        null,
                                        ConfigurationPropertyOptions.None );
        private static readonly ConfigurationProperty   _propPredicateMethod =
            new ConfigurationProperty(  "predicateMethod",
                                        typeof( string ),
                                        null,
                                        null,
                                        MobileControlsSection.NonEmptyStringValidator,
                                        ConfigurationPropertyOptions.None );
        private static readonly ConfigurationProperty   _propPageAdapter =
            new ConfigurationProperty(  "pageAdapter",
                                        typeof( Type ),
                                        null,
                                        MobileControlsSection.StdTypeNameConverter,
                                        new SubclassTypeValidator( typeof( IPageAdapter ) ),
                                        ConfigurationPropertyOptions.None );        
        private static readonly ConfigurationProperty _propControls =
            new ConfigurationProperty(  null,
                                        typeof(ControlElementCollection),
                                        null,
                                        ConfigurationPropertyOptions.IsDefaultCollection );
        #endregion

        static DeviceElement()
        {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();
            _properties.Add( _propName );
            _properties.Add( _propInheritsFrom );
            _properties.Add( _propPredicateClass );
            _properties.Add( _propPredicateMethod );
            _properties.Add( _propPageAdapter );
            _properties.Add( _propControls );
        }
        internal DeviceElement()
        {
        }
        public DeviceElement( string name, string inheritsFrom )
        {
            base[ _propName ]           = name;
            base[ _propInheritsFrom ]   = inheritsFrom;
        }

        public DeviceElement( string name, Type predicateClass, string predicateMethod, Type pageAdapter )
        {
            base[ _propName] = name;
            base[ _propPredicateClass] = predicateClass; 
            base[ _propPredicateMethod] = predicateMethod; 
            base[ _propPageAdapter ] = pageAdapter;
        }

        public DeviceElement(string name, string inheritsFrom, Type predicateClass,
                             string predicateMethod, Type pageAdapter)
        {
            base[ _propName] = name;
            base[ _propInheritsFrom ] = inheritsFrom;
            base[ _propPredicateClass] = predicateClass; 
            base[ _propPredicateMethod] = predicateMethod; 
            base[ _propPageAdapter ] = pageAdapter;
        }

        protected override ConfigurationPropertyCollection Properties 
        {
            get
            {
                return _properties;
            }
        }

        [ConfigurationProperty("name", IsRequired = true, IsKey = true)]
        [StringValidator(MinLength = 1)]
        public string Name
        {
            get
            {
                return (string)base[ _propName ];
            }
            set
            {
                base[ _propName ] = value;
            }
        }

        [ConfigurationProperty("inheritsFrom")]
        [StringValidator(MinLength = 1)]
        public string InheritsFrom
        {
            get
            {
                return (string)base[ _propInheritsFrom ];
            }
            set
            {
                base[ _propInheritsFrom ] = value;
            }
        }

        [ConfigurationProperty("predicateClass")]
        [TypeConverter(typeof(MobileTypeNameConverter))]
        public Type PredicateClass
        {
            get
            {
                return (Type)base[ _propPredicateClass ];
            }
            set
            {
                base[ _propPredicateClass ] = value;
            }
        }

        [ConfigurationProperty("predicateMethod")]
        [StringValidator(MinLength = 1)]
        public string PredicateMethod
        {
            get
            {
                return (string)base[ _propPredicateMethod ];
            }
            set
            {
                base[ _propPredicateMethod ] = value;
            }
        }

        [ConfigurationProperty("pageAdapter")]
        [TypeConverter(typeof(MobileTypeNameConverter))]
        [SubclassTypeValidator(typeof(IPageAdapter))]
        public Type PageAdapter
        {
            get
            {
                return (Type)base[_propPageAdapter];
            }
            set
            {
                base[_propPageAdapter] = value;
            }
        }

        [ConfigurationProperty("", IsDefaultCollection = true)]
        public ControlElementCollection Controls
        {
            get
            {
                return (ControlElementCollection)base[ _propControls ];
            }
        }

        protected override ConfigurationElementProperty ElementProperty
        {
            get
            {
                return s_elemProperty;
            }
        }

        internal IndividualDeviceConfig.DeviceQualifiesDelegate GetDelegate()
        {
            try
            {
                return (IndividualDeviceConfig.DeviceQualifiesDelegate)IndividualDeviceConfig.DeviceQualifiesDelegate.CreateDelegate(
                        typeof(IndividualDeviceConfig.DeviceQualifiesDelegate),
                        PredicateClass,
                        PredicateMethod );
            }
            catch
            {
                throw new ConfigurationErrorsException(
                    SR.GetString(SR.MobileControlsSectionHandler_CantCreateMethodOnClass, PredicateMethod, PredicateClass.FullName),
                                 ElementInformation.Source, ElementInformation.LineNumber);
            }
        }

        static private void ValidateElement( object value )
        {
            Debug.Assert( ( value != null ) && ( value is DeviceElement ) );

            DeviceElement elem = (DeviceElement)value;

            // If there is no inheritance the properties must exists and be valid
            if ( string.IsNullOrEmpty(elem.InheritsFrom) )
            {
                if ( elem.PredicateClass == null )
                {
                    throw new ConfigurationErrorsException( SR.GetString(SR.ConfigSect_MissingValue, "predicateClass"),
                                                            elem.ElementInformation.Source,
                                                            elem.ElementInformation.LineNumber );
                }

                if ( elem.PageAdapter == null )
                {
                    throw new ConfigurationErrorsException( SR.GetString(SR.ConfigSect_MissingValue, "pageAdapter"),
                                                            elem.ElementInformation.Source,
                                                            elem.ElementInformation.LineNumber );
                }

                // Resolve the method
                elem.GetDelegate();
            }
        }
    }

    [ConfigurationCollection(typeof(ControlElement), AddItemName = "control")]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public sealed class ControlElementCollection : ConfigurationElementCollection
    {
        private static readonly ConfigurationPropertyCollection _properties;

        static ControlElementCollection()
        {
            _properties = new ConfigurationPropertyCollection();
        }
        public ControlElementCollection()
        {
        }

        protected override ConfigurationPropertyCollection Properties 
        {
            get
            {
                return _properties;
            }   
        }

        public object[] AllKeys
        {
            get
            {
                return BaseGetAllKeys();
            }
        }

        public void Add( ControlElement controlElement )
        {
            BaseAdd( controlElement );
        }
        public void Remove( string name ) 
        {
            BaseRemove( name );
        }
        public void Remove( ControlElement controlElement ) 
        { 
            BaseRemove( GetElementKey( controlElement ) );
        }
        public void RemoveAt( int index )
        {
            BaseRemoveAt( index );
        }
        public new ControlElement this[ string name ] 
        {
            get 
            {
                return (ControlElement)BaseGet( name );
            }
        }
        public ControlElement this[ int index ] 
        {
            get
            {
                return (ControlElement)BaseGet( index );
            }
            set
            {
                if ( BaseGet( index ) != null)
                {
                    BaseRemoveAt( index );
                }

                BaseAdd( index, value );
            }
        }
        public void Clear()
        {
            BaseClear();
        }

        protected override ConfigurationElement CreateNewElement() 
        {
            return new ControlElement();
        }

        protected override Object GetElementKey( ConfigurationElement element )
        {
            return ( (ControlElement)element ).Name;
        }

        protected override string ElementName
        {
            get
            {
                return "control";
            }
        }

        protected override bool ThrowOnDuplicate
        {
            get
            {
                return true;
            }
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMap;
            }
        }
    }


    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public sealed class ControlElement : ConfigurationElement
    {
        private static readonly ConfigurationElementProperty    s_elemProperty      = new ConfigurationElementProperty( new CallbackValidator( typeof( ControlElement ), ValidateElement ) );
        private static readonly ConfigurationValidatorBase      s_SubclassTypeValidator = new SubclassTypeValidator( typeof( MobileControl ) );
        private static ConfigurationPropertyCollection _properties;
        

        #region Property Declarations
        private static readonly ConfigurationProperty   _propName =
            new ConfigurationProperty(  "name", 
                                        typeof( string ),
                                        null,
                                        null,
                                        MobileControlsSection.NonEmptyStringValidator,
                                        ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey );
        private static readonly ConfigurationProperty   _propAdapter = 
            new ConfigurationProperty(  "adapter", 
                                        typeof( Type ),
                                        null,
                                        MobileControlsSection.StdTypeNameConverter,
                                        new SubclassTypeValidator( typeof( IControlAdapter ) ),
                                        ConfigurationPropertyOptions.IsRequired );
        #endregion

        static ControlElement()
        {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();
            _properties.Add( _propName );
            _properties.Add( _propAdapter );
        }
        internal ControlElement()
        {
        }
        public ControlElement( string name, Type adapter )
        {
            base[ _propName]        = name;
            base[ _propAdapter ]    = adapter;
        }
        protected override ConfigurationPropertyCollection Properties 
        {
            get
            {
                return _properties;
            }
        }

        [ConfigurationProperty("name", IsRequired = true, IsKey = true)]
        [StringValidator(MinLength = 1)]
        public string Name
        {
            get
            {
                return (string)base[ _propName ];
            }
            set
            {
                base[ _propName ] = value;
            }
        }

        public Type Control
        {
            get
            {
                return Type.GetType( Name );
            }
            set
            {
                if ( value == null )
                {
                    throw new ArgumentNullException( "value" );
                }

                s_SubclassTypeValidator.Validate( value );
                Name = value.FullName;
            }
        }

        [ConfigurationProperty("adapter", IsRequired = true)]
        [TypeConverter(typeof(MobileTypeNameConverter))]
        [SubclassTypeValidator(typeof(IControlAdapter))]
        public Type Adapter
        {
            get
            {
                return (Type)base[ _propAdapter ];
            }
            set
            {
                base[ _propAdapter ] = value;
            }
        }

        protected override ConfigurationElementProperty ElementProperty
        {
            get
            {
                return s_elemProperty;
            }
        }

        static private void ValidateElement( object value )
        {
            Debug.Assert( ( value != null ) && ( value is ControlElement ) );

            ControlElement elem = (ControlElement)value;

            // Make sure Name is a valid type

            // This will throw if the type cannot be resolved
            Type tp = MobileControlsSection.StdTypeNameConverter.ConvertFromInvariantString( elem.Name ) as Type;

            // Validate that tp inherits from MobileControl
            s_SubclassTypeValidator.Validate( tp );
        }
    }

    // From old versions the default type names specified in mobile control config
    // section do not associate with assembly names.  So we cannot use
    // System.Configuration.TypeNameConverter as it wouldn't look up the type
    // names in the mobile assembly.  To workaround it, we create the same
    // converter here to be used in the mobile assembly.
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public sealed class MobileTypeNameConverter : ConfigurationConverterBase {

        public override object ConvertTo(ITypeDescriptorContext ctx, CultureInfo ci,
                                         object value, Type targetType) {
            Debug.Assert(targetType != null);
            Type valueType = value as Type;
            if (valueType == null) {
                throw new ArgumentException(SR.GetString(SR.MobileTypeNameConverter_UnsupportedValueType,
                                                          ((value == null) ? String.Empty : value.ToString()),
                                                          targetType.FullName));
            }

            return valueType.FullName;
        }

        public override object ConvertFrom(ITypeDescriptorContext ctx, CultureInfo ci, object data) {
            Debug.Assert(data is string);
            Type result = Type.GetType((string)data);
            if (result == null) {
                throw new ConfigurationErrorsException(
                    SR.GetString(SR.MobileTypeNameConverter_TypeNotResolved, (string)data));
            }

            return result;
        }
    }
}



