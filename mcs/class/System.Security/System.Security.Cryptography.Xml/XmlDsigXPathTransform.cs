//
// XmlDsigXPathTransform.cs - 
//	XmlDsigXPathTransform implementation for XML Signature
// http://www.w3.org/TR/1999/REC-xpath-19991116 
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

	// www.w3.org/TR/xmldsig-core/
	// see Section 6.6.3 of the XMLDSIG specification
	[MonoTODO]
	public class XmlDsigXPathTransform : Transform {

		private Type[] input;
		private Type[] output;
		private XmlNodeList xnl;
		private XmlNodeList xpathNodes;

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
			return xpathNodes;
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
			xnl = nodeList;
		}

		public override void LoadInput (object obj) 
		{
			XmlNode xn = null;
			// possible input: Stream, XmlDocument, and XmlNodeList
			if (obj is Stream) {
				XmlDocument doc = new XmlDocument ();
				doc.Load (obj as Stream);
			}
			else if (obj is XmlDocument) {
			}
			else if (obj is XmlNodeList) {
				xnl = (XmlNodeList) obj;
			}

			if (xn != null) {
				string xpath = xn.InnerXml;
				// only possible output: XmlNodeList
				xpathNodes = xnl[0].SelectNodes (xpath);
			}
			else
				xpathNodes = null;
		}
	}
}
