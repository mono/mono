//
// KeyInfoRetrievalMethod.cs - KeyInfoRetrievalMethod implementation for XML Signature
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System.Text;
using System.Xml;

namespace System.Security.Cryptography.Xml {

	public class KeyInfoRetrievalMethod : KeyInfoClause {

		static private string xmldsig = "http://www.w3.org/2000/09/xmldsig#";

		private string URI;

		public KeyInfoRetrievalMethod () {}

		public KeyInfoRetrievalMethod (string strUri) 
		{
			URI = strUri;
		}

		public string Uri {
			get { return URI; }
			set { URI = value; }
		}

		public override XmlElement GetXml () 
		{
			StringBuilder sb = new StringBuilder ();
			sb.Append ("<RetrievalElement ");
			if (URI != null) {
				sb.Append ("URI=\"");
				sb.Append (URI);
				sb.Append ("\" ");
			}
			sb.Append ("xmlns=\"");
			sb.Append (xmldsig);
			sb.Append ("\" />");

			XmlDocument doc = new XmlDocument ();
			doc.LoadXml(sb.ToString ());
			return doc.DocumentElement;
		}

		public override void LoadXml (XmlElement value) 
		{
			if (value == null)
				throw new ArgumentNullException ();

			if ((value.LocalName == "RetrievalElement") && (value.NamespaceURI == xmldsig)) {
				URI = value.Attributes["URI"].Value;
			}
			else
				URI = ""; // not null - so we return URI="" as attribute !!!
		}
	}
}