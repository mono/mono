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
using System.Xml.Serialization;
using System.Xml.Schema;
using System.Collections;

namespace System.Web.Services.Protocols {
	public class SoapHttpClientProtocol : HttpWebClientProtocol {
		TypeStubInfo type_info;
		
		#region Constructors

		public SoapHttpClientProtocol () 
		{
			type_info = TypeStubManager.GetTypeStub (this.GetType ());
		}

		#endregion // Constructors

		#region Methods

		[MonoTODO]
		protected IAsyncResult BeginInvoke (string methodName, object[] parameters, AsyncCallback callback, object asyncState)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Discover ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected object[] EndInvoke (IAsyncResult asyncResult)
		{
			throw new NotImplementedException ();
		}

		protected override WebRequest GetWebRequest (Uri uri)
		{
			return base.GetWebRequest (uri);
		}

		//
		// Just for debugging
		//
		void DumpStackFrames ()
		{
			StackTrace st = new StackTrace ();

			for (int i = 0; i < st.FrameCount; i++){
				StackFrame sf = st.GetFrame (i);
				Console.WriteLine ("At frame: {0} {1}", i, sf.GetMethod ().Name);
			}
		}
		
		void SendRequest (WebRequest request, SoapClientMessage message, SoapExtension[] extensions)
		{
			message.CollectHeaders (this, message.MethodStubInfo.Headers, SoapHeaderDirection.In);

			request.Method = "POST";
			WebHeaderCollection headers = request.Headers;
			headers.Add ("SOAPAction", message.Action);

			request.ContentType = message.ContentType + "; charset=utf-8";

			Stream s = request.GetRequestStream ();
			using (s) {

				if (extensions != null) {
					s = SoapExtension.ExecuteChainStream (extensions, s);
					message.SetStage (SoapMessageStage.BeforeSerialize);
					SoapExtension.ExecuteProcessMessage (extensions, message, true);
				}

				// What a waste of UTF8encoders, but it has to be thread safe.
				XmlTextWriter xtw = new XmlTextWriter (s, new UTF8Encoding (false));
				xtw.Formatting = Formatting.Indented;

				WebServiceHelper.WriteSoapMessage (xtw, type_info, message.MethodStubInfo.RequestSerializer, message.Parameters, message.Headers);

				if (extensions != null) {
					message.SetStage (SoapMessageStage.AfterSerialize);
					SoapExtension.ExecuteProcessMessage (extensions, message, true);
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
			HttpWebResponse http_response = (HttpWebResponse) response;
			HttpStatusCode code = http_response.StatusCode;
			MethodStubInfo msi = message.MethodStubInfo;

			if (!(code == HttpStatusCode.Accepted || code == HttpStatusCode.OK || code == HttpStatusCode.InternalServerError))
				throw new Exception ("Return code was: " + http_response.StatusCode);

			//
			// Remove optional encoding
			//
			Encoding encoding = WebServiceHelper.GetContentEncoding (response.ContentType);

			Stream stream = response.GetResponseStream ();

			if (extensions != null) {
				stream = SoapExtension.ExecuteChainStream (extensions, stream);
				message.SetStage (SoapMessageStage.BeforeDeserialize);
				SoapExtension.ExecuteProcessMessage (extensions, message, false);
			}
			
			// Deserialize the response

			StreamReader reader = new StreamReader (stream, encoding, false);
			XmlTextReader xml_reader = new XmlTextReader (reader);

			bool isSuccessful = (code != HttpStatusCode.InternalServerError);
			SoapHeaderCollection headers;
			object content;

			if (isSuccessful) {
				WebServiceHelper.ReadSoapMessage (xml_reader, type_info, msi.ResponseSerializer, out content, out headers);
				message.OutParameters = (object[]) content;
			}
			else {
				WebServiceHelper.ReadSoapMessage (xml_reader, type_info, type_info.FaultSerializer, out content, out headers);
				Fault fault = (Fault) content;
				SoapException ex = new SoapException (fault.faultstring, fault.faultcode, fault.faultactor, fault.detail);
				message.SetException (ex);
			}

			message.SetHeaders (headers);
			message.UpdateHeaderValues (this, message.MethodStubInfo.Headers);

			if (extensions != null) {
				message.SetStage (SoapMessageStage.AfterDeserialize);
				SoapExtension.ExecuteProcessMessage (extensions, message, false);
			}

			if (isSuccessful)
				return message.OutParameters;
			else
				throw message.Exception;
		}

		protected object[] Invoke (string method_name, object[] parameters)
		{
			MethodStubInfo msi = type_info.GetMethod (method_name);
			
			SoapClientMessage message = new SoapClientMessage (
				this, msi, Url, parameters);

			SoapExtension[] extensions = SoapExtension.CreateExtensionChain (type_info.SoapExtensions[0], msi.SoapExtensions, type_info.SoapExtensions[1]);

			WebResponse response;
			try
			{
				WebRequest request = GetWebRequest (uri);
				SendRequest (request, message, extensions);
				response = GetWebResponse (request);
			}
			catch (WebException ex)
			{
				response = ex.Response;
				HttpWebResponse http_response = response as HttpWebResponse;
				if (http_response == null || http_response.StatusCode != HttpStatusCode.InternalServerError)
					throw ex;
			}

			return ReceiveResponse (response, message, extensions);
		}

		#endregion // Methods
	}
}
