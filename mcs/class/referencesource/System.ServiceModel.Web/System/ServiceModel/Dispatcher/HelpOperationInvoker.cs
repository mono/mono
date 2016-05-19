//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

using System;
using System.Globalization;
using System.Net;
using System.Runtime;
using System.Web;
using System.ServiceModel.Syndication;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;

namespace System.ServiceModel.Web
{
    class HelpOperationInvoker : IOperationInvoker
    {
        HelpPage helpPage;
        IOperationInvoker unhandledDispatchOperation;        

        public const string OperationName = "HelpPageInvoke";

        public HelpOperationInvoker(HelpPage helpPage, IOperationInvoker unhandledDispatchOperation)
        {
            this.helpPage = helpPage;
            this.unhandledDispatchOperation = unhandledDispatchOperation;
        }

        public object[] AllocateInputs()
        {
            return new object[] { null };
        }

        public object Invoke(object instance, object[] inputs, out object[] outputs)
        {
            outputs = null;
            UriTemplateMatch match = (UriTemplateMatch)OperationContext.Current.IncomingMessageProperties[IncomingWebRequestContext.UriTemplateMatchResultsPropertyName];
            return this.helpPage.Invoke(match);
        }

        public IAsyncResult InvokeBegin(object instance, object[] inputs, AsyncCallback callback, object state)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
        }

        public object InvokeEnd(object instance, out object[] outputs, IAsyncResult result)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
        }

        public bool IsSynchronous
        {
            get { return true; }
        }        
    }
}
