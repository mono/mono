// 
// System.Web.Services.Protocols.SoapHttpClientProtocol.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//   Miguel de Icaza (miguel@ximian.com)
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
			WebRequest request = WebRequest.Create (uri);
			request.Method = "POST";

			return request;
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
		
		const string soap_envelope = "http://schemas.xmlsoap.org/soap/envelope/";
		
		void WriteSoapEnvelope (XmlTextWriter xtw)
		{
			xtw.WriteStartDocument ();
			
			xtw.WriteStartElement ("soap", "Envelope", soap_envelope);
			xtw.WriteAttributeString ("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");
			xtw.WriteAttributeString ("xmlns", "xsd", null, "http://www.w3.org/2001/XMLSchema");
		}
		
		void SendRequest (WebRequest request, SoapClientMessage message)
		{
			WebHeaderCollection headers = request.Headers;
			headers.Add ("SOAPAction", message.Action);

			request.ContentType = message.ContentType + "; charset=utf-8";

			using (Stream s = request.GetRequestStream ()){
				// serialize arguments

				// What a waste of UTF8encoders, but it has to be thread safe.
				
				XmlTextWriter xtw = new XmlTextWriter (s, new UTF8Encoding (false));
				xtw.Formatting = Formatting.Indented;
				WriteSoapEnvelope (xtw);
				xtw.WriteStartElement ("soap", "Body", soap_envelope);

				// Serialize arguments.
				message.MethodStubInfo.RequestSerializer.Serialize (xtw, message.Parameters);
				xtw.WriteEndElement ();
				xtw.WriteEndElement ();
				
				xtw.Flush ();
				xtw.Close ();
			 }
		}

		void GetContentTypeProperties (string cts, out string encoding, out string content_type)
		{
			encoding = "utf-8";
			int start = 0;
			int idx = cts.IndexOf (';');
			if (idx == -1)
				encoding = cts;
			content_type = cts.Substring (0, idx);
			for (start = idx + 1; idx != -1;){
				idx = cts.IndexOf (";", start);
				string body;
				if (idx == -1)
					body = cts.Substring (start);
				else {
					body = cts.Substring (start, idx);
					start = idx + 1;
				}
				if (body.StartsWith ("charset=")){
					encoding = body.Substring (8);
				}
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
			
			if (!(code == HttpStatusCode.Accepted || code == HttpStatusCode.OK))
				throw new Exception ("Return code was: " + http_response.StatusCode);

			//
			// Remove optional encoding
			//
			string content_type, encoding_name;
			GetContentTypeProperties (response.ContentType, out encoding_name, out content_type);

			if (content_type != "text/xml")
				throw new Exception ("Return value is not XML: " + content_type);

			Encoding encoding = Encoding.GetEncoding (encoding_name);
			Stream stream = response.GetResponseStream ();
			StreamReader reader = new StreamReader (stream, encoding, false);
			XmlTextReader xml_reader = new XmlTextReader (reader);

			//
			// Handle faults somewhere
			//
			xml_reader.MoveToContent ();
			xml_reader.ReadStartElement ("Envelope", soap_envelope);
			xml_reader.MoveToContent ();
			xml_reader.ReadStartElement ("Body", soap_envelope);

			xml_reader.MoveToContent ();
			xml_reader.ReadStartElement (msi.ResponseName, msi.ResponseNamespace);

			object [] ret = (object []) msi.ResponseSerializer.Deserialize (xml_reader);

			Console.WriteLine ("Returned array elements: " + ret.Length);
			for (int i = 0; i < ret.Length; i++){
				Console.WriteLine ("Value-{0}: {1}", i, ret [i]);
			}
			return (object []) ret;
		}
		
		protected object[] Invoke (string method_name, object[] parameters)
		{
			MethodStubInfo msi = type_info.GetMethod (method_name);
			
			SoapClientMessage message = new SoapClientMessage (
				this, msi, Url, parameters);

			WebRequest request = GetWebRequest (uri);
			SendRequest (request, message);

			WebResponse response = request.GetResponse ();
			return ReceiveResponse (response, message);
		}

		#endregion // Methods
	}
}
