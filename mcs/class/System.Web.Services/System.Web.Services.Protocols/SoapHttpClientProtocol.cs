// 
// System.Web.Services.Protocols.SoapHttpClientProtocol.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//   Miguel de Icaza (miguel@ximian.com)
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) Tim Coleman, 2002
// Copyright (C) Ximian, Inc, 2003
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

using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Net;
using System.Web;
using System.Xml;
using System.Text;
using System.Reflection;
using System.Web.Services;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Web.Services.Description;
using System.Web.Services.Discovery;
using System.Xml.Serialization;
using System.Xml.Schema;
using System.Collections;
using System.Threading;

namespace System.Web.Services.Protocols 
{
	[System.Runtime.InteropServices.ComVisible (true)]
	public class SoapHttpClientProtocol : HttpWebClientProtocol 
	{
		SoapTypeStubInfo type_info;
		SoapProtocolVersion soapVersion;

		#region SoapWebClientAsyncResult class

		internal class SoapWebClientAsyncResult: WebClientAsyncResult
		{
			public SoapWebClientAsyncResult (WebRequest request, AsyncCallback callback, object asyncState)
			: base (request, callback, asyncState)
			{
			}
		
			public SoapClientMessage Message;
			public SoapExtension[] Extensions;
		}
		#endregion

		#region Constructors

		public SoapHttpClientProtocol () 
		{
			type_info = (SoapTypeStubInfo) TypeStubManager.GetTypeStub (this.GetType (), "Soap");
		}

		#endregion // Constructors

		#region Methods

		protected IAsyncResult BeginInvoke (string methodName, object[] parameters, AsyncCallback callback, object asyncState)
		{
			SoapMethodStubInfo msi = (SoapMethodStubInfo) type_info.GetMethod (methodName);

			SoapWebClientAsyncResult ainfo = null;
			try
			{
				SoapClientMessage message = new SoapClientMessage (this, msi, Url, parameters);
				message.CollectHeaders (this, message.MethodStubInfo.Headers, SoapHeaderDirection.In);
				
				WebRequest request = GetRequestForMessage (uri, message);
				
				ainfo = new SoapWebClientAsyncResult (request, callback, asyncState);
				ainfo.Message = message;
				ainfo.Extensions = SoapExtension.CreateExtensionChain (type_info.SoapExtensions[0], msi.SoapExtensions, type_info.SoapExtensions[1]);

				ainfo.Request.BeginGetRequestStream (new AsyncCallback (AsyncGetRequestStreamDone), ainfo);
				RegisterMapping (asyncState, ainfo);
			}
			catch (Exception ex)
			{
				if (ainfo != null)
					ainfo.SetCompleted (null, ex, false);
			}

			return ainfo;
		}

		void AsyncGetRequestStreamDone (IAsyncResult ar)
		{
			SoapWebClientAsyncResult ainfo = (SoapWebClientAsyncResult) ar.AsyncState;
			try
			{
				SendRequest (ainfo.Request.EndGetRequestStream (ar), ainfo.Message, ainfo.Extensions);
				ainfo.Request.BeginGetResponse (new AsyncCallback (AsyncGetResponseDone), ainfo);
			}
			catch (Exception ex)
			{
				ainfo.SetCompleted (null, ex, true);
			}
		}

		void AsyncGetResponseDone (IAsyncResult ar)
		{
			SoapWebClientAsyncResult ainfo = (SoapWebClientAsyncResult) ar.AsyncState;
			WebResponse response = null;

			try {
				response = GetWebResponse (ainfo.Request, ar);
			}
			catch (WebException ex) {
				response = ex.Response;
				HttpWebResponse http_response = response as HttpWebResponse;
				if (http_response == null || http_response.StatusCode != HttpStatusCode.InternalServerError) {
					ainfo.SetCompleted (null, ex, true);
					return;
				}
			}
			catch (Exception ex) {
				ainfo.SetCompleted (null, ex, true);
				return;
			}

			try {
				object[] result = ReceiveResponse (response, ainfo.Message, ainfo.Extensions);
				ainfo.SetCompleted (result, null, true);
			}
			catch (Exception ex) {
				ainfo.SetCompleted (null, ex, true);
			}
			finally {
				response.Close();
			}
		}

		protected object[] EndInvoke (IAsyncResult asyncResult)
		{
			if (!(asyncResult is SoapWebClientAsyncResult)) throw new ArgumentException ("asyncResult is not the return value from BeginInvoke");

