//
// XmlDsigEnvelopedSignatureTransform.cs - 
//	Enveloped Signature Transform implementation for XML Signature
// http://www.w3.org/TR/1999/REC-xslt-19991116 
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//	Atsushi Enomoto (atsushi@ximian.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
// (C) 2004 Novell Inc.
//

using System.IO;
using System.Xml;
using System.Xml.Xsl;

namespace System.Security.Cryptography.Xml 
{

	public class XmlDsigXsltTransform : Transform 
	{

		private Type [] input;
		private Type [] output;
		private bool comments;
		private XmlNodeList xnl;
		private XmlDocument inputDoc;

		public XmlDsigXsltTransform () : this (false)
		{
		}

		public XmlDsigXsltTransform (bool includeComments) 
		{
			comments = includeComments;
			Algorithm = "http://www.w3.org/TR/1999/REC-xslt-19991116";
		}

		public override Type [] InputTypes {
			get {
				if (input == null) {
					lock (this) {
						// this way the result is cached if called multiple time
						input = new Type [3];
						input [0] = typeof (System.IO.Stream);
						input [1] = typeof (System.Xml.XmlDocument);
						input [2] = typeof (System.Xml.XmlNodeList);
					}
				}
				return input;
			}
		}

		public override Type [] OutputTypes {
			get {
				if (output == null) {
					lock (this) {
						// this way the result is cached if called multiple time
						output = new Type [1];
						output [0] = typeof (System.IO.Stream);
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
			XslTransform xsl = new XslTransform ();
			XmlDocument doc = new XmlDocument ();
			doc.XmlResolver = GetResolver ();
			foreach (XmlNode n in xnl)
				doc.AppendChild (doc.ImportNode (n, true));
			xsl.Load (doc);

			Stream stream = null;

			stream = new MemoryStream ();
			// only possible output: Stream
			xsl.Transform (inputDoc, null, stream);

			CryptoStream cs = null;
			if (stream != null)
				cs = new CryptoStream (stream, new FromBase64Transform (), CryptoStreamMode.Read);
			// note: there is no default are other types won't throw an exception
			return cs;
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

		public override void LoadInput (object obj) 
		{
			// possible input: Stream, XmlDocument, and XmlNodeList
			if (obj is Stream) {
				inputDoc = new XmlDocument ();
				inputDoc.Load (obj as Stream);
			}
			else if (obj is XmlDocument) {
				inputDoc= obj as XmlDocument;
			}
			else if (obj is XmlNodeList) {
				inputDoc = new XmlDocument ();
				XmlNodeList nl = (XmlNodeList) obj;
				for (int i = 0; i < nl.Count; i++)
					inputDoc.AppendChild (inputDoc.ImportNode (nl [i], true));
			}
		}
	}
}
