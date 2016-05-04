//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
#pragma warning disable 1634, 1691
namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Web;
    using System.Net;

    public class WebHttpDispatchOperationSelector : IDispatchOperationSelector
    {
        public const string HttpOperationSelectorUriMatchedPropertyName = "UriMatched";
        internal const string HttpOperationSelectorDataPropertyName = "HttpOperationSelectorData";

        // 
        public const string HttpOperationNamePropertyName = "HttpOperationName";
        internal const string redirectOperationName = ""; // always unhandled invoker
        internal const string RedirectPropertyName = "WebHttpRedirect";

        string catchAllOperationName = ""; // user UT=* Method=* operation, else unhandled invoker

        Dictionary<string, UriTemplateTable> methodSpecificTables; // indexed by the http method name
        UriTemplateTable wildcardTable; // this is one of the methodSpecificTables, special-cased for faster access
        Dictionary<string, UriTemplate> templates;

        UriTemplateTable helpUriTable;

        public WebHttpDispatchOperationSelector(ServiceEndpoint endpoint)
        {
            if (endpoint == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpoint");
            }
            if (endpoint.Address == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                    SR2.GetString(SR2.EndpointAddressCannotBeNull)));
            }
#pragma warning disable 56506 // [....], endpoint.Address.Uri is never null
            Uri baseUri = endpoint.Address.Uri;
            this.methodSpecificTables = new Dictionary<string, UriTemplateTable>();
            this.templates = new Dictionary<string, UriTemplate>();
#pragma warning restore 56506

            WebHttpBehavior webHttpBehavior = endpoint.Behaviors.Find<WebHttpBehavior>();
            if (webHttpBehavior != null && webHttpBehavior.HelpEnabled)
            {
                this.helpUriTable = new UriTemplateTable(endpoint.ListenUri, HelpPage.GetOperationTemplatePairs());
            }

            Dictionary<WCFKey, string> alreadyHaves = new Dictionary<WCFKey, string>();

#pragma warning disable 56506 // [....], endpoint.Contract is never null
            foreach (OperationDescription od in endpoint.Contract.Operations)
#pragma warning restore 56506
            {
                // ignore callback operations
                if (od.Messages[0].Direction == MessageDirection.Input)
                {
                    string method = WebHttpBehavior.GetWebMethod(od);
                    string path = UriTemplateClientFormatter.GetUTStringOrDefault(od);

                    // 

                    if (UriTemplateHelpers.IsWildcardPath(path) && (method == WebHttpBehavior.WildcardMethod))
                    {
                        if (this.catchAllOperationName != "")
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                                new InvalidOperationException(
                                SR2.GetString(SR2.MultipleOperationsInContractWithPathMethod,
                                endpoint.Contract.Name, path, method)));
                        }
                        this.catchAllOperationName = od.Name;
                    }
                    UriTemplate ut = new UriTemplate(path);
                    WCFKey wcfKey = new WCFKey(ut, method);
                    if (alreadyHaves.ContainsKey(wcfKey))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                            new InvalidOperationException(
                            SR2.GetString(SR2.MultipleOperationsInContractWithPathMethod,
                            endpoint.Contract.Name, path, method)));
                    }
                    alreadyHaves.Add(wcfKey, od.Name);

                    UriTemplateTable methodSpecificTable;
                    if (!methodSpecificTables.TryGetValue(method, out methodSpecificTable))
                    {
                        methodSpecificTable = new UriTemplateTable(baseUri);
                        methodSpecificTables.Add(method, methodSpecificTable);
                    }

                    methodSpecificTable.KeyValuePairs.Add(new KeyValuePair<UriTemplate, object>(ut, od.Name));
                    this.templates.Add(od.Name, ut);
                }
            }

            if (this.methodSpecificTables.Count == 0)
            {
                this.methodSpecificTables = null;
            }
            else
            {
                // freeze all the tables because they should not be modified after this point
                foreach (UriTemplateTable table in this.methodSpecificTables.Values)
                {
                    table.MakeReadOnly(true /* allowDuplicateEquivalentUriTemplates */);
                }

                if (!methodSpecificTables.TryGetValue(WebHttpBehavior.WildcardMethod, out wildcardTable))
                {
                    wildcardTable = null;
                }
            }
        }

        protected WebHttpDispatchOperationSelector()
        {
        }

        public virtual UriTemplate GetUriTemplate(string operationName)
        {
            if (operationName == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("operationName");
            }
            UriTemplate result;
            if (!this.templates.TryGetValue(operationName, out result))
            {
                return null;
            }
            else
            {
                return result;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "0#", Justification = "This method is defined by the IDispatchOperationSelector interface")]
        public string SelectOperation(ref Message message)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
            bool uriMatched;
            string result = this.SelectOperation(ref message, out uriMatched);
