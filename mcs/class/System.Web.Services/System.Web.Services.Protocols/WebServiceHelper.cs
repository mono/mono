//
// System.Web.Services.Protocols.WebServiceHelper.cs
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
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Web.Services.Description;

namespace System.Web.Services.Protocols
{
	internal class WebServiceHelper
	{
		public const string SoapEnvelopeNamespace = "http://schemas.xmlsoap.org/soap/envelope/";
		public const string Soap12EnvelopeNamespace = "http://www.w3.org/2003/05/soap-envelope";
		public const string SoapEncodingNamespace = "http://schemas.xmlsoap.org/soap/encoding/";
		public const string Soap12EncodingNamespace = "http://www.w3.org/2003/05/soap-encoding";
		static readonly char [] trimChars = { '"', '\'' };
		static readonly bool prettyXml;
		
		static WebServiceHelper ()
		{
			string pxml = Environment.GetEnvironmentVariable ("MONO_WEBSERVICES_PRETTYXML");
			prettyXml = (pxml != null && pxml != "no");
		}
		
		public static XmlTextWriter CreateXmlWriter (Stream s)
		{
			// What a waste of UTF8encoders, but it has to be thread safe.
			XmlTextWriter xtw = new XmlTextWriter (s, new UTF8Encoding (false));
				
			if (prettyXml)
				xtw.Formatting = Formatting.Indented;
				
			return xtw;
		}
		
		public static Encoding GetContentEncoding (string cts, out string content_type)
		{
			string encoding;

			if (cts == null) cts = "";
			
			encoding = "utf-8";
			int start = 0;
			int idx = cts.IndexOf (';');
			if (idx == -1)
				content_type = cts;
			else
				content_type = cts.Substring (0, idx);

			content_type = content_type.Trim ();
			for (start = idx + 1; idx != -1;)
			{
				idx = cts.IndexOf (';', start);
				string body;
				if (idx == -1)
					body = cts.Substring (start);
				else 
				{
					body = cts.Substring (start, idx - start);
					start = idx + 1;
				}
				body = body.Trim ();
				if (String.CompareOrdinal (body, 0, "charset=", 0, 8) == 0)
				{
					encoding = body.Substring (8);
					encoding = encoding.TrimStart (trimChars).TrimEnd (trimChars);
				}
			}

			return Encoding.GetEncoding (encoding);
		}

		public static string GetContextAction (string cts) {
			if (cts == null || cts.Length == 0)
				return null;

			int start = 0;
			int idx = cts.IndexOf (';');
			for (start = idx + 1; idx != -1;)
			{
				idx = cts.IndexOf (';', start);
				string body;
				if (idx == -1)
					body = cts.Substring (start);
				else 
				{
					body = cts.Substring (start, idx - start);
					start = idx + 1;
				}
				body = body.Trim ();
				string actionEq = "action=";
				if (String.CompareOrdinal(body, 0, actionEq, 0, actionEq.Length) == 0)
				{
					string action = body.Substring (actionEq.Length);
					return action.Trim (trimChars);
				}
			}


			return null;
		}

		public static void WriteSoapMessage (XmlTextWriter xtw, SoapMethodStubInfo method, SoapHeaderDirection dir, object bodyContent, SoapHeaderCollection headers, bool soap12)
		{
			SoapBindingUse methodUse = dir == SoapHeaderDirection.Fault ? SoapBindingUse.Literal : method.Use;
			XmlSerializer bodySerializer = method.GetBodySerializer (dir, soap12);
			XmlSerializer headerSerializer = method.GetHeaderSerializer (dir);
			object[] headerArray = method.GetHeaderValueArray (dir, headers);
			WriteSoapMessage (xtw, methodUse, bodySerializer, headerSerializer, bodyContent, headerArray, soap12);
		}
		
