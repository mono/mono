//
// XmlDsigEnvelopedSignatureTransform.cs - 
//	Enveloped Signature Transform implementation for XML Signature
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System.IO;
using System.Xml;

namespace System.Security.Cryptography.Xml { 

	[MonoTODO]
	public class XmlDsigEnvelopedSignatureTransform : Transform {

		private Type[] input;
		private Type[] output;
		private bool comments;

		public XmlDsigEnvelopedSignatureTransform () 
		{
			comments = false;
		}

		public XmlDsigEnvelopedSignatureTransform (bool includeComments) 
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
						output = new Type [2];
						input[0] = typeof (System.Xml.XmlDocument);
						input[1] = typeof (System.Xml.XmlNodeList);
					}
				}
				return output;
			}
		}

		protected override XmlNodeList GetInnerXml () 
		{
			return null; // THIS IS DOCUMENTED AS SUCH
		}

		[MonoTODO()]
		public override object GetOutput() 
		{
//			return (object) new XmlNodeList ();
			return null;
		}

		public override object GetOutput (Type type) 
		{
			if (type == Type.GetType ("Stream"))
				return GetOutput ();
			throw new ArgumentException ("type");
		}

		public override void LoadInnerXml (XmlNodeList nodeList) 
		{
			// NO CHANGE
		}

		[MonoTODO()]
		public override void LoadInput (object obj) 
		{
			//	if (type.Equals (Stream.GetType ())
		}
	}
}
