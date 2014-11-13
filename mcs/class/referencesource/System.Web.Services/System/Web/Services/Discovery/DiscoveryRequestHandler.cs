//------------------------------------------------------------------------------
// <copyright file="DiscoveryRequestHandler.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Services.Discovery {
    using System;
    using System.IO;
    using System.Collections;
    using System.Web;
    using System.Xml;
    using System.Diagnostics;
    using System.ComponentModel;
    using System.Text;
    using System.Web.Services.Protocols;
    using System.Security;
    using System.Security.Permissions;
    using System.Web.Services.Diagnostics;

    /// <include file='doc\DiscoveryRequestHandler.uex' path='docs/doc[@for="DiscoveryRequestHandler"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public sealed class DiscoveryRequestHandler : IHttpHandler {

        /// <include file='doc\DiscoveryRequestHandler.uex' path='docs/doc[@for="DiscoveryRequestHandler.IsReusable"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public bool IsReusable {
            get { return true; }
        }

        /// <include file='doc\DiscoveryRequestHandler.uex' path='docs/doc[@for="DiscoveryRequestHandler.ProcessRequest"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void ProcessRequest(HttpContext context) {
            TraceMethod method = Tracing.On ? new TraceMethod(this, "ProcessRequest") : null;
            if (Tracing.On) Tracing.Enter("IHttpHandler.ProcessRequest", method, Tracing.Details(context.Request));

            new PermissionSet(PermissionState.Unrestricted).Demand();
            // string cacheKey;

            string physicalPath = context.Request.PhysicalPath;
            if ( CompModSwitches.DynamicDiscoverySearcher.TraceVerbose ) Debug.WriteLine("DiscoveryRequestHandle: handling " + physicalPath);

            // Check to see if file exists locally.
            if (File.Exists(physicalPath)) {
                DynamicDiscoveryDocument dynDisco = null;
                FileStream stream = null;
                try {
                    stream = new FileStream(physicalPath, FileMode.Open, FileAccess.Read);
                    XmlTextReader xmlReader = new XmlTextReader(stream);
                    xmlReader.WhitespaceHandling = WhitespaceHandling.Significant;
                    xmlReader.XmlResolver = null;
                    xmlReader.DtdProcessing = DtdProcessing.Prohibit;
                    if (xmlReader.IsStartElement("dynamicDiscovery", DynamicDiscoveryDocument.Namespace)) {
                        stream.Position = 0;
                        dynDisco = DynamicDiscoveryDocument.Load(stream);
                    }
                }
                finally {
                    if (stream != null) {
                        stream.Close();
                    }
                }

                if (dynDisco != null) {
                    string[] excludeList = new string[dynDisco.ExcludePaths.Length];
                    string discoFileDirectory = Path.GetDirectoryName(physicalPath);
                    string discoFileName = Path.GetFileName(physicalPath);

                    for (int i = 0; i < excludeList.Length; i++) {
                         // Exclude list now consists of relative paths, so this transformation not needed.
                         // excludeList[i] = Path.Combine(discoFileDirectory, dynDisco.ExcludePaths[i].Path);
                         excludeList[i] = dynDisco.ExcludePaths[i].Path;
                         }

                    // Determine start url path for search
                    DynamicDiscoSearcher searcher;
                    Uri searchStartUrl = context.Request.Url;
                    string escapedUri = Uri.EscapeUriString(searchStartUrl.ToString()).Replace("#", "%23");
                    string searchStartUrlDir = GetDirPartOfPath( escapedUri );  // URL path without file name
                    string strLocalPath = GetDirPartOfPath(searchStartUrl.LocalPath);

                    if ( strLocalPath.Length == 0 ||       // no subdir present, host only
                         CompModSwitches.DynamicDiscoveryVirtualSearch.Enabled    // virtual search forced (for test suites).
                       ) {
                       discoFileName = GetFilePartOfPath( escapedUri );
                       searcher = new DynamicVirtualDiscoSearcher( discoFileDirectory, excludeList, searchStartUrlDir);
                    }
                    else
                        searcher = new DynamicPhysicalDiscoSearcher(discoFileDirectory, excludeList, searchStartUrlDir);

                    if ( CompModSwitches.DynamicDiscoverySearcher.TraceVerbose ) Debug.WriteLine( "*** DiscoveryRequestHandler.ProcessRequest() - startDir: " + searchStartUrlDir + " discoFileName :" + discoFileName);
                    searcher.Search(discoFileName);

                    DiscoveryDocument discoFile = searcher.DiscoveryDocument;

                    MemoryStream memStream = new MemoryStream(1024);
                    StreamWriter writer = new StreamWriter(memStream, new UTF8Encoding(false));
                    discoFile.Write(writer);
                    memStream.Position = 0;
                    byte[] data = new byte[(int)memStream.Length];
                    int bytesRead = memStream.Read(data, 0, data.Length);
                    context.Response.ContentType = ContentType.Compose("text/xml", Encoding.UTF8);
                    context.Response.OutputStream.Write(data, 0, bytesRead);
                }
                else {
                    // Else, just return the disco file
                    context.Response.ContentType = "text/xml";
                    context.Response.WriteFile(physicalPath);
                }
                if (Tracing.On) Tracing.Exit("IHttpHandler.ProcessRequest", method);
                return;
            }
            if (Tracing.On) Tracing.Exit("IHttpHandler.ProcessRequest", method);

            // Else, file is not found
            throw new HttpException(404, Res.GetString(Res.WebPathNotFound, context.Request.Path));
        }

        // -------------------------------------------------------------------------
        // Returns part of URL string to the left of the last slash.
        private static string GetDirPartOfPath(string str) {
            int lastSlash = str.LastIndexOf('/');
            return (lastSlash > 0) ? str.Substring(0, lastSlash) : "";
        }

        // -------------------------------------------------------------------------
        // Returns part of URL string to the right of the last slash.
        private static string GetFilePartOfPath(string str) {
            int lastSlash = str.LastIndexOf('/');
            if ( lastSlash < 0 )
                return str;
            else if ( lastSlash == str.Length - 1 )
                return "";
            return str.Substring(lastSlash + 1);
        }

    }
}

