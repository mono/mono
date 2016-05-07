//------------------------------------------------------------------------------
// <copyright file="SoapServerProtocol.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Protocols {
    using System;
    using System.Collections;
    using System.IO;
    using System.Net;
    using System.Reflection;
    using System.Text;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;
    using System.Web.Services.Configuration;
    using System.Web.Services.Description;
    using System.Threading;
    using System.Web.Services.Diagnostics;

    internal class Soap11ServerProtocolHelper : SoapServerProtocolHelper {
        internal Soap11ServerProtocolHelper(SoapServerProtocol protocol) : base(protocol) {
        }

        internal Soap11ServerProtocolHelper(SoapServerProtocol protocol, string requestNamespace) : base(protocol, requestNamespace) {
        }

        internal override SoapProtocolVersion Version { 
            get { return SoapProtocolVersion.Soap11; }
        }
        internal override WebServiceProtocols Protocol {
            get { return WebServiceProtocols.HttpSoap; }
        }
        internal override string EnvelopeNs { 
            get { return Soap.Namespace; } 
        }
        internal override string EncodingNs { 
            get { return Soap.Encoding; } 
        }
        internal override string HttpContentType { 
            get { return ContentType.TextXml; } 
        }

        internal override SoapServerMethod RouteRequest() {
            object methodKey;
            
            string methodUriString = ServerProtocol.Request.Headers[Soap.Action];
            if (methodUriString == null)
                throw new SoapException(Res.GetString(Res.UnableToHandleRequestActionRequired0), new XmlQualifiedName(Soap.Code.Client, Soap.Namespace));

            if (ServerType.routingOnSoapAction) {
                if (methodUriString.StartsWith("\"", StringComparison.Ordinal) && methodUriString.EndsWith("\"", StringComparison.Ordinal))
                    methodUriString = methodUriString.Substring(1, methodUriString.Length - 2);
                    
                methodKey = HttpUtility.UrlDecode(methodUriString);
            }
            else {
                try {
                    methodKey = GetRequestElement();
                }
                catch (SoapException) {
                    throw;
                }
                catch (Exception e) {
                    if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException) {
                        throw;
                    }
                    throw new SoapException(Res.GetString(Res.TheRootElementForTheRequestCouldNotBeDetermined0), new XmlQualifiedName(Soap.Code.Server, Soap.Namespace), e);
                }
            }

            TraceMethod caller = Tracing.On ? new TraceMethod(this, "RouteRequest") : null;
            if (Tracing.On) Tracing.Enter("RouteRequest", caller, new TraceMethod(ServerType, "GetMethod", methodKey), Tracing.Details(ServerProtocol.Request));
            SoapServerMethod method = ServerType.GetMethod(methodKey);
            if (Tracing.On) Tracing.Exit("RouteRequest", caller);

            if (method == null) {
                if (ServerType.routingOnSoapAction)
                    throw new SoapException(Res.GetString(Res.WebHttpHeader, Soap.Action, (string) methodKey), new XmlQualifiedName(Soap.Code.Client, Soap.Namespace));
                else
                    throw new SoapException(Res.GetString(Res.TheRequestElementXmlnsWasNotRecognized2, ((XmlQualifiedName) methodKey).Name, ((XmlQualifiedName) methodKey).Namespace), new XmlQualifiedName(Soap.Code.Client, Soap.Namespace));
            }
            
            return method;
        }
        
        internal override void WriteFault(XmlWriter writer, SoapException soapException, HttpStatusCode statusCode) {
            if (statusCode != HttpStatusCode.InternalServerError)
                return;
            if (soapException == null)
                return;
            SoapServerMessage message = ServerProtocol.Message;
            writer.WriteStartDocument();
            writer.WriteStartElement(Soap.Prefix, Soap.Element.Envelope, Soap.Namespace);
            writer.WriteAttributeString("xmlns", Soap.Prefix, null, Soap.Namespace);
            writer.WriteAttributeString("xmlns", "xsi", null, XmlSchema.InstanceNamespace);
            writer.WriteAttributeString("xmlns", "xsd", null, XmlSchema.Namespace);
            if (ServerProtocol.ServerMethod != null)
                SoapHeaderHandling.WriteHeaders(writer, ServerProtocol.ServerMethod.outHeaderSerializer, message.Headers, ServerProtocol.ServerMethod.outHeaderMappings, SoapHeaderDirection.Fault, ServerProtocol.ServerMethod.use == SoapBindingUse.Encoded, ServerType.serviceNamespace, ServerType.serviceDefaultIsEncoded, Soap.Namespace);
            else
                SoapHeaderHandling.WriteUnknownHeaders(writer, message.Headers, Soap.Namespace);
            writer.WriteStartElement(Soap.Element.Body, Soap.Namespace);
            
            writer.WriteStartElement(Soap.Element.Fault, Soap.Namespace);
            writer.WriteStartElement(Soap.Element.FaultCode, "");
            XmlQualifiedName code = TranslateFaultCode(soapException.Code);
            if (code.Namespace != null && code.Namespace.Length > 0 && writer.LookupPrefix(code.Namespace) == null)
                writer.WriteAttributeString("xmlns", "q0", null, code.Namespace);
            writer.WriteQualifiedName(code.Name, code.Namespace);
            writer.WriteEndElement();
            // write faultString element with possible "lang" attribute
            writer.WriteStartElement(Soap.Element.FaultString, "");
            if (soapException.Lang != null && soapException.Lang.Length != 0) {
                writer.WriteAttributeString("xml", Soap.Attribute.Lang, Soap.XmlNamespace, soapException.Lang);
            }
            writer.WriteString(ServerProtocol.GenerateFaultString(soapException));
            writer.WriteEndElement();
            // Only write an actor element if the actor was specified (it's optional for end-points)
            string actor = soapException.Actor;
            if (actor.Length > 0)
                writer.WriteElementString(Soap.Element.FaultActor, "", actor);
            
            // Only write a FaultDetail element if exception is related to the body (not a header)
            if (!(soapException is SoapHeaderException)) {
                if (soapException.Detail == null) {
                    // 



                    writer.WriteStartElement(Soap.Element.FaultDetail, "");
                    writer.WriteEndElement();
                }
                else {
                    soapException.Detail.WriteTo(writer);
                }
            }
            writer.WriteEndElement();
            
            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.Flush();
        }
        
        private static XmlQualifiedName TranslateFaultCode(XmlQualifiedName code) {
            if (code.Namespace == Soap.Namespace) {
                return code;
            }
            else if (code.Namespace == Soap12.Namespace) {
                if (code.Name == Soap12.Code.Receiver)
                    return SoapException.ServerFaultCode;
                else if (code.Name == Soap12.Code.Sender)
                    return SoapException.ClientFaultCode;
                else if (code.Name == Soap12.Code.MustUnderstand)
                    return SoapException.MustUnderstandFaultCode;
                else if (code.Name == Soap12.Code.VersionMismatch)
                    return SoapException.VersionMismatchFaultCode;
            }
            return code;
        }
    }
}
