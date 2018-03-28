//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Description
{

    using System.Xml;

    using System.ServiceModel.Channels;

    using System.Xml.Schema;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using WsdlNS = System.Web.Services.Description;

    // This class is created as part of the export process and passed to 
    // Wsdlmporter and WsdlExporter implementations as a utility for
    // Correlating between the WSDL OM and the Indigo OM
    // in the conversion process.    
    public class WsdlContractConversionContext
    {

        readonly ContractDescription contract;
        readonly WsdlNS.PortType wsdlPortType;

        readonly Dictionary<OperationDescription, WsdlNS.Operation> wsdlOperations;
        readonly Dictionary<WsdlNS.Operation, OperationDescription> operationDescriptions;
        readonly Dictionary<MessageDescription, WsdlNS.OperationMessage> wsdlOperationMessages;
        readonly Dictionary<FaultDescription, WsdlNS.OperationFault> wsdlOperationFaults;
        readonly Dictionary<WsdlNS.OperationMessage, MessageDescription> messageDescriptions;
        readonly Dictionary<WsdlNS.OperationFault, FaultDescription> faultDescriptions;
        readonly Dictionary<WsdlNS.Operation, Collection<WsdlNS.OperationBinding>> operationBindings;

        internal WsdlContractConversionContext(ContractDescription contract, WsdlNS.PortType wsdlPortType)
        {

            this.contract = contract;
            this.wsdlPortType = wsdlPortType;

            this.wsdlOperations = new Dictionary<OperationDescription, WsdlNS.Operation>();
            this.operationDescriptions = new Dictionary<WsdlNS.Operation, OperationDescription>();
            this.wsdlOperationMessages = new Dictionary<MessageDescription, WsdlNS.OperationMessage>();
            this.messageDescriptions = new Dictionary<WsdlNS.OperationMessage, MessageDescription>();
            this.wsdlOperationFaults = new Dictionary<FaultDescription, WsdlNS.OperationFault>();
            this.faultDescriptions = new Dictionary<WsdlNS.OperationFault, FaultDescription>();
            this.operationBindings = new Dictionary<WsdlNS.Operation, Collection<WsdlNS.OperationBinding>>();
        }

        internal IEnumerable<IWsdlExportExtension> ExportExtensions
        {
            get
            {
                foreach (IWsdlExportExtension extension in contract.Behaviors.FindAll<IWsdlExportExtension>())
                {
                    yield return extension;
                }

                foreach (OperationDescription operation in contract.Operations)
                {
                    if (!WsdlExporter.OperationIsExportable(operation))
                    {
                        continue;
                    }

                    // In 3.0SP1, the DCSOB and XSOB were moved from before to after the custom behaviors.  For
                    // IWsdlExportExtension compat, run them in the pre-SP1 order.
                    // TEF QFE 367607
                    Collection<IWsdlExportExtension> extensions = operation.Behaviors.FindAll<IWsdlExportExtension>();
                    for (int i = 0; i < extensions.Count; )
                    {
                        if (WsdlExporter.IsBuiltInOperationBehavior(extensions[i]))
                        {
                            yield return extensions[i];
                            extensions.RemoveAt(i);
                        }
                        else
                        {
                            i++;
                        }
                    }
                    foreach (IWsdlExportExtension extension in extensions)
                    {
                        yield return extension;
                    }
                }
            }
        }


        public ContractDescription Contract { get { return contract; } }
        public WsdlNS.PortType WsdlPortType { get { return wsdlPortType; } }

        public WsdlNS.Operation GetOperation(OperationDescription operation)
        {
            return this.wsdlOperations[operation];
        }

        public WsdlNS.OperationMessage GetOperationMessage(MessageDescription message)
        {
            return this.wsdlOperationMessages[message];
        }

        public WsdlNS.OperationFault GetOperationFault(FaultDescription fault)
        {
            return this.wsdlOperationFaults[fault];
        }

        public OperationDescription GetOperationDescription(WsdlNS.Operation operation)
        {
            return this.operationDescriptions[operation];
        }

        public MessageDescription GetMessageDescription(WsdlNS.OperationMessage operationMessage)
        {
            return this.messageDescriptions[operationMessage];
        }

        public FaultDescription GetFaultDescription(WsdlNS.OperationFault operationFault)
        {
            return this.faultDescriptions[operationFault];
        }

        // --------------------------------------------------------------------------------------------------

        internal void AddOperation(OperationDescription operationDescription, WsdlNS.Operation wsdlOperation)
        {
            this.wsdlOperations.Add(operationDescription, wsdlOperation);
            this.operationDescriptions.Add(wsdlOperation, operationDescription);
        }

        internal void AddMessage(MessageDescription messageDescription, WsdlNS.OperationMessage wsdlOperationMessage)
        {
            this.wsdlOperationMessages.Add(messageDescription, wsdlOperationMessage);
            this.messageDescriptions.Add(wsdlOperationMessage, messageDescription);
        }

        internal void AddFault(FaultDescription faultDescription, WsdlNS.OperationFault wsdlOperationFault)
        {
            this.wsdlOperationFaults.Add(faultDescription, wsdlOperationFault);
            this.faultDescriptions.Add(wsdlOperationFault, faultDescription);
        }

        internal Collection<WsdlNS.OperationBinding> GetOperationBindings(WsdlNS.Operation operation)
        {
            Collection<WsdlNS.OperationBinding> bindings;
            if (!this.operationBindings.TryGetValue(operation, out bindings))
            {
                bindings = new Collection<WsdlNS.OperationBinding>();
                WsdlNS.ServiceDescriptionCollection wsdlDocuments = WsdlPortType.ServiceDescription.ServiceDescriptions;
                foreach (WsdlNS.ServiceDescription wsdl in wsdlDocuments)
                {
                    foreach (WsdlNS.Binding wsdlBinding in wsdl.Bindings)
                    {
                        if (wsdlBinding.Type.Name == WsdlPortType.Name && wsdlBinding.Type.Namespace == WsdlPortType.ServiceDescription.TargetNamespace)
                        {
                            foreach (WsdlNS.OperationBinding operationBinding in wsdlBinding.Operations)
                            {
                                if (WsdlImporter.Binding2DescriptionHelper.Match(operationBinding, operation) != WsdlImporter.Binding2DescriptionHelper.MatchResult.None)
                                {
                                    bindings.Add(operationBinding);
                                    break;
                                }
                            }
                        }
                    }
                }
                this.operationBindings.Add(operation, bindings);
            }
            return bindings;
        }

    }

}
