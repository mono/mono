//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.ComponentModel;
    using System.Configuration;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    public abstract partial class MsmqBindingElementBase : StandardBindingElement
    {
        protected MsmqBindingElementBase(string name)
            : base(name)
        { }

        protected MsmqBindingElementBase()
            : this(null)
        { }

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

        [ConfigurationProperty(ConfigurationStrings.MaxReceivedMessageSize, DefaultValue = TransportDefaults.MaxReceivedMessageSize)]
        [LongValidator(MinValue = 0)]
        public long MaxReceivedMessageSize
        {
            get { return (long)base[ConfigurationStrings.MaxReceivedMessageSize]; }
            set { base[ConfigurationStrings.MaxReceivedMessageSize] = value; }
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

        protected internal override void InitializeFrom(Binding binding)
        {
            base.InitializeFrom(binding);
            MsmqBindingBase msmqBinding = (MsmqBindingBase)binding;


            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.DeadLetterQueue, msmqBinding.DeadLetterQueue);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.CustomDeadLetterQueue, msmqBinding.CustomDeadLetterQueue);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.Durable, msmqBinding.Durable);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.ExactlyOnce, msmqBinding.ExactlyOnce);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.MaxReceivedMessageSize, msmqBinding.MaxReceivedMessageSize);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.MaxRetryCycles, msmqBinding.MaxRetryCycles);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.ReceiveContextEnabled, msmqBinding.ReceiveContextEnabled);
            
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.ReceiveErrorHandling, msmqBinding.ReceiveErrorHandling);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.ReceiveRetryCount, msmqBinding.ReceiveRetryCount);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.RetryCycleDelay, msmqBinding.RetryCycleDelay);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.TimeToLive, msmqBinding.TimeToLive);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.UseSourceJournal, msmqBinding.UseSourceJournal);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.UseMsmqTracing, msmqBinding.UseMsmqTracing);

            // SetPropertyValueIfNotDefaultValue won't detect defaults correctly through type conversion, check explicitly
            if (msmqBinding.ValidityDuration != MsmqDefaults.ValidityDuration)
            {
                this.ValidityDuration = msmqBinding.ValidityDuration;                
            }            
        }

        protected override void OnApplyConfiguration(Binding binding)
        {
            MsmqBindingBase msmqBinding = (MsmqBindingBase)binding;

            if (this.CustomDeadLetterQueue != null)
                msmqBinding.CustomDeadLetterQueue = this.CustomDeadLetterQueue;
            msmqBinding.DeadLetterQueue = this.DeadLetterQueue;
            msmqBinding.Durable = this.Durable;
            msmqBinding.ExactlyOnce = this.ExactlyOnce;
            msmqBinding.MaxReceivedMessageSize = this.MaxReceivedMessageSize;
            msmqBinding.MaxRetryCycles = this.MaxRetryCycles;
            msmqBinding.ReceiveContextEnabled = this.ReceiveContextEnabled;
            msmqBinding.ReceiveErrorHandling = this.ReceiveErrorHandling;
            msmqBinding.ReceiveRetryCount = this.ReceiveRetryCount;
            msmqBinding.RetryCycleDelay = this.RetryCycleDelay;
            msmqBinding.TimeToLive = this.TimeToLive;
            msmqBinding.UseSourceJournal = this.UseSourceJournal;
            msmqBinding.UseMsmqTracing = this.UseMsmqTracing;
            msmqBinding.ValidityDuration = this.ValidityDuration;
        }
    }
}
