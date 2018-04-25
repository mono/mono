//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    sealed class MsmqTransportReceiveParameters
        : MsmqReceiveParameters
    {
        int maxPoolSize;
        bool useActiveDirectory;
        QueueTransferProtocol queueTransferProtocol;

        internal MsmqTransportReceiveParameters(MsmqTransportBindingElement bindingElement, MsmqUri.IAddressTranslator addressTranslator)
            : base(bindingElement, addressTranslator)
        {
            this.maxPoolSize = bindingElement.MaxPoolSize;
            this.useActiveDirectory = bindingElement.UseActiveDirectory;
            this.queueTransferProtocol = bindingElement.QueueTransferProtocol;
        }

        internal int MaxPoolSize
        {
            get { return this.maxPoolSize; }
        }

        internal bool UseActiveDirectory
        {
            get { return this.useActiveDirectory; }
        }

        internal QueueTransferProtocol QueueTransferProtocol
        {
            get { return this.queueTransferProtocol; }
        }
    }
}
