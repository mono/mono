//
// XmlDsigC14NWithCommentsTransform.cs - 
//	C14N with comments Transform implementation for XML Signature
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
//

using System.IO;
using System.Xml;

namespace System.Security.Cryptography.Xml { 

public class XmlDsigC14NWithCommentsTransform : XmlDsigC14NTransform {

	public XmlDsigC14NWithCommentsTransform() : base (true) {}
}

}
