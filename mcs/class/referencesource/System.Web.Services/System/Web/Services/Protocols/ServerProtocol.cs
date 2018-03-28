//------------------------------------------------------------------------------
// <copyright file="ServerProtocol.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Protocols {
    using System;
    using System.Diagnostics;
    using System.Collections;
    using System.IO;
    using System.Reflection;
    using System.Xml.Serialization;
    using System.Web.Caching;
    using System.ComponentModel;
    using System.Text;
    using System.Net;
    using System.Web.Services;
    using System.Threading;
    using System.Security.Permissions;
    using System.Web.Services.Diagnostics;

    [PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
    [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
    public abstract class ServerProtocol {
        Type type;
        HttpRequest request;
        HttpResponse response;
        HttpContext context;
        object target;
        WebMethodAttribute methodAttr;

        private static Object s_InternalSyncObject;
        internal static Object InternalSyncObject {
            get {
                if (s_InternalSyncObject == null) {
                    Object o = new Object();
                    Interlocked.CompareExchange(ref s_InternalSyncObject, o, null);
                }
                return s_InternalSyncObject;
            }
        }

        internal void SetContext(Type type, HttpContext context, HttpRequest request, HttpResponse response) {
            PartialTrustHelpers.FailIfInPartialTrustOutsideAspNet(); 
            this.type = type;
            this.context = context;
            this.request = request;
            this.response = response;
            Initialize();
        }

        internal virtual void CreateServerInstance() {
            target = Activator.CreateInstance(ServerType.Type);
            WebService service = target as WebService;
            if (service != null)
                service.SetContext(context);
        }

        internal virtual void DisposeServerInstance() {
            if (target == null) return;
            IDisposable disposable = target as IDisposable;
            if (disposable != null)
                disposable.Dispose();
            target = null;
        }

        protected internal HttpContext Context {
            get { return context; }
        }

        protected internal HttpRequest Request {
            get { return request; }
        }

        protected internal HttpResponse Response {
            get { return response; }
        }

        internal Type Type {
            get { return type; }
        }

        protected virtual internal object Target {
            get { return target; }
        }

        internal virtual bool WriteException(Exception e, Stream outputStream) {
            // return true if exception should not be re-thrown to ASP.NET
            return false;
        }

        internal abstract bool Initialize();
        internal abstract object[] ReadParameters();
        internal abstract void WriteReturns(object[] returns, Stream outputStream);
        internal abstract LogicalMethodInfo MethodInfo { get; }
        internal abstract ServerType ServerType { get; }
        internal abstract bool IsOneWay { get; }
        internal virtual Exception OnewayInitException { get { return null; } }

        internal WebMethodAttribute MethodAttribute {
            get {
                if (methodAttr == null)
                    methodAttr = MethodInfo.MethodAttribute;
                return methodAttr;
            }
        }

        internal string GenerateFaultString(Exception e) {
            return GenerateFaultString(e, false);
        }

        internal static void SetHttpResponseStatusCode(HttpResponse httpResponse, int statusCode) {
            // We skip IIS custom errors for HTTP requests.
            httpResponse.TrySkipIisCustomErrors = true;
            httpResponse.StatusCode = statusCode;
        }

        // 

        internal string GenerateFaultString(Exception e, bool htmlEscapeMessage) {
            bool isDevelopmentServer = Context != null && !Context.IsCustomErrorEnabled;
            if (isDevelopmentServer && !htmlEscapeMessage) {
                //If the user has specified it's a development server (versus a production server) in ASP.NET config,
                //then we should just return e.ToString instead of extracting the list of messages.            
                return e.ToString();
            }
            StringBuilder builder = new StringBuilder();
            if (isDevelopmentServer) {
                //  we are dumping the ecseption directly to IE, need to encode
                GenerateFaultString(e, builder);
            }
            else {
                for (Exception inner = e; inner != null; inner = inner.InnerException) {
                    string text = htmlEscapeMessage ? HttpUtility.HtmlEncode(inner.Message) : inner.Message;
                    if (text.Length == 0) text = e.GetType().Name;
                    builder.Append(text);
                    if (inner.InnerException != null) builder.Append(" ---> ");
                }
            }
            return builder.ToString();
        }

        static void GenerateFaultString(Exception e, StringBuilder builder) {
            builder.Append(e.GetType().FullName);
            if (e.Message != null && e.Message.Length > 0) {
                builder.Append(": ");
                builder.Append(HttpUtility.HtmlEncode(e.Message));
            }
            if (e.InnerException != null) {
                builder.Append(" ---> ");
                GenerateFaultString(e.InnerException, builder);
                builder.Append(Environment.NewLine);
                builder.Append("   ");
                builder.Append(Res.GetString(Res.StackTraceEnd));
            }
            if (e.StackTrace != null) {
                builder.Append(Environment.NewLine);
                builder.Append(e.StackTrace);
            }
        }

        internal void WriteOneWayResponse() {
            context.Response.ContentType = null;
            Response.StatusCode = (int)HttpStatusCode.Accepted;
        }

        delegate string CreateCustomKeyForAspNetWebServiceMetadataCache(Type protocolType, Type serverType, string originalKey);

        static string DefaultCreateCustomKeyForAspNetWebServiceMetadataCache(Type protocolType, Type serverType, string originalKey) {
            return originalKey;
        }

        static CreateCustomKeyForAspNetWebServiceMetadataCache GetCreateCustomKeyForAspNetWebServiceMetadataCacheDelegate(Type serverType) {
            PartialTrustHelpers.FailIfInPartialTrustOutsideAspNet();
            string key = "CreateCustomKeyForAspNetWebServiceMetadataCache-" + serverType.FullName;
            CreateCustomKeyForAspNetWebServiceMetadataCache result = (CreateCustomKeyForAspNetWebServiceMetadataCache)HttpRuntime.Cache.Get(key);
            if (result == null) {
                MethodInfo createKeyMethod = serverType.GetMethod(
                    "CreateCustomKeyForAspNetWebServiceMetadataCache",
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.ExactBinding | BindingFlags.FlattenHierarchy,
                    null,
                    new Type[] { typeof(Type), typeof(Type), typeof(string) },
                    null);

                if (createKeyMethod == null) {
                    result = ServerProtocol.DefaultCreateCustomKeyForAspNetWebServiceMetadataCache;

                } else {
                    result = delegate(Type pt, Type st, string originalString)
                    {
                        return (string)createKeyMethod.Invoke(null, new object[] { pt, st, originalString });
                    };
                }

                HttpRuntime.Cache.Add(key, result, null, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, null);
            }

            return result;
        }

        string CreateKey(Type protocolType, Type serverType, bool excludeSchemeHostPort = false, string keySuffix = null) {
            //
            // we want to use the hostname to cache since for documentation, WSDL
            // contains the cache hostname, but we definitely don't want to cache the query string!
            //            
            string protocolTypeName = protocolType.FullName;
            string serverTypeName = serverType.FullName;
            string typeHandleString = serverType.TypeHandle.Value.ToString();
            string url = excludeSchemeHostPort ? Request.Url.AbsolutePath : Request.Url.GetLeftPart(UriPartial.Path);
            int length = protocolTypeName.Length + url.Length + serverTypeName.Length + typeHandleString.Length;
            StringBuilder sb = new StringBuilder(length);
            sb.Append(protocolTypeName);
            sb.Append(url);
            sb.Append(serverTypeName);
            sb.Append(typeHandleString);
            if (keySuffix != null) {
                sb.Append(keySuffix);
            }
            
            CreateCustomKeyForAspNetWebServiceMetadataCache createKey = ServerProtocol.GetCreateCustomKeyForAspNetWebServiceMetadataCacheDelegate(serverType);

            return createKey(protocolType, serverType, sb.ToString());
        }

        protected void AddToCache(Type protocolType, Type serverType, object value) {
            this.AddToCache(protocolType, serverType, value, false);
        }

        // See comment on the ServerProtocol.IsCacheUnderPressure method for explanation of the excludeSchemeHostPort logic.
        internal void AddToCache(Type protocolType, Type serverType, object value, bool excludeSchemeHostPort) {
            PartialTrustHelpers.FailIfInPartialTrustOutsideAspNet();
            HttpRuntime.Cache.Insert(CreateKey(protocolType, serverType, excludeSchemeHostPort),
                value,
                null,
                Cache.NoAbsoluteExpiration,
                Cache.NoSlidingExpiration,
                CacheItemPriority.NotRemovable,
                null);
        }

        protected object GetFromCache(Type protocolType, Type serverType) {
            return this.GetFromCache(protocolType, serverType, false);
        }

        internal object GetFromCache(Type protocolType, Type serverType, bool excludeSchemeHostPort) {
            PartialTrustHelpers.FailIfInPartialTrustOutsideAspNet();
            return HttpRuntime.Cache.Get(CreateKey(protocolType, serverType, excludeSchemeHostPort));
        }

        // IsCacheUnderPressure is part of a DOS mitigation mechanism addressing CSDMain#195148. Original problem: when a large number of 
        // HTTP requests for WSDL or the ASMX documentation page is made, each request with a unique value of the HOST header, a unique response
        // is generated and cached for each of the requests (responses contain the scheme/host/port of the request). 
        // This leads to ever growing memory consumption and eventual crash of the process.
        // The mitigation for this DOS attack uses the following mechanism:
        // 1. The behavior of the system remains unchanged for the first 10 requests for WSDL of a given ASMX service that have differing 
        //    scheme/host/port combination of the request URI. This is to avoid breaking behavioral changes in the 99.99% case, 
        //    since the DOS attack cannot be generically fixed without breaking behavioral changes. The value of 10 is baked in, 
        //    and we consider it a reasonable default based on the assumption that ASMX services in most circumstances cannot be 
        //    reached using more than 10 different values of the scheme/host/port. 
        // 2. For any requests for WSDL going beyond the 10 limit of scheme/host/port combination, we go into a “DOS mitigation mode”. 
        //    The mode prevents the eventual process crash while introducing marginal breaking behavioral changes:
        //    a. We create a single service description and cache it using the AbsolutePath of the request URI alone 
        //       (as opposed to scheme/host/port + AbsolutePath).
        //    b. For every request for WSDL/disco/documentation document, we fix up the URLs in the returned document to match the 
        //       scheme/host/port of the actual request for WSDL/disco. This fixup only applies to the WSDL extensions we have shipped in .NET 
        //       and does not apply to custom extensions implemented externally, hence the breaking behavioral change. 
        // This mechamism affects the DiscoveryServerProtocol and DocumentationServerProtocol.
        internal bool IsCacheUnderPressure(Type protocolType, Type serverType) {
            PartialTrustHelpers.FailIfInPartialTrustOutsideAspNet();

            const int threshold = 10;
            string key = this.CreateKey(protocolType, serverType, true, "CachePressure");
            ServerProtocolCachePressure item = (ServerProtocolCachePressure)HttpRuntime.Cache.Get(key);

            // There is a potential race condition in creating a new entry or increasing the value of an existing entry, 
            // but it is acceptable since DOS threshold enforcement need not be exact.

            if (item != null) {
                return item.Pressure < threshold ? Interlocked.Increment(ref item.Pressure) >= threshold : false;
            }
            else {
                HttpRuntime.Cache.Insert(
                    key,
                    new ServerProtocolCachePressure { Pressure = 1 },
                    null,
                    Cache.NoAbsoluteExpiration,
                    Cache.NoSlidingExpiration,
                    CacheItemPriority.NotRemovable,
                    null);

                return false;
            }
        }

        class ServerProtocolCachePressure {
            public int Pressure;
        }
    }

    [PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
    [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
    public abstract class ServerProtocolFactory {
        internal ServerProtocol Create(Type type, HttpContext context, HttpRequest request, HttpResponse response, out bool abortProcessing) {
            ServerProtocol serverProtocol = null;
            abortProcessing = false;
            serverProtocol = CreateIfRequestCompatible(request);
            try {
                if (serverProtocol != null)
                    serverProtocol.SetContext(type, context, request, response);
                return serverProtocol;
            }
            catch (Exception e) {
                abortProcessing = true;
                if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException) {
                    throw;
                }
                if (Tracing.On) Tracing.ExceptionCatch(TraceEventType.Warning, this, "Create", e);
                if (serverProtocol != null) {
                    // give the protocol a shot at handling the error in a custom way
                    if (!serverProtocol.WriteException(e, serverProtocol.Response.OutputStream))
                        throw new InvalidOperationException(Res.GetString(Res.UnableToHandleRequest0), e);
                }
                return null;
            }

        }

        protected abstract ServerProtocol CreateIfRequestCompatible(HttpRequest request);

    }

}
