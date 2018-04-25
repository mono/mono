//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Configuration;
    using System.Net.Security;
    using System.ServiceModel.Channels;

    public sealed partial class MsmqTransportElement : MsmqElementBase
    {
        [ConfigurationProperty(ConfigurationStrings.MaxPoolSize, DefaultValue = MsmqDefaults.MaxPoolSize)]
        [IntegerValidator(MinValue = 0)]
        public int MaxPoolSize
        {
            get { return (int)base[ConfigurationStrings.MaxPoolSize]; }
            set { base[ConfigurationStrings.MaxPoolSize] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.QueueTransferProtocol, DefaultValue = MsmqDefaults.QueueTransferProtocol)]
        [ServiceModelEnumValidator(typeof(QueueTransferProtocolHelper))]
        public QueueTransferProtocol QueueTransferProtocol
        {
            get { return (QueueTransferProtocol)base[ConfigurationStrings.QueueTransferProtocol]; }
            set { base[ConfigurationStrings.QueueTransferProtocol] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.UseActiveDirectory, DefaultValue = MsmqDefaults.UseActiveDirectory)]
        public bool UseActiveDirectory
        {
            get { return (bool)base[ConfigurationStrings.UseActiveDirectory]; }
            set { base[ConfigurationStrings.UseActiveDirectory] = value; }
        }

        public override Type BindingElementType
        {
            get { return typeof(MsmqTransportBindingElement); }
        }

        protected override TransportBindingElement CreateDefaultBindingElement()
        {
            return new MsmqTransportBindingElement();
        }


        public override void ApplyConfiguration(BindingElement bindingElement)
        {
            base.ApplyConfiguration(bindingElement);

            MsmqTransportBindingElement binding = bindingElement as MsmqTransportBindingElement;
            binding.MaxPoolSize = this.MaxPoolSize;
            binding.QueueTransferProtocol = this.QueueTransferProtocol;
            binding.UseActiveDirectory = this.UseActiveDirectory;
        }

        public override void CopyFrom(ServiceModelExtensionElement from)
        {
            base.CopyFrom(from);

            MsmqTransportElement source = from as MsmqTransportElement;
            if (null != source)
            {
                this.MaxPoolSize = source.MaxPoolSize;
                this.QueueTransferProtocol = source.QueueTransferProtocol;
                this.UseActiveDirectory = source.UseActiveDirectory;
            }
        }

        protected internal override void InitializeFrom(BindingElement bindingElement)
        {
            base.InitializeFrom(bindingElement);
            MsmqTransportBindingElement binding = bindingElement as MsmqTransportBindingElement;
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.MaxPoolSize, binding.MaxPoolSize);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.QueueTransferProtocol, binding.QueueTransferProtocol);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.UseActiveDirectory, binding.UseActiveDirectory);
        }
    }
}



