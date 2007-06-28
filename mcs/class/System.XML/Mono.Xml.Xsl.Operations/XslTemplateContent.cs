//
// XslTemplateContent.cs
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

namespace Mono.Xml.Xsl.Operations
{
	internal class XslTemplateContent : XslCompiledElementBase
	{
		ArrayList content = new ArrayList ();
		
		bool hasStack;
		int stackSize;
		XPathNodeType parentType;
		bool xslForEach;
		
		public XslTemplateContent (Compiler c,
			XPathNodeType parentType, bool xslForEach)
			: base (c) 
		{
			this.parentType = parentType;
			this.xslForEach = xslForEach;
			Compile (c);
		}

		public XPathNodeType ParentType {
			get { return parentType; }
		}

		protected override void Compile (Compiler c)
		{
			if (c.Debugger != null)
				c.Debugger.DebugCompile (this.DebugInput);

			hasStack = (c.CurrentVariableScope == null);
			c.PushScope ();
			do {	
				Debug.EnterNavigator (c);
				XPathNavigator n = c.Input;			
				switch (n.NodeType) {
				case XPathNodeType.Element:
					switch (n.NamespaceURI) {
					case XsltNamespace:
						
						switch (n.LocalName) {
						case "apply-imports":
							content.Add (new XslApplyImports (c));
							break;
						case "apply-templates":
							content.Add (new XslApplyTemplates (c));
							break;
						case "attribute":
							if (ParentType == XPathNodeType.All
								|| ParentType == XPathNodeType.Element)
								content.Add (new XslAttribute (c));
							break;
						case "call-template":
							content.Add (new XslCallTemplate (c));
							break;
						case "choose":
							content.Add (new XslChoose (c));
							break;
						case "comment":
							if (ParentType == XPathNodeType.All
								|| ParentType == XPathNodeType.Element)
							content.Add (new XslComment (c));
							break;
						case "copy":
							content.Add (new XslCopy (c));
							break;
						case "copy-of":
							content.Add (new XslCopyOf (c));
							break;
						case "element":
							if (ParentType == XPathNodeType.All
								|| ParentType == XPathNodeType.Element)
								content.Add (new XslElement (c));
							break;
						case "fallback":
							break;
						case "for-each":
							content.Add (new XslForEach (c));
							break;
						case "if":
							content.Add (new XslIf (c));
							break;
						case "message":
							content.Add (new XslMessage(c));
							break;
						case "number":
							content.Add (new XslNumber(c));
							break;
						case "processing-instruction":
							if (ParentType == XPathNodeType.All
								|| ParentType == XPathNodeType.Element)
								content.Add (new XslProcessingInstruction(c));
							break;
						case "text":
							content.Add (new XslText(c, false));
							break;
						case "value-of":
							content.Add (new XslValueOf(c));
							break;
						case "variable":
							content.Add (new XslLocalVariable (c));
							break;
						case "sort":
							if (xslForEach)
								break;
							throw new XsltCompileException ("'sort' element is not allowed here as a templete content", null, n);
						default:
							// TODO: handle fallback, like we should
//							throw new XsltCompileException ("Did not recognize element " + n.Name, null, n);
							content.Add (new XslNotSupportedOperation (c));
							break;
						}
						break;
					default:
						if (!c.IsExtensionNamespace (n.NamespaceURI))
							content.Add (new XslLiteralElement(c));
						else {
							if (n.MoveToFirstChild ()) {
								do {
									if (n.NamespaceURI == XsltNamespace && n.LocalName == "fallback")
										content.Add (new XslFallback (c));
								} while (n.MoveToNext ());
								n.MoveToParent ();
							}
						}
						break;
					}
					break;

				case XPathNodeType.SignificantWhitespace:
					content.Add (new XslText(c, true));
					break;
				case XPathNodeType.Text:
					content.Add (new XslText(c, false));
					break;
				default:
					break;
				}

				Debug.ExitNavigator (c);
				
			} while (c.Input.MoveToNext ());
			
			
			if (hasStack) {
				stackSize = c.PopScope ().VariableHighTide;
				hasStack = stackSize > 0;
			} else 
				c.PopScope ();
		}
		
		public override void Evaluate (XslTransformProcessor p)
		{
			if (p.Debugger != null)
				p.Debugger.DebugExecute (p, this.DebugInput);

			if (hasStack)
				p.PushStack (stackSize);
			
			int len = content.Count;
			for (int i = 0; i < len; i++)
				((XslOperation) content [i]).Evaluate (p);
			
			if (hasStack)
				p.PopStack ();
		}
	}
}