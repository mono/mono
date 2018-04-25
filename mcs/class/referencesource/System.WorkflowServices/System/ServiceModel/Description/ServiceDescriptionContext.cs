//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Description
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ServiceModel;

    class ServiceDescriptionContext
    {
        Dictionary<string, ContractDescription> contracts;
        IList<Type> reflectedContracts;
        ServiceDescription serviceDescription;
        Dictionary<KeyValuePair<Type, string>, WorkflowOperationBehavior> workflowOperationBehaviors;

        internal ServiceDescriptionContext()
        {
            this.contracts = new Dictionary<string, ContractDescription>();
            this.reflectedContracts = new List<Type>();
            this.workflowOperationBehaviors = new Dictionary<KeyValuePair<Type, string>, WorkflowOperationBehavior>();
        }

        public IDictionary<string, ContractDescription> Contracts
        {
            get
            {
                return this.contracts;
            }
        }

        public IList<Type> ReflectedContracts
        {
            get
            {
                return this.reflectedContracts;
            }
        }

        public ServiceDescription ServiceDescription
        {
            get
            {
                return this.serviceDescription;
            }
            set
            {
                this.serviceDescription = value;
            }
        }

        internal IDictionary<KeyValuePair<Type, string>, WorkflowOperationBehavior> WorkflowOperationBehaviors
        {
            get
            {
                return this.workflowOperationBehaviors;
            }
        }
    }
}
