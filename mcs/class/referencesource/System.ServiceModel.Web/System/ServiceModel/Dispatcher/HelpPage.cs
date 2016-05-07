//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Syndication;
    using System.ServiceModel.Web;
    using System.Web;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    class HelpPage
    {
        public const string OperationListHelpPageUriTemplate = "help";
        public const string OperationHelpPageUriTemplate = "help/operations/{operation}";
        const string HelpMethodName = "GetHelpPage";
        const string HelpOperationMethodName = "GetOperationHelpPage";

        DateTime startupTime = DateTime.UtcNow;

        Dictionary<string, OperationHelpInformation> operationInfoDictionary;
        NameValueCache<string> operationPageCache;
        NameValueCache<string> helpPageCache;

        public HelpPage(WebHttpBehavior behavior, ContractDescription description)
        {
            this.operationInfoDictionary = new Dictionary<string, OperationHelpInformation>();
            this.operationPageCache = new NameValueCache<string>();
            this.helpPageCache = new NameValueCache<string>();
            foreach (OperationDescription od in description.Operations)
            {
                operationInfoDictionary.Add(od.Name, new OperationHelpInformation(behavior, od));
            }
        }

        Message GetHelpPage()
        {
            Uri baseUri = UriTemplate.RewriteUri(OperationContext.Current.Channel.LocalAddress.Uri, WebOperationContext.Current.IncomingRequest.Headers[HttpRequestHeader.Host]);
            string helpPage = this.helpPageCache.Lookup(baseUri.Authority);
            if (String.IsNullOrEmpty(helpPage))
            {
                helpPage = HelpHtmlBuilder.CreateHelpPage(baseUri, operationInfoDictionary.Values).ToString();
                if (HttpContext.Current == null)
                {
                    this.helpPageCache.AddOrUpdate(baseUri.Authority, helpPage);
                }
            }
            return WebOperationContext.Current.CreateTextResponse(helpPage, "text/html");
        }

        Message GetOperationHelpPage(string operation)
        {
            Uri requestUri = UriTemplate.RewriteUri(WebOperationContext.Current.IncomingRequest.UriTemplateMatch.RequestUri, WebOperationContext.Current.IncomingRequest.Headers[HttpRequestHeader.Host]);
            string helpPage = this.operationPageCache.Lookup(requestUri.AbsoluteUri);
            if (String.IsNullOrEmpty(helpPage))
            {
                OperationHelpInformation operationInfo;
                if (this.operationInfoDictionary.TryGetValue(operation, out operationInfo))
                {
                    Uri baseUri = UriTemplate.RewriteUri(OperationContext.Current.Channel.LocalAddress.Uri, WebOperationContext.Current.IncomingRequest.Headers[HttpRequestHeader.Host]);
                    helpPage = HelpHtmlBuilder.CreateOperationHelpPage(baseUri, operationInfo).ToString();
                    if (HttpContext.Current == null)
                    {
                        this.operationPageCache.AddOrUpdate(requestUri.AbsoluteUri, helpPage);
                    }
                }
                else
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WebFaultException(HttpStatusCode.NotFound));
                }
            }
            return WebOperationContext.Current.CreateTextResponse(helpPage, "text/html");
        }

        public static IEnumerable<KeyValuePair<UriTemplate, object>> GetOperationTemplatePairs()
        {
            return new KeyValuePair<UriTemplate, object>[]
            {
                new KeyValuePair<UriTemplate, object>(new UriTemplate(OperationListHelpPageUriTemplate), HelpMethodName),
                new KeyValuePair<UriTemplate, object>(new UriTemplate(OperationHelpPageUriTemplate), HelpOperationMethodName)
            };
        }

        public object Invoke(UriTemplateMatch match)
        {
            if (HttpContext.Current != null)
            {
                HttpContext.Current.Response.Cache.SetCacheability(HttpCacheability.Public);
                HttpContext.Current.Response.Cache.SetMaxAge(TimeSpan.MaxValue);
                HttpContext.Current.Response.Cache.AddValidationCallback(new HttpCacheValidateHandler(this.CacheValidationCallback), this.startupTime);
                HttpContext.Current.Response.Cache.SetValidUntilExpires(true);
            }
            switch ((string)match.Data)
            {
                case HelpMethodName:
                    return GetHelpPage();
                case HelpOperationMethodName:
                    return GetOperationHelpPage(match.BoundVariables["operation"]);
                default:
                    return null;
            }
        }

        void CacheValidationCallback(HttpContext context, object state, ref HttpValidationStatus result)
        {
            if (((DateTime)state) == this.startupTime)
            {
                result = HttpValidationStatus.Valid;
            }
            else
            {
                result = HttpValidationStatus.Invalid;
            }
        }
    }

    class OperationHelpInformation
    {
        OperationDescription od;
        WebHttpBehavior behavior;
        MessageHelpInformation request;
        MessageHelpInformation response;

        internal OperationHelpInformation(WebHttpBehavior behavior, OperationDescription od)
        {
            this.od = od;
            this.behavior = behavior;
        }

        public string Name
        {
            get
            {
                return od.Name;
            }
        }

        public string UriTemplate
        {
            get
            {
                return UriTemplateClientFormatter.GetUTStringOrDefault(od);
            }
        }

        public string Method
        {
            get
            {
                return WebHttpBehavior.GetWebMethod(od);
            }
        }

        public string Description
        {
            get
            {
                return WebHttpBehavior.GetDescription(od);
            }
        }

        public string JavascriptCallbackParameterName
        {
            get
            {
                if (this.Response.SupportsJson && this.Method == WebHttpBehavior.GET)
                {
                    return behavior.JavascriptCallbackParameterName;
                }
                return null;
            }
        }

        public WebMessageBodyStyle BodyStyle
        {
            get
            {
                return behavior.GetBodyStyle(od);
            }
        }

        public MessageHelpInformation Request
        {
            get
            {
                if (this.request == null)
                {
                    this.request = new MessageHelpInformation(od, true, GetRequestBodyType(od, this.UriTemplate),
                        this.BodyStyle == WebMessageBodyStyle.WrappedRequest || this.BodyStyle == WebMessageBodyStyle.Wrapped);
                }
                return this.request;
            }
        }

        public MessageHelpInformation Response
        {
            get
            {
                if (this.response == null)
                {
                    this.response = new MessageHelpInformation(od, false, GetResponseBodyType(od),
                        this.BodyStyle == WebMessageBodyStyle.WrappedResponse || this.BodyStyle == WebMessageBodyStyle.Wrapped);
                }
                return this.response;
            }
        }

        static Type GetResponseBodyType(OperationDescription od)
        {
            if (WebHttpBehavior.IsUntypedMessage(od.Messages[1]))
            {
                return typeof(Message);
            }
            else if (WebHttpBehavior.IsTypedMessage(od.Messages[1]))
            {
                return od.Messages[1].MessageType;
            }
            else if (od.Messages[1].Body.Parts.Count > 0)
            {
                // If it is more than 0 the response is wrapped and not supported
                return null;
            }
            else
            {
                return (od.Messages[1].Body.ReturnValue.Type);
            }
        }

        static Type GetRequestBodyType(OperationDescription od, string uriTemplate)
        {
            if (od.Behaviors.Contains(typeof(WebGetAttribute)))
            {
                return typeof(void);
            }
            else if (WebHttpBehavior.IsUntypedMessage(od.Messages[0]))
            {
                return typeof(Message);
            }
            else if (WebHttpBehavior.IsTypedMessage(od.Messages[0]))
            {
                return od.Messages[0].MessageType;
            }
            else
            {
                UriTemplate template = new UriTemplate(uriTemplate);
                IEnumerable<MessagePartDescription> parts =
                    from part in od.Messages[0].Body.Parts
                    where !template.PathSegmentVariableNames.Contains(part.Name.ToUpperInvariant()) && !template.QueryValueVariableNames.Contains(part.Name.ToUpperInvariant())
                    select part;

                if (parts.Count() == 1)
                {
                    return parts.First().Type;
                }
                else if (parts.Count() == 0)
                {
                    return typeof(void);
                }
                else
                {
                    // The request is wrapped and not supported
                    return null;
                }
            }
        }
    }

    class MessageHelpInformation
    {
        public string BodyDescription { get; private set; }
        public string FormatString { get; private set; }
        public Type Type { get; private set; }
        public bool SupportsJson { get; private set; }
        public XmlSchemaSet SchemaSet { get; private set; }
        public XmlSchema Schema { get; private set; }
        public XElement XmlExample { get; private set; }
        public XElement JsonExample { get; private set; }

        internal MessageHelpInformation(OperationDescription od, bool isRequest, Type type, bool wrapped)
        {
            this.Type = type;
            this.SupportsJson = WebHttpBehavior.SupportsJsonFormat(od);
            string direction = isRequest ? SR2.GetString(SR2.HelpPageRequest) : SR2.GetString(SR2.HelpPageResponse);

            if (wrapped && !typeof(void).Equals(type))
            {
                this.BodyDescription = SR2.GetString(SR2.HelpPageBodyIsWrapped, direction);
                this.FormatString = SR2.GetString(SR2.HelpPageUnknown);
            }
            else if (typeof(void).Equals(type))
            {
                this.BodyDescription = SR2.GetString(SR2.HelpPageBodyIsEmpty, direction);
                this.FormatString = SR2.GetString(SR2.HelpPageNA);
            }
            else if (typeof(Message).IsAssignableFrom(type))
            {
                this.BodyDescription = SR2.GetString(SR2.HelpPageIsMessage, direction);
                this.FormatString = SR2.GetString(SR2.HelpPageUnknown);
            }
            else if (typeof(Stream).IsAssignableFrom(type))
            {
                this.BodyDescription = SR2.GetString(SR2.HelpPageIsStream, direction);
                this.FormatString = SR2.GetString(SR2.HelpPageUnknown);
            }
            else if (typeof(Atom10FeedFormatter).IsAssignableFrom(type))
            {
                this.BodyDescription = SR2.GetString(SR2.HelpPageIsAtom10Feed, direction);
                this.FormatString = WebMessageFormat.Xml.ToString();
            }
            else if (typeof(Atom10ItemFormatter).IsAssignableFrom(type))
            {
                this.BodyDescription = SR2.GetString(SR2.HelpPageIsAtom10Entry, direction);
                this.FormatString = WebMessageFormat.Xml.ToString();
            }
            else if (typeof(AtomPub10ServiceDocumentFormatter).IsAssignableFrom(type))
            {
                this.BodyDescription = SR2.GetString(SR2.HelpPageIsAtomPubServiceDocument, direction);
                this.FormatString = WebMessageFormat.Xml.ToString();
            }
            else if (typeof(AtomPub10CategoriesDocumentFormatter).IsAssignableFrom(type))
            {
                this.BodyDescription = SR2.GetString(SR2.HelpPageIsAtomPubCategoriesDocument, direction);
                this.FormatString = WebMessageFormat.Xml.ToString();
            }
            else if (typeof(Rss20FeedFormatter).IsAssignableFrom(type))
            {
                this.BodyDescription = SR2.GetString(SR2.HelpPageIsRSS20Feed, direction);
                this.FormatString = WebMessageFormat.Xml.ToString();
            }
            else if (typeof(SyndicationFeedFormatter).IsAssignableFrom(type))
            {
                this.BodyDescription = SR2.GetString(SR2.HelpPageIsSyndication, direction);
                this.FormatString = WebMessageFormat.Xml.ToString();
            }
            else if (typeof(XElement).IsAssignableFrom(type) || typeof(XmlElement).IsAssignableFrom(type))
            {
                this.BodyDescription = SR2.GetString(SR2.HelpPageIsXML, direction);
                this.FormatString = WebMessageFormat.Xml.ToString();
            }
            else
            {
                try
                {
                    bool usesXmlSerializer = od.Behaviors.Contains(typeof(XmlSerializerOperationBehavior));
                    XmlQualifiedName name;
                    this.SchemaSet = new XmlSchemaSet();
                    IDictionary<XmlQualifiedName, Type> knownTypes = new Dictionary<XmlQualifiedName, Type>();
                    if (usesXmlSerializer)
                    {
                        XmlReflectionImporter importer = new XmlReflectionImporter();
                        XmlTypeMapping typeMapping = importer.ImportTypeMapping(this.Type);
                        name = new XmlQualifiedName(typeMapping.ElementName, typeMapping.Namespace);
                        XmlSchemas schemas = new XmlSchemas();
                        XmlSchemaExporter exporter = new XmlSchemaExporter(schemas);
                        exporter.ExportTypeMapping(typeMapping);
                        foreach (XmlSchema schema in schemas)
                        {
                            this.SchemaSet.Add(schema);
                        }
                    }
                    else
                    {
                        XsdDataContractExporter exporter = new XsdDataContractExporter();
                        List<Type> listTypes = new List<Type>(od.KnownTypes);
                        bool isQueryable;
                        Type dataContractType = DataContractSerializerOperationFormatter.GetSubstituteDataContractType(this.Type, out isQueryable);
                        listTypes.Add(dataContractType);
                        exporter.Export(listTypes);
                        if (!exporter.CanExport(dataContractType))
                        {
                            this.BodyDescription = SR2.GetString(SR2.HelpPageCouldNotGenerateSchema);
                            this.FormatString = SR2.GetString(SR2.HelpPageUnknown);
                            return;
                        }
                        name = exporter.GetRootElementName(dataContractType);
                        DataContract typeDataContract = DataContract.GetDataContract(dataContractType);
                        if (typeDataContract.KnownDataContracts != null)
                        {
                            foreach (XmlQualifiedName dataContractName in typeDataContract.KnownDataContracts.Keys)
                            {
                                knownTypes.Add(dataContractName, typeDataContract.KnownDataContracts[dataContractName].UnderlyingType);
                            }
                        }
                        foreach (Type knownType in od.KnownTypes)
                        {
                            XmlQualifiedName knownTypeName = exporter.GetSchemaTypeName(knownType);
                            if (!knownTypes.ContainsKey(knownTypeName))
                            {
                                knownTypes.Add(knownTypeName, knownType);
                            }
                        }

                        foreach (XmlSchema schema in exporter.Schemas.Schemas())
                        {
                            this.SchemaSet.Add(schema);
                        }
                    }
                    this.SchemaSet.Compile();

                    XmlWriterSettings settings = new XmlWriterSettings
                    {
                        CloseOutput = false,
                        Indent = true,
                    };

                    if (this.SupportsJson)
                    {
                        XDocument exampleDocument = new XDocument();
                        using (XmlWriter writer = XmlWriter.Create(exampleDocument.CreateWriter(), settings))
                        {
                            HelpExampleGenerator.GenerateJsonSample(this.SchemaSet, name, writer, knownTypes);
                        }
                        this.JsonExample = exampleDocument.Root;
                    }

                    if (name.Namespace != "http://schemas.microsoft.com/2003/10/Serialization/")
                    {
                        foreach (XmlSchema schema in this.SchemaSet.Schemas(name.Namespace))
                        {
                            this.Schema = schema;

                        }
                    }

                    XDocument XmlExampleDocument = new XDocument();
                    using (XmlWriter writer = XmlWriter.Create(XmlExampleDocument.CreateWriter(), settings))
                    {
                        HelpExampleGenerator.GenerateXmlSample(this.SchemaSet, name, writer);
                    }
                    this.XmlExample = XmlExampleDocument.Root;

                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }
                    this.BodyDescription = SR2.GetString(SR2.HelpPageCouldNotGenerateSchema);
                    this.FormatString = SR2.GetString(SR2.HelpPageUnknown);
                    this.Schema = null;
                    this.JsonExample = null;
                    this.XmlExample = null;
                }
            }
        }
    }
}
