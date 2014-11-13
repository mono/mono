//------------------------------------------------------------------------------
// <copyright file="DocumentationServerProtocol.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Protocols {
    using System;
    using System.Collections;
    using System.IO;
    using System.Reflection;
    using System.Web.Services.Discovery;
    using System.Web.UI;
    using System.Diagnostics;
    using System.Web.Services.Configuration;
    using System.Xml.Serialization;
    using System.Xml.Schema;
    using System.Text;
    using System.Net;
    using System.Web.Services.Description;
    using System.Threading;
    using System.Web.Services.Diagnostics;
    using System.Security.Permissions;
    using System.Collections.Generic;

    internal class DocumentationServerType : ServerType {
        ServiceDescriptionCollection serviceDescriptions, serviceDescriptionsWithPost;
        XmlSchemas schemas, schemasWithPost;
        LogicalMethodInfo methodInfo;

        public List<Action<Uri>> UriFixups { get; private set; }

        void AddUriFixup(Action<Uri> fixup)
        {
            if (this.UriFixups != null)
            {
                this.UriFixups.Add(fixup);
            }
        }

        // See comment on the ServerProtocol.IsCacheUnderPressure method for explanation of the excludeSchemeHostPortFromCachingKey logic.
        internal DocumentationServerType(Type type, string uri, bool excludeSchemeHostPortFromCachingKey)
            : base(typeof(DocumentationServerProtocol))
        {
            if (excludeSchemeHostPortFromCachingKey)
            {
                this.UriFixups = new List<Action<Uri>>();
            }                   
            //
            // parse the uri from a string into a URI object
            //
            Uri uriObject = new Uri(uri, true);
            //
            // and get rid of the query string if there's one
            //
            uri = uriObject.GetLeftPart(UriPartial.Path);
            methodInfo = new LogicalMethodInfo(typeof(DocumentationServerProtocol).GetMethod("Documentation", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic));
            ServiceDescriptionReflector reflector = new ServiceDescriptionReflector(this.UriFixups);
            reflector.Reflect(type, uri);
            schemas = reflector.Schemas;
            serviceDescriptions = reflector.ServiceDescriptions;
            schemasWithPost = reflector.SchemasWithPost;
            serviceDescriptionsWithPost = reflector.ServiceDescriptionsWithPost;
        }
                   
        internal LogicalMethodInfo MethodInfo {
            get { return methodInfo; }
        }

        internal XmlSchemas Schemas {
            get { return schemas; }
        }

        internal ServiceDescriptionCollection ServiceDescriptions {
            get { return serviceDescriptions; }
        }
        
        internal ServiceDescriptionCollection ServiceDescriptionsWithPost {
            get { return serviceDescriptionsWithPost; }
        }
        
        internal XmlSchemas SchemasWithPost {
            get { return schemasWithPost; }
        }
    }

    internal class DocumentationServerProtocolFactory : ServerProtocolFactory {
        protected override ServerProtocol CreateIfRequestCompatible(HttpRequest request) {
            if (request.PathInfo.Length > 0)
                return null;

            if (request.HttpMethod != "GET")
                // MethodNotAllowed = 405,
                return new UnsupportedRequestProtocol(405);

            return new DocumentationServerProtocol();
        }
    }

    internal sealed class DocumentationServerProtocol : ServerProtocol {
        DocumentationServerType serverType;
        IHttpHandler handler = null;
        object syncRoot = new object();

        private const int MAX_PATH_SIZE = 1024;

        internal override bool Initialize() {
            //
            // see if we already cached a DocumentationServerType
            //
            if (null == (serverType = (DocumentationServerType)GetFromCache(typeof(DocumentationServerProtocol), Type))
                && null == (serverType = (DocumentationServerType)GetFromCache(typeof(DocumentationServerProtocol), Type, true))) {
                lock (InternalSyncObject) {
                    if (null == (serverType = (DocumentationServerType)GetFromCache(typeof(DocumentationServerProtocol), Type))
                        && null == (serverType = (DocumentationServerType)GetFromCache(typeof(DocumentationServerProtocol), Type, true)))
                    {
                        //
                        // if not create a new DocumentationServerType and cache it
                        //
                        // 
                        bool excludeSchemeHostPortFromCachingKey = this.IsCacheUnderPressure(typeof(DocumentationServerProtocol), Type);
                        string escapedUri = Uri.EscapeUriString(Request.Url.ToString()).Replace("#", "%23");
                        serverType = new DocumentationServerType(Type, escapedUri, excludeSchemeHostPortFromCachingKey);
                        AddToCache(typeof(DocumentationServerProtocol), Type, serverType, excludeSchemeHostPortFromCachingKey);
                    }
                }
            }

            WebServicesSection config = WebServicesSection.Current;
            if (config.WsdlHelpGenerator.Href != null && config.WsdlHelpGenerator.Href.Length > 0)
            {
                TraceMethod caller = Tracing.On ? new TraceMethod(this, "Initialize") : null;
                if (Tracing.On) Tracing.Enter("ASP.NET", caller, new TraceMethod(typeof(PageParser), "GetCompiledPageInstance", config.WsdlHelpGenerator.HelpGeneratorVirtualPath, config.WsdlHelpGenerator.HelpGeneratorPath, Context));

                handler = GetCompiledPageInstance(config.WsdlHelpGenerator.HelpGeneratorVirtualPath,
                                                    config.WsdlHelpGenerator.HelpGeneratorPath,
                                                    Context);
                if (Tracing.On) Tracing.Exit("ASP.NET", caller);
            }
            return true;                       
        }

        // Asserts SecurityPermission and FileIOPermission.
        // Justification: Security Permission is demanded by PageParser.GetCompiledPageInstance() method. 
        // It is used to initialize the IHttpHandler field of the DocumentationServerProtocol object.
        // FileIOPermission is required to access the inputFile passed in as a parameter.
        // It is used only to map the virtual path to the physical file path. The FileIOPermission is not used to access any file other than the one passed in.
        [SecurityPermission(SecurityAction.Assert, Unrestricted = true)]
        [FileIOPermissionAttribute(SecurityAction.Assert, Unrestricted = true)]
        private IHttpHandler GetCompiledPageInstance(string virtualPath, string inputFile, HttpContext context)
        {
            return PageParser.GetCompiledPageInstance(virtualPath, inputFile, context);
        }

        internal override ServerType ServerType {
            get { return serverType; }
        }

        internal override bool IsOneWay {
            get { return false; }            
        }            

        internal override LogicalMethodInfo MethodInfo {
            get { return serverType.MethodInfo; }
        }
        
        internal override object[] ReadParameters() {
            return new object[0];
        }

        internal override void WriteReturns(object[] returnValues, Stream outputStream) {
            try {
                if (handler != null) {
                    Context.Items.Add("wsdls", serverType.ServiceDescriptions);
                    Context.Items.Add("schemas", serverType.Schemas);

                    // conditionally add post-enabled wsdls and schemas to support localhost-only post
                    if (Context.Request.Url.IsLoopback || Context.Request.IsLocal) {
                        Context.Items.Add("wsdlsWithPost", serverType.ServiceDescriptionsWithPost);
                        Context.Items.Add("schemasWithPost", serverType.SchemasWithPost);
                    }
                    Context.Items.Add("conformanceWarnings", WebServicesSection.Current.EnabledConformanceWarnings);
                    Response.ContentType = "text/html";
                    if (this.serverType.UriFixups == null)
                    {
                        handler.ProcessRequest(Context);
                    }
                    else
                    {
                        lock (this.syncRoot)
                        {
                            this.RunUriFixups();
                            handler.ProcessRequest(Context);
                        }
                    }
                }
            }
            catch (Exception e) {
                if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException) {
                    throw;
                }
                throw new InvalidOperationException(Res.GetString(Res.HelpGeneratorInternalError), e);
            }
        }

        internal override bool WriteException(Exception e, Stream outputStream) {
            return false;
        }

        internal void Documentation() {
            // This is the "server method" that is called for this protocol
        }

        void RunUriFixups()
        {
            foreach (Action<Uri> fixup in this.serverType.UriFixups)
            {
                fixup(this.Context.Request.Url);
            }
        }
    }
}
