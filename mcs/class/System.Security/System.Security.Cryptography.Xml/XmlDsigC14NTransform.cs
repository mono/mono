//
// XmlDsigC14NTransform.cs - C14N Transform implementation for XML Signature
// http://www.w3.org/TR/xml-c14n
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//	Aleksey Sanin (aleksey@aleksey.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
// (C) 2003 Aleksey Sanin (aleksey@aleksey.com)
//

using System.Collections;
using System.IO;
using System.Text;
using System.Xml;

using Mono.Xml;

namespace System.Security.Cryptography.Xml { 

	public class XmlDsigC14NTransform : Transform {
		private Type[] input;
		private Type[] output;
		private XmlCanonicalizer canonicalizer;
		private Stream s;
		
		public XmlDsigC14NTransform () 
		{
			Algorithm = "http://www.w3.org/TR/2001/REC-xml-c14n-20010315";
			canonicalizer = new XmlCanonicalizer(false, false);
		}

		public XmlDsigC14NTransform (bool includeComments) 
		{
			canonicalizer = new XmlCanonicalizer(includeComments, false);
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

		[MonoTODO]
		public override void LoadInput (object obj) 
		{
			if (obj is Stream) {
				s = (obj as Stream);
				// todo: parse doc from stream?
			} else if (obj is XmlDocument)
				s = canonicalizer.Canonicalize((obj as XmlDocument));
			else if (obj is XmlNodeList)
				s = canonicalizer.Canonicalize((obj as XmlNodeList));
			// note: there is no default are other types won't throw an exception
		}
	}
}

