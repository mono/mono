//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Security
{
    interface IChannelSecureConversationSessionSettings
    {
        TimeSpan KeyRenewalInterval
        {
            get;
            set;
        }

        TimeSpan KeyRolloverInterval
        {
            get;
            set;
        }

        bool TolerateTransportFailures
        {
            get;
            set;
        }
    }
}
