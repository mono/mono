//------------------------------------------------------------  
// Copyright (c) Microsoft Corporation.  All rights reserved.   
//------------------------------------------------------------  
namespace System.ServiceModel.Channels
{
    using System.Runtime.Serialization;
    using System.ServiceModel;

    abstract class MsmqChannelFactory<TChannel> : MsmqChannelFactoryBase<TChannel>
    {
        int maxPoolSize;
        QueueTransferProtocol queueTransferProtocol;
        bool useActiveDirectory;

        protected MsmqChannelFactory(MsmqTransportBindingElement bindingElement, BindingContext context)
            : base(bindingElement, context)
        {
            this.maxPoolSize = bindingElement.MaxPoolSize;
            this.queueTransferProtocol = bindingElement.QueueTransferProtocol;
            this.useActiveDirectory = bindingElement.UseActiveDirectory;
        }

        public int MaxPoolSize
        {
            get { return this.maxPoolSize; }
        }

        public QueueTransferProtocol QueueTransferProtocol
        {
            get { return this.queueTransferProtocol; }
        }

        public bool UseActiveDirectory
        {
            get { return this.useActiveDirectory; }
        }
    }

    sealed class MsmqOutputChannelFactory : MsmqChannelFactory<IOutputChannel>
    {
        internal MsmqOutputChannelFactory(MsmqTransportBindingElement bindingElement, BindingContext context)
            : base(bindingElement, context)
        {
        }

        protected override IOutputChannel OnCreateChannel(EndpointAddress to, Uri via)
        {
            base.ValidateScheme(via);
            return new MsmqOutputChannel(this, to, via, ManualAddressing);
        }
    }

    sealed class MsmqOutputSessionChannelFactory : MsmqChannelFactory<IOutputSessionChannel>
    {
        internal MsmqOutputSessionChannelFactory(MsmqTransportBindingElement bindingElement, BindingContext context)
            : base(bindingElement, context)
        {
        }

        protected override IOutputSessionChannel OnCreateChannel(EndpointAddress to, Uri via)
        {
            base.ValidateScheme(via);
            return new MsmqOutputSessionChannel(this, to, via, ManualAddressing);
        }
    }
}
