//
// XmlDsigC14NWithCommentsTransform.cs - 
//	C14N with comments Transform implementation for XML Signature
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

namespace System.Security.Cryptography.Xml { 

	public class XmlDsigC14NWithCommentsTransform : XmlDsigC14NTransform {

		public XmlDsigC14NWithCommentsTransform() : base (true) 
		{
			Algorithm = "http://www.w3.org/TR/2001/REC-xml-c14n-20010315#WithComments";
		}
	}
}
