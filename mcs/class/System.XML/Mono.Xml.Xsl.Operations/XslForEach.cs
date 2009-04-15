//
// XslForEach.cs
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
	internal class XslForEach : XslCompiledElement {
		XPathExpression select;
		XslOperation children;
		XslSortEvaluator sortEvaluator;
		
		public XslForEach (Compiler c) : base (c) {}
		
		protected override void Compile (Compiler c)
		{
			if (c.Debugger != null)
				c.Debugger.DebugCompile (c.Input);

			c.CheckExtraAttributes ("for-each", "select");

			c.AssertAttribute ("select");
			select = c.CompileExpression (c.GetAttribute ("select"));
			ArrayList sorterList = null;
			
			if (c.Input.MoveToFirstChild ()) {
				bool alldone = true;
				do {
					if (c.Input.NodeType == XPathNodeType.Text)
						{ alldone = false; break; }
					
					if (c.Input.NodeType != XPathNodeType.Element)
						continue;
					if (c.Input.NamespaceURI != Compiler.XsltNamespace)
						{ alldone = false; break; }
					if (c.Input.LocalName != "sort")
						{ alldone = false; break; }
					//c.AddSort (select, new Sort (c));
					if (sorterList == null)
						sorterList = new ArrayList ();
					sorterList.Add (new Sort (c));
				} while (c.Input.MoveToNext ());
				if (!alldone)
					children = c.CompileTemplateContent ();
				c.Input.MoveToParent ();
			}
			if (sorterList != null)
				sortEvaluator = new XslSortEvaluator (select,
					(Sort []) sorterList.ToArray (typeof (Sort)));
		}
		
		public override void Evaluate (XslTransformProcessor p)
		{
			if (p.Debugger != null)
				p.Debugger.DebugExecute (p, this.DebugInput);

			// This intelligent optimization causes poor compatibility bug shown in bug #457065
//			if (children == null)
//				return;

			XPathNodeIterator iter = sortEvaluator != null ?
				sortEvaluator.SortedSelect (p) :
				p.Select (select);

			
			while (p.NodesetMoveNext (iter)) {
				p.PushNodeset (iter);
				p.PushForEachContext ();
				children.Evaluate (p);
				p.PopForEachContext();
				p.PopNodeset ();
			}
		}
	}
}
