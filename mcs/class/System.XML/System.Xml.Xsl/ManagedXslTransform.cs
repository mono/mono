//
// ManagedXslTransform
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2003 Ben Maurer
//

using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Xml.XPath;
using Mono.Xml.Xsl;


namespace System.Xml.Xsl {
	internal class ManagedXslTransform : XslTransformImpl {
		XslTransformProcessor p;
		
		public override void Load (XPathNavigator stylesheet, XmlResolver resolver)
		{
			Compiler c = new Compiler ();
			p = new XslTransformProcessor (c.Compile (stylesheet, resolver));
		}

		public override void Transform (XPathNavigator input, XsltArgumentList args, XmlWriter output, XmlResolver resolver)
		{
			p.Process (input, output, args); // todo use resolver
		}
	}
}
