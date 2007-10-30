//
// XslApplyTemplates.cs
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

	internal class XslApplyTemplates : XslCompiledElement {
		XPathExpression select;
		XmlQualifiedName mode;
		ArrayList withParams;
		XslSortEvaluator sortEvaluator;

		public XslApplyTemplates (Compiler c) : base (c) {}
		
		protected override void Compile (Compiler c)
		{
			if (c.Debugger != null)
				c.Debugger.DebugCompile (c.Input);

			c.CheckExtraAttributes ("apply-templates", "select", "mode");

			select = c.CompileExpression (c.GetAttribute ("select"));
			mode = c.ParseQNameAttribute ("mode");
			ArrayList sorterList = null;
			if (c.Input.MoveToFirstChild ()) {
				do {
					switch (c.Input.NodeType) {
					case XPathNodeType.Comment:
					case XPathNodeType.ProcessingInstruction:
					case XPathNodeType.Whitespace:
					case XPathNodeType.SignificantWhitespace:
						continue;
					case XPathNodeType.Element:
						if (c.Input.NamespaceURI != XsltNamespace)
							throw new XsltCompileException ("Unexpected element", null, c.Input); // TODO: fwd compat
						
						switch (c.Input.LocalName)
						{
							case "with-param":
								if (withParams == null)
									withParams = new ArrayList ();
								withParams.Add (new XslVariableInformation (c));
								break;
								
							case "sort":
								if (sorterList == null)
									sorterList = new ArrayList ();
								if (select == null)
									select = c.CompileExpression ("*");
								sorterList.Add (new Sort (c));
								//c.AddSort (select, new Sort (c));
								break;
							default:
								throw new XsltCompileException ("Unexpected element", null, c.Input); // todo forwards compat
						}
						break;
					default:
						throw new XsltCompileException ("Unexpected node type " + c.Input.NodeType, null, c.Input);	// todo forwards compat
					}
				} while (c.Input.MoveToNext ());
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

			if (select == null)	
				p.ApplyTemplates (p.CurrentNode.SelectChildren (XPathNodeType.All), mode, withParams);
			else {
				XPathNodeIterator iter = sortEvaluator != null ?
					sortEvaluator.SortedSelect (p) :
					p.Select (select);
				p.ApplyTemplates (iter, mode, withParams);
			}
		}
	}
}
