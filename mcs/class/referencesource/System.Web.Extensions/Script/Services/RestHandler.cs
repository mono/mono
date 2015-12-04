//------------------------------------------------------------------------------
// <copyright file="RestHandler.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
 
namespace System.Web.Script.Services {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Reflection;
    using System.Security;
    using System.Text;
    using System.Web;
    using System.Web.Resources;
    using System.Web.Script.Serialization;
    using System.Web.SessionState;

    internal class RestHandler : IHttpHandler {
        private WebServiceMethodData _webServiceMethodData;

        internal static IHttpHandler CreateHandler(HttpContext context) {
            // Expectation is that we got a PathInfo of form /MethodName
            if (context.Request.PathInfo.Length < 2 || context.Request.PathInfo[0] != '/') {
                throw new InvalidOperationException(AtlasWeb.WebService_InvalidWebServiceCall);
            }

            // Get the data about the web service being invoked
            WebServiceData webServiceData = WebServiceData.GetWebServiceData(context, context.Request.FilePath);
            string methodName = context.Request.PathInfo.Substring(1);
            return CreateHandler(webServiceData, methodName);
        }

        private static IHttpHandler CreateHandler(WebServiceData webServiceData, string methodName) {

            // Get the data about the method being called
            WebServiceMethodData methodData = webServiceData.GetMethodData(methodName);

            // Create the proper handler, depending on whether we need session state
            RestHandler handler;
            if (methodData.RequiresSession)
                handler = new RestHandlerWithSession();
            else
                handler = new RestHandler();

            // Save the method data in the handler
            handler._webServiceMethodData = methodData;
            return handler;
        }

        // This is very similar to WebService caching, the differences are
        // 1) Here we explicitely SetValidUntilExpires(true) because in an XmlHttp there is
        //    "pragma:no-cache" in header which would result in cache miss on the server.
        // 2) Here we don't vary on header "Content-type" or "SOAPAction" because the former
        //    is specific to soap 1.2, which puts action in the content-type param; and the
        //    later is used by soap calls.
        private static void InitializeCachePolicy(WebServiceMethodData methodData, HttpContext context) {
            int cacheDuration = methodData.CacheDuration;
            if (cacheDuration > 0) {
                context.Response.Cache.SetCacheability(HttpCacheability.Server);
                context.Response.Cache.SetExpires(DateTime.Now.AddSeconds(cacheDuration));
                context.Response.Cache.SetSlidingExpiration(false);
                context.Response.Cache.SetValidUntilExpires(true);

                // DevDiv 23596: Don't set VaryBy* if the method takes no parameters
                if (methodData.ParameterDatas.Count > 0) {
                    context.Response.Cache.VaryByParams["*"] = true;
                }
                else {
                    context.Response.Cache.VaryByParams.IgnoreParams = true;
                }
            }
            else {
                context.Response.Cache.SetNoServerCaching();
                context.Response.Cache.SetMaxAge(TimeSpan.Zero);
            }
        }

        private static IDictionary<string, object> GetRawParamsFromGetRequest(HttpContext context, JavaScriptSerializer serializer, WebServiceMethodData methodData) {
            // Get all the parameters from the query string
            NameValueCollection queryString = context.Request.QueryString;
            Dictionary<string, object> rawParams = new Dictionary<string, object>();
            foreach (WebServiceParameterData param in methodData.ParameterDatas) {
                string name = param.ParameterInfo.Name;
                string val = queryString[name];
                if (val != null) {
                    rawParams.Add(name, serializer.DeserializeObject(val));
                }
            }
            return rawParams;
        }

        private static IDictionary<string, object> GetRawParamsFromPostRequest(HttpContext context, JavaScriptSerializer serializer) {
            // Read the entire body as a string
            TextReader reader = new StreamReader(context.Request.InputStream);
            string bodyString = reader.ReadToEnd();

            // If there is no body, treat it as an empty object
            if (String.IsNullOrEmpty(bodyString)) {
                return new Dictionary<string, object>();
            }

            // Deserialize the javascript request body
            return serializer.Deserialize<IDictionary<string, object>>(bodyString);
        }

        private static IDictionary<string, object> GetRawParams(WebServiceMethodData methodData, HttpContext context) {
            if (methodData.UseGet) {
                if (context.Request.HttpMethod == "GET") {
                    return GetRawParamsFromGetRequest(context, methodData.Owner.Serializer, methodData);
                }
                else {
                    throw new InvalidOperationException(
                        String.Format(CultureInfo.CurrentCulture, AtlasWeb.WebService_InvalidVerbRequest,
                            methodData.MethodName, "POST"));
                }
            }
            else if (context.Request.HttpMethod == "POST") {
                return GetRawParamsFromPostRequest(context, methodData.Owner.Serializer);
            } else {
                throw new InvalidOperationException(
                    String.Format(CultureInfo.CurrentCulture, AtlasWeb.WebService_InvalidVerbRequest,
                        methodData.MethodName, "GET"));
            }
        }

