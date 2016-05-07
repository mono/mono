//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------
namespace System.ServiceModel
{
    using System.ServiceModel.Channels;
    public enum NetMsmqSecurityMode
    {
        None,
        Transport,
        Message,
        Both
    }

    static class NetMsmqSecurityModeHelper
    {
        internal static bool IsDefined(NetMsmqSecurityMode value)
        {
            return (value == NetMsmqSecurityMode.Transport
                || value == NetMsmqSecurityMode.Message
                || value == NetMsmqSecurityMode.Both
                || value == NetMsmqSecurityMode.None);
        }

        internal static NetMsmqSecurityMode ToSecurityMode(UnifiedSecurityMode value)
        {
            switch (value)
            {
                case UnifiedSecurityMode.None:
                    return NetMsmqSecurityMode.None;
                case UnifiedSecurityMode.Transport:
                    return NetMsmqSecurityMode.Transport;
                case UnifiedSecurityMode.Message:
                    return NetMsmqSecurityMode.Message;
                case UnifiedSecurityMode.Both:
                    return NetMsmqSecurityMode.Both;
                default:
                    return (NetMsmqSecurityMode)value;
            }
        }
    }
}


