//
// System.Xml.XmlDocumentFragment
//
// Author:
//   Duncan Mak  (duncan@ximian.com)
//
// (C), Ximian, Inc
//
using System;

namespace System.Xml
{
	public class XmlDocumentFragment : XmlNode
	{
		#region Constructor

		internal XmlDocumentFragment (XmlDocument doc)
			: base (doc)
		{
		}
		
		#endregion
		#region Properties

		[MonoTODO]
		public override string InnerXml {
			get {throw new NotImplementedException(); }
		}
		
		public override string LocalName {
			get { return "#document-fragment"; }
		}


		public override string Name { 
			get { return "#document-fragment"; }
		}

		public override XmlNodeType NodeType {
			get { return XmlNodeType.DocumentFragment; }
		}

		public override XmlDocument OwnerDocument {
			get { return base.OwnerDocument; }
		}

		public override XmlNode ParentNode {
			get { return null; } // it's always null here.
		}

		#endregion

		#region Methods		
		public override XmlNode CloneNode (bool deep)
		{
			if (deep) { // clone document + child nodes
				XmlNode node = FirstChild;

				while ((node != null) && (node.HasChildNodes)) {
					AppendChild (node.NextSibling.CloneNode (false));
					node = node.NextSibling;
				}

				return node;
			} else
				return new XmlDocumentFragment (OwnerDocument);
		}

		[MonoTODO]
		public override void WriteContentTo (XmlWriter w)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void WriteTo (XmlWriter w)
		{
			throw new NotImplementedException ();
		}

		#endregion
	}
}
