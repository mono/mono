//
// Transform.cs - Transform implementation for XML Signature
//
// Author:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
// (C) 2004 Novell (http://www.novell.com)
//

using System.Runtime.InteropServices;
using System.Xml;

namespace System.Security.Cryptography.Xml { 

	public abstract class Transform {

		private string algo;

		public Transform () {}

		public string Algorithm {
			get { return algo; }
			set { algo = value; }
		}

		public abstract Type[] InputTypes {
			get;
		}

		public abstract Type[] OutputTypes {
			get;
		}

		protected abstract XmlNodeList GetInnerXml ();

		public abstract object GetOutput ();

		public abstract object GetOutput (Type type);

		public XmlElement GetXml () 
		{
			XmlDocument document = new XmlDocument ();
			XmlElement xel = document.CreateElement (XmlSignature.ElementNames.Transform, XmlSignature.NamespaceURI);
			xel.SetAttribute (XmlSignature.AttributeNames.Algorithm, algo);
			XmlNodeList xnl = this.GetInnerXml ();
			if (xnl != null) {
				foreach (XmlNode xn in xnl) {
					XmlNode importedNode = document.ImportNode (xn, true);
					xel.AppendChild (importedNode);
				}
			}
			return xel;
		}

		public abstract void LoadInnerXml (XmlNodeList nodeList);

		public abstract void LoadInput (object obj);

#if ! NET_1_0
		private XmlResolver xmlResolver;

		[ComVisible(false)]
		public XmlResolver Resolver {
			set { xmlResolver = value; }
		}
		
		internal XmlResolver GetResolver ()
		{
			return xmlResolver;
		}
#endif
	}
}
