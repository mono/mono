//
// System.ServiceModel.EndpointAddress.cs
//
// Author: Duncan Mak (duncan@novell.com)
//	   Atsushi Enomoto (atsushi@ximian.com)
//
// Copyright (C) 2005-2006 Novell, Inc (http://www.novell.com)
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
using System.Reflection;
using System.Resources;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
#if !MOBILE
using System.Security.Cryptography.Xml;
#endif

namespace System.ServiceModel
{
	public class EndpointAddress
	{
		static readonly Uri w3c_anonymous = new Uri (Constants.WsaAnonymousUri);
		static readonly Uri anonymous_role = new Uri ("http://schemas.microsoft.com/2005/12/ServiceModel/Addressing/Anonymous");
		static readonly Uri none_role = new Uri ("http://schemas.microsoft.com/2005/12/ServiceModel/Addressing/None");

		public static Uri AnonymousUri {
			get { return anonymous_role; }
		}

		public static Uri NoneUri {
			get { return none_role; }
		}

		Uri address;
		AddressHeaderCollection headers;
		EndpointIdentity identity;
		XmlDictionaryReader metadata_reader;
		XmlDictionaryReader extension_reader;

		static XmlSchema schema;

		public EndpointAddress (string uri)
			: this (new Uri (uri), new AddressHeader [0])
		{
		}

		public EndpointAddress (Uri uri, params AddressHeader [] addressHeaders)
			: this (uri, null, new AddressHeaderCollection (addressHeaders), null, null) {}

		public EndpointAddress (Uri uri, EndpointIdentity identity, params AddressHeader [] addressHeaders)
			: this (uri, identity, new AddressHeaderCollection (addressHeaders), null, null) {}

		public EndpointAddress (Uri uri, EndpointIdentity identity, AddressHeaderCollection headers)
			: this (uri, identity, headers, null, null) {}

		public EndpointAddress (
			Uri uri, EndpointIdentity identity,
			AddressHeaderCollection headers,
			XmlDictionaryReader metadataReader,
			XmlDictionaryReader extensionReader)
		{	
			if (uri == null)
				throw new ArgumentNullException ("uri");
			if (!uri.IsAbsoluteUri)
				throw new ArgumentException ("The argument uri must be absolute");
			this.address = uri;
			this.identity = identity;
			this.headers = headers;
			metadata_reader = metadataReader;
			extension_reader = extensionReader;
		}

		public bool IsAnonymous {
			get { return address.Equals (anonymous_role); }
		}

		public bool IsNone {
			get { return address.Equals (none_role); }
		}

		public AddressHeaderCollection Headers {
			get { return headers; }
		}

		public EndpointIdentity Identity {
			get { return identity; }
		}

		public Uri Uri {
			get { return address; }
		}

#if !MOBILE
		internal static XmlSchema Schema {
			get {
				if (schema == null) {
					Assembly a = Assembly.GetCallingAssembly ();
					Stream s = a.GetManifestResourceStream ("WS-Addressing.schema");
					schema = XmlSchema.Read (s, null);
				}

				return schema;
			}
		}
#endif

		[MonoTODO]
		public void ApplyTo (Message message)
		{
			throw new NotImplementedException ();
		}

		public override bool Equals (object obj)
		{
			EndpointAddress other = obj as EndpointAddress;
			if (other == null || 
			    other.Uri == null || !other.Uri.Equals (this.Uri) ||
			    other.Headers.Count != this.Headers.Count)
				return false;

			foreach (AddressHeader h in this.Headers) {
				bool match = false;
				foreach (AddressHeader o in other.Headers)
					if (h.Equals (o)) {
						match = true;
						break;
					}
				if (!match)
					return false;
			}

			return true;
		}

		public override int GetHashCode ()
		{
			return address.GetHashCode ();
		}

		public XmlDictionaryReader GetReaderAtExtensions ()
		{
			return extension_reader;
		}

		public XmlDictionaryReader GetReaderAtMetadata ()
		{
			return metadata_reader;
		}

		public static bool operator == (EndpointAddress address1, EndpointAddress address2)
		{
			if ((object) address1 == null)
				return (object) address2 == null;
			if ((object) address2 == null)
				return false;
			return address1.Equals (address2);
		}

