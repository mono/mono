//
// KeyInfo.cs - Xml Signature KeyInfo implementation
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
//

using System.Collections;
using System.Text;
using System.Xml;

namespace System.Security.Cryptography.Xml {

public class KeyInfo : IEnumerable {

	static private string xmldsig = "http://www.w3.org/2000/09/xmldsig#";

	private ArrayList Info;
	private string id;

	public KeyInfo() 
	{
		Info = new ArrayList ();
	}

	public int Count {
		get { return Info.Count; }
	}
	
	public string Id {
		get { return id; }
		set { id = value; }
	}

	public void AddClause (KeyInfoClause clause) 
	{
		Info.Add (clause);
	}

	public IEnumerator GetEnumerator () 
	{
		return Info.GetEnumerator ();
	}

	public IEnumerator GetEnumerator (Type requestedObjectType)
	{
		// Build a new ArrayList...
		ArrayList TypeList = new ArrayList ();
		IEnumerator e = Info.GetEnumerator ();
		while (true) {
			// ...with all object of specified type...
			if ((e.Current).GetType().Equals (requestedObjectType))
				TypeList.Add (e.Current);
			if (!e.MoveNext ())
				break;
		}
		// ...and return its enumerator
		return TypeList.GetEnumerator ();
	}

	public XmlElement GetXml () 
	{
		StringBuilder sb = new StringBuilder ();
		sb.Append ("<KeyInfo xmlns=\"");
		sb.Append (xmldsig);
		sb.Append ("\" />");

		XmlDocument doc = new XmlDocument ();
		doc.LoadXml (sb.ToString ());
		// we add References afterward so we don't end up with extraneous
		// xmlns="..." in each reference elements.
		foreach (KeyInfoClause kic in Info) {
			XmlNode xn = kic.GetXml ();
			XmlNode newNode = doc.ImportNode (xn, true);
			doc.DocumentElement.AppendChild (newNode);
		}
		return doc.DocumentElement;
	}

	public void LoadXml (XmlElement value) 
	{
		if (value == null)
			throw new ArgumentNullException ("value");

		if ((value.LocalName == "KeyInfo") && (value.NamespaceURI == xmldsig)) {
			foreach (XmlNode n in value.ChildNodes) {
				KeyInfoClause kic = null;
				if (n is XmlWhitespace)
					continue;

				switch (n.LocalName) {
				case "KeyValue":
					XmlNodeList xnl = n.ChildNodes;
					if (xnl.Count > 0) {
						// we must now treat the whitespace !
						foreach (XmlNode m in xnl) {
							switch (m.LocalName) {
							case "DSAKeyValue":
								kic = (KeyInfoClause) new DSAKeyValue ();
								break;
							case "RSAKeyValue":
								kic = (KeyInfoClause) new RSAKeyValue ();
								break;
							}
						}
					}
					break;
				case "KeyName":
					kic = (KeyInfoClause) new KeyInfoName ();
					break;
				case "RetrievalMethod":
					kic = (KeyInfoClause) new KeyInfoRetrievalMethod ();
					break;
				case "X509Data":
					kic = (KeyInfoClause) new KeyInfoX509Data ();
					break;
				case "RSAKeyValue":
					kic = (KeyInfoClause) new RSAKeyValue ();
					break;
				default:
					kic = (KeyInfoClause) new KeyInfoNode ();
					break;
				}

				if (kic != null) {
					kic.LoadXml ((XmlElement) n);
					AddClause (kic);
				}
			}
		}
	}
}

}
