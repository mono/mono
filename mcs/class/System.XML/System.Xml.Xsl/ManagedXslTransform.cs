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
using System.Security.Policy;
using System.Text;
using System.Xml.XPath;
using Mono.Xml.Xsl;


namespace System.Xml.Xsl {
	internal class ManagedXslTransform : XslTransformImpl {
		CompiledStylesheet s;
		
		
		public override void Load (XPathNavigator stylesheet, XmlResolver resolver, Evidence evidence)
		{
			s = new Compiler ().Compile (stylesheet, resolver, evidence);
		}

		public override void Transform (XPathNavigator input, XsltArgumentList args, XmlWriter output, XmlResolver resolver)
		{
			Outputter outputter = new XmlOutputter(output, s.Style.Outputs);
			bool wroteStartDocument = false;
			if (output.WriteState == WriteState.Start) {
				outputter.WriteStartDocument ();
				wroteStartDocument = true;
			}
			new XslTransformProcessor (s).Process (input, outputter, args, resolver);
			if (wroteStartDocument)
				outputter.WriteEndDocument ();
		}

		public override void Transform (XPathNavigator input, XsltArgumentList args, TextWriter output, XmlResolver resolver) {
			XslOutput xslOutput = (XslOutput)s.Style.Outputs[String.Empty];
			if (xslOutput == null) {
				//No xsl:output - subject to output method autodetection, XML for a while
				Transform(input, args, new XmlTextWriter(output), resolver);
				return;
			}				
			switch (xslOutput.Method) {
				case OutputMethod.XML:
					Transform(input, args, new XmlTextWriter(output), resolver);
					break;
				case OutputMethod.HTML:
					throw new NotImplementedException("HTML output method is not implemented yet.");
				case OutputMethod.Text:
					new XslTransformProcessor (s).Process (input, new TextOutputter(output), args, resolver);
					break;
				case OutputMethod.Custom:
					throw new NotImplementedException("Custom output method is not implemented yet.");
			}
		}
	}
}
