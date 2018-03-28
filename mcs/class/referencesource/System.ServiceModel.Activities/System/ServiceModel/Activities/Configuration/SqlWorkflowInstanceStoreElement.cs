//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Activities.Configuration
{
    using System.Activities.DurableInstancing;
    using System.ComponentModel;
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using System.ServiceModel.Configuration;
    using System.ServiceModel.Activities.Description;
    using System.Runtime;
    using System.Runtime.DurableInstancing;    

    [Fx.Tag.XamlVisible(false)]
    public class SqlWorkflowInstanceStoreElement : BehaviorExtensionElement
    {
        const string connectionString = "connectionString";
        const string connectionStringName = "connectionStringName";
        const string defaultConnectionStringName = "DefaultSqlWorkflowInstanceStoreConnectionString";
        const string hostLockRenewalPeriodParameter = "hostLockRenewalPeriod";
        const string runnableInstancesDetectionPeriodParameter = "runnableInstancesDetectionPeriod";
        const string instanceEncodingOption = "instanceEncodingOption";
        const string instanceCompletionAction = "instanceCompletionAction";
        const string instanceLockedExceptionAction = "instanceLockedExceptionAction";
        const string maxConnectionRetries = "maxConnectionRetries";

        public SqlWorkflowInstanceStoreElement()
        {
        }

        [SuppressMessage(
            FxCop.Category.Configuration,
            FxCop.Rule.ConfigurationPropertyAttributeRule,
            Justification = "This property only overrides the base property.")]
        public override Type BehaviorType
        {
            get { return typeof(SqlWorkflowInstanceStoreBehavior); }
        }

        protected internal override object CreateBehavior()
        {
            bool useDefaultConnectionStringName = false;

            if (string.IsNullOrEmpty(this.ConnectionString) &&
                string.IsNullOrEmpty(this.ConnectionStringName))
            {
                useDefaultConnectionStringName = true;
            }

            if (!string.IsNullOrEmpty(this.ConnectionString) &&
                !string.IsNullOrEmpty(this.ConnectionStringName))
            {
                throw FxTrace.Exception.AsError(new InstancePersistenceException(SR.CannotSpecifyBothConnectionStringAndName));
            }

            string connectionStringToUse;
            if (!string.IsNullOrEmpty(this.ConnectionStringName) || useDefaultConnectionStringName)
            {
                string connectionStringNameToUse = useDefaultConnectionStringName ? defaultConnectionStringName : this.ConnectionStringName;
                ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings[connectionStringNameToUse];

                if (settings == null)
                {
                    if (useDefaultConnectionStringName)
                    {
                        throw FxTrace.Exception.AsError(new InstancePersistenceException(SR.MustSpecifyConnectionStringOrName));
                    }

                    throw FxTrace.Exception.Argument(connectionStringName, SR.ConnectionStringNameWrong(this.ConnectionStringName));
                }

                connectionStringToUse = settings.ConnectionString;
            }
            else
            {
                connectionStringToUse = this.ConnectionString;
            }

            SqlWorkflowInstanceStoreBehavior sqlWorkflowInstanceStoreBehavior = new SqlWorkflowInstanceStoreBehavior
            {
                ConnectionString = connectionStringToUse,
                HostLockRenewalPeriod = this.HostLockRenewalPeriod,
                InstanceEncodingOption = this.InstanceEncodingOption,
                InstanceCompletionAction = this.InstanceCompletionAction,
                InstanceLockedExceptionAction = this.InstanceLockedExceptionAction,
                RunnableInstancesDetectionPeriod = this.RunnableInstancesDetectionPeriod,
                MaxConnectionRetries = this.MaxConnectionRetries
            };

            return sqlWorkflowInstanceStoreBehavior;
        }

        [ConfigurationProperty(connectionString, IsRequired = false)]
        [SuppressMessage("Configuration", "Configuration104:ConfigurationValidatorAttributeRule",
            Justification = "validated on CreateBehavior() when we try to retrive connection string")]
        [StringValidator(MinLength = 0)]
        public string ConnectionString
        {
            get { return (string)base[connectionString]; }
            set { base[connectionString] = value; }
        }

        [ConfigurationProperty(connectionStringName, IsRequired = false)]
        [SuppressMessage("Configuration", "Configuration104:ConfigurationValidatorAttributeRule",
            Justification = "validated on CreateBehavior() when we try to retrive connection string")]
        [StringValidator(MinLength = 0)]
        public string ConnectionStringName
        {
            get { return (string)base[connectionStringName]; }
            set { base[connectionStringName] = value; }
        }

        [ConfigurationProperty(
            hostLockRenewalPeriodParameter,
            IsRequired = false,
            DefaultValue = SqlWorkflowInstanceStoreBehavior.defaultHostRenewalString)]
        [TypeConverter(typeof(TimeSpanOrInfiniteConverter))]
        [PositiveTimeSpanValidator]
        public TimeSpan HostLockRenewalPeriod
        {
            get { return (TimeSpan)base[hostLockRenewalPeriodParameter]; }
            set { base[hostLockRenewalPeriodParameter] = value; }
        }

        [ConfigurationProperty(
            runnableInstancesDetectionPeriodParameter,
            IsRequired = false,
            DefaultValue = SqlWorkflowInstanceStoreBehavior.defaultRunnableInstancesDetectionPeriodString)]
        [TypeConverter(typeof(TimeSpanOrInfiniteConverter))]
        [PositiveTimeSpanValidator]
        public TimeSpan RunnableInstancesDetectionPeriod
        {
            get { return (TimeSpan)base[runnableInstancesDetectionPeriodParameter]; }
            set { base[runnableInstancesDetectionPeriodParameter] = value; }
        }

        [ConfigurationProperty(
            instanceEncodingOption,
            IsRequired = false,
            DefaultValue = SqlWorkflowInstanceStoreBehavior.defaultEncodingOption)]
        [SuppressMessage("Configuration", "Configuration104:ConfigurationValidatorAttributeRule")]
        public InstanceEncodingOption InstanceEncodingOption
        {
            get { return (InstanceEncodingOption)base[instanceEncodingOption]; }
            set { base[instanceEncodingOption] = value; }
        }

        [ConfigurationProperty(
            instanceCompletionAction,
            IsRequired = false,
            DefaultValue = SqlWorkflowInstanceStoreBehavior.defaultInstanceCompletionAction)]
        [SuppressMessage("Configuration", "Configuration104:ConfigurationValidatorAttributeRule")]
        public InstanceCompletionAction InstanceCompletionAction
        {
            get { return (InstanceCompletionAction)base[instanceCompletionAction]; }
            set { base[instanceCompletionAction] = value; }
        }

        [ConfigurationProperty(
            instanceLockedExceptionAction,
            IsRequired = false,
            DefaultValue = SqlWorkflowInstanceStoreBehavior.defaultInstanceLockedExceptionAction)]
        [SuppressMessage("Configuration", "Configuration104:ConfigurationValidatorAttributeRule")]
        public InstanceLockedExceptionAction InstanceLockedExceptionAction
        {
            get { return (InstanceLockedExceptionAction)base[instanceLockedExceptionAction]; }
            set { base[instanceLockedExceptionAction] = value; }
        }

        [ConfigurationProperty(
            maxConnectionRetries,
            IsRequired = false,
            DefaultValue = SqlWorkflowInstanceStoreBehavior.defaultMaximumRetries)]
        [IntegerValidator(MinValue = 0)]
        [SuppressMessage("Configuration", "Configuration104:ConfigurationValidatorAttributeRule")]
        public int MaxConnectionRetries
        {
            get { return (int)base[maxConnectionRetries]; }
            set { base[maxConnectionRetries] = value; }
        }
    }

}
