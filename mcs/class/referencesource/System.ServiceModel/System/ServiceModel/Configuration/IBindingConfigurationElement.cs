//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.ServiceModel.Channels;

    public interface IBindingConfigurationElement
    {
        TimeSpan CloseTimeout
        {
            get;
        }

        string Name
        {
            get;
        }

        TimeSpan OpenTimeout
        {
            get;
        }

        TimeSpan ReceiveTimeout
        {
            get;
        }

        TimeSpan SendTimeout
        {
            get;
        }

        void ApplyConfiguration(Binding binding);
    }
}
