//
// XmlDsigEnvelopedSignatureTransform.cs - 
//	Enveloped Signature Transform implementation for XML Signature
// http://www.w3.org/TR/1999/REC-xslt-19991116 
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
//

using System.IO;
using System.Security.Cryptography;
using System.Xml;
using System.Xml.Xsl;

namespace System.Security.Cryptography.Xml { 

public class XmlDsigXsltTransform : Transform {

	private bool comments;
	private XmlNodeList xnl;
	private CryptoStream cs;

	public XmlDsigXsltTransform () : this (false) {}

	public XmlDsigXsltTransform (bool includeComments) 
	{
		comments = includeComments;
		algo = "http://www.w3.org/TR/1999/REC-xslt-19991116";
	}

	public override Type[] InputTypes {
		get {
			if (input == null) {
				lock (this) {
					// this way the result is cached if called multiple time
					input = new Type [3];
					input[0] = typeof (System.IO.Stream);
					input[1] = typeof (System.Xml.XmlDocument);
					input[2] = typeof (System.Xml.XmlNodeList);
				}
			}
			return input;
		}
	}

	public override Type[] OutputTypes {
		get {
			if (output == null) {
				lock (this) {
					// this way the result is cached if called multiple time
					output = new Type [1];
					output[0] = typeof (System.IO.Stream);
				}
			}
			return output;
		}
	}
		
	protected override XmlNodeList GetInnerXml () 
	{
		return xnl;
	}

	public override object GetOutput () 
	{
		return (object) cs;
	}

	public override object GetOutput (Type type) 
	{
		if (type != Type.GetType ("System.IO.Stream"))
			throw new ArgumentException ("type");
		return GetOutput ();
	}

	public override void LoadInnerXml (XmlNodeList nodeList) 
	{
		if (nodeList == null)
			throw new CryptographicException ("nodeList");
		xnl = nodeList;
	}

	[MonoTODO()]
	public override void LoadInput (object obj) 
	{
		XslTransform xsl = new XslTransform ();
		XmlDocument doc = new XmlDocument ();
		Stream stream = null;

		// possible input: Stream, XmlDocument, and XmlNodeList
		if (obj is Stream) {
			doc.Load (obj as Stream);
			xsl.Load (doc);
		}
		else if (obj is XmlDocument) {
			xsl.Load (obj as XmlDocument);
		}
		else if (obj is XmlNodeList) {
//			xnl = (XmlNodeList) obj;
//			xsl.Load (obj a);
		}

		if (xnl != null) {
			stream = new MemoryStream ();
			// only possible output: Stream
			xsl.Transform (doc, null, stream);
		}

		if (stream != null)
			cs = new CryptoStream (stream, new FromBase64Transform (), CryptoStreamMode.Read);
		else
			cs = null;
		// note: there is no default are other types won't throw an exception
	}
}

}
