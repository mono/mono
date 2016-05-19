namespace System.ServiceModel.Activities.Description
{
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Runtime;
    using System.Security.Principal;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Activities.Dispatcher;
    using System.Xml;
    using System.ServiceModel.Security;
    using System.Net.Security;

    [Fx.Tag.XamlVisible(false)]
    public sealed class WorkflowInstanceManagementBehavior : IServiceBehavior
    {        
        public const string ControlEndpointAddress = "System.ServiceModel.Activities_IWorkflowInstanceManagement";
        
        static Binding httpBinding;
        static Binding namedPipeBinding;

        string windowsGroup;

        public WorkflowInstanceManagementBehavior() 
        {
            this.windowsGroup = GetDefaultBuiltinAdministratorsGroup();
        }

        public string WindowsGroup
        {
            get
            {
                return this.windowsGroup;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw FxTrace.Exception.ArgumentNullOrEmpty("WindowsGroup");
                }

                this.windowsGroup = value;
            }
        }

        public static Binding HttpControlEndpointBinding
        {
            get
            {                
                if (httpBinding == null)
                {
                    httpBinding = new WSHttpBinding
                    {
                        TransactionFlow = true,
                        Security = new WSHttpSecurity
                        {
                            Mode = SecurityMode.Message,
                            Message = new NonDualMessageSecurityOverHttp
                            {
                                ClientCredentialType = MessageCredentialType.Windows
                            }
                        }
                    };
                }

                return httpBinding;
            }
        }

        public static Binding NamedPipeControlEndpointBinding
        {
            get
            {                
                if (namedPipeBinding == null)
                {
                    namedPipeBinding = new NetNamedPipeBinding
                    {
                        TransactionFlow = true,
                        Security = new NetNamedPipeSecurity
                        {
                            Mode = NetNamedPipeSecurityMode.Transport,
                            Transport = new NamedPipeTransportSecurity
                            {
                                ProtectionLevel = ProtectionLevel.Sign
                            }
                        }
                    };
                }

                return namedPipeBinding;
            }
        }

        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
        {
            
        }

        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            if (serviceHostBase == null)
            {
                throw FxTrace.Exception.ArgumentNull("serviceHostBase");
            }

            WorkflowServiceHost workflowServiceHost = serviceHostBase as WorkflowServiceHost;

            if (workflowServiceHost != null)
            {
                CreateWorkflowManagementEndpoint(workflowServiceHost);
            }
        }

        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            
        }

        internal static string GetDefaultBuiltinAdministratorsGroup()
        {
            SecurityIdentifier identifier = new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null);
            NTAccount account = (NTAccount)identifier.Translate(typeof(NTAccount));
            return account.Value;
        }        

        void CreateWorkflowManagementEndpoint(WorkflowServiceHost workflowServiceHost)
        {
            Binding controlEndpointBinding; 
            if (workflowServiceHost.InternalBaseAddresses.Contains(Uri.UriSchemeNetPipe))
            {
                controlEndpointBinding = NamedPipeControlEndpointBinding;
            }
            else if (workflowServiceHost.InternalBaseAddresses.Contains(Uri.UriSchemeHttp))
            {
                controlEndpointBinding = HttpControlEndpointBinding;
            }
            else
            {
                return;
            }

            Uri controlEndpointAddress = ServiceHost.GetVia(controlEndpointBinding.Scheme, new Uri(ControlEndpointAddress, UriKind.Relative), workflowServiceHost.InternalBaseAddresses);            
            XmlQualifiedName contractName = new XmlQualifiedName(XD2.WorkflowInstanceManagementService.ContractName, XD2.WorkflowServices.Namespace);
            //Create the Endpoint Dispatcher
            EndpointAddress address = new EndpointAddress(controlEndpointAddress.AbsoluteUri);
            EndpointDispatcher endpointDispatcher = new EndpointDispatcher(address,
                XD2.WorkflowInstanceManagementService.ContractName,
                XD2.WorkflowServices.Namespace, true)
            {
                ContractFilter = new ActionMessageFilter(
                    NamingHelper.GetMessageAction(contractName, XD2.WorkflowInstanceManagementService.Abandon, null, false),
                    NamingHelper.GetMessageAction(contractName, XD2.WorkflowInstanceManagementService.Cancel, null, false),
                    NamingHelper.GetMessageAction(contractName, XD2.WorkflowInstanceManagementService.Run, null, false),
                    NamingHelper.GetMessageAction(contractName, XD2.WorkflowInstanceManagementService.Suspend, null, false),
                    NamingHelper.GetMessageAction(contractName, XD2.WorkflowInstanceManagementService.Terminate, null, false),
                    NamingHelper.GetMessageAction(contractName, XD2.WorkflowInstanceManagementService.TransactedCancel, null, false),
                    NamingHelper.GetMessageAction(contractName, XD2.WorkflowInstanceManagementService.TransactedRun, null, false),
                    NamingHelper.GetMessageAction(contractName, XD2.WorkflowInstanceManagementService.TransactedSuspend, null, false),
                    NamingHelper.GetMessageAction(contractName, XD2.WorkflowInstanceManagementService.TransactedTerminate, null, false),
                    NamingHelper.GetMessageAction(contractName, XD2.WorkflowInstanceManagementService.TransactedUnsuspend, null, false),
                    NamingHelper.GetMessageAction(contractName, XD2.WorkflowInstanceManagementService.TransactedUpdate, null, false),
                    NamingHelper.GetMessageAction(contractName, XD2.WorkflowInstanceManagementService.Unsuspend, null, false),
                    NamingHelper.GetMessageAction(contractName, XD2.WorkflowInstanceManagementService.Update, null, false)),
            };

            //Create Listener
            ServiceEndpoint endpoint = new ServiceEndpoint(WorkflowControlEndpoint.WorkflowControlServiceContract, controlEndpointBinding, address);
            BindingParameterCollection parameters = workflowServiceHost.GetBindingParameters(endpoint);

            IChannelListener listener;
            if (controlEndpointBinding.CanBuildChannelListener<IDuplexSessionChannel>(controlEndpointAddress, parameters))
            {
                listener = controlEndpointBinding.BuildChannelListener<IDuplexSessionChannel>(controlEndpointAddress, parameters);
            }
            else if (controlEndpointBinding.CanBuildChannelListener<IReplySessionChannel>(controlEndpointAddress, parameters))
            {
                listener = controlEndpointBinding.BuildChannelListener<IReplySessionChannel>(controlEndpointAddress, parameters);
            }
            else
            {
                listener = controlEndpointBinding.BuildChannelListener<IReplyChannel>(controlEndpointAddress, parameters);
            }

            //Add the operations
            bool formatRequest;
            bool formatReply;
            foreach (OperationDescription operation in WorkflowControlEndpoint.WorkflowControlServiceContract.Operations)
            {
                DataContractSerializerOperationBehavior dataContractSerializerOperationBehavior = new DataContractSerializerOperationBehavior(operation);

                DispatchOperation operationDispatcher = new DispatchOperation(endpointDispatcher.DispatchRuntime, operation.Name,
                    NamingHelper.GetMessageAction(operation, false), NamingHelper.GetMessageAction(operation, true))
                {
                    Formatter = (IDispatchMessageFormatter)dataContractSerializerOperationBehavior.GetFormatter(operation, out formatRequest, out formatReply, false),
                    Invoker = new ControlOperationInvoker(
                        operation,
                        new WorkflowControlEndpoint(controlEndpointBinding, address),
                        null,
                        workflowServiceHost),
                };
                endpointDispatcher.DispatchRuntime.Operations.Add(operationDispatcher);

                OperationBehaviorAttribute operationAttribute = operation.Behaviors.Find<OperationBehaviorAttribute>();
                ((IOperationBehavior)operationAttribute).ApplyDispatchBehavior(operation, operationDispatcher);
            }

            DispatchRuntime dispatchRuntime = endpointDispatcher.DispatchRuntime;
            dispatchRuntime.ConcurrencyMode = ConcurrencyMode.Multiple;
            dispatchRuntime.InstanceContextProvider = new DurableInstanceContextProvider(workflowServiceHost);
            dispatchRuntime.InstanceProvider = new DurableInstanceProvider(workflowServiceHost);
            dispatchRuntime.ServiceAuthorizationManager = new WindowsAuthorizationManager(this.WindowsGroup);

            //Create the Channel Dispatcher
            ServiceDebugBehavior serviceDebugBehavior = workflowServiceHost.Description.Behaviors.Find<ServiceDebugBehavior>();
            ServiceBehaviorAttribute serviceBehaviorAttribute = workflowServiceHost.Description.Behaviors.Find<ServiceBehaviorAttribute>();

            bool includeDebugInfo = false;
            if (serviceDebugBehavior != null)
            {
                includeDebugInfo |= serviceDebugBehavior.IncludeExceptionDetailInFaults;
            }
            if (serviceBehaviorAttribute != null)
            {
                includeDebugInfo |= serviceBehaviorAttribute.IncludeExceptionDetailInFaults;
            }

            ChannelDispatcher channelDispatcher = new ChannelDispatcher(listener, controlEndpointBinding.Name, controlEndpointBinding)
            {
                MessageVersion = controlEndpointBinding.MessageVersion,
                Endpoints = { endpointDispatcher },
                ServiceThrottle = workflowServiceHost.ServiceThrottle
            };
            workflowServiceHost.ChannelDispatchers.Add(channelDispatcher);
        }

        sealed class WindowsAuthorizationManager : ServiceAuthorizationManager
        {
            SecurityIdentifier sid;

            public WindowsAuthorizationManager(string windowsGroup)
            {
                NTAccount identity = new NTAccount(windowsGroup);

                try
                {
                    this.sid = identity.Translate(typeof(SecurityIdentifier)) as SecurityIdentifier;
                }
                catch (IdentityNotMappedException)
                {
                    throw FxTrace.Exception.Argument(windowsGroup, SR.WindowsGroupNotFound(windowsGroup));
                }
            }

            protected override bool CheckAccessCore(OperationContext operationContext)
            {
                WindowsPrincipal principal = new WindowsPrincipal(operationContext.ServiceSecurityContext.WindowsIdentity);

                bool isAuthorized = false;

                if (!operationContext.ServiceSecurityContext.IsAnonymous)
                {
                    isAuthorized = principal.IsInRole(sid);
                }

                return isAuthorized;
            }
        }
        
    }
}
