//
// KeyInfoClause.cs - Abstract KeyInfoClause implementation for XML Signature
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
//

using System.Xml;

namespace System.Security.Cryptography.Xml {

public abstract class KeyInfoClause {

	public KeyInfoClause () {}

	public abstract XmlElement GetXml ();

	public abstract void LoadXml (XmlElement element);
}

}