//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
#pragma warning disable 1634, 1691
namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Reflection;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;
    using System.Text;
    using System.Xml;
    using System.ServiceModel.Web;
    using System.Diagnostics;

    class UriTemplateDispatchFormatter : IDispatchMessageFormatter
    {
        internal Dictionary<int, string> pathMapping;
        internal Dictionary<int, KeyValuePair<string, Type>> queryMapping;
        Uri baseAddress;
        IDispatchMessageFormatter inner;
        string operationName;
        QueryStringConverter qsc;
        int totalNumUTVars;
        UriTemplate uriTemplate;

        public UriTemplateDispatchFormatter(OperationDescription operationDescription, IDispatchMessageFormatter inner, QueryStringConverter qsc, string contractName, Uri baseAddress)
        {
            this.inner = inner;
            this.qsc = qsc;
            this.baseAddress = baseAddress;
            this.operationName = operationDescription.Name;
            UriTemplateClientFormatter.Populate(out this.pathMapping,
                out this.queryMapping,
                out this.totalNumUTVars,
                out this.uriTemplate,
                operationDescription,
                qsc,
                contractName);
        }

        public void DeserializeRequest(Message message, object[] parameters)
        {
            object[] innerParameters = new object[parameters.Length - this.totalNumUTVars];
            if (innerParameters.Length != 0)
            {
                this.inner.DeserializeRequest(message, innerParameters);
            }
            int j = 0;
            UriTemplateMatch utmr = null;
            string UTMRName = IncomingWebRequestContext.UriTemplateMatchResultsPropertyName;
            if (message.Properties.ContainsKey(UTMRName))
            {
                utmr = message.Properties[UTMRName] as UriTemplateMatch;
            }
            else
            {
                if (message.Headers.To != null && message.Headers.To.IsAbsoluteUri)
                {
                    utmr = this.uriTemplate.Match(this.baseAddress, message.Headers.To);
                }
            }
            NameValueCollection nvc = (utmr == null) ? new NameValueCollection() : utmr.BoundVariables;
            for (int i = 0; i < parameters.Length; ++i)
            {
                if (this.pathMapping.ContainsKey(i) && utmr != null)
                {
                    parameters[i] = nvc[this.pathMapping[i]];
                }
                else if (this.queryMapping.ContainsKey(i) && utmr != null)
                {
                    string queryVal = nvc[this.queryMapping[i].Key];
                    parameters[i] = this.qsc.ConvertStringToValue(queryVal, this.queryMapping[i].Value);
                }
                else
                {
                    parameters[i] = innerParameters[j];
                    ++j;
                }
            }
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                if (utmr != null)
                {
                    foreach (string key in utmr.QueryParameters.Keys)
                    {
                        bool isParameterIgnored = true;
                        foreach (KeyValuePair<string, Type> kvp in this.queryMapping.Values)
                        {
                            if (String.Compare(key, kvp.Key, StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                isParameterIgnored = false;
                                break;
                            }
                        }
                        if (isParameterIgnored)
                        {
                            TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.WebUnknownQueryParameterIgnored, SR2.GetString(SR2.TraceCodeWebRequestUnknownQueryParameterIgnored, key, operationName));
                        }
                    }
                }
            }
        }

        public Message SerializeReply(MessageVersion messageVersion, object[] parameters, object result)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR2.GetString(SR2.QueryStringFormatterOperationNotSupportedServerSide)));
        }
    }
}
