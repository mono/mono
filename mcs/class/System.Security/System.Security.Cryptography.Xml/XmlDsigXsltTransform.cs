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
			XmlResolver resolver = GetResolver ();

			XslTransform xsl = new XslTransform ();
			XmlDocument doc = new XmlDocument ();
#if NET_1_1
			doc.XmlResolver = resolver;
#endif
			foreach (XmlNode n in xnl)
				doc.AppendChild (doc.ImportNode (n, true));
#if NET_1_1
			xsl.Load (doc, resolver);
#else
			xsl.Load (doc);
#endif

			if (inputDoc == null)
				throw new NullReferenceException ("Load input document before transformation.");

			MemoryStream stream = new MemoryStream ();
			// only possible output: Stream
#if NET_1_1
			xsl.XmlResolver = resolver;
#endif
			xsl.Transform (inputDoc, null, stream);

			stream.Seek (0, SeekOrigin.Begin);
			return stream;
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
#if NET_1_1
				inputDoc.XmlResolver = GetResolver ();
#endif
//				inputDoc.Load (obj as Stream);
				inputDoc.Load (new XmlSignatureStreamReader (
					new StreamReader (obj as Stream)));
			}
			else if (obj is XmlDocument) {
				inputDoc= obj as XmlDocument;
			}
			else if (obj is XmlNodeList) {
				inputDoc = new XmlDocument ();
#if NET_1_1
				inputDoc.XmlResolver = GetResolver ();
#endif
				XmlNodeList nl = (XmlNodeList) obj;
				for (int i = 0; i < nl.Count; i++)
					inputDoc.AppendChild (inputDoc.ImportNode (nl [i], true));
			}
		}
	}
}
