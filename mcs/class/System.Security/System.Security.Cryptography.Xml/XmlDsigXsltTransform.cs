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

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
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
			Algorithm = XmlSignature.AlgorithmNamespaces.XmlDsigXsltTransform;
		}

		public override Type [] InputTypes {
			get {
				if (input == null) {
					input = new Type [3];
					input [0] = typeof (System.IO.Stream);
					input [1] = typeof (System.Xml.XmlDocument);
					input [2] = typeof (System.Xml.XmlNodeList);
				}
				return input;
			}
		}

		public override Type [] OutputTypes {
			get {
				if (output == null) {
					output = new Type [1];
					output [0] = typeof (System.IO.Stream);
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
			if (xnl == null)
				throw new ArgumentNullException ("LoadInnerXml before transformation.");

			XmlResolver resolver = GetResolver ();

			XslTransform xsl = new XslTransform ();
			XmlDocument doc = new XmlDocument ();
			doc.XmlResolver = resolver;
			foreach (XmlNode n in xnl)
				doc.AppendChild (doc.ImportNode (n, true));
			xsl.Load (doc, resolver);

			if (inputDoc == null)
				throw new ArgumentNullException ("LoadInput before transformation.");

			MemoryStream stream = new MemoryStream ();
			// only possible output: Stream
			xsl.XmlResolver = resolver;
			xsl.Transform (inputDoc, null, stream);

			stream.Seek (0, SeekOrigin.Begin);
			return stream;
		}

		public override object GetOutput (Type type) 
		{
			if (type != typeof (Stream))
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
			Stream s = (obj as Stream);
			if (s != null) {
				inputDoc = new XmlDocument ();
				inputDoc.XmlResolver = GetResolver ();
//				inputDoc.Load (obj as Stream);
				inputDoc.Load (new XmlSignatureStreamReader (new StreamReader (s)));
				return;
			}

			XmlDocument xd = (obj as XmlDocument);
			if (xd != null) {
				inputDoc = xd;
				return;
			}

			XmlNodeList nl = (obj as XmlNodeList);
			if (nl != null) {
				inputDoc = new XmlDocument ();
				inputDoc.XmlResolver = GetResolver ();
				for (int i = 0; i < nl.Count; i++)
					inputDoc.AppendChild (inputDoc.ImportNode (nl [i], true));
			}
		}
	}
}
