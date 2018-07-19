//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Configuration;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;

    public sealed partial class DispatcherSynchronizationElement : BehaviorExtensionElement
    {
        public DispatcherSynchronizationElement()
        {
        }

        [ConfigurationProperty(ConfigurationStrings.AsynchronousSendEnabled, DefaultValue = false)]
        public bool AsynchronousSendEnabled 
        {
            get { return (bool)base[ConfigurationStrings.AsynchronousSendEnabled]; }
            set { base[ConfigurationStrings.AsynchronousSendEnabled] = value; }
        }
        
        [ConfigurationProperty(ConfigurationStrings.MaxPendingReceives,
            DefaultValue = MultipleReceiveBinder.MultipleReceiveDefaults.MaxPendingReceives)]
        [IntegerValidator(MinValue = 1)]
        public int MaxPendingReceives
        {
            get { return (int)base[ConfigurationStrings.MaxPendingReceives]; }
            set { base[ConfigurationStrings.MaxPendingReceives] = value; }
        }


        public override Type BehaviorType
        {
            get { return typeof(DispatcherSynchronizationBehavior); }
        }

        public override void CopyFrom(ServiceModelExtensionElement from)
        {
            base.CopyFrom(from);
            DispatcherSynchronizationElement source = (DispatcherSynchronizationElement)from;
            this.AsynchronousSendEnabled = source.AsynchronousSendEnabled;
            this.MaxPendingReceives = source.MaxPendingReceives;
        }

        protected internal override object CreateBehavior()
        {
            return new DispatcherSynchronizationBehavior(this.AsynchronousSendEnabled, this.MaxPendingReceives);
        }
    }
}
