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

#if USE_DEPRECATED

using System;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Web.Services.Protocols;
using System.Web.Services.Description;
using System.Collections.Generic;

using System.ServiceModel.Channels;
using System.ServiceModel.Description;

namespace System.ServiceModel.Description
{
	//Original file: WebServiceHelper.cs
	internal class WebServiceHelper
	{
		public const string SoapEnvelopeNamespace = "http://www.w3.org/2003/05/soap-envelope";
		public const string AddressingNamespace = "http://www.w3.org/2005/08/addressing";

		public static MetadataSet ReadTransferGetResponse (XmlTextReader xmlReader, SoapHeaderDirection dir, 
				out SoapHeaderCollection headers, out AddressHeaderCollection addressHeaders)
		{
			SoapReflectionImporter sri = new SoapReflectionImporter ();
			XmlSerializer bodySerializer = new XmlSerializer (sri.ImportTypeMapping (typeof (SoapHeader)));
		
			xmlReader.MoveToContent ();
			xmlReader.ReadStartElement ("Envelope", WebServiceHelper.SoapEnvelopeNamespace);

			headers = ReadHeaders (xmlReader, out addressHeaders);
			xmlReader.MoveToContent ();

			xmlReader.ReadStartElement ("Body", WebServiceHelper.SoapEnvelopeNamespace);
			
			/*if (xmlReader.LocalName == "Fault" && xmlReader.NamespaceURI == SoapEnvelopeNamespace)
				bodySerializer = Fault.Serializer;

			body = bodySerializer.Deserialize (xmlReader);*/

			MetadataSet ms = MetadataSet.ReadFrom (xmlReader);
			return ms;
		}

		static SoapHeaderCollection ReadHeaders (XmlTextReader xmlReader, out AddressHeaderCollection addressHeaders)
		{
			SoapHeaderCollection headers = new SoapHeaderCollection ();
			List<AddressHeader> addressList = new List<AddressHeader> ();
			
			xmlReader.Read ();
			while (! (xmlReader.NodeType == XmlNodeType.Element && xmlReader.LocalName == "Body" && 
						xmlReader.NamespaceURI == SoapEnvelopeNamespace))
			{
				/* FIXME: Use some deserializer for AddressHeaders,
				 * EndpointReference is a complex header..
				 * Validate 'Action', 'RelatesTo'
				 */
				if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.NamespaceURI == AddressingNamespace && 
					!xmlReader.IsEmptyElement)
				{
					//WS-Addressing headers
					
					/* FIXME: attributes? 
					while (xmlReader.MoveToNextAttribute ()) {
					}*/

					xmlReader.ReadStartElement ();
					xmlReader.MoveToContent ();
					string content = xmlReader.ReadContentAsString ();
					
					//FIXME: Use AddressHeaderSerializer
					AddressHeader ah = AddressHeader.CreateAddressHeader (xmlReader.Name, AddressingNamespace, 
							content);

					addressList.Add (ah);
					xmlReader.ReadEndElement ();
				}
				else
					//FIXME: Other headers?
					xmlReader.Skip ();
			}

			addressHeaders = new AddressHeaderCollection (addressList);
			
			return headers;
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
				SoapUnknownHeader su = new SoapUnknownHeader ();
				su.Element = e.Element;
				headers.Add (su);
			}
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

#endif
