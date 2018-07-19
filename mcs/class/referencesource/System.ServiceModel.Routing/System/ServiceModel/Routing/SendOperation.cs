//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Routing
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.Transactions;
    using System.Runtime;
    using System.Configuration;

    class SendOperation
    {
        List<RoutingEndpointTrait> endpointTraits;
        int currentIndex;
        bool sent;
        Dictionary<string, Exception> exceptions;
        OperationContext operationContext;
        Type routerContract;

        public SendOperation(IEnumerable<ServiceEndpoint> endpoints, Type routerContract, OperationContext operationContext)
        {
            this.operationContext = operationContext;
            this.routerContract = routerContract;

            this.endpointTraits = new List<RoutingEndpointTrait>();
            foreach (ServiceEndpoint endpoint in endpoints)
            {
                this.endpointTraits.Add(new RoutingEndpointTrait(routerContract, endpoint, operationContext));
            }

            if (this.endpointTraits.Count == 0)
            {
                throw FxTrace.Exception.AsError(new ConfigurationErrorsException(SR.BackupListEmpty));
            }
        }

        public RoutingEndpointTrait CurrentEndpoint
        {
            get
            {
                Fx.Assert(this.currentIndex < this.endpointTraits.Count, "CurrentEndpoint should not be accessed after TryMoveToAlternate returned false!");

                RoutingEndpointTrait trait = this.endpointTraits[this.currentIndex];
                return trait;
            }
        }

        public bool HasAlternate
        {
            get { return this.currentIndex < (this.endpointTraits.Count - 1); }
        }

        public int AlternateEndpointCount
        {
            get { return (this.endpointTraits.Count - 1); }
        }

        public bool Sent
        {
            get { return this.sent; }
        }

        public void PrepareMessage(Message message)
        {
            if (this.exceptions != null)
            {
                message.Properties["Exceptions"] = this.exceptions;
            }
        }

        public void TransmitSucceeded(Transaction sendTransaction)
        {
            if (sendTransaction == null)
            {
                this.sent = true;
            }
        }

        public bool TryMoveToAlternate(Exception exception)
        {
            if (this.exceptions == null)
            {
                this.exceptions = new Dictionary<string, Exception>();
            }
            this.exceptions[this.CurrentEndpoint.Endpoint.Name] = exception;

            this.sent = false;
            if (++this.currentIndex < this.endpointTraits.Count)
            {
                return true;
            }
            return false;
        }
    }
}
