//
// XmlDsigXPathTransform.cs - 
//	XmlDsigXPathTransform implementation for XML Signature
// http://www.w3.org/TR/1999/REC-xpath-19991116 
//
// Author:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
// (C) 2004 Novell (http://www.novell.com)
//

using System.IO;
using System.Text;
using System.Xml;

namespace System.Security.Cryptography.Xml {

	// www.w3.org/TR/xmldsig-core/
	// see Section 6.6.3 of the XMLDSIG specification
	[MonoTODO]
	public class XmlDsigXPathTransform : Transform {

		private Type[] input;
		private Type[] output;
		private XmlNodeList xpath;
		private XmlDocument doc;

		public XmlDsigXPathTransform () 
		{
			Algorithm = "http://www.w3.org/TR/1999/REC-xpath-19991116";
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
						output[0] = typeof (System.Xml.XmlNodeList);
					}
				}
				return output;
			}
		}

		protected override XmlNodeList GetInnerXml () 
		{
			if (xpath == null) {
				// default value
				XmlDocument doc = new XmlDocument ();
				doc.LoadXml ("<XPath xmlns=\"" + XmlSignature.NamespaceURI + "\"></XPath>");
				xpath = doc.ChildNodes;
			}
			return xpath;
		}
		
		public override object GetOutput () 
		{
			// note: this will throw a NullReferenceException if 
			// doc is null - just like MS implementation does
			if ((xpath == null) || (xpath.Count < 1)) {
				// can't create an XmlNodeList
				XmlDocument xd = new XmlDocument ();
				return xd.ChildNodes;
			}
			return doc.ChildNodes;
//* I know it doesn't make a lot of sense - but this is what the MS framework
//* returns - I must miss something really bad
//*			return doc.DocumentElement.SelectNodes (xpath [0].InnerXml);
		}

		public override object GetOutput (Type type) 
		{
			if (type != typeof (XmlNodeList))
				throw new ArgumentException ("type");
			return GetOutput ();
		}

		public override void LoadInnerXml (XmlNodeList nodeList) 
		{
			if (nodeList == null)
				throw new CryptographicException ("nodeList");
			xpath = nodeList;
		}

		public override void LoadInput (object obj) 
		{
			// possible input: Stream, XmlDocument, and XmlNodeList
			if (obj is Stream) {
				doc = new XmlDocument ();
				doc.Load (obj as Stream);
			}
			else if (obj is XmlDocument) {
				doc = (obj as XmlDocument);
			}
			else if (obj is XmlNodeList) {
				doc = new XmlDocument ();
				foreach (XmlNode xn in (obj as XmlNodeList))  {
					XmlNode importedNode = doc.ImportNode (xn, true);
					doc.AppendChild (importedNode);
				}
			}
		}
	}
}
