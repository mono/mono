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
		
		void SendRequest (WebRequest request, SoapClientMessage message)
		{
			request.Method = "POST";
			WebHeaderCollection headers = request.Headers;
			headers.Add ("SOAPAction", message.Action);

			request.ContentType = message.ContentType + "; charset=utf-8";

			using (Stream s = request.GetRequestStream ()){

				// What a waste of UTF8encoders, but it has to be thread safe.
				
				XmlTextWriter xtw = new XmlTextWriter (s, new UTF8Encoding (false));
				xtw.Formatting = Formatting.Indented;

				WebServiceHelper.WriteSoapMessage (xtw, type_info, message.MethodStubInfo.RequestSerializer, message.Parameters, message.Headers);
				
				xtw.Flush ();
				xtw.Close ();
			 }
		}


		//
		// TODO:
		//    Handle other web responses (multi-output?)
		//    
		object [] ReceiveResponse (WebResponse response, SoapClientMessage message)

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
			StreamReader reader = new StreamReader (stream, encoding, false);
			XmlTextReader xml_reader = new XmlTextReader (reader);

			xml_reader.MoveToContent ();
			xml_reader.ReadStartElement ("Envelope", WebServiceHelper.SoapEnvelopeNamespace);
			xml_reader.MoveToContent ();
			xml_reader.ReadStartElement ("Body", WebServiceHelper.SoapEnvelopeNamespace);

			if (code != HttpStatusCode.InternalServerError)
			{
				object [] ret = (object []) msi.ResponseSerializer.Deserialize (xml_reader);
				return (object []) ret;
			}
			else
			{
				Fault fault = (Fault) type_info.FaultSerializer.Deserialize (xml_reader);
				throw new SoapException (fault.faultstring, fault.faultcode, fault.faultactor, fault.detail);
			}
		}

		protected object[] Invoke (string method_name, object[] parameters)
		{
			MethodStubInfo msi = type_info.GetMethod (method_name);

			SoapClientMessage message = new SoapClientMessage (
				this, msi, Url, parameters);

			WebResponse response;
			try
			{
				WebRequest request = GetWebRequest (uri);
				SendRequest (request, message);
				response = GetWebResponse (request);
			}
			catch (WebException ex)
			{
				response = ex.Response;
				HttpWebResponse http_response = response as HttpWebResponse;
				if (http_response == null || http_response.StatusCode != HttpStatusCode.InternalServerError)
					throw ex;
			}

			return ReceiveResponse (response, message);
		}

		#endregion // Methods
	}
}
