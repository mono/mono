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
		CompiledStylesheet s;
		
		
		public override void Load (XPathNavigator stylesheet, XmlResolver resolver)
		{
			s = new Compiler ().Compile (stylesheet, resolver);
		}

		public override void Transform (XPathNavigator input, XsltArgumentList args, XmlWriter output, XmlResolver resolver)
		{
			new XslTransformProcessor (s).Process (input, output, args, resolver);
		}
	}
}
