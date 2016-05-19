//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Diagnostics;

    public class EndpointDispatcher
    {
        MessageFilter addressFilter;
        bool addressFilterSetExplicit;
        ChannelDispatcher channelDispatcher;
        MessageFilter contractFilter;
        string contractName;
        string contractNamespace;
        ServiceChannel datagramChannel;
        DispatchRuntime dispatchRuntime;
        MessageFilter endpointFilter;
        int filterPriority;
        Uri listenUri;
        EndpointAddress originalAddress;
        string perfCounterId;
        string perfCounterBaseId;
        string id; // for ServiceMetadataBehavior, to help get EndpointIdentity of ServiceEndpoint from EndpointDispatcher
        bool isSystemEndpoint;

        internal EndpointDispatcher(EndpointAddress address, string contractName, string contractNamespace, string id, bool isSystemEndpoint)
            : this(address, contractName, contractNamespace)
        {
            this.id = id;
            this.isSystemEndpoint = isSystemEndpoint;
        }

        public EndpointDispatcher(EndpointAddress address, string contractName, string contractNamespace)
            : this(address, contractName, contractNamespace, false)
        {
        }

        public EndpointDispatcher(EndpointAddress address, string contractName, string contractNamespace, bool isSystemEndpoint)
        {
            this.originalAddress = address;
            this.contractName = contractName;
            this.contractNamespace = contractNamespace;

            if (address != null)
            {
                this.addressFilter = new EndpointAddressMessageFilter(address);
            }
            else
            {
                this.addressFilter = new MatchAllMessageFilter();
            }

            this.contractFilter = new MatchAllMessageFilter();
            this.dispatchRuntime = new DispatchRuntime(this);
            this.filterPriority = 0;
            this.isSystemEndpoint = isSystemEndpoint;
        }

        EndpointDispatcher(EndpointDispatcher baseEndpoint, IEnumerable<AddressHeader> headers)
        {
            EndpointAddressBuilder builder = new EndpointAddressBuilder(baseEndpoint.EndpointAddress);
            foreach (AddressHeader h in headers)
            {
                builder.Headers.Add(h);
            }
            EndpointAddress address = builder.ToEndpointAddress();

            this.addressFilter = new EndpointAddressMessageFilter(address);
            // channelDispatcher is Attached
            this.contractFilter = baseEndpoint.ContractFilter;
            this.contractName = baseEndpoint.ContractName;
            this.contractNamespace = baseEndpoint.ContractNamespace;
            this.dispatchRuntime = baseEndpoint.DispatchRuntime;
            // endpointFilter is lazy
            this.filterPriority = baseEndpoint.FilterPriority + 1;
            this.originalAddress = address;
            if (PerformanceCounters.PerformanceCountersEnabled)
            {
                this.perfCounterId = baseEndpoint.perfCounterId;
                this.perfCounterBaseId = baseEndpoint.perfCounterBaseId;
            }
            this.id = baseEndpoint.id;
        }

        public MessageFilter AddressFilter
        {
            get { return this.addressFilter; }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                this.ThrowIfDisposedOrImmutable();
                this.addressFilter = value;
                this.addressFilterSetExplicit = true;
            }
        }

        internal bool AddressFilterSetExplicit
        {
            get { return this.addressFilterSetExplicit; }
        }

        public ChannelDispatcher ChannelDispatcher
        {
            get { return this.channelDispatcher; }
        }

        public MessageFilter ContractFilter
        {
            get { return this.contractFilter; }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                this.ThrowIfDisposedOrImmutable();
                this.contractFilter = value;
            }
        }

        public string ContractName
        {
            get { return this.contractName; }
        }

        public string ContractNamespace
        {
            get { return this.contractNamespace; }
        }

        internal ServiceChannel DatagramChannel
        {
            get { return this.datagramChannel; }
            set { this.datagramChannel = value; }
        }

        public DispatchRuntime DispatchRuntime
        {
            get { return this.dispatchRuntime; }
        }

        internal Uri ListenUri
        {
            get { return this.listenUri; }
        }

        internal EndpointAddress OriginalAddress
        {
            get { return this.originalAddress; }
        }

        public EndpointAddress EndpointAddress
        {
            get
            {
                if (this.channelDispatcher == null)
                {
                    return this.originalAddress;
                }

                if ((this.originalAddress != null) && (this.originalAddress.Identity != null))
                {
                    return this.originalAddress;
                }

                IChannelListener listener = this.channelDispatcher.Listener;
                EndpointIdentity identity = listener.GetProperty<EndpointIdentity>();
                if ((this.originalAddress != null) && (identity == null))
                {
                    return this.originalAddress;
                }

                EndpointAddressBuilder builder;
                if (this.originalAddress != null)
                {
                    builder = new EndpointAddressBuilder(this.originalAddress);
                }
                else
                {
                    builder = new EndpointAddressBuilder();
                    builder.Uri = listener.Uri;
                }
                builder.Identity = identity;
                return builder.ToEndpointAddress();
            }
        }

        public bool IsSystemEndpoint
        {
            get { return this.isSystemEndpoint; }
        }

        internal MessageFilter EndpointFilter
        {
            get
            {
                if (this.endpointFilter == null)
                {
                    MessageFilter addressFilter = this.addressFilter;
                    MessageFilter contractFilter = this.contractFilter;

                    // Can't optimize addressFilter similarly.
                    // AndMessageFilter tracks when the address filter matched so the correct
                    // fault can be sent back.
                    if (contractFilter is MatchAllMessageFilter)
                    {
                        this.endpointFilter = addressFilter;
                    }
                    else
                    {
                        this.endpointFilter = new AndMessageFilter(addressFilter, contractFilter);
                    }
                }
                return this.endpointFilter;
            }
        }

        public int FilterPriority
        {
            get { return this.filterPriority; }
            set { this.filterPriority = value; }
        }

        internal string Id
        {
            get { return this.id; }
            set { this.id = value; }
        }

        internal string PerfCounterId
        {
            get { return this.perfCounterId; }
        }

        internal string PerfCounterBaseId
        {
            get { return this.perfCounterBaseId; }
        }

        internal int PerfCounterInstanceId { get; set; }

        static internal EndpointDispatcher AddEndpointDispatcher(EndpointDispatcher baseEndpoint,
                                                                 IEnumerable<AddressHeader> headers)
        {
            EndpointDispatcher endpoint = new EndpointDispatcher(baseEndpoint, headers);
            baseEndpoint.ChannelDispatcher.Endpoints.Add(endpoint);
            return endpoint;
        }

        internal void Attach(ChannelDispatcher channelDispatcher)
        {
            if (channelDispatcher == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("channelDispatcher");
            }

            if (this.channelDispatcher != null)
            {
                Exception error = new InvalidOperationException(SR.GetString(SR.SFxEndpointDispatcherMultipleChannelDispatcher0));
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(error);
            }

            this.channelDispatcher = channelDispatcher;
            this.listenUri = channelDispatcher.Listener.Uri;
        }

        internal void Detach(ChannelDispatcher channelDispatcher)
        {
            if (channelDispatcher == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("channelDispatcher");
            }

            if (this.channelDispatcher != channelDispatcher)
            {
                Exception error = new InvalidOperationException(SR.GetString(SR.SFxEndpointDispatcherDifferentChannelDispatcher0));
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(error);
            }

            this.ReleasePerformanceCounters();
            this.channelDispatcher = null;
        }

        internal void ReleasePerformanceCounters()
        {
            if (PerformanceCounters.PerformanceCountersEnabled)
            {
                PerformanceCounters.ReleasePerformanceCountersForEndpoint(this.perfCounterId, this.perfCounterBaseId);
            }
        }

        internal bool SetPerfCounterId()
        {
            Uri keyUri = null;
            if (null != this.ListenUri)
            {
                keyUri = this.ListenUri;
            }
            else
            {
                EndpointAddress endpointAddress = this.EndpointAddress;
                if (null != endpointAddress)
                {
                    keyUri = endpointAddress.Uri;
                }
            }

            if (null != keyUri)
            {
                this.perfCounterBaseId = keyUri.AbsoluteUri.ToUpperInvariant();
                this.perfCounterId = this.perfCounterBaseId + "/" + contractName.ToUpperInvariant();

                return true;
            }
            else
            {
                return false;
            }
        }

        void ThrowIfDisposedOrImmutable()
        {
            ChannelDispatcher channelDispatcher = this.channelDispatcher;
            if (channelDispatcher != null)
            {
                channelDispatcher.ThrowIfDisposedOrImmutable();
            }
        }
    }
}
