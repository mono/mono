//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.ServiceModel.Channels;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.ServiceModel.Security;
    using System.Text;
    using System.Xml;

    using HeaderBit = System.ServiceModel.Dispatcher.EndpointAddressProcessor.HeaderBit;

    public class PrefixEndpointAddressMessageFilter : MessageFilter
    {
        EndpointAddress address;
        EndpointAddressMessageFilterHelper helper;
        UriPrefixTable<object> addressTable;
        HostNameComparisonMode hostNameComparisonMode;

        public PrefixEndpointAddressMessageFilter(EndpointAddress address)
            : this(address, false)
        {
        }

        public PrefixEndpointAddressMessageFilter(EndpointAddress address, bool includeHostNameInComparison)
        {
            if (address == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("address");
            }

            this.address = address;
            this.helper = new EndpointAddressMessageFilterHelper(this.address);

            this.hostNameComparisonMode = includeHostNameInComparison 
                ? HostNameComparisonMode.Exact
                : HostNameComparisonMode.StrongWildcard;

            this.addressTable = new UriPrefixTable<object>();
            this.addressTable.RegisterUri(this.address.Uri, hostNameComparisonMode, new object());
        }

        public EndpointAddress Address
        {
            get { return this.address; }
        }

        public bool IncludeHostNameInComparison
        {
            get { return (this.hostNameComparisonMode == HostNameComparisonMode.Exact); }
        }

        protected internal override IMessageFilterTable<FilterData> CreateFilterTable<FilterData>()
        {
            return new PrefixEndpointAddressMessageFilterTable<FilterData>();
        }

        public override bool Match(MessageBuffer messageBuffer)
        {
            if (messageBuffer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("messageBuffer");
            }

            Message msg = messageBuffer.CreateMessage();
            try
            {
                return Match(msg);
            }
            finally
            {
                msg.Close();
            }
        }

        public override bool Match(Message message)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }

            // To
#pragma warning suppress 56506 // Microsoft, Message.Headers can never be null
            Uri to = message.Headers.To;

            object o;
            if (to == null || !addressTable.TryLookupUri(to, this.hostNameComparisonMode, out o))
            {
                return false;
            }

            return helper.Match(message);
        }

        internal Dictionary<string, HeaderBit[]> HeaderLookup
        {
            get { return this.helper.HeaderLookup; }
        }

    }
}
