//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Diagnostics
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;

    class ServiceModelPerformanceCounters
    {
        Dictionary<string, OperationPerformanceCountersBase> operationPerfCounters;
        SortedList<string, string> actionToOperation;
        EndpointPerformanceCountersBase endpointPerfCounters;
        ServicePerformanceCountersBase servicePerfCounters;
        DefaultPerformanceCounters defaultPerfCounters;
        bool initialized;
        string perfCounterId;

        internal ServiceModelPerformanceCounters(
            ServiceHostBase serviceHost,
            ContractDescription contractDescription,
            EndpointDispatcher endpointDispatcher)
        {
            this.perfCounterId = endpointDispatcher.PerfCounterId;

            if (PerformanceCounters.Scope == PerformanceCounterScope.All)
            {
                this.operationPerfCounters = new Dictionary<string, OperationPerformanceCountersBase>(contractDescription.Operations.Count);
                this.actionToOperation = new SortedList<string, string>(contractDescription.Operations.Count);

                foreach (OperationDescription opDescription in contractDescription.Operations)
                {
                    Fx.Assert(null != opDescription.Messages, "OperationDescription.Messages should not be null");
                    Fx.Assert(opDescription.Messages.Count > 0, "OperationDescription.Messages should not be empty");
                    Fx.Assert(null != opDescription.Messages[0], "OperationDescription.Messages[0] should not be null");
                    if (null != opDescription.Messages[0].Action && !this.actionToOperation.Keys.Contains(opDescription.Messages[0].Action))
                    {
                        this.actionToOperation.Add(opDescription.Messages[0].Action, opDescription.Name);
                    }
                    OperationPerformanceCountersBase c;
                    if (!this.operationPerfCounters.TryGetValue(opDescription.Name, out c))
                    {
                        OperationPerformanceCountersBase counters =
                            PerformanceCountersFactory.CreateOperationCounters(serviceHost.Description.Name, contractDescription.Name, opDescription.Name, endpointDispatcher.PerfCounterBaseId);
                        if (counters != null && counters.Initialized)
                        {
                            this.operationPerfCounters.Add(opDescription.Name, counters);
                        }
                        else
                        {
                            // cleanup the others and return. 
                            this.initialized = false;
                            return;
                        }
                    }
                }

                // add endpoint scoped perf counters
                EndpointPerformanceCountersBase endpointCounters = PerformanceCountersFactory.CreateEndpointCounters(serviceHost.Description.Name, contractDescription.Name, endpointDispatcher.PerfCounterBaseId);
                if (endpointCounters != null && endpointCounters.Initialized)
                {
                    this.endpointPerfCounters = endpointCounters;
                }
            }

            if (PerformanceCounters.PerformanceCountersEnabled)
            {
                this.servicePerfCounters = serviceHost.Counters;
            }
            if (PerformanceCounters.MinimalPerformanceCountersEnabled)
            {
                this.defaultPerfCounters = serviceHost.DefaultCounters;
            }
            this.initialized = true;
        }

        internal OperationPerformanceCountersBase GetOperationPerformanceCountersFromMessage(Message message)
        {
            Fx.Assert(null != message, "message must not be null");
            Fx.Assert(null != message.Headers, "message headers must not be null");
            Fx.Assert(null != message.Headers.Action, "action must not be null");

            string operation;
            if (this.actionToOperation.TryGetValue(message.Headers.Action, out operation))
            {
                return this.GetOperationPerformanceCounters(operation);
            }
            else
            {
                return null;
            }
        }

        internal OperationPerformanceCountersBase GetOperationPerformanceCounters(string operation)
        {
            Fx.Assert(PerformanceCounters.Scope == PerformanceCounterScope.All, "Only call GetOparationPerformanceCounters when performance counter scope is All");

            OperationPerformanceCountersBase counters;
            Dictionary<string, OperationPerformanceCountersBase> opPerfCounters = this.operationPerfCounters;
            if (opPerfCounters != null && opPerfCounters.TryGetValue(operation, out counters))
            {
                return counters;
            }
            return null;
        }

        internal bool Initialized
        {
            get { return this.initialized; }
        }

        internal EndpointPerformanceCountersBase EndpointPerformanceCounters
        {
            get { return this.endpointPerfCounters; }
        }

        internal ServicePerformanceCountersBase ServicePerformanceCounters
        {
            get { return this.servicePerfCounters; }
        }

        internal DefaultPerformanceCounters DefaultPerformanceCounters
        {
            get { return this.defaultPerfCounters; }
        }

        internal string PerfCounterId
        {
            get { return this.perfCounterId; }
        }
    }


    internal class ServiceModelPerformanceCountersEntry
    {
        ServicePerformanceCountersBase servicePerformanceCounters;
        DefaultPerformanceCounters defaultPerformanceCounters;
        List<ServiceModelPerformanceCounters> performanceCounters;

        public ServiceModelPerformanceCountersEntry(ServicePerformanceCountersBase serviceCounters)
        {
            this.servicePerformanceCounters = serviceCounters;
            this.performanceCounters = new List<ServiceModelPerformanceCounters>();
        }

        public ServiceModelPerformanceCountersEntry(DefaultPerformanceCounters defaultServiceCounters)
        {
            this.defaultPerformanceCounters = defaultServiceCounters;
            this.performanceCounters = new List<ServiceModelPerformanceCounters>();
        }

        public void Add(ServiceModelPerformanceCounters counters)
        {
            this.performanceCounters.Add(counters);
        }

        public void Remove(string id)
        {
            for (int i = 0; i < this.performanceCounters.Count; ++i)
            {
                if (this.performanceCounters[i].PerfCounterId.Equals(id))
                {
                    this.performanceCounters.RemoveAt(i);
                    break;
                }
            }
        }

        public void Clear()
        {
            this.performanceCounters.Clear();
        }

        public ServicePerformanceCountersBase ServicePerformanceCounters
        {
            get { return this.servicePerformanceCounters; }
            set { this.servicePerformanceCounters = value; }
        }

        public DefaultPerformanceCounters DefaultPerformanceCounters
        {
            get { return this.defaultPerformanceCounters; }
            set { this.defaultPerformanceCounters = value; }
        }

        public List<ServiceModelPerformanceCounters> CounterList
        {
            get { return this.performanceCounters; }
        }
    }
}


