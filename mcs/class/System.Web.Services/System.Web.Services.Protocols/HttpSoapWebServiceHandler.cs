//
// System.Web.Services.Protocols.HttpSoapWebServiceHandler.cs
//
// Author:
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) Ximian, Inc. 2003
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Net;
using System.Web;
using System.Xml;
using System.Text;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using System.Web.Services.Description;

namespace System.Web.Services.Protocols
{
	internal class HttpSoapWebServiceHandler: WebServiceHandler
	{
		SoapTypeStubInfo _typeStubInfo;
		SoapExtension[] _extensionChainHighPrio;
		SoapExtension[] _extensionChainMedPrio;
		SoapExtension[] _extensionChainLowPrio;
		SoapMethodStubInfo methodInfo;
		SoapServerMessage requestMessage = null;
		object server;

		public HttpSoapWebServiceHandler (Type type): base (type)
		{
			_typeStubInfo = (SoapTypeStubInfo) TypeStubManager.GetTypeStub (ServiceType, "Soap");
		}

		public override bool IsReusable 
		{
			get { return false; }
		}

		internal override MethodStubInfo GetRequestMethod (HttpContext context)
		{
			try
			{
				requestMessage = DeserializeRequest (context.Request);
				return methodInfo;
			}
			catch (Exception ex)
			{
				SerializeFault (context, requestMessage, ex);
				return null;
			}
		}

		public override void ProcessRequest (HttpContext context)
		{
			Context = context;
			SoapServerMessage responseMessage = null;

			try
			{
				if (requestMessage == null)
					requestMessage = DeserializeRequest (context.Request);
					
				responseMessage = Invoke (context, requestMessage);
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
				string soapAction = null;
				string ctype;
				Encoding encoding = WebServiceHelper.GetContentEncoding (request.ContentType, out ctype);
				if (ctype != "text/xml")
					throw new WebException ("Content is not XML: " + ctype);
					
				server = CreateServerInstance ();

				SoapServerMessage message = new SoapServerMessage (request, server, stream);
				message.SetStage (SoapMessageStage.BeforeDeserialize);
				message.ContentType = ctype;
				message.ContentEncoding = encoding.WebName;

				// If the routing style is SoapAction, then we can get the method information now
				// and set it to the SoapMessage

				if (_typeStubInfo.RoutingStyle == SoapServiceRoutingStyle.SoapAction)
				{
					soapAction = request.Headers ["SOAPAction"];
					if (soapAction == null) throw new SoapException ("Missing SOAPAction header", SoapException.ClientFaultCode);
					methodInfo = _typeStubInfo.GetMethodForSoapAction (soapAction);
					if (methodInfo == null) throw new SoapException ("Server did not recognize the value of HTTP header SOAPAction: " + soapAction, SoapException.ClientFaultCode);
					message.MethodStubInfo = methodInfo;
				}

				// Execute the high priority global extensions. Do not try to execute the medium and
				// low priority extensions because if the routing style is RequestElement we still
				// don't have method information

				_extensionChainHighPrio = SoapExtension.CreateExtensionChain (_typeStubInfo.SoapExtensions[0]);
				stream = SoapExtension.ExecuteChainStream (_extensionChainHighPrio, stream);
				SoapExtension.ExecuteProcessMessage (_extensionChainHighPrio, message, false);

				// If the routing style is RequestElement, try to get the method name from the
				// stream processed by the high priority extensions

				if (_typeStubInfo.RoutingStyle == SoapServiceRoutingStyle.RequestElement)
				{
					MemoryStream mstream;
					byte[] buffer = null;

					if (stream.CanSeek)
					{
						buffer = new byte [stream.Length];
						for (int n=0; n<stream.Length;)
							n += stream.Read (buffer, n, (int)stream.Length-n);
						mstream = new MemoryStream (buffer);
					}
					else
					{
						buffer = new byte [500];
						mstream = new MemoryStream ();
					
						int len;
						while ((len = stream.Read (buffer, 0, 500)) > 0)
							mstream.Write (buffer, 0, len);
						mstream.Position = 0;
						buffer = mstream.ToArray ();
					}

					soapAction = ReadActionFromRequestElement (new MemoryStream (buffer), encoding);

					stream = mstream;
					methodInfo = (SoapMethodStubInfo) _typeStubInfo.GetMethod (soapAction);
					message.MethodStubInfo = methodInfo;
				}

				// Whatever routing style we used, we should now have the method information.
				// We can now notify the remaining extensions

				if (methodInfo == null) throw new SoapException ("Method '" + soapAction + "' not defined in the web service '" + _typeStubInfo.LogicalType.WebServiceName + "'", SoapException.ClientFaultCode);

				_extensionChainMedPrio = SoapExtension.CreateExtensionChain (methodInfo.SoapExtensions);
				_extensionChainLowPrio = SoapExtension.CreateExtensionChain (_typeStubInfo.SoapExtensions[1]);

				stream = SoapExtension.ExecuteChainStream (_extensionChainMedPrio, stream);
				stream = SoapExtension.ExecuteChainStream (_extensionChainLowPrio, stream);
				SoapExtension.ExecuteProcessMessage (_extensionChainMedPrio, message, false);
				SoapExtension.ExecuteProcessMessage (_extensionChainLowPrio, message, false);

				// Deserialize the request

				StreamReader reader = new StreamReader (stream, encoding, false);
				XmlTextReader xmlReader = new XmlTextReader (reader);

				try
				{
					object content;
					SoapHeaderCollection headers;
					WebServiceHelper.ReadSoapMessage (xmlReader, _typeStubInfo, methodInfo.Use, methodInfo.RequestSerializer, out content, out headers);
					message.InParameters = (object []) content;
					message.SetHeaders (headers);
				}
				catch (Exception ex)
				{
					throw new SoapException ("Could not deserialize Soap message", SoapException.ClientFaultCode, ex);
				}

				// Notify the extensions after deserialization

				message.SetStage (SoapMessageStage.AfterDeserialize);
				SoapExtension.ExecuteProcessMessage (_extensionChainHighPrio, message, false);
				SoapExtension.ExecuteProcessMessage (_extensionChainMedPrio, message, false);
				SoapExtension.ExecuteProcessMessage (_extensionChainLowPrio, message, false);

				xmlReader.Close ();

				return message;
			}
		}

