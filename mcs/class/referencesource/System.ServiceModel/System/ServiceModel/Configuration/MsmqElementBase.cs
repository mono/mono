//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.ComponentModel;
    using System.Configuration;
    using System.Runtime;
    using System.ServiceModel.Channels;

    public abstract partial class MsmqElementBase : TransportElement
    {

        [ConfigurationProperty(ConfigurationStrings.CustomDeadLetterQueue, DefaultValue = MsmqDefaults.CustomDeadLetterQueue)]
        public Uri CustomDeadLetterQueue
        {
            get { return (Uri)base[ConfigurationStrings.CustomDeadLetterQueue]; }
            set { base[ConfigurationStrings.CustomDeadLetterQueue] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.DeadLetterQueue, DefaultValue = MsmqDefaults.DeadLetterQueue)]
        [ServiceModelEnumValidator(typeof(DeadLetterQueueHelper))]
        public DeadLetterQueue DeadLetterQueue
        {
            get { return (DeadLetterQueue)base[ConfigurationStrings.DeadLetterQueue]; }
            set { base[ConfigurationStrings.DeadLetterQueue] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.Durable, DefaultValue = MsmqDefaults.Durable)]
        public bool Durable
        {
            get { return (bool)base[ConfigurationStrings.Durable]; }
            set { base[ConfigurationStrings.Durable] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.ExactlyOnce, DefaultValue = MsmqDefaults.ExactlyOnce)]
        public bool ExactlyOnce
        {
            get { return (bool)base[ConfigurationStrings.ExactlyOnce]; }
            set { base[ConfigurationStrings.ExactlyOnce] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.MaxRetryCycles, DefaultValue = MsmqDefaults.MaxRetryCycles)]
        [IntegerValidator(MinValue = 0)]
        public int MaxRetryCycles
        {
            get { return (int)base[ConfigurationStrings.MaxRetryCycles]; }
            set { base[ConfigurationStrings.MaxRetryCycles] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.ReceiveContextEnabled, DefaultValue = MsmqDefaults.ReceiveContextEnabled)]
        public bool ReceiveContextEnabled
        {
            get { return (bool)base[ConfigurationStrings.ReceiveContextEnabled]; }
            set { base[ConfigurationStrings.ReceiveContextEnabled] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.ReceiveErrorHandling, DefaultValue = MsmqDefaults.ReceiveErrorHandling)]
        [ServiceModelEnumValidator(typeof(ReceiveErrorHandlingHelper))]
        public ReceiveErrorHandling ReceiveErrorHandling
        {
            get { return (ReceiveErrorHandling)base[ConfigurationStrings.ReceiveErrorHandling]; }
            set { base[ConfigurationStrings.ReceiveErrorHandling] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.ReceiveRetryCount, DefaultValue = MsmqDefaults.ReceiveRetryCount)]
        [IntegerValidator(MinValue = 0)]
        public int ReceiveRetryCount
        {
            get { return (int)base[ConfigurationStrings.ReceiveRetryCount]; }
            set { base[ConfigurationStrings.ReceiveRetryCount] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.RetryCycleDelay, DefaultValue = MsmqDefaults.RetryCycleDelayString)]
        [TypeConverter(typeof(TimeSpanOrInfiniteConverter))]
        [ServiceModelTimeSpanValidator(MinValueString = ConfigurationStrings.TimeSpanZero)]
        public TimeSpan RetryCycleDelay
        {
            get { return (TimeSpan)base[ConfigurationStrings.RetryCycleDelay]; }
            set { base[ConfigurationStrings.RetryCycleDelay] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.MsmqTransportSecurity)]
        public MsmqTransportSecurityElement MsmqTransportSecurity
        {
            get { return (MsmqTransportSecurityElement)base[ConfigurationStrings.MsmqTransportSecurity]; }
        }

        [ConfigurationProperty(ConfigurationStrings.TimeToLive, DefaultValue = MsmqDefaults.TimeToLiveString)]
        [TypeConverter(typeof(TimeSpanOrInfiniteConverter))]
        [ServiceModelTimeSpanValidator(MinValueString = ConfigurationStrings.TimeSpanZero)]
        public TimeSpan TimeToLive
        {
            get { return (TimeSpan)base[ConfigurationStrings.TimeToLive]; }
            set { base[ConfigurationStrings.TimeToLive] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.UseSourceJournal, DefaultValue = MsmqDefaults.UseSourceJournal)]
        public bool UseSourceJournal
        {
            get { return (bool)base[ConfigurationStrings.UseSourceJournal]; }
            set { base[ConfigurationStrings.UseSourceJournal] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.UseMsmqTracing, DefaultValue = MsmqDefaults.UseMsmqTracing)]
        public bool UseMsmqTracing
        {
            get { return (bool)base[ConfigurationStrings.UseMsmqTracing]; }
            set { base[ConfigurationStrings.UseMsmqTracing] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.ValidityDuration, DefaultValue = MsmqDefaults.ValidityDurationString)]
        [TypeConverter(typeof(TimeSpanOrInfiniteConverter))]
        [ServiceModelTimeSpanValidator(MinValueString = ConfigurationStrings.TimeSpanZero)]
        public TimeSpan ValidityDuration
        {
            get { return (TimeSpan)base[ConfigurationStrings.ValidityDuration]; }
            set { base[ConfigurationStrings.ValidityDuration] = value; }
        }

        public override void ApplyConfiguration(BindingElement bindingElement)
        {
            base.ApplyConfiguration(bindingElement);

            System.ServiceModel.Channels.MsmqBindingElementBase binding = bindingElement as System.ServiceModel.Channels.MsmqBindingElementBase;
            if (null != binding)
            {
                if (null != this.CustomDeadLetterQueue)
                    binding.CustomDeadLetterQueue = this.CustomDeadLetterQueue;
                binding.DeadLetterQueue = this.DeadLetterQueue;
                binding.Durable = this.Durable;
                binding.ExactlyOnce = this.ExactlyOnce;
                binding.MaxRetryCycles = this.MaxRetryCycles;
                binding.ReceiveContextEnabled = this.ReceiveContextEnabled;
                binding.ReceiveErrorHandling = this.ReceiveErrorHandling;
                binding.ReceiveRetryCount = this.ReceiveRetryCount;
                binding.RetryCycleDelay = this.RetryCycleDelay;
                binding.TimeToLive = this.TimeToLive;
                binding.UseSourceJournal = this.UseSourceJournal;
                binding.UseMsmqTracing = this.UseMsmqTracing;
                binding.ValidityDuration = this.ValidityDuration;
                this.MsmqTransportSecurity.ApplyConfiguration(binding.MsmqTransportSecurity);
            }
        }

        public override void CopyFrom(ServiceModelExtensionElement from)
        {
            base.CopyFrom(from);

            MsmqElementBase source = from as MsmqElementBase;

            if (null != source)
            {
                this.CustomDeadLetterQueue = source.CustomDeadLetterQueue;
                this.DeadLetterQueue = source.DeadLetterQueue;
                this.Durable = source.Durable;
                this.ExactlyOnce = source.ExactlyOnce;
                this.MaxRetryCycles = source.MaxRetryCycles;
                this.ReceiveContextEnabled = source.ReceiveContextEnabled;
                this.ReceiveErrorHandling = source.ReceiveErrorHandling;
                this.ReceiveRetryCount = source.ReceiveRetryCount;
                this.RetryCycleDelay = source.RetryCycleDelay;
                this.TimeToLive = source.TimeToLive;
                this.UseSourceJournal = source.UseSourceJournal;
                this.UseMsmqTracing = source.UseMsmqTracing;
                this.ValidityDuration = source.ValidityDuration;
                this.MsmqTransportSecurity.MsmqAuthenticationMode = source.MsmqTransportSecurity.MsmqAuthenticationMode;
                this.MsmqTransportSecurity.MsmqProtectionLevel = source.MsmqTransportSecurity.MsmqProtectionLevel;
                this.MsmqTransportSecurity.MsmqEncryptionAlgorithm = source.MsmqTransportSecurity.MsmqEncryptionAlgorithm;
                this.MsmqTransportSecurity.MsmqSecureHashAlgorithm = source.MsmqTransportSecurity.MsmqSecureHashAlgorithm;
            }
        }

        protected internal override void InitializeFrom(BindingElement bindingElement)
        {
            base.InitializeFrom(bindingElement);

            System.ServiceModel.Channels.MsmqBindingElementBase binding = bindingElement as System.ServiceModel.Channels.MsmqBindingElementBase;

            if (null != binding)
            {
                SetPropertyValueIfNotDefaultValue(ConfigurationStrings.CustomDeadLetterQueue, binding.CustomDeadLetterQueue);
                SetPropertyValueIfNotDefaultValue(ConfigurationStrings.DeadLetterQueue, binding.DeadLetterQueue);
                SetPropertyValueIfNotDefaultValue(ConfigurationStrings.Durable, binding.Durable);
                SetPropertyValueIfNotDefaultValue(ConfigurationStrings.ExactlyOnce, binding.ExactlyOnce);
                SetPropertyValueIfNotDefaultValue(ConfigurationStrings.MaxRetryCycles, binding.MaxRetryCycles);
                SetPropertyValueIfNotDefaultValue(ConfigurationStrings.ReceiveErrorHandling, binding.ReceiveErrorHandling);
                SetPropertyValueIfNotDefaultValue(ConfigurationStrings.ReceiveRetryCount, binding.ReceiveRetryCount);
                SetPropertyValueIfNotDefaultValue(ConfigurationStrings.RetryCycleDelay, binding.RetryCycleDelay);
                SetPropertyValueIfNotDefaultValue(ConfigurationStrings.TimeToLive, binding.TimeToLive);
                SetPropertyValueIfNotDefaultValue(ConfigurationStrings.UseSourceJournal, binding.UseSourceJournal);
                SetPropertyValueIfNotDefaultValue(ConfigurationStrings.ReceiveContextEnabled, binding.ReceiveContextEnabled);
                SetPropertyValueIfNotDefaultValue(ConfigurationStrings.UseMsmqTracing, binding.UseMsmqTracing);

                // SetPropertyValueIfNotDefaultValue won't detect defaults correctly through type conversion, check explicitly            
                if (binding.ValidityDuration != MsmqDefaults.ValidityDuration)
                {
                    this.ValidityDuration = binding.ValidityDuration;
                }

                this.MsmqTransportSecurity.InitializeFrom(binding.MsmqTransportSecurity);
            }
        }
    }
}