#pragma warning disable 56506 // [....], Message.Properties is never null
            message.Properties.Add(HttpOperationSelectorUriMatchedPropertyName, uriMatched);
#pragma warning restore 56506
            if (result != null)
            {
                message.Properties.Add(HttpOperationNamePropertyName, result);
                if (DiagnosticUtility.ShouldTraceInformation)
                {
#pragma warning disable 56506 // [....], Message.Headers is never null
                    TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.WebRequestMatchesOperation, SR2.GetString(SR2.TraceCodeWebRequestMatchesOperation, message.Headers.To, result));
#pragma warning restore 56506
                }
            }
            return result;
        }

        [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "0#", Justification = "This method is like that defined by the IDispatchOperationSelector interface")]
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#", Justification = "This API needs to return multiple things")]
        protected virtual string SelectOperation(ref Message message, out bool uriMatched)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
            uriMatched = false;
            if (this.methodSpecificTables == null)
            {
                return this.catchAllOperationName;
            }

#pragma warning disable 56506 // [....], message.Properties is never null
            if (!message.Properties.ContainsKey(HttpRequestMessageProperty.Name))
            {
                return this.catchAllOperationName;
            }
            HttpRequestMessageProperty prop = message.Properties[HttpRequestMessageProperty.Name] as HttpRequestMessageProperty;
            if (prop == null)
            {
                return this.catchAllOperationName;
            }
            string method = prop.Method;

            Uri to = message.Headers.To;