		string ReadActionFromRequestElement (Stream stream, Encoding encoding)
		{
			try
			{
				StreamReader reader = new StreamReader (stream, encoding, false);
				XmlTextReader xmlReader = new XmlTextReader (reader);

				xmlReader.MoveToContent ();
				xmlReader.ReadStartElement ("Envelope", WebServiceHelper.SoapEnvelopeNamespace);

				while (! (xmlReader.NodeType == XmlNodeType.Element && xmlReader.LocalName == "Body" && xmlReader.NamespaceURI == WebServiceHelper.SoapEnvelopeNamespace))
					xmlReader.Skip ();

				xmlReader.ReadStartElement ("Body", WebServiceHelper.SoapEnvelopeNamespace);
				xmlReader.MoveToContent ();

				return xmlReader.LocalName;
			}
			catch (Exception ex)
			{
				string errmsg = "The root element for the request could not be determined. ";
				errmsg += "When RoutingStyle is set to RequestElement, SoapExtensions configured ";
				errmsg += "via an attribute on the method cannot modify the request stream before it is read. ";
				errmsg += "The extension must be configured via the SoapExtensionTypes element in web.config ";
				errmsg += "or the request must arrive at the server as clear text.";
				throw new SoapException (errmsg, SoapException.ServerFaultCode, ex);
			}
		}

