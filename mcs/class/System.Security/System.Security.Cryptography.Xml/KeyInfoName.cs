//
// KeyInfoName.cs - KeyInfoName implementation for XML Signature
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System.Text;
using System.Xml;

namespace System.Security.Cryptography.Xml {

	public class KeyInfoName : KeyInfoClause {

		static private string xmldsig = "http://www.w3.org/2000/09/xmldsig#";

		private string Name;

		public KeyInfoName() {}

		public string Value {
			get { return Name; }
			set { Name = value; }
		}

		public override XmlElement GetXml () 
		{
			StringBuilder sb = new StringBuilder ();
			sb.Append ("<KeyName xmlns=\"");
			sb.Append (xmldsig);
			sb.Append ("\">");
			sb.Append (Name);
			sb.Append ("</KeyName>");

			XmlDocument doc = new XmlDocument ();
			doc.LoadXml(sb.ToString ());
			return doc.DocumentElement;
		}

		public override void LoadXml (XmlElement value) 
		{
			if (value == null)
				throw new ArgumentNullException ();

			if ((value.LocalName == "KeyName") && (value.NamespaceURI == xmldsig))
				Name = value.InnerXml;
			else
				Name = null;
		}
	}
}