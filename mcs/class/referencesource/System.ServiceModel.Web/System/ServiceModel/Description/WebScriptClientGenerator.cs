//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Description
{
    using System;
    using System.Globalization;
    using System.Net;
    using System.Runtime.Serialization.Json;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;
    using System.Web;
 
    class WebScriptClientGenerator : ServiceMetadataExtension.IHttpGetMetadata
    {
        internal const string DebugMetadataEndpointSuffix = "jsdebug";
        internal const string MetadataEndpointSuffix = "js";
        bool debugMode;
        ServiceEndpoint endpoint;
        NameValueCache<string> proxyCache;
        DateTime serviceLastModified;
        string serviceLastModifiedRfc1123String;
        bool crossDomainScriptAccessEnabled;

        public WebScriptClientGenerator(ServiceEndpoint endpoint, bool debugMode, bool crossDomainScriptAccessEnabled)
        {
            this.endpoint = endpoint;
            this.debugMode = debugMode;
            // The service host is automatically restarted every time a service or any of its dependencies change
            // A restart adds all the behaviors from scratch. 
            // => WebScriptEnablingBehavior plugs in this contract for the "/js" endpoint afresh if the service changes.
            this.serviceLastModified = DateTime.UtcNow;
            // Zero out all millisecond and sub-millisecond information because RFC1123 doesn't support milliseconds.
            // => The parsed If-Modified-Since date, against which serviceLastModified will need to be compared,
            //    won't have milliseconds.
            this.serviceLastModified = new DateTime(this.serviceLastModified.Year, this.serviceLastModified.Month, this.serviceLastModified.Day, this.serviceLastModified.Hour, this.serviceLastModified.Minute, this.serviceLastModified.Second, DateTimeKind.Utc);
            this.proxyCache = new NameValueCache<string>();
            this.crossDomainScriptAccessEnabled = crossDomainScriptAccessEnabled;
        }

        string GetProxyContent(Uri baseUri)
        {
            string proxy = this.proxyCache.Lookup(baseUri.Authority);
            if (String.IsNullOrEmpty(proxy))
            {
                proxy = WCFServiceClientProxyGenerator.GetClientProxyScript(this.endpoint.Contract.ContractType, baseUri.AbsoluteUri, this.debugMode, endpoint);
                this.proxyCache.AddOrUpdate(baseUri.Authority, proxy);
            }
            return proxy;
        }

        string ServiceLastModifiedRfc1123String
        {
            get
            {
                if (serviceLastModifiedRfc1123String == null)
                {
                    // "R" for RFC1123. The HTTP standard requires that dates be serialized according to RFC1123.
                    serviceLastModifiedRfc1123String = serviceLastModified.ToString("R", DateTimeFormatInfo.InvariantInfo);
                }
                return serviceLastModifiedRfc1123String;
            }
        }

        public Message Get(Message message)
        {
            HttpRequestMessageProperty requestMessageProperty = (HttpRequestMessageProperty) message.Properties[HttpRequestMessageProperty.Name];
            HttpResponseMessageProperty responseMessageProperty = new HttpResponseMessageProperty();

            if ((requestMessageProperty != null) && IsServiceUnchanged(requestMessageProperty.Headers[JsonGlobals.IfModifiedSinceString]))
            {
                Message responseMessage = Message.CreateMessage(MessageVersion.None, string.Empty);
                responseMessageProperty.StatusCode = HttpStatusCode.NotModified;
                responseMessage.Properties.Add(HttpResponseMessageProperty.Name, responseMessageProperty);
                return responseMessage;
            }

            string proxyContent = this.GetProxyContent(UriTemplate.RewriteUri(this.endpoint.Address.Uri, requestMessageProperty.Headers[HttpRequestHeader.Host]));
            Message response = new WebScriptMetadataMessage(string.Empty, proxyContent);
            responseMessageProperty.Headers.Add(JsonGlobals.LastModifiedString, ServiceLastModifiedRfc1123String);
            responseMessageProperty.Headers.Add(JsonGlobals.ExpiresString, ServiceLastModifiedRfc1123String);
            if (AspNetEnvironment.Current.AspNetCompatibilityEnabled)
            {
                HttpContext.Current.Response.Cache.SetCacheability(HttpCacheability.Public);
            }
            else
            {
                responseMessageProperty.Headers.Add(JsonGlobals.CacheControlString, JsonGlobals.publicString);
            }
            response.Properties.Add(HttpResponseMessageProperty.Name, responseMessageProperty);
            return response;
        }

        internal static string GetMetadataEndpointSuffix(bool debugMode)
        {
            if (debugMode)
            {
                return DebugMetadataEndpointSuffix;
            }
            else
            {
                return MetadataEndpointSuffix;
            }
        }

        bool IsServiceUnchanged(string ifModifiedSinceHeaderValue)
        {
            if (string.IsNullOrEmpty(ifModifiedSinceHeaderValue))
            {
                return false;
            }

            DateTime ifModifiedSinceDateTime;
            if (DateTime.TryParse(ifModifiedSinceHeaderValue, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AdjustToUniversal, out ifModifiedSinceDateTime))
            {
                return (ifModifiedSinceDateTime >= serviceLastModified);
            }

            return false;
        }
    }
}
