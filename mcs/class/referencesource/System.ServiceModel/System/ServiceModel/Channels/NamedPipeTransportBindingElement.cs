//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using Collections.ObjectModel;
    using System.Security.Principal;
    using System.ServiceModel.Activation;

    public class NamedPipeTransportBindingElement : ConnectionOrientedTransportBindingElement
    {
        List<SecurityIdentifier> allowedUsers = new List<SecurityIdentifier>();
        Collection<SecurityIdentifier> allowedUsersCollection;
        NamedPipeConnectionPoolSettings connectionPoolSettings = new NamedPipeConnectionPoolSettings();
        NamedPipeSettings settings = new NamedPipeSettings();

        public NamedPipeTransportBindingElement()
            : base()
        {
        }

        protected NamedPipeTransportBindingElement(NamedPipeTransportBindingElement elementToBeCloned)
            : base(elementToBeCloned)
        {
            if (elementToBeCloned.allowedUsers != null)
            {
                foreach (SecurityIdentifier id in elementToBeCloned.allowedUsers)
                {
                    this.allowedUsers.Add(id);
                }
            }

            this.connectionPoolSettings = elementToBeCloned.connectionPoolSettings.Clone();
            this.settings = elementToBeCloned.settings.Clone();
        }

        // Used by SMSvcHost (see Activation\SharingService.cs)
        internal List<SecurityIdentifier> AllowedUsers
        {
            get
            {
                return this.allowedUsers;
            }
            set
            {
                this.allowedUsers = value;
            }
        }

        public Collection<SecurityIdentifier> AllowedSecurityIdentifiers
        {
            get
            {
                if (this.allowedUsersCollection == null)
                {
                    this.allowedUsersCollection = new Collection<SecurityIdentifier>(this.allowedUsers);
                }

                return this.allowedUsersCollection;
            }
        }

        public NamedPipeConnectionPoolSettings ConnectionPoolSettings
        {
            get { return this.connectionPoolSettings; }
        }

        public NamedPipeSettings PipeSettings
        {
            get { return this.settings; }            
        }

        public override string Scheme
        {
            get { return "net.pipe"; }
        }

        internal override string WsdlTransportUri
        {
            get
            {
                return TransportPolicyConstants.NamedPipeTransportUri;
            }
        }

        public override BindingElement Clone()
        {
            return new NamedPipeTransportBindingElement(this);
        }

        public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

            if (!this.CanBuildChannelFactory<TChannel>(context))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("TChannel", SR.GetString(SR.ChannelTypeNotSupported, typeof(TChannel)));
            }

            return (IChannelFactory<TChannel>)(object)new NamedPipeChannelFactory<TChannel>(this, context);
        }

        public override IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

            if (!this.CanBuildChannelListener<TChannel>(context))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("TChannel", SR.GetString(SR.ChannelTypeNotSupported, typeof(TChannel)));
            }

            NamedPipeChannelListener listener;
            if (typeof(TChannel) == typeof(IReplyChannel))
            {
                listener = new NamedPipeReplyChannelListener(this, context);
            }
            else if (typeof(TChannel) == typeof(IDuplexSessionChannel))
            {
                listener = new NamedPipeDuplexChannelListener(this, context);
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("TChannel", SR.GetString(SR.ChannelTypeNotSupported, typeof(TChannel)));
            }

            AspNetEnvironment.Current.ApplyHostedContext(listener, context);
            return (IChannelListener<TChannel>)(object)listener;
        }

        public override T GetProperty<T>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            if (typeof(T) == typeof(IBindingDeliveryCapabilities))
            {
                return (T)(object)new BindingDeliveryCapabilitiesHelper();
            }
            if (typeof(T) == typeof(NamedPipeSettings))
            {
                return (T)(object)this.PipeSettings;
            }
            else
            {
                return base.GetProperty<T>(context);
            }
        }

        internal override bool IsMatch(BindingElement b)
        {
            if (!base.IsMatch(b))
            {
                return false;
            }

            NamedPipeTransportBindingElement namedPipe = b as NamedPipeTransportBindingElement;
            if (namedPipe == null)
            {
                return false;
            }

            if (!this.ConnectionPoolSettings.IsMatch(namedPipe.ConnectionPoolSettings))
            {
                return false;
            }

            if (!this.PipeSettings.IsMatch(namedPipe.PipeSettings))
            {
                return false;
            }

            return true;
        }

        class BindingDeliveryCapabilitiesHelper : IBindingDeliveryCapabilities
        {
            internal BindingDeliveryCapabilitiesHelper()
            {
            }
            bool IBindingDeliveryCapabilities.AssuresOrderedDelivery
            {
                get { return true; }
            }

            bool IBindingDeliveryCapabilities.QueuedDelivery
            {
                get { return false; }
            }
        }
    }
}
