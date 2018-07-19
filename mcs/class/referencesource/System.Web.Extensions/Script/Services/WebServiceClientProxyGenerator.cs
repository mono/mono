//------------------------------------------------------------------------------
// <copyright file="WebServiceClientProxyGenerator.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
 
namespace System.Web.Script.Services {
    using System;
    using System.IO;
    using System.Reflection;
    using System.Web;
    using System.Security.Permissions;
    using System.Security;

    internal class WebServiceClientProxyGenerator : ClientProxyGenerator {
        private string _path;

        // Called by ScriptManager to generate the proxy inline
        internal static string GetInlineClientProxyScript(string path, HttpContext context, bool debug) {
            WebServiceData webServiceData = WebServiceData.GetWebServiceData(context, path, true, false, true);
            WebServiceClientProxyGenerator proxyGenerator = new WebServiceClientProxyGenerator(path, debug);
            return proxyGenerator.GetClientProxyScript(webServiceData);
        }

        private static DateTime GetAssemblyModifiedTime(Assembly assembly) {
            AssemblyName assemblyName = assembly.GetName();
            DateTime writeTime = File.GetLastWriteTime(new Uri(assemblyName.CodeBase).LocalPath);
            // DevDiv 52056: include writeTime.Second in the date, otherwise if you modify it within the same minute it we'd still respond with http status 304 not modified
            return new DateTime(writeTime.Year, writeTime.Month, writeTime.Day, writeTime.Hour, writeTime.Minute, writeTime.Second);
        }

        // This is called thru the RestClientProxyHandler
        [SecuritySafeCritical]
        [FileIOPermission(SecurityAction.Assert, Unrestricted = true)]
        internal static string GetClientProxyScript(HttpContext context) {
            WebServiceData webServiceData = WebServiceData.GetWebServiceData(context, context.Request.FilePath);
            DateTime lastModifiedDate = GetAssemblyModifiedTime(webServiceData.TypeData.Type.Assembly);

            // If the browser sent this header, we can check if we need to resend
            string modifiedSince = context.Request.Headers["If-Modified-Since"];
            if (modifiedSince != null) {
                DateTime header;
                if (DateTime.TryParse(modifiedSince, out header)) {
                    // We are done if the assembly hasn't been modified
                    if (header >= lastModifiedDate) {
                        context.Response.StatusCode = 304;
                        return null;
                    }
                }
            }
            bool debug = RestHandlerFactory.IsClientProxyDebugRequest(context.Request.PathInfo);
            // Only cache for release proxy script (/js)
            if (!debug) {
                // Only cache if we get a reasonable last modified date
                if (lastModifiedDate.ToUniversalTime() < DateTime.UtcNow) {
                    // Cache the resource so we don't keep processing the same requests
                    HttpCachePolicy cachePolicy = context.Response.Cache;
                    cachePolicy.SetCacheability(HttpCacheability.Public);
                    cachePolicy.SetLastModified(lastModifiedDate);
                    // expires is necessary so that the browser at least does an If-Modified-Since request on every request.
                    // without that, the browser wouldn't request a new proxy until the user hits refresh.
                    // Use one year ago to reasonably ensure "past" interpretation
                    cachePolicy.SetExpires(lastModifiedDate.AddYears(-1));
                }
            }

            WebServiceClientProxyGenerator proxyGenerator = new WebServiceClientProxyGenerator(context.Request.FilePath, debug);
            return proxyGenerator.GetClientProxyScript(webServiceData);
        }

        internal WebServiceClientProxyGenerator(string path, bool debug) {
            // internal because ExtensionsTest needs this path to bypass httpcontext
            _path = path;
            _debugMode = debug;
        }

        protected override string GetProxyPath() {
            return _path;
        }
    }
}
