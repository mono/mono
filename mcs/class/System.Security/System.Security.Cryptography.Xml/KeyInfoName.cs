//
// KeyInfoName.cs - KeyInfoName implementation for XML Signature
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
//

using System.Text;
using System.Xml;

namespace System.Security.Cryptography.Xml {

public class KeyInfoName : KeyInfoClause {

	protected string Name;

	public KeyInfoName() {}

	public string Value {
		get { return Name; }
		set { Name = value; }
	}

	public override XmlElement GetXml () 
	{
		StringBuilder sb = new StringBuilder ();
		sb.Append ("<KeyName xmlns=\"http://www.w3.org/2000/09/xmldsig#\">");
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

		if ((value.LocalName == "KeyName") && (value.NamespaceURI == "http://www.w3.org/2000/09/xmldsig#"))
			Name = value.InnerXml;
		else
			Name = null;
	}
}

}