#pragma warning restore 56506

            if (to == null)
            {
                return this.catchAllOperationName;
            }

            if (this.helpUriTable != null)
            {
                UriTemplateMatch match = this.helpUriTable.MatchSingle(to);
                if (match != null)
                {
                    uriMatched = true;
                    AddUriTemplateMatch(match, prop, message);
                    if (method == WebHttpBehavior.GET)
                    {
                        return HelpOperationInvoker.OperationName;
                    }
                    message.Properties.Add(WebHttpDispatchOperationSelector.HttpOperationSelectorDataPropertyName,
                        new WebHttpDispatchOperationSelectorData() { AllowedMethods = new List<string>() { WebHttpBehavior.GET } });
                    return this.catchAllOperationName;
                }
            }

            UriTemplateTable methodSpecificTable;
            bool methodMatchesExactly = methodSpecificTables.TryGetValue(method, out methodSpecificTable);
            if (methodMatchesExactly)
            {
                string operationName;
                uriMatched = CanUriMatch(methodSpecificTable, to, prop, message, out operationName);
                if (uriMatched)
                {
                    return operationName;
                }
            }

            if (wildcardTable != null)
            {
                string operationName;
                uriMatched = CanUriMatch(wildcardTable, to, prop, message, out operationName);
                if (uriMatched)
                {
                    return operationName;
                }
            }

            if (ShouldRedirectToUriWithSlashAtTheEnd(methodSpecificTable, message, to))
            {
                return redirectOperationName;
            }

            // the {method, uri} pair does not match anything the service supports.
            // we know at this point that we'll return some kind of error code, but we 
            // should go through all methods for the uri to see if any method is supported
            // so that that information could be returned to the user as well

            List<string> allowedMethods = null;
            foreach (KeyValuePair<string, UriTemplateTable> pair in methodSpecificTables)
            {
                if (pair.Key == method || pair.Key == WebHttpBehavior.WildcardMethod)
                {
                    // the uri must not match the uri template
                    continue;
                }
                UriTemplateTable table = pair.Value;
                if (table.MatchSingle(to) != null)
                {
                    if (allowedMethods == null)
                    {
                        allowedMethods = new List<string>();
                    }

                    // 

                    if (!allowedMethods.Contains(pair.Key))
                    {
                        allowedMethods.Add(pair.Key);
                    }
                }
            }

            if (allowedMethods != null)
            {
                uriMatched = true;
                message.Properties.Add(WebHttpDispatchOperationSelector.HttpOperationSelectorDataPropertyName,
                    new WebHttpDispatchOperationSelectorData() { AllowedMethods = allowedMethods });
            }
            return catchAllOperationName;
        }

        bool CanUriMatch(UriTemplateTable methodSpecificTable, Uri to, HttpRequestMessageProperty prop, Message message, out string operationName)
        {
            operationName = null;
            UriTemplateMatch result = methodSpecificTable.MatchSingle(to);

            if (result != null)
            {
                operationName = result.Data as string;
                Fx.Assert(operationName != null, "bad result");
                AddUriTemplateMatch(result, prop, message);
                return true;
            }
            return false;
        }

        void AddUriTemplateMatch(UriTemplateMatch match, HttpRequestMessageProperty requestProp, Message message)
        {
            match.SetBaseUri(match.BaseUri, requestProp);
            message.Properties.Add(IncomingWebRequestContext.UriTemplateMatchResultsPropertyName, match);
        }

        bool ShouldRedirectToUriWithSlashAtTheEnd(UriTemplateTable methodSpecificTable, Message message, Uri to)
        {
            UriBuilder ub = new UriBuilder(to);
            if (ub.Path.EndsWith("/", StringComparison.Ordinal))
            {
                return false;
            }

            ub.Path = ub.Path + "/";
            Uri originalPlusSlash = ub.Uri;

            bool result = false;
            if (methodSpecificTable != null && methodSpecificTable.MatchSingle(originalPlusSlash) != null)
            {
                // as an optimization, we check the table that matched the request's method
                // first, as it is more probable that a hit happens there
                result = true;
            }
            else
            {
                // back-compat:
                // we will redirect as long as there is any method 
                // - not necessary the one the user is looking for -
                // that matches the uri with a slash at the end

                foreach (KeyValuePair<string, UriTemplateTable> pair in methodSpecificTables)
                {
                    UriTemplateTable table = pair.Value;
                    if (table != methodSpecificTable && table.MatchSingle(originalPlusSlash) != null)
                    {
                        result = true;
                        break;
                    }
                }
            }

            if (result)
            {
                string hostAndPort = GetAuthority(message);
                originalPlusSlash = UriTemplate.RewriteUri(ub.Uri, hostAndPort);
                message.Properties.Add(RedirectPropertyName, originalPlusSlash);
            }
            return result;
        }

        static string GetAuthority(Message message)
        {
            HttpRequestMessageProperty requestProperty;
            string hostName = null;
            if (message.Properties.TryGetValue(HttpRequestMessageProperty.Name, out requestProperty))
            {
                hostName = requestProperty.Headers[HttpRequestHeader.Host];
                if (!string.IsNullOrEmpty(hostName))
                {
                    return hostName;
                }
            }
            IAspNetMessageProperty aspNetMessageProperty = AspNetEnvironment.Current.GetHostingProperty(message);
            if (aspNetMessageProperty != null)
            {
                hostName = aspNetMessageProperty.OriginalRequestUri.Authority;
            }
            return hostName;
        }

        // to enforce that no two ops have same UriTemplate & Method
        class WCFKey
        {
            string method;
            UriTemplate uriTemplate;
            public WCFKey(UriTemplate uriTemplate, string method)
            {
                this.uriTemplate = uriTemplate;
                this.method = method;
            }
            public override bool Equals(object obj)
            {
                WCFKey other = obj as WCFKey;
                if (other == null)
                {
                    return false;
                }
                return this.uriTemplate.IsEquivalentTo(other.uriTemplate) && this.method == other.method;
            }
            public override int GetHashCode()
            {
                return UriTemplateEquivalenceComparer.Instance.GetHashCode(this.uriTemplate);
            }
        }
    }
}

