//
// DataObject.cs - DataObject implementation for XML Signature
// http://www.w3.org/2000/09/xmldsig#Object
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System.Xml;

namespace System.Security.Cryptography.Xml {

	// XmlElement part of the signature
	// Note: Looks like KeyInfoNode (but the later is XmlElement inside KeyInfo)
	// required for "enveloping signatures"
	public class DataObject {

		private string id;
		private string mimeType;
		private string encoding;
		private XmlDocument document;

		public DataObject ()
		{
			Build (null, null, null, null);
		}

		public DataObject (string id, string mimeType, string encoding, XmlElement data) 
		{
			if (data == null)
				throw new ArgumentNullException ("data");

			Build (id, mimeType, encoding, data);
		}

		// this one accept a null "data" parameter
		private void Build (string id, string mimeType, string encoding, XmlElement data) 
		{
			document = new XmlDocument ();
			XmlElement xel = document.CreateElement (XmlSignature.ElementNames.Object, XmlSignature.NamespaceURI);
			if (id != null) {
				this.id = id;
				xel.SetAttribute (XmlSignature.AttributeNames.Id, id);
			}
			if (mimeType != null) {
				this.mimeType = mimeType;
				xel.SetAttribute (XmlSignature.AttributeNames.MimeType, mimeType);
			}
			if (encoding != null) {
				this.encoding = encoding;
				xel.SetAttribute (XmlSignature.AttributeNames.Encoding, encoding);
			}
			if (data != null) {
				XmlNode newNode = document.ImportNode (data, true);
				xel.AppendChild (newNode);
			}
			document.AppendChild (xel);
		}

		// why is data a XmlNodeList instead of a XmlElement ?
		public XmlNodeList Data {
			get { 
				XmlNodeList xnl = document.GetElementsByTagName (XmlSignature.ElementNames.Object);
				return xnl[0].ChildNodes;
			}
			set {
				if (value == null)
					throw new ArgumentNullException ("value");

				Build (id, mimeType, encoding, null);
				XmlNodeList xnl = document.GetElementsByTagName (XmlSignature.ElementNames.Object);
				if ((xnl != null) && (xnl.Count > 0)) {
					foreach (XmlNode xn in value) {
						XmlNode newNode = document.ImportNode (xn, true);
						xnl [0].AppendChild (newNode);
					}
				}
			}
		}

		// default to null - no encoding
		public string Encoding {
			get { return encoding; }
			set { encoding = value;	}
		}

		// default to null
		public string Id {
			get { return id; }
			set { id = value; }
		}

		// default to null
		public string MimeType {
			get { return mimeType; }
			set { mimeType = value; }
		}

		public XmlElement GetXml () 
		{
			if ((document.DocumentElement.LocalName == XmlSignature.ElementNames.Object) && (document.DocumentElement.NamespaceURI == XmlSignature.NamespaceURI)) {
				// recreate all attributes in order
				XmlAttribute xa = null;
				document.DocumentElement.Attributes.RemoveAll ();
				if (id != null) {
					xa = document.CreateAttribute (XmlSignature.AttributeNames.Id);
					xa.Value = id;
					document.DocumentElement.Attributes.Append (xa);
				}
				if (mimeType != null) {
					xa = document.CreateAttribute (XmlSignature.AttributeNames.MimeType);
					xa.Value = mimeType;
					document.DocumentElement.Attributes.Append (xa);
				}
				if (encoding != null) {
					xa = document.CreateAttribute (XmlSignature.AttributeNames.Encoding);
					xa.Value = encoding;
					document.DocumentElement.Attributes.Append (xa);
				}
				xa = document.CreateAttribute ("xmlns");
				xa.Value = XmlSignature.NamespaceURI;
				document.DocumentElement.Attributes.Append (xa);
			}
			return document.DocumentElement;
		}

		public void LoadXml (XmlElement value) 
		{
			if (value == null)
				throw new ArgumentNullException ("value");

			if ((value.LocalName != XmlSignature.ElementNames.Object) || (value.NamespaceURI != XmlSignature.NamespaceURI)) {
				document.LoadXml (value.OuterXml);
			}
			else {
				document.LoadXml (value.OuterXml);
				XmlAttribute xa = value.Attributes [XmlSignature.AttributeNames.Id];
				id = ((xa != null) ? xa.InnerText : null);
				xa = value.Attributes [XmlSignature.AttributeNames.MimeType];
				mimeType = ((xa != null) ? xa.InnerText : null);
				xa = value.Attributes [XmlSignature.AttributeNames.Encoding];
				encoding = ((xa != null) ? xa.InnerText : null);
			}
		}
	}
}