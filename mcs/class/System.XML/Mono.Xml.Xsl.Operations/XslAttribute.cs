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

using System;
using System.Collections;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
using System.IO;

using QName = System.Xml.XmlQualifiedName;

namespace Mono.Xml.Xsl.Operations {
	public class XslAttribute : XslCompiledElement {
		XslAvt name, ns;
		string calcName, calcNs, calcPrefix;
		XmlNamespaceManager nsm;
		
		XslOperation value;
		XPathNavigator nav;

		public XslAttribute (Compiler c) : base (c) {}
		
		protected override void Compile (Compiler c)
		{
			nav = c.Input.Clone ();
			
			name = c.ParseAvtAttribute ("name");
			ns = c.ParseAvtAttribute ("namespace");

			calcName = XslAvt.AttemptPreCalc (ref name);
			
			if (calcName != null && ns == null) {
				int colonAt = calcName.IndexOf (':');
				calcPrefix = colonAt < 0 ? String.Empty : calcName.Substring (0, colonAt);
				calcName = colonAt < 0 ? calcName : calcName.Substring (colonAt + 1, calcName.Length - colonAt - 1);
				calcNs = nav.GetNamespace (calcPrefix);
			} else if (ns != null)
				calcNs = XslAvt.AttemptPreCalc (ref ns);
			
			if (ns == null && calcNs == null)
				nsm = c.GetNsm ();
				
			if (c.Input.MoveToFirstChild ()) {
				value = c.CompileTemplateContent ();
				c.Input.MoveToParent ();
			}

		}

		public override void Evaluate (XslTransformProcessor p)
		{
			string nm, nmsp, localName, prefix;
			
			nm = calcName != null ? calcName : name.Evaluate (p);
			nmsp = calcNs != null ? calcNs : ns != null ? ns.Evaluate (p) : null;
			prefix = calcPrefix != null ? calcPrefix : String.Empty;

			if (nm == "xmlns")
				// It is an error. We must recover by not emmiting any attributes 
				// (unless we throw an exception).
				return;

			int colonAt = nm.IndexOf (':');
			// local attribute
			prefix = colonAt < 0 ? String.Empty : nm.Substring (0, colonAt);
			nm = colonAt < 0 ? nm : nm.Substring (colonAt + 1, nm.Length - colonAt - 1);
			if (colonAt > 0) {
				// global attribute
				if (nmsp == null) {
					QName q = XslNameUtil.FromString (nm, nsm);
					nm = q.Name;
					nmsp = q.Namespace;
				} else
					nm = XslNameUtil.LocalNameOf (nm);
			}

			if (nmsp != String.Empty && prefix == String.Empty) {
				if (nav.MoveToFirstNamespace (XPathNamespaceScope.ExcludeXml)) {
					do {
						if (nav.Value == nmsp) {
							prefix = nav.Name;
							break;
						}
					} while (nav.MoveToNextNamespace (XPathNamespaceScope.ExcludeXml));
					nav.MoveToParent ();
				}
			}

			if (prefix == "xmlns")
				prefix = String.Empty;	// Should not be allowed.

			if (value == null)
				p.Out.WriteAttributeString(prefix, nm, nmsp, "");
			else {
				StringWriter sw = new StringWriter ();
				Outputter outputter = new TextOutputter (sw, true);
				p.PushOutput (outputter);
				value.Evaluate (p);			    
				p.PopOutput ();
				outputter.Done ();			        
				p.Out.WriteAttributeString (prefix, nm, nmsp, sw.ToString ());			                    			        
			}						
		}
	}
}
