//
// DSAKeyValue.cs - DSAKeyInfo implementation for XML Signature
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
//

using System.Security.Cryptography;
using System.Text;
using System.Xml;

namespace System.Security.Cryptography.Xml {

public class DSAKeyValue : KeyInfoClause {

	private DSA dsa;

	public DSAKeyValue () 
	{
		dsa = DSA.Create ();
	}

	public DSAKeyValue (DSA key) 
	{
		dsa = key;
	}

	public DSA Key 
	{
		get { return dsa; }
		set { dsa = value; }
	}

	public override XmlElement GetXml () 
	{
		StringBuilder sb = new StringBuilder ();
		sb.Append ("<KeyValue xmlns=\"http://www.w3.org/2000/09/xmldsig#\">");
		sb.Append (dsa.ToXmlString (false));
		sb.Append ("</KeyValue>");

		XmlDocument doc = new XmlDocument ();
		doc.LoadXml(sb.ToString ());
		return doc.DocumentElement;
	}

	public override void LoadXml (XmlElement value) 
	{
		if (value == null)
			throw new ArgumentNullException ();

		if ((value.LocalName == "KeyValue") && (value.NamespaceURI == "http://www.w3.org/2000/09/xmldsig#"))
			dsa.FromXmlString (value.InnerXml);
		else
			throw new CryptographicException ("value");
	}
}

}