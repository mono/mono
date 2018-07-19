//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Dispatcher
{
    using System.ServiceModel.Channels;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.Collections.ObjectModel;
    using System.Collections.Generic;
    using System.Xml;

    class UniqueContractNameValidationBehavior : IServiceBehavior
    {
        Dictionary<XmlQualifiedName, ContractDescription> contracts = new Dictionary<XmlQualifiedName, ContractDescription>();

        public UniqueContractNameValidationBehavior() { }

        public void Validate(ServiceDescription description, ServiceHostBase serviceHostBase)
        {
            if (description == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("description");
            if (serviceHostBase == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("serviceHostBase");


            foreach (ServiceEndpoint endpoint in description.Endpoints)
            {
                XmlQualifiedName qname = new XmlQualifiedName(endpoint.Contract.Name, endpoint.Contract.Namespace);

                if (!contracts.ContainsKey(qname))
                {
                    contracts.Add(qname, endpoint.Contract);
                }
                else if (contracts[qname] != endpoint.Contract)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                        SR.GetString(SR.SFxMultipleContractsWithSameName, qname.Name, qname.Namespace)));
                }
            }
        }

        public void AddBindingParameters(ServiceDescription description, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection parameters)
        {
        }

        public void ApplyDispatchBehavior(ServiceDescription description, ServiceHostBase serviceHostBase)
        {
        }
    }
}
