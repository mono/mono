//
// Signature.cs - Signature implementation for XML Signature
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System.Collections;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

namespace System.Security.Cryptography.Xml {

	public class Signature {

		static private string xmldsig = "http://www.w3.org/2000/09/xmldsig#";

		private ArrayList list;
		private SignedInfo info;
		private KeyInfo key;
		private string id;
		private byte[] signature;

		public Signature() 
		{
			list = new ArrayList ();
		}

		public string Id {
			get { return id; }
			set { id = value; }
		}

		public KeyInfo KeyInfo {
			get { return key; }
			set { key = value; }
		}

		public IList ObjectList {
			get { return list; }
			set { list = ArrayList.Adapter (value); }
		}

		public byte[] SignatureValue {
			get { return signature; }
			set { signature = value; }
		}

		public SignedInfo SignedInfo {
			get { return info; }
			set { info = value; }
		}

		public void AddObject (DataObject dataObject) 
		{
			list.Add (dataObject);
		}

		public XmlElement GetXml () 
		{
			if (info == null)
				throw new CryptographicException ("SignedInfo");
			if (signature == null)
				throw new CryptographicException ("SignatureValue");

			StringBuilder sb = new StringBuilder ();
			sb.Append ("<Signature");
			if (id != null) {
				sb.Append (" Id = \"");
				sb.Append (id);
				sb.Append ("\"");
			}
			sb.Append (" xmlns=\"");
			sb.Append (xmldsig);
			sb.Append ("\" />");

			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (sb.ToString ());

			XmlNode xn = null;
			XmlNode newNode = null;

			if (info != null) {
				// this adds the xmlns=xmldsig
				xn = info.GetXml ();
				newNode = doc.ImportNode (xn, true);
				doc.DocumentElement.AppendChild (newNode);
			}

			if (signature != null) {
				XmlElement sv = doc.CreateElement ("SignatureValue", xmldsig);
				sv.InnerText = Convert.ToBase64String (signature);
				doc.DocumentElement.AppendChild (sv);
			}

			if (key != null) {
				xn = key.GetXml ();
				newNode = doc.ImportNode (xn, true);
				doc.DocumentElement.AppendChild (newNode);
			}

			if (list.Count > 0) {
				foreach (DataObject obj in list) {
					xn = obj.GetXml ();
					newNode = doc.ImportNode (xn, true);
					doc.DocumentElement.AppendChild (newNode);
				}
			}

			return doc.DocumentElement;
		}

		private string GetAttribute (XmlElement xel, string attribute) 
		{
			XmlAttribute xa = xel.Attributes [attribute];
			return ((xa != null) ? xa.InnerText : null);
		}

		public void LoadXml (XmlElement value) 
		{
			if (value == null)
				throw new ArgumentNullException ("value");

			if ((value.LocalName == "Signature") && (value.NamespaceURI == xmldsig)) {
				id = GetAttribute (value, "Id");

				XmlNodeList xnl = value.GetElementsByTagName ("SignedInfo");
				if ((xnl != null) && (xnl.Count == 1)) {
					info = new SignedInfo ();
					info.LoadXml ((XmlElement) xnl[0]);
				}

				xnl = value.GetElementsByTagName ("SignatureValue");
				if ((xnl != null) && (xnl.Count == 1)) {
					signature = Convert.FromBase64String (xnl[0].InnerText);
				}

				xnl = value.GetElementsByTagName ("KeyInfo");
				if ((xnl != null) && (xnl.Count == 1)) {
					key = new KeyInfo ();
					key.LoadXml ((XmlElement) xnl[0]);
				}

				xnl = value.GetElementsByTagName ("Object");
				if ((xnl != null) && (xnl.Count > 0)) {
					foreach (XmlNode xn in xnl) {
						DataObject obj = new DataObject ();
						obj.LoadXml ((XmlElement) xn);
						AddObject (obj);
					}
				}
			}

			// if invalid
			if (info == null)
				throw new CryptographicException ("SignedInfo");
			if (signature == null)
				throw new CryptographicException ("SignatureValue");
		}
	}
}