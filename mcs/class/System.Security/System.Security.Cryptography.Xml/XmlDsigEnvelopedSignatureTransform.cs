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

using System.Collections;
using System.IO;
using System.Xml;

namespace System.Security.Cryptography.Xml { 

	[MonoTODO]
	public class XmlDsigEnvelopedSignatureTransform : Transform {

		private Type[] input;
		private Type[] output;
		private bool comments;
		private object inputObj;

		public XmlDsigEnvelopedSignatureTransform () 
		{
			Algorithm = "http://www.w3.org/2000/09/xmldsig#enveloped-signature";
			comments = false;
		}

		public XmlDsigEnvelopedSignatureTransform (bool includeComments) 
		{
			comments = includeComments;
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
						output = new Type [2];
						output [0] = typeof (System.Xml.XmlDocument);
						output [1] = typeof (System.Xml.XmlNodeList);
					}
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
#if ! NET_1_0
				doc.XmlResolver = GetResolver ();
#endif
				doc.Load (inputObj as Stream);
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
			XmlDocument doc = n is XmlDocument ? n as XmlDocument : n.OwnerDocument;
			XmlNamespaceManager nsmgr = new XmlNamespaceManager (doc.NameTable);
			nsmgr.AddNamespace ("dsig", XmlSignature.NamespaceURI);
			return nsmgr;
		}

		private XmlNode GetOutputFromNode (XmlNode input, XmlNamespaceManager nsmgr, bool remove)
		{
			XmlDocument doc = input is XmlDocument ? input as XmlDocument : input.OwnerDocument;
			if (remove) {
				XmlNodeList nl = input.SelectNodes ("descendant-or-self::dsig:Signature", nsmgr);
				foreach (XmlNode n in nl)
					n.ParentNode.RemoveChild (n);
			}
			return input;
		}

		public override object GetOutput (Type type) 
		{
			if (type == Type.GetType ("Stream"))
				return GetOutput ();
			throw new ArgumentException ("type");
		}

		public override void LoadInnerXml (XmlNodeList nodeList) 
		{
			// NO CHANGE
		}

		[MonoTODO ("test")]
		public override void LoadInput (object obj) 
		{
			inputObj = obj;
		}
	}
}
