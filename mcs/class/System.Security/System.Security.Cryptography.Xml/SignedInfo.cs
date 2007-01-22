//
// SignedInfo.cs - SignedInfo implementation for XML Signature
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//      Tim Coleman (tim@timcoleman.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) Tim Coleman, 2004
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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
using System.Runtime.InteropServices;
using System.Xml;

namespace System.Security.Cryptography.Xml { 

	public class SignedInfo : ICollection, IEnumerable {

		private ArrayList references;
		private string c14nMethod;
		private string id;
		private string signatureMethod;
		private string signatureLength;
		private XmlElement element;

		public SignedInfo() 
		{
			references = new ArrayList ();
			c14nMethod = "http://www.w3.org/TR/2001/REC-xml-c14n-20010315";
		}

		public string CanonicalizationMethod {
			get { return c14nMethod; }
			set {
				c14nMethod = value;
				element = null;
			}
		}

#if NET_2_0
		[ComVisible (false)]
		[MonoTODO]
		public Transform CanonicalizationMethodObject {
			get { throw new NotImplementedException (); }
		}
#endif

		// documented as not supported (and throwing exception)
		public int Count {
			get { throw new NotSupportedException (); }
		}

		public string Id {
			get { return id; }
			set {
				element = null;
				id = value;
			}
		}

		// documented as not supported (and throwing exception)
		public bool IsReadOnly {
			get { throw new NotSupportedException (); }
		}

		// documented as not supported (and throwing exception)
		public bool IsSynchronized {
			get { throw new NotSupportedException (); }
		}

		// Manipulating this array never affects GetXml() when 
		// LoadXml() was used. 
		// (Actually, there is no way to detect modification.)
		public ArrayList References {
			get { return references; }
		}

		public string SignatureLength {
			get { return signatureLength; }
			set {
				element = null;
				signatureLength = value;
			}
		}

		public string SignatureMethod {
			get { return signatureMethod; }
			set {
				element = null;
				signatureMethod = value;
			}
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

		public XmlElement GetXml ()
		{
			if (element != null)
				return element;

			if (signatureMethod == null)
				throw new CryptographicException ("SignatureMethod");
			if (references.Count == 0)
				throw new CryptographicException ("References empty");

			XmlDocument document = new XmlDocument ();
			XmlElement xel = document.CreateElement (XmlSignature.ElementNames.SignedInfo, XmlSignature.NamespaceURI);
			if (id != null)
				xel.SetAttribute (XmlSignature.AttributeNames.Id, id);

			if (c14nMethod != null) {
				XmlElement c14n = document.CreateElement (XmlSignature.ElementNames.CanonicalizationMethod, XmlSignature.NamespaceURI);
				c14n.SetAttribute (XmlSignature.AttributeNames.Algorithm, c14nMethod);
				xel.AppendChild (c14n);
			}
			if (signatureMethod != null) {
				XmlElement sm = document.CreateElement (XmlSignature.ElementNames.SignatureMethod, XmlSignature.NamespaceURI);
				sm.SetAttribute (XmlSignature.AttributeNames.Algorithm, signatureMethod);
				if (signatureLength != null) {
					XmlElement hmac = document.CreateElement (XmlSignature.ElementNames.HMACOutputLength, XmlSignature.NamespaceURI);
					hmac.InnerText = signatureLength;
					sm.AppendChild (hmac);
				}
				xel.AppendChild (sm);
			}

			// This check is only done when element is created here.
			if (references.Count == 0)
				throw new CryptographicException ("At least one Reference element is required in SignedInfo.");

			// we add References afterward so we don't end up with extraneous
			// xmlns="..." in each reference elements.
			foreach (Reference r in references) {
				XmlNode xn = r.GetXml ();
				XmlNode newNode = document.ImportNode (xn, true);
				xel.AppendChild (newNode);
			}

			return xel;
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

			if ((value.LocalName != XmlSignature.ElementNames.SignedInfo) || (value.NamespaceURI != XmlSignature.NamespaceURI))
				throw new CryptographicException ();

			id = GetAttribute (value, XmlSignature.AttributeNames.Id);
			c14nMethod = XmlSignature.GetAttributeFromElement (value, XmlSignature.AttributeNames.Algorithm, XmlSignature.ElementNames.CanonicalizationMethod);

			XmlElement sm = XmlSignature.GetChildElement (value, XmlSignature.ElementNames.SignatureMethod, XmlSignature.NamespaceURI);
			if (sm != null) {
				signatureMethod = sm.GetAttribute (XmlSignature.AttributeNames.Algorithm);
				XmlElement length = XmlSignature.GetChildElement (sm, XmlSignature.ElementNames.HMACOutputLength, XmlSignature.NamespaceURI);
				if (length != null) {
					signatureLength = length.InnerText;
				}
			}

			for (int i = 0; i < value.ChildNodes.Count; i++) {
				XmlNode n = value.ChildNodes [i];
				if (n.NodeType == XmlNodeType.Element &&
					n.LocalName == XmlSignature.ElementNames.Reference &&
					n.NamespaceURI == XmlSignature.NamespaceURI) {
					Reference r = new Reference ();
					r.LoadXml ((XmlElement) n);
					AddReference (r);
				}
			}
			element = value;
		}
	}
}
