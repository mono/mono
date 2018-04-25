//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Activities
{
    public sealed class ReceiveSettings
    {
        public ReceiveSettings()
        {
        }

        public string Action { get; set; }
        public bool CanCreateInstance { get; set; }
        public string OwnerDisplayName { get; set; }
    }
}
