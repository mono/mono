//
// XmlDsigC14NTransform.cs - C14N Transform implementation for XML Signature
// http://www.w3.org/TR/xml-c14n
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System.IO;
using System.Text;
using System.Xml;

namespace System.Security.Cryptography.Xml { 

	[MonoTODO]
	public class XmlDsigC14NTransform : Transform {

		private Type[] input;
		private Type[] output;
		private bool comments;
		private Stream s;

		public XmlDsigC14NTransform () 
		{
			Algorithm = "http://www.w3.org/TR/2001/REC-xml-c14n-20010315";
			comments = false;
		}

		public XmlDsigC14NTransform (bool includeComments) 
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
						output = new Type [1];
						output[0] = typeof (System.IO.Stream);
					}
				}
				return output;
			}
		}

		protected override XmlNodeList GetInnerXml () 
		{
			return null; // THIS IS DOCUMENTED AS SUCH
		}

		public override object GetOutput () 
		{
			return (object) s;
		}

		public override object GetOutput (Type type) 
		{
			if (type == Type.GetType ("Stream"))
				return GetOutput ();
			throw new ArgumentException ("type");
		}

		public override void LoadInnerXml (XmlNodeList nodeList) 
		{
			// documented as not changing the state of the transform
		}

		public override void LoadInput (object obj) 
		{
			XmlNodeList xnl = null;

			if (obj is Stream) 
				s = (obj as Stream);
			else if (obj is XmlDocument)
				xnl = (obj as XmlDocument).ChildNodes;
			else if (obj is XmlNodeList)
				xnl = (XmlNodeList) obj;

			if (xnl != null) {
				StringBuilder sb = new StringBuilder ();
				foreach (XmlNode xn in xnl)
					sb.Append (xn.InnerText);

				UTF8Encoding utf8 = new UTF8Encoding ();
				byte[] data = utf8.GetBytes (sb.ToString ());
				s = new MemoryStream (data);
			}

			// note: there is no default are other types won't throw an exception
		}
	}
}
