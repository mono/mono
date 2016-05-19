//------------------------------------------------------------------------------
// <copyright file="DeviceFiltersSection.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Web.UI.MobileControls;

namespace System.Web.Mobile {
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public sealed class DeviceFiltersSection : ConfigurationSection {
        internal static readonly TypeConverter              StdTypeNameConverter        = new MobileTypeNameConverter();
        internal static readonly ConfigurationValidatorBase NonEmptyStringValidator     = new StringValidator( 1 );

        private static ConfigurationPropertyCollection _properties;

        #region Property Declarations
        private static readonly ConfigurationProperty   _propFilters =
            new ConfigurationProperty( null,
                                       typeof( DeviceFilterElementCollection ),
                                       null,
                                       ConfigurationPropertyOptions.IsDefaultCollection );
        #endregion

        private object _deviceFilterslock = new object();
        private DeviceFilterDictionary _deviceFilters;

        static DeviceFiltersSection() {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();
            _properties.Add( _propFilters );
        }

        public DeviceFiltersSection() {
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        [ConfigurationProperty("", IsDefaultCollection = true)]
        public DeviceFilterElementCollection Filters {
            get {
                return (DeviceFilterElementCollection)base[ _propFilters ];
            }
        }

        internal DeviceFilterDictionary GetDeviceFilters() {
            if (_deviceFilters == null) {
                lock (_deviceFilterslock) {
                    if (_deviceFilters == null) {
                        _deviceFilters = CreateDeviceFilters();
                    }
                }
            }
            return _deviceFilters;
        }

        // Essentially this method does what MobileDeviceCapabilitiesSectionHandler.Create()
        // does, but use this DeviceFiltersSection for retrieving config data instead
        private DeviceFilterDictionary CreateDeviceFilters() {
            DeviceFilterDictionary filterDictionary = new DeviceFilterDictionary();

            foreach (DeviceFilterElement deviceFilter in Filters) {
                if (deviceFilter.FilterClass != null) {
                    filterDictionary.AddCapabilityDelegate(deviceFilter.Name, deviceFilter.GetDelegate());
                }
                else {
                    try {
                        filterDictionary.AddComparisonDelegate(
                            deviceFilter.Name, deviceFilter.Compare, deviceFilter.Argument);
                    }
                    catch (Exception e) {
                        throw new ConfigurationErrorsException(
                            SR.GetString(SR.DevCapSect_UnableAddDelegate, deviceFilter.Name, e.Message));
                    }
                }
            }

            return filterDictionary;
        }
    }


    [ConfigurationCollection(typeof(DeviceFilterElement), AddItemName = "filter",
     CollectionType = ConfigurationElementCollectionType.BasicMap)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public sealed class DeviceFilterElementCollection : ConfigurationElementCollection {
        private static readonly ConfigurationPropertyCollection _properties;

        static DeviceFilterElementCollection() {
            _properties = new ConfigurationPropertyCollection();
        }

        public DeviceFilterElementCollection() {
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        public object[] AllKeys {
            get {
                return BaseGetAllKeys();
            }
        }

        public void Add( DeviceFilterElement deviceFilter ) {
            BaseAdd( deviceFilter );
        }

        public void Remove( string name ) {
            BaseRemove( name );
        }

        public void Remove( DeviceFilterElement deviceFilter )  {
            BaseRemove( GetElementKey( deviceFilter ) );
        }

        public void RemoveAt( int index ) {
            BaseRemoveAt( index );
        }

        public new DeviceFilterElement this[ string name ]  {
            get {
                return (DeviceFilterElement)BaseGet( name );
            }
        }

        public DeviceFilterElement this[ int index ] {
            get {
                return (DeviceFilterElement)BaseGet( index );
            }
            set {
                if ( BaseGet( index ) != null) {
                    BaseRemoveAt( index );
                }

                BaseAdd( index, value );
            }
        }

        public void Clear() {
            BaseClear();
        }

        protected override ConfigurationElement CreateNewElement() {
            return new DeviceFilterElement();
        }

        protected override Object GetElementKey( ConfigurationElement element ) {
            return ( (DeviceFilterElement)element ).Name;
        }

        protected override string ElementName {
            get
            {
                return "filter";
            }
        }

        public override ConfigurationElementCollectionType CollectionType {
            get {
                return ConfigurationElementCollectionType.BasicMap;
            }
        }
    }

    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public sealed class DeviceFilterElement : ConfigurationElement {
        private static readonly ConfigurationElementProperty s_elemProperty = new ConfigurationElementProperty( new CallbackValidator( typeof( DeviceFilterElement ), ValidateElement ) );
        private static ConfigurationPropertyCollection _properties;

        #region Property Declarations
        private static readonly ConfigurationProperty   _propName =
            new ConfigurationProperty(  "name",
                                        typeof( string ),
                                        null,
                                        null,
                                        DeviceFiltersSection.NonEmptyStringValidator,
                                        ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey );

        private static readonly ConfigurationProperty   _propFilterClass =
            new ConfigurationProperty(  "type",
                                        typeof( Type ),
                                        null,
                                        DeviceFiltersSection.StdTypeNameConverter,
                                        null,
                                        ConfigurationPropertyOptions.IsTypeStringTransformationRequired);

        private static readonly ConfigurationProperty   _propMethod =
            new ConfigurationProperty(  "method",
                                        typeof( string ),
                                        null,
                                        null,
                                        DeviceFiltersSection.NonEmptyStringValidator,
                                        ConfigurationPropertyOptions.None );

        private static readonly ConfigurationProperty   _propCompare =
            new ConfigurationProperty(  "compare",
                                        typeof( string ),
                                        null,
                                        null,
                                        DeviceFiltersSection.NonEmptyStringValidator,
                                        ConfigurationPropertyOptions.None );

        private static readonly ConfigurationProperty   _propArgument =
            new ConfigurationProperty(  "argument",
                                        typeof( string ),
                                        null,
                                        null,
                                        DeviceFiltersSection.NonEmptyStringValidator,
                                        ConfigurationPropertyOptions.None );
        #endregion

        static DeviceFilterElement() {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();
            _properties.Add( _propName );
            _properties.Add( _propFilterClass );
            _properties.Add( _propMethod );
            _properties.Add( _propCompare );
            _properties.Add( _propArgument );
        }

        internal DeviceFilterElement() {
        }

        public DeviceFilterElement( string name, Type filterClass, string method ) {
            base[_propName] = name;
            base[_propFilterClass] = filterClass;
            base[_propMethod] = method;
        }

        public DeviceFilterElement( string name, string compareName, string argument ) {
            base[_propName] = name;
            base[_propCompare] = compareName;
            base[_propArgument] = argument;
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        [ConfigurationProperty("name", IsRequired = true, IsKey = true)]
        [StringValidator(MinLength = 1)]
        public string Name {
            get {
                return (string)base[ _propName ];
            }
        }

        [ConfigurationProperty("type")]
        [TypeConverter(typeof(MobileTypeNameConverter))]
        public Type FilterClass {
            get {
                return (Type)base[ _propFilterClass ];
            }
            set {
                base[ _propFilterClass ] = value;
            }
        }

        [ConfigurationProperty("method")]
        [StringValidator(MinLength = 1)]
        public string Method {
            get {
                return (string)base[ _propMethod ];
            }
            set {
                base[ _propMethod ] = value;
            }
        }

        [ConfigurationProperty("compare")]
        [StringValidator(MinLength = 1)]
        public string Compare {
            get {
                return (string)base[ _propCompare ];
            }
            set {
                base[ _propCompare ] = value;
            }
        }

        [ConfigurationProperty("argument")]
        [StringValidator(MinLength = 1)]
        public string Argument {
            get {
                return (string)base[ _propArgument ];
            }
            set {
                base[ _propArgument ] = value;
            }
        }

        protected override ConfigurationElementProperty ElementProperty
        {
            get
            {
                return s_elemProperty;
            }
        }

        internal MobileCapabilities.EvaluateCapabilitiesDelegate GetDelegate() {
            try {
                return (MobileCapabilities.EvaluateCapabilitiesDelegate)MobileCapabilities.EvaluateCapabilitiesDelegate.CreateDelegate(
                        typeof(MobileCapabilities.EvaluateCapabilitiesDelegate),
                        FilterClass,
                        Method);
            }
            catch (Exception e) {
                throw new ConfigurationErrorsException(
                            SR.GetString(SR.DevCapSect_NoCapabilityEval, Method, e.Message));
            }
        }

        static private void ValidateElement( object value ) {
            Debug.Assert((value != null) && (value is DeviceFilterElement));

            DeviceFilterElement elem = (DeviceFilterElement)value;

            // If the filter class is specified, we need the method attribute but
            // not the compare and argument attributes.
            if (elem.FilterClass != null) {
                if(string.IsNullOrEmpty(elem.Method)) {
                    throw new ConfigurationErrorsException(
                                SR.GetString(SR.ConfigSect_MissingAttr, "method"));
                }

                if (!string.IsNullOrEmpty(elem.Compare)) {
                    throw new ConfigurationErrorsException(SR.GetString(SR.DevCapSect_ExtraCompareDelegator));
                }
                else if (!string.IsNullOrEmpty(elem.Argument)) {
                    throw new ConfigurationErrorsException(SR.GetString(SR.DevCapSect_ExtraArgumentDelegator));
                }

                // Resolve the method
                elem.GetDelegate();
            }
            // Otherwise, we need the compare and argument attributes but not
            // the method attribute.
            else {
                if (string.IsNullOrEmpty(elem.Compare)) {
                    throw new ConfigurationErrorsException(
                                SR.GetString(SR.DevCapSect_MustSpecify));
                }

                if (!string.IsNullOrEmpty(elem.Method)) {
                    throw new ConfigurationErrorsException(
                                SR.GetString(SR.DevCapSect_ComparisonAlreadySpecified));
                }
            }
        }
    }
}



