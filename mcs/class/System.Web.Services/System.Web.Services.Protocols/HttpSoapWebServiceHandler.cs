//
// System.Web.Services.Protocols.HttpSoapWebServiceHandler.cs
//
// Author:
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) Ximian, Inc. 2003
//

using System;
using System.Web;
using System.Xml;
using System.Text;
using System.IO;
using System.Xml.Serialization;

namespace System.Web.Services.Protocols
{
	internal class HttpSoapWebServiceHandler: WebServiceHandler
	{
		Type _type;
		TypeStubInfo _typeStubInfo;

		public HttpSoapWebServiceHandler (Type type)
		{
			_type = type;
			_typeStubInfo = TypeStubManager.GetTypeStub (_type);
		}

		public override bool IsReusable 
		{
			get { return false; }
		}

		public override void ProcessRequest (HttpContext context)
		{
			SoapServerMessage requestMessage = null;
			SoapServerMessage responseMessage = null;

			try
			{
				requestMessage = DeserializeRequest (context.Request);
				responseMessage = Invoke (requestMessage);
				SerializeResponse (context.Response, responseMessage);
			}
			catch (Exception ex)
			{
				SerializeFault (context, requestMessage, ex);
				return;


			}
		}

		SoapServerMessage DeserializeRequest (HttpRequest request)
		{
			Stream stream = request.InputStream;

			using (stream)
			{
				Encoding encoding = WebServiceHelper.GetContentEncoding (request.ContentType);
				StreamReader reader = new StreamReader (stream, encoding, false);
				XmlTextReader xmlReader = new XmlTextReader (reader);

				xmlReader.MoveToContent ();
				xmlReader.ReadStartElement ("Envelope", WebServiceHelper.SoapEnvelopeNamespace);

				SoapHeaderCollection headers = ReadHeaders (xmlReader);

				xmlReader.MoveToContent ();
				xmlReader.ReadStartElement ("Body", WebServiceHelper.SoapEnvelopeNamespace);
				xmlReader.MoveToContent ();

				string methodName;
				string methodNamespace;
				string soapAction = request.Headers ["SOAPAction"];

				if (_typeStubInfo.RoutingStyle ==  SoapServiceRoutingStyle.SoapAction)
				{
					if (soapAction == null) throw new SoapException ("Missing SOAPAction header", SoapException.ClientFaultCode);
					int i = soapAction.LastIndexOf ('/');
					methodName = soapAction.Substring (i + 1);
					methodNamespace = soapAction.Substring (0, i);
				}
				else
				{
					methodName = xmlReader.LocalName;
					methodNamespace = xmlReader.NamespaceURI;
				}

				MethodStubInfo methodInfo = _typeStubInfo.GetMethod (methodName);
				if (methodInfo == null) throw new SoapException ("Method '" + methodName + "' not defined in the web service '" + _typeStubInfo.WebServiceName + "'", SoapException.ClientFaultCode);


				object server = Activator.CreateInstance (_type);
				SoapServerMessage message = new SoapServerMessage (request, headers, methodInfo, server, stream);

				message.SetStage (SoapMessageStage.BeforeDeserialize);
				// TODO: Call extensions

				try
				{
					message.InParameters = (object []) methodInfo.RequestSerializer.Deserialize (xmlReader);
				}
				catch (Exception ex)
				{
					throw new SoapException ("Could not deserialize Soap message", SoapException.ClientFaultCode, ex);
				}

				message.SetStage (SoapMessageStage.AfterDeserialize);
				// TODO: Call extensions

				xmlReader.Close ();

				return message;
			}
		}

		void SerializeResponse (HttpResponse response, SoapServerMessage message)
		{
			MethodStubInfo methodInfo = message.MethodStubInfo;

			Stream outStream = response.OutputStream;
			using (outStream)
			{
				response.ContentType = message.ContentType + "; charset=utf-8";
				message.SetStage (SoapMessageStage.BeforeSerialize);
				// TODO: Call extensions

				// What a waste of UTF8encoders, but it has to be thread safe.
				XmlTextWriter xtw = new XmlTextWriter (outStream, new UTF8Encoding (false));
				xtw.Formatting = Formatting.Indented;

				if (message.Exception == null)
					WebServiceHelper.WriteSoapMessage (xtw, _typeStubInfo, message.MethodStubInfo.ResponseSerializer, message.OutParameters, message.Headers);
				else
				{
					WebServiceHelper.WriteSoapMessage (xtw, _typeStubInfo, _typeStubInfo.FaultSerializer, new Fault (message.Exception), null);
					response.StatusCode = 500;
				}

				message.SetStage (SoapMessageStage.AfterSerialize);
				// TODO: Call extensions

				xtw.Flush ();
				xtw.Close ();
			}
		}

		void SerializeFault (HttpContext context, SoapServerMessage requestMessage, Exception ex)
		{
			SoapException soex = ex as SoapException;
			if (soex == null) soex = new SoapException (ex.Message, SoapException.ServerFaultCode, ex);

			MethodStubInfo stubInfo;
			object server;
			Stream stream;

			SoapServerMessage faultMessage;
			if (requestMessage != null)
				faultMessage = new SoapServerMessage (context.Request, soex, requestMessage.MethodStubInfo, requestMessage.Server, requestMessage.Stream);
			else
				faultMessage = new SoapServerMessage (context.Request, soex, null, null, null);

			SerializeResponse (context.Response, faultMessage);
			return;
		}

		SoapHeaderCollection ReadHeaders (XmlTextReader xmlReader)
		{
			SoapHeaderCollection headers = new SoapHeaderCollection ();
			while (! (xmlReader.NodeType == XmlNodeType.Element && xmlReader.LocalName == "Body" && xmlReader.NamespaceURI == WebServiceHelper.SoapEnvelopeNamespace))
			{
				if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.LocalName == "Header" && xmlReader.NamespaceURI == WebServiceHelper.SoapEnvelopeNamespace)
				{
					xmlReader.ReadStartElement ();
					xmlReader.MoveToContent ();
					XmlQualifiedName qname = new XmlQualifiedName (xmlReader.LocalName, xmlReader.NamespaceURI);
					XmlSerializer headerSerializer = _typeStubInfo.GetHeaderSerializer (qname);
					if (headerSerializer != null)
					{
						SoapHeader header = (SoapHeader) headerSerializer.Deserialize (xmlReader);
						headers.Add (header);
					}
					else
					{
						while (xmlReader.NodeType == XmlNodeType.EndElement)
							xmlReader.Skip ();	// TODO: Check if the header has mustUnderstand=true
						xmlReader.Skip ();
					}
				}
				else
					xmlReader.Skip ();
			}
			return headers;
		}
	}
}
