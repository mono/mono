//------------------------------------------------------------------------------
// <copyright file="ProtectedConfigurationProviderCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Configuration
{
    using System.Collections.Specialized;
    using System.Runtime.Serialization;
    using System.Configuration.Provider;
    using System.Xml;

    ////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////
    public class ProtectedConfigurationProviderCollection : ProviderCollection
    {
        public override void Add(ProviderBase provider)
        {
            if( provider == null )
            {
                throw new ArgumentNullException( "provider" );
            }

            if( !( provider is ProtectedConfigurationProvider ) )
           {
                throw new ArgumentException(SR.GetString(SR.Config_provider_must_implement_type, typeof(ProtectedConfigurationProvider).ToString()), "provider");
           }

            base.Add( provider );
        }

        new public ProtectedConfigurationProvider this[string name]
        {
            get
            {
                return (ProtectedConfigurationProvider)base[name];
            }
        }
    }
}
