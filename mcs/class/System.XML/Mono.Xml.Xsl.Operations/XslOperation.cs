//
// XslOperation.cs
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//	Atsushi Enomoto (ginga@kit.hi-ho.ne.jp)
//	
// (C) 2003 Ben Maurer
// (C) 2003 Atsushi Enomoto
//

using System;
using System.Collections;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

using QName = System.Xml.XmlQualifiedName;

namespace Mono.Xml.Xsl.Operations {
	public abstract class XslOperation {
		public const string XsltNamespace = "http://www.w3.org/1999/XSL/Transform";
		public abstract void Evaluate (XslTransformProcessor p);
	}
}
