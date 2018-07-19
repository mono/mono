//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Activities
{
    using System.Activities;
    using System.Activities.Debugger;
    using System.Activities.DynamicUpdate;
    using System.Activities.XamlIntegration;
    using System.Activities.Statements;
    using System.Activities.Validation;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.Collections;
    using System.ServiceModel.Activities.Description;
    using System.ServiceModel.Description;
    using System.ServiceModel.XamlIntegration;
    using System.Text;
    using System.Windows.Markup;
    using System.Xaml;
    using System.Xml;
    using System.Xml.Linq;

    [ContentProperty("Body")]
    public class WorkflowService : IDebuggableWorkflowTree 
    {
        Collection<Endpoint> endpoints;
        Collection<Type> implementedContracts;
        NullableKeyDictionary<WorkflowIdentity, DynamicUpdateMap> updateMaps;

        IDictionary<XName, ContractDescription> cachedInferredContracts;
        IDictionary<XName, Collection<CorrelationQuery>> correlationQueryByContract;
        IDictionary<ContractAndOperationNameTuple, OperationInfo> keyedByNameOperationInfo;

        IList<Receive> knownServiceActivities;
        HashSet<ReceiveAndReplyTuple> receiveAndReplyPairs;
        ServiceDescription serviceDescription;

        XName inferedServiceName;
        
        public WorkflowService()
        {
        }

        [DefaultValue(null)]
        public Activity Body
        {
            get;
            set;
        }

        [Fx.Tag.KnownXamlExternal]
        [DefaultValue(null)]
        [TypeConverter(typeof(ServiceXNameTypeConverter))]
        public XName Name
        {
            get;
            set;
        }

        [DefaultValue(null)]
        public string ConfigurationName
        {
            get;
            set;
        }

        [DefaultValue(false)]
        public bool AllowBufferedReceive
        {
            get;
            set;
        }

        public Collection<Endpoint> Endpoints
        {
            get
            {
                if (this.endpoints == null)
                {
                    this.endpoints = new Collection<Endpoint>();
                }
                return this.endpoints;
            }
        }

        [Fx.Tag.KnownXamlExternal]
        [DefaultValue(null)]
        public WorkflowIdentity DefinitionIdentity
        {
            get;
            set;
        }


        public Collection<Type> ImplementedContracts
        {
            get
            {
                if (this.implementedContracts == null)
                {
                    this.implementedContracts = new Collection<Type>();
                }
                return this.implementedContracts;
            }
        }

        public IDictionary<WorkflowIdentity, DynamicUpdateMap> UpdateMaps
        {
            get
            {
                if (this.updateMaps == null)
                {
                    this.updateMaps = new NullableKeyDictionary<WorkflowIdentity, DynamicUpdateMap>();
                }
                return this.updateMaps;
            }
        }

        internal bool HasImplementedContracts
        {
            get
            {
                return this.implementedContracts != null && this.implementedContracts.Count > 0;
            }
        }

        [DefaultValue(null)]
        internal Dictionary<OperationIdentifier, OperationProperty> OperationProperties
        {
            get;
            set;
        }
       
        IDictionary<ContractAndOperationNameTuple, OperationInfo> OperationsInfo
        {
            get
            {
                if (this.keyedByNameOperationInfo == null)
                {
                    GetContractDescriptions();
                }
                return this.keyedByNameOperationInfo;
            }
        }

        internal XName InternalName
        {
            get
            {
                if (this.Name != null)
                {
                    return this.Name;
                }
                else
                {
                    if (this.inferedServiceName == null)
                    {
                        Fx.Assert(this.Body != null, "Body cannot be null!");

                        if (this.Body.DisplayName.Length == 0)
                        {
                            throw FxTrace.Exception.AsError(new InvalidOperationException(SR.MissingDisplayNameInRootActivity));
                        }

                        this.inferedServiceName = XName.Get(XmlConvert.EncodeLocalName(this.Body.DisplayName));
                    }
                    return this.inferedServiceName;
                }
            }
        }

        internal IDictionary<XName, Collection<CorrelationQuery>> CorrelationQueries
        {
            get
            {
                Fx.Assert(this.cachedInferredContracts != null, "Must infer contract first!");
                return this.correlationQueryByContract;
            }
        }

        public Activity GetWorkflowRoot()
        {
            return this.Body;
        }

        internal ServiceDescription GetEmptyServiceDescription()
        {
            if (this.serviceDescription == null)
            {
                WalkActivityTree();

                ServiceDescription result = new ServiceDescription
                {
                    Name = this.InternalName.LocalName,
                    Namespace = string.IsNullOrEmpty(this.InternalName.NamespaceName) ? NamingHelper.DefaultNamespace : this.InternalName.NamespaceName,
                    ConfigurationName = this.ConfigurationName ?? this.InternalName.LocalName
                };                
                this.serviceDescription = result;
            }
            return this.serviceDescription;
        }

        static void AddAdditionalConstraint(ValidationSettings workflowServiceSettings, Type constraintType, Constraint constraint)
        {
            IList<Constraint> constraintList;
            if (workflowServiceSettings.AdditionalConstraints.TryGetValue(constraintType, out constraintList))
            {
                constraintList.Add(constraint);
            }
            else
            {
                constraintList = new List<Constraint>(1)
                    {
                        constraint,
                    };
                workflowServiceSettings.AdditionalConstraints.Add(constraintType, constraintList);
            }
        }

        public ValidationResults Validate(ValidationSettings settings)
        {
            Collection<ValidationError> errors = new Collection<ValidationError>();
            ValidationSettings workflowServiceSettings = this.CopyValidationSettions(settings);

            if (this.HasImplementedContracts)
            {
                this.OperationProperties = CreateOperationProperties(errors);

                // Add additional constraints
                AddAdditionalConstraint(workflowServiceSettings, typeof(Receive), GetContractFirstValidationReceiveConstraints());
                AddAdditionalConstraint(workflowServiceSettings, typeof(SendReply), GetContractFirstValidationSendReplyConstraints());
            }

            ValidationResults results = null;
            if (this.Body != null)
            {
                results = ActivityValidationServices.Validate(this.Body, workflowServiceSettings);

                if (!this.HasImplementedContracts)
                {
                    return results;
                }
                else
                {
                    // If the user specified implemented contract, then we need to add the errors into the error collection
                    foreach (ValidationError validationError in results.Errors)
                    {
                        errors.Add(validationError);
                    }

                    foreach (ValidationError validationWarning in results.Warnings)
                    {
                        errors.Add(validationWarning);
                    }
                }
            }

            if (this.HasImplementedContracts)
            {
                this.AfterValidation(errors);
            }

            return new ValidationResults(errors);
        }

        bool IsContractValid(ContractDescription contractDescription, Collection<ValidationError> validationError)
        {
            bool isValid = true; 
            if (contractDescription.IsDuplex())
            {
                validationError.Add(new ValidationError(SR.DuplexContractsNotSupported));
                isValid = false;
            }
            
            return isValid; 
        }

        ValidationSettings CopyValidationSettions(ValidationSettings source)
        {
            if ( source == null )
            {
                return new ValidationSettings();
            }

            ValidationSettings clonedSettings = new ValidationSettings
            {
                OnlyUseAdditionalConstraints = source.OnlyUseAdditionalConstraints,
                SingleLevel = source.SingleLevel,
                SkipValidatingRootConfiguration = source.SkipValidatingRootConfiguration,
                PrepareForRuntime = source.PrepareForRuntime,
                Environment = source.Environment,
                // Retain the same cancellation token. Otherwise we can't cancel the validation of WorkflowService objects
                // which can make the designer unreponsive if the validation takes a long time.
                CancellationToken = source.CancellationToken

            };

            foreach (KeyValuePair<Type, IList<Constraint>> constrants in source.AdditionalConstraints)
            {
                if (constrants.Key != null && constrants.Value != null)
                {
                    clonedSettings.AdditionalConstraints.Add(constrants.Key, new List<Constraint>(constrants.Value));
                }
            }

            return clonedSettings;
        }

        void AfterValidation(Collection<ValidationError> errors)
        {
            if (this.HasImplementedContracts)
            {
                Dictionary<OperationIdentifier, OperationProperty> operationProperties = this.OperationProperties;
                if (operationProperties != null)
                {
                    foreach (OperationProperty property in operationProperties.Values)
                    {
                        Fx.Assert(property.Operation != null, "OperationProperty.Operation should not be null!");

                        if (property.ImplementingReceives.Count < 1)
                        {
                            errors.Add(new ValidationError(SR.OperationIsNotImplemented(property.Operation.Name, property.Operation.DeclaringContract.Name), true));
                        }
                        else if (!property.Operation.IsOneWay)
                        {
                            foreach (Receive recv in property.ImplementingReceives)
                            {
                                if (!property.ImplementingSendRepliesRequests.Contains(recv))
                                {
                                    // passing the receive activity without a matching SendReply as the SourceDetail
                                    errors.Add(new ValidationError(SR.TwoWayIsImplementedAsOneWay(property.Operation.Name, property.Operation.DeclaringContract.Name), true, string.Empty, recv));
                                }
                            }                            
                        }                        
                    }
                }
            }
        }

        Dictionary<OperationIdentifier, OperationProperty> CreateOperationProperties(Collection<ValidationError> validationErrors)
        {
            Dictionary<OperationIdentifier, OperationProperty> operationProperties = null;

            if (this.HasImplementedContracts)
            {
                operationProperties = new Dictionary<OperationIdentifier, OperationProperty>();
                foreach (Type contractType in this.ImplementedContracts)
                {
                    ContractDescription contract = null;
                    try
                    {
                        contract = ContractDescription.GetContract(contractType);
                                                
                        if (contract != null)
                        {
                            if (this.IsContractValid(contract, validationErrors))
                            {
                                foreach (OperationDescription operation in contract.Operations)
                                {
                                    OperationIdentifier id = new OperationIdentifier(operation.DeclaringContract.Name, operation.DeclaringContract.Namespace, operation.Name);
                                    if (operationProperties.ContainsKey(id))
                                    {
                                        validationErrors.Add(new ValidationError(SR.DuplicatedContract(operation.DeclaringContract.Name, operation.Name), true));
                                    }
                                    else
                                    {
                                        operationProperties.Add(id, new OperationProperty(operation));
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                        {
                            throw;
                        }

                        validationErrors.Add(new ValidationError(exception.Message));
                    }
                }
            }

            return operationProperties;
        }
        
        public virtual IDictionary<XName, ContractDescription> GetContractDescriptions()
        {
            if (this.cachedInferredContracts == null)
            {
                WalkActivityTree();

                Fx.Assert(this.knownServiceActivities != null && this.receiveAndReplyPairs != null, "Failed to walk the activity tree!");
                this.correlationQueryByContract = new Dictionary<XName, Collection<CorrelationQuery>>();

                // Contract inference
                IDictionary<XName, ContractDescription> inferredContracts = new Dictionary<XName, ContractDescription>();
                this.keyedByNameOperationInfo = new Dictionary<ContractAndOperationNameTuple, OperationInfo>();

                foreach (Receive receive in this.knownServiceActivities)
                {
                    XName contractXName = FixServiceContractName(receive.ServiceContractName);
                    ContractAndOperationNameTuple tuple = new ContractAndOperationNameTuple(contractXName, receive.OperationName);

                    OperationInfo operationInfo;
                    if (this.keyedByNameOperationInfo.TryGetValue(tuple, out operationInfo))
                    {
                        // All Receives with same ServiceContractName and OperationName need to be validated
                        ContractValidationHelper.ValidateReceiveWithReceive(receive, operationInfo.Receive);
                    }
                    else
                    {
                        // Note that activities in keyedByNameOperationInfo are keyed by
                        // ServiceContractName and OperationName tuple. So we won't run into the case where
                        // two opertions have the same OperationName.

                        ContractDescription contract;
                        if (!inferredContracts.TryGetValue(contractXName, out contract))
                        {
                            // Infer Name, Namespace
                            contract = new ContractDescription(contractXName.LocalName, contractXName.NamespaceName);

                            // We use ServiceContractName.LocalName to bind contract with config
                            contract.ConfigurationName = contractXName.LocalName;

                            // We do NOT infer ContractDescription.ProtectionLevel

                            inferredContracts.Add(contractXName, contract);
                        }

                        OperationDescription operation = ContractInferenceHelper.CreateOperationDescription(receive, contract);
                        contract.Operations.Add(operation);

                        operationInfo = new OperationInfo(receive, operation);
                        this.keyedByNameOperationInfo.Add(tuple, operationInfo);
                    }

                    CorrectOutMessageForOperationWithFault(receive, operationInfo);

                    ContractInferenceHelper.UpdateIsOneWayFlag(receive, operationInfo.OperationDescription);

                    // FaultTypes and KnownTypes need to be collected from all Receive activities
                    ContractInferenceHelper.AddFaultDescription(receive, operationInfo.OperationDescription);
                    ContractInferenceHelper.AddKnownTypesToOperation(receive, operationInfo.OperationDescription);

                    // WorkflowFormatterBehavior should have reference to all the Receive activities
                    ContractInferenceHelper.AddReceiveToFormatterBehavior(receive, operationInfo.OperationDescription);

                    Collection<CorrelationQuery> correlationQueries = null;

                    // Collect CorrelationQuery from Receive
                    if (receive.HasCorrelatesOn || receive.HasCorrelationInitializers)
                    {
                        MessageQuerySet select = receive.HasCorrelatesOn ? receive.CorrelatesOn : null;
                        CorrelationQuery correlationQuery = ContractInferenceHelper.CreateServerCorrelationQuery(select,
                            receive.CorrelationInitializers, operationInfo.OperationDescription, false);
                        CollectCorrelationQuery(ref correlationQueries, contractXName, correlationQuery);
                    }

                    // Find all known Receive-Reply pair in the activity tree. Remove them from this.receiveAndReplyPairs
                    // Also collect CorrelationQuery from following replies
                    if (receive.HasReply)
                    {
                        foreach (SendReply reply in receive.FollowingReplies)
                        {
                            ReceiveAndReplyTuple pair = new ReceiveAndReplyTuple(receive, reply);
                            this.receiveAndReplyPairs.Remove(pair);

                            CollectCorrelationQueryFromReply(ref correlationQueries, contractXName,
                                reply, operationInfo.OperationDescription);

                            reply.SetContractName(contractXName);
                        }
                    }
                    if (receive.HasFault)
                    {
                        foreach (SendReply fault in receive.FollowingFaults)
                        {
                            ReceiveAndReplyTuple pair = new ReceiveAndReplyTuple(receive, fault);
                            this.receiveAndReplyPairs.Remove(pair);

                            CollectCorrelationQueryFromReply(ref correlationQueries, contractXName,
                                fault, operationInfo.OperationDescription);
                        }
                    }

                    // Have to do this here otherwise message/fault formatters 
                    // non-WorkflowServiceHost case won't be set. Cannot do this
                    // during CacheMetadata time because activity order in which 
                    // CacheMetadata calls are made doesn't yield the correct result.
                    // Not possible to do it at runtime either because the ToReply and
                    // ToRequest activities that use the formatters do not have access
                    // to related OperationDescription. Note: non-WorkflowServiceHosts will
                    // need to call GetContractDescriptions() to get these default formatters
                    // wired up.
                    receive.SetDefaultFormatters(operationInfo.OperationDescription);

                }

                // Check for Receive referenced by SendReply but no longer in the activity tree
                if (this.receiveAndReplyPairs.Count != 0)
                {
                    throw FxTrace.Exception.AsError(new ValidationException(SR.DanglingReceive));
                }

                // Print out tracing information
                if (TD.InferredContractDescriptionIsEnabled())
                {
                    foreach (ContractDescription contract in inferredContracts.Values)
                    {
                        TD.InferredContractDescription(contract.Name, contract.Namespace);

                        if (TD.InferredOperationDescriptionIsEnabled())
                        {
                            foreach (OperationDescription operation in contract.Operations)
                            {
                                TD.InferredOperationDescription(operation.Name, contract.Name, operation.IsOneWay.ToString());
                            }
                        }
                    }
                }

                this.cachedInferredContracts = inferredContracts;
            }

            return this.cachedInferredContracts;
        }

        internal void ValidateForVersioning(WorkflowService baseWorkflowService)
        {
            if (this.knownServiceActivities == null)
            {
                WalkActivityTree();
            }
            
            foreach (Receive receive in this.knownServiceActivities)
            {
                XName contractXName = FixServiceContractName(receive.ServiceContractName);
                ContractAndOperationNameTuple tuple = new ContractAndOperationNameTuple(contractXName, receive.OperationName);

                OperationInfo operationInfo;
                if (baseWorkflowService.OperationsInfo.TryGetValue(tuple, out operationInfo))
                {
                    // All Receives with same ServiceContractName and OperationName need to be validated
                    ContractValidationHelper.ValidateReceiveWithReceive(receive, operationInfo.Receive);
                    ContractInferenceHelper.AddReceiveToFormatterBehavior(receive, operationInfo.OperationDescription);
                    ContractInferenceHelper.UpdateIsOneWayFlag(receive, operationInfo.OperationDescription);
                }
                else
                {
                    throw FxTrace.Exception.AsError(new ValidationException(SR.OperationNotFound(contractXName, receive.OperationName)));
                }
            }
        }

        internal void DetachFromVersioning(WorkflowService baseWorkflowService)
        {
            if (this.knownServiceActivities == null)
            {
                return;
            }

            foreach (Receive receive in this.knownServiceActivities)
            {
                XName contractXName = FixServiceContractName(receive.ServiceContractName);
                ContractAndOperationNameTuple tuple = new ContractAndOperationNameTuple(contractXName, receive.OperationName);

                OperationInfo operationInfo;
                if (baseWorkflowService.OperationsInfo.TryGetValue(tuple, out operationInfo))
                {   
                    ContractInferenceHelper.RemoveReceiveFromFormatterBehavior(receive, operationInfo.OperationDescription);
                }
            }
        }

        void WalkActivityTree()
        {
            if (this.knownServiceActivities != null)
            {
                // We return if we have already walked the activity tree
                return;
            }

            if (this.Body == null)
            {
                throw FxTrace.Exception.AsError(new ValidationException(SR.MissingBodyInWorkflowService));
            }

            // Validate the activity tree
            ValidationResults validationResults = null;
            StringBuilder exceptionMessage = new StringBuilder();
            bool doesErrorExist = false; 
            try
            {
                if (this.HasImplementedContracts)
                {
                    validationResults = this.Validate(new ValidationSettings() { PrepareForRuntime = true, });
                }
                else
                {
                    WorkflowInspectionServices.CacheMetadata(this.Body);
                }                
            }
            catch (InvalidWorkflowException e)
            {
                doesErrorExist = true; 
                exceptionMessage.AppendLine(e.Message);
            }

            if (validationResults != null)
            {
                if (validationResults.Errors != null && validationResults.Errors.Count > 0)
                {
                    doesErrorExist = true; 
                    foreach (ValidationError error in validationResults.Errors)
                    {
                        exceptionMessage.AppendLine(error.Message);
                    }
                }
            }

            if (doesErrorExist)
            {
                throw FxTrace.Exception.AsError(new InvalidWorkflowException(exceptionMessage.ToString()));
            }

            this.knownServiceActivities = new List<Receive>();
            this.receiveAndReplyPairs = new HashSet<ReceiveAndReplyTuple>();

            // Now let us walk the tree here
            Queue<QueueItem> activities = new Queue<QueueItem>();
            // The root activity is never "in" a TransactedReceiveScope
            activities.Enqueue(new QueueItem(this.Body, null, null));
            while (activities.Count > 0)
            {
                QueueItem queueItem = activities.Dequeue();
                Fx.Assert(queueItem != null, "Queue item cannot be null");

                Activity activity = queueItem.Activity;
                Fx.Assert(queueItem.Activity != null, "Queue item's Activity cannot be null");

                Activity parentReceiveScope = queueItem.ParentReceiveScope;
                Activity rootTransactedReceiveScope = queueItem.RootTransactedReceiveScope;

                if (activity is Receive)  // First, let's see if this is a Receive activity
                {
                    Receive receive = (Receive)activity;

                    if (rootTransactedReceiveScope != null)
                    {
                        receive.InternalReceive.AdditionalData.IsInsideTransactedReceiveScope = true;
                        Fx.Assert(parentReceiveScope != null, "Internal error.. TransactedReceiveScope should be valid here");
                        if (IsFirstTransactedReceive(receive, parentReceiveScope, rootTransactedReceiveScope))
                        {
                            receive.InternalReceive.AdditionalData.IsFirstReceiveOfTransactedReceiveScopeTree = true;
                        }
                    }

                    this.knownServiceActivities.Add(receive);
                }
                else if (activity is SendReply)  // Let's see if this is a SendReply
                {
                    SendReply sendReply = (SendReply)activity;

                    Receive pairedReceive = sendReply.Request;
                    Fx.Assert(pairedReceive != null, "SendReply must point to a Receive!");

                    if (sendReply.InternalContent.IsFault)
                    {
                        pairedReceive.FollowingFaults.Add(sendReply);
                    }
                    else
                    {
                        if (pairedReceive.HasReply)
                        {
                            SendReply followingReply = pairedReceive.FollowingReplies[0];
                            ContractValidationHelper.ValidateSendReplyWithSendReply(followingReply, sendReply);
                        }

                        pairedReceive.FollowingReplies.Add(sendReply);
                    }

                    ReceiveAndReplyTuple tuple = new ReceiveAndReplyTuple(pairedReceive, sendReply);
                    this.receiveAndReplyPairs.Add(tuple);
                }

                // Enqueue child activities and delegates
                if (activity is TransactedReceiveScope)
                {
                    parentReceiveScope = activity;
                    if (rootTransactedReceiveScope == null)
                    {
                        rootTransactedReceiveScope = parentReceiveScope;
                    }
                }

                foreach (Activity childActivity in WorkflowInspectionServices.GetActivities(activity))
                {
                    QueueItem queueData = new QueueItem(childActivity, parentReceiveScope, rootTransactedReceiveScope);
                    activities.Enqueue(queueData);
                }
            }
        }

        XName FixServiceContractName(XName serviceContractName)
        {
            // By default, we use WorkflowService.Name as ServiceContractName
            XName contractXName = serviceContractName ?? this.InternalName;

            ContractInferenceHelper.ProvideDefaultNamespace(ref contractXName);

            return contractXName;
        }

        static void CorrectOutMessageForOperationWithFault(Receive receive, OperationInfo operationInfo)
        {
            Fx.Assert(receive != null && operationInfo != null, "Argument cannot be null!");

            Receive prevReceive = operationInfo.Receive;
            if (receive != prevReceive && receive.HasReply &&
                !prevReceive.HasReply && prevReceive.HasFault)
            {
                ContractInferenceHelper.CorrectOutMessageForOperation(receive, operationInfo.OperationDescription);
                operationInfo.Receive = receive;
            }
        }

        void CollectCorrelationQuery(ref Collection<CorrelationQuery> queries, XName serviceContractName, CorrelationQuery correlationQuery)
        {
            Fx.Assert(serviceContractName != null, "Argument cannot be null!");

            if (correlationQuery == null)
            {
                return;
            }

            if (queries == null && !this.correlationQueryByContract.TryGetValue(serviceContractName, out queries))
            {
                queries = new Collection<CorrelationQuery>();
                this.correlationQueryByContract.Add(serviceContractName, queries);
            }

            queries.Add(correlationQuery);
        }

        void CollectCorrelationQueryFromReply(ref Collection<CorrelationQuery> correlationQueries, XName serviceContractName,
            Activity reply, OperationDescription operation)
        {
            SendReply sendReply = reply as SendReply;
            if (sendReply != null)
            {
                CorrelationQuery correlationQuery = ContractInferenceHelper.CreateServerCorrelationQuery(
                    null, sendReply.CorrelationInitializers, operation, true);
                CollectCorrelationQuery(ref correlationQueries, serviceContractName, correlationQuery);
            }
        }

        internal void ResetServiceDescription()
        {
            this.serviceDescription = null;
            this.cachedInferredContracts = null;
        }

        bool IsFirstTransactedReceive(Receive request, Activity parent, Activity root)
        {
            Receive receive = null;
            if (parent != null && root != null)
            {
                TransactedReceiveScope rootTRS = root as TransactedReceiveScope;
                if (rootTRS != null)
                {
                    receive = rootTRS.Request;
                }
            }

            return (parent == root && receive == request);
        }
        
        Constraint GetContractFirstValidationReceiveConstraints()
        {
            DelegateInArgument<Receive> element = new DelegateInArgument<Receive> { Name = "ReceiveElement" };
            DelegateInArgument<ValidationContext> validationContext = new DelegateInArgument<ValidationContext> { Name = "validationContext" };
            Variable<IEnumerable<Activity>> parentChainVar = new Variable<IEnumerable<Activity>>("parentChainVar");
            return new Constraint<Receive>
            {
                Body = new ActivityAction<Receive, ValidationContext>
                {
                    Argument1 = element,
                    Argument2 = validationContext,
                    Handler = new Sequence
                    {
                        Variables = { parentChainVar },
                        Activities =
                        {
                            new GetParentChain { ValidationContext = validationContext, Result = parentChainVar },
                            new ValidateReceiveContract()
                            {
                                DisplayName = "ValidateReceiveContract",
                                ReceiveActivity = element,
                                WorkflowService = new InArgument<WorkflowService>()
                                {
                                    Expression = new GetWorkflowSerivce(this)
                                },
                                ParentChain = parentChainVar,
                            }
                        }
                    }
                }
            };
        }

        Constraint GetContractFirstValidationSendReplyConstraints()
        {
            DelegateInArgument<SendReply> element = new DelegateInArgument<SendReply> { Name = "ReceiveElement" };
            DelegateInArgument<ValidationContext> validationContext = new DelegateInArgument<ValidationContext> { Name = "validationContext" };

            return new Constraint<SendReply>
            {
                Body = new ActivityAction<SendReply, ValidationContext>
                {
                    Argument1 = element,
                    Argument2 = validationContext,
                    Handler = new Sequence
                    {
                        Activities =
                        {
                            new ValidateSendReplyContract()
                            {
                                DisplayName = "ValidateReceiveContract",
                                ReceiveActivity = element,
                                WorkflowSerivce = new InArgument<WorkflowService>()
                                {
                                    Expression = new GetWorkflowSerivce(this)
                                },
                                ValidationContext = validationContext
                            }
                        }
                    }
                }
            };
        }

        struct ContractAndOperationNameTuple : IEquatable<ContractAndOperationNameTuple>
        {
            XName serviceContractXName;
            string operationName;

            public ContractAndOperationNameTuple(XName serviceContractXName, string operationName)
            {
                this.serviceContractXName = serviceContractXName;
                this.operationName = operationName;
            }

            public bool Equals(ContractAndOperationNameTuple other)
            {
                return this.serviceContractXName == other.serviceContractXName && this.operationName == other.operationName; 
            }

            public override int GetHashCode()
            {
                int hashCode = 0;
                if (this.serviceContractXName != null)
                {
                    hashCode ^= this.serviceContractXName.GetHashCode();
                }

                hashCode ^= this.operationName.GetHashCode();

                return hashCode;
            }
        }

        struct ReceiveAndReplyTuple : IEquatable<ReceiveAndReplyTuple>
        {
            Receive receive;
            Activity reply;

            public ReceiveAndReplyTuple(Receive receive, SendReply reply)
            {
                this.receive = receive;
                this.reply = reply;
            }

            public bool Equals(ReceiveAndReplyTuple other)
            {
                return this.receive == other.receive && this.reply == other.reply;
            }

            public override int GetHashCode()
            {
                int hash = 0; 
                if (this.receive != null)
                {
                    hash ^= this.receive.GetHashCode();
                }

                if (this.reply != null)
                {
                    hash ^= this.reply.GetHashCode();
                }

                return hash;
            }
        }

        class OperationInfo
        {
            Receive receive;
            OperationDescription operationDescription;

            public OperationInfo(Receive receive, OperationDescription operationDescription)
            {
                this.receive = receive;
                this.operationDescription = operationDescription;
            }

            public Receive Receive
            {
                get { return this.receive; }
                set { this.receive = value; }
            }

            public OperationDescription OperationDescription
            {
                get { return this.operationDescription; }
            }
        }

        class QueueItem
        {
            Activity activity;
            Activity parent;
            Activity rootTransactedReceiveScope;

            public QueueItem(Activity element, Activity parent, Activity root)
            {
                this.activity = element;
                this.parent = parent;
                this.rootTransactedReceiveScope = root; 
            }

            public Activity Activity
            {
                get { return this.activity; }
            }

            public Activity ParentReceiveScope
            {
                get { return this.parent; }
            }

            public Activity RootTransactedReceiveScope
            {
                get { return this.rootTransactedReceiveScope; }
            }
        }

        class GetWorkflowSerivce : CodeActivity<WorkflowService>
        {
            WorkflowService workflowService; 
            public GetWorkflowSerivce(WorkflowService serviceReference)
            {
                workflowService = serviceReference;
            }

            protected override void CacheMetadata(CodeActivityMetadata metadata)
            {
                RuntimeArgument resultArgument = new RuntimeArgument("Result", typeof(WorkflowService), ArgumentDirection.Out);
                metadata.Bind(this.Result, resultArgument);

                metadata.SetArgumentsCollection(
                    new Collection<RuntimeArgument>
                {
                    resultArgument
                });
            }

            protected override WorkflowService Execute(CodeActivityContext context)
            {
                return workflowService;
            }
        }

        class ValidateReceiveContract : NativeActivity
        {
            public InArgument<Receive> ReceiveActivity
            {
                get;
                set;
            }

            public InArgument<IEnumerable<Activity>> ParentChain
            {
                get;
                set;
            }

            public InArgument<WorkflowService> WorkflowService
            {
                get;
                set; 
            }

            protected override void CacheMetadata(NativeActivityMetadata metadata)
            {
                RuntimeArgument receiveActivity = new RuntimeArgument("ReceiveActivity", typeof(Receive), ArgumentDirection.In);
                RuntimeArgument parentChain = new RuntimeArgument("ParentChain", typeof(IEnumerable<Activity>), ArgumentDirection.In);
                RuntimeArgument operationProperties = new RuntimeArgument("OperationProperties", typeof(WorkflowService), ArgumentDirection.In);

                if (this.ReceiveActivity == null)
                {
                    this.ReceiveActivity = new InArgument<Receive>();
                }
                metadata.Bind(this.ReceiveActivity, receiveActivity);

                if (this.ParentChain == null)
                {
                    this.ParentChain = new InArgument<IEnumerable<Activity>>();
                }
                metadata.Bind(this.ParentChain, parentChain);

                if (this.WorkflowService == null)
                {
                    this.WorkflowService = new InArgument<WorkflowService>();
                }
                metadata.Bind(this.WorkflowService, operationProperties);

                Collection<RuntimeArgument> arguments = new Collection<RuntimeArgument>
                {
                    receiveActivity,
                    parentChain,
                    operationProperties,
                };

                metadata.SetArgumentsCollection(arguments);
            }

            protected override void Execute(NativeActivityContext context)
            {
                Receive receiveActivity = this.ReceiveActivity.Get(context);
                Dictionary<OperationIdentifier, OperationProperty> operationProperties;

                Fx.Assert(receiveActivity != null, "ValidateReceiveContract needs the receive activity to be present");

                if (string.IsNullOrEmpty(receiveActivity.OperationName))
                {
                    Constraint.AddValidationError(context, new ValidationError(SR.MissingOperationName(this.DisplayName)));
                }
                else
                {
                    WorkflowService workflowService = this.WorkflowService.Get(context);
                    operationProperties = workflowService.OperationProperties;
                    XName serviceName = workflowService.FixServiceContractName(receiveActivity.ServiceContractName);
                    // We only do contract first validation if there are contract specified
                    if (operationProperties != null)
                    {
                        string contractName = serviceName.LocalName;
                        string contractNamespace = string.IsNullOrEmpty(serviceName.NamespaceName) ?
                            NamingHelper.DefaultNamespace : serviceName.NamespaceName;
                        string operationXmlName = NamingHelper.XmlName(receiveActivity.OperationName);

                        OperationProperty property;
                        OperationIdentifier operationId = new OperationIdentifier(contractName, contractNamespace, operationXmlName);
                        if (operationProperties.TryGetValue(operationId, out property))
                        {
                            property.ImplementingReceives.Add(receiveActivity);
                            Fx.Assert(property.Operation != null, "OperationProperty.Operation should not be null!");                            
                            ValidateContract(context, receiveActivity, property.Operation);
                        }
                        else
                        {
                            // It is OK to add a new contract, but we do not allow adding a new operation to a specified contract.
                            foreach (OperationIdentifier id in operationProperties.Keys)
                            {
                                if (contractName == id.ContractName && contractNamespace == id.ContractNamespace)
                                {
                                    Constraint.AddValidationError(context, new ValidationError(SR.OperationDoesNotExistInContract(receiveActivity.OperationName, contractName, contractNamespace)));
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            void ValidateTransactionBehavior(NativeActivityContext context, Receive receiveActivity, OperationDescription targetOperation)
            {
                TransactionFlowAttribute transactionFlowAttribute = targetOperation.Behaviors.Find<TransactionFlowAttribute>();
                Activity parent = null;

                // we know it's IList<Activity>
                IList<Activity> parentChain = (IList<Activity>)this.ParentChain.Get(context);
                if (parentChain.Count > 0)
                {
                    parent = parentChain[0];
                }

                bool isInTransactedReceiveScope = false;

                TransactedReceiveScope trs = parent as TransactedReceiveScope;
                if (trs != null && trs.Request == receiveActivity)
                {
                    isInTransactedReceiveScope = true;
                }

                if (transactionFlowAttribute != null)
                {
                    if (transactionFlowAttribute.Transactions == TransactionFlowOption.Mandatory)
                    {
                        if (targetOperation.IsOneWay)
                        {
                            Constraint.AddValidationError(context, new ValidationError(SR.TargetContractCannotBeOneWayWithTransactionFlow(targetOperation.Name, targetOperation.DeclaringContract.Name)));
                        }

                        // Receive has to be in a transacted receive scope
                        if (!isInTransactedReceiveScope)
                        {
                            Constraint.AddValidationError(context, new ValidationError(SR.ReceiveIsNotInTRS(targetOperation.Name, targetOperation.DeclaringContract.Name)));
                        }
                    }
                    else if (transactionFlowAttribute.Transactions == TransactionFlowOption.NotAllowed)
                    {
                        if (isInTransactedReceiveScope)
                        {
                            Constraint.AddValidationError(context, new ValidationError(SR.ReceiveIsInTRSWhenTransactionFlowNotAllowed(targetOperation.Name, targetOperation.DeclaringContract.Name), true));
                        }
                    }
                }
            }

            void ValidateContract(NativeActivityContext context, Receive receiveActivity, OperationDescription targetOperation)
            {
                SerializerOption targetSerializerOption = targetOperation.Behaviors.Contains(typeof(XmlSerializerOperationBehavior)) ?
                    SerializerOption.XmlSerializer : SerializerOption.DataContractSerializer;
                if (receiveActivity.SerializerOption != targetSerializerOption)
                {
                    Constraint.AddValidationError(context, new ValidationError(SR.PropertyMismatch(receiveActivity.SerializerOption.ToString(), "SerializerOption", targetSerializerOption.ToString(), targetOperation.DeclaringContract.Name, targetSerializerOption.ToString())));
                }

                if ((!targetOperation.HasProtectionLevel && receiveActivity.ProtectionLevel.HasValue && receiveActivity.ProtectionLevel != Net.Security.ProtectionLevel.None)
                    || (receiveActivity.ProtectionLevel.HasValue && receiveActivity.ProtectionLevel.Value != targetOperation.ProtectionLevel)
                    || (!receiveActivity.ProtectionLevel.HasValue && targetOperation.ProtectionLevel != Net.Security.ProtectionLevel.None))
                {
                    string targetProtectionLevelString = targetOperation.HasProtectionLevel ?
                        targetOperation.ProtectionLevel.ToString() : SR.NotSpecified;
                    string receiveProtectionLevelString = receiveActivity.ProtectionLevel.HasValue ? receiveActivity.ProtectionLevel.ToString() : SR.NotSpecified;
                    Constraint.AddValidationError(context, new ValidationError(SR.PropertyMismatch(receiveProtectionLevelString, "ProtectionLevel", targetProtectionLevelString, targetOperation.Name, targetOperation.DeclaringContract.Name)));
                }

                // We validate that all known types on the contract be present on the activity.
                // If activity contains more known types, we don't mind.
                if (targetOperation.KnownTypes.Count > 0)
                {
                    // We require that each Receive contains all the known types specified on the contract.
                    // Known type collections from multiple Receive activities with same contract name and operation name will NOT be merged.

                    // We expect the number of known types to be small, therefore we choose to use simple iterative search.
                    foreach (Type targetType in targetOperation.KnownTypes)
                    {
                        if (receiveActivity.KnownTypes == null || !receiveActivity.KnownTypes.Contains(targetType))
                        {
                            if (targetType != null && targetType != TypeHelper.VoidType)
                            {
                                Constraint.AddValidationError(context, new ValidationError(SR.MissingKnownTypes(targetType.FullName, targetOperation.Name, targetOperation.DeclaringContract.Name)));
                            }
                        }
                    }
                }

                this.ValidateTransactionBehavior(context, receiveActivity, targetOperation);
                receiveActivity.InternalContent.ValidateContract(context, targetOperation, receiveActivity, MessageDirection.Input);
            }
        }

        class ValidateSendReplyContract : NativeActivity
        {
            public InArgument<SendReply> ReceiveActivity
            {
                get;
                set;
            }

            public InArgument<ValidationContext> ValidationContext
            {
                get;
                set;
            }
            
            public InArgument<WorkflowService> WorkflowSerivce
            {
                get;
                set;
            }

            protected override void CacheMetadata(NativeActivityMetadata metadata)
            {
                RuntimeArgument receiveActivity = new RuntimeArgument("ReceiveActivity", typeof(SendReply), ArgumentDirection.In);
                RuntimeArgument validationContext = new RuntimeArgument("ValidationContext", typeof(ValidationContext), ArgumentDirection.In);
                RuntimeArgument operationProperties = new RuntimeArgument("OperationProperties", typeof(WorkflowService), ArgumentDirection.In);

                if (this.ReceiveActivity == null)
                {
                    this.ReceiveActivity = new InArgument<SendReply>();
                }
                metadata.Bind(this.ReceiveActivity, receiveActivity);

                if (this.ValidationContext == null)
                {
                    this.ValidationContext = new InArgument<ValidationContext>();
                }
                metadata.Bind(this.ValidationContext, validationContext);

                if (this.WorkflowSerivce == null)
                {
                    this.WorkflowSerivce = new InArgument<WorkflowService>();
                }
                metadata.Bind(this.WorkflowSerivce, operationProperties);

                Collection<RuntimeArgument> arguments = new Collection<RuntimeArgument>
                {
                    receiveActivity,
                    validationContext,
                    operationProperties
                };

                metadata.SetArgumentsCollection(arguments);
            }

            protected override void Execute(NativeActivityContext context)
            {
                SendReply sendReplyActivity = this.ReceiveActivity.Get(context);
                Dictionary<OperationIdentifier, OperationProperty> operationProperties;

                if (sendReplyActivity.Request != null)
                {
                    if (sendReplyActivity.Request.ServiceContractName != null && sendReplyActivity.Request.OperationName != null)
                    {
                        WorkflowService workflowService = this.WorkflowSerivce.Get(context);
                        operationProperties = workflowService.OperationProperties;
                        if (operationProperties != null)
                        {
                            XName contractXName = sendReplyActivity.Request.ServiceContractName;
                            string contractName = contractXName.LocalName;
                            string contractNamespace = string.IsNullOrEmpty(contractXName.NamespaceName) ?
                                NamingHelper.DefaultNamespace : contractXName.NamespaceName;
                            string operationXmlName = NamingHelper.XmlName(sendReplyActivity.Request.OperationName);

                            OperationProperty property;
                            OperationIdentifier id = new OperationIdentifier(contractName, contractNamespace, operationXmlName);
                            if (operationProperties.TryGetValue(id, out property))
                            {
                                if (!property.Operation.IsOneWay)
                                {
                                    property.ImplementingSendRepliesRequests.Add(sendReplyActivity.Request);
                                    Fx.Assert(property.Operation != null, "OperationProperty.Operation should not be null!");
                                    ValidateContract(context, sendReplyActivity, property.Operation);
                                }
                                else
                                {
                                    Constraint.AddValidationError(context, new ValidationError(SR.OnewayContractIsImplementedAsTwoWay(property.Operation.Name, contractName)));
                                }
                            }
                        }
                    }
                }
            }

            void ValidateContract(NativeActivityContext context, SendReply sendReply, OperationDescription targetOperation)
            {
                sendReply.InternalContent.ValidateContract(context, targetOperation, sendReply, MessageDirection.Output);
            }
        }
    }
}
