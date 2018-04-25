//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Description
{
    using System;
    using System.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Web;

    [DebuggerDisplay("Address={address}")]
    [DebuggerDisplay("Name={name}")]
    public class WebHttpEndpoint : WebServiceEndpoint
    {
        static Type WebHttpEndpointType = typeof(WebHttpEndpoint);

        public WebHttpEndpoint(ContractDescription contract) :
            this(contract, null /* address */)
        { }

        public WebHttpEndpoint(ContractDescription contract, EndpointAddress address)
            : base(contract, address)
        {
            this.Behaviors.Add(new WebHttpBehavior());
        }

        public bool HelpEnabled
        {
            get { return this.WebHttpBehavior.HelpEnabled; }
            set { this.WebHttpBehavior.HelpEnabled = value; }
        }

        public WebMessageFormat DefaultOutgoingResponseFormat
        {
            get { return this.WebHttpBehavior.DefaultOutgoingResponseFormat; }
            set { this.WebHttpBehavior.DefaultOutgoingResponseFormat = value; }
        }

        public bool AutomaticFormatSelectionEnabled
        {
            get { return this.WebHttpBehavior.AutomaticFormatSelectionEnabled; }
            set { this.WebHttpBehavior.AutomaticFormatSelectionEnabled = value; }
        }

        public bool FaultExceptionEnabled
        {
            get { return this.WebHttpBehavior.FaultExceptionEnabled; }
            set { this.WebHttpBehavior.FaultExceptionEnabled = value; }
        }

        WebHttpBehavior WebHttpBehavior
        {
            get 
            {
                WebHttpBehavior webHttpBehavior = this.Behaviors.Find<WebHttpBehavior>();
                if (webHttpBehavior == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR2.GetString(SR2.WebBehaviorNotFoundWithEndpoint, typeof(WebHttpEndpoint).Name, typeof(WebHttpBehavior).Name)));                
                }
                return webHttpBehavior;
            }
        }

        protected override Type WebEndpointType
        {
            get { return WebHttpEndpointType; }
        }
    }

}
