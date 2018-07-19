//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
#pragma warning disable 1634, 1691

namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Proxies;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;
    using System.Threading;

    class TypedServiceChannelBuilder : IProxyCreator, IProvideChannelBuilderSettings, ICreateServiceChannel
    {

        ServiceChannelFactory serviceChannelFactory = null;
        Type contractType = null;

        // Double-checked locking pattern requires volatile for read/write synchronization
        volatile RealProxy serviceProxy = null;
        ServiceEndpoint serviceEndpoint = null;
        KeyedByTypeCollection<IEndpointBehavior> behaviors = new KeyedByTypeCollection<IEndpointBehavior>();
        Binding binding = null;
        string configurationName = null;
        string address = null;
        EndpointIdentity identity = null;

        void IDisposable.Dispose()
        {
            if (serviceProxy != null)
            {
                IChannel channel = serviceProxy.GetTransparentProxy() as IChannel;
                if (channel == null)
                {
                    throw Fx.AssertAndThrow("serviceProxy MUST support IChannel");
                }
                channel.Close();
            }
        }

        //Suppressing PreSharp warning that property get methods should not throw
#pragma warning disable 6503
        ServiceChannelFactory IProvideChannelBuilderSettings.ServiceChannelFactoryReadWrite
        {
            get
            {
                if (serviceProxy != null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new COMException(SR.GetString(SR.TooLate), HR.RPC_E_TOO_LATE));
                return serviceChannelFactory;
            }
        }
#pragma warning restore 6503
        ServiceChannelFactory IProvideChannelBuilderSettings.ServiceChannelFactoryReadOnly
        {
            get
            {
                return serviceChannelFactory;
            }
        }
        //Suppressing PreSharp warning that property get methods should not throw
#pragma warning disable 6503
        KeyedByTypeCollection<IEndpointBehavior> IProvideChannelBuilderSettings.Behaviors
        {
            get
            {
                if (serviceProxy != null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new COMException(SR.GetString(SR.TooLate), HR.RPC_E_TOO_LATE));
                return behaviors;
            }
        }
#pragma warning restore 6503

        ServiceChannel IProvideChannelBuilderSettings.ServiceChannel
        {
            get
            {
                return null;
            }
        }

        RealProxy ICreateServiceChannel.CreateChannel()
        {
            if (serviceProxy == null)
            {
                lock (this)
                {
                    if (serviceProxy == null)
                    {
                        try
                        {
                            if (serviceChannelFactory == null)
                            {
                                FaultInserviceChannelFactory();
                            }

                            if (serviceChannelFactory == null)
                            {
                                throw Fx.AssertAndThrow("ServiceChannelFactory cannot be null at this point");
                            }

                            serviceChannelFactory.Open();

                            if (contractType == null)
                            {
                                throw Fx.AssertAndThrow("contractType cannot be null");
                            }
                            if (serviceEndpoint == null)
                            {
                                throw Fx.AssertAndThrow("serviceEndpoint cannot be null");
                            }

                            object transparentProxy = serviceChannelFactory.CreateChannel(contractType, new EndpointAddress(serviceEndpoint.Address.Uri, serviceEndpoint.Address.Identity, serviceEndpoint.Address.Headers), serviceEndpoint.Address.Uri);

                            ComPlusChannelCreatedTrace.Trace(TraceEventType.Verbose, TraceCode.ComIntegrationChannelCreated,
                                SR.TraceCodeComIntegrationChannelCreated, serviceEndpoint.Address.Uri, contractType);

                            RealProxy localProxy = RemotingServices.GetRealProxy(transparentProxy);

                            serviceProxy = localProxy;

                            if (serviceProxy == null)
                            {
                                throw Fx.AssertAndThrow("serviceProxy MUST derive from RealProxy");
                            }
                        }
                        finally
                        {
                            if ((serviceProxy == null) && (serviceChannelFactory != null))
                                serviceChannelFactory.Close();
                        }
                    }
                }
            }
            return serviceProxy;
        }

        private ServiceEndpoint CreateServiceEndpoint()
        {
            TypeLoader loader = new TypeLoader();
            ContractDescription contractDescription = loader.LoadContractDescription(contractType);

            ServiceEndpoint endpoint = new ServiceEndpoint(contractDescription);
            if (address != null)
                endpoint.Address = new EndpointAddress(new Uri(address), identity);
            if (binding != null)
                endpoint.Binding = binding;

            if (configurationName != null)
            {
                ConfigLoader configLoader = new ConfigLoader();
                configLoader.LoadChannelBehaviors(endpoint, configurationName);
            }

            ComPlusTypedChannelBuilderTrace.Trace(TraceEventType.Verbose, TraceCode.ComIntegrationTypedChannelBuilderLoaded,
                SR.TraceCodeComIntegrationTypedChannelBuilderLoaded, contractType, binding);

            return endpoint;
        }

        private ServiceChannelFactory CreateServiceChannelFactory()
        {
            ServiceChannelFactory serviceChannelFactory = ServiceChannelFactory.BuildChannelFactory(serviceEndpoint) as ServiceChannelFactory;
            if (serviceChannelFactory == null)
            {
                throw Fx.AssertAndThrow("We should get a ServiceChannelFactory back");
            }
            return serviceChannelFactory;
        }

        void FaultInserviceChannelFactory()
        {
            if (contractType == null)
            {
                throw Fx.AssertAndThrow("contractType should not be null");
            }
            if (serviceEndpoint == null)
            {
                serviceEndpoint = CreateServiceEndpoint();
            }
            foreach (IEndpointBehavior behavior in behaviors)
                serviceEndpoint.Behaviors.Add(behavior);
            serviceChannelFactory = CreateServiceChannelFactory();

        }

        internal void ResolveTypeIfPossible(Dictionary<MonikerHelper.MonikerAttribute, string> propertyTable)
        {
            string typeIID;
            propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.Contract, out typeIID);
            Guid iid;
            if (!string.IsNullOrEmpty(typeIID))
            {
                try
                {
                    dispatchEnabled = true;
                    iid = new Guid(typeIID);
                    TypeCacheManager.Provider.FindOrCreateType(iid, out contractType, true, false);
                    serviceEndpoint = CreateServiceEndpoint();
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                        throw;

                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MonikerSyntaxException(SR.GetString(SR.TypeLoadForContractTypeIIDFailedWith, typeIID, e.Message)));
                }

            }
        }

        internal TypedServiceChannelBuilder(Dictionary<MonikerHelper.MonikerAttribute, string> propertyTable)
        {
            string bindingType = null;
            string bindingConfigName = null;

            string spnIdentity = null;
            string upnIdentity = null;
            string dnsIdentity = null;

            propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.Address, out address);
            propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.Binding, out bindingType);
            propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.BindingConfiguration, out bindingConfigName);
            propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.SpnIdentity, out spnIdentity);
            propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.UpnIdentity, out upnIdentity);
            propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.DnsIdentity, out dnsIdentity);

            if (!string.IsNullOrEmpty(bindingType))
            {
                try
                {
                    binding = ConfigLoader.LookupBinding(bindingType, bindingConfigName);
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                        throw;

                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MonikerSyntaxException(SR.GetString(SR.BindingLoadFromConfigFailedWith, bindingType, e.Message)));
                }
                if (binding == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MonikerSyntaxException(SR.GetString(SR.BindingNotFoundInConfig, bindingType, bindingConfigName)));

            }

            if (binding == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MonikerSyntaxException(SR.GetString(SR.BindingNotSpecified)));

            if (string.IsNullOrEmpty(address))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MonikerSyntaxException(SR.GetString(SR.AddressNotSpecified)));

            if (!string.IsNullOrEmpty(spnIdentity))
            {
                if ((!string.IsNullOrEmpty(upnIdentity)) || (!string.IsNullOrEmpty(dnsIdentity)))
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MonikerSyntaxException(SR.GetString(SR.MonikerIncorrectServerIdentity)));
                identity = EndpointIdentity.CreateSpnIdentity(spnIdentity);
            }
            else if (!string.IsNullOrEmpty(upnIdentity))
            {
                if ((!string.IsNullOrEmpty(spnIdentity)) || (!string.IsNullOrEmpty(dnsIdentity)))
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MonikerSyntaxException(SR.GetString(SR.MonikerIncorrectServerIdentity)));
                identity = EndpointIdentity.CreateUpnIdentity(upnIdentity);
            }
            else if (!string.IsNullOrEmpty(dnsIdentity))
            {
                if ((!string.IsNullOrEmpty(spnIdentity)) || (!string.IsNullOrEmpty(upnIdentity)))
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MonikerSyntaxException(SR.GetString(SR.MonikerIncorrectServerIdentity)));
                identity = EndpointIdentity.CreateDnsIdentity(dnsIdentity);
            }
            else
                identity = null;
            ResolveTypeIfPossible(propertyTable);
        }


        bool dispatchEnabled = false;
        private bool CheckDispatch(ref Guid riid)
        {
            if ((dispatchEnabled) && (riid == InterfaceID.idIDispatch))
                return true;
            else
                return false;
        }

        ComProxy IProxyCreator.CreateProxy(IntPtr outer, ref Guid riid)
        {
            if (outer == IntPtr.Zero)
            {
                throw Fx.AssertAndThrow("OuterProxy cannot be null");
            }

            // No contract Fault on in
            if (contractType == null)
                TypeCacheManager.Provider.FindOrCreateType(riid, out contractType, true, false);

            if ((contractType.GUID != riid) && !(CheckDispatch(ref riid)))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidCastException(SR.GetString(SR.NoInterface, riid)));

            Type proxiedType = EmitterCache.TypeEmitter.FindOrCreateType(contractType);
            ComProxy comProxy = null;
            TearOffProxy tearoffProxy = null;
            try
            {
                tearoffProxy = new TearOffProxy(this, proxiedType);
                comProxy = ComProxy.Create(outer, tearoffProxy.GetTransparentProxy(), tearoffProxy);
                return comProxy;

            }
            finally
            {
                if ((comProxy == null) && (tearoffProxy != null))
                    ((IDisposable)tearoffProxy).Dispose();

            }
        }

        bool IProxyCreator.SupportsErrorInfo(ref Guid riid)
        {
            if (contractType == null)
                return false;
            else
            {
                if ((contractType.GUID != riid) && !(CheckDispatch(ref riid)))
                    return false;
                else
                    return true;
            }
        }

        bool IProxyCreator.SupportsDispatch()
        {
            return dispatchEnabled;
        }

        bool IProxyCreator.SupportsIntrinsics()
        {
            return true;
        }
    }
}
          
               
          
