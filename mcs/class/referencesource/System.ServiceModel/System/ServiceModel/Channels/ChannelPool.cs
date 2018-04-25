//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Diagnostics;
    using System.Runtime;
    using System.ServiceModel;

    // Pool of channels used by OneWayChannelFactories
    class ChannelPool<TChannel> : IdlingCommunicationPool<ChannelPoolKey, TChannel>
        where TChannel : class, IChannel
    {
        static AsyncCallback onCloseComplete = Fx.ThunkCallback(new AsyncCallback(OnCloseComplete));

        public ChannelPool(ChannelPoolSettings settings)
            : base(settings.MaxOutboundChannelsPerEndpoint, settings.IdleTimeout, settings.LeaseTimeout)
        {
        }

        protected override void AbortItem(TChannel item)
        {
            item.Abort();
        }

        protected override void CloseItem(TChannel item, TimeSpan timeout)
        {
            item.Close(timeout);
        }

        protected override void CloseItemAsync(TChannel item, TimeSpan timeout)
        {
            bool succeeded = false;

            try
            {
                IAsyncResult result = item.BeginClose(timeout, onCloseComplete, item);

                if (result.CompletedSynchronously)
                {
                    item.EndClose(result);
                }

                succeeded = true;
            }
            finally
            {
                if (!succeeded)
                {
                    item.Abort();
                }
            }
        }

        protected override ChannelPoolKey GetPoolKey(EndpointAddress address, Uri via)
        {
            return new ChannelPoolKey(address, via);
        }

        static void OnCloseComplete(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
            {
                return;
            }

            TChannel item = (TChannel)result.AsyncState;
            bool succeeded = false;

            try
            {
                item.EndClose(result);
                succeeded = true;
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }

                DiagnosticUtility.TraceHandledException(e, TraceEventType.Warning);
            }
            finally
            {
                if (!succeeded)
                {
                    item.Abort();
                }
            }
        }
    }

    class ChannelPoolKey : IEquatable<ChannelPoolKey>
    {
        EndpointAddress address;
        Uri via;

        public ChannelPoolKey(EndpointAddress address, Uri via)
        {
            this.address = address;
            this.via = via;
        }

        public override int GetHashCode()
        {
            return address.GetHashCode() + via.GetHashCode();
        }

        public bool Equals(ChannelPoolKey other)
        {
            return address.EndpointEquals(other.address) && via.Equals(other.via);
        }
    }
}
