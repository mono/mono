//
// KeyInfoNode.cs - KeyInfoNode implementation for XML Signature
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System.Text;
using System.Xml;

namespace System.Security.Cryptography.Xml {

	public class KeyInfoNode : KeyInfoClause {

		private XmlElement Node;

		public KeyInfoNode () {}

		public KeyInfoNode (XmlElement node) 
		{
			LoadXml (node);
		}

		public XmlElement Value {
			get { return Node; }
			set { Node = value; }
		}

		public override XmlElement GetXml () 
		{
			return Node;
		}

		// LAMESPEC: No ArgumentNullException is thrown if value == null
		public override void LoadXml (XmlElement value) 
		{
			Node = value;
		}
	}
}