//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Configuration;
    using System.ServiceModel;
    using System.Globalization;
    using System.ServiceModel.Security;
    using System.ServiceModel.Channels;
    using System.Net.Security;

    public partial class NetMsmqBindingElement : MsmqBindingElementBase
    {
        public NetMsmqBindingElement(string name)
            : base(name)
        {
        }

        public NetMsmqBindingElement()
            : this(null)
        {
        }

        protected override Type BindingElementType
        {
            get { return typeof(NetMsmqBinding); }
        }


        [ConfigurationProperty(ConfigurationStrings.QueueTransferProtocol, DefaultValue = MsmqDefaults.QueueTransferProtocol)]
        [ServiceModelEnumValidator(typeof(QueueTransferProtocolHelper))]
        public QueueTransferProtocol QueueTransferProtocol
        {
            get { return (QueueTransferProtocol)base[ConfigurationStrings.QueueTransferProtocol]; }
            set { base[ConfigurationStrings.QueueTransferProtocol] = value; }
        }


        [ConfigurationProperty(ConfigurationStrings.ReaderQuotas)]
        public XmlDictionaryReaderQuotasElement ReaderQuotas
        {
            get { return (XmlDictionaryReaderQuotasElement)base[ConfigurationStrings.ReaderQuotas]; }
        }

        [ConfigurationProperty(ConfigurationStrings.MaxBufferPoolSize, DefaultValue = TransportDefaults.MaxBufferPoolSize)]
        [LongValidator(MinValue = 0)]
        public long MaxBufferPoolSize
        {
            get { return (long)base[ConfigurationStrings.MaxBufferPoolSize]; }
            set { base[ConfigurationStrings.MaxBufferPoolSize] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.Security)]
        public NetMsmqSecurityElement Security
        {
            get { return (NetMsmqSecurityElement)base[ConfigurationStrings.Security]; }
        }

        [ConfigurationProperty(ConfigurationStrings.UseActiveDirectory, DefaultValue = MsmqDefaults.UseActiveDirectory)]
        public bool UseActiveDirectory
        {
            get { return (bool)base[ConfigurationStrings.UseActiveDirectory]; }
            set { base[ConfigurationStrings.UseActiveDirectory] = value; }
        }

        protected internal override void InitializeFrom(Binding binding)
        {
            base.InitializeFrom(binding);
            NetMsmqBinding npmBinding = (NetMsmqBinding)binding;

            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.MaxBufferPoolSize, npmBinding.MaxBufferPoolSize);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.QueueTransferProtocol, npmBinding.QueueTransferProtocol);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.UseActiveDirectory, npmBinding.UseActiveDirectory);

            this.Security.InitializeFrom(npmBinding.Security);
            this.ReaderQuotas.InitializeFrom(npmBinding.ReaderQuotas);
        }

        protected override void OnApplyConfiguration(Binding binding)
        {
            base.OnApplyConfiguration(binding);
            NetMsmqBinding npmBinding = (NetMsmqBinding)binding;

            npmBinding.MaxBufferPoolSize = this.MaxBufferPoolSize;
            npmBinding.QueueTransferProtocol = this.QueueTransferProtocol;
            npmBinding.UseActiveDirectory = this.UseActiveDirectory;

            this.Security.ApplyConfiguration(npmBinding.Security);
            this.ReaderQuotas.ApplyConfiguration(npmBinding.ReaderQuotas);
        }
    }
}
