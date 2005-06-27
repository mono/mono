//
// Manifest.cs - Manifest implementation for XML Signature
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2004 Novell (http://www.novell.com)
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
using System.Xml;

namespace System.Security.Cryptography.Xml { 

	internal class Manifest {

		private ArrayList references;
		private string id;
		private XmlElement element;

		public Manifest ()
		{
			references = new ArrayList ();
		}

		public Manifest (XmlElement xel) : this ()
		{
			LoadXml (xel);
		}

		public string Id {
			get { return id; }
			set {
				element = null;
				id = value;
			}
		}

		public ArrayList References {
			get { return references; }
		}

		public void AddReference (Reference reference) 
		{
			references.Add (reference);
		}

		public XmlElement GetXml () 
		{
			if (element != null)
				return element;

			XmlDocument document = new XmlDocument ();
			XmlElement xel = document.CreateElement (XmlSignature.ElementNames.SignedInfo, XmlSignature.NamespaceURI);
			if (id != null)
				xel.SetAttribute (XmlSignature.AttributeNames.Id, id);

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

			if ((value.LocalName != XmlSignature.ElementNames.Manifest) || (value.NamespaceURI != XmlSignature.NamespaceURI))
				throw new CryptographicException ();

			id = GetAttribute (value, XmlSignature.AttributeNames.Id);

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
