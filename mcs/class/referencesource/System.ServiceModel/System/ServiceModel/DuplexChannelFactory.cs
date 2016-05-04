//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel
{
    using System.Collections.Generic;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Channels;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Threading;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Configuration;

    public class DuplexChannelFactory<TChannel> : ChannelFactory<TChannel>
    {
        //Type overloads
        public DuplexChannelFactory(Type callbackInstanceType)
            : this((object)callbackInstanceType)
        { }
        public DuplexChannelFactory(Type callbackInstanceType, Binding binding, String remoteAddress)
            : this((object)callbackInstanceType, binding, new EndpointAddress(remoteAddress))
        { }
        public DuplexChannelFactory(Type callbackInstanceType, Binding binding, EndpointAddress remoteAddress)
            : this((object)callbackInstanceType, binding, remoteAddress)
        { }
        public DuplexChannelFactory(Type callbackInstanceType, Binding binding)
            : this((object)callbackInstanceType, binding)
        { }
        public DuplexChannelFactory(Type callbackInstanceType, string endpointConfigurationName, EndpointAddress remoteAddress)
            : this((object)callbackInstanceType, endpointConfigurationName, remoteAddress)
        { }
        public DuplexChannelFactory(Type callbackInstanceType, string endpointConfigurationName)
            : this((object)callbackInstanceType, endpointConfigurationName)
        { }
        public DuplexChannelFactory(Type callbackInstanceType, ServiceEndpoint endpoint)
            : this((object)callbackInstanceType, endpoint)
        { }

        //InstanceContext overloads
        public DuplexChannelFactory(InstanceContext callbackInstance)
            : this((object)callbackInstance)
        { }
        public DuplexChannelFactory(InstanceContext callbackInstance, Binding binding, String remoteAddress)
            : this((object)callbackInstance, binding, new EndpointAddress(remoteAddress))
        { }
        public DuplexChannelFactory(InstanceContext callbackInstance, Binding binding, EndpointAddress remoteAddress)
            : this((object)callbackInstance, binding, remoteAddress)
        { }
        public DuplexChannelFactory(InstanceContext callbackInstance, Binding binding)
            : this((object)callbackInstance, binding)
        { }
        public DuplexChannelFactory(InstanceContext callbackInstance, string endpointConfigurationName, EndpointAddress remoteAddress)
            : this((object)callbackInstance, endpointConfigurationName, remoteAddress)
        { }
        public DuplexChannelFactory(InstanceContext callbackInstance, string endpointConfigurationName)
            : this((object)callbackInstance, endpointConfigurationName)
        { }
        public DuplexChannelFactory(InstanceContext callbackInstance, ServiceEndpoint endpoint)
            : this((object)callbackInstance, endpoint)
        { }

        // TChannel provides ContractDescription
        public DuplexChannelFactory(object callbackObject)
            : base(typeof(TChannel))
        {
            using (ServiceModelActivity activity = DiagnosticUtility.ShouldUseActivity ? ServiceModelActivity.CreateBoundedActivity() : null)
            {
                if (DiagnosticUtility.ShouldUseActivity)
                {
                    ServiceModelActivity.Start(activity, SR.GetString(SR.ActivityConstructChannelFactory, TraceUtility.CreateSourceString(this)), ActivityType.Construct);
                }
                if (callbackObject == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("callbackObject");
                }

                this.CheckAndAssignCallbackInstance(callbackObject);
                this.InitializeEndpoint((string)null, (EndpointAddress)null);
            }
        }

        // TChannel provides ContractDescription, attr/config [TChannel,name] provides Address,Binding
        public DuplexChannelFactory(object callbackObject, string endpointConfigurationName)
            : this(callbackObject, endpointConfigurationName, null)
        {
        }

        // TChannel provides ContractDescription, attr/config [TChannel,name] provides Binding, provide Address explicitly
        public DuplexChannelFactory(object callbackObject, string endpointConfigurationName, EndpointAddress remoteAddress)
            : base(typeof(TChannel))
        {
            using (ServiceModelActivity activity = DiagnosticUtility.ShouldUseActivity ? ServiceModelActivity.CreateBoundedActivity() : null)
            {
                if (DiagnosticUtility.ShouldUseActivity)
                {
                    ServiceModelActivity.Start(activity, SR.GetString(SR.ActivityConstructChannelFactory, TraceUtility.CreateSourceString(this)), ActivityType.Construct);
                }
                if (callbackObject == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("callbackObject");
                }

                if (endpointConfigurationName == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpointConfigurationName");
                }

                this.CheckAndAssignCallbackInstance(callbackObject);
                this.InitializeEndpoint(endpointConfigurationName, remoteAddress);
            }
        }

        // TChannel provides ContractDescription, attr/config [TChannel,name] provides Address,Binding
        public DuplexChannelFactory(object callbackObject, Binding binding)
            : this(callbackObject, binding, (EndpointAddress)null)
        {
        }

        // TChannel provides ContractDescription, provide Address,Binding explicitly
        public DuplexChannelFactory(object callbackObject, Binding binding, String remoteAddress)
            : this(callbackObject, binding, new EndpointAddress(remoteAddress))
        {
        }
        // TChannel provides ContractDescription, provide Address,Binding explicitly
        public DuplexChannelFactory(object callbackObject, Binding binding, EndpointAddress remoteAddress)
            : base(typeof(TChannel))
        {
            using (ServiceModelActivity activity = DiagnosticUtility.ShouldUseActivity ? ServiceModelActivity.CreateBoundedActivity() : null)
            {
                if (DiagnosticUtility.ShouldUseActivity)
                {
                    ServiceModelActivity.Start(activity, SR.GetString(SR.ActivityConstructChannelFactory, TraceUtility.CreateSourceString(this)), ActivityType.Construct);
                }
                if (callbackObject == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("callbackObject");
                }

                if (binding == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("binding");
                }

                this.CheckAndAssignCallbackInstance(callbackObject);
                this.InitializeEndpoint(binding, remoteAddress);
            }
        }

        // provide ContractDescription,Address,Binding explicitly
        public DuplexChannelFactory(object callbackObject, ServiceEndpoint endpoint)
            : base(typeof(TChannel))
        {
            using (ServiceModelActivity activity = DiagnosticUtility.ShouldUseActivity ? ServiceModelActivity.CreateBoundedActivity() : null)
            {
                if (DiagnosticUtility.ShouldUseActivity)
                {
                    ServiceModelActivity.Start(activity, SR.GetString(SR.ActivityConstructChannelFactory, TraceUtility.CreateSourceString(this)), ActivityType.Construct);
                }
                if (callbackObject == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("callbackObject");
                }

                if (endpoint == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpoint");
                }

                this.CheckAndAssignCallbackInstance(callbackObject);
                this.InitializeEndpoint(endpoint);
            }
        }

        internal void CheckAndAssignCallbackInstance(object callbackInstance)
        {
            if (callbackInstance is Type)
            {
                this.CallbackType = (Type)callbackInstance;
            }
            else if (callbackInstance is InstanceContext)
            {
                this.CallbackInstance = (InstanceContext)callbackInstance;
            }
            else
            {
                this.CallbackInstance = new InstanceContext(callbackInstance);
            }
        }

        public TChannel CreateChannel(InstanceContext callbackInstance)
        {
            return CreateChannel(callbackInstance, CreateEndpointAddress(this.Endpoint), null);
        }

        public TChannel CreateChannel(InstanceContext callbackInstance, EndpointAddress address)
        {
            if (address == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("address");
            }

            return CreateChannel(callbackInstance, address, address.Uri);
        }

        public override TChannel CreateChannel(EndpointAddress address, Uri via)
        {
            return CreateChannel(this.CallbackInstance, address, via);
        }

        public virtual TChannel CreateChannel(InstanceContext callbackInstance, EndpointAddress address, Uri via)
        {
            if (address == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("address");
            }

            if (this.CallbackType != null && callbackInstance == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxCreateDuplexChannelNoCallback1)));
            }
            if (callbackInstance == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxCreateDuplexChannelNoCallback)));
            }

            if (callbackInstance.UserObject == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxCreateDuplexChannelNoCallbackUserObject)));
            }

            if (!this.HasDuplexOperations())
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxCreateDuplexChannel1, this.Endpoint.Contract.Name)));
            }

            Type userObjectType = callbackInstance.UserObject.GetType();
            Type callbackType = this.Endpoint.Contract.CallbackContractType;
            if (callbackType != null && !callbackType.IsAssignableFrom(userObjectType))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(
                    SR.SFxCreateDuplexChannelBadCallbackUserObject, callbackType)));
            }

            EnsureOpened();
            TChannel result = (TChannel)this.ServiceChannelFactory.CreateChannel(typeof(TChannel), address, via);

            IDuplexContextChannel duplexChannel = result as IDuplexContextChannel;
            if (duplexChannel != null)
            {
                duplexChannel.CallbackInstance = callbackInstance;
            }
            return result;
        }

        //Static funtions to create channels
        static InstanceContext GetInstanceContextForObject(object callbackObject)
        {
            if (callbackObject is InstanceContext)
            {
                return (InstanceContext)callbackObject;
            }

            return new InstanceContext(callbackObject);
        }

        public static TChannel CreateChannel(object callbackObject, String endpointConfigurationName)
        {
            return CreateChannel(GetInstanceContextForObject(callbackObject), endpointConfigurationName);
        }

        public static TChannel CreateChannel(object callbackObject, Binding binding, EndpointAddress endpointAddress)
        {
            return CreateChannel(GetInstanceContextForObject(callbackObject), binding, endpointAddress);
        }

        public static TChannel CreateChannel(object callbackObject, Binding binding, EndpointAddress endpointAddress, Uri via)
        {
            return CreateChannel(GetInstanceContextForObject(callbackObject), binding, endpointAddress, via);
        }

        public static TChannel CreateChannel(InstanceContext callbackInstance, String endpointConfigurationName)
        {
            DuplexChannelFactory<TChannel> channelFactory = new DuplexChannelFactory<TChannel>(callbackInstance, endpointConfigurationName);
            TChannel channel = channelFactory.CreateChannel();
            SetFactoryToAutoClose(channel);
            return channel;
        }

        public static TChannel CreateChannel(InstanceContext callbackInstance, Binding binding, EndpointAddress endpointAddress)
        {
            DuplexChannelFactory<TChannel> channelFactory = new DuplexChannelFactory<TChannel>(callbackInstance, binding, endpointAddress);
            TChannel channel = channelFactory.CreateChannel();
            SetFactoryToAutoClose(channel);
            return channel;
        }

        public static TChannel CreateChannel(InstanceContext callbackInstance, Binding binding, EndpointAddress endpointAddress, Uri via)
        {
            DuplexChannelFactory<TChannel> channelFactory = new DuplexChannelFactory<TChannel>(callbackInstance, binding);
            TChannel channel = channelFactory.CreateChannel(endpointAddress, via);
            SetFactoryToAutoClose(channel);
            return channel;
        }

    }
}
