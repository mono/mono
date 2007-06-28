//
// XslLiteralElement.cs
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

namespace Mono.Xml.Xsl.Operations {	
	internal class XslLiteralElement : XslCompiledElement {
		XslOperation children;
		string localname, prefix, nsUri;
		bool isEmptyElement;
		ArrayList attrs;
		XmlQualifiedName [] useAttributeSets;
		Hashtable nsDecls;

		public XslLiteralElement (Compiler c) : base (c) {}
			
		class XslLiteralAttribute {
			string localname, prefix, nsUri;
			XslAvt val;
			
			public XslLiteralAttribute (Compiler c)
			{
				this.prefix = c.Input.Prefix;
				if (prefix.Length > 0) {
					string alias = 
						c.CurrentStylesheet.GetActualPrefix (prefix);
					if (alias != prefix) {
						prefix = alias;
						XPathNavigator clone = c.Input.Clone ();
						clone.MoveToParent ();
						nsUri = clone.GetNamespace (alias);
					}
					else
						nsUri = c.Input.NamespaceURI;
				}
				else
					nsUri = String.Empty;
				this.localname = c.Input.LocalName;
				this.val = new XslAvt (c.Input.Value, c);
			}
			
			public void Evaluate (XslTransformProcessor p)
			{
				//FIXME: fix attribute prefixes according to aliases
				p.Out.WriteAttributeString (prefix, localname, nsUri, val.Evaluate (p));
			}
		}
		
		protected override void Compile (Compiler c)
		{
			if (c.Debugger != null)
				c.Debugger.DebugCompile (this.DebugInput);

			prefix = c.Input.Prefix;
			string alias = c.CurrentStylesheet.GetActualPrefix (prefix);
			if (alias != prefix) {
				prefix = alias;
				nsUri = c.Input.GetNamespace (alias);
			}
			else
				nsUri = c.Input.NamespaceURI;

			this.localname = c.Input.LocalName;
			this.useAttributeSets = c.ParseQNameListAttribute ("use-attribute-sets", XsltNamespace);
			this.nsDecls = c.GetNamespacesToCopy ();
			if (nsDecls.Count == 0) nsDecls = null;
			this.isEmptyElement = c.Input.IsEmptyElement;

			if (c.Input.MoveToFirstAttribute ())
			{
				attrs = new ArrayList ();
				do {
					if (c.Input.NamespaceURI == XsltNamespace)
						continue; //already handled
					attrs.Add (new XslLiteralAttribute (c));
				} while (c.Input.MoveToNextAttribute());
				c.Input.MoveToParent ();
			}
			
			if (!c.Input.MoveToFirstChild ()) return;
			children = c.CompileTemplateContent ();
			c.Input.MoveToParent ();
		}

		public override void Evaluate (XslTransformProcessor p)
		{
			if (p.Debugger != null)
				p.Debugger.DebugExecute (p, this.DebugInput);

			// Since namespace-alias might be determined after compilation
			// of import-ing stylesheets, this must be determined later.
			bool isCData = p.InsideCDataElement;
			p.PushElementState (prefix, localname, nsUri, true);
			p.Out.WriteStartElement (prefix, localname, nsUri);

			if (useAttributeSets != null)
				foreach (XmlQualifiedName s in useAttributeSets)
					p.ResolveAttributeSet (s).Evaluate (p);
						
			if (attrs != null) {
				int len = attrs.Count;
				for (int i = 0; i < len; i++)
					((XslLiteralAttribute)attrs [i]).Evaluate (p);
			}

			p.OutputLiteralNamespaceUriNodes (nsDecls, null, null);

			if (children != null) children.Evaluate (p);

			if (isEmptyElement)
				p.Out.WriteEndElement ();
			else
				p.Out.WriteFullEndElement ();

			p.PopCDataState (isCData);
		}
	}
}
