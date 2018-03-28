//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Workflow.Runtime.Hosting
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IdentityModel.Claims;
    using System.Reflection;
    using System.Runtime;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Configuration;
    using System.ServiceModel.Description;

    static class ChannelManagerHelpers
    {
        static Hashtable contractInfoCollection = Hashtable.Synchronized(new Hashtable());
        static object thisLock = new object();

        public static EndpointAddress BuildCacheAddress(string endpointName, Type contractType)
        {
            Fx.Assert(!string.IsNullOrEmpty(endpointName), "endpointName should not be null");
            Fx.Assert(contractType != null, "contractType should not be null");

            return BuildCacheAddress(endpointName, contractType.AssemblyQualifiedName);
        }

        public static EndpointAddress BuildCacheAddress(string endpointName, string contractName)
        {
            Fx.Assert(!string.IsNullOrEmpty(endpointName), "endpointName should not be null");
            Fx.Assert(!string.IsNullOrEmpty(contractName), "contractName should not be null");

            return new EndpointAddress("channelCache://" + endpointName + "/" + contractName);
        }

        public static EndpointAddress BuildCacheAddressWithIdentity(EndpointAddress address)
        {
            Fx.Assert(address != null, "address should not be null");

            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            try
            {
                Claim claim = Claim.CreateWindowsSidClaim(identity.User);
                EndpointIdentity endpointIdentity = EndpointIdentity.CreateIdentity(claim);
                return new EndpointAddress(address.Uri, endpointIdentity);
            }
            finally
            {
                identity.Dispose();
            }
        }

        public static void CloseCommunicationObject(ICommunicationObject communicationObject)
        {
            CloseCommunicationObject(communicationObject, ServiceDefaults.CloseTimeout);
        }

        public static void CloseCommunicationObject(ICommunicationObject communicationObject, TimeSpan timeout)
        {
            if (communicationObject == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("communicationObject");
            }

            bool flag = true;
            try
            {
                if (communicationObject.State == CommunicationState.Opened)
                {
                    communicationObject.Close(timeout);
                    flag = false;
                }
            }
            catch (CommunicationException communicatioException)
            {
                DiagnosticUtility.TraceHandledException(communicatioException, TraceEventType.Information);
            }
            catch (TimeoutException timeoutException)
            {
                DiagnosticUtility.TraceHandledException(timeoutException, TraceEventType.Information);
            }
            finally
            {
                if (flag)
                {
                    communicationObject.Abort();
                }
            }
        }

        public static IChannel CreateChannel(Type contractType, ChannelFactory factory, string customAddress)
        {
            if (contractType == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contractType");
            }

            if (factory == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("factory");
            }

            IChannel channel = null;
            bool channelFailed = true;

            try
            {
                ContractInfo contractInfo = GetContractInfo(contractType);
                if (string.IsNullOrEmpty(customAddress))
                {
                    channel = contractInfo.CreateChannelMethodInfo.Invoke(factory, null) as IChannel;
                }
                else
                {
                    channel = contractInfo.CreateChannelWithCustomAddressMethodInfo.Invoke(factory,
                        new object[1] { new EndpointAddress(customAddress) }) as IChannel;
                }

                if (!contractInfo.IsSessionless)
                {
                    IContextManager contextManager = channel.GetProperty<IContextManager>();
                    if (contextManager != null)
                    {
                        contextManager.Enabled = false;
                    }
                }

                channel.Open();

                channelFailed = false;
            }
            catch (TargetInvocationException exception)
            {
                if (exception.InnerException != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception.InnerException);
                }

                throw;
            }
            finally
            {
                if (channel != null && channelFailed)
                {
                    channel.Abort();
                    channel = null;
                }
            }

            return channel;
        }

        public static ChannelFactory CreateChannelFactory(string endpointName, Type contractType)
        {
            return CreateChannelFactory(endpointName, contractType, null);
        }

        public static ChannelFactory CreateChannelFactory(string endpointName, Type contractType, IDictionary<EndpointAddress, ServiceEndpoint> codeEndpoints)
        {
            if (string.IsNullOrEmpty(endpointName))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpointName");
            }

            if (contractType == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contractType");
            }

            ChannelFactory factory = null;
            bool factoryFailed = true;

            try
            {
                ContractInfo contractInfo = GetContractInfo(contractType);
                Type channelFactoryType = contractInfo.ChannelFactoryType;
                object[] args = null;

                EndpointAddress key = BuildCacheAddress(endpointName, contractType);
                ServiceEndpoint endpoint = null;

                if (codeEndpoints != null &&
                    codeEndpoints.TryGetValue(key, out endpoint) &&
                    !IsEndpointDefinedInConfiguration(endpointName, contractType))
                {
                    args = new object[] { endpoint };
                }
                else
                {
                    args = new object[] { endpointName ?? string.Empty };
                }

                factory = Activator.CreateInstance(channelFactoryType, args) as ChannelFactory;
                factory.Open();

                factoryFailed = false;
            }
            catch (TargetInvocationException exception)
            {
                if (exception.InnerException != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception.InnerException);
                }

                throw;
            }
            finally
            {
                if (factory != null && factoryFailed)
                {
                    factory.Abort();
                }
            }

            return factory;
        }

        public static bool IsEndpointDefinedInConfiguration(string endpointName, Type contractType)
        {
            if (string.IsNullOrEmpty(endpointName))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpointName");
            }

            if (contractType == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contractType");
            }

            bool wildcard = string.Equals(endpointName, "*", StringComparison.Ordinal);

            ClientSection section = ClientSection.GetSection();
            foreach (ChannelEndpointElement element in section.Endpoints)
            {
                if ((!wildcard && (element.Name != endpointName)) || (element.Contract != contractType.FullName))
                {
                    continue;
                }
                return true;
            }

            return false;
        }

        public static bool IsSessionlessContract(Type contractType)
        {
            return GetContractInfo(contractType).IsSessionless;
        }

        static ContractInfo GetContractInfo(Type contractType)
        {
            ContractInfo contractInfo = contractInfoCollection[contractType] as ContractInfo;
            if (contractInfo == null)
            {
                lock (thisLock)
                {
                    contractInfo = contractInfoCollection[contractType] as ContractInfo;
                    if (contractInfo == null)
                    {
                        contractInfo = new ContractInfo(contractType);
                        contractInfoCollection.Add(contractType, contractInfo);
                    }
                }
            }

            return contractInfo;
        }

        class ContractInfo
        {
            Type channelFactoryType;
            Type contractType;
            MethodInfo createChannelMethodInfo;
            MethodInfo createChannelWithCustomAddressMethodInfo;
            bool isSessionless;

            public ContractInfo(Type contractType)
            {
                this.contractType = contractType;

                Type[] typeArguments = new Type[] { contractType };
                this.channelFactoryType = typeof(ChannelFactory<>).MakeGenericType(typeArguments);

                this.createChannelMethodInfo = this.channelFactoryType.GetMethod("CreateChannel", new Type[0] { });
                this.createChannelWithCustomAddressMethodInfo = this.channelFactoryType.GetMethod("CreateChannel", new Type[1] { typeof(EndpointAddress) });

                this.isSessionless = (ContractDescription.GetContract(contractType).SessionMode == SessionMode.NotAllowed);
            }

            public Type ChannelFactoryType
            {
                get
                {
                    return this.channelFactoryType;
                }
            }

            public MethodInfo CreateChannelMethodInfo
            {
                get
                {
                    return this.createChannelMethodInfo;
                }
            }

            public MethodInfo CreateChannelWithCustomAddressMethodInfo
            {
                get
                {
                    return this.createChannelWithCustomAddressMethodInfo;
                }
            }

            public bool IsSessionless
            {
                get
                {
                    return this.isSessionless;
                }
            }
        }
    }
}
