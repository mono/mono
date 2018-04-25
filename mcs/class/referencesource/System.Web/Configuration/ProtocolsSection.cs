//------------------------------------------------------------------------------
// <copyright file="ProtocolsSection.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
//

namespace System.Web.Configuration 
{
    using System;
    using System.Xml;
    using System.Configuration;
    using System.Web.Configuration;
    using System.Collections.Specialized;
    using System.Collections;
    using System.IO;
    using System.Text;
    using System.Globalization;
    using System.Web.Hosting;
    using System.Web.Util;
    using System.Security.Permissions;

    public sealed class ProtocolsSection : ConfigurationSection
    {
        private static readonly ConfigurationPropertyCollection  _properties;

        #region Property Declarations
        private static readonly ConfigurationProperty   _propProtocols = 
            new ConfigurationProperty(null, typeof(ProtocolCollection), null, ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsDefaultCollection);
        #endregion

        static ProtocolsSection()
        {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();
            _properties.Add(_propProtocols);
        }
        public ProtocolsSection() 
        {
        }

        protected override ConfigurationPropertyCollection Properties 
        {
            get
            {
                return _properties;
            }
        }

        [ConfigurationProperty("protocols", IsRequired = true, IsDefaultCollection = true)]
        public ProtocolCollection Protocols 
        {
            get
            {
                return (ProtocolCollection)base[_propProtocols];
            }
        }
    }

    [ConfigurationCollection(typeof(ProtocolElement))]
    public sealed class ProtocolCollection : ConfigurationElementCollection
    {
        private static readonly ConfigurationPropertyCollection _properties;

        static ProtocolCollection()
        {
            _properties = new ConfigurationPropertyCollection();
        }
        public ProtocolCollection()
        {
        }

        protected override ConfigurationPropertyCollection Properties 
        {
            get
            {
                return _properties;
            }
        }

        public string[] AllKeys
        {
            get
            {
                return (string[])BaseGetAllKeys();
            }
        }

        public void Add( ProtocolElement protocolElement )
        {
            BaseAdd( protocolElement );
        }
        public void Remove( string name ) 
        {
            BaseRemove( name );
        }
        public void Remove( ProtocolElement protocolElement ) 
        { 
            BaseRemove( GetElementKey( protocolElement ) );
        }
        public void RemoveAt( int index )
        {
            BaseRemoveAt( index );
        }
        public new ProtocolElement this[ string name ] 
        {
            get 
            {
                return (ProtocolElement)BaseGet( name );
            }
        }
        public ProtocolElement this[ int index ] 
        {
            get
            {
                return (ProtocolElement)BaseGet( index );
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
            return new ProtocolElement();
        }

        protected override Object GetElementKey( ConfigurationElement element )
        {
            string name = ((ProtocolElement)element).Name;

            if ( string.IsNullOrEmpty( name ) )
            {
                throw new ArgumentException( SR.GetString(SR.Config_collection_add_element_without_key) );
            }

            return name;
        }
    }

    public sealed class ProtocolElement : ConfigurationElement
    {
        private static readonly ConfigurationPropertyCollection  _properties;

        #region Property Declarations
        private static readonly ConfigurationProperty   _propName = 
            new ConfigurationProperty( "name",
                                       typeof( string ),
                                       null,
                                       null,
                                       StdValidatorsAndConverters.NonEmptyStringValidator,
                                       ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey );

        private static readonly ConfigurationProperty   _propProcessHandlerType =
            new ConfigurationProperty( "processHandlerType",
                                       typeof( string ),
                                       null);
        private static readonly ConfigurationProperty   _propAppDomainHandlerType =
            new ConfigurationProperty( "appDomainHandlerType",
                                       typeof( string ),
                                       null);
        private static readonly ConfigurationProperty   _propValidate =
            new ConfigurationProperty( "validate",
                                       typeof( bool ),
                                       false);
        #endregion

        static ProtocolElement()
        {
            _properties = new ConfigurationPropertyCollection();
            _properties.Add( _propName );
            _properties.Add( _propProcessHandlerType );
            _properties.Add( _propAppDomainHandlerType );
            _properties.Add( _propValidate );
        }

        public ProtocolElement( string name ) 
        {
            if ( string.IsNullOrEmpty( name ) )
            {
                throw ExceptionUtil.ParameterNullOrEmpty("name");
            }

            base[ _propName ] = name;
        }

        public ProtocolElement()
        {
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
        
        [ConfigurationProperty("processHandlerType")]
        public string ProcessHandlerType
        {
            get 
            {
                return (string)base[ _propProcessHandlerType ];
            }
            set 
            {
                base[ _propProcessHandlerType ] = value;
            }
        }

        [ConfigurationProperty("appDomainHandlerType")]
        public string AppDomainHandlerType
        {
            get 
            {
                return (string)base[ _propAppDomainHandlerType ];
            }
            set 
            {
                base[ _propAppDomainHandlerType ] = value;
            }
        }

        [ConfigurationProperty("validate", DefaultValue = false)]
        public bool Validate
        {
            get 
            {
                return (bool)base[ _propValidate ];
            }
            set 
            {
                base[ _propValidate ] = value;
            }
        }

        private void ValidateTypes() {
             // check process protocol handler

            Type processHandlerType;
            try {
                 processHandlerType = Type.GetType(ProcessHandlerType, true /*throwOnError*/);
            }
            catch (Exception e) {
                throw new ConfigurationErrorsException(
                              e.Message, 
                              e, 
                              this.ElementInformation.Properties["ProcessHandlerType"].Source, 
                              this.ElementInformation.Properties["ProcessHandlerType"].LineNumber);
            }
            ConfigUtil.CheckAssignableType( typeof(ProcessProtocolHandler), processHandlerType, this, "ProcessHandlerType");

            // check app domain protocol handler

            Type appDomainHandlerType;
            try {
                 appDomainHandlerType = Type.GetType(AppDomainHandlerType, true /*throwOnError*/);
            }
            catch (Exception e) {
                throw new ConfigurationErrorsException(
                              e.Message,
                              e, 
                              this.ElementInformation.Properties["AppDomainHandlerType"].Source, 
                              this.ElementInformation.Properties["AppDomainHandlerType"].LineNumber);
            }
            ConfigUtil.CheckAssignableType( typeof(AppDomainProtocolHandler), appDomainHandlerType, this, "AppDomainHandlerType");
         }

        protected override void PostDeserialize()
        {
            if (Validate) {
                ValidateTypes();
            }
        }
    }
}   
