//
// System.Web.Services.Protocols.WebServiceHelper.cs
//
// Author:
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) Ximian, Inc. 2003
//

using System;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace System.Web.Services.Protocols
{
	internal class WebServiceHelper
	{
		public const string SoapEnvelopeNamespace = "http://schemas.xmlsoap.org/soap/envelope/";

		public static Encoding GetContentEncoding (string cts)
		{
			string encoding;
			string content_type;

			encoding = "utf-8";
			int start = 0;
			int idx = cts.IndexOf (';');
			if (idx == -1)
				encoding = cts;
			content_type = cts.Substring (0, idx);
			for (start = idx + 1; idx != -1;)
			{
				idx = cts.IndexOf (";", start);
				string body;
				if (idx == -1)
					body = cts.Substring (start);
				else 
				{
					body = cts.Substring (start, idx);
					start = idx + 1;
				}
				if (body.StartsWith ("charset="))
				{
					encoding = body.Substring (8);
				}
			}

			if (content_type != "text/xml")
				throw new Exception ("Content is not XML: " + content_type);

			return Encoding.GetEncoding (encoding);
		}

		public static void WriteSoapMessage (XmlTextWriter xtw, TypeStubInfo info, XmlSerializer bodySerializer, object bodyContent, SoapHeaderCollection headers)
		{
			xtw.WriteStartDocument ();
			xtw.WriteStartElement ("soap", "Envelope", WebServiceHelper.SoapEnvelopeNamespace);
			xtw.WriteAttributeString ("xmlns", "xsi", null, XmlSchema.InstanceNamespace);
			xtw.WriteAttributeString ("xmlns", "xsd", null, XmlSchema.Namespace);

			// Serialize headers
			if (headers != null)
			{
				foreach (SoapHeader header in headers) 
				{
					XmlSerializer ser = info.GetHeaderSerializer (header.GetType());
					xtw.WriteStartElement ("soap", "Header", WebServiceHelper.SoapEnvelopeNamespace);
					ser.Serialize (xtw, header);
					xtw.WriteEndElement ();
				}
			}

			// Serialize body
			xtw.WriteStartElement ("soap", "Body", WebServiceHelper.SoapEnvelopeNamespace);
			bodySerializer.Serialize (xtw, bodyContent);

			xtw.WriteEndElement ();
			xtw.WriteEndElement ();
			xtw.Flush ();
		}

		public static void ReadSoapMessage (XmlTextReader xmlReader, TypeStubInfo typeStubInfo, XmlSerializer bodySerializer, out object body, out SoapHeaderCollection headers)
		{
			xmlReader.MoveToContent ();
			xmlReader.ReadStartElement ("Envelope", WebServiceHelper.SoapEnvelopeNamespace);

			headers = ReadHeaders (typeStubInfo, xmlReader);

			xmlReader.MoveToContent ();
			xmlReader.ReadStartElement ("Body", WebServiceHelper.SoapEnvelopeNamespace);
			xmlReader.MoveToContent ();

			body = bodySerializer.Deserialize (xmlReader);
		}

		static SoapHeaderCollection ReadHeaders (TypeStubInfo typeStubInfo, XmlTextReader xmlReader)
		{
			SoapHeaderCollection headers = new SoapHeaderCollection ();
			while (! (xmlReader.NodeType == XmlNodeType.Element && xmlReader.LocalName == "Body" && xmlReader.NamespaceURI == WebServiceHelper.SoapEnvelopeNamespace))
			{
				if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.LocalName == "Header" && xmlReader.NamespaceURI == WebServiceHelper.SoapEnvelopeNamespace)
				{
					xmlReader.ReadStartElement ();
					xmlReader.MoveToContent ();
					XmlQualifiedName qname = new XmlQualifiedName (xmlReader.LocalName, xmlReader.NamespaceURI);
					XmlSerializer headerSerializer = typeStubInfo.GetHeaderSerializer (qname);
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
