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

using System;
using System.Collections;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace Mono.Xml.Xsl.Operations {
	public class XslTemplateContent : XslCompiledElement {
		ArrayList content = new ArrayList ();
		ArrayList variables;
		
		bool hasStack;
		int stackSize;
		
		public XslTemplateContent (Compiler c) : base (c) {}
		protected override void Compile (Compiler c)
		{
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
							content.Add (new XslAttribute (c));
							break;
						case "call-template":
							content.Add (new XslCallTemplate (c));
							break;
						case "choose":
							content.Add (new XslChoose (c));
							break;
						case "comment":
							content.Add (new XslComment (c));
							break;
						case "copy":
							content.Add (new XslCopy (c));
							break;
						case "copy-of":
							content.Add (new XslCopyOf (c));
							break;
						case "element":
							content.Add (new XslElement (c));
							break;
						case "fallback":
							content.Add (new XslFallback (c));
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
							content.Add (new XslProcessingInstruction(c));
							break;
						case "text":
							content.Add (new XslText(c));
							break;
						case "value-of":
							content.Add (new XslValueOf(c));
							break;
						case "variable":
							content.Add (new XslLocalVariable (c));
							break;
						default:
							// TODO: handle fallback, like we should
							throw new Exception ("Did not recognize element " + n.Name);
						}
						break;
					default:
						content.Add (new XslLiteralElement(c));
						break;
					}
					break;
					
				case XPathNodeType.Text:
				case XPathNodeType.SignificantWhitespace:
					content.Add (new XslText(c));
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
			if (hasStack)
				p.PushStack (stackSize);
			
			foreach (XslOperation op in content)
				op.Evaluate (p);
			
			if (hasStack)
				p.PopStack ();
		}
	}
}