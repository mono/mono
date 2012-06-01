//
// XmlDsigEnvelopedSignatureTransform.cs - 
//	Enveloped Signature Transform implementation for XML Signature
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

using System.Collections;
using System.IO;
using System.Xml;

namespace System.Security.Cryptography.Xml { 

	public class XmlDsigEnvelopedSignatureTransform : Transform {

		private Type[] input;
		private Type[] output;
		private bool comments;
		private object inputObj;

		public XmlDsigEnvelopedSignatureTransform ()
			: this (false)
		{
		}

		public XmlDsigEnvelopedSignatureTransform (bool includeComments) 
		{
			Algorithm = XmlSignature.AlgorithmNamespaces.XmlDsigEnvelopedSignatureTransform;
			comments = includeComments;
		}

		public override Type[] InputTypes {
			get {
				if (input == null) {
					input = new Type [3];
					input[0] = typeof (System.IO.Stream);
					input[1] = typeof (System.Xml.XmlDocument);
					input[2] = typeof (System.Xml.XmlNodeList);
				}
				return input;
			}
		}

		public override Type[] OutputTypes {
			get {
				if (output == null) {
					output = new Type [2];
					output [0] = typeof (System.Xml.XmlDocument);
					output [1] = typeof (System.Xml.XmlNodeList);
				}
				return output;
			}
		}

		protected override XmlNodeList GetInnerXml () 
		{
			return null; // THIS IS DOCUMENTED AS SUCH
		}

		// NOTE: This method never supports the requirements written
		// in xmldsig spec that says its input is canonicalized before
		// transforming. This method just removes Signature element.
		// Canonicalization is done in SignedXml.
		public override object GetOutput ()
		{
			XmlDocument doc = null;

			// possible input: Stream, XmlDocument, and XmlNodeList
			if (inputObj is Stream) {
				doc = new XmlDocument ();
				doc.PreserveWhitespace = true;
				doc.XmlResolver = GetResolver ();
				doc.Load (new XmlSignatureStreamReader (
					new StreamReader (inputObj as Stream)));
				return GetOutputFromNode (doc, GetNamespaceManager (doc), true);
			}
			else if (inputObj is XmlDocument) {
				doc = inputObj as XmlDocument;
				return GetOutputFromNode (doc, GetNamespaceManager (doc), true);
			}
			else if (inputObj is XmlNodeList) {
				ArrayList al = new ArrayList ();
				XmlNodeList nl = (XmlNodeList) inputObj;
				if (nl.Count > 0) {
					XmlNamespaceManager m = GetNamespaceManager (nl.Item (0));
					ArrayList tmp = new ArrayList ();
					foreach (XmlNode n in nl)
						tmp.Add (n);
					foreach (XmlNode n in tmp)
						if (n.SelectNodes ("ancestor-or-self::dsig:Signature", m).Count == 0)
							al.Add (GetOutputFromNode (n, m, false));
				}
				return new XmlDsigNodeList (al);
			}
			// Note that it is unexpected behavior with related to InputTypes (MS.NET accepts XmlElement)
			else if (inputObj is XmlElement) {
				XmlElement el = inputObj as XmlElement;
				XmlNamespaceManager m = GetNamespaceManager (el);
				if (el.SelectNodes ("ancestor-or-self::dsig:Signature", m).Count == 0)
					return GetOutputFromNode (el, m, true);
			}

			throw new NullReferenceException ();
		}

		private XmlNamespaceManager GetNamespaceManager (XmlNode n)
		{
			XmlDocument doc = ((n is XmlDocument) ? (n as XmlDocument) : n.OwnerDocument);
			XmlNamespaceManager nsmgr = new XmlNamespaceManager (doc.NameTable);
			nsmgr.AddNamespace ("dsig", XmlSignature.NamespaceURI);
			return nsmgr;
		}

		private XmlNode GetOutputFromNode (XmlNode input, XmlNamespaceManager nsmgr, bool remove)
		{
			if (remove) {
				XmlNodeList nl = input.SelectNodes ("descendant-or-self::dsig:Signature", nsmgr);
				ArrayList al = new ArrayList ();
				foreach (XmlNode n in nl)
					al.Add (n);
				foreach (XmlNode n in al)
					n.ParentNode.RemoveChild (n);
			}
			return input;
		}

		public override object GetOutput (Type type) 
		{
			if (type == typeof (Stream))
				return GetOutput ();
			throw new ArgumentException ("type");
		}

		public override void LoadInnerXml (XmlNodeList nodeList) 
		{
			// NO CHANGE
		}

		public override void LoadInput (object obj) 
		{
			inputObj = obj;
		}
	}
}
