//
// XslAttributeSet.cs
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
using System.Collections.Specialized;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;
using System.Xml.Xsl;
using Mono.Xml.Xsl.Operations;

using QName = System.Xml.XmlQualifiedName;

namespace Mono.Xml.Xsl {
	internal class XslAttributeSet : XslCompiledElement {
		QName name;
		// [QName]=>XslAttributeSet
		ArrayList usedAttributeSets = new ArrayList ();
		
		// [QName]=>XslAttribute
		ArrayList attributes = new ArrayList ();
		
		public XslAttributeSet (Compiler c) : base (c) {}
		
		public QName Name {
			get { return name; }
		}

		protected override void Compile (Compiler c)
		{
			this.name = c.ParseQNameAttribute ("name");
			
			QName [] attrSets = c.ParseQNameListAttribute ("use-attribute-sets");
			if (attrSets != null)
				foreach (QName q in attrSets)
					usedAttributeSets.Add (q);

			
			if (!c.Input.MoveToFirstChild ()) return;
				
			do {
				if (c.Input.NodeType != XPathNodeType.Element)
					continue;
				if (c.Input.NamespaceURI != XsltNamespace || c.Input.LocalName != "attribute")
					throw new XsltCompileException ("Invalid attr set content", null, c.Input);
				attributes.Add (new XslAttribute (c));
			} while (c.Input.MoveToNext ());
			
			c.Input.MoveToParent ();
			
		}
		
		public void Merge (XslAttributeSet s)
		{
			attributes.AddRange (s.attributes);
			
			foreach (QName q in s.usedAttributeSets)
				if (!usedAttributeSets.Contains (q))
					usedAttributeSets.Add (q);
		}
		
		public override void Evaluate (XslTransformProcessor p) {
			p.SetBusy (this);
			
			if (usedAttributeSets != null) {
				for (int i = 0; i < usedAttributeSets.Count; i++) {
					QName set = (QName) usedAttributeSets [i];
					XslAttributeSet s = p.ResolveAttributeSet (set);
					if (s == null)
						throw new XsltException ("Could not resolve attribute set", null, p.CurrentNode);
					
					if (p.IsBusy (s))
						throw new XsltException ("circular dependency", null, p.CurrentNode);
					
					s.Evaluate (p);
				}
			}
						
			for (int i = 0; i < attributes.Count; i++)
				((XslAttribute) attributes [i]).Evaluate (p);
			
			p.SetFree (this);
		}
	}
}
