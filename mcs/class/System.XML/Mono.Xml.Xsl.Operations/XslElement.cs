//
// XslElement.cs
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

using QName = System.Xml.XmlQualifiedName;

namespace Mono.Xml.Xsl.Operations {	
	internal class XslElement : XslCompiledElement {
		XslAvt name, ns;
		string calcName, calcNs, calcPrefix;
		Hashtable nsDecls;
		bool isEmptyElement;

		XslOperation value;
		XmlQualifiedName [] useAttributeSets;

		public XslElement (Compiler c) : base (c) {}
		protected override void Compile (Compiler c)
		{
			if (c.Debugger != null)
				c.Debugger.DebugCompile (c.Input);

			c.CheckExtraAttributes ("element", "name", "namespace", "use-attribute-sets");

			name = c.ParseAvtAttribute ("name");
			ns = c.ParseAvtAttribute ("namespace");
			nsDecls = c.GetNamespacesToCopy ();
			calcName = XslAvt.AttemptPreCalc (ref name);
			
			if (calcName != null) {
				int colonAt = calcName.IndexOf (':');
				if (colonAt == 0)
					throw new XsltCompileException ("Invalid name attribute", null, c.Input);
				calcPrefix = colonAt < 0 ? String.Empty : calcName.Substring (0, colonAt);
				if (colonAt > 0)
					calcName = calcName.Substring (colonAt + 1);

				try {
					XmlConvert.VerifyNCName (calcName);
					if (calcPrefix != String.Empty)
						XmlConvert.VerifyNCName (calcPrefix);
				} catch (XmlException ex) {
					throw new XsltCompileException ("Invalid name attribute", ex, c.Input);
				}

				if (ns == null) {
					calcNs = c.Input.GetNamespace (calcPrefix);
					if (calcPrefix != String.Empty && calcNs == String.Empty)
						throw new XsltCompileException ("Invalid name attribute", null, c.Input);
				}
			} else if (ns != null)
				calcNs = XslAvt.AttemptPreCalc (ref ns);
			
			useAttributeSets = c.ParseQNameListAttribute ("use-attribute-sets");
			
			isEmptyElement = c.Input.IsEmptyElement;

			if (c.Input.MoveToFirstChild ()) {
				value = c.CompileTemplateContent (XPathNodeType.Element);
				c.Input.MoveToParent ();
			}
		}
		
		public override void Evaluate (XslTransformProcessor p)
		{
			if (p.Debugger != null)
				p.Debugger.DebugExecute (p, this.DebugInput);

			string nm, nmsp, localName, prefix;
			
			localName = nm = calcName != null ? calcName : name.Evaluate (p);
			nmsp = calcNs != null ? calcNs : ns != null ? ns.Evaluate (p) : null;

			QName q = XslNameUtil.FromString (nm, nsDecls);
			localName = q.Name;
			if (nmsp == null)
				nmsp = q.Namespace;
			int colonAt = nm.IndexOf (':');
			if (colonAt > 0)
				calcPrefix = nm.Substring (0, colonAt);
			else if (colonAt == 0)
				// raises an error
				XmlConvert.VerifyNCName (String.Empty);

			prefix = calcPrefix != null ? calcPrefix : String.Empty;

			if (prefix != String.Empty)
				XmlConvert.VerifyNCName (prefix);
			XmlConvert.VerifyNCName (localName);

			bool isCData = p.InsideCDataElement;
			p.PushElementState (prefix, localName, nmsp, false);
			p.Out.WriteStartElement (prefix, localName, nmsp);

			if (useAttributeSets != null)
				foreach (XmlQualifiedName s in useAttributeSets)
					p.ResolveAttributeSet (s).Evaluate (p);

			if (value != null) value.Evaluate (p);

			if (isEmptyElement && useAttributeSets == null)
				p.Out.WriteEndElement ();
			else
				p.Out.WriteFullEndElement ();
			p.PopCDataState (isCData);
		}
	}
}
