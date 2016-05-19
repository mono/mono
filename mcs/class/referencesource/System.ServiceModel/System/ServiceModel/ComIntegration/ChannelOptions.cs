//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Runtime.InteropServices;
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    internal class ChannelOptions : IChannelOptions, IDisposable
    {
        protected IProvideChannelBuilderSettings channelBuilderSettings;

        internal ChannelOptions(IProvideChannelBuilderSettings channelBuilderSettings)
        {
            this.channelBuilderSettings = channelBuilderSettings;
        }
        internal static ComProxy Create(IntPtr outer, IProvideChannelBuilderSettings channelBuilderSettings)
        {

            if (channelBuilderSettings == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.CannotCreateChannelOption)));


            ChannelOptions channelOptions = null;
            ComProxy proxy = null;
            try
            {
                channelOptions = new ChannelOptions(channelBuilderSettings);
                proxy = ComProxy.Create(outer, channelOptions, channelOptions);
                return proxy;
            }
            finally
            {
                if (proxy == null)
                {
                    if (channelOptions != null)
                        ((IDisposable)channelOptions).Dispose();
                }

            }
        }
        void IDisposable.Dispose()
        {
        }
    }
}