		public static void WriteSoapMessage (XmlTextWriter xtw, SoapBindingUse methodUse, XmlSerializer bodySerializer, XmlSerializer headerSerializer, object bodyContent, object[] headers, bool soap12)
		{
			string ns = soap12 ?
				WebServiceHelper.Soap12EnvelopeNamespace :
				WebServiceHelper.SoapEnvelopeNamespace;
			string encNS = soap12 ?
				WebServiceHelper.Soap12EncodingNamespace :
				WebServiceHelper.SoapEncodingNamespace;
			xtw.WriteStartDocument ();
			xtw.WriteStartElement ("soap", "Envelope", ns);
			xtw.WriteAttributeString ("xmlns", "xsi", null, XmlSchema.InstanceNamespace);
			xtw.WriteAttributeString ("xmlns", "xsd", null, XmlSchema.Namespace);

			// Serialize headers
			if (headers != null)
			{
				xtw.WriteStartElement ("soap", "Header", ns);
				headerSerializer.Serialize (xtw, headers);
				xtw.WriteEndElement ();
			}

			// Serialize body
			xtw.WriteStartElement ("soap", "Body", ns);
			
			if (methodUse == SoapBindingUse.Encoded)
				xtw.WriteAttributeString ("encodingStyle", ns, encNS);
				
			bodySerializer.Serialize (xtw, bodyContent);

			xtw.WriteEndElement ();
			xtw.WriteEndElement ();
			xtw.Flush ();
		}

		public static void ReadSoapMessage (XmlTextReader xmlReader, SoapMethodStubInfo method, SoapHeaderDirection dir, bool soap12, out object body, out SoapHeaderCollection headers)
		{
			XmlSerializer bodySerializer = method.GetBodySerializer (dir, false);// no need to worry about soap12 arg since no call for Fault anyways here.
			XmlSerializer headerSerializer = method.GetHeaderSerializer (dir);
			ReadSoapMessage (xmlReader, bodySerializer, headerSerializer, soap12, out body, out headers);
		}
		
		public static void ReadSoapMessage (XmlTextReader xmlReader, XmlSerializer bodySerializer, XmlSerializer headerSerializer, bool soap12, out object body, out SoapHeaderCollection headers)
		{
			xmlReader.MoveToContent ();
			string ns = xmlReader.NamespaceURI;
			switch (ns) {
#if NET_2_0
			case WebServiceHelper.Soap12EnvelopeNamespace:
#endif
			case WebServiceHelper.SoapEnvelopeNamespace:
				break;
			default:
				throw new SoapException (String.Format ("SOAP version mismatch. Namespace '{0}' is not supported in this runtime profile.", ns), VersionMismatchFaultCode (soap12));
			}
			xmlReader.ReadStartElement ("Envelope", ns);

			headers = ReadHeaders (xmlReader, headerSerializer, ns);

			xmlReader.MoveToContent ();
			xmlReader.ReadStartElement ("Body", ns);
			xmlReader.MoveToContent ();
			
			if (xmlReader.LocalName == "Fault" && xmlReader.NamespaceURI == ns)
				bodySerializer = ns == Soap12EnvelopeNamespace ? Soap12Fault.Serializer : Fault.Serializer;

			body = bodySerializer.Deserialize (xmlReader);
		}

		static SoapHeaderCollection ReadHeaders (XmlTextReader xmlReader, XmlSerializer headerSerializer, string ns)
		{
			SoapHeaderCollection headers = null;
			while (! (xmlReader.NodeType == XmlNodeType.Element && xmlReader.LocalName == "Body" && xmlReader.NamespaceURI == ns))
			{
				if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.LocalName == "Header" 
				    && xmlReader.NamespaceURI == ns && !xmlReader.IsEmptyElement
				    && headerSerializer != null)
				{
					xmlReader.ReadStartElement ();
					xmlReader.MoveToContent ();
					
					HeaderSerializationHelper uh = new HeaderSerializationHelper (headerSerializer);
					headers = uh.Deserialize (xmlReader);
					
					while (xmlReader.NodeType != XmlNodeType.EndElement)
						xmlReader.Skip ();
						
					xmlReader.ReadEndElement ();
				}
				else
					xmlReader.Skip ();
			}
			if (headers != null)
				return headers;
			else
				return new SoapHeaderCollection ();
		}
		
