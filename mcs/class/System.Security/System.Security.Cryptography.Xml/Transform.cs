//
// Transform.cs - Transform implementation for XML Signature
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
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
			return xel;
		}

		public abstract void LoadInnerXml (XmlNodeList nodeList);

		public abstract void LoadInput (object obj);

#if ! USE_VERSION_1_0
		private XmlResolver xmlResolver;

		[MonoTODO("property not (yet) used in derived classes")]
		[ComVisible(false)]
		XmlResolver Resolver {
			set { xmlResolver = value; }
		}
#endif
	}
}