        private static void InvokeMethod(HttpContext context, WebServiceMethodData methodData, IDictionary<string, object> rawParams) {
            // Initialize HttpCachePolicy
            InitializeCachePolicy(methodData, context);

            // Create an new instance of the class
            object target = null;
            if (!methodData.IsStatic) target = Activator.CreateInstance(methodData.Owner.TypeData.Type);

            // Make the actual method call on it
            object retVal = methodData.CallMethodFromRawParams(target, rawParams);

            string contentType;
            string responseString = null;
            if (methodData.UseXmlResponse) {
                responseString = retVal as string;

                // If it's a string, output it as is unless XmlSerializeString is set
                if (responseString == null || methodData.XmlSerializeString) {
                    // Use the Xml Serializer
                    try {
                        responseString = ServicesUtilities.XmlSerializeObjectToString(retVal);
                    }
                    catch (Exception e) {
                        // Throw a better error if Xml serialization fails
                        throw new InvalidOperationException(
                            String.Format(CultureInfo.CurrentCulture, AtlasWeb.WebService_InvalidXmlReturnType,
                                methodData.MethodName, retVal.GetType().FullName, e.Message));
                    }
                }

                contentType = "text/xml";
            }
            else {

                // Convert the result to a JSON string
                // DevDiv 88409:Change JSON wire format to prevent CSRF attack 
                // We wrap the returned value inside an object , and assign the returned value
                // to member "d" of the object. We do so as JSOM for object will never be parsed
                // as valid Javascript , unlike arrays.
                responseString =@"{""d"":" + methodData.Owner.Serializer.Serialize(retVal) + "}";
                contentType = "application/json";
            }

            // Set the response content-type
            context.Response.ContentType = contentType;

            // Write the string to the response
            if (responseString != null)
                context.Response.Write(responseString);
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
            Justification="All exceptions need to be reported to the client")]
        [SuppressMessage("Microsoft.Security", "CA2107:ReviewDenyAndPermitOnlyUsage",
           Justification = "Fix for DevDiv 39162 GAC'd non-APTCA types can instantiate in networking stack in Medium trust")]
        internal static void ExecuteWebServiceCall(HttpContext context, WebServiceMethodData methodData) {
            try {
                NamedPermissionSet s_permissionSet = HttpRuntime.NamedPermissionSet;
                if (s_permissionSet != null) {
                    s_permissionSet.PermitOnly();
                }

                // Deserialize the javascript request body
                IDictionary<string, object> rawParams = GetRawParams(methodData, context);
                InvokeMethod(context, methodData, rawParams);
            }
            catch (Exception ex) {
                WriteExceptionJsonString(context, ex);
            }
        }

        private static object BuildWebServiceError(string msg, string stack, string type) {
            var result = new OrderedDictionary();
            result["Message"] = msg;
            result["StackTrace"] = stack;
            result["ExceptionType"] = type;
            return result;
        }

        internal static void WriteExceptionJsonString(HttpContext context, Exception ex) {
            WriteExceptionJsonString(context, ex, (int)HttpStatusCode.InternalServerError);
        }

        internal static void WriteExceptionJsonString(HttpContext context, Exception ex, int statusCode) {
            // Record the charset before we call ClearHeaders(). (DevDiv Bugs 158401)
            string charset = context.Response.Charset;
            context.Response.ClearHeaders();
            context.Response.ClearContent();
            context.Response.Clear();
            context.Response.StatusCode = statusCode;
            context.Response.StatusDescription = HttpWorkerRequest.GetStatusDescription(statusCode);
            context.Response.ContentType = "application/json";
            context.Response.AddHeader("jsonerror", "true");
            // Maintain the Charset from before. (DevDiv Bugs 158401)
            context.Response.Charset = charset;
            //Devdiv Bug: 118619:When accessed remotely, an Ajax web service that throws an error doesn't return the error string in the proper format on IIS7
            //For IIS 7.0 integrated mode we need to set TrySkipIisCustomErrors to override IIS custom error handling. This has no functional/perf impact on
            //IIS 7.0 classic mode or earlier versions.
            context.Response.TrySkipIisCustomErrors = true;
            using (StreamWriter writer = new StreamWriter(context.Response.OutputStream, new UTF8Encoding(false))) {
                if (ex is TargetInvocationException) {
                    ex = ex.InnerException;
                }

                // Don't show any error stack or sensitive info when custom error is enabled.
                if (context.IsCustomErrorEnabled) {
                    writer.Write(JavaScriptSerializer.SerializeInternal(BuildWebServiceError(AtlasWeb.WebService_Error, String.Empty, String.Empty)));
                }
                else {
                    writer.Write(JavaScriptSerializer.SerializeInternal(BuildWebServiceError(ex.Message, ex.StackTrace, ex.GetType().FullName)));
                }
                writer.Flush();
            }
        }

        public void ProcessRequest(HttpContext context) {
            ExecuteWebServiceCall(context, _webServiceMethodData);
        }

        public bool IsReusable {
            get {
                return false;
            }
        }
    }

    // Same handler, but implementing IRequiresSessionState to allow session state use
    internal class RestHandlerWithSession: RestHandler, IRequiresSessionState {
    }
}
