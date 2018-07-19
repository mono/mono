//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.ServiceModel;

    class SharedHttpsTransportManager : SharedHttpTransportManager
    {
        static UriPrefixTable<ITransportManagerRegistration> transportManagerTable =
            new UriPrefixTable<ITransportManagerRegistration>(true);

        public SharedHttpsTransportManager(Uri listenUri, HttpChannelListener factory)
            : base(listenUri, factory)
        {
            // empty
        }

        internal override string Scheme
        {
            get 
            { 
                return Uri.UriSchemeHttps; 
            }
        }

        internal static UriPrefixTable<ITransportManagerRegistration> StaticTransportManagerTable
        {
            get { return transportManagerTable; }
        }

        internal override UriPrefixTable<ITransportManagerRegistration> TransportManagerTable
        {
            get
            {
                return transportManagerTable;
            }
        }
    }
}
