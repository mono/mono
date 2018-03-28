//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    class PrefixEndpointAddressMessageFilterTable<TFilterData> : EndpointAddressMessageFilterTable<TFilterData>
    {
        UriPrefixTable<CandidateSet> toHostTable;
        UriPrefixTable<CandidateSet> toNoHostTable;

        public PrefixEndpointAddressMessageFilterTable()
            : base()
        {
        }

        protected override void InitializeLookupTables()
        {
            this.toHostTable = new UriPrefixTable<CandidateSet>();
            this.toNoHostTable = new UriPrefixTable<CandidateSet>();
        }

        public override void Add(MessageFilter filter, TFilterData data)
        {
            if (filter == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("filter");
            }

            Add((PrefixEndpointAddressMessageFilter)filter, data);
        }

        public override void Add(EndpointAddressMessageFilter filter, TFilterData data)
        {
            if (filter == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("filter");
            }

            Fx.Assert("EndpointAddressMessageFilter cannot be added to PrefixEndpointAddressMessageFilterTable");
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException("EndpointAddressMessageFilter cannot be added to PrefixEndpointAddressMessageFilterTable"));
        }

        public void Add(PrefixEndpointAddressMessageFilter filter, TFilterData data)
        {
            if (filter == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("filter");
            }

            this.filters.Add(filter, data);

            // Create the candidate
            byte[] mask = BuildMask(filter.HeaderLookup);
            Candidate can = new Candidate(filter, data, mask, filter.HeaderLookup);
            this.candidates.Add(filter, can);

#pragma warning suppress 56506 // Microsoft, PrefixEndpointAddressMessageFilter.Address can never be null
            Uri soapToAddress = filter.Address.Uri;

            CandidateSet cset;
            if (!TryMatchCandidateSet(soapToAddress, filter.IncludeHostNameInComparison, out cset))
            {
                cset = new CandidateSet();
                GetAddressTable(filter.IncludeHostNameInComparison).RegisterUri(soapToAddress, GetComparisonMode(filter.IncludeHostNameInComparison), cset);
            }
            cset.candidates.Add(can);

            IncrementQNameCount(cset, filter.Address);
        }

        HostNameComparisonMode GetComparisonMode(bool includeHostNameInComparison)
        {
            return includeHostNameInComparison ? HostNameComparisonMode.Exact : HostNameComparisonMode.StrongWildcard;
        }

        UriPrefixTable<CandidateSet> GetAddressTable(bool includeHostNameInComparison)
        {
            return includeHostNameInComparison ? this.toHostTable : this.toNoHostTable;
        }

        internal override bool TryMatchCandidateSet(Uri to, bool includeHostNameInComparison, out CandidateSet cset)
        {
            return GetAddressTable(includeHostNameInComparison).TryLookupUri(to, GetComparisonMode(includeHostNameInComparison), out cset);
        }

        protected override void ClearLookupTables()
        {
            this.toHostTable = new UriPrefixTable<EndpointAddressMessageFilterTable<TFilterData>.CandidateSet>();
            this.toNoHostTable = new UriPrefixTable<EndpointAddressMessageFilterTable<TFilterData>.CandidateSet>();
        }

        public override bool Remove(MessageFilter filter)
        {
            if (filter == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("filter");
            }

            PrefixEndpointAddressMessageFilter pFilter = filter as PrefixEndpointAddressMessageFilter;
            if (pFilter != null)
            {
                return Remove(pFilter);
            }

            return false;
        }

        public override bool Remove(EndpointAddressMessageFilter filter)
        {
            if (filter == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("filter");
            }

            Fx.Assert("EndpointAddressMessageFilter cannot be removed from PrefixEndpointAddressMessageFilterTable");
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException("EndpointAddressMessageFilter cannot be removed from PrefixEndpointAddressMessageFilterTable"));
        }

        public bool Remove(PrefixEndpointAddressMessageFilter filter)
        {
            if (filter == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("filter");
            }

            if (!this.filters.Remove(filter))
            {
                return false;
            }

            Candidate can = this.candidates[filter];
#pragma warning suppress 56506 // Microsoft, PrefixEndpointAddressMessageFilter.Address can never be null
            Uri soapToAddress = filter.Address.Uri;

            CandidateSet cset = null;
            if (TryMatchCandidateSet(soapToAddress, filter.IncludeHostNameInComparison, out cset))
            {
                if (cset.candidates.Count == 1)
                {
                    GetAddressTable(filter.IncludeHostNameInComparison).UnregisterUri(soapToAddress, GetComparisonMode(filter.IncludeHostNameInComparison));
                }
                else
                {
                    DecrementQNameCount(cset, filter.Address);

                    // Remove Candidate
                    cset.candidates.Remove(can);
                }
            }
            this.candidates.Remove(filter);

            RebuildMasks();
            return true;
        }
    }
}