		class HeaderSerializationHelper
		{
			SoapHeaderCollection headers;
			XmlSerializer headerSerializer;
			
			public HeaderSerializationHelper (XmlSerializer headerSerializer)
			{
				this.headers = new SoapHeaderCollection ();
				this.headerSerializer = headerSerializer;
			}
			
			public SoapHeaderCollection Deserialize (XmlTextReader xmlReader)
			{
				try {
					headerSerializer.UnknownElement += new XmlElementEventHandler (OnAddUnknownHeader);
					object[] headerArray = (object[]) headerSerializer.Deserialize (xmlReader);
					foreach (SoapHeader h in headerArray)
						if (h != null) headers.Add (h);
					return headers;
				} finally {
					headerSerializer.UnknownElement -= new XmlElementEventHandler (OnAddUnknownHeader);
				}
			}
			
			void OnAddUnknownHeader (object sender, XmlElementEventArgs e)
			{
				headers.Add (new SoapUnknownHeader (e.Element));
			}
		}

#if NET_2_0
		public static SoapException Soap12FaultToSoapException (Soap12Fault fault)
		{
			Soap12FaultReasonText text =
				fault.Reason != null &&
				fault.Reason.Texts != null &&
				fault.Reason.Texts.Length > 0 ?
				fault.Reason.Texts [fault.Reason.Texts.Length - 1] : null;
			XmlNode detail = (fault.Detail == null) ? null :
				(fault.Detail.Children != null &&
				fault.Detail.Children.Length > 0) ?
				(XmlNode) fault.Detail.Children [0] :
				(fault.Detail.Attributes != null &&
				fault.Detail.Attributes.Length > 0) ?
				fault.Detail.Attributes [0] : null;
			SoapFaultSubCode subcode = Soap12Fault.GetSoapFaultSubCode (fault.Code.Subcode);
			return new SoapException (
				text != null ? text.Value : null,
				fault.Code.Value, null, fault.Role,
				text != null ? text.XmlLang : null,
				detail, subcode, null);
		}
#endif

		public static XmlQualifiedName ClientFaultCode (bool soap12)
		{
#if NET_2_0
			return soap12 ? Soap12FaultCodes.SenderFaultCode : SoapException.ClientFaultCode;
#else
			return SoapException.ClientFaultCode;
#endif
		}

		public static XmlQualifiedName ServerFaultCode (bool soap12)
		{
#if NET_2_0
			return soap12 ? Soap12FaultCodes.ReceiverFaultCode : SoapException.ServerFaultCode;
#else
			return SoapException.ServerFaultCode;
#endif
		}

		public static XmlQualifiedName MustUnderstandFaultCode (bool soap12)
		{
#if NET_2_0
			return soap12 ? Soap12FaultCodes.ReceiverFaultCode : SoapException.MustUnderstandFaultCode;
#else
			return SoapException.MustUnderstandFaultCode;
#endif
		}

		public static XmlQualifiedName VersionMismatchFaultCode (bool soap12)
		{
#if NET_2_0
			return soap12 ? Soap12FaultCodes.VersionMismatchFaultCode : SoapException.VersionMismatchFaultCode;
#else
			return SoapException.VersionMismatchFaultCode;
#endif
		}

		public static void InvalidOperation (string message, WebResponse response, Encoding enc)
		{
			if (response == null)
				throw new InvalidOperationException (message);

			if (enc == null)
				enc = Encoding.UTF8;

			StringBuilder sb = new StringBuilder ();
			sb.Append (message);
			if (response.ContentLength > 0) {
				sb.Append ("\r\nResponse error message:\r\n--\r\n");

				try {
					StreamReader resp = new StreamReader (response.GetResponseStream (), enc);
					sb.Append (resp.ReadToEnd ());
				} catch (Exception) {
				}
			}

			throw new InvalidOperationException (sb.ToString ());
		}
	}
}
