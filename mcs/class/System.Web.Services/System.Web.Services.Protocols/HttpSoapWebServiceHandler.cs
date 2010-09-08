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
#if !MOBILE
using System.Web.Services.Description;
#endif

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
				requestMessage = DeserializeRequest (context);
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
				if (requestMessage == null) {
					requestMessage = DeserializeRequest (context);
				}
				
				if (methodInfo != null && methodInfo.OneWay) {
					context.Response.BufferOutput = false;
					context.Response.StatusCode = 202;
					context.Response.Flush ();
					context.Response.Close ();
					Invoke (context, requestMessage);
				} else {
					responseMessage = Invoke (context, requestMessage);
					SerializeResponse (context.Response, responseMessage);
				}
			}
			catch (Exception ex)
			{
				if (methodInfo != null && methodInfo.OneWay) {
					context.Response.StatusCode = 500;
					context.Response.Flush ();
					context.Response.Close ();
				} else {
					SerializeFault (context, requestMessage, ex);
				}
			}
			finally {
				IDisposable disp = requestMessage.Server as IDisposable;
				requestMessage = null;
				if (disp != null)
					disp.Dispose();
			}
		}

		SoapServerMessage DeserializeRequest (HttpContext context)
		{
			HttpRequest request = context.Request;
			Stream stream = request.InputStream;

			//using (stream)
			//{
				string soapAction = null;
				string ctype;
				Encoding encoding = WebServiceHelper.GetContentEncoding (request.ContentType, out ctype);
#if NET_2_0
				if (ctype != "text/xml" && ctype != "application/soap+xml")
#else
				if (ctype != "text/xml")
#endif
					throw new WebException ("Content is not XML: " + ctype);
					
				object server = CreateServerInstance ();

				SoapServerMessage message = new SoapServerMessage (request, server, stream);
				message.SetStage (SoapMessageStage.BeforeDeserialize);
				message.ContentType = ctype;
#if NET_2_0
				object soapVer = context.Items ["WebServiceSoapVersion"];
				if (soapVer != null)
					message.SetSoapVersion ((SoapProtocolVersion) soapVer);
#endif

				// If the routing style is SoapAction, then we can get the method information now
				// and set it to the SoapMessage

				if (_typeStubInfo.RoutingStyle == SoapServiceRoutingStyle.SoapAction)
				{
					string headerName = message.IsSoap12 ? "action" : "SOAPAction";
					soapAction = message.IsSoap12 ? WebServiceHelper.GetContextAction(request.ContentType) : request.Headers [headerName];
					if (soapAction == null) {
						if (!message.IsSoap12)
							throw new SoapException ("Missing SOAPAction header", WebServiceHelper.ClientFaultCode (message.IsSoap12));
					}
					else
					{
					methodInfo = _typeStubInfo.GetMethodForSoapAction (soapAction);
					if (methodInfo == null) throw new SoapException ("Server did not recognize the value of HTTP header " + headerName + ": " + soapAction, WebServiceHelper.ClientFaultCode (message.IsSoap12));
					message.MethodStubInfo = methodInfo;
					}
				}

				// Execute the high priority global extensions. Do not try to execute the medium and
				// low priority extensions because if the routing style is RequestElement we still
				// don't have method information

				_extensionChainHighPrio = SoapExtension.CreateExtensionChain (_typeStubInfo.SoapExtensions[0]);
				stream = SoapExtension.ExecuteChainStream (_extensionChainHighPrio, stream);
				SoapExtension.ExecuteProcessMessage (_extensionChainHighPrio, message, stream, false);

				// If the routing style is RequestElement, try to get the method name from the
				// stream processed by the high priority extensions

				if (_typeStubInfo.RoutingStyle == SoapServiceRoutingStyle.RequestElement || (message.IsSoap12 && soapAction == null))
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

					soapAction = ReadActionFromRequestElement (new MemoryStream (buffer), encoding, message.IsSoap12);

					stream = mstream;
					methodInfo = (SoapMethodStubInfo) _typeStubInfo.GetMethod (soapAction);
					message.MethodStubInfo = methodInfo;
				}

				// Whatever routing style we used, we should now have the method information.
				// We can now notify the remaining extensions

				if (methodInfo == null) throw new SoapException ("Method '" + soapAction + "' not defined in the web service '" + _typeStubInfo.LogicalType.WebServiceName + "'", WebServiceHelper.ClientFaultCode (message.IsSoap12));

				_extensionChainMedPrio = SoapExtension.CreateExtensionChain (methodInfo.SoapExtensions);
				_extensionChainLowPrio = SoapExtension.CreateExtensionChain (_typeStubInfo.SoapExtensions[1]);

				stream = SoapExtension.ExecuteChainStream (_extensionChainMedPrio, stream);
				stream = SoapExtension.ExecuteChainStream (_extensionChainLowPrio, stream);
				SoapExtension.ExecuteProcessMessage (_extensionChainMedPrio, message, stream, false);
				SoapExtension.ExecuteProcessMessage (_extensionChainLowPrio, message, stream, false);

				// Deserialize the request

				StreamReader reader = new StreamReader (stream, encoding, false);
				XmlTextReader xmlReader = new XmlTextReader (reader);

				try
				{
					object content;
					SoapHeaderCollection headers;
					WebServiceHelper.ReadSoapMessage (xmlReader, methodInfo, SoapHeaderDirection.In, message.IsSoap12, out content, out headers);
					message.InParameters = (object []) content;
					message.SetHeaders (headers);
				}
				catch (Exception ex)
				{
					throw new SoapException ("Could not deserialize Soap message", WebServiceHelper.ClientFaultCode (message.IsSoap12), ex);
				}

				// Notify the extensions after deserialization

				message.SetStage (SoapMessageStage.AfterDeserialize);
				SoapExtension.ExecuteProcessMessage (_extensionChainHighPrio, message, stream, false);
				SoapExtension.ExecuteProcessMessage (_extensionChainMedPrio, message, stream, false);
				SoapExtension.ExecuteProcessMessage (_extensionChainLowPrio, message, stream, false);

				return message;
			//}
		}

		string ReadActionFromRequestElement (Stream stream, Encoding encoding, bool soap12)
		{
			string envNS = soap12 ?
				WebServiceHelper.Soap12EnvelopeNamespace :
				WebServiceHelper.SoapEnvelopeNamespace;
			try
			{
				StreamReader reader = new StreamReader (stream, encoding, false);
				XmlTextReader xmlReader = new XmlTextReader (reader);

				xmlReader.MoveToContent ();
				xmlReader.ReadStartElement ("Envelope", envNS);

				while (! (xmlReader.NodeType == XmlNodeType.Element && xmlReader.LocalName == "Body" && xmlReader.NamespaceURI == envNS))
					xmlReader.Skip ();

				xmlReader.ReadStartElement ("Body", envNS);
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
				throw new SoapException (errmsg, WebServiceHelper.ServerFaultCode (soap12), ex);
			}
		}

		void SerializeResponse (HttpResponse response, SoapServerMessage message)
		{
			SoapMethodStubInfo methodInfo = message.MethodStubInfo;
			
			if ((message.ContentEncoding != null) && (message.ContentEncoding.Length > 0))
				response.AppendHeader("Content-Encoding", message.ContentEncoding);

			response.ContentType = message.IsSoap12 ?
				"application/soap+xml; charset=utf-8" :
				"text/xml; charset=utf-8";
			if (message.Exception != null) response.StatusCode = 500;

			Stream responseStream = response.OutputStream;
			Stream outStream = responseStream;
			bool bufferResponse = (methodInfo == null || methodInfo.MethodAttribute.BufferResponse);
			response.BufferOutput = bufferResponse;

			try
			{
				// While serializing, process extensions in reverse order

				if (bufferResponse)
				{
					outStream = SoapExtension.ExecuteChainStream (_extensionChainHighPrio, outStream);
					outStream = SoapExtension.ExecuteChainStream (_extensionChainMedPrio, outStream);
					outStream = SoapExtension.ExecuteChainStream (_extensionChainLowPrio, outStream);
	
					message.SetStage (SoapMessageStage.BeforeSerialize);
					SoapExtension.ExecuteProcessMessage (_extensionChainLowPrio, message, outStream, true);
					SoapExtension.ExecuteProcessMessage (_extensionChainMedPrio, message, outStream, true);
					SoapExtension.ExecuteProcessMessage (_extensionChainHighPrio, message, outStream, true);
				}
				
				XmlTextWriter xtw = WebServiceHelper.CreateXmlWriter (outStream);


				if (message.Exception == null)
					WebServiceHelper.WriteSoapMessage (xtw, methodInfo, SoapHeaderDirection.Out, message.OutParameters, message.Headers, message.IsSoap12);
				else if (methodInfo != null) {
#if NET_2_0
					if (message.IsSoap12)
						WebServiceHelper.WriteSoapMessage (xtw, methodInfo, SoapHeaderDirection.Fault, new Soap12Fault (message.Exception), message.Headers, message.IsSoap12);
					else
#endif
					{
						WebServiceHelper.WriteSoapMessage (xtw, methodInfo, SoapHeaderDirection.Fault, new Fault (message.Exception), message.Headers, message.IsSoap12);
					}
				}
				else {
#if NET_2_0
					if (message.IsSoap12)
						WebServiceHelper.WriteSoapMessage (xtw, SoapBindingUse.Literal, Soap12Fault.Serializer, null, new Soap12Fault (message.Exception), null, message.IsSoap12);
					else
#endif
					{
						WebServiceHelper.WriteSoapMessage (xtw, SoapBindingUse.Literal, Fault.Serializer, null, new Fault (message.Exception), null, message.IsSoap12);
					}
				}

				if (bufferResponse)
				{
					message.SetStage (SoapMessageStage.AfterSerialize);
					SoapExtension.ExecuteProcessMessage (_extensionChainLowPrio, message, outStream, true);
					SoapExtension.ExecuteProcessMessage (_extensionChainMedPrio, message, outStream, true);
					SoapExtension.ExecuteProcessMessage (_extensionChainHighPrio, message, outStream, true);
				}
				
				xtw.Flush ();
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
		}

		void SerializeFault (HttpContext context, SoapServerMessage requestMessage, Exception ex)
		{
			SoapException soex = ex as SoapException;
			if (soex == null) soex = new SoapException (ex.ToString (), WebServiceHelper.ServerFaultCode (requestMessage != null && requestMessage.IsSoap12), ex);

			SoapServerMessage faultMessage;
			if (requestMessage != null)
				faultMessage = new SoapServerMessage (context.Request, soex, requestMessage.MethodStubInfo, requestMessage.Server, requestMessage.Stream);
			else
				faultMessage = new SoapServerMessage (context.Request, soex, null, null, null);
#if NET_2_0
			object soapVer = context.Items ["WebServiceSoapVersion"];
			if (soapVer != null)
				faultMessage.SetSoapVersion ((SoapProtocolVersion) soapVer);
#endif

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
					throw new SoapHeaderException ("Header not understood: " + header.GetType(), WebServiceHelper.MustUnderstandFaultCode (requestMessage.IsSoap12));
			}

			// Collect headers that must be sent to the client
			requestMessage.CollectHeaders (requestMessage.Server, methodInfo.Headers, SoapHeaderDirection.Out);

			return requestMessage;
		}
	}
}
