//
// KeyIdentifier.cs: Handles WS-Security KeyIdentifier
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.Security.Cryptography.Xml;
using System.Xml;
using Microsoft.Web.Services;
using Microsoft.Web.Services.Security.X509;
#if !WSE1
using Microsoft.Web.Services.Xml;
#endif

namespace Microsoft.Web.Services.Security {

	public class KeyIdentifier : IXmlElement {

		private byte[] kivalue;
		private XmlQualifiedName vtype;
		static private char[] separator = { ':' };

		public KeyIdentifier (byte[] identifier) 
		{
			if (identifier == null)
				throw new ArgumentNullException ("identifier");
			kivalue = (byte[]) identifier.Clone ();
		}

		public KeyIdentifier (XmlElement element) 
		{
			LoadXml (element);
		}

		public KeyIdentifier (byte[] identifier, XmlQualifiedName valueType) 
		{
			if (identifier == null)
				throw new ArgumentNullException ("identifier");
			kivalue = (byte[]) identifier.Clone ();
			vtype = valueType;
		}

		public byte[] Value {
			get { return (byte[]) kivalue.Clone (); }
			set {
				if (value == null)
					throw new ArgumentNullException ("value");
				kivalue = value;
			}
		}

		public XmlQualifiedName ValueType {
			get { return vtype; }
			set { vtype = value; }
		}

		public XmlElement GetXml (XmlDocument document) 
		{
			if (document == null)
				throw new ArgumentNullException ("document");

			XmlElement ki = document.CreateElement (WSSecurity.Prefix, WSSecurity.ElementNames.KeyIdentifier, WSSecurity.NamespaceURI);
			ki.InnerText = Convert.ToBase64String (kivalue);
			if ((vtype != null) && (!vtype.IsEmpty)) {
				string ns = ki.GetPrefixOfNamespace (vtype.Namespace);
				if ((ns == null) || (ns == String.Empty)) {
					ns = "vt";
					XmlAttribute nsa = document.CreateAttribute ("xmlns:vt");
					nsa.InnerText = vtype.Namespace;
					ki.Attributes.Append (nsa);
				}
				XmlAttribute vt = document.CreateAttribute (WSSecurity.AttributeNames.ValueType);
				vt.InnerText = String.Concat (ns, ":", vtype.Name);
				ki.Attributes.Append (vt);
			}
			return ki;
		}

		public void LoadXml (XmlElement element) 
		{
			if (element == null)
				throw new ArgumentNullException ("element");

			if ((element.LocalName != WSSecurity.ElementNames.KeyIdentifier) || (element.NamespaceURI != WSSecurity.NamespaceURI))
				throw new ArgumentException ("invalid LocalName or NamespaceURI");

			try {
				kivalue = Convert.FromBase64String (element.InnerText);
			}
			catch {
				kivalue = null;
			}

			XmlAttribute vt = element.Attributes [WSSecurity.AttributeNames.ValueType];
			if (vt != null) {
				string[] nsvt = vt.InnerText.Split (separator);
				switch (nsvt.Length) {
					case 2:
						string ns = element.GetNamespaceOfPrefix (nsvt [0]);
						vtype = new XmlQualifiedName (nsvt [1], ns);
						break;
					default:
						throw new SecurityFormatException ("missing namespace");
				}
			}
		}

		internal X509Certificate Certificate {
			get {
				if ((vtype.Name == "X509v3") && (vtype.Namespace == WSSecurity.NamespaceURI)) {
					// TODO - use microsoft.web.service config in .exe.config for store location
					X509CertificateStore store = X509CertificateStore.LocalMachineStore (X509CertificateStore.MyStore);
					if (store.OpenRead ()) {
						X509CertificateCollection coll = store.FindCertificateByKeyIdentifier (kivalue);
						if ((coll != null) && (coll.Count > 0)) {
							return coll [0];
						}
						store.Close ();
					}
				}
				return null;
			}
		}

		internal DecryptionKey DecryptionKey {
			get {
				X509Certificate x509 = Certificate;
				if (x509 != null) {
					return new AsymmetricDecryptionKey (x509.Key);
				}
				return null;
			}
		}

		internal EncryptionKey EncryptionKey {
			get {
				X509Certificate x509 = Certificate;
				if (x509 != null) {
					return new AsymmetricEncryptionKey (x509.PublicKey);
				}
				return null;
			}
		}
	}
}
