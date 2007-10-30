//
// XslAttribute.cs
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//	Atsushi Enomoto (ginga@kit.hi-ho.ne.jp)
//	
// (C) 2003 Ben Maurer
// (C) 2003 Atsushi Enomoto
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
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
using System.IO;

using QName = System.Xml.XmlQualifiedName;

namespace Mono.Xml.Xsl.Operations {
	internal class XslAttribute : XslCompiledElement {
		XslAvt name, ns;
		string calcName, calcNs, calcPrefix;
//		XmlNamespaceManager nsm;
		Hashtable nsDecls;
		XslOperation value;

		public XslAttribute (Compiler c) : base (c) {}
		
		protected override void Compile (Compiler c)
		{
			if (c.Debugger != null)
				c.Debugger.DebugCompile (c.Input);

			XPathNavigator nav = c.Input.Clone ();
			nsDecls = c.GetNamespacesToCopy ();

			c.CheckExtraAttributes ("attribute", "name", "namespace");

			name = c.ParseAvtAttribute ("name");
			if (name == null)
				throw new XsltCompileException ("Attribute \"name\" is required on XSLT attribute element", null, c.Input);
			ns = c.ParseAvtAttribute ("namespace");

			calcName = XslAvt.AttemptPreCalc (ref name);
			calcPrefix = String.Empty;

			if (calcName != null) {
				int colonAt = calcName.IndexOf (':');
				calcPrefix = colonAt < 0 ? String.Empty : calcName.Substring (0, colonAt);
				calcName = colonAt < 0 ? calcName : calcName.Substring (colonAt + 1, calcName.Length - colonAt - 1);

				try {
					XmlConvert.VerifyNCName (calcName);
					if (calcPrefix != String.Empty)
						XmlConvert.VerifyNCName (calcPrefix);
				} catch (XmlException ex) {
					throw new XsltCompileException ("Invalid attribute name", ex, c.Input);
				}
			}

			if (calcPrefix != String.Empty) {
				calcPrefix = c.CurrentStylesheet.GetActualPrefix (calcPrefix);
				if (calcPrefix == null)
					calcPrefix = String.Empty;
			}

			if (calcPrefix != String.Empty && ns == null)
				calcNs = nav.GetNamespace (calcPrefix);
			else if (ns != null)
				calcNs = XslAvt.AttemptPreCalc (ref ns);

			if (c.Input.MoveToFirstChild ()) {
				value = c.CompileTemplateContent (XPathNodeType.Attribute);
				c.Input.MoveToParent ();
			}
		}

		public override void Evaluate (XslTransformProcessor p)
		{
			if (p.Debugger != null)
				p.Debugger.DebugExecute (p, this.DebugInput);

			string nm, nmsp, prefix;
			
			nm = calcName != null ? calcName : name.Evaluate (p);
			nmsp = calcNs != null ? calcNs : ns != null ? ns.Evaluate (p) : String.Empty;
			prefix = calcPrefix != null ? calcPrefix : String.Empty;

			if (nm == "xmlns")
				// It is an error. We must recover by not emmiting any attributes 
				// (unless we throw an exception).
				return;

			int colonAt = nm.IndexOf (':');
			// local attribute
			if (colonAt > 0) {
				prefix = nm.Substring (0, colonAt);
				nm = nm.Substring (colonAt + 1);

				// global attribute
				if (nmsp == String.Empty &&
					prefix == XmlNamespaceManager.PrefixXml)
					nmsp = XmlNamespaceManager.XmlnsXml;
				else if (nmsp == String.Empty) {
					nmsp = (string) nsDecls [prefix];
					if (nmsp == null)
						nmsp = String.Empty;
				}
			}

			if (prefix == "xmlns")
				prefix = String.Empty;	// Should not be allowed.

			XmlConvert.VerifyName (nm);

			p.Out.WriteAttributeString (prefix, nm, nmsp,
				value == null ? "" : value.EvaluateAsString (p));
		}
	}
}
