//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Description
{
    using System;
    using System.Globalization;
    using System.ServiceModel;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Channels;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.ServiceModel.Web;
    using System.Text;
    using System.Xml;

    [DebuggerDisplay("Address={address}")]
    [DebuggerDisplay("Name={name}")]
    public class WebScriptEndpoint : WebServiceEndpoint
    {
        static Type WebScriptEndpointType = typeof(WebScriptEndpoint);

        public WebScriptEndpoint(ContractDescription contract) :
            this(contract, null /* address */)
        { }

        public WebScriptEndpoint(ContractDescription contract, EndpointAddress address)
            : base(contract, address)
        {
            this.Behaviors.Add(new WebScriptEnablingBehavior());
        }

        WebScriptEnablingBehavior webScriptEnablingBehavior
        {
            get 
            {
                WebScriptEnablingBehavior webScriptEnablingBehavior = this.Behaviors.Find<WebScriptEnablingBehavior>();
                if (webScriptEnablingBehavior == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR2.GetString(SR2.WebBehaviorNotFoundWithEndpoint, WebEndpointType.Name, typeof(WebScriptEnablingBehavior).Name)));
                }
                return webScriptEnablingBehavior;
            }
        }

        protected override Type WebEndpointType
        {
            get { return WebScriptEndpointType; }
        }
    }

}
