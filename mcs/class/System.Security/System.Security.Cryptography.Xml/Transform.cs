//
// Transform.cs - Transform implementation for XML Signature
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System.Text;
using System.Xml;

namespace System.Security.Cryptography.Xml { 

	public abstract class Transform {

		static private string xmldsig = "http://www.w3.org/2000/09/xmldsig#";

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
			StringBuilder sb = new StringBuilder ();
			sb.Append ("<Transform Algorithm=\"");
			sb.Append (algo);
//			sb.Append ("\" xmlns=\"");
//			sb.Append (xmldsig);
			sb.Append ("\" />");

			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (sb.ToString ());
			return doc.DocumentElement;
		}

		public abstract void LoadInnerXml (XmlNodeList nodeList);

		public abstract void LoadInput (object obj);
	}
}
