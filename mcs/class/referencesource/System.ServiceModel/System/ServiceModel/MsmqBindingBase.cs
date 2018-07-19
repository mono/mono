//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel
{
    using System;
    using System.ComponentModel;
    using System.ServiceModel.Channels;
    using System.Net.Security;

    public abstract class MsmqBindingBase : Binding, IBindingRuntimePreferences
    {
        internal MsmqBindingElementBase transport;

        protected MsmqBindingBase()
        { }

        [DefaultValue(typeof(System.TimeSpan), MsmqDefaults.ValidityDurationString)]
        public TimeSpan ValidityDuration
        {
            get { return transport.ValidityDuration; }
            set { transport.ValidityDuration = value; }
        }

        [DefaultValue(MsmqDefaults.CustomDeadLetterQueue)]
        public Uri CustomDeadLetterQueue
        {
            get { return transport.CustomDeadLetterQueue; }
            set { transport.CustomDeadLetterQueue = value; }
        }

        [DefaultValue(MsmqDefaults.DeadLetterQueue)]
        public DeadLetterQueue DeadLetterQueue
        {
            get { return transport.DeadLetterQueue; }
            set { transport.DeadLetterQueue = value; }
        }

        [DefaultValue(MsmqDefaults.Durable)]
        public bool Durable
        {
            get { return transport.Durable; }
            set { transport.Durable = value; }
        }

        [DefaultValue(MsmqDefaults.ExactlyOnce)]
        public bool ExactlyOnce
        {
            get { return transport.ExactlyOnce; }
            set { transport.ExactlyOnce = value; }
        }

        [DefaultValue(TransportDefaults.MaxReceivedMessageSize)]
        public long MaxReceivedMessageSize
        {
            get { return transport.MaxReceivedMessageSize; }
            set { transport.MaxReceivedMessageSize = value; }
        }

        [DefaultValue(MsmqDefaults.ReceiveRetryCount)]
        public int ReceiveRetryCount
        {
            get { return transport.ReceiveRetryCount; }
            set { transport.ReceiveRetryCount = value; }
        }

        [DefaultValue(MsmqDefaults.MaxRetryCycles)]
        public int MaxRetryCycles
        {
            get { return transport.MaxRetryCycles; }
            set { transport.MaxRetryCycles = value; }
        }

        [DefaultValue(MsmqDefaults.ReceiveContextEnabled)]
        public bool ReceiveContextEnabled
        {
            get { return transport.ReceiveContextEnabled; }
            set { transport.ReceiveContextEnabled = value; }
        }

        [DefaultValue(MsmqDefaults.ReceiveErrorHandling)]
        public ReceiveErrorHandling ReceiveErrorHandling
        {
            get { return transport.ReceiveErrorHandling; }
            set { transport.ReceiveErrorHandling = value; }
        }

        [DefaultValue(typeof(System.TimeSpan), MsmqDefaults.RetryCycleDelayString)]
        public TimeSpan RetryCycleDelay
        {
            get { return transport.RetryCycleDelay; }
            set { transport.RetryCycleDelay = value; }
        }

        public override string Scheme { get { return transport.Scheme; } }

        [DefaultValue(typeof(System.TimeSpan), MsmqDefaults.TimeToLiveString)]
        public TimeSpan TimeToLive
        {
            get { return transport.TimeToLive; }
            set { transport.TimeToLive = value; }
        }

        [DefaultValue(MsmqDefaults.UseSourceJournal)]
        public bool UseSourceJournal
        {
            get { return transport.UseSourceJournal; }
            set { transport.UseSourceJournal = value; }
        }

        [DefaultValue(MsmqDefaults.UseMsmqTracing)]
        public bool UseMsmqTracing
        {
            get { return transport.UseMsmqTracing; }
            set { transport.UseMsmqTracing = value; }
        }

        bool IBindingRuntimePreferences.ReceiveSynchronously
        {
            get { return this.ExactlyOnce; }
        }
    }
}
