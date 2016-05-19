//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Web
{
    using System;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Web;
    using System.ServiceModel.Administration;
    using System.Collections.Generic;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class JavascriptCallbackBehaviorAttribute : Attribute, IContractBehavior
    {
        string urlParameterName;

        public string UrlParameterName
        {
            get { return urlParameterName; }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }
                else
                {
                    urlParameterName = value;
                }
            }
        }

        public JavascriptCallbackBehaviorAttribute()
        {
        }

        public void AddBindingParameters(ContractDescription contractDescription, ServiceEndpoint endpoint, System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyClientBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint, System.ServiceModel.Dispatcher.ClientRuntime clientRuntime)
        {
        }

        public void ApplyDispatchBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint, System.ServiceModel.Dispatcher.DispatchRuntime dispatchRuntime)
        {
        }

        public void Validate(ContractDescription contractDescription, ServiceEndpoint endpoint)
        {

        }
    }
}

