//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Description
{
    using System;
    using System.Globalization;
    using System.ServiceModel.Administration;
    using System.ServiceModel.Persistence;

    [Obsolete("The WF3 types are deprecated.  Instead, please use the new WF4 types from System.Activities.*")]
    public class PersistenceProviderBehavior : IServiceBehavior, IWmiInstanceProvider
    {
        internal static readonly TimeSpan DefaultPersistenceOperationTimeout = TimeSpan.Parse(DefaultPersistenceOperationTimeoutString, CultureInfo.InvariantCulture);
        // 30 seconds was chosen because it is the default timeout for SqlCommand
        // (seemed like a reasonable reference point)
        internal const string DefaultPersistenceOperationTimeoutString = "00:00:30";
        TimeSpan persistenceOperationTimeout;

        PersistenceProviderFactory persistenceProviderFactory;

        public PersistenceProviderBehavior(PersistenceProviderFactory providerFactory)
            : this(providerFactory, DefaultPersistenceOperationTimeout)
        {
            // empty
        }

        public PersistenceProviderBehavior(PersistenceProviderFactory providerFactory, TimeSpan persistenceOperationTimeout)
        {
            this.PersistenceProviderFactory = providerFactory;
            this.PersistenceOperationTimeout = persistenceOperationTimeout;
        }

        public TimeSpan PersistenceOperationTimeout
        {
            get
            {
                return this.persistenceOperationTimeout;
            }
            set
            {
                if (value < TimeSpan.Zero)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new ArgumentOutOfRangeException(SR2.GetString(SR2.PersistenceOperationTimeoutOutOfRange)));
                }
                this.persistenceOperationTimeout = value;
            }
        }

        public PersistenceProviderFactory PersistenceProviderFactory
        {
            get
            {
                return this.persistenceProviderFactory;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                this.persistenceProviderFactory = value;
            }
        }


        public virtual void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, System.Collections.ObjectModel.Collection<ServiceEndpoint> endpoints, System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
        {
            // empty
        }

        public virtual void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            // empty
        }

        void IWmiInstanceProvider.FillInstance(IWmiInstance wmiInstance)
        {
            wmiInstance.SetProperty("PersistenceOperationTimeout", this.PersistenceOperationTimeout.ToString());
            wmiInstance.SetProperty("PersistenceProviderFactoryType", this.PersistenceProviderFactory.GetType().FullName);
        }

        string IWmiInstanceProvider.GetInstanceType()
        {
            return "PersistenceProviderBehavior";
        }

        public virtual void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            // empty
        }
    }
}