		void SerializeResponse (HttpResponse response, SoapServerMessage message)
		{
			SoapMethodStubInfo methodInfo = message.MethodStubInfo;
			
			response.ContentType = "text/xml; charset=utf-8";
			if (message.Exception != null) response.StatusCode = 500;

			long contentLength = 0;
			Stream outStream = null;
			MemoryStream bufferStream = null;
			Stream responseStream = response.OutputStream;
			bool bufferResponse = (methodInfo == null || methodInfo.MethodAttribute.BufferResponse);
			
			if (bufferResponse)
			{
				bufferStream = new MemoryStream ();
				outStream = bufferStream;
			}
			else
				outStream = responseStream;

			try
			{
				// While serializing, process extensions in reverse order

				if (bufferResponse)
				{
					outStream = SoapExtension.ExecuteChainStream (_extensionChainHighPrio, outStream);
					outStream = SoapExtension.ExecuteChainStream (_extensionChainMedPrio, outStream);
					outStream = SoapExtension.ExecuteChainStream (_extensionChainLowPrio, outStream);
	
					message.SetStage (SoapMessageStage.BeforeSerialize);
					SoapExtension.ExecuteProcessMessage (_extensionChainLowPrio, message, true);
					SoapExtension.ExecuteProcessMessage (_extensionChainMedPrio, message, true);
					SoapExtension.ExecuteProcessMessage (_extensionChainHighPrio, message, true);
				}
				
				XmlTextWriter xtw = WebServiceHelper.CreateXmlWriter (outStream);
				
				if (message.Exception == null)
					WebServiceHelper.WriteSoapMessage (xtw, _typeStubInfo, methodInfo.Use, methodInfo.ResponseSerializer, message.OutParameters, message.Headers);
				else
					WebServiceHelper.WriteSoapMessage (xtw, _typeStubInfo, SoapBindingUse.Literal, Fault.Serializer, new Fault (message.Exception), null);

				if (bufferResponse)
				{
					message.SetStage (SoapMessageStage.AfterSerialize);
					SoapExtension.ExecuteProcessMessage (_extensionChainLowPrio, message, true);
					SoapExtension.ExecuteProcessMessage (_extensionChainMedPrio, message, true);
					SoapExtension.ExecuteProcessMessage (_extensionChainHighPrio, message, true);
				}
				
				if (bufferStream != null) contentLength = bufferStream.Length;
				xtw.Close ();
			}
			catch (Exception ex)
			{
				// If the response is buffered, we can discard the response and
				// serialize a new Fault message as response.
				if (bufferResponse) throw ex;
				
				// If it is not buffered, we can't rollback what has been sent,
				// so we can only close the connection and return.
				responseStream.Close ();
				return;
			}
			
			try
			{
				if (bufferResponse)
					responseStream.Write (bufferStream.GetBuffer(), 0, (int) contentLength);
			}
			catch {}
			
			responseStream.Close ();
		}

		void SerializeFault (HttpContext context, SoapServerMessage requestMessage, Exception ex)
		{
			SoapException soex = ex as SoapException;
			if (soex == null) soex = new SoapException (ex.Message, SoapException.ServerFaultCode, ex);

			SoapServerMessage faultMessage;
			if (requestMessage != null)
				faultMessage = new SoapServerMessage (context.Request, soex, requestMessage.MethodStubInfo, requestMessage.Server, requestMessage.Stream);
			else
				faultMessage = new SoapServerMessage (context.Request, soex, null, null, null);

			SerializeResponse (context.Response, faultMessage);
			context.Response.End ();
			return;
		}
		
		private SoapServerMessage Invoke (HttpContext ctx, SoapServerMessage requestMessage)
		{
			SoapMethodStubInfo methodInfo = requestMessage.MethodStubInfo;

			// Assign header values to web service members

			requestMessage.UpdateHeaderValues (requestMessage.Server, methodInfo.Headers);

			// Fill an array with the input parameters at the right position

			object[] parameters = new object[methodInfo.MethodInfo.Parameters.Length];
			ParameterInfo[] inParams = methodInfo.MethodInfo.InParameters;
			for (int n=0; n<inParams.Length; n++)
				parameters [inParams[n].Position] = requestMessage.InParameters [n];

			// Invoke the method

			try
			{
				object[] results = methodInfo.MethodInfo.Invoke (requestMessage.Server, parameters);
				requestMessage.OutParameters = results;
			}
			catch (TargetInvocationException ex)
			{
				throw ex.InnerException;
			}

			// Check that headers with MustUnderstand flag have been understood
			
			foreach (SoapHeader header in requestMessage.Headers)
			{
				if (header.MustUnderstand && !header.DidUnderstand)
					throw new SoapHeaderException ("Header not understood: " + header.GetType(), SoapException.MustUnderstandFaultCode);
			}

			// Collect headers that must be sent to the client
			requestMessage.CollectHeaders (requestMessage.Server, methodInfo.Headers, SoapHeaderDirection.Out);

			return requestMessage;
		}
	}
}
