//
// SignedInfo.cs - SignedInfo implementation for XML Signature
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

public class SignedInfo : ICollection, IEnumerable {

	private ArrayList references;
	private string c14nMethod;
	private string id;
	private string signatureMethod;
	private string signatureLength;

	static private string xmldsig = "http://www.w3.org/2000/09/xmldsig#";

	public SignedInfo() 
	{
		references = new ArrayList ();
		c14nMethod = "http://www.w3.org/TR/2001/REC-xml-c14n-20010315";
	}

	public string CanonicalizationMethod {
		get { return c14nMethod; }
		set { c14nMethod = value; }
	}

	// documented as not supported (and throwing exception)
	public int Count {
		get { throw new NotSupportedException (); }
	}

	public string Id {
		get { return id; }
		set { id = value; }
	}

	// documented as not supported (and throwing exception)
	public bool IsReadOnly {
		get { throw new NotSupportedException (); }
	}

	// documented as not supported (and throwing exception)
	public bool IsSynchronized {
		get { throw new NotSupportedException (); }
	}

	public ArrayList References {
		get { return references; }
	}

	public string SignatureLength {
		get { return signatureLength; }
		set { signatureLength = value; }
	}

	public string SignatureMethod {
		get { return signatureMethod; }
		set { signatureMethod = value; }
	}

	// documented as not supported (and throwing exception)
	public object SyncRoot {
		get { throw new NotSupportedException (); }
	}

	public void AddReference (Reference reference) 
	{
		references.Add (reference);
	}

	// documented as not supported (and throwing exception)
	public void CopyTo (Array array, int index) 
	{
		throw new NotSupportedException ();
	}

	public IEnumerator GetEnumerator () 
	{
		return references.GetEnumerator ();
	}

	public XmlElement GetXml() 
	{
		if (signatureMethod == null)
			throw new CryptographicException ("SignatureMethod");
		if (references.Count == 0)
			throw new CryptographicException ("References empty");

		StringBuilder sb = new StringBuilder ();
		sb.Append ("<SignedInfo");
		if (id != null) {
			sb.Append (" Id=\"");
			sb.Append (id);
			sb.Append ("\"");
		}
		sb.Append (" xmlns=\"");
		sb.Append (xmldsig);
		sb.Append ("\">");
		if (c14nMethod != null) {
			sb.Append ("<CanonicalizationMethod Algorithm=\"");
			sb.Append (c14nMethod);
			sb.Append ("\" />");
		}
		if (signatureMethod != null) {
			sb.Append ("<SignatureMethod Algorithm=\"");
			sb.Append (signatureMethod);
			if (signatureLength != null) {
				sb.Append ("\">");
				sb.Append ("<HMACOutputLength>");
				sb.Append (signatureLength);
				sb.Append ("</HMACOutputLength>");
				sb.Append ("</SignatureMethod>");
			}
			else
				sb.Append ("\" />");
		}
		sb.Append ("</SignedInfo>");

		XmlDocument doc = new XmlDocument ();
		doc.LoadXml (sb.ToString ());
		// we add References afterward so we don't end up with extraneous
		// xmlns="..." in each reference elements.
		foreach (Reference r in references) {
			XmlNode xn = r.GetXml ();
			XmlNode newNode = doc.ImportNode (xn, true);
			doc.DocumentElement.AppendChild (newNode);
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

	private string GetAttribute (XmlElement xel, string attribute) 
	{
		XmlAttribute xa = xel.Attributes [attribute];
		return ((xa != null) ? xa.InnerText : null);
	}

	public void LoadXml (XmlElement value) 
	{
		if (value == null)
			throw new ArgumentNullException ("value");

		if ((value.LocalName == "SignedInfo") && (value.NamespaceURI == xmldsig)) {
			id = GetAttribute (value, "Id");
			c14nMethod = GetAttributeFromElement (value, "Algorithm", "CanonicalizationMethod");
			signatureMethod = GetAttributeFromElement (value, "Algorithm", "SignatureMethod");
			// signatureLength for HMAC
			XmlNodeList xnl = value.GetElementsByTagName ("Reference");
			foreach (XmlNode xn in xnl) {
				Reference r = new Reference ();
				r.LoadXml ((XmlElement) xn);
				AddReference (r);
			}
		}
		else
			throw new CryptographicException ();
	}
}

}
