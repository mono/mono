//
// XslTransfromImpl
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


namespace System.Xml.Xsl
{
	internal abstract class XslTransformImpl
	{
		public virtual void Load (string url, XmlResolver resolver)
		{
			XmlResolver res = resolver;
			if (res == null)
				res = new XmlUrlResolver ();
			Uri uri = res.ResolveUri (null, url);
			using (Stream s = res.GetEntity (uri, null, typeof (Stream)) as Stream) {
				XmlTextReader xtr = new XmlTextReader (uri.ToString (), s);
				xtr.XmlResolver = res;
				XmlValidatingReader xvr = new XmlValidatingReader (xtr);
				xvr.XmlResolver = res;
				xvr.ValidationType = ValidationType.None;
				Load (new XPathDocument (xvr, XmlSpace.Preserve).CreateNavigator (), resolver, null);
			}
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