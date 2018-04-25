//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System.ComponentModel;
using System.Configuration;

namespace System.IdentityModel.Configuration
{
#pragma warning disable 1591
    public sealed partial class CustomTypeElement : ConfigurationElementInterceptor
    {
        public CustomTypeElement()
        {
        }

        internal CustomTypeElement( Type typeName )
        {
            this.Type = typeName;
        }

        public static T Resolve<T>( CustomTypeElement customTypeElement ) where T : class
        {
            return TypeResolveHelper.Resolve<T>( customTypeElement, customTypeElement.Type );
        }

        /// <summary>
        /// Returns a value indicating whether this element has been configured with non-default values.
        /// </summary>
        public bool IsConfigured
        {
            get
            {
                return ( ( ElementInformation.Properties[ConfigurationStrings.Type].ValueOrigin != PropertyValueOrigin.Default ) );
            }
        }

        [ConfigurationProperty( ConfigurationStrings.Type, IsRequired = true, IsKey = true )]
        [TypeConverter(typeof(System.Configuration.TypeNameConverter))]
        public Type Type
        {
            get { return (Type)this[ConfigurationStrings.Type]; }
            set { this[ConfigurationStrings.Type] = value; }
        }
    }
#pragma warning restore 1591
}
