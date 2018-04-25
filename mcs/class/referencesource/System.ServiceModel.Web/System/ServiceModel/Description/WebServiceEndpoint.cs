//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Description
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Text;
    using System.Xml;
    using System.ServiceModel.Web;

    public abstract class WebServiceEndpoint : ServiceEndpoint
    {
        internal WebServiceEndpoint(ContractDescription contract, EndpointAddress address)
            : base(contract, new WebHttpBinding(), address)
        { }

        public HostNameComparisonMode HostNameComparisonMode
        {
            get { return this.webHttpBinding.HostNameComparisonMode; }
            set { this.webHttpBinding.HostNameComparisonMode = value; }
        }

        public long MaxBufferPoolSize
        {
            get { return this.webHttpBinding.MaxBufferPoolSize; }
            set { this.webHttpBinding.MaxBufferPoolSize = value; }
        }

        public int MaxBufferSize
        {
            get { return this.webHttpBinding.MaxBufferSize; }
            set { this.webHttpBinding.MaxBufferSize = value; }
        }

        public long MaxReceivedMessageSize
        {
            get { return this.webHttpBinding.MaxReceivedMessageSize; }
            set { this.webHttpBinding.MaxReceivedMessageSize = value; }
        }

        public TransferMode TransferMode
        {
            get { return this.webHttpBinding.TransferMode; }
            set { this.webHttpBinding.TransferMode = value; }
        }

        public XmlDictionaryReaderQuotas ReaderQuotas
        {
            get { return this.webHttpBinding.ReaderQuotas; }
            set { this.webHttpBinding.ReaderQuotas = value; }
        }

        public WebHttpSecurity Security
        {
            get { return this.webHttpBinding.Security; }
        }

        public Encoding WriteEncoding
        {
            get { return this.webHttpBinding.WriteEncoding; }
            set { this.webHttpBinding.WriteEncoding = value; }
        }

        public WebContentTypeMapper ContentTypeMapper 
        {
            get { return this.webHttpBinding.ContentTypeMapper; }
            set { this.webHttpBinding.ContentTypeMapper = value; }
        }

        public bool CrossDomainScriptAccessEnabled 
        {
            get { return this.webHttpBinding.CrossDomainScriptAccessEnabled; }
            set
            {
                this.webHttpBinding.CrossDomainScriptAccessEnabled = value;
            }
        }

        protected abstract Type WebEndpointType { get; }

        WebHttpBinding webHttpBinding
        {
            get
            {
                WebHttpBinding webHttpBinding = this.Binding as WebHttpBinding;
                if (webHttpBinding == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR2.GetString(SR2.WebHttpBindingNotFoundWithEndpoint, WebEndpointType.Name, typeof(WebHttpBinding).Name)));
                }
                return webHttpBinding;
            }
        }
    }

}
