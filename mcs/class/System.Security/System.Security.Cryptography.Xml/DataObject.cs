//
// DataObject.cs - DataObject implementation for XML Signature
// http://www.w3.org/2000/09/xmldsig#Object
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System.Text;
using System.Xml;

namespace System.Security.Cryptography.Xml {

	// XmlElement part of the signature
	// Note: Looks like KeyInfoNode (but the later is XmlElement inside KeyInfo)
	// required for "enveloping signatures"
	public class DataObject {

		private string id;
		private string mimeType;
		private string encoding;
		private XmlDocument doc;

		static private string xmldsig = "http://www.w3.org/2000/09/xmldsig#";

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

		private void Build (string id, string mimeType, string encoding, XmlElement data) 
		{
			StringBuilder sb = new StringBuilder ();
			sb.Append ("<Object ");
			if (id != null) {
				this.id = id;
				sb.Append ("Id=\"");
				sb.Append (id);
				sb.Append ("\" ");
			}
			if (mimeType != null) {
				this.mimeType = mimeType;
				sb.Append ("MimeType=\"");
				sb.Append (mimeType);
				sb.Append ("\" ");
			}
			if (encoding != null) {
				this.encoding = encoding;
				sb.Append ("Encoding=\"");
				sb.Append (encoding);
				sb.Append ("\" ");
			}
			sb.Append ("xmlns=\"http://www.w3.org/2000/09/xmldsig#\" />");
			
			doc = new XmlDocument ();
			doc.LoadXml (sb.ToString ());
			if (data != null) {
				XmlNodeList xnl = doc.GetElementsByTagName ("Object");
				XmlNode newNode = doc.ImportNode (data, true);
				xnl[0].AppendChild (newNode);
			}
		}

		// why is data a XmlNodeList instead of a XmlElement ?
		public XmlNodeList Data {
			get { 
				XmlNodeList xnl = doc.GetElementsByTagName ("Object");
				return xnl[0].ChildNodes;
			}
			set {
				if (value == null)
					throw new ArgumentNullException ("value");

				Build (id, mimeType, encoding, null);
				XmlNodeList xnl = doc.GetElementsByTagName ("Object");
				if ((xnl != null) && (xnl.Count > 0)) {
					foreach (XmlNode xn in value) {
						XmlNode newNode = doc.ImportNode (xn, true);
						xnl[0].AppendChild (newNode);
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
			if ((doc.DocumentElement.LocalName == "Object") && (doc.DocumentElement.NamespaceURI == xmldsig)) {
				// recreate all attributes in order
				XmlAttribute xa = null;
				doc.DocumentElement.Attributes.RemoveAll ();
				if (id != null) {
					xa = doc.CreateAttribute ("Id");
					xa.Value = id;
					doc.DocumentElement.Attributes.Append (xa);
				}
				if (mimeType != null) {
					xa = doc.CreateAttribute ("MimeType");
					xa.Value = mimeType;
					doc.DocumentElement.Attributes.Append (xa);
				}
				if (encoding != null) {
					xa = doc.CreateAttribute ("Encoding");
					xa.Value = encoding;
					doc.DocumentElement.Attributes.Append (xa);
				}
				xa = doc.CreateAttribute ("xmlns");
				xa.Value = xmldsig;
				doc.DocumentElement.Attributes.Append (xa);
			}
			return doc.DocumentElement;
		}

		public void LoadXml (XmlElement value) 
		{
			if (value == null)
				throw new ArgumentNullException ("value");

			if ((value.LocalName == "Object") && (value.NamespaceURI == xmldsig)) {
				doc.LoadXml (value.OuterXml);
				XmlAttribute xa = value.Attributes ["Id"];
				id = ((xa != null) ? xa.InnerText : null);
				xa = value.Attributes ["MimeType"];
				mimeType = ((xa != null) ? xa.InnerText : null);
				xa = value.Attributes ["Encoding"];
				encoding = ((xa != null) ? xa.InnerText : null);
			}
			else
				doc.LoadXml (value.OuterXml);
		}
	}
}