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
using System.Xml;

namespace System.Security.Cryptography.Xml {

	public class Signature {
		static XmlNamespaceManager dsigNsmgr;
		
		static Signature ()
		{
			dsigNsmgr = new XmlNamespaceManager (new NameTable ());
			dsigNsmgr.AddNamespace ("xd", XmlSignature.NamespaceURI);
		}

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

			XmlDocument document = new XmlDocument ();
			XmlElement xel = document.CreateElement (XmlSignature.ElementNames.Signature, XmlSignature.NamespaceURI);
			if (id != null)
				xel.SetAttribute (XmlSignature.AttributeNames.Id, id);

			XmlNode xn = info.GetXml ();
			XmlNode newNode = document.ImportNode (xn, true);
			xel.AppendChild (newNode);

			if (signature != null) {
				XmlElement sv = document.CreateElement (XmlSignature.ElementNames.SignatureValue, XmlSignature.NamespaceURI);
				sv.InnerText = Convert.ToBase64String (signature);
				xel.AppendChild (sv);
			}

			if (key != null) {
				xn = key.GetXml ();
				newNode = document.ImportNode (xn, true);
				xel.AppendChild (newNode);
			}

			if (list.Count > 0) {
				foreach (DataObject obj in list) {
					xn = obj.GetXml ();
					newNode = document.ImportNode (xn, true);
					xel.AppendChild (newNode);
				}
			}

			return xel;
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

			if ((value.LocalName == XmlSignature.ElementNames.Signature) && (value.NamespaceURI == XmlSignature.NamespaceURI)) {
				id = GetAttribute (value, XmlSignature.AttributeNames.Id);

				// LAMESPEC: This library is totally useless against eXtensibly Marked-up document.
				int i = NextElementPos (value.ChildNodes, 0, XmlSignature.ElementNames.SignedInfo, XmlSignature.NamespaceURI, true);
				XmlElement sinfo = (XmlElement) value.ChildNodes [i];
				info = new SignedInfo ();
				info.LoadXml (sinfo);

				i = NextElementPos (value.ChildNodes, ++i, XmlSignature.ElementNames.SignatureValue, XmlSignature.NamespaceURI, true);
				XmlElement sigValue = (XmlElement) value.ChildNodes [i];
				signature = Convert.FromBase64String (sigValue.InnerText);

				// signature isn't required: <element ref="ds:KeyInfo" minOccurs="0"/> 
				i = NextElementPos (value.ChildNodes, ++i, XmlSignature.ElementNames.KeyInfo, XmlSignature.NamespaceURI, false);
				if (i > 0) {
					XmlElement kinfo = (XmlElement) value.ChildNodes [i];
					key = new KeyInfo ();
					key.LoadXml (kinfo);
				}

				XmlNodeList xnl = value.SelectNodes ("xd:Object", dsigNsmgr);
				foreach (XmlElement xn in xnl) {
					DataObject obj = new DataObject ();
					obj.LoadXml (xn);
					AddObject (obj);
				}
			}
			else
				throw new CryptographicException ("Malformed element: Signature.");

			// if invalid
			if (info == null)
				throw new CryptographicException ("SignedInfo");
			if (signature == null)
				throw new CryptographicException ("SignatureValue");
		}

		private int NextElementPos (XmlNodeList nl, int pos, string name, string ns, bool required)
		{
			while (pos < nl.Count) {
				if (nl [pos].NodeType == XmlNodeType.Element) {
					if (nl [pos].LocalName != name && nl [pos].NamespaceURI != ns) {
						if (required)
							throw new CryptographicException ("Malformed element " + name);
						else
							return -2;
					}
					return pos;
				}
				else
					pos++;
			}
			if (required)
				throw new CryptographicException ("Malformed element " + name);
			return -1;
		}
	}
}