		public static bool operator != (EndpointAddress address1, EndpointAddress address2)
		{
			return ! (address1 == address2);
		}

//#if !MOBILE
		public static EndpointAddress ReadFrom (
			XmlDictionaryReader reader)
		{
			if (reader == null)
				throw new ArgumentNullException ("reader");

			return ReadFromInternal (null, reader, null, null, null, null);
		}

		public static EndpointAddress ReadFrom (
			AddressingVersion addressingVersion,
			XmlDictionaryReader reader)
		{
			return ReadFrom (addressingVersion, (XmlReader) reader);
		}

		public static EndpointAddress ReadFrom (
			AddressingVersion addressingVersion,
			XmlReader reader)
		{
			if (addressingVersion == null)
				throw new ArgumentNullException ("addressingVersion");
			if (reader == null)
				throw new ArgumentNullException ("reader");

			return ReadFromInternal (addressingVersion, reader, null, null, null, null);
		}

		public static EndpointAddress ReadFrom (
			XmlDictionaryReader reader,
			XmlDictionaryString localName,
			XmlDictionaryString ns)
		{
			return ReadFrom (AddressingVersion.WSAddressing10,
					 reader, localName, ns);
		}

		public static EndpointAddress ReadFrom (
			AddressingVersion addressingVersion,
			XmlDictionaryReader reader,
			XmlDictionaryString localName,
			XmlDictionaryString ns)
		{
			// Empty localName and ns will be rejected by ReadStartElement() by feeding empty strings.
			return ReadFromInternal (addressingVersion, reader, null, null, localName ?? XmlDictionaryString.Empty, ns ?? XmlDictionaryString.Empty);
		}

		public static EndpointAddress ReadFrom (
			AddressingVersion addressingVersion,
			XmlReader reader, string localName, string ns)
		{
			// Empty localName and ns will be rejected by ReadStartElement() by feeding empty strings.
			return ReadFromInternal (addressingVersion, reader, localName ?? String.Empty, ns ?? String.Empty, null, null);
		}

		private static EndpointAddress ReadFromInternal (
			AddressingVersion addressingVersion,
			XmlReader reader, string localName, string ns,
			XmlDictionaryString dictLocalName,
			XmlDictionaryString dictNS)
		{
			reader.MoveToContent ();
			if (reader.NodeType != XmlNodeType.Element ||
			    reader.IsEmptyElement)
				throw new ArgumentException ("Cannot detect appropriate WS-Addressing Address element.");

			if (localName != null)
				reader.ReadStartElement (localName, ns);
			else if (dictLocalName != null)
				((XmlDictionaryReader) reader).ReadStartElement (dictLocalName, dictNS);
			else
				reader.ReadStartElement ();
			reader.MoveToContent ();

			if (addressingVersion == null) {
				if (reader.NamespaceURI == AddressingVersion.WSAddressing10.Namespace)
					addressingVersion = AddressingVersion.WSAddressing10;
				else
				if (reader.NamespaceURI == AddressingVersion.WSAddressingAugust2004.Namespace)
					addressingVersion = AddressingVersion.WSAddressingAugust2004;
				else
					throw new ArgumentException ("Cannot detect appropriate WS-Addressing version.");
			}

			EndpointAddress ea = ReadContents (addressingVersion, reader);

			reader.MoveToContent ();
			reader.ReadEndElement ();
			return ea;
		}
		
