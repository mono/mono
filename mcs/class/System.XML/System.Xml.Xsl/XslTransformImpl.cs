//
// XslTransfromImpl
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


namespace System.Xml.Xsl {
	internal abstract class XslTransformImpl {

		public virtual void Load (string url, XmlResolver resolver)
		{
			Load (new XPathDocument (url, XmlSpace.Preserve).CreateNavigator (), resolver, null);
		}

		public virtual void Load (XmlReader stylesheet, XmlResolver resolver, Evidence evidence)
		{
			Load (new XPathDocument (stylesheet, XmlSpace.Preserve).CreateNavigator (), resolver, evidence);
		}

		public abstract void Load (XPathNavigator stylesheet, XmlResolver resolver, Evidence evidence);	

		public abstract void Transform (XPathNavigator input, XsltArgumentList args, XmlWriter output, XmlResolver resolver);
		public abstract void Transform (XPathNavigator input, XsltArgumentList args, TextWriter output, XmlResolver resolver);
		public virtual void Transform (XPathNavigator input, XsltArgumentList args, Stream output, XmlResolver resolver)
		{
			Transform (input, args, new StreamWriter (output), resolver);
		}

		public virtual void Transform (string inputfile, string outputfile, XmlResolver resolver)
		{
			using (Stream s =  new FileStream (outputfile, FileMode.Create, FileAccess.ReadWrite)) {
				Transform(new XPathDocument (inputfile).CreateNavigator (), null, s, resolver);
			}
		}
	}
}