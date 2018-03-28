//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Activities
{
    using System.Net.Security;
    using System.Security.Principal;

    public sealed class SendSettings
    {
        public SendSettings()
        {
        }

        // Send settings
        public bool IsOneWay { get; set; }
        public Endpoint Endpoint { get; set; }
        public Uri EndpointAddress { get; set; }
        public string EndpointConfigurationName { get; set; }
        public TokenImpersonationLevel TokenImpersonationLevel { get; set; }
        public ProtectionLevel? ProtectionLevel { get; set; }
        public string OwnerDisplayName { get; set; }

        // Currently only used in SendReply case
        public bool RequirePersistBeforeSend { get; set; }
    }
}
