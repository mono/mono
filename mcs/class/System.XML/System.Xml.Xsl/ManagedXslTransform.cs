//
// ManagedXslTransform
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2003 Ben Maurer
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
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
			if (s == null)
				throw new XsltException ("No stylesheet was loaded.", null);

			Outputter outputter = new GenericOutputter (output, s.Outputs, null);
			new XslTransformProcessor (s).Process (input, outputter, args, resolver);
			output.Flush ();
		}

		public override void Transform (XPathNavigator input, XsltArgumentList args, TextWriter output, XmlResolver resolver) {
			Outputter outputter = new GenericOutputter(output, s.Outputs, output.Encoding);			
			new XslTransformProcessor (s).Process (input, outputter, args, resolver);
			outputter.Done ();
			output.Flush ();
		}
		
		public override void Transform (XPathNavigator input, XsltArgumentList args, Stream output, XmlResolver resolver)
		{
			XslOutput xslOutput = (XslOutput)s.Outputs[String.Empty];
			Transform (input, args, new StreamWriter (output, xslOutput.Encoding), resolver);
		}
	}
}
