//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Activities
{
    using System.Globalization;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    static class ChannelCacheDefaults
    {
        internal const string DefaultIdleTimeoutString = "00:02:00";
        internal static TimeSpan DefaultIdleTimeout = TimeSpan.Parse(DefaultIdleTimeoutString, CultureInfo.InvariantCulture);
        
        internal const string DefaultMaxItemsPerCacheString = "16";
        internal static int DefaultMaxItemsPerCache = Int32.Parse(DefaultMaxItemsPerCacheString, CultureInfo.CurrentCulture);
        
        internal const string DefaultLeaseTimeoutString = "00:10:00";
        internal static TimeSpan DefaultLeaseTimeout = TimeSpan.Parse(DefaultLeaseTimeoutString, CultureInfo.InvariantCulture);

        internal const string DefaultFactoryLeaseTimeoutString = "Infinite";
        internal static TimeSpan DefaultFactoryLeaseTimeout = TimeSpan.MaxValue;
        
        internal const string DefaultChannelLeaseTimeoutString = "00:05:00";
        internal static TimeSpan DefaultChannelLeaseTimeout = TimeSpan.Parse(DefaultChannelLeaseTimeoutString, CultureInfo.InvariantCulture);
        
        internal const bool DefaultAllowUnsafeSharing = false;
    }
    
}
