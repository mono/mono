//
// Reference.cs - Reference implementation for XML Signature
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System.IO;
using System.Text;
using System.Xml;

namespace System.Security.Cryptography.Xml { 

	// http://www.w3.org/TR/2002/REC-xmldsig-core-20020212/Overview.html#sec-Reference
	public class Reference {

		private TransformChain chain;
		private string digestMethod;
		private byte[] digestValue;
		private string id;
		private string uri;
		private string type;
		private HashAlgorithm hash;

		static private string xmldsig = "http://www.w3.org/2000/09/xmldsig#";
		static private string sha1 = xmldsig + "sha1";

		public Reference () 
		{
			chain = new TransformChain ();
			digestMethod = sha1;
		}

		[MonoTODO()]
		public Reference (Stream stream) : this () 
		{
		}

		public Reference (string uri) : this ()
		{
			this.uri = uri;
		}

		// default to SHA1
		public string DigestMethod {
			get { return digestMethod; }
			set { digestMethod = value; }
		}

		public byte[] DigestValue {
			get { return digestValue; }
			set { digestValue = value; }
		}

		public string Id {
			get { return id; }
			set { id = value; }
		}

		public TransformChain TransformChain {
			get { return chain; }
		}

		public string Type {
			get { return type; }
			set { type = value; }
		}

		public string Uri {
			get { return uri; }
			set { uri = value; }
		}

		public void AddTransform (Transform transform) 
		{
			chain.Add (transform);
		}

		public XmlElement GetXml () 
		{
			if (digestMethod == null)
				throw new CryptographicException ("DigestMethod");
			if (digestValue == null)
				throw new NullReferenceException ("DigestValue");

			StringBuilder sb = new StringBuilder ();
			sb.Append ("<Reference");
			if (id != null) {
				sb.Append (" Id=\"");
				sb.Append (id);
				sb.Append ("\"");
			}
			if (uri != null) {
				sb.Append (" URI=\"");
				sb.Append (uri);
				sb.Append ("\"");
			}
			if (type != null) {
				sb.Append (" Type=\"");
				sb.Append (type);
				sb.Append ("\"");
			}
			sb.Append (" xmlns=\"");
			sb.Append (xmldsig);
			sb.Append ("\">");

			if (chain.Count > 0) {
				sb.Append ("<Transforms>");
				sb.Append ("</Transforms>");
			}

			sb.Append ("<DigestMethod Algorithm=\"");
			sb.Append (digestMethod);
			sb.Append ("\" />");
			sb.Append ("<DigestValue>");
			sb.Append (Convert.ToBase64String (digestValue));
			sb.Append ("</DigestValue>");
			sb.Append ("</Reference>");

			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (sb.ToString ());

			if (chain.Count > 0) {
				XmlNodeList xnl = doc.GetElementsByTagName ("Transforms");
				foreach (Transform t in chain) {
					XmlNode xn = t.GetXml ();
					XmlNode newNode = doc.ImportNode (xn, true);
					xnl[0].AppendChild (newNode);
				}
			}

			return doc.DocumentElement;
		}

		private string GetAttributeFromElement (XmlElement xel, string attribute, string element) 
		{
			string result = null;
			XmlNodeList xnl = xel.GetElementsByTagName (element);
			if ((xnl != null) && (xnl.Count > 0)) {
				XmlAttribute xa = xnl[0].Attributes [attribute];
				if (xa != null)
					result = xa.InnerText;
			}
			return result;
		}

		// note: we do NOT return null -on purpose- if attribute isn't found
		private string GetAttribute (XmlElement xel, string attribute) 
		{
			XmlAttribute xa = xel.Attributes [attribute];
			return ((xa != null) ? xa.InnerText : null);
		}

		public void LoadXml (XmlElement value) 
		{
			if (value == null)
				throw new ArgumentNullException ("value");

			if ((value.LocalName == "Reference") && (value.NamespaceURI == xmldsig)) {
				id = GetAttribute (value, "Id");
				uri = GetAttribute (value, "URI");
				type = GetAttribute (value, "Type");
				// Note: order is important for validations
				XmlNodeList xnl = value.GetElementsByTagName ("Transform");
				if ((xnl != null) && (xnl.Count > 0)) {
					Transform t = null;
					foreach (XmlNode xn in xnl) {
						string a = GetAttribute ((XmlElement)xn, "Algorithm");
						switch (a) {
							case "http://www.w3.org/2000/09/xmldsig#base64":
								t = new XmlDsigBase64Transform ();
								break;
							case "http://www.w3.org/TR/2001/REC-xml-c14n-20010315":
								t = new XmlDsigC14NTransform ();
								break;
							case "http://www.w3.org/TR/2001/REC-xml-c14n-20010315#WithComments":
								t = new XmlDsigC14NWithCommentsTransform ();
								break;
							case "http://www.w3.org/2000/09/xmldsig#enveloped-signature":
								t = new XmlDsigEnvelopedSignatureTransform ();
								break;
							case "http://www.w3.org/TR/1999/REC-xpath-19991116":
								t = new XmlDsigXPathTransform ();
								break;
							case "http://www.w3.org/TR/1999/REC-xslt-19991116":
								t = new XmlDsigXsltTransform ();
								break;
							default:
								throw new NotSupportedException ();
						}
						AddTransform (t);
					}
				}
				// get DigestMethod
				DigestMethod = GetAttributeFromElement (value, "Algorithm", "DigestMethod");
				// get DigestValue
				xnl = value.GetElementsByTagName ("DigestValue");
				if ((xnl != null) && (xnl.Count > 0)) {
					DigestValue = Convert.FromBase64String (xnl[0].InnerText);
				}
			}
			else
				throw new CryptographicException ();
		}
	}
}
