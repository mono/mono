//
// Manifest.cs - Manifest implementation for XML Signature
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2004 Novell (http://www.novell.com)
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
