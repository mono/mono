//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Runtime.Serialization.Json;
    using System.ServiceModel.Description;
    using System.ServiceModel.Web;
    using System.ServiceModel.Web.Configuration;
    using System.Text;
    using System.Xml;
    using System.Xml.Linq;
    using System.Globalization;
    using System.Web;
    using System.Xml.Schema;

    class HelpHtmlBuilder
    {
        const string HelpOperationPageUrl = "help/operations/{0}";
        const string HtmlHtmlElementName = "{http://www.w3.org/1999/xhtml}html";
        const string HtmlHeadElementName = "{http://www.w3.org/1999/xhtml}head";
        const string HtmlTitleElementName = "{http://www.w3.org/1999/xhtml}title";
        const string HtmlBodyElementName = "{http://www.w3.org/1999/xhtml}body";
        const string HtmlBrElementName = "{http://www.w3.org/1999/xhtml}br";
        const string HtmlPElementName = "{http://www.w3.org/1999/xhtml}p";
        const string HtmlTableElementName = "{http://www.w3.org/1999/xhtml}table";
        const string HtmlTrElementName = "{http://www.w3.org/1999/xhtml}tr";
        const string HtmlThElementName = "{http://www.w3.org/1999/xhtml}th";
        const string HtmlTdElementName = "{http://www.w3.org/1999/xhtml}td";
        const string HtmlDivElementName = "{http://www.w3.org/1999/xhtml}div";
        const string HtmlAElementName = "{http://www.w3.org/1999/xhtml}a";
        const string HtmlPreElementName = "{http://www.w3.org/1999/xhtml}pre";
        const string HtmlClassAttributeName = "class";
        const string HtmlTitleAttributeName = "title";
        const string HtmlHrefAttributeName = "href";
        const string HtmlRelAttributeName = "rel";
        const string HtmlIdAttributeName = "id";
        const string HtmlNameAttributeName = "name";
        const string HtmlRowspanAttributeName = "rowspan";
        const string HtmlHeading1Class = "heading1";
        const string HtmlContentClass = "content";

        const string HtmlRequestXmlId = "request-xml";
        const string HtmlRequestJsonId = "request-json";
        const string HtmlRequestSchemaId = "request-schema";
        const string HtmlResponseXmlId = "response-xml";
        const string HtmlResponseJsonId = "response-json";        
        const string HtmlResponseSchemaId = "response-schema";        
        const string HtmlOperationClass = "operation";        

        public static XDocument CreateHelpPage(Uri baseUri, IEnumerable<OperationHelpInformation> operations)
        {
            XDocument document = CreateBaseDocument(SR2.GetString(SR2.HelpPageOperationsAt, baseUri));
            XElement table = new XElement(HtmlTableElementName,
                    new XElement(HtmlTrElementName,
                        new XElement(HtmlThElementName, SR2.GetString(SR2.HelpPageUri)),
                        new XElement(HtmlThElementName, SR2.GetString(SR2.HelpPageMethod)),
                        new XElement(HtmlThElementName, SR2.GetString(SR2.HelpPageDescription))));
            
            string lastOperation = null;
            XElement firstTr = null;
            int rowspan = 0;
            foreach (OperationHelpInformation operationHelpInfo in operations.OrderBy(o => FilterQueryVariables(o.UriTemplate)))
            {
                string operationUri = FilterQueryVariables(operationHelpInfo.UriTemplate);
                string description = operationHelpInfo.Description;
                if (String.IsNullOrEmpty(description))
                {
                    description = SR2.GetString(SR2.HelpPageDefaultDescription, BuildFullUriTemplate(baseUri, operationHelpInfo.UriTemplate));
                }
                XElement tr = new XElement(HtmlTrElementName,
                    new XElement(HtmlTdElementName, new XAttribute(HtmlTitleAttributeName, BuildFullUriTemplate(baseUri, operationHelpInfo.UriTemplate)),
                        new XElement(HtmlAElementName,
                            new XAttribute(HtmlRelAttributeName, HtmlOperationClass),
                            new XAttribute(HtmlHrefAttributeName, String.Format(CultureInfo.InvariantCulture, HelpOperationPageUrl, operationHelpInfo.Name)), operationHelpInfo.Method)),
                    new XElement(HtmlTdElementName, description));
                table.Add(tr);
                if (operationUri != lastOperation)
                {
                    XElement td = new XElement(HtmlTdElementName, operationUri == lastOperation ? String.Empty : operationUri);
                    tr.AddFirst(td);
                    if (firstTr != null && rowspan > 1)
                    {
                        firstTr.Descendants(HtmlTdElementName).First().Add(new XAttribute(HtmlRowspanAttributeName, rowspan));
                    }
                    firstTr = tr;
                    rowspan = 0;
                    lastOperation = operationUri;
                }
                ++rowspan;
            }
            if (firstTr != null && rowspan > 1)
            {
                firstTr.Descendants(HtmlTdElementName).First().Add(new XAttribute(HtmlRowspanAttributeName, rowspan));
            }
            document.Descendants(HtmlBodyElementName).First().Add(new XElement(HtmlDivElementName, new XAttribute(HtmlIdAttributeName, HtmlContentClass),
                new XElement(HtmlPElementName, new XAttribute(HtmlClassAttributeName, HtmlHeading1Class), SR2.GetString(SR2.HelpPageOperationsAt, baseUri)),
                new XElement(HtmlPElementName, SR2.GetString(SR2.HelpPageStaticText)),
                table));
            return document;
        }

        public static XDocument CreateOperationHelpPage(Uri baseUri, OperationHelpInformation operationInfo)
        {            
            XDocument document = CreateBaseDocument(SR2.GetString(SR2.HelpPageReferenceFor, BuildFullUriTemplate(baseUri, operationInfo.UriTemplate)));
            XElement table = new XElement(HtmlTableElementName,
                new XElement(HtmlTrElementName,
                    new XElement(HtmlThElementName, SR2.GetString(SR2.HelpPageMessageDirection)),
                    new XElement(HtmlThElementName, SR2.GetString(SR2.HelpPageFormat)),
                    new XElement(HtmlThElementName, SR2.GetString(SR2.HelpPageBody))));

            RenderMessageInformation(table, operationInfo, true);
            RenderMessageInformation(table, operationInfo, false);

            XElement div = new XElement(HtmlDivElementName, new XAttribute(HtmlIdAttributeName, HtmlContentClass),
                new XElement(HtmlPElementName, new XAttribute(HtmlClassAttributeName, HtmlHeading1Class), SR2.GetString(SR2.HelpPageReferenceFor, BuildFullUriTemplate(baseUri, operationInfo.UriTemplate))),
                new XElement(HtmlPElementName, operationInfo.Description),
                XElement.Parse(SR2.GetString(SR2.HelpPageOperationUri, HttpUtility.HtmlEncode(BuildFullUriTemplate(baseUri, operationInfo.UriTemplate)))),
                XElement.Parse(SR2.GetString(SR2.HelpPageOperationMethod, HttpUtility.HtmlEncode(operationInfo.Method))));
            if (!String.IsNullOrEmpty(operationInfo.JavascriptCallbackParameterName))
            {
                div.Add(XElement.Parse(SR2.GetString(SR2.HelpPageCallbackText, HttpUtility.HtmlEncode(operationInfo.JavascriptCallbackParameterName))), table);
            }
            else
            {
                div.Add(table);
            }
            document.Descendants(HtmlBodyElementName).First().Add(div);

            CreateOperationSamples(document.Descendants(HtmlDivElementName).First(), operationInfo);

            return document;
        }

        public static XDocument CreateMethodNotAllowedPage(Uri helpUri)
        {
            XDocument document = CreateBaseDocument(SR2.GetString(SR2.HelpPageTitleText));

            XElement div = new XElement(HtmlDivElementName, new XAttribute(HtmlIdAttributeName, HtmlContentClass),
                new XElement(HtmlPElementName, new XAttribute(HtmlClassAttributeName, HtmlHeading1Class), SR2.GetString(SR2.HelpPageTitleText)));
            if (helpUri == null)
            {
                div.Add(new XElement(HtmlPElementName, SR2.GetString(SR2.HelpPageMethodNotAllowed)));
            }
            else
            {
                div.Add(XElement.Parse(SR2.GetString(SR2.HelpPageMethodNotAllowedWithLink, HttpUtility.HtmlEncode(helpUri.AbsoluteUri))));
            }
            document.Descendants(HtmlBodyElementName).First().Add(div);
            return document;
        }

        public static XDocument CreateServerErrorPage(Uri helpUri, Exception error)
        {
            XDocument document = CreateBaseDocument(SR2.GetString(SR2.HelpPageRequestErrorTitle));

            XElement div = new XElement(HtmlDivElementName, new XAttribute(HtmlIdAttributeName, HtmlContentClass),
                new XElement(HtmlPElementName, new XAttribute(HtmlClassAttributeName, HtmlHeading1Class), SR2.GetString(SR2.HelpPageRequestErrorTitle)));
            if (helpUri == null)
            {
                if (error != null)
                {
                    //TFS Bug 500275: it is not necessary to HtmlEncode the error.Message string here because XElement ctor will encode it.
                    div.Add(new XElement(HtmlPElementName, SR2.GetString(SR2.HelpServerErrorProcessingRequestWithDetails, error.Message)));
                    div.Add(new XElement(HtmlPElementName, error.StackTrace ?? String.Empty));
                }
                else
                {
                    div.Add(new XElement(HtmlPElementName, SR2.GetString(SR2.HelpServerErrorProcessingRequest)));
                }
            }
            else
            {
                string encodedHelpLink = HttpUtility.HtmlEncode(helpUri.AbsoluteUri);
                if (error != null)
                {
                    //TFS Bug 500275: XElement.Parse does not HtmlEncode the string passed to it, so we need to encode it before calling Parse.
                    string errorMessage = AppSettings.DisableHtmlErrorPageExceptionHtmlEncoding ? error.Message : HttpUtility.HtmlEncode(error.Message);
                    div.Add(XElement.Parse(SR2.GetString(SR2.HelpServerErrorProcessingRequestWithDetailsAndLink, encodedHelpLink, errorMessage)));
                    div.Add(new XElement(HtmlPElementName, error.StackTrace ?? String.Empty));
                }
                else
                {
                    div.Add(XElement.Parse(SR2.GetString(SR2.HelpServerErrorProcessingRequestWithLink, encodedHelpLink)));
                }

            }
            document.Descendants(HtmlBodyElementName).First().Add(div);
            return document;
        }

        public static XDocument CreateEndpointNotFound(Uri helpUri)
        {
            XDocument document = CreateBaseDocument(SR2.GetString(SR2.HelpPageTitleText));

            XElement div = new XElement(HtmlDivElementName, new XAttribute(HtmlIdAttributeName, HtmlContentClass),
                new XElement(HtmlPElementName, new XAttribute(HtmlClassAttributeName, HtmlHeading1Class), SR2.GetString(SR2.HelpPageTitleText)));
            if (helpUri == null)
            {
                div.Add(new XElement(HtmlPElementName, SR2.GetString(SR2.HelpPageEndpointNotFound)));
            }
            else
            {
                div.Add(XElement.Parse(SR2.GetString(SR2.HelpPageEndpointNotFoundWithLink, HttpUtility.HtmlEncode(helpUri.AbsoluteUri))));
            }
            document.Descendants(HtmlBodyElementName).First().Add(div);
            return document;
        }        

        public static XDocument CreateTransferRedirectPage(string originalTo, string newLocation)
        {
            XDocument document = CreateBaseDocument(SR2.GetString(SR2.HelpPageTitleText));

            XElement div = new XElement(HtmlDivElementName, new XAttribute(HtmlIdAttributeName, HtmlContentClass),
                new XElement(HtmlPElementName, new XAttribute(HtmlClassAttributeName, HtmlHeading1Class), SR2.GetString(SR2.HelpPageTitleText)),
                XElement.Parse(SR2.GetString(SR2.HelpPageRedirect, HttpUtility.HtmlEncode(originalTo), HttpUtility.HtmlEncode(newLocation))));
            document.Descendants(HtmlBodyElementName).First().Add(div);
            return document;
        }

        static XDocument CreateBaseDocument(string title)
        {
            return new XDocument(
                new XDocumentType("html", "-//W3C//DTD XHTML 1.0 Transitional//EN", "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd", null),
                new XElement(HtmlHtmlElementName,
                    new XElement(HtmlHeadElementName,
                        new XElement(HtmlTitleElementName, title),
                        new XElement("{http://www.w3.org/1999/xhtml}style", SR2.GetString(SR2.HelpPageHtml))),
                    new XElement(HtmlBodyElementName)));
        }

        static string FilterQueryVariables(string uriTemplate)
        {
            int variablesIndex = uriTemplate.IndexOf('?');
            if (variablesIndex > 0)
            {
                return uriTemplate.Substring(0, variablesIndex);
            }
            return uriTemplate;
        }

        static void RenderMessageInformation(XElement table, OperationHelpInformation operationInfo, bool isRequest)
        {       
            MessageHelpInformation info = isRequest ? operationInfo.Request : operationInfo.Response;
            string direction = isRequest ? SR2.GetString(SR2.HelpPageRequest) : SR2.GetString(SR2.HelpPageResponse);
            string nonLocalizedDirection = isRequest ? HtmlRequestXmlId : HtmlResponseXmlId;

            if (info.BodyDescription != null)
            {
                table.Add(new XElement(HtmlTrElementName,
                    new XElement(HtmlTdElementName, direction),
                    new XElement(HtmlTdElementName, info.FormatString),
                    new XElement(HtmlTdElementName, info.BodyDescription)));                
            }
            else
            {
                if (info.XmlExample != null || info.Schema != null)
                {
                    XElement contentTd;
                    table.Add(new XElement(HtmlTrElementName,
                        new XElement(HtmlTdElementName, direction),
                        new XElement(HtmlTdElementName, "Xml"),
                        contentTd = new XElement(HtmlTdElementName)));

                    if (info.XmlExample != null)
                    {
                        contentTd.Add(new XElement(HtmlAElementName, new XAttribute(HtmlHrefAttributeName, "#" + (isRequest ? HtmlRequestXmlId : HtmlResponseXmlId)), SR2.GetString(SR2.HelpPageExample)));
                        if (info.Schema != null)
                        {
                            contentTd.Add(",");
                        }
                    }
                    if (info.Schema != null)
                    {
                        contentTd.Add(new XElement(HtmlAElementName, new XAttribute(HtmlHrefAttributeName, "#" + (isRequest ? HtmlRequestSchemaId : HtmlResponseSchemaId)), SR2.GetString(SR2.HelpPageSchema)));
                    }
                }
                if (info.JsonExample != null)
                {
                    table.Add(new XElement(HtmlTrElementName,
                        new XElement(HtmlTdElementName, direction),
                        new XElement(HtmlTdElementName, "Json"),
                        new XElement(HtmlTdElementName,
                            new XElement(HtmlAElementName, new XAttribute(HtmlHrefAttributeName, "#" + (isRequest ? HtmlRequestJsonId : HtmlResponseJsonId)), SR2.GetString(SR2.HelpPageExample)))));
                }
            }
        }

        static void CreateOperationSamples(XElement element, OperationHelpInformation operationInfo)
        {
            if (operationInfo.Request.XmlExample != null)
            {
                element.Add(GenerateSampleXml(operationInfo.Request.XmlExample, SR2.GetString(SR2.HelpPageXmlRequest), HtmlRequestXmlId));
            }
            if (operationInfo.Request.JsonExample != null)
            {
                element.Add(AddSampleJson(operationInfo.Request.JsonExample, SR2.GetString(SR2.HelpPageJsonRequest), HtmlRequestJsonId));
            }
            if (operationInfo.Response.XmlExample != null)
            {
                element.Add(GenerateSampleXml(operationInfo.Response.XmlExample, SR2.GetString(SR2.HelpPageXmlResponse), HtmlResponseXmlId));
            }
            if (operationInfo.Response.JsonExample != null)
            {
                element.Add(AddSampleJson(operationInfo.Response.JsonExample, SR2.GetString(SR2.HelpPageJsonResponse), HtmlResponseJsonId));
            }

            if (operationInfo.Request.Schema != null)
            {
                element.Add(GenerateSampleXml(XmlSchemaToXElement(operationInfo.Request.Schema), SR2.GetString(SR2.HelpPageRequestSchema), HtmlRequestSchemaId));
                int count = 0;
                foreach (XmlSchema schema in operationInfo.Request.SchemaSet.Schemas())
                {
                    if (schema.TargetNamespace != operationInfo.Request.Schema.TargetNamespace)
                    {
                        element.Add(GenerateSampleXml(XmlSchemaToXElement(schema), ++count == 1 ? SR2.GetString(SR2.HelpPageAdditionalRequestSchema) : null, HtmlRequestSchemaId));
                    }
                }
            }
            if (operationInfo.Response.Schema != null)
            {
                element.Add(GenerateSampleXml(XmlSchemaToXElement(operationInfo.Response.Schema), SR2.GetString(SR2.HelpPageResponseSchema), HtmlResponseSchemaId));
                int count = 0;
                foreach (XmlSchema schema in operationInfo.Response.SchemaSet.Schemas())
                {
                    if (schema.TargetNamespace != operationInfo.Response.Schema.TargetNamespace)
                    {
                        element.Add(GenerateSampleXml(XmlSchemaToXElement(schema), ++count == 1 ? SR2.GetString(SR2.HelpPageAdditionalResponseSchema) : null, HtmlResponseSchemaId));
                    }
                }
            }
        }

        private static XElement XmlSchemaToXElement(XmlSchema schema)
        {
            XmlWriterSettings settings = new XmlWriterSettings
            {
                CloseOutput = false,
                Indent = true,
            };

            XDocument schemaDocument = new XDocument();

            using (XmlWriter writer = XmlWriter.Create(schemaDocument.CreateWriter(), settings))
            {
                schema.Write(writer);
            }
            return schemaDocument.Root;
        }

        static XElement AddSample(object content, string title, string label)
        {
            if (String.IsNullOrEmpty(title))
            {
                return new XElement(HtmlPElementName,
                    new XElement(HtmlPreElementName, new XAttribute(HtmlClassAttributeName, label), content));
            }
            else
            {
                return new XElement(HtmlPElementName,
                    new XElement(HtmlAElementName, new XAttribute(HtmlNameAttributeName, label), title),
                    new XElement(HtmlPreElementName, new XAttribute(HtmlClassAttributeName, label), content));
            }
        }

        static XElement GenerateSampleXml(XElement content, string title, string label)
        {
            StringBuilder sample = new StringBuilder();
            using (XmlWriter writer = XmlTextWriter.Create(sample, new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true }))
            {
                content.WriteTo(writer);
            }
            return AddSample(sample.ToString(), title, label);
        }

        static XElement AddSampleJson(XElement content, string title, string label)
        {
            StringBuilder sample = new StringBuilder();
            using (MemoryStream stream = new MemoryStream())
            {
                using (XmlJsonWriter writer = new XmlJsonWriter())
                {
                    writer.SetOutput(stream, Encoding.Unicode, false);
                    content.WriteTo(writer);
                }
                stream.Position = 0;
                sample.Append(new StreamReader(stream, Encoding.Unicode).ReadToEnd());
            }
            int depth = 0;
            bool inString = false;
            for (int i = 0; i < sample.Length; ++i)
            {
                if (sample[i] == '"')
                {
                    inString = !inString;
                }
                else if (sample[i] == '{')
                {
                    sample.Insert(i + 1, "\n" + new String('\t', ++depth));
                    i += depth + 1;
                }
                else if (sample[i] == ',' && !inString)
                {
                    sample.Insert(i + 1, "\n" + new String('\t', depth));
                }
                else if (sample[i] == '}' && depth > 0)
                {
                    sample.Insert(i, "\n" + new String('\t', --depth));
                    i += depth + 1;
                }
            }
            return AddSample(sample.ToString(), title, label);
        }

        static string BuildFullUriTemplate(Uri baseUri, string uriTemplate)
        {
            UriTemplate template = new UriTemplate(uriTemplate);
            Uri result = template.BindByPosition(baseUri, template.PathSegmentVariableNames.Concat(template.QueryValueVariableNames).Select(name => "{" + name + "}").ToArray());            
            return result.ToString();
        }
    }
}
