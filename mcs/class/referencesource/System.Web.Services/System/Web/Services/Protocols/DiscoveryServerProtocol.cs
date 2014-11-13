//------------------------------------------------------------------------------
// <copyright file="DiscoveryServerProtocol.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Protocols {
    using System;
    using System.Collections;
    using System.IO;
    using System.Reflection;
    using System.Xml.Serialization;
    using System.Xml.Schema;
    using System.Web.Services.Description;
    using System.Web.Services.Discovery;
    using System.Web.UI;
    using System.Text;
    using System.Diagnostics;
    using System.Net;
    using System.Web.Services.Configuration;
    using System.Globalization;
    using System.Collections.Generic;
    
    internal class DiscoveryServerType : ServerType {
        ServiceDescription description;
        LogicalMethodInfo methodInfo;
        Hashtable schemaTable = new Hashtable();
        Hashtable wsdlTable = new Hashtable();
        DiscoveryDocument discoDoc;

        public List<Action<Uri>> UriFixups { get; private set; }

        void AddUriFixup(Action<Uri> fixup)
        {
            if (this.UriFixups != null)
            {
                this.UriFixups.Add(fixup);
            }
        }

        // See comment on the ServerProtocol.IsCacheUnderPressure method for explanation of the excludeSchemeHostPortFromCachingKey logic.
        internal DiscoveryServerType(Type type, string uri, bool excludeSchemeHostPortFromCachingKey)
            : base(typeof(DiscoveryServerProtocol))
        {
            if (excludeSchemeHostPortFromCachingKey)
            {
                this.UriFixups = new List<Action<Uri>>();
            }            
            //
            // parse the uri from a string into a Uri object
            //
            Uri uriObject = new Uri(uri, true);
            //
            // and get rid of the query string if there's one
            //
            uri = uriObject.GetLeftPart(UriPartial.Path);
            methodInfo = new LogicalMethodInfo(typeof(DiscoveryServerProtocol).GetMethod("Discover", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic));
            ServiceDescriptionReflector reflector = new ServiceDescriptionReflector(this.UriFixups);
            reflector.Reflect(type, uri);

            XmlSchemas schemas = reflector.Schemas;
            this.description = reflector.ServiceDescription;
            
            // We need to force initialization of ServiceDescription's XmlSerializer since we
            // won't necessarily have the permissions to do it when we actually need it
            XmlSerializer serializer = ServiceDescription.Serializer;

            // add imports to the external schemas
            AddSchemaImports(schemas, uri, reflector.ServiceDescriptions);

            // add imports to the other service descriptions
            for (int i = 1; i < reflector.ServiceDescriptions.Count; i++) {
                ServiceDescription description = reflector.ServiceDescriptions[i];
                Import import = new Import();
                import.Namespace = description.TargetNamespace;

                // 


                string id = "wsdl" + i.ToString(CultureInfo.InvariantCulture);

                import.Location = uri + "?wsdl=" + id;
                this.AddUriFixup(delegate(Uri current)
                {
                    import.Location = CombineUris(current, import.Location);
                });
                reflector.ServiceDescription.Imports.Add(import);
                wsdlTable.Add(id, description);
            }

            discoDoc = new DiscoveryDocument();
            ContractReference contractReference = new ContractReference(uri + "?wsdl", uri);
            this.AddUriFixup(delegate(Uri current)
            {
                contractReference.Ref = CombineUris(current, contractReference.Ref);
                contractReference.DocRef = CombineUris(current, contractReference.DocRef);
            });
            discoDoc.References.Add(contractReference);

            foreach (Service service in reflector.ServiceDescription.Services) {
                foreach (Port port in service.Ports) {
                    SoapAddressBinding soapAddress = (SoapAddressBinding)port.Extensions.Find(typeof(SoapAddressBinding));
                    if (soapAddress != null) {
                        System.Web.Services.Discovery.SoapBinding binding = new System.Web.Services.Discovery.SoapBinding();
                        binding.Binding = port.Binding;
                        binding.Address = soapAddress.Location;
                        this.AddUriFixup(delegate(Uri current)
                        {
                            binding.Address = CombineUris(current, binding.Address);
                        });
                        discoDoc.References.Add(binding);
                    }
                }
            }
        }

        internal void AddExternal(XmlSchema schema, string ns, string location) {
            if (schema == null)
                return;

            if (schema.TargetNamespace == ns) {
                XmlSchemaInclude include = new XmlSchemaInclude();
                include.SchemaLocation = location;
                this.AddUriFixup(delegate(Uri current)
                {
                    include.SchemaLocation = CombineUris(current, include.SchemaLocation);
                });
                schema.Includes.Add(include);
            }
            else {
                XmlSchemaImport import = new XmlSchemaImport();
                import.SchemaLocation = location;
                this.AddUriFixup(delegate(Uri current)
                {
                    import.SchemaLocation = CombineUris(current, import.SchemaLocation);
                }); 
                import.Namespace = ns;
                schema.Includes.Add(import);
            }
        }

        void AddSchemaImports(XmlSchemas schemas, string uri, ServiceDescriptionCollection descriptions) {
            int id = 0;
            foreach (XmlSchema schema in schemas) {
                if (schema == null)
                    continue;
                // 
                if (schema.Id == null || schema.Id.Length == 0)
                    schema.Id = "schema" + (++id).ToString(CultureInfo.InvariantCulture);

                string location = uri + "?schema=" + schema.Id;
                foreach (ServiceDescription description in descriptions) {
                    if (description.Types.Schemas.Count == 0) {
                        XmlSchema top = new XmlSchema();
                        top.TargetNamespace = description.TargetNamespace;
                        schema.ElementFormDefault = XmlSchemaForm.Qualified;
                        AddExternal(top, schema.TargetNamespace, location);
                        description.Types.Schemas.Add(top);
                    }
                    else {
                        AddExternal(description.Types.Schemas[0], schema.TargetNamespace, location);
                    }
                }
                //schema.SchemaLocation = location;
                schemaTable.Add(schema.Id, schema);
            }
        }

        internal XmlSchema GetSchema(string id) {
            return (XmlSchema)schemaTable[id];
        }


        internal ServiceDescription GetServiceDescription(string id) {
            return (ServiceDescription)wsdlTable[id];
        }

        internal ServiceDescription Description {
            get { return description; }
        }        
                    
        internal LogicalMethodInfo MethodInfo {
            get { return methodInfo; }
        }

        internal DiscoveryDocument Disco {
            get {
                return discoDoc;
            }
        }

        // Creates a new Uri by combining scheme/host/port from the first param and the absolute path and Query from the second
        internal static string CombineUris(Uri schemeHostPort, string absolutePathAndQuery)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}://{1}{2}",
                schemeHostPort.Scheme,
                schemeHostPort.Authority,
                new Uri(absolutePathAndQuery).PathAndQuery);
        }
    }

    internal class DiscoveryServerProtocolFactory : ServerProtocolFactory {
        protected override ServerProtocol CreateIfRequestCompatible(HttpRequest request) {
            if (request.PathInfo.Length > 0)
                return null;

            if (request.HttpMethod != "GET")
                // MethodNotAllowed = 405,
                return new UnsupportedRequestProtocol(405);

            string queryString = request.QueryString[null];
            if (queryString == null) queryString = "";
            if (request.QueryString["schema"] == null &&
                  request.QueryString["wsdl"] == null &&
                  string.Compare(queryString, "wsdl", StringComparison.OrdinalIgnoreCase) != 0 &&
                  string.Compare(queryString, "disco", StringComparison.OrdinalIgnoreCase) != 0)
                return null;
            
            return new DiscoveryServerProtocol();
        }
    }

    internal sealed class DiscoveryServerProtocol : ServerProtocol {
        DiscoveryServerType serverType;
        object syncRoot = new object();

        internal override bool Initialize() {
            //
            // see if we already cached a DiscoveryServerType
            //
            if (null == (serverType = (DiscoveryServerType)GetFromCache(typeof(DiscoveryServerProtocol), Type))
                && null == (serverType = (DiscoveryServerType)GetFromCache(typeof(DiscoveryServerProtocol), Type, true))) {
                lock (InternalSyncObject) {
                    if (null == (serverType = (DiscoveryServerType)GetFromCache(typeof(DiscoveryServerProtocol), Type))
                        && null == (serverType = (DiscoveryServerType)GetFromCache(typeof(DiscoveryServerProtocol), Type, true)))
                    {
                        //
                        // if not create a new DiscoveryServerType and cache it
                        //
                        bool excludeSchemeHostPortFromCachingKey = this.IsCacheUnderPressure(typeof(DiscoveryServerProtocol), Type);
                        string escapedUri = Uri.EscapeUriString(Request.Url.ToString()).Replace("#", "%23");
                        serverType = new DiscoveryServerType(Type, escapedUri, excludeSchemeHostPortFromCachingKey);
                        AddToCache(typeof(DiscoveryServerProtocol), Type, serverType, excludeSchemeHostPortFromCachingKey);
                    }
                }
            }

            return true;                       
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
            string id = Request.QueryString["schema"];
            Encoding encoding = new UTF8Encoding(false);

            if (id != null) {
                XmlSchema schema = serverType.GetSchema(id);
                if (schema == null) throw new InvalidOperationException(Res.GetString(Res.WebSchemaNotFound));
                Response.ContentType = ContentType.Compose("text/xml", encoding);
                schema.Write(new StreamWriter(outputStream, encoding));
                return;
            }
           
            id = Request.QueryString["wsdl"];
            if (id != null) {
                ServiceDescription description = serverType.GetServiceDescription(id);
                if (description == null) throw new InvalidOperationException(Res.GetString(Res.ServiceDescriptionWasNotFound0));
                Response.ContentType = ContentType.Compose("text/xml", encoding);
                if (this.serverType.UriFixups == null)
                {
                    description.Write(new StreamWriter(outputStream, encoding));
                }
                else
                {
                    lock (this.syncRoot)
                    {
                        this.RunUriFixups();
                        description.Write(new StreamWriter(outputStream, encoding));
                    }
                }
                return;
            }

            string queryString = Request.QueryString[null];
            if (queryString != null && string.Compare(queryString, "wsdl", StringComparison.OrdinalIgnoreCase) == 0) {
                Response.ContentType = ContentType.Compose("text/xml", encoding);
                if (this.serverType.UriFixups == null)
                {
                    serverType.Description.Write(new StreamWriter(outputStream, encoding));
                }
                else
                {
                    lock (this.syncRoot)
                    {
                        this.RunUriFixups();
                        serverType.Description.Write(new StreamWriter(outputStream, encoding));
                    }
                }
                return;
            }

            if (queryString != null && string.Compare(queryString, "disco", StringComparison.OrdinalIgnoreCase) == 0) {
                Response.ContentType = ContentType.Compose("text/xml", encoding);
                if (this.serverType.UriFixups == null)
                {
                    serverType.Disco.Write(new StreamWriter(outputStream, encoding));
                }
                else
                {
                    lock (this.syncRoot)
                    {
                        this.RunUriFixups();
                        serverType.Disco.Write(new StreamWriter(outputStream, encoding));
                    }
                }
                return;
            }


            throw new InvalidOperationException(Res.GetString(Res.internalError0));
        }

        internal override bool WriteException(Exception e, Stream outputStream) {
            Response.Clear();
            Response.ClearHeaders();
            Response.ContentType = ContentType.Compose("text/plain", Encoding.UTF8);
            Response.StatusCode = (int) HttpStatusCode.InternalServerError;
            Response.StatusDescription = HttpWorkerRequest.GetStatusDescription(Response.StatusCode);
            StreamWriter writer = new StreamWriter(outputStream, new UTF8Encoding(false));
            writer.WriteLine(GenerateFaultString(e, true));
            writer.Flush();
            return true;
        }

        internal void Discover() {
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