			SoapWebClientAsyncResult ainfo = (SoapWebClientAsyncResult) asyncResult;
			lock (ainfo)
			{
				if (!ainfo.IsCompleted)
					ainfo.WaitForComplete ();

				UnregisterMapping (ainfo.AsyncState);
				
				if (ainfo.Exception != null)
					throw ainfo.Exception;
				else
					return (object[]) ainfo.Result;
			}
		}

		public void Discover ()
		{
			BindingInfo bnd = (BindingInfo) type_info.Bindings [0];
			
			DiscoveryClientProtocol discoverer = new DiscoveryClientProtocol ();
			discoverer.Discover (Url);
			
			foreach (object info in discoverer.AdditionalInformation)
			{
				System.Web.Services.Discovery.SoapBinding sb = info as System.Web.Services.Discovery.SoapBinding;
				if (sb != null && sb.Binding.Name == bnd.Name && sb.Binding.Namespace == bnd.Namespace) {
					Url = sb.Address;
					return;
				}
			}
			
			string msg = string.Format (
		        	"The binding named '{0}' from namespace '{1}' was not found in the discovery document at '{2}'",
				bnd.Name, bnd.Namespace, Url);
			throw new Exception (msg);
		}

		protected override WebRequest GetWebRequest (Uri uri)
		{
			return base.GetWebRequest (uri);
		}

		WebRequest GetRequestForMessage (Uri uri, SoapClientMessage message)
		{
			WebRequest request = GetWebRequest (uri);
			request.Method = "POST";
			WebHeaderCollection headers = request.Headers;
			request.ContentType = message.ContentType + "; charset=utf-8";
			if (!message.IsSoap12) {
				headers.Add ("SOAPAction", "\"" + message.Action + "\"");
			} else {
				request.ContentType += "; action=\"" + message.Action + "\"";
			}
			return request;
		}

		[MonoTODO]
		protected virtual
		XmlReader GetReaderForMessage (
			SoapClientMessage message, int bufferSize)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual
		XmlWriter GetWriterForMessage (
			SoapClientMessage message, int bufferSize)
		{
			throw new NotImplementedException ();
		}

		void SendRequest (Stream s, SoapClientMessage message, SoapExtension[] extensions)
		{
			using (s) {

				if (extensions != null) {
					s = SoapExtension.ExecuteChainStream (extensions, s);
					message.SetStage (SoapMessageStage.BeforeSerialize);
					SoapExtension.ExecuteProcessMessage (extensions, message, s, true);
				}

				XmlTextWriter xtw = WebServiceHelper.CreateXmlWriter (s);
				WebServiceHelper.WriteSoapMessage (xtw, message.MethodStubInfo, SoapHeaderDirection.In, message.Parameters, message.Headers, message.IsSoap12);

				if (extensions != null) {
					message.SetStage (SoapMessageStage.AfterSerialize);
					SoapExtension.ExecuteProcessMessage (extensions, message, s, true);
				}

				xtw.Flush ();
				xtw.Close ();
			 }
		}


		//
		// TODO:
		//    Handle other web responses (multi-output?)
		//    
		object [] ReceiveResponse (WebResponse response, SoapClientMessage message, SoapExtension[] extensions)
		{
			SoapMethodStubInfo msi = message.MethodStubInfo;
			HttpWebResponse http_response = response as HttpWebResponse;

			if (http_response != null)
			{
				HttpStatusCode code = http_response.StatusCode;
	
				if (!(code == HttpStatusCode.Accepted || code == HttpStatusCode.OK || code == HttpStatusCode.InternalServerError)) {
					string msg = "The request failed with HTTP status {0}: {1}";
					msg = String.Format (msg, (int) code, code);
					throw new WebException (msg, null, WebExceptionStatus.ProtocolError, http_response);
				}
				if (message.OneWay && response.ContentLength <= 0 && (code == HttpStatusCode.Accepted || code == HttpStatusCode.OK)) {
					return new object[0];
				}
			}
			
			//
			// Remove optional encoding
			//
			string ctype;
			Encoding encoding = WebServiceHelper.GetContentEncoding (response.ContentType, out ctype);
			ctype = ctype.ToLower (CultureInfo.InvariantCulture);
			if ((!message.IsSoap12 || ctype != "application/soap+xml") && ctype != "text/xml")
				WebServiceHelper.InvalidOperation (
					String.Format ("Not supported Content-Type in the response: '{0}'", response.ContentType),
					response, encoding);

