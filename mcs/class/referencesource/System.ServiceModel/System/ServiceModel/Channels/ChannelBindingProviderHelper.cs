//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    class ChannelBindingProviderHelper : IChannelBindingProvider
    {
        public void EnableChannelBindingSupport()
        {
            this.IsChannelBindingSupportEnabled = true;
        }

        public bool IsChannelBindingSupportEnabled
        {
            get;
            private set;
        }
    }
}
