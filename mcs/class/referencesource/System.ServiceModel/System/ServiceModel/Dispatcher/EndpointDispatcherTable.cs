//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.ServiceModel.Diagnostics;
    using System.Threading;

    class EndpointDispatcherTable
    {
        MessageFilterTable<EndpointDispatcher> filters;
        object thisLock;
        const int optimizationThreshold = 2;
        List<EndpointDispatcher> cachedEndpoints;

        public EndpointDispatcherTable(object thisLock)
        {
            this.thisLock = thisLock;
        }

        public int Count
        {
            get
            {
                return ((this.cachedEndpoints != null) ? cachedEndpoints.Count : 0) +
                    ((this.filters != null) ? this.filters.Count : 0);
            }
        }

        object ThisLock
        {
            get { return thisLock; }
        }

        public void AddEndpoint(EndpointDispatcher endpoint)
        {
            lock (ThisLock)
            {
                MessageFilter filter = endpoint.EndpointFilter;
                int priority = endpoint.FilterPriority;

                if (filters == null)
                {
                    if (this.cachedEndpoints == null)
                    {
                        this.cachedEndpoints = new List<EndpointDispatcher>(optimizationThreshold);
                    }

                    if (this.cachedEndpoints.Count < optimizationThreshold)
                    {
                        this.cachedEndpoints.Add(endpoint);
                    }
                    else
                    {
                        filters = new MessageFilterTable<EndpointDispatcher>();
                        for (int i = 0; i < this.cachedEndpoints.Count; i++)
                        {
                            int cachedPriority = cachedEndpoints[i].FilterPriority;
                            MessageFilter cachedFilter = cachedEndpoints[i].EndpointFilter;
                            filters.Add(cachedFilter, cachedEndpoints[i], cachedPriority);
                        }
                        filters.Add(filter, endpoint, priority);
                        this.cachedEndpoints = null;
                    }
                }
                else
                {
                    filters.Add(filter, endpoint, priority);
                }
            }
        }
        
        public void RemoveEndpoint(EndpointDispatcher endpoint)
        {
            lock (ThisLock)
            {
                if (filters == null)
                {
                    if (cachedEndpoints != null && cachedEndpoints.Contains(endpoint))
                    {
                        cachedEndpoints.Remove(endpoint);
                    }
                }
                else
                {
                    MessageFilter filter = endpoint.EndpointFilter;
                    filters.Remove(filter);
                }
            }
        }

        EndpointDispatcher LookupInCache(Message message, out bool addressMatched)
        {
            EndpointDispatcher result = null;
            int priority = int.MinValue;
            bool duplicatePriority = false;
            addressMatched = false;

            if (this.cachedEndpoints != null && this.cachedEndpoints.Count > 0)
            {
                for (int i = 0; i < this.cachedEndpoints.Count; i++)
                {
                    EndpointDispatcher cachedEndpoint = cachedEndpoints[i];
                    int cachedPriority = cachedEndpoint.FilterPriority;
                    MessageFilter cachedFilter = cachedEndpoint.EndpointFilter;

                    bool matchResult;
                    AndMessageFilter andFilter = cachedFilter as AndMessageFilter;
                    if (andFilter != null)
                    {
                        bool addressResult;
                        matchResult = andFilter.Match(message, out addressResult);
                        addressMatched |= addressResult;
                    }
                    else
                    {
                        matchResult = cachedFilter.Match(message);
                    }
                    
                    if (matchResult)
                    {
                        addressMatched = true;
                        if (cachedPriority > priority || result == null)
                        {
                            result = cachedEndpoint;
                            priority = cachedPriority;
                            duplicatePriority = false;
                        }
                        else if (cachedPriority == priority && result != null)
                        {
                            duplicatePriority = true;
                        }
                    }
                }
            }

            if (duplicatePriority)
            {
                throw TraceUtility.ThrowHelperError(new MultipleFilterMatchesException(SR.GetString(SR.FilterMultipleMatches)), message);
            }

            return result;
        }

        public EndpointDispatcher Lookup(Message message, out bool addressMatched)
        {
            EndpointDispatcher data = null;

            data = LookupInCache(message, out addressMatched);

            if (data == null)
            {
                lock (ThisLock)
                {
                    data = LookupInCache(message, out addressMatched);

                    if (data == null && filters != null)
                    {
                        filters.GetMatchingValue(message, out data, out addressMatched);
                    }
                }
            }

            return data;
        }
    }
}