		private static EndpointAddress ReadContents (
			AddressingVersion addressingVersion, XmlReader reader)
		{
			Uri uri = null;
			EndpointIdentity identity = null;
			reader.MoveToContent ();
			if (reader.LocalName == "Address" && 
			    reader.NamespaceURI == addressingVersion.Namespace &&
			    reader.NodeType == XmlNodeType.Element &&
			    !reader.IsEmptyElement)
				uri = new Uri (reader.ReadElementContentAsString ());
			else
				throw new XmlException (String.Format (
					"Expecting 'Address' from namespace '{0}', but found '{1}' from namespace '{2}'",
					addressingVersion.Namespace, reader.LocalName, reader.NamespaceURI));

			reader.MoveToContent ();
#if !MOBILE
			MetadataSet metadata = null;
			if (reader.LocalName == "Metadata" &&
			    reader.NamespaceURI == addressingVersion.Namespace &&
			    !reader.IsEmptyElement) {
				reader.Read ();
				metadata = (MetadataSet) new XmlSerializer (typeof (MetadataSet)).Deserialize (reader);
				reader.MoveToContent ();
				reader.ReadEndElement ();
			}
			reader.MoveToContent ();
			if (reader.LocalName == "Identity" &&
			    reader.NamespaceURI == Constants.WsaIdentityUri) {
				// FIXME: implement
				reader.Skip ();
			}
#endif

			if (addressingVersion == AddressingVersion.WSAddressing10 && uri == w3c_anonymous)
				uri = anonymous_role;

#if MOBILE
			return new EndpointAddress (uri, identity);
#else
			if (metadata == null)
				return new EndpointAddress (uri, identity);
			return new EndpointAddress (uri, identity,
				AddressHeader.CreateAddressHeader (metadata));
#endif
		}

		public override string ToString ()
		{
			return address.ToString (); 
		}

		public void WriteContentsTo (
			AddressingVersion addressingVersion,
			XmlDictionaryWriter writer)
		{
			if (writer == null)
				throw new ArgumentNullException ("writer");
#if MOBILE
			if (addressingVersion == AddressingVersion.None) {
				writer.WriteString (Uri.AbsoluteUri);
			} else {
				writer.WriteStartElement ("Address", addressingVersion.Namespace);
				writer.WriteString (Uri.AbsoluteUri);
				writer.WriteEndElement ();
			}
#else
			if (addressingVersion == AddressingVersion.None)
				writer.WriteString (Uri.AbsoluteUri);
			else {
				writer.WriteStartElement ("Address", addressingVersion.Namespace);
				writer.WriteString (Uri.AbsoluteUri);
				writer.WriteEndElement ();

				if (Identity == null)
					return;

				if (Headers != null)
					foreach (AddressHeader ah in Headers)
						ah.WriteAddressHeader (writer);

				writer.WriteStartElement ("Identity", Constants.WsaIdentityUri);

				X509CertificateEndpointIdentity x509 =
					Identity as X509CertificateEndpointIdentity;
				if (x509 != null) {
					KeyInfo ki = new KeyInfo ();
					KeyInfoX509Data x = new KeyInfoX509Data ();
					foreach (X509Certificate2 cert in x509.Certificates)
						x.AddCertificate (cert);
					ki.AddClause (x);
					ki.GetXml ().WriteTo (writer);
				} else {
					DataContractSerializer ds = new DataContractSerializer (Identity.IdentityClaim.GetType ());
					ds.WriteObject (writer, Identity.IdentityClaim);
				}
				writer.WriteEndElement ();
			}
#endif
		}

		public void WriteContentsTo (
			AddressingVersion addressingVersion,
			XmlWriter writer)
		{
			WriteContentsTo (addressingVersion,
				XmlDictionaryWriter.CreateDictionaryWriter (writer));
		}

		public void WriteTo (
			AddressingVersion addressingVersion,
			XmlDictionaryWriter writer)
		{
			WriteTo (addressingVersion, writer, "EndpointReference", addressingVersion.Namespace);
		}

		public void WriteTo (
			AddressingVersion addressingVersion, XmlWriter writer)
		{
			WriteTo (addressingVersion,
				XmlDictionaryWriter.CreateDictionaryWriter (writer));
		}

		public void WriteTo (
			AddressingVersion addressingVersion,
			XmlDictionaryWriter writer,
			XmlDictionaryString localName,
			XmlDictionaryString ns)
		{
			writer.WriteStartElement (localName, ns);
			WriteContentsTo (addressingVersion, writer);
			writer.WriteEndElement ();
		}

		public void WriteTo (
			AddressingVersion addressingVersion,
			XmlWriter writer, string localName, string ns)
		{
			writer.WriteStartElement (localName, ns);
			WriteContentsTo (addressingVersion, writer);
			writer.WriteEndElement ();
		}
	}
}
