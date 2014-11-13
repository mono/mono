//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    abstract class MsmqReceiveParameters
    {
        MsmqUri.IAddressTranslator addressTranslator;
        bool durable;
        bool exactlyOnce;
        int maxRetryCycles;
        ReceiveErrorHandling receiveErrorHandling;
        int receiveRetryCount;
        TimeSpan retryCycleDelay;
        MsmqTransportSecurity transportSecurity;
        MsmqReceiveContextSettings receiveContextSettings;
        bool useMsmqTracing;
        bool useSourceJournal;
        
        internal MsmqReceiveParameters(MsmqBindingElementBase bindingElement)
            : this(bindingElement, bindingElement.AddressTranslator)
        {
        }

        internal MsmqReceiveParameters(MsmqBindingElementBase bindingElement, MsmqUri.IAddressTranslator addressTranslator)
        {
            this.addressTranslator = addressTranslator;
            this.durable = bindingElement.Durable;
            this.exactlyOnce = bindingElement.ExactlyOnce;
            this.maxRetryCycles = bindingElement.MaxRetryCycles;
            this.receiveErrorHandling = bindingElement.ReceiveErrorHandling;
            this.receiveRetryCount = bindingElement.ReceiveRetryCount;
            this.retryCycleDelay = bindingElement.RetryCycleDelay;
            this.transportSecurity = new MsmqTransportSecurity(bindingElement.MsmqTransportSecurity);
            this.useMsmqTracing = bindingElement.UseMsmqTracing;
            this.useSourceJournal = bindingElement.UseSourceJournal;
            this.receiveContextSettings = new MsmqReceiveContextSettings(bindingElement.ReceiveContextSettings);
        }

        internal MsmqReceiveContextSettings ReceiveContextSettings
        {
            get { return this.receiveContextSettings; }
        }

        internal MsmqUri.IAddressTranslator AddressTranslator
        {
            get { return this.addressTranslator; }
        }

        internal bool Durable
        {
            get { return this.durable; }
        }

        internal bool ExactlyOnce
        {
            get { return this.exactlyOnce; }
        }

        internal int ReceiveRetryCount
        {
            get { return this.receiveRetryCount; }
        }

        internal int MaxRetryCycles
        {
            get { return this.maxRetryCycles; }
        }

        internal ReceiveErrorHandling ReceiveErrorHandling
        {
            get { return this.receiveErrorHandling; }
        }

        internal TimeSpan RetryCycleDelay
        {
            get { return this.retryCycleDelay; }
        }

        internal MsmqTransportSecurity TransportSecurity
        {
            get { return this.transportSecurity; }
        }
        
        internal bool UseMsmqTracing
        {
            get { return this.useMsmqTracing; }
        }

        internal bool UseSourceJournal
        {
            get { return this.useSourceJournal; }
        }
    }
}