			message.ContentType = ctype;
			message.ContentEncoding = encoding.WebName;
			
			Stream stream = response.GetResponseStream ();

			if (extensions != null) {
				stream = SoapExtension.ExecuteChainStream (extensions, stream);
				message.SetStage (SoapMessageStage.BeforeDeserialize);
				SoapExtension.ExecuteProcessMessage (extensions, message, stream, false);
			}
			
			// Deserialize the response

			SoapHeaderCollection headers;
			object content;

			using (StreamReader reader = new StreamReader (stream, encoding, false)) {
				XmlTextReader xml_reader = new XmlTextReader (reader);

				WebServiceHelper.ReadSoapMessage (xml_reader, msi, SoapHeaderDirection.Out, message.IsSoap12, out content, out headers);
			}

			if (content is Soap12Fault) {
				SoapException ex = WebServiceHelper.Soap12FaultToSoapException ((Soap12Fault) content);
				message.SetException (ex);
			}
			else
			if (content is Fault) {
				Fault fault = (Fault) content;
				SoapException ex = new SoapException (fault.faultstring, fault.faultcode, fault.faultactor, fault.detail);
				message.SetException (ex);
			}
			else
				message.OutParameters = (object[]) content;
			
			message.SetHeaders (headers);
			message.UpdateHeaderValues (this, message.MethodStubInfo.Headers);

			if (extensions != null) {
				message.SetStage (SoapMessageStage.AfterDeserialize);
				SoapExtension.ExecuteProcessMessage (extensions, message, stream, false);
			}

			if (message.Exception == null)
				return message.OutParameters;
			else
				throw message.Exception;
		}

		protected object[] Invoke (string method_name, object[] parameters)
		{
			SoapMethodStubInfo msi = (SoapMethodStubInfo) type_info.GetMethod (method_name);
			
			SoapClientMessage message = new SoapClientMessage (this, msi, Url, parameters);
			message.CollectHeaders (this, message.MethodStubInfo.Headers, SoapHeaderDirection.In);

			SoapExtension[] extensions = SoapExtension.CreateExtensionChain (type_info.SoapExtensions[0], msi.SoapExtensions, type_info.SoapExtensions[1]);

			WebResponse response;
			try
			{
				WebRequest request = GetRequestForMessage (uri, message);
				SendRequest (request.GetRequestStream (), message, extensions);
				response = GetWebResponse (request);
			}
			catch (WebException ex)
			{
				response = ex.Response;
				HttpWebResponse http_response = response as HttpWebResponse;
				if (http_response == null || http_response.StatusCode != HttpStatusCode.InternalServerError)
					throw ex;
			}

			try {
				return ReceiveResponse (response, message, extensions);
			}
			finally {
				response.Close();
			}
		}
		

		[MonoTODO ("Do something with this")]
		[System.Runtime.InteropServices.ComVisible(false)]
		[DefaultValue (SoapProtocolVersion.Default)]
		public SoapProtocolVersion SoapVersion {
			get { return soapVersion; }
			set { soapVersion = value; }
		}
		
		protected void InvokeAsync (string methodName, object[] parameters, SendOrPostCallback callback)
		{
			InvokeAsync (methodName, parameters, callback, null);
		}

		protected void InvokeAsync (string methodName, object[] parameters, SendOrPostCallback callback, object userState)
		{
			InvokeAsyncInfo info = new InvokeAsyncInfo (callback, userState);
			BeginInvoke (methodName, parameters, new AsyncCallback (InvokeAsyncCallback), info);
		}
		
		void InvokeAsyncCallback (IAsyncResult ar)
		{
			InvokeAsyncInfo info = (InvokeAsyncInfo) ar.AsyncState;
			SoapWebClientAsyncResult sar = (SoapWebClientAsyncResult) ar;
			InvokeCompletedEventArgs args = new InvokeCompletedEventArgs (sar.Exception, false, info.UserState, (object[]) sar.Result);
			UnregisterMapping (ar.AsyncState);
			if (info.Context != null)
				info.Context.Send (info.Callback, args);
			else
				info.Callback (args);
		}


		#endregion // Methods
	}
}

